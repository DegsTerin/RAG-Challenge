# STATE-06 Integration Report

## Purpose and authority

This report records the authorised local, offline and sequential `S06-A` work
performed on 2026-08-05 after the approved closure of `STATE-05
FRONTEND_IMPLEMENTATION`. It is implementation evidence for `STATE-06
INTEGRATION`; it is not an Automatic Quality Gate, Human Gate, deployment,
publication or authority for `STATE-07`.

The owner authorised only:

- recording entry into `STATE-06` before executable work;
- integrating a synthetic document → index → question → answer flow across the
  backend and Dashboard;
- synchronising an official-source fixture only through a fake HTTP server on
  loopback;
- validating restart and durable raw content, catalogue, activation and index;
- adding non-secret environment configuration;
- producing and reproducing a local artefact from a clean tracked baseline;
- running applicable existing .NET and npm checks; and
- recording the evidence in focused local commits.

Dependencies, installation, package manifests, lockfiles, public contracts,
OpenAPI, ADRs, external network access, real providers, accounts, secrets,
real corpus content, real official sources, GitHub, real OCI, publication,
deployment, DB-Notifier, the Automatic Quality Gate, the Human Gate and
`STATE-07` remained outside authority.

## Entry and baselines

Before the state entry, the following facts were reconfirmed:

| Fact | Observed value |
| --- | --- |
| Location | `C:\Projects\RAG-Challenge` |
| Git top-level | `C:/Projects/RAG-Challenge` |
| Git directory | `.git` |
| Branch | `main` |
| Authorised HEAD | `8fb3b93532a569af953cdf24e190b82998020464` |
| Prompt corpus | `4.9.2` |
| Working tree | clean |

Entry into `STATE-06` was recorded before executable work in commit
`ad218b58210e41d0c3a2c76ef81b5886498fd01a` (`docs(state): enter state 06
integration`). The executable implementation and its tests were isolated in
commit `8041e25a554a7cc47ecebf4abe1fc8b94b12d12d` (`feat(integration): add
synthetic e2e artefact`).

The accepted clean-baseline artefact reproduction ran on
`main@8041e25a554a7cc47ecebf4abe1fc8b94b12d12d`, corpus `4.9.2`, with no
tracked or untracked Git change. Ignored outputs and the already installed
toolchains remained present as expressly authorised. No fresh clone and no
dependency restore or installation were used.

## Runtime preflight

The directed preflight applied because executable behaviour and listeners
would be exercised. It inspected only command lines under the repository and
the task ports 4173, 5086, 5096, 5173 and 9230. No RAG-Challenge application
process or owned listener required termination. One Codex computer-use Node
process referenced the workspace but was not a product process and was not
stopped.

Every later task listener used `127.0.0.1`, was tied to the synthetic host or
fake official server, and was stopped after use. At hand-off, ports 5086 and
5096 were explicitly observed free; the artefact reproduction runtime had
been removed.

## Implemented integration boundary

### Explicit Integration environment

`SetupHost` now composes the synthetic runtime only when both conditions are
true:

1. the host environment is exactly `Integration`; and
2. `RagChallenge:Integration:Enabled` is explicitly `true`.

Enabling the profile in another environment fails closed. The store root must
be an explicit absolute path. `appsettings.Integration.json` contains only
non-secret defaults, keeps the profile disabled, keeps external services and
administration disabled, and leaves the store root empty. The normal host
continues to use its disabled query/readiness services.

When explicitly enabled, the profile serves the built Dashboard through the
same origin as API v1 and maps the existing SPA fallback. No public route,
request/response contract or OpenAPI artefact changed.

### Synthetic durable flow

The integration profile uses the existing application and infrastructure
services rather than a substitute in-memory workflow:

1. a bounded synthetic CSV document is parsed and chunked;
2. its raw bytes are written through `ImmutableContentStore`;
3. an active catalogue revision is committed through the existing SQLite
   control plane;
4. deterministic three-dimensional embeddings are built into a candidate in
   the existing SQLite exact vector store;
5. the finalised generation is committed and activated through the existing
   compare-and-swap activation service;
6. the existing query activation reader, vector search and
   `QuestionAnsweringService` retrieve the evidence; and
7. a deterministic local language-model adapter returns a grounded answer in
   the requested `pt-BR` or `en-GB` language with the existing citation and
   coverage contract.

The deterministic adapters are private to the explicit integration profile.
They do not introduce provider packages, dynamic loading or network access.
The configured corpus remains `database-systems-catalogue-mvp`; the fixture is
synthetic and is not product corpus material.

On every new process, initialisation reopens and verifies the current
catalogue, activation, every bound raw-content object and a sentinel search in
the active vector generation. Bootstrap occurs only when no active activation
exists, so restart reuses the durable generation instead of rebuilding it.

### Fake official-source synchronisation

The integration test creates an ephemeral ASP.NET HTTP listener at
`http://127.0.0.1:<dynamic-port>/synthetic.csv`. Its `HttpClient` disables
proxy use and redirects. The test-only transport rejects any non-loopback or
non-HTTP target, records the one requested URI and enforces the configured
body bound.

The registration retains canonical HTTPS metadata
`https://official.invalid/synthetic.csv`, but that value is never passed to an
HTTP client. The only observed request URI is the fake loopback endpoint. The
existing `OfficialSourceSynchronisationService` parses the response, writes
an immutable content object, commits an official snapshot and appends its
observation while preserving `OfficialExternal` context. No product transport
for real official egress was added.

## End-to-end and restart evidence

`IntegrationHostEndToEndTests` opens an actual Kestrel listener on an
OS-assigned loopback port. The first process:

- serves `index.html` from the API origin;
- accepts the existing `/api/v1/questions` request in `en-GB` and `pt-BR`;
- returns `Answered`, the matching answer language and one citation; and
- persists `control.db`, `vectors.db` and at least one immutable content
  object.

