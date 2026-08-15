// Purpose: Verifies filesystem recovery, lock ownership, Codex policy mapping, bounded subprocesses and sequential integration safety.
import assert from "node:assert/strict";
import { mkdtemp, readFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join, resolve } from "node:path";
import test from "node:test";
import { CodexRunner, type CodexClientFactory } from "../src/adapters/codex-runner.js";
import { BoundedProcess } from "../src/adapters/bounded-process.js";
import { FileResourceLocks } from "../src/adapters/file-resource-locks.js";
import { FileStateStore } from "../src/adapters/file-state-store.js";
import { GitBaselineVerifier } from "../src/adapters/git-baseline.js";
import { SequentialIntegrationPipeline } from "../src/application/integration.js";
import type { CommandEvidence, PersistedRunState } from "../src/core/contracts.js";
import { OrchestratorStop } from "../src/core/errors.js";
import type { ProcessExecutor, ProcessRequest } from "../src/ports/process-executor.js";
import type { QualityGate } from "../src/adapters/quality-gate.js";
import { baseline, instant, passingResult, task } from "./helpers.js";

async function temporaryDirectory(name: string): Promise<string> {
  return await mkdtemp(join(tmpdir(), `${name}-`));
}

function state(runId = "run-fixture", revision = 0): PersistedRunState {
  return {
    schemaVersion: 1,
    runId,
    revision,
    baseline,
    createdAt: instant,
    updatedAt: instant,
    tasks: [task({ status: "PASS", startedAt: instant, finishedAt: instant, result: passingResult() })],
    attempts: [],
    heldLocks: [],
    humanGateReached: false,
  };
}

