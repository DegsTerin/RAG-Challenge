# ADR-0001 — Runtime Stack and Modular Monolith

- Status: accepted
- Date: 2026-07-29
- Accepted: 2026-07-30
- Owners: Challenge architecture / product owner

## Context

The Challenge materials suggest Python, LangChain and OCI Compute but state
that these are optional. The product owner also wants the standalone MVP to
be compatible with a future DB-Notifier module. DB-Notifier uses .NET 10,
C#, ASP.NET Core, React and TypeScript with inward dependency boundaries.

The MVP must remain small. A microservice topology, multiple production
runtimes or a framework-specific RAG core would increase setup and deployment
cost without solving a current requirement.

GitHub Pages can host static assets but cannot execute the RAG backend or keep
provider credentials. The official Challenge requires use of at least one OCI
service.

## Decision

The `GATE-B01 ARCHITECTURE_BOOTSTRAP_DECISION` accepted this ADR with the
following bootstrap shape:

- Use .NET 10 LTS and C# for Domain, RAG abstractions, Application,
  Infrastructure, persistence and API.
- Use ASP.NET Core for the versioned HTTP API and composition root.
- Use React and TypeScript for the minimal Web Dashboard.
- Use a modular monolith. The API may serve the compiled Dashboard in the MVP
  so one deployable boundary is sufficient.
- Use the following proportional physical projects:

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

- Keep RAG abstractions as conceptual and namespace boundaries inside
  `Challenge.Application`; do not create a separate
  `Challenge.Rag.Abstractions` assembly at bootstrap.
- Keep persistence adapters inside `Challenge.Infrastructure`; do not create
  `Challenge.Persistence.Sqlite` or materialise a SQLite adapter before the
  persistence decision owned by `STATE-02`.
- Map modules to projects, namespaces and folders as follows. A folder is
  materialised only when an authorised type requires it; the map does not
  require speculative empty directories.

  | Module | Approved ownership |
  |---|---|
  | `CH-MOD-01 CORPUS_CATALOG` | `Challenge.Domain.CorpusCatalog`, `Challenge.Application.CorpusCatalog` and `Challenge.Infrastructure.CorpusCatalog`. |
  | `CH-MOD-02 DOCUMENT_INGESTION` | `Challenge.Domain.DocumentIngestion`, `Challenge.Application.DocumentIngestion` and `Challenge.Infrastructure.DocumentIngestion`. |
  | `CH-MOD-03 INDEXING_RETRIEVAL` | `Challenge.Domain.IndexingRetrieval`, `Challenge.Application.IndexingRetrieval` and `Challenge.Infrastructure.IndexingRetrieval`. |
  | `CH-MOD-04 ANSWER_GENERATION` | `Challenge.Domain.AnswerGeneration`, `Challenge.Application.AnswerGeneration` and `Challenge.Infrastructure.AnswerGeneration`. |
  | `CH-MOD-05 QUERY_EXPERIENCE` | `Challenge.Application.QueryExperience`, `Challenge.Server.Api.QueryExperience` and `Challenge.Dashboard.Web/src/features/query-experience`. |
  | `CH-MOD-06 OPERATIONS_GOVERNANCE` | `Challenge.Application.OperationsGovernance`, `Challenge.Infrastructure.OperationsGovernance` and `Challenge.Server.Api.OperationsGovernance`. |
  | `CH-MOD-07 OFFICIAL_EXTERNAL_SOURCES` | `Challenge.Domain.OfficialExternalSources`, `Challenge.Application.OfficialExternalSources` and `Challenge.Infrastructure.OfficialExternalSources`. |
  | `CH-MOD-08 EXTERNAL_INTEGRATION_CONTRACTS` | `Challenge.Server.Api.Contracts.V1`, the versioned OpenAPI artefact and the Dashboard HTTP client under `Challenge.Dashboard.Web/src/api`; consumer adapters remain outside Challenge. |

  Replaceable ports remain in an `Abstractions` subnamespace of the owning
  Application module.
- Permit only these production project-reference directions:

  ```text
  Challenge.Application -> Challenge.Domain
  Challenge.Infrastructure -> Challenge.Application + Challenge.Domain
  Challenge.Server.Api -> Challenge.Application + Challenge.Infrastructure
  Challenge.Dashboard.Web -> versioned HTTP/OpenAPI only
  ```

  `Challenge.Domain` has no Challenge project reference. Infrastructure does
  not reference the API or Dashboard. Production projects do not reference
  tests, cycles are forbidden and no project references DB-Notifier.
- Use `Challenge.UnitTests` for Domain/Application unit tests,
  `Challenge.Architecture.Tests` to inspect all production .NET assemblies,
  and `Challenge.IntegrationTests` for Server/Infrastructure integration.
  Dashboard tests remain in its web toolchain.
- Architecture tests must prove the approved reference matrix, absence of
  cycles, forbidden outer frameworks and concrete adapters in
  Domain/Application, module/namespace placement, port/adapter separation,
  absence of internal or provider types in OpenAPI, Dashboard isolation,
  composition in the server host and absence of DB-Notifier references. The
  test library and package versions are selected and pinned only in an
  authorised `STATE-01`.
