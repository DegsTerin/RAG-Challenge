# Current State

This document is the current factual snapshot of the workspace on 2026-08-16. It
grants no authority. Evolution and decisions in their original context belong in
[`State-Transition-Log.md`](State-Transition-Log.md) and the owning
reports.

## Lifecycle and gates

- The audit, permitted corrections and multi-agent readiness validation
  for Stage 1 were completed on branch `codex/stage1-multi-agent-readiness`,
  starting from `main@9f309e1b6a21a33cbd24b4b6498e840dd26585c9`, without changing the
  lifecycle. Commit `1055934` formalised envelopes, ownership, isolation,
  stop conditions, project-scoped agents and the fail-closed coverage gate.
  The first clean gate revealed process-wide interference between test
  classes that cleared SQLite pools; `b64291d` serialised the integration
  assembly classes, preserving the same tree as the pre-rewrite local identity
  `a0def61`. The focused suite passed 279/279 and `./eng/ci.ps1 -Offline` passed
  in a clean worktree with 505 .NET tests, 45 web tests, 95.38% line coverage and
  67.23% branch coverage. On 2026-08-14, at
  `codex/stage1-multi-agent-readiness@355bd6cd731528bcdb8fccfe71ee93b70acb1d1e`,
  the owner selected historical quarantine for the RB-2/RB-3 freezes and
  explicitly accepted ADR-0016. RB-2 still does not satisfy its gate,
  RB-3 remains unavailable for RB-4, and any successor requires separate
  authority, two independent human reviews and genuine human adjudication. The
  TypeScript/Node 24 architecture and `AgentRunner` boundary were accepted without
  constituting a Human Gate. After clean documentation commit `60ccbdc`, the Stage 2
  dependency preflight found no local package or tarball for
  `@openai/codex-sdk` and at that time refined the classification to
  `HUMAN_DECISION_REQUIRED`. The owner subsequently granted HTTPS authority
  bounded to `registry.npmjs.org`; the exact `0.147.0` graph was acquired
  with lifecycle, audit and fund scripts disabled, and subsequent
  validations used the offline cache. Stage 2 implemented the isolated deterministic
  orchestrator in `tools/ai-orchestrator/` through commits `b10d8ac` to
  `94ea9b7`. The clean canonical gate passed 215 unit, 11
  architecture, 279 integration, 45 web and 81 orchestrator tests; the dry run and
  controlled E2E with `FakeAgentRunner` passed. On that baseline, the operational
  disposition was `MULTI_AGENT_READY_WITH_CONDITIONS`: only the local fake
  was operational; persisted
  resume is mapped and contract-tested, not exposed through the CLI
  and not exercised against real Codex. The SDK does not provide a new thread ID
  before the first turn, so a real Codex start returns
  `ARCHITECTURE_CHANGE_REQUIRED`. No Codex/provider call, secret,
  Human Gate or lifecycle change had occurred at that boundary. After
  local documentary assessment of the locked SDK, the owner decided to keep
  ADR-0016 `accepted` rather than
  `superseded`, preserve `FakeAgentRunner` as the only validated operational baseline
  and keep `NEW_REAL_START` under
  `ARCHITECTURE_CHANGE_REQUIRED`. Under that decision, no ADR-0017 was created
  or accepted, and no additional SDK version was verified.
- Under `AUTH-MULTI-AGENT-REAL-RUNNER-PREP-001`, the owner’s subsequent request
  to make Stage 0, Stage 1 and Stage 2 operational authorised the
  documentary preparation of successor ADR-0017. The official query and npm registry
  confirmed that stable `@openai/codex-sdk` remains `0.147.0`; the SDK
  still has no pre-turn identity. ADR-0017 was subsequently explicitly
  `accepted`, and the implementation replaced only the SDK transport with Codex
  App Server `thread/start` → durable checkpoint → `turn/start` in technical
  commits `583c3b4` and `9512d6e`. The direct dependency is locked at
  `@openai/codex` `0.147.0`; the CLI exposes `--runner codex` only with
  `--authority-reference`, validates `account/read` as a `chatgpt` session and does not
  inherit the product provider credential. The first execution preflight stopped before creating a
  thread or turn because two fields required `experimentalApi`; the correction
  removed them and retained only the stable protocol. One real,
  read-only and controlled validation passed in run
  `run-38b7dabe-491d-40f8-baaf-ce11906bd78e`: revision `4` already contained the
  durable identity while the task remained `RUNNING`; revisions `5` and
  `6` recorded `PASS`, and final locks were zero. The current tooling
  classification is `MULTI_AGENT_READY` within the development envelope; every
  future execution still requires its own plan, clean baseline and bounded
  authority. The product, product provider, secret, Human Gate and
  lifecycle were not changed.
- Under `AUTH-STAGE012-GOV-SEC-ENGB-IMPL-001`, the isolated integration branch
  implemented and independently reviewed the permanent Stage hand-off rule,
  the product-provider credential boundary, trusted en-GB enforcement and the
  approved migration of current project-owned technical prose. The product
  credential is separated from development tooling and agents; administrative
  indexing, query embedding and grounded generation each require a distinct
  operation-specific request reference and an independently trusted in-memory
  grant immediately before credential lookup. Local product launchers no
  longer read `.env.local`, and verification used synthetic values, maps,
  readers and handlers only. The language checker validates immutable Git
  objects, protected controls and structured exclusions; its migration
  baseline is `COMPLETE` with zero findings.
  The three original Stage owner inputs were then moved byte for byte from the
  repository root to the ignored `reference-materials/governance-inputs/`
  directory. Their hashes, sizes, filenames and implementation lineage are
  recorded in
  [`Stage-0-1-2-Historical-Owner-Input-Manifest.md`](../../docs/Stage-0-1-2-Historical-Owner-Input-Manifest.md).
  They are local historical evidence, inactive and non-normative; no tracked
  translation or second authority was created. The owner’s supplemental
  decision classifies only the instruction-system version ledger as a mutable
  current header with newest-first entries; all entries from `4.15.0`
  backwards are one digest-bound historical region. The complete prefix of
  `State-Transition-Log.md` and every other formally append-only region remain
  byte protected. The subsequent credential-identifier correction confines
  the product identifier to 16 exact, classified current or digest-bound
  historical paths and removes it from editable descriptive documentation.
  Commit `7ee0df1` made the orchestrator parser recognise and validate that
  closed manifest field; commit `6a52054` moved its two synthetic contract
  tests outside the digest-bound enforcement region and restored that region
  to its recorded identity. At clean integration candidate
  `6a520545b12a93b04f1e45fedc637c885ac963e6`, `npm run check` passed 105 of
  107 orchestrator tests with zero failures and two host symlink-permission
  skips, and the language-policy suite passed 100/100. The real parser loaded
  all 16 permissions, and independent governance, security and result reviews
  passed. The documentary gate subsequently passed. At clean candidate
  `311f115e5b080b1d5c1cc55f43dc91426e9fcdd2`, the canonical offline gate ran
  exactly once in a closed child environment with MSBuild node reuse disabled
  and passed in 279,679 ms with no stderr or failed stage. It passed 215 unit,
  11 architecture, 294 integration and 45 web tests; merged .NET coverage was
  95.41% of lines and 67.29% of branches. The orchestrator passed 105 of 107
  tests with zero failures and two host symlink-permission skips, with 82.04%
  line, 76.83% branch and 88.74% function coverage. The language-policy suite
  passed 100/100, the language check covered 419 files with zero migration
  findings, the repository audit passed for 419 non-ignored files and Git diff
  hygiene passed. No connection to a previously reusable MSBuild node was
  observed, and no reusable worker remained after the gate. No provider call,
  credential use, external network, external action, Human Gate or lifecycle
  transition occurred under this authority.
- Under `AUTH-ENGB-REPOSITORY-COMPLETION-IMPL-001`, the coordinator
  started from the exact clean baseline
  `main@8882ab8a58e1db58fb0148b967894f1b8388adc2`, reviewed and integrated
  the private-identifier candidate as `172575da`, then created, reviewed and
  integrated the enforcement candidate as `b9031d5` followed by corrective
  commits `fe7f9f0` and `08a2c96`. The internal lane changed only private, editable,
  non-serialised, non-persisted and non-contractual names. The binding
  owner-listed canonical family, public script names, OpenAPI v1/v2, migrations,
  evaluation data, localisation, sources, citations, accepted ADRs and product
  requirements remain preserved. The v2 language policy classifies every
  tracked blob, exact binary and immutable-text identities, digest-bound
  regions, filenames, internal identifiers and new commit messages. An
  independent security review found a generic `pt-BR` dictionary bypass; the
  candidate was held, the bypass was removed, two exposed localisation
  fixtures received exact region classifications and the closing review
  passed. A later integrated result review found that arbitrary backticked
  content bypassed commit-message inspection. Gates remained stopped; both
  implementations now exempt only a complete value from the validated closed
  canonical set, with negative tests for a private identifier and American
  prose. The originating read-only reviewer approved the second correction
  with zero residual P0–P3 findings. Focused checks and both integrated final
  reviews passed with zero P0–P3 findings; the documentary gate passed for 420
  files. The initial canonical offline gate ran on clean `2c2b80c`, exited `1`
  after 6,085 ms and was not retried. The first language-policy stage passed 84
  of 105 tests; 21 synthetic Git tests failed at `git add .` before any restore,
  build, coverage or later stage. A bounded diagnostic reproduced the failure
  only when the task-owned temporary path produced 264-character fixture paths;
  the same closed Git add passed in a shorter task-owned path. Its disposition
  remains historically `TEST_BASELINE_BROKEN`: the result was attributable to
  the coordinator’s execution envelope, not evidence of a lane regression.
  Under `AUTH-ENGB-REPOSITORY-COMPLETION-CANONICAL-GATE-CORR-001`, the owner
  then authorised one corrected run on exact clean `6662aa0`. Exclusive short
  temporary root `C:\t\engb-corr-001` reduced the temporary-path length from
  153 to 23 characters; task-owned offline caches were complete, and preflight
  and postflight each found zero RAG-Challenge-owned process or listener. The
  command ran exactly once from `2026-08-16T15:16:46.4947443Z` to
  `2026-08-16T15:21:24.1317617Z`, exited `0` after 277,634 ms and was not
  retried. It passed 105/105 language-policy tests, inspected 420 tracked files
  with zero migration findings, built Release with zero warnings or errors,
  passed 215 unit, 11 architecture, 294 integration and 45 Dashboard tests,
  and produced merged .NET coverage of 95.41% of lines and 67.29% of branches.
  The orchestrator passed 105 of 107 tests with zero failures and two host
  symlink-permission skips, with 82.12% line, 76.71% branch and 88.78% function
  coverage. Dashboard and orchestrator lint, typecheck and builds passed; the
  repository audit passed for 420 non-ignored files and Git diff hygiene
  passed. The corrected canonical disposition is `PASS`; offline validation
  does not substitute for an online dependency audit. No provider, credential,
  network, external action, Human Gate or lifecycle transition occurred. The
  complete factual record is
  [`En-GB-Repository-Completion-Report.md`](../../docs/En-GB-Repository-Completion-Report.md).
- Position: `STATE-00 DISCOVERY` closed; `GATE-B01
  ARCHITECTURE_BOOTSTRAP_DECISION` approved and closed; `STATE-01
  PROJECT_SETUP` closed after a Human Gate approved without reservations on
  2026-07-31; entry into `STATE-02 ARCHITECTURE` authorised on 2026-07-31,
  with local, sequential documentary execution of batches `S02-A` and `S02-B`;
  `STATE-02` closed after a Human Gate approved without reservations on 2026-08-02;
  entry into `STATE-03 DATA_AND_INDEX_MODELING` authorised on 2026-08-02;
  `S03-A` and `S03-B0` through `S03-B5` completed; the Automatic Quality Gate for
  `STATE-03` approved without findings; `STATE-03` closed after a Human Gate
  approved without reservations on 2026-08-02; entry into `STATE-04
  BACKEND_IMPLEMENTATION` authorised and recorded on 2026-08-03. The
  owner authorised the closure of `S04-A0`, the offline pinning
  of the two selected parsers, the sequential execution of `S04-A` through
  `S04-D` and then the Automatic Quality Gate for `STATE-04` on 2026-08-04. `S04-A0` was
  closed through documentation. The incomplete offline-source precondition was
  resolved through a read-only allowlisted seed for an isolated cache; the
  pins were applied, the locked restore passed and the first synthetic runtime gate
  for `S04-A` was approved. `S04-A` completed administration, PDF/CSV ingestion,
  synchronisation through a fake transport, snapshots, chunks and idempotency;
  `S04-B` completed embeddings through a port with a deterministic fake, staging,
  canonical finalisation, commit, CAS activation, hard pre-filtering and idempotent
  replay; `S04-C` completed retrieval over a single active revision,
  eligibility/freshness, refusal, grounded response, citations and the
  `pt-BR`/`en-GB` matrix; `S04-D` completed the public v1 API, versioned OpenAPI,
  fail-closed health, Problem Details, limits, cancellation, rate limiting and the
  OpenAI adapters over direct HTTP exercised only with a fake handler. The
  four batches are complete. The Automatic Quality Gate for `STATE-04` was
  approved without open findings; `AQG-S04-001` (P2) was resolved by an
  integrated test of the complete synthetic flow.
  The Human Gate for `STATE-04` was approved with the documented reservations on 2026-08-04; `STATE-04` is
  closed.
- A local audit after the Human Gate, at
  `main@f71343291b942c66d0ff417a8764b032bbd63bff`, identified findings
  `AUD-S04-001` through `AUD-S04-004`. The owner authorised the consolidated
  corrective increment `S04-CORR-01`: transactional rebinding of observations
  on `304`/identical hash (`a674560ed1093e96d533012f1b11a292c3f641b5`),
  complete `paragraph-window-v1` chunking
  (`b875eac6e9ce4c72783d4e4bb72a59686ca58248`), governed one-shot administration
  with a durable journal (`ac34c085a499a34ea8ee1c9106675482e38790c3`)
  and this documentary reconciliation. The executable corrections are
  implemented and the factual documents were reconciled. The corrective Automatic
  Quality Gate was approved at
  `main@114ea6f7f76936dac991553588660fc986bd0f10`; the subsequent disposition of the
  four findings is part of the consolidated result below. This did not reopen the
  lifecycle, alter the historical Human Gate or authorise `STATE-05`.
- The subsequent resumption of the audit identified `AUD-S04-005` through
  `AUD-S04-009`. `S04-CORR-02` implemented global reachability before clean-up,
  exact replay in persisted domains, validation and typed failures for the
  OpenAI adapters, administrative classification by phase and reconciliation of
  comments. The new pass found residual `AUD-S04-005-R1` in
  recovering a reservation after a crash. `S04-CORR-03`, in commit
  `19889f560dad0f011006ff17fc7414c807838149`, added the internal
  versioned plan and transactional reconciliation of reservations before planning
  and finalisation. Its Automatic Quality Gate was approved with 169 tests,
  92.04% line coverage and 66.46% branch coverage. The restarted complete audit was
  `APROVADA`, with no new P0, P1, P2 or P3, and disposed of `AUD-S04-001` through
  `AUD-S04-009`, including `AUD-S04-005-R1`, as `RESOLVIDOS`. The lifecycle and
  historical Human Gate were not changed; `STATE-05` remained
  unauthorised in that result.
- Entry into `STATE-05 FRONTEND_IMPLEMENTATION`: recorded through documentation
  on 2026-08-04. Subsequently, at
  `main@cab336ada60866083f3e688fe1a13cff348a3335`, corpus `4.9.2` and a clean working
  tree, the owner authorised the local, offline, sequential and
  bounded execution of `S05-A0` through `S05-A4`. The five batches were completed in commits
  `9c27cc49442ff467486c93febf7144e6d3a652b7`,
  `2fd7526f0907361d6c03552379341b877e88c236`,
  `7a42d332ddf6646c575c7cae16cfe9085120e18d`,
  `a8835b94ab485e542f7cfe23355283c92de17fc8` and
  `5865a225cdab9bd92f9befa00c7ee581b2aa0877`. The Dashboard implements the
  existing v1 client contract, states, same-origin query, `pt-BR`/`en-GB`
  localisation, `Light`/`Dark` themes, coverage, provenance, citations,
  safe failures and accessibility within the approved scope. Final checks
  approved lint, typecheck, 28 offline tests and build. The Automatic
  Quality Gate, Human Gate and `STATE-06` were neither authorised nor executed.
- Automatic Quality Gate for `STATE-05`: authorised and started on 2026-08-05
  at `main@f6df67a67657af891e4831a616b142d8da9fb584`, corpus `4.9.2` and a
  clean working tree. The audit stopped in accordance with the owner’s condition and
  resulted in `REPROVADO` with `AQG-S05-001` (P1): the client accepts a
  `canonicalUrl` with the `javascript:` scheme in a `LocalAuthorised` citation and
  presents it as a link. The local in-memory reproduction confirmed that the decoder
  accepted the payload and SSR emitted the unsafe `href`. No correction, product
  change, installation or external action was performed; lint, typecheck,
  tests, build and the gate’s browser stage were not reached after the finding. The
  Human Gate and `STATE-06` remain unauthorised and unexecuted.
- Correction `S05-CORR-01`: authorised and completed on 2026-08-05 at
  `main@7ee2241049dc68f16a38e85bd622928e64a317e7`, corpus `4.9.2` and a clean working
  tree. Commit `654fce6e0a09d6e7196e434de0ff6f5d6ccd5b04`
  rejects every non-HTTPS citation URL, requires a null `canonicalUrl` for
  `LocalAuthorised`, limits presented links to `OfficialExternal` with validated HTTPS
  and adds contract and presentation regressions. Lint, typecheck,
  29 tests and build passed; package, lockfile, OpenAPI, external contracts and
  backend remained unchanged. `AQG-S05-001` is
  `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`; the correction neither repeated nor approved
  the Automatic Quality Gate.
- Complete restart of the Automatic Quality Gate for `STATE-05`: authorised and
  started on 2026-08-05 at
  `main@f7e7f4a9d4afd234c9f3fcc725e7093653bc3363`, corpus `4.9.2` and a clean working
  tree. Static inspection confirmed the barrier implemented for
  `AQG-S05-001`, but the stop condition occurred before the npm retest and its
  disposition remains `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`. The restart
  resulted in `REPROVADO` with `AQG-S05-002` (P2), because the HTTP body limit is
  applied only after `response.text()` materialises the complete response,
  and `AQG-S05-003` (P2), because the visual document title remains fixed in
  English when `interfaceLanguage` is `pt-BR`. Executable preflight, lint,
  typecheck, tests, build, percentage coverage, browser, narrow viewport,
  visual accessibility, keyboard and the eight-combination matrix were not
  reached. No code, test, listener or configuration was changed.
- Correction `S05-CORR-02`: authorised and completed on 2026-08-05 at
  `main@651b4ad9edba79b3fc8a16e550fc2a357b6b85d2`, corpus `4.9.2` and a clean working
  tree. Commit `ec5ecf41b113853fc2863a94cbfe77dbe4741828`
  applies the 262,144-byte ceiling while reading the response, brings forward
  rejection of an excessive decimal `Content-Length`, stops at the first
  overflow and preserves cancellation. Commit
  `20458c8189b132b775786b2fc8f9b44ee5c2f7b8` localises the visual title by
  `interfaceLanguage`, without coupling it to the question language or theme.
  Lint, typecheck, 34 tests and build passed; loopback validation confirmed
  the `pt-BR` and `en-GB` titles, and the listener was stopped. Package, lockfile,
  OpenAPI, external contracts and backend remained unchanged.
  `AQG-S05-001`, `AQG-S05-002` and `AQG-S05-003` are corrected, but await
  retesting and disposition through a complete restart of the Automatic Quality Gate
  under subsequent and separate human authority.
- Complete restart of the Automatic Quality Gate after `S05-CORR-02`: authorised
  and started on 2026-08-05 at
  `main@3f120aaf3cbc199c821685b161ece95a1988a659`, corpus `4.9.2` and a clean working
  tree. Static inspection confirmed the barriers implemented for
  `AQG-S05-001`, `AQG-S05-002` and `AQG-S05-003`, but found
  `AQG-S05-004` (P2): the canonical backend emits `sourceFreshness: "Local"` for
  `LocalAuthorised` citations, while the Dashboard does not localise `Local` and
  presents the unknown-state fallback; the local fixture incorrectly uses
  `Current` and masks the divergence. The stop condition was
  triggered before executable preflight, lint, typecheck, tests, build,
  coverage and browser. No code, test, process, listener or
  configuration was changed. The gate was `REPROVADO`; the four findings
  remain without final disposition by an approved complete gate.
- Correction `S05-CORR-03`: authorised and completed on 2026-08-05 at
  `main@800e6dc92d2a3555dbe92bc4e3b6b16e6411726b`, corpus `4.9.2` and a clean working
  tree. Commit `9ef937744302044ee3cd9105c9a23ddd3557a861`
  restricts `sourceFreshness` to the canonical set, accepts
  `LocalAuthorised` only with `Local` and a null URL, rejects `Local` for
  `OfficialExternal`, localises `Local` in `pt-BR` and `en-GB` and corrects the
  synthetic fixture. Lint, typecheck, 35 tests and build passed; loopback
  validation confirmed the localised switch and ended without a listener.
  Package, lockfile, OpenAPI, external contracts and backend remained
  unchanged. `AQG-S05-001` through `AQG-S05-004` are corrected, but await
  retesting and disposition through a complete restart of the Automatic Quality Gate under
  subsequent and separate human authority.
- Complete restart of the Automatic Quality Gate after `S05-CORR-03`: authorised
  and started on 2026-08-05 at
  `main@b457970aed4564d5a654bb4e8d38439c98f29522`, corpus `4.9.2` and a clean working
  tree. Static inspection confirmed the barriers implemented for
  `AQG-S05-001` through `AQG-S05-004`, but found `AQG-S05-005` (P2): the client
  accepts a conclusion whose `answerLanguage` is a supported language different
  from the submitted `questionLanguage`. The stop condition was triggered before
  executable preflight, lint, typecheck, tests, build, coverage and browser.
  No code, test, process, listener or configuration was changed. The
  gate was `REPROVADO`; the four corrected findings continue to await
  executable retesting and `AQG-S05-005` remains open.
- Correction `S05-CORR-04`: authorised and completed on 2026-08-05 at
  `main@fb59861a8367749f2a11ac279add5007989d27e0`, corpus `4.9.2` and a clean working
  tree. Commit `bed8ec03d670ed4e76a556f7df723c30db320a24`
  requires `answerLanguage` to match the `questionLanguage` actually
  submitted and makes the client fail closed in both incompatible directions. The
  contract and transport fixtures and regressions cover valid conclusions in
  `pt-BR` and `en-GB`; the exact-boundary test no longer accepts the divergence.
  Lint, typecheck, 37 tests and build passed in the existing installation and
  offline. Package, lockfile, OpenAPI, external contracts and backend
  remained unchanged. `AQG-S05-001` through `AQG-S05-005` are corrected,
  but await retesting and disposition through a complete restart of the Automatic
  Quality Gate under subsequent and separate human authority.
- Complete restart of the Automatic Quality Gate after `S05-CORR-04`: authorised
  and started on 2026-08-05 at
  `main@a58c4038fb14e656c95303d914e02c7f8ad75c17`, corpus `4.9.2` and a clean working
  tree. Static inspection, lint, typecheck, 37 tests, build and byte-for-byte
  build repetition passed; `AQG-S05-001` through `AQG-S05-005` were
  disposed of as `RESOLVIDOS`. Keyboard validation found
  `AQG-S05-006` (P2): the skip link receives visible focus and changes the fragment
  to `#main-content`, but does not transfer focus to `<main>`; the active element
  returns to `<body>`, without providing a reliable focus bypass. The stop
  condition was triggered before narrow-viewport/reflow, complete Light/Dark
  switching and the browser matrix for the eight combinations. The gate was `REPROVADO`.
  The task-owned listener listened only on `127.0.0.1:4173`, was
  identified and stopped; the port ended free. No correction or frontend,
  code, test, dependency, contract or backend change was performed.
- Correction `S05-CORR-05`: authorised and completed on 2026-08-05 at
  `main@3ff7002b394199bbf253139836827231c1988116`, corpus `4.9.2` and a clean working
  tree. Commit `8b543eb85907b5aa4023f109dabb4bb11100da3e`
  makes `main#main-content` programmatically focusable, transfers focus to the
  target when the skip link is activated and adds a focused component regression.
  Lint, typecheck, 38 tests and build passed in the existing installation and
  offline. In the loopback build, `Tab` focused the skip link, `Enter` focused
  `<main>`, and the next `Tab` advanced to the selected question-language radio
  within the main content; there was no warning or error in the console.
  The task-owned listener was revalidated and stopped, and the port
  ended free. `AQG-S05-006` is
  `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`; the correction neither restarted nor
  approved the Automatic Quality Gate.
