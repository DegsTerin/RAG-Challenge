// Purpose: Verifies the exact clean Git baseline and coordinator branch before any non-dry-run orchestration begins or resumes.
import type { ProcessExecutor } from "../ports/process-executor.js";
import { OrchestratorStop } from "../core/errors.js";

export class GitBaselineVerifier {
  public constructor(
    private readonly process: ProcessExecutor,
    private readonly environment: Readonly<Record<string, string>>,
  ) {}

  public async verify(repositoryRoot: string, expectedHead: string, signal?: AbortSignal): Promise<void> {
    const head = await this.git(repositoryRoot, "baseline-head", ["rev-parse", "HEAD"], signal);
    const branch = await this.git(repositoryRoot, "baseline-branch", ["branch", "--show-current"], signal);
    const status = await this.git(repositoryRoot, "baseline-status", ["status", "--porcelain", "--untracked-files=all"], signal);
    if (head.result !== "PASS" || head.relevantOutput.at(-1) !== expectedHead || branch.result !== "PASS" ||
        !(branch.relevantOutput.at(-1) ?? "").startsWith("codex/") || status.result !== "PASS" || status.relevantOutput.length !== 0) {
      throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "Git HEAD, branch or working tree differs from the execution baseline.");
    }
  }

  private async git(cwd: string, commandId: string, arguments_: readonly string[], signal?: AbortSignal) {
    return await this.process.run({
      commandId,
      executable: "git",
      arguments: arguments_,
      cwd,
      environment: this.environment,
      timeoutMs: 120_000,
      maximumOutputBytes: 1_048_576,
      maximumRelevantLines: 4096,
    }, signal);
  }
}