After a complete host stop and disposal, a second host opens the same store.
It reports `Ready`, answers again and returns the exact generation identity
created by the first process. The accepted artefact reproduction observed:

```text
idxgen-795825d3ad7afad1acd3a16ef48f2448270dda36ea71725fe6f6231956ced2c5
```

The published Dashboard was also exercised through a controlled Chrome tab
against `http://127.0.0.1:5096/`. A `pt-BR` question submitted through the
visible form produced “Resposta fundamentada”, coverage of one active and one
eligible database/document, and the local authorised CSV citation. The page
reported no console warning or error. The tab, exact task PID, listener and
temporary browser store were then closed or removed.

## Reproducible local artefact

The build entry point is:

```powershell
& 'src/RagChallenge.Server.Api/Build-IntegrationArtifact.ps1'
```

It uses only the existing installation, sets npm offline mode, runs the
Dashboard build, publishes the server in Release with `--no-restore`, copies
the Dashboard into `wwwroot`, writes a sorted SHA-256/size manifest and creates
a ZIP with sorted entries, no compression and a fixed UTC timestamp. The
default output is ignored local data under `artifacts-local/s06-a/`.

The reproduction entry point is:

```powershell
& 'src/RagChallenge.Server.Api/Test-IntegrationArtifact.ps1'
```

It validates output containment and port ownership, extracts the ZIP, starts
the published host on `127.0.0.1:5086`, exercises same-origin Dashboard/API
queries in both answer languages, stops the process, verifies all three store
classes, restarts against the same store, compares generation identity, then
stops the exact process and removes only its task-owned runtime directory.

Two consecutive clean-baseline builds produced identical results:

| Property | Accepted value |
| --- | --- |
| Archive | `artifacts-local/s06-a/rag-challenge-s06-a.zip` |
| SHA-256 | `b2b6f50352c29a89f91640870564df263a2a5888f2009a94dc9a0ec1bb33b3c4` |
| ZIP bytes | 47,234,158 |
| Published files | 58, including the manifest |
| Manifest entries | 57 |
| Reproduction | passed |
| Restart | same active generation |

The archive is deliberately ignored and remains local. Its hash changes when
the committed source revision changes because the .NET build records source
revision metadata; repeatability is asserted for the same clean commit.

## Verification record

All accepted commands ran locally on 2026-08-05. Unless another directory is
shown, the working directory was `C:\Projects\RAG-Challenge`.

| Command or check | Result |
| --- | --- |
| Baseline: `Get-Location`; `git rev-parse --show-toplevel --git-dir HEAD`; branch, status and corpus inspection | Passed before state entry and again before the accepted artefact reproduction. |
| Directed process/listener preflight | Passed; no product runtime needed termination. |
| `dotnet format RAG-Challenge.sln --verify-no-changes --no-restore --verbosity diagnostic` | Exit 0; 0 of 118 files required formatting. |
| `dotnet build RAG-Challenge.sln --configuration Release --no-restore` | Exit 0; 0 warnings and 0 errors. |
| `dotnet test RAG-Challenge.sln --configuration Release --no-build` | Exit 0; 74 unit, 10 architecture and 90 integration tests passed; 174 total, 0 failed/skipped. |
| Focused listener/restart and fake-official tests | Exit 0; 2 passed. |
| `.NET XPlat Code Coverage` collection over the Release solution | Exit 0; all 174 tests passed. The integration collector measured 90.72% lines (17,465/19,250) and 62.35% branches (2,314/3,711), above the repository's 70%/45% floors. |
| `npm run lint` in `src/RagChallenge.Dashboard.Web` | Exit 0. |
| `npm run typecheck` in `src/RagChallenge.Dashboard.Web` | Exit 0. |
| `npm test` in `src/RagChallenge.Dashboard.Web` | Exit 0; 38 passed, 0 failed/skipped/cancelled. |
| `npm run build` in `src/RagChallenge.Dashboard.Web` | Exit 0; Vite transformed 20 modules. |
| Two consecutive `Build-IntegrationArtifact.ps1` runs on clean `main@8041e25...` | Exit 0 twice; identical ZIP SHA-256, file count and size. |
| `Test-IntegrationArtifact.ps1` against the second archive | Exit 0; both readiness checks, both languages, stores and restart passed. |
| Controlled Chrome submission through the published Dashboard | Passed; visible completion/citation and no console issue. |
| `git diff --check`, apparent-secret scan, UTF-8/LF/final-newline check and protected-file status | Passed; package manifests, lockfiles, OpenAPI, contracts and ADRs unchanged. |
| OpenAPI SHA-256 | Still `D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34`. |
| Final runtime hygiene | Ports 5086 and 5096 free; no artefact runtime directory; tracked baseline clean before this report edit. |

One exploratory npm command was invalid: `npm test -- --run` passed `--run`
without the argument required by Node 24 and exited 9 before executing tests.
It did not change product state. The corrected repository-owned `npm test`
command subsequently passed all 38 tests.

During implementation, the initial listener test exposed that the dependency
injection container could not construct an internal integration runtime by
type. Registration was corrected to use an explicit factory, after which the
direct listener and complete suite passed. The first two exploratory artefact
reproduction attempts timed out while the harness used unsuitable child
process output handling and provided no useful process diagnostic. The
harness now uses bounded redirected output, omits unobserved asynchronous
readers and includes a bounded failure diagnostic. The accepted reproduction
runs passed and cleaned up. These exploratory failures are not accepted test
evidence and are retained here for factual completeness.

## Toolchain and environment

| Component | Observed version |
| --- | --- |
| Windows | `Microsoft Windows NT 10.0.26200.0` x64 |
| PowerShell | `7.6.4` |
| Git | `2.55.0.windows.3` |
| .NET SDK | `10.0.302` |
| ASP.NET Core / .NET runtime | `10.0.10` |
| Node.js | `24.18.1` |
| npm | `11.16.0` |

