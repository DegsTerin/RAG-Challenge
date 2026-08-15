// Purpose: Provides deterministic agent outcomes for planning, tests and controlled end-to-end validation without external execution.
import type { AgentResult, AgentRunner, AgentRunRequest, AgentRunResponse } from "../core/contracts.js";
import { OrchestratorStop } from "../core/errors.js";
import { parseAgentResult } from "../core/validation.js";

export type FakeOutcome = AgentResult | Error | ((request: AgentRunRequest) => AgentResult | Promise<AgentResult>);

export class FakeAgentRunner implements AgentRunner {
  public readonly calls: AgentRunRequest[] = [];

  public constructor(
    private readonly outcomes: ReadonlyMap<string, FakeOutcome>,
    private readonly fallback: AgentResult | null = null,
  ) {}

  public async run(request: AgentRunRequest, signal?: AbortSignal): Promise<AgentRunResponse> {
    if (signal?.aborted === true) {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Fake execution for '${request.task.taskId}' was interrupted.`, request.task.taskId);
    }
    this.calls.push(request);
    const configured = this.outcomes.get(request.task.taskId) ?? this.fallback;
    if (configured === null || configured === undefined) {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", `No fake outcome exists for '${request.task.taskId}'.`, request.task.taskId);
    }
    if (configured instanceof Error) {
      throw configured;
    }
    const value = typeof configured === "function" ? await configured(request) : configured;
    const result = parseAgentResult(JSON.parse(JSON.stringify(value)) as unknown);
    return { result, threadId: `fake-${request.attemptId}` };
  }
}
