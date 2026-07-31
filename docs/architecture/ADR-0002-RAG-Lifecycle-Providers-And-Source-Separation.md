# ADR-0002 — RAG Lifecycle, Provider Boundaries and Source Separation

- Status: proposed
- Date: 2026-07-29
- Owners: RAG-Challenge RAG / data / security architecture

## Context

The MVP uses one logical corpus and must support a local PDF plus one
official-online PDF, with the evidence scope selected by the user at query
time. The owner also requires later replacement of documents, multiple
corpora, document versioning, incremental synchronisation and provider
substitution. Coupling use cases directly to one source, embedding SDK, vector
database, LLM or mutable index would force large refactoring.

Implementing every future capability now would also overcomplicate the
RAG-Challenge. The design needs stable seams while the MVP keeps one concrete
implementation per seam.

External documentation introduces licensing, provenance, SSRF, prompt
injection, freshness and rate-limit concerns that are different from a local
owner-controlled source.

## Decision

If accepted:

- Define typed ports for local document source, official-source
  synchronisation, parser, chunker, embeddings, vector store, language model,
  immutable document-content store, catalogue and index-generation store.
- Give each provider a stable ID, version, declared capabilities and typed
  non-secret configuration.
- Register one implementation per port through dependency injection in the
  MVP. Do not implement dynamic plug-in loading.
- Model a stable `DocumentId` and immutable content-addressed
  `DocumentVersion`.
- Persist the raw bytes of local versions and official snapshots through
  `IDocumentContentStore`; vector data is derivative and cannot replace the
  source needed for restart, rebuild or rollback.
- Preserve parser, chunking, provider, model, dimensions and schema in an
  immutable index-generation manifest.
- Make the index-generation store the sole system of record for a
  `CorpusActivationRecord` that atomically binds the active generation,
  official snapshot and applicable freshness observation. Query-time policy
  evaluates whether that observation remains eligible. The vector store reads
  and writes immutable generations only by explicit `IndexGenerationId` and
  never owns activation.
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
- Allow the MVP to rebuild the complete index after manual corpus replacement.
  Per-document incremental synchronisation remains future work.
- Include `corpusId` in canonical identities and vector namespaces even though
  the MVP configures one corpus.
- Keep local and official external sources in separate adapters. Use an open
  `sourceAdapterId` and a closed trust classification:
  `LocalAuthorised` or `OfficialExternal`.
- Give the one logical corpus two fixed `SourceScope` values: `Local` and
  `OfficialOnline`. Scope is part of document/chunk identity, digests, vector
  metadata, mandatory filters and citations.
- In the MVP, synchronise exactly one canonical HTTPS PDF URL through an
  allowlisted, anonymous adapter into an immutable governed snapshot before
  retrieval. The URL/query and request contain no token, signature,
  `Authorization`, API key, client certificate or ambient credential. Query
  execution never performs network access.
- Keep snapshot content immutable. Store revalidation, freshness and source
  state as separate append-only observations, including validators sent,
  response status and ETag/Last-Modified observed; a `304` or identical
  content hash updates only the observation binding without creating a
  snapshot or index generation when the active record already references that
  compatible snapshot. Otherwise rebuild a candidate generation.
- Require every query to select exactly one scope. Do not expose `All`, user
  URLs, generic crawling or silent fallback.
- Build a coherent candidate generation containing both scopes. Serialise
  operational content mutations so at most one scope changes per generation;
  bootstrap and global compatibility migrations may rebuild both scopes and
  must validate the complete set. Validate hard pre-filtering before top-k and
  roll back the generation as a whole.
- Preserve the official snapshot selected by the active record when updating
  `Local`, even if official freshness is `Stale`. Freshness controls query
  eligibility and does not remove a snapshot from the generation manifest.
- Require `VectorSearchRequest` to carry `CorpusId`, `IndexGenerationId` and
  `SourceScope`. All three selectors apply before top-k. A global search
  followed by post-filtering violates the port contract; adapters without hard
  pre-filter use equivalent physical partitions.
- Reactivate a previous generation only when the complete two-scope set and
  compatibility key match the intended target. A partial document rollback
  always creates a new candidate.
- Evaluate official freshness outside the vector index. Rollback never marks
  an old snapshot fresh without a real revalidation.
- Preserve provenance in every citation and return
  `INSUFFICIENT_EVIDENCE` when retrieved content does not support an answer.
- Resolve the complete activation record once at query start and use its
  explicit generation, snapshot and observation identities throughout
  retrieval, validation, response metadata and citations.
- Make RAG-Challenge the owner of a generated, versioned HTTP/OpenAPI contract;
  consumer adapters, including a future DB-Notifier adapter, belong to their
  consumer repositories and gates. Do not expose Domain entities or provider
  ports.

The minimum index manifest contains:

