# RAG-Challenge Architecture Pack

## Status and authority

This pack originated during `STATE-00 DISCOVERY` and `STATE-02 ARCHITECTURE`.
The current lifecycle position is `STATE-07 TESTING_HOMOLOGATION`; the
authoritative current disposition is maintained in
[`Current-State.md`](../../prompts/state/Current-State.md). ADR-0001 was accepted by
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
byte-for-byte unchanged. Subsequent separately authorised increments implemented
OpenAPI v2, same-origin page-image serving and the local notice-bearing profile;
that evidence remains synthetic and does not constitute product homologation.

The owner accepted ADR-0010 on 2026-08-07 and authorised its documentary
registration as corpus `4.10.0`. It assigns `S04-CORR-04-E` to the internal
persistent `AnswerEvidenceRecordV1`, fixed `P30D` retention and answer-evidence
reachability. Registration does not start that increment or authorise a
migration, executable test, public API, serving, gate or lifecycle change.
The owner separately authorised the local, offline and sequential increment on
2026-08-08; corpus `4.10.1` records its implementation and direct synthetic
verification without claiming an Automatic Quality Gate, Human Gate or
lifecycle transition.

ADR-0011 was prepared on 2026-08-09 under
`AUTH-S07-A-RIGHTS-POLICY-CORR-PREP-001` and explicitly accepted by the owner
on the same date through `ADR-0011: ACEITAR.`. It establishes an explicit,
auditable and conditional mapping from authoritative broad rights evidence to
the existing ten independent technical decisions, and defines the boundary
between same-origin derivative display and separate byte distribution or
publication. Acceptance changes no candidate disposition and grants no
implementation or product authority.

ADR-0012 was prepared on 2026-08-09 under
`AUTH-S07-A-NOTICE-BEARING-PROFILE-ADR-PREP-001` and explicitly accepted by
the owner on the same date through `ADR-0012: ACEITAR.`. It selects one
deterministic self-contained PNG mechanism that preserves the source-page
pixels in a separate region while making complete derivative obligations part
of each image and of the adjacent accessible context. It also establishes the
required schema, migration and v2 contract direction. Acceptance grants no
reconciliation, implementation, migration, rendering, candidate or product
authority.

Subsequent separately authorised increments reconciled ADR-0012, froze the
protected v2 revision, implemented its schema/migrations and implemented the
local notice-bearing behaviour in
`f682827d1a26b08fa8c450a1fadb3bd0e1fa1700`. Focused implementation evidence
does not replace the still-separate notice-bearing Automatic Quality Gate, a
new candidate-specific A0 or product-data/browser/assistive-technology
homologation.

ADR-0013 was prepared on 2026-08-10 under
`AUTH-STATE07-LLM-CANDIDATE-ADR-PREP-001` and explicitly accepted by the owner
on the same date through `ADR-0013: ACEITAR.`. It selects the dated
`gpt-5.4-mini-2026-03-17` snapshot as the sole MVP language-model candidate,
superseding only ADR-0005's earlier language-model candidate selection, and
retains `gpt-5.6-sol` only as an inactive future evaluation candidate with a
recorded mutable-identifier risk. Corpus `4.10.19` applies the separately
authorised semantic reconciliation to ADR-0005 and the `STATE-02` architecture
report. Reconciliation grants no adapter change, provider access, paid
evaluation, real-corpus processing, OCI, deployment or product authority.

ADR-0014 was prepared on 2026-08-11 under
`AUTH-STATE07-RETRIEVAL-DETERMINISM-ADR-PREP-001` and explicitly accepted by
the owner on the same date through `ADR-0014: ACEITAR.` on clean
`main@52e1ac7d9bc61be196549a8ee61399fde477b8fb`, corpus `4.10.26`. It records
the existing `Score DESC, global ChunkOrdinal ASC` total ranking, defines the
typed fail-closed Application retrieval boundary and establishes the governed
retrieval-only evaluation design. `retrieval-v1` remains unchanged for valid
inputs and `retrieval-multi-query-v1-candidate` remains parked. Corpus
`4.10.27` records this acceptance only as architecture authority; it grants no
implementation, executable test, dataset, campaign, provider, network,
OpenAPI, schema, migration, gate or lifecycle authority.

ADR-0015 was prepared on 2026-08-11 under
`AUTH-DR3-NUMERIC-SEMANTICS-PROPOSAL-001` after the failed DR-3 gate exposed
an admissible cosine boundary result outside `[-1, 1]`. The owner explicitly
accepted it on the same date through `ADR-0015: ACEITAR.` on clean
`main@46de807148d5b547f56a0f7265b32428b232100f`, corpus `4.10.30`. It selects
`cosine-f32mul-f64acc-boundary-canonical-v1`, `retrieval-v2` and the `/2`
vector-store descriptor while preserving the exact one-ULP corridor and
scaled-binary64 alternatives as non-selected traceability. It also defines the
verifiable correction plan for `DR3-FIND-001` to `DR3-FIND-004`. Corpus
`4.10.31` records architecture authority only; implementation, generation,
tests, gate and lifecycle remain separate.

