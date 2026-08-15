// Purpose: Coordinates bounded task waves, resource ownership, retries, persisted evidence and external Human Gate stops.
import { randomUUID } from "node:crypto";
import type {
  AgentResult,
  AgentRunner,
  AttemptRecord,
  PersistedRunState,
  ProjectPlan,
  TaskDefinition,
  TaskStatus,
} from "../core/contracts.js";
import { DependencyGraph } from "../core/dependency-graph.js";
import { errorMessage, OrchestratorStop } from "../core/errors.js";
import { ClassifiedFailure, mayRetry } from "../core/retry.js";
import { scheduleWave } from "../core/scheduler.js";
import { assertTransition } from "../core/state-machine.js";
import { assertRepositoryPath } from "../core/validation.js";
import type { EventSink } from "../observability/structured-log.js";
import { opaqueLocationId } from "../observability/structured-log.js";
import type { ResourceLocks } from "../ports/resource-locks.js";
import type { StateStore } from "../ports/state-store.js";
import { validatePlanSemantics } from "./plan.js";

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

function replaceTask(tasks: readonly TaskDefinition[], replacement: TaskDefinition): TaskDefinition[] {
  return tasks.map((task) => task.taskId === replacement.taskId ? replacement : task);
}

function transition(task: TaskDefinition, status: TaskStatus, at: string, result: AgentResult | null = task.result): TaskDefinition {
  assertTransition(task.taskId, task.status, status);
  return {
    ...task,
    status,
    startedAt: status === "ASSIGNED" && task.startedAt === null ? at : task.startedAt,
    finishedAt: ["PASS", "FAIL", "BLOCKED", "HUMAN_REVIEW_REQUIRED", "CANCELLED"].includes(status) ? at : task.finishedAt,
    result,
  };
}

function passPipeline(task: TaskDefinition, at: string, result: AgentResult): TaskDefinition {
  let current = transition(task, task.owner === "implementation_worker" ? "IMPLEMENTED" : "REVIEW", at, result);
  if (current.status === "IMPLEMENTED") {
    current = transition(current, "TESTING", at, result);
    current = transition(current, "REVIEW", at, result);
  }
  current = transition(current, "INTEGRATION_READY", at, result);
  current = transition(current, "INTEGRATING", at, result);
  current = transition(current, "VALIDATING", at, result);
  return transition(current, "PASS", at, result);
}

function pathWithin(path: string, scope: string): boolean {
  return path === scope || path.startsWith(`${scope}/`);
}

function assertResultScope(task: TaskDefinition, result: AgentResult): void {
  if (task.owner !== "implementation_worker" && result.changedFiles.length > 0) {
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", `Read-only agent '${task.owner}' reported file changes.`, task.taskId);
  }
  for (const path of result.changedFiles) {
    assertRepositoryPath(path, "changed file");
    if (!task.allowedPaths.some((scope) => pathWithin(path, scope)) || task.forbiddenPaths.some((scope) => pathWithin(path, scope))) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", `Task '${task.taskId}' changed '${path}' outside its envelope.`, task.taskId);
    }
  }
}

export class Coordinator {
  public constructor(
    private readonly runner: AgentRunner,
    private readonly stateStore: StateStore,
    private readonly resourceLocks: ResourceLocks,
    private readonly events: EventSink,
    private readonly clock: Clock = systemClock,
    private readonly ids: IdSource = randomIds,
  ) {}

  public async start(plan: ProjectPlan, signal?: AbortSignal): Promise<PersistedRunState> {
    validatePlanSemantics(plan);
    const now = this.clock.now();
    let state: PersistedRunState = {
      schemaVersion: 1,
      runId: this.ids.runId(),
      revision: 0,
      baseline: plan.baseline,
      createdAt: now,
      updatedAt: now,
      tasks: plan.tasks,
      attempts: [],
      heldLocks: [],
      humanGateReached: false,
    };
    await this.stateStore.save(state);
    return await this.execute(state, plan.maxConcurrency, signal);
  }

  public async resume(runId: string, maximumConcurrency: number, signal?: AbortSignal): Promise<PersistedRunState> {
    const state = await this.stateStore.load(runId);
    const orphanLocks = await this.resourceLocks.inspect();
    if (state.heldLocks.length > 0 || orphanLocks.length > 0) {
      throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", "Recovered state records held locks; operator reconciliation is required.");
    }
    return await this.execute(state, maximumConcurrency, signal);
  }