## Limitations and residual risks

- The accepted reproduction proves a clean tracked baseline with the existing
  local installation. It does not prove a fresh clone or machine without
  restored NuGet/npm assets, because installation and restore were explicitly
  outside authority.
- At the accepted `S06-A` reproduction, Node.js `24.18.1` differed from the
  then-current repository engine pin `24.18.0`; npm matched `11.16.0`. The
  subsequent bounded toolchain-policy correction recorded below supersedes
  that exact-pin policy without changing this historical observation. The
  existing frontend dependency set provides no percentage coverage
  instrumentation, so no JavaScript line or branch percentage is claimed.
- Deterministic embeddings and language-model output prove composition,
  persistence and contracts only. They do not validate provider quality,
  latency, cost, entitlement, quota or behaviour.
- The official sync proves the application workflow through fake loopback
  HTTP only. It does not validate DNS, TLS, SSRF controls, allowlists,
  redirects or availability for a real official source.
- Windows x64 was exercised. Linux ARM64, OCI runtime, managed storage,
  operational backup/restore, capacity, crash at every persistence boundary,
  load and performance were not tested.
- Browser evidence covered one visible `pt-BR` submitted query against the
  real published composition. The existing 38 frontend tests retain the wider
  language/theme/accessibility matrix; that full matrix was not repeated
  against the backend in this lot.
- No host-wide packet capture was performed. The task configured only
  loopback URLs; the fake official transport disabled proxies and rejected a
  non-loopback target; no external request was observed by the task.
- The local ZIP is not signed, published or a release. It contains no secret
  or real corpus and must not be treated as production deployment evidence.

No P0 or P1 issue was found. These limitations remain inputs to later work;
they are not silently treated as completed validation.

## Negative scope and security result

No dependency was added or installed. `package.json`, `package-lock.json`,
all .NET package declarations and lockfiles, public contracts, OpenAPI and
ADRs are unchanged. No external source, provider, account, secret, real corpus,
GitHub resource, OCI resource, publication, deployment or DB-Notifier action
was used. No Automatic Quality Gate or Human Gate was started, and no entry
into `STATE-07` was recorded.

Configuration contains only typed non-secret values. Runtime stores and logs
are ignored local data. The fake official content and local corpus are small,
bounded synthetic fixtures. Retrieved text remains data and does not carry
instruction authority.

## Rollback

Rollback was not required or executed. If separately authorised, the tracked
change can be reversed through ordinary focused reverts, in reverse order:

1. the final documentation commit that records this report;
2. `8041e25a554a7cc47ecebf4abe1fc8b94b12d12d`; and
3. `ad218b58210e41d0c3a2c76ef81b5886498fd01a` only if the authorised
   `STATE-06` entry itself must also be reverted in current state while the
   append-only transition history remains preserved by a compensating entry.

The ignored archive can be regenerated from the implementation commit or,
with separate destructive authority, removed only at the explicit validated
path `artifacts-local/s06-a/`. No migration was added and no external state
requires rollback.

## Outcome

`S06-A` is complete on the local synthetic boundary described above. The
document-to-answer path, fake official synchronisation, same-origin
Dashboard/API composition, restart durability, reproducible artefact and
clean-baseline reproduction passed. `STATE-06 INTEGRATION` remains active.
The Automatic Quality Gate, Human Gate, `STATE-07` and every external action
remain unexecuted and unauthorised.

## Bounded toolchain-policy correction

After `S06-A`, the owner reported and locally applied the normal workstation
update to Node.js `24.19.0` and npm `11.17.0`. Because workstation LTS updates
are expected to continue, the owner authorised a focal correction that accepts
compatible updates within Node 24 and npm 11 instead of treating one patch as
the only valid development runtime.

Commit `a7d50d8e72d5f5600ae41e3fdd313f4f1e502188` (`fix(frontend): allow
compatible toolchain updates`) makes only these executable changes:

- `engines.node` is `>=24.18.0 <25`;
- `engines.npm` is `>=11.16.0 <12`;
- `devEngines.runtime` and `devEngines.packageManager` enforce the same
  ranges with `onFail: "error"` before npm install, CI and run commands;
- the unenforced exact `packageManager: "npm@11.16.0"` field is removed;
- the package-lock root engine metadata matches the manifest; and
- the repository-owned boundary test requires the complete bounded policy and
  rejects reintroduction of an exact `packageManager` field.

The existing `.nvmrc` remains `24.18.0` as an optional reproducible selector
at the supported lower boundary; it does not narrow the manifest's supported
range. No dependency version or integrity record changed.

The correction was executed locally and offline with Node.js `24.19.0`, npm
`11.17.0` and `engine-strict=false`. `devEngines` accepted both versions.
`npm run lint`, `npm run typecheck`, all 38 npm tests and `npm run build`
passed. Two artefact builds on clean
`main@a7d50d8e72d5f5600ae41e3fdd313f4f1e502188` produced the identical
58-file, 47,234,166-byte ZIP with SHA-256
`65b405c690a1c66c374296745613217717d7fd38f04cbefb15994323da1ffc98`.
The second archive passed the complete loopback reproduction, both answer
languages and restart with the same active generation.

No install, restore, dependency, source contract, OpenAPI, ADR, lifecycle,
external network, provider, account, secret, real corpus, official source,
GitHub, OCI, publication, deployment or DB-Notifier action occurred. The
Automatic Quality Gate and Human Gate were not started. Future Node 25 or npm
12 adoption remains a deliberate compatibility decision outside this bounded
policy.

## Automatic Quality Gate — 2026-08-06

### Authority, baseline and scope

