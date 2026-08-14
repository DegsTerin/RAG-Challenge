# ADR-0016: Deterministic Development Orchestrator and Codex Runner Boundary

## Status

`accepted`

## Date

2026-08-14

## Acceptance record

- Accepted explicitly by the RAG-Challenge owner on 2026-08-14 through the
  exact phrase `ADR-0016: ACEITAR.`
- Decision baseline:
  `codex/stage1-multi-agent-readiness@355bd6cd731528bcdb8fccfe71ee93b70acb1d1e`
- The owner independently selected historical quarantine for the retained
  RB-2/RB-3 freezes. That disposition does not make RB-2 valid, authorise
  RB-4 or grant successor materialisation authority.
- Acceptance selects only the architecture recorded by this ADR. It is not a
  Human Gate and grants no secret, provider, network, billing, production,
  push, merge, release or lifecycle authority.

## Owners

- Product owner: RAG-Challenge owner
- Architecture owner: RAG-Challenge
- Technical owner: development tooling

## Preparation authority and baseline

- Preparation authority: the owner-provided Stage 0, Stage 1 and Stage 2
  evolution plan
- Branch: `codex/stage1-multi-agent-readiness`
- Commit: `9f309e1b6a21a33cbd24b4b6498e840dd26585c9`
- Prompt corpus before this proposal: `4.10.42`
- Lifecycle position: `STATE-07 TESTING_HOMOLOGATION`; unchanged by this
  proposal
- Runtime preflight: `NOT_APPLICABLE` for proposal preparation and static
  governance validation