  private async execute(initial: PersistedRunState, maximumConcurrency: number, signal?: AbortSignal): Promise<PersistedRunState> {
    let state = initial;
    while (true) {
      if (signal?.aborted === true) {
        throw new OrchestratorStop("TEST_BASELINE_BROKEN", "The orchestrator execution was interrupted.");
      }
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
      const wave = scheduleWave(state.tasks, maximumConcurrency).tasks;
      if (wave.length === 0) {
        if (groups.blocked.length > 0) {
          return state;
        }
        throw new OrchestratorStop("AMBIGUOUS_AUTHORITY", "No task can make progress from the persisted state.");
      }
      const humanGate = wave.find((task) => task.humanGate);
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
      state = await this.persist({
        ...state,
        heldLocks: [...new Set(wave.flatMap((task) => task.sharedResources))].sort(),
      });
      const results = await Promise.all(wave.map(async (task) => await this.runTask(state, task.taskId, signal)));
      for (const completed of results) {
        state = {
          ...state,
          tasks: replaceTask(state.tasks, completed.task),
          attempts: [...state.attempts, ...completed.attempts],
          heldLocks: [],
        };
      }
      state = await this.persist(state);
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

  private async runTask(state: PersistedRunState, taskId: string, signal?: AbortSignal): Promise<{ task: TaskDefinition; attempts: AttemptRecord[] }> {
    let task = new DependencyGraph(state.tasks).task(taskId);
    const attempts: AttemptRecord[] = [];
    for (let attemptNumber = 1; attemptNumber <= task.maxAttempts; attemptNumber += 1) {
      const attemptId = this.ids.attemptId(task.taskId, attemptNumber);
      const startedAt = this.clock.now();
      this.events.write({ timestamp: startedAt, event: "TASK_ASSIGNED", runId: state.runId, taskId, agentId: task.owner, attemptId, branchId: opaqueLocationId(task.branch), worktreeId: opaqueLocationId(task.worktree), result: null, stopCode: null, durationMs: null });
      const acquired: string[] = [];
      try {
        for (const resource of task.sharedResources) {
          await this.resourceLocks.acquire(resource, { runId: state.runId, taskId, attemptId, acquiredAt: this.clock.now() });
          acquired.push(resource);
        }
        const response = await this.runner.run({
          runId: state.runId,
          attemptId,
          task,
          baseline: state.baseline,
          contracts: task.requiredContracts,
          resumeThreadId: null,
        }, signal);
        assertResultScope(task, response.result);
        const finishedAt = this.clock.now();
        attempts.push({ attemptId, taskId, agentId: task.owner, startedAt, finishedAt, retryClass: null, result: response.result });
        if (response.result.status === "PASS") {
          task = passPipeline(task, finishedAt, response.result);
        } else if (response.result.status === "FAIL") {
          task = transition(task, "FAIL", finishedAt, response.result);
        } else if (response.result.status === "BLOCKED") {
          task = transition(task, "BLOCKED", finishedAt, response.result);
        } else {
          task = transition(task, "HUMAN_REVIEW_REQUIRED", finishedAt, response.result);
        }
        this.events.write({ timestamp: finishedAt, event: response.result.status === "BLOCKED" ? "TASK_BLOCKED" : "TASK_COMPLETED", runId: state.runId, taskId, agentId: task.owner, attemptId, branchId: opaqueLocationId(task.branch), worktreeId: opaqueLocationId(task.worktree), result: response.result.status, stopCode: response.result.stopCondition, durationMs: Date.parse(finishedAt) - Date.parse(startedAt) });
        return { task, attempts };
      } catch (error) {
        const finishedAt = this.clock.now();
        const retryClass = error instanceof ClassifiedFailure ? error.retryClass : "POLICY_FAILURE";
        attempts.push({ attemptId, taskId, agentId: task.owner, startedAt, finishedAt, retryClass, result: null });
        if (mayRetry(retryClass, attemptNumber, task.maxAttempts)) {
          continue;
        }
        const stop = error instanceof OrchestratorStop ? error : new OrchestratorStop("TEST_BASELINE_BROKEN", errorMessage(error), task.taskId);
        const blockedResult: AgentResult = {
          status: "BLOCKED",
          summary: "Execution stopped before a valid agent result was accepted.",
          changedFiles: [], commands: [], tests: [], evidence: [], risks: [], blockers: [stop.message], requestedAuthority: [],
          stopCondition: stop.code,
        };
        task = transition(task, "BLOCKED", finishedAt, blockedResult);
        return { task, attempts };
      } finally {
        for (const resource of acquired.reverse()) {
          await this.resourceLocks.release(resource, state.runId, attemptId);
        }
      }
    }
    throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Task '${task.taskId}' exhausted its attempt loop without a result.`, task.taskId);
  }

  private async persist(state: PersistedRunState): Promise<PersistedRunState> {
    const next = { ...state, revision: state.revision + 1, updatedAt: this.clock.now() };
    await this.stateStore.save(next);
    return next;
  }
}
