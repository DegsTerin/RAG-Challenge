// Purpose: Adapts the verified Codex SDK behind a deny-by-default runner with isolated cwd, environment, sandbox and structured output.
import { Codex, type ThreadOptions, type TurnOptions } from "@openai/codex-sdk";
import { isAbsolute, relative, resolve, sep } from "node:path";
import { buildAgentPrompt } from "../application/agent-prompt.js";
import type { AgentRunner, AgentRunRequest, AgentRunResponse } from "../core/contracts.js";
import { OrchestratorStop } from "../core/errors.js";
import { agentResultOutputSchema, parseAgentResult } from "../core/validation.js";
import { parseSecureJson } from "../security/secure-json.js";

interface CodexTurn {
  readonly finalResponse: string;
}

interface CodexThread {
  readonly id: string | null;
  run(input: string, options?: TurnOptions): Promise<CodexTurn>;
}

interface CodexClient {
  startThread(options?: ThreadOptions): CodexThread;
  resumeThread(id: string, options?: ThreadOptions): CodexThread;
}

export interface CodexRunnerPolicy {
  readonly executionAuthorised: boolean;
  readonly authorityReference: string | null;
  readonly worktreeRoot: string;
  readonly environment: Readonly<Record<string, string>>;
  readonly model: string | null;
  readonly permittedModels?: readonly string[];
}

export type CodexClientFactory = (policy: CodexRunnerPolicy) => CodexClient;

const permittedEnvironmentNames = new Set(["PATH", "SystemRoot", "TEMP", "TMP", "USERPROFILE", "LOCALAPPDATA", "APPDATA", "HOME"]);

function defaultFactory(policy: CodexRunnerPolicy): CodexClient {
  return new Codex({
    env: { ...policy.environment },
    config: {
      web_search: "disabled",
      sandbox_workspace_write: { network_access: false },
    },
  });
}

function assertWorktree(root: string, worktree: string | null): string {
  if (worktree === null || !isAbsolute(worktree)) {
    throw new OrchestratorStop("AMBIGUOUS_AUTHORITY", "Codex execution requires an absolute isolated worktree.");
  }
  const absoluteRoot = resolve(root);
  const absoluteWorktree = resolve(worktree);
  const relation = relative(absoluteRoot, absoluteWorktree);
  if (relation === "" || relation === ".." || relation.startsWith(`..${sep}`) || isAbsolute(relation)) {
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The Codex worktree is outside its authorised root.");
  }
  return absoluteWorktree;
}

function assertExecutionSurface(request: AgentRunRequest, root: string): string {
  if (["INTEGRATION", "QUALITY_GATE", "HUMAN_GATE"].includes(request.task.taskKind)) {
    throw new OrchestratorStop("AMBIGUOUS_AUTHORITY", "Deterministic and Human Gate tasks cannot be dispatched to Codex.", request.task.taskId);
  }
  const expectedSandbox = request.task.taskKind === "IMPLEMENTATION" ? "workspace-write" : "read-only";
  if (request.task.executionSurface.sandbox !== expectedSandbox || request.task.executionSurface.networkAccess ||
      request.task.executionSurface.approvalPolicy !== "never" || request.task.executionSurface.environmentPolicy !== "minimal" ||
      request.task.executionSurface.mcpServers.length > 0 || request.task.executionSurface.skills.length > 0) {
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The task execution surface exceeds the Codex runner policy.", request.task.taskId);
  }
  if (request.task.taskKind === "IMPLEMENTATION") {
    return assertWorktree(root, request.task.worktree);
  }
  if (!isAbsolute(request.task.executionSurface.cwd) || request.task.executionSurface.writableRoots.length > 0) {
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "A read-only Codex task requires an absolute non-writable cwd.", request.task.taskId);
  }
  return resolve(request.task.executionSurface.cwd);
}

export class CodexRunner implements AgentRunner {
  public constructor(
    private readonly policy: CodexRunnerPolicy,
    private readonly factory: CodexClientFactory = defaultFactory,
  ) {}

  public async run(request: AgentRunRequest, signal?: AbortSignal): Promise<AgentRunResponse> {
    if (!this.policy.executionAuthorised || this.policy.authorityReference === null) {
      throw new OrchestratorStop(
        "HUMAN_DECISION_REQUIRED",
        "Real Codex execution is disabled until a separate bounded authority is recorded.",
        request.task.taskId,
      );
    }
    if (Object.keys(this.policy.environment).some((name) => !permittedEnvironmentNames.has(name))) {
      throw new OrchestratorStop("SECRET_REQUIRED", "The explicit Codex environment contains a variable outside the allowlist.", request.task.taskId);
    }
    if (this.policy.model !== null && !(this.policy.permittedModels ?? []).includes(this.policy.model)) {
      throw new OrchestratorStop("HUMAN_DECISION_REQUIRED", "The requested Codex model is not present in the separately authorised model allowlist.", request.task.taskId);
    }
    const workingDirectory = assertExecutionSurface(request, this.policy.worktreeRoot);
    const client = this.factory(this.policy);
    const threadOptions: ThreadOptions = {
      workingDirectory,
      skipGitRepoCheck: false,
      sandboxMode: request.task.executionSurface.sandbox,
      approvalPolicy: "never",
      networkAccessEnabled: false,
      webSearchMode: "disabled",
      ...(this.policy.model === null ? {} : { model: this.policy.model }),
    };
    const thread = request.resumeThreadId === null
      ? client.startThread(threadOptions)
      : client.resumeThread(request.resumeThreadId, threadOptions);
    const turn = await thread.run(buildAgentPrompt(request), { outputSchema: agentResultOutputSchema, ...(signal === undefined ? {} : { signal }) });
    let parsed: unknown;
    try {
      if (Buffer.byteLength(turn.finalResponse, "utf8") > 262_144) {
        throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Codex returned an oversized structured result.", request.task.taskId);
      }
      parsed = parseSecureJson(turn.finalResponse, "Codex structured result");
    } catch {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Codex returned invalid structured JSON.", request.task.taskId);
    }
    return { result: parseAgentResult(parsed), threadId: thread.id };
  }
}
