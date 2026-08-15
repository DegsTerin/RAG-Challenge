// Purpose: Owns the bounded Codex App Server JSONL protocol, ChatGPT-session authentication gate and pre-turn thread identity boundary.
import { spawn, type ChildProcessWithoutNullStreams } from "node:child_process";
import { createRequire } from "node:module";
import { OrchestratorStop } from "../core/errors.js";
import { parseSecureJson } from "../security/secure-json.js";

const maximumProtocolLineBytes = 1_048_576;
const maximumStderrBytes = 65_536;
const defaultRequestTimeoutMs = 30_000;

type JsonObject = Readonly<Record<string, unknown>>;

export interface CodexThreadConfiguration {
  readonly workingDirectory: string;
  readonly sandbox: "read-only" | "workspace-write";
  readonly model: string | null;
}

export interface CodexTurnConfiguration extends CodexThreadConfiguration {
  readonly outputSchema: JsonObject;
}

export interface CodexAppServer {
  assertChatGptSession(): Promise<void>;
  startThread(configuration: CodexThreadConfiguration, taskId: string): Promise<string>;
  resumeThread(threadId: string, configuration: CodexThreadConfiguration, taskId: string): Promise<string>;
  runTurn(threadId: string, prompt: string, configuration: CodexTurnConfiguration, taskId: string, signal?: AbortSignal): Promise<string>;
  close(): Promise<void>;
}

export interface CodexAppServerTransport {
  send(line: string): Promise<void>;
  onLine(listener: (line: string) => void): void;
  onFailure(listener: (error: Error) => void): void;
  close(): Promise<void>;
}

export interface CodexAppServerLaunchPolicy {
  readonly environment: Readonly<Record<string, string>>;
}

interface PendingRequest {
  readonly resolve: (result: unknown) => void;
  readonly reject: (error: Error) => void;
  readonly timeout: NodeJS.Timeout;
}

interface TurnState {
  finalResponse: string | null;
  completed: boolean;
  failure: Error | null;
  waiter: { readonly resolve: (value: string) => void; readonly reject: (error: Error) => void } | null;
}

function object(value: unknown, label: string): JsonObject {
  if (value === null || typeof value !== "object" || Array.isArray(value)) {
    throw new OrchestratorStop("TEST_BASELINE_BROKEN", `${label} is not an object.`);
  }
  return value as JsonObject;
}

function nonEmptyString(value: unknown, label: string): string {
  if (typeof value !== "string" || value.length === 0) {
    throw new OrchestratorStop("TEST_BASELINE_BROKEN", `${label} is not a non-empty string.`);
  }
  return value;
}

class SpawnedCodexTransport implements CodexAppServerTransport {
  private readonly process: ChildProcessWithoutNullStreams;
  private readonly lineListeners: Array<(line: string) => void> = [];
  private readonly failureListeners: Array<(error: Error) => void> = [];
  private stdoutBuffer = Buffer.alloc(0);
  private stderrBytes = 0;
  private failed = false;
  private closing = false;

  public constructor(policy: CodexAppServerLaunchPolicy) {
    const codexCliPath = createRequire(import.meta.url).resolve("@openai/codex/bin/codex.js");
    this.process = spawn(process.execPath, [
      codexCliPath,
      "app-server",
      "--listen",
      "stdio://",
      "--disable", "apps",
      "--disable", "browser_use",
      "--disable", "computer_use",
      "--disable", "image_generation",
      "--disable", "in_app_browser",
      "--disable", "multi_agent",
      "--disable", "plugins",
      "--disable", "remote_plugin",
      "--disable", "recommended_plugins",
      "--disable", "skill_mcp_dependency_install",
      "--disable", "skill_search",
      "--disable", "tool_suggest",
      "--disable", "workspace_dependencies",
      "-c",
      'web_search="disabled"',
      "-c",
      "mcp_servers={}",
    ], {
      cwd: process.cwd(),
      env: { ...policy.environment },
      shell: false,
      windowsHide: true,
      stdio: ["pipe", "pipe", "pipe"],
    });
    this.process.stdout.on("data", (chunk: Buffer) => this.acceptStdout(chunk));
    this.process.stderr.on("data", (chunk: Buffer) => {
      this.stderrBytes += chunk.length;
      if (this.stderrBytes > maximumStderrBytes) this.fail(new Error("Codex App Server exceeded the bounded stderr allowance."));
    });
    this.process.on("error", () => this.fail(new Error("Codex App Server could not be started.")));
    this.process.on("exit", (code, signal) => {
      if (!this.failed && !this.closing) this.fail(new Error(`Codex App Server exited unexpectedly (${signal === null ? `code ${String(code)}` : "signal"}).`));
    });
  }

