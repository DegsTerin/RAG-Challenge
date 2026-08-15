# Stage 2 Multi-Agent Orchestrator Implementation Report

## 1. Baseline

```text
Branch: codex/stage1-multi-agent-readiness
Initial HEAD: 5f98f128a605a577eb987be3f951d90d9453b193
Initial working tree: tracked files clean; the three owner Stage 0/1/2 prompts untracked and preserved
Lifecycle: STATE-07 TESTING_HOMOLOGATION active; unchanged by Stage 2
Stage 1 readiness: HUMAN_DECISION_REQUIRED initially; owner quarantine and ADR-0016 acceptance yielded READY_FOR_STAGE_2; the subsequent dependency preflight returned HUMAN_DECISION_REQUIRED; later bounded registry authority satisfied the final entry condition without rewriting either historical boundary
```

Stage 2 used the factual state left by Stage 1. The owner subsequently
authorised HTTPS access only to `registry.npmjs.org` for exact dependencies,
with lifecycle scripts, audit and funding calls disabled. No product provider,
Codex turn, API call, secret, billing, production, push, merge, release, RB-4,
Human Gate or lifecycle operation was authorised or performed.

## 2. Implemented architecture

The development-only tool is isolated under `tools/ai-orchestrator/` and is
not referenced by `RAG-Challenge.sln` or any product project. Its dependency
direction is:

```text
CLI and adapters
        |
        v
application coordinator and sequential integration pipeline
        |
        v
deterministic contracts, graph, scheduler and state machine
        ^
        |
AgentRunner, StateStore, ResourceLocks, ProcessExecutor and WorktreeManager ports
```

The core owns authority validation, task transitions, dependency ordering,
conflict detection, retry classification and result validation. The
coordinator owns state promotion, physical execution leases, canonical tests,
review binding, integration, final quality and the external Human Gate stop.
`FakeAgentRunner` supports deterministic local execution. `CodexRunner` maps
the verified direct `@openai/codex-sdk` surface behind the same port, but is
deny-by-default and is not exposed by the CLI for real execution.

The exact locked graph uses Node 24, npm 11, `@openai/codex-sdk` and
`@openai/codex` `0.147.0`, TypeScript `5.7.3`, `@types/node` `24.13.3` and
`undici` `7.18.2`.

## 3. Agents

| Agent | Authority | Write access | Purpose |
|---|---|---|---|
| `governance_guard` | Read-only governance and coordinator-owned deterministic tasks | None as a worker | Verify authority, integration/quality ordering and stop conditions. |
| `code_mapper` | Read-only repository inspection | None | Map bounded dependencies, ownership and mutable resources. |
| `architect` | Read-only accepted-decision assessment | None | Verify architecture and identify ADR-required changes. |
| `implementation_worker` | One explicit task envelope | One exclusive branch, worktree, writable root and resource set | Produce one candidate commit without integrating it. |
| `independent_reviewer` | Read-only trusted candidate evidence | None | Review the Git-derived candidate independently. |
| `security_reviewer` | Read-only trusted candidate evidence | None | Review security boundaries independently. |

No agent may accept an ADR, adjudicate human evidence, approve a Human Gate,
change lifecycle or broaden its task envelope.

## 4. Task model

Each closed `TaskDefinition` records identity, kind, objective, authority,
execution surface, owner, priority, dependency and blocker sets, allowed and
forbidden paths, ownership, shared resources, frozen contracts, acceptance
criteria, required tests, stop conditions, deliverables, worktree, branch,
parallelism, review requirements, candidate identity, attempt limit, timestamps,
result and evidence.

The coordinator-owned state machine is:

```text
DISCOVERED -> READY -> ASSIGNED -> RUNNING
RUNNING -> IMPLEMENTED | TESTING | REVIEW | INTEGRATION_READY | VALIDATING
INTEGRATION_READY -> INTEGRATING -> VALIDATING
VALIDATING -> PASS | FAIL | BLOCKED | HUMAN_REVIEW_REQUIRED | CANCELLED
FAIL | BLOCKED | HUMAN_REVIEW_REQUIRED -> READY only through an allowed transition
PASS and CANCELLED are terminal
```

Only a transient classified failure receives a bounded retry. Attempts,
thread identity and outcome are write-ahead records; recovery never replenishes
the cumulative attempt budget.

## 5. Dependency model

The parser rejects missing, contradictory or cyclic dependencies. Ready tasks
are ordered by descending priority and then `taskId`. The scheduler admits at
most three tasks and considers `SAFE_PARALLEL`,
`CONTRACT_FROZEN_PARALLEL`, `SINGLE_OWNER` and `SEQUENTIAL_ONLY` together with
path, worktree, branch, contract-owner and mutable-resource conflicts.

