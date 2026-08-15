// Purpose: Supplies deterministic closed-contract fixtures and in-memory ports for orchestrator tests.
import { createHash } from "node:crypto";
import type {
  AgentResult,
  PersistedRunState,
  ProjectPlan,
  TaskDefinition,
  ThreadCheckpoint,
} from "../src/core/contracts.js";
import type { EventSink, StructuredEvent } from "../src/observability/structured-log.js";
import type { PreparedResourceLock, ResourceLockOwner, ResourceLocks } from "../src/ports/resource-locks.js";
import type { StateStore } from "../src/ports/state-store.js";
import type { ThreadCheckpointStore } from "../src/ports/thread-checkpoints.js";

export const baseline = "0123456789abcdef0123456789abcdef01234567";
export const instant = "2026-08-14T12:00:00.000Z";

export function passingResult(changedFiles: readonly string[] = []): AgentResult {
  return {
    schemaVersion: 1,
    status: "PASS",
    summary: "The bounded task completed.",
    changedFiles,
    commands: [],
    tests: [],
    evidence: ["fixture-evidence"],
    risks: [],
    blockers: [],
    requestedAuthority: [],
    stopCondition: null,
  };
}

export function task(overrides: Partial<TaskDefinition> = {}): TaskDefinition {
  const writable = overrides.owner === "implementation_worker";
  const worktree = overrides.worktree ?? (writable ? "C:/managed/default" : null);
  const inferredKind = overrides.taskKind ?? (overrides.humanGate === true
    ? "HUMAN_GATE"
    : overrides.owner === "independent_reviewer"
      ? "INDEPENDENT_REVIEW"
      : overrides.owner === "security_reviewer"
        ? "SECURITY_REVIEW"
        : writable
          ? "IMPLEMENTATION"
          : "DISCOVERY");
  const tools = inferredKind === "IMPLEMENTATION"
    ? ["shell", "apply_patch"]
    : ["DISCOVERY", "INDEPENDENT_REVIEW", "SECURITY_REVIEW"].includes(inferredKind)
      ? ["shell"]
      : [];
  return {
    taskId: "map-repository",
    taskKind: inferredKind,
    title: "Map repository",
    objective: "Map the authorised repository paths.",
    authority: { references: ["owner-request"], grants: ["read-only inspection"], negativeScope: ["no writes"] },
    executionSurface: {
      cwd: worktree ?? "C:/repository",
      writableRoots: writable && worktree !== null ? [worktree] : [],
      sandbox: writable ? "workspace-write" : "read-only",
      approvalPolicy: "never",
      networkAccess: false,
      environmentPolicy: "minimal",
      tools,
      mcpServers: [],
      skills: [],
    },
    owner: "code_mapper",
    status: "DISCOVERED",
    priority: 100,
    dependencies: [],
    blockedBy: [],
    allowedPaths: [],
    forbiddenPaths: ["reference-materials"],
    ownership: "READ_ONLY_FOR_WORKERS",
    sharedResources: [],
    requiredContracts: [],
    acceptanceCriteria: ["Return a bounded map."],
    requiredTests: ["IMPLEMENTATION", "INTEGRATION", "QUALITY_GATE"].includes(inferredKind) ? ["./eng/ci.ps1 -Offline"] : [],
    stopConditions: ["UNEXPECTED_DIRTY_TREE"],
    deliverables: ["repository-map"],
    worktree,
    branch: null,
    parallelism: "SAFE_PARALLEL",
    requiresIndependentReview: false,
    requiresSecurityReview: false,
    humanGate: false,
    candidateTaskId: null,
    candidate: null,
    maxAttempts: 1,
    createdAt: instant,
    startedAt: null,
    finishedAt: null,
    result: null,
    evidence: [],
    ...overrides,
  };
}

export function projectPlan(tasks: readonly TaskDefinition[], maxConcurrency = 3): ProjectPlan {
  return { schemaVersion: 1, project: "RAG-Challenge", baseline, maxConcurrency, tasks };
}

export class InMemoryStateStore implements StateStore {
  public readonly saved: PersistedRunState[] = [];

  public async save(state: PersistedRunState): Promise<void> {
    this.saved.push(structuredClone(state));
  }

  public async load(runId: string): Promise<PersistedRunState> {
    const state = [...this.saved].reverse().find((candidate) => candidate.runId === runId);
    if (state === undefined) {
      throw new Error("Missing in-memory run.");
    }
    return structuredClone(state);
  }
}

export class InMemoryResourceLocks implements ResourceLocks {
  public readonly held = new Map<string, ResourceLockOwner>();
  public readonly prepared = new Map<string, { prepared: PreparedResourceLock; owner: ResourceLockOwner }>();
  public ownerAbsent = false;
  public readonly statuses = new Map<string, "ACTIVE" | "OWNER_PROCESS_ABSENT">();

  public async acquire(resource: string, owner: ResourceLockOwner): Promise<void> {
    if (this.held.has(resource)) {
      throw new Error("Resource already held.");
    }
    this.held.set(resource, owner);
  }