  public onLine(listener: (line: string) => void): void {
    this.lineListeners.push(listener);
  }

  public onFailure(listener: (error: Error) => void): void {
    this.failureListeners.push(listener);
  }

  public async send(line: string): Promise<void> {
    if (this.failed || this.process.stdin.destroyed || Buffer.byteLength(line, "utf8") > maximumProtocolLineBytes) {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Codex App Server transport is unavailable or the request is oversized.");
    }
    await new Promise<void>((resolvePromise, rejectPromise) => {
      this.process.stdin.write(`${line}\n`, "utf8", (error) => error === null || error === undefined ? resolvePromise() : rejectPromise(error));
    });
  }

  public async close(): Promise<void> {
    this.closing = true;
    if (!this.process.killed) this.process.kill();
  }

  private acceptStdout(chunk: Buffer): void {
    this.stdoutBuffer = Buffer.concat([this.stdoutBuffer, chunk]);
    if (this.stdoutBuffer.length > maximumProtocolLineBytes && this.stdoutBuffer.indexOf(0x0a) < 0) {
      this.fail(new Error("Codex App Server emitted an oversized protocol line."));
      return;
    }
    let delimiter = this.stdoutBuffer.indexOf(0x0a);
    while (delimiter >= 0) {
      const rawLine = this.stdoutBuffer.subarray(0, delimiter);
      this.stdoutBuffer = this.stdoutBuffer.subarray(delimiter + 1);
      if (rawLine.length > maximumProtocolLineBytes) {
        this.fail(new Error("Codex App Server emitted an oversized protocol line."));
        return;
      }
      const lineBytes = rawLine.at(-1) === 0x0d ? rawLine.subarray(0, -1) : rawLine;
      try {
        const line = new TextDecoder("utf-8", { fatal: true }).decode(lineBytes);
        if (line.length > 0) this.lineListeners.forEach((listener) => listener(line));
      } catch {
        this.fail(new Error("Codex App Server emitted invalid UTF-8."));
        return;
      }
      delimiter = this.stdoutBuffer.indexOf(0x0a);
    }
  }

  private fail(error: Error): void {
    if (this.failed) return;
    this.failed = true;
    if (!this.process.killed) this.process.kill();
    this.failureListeners.forEach((listener) => listener(error));
  }
}

export class CodexAppServerClient implements CodexAppServer {
  private nextRequestId = 1;
  private readonly pending = new Map<number, PendingRequest>();
  private readonly turns = new Map<string, TurnState>();
  private terminalError: Error | null = null;
  private initialised: Promise<void> | null = null;

  public constructor(
    private readonly transport: CodexAppServerTransport,
    private readonly requestTimeoutMilliseconds = defaultRequestTimeoutMs,
  ) {
    transport.onLine((line) => this.acceptLine(line));
    transport.onFailure((error) => this.fail(error));
  }

  public assertChatGptSession(): Promise<void> {
    this.initialised ??= this.initialiseAndAuthorise();
    return this.initialised;
  }

  public async startThread(configuration: CodexThreadConfiguration, taskId: string): Promise<string> {
    await this.assertChatGptSession();
    const result = object(await this.request("thread/start", this.threadParameters(configuration)), "thread/start result");
    return this.threadIdentity(result, taskId, "thread/start");
  }

  public async resumeThread(threadId: string, configuration: CodexThreadConfiguration, taskId: string): Promise<string> {
    await this.assertChatGptSession();
    const result = object(await this.request("thread/resume", { threadId, ...this.threadParameters(configuration) }), "thread/resume result");
    const resumed = this.threadIdentity(result, taskId, "thread/resume");
    if (resumed !== threadId) {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Codex App Server resumed an inconsistent thread identity.", taskId);
    }
    return resumed;
  }