Every implementation requires a dependent independent review, security review
and exactly one coordinator-owned integration. Integrations form one evolving
HEAD chain. One canonical quality task and one external Human Gate must close
the entire writable dependency graph.

## 6. Isolation model

```text
branches: one distinct codex/ branch per writable lane; compare-and-swap deletion only
worktrees: one physically disjoint managed path per lane; equal, ancestor and descendant overlap rejected
ports: listeners are outside the generic surface; any authorised listener requires an exclusive task lease
stores: artifacts-local/ai-orchestrator/<run-id>, atomically replaced snapshots and append-only digest journal
temporary resources: same-volume temporary files and retained cleanup tombstones under physically checked roots
locks: declared resource locks plus one mandatory physical execution lease per running task
```

Paths are bounded lexically and physically. Symlink, junction, reparse, size,
identity and time-of-check drift are rejected where observable. Worktree
ownership markers are outside the worktrees. Cleanup and automatic rollback
preserve any foreign, dirty, advanced, partial or recreated resource when
ownership cannot be proved.

## 7. Integration pipeline

```text
implementation
-> coordinator-owned canonical tests
-> independent review
-> security review
-> sequential integration
-> post-integration QA/security validation
-> canonical quality gate
-> external Human Gate stop
```

The candidate identity is derived from Git, not from agent output. Integration
requires one candidate commit and exact commit, tree, complete diff and changed
file identities. A failed cherry-pick is preserved for explicit reconciliation.
A post-integration failure rolls back only an exact clean coordinator-owned
commit through compare-and-swap; observed drift is preserved and blocks.

## 8. Files created

All implementation files below are authorised by the Stage 2 owner envelope
and accepted ADR-0016. Schema, example, test and build files additionally
materialise the Stage 2 validation requirements.