- Run administrative operations through an explicit one-shot mode of
  `Challenge.Server.Api`; do not create `Challenge.Tools.Admin`. Normal
  startup performs no ingestion, synchronisation, activation or rollback,
  and administrative operations are not public anonymous endpoints.
  Operating-system identity, least privilege, required reason, idempotency,
  sanitised audit and concrete command syntax remain decisions for
  `STATE-02`.

- Use SQLite as the primary candidate for local catalogue and index-manifest
  metadata, behind Application ports. The exact vector-store implementation
  is a separate provider decision.
- Pin SDK, package and Node versions; use central package management,
  lockfiles, nullable reference types, analyzers, warnings as errors and
  deterministic builds.
- Use OCI as the required public runtime target. OCI Compute is the initial
  deployment candidate because the local materials explicitly suggest it,
  but the exact service and region remain subject to `STATE-02` validation
  and owner approval.
- Keep GitHub Pages optional and frontend-only. The public backend remains in
  OCI or another separately authorised server environment.
- Do not reference DB-Notifier assemblies, databases or configuration.

The same gate selected the MIT repository licence with the exact notice
`Copyright (c) 2026 Bruno Araújo - DegsTerin.`. This does not license the
product corpus, official snapshots, third-party material,
`reference-materials/` or external trademarks. Corpus provenance, licence and
redistribution remain separate `STATE-02` decisions. The licence file is not
created until `STATE-01` receives separate authority.

## Alternatives

### Python and LangChain

Viable and explicitly suggested by the course. Not selected as the primary
proposal because it creates a second production stack relative to the future
DB-Notifier integration goal. It remains a fallback if a `STATE-02` spike
demonstrates that the .NET path cannot meet the Challenge within the delivery
constraints.

### Microservices

Rejected for the MVP. Separate deploys, discovery, network security and
distributed consistency are not justified by one corpus and one query flow.

### Static-only application on GitHub Pages

Rejected as the complete solution because it cannot safely host the backend,
protect model credentials or satisfy the OCI runtime requirement.

### Directly implement inside DB-Notifier

Rejected because the Challenge must remain independently runnable,
publishable and reviewable.

### Separate RAG, persistence and administration projects

Not selected for bootstrap. The extra assemblies do not yet provide a
distinct lifecycle or dependency boundary proportional to the MVP. The
conceptual boundaries remain explicit and architecture-tested inside
Application and Infrastructure. A separate administration project requires
a later ADR and demonstrated privilege, deployment or lifecycle isolation.

### Apache-2.0 or proprietary repository licensing

Not selected. MIT is the proportional permissive licence for the public
Challenge repository. Corpus and third-party rights remain outside that
selection.

## Consequences

- Local and future DB-Notifier development share runtime, language and
  architectural conventions.
- One deployable boundary reduces OCI and operational complexity.
- Provider SDKs remain isolated behind RAG ports.
- The Dashboard still has an independent build toolchain.
- Fewer production assemblies reduce bootstrap overhead but increase the
  importance of namespace placement and architecture tests.
- SQLite is not treated as the vector store unless a later provider decision
  explicitly selects and validates that capability.
- Challenge owns the versioned OpenAPI contract. A future consumer adapter
  belongs to DB-Notifier or another consumer repository; extraction is not
  required for the MVP.
- The owner must still select exact packages, providers, model, vector store
  and OCI shape.

## Security and operations

- Configuration fails closed when provider, model, dimensions, corpus path or
  secret reference is absent.
- Provider secrets remain outside files and frontend bundles.
- CI uses lockfiles, least privilege, secret scanning and dependency audit.
- Public deployment uses TLS, bounded requests, rate limits and external
  secrets.
- The application produces liveness independently from external provider
  health and reports readiness per dependency.
- A single deployable does not imply a single code module or shared
  responsibility.
- A conceptual module does not automatically require a separate assembly.
- The server host's administrative mode must remain explicit, one-shot and
  isolated from normal startup.

## Compatibility and migration

- Domain/Application contracts do not include ASP.NET, React, SQLite or OCI
  types.
- A different persistence or provider can be added through adapters.
- A later DB-Notifier-owned adapter maps versioned Challenge contracts at the
  boundary.
- Moving the Dashboard to GitHub Pages later requires CORS, TLS and API-origin
  review; it does not change the core.

## Acceptance checks

- Every active .NET project targets the accepted .NET 10 baseline.
- The scaffold matches the physical project map, module ownership,
  dependency rules and administrative execution shape accepted here.
- The append-only `GATE-B01` State Transition Log entry records the exact
  human decision, repository licence and evidence.
- Architecture tests prevent outer-layer references from Domain/Application.
- Architecture tests enforce the complete accepted reference matrix, module
  placement, public-contract isolation and absence of DB-Notifier references.
- Toolchain and dependency versions are pinned and restored by lockfile.
- A clean clone builds and tests without `reference-materials/`.
- The Dashboard accesses only the API.
- Local execution and the selected OCI target use the same application
  contracts.
- No DB-Notifier project reference exists.