- Complete restart of the Automatic Quality Gate after `S05-CORR-05`: authorised
  and executed on 2026-08-05 at
  `main@8ee1213eed3522493204c68b4f843e9c438e0f69`, corpus `4.9.2` and a clean working
  tree. Static inspection, lint, typecheck, 38 tests, build and byte-for-byte
  build repetition passed; `AQG-S05-001` through `AQG-S05-006` were
  disposed of as `RESOLVIDOS`. The standard browser matrix approved all eight
  combinations of interface language, question language and theme. In the
  narrow matrix, all four `pt-BR` combinations produced horizontal overflow at
  320 CSS px (`scrollWidth` 355 versus `clientWidth` 303), while all four
  `en-GB` combinations produced no overflow. `AQG-S05-007` (P2) records the
  reflow failure of the Portuguese interface. The stop condition was triggered
  without correction. The gate was `REPROVADO`, with no new P0/P1. The task-owned
  listener listened only on `127.0.0.1:4173`, was revalidated and stopped, and
  the port ended free.
- Correction `S05-CORR-06`: authorised and completed on 2026-08-05 at
  `main@c32953eceb149efa3cfeb952f1dbfdbe0c00e2eb`, corpus `4.9.2` and a clean working
  tree. Commit `e34e73c7bbe8fabf96d5a5683df35935a3266e37`
  keeps the hero’s single column shrinkable and limits H1 typographic scaling
  at the compact breakpoint. The focused regression covers all eight combinations of
  interface language, question language and theme. Lint, typecheck, 38 tests
  and build passed offline. In temporary Chrome with extensions disabled and
  no extension target, all eight combinations passed at 320 CSS px with
  `scrollWidth` and `clientWidth` both equal to 305; visual reflow, Light/Dark and the
  complete focus and keyboard sequence were preserved. The task’s listeners
  were stopped and the ports ended free. `AQG-S05-007` is
  `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`; the Automatic Quality Gate was neither
  restarted nor approved.
- Complete restart of the Automatic Quality Gate after `S05-CORR-06`: authorised
  and started on 2026-08-05 at
  `main@bc2ddd6bf64fc82f7d68eb518c3013d85655c16a`, corpus `4.9.2` and a clean working
  tree. Static inspection repeated authority, lifecycle, scope,
  contract and security checks and found `AQG-S05-008` (P2): response, title and
  citation excerpt derived from the API accept continuous tokens valid under the
  contract, but their presentation surfaces do not permit those tokens to break
  in the narrow viewport. The stop condition was triggered before
  executable preflight, npm checks, build and browser; no process or
  listener was started. `AQG-S05-001` through `AQG-S05-006` retain the
  `RESOLVIDOS` disposition; `AQG-S05-007` remains
  `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`, because executable retesting was not
  reached. The gate was `REPROVADO`, with no new P0/P1 and without a product
  or test correction.
- Correction `S05-CORR-07`: authorised and completed on 2026-08-05 at
  `main@dfa31d02e8ba3fd171986ea2c1d06c70101d07a3`, corpus `4.9.2` and a clean working
  tree. Commit `3f003b9db67eefeccc7e677c319ca37a26d49fa7`
  applies safe breaking without truncation to the response, title and citation
  excerpt and extends the eight-combination regression with continuous tokens
  valid under the decoder. Lint, typecheck, 38 tests and build passed offline.
  The first headless attempt mistakenly activated the synthetic official URL through the harness
  before confirming focus, generating unauthorised external access;
  the task stopped, ended the runtimes and informed the owner. After the
  owner permitted headless continuation, the controlled repetition used
  only a local citation without a URL, blocking of every non-loopback request
  and a guard on the active element before `Enter`. All eight combinations passed at
  320 CSS px with a 305/305 document, intact and reflowed tokens, focus, keyboard,
  languages and themes preserved, no extension target and no external attempt
  or URL. Chrome and preview were stopped and the ports were left free. The
  policy refused to delete four temporary directories; the first profile
  may retain a cache of the accidental navigation. `AQG-S05-008` is
  `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`; the gate was not restarted.
- Complete restart of the Automatic Quality Gate after `S05-CORR-07`: authorised
  and completed on 2026-08-05 at
  `main@97ea076da84d7afdb3330aa05dcb39fc7b44ce0f`, corpus `4.9.2` and a clean working
  tree. Authority, lifecycle, scope, contracts and security were
  reinspected; lint, typecheck, 38 tests, build and byte-for-byte build
  repetition passed. Headless Chrome `151.0.7922.75`, in a temporary profile without
  extensions, executed all eight combinations at 1280 CSS px and again at 320 CSS
  px. The fixture contained only a local citation without a URL; interception
  blocked every non-loopback destination, and each `Enter` was preceded by an
  active-element guard. There was no external attempt or URL, no runtime
  exception and no new P0, P1, P2 or P3 finding. Reflow, continuous tokens,
  escaping, focus, keyboard, Light/Dark, `pt-BR`/`en-GB` and the citation’s original
  language passed; `AQG-S05-001` through `AQG-S05-008` are `RESOLVIDOS` and the gate
  is `APROVADO`. The task’s listeners were stopped and the ports
  were left free. JavaScript percentage coverage, reproduction on the exact Node
  version, an external accessibility engine and browser in a visible window
  remain limitations. The Human Gate was neither authorised nor executed, and
  `STATE-06` remains unauthorised.
- Complete restart of the Automatic Quality Gate after `S05-CORR-08`: authorised
  and completed on 2026-08-05 at
  `main@b68cf2d8a9a6c735781529f1f3fb63d5cd515f95`, corpus `4.9.2` and a clean working
  tree. Authority, lifecycle, scope, contracts and security were
  reinspected; lint, typecheck, 38 tests and two byte-for-byte identical builds
  passed. Headless Chrome `151.0.7922.75`, in a temporary profile without
  extensions, repeated all eight combinations at 1280 and 320 CSS px with a local
  citation without a URL, non-loopback interception and a guard before each `Enter`.
  Reflow, continuous tokens, escaping, focus, keyboard, Light/Dark,
  `pt-BR`/`en-GB`, the citation’s original language and the simplified hero passed.
  There was no external attempt or URL, no runtime exception and no new P0,
  P1, P2 or P3. `AQG-S05-001` through `AQG-S05-008` are `RESOLVIDOS` and the gate
  is `APROVADO`. Preview and Chrome were stopped, and ports 4173, 5173 and
  9230 were left free. The Human Gate was not executed, and `STATE-06` remains
  unauthorised.
- Human Gate for `STATE-05`: `APROVADO` without reservations on 2026-08-05 at
  `main@192613364429a79ce82a208f072f5005209e6f52`, corpus `4.9.2` and a clean working
  tree. In the same conversation, the owner received and reviewed the complete
  summary of the baseline, the approved Automatic Quality Gate,
  `AQG-S05-001` through `AQG-S05-008`, `S05-CORR-08`, the critical samples,
  checks, limitations, residual risks, negative scope and rollback, and
  confirmed the canonical phrase `Confirmo a decisão acima exclusivamente para
  STATE-05`. No new finding or reservation was recorded. The exclusively loopback human
  preview was stopped, and ports 4173, 5173 and 9230
  were left free. `STATE-05` is closed; `STATE-06` remains
  unauthorised and unexecuted.
- Entry into `STATE-06 INTEGRATION`: authorised and recorded on 2026-08-05
  at `main@8fb3b93532a569af953cdf24e190b82998020464`, corpus `4.9.2` and a
  clean working tree, after reconfirming location
  `C:\Projects\RAG-Challenge`, Git top-level `C:/Projects/RAG-Challenge`,
  Git directory `.git`, branch `main`, HEAD and corpus. After this record, the authority permits
  only `S06-A` to be executed locally, offline and sequentially:
  synthetic document → index → question → response flow between
  backend and frontend; official synchronisation only through a fake HTTP server
  and loopback; restart and persistence; non-secret configuration by environment;
  reproducible local artefact; reproduction on a clean baseline; .NET/npm checks,
  integration/E2E, build, hygiene, documentation and focused local commits.
  Dependencies, manifests/lockfiles, contracts, OpenAPI, ADRs, external network,
  real providers/accounts, secrets, real corpus/official source, GitHub, real OCI,
  publication, deployment, DB-Notifier, Automatic Quality Gate, Human Gate and
  `STATE-07` remain outside the authority. `STATE-06` is active; `S06-A`
  is authorised and not yet executed in this record.
- Batch `S06-A`: completed locally, offline and sequentially on 2026-08-05.
  The entry record is in commit
  `ad218b58210e41d0c3a2c76ef81b5886498fd01a`; the executable composition,
  E2E/loopback tests and artefact scripts are in commit
  `8041e25a554a7cc47ecebf4abe1fc8b94b12d12d`. The explicitly
  enabled profile in the `Integration` environment uses the existing SQLite stores and immutable
  content, a synthetic CSV fixture and local deterministic providers for
  document → index → question → response. The published Dashboard and v1 API
  run on the same origin; `pt-BR` and `en-GB` responses, citation, coverage,
  restart, catalogue, activation, index and persisted raw content passed. The
  official synchronisation was exercised only through a fake HTTP server on
  loopback, with proxy and redirects disabled. The local 58-file artefact
  was produced twice over the clean tracked baseline
  `main@8041e25a554a7cc47ecebf4abe1fc8b94b12d12d` with an identical SHA-256
  `b2b6f50352c29a89f91640870564df263a2a5888f2009a94dc9a0ec1bb33b3c4`,
  and the second copy was reproduced with the same active generation after restart.
  Format, Release build, 174 .NET tests, applicable .NET coverage, lint,
  typecheck, 38 npm tests, Vite build and submission through the published UI in
  Chrome passed. Ports 5086/5096 and temporary runtimes ended clean.
  The owning report is
  [`STATE-06-Integration-Report.md`](../../docs/STATE-06-Integration-Report.md).
  `STATE-06` remains active; Automatic Quality Gate, Human Gate, `STATE-07`
  and external actions were neither authorised nor executed.
- Focused correction of the Dashboard toolchain policy: authorised and completed
  on 2026-08-05 in commit
  `a7d50d8e72d5f5600ae41e3fdd313f4f1e502188`. `engines` and `devEngines`
  now accept and enforce Node.js `>=24.18.0 <25` and npm `>=11.16.0 <12`, with
  `onFail: "error"`; the unapplied exact `packageManager` was removed and the
  lockfile root metadata was reconciled. `.nvmrc` retains `24.18.0`
  as an optional lower-bound selector, without restricting the supported
  range. On the installation already updated to Node.js `24.19.0` and npm
  `11.17.0`, lint, typecheck, 38 tests and build passed offline. Two
  builds over the commit’s clean baseline produced the same 58-file ZIP
  and SHA-256
  `65b405c690a1c66c374296745613217717d7fd38f04cbefb15994323da1ffc98`;
  loopback reproduction and restart passed. No dependency, installation,
  contract, OpenAPI, ADR, lifecycle or external action changed.
- Automatic Quality Gate for `STATE-06`: authorised and executed locally,
  offline and sequentially on 2026-08-06 at
  `main@a6f0480b7f229b63c5ac24d65e61f55de1c6483a`, corpus `4.9.2` and a clean working
  tree. Format, Release build, 174 .NET tests, combined coverage of
  92.38% of lines and 66.59% of branches, lint, typecheck, 38 npm tests,
  Vite build, E2E/restart, synchronisation over fake loopback HTTP, secret-free
  configuration, two identical ZIP builds and artefact reproduction passed.
  The gate was `REPROVADO` because of three open P2 findings: `AQG-S06-001`, absence
  of the state-owned non-production OCI plan/rehearsal;
  `AQG-S06-002`, partial integrated coverage of resilience and cancellation; and
  `AQG-S06-003`, absence of real examples in the README, its obsolete factual
  state and the divergence between Lifecycle (`STATE-06`) and roadmap
  (`S08-B`/`BL-M13`). No P0, P1 or P3 was identified. The audit did not
  correct the product, tests, README, Lifecycle, roadmap, ADRs or contracts and
  did not execute a Human Gate, OCI, external action or `STATE-07`. `STATE-06`
  remains active and the Human Gate is premature. Task processes, listeners and temporary
  stores ended absent; execution policy refused the recursive
  removal of the ignored coverage directory under `TestResults/`, which
  retains only generated evidence.
- Correction `S06-CORR-01`: authorised by the owner on 2026-08-06 at
  `main@140c0516e4dbfc02808a90f0496550eb6b09da1b`, corpus `4.9.2` and a clean working
  tree. Decision `NORM-S06-001` keeps a factually current README in `STATE-06`
  with a genuinely verified local/synthetic example and
  reserves its public finalisation with its own OCI evidence
  and real product execution for `STATE-08`; normative reconciliation is recorded as
  corpus `4.9.3`. The supply chain for the three Linux ARM64 `10.0.10` runtime packs
  approved identity, version, catalogue SHA-512, author/repository
  signatures under offline revocation, MIT licence, dependency-free
  closure and no applicable advisory. After the explicit authority
  extensions, the four production lockfiles recorded only that RID and
  those three packs in commit
  `4b808319b0c1abf0970f9f41c77fb1e08d295585`; the ARM64 rehearsal, the composite
  cancellation/resilience proofs and the local/synthetic README were
  implemented in commits
  `405ab20d3e76a75f1a0f50fd625ec71831b9134b`,
  `801f77625e68692fe7b4691798694b4e8d92433a` and
  `9d72a1bb93325f6303516592fb4ff352a0a531ca`. `AUTH-S06-DEP-002` added
  only `linux-arm64` to the four production projects and completed the isolated
  cache with the 13 already-locked test packages; commit
  `f1a02cd7c7acb50bcd3fa8b00e69e6c3f59b88c3` materialises the four
  project declarations. The locked solution restore passed only with the
  verified local source and isolated cache, without changing any lockfile or
  graph. C4 approved format, warning-free Release build, 179 .NET tests,
  92.40% line coverage and 66.60% branch coverage, lint, typecheck, 38
  npm tests, Vite build, audit of 198 files, two identical ARM64
  reproductions, static verification and the README commands. The local flow
  preserved the same generation after restart; processes and listeners ended
  absent. `AQG-S06-001` through `AQG-S06-003` are
  `CORRECTED_PENDING_GATE_RETEST`; the historical Automatic Quality Gate remains
  `REPROVADO` and was not repeated. Human Gate, `STATE-07`, Linux execution, real
  OCI and other external actions remain unauthorised.
- Complete restart of the Automatic Quality Gate for `STATE-06`: authorised by
  `AUTH-S06-AQG-RETEST-001` and executed locally, offline and
  sequentially on 2026-08-06 at
  `main@9d7c4ce816eca049ba09942ab7fe8b1148aa73c9`, corpus `4.9.3` and a clean working
  tree. The preflight found no product-owned process and
  no listener on the task ports. The audit repeated from the beginning the
  supply chain for the three ARM64 runtime packs, locked restore with a verified local
  source and isolated cache, static inspections, the complete technical gate,
  coverage, focused integration/cancellation/resilience tests, two
  ARM64 reproductions, the static verifier, the published README commands
  and security hygiene. Format and warning-free Release build, 179
  .NET tests, 92.40% line coverage and 66.60% branch coverage, lint, typecheck,
  38 npm tests and Vite build passed. The two ARM64 reproductions were
  identical, with SHA-256
  `d539f0dd27553859966fe45f373363d32ffd34c61cd59618fe7cf61dcd9b2369`, and the
  verifier approved 17 ELF64 AArch64 payloads without executing Linux or contacting
  OCI. The README commands locally produced and reproduced the synthetic
  artefact, including the same active generation after restart. The gate was
  `APROVADO`, with no new P0, P1, P2 or P3; `AQG-S06-001` through `AQG-S06-003` are
  `RESOLVIDOS`. `STATE-06` remains active. Human Gate, `STATE-07`, Linux
  execution, real OCI and other external actions were neither authorised nor executed.
- Static technical audit after the gate: findings `AST-001`,
  `AST-002` and `AST-003` were confirmed and corrected sequentially over the
  pre-correction anchor
  `main@bfc3aefc3a731b1b49b47458374cb903860faf6f`. Commit
  `0b3c5be2c80f0f1ee83af82d2158e87360c33ea7` binds official-record resolution
  to the immutable snapshot revision; commit
  `d3fa9d77863092918dbef6fa7afee12992c2053f` requires and validates complete
  generation-bound authority in vector search; commit
  `cfb93892571bec1beae3087b1f5ff44932d24693` transactionally validates the
  complete set of active bindings; and commit
  `dc3dde2437ad3cbb50b397358fcda043c9d6f4b3` adds the local post-snapshot
  referential-integrity migration. Consolidated verification on the latest
  baseline approved a warning/error-free Release build, 87 unit, 10
  architecture and 109 integration tests, 206 in total without failure or skip, format,
  no pending change in the EF model and Git hygiene. The
  post-correction review at `main@dc3dde2437ad3cbb50b397358fcda043c9d6f4b3`,
  corpus `4.9.3` and a clean working tree disposed of `AST-001` through `AST-003` as
  `RESOLVIDOS`. The canonical v1 contract, public API and OpenAPI remained
  unchanged. No real database was migrated or repaired. This disposition
  belongs to the AST audit and neither repeated nor replaced the approved Automatic Quality
  Gate at the previous baseline; `STATE-06` remains active, without a Human
  Gate, `STATE-07` or external action.
- New complete restart of the Automatic Quality Gate for `STATE-06`: authorised
  by the owner and started locally, offline and sequentially on
  2026-08-06 at
  `main@f92e26c7008a2d124bd10edb2e3f03c0c9ad2bf6`, corpus `4.9.3` and a clean working
  tree. The reconciliation inventoried the eight commits and 25 files after
  `bfc3aefc3a731b1b49b47458374cb903860faf6f`; the preflight found no
  product process and no listener on the task ports. Static
  inspection identified `AQG-S06-005` (P2): the only two PowerShell tests
  for the fail-closed controls, `eng/test-assert-coverage.ps1` and
  `eng/test-ci-policy.ps1`, are not invoked by any entry point; the
  workflow calls only `eng/ci.ps1`, which also does not execute them. Therefore, the
  canonical CI can pass without testing the coverage aggregator or the policy
  it claims to enforce. The gate stopped without a silent correction before restore,
  build, suites, coverage, migration, ARM64 or README commands and is
  `REPROVADO`. `AQG-S06-005` remains `ABERTO`; the Human Gate remains
  premature, and `STATE-07` and external actions remain unauthorised.
- Focused correction of `AQG-S06-005`: authorised by the owner on 2026-08-06
  at `main@000dca0210e220a9f247159178c6d97d9fc4fd55`, corpus `4.9.3` and a
  clean working tree. `eng/ci-policy.ps1` now provides a mandatory invocation
  that fails when the script does not exist and propagates any exception
  with context; `eng/ci.ps1` executes the coverage and policy tests once
  each, before restore; and `eng/test-ci-policy.ps1` proves success, failure
  propagation, missing script, single invocation of both tests and single consumption of the
  canonical entry point by the workflow. The workflow remained unchanged. The
  focused verification approved parsing of the three scripts, 11 coverage cases, 14
  policy/integration controls, `git diff --check` and an audit of 203
  files. The complete Automatic Quality Gate was not restarted and no
  other suite was executed. `AQG-S06-005` is
  `CORRECTED_PENDING_GATE_RETEST`; the Human Gate remains premature.
- Complete restart after the correction of `AQG-S06-005`: authorised by the
  owner and executed locally, offline and sequentially on
  2026-08-06 at
  `main@616bef4e2ae8c0b26c10781cd728dc6089136a60`, corpus `4.9.3` and a clean working
  tree. The local source for the three ARM64 `10.0.10` runtime packs, their
  signatures and the isolated locked restore were revalidated without a
  lockfile change. The automatic entry point executed and approved the 11
  coverage cases and 14 policy controls before the technical gate. Release build,
  206 .NET tests, 93.11% line coverage and 66.89% branch coverage, 38
  npm tests, persistence/migration, EF without a pending change,
  cancellation/resilience, two identical ARM64 reproductions, static
  verifier, README commands, security and hygiene passed. The gate is
  `APROVADO`, with no new P0, P1, P2 or P3; `AQG-S06-005` is `RESOLVIDO` and
  `AQG-S06-001` through `AQG-S06-005` remain `RESOLVIDOS`. `STATE-06` remains
  active; Human Gate, `STATE-07`, external network and OCI were not executed.
- Human Gate for `STATE-06`: the owner received and reviewed in the same
  conversation the complete summary at
  `main@2f70705dcbe293b22ccd039d0764b2b9ca4b2e8a`, corpus `4.9.3` and a clean working
  tree, including deliverables, Automatic Quality Gate, historical
  findings, technical samples, limitations, rollback and negative scope. The
  decision was `APROVADO COM RESSALVAS` after the canonical confirmation
  `Confirmo a decisão acima exclusivamente para STATE-06`. The reservations
  preserve the absence of Linux ARM64 execution, real OCI, providers, real corpus and
  sources, operational storage, JavaScript percentage coverage,
  packet-level network observation and migration on a real database.
  `STATE-06 INTEGRATION` is closed. The decision neither authorised nor started
  `STATE-07`, network, OCI, publication or deployment.
- Documentary entry into `STATE-07 TESTING_HOMOLOGATION`: authorised and
  recorded on 2026-08-06 at
  `main@3240a4b13acd82a1cf5815ac64f6997b2a7f89bf`, corpus `4.9.3` and a clean working
  tree. The authority covers only the factual snapshot, append-only
  history, strictly necessary public-status blocks and one
  focused local commit. `STATE-07` is active without an authorised or executed batch.
  Dataset, RAG evaluation, tests, load, dynamic security, browser,
  providers, real sources, network, OCI, GitHub, publication, deployment, `STATE-08`
  and any external action remain unauthorised and unexecuted.
- Planning baseline for `S07-A`: documentary proposal
  [`STATE-07-S07-A-Evaluation-And-Security-Proposal.md`](../../docs/STATE-07-S07-A-Evaluation-And-Security-Proposal.md)
  was created in commit `183c8cd9fe303096a355ab731e72dc81748eb626` and
  confirmed by the owner on 2026-08-07 exclusively as a planning
  baseline. The confirmation did not grant `AUTH-S07-A-DATASET-001`,
  `AUTH-S07-A-RUN-001`, dataset materialisation, evaluation, tests, load,
  dynamic security, browser, providers, real sources, network or external action.
- Factual execution of `S07-A` A1-A5: A1 was materialised in commit
  `968f69c2d9c37959d617742af5ac48aee5ca09d5`; harness preparation and its
  freeze-safe correction are in `ae8d96487fe719d89741aa33e5607e532301d60e` and
  `18994db15d963b321ace93b0069436ffc4813b53`; A2 was frozen in
  `43ddc0de4a6c10b32a657f3c1e471a743cb42b5f`; A3 successfully executed 11 synthetic cases
  under `AUTH-S07-A-RUN-001`, preserving eight ignored files and the
  result at SHA-256
  `9efc2eef05388433af58e01242a1b1589556c43620eeec509f583fba0c2073bc`; and A4
  was reconciled in `760bbcf4626b7890ffdfb0eeb0a8c5419b5feec7`. The focused corrections
  to retained-workspace validation and line endings for future
  evidence are in `275becfb04a4d0f7a1703c3be3f4c59d87550cc2` and
  `6cd939849909a8abf2c5dd0534244da5f19be833`. A5 was `APROVADO` under
  `AUTH-S07-A-A5-RETEST-002`: the three authorised commands ended with exit
  code `0`; 146 unit, 164 integration, 10 architecture
  and 38 Dashboard tests passed; coverage was 94.91% of lines and 67.42% of branches.
  All frozen digests and aggregates were recalculated and matched.
  `S07-A-FIND-001` remains `OPEN`; `S07-A-FIND-004` remains historically `OPEN`,
  with its cause corrected only for future evidence; and
  `S07-A-FIND-002`, `S07-A-FIND-003` and `S07-A-FIND-005` are `RESOLVIDOS`.
  The historical A3 evidence remains immutable. A5 was not an Automatic Quality
  Gate, Human Gate or lifecycle change and does not extend homologation beyond the
  local, offline, deterministic and sequential synthetic boundary.
- Automatic Quality Gate for `S07-A`: restarted in full and `APROVADO` on
  2026-08-09 under `AUTH-S07-A-AQG-RETEST-003`, at
  `main@a6626a363713b4fbcf83387b7b2104eae1f3e918`, corpus `4.10.1`, an initially clean
  working tree and OpenAPI v1 at the protected SHA-256. The static audit
  confirmed A1-A5, commits, factual state, append-only history,
  frozen manifests, eight ignored files without reparse points, all
  digests and the seven synthetic aggregates at `1.000000`. The preflight found
  no RAG-Challenge-owned process or listener. The three authorised commands
  ended with exit code `0`: an audit of 244 files,
  `Validate` with 6 of 6 tests and offline CI with 146 unit, 164
  integration, 10 architecture and 38 Dashboard tests; coverage of 94.91% of
  lines and 67.42% of branches, with a build without warnings or errors. `AQG-S07-001` through
  `AQG-S07-004` are `RESOLVIDOS`; no new finding arose.
  `S07-A-FIND-001` and `S07-A-FIND-004` remain `OPEN`, while
  `S07-A-FIND-002`, `S07-A-FIND-003` and `S07-A-FIND-005` remain
  `RESOLVIDOS`. Approval applies only to the local synthetic boundary;
  product thresholds, provider, source, browser, dynamic security, load,
  recovery, accessibility, Linux, OCI and production remain `NOT_RUN`.
  No Human Gate or lifecycle advance is inferred.
