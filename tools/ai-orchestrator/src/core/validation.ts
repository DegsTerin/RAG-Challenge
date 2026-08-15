// Purpose: Validates all untrusted plans and agent results against closed, bounded contracts before they affect orchestration state.
import {
  agentIds,
  contractStates,
  ownershipClasses,
  parallelismClasses,
  resultStatuses,
  stopCodes,
  taskStatuses,
  type AgentResult,
  type ProjectPlan,
  type TaskDefinition,
} from "./contracts.js";
import { OrchestratorStop } from "./errors.js";

const identifierPattern = /^[a-z0-9][a-z0-9-]{0,63}$/;
const gitObjectPattern = /^[0-9a-f]{40}$/;
const absoluteOrTraversalPattern = /^(?:[a-zA-Z]:|[/\\]|\.{1,2}(?:[/\\]|$)|\\\\|\\\?\\)|(?:^|[/\\])\.\.(?:[/\\]|$)|:/;

type JsonRecord = Record<string, unknown>;

function record(value: unknown, label: string): JsonRecord {
  if (value === null || typeof value !== "object" || Array.isArray(value)) {
    throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", `${label} must be an object.`);
  }
  return value as JsonRecord;
}

function exactKeys(value: JsonRecord, allowed: readonly string[], label: string): void {
  const unexpected = Object.keys(value).filter((key) => !allowed.includes(key));
  if (unexpected.length > 0) {
    throw new OrchestratorStop(
      "CONFLICTING_REQUIREMENTS",
      `${label} contains unsupported fields: ${unexpected.sort().join(", ")}.`,
    );
  }
}

function stringValue(value: unknown, label: string, maximum = 2048): string {
  if (typeof value !== "string" || value.length === 0 || value.length > maximum || /[\u0000-\u001f]/.test(value)) {
    throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", `${label} must be a bounded printable string.`);
  }
  return value;
}

function nullableString(value: unknown, label: string, maximum = 2048): string | null {
  return value === null ? null : stringValue(value, label, maximum);
}

function stringArray(value: unknown, label: string, maximumItems = 64, maximumLength = 2048): string[] {
  if (!Array.isArray(value) || value.length > maximumItems) {
    throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", `${label} must be a bounded array.`);
  }
  return value.map((item, index) => stringValue(item, `${label}[${index}]`, maximumLength));
}

function enumValue<T extends string>(value: unknown, values: readonly T[], label: string): T {
  if (typeof value !== "string" || !values.includes(value as T)) {
    throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", `${label} has an unsupported value.`);
  }
  return value as T;
}

function booleanValue(value: unknown, label: string): boolean {
  if (typeof value !== "boolean") {
    throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", `${label} must be a boolean.`);
  }
  return value;
}

function integerValue(value: unknown, label: string, minimum: number, maximum: number): number {
  if (!Number.isInteger(value) || (value as number) < minimum || (value as number) > maximum) {
    throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", `${label} must be an integer from ${minimum} to ${maximum}.`);
  }
  return value as number;
}

function isoInstant(value: unknown, label: string): string {
  const parsed = stringValue(value, label, 64);
  if (!/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{3})?Z$/.test(parsed) || Number.isNaN(Date.parse(parsed))) {
    throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", `${label} must be a UTC ISO 8601 instant.`);
  }
  return parsed;
}

function nullableInstant(value: unknown, label: string): string | null {
  return value === null ? null : isoInstant(value, label);
}

export function assertIdentifier(value: string, label: string): void {
  if (!identifierPattern.test(value)) {
    throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", `${label} is not a safe canonical identifier.`);
  }
}

export function assertRepositoryPath(value: string, label: string): void {
  if (value.length > 240 || value.includes("//") || value.includes("\\") || /[*?[\]]/.test(value) || absoluteOrTraversalPattern.test(value)) {
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", `${label} is not a safe repository-relative path.`);
  }
  for (const segment of value.split("/")) {
    if (segment.length === 0 || segment === "." || segment === ".." || /[. ]$/.test(segment)) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", `${label} contains an ambiguous path segment.`);
    }
  }
}

const authorityKeys = ["references", "grants", "negativeScope"] as const;
const contractKeys = ["contractId", "state", "owner"] as const;
const commandKeys = ["commandId", "exitCode", "durationMs", "result", "relevantOutput"] as const;
const resultKeys = [
  "status", "summary", "changedFiles", "commands", "tests", "evidence", "risks", "blockers",
  "requestedAuthority", "stopCondition",
] as const;
const taskKeys = [
  "taskId", "title", "objective", "authority", "owner", "status", "priority", "dependencies", "blockedBy",
  "allowedPaths", "forbiddenPaths", "ownership", "sharedResources", "requiredContracts", "acceptanceCriteria",
  "requiredTests", "stopConditions", "deliverables", "worktree", "branch", "parallelism",
  "requiresIndependentReview", "requiresSecurityReview", "humanGate", "maxAttempts", "createdAt", "startedAt",
  "finishedAt", "result", "evidence",
] as const;

