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