- HTTP/OpenAPI v2 contract and same-origin visual serving: the contract was
  frozen in commit `54bab1aa5f25b778093bea62ffecf7c479557f9a`, implemented
  locally in commit `c01abf525f4cc113baa389982da3b419d07556b6` and corrected
  focally in commit `5505a85253aa4a8a7a3690caf3dd7a762175cab9`. The Automatic
  Quality Gate restarted under `AUTH-STATE07-V2-SERVING-AQG-RETEST-001` was
  `APROVADO` on that latest clean baseline, corpus `4.10.1`. The static
  audit confirmed the implementation’s 33 paths, correction of malformed
  `pageNumber` routing, public boundaries and byte-for-byte preservation
  of OpenAPI v1 and v2. The preflight found no product-owned process or
  listener. All focused commands and offline CI ended
  with exit code `0`; 147 unit, 171 integration, 11
  architecture and 42 Dashboard tests passed, with 94.80% line coverage and 67.14%
  branch coverage. `AQG-S07-V2-001` and `AQG-S07-V2-002` are `RESOLVIDOS`, with no new
  finding. Real browser/assistive technology, data, renderer, provider, source and network,
  load, crash/recovery, Linux, OCI and production remain `NOT_RUN`.
  No Human Gate or lifecycle advance is inferred.
- Integration, restart and cold recovery of the composed v2 runtime: authorised
  under `AUTH-STATE07-V2-INTEGRATION-RECOVERY-IMPL-001` on clean baseline
  `main@a47bd40b1873920c7660abb14acd68de45a7dde4`, corpus `4.10.1`, and
  completed in commit `e5dae7ee5a786417fba2c6ef0555686816b0b330`. The
  composition remains fail-closed outside the `Integration` profile; within it,
  the same synthetic instance serves query, readiness and the verified visual
  reader over in-memory project-owned PDF/PNG. 52 of 52 focused tests
  passed. The harness published on `127.0.0.1:5086` resulted in `Passed`: PNG serving
  `200`, revalidation `304`, the same generation after restart and cold restore,
  identical fingerprints for the confined copies, a 64 MiB visual ceiling and a token
  bucket with ten accepted accesses and the eleventh rejected with `429`. Two
  offline builds produced the same ZIP SHA-256
  `e27c64571b63538e4cba21f552df500c24a4bab3a6365e6229e2d9dd033f2f7d`.
  Clean-up removed the runtime, stores, backup, restore and task-owned temporaries;
  no host or listener remained. OpenAPI v1/v2, contract, schema,
  migration, ADR, dependency and lockfile did not change. This focused verification
  was not an Automatic Quality Gate, Human Gate or lifecycle; the evidence does not
  homologate the product, real data/renderer/provider/source, browser, assistive
  technology, load, comprehensive crash injection, operational recovery,
  Linux, OCI or production.
- Factual correction to the `STATE-07` dependency order in Lifecycle:
  authorised under `AUTH-STATE07-V2-INTEGRATION-RECOVERY-LIFECYCLE-CORR-001`
  at `main@de40a93e0023f854fec840a93934c199c294f9c6`, corpus `4.10.2` and a
  clean working tree. Only state annotations were reconciled:
  `S04-CORR-04-E` has an approved corrective Automatic Quality Gate;
  the v2 contract/serving are implemented and have an approved Automatic Quality Gate;
  integration, restart, confined cold backup/restore and limits were
  implemented and verified focally in commit
  `e5dae7ee5a786417fba2c6ef0555686816b0b330`; in that record, its Automatic
  Quality Gate remained `NOT_RUN`. Dataset and homologation remained
  subsequent and unauthorised. The normative order, states and criteria
  did not change; no runtime, test, gate or lifecycle was executed in this
  documentary correction.
- Automatic Quality Gate for v2 integration and recovery: restarted
  in full and `APROVADO` on 2026-08-09 under
  `AUTH-STATE07-V2-INTEGRATION-RECOVERY-AQG-RETEST-001`, at
  `main@f6c648c40cf8d0280cfceca5509a381bddb9fc8f`, corpus `4.10.3`, an initially clean
  working tree and protected OpenAPI v1/v2. The audit of 255
  files passed; the preflight found no RAG-Challenge-owned process or listener;
  and 53 of 53 focused tests passed. Two deterministic builds
  produced the same ZIP SHA-256
  `ab5e450efe1b606f2b8e50e2f5885a3c1ae19bf4ad90dd96d096e00506daec28`.
  The published harness resulted in `Passed`, with three `Ready` readiness results, generation
  preserved after restart and cold restore, PNG serving and `304`, a 64 MiB ceiling
  and a token bucket with ten accepted accesses and the eleventh rejected. Offline
  CI approved 147 unit, 174 integration, 11 architecture and
  42 Dashboard tests, 94.81% line coverage and 67.24% branch coverage, and a build
  without warnings or errors. Clean-up was completed without a remaining runtime or
  listener. The gate was approved without a new finding and
  `AQG-S07-V2-IR-001` is `RESOLVIDO`. Approval remains synthetic and does not
  constitute product homologation, a Human Gate or a lifecycle change.
- Subsequent factual correction to Lifecycle: authorised under
  `AUTH-STATE07-V2-INTEGRATION-RECOVERY-LIFECYCLE-CORR-002` at
  `main@7ad6bae369eb1efbf6429902a2fd1f4441b60a32`, corpus `4.10.4` and a clean working
  tree. The two outdated current claims were reconciled to
  record only the v2 integration and recovery Automatic Quality Gate
  `APROVADO` under `AUTH-STATE07-V2-INTEGRATION-RECOVERY-AQG-RETEST-001`, with no
  new finding, and `AQG-S07-V2-IR-001` `RESOLVIDO`. The normative order,
  states and criteria did not change; dataset and product homologation
  remain subsequent, `NOT_RUN` and unauthorised. No runtime, test,
  gate, Human Gate or lifecycle was executed in this documentary correction.
- Readiness A0 for the first product document: executed locally,
  offline, sequentially and without product behaviour under
  `AUTH-S07-A-PRODUCT-A0-001`, at
  `main@78d49e135d7b517c7ff89a9e5edcbcc7839e4043`, corpus `4.10.5`, an initially clean
  working tree and protected OpenAPI v1/v2. Ignored candidate
  `postgresql-18-reference-a4`, PostgreSQL `18.4`, remained confined to the
  authorised intake, as an untracked regular file without a reparse point.
  The `15,771,040` bytes, `%PDF-1.4`, EOF and SHA-256
  `cea7b845568095eb56dee1b51bfa145c6c6637bc4377c986019971577efefae4`
  matched the record. Provenance, `contentLanguage=en`,
  `sourceDeclaredLanguage=en`, publisher and attribution remained
  consistent. Parsing, indexing, source-byte retention, quotation and citation
  have an explicit disposition; page rendering, derivative-image creation,
  derivative-image retention, runtime derivative display and the intended source
  or derivative distribution boundary remain `UNPROVEN`. Without inferring
  rights from the general permission, the factual disposition was
  `BLOCKED/EXCLUDED`, not `READY_FOR_PRODUCT_ACTIVATION`. No dataset,
  manifest, derivative, indexing, activation, parser, renderer, runtime, test,
  gate, Human Gate or lifecycle was executed or changed.
- Architectural decision on rights mapping: prepared under
  `AUTH-S07-A-RIGHTS-POLICY-CORR-PREP-001` at
  `main@17c41a78cbe853473860403d476797064b77c78a`, corpus `4.10.7` and an initially clean
  working tree and explicitly accepted by the owner through
  `ADR-0011: ACEITAR.` at
  `main@09f6760cb1a41d907da42b8c01cb34a7425030b9`, corpus `4.10.8`. The
  [ADR-0011](../../docs/architecture/ADR-0011-Source-Rights-Evidence-Mapping-And-Same-Origin-Derivative-Display-Boundary.md)
  is `accepted`: it preserves the ten independent decisions and the
  fail-closed posture, establishes explicit, auditable and conditional mapping of
  broad primary grants and separates same-origin runtime display from
  external byte distribution/publication. The decision records the
  static incompatibility between the v2 contract, which requires reassessment of the
  intended distribution boundary, and
  `DocumentRightsEligibilityPolicy.PdfVisualEvidence`, which does not assess
  `SourceAndDerivativeByteDistributionOrPublication`. Acceptance establishes
  architectural authority only: it changed no public contract or
  behaviour and did not reclassify the five `UNPROVEN` rights or the
  PostgreSQL `BLOCKED/EXCLUDED` disposition. Acceptance alone did not
  authorise semantic reconciliation, internal correction or a new A0; the
  subsequent reconciliation is recorded below, and the two executable stages
  continue to require separate authority.
- Semantic reconciliation of ADR-0011: authorised under
  `AUTH-S07-A-RIGHTS-POLICY-CORR-RECONCILE-001` at
  `main@6fc81b973ca217693a286479df3ff6db0f4577e9`, corpus `4.10.9`, an initially clean
  working tree and protected OpenAPI v1/v2. ADR-0004, ADR-0008, the
  eligibility record and v2 documentary contract now apply the
  explicit, auditable and conditional mapping, preserve the ten decisions and
  fail-closed posture, distinguish same-origin runtime display from external distribution or
  publication and bind attribution, notices, disclaimers, trade mark
  and change marking to each derivative’s lineage. No public contract or
  behaviour changed. `postgresql-18-reference-a4` remains
  `BLOCKED/EXCLUDED`, with the five rights `UNPROVEN`; no new A0 was
  executed. At that baseline, the executable incompatibility still remained;
  the subsequent internal correction is recorded below.
- Internal correction of the ADR-0011 serving policy: implemented under
  `AUTH-S07-A-RIGHTS-POLICY-CORR-IMPL-001` in commit
  `b9c3e5f3a72c2dd7762c256198452ae2c217b2d2` and reconciled
  through documentation under `AUTH-S07-A-RIGHTS-POLICY-CORR-IMPL-RECONCILE-001`,
  from that `main`, corpus `4.10.10`, a clean working tree and protected OpenAPI v1/v2.
  Internal gate `PdfVisualEvidenceServing` assesses the ten decisions:
  `RuntimeDerivativeImageDisplay` must be `Permitted`;
  `SourceAndDerivativeByteDistributionOrPublication` `Unproven` blocks; and
  `Denied` is compatible only with `RuntimeDerivativeImageDisplay`
  `Permitted` at the accepted same-origin boundary. Focused verification approved 19
  policy tests, 23 regressions of the existing gates, three real-reader tests
  and six v1/v2 contract tests. No runtime or listener remained.
  At that baseline, no new A0 or gate had been executed and
  `postgresql-18-reference-a4` remained `BLOCKED/EXCLUDED` with the five
  `UNPROVEN` rights; the subsequent reassessment is recorded below.
- Candidate-specific A0 reassessment under ADR-0011: executed locally,
  offline, sequentially and without product behaviour under
  `AUTH-S07-A-PRODUCT-A0-002`, at
  `main@f21cdea2052d28de1e2ffb86b1629c1c10bc6b6a`, corpus `4.10.11`, an initially clean
  working tree and protected OpenAPI v1/v2. The ignored PDF remained a
  regular file, without a reparse point, with `15,771,040` bytes and SHA-256
  `cea7b845568095eb56dee1b51bfa145c6c6637bc4377c986019971577efefae4`.
  The already-recorded official grant is relevant to the five operations, but requires
  copyright, permission notice and two disclaimers in all copies. The
  current contract offers no determined mechanism for these texts in the PNG or
  public citation. Page rendering, derivative-image creation,
  derivative-image retention and `RuntimeDerivativeImageDisplay` remain
  `UNPROVEN`; `SourceAndDerivativeByteDistributionOrPublication` is `DENIED`
  outside the runtime-display boundary through deliberate exclusion of download,
  public hosting, permissive CORS, CDN, export, bundles, Git/Git LFS and
  republication. The disposition remains `BLOCKED/EXCLUDED`, not
  `READY_FOR_PRODUCT_ACTIVATION`. No dataset, manifest, derivative, parser,
  renderer, indexing, activation, test, runtime, gate, Human Gate or lifecycle
  was executed or changed.
- Architectural decision on a self-contained derivative image: prepared under
  `AUTH-S07-A-NOTICE-BEARING-PROFILE-ADR-PREP-001` at
  `main@1b64ca88a0efebd7ab450f5bdc22004a72f3dc53`, corpus `4.10.12`, an initially clean
  working tree and explicitly accepted by the owner through
  `ADR-0012: ACEITAR.` at
  `main@243a448823a114190f68a25f9d521e1849eddacf`, corpus `4.10.13`, a clean working
  tree and protected OpenAPI v1/v2. The
  [ADR-0012](../../docs/architecture/ADR-0012-Notice-Bearing-Page-Image-Profile-And-Derivative-Obligation-Delivery.md)
  is `accepted`: it defines one versioned composite PNG profile in
  which the page region preserves every pixel and a separate panel carries the
  complete notices. The decision also defines `DerivativeObligationSetV1`, its
  immutable binding to the render manifest, storage, backup/cold restore,
  same-origin serving, accessible presentation and the necessary impacts on
  schema, migration and the v2 contract. Acceptance grants architectural
  authority only: it does not reclassify PostgreSQL or change OpenAPI, code, schema,
  migration, dataset or behaviour; no renderer, runtime, test, gate,
  Human Gate or lifecycle was executed.
- Semantic reconciliation of ADR-0012: authorised under
  `AUTH-S07-A-NOTICE-BEARING-PROFILE-RECONCILE-001` at
  `main@5c2cea66e45f13479486a345552e5cc3cd47fefe`, corpus `4.10.14`, an initially clean
  working tree and protected OpenAPI v1/v2. ADR-0008, the v2 documentary
  contract, data dictionary, Security-And-Access, threat model and eligibility
  record now apply `pdf-page-png-notice-v1`,
  `DerivativeObligationSetV1`, manifest binding, storage/reachability,
  backup/cold restore, same-origin serving and accessible presentation. The
  reconciliation identifies the protected revision of the v2 contract,
  schema and migration as mandatory future work, without executing them. The ten decisions and
  fail-closed posture remain independent; `postgresql-18-reference-a4` remains
  `BLOCKED/EXCLUDED`, with four visual operations `UNPROVEN` and external
  distribution/publication `DENIED`. No new A0, code, test, renderer,
  runtime, gate, Human Gate or lifecycle was executed.
- Protected revision of the notice-bearing v2 contract: frozen under
  `AUTH-S07-A-NOTICE-BEARING-V2-CONTRACT-001` at
  `main@6982b0643468aee0a97c3bea6b5bbe9018f0804c`, corpus `4.10.15`, an initially clean
  working tree and protected OpenAPI v1/v2. OpenAPI v1 remained
  byte-for-byte at SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` and blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; the new OpenAPI v2 revision has
  SHA-256 `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733`
  and blob `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`. The contract adds only
  `obligationSetId` to images and `DerivativeObligationPresentationV1` to the PDF
  citation: `null` values preserve the legacy projection, while notice-bearing
  pages require one matching ID and complete presentation. The
  C# types and strict Dashboard decoder were updated. Five decoder
  tests and six .NET contract tests passed focally. At the end of that
  increment, schema, migration, renderer, storage and notice-bearing
  behaviour were not yet implemented. PostgreSQL remains
  `BLOCKED/EXCLUDED`; no new A0, runtime, gate, Human Gate or lifecycle was
  executed.
- Notice-bearing schema and migrations: implemented under
  `AUTH-S07-A-NOTICE-BEARING-SCHEMA-MIGRATION-001` in commit
  `98036f3c8c496544f4532d1fe48c981f836a1871`, at
  `main@564d9efd72285bb41545a5e60b63fcd44f9705fd`, corpus `4.10.16`, an initially clean
  working tree and protected OpenAPI v1/v2. The Control schema now
  persists immutable `DerivativeObligationSetV1` and its ordered blocks, accepts
  `pdf-page-png-notice-v1` alongside the legacy profile and binds
  `obligationSetId`/digest and source/notice dimensions to the render manifest. Migrations
  `20260810033026_AddNoticeBearingObligationSchema` and
  `20260810034537_SealNoticeBearingObligationBindings` apply fail-closed constraints,
  foreign keys and sealing triggers without inferred backfill or mutation
  of legacy records, manifests, hashes or activations. Seven of seven focused tests
  passed; there were no pending model changes; `foreign_key_check`,
  upgrade and rollback/reapply were approved in temporary task-owned SQLite
  stores. Clean-up was completed. Renderer, PNG, notice-bearing
  behaviour, dataset, new A0, gate, Human Gate and lifecycle were not
  executed; PostgreSQL remains `BLOCKED/EXCLUDED`.
- Architectural decision on corpus storage and visual evidence:
  [ADR-0008](../../docs/architecture/ADR-0008-Product-Corpus-Storage-And-Page-Image-Evidence.md)
  was explicitly accepted by the owner on 2026-08-07 at
  `main@5c151c64ae4d3049d68fee6788502d439aa25251`, corpus `4.9.4` and a clean working
  tree. Acceptance establishes architectural authority only; it does not
  reconcile ADR-0002, ADR-0004, security, the RAG module, contracts, data
  dictionary, threat model, OpenAPI or another normative document, and does not
  authorise implementation, moving the PDF, PNG generation, dataset,
  indexing, activation, tests, providers, network, publication or external action.
- Architectural decision on document-language taxonomy:
  [ADR-0009](../../docs/architecture/ADR-0009-Document-Evidence-And-Query-Language-Taxonomy.md)
  was explicitly accepted by the owner on 2026-08-07 at
  `main@89994e82d246b1cc0a240e99a2d09942e316f7cc`, corpus `4.9.4` and a clean working
  tree. `SupportedQueryLanguage` remains restricted to `pt-BR` and
  `en-GB`; `DocumentContentLanguage` is a distinct BCP 47 domain; the `en`
  declared by the PostgreSQL PDF is not inferred as `en-GB`; citations
  preserve the original language; and OpenAPI v1 remains unchanged. Acceptance
  removed only the decision blocker. In that record, semantic reconciliation
  of ADR-0008 had not yet been executed; contracts, normative
  corpus, dataset and runtime remained unchanged, and implementation
  remained unauthorised.
- Joint semantic reconciliation of ADR-0008/0009: authorised by the
  owner on 2026-08-07 at
  `main@3d15ad4f2726f715c8dcf880491927ad0ff37b2f`, corpus `4.9.4` and a clean working
  tree. Corpus `4.9.5` aligns the 18 confirmed canonical documents
  for permanent source/PNG storage, content addressing,
  render lifecycle and rights; separates `SupportedQueryLanguage=pt-BR|en-GB` from
  BCP 47 `DocumentContentLanguage`; preserves `en`, citations in their original language
  and exact evaluation strata. OpenAPI v1 was preserved byte-for-byte; v2 is
  only a planned, unimplemented contract. The batch did not change code,
  tests, schema, migrations, data, dataset, eligibility record,
  dependencies, lockfiles or PDF; it did not generate PNGs, index, activate or execute
  a provider/browser/network operation or perform an external action.
- Corrective implementation `S03-CORR-01`: authorised by
  `AUTH-S03-CORR-001` on 2026-08-07 at
  `main@ffc7bef913dda2699b072ef172188291f6ac0500`, corpus `4.9.5` and a clean working
  tree, with the `STATE-03` technical owner. The directed runtime preflight
  found no process or listener proven to belong to
  RAG-Challenge and stopped nothing. Commit
  `5fdbbc36d8eee29fdeec4b179564bd1eff322558` separates
  `SupportedQueryLanguage` from `DocumentContentLanguage`, preserves the observed
  `SourceDeclaredLanguage`, keeps `en` distinct from `en-GB`, models
  `DocumentPageImage`/`DocumentRenderManifest`, adds the sole Control migration
  `20260807161323_AddDocumentLanguageAndRenderManifestModel`, propagates
  the separation through ingestion, indexing, query, provider, vector metadata
  and Server, and protects sources/images reached by manifests during clean-up.
- Verification of `S03-CORR-01`: 19 unit tests and 6 focused integration cases
  passed; `eng/ci.ps1 -Offline` approved 106 unit, 116
  integration, 10 architecture and 38 Dashboard tests, 93.74%
  line coverage and 67.11% branch coverage and an audit of 212 files. Legacy upgrade,
  rollback/reapply, `foreign_key_check`, vector reading and the two pending
  model checks passed only in disposable SQLite. OpenAPI v1 remains
  byte-for-byte at SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` and Git blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`. The new tables remained
  empty; there was no renderer, PNG, import, change to the PostgreSQL candidate,
  dataset, activation, serving, v2, dependency, lockfile, network, external action,
  Automatic Quality Gate, Human Gate or lifecycle change.
- Corrective implementation `S04-CORR-04-A`: authorised by
  `AUTH-S04-CORR-04-A-001` on 2026-08-07 at
  `main@ea7fc582f991bb9290e26a7e2d4e074abc46bf3c`, corpus `4.9.7` and a clean working
  tree, with the `STATE-04` technical owner. The directed runtime preflight
  found no product process or RAG-Challenge listener; nothing
  was stopped before implementation. Commit
  `26f2e154b736687693b31ab02ca59cfb8ba86655` replaces the store’s minimal
  result with typed descriptors, implements bounded, idempotent,
  atomic writing verified through reopening, requires hash/length upon reopening and
  migrates ingestion, composition and control-plane validation to the new port.
  `IStorageMaintenance`, `cleanup-plan-v1` and the reservation/finalisation protocol
  remain the sole existing authority for physical deletion.
- Verification of `S04-CORR-04-A`: 3 unit tests and 57 focused integration cases
  passed; `eng/ci.ps1 -Offline` approved 109 unit, 118
  integration, 10 architecture and 38 Dashboard tests, 93.76%
  line coverage and 67.15% branch coverage and an audit of 213 files. OpenAPI v1
  remained byte-for-byte at SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
  Packages, lockfiles, schema, migrations, renderer, PNG, rights, persisted
  manifests, activation digests, v2, real data and external actions were not
  changed. Corpus `4.9.8` records only this fact; `STATE-07` remains
  active, without a gate or transition, and no subsequent increment was started.
- Corrective implementation `S04-CORR-04-B`: authorised by
  `AUTH-S04-CORR-04-B-001` on 2026-08-07 at
  `main@196bbcafcb493ce4e45a2c9e784965ff933f124d`, corpus `4.9.8` and a clean working
  tree, with the `STATE-04` corrective technical owner. The directed runtime preflight
  found no product process or
  RAG-Challenge listener; nothing was stopped. Commit
  `a886a944ecd1ce485eee9c072385e96210e90520` introduces typed record
  `DocumentRightsEligibilityRecordV1`, ADR-0008’s ten independent
  decisions, closed states `Permitted`, `Denied` and `Unproven`, stable
  evidence references and fixed textual/visual gates that accept only
  explicit permissions. Distribution/publication remains a separate decision and
  is not inferred from the other rights.
- Verification of `S04-CORR-04-B`: 14 focused synthetic unit cases passed;
  `eng/ci.ps1 -Offline` approved 123 unit, 118 integration, 10
  architecture and 38 Dashboard tests, 93.72% line coverage and 67.20%
  branch coverage and an audit of 216 files. OpenAPI v1 remained byte-for-byte at
  SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
  Packages, lockfiles, schema, migrations, renderer, PNG, persistence of
  rights/manifests, activation, v2, real source/licence/right/data, network and
  external actions did not change. Corpus `4.9.9` records only this fact;
  `STATE-07` remains active, without a gate or transition, and `S04-CORR-04-C` or a
  subsequent increment was not started.
- Corrective implementation `S04-CORR-04-C`: authorised by
  `AUTH-S04-CORR-04-C-001` on 2026-08-07 at
  `main@75475c391c7fc1fb5ff298492a5d1da4c4f99fbb`, corpus `4.9.9` and a clean working
  tree, with the `STATE-04` corrective technical owner. The directed runtime preflight
  found no product process or listener proven to
  belong to RAG-Challenge; nothing was stopped. The supply-chain gate
  used isolated caches, CLI home and artefacts, verified the eight selected identities and
  versions, signatures, hashes, licences, upstream commits,
  graph, absence of a material advisory/deprecation and native Windows and
  Linux AArch64 assets before implementation. The temporary evidence remains outside
  Git, without material clean-up.
- Commit `981e61c3308ee3407769d10ab1fa554007f12799` implements the renderer port,
  explicit limit policy, internal single-document worker before the HTTP
  host, private binary framing, containment through a Job Object on Windows and
  `rlimit`/`prctl` on Linux, deterministic `pdf-page-png-v1` profile,
  fail-closed structural validation of PNGs and verified, idempotent
  and atomic finalisation of `DocumentRenderManifest` in the existing tables.
  The visual-rights gate, verified reopening of the source and every PNG and
  complete validation of every page precede manifest persistence.
  `IStorageMaintenance`, `cleanup-plan-v1` and the reservation/finalisation
  protocol remain the sole authority for physical deletion.
- Verification of `S04-CORR-04-C`: 7 focused unit cases and 10 focused
  integration cases passed with synthetic PDF/PNG bytes. Framework-dependent
  `linux-arm64` publish in locked/offline mode selected
  `libpdfium.so` and `libSkiaSharp.so` ELF64 AArch64 (`e_machine=183`).
  `eng/ci.ps1 -Offline` approved 130 unit, 128 integration, 10
  architecture and 38 Dashboard tests, with 93.53% line coverage and 66.80% branch coverage,
  warning-free Release build and audit of 223 files. Only four expected
  lockfiles changed; there was no project, schema, migration, model
  snapshot, endpoint or v2-contract change. OpenAPI v1 remained byte-for-byte at
  SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
  No real data, source, licence or right was used; there was no import,
  indexing, activation, serving, clean-up, provider, external action, Automatic
  Quality Gate, Human Gate or lifecycle change. Corpus `4.9.10` records
  only these facts; `STATE-07` remains active and `S04-CORR-04-D` or a
  subsequent increment was not started.
- Corrective implementation `S04-CORR-04-D`: authorised by
  `AUTH-S04-CORR-04-D-001` on 2026-08-07 at
  `main@548a817e2db4d9bad2d1a63e7dc9e9bb9ace418c`, corpus `4.9.10` and a clean working
  tree, with the `STATE-04` corrective technical owner. The directed runtime preflight
  found no product process or listener proven to
  belong to RAG-Challenge; nothing was stopped. Commit
  `d18224e46f559229a58e82b097abbf16ea9f359a` persists per revision the exact
  document/source binding, immutable schema-v1 snapshot of the ten rights
  decisions and mandatory render manifest for PDF/absent for CSV; requires the
  bindings in Initial, Replacement and Rollback; revalidates rollback and restricts
  ObservationRebind to an observation-only change with identical evidence.
- Pre-CAS now checks complete identity, supported document language,
  textual/visual gate, finalised generation, reopened source and, for PDF, finalised
  manifest, consecutive physical pages and all reopened PNGs. Replay
  compares the new bindings and rights. A Control transaction persists revision,
  bindings, evidence/rights, retention, head, audit and applicable journal
  completion; query readback fails closed when faced with an incomplete
  or divergent current revision.
- The sole Control migration
  `20260808004846_AddDocumentRightsAndActivationEvidenceBindings` creates only
  `activation_evidence_bindings` and `activation_rights_decisions`, without a data
  operation or backfill. History retains the existing fields and receives no
  inferred rights/manifests; the Vector database and the
  `sourceBindingSetDigest`/`activationBindingSetDigest` domains remain unchanged.
- Verification of `S04-CORR-04-D`: focused unit selections and 15 focused
  integration cases passed. Upgrade, rollback/reapply,
  `foreign_key_check`, historical compatibility and the two pending model checks
  passed in disposable SQLite. `eng/ci.ps1 -Offline` approved 135 unit,
  137 integration, 10 architecture and 38 Dashboard tests, with
  94.34% line coverage and 67.25% branch coverage, Release build and an approved audit of 226
  files. OpenAPI v1 remained byte-for-byte at SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` and
  Git blob `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`. No real data/right,
  v2, serving, `AnswerEvidenceRecord`, network, external action, gate or lifecycle
  change occurred. Corpus `4.9.11` records only these facts;
  `S04-CORR-04-E` was not started.