| File | Purpose | Authority |
|---|---|---|
| `docs/Stage-2-Multi-Agent-Orchestrator-Report.md` | Mandatory implementation, evidence and readiness report. | Stage 2 final-report requirement. |
| `tools/ai-orchestrator/README.md` | Operator boundary, architecture, recovery, security and limitations. | ADR-0016 and Stage 2. |
| `tools/ai-orchestrator/examples/controlled-plan.json` | Safe preview plan. | Stage 2 dry-run requirement. |
| `tools/ai-orchestrator/examples/controlled-results.json` | Deterministic fake outcomes. | Stage 2 controlled-E2E requirement. |
| `tools/ai-orchestrator/package.json` | Exact package and command boundary. | ADR-0016 and bounded npm authority. |
| `tools/ai-orchestrator/package-lock.json` | Integrity-locked dependency graph. | Bounded npm authority. |
| `tools/ai-orchestrator/tsconfig.json` | Strict type-check configuration. | Stage 2 quality requirements. |
| `tools/ai-orchestrator/tsconfig.build.json` | Reproducible build configuration. | Stage 2 quality requirements. |
| `tools/ai-orchestrator/scripts/lint.mjs` | Module-header and text hygiene gate. | Stage 2 quality requirements. |
| `tools/ai-orchestrator/schemas/agent-result.schema.json` | Operator-facing result contract. | Stage 2 task contract. |
| `tools/ai-orchestrator/schemas/persisted-state.schema.json` | Operator-facing persisted-state contract. | Stage 2 persistence contract. |
| `tools/ai-orchestrator/schemas/project-plan.schema.json` | Operator-facing project-plan contract. | Stage 2 task contract. |
| `tools/ai-orchestrator/src/cli.ts` | Deny-by-default command surface. | ADR-0016 and Stage 2 CLI boundary. |
| `tools/ai-orchestrator/src/application/agent-prompt.ts` | Closed task envelope presented to runners. | Stage 2 authority model. |
| `tools/ai-orchestrator/src/application/coordinator.ts` | Deterministic execution, persistence, recovery and gate coordination. | ADR-0016. |
| `tools/ai-orchestrator/src/application/integration.ts` | Sequential trusted-candidate integration. | Stage 2 integration model. |
| `tools/ai-orchestrator/src/application/plan.ts` | Cross-task semantic and graph validation. | Stage 2 planning model. |
| `tools/ai-orchestrator/src/core/canonical-json.ts` | Stable canonical JSON and digests. | Stage 2 determinism. |
| `tools/ai-orchestrator/src/core/conflicts.ts` | Ownership and mutable-resource conflict detection. | Stage 2 isolation model. |
| `tools/ai-orchestrator/src/core/contracts.ts` | Frozen task, result, authority and persisted-state contracts. | Stage 2 task model. |
| `tools/ai-orchestrator/src/core/dependency-graph.ts` | Missing dependency and cycle rejection. | Stage 2 dependency model. |
| `tools/ai-orchestrator/src/core/errors.ts` | Typed canonical stop outcomes. | Stage 1 governance and Stage 2. |
| `tools/ai-orchestrator/src/core/retry.ts` | Bounded retry classification. | Stage 2 recovery model. |
| `tools/ai-orchestrator/src/core/scheduler.ts` | Deterministic bounded wave scheduling. | Stage 2 parallelism model. |
| `tools/ai-orchestrator/src/core/state-machine.ts` | Coordinator-owned closed transitions. | Stage 2 task model. |
| `tools/ai-orchestrator/src/core/validation.ts` | Closed untrusted-input validation. | Stage 2 contracts and security. |
| `tools/ai-orchestrator/src/observability/structured-log.ts` | Sanitised structured events. | Stage 2 observability boundary. |
| `tools/ai-orchestrator/src/ports/candidate-inspector.ts` | Trusted candidate-evidence abstraction. | ADR-0016 ports boundary. |
| `tools/ai-orchestrator/src/ports/integration-executor.ts` | Coordinator integration abstraction. | ADR-0016 ports boundary. |
| `tools/ai-orchestrator/src/ports/process-executor.ts` | Bounded subprocess abstraction. | ADR-0016 ports boundary. |
| `tools/ai-orchestrator/src/ports/resource-locks.ts` | Resource ownership abstraction. | ADR-0016 ports boundary. |
| `tools/ai-orchestrator/src/ports/state-store.ts` | Durable state abstraction. | ADR-0016 ports boundary. |
| `tools/ai-orchestrator/src/ports/thread-checkpoints.ts` | Pre-turn checkpoint abstraction. | ADR-0016 ports boundary. |
| `tools/ai-orchestrator/src/ports/worktrees.ts` | Worktree ownership abstraction. | ADR-0016 ports boundary. |
| `tools/ai-orchestrator/src/adapters/bounded-process.ts` | Shell-free bounded process execution and tree termination. | Stage 2 security model. |
| `tools/ai-orchestrator/src/adapters/codex-runner.ts` | Direct SDK adapter behind `AgentRunner`. | Accepted ADR-0016. |
| `tools/ai-orchestrator/src/adapters/fake-agent-runner.ts` | Deterministic local runner. | Accepted ADR-0016. |
| `tools/ai-orchestrator/src/adapters/file-resource-locks.ts` | Atomic physical lock records and recovery. | Stage 2 persistence model. |
| `tools/ai-orchestrator/src/adapters/file-state-store.ts` | Atomic snapshots and append-only journal. | Stage 2 persistence model. |
| `tools/ai-orchestrator/src/adapters/file-thread-checkpoints.ts` | Durable pre-turn checkpoints and prepared removal. | Stage 2 recovery model. |
| `tools/ai-orchestrator/src/adapters/git-baseline.ts` | Exact branch, HEAD and clean-tree verification. | Stage 2 Git boundary. |
| `tools/ai-orchestrator/src/adapters/git-candidate-inspector.ts` | Exact candidate commit/tree/diff derivation. | Stage 2 review boundary. |
| `tools/ai-orchestrator/src/adapters/git-worktrees.ts` | Owned worktree lifecycle and fail-closed cleanup. | Stage 2 isolation model. |
| `tools/ai-orchestrator/src/adapters/quality-gate.ts` | Canonical offline repository gate adapter. | Stage 2 quality model. |
| `tools/ai-orchestrator/src/security/git-process-policy.ts` | Fixed safe Git process configuration. | Stage 2 security model. |
| `tools/ai-orchestrator/src/security/git-repository-policy.ts` | Executable Git configuration and attribute rejection. | Stage 2 security model. |
| `tools/ai-orchestrator/src/security/path-policy.ts` | Physical containment and bounded file reads. | Stage 2 security model. |
| `tools/ai-orchestrator/src/security/secure-json.ts` | Duplicate/prototype-safe bounded JSON parsing. | Stage 2 security model. |
| `tools/ai-orchestrator/test/adapters.test.ts` | Adapter, SDK, persistence and integration contracts. | Stage 2 verification. |
| `tools/ai-orchestrator/test/cli.test.ts` | CLI, Human Gate and cleanup behaviour. | Stage 2 verification. |
| `tools/ai-orchestrator/test/coordinator.test.ts` | Coordinator, recovery, retry, E2E and gate behaviour. | Stage 2 verification. |
| `tools/ai-orchestrator/test/core.test.ts` | Core graph, conflicts, transitions and dry run. | Stage 2 verification. |
| `tools/ai-orchestrator/test/helpers.ts` | Closed deterministic test fixtures. | Stage 2 verification. |
| `tools/ai-orchestrator/test/security-boundaries.test.ts` | Real Git and filesystem security sentinels. | Stage 2 security verification. |