The owner authorised a complete local, offline and sequential Automatic
Quality Gate over `main@a6f0480b7f229b63c5ac24d65e61f55de1c6483a`,
prompt corpus `4.9.2` and a clean working tree. Before executable work, the
audit reconfirmed:

| Fact | Observed value |
| --- | --- |
| Location | `C:\Projects\RAG-Challenge` |
| Git top-level | `C:/Projects/RAG-Challenge` |
| Git directory | `.git` |
| Branch | `main` |
| HEAD | `a6f0480b7f229b63c5ac24d65e61f55de1c6483a` |
| Prompt corpus | `4.9.2` |
| Working tree | clean |

The audit used only the existing installation, synthetic fixtures, temporary
stores and task-owned loopback listeners. It did not restore, install or
update a dependency; access an external network, provider, account, secret,
real product corpus or real official source; mutate GitHub or OCI; publish,
deploy, run DB-Notifier, execute a Human Gate or enter `STATE-07`.

The directed runtime preflight found no RAG-Challenge process and no listener
on ports 4173, 5086, 5096, 5173 or 9230. One unrelated `node.exe` process
referenced the workspace but was neither a product process nor rooted in the
repository and was not stopped.

### Deliverable and acceptance reconciliation

| STATE-06 requirement | Result | Evidence and boundary |
| --- | --- | --- |
| Local/sandbox E2E | `APROVADO` | The real loopback host served the same-origin Dashboard/API flow in both answer languages and preserved its generation across restart. |
| Fake official-source server; real smoke only when authorised | `APROVADO` for fake loopback; `NÃO APLICÁVEL` for real smoke | The focused test made one bounded request to an ephemeral `127.0.0.1` HTTP server with proxy and redirects disabled. External smoke was explicitly prohibited. |
| Environment configuration | `APROVADO` | The `Integration` profile requires an explicit environment, enable flag and absolute store root; committed and packaged defaults disable integration, administration and external services and contain no secret. |
| Resilience and cancellation | `PARCIAL` | Existing tests cover provider failures, activation/persistence faults, idempotency, concurrency, frontend stream cancellation and the canonical cancellation error. The STATE-06 E2E tests do not cancel an in-flight host request or inject a sync/provider/store failure into the composed integration runtime and then prove that the active generation remains serviceable. |
| Reproducible artefact | `APROVADO` | Two builds produced the same 58-file, 47,234,166-byte ZIP with SHA-256 `7b934d3fc8a099683c6599c3663c82d04de19ccdbf89fdeca885895821ade17f`. All 57 payload entries matched the embedded manifest and the declared archive digest matched the ZIP. |
| OCI plan and non-production rehearsal | `REPROVADO` | ADR-0005 contains a conditional deployment direction, but STATE-06 has no state-owned rehearsal/runbook or Linux ARM64 candidate evidence. The current assets contain the SQLite Linux ARM64 native library but no restored `net10.0/linux-arm64` publish target; a rehearsal would require a restore or additional dependency authority and was not attempted. |
| Real README examples | `REPROVADO` with a normative divergence | Lifecycle assigns real README examples to STATE-06. The roadmap assigns the final README and real examples to `S08-B`/`BL-M13`. Lifecycle has higher precedence, the divergence remains unresolved, and the current README has no verified STATE-06 example. It also still describes STATE-03 as active and states that no functional RAG product or persistent store exists. |
| Complete flow is reproducible | `APROVADO` | Document → immutable content → catalogue → candidate index → activation → question → grounded answer passed in the solution suite, focused E2E and packaged artefact. |
| Restart and persistence are known | `APROVADO` | Control, vector and raw-content stores reopened after a complete process restart with the same active generation. |
| External errors do not corrupt the active index | `PARCIAL` | Lower-level integration tests cover failed persistence, compare-and-swap and provider outcomes, but the composed STATE-06 host lacks a deterministic fault/cancellation E2E that proves continued service from the active generation. |
| Query performs no source fetch and preserves active provenance/generation | `APROVADO` within the synthetic boundary | The query composition has no official transport, resolves one active generation and returned the local-authorised CSV citation. Official synchronisation remained a separate administrative loopback test. |
| No secret in artefact | `APROVADO` | Configuration inspection and an archive text-signature scan found no secret signature; external capabilities remain disabled. |
| Evidence is not represented as production | `APROVADO` | The artefact, synthetic providers, fake source and Windows x64 boundary remain explicitly identified as local evidence only. |

The Lifecycle/roadmap disagreement was not resolved by changing Lifecycle,
roadmap, ADRs or contracts. Under the current precedence, the Lifecycle
deliverable remains applicable to this gate.

### Commands, versions and observed results

The accepted commands ran from `C:\Projects\RAG-Challenge` unless another
directory is named.

| Command or check | Result |
| --- | --- |
| Location/Git/corpus/status precondition checks | Passed with the exact authorised baseline and zero status entries. |
| Directed process/listener preflight | Passed; no product process or task-port listener required termination. |
| `dotnet format RAG-Challenge.sln --verify-no-changes --no-restore --verbosity diagnostic` | Exit 0; 0 of 118 files required formatting. |
| `dotnet build RAG-Challenge.sln --configuration Release --no-restore` | Exit 0; 0 warnings and 0 errors. |
| `dotnet test RAG-Challenge.sln --configuration Release --no-build --no-restore --collect:"XPlat Code Coverage"` | Exit 0; 74 unit, 10 architecture and 90 integration tests passed, 174 total and none failed/skipped. |
| `eng/assert-coverage.ps1` over the gate results | Passed; merged coverage 92.38% lines (17,783/19,250) and 66.59% branches (2,466/3,703). |
| `npm run lint`, `npm run typecheck`, `npm test`, `npm run build` in the Dashboard | Exit 0 for all; 38 tests passed and Vite transformed 20 modules. npm was forced to offline mode. |
| Focused `IntegrationHostEndToEndTests` and `OfficialSourceLoopbackTests` | Exit 0; 2/2 passed with ephemeral loopback listeners, restart and cleanup. |
| Two `Build-IntegrationArtifact.ps1` runs | Exit 0; identical hash, byte count and file count on the clean tracked baseline. |
| `Test-IntegrationArtifact.ps1 -Port 5086` | Exit 0; both languages, same-origin Dashboard/API, all three store classes and restart passed. |
| Independent manifest/archive integrity and non-secret configuration check | Passed; 57/57 payload entries matched, archive digest matched, 0 secret signatures, integration and external services disabled. |
| `eng/check-repository.ps1` and `git diff --check` | Passed for 194 non-ignored files and diff hygiene. |

