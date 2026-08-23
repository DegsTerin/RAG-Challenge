<!-- Purpose: Records the executable audit and correction plan for the current owner request without granting lifecycle, provider, deployment or publication authority. -->

# RAG-Challenge audit and correction plan

## Control record

| Field | Value |
|---|---|
| Plan ID | `PLAN-AUDIT-CORRECTION-20260821-01` |
| Status | `DONE_WITH_BLOCKERS` |
| Created | `2026-08-21T02:20:27Z` |
| Baseline | `main@a25921d952b8f68498235c7449449da7f4e73238`; tree `d46b659ff09fdd48ccdaf2c90f37b23482c5f02d` |
| Git state | `main` was 12 commits ahead of `origin/main`; only `?? Plans.md` existed |
| Preserved owner work | The original request was read and fingerprinted as SHA-256 `e4954b14ebbc830dfdd391e2b48fc4ac86db01c46db1e1056bac692f68a589b7`; this executable plan implements its intent |
| Lifecycle | `STATE-07 TESTING_HOMOLOGATION`; unchanged |
| Prompt corpus | `4.19.4` |
| Runtime preflight | No RAG-Challenge-owned process or known task listener was found |
| Parallel work | `PARALLEL_RECOMMENDED` for three read-only audit lanes; all writes remain sequential in the shared worktree |
| Delivery | No commit, push, publication, deployment or lifecycle transition |

## Objective and completion rule

Audit the complete repository, preserve the starting baseline, correct safe and
authorised root causes incrementally, add regression tests, and leave
reproducible evidence.

This request is complete only when:

1. every critical and high finding is corrected or explicitly blocked by a
   named missing authority, external datum or human action;
2. essential credential-free local flows pass end to end;
3. locked restore, format, Release build, tests, coverage, lint, type checking,
   dependency audits, repository audit and diff hygiene pass where applicable;
4. every changed executable boundary has focused regression coverage;
5. documentation states observed implementation and unexecuted limitations;
6. no supplemental result is represented as a canonical gate.

This plan cannot approve a Human Gate, accept risk, change lifecycle, arm a
provider budget, call a paid provider, publish, deploy, or implement the
accepted `pdf-render-sandbox-v1` without its separate platform authority.

## Authority and negative scope

The current owner request authorises local inspection, online package metadata,
task-owned restore/build/test outputs, disposable local stores,
credential-free start-up, and safe local corrections.

The following remain outside scope:

- credentials, provider calls, non-zero budget, billing or external content;
- Render, OCI, GHCR or GitHub mutation;
- production publication, deployment, release, push or merge;
- destructive operations against owner or shared data;
- public-contract, schema or migration changes without governing review;
- real product/RAG adjudication, Human Gate or `STATE-08`.

## Audited system

The only applicable repository instruction file is `AGENTS.md`; no nested
`AGENTS.md` exists. The routed governance, current state, lifecycle,
architecture, security, quality, language, templates, README files,
configuration, scripts, source and tests were inspected. The baseline has 433
tracked files.

| Boundary | Observed responsibility | Dependencies |
|---|---|---|
| `RagChallenge.Domain` | Stable entities, value objects and outcomes | Framework only |
| `RagChallenge.Application` | Use cases and inward-owned ports | Domain |
| `RagChallenge.Infrastructure` | PDF/CSV, SQLite control/vector stores, official-source and provider adapters | Application, Domain, CsvHelper, EF Core/SQLite, PDFtoImage, PdfPig |
| `RagChallenge.Server.Api` | Composition, public v1/v2 API, local administrative CLI, health and Dashboard host | Application, Infrastructure |
| `RagChallenge.Dashboard.Web` | React/TypeScript public query UI | React 18, Vite |
| `tools/ai-orchestrator` | Isolated development-only deterministic coordinator | Node 24, TypeScript, exact Codex SDK graph |
| Tests | Unit, architecture and composed integration coverage | xUnit, test SDK, coverlet |

### Data, API, authority and integration map

