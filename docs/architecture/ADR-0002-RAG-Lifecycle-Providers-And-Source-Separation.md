# ADR-0002 — RAG Lifecycle, Provider Boundaries and Source Separation

- Status: accepted
- Date: 2026-07-29
- Accepted: 2026-08-01
- Decision authority: explicit product-owner acceptance on baseline
  `main@39e2f803bf73cb4e2b59e56a0596e2858a3aed51`, corpus `4.7.0`
- Owners: RAG-Challenge RAG / data / security architecture
- STATE-02 review: accepted independently from ADR-0004, ADR-0005 and
  ADR-0006; acceptance does not authorise implementation or lifecycle progress

## Context

The MVP uses one logical corpus with an administrator-managed catalogue. Its
initial canonical revision contains 51 unique database products, 9 categories
and 54 many-to-many assignments. Every active database must have at least one
active PDF or CSV document and may have any number of additional compatible
documents. All active, validated documents participate in unified retrieval;
local or official origin remains provenance rather than a mutually exclusive
query scope.

The catalogue must accept compatible database/document/source records without
a hard-coded product list, code change or ADR per item. A new format, protocol,
authentication or trust class may still require implementation and a separate
architectural decision. Coupling use cases directly to one source, parser,
embedding SDK, vector database, LLM or mutable index would force large
refactoring.

Implementing every future capability now would also overcomplicate the
RAG-Challenge. The design needs stable seams while the MVP keeps one concrete
implementation per seam.

External documentation introduces licensing, provenance, SSRF, prompt
injection, freshness and rate-limit concerns that are different from a local
owner-controlled source.

## Decision

The accepted decision is:

- Define typed ports for catalogue/document administration, local document
  sources, official-source synchronisation, PDF/CSV parsers, chunker,
  embeddings, vector store, language model, immutable document-content store,
  catalogue and index-generation store.
- Give each provider a stable ID, version, declared capabilities and typed
  non-secret configuration.
- Register one implementation per port through dependency injection in the
  MVP. Do not implement dynamic plug-in loading.
- Model stable `DatabaseProductId`, `DatabaseProductRevision`, many-to-many
  category assignments, `DocumentId` and immutable content-addressed
  `DocumentVersion` with `Pdf|Csv` format.
- Give database products and documents the governed lifecycle `Candidate →
  Active ↔ Deactivated`, with `Removed` as an auditable logical tombstone.
  Physical deletion follows retention and reachability policy. A database is
  active only with at least one active document; removing or deactivating its
  last active document requires explicit atomic database deactivation.
- Persist the raw bytes of local versions and official snapshots through
  `IDocumentContentStore`; vector data is derivative and cannot replace the
  source needed for restart, rebuild or rollback.
- Preserve parser, chunking, provider, model, dimensions and schema in an
  immutable index-generation manifest.
- Make the index-generation store the sole system of record for a
  `CorpusActivationRecord` that atomically binds the active generation,
  catalogue revision and ordinal set of active database/document/source,
  snapshot and freshness identities. Query-time policy evaluates each binding.
  The vector store reads and writes immutable generations only by explicit
  `IndexGenerationId` and never owns activation.
- Treat `Active` and `Retained` as projections of the current activation
  record and its retained complete revisions, not independently mutable
  generation states.
- Build a candidate generation without mutating the active generation,
  validate it and switch the complete activation record by transactional
  compare-and-swap. The same transaction retains the complete previous and new
  activation revisions and writes a sanitised audit record. Candidate content,
  observations and vectors may remain as auditable orphans after a conflict,
  but never become active implicitly.
- Retain the active generation and at least one previous validated, compatible
  generation until explicit cleanup after the approved rollback window.
- Allow the MVP to rebuild the complete index after a manual catalogue or
  document change. Per-document artefact reuse and scheduled incremental
  synchronisation remain future optimisations.
- Include `corpusId` in canonical identities and vector namespaces even though
  the MVP configures one corpus.
