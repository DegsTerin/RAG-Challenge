// Purpose: Integrates one reviewed candidate sequentially after independently revalidating baseline, identity, tree and changed paths.
import type { AgentResult } from "../core/contracts.js";
import { OrchestratorStop } from "../core/errors.js";
import type { IntegrationExecutor, IntegrationRequest } from "../ports/integration-executor.js";
import type { StructuredProcessExecutor } from "../ports/process-executor.js";
import { assertAbsoluteExecutable, gitArguments, gitEnvironment } from "../security/git-process-policy.js";
import { assertSafeGitAttributes, assertSafeGitRepositoryConfiguration } from "../security/git-repository-policy.js";

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

  public async integrate(request: IntegrationRequest, signal?: AbortSignal) {
    const { integrationTask, implementationTask, candidate, workerResult } = request;
    if (implementationTask.status !== "IMPLEMENTED" || workerResult.status !== "PASS" || implementationTask.candidate?.commitId !== candidate.commitId) {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Candidate integration requires one trusted implemented candidate.", integrationTask.taskId);
    }
    if (implementationTask.requiresIndependentReview) assertPassingReview("Independent review", request.independentReview);
    if (implementationTask.requiresSecurityReview) assertPassingReview("Security review", request.securityReview);
    const root = integrationTask.executionSurface.cwd;
    await assertSafeGitRepositoryConfiguration(this.process, this.gitExecutable, root, this.environment, signal);
    await assertSafeGitAttributes(this.process, this.gitExecutable, root, this.environment, signal);
    const head = await this.git(root, "integration-head", ["rev-parse", "--verify", "HEAD"], signal);
    const status = await this.git(root, "integration-pre-status", ["status", "--porcelain=v1", "-z", "--untracked-files=all"], signal);
    const branch = await this.git(root, "integration-branch", ["branch", "--show-current"], signal);
    if (head.evidence.result !== "PASS" || head.stdout.trim() !== request.expectedCoordinatorHead || status.evidence.result !== "PASS" || status.stdout.length !== 0 ||
        branch.evidence.result !== "PASS" || !branch.stdout.trim().startsWith("codex/") || branch.stdout.trim() === "main") {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "The coordinator worktree is not the clean integration baseline.", integrationTask.taskId);
    }
    const ancestry = await this.git(root, "integration-candidate-ancestry", ["merge-base", "--is-ancestor", request.baseline, candidate.commitId], signal);
    const commitCount = await this.git(root, "integration-candidate-count", ["rev-list", "--count", `${request.baseline}..${candidate.commitId}`], signal);
    const tree = await this.git(root, "integration-candidate-tree", ["rev-parse", "--verify", `${candidate.commitId}^{tree}`], signal);
    const diff = await this.git(root, "integration-candidate-diff", ["diff", "--no-ext-diff", "--no-textconv", "--name-only", "-z", "--diff-filter=ACDMRTUXB", `${request.baseline}..${candidate.commitId}`, "--"], signal);
    const candidatePatch = await this.git(root, "integration-candidate-patch", ["diff", "--binary", "--no-ext-diff", "--no-textconv", `${request.baseline}..${candidate.commitId}`, "--"], signal);
    if (ancestry.evidence.result !== "PASS" || commitCount.evidence.result !== "PASS" || commitCount.stdout.trim() !== "1" || tree.evidence.result !== "PASS" || tree.stdout.trim() !== candidate.treeId ||
        diff.evidence.result !== "PASS" || candidatePatch.evidence.result !== "PASS" ||
        nulPaths(diff.stdout).join("\u0000") !== [...candidate.changedFiles].sort().join("\u0000")) {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "Candidate identity changed after review.", integrationTask.taskId);
    }
    const integration = await this.git(root, "integration-cherry-pick", ["cherry-pick", "--no-edit", candidate.commitId], signal);
    if (integration.evidence.result !== "PASS") {
      const abort = await this.git(root, "integration-abort", ["cherry-pick", "--abort"], signal);
      if (abort.evidence.result !== "PASS") throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "Integration failed and abort could not restore the baseline.", integrationTask.taskId);
      throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", "Candidate integration conflicted and was rolled back.", integrationTask.taskId);
    }
    const postStatus = await this.git(root, "integration-post-status", ["status", "--porcelain=v1", "-z", "--untracked-files=all"], signal);
    const postHead = await this.git(root, "integration-post-head", ["rev-parse", "--verify", "HEAD"], signal);
    const postTree = await this.git(root, "integration-post-tree", ["rev-parse", "--verify", "HEAD^{tree}"], signal);
    const postCount = await this.git(root, "integration-post-count", ["rev-list", "--count", `${request.expectedCoordinatorHead}..HEAD`], signal);
    const postDiff = await this.git(root, "integration-post-diff", ["diff", "--no-ext-diff", "--no-textconv", "--name-only", "-z", "--diff-filter=ACDMRTUXB", `${request.expectedCoordinatorHead}..HEAD`, "--"], signal);
    const postPatch = await this.git(root, "integration-post-patch", ["diff", "--binary", "--no-ext-diff", "--no-textconv", `${request.expectedCoordinatorHead}..HEAD`, "--"], signal);
    if (postStatus.evidence.result !== "PASS" || postStatus.stdout.length !== 0 || postHead.evidence.result !== "PASS" ||
        !/^[0-9a-f]{40}$/.test(postHead.stdout.trim()) || postTree.evidence.result !== "PASS" || !/^[0-9a-f]{40}$/.test(postTree.stdout.trim()) ||
        postCount.evidence.result !== "PASS" || postCount.stdout.trim() !== "1" || postDiff.evidence.result !== "PASS" || postPatch.evidence.result !== "PASS" ||
        nulPaths(postDiff.stdout).join("\u0000") !== [...candidate.changedFiles].sort().join("\u0000") || postPatch.stdout !== candidatePatch.stdout) {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "Integrated commit does not preserve the reviewed candidate patch.", integrationTask.taskId);
    }
    return {
      evidence: integration.evidence,
      candidate: { commitId: postHead.stdout.trim(), treeId: postTree.stdout.trim(), changedFiles: candidate.changedFiles },
    };
  }

  private async git(cwd: string, commandId: string, arguments_: readonly string[], signal?: AbortSignal) {
    await assertSafeGitRepositoryConfiguration(this.process, this.gitExecutable, cwd, this.environment, signal);
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
