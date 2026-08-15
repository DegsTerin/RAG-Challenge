// Purpose: Persists recoverable run snapshots and a tamper-evident journal using same-volume atomic replacement and fsync.
import { createHash } from "node:crypto";
import { mkdir, open, readFile, readdir, rename, stat } from "node:fs/promises";
import { basename, join } from "node:path";
import {
  agentIds,
  retryClasses,
  type AgentId,
  type AttemptRecord,
  type PersistedRunState,
  type RetryClass,
} from "../core/contracts.js";
import { canonicalJson } from "../core/canonical-json.js";
import { OrchestratorStop } from "../core/errors.js";
import { assertIdentifier, parseAgentResult, parseProjectPlan } from "../core/validation.js";
import { assertNoExistingReparseBoundary, resolveRunRoot } from "../security/path-policy.js";
import { parseSecureJson } from "../security/secure-json.js";
import type { StateStore } from "../ports/state-store.js";

interface JournalEntry {
  readonly sequence: number;
  readonly event: "STATE_SAVED";
  readonly runId: string;
  readonly revision: number;
  readonly stateHash: string;
  readonly previousHash: string | null;
  readonly recordedAt: string;
}

const maximumSnapshotBytes = 8_388_608;
const maximumJournalBytes = 16_777_216;

async function readBounded(path: string, maximumBytes: number, label: string): Promise<string> {
  const metadata = await stat(path);
  if (!metadata.isFile() || metadata.size > maximumBytes) {
    throw new OrchestratorStop("TEST_BASELINE_BROKEN", `${label} is not a bounded regular file.`);
  }
  return await readFile(path, "utf8");
}

function hash(value: string): string {
  return createHash("sha256").update(value, "utf8").digest("hex");
}

function snapshotName(revision: number): string {
  return `state-${revision.toString().padStart(8, "0")}.json`;
}

function instant(value: unknown, label: string): string {
  if (typeof value !== "string" || !/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{3})?Z$/.test(value) || Number.isNaN(Date.parse(value))) {
    throw new OrchestratorStop("TEST_BASELINE_BROKEN", `${label} is not a UTC instant.`);
  }
  return value;
}