## 9. Files modified

| File | Purpose | Authority |
|---|---|---|
| `.github/workflows/ci.yml` | Restore and validate the locked orchestrator toolchain in hosted CI. | Stage 2 quality integration. |
| `eng/ci.ps1` | Add the orchestrator to the canonical aggregate gate. | Stage 2 quality integration. |
| `eng/test-ci-policy.ps1` | Prove fail-closed orchestrator CI consumption. | Stage 2 quality integration. |
| `docs/README.md` | Index Stage 2 evidence and current ADR-0016 status. | Documentary reconciliation. |
| `docs/architecture/README.md` | Reconcile ADR-0016 implementation and remaining condition. | Documentary reconciliation. |
| `docs/Multi-Agent-Readiness-Audit.md` | Preserve Stage 1 result and record subsequent condition satisfaction. | Documentary reconciliation. |
| `prompts/state/Current-State.md` | Record the factual Stage 2 result without lifecycle change. | Canonical factual-state ownership. |
| `prompts/state/State-Transition-Log.md` | Append the Stage 2 authority and execution history. | Append-only history ownership. |
| `prompts/system/Prompt-System-Change-Log.md` | Register corpus version `4.13.0`. | Prompt-corpus version ownership. |

## 10. Tests executed

| Command | Result | Evidence |
|---|---|---|
| `npm run check --offline --ignore-scripts` | PASS | 81 tests: 79 passed, 0 failed, 2 skipped because file-symlink creation was not permitted on this Windows host; 84.27% lines, 76.78% branches. |
| `./eng/ci.ps1 -Offline` in a clean detached worktree at `94ea9b7` | PASS | 215 unit, 11 architecture, 279 integration, 45 dashboard and 81 orchestrator tests; .NET 95.38% lines/67.23% branches; repository audit passed 404 files. |
| `node ./dist/src/cli.js run --dry-run --plan ./examples/controlled-plan.json` with validation-only exact baseline/cwd binding | PASS | Four deterministic waves, maximum concurrency three, one canonical quality gate and one external Human Gate. The example was restored before the validation worktree was removed. |
| `node --test --test-concurrency=1 --test-name-pattern="controlled E2E" ./dist/test/coordinator.test.js` | PASS | 1/1 controlled E2E test; trusted candidate, reviews, integration and quality remained deterministic. |
| Three independent read-only final reviews of snapshot `ef8bdf27...` | PASS | No P0, P1, P2 or P3; `SECURITY_REVIEW_PASS`, `MULTI_AGENT_READY`, `READY_FOR_GATES`. |
| `git diff --check` before the technical commit | PASS | No whitespace error in the reviewed snapshot. |

The canonical gate installed only from the verified local npm cache after the
initial authorised acquisition. It did not call an AI provider or other host.

## 11. Dry run

The exact validation baseline was
`94ea9b794f041c047363c85b0102e11e34fb2c9f`. The dry run produced:

```text
Wave 1: map-repository, check-architecture
Wave 2: independent-review
Wave 3: quality-gate
Wave 4: human-gate
Maximum concurrency: 3
Mutable lock: quality-gate:repository
Human Gate: external; never auto-approved
```

Dry run created no state, branch, worktree, lock or agent execution.

## 12. Recovery test

The suite proves:

- atomic snapshot replacement and append-only digest journal validation;
- interrupted temporary write and digest-tamper rejection;
- write-ahead attempt reservation, thread binding and outcome persistence;
- recovery before attempt reservation, before checkpoint creation and at a
  completed transient retry boundary;
- cumulative retry budget across crashes;
- bound checkpoint recovery and prepared checkpoint removal;
- physical execution leases, prepared lock release and confirmed absent-owner
  reconciliation;
- rejection of dirty, advanced, prunable, foreign or overlapping worktrees;
- safe partial cleanup and automatic rollback without adopting a recreated
  path; and
- exact coordinator integration rollback only while branch, HEAD and clean
  tree identity remain owned.

## 13. Security