| Area | Observed implementation and boundary |
|---|---|
| Control data | `control.db` contains catalogue, document/source versions, activation, administration journal/lease, response evidence, maintenance/recovery records and the provider-budget ledger. |
| Retrieval data | `vectors.db` has independent migrations and vector-generation state. Immutable source and derived bytes are addressed by hash in the filesystem content store. |
| SQLite posture | Foreign keys, `synchronous=FULL`, `trusted_schema=OFF`, busy timeout and WAL are configured. Control and vector migrations are independent. |
| Public API | Anonymous `GET /api/v1/health/live`, `GET /api/v1/health/ready`, `POST /api/v1/questions`, `POST /api/v2/questions` and the identity-bound v2 page-image route. Frozen contracts are `docs/api/openapi-v1.json` and `openapi-v2.json`. |
| Abuse controls | Body/question bounds, language/normalisation validation, deadlines, concurrency gates and rate limits protect the anonymous surface. There is no user authentication or RBAC in the current host. |
| Administration | No administrative HTTP route exists. Explicitly enabled local one-shot CLI commands use opaque operating-system identity, operation/corpus identifiers, reason digest, durable journal and per-corpus lease. |
| Provider authority | Typed configuration holds only an environment-variable name. Exact operational grants and durable budget admission precede credential lookup and HTTP. The current Product composition has no authorised debit path and is fail-closed. |
| OpenAI | Direct bounded HTTPS adapters behind Application ports use the exact `https://api.openai.com/` authority and no implicit SDK retry. No real call was made. |
| Official sources | Exact HTTPS authority, DNS resolution/pinning, special-network rejection, local TLS validation, no general link following, bounded redirects and response limits preserve provenance. No online document source was accessed. |
| Parsing and rendering | PDF/CSV adapters use PdfPig, PDFtoImage and CsvHelper; deterministic chunking is Application-owned. Product rendering is disabled until the accepted sandbox has separate implementation/attestation. |
| Future only | User authentication/RBAC, remote administration, multiple active corpora, scheduled sync, generic crawling, extra formats, dynamic providers and distributed/alternative vector stores are not current implementation. |

### Essential functional flows

1. Administration: local CLI validation and identity -> journal/lease ->
   catalogue/source/import/build/activate/rollback/status command -> canonical
   JSON result.
2. Ingestion and indexing: authorised local bytes or official snapshot ->
   immutable content store and readback -> PDF/CSV parsing -> deterministic
   chunking -> admitted embedding -> candidate vector generation -> validated
   atomic activation with rollback target.
3. Query: public validation/rate/concurrency -> selected runtime -> activation
   snapshot -> eligibility filtering before top-k -> retrieval -> admitted
   embedding/generation -> cited answer or explicit insufficient-evidence
   outcome; v2 may persist bounded answer evidence.
4. Visual evidence: answer binding, render manifest, page and content identity
   are verified before a persisted PNG is served. Product on-demand rendering
   remains unavailable.
5. Recovery: bounded manifest and hashes -> isolated SQLite integrity and
   foreign-key checks -> journal/lease-controlled activation. A real
   representative restore and crash-window proof remain unexecuted.

### Build, test and deployment map

The .NET 10 solution has unit, architecture and composed integration suites.
The separate React/Vite Dashboard provides lint, typecheck, test and production
build scripts. The development-only Node orchestrator has its own exact graph,
checks and tests. `eng/ci.ps1` aggregates locked restore, format, Release build,
.NET tests/coverage, both npm workspaces, repository policy and online
dependency audits; offline registry checks are explicitly `NOT_RUN`. The
Windows GitHub workflow validates only and does not publish.

The Render Free scripts build and verify a private, non-deploying context with
an unprivileged runtime, verified seed and ephemeral marked store. OCI rehearsal
is a local artefact only. Docker execution, a current external Render check,
image publication, OCI provisioning and production deployment were not
authorised or executed; historical documentation is not current external-state
proof.

Production dependencies point inward. Administration is a local CLI mode, not
HTTP. The anonymous HTTP surface is liveness, readiness, v1/v2 questions, v2
visual evidence and static UI. SQLite stores control/vector state; content is
immutable and filesystem-backed. Generation activation is atomic and bounded
recovery is implemented.

Official-source and provider adapters have strong exact-authority, bounded
response, no-redirect/proxy and fail-closed controls. Product provider
admission demands the exact trusted grant but then stops with an unavailable
outcome before credential access or HTTP because this composition has no
authorised durable debit implementation. A persisted zero-value `Armed`
envelope is sanitised to `Disarmed`. PDF text parsing remains in-process. The
renderer child process has Job Object/`rlimit` controls but is not ADR-0019's
attested sandbox.

## Initial validation evidence

All evidence was observed on 2026-08-21 UTC.

| Command or check | Result |
|---|---|
| Toolchain | PowerShell 7.6.5; Git 2.55.0.windows.3; .NET SDK 10.0.303; Node 24.19.0; npm 11.17.0 |
| `./eng/ci.ps1` | 105/105 language tests passed, then the canonical gate stopped because protected baseline changes require exceptional manual review |
| CI/coverage policy tests | Coverage policy 11/11; CI policy exit 0 |
| `dotnet restore ... --locked-mode` | Seven projects restored |
| `dotnet format ... --verify-no-changes` | Passed |
| Release build | Passed with zero warnings and errors |
| Release tests | 227 unit + 11 architecture + 317 integration = 555 passed; zero failed/skipped |
| Coverage | 95.75% lines, 67.03% branches; 70%/45% floors passed |
| Dashboard checks | Clean install, lint, typecheck, 45/45 tests and Vite build passed |
| Dashboard audit | Failed: high `nanoid <3.3.18`; installed 3.3.16 via PostCSS/Vite |
| Dashboard signatures | 21 registry signatures and nine attestations verified |
| Orchestrator checks | Lint/type/build passed; 105/107 tests passed, two Windows symlink-permission skips, zero failures; 82.12% lines and 76.71% branches |
| Orchestrator audit/signatures | Zero vulnerabilities; five registry signatures and three attestations verified |
| NuGet vulnerability audit | No vulnerable package reported in the exact seven-project set; structured JSON parser and real no-restore output both passed |
| NuGet deprecation audit | xUnit 2.9.3 and its v2 graph are legacy in all three test projects |
| Outdated inventory | Compatible EF/PDF/test patches and breaking React/TypeScript/xUnit/Codex SDK versions exist; no blind update is authorised |
| EF disposable migrations | 12 control + one vector migration applied; both contexts had no pending model change |
| Integrated artefact build | 92 files, 625,890,778 bytes, SHA-256 `f3c043f3f00659da0106923996f9ee2075084d5867cc10529aefd1cae31ea8c8` |
| Integrated start-up | Dashboard, v1/v2 `en-GB`/`pt-BR`, PNG/304, visual bounds/rate limit, restart and cold recovery passed on port 5186 |
| Repository audit | Initially failed only because the original plan had CRLF and no final newline; this file corrects that owned defect |

