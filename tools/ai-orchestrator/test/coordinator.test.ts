// Purpose: Proves phase separation, trusted candidate binding, deterministic gates, bounded retry and external Human Gate stops.
import assert from "node:assert/strict";
import test from "node:test";
import { FakeAgentRunner } from "../src/adapters/fake-agent-runner.js";
import type { QualityGate } from "../src/adapters/quality-gate.js";
import { Coordinator, type Clock, type IdSource } from "../src/application/coordinator.js";
import type { AgentResult, AgentRunner, AgentRunRequest, AgentRunResponse, CommandEvidence, ExecutionSurface, PersistedRunState } from "../src/core/contracts.js";
import { ClassifiedFailure } from "../src/core/retry.js";
import type { CandidateInspector } from "../src/ports/candidate-inspector.js";
import type { IntegrationExecutor, IntegrationRequest, IntegrationOutcome } from "../src/ports/integration-executor.js";
import type { WorktreeManager, WorktreeRecord } from "../src/ports/worktrees.js";
import { CollectingEvents, InMemoryResourceLocks, InMemoryStateStore, InMemoryThreadCheckpoints, instant, passingResult, projectPlan, task } from "./helpers.js";

class DeterministicClock implements Clock {
  private tick = 0;
  public now(): string {
    this.tick += 1;
    return new Date(Date.UTC(2026, 7, 14, 12, 0, 0, this.tick)).toISOString();
  }
}

const ids: IdSource = {
  runId: () => "run-fixture",
  attemptId: (taskId, attemptNumber) => `attempt-${taskId}-${attemptNumber}`,
};

const passedCommand: CommandEvidence = {
  commandId: "coordinator-observed", exitCode: 0, durationMs: 1, result: "PASS", relevantOutput: ["untrusted detail"],
};

function deterministicSurface(cwd = "C:/repository"): ExecutionSurface {
  return { cwd, writableRoots: [cwd], sandbox: "workspace-write", approvalPolicy: "never", networkAccess: false, environmentPolicy: "minimal", tools: [], mcpServers: [], skills: [] };
}

class StubWorktrees implements WorktreeManager {
  public readonly validated: string[] = [];
  public async list(): Promise<readonly WorktreeRecord[]> { return this.validated.map((path) => ({ path, branch: "codex/implementation", head: "0123456789abcdef0123456789abcdef01234567", prunable: false })); }
  public async create(_taskId: string, path: string, branch: string, head: string): Promise<WorktreeRecord> {
    this.validated.push(path);
    return { path, branch, head, prunable: false };
  }
  public async validate(_taskId: string, path: string, branch: string, head: string): Promise<WorktreeRecord> { this.validated.push(path); return { path, branch, head, prunable: false }; }
  public async removeManaged(): Promise<void> { throw new Error("Not used by the coordinator fixture."); }
}

class StubCandidateInspector implements CandidateInspector {
  public constructor(private readonly actualFiles: readonly string[] | null = null) {}
  public async inspect(_task: ReturnType<typeof task>, _baseline: string, result: AgentResult) {
    return { commitId: "1".repeat(40), treeId: "2".repeat(40), changedFiles: this.actualFiles ?? result.changedFiles };
  }
}

class StubIntegration implements IntegrationExecutor {
  public readonly calls: IntegrationRequest[] = [];
  public constructor(private readonly evidence: CommandEvidence = passedCommand) {}
  public async integrate(request: IntegrationRequest): Promise<IntegrationOutcome> {
    this.calls.push(request);
    return { evidence: this.evidence, candidate: { commitId: "3".repeat(40), treeId: "4".repeat(40), changedFiles: request.candidate.changedFiles } };
  }
}

class StubQuality implements QualityGate {
  public calls = 0;
  public constructor(private readonly evidence: CommandEvidence = passedCommand) {}
  public async run(): Promise<CommandEvidence> { this.calls += 1; return this.evidence; }
}

function coordinator(
  runner: AgentRunner,
  inspector: CandidateInspector = new StubCandidateInspector(),
  integration: StubIntegration = new StubIntegration(),
  quality: StubQuality = new StubQuality(),
  agentTimeoutMs = 300_000,
) {
  const worktrees = new StubWorktrees();
  const store = new InMemoryStateStore();
  const locks = new InMemoryResourceLocks();
  const events = new CollectingEvents();
  const checkpoints = new InMemoryThreadCheckpoints();
  return { instance: new Coordinator(runner, store, locks, events, worktrees, inspector, integration, quality, checkpoints, "C:/repository", agentTimeoutMs, new DeterministicClock(), ids), worktrees, store, locks, events, integration, quality, checkpoints };
}

