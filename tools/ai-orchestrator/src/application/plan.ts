// Purpose: Produces a deterministic side-effect-free execution preview including lanes, locks, gates and conflicts.
import type { ProjectPlan, TaskDefinition } from "../core/contracts.js";
import { assertTaskIsolation, taskConflict } from "../core/conflicts.js";
import { DependencyGraph } from "../core/dependency-graph.js";
import { scheduleWave } from "../core/scheduler.js";
import { OrchestratorStop } from "../core/errors.js";

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

export function validatePlanSemantics(plan: ProjectPlan): void {
  new DependencyGraph(plan.tasks);
  assertTaskIsolation(plan.tasks);
  for (const task of plan.tasks) {
    if (task.allowedPaths.some((allowed) =>
      task.forbiddenPaths.some((forbidden) => allowed === forbidden || allowed.startsWith(`${forbidden}/`) || forbidden.startsWith(`${allowed}/`)))) {
      throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", `Task '${task.taskId}' has overlapping allowed and forbidden paths.`, task.taskId);
    }
    if (task.requiresIndependentReview && !plan.tasks.some((candidate) =>
      candidate.owner === "independent_reviewer" && candidate.dependencies.includes(task.taskId))) {
      throw new OrchestratorStop("AMBIGUOUS_AUTHORITY", `Task '${task.taskId}' has no dependent independent review.`, task.taskId);
    }
    if (task.requiresSecurityReview && !plan.tasks.some((candidate) =>
      candidate.owner === "security_reviewer" && candidate.dependencies.includes(task.taskId))) {
      throw new OrchestratorStop("AMBIGUOUS_AUTHORITY", `Task '${task.taskId}' has no dependent security review.`, task.taskId);
    }
    if (task.humanGate && task.owner !== "governance_guard") {
      throw new OrchestratorStop("HUMAN_GATE_REQUIRED", `Human Gate task '${task.taskId}' must remain under governance ownership.`, task.taskId);
    }
  }
}

export function createDryRunPlan(plan: ProjectPlan): DryRunPlan {
  validatePlanSemantics(plan);
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