| Risk | Mitigation |
|---|---|
| Authority or schema injection | Closed contracts, duplicate/prototype-key rejection and typed stop outcomes. |
| Secret or environment disclosure | Minimal environment allowlist, secret-name denial and sanitised evidence only. |
| Shell/process abuse | Absolute executable, argv with `shell: false`, no login shell, deadline, output bound and process-tree termination. |
| Git helper or protocol execution | Command-scoped disabled hooks/helpers/signing/fsmonitor/protocols plus repository config and attributes validation. |
| Path traversal or reparse escape | Lexical and physical containment, bounded no-follow reads and identity checks. |
| Worktree/branch deletion | External ownership marker, exact candidate binding, clean/ancestry proof and compare-and-swap branch removal. |
| Retry duplication or stale thread use | Write-ahead attempts, opaque persisted thread identity, exact checkpoint binding and cumulative attempt cap. |
| Agent self-approval | Agent results cannot promote state; reviews, integration, canonical quality and Human Gate remain distinct. |
| Provider/network use | CLI enables only `FakeAgentRunner`; execution surfaces set network/web search off and approval to `never`. |

The active local threat model requires the state root to be writable only by
the coordinator account. A concurrently privileged local process that can
replace a checked path between metadata validation and an operating-system
call is outside this boundary; any observed mutation is a stop condition.

## 14. Limitations

- `@openai/codex-sdk` `0.147.0` exposes a new thread ID only after the first
  turn starts. The required pre-turn durable identity cannot therefore be
  established for a new real turn. `CodexRunner` returns
  `ARCHITECTURE_CHANGE_REQUIRED` before that turn; persisted-thread resume is
  mapped and contract-tested.
- Real Codex execution remains disabled in the CLI and was not authorised or
  tested.
- The two file-symlink leaf tests were skipped because this Windows host did
  not grant symlink creation; junction and other physical-boundary tests ran.
- The tool is local development tooling, not a service, product runtime,
  authenticated Human Gate system or production controller.
- No provider, secret, billing, network after acquisition, tracing, product
  corpus, RB-4, deployment, push, merge, release or lifecycle behaviour was
  exercised.

## 15. Pending work

```text
TECHNICAL: obtain an SDK surface that exposes a new thread identity before the first turn, or retain fake-only operation while the resume mapping remains contract-tested but not operationally exercised
ARCHITECTURE: any design that starts a new real Codex turn requires an accepted ADR-0016 successor and must clear ARCHITECTURE_CHANGE_REQUIRED
GOVERNANCE: every real execution, Human Gate, lifecycle action and RB-2/RB-3 successor remains separately authorised
SECURITY: preserve coordinator-exclusive write access to the state root; rerun the two leaf-symlink tests on a host permitted to create file symlinks when such evidence is required
HUMAN_DECISION_REQUIRED: owner authority is required before changing the runner architecture or attempting real Codex execution
```

## 16. Operational readiness

```text
MULTI_AGENT_READY_WITH_CONDITIONS
```

The deterministic local orchestrator with `FakeAgentRunner`, isolation,
reviews, integration, quality and Human Gate stop are implemented and
validated on this Windows host. The persisted Codex resume path is mapped and
contract-tested only; it is not exposed by the CLI, was not exercised against
Codex and still requires separate provider, network, credential and execution
authority. A new real Codex thread is not operationally ready and fails closed
with `ARCHITECTURE_CHANGE_REQUIRED`. On a host that permits file-symlink
creation, both leaf-symlink sentinel tests must pass before claiming the same
host-level readiness.

## 17. Next permitted step

Only a separately authorised local, offline plan using `FakeAgentRunner` may
be executed on a clean exact baseline. No real Codex start may occur. If a new
real Codex turn is required, the next operation is an owner decision on an
ADR-0016 successor or a verified SDK version with a compatible pre-turn thread
identity; that decision is not part of this Stage 2 implementation.

## 18. Final Git state

```text
Branch: codex/stage1-multi-agent-readiness
Validated technical HEAD: 94ea9b794f041c047363c85b0102e11e34fb2c9f
Final HEAD: the exact documentary commit is reported in the owner hand-off because a commit cannot truthfully embed its own object ID
Working tree at documentary review: six tracked documentation files modified, this report untracked, no staged files
Modified: canonical documentation and history files listed in section 9
Untracked: this report plus the three owner Stage 0/1/2 prompts; only the prompts remain untracked after the focused documentary commit
Commits observed before documentary commit: b10d8ac, 9433962, ac41b13, a4889c3, 5da6b9a, 94ea9b7
```

No push, merge, release or deployment was performed.
