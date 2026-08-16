# MVP Roadmap and Backlog

## Objective

Deliver an independent RAG agent that is locally reproducible and deployed on
OCI, and that answers questions about all active PDF/CSV documents in the
database catalogue with explicit provenance, citations and safe refusal.

This document plans work; it does not authorise entry into a state, code,
external consumption or deployment.

## MVP definition

Included:

- one logical corpus;
- an initial catalogue of 51 databases, 9 categories and 54 associations;
- any number of PDF/CSV documents per database;
- Candidate/Active/Deactivated/Removed administration without hard-coding;
- official sources obtained from individually allowlisted HTTPS URLs;
- manual synchronisation into versioned snapshots;
- unified retrieval across all active documents, with explicit local/official
  provenance and coverage;
- immutable catalogue and versions;
- immutable, reopenable storage of raw content;
- versioned parsing and chunking;
- one embedding provider;
- one vector store;
- one LLM;
- immutable index generations;
- queries with citations and `INSUFFICIENT_EVIDENCE`;
- questions and answers in `pt-BR` and `en-GB`, with the answer in the declared
  question language and citations in their original language;
- an interface localised in `pt-BR` and `en-GB`, with an explicit selection
  independent of the query language;
- `Light` and `Dark` interface themes, selected independently from the visual
  and query languages;
- an API, versioned OpenAPI v1 artefact and minimal web interface;
- local execution;
- CI;
- OCI deployment and evidence; and
- documentation and tests.

Not included:

- formats beyond PDF and CSV;
- multiple active corpora;
- incremental or scheduled synchronisation;
- crawling, generic HTML or a user-provided URL;
- several production providers;
- corporate authentication;
- microservices; or
- executable DB-Notifier integration.

## Roadmap by state

| State | Incremental result | Exit condition |
|---|---|---|
| `STATE-00` | Discovery, requirements, proposed architecture, risks, backlog and governance. | Documentary Quality Gate and explicit Human Gate. |
| `GATE-B01` | ADR-0001, repository licence and physical project map decided. | Recorded human decision; no implementation authorised. |
| `STATE-01` | Reproducible repository and scaffold without RAG logic, following the accepted bootstrap. | A clean clone builds/tests and structural CI passes. |
| `STATE-02` | ADR-0002, providers, catalogue/documents/licences/sources, threat model and OCI decided. | Decisions accepted and critical risks addressed. |
| `STATE-03` | Database/category/document/snapshot/index model, freshness, bindings, migrations and rollback. | Generation/activation and isolation are verifiable without a production service. |
| `STATE-04` | Local functional administration/PDF/CSV ingestion, official synchronisation, RAG pipeline and API. | Unified retrieval, citations and failures pass tests. |
| `STATE-05` | Minimal interface with coverage, provenance and freshness. | Local/official PDF/CSV flows and citations validated by humans. |
| `STATE-06` | Offline E2E, artefact, non-production OCI rehearsal and factually current README with a verified local/synthetic example. | Reproducible execution without corruption, leaks, secrets or overstated evidence. |
| `STATE-07` | RAG homologation, SSRF, isolation, load, recovery and accessibility. | Predefined thresholds met and risks accepted. |
| `STATE-08` | OCI deployment, official egress, smoke check, evidence and final README. | Public delivery satisfies the official criteria. |

## Small development increments

### Batch S00-DOC — Documentary baseline

- Create the 20 originally approved files. Normative documents added later
  are versioned increments and do not rewrite that historical scope.
- Validate links, format, scope and separation of authority.
- Reconcile promotion of the online official source into the MVP.
- Produce the automatic report.
- Request a Human Gate only for `STATE-00`.

### GATE-B01 — Bootstrap decision

- Explicitly accept or reject ADR-0001.
- Select the repository licence separately from the corpus licence.
- Confirm whether each candidate assembly has sufficient responsibility or a
  dependency/test boundary, and record the approved physical map.
- Map `CH-MOD-*` to namespaces/directories/projects, permitted dependencies
  and architecture tests.
- Select one-shot mode in the main host or justify a separate administrative
  project, without yet defining identity or permissions.
