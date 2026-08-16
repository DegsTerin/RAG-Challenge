# Deterministic AI development orchestrator

## Purpose and boundary

This standalone TypeScript/Node 24 tool coordinates governed development work;
it is not part of the RAG-Challenge product runtime or solution. It implements
ADR-0016 and ADR-0017 behind a deterministic core and an `AgentRunner` port. The product
projects, product providers, corpus, database, OpenAPI contracts and lifecycle
remain unchanged.

The current package locks `@openai/codex` at `0.147.0`, TypeScript at `5.7.3`
and `@types/node` at `24.13.3`. Installation and CI use
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
`CodexRunner` is a typed adapter to the Codex App Server JSONL surface.

The App Server adapter maps `thread/start`, `thread/resume` and `turn/start`, an
isolated absolute working directory, task-bound read-only or workspace-write
sandboxing, `approvalPolicy: "never"`, disabled agent network and web search, a
closed output schema and a small environment allowlist. It requires a separate
execution authority and validates `account/read` as an existing ChatGPT session;
API-key and other provider authentication modes fail closed. A new durable
thread ID is checkpointed before `turn/start`, so an interrupted task can resume
by opaque thread ID. No terminal scraping, Codex SDK or Agents SDK dependency is
used.

## Agents and task contracts

Only the six Stage 1 roles are accepted:

- `governance_guard`;
- `code_mapper`;
- `architect`;
- `implementation_worker`;
- `independent_reviewer`;
- `security_reviewer`.

Plans, versioned results and execution surfaces are closed contracts. Their operator-facing JSON Schemas are
in [`schemas/`](schemas/); runtime validation remains the authoritative
acceptance boundary. A write task requires one `codex/` branch, one distinct
worktree, an explicit ownership class and named mutable resources. Every write
task requires both a dependent independent review and a dependent security
review, followed by exactly one coordinator-owned integration. Integrations
form one deterministic chain; one canonical quality gate and one external Human
Gate close the complete writable graph. Reviews receive the Git-derived
candidate commit, tree and changed-file set; they do not review an
agent-selected identity.

Candidate inspection also reads the exact commit message with
`git show -s --format=%B` and applies the coordinator repository's trusted
language manifest, schema binding, migration baseline and checker executable
before evidence or integration. Raw commit text passes the central non-echoing
secret boundary before language parsing. The coordinator checker uses its
repository as a separate trusted policy root, while candidate paths, modes,
bounded bytes, append-only prefixes and excluded regions come from the exact
immutable commit tree. Later worktree mutation cannot change that result. A
candidate cannot relax exclusions, schema constraints, digests or debt by
changing its own copy. Protected controls include the direct and transitive
secret, filesystem, Git, process and validation implementation plus their
enforcement tests; an ordinary candidate cannot change any of them.
Non-compliant technical prose stops with
`OUT_OF_SCOPE_CHANGE_REQUIRED`; secret-shaped commit text stops with
`SECRET_REQUIRED`; prior Git history remains unchanged.

The initial Stage 0/1/2 language-control change is a manually reviewed
exception outside the ordinary candidate-range path. The checker exposes no
general policy-update capability or reusable authority flag. Rechecking that
historical bootstrap range under the post-bootstrap rule therefore reports the
control-file change and requires the already separate manual review; this local
control does not claim an external branch-protection configuration.

