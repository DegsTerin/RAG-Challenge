# Multi-Agent Readiness Audit

## Result

The Stage 1 audit, permitted corrections and clean-worktree validation are
complete. Its final classification at the dependency preflight boundary was:

```text
HUMAN_DECISION_REQUIRED
```

The owner selected historical quarantine for the invalid RB-2/RB-3 retained
package and explicitly accepted ADR-0016 on the post-audit baseline
`355bd6cd731528bcdb8fccfe71ee93b70acb1d1e`. Both blocking decisions are now
disposed. RB-2 remains invalid, RB-3 remains unavailable to RB-4 and successor
human review remains separately governed; none is a Stage 2 dependency.

The Stage 2 dependency preflight on the resulting committed baseline
`60ccbdc4ec1e53bd456ba91c339846d65ada95e3` found no locally cached or
installed `@openai/codex-sdk` package. ADR-0016 required an exact locked SDK
version and contract tests, while the authority at that historical boundary
did not permit registry acquisition.

The owner subsequently granted the exact bounded npm-registry authority
requested by this report. The package condition was satisfied, Stage 2 was
implemented and validated, and its current result is recorded separately in
[`Stage-2-Multi-Agent-Orchestrator-Report.md`](Stage-2-Multi-Agent-Orchestrator-Report.md).
This later execution does not rewrite the Stage 1 classification or its
evidence.

## Scope and baseline

The audit covered governance, requirements, accepted and proposed ADRs,
lifecycle and factual state, contracts, OpenAPI, schemas, migrations, CI,
validation scripts, all test suites, evaluation design, the complete retained
RB-2/RB-3 package and local worktree inventory.

Observed starting baseline:

| Field | Value |
|---|---|
| Branch | `main` |
| HEAD | `9f309e1b6a21a33cbd24b4b6498e840dd26585c9` |
| Upstream | `origin/main`, local branch ahead by one commit |
| Tracked tree | Clean |
| Owner-owned untracked inputs | Stage 0, Stage 1 and Stage 2 prompt files |
| Tags | None |
| Lifecycle | `STATE-07 TESTING_HOMOLOGATION` active |
| Prompt corpus | `4.10.42`, 13 files under `prompts/` |
| `.codex` | Absent |
| OpenAPI v1 | SHA-256 `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` |
| OpenAPI v2 | SHA-256 `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` |
| Toolchains observed | .NET `10.0.303`; Node `24.19.0`; npm `11.17.0`; PowerShell `7.6.4` |

The Stage 1 write branch is `codex/stage1-multi-agent-readiness`. The three
owner prompt files remain unmodified and untracked.

Existing worktree records were treated as foreign resources:

- a prunable record for `<worktree-root>/RAG-Challenge-task` on
  `codex/ingestion-hardening@8107a8c`; and
- an existing detached worktree under the user's Codex worktree root at
  `a815252`.

Neither record was pruned, reused, cleaned or modified.

## Authority hierarchy

The determinable order for this audit is:

1. platform/system instructions and the owner's current Stage 0/1/2 request;
2. security, data-protection and lifecycle boundaries;
3. the global and repository `AGENTS.md` files;
4. `prompts/Start-Here.md` and its thematic routing;
5. `prompts/state/Current-State.md` for the factual present;
6. accepted Governance, Quality Gates, Security and Lifecycle rules;
7. accepted ADRs and frozen shared contracts within their stated scope;
8. observed implementation, scripts and tests as evidence of behaviour, not
   authority to broaden it; and
9. templates, roadmaps and historical reports as format, planning or retained
   evidence only.

An accepted ADR constrains implementation but does not prove implementation or
grant execution authority. An artefact literal such as `complete` does not
override a higher-authority Human Gate requirement when its own retained
evidence contradicts that requirement.

## Lifecycle reconstruction

The canonical sequence remains:

```text
STATE-00 DISCOVERY
  -> GATE-B01 ARCHITECTURE_BOOTSTRAP_DECISION
  -> STATE-01 PROJECT_SETUP
  -> STATE-02 ARCHITECTURE
  -> STATE-03 DATA_AND_INDEX_MODELING
  -> STATE-04 BACKEND_IMPLEMENTATION
  -> STATE-05 FRONTEND_IMPLEMENTATION
  -> STATE-06 INTEGRATION
  -> STATE-07 TESTING_HOMOLOGATION
  -> STATE-08 PRODUCTION_RELEASE
```

Each state transition requires deliverables, applicable automatic validation,
a factual report, an explicit Human Gate for one named state, append-only
history and a factual-state update. ADR acceptance, an automatic gate or a
worker result cannot perform a transition by implication.

The current lifecycle remains `STATE-07`; this audit does not execute its
Automatic Quality Gate or Human Gate and does not authorise `STATE-08`.

## Findings

| ID | Severity | Finding | Impact | Disposition |
|---|---|---|---|---|
| `GOV-001` | `BLOCKER` | The RB-2 checkpoint requires two independent human reviews and human adjudication without agent-authored decisions, but the retained package records both reviewers with `humanAttribution=false`, twenty agent-authored decisions, zero human decisions and a contradictory `no agent-authored adjudication` claim. | RB-2 does not satisfy its gate; RB-3 cannot be consumed by RB-4. | Disposed for Stage 2 readiness by explicit historical quarantine. Frozen bytes remain invalid and unavailable; successor work requires separate authority. |
| `ARCH-001` | `BLOCKER` | Stage 2 introduces a tooling stack, physical boundary, runner contract, persistence and security model without an accepted owning ADR. | Repository rules prohibit materialising the orchestrator. | Resolved by the owner's explicit acceptance of ADR-0016 on 2026-08-14. |
| `GOV-002` | `HIGH` | Governance lacked one normative task envelope, operational parallelism taxonomy, ownership classes, resource rules and canonical stop codes. | Delegations could infer scope, race or continue after authority loss. | Corrected in Governance. |
| `CFG-001` | `HIGH` | No project-scoped `.codex` configuration or specialised roles existed. | Agent capability, sandbox, environment and responsibility were implicit. | Six conservative roles and a three-subagent cap added; the writable role has deny-by-default network, web and shell-environment defaults plus mandatory effective-surface verification. |
| `QA-001` | `HIGH` | Cobertura aggregation treated zero valid branch observations as 100% coverage. | The 45% branch floor could pass without branch evidence. | Fixed fail-closed; a regression case was added and its execution evidence is recorded in the Quality gate section. |
| `ISO-001` | `HIGH` | The full CI gate shares restore/build/npm outputs in one worktree. | Concurrent gates can corrupt or misattribute evidence. | Full gate made explicitly sequential per worktree; writable lanes require separate worktrees/resources. |
| `DOC-001` | `HIGH` | Current State and indexes simultaneously claimed no container/deploy/GitHub resource and recorded a private GHCR image plus live Render deployment; other files retained superseded lifecycle/runtime claims. | Present factual state was ambiguous. | Current claims and indexes reconciled; historical context retained and labelled. |
| `SCRIPT-001` | `HIGH` | `eng/New-Oracle19ProductPlans.ps1` accepts and overwrites an arbitrary destination without a task-owned containment marker. | Generic agent dispatch could overwrite a shared path. | Not changed in this stage; excluded from generic dispatch until separately contained and tested. |
| `GIT-001` | `MEDIUM` | One prunable and one detached foreign worktree record already exist. | Automated cleanup or reuse could destroy foreign work. | Inventoried and protected; no automatic prune/reuse. |
| `SEC-001` | `MEDIUM` | The online NuGet vulnerability command relies on exit code and does not prove that a reported vulnerable package fails the gate. | Dependency findings may require manual interpretation. | Technical follow-up; no threshold was weakened. |
| `QA-002` | `MEDIUM` | `eng/ci.ps1 -Offline` omits online dependency audits. | An offline PASS is not equivalent to the hosted workflow. | Difference formalised in Quality Gates and evidence labels. |
| `EVID-001` | `MEDIUM` | An authority-gated S07 campaign test returns normally when authority is absent and may be counted as PASS rather than explicit skip/not-run. | Aggregate test counts do not prove campaign execution. | Campaign remains outside the generic gate/dispatch; dedicated authority and readback remain mandatory. |
| `TEST-001` | `HIGH` | Integration test classes ran in parallel while thirteen calls across seven files clear process-wide SQLite pools. The first clean gate failed one of 279 integration tests with a disposed native SQLite handle, while that test passed in isolation. | Independent fixtures could invalidate another class's pooled handle and make the canonical gate intermittent. | Resolved by assembly-level serialisation of integration test classes; the focused suite passed 279/279 and the clean aggregate gate then passed. |
| `NET-001` | `MEDIUM` | Some harnesses release a dynamic port before host bind or use fixed default ports. | A TOCTOU collision can create flaky or false evidence. | Exclusive task lease/port is required; Stage 2 must test collision handling. |
| `TOOL-001` | `MEDIUM` | `eng/format.ps1` mutates tracked and untracked non-ignored files. | A worker could rewrite owner inputs. | Classified coordinator-only and excluded from validation dispatch. |
| `STATE-001` | `MEDIUM` | Current State retained long historical sequences with present-tense wording. | Old statements could be mistaken for current authority. | Current section corrected and the retained sequence explicitly labelled historical; full historical migration is not required for Stage 2. |
| `DEP-001` | `BLOCKER` | The local npm cache and installed package inventory contained no `@openai/codex-sdk`, but ADR-0016 required an exact locked SDK version, a reproducible lockfile and `CodexRunner` contract tests. | Complete Stage 2 implementation could not be built or validated without acquiring and verifying the package graph. | Resolved after bounded owner authority: the exact `0.147.0` package graph and lockfile were acquired and validated with lifecycle scripts disabled. |