function parseCommand(value: unknown, label: string) {
  const source = record(value, label);
  exactKeys(source, commandKeys, label);
  return {
    commandId: stringValue(source.commandId, `${label}.commandId`, 128),
    exitCode: integerValue(source.exitCode, `${label}.exitCode`, -2147483648, 2147483647),
    durationMs: integerValue(source.durationMs, `${label}.durationMs`, 0, 86_400_000),
    result: enumValue(source.result, ["PASS", "FAIL", "BLOCKED"] as const, `${label}.result`),
    relevantOutput: stringArray(source.relevantOutput, `${label}.relevantOutput`, 32, 512),
  } as const;
}

export function parseAgentResult(value: unknown): AgentResult {
  const source = record(value, "agent result");
  exactKeys(source, resultKeys, "agent result");
  const changedFiles = stringArray(source.changedFiles, "agent result.changedFiles", 256, 240);
  for (const [index, path] of changedFiles.entries()) {
    assertRepositoryPath(path, `agent result.changedFiles[${index}]`);
  }
  const stopCondition = source.stopCondition === null
    ? null
    : enumValue(source.stopCondition, stopCodes, "agent result.stopCondition");
  const status = enumValue(source.status, resultStatuses, "agent result.status");
  if (status === "BLOCKED" && stopCondition === null) {
    throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", "A blocked agent result must carry a stop condition.");
  }
  if (status !== "BLOCKED" && stopCondition !== null) {
    throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", "Only a blocked agent result may carry a stop condition.");
  }
  const commands = Array.isArray(source.commands) ? source.commands.map((item, index) => parseCommand(item, `commands[${index}]`)) : null;
  const tests = Array.isArray(source.tests) ? source.tests.map((item, index) => parseCommand(item, `tests[${index}]`)) : null;
  if (commands === null || tests === null || commands.length > 64 || tests.length > 64) {
    throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", "Agent command and test evidence must be bounded arrays.");
  }
  return {
    status,
    summary: stringValue(source.summary, "agent result.summary", 4096),
    changedFiles,
    commands,
    tests,
    evidence: stringArray(source.evidence, "agent result.evidence"),
    risks: stringArray(source.risks, "agent result.risks"),
    blockers: stringArray(source.blockers, "agent result.blockers"),
    requestedAuthority: stringArray(source.requestedAuthority, "agent result.requestedAuthority"),
    stopCondition,
  };
}

