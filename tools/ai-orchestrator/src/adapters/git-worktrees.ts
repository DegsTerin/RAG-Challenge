// Purpose: Manages only coordinator-marked Git worktrees through fixed argv calls and refuses foreign or dirty resources.
import { mkdir, readFile, unlink, writeFile } from "node:fs/promises";
import { isAbsolute, join, relative, resolve, sep } from "node:path";
import type { ProcessExecutor } from "../ports/process-executor.js";
import type { WorktreeManager, WorktreeRecord } from "../ports/worktrees.js";
import { OrchestratorStop } from "../core/errors.js";
import { assertIdentifier } from "../core/validation.js";

function parsePorcelain(output: readonly string[]): readonly WorktreeRecord[] {
  const records: WorktreeRecord[] = [];
  let current: { path?: string; head?: string; branch?: string | null; prunable?: boolean } = {};
  const flush = (): void => {
    if (current.path !== undefined && current.head !== undefined) {
      records.push({ path: current.path, head: current.head, branch: current.branch ?? null, prunable: current.prunable ?? false });
    }
    current = {};
  };
  for (const line of output) {
    if (line.startsWith("worktree ")) {
      flush();
      current.path = line.slice(9);
    } else if (line.startsWith("HEAD ")) {
      current.head = line.slice(5);
    } else if (line.startsWith("branch refs/heads/")) {
      current.branch = line.slice(18);
    } else if (line === "prunable") {
      current.prunable = true;
    }
  }
  flush();
  return records;
}

export class GitWorktreeManager implements WorktreeManager {
  public constructor(
    private readonly repositoryRoot: string,
    private readonly managedRoot: string,
    private readonly process: ProcessExecutor,
    private readonly environment: Readonly<Record<string, string>>,
  ) {}

  public async list(): Promise<readonly WorktreeRecord[]> {
    const result = await this.git("worktree-list", ["worktree", "list", "--porcelain"]);
    if (result.result !== "PASS") {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Git worktree discovery failed.");
    }
    return parsePorcelain(result.relevantOutput);
  }

  public async create(taskId: string, path: string, branch: string, baseline: string): Promise<WorktreeRecord> {
    assertIdentifier(taskId, "taskId");
    if (!this.isManagedPath(path) || !branch.startsWith("codex/")) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "Worktree path or branch is outside the managed namespace.", taskId);
    }
    if ((await this.list()).some((record) => record.path === path || record.branch === branch)) {
      throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", "The requested worktree path or branch already exists.", taskId);
    }
    await mkdir(this.managedRoot, { recursive: true });
    const result = await this.git("worktree-create", ["worktree", "add", "-b", branch, path, baseline]);
    if (result.result !== "PASS") {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Git worktree creation failed.", taskId);
    }
    await mkdir(join(this.managedRoot, ".owners"), { recursive: true });
    await writeFile(this.ownerMarker(taskId), JSON.stringify({ taskId, path: resolve(path), branch, baseline }), { encoding: "utf8", flag: "wx", mode: 0o600 });
    return await this.validate(path, branch, baseline);
  }

  public async validate(path: string, branch: string, baseline: string): Promise<WorktreeRecord> {
    const record = (await this.list()).find((candidate) => candidate.path === path);
    if (record === undefined || record.branch !== branch || record.head !== baseline || record.prunable) {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "The worktree mapping differs from its task envelope.");
    }
    const status = await this.git("worktree-status", ["-C", path, "status", "--porcelain"]);
    if (status.result !== "PASS" || status.relevantOutput.length !== 0) {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "The managed worktree is not clean.");
    }
    return record;
  }

  public async removeManaged(taskId: string, path: string): Promise<void> {
    const marker = this.ownerMarker(taskId);
    let owner: { taskId?: string; path?: string };
    try {
      owner = JSON.parse(await readFile(marker, "utf8")) as { taskId?: string; path?: string };
    } catch {
      throw new OrchestratorStop("DESTRUCTIVE_OPERATION", "The worktree has no readable orchestrator ownership marker.", taskId);
    }
    if (owner.taskId !== taskId || owner.path !== resolve(path) || !this.isManagedPath(path)) {
      throw new OrchestratorStop("DESTRUCTIVE_OPERATION", "The worktree ownership marker does not match the task.", taskId);
    }
    const record = (await this.list()).find((candidate) => resolve(candidate.path) === resolve(path));
    if (record === undefined || record.branch === null) {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "The managed worktree mapping is absent.", taskId);
    }
    await this.validate(path, record.branch, record.head);
    const result = await this.git("worktree-remove", ["worktree", "remove", path]);
    if (result.result !== "PASS") {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Git worktree removal failed.", taskId);
    }
    await unlink(marker);
  }

  private isManagedPath(path: string): boolean {
    const relation = relative(resolve(this.managedRoot), resolve(path));
    return relation !== "" && relation !== ".." && !relation.startsWith(`..${sep}`) && !isAbsolute(relation);
  }

  private ownerMarker(taskId: string): string {
    return join(this.managedRoot, ".owners", `${taskId}.json`);
  }

  private async git(commandId: string, arguments_: readonly string[]) {
    return await this.process.run({
      commandId,
      executable: "git",
      arguments: arguments_,
      cwd: this.repositoryRoot,
      environment: this.environment,
      timeoutMs: 120_000,
      maximumOutputBytes: 1_048_576,
      maximumRelevantLines: 4096,
    });
  }
}