  public async release(resource: string, runId: string, attemptId: string): Promise<PreparedResourceLock> {
    const owner = this.held.get(resource);
    if (owner?.runId !== runId || owner.attemptId !== attemptId) {
      throw new Error("Resource owner mismatch.");
    }
    this.held.delete(resource);
    this.statuses.delete(resource);
    const prepared = { lockId: `${resource}.${runId}.${attemptId}.release`, resource, action: "RELEASE" as const, recordDigest: this.digest(resource, owner) };
    this.prepared.set(prepared.lockId, { prepared, owner });
    return prepared;
  }

  public async inspect(): Promise<readonly string[]> {
    return [...this.held.keys(), ...this.prepared.keys()].sort();
  }

  public async inspectRecords() {
    return [
      ...[...this.held.entries()].map(([lockId, owner]) => ({ lockId, resource: lockId, status: this.statuses.get(lockId) ?? (this.ownerAbsent ? "OWNER_PROCESS_ABSENT" as const : "ACTIVE" as const), runId: owner.runId, taskId: owner.taskId, attemptId: owner.attemptId, acquiredAt: owner.acquiredAt, recordDigest: this.digest(lockId, owner) })),
      ...[...this.prepared.values()].map(({ prepared, owner }) => ({ lockId: prepared.lockId, resource: prepared.resource, status: prepared.action === "RELEASE" ? "RELEASE_PREPARED" as const : "RECONCILE_PREPARED" as const, runId: owner.runId, taskId: owner.taskId, attemptId: owner.attemptId, acquiredAt: owner.acquiredAt, recordDigest: prepared.recordDigest })),
    ];
  }
  public async reconcileAbsentOwner(lockId: string, expected: ResourceLockOwner): Promise<PreparedResourceLock> {
    if ((this.statuses.get(lockId) ?? (this.ownerAbsent ? "OWNER_PROCESS_ABSENT" : "ACTIVE")) !== "OWNER_PROCESS_ABSENT") throw new Error("Lock owner remains active.");
    const entry = [...this.held.entries()].find(([resource, owner]) => resource === lockId && owner.runId === expected.runId && owner.attemptId === expected.attemptId);
    if (entry === undefined) throw new Error("Missing interrupted lock.");
    this.held.delete(entry[0]);
    this.statuses.delete(entry[0]);
    const prepared = { lockId: `${entry[0]}.${expected.runId}.${expected.attemptId}.reconcile`, resource: entry[0], action: "RECONCILE" as const, recordDigest: this.digest(entry[0], entry[1]) };
    this.prepared.set(prepared.lockId, { prepared, owner: entry[1] });
    return prepared;
  }
  public async finalise(prepared: PreparedResourceLock, expected: ResourceLockOwner): Promise<void> {
    const existing = this.prepared.get(prepared.lockId);
    if (existing === undefined || existing.owner.runId !== expected.runId || existing.owner.taskId !== expected.taskId || existing.owner.attemptId !== expected.attemptId || existing.owner.acquiredAt !== expected.acquiredAt) {
      throw new Error("Prepared lock owner mismatch.");
    }
    this.prepared.delete(prepared.lockId);
  }

  private digest(resource: string, owner: ResourceLockOwner): string {
    return createHash("sha256").update(JSON.stringify({ resource, ...owner }), "utf8").digest("hex");
  }
}

export class CollectingEvents implements EventSink {
  public readonly events: StructuredEvent[] = [];
  public write(event: StructuredEvent): void {
    this.events.push(event);
  }
}

export class InMemoryThreadCheckpoints implements ThreadCheckpointStore {
  public readonly values = new Map<string, ThreadCheckpoint>();
  private key(runId: string, taskId: string): string { return `${runId}\u0000${taskId}`; }
  public async save(checkpoint: ThreadCheckpoint): Promise<void> {
    const key = this.key(checkpoint.runId, checkpoint.taskId);
    const existing = this.values.get(key);
    if (existing !== undefined && JSON.stringify(existing) !== JSON.stringify(checkpoint)) throw new Error("Checkpoint mismatch.");
    this.values.set(key, structuredClone(checkpoint));
  }
  public async load(runId: string, taskId: string): Promise<ThreadCheckpoint | null> {
    return structuredClone(this.values.get(this.key(runId, taskId)) ?? null);
  }
  public async remove(expected: ThreadCheckpoint): Promise<void> {
    const key = this.key(expected.runId, expected.taskId);
    if (JSON.stringify(this.values.get(key)) !== JSON.stringify(expected)) throw new Error("Checkpoint owner mismatch.");
    this.values.delete(key);
  }
  public async inspect(runId: string): Promise<readonly ThreadCheckpoint[]> {
    return [...this.values.values()].filter((checkpoint) => checkpoint.runId === runId).map((checkpoint) => structuredClone(checkpoint));
  }
  public async inspectPreparedRemovals(): Promise<readonly ThreadCheckpoint[]> { return []; }
  public async finalisePreparedRemoval(): Promise<void> { throw new Error("No prepared in-memory checkpoint removal exists."); }
}
