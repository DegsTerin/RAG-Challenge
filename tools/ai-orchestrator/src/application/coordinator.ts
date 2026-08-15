// Purpose: Coordinates isolated agent work and deterministic review, integration and quality boundaries without accepting self-asserted evidence.
import { createHash, randomUUID } from "node:crypto";
import { resolve } from "node:path";
import type { QualityGate } from "../adapters/quality-gate.js";
import type {
  AgentResult,
  AgentRunner,
  AttemptRecord,
  CommandEvidence,
  PersistedRunState,
  ProjectPlan,
  RetryClass,
  TaskDefinition,
  TaskStatus,
  ThreadCheckpoint,
} from "../core/contracts.js";
import { canonicalJson } from "../core/canonical-json.js";
import { DependencyGraph } from "../core/dependency-graph.js";
import { errorMessage, OrchestratorStop } from "../core/errors.js";
import { ClassifiedFailure, mayRetry } from "../core/retry.js";
import { scheduleWave } from "../core/scheduler.js";
import { assertTransition } from "../core/state-machine.js";
import { assertRepositoryPath } from "../core/validation.js";
import type { EventSink } from "../observability/structured-log.js";
import { opaqueLocationId } from "../observability/structured-log.js";
import type { CandidateInspector } from "../ports/candidate-inspector.js";
import type { IntegrationExecutor } from "../ports/integration-executor.js";
import type { ResourceLocks } from "../ports/resource-locks.js";
import type { StateStore } from "../ports/state-store.js";
import type { WorktreeManager } from "../ports/worktrees.js";
import type { ThreadCheckpointStore } from "../ports/thread-checkpoints.js";
import { validatePersistedStateSemantics, validatePlanSemantics } from "./plan.js";

export interface Clock {
  now(): string;
}

export interface IdSource {
  runId(): string;
  attemptId(taskId: string, attemptNumber: number): string;
}

export const systemClock: Clock = { now: () => new Date().toISOString() };
export const randomIds: IdSource = {
  runId: () => `run-${randomUUID()}`,
  attemptId: (taskId, attemptNumber) => `attempt-${taskId}-${attemptNumber}`,
};

class AgentDeadlineExceeded extends Error {}
class AgentExecutionCancelled extends Error {}
class AgentTerminationUnconfirmed extends Error {}

function classifyAttemptFailure(error: unknown, task: TaskDefinition, cancelled: boolean): RetryClass {
  if (cancelled) return "CANCELLED";
  if (error instanceof AgentDeadlineExceeded) return "TIMED_OUT";
  if (error instanceof ClassifiedFailure) return error.retryClass;
  if (error instanceof OrchestratorStop) {
    if (error.code === "SHARED_RESOURCE_COLLISION") return "RESOURCE_COLLISION";
    if (["AMBIGUOUS_AUTHORITY", "CONFLICTING_REQUIREMENTS", "ARCHITECTURE_CHANGE_REQUIRED", "PUBLIC_CONTRACT_CHANGE_REQUIRED", "SCHEMA_CHANGE_REQUIRED", "MIGRATION_REQUIRED", "SECRET_REQUIRED", "PROVIDER_CHANGE_REQUIRED", "HUMAN_DECISION_REQUIRED", "HUMAN_GATE_REQUIRED"].includes(error.code)) return "AUTHORITY_FAILURE";
    if (error.code === "TEST_BASELINE_BROKEN") return "TEST_FAILURE";
    if (task.taskKind === "IMPLEMENTATION" && ["UNEXPECTED_DIRTY_TREE", "OUT_OF_SCOPE_CHANGE_REQUIRED"].includes(error.code)) return "IMPLEMENTATION_FAILURE";
  }
  return "POLICY_FAILURE";
}

export function taskEnvelopeHash(task: TaskDefinition): string {
  const immutableEnvelope = {
    taskId: task.taskId,
    taskKind: task.taskKind,
    title: task.title,
    objective: task.objective,
    authority: task.authority,
    executionSurface: task.executionSurface,
    owner: task.owner,
    priority: task.priority,
    dependencies: task.dependencies,
    blockedBy: task.blockedBy,
    allowedPaths: task.allowedPaths,
    forbiddenPaths: task.forbiddenPaths,
    ownership: task.ownership,
    sharedResources: task.sharedResources,
    requiredContracts: task.requiredContracts,
    acceptanceCriteria: task.acceptanceCriteria,
    requiredTests: task.requiredTests,
    stopConditions: task.stopConditions,
    deliverables: task.deliverables,
    worktree: task.worktree,
    branch: task.branch,
    parallelism: task.parallelism,
    requiresIndependentReview: task.requiresIndependentReview,
    requiresSecurityReview: task.requiresSecurityReview,
    humanGate: task.humanGate,
    candidateTaskId: task.candidateTaskId,
    maxAttempts: task.maxAttempts,
    createdAt: task.createdAt,
  };
  return createHash("sha256").update(canonicalJson(immutableEnvelope), "utf8").digest("hex");
}

function replaceTask(tasks: readonly TaskDefinition[], replacement: TaskDefinition): TaskDefinition[] {
  return tasks.map((task) => task.taskId === replacement.taskId ? replacement : task);
}

function transition(task: TaskDefinition, status: TaskStatus, at: string, result: AgentResult | null = task.result): TaskDefinition {
  assertTransition(task.taskId, task.status, status);
  return {
    ...task,
    status,
    startedAt: status === "ASSIGNED" && task.startedAt === null ? at : task.startedAt,
    finishedAt: ["PASS", "IMPLEMENTED", "FAIL", "BLOCKED", "HUMAN_REVIEW_REQUIRED", "CANCELLED"].includes(status) ? at : task.finishedAt,
    result,
  };
}

