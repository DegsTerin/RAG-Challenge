// Purpose: Produces a deterministic side-effect-free execution preview including lanes, locks, gates and conflicts.
import type { ProjectPlan, TaskDefinition } from "../core/contracts.js";
import { assertTaskIsolation, taskConflict } from "../core/conflicts.js";
import { DependencyGraph } from "../core/dependency-graph.js";
import { scheduleWave } from "../core/scheduler.js";
import { OrchestratorStop } from "../core/errors.js";
import { resolve } from "node:path";

export interface DryRunPlan {
  readonly baseline: string;
  readonly maximumConcurrency: number;
  readonly tasks: readonly string[];
  readonly dependencies: Readonly<Record<string, readonly string[]>>;
  readonly agents: Readonly<Record<string, string>>;
  readonly waves: readonly (readonly string[])[];
  readonly worktrees: Readonly<Record<string, string | null>>;
  readonly branches: Readonly<Record<string, string | null>>;
  readonly resources: Readonly<Record<string, readonly string[]>>;
  readonly locks: readonly string[];
  readonly qualityGates: readonly string[];
  readonly humanGates: readonly string[];
  readonly conflicts: readonly { readonly tasks: readonly [string, string]; readonly reasons: readonly string[] }[];
}

function passTasks(tasks: readonly TaskDefinition[], passed: ReadonlySet<string>): TaskDefinition[] {
  return tasks.map((task) => passed.has(task.taskId) ? { ...task, status: "PASS" as const } : task);
}

function sameAbsolutePath(left: string, right: string): boolean {
  const normalise = (value: string): string => process.platform === "win32" ? resolve(value).toLowerCase() : resolve(value);
  return normalise(left) === normalise(right);
}

function integrationOrder(tasks: readonly TaskDefinition[]): TaskDefinition[] {
  return tasks
    .filter((task) => task.taskKind === "INTEGRATION")
    .sort((left, right) => right.priority - left.priority || left.taskId.localeCompare(right.taskId));
}

function transitiveDependencies(taskId: string, tasks: readonly TaskDefinition[]): ReadonlySet<string> {
  const byId = new Map(tasks.map((task) => [task.taskId, task]));
  const visited = new Set<string>();
  const visit = (current: string): void => {
    for (const dependency of byId.get(current)?.dependencies ?? []) {
      if (!visited.has(dependency)) {
        visited.add(dependency);
        visit(dependency);
      }
    }
  };
  visit(taskId);
  return visited;
}

