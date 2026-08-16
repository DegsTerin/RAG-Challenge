# RAG-Challenge Project Vision

## Context

The Alura/ONE Challenge materials describe a company whose documents are
difficult to query and that needs an agent able to answer natural-language
questions. The minimum delivery requires organised code in a public
repository, a functional document-grounded agent, a README and deployment
evidence using at least one OCI service.

The product specialises in database documentation. The original BimBam Buy,
Santo Pegasus and Mercado Central 24h examples remain local references and do
not constitute the product corpus.

## Problem

Professionals need to find differences, characteristics and guidance in
extensive database documentation quickly. Manual searches take time, and
model answers without evidence can invent information.

## Value proposition

Provide a simple question-and-answer experience that:

- searches a controlled corpus;
- retrieves relevant passages;
- generates an evidence-bounded answer;
- accepts questions in `pt-BR` and `en-GB` and answers in the declared question
  language;
- presents traceable citations;
- preserves the original language of referenced source content in citations;
  and
- declares when evidence is insufficient.

## Classification

| Dimension | Initial classification |
|---|---|
| Phase | Discovery for an MVP |
| Size | Small, with evolvable product architecture |
| Criticality | Moderate |
| Exposure | Local during development; public in the RAG-Challenge deployment |
| Data | Publishable corpus and untrusted user questions |
| Availability | Best effort in the MVP |
| Candidate architecture | Modular monolith with API and web interface |
| Delivery model | Independent application deployable on OCI |

## Users and stakeholders

- the participant who develops and maintains RAG-Challenge;
- the evaluator who installs, queries and verifies the delivery;
- a person interested in database documentation; and
- a future DB-Notifier maintainer, only when separate integration is
  authorised.

The project owner sets priorities, accepts Human Gates, selects the licence and
authorises external actions.

## Objectives

- Deliver a demonstrably functional local and online MVP.
- Produce grounded answers with sources and safe refusal.
- Support questions and answers in `pt-BR` and `en-GB`, answering in the
  declared question language without translating cited content.
- Keep the domain loosely coupled from AI providers.
- Allow embeddings, vector database and model to be replaced without rewriting
  use cases.
- Allow databases and document versions to be administered, candidates to be
  built and a coherent set to be activated without interrupting the current
  generation.
- Deliver unified query over authorised local PDF/CSV documents and controlled
  official sources without unrestricted browsing.
- Prepare, without implementing now, multiple corpora and scheduled
  incremental updates.
- Preserve a future DB-Notifier integration path without a direct dependency.

## MVP scope

- One configured logical corpus with an administrative database and document
  catalogue.
- An initial canonical catalogue of 51 databases and 54 associations with 9
  categories, without product hard-coding.
- Any number of authorised local or official PDF/CSV documents per database,
  provided every active database has at least one active document.
- One parser adapter per initial format: PDF and CSV.
- Manual server-side synchronisation of each approved official source into an
  immutable snapshot, with provenance, freshness and rollback.
- Unified retrieval across all active documents. Local/official origin and
  trust class remain visible in metadata and citations.
- One shared, versioned normalisation and chunking strategy.
- One embedding provider.
- One vector database or index.
- One language model.
- Query through an API and a minimal web interface.
- A versioned RAG-Challenge-owned OpenAPI v1 contract.
- Answers with citations and an explicit insufficient-evidence outcome.
- Question language explicitly declared as `pt-BR` or `en-GB`, with the answer
  in the same language and citations preserving the evidence language.
- A web interface available in `pt-BR` and `en-GB`, with an explicit selection
  independent of question language.
- `Light` and `Dark` visual themes, explicitly selected and independent of the
  interface, question, answer and evidence languages.
- Local catalogue metadata, document versions and index generations.
- Durable, content-addressed storage of the bytes required for rebuild and
  rollback.
- Local administration of databases and documents, candidate indexing and
  explicit safe activation.
- Reproducible local execution.
- Authorised OCI deployment and execution evidence.
- Tests, minimal observability and public documentation.

## Outside the MVP scope

- Promise coverage of every existing database beyond the administratively
  activated catalogue.
- Ingest Word, Excel, PowerPoint, Markdown, JSON, HTML or formats beyond PDF
  and CSV without compatible adapters and decisions.
- Public upload and remote corpus administration.
- More than one active corpus.
- Scheduled and distributed incremental synchronisation.
- Arbitrary URLs, generic crawling or direct internet query during each
  question.
- Corporate authentication, complete RBAC or multi-tenancy.
- Microservices, distributed queues or dynamic plug-in loading.
- Executable DB-Notifier integration.
- Start, stop, administration of or connection to real databases.