function controlledTasks() {
  const implementation = task({
    taskId: "implementation", owner: "implementation_worker", ownership: "LANE_OWNED", dependencies: ["mapping"],
    allowedPaths: ["tools/fixture"], worktree: "C:/managed/implementation", branch: "codex/implementation",
    requiresIndependentReview: true, requiresSecurityReview: true, sharedResources: ["temp:implementation"],
  });
  const independentReview = task({ taskId: "independent-review", owner: "independent_reviewer", dependencies: ["implementation"], candidateTaskId: "implementation", priority: 200 });
  const securityReview = task({ taskId: "security-review", owner: "security_reviewer", dependencies: ["implementation"], candidateTaskId: "implementation", priority: 200 });
  const integration = task({
    taskId: "integration", taskKind: "INTEGRATION", owner: "governance_guard", dependencies: ["implementation", "independent-review", "security-review"],
    candidateTaskId: "implementation", executionSurface: deterministicSurface(), ownership: "COORDINATOR_ONLY", parallelism: "SEQUENTIAL_ONLY",
  });
  const quality = task({
    taskId: "quality-gate", taskKind: "QUALITY_GATE", owner: "governance_guard", dependencies: ["integration"], executionSurface: deterministicSurface(),
    ownership: "COORDINATOR_ONLY", requiredTests: ["./eng/ci.ps1 -Offline"], parallelism: "SEQUENTIAL_ONLY",
  });
  const human = task({
    taskId: "human-gate", taskKind: "HUMAN_GATE", owner: "governance_guard", dependencies: ["quality-gate"], humanGate: true,
    executionSurface: { ...deterministicSurface(), writableRoots: [], sandbox: "read-only" }, ownership: "HUMAN_CONTROLLED", parallelism: "SEQUENTIAL_ONLY",
  });
  return [task({ taskId: "mapping", priority: 300 }), implementation, independentReview, securityReview, integration, quality, human];
}

test("controlled E2E binds reviews to a trusted candidate and keeps integration and quality deterministic", async () => {
  const tasks = controlledTasks();
  const outcomes = new Map(tasks
    .filter((candidate) => ["DISCOVERY", "IMPLEMENTATION", "INDEPENDENT_REVIEW", "SECURITY_REVIEW"].includes(candidate.taskKind))
    .map((candidate) => [candidate.taskId, {
      ...passingResult(candidate.taskKind === "IMPLEMENTATION" ? ["tools/fixture/result.txt"] : []),
      risks: candidate.taskKind === "SECURITY_REVIEW" ? ["A bounded review risk."] : [],
    }]));
  const runner = new FakeAgentRunner(outcomes);
  const fixture = coordinator(runner);
  const state = await fixture.instance.start(projectPlan(tasks));

  assert.equal(state.humanGateReached, true);
  assert.equal(state.tasks.find((candidate) => candidate.taskId === "implementation")?.status, "IMPLEMENTED");
  assert.equal(state.tasks.find((candidate) => candidate.taskId === "integration")?.status, "PASS");
  assert.equal(state.tasks.find((candidate) => candidate.taskId === "quality-gate")?.status, "PASS");
  assert.equal(state.tasks.find((candidate) => candidate.taskId === "human-gate")?.status, "HUMAN_REVIEW_REQUIRED");
  assert.deepEqual(fixture.worktrees.validated, ["C:/managed/implementation"]);
  assert.equal(fixture.integration.calls.length, 1);
  assert.equal(fixture.quality.calls, 1);
  assert.equal(runner.calls.some((call) => ["INTEGRATION", "QUALITY_GATE", "HUMAN_GATE"].includes(call.task.taskKind)), false);
  assert.equal(runner.calls.filter((call) => ["INDEPENDENT_REVIEW", "SECURITY_REVIEW"].includes(call.task.taskKind)).every((call) => call.candidate?.commitId === "1".repeat(40)), true);
  assert.equal(state.tasks.find((candidate) => candidate.taskId === "quality-gate")?.result?.tests[0]?.relevantOutput.length, 0);
  const securityResult = state.tasks.find((candidate) => candidate.taskId === "security-review")?.result;
  assert.equal(securityResult?.evidence.some((value) => value.startsWith("evidence:sha256:")), true);
  assert.equal(securityResult?.risks.some((value) => value.startsWith("risk:sha256:")), true);
});

test("a failed independent review prevents integration and quality execution", async () => {
  const tasks = controlledTasks();
  const outcomes = new Map<string, AgentResult>([
    ["mapping", passingResult()], ["implementation", passingResult(["tools/fixture/result.txt"])],
    ["independent-review", { ...passingResult(), status: "FAIL", summary: "Review failed." }],
    ["security-review", passingResult()],
  ]);
  const fixture = coordinator(new FakeAgentRunner(outcomes));
  const state = await fixture.instance.start(projectPlan(tasks));
  assert.equal(state.tasks.find((candidate) => candidate.taskId === "independent-review")?.status, "FAIL");
  assert.equal(state.tasks.find((candidate) => candidate.taskId === "integration")?.status, "DISCOVERED");
  assert.equal(fixture.integration.calls.length, 0);
  assert.equal(fixture.quality.calls, 0);
});

