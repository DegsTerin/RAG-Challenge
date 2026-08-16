# Quality, Evidence and Gates

## Evidence standard

Every technical claim records, as applicable:

- command and directory;
- tool versions;
- date and environment;
- exit code;
- scope;
- summarised result;
- sanitised artefact;
- distinction between observed, inferred, not tested and blocked.

An apparent absence of errors, an isolated compilation or a model response does
not prove complete operation.

## Definition of Ready

A batch is ready when:

- requirements and acceptance criteria have IDs;
- state and authority are clear;
- positive and negative scope are defined;
- dependencies and blocking decisions are resolved;
- data, secrets and external actions are authorised;
- the test, evidence and rollback plan is proportionate to risk;
- when parallelism applies, the coordinator, independent lanes, ownership,
  isolation, messages and integration order are defined;
- the next conversation and each lane have a recommended Codex reasoning
  level, justification and alternative if unavailable;
- the [language policy](Language-Policy.md) applies to the batch without
  translating history or inferring interface language.

## Definition of Done

- Requirements and criteria are satisfied.
- Applicable build, formatting, lint, type and test checks pass.
- Dependency direction is preserved.
- Security, failures, limits and compatibility are assessed.
- Logs, health and errors are appropriate to the batch.
- Documentation and rollback are updated.
- No secret, unlicensed content or false evidence is present.
- Untested items and residual risks are explicit.
- Unrelated pre-existing changes are preserved.
- The diff is reviewed and the delivery is focused.
- The hand-off fully complies with [Governance](Governance.md) semantics and
  [Templates](../templates/Templates.md) format: it appears once in the final
  response, uses only applicable fields and keeps result, future deliverable,
  lifecycle, human action and routing distinct.
- Intermediate updates add materially new information and neither repeat nor
  preview the hand-off.
- Route, target, action and payload are coherent; mandatory text is complete
  and placeholder-free immediately after the conversation field, with label
  and fences outside the content, including a one-line Human Gate or a payload
  with an inner fence. Absence is accepted only when no action depends on a
  message.
- Runtime preflight was classified before any inspection and applies only to
  executable change or validation; documentation/read-only work does not
  announce, enumerate or stop a process.
- When applicable, the parallel classification satisfies the specific gate
  below; reasoning uses a canonical value, justification and fallback without
  broadening authority or claiming automatic configuration.
- Communication and artefacts comply with the
  [language policy](Language-Policy.md).
- Schema, policy digest, migration fingerprints, append-only identities,
  repository prose and applicable new commit messages pass the automated
  language check. Candidate checks use coordinator-owned policy bytes and the
  exact candidate commit tree, and reject changes to the full transitive
  enforcement boundary. This lexical result is
  necessary but never substitutes for
  independent semantic review of British English and the structured
  exclusions owned by the language policy.

## Coverage policy

- Initial future floor: 70% of lines and 45% of branches in the covered .NET
  suite.
- The aggregator fails closed when it finds no report, instrumented line or
  valid instrumented branch. Absence of branches does not equal 100%.
- 80% line coverage is a risk-based directional target.
- Coverage does not replace functional, negative, integration, contract, RAG,
  security, accessibility, recovery or performance tests.
- Critical code may require higher coverage.
- Exclusions must be narrow, justified and verifiable.
- In `STATE-00`, code coverage is `NOT_APPLICABLE`.

## Test strategy

| Type | Purpose |
|---|---|
| Unit | Invariants, versioning, hashing, policies, limits and failures. |
| Architecture | Inward dependencies and prohibited surfaces. |
| Contract | PDF/CSV adapters, catalogue, source/PNG content, render manifest, separate languages, internal answer evidence, embeddings, vector, LLM, preserved OpenAPI v1 and planned v2. |
| Integration | Persistence, separate generation/activation digests, observation rebinding, render/lifecycle, atomic activation, answer-evidence retention/reachability, rollback through a new record, restart and HTTP. |
| RAG evaluation | Retrieval, groundedness, citations, refusal, the `pt-BR`/`en-GB` matrix and additional strata by exact document language. |
| Security | Malicious PDF/CSV/renderer, image binding, language coercion, record poisoning, prompt injection, SSRF, source leakage, secrets and abuse. |
| Accessibility | Keyboard, focus, semantics, contrast, reflow, `pt-BR`/`en-GB` localisation, `Light`/`Dark` themes and text equivalent for visual evidence. |
| E2E | Document to answer and deployment to smoke. |
| Performance | Defined latency, limits, memory, cost and load. |
| Recovery | Indexing failure, incompatible generation, observation mismatch and rollback without freshness replay. |

