# ADR-0017: Codex App Server Pre-Turn Checkpoint Runner

## Status

`proposed`

## Date

2026-08-15

## Acceptance record

- No acceptance or rejection has been recorded.
- This proposal remains `HUMAN_DECISION_REQUIRED`.
- Preparing this ADR does not authorise implementation, a real Codex turn,
  account usage, network access, a secret, billing or a lifecycle transition.

## Owners

- Product owner: RAG-Challenge owner
- Architecture owner: RAG-Challenge
- Technical owner: development tooling

## Preparation authority and baseline

- Authority: the owner's request to make Stage 0, Stage 1 and Stage 2
  operational while excluding unrelated work and `OPENAI_API_KEY` usage.
- Authority identifier: `AUTH-MULTI-AGENT-REAL-RUNNER-PREP-001`
- Branch: `codex/main-stage-integration`
- Commit: `0854d46717214321783423370601ba0a0d045e7e`
- Prompt corpus before preparation: `4.13.1`
- Lifecycle position: `STATE-07 TESTING_HOMOLOGATION`; unchanged
- Runtime preflight: `NOT_APPLICABLE` for documentary preparation
- Protected OpenAPI v1 SHA-256:
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`
- Protected OpenAPI v2 SHA-256:
  `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733`

## Purpose and authority boundary

This proposal is a narrow successor to ADR-0016. It preserves the accepted
deterministic coordinator, `AgentRunner` port, task/result contracts,
worktree isolation, resource locks, sequential integration, quality gate and
external Human Gate. It changes only the programmatic Codex transport needed
to create a durable thread identity before starting its first turn.

The proposal affects development tooling only. It does not alter the
RAG-Challenge product runtime, product providers, corpus, index, OpenAPI,
deployment or lifecycle.

Acceptance would select the architecture below. Implementation and one
bounded real validation remain separate execution authorities. No credential
may be supplied through repository files, CLI arguments, task envelopes,
persisted state or logs.

## Context

ADR-0016 selected `@openai/codex-sdk` `0.147.0` behind `CodexRunner`. Stage 2
implemented and contract-tested that boundary, but the stable SDK exposes a
new thread ID only after the first turn starts. The orchestrator requires a
durable thread checkpoint before the turn so interruption never produces an
unrecoverable unknown execution. Consequently, `NEW_REAL_START` currently
stops with `ARCHITECTURE_CHANGE_REQUIRED`.

The stable npm release was rechecked on 2026-08-15 and remains `0.147.0`.
Current official Codex SDK documentation retains the combined start-and-run
surface. Current official Codex App Server documentation provides two
separate stable protocol requests:

1. `thread/start`, which returns `thread.id`; and
2. `turn/start`, which begins agent generation for that recorded thread.

That separation satisfies the existing pre-turn checkpoint requirement
without parsing an interactive terminal or relaxing recovery guarantees.

Codex authentication supports account state provisioned outside the
repository. Local authentication files are present, but their current online
validity has not been exercised by this proposal. This proposal deliberately does not add, read or pass
`OPENAI_API_KEY`. The presence of any product-provider credential does not
authorise its use by development tooling.

## Decision drivers

- Preserve the pre-turn durable checkpoint and fail-closed recovery model.
- Use an officially documented, structured Codex protocol.
- Keep the deterministic coordinator independent of Codex protocol types.
- Reuse the accepted Node 24/npm 11 development-tooling boundary.
- Use only the existing user-scoped Codex authentication state, after a
  sanitised validity check, without copying credentials into the repository
  or task workers.
- Keep agent tool network access disabled independently from the Codex service
  connection required to execute the authenticated turn.
- Maintain complete credential-free fake and contract tests in CI.

## Proposed decision

### Runner transport

Retain the `CodexRunner` role behind the existing `AgentRunner` port, but
replace its direct `@openai/codex-sdk` turn transport with a project-owned,
typed client for the officially documented Codex App Server JSONL protocol.

The client launches the exact locked `@openai/codex` executable directly with
`app-server --listen stdio://`. It uses `spawn` without a shell, bounded input
and output, explicit timeouts, cancellation and deterministic process cleanup.
Interactive terminal output, scraping and visual automation remain prohibited.

### Connection and concurrency

One App Server process belongs to one orchestrator run. The client performs
one `initialize` handshake and multiplexes bounded requests by numeric request
ID. Each agent task receives a distinct Codex thread. Writable task
concurrency remains controlled by the deterministic scheduler, disjoint
worktrees and resource locks; the App Server never decides concurrency.

Unexpected process termination fails every in-flight task. Restart and resume
require persisted thread checkpoints and the existing recovery validation.

### Pre-turn checkpoint protocol

For a new task, the runner executes exactly:

```text
initialize
  -> thread/start
  -> validate returned thread.id
  -> persist and read back the thread checkpoint
  -> turn/start
  -> collect structured events
  -> validate the final result
```

For a resumed task, it executes:

```text
initialize
  -> thread/resume with the persisted thread.id
  -> validate the returned identity
  -> revalidate and read back the checkpoint
  -> turn/start
```

No `turn/start` request may be emitted until checkpoint persistence and
readback succeed. A missing, malformed or inconsistent identity fails closed.