  public async runTurn(
    threadId: string,
    prompt: string,
    configuration: CodexTurnConfiguration,
    taskId: string,
    signal?: AbortSignal,
  ): Promise<string> {
    const result = object(await this.request("turn/start", {
      threadId,
      input: [{ type: "text", text: prompt, text_elements: [] }],
      cwd: configuration.workingDirectory,
      approvalPolicy: "never",
      sandboxPolicy: configuration.sandbox === "read-only"
        ? { type: "readOnly", networkAccess: false }
        : { type: "workspaceWrite", writableRoots: [configuration.workingDirectory], networkAccess: false, excludeTmpdirEnvVar: true, excludeSlashTmp: true },
      ...(configuration.model === null ? {} : { model: configuration.model }),
      outputSchema: configuration.outputSchema,
    }), "turn/start result");
    if (this.terminalError !== null) throw this.terminalError;
    const turnId = nonEmptyString(object(result.turn, "turn/start turn").id, "turn/start turn id");
    const state = this.turns.get(turnId) ?? { finalResponse: null, completed: false, failure: null, waiter: null };
    this.turns.set(turnId, state);
    if (state.failure !== null) throw state.failure;
    if (state.completed) return this.completedResponse(state, taskId);
    return await new Promise<string>((resolvePromise, rejectPromise) => {
      const abort = (): void => {
        state.waiter = null;
        void this.request("turn/interrupt", { threadId, turnId }).catch(() => undefined);
        rejectPromise(new OrchestratorStop("TEST_BASELINE_BROKEN", "Codex turn was interrupted by its bounded deadline.", taskId));
      };
      if (signal?.aborted === true) {
        abort();
        return;
      }
      state.waiter = {
        resolve: (value) => { signal?.removeEventListener("abort", abort); resolvePromise(value); },
        reject: (error) => { signal?.removeEventListener("abort", abort); rejectPromise(error); },
      };
      signal?.addEventListener("abort", abort, { once: true });
      this.settleTurn(turnId, taskId);
    });
  }

  public async close(): Promise<void> {
    await this.transport.close();
  }

  private async initialiseAndAuthorise(): Promise<void> {
    await this.request("initialize", {
      clientInfo: { name: "rag-challenge-ai-orchestrator", title: "RAG-Challenge AI Orchestrator", version: "0.1.0" },
      capabilities: null,
    });
    await this.transport.send(JSON.stringify({ method: "initialized" }));
    const accountResult = object(await this.request("account/read", { refreshToken: false }), "account/read result");
    const account = accountResult.account === null ? null : object(accountResult.account, "account/read account");
    if (account === null || account.type !== "chatgpt" || accountResult.requiresOpenaiAuth !== true) {
      throw new OrchestratorStop(
        "SECRET_REQUIRED",
        "Real Codex execution requires the existing ChatGPT session; API-key and other provider authentication modes are excluded.",
      );
    }
  }

  private threadParameters(configuration: CodexThreadConfiguration): JsonObject {
    return {
      cwd: configuration.workingDirectory,
      approvalPolicy: "never",
      sandbox: configuration.sandbox,
      serviceName: "rag_challenge_ai_orchestrator",
      developerInstructions: "Do not use MCP servers, apps, skills, web search, or network access. Follow the supplied task envelope and output schema.",
      ...(configuration.model === null ? {} : { model: configuration.model }),
    };
  }

  private threadIdentity(result: JsonObject, taskId: string, label: string): string {
    const thread = object(result.thread, `${label} thread`);
    try {
      return nonEmptyString(thread.id, `${label} thread id`);
    } catch {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", `Codex App Server did not return a durable ${label} identity.`, taskId);
    }
  }

  private async request(method: string, params: unknown): Promise<unknown> {
    if (this.terminalError !== null) throw this.terminalError;
    const id = this.nextRequestId++;
    return await new Promise<unknown>((resolvePromise, rejectPromise) => {
      const timeout = setTimeout(() => {
        this.pending.delete(id);
        rejectPromise(new OrchestratorStop("TEST_BASELINE_BROKEN", `Codex App Server request '${method}' timed out.`));
      }, this.requestTimeoutMilliseconds);
      this.pending.set(id, { resolve: resolvePromise, reject: rejectPromise, timeout });
      void this.transport.send(JSON.stringify({ id, method, params })).catch((error: unknown) => {
        clearTimeout(timeout);
        this.pending.delete(id);
        rejectPromise(error instanceof Error ? error : new Error("Codex App Server request failed."));
      });
    });
  }

