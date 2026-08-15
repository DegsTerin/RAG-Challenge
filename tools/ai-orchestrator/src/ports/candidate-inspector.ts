// Purpose: Defines trusted Git-derived candidate evidence independently from agent-reported changed-file claims.
import type { AgentResult, CandidateEvidence, TaskDefinition } from "../core/contracts.js";

export interface CandidateInspector {
  inspect(task: TaskDefinition, baseline: string, agentResult: AgentResult): Promise<CandidateEvidence>;
}
