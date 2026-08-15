// Purpose: Exposes explicit, fail-closed plan, run, resume, status, validate and cleanup commands for the standalone tool.
import { readFile, readdir, rm } from "node:fs/promises";
import { dirname, isAbsolute, relative, resolve, sep } from "node:path";
import { fileURLToPath } from "node:url";
import { BoundedProcess } from "./adapters/bounded-process.js";
import { FakeAgentRunner } from "./adapters/fake-agent-runner.js";
import { FileResourceLocks } from "./adapters/file-resource-locks.js";
import { FileStateStore } from "./adapters/file-state-store.js";
import { GitBaselineVerifier } from "./adapters/git-baseline.js";
import { RepositoryQualityGate } from "./adapters/quality-gate.js";
import { Coordinator } from "./application/coordinator.js";
import { createDryRunPlan } from "./application/plan.js";
import { canonicalJson } from "./core/canonical-json.js";
import type { AgentResult, PersistedRunState } from "./core/contracts.js";
import { errorMessage, OrchestratorStop } from "./core/errors.js";
import { parseAgentResult, parseProjectPlan } from "./core/validation.js";
import type { EventSink, StructuredEvent } from "./observability/structured-log.js";
import { resolveRunRoot } from "./security/path-policy.js";

const packageRoot = resolve(dirname(fileURLToPath(import.meta.url)), "../..");
const repositoryRoot = resolve(packageRoot, "../..");
const defaultStateRoot = resolve(repositoryRoot, "artifacts-local", "ai-orchestrator");

class ConsoleEventSink implements EventSink {
  public write(event: StructuredEvent): void {
    process.stderr.write(canonicalJson(event));
  }
}

function valueAfter(arguments_: readonly string[], name: string): string | null {
  const index = arguments_.indexOf(name);
  if (index < 0) {
    return null;
  }
  const value = arguments_[index + 1];
  if (value === undefined || value.startsWith("--")) {
    throw new OrchestratorStop("AMBIGUOUS_AUTHORITY", `Argument '${name}' requires a value.`);
  }
  return value;
}

function hasFlag(arguments_: readonly string[], name: string): boolean {
  return arguments_.includes(name);
}

function requiredValue(arguments_: readonly string[], name: string): string {
  const value = valueAfter(arguments_, name);
  if (value === null) {
    throw new OrchestratorStop("AMBIGUOUS_AUTHORITY", `Argument '${name}' is required.`);
  }
  return value;
}

function stateRoot(arguments_: readonly string[]): string {
  const configured = valueAfter(arguments_, "--state-root");
  const candidate = configured === null ? defaultStateRoot : resolve(configured);
  const relation = relative(defaultStateRoot, candidate);
  if (relation === ".." || relation.startsWith(`..${sep}`) || isAbsolute(relation)) {
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The state root must remain within artifacts-local/ai-orchestrator.");
  }
  return candidate;
}

function safeEnvironment(): Readonly<Record<string, string>> {
  const allowedNames = ["PATH", "SystemRoot", "TEMP", "TMP", "USERPROFILE", "LOCALAPPDATA", "APPDATA"] as const;
  return Object.fromEntries(allowedNames.flatMap((name) => {
    const value = process.env[name];
    return value === undefined ? [] : [[name, value]];
  }));
}

async function readJson(path: string): Promise<unknown> {
  return JSON.parse(await readFile(resolve(path), "utf8")) as unknown;
}

async function fakeOutcomes(path: string): Promise<ReadonlyMap<string, AgentResult>> {
  const value = await readJson(path);
  if (value === null || typeof value !== "object" || Array.isArray(value)) {
    throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", "Fake results must be an object keyed by task ID.");
  }
  return new Map(Object.entries(value as Record<string, unknown>).map(([taskId, result]) => [taskId, parseAgentResult(result)]));
}

function stateSummary(state: PersistedRunState): unknown {
  return {
    schemaVersion: state.schemaVersion,
    runId: state.runId,
    revision: state.revision,
    baseline: state.baseline,
    updatedAt: state.updatedAt,
    humanGateReached: state.humanGateReached,
    heldLocks: state.heldLocks,
    tasks: state.tasks.map((task) => ({ taskId: task.taskId, owner: task.owner, status: task.status, stopCondition: task.result?.stopCondition ?? null })),
    attempts: state.attempts.map((attempt) => ({ attemptId: attempt.attemptId, taskId: attempt.taskId, retryClass: attempt.retryClass })),
  };
}