Default tests use synthetic fixtures or a small authorised corpus and do not
depend on network or billing. External tests are opt-in, isolated and require
their own authority/configuration.

## Common automatic audit

1. Confirm state, scope and authority.
2. Check expected files and unrelated changes.
3. Discover and run the real commands.
4. Validate applicable formatting, build, tests, coverage and architecture.
5. Validate applicable dependencies, lockfiles, secrets and licences.
6. Check links, UTF-8/LF and trailing whitespace.
7. Check fail-closed configuration and absence of private local material.
8. Classify each gate as `APROVADO`, `REPROVADO`, `BLOQUEADO` or
   `NÃO APLICÁVEL`.
9. Audit the hand-off outcomes defined in Definition of Done against
   Governance and Templates, including uniqueness, conditional fields,
   intermediate commentary, vocabulary, route/target/action, copy-ready
   payload, inner fence, Human Gate, reasoning and fallback.
10. Confirm runtime preflight was classified before any inspection and that
    the observed decision matches the work type.
11. Record findings with severity, impact, reproduction and recommendation.

The documentary audit verifies every copy-ready payload against the complete
Stage 0/1/2 hand-off rule owned by [Governance](Governance.md), including its
applicability, exclusions and non-authority boundary. The audit must not
restate, replace or broaden that rule.

An audit does not correct silently, invent evidence or advance state.

## Canonical repository gate

The sole aggregated CI entry point is `./eng/ci.ps1`; the workflow must invoke
it exactly once. The governed order is:

1. language-policy tests and one fail-closed repository/commit check;
2. fail-closed CI and coverage policy tests;
3. LF verification of NuGet lockfiles;
4. locked restore;
5. repeat lockfile verification;
6. `dotnet format --verify-no-changes`;
7. Release build;
8. .NET tests and aggregated Cobertura coverage;
9. floors of 70% of lines and 45% of branches;
10. `npm ci`, lint, typecheck, Dashboard tests and build;
11. dependency audits only when online mode is authorised;
12. repository audit;
13. `git diff --check`.

`./eng/ci.ps1 -Offline` is partial local evidence because it omits dependency
audits that require network access. Its PASS must not be labelled equivalent
to the online workflow. Missing authority/network produces `BLOCKED` with an
explicit limitation, never an invented external audit.

The complete gate is `SEQUENTIAL_ONLY` per worktree: restore, `bin/`, `obj/`,
`node_modules/`, `dist/` and other caches/outputs are shared even when
`TestResults` uses a GUID. Lanes may run focused checks in isolated worktrees
and resources; after integration, the coordinator runs one aggregated gate on
the combined baseline.

`eng/format.ps1` is mutating, reaches tracked and untracked files and is not a
validator. Its use is coordinator-only, requires reviewed scope and must never
alter prompts or the owner's local work for convenience. The validation
command remains `dotnet format --verify-no-changes` inside the canonical gate.

## Parallel-work gate

A `PARALLEL_OPTIONAL` or `PARALLEL_RECOMMENDED` recommendation passes only
when:

- the coordinating conversation has a confirmed title or label;
- baseline and authority envelope are common and explicit; each lane receives
  only its authorised subset, global negative scope and its own additional
  restrictions;
- dependencies form independent fronts without consuming another lane's
  not-yet-integrated output;
- each path, artefact and mutable resource has one owner;
- before tracked Git exists, every simultaneous worker is `read-only`;
- after Git authority, writes use separate worktrees/branches and applicable
  runtime, data, temporary and output isolation;
- each lane has an exact message, checks, deliverable, stop conditions and
  coordinator return;
- coordinator and lanes have their own reasoning levels, with justification
  and an alternative if unavailable;
