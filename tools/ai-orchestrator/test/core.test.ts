// Purpose: Verifies closed contracts, state transitions, dependency resolution, conflict detection and deterministic dry-run scheduling.
import assert from "node:assert/strict";
import test from "node:test";
import { createDryRunPlan, persistedCoordinatorHead, validatePlanSemantics } from "../src/application/plan.js";
import type { PersistedRunState } from "../src/core/contracts.js";
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
  const parent = task({ taskId: "parent", owner: "implementation_worker", ownership: "LANE_OWNED", worktree: "C:/managed/lane", branch: "codex/parent" });
  const child = task({ taskId: "child", owner: "implementation_worker", ownership: "LANE_OWNED", worktree: "C:/managed/lane/nested", branch: "codex/child" });
  assert.throws(() => assertTaskIsolation([parent, child]), (error: unknown) => error instanceof OrchestratorStop && error.code === "SHARED_RESOURCE_COLLISION");
  if (process.platform === "win32") {
    const caseVariant = task({ taskId: "case-variant", owner: "implementation_worker", ownership: "LANE_OWNED", worktree: "c:/MANAGED/LANE", branch: "codex/case-variant" });
    assert.throws(() => assertTaskIsolation([parent, caseVariant]), (error: unknown) => error instanceof OrchestratorStop && error.code === "SHARED_RESOURCE_COLLISION");
  }
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
  assert.throws(() => validatePlanSemantics(projectPlan([implementation])), /writable plan requires/);
});

test("dry run is deterministic and exposes lanes, locks and the external Human Gate", () => {
  const mapping = task({ taskId: "mapping", priority: 200 });
  const qualitySurface = { cwd: "C:/repository", writableRoots: ["C:/repository"], sandbox: "workspace-write" as const, approvalPolicy: "never" as const, networkAccess: false as const, environmentPolicy: "minimal" as const, tools: [], mcpServers: [], skills: [] };
  const gateSurface = { ...qualitySurface, writableRoots: [], sandbox: "read-only" as const };
  const quality = task({ taskId: "quality", taskKind: "QUALITY_GATE", owner: "governance_guard", dependencies: ["mapping"], executionSurface: qualitySurface, ownership: "COORDINATOR_ONLY", parallelism: "SEQUENTIAL_ONLY", priority: 100, requiredTests: ["./eng/ci.ps1 -Offline"] });
  const gate = task({ taskId: "human-gate", taskKind: "HUMAN_GATE", owner: "governance_guard", dependencies: ["quality"], humanGate: true, executionSurface: gateSurface, parallelism: "SEQUENTIAL_ONLY" });
  const preview = createDryRunPlan(projectPlan([mapping, quality, gate]));
  assert.deepEqual(preview.waves, [["mapping"], ["quality"], ["human-gate"]]);
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

test("plans and agent results reject synthetic secret-shaped strings before acceptance", () => {
  const synthetic = "sk-proj-synthetic-not-a-real-secret";
  const plan = structuredClone(projectPlan([task()])) as unknown as { tasks: { objective: string }[] };
  plan.tasks[0]!.objective = synthetic;
  assert.throws(() => parseProjectPlan(plan), (error: unknown) =>
    error instanceof OrchestratorStop && error.code === "SECRET_REQUIRED" && !error.message.includes(synthetic));
  assert.throws(() => parseAgentResult({ ...passingResult(), summary: synthetic }), (error: unknown) =>
    error instanceof OrchestratorStop && error.code === "SECRET_REQUIRED" && !error.message.includes(synthetic));
});

test("parsed plan preserves its exact baseline and bounded concurrency", () => {
  const parsed = parseProjectPlan(projectPlan([task()], 2));
  assert.equal(parsed.baseline, baseline);
  assert.equal(parsed.maxConcurrency, 2);
});

test("input plans reject forged runtime state and evidence", () => {
  assert.throws(() => validatePlanSemantics(projectPlan([task({ status: "PASS" })])), (error: unknown) =>
    error instanceof OrchestratorStop && error.code === "UNEXPECTED_DIRTY_TREE");
  assert.throws(() => validatePlanSemantics(projectPlan([task({ evidence: ["forged"] })])), (error: unknown) =>
    error instanceof OrchestratorStop && error.code === "UNEXPECTED_DIRTY_TREE");
});

test("non-implementation execution is bound to the coordinator repository root", () => {
  assert.throws(() => validatePlanSemantics(projectPlan([task({ executionSurface: { ...task().executionSurface, cwd: "C:/outside" } })]), "C:/repository"),
    (error: unknown) => error instanceof OrchestratorStop && error.code === "OUT_OF_SCOPE_CHANGE_REQUIRED");
});

test("command evidence cannot claim PASS for a non-zero exit code", () => {
  assert.throws(() => parseAgentResult({
    ...passingResult(),
    tests: [{ commandId: "forged", exitCode: 1, durationMs: 1, result: "PASS", relevantOutput: [] }],
  }), /inconsistent exit-code evidence/);
});

test("resume baseline follows the contiguous persisted integration chain", () => {
  const integratedHead = "3".repeat(40);
  const first = task({ taskId: "integration-a", taskKind: "INTEGRATION", owner: "governance_guard", priority: 200, status: "PASS", candidate: { commitId: integratedHead, treeId: "4".repeat(40), changedFiles: [] } });
  const second = task({ taskId: "integration-b", taskKind: "INTEGRATION", owner: "governance_guard", priority: 100, status: "DISCOVERED" });
  const state: PersistedRunState = { schemaVersion: 1, runId: "run-integration", revision: 0, baseline, maxConcurrency: 1, createdAt: "2026-08-14T12:00:00.000Z", updatedAt: "2026-08-14T12:00:00.000Z", tasks: [first, second], attempts: [], heldLocks: [], humanGateReached: false };
  assert.equal(persistedCoordinatorHead(state), integratedHead);
  assert.throws(() => persistedCoordinatorHead({ ...state, tasks: [{ ...first, status: "DISCOVERED", candidate: null }, { ...second, status: "PASS", candidate: { commitId: "5".repeat(40), treeId: "6".repeat(40), changedFiles: [] } }] }), /not a contiguous coordinator chain/);
});