async function runCommand(arguments_: readonly string[], resume: boolean): Promise<void> {
  const runnerName = valueAfter(arguments_, "--runner") ?? "disabled";
  if (runnerName !== "fake") {
    throw new OrchestratorStop(
      "HUMAN_DECISION_REQUIRED",
      "CLI agent execution is deny-by-default; this authority permits only --runner fake. Real Codex requires a separate execution envelope.",
    );
  }
  const resultsPath = requiredValue(arguments_, "--fixture-results");
  const root = stateRoot(arguments_);
  const store = new FileStateStore(root);
  const plan = resume ? null : parseProjectPlan(await readJson(requiredValue(arguments_, "--plan")));
  const recovered = resume ? await store.load(requiredValue(arguments_, "--run-id")) : null;
  await new GitBaselineVerifier(new BoundedProcess(), safeEnvironment()).verify(
    repositoryRoot,
    plan?.baseline ?? recovered?.baseline ?? "",
  );
  const coordinator = new Coordinator(
    new FakeAgentRunner(await fakeOutcomes(resultsPath)),
    store,
    new FileResourceLocks(resolve(root, "locks")),
    new ConsoleEventSink(),
  );
  const state = resume
    ? await coordinator.resume(requiredValue(arguments_, "--run-id"), Number(valueAfter(arguments_, "--max-concurrency") ?? "3"))
    : await coordinator.start(plan ?? (() => { throw new OrchestratorStop("TEST_BASELINE_BROKEN", "The execution plan was not loaded."); })());
  process.stdout.write(canonicalJson(stateSummary(state)));
}

async function validateCommand(arguments_: readonly string[]): Promise<void> {
  const root = stateRoot(arguments_);
  const state = await new FileStateStore(root).load(requiredValue(arguments_, "--run-id"));
  const locks = await new FileResourceLocks(resolve(root, "locks")).inspect();
  if (state.heldLocks.length > 0 || locks.length > 0) {
    throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", "Validation found unreconciled resource locks.");
  }
  let qualityGate = null;
  if (hasFlag(arguments_, "--quality-gate")) {
    qualityGate = await new RepositoryQualityGate(new BoundedProcess(), safeEnvironment()).run(repositoryRoot);
    if (qualityGate.result !== "PASS") {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "The canonical repository quality gate failed.");
    }
  }
  process.stdout.write(canonicalJson({ state: stateSummary(state), qualityGate }));
}

async function cleanupCommand(arguments_: readonly string[]): Promise<void> {
  const root = stateRoot(arguments_);
  const runId = requiredValue(arguments_, "--run-id");
  const runRoot = resolveRunRoot(root, runId);
  const state = await new FileStateStore(root).load(runId);
  const terminal = state.tasks.every((task) => ["PASS", "FAIL", "HUMAN_REVIEW_REQUIRED", "CANCELLED", "BLOCKED"].includes(task.status));
  const confirmation = valueAfter(arguments_, "--confirm-run-id");
  if (confirmation === null) {
    const files = (await readdir(runRoot)).sort();
    process.stdout.write(canonicalJson({ runId, action: "REPORT_ONLY", terminal, files }));
    return;
  }
  if (confirmation !== runId || !terminal || state.heldLocks.length > 0 || (await new FileResourceLocks(resolve(root, "locks")).inspect()).length > 0) {
    throw new OrchestratorStop("DESTRUCTIVE_OPERATION", "Cleanup requires exact run confirmation, terminal tasks and no locks.");
  }
  await rm(runRoot, { recursive: true, force: false });
  process.stdout.write(canonicalJson({ runId, action: "REMOVED" }));
}

async function main(): Promise<void> {
  const [command, ...arguments_] = process.argv.slice(2);
  if (command === "plan" || (command === "run" && hasFlag(arguments_, "--dry-run"))) {
    const plan = parseProjectPlan(await readJson(requiredValue(arguments_, "--plan")));
    process.stdout.write(canonicalJson(createDryRunPlan(plan)));
  } else if (command === "run") {
    await runCommand(arguments_, false);
  } else if (command === "resume") {
    await runCommand(arguments_, true);
  } else if (command === "status") {
    const state = await new FileStateStore(stateRoot(arguments_)).load(requiredValue(arguments_, "--run-id"));
    process.stdout.write(canonicalJson(stateSummary(state)));
  } else if (command === "validate") {
    await validateCommand(arguments_);
  } else if (command === "cleanup") {
    await cleanupCommand(arguments_);
  } else {
    throw new OrchestratorStop("AMBIGUOUS_AUTHORITY", "Usage: <status|plan|run|resume|validate|cleanup> with explicit command arguments.");
  }
}

try {
  await main();
} catch (error) {
  const code = error instanceof OrchestratorStop ? error.code : "TEST_BASELINE_BROKEN";
  process.stderr.write(canonicalJson({ status: "BLOCKED", stopCondition: code, message: errorMessage(error) }));
  process.exitCode = 1;
}
