# ADR-0002 — RAG Lifecycle, Provider Boundaries and Source Separation

- Status: accepted
- Date: 2026-07-29
- Accepted: 2026-08-01
- Decision authority: explicit product-owner acceptance on baseline
  `main@39e2f803bf73cb4e2b59e56a0596e2858a3aed51`, corpus `4.7.0`
- Owners: RAG-Challenge RAG / data / security architecture
- STATE-02 review: accepted independently from ADR-0004, ADR-0005 and
  ADR-0006; acceptance does not authorise implementation or lifecycle progress
- Amended by: accepted
  [ADR-0007](ADR-0007-Generation-Identity-And-Freshness-Observation-Rebinding.md),
  which supersedes only the observation-inclusive generation-identity and
  exact-record rollback clauses
- Refined by: accepted
  [ADR-0008](ADR-0008-Product-Corpus-Storage-And-Page-Image-Evidence.md) for
  durable source/page-image storage and render lifecycle, and accepted
  [ADR-0009](ADR-0009-Document-Evidence-And-Query-Language-Taxonomy.md) for
  separate query and document-language domains. These refinements are
  architecturally current but remain unimplemented unless current factual
  evidence says otherwise.

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
- Use `IDocumentContentStore` as the sole product system of record for exact
  source bytes and persistent PDF page-image bytes. Store every object
  immutably by SHA-256, verify it after write and on reopen, and keep Git, Git
  LFS, intake quarantine and the vector store outside this authority. Vector
  and page-image data are derivatives and cannot replace source bytes needed
  for restart, rebuild, rollback or reproducibility.
- Govern each PDF render through the accepted `pdf-page-png-v1` profile, an
  immutable `DocumentPageImage` binding per physical page and a finalised
  `DocumentRenderManifest` covering the complete ordered page set. CSV has no
  implicit page-image derivative.
- Preserve parser, chunking, provider, model, dimensions and schema in an
  immutable index-generation manifest.
- Make the index-generation store the sole system of record for a
  `CorpusActivationRecord` that atomically binds the active generation,
  generation-bound catalogue revision and ordinal set of active
  database/document/source, snapshot and observation identities. The manifest
  protects the generation-bound projection and the record separately protects
  the complete projection including observations. Query-time policy evaluates
  each binding.
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
  content hash creates a new complete activation-record revision with the new
  observation binding and `activationBindingSetDigest`, without creating a
  snapshot or index generation when the active record already references that
  compatible registration and snapshot. Otherwise fail closed or rebuild a
  candidate generation as required by the changed generation-bound input.
- Query all active/current document bindings by default. Do not expose user
  URLs, generic crawling or public authority-bearing catalogue fields. Report
  provenance and partial coverage explicitly rather than silently substituting
  one source for another.
- Build a coherent candidate generation containing the intended complete
  catalogue set. Serialise content mutations by corpus, validate the complete
  set and roll back the generation as a whole.
- Require `VectorSearchRequest` to carry `CorpusId`, `IndexGenerationId` and
  the eligible generation-bound binding selectors derived from the one
  activation record resolved at query start. Optional explicit
  database/document filters, if later exposed, also apply before top-k. A
  global search followed by post-filtering violates the port contract.
- Roll back to a previous retained, validated generation only by constructing
  a new complete activation-record revision. Its generation-bound projection
  and compatibility key must match the target manifest, and each official
  binding must use an explicitly selected existing observation that is
  compatible and eligible under current policy. Never replay an old activation
  record byte for byte. A partial document rollback always creates a new
  candidate.
- Evaluate official freshness outside the vector index. Rollback never marks
  an old snapshot fresh without a real revalidation.
- Preserve provenance in every citation and return
  `INSUFFICIENT_EVIDENCE` when retrieved content does not support an answer.
- Keep `SupportedQueryLanguage` closed to exact `pt-BR` and `en-GB` values for
  questions and answers. Model document `contentLanguage` separately as a
  canonical BCP 47 `DocumentContentLanguage`, preserve any publisher-declared
  tag as `sourceDeclaredLanguage`, and never infer a more specific tag. In
  particular, `en` is not `en-GB`.
- Preserve every source-derived citation title, section, excerpt, page label
  and quotation in its governed original language. The answer may explain the
  evidence in the supported question language but cannot rewrite citation
  content as if it came from the source.
- Resolve the complete activation record once at query start and use its
  explicit generation and ordered binding identities throughout retrieval,
  validation, coverage, response metadata and citations.
- Make RAG-Challenge the owner of a generated, versioned HTTP/OpenAPI contract;
  consumer adapters, including a future DB-Notifier adapter, belong to their
  consumer repositories and gates. Do not expose Domain entities or provider
  ports.
- Preserve `QueryRequestV1`, `QueryResponseV1`, `CitationV1` and the OpenAPI v1
  artefact unchanged. The accepted `QueryResponseV2`/`CitationV2` direction is
  planned and unimplemented; only that future contract may expose broader BCP
  47 citation tags and validated page-image references.

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
revisions, versions and formats. `sourceBindingSetDigest` covers the ordered
generation-bound projection: database/document identities, revisions,
versions and formats, source adapter, trust class, immutable/versioned source
registration and immutable snapshot. `sourceObservationId` is excluded from
`sourceBindingSetDigest`, `generationSpecDigest`, the complete manifest digest
and `IndexGenerationId`.