function pathWithin(path: string, scope: string): boolean {
  return path === scope || path.startsWith(`${scope}/`);
}

function assertResultScope(task: TaskDefinition, changedFiles: readonly string[]): void {
  if (task.taskKind !== "IMPLEMENTATION" && changedFiles.length > 0) {
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", `Read-only task '${task.taskId}' reported file changes.`, task.taskId);
  }
  for (const path of changedFiles) {
    assertRepositoryPath(path, "changed file");
    if (!task.allowedPaths.some((scope) => pathWithin(path, scope)) || task.forbiddenPaths.some((scope) => pathWithin(path, scope))) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", `Task '${task.taskId}' changed '${path}' outside its envelope.`, task.taskId);
    }
  }
}

function sanitiseCommand(command: CommandEvidence): CommandEvidence {
  return { ...command, relevantOutput: [] };
}

function digests(label: string, values: readonly string[]): string[] {
  return values.map((value) => `${label}:sha256:${createHash("sha256").update(value, "utf8").digest("hex")}`);
}

function decisionReferences(prefix: "finding" | "risk", values: readonly string[]): string[] {
  return values.filter((value) => {
    if (prefix === "finding") {
      const match = /^finding:[A-Za-z0-9][A-Za-z0-9._-]{0,63}\|severity:P[0-3]\|location:([^|:]+(?:\/[^|:]+)*):[1-9][0-9]{0,6}\|summary:[A-Za-z0-9][A-Za-z0-9 .,_()\/-]{0,199}$/.exec(value);
      if (match === null) return false;
      try { assertRepositoryPath(match[1] ?? "", "finding location"); return true; } catch { return false; }
    }
    return /^risk:[A-Za-z0-9][A-Za-z0-9._-]{0,63}\|severity:(?:LOW|MEDIUM|HIGH|CRITICAL)\|summary:[A-Za-z0-9][A-Za-z0-9 .,_()\/-]{0,199}\|mitigation:[A-Za-z0-9][A-Za-z0-9 .,_()\/-]{0,199}$/.test(value);
  });
}

export function taskExecutionLease(runId: string, taskId: string): string {
  return `orchestrator-attempt:${createHash("sha256").update(`${runId}\u0000${taskId}`, "utf8").digest("hex")}`;
}

const recoverableIntermediateStatuses: readonly TaskStatus[] = [
  "ASSIGNED", "RUNNING", "TESTING", "REVIEW", "INTEGRATION_READY", "INTEGRATING", "VALIDATING",
];

function isRecoverableIntermediate(task: TaskDefinition): boolean {
  return recoverableIntermediateStatuses.includes(task.status);
}

function taskResources(runId: string, task: TaskDefinition): readonly string[] {
  return [taskExecutionLease(runId, task.taskId), ...task.sharedResources];
}

function retryBoundaryIsComplete(state: PersistedRunState, task: TaskDefinition, attempt: AttemptRecord): boolean {
  const attempts = state.attempts.filter((entry) => entry.taskId === task.taskId);
  return attempts.at(-1)?.attemptId === attempt.attemptId && attempt.finishedAt !== null && attempt.result === null &&
    attempt.retryClass !== null && mayRetry(attempt.retryClass, attempts.length, task.maxAttempts);
}

function resetRecoveredTask(task: TaskDefinition): TaskDefinition {
  return { ...task, status: "READY", startedAt: null, finishedAt: null, result: null };
}

function sanitiseAgentResult(result: AgentResult, changedFiles: readonly string[] = []): AgentResult {
  return {
    schemaVersion: 1,
    status: result.status,
    summary: `A bounded ${result.status.toLowerCase()} result was accepted.`,
    changedFiles,
    commands: result.commands.map(sanitiseCommand),
    tests: result.tests.map(sanitiseCommand),
    evidence: [...decisionReferences("finding", result.evidence), ...digests("evidence", result.evidence), ...(result.status === "PASS" ? ["structured-result-accepted"] : [])],
    risks: [...decisionReferences("risk", result.risks), ...digests("risk", result.risks)],
    blockers: result.status === "BLOCKED" ? ["Execution stopped at a governed boundary."] : [],
    requestedAuthority: result.status === "BLOCKED" ? ["A separate authority may be required."] : [],
    stopCondition: result.stopCondition,
  };
}

function deterministicResult(summary: string, commands: readonly CommandEvidence[] = [], tests: readonly CommandEvidence[] = []): AgentResult {
  return {
    schemaVersion: 1,
    status: "PASS",
    summary,
    changedFiles: [],
    commands: commands.map(sanitiseCommand),
    tests: tests.map(sanitiseCommand),
    evidence: ["coordinator-observed-evidence"],
    risks: [],
    blockers: [],
    requestedAuthority: [],
    stopCondition: null,
  };
}

function samePaths(left: readonly string[], right: readonly string[]): boolean {
  return [...left].sort().join("\u0000") === [...right].sort().join("\u0000");
}

function sameAbsolutePath(left: string, right: string): boolean {
  const normalise = (value: string): string => process.platform === "win32" ? resolve(value).toLowerCase() : resolve(value);
  return normalise(left) === normalise(right);
}

function candidateTask(state: PersistedRunState, task: TaskDefinition): TaskDefinition {
  if (task.candidateTaskId === null) {
    throw new OrchestratorStop("AMBIGUOUS_AUTHORITY", `Task '${task.taskId}' has no candidate binding.`, task.taskId);
  }
  return new DependencyGraph(state.tasks).task(task.candidateTaskId);
}

function checkpointCandidateCommit(state: PersistedRunState, task: TaskDefinition): string | null {
  return ["INDEPENDENT_REVIEW", "SECURITY_REVIEW"].includes(task.taskKind)
    ? candidateTask(state, task).candidate?.commitId ?? null
    : null;
}

