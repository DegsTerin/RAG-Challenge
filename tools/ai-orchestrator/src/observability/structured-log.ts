// Purpose: Emits bounded correlation metadata while excluding prompts, environment data, absolute paths and raw agent output.
import type { AgentId, ResultStatus, StopCode } from "../core/contracts.js";

export interface StructuredEvent {
  readonly timestamp: string;
  readonly event: "TASK_ASSIGNED" | "TASK_COMPLETED" | "TASK_BLOCKED" | "HUMAN_GATE_REACHED" | "RUN_COMPLETED";
  readonly runId: string;
  readonly taskId: string | null;
  readonly agentId: AgentId | null;
  readonly attemptId: string | null;
  readonly branchId: string | null;
  readonly worktreeId: string | null;
  readonly result: ResultStatus | null;
  readonly stopCode: StopCode | null;
  readonly durationMs: number | null;
}

export interface EventSink {
  write(event: StructuredEvent): void;
}

export class MemoryEventSink implements EventSink {
  public readonly events: StructuredEvent[] = [];

  public write(event: StructuredEvent): void {
    this.events.push(event);
  }
}

export function opaqueLocationId(value: string | null): string | null {
  if (value === null) {
    return null;
  }
  let hash = 2166136261;
  for (const character of value) {
    hash ^= character.charCodeAt(0);
    hash = Math.imul(hash, 16777619);
  }
  return `loc-${(hash >>> 0).toString(16).padStart(8, "0")}`;
}