EF warned that selected SQLite `PRAGMA foreign_keys=0` operations cannot be
fully transactional and that `AddNoticeBearingObligationSchema` combines SQL
with a pending table rebuild. Disposable execution passed; crash atomicity and
a representative existing database remain unproven.

## Finding register

### Critical

No critical finding was confirmed.

### High

| ID | Finding | State and required disposition |
|---|---|---|
| `H-SEC-PDF-ADMIN-001` | Product administration could invoke the incomplete renderer with an untrusted PDF | `CORRECTED` by containment: Product administration is text-first, requires rendering disabled, composes no renderer ports and returns the stable unavailable outcome; full sandbox remains separate |
| `H-READY-BUDGET-001` | Readiness said `Ready`/`Configured` while durable budget was zero/`Disarmed`, and a synthetic zero-value `Armed` envelope could reach provider composition | `CORRECTED`: exact grants and both local envelopes are evaluated without credentials/HTTP; absent, unreadable, non-armed or zero-value authority returns sanitised `Disarmed`/closed state and HTTP 503; even an exact grant stops before credential or HTTP access |
| `H-READY-LOAD-001` | Anonymous readiness rehashed full content and exact-scanned all eligible vectors on every request | `CORRECTED` for the authorised zero/`Disarmed` baseline: budget now short-circuits before CAS/vector work; a separately authorised armed mode retains the conditional medium limitation below |
| `H-SCRIPT-RENDER-001` | Render entrypoint executed `rm -rf` on an environment-controlled path | `CORRECTED`: seed/runtime paths are canonical, unsafe overrides and symlink/foreign stores fail before mutation, and only an exactly marked leaf can be replaced |
| `H-SCRIPT-RENDER-BUILDER-001` | Render package builder accepted an arbitrary repository descendant and recursively replaced it | `CORRECTED`: the builder accepts only the canonical marked output, rejects reparse points and foreign/malformed trees, and verifies offline administration plus fail-closed public readiness before packaging |
| `H-SCRIPT-ARTEFACT-001` | Integration and OCI rehearsal builders/verifiers could recursively replace an arbitrary `artifacts-local` descendant, including a supplied runtime child | `CORRECTED`: all three roots use exact canonical paths, bounded ownership markers and whole-tree reparse checks; verifiers remove only their exact nested marker-owned runtime |
| `H-INTEGRATION-ARCHIVE-001` | The integration verifier extracted and executed an adjacent ZIP without a trusted provenance anchor, resource bounds or internal-manifest verification | `CORRECTED`: a coordinator-supplied SHA-256 is mandatory; the exact sidecar and streaming archive digest, entry/path/link/count/size/ratio bounds and complete internal hashes are checked before task runtime mutation; extraction uses exclusive files and rechecks bytes before any process start |
| `H-SCRIPT-001` | Oracle plan generator accepted arbitrary output and overwrote 52 known names | `CORRECTED`: output is derived from a bounded task ID, remains under `artifacts-local`, uses an ownership marker and `CreateNew`, and refuses existing/reparse/foreign targets |
| `H-NPM-001` | Dashboard graph contained vulnerable `nanoid` 3.3.16 | `CORRECTED`: lockfile resolves 3.3.18; clean audit, signatures, lint, typecheck, 45 tests and build passed |
| `H-HOMOLOGATION-001` | Real provider-backed product/RAG homologation is not executed | Blocked with justification: separate authority, valid frozen inputs, two independent human reviews and genuine adjudication are required |
| `H-GATE-LANGUAGE-001` | Protected language-control integration required exceptional manual review and one canonical offline execution | `RESOLVED` for sealed candidate `e80d10a29738fa7a042286c549687d08b2fe1dea`: the two bounded manual reviews returned zero P0-P3 and the single corrected offline gate exited `0`; the candidate and factual evidence were fast-forwarded into `main` at `a6ff32038510c552d9568786091a993a9be01117`; every earlier failure remains historical evidence |

### Medium