- Consolidate the map, modules and administrative form in accepted ADR-0001;
  record decision/licence/evidence in an append-only State Transition Log
  entry and update Current State only as a snapshot.

Criterion: recorded human decision. This gate does not initialise Git, create
the scaffold, accept ADR-0002 or authorise `STATE-01`.

### Batch S01-A — Repository foundation

- Initialise Git only after authority is granted.
- Apply the previously selected repository licence, `.editorconfig` and
  `.gitattributes`.
- Complete `.gitignore`.
- Pin the toolchains accepted in ADR-0001.
- Create central package management and lockfiles.

Criterion: no functional logic and no secret.

### Batch S01-B — Empty boundaries

- Create the solution and projects.
- Add references in the approved direction.
- Create architecture tests.
- Add minimal hosts and health without external dependencies.

Criterion: local restore, build, format and tests pass.

### Batch S01-C — Initial CI

- Build/test/format.
- Structural coverage.
- Dashboard lint/type/test/build.
- Dependency and secret scans.
- Markdown links and diff hygiene.

Criterion: locally reproducible pipeline; CI does not deploy.

### Batch S02-A — Blocking decisions

- Accept/reject ADR-0002 and any necessary additional decisions.
- Select the corpus licence without silently reopening the repository licence.
- Freeze the initial 51/54/9 catalogue and the PDF/CSV contract without a
  numerical cap.
- Define the administrative cycle and records for official URLs,
  terms/licences, `maxAge` and per-source limits.
- Select parser, embeddings, vector store and LLM.
- Define durable persistence and retention for raw content, catalogue and
  index, including restart and OCI storage.
- If the vector store is external, define and separately authorise its egress
  and data handling; retain a local adapter as the simple alternative.
- Select the OCI service/region.

Criterion: every choice has an alternative, consequence and owner.

### Batch S02-B — Contracts and security

- Specify entities, ports, `VectorSearchRequest`, `IDocumentContentStore`,
  `CorpusActivationRecord` and outcomes.
- Detail the threat model and the separate `AI_PROVIDER_EGRESS`,
  `VECTOR_STORE_EGRESS`, `OFFICIAL_SOURCE_EGRESS` and `OCI_RUNTIME_EGRESS`
  policies.
- Define canonicalisation and per-connection DNS/IP pinning, Host/SNI and
  disabled redirects for the official source.
- Require a credential-free public source and decide trust, revocation, chain
  downloads and possible TLS material provision without unauthorised
  auxiliary egress, with evidence planned for a clean local clone and OCI.
- Select the non-public local administrative surface, its identity,
  permissions, idempotency, mandatory reason and audit.
- Define configuration, canonical error table, global/per-scope readiness,
  logging and audit.
- Define ownership, schemas, metadata and the OpenAPI v1 compatibility policy.
- Define `questionLanguage`, `answerLanguage` and `contentLanguage` with
  canonical values `pt-BR` and `en-GB`, without deciding interface language.
- Define the dataset, rubric and thresholds before execution.
- Define unified retrieval, coverage, provenance, absence of fallback and
  authority for real tests.

Criterion: implementation can begin without an open material decision.

### Batch S03-A — Catalogue model

- Model corpus, database, many-to-many category, document, version, format,
  state and provenance.
- Model source records, immutable official snapshots, revalidation observations
  and freshness.
- Model canonical final specification and manifest, non-queryable staging,
  logical-artefact digest/counts, deterministic identity of the finalised
  generation, observation-free generation-bound `sourceBindingSetDigest`, and
  separation between selected snapshot and freshness.
- Model build states and `Active`/`Retained` projections derived from the
  `CorpusActivationRecord` and its complete history.
- Model `activationBindingSetDigest` for the complete binding with observation
  and a separate observation journal/revision from `catalogueRevision`.
- Publish canonical vectors for both digests, with unambiguous
  null/order/versioning semantics, and prove the three validations before
  compare-and-swap.
- Define constraints, indexes, UTC and concurrency.

Criterion: the model contains no secret or SDK/provider type.

### Batch S03-B — Persistence and rollback

- Create non-production migrations.
- Test create/upgrade/failure/rollback.
- Prove atomic compare-and-swap of the complete generation record,
  database/document/snapshot/observation bindings and audit.
