# Deterministic AI development orchestrator

## Purpose and boundary

This standalone TypeScript/Node 24 tool coordinates governed development work;
it is not part of the RAG-Challenge product runtime or solution. It implements
ADR-0016 behind a deterministic core and an `AgentRunner` port. The product
projects, product providers, corpus, database, OpenAPI contracts and lifecycle
remain unchanged.

The current package locks `@openai/codex-sdk` and `@openai/codex` at `0.147.0`,
TypeScript at `5.7.3` and `@types/node` at `24.13.3`. Installation and CI use
`--ignore-scripts --no-audit --no-fund`; offline restore is the preferred mode
after the initial authorised acquisition.

## Architecture

The dependency direction is:

```text
CLI and adapters
        |
        v
application coordinator and integration pipeline
        |
        v
deterministic contracts, graph, scheduler and state machine
        ^
        |
AgentRunner, StateStore, ResourceLocks, ProcessExecutor and WorktreeManager
```

The core owns task transitions, dependency and cycle detection, concurrency
classification, path/resource conflicts, retry classification and closed
structured-result validation. Adapters cannot grant authority or promote task
state. `FakeAgentRunner` supplies deterministic local and test execution.
`CodexRunner` is a thin direct adapter to the verified SDK surface.

The SDK adapter maps both `startThread` and `resumeThread`, an isolated absolute
`workingDirectory`, `sandboxMode: "workspace-write"`,
`approvalPolicy: "never"`, disabled network and web search, a closed output
schema and a small environment allowlist. It refuses construction of the SDK
client until a separate execution authority is supplied. No terminal scraping
or Agents SDK dependency is used.

## Agents and task contracts

Only the six Stage 1 roles are accepted:

- `governance_guard`;
- `code_mapper`;
- `architect`;
- `implementation_worker`;
- `independent_reviewer`;
- `security_reviewer`.

Plans and results are closed contracts. Their operator-facing JSON Schemas are
in [`schemas/`](schemas/); runtime validation remains the authoritative
acceptance boundary. A write task requires one `codex/` branch, one distinct
worktree, an explicit ownership class and named mutable resources. A task that
requires independent or security review must have the corresponding dependent
review task in the plan.

`SAFE_PARALLEL`, `CONTRACT_FROZEN_PARALLEL`, `SINGLE_OWNER` and
`SEQUENTIAL_ONLY` are scheduled with a global maximum of three. Priority is
descending and `taskId` is the deterministic tie-breaker. Dependencies,
allowed paths, worktrees, branches, mutable resources and contract owners are
checked before a wave is assigned.

## Operation

Install and validate locally:

```powershell
Set-Location .\tools\ai-orchestrator
npm ci --offline --ignore-scripts --no-audit --no-fund
npm run check
```

Build before using the CLI:

```powershell
npm run build
```

The supported commands are:

```powershell
node .\dist\src\cli.js plan --plan <plan.json>
node .\dist\src\cli.js run --dry-run --plan <plan.json>
node .\dist\src\cli.js run --plan <plan.json> --runner fake --fixture-results <results.json>
node .\dist\src\cli.js status --run-id <run-id>
node .\dist\src\cli.js resume --run-id <run-id> --runner fake --fixture-results <results.json>
node .\dist\src\cli.js validate --run-id <run-id>
node .\dist\src\cli.js validate --run-id <run-id> --quality-gate
node .\dist\src\cli.js cleanup --run-id <run-id>
```

`plan` and `run --dry-run` do not create state, worktrees, branches, locks or
agent executions. The preview lists tasks, dependencies, agents, execution
waves, worktrees, branches, resources, locks, quality gates, the external Human
Gate and candidate conflicts. The example in
[`examples/controlled-plan.json`](examples/controlled-plan.json) is for preview
only; its baseline must be replaced by the exact clean Git HEAD before a run.

Non-dry-run CLI execution currently permits only `--runner fake`. It verifies
an exact clean Git HEAD on a named `codex/` branch first. Real Codex execution
requires a separately authorised programmatic envelope and is not enabled by
installing the package. `validate --quality-gate` invokes the repository's
canonical `eng/ci.ps1 -Offline`; it does not substitute another gate.

## Worktrees and integration

`GitWorktreeManager` creates only paths below its configured managed root and
stores ownership markers outside Git worktrees. It refuses existing paths,
branches, dirty mappings, missing markers and foreign ownership. Removal has no
force option.

`SequentialIntegrationPipeline` requires worker `PASS`, required read-only
reviews, `INTEGRATION_READY`, a full candidate commit ID, a clean isolated
`codex/` coordinator branch and a passing post-integration canonical quality
gate. Integration is sequential. A conflicting cherry-pick is aborted; failure
to restore the baseline becomes `UNEXPECTED_DIRTY_TREE`. The pipeline never
pushes, merges to `main`, releases or deploys.

## Persistence, recovery and idempotency

Runtime data is limited to
`artifacts-local/ai-orchestrator/<run-id>/`, which is already ignored by the
repository. Each revision is written to a same-volume temporary file, flushed,
renamed atomically and linked to an append-only SHA-256 journal. Completed
snapshots, attempt metadata and journal links are validated as untrusted input
on load.

An interrupted temporary write, invalid response, digest mismatch, recorded
held lock, orphan lock or incomplete execution never becomes success. `resume`
stops for reconciliation when any lock remains. `status`, `validate`, report-
only `cleanup` and safe `resume` reads are idempotent. Destructive cleanup
requires the exact `--confirm-run-id`, terminal tasks and zero locks; it can
remove only that contained run directory.

Automatic retry is limited to `TRANSIENT_FAILURE`, respects the task's maximum
of three attempts and preserves every attempt. Policy, authority, test,
implementation and resource failures do not receive blind retries. Corrective
work is represented by a new dependent task rather than rewriting history.

## Security

Plans, repository content and agent output are untrusted data. Identifiers,
paths, Git object IDs, enums, task fields and result fields are bounded and
closed. Absolute, traversal, alternate-stream, wildcard, ambiguous Windows and
reparse-boundary paths fail closed. Agent-reported commands are evidence only;
they are never executed.

Coordinator-owned processes use executable-plus-argv with `shell: false`, no
login shell, a bounded explicit environment, timeout and output limits.
Structured events contain correlation IDs, opaque location IDs, timing,
result and stop code only; they exclude prompts, objectives, environment
values, raw agent output and absolute paths. Secrets, provider calls, network,
web search, tracing and approval prompts are not part of the current execution
surface.

The Human Gate remains external. Reaching a Human Gate task persists
`HUMAN_REVIEW_REQUIRED`, emits `HUMAN_GATE_REQUIRED` and stops. The tool cannot
accept an ADR, adjudicate human evidence, change lifecycle, enable RB-4 or
approve its own result.

## Verification

`npm run check` runs text/header hygiene, strict TypeScript type checking, a
build and serial Node tests with enforced floors of 70% lines and 45% branches.
The suite covers contracts, graph failures, state transitions, ownership and
resource collisions, contract freeze, worktree/baseline mapping, structured
output, retry policy, persistence, recovery, dry run, controlled E2E delegation,
review, quality-gate boundary, Human Gate and injected failures. No test invokes
Codex, an API, a provider or a paid service.