- workers do not update state/history, integrate other lanes, take human
  decisions or broaden authority;
- the coordinator integrates one deliverable at a time and runs cross-cutting
  checks on the combined result;
- conflict, stale baseline or insufficient isolation reclassifies the
  remainder as `SEQUENTIAL_ONLY`.

A Human Gate, lifecycle transition or ADR decision is never decided in
parallel. Independent lanes may produce evidence, but the decision and its
record belong to the coordinator after integration. `Ultra` may be recommended
only when this gate permits parallel work; its unavailability removes no
isolation, coordination or validation requirement.

## `STATE-00` documentary gate

- Structure matches the approved list.
- Local links resolve.
- Canonical names, IDs and headings are consistent.
- Files use UTF-8, LF, a final newline and no trailing whitespace.
- No secret, real host or unnecessary personal data is present.
- Official requirements, MVP interpretation and evolution are traceable.
- Risks, assumptions, criteria, backlog and roadmap exist.
- Current State contains the present; the log contains history.
- ADRs remain `proposed`.
- No capability is presented as implemented.
- During gate execution, the Human Gate remains `PENDENTE` until the automatic
  report is reviewed and a human decision is made; afterwards, Current State
  and history preserve the decision actually recorded.
- The order `Human Gate STATE-00` → `GATE-B01` → authority to enter `STATE-01`
  is explicit, without implicit ADR acceptance.
- The official source is in the MVP with one PDF URL, snapshot and query scope;
  no active claim says it is disabled/future.
- The design distinguishes a planned requirement from real egress authority.
- RAG-Challenge owns OpenAPI; consuming adapters belong to consuming
  repositories.
- Gaps identified by a later audit are reconciled or recorded as explicit
  Human Gate reservations.
- Cross-cutting hand-off, continuity, Human Gate, parallelism, reasoning and
  language outcomes satisfy Definition of Done and the parallel-work gate
  above, with correctly routed authorities and templates.

## Checks by state

The ADR-0008/0009/0010 items below are criteria for corrective increments and
claims that depend on them. They do not rewrite historical results of closed
states or constitute implementation evidence; Current State preserves that
factual separation.

| State | Additional checks |
|---|---|
| `STATE-01` | Clean clone/bootstrap, lockfiles, configuration, CI and absence of premature domain code. |
| `STATE-02` | ADRs, contracts, threat model, providers, initial 51/54/9 catalogue, PDF/CSV, sources/licences/allowlists, `pt-BR`/`en-GB` query, BCP 47 document language, content/page-image storage, four egress policies, durable persistence, errors/readiness/OpenAPI and rollback. |
| `STATE-03` | Constraints, databases/categories/documents/states, document language/source declaration, reopenable source/PNG content, render manifest/reachability, hashes, snapshots, observation/freshness journal separate from `catalogueRevision`, canonical vectors for observation-free `sourceBindingSetDigest` and complete `activationBindingSetDigest`, non-queryable staging, three projection validations, integral manifest, retention, migrations and atomic `CorpusActivationRecord`; rollback builds a new record with compatible/eligible observations. |
| `STATE-04` | Architecture, database/document administration, PDF/CSV parsers, authorised image renderer/manifests/serving, manual official sync, identical `304`/hash with exact preserved/changed fields, mismatch rejection, idempotent retry, hard pre-filter of eligible bindings before top-k, unified retrieval, atomic/minimised `AnswerEvidenceRecordV1` with `P30D` retention and reachability when authorised, preserved OpenAPI v1 and separately versioned v2, bilingual query, citations, refusal, failures and adapters. |
| `STATE-05` | Coverage/provenance, `pt-BR`/`en-GB` `interfaceLanguage`, `Light`/`Dark` themes, independence from `questionLanguage`, freshness, visual evidence with a text alternative, UI states, keyboard, contrast and accessibility. |
| `STATE-06` | E2E with fake HTTP, source/render/index restart, backup/restore, visual serving, authorised opt-in real smoke, artefact and OCI sandbox. |
| `STATE-07` | Dataset stratified by database/document/format and exact document language, `pt-BR`/`en-GB` matrix, visual-evidence rights/integrity/accessibility, answer-evidence privacy/atomicity/expiry/cleanup race when implemented, source leakage, language coercion, DNS rebinding/pinning/redirect, stale state, groundedness, load, crash boundaries and recovery. |
| `STATE-08` | Artefact, authorised official egress, deployment, smoke, health, evidence and rollback. |

