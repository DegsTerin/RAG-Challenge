# STATE-05 Frontend Implementation Report

## Purpose and boundary

This report is the durable, sanitised record of the locally authorised,
sequential `S05-A0` through `S05-A4` implementation in `STATE-05
FRONTEND_IMPLEMENTATION`. It records completed deliverables, verification,
limitations and residual risks. It is evidence, not authority to run the
Automatic Quality Gate, make a Human Gate decision, enter `STATE-06`, publish,
deploy or perform an external action.

## Baseline and authority

- Initial implementation baseline: branch `main`, commit
  `cab336ada60866083f3e688fe1a13cff348a3335`, corpus `4.9.2`, clean working
  tree, reconfirmed immediately before the first change.
- Completed lot baseline before this factual record: branch `main`, commit
  `5865a225cdab9bd92f9befa00c7ee581b2aa0877`, corpus `4.9.2`, clean working
  tree.
- Owner authority dated 2026-08-04: execute only `S05-A0` through `S05-A4`
  locally, offline and sequentially, with the existing installation and
  focused local commits.
- Allowed writes: `src/RagChallenge.Dashboard.Web`, its own tests, this report
  and necessary factual updates to current state and append-only history.
- Runtime preflight was targeted before loopback validation. The only matching
  process was the Codex browser-control runtime, not a RAG-Challenge product
  process; it was not stopped. Port `4173` was free.

## Sequential lot outcomes

### S05-A0 — client contract, state, preferences and fixtures

Commit `9c27cc49442ff467486c93febf7144e6d3a652b7`:

- froze TypeScript transport types and fail-closed runtime decoding for the
  existing `POST /api/v1/questions` contract;
- bounded and NFC-normalised questions by UTF-8 bytes;
- modelled idle, submitting, completed and failed view states with stale
  request protection;
- separated interface language, question language and visual theme
  preferences with versioned, bounded local persistence;
- added synthetic answered, insufficient-evidence and Problem Details
  fixtures without a corpus, provider, account or network;
- pinned the repository-owned OpenAPI snapshot at SHA-256
  `D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34`.

### S05-A1 — semantic shell, localisation and themes

Commit `2fd7526f0907361d6c03552379341b877e88c236`:

- implemented a semantic header, main query workspace, result region and
  footer with a skip link and visible focus treatment;
- implemented product-owned `pt-BR` and `en-GB` interface copy;
- kept interface language, question language and theme selectors independent;
- implemented `Light` and `Dark` design tokens, system-theme initial choice,
  explicit persistence and `Light` fallback;
- added responsive rules, reduced-motion handling and server-rendered
  component checks without adding a dependency.

### S05-A2 — bounded query flow

Commit `7a42d332ddf6646c575c7cae16cfe9085120e18d`:

- implemented form validation for empty, control-character and 4,096-byte
  UTF-8 boundary failures;
- implemented the API v1 same-origin client with `POST`, JSON media-type
  checks, no credentials, no redirects, no cache, a 262,144-byte response
  ceiling and fail-closed decoding;
- implemented cancellation of the previous request and protection against a
  late response replacing the current result;
- mapped network, cancellation and incompatible-response failures to bounded
  client states rather than rendering transport prose.

### S05-A3 — response, coverage, provenance, citations and failures

Commit `a8835b94ab485e542f7cfe23355283c92de17fc8`:

- implemented distinct answered and insufficient-evidence presentation;
- presented evaluated active/eligible database and document counts separately
  from cited evidence and disclosed degraded source states;
- presented authorised local and official external provenance, original
  evidence language, PDF page or CSV record/column locations, generation,
  snapshot, freshness and revalidation metadata;
- limited official links to contract-validated HTTPS URLs and added
  `noopener noreferrer` on new-window links;
- mapped all 12 stable API v1 failure codes to product-owned localised copy,
  with bounded correlation and retry metadata;
- rendered answers and source evidence only as escaped text. No generated
  Markdown or HTML rendering sink was introduced.

### S05-A4 — matrix, final checks and gate preparation

Commit `5865a225cdab9bd92f9befa00c7ee581b2aa0877`:

- exercised all eight combinations of interface language (`pt-BR`/`en-GB`),
  question language (`pt-BR`/`en-GB`) and theme (`Light`/`Dark`);
- verified localised ownership of all 12 API v1 error-code messages;
- verified primary Light/Dark text-token pairs against the WCAG AA 4.5:1
  normal-text contrast floor;
- completed the final offline lint, typecheck, component/flow test and build
  sequence and prepared this factual evidence for a separately authorised
  Automatic Quality Gate.

## API v1 client contract

The Dashboard consumes only the existing `POST /api/v1/questions` route. It
sends the fixed MVP corpus identifier, explicit `questionLanguage` and the
normalised question. It accepts only the existing answered,
insufficient-evidence and Problem Details shapes. It does not change or
extend OpenAPI, server contracts or backend behaviour.

The browser never decides corpus eligibility, source trust, citation identity,
freshness or generation compatibility. Runtime decoding rejects inconsistent
corpus/generation identities and incomplete official provenance. Unknown or
malformed responses fail closed as an incompatible response.

## Accessibility and localisation evidence

- One `main` landmark and one level-one heading were observed in the built
  page; no duplicate IDs or unlabelled interactive controls were found by the
  bounded browser inspection.
- The skip link was the first keyboard focus target and had a visible solid
  outline. Completion and failure move focus to the result heading.
- Form controls have programmatic names, validation uses `aria-invalid` and a
  live error region, and query loading uses `aria-busy`.
- Answer text is marked with the answer language; each citation title and
  excerpt retains the source `contentLanguage` rather than being translated.
- Reduced-motion and narrow-layout CSS paths exist. The browser connector did
  not apply its requested narrow viewport override, so a narrow viewport was
  not visually observed in this execution.

## Local verification record

All commands ran from `src/RagChallenge.Dashboard.Web` on 2026-08-04 using the
existing installation. No install command ran.

| Verification | Result |
| --- | --- |
| `npm run lint` | Passed, exit code 0 |
| `npm run typecheck` | Passed, exit code 0 |
| `npm test` | Passed, 28 tests, 0 failed/skipped/cancelled |
| `npm run build` | Passed, 20 modules transformed |
| Built HTML | 0.96 kB (0.54 kB gzip) |
| Built CSS | 11.96 kB (3.35 kB gzip) |
| Built JavaScript | 170.52 kB (54.88 kB gzip) |
| `git diff --check` | Passed for each focused increment |
| Package and lockfile diff | Empty |
| OpenAPI diff and hash | Empty; pinned SHA-256 retained |

The repository pins Node.js `24.18.0` and npm `11.16.0`. The observed local
runtime was Node.js `24.18.1` and npm `11.16.0`; the Node patch difference is a
local verification limitation and does not change the pin.

## Loopback browser validation

A task-owned listener was bound exclusively to `127.0.0.1:4173`. The Vite
development mode injects CSS inline and was incompatible with the production
CSP, so it was not accepted as visual evidence. The listener was replaced by
the already built Vite preview, where the versioned external stylesheet loaded
under the CSP.

On that built page, bounded browser checks observed:

- the `Dark` and `Light` computed colour schemes and localised `pt-BR` to
  `en-GB` interface transition;
- independent `en-GB` question selection, exact 47-byte UTF-8 count and
  clearing of the prior validation error;
- no request after empty input validation;
- fail-closed `ResponseIncompatible` presentation after a same-origin request
  to the preview listener, with focus moved to the error heading;
- no horizontal overflow at the browser's available viewport, no duplicate
  IDs, no unlabelled controls and no browser console warning or error;
- skip-link-first keyboard navigation with visible focus.

The browser's screenshot command timed out for the styled build and its
requested 320-pixel viewport override was not applied. Consequently, this
record does not claim screenshot-based visual approval or direct narrow-screen
observation. Both task-owned Vite processes were identity-checked and stopped;
port `4173` had no listener afterwards.

## Test and coverage boundary

The 28 tests cover the frozen contract, UTF-8 bounds, state transitions,
preferences, fake-fetch transport, response limits, component rendering,
insufficient evidence, PDF/CSV citations, local/official provenance, hostile
text escaping, all failure codes, the eight-combination matrix and contrast
tokens.

The existing dependency set has no JavaScript instrumentation package or
coverage script. Meaningful component and flow tests were possible through
the installed React server renderer, Vite SSR, reducer tests and injected fake
fetch, so execution did not require a new dependency. Line and branch coverage
percentages were not measured and the repository floors of 70% and 45% are
therefore not claimed for the Dashboard. This is an explicit Automatic Quality
Gate preparation risk, not a passing coverage result.

## Security and failure states

- CSP restricts the built application to same-origin scripts, styles and
  connections; no analytics or external assets were introduced.
- Requests omit credentials, reject redirects, enforce request/response bounds
  and validate response media types and structures.
- Server `title`, `detail` and arbitrary problem codes are never rendered as
  trusted interface copy.
- Answer, title, excerpt, IDs and metadata use ordinary React text rendering;
  hostile synthetic markup remained escaped.
- No secret, personal data, prompt, answer or document content is persisted.
  Local storage contains only explicit versioned language and theme choices.

## Limitations and residual risks

1. JavaScript line and branch coverage percentages are unavailable with the
   existing installation.
2. No real API host, provider, account, secret, corpus, official source or
   operational persistence was exercised; transport and evidence are
   synthetic.
3. No screenshot-based review or directly forced narrow viewport completed;
   responsive behaviour has code and matrix evidence only.
4. No third-party accessibility engine was installed or run. Accessibility
   evidence is semantic, keyboard, computed-style and component-test based.
5. Browser preview returned a non-contract response because no backend was
   authorised or running. The observed result was the intended fail-closed
   client state, not an end-to-end grounded answer.
6. Node.js `24.18.1` was used locally instead of the repository's exact
   `24.18.0` pin.

No observed item was classified as P0 or P1. The Automatic Quality Gate must
independently classify these limitations and may stop on any material security,
accessibility, coverage or reproducibility finding.

## Rollback

Rollback is the sequential reversal of the focused commits, newest first:

1. `5865a225cdab9bd92f9befa00c7ee581b2aa0877`;
2. `a8835b94ab485e542f7cfe23355283c92de17fc8`;
3. `7a42d332ddf6646c575c7cae16cfe9085120e18d`;
4. `2fd7526f0907361d6c03552379341b877e88c236`;
5. `9c27cc49442ff467486c93febf7144e6d3a652b7`.

Rollback requires its own explicit authority. It must not use destructive Git
commands, alter the accepted API contract or erase append-only lifecycle
history.

## Preserved negative scope and next authority

No dependency, `package.json`, lockfile, OpenAPI, ADR, backend, Domain,
Application, Infrastructure, API, provider configuration or state after
`STATE-05` changed. No `dotnet`, install, external network, provider, account,
secret, real corpus, real official source, GitHub, OCI, publication, deploy,
DB-Notifier, administration, upload, crawling, authentication, RBAC or
generated Markdown/HTML execution occurred.

