// Purpose: Defines fixed coordinator-owned process execution independently from the operating-system adapter.
import type { CommandEvidence } from "../core/contracts.js";

export interface ProcessRequest {
  readonly commandId: string;
  readonly executable: string;
  readonly arguments: readonly string[];
  readonly cwd: string;
  readonly environment: Readonly<Record<string, string>>;
  readonly timeoutMs: number;
  readonly maximumOutputBytes: number;
  readonly maximumRelevantLines?: number;
}

export interface ProcessExecutor {
  run(request: ProcessRequest, signal?: AbortSignal): Promise<CommandEvidence>;
}
