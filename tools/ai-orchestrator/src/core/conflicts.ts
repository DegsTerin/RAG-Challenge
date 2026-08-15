// Purpose: Detects path, ownership, worktree, branch, resource and contract collisions before concurrent assignment.
import { resolve, sep } from "node:path";
import type { TaskDefinition } from "./contracts.js";
import { OrchestratorStop } from "./errors.js";

function pathOverlaps(left: string, right: string): boolean {
  return left === right || left.startsWith(`${right}/`) || right.startsWith(`${left}/`);
}

function worktreePathOverlaps(left: string, right: string): boolean {
  const normalise = (value: string): string => process.platform === "win32" ? resolve(value).toLowerCase() : resolve(value);
  const leftPath = normalise(left);
  const rightPath = normalise(right);
  return leftPath === rightPath || leftPath.startsWith(`${rightPath}${sep}`) || rightPath.startsWith(`${leftPath}${sep}`);
}

function intersects(left: readonly string[], right: readonly string[]): boolean {
  return left.some((value) => right.includes(value));
}

function isWritable(task: TaskDefinition): boolean {
  return task.owner === "implementation_worker" || ["LANE_OWNED", "SINGLE_OWNER", "GENERATED", "COORDINATOR_ONLY"].includes(task.ownership);
}

export interface TaskConflict {
  readonly leftTaskId: string;
  readonly rightTaskId: string;
  readonly reasons: readonly string[];
}

export function taskConflict(left: TaskDefinition, right: TaskDefinition): TaskConflict | null {
  const reasons: string[] = [];
  if (left.parallelism === "SEQUENTIAL_ONLY" || right.parallelism === "SEQUENTIAL_ONLY") {
    reasons.push("sequential classification");
  }
  if (left.parallelism === "SINGLE_OWNER" && left.owner === right.owner) {
    reasons.push("single-owner lane");
  }
  if (right.parallelism === "SINGLE_OWNER" && left.owner === right.owner) {
    reasons.push("single-owner lane");
  }
  if (left.allowedPaths.some((leftPath) => right.allowedPaths.some((rightPath) => pathOverlaps(leftPath, rightPath)))) {
    reasons.push("overlapping writable paths");
  }
  if (intersects(left.sharedResources, right.sharedResources)) {
    reasons.push("shared mutable resource");
  }
  if (isWritable(left) && isWritable(right) && left.worktree !== null && right.worktree !== null && worktreePathOverlaps(left.worktree, right.worktree)) {
    reasons.push("overlapping worktrees");
  }
  if (isWritable(left) && isWritable(right) && left.branch !== null && left.branch === right.branch) {
    reasons.push("shared branch");
  }
  const mutableContracts = left.requiredContracts.filter((contract) => contract.state === "MUTABLE_WITH_OWNER");
  if (mutableContracts.some((contract) =>
    right.requiredContracts.some((other) => other.contractId === contract.contractId && other.owner !== contract.owner))) {
    reasons.push("contract ownership conflict");
  }
  return reasons.length === 0 ? null : { leftTaskId: left.taskId, rightTaskId: right.taskId, reasons: [...new Set(reasons)] };
}

export function assertTaskIsolation(tasks: readonly TaskDefinition[]): void {
  const writeTasks = tasks.filter((task) => task.taskKind === "IMPLEMENTATION" || ["LANE_OWNED", "SINGLE_OWNER", "GENERATED"].includes(task.ownership));
  for (const task of writeTasks) {
    if (task.worktree === null || task.branch === null) {
      throw new OrchestratorStop(
        "AMBIGUOUS_AUTHORITY",
        `Writable task '${task.taskId}' requires an explicit worktree and branch.`,
        task.taskId,
      );
    }
    if (!task.branch.startsWith("codex/")) {
      throw new OrchestratorStop(
        "OUT_OF_SCOPE_CHANGE_REQUIRED",
        `Writable task '${task.taskId}' does not use the authorised codex/ branch prefix.`,
        task.taskId,
      );
    }
  }
  for (let leftIndex = 0; leftIndex < writeTasks.length; leftIndex += 1) {
    const left = writeTasks[leftIndex];
    if (left === undefined) {
      continue;
    }
    for (let rightIndex = leftIndex + 1; rightIndex < writeTasks.length; rightIndex += 1) {
      const right = writeTasks[rightIndex];
      if (right === undefined) {
        continue;
      }
      if ((left.worktree !== null && right.worktree !== null && worktreePathOverlaps(left.worktree, right.worktree)) || left.branch === right.branch) {
        throw new OrchestratorStop(
          "SHARED_RESOURCE_COLLISION",
          `Writable tasks '${left.taskId}' and '${right.taskId}' have overlapping worktrees or share a branch.`,
        );
      }
    }
  }
}

export function assertFrozenContracts(task: TaskDefinition): void {
  if (task.parallelism !== "CONTRACT_FROZEN_PARALLEL") {
    return;
  }
  const mutable = task.requiredContracts.find((contract) => contract.state !== "FROZEN");
  if (mutable !== undefined) {
    throw new OrchestratorStop(
      "PUBLIC_CONTRACT_CHANGE_REQUIRED",
      `Task '${task.taskId}' cannot run as CONTRACT_FROZEN_PARALLEL because '${mutable.contractId}' is ${mutable.state}.`,
      task.taskId,
    );
  }
}