export class Coordinator {
  private stateMutationTail: Promise<void> = Promise.resolve();

  public constructor(
    private readonly runner: AgentRunner,
    private readonly stateStore: StateStore,
    private readonly resourceLocks: ResourceLocks,
    private readonly events: EventSink,
    private readonly worktrees: WorktreeManager,
    private readonly candidateInspector: CandidateInspector,
    private readonly integration: IntegrationExecutor,
    private readonly qualityGate: QualityGate,
    private readonly checkpoints: ThreadCheckpointStore,
    private readonly repositoryRoot: string,
    private readonly agentTimeoutMs = 300_000,
    private readonly clock: Clock = systemClock,
    private readonly ids: IdSource = randomIds,
    private readonly agentTerminationGraceMs = 5_000,
  ) {}

  public async start(plan: ProjectPlan, signal?: AbortSignal): Promise<PersistedRunState> {
    validatePlanSemantics(plan, this.repositoryRoot);
    const now = this.clock.now();
    let state: PersistedRunState = {
      schemaVersion: 1,
      runId: this.ids.runId(),
      revision: 0,
      baseline: plan.baseline,
      maxConcurrency: plan.maxConcurrency,
      createdAt: now,
      updatedAt: now,
      tasks: plan.tasks,
      attempts: [],
      heldLocks: [],
      humanGateReached: false,
    };
    await this.stateStore.save(state);
    return await this.execute(state, signal);
  }

