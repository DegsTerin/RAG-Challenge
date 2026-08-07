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

The owner later accepted ADR-0008 and ADR-0009 on 2026-08-07. Prompt corpus
`4.9.5` applies their separately authorised semantic reconciliation: durable
content-addressed source/page-image storage, deterministic PDF render
manifests, expanded rights gates, separate closed query and open BCP 47
document-language domains, original-language citations and stratified
evaluation. This is architecture documentation only; OpenAPI v1 remains
byte-for-byte unchanged and v2 remains planned and unimplemented.

The later combined audit failed on `AQG-S02-001`, an internal contradiction
between observation-inclusive generation identity and observation-only
freshness rebinding. The owner accepted ADR-0007 explicitly on 2026-08-02. It
now supersedes the conflicting generation-identity and exact-record rollback
clauses of ADR-0002. Corpus `4.9.1` applies the traced semantic reconciliation;
the separately authorised renewed audit on
`main@3978a17201cf5f6ac4ddc189862736fc3646457b` approved the Automatic
Quality Gate, disposed `AQG-S02-001` to `AQG-S02-003` as `RESOLVED` and found
no new classified finding. The Human Gate remains pending and was not
requested.

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
  and generation IDs. Accepted ADR-0007 separates the generation-bound source
  digest from the activation-record digest that also covers observation
  identity; the accepted semantic documents are reconciled in corpus `4.9.1`.
- Authorised source bytes and persistent page-image bytes remain
  content-addressed and reopenable in the document content store; Git, Git LFS,
  intake quarantine and vector data are not the product source of truth.
- A visually active PDF requires the complete deterministic render manifest,
  every verified page-image object and applicable derivative/display rights;
  CSV has no implicit page-image evidence.
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
- Query contracts use the closed explicit `pt-BR`/`en-GB` language tags;
  document/evidence content uses a distinct canonical BCP 47 domain. Answer
  language matches the question, publisher `en` is not inferred as `en-GB`,
  and source-derived citation content remains in its original language.

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
  (`accepted`; corrective semantic reconciliation applied and renewed audit
  approved)
- [ADR-0008 — Product Corpus Storage and Page-Image Evidence](ADR-0008-Product-Corpus-Storage-And-Page-Image-Evidence.md)
  (`accepted`; semantic reconciliation applied in corpus `4.9.5`;
  implementation remains separately authorised)
- [ADR-0009 — Document, Evidence and Query Language Taxonomy](ADR-0009-Document-Evidence-And-Query-Language-Taxonomy.md)
  (`accepted`; semantic reconciliation applied in corpus `4.9.5`;
  implementation remains separately authorised)

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
- A complete `STATE-02` Human Gate summary, prepared and presented only under
  separate owner authority. The approved Automatic Quality Gate does not
  request or decide that Human Gate.

ADR-0007 and corpus `4.9.1` supplied the material decision and documentary
change required by `AQG-S02-001`; the renewed audit disposed all three
historical findings without altering the accepted architecture. The other
inputs do not reopen that architecture merely by remaining conditional or
unverified.

On 2026-08-07, the owner first authorised preparation of ADR-0008 as a change
proposal during `STATE-07` and later accepted it explicitly on baseline
`main@5c151c64ae4d3049d68fee6788502d439aa25251`, corpus `4.9.4`. It establishes
architectural authority for durable product-corpus storage and persistent PDF
page-image evidence. Acceptance does not reconcile other normative documents,
implement the decision, move source bytes, render images or activate content.

Preparation of ADR-0009 was authorised on 2026-08-07 after the PostgreSQL PDF
language tag `en` exposed a conflict with the current closed `pt-BR`/`en-GB`
document-language contract. The owner explicitly accepted ADR-0009 on
2026-08-07 over baseline
`main@89994e82d246b1cc0a240e99a2d09942e316f7cc`, corpus `4.9.4`, exclusively
as architectural authority. The decision separates document and evidence
language tags from query and answer languages, rejects implicit `en` to
`en-GB` mapping and keeps OpenAPI v1 unchanged. Acceptance does not reconcile
other normative documents or authorise implementation.

On 2026-08-07, the owner then authorised the bounded joint semantic
reconciliation on clean
`main@3d15ad4f2726f715c8dcf880491927ad0ff37b2f`, corpus `4.9.4`. The resulting
corpus `4.9.5` updates only the named normative owners and factual records. It
does not change code, tests, data, schema, migrations, dependencies, lockfiles,
the eligibility register, the dataset or the OpenAPI v1 bytes; it does not
import, render, index, activate, evaluate or perform an external action.
