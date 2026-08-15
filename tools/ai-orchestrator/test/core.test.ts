// Purpose: Verifies closed contracts, state transitions, dependency resolution, conflict detection and deterministic dry-run scheduling.
import assert from "node:assert/strict";
import test from "node:test";
import { createDryRunPlan, validatePlanSemantics } from "../src/application/plan.js";
import { assertFrozenContracts, assertTaskIsolation, taskConflict } from "../src/core/conflicts.js";
import { DependencyGraph } from "../src/core/dependency-graph.js";
import { OrchestratorStop } from "../src/core/errors.js";
import { scheduleWave } from "../src/core/scheduler.js";
import { assertTransition, canTransition } from "../src/core/state-machine.js";
import { parseAgentResult, parseProjectPlan } from "../src/core/validation.js";
import { baseline, passingResult, projectPlan, task } from "./helpers.js";

test("task transitions are closed and coordinator-owned", () => {
  assert.equal(canTransition("DISCOVERED", "READY"), true);
  assert.equal(canTransition("READY", "PASS"), false);
  assert.throws(() => assertTransition("task-a", "READY", "PASS"), (error: unknown) =>
    error instanceof OrchestratorStop && error.code === "CONFLICTING_REQUIREMENTS");
});

test("dependency graph rejects missing, contradictory and circular dependencies", () => {
  assert.throws(() => new DependencyGraph([task({ dependencies: ["missing"] })]), /missing dependency/);
  assert.throws(() => new DependencyGraph([task({ taskId: "task-a", dependencies: ["task-b"], blockedBy: ["task-b"] }), task({ taskId: "task-b" })]), /both depends/);
  assert.throws(() => new DependencyGraph([
    task({ taskId: "task-a", dependencies: ["task-b"] }),
    task({ taskId: "task-b", dependencies: ["task-a"] }),
  ]), /cycle detected/);
});

test("scheduler honours priority, concurrency and mutable-resource conflicts", () => {
  const high = task({ taskId: "high", priority: 200, sharedResources: ["sqlite:test"] });
  const collision = task({ taskId: "collision", priority: 150, sharedResources: ["sqlite:test"] });
  const independent = task({ taskId: "independent", priority: 100 });
  const wave = scheduleWave([high, collision, independent], 2);
  assert.deepEqual(wave.tasks.map((candidate) => candidate.taskId), ["high", "independent"]);
  assert.deepEqual(wave.deferred.map((candidate) => candidate.taskId), ["collision"]);
});

test("conflict detection includes paths, worktrees, branches and sequential tasks", () => {
  const left = task({ taskId: "left", parallelism: "SEQUENTIAL_ONLY" });
  const right = task({ taskId: "right" });
  assert.ok(taskConflict(left, right)?.reasons.includes("sequential classification"));
  const writer = task({
    taskId: "writer", owner: "implementation_worker", ownership: "LANE_OWNED", allowedPaths: ["src"],
    worktree: "C:/managed/writer", branch: "codex/writer",
  });
  const other = task({
    taskId: "other", owner: "implementation_worker", ownership: "LANE_OWNED", allowedPaths: ["src/module"],
    worktree: "C:/managed/other", branch: "codex/other",
  });
  assert.ok(taskConflict(writer, other)?.reasons.includes("overlapping writable paths"));
});

test("writable isolation and frozen-contract parallelism fail closed", () => {
  assert.throws(() => assertTaskIsolation([task({ owner: "implementation_worker", ownership: "LANE_OWNED" })]), /requires an explicit worktree/);
  assert.throws(() => assertFrozenContracts(task({
    parallelism: "CONTRACT_FROZEN_PARALLEL",
    requiredContracts: [{ contractId: "contract-a", state: "MUTABLE_WITH_OWNER", owner: "architect" }],
  })), (error: unknown) => error instanceof OrchestratorStop && error.code === "PUBLIC_CONTRACT_CHANGE_REQUIRED");
});

test("plan semantics require independent and security review dependencies", () => {
  const implementation = task({
    taskId: "implementation", owner: "implementation_worker", ownership: "LANE_OWNED",
    worktree: "C:/managed/implementation", branch: "codex/implementation",
    requiresIndependentReview: true, requiresSecurityReview: true,
  });
  assert.throws(() => validatePlanSemantics(projectPlan([implementation])), /no dependent independent review/);
});

test("dry run is deterministic and exposes lanes, locks and the external Human Gate", () => {
  const mapping = task({ taskId: "mapping", priority: 200 });
  const review = task({ taskId: "review", owner: "independent_reviewer", dependencies: ["mapping"], priority: 100 });
  const gate = task({ taskId: "human-gate", owner: "governance_guard", dependencies: ["review"], humanGate: true, parallelism: "SEQUENTIAL_ONLY" });
  const preview = createDryRunPlan(projectPlan([mapping, review, gate]));
  assert.deepEqual(preview.waves, [["mapping"], ["review"], ["human-gate"]]);
  assert.deepEqual(preview.humanGates, ["human-gate"]);
});

test("untrusted plan and agent output reject additional fields and unsafe paths", () => {
  const source = projectPlan([task()]) as unknown as Record<string, unknown>;
  assert.throws(() => parseProjectPlan({ ...source, inventedAuthority: true }), /unsupported fields/);
  const unsafe = structuredClone(projectPlan([task()])) as unknown as { tasks: { allowedPaths: string[] }[] };
  unsafe.tasks[0]?.allowedPaths.push("../outside");
  assert.throws(() => parseProjectPlan(unsafe), (error: unknown) => error instanceof OrchestratorStop && error.code === "OUT_OF_SCOPE_CHANGE_REQUIRED");
  assert.throws(() => parseAgentResult({ ...passingResult(), extra: true }), /unsupported fields/);
  assert.throws(() => parseAgentResult({ ...passingResult(), status: "BLOCKED" }), /must carry a stop condition/);
});

test("parsed plan preserves its exact baseline and bounded concurrency", () => {
  const parsed = parseProjectPlan(projectPlan([task()], 2));
  assert.equal(parsed.baseline, baseline);
  assert.equal(parsed.maxConcurrency, 2);
});
