// Purpose: Exposes explicit, fail-closed plan, run, resume, status, validate and cleanup commands for the standalone tool.
import { readdir, rename, stat } from "node:fs/promises";
import { dirname, isAbsolute, relative, resolve, sep } from "node:path";
import { fileURLToPath } from "node:url";
import { BoundedProcess } from "./adapters/bounded-process.js";
import { CodexRunner } from "./adapters/codex-runner.js";
import { FakeAgentRunner } from "./adapters/fake-agent-runner.js";
import { FileResourceLocks } from "./adapters/file-resource-locks.js";
import { FileStateStore } from "./adapters/file-state-store.js";
import { FileThreadCheckpointStore } from "./adapters/file-thread-checkpoints.js";
import { GitBaselineVerifier } from "./adapters/git-baseline.js";
import { GitCandidateInspector } from "./adapters/git-candidate-inspector.js";
import { GitWorktreeManager } from "./adapters/git-worktrees.js";
import { RepositoryQualityGate } from "./adapters/quality-gate.js";
import { Coordinator } from "./application/coordinator.js";
import { SequentialIntegrationPipeline } from "./application/integration.js";
import { createDryRunPlan, persistedCoordinatorHead, validatePersistedStateSemantics } from "./application/plan.js";
import { canonicalJson } from "./core/canonical-json.js";
import type { AgentResult, AgentRunner, PersistedRunState } from "./core/contracts.js";
import { errorMessage, OrchestratorStop } from "./core/errors.js";
import { parseAgentResult, parseProjectPlan } from "./core/validation.js";
import type { EventSink, StructuredEvent } from "./observability/structured-log.js";
import { assertNoExistingReparseBoundary, readBoundedRegularFile, resolveRunRoot } from "./security/path-policy.js";
import { parseSecureJson } from "./security/secure-json.js";
import {
  assertAuthorityReference,
  assertCliArgumentsContainNoSecretMaterial,
} from "./security/secret-policy.js";
import { loadTrustedLanguagePolicy } from "./security/language-policy.js";

const packageRoot = resolve(dirname(fileURLToPath(import.meta.url)), "../..");
const repositoryRoot = resolve(packageRoot, "../..");
const defaultStateRoot = resolve(repositoryRoot, "artifacts-local", "ai-orchestrator");
const managedWorktreeRoot = resolve(repositoryRoot, "..", "RAG-Challenge-worktrees");

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
  const resolved = resolve(path);
  const text = await readBoundedRegularFile(dirname(resolved), resolved, 8_388_608, "JSON input", "CONFLICTING_REQUIREMENTS");
  return parseSecureJson(text, "JSON input", "CONFLICTING_REQUIREMENTS");
}

async function fakeOutcomes(path: string): Promise<ReadonlyMap<string, AgentResult>> {
  const value = await readJson(path);
  if (value === null || typeof value !== "object" || Array.isArray(value)) {
    throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", "Fake results must be an object keyed by task ID.");
  }
  return new Map(Object.entries(value as Record<string, unknown>).map(([taskId, result]) => [taskId, parseAgentResult(result)]));
}

