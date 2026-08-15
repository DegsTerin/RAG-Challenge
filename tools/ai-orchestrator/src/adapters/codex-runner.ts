// Purpose: Adapts Codex App Server behind a deny-by-default runner with pre-turn checkpointing, isolated cwd and structured output.
import { isAbsolute, relative, resolve, sep } from "node:path";
import { buildAgentPrompt } from "../application/agent-prompt.js";
import type { AgentRunner, AgentRunRequest, AgentRunResponse } from "../core/contracts.js";
import { OrchestratorStop } from "../core/errors.js";
import { agentResultOutputSchema, parseAgentResult } from "../core/validation.js";
import { parseSecureJson } from "../security/secure-json.js";
import { assertAuthorityReference, assertClosedEnvironment, assertNoSecretShapedText } from "../security/secret-policy.js";
import { createCodexAppServer, type CodexAppServer } from "./codex-app-server.js";

export interface CodexRunnerPolicy {
  readonly executionAuthorised: boolean;
  readonly authorityReference: string | null;
  readonly worktreeRoot: string;
  readonly environment: Readonly<Record<string, string>>;
  readonly model: string | null;
  readonly permittedModels?: readonly string[];
}

export type CodexAppServerFactory = (policy: CodexRunnerPolicy) => CodexAppServer;

const permittedEnvironmentNames = new Set(["PATH", "SystemRoot", "TEMP", "TMP", "USERPROFILE", "LOCALAPPDATA", "APPDATA"]);

function defaultFactory(policy: CodexRunnerPolicy): CodexAppServer {
  return createCodexAppServer({ environment: policy.environment });
}

function assertWorktree(root: string, worktree: string | null): string {
  if (!isAbsolute(root) || worktree === null || !isAbsolute(worktree)) {
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
  if (request.task.taskKind === "IMPLEMENTATION") return assertWorktree(root, request.task.worktree);
  if (!isAbsolute(request.task.executionSurface.cwd) || request.task.executionSurface.writableRoots.length > 0) {
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "A read-only Codex task requires an absolute non-writable cwd.", request.task.taskId);
  }
  return resolve(request.task.executionSurface.cwd);
}

export class CodexRunner implements AgentRunner {
  private client: CodexAppServer | null = null;

  public constructor(
    private readonly policy: CodexRunnerPolicy,
    private readonly factory: CodexAppServerFactory = defaultFactory,
  ) {}

  public async run(request: AgentRunRequest, signal?: AbortSignal): Promise<AgentRunResponse> {
    if (!this.policy.executionAuthorised || this.policy.authorityReference === null) {
      throw new OrchestratorStop(
        "HUMAN_DECISION_REQUIRED",
        "Real Codex execution is disabled until a separate bounded authority is recorded.",
        request.task.taskId,
      );
    }
    assertAuthorityReference(this.policy.authorityReference, "Codex authority reference");
    assertClosedEnvironment(this.policy.environment, permittedEnvironmentNames, "Codex environment");
    if (this.policy.model !== null && !(this.policy.permittedModels ?? []).includes(this.policy.model)) {
      throw new OrchestratorStop("HUMAN_DECISION_REQUIRED", "The requested Codex model is not present in the separately authorised model allowlist.", request.task.taskId);
    }
    const workingDirectory = assertExecutionSurface(request, this.policy.worktreeRoot);
    this.client ??= this.factory(this.policy);
    await this.client.assertChatGptSession();
    const configuration = {
      workingDirectory,
      sandbox: request.task.executionSurface.sandbox,
      model: this.policy.model,
    } as const;
    const threadId = request.resumeThreadId === null
      ? await this.client.startThread(configuration, request.task.taskId)
      : await this.client.resumeThread(request.resumeThreadId, configuration, request.task.taskId);
    await request.checkpointThread(threadId);
    const prompt = buildAgentPrompt(request);
    assertNoSecretShapedText(prompt, "Codex prompt");
    const finalResponse = await this.client.runTurn(
      threadId,
      prompt,
      { ...configuration, outputSchema: agentResultOutputSchema },
      request.task.taskId,
      signal,
    );
    let parsed: unknown;
    try {
      if (Buffer.byteLength(finalResponse, "utf8") > 262_144) {
        throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Codex returned an oversized structured result.", request.task.taskId);
      }
      assertNoSecretShapedText(finalResponse, "Codex structured result");
      parsed = parseSecureJson(finalResponse, "Codex structured result");
    } catch (error) {
      if (error instanceof OrchestratorStop && error.code === "SECRET_REQUIRED") throw error;
      throw new OrchestratorStop("TEST_BASELINE_BROKEN", "Codex returned invalid structured JSON.", request.task.taskId);
    }
    return { result: parseAgentResult(parsed), threadId };
  }

  public async close(): Promise<void> {
    await this.client?.close();
    this.client = null;
  }
}
