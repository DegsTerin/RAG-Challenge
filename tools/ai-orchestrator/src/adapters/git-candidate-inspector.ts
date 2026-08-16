// Purpose: Derives candidate identity and changed paths from Git so agent-reported evidence cannot define its own scope.
import { OrchestratorStop } from "../core/errors.js";
import type { AgentResult, CandidateEvidence, TaskDefinition } from "../core/contracts.js";
import type { CandidateInspector } from "../ports/candidate-inspector.js";
import type { StructuredProcessExecutor } from "../ports/process-executor.js";
import { assertAbsoluteExecutable, gitArguments, gitEnvironment } from "../security/git-process-policy.js";
import { assertRepositoryPath } from "../core/validation.js";
import { assertSafeGitAttributes, assertSafeGitRepositoryConfiguration } from "../security/git-repository-policy.js";
import { assertBritishCommitMessage, type TrustedLanguagePolicy } from "../security/language-policy.js";
import { readBoundedRegularFile } from "../security/path-policy.js";
import { assertNoSecretShapedText } from "../security/secret-policy.js";

export interface CandidateLanguageChecker {
  check(worktree: string, commitId: string, taskId: string): Promise<void>;
}

const languageControlPaths = new Set([
  ".github/workflows/ci.yml",
  "eng/check-language.mjs",
  "eng/ci.ps1",
  "eng/language-migration-baseline.json",
  "eng/language-migration-baseline.schema.json",
  "eng/language-policy.json",
  "eng/language-policy.schema.json",
  "eng/test-ci-policy.ps1",
  "eng/test-language-policy.mjs",
  "prompts/governance/Language-Policy.md",
  "prompts/governance/Quality-Gates.md",
  "tools/ai-orchestrator/src/adapters/git-candidate-inspector.ts",
  "tools/ai-orchestrator/src/cli-main.ts",
  "tools/ai-orchestrator/src/security/language-policy.ts",
]);

export class TrustedCandidateLanguageChecker implements CandidateLanguageChecker {
  public constructor(
    private readonly process: StructuredProcessExecutor,
    private readonly nodeExecutable: string,
    private readonly checkerPath: string,
    private readonly coordinatorRoot: string,
    private readonly environment: Readonly<Record<string, string>>,
  ) {
    assertAbsoluteExecutable(nodeExecutable, "Node executable");
    assertAbsoluteExecutable(checkerPath, "Trusted language checker");
  }

  public async check(worktree: string, commitId: string, taskId: string): Promise<void> {
    await readBoundedRegularFile(
      this.coordinatorRoot,
      this.checkerPath,
      2_097_152,
      "Trusted language checker",
      "OUT_OF_SCOPE_CHANGE_REQUIRED",
      taskId,
    );
    const result = await this.process.runStructured({
      commandId: "candidate-language",
      executable: this.nodeExecutable,
      arguments: [
        this.checkerPath,
        "--repository-root", worktree,
        "--trusted-policy-root", this.coordinatorRoot,
        "--commit-head", commitId,
      ],
      cwd: this.coordinatorRoot,
      environment: this.environment,
      timeoutMs: 120_000,
      maximumOutputBytes: 1_048_576,
      maximumRelevantLines: 32,
    });
    if (result.evidence.result !== "PASS") {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The candidate failed the trusted coordinator language check.", taskId);
    }
  }
}

export class GitCandidateInspector implements CandidateInspector {
  private readonly environment: Readonly<Record<string, string>>;

  public constructor(
    private readonly process: StructuredProcessExecutor,
    private readonly gitExecutable: string,
    environment: Readonly<Record<string, string | undefined>>,
    private readonly languagePolicy: TrustedLanguagePolicy,
    private readonly languageChecker: CandidateLanguageChecker,
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
    const message = await this.git(task.worktree, "candidate-message", ["show", "-s", "--format=%B", commitId]);
    const treeId = tree.stdout.trim();
    if (ancestry.evidence.result !== "PASS" || commitCount.evidence.result !== "PASS" || commitCount.stdout.trim() !== "1" ||
        tree.evidence.result !== "PASS" || !/^[0-9a-f]{40}$/.test(treeId) || diff.evidence.result !== "PASS" || message.evidence.result !== "PASS") {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "The candidate must be exactly one commit descending from the authorised baseline.", task.taskId);
    }
    if (message.stdout.includes("\uFFFD")) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The candidate commit message is not valid UTF-8.", task.taskId);
    }
    assertNoSecretShapedText(message.stdout, "Candidate commit message");
    assertBritishCommitMessage(message.stdout, this.languagePolicy, task.taskId);
    const changedFiles = diff.stdout.split("\u0000").filter((path) => path.length > 0).sort();
    if (changedFiles.length === 0 || changedFiles.length > 256) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The candidate diff is empty or exceeds its bounded contract.", task.taskId);
    }
    changedFiles.forEach((path, index) => assertRepositoryPath(path, `candidate.changedFiles[${index}]`));
    if (changedFiles.some((path) => path.split("/").at(-1) === ".gitattributes" || path === ".gitmodules")) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "Candidates cannot change Git attribute or submodule control files.", task.taskId);
    }
    if (changedFiles.some((path) => languageControlPaths.has(path))) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "Ordinary candidates cannot change language-enforcement controls.", task.taskId);
    }
    await this.languageChecker.check(task.worktree, commitId, task.taskId);
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