```text
manifestSchemaVersion
corpusId
corpusRevision
documentSetDigest
officialSnapshotSetDigest
indexCompatibilityKey
generationSpecDigest
chunkCount
vectorCount
logicalArtifactDigest
```

The first six fields form a canonical build specification whose SHA-256 is
`generationSpecDigest`. A candidate writes under a temporary
`candidateBuildId`; finalisation adds canonical logical-payload counts and
digest. `IndexGenerationId` derives from the SHA-256 content digest of the
complete, versioned canonical UTF-8 manifest with fixed property and ordinal
collection ordering. Identical specification and logical outputs reuse an
identity; different outputs cannot collide under a finalised ID. `createdAt`,
activation status and freshness observations remain outside the identity.
`STATE-03` defines idempotent staging/finalisation, readback evidence and
orphan cleanup; a partial candidate never becomes queryable.

`documentSetDigest` covers ordered document identities and versions from both
scopes. `officialSnapshotSetDigest` covers only official snapshot identities.
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

### Implement full incremental synchronisation in the first MVP

Rejected as unnecessary complexity. Immutable full-generation rebuilds meet
the initial document-replacement need while preserving the future diff seam.

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
- A combined generation makes activation simple but couples rollback of both
  scopes; partial rollback requires a new candidate.
- Multiple corpora can be introduced without changing existing canonical IDs,
  but management UI, RBAC and scheduling still require future work.
- A managed vector store introduces a separate data-egress review; a local
  adapter keeps that policy empty.

## Security and operations

- Treat parsed content and retrieved passages as untrusted.
- Limit file size, pages, chunks, context, tokens, time and concurrency.
- Do not expose file paths, provider exceptions or secrets in citations.
- Validate citations against the retrieved evidence set.
- Apply corpus authorisation before retrieval when RBAC is introduced.
- The official adapter requires an exact HTTPS URL allowlist, bounded PDF
  response, signature/media validation, redirects disabled, canonical URI and
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
  unchanged. Activation changes generation, snapshot, observation and
  sanitised audit record atomically in the control-plane transaction.

## Compatibility and migration

- Changing parser, chunker, embedding model/dimensions or vector schema creates
  a new generation; it never silently reuses incompatible vectors.
- A future incremental algorithm classifies Added, Changed, Removed and
  Unchanged documents using source keys and hashes.
- The MVP official snapshot carries immutable `sourceKey`, canonical URL,
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
- Additional formats and sources add adapters, capability declarations and
  tests without changing Domain semantics.

## Acceptance checks

- Domain/Application contain no provider SDK or filesystem/network code.
- The same use case runs against deterministic test doubles.
- Raw local and official content can be reopened by hash after restart while
  referenced by the active or retained rollback generation.
- Every candidate reopens and verifies each referenced content object before
  activation.
- Candidate staging remains unqueryable until final counts, canonical logical
  artifact digest and adapter-supported readback/sentinel checks pass.
- Generation activation uses compare-and-swap of the complete
  `CorpusActivationRecord`; failed builds, audit failure and concurrency
  conflicts preserve generation, snapshot and observation bindings. The
  complete preceding activation revision remains a rollback target.
- Activation and return to the retained previous generation are tested,
  including crash before, during and after every persistence boundary.
- A configuration mismatch returns a typed unavailable/incompatible result.
- A query without supporting evidence returns `INSUFFICIENT_EVIDENCE`.
- Citations contain scope, corpus, generation, document and version identity;
  official citations also contain canonical URL, snapshot and freshness.
- `Local` and `OfficialOnline` retrieval prove hard pre-filtering of
  `CorpusId`, `IndexGenerationId` and `SourceScope` before top-k; otherwise the
  vector adapter uses equivalent physical partitions. Adversarial tests place
  higher-scoring chunks in the wrong scope, generation and, when applicable,
  corpus.
- Stale, withdrawn or unavailable official content returns a typed outcome
  before retrieval/LLM and never falls back to `Local`.
- A `304` or identical content hash appends a revalidation observation without
  creating a new snapshot or index generation only when the active record
  already references that compatible snapshot; the observation records
  request/response validators and status.
- A local update while the official source is stale preserves the official
  snapshot; a later eligible `304` can restore official availability without
  mixing scopes.
- Local queries and all query-time paths perform no official-source network
  access. A real sync test is opt-in and separately authorised.
- SSRF tests cover DNS rebinding, mixed DNS answers, approved-IP connection,
  Host/SNI preservation, disabled redirects, decompressed-byte limits and
  refusal of certificate-validation egress.
- The generated OpenAPI v1 artefact has compatibility tests and exposes no
  Domain or provider SDK types; its consumer adapter is outside RAG-Challenge.
- Readiness reports official-source degradation by scope without making a
  healthy Local query path globally unready.
- Architecture tests prevent direct controller-to-provider coupling.
