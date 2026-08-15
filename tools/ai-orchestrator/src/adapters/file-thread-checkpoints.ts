// Purpose: Stores bounded thread checkpoints atomically inside the physically contained orchestrator state root.
import { mkdir, open, readdir, rename, unlink } from "node:fs/promises";
import { dirname, join } from "node:path";
import { canonicalJson } from "../core/canonical-json.js";
import { agentIds, taskKinds, type AgentId, type TaskKind, type ThreadCheckpoint } from "../core/contracts.js";
import { OrchestratorStop } from "../core/errors.js";
import { assertIdentifier } from "../core/validation.js";
import type { ThreadCheckpointStore } from "../ports/thread-checkpoints.js";
import { assertNoExistingReparseBoundary, readBoundedRegularFile, resolveContained, resolveRunRoot } from "../security/path-policy.js";
import { parseSecureJson } from "../security/secure-json.js";

const maximumCheckpointBytes = 8_192;

function removalName(checkpoint: ThreadCheckpoint): string {
  return `${checkpoint.taskId}.${checkpoint.attemptId}.remove`;
}

function parseCheckpoint(value: unknown, runId: string, taskId: string): ThreadCheckpoint {
  if (value === null || typeof value !== "object" || Array.isArray(value)) {
    throw new OrchestratorStop("TEST_BASELINE_BROKEN", "A thread checkpoint is invalid.", taskId);
  }
  const source = value as Record<string, unknown>;
  const keys = ["agentId", "attemptId", "baseline", "candidateCommitId", "deadlineMs", "envelopeHash", "runId", "schemaVersion", "startedAt", "stateRevision", "taskId", "taskKind", "threadId"];
  if (Object.keys(source).sort().join("\u0000") !== keys.sort().join("\u0000") || source.schemaVersion !== 1 ||
      source.runId !== runId || source.taskId !== taskId || typeof source.attemptId !== "string" || typeof source.threadId !== "string" ||
      typeof source.agentId !== "string" || !agentIds.includes(source.agentId as AgentId) || typeof source.startedAt !== "string" ||
      typeof source.taskKind !== "string" || !taskKinds.includes(source.taskKind as TaskKind) || typeof source.baseline !== "string" || !/^[0-9a-f]{40}$/.test(source.baseline) ||
      (source.candidateCommitId !== null && (typeof source.candidateCommitId !== "string" || !/^[0-9a-f]{40}$/.test(source.candidateCommitId))) ||
      typeof source.envelopeHash !== "string" || !/^[0-9a-f]{64}$/.test(source.envelopeHash) ||
      !Number.isInteger(source.stateRevision) || (source.stateRevision as number) < 0 ||
      !Number.isInteger(source.deadlineMs) || (source.deadlineMs as number) < 1 || (source.deadlineMs as number) > 7_200_000 ||
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
    taskKind: source.taskKind as TaskKind,
    baseline: source.baseline as string,
    candidateCommitId: source.candidateCommitId as string | null,
    envelopeHash: source.envelopeHash as string,
    stateRevision: source.stateRevision as number,
    deadlineMs: source.deadlineMs as number,
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
      const text = await readBoundedRegularFile(this.authorityRoot, path, maximumCheckpointBytes, "Thread checkpoint", "TEST_BASELINE_BROKEN", taskId);
      return parseCheckpoint(parseSecureJson(text, "Thread checkpoint"), runId, taskId);
    } catch (error) {
      if ((error as NodeJS.ErrnoException).code === "ENOENT") return null;
      if (error instanceof OrchestratorStop) throw error;
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "A thread checkpoint is unreadable.", taskId);
    }
  }

  public async remove(expected: ThreadCheckpoint): Promise<void> {
    const existing = await this.load(expected.runId, expected.taskId);
    if (existing === null || canonicalJson(existing) !== canonicalJson(expected)) {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "Thread checkpoint ownership changed.", expected.taskId);
    }
    const directory = this.directory(expected.runId);
    const path = resolveContained(directory, `${expected.taskId}.json`);
    const tombstone = resolveContained(directory, removalName(expected));
    await assertNoExistingReparseBoundary(this.authorityRoot, directory);
    await rename(path, tombstone);
    await assertNoExistingReparseBoundary(this.authorityRoot, directory);
    const movedText = await readBoundedRegularFile(this.authorityRoot, tombstone, maximumCheckpointBytes, "Removed thread checkpoint", "TEST_BASELINE_BROKEN", expected.taskId);
    const moved = parseCheckpoint(parseSecureJson(movedText, "Removed thread checkpoint"), expected.runId, expected.taskId);
    if (canonicalJson(moved) !== canonicalJson(existing)) {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "Thread checkpoint changed during removal.", expected.taskId);
    }
    await unlink(tombstone);
  }

  public async inspect(runId: string): Promise<readonly ThreadCheckpoint[]> {
    assertIdentifier(runId, "runId");
    const directory = this.directory(runId);
    await assertNoExistingReparseBoundary(this.authorityRoot, directory);
    try {
      const names = (await readdir(directory)).sort();
      if (names.some((name) => !/^[a-z0-9][a-z0-9-]{0,63}\.json$/.test(name) && !/^[a-z0-9][a-z0-9-]{0,63}\.[a-z0-9][a-z0-9-]{0,63}\.remove$/.test(name))) {
        throw new OrchestratorStop("TEST_BASELINE_BROKEN", "The thread checkpoint directory contains an unexpected artefact.");
      }
      return await Promise.all(names.filter((name) => name.endsWith(".json")).map(async (name) => {
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

  public async inspectPreparedRemovals(runId: string): Promise<readonly ThreadCheckpoint[]> {
    assertIdentifier(runId, "runId");
    const directory = this.directory(runId);
    await assertNoExistingReparseBoundary(this.authorityRoot, directory);
    try {
      const names = (await readdir(directory)).filter((name) => name.endsWith(".remove")).sort();
      return await Promise.all(names.map(async (name) => {
        const match = /^([a-z0-9][a-z0-9-]{0,63})\.([a-z0-9][a-z0-9-]{0,63})\.remove$/.exec(name);
        if (match === null) throw new OrchestratorStop("TEST_BASELINE_BROKEN", "A prepared checkpoint removal has an invalid name.");
        const path = resolveContained(directory, name);
        const text = await readBoundedRegularFile(this.authorityRoot, path, maximumCheckpointBytes, "Prepared checkpoint removal", "TEST_BASELINE_BROKEN", match[1]);
        const checkpoint = parseCheckpoint(parseSecureJson(text, "Prepared checkpoint removal"), runId, match[1]!);
        if (checkpoint.attemptId !== match[2] || removalName(checkpoint) !== name) throw new OrchestratorStop("TEST_BASELINE_BROKEN", "A prepared checkpoint removal changed identity.", checkpoint.taskId);
        return checkpoint;
      }));
    } catch (error) {
      if ((error as NodeJS.ErrnoException).code === "ENOENT") return [];
      throw error;
    }
  }

  public async finalisePreparedRemoval(expected: ThreadCheckpoint): Promise<void> {
    const directory = this.directory(expected.runId);
    await assertNoExistingReparseBoundary(this.authorityRoot, directory);
    const path = resolveContained(directory, removalName(expected));
    const text = await readBoundedRegularFile(this.authorityRoot, path, maximumCheckpointBytes, "Prepared checkpoint removal", "TEST_BASELINE_BROKEN", expected.taskId);
    const moved = parseCheckpoint(parseSecureJson(text, "Prepared checkpoint removal"), expected.runId, expected.taskId);
    if (canonicalJson(moved) !== canonicalJson(expected)) throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "Prepared checkpoint removal ownership changed.", expected.taskId);
    await unlink(path);
    await assertNoExistingReparseBoundary(this.authorityRoot, directory);
  }

  private directory(runId: string): string {
    return join(resolveRunRoot(this.stateRoot, runId), "threads");
  }
}