## Corrections made

- Extended existing Governance instead of creating a competing governance
  source.
- Formalised task envelopes, operational parallelism, ownership, shared
  resources, worktree isolation, locks, agent roles and canonical stop codes.
- Added project-scoped Codex agent definitions with five read-only roles and
  one narrowly writable implementation role. Its default layer disables
  network and web search, filters the shell environment and requires the
  coordinator to verify the effective inherited surface before dispatch.
- Defined the canonical CI order, offline limitation and sequential final gate
  in Quality Gates.
- Corrected the branch-coverage fail-open and its regression tests.
- Serialised integration test classes at assembly scope because their retained
  cleanup calls clear SQLite pools process-wide; in-method concurrency tests
  remain intact.
- Prepared ADR-0016 as a proposal only, with direct Codex SDK behind an
  `AgentRunner` boundary and no package/dependency installation.
- Reconciled current-state, architecture, security, RAG and project-foundation
  statements that were objectively superseded.
- Appended a governance reaudit disposition to retained ADR/report evidence
  without mutating any RB-2/RB-3 freeze.

No product feature, OpenAPI, schema, migration, product corpus, product index,
provider, secret, billing configuration, deployment or lifecycle state was
changed.

## Parallelism and ownership model

| Operation class | Rule |
|---|---|
| `SAFE_PARALLEL` | Read-only analysis/review or fully isolated tests and outputs. |
| `CONTRACT_FROZEN_PARALLEL` | Disjoint writable lanes after shared contracts have an owner, version/hash and frozen baseline. |
| `SINGLE_OWNER` | Exactly one writer for contracts, schemas, migrations, solution/projects, lockfiles, CI/configuration, manifests and canonical state. |
| `SEQUENTIAL_ONLY` | Human/ADR/lifecycle decisions, adjudication, one-shot campaigns, integration, destructive/external operations and the final quality gate. |

Writable isolation is:

```text
1 writable lane
= 1 branch
+ 1 worktree
+ 1 ownership scope
+ isolated mutable resources
```

The coordinator integrates one candidate at a time. A worker never integrates
its own branch, updates canonical project memory or decides a gate.