- Architectural decision on persistent answer evidence:
  [ADR-0010](../../docs/architecture/ADR-0010-Persistent-Answer-Evidence-Records-And-Bounded-Retention.md)
  was explicitly accepted by the owner on 2026-08-07 and recorded under
  local, offline and sequential documentary authority at
  `main@745304051c113c86f5ebbaaaf625fbde74c50c6a`, corpus `4.9.11` and a clean working
  tree. The decision fixes `S04-CORR-04-E` as the internal persistent contract
  `AnswerEvidenceRecordV1`, only for `Answered`, with `P30D` retention without
  refresh and participation in reachability. Corpus `4.10.0` records only the
  architectural authority and reconciles current facts; it does not implement E, create a
  migration/test, alter OpenAPI v1 or start v2, serving, real data,
  gate, lifecycle, network or external action.
- Corrective implementation of persistent answer evidence: on 2026-08-08,
  at `main@fc83e1ea6922a519baf527efc3f0a219e2674453`, corpus `4.10.0`, a clean working
  tree and OpenAPI v1 at the protected SHA-256, the owner authorised exclusively
  the local, offline and sequential implementation of
  `S04-CORR-04-E`. The directed runtime preflight found no process or
  listener proven to belong to the product. The increment materialises the
  canonical model/serialisation, `Answered`-only Control port and store, commit and
  readback before the v1 response, replay/conflict, allowlisted audit and the
  empty migration `20260808033247_AddAnswerEvidenceRecords`, without backfill or
  historical inference. `P30D` without refresh participates in `cleanup-plan-v1`,
  reservation, revalidation, finalisation and source/PNG reachability. Warning-free Release build,
  146 unit, 153 integration and 10 architecture tests and the
  two pending-model checks passed with disposable fixtures and stores. These
  direct checks did not execute an Automatic Quality Gate. Corpus
  `4.10.1` records only these facts; OpenAPI v1 remains byte-for-byte at the
  protected SHA-256, and v2, serving, real data, gates, lifecycle, network and
  external actions remain out of scope.
- Corrective Automatic Quality Gate for `S04-CORR-04-E`: executed locally,
  offline and sequentially on 2026-08-08 at
  `main@990d14172954567456d9ad90b6a767f6b6e0da78`, corpus `4.10.1`, a clean working
  tree and OpenAPI v1 at the protected SHA-256. The static audit identified
  `AQG-S04-002` (P2): the canonical contract states on lines 12–13 that
  evidence persistence remains unimplemented, but the same document
  records on lines 537 and 597–600 that `S04-CORR-04-E` implemented it
  locally. The finding remains `ABERTO` and the gate is `REPROVADO`. Because of the mandatory
  stop, runtime preflight, `eng/ci.ps1 -Offline` and the executable checks for
  build, tests, coverage, migration, restart, concurrency, failures,
  retention, clean-up, privacy and reachability were not started. No
  correction, Human Gate, lifecycle change, network or external action occurred.
- Focused documentary correction of `AQG-S04-002`: authorised by the owner and
  executed on 2026-08-08 at
  `main@3f42214c5c3554b6b341ab4c75a0a8e3cdb93f2a`, corpus `4.10.1`, a clean working
  tree and OpenAPI v1 at the protected SHA-256. The purpose paragraph of the
  canonical contract now records that persistent answer evidence was
  implemented locally by the separately authorised increment
  `S04-CORR-04-E`; image serving and v2 remain unimplemented. The accepted
  semantics of ADR-0010 and the implementation did not change. `AQG-S04-002` is
  `CORRECTED_PENDING_GATE_RETEST`; the historical Automatic Quality Gate remains
  `REPROVADO` and was not restarted. No source, test, behaviour, schema,
  migration, ADR, OpenAPI, Human Gate, lifecycle, network or external action changed.
- Complete restart of the corrective Automatic Quality Gate after correction of
  `AQG-S04-002`: authorised and started on 2026-08-08 at
  `main@da569d8dae6990db72e43df69f1ff0bb8861ac54`, corpus `4.10.1`, a completely clean
  working tree and OpenAPI v1 at the protected SHA-256. The directed runtime
  preflight found no process or listener proven to
  belong to the product; nothing was stopped. Static inspection confirmed the
  correction and disposed of `AQG-S04-002` as `RESOLVIDO`, but identified
  `AQG-S04-003` (P2): the mandatory-verification section of the canonical contract
  still describes the answer-evidence tests on lines 788–791 as future,
  while the baseline contains and records the unit and integration suites already
  implemented and directly executed. The stop condition was triggered;
  `eng/ci.ps1 -Offline`, build, tests, coverage, migration, restart,
  concurrency, failures, retention, clean-up, privacy and reachability were not
  executed in this restart. The gate is `REPROVADO`, `AQG-S04-003` remains
  `ABERTO` and no correction, Human Gate, lifecycle change, network or
  external action occurred.
- Focused documentary correction of `AQG-S04-003`: authorised by the owner and
  executed on 2026-08-08 at
  `main@cb67c7f752521f416f46d9cb4f2bb6a189ca1a48`, corpus `4.10.1`, a completely clean
  working tree and OpenAPI v1 at the protected SHA-256. The mandatory-
  verification section of the canonical contract now classifies the answer-evidence tests as
  requirements, rather than future work, without converting the
  architectural document into implementation or execution evidence. The described scope and
  coverage did not change. `AQG-S04-003` is
  `CORRECTED_PENDING_GATE_RETEST`; the historical Automatic Quality Gate remains
  `REPROVADO` and was not restarted. No source, test, behaviour, schema,
  migration, ADR-0010, OpenAPI, v2, serving, Human Gate, lifecycle, network or
  external action changed.
- Complete restart of the corrective Automatic Quality Gate after correction of
  `AQG-S04-003`: authorised and started on 2026-08-08 at
  `main@baa85553f9d48c7c833b1e875699817849360bab`, corpus `4.10.1`, a completely clean
  working tree and OpenAPI v1 at the protected SHA-256. Static inspection
  confirmed the correction and disposed of `AQG-S04-003` as `RESOLVIDO`, but
  identified `AQG-S04-004` (P2): ADR-0010 requires direct rejection tests for
  citation, source, activation, manifest and page mismatches, while the focused
  persistence suite tests only one digest mismatch in the activation
  header. The page-domain test checks absence/excess against the
  record itself, without comparing divergent values with the persisted Control
  authority. The fail-closed branches exist in the implementation, but the required
  regression matrix is incomplete. The stop condition was triggered before
  runtime preflight, `eng/ci.ps1 -Offline`, build, tests, coverage, migration,
  restart, concurrency, failures, retention, clean-up, privacy and reachability.
  The gate is `REPROVADO`, `AQG-S04-004` remains `ABERTO` and no correction,
  Human Gate, lifecycle change, network or external action occurred.
- Focused correction of `AQG-S04-004`: authorised and executed on 2026-08-08 at
  `main@fd2e164ef1d8b1a90d867f4e77beea0e43fba9c3`, corpus `4.10.1`, a completely clean
  working tree and OpenAPI v1 at the protected SHA-256. The focused SQLite
  persistence suite now compares, one value per case, citation,
  source, activation, manifest and page mismatches with the persisted Control
  authority. The five cases reject with `InvalidDataException` and prove the
  absence of a header, citations, pages, administrative operation and answer-evidence
  creation audit. The focused file approved 14 cases and the affected integration
  project approved 157 Release cases, without restore. No
  implementation defect was demonstrated and no product change was
  necessary. `AQG-S04-004` is `CORRECTED_PENDING_GATE_RETEST`; the historical Automatic
  Quality Gate remains `REPROVADO` and was not restarted. No
  source, behaviour, schema, migration, ADR-0010, OpenAPI, v2, serving,
  Human Gate, lifecycle, network or external action changed.
- Complete restart of the corrective Automatic Quality Gate after correction of
  `AQG-S04-004`: authorised and executed on 2026-08-08 at
  `main@5a2dcbafdc0a3925338043b079f9eacc9e70ca1b`, corpus `4.10.1`, a completely clean
  working tree and OpenAPI v1 at the protected SHA-256. Static inspection
  disposed of `AQG-S04-004` as `RESOLVIDO`; the directed preflight found
  no product-owned process or TCP listener and
  stopped nothing. `eng/ci.ps1 -Offline` approved locked restore, format, warning-
  and error-free Release build, 146 unit, 157 integration, 10
  architecture and 38 Dashboard tests. .NET coverage was 94.91% of lines and
  67.42% of branches; lint, typecheck, web build and audit of the 235 files
  also passed. The gate is `APROVADO`, `AQG-S04-002` through `AQG-S04-004` are
  `RESOLVIDOS` and no new P0, P1, P2 or P3 was identified. The protected
  baseline remained intact and without a residual runtime. No source, test,
  behaviour, schema, migration, ADR-0010, OpenAPI, v2, serving, Human Gate,
  lifecycle, network or external action changed.
- Closure of `S04-A0`: `PdfPig` `0.1.15` and `CsvHelper` `33.1.0` were
  selected conditionally for local development;
  `Sylvan.Data.Csv` `1.4.4` remains an unselected fallback and cannot be
  authorised through automatic substitution. The OpenAI adapter uses direct HTTP,
  without an `OpenAI` or `System.ClientModel` package. Hashes, gates, limitations,
  evidence, resolution of the pre-pin blocker and runtime gate are recorded in the
  [STATE-04 report](../../docs/STATE-04-Backend-Implementation-Report.md).
  The parser signature remains
  `CONDITIONAL_REVOCATION_NOT_CURRENT`, and the incomplete semantics of the NuGet hash
  domains were accepted only for local development in `STATE-04`.
- Completed scope of `STATE-01`: record entry and execute locally,
  sequentially, batches `S01-A`, `S01-B` and `S01-C`, without RAG or
  functional logic. The additional authority of 2026-07-30 permits exclusively
  HTTPS access to `https://registry.npmjs.org/` and
  `https://api.nuget.org/v3/index.json`, local installation of pinned
  dependencies, npm lockfile, npm/.NET audits and loopback for a health smoke test.
- Documentary Automatic Quality Gate: `APROVADO` for baseline `3.4.0`, which
  closed `STATE-00`, and for cross-cutting increment `3.5.0`, without reopening
  the gate; corrections `3.5.1` through `3.5.4` also `APROVADAS`; identity
  migration `4.0.0`, factual correction `4.0.1` and copy-ready pattern `4.1.0`
  also `APROVADOS`. The normative decision-efficiency increment was
  recorded as `4.2.0` (`MINOR`), and its semantically
  equivalent reorganisation as `4.2.1` (`PATCH`), on 2026-08-01. The bilingual
  query-support decision was formalised as corpus `4.3.0` (`MINOR`) on the
  same date, and the separate decision for visual `pt-BR` and `en-GB` support was
  formalised as corpus `4.4.0` (`MINOR`). The subsequent selection of
  `Light` and `Dark` themes was formalised as corpus `4.5.0` (`MINOR`); the incremental
  audits were `APROVADAS`. The subsequent removal of the 12-system
  and 120-page ceilings was formalised as corpus `4.6.0` (`MINOR`), with targeted
  documentary validation. The subsequent reconciliation of the initial catalogue of 51
  databases, 9 categories, 54 associations, PDF/CSV and unified retrieval was
  formalised as corpus `4.7.0` (`MINOR`). The explicit and
  independent human acceptance of ADR-0002 and ADR-0004 through ADR-0006 was recorded as corpus
  `4.8.0` (`MINOR`). The subsequent combined audit was executed at
  `main@a01a765d177efb6c4013c6846c5f54c8adbe7e0f` and resulted in
  `REPROVADA`, with one P1, one P2 and one P3 finding. After acceptance and
  reconciliation of ADR-0007, the new combined audit at
  `main@3978a17201cf5f6ac4ddc189862736fc3646457b`, corpus `4.9.1`, resulted in
  `APROVADA`, disposed of the three findings as `RESOLVIDOS` and found no new
  P0, P1, P2 or P3. None of these increments transitioned the lifecycle or accepted
  an ADR by implication.
- Human Gate for `STATE-00`: `APROVADO` without reservations on 2026-07-30.
- `GATE-B01`: `APROVADO` without reservations on 2026-07-30.
- Transition to `STATE-01 PROJECT_SETUP`: authorised on 2026-07-30.
- Automatic Quality Gate for `STATE-01`: `APROVADO`; lockfiles, restore,
  format, build, tests, coverage, Dashboard, audits, loopback health,
  hygiene and reproduction in a clean clone were validated on 2026-07-30. The offline
  gate, health smoke test and clean reproduction were repeated on the
  renamed baseline.
- Human Gate for `STATE-01`: `APROVADO` without reservations on 2026-07-31.
- Transition to `STATE-02 ARCHITECTURE`: authorised on 2026-07-31,
  exclusively within the limits, deliverables, checks, risks, rollback and
  negative scope of the complete summary presented in the coordinating conversation.
  The authority does not accept an ADR by implication or grant network, installation,
  paid-service, GitHub, OCI, publication, deployment or DB-Notifier-change authority.
- Execution of `STATE-02`: sequential documentary package for `S02-A` and `S02-B`
  prepared in commit `979677fa1f4d7324340b8be15d88eb8b5b802a1a` on
  2026-07-31, with canonical contracts, threat model and factual report. The
  authorised public
  verification was recorded in commits
  `f1066c3509f5f48d4fe6e21c9e36403e642c1431`,
  `e80f8c41bea3f28deff3d8cdccafccbca5dcc016` and
  `9cc62746ea2ba861676a2d5bfee317eaf66dad7c`: no public primary-source item
  remains pending within the authorised scope. Facts dependent on
  an account, entitlement, capacity or execution remain unverified and
  require their own future authority. On 2026-08-01, the owner explicitly
  and independently accepted ADR-0002 and ADR-0004 through ADR-0006 at
  `main@39e2f803bf73cb4e2b59e56a0596e2858a3aed51`, corpus `4.7.0`; no
  choice followed from another and nothing was implemented by implication.
- Automatic Quality Gate for `STATE-02`: `APROVADO` after a new combined
  audit of reconciled baseline
  `main@3978a17201cf5f6ac4ddc189862736fc3646457b`, corpus `4.9.1`.
  `AQG-S02-001` (P1), `AQG-S02-002` (P2) and `AQG-S02-003` (P3), historically
  recorded by the previous audit, were disposed of as `RESOLVIDOS`;
  the new audit found no new P0, P1, P2 or P3.
- Human Gate for `STATE-02`: `APROVADO` without reservations on 2026-08-02 at
  `main@6e61c4cf4429e2a62145d43bec3783146f01e37f`, corpus `4.9.1`, after
  review of the automatic report, critical samples, limitations, residual
  risks, pending conditions and negative scope. The decision closes only
  `STATE-02` and does not authorise entry into `STATE-03`.
- Transition to `STATE-03 DATA_AND_INDEX_MODELING`: authorised on 2026-08-02
  at `main@35b67c194f6ea2459833420b8bc2143fadfe75df`, corpus `4.9.1` and a
  clean working tree. The authority permits recording the entry and executing
  locally and sequentially only `S03-A`: model and dictionary,
  identities, states, relations, constraints, revisions, canonical
  serialisation, reference vectors for the two digest domains, three
  pre-CAS validations, activation/retention/rollback invariants and deterministic
  fixtures. `S03-B`, migrations, persistent stores, new
  dependencies and installation remain unauthorised.
- Execution of `S03-A`: completed locally in commit
  `ace780a25edb2749046e9897b8af36e0719e3e54` with the logical model in Domain,
  pre-CAS construction and validation in Application, permanent dictionary,
  executable canonical vectors, 51/54/9 fixtures and staging,
  activation, observation, retention and rollback invariants. Infrastructure, projects,
  dependencies and lockfiles were not changed; there is no migration or
  persistent store.
- Verification of `S03-A`: format and Release build without restore passed; 68
  tests passed (53 unit, 10 architecture and 5 integration), with
  95.55% line coverage and 89.93% branch coverage; lint, typecheck, 2
  tests and Dashboard build also passed on the existing installation;
  audit approved for 104 non-ignored files and diff hygiene approved.
  The owner explicitly accepted Node.js `24.18.1` only as a
  local verification variation; the repository pins remain at
  `24.18.0`/npm `11.16.0`. The aggregate `eng/ci.ps1 -Offline` was not executed
  because it would perform restore and `npm ci`, actions blocked with `S03-B`.
- Authority for `S03-B`: recorded in commit
  `381d1cd297580476e461a242ce5b66c4884e521b` after an approved repetition of
  `S03-B0`. The conservative supply-chain set contains 42 nupkgs, all
  verified by catalogue SHA-512, repository signature, advisory and
  licence. The Linux ARM64 asset is ELF64 AArch64 and contains SQLite `3.53.3`.
- Closure reconciliation: the real `net10.0` restore materialised 40
  packages in `project.assets.json` and separately installed local tool
  `dotnet-ef 10.0.10`, totalling 41 items. `System.Memory 4.5.3`,
  declared only in the `.NETStandard2.0` group of `SQLitePCLRaw.core 2.1.12`,
  remained verified conservative evidence and was not pinned, referenced
  or materialised. The owner explicitly accepted this distinction and
  authorised resuming `S03-B1` in the interrupted working tree.
- Execution of `S03-B1` through `S03-B4`: `S03-B1` completed locked restore with 40
  materialised packages, tool `dotnet-ef 10.0.10`, absence of
  `System.Memory` and exactly four affected lockfiles. `S03-B2` recorded
  the separate physical models and initial migrations for `control.db` and
  `vectors.db`. `S03-B3` implemented ports in Application and local stores in
  Infrastructure for control authority, derived vectors, immutable
  content, CAS, retention, clean-up and recovery. `S03-B4` added deterministic
  fixtures and tests, including concurrency, rollback, corruption,
  recovery and the synthetic functional case of 10,000 vectors by 1,536
  dimensions. Canonical finalisation calculates specification digests,
  logical artefacts and complete manifest from SQLite readback.
- Execution of `S03-B5`: the previous Control migration-discovery divergence
  did not reproduce after a new clean and Release build at
  `main@c72c8b967667f72e8971f4887174585d3640a36e`. The evidence is consistent
  with stale incremental output used by `--no-build`, without proving a deeper
  historical cause and without requiring a change to model, migration,
  snapshot or contract. Control and Vector passed list, apply, rollback to
  zero, reapply and pending-model check in separate temporary stores, which were then
  removed. The offline aggregate passed 82 .NET tests, 94.83% line
  coverage and 72.34% branch coverage, lint, typecheck, two tests and Dashboard
  build, audit of 130 files and diff hygiene. The current NuGet query
  found no vulnerable package. `S03-B5` and S03-B are complete;
  this does not execute an Automatic Quality Gate or Human Gate.
- Automatic Quality Gate for `STATE-03`: `APROVADO` at
  `main@3d0731fdf3f5004fb185dc760b5f74e4d73b4aa5`, corpus `4.9.1`, with no
  P0, P1, P2 or P3 findings. Preflight found no product processes/listeners.
  The local offline gate confirmed the 51/54/9 catalogue, two digest
  domains, three pre-CAS validations, architectural boundaries, reopenable
  content, non-queryable staging, CAS, retention, rollback through a new revision,
  recovery, 40 packages materialised in Infrastructure without
  `System.Memory`, Control/Vector migrations and a clean baseline. The aggregate
  approved 82 tests, 94.83% line coverage, 72.34% branch coverage, Dashboard and
  audit of 130 files. No tracked file was changed during
  collection; the temporary stores were removed. A Human Gate was not executed.
- Automatic Quality Gate for `STATE-04`: `APROVADO` at
  `main@7f236542133719481a02f507cf802a1dd385f328`, corpus `4.9.2`, with no
  open findings. `AQG-S04-001` (P2), the initial absence of one proof of the
  synthetic ingestion→activation→query flow, was `RESOLVIDO`. The offline gate
  approved format, warning-free Release build, 119 tests, 92.37% line coverage,
  65.73% branch coverage, architecture,
  contracts, integration, parsers, hashes, lockfiles, OpenAPI, failures, health,
  security and hygiene. Dashboard and external validations were `NÃO
  APLICÁVEIS` under the explicit negative scope. The automatic result did not
  execute the Human Gate.
- Human Gate for `STATE-04`: `APROVADO COM RESSALVAS` on 2026-08-04 at
  `main@6d141decdf5f40661bb9f408d6aa97f9f322cfcf`, corpus `4.9.2` and a clean working
  tree, after presentation of the complete summary of the gate, deliverables,
  critical samples, limitations, risks, rollback and negative scope. The canonical
  phrase was `Confirmo a decisão acima exclusivamente para STATE-04`. The
  decision closes only `STATE-04`; it does not authorise entry into or execution of
  `STATE-05`, production, external action or clean-up of temporary evidence.