export function validatePlanSemantics(plan: ProjectPlan, repositoryRoot = "C:/repository"): void {
  new DependencyGraph(plan.tasks);
  assertTaskIsolation(plan.tasks);
  const implementations = plan.tasks.filter((task) => task.taskKind === "IMPLEMENTATION");
  const integrations = integrationOrder(plan.tasks);
  const qualityTasks = plan.tasks.filter((task) => task.taskKind === "QUALITY_GATE");
  const humanGates = plan.tasks.filter((task) => task.taskKind === "HUMAN_GATE");
  if (implementations.length > 0 && (integrations.length !== implementations.length || qualityTasks.length !== 1 || humanGates.length !== 1)) {
    throw new OrchestratorStop("AMBIGUOUS_AUTHORITY", "A writable plan requires one reviewed integration per implementation, one quality gate and one external Human Gate.");
  }
  for (const task of plan.tasks) {
    const expectedOwner = task.taskKind === "IMPLEMENTATION"
      ? "implementation_worker"
      : task.taskKind === "INDEPENDENT_REVIEW"
        ? "independent_reviewer"
        : task.taskKind === "SECURITY_REVIEW"
          ? "security_reviewer"
          : ["INTEGRATION", "QUALITY_GATE", "HUMAN_GATE"].includes(task.taskKind)
            ? "governance_guard"
            : null;
    if (expectedOwner !== null && task.owner !== expectedOwner) {
      throw new OrchestratorStop("AMBIGUOUS_AUTHORITY", `Task '${task.taskId}' has an owner incompatible with ${task.taskKind}.`, task.taskId);
    }
    if (task.taskKind === "DISCOVERY" && !["governance_guard", "code_mapper", "architect"].includes(task.owner)) {
      throw new OrchestratorStop("AMBIGUOUS_AUTHORITY", `Discovery task '${task.taskId}' has an incompatible specialised owner.`, task.taskId);
    }
    const implementationWritable = task.taskKind === "IMPLEMENTATION";
    const coordinatorWritable = ["INTEGRATION", "QUALITY_GATE"].includes(task.taskKind);
    const writable = implementationWritable || coordinatorWritable;
    if ((writable && task.executionSurface.sandbox !== "workspace-write") ||
        (!writable && task.executionSurface.sandbox !== "read-only") ||
        task.executionSurface.networkAccess || task.executionSurface.approvalPolicy !== "never" ||
        task.executionSurface.environmentPolicy !== "minimal" || task.executionSurface.mcpServers.length > 0 ||
        task.executionSurface.skills.length > 0) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", `Task '${task.taskId}' has an execution surface outside the current authority.`, task.taskId);
    }
    if (implementationWritable && (task.worktree === null || task.executionSurface.cwd !== task.worktree ||
        task.executionSurface.writableRoots.length !== 1 || task.executionSurface.writableRoots[0] !== task.worktree)) {
      throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", `Implementation task '${task.taskId}' does not bind its sole writable root to its worktree.`, task.taskId);
    }
    if (!implementationWritable && !sameAbsolutePath(task.executionSurface.cwd, repositoryRoot)) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", `Task '${task.taskId}' is not bound to the coordinator repository root.`, task.taskId);
    }
    if (coordinatorWritable && (task.ownership !== "COORDINATOR_ONLY" || task.parallelism !== "SEQUENTIAL_ONLY" ||
        task.executionSurface.writableRoots.length !== 1 || task.executionSurface.writableRoots[0] !== task.executionSurface.cwd)) {
      throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", `Coordinator task '${task.taskId}' does not bind its sole writable root sequentially.`, task.taskId);
    }
    if (!writable && task.executionSurface.writableRoots.length > 0) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", `Read-only task '${task.taskId}' declares writable roots.`, task.taskId);
    }
    const expectedTools = task.taskKind === "IMPLEMENTATION"
      ? ["apply_patch", "shell"]
      : ["DISCOVERY", "INDEPENDENT_REVIEW", "SECURITY_REVIEW"].includes(task.taskKind)
        ? ["shell"]
        : [];
    if ([...task.executionSurface.tools].sort().join("\u0000") !== expectedTools.join("\u0000")) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", `Task '${task.taskId}' has an unsupported tool surface.`, task.taskId);
    }
    if (task.allowedPaths.some((allowed) =>
      task.forbiddenPaths.some((forbidden) => allowed === forbidden || allowed.startsWith(`${forbidden}/`) || forbidden.startsWith(`${allowed}/`)))) {
      throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", `Task '${task.taskId}' has overlapping allowed and forbidden paths.`, task.taskId);
    }
    if (implementationWritable && (!task.requiresIndependentReview || !task.requiresSecurityReview)) {
      throw new OrchestratorStop("AMBIGUOUS_AUTHORITY", `Implementation task '${task.taskId}' requires both independent and security review.`, task.taskId);
    }
    if (task.requiresIndependentReview && !plan.tasks.some((candidate) =>
      candidate.taskKind === "INDEPENDENT_REVIEW" && candidate.candidateTaskId === task.taskId && candidate.dependencies.includes(task.taskId))) {
      throw new OrchestratorStop("AMBIGUOUS_AUTHORITY", `Task '${task.taskId}' has no dependent independent review.`, task.taskId);
    }
    if (task.requiresSecurityReview && !plan.tasks.some((candidate) =>
      candidate.taskKind === "SECURITY_REVIEW" && candidate.candidateTaskId === task.taskId && candidate.dependencies.includes(task.taskId))) {
      throw new OrchestratorStop("AMBIGUOUS_AUTHORITY", `Task '${task.taskId}' has no dependent security review.`, task.taskId);
    }
    if (task.humanGate !== (task.taskKind === "HUMAN_GATE")) {
      throw new OrchestratorStop("HUMAN_GATE_REQUIRED", `Human Gate task '${task.taskId}' must remain under governance ownership.`, task.taskId);
    }
    if (task.taskKind === "QUALITY_GATE") {
      if (task.requiredTests.length !== 1 || task.requiredTests[0] !== "./eng/ci.ps1 -Offline") {
        throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Quality task '${task.taskId}' does not name the canonical coordinator-owned gate.`, task.taskId);
      }
    } else if (task.requiredTests.length > 0) {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Task '${task.taskId}' cannot treat agent-declared tests as trusted evidence.`, task.taskId);
    }
    if (["INDEPENDENT_REVIEW", "SECURITY_REVIEW", "INTEGRATION"].includes(task.taskKind)) {
      const candidate = task.candidateTaskId === null ? null : plan.tasks.find((entry) => entry.taskId === task.candidateTaskId);
      if (candidate?.taskKind !== "IMPLEMENTATION" || !task.dependencies.includes(candidate.taskId)) {
        throw new OrchestratorStop("AMBIGUOUS_AUTHORITY", `Task '${task.taskId}' is not bound to a direct implementation dependency.`, task.taskId);
      }
    } else if (task.candidateTaskId !== null) {
      throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", `Task '${task.taskId}' cannot declare a candidate task.`, task.taskId);
    }
    if (task.status !== "DISCOVERED" || task.startedAt !== null || task.finishedAt !== null || task.result !== null || task.evidence.length > 0 || task.candidate !== null) {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", `Input plan task '${task.taskId}' contains forged runtime state or evidence.`, task.taskId);
    }
    if (task.taskKind === "INTEGRATION" && task.candidateTaskId !== null) {
      const implementation = plan.tasks.find((entry) => entry.taskId === task.candidateTaskId);
      const requiredReviewIds = plan.tasks
        .filter((entry) => entry.candidateTaskId === implementation?.taskId && ["INDEPENDENT_REVIEW", "SECURITY_REVIEW"].includes(entry.taskKind))
        .map((entry) => entry.taskId);
      if (requiredReviewIds.some((reviewId) => !task.dependencies.includes(reviewId))) {
        throw new OrchestratorStop("AMBIGUOUS_AUTHORITY", `Integration task '${task.taskId}' does not depend on every required review.`, task.taskId);
      }
    }
    if (task.taskKind === "QUALITY_GATE" && plan.tasks.some((entry) => entry.taskKind === "IMPLEMENTATION") &&
        !task.dependencies.includes(integrations.at(-1)?.taskId ?? "")) {
      throw new OrchestratorStop("AMBIGUOUS_AUTHORITY", `Quality task '${task.taskId}' is not downstream of the complete integration chain.`, task.taskId);
    }
    if (task.taskKind === "HUMAN_GATE" &&
        !task.dependencies.some((dependency) => plan.tasks.find((entry) => entry.taskId === dependency)?.taskKind === "QUALITY_GATE")) {
      throw new OrchestratorStop("HUMAN_GATE_REQUIRED", `Human Gate task '${task.taskId}' is not downstream of a deterministic quality gate.`, task.taskId);
    }
  }
  for (const [index, integration] of integrations.entries()) {
    const implementation = implementations.find((task) => task.taskId === integration.candidateTaskId);
    if (implementation === undefined || integrations.filter((task) => task.candidateTaskId === implementation.taskId).length !== 1) {
      throw new OrchestratorStop("AMBIGUOUS_AUTHORITY", `Integration task '${integration.taskId}' is not the unique integration for its implementation.`, integration.taskId);
    }
    const predecessor = integrations[index - 1];
    if (predecessor !== undefined && !integration.dependencies.includes(predecessor.taskId)) {
      throw new OrchestratorStop("AMBIGUOUS_AUTHORITY", `Integration task '${integration.taskId}' does not depend on the preceding deterministic integration.`, integration.taskId);
    }
  }
  if (implementations.length > 0) {
    const humanGate = humanGates[0];
    const closure = humanGate === undefined ? new Set<string>() : transitiveDependencies(humanGate.taskId, plan.tasks);
    const omitted = plan.tasks.filter((task) => task.taskKind !== "HUMAN_GATE" && !closure.has(task.taskId));
    if (omitted.length > 0) {
      throw new OrchestratorStop("HUMAN_GATE_REQUIRED", "The external Human Gate is not downstream of the complete task graph.", humanGate?.taskId);
    }
  }
}