- Keep local and official external sources in separate adapters. Use an open
  `sourceAdapterId` and a closed trust classification:
  `LocalAuthorised` or `OfficialExternal`. Trust and origin are part of
  identity, digests, vector metadata and citations, but do not partition the
  default query.
- Synchronise any number of registered, compatible canonical HTTPS PDF/CSV
  URLs through allowlisted anonymous adapters into immutable governed
  snapshots before retrieval. Each URL/query and request contains no token,
  signature, `Authorization`, API key, client certificate or ambient
  credential. Query execution never performs network access.
- Keep snapshot content immutable. Store revalidation, freshness and source
  state as separate append-only observations, including validators sent,
  response status and ETag/Last-Modified observed; a `304` or identical
  content hash updates only the observation binding without creating a
  snapshot or index generation when the active record already references that
  compatible snapshot. Otherwise rebuild a candidate generation.
- Query all active/current document bindings by default. Do not expose user
  URLs, generic crawling or public authority-bearing catalogue fields. Report
  provenance and partial coverage explicitly rather than silently substituting
  one source for another.
- Build a coherent candidate generation containing the intended complete
  catalogue set. Serialise content mutations by corpus, validate the complete
  set and roll back the generation as a whole.
- Require `VectorSearchRequest` to carry `CorpusId` and `IndexGenerationId`.
  Optional explicit database/document filters, if later exposed, apply before
  top-k. A global search followed by post-filtering violates the port contract.
- Reactivate a previous generation only when the complete binding set and
  compatibility key match the intended target. A partial document rollback
  always creates a new candidate.
- Evaluate official freshness outside the vector index. Rollback never marks
  an old snapshot fresh without a real revalidation.
- Preserve provenance in every citation and return
  `INSUFFICIENT_EVIDENCE` when retrieved content does not support an answer.
- Resolve the complete activation record once at query start and use its
  explicit generation and ordered binding identities throughout retrieval,
  validation, coverage, response metadata and citations.
- Make RAG-Challenge the owner of a generated, versioned HTTP/OpenAPI contract;
  consumer adapters, including a future DB-Notifier adapter, belong to their
  consumer repositories and gates. Do not expose Domain entities or provider
  ports.

The minimum index manifest contains:

```text
manifestSchemaVersion
corpusId
corpusRevision
catalogueRevision
activeDocumentSetDigest
sourceBindingSetDigest
indexCompatibilityKey
generationSpecDigest
chunkCount
vectorCount
logicalArtifactDigest
```

The first seven fields form a canonical build specification whose SHA-256 is
`generationSpecDigest`. A candidate writes under a temporary
`candidateBuildId`; finalisation adds canonical logical-payload counts and
digest. `IndexGenerationId` derives from the SHA-256 content digest of the
complete, versioned canonical UTF-8 manifest with fixed property and ordinal
collection ordering. Identical specification and logical outputs reuse an
identity; different outputs cannot collide under a finalised ID. `createdAt`,
activation status and freshness observations remain outside the identity.
`STATE-03` defines idempotent staging/finalisation, readback evidence and
orphan cleanup; a partial candidate never becomes queryable.

`activeDocumentSetDigest` covers ordered database/document identities,
revisions, versions and formats. `sourceBindingSetDigest` covers ordered trust,
adapter, source-registration, snapshot and observation identities.
`IndexCompatibilityKey` is a digest over canonical, non-secret parser,
normalisation, chunker and embedding-adapter versions/configuration
descriptors; embedding provider, model revision, dimensions and vector
normalisation; and vector-store adapter, schema, distance metric, index
algorithm and parameters. Changing any input requires a new generation.

## Alternatives

### Direct LangChain or SDK calls in controllers

Rejected because HTTP, orchestration and providers would become inseparable
and hard to test or reuse.

### One mutable index updated in place