- Subsequent corrective audit of `STATE-04`: the first pass, started
  at `main@f71343291b942c66d0ff417a8764b032bbd63bff`, identified
  `AUD-S04-001` through `AUD-S04-004` and was interrupted in accordance with its stop
  condition. `S04-CORR-01` implemented C1, C2 and C3 in focused commits
  `a674560ed1093e96d533012f1b11a292c3f641b5`,
  `b875eac6e9ce4c72783d4e4bb72a59686ca58248` and
  `ac34c085a499a34ea8ee1c9106675482e38790c3`; C4 reconciles the factual
  records. The corrective Automatic Quality Gate was `APROVADO` at
  `main@114ea6f7f76936dac991553588660fc986bd0f10`, with 150 applicable tests,
  92.26% line coverage and 65.07% branch coverage; the complete audit remains
  pending and mandatory before disposition of the findings. No new Human
  Gate was executed.
- Human Gate for `STATE-03`: `APROVADO` without reservations on 2026-08-02 at
  `main@a88dc1f296bb9117dd8e869b83d1665cee99634f`, corpus `4.9.1`, after
  review, in the same conversation, of the complete summary of the current baseline,
  deliverables, automatic results, limitations, residual risks, negative
  scope and rollback. The canonical phrase was `Confirmo a decisão acima
  exclusivamente para STATE-03`. The decision closes only `STATE-03` and does not
  authorise entry into `STATE-04` or any external action.
- Transition to `STATE-04 BACKEND_IMPLEMENTATION`: authorised and recorded
  on 2026-08-03 at `main@e62fbc4da7e580dc1f5449689699374e42ea8ab4`,
  corpus `4.9.2` and a clean working tree. The authority permits only updating
  the factual snapshot and append-only history and creating the focused local commit
  for that record. `S04-A`, `S04-B`, `S04-C` and `S04-D`, code, dependencies,
  packages, lockfiles, migrations, contracts, ADRs, network, providers, accounts,
  secrets, real corpus, official sources, operational storage, Dashboard,
  GitHub, OCI, publication, deployment and DB-Notifier remain unauthorised.
- Corrective batch for `STATE-02`: at
  `main@9707b87d75a6acb14c8993ff0283a4221bc6c762`, corpus `4.8.0`,
  ADR-0007 was prepared, recommending separation of generation identity
  from activation-record identity. The sources of `AQG-S02-002` and
  `AQG-S02-003` were factually reconciled, without recording acceptance of the
  ADR, altering the accepted semantic contracts or repeating the Automatic Quality
  Gate. In that batch, the gate result remained `REPROVADO`.
- Corrective decision for `STATE-02`: on 2026-08-02, at
  `main@664187c6926be5ce4bef3734603f8d936626d535`, corpus `4.8.1`, the
  owner explicitly accepted ADR-0007 with decision
  `ADR-0007: ACEITAR.`. Acceptance corrects the architectural authority for
  identity/freshness and rollback, but neither authorises nor executes the
  tracked semantic reconciliation, did not dispose of `AQG-S02-001` in that record
  and did not repeat the Automatic Quality Gate.
- Semantic reconciliation of `STATE-02`: on 2026-08-02, starting from
  `main@9aa90c012e3bc973330f5a79678fc358c81809df`, corpus `4.9.0`, the
  accepted semantics of ADR-0007 were applied across the documentary baseline
  as corpus `4.9.1`. That batch did not repeat the Automatic Quality
  Gate; the result therefore remained `REPROVADO` and the findings had not yet
  received a new disposition.
- New combined audit of `STATE-02`: on 2026-08-02, at clean baseline
  `main@3978a17201cf5f6ac4ddc189862736fc3646457b`, corpus `4.9.1`, all
  applicable documentary areas were `APROVADAS`. The two digest domains,
  three pre-CAS validations, revalidation, hard pre-filtering, rollback, provenance,
  risks and routed documents converge; `AQG-S02-001` through `AQG-S02-003` are
  `RESOLVIDOS`, with no new classified finding. The work neither implemented nor
  executed behaviour, requested no Human Gate and did not authorise `STATE-03`.
- ADR-0001: `superseded` by ADR-0003, after original acceptance at
  `GATE-B01`; ADR-0002: `accepted`; ADR-0003: `accepted` by the explicit human
  request to rename the project to `RAG-Challenge`, incorporating
  unchanged all ADR-0001 decisions unrelated to naming;
  ADR-0004, ADR-0005 and ADR-0006: `accepted` by explicit and independent human
  decisions on 2026-08-01; ADR-0007: `accepted` by an explicit human decision
  on 2026-08-02; ADR-0008 and ADR-0009: `accepted` by explicit and independent
  human decisions on 2026-08-07, with joint semantic reconciliation
  applied in corpus `4.9.5`; ADR-0010: `accepted` by an explicit human
  decision on 2026-08-07, with recording and documentary reconciliation in corpus
  `4.10.0`. ADR acceptance does not replace implementation authorities:
  `S04-CORR-04-A` through `S04-CORR-04-E` are complete at the authorised local,
  offline and synthetic boundary; this does not constitute a gate or homologation.
- ADR-0011: `accepted` by explicit human decision
  `ADR-0011: ACEITAR.` on 2026-08-09. The decision refines the evidence mapping
  of ADR-0004/ADR-0008; its semantic reconciliation in the named documentary
  owners was applied in corpus `4.10.10`. The internal serving correction
  was implemented in commit
  `b9c3e5f3a72c2dd7762c256198452ae2c217b2d2`; A0-003 subsequently made the
  four visual operations `PERMITTED` only under the notice-bearing mechanism
  and preserved external distribution as `DENIED`, without behaviour involving product
  data.
- ADR-0012: `accepted` by explicit human decision
  `ADR-0012: ACEITAR.` on 2026-08-09. The decision establishes the self-contained
  composite image and the necessary schema, migration and v2-contract changes.
  Its semantic reconciliation in the six documentary owners was applied
  in corpus `4.10.15`, and the protected revision of the v2 contract was frozen in
  corpus `4.10.16`. Schema and migrations were implemented in commit
  `98036f3c8c496544f4532d1fe48c981f836a1871`; notice-bearing behaviour
  was implemented in commit
  `f682827d1a26b08fa8c450a1fadb3bd0e1fa1700`. Its own Automatic Quality Gate
  was `APROVADO` under
  `AUTH-S07-A-NOTICE-BEARING-PROFILE-AQG-RETEST-001` only at the
  local, offline, deterministic and synthetic boundary, with no P0, P1, P2 or P3 finding.
  Subsequent A0-003 removed the rights blocker without rendering, materialising
  or activating PostgreSQL.
- ADR-0013: `accepted` by explicit human decision
  `ADR-0013: ACEITAR.` on 2026-08-10 at
  `main@f03162bad0fc166a597739b22e55fbc46ec59535`, corpus `4.10.17`. The decision
  selects `gpt-5.4-mini-2026-03-17` as the sole MVP LLM candidate,
  replaces only the previous LLM selection in ADR-0005 and keeps
  `gpt-5.6-sol` inactive for future evaluation. Its semantic reconciliation in
  ADR-0005, the `STATE-02` architecture report and architecture index
  was applied in corpus `4.10.19`. The local, offline and
  deterministic adapter-compatibility increment was implemented under
  `AUTH-STATE07-LLM-ADAPTER-COMPAT-001` in commit
  `b6d6f9102ecf0ea93309f8080acebad02cf16584` and factually reconciled in
  corpus `4.10.20`. The specific Automatic Quality Gate was `APROVADO`, with no
  P0, P1, P2 or P3 finding, under
  `AUTH-STATE07-LLM-ADAPTER-COMPAT-AQG-001` at baseline
  `main@6e6fdabb91e2fb4c5186c464ce08f5da390d727a`, corpus `4.10.20`. The evidence
  remains limited to Infrastructure, local offline tests and fake
  handlers; there was no operational configuration, account, credential, call to the
  provider, real corpus, OCI, deployment, real evaluation, Human Gate or lifecycle
  change. The local, offline and deterministic preparation of campaign
  `s07-a-provider-gpt54m-candidate-001` was completed under
  `AUTH-S07-A-PROVIDER-PREP-001` in commit
  `422286863e7a3c213e96db18144769bd0458a75b` and factually reconciled in
  corpus `4.10.22`. It materialises a synthetic, immutable and
  unscored successor revision, with a harness limited to fake handlers; it neither executes nor homologates
  the provider. The preparation-specific Automatic Quality Gate was
  `APROVADO`, with no P0, P1, P2 or P3 finding, under
  `AUTH-S07-A-PROVIDER-PREP-AQG-001` at baseline
  `main@5d74c9c9190b0b3465b11dc6864e3dd519cc88f9`, corpus `4.10.22`, only at the
  local, offline, deterministic boundary with fake handlers.
- ADR-0014: `accepted` by explicit human decision `ADR-0014: ACEITAR.` on
  2026-08-11 at `main@52e1ac7d9bc61be196549a8ee61399fde477b8fb`, corpus
  `4.10.26`, a clean working tree and protected OpenAPI v1/v2. The decision records
  the existing ordering `Score DESC, global ChunkOrdinal ASC`, preserves
  `retrieval-v1` for valid inputs, defines the typed and fail-closed Application
  retrieval-only port and establishes the governed design of the retrieval
  baseline. `retrieval-multi-query-v1-candidate` remains parked. Corpus
  `4.10.27` reconciles only that architectural authority under
  `AUTH-STATE07-RETRIEVAL-DETERMINISM-ADR-RECONCILE-001`. Under subsequent and separate human
  authority, granted at
  `main@ade89d737975f65c38e88b35758f8c6091e57406`, corpus `4.10.27`,
  `DR-2 — Determinism implementation` was completed in focused commit
  `fabb24cad16201070e3b95fffb22467cd55963ab`. Corpus `4.10.28` factually reconciles
  the typed Application retrieval-only port, complete fixed configuration,
  finite and total-order validations and fail-closed outcomes.
  The recorded focused evidence — build without warnings or errors, 74 focused unit
  tests, 8 local/SQLite integration tests, 11 architecture tests and an audit of 279
  files — did not constitute `DR-3` or an Automatic Quality Gate. Subsequently,
  under separate human authority at
  `main@272a868c2f2a90eba21ee422ba5a2c34aa2337d5`, corpus `4.10.28`,
  `DR-3 — Determinism Automatic Quality Gate` was executed locally, offline
  and deterministically and ended `REPROVADO`, with `DR3-FIND-001` P1 and
  `DR3-FIND-002`, `DR3-FIND-003` and `DR3-FIND-004` P2. The focused checks and full offline
  CI passed, but do not overcome the P1 numerical defect or the three
  P2 proof gaps. Dataset, campaign, provider, network, paid call, OpenAPI,
  schema, migration, Human Gate and lifecycle were neither executed nor changed;
  MultiQuery remains parked. After the versioned correction and its
  reconciliation, the independent corrective retest authorised by
  `AUTH-DR3-NUMERIC-SEMANTICS-AQG-RETEST-001` was `APROVADO` at
  `main@bf8a156e7c5eea801f29fb6e7742cac880783bc0`, corpus `4.10.32`, with no new
  P0, P1, P2 or P3 finding; `DR3-FIND-001` through `DR3-FIND-004` are `RESOLVED`.
- `RB-1 — Evaluation design freeze`: completed through documentation under
  `AUTH-RB1-EVALUATION-DESIGN-FREEZE-001` at clean baseline
  `main@45cbcf2624262572abf8180498ac63709a9130e4`, corpus `4.10.33`, with the
  four protected OpenAPI identities preserved. Immutable revision
  `retrieval-v2-evaluation-design-v1` is
  `frozen-unmaterialised-unscored` across 28 normative artefacts — eight design
  instances and 20 Draft 2020-12 schemas — bound by a closed inventory and
  SHA-256. The root contract has self-digest
  `0e8d928aee055211773d83eb33f2d54485033c81cfad15dd95b0fdd551f8ed08`,
  38 contract cells and 10 eligibility cells that are defined only, and all
  seven materialisation counters at zero. No product document/case,
  question, qrel, vector, generation, result or score was created. Build,
  executable tests, scorer, campaign, Automatic Quality Gate, Human Gate,
  lifecycle and external action remained `NOT_RUN`. At that baseline, `RB-2`
  remained unauthorised.
- `RB-2 — Dataset materialisation readiness` and `RB-3 — Campaign-input freeze`:
  subsequently materialised and mechanically frozen on the
  clean tracked baseline
  `main@0dbc415bad6532842aa6c4d1bb45ecd915bf5022`, corpus `4.10.41`, without a
  scored result. RB-2 revision
  `rag-eval-catalogue-v1-postgresql-18-rb2-20260814-001` is
  `frozen-unscored-rb2-complete`, bound to the materialisation-freeze manifest
  SHA-256 `daede1db869f7daf784fa2f3fc3b55e037cf4f3bb59a22f94e2026175858bfe4`,
  with 252 unique cases: 200 positive, 52 negative and a split of 126 `pt-BR`/
  126 `en-GB` questions. Tier `REPRESENTATIVE_HOMOLOGATION` qualifies only
  the unscored frozen denominator; it is not product homologation. The
  RB-3 campaign-input freeze
  `rag-eval-catalogue-v1-postgresql-18-rb3-20260814-001` is
  `frozen-unscored-rb3-complete`, bound to SHA-256
  `ac7b5763bc9e571b6365449b340c8256790c5fe57ba79142b592b854cf25303c`,
  with exactly one original vector per case, 252 vectors of 1,536 `float32`
  little-endian components and 6,144 bytes each. The receipt records one
  embedding materialisation completed without a retry and protected stores
  identical before/after. These mechanical integrity facts remain.
  However, the subsequent audit found that the checkpoint required two independent
  human reviews and human adjudication without agent decisions, while
  the retained package records `humanAttribution=false`, twenty agent
  adjudication decisions, no human decisions and, contradictorily,
  `no agent-authored adjudication`. The bytes and hashes were not changed, but
  literal `frozen-unscored-rb2-complete` does not prove gate satisfaction. The
  owner selected historical quarantine: RB-2 is permanently
  invalid in that revision, RB-3 is unavailable as RB-4 input and both
  remain intact only as historical evidence. Any successor
  requires separate authority, two independent human reviews and
  genuine human adjudication. `RB-4` remains `NOT_RUN` and blocked.
- ADR-0015: `accepted` by explicit human decision `ADR-0015: ACEITAR.` on
  2026-08-11 at `main@46de807148d5b547f56a0f7265b32428b232100f`, corpus
  `4.10.30`, a clean working tree and protected OpenAPI v1/v2. The decision selects
  `cosine-f32mul-f64acc-boundary-canonical-v1`, `retrieval-v2` and descriptor
  `/2`, with a new `IndexCompatibilityKey`, generation and evaluation baseline before
  serving; an exact 1 ULP corridor and binary64 scaled arithmetic
  remain unselected alternatives. Subsequently, under
  `AUTH-DR3-NUMERIC-SEMANTICS-IMPLEMENTATION-001`, commit
  `9addb166e82dd04581beee7b4276a74977fe04c5` implemented the semantics, policy,
  fail-closed compatibility and four proof corrections. The implementation
  neither created nor activated a product generation and, in that increment, did not repeat the
  gate: `DR-3` remained `REPROVADO`, with the four findings
  `CORRECTED_PENDING_GATE_RETEST`. The subsequent independent retest was
  `APROVADO` and disposed of the four findings as `RESOLVED`, preserving the previous
  historical evidence.
- Sanitised closure of the administrative provisioning key: clean-up
  completed under `AUTH-S07-A-PROVIDER-ADMIN-KEY-CLEANUP-002` was reconciled
  through documentation under
  `AUTH-S07-A-PROVIDER-ADMIN-KEY-CLEANUP-RECONCILE-001`, at
  `main@b2654088d11ab94c23cdf19e2aa57d89f0b3ae49`, corpus `4.10.24`, an initially clean
  working tree and protected OpenAPI v1/v2. According to the sanitised closure
  record provided by the owner, the Admin key with exact label
  `s07-a-provider-gpt54m-candidate-001-admin-provisioning` was revoked,
  is absent from the Active inventory and appears historically only as
  Inactive; `Last used` remained `Never` and spend remained `USD 0.00`. Target
  `RAG-Challenge/OpenAI/AdminKey/s07-a-provider-gpt54m-candidate-001`
  was removed from Windows Credential Manager and its absence was verified in the
  authorised clean-up. This reconciliation did not re-access those systems and did not
  retain a secret, fragment, fingerprint or masked representation. There was no
  provider or `/v1/responses` call, new cost, billing,
  limit, allowlist or project change, Human Gate or lifecycle.
- Reaudit of preflight and homologation boundaries: the initial operational preflight
  for `s07-a-provider-gpt54m-candidate-001` was finalised as
  `BLOQUEADO`, without a real campaign, provider or `/v1/responses` call. The
  subsequent clean-up of the Admin key and its local credential is closed. The
  subsequent experimental Coordinator/Docker/C3 flow was revoked and does not
  constitute current authority or a canonical pending item. The notice-bearing
  mechanism has existed since commit
  `f682827d1a26b08fa8c450a1fadb3bd0e1fa1700`, but does not retrospectively
  reclassify A0: the notice-bearing Automatic Quality Gate was
  `APROVADO` at the local, offline, deterministic and synthetic boundary and its
  result was reconciled in corpus `4.10.35`, with no P0, P1, P2 or P3 finding.
  A0-003 disposes of the four visual operations as `PERMITTED` only under the
  notice-bearing profile and keeps external distribution/publication `DENIED`; the
  candidate remains `ELIGIBLE_CANDIDATE`, without materialisation or activation.
- Product administrative composition: the complete retest authorised under
  `AUTH-S07-A-PRODUCT-ADMIN-COMPOSITION-AQG-RETEST-001`, at
  `main@e63f061d0bce4e48cd3b32294c20e29727cd7156`, corpus `4.10.36` and a clean
  tree, was `APROVADO` with no P0, P1, P2 or P3 finding. Inspection followed the
  path actually called by `Program`; selection by six fully qualified
  names executed exactly 6/6 tests, with fail-on-zero, and
  `eng/ci.ps1 -Offline` was executed exactly once only after that positive
  result. CI approved 202 unit, 208 integration,
  11 architecture and 45 Dashboard tests, with 95.58% line coverage and 68.07%
  branch coverage, a build without warnings or errors and an audit of 311 files. Approval is
  exclusively local, offline, deterministic and synthetic; no product
  materialisation or activation, dataset, RB-2, provider, network, Human
  Gate or lifecycle occurred.
- Project-owned notice-bearing disposition: the owner approved exactly,
  under
  `AUTH-S07-A-PRODUCT-ADMIN-NOTICE-BEARING-PROJECT-OWNED-DISPOSITION-PROPOSAL-001`,
  the six candidate values for `postgresql-18-reference-a4`:
  `attributionText="Source: The PostgreSQL Global Development Group; document:
  PostgreSQL 18.4 Documentation; version: 18.4; source reference:
  https://www.postgresql.org/files/documentation/pdf/18/postgresql-18-A4.pdf"`,
  `trademarkTreatment=Required`,
  `trademarkOrNonEndorsementText="Do not imply PostgreSQL project endorsement.
  No trademark permission is inferred from the documentation licence."`,
  `changeMarkingText="The composite PNG is a marked derivative, not a claim
  that its complete canvas is an unmodified publisher page. The source-page
  region nevertheless remains pixel-identical visual evidence."`,
  `assessedAt=2026-08-12T04:05:14.0000000+00:00` and
  `assessorId=assessor:auth-s07-a-product-a0-003`. These values are a project-owned
  control disposition, not primary evidence or a new legal
  conclusion. No deterministic identity, obligation set, manifest,
  generation, activation or bundle mutation was created or authorised.
- Under `SEC-CORR-ADR-PREP-01`, ADR-0018 and ADR-0019 were prepared on clean
  `main@334053e0101ce882767ccba29c69da7882917280`, corpus `4.17.1`, as
  `proposed` architecture records only. On 2026-08-16, the owner explicitly
  accepted both records through `ADR-0018: ACEITAR.` and
  `ADR-0019: ACEITAR.` on clean
  `main@89be70aba4de556611c9bdda8da62d1d4f9a1e41`, corpus `4.17.2`.
  ADR-0018 assigns `SEC-CORR-001` to persistent provider budget admission and
  explicit runtime-session rearming; the operational budget remains zero and
  disarmed, and no external price was consulted. ADR-0019 assigns
  `SEC-CORR-002` to a dedicated, attested Windows and Linux ARM64 PDF renderer
  sandbox. The pre-existing `SEC-001` dependency-audit finding remains
  unchanged. Acceptance establishes architecture authority only, accepts no
  risk and authorises no semantic reconciliation, implementation, public
  contract, schema, migration, dependency, provider, credential, network,
  billing, Human Gate or lifecycle action.

## Documentary baseline

- The 20 files in the originally approved structure remain preserved;
  the language policy added the 21st public document through a versioned
  increment, and ADR-0003 added the 22nd.
- The baseline approved at the `STATE-00` Human Gate remains `3.4.0`.
- The current instruction corpus is version `4.17.3` and has 13 files under
  `prompts/`.
- Vision, requirements, architecture, RAG, security, quality, lifecycle,
  roadmap, backlog, state, history and templates are documented.
- `STATE-02` added seven technical artefacts: four new ADRs, one
  canonical contract, one threat model and one execution report. ADR-0002 and
  ADR-0004 through ADR-0007 are accepted. The artefacts are not implementation
  evidence.
- The audit of the proposed package confirmed 83 non-ignored files, 30
  Markdown files, valid links and format, four ADRs with `proposed` status, 30 threat
  IDs and 12 security-test groups. Subsequent checks
  reconciled public facts about official sources, parser/package,
  provider/model and OCI without resolving account- or runtime-dependent facts and
  without replacing human decisions.
- The audit of corpus `4.1.0` confirmed 22 documents, 114 valid local links,
  20 RF, 14 RNF, 15 acceptance criteria, 31 backlog items, 8
  modules, 13 risks, consistent format and traceability. At that snapshot,
  implementation was still limited to the scaffold delivered by the closed
  `STATE-01`.
- Corpus `4.2.0` adds permanent decision-efficiency and proportionality rules to
  `AGENTS.md`: identify the deliverable before collection,
  separate decisive facts from context, calibrate depth to risk,
  verify candidates in two stages, prefer complete bounded authority,
  stop at diminishing value and fully preserve security, quality,
  lifecycle and explicit authority.
- Corpus `4.2.1` consolidates normative ownership without changing behaviour:
  Governance retains hand-off, continuity, reasoning and
  parallelism semantics; Templates retains the format; Quality Gates retains the
  auditable outcomes; AGENTS maintains minimum cross-cutting enforcement; Start
  Here maintains routing; Language Policy retains only language
  conventions.
- Corpus `4.10.7` records the owner’s permanent instruction to receive
  first a practical, concise explanation in plain language, suitable for
  someone without specialist technical knowledge. Necessary technical
  terms now require their meaning and consequence to be explained in
  `pt-BR`, without hiding uncertainty, risk, an authority boundary or an unverified
  fact.
- Corpus `4.10.8` records only the preparation of ADR-0011 as a proposal.
  The change documents the conditional mapping of primary evidence, the
  boundary between same-origin serving and distribution/publication, the obligations
  that accompany derivatives and the static incompatibility between the v2 contract
  and internal policy. It does not accept the ADR or change rights, public contract or
  product behaviour.
- Corpus `4.10.9` records explicit decision `ADR-0011: ACEITAR.` only
  as architectural authority. Acceptance does not execute semantic
  reconciliation, the internal policy correction, a new A0 or any change to
  rights, public contract, behaviour, gate or lifecycle.
- Corpus `4.10.10` applies the authorised documentary reconciliation of ADR-0011
  to ADR-0004, ADR-0008, the eligibility record and the v2 documentary
  contract. Reconciliation preserves the blocked PostgreSQL candidate, does not alter
  OpenAPI or behaviour and keeps the internal correction and new A0 under
  subsequent authorities.
- Corpus `4.10.11` records the internal correction subsequently implemented in
  commit `b9c3e5f3a72c2dd7762c256198452ae2c217b2d2`. The serving policy
  now assesses the ten decisions and fails closed when the distribution
  boundary is `Unproven`, without changing OpenAPI, public contract, candidate or
  lifecycle. A new A0 remains subsequent and separately authorised.
- Corpus `4.10.12` records the candidate-specific A0 authorised under
  `AUTH-S07-A-PRODUCT-A0-002`. Four visual operations remain `UNPROVEN`
  because no determined mechanism exists for the notices required in all
  copies, external distribution/publication is `DENIED` by the excluded
  internal boundary and the candidate remains `BLOCKED/EXCLUDED`.
- Corpus `4.10.13` records only preparation of ADR-0012 as a proposal.
  The new profile preserves the page region pixel-for-pixel and adds a
  self-contained obligations panel, with immutable record, manifest, recovery,
  serving and accessible presentation. The proposal identifies future
  schema, migration and v2-contract changes, preserves v1 and fail-closed behaviour and does not accept the
  ADR, reclassify the candidate or alter behaviour.