export function stateSummary(state: PersistedRunState, humanGateLiveValidated = false): unknown {
  const implementationTasks = state.tasks.filter((task) => task.taskKind === "IMPLEMENTATION");
  const reviewTasks = state.tasks.filter((task) => ["INDEPENDENT_REVIEW", "SECURITY_REVIEW"].includes(task.taskKind));
  const qualityTasks = state.tasks.filter((task) => task.taskKind === "QUALITY_GATE");
  const humanGateTask = state.tasks.find((task) => task.taskKind === "HUMAN_GATE") ?? null;
  return {
    schemaVersion: state.schemaVersion,
    runId: state.runId,
    revision: state.revision,
    baseline: state.baseline,
    maxConcurrency: state.maxConcurrency,
    updatedAt: state.updatedAt,
    humanGateReached: state.humanGateReached,
    heldLocks: state.heldLocks,
    tasks: state.tasks.map((task) => ({ taskId: task.taskId, owner: task.owner, status: task.status, stopCondition: task.result?.stopCondition ?? null })),
    attempts: state.attempts.map((attempt) => ({ attemptId: attempt.attemptId, taskId: attempt.taskId, retryClass: attempt.retryClass, threadId: attempt.threadId })),
    humanGatePackage: state.humanGateReached ? {
      evidenceAuthenticity: humanGateLiveValidated ? "LOCAL_UNAUTHENTICATED_LIVE_REVALIDATED" : "LOCAL_UNAUTHENTICATED",
      decisionReady: humanGateLiveValidated,
      baseline: state.baseline,
      implementedCandidates: implementationTasks.map((task) => ({ taskId: task.taskId, status: task.status, candidate: task.candidate })),
      remainingWork: state.tasks.filter((task) => !["PASS", "IMPLEMENTED", "HUMAN_REVIEW_REQUIRED"].includes(task.status)).map((task) => ({ taskId: task.taskId, status: task.status })),
      reviews: reviewTasks.map((task) => ({ taskId: task.taskId, kind: task.taskKind, status: task.status, stopCondition: task.result?.stopCondition ?? null })),
      qualityGates: qualityTasks.map((task) => ({ taskId: task.taskId, status: task.status, tests: task.result?.tests ?? [] })),
      changedFiles: [...new Set(implementationTasks.flatMap((task) => task.candidate?.changedFiles ?? []))].sort(),
      findingReferences: [...new Set(reviewTasks.flatMap((task) => task.result?.evidence ?? []).filter((value) => value.startsWith("finding:")))].sort(),
      riskReferences: [...new Set(state.tasks.flatMap((task) => task.result?.risks ?? []).filter((value) => value.startsWith("risk:") && !value.startsWith("risk:sha256:")))].sort(),
      evidenceDigests: [...new Set(reviewTasks.flatMap((task) => task.result?.evidence ?? []).filter((value) => value.startsWith("evidence:sha256:")))].sort(),
      knownRiskDigests: [...new Set(state.tasks.flatMap((task) => task.result?.risks ?? []).filter((value) => value.startsWith("risk:sha256:")))].sort(),
      gate: humanGateTask === null ? null : { taskId: humanGateTask.taskId, title: humanGateTask.title, objective: humanGateTask.objective, status: humanGateTask.status },
      requestedDecision: humanGateTask === null
        ? "No Human Gate task is available."
        : humanGateLiveValidated
          ? `Accept or reject Human Gate '${humanGateTask.taskId}' externally after reviewing this exact package; the orchestrator does not approve or advance the lifecycle.`
          : "No Human Gate decision is requested from this unauthenticated local report; run live validation with the canonical offline quality gate and exact Git HEAD first.",
    } : null,
  };
}

