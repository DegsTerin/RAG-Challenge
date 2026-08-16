# Development Lifecycle

## General rule

Every state requires inputs, deliverables, an automatic audit, a human decision
and a hand-off. Corrections belong to the state that owns the defect. Evidence
formats are in [`../templates/Templates.md`](../templates/Templates.md).

## STATE-00 DISCOVERY

Objective: understand the Challenge, delimit the problem and MVP, inventory
sources, propose the architecture and prepare setup.

Inputs:

- owner requirements;
- original local materials;
- read-only DB-Notifier analysis.

Deliverables:

- vision, scope, requirements and criteria;
- risks, assumptions and pending decisions;
- high-level architecture and RAG module;
- proposed ADRs;
- governance, gates and memory;
- roadmap and backlog;
- automatic state report.

Acceptance:

- official requirements and interpretations are traceable;
- the MVP and future evolution are separate;
- ignored materials are not a product dependency;
- proposals are not presented as implementation;
- links and documentary structure are valid;
- the Human Gate is explicit and separate.

## STATE-01 PROJECT_SETUP

Objective: prepare the repository, solution, conventions, builds, tests,
secure configuration and CI without implementing RAG.

Entry preconditions:

- the `STATE-00` Human Gate is complete;
- `GATE-B01` is complete, with ADR-0001 accepted, the repository licence
  selected and the physical project map recorded;
- entry into `STATE-01` is explicitly authorised by a separate decision;
- Git initialisation, scaffolding and dependencies were not executed before
  that authority.

Deliverables:

- authorised Git and initial branch;
- complete `.editorconfig`, `.gitattributes` and `.gitignore`;
- pinned SDK/toolchains;
- empty solution and projects at the approved boundaries;
- central dependency management and lockfiles;
- minimal hosts with health and no functional rule;
- structural tests and CI pipeline;
- onboarding documentation;
- setup report.

Acceptance:

- a clean clone restores, builds and tests;
- dependency architecture is verified;
- invalid configuration fails closed;
- no secret or private corpus is present;
- no premature ingestion, retrieval or generation exists;
- the setup Human Gate is complete.

## STATE-02 ARCHITECTURE

Objective: accept the MVP boundaries, providers, contracts, data, security,
deployment and evaluation.

Deliverables:

- accepted or rejected ADRs;
- canonical contracts and diagrams;
- bilingual query contract with `pt-BR`/`en-GB`, answer in the question
  language and citation in the original language;
- detailed threat model;
- parser, embedding, vector and LLM selection;
- corpus and corpus-licence definition;
- initial canonical catalogue, database/document administrative lifecycle,
  PDF/CSV formats and official-source records with URLs, terms/licences,
  `maxAge` and individual limits;
- durable-persistence decision for raw content, catalogue and index;
- configuration, `AI_PROVIDER_EGRESS`, `VECTOR_STORE_EGRESS`,
  `OFFICIAL_SOURCE_EGRESS` and `OCI_RUNTIME_EGRESS` policies;
- canonical vector-search, failure, readiness and OpenAPI contracts;
- SSRF protection with per-connection canonicalisation and DNS/IP pinning;
- evaluation, OCI and rollback strategy.

Acceptance:

- dependencies point towards the core;
- providers are replaceable through ports;
- local/official origin remains traceable without silently splitting unified
  retrieval;
- every official source has an allowlisted PDF/CSV URL without crawling or
  silent fallback;
- limits, costs, failures and security are addressed;
- thresholds are defined before homologation;
- question, answer and evidence language have canonical semantics without
  deciding interface language.

## STATE-03 DATA_AND_INDEX_MODELING

Objective: model databases, many-to-many categories, documents, versions,
chunks, official snapshots, freshness, manifests, generations, audit and
persistence.

Deliverables:

- model and dictionary;
- constraints, indexes and concurrency;
- non-production migrations;
- retention and recovery;
- Candidate/Active/Deactivated/Removed states, PDF/CSV format, immutable
  snapshot, revalidation observations, canonical URL, freshness and withdrawal;
- versioned canonical manifest, idempotent staging/finalisation, logical
  artefact digest/counts and deterministic finalised-generation identity;
- observation-free generation-bound `sourceBindingSetDigest`, complete-binding
  `activationBindingSetDigest` and canonical vectors for both;
- an append-only observation-journal revision separate from
  `catalogueRevision` and the internal transactional revision;
- `CorpusActivationRecord` and transactional activation/rollback algorithm by
  constructing a new revision without replaying a historical record;
- retention of reachable raw content and orphan cleanup;
- deterministic fixtures.

Acceptance:

- document and index have independent versions;
- raw content remains reopenable for authorised rebuild and rollback;
- a partial candidate is never queryable, and finalisation validates
  digest/counts/readback before activation;
- secrets do not enter the model;
- a partial generation or unbound observation is never active;
- a mismatch between observation and record/snapshot fails closed; changing
  only `sourceObservationId` changes only the activation digest/revision;