`S05-A0` through `S05-A4` are locally implemented and verified within the
limitations above. The `STATE-05` Automatic Quality Gate, Human Gate and entry
into `STATE-06` remain not executed and not authorised. The next possible
action is a separate explicit owner decision limited to the Automatic Quality
Gate after review of the clean post-record baseline.

## Automatic Quality Gate — 2026-08-05

### Authority and baseline

- Gate baseline: branch `main`, commit
  `f6df67a67657af891e4831a616b142d8da9fb584`, corpus `4.9.2`, clean working
  tree. Location, Git top-level, Git directory, branch, commit, corpus and
  cleanliness were reconfirmed before the audit.
- Authority: local, offline and sequential Automatic Quality Gate with no
  product correction, dependency change, installation, external action,
  Human Gate or later lifecycle state.
- Runtime preflight before the bounded executable reproduction found only the
  Codex browser-control runtime associated with the workspace and no listener.
  It was not a RAG-Challenge product process and was not stopped.

### Result

`REPROVADO`. The gate stopped on one material security finding, as required by
the owner's stop condition. No product or test file was changed.

#### AQG-S05-001 — P1 — unsafe local citation URL reaches an anchor

- Requirement: API responses and citation metadata are untrusted at the
  Dashboard boundary. Only approved URL schemes may become interactive links,
  and malformed responses must fail closed.
- Location: `src/RagChallenge.Dashboard.Web/src/contracts/api-v1.ts` validates
  `canonicalUrl` as safe HTTPS only when `sourceTrustClass` is
  `OfficialExternal`. `src/RagChallenge.Dashboard.Web/src/App.tsx` renders any
  non-null `canonicalUrl` as an anchor.
- Reproduction: a synthetic answered response was changed in memory so its
  second, `LocalAuthorised` citation carried
  `javascript:alert(document.domain)`. The normal response decoder accepted
  the value and server rendering emitted
  `href="javascript:alert(document.domain)"`. React also emitted its unsafe-URL
  warning.
- Existing test gap: the dangerous-URL contract test mutates only the first
  fixture citation, which is `OfficialExternal`. Hostile title and excerpt
  rendering is covered, but a hostile `canonicalUrl` on a local citation is
  not.
- Backend context: the current Application invariant requires
  `LocalAuthorised` evidence to have a null canonical URL. That reduces the
  normal-path likelihood but does not remove the browser/API trust boundary or
  the frontend requirement to reject a malformed response.
- Impact: a malformed or compromised API response can create an interactive
  script-scheme citation link. The current CSP may provide an additional
  mitigation, but link activation under the CSP was not exercised after the
  mandatory stop and cannot be treated as the primary validation boundary.
- Recommendation: under separate corrective authority, require every non-null
  citation URL to pass the approved HTTPS validation and enforce the
  `LocalAuthorised` null-URL invariant before rendering; add contract and
  presentation regression tests for the local citation case.

### Checks stopped or not reached

The following authorised checks were deliberately not executed after the
finding: `npm run lint`, `npm run typecheck`, `npm test`, `npm run build`,
coverage reassessment, styled screenshot, narrow-viewport/reflow observation,
keyboard/browser repetition and build reproducibility. No loopback listener
was started, so no listener cleanup was necessary.

The previously recorded implementation evidence remains historical evidence
only; it does not substitute for a complete passing Automatic Quality Gate.
The accessibility, narrow-viewport, JavaScript coverage and exact Node pin
limitations therefore remain open and were not disposed by this gate.

### Lifecycle consequence

`STATE-05 FRONTEND_IMPLEMENTATION` remains active. Its Automatic Quality Gate
is failed with `AQG-S05-001` open. Human Gate and `STATE-06` remain not
authorised and not executed. A correction and a complete restart of the
Automatic Quality Gate require separate explicit owner authority.

## S05-CORR-01 — 2026-08-05

### Authority and baseline

- Corrective baseline: branch `main`, commit
  `7ee2241049dc68f16a38e85bd622928e64a317e7`, corpus `4.9.2`, clean working
  tree, reconfirmed immediately before the first change.
- Authority: local, offline and sequential correction of `AQG-S05-001` only,
  including Dashboard contract and presentation regressions, the four existing
  npm checks, factual records and focused local commits.
- Negative scope preserved: no dependency, package, lockfile, external
  contract, OpenAPI, ADR, backend, provider, installation, network, external
  action, Automatic Quality Gate, Human Gate or later state.

### Correction

Commit `654fce6e0a09d6e7196e434de0ff6f5d6ccd5b04`:

- rejects every non-null citation URL that does not pass the existing safe
  HTTPS validation;
- rejects every non-null `canonicalUrl` on a `LocalAuthorised` citation,
  including an otherwise valid HTTPS URL;
- retains the official requirement for a safe HTTPS URL, snapshot and UTC
  revalidation instant;
- adds a presentation guard so only `OfficialExternal` citations with a safe
  HTTPS URL can produce an anchor;
- adds contract regressions for the local `javascript:` and local HTTPS cases,
  plus explicit preservation of official HTTPS and local null-URL fixtures;
- adds a presentation regression proving a malformed local URL never becomes
  interactive while the valid official citation link remains present.

### Verification

All commands ran from `src/RagChallenge.Dashboard.Web` on 2026-08-05 with the
existing Node.js `24.18.1` and npm `11.16.0` installation. No install command
or `dotnet` command ran.

| Verification | Result |
| --- | --- |
| `npm run lint` | Passed, exit code 0 |
| `npm run typecheck` | Passed, exit code 0 |
| `npm test` | Passed, 29 tests, 0 failed/skipped/cancelled |
| `npm run build` | Passed, 20 modules transformed |
| Built HTML | 0.96 kB (0.54 kB gzip) |
| Built CSS | 11.96 kB (3.35 kB gzip) |
| Built JavaScript | 170.81 kB (54.94 kB gzip) |
| Package, lockfile and OpenAPI diff | Empty |
| OpenAPI SHA-256 | `D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34` |

The runtime preflight before executable verification found only the Codex
browser-control runtime associated with the workspace and no product-owned
listener. It was not a RAG-Challenge process and was not stopped. This
increment did not authorise or require a browser listener.

### Disposition and next authority

`AQG-S05-001` is `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`. The correction and
targeted regressions passed, but only a separately authorised complete restart
of the `STATE-05` Automatic Quality Gate can resolve the finding or approve the
gate. The earlier JavaScript coverage, styled screenshot, narrow viewport,
external accessibility-engine and exact Node pin limitations remain open for
that gate.

`STATE-05 FRONTEND_IMPLEMENTATION` remains active. Automatic Quality Gate,
Human Gate and `STATE-06` were not executed by `S05-CORR-01` and remain without
authority.

## Automatic Quality Gate restart — 2026-08-05

### Authority and baseline

- Restart baseline: branch `main`, commit
  `f7e7f4a9d4afd234c9f3fcc725e7093653bc3363`, corpus `4.9.2`, clean working
  tree. Location, Git top-level, Git directory, branch, commit, corpus and
  cleanliness were reconfirmed before the audit.
- Authority: restart the complete `STATE-05` Automatic Quality Gate locally,
  offline and sequentially, without product correction, dependency change,
  installation, external action, Human Gate or later lifecycle state.
- The audit began again with authority, lifecycle, scope, contract and
  security inspection. Its owner-defined stop condition applied to any
  material finding.

### Result

`REPROVADO`. Two material P2 findings were observed during the initial static
inspection. The gate stopped before executable preflight, npm commands or
loopback browser validation. No product or test file was changed.

#### AQG-S05-002 — P2 — response ceiling is enforced after full materialisation

- Requirement: untrusted response input must remain bounded in bytes before it
  can consume unbounded client memory.
- Location: `src/RagChallenge.Dashboard.Web/src/query-client.ts` calls
  `response.text()` before calculating the UTF-8 byte count against the
  262,144-byte ceiling.
- Evidence: no `Content-Length` pre-check, streaming reader or incremental
  byte counter exists in the client or its tests. The oversized-response test
  proves rejection only after the complete synthetic body has been allocated
  and decoded as text.
- Impact: a malformed, compromised or unexpectedly large same-origin API
  response can be fully buffered before rejection. The declared ceiling
  protects JSON parsing and rendering, but does not bound transport-body
  materialisation or its transient memory cost.
- Recommendation: under separate corrective authority, enforce the ceiling
  while reading the response body, reject a declared oversized length before
  reading where present, preserve cancellation, and add deterministic fake
  stream tests for boundary, overflow and abort behaviour.

#### AQG-S05-003 — P2 — document title is not localised with the interface

- Requirement: product-owned visual text must use the selected
  `interfaceLanguage` without mixing `pt-BR` and `en-GB`.
- Location: `src/RagChallenge.Dashboard.Web/index.html` fixes the document
  title as `RAG-Challenge — Database documentation`. No source or test updates
  `document.title` when `interfaceLanguage` changes.
- Impact: the default `pt-BR` interface and subsequent language changes leave
  an English product label visible in the browser tab, so the visual language
  is not complete even though the in-page shell is localised.
- Recommendation: under separate corrective authority, add localised document
  metadata owned by the interface-language state and test both initial and
  switched language values without coupling them to `questionLanguage` or
  theme.

### AQG-S05-001 re-evaluation

Static inspection confirmed that the decoder rejects non-HTTPS citation URLs,
requires a null `canonicalUrl` for `LocalAuthorised`, and that presentation
creates anchors only for `OfficialExternal` citations with validated HTTPS.
The two focused regression files contain the expected malformed-local and
preserved-valid cases. However, the mandatory stop occurred before `npm test`,
so `AQG-S05-001` remains `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE` rather than
being disposed as resolved by this incomplete restart.

### Checks stopped or not reached

- Executable runtime preflight was not reached; no process or listener was
  inspected, started or stopped.
- `npm run lint`, `npm run typecheck`, `npm test` and `npm run build` were not
  executed.
- JavaScript percentage coverage remains unavailable in the existing package
  scripts and dependency set; no percentage is claimed.
- Styled visual review, external accessibility-engine checks, narrow viewport
  and reflow observation, keyboard repetition, the eight-combination browser
  matrix and exact-toolchain reproducibility were not reached.
- No loopback listener or browser session was started, so no listener cleanup
  was required.

### Lifecycle consequence

`STATE-05 FRONTEND_IMPLEMENTATION` remains active. Its restarted Automatic
Quality Gate is failed with `AQG-S05-002` and `AQG-S05-003` open, while
`AQG-S05-001` remains corrected but pending executable gate retest. Human Gate
and `STATE-06` remain not authorised and not executed. Corrections and a later
complete gate restart require separate explicit owner authorities.

## S05-CORR-02 — 2026-08-05

### Authority and baseline

- Correction baseline: branch `main`, commit
  `651b4ad9edba79b3fc8a16e550fc2a357b6b85d2`, corpus `4.9.2`, clean working
  tree. Location, Git top-level, Git directory, branch, commit, corpus and
  cleanliness were reconfirmed immediately before the first alteration.
