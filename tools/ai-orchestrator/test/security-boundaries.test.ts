// Purpose: Exercises duplicate JSON, bounded persistence, process denial and a disposable real Git worktree lifecycle.
import assert from "node:assert/strict";
import { randomUUID } from "node:crypto";
import { access, mkdir, readFile, rm, symlink, unlink, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { delimiter, join, resolve } from "node:path";
import test from "node:test";
import { BoundedProcess } from "../src/adapters/bounded-process.js";
import { FileResourceLocks } from "../src/adapters/file-resource-locks.js";
import { FileStateStore } from "../src/adapters/file-state-store.js";
import { FileThreadCheckpointStore } from "../src/adapters/file-thread-checkpoints.js";
import { GitCandidateInspector, TrustedCandidateLanguageChecker } from "../src/adapters/git-candidate-inspector.js";
import { GitWorktreeManager } from "../src/adapters/git-worktrees.js";
import { SequentialIntegrationPipeline } from "../src/application/integration.js";
import type { PersistedRunState } from "../src/core/contracts.js";
import { OrchestratorStop } from "../src/core/errors.js";
import type { ProcessRequest, StructuredProcessExecutor, StructuredProcessResult } from "../src/ports/process-executor.js";
import { gitArguments, gitEnvironment } from "../src/security/git-process-policy.js";
import { parseSecureJson } from "../src/security/secure-json.js";
import {
  assertArgumentsContainNoSecretMaterial,
  assertAuthorityReference,
  assertCliArgumentsContainNoSecretMaterial,
  assertClosedEnvironment,
  assertNoSecretShapedMaterial,
  assertNoSecretShapedText,
  removeProductCredentialFromEnvironment,
} from "../src/security/secret-policy.js";
import { assertNoExistingReparseBoundary } from "../src/security/path-policy.js";
import { assertBritishCommitMessage, type TrustedLanguagePolicy } from "../src/security/language-policy.js";
import { baseline, instant, passingResult, task } from "./helpers.js";

const trustedLanguagePolicy: TrustedLanguagePolicy = {
  policyId: "rag-challenge-language-policy-v1",
  technicalLanguage: "en-GB",
  bannedAmericanSpellings: [
    { american: "behavior", british: "behaviour" },
    { american: "normalize", british: "normalise" },
  ],
  portugueseTechnicalMarkers: ["implementação", "validação"],
};
const passingLanguageChecker = { check: async (): Promise<void> => undefined };

async function guidTempDirectory(prefix: string): Promise<string> {
  const root = join(tmpdir(), `${prefix}${randomUUID()}`);
  await mkdir(root, { recursive: false });
  return root;
}

function syntheticGitArguments(arguments_: readonly string[]): readonly string[] {
  const nullDevice = process.platform === "win32" ? "NUL" : "/dev/null";
  return gitArguments(["-c", `init.templateDir=${nullDevice}`, ...arguments_]);
}

function state(): PersistedRunState {
  return {
    schemaVersion: 1, runId: "run-security", revision: 0, baseline, maxConcurrency: 1,
    createdAt: instant, updatedAt: instant, tasks: [task()], attempts: [], heldLocks: [], humanGateReached: false,
  };
}

class RecordingGitProcess implements StructuredProcessExecutor {
  public readonly requests: ProcessRequest[] = [];
  public constructor(private readonly outputs: Readonly<Record<string, string>> = {}) {}
  public async run(request: ProcessRequest) { return (await this.runStructured(request)).evidence; }
  public async runStructured(request: ProcessRequest): Promise<StructuredProcessResult> {
    this.requests.push(request);
    return {
      stdout: this.outputs[request.commandId] ?? "", stderr: "",
      evidence: { commandId: request.commandId, exitCode: 0, durationMs: 1, result: "PASS", relevantOutput: [] },
    };
  }
}

class InterceptingGitProcess implements StructuredProcessExecutor {
  public constructor(
    private readonly delegate: StructuredProcessExecutor,
    private readonly intercept: (request: ProcessRequest, next: () => Promise<StructuredProcessResult>) => Promise<StructuredProcessResult>,
  ) {}
  public async run(request: ProcessRequest) { return (await this.runStructured(request)).evidence; }
  public async runStructured(request: ProcessRequest): Promise<StructuredProcessResult> {
    return await this.intercept(request, async () => await this.delegate.runStructured(request));
  }
}

test("worktree porcelain accepts a prunable reason", async () => {
  const output = `worktree C:/managed/stale\u0000HEAD ${"a".repeat(40)}\u0000prunable gitdir file points to non-existent location\u0000`;
  const executable = process.platform === "win32" ? "C:/Git/bin/git.exe" : "/usr/bin/git";
  const manager = new GitWorktreeManager("C:/repository", "C:/managed", new RecordingGitProcess({ "worktree-list": output }), executable, {});
  const records = await manager.list();
  assert.equal(records.length, 1);
  assert.equal(records[0]?.prunable, true);
});

test("secure JSON rejects duplicate keys before schema validation", () => {
  assert.throws(() => parseSecureJson('{"status":"PASS","status":"FAIL"}', "fixture"), /duplicate key 'status'/);
  assert.throws(() => parseSecureJson(`${"[".repeat(65)}null${"]".repeat(65)}`, "fixture"), /maximum structural depth/);
  for (const key of ["__proto__", "constructor", "prototype"]) {
    assert.throws(() => parseSecureJson(`{"${key}":{}}`, "fixture"), /forbidden prototype key/);
  }
});

// SYNTHETIC_ORCHESTRATOR_ENFORCEMENT_START
test("central secret policy rejects synthetic secret-shaped material without echoing it", () => {
  const synthetic = "sk-proj-synthetic-not-a-real-secret";
  for (const action of [
    () => assertNoSecretShapedMaterial({ plan: { note: synthetic } }, "plan"),
    () => assertArgumentsContainNoSecretMaterial(["--value", synthetic], "arguments"),
    () => assertClosedEnvironment({ PATH: synthetic }, new Set(["PATH"]), "environment"),
  ]) {
    assert.throws(action, (error: unknown) =>
      error instanceof OrchestratorStop && error.code === "SECRET_REQUIRED" && !error.message.includes(synthetic));
  }
});

test("real runner authority accepts only bounded AUTH references", () => {
  assert.doesNotThrow(() => assertAuthorityReference("AUTH-CODEX-RUN-001", "authority"));
  assert.throws(() => assertAuthorityReference("separate-test-authority", "authority"), (error: unknown) =>
    error instanceof OrchestratorStop && error.code === "HUMAN_DECISION_REQUIRED");
});

test("product credential identifier cannot traverse text, object fields or CLI arguments", () => {
  const identifier = "OPENAI_API_KEY";
  for (const action of [
    () => assertNoSecretShapedMaterial({ note: identifier }, "material"),
    () => assertNoSecretShapedMaterial({ [identifier]: null }, "material"),
    () => assertCliArgumentsContainNoSecretMaterial([identifier], "arguments"),
    () => assertCliArgumentsContainNoSecretMaterial(["--openai-api-key", "synthetic"], "arguments"),
    () => assertCliArgumentsContainNoSecretMaterial(["--credential", "synthetic"], "arguments"),
  ]) {
    assert.throws(action, (error: unknown) =>
      error instanceof OrchestratorStop && error.code === "SECRET_REQUIRED" && !error.message.includes(identifier));
  }
});

test("product credential identifier rejection is case-insensitive", () => {
  const lowerCaseIdentifier = "openai_api_key";
  const mixedCaseIdentifier = "OpenAI_Api_Key";
  assert.throws(
    () => assertNoSecretShapedMaterial({ note: lowerCaseIdentifier }, "material"),
    (error: unknown) => error instanceof OrchestratorStop &&
      error.code === "SECRET_REQUIRED" &&
      !error.message.includes(lowerCaseIdentifier),
  );
  assert.throws(
    () => assertNoSecretShapedMaterial({ [mixedCaseIdentifier]: null }, "material"),
    (error: unknown) => error instanceof OrchestratorStop &&
      error.code === "SECRET_REQUIRED" &&
      !error.message.includes(mixedCaseIdentifier),
  );
});

test("product credential removal deletes only the assembled identifier without reading it", () => {
  const identifier = ["OPENAI", "API", "KEY"].join("_");
  const environment: Record<string, string | undefined> = {
    [identifier]: "synthetic-never-read",
    PATH: "C:/synthetic",
  };
  removeProductCredentialFromEnvironment(environment);
  assert.deepEqual(environment, { PATH: "C:/synthetic" });
});

test("trusted candidate language policy accepts British prose and external literals but rejects subject or body debt", () => {
  assert.doesNotThrow(() => assertBritishCommitMessage("fix(orchestrator): normalise candidate behaviour", trustedLanguagePolicy));
  assert.doesNotThrow(() => assertBritishCommitMessage("fix(api): preserve https://example.invalid/behavior", trustedLanguagePolicy));
  const identifier = "OPENAI_API_KEY";
  assert.throws(() => assertNoSecretShapedText(`fix(api): preserve ${identifier}`, "Candidate commit message"), (error: unknown) =>
    error instanceof OrchestratorStop && error.code === "SECRET_REQUIRED" && !error.message.includes(identifier));
  assert.throws(() => assertBritishCommitMessage("fix(orchestrator): normalize candidate", trustedLanguagePolicy), (error: unknown) =>
    error instanceof OrchestratorStop && error.code === "OUT_OF_SCOPE_CHANGE_REQUIRED");
  assert.throws(() => assertBritishCommitMessage("fix(orchestrator): preserve candidate\n\nReject behavior drift.", trustedLanguagePolicy), (error: unknown) =>
    error instanceof OrchestratorStop && error.code === "OUT_OF_SCOPE_CHANGE_REQUIRED");
});

test("candidate cannot relax the coordinator-owned language policy", async () => {
  const commit = "a".repeat(40);
  const tree = "b".repeat(40);
  const processAdapter = new RecordingGitProcess({
    "candidate-head": `${commit}\n`,
    "candidate-branch": "codex/implementation\n",
    "candidate-status": "",
    "candidate-count": "1\n",
    "candidate-tree": `${tree}\n`,
    "candidate-diff": "eng/language-policy.json\u0000",
    "candidate-message": "fix(language): normalize behavior\n",
  });
  const definition = task({
    taskId: "implementation",
    owner: "implementation_worker",
    allowedPaths: ["eng/language-policy.json"],
    worktree: "C:/managed/implementation",
    branch: "codex/implementation",
  });
  await assert.rejects(
    new GitCandidateInspector(processAdapter, process.execPath, {}, trustedLanguagePolicy, passingLanguageChecker).inspect(
      definition,
      baseline,
      passingResult(["eng/language-policy.json"]),
    ),
    (error: unknown) => error instanceof OrchestratorStop && error.code === "OUT_OF_SCOPE_CHANGE_REQUIRED",
  );
});

test("candidate commit secrets and invalid UTF-8 fail before language evidence without echoing", async () => {
  const commit = "a".repeat(40);
  const tree = "b".repeat(40);
  const definition = task({
    taskId: "implementation",
    owner: "implementation_worker",
    allowedPaths: ["fixture.txt"],
    worktree: "C:/managed/implementation",
    branch: "codex/implementation",
  });
  for (const [message, code] of [
    ["fix(candidate): preserve OPENAI_API_KEY\n", "SECRET_REQUIRED"],
    ["fix(candidate): preserve sk-proj-synthetic-not-a-real-secret\n", "SECRET_REQUIRED"],
    ["fix(candidate): invalid \uFFFD message\n", "OUT_OF_SCOPE_CHANGE_REQUIRED"],
  ] as const) {
    let checkerCalls = 0;
    const processAdapter = new RecordingGitProcess({
      "candidate-head": `${commit}\n`, "candidate-branch": "codex/implementation\n", "candidate-status": "",
      "candidate-count": "1\n", "candidate-tree": `${tree}\n`, "candidate-diff": "fixture.txt\u0000", "candidate-message": message,
    });
    await assert.rejects(
      new GitCandidateInspector(processAdapter, process.execPath, {}, trustedLanguagePolicy, {
        check: async () => { checkerCalls += 1; },
      }).inspect(definition, baseline, passingResult(["fixture.txt"])),
      (error: unknown) => error instanceof OrchestratorStop && error.code === code && !error.message.includes(message.trim()),
    );
    assert.equal(checkerCalls, 0);
  }
});

test("ordinary candidates cannot alter trusted language controls or bypass the trusted content check", async () => {
  const commit = "a".repeat(40);
  const tree = "b".repeat(40);
  const outputs = {
    "candidate-head": `${commit}\n`, "candidate-branch": "codex/implementation\n", "candidate-status": "",
    "candidate-count": "1\n", "candidate-tree": `${tree}\n`, "candidate-message": "fix(candidate): preserve trusted language controls\n",
  };
  const definition = task({
    taskId: "implementation",
    owner: "implementation_worker",
    allowedPaths: ["eng/language-policy.json", "eng/language-migration-baseline.json", "eng/language-policy.schema.json"],
    worktree: "C:/managed/implementation",
    branch: "codex/implementation",
  });
  await assert.rejects(
    new GitCandidateInspector(
      new RecordingGitProcess({ ...outputs, "candidate-diff": "eng/language-policy.json\u0000eng/language-migration-baseline.json\u0000eng/language-policy.schema.json\u0000" }),
      process.execPath, {}, trustedLanguagePolicy, passingLanguageChecker,
    ).inspect(definition, baseline, passingResult(definition.allowedPaths)),
    (error: unknown) => error instanceof OrchestratorStop && error.code === "OUT_OF_SCOPE_CHANGE_REQUIRED",
  );

  let checkerCalls = 0;
  const ordinary = task({ ...definition, allowedPaths: ["fixture.txt"] });
  await assert.rejects(
    new GitCandidateInspector(
      new RecordingGitProcess({ ...outputs, "candidate-diff": "fixture.txt\u0000" }),
      process.execPath,
      {},
      trustedLanguagePolicy,
      { check: async () => { checkerCalls += 1; throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "Synthetic trusted policy rejection."); } },
    ).inspect(ordinary, baseline, passingResult(["fixture.txt"])),
    (error: unknown) => error instanceof OrchestratorStop && error.code === "OUT_OF_SCOPE_CHANGE_REQUIRED",
  );
  assert.equal(checkerCalls, 1);
});
// SYNTHETIC_ORCHESTRATOR_ENFORCEMENT_END