- Test observation append, both digest calculations, audit and conflict at
  every boundary; retry is idempotent and does not select the “latest
  observation”.
- Rollback creates a new revision for a retained and validated generation with
  explicitly selected, compatible and currently eligible observations; it
  never restores a historical record/freshness byte for byte.
- Preserve and reopen reachable content-addressed bytes; clean only proven
  orphans after retention.
- Preserve the active generation and at least one prior validated generation
  until explicit cleanup after the approved rollback window.

Criterion: failure preserves the previous generation, and the activation →
previous-generation return is tested.

### Batch S04-A — Administration, ingestion and official synchronisation

- Administer databases, categories, documents, versions and states with audit.
- Validate local PDF/CSV and root.
- Validate every allowlisted URL and manually synchronise official PDF/CSV into
  a snapshot.
- Persist and reopen local/official bytes by hash before activation and record
  the status and sent/received HTTP validators in every observation.
- On `304` or an identical hash, record a new revalidation observation without
  creating a snapshot or index only when it names the same immutable record and
  active-manifest snapshot; create a complete new revision and
  `activationBindingSetDigest`, preserve the generation-bound fields defined
  by ADR-0007, and reject mismatch before compare-and-swap.
- Extract PDF and CSV through dedicated adapters.
- Normalise and produce chunks deterministically.
- Persist raw bytes, metadata and hashes idempotently.

Criterion: local/HTTP PDF/CSV fixtures produce traceable chunks; a
synchronisation failure preserves active snapshots, documents and generation.

### Batch S04-B — Indexing

- Integrate the embedding provider.
- Build inactive staging by `candidateBuildId`.
- Include database, document, format, origin and trust in identity, digest and
  vector metadata.
- Require `CorpusId` and `IndexGenerationId` in the search contract and prove
  hard pre-filtering of those IDs, eligible generation-bound bindings derived
  from the resolved record, and optional administrative filters.
- Finalise digest/counts/readback, derive `IndexGenerationId`, validate the
  final manifest and activate.
- Rerun idempotently without promoting a partial candidate.

Criterion: identical content creates no inconsistency; failure does not
replace the active generation.

### Batch S04-C — Retrieval and answer

- Validate the question.
- Validate `questionLanguage=pt-BR|en-GB` before any provider and require the
  generation to use that same language in the answer.
- Resolve one record revision, evaluate its observations and apply eligible
  generation-bound bindings and authorised filters before top-k.
- Retrieve evidence across the active set and expose coverage/provenance.
- Generate a constrained answer.
- Validate citations.
- Return insufficient evidence.

Criterion: tests cover databases/documents/formats/origins, all four pairs of
question and evidence language, preservation of original citation language,
no answer, stale, unavailable, source leakage, provider down and injection.

### Batch S04-D — API

- Implement `/api/v1/questions`.
- Reject URL/domain/adapter and public catalogue-authority fields.
- Require `questionLanguage=pt-BR|en-GB`, return matching `answerLanguage` and
  expose citation `contentLanguage`.
- Implement liveness/readiness.
- Map the canonical taxonomy to `CH_*` codes and Problem Details.
- Generate and version the OpenAPI v1 artefact with query, answer, citation and
  failure schemas.
- Include non-secret policy, prompt and model metadata and run a
  compatibility/breaking-change test.
- Apply limits, timeout, cancellation and rate limiting.

Criterion: the API exposes no secret, stack trace or inappropriate content.

### Batch S05-A — Minimal interface

- Question form.
- Explicit `interfaceLanguage=pt-BR|en-GB` selector, independent of
  `questionLanguage`.
- Localise labels, instructions, validation, loading, empty, error, stale,
  unavailable, rate limit and no-evidence states in both languages.
- Explicit `Light`/`Dark` theme selector that does not alter language,
  question, answer, evidence or citations.
- Apply accessible background, surface, text, border, focus and state tokens
  in both themes without communicating information through colour alone.
- Active-coverage and local/official-provenance indicator, with per-source or
  per-document degradation and no silent query split.
- Answer and citation list.
- URL/snapshot/freshness in official citations.
- Loading, empty, error, stale, unavailable and no-evidence states.
- Plain text by default; any Markdown uses a sanitised subset without raw HTML,
  with permitted URL schemes and CSP.