- Authority: execute the local, offline and sequential `S05-CORR-02`
  increment only, correcting `AQG-S05-002` and `AQG-S05-003`, preserving the
  `AQG-S05-001` correction, running the four existing npm checks and using a
  task-owned loopback listener only for title validation.
- No dependency, package, lockfile, external contract, OpenAPI, ADR, backend,
  provider, installation or external action was authorised or performed.

### Corrections

The focused commit `ec5ecf41b113853fc2863a94cbfe77dbe4741828`
implements the response-body boundary. The client now:

- rejects a valid decimal `Content-Length` greater than 262,144 bytes before
  acquiring the response stream reader;
- reads the body incrementally and counts raw bytes before decoding or JSON
  parsing;
- cancels the reader and fails closed on the first byte beyond the limit;
- accepts an exactly 262,144-byte valid JSON response; and
- propagates cancellation while preserving the existing same-origin request,
  accepted JSON media types and contract-validation boundaries.

Deterministic fake-stream regressions cover the exact limit, incremental
overflow, declared oversized length and abort during body reading. The
existing unsafe-local-URL and valid official/local citation regressions remain
present and passed in the complete suite.

The focused commit `20458c8189b132b775786b2fc8f9b44ee5c2f7b8`
implements the visual document title. The static fallback is Portuguese, and
the interface-language effect now updates both the root `lang` attribute and
`document.title` from `interfaceLanguage`. Tests exercise all eight
interface-language, question-language and theme combinations and prove that
the title depends only on `interfaceLanguage`:

- `pt-BR`: `RAG-Challenge — Documentação de bancos de dados`;
- `en-GB`: `RAG-Challenge — Database documentation`.

### Verification

The full offline verification ran from
`src/RagChallenge.Dashboard.Web` with the existing Node.js `24.18.1` and npm
`11.16.0` installation. No install command or `dotnet` command ran.

| Verification | Result |
| --- | --- |
| `npm run lint` | Passed, exit code 0 |
| `npm run typecheck` | Passed, exit code 0 |
| `npm test` | Passed, 34 tests, 0 failed/skipped/cancelled |
| `npm run build` | Passed, 20 modules transformed |
| Built HTML | 0.97 kB (0.56 kB gzip) |
| Built CSS | 11.96 kB (3.35 kB gzip) |
| Built JavaScript | 171.54 kB (55.19 kB gzip) |
| Package, lockfile and OpenAPI diff | Empty |
| OpenAPI SHA-256 | `D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34` |

The directed preflight found no task-owned listener on port 4173 and no
RAG-Challenge development server to stop. The only process whose command text
matched the inspection was the current PowerShell inspection process; it was
not a product process and was not stopped.

The built application was then served by the task-owned Vite preview process
on `127.0.0.1:4173`. Browser inspection observed an existing `en-GB`
preference with the English title, then the Portuguese title and root language
after selecting `pt-BR`, and the English values again after selecting `en-GB`.
No browser console warning or error was observed. The browser session was
finalised, the exact preview process was verified by executable and command
line before termination, and port 4173 was confirmed to have no remaining
listener.

### Disposition, limitations and next authority

`AQG-S05-002` and `AQG-S05-003` are
`CORRIGIDOS_PENDENTES_DE_RETESTE_DO_GATE`. The complete npm suite also
exercised the preserved `AQG-S05-001` regressions successfully, but this
corrective increment did not execute or approve the Automatic Quality Gate;
all three findings still require disposition by a separately authorised
complete restart.

JavaScript line and branch percentages remain unavailable in the existing
package scripts and dependency set. The observed Node.js patch version is
`24.18.1`, not the exact `24.18.0` pin. This correction validated the title in
a loopback browser, but it did not repeat the gate's complete styled visual,
external accessibility-engine, narrow-viewport/reflow, keyboard or
eight-combination browser review. No real backend, provider, account, secret,
corpus, official source or external network was exercised. No new P0 or P1
was observed within the authorised correction scope.

`STATE-05 FRONTEND_IMPLEMENTATION` remains active. Human Gate, `STATE-06` and
a new Automatic Quality Gate restart were not authorised or executed. The
next possible action is a separate explicit owner authority to restart the
complete `STATE-05` Automatic Quality Gate over the resulting clean baseline.

## Automatic Quality Gate restart after S05-CORR-02 — 2026-08-05

### Authority and baseline

- Gate baseline: branch `main`, commit
  `3f120aaf3cbc199c821685b161ece95a1988a659`, corpus `4.9.2`, clean working
  tree. Location, Git top-level, Git directory, branch, commit, corpus and
  cleanliness were reconfirmed immediately before the audit.
- Authority: restart the complete `STATE-05` Automatic Quality Gate locally,
  offline and sequentially, without product correction, dependency change,
  installation, external action, Human Gate or later lifecycle state.
- The audit restarted with the required authority, lifecycle, scope, contract
  and security inspection. The owner-defined stop condition applied to every
  material finding.

### Result

`REPROVADO`. One material P2 finding was observed during the initial static
inspection. The gate stopped before executable preflight, npm commands or
loopback browser validation. No product or test file was changed.

#### AQG-S05-004 — P2 — authorised local freshness is presented as unknown

- Requirement: coverage, provenance and freshness must be factual and fully
  localised for both trust classes and both interface languages.
- Canonical backend evidence:
  `src/RagChallenge.Application/IndexingRetrieval/QueryServices.cs` requires
  `SourceFreshness.Local` for every `LocalAuthorised` evidence binding, and
  `src/RagChallenge.Server.Api/Contracts/V1/QueryContracts.cs` serialises that
  enum value as `"Local"` in `sourceFreshness`.
- Dashboard mismatch: `src/RagChallenge.Dashboard.Web/src/i18n.ts` maps
  `Current`, `Stale`, `Unavailable`, `Withdrawn` and `Deactivated`, but not
  `Local`. `CitationCard` therefore falls back to `Estado não reconhecido` or
  `Unrecognised state` for a valid local citation. The response decoder accepts
  any non-empty freshness string and does not enforce the trust/freshness
  relation.
- Fixture gap: the synthetic local citation uses `sourceFreshness: "Current"`,
  a value rejected by the Application invariant for `LocalAuthorised`
  evidence. Existing presentation and matrix tests therefore mask the real
  contract value rather than exercising it.
- Impact: a valid API v1 response containing authorised local evidence shows
  an inaccurate freshness label in either interface language. This weakens
  the explicit provenance/freshness acceptance criterion and prevents the
  complete interface from being factually localised.
- Recommendation: under separate corrective authority, localise `Local` in
  both interface languages, enforce the canonical trust/freshness relation in
  the client decoder, correct the synthetic fixture and add contract and
  presentation regressions for valid local and invalid cross-class states.

### AQG-S05-001 to AQG-S05-003 re-evaluation

Static inspection confirmed that:

- `AQG-S05-001`: the decoder rejects non-HTTPS citation URLs and non-null
  local URLs, while presentation creates an anchor only for validated
  `OfficialExternal` HTTPS citations;
- `AQG-S05-002`: the response body is read incrementally, declared oversized
  decimal length is rejected before reader acquisition and the first
  incremental overflow cancels the reader; and
- `AQG-S05-003`: the static title fallback is `pt-BR`, and the
  interface-language effect updates the document title and root language from
  `interfaceLanguage` only.

The focused regressions for these corrections remain present. The mandatory
stop occurred before `npm test` and browser execution, so all three findings
remain corrected but pending executable retest and disposition by a later
complete gate restart.

### Checks stopped or not reached

- Executable runtime preflight was not reached; no process or listener was
  inspected, started or stopped.
- `npm run lint`, `npm run typecheck`, `npm test` and `npm run build` were not
  executed. Toolchain inspection observed Node.js `24.18.1` and npm `11.16.0`;
  the repository still pins Node.js `24.18.0` and npm `11.16.0`.
- JavaScript percentage coverage remains unavailable in the existing package
  scripts and dependency set; no percentage is claimed.
- Styled visual and external accessibility-engine checks, narrow viewport and
  reflow, keyboard, Light/Dark, `pt-BR`/`en-GB`, the eight-combination browser
  matrix and build reproducibility were not reached.
- No loopback listener or browser session was started, so no listener cleanup
  was required.
- Static scope inspection found no package, lockfile, OpenAPI, ADR, backend or
  other protected-path change in the `STATE-05` implementation range.

### Lifecycle consequence

`STATE-05 FRONTEND_IMPLEMENTATION` remains active. The restarted Automatic
Quality Gate is failed with `AQG-S05-004` open. `AQG-S05-001`,
`AQG-S05-002` and `AQG-S05-003` remain corrected but pending complete
executable gate retest. Human Gate and `STATE-06` remain not authorised and
not executed. Correction of `AQG-S05-004` and a later complete gate restart
require separate explicit owner authorities.

## S05-CORR-03 — 2026-08-05

### Authority and baseline

- Initial baseline: branch `main`, commit
  `800e6dc92d2a3555dbe92bc4e3b6b16e6411726b`, corpus `4.9.2`, clean
  working tree. Location, Git top-level, Git directory, branch, commit,
  corpus and cleanliness were reconfirmed immediately before the first
  alteration.
- Authority: execute only the local, offline and sequential `S05-CORR-03`
  correction for `AQG-S05-004`, preserve the `AQG-S05-001` through
  `AQG-S05-003` corrections, use the existing installation and create
  focused local commits.
- No dependency, package, lockfile, external contract, OpenAPI, ADR, backend,
  provider, installation or external action was authorised or performed.

### Correction

The focused commit `9ef937744302044ee3cd9105c9a23ddd3557a861`
aligns the Dashboard with the canonical API v1 freshness relation:

- `sourceFreshness` is decoded from the closed set `Local`, `Current`,
  `Stale`, `Withdrawn`, `Deactivated` and `Unavailable`;
- a `LocalAuthorised` citation is accepted only with `sourceFreshness:
  "Local"` and a null `canonicalUrl`;
- an `OfficialExternal` citation rejects `Local` and continues to require a
  validated HTTPS `canonicalUrl`, snapshot identity and revalidation instant;
- `Local` is owned by the exhaustive `pt-BR` and `en-GB` source-state maps;
  unknown degraded-source coverage values retain their bounded visual
  fallback; and
- the synthetic local citation now uses `Local`, while the official fixture
  remains `Current`.

Deterministic contract regressions cover the valid local citation, local
freshness paired with the official trust class, official freshness paired
with the local trust class and an unknown freshness value. Presentation
regressions render the valid local citation as `Local` under both interface
languages. The complete suite retained the URL-scheme and local-URL checks of
`AQG-S05-001`, the bounded incremental-response and cancellation checks of
`AQG-S05-002`, and the title/localisation matrix checks of `AQG-S05-003`.

### Verification

The full offline verification ran from
`src/RagChallenge.Dashboard.Web` with the existing Node.js `24.18.1` and npm
`11.16.0` installation. No install command or `dotnet` command ran.

