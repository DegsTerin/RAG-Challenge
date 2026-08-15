// Purpose: Owns the deterministic task-state machine so agents cannot promote their own work arbitrarily.
import type { TaskStatus } from "./contracts.js";
import { OrchestratorStop } from "./errors.js";

const transitions: Readonly<Record<TaskStatus, readonly TaskStatus[]>> = {
  DISCOVERED: ["READY", "BLOCKED", "CANCELLED"],
  READY: ["ASSIGNED", "BLOCKED", "CANCELLED"],
  BLOCKED: ["READY", "CANCELLED"],
  ASSIGNED: ["RUNNING", "BLOCKED", "CANCELLED"],
  RUNNING: ["IMPLEMENTED", "TESTING", "REVIEW", "INTEGRATION_READY", "VALIDATING", "FAIL", "BLOCKED", "HUMAN_REVIEW_REQUIRED", "CANCELLED"],
  IMPLEMENTED: ["FAIL", "BLOCKED"],
  TESTING: ["REVIEW", "INTEGRATION_READY", "VALIDATING", "FAIL", "BLOCKED", "CANCELLED"],
  REVIEW: ["INTEGRATION_READY", "PASS", "FAIL", "BLOCKED", "HUMAN_REVIEW_REQUIRED", "CANCELLED"],
  INTEGRATION_READY: ["INTEGRATING", "HUMAN_REVIEW_REQUIRED", "CANCELLED"],
  INTEGRATING: ["VALIDATING", "FAIL", "BLOCKED", "CANCELLED"],
  VALIDATING: ["PASS", "FAIL", "BLOCKED", "HUMAN_REVIEW_REQUIRED", "CANCELLED"],
  PASS: [],
  FAIL: ["READY", "CANCELLED"],
  HUMAN_REVIEW_REQUIRED: ["READY", "CANCELLED"],
  CANCELLED: [],
};

export function canTransition(from: TaskStatus, to: TaskStatus): boolean {
  return transitions[from].includes(to);
}

export function assertTransition(taskId: string, from: TaskStatus, to: TaskStatus): void {
  if (!canTransition(from, to)) {
    throw new OrchestratorStop(
      "CONFLICTING_REQUIREMENTS",
      `Task '${taskId}' cannot transition from ${from} to ${to}.`,
      taskId,
    );
  }
}