One audit wrapper attempt exited 1 after a successful artefact build because
it tried to parse npm prose plus the final JSON object as one JSON document.
It did not change tracked state. The corrected wrapper selected the final JSON
line and then completed two accepted identical builds. A proposed ad hoc
cancellation diagnostic was rejected by the local execution policy before a
process was launched; it is not test evidence.

Observed toolchain: Windows `10.0.26200` x64, PowerShell `7.6.4`, Git
`2.55.0.windows.3`, .NET SDK `10.0.302` with runtime `10.0.10`, Node.js
`24.19.0` and npm `11.17.0`. The audit began at
`2026-08-06T03:18:11.8368142Z`.

### Findings

#### AQG-S06-001 — P2 — OCI non-production rehearsal is absent

- Impact: the accepted conditional OCI direction has no STATE-06 proof that
  the candidate artefact, Linux ARM64 runtime, environment model, durable
  paths and startup/rollback procedure form a reproducible non-production
  deployment candidate.
- Reproduction: compare the STATE-06 Lifecycle deliverable with this report
  and repository files; no rehearsal/runbook exists. The current Server API
  assets have no `net10.0/linux-arm64` target, so the authorised no-restore
  installation cannot perform the candidate publish.
- Recommendation: under separate corrective authority, prepare the bounded
  state-owned OCI readiness plan and execute an offline/non-production Linux
  ARM64 rehearsal with already approved dependencies or separately approved
  restore inputs. Do not contact or create OCI resources for that rehearsal.

#### AQG-S06-002 — P2 — Integrated resilience and cancellation coverage is incomplete

- Impact: lower-level fault handling is strong, but the composed host has no
  deterministic evidence that an in-flight cancellation or injected
  sync/provider/store failure leaves the active generation serviceable and
  restartable.
- Reproduction: inspect `IntegrationHostEndToEndTests` and
  `OfficialSourceLoopbackTests`; they exercise only successful requests. The
  suite maps cancellation and covers faults in other boundaries, but does not
  perform that STATE-06 composition-level assertion.
- Recommendation: under separate corrective authority, add deterministic E2E
  cases for request cancellation/deadline and bounded integration failures,
  then assert no activation corruption, continued query service and restart
  consistency.

#### AQG-S06-003 — P2 — README deliverable is missing and its owning documents diverge

- Impact: gate ownership is ambiguous and the public entry point materially
  understates the implemented state. A reviewer following README receives an
  obsolete STATE-03 baseline and no verified STATE-06 example.
- Reproduction: Lifecycle requires `examples reais para o README` in
  STATE-06, while the roadmap assigns `README final com exemplos reais` to
  `S08-B`/`BL-M13`. README still states that STATE-03 is active and that
  migrations, stores, parsers, providers and functional RAG do not exist.
- Recommendation: obtain bounded corrective authority to reconcile the
  owner documents without weakening evidence requirements, then update README
  with the current factual state and examples that are explicitly labelled by
  their verified local/synthetic or future real-product boundary. Real corpus
  or provider claims still require their own authority and evidence.

No P0, P1 or P3 finding was identified. The three P2 findings remain open;
the audit did not correct code, configuration, tests, README, lifecycle,
roadmap, ADRs or contracts.

### Limitations, cleanup, rollback and gate outcome

- The clean-baseline reproduction used the existing restored local
  installation, not a fresh machine or dependency installation.
- Windows x64 was exercised. Linux ARM64, OCI runtime/IAM/storage,
  operational backup/restore, capacity, performance, real providers, real
  source TLS/SSRF and real corpus quality remain untested.
- JavaScript percentage coverage remains unavailable; the 38 frontend tests
  nevertheless passed.
- No host-wide packet capture was performed. Every task URL was loopback,
  npm was offline, .NET commands used `--no-restore`, and no external request
  was observed by the task.
- Task runtimes and temporary stores were removed. The ignored reproducible
  artefact remains under the validated `artifacts-local/s06-a/` path and can
  be regenerated from the audited commit.
- The execution policy refused recursive removal of the exact, validated
  gate coverage directory under `TestResults/`. It remains ignored and
  contains only generated Cobertura/test evidence; no process or listener
  uses it.
- Rollback was neither required nor executed. This evidence-only change can
  be reverted by one ordinary focused revert under separate authority while
  preserving this append-only factual history; no external state exists to
  roll back.

The Automatic Quality Gate is **REPROVADO**. The result is not `BLOQUEADO`
because the available evidence is sufficient to determine that required
STATE-06 deliverables are unmet. `STATE-06 INTEGRATION` remains active; the
Human Gate is premature and was neither requested nor executed. `STATE-07`
and every external action remain unauthorised.

## S06-CORR-01 dependency-gate execution

### Normative baseline

On 2026-08-06, the owner accepted `NORM-S06-001` and authorised the sequential
`AUTH-S06-NORM-001`, `AUTH-S06-DEP-001` and `AUTH-S06-CORR-001` envelopes on
clean `main@140c0516e4dbfc02808a90f0496550eb6b09da1b`, corpus `4.9.2`.
Lifecycle and the roadmap now keep the factually current, verified
local/synthetic README example in `STATE-06` and the separately verified
public OCI/real-product finalisation in `STATE-08`. The resulting corpus is
`4.9.3`.