export function createDryRunPlan(plan: ProjectPlan, repositoryRoot = "C:/repository"): DryRunPlan {
  validatePlanSemantics(plan, repositoryRoot);
  const passed = new Set<string>();
  const waves: string[][] = [];
  while (passed.size < plan.tasks.length) {
    const current = passTasks(plan.tasks, passed);
    const wave = scheduleWave(current, plan.maxConcurrency).tasks.filter((task) => !passed.has(task.taskId));
    if (wave.length === 0) {
      const unresolved = plan.tasks.filter((task) => !passed.has(task.taskId)).map((task) => task.taskId);
      throw new OrchestratorStop("AMBIGUOUS_AUTHORITY", `The dry run cannot schedule tasks: ${unresolved.join(", ")}.`);
    }
    const ids = wave.map((task) => task.taskId);
    waves.push(ids);
    ids.forEach((taskId) => passed.add(taskId));
  }

  const conflicts: { tasks: [string, string]; reasons: readonly string[] }[] = [];
  for (let leftIndex = 0; leftIndex < plan.tasks.length; leftIndex += 1) {
    const left = plan.tasks[leftIndex];
    if (left === undefined) {
      continue;
    }
    for (let rightIndex = leftIndex + 1; rightIndex < plan.tasks.length; rightIndex += 1) {
      const right = plan.tasks[rightIndex];
      if (right === undefined) {
        continue;
      }
      const conflict = taskConflict(left, right);
      if (conflict !== null) {
        conflicts.push({ tasks: [left.taskId, right.taskId], reasons: conflict.reasons });
      }
    }
  }

  return {
    baseline: plan.baseline,
    maximumConcurrency: plan.maxConcurrency,
    tasks: plan.tasks.map((task) => task.taskId),
    dependencies: Object.fromEntries(plan.tasks.map((task) => [task.taskId, task.dependencies])),
    agents: Object.fromEntries(plan.tasks.map((task) => [task.taskId, task.owner])),
    waves,
    worktrees: Object.fromEntries(plan.tasks.map((task) => [task.taskId, task.worktree])),
    branches: Object.fromEntries(plan.tasks.map((task) => [task.taskId, task.branch])),
    resources: Object.fromEntries(plan.tasks.map((task) => [task.taskId, task.sharedResources])),
    locks: [...new Set(plan.tasks.flatMap((task) => task.sharedResources))].sort(),
    qualityGates: plan.tasks.filter((task) => task.requiredTests.some((test) => test.includes("eng/ci.ps1"))).map((task) => task.taskId),
    humanGates: plan.tasks.filter((task) => task.humanGate).map((task) => task.taskId),
    conflicts,
  };
}