Rejected because partial failures could leave queries reading mixed versions
and remove the rollback path.

### Implement full incremental artefact reuse in the first MVP

Rejected as unnecessary complexity. Immutable full-generation rebuilds meet
the initial administration need while preserving the future diff seam. This is
an operational trade-off, not a catalogue cardinality limit.

### Query official websites live for every question

Rejected because availability, SSRF, freshness, licensing and prompt
injection would enter the latency-critical query path without a governed
snapshot.

### Implement several providers immediately

Rejected. Replaceability is proved first by contracts, fakes and architecture
tests; production adapters are added only when needed.

## Consequences

- MVP implementation remains finite while core contracts support evolution.
- Index rebuild may use more time/storage than a later incremental process.
- Version compatibility must be checked before serving a generation.
- Provider-specific scores and errors require normalisation.
- Catalogue and vector storage lifecycles are coordinated but not conflated.
- Raw-content retention is explicit and may require durable filesystem or
  object storage in OCI; the catalogue stores references rather than bytes.
- There is one activation authority, avoiding split-brain between catalogue
  metadata, freshness observations and vector-store aliases.
- Source licensing and provenance become first-class delivery criteria.
- A combined generation makes activation simple but couples rollback of the
  complete active catalogue; partial rollback requires a new candidate.
- Database/document administration is data-driven for supported integration
  classes, while new parser/authentication/trust classes remain explicit design
  changes.
- Multiple corpora can be introduced without changing existing canonical IDs,
  but management UI, RBAC and scheduling still require future work.
- A managed vector store introduces a separate data-egress review; a local
  adapter keeps that policy empty.

## Security and operations

- Treat parsed content and retrieved passages as untrusted.
- Limit per-operation file size, pages/lines, chunks, context, tokens, time and
  concurrency without creating a product ceiling on catalogue cardinality.
- Do not expose file paths, provider exceptions or secrets in citations.
- Validate citations against the retrieved evidence set.
- Apply corpus authorisation before retrieval when RBAC is introduced.
- Each official registration requires an exact HTTPS URL allowlist, bounded
  PDF/CSV response, signature/structure/media validation, redirects disabled, canonical URI and
  DNS/IP pinning per physical connection. It rejects mixed/prohibited A/AAAA
  answers, connects only to an approved endpoint and preserves Host/SNI.
- Certificate validation cannot create uncontrolled AIA, CRL or OCSP traffic.
  `STATE-02` selects trust, revocation, chain-download and material-provisioning
  policy; every auxiliary destination requires its own allowlist and authority,
  and the resulting policy must work in clean local and OCI environments.
- AI-provider egress is separately deny-by-default and requires selected
  endpoints, data-classification review, retention/training/residency review
  and owner authorisation. Official-source egress is separately deny-by-default
  and may run only in the authorised administrative sync profile.
- Managed-vector-store egress is separately deny-by-default and requires
  endpoint/TLS, data classification, residence, retention, deletion, tenant
  isolation and credential review. A local adapter has no such destination.
- Log IDs, hashes, versions, counts and timings; do not log complete content by
  default.
- A failed candidate generation leaves the complete activation record
  unchanged. Activation changes generation, catalogue revision, every binding
  and the sanitised audit record atomically in the control-plane transaction.

## Compatibility and migration

- Changing parser, chunker, embedding model/dimensions or vector schema creates
  a new generation; it never silently reuses incompatible vectors.
- A future incremental algorithm classifies Added, Changed, Removed and
  Unchanged documents using source keys and hashes.
- Each official snapshot carries immutable `sourceKey`, canonical URL,
  `snapshotId`, content hash and retrieval metadata. Separate append-only
  observations carry request/response validators, status, revalidation time,
  `maxAge`, withdrawal/deactivation state and citation freshness.
- Policy-authoritative `404`/`410` creates a `Withdrawn` observation, while an
  explicit audited administrative action creates `Deactivated`. A
  compare-and-swap updates only the observation binding for the compatible
  active generation/snapshot; transient transport/`5xx` failures do not
  replace a `Current` observation and freshness expires through `maxAge`.