| ID | Finding and disposition |
|---|---|
| `M-SQLITE-CS-001` | `CORRECTED`: all six production SQLite openings use `SqliteConnectionStringBuilder` with exact full paths, explicit mode, private cache and explicit pooling; delimiter-bearing EF, backup and verification paths are regression-tested |
| `M-RECOVERY-MANIFEST-001` | `CORRECTED`: recovery manifests reject duplicate properties, excessive bytes/tokens/depth/cardinality, per-file reparse points and changed lengths before hashing; validated database and authority paths are rechecked immediately before SQLite opens |
| `M-RECOVERY-MEMORY-001` | A compatible manifest may still be loaded as one byte array and then materialised as a typed object graph up to the historical 256 MiB/1,000,002-entry limit; replace this with a streaming typed reader in separately bounded compatibility work |
| `M-PDF-PARSER-001` | PdfPig parses hostile input in-process with non-cancellable calls; retain fail-closed bounds and design a separately contained parser |
| `M-SEC-001` | `CORRECTED`: NuGet audit uses bounded JSON v1, rejects duplicate/malformed/problem/incomplete output, requires exact project coverage and fails on any vulnerability |
| `M-CI-ACTION-001` | `CORRECTED`: all Actions use verified full commit identities and Node is fixed to 24.19.0; policy rejects mutable Action or Node references |
| `M-CI-SCAN-001` | Secret scan is a narrow assignment regex; add a fixed dedicated scanner for worktree/history without printing matches |
| `M-CI-NPM-001` | `CORRECTED`: canonical online CI audits both npm graphs at high severity; offline CI records both as `NOT_RUN` |
| `M-RATE-001` | Rate-limit identity behind Render proxy is unverified; trusted proxy topology is required before accepting forwarded headers |
| `M-GRANT-BOUNDARY-001` | Requested authorities and trusted grants share ordinary configuration; separate trusted capability before any non-zero budget |
| `M-READY-ARMED-LOAD-001` | Conditional future risk: if both envelopes become `Armed` under separate authority, deep CAS/vector verification remains uncached and needs explicit freshness/integrity semantics before that mode is supported |
| `M-VECTOR-SCALE-001` | Eligibility builds a linear OR predicate; validate above 1,000 bindings before product-scale claims |
| `M-MIGRATION-ATOMICITY-001` | EF warnings identify partial-migration crash windows; prove recovery on a representative copy before real migration |
| `M-DOC-STATE-001` | `CORRECTED`: the documentation index now reports corpus 4.18.6 and records the completed integration, independent reviews and corrected offline gate without claiming product homologation |
| `M-RENDER-PROVENANCE-001` | `CORRECTED`: the generated Render package and verifier now bind `source.corpus` to current corpus 4.18.6; the action manifest describes submitted/configured actions and explicitly records that OS-level egress observation was not performed |
| `M-RENDER-STORE-LINK-001` | `CORRECTED`: the complete Product source-store tree is checked for reparse points before output mutation and again immediately before copy; the copied seed is rechecked before hashing |
| `M-OCI-ARCHIVE-001` | `CORRECTED` in code: the OCI verifier now requires a coordinator-held digest, keeps one archive handle from hash through inspection, and bounds archive bytes, entry count/size, total expansion, compression ratio, manifest and scanned text while rejecting link entries. Static regressions passed; real replay is `NOT_RUN` because the preserved historical output is unmarked and a current rebuild requires a clean source tree. |
| `M-DOC-DEPLOY-001` | `CORRECTED`: public and Render package documentation now reflects ADR-0020's concurrent isolated roles, historical evidence date and undeployed OCI target |
| `M-A11Y-EVIDENCE-001` | `PARTIALLY CORRECTED`: current v2 local browser evidence confirms labelled controls, coherent headings/landmarks, result-focus transfer, `pt-BR`/`en-GB` document language, citation rendering, no console errors and no horizontal overflow at 390x844; deployed browser and screen-reader evidence remains incomplete |
| `M-XUNIT-LEGACY-001` | xUnit v2 is deprecated; migrate to v3 through isolated compatibility work |
| `M-IPV6-EGRESS-001` | Special-use IPv6 rejection needs exhaustive table-driven verification before external-source production use |

### Low

| ID | Finding and disposition |
|---|---|
| `L-HTTP-HARDEN-001` | Global `frame-ancestors`, Referrer-Policy, Permissions-Policy and trusted HTTPS/HSTS policy are incomplete |
| `L-ALLOWED-HOSTS-001` | `AllowedHosts` defaults to `*`; deployment hostname is not fixed locally |
| `L-OUTPUT-TOCTOU-001` | Canonical marker/reparse checks substantially contain deletion, but PowerShell path traversal and recursive removal are not handle-atomic against a concurrent same-user filesystem attacker; retain isolated task ownership and move privileged execution to a stronger filesystem boundary if needed |
| `L-CSS-TOKEN-001` | `CORRECTED`: `--font-display` is defined at the Dashboard root with the existing serif display stack |
| `L-API-DUPLICATION-001` | v1/v2 endpoint mapping is duplicated; defer unless contract identities remain byte-stable |
| `L-DEPENDENCY-DRIFT-001` | No audited vulnerability remains, but updates exist: EF/SQLite 10.0.10 -> 10.0.11, PDFtoImage 5.3 -> 5.4, test SDK 18.7 -> 18.9, xUnit runner 3.1.5 -> 4, Vite 8.1.4 -> 8.2.2, React 18 -> 19, TypeScript 5.7 -> 7, Codex 0.147 -> 0.149 and Node types 24 -> 26. Apply patches/majors only through isolated compatibility and supply-chain review. |

