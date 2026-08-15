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

export interface StructuredProcessResult {
  readonly evidence: CommandEvidence;
  readonly stdout: string;
  readonly stderr: string;
}

export interface StructuredProcessExecutor extends ProcessExecutor {
  runStructured(request: ProcessRequest, signal?: AbortSignal): Promise<StructuredProcessResult>;
}
