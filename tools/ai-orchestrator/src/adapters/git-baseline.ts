// Purpose: Verifies the exact clean Git baseline through an absolute, non-interactive and configuration-isolated Git executable.
import { OrchestratorStop } from "../core/errors.js";
import type { StructuredProcessExecutor } from "../ports/process-executor.js";
import { assertAbsoluteExecutable, gitArguments, gitEnvironment } from "../security/git-process-policy.js";
import { assertSafeGitAttributes, assertSafeGitRepositoryConfiguration } from "../security/git-repository-policy.js";

export class GitBaselineVerifier {
  private readonly environment: Readonly<Record<string, string>>;

  public constructor(
    private readonly process: StructuredProcessExecutor,
    private readonly gitExecutable: string,
    environment: Readonly<Record<string, string | undefined>>,
  ) {
    assertAbsoluteExecutable(gitExecutable, "Git executable");
    this.environment = gitEnvironment(environment);
  }

  public async verify(repositoryRoot: string, expectedHead: string, signal?: AbortSignal): Promise<void> {
    await assertSafeGitRepositoryConfiguration(this.process, this.gitExecutable, repositoryRoot, this.environment, signal);
    await assertSafeGitAttributes(this.process, this.gitExecutable, repositoryRoot, this.environment, signal);
    const head = await this.git(repositoryRoot, "baseline-head", ["rev-parse", "HEAD"], signal);
    const branch = await this.git(repositoryRoot, "baseline-branch", ["branch", "--show-current"], signal);
    const status = await this.git(repositoryRoot, "baseline-status", ["status", "--porcelain=v1", "-z", "--untracked-files=all"], signal);
    if (head.evidence.result !== "PASS" || head.stdout.trim() !== expectedHead || branch.evidence.result !== "PASS" ||
        !branch.stdout.trim().startsWith("codex/") || status.evidence.result !== "PASS" || status.stdout.length !== 0) {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "Git HEAD, branch or working tree differs from the execution baseline.");
    }
  }

  private async git(cwd: string, commandId: string, arguments_: readonly string[], signal?: AbortSignal) {
    await assertSafeGitRepositoryConfiguration(this.process, this.gitExecutable, cwd, this.environment, signal);
    return await this.process.runStructured({
      commandId,
      executable: this.gitExecutable,
      arguments: gitArguments(arguments_),
      cwd,
      environment: this.environment,
      timeoutMs: 120_000,
      maximumOutputBytes: 1_048_576,
      maximumRelevantLines: 256,
    }, signal);
  }
}
