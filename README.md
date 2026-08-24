# RAG-Challenge

RAG-Challenge is a .NET and React retrieval-augmented generation application
for querying authorised database documentation with grounded answers and
source citations.

## Online demonstration

The public demonstration runs on a single free Render Web Service:

[Open RAG-Challenge](https://rag-challenge-ac09.onrender.com)

The free instance can sleep after inactivity, so its first response can take
approximately 50 seconds. The service uses an immutable private GHCR image,
an ephemeral runtime store and no Render database or persistent disk.

## Public repository contents

- `src/`: Domain, Application, Infrastructure, API and Dashboard source code.
- `tests/`: automated .NET tests.
- `deploy/render-free/`: the pinned container and free-hosting boundary.
- `eng/`: build, test, packaging and runtime scripts used by the public system.
- `docs/api/`: versioned OpenAPI contracts.
- `docs/evaluation/`: deterministic evaluation fixtures and schemas.
- `corpus/postgresql/18.4/`: the licensed PostgreSQL 18.4 documentation input.

Internal agent configuration, governance prompts, private evidence, generated
stores, secrets, build output and unauthorised source material are excluded.

## Architecture

```text
Browser
  -> React Dashboard
  -> ASP.NET Core API
  -> Application use cases
  -> authorised content store and parser
  -> embeddings and vector retrieval
  -> grounded generation with citations
```

The inner Domain and Application projects do not depend on providers, storage,
web frameworks or the user interface. Infrastructure implements their ports,
and the API host composes the runtime.

## Prerequisites

- .NET SDK defined by `global.json`;
- Node.js and npm versions defined by `.nvmrc` and the Dashboard package;
- PowerShell 7 for the engineering entrypoints;
- an OpenAI API key supplied only through `OPENAI_API_KEY` when provider-backed
  operations are explicitly run.

Never commit `.env.local`, credentials, generated stores or provider output.

## Build and test

```powershell
dotnet restore RAG-Challenge.sln --locked-mode
dotnet build RAG-Challenge.sln --configuration Release --no-restore
dotnet test RAG-Challenge.sln --configuration Release --no-build --no-restore

Push-Location src/RagChallenge.Dashboard.Web
npm ci --ignore-scripts --no-audit --no-fund
npm run lint
npm run typecheck
npm test
npm run build
Pop-Location
```

The complete local system check is available through:

```powershell
./eng/ci.ps1
```

## Local PostgreSQL product runtime

The product runtime consumes an activated, content-addressed store. A prepared
store remains local and is not committed. Once that store and the required
non-secret authority references exist, start the runtime with:

```powershell
./eng/Start-PostgreSql18Product.ps1
```

The public PostgreSQL PDF is a reproducible source input; it is not itself the
runtime content store or an activated index.

## Render package

The packaging entrypoints operate locally and do not publish or deploy by
themselves:

```powershell
./eng/Build-RenderFreePackage.ps1
./eng/Test-RenderFreePackage.ps1
```

## API contracts

- [OpenAPI v1](docs/api/openapi-v1.json)
- [OpenAPI v2](docs/api/openapi-v2.json)

## Corpus licence

The repository software is licensed under the [MIT License](LICENSE).
PostgreSQL documentation is distributed separately under the PostgreSQL
Licence. Its complete notice, provenance, digest and redistribution boundary
are recorded in
[`corpus/postgresql/18.4/NOTICE.md`](corpus/postgresql/18.4/NOTICE.md).

Oracle documentation and all other unapproved source material are excluded
from this public repository.
