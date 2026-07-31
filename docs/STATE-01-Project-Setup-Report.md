# STATE-01 project setup report

## Purpose and scope

This report records the factual execution of `STATE-01 PROJECT_SETUP` on
2026-07-30. The authorised scope was local Git and focused commits, the MIT
licence, repository configuration, pinned toolchains, dependency management
and lockfiles, the ADR-0001 scaffold, structural tests, minimal health
endpoints, onboarding, and CI definition without deployment.

The entry authority explicitly excluded functional RAG or product behaviour,
ADR-0002, corpus decisions, providers, official sources, durable persistence,
infrastructure, external network access, software installation, GitHub, OCI,
push, publication, deployment, CD, and DB-Notifier changes. A later bounded
authority permitted only the npm and NuGet registries, installation of pinned
dependencies without lifecycle scripts, dependency audits, and loopback
health smoke needed to finish this gate.

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
| Gate candidate commit | `8a604ceaa34162673aea6b7ce3267bc9d3f8b83a` |

No real workstation or host name is stored in project files. Evidence uses
the stable `<challenge-root>` placeholder.

## Preconditions

- The `STATE-00` Human Gate was approved.
- `GATE-B01` was approved.
- ADR-0001 was accepted.
- The MIT repository licence and physical project map were recorded.
- `.git`, `LICENSE`, scaffold, dependencies, and implementation were absent
  before the entry authority.
- Bounded npm/NuGet registry access and loopback health smoke were separately
  authorised after the offline gate blocker was recorded.
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
- Generated npm `package-lock.json` v3 with 53 package entries and only
  `registry.npmjs.org` resolved URLs.
- Installed the pinned Dashboard dependencies with lifecycle scripts
  disabled. The only install-script metadata belongs to optional macOS
  package `fsevents`; it was not executed.

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
- Validated liveness and readiness on an ephemeral loopback listener and
  removed the listener after the smoke.

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
| `npm install --package-lock-only --ignore-scripts` | `0` | Lockfile v3 generated with pinned direct versions and only the authorised npm registry. |
| `npm ci --ignore-scripts` | `0` | Clean install added 21 packages without lifecycle script execution. |
| `npm run lint`; `npm test` | `0` | Lint and two structural tests passed. |
| `npm run typecheck`; `npm run build` | `0` | TypeScript and Vite production build passed. |
| `npm audit --audit-level=high` | `0` | Zero vulnerabilities at every severity. |
| `dotnet list Challenge.sln package --vulnerable --include-transitive` | `0` | No vulnerable package found in seven projects. |
| Loopback health smoke | `0` | `/health/live` and `/health/ready` returned `200 Healthy`; zero listener remained after cleanup. |
| Clean clone `eng/ci.ps1` | `0` | Isolated caches reproduced the complete gate without `reference-materials/`; worktree remained clean. |
| Main worktree `eng/ci.ps1` | `0` | Integral gate repeated successfully on the committed baseline. |

Two intermediate architecture-test runs failed because coverage
instrumentation and compiler-generated types appeared outside product
namespaces. The test was corrected to exclude instrumentation and
compiler-generated types while continuing to reject misplaced product types.
The final build and all final tests passed.

The first Dashboard typecheck failed because the TypeScript Vite
configuration imported Vite's Node-facing declarations without a direct Node
type dependency. The configuration was converted to JavaScript ESM and
excluded from the application typecheck. No dependency or version was added
or changed; the complete Dashboard sequence then passed.

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
| npm lockfile, clean install, typecheck, and build | `APROVADO` | Lockfile v3, clean install, lint, tests, typecheck, and Vite build pass. |
| Dependency vulnerability audits | `APROVADO` | npm and all seven .NET projects have zero observed vulnerabilities. |
| GitHub CI execution | `NÃO APLICÁVEL` | Workflow definition is local; GitHub mutation/execution is unauthorised. |
| Loopback health smoke | `APROVADO` | Both health routes returned `200 Healthy`; cleanup left zero listener. |
| Clean-clone reproduction | `APROVADO` | Isolated restores and the integral gate pass without local-only materials. |

Overall Automatic Quality Gate result: `APROVADO`.

No P0-P3 residual setup finding was observed. GitHub workflow execution is
not evidence required from this local authority and remains `NÃO APLICÁVEL`.

## Security and data

- No secret value, corpus, snapshot, provider configuration, or product
  external URL was added.
- `reference-materials/` remains ignored and is not required by the .NET
  restore, build, or tests.
- External services fail closed in committed setup configuration.
- Registry traffic used only `registry.npmjs.org` and the configured
  `api.nuget.org` service. Entry-point probes did not redirect, npm lock
  entries use only the authorised npm host, and the NuGet vulnerability
  resource remained on the authorised host.
- The only listener bound to loopback for health smoke and was stopped after
  the checks.
- No GitHub action, OCI action, push, publication, deployment, CD, product
  provider, official-source access, or DB-Notifier change was executed.
- npm's `--offline` failures were cache-only checks and did not fall back to
  the network.

## Untested or blocked behaviour

- external GitHub Actions execution;
- every product capability owned by later states.

## Rollback

The rollback target is the pre-setup documentation baseline. Prefer focused
Git reversions after the local commit exists. Do not rewrite history, remove
`.git`, or delete material artefacts without explicit authority. Preserve
`docs/`, `prompts/`, `.gitignore`, and `reference-materials/`. Validate any
authorised rollback with the repository audit and an inventory proving that
local-only material remains intact.

## State and recommendation

`STATE-01 PROJECT_SETUP` remains active. Its Automatic Quality Gate is
`APROVADO` and its Human Gate is `PENDENTE`.

The next lifecycle action is a separate human review of this complete setup
summary. Automatic approval does not close `STATE-01`, authorise `STATE-02`,
or permit GitHub mutation, push, deployment, OCI, product providers, or any
later-state capability.