- rollback binds explicitly selected, compatible and currently eligible
  observations without reviving historical freshness;
- every active document enters the manifest; origin/trust enter identity,
  digest and citation without forming mutually exclusive corpora;
- migrations and recovery are verifiable;
- the product corpus is not confused with a documented database.

## STATE-04 BACKEND_IMPLEMENTATION

Objective: implement administration/PDF/CSV ingestion, manual official
synchronisation, indexing, unified retrieval, generation and API.

Deliverables:

- Domain and Application;
- authorised adapters;
- PDF/CSV adapters and allowlisted official-source records with governed
  snapshots;
- persistence;
- versioned API;
- validation of `questionLanguage`, generation in `answerLanguage` and
  propagation of citation `contentLanguage`;
- versioned OpenAPI v1 artefact and compatibility tests;
- configuration;
- citations and refusal;
- unit, architecture, contract and integration tests.

Acceptance:

- one corpus is processed end to end;
- providers do not leak into the core;
- failures are typed and sanitised;
- hard pre-filtering is part of the vector-store contract and precedes top-k;
- hard pre-filtering includes eligible generation-bound bindings derived from
  the single activation record resolved by the query;
- the prior generation survives a rebuild failure;
- compatible `304`/identical hash creates a complete new record revision,
  preserves manifest/generation/`catalogueRevision` and rejects mismatch before
  compare-and-swap;
- one source's failure/staleness explicitly reduces coverage without
  presenting another origin as a substitute;
- questions without evidence are refused;
- `pt-BR` and `en-GB` questions receive answers in the same language and
  citations preserve source language, including cross-language retrieval;
- the standard suite requires no paid service.

## STATE-05 FRONTEND_IMPLEMENTATION

Objective: implement the minimal web interface.

Deliverables:

- question and answer;
- interface localised in `pt-BR` and `en-GB`, with an explicit selector and
  visual state independent of `questionLanguage`;
- `Light` and `Dark` themes, with an explicit selector and state independent
  of `interfaceLanguage` and `questionLanguage`;
- coverage and provenance indicator for the sources actually queried;
- citations;
- loading, empty, error, stale/unavailable source, rate-limit and no-evidence
  states;
- responsiveness and accessibility;
- component and flow tests.

Acceptance:

- suitable keyboard use, focus, contrast and semantics;
- no authorisation logic or direct provider access in the client;
- source information does not depend on colour alone;
- degraded coverage is explicit and citations display origin,
  snapshot/freshness and PDF/CSV location;
- product-owned messages are factual and fully localised in the selected
  `interfaceLanguage`;
- `pt-BR` and `en-GB` flows preserve keyboard use, focus, semantics, reflow and
  absence of mixed languages;
- `Light` and `Dark` preserve contrast, focus, hierarchy, reflow, states and
  information that does not depend on colour alone;
- the four `interfaceLanguage`/`questionLanguage` combinations run in both
  themes;
- `interfaceLanguage` is never inferred from the bilingual query contract and
  does not translate citation content.

## STATE-06 INTEGRATION

Objective: integrate API, interface, providers and artefact in a controlled
environment.

Deliverables:

- local/sandbox E2E;
- fake server for official synchronisation and authorised opt-in real smoke
  only;
- per-environment configuration;
- resilience and cancellation;
- reproducible artefact;
- non-production OCI plan and rehearsal;
- factually current README with at least one command and result verified in the
  integrated local/synthetic artefact, with that boundary explicit.

Acceptance:

- the complete flow is reproducible;
- restart and persistence are understood;
- external errors do not corrupt the active index;
- query never fetches, uses only active bindings and exposes each item's
  provenance without mixing generations;
- no secret exists in the artefact;
- evidence is not confused with production.

## STATE-07 TESTING_HOMOLOGATION

Objective: validate RAG quality, security, performance, recovery and a
representative experience.

Deliverables:

- dataset and evaluation report;
- question/evidence language matrix for `pt-BR` and `en-GB`, with matching
  pairs and both cross-language directions;
- additional strata by exact document BCP 47 tag, without inferring `en` as
  `en-GB` or merging results;
- when implemented and eligible, rights, render-manifest, serving and
  accessibility evidence for cited-page PNGs;
- negative and prompt-injection tests;
- SSRF, DNS rebinding, mixed DNS response, IP/Host/SNI pinning, redirect, URL,
  media type, size, freshness and source leakage;
- load and limits;
- recovery/rollback;
- accessibility;
- environment/provider matrix;
- residual risks.

Acceptance:

- previously approved thresholds are met;
- answers use the declared question language and citation text preserves the
  original language in all four matrix pairs;
- every additional document language is reported separately for both supported
  question languages without replacing the four mandatory pairs;
- visual evidence, when part of the candidate, derives only from a validated
  citation, preserves an accessible text alternative and fails closed on an
  incompatible manifest, binding, rights or lifecycle;
