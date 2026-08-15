// Purpose: Stores bounded thread checkpoints atomically inside the physically contained orchestrator state root.
import { mkdir, open, readFile, readdir, stat, unlink } from "node:fs/promises";
import { dirname, join } from "node:path";
import { canonicalJson } from "../core/canonical-json.js";
import { agentIds, type AgentId, type ThreadCheckpoint } from "../core/contracts.js";
import { OrchestratorStop } from "../core/errors.js";
import { assertIdentifier } from "../core/validation.js";
import type { ThreadCheckpointStore } from "../ports/thread-checkpoints.js";
import { assertNoExistingReparseBoundary, resolveContained, resolveRunRoot } from "../security/path-policy.js";
import { parseSecureJson } from "../security/secure-json.js";

const maximumCheckpointBytes = 8_192;

function parseCheckpoint(value: unknown, runId: string, taskId: string): ThreadCheckpoint {
  if (value === null || typeof value !== "object" || Array.isArray(value)) {
    throw new OrchestratorStop("TEST_BASELINE_BROKEN", "A thread checkpoint is invalid.", taskId);
  }
  const source = value as Record<string, unknown>;
  const keys = ["agentId", "attemptId", "runId", "schemaVersion", "startedAt", "taskId", "threadId"];
  if (Object.keys(source).sort().join("\u0000") !== keys.sort().join("\u0000") || source.schemaVersion !== 1 ||
      source.runId !== runId || source.taskId !== taskId || typeof source.attemptId !== "string" || typeof source.threadId !== "string" ||
      typeof source.agentId !== "string" || !agentIds.includes(source.agentId as AgentId) || typeof source.startedAt !== "string" ||
      source.threadId.length === 0 || source.threadId.length > 256 || !/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{3})?Z$/.test(source.startedAt) ||
      Number.isNaN(Date.parse(source.startedAt))) {
    throw new OrchestratorStop("TEST_BASELINE_BROKEN", "A thread checkpoint failed its closed contract.", taskId);
  }
  assertIdentifier(source.attemptId, "checkpoint.attemptId");
  return {
    schemaVersion: 1,
    runId: source.runId as string,
    taskId: source.taskId as string,
    attemptId: source.attemptId as string,
    agentId: source.agentId as AgentId,
    threadId: source.threadId as string,
    startedAt: source.startedAt as string,
  };
}

export class FileThreadCheckpointStore implements ThreadCheckpointStore {
  public constructor(private readonly stateRoot: string, private readonly authorityRoot: string = dirname(stateRoot)) {}

  public async save(checkpoint: ThreadCheckpoint): Promise<void> {
    assertIdentifier(checkpoint.runId, "checkpoint.runId");
    assertIdentifier(checkpoint.taskId, "checkpoint.taskId");
    const parsed = parseCheckpoint(parseSecureJson(canonicalJson(checkpoint), "Thread checkpoint"), checkpoint.runId, checkpoint.taskId);
    const directory = this.directory(parsed.runId);
    await assertNoExistingReparseBoundary(this.authorityRoot, directory);
    await mkdir(directory, { recursive: true });
    await assertNoExistingReparseBoundary(this.authorityRoot, directory);
    const path = resolveContained(directory, `${parsed.taskId}.json`);
    let handle;
    try {
      handle = await open(path, "wx", 0o600);
      await handle.writeFile(canonicalJson(parsed), "utf8");
      await handle.sync();
    } catch (error) {
      if ((error as NodeJS.ErrnoException).code !== "EEXIST") throw error;
      const existing = await this.load(parsed.runId, parsed.taskId);
      if (existing === null || canonicalJson(existing) !== canonicalJson(parsed)) {
        throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "A different thread checkpoint already exists.", parsed.taskId);
      }
    } finally {
      await handle?.close();
    }
  }

  public async load(runId: string, taskId: string): Promise<ThreadCheckpoint | null> {
    assertIdentifier(runId, "runId");
    assertIdentifier(taskId, "taskId");
    const directory = this.directory(runId);
    await assertNoExistingReparseBoundary(this.authorityRoot, directory);
    const path = resolveContained(directory, `${taskId}.json`);
    try {
      const metadata = await stat(path);
      if (!metadata.isFile() || metadata.size > maximumCheckpointBytes) throw new Error("invalid");
      return parseCheckpoint(parseSecureJson(await readFile(path, "utf8"), "Thread checkpoint"), runId, taskId);
    } catch (error) {
      if ((error as NodeJS.ErrnoException).code === "ENOENT") return null;
      if (error instanceof OrchestratorStop) throw error;
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "A thread checkpoint is unreadable.", taskId);
    }
  }

  public async remove(runId: string, taskId: string, attemptId: string): Promise<void> {
    const existing = await this.load(runId, taskId);
    if (existing === null || existing.attemptId !== attemptId) {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "Thread checkpoint ownership changed.", taskId);
    }
    await unlink(resolveContained(this.directory(runId), `${taskId}.json`));
  }

  public async inspect(runId: string): Promise<readonly ThreadCheckpoint[]> {
    assertIdentifier(runId, "runId");
    const directory = this.directory(runId);
    await assertNoExistingReparseBoundary(this.authorityRoot, directory);
    try {
      const names = (await readdir(directory)).filter((name) => /^[a-z0-9][a-z0-9-]{0,63}\.json$/.test(name)).sort();
      return await Promise.all(names.map(async (name) => {
        const taskId = name.slice(0, -5);
        const checkpoint = await this.load(runId, taskId);
        if (checkpoint === null) throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Thread checkpoint disappeared during inspection.", taskId);
        return checkpoint;
      }));
    } catch (error) {
      if ((error as NodeJS.ErrnoException).code === "ENOENT") return [];
      throw error;
    }
  }

  private directory(runId: string): string {
    return join(resolveRunRoot(this.stateRoot, runId), "threads");
  }
}
