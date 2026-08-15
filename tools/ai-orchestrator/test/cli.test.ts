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
      tasks: [task({ status: "PASS", startedAt: instant, finishedAt: instant, result: passingResult() })],
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

test("CLI confirmed cleanup removes a terminal run through a same-parent tombstone", async () => {
  const temporary = await testStateRoot("cli-cleanup");
  try {
    const run = {
      schemaVersion: 1 as const, runId: "run-cleanup", revision: 0, baseline, maxConcurrency: 1,
      createdAt: instant, updatedAt: instant,
      tasks: [task({ status: "PASS", startedAt: instant, finishedAt: instant, result: passingResult() })],
      attempts: [], heldLocks: [], humanGateReached: false,
    };
    await new FileStateStore(temporary).save(run);
    const cleanup = await execute(process.execPath, [cliPath, "cleanup", "--run-id", run.runId, "--state-root", temporary, "--confirm-run-id", run.runId], { cwd: packageRoot });
    assert.equal((JSON.parse(cleanup.stdout) as { action: string }).action, "REMOVED");
    await assert.rejects(readdir(join(temporary, run.runId)), (error: unknown) => (error as NodeJS.ErrnoException).code === "ENOENT");
  } finally {
    await rm(temporary, { recursive: true, force: true });
  }
});
