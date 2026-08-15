// Purpose: Executes fixed absolute binaries without a shell while bounding environment, time, output and descendant lifetime.
import { spawn, type ChildProcess } from "node:child_process";
import { isAbsolute, join } from "node:path";
import type { CommandEvidence } from "../core/contracts.js";
import { OrchestratorStop } from "../core/errors.js";
import type { ProcessRequest, StructuredProcessExecutor, StructuredProcessResult } from "../ports/process-executor.js";

const permittedEnvironmentNames = new Set([
  "PATH", "PATHEXT", "SystemRoot", "WINDIR", "TEMP", "TMP", "USERPROFILE", "HOME", "LOCALAPPDATA", "APPDATA",
  "DOTNET_CLI_HOME", "NUGET_PACKAGES", "NUGET_HTTP_CACHE_PATH", "NPM_CONFIG_CACHE", "NPM_CONFIG_OFFLINE",
  "GIT_CONFIG_NOSYSTEM", "GIT_CONFIG_GLOBAL", "GIT_TERMINAL_PROMPT", "GCM_INTERACTIVE", "GIT_PAGER",
  "GIT_ATTR_NOSYSTEM",
]);

function sanitiseLine(value: string): string {
  return value
    .replace(/file:\/\/\/[^\s\r\n\0]+/gi, "<path>")
    .replace(/(?:[A-Za-z]:[\\/]|\\\\|\/\/)[^\s\r\n\0]+/g, "<path>")
    .replace(/(?:^|\s)\/(?:home|Users|var|tmp)\/[^\s\r\n\0]+/g, " <path>")
    .replace(/(?:^|\s)(?:[^\s=]*(?:KEY|SECRET|TOKEN|PASSWORD)[^\s=]*)=[^\s]+/gi, " <redacted-assignment>")
    .replace(/\b(?:sk|sk-proj)-[A-Za-z0-9_-]{8,}\b/g, "<redacted-token>")
    .trim();
}

function validateRequest(request: ProcessRequest): void {
  if (!isAbsolute(request.executable) || !isAbsolute(request.cwd)) {
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "Subprocess executable and cwd must be absolute paths.");
  }
  if (!Number.isInteger(request.timeoutMs) || request.timeoutMs < 1 || request.timeoutMs > 7_200_000 ||
      !Number.isInteger(request.maximumOutputBytes) || request.maximumOutputBytes < 1 || request.maximumOutputBytes > 16_777_216 ||
      (request.maximumRelevantLines !== undefined && (!Number.isInteger(request.maximumRelevantLines) || request.maximumRelevantLines < 1 || request.maximumRelevantLines > 4096))) {
    throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", "Subprocess bounds are outside the supported limits.");
  }
  for (const [name, value] of Object.entries(request.environment)) {
    if (!permittedEnvironmentNames.has(name) || value.length > 32_768 || /[\u0000\r\n]/.test(value)) {
      throw new OrchestratorStop("SECRET_REQUIRED", `Subprocess environment name '${name}' is not in the closed operational allowlist.`);
    }
  }
  if (request.arguments.some((argument) => argument.includes("\u0000"))) {
    throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", "Subprocess arguments cannot contain NUL characters.");
  }
}

async function terminateTree(child: ChildProcess, environment: Readonly<Record<string, string>>): Promise<boolean> {
  if (child.pid === undefined) return true;
  if (process.platform !== "win32") {
    try {
      process.kill(-child.pid, "SIGKILL");
      return true;
    } catch {
      return child.kill("SIGKILL");
    }
  }
  const systemRoot = environment.SystemRoot ?? environment.WINDIR;
  if (systemRoot === undefined || !isAbsolute(systemRoot)) return child.kill();
  const taskkill = join(systemRoot, "System32", "taskkill.exe");
  return await new Promise<boolean>((resolve) => {
    const killer = spawn(taskkill, ["/PID", String(child.pid), "/T", "/F"], {
      shell: false,
      windowsHide: true,
      stdio: "ignore",
      env: { SystemRoot: systemRoot, WINDIR: systemRoot },
    });
    killer.once("error", () => resolve(child.kill()));
    killer.once("close", (code) => resolve(code === 0 || child.killed));
  });
}

export class BoundedProcess implements StructuredProcessExecutor {
  public async run(request: ProcessRequest, signal?: AbortSignal): Promise<CommandEvidence> {
    return (await this.runStructured(request, signal)).evidence;
  }

  public async runStructured(request: ProcessRequest, signal?: AbortSignal): Promise<StructuredProcessResult> {
    validateRequest(request);
    const started = Date.now();
    return await new Promise<StructuredProcessResult>((resolvePromise, reject) => {
      const child = spawn(request.executable, [...request.arguments], {
        cwd: request.cwd,
        env: { ...request.environment },
        shell: false,
        windowsHide: true,
        detached: process.platform !== "win32",
        stdio: ["ignore", "pipe", "pipe"],
      });
      const stdout: Buffer[] = [];
      const stderr: Buffer[] = [];
      let bytes = 0;
      let termination: "PROCESS_TIMEOUT" | "OUTPUT_LIMIT_EXCEEDED" | "PROCESS_CANCELLED" | "PROCESS_TREE_TERMINATION_FAILED" | null = null;
      let terminationStarted = false;
      let terminationPromise: Promise<void> | null = null;
      let settled = false;

      const stop = (reason: Exclude<typeof termination, "PROCESS_TREE_TERMINATION_FAILED" | null>): Promise<void> => {
        if (terminationStarted) return terminationPromise ?? Promise.resolve();
        terminationStarted = true;
        termination = reason;
        terminationPromise = (async () => {
          if (!(await terminateTree(child, request.environment))) termination = "PROCESS_TREE_TERMINATION_FAILED";
        })();
        return terminationPromise;
      };
      const collect = (target: Buffer[], chunk: Buffer): void => {
        bytes += chunk.length;
        if (bytes <= request.maximumOutputBytes) target.push(Buffer.from(chunk));
        else void stop("OUTPUT_LIMIT_EXCEEDED");
      };
      child.stdout.on("data", (chunk: Buffer) => collect(stdout, chunk));
      child.stderr.on("data", (chunk: Buffer) => collect(stderr, chunk));
      child.once("error", (error) => {
        if (!settled) {
          settled = true;
          clearTimeout(timer);
          signal?.removeEventListener("abort", abort);
          reject(error);
        }
      });
      const timer = setTimeout(() => { void stop("PROCESS_TIMEOUT"); }, request.timeoutMs);
      const abort = (): void => { void stop("PROCESS_CANCELLED"); };
      signal?.addEventListener("abort", abort, { once: true });
      child.once("close", async (code) => {
        if (settled) return;
        settled = true;
        clearTimeout(timer);
        signal?.removeEventListener("abort", abort);
        await terminationPromise;
        const stdoutText = Buffer.concat(stdout).toString("utf8");
        const stderrText = Buffer.concat(stderr).toString("utf8");
        const maximumLines = request.maximumRelevantLines ?? 128;
        const lines = `${stdoutText}\n${stderrText}`.split(/\r?\n/).map(sanitiseLine).filter((line) => line.length > 0).slice(-maximumLines);
        if (termination !== null) lines.unshift(termination);
        const evidence: CommandEvidence = {
          commandId: request.commandId,
          exitCode: code ?? -1,
          durationMs: Date.now() - started,
          result: termination !== null ? "BLOCKED" : code === 0 ? "PASS" : "FAIL",
          relevantOutput: lines,
        };
        resolvePromise({ evidence, stdout: stdoutText, stderr: stderrText });
      });
    });
  }
}