No production `TODO`, `FIXME`, `HACK`, `XXX` or
`NotImplementedException` was found. No unauthorised administrative HTTP API,
obvious secret value or Domain/Application inversion was observed.

## Incremental execution plan

Only one write increment is active at a time. Each increment starts with diff
inspection, changes only its owned files, runs focused regressions, then runs
applicable lint/type/build checks.

### `A-01` — Baseline and deep audit

- Status: `COMPLETED`.
- Acceptance: authority, Git, architecture, data/API/security flows, three
  independent reviews and initial validation matrix are recorded.

### `B-01` — Remove the Dashboard high vulnerability

- Status: `COMPLETED`.
- Files: Dashboard lockfile only unless npm proves a manifest change necessary.
- Work: resolve `nanoid >=3.3.18` within the existing Vite/PostCSS range.
- Tests: clean install, audit, signatures, lint, typecheck, 45 tests, build.

### `B-02` — Contain task-owned script output

- Status: `COMPLETED`.
- Files: Oracle plan generator, focused policy test and CI invocation.
- Work: require output below `artifacts-local`, ownership marker, no reparse
  point/foreign content, and non-overwriting creation.
- Tests: safe generation yields exactly 52 deterministic plans; repository
  root, outside root, foreign directory, existing plan and reparse point fail
  before mutation; dangling links are detected regardless of cleanup order.

### `B-03` — Contain Render runtime-store recreation

- Status: `COMPLETED`.
- Files: entrypoint and package verifier/regression fixtures.
- Work: fixed task-owned parent/exact leaf; reject unsafe override, symlink and
  unowned existing target; replace only marker-owned content.
- Tests: canonical create/restart, one `/app` override, unmarked/corrupt store
  and invalid seed fail closed in the available POSIX shell; static checks
  enforce the remaining path and reparse-point boundaries.

### `B-04` — Fail closed the administrative renderer

- Status: `COMPLETED` for containment only.
- Files: Product administrative composition and focused tests.
- Work: prevent current Server.Api renderer construction/invocation while
  preserving text-first materialisation and verified persisted-image serving.
- Tests: enabled rendering fails before process/PDF access; text-first
  administration and Product query remain unchanged.

### `B-05` — Make readiness bounded and truthful

- Status: `COMPLETED` for the current zero-budget baseline.
- Files: Product budget/readiness composition and focused integration tests.
- Work: validate cheap authority, read sanitised local budget state and return
  `Unready`/503 for `Disarmed` before CAS hashing/vector scan. Do not read
  credentials, call HTTP, rearm or expose budget details. Existing DTO/OpenAPI
  remains unchanged.
- Tests: absent/mismatched/expired/grantless budget states fail closed; the
  disarmed path proves no deep store verification; persisted profile corruption
  still wins; a zero-value `Armed` envelope is sanitised; an exact grant still
  produces zero credential reads and zero HTTP calls; HTTP mapping remains 503
  and the payload is sanitised.
- Future armed-mode stop condition: no cache/vector-health redesign without a
  separate decision that defines freshness and integrity semantics.

### `B-06` — Contain generated deployment and integration artefacts

- Status: `COMPLETED`.
- Files: shared owned-output policy, Render output specialisation, Render
  builder/verifier, integration and OCI rehearsal builders/verifiers, focused
  script and static integration tests.
- Work: accept only fixed canonical output roots, create bounded markers
  exclusively, reject unsafe/reparse/foreign trees before mutation, and make
  disposable runtime deletion subordinate to its own exact marker. Integration
  and OCI verification additionally require a coordinator-held archive digest;
  integration payloads are bounded and fully hashed before extraction/execution.
- Tests: fresh and owned replay succeed; foreign, corrupt, traversal, outside,
  root, sibling, file-leaf and unsafe-child cases preserve the target and fail;
  three link cases are explicit host-permission skips. Eleven archive fixtures cover
  trusted extraction, tampering, forged sidecars/manifests, rebuilt payload,
  traversal, duplicate/link entries, entry count and compression ratio. The
  rebuilt integration artefact passed authenticated start, query, visual,
  rate-limit, restart and cold recovery.
- Render-specific result: offline administrative status and the Product public
  `Live`/`Unready`/`provider-budget: Disarmed` contract passed on a disposable
  staged-store copy. The complete builder was not run because its clean-`main`
  invariant correctly rejects this dirty audit worktree and the existing source
  store has owner data sidecars that were not mutated.