- A future DB-Notifier adapter consumes query and citation contracts without
  knowing provider implementations. The adapter is owned and gated by
  DB-Notifier. The RAG-Challenge-owned v1 response includes typed outcome,
  citations, generation identity, retrieval/prompt versions, a sanitised model
  descriptor and correlation identity.
- Application failures use one canonical taxonomy. `STATE-02` maps it to
  stable `CH_*` codes, HTTP and Problem Details; adapters cannot introduce
  competing names for the same condition.
- Compatible database/document/source records add data and tests without
  changing Domain semantics. Formats beyond PDF/CSV or new integration classes
  add adapters, capability declarations and potentially a new ADR.

## Acceptance checks

- Domain/Application contain no provider SDK or filesystem/network code.
- The same use case runs against deterministic test doubles.
- Raw PDF/CSV local and official content can be reopened by hash after restart while
  referenced by the active or retained rollback generation.
- Every candidate reopens and verifies each referenced content object before
  activation.
- Candidate staging remains unqueryable until final counts, canonical logical
  artifact digest and adapter-supported readback/sentinel checks pass.
- Generation activation uses compare-and-swap of the complete
  `CorpusActivationRecord`; failed builds, audit failure and concurrency
  conflicts preserve generation and all document/source bindings. The
  complete preceding activation revision remains a rollback target.
- Activation and return to the retained previous generation are tested,
  including crash before, during and after every persistence boundary.
- A configuration mismatch returns a typed unavailable/incompatible result.
- A query without supporting evidence returns `INSUFFICIENT_EVIDENCE`.
- Citations contain corpus, database, generation, document, version, format,
  trust and location identity; official citations also contain canonical URL,
  snapshot and freshness.
- Retrieval proves hard pre-filtering of `CorpusId`, `IndexGenerationId` and
  any explicit administrative filters before top-k. Adversarial tests place
  higher-scoring chunks in the wrong generation, database and, when
  applicable, corpus.
- Stale, withdrawn or unavailable official content is excluded before
  retrieval/LLM and reported as degraded coverage without a silent substitute.
- A `304` or identical content hash appends a revalidation observation without
  creating a new snapshot or index generation only when the active record
  already references that compatible snapshot; the observation records
  request/response validators and status.
- A document update while another official source is stale preserves every
  unaffected binding; an eligible `304` can restore availability without
  mixing generations.
- All query-time paths perform no official-source network access. A real sync
  test is opt-in and separately authorised.
- SSRF tests cover DNS rebinding, mixed DNS answers, approved-IP connection,
  Host/SNI preservation, disabled redirects, decompressed-byte limits and
  refusal of certificate-validation egress.
- The generated OpenAPI v1 artefact has compatibility tests and exposes no
  Domain or provider SDK types; its consumer adapter is outside RAG-Challenge.
- Readiness reports degradation per source/document without making a remaining
  healthy active-document path globally unready.
- Architecture tests prevent direct controller-to-provider coupling.

## Related STATE-02 decisions and artefacts

- [ADR-0004 — MVP Catalogue, Governed Documents, Official Sources and Evaluation](ADR-0004-MVP-Corpus-Official-Source-And-Evaluation.md)
- [ADR-0005 — MVP Providers, Persistence and OCI Deployment](ADR-0005-MVP-Providers-Persistence-And-OCI-Deployment.md)
- [ADR-0006 — Security, Egress, Administration and HTTP Contracts](ADR-0006-Security-Egress-Administration-And-HTTP-Contracts.md)
- [STATE-02 Canonical Contracts](STATE-02-Canonical-Contracts.md)
- [STATE-02 Threat Model](../security/STATE-02-Threat-Model.md)

These documents refine this decision but do not provide implementation or
runtime evidence.