### Runtime-pack evidence

The bounded HTTPS collection used only `api.nuget.org`, refused redirects and
stored bytes under the ignored `artifacts-local/s06-dependencies/` boundary.
One assumed optional `.nupkg.sha512` route returned `404` and was not retried;
the authoritative SHA-512 values came from each package catalogue entry and
matched the downloaded package bytes exactly.

| Package | Bytes | SHA-512 catalogue/package result | Supply-chain result |
| --- | ---: | --- | --- |
| `Microsoft.NETCore.App.Runtime.linux-arm64` `10.0.10` | 37,584,411 | `wvXLiOfFb1gKY6uBDbZ6xyxlmieVXJLvkmVjoxi7CHzo6LEqypq88xRYtCPX1pZvGBcOQIWHGxvQGvdaEn1oCw==` | Listed stable `DotnetPlatform`; MIT; zero declared dependencies; author and repository signatures valid with offline revocation; zero applicable advisory. |
| `Microsoft.AspNetCore.App.Runtime.linux-arm64` `10.0.10` | 12,387,032 | `aBtCKth6hLH5uSd0l5zhKXub77x/B9rQtBRR7li1s/j/4/5rBG2UusqHShGzxmcGEsvLIe2YrWHPdgVIm5kf+g==` | Listed stable `DotnetPlatform`; MIT; zero declared dependencies; author and repository signatures valid with offline revocation; zero applicable advisory. |
| `Microsoft.NETCore.App.Host.linux-arm64` `10.0.10` | 5,309,240 | `714TBU99meQpT9JXZuvrV3H9pDK0TwGhcuMvo38IOIjPuZSGExG6JcJToReQZrODB+5+ons0fil+xnrOpCDh3w==` | Listed stable `DotnetPlatform`; MIT; zero declared dependencies; author and repository signatures valid with offline revocation; zero applicable advisory. |

The NuGet vulnerability base dated 2026-08-04 contained historical ranges for
the three package families; exact `NuGet.Versioning` range evaluation found
that `10.0.10` satisfied none of them. The 2026-08-06 update was empty. The
three package identities therefore matched the authorised dependency matrix;
no additional runtime-pack identity or version was discovered.

### Locked-restore stop condition

The isolated cache was seeded read-only with the 42 existing locked production
package identities. The restore then used only the three-package verified
local source, `eng/NuGet.Offline.config`, `linux-arm64`, the isolated package
and HTTP caches, offline certificate revocation and locked mode. It contacted
no package source during restore and did not mutate the global cache.

The command exited `1` with `NU1004` for Domain, Application, Infrastructure
and Server: adding the runtime identifier makes each corresponding lockfile
inconsistent until that project records `linux-arm64`. The owner authorised a
change only to `src/RagChallenge.Server.Api/packages.lock.json` and explicitly
required a stop if another lockfile needed to change. Consequently:

- no lockfile or other tracked file changed;
- generated ignored `obj/project.assets.json` files contain non-authoritative
  failed-restore targets and are not accepted build evidence;
- no rehearsal build, executable test, README update or product correction
  was attempted; and
- the runtime preflight remained `NOT_APPLICABLE` because no executable
  behaviour was changed or validated.

`S06-CORR-01` is blocked at the dependency gate, not
`CORRECTED_PENDING_GATE_RETEST`. `AQG-S06-001` to `AQG-S06-003` remain open,
the Automatic Quality Gate remains **REPROVADO**, and the Human Gate remains
premature. Continuing requires new owner authority that permits the exact
Domain, Application, Infrastructure and Server lockfile set to record the
same verified `linux-arm64` restore target. No new Automatic Quality Gate is
authorised by that prerequisite.

## S06-CORR-01 corrective completion

### Authority continuation and baselines

The dependency-gate section above remains the historical record of the first
bounded stop. The owner subsequently expanded `AUTH-S06-DEP-001` on clean
`main@872c62a093f4df6549357f3a601f2f1d61943e0d`, corpus `4.9.3`, to allow
only the four production lockfiles to record `linux-arm64`. Force evaluation
changed those four lockfiles only by adding the approved RID target and the
three verified runtime packs. Locked restore then passed, and commit
`4b808319b0c1abf0970f9f41c77fb1e08d295585` recorded that graph.

`AUTH-S06-CORR-001` continued sequentially in these focused commits:

| Commit | Corrective responsibility |
| --- | --- |
| `405ab20d3e76a75f1a0f50fd625ec71831b9134b` | Adds the bounded OCI readiness plan, Linux ARM64 rehearsal builder/verifier and structural assertions. |
| `801f77625e68692fe7b4691798694b4e8d92433a` | Adds the internal deterministic composition seam and composed cancellation, provider-failure and official-source failure evidence. |
| `9d72a1bb93325f6303516592fb4ff352a0a531ca` | Makes the existing `pt-BR` README factual and records only verified local/synthetic commands and boundaries. |

The first C4 aggregate run then stopped at solution restore with `NU1004`:
the four production project files did not declare the RID already present in
their lockfiles. No later C4 check or C5 disposition was claimed from that
attempt. On clean `main@9d72a1bb93325f6303516592fb4ff352a0a531ca`, the
owner authorised `AUTH-S06-DEP-002` to add only
`<RuntimeIdentifiers>linux-arm64</RuntimeIdentifiers>` to Domain, Application,
Infrastructure and Server, and to complete the isolated cache by read-only
copy of these already locked test packages:

| Package | Version |
| --- | --- |
| `coverlet.collector` | `10.0.1` |
| `Microsoft.CodeCoverage` | `18.7.0` |
| `Microsoft.NET.Test.Sdk` | `18.7.0` |
| `Microsoft.TestPlatform.ObjectModel` | `18.7.0` |
| `Microsoft.TestPlatform.TestHost` | `18.7.0` |
| `xunit` | `2.9.3` |
| `xunit.abstractions` | `2.0.3` |
| `xunit.analyzers` | `1.18.0` |
| `xunit.assert` | `2.9.3` |
| `xunit.core` | `2.9.3` |
| `xunit.extensibility.core` | `2.9.3` |
| `xunit.extensibility.execution` | `2.9.3` |
| `xunit.runner.visualstudio` | `3.1.5` |

Every authorised source directory existed in the global cache, every target
directory was initially absent, and deterministic per-file SHA-256 tree
digests matched before and after each copy. The source tree digest also
remained unchanged. No global-cache write or network access occurred.

The full solution restore used only
`artifacts-local/s06-dependencies/verified-source`, the isolated package cache,
`eng/NuGet.Offline.config`, offline certificate revocation, `--no-cache` and
locked mode. It exited `0`. Raw SHA-256 and semantic graph digests for all
seven lockfiles were identical before and after restore. Each restored assets
file contained exactly the packages from its lockfile and only the isolated
package folder; the four production projects contained `net10.0` and
`net10.0/linux-arm64`, while the three test projects retained only `net10.0`.
Commit `f1a02cd7c7acb50bcd3fa8b00e69e6c3f59b88c3` records only the four
authorised project declarations.

### C4 corrective verification

C4 restarted from the beginning on clean
`main@f1a02cd7c7acb50bcd3fa8b00e69e6c3f59b88c3`. Runtime preflight found no
RAG-Challenge-owned process and no listener on ports 4173, 5086, 5096, 5173 or
9230.

The complete `eng/ci.ps1 -Offline` gate used the isolated NuGet cache and
passed:

- locked solution restore with zero lockfile byte or graph change;
- format verification and Release build with zero warning or error;
- 74 unit, 10 architecture and 95 integration tests, 179 total, with zero
  failure or skip;
- merged .NET coverage of 92.40% lines (17,798/19,262) and 66.60% branches
  (2,469/3,707), above the repository floors;
- offline `npm ci`, lint, typecheck, 38 tests and Vite build; and
- repository audit for 198 non-ignored files and `git diff --check`.

Two consecutive no-restore Linux ARM64 rehearsal builds from that same clean
commit produced byte-identical evidence:

| Evidence | Observed value |
| --- | --- |
| Archive SHA-256 | `0dfdf1c0604e8ccf9e3064d8131e48ae463cf655c0723dc57ebab4b06d2a2880` |
| Archive bytes | 133,379,066 |
| Archive/content files | 361 |
| Manifest payload records | 360 |
| Manifest SHA-256 | `ceafd82aadbf6552d16fd427dde534fc3feac54b2bcdf3069501ccbb8be54f65` |
| Native ELF64 AArch64 files | 17 |

The static verifier passed complete manifest readback, archive/path safety,
required execute modes, Dashboard payload, fail-closed configuration, apparent
secret/workstation-path scans and AArch64 identity for every native payload.
It did not execute the ARM64 app host and contacted no OCI endpoint.

The two commands published in the README were then executed literally from the
repository root. The local integration builder produced 58 files and a
47,234,206-byte ZIP with SHA-256
`147586466ca5a92ac18760c77822d78899200ea4c72b0898a66b45b9aafb7301`.
The verifier returned `Passed`, served the Dashboard, answered in `en-GB` and
`pt-BR`, reopened generation
`idxgen-795825d3ad7afad1acd3a16ef48f2448270dda36ea71725fe6f6231956ced2c5`
after restart, and confirmed `control.db` and `vectors.db`. This evidence used
only the synthetic CSV fixture, deterministic providers, temporary SQLite
stores and loopback on Windows.

Final C4 hygiene found a clean working tree, no diff error, no owned process
and no relevant listener. Ignored build, coverage, package-cache and rehearsal
outputs remain non-authoritative local evidence under their bounded paths.

### Corrective disposition and gate boundary

The evidence supports the following corrective status:

| Finding | Corrective evidence | Status |
| --- | --- | --- |
| `AQG-S06-001` | State-owned readiness plan, exact locked ARM64 graph, two identical self-contained cross-publishes and passing static verifier. | `CORRECTED_PENDING_GATE_RETEST` |
| `AQG-S06-002` | Deterministic composed cancellation and bounded provider/source failures, followed by successful query service and restart against the same active generation. | `CORRECTED_PENDING_GATE_RETEST` |
| `AQG-S06-003` | Accepted ownership rule, reconciled corpus `4.9.3`, factual README and locally re-executed synthetic example commands/results. | `CORRECTED_PENDING_GATE_RETEST` |

The original Automatic Quality Gate remains **REPROVADO** as historical fact.
C4 and C5 are corrective verification and factual reconciliation, not
`AUTH-S06-AQG-RETEST-001`; they do not resolve the findings or approve the
gate. `STATE-06` remains active and its Human Gate remains premature.

Linux ARM64 execution, OCI tenancy/IAM/capacity/network/storage/cost, TLS,
operational backup/restore, providers, accounts, real corpus and real official
sources remain untested and unauthorised. No OCI, GitHub, publication,
deployment, public contract, OpenAPI, schema, migration, ADR, Human Gate,
`STATE-07` or external product action occurred.

Rollback was not executed. A future rollback requires ordinary focused reverts
of the applicable corrective commits under separate authority, plus a
compensating append-only factual record. Ignored task caches and outputs may be
removed only from their validated paths under explicit cleanup authority; no
external state exists to roll back.

## Automatic Quality Gate restart — 2026-08-06