test("a failed deterministic quality gate cannot reach the Human Gate", async () => {
  const tasks = controlledTasks();
  const outcomes = new Map(tasks
    .filter((candidate) => ["DISCOVERY", "IMPLEMENTATION", "INDEPENDENT_REVIEW", "SECURITY_REVIEW"].includes(candidate.taskKind))
    .map((candidate) => [candidate.taskId, passingResult(candidate.taskKind === "IMPLEMENTATION" ? ["tools/fixture/result.txt"] : [])]));
  const failed = { ...passedCommand, exitCode: 1, result: "FAIL" as const };
  const fixture = coordinator(new FakeAgentRunner(outcomes), new StubCandidateInspector(), new StubIntegration(), new StubQuality(failed));
  const state = await fixture.instance.start(projectPlan(tasks));
  assert.equal(state.tasks.find((candidate) => candidate.taskId === "quality-gate")?.status, "BLOCKED");
  assert.equal(state.humanGateReached, false);
});

test("only a transient classified failure receives a bounded retry", async () => {
  let calls = 0;
  const runner: AgentRunner = {
    async run(_request: AgentRunRequest): Promise<AgentRunResponse> {
      calls += 1;
      if (calls === 1) throw new ClassifiedFailure("TRANSIENT_FAILURE", "Temporary fixture failure.");
      return { result: passingResult(), threadId: "thread-fixture" };
    },
  };
  const fixture = coordinator(runner);
  const state = await fixture.instance.start(projectPlan([task({ taskId: "retry-task", maxAttempts: 2 })]));
  assert.equal(calls, 2);
  assert.equal(state.attempts.length, 2);
  assert.equal(state.attempts[0]?.retryClass, "TRANSIENT_FAILURE");
  assert.equal(state.attempts[1]?.threadId, "thread-fixture");
  assert.equal(state.tasks[0]?.status, "PASS");
});

test("trusted Git diff mismatch blocks an implementation even when the agent reports PASS", async () => {
  const tasks = controlledTasks();
  const outcomes = new Map(tasks
    .filter((candidate) => ["DISCOVERY", "IMPLEMENTATION", "INDEPENDENT_REVIEW", "SECURITY_REVIEW"].includes(candidate.taskKind))
    .map((candidate) => [candidate.taskId, passingResult(candidate.taskKind === "IMPLEMENTATION" ? ["tools/fixture/result.txt"] : [])]));
  const fixture = coordinator(
    new FakeAgentRunner(outcomes),
    new StubCandidateInspector(["src/out-of-scope.txt"]),
  );
  const state = await fixture.instance.start(projectPlan(tasks));
  const implementation = state.tasks.find((candidate) => candidate.taskKind === "IMPLEMENTATION");
  assert.equal(implementation?.status, "BLOCKED");
  assert.equal(implementation?.result?.stopCondition, "OUT_OF_SCOPE_CHANGE_REQUIRED");
});

test("agent-declared focused tests cannot substitute for coordinator-owned evidence", async () => {
  const tasks = controlledTasks().map((candidate) => candidate.taskKind === "IMPLEMENTATION" ? { ...candidate, requiredTests: ["focused-test"] } : candidate);
  const fixture = coordinator(new FakeAgentRunner(new Map()));
  await assert.rejects(fixture.instance.start(projectPlan(tasks)), (error: unknown) =>
    error instanceof Error && error.message.includes("cannot treat agent-declared tests"));
});

test("resume rejects concurrency drift beyond the persisted plan", async () => {
  const fixture = coordinator(new FakeAgentRunner(new Map([["mapping", passingResult()]])));
  await fixture.instance.start(projectPlan([task({ taskId: "mapping" })], 2));
  await assert.rejects(fixture.instance.resume("run-fixture", 3), /must equal the persisted bounded value/);
});

