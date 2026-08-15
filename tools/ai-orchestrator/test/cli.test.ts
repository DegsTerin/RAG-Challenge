// Purpose: Proves that CLI dry-run is side-effect free and that cleanup defaults to report-only behaviour.
import assert from "node:assert/strict";
import { execFile } from "node:child_process";
import { mkdir, mkdtemp, readFile, readdir, rm, writeFile } from "node:fs/promises";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { promisify } from "node:util";
import test from "node:test";
import { FileStateStore } from "../src/adapters/file-state-store.js";
import { baseline, instant, passingResult, projectPlan, task } from "./helpers.js";

const execute = promisify(execFile);
const packageRoot = resolve(dirname(fileURLToPath(import.meta.url)), "../..");
const cliPath = join(packageRoot, "dist", "src", "cli.js");
const repositoryRoot = resolve(packageRoot, "../..");
const permittedStateRoot = resolve(repositoryRoot, "artifacts-local", "ai-orchestrator");

async function testStateRoot(prefix: string): Promise<string> {
  await mkdir(permittedStateRoot, { recursive: true });
  return await mkdtemp(join(permittedStateRoot, `${prefix}-`));
}

test("CLI plan emits a preview without creating its configured state root", async () => {
  const temporary = await testStateRoot("cli-plan");
  try {
    const planPath = join(temporary, "plan.json");
    const stateRoot = join(temporary, "state");
    await writeFile(planPath, JSON.stringify(projectPlan([task({ executionSurface: { ...task().executionSurface, cwd: repositoryRoot } })])), "utf8");
    const { stdout } = await execute(process.execPath, [cliPath, "plan", "--plan", planPath, "--state-root", stateRoot], { cwd: packageRoot });
    const preview = JSON.parse(stdout) as { tasks: string[]; waves: string[][] };
    assert.deepEqual(preview.tasks, ["map-repository"]);
    assert.deepEqual(preview.waves, [["map-repository"]]);
    await assert.rejects(readdir(stateRoot), (error: unknown) => (error as NodeJS.ErrnoException).code === "ENOENT");
  } finally {
    await rm(temporary, { recursive: true, force: true });
  }
});

test("CLI status and cleanup report do not mutate a completed run", async () => {
  const temporary = await testStateRoot("cli-state");
  try {
    const run = {
      schemaVersion: 1 as const,
      runId: "run-cli",
      revision: 0,
      baseline,
      maxConcurrency: 3,
      createdAt: instant,
      updatedAt: instant,
      tasks: [task({ status: "PASS", startedAt: instant, finishedAt: instant, result: passingResult(), executionSurface: { ...task().executionSurface, cwd: repositoryRoot } })],
      attempts: [],
      heldLocks: [],
      humanGateReached: false,
    };
    await new FileStateStore(temporary).save(run);
    const snapshot = join(temporary, "run-cli", "state-00000000.json");
    const before = await readFile(snapshot, "utf8");
    const status = await execute(process.execPath, [cliPath, "status", "--run-id", "run-cli", "--state-root", temporary], { cwd: packageRoot });
    const cleanup = await execute(process.execPath, [cliPath, "cleanup", "--run-id", "run-cli", "--state-root", temporary], { cwd: packageRoot });
    assert.equal((JSON.parse(status.stdout) as { runId: string }).runId, "run-cli");
    assert.equal((JSON.parse(cleanup.stdout) as { action: string }).action, "REPORT_ONLY");
    assert.equal(await readFile(snapshot, "utf8"), before);
  } finally {
    await rm(temporary, { recursive: true, force: true });
  }
});

