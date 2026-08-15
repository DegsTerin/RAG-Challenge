// Purpose: Exercises duplicate JSON, bounded persistence, process denial and a disposable real Git worktree lifecycle.
import assert from "node:assert/strict";
import { access, mkdir, mkdtemp, readFile, rm, symlink, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { delimiter, join, resolve } from "node:path";
import test from "node:test";
import { BoundedProcess } from "../src/adapters/bounded-process.js";
import { FileResourceLocks } from "../src/adapters/file-resource-locks.js";
import { FileStateStore } from "../src/adapters/file-state-store.js";
import { GitCandidateInspector } from "../src/adapters/git-candidate-inspector.js";
import { GitWorktreeManager } from "../src/adapters/git-worktrees.js";
import type { PersistedRunState } from "../src/core/contracts.js";
import { OrchestratorStop } from "../src/core/errors.js";
import type { ProcessRequest } from "../src/ports/process-executor.js";
import { gitArguments, gitEnvironment } from "../src/security/git-process-policy.js";
import { parseSecureJson } from "../src/security/secure-json.js";
import { assertNoExistingReparseBoundary } from "../src/security/path-policy.js";
import { baseline, instant, passingResult, task } from "./helpers.js";

function state(): PersistedRunState {
  return {
    schemaVersion: 1, runId: "run-security", revision: 0, baseline, maxConcurrency: 1,
    createdAt: instant, updatedAt: instant, tasks: [task()], attempts: [], heldLocks: [], humanGateReached: false,
  };
}

test("secure JSON rejects duplicate keys before schema validation", () => {
  assert.throws(() => parseSecureJson('{"status":"PASS","status":"FAIL"}', "fixture"), /duplicate key 'status'/);
  assert.throws(() => parseSecureJson(`${"[".repeat(65)}null${"]".repeat(65)}`, "fixture"), /maximum structural depth/);
});

test("existing symbolic-link or junction boundaries are rejected", async () => {
  const root = await mkdtemp(join(tmpdir(), "orchestrator-reparse-"));
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

test("state and lock reads fail closed on oversized or ambiguous local artefacts", async () => {
  const root = await mkdtemp(join(tmpdir(), "orchestrator-bounds-"));
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
    assert.deepEqual(report, [{ lockId: lockName, status: "INVALID", runId: null, taskId: null, attemptId: null, acquiredAt: null }]);
    assert.equal(await readFile(join(lockRoot, lockName), "utf8"), '{"schemaVersion":1,"schemaVersion":1}');
  } finally {
    await rm(root, { recursive: true, force: true });
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

test("real disposable Git worktree binds branch, clean baseline, candidate diff and task-owned cleanup", async () => {
  const root = await mkdtemp(join(tmpdir(), "orchestrator-git-"));
  const repository = join(root, "repository");
  const managed = join(root, "managed");
  const worktree = join(managed, "implementation");
  const git = await absoluteGit();
  const processAdapter = new BoundedProcess();
  const environment = gitEnvironment(process.env);
  const runGit = async (cwd: string, arguments_: readonly string[]) => await processAdapter.runStructured({
    commandId: "git-fixture", executable: git, arguments: gitArguments(arguments_), cwd, environment,
    timeoutMs: 120_000, maximumOutputBytes: 1_048_576,
  });
  try {
    await mkdir(repository, { recursive: true });
    assert.equal((await runGit(repository, ["init", "--initial-branch=codex/coordinator"])).evidence.result, "PASS");
    assert.equal((await runGit(repository, ["config", "user.name", "Orchestrator Fixture"])).evidence.result, "PASS");
    assert.equal((await runGit(repository, ["config", "user.email", "fixture@example.invalid"])).evidence.result, "PASS");
    await writeFile(join(repository, "fixture.txt"), "baseline\n", "utf8");
    assert.equal((await runGit(repository, ["add", "--", "fixture.txt"])).evidence.result, "PASS");
    assert.equal((await runGit(repository, ["commit", "-m", "test(orchestrator): create disposable baseline"])).evidence.result, "PASS");
    const base = (await runGit(repository, ["rev-parse", "HEAD"])).stdout.trim();
    assert.match(base, /^[0-9a-f]{40}$/);

    const manager = new GitWorktreeManager(repository, managed, processAdapter, git, process.env);
    const created = await manager.create("implementation", worktree, "codex/implementation", base);
    assert.equal(created.head, base);
    await writeFile(join(worktree, "fixture.txt"), "candidate\n", "utf8");
    assert.equal((await runGit(worktree, ["add", "--", "fixture.txt"])).evidence.result, "PASS");
    assert.equal((await runGit(worktree, ["commit", "-m", "test(orchestrator): create disposable candidate"])).evidence.result, "PASS");
    const implementation = task({ taskId: "implementation", owner: "implementation_worker", allowedPaths: ["fixture.txt"], worktree, branch: "codex/implementation" });
    const candidate = await new GitCandidateInspector(processAdapter, git, process.env).inspect(implementation, base, passingResult(["fixture.txt"]));
    assert.deepEqual(candidate.changedFiles, ["fixture.txt"]);
    await manager.removeManaged("implementation", worktree);
    assert.equal((await manager.list()).some((record) => resolve(record.path) === resolve(worktree)), false);
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});