### `C-01` — Strengthen dependency policy

- Status: `COMPLETED` for dependency-audit and immutable-workflow scope.
- Work: audit orchestrator online in canonical CI; parse NuGet audit JSON
  fail-closed; add clean/vulnerable/malformed/tool-failure fixtures.
- Completed extension: exact upstream Action commits and Node patch are pinned
  and enforced. A dedicated secret scanner remains the separate
  `M-CI-SCAN-001` supply-chain selection rather than an inferred dependency.

### `C-02` — Harden SQLite recovery

- Status: `COMPLETED`.
- Work: connection-string builders, bounded manifest parsing and hostile tests.
- Acceptance: delimiter-bearing validated paths keep physical identity; large,
  deep, duplicate-property or high-cardinality manifests fail before typed
  materialisation; per-file links and post-length-change hashes fail closed.
- Result: production connection strings are structured and preserve exact
  physical identity. Manifest verification performs bounded byte loading and a
  `Utf8JsonReader` preflight before typed deserialisation while preserving the
  historical one-million-content-object ceiling. The remaining full-buffer and
  typed-object-graph memory cost is tracked as `M-RECOVERY-MEMORY-001` rather
  than represented as non-allocating end to end.

### `C-03` — Reconcile documentation and small defects

- Status: `COMPLETED`.
- Work: README corpus/state and ADR-0020 wording; CSS token; explicit provider,
  proxy, browser/AT, real-database, Linux ARM64 and OCI limitations.
- Result: the public, deployment and documentation indexes now distinguish
  dated Render evidence, Docker-context generation, ADR-0020's isolated dual
  roles and all listed unexecuted verification boundaries. Generated package
  provenance now uses corpus 4.18.6 rather than historical 4.10.40 and does not
  present uninstrumented egress as observed evidence. The Dashboard's previously
  undefined display-font token now uses its established serif stack.

### `D-01` — Final verification and independent review

- Status: `COMPLETED_WITH_BLOCKERS`.
- Run focused tests after each change, then locked restore, format, Release
  build, all .NET tests/coverage, both npm workspaces' checks/audits/signatures,
  disposable migrations, integrated start/restart/cold recovery, repository
  audit, `git diff --check`, final Git status/diff and independent review.
- The first protected-language execution passed its language tests and stopped
  at the exceptional-review boundary. The integrated candidate then preserved
  its configuration, identifier and focused checker failures before the two
  bounded manual reviews and the single corrected offline gate approved sealed
  candidate `e80d10a29738fa7a042286c549687d08b2fe1dea`.
- Result: every safe local increment and final full-suite repetition passed.
  Independent security re-review found zero critical or high local findings.
  The protected language gate is resolved only for the sealed candidate. Real
  provider homologation remains the justified high external blocker; no
  lifecycle state or external system changed.

## Final local verification evidence

The earlier results below were observed on 2026-08-21 UTC after the executable
changes and retain their original disposition. The later protected-language
entry records the separately sealed and approved integrated candidate without
reclassifying any preceding result.