## Critical shared resources

| Resource | Risk | Required isolation |
|---|---|---|
| `bin/`, `obj/`, `node_modules/`, `dist/` | Concurrent rewrite and stale build evidence | Exclusive worktree; no concurrent full gate |
| NuGet/npm caches | Shared mutable cache or partial install | Task namespace where mutable, otherwise sequential coordination |
| TestResults/coverage/artefact roots | Output overwrite or evidence mixing | Unique task-owned root and run ID |
| SQLite/PostgreSQL/vector stores | Locking, corruption or cross-test state | Exclusive database/store or verified lease |
| Corpus, embeddings and indexes | Irreversible drift or incompatible reads | Frozen identity/hash for readers; single owner for mutation |
| Ports/listeners/browser profiles/containers | Collision, wrong process or false positive | Exclusive lease, owner marker and verified cleanup |
| Secrets/providers/external resources | Disclosure, cost or unauthorised mutation | Human authority, least privilege and no worker sharing |

## Prepared agent roles

| Role | Sandbox | Authority |
|---|---|---|
| `governance_guard` | read-only | Authority, lifecycle, ADR, gates and stop conditions |
| `code_mapper` | read-only | Repository impact, dependencies, tests and resources |
| `architect` | read-only | Boundaries, contracts, options and ADR need; cannot accept a proposal |
| `implementation_worker` | workspace-write | One authorised isolated task envelope only |
| `independent_reviewer` | read-only | Independent correctness, scope, regression and evidence review |
| `security_reviewer` | read-only | Secret, trust, path, subprocess, provider and supply-chain review |

The writable role defaults to `workspace-write`, no outbound network, disabled
web search, no approval escalation, a core-only shell environment and explicit
secret-name filters. Parent live overrides and inherited tool, MCP or skill
surfaces can still supersede defaults, so the coordinator must compare the
effective execution surface to the task envelope and refuse dispatch on any
extra capability. Configuration grants no requirement, ADR, Human Gate,
lifecycle, provider, billing, production or release authority.

## Quality gate

The canonical aggregate entry point is:

```powershell
./eng/ci.ps1
```

For an authorised offline local run:

```powershell
./eng/ci.ps1 -Offline
```

The offline form omits dependency audits and is partial relative to the hosted
online workflow. All commands below ran on 2026-08-14 with .NET `10.0.303`,
Node `24.19.0`, npm `11.17.0` and PowerShell `7.6.4`. Runtime preflight before
each executable validation found zero verified RAG-Challenge-owned processes
and zero listeners on the known task ports; nothing was stopped.

| Worktree / HEAD | Command | Exit / duration | Observed result |
|---|---|---|---|
| Coordinator / Stage 1 candidate | `./eng/test-assert-coverage.ps1` | `0`; focused run | `PASS`: 11/11 policy cases, including rejection of branchless `0/0` coverage. |
| Coordinator / Stage 1 candidate | `./eng/ci.ps1 -Offline` | `1`; diagnostic aggregate run | All executable checks passed: Release build with zero warnings/errors, 505 .NET tests, 95.38% line coverage, 67.23% branch coverage, web lint/typecheck, 45 web tests and web build. Repository hygiene then rejected only the three owner-owned untracked CRLF prompt files. |
| Clean isolated worktree / `1055934` | `./eng/ci.ps1 -Offline` | `1`; first clean aggregate run | `FAIL`: 278/279 integration tests passed. `BackendEndToEndWorkflowTests.SyntheticCsvCorpusFlowsFromIngestionToGroundedCitation` received `ObjectDisposedException` for `SQLitePCL.sqlite3` while opening a pooled connection. |
| Clean isolated worktree / `1055934` | Focused execution of the failed integration test | `0`; focused diagnostic run | `PASS`: 1/1, supporting cross-class interference rather than deterministic failure in the test's own fixture. |
| Coordinator / pre-rewrite `a0def61` (current equivalent `b64291d`) | Full `RagChallenge.IntegrationTests` suite | `0`; approximately 72 seconds | `PASS`: 279/279 after assembly-level integration-test serialisation. |
| `<worktree-root>/RAG-Challenge-stage1-ci-a0def61` (detached) / pre-rewrite `a0def61bf39471fd7647198d29bbcd2702171fca` (current equivalent `b64291d637b198120314f3152fc171b7904bb888`) | `./eng/ci.ps1 -Offline` | `0`; started `2026-08-14T21:42:36.9170657Z`; approximately 144 seconds | `PASS`: coverage policy 11/11; Release build zero warnings/errors; 215 unit, 11 architecture and 279 integration tests (505 total); 95.38% lines (50,110/52,539); 67.23% branches (5,164/7,681); web lint/typecheck; 45/45 web tests; web build; repository audit passed for 351 non-ignored files. |