Criterion: the flow works by keyboard and on a narrow viewport in `pt-BR` and
`en-GB`; tests run the four `interfaceLanguage`/`questionLanguage` pairs in
`Light` and `Dark`, totalling eight combinations without mixed product text,
lost contrast or translated citations.

### Batch S06-A — E2E and artefact

- Run document → index → question → answer.
- Run synchronisation through a fake HTTP server; real smoke only when
  authorised.
- Validate restart/persistence of raw content, catalogue, activation and index.
- Produce a reproducible artefact.
- Prepare environment configuration without a secret.

Criterion: a clean clone reproduces the documented path.

### Batch S06-CORR-01 — Automatic Quality Gate correction

- Produce the readiness plan and non-production static Linux ARM64 rehearsal
  without OCI contact.
- Prove in the composed host that cancellation and bounded failures preserve
  the active generation for query and restart.
- Keep the README factually current with at least one command and result
  verified in the local/synthetic integrated artefact, identifying that
  boundary without claiming a real corpus, provider, official source, Linux,
  OCI or production.

Criterion: all three `STATE-06` findings have local corrective evidence and
remain pending disposition by a new Automatic Quality Gate; public README
finalisation remains owned by `S08-B`.

### Batch S07-A — Evaluation and security

- Run the frozen dataset.
- Run the `pt-BR→pt-BR`, `en-GB→en-GB`, `pt-BR→en-GB` and `en-GB→pt-BR`
  matrix between question and evidence languages, verifying an answer in the
  question language and an untranslated citation.
- Measure retrieval, groundedness, citations, latency and cost.
- Test prompt injection, abuse, rate limiting and failures.
- Test SSRF, DNS rebinding, mixed responses, IP/Host/SNI pinning, redirect,
  path, media type, decompressed bytes, rejected authentication, absence of
  AIA/CRL/OCSP egress, stale and isolation.
- Test crash at every observation-append, digest, audit, activation,
  new-record rollback and accessibility boundary.

Criterion: predefined thresholds and no residual P0/P1.

### Batch S08-A — OCI deployment

- Authorise target and costs.
- Restrict `OFFICIAL_SOURCE_EGRESS` to the exact official URL and compose
  `OCI_RUNTIME_EGRESS` only from separately authorised destinations.
- Keep `VECTOR_STORE_EGRESS` empty for the local adapter or validate its
  specific allowlist when a managed service is used.
- Provision/configure the secret.
- Publish the artefact.
- Run smoke and health checks.
- Rehearse recovery.

Criterion: functional, identifiable public application.

### Batch S08-B — Evidence and delivery

- Record a sanitised link/screenshot.
- Finalise the README by supplementing or replacing local/synthetic examples
  with commands and evidence separately verified on OCI and in real product
  execution.
- Check history, licence and versioned material.
- Submit the GitHub URL under the Challenge rules.

Criterion: complete official checklist.

## Prioritised backlog

### Must — mandatory for the MVP

