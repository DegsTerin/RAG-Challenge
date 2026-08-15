// Purpose: Integrates one reviewed candidate sequentially after independently revalidating baseline, identity, tree and changed paths.
import type { AgentResult, CommandEvidence } from "../core/contracts.js";
import { OrchestratorStop } from "../core/errors.js";
import type { IntegrationExecutor, IntegrationRequest } from "../ports/integration-executor.js";
import type { StructuredProcessExecutor } from "../ports/process-executor.js";
import { assertAbsoluteExecutable, gitArguments, gitEnvironment } from "../security/git-process-policy.js";

function assertPassingReview(name: string, result: AgentResult | null): void {
  if (result === null || result.status !== "PASS" || result.changedFiles.length > 0) {
    throw new OrchestratorStop("TEST_BASELINE_BROKEN", `${name} did not produce a read-only PASS result.`);
  }
}

function nulPaths(value: string): readonly string[] {
  return value.split("\u0000").filter((path) => path.length > 0).sort();
}

export class SequentialIntegrationPipeline implements IntegrationExecutor {
  private readonly environment: Readonly<Record<string, string>>;

  public constructor(
    private readonly process: StructuredProcessExecutor,
    private readonly gitExecutable: string,
    environment: Readonly<Record<string, string | undefined>>,
  ) {
    assertAbsoluteExecutable(gitExecutable, "Git executable");
    this.environment = gitEnvironment(environment);
  }

  public async integrate(request: IntegrationRequest, signal?: AbortSignal): Promise<CommandEvidence> {
    const { integrationTask, implementationTask, candidate, workerResult } = request;
    if (implementationTask.status !== "IMPLEMENTED" || workerResult.status !== "PASS" || implementationTask.candidate?.commitId !== candidate.commitId) {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Candidate integration requires one trusted implemented candidate.", integrationTask.taskId);
    }
    if (implementationTask.requiresIndependentReview) assertPassingReview("Independent review", request.independentReview);
    if (implementationTask.requiresSecurityReview) assertPassingReview("Security review", request.securityReview);
    const root = integrationTask.executionSurface.cwd;
    const head = await this.git(root, "integration-head", ["rev-parse", "--verify", "HEAD"], signal);
    const status = await this.git(root, "integration-pre-status", ["status", "--porcelain=v1", "-z", "--untracked-files=all"], signal);
    const branch = await this.git(root, "integration-branch", ["branch", "--show-current"], signal);
    if (head.evidence.result !== "PASS" || head.stdout.trim() !== request.baseline || status.evidence.result !== "PASS" || status.stdout.length !== 0 ||
        branch.evidence.result !== "PASS" || !branch.stdout.trim().startsWith("codex/") || branch.stdout.trim() === "main") {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "The coordinator worktree is not the clean integration baseline.", integrationTask.taskId);
    }
    const ancestry = await this.git(root, "integration-candidate-ancestry", ["merge-base", "--is-ancestor", request.baseline, candidate.commitId], signal);
    const tree = await this.git(root, "integration-candidate-tree", ["rev-parse", "--verify", `${candidate.commitId}^{tree}`], signal);
    const diff = await this.git(root, "integration-candidate-diff", ["diff", "--name-only", "-z", "--diff-filter=ACMRTUXB", `${request.baseline}..${candidate.commitId}`, "--"], signal);
    if (ancestry.evidence.result !== "PASS" || tree.evidence.result !== "PASS" || tree.stdout.trim() !== candidate.treeId ||
        diff.evidence.result !== "PASS" || nulPaths(diff.stdout).join("\u0000") !== [...candidate.changedFiles].sort().join("\u0000")) {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "Candidate identity changed after review.", integrationTask.taskId);
    }
    const integration = await this.git(root, "integration-cherry-pick", ["cherry-pick", "--no-edit", candidate.commitId], signal);
    if (integration.evidence.result !== "PASS") {
      const abort = await this.git(root, "integration-abort", ["cherry-pick", "--abort"], signal);
      if (abort.evidence.result !== "PASS") throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "Integration failed and abort could not restore the baseline.", integrationTask.taskId);
      throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", "Candidate integration conflicted and was rolled back.", integrationTask.taskId);
    }
    const postStatus = await this.git(root, "integration-post-status", ["status", "--porcelain=v1", "-z", "--untracked-files=all"], signal);
    const postTree = await this.git(root, "integration-post-tree", ["rev-parse", "--verify", "HEAD^{tree}"], signal);
    if (postStatus.evidence.result !== "PASS" || postStatus.stdout.length !== 0 || postTree.evidence.result !== "PASS" || postTree.stdout.trim() !== candidate.treeId) {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "Integrated tree does not match the reviewed candidate.", integrationTask.taskId);
    }
    return integration.evidence;
  }

  private async git(cwd: string, commandId: string, arguments_: readonly string[], signal?: AbortSignal) {
    return await this.process.runStructured({
      commandId,
      executable: this.gitExecutable,
      arguments: gitArguments(arguments_),
      cwd,
      environment: this.environment,
      timeoutMs: 300_000,
      maximumOutputBytes: 1_048_576,
      maximumRelevantLines: 256,
    }, signal);
  }
}