test("two reviewed candidates integrate against an evolving coordinator HEAD", async () => {
  const implementationA = task({
    taskId: "implementation-a", owner: "implementation_worker", ownership: "LANE_OWNED", allowedPaths: ["tools/a"],
    worktree: "C:/managed/implementation-a", branch: "codex/implementation-a", requiresIndependentReview: true, requiresSecurityReview: true,
  });
  const implementationB = task({
    taskId: "implementation-b", owner: "implementation_worker", ownership: "LANE_OWNED", allowedPaths: ["tools/b"],
    worktree: "C:/managed/implementation-b", branch: "codex/implementation-b", requiresIndependentReview: true, requiresSecurityReview: true,
  });
  const reviews = [implementationA, implementationB].flatMap((implementation) => [
    task({ taskId: `${implementation.taskId}-independent`, owner: "independent_reviewer", dependencies: [implementation.taskId], candidateTaskId: implementation.taskId }),
    task({ taskId: `${implementation.taskId}-security`, owner: "security_reviewer", dependencies: [implementation.taskId], candidateTaskId: implementation.taskId }),
  ]);
  const integrationA = task({
    taskId: "integration-a", taskKind: "INTEGRATION", owner: "governance_guard", priority: 200,
    dependencies: [implementationA.taskId, "implementation-a-independent", "implementation-a-security"], candidateTaskId: implementationA.taskId,
    executionSurface: deterministicSurface(), ownership: "COORDINATOR_ONLY", parallelism: "SEQUENTIAL_ONLY",
  });
  const integrationB = task({
    taskId: "integration-b", taskKind: "INTEGRATION", owner: "governance_guard", priority: 100,
    dependencies: [implementationB.taskId, "implementation-b-independent", "implementation-b-security", integrationA.taskId], candidateTaskId: implementationB.taskId,
    executionSurface: deterministicSurface(), ownership: "COORDINATOR_ONLY", parallelism: "SEQUENTIAL_ONLY",
  });
  const quality = task({ taskId: "quality", taskKind: "QUALITY_GATE", owner: "governance_guard", dependencies: [integrationB.taskId], executionSurface: deterministicSurface(), ownership: "COORDINATOR_ONLY", parallelism: "SEQUENTIAL_ONLY", requiredTests: ["./eng/ci.ps1 -Offline"] });
  const human = task({ taskId: "human", taskKind: "HUMAN_GATE", owner: "governance_guard", dependencies: [quality.taskId], humanGate: true, executionSurface: { ...deterministicSurface(), writableRoots: [], sandbox: "read-only" }, ownership: "HUMAN_CONTROLLED", parallelism: "SEQUENTIAL_ONLY" });
  const tasks = [implementationA, implementationB, ...reviews, integrationA, integrationB, quality, human];
  const outcomes = new Map(tasks.filter((entry) => ["IMPLEMENTATION", "INDEPENDENT_REVIEW", "SECURITY_REVIEW"].includes(entry.taskKind))
    .map((entry) => [entry.taskId, passingResult(entry.taskKind === "IMPLEMENTATION" ? [`tools/${entry.taskId.endsWith("a") ? "a" : "b"}/result.txt`] : [])]));
  const integration = new StubIntegration();
  const fixture = coordinator(new FakeAgentRunner(outcomes), new StubCandidateInspector(), integration);
  const state = await fixture.instance.start(projectPlan(tasks));
  assert.equal(state.humanGateReached, true);
  assert.deepEqual(integration.calls.map((call) => call.expectedCoordinatorHead), ["0123456789abcdef0123456789abcdef01234567", "3".repeat(40)]);
});

test("resume uses a checkpoint recorded before an interrupted agent turn", async () => {
  const runner = new FakeAgentRunner(new Map([["mapping", passingResult()]]));
  const fixture = coordinator(runner);
  const runningTask = task({ taskId: "mapping", status: "RUNNING", startedAt: instant });
  const state: PersistedRunState = {
    schemaVersion: 1, runId: "run-fixture", revision: 0, baseline: projectPlan([runningTask]).baseline, maxConcurrency: 1,
    createdAt: instant, updatedAt: instant, tasks: [runningTask], attempts: [], heldLocks: [], humanGateReached: false,
  };
  await fixture.store.save(state);
  await fixture.checkpoints.save({ schemaVersion: 1, runId: state.runId, taskId: runningTask.taskId, attemptId: "attempt-mapping-1", agentId: runningTask.owner, threadId: "thread-recovery", startedAt: instant });
  const recovered = await fixture.instance.resume(state.runId, 1);
  assert.equal(recovered.tasks[0]?.status, "PASS");
  assert.equal(runner.calls[0]?.resumeThreadId, "thread-recovery");
  assert.deepEqual(await fixture.checkpoints.inspect(state.runId), []);
});

test("agent execution has a coordinator-owned deadline", async () => {
  const hanging = new FakeAgentRunner(new Map([["mapping", async () => await new Promise<AgentResult>(() => undefined)]]));
  const fixture = coordinator(hanging, new StubCandidateInspector(), new StubIntegration(), new StubQuality(), 20);
  const state = await fixture.instance.start(projectPlan([task({ taskId: "mapping" })]));
  assert.equal(state.tasks[0]?.status, "BLOCKED");
  assert.equal(state.tasks[0]?.result?.stopCondition, "TEST_BASELINE_BROKEN");
  assert.deepEqual(await fixture.checkpoints.inspect(state.runId), []);
});