Every `CorpusActivationRecord` additionally stores
`activationBindingSetDigest`, which covers the same ordered projection plus
the applicable `sourceObservationId`. Both digests use distinct, explicitly
versioned canonical UTF-8 representations with fixed property order, ordinal
binding order and unambiguous null handling. Before compare-and-swap,
Application recomputes `activeDocumentSetDigest` and
`sourceBindingSetDigest` against the finalised manifest, recomputes
`activationBindingSetDigest` against the proposed record, and verifies that
each official observation names the same immutable registration and snapshot.
An observation-only append advances the observation journal and activation
record revision, not the generation-bound `catalogueRevision`.
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
- Persistent page images inherit the source classification and add bounded
  rendering, backup, serving, accessibility, retention and rights obligations.
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
- Observation-only rebinding changes the complete record, its
  `activationBindingSetDigest` and the sanitised audit atomically while
  preserving the referenced manifest and generation identity.

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
  compare-and-swap publishes a new complete activation-record revision and
  digest for the compatible active registration/snapshot; transient
  transport/`5xx` failures do not replace a `Current` observation and
  freshness expires through `maxAge`.
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
- The implemented closed `SupportedLanguage` type remains the v1 runtime
  boundary until a separately authorised change introduces
  `SupportedQueryLanguage` and `DocumentContentLanguage`. Existing `pt-BR` and
  `en-GB` document tags retain their exact values; broader tags require the
  planned v2 contract and compatible catalogue, dataset and runtime support.
- A PDF version requiring visual evidence can become active only when source
  bytes, the complete render manifest, every referenced PNG, the text/index
  generation and all applicable rights are validated and atomically bound.
  Rendering or import alone never activates content.

## Acceptance checks

- Domain/Application contain no provider SDK or filesystem/network code.
- The same use case runs against deterministic test doubles.
- Raw PDF/CSV local and official content can be reopened by hash after restart while
  referenced by the active or retained rollback generation.
- Every PDF visual-evidence candidate has a complete, consecutive, canonical
  render manifest; source and PNG hashes, byte lengths, dimensions, media type,
  renderer/profile descriptors and verified reopen all match before activation.
- Deactivated or removed documents cannot serve page images. Cleanup proves
  that no active/retained document, render manifest, answer-evidence record or
  rollback target reaches the source or derivative object.
- Every candidate reopens and verifies each referenced content object before
  activation.
- Candidate staging remains unqueryable until final counts, canonical logical
  artifact digest and adapter-supported readback/sentinel checks pass.
- Generation activation uses compare-and-swap of the complete
  `CorpusActivationRecord`; failed builds, audit failure and concurrency
  conflicts preserve generation and all document/source bindings. The
  retained, validated generation and its generation-bound projection remain a
  rollback target; historical freshness bindings are not replayed.
- Activation and return to the retained previous generation are tested by
  constructing a new record with compatible, currently eligible observations,
  including crash before, during and after every persistence boundary.
- A configuration mismatch returns a typed unavailable/incompatible result.
- A query without supporting evidence returns `INSUFFICIENT_EVIDENCE`.
- Citations contain corpus, database, generation, document, version, format,
  trust and location identity; official citations also contain canonical URL,
  snapshot and freshness.
- Retrieval proves hard pre-filtering of `CorpusId`, `IndexGenerationId`, the
  eligible generation-binding selectors derived from the one resolved
  activation record, and any explicit administrative filters before top-k.
  Adversarial tests place higher-scoring chunks in an ineligible binding, the
  wrong generation, database and, when applicable, corpus.
- Stale, withdrawn or unavailable official content is excluded before
  retrieval/LLM and reported as degraded coverage without a silent substitute.
- A `304` or identical content hash appends a revalidation observation without
  creating a new snapshot or index generation only when the active record
  already references that compatible registration/snapshot. It creates a new
  complete activation-record revision and `activationBindingSetDigest` while
  preserving manifest bytes, `sourceBindingSetDigest`,
  `generationSpecDigest`, `IndexGenerationId`, `catalogueRevision` and
  `generationActivatedAt`; the observation records request/response validators
  and status.
- Canonical vectors prove that changing only `sourceObservationId` changes
  `activationBindingSetDigest` but not `sourceBindingSetDigest`,
  `generationSpecDigest` or `IndexGenerationId`; a snapshot, adapter, trust or
  immutable registration change does change generation identity.
- An observation naming another registration or snapshot is rejected before
  compare-and-swap, and retry after conflict is idempotent without a
  query-time "latest observation" lookup.
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
- OpenAPI v1 remains byte-for-byte compatible. Any future v2 artefact is
  separately authorised, implemented and compatibility-tested; an accepted
  planned schema is not an endpoint or runtime capability.
- Evaluation retains the four mandatory `pt-BR`/`en-GB` question/evidence
  pairs and reports every additional exact document-language tag as a distinct
  evidence stratum without coercion or silent aggregation.
- Readiness reports degradation per source/document without making a remaining
  healthy active-document path globally unready.
- Architecture tests prevent direct controller-to-provider coupling.

## Related STATE-02 decisions and artefacts

- [ADR-0004 — MVP Catalogue, Governed Documents, Official Sources and Evaluation](ADR-0004-MVP-Corpus-Official-Source-And-Evaluation.md)
- [ADR-0005 — MVP Providers, Persistence and OCI Deployment](ADR-0005-MVP-Providers-Persistence-And-OCI-Deployment.md)
- [ADR-0006 — Security, Egress, Administration and HTTP Contracts](ADR-0006-Security-Egress-Administration-And-HTTP-Contracts.md)
- [ADR-0008 — Product Corpus Storage and Page-Image Evidence](ADR-0008-Product-Corpus-Storage-And-Page-Image-Evidence.md)
- [ADR-0009 — Document, Evidence and Query Language Taxonomy](ADR-0009-Document-Evidence-And-Query-Language-Taxonomy.md)
- [STATE-02 Canonical Contracts](STATE-02-Canonical-Contracts.md)
- [STATE-02 Threat Model](../security/STATE-02-Threat-Model.md)

These documents refine this decision but do not provide implementation or
runtime evidence.