- Corpus `4.10.14` records explicit decision `ADR-0012: ACEITAR.` only
  as architectural authority. Acceptance does not execute reconciliation,
  v2-contract revision, schema, migration, implementation, new A0, renderer,
  dataset, gate or lifecycle and keeps OpenAPI v1/v2 protected.
- Corpus `4.10.15` applies the authorised semantic reconciliation of ADR-0012
  to the six documentary owners. It records the notice-bearing profile, the
  obligation set, its manifest/storage/reachability/recovery/
  serving/accessibility bindings and the mandatory future revisions to the v2 contract,
  schema and migration. It does not alter OpenAPI, implementation, candidate rights,
  gate or lifecycle.
- Corpus `4.10.16` records the frozen public notice-bearing v2 contract.
  OpenAPI v2 and its strict types/decoders add only the identity of the
  obligation set and its complete presentation; the route and all previous
  fields remain. Legacy compatibility uses `null` values, while
  the notice-bearing case fails closed on mixing, absence or divergence.
  OpenAPI v1, schema, migration, rights, dataset, gate and lifecycle do not change.
- Corpus `4.10.17` reconciles implementation of the schema and two notice-bearing
  migrations in commit `98036f3c8c496544f4532d1fe48c981f836a1871`.
  It records immutable obligation and ordered blocks, profile coexistence,
  obligation-set binding and digest, source/notice dimensions, constraints,
  foreign keys and fail-closed sealing triggers, without backfill or legacy mutation.
  Renderer, PNG, notice-bearing serving, Dashboard, rights, dataset, new A0,
  gate and lifecycle remain unchanged or `NOT_RUN`.
- Corpus `4.10.18` records explicit decision `ADR-0013: ACEITAR.` only
  as architectural authority. `gpt-5.4-mini-2026-03-17` becomes the sole
  MVP LLM candidate and `gpt-5.6-sol` remains inactive for future
  evaluation, with the moving-identifier risk recorded. Acceptance does not execute
  semantic reconciliation, implementation, account access, credential, provider,
  paid call, real corpus, OCI, deployment, gate or lifecycle and keeps OpenAPI
  v1/v2 protected.
- Corpus `4.10.19` applies under
  `AUTH-STATE07-LLM-CANDIDATE-ADR-RECONCILE-001` the documentary semantic
  reconciliation of accepted ADR-0013. ADR-0005 and the `STATE-02`
  architecture report now select `gpt-5.4-mini-2026-03-17`, preserve the previous
  selection as a historical fact and keep `gpt-5.6-sol` only as an inactive future
  candidate. No other ADR-0005 decision changes; code, tests,
  OpenAPI, configuration, provider, account, credential, paid call, real
  corpus, OCI, deployment, gate and lifecycle remain unchanged or `NOT_RUN`.
- Corpus `4.10.20` reconciles under
  `AUTH-STATE07-LLM-ADAPTER-COMPAT-RECONCILE-001` the increment implemented in
  commit `b6d6f9102ecf0ea93309f8080acebad02cf16584`. The adapter requires exact snapshot
  `gpt-5.4-mini-2026-03-17`, uses typed immutable configuration for
  `reasoning.effort=none` and `reasoning.context=current_turn`, preserves
  `store=false`, emits neither `tools` nor unproven parameters and strictly validates
  the final structured message. Local tests with a fake
  handler approved 18 of 18 cases, and the 11 architecture tests also
  passed. These results do not constitute a provider call, bilingual
  or quality evaluation, homologation, Automatic Quality Gate, Human Gate,
  deployment or lifecycle change.
- Corpus `4.10.21` reconciles under
  `AUTH-STATE07-LLM-ADAPTER-COMPAT-AQG-RECONCILE-001` the Automatic Quality Gate
  approved without findings at baseline
  `main@6e6fdabb91e2fb4c5186c464ce08f5da390d727a`. The audit confirmed all seven
  ADR-0013 requirements, 18 of 18 focused tests, 11 of 11
  architecture tests and complete offline CI with 154 unit, 191
  integration, 11 architecture and 45 Dashboard tests; 95.63% line
  coverage and 67.65% branch coverage; build without warnings or errors. Approval applies
  only to local, offline, deterministic compatibility with fake
  handlers. Real provider, bilingual evaluation, groundedness, citations,
  insufficient evidence, prompt injection, latency, cost, real corpus,
  OCI, deployment, Human Gate and lifecycle remain `NOT_RUN`.
- Corpus `4.10.22` reconciles under
  `AUTH-STATE07-S07-A-PROVIDER-PREP-RECONCILE-001` the preparation completed in
  commit `422286863e7a3c213e96db18144769bd0458a75b`. Successor revision
  `rag-eval-catalogue-v1-provider-gpt54m-candidate-001` preserves the previous
  frozen revision and records two synthetic documents, 60 cases, 40
  answerable cases distributed as ten for each required `pt-BR`/`en-GB`
  direction, 20 insufficiency cases and 12 prompt-injection cases.
  Prompt, schema, snapshot `gpt-5.4-mini-2026-03-17`, configuration, limits,
  maximum schedule of 109 calls, operational budget of `USD 16` and absolute
  ceiling of `USD 20` are frozen. The harness and tests used only
  fake handlers; real provider, bilingual quality, groundedness, citations,
  real insufficient evidence, prompt-injection resistance, latency,
  observed cost, Automatic Quality Gate, Human Gate and lifecycle remain
  `NOT_RUN`.
- Corpus `4.10.23` reconciles under
  `AUTH-STATE07-S07-A-PROVIDER-PREP-AQG-RECONCILE-001` the Automatic Quality Gate
  approved without findings at baseline
  `main@5d74c9c9190b0b3465b11dc6864e3dd519cc88f9`. The audit preserved the
  predecessor, confirmed the five manifests and their digests, the 60 cases, 12
  prompt-injection cases, 20 insufficiency cases, maximum schedule of 109
  calls and frozen `USD 16`/`USD 20` budget. 2 of 2 focused tests,
  20 of 20 combined tests and complete offline CI passed, with 154 unit,
  193 integration, 11 architecture and 45 Dashboard tests; coverage
  of 95.63% of lines and 67.66% of branches; build without warnings or errors. The
  approval applies only to local, offline, deterministic preparation with
  fake handlers. Account, credential, provider, paid call, real corpus/source,
  real evaluation, bilingual quality, groundedness, citations, real insufficient
  evidence, prompt-injection resistance, latency, observed cost,
  OCI, deployment, Human Gate and lifecycle remain `NOT_RUN`.
- Corpus `4.10.24` reconciles under
  `AUTH-S07-A-NOTICE-BEARING-PROFILE-IMPL-RECONCILE-001` the notice-bearing
  implementation from commit
  `f682827d1a26b08fa8c450a1fadb3bd0e1fa1700`. The immutable obligation set,
  PNG compositor with preserved page region, manifest
  bindings, persistence/reachability, fail-closed v2 readback/serving and the
  accessible Dashboard presentation are implemented. The observed focused evidence was
  a clean Release build, 47 unit, 40 integration/contract, 11
  architecture and 45 Dashboard tests, as well as Dashboard build and lint. This is not an
  Automatic Quality Gate. A new A0, product data, browser/assistive
  technology, Human Gate and lifecycle remain `NOT_RUN`; PostgreSQL remains
  `BLOCKED/EXCLUDED`.
- Corpus `4.10.25` reconciles under
  `AUTH-S07-A-PROVIDER-ADMIN-KEY-CLEANUP-RECONCILE-001` the closure completed
  under `AUTH-S07-A-PROVIDER-ADMIN-KEY-CLEANUP-002`. The sanitised record
  documents revocation and historical Inactive state of the named Admin key,
  `Last used` `Never`, spend `USD 0.00` and verified removal of the target from
  Windows Credential Manager. No secret is retained. The reconciliation does not
  re-access OpenAI, Credential Manager, provider, billing or project and does not
  execute a call, cost, configuration change, gate or lifecycle.
- Corpus `4.10.26` reconciles under
  `AUTH-STATE07-PREFLIGHT-BOUNDARY-REAUDIT-RECONCILE-001` the reaudits of
  tasks “Preflight operacional GPT-5.4-mini” and “Next Homologation Boundary”.
  The initial preflight remains closed as `BLOQUEADO`, without a real campaign;
  administrative and local clean-up is closed; and the experimental
  Coordinator/Docker/C3 flow is revoked, without authority or a canonical pending item.
  The notice-bearing mechanism implemented in `f682827d` does not alter historical
  A0. The notice-bearing Automatic Quality Gate, any reconciliation of
  its result and a new A0 remain separate and `NOT_RUN`.
- Corpus `4.10.27` records under
  `AUTH-STATE07-RETRIEVAL-DETERMINISM-ADR-RECONCILE-001` explicit decision
  `ADR-0014: ACEITAR.` only as architectural authority. Ordering
  `Score DESC, global ChunkOrdinal ASC` becomes an explicit contract without altering
  valid `retrieval-v1` results. In that reconciliation, the typed port,
  fail-closed failures, `retrieval-evaluation-scorer-v1` and the design,
  dataset and `campaign-input` freezes remained unimplemented future
  requirements; implementation, executable test, dataset, campaign, provider,
  network, paid call, OpenAPI, schema, migration, gate, Human Gate and lifecycle
  were `NOT_RUN`. MultiQuery remained parked.
- Corpus `4.10.28` factually reconciles
  `DR-2 — Determinism implementation`, separately authorised at
  `main@ade89d737975f65c38e88b35758f8c6091e57406` and implemented in commit
  `fabb24cad16201070e3b95fffb22467cd55963ab`. The typed Application retrieval-
  only port, fixed policy and generation identities, validation of query,
  vectors, norms, finite scores in `[-1, 1]`, global ordinal, identities and
  `Score DESC, global ChunkOrdinal ASC` ordering, as well as fail-closed outcomes,
  are implemented. `retrieval-v1` preserves top-k `8`, inclusive minimum
  `0.25`, maximum of six evidence items, budget of 16,000 scalars and score `0`
  for a stored zero-vector. The observed focused evidence was a build without warnings or
  errors, 74 focused unit tests, 8 local/SQLite integration tests, 11
  architecture tests and an approved documentary audit of 279 files. This does not
  execute or approve `DR-3`.
  Dataset, `retrieval-evaluation-scorer-v1`, campaign, provider, real corpus,
  network, paid call, OpenAPI, schema, migration, Human Gate and lifecycle
  remain `NOT_RUN`; MultiQuery remains non-canonical and parked.
- Corpus `4.10.29` factually reconciles
  `DR-3 — Determinism Automatic Quality Gate`, executed under separate human
  authority at `main@272a868c2f2a90eba21ee422ba5a2c34aa2337d5`, corpus
  `4.10.28`, and closed `REPROVADO`. `DR3-FIND-001` P1 records that identical
  admissible vectors `[1f, 1f, 1f]` produce score
  `1.0000000000000002`, converted to `InvalidIndexData` and
  `CH_INDEX_UNAVAILABLE`. `DR3-FIND-002` P2 records that the determinism
  test does not adversarially prove complete sorting before `Take(k)`;
  `DR3-FIND-003` P2 records absence of proof of filters before score/top-k
  with concurrent eligible and ineligible hits; and `DR3-FIND-004` P2 records
  absence of executable regression for `ChunkOrdinal < 0`. The observed
  implementation applies filters and then `OrderByDescending(Score)`,
  `ThenBy(ChunkOrdinal)` and `Take(k)`; the three P2 items are proof gaps, not
  observed behavioural defects.
  The gate recorded a Release build without warnings or errors; 74/74 focused unit,
  35/35 focused integration and 11/11 architecture tests; 3/3 independent
  executions of the tie/reopen case; and complete offline CI with 201 unit,
  197 integration, 11 architecture and 45 Dashboard tests, coverage
  of 95.53% of lines and 68.34% of branches and an audit of 279 files. These
  passing checks do not overcome the four findings. No tracked file,
  dataset, contract or configuration was changed by the gate; only ignored check
  outputs were materialised. No numerical semantics or
  correction was defined; dataset, scorer, campaign, provider, real corpus,
  network, paid call, OpenAPI, schema, migration, MultiQuery, Human Gate and
  lifecycle were neither executed nor changed.
- Corpus `4.10.30` materialises only architectural proposal ADR-0015 under
  `AUTH-DR3-NUMERIC-SEMANTICS-PROPOSAL-001`. The recommended alternative, not yet
  accepted, preserves binary32 multiplication, serial binary64 accumulation and
  bit-for-bit internal scores, but canonicalises finite quotients outside the codomain
  to `-1` or `+1`; it would require `retrieval-v2`, an advanced vector-store
  descriptor, new `IndexCompatibilityKey`, new generation and new evaluation
  baseline. The ADR preserves an exact 1 ULP corridor and scaled binary64 arithmetic as
  conditional alternatives and defines future executable proofs
  for the four findings. `DR-3` remains `REPROVADO`; decision, implementation,
  retest, dataset, scorer, campaign, provider, network, paid call, OpenAPI,
  schema, migration, MultiQuery, Human Gate and lifecycle remain separate and
  `NOT_RUN` in this increment.
- Corpus `4.10.31` records explicit decision `ADR-0015: ACEITAR.` only
  as architectural authority. Semantics
  `cosine-f32mul-f64acc-boundary-canonical-v1` canonicalises every finite quotient
  outside the codomain to the exact endpoint and preserves the internal bits; the
  successor policy is `retrieval-v2` and the selected descriptor is
  `sqlite-exact-vector-store/2;schema=1;distance=cosine;algorithm=exact-scan;vector=float32;score=cosine-f32mul-f64acc-boundary-canonical-v1`.
  A new `IndexCompatibilityKey`, new generation and new evaluation baseline are
  mandatory before serving. The 1 ULP and scaled binary64 alternatives
  were not selected. Acceptance did not implement code or tests, did not
  create a generation, dataset, scorer or campaign and did not execute provider, network,
  paid call, OpenAPI, schema, migration, MultiQuery, Automatic Quality Gate,
  Human Gate or lifecycle; `DR-3` remains `REPROVADO` with the four findings
  open.
- Corpus `4.10.32` factually reconciles the increment implemented under
  `AUTH-DR3-NUMERIC-SEMANTICS-IMPLEMENTATION-001`, at clean baseline
  `main@9735ff5bc243d9a517b2cceb7ca8bfe16f24b438`, by commit
  `9addb166e82dd04581beee7b4276a74977fe04c5`. The implementation materialises
  `cosine-f32mul-f64acc-boundary-canonical-v1`, `retrieval-v2` and descriptor
  `sqlite-exact-vector-store/2;schema=1;distance=cosine;algorithm=exact-scan;vector=float32;score=cosine-f32mul-f64acc-boundary-canonical-v1`;
  advances the internal compatibility key and fails closed for generation or
  `IndexCompatibilityKey` `/1`; preserves binary32 multiplication, serial
  binary64 accumulation, exact comparison and ordinal tie-breaking; and canonicalises only
  finite quotients outside the codomain to exact `-1` or `+1`. The corrective
  proof includes bit-for-bit boundaries and reopen, adversarial top-k with nine chunks
  and two permutations, concurrent filters before score/top-k and negative
  ordinal at the Application and task-owned SQLite boundaries. The implementation
  turn recorded a Release build without warnings or errors and 416 passing
  local/offline tests — 202 unit, 203 integration and 11
  architecture — without failures or skips; that evidence is not an Automatic Quality
  Gate. `DR3-FIND-001` through `DR3-FIND-004` are
  `CORRECTED_PENDING_GATE_RETEST`; `DR-3` remains `REPROVADO` until an independent
  retest and explicit disposition. No product generation, dataset,
  scorer, campaign, provider, credential, network, paid call, real corpus,
  OpenAPI, schema, migration, MultiQuery, Human Gate or lifecycle was created,
  activated, executed or changed.
- Corpus `4.10.33` reconciles under
  `AUTH-DR3-NUMERIC-SEMANTICS-AQG-RETEST-RECONCILE-001` the approved hand-off from the
  independent corrective retest executed under
  `AUTH-DR3-NUMERIC-SEMANTICS-AQG-RETEST-001` at clean baseline
  `main@bf8a156e7c5eea801f29fb6e7742cac880783bc0`, corpus `4.10.32`. `DR-3`
  is `APROVADO`, `DR3-FIND-001` through `DR3-FIND-004` are `RESOLVED` and no
  new P0, P1, P2 or P3 finding was identified. Release build, focused tests,
  three independent executions of the SQLite matrix, the solution’s 416 tests and
  complete offline CI passed; CI recorded another 45 Dashboard tests,
  95.53% line coverage, 68.47% branch coverage and an audit of 280 files.
  The evidence remains local, offline, synthetic and Windows x64. No tracked
  file was changed by the retest; product generation, dataset, scorer,
  campaign, provider, network, paid call, real corpus, OpenAPI, schema,
  migration, MultiQuery, Human Gate and lifecycle were not created, activated,
  executed or changed.
- Corpus `4.10.34` records under
  `AUTH-RB1-EVALUATION-DESIGN-FREEZE-001` the exclusively documentary completion
  of `RB-1 — Evaluation design freeze`. Immutable revision
  `retrieval-v2-evaluation-design-v1` freezes the unmaterialised and
  unscored design in 28 normative artefacts, with eight instances, 20 Draft
  2020-12 schemas, 27 companions bound by SHA-256, deterministic self-digest,
  formulae, thresholds, quotas, matrices, versioning, retention, gates, stop
  conditions and negative scope. The seven counters remain at zero. No
  product data/case, question, qrel, vector, generation, result, observed
  metric, scorer, campaign, provider, network, MultiQuery, Automatic Quality
  Gate, Human Gate or lifecycle was created, executed or changed. `RB-2`
  remains `NOT_RUN` and unauthorised.
- Corpus `4.10.35` reconciles under
  `AUTH-S07-A-NOTICE-BEARING-PROFILE-AQG-RECONCILE-001` the `APROVADO`
  result of
  `AUTH-S07-A-NOTICE-BEARING-PROFILE-AQG-RETEST-001`. The audit of 308
  files, Release build, two unit, ten integration, one
  architecture and 45 Dashboard tests, lint, typecheck and web build passed. Complete
  offline CI approved 202 unit, 203 integration, 11
  architecture and 45 Dashboard tests, with 95.53% line coverage and 68.47% branch coverage. The
  preflight found no project process or listener; the gate started
  and ended at the same clean baseline, with no P0, P1, P2 or P3 finding. Approval
  is only local, offline, deterministic and synthetic. The documentary
  reconciliation passed `git diff --check` and the audit of 308 files, without
  repeating build, tests or gate; it does not execute a new A0, product data, RB-2,
  provider, network, Human Gate or lifecycle.
- Corpus `4.10.36` records under `AUTH-S07-A-PRODUCT-A0-003` the new
  candidate-specific A0 for `postgresql-18-reference-a4`. The local identity
  matched at 15,771,040 bytes and SHA-256
  `cea7b845568095eb56dee1b51bfa145c6c6637bc4377c986019971577efefae4`. Page
  rendering, derivative-image creation, derivative-image retention and
  `RuntimeDerivativeImageDisplay` are `PERMITTED` only through profile
  `pdf-page-png-notice-v1`, with complete notices inside every PNG and all
  fail-closed bindings; external distribution/publication remains `DENIED`.
  The candidate leaves the rights blocker and remains `ELIGIBLE_CANDIDATE`,
  but no product artefact, RB-2, Human Gate or lifecycle was executed.
  The documentary reconciliation passed `git diff --check` and the audit of 308
  files, without executing build, tests or `eng/ci.ps1`.
- Corpus `4.10.37` reconciles under
  `AUTH-S07-A-PRODUCT-ADMIN-COMPOSITION-AQG-RETEST-RECONCILE-001` the
  `APROVADO` result of the administrative-composition retest executed under
  `AUTH-S07-A-PRODUCT-ADMIN-COMPOSITION-AQG-RETEST-001`. Selection by fully
  qualified names executed exactly 6/6 tests with fail-on-zero; the
  single subsequent execution of `eng/ci.ps1 -Offline` approved 202 unit,
  208 integration, 11 architecture and 45 Dashboard tests, coverage
  of 95.58% of lines and 68.07% of branches, a build without warnings or errors and an audit
  of 311 files. No P0, P1, P2 or P3 finding was identified; OpenAPI
  v1/v2 and the baseline remained intact. The reconciliation did not repeat build,
  tests, gate or CI and did not execute product materialisation/activation,
  dataset, RB-2, Human Gate or lifecycle.
- Corpus `4.10.38` records under
  `AUTH-S07-A-PRODUCT-ADMIN-NOTICE-BEARING-PROJECT-OWNED-DISPOSITION-RECONCILE-001`
  only the exact project-owned disposition of the six fields necessary for the
  future `DerivativeObligationSetV1` of `postgresql-18-reference-a4`. The
  decision keeps primary evidence and control wording separate,
  freezes the UTC anchor and stable assessor identity and does not calculate
  `rightsMappingRevision`, `obligationSetId` or `canonicalSha256`. Bundle,
  materialisation, rendering, embeddings, indexing, activation, AQG, Human
  Gate and lifecycle remain unchanged or `NOT_RUN`.
- Corpus `4.10.39` reconciles under
  `AUTH-S07-LOCAL-PRODUCT-TEXT-FIRST-CONTRACT-RECONCILIATION-PREFLIGHT-001`,
  at clean baseline
  `main@87f191c733715198451ee63da21ef24e121b0ac8`, the active RAG contract with the
  already-implemented text-first pipeline. PDF may join an activation with
  `renderManifestId=null` through `TextualEvidence`; complete visual mode
  remains supported through `PdfVisualEvidence`, and sparse manifests of
  one to five cited physical pages belong only to the persisted
  `AnswerEvidenceRecordV1`. PostgreSQL 18.4 generation
  `idxgen-ec39244b021c90fceea1b3a628fe793a99f74650cad451f16ffbcd414af636f6`
  is validated with 3,282 chunks and 3,282 vectors, no render manifest and entry
  `07-activate-generation.runtime.json` prepared with
  `renderManifestId=null`. The subsequent single execution of
  `activate-generation` applied active revision `1`; query and Responses were not
  executed.
- Corpus `4.10.41` preserves reconciliation of PostgreSQL activation and records
  private publication and the single deployment of the Render Hobby/Free candidate.
  The private GHCR image is fixed by digest
  `sha256:536e431126470a51370bf9aeb4c769ff1d75313c67643c3922cf0fd2e2688c08`.
  Service `rag-challenge` uses the `Free` plan, one instance, autoscaling
  disabled, no persistent disk and no Render database. The single deployment is
  `Live`; public liveness and readiness returned HTTP 200, with the expected
  PostgreSQL generation, one active database, one eligible document and none degraded.
  No query, Responses or embedding call was executed. Billing
  remained without a card or services, with monthly total and projection at `USD 0.00`. The deployment is
  public homologation evidence in `STATE-07`; it does not replace ADR-0005, does not
  satisfy the OCI requirement by itself and does not authorise `STATE-08`.
- Corpus `4.10.42` factually reconciles the RB-2 and RB-3 freezes already
  completed at `main@0dbc415bad6532842aa6c4d1bb45ecd915bf5022`, corpus
  `4.10.41`. It records the 252-case RB-2 revision and its
  materialisation-freeze manifest SHA-256
  `daede1db869f7daf784fa2f3fc3b55e037cf4f3bb59a22f94e2026175858bfe4`,
  as well as the RB-3 campaign-input freeze SHA-256
  `ac7b5763bc9e571b6365449b340c8256790c5fe57ba79142b592b854cf25303c`
  with 252 vectors. It preserves no result, scorer or Responses campaign,
  `RB-4` and Human Gate; denominator qualification is not product
  homologation and does not change lifecycle.
- Corpus `4.13.1` records the human decision after documentary assessment
  of the locked SDK: ADR-0016 remains `accepted` and not `superseded`,
  `FakeAgentRunner` remains the only validated operational baseline, no
  ADR-0017 was created or accepted and `NEW_REAL_START` remains
  `ARCHITECTURE_CHANGE_REQUIRED`. The assessment did not verify an additional SDK
  version and did not execute network, Codex, provider, credential, billing, Human Gate,
  lifecycle, production, push, merge or release.
- Corpus `4.13.0` records the subsequent bounded authority, acquisition
  of the exact `@openai/codex-sdk` `0.147.0` graph, implementation and validation
  of Stage 2. The orchestrator remains development tooling outside the
  solution and product runtime. The clean gate, dry run, controlled E2E and
  independent reviews passed; readiness is
  `MULTI_AGENT_READY_WITH_CONDITIONS` only for the local fake on this host; Codex
  resume is non-operational contract evidence and a new real Codex thread
  remains `ARCHITECTURE_CHANGE_REQUIRED`. On a host where file symlink creation
  is permitted, both leaf-symlink tests must pass before
  carrying the same classification to that host.