| Verification | Result |
| --- | --- |
| `npm run typecheck` | Passed, exit code 0 after one in-scope type-guard correction |
| `npm run lint` | Passed, exit code 0 |
| `npm test` | Passed, 35 tests, 0 failed/skipped/cancelled |
| `npm run build` | Passed, 20 modules transformed |
| Built HTML | 0.97 kB (0.56 kB gzip) |
| Built CSS | 11.96 kB (3.35 kB gzip) |
| Built JavaScript | 171.75 kB (55.27 kB gzip) |
| Package, lockfile and OpenAPI diff | Empty |
| OpenAPI SHA-256 | `D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34` |

The first typecheck exposed that the now-exhaustive freshness map could not be
indexed by the open degraded-source coverage string. The in-scope correction
added a freshness type guard, preserving the existing unknown-state fallback
for coverage without weakening citation decoding. The complete verification
sequence then passed.

The directed preflight found no RAG-Challenge listener on ports 4173 or 5173
and no Dashboard development server to stop. The observed Node.js process was
the Codex browser-control runtime, not a product listener, and was not
stopped. The built application was served by the task-owned Vite preview on
`127.0.0.1:4173`. Browser inspection confirmed the English title and heading,
the Portuguese title and heading after switching to `pt-BR`, and the English
values again after switching to `en-GB`; no console error was observed. The
browser session was finalised, the preview process was revalidated by
executable and command line before termination, and port 4173 was confirmed
clear. Local citation rendering itself was verified by the deterministic
component presentation tests with synthetic data and no backend or network.

### Disposition, limitations and next authority

`AQG-S05-004` is `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`. The complete npm
suite also exercised the preserved regressions for `AQG-S05-001`,
`AQG-S05-002` and `AQG-S05-003`, but this corrective increment did not
execute or approve the Automatic Quality Gate. All four findings still
require disposition by a separately authorised complete restart.

JavaScript line and branch percentages remain unavailable in the existing
package scripts and dependency set. The observed Node.js patch version is
`24.18.1`, not the exact `24.18.0` pin. This increment did not repeat the
gate's complete styled visual, external accessibility-engine,
narrow-viewport/reflow, keyboard, Light/Dark or eight-combination browser
review. No real backend, provider, account, secret, corpus, official source or
external network was exercised. No new P0 or P1 was observed within the
authorised correction scope.

`STATE-05 FRONTEND_IMPLEMENTATION` remains active. Automatic Quality Gate,
Human Gate and `STATE-06` were not authorised or executed. The next possible
action is a separate explicit owner authority to restart the complete
`STATE-05` Automatic Quality Gate over the resulting clean baseline.

## Automatic Quality Gate restart after S05-CORR-03 — 2026-08-05

### Authority and baseline

- Gate baseline: branch `main`, commit
  `b457970aed4564d5a654bb4e8d38439c98f29522`, corpus `4.9.2`, clean working
  tree. Location, Git top-level, Git directory, branch, commit, corpus and
  cleanliness were reconfirmed immediately before the audit.
- Authority: restart the complete `STATE-05` Automatic Quality Gate locally,
  offline and sequentially, without product correction, dependency change,
  installation, external action, Human Gate or later lifecycle state.
- The audit restarted with the required authority, lifecycle, scope, contract
  and security inspection. The owner-defined stop condition applied to every
  material finding.

### Result

`REPROVADO`. One material P2 finding was observed during the initial static
inspection. The gate stopped before executable preflight, npm commands or
loopback browser validation. No product or test file was changed.

#### AQG-S05-005 — P2 — completed response language is not bound to the request

- Requirement: every completed API v1 response must have `answerLanguage`
  equal to the accepted `questionLanguage`. The Dashboard must fail closed on
  an incompatible response rather than present an answer under a different
  language contract.
- Location: `src/RagChallenge.Dashboard.Web/src/contracts/api-v1.ts` validates
  `answerLanguage` only as a member of the supported-language set.
  `src/RagChallenge.Dashboard.Web/src/query-client.ts` calls
  `decodeQueryResponse(payload)` without supplying or comparing the
  `questionLanguage` used to create the request.
- Existing test evidence: the shared answered fixture declares
  `answerLanguage: "pt-BR"`. The exact-response-ceiling transport test sends
  an `en-GB` question, receives that fixture and accepts the result as
  completed. The eight-combination presentation test constructs matching
  response values directly and therefore does not exercise the transport
  mismatch.
- Impact: a malformed, compromised or incompatible same-origin API response
  can return a supported but different answer language. The Dashboard then
  presents and marks that answer with the response language despite the
  user's explicit query-language selection, violating the v1 compatibility
  boundary and the bilingual experience contract.
- Recommendation: under separate corrective authority, bind completion
  decoding to the request language, reject every mismatch, and add
  deterministic client/contract regressions for both valid languages and the
  two cross-language mismatch directions.

### AQG-S05-001 to AQG-S05-004 re-evaluation

Static inspection confirmed that:

- `AQG-S05-001`: non-HTTPS citation URLs and every non-null local URL are
  rejected, and presentation creates links only for validated official HTTPS
  citations;
- `AQG-S05-002`: response reading rejects a declared oversized length before
  reader acquisition, counts incremental bytes, cancels on the first overflow
  and preserves abort propagation;
- `AQG-S05-003`: the Portuguese static title is replaced from
  `interfaceLanguage`, independently of query language and theme; and
- `AQG-S05-004`: citation freshness uses the closed canonical set,
  `LocalAuthorised` requires `Local`, `OfficialExternal` rejects `Local`, and
  both interface maps own the `Local` label.

The focused regressions for all four corrections remain present. The
mandatory stop occurred before `npm test` and browser execution, so the four
findings remain corrected but pending executable retest and disposition by a
later complete gate restart.

### Checks stopped or not reached

- Executable runtime preflight was not reached; no process or listener was
  inspected, started or stopped.
- `npm run lint`, `npm run typecheck`, `npm test` and `npm run build` were not
  executed. Toolchain inspection was not used as executable gate evidence.
- JavaScript percentage coverage remains unavailable in the existing package
  scripts and dependency set; no percentage is claimed.
- Styled visual and external accessibility-engine checks, narrow viewport and
  reflow, keyboard, Light/Dark, `pt-BR`/`en-GB`, the eight-combination browser
  matrix and build reproducibility were not reached.
- No loopback listener or browser session was started, so no listener cleanup
  was required.
- Static scope inspection found no package, lockfile, OpenAPI, ADR, backend or
  other protected-path change in the `STATE-05` implementation range. The
  OpenAPI SHA-256 remained
  `D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34`.

### Lifecycle consequence

`STATE-05 FRONTEND_IMPLEMENTATION` remains active. The restarted Automatic
Quality Gate is failed with `AQG-S05-005` open. `AQG-S05-001` through
`AQG-S05-004` remain corrected but pending complete executable gate retest.
Human Gate and `STATE-06` remain not authorised and not executed. Correction
of `AQG-S05-005` and a later complete gate restart require separate explicit
owner authorities.

## S05-CORR-04 — 2026-08-05

### Authority and baseline

- Initial baseline: location `C:\Projects\RAG-Challenge`, Git top-level
  `C:/Projects/RAG-Challenge`, Git directory `.git`, branch `main`, commit
  `fb59861a8367749f2a11ac279add5007989d27e0`, corpus `4.9.2` and clean
  working tree. All seven conditions were reconfirmed immediately before the
  first alteration.
- Authority: local, offline, sequential and limited correction of
  `AQG-S05-005`, with deterministic contract and client regressions. The
  Automatic Quality Gate, Human Gate and later lifecycle states were not
  authorised.

### Correction and regressions

The focused commit `bed8ec03d670ed4e76a556f7df723c30db320a24`
binds every completed response to the request that produced it:

- `decodeQueryResponse` requires the expected answer language and rejects a
  supported but different `answerLanguage`;
- `askQuestion` supplies the same `questionLanguage` used to construct the
  request body to completion decoding;
- synthetic completed fixtures now represent both `pt-BR` and `en-GB`; and
- the exact 262,144-byte transport fixture now matches its `en-GB` request
  instead of accepting the former `pt-BR` mismatch.

Contract and client regressions cover valid `pt-BR`, valid `en-GB`, invalid
`pt-BR` to `en-GB` and invalid `en-GB` to `pt-BR` completions. The complete
suite also retained the citation URL, incremental body limit, cancellation,
document-title localisation and source-freshness regressions associated with
`AQG-S05-001` through `AQG-S05-004`.

### Verification

All commands ran from `src/RagChallenge.Dashboard.Web` with the existing
installation and `npm_config_offline=true`. No installation, `dotnet`, real
backend or network command ran.

| Verification | Result |
| --- | --- |
| `npm run lint` | Passed, exit code 0 |
| `npm run typecheck` | Passed, exit code 0 |
| `npm test` | Passed, 37 tests, 0 failed/skipped/cancelled |
| `npm run build` | Passed, 20 modules transformed |
| Built HTML | 0.97 kB (0.56 kB gzip) |
| Built CSS | 11.96 kB (3.35 kB gzip) |
| Built JavaScript | 171.86 kB (55.29 kB gzip) |
| Package, lockfile and OpenAPI diff | Empty |
| OpenAPI SHA-256 | `D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34` |

The directed preflight observed one Node.js process whose executable belonged
to the Codex runtime rather than RAG-Challenge and found no project-owned
listener. It was not stopped. No loopback listener was needed or started:
the correction changes only contract/client acceptance, and the authorised
language behaviour was exercised deterministically with fake fetch responses.

### Disposition, limitations and next authority

`AQG-S05-005` is `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`.
`AQG-S05-001` through `AQG-S05-004` remain in the same status. Passing the
complete local suite in this corrective increment does not execute or approve
the Automatic Quality Gate and does not dispose any finding.

JavaScript line and branch percentages remain unavailable in the existing
package scripts and dependency set. Node.js `24.18.1` was observed instead of
the exact `24.18.0` pin. This correction did not repeat complete styled
visual, external accessibility-engine, narrow-viewport/reflow, keyboard,
Light/Dark or eight-combination browser review. No real backend, provider,
account, secret, corpus, official source or external network was exercised.
No new P0/P1, security or accessibility failure was observed within the
authorised correction scope.

`STATE-05 FRONTEND_IMPLEMENTATION` remains active. A complete restart of its
Automatic Quality Gate requires new explicit and separate owner authority.
Human Gate and `STATE-06` remain not authorised and not executed.



## Automatic Quality Gate restart after S05-CORR-04 — 2026-08-05

### Authority and baseline

- Gate baseline: location `C:\Projects\RAG-Challenge`, Git top-level
  `C:/Projects/RAG-Challenge`, Git directory `.git`, branch `main`, commit
  `a58c4038fb14e656c95303d914e02c7f8ad75c17`, corpus `4.9.2` and clean
  working tree. All seven conditions were reconfirmed immediately before the
  audit.
- Authority: restart the complete `STATE-05` Automatic Quality Gate locally,
  offline and sequentially, using only the existing installation, synthetic
  fixtures, fake fetch and a task-owned loopback listener. Product correction,
  dependency changes, installation, external actions, Human Gate and later
  lifecycle states remained outside authority.
- The audit restarted with authority, lifecycle, scope, contract and security
  inspection. The owner-defined stop condition applied to every material
  finding.

### Result

