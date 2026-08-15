// Purpose: Creates, validates and removes only coordinator-marked Git worktrees through isolated Git configuration and raw structural output.
import { mkdir, open, readFile, realpath, stat, unlink } from "node:fs/promises";
import { isAbsolute, join, relative, resolve, sep } from "node:path";
import { canonicalJson } from "../core/canonical-json.js";
import { OrchestratorStop } from "../core/errors.js";
import { assertIdentifier } from "../core/validation.js";
import type { StructuredProcessExecutor } from "../ports/process-executor.js";
import type { WorktreeManager, WorktreeRecord } from "../ports/worktrees.js";
import { gitArguments, gitEnvironment, assertAbsoluteExecutable } from "../security/git-process-policy.js";
import { assertNoExistingReparseBoundary } from "../security/path-policy.js";
import { parseSecureJson } from "../security/secure-json.js";

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
    } else if (token === "prunable") {
      current.prunable = true;
    }
  }
  flush();
  return records;
}

function samePath(left: string, right: string): boolean {
  return resolve(left).toLocaleLowerCase("en-US") === resolve(right).toLocaleLowerCase("en-US");
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
    if ((await this.list()).some((record) => samePath(record.path, path) || record.branch === branch)) {
      throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", "The requested worktree path or branch already exists.", taskId);
    }
    await mkdir(join(this.managedRoot, ".owners"), { recursive: true });
    await assertNoExistingReparseBoundary(this.managedRoot, path);
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
      else await unlink(this.ownerMarker(taskId)).catch(() => undefined);
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
    const status = await this.git("worktree-status", ["-C", path, "status", "--porcelain=v1", "-z", "--untracked-files=all"]);
    if (status.evidence.result !== "PASS" || status.stdout.length !== 0) {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "The managed worktree is not clean.", taskId);
    }
    return record;
  }

  public async removeManaged(taskId: string, path: string): Promise<void> {
    assertIdentifier(taskId, "taskId");
    await this.assertManagedPath(path);
    const marker = await this.readMarker(taskId);
    if (marker.taskId !== taskId || !samePath(marker.path, path)) {
      throw new OrchestratorStop("DESTRUCTIVE_OPERATION", "The worktree ownership marker does not match the task.", taskId);
    }
    const record = (await this.list()).find((candidate) => samePath(candidate.path, path));
    if (record === undefined || record.branch !== marker.branch || record.prunable) {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "The managed worktree mapping is absent or foreign.", taskId);
    }
    const status = await this.git("worktree-remove-status", ["-C", path, "status", "--porcelain=v1", "-z", "--untracked-files=all"]);
    const ancestor = await this.git("worktree-remove-ancestry", ["merge-base", "--is-ancestor", marker.baseline, record.head]);
    if (status.evidence.result !== "PASS" || status.stdout.length !== 0 || ancestor.evidence.result !== "PASS") {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "The managed worktree is dirty or no longer descends from its baseline.", taskId);
    }
    const result = await this.git("worktree-remove", ["worktree", "remove", path]);
    if (result.evidence.result !== "PASS") throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Git worktree removal failed.", taskId);
    await unlink(this.ownerMarker(taskId));
  }

  private async rollbackCreated(taskId: string, path: string, branch: string, baseline: string): Promise<void> {
    const record = (await this.list()).find((candidate) => samePath(candidate.path, path));
    if (record?.branch === branch && record.head === baseline && !record.prunable) {
      const status = await this.git("worktree-rollback-status", ["-C", path, "status", "--porcelain=v1", "-z", "--untracked-files=all"]);
      if (status.evidence.result === "PASS" && status.stdout.length === 0) {
        await this.git("worktree-rollback-remove", ["worktree", "remove", path]);
        await this.git("worktree-rollback-branch", ["branch", "--delete", "--force", branch]);
      }
    }
    await unlink(this.ownerMarker(taskId)).catch(() => undefined);
  }

  private async assertManagedPath(path: string): Promise<void> {
    const relation = relative(resolve(this.managedRoot), resolve(path));
    if (relation === "" || relation === ".." || relation.startsWith(`..${sep}`) || isAbsolute(relation)) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The worktree path is outside the managed root.");
    }
    await assertNoExistingReparseBoundary(this.managedRoot, path);
  }

  private ownerMarker(taskId: string): string {
    return join(this.managedRoot, ".owners", `${taskId}.json`);
  }

  private async writeMarker(taskId: string, marker: OwnerMarker): Promise<void> {
    const finalPath = this.ownerMarker(taskId);
    const handle = await open(finalPath, "wx", 0o600);
    try {
      await handle.writeFile(canonicalJson(marker), "utf8");
      await handle.sync();
    } finally {
      await handle.close();
    }
  }

  private async readMarker(taskId: string): Promise<OwnerMarker> {
    const path = this.ownerMarker(taskId);
    const metadata = await stat(path).catch(() => null);
    if (metadata === null || metadata.size > 8_192) throw new OrchestratorStop("DESTRUCTIVE_OPERATION", "The ownership marker is absent or oversized.", taskId);
    let value: unknown;
    try { value = parseSecureJson(await readFile(path, "utf8"), "Worktree ownership marker", "DESTRUCTIVE_OPERATION"); }
    catch { throw new OrchestratorStop("DESTRUCTIVE_OPERATION", "The ownership marker is invalid.", taskId); }
    if (value === null || typeof value !== "object" || Array.isArray(value)) throw new OrchestratorStop("DESTRUCTIVE_OPERATION", "The ownership marker is invalid.", taskId);
    const source = value as Record<string, unknown>;
    if (Object.keys(source).sort().join(",") !== "baseline,branch,path,taskId" ||
        typeof source.taskId !== "string" || typeof source.path !== "string" || typeof source.branch !== "string" || typeof source.baseline !== "string") {
      throw new OrchestratorStop("DESTRUCTIVE_OPERATION", "The ownership marker failed its closed contract.", taskId);
    }
    return source as unknown as OwnerMarker;
  }

  private async git(commandId: string, arguments_: readonly string[]) {
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
