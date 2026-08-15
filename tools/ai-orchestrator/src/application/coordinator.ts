// Purpose: Coordinates isolated agent work and deterministic review, integration and quality boundaries without accepting self-asserted evidence.
import { randomUUID } from "node:crypto";
import type { QualityGate } from "../adapters/quality-gate.js";
import type {
  AgentResult,
  AgentRunner,
  AttemptRecord,
  CommandEvidence,
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
import type { CandidateInspector } from "../ports/candidate-inspector.js";
import type { IntegrationExecutor } from "../ports/integration-executor.js";
import type { ResourceLocks } from "../ports/resource-locks.js";
import type { StateStore } from "../ports/state-store.js";
import type { WorktreeManager } from "../ports/worktrees.js";
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

function sanitiseAgentResult(result: AgentResult, changedFiles: readonly string[] = []): AgentResult {
  return {
    schemaVersion: 1,
    status: result.status,
    summary: `A bounded ${result.status.toLowerCase()} result was accepted.`,
    changedFiles,
    commands: result.commands.map(sanitiseCommand),
    tests: result.tests.map(sanitiseCommand),
    evidence: result.status === "PASS" ? ["structured-result-accepted"] : [],
    risks: [],
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

function assertRequiredAgentEvidence(task: TaskDefinition, result: AgentResult): void {
  const observed = new Set(result.tests.filter((test) => test.result === "PASS").map((test) => test.commandId));
  const missing = task.requiredTests.filter((testId) => !observed.has(testId));
  if (missing.length > 0) {
    throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Task '${task.taskId}' did not return every required focused test ID.`, task.taskId);
  }
}

function candidateTask(state: PersistedRunState, task: TaskDefinition): TaskDefinition {
  if (task.candidateTaskId === null) {
    throw new OrchestratorStop("AMBIGUOUS_AUTHORITY", `Task '${task.taskId}' has no candidate binding.`, task.taskId);
  }
  return new DependencyGraph(state.tasks).task(task.candidateTaskId);
}

export class Coordinator {
  public constructor(
    private readonly runner: AgentRunner,
    private readonly stateStore: StateStore,
    private readonly resourceLocks: ResourceLocks,
    private readonly events: EventSink,
    private readonly worktrees: WorktreeManager,
    private readonly candidateInspector: CandidateInspector,
    private readonly integration: IntegrationExecutor,
    private readonly qualityGate: QualityGate,
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

  public async resume(runId: string, maximumConcurrency: number, signal?: AbortSignal): Promise<PersistedRunState> {
    const state = await this.stateStore.load(runId);
    if (!Number.isInteger(maximumConcurrency) || maximumConcurrency < 1 || maximumConcurrency > 3 || maximumConcurrency !== state.maxConcurrency) {
      throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", "Resume concurrency must equal the persisted bounded value from 1 to 3.");
    }
    const orphanLocks = await this.resourceLocks.inspect();
    if (state.heldLocks.length > 0 || orphanLocks.length > 0) {
      throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", "Recovered state records held locks; operator reconciliation is required.");
    }
    const interrupted = state.tasks.find((task) => ["ASSIGNED", "RUNNING", "TESTING", "REVIEW", "INTEGRATION_READY", "INTEGRATING", "VALIDATING"].includes(task.status));
    if (interrupted !== undefined) {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Task '${interrupted.taskId}' was interrupted; report-only reconciliation is required before resume.`, interrupted.taskId);
    }
    return await this.execute(state, signal);
  }

  private async execute(initial: PersistedRunState, signal?: AbortSignal): Promise<PersistedRunState> {
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
      state = await this.persist({ ...state, heldLocks: [...new Set(wave.flatMap((task) => task.sharedResources))].sort() });
      const results = await Promise.all(wave.map(async (task) => await this.runTask(state, task.taskId, signal)));
      for (const completed of results) {
        state = { ...state, tasks: replaceTask(state.tasks, completed.task), attempts: [...state.attempts, ...completed.attempts], heldLocks: [] };
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
        const completed = ["INTEGRATION", "QUALITY_GATE"].includes(task.taskKind)
          ? await this.runDeterministicTask(state, task, signal)
          : await this.runAgentTask(state, task, attemptId, signal);
        task = completed.task;
        const finishedAt = this.clock.now();
        attempts.push({ attemptId, taskId, agentId: task.owner, startedAt, finishedAt, retryClass: null, threadId: completed.threadId, result: task.result });
        this.events.write({ timestamp: finishedAt, event: "TASK_COMPLETED", runId: state.runId, taskId, agentId: task.owner, attemptId, branchId: opaqueLocationId(task.branch), worktreeId: opaqueLocationId(task.worktree), result: task.result?.status ?? "PASS", stopCode: task.result?.stopCondition ?? null, durationMs: Date.parse(finishedAt) - Date.parse(startedAt) });
        return { task, attempts };
      } catch (error) {
        const finishedAt = this.clock.now();
        const retryClass = error instanceof ClassifiedFailure ? error.retryClass : "POLICY_FAILURE";
        attempts.push({ attemptId, taskId, agentId: task.owner, startedAt, finishedAt, retryClass, threadId: null, result: null });
        if (mayRetry(retryClass, attemptNumber, task.maxAttempts)) {
          continue;
        }
        const stop = error instanceof OrchestratorStop ? error : new OrchestratorStop("TEST_BASELINE_BROKEN", errorMessage(error), task.taskId);
        const blockedResult: AgentResult = {
          schemaVersion: 1, status: "BLOCKED", summary: "Execution stopped before trusted evidence was accepted.",
          changedFiles: [], commands: [], tests: [], evidence: [], risks: [], blockers: ["A governed boundary stopped execution."],
          requestedAuthority: ["Separate reconciliation or authority is required."], stopCondition: stop.code,
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

  private async runAgentTask(state: PersistedRunState, task: TaskDefinition, attemptId: string, signal?: AbortSignal): Promise<{ task: TaskDefinition; threadId: string | null }> {
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
    const previousThread = [...state.attempts].reverse().find((attempt) => attempt.taskId === task.taskId && attempt.threadId !== null)?.threadId ?? null;
    const response = await this.runner.run({ runId: state.runId, attemptId, task, baseline: state.baseline, contracts: task.requiredContracts, candidate: boundCandidate, resumeThreadId: previousThread }, signal);
    if (response.result.status !== "PASS") {
      const accepted = sanitiseAgentResult(response.result);
      const terminal = response.result.status === "FAIL" ? "FAIL" : response.result.status === "BLOCKED" ? "BLOCKED" : "HUMAN_REVIEW_REQUIRED";
      return { task: transition(task, terminal, this.clock.now(), accepted), threadId: response.threadId };
    }
    if (task.taskKind === "IMPLEMENTATION") {
      assertRequiredAgentEvidence(task, response.result);
      const candidate = await this.candidateInspector.inspect(task, state.baseline, response.result);
      if (!samePaths(candidate.changedFiles, response.result.changedFiles)) {
        throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "Agent-reported files do not match the trusted Git diff.", task.taskId);
      }
      assertResultScope(task, candidate.changedFiles);
      const accepted = sanitiseAgentResult(response.result, candidate.changedFiles);
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
    if (implementation.candidate === null || implementation.result?.status !== "PASS") {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Integration requires a trusted implemented candidate.", task.taskId);
    }
    const reviews = state.tasks.filter((entry) => entry.candidateTaskId === implementation.taskId);
    const independentReview = reviews.find((entry) => entry.taskKind === "INDEPENDENT_REVIEW")?.result ?? null;
    const securityReview = reviews.find((entry) => entry.taskKind === "SECURITY_REVIEW")?.result ?? null;
    let current = transition(task, "INTEGRATION_READY", this.clock.now());
    current = transition(current, "INTEGRATING", this.clock.now());
    const evidence = await this.integration.integrate({ baseline: state.baseline, integrationTask: task, implementationTask: implementation, candidate: implementation.candidate, workerResult: implementation.result, independentReview, securityReview }, signal);
    if (evidence.result !== "PASS") {
      throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", "Sequential integration did not pass.", task.taskId);
    }
    const result = deterministicResult("The coordinator integrated the reviewed candidate sequentially.", [evidence]);
    current = transition(current, "VALIDATING", this.clock.now(), result);
    current = transition(current, "PASS", this.clock.now(), result);
    return { task: current, threadId: null };
  }

  private async persist(state: PersistedRunState): Promise<PersistedRunState> {
    const next = { ...state, revision: state.revision + 1, updatedAt: this.clock.now() };
    await this.stateStore.save(next);
    return next;
  }
}