`REPROVADO`. The static inspection and complete existing npm suite disposed
`AQG-S05-001` through `AQG-S05-005` as `RESOLVIDOS`, but the browser keyboard
reproduction found `AQG-S05-006` (P2). The gate stopped before the remaining
narrow-viewport, reflow, theme and eight-combination browser checks. No
product or test file was changed.

#### AQG-S05-001 to AQG-S05-005 disposition

- `AQG-S05-001`: `RESOLVIDO`. Static inspection confirmed the HTTPS-only
  citation boundary, null URL for `LocalAuthorised` and official-link
  presentation guard. The unsafe local URL regressions passed.
- `AQG-S05-002`: `RESOLVIDO`. The client rejects an oversized declared length
  before acquiring the reader, counts incremental bytes, cancels on the first
  overflow and preserves cancellation. Boundary and streaming regressions
  passed.
- `AQG-S05-003`: `RESOLVIDO`. The static fallback is Portuguese and the
  document metadata follows only `interfaceLanguage`. Both titles and all
  eight state combinations passed the deterministic matrix test; the built
  browser page also presented metadata consistent with its persisted `en-GB`
  preference.
- `AQG-S05-004`: `RESOLVIDO`. Citation freshness uses the closed canonical
  set, local evidence requires `Local`, official evidence rejects `Local`, and
  both interface maps own the localised presentation. Contract and component
  regressions passed.
- `AQG-S05-005`: `RESOLVIDO`. Completion decoding is bound to the
  `questionLanguage` actually sent. The two valid and two incompatible
  language directions passed in contract and fake-fetch client regressions.

#### AQG-S05-006 — P2 — skip link does not transfer focus to the main content

- Requirement: keyboard users must be able to bypass repeated header controls,
  with visible and deterministic focus behaviour.
- Location: `src/RagChallenge.Dashboard.Web/src/App.tsx` links the first focus
  target to `#main-content`, but the target `<main>` is not programmatically
  focusable and activation has no focus-management handler.
- Reproduction: on the built application served from the task-owned loopback
  listener, the first `Tab` focused `Skip to content` and exposed its visible
  solid outline. Activating it changed the URL fragment to `#main-content`,
  but `document.activeElement` became `<body>` rather than the target
  `<main>`.
- Impact: the skip link does not provide a reliable keyboard-focus bypass of
  the repeated visual-preference controls. The presence and visible focus of
  the link do not satisfy its intended navigation behaviour.
- Recommendation: under separate corrective authority, make the main target
  programmatically focusable and move focus to it on skip-link activation,
  then add a browser regression that proves both the target focus and the next
  tab order.

### Executable verification reached before the stop

All npm commands ran from `src/RagChallenge.Dashboard.Web` with
`npm_config_offline=true`, the existing Node.js `24.18.1` and npm `11.16.0`
installation, and no install or `dotnet` command.

| Verification | Result |
| --- | --- |
| `npm run lint` | Passed, exit code 0 |
| `npm run typecheck` | Passed, exit code 0 |
| `npm test` | Passed, 37 tests, 0 failed/skipped/cancelled |
| `npm run build` | Passed, 20 modules transformed |
| Repeated build | Byte-for-byte identical SHA-256 manifest for all three built files |
| Built HTML | 0.97 kB (0.56 kB gzip) |
| Built CSS | 11.96 kB (3.35 kB gzip) |
| Built JavaScript | 171.86 kB (55.29 kB gzip) |
| Working tree after executable checks | Clean |

The directed preflight found no RAG-Challenge listener on ports 4173 or 5173
and no product process to stop. The existing matching Node.js process belonged
to the Codex browser-control runtime and was not stopped. The built application
was then served exclusively on `127.0.0.1:4173` by a task-owned Vite preview.
The exact preview process was revalidated by executable, command line, address
and port before termination; port 4173 was clear afterwards.

### Checks stopped or limitations retained

- JavaScript line and branch percentages remain unavailable because the
  existing package scripts and dependency set contain no instrumentation.
  No percentage or repository coverage floor is claimed for the Dashboard.
- Node.js `24.18.1` was observed instead of the exact repository pin
  `24.18.0`. Build output reproduced byte for byte on the observed runtime,
  but exact-Node reproducibility was not proved.
- The browser screenshot operation timed out. Semantic structure, labels,
  visible keyboard focus and computed runtime state were inspected, but no
  screenshot is claimed as evidence.
- Narrow viewport, reflow, the complete Light/Dark browser switch, the two
  interface languages and the eight-combination browser matrix were not
  reached after the mandatory stop. Their deterministic component, CSS-token
  and fake-fetch tests passed before the finding.
- No third-party accessibility engine was installed or run. No real backend,
  provider, account, secret, corpus, official source or external network was
  exercised.
- A sanitised task log directory outside the repository remained in the
  system temporary directory after the execution policy refused its deletion.
  It contains no listener, secret, corpus or tracked project change.

### Lifecycle consequence

`STATE-05 FRONTEND_IMPLEMENTATION` remains active. Its Automatic Quality Gate
is failed with `AQG-S05-006` open; `AQG-S05-001` through `AQG-S05-005` are
resolved by this restart. Human Gate and `STATE-06` remain not authorised and
not executed. Correction of `AQG-S05-006` and any later complete gate restart
require separate explicit owner authorities.

## S05-CORR-05 — 2026-08-05

### Authority and baseline

- Initial baseline: location `C:\Projects\RAG-Challenge`, Git top-level
  `C:/Projects/RAG-Challenge`, Git directory `.git`, branch `main`, commit
  `3ff7002b394199bbf253139836827231c1988116`, corpus `4.9.2` and clean
  working tree. All seven conditions were reconfirmed immediately before the
  executable preflight and first alteration.
- Authority: local, offline, sequential and limited correction of
  `AQG-S05-006`, with a focal component regression, the four existing npm
  checks, factual records and focused local commits. The Automatic Quality
  Gate, Human Gate and later lifecycle states were not authorised.

### Correction and regression

The focused commit `8b543eb85907b5aa4023f109dabb4bb11100da3e`:

- makes `main#main-content` programmatically focusable with `tabindex="-1"`;
- retains the semantic skip-link destination while its activation prevents
  default fragment navigation and explicitly focuses the main target; and
- adds a component regression that exercises the focus-transfer function and
  proves the structural order from the skip link to the main target and its
  next in-main control.

### Verification

All final commands ran from `src/RagChallenge.Dashboard.Web` with the existing
installation and `npm_config_offline=true`. No installation, `dotnet`, real
backend or external-network command ran.

| Verification | Result |
| --- | --- |
| `npm run lint` | Passed, exit code 0 |
| `npm run typecheck` | Passed, exit code 0 after one in-scope local typing correction |
| `npm test` | Passed, 38 tests, 0 failed/skipped/cancelled |
| `npm run build` | Passed, 20 modules transformed |
| Built HTML | 0.97 kB (0.56 kB gzip) |
| Built CSS | 11.96 kB (3.35 kB gzip) |
| Built JavaScript | 171.97 kB (55.33 kB gzip) |
| Package, lockfile and OpenAPI diff | Empty |

The first typecheck attempt exposed that the repository's deliberately minimal
React declaration does not export `MouseEvent`. The in-scope implementation
was corrected to use a local structural activation-event type; no dependency
or shared declaration changed. Typecheck and the remaining final checks then
passed.

The directed preflight found no RAG-Challenge listener on ports 4173 or 5173
and no product process to stop. The built application was served exclusively
by the task-owned Vite preview on `127.0.0.1:4173`. Browser keyboard validation
observed this sequence on the persisted `en-GB`/`Light` interface:

1. the first `Tab` focused the visible `Skip to content` link;
2. `Enter` made `MAIN#main-content` with `tabindex="-1"` the active element;
3. the next `Tab` focused the selected `en-GB` question-language radio inside
   the main content.

The focus transfer intentionally leaves the URL fragment unchanged because the
handler prevents default navigation before focusing the target. No browser
warning or error was observed. The exact preview process was revalidated by
PID, executable, command line, address and port before termination; port 4173
was clear afterwards.

### Disposition, limitations and next authority

`AQG-S05-006` is `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`. The correction,
component regression, complete npm suite and directed browser keyboard path
passed, but this increment did not restart or approve the Automatic Quality
Gate and cannot dispose the finding. `AQG-S05-001` through `AQG-S05-005`
retain their prior `RESOLVIDOS` disposition.

JavaScript line and branch percentages remain unavailable in the existing
package scripts and dependency set. Node.js `24.18.1` was observed instead of
the exact `24.18.0` pin. This correction did not repeat the complete gate,
narrow-viewport/reflow review, full Light/Dark and language browser matrix,
screenshot review or an external accessibility engine. No real backend,
provider, account, secret, corpus, official source or external network was
exercised. No new material finding or P0/P1 was observed within the authorised
correction scope.

`STATE-05 FRONTEND_IMPLEMENTATION` remains active. A complete restart of its
Automatic Quality Gate requires new explicit and separate owner authority.
Human Gate and `STATE-06` remain not authorised and not executed.

## Automatic Quality Gate restart after S05-CORR-05 — 2026-08-05

### Authority, baseline and result

The gate baseline was location `C:\Projects\RAG-Challenge`, Git top-level
`C:/Projects/RAG-Challenge`, Git directory `.git`, branch `main`, commit
`8ee1213eed3522493204c68b4f843e9c438e0f69`, corpus `4.9.2` and a clean
working tree. All seven conditions were reconfirmed immediately before the
executable preflight and audit.

The complete `STATE-05` Automatic Quality Gate restarted locally, offline and
sequentially with only the existing installation, synthetic fixtures, fake
fetch and a task-owned loopback listener. Authority, lifecycle, scope,
contract and security inspection was repeated from the beginning. Product
correction, dependencies, installation, external actions, Human Gate and later
states remained outside authority.

Result: `REPROVADO`. `AQG-S05-001` through `AQG-S05-006` are `RESOLVIDOS`,
but the narrow-browser matrix found `AQG-S05-007` (P2). The owner-defined stop
condition was applied without any product or test correction.

### Finding disposition

- `AQG-S05-001`: `RESOLVIDO`; official citation links remain HTTPS-only and
  local evidence requires a null URL.
- `AQG-S05-002`: `RESOLVIDO`; the response reader enforces declared and
  streamed 262,144-byte limits before full materialisation and preserves
  cancellation.
- `AQG-S05-003`: `RESOLVIDO`; browser and component matrices confirmed that
  document language and title follow only `interfaceLanguage`.
- `AQG-S05-004`: `RESOLVIDO`; the closed freshness model, provenance
  relationships and both localised presentations passed.
- `AQG-S05-005`: `RESOLVIDO`; completion decoding remains bound to the
  `questionLanguage` sent, including both incompatible fake-fetch directions.
- `AQG-S05-006`: `RESOLVIDO`; the first `Tab` visibly focused the skip link,
  `Enter` focused `MAIN#main-content` with `tabindex="-1"`, and the next
  `Tab` reached the selected question-language radio inside the main content.

#### AQG-S05-007 — P2 — pt-BR narrow viewport creates horizontal overflow

