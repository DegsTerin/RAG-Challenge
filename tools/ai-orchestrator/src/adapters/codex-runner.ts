// Purpose: Adapts the verified Codex SDK behind a deny-by-default runner with isolated cwd, environment, sandbox and structured output.
import { Codex, type ThreadOptions, type TurnOptions } from "@openai/codex-sdk";
import { isAbsolute, relative, resolve, sep } from "node:path";
import { buildAgentPrompt } from "../application/agent-prompt.js";
import type { AgentRunner, AgentRunRequest, AgentRunResponse } from "../core/contracts.js";
import { OrchestratorStop } from "../core/errors.js";
import { agentResultOutputSchema, parseAgentResult } from "../core/validation.js";

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
    const workingDirectory = assertWorktree(this.policy.worktreeRoot, request.task.worktree);
    const client = this.factory(this.policy);
    const threadOptions: ThreadOptions = {
      workingDirectory,
      skipGitRepoCheck: false,
      sandboxMode: "workspace-write",
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
      parsed = JSON.parse(turn.finalResponse) as unknown;
    } catch {
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Codex returned invalid structured JSON.", request.task.taskId);
    }
    return { result: parseAgentResult(parsed), threadId: thread.id };
  }
}
