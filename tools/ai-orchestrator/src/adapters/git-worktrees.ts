// Purpose: Creates, validates and removes only coordinator-marked Git worktrees through isolated Git configuration and raw structural output.
import { lstat, mkdir, open, realpath, unlink } from "node:fs/promises";
import { dirname, isAbsolute, join, relative, resolve, sep } from "node:path";
import { canonicalJson } from "../core/canonical-json.js";
import { OrchestratorStop } from "../core/errors.js";
import { assertIdentifier } from "../core/validation.js";
import type { StructuredProcessExecutor } from "../ports/process-executor.js";
import type { WorktreeManager, WorktreeRecord } from "../ports/worktrees.js";
import { gitArguments, gitEnvironment, assertAbsoluteExecutable } from "../security/git-process-policy.js";
import { assertNoExistingReparseBoundary, readBoundedRegularFile } from "../security/path-policy.js";
import { parseSecureJson } from "../security/secure-json.js";
import { assertSafeGitRepositoryConfiguration } from "../security/git-repository-policy.js";

interface OwnerMarker {
  readonly taskId: string;
  readonly path: string;
  readonly branch: string;
  readonly baseline: string;
}

function parsePorcelain(output: string): readonly WorktreeRecord[] {
  const records: WorktreeRecord[] = [];
  let current: { path?: string; head?: string; branch?: string | null; prunable?: boolean } = {};
  const flush = (): void => {
    if (current.path !== undefined && current.head !== undefined) {
      records.push({ path: current.path, head: current.head, branch: current.branch ?? null, prunable: current.prunable ?? false });
    }
    current = {};
  };
  for (const token of output.split("\u0000")) {
    if (token.startsWith("worktree ")) {
      flush();
      current.path = token.slice(9);
    } else if (token.startsWith("HEAD ")) {
      current.head = token.slice(5);
    } else if (token.startsWith("branch refs/heads/")) {
      current.branch = token.slice(18);
    } else if (token === "prunable" || token.startsWith("prunable ")) {
      current.prunable = true;
    }
  }
  flush();
  return records;
}

function samePath(left: string, right: string): boolean {
  const normalise = (value: string): string => process.platform === "win32" ? resolve(value).toLowerCase() : resolve(value);
  return normalise(left) === normalise(right);
}

function pathsOverlap(left: string, right: string): boolean {
  const normalise = (value: string): string => process.platform === "win32" ? resolve(value).toLowerCase() : resolve(value);
  const leftPath = normalise(left);
  const rightPath = normalise(right);
  return leftPath === rightPath || leftPath.startsWith(`${rightPath}${sep}`) || rightPath.startsWith(`${leftPath}${sep}`);
}

export class GitWorktreeManager implements WorktreeManager {
  private readonly environment: Readonly<Record<string, string>>;

  public constructor(
    private readonly repositoryRoot: string,
    private readonly managedRoot: string,
    private readonly process: StructuredProcessExecutor,
    private readonly gitExecutable: string,
    environment: Readonly<Record<string, string | undefined>>,
  ) {
    assertAbsoluteExecutable(gitExecutable, "Git executable");
    this.environment = gitEnvironment(environment);
  }

  public async list(): Promise<readonly WorktreeRecord[]> {
    const result = await this.git("worktree-list", ["worktree", "list", "--porcelain", "-z"]);
    if (result.evidence.result !== "PASS") throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Git worktree discovery failed.");
    return parsePorcelain(result.stdout);
  }