| Command or check | Observed result |
|---|---|
| Runtime preflight | Zero RAG-Challenge-owned server processes and zero listeners on the two task ports before final validation |
| Locked .NET restore | Seven projects restored; exit 0 |
| `dotnet format ... --verify-no-changes` | Exit 0 |
| Release build | Exit 0; zero warnings and zero errors |
| Full .NET tests | 227 unit + 11 architecture + 333 integration = 571 passed; zero failed/skipped |
| Merged .NET coverage | 95.74% lines (61,875/64,627) and 67.30% branches (5,981/8,887); floors passed |
| Focused corrected-boundary tests | 53/53 Product budget/readiness, recovery, Render and generated-artefact tests passed |
| Script policy suites | Render output, Render entrypoint, Oracle generator, CI, NuGet audit and trusted integration-archive policies passed; three Render-output and one Oracle link cases were explicit host-permission skips |
| Dashboard | Clean install, lint, typecheck, 45/45 tests, Vite production build, zero high vulnerabilities, 21 signatures and nine attestations passed |
| Orchestrator | Clean install, lint, typecheck, build, 105/107 tests and coverage passed; two link tests skipped because this host denies file-symlink creation; zero vulnerabilities, five signatures and three attestations |
| NuGet audit | Structured exact seven-project online audit passed with no reported vulnerability; ten hostile parser fixtures passed |
| Disposable EF migration | 12 control and one vector migration applied; both pending-model checks returned no change; known transactional/rebuild warnings remain |
| Rebuilt integrated artefact | 92 files, 625,898,927 bytes, SHA-256 `95a545a5bbaac677d40207d816ae9799777c14bbb75f77c74e3e22372051e824` |
| Integrated start/restart/recovery | The trusted SHA-256 `95a545a5bbaac677d40207d816ae9799777c14bbb75f77c74e3e22372051e824` was supplied independently to the hardened verifier; Dashboard, v1/v2 `en-GB`/`pt-BR`, PNG/conditional response, bounds/rate limit, restart and cold recovery passed on task port 5186 |
| Browser inspection | Local v2 query produced a grounded answer, coverage and cited visual evidence; interface switch set document language to `en-GB`; 390x844 had no horizontal overflow; console had no warning/error |
| Render offline contract | Disposable staged-store status returned `CH_ADMIN_STATUS_AVAILABLE`; Product public host, started without a configured credential or trusted grant, returned liveness 200/`Live` and readiness 503/`Unready` with one sanitised `provider-budget: Disarmed` and zero active counts. Separate focused Product tests observed zero credential-reader and HTTP-handler calls; OS-level egress was not independently observed. |
| Full Render package builder | `NOT_RUN`: an explicit attempt stopped before mutation because the audit worktree is not a clean `main`; the legacy unmarked output and owner store sidecars were preserved |
| OCI verifier replay | `NOT_RUN`: the preserved historical output predates the required ownership marker; current builder/verifier code and static regressions passed, but no legacy artefact was adopted or replaced |
| Repository hygiene | Repository audit passed for 443 non-ignored files; 18 changed PowerShell scripts parsed; `git diff --check` passed |
| Canonical `eng/ci.ps1` | 105/105 language-policy tests passed, then exit 1 at the intentional exceptional manual-review requirement; no later canonical stage ran |
| Integrated protected-language gate | Two bounded `MANUAL_REVIEW_PASS` dispositions returned zero P0-P3. The single corrected offline gate for sealed candidate `e80d10a29738fa7a042286c549687d08b2fe1dea` exited `0`: 107 language-policy tests, 445-file checker, Release build, 571 .NET tests, Dashboard checks and 105 orchestrator tests passed; two environment-bound symlink cases were skipped and online dependency audits remained `NOT_RUN` in offline mode |
| Independent final review | Security re-review confirmed zero critical/high local finding after archive hardening; documentation re-review approved delivery as `DONE_WITH_BLOCKERS` after status, corpus provenance and evidence wording were corrected |

No Docker daemon, paid/provider request, online documentation source, current
Render/OCI environment, publication or production system was exercised.

## Objective definition of done

Local correction work is `DONE_WITH_BLOCKERS` when all safe increments pass
and the remaining items are limited to explicitly external/human-controlled
work:

- real provider/RAG homologation and successor RB authority;
- full `pdf-render-sandbox-v1` implementation/attestation;
- trusted Render proxy/edge and real deployment evidence;
- representative real-database crash migration;
- final deployed browser/assistive-technology evidence;
- OCI selection, provisioning, secret integration and `STATE-08`.

The overall project must not be labelled complete while a high blocker remains.
A blocker is not risk acceptance and is not silently downgraded.

Disposition: the local correction work achieved `DONE_WITH_BLOCKERS`. The
overall project remains incomplete because `H-HOMOLOGATION-001` requires
valid successor inputs, real provider evidence and genuine human review and
adjudication. The sealed candidate and its factual evidence are integrated in
`main`, so `H-GATE-LANGUAGE-001` no longer blocks that baseline. No critical or
uncorrected high local finding remains.

## Change log