function parseAttempts(value: unknown, taskIds: ReadonlySet<string>): AttemptRecord[] {
  if (!Array.isArray(value) || value.length > 768) {
    throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Persisted attempts are not a bounded array.");
  }
  return value.map((entry, index) => {
    if (entry === null || typeof entry !== "object" || Array.isArray(entry)) {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Persisted attempt ${index} is invalid.`);
    }
    const attempt = entry as Record<string, unknown>;
    const allowed = ["attemptId", "taskId", "agentId", "startedAt", "finishedAt", "retryClass", "threadId", "result"];
    if (Object.keys(attempt).some((key) => !allowed.includes(key)) || typeof attempt.attemptId !== "string" || typeof attempt.taskId !== "string") {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Persisted attempt ${index} failed its closed contract.`);
    }
    assertIdentifier(attempt.attemptId, `attempts[${index}].attemptId`);
    assertIdentifier(attempt.taskId, `attempts[${index}].taskId`);
    if (!taskIds.has(attempt.taskId) || typeof attempt.agentId !== "string" || !agentIds.includes(attempt.agentId as AgentId)) {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Persisted attempt ${index} references an unknown task or agent.`);
    }
    const retryClass = attempt.retryClass === null
      ? null
      : typeof attempt.retryClass === "string" && retryClasses.includes(attempt.retryClass as RetryClass)
        ? attempt.retryClass as RetryClass
        : (() => { throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Persisted attempt ${index} has an invalid retry class.`); })();
    return {
      attemptId: attempt.attemptId,
      taskId: attempt.taskId,
      agentId: attempt.agentId as AgentId,
      startedAt: instant(attempt.startedAt, `attempts[${index}].startedAt`),
      finishedAt: attempt.finishedAt === null ? null : instant(attempt.finishedAt, `attempts[${index}].finishedAt`),
      retryClass,
      threadId: attempt.threadId === null
        ? null
        : typeof attempt.threadId === "string" && attempt.threadId.length > 0 && attempt.threadId.length <= 256
          ? attempt.threadId
          : (() => { throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Persisted attempt ${index} has an invalid thread ID.`); })(),
      result: attempt.result === null ? null : parseAgentResult(attempt.result),
    };
  });
}

function parseState(value: unknown, expectedRunId: string): PersistedRunState {
  if (value === null || typeof value !== "object" || Array.isArray(value)) {
    throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Persisted run state is not an object.");
  }
  const source = value as Record<string, unknown>;
  const allowed = [
    "schemaVersion", "runId", "revision", "baseline", "maxConcurrency", "createdAt", "updatedAt", "tasks", "attempts", "heldLocks",
    "humanGateReached",
  ];
  const unexpected = Object.keys(source).filter((key) => !allowed.includes(key));
  if (unexpected.length > 0 || source.schemaVersion !== 1 || source.runId !== expectedRunId ||
      !Number.isInteger(source.revision) || (source.revision as number) < 0 ||
      !Number.isInteger(source.maxConcurrency) || (source.maxConcurrency as number) < 1 || (source.maxConcurrency as number) > 3 ||
      !Array.isArray(source.attempts) || !Array.isArray(source.heldLocks) || typeof source.humanGateReached !== "boolean") {
    throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Persisted run state failed its closed contract.");
  }
  const plan = parseProjectPlan({
    schemaVersion: 1,
    project: "RAG-Challenge",
    baseline: source.baseline,
    maxConcurrency: source.maxConcurrency,
    tasks: source.tasks,
  });
  for (const lock of source.heldLocks) {
    if (typeof lock !== "string" || lock.length === 0 || lock.length > 128) {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Persisted lock metadata is invalid.");
    }
  }
  const attempts = parseAttempts(source.attempts, new Set(plan.tasks.map((task) => task.taskId)));
  return {
    schemaVersion: 1,
    runId: expectedRunId,
    revision: source.revision as number,
    baseline: plan.baseline,
    maxConcurrency: plan.maxConcurrency,
    createdAt: instant(source.createdAt, "state.createdAt"),
    updatedAt: instant(source.updatedAt, "state.updatedAt"),
    tasks: plan.tasks,
    attempts,
    heldLocks: source.heldLocks as readonly string[],
    humanGateReached: source.humanGateReached,
  };
}

async function syncDirectory(directory: string): Promise<void> {
  const handle = await open(directory, "r");
  try {
    try {
      await handle.sync();
    } catch (error) {
      if (process.platform !== "win32" || (error as NodeJS.ErrnoException).code !== "EPERM") {
        throw error;
      }
    }
  } finally {
    await handle.close();
  }
}

export class FileStateStore implements StateStore {
  public constructor(private readonly stateRoot: string) {}

  public async save(state: PersistedRunState): Promise<void> {
    const validated = parseState(parseSecureJson(canonicalJson(state), "State snapshot"), state.runId);
    const runRoot = resolveRunRoot(this.stateRoot, validated.runId);
    await assertNoExistingReparseBoundary(this.stateRoot, runRoot);
    await mkdir(runRoot, { recursive: true });
    const finalPath = join(runRoot, snapshotName(validated.revision));
    const temporaryPath = `${finalPath}.tmp`;
    try {
      await stat(finalPath);
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", `State revision ${validated.revision} already exists.`);
    } catch (error) {
      if ((error as NodeJS.ErrnoException).code !== "ENOENT") {
        throw error;
      }
    }
    const serialised = canonicalJson(validated);
    if (Buffer.byteLength(serialised, "utf8") > maximumSnapshotBytes) {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "The state snapshot exceeds its persistence limit.");
    }
    const handle = await open(temporaryPath, "wx", 0o600);
    try {
      await handle.writeFile(serialised, { encoding: "utf8" });
      await handle.sync();
    } finally {
      await handle.close();
    }
    await rename(temporaryPath, finalPath);
    await syncDirectory(runRoot);
    await this.appendJournal(runRoot, validated, hash(serialised));
  }

  public async load(runId: string): Promise<PersistedRunState> {
    const runRoot = resolveRunRoot(this.stateRoot, runId);
    await assertNoExistingReparseBoundary(this.stateRoot, runRoot);
    const files = await readdir(runRoot);
    const partial = files.find((file) => file.endsWith(".tmp"));
    if (partial !== undefined) {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Interrupted state write '${partial}' requires operator cleanup.`);
    }
    const snapshots = files.filter((file) => /^state-\d{8}\.json$/.test(file)).sort().reverse();
    const latest = snapshots[0];
    if (latest === undefined) {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Run '${runId}' has no complete state snapshot.`);
    }
    await this.verifyJournal(runRoot, runId);
    const text = await readBounded(join(runRoot, latest), maximumSnapshotBytes, "State snapshot");
    const parsed = parseState(parseSecureJson(text, "State snapshot"), runId);
    if (snapshotName(parsed.revision) !== basename(latest)) {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Snapshot filename and revision disagree.");
    }
    if (hash(canonicalJson(parsed)) !== await this.latestJournalStateHash(runRoot)) {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Latest state does not match its journal digest.");
    }
    return parsed;
  }

  private async appendJournal(runRoot: string, state: PersistedRunState, stateHash: string): Promise<void> {
    const journalPath = join(runRoot, "journal.jsonl");
    let existing = "";
    try {
      existing = await readBounded(journalPath, maximumJournalBytes, "State journal");
    } catch (error) {
      if ((error as NodeJS.ErrnoException).code !== "ENOENT") {
        throw error;
      }
    }
    const lines = existing.split("\n").filter((line) => line.length > 0);
    const previousHash = lines.length === 0 ? null : hash(`${lines.at(-1)}\n`);
    const entry: JournalEntry = {
      sequence: lines.length + 1,
      event: "STATE_SAVED",
      runId: state.runId,
      revision: state.revision,
      stateHash,
      previousHash,
      recordedAt: state.updatedAt,
    };
    if (Buffer.byteLength(existing, "utf8") + Buffer.byteLength(canonicalJson(entry), "utf8") > maximumJournalBytes) {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "The state journal exceeds its persistence limit.");
    }
    const temporaryPath = join(runRoot, "journal.jsonl.tmp");
    const handle = await open(temporaryPath, "wx", 0o600);
    try {
      await handle.writeFile(`${existing}${canonicalJson(entry)}`, { encoding: "utf8" });
      await handle.sync();
    } finally {
      await handle.close();
    }
    await rename(temporaryPath, journalPath);
    await syncDirectory(runRoot);
  }

  private async verifyJournal(runRoot: string, expectedRunId?: string): Promise<readonly JournalEntry[]> {
    const text = await readBounded(join(runRoot, "journal.jsonl"), maximumJournalBytes, "State journal");
    const lines = text.split("\n").filter((line) => line.length > 0);
    if (lines.length === 0 || lines.length > 4096) throw new OrchestratorStop("TEST_BASELINE_BROKEN", "The state journal has an invalid entry count.");
    const entries = lines.map((line, index) => {
      const parsed = parseSecureJson(line, `State journal entry ${index + 1}`);
      if (parsed === null || typeof parsed !== "object" || Array.isArray(parsed)) throw new OrchestratorStop("TEST_BASELINE_BROKEN", "A state journal entry is not an object.");
      const source = parsed as Record<string, unknown>;
      if (Object.keys(source).sort().join(",") !== "event,previousHash,recordedAt,revision,runId,sequence,stateHash" ||
          source.event !== "STATE_SAVED" || typeof source.runId !== "string" || typeof source.sequence !== "number" ||
          typeof source.revision !== "number" || !Number.isInteger(source.sequence) || !Number.isInteger(source.revision) ||
          typeof source.stateHash !== "string" || !/^[0-9a-f]{64}$/.test(source.stateHash) ||
          !(source.previousHash === null || (typeof source.previousHash === "string" && /^[0-9a-f]{64}$/.test(source.previousHash))) ||
          typeof source.recordedAt !== "string") {
        throw new OrchestratorStop("TEST_BASELINE_BROKEN", "A state journal entry failed its closed contract.");
      }
      instant(source.recordedAt, `journal[${index}].recordedAt`);
      return source as unknown as JournalEntry;
    });
    let previousLine: string | null = null;
    for (const [index, entry] of entries.entries()) {
      const expectedPrevious = previousLine === null ? null : hash(`${previousLine}\n`);
      if (entry.sequence !== index + 1 || entry.revision !== index || entry.previousHash !== expectedPrevious ||
          (expectedRunId !== undefined && entry.runId !== expectedRunId) || lines[index] !== canonicalJson(entry).trimEnd()) {
        throw new OrchestratorStop("TEST_BASELINE_BROKEN", "The state journal hash chain is invalid.");
      }
      previousLine = lines[index] ?? null;
    }
    return entries;
  }

  private async latestJournalStateHash(runRoot: string): Promise<string> {
    const entries = await this.verifyJournal(runRoot);
    const latest = entries.at(-1);
    if (latest === undefined) {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "The state journal is empty.");
    }
    return latest.stateHash;
  }
}