- Corpus `4.12.1` historically recorded the dependency preflight at
  `60ccbdc`: Node `24.19.0` and npm `11.17.0` were available, but the local cache and
  inventory contained no `@openai/codex-sdk`. Without authority then
  current for acquisition from the npm registry, the lockfile and runner tests could not
  be produced; readiness at that boundary was
  `HUMAN_DECISION_REQUIRED`. Subsequent authority satisfied that condition
  without rewriting the historical fact.
- Corpus `4.12.0` records the selected historical quarantine for the
  RB-2/RB-3 freezes, without in-place editing, and the explicit architectural acceptance of
  ADR-0016. RB-2 remains invalid, RB-3 unavailable for RB-4 and any
  successor requires separate authority, two independent human reviews and
  genuine human adjudication. Objective reassessment results in
  `READY_FOR_STAGE_2` only for local, offline-first implementation of the
  orchestrator under the original authority and the owner’s explicit
  resumption; it does not authorise RB-4, provider, secret, network, billing,
  production, release, Human Gate or lifecycle.
- Corpus `4.11.1` makes British English
  (`en-GB`) mandatory in every new project-owned commit message, including
  subject, body and footer. Types/scopes, identifiers and external literals
  preserve their required spelling. The rule must be checked before the commit and
  does not grant amend, rebase or history-rewrite authority by implication.
- Corpus `4.11.0` adds the verifiable multi-agent development
  playbook, configures six project-scoped roles, corrects fail-open
  coverage without branches, prepares ADR-0016 as `proposed` and records the
  RB-2 authority reaudit. It fully preserves the freezes and does not
  accept an ADR, execute Stage 2, alter the product, Human Gate or lifecycle.
- Corpus `4.3.0` formalises the owner’s explicit decision to accept
  questions and answers in `pt-BR` and `en-GB`: each query declares its language,
  the answer uses the same language, source-derived text remains in the
  citation’s original language and the test matrix covers matching pairs and the
  two cross-directions. The decision does not define the interface language and does not
  accept the still-proposed ADRs.
- Corpus `4.4.0` formalises the subsequent and independent decision to support the
  interface in `pt-BR` and `en-GB`, with an explicit visual choice and localisation
  of product-owned text. The visual language neither alters nor is
  inferred from `questionLanguage`, `answerLanguage` or `contentLanguage`; the
  citations remain in the source’s original language. Initial language,
  preference persistence and fallback remain for a future frontend
  decision. The decision does not accept the still-proposed ADRs.
- Corpus `4.5.0` formalises the subsequent decision to support `Light`
  and `Dark` themes, with an explicit choice independent of interface and query
  languages. The four-combination matrix between `interfaceLanguage` and
  `questionLanguage` must be executed in both themes. Initial theme,
  system preference, persistence and fallback remain for a future
  frontend decision. The decision does not accept the still-proposed ADRs.
- Corpus `4.6.0` formalises the independent decision not to impose a product
  ceiling on the number of systems or corpus pages. Each version
  remains finite and records the observed counts; security and
  capacity controls are conditional on the corpus and environment, not a fixed
  coverage subset. The decision does not accept ADR-0004.
- Corpus `4.7.0` formalises the subsequent decision to use the 51 exact names
  provided by the owner as the canonical initial catalogue, in 9 categories
  and 54 many-to-many associations. Each active database requires at least one
  active PDF and/or CSV document; there is no document ceiling. All active/current
  documents participate in unified retrieval, while local/official
  origin remains provenance. Compatible items are administrable
  without hard-coding, code or an ADR per item; new integration classes retain
  their own decision. Nothing was implemented, ingested, indexed or activated; in that
  increment, all four ADRs still remained proposed.
- Corpus `4.8.0` records the explicit and independent acceptance of the four
  ADRs on the reconciled baseline. ADR-0005 keeps OCI, package
  versions and operational targets conditional, with consistent backup,
  read-only primary instance, disclosure limited to OpenAI and blocking
  of new indexing when the mutable alias drifts. Acceptance does not install,
  execute, test or authorise any of those components.
- The authorised combined audit of `STATE-02` confirmed the mechanical
  baseline — 83 non-ignored files, 30 Markdown files, 13 files under
  `prompts/`, identical 51/54/9 catalogue, 25 RF, 18 RNF, 20 criteria, 19 Must
  items, 36 threats and 15 test groups — but rejected the gate because of
  `AQG-S02-001` (P1), `AQG-S02-002` (P2) and `AQG-S02-003` (P3). No finding
  was silently corrected.
- Corpus `4.8.1` records the corrective package without a new architectural decision:
  ADR-0007 compares identity models and recommends excluding
  `sourceObservationId` from the generation, protecting the complete binding with
  `activationBindingSetDigest`. Threat model, vision, architecture and security
  were reconciled with the already-accepted facts for `AQG-S02-002` and
  `AQG-S02-003`. At that snapshot, the decision was still pending; the gate was not
  repeated and its historical findings were not reclassified by
  inference.
- Corpus `4.9.0` records the explicit acceptance of ADR-0007. The decision makes
  authoritative the separation between generation identity and activation-record
  identity and replaces only the conflicting identity and rollback clauses of
  ADR-0002. At that snapshot, semantic reconciliation was
  still pending and the gate remained rejected.
- Corpus `4.9.1` applies the accepted reconciliation: `sourceBindingSetDigest`
  excludes `sourceObservationId`; `activationBindingSetDigest` protects the complete
  binding; `catalogueRevision` remains separate from the observation journal;
  `304`/identical hash preserves manifest and generation; query filters eligible
  bindings before top-k; rollback builds a new record with compatible
  and currently eligible observations. ADR-0002, canonical contracts,
  solution architecture, RAG module, requirements, lifecycle, Quality Gates,
  roadmap, threat model and factual records now converge. The directed
  validation neither repeated the gate nor proved implementation. The subsequent new combined
  audit disposed of `AQG-S02-001`, `AQG-S02-002` and `AQG-S02-003`
  as `RESOLVIDOS` and approved the documentary baseline, without proving
  implementation.
- Corpus `4.9.2` corrects thematic isolation of responses and hand-offs:
  confirmation, clarification or a bounded follow-up remains within the current
  request; `Próximo trabalho recomendado` does not import lifecycle, backlog or an
  optional improvement without a direct relationship and uses canonical absence when no
  pertinent additional work exists. The correction does not alter the product,
  lifecycle, authority or executable state.
- Corpus `4.9.3` records `NORM-S06-001`: `STATE-06` retains the factually
  current README and at least one genuinely verified example in the local/synthetic
  integrated artefact, while `STATE-08` retains its public
  finalisation with separately verified OCI evidence and real product
  execution. The change eliminates the ownership divergence without altering the
  lifecycle order, disposing of the findings or repeating the gate.
- Corpus `4.9.4` corrects enforcement of the next recommended work:
  every hand-off reports exactly one concrete, prioritised and directly
  related action, with an owner and condition/authority. A completed request,
  waiting project or lack of authority does not justify omission when a datum,
  document, decision or authority can still unblock continuity.
  Canonical absence is restricted to a genuine lack of actionable
  continuation and does not permit importing unrelated lifecycle or backlog.
- Corpus `4.9.7` corrects a subsequent recurrence: generic review of
  commits or completed results cannot replace the first still-incomplete
  item in a dependency order. When authority for
  that item is absent, obtaining it from the owner is the next action and the hand-off provides the
  bounded payload. Direct questions about the next step receive the action
  before the recap.
- The `STATE-02` Human Gate was confirmed in the same conversation that presented
  the complete summary of current baseline `main@6e61c4c`, corpus `4.9.1`. The
  decision accepted the documentary architecture without reservations, preserved all
  stated limitations and residual risks and did not authorise `STATE-03` or an external
  action.
- The architecture adopts principles compatible with DB-Notifier without creating a
  reference or dependency between the projects.
- The `STATE-00` Human Gate was confirmed in the coordinating conversation that
  contained the complete summary of baseline `3.4.0`; the decision does not accept an ADR,
  decide `GATE-B01` or authorise `STATE-01`.
- `GATE-B01` was confirmed in the coordinating conversation that contained the complete
  current summary. The decision accepted ADR-0001, selected the MIT licence
  with the exact notice
  `Copyright (c) 2026 Bruno Araújo - DegsTerin.`, consolidated RAG abstractions
  in Application and persistence in Infrastructure, approved the
  `CH-MOD-*` map, architectural dependencies/tests and one-shot administrative
  mode in the primary host.
- Approval of `GATE-B01` did not create the licence, solution or projects, did not
  accept ADR-0002 and did not authorise `STATE-01`.
- Local Git exists. Governance reorganisation `4.2.1` was executed
  sequentially in the coordinating conversation, without parallel lanes; that
  execution was exclusively documentary and its runtime preflight remained
  `NÃO APLICÁVEL`.
- The continuity policy, enforcement, routing, criteria and templates
  remain current in their thematic authorities; the snapshot
  does not redefine them.

## Workspace

- `.gitignore` excludes `reference-materials/`.
- `reference-materials/` preserves 27 local files: 23 original Challenge
  materials, 1 generic governance prompt archived unchanged and 3 historical
  Stage owner inputs preserved under `governance-inputs/`.
- `reference-materials/challenge-original/` retains 8 Markdown files, 14 PDFs and 1
  PNG.
- The original materials are not the product corpus and will not be sent to
  GitHub.
- An initialised local Git repository exists on branch `main`; the scaffold is
  in commit `16aec5f8586f07c9a9d89165e330335b460d6fbf` and the npm lockfile in
  commit `8a604ceaa34162673aea6b7ce3267bc9d3f8b83a`; the technical identity
  migration is in commit
  `8c347c0fa73fead3e03a1eb979deba9fe3617379`.
- `RAG-Challenge.sln`, four production .NET projects under the
  `RagChallenge` prefix, a React/TypeScript boundary for the Dashboard and three
  .NET test projects exist, in accordance with ADR-0003. Domain and Application contain the
  model, canonical identities and persistence ports; Infrastructure
  contains local SQLite persistence, immutable content, catalogue, snapshots,
  observations, generations, activations, leases and administrative journal. The
  local `STATE-04` backend implements one-shot administration, PDF/CSV ingestion,
  synchronisation through a controlled transport, chunking, indexing, retrieval,
  grounded generation and public v1 API; the external paths remain
  fail-closed and were exercised only with fakes and synthetic fixtures.
- .NET SDK `10.0.302` and C# `14.0` are pinned. The Dashboard supports Node.js
  `>=24.18.0 <25` and npm `>=11.16.0 <12`, with enforcement through `devEngines`;
  `.nvmrc` selects lower bound `24.18.0`. NuGet uses central management and
  seven lockfiles reproduced offline.
- The historical setup gate approved locked offline .NET restore, format,
  Release build, 15 tests and coverage of 88% of lines/100% of branches.
  `S03-A` was verified without restore or installation: 68 tests passed and
  coverage was 95.55% of lines/89.93% of branches.
- `S03-B5` repeated the offline aggregate with 82 passing tests and coverage of
  94.83% of lines/72.34% of branches and Control/Vector migrations without a pending
  change. CI explicitly normalises to LF only the seven tracked NuGet
  lockfiles that restore can reserialise on Windows.
- The Dashboard has `package-lock.json` v3 and passed clean install without
  lifecycle scripts, lint, two structural tests, typecheck and Vite build.
- npm and .NET audits found no vulnerabilities in the current sources.
- The clean clone of the renamed baseline, without `reference-materials/`, reproduced
  locked restore, format, build, 15 tests, coverage, Dashboard and hygiene;
  liveness and readiness returned `200` on loopback, and the project-owned listener
  was stopped.
- The temporary clone from that reproduction remains in the system temporary
  directory because execution policy refused its recursive removal. It does not
  contain `reference-materials/`, a secret or an untracked change.
- The checkout’s physical directory, external to Git, was manually renamed
  to `RAG-Challenge`; no sibling `Challenge` directory exists.
- The seven legacy technical trees, which contained no files, were
  removed after target validation. The 15 ignored build and test roots
  that retained the previous absolute path were also removed as
  reproducible artefacts.
- Subsequent .NET checks transiently recreated 14 canonical
  `bin/` and `obj/` roots, without the previous path; a second pass
  removed those outputs. In the final snapshot, `bin/`, `obj/` and `TestResults/`
  are absent from all seven projects.
- No active technical file or path retains the previous prefix or absolute
  path. At that historical path-migration boundary, `reference-materials/`
  remained ignored and fully preserved its then-current 24 files; historical,
  external and provenance uses remain deliberately unchanged.
- The CI pipeline is defined with least privilege and without deployment. This audit
  did not query the remote GitHub run history; remote CI
  state was not revalidated.
- The `LICENSE` file materialises the approved MIT licence.
- Functional API, ingestion, retrieval, SQLite persistence and exact vector store
  exist. PostgreSQL 18.4 `LocalAuthorised` is active at revision `1`, and
  the private image was published to GHCR and deployed once to the
  Render Free service described above. This does not constitute production, does not by
  itself satisfy the OCI requirement and does not authorise `STATE-08`.
- The private GHCR package and Render Free service are already-observed external
  resources; no remote action was executed by this audit.
- DB-Notifier remained read-only.

## Current product scope

- Independent RAG application for database documentation.
- MVP with one logical corpus, initial catalogue of 51 databases, 9 categories and 54
  associations, administrable through records and without hard-coding.
- Each active database has at least one active PDF/CSV document and may have
  any additional number.
- No product ceiling on the number of systems or corpus pages;
  each finite version records its counts and must fit safely within the
  homologated environment without silently reducing the approved catalogue.
- Unified retrieval of all active/current documents; local or official
  origin remains explicit in provenance, coverage and citations.
- Manual and governed synchronisation of each registered official source into a
  versioned snapshot.
- Public official sources without credentials, deny-by-default egress by exact
  URL and TLS validation without unauthorised lateral destinations.
- Grounded response with citations and explicit insufficient evidence.
- Questions and answers with explicit `pt-BR` or `en-GB` language, answer in the
  question language and citations preserved in the source’s original language.
- The accepted architecture separates closed `SupportedQueryLanguage` in `pt-BR` and
  `en-GB` from BCP 47 `DocumentContentLanguage`; preserves the exact
  `sourceDeclaredLanguage` declaration and does not convert `en` to `en-GB`. The v1 runtime still
  retains the implemented closed model.
- Interface with explicit choice between `pt-BR` and `en-GB`, independent of the
  question, answer and evidence languages.
- Interface with explicit choice between `Light` and `Dark`, independent of the
  interface, question, answer and evidence languages.
- Candidate/Active/Deactivated/Removed lifecycle for databases and documents,
  manual versioning and a new candidate generation.
- Immutable/reopenable raw content, non-queryable staging, intact final
  manifest, generation-bound digest separate from the complete activation digest, and
  activation/rollback through a new revision of the complete versioned record.
- The accepted architecture makes `IDocumentContentStore` the permanent authority for
  content-addressed source and PNGs, requires a complete PDF render manifest, specific
  rights and visual serving bound to the citation. The executable content-store
  boundary, rights contracts/gates, renderer/PNG, manifest finalisation
  and atomic activation bindings are implemented in
  separate corrective increments. `AnswerEvidenceRecordV1`, its retention and
  reachability are also implemented locally. The HTTP/OpenAPI v2
  contract and same-origin visual serving were implemented and approved by the
  Automatic Quality Gate at the local, offline, deterministic and
  synthetic boundary recorded above.
- Versioned RAG-Challenge-owned HTTP/OpenAPI v1 contract; consuming
  adapters remain outside this repository.
- HTTP/OpenAPI v1 and v2 contracts coexist. V2 projects BCP 47 document
  language and same-origin visual evidence; both OpenAPI artefacts remain
  preserved byte-for-byte.
- Local execution and future OCI deployment.
- GitHub Pages only as an optional static frontend.

## Inactive future capabilities

- multiple corpora;
- formats beyond PDF and CSV;
- scheduled incremental synchronisation;
- generic crawling and new source/authentication classes;
- multiple providers;
- RBAC/multi-tenancy;
- DB-Notifier integration.

None of these capabilities is implemented, tested, deployed or
authorised.

## Pending future evidence and decisions

1. A0-003 for `postgresql-18-reference-a4` confirmed identity, provenance,
   languages and the grant already recorded under ADR-0011. The ADR-0012 mechanism,
   implemented in `f682827d1a26b08fa8c450a1fadb3bd0e1fa1700` and approved at the
   notice-bearing gate, carries the copyright notice, permission paragraph and the
   two complete disclaimers inside every PNG. Page rendering,
   derivative-image creation/retention and runtime display are `PERMITTED`
   only under that profile and its fail-closed conditions; external distribution
   remains `DENIED`. The document was imported and text-first generation
   `idxgen-ec39244b021c90fceea1b3a628fe793a99f74650cad451f16ffbcd414af636f6`
   was materialised, validated and activated at revision `1`, with 3,282 chunks,
   3,282 vectors and no render manifest. Activation preserves
   `renderManifestId=null`. Each subsequent document maintains its own independent
   rights, provenance and language gate.
2. Individually validate and activate new official-source records; architectural
   acceptance does not authorise a URL, network, download or crawling.
3. `S07-A` A1-A5 and its Automatic Quality Gate were completed and approved at the
   authorised local, offline, deterministic and synthetic boundary. The
   v2 integration, restart, confined cold backup/restore and limits were also
   implemented; their subsequent Automatic Quality Gate was `APROVADO` at the
   corresponding synthetic boundary, and `AQG-S07-V2-IR-001` is `RESOLVIDO`.
   Product homologation and the provider, source, browser,
   dynamic-security, load, operational-recovery, accessibility, Linux,
   OCI and production boundaries remain `NOT_RUN`; every other `STATE-07` batch
   remains unexecuted and unauthorised.
4. Synthetic preparation of campaign
   `s07-a-provider-gpt54m-candidate-001` is frozen in commit
   `422286863e7a3c213e96db18144769bd0458a75b`; its Automatic Quality Gate was
   `APROVADO` without findings only at the local, offline, deterministic boundary
   with fake handlers. There was no real call or scored result. It is still
   necessary to verify tier, entitlement, spend limit and OpenAI account
   controls, as well as bilingual retrieval/generation, before using or announcing the
   providers.
5. Homologate performance and capacity of `SqliteExactVectorStore`; the
   functional 10,000 × 1,536 fixture passed, but is not a benchmark, SLA or product
   ceiling.
6. Test comprehensive process-crash boundaries in `STATE-07`; restart and cold
   backup/restore of the task-owned v2 store already have local, synthetic and
   confined evidence, without representing operational storage or recovery.
7. Verify real capacity, entitlement, IAM, restore, cost and billing for the
   OCI tenancy; public sources still diverge on the free allowance.
8. The bytes, hashes, 252 cases and 252 vectors in the RB-2/RB-3 freezes remain
   preserved and unscored. The reaudit found an incompatibility between
   the human review/adjudication requirement and the recorded actors/decisions.
   Under the selected historical quarantine, mechanical completeness does not satisfy the
   gate: RB-2 remains invalid, RB-3 cannot feed `RB-4`, and any coherent
   successor requires separate authority, two independent human
   reviews and genuine human adjudication.
9. `S03-CORR-01` completed the first item in the dependency order.
   `S04-CORR-04-A` completed verified content-store descriptors and
   `S04-CORR-04-B` completed rights contracts/gates;
   `S04-CORR-04-C` completed deterministic rendering and verified
   candidate finalisation; `S04-CORR-04-D` completed persistence and atomic
   activation of the source, rights, generation and manifest bindings; and
   `S04-CORR-04-E` completed `AnswerEvidenceRecordV1`, fixed `P30D` retention and
   reachability at the local/offline boundary. The v2 contract was frozen, its
   implementation and same-origin visual serving were completed and the corresponding Automatic
   Quality Gate was `APROVADO` at the local, offline,
   deterministic and synthetic boundary. OpenAPI v1 and v2 remain protected. The
   v2 integration, restart, confined cold backup/restore and limits were
   completed in commit `e5dae7ee5a786417fba2c6ef0555686816b0b330`; the
   focused correction is in commit
   `f6c648c40cf8d0280cfceca5509a381bddb9fc8f`, and its own Automatic Quality Gate
   was `APROVADO` without a new finding, with `AQG-S07-V2-IR-001`
   `RESOLVIDO`. The real browser/assistive-technology, data, renderer,
   provider, source and network, load, comprehensive crash-injection,
   operational-recovery, Linux, OCI and production boundaries remain `NOT_RUN`.
10. ADR-0018 and ADR-0019 are `accepted` through the owner's explicit decisions
    `ADR-0018: ACEITAR.` and `ADR-0019: ACEITAR.`. Acceptance establishes
    architecture authority only and accepts no risk. Separate authorities
    remain required for semantic reconciliation, persistence, schema,
    migration, implementation, dependencies, platform-specific tests, budget
    arming, provider access and operational evidence.

## Next authority

The owner explicitly accepted ADR-0018 and ADR-0019 as architecture authority
only. No semantic reconciliation or implementation authority has been granted.
The directly related next authority, if pursued, is a bounded documentary
semantic reconciliation of both accepted decisions into their security and
threat-model owners before implementation planning. It would not arm a
provider budget, accept risk or authorise schema, migration, code, dependency,
renderer, platform test, provider, network, Human Gate or lifecycle activity.

Stage 2 is implemented and validated for deterministic coordination with
`FakeAgentRunner` and the Codex App Server runner selected by ADR-0017. The
CLI exposes real start and resume under `--runner codex`, but remains deny-by-default:
each execution requires a closed plan, a clean Git baseline on a `codex/` branch,
`--authority-reference`, sandbox per task, `never` approval, agent network
denied and valid local ChatGPT state. The product provider credential remains outside this
boundary.

On 2026-08-15, ADR-0017 was accepted with the explicit phrase
`ADR-0017: ACEITAR.` and implemented with the stable App Server. Sanitised
authentication confirmed only `chatgpt` mode; `thread/start` returned the
identity before `turn/start`; the checkpoint was persisted before the turn; the
structured result was validated; and the controlled real execution ended
`PASS`. The previous `ARCHITECTURE_CHANGE_REQUIRED` condition for
`NEW_REAL_START` is resolved by ADR-0017 and does not apply to the current runner.

No implementation remains directly related to activating Stage 0,
Stage 1 and Stage 2. The next orchestrator execution, when concrete development
work exists, must receive a new bounded envelope from the
owner and a closed plan; readiness does not grant continuous authority.
Product provider, production, push, merge, release, Human Gate and lifecycle
change remain outside this activation.

A successor to RB-2/RB-3 remains outside this execution and requires separate
authority, two independent human reviews and genuine human adjudication. The
current freezes cannot be edited or consumed by RB-4; `RB-4` remains
blocked and `NOT_RUN`.

## Contexto histórico preservado

Os parágrafos seguintes registram snapshots e sequências anteriores. Verbos no
presente dentro deles descrevem a baseline nomeada no próprio parágrafo e não
substituem o snapshot vigente acima.

O ADR-0013 foi aceito explicitamente mediante `ADR-0013: ACEITAR.` somente
como autoridade arquitetural. Ele seleciona `gpt-5.4-mini-2026-03-17` para o
MVP e mantém `gpt-5.6-sol` apenas como candidato futuro inativo. A reconciliação
semântica documental foi concluída sob
`AUTH-STATE07-LLM-CANDIDATE-ADR-RECONCILE-001`, e o incremento local, offline e
determinístico de compatibilidade do adaptador foi implementado sob
`AUTH-STATE07-LLM-ADAPTER-COMPAT-001` no commit
`b6d6f9102ecf0ea93309f8080acebad02cf16584`. O Automatic Quality Gate
específico foi `APROVADO` sem achados sob
`AUTH-STATE07-LLM-ADAPTER-COMPAT-AQG-001`, somente na fronteira local, offline,
determinística e com handlers falsos. Isso não constitui avaliação real nem
homologação. A campanha `s07-a-provider-gpt54m-candidate-001` possui preparação
sintética congelada sob `AUTH-S07-A-PROVIDER-PREP-001` no commit
`422286863e7a3c213e96db18144769bd0458a75b`, com agenda e orçamento somente
planejados e zero execução real. Seu Automatic Quality Gate foi `APROVADO`, sem
achados, sob `AUTH-S07-A-PROVIDER-PREP-AQG-001`, exclusivamente na fronteira
local, offline, determinística e com handlers falsos. Qualquer avaliação com
conta, credencial, provider, chamada paga, corpus real ou OCI exige autoridade
humana separada; Human Gate e lifecycle permanecem sem alteração.

`S07-A` A1-A5 e seu Automatic Quality Gate estão concluídos e aprovados na
fronteira sintética local autorizada, com `S07-A-FIND-001` e o histórico
`S07-A-FIND-004` ainda abertos nas disposições acima. Isso não satisfaz a
homologação de produto nem prepara Human Gate: qualquer continuação deve nomear
e autorizar separadamente a fronteira ainda `NOT_RUN`, sem inferir avanço de
lifecycle.