test("CLI confirmed cleanup quarantines a terminal run through a same-parent tombstone", async () => {
  const temporary = await testStateRoot("cli-cleanup");
  try {
    const run = {
      schemaVersion: 1 as const, runId: "run-cleanup", revision: 0, baseline, maxConcurrency: 1,
      createdAt: instant, updatedAt: instant,
      tasks: [task({ status: "PASS", startedAt: instant, finishedAt: instant, result: passingResult(), executionSurface: { ...task().executionSurface, cwd: repositoryRoot } })],
      attempts: [], heldLocks: [], humanGateReached: false,
    };
    await new FileStateStore(temporary).save(run);
    const cleanup = await execute(process.execPath, [cliPath, "cleanup", "--run-id", run.runId, "--state-root", temporary, "--confirm-run-id", run.runId], { cwd: packageRoot });
    assert.equal((JSON.parse(cleanup.stdout) as { action: string }).action, "QUARANTINED");
    await assert.rejects(readdir(join(temporary, run.runId)), (error: unknown) => (error as NodeJS.ErrnoException).code === "ENOENT");
    assert.ok((await readdir(temporary)).includes(`${run.runId}.cleanup`));
  } finally {
    await rm(temporary, { recursive: true, force: true });
  }
});

test("Human Gate status includes bounded findings and risks that can be evaluated", async () => {
  const temporary = await testStateRoot("cli-human-gate");
  const finding = "finding:SEC-001|severity:P1|location:tools/fixture/result.txt:7|summary:Unsafe fixture boundary";
  const risk = "risk:SEC-001|severity:HIGH|summary:Fixture integrity can fail|mitigation:Keep the validation boundary closed";
  const reviewResult = { ...passingResult(), evidence: [finding], risks: [risk] };
  const candidate = { commitId: "1".repeat(40), treeId: "2".repeat(40), changedFiles: ["tools/fixture/result.txt"] };
  const readOnlySurface = { ...task().executionSurface, cwd: repositoryRoot };
  const implementation = task({
    taskId: "implementation", owner: "implementation_worker", status: "IMPLEMENTED", allowedPaths: ["tools/fixture"],
    worktree: resolve(repositoryRoot, "..", "managed", "implementation"), branch: "codex/implementation", candidate,
    requiresIndependentReview: true, requiresSecurityReview: true, startedAt: instant, finishedAt: instant, result: passingResult(candidate.changedFiles),
  });
  const independent = task({ taskId: "independent-review", owner: "independent_reviewer", dependencies: [implementation.taskId], candidateTaskId: implementation.taskId, status: "PASS", executionSurface: readOnlySurface, startedAt: instant, finishedAt: instant, result: passingResult() });
  const review = task({ taskId: "security-review", owner: "security_reviewer", dependencies: [implementation.taskId], candidateTaskId: implementation.taskId, status: "PASS", executionSurface: readOnlySurface, startedAt: instant, finishedAt: instant, result: reviewResult });
  const integration = task({
    taskId: "integration", taskKind: "INTEGRATION", owner: "governance_guard", dependencies: [implementation.taskId, independent.taskId, review.taskId],
    candidateTaskId: implementation.taskId, status: "PASS", executionSurface: { ...readOnlySurface, writableRoots: [repositoryRoot], sandbox: "workspace-write", tools: [] },
    ownership: "COORDINATOR_ONLY", parallelism: "SEQUENTIAL_ONLY", candidate, startedAt: instant, finishedAt: instant, result: passingResult(candidate.changedFiles),
  });
  const qualityResult = { ...passingResult(), tests: [{ commandId: "repository-ci-offline", exitCode: 0, durationMs: 1, result: "PASS" as const, relevantOutput: [] }] };
  const quality = task({ taskId: "quality-gate", taskKind: "QUALITY_GATE", owner: "governance_guard", dependencies: [integration.taskId], status: "PASS", executionSurface: { ...readOnlySurface, writableRoots: [repositoryRoot], sandbox: "workspace-write", tools: [] }, ownership: "COORDINATOR_ONLY", parallelism: "SEQUENTIAL_ONLY", requiredTests: ["./eng/ci.ps1 -Offline"], startedAt: instant, finishedAt: instant, result: qualityResult });
  const human = task({ taskId: "human-gate", taskKind: "HUMAN_GATE", owner: "governance_guard", dependencies: [quality.taskId], status: "HUMAN_REVIEW_REQUIRED", humanGate: true, executionSurface: { ...readOnlySurface, tools: [] }, ownership: "HUMAN_CONTROLLED", parallelism: "SEQUENTIAL_ONLY", startedAt: instant, finishedAt: instant });
  const run = {
    schemaVersion: 1 as const, runId: "run-human", revision: 0, baseline, maxConcurrency: 1, createdAt: instant, updatedAt: instant,
    tasks: [implementation, independent, review, integration, quality, human],
    attempts: [implementation, independent, review, integration, quality].map((entry, index) => ({
      attemptId: `attempt-${entry.taskId}-${index + 1}`, taskId: entry.taskId, agentId: entry.owner,
      startedAt: instant, finishedAt: instant, retryClass: null,
      threadId: ["INTEGRATION", "QUALITY_GATE"].includes(entry.taskKind) ? null : `thread-${entry.taskId}`,
      result: entry.result,
    })),
    heldLocks: [], humanGateReached: true,
  };
  try {
    await new FileStateStore(temporary).save(run);
    const status = await execute(process.execPath, [cliPath, "status", "--run-id", run.runId, "--state-root", temporary], { cwd: packageRoot });
    const summary = JSON.parse(status.stdout) as { humanGatePackage: { findingReferences: string[]; riskReferences: string[]; requestedDecision: string; decisionReady: boolean; evidenceAuthenticity: string } };
    assert.deepEqual(summary.humanGatePackage.findingReferences, [finding]);
    assert.deepEqual(summary.humanGatePackage.riskReferences, [risk]);
    assert.equal(summary.humanGatePackage.decisionReady, false);
    assert.equal(summary.humanGatePackage.evidenceAuthenticity, "LOCAL_UNAUTHENTICATED");
    assert.match(summary.humanGatePackage.requestedDecision, /No Human Gate decision is requested/);
    await assert.rejects(execute(process.execPath, [cliPath, "validate", "--run-id", run.runId, "--state-root", temporary], { cwd: packageRoot }), /requires live Git and canonical offline quality validation/);
    const noAttempts = { ...run, runId: "run-human-without-attempts", attempts: [] };
    await new FileStateStore(temporary).save(noAttempts);
    await assert.rejects(execute(process.execPath, [cliPath, "status", "--run-id", noAttempts.runId, "--state-root", temporary], { cwd: packageRoot }), /coherent terminal coordinator attempt/);
    const unobservedQualityResult = passingResult();
    const unobservedQuality = {
      ...run,
      runId: "run-human-without-quality-evidence",
      tasks: run.tasks.map((entry) => entry.taskId === quality.taskId ? { ...entry, result: unobservedQualityResult } : entry),
      attempts: run.attempts.map((attempt) => attempt.taskId === quality.taskId ? { ...attempt, result: unobservedQualityResult } : attempt),
    };
    await new FileStateStore(temporary).save(unobservedQuality);
    await assert.rejects(execute(process.execPath, [cliPath, "status", "--run-id", unobservedQuality.runId, "--state-root", temporary], { cwd: packageRoot }), /lacks the canonical coordinator-observed gate evidence/);
    const forged = {
      ...run,
      runId: "run-forged-human",
      tasks: run.tasks.map((entry) => entry.taskId === quality.taskId
        ? { ...entry, status: "BLOCKED" as const, result: { ...passingResult(), status: "BLOCKED" as const, stopCondition: "TEST_BASELINE_BROKEN" as const } }
        : entry),
    };
    await new FileStateStore(temporary).save(forged);
    await assert.rejects(execute(process.execPath, [cliPath, "status", "--run-id", forged.runId, "--state-root", temporary], { cwd: packageRoot }), /HUMAN_GATE_REQUIRED/);
  } finally {
    await rm(temporary, { recursive: true, force: true });
  }
});
