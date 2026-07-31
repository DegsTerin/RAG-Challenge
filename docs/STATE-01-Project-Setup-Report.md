# STATE-01 project setup report

## Purpose and scope

This report records the factual execution of `STATE-01 PROJECT_SETUP` on
2026-07-30. The authorised scope was local Git and focused commits, the MIT
licence, repository configuration, pinned toolchains, dependency management
and lockfiles, the ADR-0001 scaffold, structural tests, minimal health
endpoints, onboarding, and CI definition without deployment.

The authority explicitly excluded functional RAG or product behaviour,
ADR-0002, corpus decisions, providers, official sources, durable persistence,
infrastructure, external network access, software installation, GitHub, OCI,
push, publication, deployment, CD, and DB-Notifier changes.

## Environment

| Item | Observed value |
|---|---|
| Date | `2026-07-30` |
| Working directory | `<challenge-root>` |
| Operating system | Windows |
| PowerShell | `7.6.4` |
| Git | `2.55.0.windows.3` |
| .NET SDK | `10.0.302` |
| .NET runtime used by format | `10.0.10` |
| Node.js | `24.18.0` |
| npm | `11.16.0` |
| ripgrep | `15.2.0` |
| Setup candidate commit | `16aec5f8586f07c9a9d89165e330335b460d6fbf` |

No real workstation or host name is stored in project files. Evidence uses
the stable `<challenge-root>` placeholder.

## Preconditions

- The `STATE-00` Human Gate was approved.
- `GATE-B01` was approved.
- ADR-0001 was accepted.
- The MIT repository licence and physical project map were recorded.
- `.git`, `LICENSE`, scaffold, dependencies, and implementation were absent
  before the entry authority.
- The runtime preflight found zero Challenge-owned processes.

## Delivered artefacts

### S01-A — Repository foundation

- Initialised a local Git repository on `main`.
- Materialised the approved MIT `LICENSE`.
- Added `.editorconfig`, `.gitattributes`, and expanded `.gitignore`.
- Preserved `/reference-materials/` as ignored local-only content.
- Pinned .NET SDK, C#, Node.js, and npm versions.
- Added central NuGet package management and repository lockfile policy.
- Generated and validated seven .NET `packages.lock.json` files offline.
- Added the React/TypeScript dependency manifest with exact versions.

The npm `package-lock.json` is not delivered. npm's offline mode reported
`ENOTCACHED` because the local cache lacks package metadata required to
resolve the complete dependency graph. No online fallback was attempted.

### S01-B — Empty boundaries

The scaffold matches ADR-0001:

```text
Challenge.Domain
Challenge.Application
Challenge.Infrastructure
Challenge.Server.Api
Challenge.Dashboard.Web

Challenge.UnitTests
Challenge.Architecture.Tests
Challenge.IntegrationTests
```

The production reference matrix is:

```text
Challenge.Application -> Challenge.Domain
Challenge.Infrastructure -> Challenge.Application + Challenge.Domain
Challenge.Server.Api -> Challenge.Application + Challenge.Infrastructure
Challenge.Dashboard.Web -> no .NET or provider dependency
```

The .NET projects contain only assembly markers, setup composition,
fail-closed external-service configuration, and dependency-free health
endpoints. No RAG port, adapter, persistence implementation, query contract,
administrative command, or product rule was added.

Structural tests verify:

- the approved project-reference matrix;
- core isolation from outer frameworks and adapters;
- canonical namespace ownership;
- absence of prohibited RAG, persistence, and administration projects;
- absence of DB-Notifier project references;
- Dashboard isolation from server and provider packages;
- setup composition through the server host;
- fail-closed external-service configuration;
- liveness and readiness route mappings without opening a listener.

### S01-C — Initial CI

- Added a least-privilege GitHub Actions workflow with no deployment job.
- Checkout credentials are not persisted.
- The workflow applies timeout and concurrency cancellation.
- Toolchain versions are checked before the repository gate.
- Added local format, repository audit, merged coverage, and CI scripts.
- Added Dashboard lint and Node-native structural tests.
- Added setup onboarding and local/offline procedures.

The workflow was defined but not executed on GitHub. GitHub mutation and
external CI execution remain unauthorised.

## Commands and observed results

All successful commands below ran from `<challenge-root>` unless stated
otherwise.

