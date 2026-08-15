// Purpose: Derives candidate identity and changed paths from Git so agent-reported evidence cannot define its own scope.
import { OrchestratorStop } from "../core/errors.js";
import type { AgentResult, CandidateEvidence, TaskDefinition } from "../core/contracts.js";
import type { CandidateInspector } from "../ports/candidate-inspector.js";
import type { StructuredProcessExecutor } from "../ports/process-executor.js";
import { assertAbsoluteExecutable, gitArguments, gitEnvironment } from "../security/git-process-policy.js";
import { assertRepositoryPath } from "../core/validation.js";
import { assertSafeGitAttributes, assertSafeGitRepositoryConfiguration } from "../security/git-repository-policy.js";

export class GitCandidateInspector implements CandidateInspector {
  private readonly environment: Readonly<Record<string, string>>;

  public constructor(
    private readonly process: StructuredProcessExecutor,
    private readonly gitExecutable: string,
    environment: Readonly<Record<string, string | undefined>>,
  ) {
    assertAbsoluteExecutable(gitExecutable, "Git executable");
    this.environment = gitEnvironment(environment);
  }

  public async inspect(task: TaskDefinition, baseline: string, _agentResult: AgentResult): Promise<CandidateEvidence> {
    if (task.worktree === null || task.branch === null) {
      throw new OrchestratorStop("AMBIGUOUS_AUTHORITY", "Candidate inspection requires a bound worktree and branch.", task.taskId);
    }
    await assertSafeGitRepositoryConfiguration(this.process, this.gitExecutable, task.worktree, this.environment);
    await assertSafeGitAttributes(this.process, this.gitExecutable, task.worktree, this.environment);
    const head = await this.git(task.worktree, "candidate-head", ["rev-parse", "--verify", "HEAD"]);
    const branch = await this.git(task.worktree, "candidate-branch", ["branch", "--show-current"]);
    const status = await this.git(task.worktree, "candidate-status", ["status", "--porcelain=v1", "-z", "--untracked-files=all"]);
    const commitId = head.stdout.trim();
    if (head.evidence.result !== "PASS" || !/^[0-9a-f]{40}$/.test(commitId) || branch.evidence.result !== "PASS" ||
        branch.stdout.trim() !== task.branch || status.evidence.result !== "PASS" || status.stdout.length !== 0 || commitId === baseline) {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "The candidate worktree is dirty, uncommitted or on the wrong branch.", task.taskId);
    }
    const ancestry = await this.git(task.worktree, "candidate-ancestry", ["merge-base", "--is-ancestor", baseline, commitId]);
    const commitCount = await this.git(task.worktree, "candidate-count", ["rev-list", "--count", `${baseline}..${commitId}`]);
    const tree = await this.git(task.worktree, "candidate-tree", ["rev-parse", "--verify", `${commitId}^{tree}`]);
    const diff = await this.git(task.worktree, "candidate-diff", ["diff", "--no-ext-diff", "--no-textconv", "--name-only", "-z", "--diff-filter=ACDMRTUXB", `${baseline}..${commitId}`, "--"]);
    const treeId = tree.stdout.trim();
    if (ancestry.evidence.result !== "PASS" || commitCount.evidence.result !== "PASS" || commitCount.stdout.trim() !== "1" ||
        tree.evidence.result !== "PASS" || !/^[0-9a-f]{40}$/.test(treeId) || diff.evidence.result !== "PASS") {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "The candidate must be exactly one commit descending from the authorised baseline.", task.taskId);
    }
    const changedFiles = diff.stdout.split("\u0000").filter((path) => path.length > 0).sort();
    if (changedFiles.length === 0 || changedFiles.length > 256) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The candidate diff is empty or exceeds its bounded contract.", task.taskId);
    }
    changedFiles.forEach((path, index) => assertRepositoryPath(path, `candidate.changedFiles[${index}]`));
    if (changedFiles.some((path) => path.split("/").at(-1) === ".gitattributes" || path === ".gitmodules")) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "Candidates cannot change Git attribute or submodule control files.", task.taskId);
    }
    return { commitId, treeId, changedFiles };
  }

  private async git(cwd: string, commandId: string, arguments_: readonly string[]) {
    await assertSafeGitRepositoryConfiguration(this.process, this.gitExecutable, cwd, this.environment);
    return await this.process.runStructured({
      commandId,
      executable: this.gitExecutable,
      arguments: gitArguments(arguments_),
      cwd,
      environment: this.environment,
      timeoutMs: 120_000,
      maximumOutputBytes: 1_048_576,
      maximumRelevantLines: 256,
    });
  }
}