| UTC instant | Change | Validation |
|---|---|---|
| `2026-08-21T02:20:27Z` | Converted the owner request into this executable, prioritised plan after the initial audit | UTF-8/LF/final-newline and repository audit pending immediate verification |
| `2026-08-21T02:25:00Z` | Updated only the Dashboard lockfile from `nanoid` 3.3.16 to compatible 3.3.18 | Clean install; zero-vulnerability audit; signatures; lint; typecheck; 45/45 tests; production build passed |
| `2026-08-21T02:32:00Z` | Replaced arbitrary Oracle plan output with a derived owned-task namespace, exclusive marker/files and safe-path checks; added a canonical policy test | Generator policy passed all executable cases; reparse creation was explicitly unavailable; CI policy and focused integration test passed; repository audit/diff hygiene passed |
| `2026-08-21T02:36:51Z` | Fixed the Render entrypoint to accept only its canonical store, validate an exact ownership marker and seed before deletion, and use private creation permissions | Git-sh syntax and seven behavioural cases passed; CI policy plus five Render package integration tests passed; repository audit/diff hygiene passed |
| `2026-08-21T02:40:23Z` | Removed renderer construction and ports from the Product administrative profile, requiring rendering to remain disabled while keeping text-first ports available | Focused format passed; 12 Product-profile configuration, host and renderer-containment tests passed; repository audit/diff hygiene passed |
| `2026-08-21T02:58:00Z` | Made Product readiness evaluate exact local budget authority and return sanitised `Disarmed`/503 before deep content or vector verification in the authorised baseline | Focused format passed; 21/21 Product runtime tests passed, including missing grant, unreadable SQLite, session/vigency/expiry states, zero credential reads and a sanitised public payload |
| `2026-08-21T03:06:36Z` | Added fail-closed structured NuGet audit policy, online orchestrator audit, explicit offline dispositions, immutable Action commits and exact Node 24.19.0 | Ten NuGet policy fixtures, shared CI policy, real seven-project no-restore audit and orchestrator audit passed; zero vulnerabilities reported |
| `2026-08-21T03:20:23Z` | Replaced interpolated production SQLite connection strings and bounded recovery manifests without reducing the historical one-million-object ceiling | Seven hostile SQLite/recovery tests and the existing full recovery regression passed; focused format, source scan, repository audit and diff hygiene passed |
| `2026-08-21T03:24:00Z` | Reconciled current corpus/Stage evidence and ADR-0020 deployment roles across the documentation; recorded dated evidence and unexecuted boundaries; defined the missing Dashboard display-font token | Dashboard lint, typecheck, 45 tests and production build passed; documentation links, repository audit and diff hygiene passed |
| `2026-08-21T04:20:01Z` | Closed zero-budget admission before credential/HTTP even with an exact grant; hardened recovery duplicate/link/length races; introduced exact marker-owned output policy for Render, integration and OCI artefacts; made the Render builder prove offline status and fail-closed public health; completed local browser and dependency-drift inspection | Focused suites, rebuilt integration artefact, local browser query, Render component harness, locked restore, format, Release build, 571 .NET tests/coverage, both npm graphs, structured audits, disposable migrations and repository hygiene passed; canonical CI retained its human-review stop |
| `2026-08-21T04:52:58Z` | Corrected Render corpus provenance/evidence wording; authenticated and bounded integration/OCI archives; required coordinator-held digests; revalidated Render source trees; narrowed link-test skips; rehashed the extracted server immediately before launch | Eleven hostile/trusted archive fixtures, seven policy suites, authenticated integration start/restart/cold recovery, focused static regressions, final format/build and all 571 .NET tests passed. One initial focused CA1875 finding and one stale static assertion failed, were corrected at root, and passed on rerun. Independent security re-review found zero critical/high local finding. |
| `2026-08-22T01:52:02Z` | Recorded the two bounded exceptional language-review dispositions and the single approved corrected offline gate for sealed candidate `e80d10a29738fa7a042286c549687d08b2fe1dea` | `H-GATE-LANGUAGE-001` is resolved only for that sealed candidate; every prior failure and interruption retains its original disposition; `H-HOMOLOGATION-001` remains blocked and no Human Gate or lifecycle transition occurred |
| `2026-08-22T02:14:50Z` | Preserved the 36-path `main` WIP in stash `e906e6945e72afe8f08774818d3d324f2dbb0b05` and fast-forwarded `main` from `e2ae23f8870ac2f447a9bfa09812288c7dcf3f66` to factual evidence commit `a6ff32038510c552d9568786091a993a9be01117` | The integrated tree is `38c7fa18b52f69671ad2a757ace90520d914cf57`, the worktree is clean, no merge commit was created, `H-HOMOLOGATION-001` remains blocked and no Human Gate or lifecycle transition occurred |

## Authorised current Render candidate addendum — 2026-08-23

- Status: `LOCAL_PACKAGE_READY_EXTERNAL_ACTION_BLOCKED`.
- Baseline: exact clean
  `main@468a8c5220db2c1b1bd1e2fbb12fa9348da497fa`; implementation commits
  `1a304bfd1698026cb3450a863f111f6e8e849a91` and
  `3f51a3cac61e8f153c4d83946907650c6266de79`.
- Provider result: exact 3,282-chunk plan, 52 sequential zero-retry embedding
  requests and USD 0.149629 conservatively committed within the USD 1.00
  AdministrativeIndexEmbedding allocation. QueryEmbedding and
  GroundedGeneration remained zero.
- Store result: fresh 3,282-vector PostgreSQL 18.4 store, active generation
  `idxgen-4b417b79a9d8cd2472cb657a5fe7509f297b39f4831215f62143080d896e4f0d`
  and structural-tree SHA-256
  `589426ba8f9437812f112b988882e28e970f8eb18a3575f8a5ab9960347d586c`.
- Package result: Build and Test passed locally without restore, Docker,
  registry or Render. The 84,573,051-byte package structural-tree SHA-256 is
  `5c06e33aa2bb7fc2cadeabe7469a8534e460899ccc86c7ab9b0af7d3afcdfc10`.
- Readiness result: HTTP 200 `Live`; expected fail-closed HTTP 503 `Unready`
  with `provider-budget: Disarmed` when no runtime credential/grant is supplied.
- Remaining ordered work: obtain exact external authority; build the private
  image from the attested context; publish it to an access-controlled registry
  by immutable digest; configure the Render Free service, secret reference and
  separately budgeted QueryEmbedding/GroundedGeneration grants; deploy; then
  verify liveness/readiness before any separately authorised product query.
- Stop condition: no external step starts without exact authority for its
  package digest, account, registry, cost ceiling, secret reference and
  provider-operation budgets. Human Gate and lifecycle transition remain
  separately governed.