## Initial corpus

The logical corpus is `Catálogo de Bancos de Dados — MVP`. Its initial
revision contains 51 unique entities and 54 associations across 9 categories.
Categories are many-to-many; Redis, SAP HANA and SingleStore are unique
entities that each belong to two categories.

| Category | Canonical databases |
|---|---|
| Relational (SQL) | PostgreSQL; MySQL; MariaDB; Microsoft SQL Server; Oracle Database; SQLite; IBM Db2; SAP HANA; Firebird; Teradata; CockroachDB; YugabyteDB; SingleStore; TiDB; Amazon Aurora |
| Document (NoSQL) | MongoDB; Couchbase; CouchDB; RavenDB; Amazon DocumentDB; Azure Cosmos DB |
| Key-value | Redis; Valkey; Amazon DynamoDB; Riak KV; Aerospike |
| Wide-column | Apache Cassandra; ScyllaDB; Apache HBase; Google Bigtable |
| Graph | Neo4j; Amazon Neptune; TigerGraph; JanusGraph; ArangoDB |
| Search | Elasticsearch; OpenSearch; Apache Solr |
| Time series | InfluxDB; TimescaleDB; QuestDB; VictoriaMetrics |
| Data Warehouse / Analytics | Snowflake; Google BigQuery; Databricks SQL; Amazon Redshift; ClickHouse; Vertica; DuckDB; Apache Doris; StarRocks |
| In-memory | Redis; SAP HANA; SingleStore |

The list is initial canonical data, not an enum, constant or hard-coded
condition. The administrator can add compatible databases and documents
without a code change or per-item ADR. Every addition records provenance,
licence, language, an allowlisted source/URL when external, immutable snapshot,
hash, adapter, validation, candidate indexing and activation. A new format,
protocol, authentication or trust class may require implementation and its own
architectural decision.

There is no product ceiling for databases, documents or pages. Every version
is finite, records its observed counts and must fit safely in the homologated
environment. File, row, page, memory, time and concurrency limits are
operational controls, not catalogue limits.

## Functional requirements

| ID | Requirement | MVP |
|---|---|---|
| `RF-001` | Load authorised PDF/CSV documents without depending on `reference-materials/`. | Yes |
| `RF-002` | Validate document type, size, identity and integrity before processing. | Yes |
| `RF-003` | Extract PDF and CSV content and produce chunks with format-specific location and origin metadata. | Yes |
| `RF-004` | Generate embeddings and build an identifiable index generation. | Yes |
| `RF-005` | Query the index with a natural-language question. | Yes |
| `RF-006` | Generate an answer only from retrieved passages. | Yes |
| `RF-007` | Return citations with document, version and available location. | Yes |
| `RF-008` | Return `INSUFFICIENT_EVIDENCE` when retrieval does not support an answer. | Yes |
| `RF-009` | Manually version a document and build a candidate without first destroying the active version. | Yes |
| `RF-010` | Expose liveness, readiness and sanitised dependency diagnostics. | Yes |
| `RF-011` | Run locally through a documented procedure. | Yes |
| `RF-012` | Run on OCI and produce verifiable evidence. | Yes |
| `RF-013` | Add, remove, version, activate and deactivate multiple corpora. | Future |
| `RF-014` | Synchronise changes incrementally and on a schedule per document. | Future |
| `RF-015` | Replace embeddings, vector storage and LLM through configuration/composition. | Prepared; one MVP implementation |
| `RF-016` | Manually synchronise every registered official source through a compatible adapter, allowlist and versioned snapshot, preserving URL and freshness. | Yes |
| `RF-017` | Publish the versioned RAG-Challenge HTTP/OpenAPI contract in the MVP; any consuming adapter, including DB-Notifier, belongs to the consuming repository and its own future gates. | Contract in MVP; consuming adapters in future |
| `RF-018` | Process PDF and CSV through dedicated adapters without changing core use cases; additional formats remain future capabilities. | Yes for PDF/CSV |
| `RF-019` | Apply RBAC and per-corpus scope before retrieval. | Future |
| `RF-020` | Retrieve from all active documents by default, record local/official provenance for each item of evidence and never silently replace an unavailable source with another. | Yes |
| `RF-021` | Accept questions declared as `pt-BR` or `en-GB`, answer in the same language and preserve all source-derived citation content in its original language. | Yes |
| `RF-022` | Allow selection of `pt-BR` or `en-GB` for the interface and localise all product-owned visual text without changing `questionLanguage`, `answerLanguage` or cited content. | Yes |
| `RF-023` | Allow selection of the `Light` or `Dark` visual theme without changing `interfaceLanguage`, `questionLanguage`, `answerLanguage`, evidence or citations. | Yes |
| `RF-024` | Allow an administrator to add, version, activate, deactivate and logically remove catalogue databases, with Candidate state before activation. | Yes |
| `RF-025` | Allow any number of documents per database and administer their versions/states; every active database requires at least one active document and all active documents participate in retrieval. | Yes |

