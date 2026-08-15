// Purpose: Carries canonical stop conditions across core and adapter boundaries without converting them into free-form failures.
import type { StopCode } from "./contracts.js";

export class OrchestratorStop extends Error {
  public constructor(
    public readonly code: StopCode,
    message: string,
    public readonly taskId: string | null = null,
  ) {
    super(message);
    this.name = "OrchestratorStop";
  }
}

export function errorMessage(error: unknown): string {
  return error instanceof Error ? error.message : "Unknown orchestrator failure.";
}
