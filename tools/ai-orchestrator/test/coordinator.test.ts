// Purpose: Exercises controlled end-to-end delegation, review, gate, retry and failure-injection behaviour with no external agent calls.
import assert from "node:assert/strict";
import test from "node:test";
import { Coordinator, type Clock, type IdSource } from "../src/application/coordinator.js";
import { FakeAgentRunner } from "../src/adapters/fake-agent-runner.js";
import type { AgentRunner, AgentRunRequest, AgentRunResponse } from "../src/core/contracts.js";
import { ClassifiedFailure } from "../src/core/retry.js";
import {
  CollectingEvents,
  InMemoryResourceLocks,
  InMemoryStateStore,
  passingResult,
  projectPlan,
  task,
} from "./helpers.js";

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

test("controlled E2E delegates, reviews, validates and stops at the external Human Gate", async () => {
  const implementation = task({
    taskId: "implementation", owner: "implementation_worker", ownership: "LANE_OWNED", dependencies: ["mapping"],
    allowedPaths: ["tools/fixture"], worktree: "C:/managed/implementation", branch: "codex/implementation",
    requiresIndependentReview: true, requiresSecurityReview: true, sharedResources: ["temp:implementation"],
  });
  const tasks = [
    task({ taskId: "mapping", priority: 300 }),
    implementation,
    task({ taskId: "independent-review", owner: "independent_reviewer", dependencies: ["implementation"], priority: 200 }),
    task({ taskId: "security-review", owner: "security_reviewer", dependencies: ["implementation"], priority: 200 }),
    task({ taskId: "quality-gate", owner: "governance_guard", dependencies: ["independent-review", "security-review"], requiredTests: ["./eng/ci.ps1 -Offline"] }),
    task({ taskId: "human-gate", owner: "governance_guard", dependencies: ["quality-gate"], humanGate: true, parallelism: "SEQUENTIAL_ONLY" }),
  ];
  const outcomes = new Map(tasks.filter((candidate) => !candidate.humanGate).map((candidate) => [candidate.taskId, passingResult(candidate.taskId === "implementation" ? ["tools/fixture/result.txt"] : [])]));
  const runner = new FakeAgentRunner(outcomes);
  const store = new InMemoryStateStore();
  const locks = new InMemoryResourceLocks();
  const events = new CollectingEvents();
  const state = await new Coordinator(runner, store, locks, events, new DeterministicClock(), ids).start(projectPlan(tasks));

  assert.equal(state.humanGateReached, true);
  assert.equal(state.tasks.find((candidate) => candidate.taskId === "human-gate")?.status, "HUMAN_REVIEW_REQUIRED");
  assert.equal(state.tasks.filter((candidate) => !candidate.humanGate).every((candidate) => candidate.status === "PASS"), true);
  assert.equal(runner.calls.some((call) => call.task.humanGate), false);
  assert.equal(locks.held.size, 0);
  assert.ok(events.events.some((event) => event.event === "HUMAN_GATE_REACHED"));
});

test("only a transient classified failure receives a bounded retry", async () => {
  let calls = 0;
  const runner: AgentRunner = {
    async run(_request: AgentRunRequest): Promise<AgentRunResponse> {
      calls += 1;
      if (calls === 1) {
        throw new ClassifiedFailure("TRANSIENT_FAILURE", "Temporary fixture failure.");
      }
      return { result: passingResult(), threadId: null };
    },
  };
  const retryTask = task({ taskId: "retry-task", maxAttempts: 2 });
  const state = await new Coordinator(
    runner,
    new InMemoryStateStore(),
    new InMemoryResourceLocks(),
    new CollectingEvents(),
    new DeterministicClock(),
    ids,
  ).start(projectPlan([retryTask]));
  assert.equal(calls, 2);
  assert.equal(state.attempts.length, 2);
  assert.equal(state.attempts[0]?.retryClass, "TRANSIENT_FAILURE");
  assert.equal(state.tasks[0]?.status, "PASS");
});

test("invalid changed-file scope is converted into a canonical blocked result", async () => {
  const writer = task({
    taskId: "writer", owner: "implementation_worker", ownership: "LANE_OWNED", allowedPaths: ["tools/allowed"],
    worktree: "C:/managed/writer", branch: "codex/writer",
  });
  const runner = new FakeAgentRunner(new Map([["writer", passingResult(["src/out-of-scope.txt"])]]));
  const state = await new Coordinator(
    runner,
    new InMemoryStateStore(),
    new InMemoryResourceLocks(),
    new CollectingEvents(),
    new DeterministicClock(),
    ids,
  ).start(projectPlan([writer]));
  assert.equal(state.tasks[0]?.status, "BLOCKED");
  assert.equal(state.tasks[0]?.result?.stopCondition, "OUT_OF_SCOPE_CHANGE_REQUIRED");
});

test("worker FAIL stops dependants without rewriting the original evidence", async () => {
  const failure = { ...passingResult(), status: "FAIL" as const, summary: "Fixture implementation failed." };
  const runner = new FakeAgentRunner(new Map([["worker", failure]]));
  const tasks = [
    task({ taskId: "worker" }),
    task({ taskId: "review", owner: "independent_reviewer", dependencies: ["worker"] }),
  ];
  const state = await new Coordinator(
    runner,
    new InMemoryStateStore(),
    new InMemoryResourceLocks(),
    new CollectingEvents(),
    new DeterministicClock(),
    ids,
  ).start(projectPlan(tasks));
  assert.equal(state.tasks.find((candidate) => candidate.taskId === "worker")?.status, "FAIL");
  assert.equal(state.tasks.find((candidate) => candidate.taskId === "review")?.status, "DISCOVERED");
});
