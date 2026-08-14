# Multi-Agent Readiness Audit

## Result

The Stage 1 correction set is prepared, but final clean-worktree validation is
pending in this snapshot. The provisional Stage 2 readiness classification is:

```text
HUMAN_DECISION_REQUIRED
```

Stage 2 has not started. Two independent human decisions are required: the
owner must dispose the RB-2 adjudication-authority conflict, and proposed
ADR-0016 must be explicitly accepted or rejected before its tooling stack is
materialised.

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
| `GOV-001` | `BLOCKER` | The RB-2 checkpoint requires two independent human reviews and human adjudication without agent-authored decisions, but the retained package records both reviewers with `humanAttribution=false`, twenty agent-authored decisions, zero human decisions and a contradictory `no agent-authored adjudication` claim. | RB-2 does not satisfy its gate; RB-3 cannot be consumed by RB-4. | Frozen bytes preserved; current claims corrected; `HUMAN_DECISION_REQUIRED`. |
| `ARCH-001` | `BLOCKER` | Stage 2 introduces a tooling stack, physical boundary, runner contract, persistence and security model without an accepted owning ADR. | Repository rules prohibit materialising the orchestrator. | ADR-0016 prepared as `proposed`; explicit owner decision required. |
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
| `TEST-001` | `MEDIUM` | xUnit parallelism is not globally configured while some tests call global SQLite pool cleanup. | Multiple concurrent test processes or poorly isolated cases may interfere. | Full gate remains sequential per worktree; focused follow-up remains open. |
| `NET-001` | `MEDIUM` | Some harnesses release a dynamic port before host bind or use fixed default ports. | A TOCTOU collision can create flaky or false evidence. | Exclusive task lease/port is required; Stage 2 must test collision handling. |
| `TOOL-001` | `MEDIUM` | `eng/format.ps1` mutates tracked and untracked non-ignored files. | A worker could rewrite owner inputs. | Classified coordinator-only and excluded from validation dispatch. |
| `STATE-001` | `MEDIUM` | Current State retained long historical sequences with present-tense wording. | Old statements could be mistaken for current authority. | Current section corrected and the retained sequence explicitly labelled historical; full historical migration is not required for Stage 2. |

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

The offline form omits dependency audits and must be reported as partial
relative to the hosted online workflow. The final report must record every
focused Stage 1 command and the clean-worktree aggregate result individually;
the transition log is appended only after those results are observed.

Current validation state before the clean-worktree run:

| Command | Result |
|---|---|
| `./eng/test-assert-coverage.ps1` | `PASS`: all 11 policy cases passed, including the branchless failure case. |
| `./eng/ci.ps1 -Offline` in the coordinator worktree | `FAIL` at repository hygiene only: build, 505 .NET tests, coverage (95.38% lines; 67.23% branches), web lint/typecheck, 45 web tests and web build passed; the three owner-owned untracked prompt files were rejected only for CRLF. |
| `./eng/ci.ps1 -Offline` in a clean isolated worktree | `PENDING`. |

The owner-owned Stage 0/1/2 prompt files use CRLF and are intentionally
unmodified. Therefore the repository hygiene script is expected to fail in
the coordinator worktree when it includes those untracked inputs; the final
tracked candidate must be validated from a clean isolated worktree rather than
rewriting or hiding the owner's files.

## Human decisions and readiness

### RB-2/RB-3 disposition

The existing freezes cannot be corrected in place.

1. Recommended: retain/quarantine them as historical evidence and authorise a
   successor RB-2 with two real independent human reviews and human
   adjudication; produce a successor RB-3 if its bound inputs change.
2. Alternative: formally change the human-adjudication requirement and accept
   the risk, while still producing internally coherent successor artefacts.

The second option changes a requirement and risk disposition. Neither option
authorises RB-4, a provider call or a new embedding materialisation by itself.

### ADR-0016

The proposal recommends a deterministic TypeScript/Node 24 tool under
`tools/ai-orchestrator/`, with `FakeAgentRunner` and direct
`@openai/codex-sdk` integration behind `AgentRunner`. The exact acceptance
phrase is:

```text
ADR-0016: ACEITAR.
```

Acceptance is architecture authority only. It does not resolve RB-2 or grant
implementation, authentication, network, billing, production, push or merge
authority.

Until both decisions are made and the baseline is re-evaluated, Stage 2 is
not authorised.