At an observed `innerWidth` of 320 CSS pixels and a 303-pixel document client
width after the vertical scrollbar, every `pt-BR` combination produced a
document `scrollWidth` of 355 pixels in both `Light` and `Dark` and for both
question languages. All four equivalent `en-GB` combinations retained a
303-pixel `scrollWidth` without horizontal overflow.

The `pt-BR` hero expanded beyond its 287-pixel container; its content measured
approximately 348 pixels and extended to approximately 356 pixels from the
viewport origin. Direct styled visual inspection showed clipped hero and
workspace content plus a horizontal scrollbar. Narrow-screen, low-vision and
keyboard users of the supported Portuguese interface must therefore pan
horizontally to read and operate the page, contrary to the required reflow.

Under separate corrective authority, the Portuguese min-content expansion
should be removed at the narrow breakpoint without weakening supported content
or focus behaviour, with a focal `pt-BR` 320-pixel regression. A later complete
gate restart requires its own separate authority.

### Verification and reproducibility

All npm commands ran from `src/RagChallenge.Dashboard.Web` with
`npm_config_offline=true`, Node.js `24.18.1`, npm `11.16.0` and the existing
installation. No install, `dotnet`, real backend or external-network command
ran.

| Verification | Result |
| --- | --- |
| `npm run lint` | Passed, exit code 0 |
| `npm run typecheck` | Passed, exit code 0 |
| `npm test` | Passed, 38 tests, 0 failed/skipped/cancelled |
| `npm run build` | Passed, 20 modules transformed |
| Repeated build | Byte-for-byte identical SHA-256 manifest for all three files |
| Built HTML | 0.97 kB (0.56 kB gzip) |
| Built CSS | 11.96 kB (3.35 kB gzip) |
| Built JavaScript | 171.97 kB (55.33 kB gzip) |
| Default-width matrix | All eight combinations passed |
| Narrow matrix | Four `pt-BR` combinations failed; four `en-GB` combinations passed |
| Working tree after executable checks | Clean |

The repeated manifest was `index.html`, 977 bytes,
`8EA669A1672752FC91E5864975DE2054DF07184D6E4397EBCE6552EE0AE73473`;
`assets/index-DyZXAIuo.css`, 11,963 bytes,
`7B43E53F5E614778D571649141D8367CF7FFB4E300AAA690E58F12E10F7BBF1D`;
and `assets/index-DC8AuelO.js`, 171,973 bytes,
`1D1E4981B8B34FB585260DACCB32646C0594A3C0F4CF080DA35A73625CC7DCCA`.

The default-width browser inspection found one header, main and footer, one
H1, no duplicate IDs, no unnamed interactive control and no horizontal
overflow. Focus outlines were solid and visible. Both interface languages,
both question languages and both themes preserved localised titles, H1 text,
canonical checked/pressed state and the synthetic input.

Preflight found no product listener on ports 4173 or 5173. The built app was
served only on `127.0.0.1:4173`; the task-owned preview's PID, executable,
command line and port were revalidated before termination, and port 4173
finished clear.

### Limitations, risks and lifecycle consequence

JavaScript line and branch percentages remain unavailable because the existing
scripts and dependency set provide no coverage instrumentation. No Dashboard
coverage percentage or repository floor is claimed. Node.js `24.18.1` was
observed instead of the `24.18.0` pin, so reproduction passed on the observed
runtime rather than the exact pin. No third-party accessibility engine was
installed; semantic, label, focus, computed-layout and direct styled visual
inspection used the existing browser surface. No real backend, provider,
account, secret, corpus, official source or external network was exercised.

`STATE-05 FRONTEND_IMPLEMENTATION` remains active. Its Automatic Quality Gate
is failed with `AQG-S05-007` open; `AQG-S05-001` through `AQG-S05-006` are
resolved. No P0 or P1 was observed. Human Gate and `STATE-06` remain not
authorised and not executed. Correction of `AQG-S05-007` and any later full
gate restart require separate explicit owner authorities.

## S05-CORR-06 — 2026-08-05

### Authority and baseline

- Initial baseline: location `C:\Projects\RAG-Challenge`, Git top-level
  `C:/Projects/RAG-Challenge`, Git directory `.git`, branch `main`, commit
  `c32953eceb149efa3cfeb952f1dbfdbe0c00e2eb`, corpus `4.9.2` and clean
  working tree. All seven conditions were reconfirmed immediately before the
  executable preflight and first alteration.
- Authority: local, offline, sequential and limited correction of
  `AQG-S05-007`, with a focal narrow-layout regression, the four existing npm
  checks, an extension-free task-owned browser context and focused local
  commits. The Automatic Quality Gate, Human Gate and later lifecycle states
  were not authorised.

### Correction and regression

The focused commit `e34e73c7bbe8fabf96d5a5683df35935a3266e37`:

- keeps the single responsive hero grid track shrinkable with
  `minmax(0, 1fr)`, instead of restoring its intrinsic `1fr` minimum;
- reduces the compact heading's fluid viewport term from `14vw` to `11vw`,
  retaining a 2.25 rem minimum while fitting the current Portuguese
  min-content at 320 CSS pixels; and
- extends the existing deterministic eight-combination matrix regression to
  pin both narrow-layout constraints while continuing to render both
  interface languages, both question languages and both themes.

### Verification

All final commands ran from `src/RagChallenge.Dashboard.Web` with the existing
installation, `npm_config_offline=true`, Node.js `24.18.1` and npm `11.16.0`.
No installation, `dotnet`, real backend or external-network command ran.

| Verification | Result |
| --- | --- |
| `npm run lint` | Passed, exit code 0 |
| `npm run typecheck` | Passed, exit code 0 |
| `npm test` | Passed, 38 tests, 0 failed/skipped/cancelled |
| `npm run build` | Passed, 20 modules transformed |
| Built HTML | 0.97 kB (0.56 kB gzip) |
| Built CSS | 11.97 kB (3.35 kB gzip) |
| Built JavaScript | 171.97 kB (55.33 kB gzip) |
| Package, lockfile and OpenAPI diff | Empty |

The final browser run used a fresh temporary Chrome profile with extensions,
component extension backgrounds and background networking disabled. The CDP
inventory contained zero extension targets, so Dark Reader and other
page-modifying extensions did not participate. The built application was
served only on `127.0.0.1:4173`, and the viewport override observed an
`innerWidth` of 320 CSS pixels and a 305-pixel document client width.

All eight `interfaceLanguage` × `questionLanguage` × theme combinations
passed with `scrollWidth` equal to 305 pixels. The hero and heading each fit
their 289-pixel content width; the heading computed to 36 pixels and its
`scrollWidth` equalled its `clientWidth`. Root language, localised title,
selected question-language radio, two pressed preference controls and
`Light`/`Dark` state remained consistent in every combination. Computed body
colours were `rgb(241, 239, 232)` / `rgb(23, 33, 29)` in Light and
`rgb(16, 23, 20)` / `rgb(245, 242, 233)` in Dark. Direct screenshots of the
Portuguese Light and Dark states showed readable vertical reflow without
clipped content or horizontal panning.

The complete keyboard path also passed in every combination: the first
`Tab` exposed the skip link with a solid three-pixel outline, `Enter` focused
`MAIN#main-content` with `tabindex="-1"`, and the next `Tab` focused the
selected question-language radio. No runtime exception occurred. Chrome made
one non-material request for the pre-existing absent `/favicon.ico`, which
returned 404; the JavaScript and CSS assets returned 200 and no product
correction was made for that unrelated diagnostic.

The exact preview and isolated-Chrome processes were revalidated by
executable, command line, profile, address and port before termination. Ports
4173 and 9230 finished without listeners. The execution policy refused the
recursive deletion of five task-specific directories under the system
temporary directory; they contain only sanitised preview logs, temporary
browser profiles and screenshots, with no running process, listener, secret,
real corpus or tracked project change.

### Disposition, limitations and next authority

`AQG-S05-007` is `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`. The focused
regression, complete npm suite, extension-free narrow matrix, visual review
and keyboard repetition passed, but this corrective increment did not restart
or approve the Automatic Quality Gate and cannot dispose the finding.
`AQG-S05-001` through `AQG-S05-006` retain their prior `RESOLVIDOS`
disposition.

JavaScript line and branch percentages remain unavailable in the existing
package scripts and dependency set. Node.js `24.18.1` was observed instead of
the exact `24.18.0` pin. No third-party accessibility engine, real backend,
provider, account, secret, corpus, official source or external network was
used. No new material finding or P0/P1 was observed within the authorised
correction scope.

`STATE-05 FRONTEND_IMPLEMENTATION` remains active. A complete restart of its
Automatic Quality Gate requires new explicit and separate owner authority.
Human Gate and `STATE-06` remain not authorised and not executed.

## Automatic Quality Gate restart after S05-CORR-06 — 2026-08-05

### Authority, baseline and result

The gate baseline was location `C:\Projects\RAG-Challenge`, Git top-level
`C:/Projects/RAG-Challenge`, Git directory `.git`, branch `main`, commit
`bc2ddd6bf64fc82f7d68eb518c3013d85655c16a`, corpus `4.9.2` and a clean
working tree. All seven conditions were reconfirmed before the audit.

The complete `STATE-05` Automatic Quality Gate restarted locally, offline and
sequentially. Authority, lifecycle, scope, contract and security inspection
was repeated from the beginning. The owner-defined stop condition was applied
during static inspection, before executable preflight, npm checks, build or
browser validation.

Result: `REPROVADO`. `AQG-S05-001` through `AQG-S05-006` retain their prior
`RESOLVIDOS` disposition. The static controls for `AQG-S05-007` were present,
but its gate retest was not completed before the stop, so it remains
`CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`. `AQG-S05-008` (P2) is open. No
product or test correction was made.

### Finding disposition

- `AQG-S05-001` through `AQG-S05-006`: prior `RESOLVIDOS` dispositions
  retained. Static inspection reconfirmed their implemented controls, but the
  stopped restart did not repeat executable or browser evidence.
- `AQG-S05-007`: `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`. The narrow hero
  track, compact heading constraint and deterministic eight-combination
  regression are present, but the complete gate did not reach npm or browser
  execution and therefore did not dispose the finding.

#### AQG-S05-008 — P2 — untrusted long tokens can break narrow reflow

The completed-result presentation renders API-derived answer text, citation
titles and citation excerpts in `.answer-copy`, `.citation-card h4` and
`.citation-card blockquote`. These three surfaces do not apply an
`overflow-wrap` or equivalent rule that can break a continuous token. The
answer preserves whitespace with `white-space: pre-wrap`, which does not by
itself break an otherwise unbroken token.

The versioned OpenAPI contract permits these fields as strings without an
individual maximum length or a requirement that they contain break
opportunities. Its SHA-256 remained
`D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34`.
The client bounds the total response stream to 262,144 bytes, but a valid
response can still devote a long continuous value to any affected field. At a
narrow supported viewport, CSS intrinsic sizing can consequently widen a
result card or its scrollable ancestor beyond the viewport.

