// Purpose: Defines the frozen task, result, authority and persisted-state contracts used by every orchestrator boundary.
export const agentIds = [
  "governance_guard",
  "code_mapper",
  "architect",
  "implementation_worker",
  "independent_reviewer",
  "security_reviewer",
] as const;

export type AgentId = (typeof agentIds)[number];

export const taskKinds = [
  "DISCOVERY",
  "IMPLEMENTATION",
  "INDEPENDENT_REVIEW",
  "SECURITY_REVIEW",
  "INTEGRATION",
  "QUALITY_GATE",
  "HUMAN_GATE",
] as const;

export type TaskKind = (typeof taskKinds)[number];

export const taskStatuses = [
  "DISCOVERED",
  "READY",
  "BLOCKED",
  "ASSIGNED",
  "RUNNING",
  "IMPLEMENTED",
  "TESTING",
  "REVIEW",
  "INTEGRATION_READY",
  "INTEGRATING",
  "VALIDATING",
  "PASS",
  "FAIL",
  "HUMAN_REVIEW_REQUIRED",
  "CANCELLED",
] as const;

export type TaskStatus = (typeof taskStatuses)[number];

export const parallelismClasses = [
  "SAFE_PARALLEL",
  "CONTRACT_FROZEN_PARALLEL",
  "SINGLE_OWNER",
  "SEQUENTIAL_ONLY",
] as const;

export type ParallelismClass = (typeof parallelismClasses)[number];

export const ownershipClasses = [
  "READ_ONLY_FOR_WORKERS",
  "SINGLE_OWNER",
  "LANE_OWNED",
  "SHARED_BUT_FROZEN",
  "GENERATED",
  "HUMAN_CONTROLLED",
  "COORDINATOR_ONLY",
] as const;

export type OwnershipClass = (typeof ownershipClasses)[number];

export const contractStates = ["FROZEN", "MUTABLE_WITH_OWNER", "HUMAN_CONTROLLED"] as const;
export type ContractState = (typeof contractStates)[number];

export const stopCodes = [
  "AMBIGUOUS_AUTHORITY",
  "CONFLICTING_REQUIREMENTS",
  "ARCHITECTURE_CHANGE_REQUIRED",
  "PUBLIC_CONTRACT_CHANGE_REQUIRED",
  "SCHEMA_CHANGE_REQUIRED",
  "MIGRATION_REQUIRED",
  "DESTRUCTIVE_OPERATION",
  "SECRET_REQUIRED",
  "PROVIDER_CHANGE_REQUIRED",
  "HUMAN_DECISION_REQUIRED",
  "HUMAN_GATE_REQUIRED",
  "UNEXPECTED_DIRTY_TREE",
  "SHARED_RESOURCE_COLLISION",
  "OUT_OF_SCOPE_CHANGE_REQUIRED",
  "TEST_BASELINE_BROKEN",
] as const;

export type StopCode = (typeof stopCodes)[number];

export const resultStatuses = ["PASS", "FAIL", "BLOCKED", "HUMAN_REVIEW_REQUIRED"] as const;
export type ResultStatus = (typeof resultStatuses)[number];

export const retryClasses = [
  "TRANSIENT_FAILURE",
  "IMPLEMENTATION_FAILURE",
  "POLICY_FAILURE",
  "AUTHORITY_FAILURE",
  "TEST_FAILURE",
  "RESOURCE_COLLISION",
] as const;

export type RetryClass = (typeof retryClasses)[number];

export interface AuthorityEnvelope {
  readonly references: readonly string[];
  readonly grants: readonly string[];
  readonly negativeScope: readonly string[];
}

export interface ContractRequirement {
  readonly contractId: string;
  readonly state: ContractState;
  readonly owner: string;
}

export interface CommandEvidence {
  readonly commandId: string;
  readonly exitCode: number;
  readonly durationMs: number;
  readonly result: "PASS" | "FAIL" | "BLOCKED";
  readonly relevantOutput: readonly string[];
}