| ID | Item | Owning state |
|---|---|---|
| `BL-M01` | Preserve the initial 51/54/9 catalogue and verify licence/provenance of PDF/CSV documents. | S02–S04 |
| `BL-M02` | Modular .NET 10 scaffold and CI. | S01 |
| `BL-M03` | Versioned catalogue, reopenable raw content, document, manifest and index. | S03 |
| `BL-M04` | Secure local and official PDF/CSV ingestion. | S04 |
| `BL-M05` | Embeddings and immutable index generation. | S04 |
| `BL-M06` | Retrieval, grounded answer and citations. | S04 |
| `BL-M07` | Insufficient-evidence outcome. | S04 |
| `BL-M08` | API with limits, health, sanitised errors and a versioned/tested OpenAPI v1 artefact. | S04 |
| `BL-M09` | Minimal accessible web interface. | S05 |
| `BL-M10` | RAG tests and evaluation. | S04/S07 |
| `BL-M11` | Reproducible local execution. | S06 |
| `BL-M12` | OCI deployment and evidence. | S08 |
| `BL-M13` | Final public README with examples supported by evidence separately verified on OCI and in real product execution. | S08 |
| `BL-M14` | Preserve one eligible prior generation; test the separate digests, observation rebinding and atomic activation/rollback through a new `CorpusActivationRecord` in compare-and-swap, without historical freshness replay. | S03/S04/S07 |
| `BL-M15` | Synchronise allowlisted official PDF/CSV records with DNS/IP pinning, snapshot, freshness and explicit coverage. | S02–S08 |
| `BL-M16` | Support and homologate questions/answers in `pt-BR` and `en-GB`, including cross-language retrieval and preservation of original citation language. | S02/S04/S07 |
| `BL-M17` | Localise the interface in `pt-BR` and `en-GB` with an explicit selector, query independence and accessibility tests in both languages. | S05/S07 |
| `BL-M18` | Implement and homologate `Light` and `Dark` themes with an explicit selector, language independence and a visual/accessibility matrix in both themes. | S05/S07 |
| `BL-M19` | Administer databases, categories and any number of documents through Candidate/Active/Deactivated/Removed records, without hard-coding or an ADR per compatible item. | S03/S04/S07 |

### Should — when delivery is not compromised

| ID | Item | Note |
|---|---|---|
| `BL-S02` | Secure embedding cache by hash. | Only after measuring benefit. |
| `BL-S03` | Cost/token metrics. | When the provider exposes safe data. |
| `BL-S04` | Corpus diagnostic interface. | Read-only and sanitised. |
| `BL-S05` | Deployment-readiness map and local preflight, keeping decided, implemented, locally verified, OCI-verified and deployed distinct. | Prepare in S06; real OCI and deployment evidence belongs to S08. |

### Could — evolution

| ID | Item |
|---|---|
| `BL-C01` | Markdown, HTML, JSON and Office formats beyond PDF/CSV (`RF-018`). |
| `BL-C02` | Multiple corpora and individual activation. |
| `BL-C03` | Incremental synchronisation and scheduler. |
| `BL-C04` | HTML/crawling, source authentication and scheduled synchronisation. |
| `BL-C05` | More embedding, vector and LLM providers. |
| `BL-C06` | RBAC and per-corpus scope (`RF-019`). |
| `BL-C07` | Optional static frontend on GitHub Pages. |
| `BL-C08` | Consuming adapter owned by DB-Notifier under the consumer repository's ADR and gates; RAG-Challenge supplies only versioned OpenAPI. |

### Won't — not in this RAG-Challenge

| ID | Item |
|---|---|
| `BL-W01` | Literal coverage of every known database. |
| `BL-W02` | Microservices and distributed orchestration without measured need. |
| `BL-W03` | SQL execution or database administration by the agent. |
| `BL-W04` | Unrestricted web browsing during questions. |
| `BL-W05` | Direct dependency on the DB-Notifier repository. |

## Roadmap risks

- The corpus and its licence are the first material blocker.
- Every official source, its terms/licence and stability is a separate blocker.
- Egress/SSRF and freshness require tests without making the standard suite
  depend on the internet.
- An external provider may require an account, quota, region and cost.
- Cross-language retrieval and generation in `pt-BR`/`en-GB` may reject
  candidate providers and require an alternative without reducing the
  approval requirement.
- Incomplete or mixed `pt-BR` and `en-GB` text may degrade accessibility and
  comprehension; the UI matrix must block homologation.
- Incomplete tokens or inadequate contrast in `Light` or `Dark` may hide
  focus, states and provenance; the theme matrix must block homologation.
- Late selection of vector dimension/store may force reindexing.
- A managed vector store may expose chunks/embeddings without its own policy.
- GitHub Pages may be mistaken for the backend; documentation must preserve
  the distinction.
- Formats beyond PDF/CSV threaten the schedule and require a compatible
  adapter/decision.
- The uncapped catalogue may exceed candidate vector-store capacity; failure
  blocks activation and does not silently reduce databases or documents.
- Evaluation without a frozen dataset may produce irreproducible success.

## Progression rule

Completing one batch does not authorise the next. Every state requires the
gates described in
[`Quality-Gates.md`](../prompts/governance/Quality-Gates.md), and every external
action retains its own authority.
