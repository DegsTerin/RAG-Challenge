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

## 2026-07-30 identity migration addendum

### Authority and decision

The product owner requested:

```text
Gostaria de mudar o nome do projeto de Challenge para RAG-Challenge
```

The request explicitly changes the canonical project identity. ADR-0003
records `RAG-Challenge` as the public product and solution name and
`RagChallenge` as the syntax-safe PascalCase form for .NET projects,
assemblies, namespaces and configuration. This is a naming amendment only;
ADR-0003 supersedes ADR-0001 as the current decision record while
incorporating every non-naming ADR-0001 decision unchanged. Every lifecycle
boundary also remains unchanged.

The migration is isolated in commit
`8c347c0fa73fead3e03a1eb979deba9fe3617379`.

### Migrated surfaces

| Surface | Previous evidence | Current baseline |
|---|---|---|
| Product and repository identity | `Challenge` | `RAG-Challenge` |
| Solution | `Challenge.sln` | `RAG-Challenge.sln` |
| .NET prefix | `Challenge.*` | `RagChallenge.*` |
| Configuration root | `Challenge` | `RagChallenge` |
| Dashboard package | `challenge-dashboard-web` | `rag-challenge-dashboard-web` |
| Documentation root placeholder | `<challenge-root>` | `<rag-challenge-root>` |

Stable `CH-MOD-*` module IDs, `CH_*` error codes, historical evidence,
Alura/ONE Challenge references and
`reference-materials/challenge-original/` remain unchanged by design.

### Verification evidence

| Check | Exit/result | Sanitised evidence |
|---|---:|---|
| Runtime preflight | `0` | Zero verified RAG-Challenge process and zero relevant listener existed before the migration validation. |
| Forced offline lockfile regeneration and locked restore | `0` | All seven .NET lockfiles resolve without a package-version change. |
| `eng/ci.ps1 -Offline` on the main worktree | `0` | Restore, format, Release build, 15 .NET tests, coverage, Dashboard clean install, lint, two tests, typecheck, Vite build and repository audit passed. |
| .NET build | `0` | Seven projects built with zero warnings and zero errors. |
| .NET tests and coverage | `0` | 15 tests passed; merged coverage is 88% of lines and 100% of branches. |
| Dashboard checks | `0` | Two structural tests, lint, typecheck and Vite production build passed with package `rag-challenge-dashboard-web`. |
| Repository audit | `0` | 77 non-ignored files passed UTF-8, LF, final-newline, whitespace, local-link, ignored-material and common-secret-pattern checks. |
| Loopback health smoke | `0` | `/health/live` and `/health/ready` returned HTTP `200`; the listener executable was verified under the renamed project output and stopped, leaving zero listener. |
| Clean-clone reproduction | `0` | Commit `8c347c0` reproduced the complete offline gate without `reference-materials/`; the clone worktree remained clean. |
| Technical residual scan | `0` | No active solution, project, source, configuration or package-lock artefact retains the previous technical prefix; preserved historical records remain explicit. |
| Git whitespace checks | `0` | Staged and worktree diffs passed. |

No dependency version changed, so the previously recorded npm and .NET
vulnerability results remain evidence for the same resolved dependency set.
No network vulnerability audit was repeated during this offline identity
migration.

The first integrated CI attempt identified one `using` ordering error, which
was corrected without changing test coverage. A later repository-audit
attempt correctly rejected the unstaged rename index; staging the complete
migration and normalising final newlines resolved it. The final main-worktree
and clean-clone gates both passed.

### Local-only limitations

- The physical checkout directory is outside Git and was not renamed.
- Seven legacy directory trees contain no files and remain ignored locally
  because the workspace ACL denied their recursive removal. They are absent
  from the clean clone.
- The validation clone contains no local reference material or untracked
  change, but its recursive cleanup was denied by the execution policy. It
  remains only under the operating-system temporary directory.

No GitHub or OCI resource was created, renamed or contacted. No push,
publication, deployment, product provider, corpus, official source,
DB-Notifier resource or functional RAG capability was changed.

### Refreshed gate result

The `STATE-01 PROJECT_SETUP` Automatic Quality Gate remains `APROVADO` for
the renamed baseline. `STATE-01` remains active and its Human Gate remains
`PENDENTE`. A fresh complete Human Gate summary must include this addendum;
neither the rename nor this automatic validation authorises `STATE-02`.

## 2026-07-30 checkout rename and local cleanup addendum

### Subsequent factual change

The earlier local-only limitations remain unchanged because they record the
conditions observed during the identity-migration validation. Subsequently,
the checkout was observed under the physical directory name
`RAG-Challenge`, with no sibling directory named `Challenge`.

The product owner explicitly required every file under
`reference-materials/` to remain unchanged and authorised correction of the
remaining naming-audit findings.

### Preservation and cleanup evidence

Before removal, the seven legacy technical trees were resolved inside the
checkout and outside `reference-materials/`. They contained zero files and
149 directories, including their seven roots:

```text
src/Challenge.Application
src/Challenge.Domain
src/Challenge.Infrastructure
src/Challenge.Server.Api
tests/Challenge.Architecture.Tests
tests/Challenge.IntegrationTests
tests/Challenge.UnitTests
```

Fifteen ignored `bin/`, `obj/` and `TestResults/` roots contained 529
generated files. Of those files, 68 contained 501 references to the previous
absolute checkout path. An initial procedure check removed one
`*.csproj.FileListAbsolute.txt`; the subsequent complete root cleanup removed
the other 528 files. Cumulatively, the first pass removed all 529 generated
files, 186 generated directories and 149 empty legacy directories. All 335
removed directories were absent in the immediate post-condition check.

Subsequent .NET solution checks transiently recreated 14 canonical `bin/` and
`obj/` roots with 35 files and 56 directories. Every regenerated file used
the current checkout path and none contained the previous path. A second
hygiene pass removed those reused outputs, and no further .NET command was
run afterwards. The final snapshot contains no `bin/`, `obj/` or
`TestResults/` root under the seven projects.

The execution policy rejected recursive PowerShell removal before it ran. A
scoped ACL experiment on one empty legacy tree did not enable deletion, and
its original inherited ACL was restored before cleanup. The successful
procedure left all parent ACLs unchanged, cleared the OneDrive `ReadOnly`
attribute only on disposable target entries, deleted files individually and
removed empty directories bottom-up.

`reference-materials/` retained 24 files and 7,065,607 bytes. Its aggregate
SHA-256 remained
`699708516083ad2e3274098f43352c7ac93280fc6c5a0e6b0a73eaf120e319fe`
before and after cleanup. No reference material was edited, moved or removed.
The external validation clone remained outside the authorised scope.

The aggregate is the SHA-256 of an ordinally sorted UTF-8 manifest without a
BOM. Each line contains the project-relative path with forward slashes, a tab,
the byte count, a tab, the lowercase file SHA-256 and a final line feed.

The final repository audit passed for 77 non-ignored files. The governed
documentation remains at 22 documents and 111 valid local links. The
solution inventory still contains the seven expected projects, no legacy
technical root remains, and no previous absolute checkout path remains
outside `.git/` and `reference-materials/`.

### Lifecycle effect

This local cleanup does not change executable behaviour or the approved setup
baseline. `STATE-01 PROJECT_SETUP` remains active, its Automatic Quality Gate
remains `APROVADO`, and its Human Gate remains `PENDENTE`. No lifecycle
transition, remote action or external-resource mutation was authorised.
Runtime preflight was `NÃO APLICÁVEL`; no process or listener was inspected
or stopped.