| Command or check | Exit code | Result |
|---|---:|---|
| `git init -b main` | `0` | Empty local repository initialised. |
| `.NET project templates --no-restore` | `0` | Seven approved .NET projects created. |
| `dotnet restore Challenge.sln --configfile eng/NuGet.Offline.config` | `0` | All seven .NET projects restored from local cache; lockfiles generated. |
| Same restore with `--locked-mode` | `0` | Lockfiles reproduced successfully offline. |
| `dotnet format Challenge.sln --verify-no-changes --no-restore` | `0` | No formatting changes required. |
| `dotnet build Challenge.sln --configuration Release --no-restore` | `0` | Seven projects built; zero warnings and zero errors. |
| `dotnet test ... --collect:"XPlat Code Coverage"` | `0` | 15 tests passed: 2 unit, 8 architecture, 5 integration. |
| `eng/assert-coverage.ps1` | `0` | Merged coverage: 88% lines (`22/25`) and 100% branches (`2/2`). |
| `dotnet list Challenge.sln package --include-transitive --no-restore` | `0` | Resolved dependency inventory produced from restored assets. |
| `npm run lint` | `0` | Dashboard text and format lint passed. |
| `npm test` | `0` | Two Dashboard structural tests passed. |
| `eng/check-repository.ps1` | `0` | Format, local links, ignored materials, and common secret assignments passed. |
| `npm install --offline ... --package-lock-only` | `1` | `ENOTCACHED`; complete npm dependency graph unavailable offline. |
| `eng/ci.ps1 -Offline` | `1` | All .NET gates passed, then npm clean install stopped because `package-lock.json` is absent. |

Two intermediate architecture-test runs failed because coverage
instrumentation and compiler-generated types appeared outside product
namespaces. The test was corrected to exclude instrumentation and
compiler-generated types while continuing to reject misplaced product types.
The final build and all final tests passed.

## Automatic Quality Gate

| Gate | Result | Evidence |
|---|---|---|
| Authority and negative scope | `APROVADO` | State entry and limits recorded append-only. |
| Git, licence, configuration, and toolchains | `APROVADO` | Local artefacts exist and versions are pinned. |
| Accepted project boundaries | `APROVADO` | Build and eight architecture tests pass. |
| Fail-closed setup host and health mapping | `APROVADO` | Five integration tests pass without listeners or external access. |
| .NET locked restore, build, tests, and coverage | `APROVADO` | Offline restore passes; build has zero warnings; 15 tests pass; floors exceeded. |
| Repository format, links, ignored materials, and secret pattern check | `APROVADO` | Local repository audit passes. |
| Dashboard lint and structural tests | `APROVADO` | Lint and two Node tests pass. |
| npm lockfile, clean install, typecheck, and build | `BLOQUEADO` | npm cache metadata is incomplete and network authority is absent. |
| Dependency vulnerability audits | `BLOQUEADO` | Current advisory data requires separately authorised registry access. |
| GitHub CI execution | `NÃO APLICÁVEL` | Workflow definition is local; GitHub mutation/execution is unauthorised. |
| Clean-clone reproduction | `BLOQUEADO` | The npm lockfile and registry authority are missing. |

Overall Automatic Quality Gate result: `BLOQUEADO`.

No P0 or P1 defect was observed. The blocking condition is missing authority
for package-registry access needed to complete and validate the npm lockfile,
Dashboard typecheck/build, current vulnerability audits, and a clean restore.

## Security and data

- No secret value, corpus, snapshot, provider configuration, or external URL
  was added.
- `reference-materials/` remains ignored and is not required by the .NET
  restore, build, or tests.
- External services fail closed in committed setup configuration.
- No process listener, external request, package-registry request, GitHub
  action, OCI action, or deployment was executed.
- npm's `--offline` failures were cache-only checks and did not fall back to
  the network.

## Untested or blocked behaviour

- npm clean install and `package-lock.json` reproducibility;
- Dashboard TypeScript typecheck and Vite production build;
- current npm and NuGet vulnerability advisory checks;
- external GitHub Actions execution;
- clean-clone restore, build, and test;
- live loopback health smoke, conservatively omitted under the no-network
  authority;
- every product capability owned by later states.

## Rollback

The rollback target is the pre-setup documentation baseline. Prefer focused
Git reversions after the local commit exists. Do not rewrite history, remove
`.git`, or delete material artefacts without explicit authority. Preserve
`docs/`, `prompts/`, `.gitignore`, and `reference-materials/`. Validate any
authorised rollback with the repository audit and an inventory proving that
local-only material remains intact.

## State and recommendation

`STATE-01 PROJECT_SETUP` remains active. Its Human Gate is `PENDENTE` and must
not be requested while the Automatic Quality Gate is blocked.

The minimum unblock is explicit authority for bounded package-registry access
to generate and validate `package-lock.json`, run npm clean install,
typecheck, build, and dependency audits, then execute a clean-clone
reproduction. That authority would not permit GitHub mutation, push, deploy,
OCI, product providers, or any later-state capability.