ADR-0016 was prepared on 2026-08-14 by the Stage 1 governance and multi-agent
readiness audit and accepted explicitly by the owner through
`ADR-0016: ACEITAR.`. It selects a development-only deterministic
TypeScript/Node 24 coordinator and a narrow `AgentRunner` boundary with fake
and Codex SDK adapters. The subsequent governance and architecture conditions
passed. After the owner separately authorised bounded npm-registry acquisition,
the exact SDK graph was locked and Stage 2 implemented and validated the
development orchestrator. Only `FakeAgentRunner` is operational locally. The
persisted-thread resume path is mapped and contract-tested, but it is not
exposed by the CLI, was not exercised against Codex and still requires
separate execution, provider, network and credential authority. The locked SDK
does not expose a new thread ID before its first turn, so starting a new real
Codex thread remains `ARCHITECTURE_CHANGE_REQUIRED`; authentication, real
agent execution and lifecycle remain separately governed.

The later combined audit failed on `AQG-S02-001`, an internal contradiction
between observation-inclusive generation identity and observation-only
freshness rebinding. The owner accepted ADR-0007 explicitly on 2026-08-02. It
now supersedes the conflicting generation-identity and exact-record rollback
clauses of ADR-0002. Corpus `4.9.1` applies the traced semantic reconciliation;
the separately authorised renewed audit on
`main@3978a17201cf5f6ac4ddc189862736fc3646457b` approved the Automatic
Quality Gate, disposed `AQG-S02-001` to `AQG-S02-003` as `RESOLVED` and found
no new classified finding. That paragraph records the gate's then-current
boundary; the later `STATE-02` Human Gate and lifecycle transition are
recorded in Current State and the append-only transition log.

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
  corrective content/render/activation, v2 serving and notice-bearing
  increments implemented locally; product homologation remains separate)
- [ADR-0009 — Document, Evidence and Query Language Taxonomy](ADR-0009-Document-Evidence-And-Query-Language-Taxonomy.md)
  (`accepted`; semantic reconciliation applied in corpus `4.9.5`;
  internal language split implemented; broader public v2 remains separately
  authorised)
- [ADR-0010 — Persistent Answer-Evidence Records and Bounded Retention](ADR-0010-Persistent-Answer-Evidence-Records-And-Bounded-Retention.md)
  (`accepted`; documentary reconciliation applied in corpus `4.10.0`;
  `S04-CORR-04-E` implemented locally under separate authority and reconciled
  in corpus `4.10.1`; formal gate remains separate)
- [ADR-0011 — Source Rights Evidence Mapping and Same-Origin Derivative Display Boundary](ADR-0011-Source-Rights-Evidence-Mapping-And-Same-Origin-Derivative-Display-Boundary.md)
  (`accepted`; serving-policy correction implemented; no PostgreSQL rights
  reclassification or product authority)
- [ADR-0012 — Notice-Bearing Page-Image Profile and Derivative Obligation Delivery](ADR-0012-Notice-Bearing-Page-Image-Profile-And-Derivative-Obligation-Delivery.md)
  (`accepted`; contract, schema/migrations, local behaviour, its synthetic AQG
  and A0-003 rights disposition completed; product/browser homologation
  remains separate)
- [ADR-0013 — MVP Language-Model Candidate and Deferred Frontier Evaluation](ADR-0013-MVP-Language-Model-Candidate-And-Deferred-Frontier-Evaluation.md)
  (`accepted`; semantic reconciliation, fake-handler adapter compatibility and
  its focused AQG completed; no provider access, paid evaluation or operational
  configuration authority)
- [ADR-0014 — Deterministic Retrieval Ranking and Retrieval-Only Baseline](ADR-0014-Deterministic-Retrieval-Ranking-And-Retrieval-Only-Baseline.md)
  (`accepted`; deterministic implementation and corrective gate are complete,
  RB-1 remains valid, and the mechanically intact RB-2/RB-3 freezes are
  quarantined as historical evidence and unavailable for RB-4)
- [ADR-0015 — Versioned Cosine Numerical Semantics](ADR-0015-Versioned-Cosine-Numerical-Semantics.md)
  (`accepted`; selects boundary canonicalisation with
  `cosine-f32mul-f64acc-boundary-canonical-v1`, `retrieval-v2` and a new
  compatibility generation; implementation and corrective retest were later
  completed under separate authority)
- [ADR-0016 — Deterministic Development Orchestrator and Codex Runner Boundary](ADR-0016-Deterministic-Development-Orchestrator-And-Codex-Runner-Boundary.md)
  (`accepted`; Stage 2 implementation and exact dependency validation are
  complete; a new real Codex thread still requires an architecture change)

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

- The existing RB-2/RB-3 freezes remain quarantined historical evidence and
  cannot be corrected in place or consumed by RB-4.
- A separately authorised successor review/adjudication package with two
  independent human reviews and real human adjudication, and if required a
  successor vector freeze, before any RB-4 execution can be authorised.
- A separately accepted ADR-0016 successor or a verified compatible SDK
  surface before starting a new real Codex thread. Current fake execution
  remains separately authorised; persisted-thread resume is contract evidence
  only until separately authorised and exercised.
- Product evaluation, provider, security, accessibility, operational recovery,
  load, OCI and production evidence that remains `NOT_RUN` or separately
  governed in Current State.
- The `STATE-07` Automatic and Human Gates before any transition to
  `STATE-08`.

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
