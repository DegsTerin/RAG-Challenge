// Purpose: Enforces sequential evidence, review, ownership and quality checks around coordinator-owned candidate integration.
import type { AgentResult, CommandEvidence, TaskDefinition } from "../core/contracts.js";
import { OrchestratorStop } from "../core/errors.js";
import type { ProcessExecutor } from "../ports/process-executor.js";
import type { QualityGate } from "../adapters/quality-gate.js";

export interface IntegrationEvidence {
  readonly integration: CommandEvidence;
  readonly qualityGate: CommandEvidence;
}

function assertPassingReview(name: string, result: AgentResult | null): void {
  if (result === null || result.status !== "PASS" || result.changedFiles.length > 0) {
    throw new OrchestratorStop("TEST_BASELINE_BROKEN", `${name} did not produce a read-only PASS result.`);
  }
}

export class SequentialIntegrationPipeline {
  public constructor(
    private readonly process: ProcessExecutor,
    private readonly qualityGate: QualityGate,
    private readonly environment: Readonly<Record<string, string>>,
  ) {}

  public async integrate(
    coordinatorRoot: string,
    task: TaskDefinition,
    candidateCommit: string,
    workerResult: AgentResult,
    independentReview: AgentResult | null,
    securityReview: AgentResult | null,
    signal?: AbortSignal,
  ): Promise<IntegrationEvidence> {
    if (task.status !== "INTEGRATION_READY" || workerResult.status !== "PASS") {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Candidate integration requires worker PASS and INTEGRATION_READY state.", task.taskId);
    }
    if (task.requiresIndependentReview) {
      assertPassingReview("Independent review", independentReview);
    }
    if (task.requiresSecurityReview) {
      assertPassingReview("Security review", securityReview);
    }
    if (!/^[0-9a-f]{40}$/.test(candidateCommit)) {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "Candidate commit identity is not a full Git object identifier.", task.taskId);
    }
    const status = await this.git(coordinatorRoot, "integration-pre-status", ["status", "--porcelain"], signal);
    const branch = await this.git(coordinatorRoot, "integration-branch", ["branch", "--show-current"], signal);
    if (status.result !== "PASS" || status.relevantOutput.length !== 0 || branch.result !== "PASS") {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "The coordinator worktree is not a clean integration baseline.", task.taskId);
    }
    const branchName = branch.relevantOutput.at(-1) ?? "";
    if (!branchName.startsWith("codex/") || branchName === "main") {
      throw new OrchestratorStop("HUMAN_DECISION_REQUIRED", "Integration is permitted only on an isolated codex/ coordinator branch.", task.taskId);
    }
    const integration = await this.git(coordinatorRoot, "integration-cherry-pick", ["cherry-pick", "--no-edit", candidateCommit], signal);
    if (integration.result !== "PASS") {
      const abort = await this.git(coordinatorRoot, "integration-abort", ["cherry-pick", "--abort"], signal);
      if (abort.result !== "PASS") {
        throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "Candidate integration failed and recovery could not restore the coordinator.", task.taskId);
      }
      throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", "Candidate integration conflicted and was rolled back.", task.taskId);
    }
    const qualityGate = await this.qualityGate.run(coordinatorRoot, signal);
    if (qualityGate.result !== "PASS") {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "The canonical quality gate failed after integration.", task.taskId);
    }
    return { integration, qualityGate };
  }

  private async git(cwd: string, commandId: string, arguments_: readonly string[], signal?: AbortSignal): Promise<CommandEvidence> {
    return await this.process.run({
      commandId,
      executable: "git",
      arguments: arguments_,
      cwd,
      environment: this.environment,
      timeoutMs: 300_000,
      maximumOutputBytes: 1_048_576,
      maximumRelevantLines: 1024,
    }, signal);
  }
}
