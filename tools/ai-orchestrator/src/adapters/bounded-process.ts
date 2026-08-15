// Purpose: Runs coordinator-owned executable and argument lists without a shell, inherited environment or unbounded output.
import { spawn } from "node:child_process";
import type { CommandEvidence } from "../core/contracts.js";
import { OrchestratorStop } from "../core/errors.js";
import type { ProcessExecutor, ProcessRequest } from "../ports/process-executor.js";

function sanitisedLines(value: string, maximumLines: number): readonly string[] {
  return value
    .replace(/\b(?:[A-Za-z]:[\\/]|\\\\)[^\r\n"']+/g, "<path>")
    .replace(/\b(?:KEY|SECRET|TOKEN|PASSWORD|CREDENTIAL|CONNECTION)[A-Za-z0-9_]*\s*=\s*[^\s]+/gi, "<redacted>")
    .replace(/(?:sk|pk)-[A-Za-z0-9_-]{8,}/g, "<redacted>")
    .split(/\r?\n/)
    .filter((line) => line.length > 0)
    .slice(-maximumLines)
    .map((line) => line.slice(0, 512));
}

export class BoundedProcess implements ProcessExecutor {
  public async run(request: ProcessRequest, signal?: AbortSignal): Promise<CommandEvidence> {
    const maximumLines = request.maximumRelevantLines ?? 20;
    if (request.timeoutMs < 1 || request.timeoutMs > 7_200_000 || request.maximumOutputBytes < 1024 || request.maximumOutputBytes > 4_194_304 || maximumLines < 1 || maximumLines > 4096) {
      throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", "Process limits are outside the authorised range.");
    }
    if (Object.keys(request.environment).some((name) => /KEY|SECRET|TOKEN|PASSWORD|CREDENTIAL|CONNECTION|OPENAI/i.test(name))) {
      throw new OrchestratorStop("SECRET_REQUIRED", "The process environment contains a prohibited variable name.");
    }
    const started = Date.now();
    return await new Promise<CommandEvidence>((resolvePromise, reject) => {
      const child = spawn(request.executable, [...request.arguments], {
        cwd: request.cwd,
        env: { ...request.environment },
        shell: false,
        windowsHide: true,
        stdio: ["ignore", "pipe", "pipe"],
        signal,
      });
      const chunks: Buffer[] = [];
      let captured = 0;
      let exceeded = false;
      const capture = (chunk: Buffer): void => {
        captured += chunk.length;
        if (captured <= request.maximumOutputBytes) {
          chunks.push(chunk);
        } else {
          exceeded = true;
          child.kill();
        }
      };
      child.stdout.on("data", capture);
      child.stderr.on("data", capture);
      const timeout = setTimeout(() => child.kill(), request.timeoutMs);
      child.once("error", (error) => {
        clearTimeout(timeout);
        reject(error);
      });
      child.once("close", (exitCode) => {
        clearTimeout(timeout);
        const durationMs = Date.now() - started;
        const output = sanitisedLines(Buffer.concat(chunks).toString("utf8"), maximumLines);
        if (exceeded) {
          resolvePromise({ commandId: request.commandId, exitCode: -1, durationMs, result: "BLOCKED", relevantOutput: [...output, "Output limit exceeded."] });
          return;
        }
        const code = exitCode ?? -1;
        resolvePromise({ commandId: request.commandId, exitCode: code, durationMs, result: code === 0 ? "PASS" : "FAIL", relevantOutput: output });
      });
    });
  }
}