The owner later authorised a local, unpublished history rewrite solely to
change the second commit subject from `serialize` to British English
`serialise`. Pre-rewrite `a0def61bf39471fd7647198d29bbcd2702171fca` and
current `b64291d637b198120314f3152fc171b7904bb888` have the identical tree
`896659a5c4f40e57e954dc3980d4ba2377d9acda`. Historical rows retain the old
identity where it names the commit actually executed; the current branch uses
the new identity.

The clean validation worktree remained detached at the exact candidate commit
with zero tracked or untracked changes after the aggregate run. TOML parsing
and agent/config invariants, internal Markdown links, protected OpenAPI hashes,
RB-2/RB-3 retained hashes and `git diff --check` also passed. No Stage 2 tool,
package, project or product dependency was present.

The owner-owned Stage 0/1/2 prompt files use CRLF and are intentionally
unmodified. Therefore the repository hygiene script is expected to fail in
the coordinator worktree when it includes those untracked inputs; the final
tracked candidate must be validated from a clean isolated worktree rather than
rewriting or hiding the owner's files.

## Human decisions and readiness

### RB-2/RB-3 disposition

The owner selected option 1 on 2026-08-14. The existing freezes are retained
unchanged as historical evidence, RB-2 is explicitly not gate-valid and RB-3
is unavailable for RB-4. Any successor requires separate authority, two real
independent human reviews and human adjudication; a successor RB-3 is required
if its bound inputs change. This disposition authorises no RB-4, provider call
or new embedding materialisation.

### ADR-0016

ADR-0016 was explicitly accepted by the owner on 2026-08-14 through
`ADR-0016: ACEITAR.`. It selects a deterministic TypeScript/Node 24 tool under
`tools/ai-orchestrator/`, with `FakeAgentRunner` and direct
`@openai/codex-sdk` integration behind `AgentRunner`.

```text
ADR-0016: ACEITAR.
```

Acceptance is architecture authority only. It does not resolve RB-2 or grant
implementation, authentication, network, billing, production, push or merge
authority.

### Re-evaluated readiness

The RB-2 defect is now contained outside Stage 2 by historical quarantine,
and ADR-0016 supplies the required accepted architecture. No remaining Stage 1
governance or architecture finding requires a further human decision.

The subsequent dependency preflight established one then-unsatisfied
operational condition: `@openai/codex-sdk` and its package graph were
unavailable from the local cache, and external package acquisition was outside
the authority at that boundary. The implementation could not fabricate a
lockfile, omit `CodexRunner` or use a terminal wrapper. The condition could be
satisfied by either:

1. bounded owner authority to resolve and acquire the exact SDK and required
   development dependencies from the npm registry with lifecycle scripts
   disabled, followed by lockfile and supply-chain validation; or
2. a complete, integrity-verifiable offline cache supplied under separate
   authority.

Because satisfying the condition required new owner authority or an
owner-supplied dependency source, the Stage 1 classification at that boundary
was `HUMAN_DECISION_REQUIRED`. The owner later supplied the bounded registry
authority, and the Stage 2 report records the resulting implementation,
validation and remaining `ARCHITECTURE_CHANGE_REQUIRED` condition for starting
a new real Codex thread.