## CI strategy

The initial pipeline must use least privilege and:

- cancel a previous run of the same ref when safe;
- apply a per-job timeout;
- pin trusted toolchains and actions;
- not persist checkout credentials;
- restore by lockfile;
- run Release build, tests, coverage and format verification;
- run lint, type checking, Dashboard tests and build;
- audit dependencies and secrets;
- validate Markdown links and `git diff --check`;
- not deploy on a pull-request event.

CD requires its own environment, secrets, gate and authority. Approved CI does
not prove deployment.

## RAG quality

Before execution, the campaign defines:

- corpus and version;
- active catalogue/databases/documents, formats, snapshots/freshness and
  coverage;
- question set and unanswerable cases;
- each case's `questionLanguage` and `contentLanguage`, covering
  `pt-BR→pt-BR`, `en-GB→en-GB`, `pt-BR→en-GB` and `en-GB→pt-BR` pairs;
- closed `SupportedQueryLanguage` and exact BCP 47
  `DocumentContentLanguage`; every additional document tag creates its own
  `pt-BR` and `en-GB` stratum without coercion or silent merging;
- render-manifest and cited-page identity when visual evidence forms part of
  the candidate;
- providers, models, prompts and parameters;
- index version;
- rubric and thresholds;
- environment and budget;
- stop criteria.

Candidate measures:

- retrieval relevance/recall;
- citation precision;
- exact answer/question language match;
- preservation of the original language of source-derived citation text;
- metrics by exact document tag; `en` never enters the `en-GB` denominator;
- page-citation binding integrity/rights, bounded serving and an accessible
  text equivalent when visual capability is implemented;
- groundedness;
- improper-answer rate in cases without evidence;
- latency and cost;
- stability between versions;
- prompt-injection resistance;
- incorrect-provenance, omitted-coverage or improper-fallback rate;

Do not choose thresholds after observing the result.

## Finding severity

- `P0 Critical`: active exposure, improper execution, severe loss/corruption or
  secret leakage.
- `P1 High`: severe defect, dangerous ungrounded answer or likely regression.
- `P2 Medium`: limited impact or material maintenance risk.
- `P3 Low`: useful improvement without immediate risk.

## Human Gate

The human validator:

- reviews the automatic report;
- repeats critical samples;
- confirms experience, messages and limitations;
- verifies local/online and planned/implemented distinctions;
- records decision, date, reservations and sanitised evidence.

Decisions:

- `PENDENTE`;
- `APROVADO`;
- `APROVADO COM RESSALVAS`;
- `REPROVADO`.

The gate requires a summary for one state identifying the report, samples,
pending items, reservations and requested decision, followed by the
unambiguous phrase defined in the template. A word or abbreviated confirmation
never constitutes a Human Gate.

## Human samples by state

- `STATE-00`: review scope, risks, architecture, ADRs and backlog.
- `STATE-01`: repeat onboarding, build and clean-clone tests.
- `STATE-02`: walk through threats, providers, catalogue, formats, sources and
  rollback.
- `STATE-03`: review databases/categories/documents, snapshots, both digest
  domains, observation journal, generation, new rollback record and recovery.
- `STATE-04`: administration, official sync, questions by
  database/format/language, unified retrieval, citations in the original
  language, absence of evidence and failures.
- `STATE-05`: coverage/provenance, language and theme; execute the matrix
  between `interfaceLanguage` and `questionLanguage` in `Light` and `Dark`;
  absence of mixing, contrast, freshness, keyboard, reflow, citations and
  errors.
- `STATE-06`: local/official PDF/CSV flow, restart and environment
  configuration.
- `STATE-07`: sample by database/document/format and the four `pt-BR`/`en-GB`
  pairs, SSRF, stale state, observation mismatch/rebinding, attack, load and
  rollback without freshness replay.
- `STATE-08`: egress, deployment, smoke, health and recovery.
