// Purpose: Executes only the repository-owned canonical quality gate and returns individual bounded evidence to the coordinator.
import { join } from "node:path";
import type { CommandEvidence } from "../core/contracts.js";
import type { ProcessExecutor } from "../ports/process-executor.js";
import { assertAbsoluteExecutable } from "../security/git-process-policy.js";

export interface QualityGate {
  run(repositoryRoot: string, signal?: AbortSignal): Promise<CommandEvidence>;
}

export class RepositoryQualityGate implements QualityGate {
  public constructor(
    private readonly process: ProcessExecutor,
    private readonly executableEnvironment: Readonly<Record<string, string>>,
    private readonly powershellExecutable: string,
  ) { assertAbsoluteExecutable(powershellExecutable, "PowerShell executable"); }

  public async run(repositoryRoot: string, signal?: AbortSignal): Promise<CommandEvidence> {
    return await this.process.run({
      commandId: "repository-ci-offline",
      executable: this.powershellExecutable,
      arguments: ["-NoLogo", "-NoProfile", "-NonInteractive", "-File", join(repositoryRoot, "eng", "ci.ps1"), "-Offline"],
      cwd: repositoryRoot,
      environment: this.executableEnvironment,
      timeoutMs: 7_200_000,
      maximumOutputBytes: 4_194_304,
    }, signal);
  }
}