test("trusted candidate checker executes the coordinator script against the candidate root with a closed environment", async () => {
  const processAdapter = new RecordingGitProcess();
  const coordinatorRoot = resolve("../..");
  const checkerPath = join(coordinatorRoot, "eng", "check-language.mjs");
  await new TrustedCandidateLanguageChecker(
    processAdapter,
    process.execPath,
    checkerPath,
    coordinatorRoot,
    {},
  ).check("C:/synthetic-candidate", "a".repeat(40), "implementation");
  const request = processAdapter.requests.at(-1);
  assert.equal(request?.commandId, "candidate-language");
  assert.deepEqual(request?.environment, {});
  assert.deepEqual(request?.arguments, [
    checkerPath,
    "--repository-root", "C:/synthetic-candidate",
    "--trusted-policy-root", coordinatorRoot,
    "--commit-head", "a".repeat(40),
  ]);
});

test("state persistence rejects a junction in an ancestor below the repository anchor", async (context) => {
  const root = await guidTempDirectory("orchestrator-state-anchor-");
  const external = await guidTempDirectory("orchestrator-state-external-");
  try {
    const link = join(root, "artifacts-local");
    try { await symlink(external, link, process.platform === "win32" ? "junction" : "dir"); }
    catch (error) {
      if ((error as NodeJS.ErrnoException).code === "EPERM") { context.skip("Junction creation is not permitted in this environment."); return; }
      throw error;
    }
    const sentinel = join(external, "sentinel.txt");
    await writeFile(sentinel, "preserve", "utf8");
    await assert.rejects(new FileStateStore(join(link, "ai-orchestrator"), root).save(state()),
      (error: unknown) => error instanceof OrchestratorStop && error.code === "OUT_OF_SCOPE_CHANGE_REQUIRED");
    assert.equal(await readFile(sentinel, "utf8"), "preserve");
  } finally {
    await rm(root, { recursive: true, force: true });
    await rm(external, { recursive: true, force: true });
  }
});