- limitations and costs are explicit;
- no blocking vulnerability remains;
- the real official source is tested only when specific egress is authorised;
- public claims match the tested matrix;
- the Human Gate repeats critical samples.

## Architectural refinements accepted during STATE-07

ADRs 0008, 0009 and 0010 were accepted after closure of the states that owned
the original implementations. Documentary reconciliation neither reopens nor
rewrites historical evidence and does not declare a capability implemented.
Every item requires its own corrective authority. The dependency order and
current factual state are:

1. `S03-CORR-01`, complete: compatible logical/physical model for languages,
   `DocumentPageImage`, `DocumentRenderManifest` and reachability, without
   data inference;
2. corrective `STATE-04` owner, preserving v1:
   - `S04-CORR-04-A`, complete: permanent content store and verified readback;
   - `S04-CORR-04-B`, complete: rights contracts and gates;
   - `S04-CORR-04-C`, complete: deterministic rendering and
     source/PNG/manifest finalisation;
   - `S04-CORR-04-D`, complete: atomic source, rights, generation and manifest
     persistence and activation; and
   - `S04-CORR-04-E`, complete at the local/offline boundary: persistent
     `AnswerEvidenceRecordV1` contract, fixed `P30D` retention and reachability
     participation, with approved corrective Automatic Quality Gate but no
     product homologation;
3. separately versioned v2 contract and secure, accessible same-origin visual
   evidence presentation, implemented with an approved Automatic Quality Gate;
4. integration, restart, confined cold backup/restore and limits, implemented
   and verified focally in commit
   `e5dae7ee5a786417fba2c6ef0555686816b0b330`, with an Automatic Quality Gate
   approved under `AUTH-STATE07-V2-INTEGRATION-RECOVERY-AQG-RETEST-001`, no new
   finding, and `AQG-S07-V2-IR-001` `RESOLVIDO`; and
5. dataset/homologation stratified by exact document language and actually
   implemented capabilities, later and unauthorised.

These responsibilities belong to the named technical owners from `STATE-03`
through `STATE-07`, but do not promote, regress or close a state by themselves.
`STATE-07` remains active; while corresponding implementation/evidence is
absent, the claim remains absent. OpenAPI v1 preserves the closed
`pt-BR|en-GB` surface byte for byte; the v2 contract/serving are implemented
and have an approved Automatic Quality Gate; integration, restart, confined
cold backup/restore and limits are implemented and verified focally; their
Automatic Quality Gate was approved under
`AUTH-STATE07-V2-INTEGRATION-RECOVERY-AQG-RETEST-001`, with no new finding, and
`AQG-S07-V2-IR-001` is `RESOLVIDO`; product dataset and homologation remain
later, `NOT_RUN` and unauthorised.

## STATE-08 PRODUCTION_RELEASE

Objective: publish the RAG-Challenge delivery to the authorised OCI target.

Deliverables:

- identifiable release candidate;
- external configuration and secrets;
- `OFFICIAL_SOURCE_EGRESS` restricted to the exact active URL set and
  `OCI_RUNTIME_EGRESS` composed only from separately authorised destinations;
- `VECTOR_STORE_EGRESS` empty for the local adapter or restricted to the
  approved managed service;
- deployment and smoke test;
- health and observability;
- rollback;
- execution link/screenshot;
- final public README, supplementing or replacing local/synthetic examples
  with commands and evidence separately verified on OCI and in real product
  execution, and GitHub submission.

Acceptance:

- the target and action are authorised;
- the public application is functional;
- evidence is sanitised and reproducible;
- no ignored local material is required;
- rollback or recovery was rehearsed;
- official synchronisation, freshness and operational runbook are verified;
- formal Challenge criteria are met.

## Module × state matrix

| Module | S02 | S03 | S04 | S05 | S06 | S07 | S08 |
|---|---|---|---|---|---|---|---|
| Corpus Catalog | Contracts | Model | Use cases | View | Persistence | Recovery | Operation |
| Document Ingestion | Adapters | Versions | Parser | State | E2E | Security/load | Runbook |
| Indexing/Retrieval | Providers | Manifest | Pipeline | Diagnostics | Compatibility | Evals | Operation |
| Answer Generation | Policy | Evidence | LLM/citations | Answer | E2E | Groundedness | Limits |
| Query Experience | API/UX | N/A | API | Interface | Integration | A11y/load | Publication |
| Operations/Governance | Security | Audit | Health/logs | Errors | Environment | Homologation | Release |
| Official Sources | Contract/allowlist | Snapshot/freshness | Adapter/synchronisation | Selector/citation | Controlled E2E | SSRF/stale | Egress/runbook |
| External Integration Contracts | OpenAPI policy | N/A | Artefact/tests | Web client | Compatibility | Regression | Publication |

A design created before its phase does not authorise premature implementation.