async function runCommand(arguments_: readonly string[], resume: boolean): Promise<void> {
  const runnerName = valueAfter(arguments_, "--runner") ?? "disabled";
  if (!["fake", "codex"].includes(runnerName)) {
    throw new OrchestratorStop(
      "HUMAN_DECISION_REQUIRED",
      "CLI agent execution is deny-by-default; select --runner fake or an explicitly authorised --runner codex envelope.",
    );
  }
  const fixtureResultsPath = runnerName === "fake" ? requiredValue(arguments_, "--fixture-results") : null;
  const codexAuthorityReference = runnerName === "codex" ? requiredValue(arguments_, "--authority-reference") : null;
  if (codexAuthorityReference !== null) assertAuthorityReference(codexAuthorityReference, "Codex authority reference");
  const root = stateRoot(arguments_);
  const store = new FileStateStore(root, repositoryRoot);
  const plan = resume ? null : parseProjectPlan(await readJson(requiredValue(arguments_, "--plan")));
  const recovered = resume ? await store.load(requiredValue(arguments_, "--run-id")) : null;
  if (recovered !== null) validatePersistedStateSemantics(recovered, repositoryRoot);
  const processAdapter = new BoundedProcess();
  const gitExecutable = requiredValue(arguments_, "--git-executable");
  const powershellExecutable = requiredValue(arguments_, "--powershell-executable");
  await new GitBaselineVerifier(processAdapter, gitExecutable, safeEnvironment()).verify(
    repositoryRoot,
    plan?.baseline ?? (recovered === null ? "" : persistedCoordinatorHead(recovered)),
  );
  const worktrees = new GitWorktreeManager(repositoryRoot, managedWorktreeRoot, processAdapter, gitExecutable, safeEnvironment());
  const inspector = new GitCandidateInspector(
    processAdapter,
    gitExecutable,
    safeEnvironment(),
    await loadTrustedLanguagePolicy(repositoryRoot),
  );
  const quality = new RepositoryQualityGate(processAdapter, safeEnvironment(), powershellExecutable);
  const integration = new SequentialIntegrationPipeline(processAdapter, gitExecutable, safeEnvironment(), quality);
  let codexRunner: CodexRunner | null = null;
  let runner: AgentRunner;
  if (runnerName === "fake") {
    runner = new FakeAgentRunner(await fakeOutcomes(fixtureResultsPath ?? ""));
  } else {
    const model = valueAfter(arguments_, "--model");
    const permittedModels = (valueAfter(arguments_, "--permitted-models") ?? "")
      .split(",")
      .map((value) => value.trim())
      .filter((value) => value.length > 0);
    codexRunner = new CodexRunner({
      executionAuthorised: true,
      authorityReference: codexAuthorityReference,
      worktreeRoot: managedWorktreeRoot,
      environment: safeEnvironment(),
      model,
      permittedModels,
    });
    runner = codexRunner;
  }
  const coordinator = new Coordinator(
    runner,
    store,
    new FileResourceLocks(resolve(root, "locks"), repositoryRoot),
    new ConsoleEventSink(),
    worktrees,
    inspector,
    integration,
    quality,
    new FileThreadCheckpointStore(root, repositoryRoot),
    repositoryRoot,
  );
  const resumeRunId = resume ? requiredValue(arguments_, "--run-id") : null;
  const reconcileAbsentLocks = resume && hasFlag(arguments_, "--reconcile-absent-locks");
  if (reconcileAbsentLocks && (valueAfter(arguments_, "--confirm-run-id") !== resumeRunId || valueAfter(arguments_, "--confirm-runner-quiescence") !== resumeRunId)) {
    throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", "Absent-owner lock reconciliation requires exact run and runner-quiescence confirmations.");
  }
  try {
    const state = resume
      ? await coordinator.resume(resumeRunId ?? "", Number(valueAfter(arguments_, "--max-concurrency") ?? "3"), undefined, reconcileAbsentLocks)
      : await coordinator.start(plan ?? (() => { throw new OrchestratorStop("TEST_BASELINE_BROKEN", "The execution plan was not loaded."); })());
    process.stdout.write(canonicalJson(stateSummary(state)));
  } finally {
    await codexRunner?.close();
  }
}

async function validateCommand(arguments_: readonly string[]): Promise<void> {
  const root = stateRoot(arguments_);
  const state = await new FileStateStore(root, repositoryRoot).load(requiredValue(arguments_, "--run-id"));
  validatePersistedStateSemantics(state, repositoryRoot);
  const lockManager = new FileResourceLocks(resolve(root, "locks"), repositoryRoot);
  const locks = await lockManager.inspect();
  if (state.heldLocks.length > 0 || locks.length > 0) {
    throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", "Validation found unreconciled resource locks.");
  }
  const qualityRequested = hasFlag(arguments_, "--quality-gate");
  if (state.humanGateReached && !qualityRequested) {
    throw new OrchestratorStop("HUMAN_GATE_REQUIRED", "A Human Gate package requires live Git and canonical offline quality validation before it can request a decision.");
  }
  if (state.humanGateReached) {
    await new GitBaselineVerifier(new BoundedProcess(), requiredValue(arguments_, "--git-executable"), safeEnvironment()).verify(
      repositoryRoot,
      persistedCoordinatorHead(state),
    );
  }
  let qualityGate = null;
  if (qualityRequested) {
    qualityGate = await new RepositoryQualityGate(new BoundedProcess(), safeEnvironment(), requiredValue(arguments_, "--powershell-executable")).run(repositoryRoot);
    if (qualityGate.result !== "PASS") {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "The canonical repository quality gate failed.");
    }
  }
  process.stdout.write(canonicalJson({ state: stateSummary(state, state.humanGateReached && qualityGate?.result === "PASS"), locks: await lockManager.inspectRecords(), qualityGate: qualityGate === null ? null : { ...qualityGate, relevantOutput: [] } }));
}