  public async create(taskId: string, path: string, branch: string, baseline: string): Promise<WorktreeRecord> {
    assertIdentifier(taskId, "taskId");
    await this.assertManagedPath(path);
    if (!branch.startsWith("codex/") || !/^[0-9a-f]{40}$/.test(baseline)) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "Worktree branch or baseline is outside the managed namespace.", taskId);
    }
    if ((await this.list()).some((record) => pathsOverlap(record.path, path) || record.branch === branch)) {
      throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", "The requested worktree path overlaps an existing worktree or its branch already exists.", taskId);
    }
    await this.prepareOwnersDirectory();
    await assertNoExistingReparseBoundary(dirname(this.managedRoot), path);
    const marker: OwnerMarker = { taskId, path: resolve(path), branch, baseline };
    await this.writeMarker(taskId, marker);
    let created = false;
    try {
      const result = await this.git("worktree-create", ["worktree", "add", "-b", branch, path, baseline]);
      if (result.evidence.result !== "PASS") throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Git worktree creation failed.", taskId);
      created = true;
      return await this.validate(taskId, path, branch, baseline);
    } catch (error) {
      if (created) await this.rollbackCreated(taskId, path, branch, baseline);
      else await this.rollbackFailedCreate(taskId, path, branch, baseline);
      throw error;
    }
  }

  public async validate(taskId: string, path: string, branch: string, baseline: string): Promise<WorktreeRecord> {
    assertIdentifier(taskId, "taskId");
    await this.assertManagedPath(path);
    const marker = await this.readMarker(taskId);
    if (marker.taskId !== taskId || !samePath(marker.path, path) || marker.branch !== branch || marker.baseline !== baseline) {
      throw new OrchestratorStop("DESTRUCTIVE_OPERATION", "The worktree ownership marker does not match the task envelope.", taskId);
    }
    const record = (await this.list()).find((candidate) => samePath(candidate.path, path));
    if (record === undefined || record.branch !== branch || record.head !== baseline || record.prunable) {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "The worktree mapping differs from its task envelope.", taskId);
    }
    const resolvedRealPath = await realpath(path);
    if (!samePath(resolvedRealPath, path)) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The worktree path resolves through an unexpected filesystem boundary.", taskId);
    }
    const status = await this.git("worktree-status", ["-C", path, "status", "--porcelain=v1", "-z", "--untracked-files=all"], path);
    if (status.evidence.result !== "PASS" || status.stdout.length !== 0) {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "The managed worktree is not clean.", taskId);
    }
    return record;
  }

  public async removeManaged(taskId: string, path: string, expected?: { readonly branch: string; readonly baseline: string; readonly head: string }): Promise<void> {
    assertIdentifier(taskId, "taskId");
    await this.assertManagedPath(path);
    const marker = await this.readMarkerOrNull(taskId);
    const records = await this.list();
    const record = records.find((candidate) => samePath(candidate.path, path));
    if (marker === null) {
      if (expected === undefined || record !== undefined) {
        throw new OrchestratorStop("DESTRUCTIVE_OPERATION", "The worktree ownership marker is absent.", taskId);
      }
      const branch = await this.git("worktree-absent-marker-branch", ["rev-parse", "--verify", "--quiet", `refs/heads/${expected.branch}`]);
      if (branch.evidence.result === "FAIL" && branch.evidence.exitCode === 1) {
        await this.assertPathAbsent(path, taskId);
        return;
      }
      throw new OrchestratorStop("DESTRUCTIVE_OPERATION", "A managed branch remains without its ownership marker.", taskId);
    }
    if (marker.taskId !== taskId || !samePath(marker.path, path)) {
      throw new OrchestratorStop("DESTRUCTIVE_OPERATION", "The worktree ownership marker does not match the task.", taskId);
    }
    if (expected !== undefined && (marker.branch !== expected.branch || marker.baseline !== expected.baseline || !/^[0-9a-f]{40}$/.test(expected.head))) {
      throw new OrchestratorStop("DESTRUCTIVE_OPERATION", "The worktree ownership marker differs from the persisted task envelope.", taskId);
    }
    if (record === undefined && records.some((candidate) => candidate.branch === marker.branch)) {
      throw new OrchestratorStop("DESTRUCTIVE_OPERATION", "The managed branch is attached to a different worktree path.", taskId);
    }
    if (record !== undefined) {
      if (record.branch !== marker.branch || record.prunable || (expected !== undefined && record.head !== expected.head)) {
        throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "The managed worktree mapping is foreign or prunable.", taskId);
      }
      const status = await this.git("worktree-remove-status", ["-C", path, "status", "--porcelain=v1", "-z", "--untracked-files=all"], path);
      const ancestor = await this.git("worktree-remove-ancestry", ["merge-base", "--is-ancestor", marker.baseline, record.head]);
      if (status.evidence.result !== "PASS" || status.stdout.length !== 0 || ancestor.evidence.result !== "PASS") {
        throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "The managed worktree is dirty or no longer descends from its baseline.", taskId);
      }
      const result = await this.git("worktree-remove", ["worktree", "remove", path], path, async () => await this.assertMarkerUnchanged(taskId, marker));
      if (result.evidence.result !== "PASS") throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Git worktree removal failed.", taskId);
    }
    await this.assertPathAbsent(path, taskId);
    const reference = `refs/heads/${marker.branch}`;
    const branch = await this.git("worktree-remove-branch-head", ["rev-parse", "--verify", "--quiet", reference]);
    if (branch.evidence.result === "FAIL" && branch.evidence.exitCode === 1) {
      await this.assertPathAbsent(path, taskId);
      await this.removeMarker(taskId, marker);
      return;
    }
    const branchHead = branch.stdout.trim();
    if (branch.evidence.result !== "PASS" || !/^[0-9a-f]{40}$/.test(branchHead)) {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "The managed branch identity could not be established.", taskId);
    }
    if (expected === undefined || branchHead !== expected.head) {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "The managed branch no longer matches the persisted candidate identity.", taskId);
    }
    const ancestor = await this.git("worktree-remove-branch-ancestry", ["merge-base", "--is-ancestor", marker.baseline, branchHead]);
    if (ancestor.evidence.result !== "PASS") {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "The managed branch no longer descends from its baseline.", taskId);
    }
    const removed = await this.git("worktree-remove-branch", ["update-ref", "-d", reference, branchHead], this.repositoryRoot, async () => {
      await this.assertMarkerUnchanged(taskId, marker);
      await this.assertPathAbsent(path, taskId);
    });
    if (removed.evidence.result !== "PASS") {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "The managed branch changed before compare-and-swap removal.", taskId);
    }
    await this.assertPathAbsent(path, taskId);
    await this.removeMarker(taskId, marker);
  }

  private async rollbackCreated(taskId: string, path: string, branch: string, baseline: string): Promise<void> {
    const marker = await this.readMarker(taskId);
    if (marker.taskId !== taskId || !samePath(marker.path, path) || marker.branch !== branch || marker.baseline !== baseline) {
      throw new OrchestratorStop("DESTRUCTIVE_OPERATION", "Created worktree rollback lost its ownership proof.", taskId);
    }
    const record = (await this.list()).find((candidate) => samePath(candidate.path, path));
    if (record === undefined || record.branch !== branch || record.head !== baseline || record.prunable) {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "Created worktree rollback could not prove the exact owned mapping.", taskId);
    }
    const status = await this.git("worktree-rollback-status", ["-C", path, "status", "--porcelain=v1", "-z", "--untracked-files=all"], path);
    if (status.evidence.result !== "PASS" || status.stdout.length !== 0) {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "Created worktree rollback found unexpected changes.", taskId);
    }
    const removal = await this.git("worktree-rollback-remove", ["worktree", "remove", path], path, async () => await this.assertMarkerUnchanged(taskId, marker));
    if (removal.evidence.result !== "PASS") throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "Created worktree rollback could not remove the exact owned worktree.", taskId);
    await this.assertPathAbsent(path, taskId);
    const branchRemoval = await this.git("worktree-rollback-branch", ["update-ref", "-d", `refs/heads/${branch}`, baseline], this.repositoryRoot, async () => {
      await this.assertMarkerUnchanged(taskId, marker);
      await this.assertPathAbsent(path, taskId);
    });
    if (branchRemoval.evidence.result !== "PASS") {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "Rollback branch identity changed before compare-and-swap removal.", taskId);
    }
    await this.assertPathAbsent(path, taskId);
    const remainingRecord = (await this.list()).some((candidate) => samePath(candidate.path, path) || candidate.branch === branch);
    const remainingBranch = await this.git("worktree-rollback-verify-branch", ["rev-parse", "--verify", "--quiet", `refs/heads/${branch}`]);
    if (remainingRecord || !(remainingBranch.evidence.result === "FAIL" && remainingBranch.evidence.exitCode === 1)) {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "Created worktree rollback could not prove resource absence.", taskId);
    }
    await this.assertPathAbsent(path, taskId);
    await this.removeMarker(taskId, marker);
  }

  private async rollbackFailedCreate(taskId: string, path: string, branch: string, baseline: string): Promise<void> {
    const marker = await this.readMarker(taskId);
    if (marker.taskId !== taskId || !samePath(marker.path, path) || marker.branch !== branch || marker.baseline !== baseline) {
      throw new OrchestratorStop("DESTRUCTIVE_OPERATION", "Failed worktree creation lost its ownership proof.", taskId);
    }
    const records = await this.list();
    const record = records.find((candidate) => samePath(candidate.path, path) || candidate.branch === branch);
    if (record !== undefined) {
      if (samePath(record.path, path) && record.branch === branch && record.head === baseline && !record.prunable) {
        await this.rollbackCreated(taskId, path, branch, baseline);
        return;
      }
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "Failed worktree creation left an unproved resource; its ownership marker was preserved.", taskId);
    }
    const branchState = await this.git("worktree-failed-create-branch", ["rev-parse", "--verify", "--quiet", `refs/heads/${branch}`]);
    if (!(branchState.evidence.result === "FAIL" && branchState.evidence.exitCode === 1)) {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "Failed worktree creation left a branch; its ownership marker was preserved.", taskId);
    }
    await this.assertPathAbsent(path, taskId);
    await this.removeMarker(taskId, marker);
  }

  private async assertManagedPath(path: string): Promise<void> {
    const relation = relative(resolve(this.managedRoot), resolve(path));
    if (relation === "" || relation === ".." || relation.startsWith(`..${sep}`) || isAbsolute(relation)) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The worktree path is outside the managed root.");
    }
    await assertNoExistingReparseBoundary(dirname(this.managedRoot), path);
  }

  private async assertPathAbsent(path: string, taskId: string): Promise<void> {
    try {
      await lstat(path);
    } catch (error) {
      if ((error as NodeJS.ErrnoException).code === "ENOENT") return;
      throw error;
    }
    throw new OrchestratorStop("DESTRUCTIVE_OPERATION", "A filesystem entry remains at the unowned worktree path.", taskId);
  }

  private ownerMarker(taskId: string): string {
    return join(this.managedRoot, ".owners", `${taskId}.json`);
  }

  private ownersDirectory(): string {
    return join(this.managedRoot, ".owners");
  }

  private async prepareOwnersDirectory(): Promise<void> {
    const authorityRoot = dirname(this.managedRoot);
    await assertNoExistingReparseBoundary(authorityRoot, this.ownersDirectory());
    await mkdir(this.ownersDirectory(), { recursive: true });
    await assertNoExistingReparseBoundary(authorityRoot, this.ownersDirectory());
  }

  private async writeMarker(taskId: string, marker: OwnerMarker): Promise<void> {
    const finalPath = this.ownerMarker(taskId);
    const authorityRoot = dirname(this.managedRoot);
    await assertNoExistingReparseBoundary(authorityRoot, this.ownersDirectory());
    await assertNoExistingReparseBoundary(authorityRoot, finalPath);
    let handle;
    try {
      handle = await open(finalPath, "wx", 0o600);
      await handle.writeFile(canonicalJson(marker), "utf8");
      await handle.sync();
    } catch (error) {
      if ((error as NodeJS.ErrnoException).code === "EEXIST") {
        throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "A worktree ownership marker already exists.", taskId);
      }
      throw error;
    } finally {
      await handle?.close();
    }
    await assertNoExistingReparseBoundary(authorityRoot, this.ownersDirectory());
    const text = await readBoundedRegularFile(authorityRoot, finalPath, 8_192, "Worktree ownership marker", "OUT_OF_SCOPE_CHANGE_REQUIRED", taskId);
    if (text !== canonicalJson(marker)) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The worktree ownership marker changed during creation.", taskId);
    }
  }

  private async readMarker(taskId: string): Promise<OwnerMarker> {
    const marker = await this.readMarkerOrNull(taskId);
    if (marker === null) throw new OrchestratorStop("DESTRUCTIVE_OPERATION", "The ownership marker is absent.", taskId);
    return marker;
  }

  private async readMarkerOrNull(taskId: string): Promise<OwnerMarker | null> {
    const path = this.ownerMarker(taskId);
    const authorityRoot = dirname(this.managedRoot);
    await assertNoExistingReparseBoundary(authorityRoot, this.ownersDirectory());
    let text: string;
    try {
      text = await readBoundedRegularFile(authorityRoot, path, 8_192, "Worktree ownership marker", "DESTRUCTIVE_OPERATION", taskId);
    } catch (error) {
      if ((error as NodeJS.ErrnoException).code === "ENOENT") return null;
      throw error;
    }
    let value: unknown;
    try { value = parseSecureJson(text, "Worktree ownership marker", "DESTRUCTIVE_OPERATION"); }
    catch { throw new OrchestratorStop("DESTRUCTIVE_OPERATION", "The ownership marker is invalid.", taskId); }
    if (value === null || typeof value !== "object" || Array.isArray(value)) throw new OrchestratorStop("DESTRUCTIVE_OPERATION", "The ownership marker is invalid.", taskId);
    const source = value as Record<string, unknown>;
    if (Object.keys(source).sort().join(",") !== "baseline,branch,path,taskId" ||
        typeof source.taskId !== "string" || typeof source.path !== "string" || typeof source.branch !== "string" || typeof source.baseline !== "string") {
      throw new OrchestratorStop("DESTRUCTIVE_OPERATION", "The ownership marker failed its closed contract.", taskId);
    }
    return source as unknown as OwnerMarker;
  }

  private async assertMarkerUnchanged(taskId: string, expected: OwnerMarker): Promise<void> {
    const current = await this.readMarker(taskId);
    if (canonicalJson(current) !== canonicalJson(expected)) {
      throw new OrchestratorStop("DESTRUCTIVE_OPERATION", "The worktree ownership marker changed before a Git mutation.", taskId);
    }
  }

  private async removeMarker(taskId: string, expected: OwnerMarker): Promise<void> {
    await this.assertMarkerUnchanged(taskId, expected);
    const authorityRoot = dirname(this.managedRoot);
    await assertNoExistingReparseBoundary(authorityRoot, this.ownersDirectory());
    await assertNoExistingReparseBoundary(authorityRoot, this.ownerMarker(taskId));
    await unlink(this.ownerMarker(taskId));
    await assertNoExistingReparseBoundary(authorityRoot, this.ownersDirectory());
  }

  private async git(commandId: string, arguments_: readonly string[], policyCwd = this.repositoryRoot, beforeExecute?: () => Promise<void>) {
    await assertSafeGitRepositoryConfiguration(this.process, this.gitExecutable, policyCwd, this.environment);
    await beforeExecute?.();
    return await this.process.runStructured({
      commandId,
      executable: this.gitExecutable,
      arguments: gitArguments(arguments_),
      cwd: this.repositoryRoot,
      environment: this.environment,
      timeoutMs: 120_000,
      maximumOutputBytes: 1_048_576,
      maximumRelevantLines: 256,
    });
  }
}