The existing hostile-content regression proves text escaping, and the narrow
eight-combination regression pins the hero constraints, but neither exercises
a completed answer, citation title or citation excerpt containing a long
unbroken token. The supported reflow requirement is therefore not preserved
for all contract-valid, untrusted result content.

Under separate corrective authority, the three affected text surfaces should
permit safe token wrapping without changing or truncating evidence, and a
focal regression should exercise contract-valid long unbroken values at the
narrow viewport. A later complete gate restart requires its own separate
authority.

### Static verification and stopped checks

Static inspection reconfirmed the accepted lifecycle and frontend boundary,
the same-origin API surface, strict document CSP, HTTPS-only official citation
links, null local citation URLs, streamed response limit, explicit language
binding, closed trust/freshness relationships, interface-language document
metadata and skip-link focus implementation. The scoped source history
contained only the authorised Dashboard and state-evidence work. Package,
lockfile, OpenAPI, contracts, ADRs, backend and other protected surfaces had no
diff from the `STATE-05` entry baseline. A targeted secret-marker scan found
no secret value.

Because the material finding triggered the mandatory stop:

- runtime preflight was not applicable after the stop; no process or listener
  was inspected, started or stopped;
- `npm run lint`, `npm run typecheck`, `npm test` and `npm run build` were not
  run;
- build reproducibility and JavaScript coverage capability were not rerun;
- no extension-free browser context was started, so visual accessibility,
  narrow viewport, reflow, keyboard, focus, Light/Dark, `pt-BR`/`en-GB` and
  the eight-combination browser matrix were not repeated; and
- no frontend, code, test, dependency, package, lockfile, contract, OpenAPI,
  ADR, backend or configuration file was changed.

### Limitations, risks and lifecycle consequence

This restart establishes a static, contract-reachable reflow defect; it does
not include a browser measurement of a chosen hostile token. JavaScript line
and branch percentages remain unavailable in the existing package scripts
and dependency set. No third-party accessibility engine, real backend,
provider, account, secret, corpus, official source or external network was
used.

`STATE-05 FRONTEND_IMPLEMENTATION` remains active. Its Automatic Quality Gate
is failed with `AQG-S05-008` open. `AQG-S05-007` remains corrected pending a
complete gate retest, and `AQG-S05-001` through `AQG-S05-006` retain their
resolved dispositions. No new P0 or P1 was observed. Human Gate and
`STATE-06` remain not authorised and not executed. Correction of
`AQG-S05-008` and any later complete gate restart require separate explicit
owner authorities.

## S05-CORR-07 — 2026-08-05

### Authority and baseline

- Initial baseline: location `C:\Projects\RAG-Challenge`, Git top-level
  `C:/Projects/RAG-Challenge`, Git directory `.git`, branch `main`, commit
  `dfa31d02e8ba3fd171986ea2c1d06c70101d07a3`, corpus `4.9.2` and clean
  working tree. All seven conditions were reconfirmed immediately before the
  executable preflight and first alteration.
- Authority: local, offline, sequential and limited correction of
  `AQG-S05-008`, with a focal contract-valid long-token regression, the four
  existing npm checks, extension-free loopback browser validation and focused
  local commits. The Automatic Quality Gate, Human Gate and later lifecycle
  states were not authorised.

### Correction and regression

The focused commit `3f003b9db67eefeccc7e677c319ca37a26d49fa7`:

- applies `overflow-wrap: anywhere` to the untrusted completed answer,
  citation title and citation excerpt surfaces without truncating, hiding or
  changing their text;
- extends the deterministic eight-combination matrix with separate 517- to
  519-character unbroken answer, title and excerpt values;
- passes those values through the API v1 decoder for each matching question
  language, proving they remain contract-valid; and
- verifies that all three complete values reach escaped React text output in
  both interface languages, both question languages and both themes.

### Verification

All npm commands ran from `src/RagChallenge.Dashboard.Web` with the existing
installation, `npm_config_offline=true`, Node.js `24.18.1` and npm `11.16.0`.
No installation, `dotnet` or real backend command ran.

| Verification | Result |
| --- | --- |
| `npm run lint` | Passed, exit code 0 |
| `npm run typecheck` | Passed, exit code 0 |
| `npm test` | Passed, 38 tests, 0 failed/skipped/cancelled |
| `npm run build` | Passed, 20 modules transformed |
| Built HTML | 977 bytes |
| Built CSS | 12,053 bytes |
| Built JavaScript | 171,973 bytes |
| Package, lockfile, OpenAPI and protected backend diff | Empty |
| OpenAPI SHA-256 | `D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34` |

The directed preflight found no listener on ports 4173 or 9230. The built
application was served only on `127.0.0.1:4173` and the isolated Chrome debug
surface only on `127.0.0.1:9230`.

### Browser incident, stop and controlled resumption

The available connected Chrome surface belonged to the owner's ordinary
profile and could include Dark Reader, so no tab was opened there. Browser
validation instead used a task-specific headless Chrome profile with
extensions disabled.

During the first isolated-browser attempt, the harness blurred the result
heading and assumed that the next `Tab` restarted document-order focus. It
sent `Enter` before verifying the active element. The official synthetic
citation link had received focus and was activated, unintentionally navigating
to
`https://www.postgresql.org/files/documentation/pdf/18/postgresql-18-A4.pdf`.
This was an unauthorised external-source access and violated the offline
boundary. The task stopped immediately; Chrome and preview were terminated,
ports 4173 and 9230 were cleared, no documentation or commit was produced at
that point, and the owner was informed. No credential, account, secret, real
product corpus or real user question participated; the question and result
were synthetic.

After the owner confirmed that headless validation could continue, the dirty
resume baseline was reconfirmed as the original commit plus only the two
expected correction files. The controlled retry used all of these safeguards:

- a single `LocalAuthorised` synthetic citation with a null `canonicalUrl`,
  leaving no external anchor in the rendered result;
- CDP request interception that continued only exact
  `http://127.0.0.1:4173/` requests and failed any other HTTP or HTTPS request
  before network dispatch;
- a new navigation for every matrix state, so the first `Tab` began from a
  fresh document; and
- an explicit active-element guard before every `Enter`, with immediate abort
  on any target other than the skip link.

The controlled retry used Chrome `151.0.7922.75` and reported zero extension
targets, zero blocked external attempts, zero observed external URLs and zero
runtime exceptions. Network observations were limited to the loopback HTML,
JavaScript, CSS and the pre-existing absent `/favicon.ico`; the latter returned
404 and was the only console issue.

All eight `interfaceLanguage` × `questionLanguage` × theme combinations passed
at `innerWidth` 320 CSS pixels. The document `clientWidth` and `scrollWidth`
were both 305 pixels. Every affected surface computed
`overflow-wrap: anywhere`, preserved the exact complete token and wrapped to
multiple visual lines with `scrollWidth` equal to `clientWidth`. The answer
measured 243/243 pixels, the title 213/213 and the excerpt 210/210. Light used
body colours `rgb(241, 239, 232)` / `rgb(23, 33, 29)` and Dark used
`rgb(16, 23, 20)` / `rgb(245, 242, 233)`.

In every combination, the first `Tab` focused the localised skip link with a
solid three-pixel outline, the guarded `Enter` focused
`MAIN#main-content` with `tabindex="-1"`, and the next `Tab` focused the
selected question-language radio. Direct full-page screenshots of the
Portuguese Light and Dark states showed readable vertical reflow, intact
tokens, visible completion focus and no clipping or horizontal panning.

### Cleanup, disposition and limitations

The final preview and isolated-Chrome processes were revalidated by PID,
executable, command line, profile, address and port before termination. Ports
4173 and 9230 finished without listeners. The execution policy refused the
recursive deletion of the task directories. Four sanitised directories remain
under the system temporary directory: preview logs, two temporary browser
profiles and the two screenshots. The first profile may retain cache from the
accidental public-PDF navigation; none has a running process, listener,
credential, secret, real product corpus or tracked project change.

`AQG-S05-008` is `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`. The correction,
contract-valid regression, complete npm suite and controlled browser matrix
passed, but this increment did not restart or approve the Automatic Quality
Gate and cannot dispose the finding. `AQG-S05-007` remains in the same status,
and `AQG-S05-001` through `AQG-S05-006` retain `RESOLVIDOS`.

JavaScript line and branch percentages remain unavailable in the existing
package scripts and dependency set. Node.js `24.18.1` was observed instead of
the exact `24.18.0` pin. No third-party accessibility engine or visible-window
browser pass was used. The unauthorised external navigation and retained
temporary profile are explicit execution risks; no new product P0/P1 or
material product finding was observed in the controlled retry.

`STATE-05 FRONTEND_IMPLEMENTATION` remains active. A complete restart of its
Automatic Quality Gate requires new explicit and separate owner authority.
Human Gate and `STATE-06` remain not authorised and not executed.

## Automatic Quality Gate restart after S05-CORR-07 — 2026-08-05

### Authority and baseline

- Gate baseline: location `C:\Projects\RAG-Challenge`, Git top-level
  `C:/Projects/RAG-Challenge`, Git directory `.git`, branch `main`, commit
  `97ea076da84d7afdb3330aa05dcb39fc7b44ce0f`, corpus `4.9.2` and clean
  working tree. All seven conditions were reconfirmed immediately before the
  audit, and branch, commit and cleanliness were reconfirmed before recording
  the result.
- Authority: restart the complete `STATE-05` Automatic Quality Gate locally,
  offline and sequentially, using only the existing installation, synthetic
  fixtures, fake fetch and task-owned loopback listeners. Browser validation
  required a temporary headless Chrome without Dark Reader or other
  page-modifying extensions, a fixture without external interactive links,
  deny-by-default non-loopback interception and an active-element guard before
  every `Enter`.
- Product correction, frontend/code/test changes, dependencies, installation,
  contracts, OpenAPI, ADRs, backend, `dotnet`, external network, provider,
  account, secret, real corpus, real official source, remote action, Human Gate
  and later lifecycle states remained outside authority.

### Result and finding disposition

Result: `APROVADO`. No new P0, P1, P2 or P3 finding was observed. The complete
restart disposed all eight recorded findings as `RESOLVIDOS`:

- `AQG-S05-001`: the decoder and presentation retain the HTTPS-only official
  citation boundary and null URL for `LocalAuthorised`; hostile local URL and
  valid official/local regressions passed.
- `AQG-S05-002`: declared and streamed response limits are enforced before
  full materialisation, overflow cancels the reader and cancellation remains
  observable; boundary and fake-stream regressions passed.
- `AQG-S05-003`: root language and document title follow only
  `interfaceLanguage` in both interface languages and all matrix states.
- `AQG-S05-004`: the closed freshness set and trust/freshness relationships
  remain aligned with the backend contract, including localised `Local`.
- `AQG-S05-005`: completed responses remain bound to the
  `questionLanguage` sent; both valid languages and both incompatible
  directions passed.
- `AQG-S05-006`: in every browser case, the first `Tab` focused the skip link
  with a solid three-pixel outline, the guarded `Enter` focused
  `MAIN#main-content`, and the next `Tab` focused the selected
  question-language radio.