function parseTask(value: unknown, label: string): TaskDefinition {
  const source = record(value, label);
  exactKeys(source, taskKeys, label);
  const taskId = stringValue(source.taskId, `${label}.taskId`, 64);
  assertIdentifier(taskId, `${label}.taskId`);
  const authoritySource = record(source.authority, `${label}.authority`);
  exactKeys(authoritySource, authorityKeys, `${label}.authority`);
  const allowedPaths = stringArray(source.allowedPaths, `${label}.allowedPaths`, 64, 240);
  const forbiddenPaths = stringArray(source.forbiddenPaths, `${label}.forbiddenPaths`, 64, 240);
  for (const [index, path] of [...allowedPaths, ...forbiddenPaths].entries()) {
    assertRepositoryPath(path, `${label}.paths[${index}]`);
  }
  if (!Array.isArray(source.requiredContracts) || source.requiredContracts.length > 64) {
    throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", `${label}.requiredContracts must be a bounded array.`);
  }
  const requiredContracts = source.requiredContracts.map((item, index) => {
    const contract = record(item, `${label}.requiredContracts[${index}]`);
    exactKeys(contract, contractKeys, `${label}.requiredContracts[${index}]`);
    return {
      contractId: stringValue(contract.contractId, `${label}.requiredContracts[${index}].contractId`, 128),
      state: enumValue(contract.state, contractStates, `${label}.requiredContracts[${index}].state`),
      owner: stringValue(contract.owner, `${label}.requiredContracts[${index}].owner`, 128),
    };
  });
  return {
    taskId,
    title: stringValue(source.title, `${label}.title`, 256),
    objective: stringValue(source.objective, `${label}.objective`, 4096),
    authority: {
      references: stringArray(authoritySource.references, `${label}.authority.references`),
      grants: stringArray(authoritySource.grants, `${label}.authority.grants`),
      negativeScope: stringArray(authoritySource.negativeScope, `${label}.authority.negativeScope`),
    },
    owner: enumValue(source.owner, agentIds, `${label}.owner`),
    status: enumValue(source.status, taskStatuses, `${label}.status`),
    priority: integerValue(source.priority, `${label}.priority`, 0, 1000),
    dependencies: stringArray(source.dependencies, `${label}.dependencies`, 64, 64),
    blockedBy: stringArray(source.blockedBy, `${label}.blockedBy`, 64, 64),
    allowedPaths,
    forbiddenPaths,
    ownership: enumValue(source.ownership, ownershipClasses, `${label}.ownership`),
    sharedResources: stringArray(source.sharedResources, `${label}.sharedResources`, 64, 128),
    requiredContracts,
    acceptanceCriteria: stringArray(source.acceptanceCriteria, `${label}.acceptanceCriteria`),
    requiredTests: stringArray(source.requiredTests, `${label}.requiredTests`),
    stopConditions: Array.isArray(source.stopConditions)
      ? source.stopConditions.map((item, index) => enumValue(item, stopCodes, `${label}.stopConditions[${index}]`))
      : (() => { throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", `${label}.stopConditions must be an array.`); })(),
    deliverables: stringArray(source.deliverables, `${label}.deliverables`),
    worktree: nullableString(source.worktree, `${label}.worktree`, 240),
    branch: nullableString(source.branch, `${label}.branch`, 240),
    parallelism: enumValue(source.parallelism, parallelismClasses, `${label}.parallelism`),
    requiresIndependentReview: booleanValue(source.requiresIndependentReview, `${label}.requiresIndependentReview`),
    requiresSecurityReview: booleanValue(source.requiresSecurityReview, `${label}.requiresSecurityReview`),
    humanGate: booleanValue(source.humanGate, `${label}.humanGate`),
    maxAttempts: integerValue(source.maxAttempts, `${label}.maxAttempts`, 1, 3),
    createdAt: isoInstant(source.createdAt, `${label}.createdAt`),
    startedAt: nullableInstant(source.startedAt, `${label}.startedAt`),
    finishedAt: nullableInstant(source.finishedAt, `${label}.finishedAt`),
    result: source.result === null ? null : parseAgentResult(source.result),
    evidence: stringArray(source.evidence, `${label}.evidence`),
  };
}

export function parseProjectPlan(value: unknown): ProjectPlan {
  const source = record(value, "project plan");
  exactKeys(source, ["schemaVersion", "project", "baseline", "maxConcurrency", "tasks"], "project plan");
  if (source.schemaVersion !== 1 || source.project !== "RAG-Challenge") {
    throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", "The plan schema or project identity is unsupported.");
  }
  const baseline = stringValue(source.baseline, "project plan.baseline", 40);
  if (!gitObjectPattern.test(baseline)) {
    throw new OrchestratorStop("UNEXPECTED_DIRTY_TREE", "The plan baseline must be a full Git object identifier.");
  }
  if (!Array.isArray(source.tasks) || source.tasks.length === 0 || source.tasks.length > 256) {
    throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", "The plan must contain between 1 and 256 tasks.");
  }
  const tasks = source.tasks.map((task, index) => parseTask(task, `tasks[${index}]`));
  const ids = new Set<string>();
  for (const task of tasks) {
    if (ids.has(task.taskId)) {
      throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", `Task '${task.taskId}' is duplicated.`);
    }
    ids.add(task.taskId);
  }
  return {
    schemaVersion: 1,
    project: "RAG-Challenge",
    baseline,
    maxConcurrency: integerValue(source.maxConcurrency, "project plan.maxConcurrency", 1, 3),
    tasks,
  };
}

export const agentResultOutputSchema = {
  type: "object",
  additionalProperties: false,
  required: [...resultKeys],
  properties: {
    status: { type: "string", enum: [...resultStatuses] },
    summary: { type: "string", minLength: 1, maxLength: 4096 },
    changedFiles: { type: "array", maxItems: 256, items: { type: "string", minLength: 1, maxLength: 240 } },
    commands: {
      type: "array", maxItems: 64, items: {
        type: "object", additionalProperties: false,
        required: [...commandKeys],
        properties: {
          commandId: { type: "string", minLength: 1, maxLength: 128 },
          exitCode: { type: "integer", minimum: -2147483648, maximum: 2147483647 },
          durationMs: { type: "integer", minimum: 0, maximum: 86400000 },
          result: { type: "string", enum: ["PASS", "FAIL", "BLOCKED"] },
          relevantOutput: { type: "array", maxItems: 32, items: { type: "string", maxLength: 512 } },
        },
      },
    },
    tests: {
      type: "array", maxItems: 64, items: {
        type: "object", additionalProperties: false,
        required: [...commandKeys],
        properties: {
          commandId: { type: "string", minLength: 1, maxLength: 128 },
          exitCode: { type: "integer", minimum: -2147483648, maximum: 2147483647 },
          durationMs: { type: "integer", minimum: 0, maximum: 86400000 },
          result: { type: "string", enum: ["PASS", "FAIL", "BLOCKED"] },
          relevantOutput: { type: "array", maxItems: 32, items: { type: "string", maxLength: 512 } },
        },
      },
    },
    evidence: { type: "array", maxItems: 64, items: { type: "string", maxLength: 2048 } },
    risks: { type: "array", maxItems: 64, items: { type: "string", maxLength: 2048 } },
    blockers: { type: "array", maxItems: 64, items: { type: "string", maxLength: 2048 } },
    requestedAuthority: { type: "array", maxItems: 64, items: { type: "string", maxLength: 2048 } },
    stopCondition: { anyOf: [{ type: "null" }, { type: "string", enum: [...stopCodes] }] },
  },
} as const;
