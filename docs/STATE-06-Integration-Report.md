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
- Node.js `24.18.1` differs from the repository engine pin `24.18.0`. npm is
  exactly the pinned `11.16.0`. The existing frontend dependency set provides
  no percentage coverage instrumentation, so no JavaScript line or branch
  percentage is claimed.
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