O A0 candidato-específico de `postgresql-18-reference-a4` foi novamente
executado sob `AUTH-S07-A-PRODUCT-A0-003` após o gate notice-bearing aprovado.
A concessão oficial registrada foi aplicada operação por operação sem inferir
suficiência de apresentação apenas contextual. Page rendering,
derivative-image creation, derivative-image retention e runtime derivative
display são `PERMITTED` somente pelo `pdf-page-png-notice-v1`, com avisos
completos dentro de cada PNG e ligações fail-closed. A intended
source/derivative distribution boundary permanece `DENIED` fora do
runtime-display. O A0 removeu o bloqueio de direitos e tornou o documento
`ELIGIBLE_CANDIDATE`; a materialização text-first posterior importou e indexou
o conteúdo em geração validada, sem renderização prévia. A ativação posterior
foi executada sob autoridade própria; ela não foi autorizada por esse resultado
de direitos.

O ADR-0012 foi aceito explicitamente mediante `ADR-0012: ACEITAR.` somente
como autoridade arquitetural, e sua reconciliação semântica nos seis
proprietários documentais foi concluída sob
`AUTH-S07-A-NOTICE-BEARING-PROFILE-RECONCILE-001`. Ela registra o mecanismo
autocontido. A revisão protegida do contrato v2 foi congelada sob
`AUTH-S07-A-NOTICE-BEARING-V2-CONTRACT-001`, preservando v1 e a rota v2. Isso
não reclassifica o candidato. O schema e as migrations foram implementados no
commit `98036f3c8c496544f4532d1fe48c981f836a1871`, preservando registros legados
e falhando fechado. O obligation set, renderer composto, manifest, storage,
reachability, serving v2 fail-closed e Dashboard notice-bearing foram
implementados no commit `f682827d1a26b08fa8c450a1fadb3bd0e1fa1700`, com
evidência focal que não constitui gate. O Automatic Quality Gate local,
offline, determinístico e sintético desse comportamento foi posteriormente
`APROVADO` e reconciliado no corpus `4.10.35`. O A0-003 posterior aplicou esse
mecanismo ao candidato somente como disposição documental de direitos.

O ADR-0011 foi aceito, sua semântica foi reconciliada e a política interna de
serving v2 foi corrigida no commit
`b9c3e5f3a72c2dd7762c256198452ae2c217b2d2`. A verificação ocorre antes de
`200` ou `304`, sem alteração do contrato público. O A0-003 dispõe as quatro
operações visuais como `PERMITTED` somente sob o perfil notice-bearing e
preserva distribuição/publicação externa `DENIED`. A geração text-first está
materializada, validada e ativa na revisão `1`; páginas visuais continuam
ausentes até uma resposta ativa citar páginas que requeiram evidência sob
demanda.

O segundo refinamento arquitetural da ordem registrada em Lifecycle está
implementado até integração v2, restart, cold backup/restore confinado e
limites na fronteira local, offline, sintética e sequencial. `AQG-S04-002` a
`AQG-S04-004`, `AQG-S07-V2-001`, `AQG-S07-V2-002` e
`AQG-S07-V2-IR-001` estão `RESOLVIDOS`; os Automatic Quality Gates corretivo
de `S04-CORR-04-E`, do incremento de contrato/serving v2 e da integração e
recuperação v2 estão `APROVADOS`. Não existe novo Human Gate canonicamente
aplicável a esses incrementos: Human Gate pertence a um único `STATE-ID`; o
Human Gate histórico de `STATE-04` permanece inalterado e `STATE-07` não
recebe decisão por implicação. Dataset e homologação de produto continuam
posteriores, `NOT_RUN` e não autorizados; nenhum avanço posterior está
autorizado.

`STATE-04 BACKEND_IMPLEMENTATION` está encerrado após Automatic Quality Gate
aprovado e Human Gate aprovado com as ressalvas documentadas em 2026-08-04. O
fechamento documental de
`S04-A0`, o pin offline de `PdfPig` `0.1.15` e `CsvHelper` `33.1.0`, a
execução sequencial de `S04-A` a `S04-D` e o Automatic Quality Gate posterior
foram autorizados pelo proprietário em 2026-08-04. A fonte offline isolada foi
completada por cópia somente leitura e allowlisted das identidades e versões
já fixadas, sem alterar o cache global. `PdfPig` `0.1.15` e `CsvHelper`
`33.1.0` foram fixados com grafo aplicável vazio, restore locked e hashes
aprovados; o primeiro gate runtime sintético passou. `S04-A`, `S04-B`, `S04-C`
e `S04-D` foram concluídos sequencialmente. O Automatic Quality Gate de
`STATE-04` foi aprovado sem achados abertos; o Human Gate subsequente aceitou
o estado com as limitações e os riscos residuais já registrados.

A auditoria local posterior identificou `AUD-S04-001` a `AUD-S04-009` e o
residual `AUD-S04-005-R1`. `S04-CORR-01`, `S04-CORR-02` e `S04-CORR-03`
implementaram as correções autorizadas sem ampliar o lifecycle. O último
Automatic Quality Gate corretivo foi aprovado e a auditoria completa
reiniciada resolveu todos os achados, sem identificar novo P0, P1, P2 ou P3.
Depois da auditoria e da entrada documental, o proprietário autorizou a
execução local, sequencial e limitada de `S05-A0` a `S05-A4` sobre
`main@cab336ada60866083f3e688fe1a13cff348a3335`, corpus `4.9.2` e working
tree limpa. Os lotes foram concluídos com fixtures sintéticas, fetch falso e
verificações offline na instalação existente. A matriz das oito combinações,
lint, typecheck, 28 testes e build foram aprovados. A validação do build em
loopback confirmou preferências, validação, fluxo fail-closed, foco, landmarks
e controles rotulados; o listener foi encerrado. Cobertura percentual
JavaScript, screenshot do build estilizado e observação direta de viewport
estreito permanecem limitações, conforme o
[relatório de STATE-05](../../docs/STATE-05-Frontend-Implementation-Report.md).

A Automatic Quality Gate de `STATE-05` foi autorizada sobre
`main@f6df67a67657af891e4831a616b142d8da9fb584`, iniciou pela inspeção
estática e foi `REPROVADA` com `AQG-S05-001` (P1). A reprodução mostrou que
uma citação local malformada com `canonicalUrl` `javascript:` atravessa o
decoder e se torna link interativo. A condição de parada cancelou as demais
verificações antes de lint, typecheck, testes, build ou listener; nenhuma
correção foi executada.

`S05-CORR-01` foi autorizado e concluído no commit
`654fce6e0a09d6e7196e434de0ff6f5d6ccd5b04`. O decoder agora rejeita scheme
não HTTPS e qualquer URL em citação local; a apresentação mantém somente link
oficial HTTPS validado. Lint, typecheck, 29 testes e build passaram.
`AQG-S05-001` está `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`, sem disposição
automática.

O reinício integral posterior do gate, autorizado sobre
`main@f7e7f4a9d4afd234c9f3fcc725e7093653bc3363`, foi `REPROVADO` durante a
inspeção estática. `AQG-S05-002` (P2) registra que o limite da resposta é
verificado somente depois da materialização integral por `response.text()`;
`AQG-S05-003` (P2) registra que o título do documento permanece em inglês na
interface `pt-BR`. A parada obrigatória ocorreu antes do preflight executável,
dos checks npm e da validação em browser; nenhum produto ou teste foi alterado.

`S05-CORR-02` foi concluído nos commits
`ec5ecf41b113853fc2863a94cbfe77dbe4741828` e
`20458c8189b132b775786b2fc8f9b44ee5c2f7b8`. A leitura HTTP agora aplica o
teto de 262.144 bytes antes da materialização integral e o título visual segue
somente `interfaceLanguage`. Lint, typecheck, 34 testes e build passaram; a
validação loopback confirmou os dois títulos e terminou sem listener. Os três
achados estão corrigidos, mas pendem de reteste e disposição pelo gate.

O reinício integral autorizado sobre
`main@3f120aaf3cbc199c821685b161ece95a1988a659` foi `REPROVADO` durante a
inspeção estática. `AQG-S05-004` (P2) registra que a citação local válida usa
o valor canônico de freshness `Local`, mas o Dashboard não o localiza e a
fixture sintética o substitui por `Current`, mascarando o estado desconhecido
na apresentação. A parada obrigatória ocorreu antes do preflight executável,
dos checks npm e da validação em browser; nenhuma correção foi executada.

`S05-CORR-03` foi concluído no commit
`9ef937744302044ee3cd9105c9a23ddd3557a861`. O decoder agora restringe
freshness ao conjunto canônico, exige `Local` e URL nula para
`LocalAuthorised`, rejeita relações cross-class incompatíveis e apresenta
`Local` nas interfaces `pt-BR` e `en-GB`. A fixture local foi corrigida. Lint,
typecheck, 35 testes e build passaram; a validação loopback confirmou a
alternância localizada e terminou sem listener. As regressões de
`AQG-S05-001` a `AQG-S05-003` permaneceram verdes.

O reinício integral autorizado sobre
`main@b457970aed4564d5a654bb4e8d38439c98f29522` foi `REPROVADO` durante a
inspeção estática. `AQG-S05-005` (P2) registra que o decoder aceita
`answerLanguage` apenas por pertencer ao conjunto suportado, sem compará-lo ao
`questionLanguage` enviado. O teste de limite exato demonstra que uma
pergunta `en-GB` aceita a fixture concluída `pt-BR`. A parada obrigatória
ocorreu antes do preflight executável, dos checks npm e da validação em
browser; nenhuma correção foi executada.

`S05-CORR-04` foi concluído no commit
`bed8ec03d670ed4e76a556f7df723c30db320a24`. O decoder exige que o idioma da
resposta concluída corresponda ao idioma enviado, o cliente preserva esse
binding e as quatro combinações válidas/incompatíveis foram exercitadas.
Lint, typecheck, 37 testes e build passaram.

O reinício integral posterior sobre
`main@a58c4038fb14e656c95303d914e02c7f8ad75c17` dispôs
`AQG-S05-001` a `AQG-S05-005` como `RESOLVIDOS`, mas foi `REPROVADO` por
`AQG-S05-006` (P2). A reprodução em browser mostrou que o skip link visível
não transfere o foco ao conteúdo principal. A parada obrigatória impediu a
conclusão das verificações browser de viewport estreito/reflow, temas e matriz
das oito combinações. O Human Gate continua prematuro e `STATE-06` não está
autorizado. `S05-CORR-05` corrigiu o foco do skip link e está
`RESOLVIDO` pelo reinício integral posterior sobre
`main@8ee1213eed3522493204c68b4f843e9c438e0f69`. Esse gate foi
`REPROVADO` por `AQG-S05-007` (P2): em viewport de 320 CSS px, todas as quatro
combinações `pt-BR` geravam overflow horizontal, enquanto as quatro
combinações `en-GB` refluíam sem overflow. `S05-CORR-06`, no commit
`e34e73c7bbe8fabf96d5a5683df35935a3266e37`, tornou reduzíveis a coluna da
hero e sua tipografia compacta; os quatro checks npm e a matriz browser
isolada das oito combinações passaram a 320 CSS px, com foco, teclado e temas
preservados. `AQG-S05-007` está
`CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`. O reinício integral posterior sobre
`main@bc2ddd6bf64fc82f7d68eb518c3013d85655c16a` foi `REPROVADO` durante a
inspeção estática por `AQG-S05-008` (P2): tokens contínuos válidos pelo
contrato em resposta, título ou trecho de citação podem forçar overflow no
viewport estreito porque as superfícies correspondentes não permitem sua
quebra. A parada ocorreu antes de preflight executável, checks npm, build ou
browser, sem processo ou listener iniciado e sem correção. `AQG-S05-001` a
`AQG-S05-006` conservam `RESOLVIDOS`; `AQG-S05-007` permanece corrigido
pendente de reteste. `S05-CORR-07`, no commit
`3f003b9db67eefeccc7e677c319ca37a26d49fa7`, tornou quebráveis sem truncamento
os três textos não confiáveis e adicionou tokens contínuos válidos à matriz
das oito combinações. Os quatro checks npm e a repetição headless controlada
passaram a 320 CSS px com reflow, idiomas, temas, foco e teclado preservados.
A primeira tentativa browser gerou acesso externo não autorizado ao ativar a
URL oficial sintética antes da guarda de foco; o incidente, a parada e a
retomada autorizada estão registrados no relatório. A repetição final usou
somente citação local sem URL e bloqueio não loopback, sem nova tentativa
externa. `AQG-S05-008` ficou `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE` naquele
lote. O reinício integral posterior, sobre
`main@97ea076da84d7afdb3330aa05dcb39fc7b44ce0f`, repetiu desde o início as
inspeções estáticas, os quatro checks npm, o build reprodutível e a validação
browser completa. As oito combinações passaram em largura padrão e a 320 CSS
px, com guarda antes de cada `Enter`, citação local sem URL, bloqueio de
destinos não loopback e zero tentativa ou URL externa. O gate foi `APROVADO`,
sem novo P0, P1, P2 ou P3, e `AQG-S05-001` a `AQG-S05-008` estão
`RESOLVIDOS` naquela baseline. Durante a revisão humana posterior, o
proprietário solicitou simplificar a hero. `S05-CORR-08`, no commit
`b65d3b45a0ad32f0f7db1e97ccf415bdef5bb113`, removeu o rótulo promocional,
o título anterior e a ilustração decorativa, mantendo como único conteúdo
visível da área a introdução localizada promovida a H1 proporcional. Os
quatro checks npm passaram após a restauração do rótulo de workspace usado
somente como nome acessível; a matriz browser isolada passou nas oito
combinações em 1280 e 320 CSS px, com reflow, temas, idiomas, foco e teclado
preservados e sem URL ou tentativa externa. O listener e o Chrome temporário
foram encerrados, e as portas 4173, 5173 e 9230 ficaram livres.

O reinício integral posterior a `S05-CORR-08`, sobre
`main@b68cf2d8a9a6c735781529f1f3fb63d5cd515f95`, repetiu as inspeções
estáticas, os quatro checks npm, o build reprodutível e a matriz browser em
largura padrão e estreita. A hero simplificada, as oito combinações, foco,
teclado, temas, idiomas, reflow e tokens contínuos passaram sem tentativa ou
URL externa. O gate foi `APROVADO`, sem novo P0, P1, P2 ou P3, e
`AQG-S05-001` a `AQG-S05-008` estão `RESOLVIDOS` na baseline. O Human Gate
posterior foi `APROVADO` sem ressalvas sobre
`main@192613364429a79ce82a208f072f5005209e6f52`, corpus `4.9.2` e working
tree limpa, depois do resumo completo e da confirmação canônica do
proprietário. O preview humano exclusivamente loopback foi encerrado e as
portas 4173, 5173 e 9230 terminaram livres. `STATE-05` está encerrado.

`STATE-06 INTEGRATION` está ativo por autorização explícita do proprietário
sobre `main@8fb3b93532a569af953cdf24e190b82998020464`, corpus `4.9.2` e
working tree limpa. O único lote autorizado, `S06-A`, foi concluído no limite
local, offline, sintético e sequencial, com implementação focal em
`8041e25a554a7cc47ecebf4abe1fc8b94b12d12d` e relatório próprio. O Automatic
Quality Gate posterior foi executado sobre
`main@a6f0480b7f229b63c5ac24d65e61f55de1c6483a` e ficou `REPROVADO` por
`AQG-S06-001` a `AQG-S06-003`, todos P2. O proprietário autorizou
`S06-CORR-01` sobre `main@140c0516e4dbfc02808a90f0496550eb6b09da1b` e
aceitou `NORM-S06-001`; o corpus `4.9.3` reconcilia o ownership do README. As
ampliações posteriores de `AUTH-S06-DEP-001` e `AUTH-S06-DEP-002` permitiram
somente os quatro lockfiles/projetos de produção, o RID `linux-arm64`, os três
runtime packs verificados `10.0.10` e os 13 packages de teste já locked no
cache isolado. Restore locked, implementação e C4 foram aprovados sem mudança
de lockfile ou grafo além do fechamento previamente autorizado. Os commits
`4b808319b0c1abf0970f9f41c77fb1e08d295585`,
`405ab20d3e76a75f1a0f50fd625ec71831b9134b`,
`801f77625e68692fe7b4691798694b4e8d92433a`,
`9d72a1bb93325f6303516592fb4ff352a0a531ca` e
`f1a02cd7c7acb50bcd3fa8b00e69e6c3f59b88c3` materializam a correção e a
compatibilização final. O reinício integral autorizado por
`AUTH-S06-AQG-RETEST-001` foi executado sobre
`main@9d7c4ce816eca049ba09942ab7fe8b1148aa73c9`, corpus `4.9.3` e working tree
limpa. O gate foi `APROVADO`, sem novo P0, P1, P2 ou P3, e `AQG-S06-001` a
`AQG-S06-003` estão `RESOLVIDOS`. `STATE-06` continua ativo. Human Gate,
`STATE-07`, execução Linux, OCI real, providers, contas, secrets, corpus ou
fontes reais, armazenamento operacional, GitHub, publicação, deploy,
DB-Notifier e estados posteriores continuam sem autorização.

Depois desse gate, a auditoria técnica AST confirmou três problemas na
persistência e na autoridade de recuperação. Os commits
`0b3c5be2c80f0f1ee83af82d2158e87360c33ea7`,
`d3fa9d77863092918dbef6fa7afee12992c2053f`,
`cfb93892571bec1beae3087b1f5ff44932d24693` e
`dc3dde2437ad3cbb50b397358fcda043c9d6f4b3` corrigiram `AST-002`, `AST-003`,
`AST-001` e o reforço referencial complementar, respectivamente. A revisão
pós-correção dispôs os três achados como `RESOLVIDOS` sobre
`main@dc3dde2437ad3cbb50b397358fcda043c9d6f4b3`, corpus `4.9.3` e working
tree limpa.

O reinício integral posterior do Automatic Quality Gate sobre
`main@726546dbe0302b9664a62e890b6a27f19bf0c6e4`, corpus `4.9.3` e working
tree inicialmente limpa, revisou todo o diff de 20 arquivos após
`9d7c4ce816eca049ba09942ab7fe8b1148aa73c9`. A primeira tentativa parou com
`AQG-S06-004`: duas working copies geradas do Entity Framework estavam com
EOL misto. A remediação autorizada normalizou somente seus bytes para LF, sem
diff semântico, e o reinício integral subsequente foi `APROVADO`, sem novo P0,
P1, P2 ou P3; `AQG-S06-004` está `RESOLVIDO`.

O gate consolidado aprovou 206 testes .NET e 38 testes npm, cobertura .NET de
93,11% de linhas e 66,89% de branches, cinco testes focais de migration apenas
em SQLite descartável, quatro testes focais do host composto, verificação EF
sem mudança pendente, duas reproduções ARM64 idênticas e os comandos literais
de integração do README. Invocações preliminares de EF com tool home isolado,
startup project incorreto ou sem store root explícito não foram aceitas como
evidência. A limpeza removeu os diretórios exclusivos do gate e da cobertura,
encerrou sem processo/listener da tarefa e preservou limpa a baseline Git; o
artefato ignorado do comando literal do README permanece não autoritativo.

Persistem como limitações e riscos residuais: ausência de execução Linux
ARM64, OCI real, providers, contas, secrets, corpus ou fonte oficial real,
armazenamento operacional, cobertura percentual JavaScript, observação de
rede em nível de pacotes, migration em banco real e reparo de dados. O gate
não executou Human Gate, não alterou o lifecycle e não autorizou `STATE-07`,
ação externa, publicação, push ou deploy. `STATE-06` continua ativo.

Depois disso, os commits
`8ab79d59f4dfe9d35e73a25f05612fd244e31393` e
`f92e26c7008a2d124bd10edb2e3f03c0c9ad2bf6` alteraram o agregador de
cobertura e a política fail-closed de CI. O novo reinício integral autorizado
sobre `main@f92e26c7008a2d124bd10edb2e3f03c0c9ad2bf6`, corpus `4.9.3` e working
tree limpa inventariou os oito commits posteriores a `bfc3aefc` e parou na
auditoria estática com `AQG-S06-005` (P2):
`eng/test-assert-coverage.ps1` e `eng/test-ci-policy.ps1` não são chamados por
nenhum entry point automático, inclusive `eng/ci.ps1` e o workflow. O gate
está `REPROVADO`, o achado permanece `ABERTO` e nenhuma etapa executável
posterior foi iniciada. `STATE-06` continua ativo; Human Gate, `STATE-07` e
ações externas permanecem não autorizados.

A correção focal posterior, autorizada sobre
`main@000dca0210e220a9f247159178c6d97d9fc4fd55`, integrou os dois testes ao
início de `eng/ci.ps1` por um helper fail-closed compartilhado e ampliou o
teste de política para provar sucesso, propagação de falha, ausência de script,
invocação única e consumo canônico pelo workflow. Parsing, 11 casos de
coverage, 14 controles de política/integração, `git diff --check` e auditoria
de 203 arquivos passaram localmente e offline. O workflow não mudou. O gate
integral não foi reiniciado; `AQG-S06-005` está
`CORRECTED_PENDING_GATE_RETEST`, e Human Gate continua prematuro.

O reinício integral subsequente, autorizado sobre
`main@616bef4e2ae8c0b26c10781cd728dc6089136a60`, corpus `4.9.3` e working
tree limpa, repetiu desde o início supply chain, restore locked isolado,
controles fail-closed, gate técnico, cobertura, persistência/migration,
cancelamento/resiliência, duas reproduções ARM64, verificador estático,
comandos do README, segurança e higiene. Todos os controles passaram: 206
testes .NET, 38 testes npm, 93,11% de linhas, 66,89% de branches e dois ZIPs
ARM64 idênticos com 17 ELF64 AArch64. O Automatic Quality Gate está
`APROVADO`, sem novo P0, P1, P2 ou P3, e `AQG-S06-005` está `RESOLVIDO`.
Human Gate e `STATE-07` não foram executados; `STATE-06` continua ativo.

O proprietário recebeu o resumo completo do Human Gate sobre
`main@2f70705dcbe293b22ccd039d0764b2b9ca4b2e8a`, corpus `4.9.3` e working
tree limpa e confirmou exatamente
`Confirmo a decisão acima exclusivamente para STATE-06`. O Human Gate foi
`APROVADO COM RESSALVAS` para a fronteira local, offline, sintética e
estática documentada. As limitações de Linux ARM64, OCI, providers, corpus,
fontes e armazenamento reais, cobertura percentual JavaScript, observação de
pacotes e migration real permanecem explícitas. `STATE-06 INTEGRATION` está
encerrado; `STATE-07` não foi autorizado nem iniciado, e nenhuma ação externa
foi executada.

Em 2026-08-06, sobre
`main@3240a4b13acd82a1cf5815ac64f6997b2a7f89bf`, corpus `4.9.3` e working
tree limpa, o proprietário autorizou exclusivamente a entrada documental em
`STATE-07 TESTING_HOMOLOGATION` e a reconciliação dos blocos de status
público necessários. `STATE-07` está ativo sem lote autorizado ou executado.
Qualquer dataset, avaliação RAG, teste, carga, segurança dinâmica, browser,
provider, fonte real, rede, OCI, GitHub, publicação, deploy, `STATE-08` ou
ação externa exige autoridade humana explícita e separada.

Em 2026-08-07, sobre
`main@183c8cd9fe303096a355ab731e72dc81748eb626`, corpus `4.9.3` e working
tree limpa, o proprietário confirmou a proposta documental de `S07-A`
exclusivamente como baseline de planejamento. A confirmação não autorizou
`AUTH-S07-A-DATASET-001`, `AUTH-S07-A-RUN-001`, materialização de dataset,
avaliação, testes, carga, segurança dinâmica, browser, providers, fontes
reais, rede ou ação externa. Naquele registro histórico, `S07-A` ainda não
havia sido executado; a execução posterior de A1-A5 e a aprovação de A5 estão
registradas no estado factual vigente acima.

Em 2026-08-07, sobre
`main@66c47d94d423abf4f0c1509ba04b8064d3efd8ca`, corpus `4.9.3` e working
tree limpa, o proprietário determinou a correção permanente do handoff para
sempre informar exatamente uma próxima ação concreta. O corpus `4.9.4`
registra essa regra em AGENTS, Governance e Templates, preservando o limite
temático e a ausência de autoridade para `S07-A` ou qualquer execução.

Em 2026-08-07, sobre
`main@3d15ad4f2726f715c8dcf880491927ad0ff37b2f`, corpus `4.9.4` e working
tree limpa, o proprietário autorizou exclusivamente a reconciliação semântica
conjunta dos ADRs 0008/0009. O corpus `4.9.5` registra a arquitetura aceita nos
18 arquivos confirmados, preserva OpenAPI v1 byte a byte e mantém implementação,
dataset, conteúdo, renderização, indexação, ativação, execução e ação externa
fora da autoridade.