Agent tasks and deterministic tasks remain separate. Agent-declared test IDs
or exit status are never coordinator-observed proof. The coordinator runs the
fixed canonical offline gate after each implementation, after each sequential
integration and once more as the final quality task. An implementation-agent
`PASS` can produce only `IMPLEMENTED` after worktree and Git inspection.
Independent and security reviews are distinct read-only tasks. Integration and
the canonical quality gate are coordinator-owned executors and are never
delegated to an `AgentRunner`.

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
node .\dist\src\cli.js run --plan <plan.json> --runner fake --fixture-results <results.json> --git-executable <absolute-git.exe> --powershell-executable <absolute-pwsh.exe>
node .\dist\src\cli.js run --plan <plan.json> --runner codex --authority-reference <authority-id> --git-executable <absolute-git.exe> --powershell-executable <absolute-pwsh.exe>
node .\dist\src\cli.js status --run-id <run-id>
node .\dist\src\cli.js resume --run-id <run-id> --runner fake --fixture-results <results.json> --git-executable <absolute-git.exe> --powershell-executable <absolute-pwsh.exe>
node .\dist\src\cli.js resume --run-id <run-id> --runner codex --authority-reference <authority-id> --git-executable <absolute-git.exe> --powershell-executable <absolute-pwsh.exe>
node .\dist\src\cli.js resume --run-id <run-id> --reconcile-absent-locks --confirm-run-id <run-id> --confirm-runner-quiescence <run-id> --runner fake --fixture-results <results.json> --git-executable <absolute-git.exe> --powershell-executable <absolute-pwsh.exe>
node .\dist\src\cli.js validate --run-id <run-id>
node .\dist\src\cli.js validate --run-id <run-id> --quality-gate --powershell-executable <absolute-pwsh.exe>
node .\dist\src\cli.js cleanup --run-id <run-id>
```

`plan` and `run --dry-run` do not create state, worktrees, branches, locks or
agent executions. The preview lists tasks, dependencies, agents, execution
waves, worktrees, branches, resources, locks, quality gates, the external Human
Gate and candidate conflicts. The example in
[`examples/controlled-plan.json`](examples/controlled-plan.json) is for preview
only; its baseline must be replaced by the exact clean Git HEAD before a run.

Non-dry-run CLI execution supports `--runner fake` by default and `--runner
codex` only with `--authority-reference`. Both require absolute Git and
PowerShell paths and verify an exact clean Git HEAD on a named `codex/` branch
first. Real execution uses the existing local ChatGPT login and never inherits
`OPENAI_API_KEY`; installing the package alone does not authorise a run. An
explicit model additionally requires `--model <id> --permitted-models <id,...>`.
`validate --quality-gate` invokes the repository's canonical
`eng/ci.ps1 -Offline`; it does not substitute another gate.

## Worktrees and integration

`GitWorktreeManager` creates only paths below its configured managed root and
stores ownership markers outside Git worktrees. It refuses existing paths,
branches, dirty mappings, missing markers and foreign ownership. Removal has no
force option and deletes a task branch only with compare-and-swap `update-ref`
against the exact persisted candidate commit. An exact persisted task envelope allows cleanup to
finish branch and marker removal after a previously completed worktree removal;
missing or foreign ownership still fails closed.

`SequentialIntegrationPipeline` requires a trusted `IMPLEMENTED` candidate,
required read-only reviews, exactly one candidate commit, full commit and tree
IDs, the exact Git-derived diff including deletions, and a clean isolated
`codex/` coordinator branch at the expected evolving HEAD. Integration is
sequential. It compares the complete bounded candidate patch with the patch
actually integrated, so multiple disjoint candidates can advance the
coordinator HEAD without weakening candidate identity. A conflicting
cherry-pick is quit without discarding its partial state and returns
`UNEXPECTED_DIRTY_TREE` for explicit reconciliation. After a successful
cherry-pick, post-integration validation or test failure moves the branch back
only when HEAD, branch and a clean worktree still prove ownership of the exact
integrated commit. It uses compare-and-swap `update-ref`, restores that known
tree and then proves both HEAD and status; any drift is preserved without an
automatic rollback. The pipeline never pushes, merges to `main`, releases or
deploys.

The separate downstream `QUALITY_GATE` task invokes only the canonical offline
gate. A failed review, integration or quality gate prevents the Human Gate.

## Persistence, recovery and idempotency

Runtime data is limited to
`artifacts-local/ai-orchestrator/<run-id>/`, which is already ignored by the
repository. Each revision is written to a same-volume temporary file, flushed,
renamed atomically and linked to an append-only SHA-256 journal. Snapshots,
journals, locks and ownership markers have byte limits. JSON input rejects
duplicate and prototype keys before closed-schema validation. Completed
snapshots, thread checkpoints, attempt metadata and journal links are validated
as untrusted input on load. Thread checkpoints are bound to the immutable task
envelope, baseline, candidate, owner, task kind, state revision and deadline.
The coordinator writes a pre-turn checkpoint and attempt record before lock
acquisition; the runner's opaque thread ID is then written to the append-only
attempt history before streamed work continues.
Review prose persists only as SHA-256 references. The Human Gate package may
also retain closed, bounded finding items with severity, repository-relative
location and summary, and risk items with severity, summary and mitigation.
Command output is minimised before persistence.

An interrupted temporary write, invalid response, digest mismatch, recorded
held lock or orphan lock never becomes success. Every executing task holds a
coordinator-generated physical execution lease in addition to its declared
resource locks. Acquisition and release update persisted ownership per task
rather than per wave. A persisted lock must have a physical record; a physical
record created immediately before a crash is accepted only when its owner and
pre-turn checkpoint match the exact interrupted attempt. Release and
absent-owner reconciliation use recoverable prepare, persisted-state and
finalise phases. `resume` requires a bound pre-turn checkpoint once agent
execution can have started. It also recognises three narrower coordinator-owned
boundaries without one: wave assignment before attempt reservation, a reserved
attempt with no thread or lock before checkpoint creation, and a completed
transient attempt awaiting its next bounded retry. Absent-owner locks require `--reconcile-absent-locks`, exact
`--confirm-run-id` and exact `--confirm-runner-quiescence`; active, invalid,
foreign or incomplete lock sets remain blocked. Recovery records an
`INTERRUPTED` attempt and assigns a new attempt identity without replenishing
the cumulative attempt budget. Attempt start, thread binding and outcome are
write-ahead revisions serialised by the coordinator even while runners execute
in parallel. Dirty or advanced implementation worktrees
cannot resume and require separate preservation or quarantine authority;
the tool never adopts or discards their contents. Deterministic-task
interruption remains fail-closed. Agent turns have a coordinator-owned deadline;
locks and checkpoints remain preserved when termination cannot be confirmed.
`status`, `validate`, report-only `cleanup` and safe `resume` reads are
idempotent. Resume derives the expected clean coordinator HEAD from the
contiguous persisted integration chain. Prepared checkpoint removals are
finalised only when the persisted attempt proves their ownership. Destructive
cleanup requires the exact `--confirm-run-id`, terminal
tasks and zero locks. It removes task-owned worktrees and branches first, then
atomically renames the physically contained run directory to a same-parent
tombstone retained as a quarantined audit record. It never recursively deletes
the run record.

Automatic retry is limited to `TRANSIENT_FAILURE`, respects the task's
cumulative maximum of three attempts across recovery and preserves every
attempt. `INTERRUPTED`, `TIMED_OUT` and `CANCELLED` remain distinct history;
policy, authority, test, implementation and resource failures do not receive
blind retries. Corrective
work is represented by a new dependent task rather than rewriting history.

## Security

Plans, repository content and agent output are untrusted data. Identifiers,
paths, Git object IDs, enums, task fields and result fields are bounded and
closed. Absolute, traversal, alternate-stream, wildcard, ambiguous Windows and
reparse-boundary paths fail closed. State and cleanup are physically anchored
at the repository root, not merely lexically contained. Agent-reported commands
are evidence only; they are never executed.

The local development threat model requires the orchestrator state root to be
writable only by the coordinator account while a run is active. Node pathname
operations cannot make each metadata check and subsequent filesystem mutation
handle-relative and atomic; an independently privileged local process that can
replace those paths concurrently is outside this boundary. Unexpected local
mutation is a stop condition, not a recoverable ownership transfer.

Coordinator-owned processes require absolute executables and use argv with
`shell: false`, no login shell, a closed environment allowlist, timeout, output
limits and process-tree termination. Git additionally disables hooks, signing,
credential helpers, prompts, pagers, fsmonitor and external protocols through
fixed command-scoped configuration. Repository and worktree configuration is
revalidated before sensitive commands; executable filters, text conversion,
merge drivers, includes and similar keys are rejected. Custom `filter`, `diff`
or `merge` attributes and candidate changes to `.gitattributes` or
`.gitmodules` are also rejected. Raw bounded stdout is available only to
structural parsers; persisted evidence is sanitised across Windows, UNC and
POSIX path forms.
Structured events contain correlation IDs, opaque location IDs, timing,
result and stop code only; they exclude prompts, objectives, environment
values, raw agent output and absolute paths. Secrets, agent network, web
search, tracing and approval prompts are excluded. The separately authorised
Codex service connection uses only the existing ChatGPT session and is distinct
from the product's provider boundary.

The former Codex SDK boundary exposed a new thread ID only after its first turn
started. ADR-0017 replaces that transport with App Server `thread/start`, which
returns the durable identity before `turn/start`. The CLI still keeps real
Codex execution disabled unless the operator supplies a bounded authority
reference and a plan that passes every existing coordinator gate.

The Human Gate remains external. `status`, `validate`, `resume` and `cleanup`
revalidate persisted plan semantics, including one coherent terminal attempt
per non-human task and canonical quality evidence. `status` labels its package
`LOCAL_UNAUTHENTICATED` and cannot request a decision. Only `validate` with a
live exact Git HEAD check and a newly passed canonical offline quality gate can
label the package `LOCAL_UNAUTHENTICATED_LIVE_REVALIDATED` and make it
decision-ready without claiming authenticated provenance. A package is emitted only when the one
external gate is reached with zero locks, trusted candidates, passed reviews
and quality gates, and the complete dependency closure. Reaching a Human Gate task persists
`HUMAN_REVIEW_REQUIRED`, emits `HUMAN_GATE_REQUIRED` and stops. The tool cannot
accept an ADR, adjudicate human evidence, change lifecycle, enable RB-4 or
approve its own result.

## Verification

`npm run check` runs text/header hygiene, strict TypeScript type checking, a
build and serial Node tests with enforced floors of 70% lines and 45% branches.
The suite covers contracts, graph failures, state transitions, ownership and
resource collisions, contract freeze, worktree/baseline mapping, structured
output, retry policy, persistence, recovery, dry run, controlled E2E delegation,
review, evaluable Human Gate package, quality-gate boundary, duplicate/prototype JSON, physical
reparse boundaries, coordinator `cwd` binding, forged initial state, agent
deadlines and cancellation, pre-turn App Server checkpointing and resume,
ChatGPT-only authentication, approval and user-input denial, malformed output,
protocol timeout and transport failure, absent-owner lock
reconciliation including prepared tombstones, mandatory execution leases,
write-ahead retries, parallel-wave crash recovery, cumulative recovery budget, real
two-candidate integration, real ownership-proved rollback, drift preservation, deletions,
multi-commit rejection, Git configuration classes, path sanitisation, output
overflow, partial cleanup recovery, and a disposable real Git worktree lifecycle
with compare-and-swap cleanup. No test invokes
Codex, an API, a provider or a paid service.