## Non-functional requirements

| ID | Requirement |
|---|---|
| `RNF-001` | The core does not depend on an AI SDK, parser, vector implementation, UI, transport or DB-Notifier. |
| `RNF-002` | Configuration is typed, validated at start-up and fail-closed. |
| `RNF-003` | Secrets do not enter the repository, logs, responses or evidence. |
| `RNF-004` | External operations have timeouts, cancellation and size/cost limits. |
| `RNF-005` | Document, raw content, snapshot, chunk, provider and index have traceable provenance, identity and version. Generation identity covers source binding without observation; each activation-record revision separately covers the complete binding with `sourceObservationId`. Immutable bytes remain reopenable while required for rebuild or rollback. |
| `RNF-006` | Logs are structured, sanitised and correlatable. |
| `RNF-007` | The product distinguishes unavailability, invalid content and insufficient evidence. |
| `RNF-008` | Tests are deterministic and do not require paid services in the standard suite. |
| `RNF-009` | The minimal interface supports keyboard use, contrast and loading, empty and error states. |
| `RNF-010` | Build, dependencies and toolchains are reproducible and versioned. |
| `RNF-011` | The public clone does not depend on ignored files or private data. |
| `RNF-012` | Document or provider changes do not require core refactoring. |
| `RNF-013` | The public repository has an understandable structure and incremental commit history. |
| `RNF-014` | Official-source egress fails closed, keeps scopes distinguishable and applies HTTPS, an allowlist, limits, connection pinning to authorised DNS/IP, blocked redirects, TLS validation without lateral destinations and SSRF protection. |
| `RNF-015` | Contracts, retrieval and generation handle `pt-BR` and `en-GB` through explicit BCP 47 tags; homologation covers questions and evidence in the same language and both cross-language directions. |
| `RNF-016` | The interface does not mix languages in product-owned text, preserves accessibility in both localisations and keeps visual language independent of query language. |
| `RNF-017` | `Light` and `Dark` themes preserve contrast, visible focus, semantics, reflow and every interface state without communicating information through colour alone. |
| `RNF-018` | Compatible databases, categories, documents and sources are administrable records, not hard-coded lists; per-item addition requires neither code nor an ADR, but a new integration class may require both. |

## MVP acceptance criteria

