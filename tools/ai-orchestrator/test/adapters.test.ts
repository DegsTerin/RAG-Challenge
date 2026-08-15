// Purpose: Verifies filesystem recovery, lock ownership, Codex policy mapping, bounded subprocesses and sequential integration safety.
import assert from "node:assert/strict";
import { mkdtemp, readFile, rename, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join, resolve } from "node:path";
import test from "node:test";
import { CodexRunner, type CodexAppServerFactory } from "../src/adapters/codex-runner.js";
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
    const prepared = await first.release("sqlite:test", owner.runId, owner.attemptId);
    await first.finalise(prepared, owner);
    assert.deepEqual(await first.inspect(), []);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test("prepared lock finalisation rejects any change to the complete ownership record", async () => {
  const root = await temporaryDirectory("orchestrator-lock-digest");
  try {
    const locks = new FileResourceLocks(root);
    const owner = { runId: "run-fixture", taskId: "task-fixture", attemptId: "attempt-fixture", acquiredAt: instant };
    await locks.acquire("sqlite:test", owner);
    const prepared = await locks.release("sqlite:test", owner.runId, owner.attemptId);
    const path = join(root, prepared.lockId);
    const record = JSON.parse(await readFile(path, "utf8")) as Record<string, unknown>;
    await writeFile(path, JSON.stringify({ ...record, processId: Number(record.processId) + 1 }), "utf8");
    await assert.rejects(locks.finalise(prepared, owner), /ownership changed before finalisation/);
    assert.equal((await readFile(path, "utf8")).length > 0, true);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test("thread checkpoints persist before a turn and require exact ownership for removal", async () => {
  const root = await temporaryDirectory("orchestrator-thread");
  try {
    const store = new FileThreadCheckpointStore(join(root, "state"));
    const checkpoint = {
      schemaVersion: 1 as const,
      runId: "run-fixture",
      taskId: "task-fixture",
      attemptId: "attempt-fixture",
      agentId: "code_mapper" as const,
      taskKind: "DISCOVERY" as const,
      baseline,
      candidateCommitId: null,
      envelopeHash: "a".repeat(64),
      stateRevision: 0,
      deadlineMs: 300_000,
      threadId: "thread-fixture",
      startedAt: instant,
    };
    await store.save(checkpoint);
    assert.deepEqual(await store.load(checkpoint.runId, checkpoint.taskId), checkpoint);
    await assert.rejects(store.remove({ ...checkpoint, attemptId: "attempt-other" }), /ownership changed/);
    await store.remove(checkpoint);
    assert.deepEqual(await store.inspect(checkpoint.runId), []);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test("prepared checkpoint removal is discoverable and can be finalised after a crash", async () => {
  const root = await temporaryDirectory("orchestrator-thread-removal");
  try {
    const stateRoot = join(root, "state");
    const store = new FileThreadCheckpointStore(stateRoot);
    const checkpoint = {
      schemaVersion: 1 as const, runId: "run-fixture", taskId: "task-fixture", attemptId: "attempt-fixture",
      agentId: "code_mapper" as const, taskKind: "DISCOVERY" as const, baseline, candidateCommitId: null,
      envelopeHash: "a".repeat(64), stateRevision: 1, deadlineMs: 300_000, threadId: "thread-fixture", startedAt: instant,
    };
    await store.save(checkpoint);
    const directory = join(stateRoot, checkpoint.runId, "threads");
    await rename(join(directory, `${checkpoint.taskId}.json`), join(directory, `${checkpoint.taskId}.${checkpoint.attemptId}.remove`));
    assert.deepEqual(await store.inspect(checkpoint.runId), []);
    assert.deepEqual(await store.inspectPreparedRemovals(checkpoint.runId), [checkpoint]);
    await store.finalisePreparedRemoval(checkpoint);
    assert.deepEqual(await store.inspectPreparedRemovals(checkpoint.runId), []);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test("CodexRunner is disabled before App Server construction without separate execution authority", async () => {
  let constructed = false;
  const factory: CodexAppServerFactory = () => {
    constructed = true;
    throw new Error("The App Server client must not be constructed.");
  };
  const runner = new CodexRunner({ executionAuthorised: false, authorityReference: null, worktreeRoot: "C:/managed", environment: { PATH: "C:/bin" }, model: null }, factory);
  await assert.rejects(runner.run({
    runId: "run-fixture", attemptId: "attempt-fixture", task: task(), baseline, contracts: [], candidate: null, resumeThreadId: null,
    checkpointThread: async () => undefined,
  }), (error: unknown) => error instanceof OrchestratorStop && error.code === "HUMAN_DECISION_REQUIRED");
  assert.equal(constructed, false);
});

test("CodexRunner rejects an empty declarative authority reference before App Server construction", async () => {
  let constructed = false;
  const runner = new CodexRunner({ executionAuthorised: true, authorityReference: "", worktreeRoot: "C:/managed", environment: {}, model: null }, () => {
    constructed = true;
    throw new Error("not reached");
  });
  await assert.rejects(runner.run({
    runId: "run-fixture", attemptId: "attempt-fixture", task: task(), baseline, contracts: [], candidate: null, resumeThreadId: null,
    checkpointThread: async () => undefined,
  }), (error: unknown) => error instanceof OrchestratorStop && error.code === "HUMAN_DECISION_REQUIRED");
  assert.equal(constructed, false);
});

test("CodexRunner checkpoints a new durable thread identity before the first turn", async () => {
  const calls: string[] = [];
  const runner = new CodexRunner({
    executionAuthorised: true, authorityReference: "AUTH-CODEX-TEST-001", worktreeRoot: "C:/managed",
    environment: { PATH: "C:/bin" }, model: null,
  }, () => ({
    async assertChatGptSession() { calls.push("auth"); },
    async startThread() { calls.push("thread/start"); return "thread-new"; },
    async resumeThread() { throw new Error("not reached"); },
    async runTurn() { calls.push("turn/start"); return JSON.stringify(passingResult()); },
    async close() { calls.push("close"); },
  }));
  const response = await runner.run({
    runId: "run-fixture", attemptId: "attempt-fixture", task: task({ worktree: "C:/managed/lane" }), baseline, contracts: [], candidate: null, resumeThreadId: null,
    checkpointThread: async (threadId) => { assert.equal(threadId, "thread-new"); calls.push("checkpoint"); },
  });
  assert.equal(response.threadId, "thread-new");
  assert.deepEqual(calls, ["auth", "thread/start", "checkpoint", "turn/start"]);
});

test("CodexRunner maps isolated cwd, sandbox and structured output for a persisted resume", async () => {
  const calls: { method: string; value: unknown }[] = [];
  const factory: CodexAppServerFactory = () => ({
    async assertChatGptSession() { calls.push({ method: "auth", value: null }); },
    async startThread() { throw new Error("not reached"); },
    async resumeThread(id, configuration) { calls.push({ method: "resume", value: { id, configuration } }); return id; },
    async runTurn(id, _prompt, configuration) {
      calls.push({ method: "turn", value: { id, configuration } });
      return JSON.stringify(passingResult());
    },
    async close() { calls.push({ method: "close", value: null }); },
  });
  const runner = new CodexRunner({
    executionAuthorised: true,
    authorityReference: "AUTH-CODEX-TEST-002",
    worktreeRoot: "C:/managed",
    environment: { PATH: "C:/bin", SystemRoot: "C:/Windows" },
    model: null,
  }, factory);
  const agentTask = task({ owner: "implementation_worker", worktree: "C:/managed/lane-a", branch: "codex/lane-a" });
  await runner.run({ runId: "run-fixture", attemptId: "attempt-fixture", task: agentTask, baseline, contracts: [], candidate: null, resumeThreadId: "thread-fixture", checkpointThread: async () => undefined });
  const resume = calls.find((call) => call.method === "resume")?.value as { configuration: Record<string, unknown> };
  const turn = calls.find((call) => call.method === "turn")?.value as { configuration: Record<string, unknown> };
  assert.equal(resume.configuration.sandbox, "workspace-write");
  assert.equal(resume.configuration.workingDirectory, resolve("C:/managed/lane-a"));
  assert.equal(turn.configuration.sandbox, "workspace-write");
  assert.ok(turn.configuration.outputSchema !== undefined);
  assert.equal(calls.filter((call) => call.method === "resume").length, 1);
});

test("CodexRunner rejects explicit environment names outside its allowlist", async () => {
  const factory: CodexAppServerFactory = () => { throw new Error("not reached"); };
  const runner = new CodexRunner({
    executionAuthorised: true, authorityReference: "AUTH-CODEX-TEST-003", worktreeRoot: "C:/managed",
    environment: { UNSAFE_VARIABLE: "fixture" }, model: null,
  }, factory);
  await assert.rejects(runner.run({
    runId: "run-fixture", attemptId: "attempt-fixture", task: task({ worktree: "C:/managed/lane" }), baseline, contracts: [], candidate: null, resumeThreadId: null,
    checkpointThread: async () => undefined,
  }), (error: unknown) => error instanceof OrchestratorStop && error.code === "SECRET_REQUIRED");
});

test("CodexRunner rejects the product credential identifier before starting a turn", async () => {
  let turnStarted = false;
  const runner = new CodexRunner({
    executionAuthorised: true,
    authorityReference: "AUTH-CODEX-TEST-004",
    worktreeRoot: "C:/managed",
    environment: { PATH: "C:/bin" },
    model: null,
  }, () => ({
    async assertChatGptSession() {},
    async startThread() { return "thread-secret-guard"; },
    async resumeThread() { throw new Error("not reached"); },
    async runTurn() { turnStarted = true; return JSON.stringify(passingResult()); },
    async close() {},
  }));
  await assert.rejects(runner.run({
    runId: "run-fixture",
    attemptId: "attempt-fixture",
    task: task({ owner: "implementation_worker", worktree: "C:/managed/lane", objective: "OPENAI_API_KEY" }),
    baseline,
    contracts: [],
    candidate: null,
    resumeThreadId: null,
    checkpointThread: async () => undefined,
  }), (error: unknown) => error instanceof OrchestratorStop && error.code === "SECRET_REQUIRED");
  assert.equal(turnStarted, false);
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
    structured("cherry-pick"), structured("owned-head", `${commit}\n`), structured("post-status"), structured("post-head", `${commit}\n`), structured("post-tree", `${tree}\n`),
    structured("post-count", "1\n"), structured("post-diff", "tools/file.ts\u0000"), structured("post-patch", "patch"),
  ]);
  const pipeline = new SequentialIntegrationPipeline(process, "C:/git.exe", {}, { run: async () => command("repository-ci-offline") });
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

test("failed integration is preserved and reported without a destructive rollback", async () => {
  const commit = "1".repeat(40);
  const tree = "2".repeat(40);
  const process = new ScriptedProcess([
    structured("head", `${baseline}\n`), structured("status"), structured("branch", "codex/integration\n"),
    structured("ancestry"), structured("count", "1\n"), structured("tree", `${tree}\n`), structured("diff", "tools/file.ts\u0000"), structured("candidate-patch", "patch"),
    structured("cherry-pick", "", "FAIL"), structured("quit"),
  ]);
  const pipeline = new SequentialIntegrationPipeline(process, "C:/git.exe", {}, { run: async () => command("repository-ci-offline") });
  const candidate = { commitId: commit, treeId: tree, changedFiles: ["tools/file.ts"] };
  const implementation = task({ taskId: "implementation", owner: "implementation_worker", status: "IMPLEMENTED", candidate, result: passingResult(["tools/file.ts"]) });
  const integrationTask = task({ taskId: "integration", taskKind: "INTEGRATION", owner: "governance_guard", candidateTaskId: "implementation", executionSurface: { ...task().executionSurface, cwd: "C:/coordinator", tools: [] } });
  await assert.rejects(pipeline.integrate({ baseline, expectedCoordinatorHead: baseline, integrationTask, implementationTask: implementation, candidate, workerResult: implementation.result!, independentReview: null, securityReview: null }),
    (error: unknown) => error instanceof OrchestratorStop && error.code === "UNEXPECTED_DIRTY_TREE");
  assert.ok(process.requests.some((request) => request.arguments.includes("--quit")));
  assert.equal(process.requests.some((request) => request.arguments.includes("reset") || request.arguments.includes("--hard")), false);
});

test("post-integration validation failure restores the exact expected coordinator HEAD", async () => {
  const commit = "1".repeat(40);
  const tree = "2".repeat(40);
  const process = new ScriptedProcess([
    structured("head", `${baseline}\n`), structured("status"), structured("branch", "codex/integration\n"),
    structured("ancestry"), structured("count", "1\n"), structured("tree", `${tree}\n`), structured("diff", "tools/file.ts\u0000"), structured("candidate-patch", "reviewed-patch"),
    structured("cherry-pick"), structured("owned-head", `${commit}\n`), structured("post-status"), structured("post-head", `${commit}\n`), structured("post-tree", `${tree}\n`),
    structured("post-count", "1\n"), structured("post-diff", "tools/file.ts\u0000"), structured("post-patch", "different-patch"),
    structured("rollback-owned-head", `${commit}\n`), structured("rollback-owned-status"), structured("rollback-owned-branch", "codex/integration\n"),
    structured("rollback-reference"), structured("rollback-worktree"), structured("rollback-head", `${baseline}\n`), structured("rollback-status"),
  ]);
  const pipeline = new SequentialIntegrationPipeline(process, "C:/git.exe", {}, { run: async () => command("repository-ci-offline") });
  const candidate = { commitId: commit, treeId: tree, changedFiles: ["tools/file.ts"] };
  const implementation = task({ taskId: "implementation", owner: "implementation_worker", status: "IMPLEMENTED", candidate, result: passingResult(["tools/file.ts"]) });
  const integrationTask = task({ taskId: "integration", taskKind: "INTEGRATION", owner: "governance_guard", candidateTaskId: "implementation", executionSurface: { ...task().executionSurface, cwd: "C:/coordinator", tools: [] } });
  await assert.rejects(
    pipeline.integrate({ baseline, expectedCoordinatorHead: baseline, integrationTask, implementationTask: implementation, candidate, workerResult: implementation.result!, independentReview: null, securityReview: null }),
    (error: unknown) => error instanceof OrchestratorStop && error.code === "UNEXPECTED_DIRTY_TREE",
  );
  assert.ok(process.requests.some((request) => request.arguments.includes("update-ref") && request.arguments.includes(commit) && request.arguments.includes(baseline)));
  assert.ok(process.requests.some((request) => request.arguments.includes("read-tree") && request.arguments.includes(baseline)));
  assert.equal(process.requests.some((request) => request.arguments.includes("--hard")), false);
});

test("post-integration drift is preserved without moving the coordinator reference", async () => {
  const commit = "1".repeat(40);
  const tree = "2".repeat(40);
  const process = new ScriptedProcess([
    structured("head", `${baseline}\n`), structured("status"), structured("branch", "codex/integration\n"),
    structured("ancestry"), structured("count", "1\n"), structured("tree", `${tree}\n`), structured("diff", "tools/file.ts\u0000"), structured("candidate-patch", "reviewed-patch"),
    structured("cherry-pick"), structured("owned-head", `${commit}\n`), structured("post-status"), structured("post-head", `${commit}\n`), structured("post-tree", `${tree}\n`),
    structured("post-count", "1\n"), structured("post-diff", "tools/file.ts\u0000"), structured("post-patch", "different-patch"),
    structured("rollback-owned-head", `${commit}\n`), structured("rollback-owned-status", " M tools/file.ts\u0000"), structured("rollback-owned-branch", "codex/integration\n"),
  ]);
  const pipeline = new SequentialIntegrationPipeline(process, "C:/git.exe", {}, { run: async () => command("repository-ci-offline") });
  const candidate = { commitId: commit, treeId: tree, changedFiles: ["tools/file.ts"] };
  const implementation = task({ taskId: "implementation", owner: "implementation_worker", status: "IMPLEMENTED", candidate, result: passingResult(["tools/file.ts"]) });
  const integrationTask = task({ taskId: "integration", taskKind: "INTEGRATION", owner: "governance_guard", candidateTaskId: "implementation", executionSurface: { ...task().executionSurface, cwd: "C:/coordinator", tools: [] } });
  await assert.rejects(
    pipeline.integrate({ baseline, expectedCoordinatorHead: baseline, integrationTask, implementationTask: implementation, candidate, workerResult: implementation.result!, independentReview: null, securityReview: null }),
    (error: unknown) => error instanceof OrchestratorStop && error.code === "UNEXPECTED_DIRTY_TREE",
  );
  assert.equal(process.requests.some((request) => request.arguments.includes("update-ref")), false);
  assert.equal(process.requests.some((request) => request.arguments.includes("read-tree")), false);
});