  private acceptLine(line: string): void {
    try {
      const message = object(parseSecureJson(line, "Codex App Server message"), "Codex App Server message");
      if (typeof message.id === "number" && typeof message.method === "string") {
        void this.transport.send(JSON.stringify({
          id: message.id,
          error: { code: -32_000, message: "Denied by RAG-Challenge orchestrator policy." },
        })).catch(() => undefined);
        this.fail(new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "Codex App Server requested approval, user input or another server-side capability."));
      } else if (typeof message.id === "number") {
        this.acceptResponse(message.id, message);
      } else if (typeof message.method === "string") {
        this.acceptNotification(message.method, object(message.params, `Codex notification '${message.method}'`));
      } else {
        this.fail(new OrchestratorStop("TEST_BASELINE_BROKEN", "Codex App Server emitted an unrecognised protocol message."));
      }
    } catch (error) {
      this.fail(error instanceof Error ? error : new Error("Codex App Server protocol failed."));
    }
  }

  private acceptResponse(id: number, message: JsonObject): void {
    const pending = this.pending.get(id);
    if (pending === undefined) {
      this.fail(new OrchestratorStop("TEST_BASELINE_BROKEN", "Codex App Server returned an unknown request identity."));
      return;
    }
    clearTimeout(pending.timeout);
    this.pending.delete(id);
    if (message.error !== undefined) {
      pending.reject(new OrchestratorStop("TEST_BASELINE_BROKEN", "Codex App Server rejected a protocol request."));
    } else {
      pending.resolve(message.result);
    }
  }

  private acceptNotification(method: string, params: JsonObject): void {
    if (method === "item/completed") {
      const item = object(params.item, "item/completed item");
      if (item.type === "agentMessage" && typeof item.text === "string") {
        const turnId = nonEmptyString(params.turnId, "item/completed turn id");
        const state = this.turns.get(turnId) ?? { finalResponse: null, completed: false, failure: null, waiter: null };
        state.finalResponse = item.text;
        this.turns.set(turnId, state);
      }
      return;
    }
    if (method === "turn/completed") {
      const turn = object(params.turn, "turn/completed turn");
      const turnId = nonEmptyString(turn.id, "turn/completed turn id");
      const state = this.turns.get(turnId) ?? { finalResponse: null, completed: false, failure: null, waiter: null };
      state.completed = true;
      if (turn.status !== "completed") state.failure = new OrchestratorStop("TEST_BASELINE_BROKEN", "Codex App Server reported a failed or interrupted turn.");
      this.turns.set(turnId, state);
      this.settleTurn(turnId, null);
      return;
    }
    if (method === "error") {
      this.fail(new OrchestratorStop("TEST_BASELINE_BROKEN", "Codex App Server emitted an error notification."));
    }
  }

  private settleTurn(turnId: string, taskId: string | null): void {
    const state = this.turns.get(turnId);
    if (state?.waiter === null || state?.waiter === undefined || (!state.completed && state.failure === null)) return;
    const waiter = state.waiter;
    state.waiter = null;
    if (state.failure !== null) waiter.reject(state.failure);
    else {
      try { waiter.resolve(this.completedResponse(state, taskId)); }
      catch (error) { waiter.reject(error instanceof Error ? error : new Error("Codex turn result failed.")); }
    }
  }

  private completedResponse(state: TurnState, taskId: string | null): string {
    if (state.finalResponse === null) {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Codex turn completed without a structured agent message.", taskId);
    }
    return state.finalResponse;
  }

  private fail(error: Error): void {
    if (this.terminalError !== null) return;
    this.terminalError = error;
    for (const pending of this.pending.values()) {
      clearTimeout(pending.timeout);
      pending.reject(error);
    }
    this.pending.clear();
    for (const state of this.turns.values()) {
      state.failure = error;
      if (state.waiter !== null) {
        const waiter = state.waiter;
        state.waiter = null;
        waiter.reject(error);
      }
    }
  }
}

export function createCodexAppServer(policy: CodexAppServerLaunchPolicy): CodexAppServer {
  return new CodexAppServerClient(new SpawnedCodexTransport(policy));
}