- Protected OpenAPI v1 SHA-256:
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`
- Protected OpenAPI v2 SHA-256:
  `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733`

## Purpose and authority boundary

This decision defines the architecture boundary for a development-only
multi-agent orchestrator. It does not add a product capability, change the
RAG runtime, select a product AI provider or advance lifecycle.

The decision exists because its implementation introduces a new
tooling stack, task and result contracts, local orchestration persistence,
worktree control and security boundaries. Repository governance requires an
accepted ADR before those boundaries are materialised.

Acceptance selects the architecture below only. It does not install a
package, implement the tool, run Codex, grant a secret, permit network or paid
usage, accept a Human Gate, authorise production, push, merge to `main` or
advance `STATE-07`.

## Context

The repository already has:

- a governed .NET 10 modular-monolith product;
- a Node 24/npm 11 Dashboard boundary;
- project-scoped multi-agent roles under `.codex/agents/`;
- explicit task envelopes, ownership classes, stop conditions and isolation
  rules in repository governance; and
- a canonical sequential quality gate in `eng/ci.ps1`.

The orchestrator must execute those rules rather than create a second source
of governance. Model output is untrusted task evidence. It cannot decide
authority, dependency satisfaction, ownership, integration, gate disposition
or lifecycle.

The current official [Codex SDK documentation](https://learn.chatgpt.com/docs/codex-sdk)
describes a TypeScript library for starting, continuing and resuming Codex
threads. The official [subagent documentation](https://learn.chatgpt.com/docs/agent-configuration/subagents)
defines project-scoped custom agents and their sandboxed configuration. Both
surfaces remain version-sensitive and must be reverified at implementation
time.

## Decision drivers

- Keep scheduling, dependencies, locks, transitions and gates deterministic.
- Use an officially supported Codex programmatic boundary instead of parsing
  an interactive terminal.
- Reuse the repository's governed Node 24 toolchain rather than introduce a
  third language runtime.
- Keep Codex-specific code outside orchestration policy and product layers.
- Permit complete local tests with no authentication, network or model call.
- Recover conservatively after interruption without claiming false success.
- Preserve one branch, worktree, ownership scope and mutable-resource set per
  writable lane.
- Keep Human Gates and architectural decisions outside the tool.

## Decision

### Tool boundary

Create a standalone TypeScript tool under `tools/ai-orchestrator/`.

- It is development tooling, not a RAG-Challenge runtime module.
- It is not referenced by Domain, Application, Infrastructure, Server or
  Dashboard.
- It is not added to `RAG-Challenge.sln`.
- It owns a separate `package.json` and `package-lock.json`.
- It uses the existing Node range `>=24.18.0 <25` and npm range
  `>=11.16.0 <12` unless a later accepted decision changes them.
- Exact dependency versions are selected and locked only after Stage 2
  revalidates the current official interface and package metadata.

### Deterministic coordinator

The core is a deterministic state machine. It owns:

- task-envelope validation;
- dependency-graph validation and readiness;
- ownership and mutable-resource conflict detection;
- bounded concurrency decisions;
- task-state transitions;
- result-schema validation;
- sequential integration eligibility;
- quality-gate dispatch; and
- generation of a Human Gate package followed by a hard stop.

No language model or runner may mutate those rules or mark its own output as
accepted evidence.

### Runner boundary

`AgentRunner` is the sole boundary between deterministic orchestration and an
agent execution mechanism.

Initial implementations are limited to:

- `FakeAgentRunner`: deterministic, local, credential-free and used by the
  complete automated suite; and
- `CodexRunner`: a thin adapter over an exact locked version of
  `@openai/codex-sdk`, enabled only when supported authentication and explicit
  run authority are present.

`CodexRunner` starts or resumes an opaque thread, supplies the validated task
envelope and requires a project-owned structured result. Invalid, incomplete
or unparseable output is failure evidence, never success.

The state machine must not import Codex SDK types. Future runners require
separate authority and a successor ADR when they change this boundary.

### Agents SDK disposition

The first implementation does not add OpenAI Agents SDK. Direct Codex SDK use
is the smaller boundary for coding-focused threads and avoids a second agent
loop, external tracing behaviour and additional provider configuration.

This is an explicit adaptation of the conceptual Stage 2 diagram, not an
accidental omission. If a later requirement needs a broader agent workflow,
an ADR successor may evaluate Agents SDK with Codex exposed through a
documented MCP boundary. Experimental Codex extensions are not an automatic
fallback.

### Task and result contracts

The orchestrator consumes the task envelope owned by Governance, including
`TASK_ID`, objective, authority, owner, baseline, allowed and forbidden paths,
dependencies, shared resources, acceptance criteria, required tests, stop
conditions and deliverables.

Every result is schema-versioned and distinguishes:

- completed candidate work;
- blocked work with a canonical stop code;
- failed validation;
- cancelled or timed-out execution; and
- evidence awaiting coordinator review.

A worker result cannot accept its own diff, integrate a branch, approve an ADR
or Human Gate, change lifecycle or waive a failed check.

### Worktrees, branches and integration

- Each writable lane uses one exclusive `codex/<task-slug>` branch and one
  exclusive worktree outside the coordinator worktree.
- Local absolute worktree paths are configuration, never tracked project
  data.
- Existing or foreign worktrees and branches are read-only inventory unless
  the run created them and retained an ownership marker.
- The coordinator is the only integration owner and integrates one candidate
  at a time.
- Workers never merge or rebase the coordinator branch or `main` and never
  push.
- A conflict, stale baseline or unexpected dirty tree stops integration.
- Cleanup is explicit, validates the exact task-owned path and marker, and
  never uses broad or inferred recursive targets.

### Resource locks

Locks are scoped to a run, task, lane and concrete mutable resource. They are
used for worktrees, ports, stores, output roots, caches or external resources
only where isolation by namespace is insufficient.

A stale lock is not stolen automatically. Resume or cleanup first validates
the baseline, owner marker, process or listener, path and external state. Any
ambiguity returns `SHARED_RESOURCE_COLLISION` or
`UNEXPECTED_DIRTY_TREE`.

### Local persistence and recovery

Runtime state is stored only below the ignored root:

```text
artifacts-local/ai-orchestrator/<run-id>/
```

Persisted data is limited to:

- schema version and run/task/correlation IDs;
- Git baseline and validated dependency graph;
- task transitions and sanitised outcomes;
- ownership/resource locks;
- branch/worktree mappings;
- opaque thread IDs; and
- evidence references and digests.

Snapshots use a temporary file, flush and atomic replacement. An append-only
event journal supports recovery and is verified against the latest snapshot.
A partial write, missing event or checksum mismatch never implies success.

Resume revalidates HEAD, worktree status, branches, paths, locks, task
dependencies and evidence before dispatch. Replaying a completed transition
is idempotent; repeating agent execution requires a policy-authorised retry
and a new attempt identity.

The store never contains secrets, tokens, environment-variable values,
connection strings, complete prompts, file contents, unrestricted stdout or
provider payloads. It does not duplicate `Current-State.md` as project
authority.

### Authentication, network and cost

- `FakeAgentRunner` and `--dry-run` are the default validation modes.
- `CodexRunner` remains disabled until supported authentication already
  provisioned outside the repository and exact execution authority are both
  present.
- No credential is accepted in CLI arguments, task envelopes, tracked
  configuration or persisted state.
- Missing authentication returns `SECRET_REQUIRED`; there is no provider,
  model or network fallback.
- CI never invokes a real agent or external tracing.
- Codex execution is a development channel and does not change the product's
  OpenAI provider/model decisions.

### Human control

Human Gates, risk acceptance, ADR acceptance, lifecycle transitions,
adjudication, provider/billing changes, production, release and destructive
operations remain external. The orchestrator may assemble sanitised evidence
and an exact decision request, then must stop at `HUMAN_DECISION_REQUIRED`.
Only a lifecycle Human Gate for one named state uses `HUMAN_GATE_REQUIRED`.

## Required implementation evidence

Before operational use, Stage 2 must prove at least:

1. schema validation for task and result envelopes;
2. deterministic dependency ordering and bounded concurrency;
3. rejection of path, ownership and mutable-resource overlap;
4. sequential integration and quality-gate dispatch;
5. `FakeAgentRunner` success, failure, timeout, cancellation, invalid output
   and stop-condition cases;
6. `CodexRunner` contract tests without a real call;
7. dry-run with no branch, worktree, lock, process or agent execution;
8. atomic persistence, interrupted write, resume and idempotency;
9. disposable-repository worktree tests with task-owned cleanup only;
10. hard stop at Human Gate and architecture boundaries;
11. security review of paths, subprocesses, logs, secrets and supply chain;
12. the repository's canonical quality gate without reduced thresholds; and
13. unchanged protected OpenAPI identities and no product behaviour change.

If the exact Codex SDK version cannot provide a documented start/resume
surface, isolated working directory, compatible sandbox configuration and a
result that can be validated, implementation stops with
`ARCHITECTURE_CHANGE_REQUIRED`. A terminal-output wrapper is not a fallback.

## Consequences

### Positive

- Deterministic governance remains testable without an AI runtime.
- Codex integration is replaceable and narrowly contained.
- Existing Node operational knowledge and version policy are reused.
- Recovery and worktree ownership fail closed.
- Product code and provider decisions remain isolated.

### Costs and risks

- The repository gains a second Node package and lockfile.
- Codex SDK behaviour and authentication remain version-sensitive.
- Local state and worktree cleanup require careful Windows path handling.
- Direct Codex SDK use differs from the conceptual Agents SDK layer in the
  Stage 2 prompt; the owner explicitly accepted this adaptation.
- Full online behaviour cannot be proven by the credential-free CI suite.

## Alternatives

### OpenAI Agents SDK for TypeScript plus a Codex extension

Not selected. It adds a second orchestration loop and an experimental surface
without a demonstrated requirement. It may be reconsidered only through a
successor decision with explicit observability, authentication and cost
boundaries.

### Python Agents SDK with Codex through MCP

Not selected. It is a documented broader-workflow shape but introduces Python,
virtual-environment and lockfile management, a second process boundary and
additional secret/tracing considerations not otherwise present in the
repository.

### Ad hoc CLI or terminal parsing

Rejected. Interactive output is not a stable typed contract and would make
resume, error classification and security controls fragile.

### Manual coordination only

Retained as the fallback. If this ADR is rejected or the supported SDK
surface is insufficient, the repository continues using the Stage 1
governance and project-scoped agents without an automated orchestrator.

## Decision record

The owner accepted this decision through the exact phrase:

```text
ADR-0016: ACEITAR.
```

Acceptance is not a Human Gate and does not by itself authorise Stage 2
implementation. The owner's original Stage 2 implementation request remains
the execution authority only if the Stage 1 readiness re-evaluation passes on
the resulting baseline. The independent RB-2 authority defect is disposed by
historical quarantine, not by treating the retained package as valid.
