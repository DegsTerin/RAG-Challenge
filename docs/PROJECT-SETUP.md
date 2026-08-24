# Project setup

This guide prepares a local checkout for build and test without configuring a
provider, publishing an image or deploying a service.

## Required toolchains

- the .NET SDK selected by `global.json`;
- Node.js selected by `.nvmrc`;
- the npm range declared in `src/RagChallenge.Dashboard.Web/package.json`;
- PowerShell 7.

## Restore

```powershell
dotnet restore RAG-Challenge.sln --locked-mode

Push-Location src/RagChallenge.Dashboard.Web
npm ci --ignore-scripts --no-audit --no-fund
Pop-Location
```

Use `./eng/ci.ps1 -Offline` only after the required NuGet and npm packages are
already available in the local caches.

## Validate

```powershell
./eng/ci.ps1
```

The entrypoint builds and tests the .NET solution and Dashboard, checks
coverage, validates the Render packaging boundary, audits repository hygiene
and performs dependency audits when online.

## Local configuration

Copy only non-secret settings into local configuration. Store the OpenAI key
in `.env.local` as `OPENAI_API_KEY`; the file is ignored by Git. Never place a
credential in source, examples, logs, screenshots or command arguments.

## Product data

The licensed PostgreSQL PDF under `corpus/postgresql/18.4/` is an input to
materialisation. Generated stores and indexes remain under ignored local
artefact roots and are not part of the Git repository.
