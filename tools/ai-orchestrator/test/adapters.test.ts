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
import { FileThreadCheckpointStore } from "../src/adapters/file-thread-checkpoints.js";
import { GitBaselineVerifier } from "../src/adapters/git-baseline.js";
import { SequentialIntegrationPipeline } from "../src/application/integration.js";
import type { CommandEvidence, PersistedRunState } from "../src/core/contracts.js";
import { OrchestratorStop } from "../src/core/errors.js";
import type { ProcessRequest, StructuredProcessExecutor, StructuredProcessResult } from "../src/ports/process-executor.js";
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
    maxConcurrency: 3,
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
    await assert.rejects(store.load("run-fixture"), /state journal entry failed its closed contract/i);
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

test("thread checkpoints persist before a turn and require exact ownership for removal", async () => {
  const root = await temporaryDirectory("orchestrator-thread");
  try {
    const store = new FileThreadCheckpointStore(join(root, "state"));
    const checkpoint = { schemaVersion: 1 as const, runId: "run-fixture", taskId: "task-fixture", attemptId: "attempt-fixture", agentId: "code_mapper" as const, threadId: "thread-fixture", startedAt: instant };
    await store.save(checkpoint);
    assert.deepEqual(await store.load(checkpoint.runId, checkpoint.taskId), checkpoint);
    await assert.rejects(store.remove(checkpoint.runId, checkpoint.taskId, "attempt-other"), /ownership changed/);
    await store.remove(checkpoint.runId, checkpoint.taskId, checkpoint.attemptId);
    assert.deepEqual(await store.inspect(checkpoint.runId), []);
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
    runId: "run-fixture", attemptId: "attempt-fixture", task: task(), baseline, contracts: [], candidate: null, resumeThreadId: null,
    checkpointThread: async () => undefined,
  }), (error: unknown) => error instanceof OrchestratorStop && error.code === "HUMAN_DECISION_REQUIRED");
  assert.equal(constructed, false);
});

