// Purpose: Rejects repository-local Git configuration that could execute code under the coordinator identity.
import { OrchestratorStop } from "../core/errors.js";
import type { StructuredProcessExecutor } from "../ports/process-executor.js";
import { gitArguments } from "./git-process-policy.js";

const deniedConfiguration = [
  /^filter\./i,
  /^diff\..*\.(?:command|textconv)$/i,
  /^merge\..*\.driver$/i,
  /^core\.(?:hookspath|fsmonitor|sshcommand|editor)$/i,
  /^credential\./i,
  /^include(?:if)?\./i,
  /^url\..*\.(?:insteadof|pushinsteadof)$/i,
  /^submodule\..*\.update$/i,
  /^gpg\.program$/i,
  /^(?:sequence|core)\.editor$/i,
  /^(?:diff|merge)tool\./i,
] as const;

function names(stdout: string): readonly string[] {
  return stdout.split("\u0000").filter((name) => name.length > 0);
}

function assertNamesSafe(values: readonly string[]): void {
  if (values.some((name) => deniedConfiguration.some((pattern) => pattern.test(name)))) {
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "Repository-local Git configuration exceeds the non-executable policy.");
  }
}

export async function assertSafeGitRepositoryConfiguration(
  process: StructuredProcessExecutor,
  gitExecutable: string,
  cwd: string,
  environment: Readonly<Record<string, string>>,
  signal?: AbortSignal,
): Promise<void> {
  const inspect = async (scope: "--local" | "--worktree") => await process.runStructured({
    commandId: `git-config-policy-${scope.slice(2)}`,
    executable: gitExecutable,
    arguments: gitArguments(["config", "--null", scope, "--includes", "--name-only", "--list"]),
    cwd,
    environment,
    timeoutMs: 30_000,
    maximumOutputBytes: 262_144,
    maximumRelevantLines: 64,
  }, signal);
  const local = await inspect("--local");
  if (local.evidence.result !== "PASS") {
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "Repository-local Git configuration could not be inspected safely.");
  }
  const localNames = names(local.stdout);
  assertNamesSafe(localNames);
  if (localNames.some((name) => name.toLowerCase() === "extensions.worktreeconfig")) {
    const worktree = await inspect("--worktree");
    if (worktree.evidence.result !== "PASS") {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "Worktree Git configuration could not be inspected safely.");
    }
    assertNamesSafe(names(worktree.stdout));
  }
}

export async function assertSafeGitAttributes(
  process: StructuredProcessExecutor,
  gitExecutable: string,
  cwd: string,
  environment: Readonly<Record<string, string>>,
  signal?: AbortSignal,
): Promise<void> {
  const tracked = await process.runStructured({
    commandId: "git-attribute-tracked-files",
    executable: gitExecutable,
    arguments: gitArguments(["ls-files", "-z"]),
    cwd,
    environment,
    timeoutMs: 60_000,
    maximumOutputBytes: 4_194_304,
    maximumRelevantLines: 64,
  }, signal);
  if (tracked.evidence.result !== "PASS") throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "Tracked files could not be inspected for Git attributes.");
  const files = tracked.stdout.split("\u0000").filter((path) => path.length > 0);
  if (files.length > 100_000) throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The tracked-file set exceeds the bounded attribute policy.");
  for (let index = 0; index < files.length; index += 128) {
    const result = await process.runStructured({
      commandId: "git-attribute-policy",
      executable: gitExecutable,
      arguments: gitArguments(["check-attr", "-z", "filter", "diff", "merge", "--", ...files.slice(index, index + 128)]),
      cwd,
      environment,
      timeoutMs: 60_000,
      maximumOutputBytes: 1_048_576,
      maximumRelevantLines: 64,
    }, signal);
    const tokens = result.stdout.split("\u0000").filter((value) => value.length > 0);
    if (result.evidence.result !== "PASS" || tokens.length % 3 !== 0) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "Git attributes could not be inspected safely.");
    }
    for (let token = 2; token < tokens.length; token += 3) {
      if (!["unspecified", "unset"].includes(tokens[token] ?? "")) {
        throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "Executable or custom Git attributes are not permitted.");
      }
    }
  }
}