### Authentication and environment

The runner uses only the existing user-scoped local Codex authentication
state after verifying its validity through the supported Codex surface.
It does not accept an API key option or any credential value.

The child environment is deny-by-default and contains only operating-system
variables required to locate the executable, temporary directory and the
existing user-scoped Codex configuration. `USERPROFILE` is permitted solely
for this purpose on Windows. Environment values are never logged or persisted.

Before a real turn, the client performs a sanitised authentication-status
check through the documented App Server account surface. It records only
authenticated/not-authenticated disposition. Missing authentication returns
`SECRET_REQUIRED` without fallback or credential discovery.

### Execution surface

The task envelope continues to define:

- exact working directory and writable roots;
- `read-only` or `workspace-write` sandbox;
- approval policy `never`;
- agent tool network access disabled;
- no MCP server or skill unless separately authorised;
- allowed and forbidden paths;
- model allowlist when a model override is requested; and
- task-specific stop conditions and tests.

The runner rejects any App Server request for approval or user input. It never
widens the sandbox, enables web search, enables worker network access or
changes the model to recover from a failure.

### Structured result and evidence

`turn/start` supplies the existing project-owned output schema. The client
accepts only the expected thread, turn and item notifications, enforces size
and sequence bounds, and validates the final agent message through the
existing `parseAgentResult` boundary.

Provider payloads, prompts, reasoning, unrestricted command output and file
contents are not persisted. Existing sanitised attempt, checkpoint, evidence
digest and state rules remain unchanged.

### CLI activation

After separately authorised implementation, the CLI may add
`--runner codex`. Real execution additionally requires:

- a valid `--authority-reference` containing no secret;
- a plan whose task envelopes permit real Codex execution;
- a clean, exact Git baseline;
- a valid local Codex account session;
- a separately authorised model when an override is used; and
- the existing isolated worktree and resource checks.

`fake` remains the default and the only runner used by CI. No real execution
is inferred from installation, authentication or ADR acceptance.

## Required implementation and validation sequence

After acceptance, implementation must remain sequential and produce:

1. typed App Server request, response and notification validation;
2. handshake, authentication-status and process-lifecycle tests;
3. proof that checkpoint write and readback precede `turn/start`;
4. new-thread and persisted-thread resume contract tests;
5. rejection of approval, user-input, malformed identity, foreign thread,
   invalid output, oversized output, timeout and interrupted process;
6. CLI denial tests for missing authority, authentication, model allowlist,
   dirty baseline and invalid execution surface;
7. unchanged fake runner, dry run, recovery, integration and Human Gate tests;
8. the orchestrator coverage gate and canonical offline repository gate;
9. protected OpenAPI identity verification; and
10. one separately authorised, non-destructive real read-only validation using
    the existing Codex account session and no API key.

The real validation must use one task, one turn, network-disabled agent tools,
approval `never`, bounded output and no product/provider action. It is not a
product test, Human Gate or lifecycle evidence.

## Consequences

### Positive

- New real threads can be checkpointed before agent work starts.
- The existing recovery invariant remains intact.
- Stage 0 and Stage 1 governance continue to control Stage 2 execution.
- Authentication remains outside the repository and separate from product
  credentials.
- The deterministic core and fake CI suite remain unchanged.

### Costs and risks

- The project owns a small JSONL/JSON-RPC client and protocol validators.
- App Server is a deeper integration surface than the Codex SDK and remains
  version-sensitive.
- One long-lived child process requires explicit shutdown and failure fan-out.
- A real Codex turn uses the authenticated account and may consume applicable
  subscription or usage allowance; each run therefore remains separately
  authorised.
- Authentication status and Windows user-profile discovery require careful
  sanitisation and tests.

## Alternatives

### Relax checkpointing after the first SDK event

Rejected. The SDK starts the turn before the durable identity is available.
Persisting the first streamed `thread.started` event would leave a crash window
in which agent work may exist without a recoverable coordinator checkpoint.

### Use the SDK only for persisted-thread resume

Retained as the current fallback, but insufficient for an operational system
that must create new specialised-agent threads.

### Adopt an alpha Codex SDK

Rejected for this proposal. The stable `0.147.0` release is current, and an
alpha dependency would not remove the need for documented pre-turn identity
evidence and a separately accepted version decision.

### Use OpenAI Agents SDK or Codex through MCP

Not selected. The requirement is a narrower Codex development orchestrator,
and adding another agent loop or provider configuration would expand the
accepted boundary without solving a demonstrated additional need.

### Keep fake-only operation

Retained if this ADR is rejected. Stage 0 and Stage 1 remain effective and the
Stage 2 deterministic core remains validated, but new real Codex execution
continues to stop with `ARCHITECTURE_CHANGE_REQUIRED`.

## Decision request

The owner may accept this proposal only after reviewing its transport,
authentication, recovery, cost and validation boundaries. Acceptance uses:

```text
ADR-0017: ACEITAR.
```

Acceptance is not a Human Gate and does not itself authorise implementation or
a real turn. Those actions remain bounded by the owner's current Stage 0/1/2
activation request and the separate execution conditions recorded above.
