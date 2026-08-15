// Purpose: Verifies the Codex App Server JSONL handshake, ChatGPT auth gate, pre-turn identity and fail-closed server requests.
import assert from "node:assert/strict";
import test from "node:test";
import { CodexAppServerClient, type CodexAppServerTransport } from "../src/adapters/codex-app-server.js";
import { OrchestratorStop } from "../src/core/errors.js";
import { passingResult } from "./helpers.js";

type Message = Record<string, unknown>;

class ScriptedTransport implements CodexAppServerTransport {
  public readonly sent: Message[] = [];
  private lineListener: ((line: string) => void) | null = null;
  private failureListener: ((error: Error) => void) | null = null;

  public constructor(private readonly handle: (message: Message, transport: ScriptedTransport) => void) {}

  public async send(line: string): Promise<void> {
    const message = JSON.parse(line) as Message;
    this.sent.push(message);
    this.handle(message, this);
  }

  public onLine(listener: (line: string) => void): void {
    this.lineListener = listener;
  }

  public onFailure(listener: (error: Error) => void): void {
    this.failureListener = listener;
  }

  public async close(): Promise<void> {}

  public emit(message: Message): void {
    queueMicrotask(() => this.lineListener?.(JSON.stringify(message)));
  }

  public emitRaw(line: string): void {
    queueMicrotask(() => this.lineListener?.(line));
  }

  public fail(error: Error): void {
    queueMicrotask(() => this.failureListener?.(error));
  }
}

function response(transport: ScriptedTransport, request: Message, result: unknown): void {
  transport.emit({ id: request.id, result });
}

test("App Server returns and checkpoints a durable thread before a bounded turn can start", async () => {
  const transport = new ScriptedTransport((message, scripted) => {
    if (message.method === "initialize") response(scripted, message, { userAgent: "fixture", codexHome: "C:/fixture", platformFamily: "windows", platformOs: "windows" });
    if (message.method === "account/read") response(scripted, message, { account: { type: "chatgpt", email: null, planType: "plus" }, requiresOpenaiAuth: true });
    if (message.method === "thread/start") response(scripted, message, { thread: { id: "thread-fixture" } });
    if (message.method === "turn/start") {
      response(scripted, message, { turn: { id: "turn-fixture" } });
      scripted.emit({ method: "item/completed", params: { threadId: "thread-fixture", turnId: "turn-fixture", item: { type: "agentMessage", id: "message-fixture", text: JSON.stringify(passingResult()) } } });
      scripted.emit({ method: "turn/completed", params: { threadId: "thread-fixture", turn: { id: "turn-fixture", status: "completed" } } });
    }
  });
  const client = new CodexAppServerClient(transport);
  const configuration = { workingDirectory: "C:/repository", sandbox: "read-only" as const, model: null };
  const threadId = await client.startThread(configuration, "task-fixture");
  assert.equal(threadId, "thread-fixture");
  assert.equal(transport.sent.some((message) => message.method === "turn/start"), false);
  const result = await client.runTurn(threadId, "bounded prompt", { ...configuration, outputSchema: {} }, "task-fixture");
  assert.deepEqual(JSON.parse(result), passingResult());
  const methods = transport.sent.map((message) => message.method);
  assert.deepEqual(methods.slice(0, 4), ["initialize", "initialized", "account/read", "thread/start"]);
  const thread = transport.sent.find((message) => message.method === "thread/start")?.params as Record<string, unknown>;
  assert.equal("runtimeWorkspaceRoots" in thread, false);
  assert.equal("selectedCapabilityRoots" in thread, false);
  const turn = transport.sent.find((message) => message.method === "turn/start")?.params as Record<string, unknown>;
  assert.equal(turn.approvalPolicy, "never");
  assert.equal("environments" in thread, false);
  assert.equal("dynamicTools" in thread, false);
  assert.equal("environments" in turn, false);
  assert.deepEqual(turn.sandboxPolicy, { type: "readOnly", networkAccess: false });
  assert.equal("runtimeWorkspaceRoots" in turn, false);
});