test("CodexRunner maps isolated cwd, sandbox and structured output for start and resume", async () => {
  const calls: { method: string; options: unknown; turnOptions?: unknown }[] = [];
  const thread = {
    id: "thread-fixture",
    async runStreamed(_input: string, turnOptions?: unknown) {
      calls.push({ method: "run", options: null, turnOptions });
      return { events: (async function* () {
        yield { type: "thread.started", thread_id: "thread-fixture" };
        yield { type: "item.completed", item: { type: "agent_message", text: JSON.stringify(passingResult()) } };
        yield { type: "turn.completed" };
      })() };
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
  const agentTask = task({ owner: "implementation_worker", worktree: "C:/managed/lane-a", branch: "codex/lane-a" });
  await runner.run({ runId: "run-fixture", attemptId: "attempt-fixture", task: agentTask, baseline, contracts: [], candidate: null, resumeThreadId: null, checkpointThread: async () => undefined });
  await runner.run({ runId: "run-fixture", attemptId: "attempt-fixture", task: agentTask, baseline, contracts: [], candidate: null, resumeThreadId: "thread-fixture", checkpointThread: async () => undefined });
  const threadOptions = calls.find((call) => call.method === "start")?.options as Record<string, unknown>;
  assert.equal(threadOptions.sandboxMode, "workspace-write");
  assert.equal(threadOptions.approvalPolicy, "never");
  assert.equal(threadOptions.networkAccessEnabled, false);
  assert.equal(threadOptions.webSearchMode, "disabled");
  assert.ok(calls.some((call) => call.method === "resume"));
  assert.ok(calls.find((call) => call.method === "run")?.turnOptions !== undefined);
});

test("CodexRunner rejects explicit environment names outside its allowlist", async () => {
  const factory: CodexClientFactory = () => { throw new Error("not reached"); };
  const runner = new CodexRunner({
    executionAuthorised: true, authorityReference: "test", worktreeRoot: "C:/managed",
    environment: { UNSAFE_VARIABLE: "fixture" }, model: null,
  }, factory);
  await assert.rejects(runner.run({
    runId: "run-fixture", attemptId: "attempt-fixture", task: task({ worktree: "C:/managed/lane" }), baseline, contracts: [], candidate: null, resumeThreadId: null,
    checkpointThread: async () => undefined,
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

class ScriptedProcess implements StructuredProcessExecutor {
  public readonly requests: ProcessRequest[] = [];
  public constructor(private readonly results: StructuredProcessResult[]) {}
  public async run(request: ProcessRequest): Promise<CommandEvidence> {
    return (await this.runStructured(request)).evidence;
  }
  public async runStructured(request: ProcessRequest): Promise<StructuredProcessResult> {
    this.requests.push(request);
    if (request.arguments.includes("config") && request.arguments.includes("--name-only")) {
      return structured("git-config-policy");
    }
    if (request.arguments.includes("ls-files") || request.arguments.includes("check-attr")) {
      return structured("git-attribute-policy");
    }
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

function structured(commandId: string, stdout = "", result: "PASS" | "FAIL" = "PASS"): StructuredProcessResult {
  return { evidence: command(commandId, result), stdout, stderr: "" };
}

test("Git baseline verification requires exact HEAD, codex branch and a clean tree", async () => {
  const passing = new ScriptedProcess([
    structured("head", `${baseline}\n`),
    structured("branch", "codex/coordinator\n"),
    structured("status"),
  ]);
  await new GitBaselineVerifier(passing, "C:/git.exe", {}).verify("C:/repository", baseline);
  const dirty = new ScriptedProcess([
    structured("head", `${baseline}\n`),
    structured("branch", "codex/coordinator\n"),
    structured("status", "?? owner-file\u0000"),
  ]);
  await assert.rejects(new GitBaselineVerifier(dirty, "C:/git.exe", {}).verify("C:/repository", baseline), (error: unknown) =>
    error instanceof OrchestratorStop && error.code === "UNEXPECTED_DIRTY_TREE");
});

test("sequential integration revalidates the reviewed candidate and integrated tree", async () => {
  const commit = "1".repeat(40);
  const tree = "2".repeat(40);
  const process = new ScriptedProcess([
    structured("head", `${baseline}\n`), structured("status"), structured("branch", "codex/integration\n"),
    structured("ancestry"), structured("count", "1\n"), structured("tree", `${tree}\n`), structured("diff", "tools/file.ts\u0000"), structured("candidate-patch", "patch"),
    structured("cherry-pick"), structured("post-status"), structured("post-head", `${commit}\n`), structured("post-tree", `${tree}\n`),
    structured("post-count", "1\n"), structured("post-diff", "tools/file.ts\u0000"), structured("post-patch", "patch"),
  ]);
  const pipeline = new SequentialIntegrationPipeline(process, "C:/git.exe", {});
  const candidate = { commitId: commit, treeId: tree, changedFiles: ["tools/file.ts"] };
  const implementation = task({
    taskId: "implementation", owner: "implementation_worker", status: "IMPLEMENTED", candidate,
    result: passingResult(["tools/file.ts"]), requiresIndependentReview: true, requiresSecurityReview: true,
  });
  const integrationTask = task({ taskId: "integration", taskKind: "INTEGRATION", owner: "governance_guard", candidateTaskId: "implementation", executionSurface: { ...task().executionSurface, cwd: "C:/coordinator", tools: [] } });
  const evidence = await pipeline.integrate({ baseline, expectedCoordinatorHead: baseline, integrationTask, implementationTask: implementation, candidate, workerResult: implementation.result!, independentReview: passingResult(), securityReview: passingResult() });
  assert.equal(evidence.evidence.result, "PASS");
  assert.ok(process.requests.every((request) => request.executable === "C:/git.exe" && request.arguments.some((argument) => argument.startsWith("core.hooksPath="))));
});

test("failed integration is aborted and reported as a conflict", async () => {
  const commit = "1".repeat(40);
  const tree = "2".repeat(40);
  const process = new ScriptedProcess([
    structured("head", `${baseline}\n`), structured("status"), structured("branch", "codex/integration\n"),
    structured("ancestry"), structured("count", "1\n"), structured("tree", `${tree}\n`), structured("diff", "tools/file.ts\u0000"), structured("candidate-patch", "patch"),
    structured("cherry-pick", "", "FAIL"), structured("abort"),
  ]);
  const pipeline = new SequentialIntegrationPipeline(process, "C:/git.exe", {});
  const candidate = { commitId: commit, treeId: tree, changedFiles: ["tools/file.ts"] };
  const implementation = task({ taskId: "implementation", owner: "implementation_worker", status: "IMPLEMENTED", candidate, result: passingResult(["tools/file.ts"]) });
  const integrationTask = task({ taskId: "integration", taskKind: "INTEGRATION", owner: "governance_guard", candidateTaskId: "implementation", executionSurface: { ...task().executionSurface, cwd: "C:/coordinator", tools: [] } });
  await assert.rejects(pipeline.integrate({ baseline, expectedCoordinatorHead: baseline, integrationTask, implementationTask: implementation, candidate, workerResult: implementation.result!, independentReview: null, securityReview: null }),
    (error: unknown) => error instanceof OrchestratorStop && error.code === "CONFLICTING_REQUIREMENTS");
  assert.ok(process.requests.at(-1)?.arguments.includes("--abort"));
});
