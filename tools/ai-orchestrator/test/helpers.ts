// Purpose: Supplies deterministic closed-contract fixtures and in-memory ports for orchestrator tests.
import type {
  AgentResult,
  PersistedRunState,
  ProjectPlan,
  TaskDefinition,
} from "../src/core/contracts.js";
import type { EventSink, StructuredEvent } from "../src/observability/structured-log.js";
import type { ResourceLockOwner, ResourceLocks } from "../src/ports/resource-locks.js";
import type { StateStore } from "../src/ports/state-store.js";

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
  const inferredKind = overrides.humanGate === true
    ? "HUMAN_GATE"
    : overrides.owner === "independent_reviewer"
      ? "INDEPENDENT_REVIEW"
      : overrides.owner === "security_reviewer"
        ? "SECURITY_REVIEW"
        : writable
          ? "IMPLEMENTATION"
          : "DISCOVERY";
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
    requiredTests: [],
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

  public async acquire(resource: string, owner: ResourceLockOwner): Promise<void> {
    if (this.held.has(resource)) {
      throw new Error("Resource already held.");
    }
    this.held.set(resource, owner);
  }

  public async release(resource: string, runId: string, attemptId: string): Promise<void> {
    const owner = this.held.get(resource);
    if (owner?.runId !== runId || owner.attemptId !== attemptId) {
      throw new Error("Resource owner mismatch.");
    }
    this.held.delete(resource);
  }

  public async inspect(): Promise<readonly string[]> {
    return [...this.held.keys()].sort();
  }

  public async inspectRecords() {
    return [...this.held.entries()].map(([lockId, owner]) => ({ lockId, status: "ACTIVE" as const, runId: owner.runId, taskId: owner.taskId, attemptId: owner.attemptId, acquiredAt: owner.acquiredAt }));
  }
}

export class CollectingEvents implements EventSink {
  public readonly events: StructuredEvent[] = [];
  public write(event: StructuredEvent): void {
    this.events.push(event);
  }
}