| ID | Criterion |
|---|---|
| `AC-MVP-001` | A clean clone can be configured, built, tested and run through the published procedure. |
| `AC-MVP-002` | Every authorised document is persisted/reopened by hash, processed and incorporated into a validated candidate; partial staging remains non-queryable. |
| `AC-MVP-003` | Approved representative questions retrieve correct citations. |
| `AC-MVP-004` | Questions outside the corpus receive no invented factual answer. |
| `AC-MVP-005` | A database or document change creates a validated candidate and atomically activates the complete manifest with every applicable document binding. Activation validates `activeDocumentSetDigest`, the generation-bound `sourceBindingSetDigest` and complete `activationBindingSetDigest`; it preserves an eligible prior generation and tests rollback through a new record without historical freshness replay. |
| `AC-MVP-006` | No secret or ignored local material enters the repository. |
| `AC-MVP-007` | Automatic checks applicable to the state pass. |
| `AC-MVP-008` | The application runs on OCI with a link or sanitised visual evidence. |
| `AC-MVP-009` | The README contains architecture, technologies, operation and real examples verified after implementation. |
| `AC-MVP-010` | The minimal interface supports asking, viewing citations and understanding loading, empty, error, unavailable and insufficient-evidence states with suitable keyboard access, focus and contrast. |
| `AC-MVP-011` | The API exposes versioned query, health and OpenAPI v1 artefact with fail-closed configuration, limits, cancellation, reproducible metadata, canonical errors and sanitised diagnostics; contract compatibility is tested. |
| `AC-MVP-012` | Architecture and contract tests demonstrate that Domain/Application do not depend on SDKs or concrete adapters and that providers are composed at the boundaries. |
| `AC-MVP-013` | The public repository has an understandable structure and incremental commit history without secrets or ignored local materials. |
| `AC-MVP-014` | Authorised synchronisation of every allowlisted source produces versioned snapshot/observation records. A content change requires a candidate; `304`/identical hash for the same record/snapshot creates a complete new record revision and `activationBindingSetDigest`, while preserving manifest, `sourceBindingSetDigest`, `generationSpecDigest`, `IndexGenerationId`, `catalogueRevision` and `generationActivatedAt`. A mismatch fails closed; citations expose source, public URL when applicable, snapshot and freshness. |
| `AC-MVP-015` | Every external source rejects a domain, IP, port, path, query, mixed DNS response, redirect or lateral TLS destination outside its policy. Query performs no fetch and considers only active/current documents, exposing degraded coverage without silent fallback. |
| `AC-MVP-016` | Questions declared as `pt-BR` receive `pt-BR` answers, questions declared as `en-GB` receive `en-GB` answers, and citations do not translate source-derived titles, sections, passages or other content. Deterministic tests cover `pt-BR→pt-BR`, `en-GB→en-GB`, `pt-BR→en-GB` and `en-GB→pt-BR` between question and evidence languages. |
| `AC-MVP-017` | A person can explicitly switch the interface between `pt-BR` and `en-GB`; product-owned labels, instructions, validation and states use the selected visual language throughout. Component and flow tests cover each interface language with each `questionLanguage` without translating citations. |
| `AC-MVP-018` | A person can explicitly switch the interface between `Light` and `Dark`; query content, language and context remain unchanged. Component, accessibility and flow tests run the four `interfaceLanguage`/`questionLanguage` combinations in both themes, totalling eight combinations, and validate contrast, focus, states and absence of colour-only information. |
| `AC-MVP-019` | The initial catalogue contains exactly 51 entities and 54 associations in the 9 approved categories, preserving Redis, SAP HANA and SingleStore as unique multi-class entities. |
| `AC-MVP-020` | A new database/document starts as Candidate; only validation and explicit activation permit query. Deactivation preserves history; removal is logical; the final active document can be removed only by an operation that also explicitly deactivates the database. |

## Assumptions

- The first interface will be simple; the RAG flow is the main value.
- Question access may be anonymous in the MVP, following the Challenge
  materials.
- Administrative ingestion operations will not be exposed anonymously.
- Official synchronisation is manual and administrative; a public question
  neither starts crawling nor selects a URL.
- Initial official sources are publicly accessible without authentication;
  URL, headers and query carry no token, signature or credential.
- One OCI hosting service is sufficient for the minimum requirement when
  execution is real and documented.
- Technologies suggested by the course are optional.
- The interface supports `pt-BR` and `en-GB` under a decision separate from
  bilingual query support; initial selection, persistence and fallback remain
  their own future details.
- The interface supports `Light` and `Dark` under its own decision; initial
  theme, system preference, persistence and fallback remain future frontend
  details.

## Limitations and pending evidence

- The repository MIT licence was accepted and materialised. PostgreSQL 18.4
  `LocalAuthorised` became the first materialised and activated product
  document under recorded rights, provenance and language evidence. Every
  additional document still requires its own verifiable disposition.
- PostgreSQL 18 is the first verified official source. Every additional record
  still requires its own canonical URL, terms/licence, `maxAge`, limits,
  network authority and activation.
- ADR-0005 conditionally accepted OpenAI for embeddings and LLM,
  `SqliteExactVectorStore`, EF Core SQLite and a content-addressed filesystem.
  Adapters and packages were implemented under governed versions, but the
  scored LLM campaign, real bilingual quality, observed query cost,
  representative performance and operational recovery remain separate.
- ADR-0006 accepted the four deny-by-default egress policies and the disclosure
  boundary. That decision alone does not enable egress, a provider, an account
  or a destination; later executions belong to the authorities and evidence
  recorded in Current State.
- ADR-0005 conditionally accepted the OCI target in `sa-saopaulo-1`; tenancy
  capacity, entitlement, IAM, billing and backup/restore consistency remain
  unverified.
- RB-1 froze the design and thresholds. RB-2/RB-3 artefacts were materialised,
  but governance reaudit found a human-adjudication conflict; they remain
  unavailable to a scored campaign pending owner disposition and a possible
  coherent successor.
- ADR-0007 was accepted, its semantics were reconciled in documentary
  contracts and it was implemented under later authority. The combined audit
  disposed `AQG-S02-001` through `AQG-S02-003` as `RESOLVIDOS`; earlier failed
  results remain historical.

These items distinguish accepted architectural decisions from evidence,
implementation and external authority that remain absent.