async function cleanupCommand(arguments_: readonly string[]): Promise<void> {
  const root = stateRoot(arguments_);
  const runId = requiredValue(arguments_, "--run-id");
  const runRoot = resolveRunRoot(root, runId);
  const state = await new FileStateStore(root, repositoryRoot).load(runId);
  validatePersistedStateSemantics(state, repositoryRoot);
  const lockManager = new FileResourceLocks(resolve(root, "locks"), repositoryRoot);
  const lockReport = await lockManager.inspectRecords();
  const terminal = state.tasks.every((task) => ["PASS", "IMPLEMENTED", "FAIL", "HUMAN_REVIEW_REQUIRED", "CANCELLED", "BLOCKED"].includes(task.status));
  const managed = state.tasks.filter((task) => task.taskKind === "IMPLEMENTATION" && task.worktree !== null)
    .map((task) => ({ taskId: task.taskId, worktree: task.worktree, branch: task.branch, baseline: state.baseline, head: task.candidate?.commitId ?? state.baseline, status: task.status }));
  const confirmation = valueAfter(arguments_, "--confirm-run-id");
  if (confirmation === null) {
    const files = (await readdir(runRoot)).sort();
    process.stdout.write(canonicalJson({ runId, action: "REPORT_ONLY", terminal, files, locks: lockReport, managedWorktrees: managed }));
    return;
  }
  if (confirmation !== runId || !terminal || state.heldLocks.length > 0 || lockReport.length > 0) {
    throw new OrchestratorStop("DESTRUCTIVE_OPERATION", "Cleanup requires exact run confirmation, terminal tasks and no locks.");
  }
  if (managed.length > 0) {
    const gitExecutable = requiredValue(arguments_, "--git-executable");
    const worktrees = new GitWorktreeManager(repositoryRoot, managedWorktreeRoot, new BoundedProcess(), gitExecutable, safeEnvironment());
    for (const entry of managed) {
      if (entry.worktree !== null && entry.branch !== null) {
        await worktrees.removeManaged(entry.taskId, entry.worktree, { branch: entry.branch, baseline: entry.baseline, head: entry.head });
      }
    }
  }
  await assertNoExistingReparseBoundary(repositoryRoot, runRoot);
  const tombstone = `${runRoot}.cleanup`;
  await stat(tombstone).then(() => { throw new OrchestratorStop("DESTRUCTIVE_OPERATION", "The cleanup tombstone already exists."); }).catch((error) => {
    if ((error as NodeJS.ErrnoException).code !== "ENOENT" && !(error instanceof OrchestratorStop)) throw error;
    if (error instanceof OrchestratorStop) throw error;
  });
  await rename(runRoot, tombstone);
  await assertNoExistingReparseBoundary(repositoryRoot, tombstone);
  process.stdout.write(canonicalJson({ runId, action: "QUARANTINED", tombstone: `${runId}.cleanup`, managedWorktrees: managed.map((entry) => entry.taskId) }));
}

async function main(processArguments: readonly string[]): Promise<void> {
  const [command, ...arguments_] = processArguments;
  if (command === "plan" || (command === "run" && hasFlag(arguments_, "--dry-run"))) {
    const plan = parseProjectPlan(await readJson(requiredValue(arguments_, "--plan")));
    process.stdout.write(canonicalJson(createDryRunPlan(plan, repositoryRoot)));
  } else if (command === "run") {
    await runCommand(arguments_, false);
  } else if (command === "resume") {
    await runCommand(arguments_, true);
  } else if (command === "status") {
    const state = await new FileStateStore(stateRoot(arguments_), repositoryRoot).load(requiredValue(arguments_, "--run-id"));
    validatePersistedStateSemantics(state, repositoryRoot);
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
  assertCliArgumentsContainNoSecretMaterial(process.argv, "CLI arguments");
  const processArguments = process.argv.slice(2);
  await main(processArguments);
} catch (error) {
  const code = error instanceof OrchestratorStop ? error.code : "TEST_BASELINE_BROKEN";
  process.stderr.write(canonicalJson({ status: "BLOCKED", stopCondition: code, message: errorMessage(error) }));
  process.exitCode = 1;
}