test("file state store round-trips snapshots and detects interrupted writes", async () => {
  const root = await temporaryDirectory("orchestrator-state");
  try {
    const store = new FileStateStore(root);
    await store.save(state());
    assert.deepEqual(await store.load("run-fixture"), state());
    await writeFile(join(root, "run-fixture", "state-00000001.json.tmp"), "partial", "utf8");
    await assert.rejects(store.load("run-fixture"), (error: unknown) =>
      error instanceof OrchestratorStop && error.code === "TEST_BASELINE_BROKEN");
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test("file state journal rejects digest tampering", async () => {
  const root = await temporaryDirectory("orchestrator-journal");
  try {
    const store = new FileStateStore(root);
    await store.save(state());
    const journalPath = join(root, "run-fixture", "journal.jsonl");
    const journal = await readFile(journalPath, "utf8");
    await writeFile(journalPath, journal.replace(/"stateHash":"[0-9a-f]+"/, '"stateHash":"0000"'), "utf8");
    await assert.rejects(store.load("run-fixture"), /does not match its journal digest/);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test("resource locks are atomic and cannot be released by a foreign attempt", async () => {
  const root = await temporaryDirectory("orchestrator-locks");
  try {
    const first = new FileResourceLocks(root);
    const second = new FileResourceLocks(root);
    const owner = { runId: "run-fixture", taskId: "task-fixture", attemptId: "attempt-fixture", acquiredAt: instant };
    await first.acquire("sqlite:test", owner);
    await assert.rejects(second.acquire("sqlite:test", { ...owner, attemptId: "attempt-other" }), (error: unknown) =>
      error instanceof OrchestratorStop && error.code === "SHARED_RESOURCE_COLLISION");
    await assert.rejects(first.release("sqlite:test", owner.runId, "attempt-other"), /not owned/);
    await first.release("sqlite:test", owner.runId, owner.attemptId);
    assert.deepEqual(await first.inspect(), []);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test("CodexRunner is disabled before SDK construction without separate execution authority", async () => {
  let constructed = false;
  const factory: CodexClientFactory = () => {
    constructed = true;
    throw new Error("The SDK client must not be constructed.");
  };
  const runner = new CodexRunner({ executionAuthorised: false, authorityReference: null, worktreeRoot: "C:/managed", environment: { PATH: "C:/bin" }, model: null }, factory);
  await assert.rejects(runner.run({
    runId: "run-fixture", attemptId: "attempt-fixture", task: task(), baseline, contracts: [], resumeThreadId: null,
  }), (error: unknown) => error instanceof OrchestratorStop && error.code === "HUMAN_DECISION_REQUIRED");
  assert.equal(constructed, false);
});

test("CodexRunner maps isolated cwd, sandbox and structured output for start and resume", async () => {
  const calls: { method: string; options: unknown; turnOptions?: unknown }[] = [];
  const thread = {
    id: "thread-fixture",
    async run(_input: string, turnOptions?: unknown) {
      calls.push({ method: "run", options: null, turnOptions });
      return { finalResponse: JSON.stringify(passingResult()) };
    },
  };
  const factory: CodexClientFactory = () => ({
    startThread(options) { calls.push({ method: "start", options }); return thread; },
    resumeThread(_id, options) { calls.push({ method: "resume", options }); return thread; },
  });
  const runner = new CodexRunner({
    executionAuthorised: true,
    authorityReference: "separate-test-authority",
    worktreeRoot: "C:/managed",
    environment: { PATH: "C:/bin", SystemRoot: "C:/Windows" },
    model: null,
  }, factory);
  const agentTask = task({ worktree: "C:/managed/lane-a" });
  await runner.run({ runId: "run-fixture", attemptId: "attempt-fixture", task: agentTask, baseline, contracts: [], resumeThreadId: null });
  await runner.run({ runId: "run-fixture", attemptId: "attempt-fixture", task: agentTask, baseline, contracts: [], resumeThreadId: "thread-fixture" });
  const threadOptions = calls.find((call) => call.method === "start")?.options as Record<string, unknown>;
  assert.equal(threadOptions.sandboxMode, "workspace-write");
  assert.equal(threadOptions.approvalPolicy, "never");
  assert.equal(threadOptions.networkAccessEnabled, false);
  assert.equal(threadOptions.webSearchMode, "disabled");
  assert.ok(calls.some((call) => call.method === "resume"));
  assert.ok(calls.find((call) => call.method === "run")?.turnOptions !== undefined);
});

test("CodexRunner rejects secret-shaped explicit environment names", async () => {
  const factory: CodexClientFactory = () => { throw new Error("not reached"); };
  const runner = new CodexRunner({
    executionAuthorised: true, authorityReference: "test", worktreeRoot: "C:/managed",
    environment: { OPENAI_API_KEY: "prohibited" }, model: null,
  }, factory);
  await assert.rejects(runner.run({
    runId: "run-fixture", attemptId: "attempt-fixture", task: task({ worktree: "C:/managed/lane" }), baseline, contracts: [], resumeThreadId: null,
  }), (error: unknown) => error instanceof OrchestratorStop && error.code === "SECRET_REQUIRED");
});

test("bounded process uses argv execution and captures a bounded result", async () => {
  const result = await new BoundedProcess().run({
    commandId: "node-fixture",
    executable: process.execPath,
    arguments: ["-e", "process.stdout.write('fixture-output')"],
    cwd: resolve("."),
    environment: {},
    timeoutMs: 10_000,
    maximumOutputBytes: 4096,
  });
  assert.equal(result.result, "PASS");
  assert.deepEqual(result.relevantOutput, ["fixture-output"]);
});

class ScriptedProcess implements ProcessExecutor {
  public readonly requests: ProcessRequest[] = [];
  public constructor(private readonly results: CommandEvidence[]) {}
  public async run(request: ProcessRequest): Promise<CommandEvidence> {
    this.requests.push(request);
    const result = this.results.shift();
    if (result === undefined) {
      throw new Error("Missing scripted process result.");
    }
    return result;
  }
}

function command(commandId: string, result: "PASS" | "FAIL" = "PASS", output: readonly string[] = []): CommandEvidence {
  return { commandId, exitCode: result === "PASS" ? 0 : 1, durationMs: 1, result, relevantOutput: output };
}

test("Git baseline verification requires exact HEAD, codex branch and a clean tree", async () => {
  const passing = new ScriptedProcess([
    command("head", "PASS", [baseline]),
    command("branch", "PASS", ["codex/coordinator"]),
    command("status"),
  ]);
  await new GitBaselineVerifier(passing, {}).verify("C:/repository", baseline);
  const dirty = new ScriptedProcess([
    command("head", "PASS", [baseline]),
    command("branch", "PASS", ["codex/coordinator"]),
    command("status", "PASS", ["?? owner-file"]),
  ]);
  await assert.rejects(new GitBaselineVerifier(dirty, {}).verify("C:/repository", baseline), (error: unknown) =>
    error instanceof OrchestratorStop && error.code === "UNEXPECTED_DIRTY_TREE");
});

test("sequential integration requires reviews, a clean codex branch and a passing quality gate", async () => {
  const process = new ScriptedProcess([
    command("status"),
    command("branch", "PASS", ["codex/integration"]),
    command("cherry-pick"),
  ]);
  const quality: QualityGate = { async run() { return command("quality"); } };
  const pipeline = new SequentialIntegrationPipeline(process, quality, {});
  const implementation = task({
    taskId: "implementation", owner: "implementation_worker", status: "INTEGRATION_READY",
    requiresIndependentReview: true, requiresSecurityReview: true,
  });
  const evidence = await pipeline.integrate(
    "C:/coordinator", implementation, baseline, passingResult(["tools/file.ts"]), passingResult(), passingResult(),
  );
  assert.equal(evidence.qualityGate.result, "PASS");
  assert.deepEqual(process.requests.map((request) => request.arguments[0]), ["status", "branch", "cherry-pick"]);
});

test("failed integration is aborted and reported as a conflict", async () => {
  const process = new ScriptedProcess([
    command("status"),
    command("branch", "PASS", ["codex/integration"]),
    command("cherry-pick", "FAIL"),
    command("abort"),
  ]);
  const quality: QualityGate = { async run() { throw new Error("Quality gate must not run."); } };
  const pipeline = new SequentialIntegrationPipeline(process, quality, {});
  await assert.rejects(pipeline.integrate(
    "C:/coordinator",
    task({ taskId: "implementation", owner: "implementation_worker", status: "INTEGRATION_READY" }),
    baseline,
    passingResult(),
    null,
    null,
  ), (error: unknown) => error instanceof OrchestratorStop && error.code === "CONFLICTING_REQUIREMENTS");
  assert.equal(process.requests.at(-1)?.arguments[1], "--abort");
});
