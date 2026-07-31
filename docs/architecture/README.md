# RAG-Challenge Architecture Pack

## Status and authority

This pack contains architecture proposals prepared during `STATE-00
DISCOVERY` and the active `STATE-02 ARCHITECTURE`. ADR-0001 was accepted by
`GATE-B01` on 2026-07-30 and was later
superseded by accepted ADR-0003. ADR-0003 incorporates every non-naming
ADR-0001 decision unchanged and replaces its naming provisions. ADR-0002 and
ADR-0004 to ADR-0006 remain proposed. Acceptance records a decision but does
not authorise implementation or prove runtime behaviour.

The architecture follows DB-Notifier principles where they are proportional
to the RAG-Challenge: inward dependencies, provider-neutral contracts,
fail-closed configuration, typed outcomes, versioned evidence and explicit
gates. It does not copy DB-Notifier's Agent, WPF, service control, distributed
protocol, central PostgreSQL topology or other product-specific complexity.

## System context

```text
User / Evaluator
       |
       v
Web Dashboard
       |
       v
RAG-Challenge API
       |
       v
Application use cases
   |          |        |        |
 local     official   vector   language
 source    PDF sync   store    model
```

The API owns request validation and composition. Application owns use cases.
Infrastructure owns volatile integrations. The Dashboard never accesses a
document, vector store or language model directly.

## Dependency boundaries

```text
RagChallenge.Domain
        ^
        |
RagChallenge.Application
(including RAG abstractions)
        ^
        |
Infrastructure / Persistence / API

Dashboard -- versioned HTTP --> API
```

- Domain owns canonical identities, versions and outcomes.
- RAG abstractions own replaceable capability contracts.
- Application owns local ingestion, official synchronisation, activation,
  scoped retrieval and answer use cases.
- Infrastructure implements parser, provider, network and storage adapters.
- Hosts are composition roots.
- Dashboard has no code reference to Application and uses only versioned HTTP
  contracts.

## Runtime responsibilities

| Component | Owns | Must not own |
|---|---|---|
| Domain | identities, versions, states and invariants | AI SDK, filesystem, HTTP, SQL or UI |
| Application | use cases, policy, transaction boundaries and typed RAG ports | provider-specific code or composition |
| Infrastructure | concrete adapters, persistence and external communication | business policy |
| API | validation, HTTP mapping and composition | direct vector/LLM rules |
| Dashboard | accessible presentation | secrets, authorisation or data access |

## Failure and consistency model

- A failed candidate index never replaces the active generation.
- The generation store is the sole system of record for the atomic activation
  record binding generation, official snapshot and freshness observation;
  vector access always uses explicit corpus, generation and source-scope IDs.
- Raw document and snapshot bytes remain content-addressed and reopenable;
  vector data is derivative, not the source of truth.
- Document, provider and index incompatibility fail closed.
- External failures are isolated and returned as typed outcomes.
- Unknown or insufficient evidence never maps to a confident answer.
- Query cancellation propagates to external calls.
- Retry is bounded and limited to transient, idempotent operations.
- Source, document, chunk, index and model versions remain traceable.
- `Local` and `OfficialOnline` are hard-filtered before top-k and never fall
  back or mix silently.
- Official freshness is validated before retrieval; query-time never accesses
  the web.

## Deployment shapes

- Local development: API, Dashboard and configured providers on one machine.
- OCI MVP: one deployable application boundary, with external secrets and
  environment configuration. Official-source egress is limited to the exact
  approved URL; the runtime allowlist separately aggregates only approved
  official, AI, vector-store, secret-store, telemetry and operational
  destinations. A managed vector store also requires its own egress policy.
- GitHub: source, documentation and CI.
- GitHub Pages: optional static frontend only; never the RAG backend.
- Future DB-Notifier: RAG-Challenge owns HTTP/OpenAPI v1; the HTTP adapter
  belongs to DB-Notifier. Any in-process module requires a later decision and
  gates.

## Architecture decisions

- [ADR-0001 — Runtime Stack and Modular Monolith](ADR-0001-Runtime-Stack-And-Modular-Monolith.md)
  (`superseded` by ADR-0003)
- [ADR-0002 — RAG Lifecycle, Provider Boundaries and Source Separation](ADR-0002-RAG-Lifecycle-Providers-And-Source-Separation.md)
  (`proposed`)
- [ADR-0003 — Product and Technical Naming](ADR-0003-Product-And-Technical-Naming.md)
  (`accepted`; current bootstrap decision record)
- [ADR-0004 — MVP Corpus, Official Source and Evaluation Baseline](ADR-0004-MVP-Corpus-Official-Source-And-Evaluation.md)
  (`proposed`; external source facts unverified)
- [ADR-0005 — MVP Providers, Persistence and OCI Deployment](ADR-0005-MVP-Providers-Persistence-And-OCI-Deployment.md)
  (`proposed`; provider/package/OCI facts unverified)
- [ADR-0006 — Security, Egress, Administration and HTTP Contracts](ADR-0006-Security-Egress-Administration-And-HTTP-Contracts.md)
  (`proposed`)

## STATE-02 design artefacts

- [Canonical application, provider and HTTP contracts](STATE-02-Canonical-Contracts.md)
- [Threat model](../security/STATE-02-Threat-Model.md)

## Related contracts

- [Solution Architecture](../../prompts/foundation/Solution-Architecture-Document.md)
- [RAG Module](../../prompts/foundation/RAG-Module.md)
- [Security and Access](../../prompts/governance/Security-And-Access.md)
- [Quality Gates](../../prompts/governance/Quality-Gates.md)
- [MVP Roadmap and Backlog](../MVP-Roadmap-And-Backlog.md)

## Decisions still required

- Corpus licence and provenance in `STATE-02`; the repository licence is MIT
  as recorded by `GATE-B01`.
- Exact corpus scope and redistributable PDF.
- Exact official PDF URL, terms/licence, maxAge and egress limits.
- Embedding, vector-store and language-model implementations.
- Persistence technology for raw content, catalogue and vector index,
  including deployment durability and retention.
- Managed-vector-store data policy and egress, if a local adapter is not
  selected.
- OCI service, region, domain/TLS and operating budget.
- Evaluation dataset and thresholds.

These decisions belong to the authorised state that owns them.