test("App Server rejects API-key authentication without exposing credential data", async () => {
  const transport = new ScriptedTransport((message, scripted) => {
    if (message.method === "initialize") response(scripted, message, {});
    if (message.method === "account/read") response(scripted, message, { account: { type: "apiKey" }, requiresOpenaiAuth: true });
  });
  const client = new CodexAppServerClient(transport);
  await assert.rejects(client.assertChatGptSession(), (error: unknown) =>
    error instanceof OrchestratorStop && error.code === "SECRET_REQUIRED" && !error.message.includes("sk-"));
});

test("App Server rejects synthetic secret-shaped prompts before transport", async () => {
  const transport = new ScriptedTransport((message, scripted) => {
    if (message.method === "initialize") response(scripted, message, {});
    if (message.method === "account/read") response(scripted, message, { account: { type: "chatgpt", email: null, planType: "plus" }, requiresOpenaiAuth: true });
    if (message.method === "thread/start") response(scripted, message, { thread: { id: "thread-fixture" } });
  });
  const client = new CodexAppServerClient(transport);
  const configuration = { workingDirectory: "C:/repository", sandbox: "read-only" as const, model: null };
  const threadId = await client.startThread(configuration, "task-fixture");
  for (const synthetic of ["sk-proj-synthetic-not-a-real-secret", "OPENAI_API_KEY"]) {
    await assert.rejects(
      client.runTurn(threadId, synthetic, { ...configuration, outputSchema: {} }, "task-fixture"),
      (error: unknown) => error instanceof OrchestratorStop && error.code === "SECRET_REQUIRED" && !error.message.includes(synthetic),
    );
  }
  assert.equal(transport.sent.some((message) => message.method === "turn/start"), false);
});

test("App Server denies approval and user-input requests and fails the active turn", async () => {
  const transport = new ScriptedTransport((message, scripted) => {
    if (message.method === "initialize") response(scripted, message, {});
    if (message.method === "account/read") response(scripted, message, { account: { type: "chatgpt", email: null, planType: "plus" }, requiresOpenaiAuth: true });
    if (message.method === "thread/start") response(scripted, message, { thread: { id: "thread-fixture" } });
    if (message.method === "turn/start") {
      response(scripted, message, { turn: { id: "turn-fixture" } });
      scripted.emit({ id: 99, method: "item/tool/requestUserInput", params: { threadId: "thread-fixture", turnId: "turn-fixture", itemId: "item-fixture", questions: [] } });
    }
  });
  const client = new CodexAppServerClient(transport);
  const configuration = { workingDirectory: "C:/repository", sandbox: "read-only" as const, model: null };
  const threadId = await client.startThread(configuration, "task-fixture");
  await assert.rejects(client.runTurn(threadId, "bounded prompt", { ...configuration, outputSchema: {} }, "task-fixture"), (error: unknown) =>
    error instanceof OrchestratorStop && error.code === "OUT_OF_SCOPE_CHANGE_REQUIRED");
  assert.ok(transport.sent.some((message) => message.id === 99 && message.error !== undefined));
});

test("App Server rejects malformed protocol output and transport failure", async () => {
  const malformed = new ScriptedTransport((message, scripted) => {
    if (message.method === "initialize") scripted.emitRaw("{not-json}");
  });
  await assert.rejects(new CodexAppServerClient(malformed).assertChatGptSession(), (error: unknown) =>
    error instanceof OrchestratorStop && error.code === "TEST_BASELINE_BROKEN");

  const failed = new ScriptedTransport((message, scripted) => {
    if (message.method === "initialize") scripted.fail(new Error("bounded process failure"));
  });
  await assert.rejects(new CodexAppServerClient(failed).assertChatGptSession(), /bounded process failure/);
});

test("App Server rejects secret-bearing protocol fields without echoing them", async () => {
  const synthetic = "OPENAI_API_KEY";
  const transport = new ScriptedTransport((message, scripted) => {
    if (message.method === "initialize") response(scripted, message, { note: synthetic });
  });
  await assert.rejects(
    new CodexAppServerClient(transport).assertChatGptSession(),
    (error: unknown) => error instanceof OrchestratorStop && error.code === "SECRET_REQUIRED" && !error.message.includes(synthetic),
  );
});

test("App Server bounds unanswered protocol requests", async () => {
  const silent = new ScriptedTransport(() => undefined);
  await assert.rejects(new CodexAppServerClient(silent, 5).assertChatGptSession(), (error: unknown) =>
    error instanceof OrchestratorStop && error.code === "TEST_BASELINE_BROKEN" && /timed out/i.test(error.message));
});