test("existing symbolic-link or junction boundaries are rejected", async () => {
  const root = await guidTempDirectory("orchestrator-reparse-");
  try {
    const target = join(root, "target");
    const link = join(root, "link");
    await mkdir(target);
    await symlink(target, link, process.platform === "win32" ? "junction" : "dir");
    await assert.rejects(assertNoExistingReparseBoundary(root, join(link, "child")), /symbolic-link boundary/);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test("report-only lock inspection rejects a junction boundary", async (context) => {
  const root = await guidTempDirectory("orchestrator-lock-anchor-");
  const external = await guidTempDirectory("orchestrator-lock-external-");
  try {
    const link = join(root, "locks");
    try { await symlink(external, link, process.platform === "win32" ? "junction" : "dir"); }
    catch (error) {
      if ((error as NodeJS.ErrnoException).code === "EPERM") { context.skip("Junction creation is not permitted in this environment."); return; }
      throw error;
    }
    await writeFile(join(external, "sentinel.txt"), "preserve", "utf8");
    await assert.rejects(new FileResourceLocks(link, root).inspectRecords(), (error: unknown) =>
      error instanceof OrchestratorStop && error.code === "OUT_OF_SCOPE_CHANGE_REQUIRED");
    assert.equal(await readFile(join(external, "sentinel.txt"), "utf8"), "preserve");
  } finally {
    await rm(root, { recursive: true, force: true });
    await rm(external, { recursive: true, force: true });
  }
});

test("worktree ownership directory junction cannot escape the managed root", async (context) => {
  const root = await guidTempDirectory("orchestrator-owner-root-");
  const external = await guidTempDirectory("orchestrator-owner-external-");
  try {
    const managed = join(root, "managed");
    await mkdir(managed);
    const owners = join(managed, ".owners");
    try { await symlink(external, owners, process.platform === "win32" ? "junction" : "dir"); }
    catch (error) {
      if ((error as NodeJS.ErrnoException).code === "EPERM") { context.skip("Junction creation is not permitted in this environment."); return; }
      throw error;
    }
    const sentinel = join(external, "sentinel.txt");
    await writeFile(sentinel, "preserve", "utf8");
    const processAdapter = new RecordingGitProcess();
    const manager = new GitWorktreeManager(root, managed, processAdapter, process.execPath, {});
    await assert.rejects(manager.create("implementation", join(managed, "implementation"), "codex/implementation", baseline), (error: unknown) =>
      error instanceof OrchestratorStop && error.code === "OUT_OF_SCOPE_CHANGE_REQUIRED");
    assert.equal(await readFile(sentinel, "utf8"), "preserve");
    assert.equal(processAdapter.requests.some((request) => request.commandId === "worktree-create"), false);
  } finally {
    await rm(root, { recursive: true, force: true });
    await rm(external, { recursive: true, force: true });
  }
});

test("worktree ownership marker leaf link is rejected before Git mutation", async (context) => {
  const root = await guidTempDirectory("orchestrator-owner-marker-");
  const external = await guidTempDirectory("orchestrator-owner-marker-external-");
  try {
    const managed = join(root, "managed");
    const owners = join(managed, ".owners");
    const worktree = join(managed, "implementation");
    await mkdir(owners, { recursive: true });
    const target = join(external, "implementation.json");
    const marker = JSON.stringify({ taskId: "implementation", path: resolve(worktree), branch: "codex/implementation", baseline });
    await writeFile(target, marker, "utf8");
    try { await symlink(target, join(owners, "implementation.json"), "file"); }
    catch (error) {
      if ((error as NodeJS.ErrnoException).code === "EPERM") { context.skip("File symlink creation is not permitted in this environment."); return; }
      throw error;
    }
    const processAdapter = new RecordingGitProcess();
    const manager = new GitWorktreeManager(root, managed, processAdapter, process.execPath, {});
    await assert.rejects(manager.create("implementation", worktree, "codex/implementation", baseline), (error: unknown) =>
      error instanceof OrchestratorStop && error.code === "OUT_OF_SCOPE_CHANGE_REQUIRED");
    assert.equal(await readFile(target, "utf8"), marker);
    assert.equal(processAdapter.requests.some((request) => request.commandId === "worktree-create"), false);
  } finally {
    await rm(root, { recursive: true, force: true });
    await rm(external, { recursive: true, force: true });
  }
});

test("state and lock reads fail closed on oversized or ambiguous local artefacts", async () => {
  const root = await guidTempDirectory("orchestrator-bounds-");
  try {
    const store = new FileStateStore(root);
    await store.save(state());
    await writeFile(join(root, "run-security", "state-00000000.json"), "x".repeat(8_388_609), "utf8");
    await assert.rejects(store.load("run-security"), /bounded regular file/);

    const lockRoot = join(root, "locks");
    await mkdir(lockRoot, { recursive: true });
    const lockName = `${"a".repeat(64)}.lock`;
    await writeFile(join(lockRoot, lockName), '{"schemaVersion":1,"schemaVersion":1}', "utf8");
    const report = await new FileResourceLocks(lockRoot).inspectRecords();
    assert.deepEqual(report, [{ lockId: lockName, resource: null, status: "INVALID", runId: null, taskId: null, attemptId: null, acquiredAt: null, recordDigest: null }]);
    assert.equal(await readFile(join(lockRoot, lockName), "utf8"), '{"schemaVersion":1,"schemaVersion":1}');
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test("persisted snapshot, journal, checkpoint and lock leaf links fail closed", async (context) => {
  const root = await guidTempDirectory("orchestrator-leaf-links-");
  const external = await guidTempDirectory("orchestrator-leaf-external-");
  const replaceWithLink = async (path: string, name: string): Promise<boolean> => {
    const target = join(external, name);
    await writeFile(target, await readFile(path));
    await unlink(path);
    try { await symlink(target, path, "file"); return true; }
    catch (error) {
      if ((error as NodeJS.ErrnoException).code === "EPERM") return false;
      throw error;
    }
  };
  try {
    const stateRoot = join(root, "state");
    const store = new FileStateStore(stateRoot, root);
    await store.save(state());
    const runRoot = join(stateRoot, "run-security");
    const snapshot = join(runRoot, "state-00000000.json");
    const snapshotText = await readFile(snapshot);
    if (!await replaceWithLink(snapshot, "snapshot.json")) { context.skip("File symlink creation is not permitted in this environment."); return; }
    await assert.rejects(store.load("run-security"), /symbolic-link boundary|bounded regular file/);
    await unlink(snapshot);
    await writeFile(snapshot, snapshotText);
    const journal = join(runRoot, "journal.jsonl");
    if (!await replaceWithLink(journal, "journal.jsonl")) { context.skip("File symlink creation is not permitted in this environment."); return; }
    await assert.rejects(store.load("run-security"), /symbolic-link boundary|bounded regular file/);

    const checkpoints = new FileThreadCheckpointStore(stateRoot, root);
    const checkpoint = {
      schemaVersion: 1 as const, runId: "run-checkpoint", taskId: "map-repository", attemptId: "attempt-checkpoint",
      agentId: "code_mapper" as const, taskKind: "DISCOVERY" as const, baseline, candidateCommitId: null,
      envelopeHash: "a".repeat(64), stateRevision: 1, deadlineMs: 300_000, threadId: "thread-checkpoint", startedAt: instant,
    };
    await checkpoints.save(checkpoint);
    const checkpointPath = join(stateRoot, checkpoint.runId, "threads", `${checkpoint.taskId}.json`);
    if (!await replaceWithLink(checkpointPath, "checkpoint.json")) { context.skip("File symlink creation is not permitted in this environment."); return; }
    await assert.rejects(checkpoints.load(checkpoint.runId, checkpoint.taskId), /symbolic-link boundary|unreadable/);

    const lockRoot = join(root, "locks-leaf");
    const locks = new FileResourceLocks(lockRoot, root);
    await locks.acquire("sqlite:leaf", { runId: "run-lock", taskId: "map-repository", attemptId: "attempt-lock", acquiredAt: instant });
    const lockName = (await locks.inspect())[0]!;
    const lockPath = join(lockRoot, lockName);
    if (!await replaceWithLink(lockPath, "lock.json")) { context.skip("File symlink creation is not permitted in this environment."); return; }
    const report = await locks.inspectRecords();
    assert.equal(report[0]?.status, "INVALID");
    assert.equal(await readFile(join(external, "lock.json"), "utf8").then((value) => value.length > 0), true);
  } finally {
    await rm(root, { recursive: true, force: true });
    await rm(external, { recursive: true, force: true });
  }
});

test("bounded processes reject PATH resolution, unsafe environment names, timeout and output overflow", async () => {
  const processAdapter = new BoundedProcess();
  const base: ProcessRequest = {
    commandId: "fixture", executable: process.execPath, arguments: ["-e", ""], cwd: resolve("."), environment: {},
    timeoutMs: 10_000, maximumOutputBytes: 4096,
  };
  await assert.rejects(processAdapter.run({ ...base, executable: "node" }), (error: unknown) => error instanceof OrchestratorStop && error.code === "OUT_OF_SCOPE_CHANGE_REQUIRED");
  await assert.rejects(processAdapter.run({ ...base, environment: { TEST_SECRET: "fixture" } }), (error: unknown) => error instanceof OrchestratorStop && error.code === "SECRET_REQUIRED");
  const operationalEnvironment = Object.fromEntries(["SystemRoot", "WINDIR"].flatMap((name) => process.env[name] === undefined ? [] : [[name, process.env[name] as string]]));
  const timeout = await processAdapter.run({ ...base, arguments: ["-e", "setTimeout(() => {}, 5000)"], environment: operationalEnvironment, timeoutMs: 50 });
  assert.equal(timeout.result, "BLOCKED");
  assert.ok(timeout.relevantOutput.includes("PROCESS_TIMEOUT"));
  const overflow = await processAdapter.run({ ...base, arguments: ["-e", "process.stdout.write('x'.repeat(10000))"], maximumOutputBytes: 64 });
  assert.equal(overflow.result, "BLOCKED");
  assert.ok(overflow.relevantOutput.includes("OUTPUT_LIMIT_EXCEEDED"));
  const paths = await processAdapter.run({
    ...base,
    arguments: ["-e", "process.stdout.write('C:/Users/Alice/repo/file.cs\\n\\\\\\\\host\\\\share\\\\repo\\\\file.cs\\n/home/alice/repo/file.cs\\npath=/home/alice/repo/file.cs\\n\\\'/private/file.cs\\\'\\nerror(/srv/private/file.cs)\\n/workspace/private/file.cs\\n/opt/private/file.cs\\n/srv/private/file.cs\\n/mnt/private/file.cs\\nfile:///C:/Users/Alice/repo/file.cs')"],
  });
  assert.equal(paths.result, "PASS");
  assert.equal(paths.relevantOutput.some((line) => /Alice|alice|host|share|workspace|opt|srv|mnt|private/.test(line)), false);
});

test("timeout terminates the task-owned descendant process tree", async () => {
  const environment = Object.fromEntries(["SystemRoot", "WINDIR"].flatMap((name) => process.env[name] === undefined ? [] : [[name, process.env[name] as string]]));
  const script = "const {spawn}=require('node:child_process');const child=spawn(process.execPath,['-e','setInterval(()=>{},1000)']);process.stdout.write(String(child.pid)+'\\n');setInterval(()=>{},1000);";
  const evidence = await new BoundedProcess().run({
    commandId: "tree-timeout", executable: process.execPath, arguments: ["-e", script], cwd: resolve("."), environment,
    timeoutMs: 150, maximumOutputBytes: 4096,
  });
  assert.equal(evidence.result, "BLOCKED");
  const childPid = Number(evidence.relevantOutput.find((line) => /^\d+$/.test(line)));
  assert.equal(Number.isInteger(childPid), true);
  await new Promise((resolveDelay) => setTimeout(resolveDelay, 100));
  let alive = true;
  try { process.kill(childPid, 0); } catch (error) { alive = (error as NodeJS.ErrnoException).code !== "ESRCH"; }
  assert.equal(alive, false);
});

async function absoluteGit(): Promise<string> {
  const names = process.platform === "win32" ? ["git.exe"] : ["git"];
  const candidates = [
    ...(process.platform === "win32" ? [
      join(process.env.ProgramFiles ?? "C:/Program Files", "Git", "cmd", "git.exe"),
      join(process.env.LOCALAPPDATA ?? "C:/missing", "Programs", "Git", "cmd", "git.exe"),
    ] : ["/usr/bin/git", "/usr/local/bin/git"]),
    ...(process.env.PATH ?? "").split(delimiter).flatMap((directory) => names.map((name) => join(directory, name))),
  ];
  for (const candidate of candidates) {
    try { await access(candidate); return resolve(candidate); } catch { /* Continue through deterministic candidates. */ }
  }
  throw new Error("An absolute Git executable is required for the disposable worktree test.");
}

test("real disposable Git worktree captures deletions, rejects multi-commit candidates and cleans up with branch CAS", async () => {
  const root = await guidTempDirectory("orchestrator-git-");
  const repository = join(root, "repository");
  const managed = join(root, "managed");
  const worktree = join(managed, "implementation");
  const git = await absoluteGit();
  const processAdapter = new BoundedProcess();
  const environment = gitEnvironment(process.env);
  const runGit = async (cwd: string, arguments_: readonly string[]) => await processAdapter.runStructured({
    commandId: "git-fixture", executable: git, arguments: syntheticGitArguments(arguments_), cwd, environment,
    timeoutMs: 120_000, maximumOutputBytes: 1_048_576,
  });
  try {
    await mkdir(repository, { recursive: true });
    assert.equal((await runGit(repository, ["init", "--initial-branch=codex/coordinator"])).evidence.result, "PASS");
    assert.equal((await runGit(repository, ["config", "user.name", "Orchestrator Fixture"])).evidence.result, "PASS");
    assert.equal((await runGit(repository, ["config", "user.email", "fixture@example.invalid"])).evidence.result, "PASS");
    await writeFile(join(repository, "fixture.txt"), "baseline\n", "utf8");
    await writeFile(join(repository, "forbidden.txt"), "preserve\n", "utf8");
    assert.equal((await runGit(repository, ["add", "--", "fixture.txt", "forbidden.txt"])).evidence.result, "PASS");
    assert.equal((await runGit(repository, ["commit", "-m", "test(orchestrator): create disposable baseline"])).evidence.result, "PASS");
    const base = (await runGit(repository, ["rev-parse", "HEAD"])).stdout.trim();
    assert.match(base, /^[0-9a-f]{40}$/);

    const manager = new GitWorktreeManager(repository, managed, processAdapter, git, process.env);
    const created = await manager.create("implementation", worktree, "codex/implementation", base);
    assert.equal(created.head, base);
    await assert.rejects(
      manager.create("nested", join(worktree, "nested"), "codex/nested", base),
      (error: unknown) => error instanceof OrchestratorStop && error.code === "SHARED_RESOURCE_COLLISION",
    );
    await writeFile(join(worktree, "fixture.txt"), "candidate\n", "utf8");
    await rm(join(worktree, "forbidden.txt"));
    await assert.rejects(manager.validate("implementation", worktree, "codex/implementation", base), /not clean/);
    assert.equal((await runGit(worktree, ["add", "--", "fixture.txt", "forbidden.txt"])).evidence.result, "PASS");
    assert.equal((await runGit(worktree, ["commit", "-m", "test(orchestrator): create disposable candidate"])).evidence.result, "PASS");
    const implementation = task({ taskId: "implementation", owner: "implementation_worker", allowedPaths: ["fixture.txt"], worktree, branch: "codex/implementation" });
    const candidate = await new GitCandidateInspector(processAdapter, git, process.env, trustedLanguagePolicy, passingLanguageChecker).inspect(implementation, base, passingResult(["fixture.txt"]));
    assert.deepEqual(candidate.changedFiles, ["fixture.txt", "forbidden.txt"]);
    await writeFile(join(worktree, "second.txt"), "second\n", "utf8");
    assert.equal((await runGit(worktree, ["add", "--", "second.txt"])).evidence.result, "PASS");
    assert.equal((await runGit(worktree, ["commit", "-m", "test(orchestrator): create second disposable commit"])).evidence.result, "PASS");
    await assert.rejects(new GitCandidateInspector(processAdapter, git, process.env, trustedLanguagePolicy, passingLanguageChecker).inspect(implementation, base, passingResult(["fixture.txt", "forbidden.txt", "second.txt"])),
      /exactly one commit/);
    const advancedHead = (await runGit(worktree, ["rev-parse", "HEAD"])).stdout.trim();
    await assert.rejects(manager.removeManaged("implementation", worktree, { branch: "codex/implementation", baseline: base, head: candidate.commitId }), /persisted candidate identity|foreign or prunable/);
    await manager.removeManaged("implementation", worktree, { branch: "codex/implementation", baseline: base, head: advancedHead });
    assert.equal((await manager.list()).some((record) => resolve(record.path) === resolve(worktree)), false);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test("real sequential integration applies two reviewed one-commit candidates against the evolving HEAD", async () => {
  const root = await guidTempDirectory("orchestrator-integration-chain-");
  const repository = join(root, "repository");
  const managed = join(root, "managed");
  const git = await absoluteGit();
  const processAdapter = new BoundedProcess();
  const environment = gitEnvironment(process.env);
  const runGit = async (cwd: string, arguments_: readonly string[]) => await processAdapter.runStructured({
    commandId: "git-chain-fixture", executable: git, arguments: syntheticGitArguments(arguments_), cwd, environment,
    timeoutMs: 120_000, maximumOutputBytes: 1_048_576,
  });
  try {
    await mkdir(repository, { recursive: true });
    assert.equal((await runGit(repository, ["init", "--initial-branch=codex/coordinator"])).evidence.result, "PASS");
    assert.equal((await runGit(repository, ["config", "user.name", "Orchestrator Fixture"])).evidence.result, "PASS");
    assert.equal((await runGit(repository, ["config", "user.email", "fixture@example.invalid"])).evidence.result, "PASS");
    await writeFile(join(repository, "baseline.txt"), "baseline\n", "utf8");
    assert.equal((await runGit(repository, ["add", "--", "baseline.txt"])).evidence.result, "PASS");
    assert.equal((await runGit(repository, ["commit", "-m", "test(orchestrator): create integration baseline"])).evidence.result, "PASS");
    const base = (await runGit(repository, ["rev-parse", "HEAD"])).stdout.trim();
    const manager = new GitWorktreeManager(repository, managed, processAdapter, git, process.env);
    const inspector = new GitCandidateInspector(processAdapter, git, process.env, trustedLanguagePolicy, passingLanguageChecker);
    const candidates = [] as Array<{ taskId: string; path: string; branch: string; file: string }>;
    for (const suffix of ["a", "b"]) {
      const taskId = `implementation-${suffix}`;
      const path = join(managed, taskId);
      const branch = `codex/${taskId}`;
      const file = `candidate-${suffix}.txt`;
      await manager.create(taskId, path, branch, base);
      await writeFile(join(path, file), `${suffix}\n`, "utf8");
      assert.equal((await runGit(path, ["add", "--", file])).evidence.result, "PASS");
      assert.equal((await runGit(path, ["commit", "-m", `test(orchestrator): create candidate ${suffix}`])).evidence.result, "PASS");
      candidates.push({ taskId, path, branch, file });
    }
    const pipeline = new SequentialIntegrationPipeline(processAdapter, git, process.env, {
      run: async () => ({ commandId: "repository-ci-offline", exitCode: 0, durationMs: 1, result: "PASS", relevantOutput: [] }),
    });
    let expectedHead = base;
    for (const entry of candidates) {
      const definition = task({ taskId: entry.taskId, owner: "implementation_worker", allowedPaths: [entry.file], worktree: entry.path, branch: entry.branch });
      const candidate = await inspector.inspect(definition, base, passingResult([entry.file]));
      const implementation = { ...definition, status: "IMPLEMENTED" as const, candidate, result: passingResult([entry.file]) };
      const integrationTask = task({
        taskId: `integration-${entry.taskId}`, taskKind: "INTEGRATION", owner: "governance_guard", candidateTaskId: entry.taskId,
        executionSurface: { ...task().executionSurface, cwd: repository, tools: [] },
      });
      const outcome = await pipeline.integrate({
        baseline: base, expectedCoordinatorHead: expectedHead, integrationTask, implementationTask: implementation,
        candidate, workerResult: implementation.result, independentReview: null, securityReview: null,
      });
      expectedHead = outcome.candidate.commitId;
      assert.equal(outcome.tests[0]?.result, "PASS");
    }
    assert.equal((await runGit(repository, ["rev-parse", "HEAD"])).stdout.trim(), expectedHead);
    assert.equal((await readFile(join(repository, "candidate-a.txt"), "utf8")).trim(), "a");
    assert.equal((await readFile(join(repository, "candidate-b.txt"), "utf8")).trim(), "b");
    for (const entry of candidates) {
      const candidateHead = (await runGit(entry.path, ["rev-parse", "HEAD"])).stdout.trim();
      await manager.removeManaged(entry.taskId, entry.path, { branch: entry.branch, baseline: base, head: candidateHead });
    }
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test("real failed post-integration gate restores only the coordinator-owned commit", async () => {
  const root = await guidTempDirectory("orchestrator-integration-rollback-");
  const repository = join(root, "repository");
  const managed = join(root, "managed");
  const worktree = join(managed, "implementation");
  const branch = "codex/implementation";
  const git = await absoluteGit();
  const processAdapter = new BoundedProcess();
  const environment = gitEnvironment(process.env);
  const runGit = async (cwd: string, arguments_: readonly string[]) => await processAdapter.runStructured({
    commandId: "git-rollback-fixture", executable: git, arguments: syntheticGitArguments(arguments_), cwd, environment,
    timeoutMs: 120_000, maximumOutputBytes: 1_048_576,
  });
  try {
    await mkdir(repository, { recursive: true });
    assert.equal((await runGit(repository, ["init", "--initial-branch=codex/coordinator"])).evidence.result, "PASS");
    assert.equal((await runGit(repository, ["config", "user.name", "Orchestrator Fixture"])).evidence.result, "PASS");
    assert.equal((await runGit(repository, ["config", "user.email", "fixture@example.invalid"])).evidence.result, "PASS");
    await writeFile(join(repository, "baseline.txt"), "baseline\n", "utf8");
    assert.equal((await runGit(repository, ["add", "--", "baseline.txt"])).evidence.result, "PASS");
    assert.equal((await runGit(repository, ["commit", "-m", "test(orchestrator): create rollback baseline"])).evidence.result, "PASS");
    const base = (await runGit(repository, ["rev-parse", "HEAD"])).stdout.trim();
    const manager = new GitWorktreeManager(repository, managed, processAdapter, git, process.env);
    await manager.create("implementation", worktree, branch, base);
    await writeFile(join(worktree, "candidate.txt"), "candidate\n", "utf8");
    assert.equal((await runGit(worktree, ["add", "--", "candidate.txt"])).evidence.result, "PASS");
    assert.equal((await runGit(worktree, ["commit", "-m", "test(orchestrator): create rollback candidate"])).evidence.result, "PASS");
    const definition = task({ taskId: "implementation", owner: "implementation_worker", allowedPaths: ["candidate.txt"], worktree, branch });
    const candidate = await new GitCandidateInspector(processAdapter, git, process.env, trustedLanguagePolicy, passingLanguageChecker).inspect(definition, base, passingResult(["candidate.txt"]));
    const implementation = { ...definition, status: "IMPLEMENTED" as const, candidate, result: passingResult(["candidate.txt"]) };
    const integrationTask = task({
      taskId: "integration", taskKind: "INTEGRATION", owner: "governance_guard", candidateTaskId: definition.taskId,
      executionSurface: { ...task().executionSurface, cwd: repository, tools: [] },
    });
    const pipeline = new SequentialIntegrationPipeline(processAdapter, git, process.env, {
      run: async () => ({ commandId: "repository-ci-offline", exitCode: 1, durationMs: 1, result: "FAIL", relevantOutput: [] }),
    });
    await assert.rejects(
      pipeline.integrate({ baseline: base, expectedCoordinatorHead: base, integrationTask, implementationTask: implementation, candidate, workerResult: implementation.result, independentReview: null, securityReview: null }),
      (error: unknown) => error instanceof OrchestratorStop && error.code === "TEST_BASELINE_BROKEN",
    );
    assert.equal((await runGit(repository, ["rev-parse", "HEAD"])).stdout.trim(), base);
    assert.equal((await runGit(repository, ["status", "--porcelain=v1", "-z", "--untracked-files=all"])).stdout, "");
    await assert.rejects(readFile(join(repository, "candidate.txt"), "utf8"), (error: unknown) => (error as NodeJS.ErrnoException).code === "ENOENT");
    await manager.removeManaged("implementation", worktree, { branch, baseline: base, head: candidate.commitId });
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test("managed cleanup completes branch and marker removal after the worktree was already removed", async () => {
  const root = await guidTempDirectory("orchestrator-partial-cleanup-");
  const repository = join(root, "repository");
  const managed = join(root, "managed");
  const worktree = join(managed, "implementation");
  const branch = "codex/implementation";
  const git = await absoluteGit();
  const processAdapter = new BoundedProcess();
  const environment = gitEnvironment(process.env);
  const runGit = async (arguments_: readonly string[]) => await processAdapter.runStructured({
    commandId: "git-partial-cleanup-fixture", executable: git, arguments: syntheticGitArguments(arguments_), cwd: repository, environment,
    timeoutMs: 120_000, maximumOutputBytes: 1_048_576,
  });
  try {
    await mkdir(repository, { recursive: true });
    assert.equal((await runGit(["init", "--initial-branch=codex/coordinator"])).evidence.result, "PASS");
    assert.equal((await runGit(["config", "user.name", "Orchestrator Fixture"])).evidence.result, "PASS");
    assert.equal((await runGit(["config", "user.email", "fixture@example.invalid"])).evidence.result, "PASS");
    await writeFile(join(repository, "baseline.txt"), "baseline\n", "utf8");
    assert.equal((await runGit(["add", "--", "baseline.txt"])).evidence.result, "PASS");
    assert.equal((await runGit(["commit", "-m", "test(orchestrator): create cleanup baseline"])).evidence.result, "PASS");
    const base = (await runGit(["rev-parse", "HEAD"])).stdout.trim();
    const manager = new GitWorktreeManager(repository, managed, processAdapter, git, process.env);
    await manager.create("implementation", worktree, branch, base);
    assert.equal((await runGit(["worktree", "remove", worktree])).evidence.result, "PASS");
    await manager.removeManaged("implementation", worktree, { branch, baseline: base, head: base });
    assert.equal((await runGit(["rev-parse", "--verify", "--quiet", `refs/heads/${branch}`])).evidence.result, "FAIL");
    await assert.rejects(access(join(managed, ".owners", "implementation.json")), (error: unknown) => (error as NodeJS.ErrnoException).code === "ENOENT");
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test("managed cleanup rejects a recreated path after marker, record and branch are absent", async () => {
  const root = await guidTempDirectory("orchestrator-unowned-cleanup-");
  const repository = join(root, "repository");
  const managed = join(root, "managed");
  const worktree = join(managed, "implementation");
  const branch = "codex/implementation";
  const git = await absoluteGit();
  const processAdapter = new BoundedProcess();
  const environment = gitEnvironment(process.env);
  const runGit = async (arguments_: readonly string[]) => await processAdapter.runStructured({
    commandId: "git-unowned-cleanup-fixture", executable: git, arguments: syntheticGitArguments(arguments_), cwd: repository, environment,
    timeoutMs: 120_000, maximumOutputBytes: 1_048_576,
  });
  try {
    await mkdir(repository, { recursive: true });
    assert.equal((await runGit(["init", "--initial-branch=codex/coordinator"])).evidence.result, "PASS");
    assert.equal((await runGit(["config", "user.name", "Orchestrator Fixture"])).evidence.result, "PASS");
    assert.equal((await runGit(["config", "user.email", "fixture@example.invalid"])).evidence.result, "PASS");
    await writeFile(join(repository, "baseline.txt"), "baseline\n", "utf8");
    assert.equal((await runGit(["add", "--", "baseline.txt"])).evidence.result, "PASS");
    assert.equal((await runGit(["commit", "-m", "test(orchestrator): create unowned cleanup baseline"])).evidence.result, "PASS");
    const base = (await runGit(["rev-parse", "HEAD"])).stdout.trim();
    const manager = new GitWorktreeManager(repository, managed, processAdapter, git, process.env);
    await manager.create("implementation", worktree, branch, base);
    assert.equal((await runGit(["worktree", "remove", worktree])).evidence.result, "PASS");
    assert.equal((await runGit(["update-ref", "-d", `refs/heads/${branch}`, base])).evidence.result, "PASS");
    await unlink(join(managed, ".owners", "implementation.json"));
    await mkdir(worktree, { recursive: true });
    const sentinel = join(worktree, "foreign.txt");
    await writeFile(sentinel, "preserve\n", "utf8");
    await assert.rejects(
      manager.removeManaged("implementation", worktree, { branch, baseline: base, head: base }),
      (error: unknown) => error instanceof OrchestratorStop && error.code === "DESTRUCTIVE_OPERATION",
    );
    assert.equal(await readFile(sentinel, "utf8"), "preserve\n");
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test("managed cleanup preserves marker and branch when a removed worktree path is recreated", async () => {
  const root = await guidTempDirectory("orchestrator-recreated-cleanup-");
  const repository = join(root, "repository");
  const managed = join(root, "managed");
  const worktree = join(managed, "implementation");
  const marker = join(managed, ".owners", "implementation.json");
  const branch = "codex/implementation";
  const git = await absoluteGit();
  const processAdapter = new BoundedProcess();
  const environment = gitEnvironment(process.env);
  const runGit = async (arguments_: readonly string[]) => await processAdapter.runStructured({
    commandId: "git-recreated-cleanup-fixture", executable: git, arguments: syntheticGitArguments(arguments_), cwd: repository, environment,
    timeoutMs: 120_000, maximumOutputBytes: 1_048_576,
  });
  try {
    await mkdir(repository, { recursive: true });
    assert.equal((await runGit(["init", "--initial-branch=codex/coordinator"])).evidence.result, "PASS");
    assert.equal((await runGit(["config", "user.name", "Orchestrator Fixture"])).evidence.result, "PASS");
    assert.equal((await runGit(["config", "user.email", "fixture@example.invalid"])).evidence.result, "PASS");
    await writeFile(join(repository, "baseline.txt"), "baseline\n", "utf8");
    assert.equal((await runGit(["add", "--", "baseline.txt"])).evidence.result, "PASS");
    assert.equal((await runGit(["commit", "-m", "test(orchestrator): create recreated cleanup baseline"])).evidence.result, "PASS");
    const base = (await runGit(["rev-parse", "HEAD"])).stdout.trim();
    const manager = new GitWorktreeManager(repository, managed, processAdapter, git, process.env);
    await manager.create("implementation", worktree, branch, base);
    assert.equal((await runGit(["worktree", "remove", worktree])).evidence.result, "PASS");
    await mkdir(worktree, { recursive: true });
    await writeFile(join(worktree, "foreign.txt"), "preserve\n", "utf8");

    await assert.rejects(
      manager.removeManaged("implementation", worktree, { branch, baseline: base, head: base }),
      (error: unknown) => error instanceof OrchestratorStop && error.code === "DESTRUCTIVE_OPERATION",
    );
    assert.equal((await runGit(["rev-parse", "--verify", "--quiet", `refs/heads/${branch}`])).evidence.result, "PASS");
    assert.match(await readFile(marker, "utf8"), /"taskId":"implementation"/);

    await rm(worktree, { recursive: true });
    assert.equal((await runGit(["update-ref", "-d", `refs/heads/${branch}`, base])).evidence.result, "PASS");
    await mkdir(worktree, { recursive: true });
    await writeFile(join(worktree, "foreign.txt"), "preserve\n", "utf8");
    await assert.rejects(
      manager.removeManaged("implementation", worktree, { branch, baseline: base, head: base }),
      (error: unknown) => error instanceof OrchestratorStop && error.code === "DESTRUCTIVE_OPERATION",
    );
    assert.match(await readFile(marker, "utf8"), /"taskId":"implementation"/);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test("failed worktree creation preserves its marker and partial filesystem path", async () => {
  const root = await guidTempDirectory("orchestrator-failed-create-");
  const repository = join(root, "repository");
  const managed = join(root, "managed");
  const worktree = join(managed, "implementation");
  const branch = "codex/implementation";
  const git = await absoluteGit();
  const bounded = new BoundedProcess();
  const environment = gitEnvironment(process.env);
  const runGit = async (arguments_: readonly string[]) => await bounded.runStructured({
    commandId: "git-failed-create-fixture", executable: git, arguments: syntheticGitArguments(arguments_), cwd: repository, environment,
    timeoutMs: 120_000, maximumOutputBytes: 1_048_576,
  });
  try {
    await mkdir(repository, { recursive: true });
    assert.equal((await runGit(["init", "--initial-branch=codex/coordinator"])).evidence.result, "PASS");
    assert.equal((await runGit(["config", "user.name", "Orchestrator Fixture"])).evidence.result, "PASS");
    assert.equal((await runGit(["config", "user.email", "fixture@example.invalid"])).evidence.result, "PASS");
    await writeFile(join(repository, "baseline.txt"), "baseline\n", "utf8");
    assert.equal((await runGit(["add", "--", "baseline.txt"])).evidence.result, "PASS");
    assert.equal((await runGit(["commit", "-m", "test(orchestrator): create failed creation baseline"])).evidence.result, "PASS");
    const base = (await runGit(["rev-parse", "HEAD"])).stdout.trim();
    const processAdapter = new InterceptingGitProcess(bounded, async (request, next) => {
      if (request.commandId !== "worktree-create") return await next();
      await mkdir(worktree, { recursive: true });
      await writeFile(join(worktree, "partial.txt"), "preserve\n", "utf8");
      return { stdout: "", stderr: "creation failed", evidence: { commandId: request.commandId, exitCode: 1, durationMs: 1, result: "FAIL", relevantOutput: [] } };
    });
    const manager = new GitWorktreeManager(repository, managed, processAdapter, git, process.env);
    await assert.rejects(
      manager.create("implementation", worktree, branch, base),
      (error: unknown) => error instanceof OrchestratorStop && error.code === "DESTRUCTIVE_OPERATION",
    );
    assert.equal(await readFile(join(worktree, "partial.txt"), "utf8"), "preserve\n");
    assert.match(await readFile(join(managed, ".owners", "implementation.json"), "utf8"), /"taskId":"implementation"/);
    assert.equal((await runGit(["rev-parse", "--verify", "--quiet", `refs/heads/${branch}`])).evidence.result, "FAIL");
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test("created worktree rollback preserves its marker and branch when the path is recreated", async () => {
  const root = await guidTempDirectory("orchestrator-recreated-rollback-");
  const repository = join(root, "repository");
  const managed = join(root, "managed");
  const worktree = join(managed, "implementation");
  const branch = "codex/implementation";
  const git = await absoluteGit();
  const bounded = new BoundedProcess();
  const environment = gitEnvironment(process.env);
  const runGit = async (arguments_: readonly string[]) => await bounded.runStructured({
    commandId: "git-recreated-rollback-fixture", executable: git, arguments: syntheticGitArguments(arguments_), cwd: repository, environment,
    timeoutMs: 120_000, maximumOutputBytes: 1_048_576,
  });
  try {
    await mkdir(repository, { recursive: true });
    assert.equal((await runGit(["init", "--initial-branch=codex/coordinator"])).evidence.result, "PASS");
    assert.equal((await runGit(["config", "user.name", "Orchestrator Fixture"])).evidence.result, "PASS");
    assert.equal((await runGit(["config", "user.email", "fixture@example.invalid"])).evidence.result, "PASS");
    await writeFile(join(repository, "baseline.txt"), "baseline\n", "utf8");
    assert.equal((await runGit(["add", "--", "baseline.txt"])).evidence.result, "PASS");
    assert.equal((await runGit(["commit", "-m", "test(orchestrator): create rollback recreation baseline"])).evidence.result, "PASS");
    const base = (await runGit(["rev-parse", "HEAD"])).stdout.trim();
    const processAdapter = new InterceptingGitProcess(bounded, async (request, next) => {
      if (request.commandId === "worktree-status") {
        return { stdout: "", stderr: "validation failed", evidence: { commandId: request.commandId, exitCode: 1, durationMs: 1, result: "FAIL", relevantOutput: [] } };
      }
      const result = await next();
      if (request.commandId === "worktree-rollback-remove" && result.evidence.result === "PASS") {
        await mkdir(worktree, { recursive: true });
        await writeFile(join(worktree, "foreign.txt"), "preserve\n", "utf8");
      }
      return result;
    });
    const manager = new GitWorktreeManager(repository, managed, processAdapter, git, process.env);
    await assert.rejects(
      manager.create("implementation", worktree, branch, base),
      (error: unknown) => error instanceof OrchestratorStop && error.code === "DESTRUCTIVE_OPERATION",
    );
    assert.equal(await readFile(join(worktree, "foreign.txt"), "utf8"), "preserve\n");
    assert.match(await readFile(join(managed, ".owners", "implementation.json"), "utf8"), /"taskId":"implementation"/);
    assert.equal((await runGit(["rev-parse", "--verify", "--quiet", `refs/heads/${branch}`])).evidence.result, "PASS");
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

test("repository-local executable Git configuration classes are rejected before a sensitive operation", async () => {
  const root = await guidTempDirectory("orchestrator-git-policy-");
  const repository = join(root, "repository");
  const git = await absoluteGit();
  const processAdapter = new BoundedProcess();
  const environment = gitEnvironment(process.env);
  const runGit = async (arguments_: readonly string[]) => await processAdapter.runStructured({
    commandId: "git-policy-fixture", executable: git, arguments: syntheticGitArguments(arguments_), cwd: repository, environment,
    timeoutMs: 120_000, maximumOutputBytes: 1_048_576,
  });
  try {
    await mkdir(repository, { recursive: true });
    assert.equal((await runGit(["init", "--initial-branch=codex/coordinator"])).evidence.result, "PASS");
    const sentinel = join(root, "sentinel.txt");
    const proof = join(root, "helper-proof.txt");
    const helper = join(root, "sentinel-helper.cjs");
    await writeFile(helper, "require('node:fs').writeFileSync(process.argv[2], 'executed');\n", "utf8");
    const helperProof = await processAdapter.run({
      commandId: "git-policy-helper-proof", executable: process.execPath, arguments: [helper, proof], cwd: root, environment: {},
      timeoutMs: 10_000, maximumOutputBytes: 4096,
    });
    assert.equal(helperProof.result, "PASS");
    assert.equal(await readFile(proof, "utf8"), "executed");
    await rm(proof);
    const manager = new GitWorktreeManager(repository, join(root, "managed"), processAdapter, git, process.env);
    for (const name of [
      "filter.evil.clean", "filter.evil.smudge", "filter.evil.process", "diff.evil.textconv", "merge.evil.driver", "core.hooksPath",
    ]) {
      assert.equal((await runGit(["config", name, `"${process.execPath}" "${helper}" "${sentinel}"`])).evidence.result, "PASS");
      await assert.rejects(manager.list(), (error: unknown) => error instanceof OrchestratorStop && error.code === "OUT_OF_SCOPE_CHANGE_REQUIRED");
      await assert.rejects(access(sentinel), (error: unknown) => (error as NodeJS.ErrnoException).code === "ENOENT");
      assert.equal((await runGit(["config", "--unset-all", name])).evidence.result, "PASS");
    }
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});
