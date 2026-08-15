// Purpose: Builds a fixed, non-interactive Git execution policy that disables local hooks, helpers, prompts and external protocols.
import { isAbsolute } from "node:path";
import { OrchestratorStop } from "../core/errors.js";

const inheritedNames = ["SystemRoot", "WINDIR", "TEMP", "TMP", "USERPROFILE", "HOME", "LOCALAPPDATA", "APPDATA"] as const;

export const gitConfigurationArguments = [
  "-c", "core.hooksPath=NUL",
  "-c", "core.fsmonitor=false",
  "-c", "commit.gpgSign=false",
  "-c", "tag.gpgSign=false",
  "-c", "credential.helper=",
  "-c", "core.askPass=",
  "-c", "core.pager=cat",
  "-c", "diff.external=",
  "-c", "protocol.file.allow=never",
  "-c", "protocol.http.allow=never",
  "-c", "protocol.https.allow=never",
  "-c", "protocol.ssh.allow=never",
  "-c", "protocol.git.allow=never",
  "-c", "protocol.ext.allow=never",
] as const;

export function assertAbsoluteExecutable(path: string, label: string): void {
  if (!isAbsolute(path)) {
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", `${label} must be an absolute executable path.`);
  }
}

export function gitEnvironment(source: Readonly<Record<string, string | undefined>>): Readonly<Record<string, string>> {
  const environment: Record<string, string> = {
    GIT_CONFIG_NOSYSTEM: "1",
    GIT_CONFIG_GLOBAL: process.platform === "win32" ? "NUL" : "/dev/null",
    GIT_TERMINAL_PROMPT: "0",
    GCM_INTERACTIVE: "Never",
    GIT_PAGER: "cat",
  };
  for (const name of inheritedNames) {
    const value = source[name];
    if (value !== undefined) environment[name] = value;
  }
  return environment;
}

export function gitArguments(arguments_: readonly string[]): readonly string[] {
  return [...gitConfigurationArguments, ...arguments_];
}
