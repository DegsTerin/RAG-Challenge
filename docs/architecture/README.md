# RAG-Challenge Architecture Pack

## Status and authority

This pack contains architecture proposals prepared during `STATE-00
DISCOVERY` and the active `STATE-02 ARCHITECTURE`. ADR-0001 was accepted by
`GATE-B01` on 2026-07-30 and was later
superseded by accepted ADR-0003. ADR-0003 incorporates every non-naming
ADR-0001 decision unchanged and replaces its naming provisions. ADR-0002 and
ADR-0004 to ADR-0006 were accepted explicitly and independently by the owner
on 2026-08-01 against
`main@39e2f803bf73cb4e2b59e56a0596e2858a3aed51`, corpus `4.7.0`.
Acceptance records architecture authority but does not authorise
implementation or prove runtime behaviour.

The later combined audit failed on `AQG-S02-001`, an internal contradiction
between observation-inclusive generation identity and observation-only
freshness rebinding. ADR-0007 now records a corrective proposal; it is not
accepted and does not alter ADR-0002 until an explicit owner decision.

The owner has independently fixed the query-language boundary to `pt-BR` and
`en-GB`: answers use the declared question language and source-derived
citations retain their original language. That constraint did not at the time
accept an ADR or determine the Dashboard language. A later independent owner
decision selected `pt-BR` and `en-GB` as the supported Dashboard languages;
the visual selection remains independent from the query language.

The owner also selected `Light` and `Dark` as the supported Dashboard themes.
Theme state remains independent from interface and query languages and is not
part of the public query contract.

The owner has now fixed the initial canonical catalogue at 51 unique database
products, 9 categories and 54 many-to-many assignments. Every active database
has at least one active PDF or CSV document; any number of additional compatible
documents may be administered without a hard-coded list, code change or ADR per
item. All active documents participate in unified retrieval, while local or
official origin remains explicit provenance. This constraint was established
independently before the four ADRs were later accepted and does not authorise
implementation.

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
   |             |        |        |
 catalogue/   governed   vector   language
 PDF+CSV      sources    store    model
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
- Application owns catalogue/document administration, local ingestion,
  official synchronisation, activation, unified retrieval and answer use cases.
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
  record binding generation and the ordered set of active database/document,
  snapshot and freshness identities; vector access always uses explicit corpus
  and generation IDs. The exact digest boundary between snapshot and
  observation identity is the unresolved subject of proposed ADR-0007.
- Raw document and snapshot bytes remain content-addressed and reopenable;
  vector data is derivative, not the source of truth.
- Document, provider and index incompatibility fail closed.
- External failures are isolated and returned as typed outcomes.
- Unknown or insufficient evidence never maps to a confident answer.
- Query cancellation propagates to external calls.
- Retry is bounded and limited to transient, idempotent operations.
- Source, document, chunk, index and model versions remain traceable.
- Every active/current document is eligible in unified retrieval; origin and
  trust remain citation metadata, and partial coverage is explicit rather than
  a silent fallback.
- Official freshness is validated per source before retrieval; query-time never
  accesses the web.
- Query contracts use explicit `pt-BR`/`en-GB` language tags; answer language
  matches the question and citation content remains in its source language.

## Deployment shapes

- Local development: API, Dashboard and configured providers on one machine.
- OCI MVP: one deployable application boundary, with external secrets and
  environment configuration. Official-source egress is limited to the exact
  approved active URL set; the runtime allowlist separately aggregates only approved
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
  (`accepted`)
- [ADR-0003 — Product and Technical Naming](ADR-0003-Product-And-Technical-Naming.md)
  (`accepted`; current bootstrap decision record)
- [ADR-0004 — MVP Catalogue, Governed Documents, Official Sources and Evaluation](ADR-0004-MVP-Corpus-Official-Source-And-Evaluation.md)
  (`accepted`; every actual document/source still requires its own evidence
  and activation)
- [ADR-0005 — MVP Providers, Persistence and OCI Deployment](ADR-0005-MVP-Providers-Persistence-And-OCI-Deployment.md)
  (`accepted`; package versions, OCI targets and operational evidence remain
  conditional as recorded)
- [ADR-0006 — Security, Egress, Administration and HTTP Contracts](ADR-0006-Security-Egress-Administration-And-HTTP-Contracts.md)
  (`accepted`; profiles remain disabled and untested)
- [ADR-0007 — Generation Identity and Freshness Observation Rebinding](ADR-0007-Generation-Identity-And-Freshness-Observation-Rebinding.md)
  (`proposed`; corrective decision pending for `AQG-S02-001`)

## STATE-02 design artefacts

- [Canonical application, provider and HTTP contracts](STATE-02-Canonical-Contracts.md)
- [Threat model](../security/STATE-02-Threat-Model.md)

## Related contracts

- [Solution Architecture](../../prompts/foundation/Solution-Architecture-Document.md)
- [RAG Module](../../prompts/foundation/RAG-Module.md)
- [Security and Access](../../prompts/governance/Security-And-Access.md)
- [Quality Gates](../../prompts/governance/Quality-Gates.md)
- [MVP Roadmap and Backlog](../MVP-Roadmap-And-Backlog.md)

## Remaining evidence and activation inputs

- Actual initial PDF/CSV documents, per-document rights, provenance and
  language evidence before any database product becomes active.
- Evidence and explicit activation for each additional official source
  registration; no generic URL or crawling authority exists.
- Exact PDF/CSV package versions and disposable extraction/security spikes
  required by accepted ADR-0005.
- OpenAI account entitlement, limits, spend controls and the accepted
  bilingual evaluation before provider use.
- Representative exact-vector performance, persistence restart, backup
  consistency and restore evidence.
- OCI tenancy capacity, entitlement, IAM enforcement, effective billing and
  restore evidence for the accepted conditional target.
- The extensible evaluation dataset and thresholds frozen before scored runs.
- An explicit decision on proposed ADR-0007, its post-acceptance semantic
  reconciliation and a new separately authorised combined `STATE-02` audit
  before any Human Gate. The source documents for `AQG-S02-002` and
  `AQG-S02-003` have been factually reconciled, but the failed gate remains
  unchanged until that audit.

`AQG-S02-001` is evidence that requires a material change through a later ADR;
the other inputs do not reopen the accepted architecture merely by remaining
conditional or unverified.