- `AQG-S05-007`: all eight narrow combinations reflowed at 320 CSS pixels with
  document `clientWidth` and `scrollWidth` equal to 305 pixels.
- `AQG-S05-008`: distinct 517- to 519-character continuous values remained
  intact and multiline in answer, citation title and excerpt. At the narrow
  viewport the three surfaces measured 243/243, 213/213 and 210/210 pixels for
  client/scroll width, with `overflow-wrap: anywhere`.

### Static, npm and reproducibility evidence

Authority, lifecycle, scope, contracts and security were inspected again from
the beginning. The `STATE-05` history changed only the authorised Dashboard
and factual evidence surfaces. Package manifests, lockfiles, OpenAPI, ADRs,
backend and other protected technical paths had no diff from the state-entry
baseline. The OpenAPI SHA-256 remained
`D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34`.
The production source contained no raw HTML, script-evaluation or equivalent
rendering sink, and the same-origin request/CSP boundaries remained present.

All commands ran from `src/RagChallenge.Dashboard.Web` with the existing
installation and `npm_config_offline=true`:

| Verification | Result |
| --- | --- |
| `npm run lint` | Passed, exit code 0 |
| `npm run typecheck` | Passed, exit code 0 |
| `npm test` | Passed, 38 tests, 0 failed/skipped/cancelled |
| `npm run build` | Passed, 20 modules transformed |
| Repeated build | Byte-for-byte identical SHA-256 manifest for all three files |
| Built HTML | 977 bytes; `53B4C11EED457043B6FDEFC6437A1F7539DEEE28E02BD7D9A718065FDD885BF1` |
| Built CSS | 12,053 bytes; `5AF87FA05E947BC21DEFB8A4BCE69A202E7BC77D5A85AC85574578B723633B9A` |
| Built JavaScript | 171,973 bytes; `1D1E4981B8B34FB585260DACCB32646C0594A3C0F4CF080DA35A73625CC7DCCA` |

The observed toolchain was Node.js `24.18.1` and npm `11.16.0`; the repository
continues to pin Node.js `24.18.0` and npm `11.16.0`.

### Browser, accessibility and network evidence

The runtime preflight found no product-owned process or listener on ports
4173, 5173 or 9230. The built application was then served only on
`127.0.0.1:4173`, and Chrome `151.0.7922.75` exposed its temporary CDP surface
only on `127.0.0.1:9230`. The fresh task profile started with extensions,
component-extension backgrounds and background networking disabled; target
inventory reported zero extension target.

The browser executed 16 fresh navigations: the complete eight-combination
matrix at 1280 CSS pixels and again at 320 CSS pixels. Every case preserved
the selected interface language, question language and theme; the localised
title, root language, one header/main/footer/H1, labelled controls, unique IDs,
pressed states, answer language and original citation language were coherent.
No initial or completed state produced horizontal overflow. Runtime contrast
checks observed ratios of 14.35–16.25 for body text, 10.48–11.34 for selected
preference controls and 12.93–16.51 for the textarea, in addition to the
existing token-level contrast regression.

The fake fetch returned one contract-valid `LocalAuthorised` citation with a
null URL, so the fixture created no external interactive link. CDP request
interception continued only exact-origin `http://127.0.0.1:4173/` traffic and
failed every other HTTP/HTTPS destination before dispatch. The complete run
reported zero blocked external attempt, zero observed external URL, zero
runtime exception and zero significant console entry. Network observations
were limited to loopback HTML, JavaScript, CSS and the pre-existing absent
`/favicon.ico`, whose 404 remained non-material.

Direct full-page screenshots for `pt-BR` and `en-GB` in both `Light` and
`Dark` at 320 CSS pixels showed readable vertical reflow, visible completion
focus, intact long tokens, theme hierarchy and no clipping or horizontal
panning. The browser used no visible window and no owner-profile extension.

An initial temporary harness invocation failed before CDP connection because
the local script was interpreted as CommonJS with top-level `await`. It made
no page navigation or page request. Wrapping that temporary harness in an
asynchronous entry point resolved the tooling issue; no product or test file
was changed.

### Cleanup, limitations and lifecycle consequence

The exact preview and Chrome processes were identified by their task-owned
PIDs, command/profile and ports before termination. Ports 4173, 5173 and 9230
finished without listeners. One sanitised task directory remains under the
system temporary directory with preview/browser diagnostics, an isolated
loopback-only profile, the temporary harness and four screenshots. It has no
running process, listener, credential, secret, real corpus or tracked project
change.

JavaScript line and branch percentages remain unavailable because the existing
scripts and dependency set contain no coverage instrumentation; no Dashboard
percentage or repository floor is claimed. Exact-Node reproducibility remains
unproved despite byte-identical builds on Node.js `24.18.1`. No third-party
accessibility engine, visible-window browser pass, real backend, provider,
account, secret, corpus, official source or external network was exercised.

`STATE-05 FRONTEND_IMPLEMENTATION` remains active with its Automatic Quality
Gate approved and `AQG-S05-001` through `AQG-S05-008` resolved. This automatic
result does not execute or approve the Human Gate, enter `STATE-06` or grant
authority for any external action. Only this report and the two factual state
records are changed by the gate record.

## S05-CORR-08 — Human Gate visual observation correction — 2026-08-05

### Authority and baseline

- The authorised correction baseline was reconfirmed as location
  `C:\Projects\RAG-Challenge`, Git top-level `C:/Projects/RAG-Challenge`, Git
  directory `.git`, branch `main`, commit
  `3bf97915d623f2e4c5c3d86da52e724ad906ea35`, corpus `4.9.2` and a clean
  working tree. No project-owned runtime or listener was active on ports 4173,
  5173 or 9230 before the executable work.
- Authority was limited to the visual observation raised during the pending
  Human Gate review: simplify the hero to its localised introductory sentence
  as the only visible content, preserve the specified interaction and matrix
  behaviours, add focal regressions, run the four existing npm checks and
  validate in a task-owned loopback browser without modifying extensions.
- Dependencies, installation, manifests, lockfiles, contracts, OpenAPI, ADRs,
  backend, `dotnet`, external network, real services or data, remote actions,
  Automatic Quality Gate execution, Human Gate recording and `STATE-06`
  remained outside authority.

### Correction

Commit `b65d3b45a0ad32f0f7db1e97ccf415bdef5bb113` removes the promotional
eyebrow, the previous display title and the decorative orbit from the hero.
The exact Portuguese introduction is now the hero's single visible child and
its only H1. The existing English equivalent receives the same semantics.
The H1 uses proportional `clamp(1.35rem, 2.8vw, 2rem)` typography, a readable
1.35 line height and a reducible 48-character measure. The removed title copy
and decorative CSS are no longer retained. The localised workspace label was
deliberately preserved because it continues to name workspace regions for
assistive technology, although it is no longer visible in the hero.

The focal regressions verify both localised headings in Light and Dark, exactly
one H1 throughout the eight interface-language × question-language × theme
combinations, absence of the removed titles and orbit, and the proportional
hero typography without compact-viewport overrides. The existing long-token,
escaping, focus, language and matrix assertions remain active.

### Verification evidence

The first sequential check run passed lint but exposed that removing
`workspaceLabel` from the copy model would break typechecking because the same
localised value supplies accessible region labels. The field was restored for
that existing non-visible responsibility, and all four checks were restarted
from the beginning. The final offline run from
`src/RagChallenge.Dashboard.Web`, at `2026-08-06T00:05:03.8860069Z`, used
Node.js `24.18.1`, npm `11.16.0` and the existing installation:

| Verification | Result |
| --- | --- |
| `npm run lint` | Passed, exit code 0 |
| `npm run typecheck` | Passed, exit code 0 |
| `npm test` | Passed, 38 tests, 0 failed/skipped/cancelled |
| `npm run build` | Passed, 20 modules transformed |
| Built HTML | 977 bytes; `6EFBCFC22A91304666795DBFC84F217BBB1652B474298C3B39C3F7E6F46140DC` |
| Built CSS | 10,944 bytes; `C41332E3477D73EC4FE98E5DE40AF6718B644CAAC2DFDDD0015349FFD860442F` |
| Built JavaScript | 171,416 bytes; `83E54FA38EEC968F34CFD97C4EB5871C47002CA099D4E0A31BB3C5798D78C1B5` |

The isolated browser validation used Chrome `151.0.7922.75`, a fresh headless
profile, disabled extensions and background networking, zero extension targets
and loopback-only CDP. The synthetic fixture contained one `LocalAuthorised`
citation with no URL. Request interception continued only the exact
`http://127.0.0.1:4173` origin, and every `Enter` required the expected active
element first.

All eight combinations passed at 1280 CSS pixels and again at 320 CSS pixels.
Each of the 16 navigations had exactly one H1 containing the applicable new
hero sentence, without the removed title, promotional label or decorative
orbit. At the narrow viewport, document client and scroll widths were both
305 pixels in every case. The long answer, citation title and excerpt remained
complete and measured 243/243, 213/213 and 210/210 pixels respectively.

In every case, the first `Tab` visibly focused the localised skip link with a
solid outline of at least three pixels, guarded `Enter` focused
`MAIN#main-content`, and the next `Tab` focused the selected question-language
radio. Runtime contrast ratios remained at least 14.35 for body text, 10.48
for the selected preference control and 12.93 for the textarea. Direct narrow
screenshots for `pt-BR` and `en-GB` in Light and Dark showed proportional hero
typography, readable reflow and no clipping or horizontal panning. The profile
did not contain Dark Reader or another page-modifying extension. The run
reported zero blocked external attempt, zero observed external URL, zero
runtime exception and zero significant console entry; only the known loopback
`/favicon.ico` 404 remained non-material.

### Cleanup, scope and lifecycle consequence

The preview and Chrome processes were revalidated by PID, executable, command,
profile and port before termination. Ports 4173, 5173 and 9230 finished free,
with no remaining process tied to the task profile. Execution policy refused
recursive removal of the sanitised task directory under the system temporary
directory; it retains only loopback diagnostics, the isolated profile and four
screenshots, with no listener, credential, secret, real corpus or tracked
project change.

The final product/test diff from the authorised baseline contains only the
five Dashboard source and focal-test files named by this correction. Package
and lock files, contracts, OpenAPI, ADRs, backend and other protected technical
surfaces remain unchanged.

JavaScript line and branch percentages remain unavailable in the existing
scripts and dependencies. The observed Node.js patch remains `24.18.1` rather
than the exact `24.18.0` pin. No third-party accessibility engine,
visible-window browser, real backend, provider, account, corpus, official
source or external network was exercised.

`STATE-05 FRONTEND_IMPLEMENTATION` remains active. The approved Automatic
Quality Gate result on the pre-correction baseline remains historical evidence
but does not cover commit `b65d3b45a0ad32f0f7db1e97ccf415bdef5bb113`.
This increment neither restarted that gate nor recorded the Human Gate. A
complete Automatic Quality Gate restart over the new clean baseline requires
new explicit owner authority before the Human Gate review can resume;
`STATE-06` remains not authorised.
