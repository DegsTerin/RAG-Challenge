# Project setup

## Purpose

This guide reproduces the `STATE-01 PROJECT_SETUP` scaffold. It covers only
repository conventions, empty architectural boundaries, dependency-free
health endpoints, structural tests, and CI. It does not authorise or implement
RAG, persistence, providers, a product corpus, external sources, deployment,
or DB-Notifier integration.

## Pinned toolchains

- Git `2.55.0.windows.3`
- .NET SDK `10.0.302`
- C# `14.0`
- Node.js `24.18.0`
- npm `11.16.0`

`global.json`, `.nvmrc`, `package.json`, central NuGet package management, and
lockfiles are the machine-readable authorities.

## Repository boundaries

The accepted production dependency direction is:

```text
RagChallenge.Application -> RagChallenge.Domain
RagChallenge.Infrastructure -> RagChallenge.Application + RagChallenge.Domain
RagChallenge.Server.Api -> RagChallenge.Application + RagChallenge.Infrastructure
RagChallenge.Dashboard.Web -> versioned HTTP/OpenAPI only
```

The Dashboard is a separate React/TypeScript build boundary. It does not
reference .NET projects, providers, persistence, or DB-Notifier.

## Restore

The standard restore requires access to the package registries configured in
`NuGet.config` and npm's lockfile:

```powershell
dotnet restore RAG-Challenge.sln --locked-mode
Set-Location src/RagChallenge.Dashboard.Web
npm ci --ignore-scripts --no-audit --no-fund
Set-Location ../..
```

In a governed local execution, do not run the standard restore until package
registry access is explicitly authorised. When all packages already exist in
the local caches, use:

```powershell
dotnet restore RAG-Challenge.sln `
  --configfile eng/NuGet.Offline.config `
  --locked-mode
Set-Location src/RagChallenge.Dashboard.Web
npm ci --offline --ignore-scripts --no-audit --no-fund
Set-Location ../..
```

An offline cache miss is a blocked validation, not permission to fall back to
the network.

## Build and test

After a successful restore:

```powershell
dotnet format RAG-Challenge.sln --verify-no-changes --no-restore
dotnet build RAG-Challenge.sln --configuration Release --no-restore
dotnet test RAG-Challenge.sln `
  --configuration Release `
  --no-build `
  --no-restore `
  --collect:"XPlat Code Coverage"

Set-Location src/RagChallenge.Dashboard.Web
npm run lint
npm run typecheck
npm test
npm run build
Set-Location ../..

./eng/check-repository.ps1
git diff --check
```

The full local/CI entry point is:

```powershell
./eng/ci.ps1
```

Use `./eng/ci.ps1 -Offline` only when both dependency caches and both lockfile
sets are already complete.

NuGet can reserialise tracked `packages.lock.json` files with platform line
endings during restore on Windows. The CI entry point reports and normalises
only those tracked generated files to the repository's UTF-8/LF convention
before hygiene checks; locked restore still rejects dependency-graph changes.

## Setup host

The API host exposes only dependency-free liveness and readiness endpoints.
Before starting it, apply the runtime preflight in `AGENTS.md`.

```powershell
dotnet run `
  --project src/RagChallenge.Server.Api/RagChallenge.Server.Api.csproj `
  --no-restore `
  --no-launch-profile `
  --urls http://127.0.0.1:5242
```

The setup endpoints are:

- `GET /health/live`
- `GET /health/ready`

`RagChallenge:Setup:AllowExternalServices` defaults to `false`. Setting it to
`true` fails startup closed. No administrative mode, ingestion, query,
provider, persistence, or external connection exists in this state.

## Secrets and local materials

- Keep secrets outside the repository.
- Do not commit `.env` files, local settings, runtime output, or test output.
- `reference-materials/` remains ignored and is not required by restore,
  build, tests, or runtime.
- Do not enable external services or package registry access by inference.

## CI boundary

`.github/workflows/ci.yml` has read-only repository permission, does not
persist checkout credentials, applies a timeout and concurrency cancellation,
uses locked restores, and runs the repository gate. It contains no deployment
job. Executing it on GitHub requires separate external authority.