export interface AgentResult {
  readonly schemaVersion: 1;
  readonly status: ResultStatus;
  readonly summary: string;
  readonly changedFiles: readonly string[];
  readonly commands: readonly CommandEvidence[];
  readonly tests: readonly CommandEvidence[];
  readonly evidence: readonly string[];
  readonly risks: readonly string[];
  readonly blockers: readonly string[];
  readonly requestedAuthority: readonly string[];
  readonly stopCondition: StopCode | null;
}

export interface ExecutionSurface {
  readonly cwd: string;
  readonly writableRoots: readonly string[];
  readonly sandbox: "read-only" | "workspace-write";
  readonly approvalPolicy: "never";
  readonly networkAccess: false;
  readonly environmentPolicy: "minimal";
  readonly tools: readonly string[];
  readonly mcpServers: readonly string[];
  readonly skills: readonly string[];
}

export interface CandidateEvidence {
  readonly commitId: string;
  readonly treeId: string;
  readonly changedFiles: readonly string[];
}

export interface TaskDefinition {
  readonly taskId: string;
  readonly taskKind: TaskKind;
  readonly title: string;
  readonly objective: string;
  readonly authority: AuthorityEnvelope;
  readonly executionSurface: ExecutionSurface;
  readonly owner: AgentId;
  readonly status: TaskStatus;
  readonly priority: number;
  readonly dependencies: readonly string[];
  readonly blockedBy: readonly string[];
  readonly allowedPaths: readonly string[];
  readonly forbiddenPaths: readonly string[];
  readonly ownership: OwnershipClass;
  readonly sharedResources: readonly string[];
  readonly requiredContracts: readonly ContractRequirement[];
  readonly acceptanceCriteria: readonly string[];
  readonly requiredTests: readonly string[];
  readonly stopConditions: readonly StopCode[];
  readonly deliverables: readonly string[];
  readonly worktree: string | null;
  readonly branch: string | null;
  readonly parallelism: ParallelismClass;
  readonly requiresIndependentReview: boolean;
  readonly requiresSecurityReview: boolean;
  readonly humanGate: boolean;
  readonly candidateTaskId: string | null;
  readonly candidate: CandidateEvidence | null;
  readonly maxAttempts: number;
  readonly createdAt: string;
  readonly startedAt: string | null;
  readonly finishedAt: string | null;
  readonly result: AgentResult | null;
  readonly evidence: readonly string[];
}

export interface ProjectPlan {
  readonly schemaVersion: 1;
  readonly project: "RAG-Challenge";
  readonly baseline: string;
  readonly maxConcurrency: number;
  readonly tasks: readonly TaskDefinition[];
}

export interface AttemptRecord {
  readonly attemptId: string;
  readonly taskId: string;
  readonly agentId: AgentId;
  readonly startedAt: string;
  readonly finishedAt: string | null;
  readonly retryClass: RetryClass | null;
  readonly threadId: string | null;
  readonly result: AgentResult | null;
}

export interface PersistedRunState {
  readonly schemaVersion: 1;
  readonly runId: string;
  readonly revision: number;
  readonly baseline: string;
  readonly maxConcurrency: number;
  readonly createdAt: string;
  readonly updatedAt: string;
  readonly tasks: readonly TaskDefinition[];
  readonly attempts: readonly AttemptRecord[];
  readonly heldLocks: readonly string[];
  readonly humanGateReached: boolean;
}

export interface AgentRunRequest {
  readonly runId: string;
  readonly attemptId: string;
  readonly task: TaskDefinition;
  readonly baseline: string;
  readonly contracts: readonly ContractRequirement[];
  readonly candidate: CandidateEvidence | null;
  readonly resumeThreadId: string | null;
  readonly checkpointThread: (threadId: string) => Promise<void>;
}

export interface AgentRunResponse {
  readonly result: AgentResult;
  readonly threadId: string | null;
}

export interface AgentRunner {
  run(request: AgentRunRequest, signal?: AbortSignal): Promise<AgentRunResponse>;
}

export interface ThreadCheckpoint {
  readonly schemaVersion: 1;
  readonly runId: string;
  readonly taskId: string;
  readonly attemptId: string;
  readonly agentId: AgentId;
  readonly threadId: string;
  readonly startedAt: string;
}