  public async resume(runId: string, maximumConcurrency: number, signal?: AbortSignal, reconcileAbsentLocks = false): Promise<PersistedRunState> {
    let state = await this.stateStore.load(runId);
    validatePersistedStateSemantics(state, this.repositoryRoot);
    if (!Number.isInteger(maximumConcurrency) || maximumConcurrency < 1 || maximumConcurrency > 3 || maximumConcurrency !== state.maxConcurrency) {
      throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", "Resume concurrency must equal the persisted bounded value from 1 to 3.");
    }
    const preparedCheckpointRemovals = await this.checkpoints.inspectPreparedRemovals(runId);
    for (const prepared of preparedCheckpointRemovals) {
      const task = state.tasks.find((entry) => entry.taskId === prepared.taskId);
      const attempt = state.attempts.find((entry) => entry.taskId === prepared.taskId && entry.attemptId === prepared.attemptId);
      if (task === undefined || attempt?.finishedAt === null || attempt === undefined || !this.checkpointMatches(state, task, prepared)) {
        throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Task '${prepared.taskId}' has an unproved checkpoint removal.`, prepared.taskId);
      }
    }
    for (const prepared of preparedCheckpointRemovals) {
      await this.checkpoints.finalisePreparedRemoval(prepared);
    }
    const checkpoints = await this.checkpoints.inspect(runId);
    const checkpointByTask = new Map(checkpoints.map((checkpoint) => [checkpoint.taskId, checkpoint]));
    for (const checkpoint of checkpoints) {
      const task = state.tasks.find((entry) => entry.taskId === checkpoint.taskId);
      if (task === undefined || !this.checkpointMatches(state, task, checkpoint)) {
        throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Task '${checkpoint.taskId}' has a checkpoint outside its immutable execution envelope.`, checkpoint.taskId);
      }
    }
    const intermediate = state.tasks.filter(isRecoverableIntermediate);
    const interruptedDeterministic = intermediate.find((task) => ["INTEGRATION", "QUALITY_GATE", "HUMAN_GATE"].includes(task.taskKind));
    if (interruptedDeterministic !== undefined) {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Task '${interruptedDeterministic.taskId}' was interrupted outside a recoverable agent boundary.`, interruptedDeterministic.taskId);
    }
    const lockRecords = await this.resourceLocks.inspectRecords();
    if (state.heldLocks.length > 0 || lockRecords.length > 0) {
      if (!reconcileAbsentLocks) {
        throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", "Recovered state records held locks; explicit absent-owner reconciliation is required.");
      }
      const reconciliations = lockRecords.map((record) => {
        const persistedTask = record.taskId === null ? undefined : state.tasks.find((task) => task.taskId === record.taskId);
        const checkpoint = record.taskId === null ? undefined : checkpointByTask.get(record.taskId);
        const terminalAttempt = record.attemptId === null || record.taskId === null ? undefined : state.attempts.find((attempt) =>
          attempt.taskId === record.taskId && attempt.attemptId === record.attemptId && attempt.finishedAt !== null);
        const terminalOwnership = persistedTask !== undefined && terminalAttempt !== undefined &&
          (!isRecoverableIntermediate(persistedTask) || retryBoundaryIsComplete(state, persistedTask, terminalAttempt));
        const checkpointOwnership = checkpoint !== undefined && record.attemptId === checkpoint.attemptId;
        const authorisedResource = persistedTask !== undefined && record.resource !== null && taskResources(state.runId, persistedTask).includes(record.resource);
        if (record.resource === null || record.taskId === null || record.attemptId === null || record.acquiredAt === null || record.recordDigest === null ||
            !["OWNER_PROCESS_ABSENT", "RELEASE_PREPARED", "RECONCILE_PREPARED"].includes(record.status) || record.runId !== state.runId ||
            !authorisedResource || (!checkpointOwnership && !terminalOwnership)) {
          throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", "A recovered lock is active, invalid or foreign and cannot be reconciled automatically.", record.taskId);
        }
        return { lockId: record.lockId, resource: record.resource, status: record.status, recordDigest: record.recordDigest, owner: {
          runId: state.runId,
          taskId: record.taskId,
          attemptId: record.attemptId,
          acquiredAt: record.acquiredAt,
        } };
      });
      const expectedLocks = [...state.heldLocks].sort();
      const actualLocks = reconciliations.map((entry) => entry.resource).sort();
      if (new Set(expectedLocks).size !== expectedLocks.length || new Set(actualLocks).size !== actualLocks.length ||
          expectedLocks.some((resource) => !actualLocks.includes(resource))) {
        throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", "Persisted lock identities are not represented by the physical lock records.");
      }
      for (const reconciliation of reconciliations) {
        const prepared = reconciliation.status === "OWNER_PROCESS_ABSENT"
          ? await this.resourceLocks.reconcileAbsentOwner(reconciliation.lockId, reconciliation.owner)
          : {
              lockId: reconciliation.lockId,
              resource: reconciliation.resource,
              action: reconciliation.status === "RELEASE_PREPARED" ? "RELEASE" as const : "RECONCILE" as const,
              recordDigest: reconciliation.recordDigest,
            };
        if (state.heldLocks.includes(reconciliation.resource)) {
          state = await this.persist({ ...state, heldLocks: state.heldLocks.filter((resource) => resource !== reconciliation.resource) });
        }
        await this.resourceLocks.finalise(prepared, reconciliation.owner);
      }
    }
    let recovered = state;
    if (intermediate.length > 0) {
      const interruptedAt = this.clock.now();
      const attempts = [...state.attempts];
      for (const task of intermediate) {
        const checkpoint = checkpointByTask.get(task.taskId);
        const taskAttemptIndexes = attempts.flatMap((attempt, index) => attempt.taskId === task.taskId ? [index] : []);
        const latestIndex = taskAttemptIndexes.at(-1);
        const latestAttempt = latestIndex === undefined ? undefined : attempts[latestIndex];
        const checkpointIndex = checkpoint === undefined
          ? -1
          : attempts.findIndex((attempt) => attempt.taskId === task.taskId && attempt.attemptId === checkpoint.attemptId);
        if (checkpoint !== undefined && checkpointIndex < 0) {
          throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Task '${task.taskId}' has a checkpoint outside its append-only attempt history.`, task.taskId);
        }
        if (checkpoint !== undefined) {
          const existing = attempts[checkpointIndex]!;
          if (checkpointIndex !== latestIndex) {
            throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Task '${task.taskId}' checkpoint does not identify its latest attempt.`, task.taskId);
          }
          if (existing.finishedAt === null && existing.result === null) {
            attempts[checkpointIndex] = {
              ...existing,
              finishedAt: interruptedAt,
              retryClass: "INTERRUPTED",
              threadId: existing.threadId ?? (checkpoint.threadId.startsWith("pending-") ? null : checkpoint.threadId),
            };
          } else if (!retryBoundaryIsComplete(state, task, existing)) {
            throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Task '${task.taskId}' has a completed attempt outside a retry boundary.`, task.taskId);
          }
          continue;
        }
        if (latestAttempt === undefined) {
          continue;
        }
        if (latestAttempt.finishedAt === null && latestAttempt.result === null) {
          const physicalOwnership = lockRecords.some((record) => record.taskId === task.taskId && record.attemptId === latestAttempt.attemptId);
          const persistedOwnership = taskResources(state.runId, task).some((resource) => state.heldLocks.includes(resource));
          if (latestAttempt.threadId !== null || physicalOwnership || persistedOwnership) {
            throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Task '${task.taskId}' has an unfinished attempt without its required pre-turn checkpoint.`, task.taskId);
          }
          attempts[latestIndex!] = { ...latestAttempt, finishedAt: interruptedAt, retryClass: "INTERRUPTED" };
        } else if (!retryBoundaryIsComplete(state, task, latestAttempt)) {
          throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Task '${task.taskId}' has no recoverable checkpoint or completed retry boundary.`, task.taskId);
        }
      }
      recovered = await this.persist({
        ...state,
        attempts,
        tasks: state.tasks.map((task) => intermediate.some((entry) => entry.taskId === task.taskId)
          ? resetRecoveredTask(task)
          : task),
      });
    }
    for (const checkpoint of checkpoints) {
      const task = recovered.tasks.find((entry) => entry.taskId === checkpoint.taskId);
      const interrupted = intermediate.some((entry) => entry.taskId === checkpoint.taskId);
      const recorded = recovered.attempts.some((attempt) => attempt.attemptId === checkpoint.attemptId && attempt.taskId === checkpoint.taskId);
      if (task !== undefined && (interrupted || recorded || !["READY", "ASSIGNED", "RUNNING"].includes(task.status))) {
        await this.checkpoints.remove(checkpoint);
      } else {
        throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Task '${checkpoint.taskId}' has a stale checkpoint that is not represented in recovery history.`, checkpoint.taskId);
      }
    }
    return await this.execute(recovered, signal);
  }

  private checkpointMatches(state: PersistedRunState, task: TaskDefinition, checkpoint: ThreadCheckpoint): boolean {
    const recordedAttempt = state.attempts.find((attempt) => attempt.taskId === checkpoint.taskId && attempt.attemptId === checkpoint.attemptId);
    const revisionMatches = checkpoint.stateRevision === state.revision || (recordedAttempt !== undefined && checkpoint.stateRevision < state.revision);
    const threadMatches = recordedAttempt !== undefined &&
      (checkpoint.threadId === `pending-${checkpoint.attemptId}` || checkpoint.threadId === recordedAttempt.threadId);
    return checkpoint.runId === state.runId && checkpoint.taskId === task.taskId && checkpoint.agentId === task.owner &&
      checkpoint.taskKind === task.taskKind && checkpoint.baseline === state.baseline &&
      checkpoint.candidateCommitId === checkpointCandidateCommit(state, task) && checkpoint.envelopeHash === taskEnvelopeHash(task) &&
      recordedAttempt?.startedAt === checkpoint.startedAt && threadMatches && revisionMatches && checkpoint.deadlineMs === this.agentTimeoutMs;
  }

  private async execute(initial: PersistedRunState, signal?: AbortSignal): Promise<PersistedRunState> {
    let state = initial;
    while (true) {
      state = this.promoteReadyTasks(state);
      const graph = new DependencyGraph(state.tasks);
      const groups = graph.groups();
      if (groups.failed.length > 0) {
        return state;
      }
      if (groups.completed.length === state.tasks.length) {
        this.events.write({ timestamp: this.clock.now(), event: "RUN_COMPLETED", runId: state.runId, taskId: null, agentId: null, attemptId: null, branchId: null, worktreeId: null, result: "PASS", stopCode: null, durationMs: null });
        return state;
      }
      if (signal?.aborted === true) {
        throw new OrchestratorStop("TEST_BASELINE_BROKEN", "The orchestrator execution was interrupted.");
      }
      const wave = scheduleWave(state.tasks, state.maxConcurrency).tasks;
      if (wave.length === 0) {
        if (groups.blocked.length > 0) {
          return state;
        }
        throw new OrchestratorStop("AMBIGUOUS_AUTHORITY", "No task can make progress from the persisted state.");
      }
      const humanGate = wave.find((task) => task.taskKind === "HUMAN_GATE");
      if (humanGate !== undefined) {
        let gated = transition(humanGate, "ASSIGNED", this.clock.now());
        gated = transition(gated, "RUNNING", this.clock.now());
        gated = transition(gated, "HUMAN_REVIEW_REQUIRED", this.clock.now());
        state = await this.persist({ ...state, tasks: replaceTask(state.tasks, gated), humanGateReached: true });
        this.events.write({ timestamp: this.clock.now(), event: "HUMAN_GATE_REACHED", runId: state.runId, taskId: gated.taskId, agentId: gated.owner, attemptId: null, branchId: opaqueLocationId(gated.branch), worktreeId: opaqueLocationId(gated.worktree), result: "HUMAN_REVIEW_REQUIRED", stopCode: "HUMAN_GATE_REQUIRED", durationMs: null });
        return state;
      }
      for (const task of wave) {
        let assigned = transition(task, "ASSIGNED", this.clock.now());
        assigned = transition(assigned, "RUNNING", this.clock.now());
        state = { ...state, tasks: replaceTask(state.tasks, assigned) };
      }
      state = await this.persist(state);
      const results = await Promise.allSettled(wave.map(async (task) => await this.runTask(state, task.taskId, signal)));
      state = await this.stateStore.load(state.runId);
      for (const result of results) {
        if (result.status === "fulfilled") {
          const checkpoint = await this.checkpoints.load(state.runId, result.value.taskId);
          if (checkpoint?.attemptId === result.value.attemptId) await this.checkpoints.remove(checkpoint);
        }
      }
      const rejected = results.find((result): result is PromiseRejectedResult => result.status === "rejected");
      if (rejected !== undefined) throw rejected.reason;
    }
  }

  private promoteReadyTasks(state: PersistedRunState): PersistedRunState {
    const graph = new DependencyGraph(state.tasks);
    let tasks = [...state.tasks];
    for (const task of graph.groups().ready) {
      if (task.status === "DISCOVERED") {
        tasks = replaceTask(tasks, transition(task, "READY", this.clock.now()));
      }
    }
    return { ...state, tasks };
  }

  private async runTask(state: PersistedRunState, taskId: string, signal?: AbortSignal): Promise<{ taskId: string; attemptId: string }> {
    let task = new DependencyGraph(state.tasks).task(taskId);
    let recoveredCheckpoint = await this.checkpoints.load(state.runId, task.taskId);
    const previousAttemptCount = state.attempts.filter((attempt) => attempt.taskId === task.taskId).length;
    if (previousAttemptCount >= task.maxAttempts) {
      const exhaustedResult: AgentResult = {
        schemaVersion: 1, status: "BLOCKED", summary: "The cumulative attempt budget was exhausted before another execution could start.",
        changedFiles: [], commands: [], tests: [], evidence: [], risks: [], blockers: ["No governed attempt remains."],
        requestedAuthority: ["A new task envelope with separate authority is required."], stopCondition: "TEST_BASELINE_BROKEN",
      };
      task = transition(task, "BLOCKED", this.clock.now(), exhaustedResult);
      await this.mutatePersisted(state.runId, (current) => ({ ...current, tasks: replaceTask(current.tasks, task) }));
      return { taskId: task.taskId, attemptId: "" };
    }
    const resources = [taskExecutionLease(state.runId, task.taskId), ...task.sharedResources];
    for (let attemptNumber = 1; attemptNumber <= task.maxAttempts - previousAttemptCount; attemptNumber += 1) {
      const cumulativeAttemptNumber = previousAttemptCount + attemptNumber;
      const attemptId = recoveredCheckpoint?.attemptId ?? this.ids.attemptId(task.taskId, previousAttemptCount + attemptNumber);
      const startedAt = recoveredCheckpoint?.startedAt ?? this.clock.now();
      const attemptState = await this.mutatePersisted(state.runId, (current) => {
        if (current.attempts.some((attempt) => attempt.attemptId === attemptId)) {
          throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Attempt identity '${attemptId}' is not unique.`, task.taskId);
        }
        const started: AttemptRecord = { attemptId, taskId, agentId: task.owner, startedAt, finishedAt: null, retryClass: null, threadId: null, result: null };
        return { ...current, attempts: [...current.attempts, started] };
      });
      if (!["INTEGRATION", "QUALITY_GATE"].includes(task.taskKind)) {
        await this.checkpoints.save({
          schemaVersion: 1, runId: state.runId, taskId, attemptId, agentId: task.owner, taskKind: task.taskKind,
          baseline: state.baseline, candidateCommitId: checkpointCandidateCommit(state, task), envelopeHash: taskEnvelopeHash(task),
          stateRevision: attemptState.revision, deadlineMs: this.agentTimeoutMs, threadId: `pending-${attemptId}`, startedAt,
        });
      }
      this.events.write({ timestamp: startedAt, event: "TASK_ASSIGNED", runId: state.runId, taskId, agentId: task.owner, attemptId, branchId: opaqueLocationId(task.branch), worktreeId: opaqueLocationId(task.worktree), result: null, stopCode: null, durationMs: null });
      const acquired: { resource: string; owner: { runId: string; taskId: string; attemptId: string; acquiredAt: string } }[] = [];
      let releaseResources = true;
      try {
        for (const resource of resources) {
          const owner = { runId: state.runId, taskId, attemptId, acquiredAt: this.clock.now() };
          await this.resourceLocks.acquire(resource, owner);
          acquired.push({ resource, owner });
          await this.mutatePersisted(state.runId, (current) => ({ ...current, heldLocks: [...new Set([...current.heldLocks, resource])].sort() }));
        }
        const completed = ["INTEGRATION", "QUALITY_GATE"].includes(task.taskKind)
          ? await this.runDeterministicTask(state, task, signal)
          : await this.runAgentTask(state, task, attemptId, signal);
        task = completed.task;
        const finishedAt = this.clock.now();
        await this.finishAttempt(state.runId, attemptId, { finishedAt, retryClass: null, threadId: completed.threadId, result: task.result }, task);
        this.events.write({ timestamp: finishedAt, event: "TASK_COMPLETED", runId: state.runId, taskId, agentId: task.owner, attemptId, branchId: opaqueLocationId(task.branch), worktreeId: opaqueLocationId(task.worktree), result: task.result?.status ?? "PASS", stopCode: task.result?.stopCondition ?? null, durationMs: Date.parse(finishedAt) - Date.parse(startedAt) });
        return { taskId: task.taskId, attemptId };
      } catch (error) {
        if (error instanceof AgentTerminationUnconfirmed) {
          releaseResources = false;
          throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", "Agent termination was not confirmed; checkpoint and locks were preserved for explicit reconciliation.", task.taskId);
        }
        const finishedAt = this.clock.now();
        const cancelled = error instanceof AgentExecutionCancelled || signal?.aborted === true;
        const retryClass = classifyAttemptFailure(error, task, cancelled);
        const checkpoint = await this.checkpoints.load(state.runId, task.taskId);
        const persistedAttempt = (await this.stateStore.load(state.runId)).attempts.find((entry) => entry.taskId === task.taskId && entry.attemptId === attemptId);
        const checkpointThreadId = checkpoint === null || checkpoint.threadId.startsWith("pending-") ? null : checkpoint.threadId;
        if (persistedAttempt === undefined || (persistedAttempt.threadId !== null && checkpointThreadId !== null && persistedAttempt.threadId !== checkpointThreadId)) {
          throw new OrchestratorStop("TEST_BASELINE_BROKEN", "The failed attempt lost its persisted thread identity.", task.taskId);
        }
        const completedThreadId = persistedAttempt.threadId ?? checkpointThreadId;
        if (cancelled) {
          const cancelledResult: AgentResult = {
            schemaVersion: 1, status: "BLOCKED", summary: "Execution was cancelled by the coordinator before trusted evidence was accepted.",
            changedFiles: [], commands: [], tests: [], evidence: ["execution-outcome:CANCELLED"], risks: [], blockers: ["The execution was cancelled."],
            requestedAuthority: [], stopCondition: "TEST_BASELINE_BROKEN",
          };
          task = transition(task, "CANCELLED", finishedAt, cancelledResult);
          await this.finishAttempt(state.runId, attemptId, { finishedAt, retryClass, threadId: completedThreadId, result: null }, task);
          return { taskId: task.taskId, attemptId };
        }
        if (mayRetry(retryClass, cumulativeAttemptNumber, task.maxAttempts)) {
          await this.finishAttempt(
            state.runId,
            attemptId,
            { finishedAt, retryClass, threadId: completedThreadId, result: null },
            resetRecoveredTask(task),
          );
          if (checkpoint !== null) await this.checkpoints.remove(checkpoint);
          return { taskId: task.taskId, attemptId };
        }
        const stop = error instanceof OrchestratorStop
          ? error
          : error instanceof AgentDeadlineExceeded
            ? new OrchestratorStop("TEST_BASELINE_BROKEN", "The bounded agent deadline expired.", task.taskId)
            : new OrchestratorStop("TEST_BASELINE_BROKEN", errorMessage(error), task.taskId);
        const timedOut = error instanceof AgentDeadlineExceeded;
        const blockedResult: AgentResult = {
          schemaVersion: 1, status: "BLOCKED", summary: timedOut ? "Execution exceeded its bounded coordinator deadline." : "Execution stopped before trusted evidence was accepted.",
          changedFiles: [], commands: [], tests: [], evidence: timedOut ? ["execution-outcome:TIMED_OUT"] : [], risks: [], blockers: ["A governed boundary stopped execution."],
          requestedAuthority: ["Separate reconciliation or authority is required."], stopCondition: stop.code,
        };
        task = transition(task, "BLOCKED", finishedAt, blockedResult);
        await this.finishAttempt(state.runId, attemptId, { finishedAt, retryClass, threadId: completedThreadId, result: null }, task);
        return { taskId: task.taskId, attemptId };
      } finally {
        if (releaseResources) {
          for (const acquiredResource of acquired.reverse()) {
            const prepared = await this.resourceLocks.release(acquiredResource.resource, state.runId, attemptId);
            await this.mutatePersisted(state.runId, (current) => ({ ...current, heldLocks: current.heldLocks.filter((entry) => entry !== acquiredResource.resource) }));
            await this.resourceLocks.finalise(prepared, acquiredResource.owner);
          }
        }
      }
    }
    throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Task '${task.taskId}' exhausted its attempt loop without a result.`, task.taskId);
  }

  private async runAgentTask(state: PersistedRunState, task: TaskDefinition, attemptId: string, signal?: AbortSignal): Promise<{ task: TaskDefinition; threadId: string | null }> {
    const checkpoint = await this.checkpoints.load(state.runId, task.taskId);
    if (task.taskKind === "IMPLEMENTATION") {
      if (task.worktree === null || task.branch === null) {
        throw new OrchestratorStop("AMBIGUOUS_AUTHORITY", "Implementation execution requires a bound worktree and branch.", task.taskId);
      }
      const existing = (await this.worktrees.list()).find((record) => record.path === task.worktree || record.branch === task.branch);
      if (existing === undefined) await this.worktrees.create(task.taskId, task.worktree, task.branch, state.baseline);
      else await this.worktrees.validate(task.taskId, task.worktree, task.branch, state.baseline);
    }
    const boundCandidate = ["INDEPENDENT_REVIEW", "SECURITY_REVIEW"].includes(task.taskKind) ? candidateTask(state, task).candidate : null;
    if (["INDEPENDENT_REVIEW", "SECURITY_REVIEW"].includes(task.taskKind) && boundCandidate === null) {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Review execution requires trusted candidate evidence.", task.taskId);
    }
    const previousThread = checkpoint !== null && !checkpoint.threadId.startsWith("pending-")
      ? checkpoint.threadId
      : [...state.attempts].reverse().find((attempt) => attempt.taskId === task.taskId && attempt.threadId !== null)?.threadId ?? null;
    const response = await this.runAgentWithDeadline({
      runId: state.runId,
      attemptId,
      task,
      baseline: state.baseline,
      contracts: task.requiredContracts,
      candidate: boundCandidate,
      resumeThreadId: previousThread,
      checkpointThread: async (threadId) => {
        if (threadId.startsWith("pending-")) {
          throw new OrchestratorStop("TEST_BASELINE_BROKEN", "The runner returned a reserved checkpoint placeholder as a real thread identity.", task.taskId);
        }
        await this.mutatePersisted(state.runId, (current) => {
          const attempt = current.attempts.find((entry) => entry.attemptId === attemptId && entry.taskId === task.taskId);
          if (attempt === undefined || attempt.finishedAt !== null || (attempt.threadId !== null && attempt.threadId !== threadId)) {
            throw new OrchestratorStop("TEST_BASELINE_BROKEN", "The runner thread identity conflicts with the persisted attempt.", task.taskId);
          }
          return { ...current, attempts: current.attempts.map((entry) => entry.attemptId === attemptId ? { ...entry, threadId } : entry) };
        });
      },
    }, signal);
    if (response.result.status !== "PASS") {
      const accepted = sanitiseAgentResult(response.result);
      const terminal = response.result.status === "FAIL" ? "FAIL" : response.result.status === "BLOCKED" ? "BLOCKED" : "HUMAN_REVIEW_REQUIRED";
      return { task: transition(task, terminal, this.clock.now(), accepted), threadId: response.threadId };
    }
    if (task.taskKind === "IMPLEMENTATION") {
      const focusedTests = await this.qualityGate.run(task.worktree ?? "", signal);
      if (focusedTests.result !== "PASS") {
        throw new OrchestratorStop("TEST_BASELINE_BROKEN", "The coordinator-owned candidate test gate did not pass.", task.taskId);
      }
      const candidate = await this.candidateInspector.inspect(task, state.baseline, response.result);
      if (!samePaths(candidate.changedFiles, response.result.changedFiles)) {
        throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "Agent-reported files do not match the trusted Git diff.", task.taskId);
      }
      assertResultScope(task, candidate.changedFiles);
      const accepted = { ...sanitiseAgentResult(response.result, candidate.changedFiles), tests: [sanitiseCommand(focusedTests)] };
      return { task: transition({ ...task, candidate }, "IMPLEMENTED", this.clock.now(), accepted), threadId: response.threadId };
    }
    assertResultScope(task, response.result.changedFiles);
    const accepted = sanitiseAgentResult(response.result);
    let passed = transition(task, task.taskKind === "DISCOVERY" ? "VALIDATING" : "REVIEW", this.clock.now(), accepted);
    passed = transition(passed, "PASS", this.clock.now(), accepted);
    return { task: passed, threadId: response.threadId };
  }

  private async runDeterministicTask(state: PersistedRunState, task: TaskDefinition, signal?: AbortSignal): Promise<{ task: TaskDefinition; threadId: null }> {
    if (task.taskKind === "QUALITY_GATE") {
      if (!sameAbsolutePath(task.executionSurface.cwd, this.repositoryRoot)) {
        throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The quality gate cwd differs from the coordinator repository root.", task.taskId);
      }
      let current = transition(task, "TESTING", this.clock.now());
      const evidence = await this.qualityGate.run(task.executionSurface.cwd, signal);
      if (evidence.result !== "PASS") {
        throw new OrchestratorStop("TEST_BASELINE_BROKEN", "The deterministic quality gate did not pass.", task.taskId);
      }
      const result = deterministicResult("The coordinator observed the canonical offline quality gate pass.", [], [evidence]);
      current = transition(current, "VALIDATING", this.clock.now(), result);
      current = transition(current, "PASS", this.clock.now(), result);
      return { task: current, threadId: null };
    }
    const implementation = candidateTask(state, task);
    if (!sameAbsolutePath(task.executionSurface.cwd, this.repositoryRoot)) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The integration cwd differs from the coordinator repository root.", task.taskId);
    }
    if (implementation.candidate === null || implementation.result?.status !== "PASS") {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Integration requires a trusted implemented candidate.", task.taskId);
    }
    const reviews = state.tasks.filter((entry) => entry.candidateTaskId === implementation.taskId);
    const independentReview = reviews.find((entry) => entry.taskKind === "INDEPENDENT_REVIEW")?.result ?? null;
    const securityReview = reviews.find((entry) => entry.taskKind === "SECURITY_REVIEW")?.result ?? null;
    let current = transition(task, "INTEGRATION_READY", this.clock.now());
    current = transition(current, "INTEGRATING", this.clock.now());
    const orderedIntegrations = state.tasks.filter((entry) => entry.taskKind === "INTEGRATION")
      .sort((left, right) => right.priority - left.priority || left.taskId.localeCompare(right.taskId));
    const integrationIndex = orderedIntegrations.findIndex((entry) => entry.taskId === task.taskId);
    const predecessor = integrationIndex > 0 ? orderedIntegrations[integrationIndex - 1] : undefined;
    const expectedCoordinatorHead = predecessor?.candidate?.commitId ?? state.baseline;
    const outcome = await this.integration.integrate({ baseline: state.baseline, expectedCoordinatorHead, integrationTask: task, implementationTask: implementation, candidate: implementation.candidate, workerResult: implementation.result, independentReview, securityReview }, signal);
    if (outcome.evidence.result !== "PASS") {
      throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", "Sequential integration did not pass.", task.taskId);
    }
    const result = deterministicResult("The coordinator integrated and tested the reviewed candidate sequentially.", [outcome.evidence], outcome.tests);
    current = transition({ ...current, candidate: outcome.candidate }, "VALIDATING", this.clock.now(), result);
    current = transition(current, "PASS", this.clock.now(), result);
    return { task: current, threadId: null };
  }

  private async persist(state: PersistedRunState): Promise<PersistedRunState> {
    const next = { ...state, revision: state.revision + 1, updatedAt: this.clock.now() };
    await this.stateStore.save(next);
    return next;
  }

  private async mutatePersisted(runId: string, mutate: (state: PersistedRunState) => PersistedRunState): Promise<PersistedRunState> {
    let updated!: PersistedRunState;
    const operation = this.stateMutationTail.then(async () => {
      const current = await this.stateStore.load(runId);
      updated = await this.persist(mutate(current));
    });
    this.stateMutationTail = operation.then(() => undefined, () => undefined);
    await operation;
    return updated;
  }

  private async finishAttempt(
    runId: string,
    attemptId: string,
    update: Pick<AttemptRecord, "finishedAt" | "retryClass" | "threadId" | "result">,
    task?: TaskDefinition,
  ): Promise<void> {
    await this.mutatePersisted(runId, (current) => {
      const matches = current.attempts.filter((attempt) => attempt.attemptId === attemptId);
      if (matches.length !== 1 || matches[0]?.finishedAt !== null) {
        throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Attempt '${attemptId}' cannot be completed from its persisted state.`, task?.taskId);
      }
      const attempts = current.attempts.map((attempt) => attempt.attemptId === attemptId ? { ...attempt, ...update } : attempt);
      return { ...current, attempts, tasks: task === undefined ? current.tasks : replaceTask(current.tasks, task) };
    });
  }

  private async runAgentWithDeadline(request: Parameters<AgentRunner["run"]>[0], signal?: AbortSignal) {
    if (!Number.isInteger(this.agentTimeoutMs) || this.agentTimeoutMs < 1 || this.agentTimeoutMs > 7_200_000) {
      throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", "Agent deadline is outside the supported bound.", request.task.taskId);
    }
    if (!Number.isInteger(this.agentTerminationGraceMs) || this.agentTerminationGraceMs < 1 || this.agentTerminationGraceMs > 60_000) {
      throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", "Agent termination grace is outside the supported bound.", request.task.taskId);
    }
    const controller = new AbortController();
    type RunnerOutcome = { readonly kind: "PASS"; readonly value: Awaited<ReturnType<AgentRunner["run"]>> } | { readonly kind: "FAIL"; readonly error: unknown };
    const runnerOutcome: Promise<RunnerOutcome> = this.runner.run(request, controller.signal).then(
      (value) => ({ kind: "PASS" as const, value }),
      (error: unknown) => ({ kind: "FAIL" as const, error }),
    );
    let triggerAbort!: (reason: "TIMEOUT" | "CANCELLED") => void;
    const abortTrigger = new Promise<"TIMEOUT" | "CANCELLED">((resolveAbort) => { triggerAbort = resolveAbort; });
    const cancel = (): void => triggerAbort("CANCELLED");
    signal?.addEventListener("abort", cancel, { once: true });
    const timer = setTimeout(() => triggerAbort("TIMEOUT"), this.agentTimeoutMs);
    if (signal?.aborted === true) triggerAbort("CANCELLED");
    try {
      const first = await Promise.race([runnerOutcome, abortTrigger]);
      if (typeof first !== "string") {
        if (first.kind === "FAIL") throw first.error;
        return first.value;
      }
      controller.abort();
      let graceTimer: ReturnType<typeof setTimeout> | null = null;
      const settled = await Promise.race([
        runnerOutcome,
        new Promise<"UNCONFIRMED">((resolveGrace) => { graceTimer = setTimeout(() => resolveGrace("UNCONFIRMED"), this.agentTerminationGraceMs); }),
      ]);
      if (graceTimer !== null) clearTimeout(graceTimer);
      if (settled === "UNCONFIRMED") throw new AgentTerminationUnconfirmed();
      if (first === "CANCELLED") throw new AgentExecutionCancelled();
      throw new AgentDeadlineExceeded("The agent deadline expired.");
    } finally {
      clearTimeout(timer);
      signal?.removeEventListener("abort", cancel);
    }
  }
}
