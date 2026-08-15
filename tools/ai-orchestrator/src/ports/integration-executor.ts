// Purpose: Defines sequential candidate integration independently from agent execution and coordinator state transitions.
import type { AgentResult, CandidateEvidence, CommandEvidence, TaskDefinition } from "../core/contracts.js";

export interface IntegrationRequest {
  readonly baseline: string;
  readonly expectedCoordinatorHead: string;
  readonly integrationTask: TaskDefinition;
  readonly implementationTask: TaskDefinition;
  readonly candidate: CandidateEvidence;
  readonly workerResult: AgentResult;
  readonly independentReview: AgentResult | null;
  readonly securityReview: AgentResult | null;
}

export interface IntegrationOutcome {
  readonly evidence: CommandEvidence;
  readonly candidate: CandidateEvidence;
}

export interface IntegrationExecutor {
  integrate(request: IntegrationRequest, signal?: AbortSignal): Promise<IntegrationOutcome>;
}