`AUTH-S06-AQG-RETEST-001` restarted the complete `STATE-06` Automatic Quality
Gate locally, offline and sequentially. The audited baseline was
`main@9d7c4ce816eca049ba09942ab7fe8b1148aa73c9`, prompt corpus `4.9.3`, with a
clean working tree. The run made no silent correction and stopped neither for
a finding nor for a concurrent change, divergence or authority boundary.

Restricted preflight found no RAG-Challenge-owned process and no listener on
ports 4173, 5086, 5096, 5173 or 9230. The repository-open editor was not a
product process and was left untouched. Static inspection covered the complete
corrective diff from the failed-gate baseline, the internal composition seam,
production defaults, focused tests, project RID declarations, lockfiles,
rehearsal scripts, CI scripts, configuration and published README commands.
No static finding was raised.

### Dependency, restore and technical evidence

The isolated verified source contained exactly the following signed `10.0.10`
packages:

| Package | Size (bytes) | Verification |
| --- | ---: | --- |
| `Microsoft.AspNetCore.App.Runtime.linux-arm64` | 12,387,032 | Catalogue SHA-512, source SHA-256, author/repository signatures, MIT licence and zero package dependencies matched. |
| `Microsoft.NETCore.App.Host.linux-arm64` | 5,309,240 | Catalogue SHA-512, source SHA-256, author/repository signatures, MIT licence and zero package dependencies matched. |
| `Microsoft.NETCore.App.Runtime.linux-arm64` | 37,584,411 | Catalogue SHA-512, source SHA-256, author/repository signatures, MIT licence and zero package dependencies matched. |

Signature verification used offline revocation mode. Solution restore used only
the verified local source, isolated NuGet package and HTTP caches,
`--locked-mode` and `--no-cache`. It passed without changing any of the seven
lockfiles. The four production assets contained only `net10.0` and
`net10.0/linux-arm64` targets; the three test assets contained only `net10.0`;
all used the isolated package folder.

`eng/ci.ps1 -Offline` then passed locked restore, format, Release build with
zero warnings and errors, 74 unit tests, 10 architecture tests and 95
integration tests: 179 total, with no failure or skip. Merged .NET coverage was
92.40% of lines (17,798/19,262) and 66.60% of branches (2,469/3,707). Offline
`npm ci`, lint, typecheck, 38 tests and the Vite build passed. The repository
audit covered 198 non-ignored files, and Git diff hygiene passed.

The four focused composed-host tests passed. They repeated successful local
operation and restart, cancellation, bounded provider and official-source
failure, subsequent recovery, and service after restart against the same
active generation.

### ARM64, README and security evidence

Two consecutive ARM64 rehearsal builds on the same baseline produced identical
archives:

| Evidence | Reproduction 1 | Reproduction 2 |
| --- | --- | --- |
| Archive SHA-256 | `d539f0dd27553859966fe45f373363d32ffd34c61cd59618fe7cf61dcd9b2369` | `d539f0dd27553859966fe45f373363d32ffd34c61cd59618fe7cf61dcd9b2369` |
| Archive size | 133,379,066 bytes | 133,379,066 bytes |
| Archive entries | 361 | 361 |
| Manifest SHA-256 | `ba2ba62001b6da0fb4c9405fcd419d398d491dee0557fa1ceb035394c865fddb` | `ba2ba62001b6da0fb4c9405fcd419d398d491dee0557fa1ceb035394c865fddb` |
| Manifest payload records | 360 | 360 |

The static verifier passed integrity, required execution modes, Dashboard
payload, fail-closed configuration, apparent secret/workstation-path scans and
17 native ELF64 AArch64 payloads. It reported `LinuxArm64Executed: false` and
`OciContacted: false`.

The two local integration commands published in the README were executed from
the repository root. They produced a 58-file, 47,234,206-byte ZIP with SHA-256
`fc3604a8d99a87c0f0d71b37309c125f7645ba1516ee833cba30ff3310a39a2f` and a
`Passed` verification result. The Dashboard was served, answers succeeded in
`en-GB` and `pt-BR`, `control.db` and `vectors.db` were present, and generation
`idxgen-795825d3ad7afad1acd3a16ef48f2448270dda36ea71725fe6f6231956ced2c5`
remained active after restart.

Final security and hygiene inspection found no protected OpenAPI, dependency,
migration, schema or ADR change in the corrective range; no tracked
`reference-materials/`; no owned runtime; no task-port listener; a clean
working tree; and a passing `git diff --check`. The public OpenAPI SHA-256
remained `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
The recorded toolchain was Windows `10.0.26200.0`, PowerShell `7.6.4`, Git
`2.55.0.windows.3`, .NET SDK `10.0.302`, Node.js `24.19.0` and npm `11.17.0`.

### Finding disposition and gate result

| Finding | Repeated gate evidence | Final disposition |
| --- | --- | --- |
| `AQG-S06-001` | State-owned plan, exact locked graph, two identical ARM64 builds and passing static verifier. | `RESOLVED` |
| `AQG-S06-002` | Composed cancellation and bounded provider/source failures, followed by recovery and restart against the same generation. | `RESOLVED` |
| `AQG-S06-003` | Accepted ownership rule, corpus `4.9.3`, factual README and repeated published commands. | `RESOLVED` |

The restarted Automatic Quality Gate is **APROVADO**, with no new P0, P1, P2
or P3 finding. The earlier failed gate and corrective dispositions remain
historical evidence; this restart is the authority that resolved the three
findings.

Linux ARM64 execution, OCI tenancy/IAM/capacity/network/storage/cost, TLS,
operational backup/restore, providers, accounts, real corpus, real official
sources, JavaScript percentage coverage and packet-level offline observation
remain untested. Ignored local artefacts and caches remain non-authoritative.
No network, global cache, OCI, GitHub, public contract, OpenAPI, schema,
migration, ADR, publication, deployment, Human Gate or `STATE-07` action
occurred. `STATE-06` remains active.
