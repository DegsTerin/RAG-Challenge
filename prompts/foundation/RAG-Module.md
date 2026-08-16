# RAG Module

## Status

Current architectural contract for the MVP and its evolution, reconciled with
accepted ADRs. Implemented and tested state belongs to Current State and state
reports. Corrective increments implemented the language split, content store,
rights gates, renderer/PNG and activation bindings anticipated by ADRs
0008/0009. Later increments implemented v2, same-origin serving, the local
notice-bearing profile and the text-first flow while preserving v1 byte for
byte; notice-bearing AQG and product homologation remain separate.
`S04-CORR-04-E` locally implemented ADR-0010's `AnswerEvidenceRecordV1`,
`P30D` retention and reachability participation without a gate or homologation.
The PostgreSQL 18.4 textual generation was materialised, validated and later
activated at revision `1` with `renderManifestId=null`; that activation made no
product query or answer. Current State preserves the current evidence and
boundaries.

## Objective and boundaries

The module retrieves evidence from a governed corpus and generates grounded
answers. It does not:

- administer databases;
- execute SQL or shell;
- access secrets directly;
- decide authorisation from model output;
- access the internet without its own adapter and policy;
- treat retrieved text as trusted instruction.

## Canonical pipeline

```text
Governed local document or official snapshot (PDF/CSV)
  -> Discovery
  -> Validation
  -> Content-addressed persistence and verified reopen
  -> Parsing
  -> Normalisation
  -> Chunking
  -> Embeddings
  -> Index generation
  -> Activation

Question
  -> Validation
  -> Question-language validation
  -> Active catalogue and coverage resolution
  -> Per-document availability/freshness validation
  -> Query embedding
  -> Retrieval
  -> Evidence policy
  -> Grounded generation
  -> Citation validation
  -> Optional notice-bearing rendering of at most five cited PDF pages
  -> Persistent answer-evidence binding for Answered
  -> Answer outcome
```

## Concepts

### Corpus

A logical knowledge unit with:

- stable `corpusId`;
- name and description;
- `Active`, `Inactive` or `Unavailable` state;
- source policy;
- content languages declared through canonical BCP 47 tags, without broadening
  supported question/answer languages;
- declared revision;
- logical reference to the active index generation, whose canonical record
  belongs exclusively to `IIndexGenerationStore`.

The MVP has one configured logical corpus containing an administrable database
and document catalogue; this does not implement multiple-corpus management.

`LocalAuthorised` or `OfficialExternal` origin enters provenance, digests,
vector metadata and citations but does not split query into mutually exclusive
corpora. Every active eligible document participates in default retrieval.

### Database and category

`DatabaseProductId` identifies a logical entity independently of its display
name. Immutable `DatabaseProductRevision` associates zero or more categories
through `DatabaseCategoryAssignment`; categories are many-to-many.

Databases move `Candidate → Active ↔ Deactivated`; `Candidate` or
`Deactivated` may move to `Removed`. `Removed` is an auditable logical
tombstone. A database can be `Active` only when at least one associated
document is also `Active` and eligible. Removing the final active document
requires explicit atomic database deactivation.

### Document and version

`DocumentId` identifies a logical document associated with a
`DatabaseProductId`. Every immutable content object receives a
`DocumentVersion` with:

- SHA-256 hash;
- size and media type;
- `Pdf` or `Csv` format;
- declared version when available;
- source and ingestion dates;
- `contentLanguage` as canonical BCP 47 `DocumentContentLanguage`;
- exact `sourceDeclaredLanguage` when supplied by publisher or embedded
  metadata, without inferring region or script;
- `sourceAdapterId`;
- `SourceTrustClass`;
- sanitised locator;
- licensing/provenance state.

Documents and versions use `Candidate`, `Active`, `Deactivated` and `Removed`
with catalogue semantics. A new version is a candidate while the prior one
remains active. Deactivation removes it from retrieval without erasing history;
removal is logical, and bytes are deleted only after retention and proof that
no active/retained revision reaches them.

Renaming a file must not silently create a new logical identity when a stable
catalogue mapping exists. Changed content always creates a new version.

Parser and configuration do not change `DocumentVersion`. A derived textual
artefact is separately identified by `DocumentVersion`, the
parser/normalisation descriptor and its non-secret configuration; this
compatibility enters the index generation.

Runtime implements `SupportedQueryLanguage` closed to `pt-BR` and `en-GB` and
separate BCP 47 `DocumentContentLanguage`, preserving exact
`sourceDeclaredLanguage`. Public v1 remains closed to `pt-BR|en-GB`; PostgreSQL
candidate `en` is neither coerced to `en-GB` nor activated through that
surface. Locally implemented v2 preserves the query/document-language split.

### Content and page visual evidence

`IDocumentContentStore` is the sole binary source of truth for exact source
bytes and persistent PNGs. Every object is immutable, identified by its own
byte SHA-256, written idempotently and reopened with verified hash and size.
Git, Git LFS, `artifacts-local/`, catalogue and vector store are not substitutes.

For PDF, accepted profiles produce one `image/png` per physical page, numbered
from 1, at 144 DPI, 8-bit RGB, white background, preserved aspect ratio and no
more than 4,096 pixels per axis. `DocumentPageImage` binds document/version,
source content, page, profile/renderer and PNG content. An activation-bound
`DocumentRenderManifest` records the complete ordinal set, page count,
descriptors and canonical digest. Failure, gap, duplication, exceeded limit,
invalid signature or inconsistent readback rejects that visual activation
mode. CSV receives no implicit visualisation.

Implemented `pdf-page-png-notice-v1` preserves the page region pixel for pixel
and appends a deterministic panel containing the complete
`DerivativeObligationSetV1`. Manifest, persistence/reachability, readback and
serving bind and revalidate the same identity/digest; the Dashboard presents
the exact content as accessible text beside the figure. This neither
reclassifies a candidate nor creates a product corpus or replaces its AQG.

Importing or rendering does not activate content. Text-first mode can activate
a PDF with a finalised textual/indexed generation, `TextualEvidence` rights, a
verified source object and `renderManifestId=null`. Alternatively, visual
activation atomically binds `PdfVisualEvidence` rights, a complete manifest and
all verified PNGs. After a text-first v2 answer cites exact PDF pages, runtime
may render on demand only those pages, from one to five per answer. The sparse
manifest belongs only to that answer's persisted evidence, never to the
activation binding. A `Deactivated` or `Removed` document serves no image;
cleanup proves no reachability through active/retained document, manifest,
answer evidence or rollback.

Implemented `AnswerEvidenceRecordV1` makes answer evidence a fixed-expiry,
no-refresh root. Local synthetic `S04-CORR-04-E` evidence replaces neither an
Automatic Quality Gate, homologation nor operational cleanup proof.

### Chunk

Every chunk preserves:

- corpus, database/revision, document/version, format and trust class;
- `contentLanguage` inherited from the document version;
- chunking strategy and version;
- order;
- PDF page/block or CSV row/column/header;
- normalised-text hash;
- permitted filter and citation metadata.

Chunks carry neither secrets nor trusted instructions.

### Index generation and compatibility

An immutable generation is identified by a finalised canonical manifest:

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

`activeDocumentSetDigest` covers the ordinal database/revision,
document/version and format list. `sourceBindingSetDigest` covers the ordinal
generation-bound projection of database/revision, document/version/format,
`sourceAdapterId`, trust, immutable/versioned source registration and immutable
snapshot. `sourceObservationId` is excluded from `sourceBindingSetDigest`,
`generationSpecDigest`, complete-manifest digest and `IndexGenerationId`. The
digest overlap is deliberate: one proves the document set and the other proves
origin, trust, registration and snapshot materialised in artefacts/citations.

`generationSpecDigest` is the SHA-256 of the canonical representation of the
first seven manifest fields and identifies the build specification. The
candidate uses temporary `candidateBuildId`; after all chunks and vectors are
produced, finalisation computes `logicalArtifactDigest` over ordered canonical
logical records, records counts and produces the complete manifest.

The manifest uses a versioned schema, fixed property names/order, UTF-8
serialisation and ordinal set ordering. `generationContentDigest` is the
complete-manifest SHA-256; `IndexGenerationId` derives from it with a stable
prefix. Identical specification and logical artefacts yield the same identity;
different output never collides under an already-finalised ID. `createdAt`,
operational status and freshness observations remain outside the content digest.

The complete activation-record binding is protected by
`activationBindingSetDigest`. It covers the same ordinal source projection plus
`sourceObservationId` under a distinct versioned canonical domain with fixed
properties, UTF-8, ordinal order and unambiguous null. Changing only the
observation changes the record digest but not manifest digests or
`IndexGenerationId`. Changing snapshot, adapter, trust, immutable registration,
document or compatibility requires a new generation.

`STATE-03` closes idempotent staging/finalisation. Activation admits only a
finalised manifest after checking counts, produced-payload digest and
adapter-supported readback/sentinels. A partial candidate gets no
`IndexGenerationId` and is not queryable; retry may reuse only proven-compatible
staging or explicitly clean the orphan.

`IndexCompatibilityKey` is the SHA-256 of a canonical, versioned, secret-free
serialisation of:

```text
parserAdapterId/version/nonSecretConfigDigest
sourceAdapterDescriptorSetDigest
normalisationId/version/nonSecretConfigDigest
chunkerId/version/nonSecretConfigDigest
chunkSize/chunkOverlap/separatorPolicy
embeddingAdapterId/version/nonSecretConfigDigest
embeddingProviderId/modelRevision/dimensions/vectorNormalisation
vectorStoreAdapterId/version/schemaVersion
distanceMetric/indexAlgorithm/nonSecretIndexParametersDigest
```

Any change produces a different key and requires a new generation. The
application refuses an index whose key does not match active configuration;
nominal version equality cannot silently reuse incompatible artefacts.

## Replaceable ports

| Port | Responsibility |
|---|---|
| `IDocumentSource` | Enumerate and open documents without interpreting content. |
| `IOfficialSourceSynchroniser` | Fetch only configured official sources and produce an immutable snapshot. |
| `IDocumentParser` | Transform validated bytes into textual units and locations. |
| `IChunkingStrategy` | Produce deterministic versioned chunks. |
| `IEmbeddingProvider` | Generate vectors with model descriptor and dimension. |
| `IVectorStore` | Write immutable generations and query by `VectorSearchRequest` with `CorpusId`, `IndexGenerationId`, eligible generation-bound selectors and optional administrative filters; prove hard pre-filtering before top-k and do not manage activation. |
| `ILanguageModel` | Generate an answer bounded by prompt and evidence. |
| `IDocumentContentStore` | Persist and reopen immutable content-addressed bytes for sources, official snapshots and page PNGs. |
| `IDocumentCatalog` | Persist identities, versions, provenance and state. |
| `IIndexGenerationStore` | Persist manifests and be the sole source of truth for `CorpusActivationRecord`, with compare-and-swap and rollback. |

Each implementation declares identifier, version, capabilities, limits and
non-secret configuration. Registration is static through dependency injection
in the MVP; dynamic plug-ins are outside scope.

`IDocumentContentStore` writes idempotently by hash, validates reopened content
and prevents overwrite. Catalogue and manifest keep stable references;
retention prevents removing source/image content reachable through
active/retained documents/manifests/evidence and the sole rollback target.
Vector storage contains derivatives and does not replace raw rebuild material.

Application orchestrates activation. The vector store maintains no second
active-state authority; an adapter's technical alias is a recoverable
projection, never the system of record. Global search followed by post-filter
also violates the isolation contract.

### Activation state and record

A generation moves `Candidate → Validated`; pre-validation failure becomes
`Failed`. `Active` and `Retained` are projections from the current activation
record and complete revisions preserved for rollback, never concurrent mutable
states elsewhere. Only `Validated` may enter an active record.

`CorpusActivationRecord` contains at least:

```text
corpusId
recordRevision
previousRecordRevision?
indexGenerationId
catalogueRevision
activationBindingSetDigest
documentBindings[]
  databaseProductId/databaseProductRevision
  documentId/documentVersion/documentFormat
  sourceTrustClass/sourceAdapterId
  officialSourceRegistrationId?/sourceSnapshotId?/sourceObservationId?
evidenceBindings[]
  complete documentBinding
  sourceContentObjectId
  rightsSchemaVersion: 1
  rightsDecisions[10]
  renderManifestId? # null for text-first PDF/CSV; complete for visual PDF
generationActivatedAt
recordUpdatedAt
```

Compare-and-swap changes the complete record in one control-plane transaction
that preserves complete old/new representations in versioned activation
history and writes the sanitised audit event. Bindings are ordinally ordered.
Observation-free `activeDocumentSetDigest` and `sourceBindingSetDigest` must
match the manifest; observation-bearing `activationBindingSetDigest` must
match the record. Every official observation already exists immutably and
names the same binding registration/snapshot before the transaction. Failure
or conflict leaves prior record/history intact; candidate content,
observations and vectors remain auditable orphans until explicit cleanup.
Query reads the current record once and does not combine generation with
separately read catalogue state or “latest observation”.

Every new revision persists one immutable evidence binding per document: exact
`DocumentBinding` and source object, complete snapshot of ten rights decisions
in schema `1`, and optional `renderManifestId`. `null` selects text-first; a
value selects the complete visual-activation manifest. These fields create no
administrative identity/revision or global rights digest and do not alter the
`sourceBindingSetDigest` or `activationBindingSetDigest` domains. Replay by the
same `OperationId` also compares all links and decisions. A historical revision
without the complete set remains uninferred but fails closed as current query
or visual-readiness authority.

Before CAS, implementation checks corpus, document, version, format, source
object, supported document language, finalised textual/vector generation and
manifest-identical bindings. CSV and text-first PDF with
`renderManifestId=null` require all `TextualEvidence` rights `Permitted` and a
verified source reopen. PDF with `renderManifestId` requires all
`PdfVisualEvidence` rights `Permitted`, a finalised same-source manifest, one
consecutive row per physical page and verified source/all-PNG reopen. An
on-demand sparse manifest cannot satisfy this activation binding. The Control
transaction writes revision, bindings, evidence/rights, retention, head, audit
and applicable administrative-journal completion atomically.

`catalogueRevision` identifies the immutable catalogue snapshot in the
generation specification. The append-only observation journal has its own
revision. Freshness rebinding advances journal and `recordRevision`, never
`catalogueRevision`; a row's internal transactional version is also distinct.

## Local and external sources

### Authorised local sources

Uses `sourceAdapterId=local-directory` and
`SourceTrustClass=LocalAuthorised`. Adapter ID is extensible; trust
classification is closed and grants no authority itself.

- configured canonicalised root;
- no access outside the root;
- extension/media-type allowlist;
- per-file/operation size, page/row and concurrency limits, without a product
  ceiling on total documents;
- content hashed before indexing;
- validated bytes promoted idempotently to `IDocumentContentStore` and reopened
  with verified hash before activation;
- activation-bound PDF manifests require complete rendering and verified
  persistence/reopen of all PNGs before CAS; text-first PDF keeps
  `renderManifestId=null` and does not pre-render pages;
- no dependency on `reference-materials/`.

### External official sources

Separate implementation with `SourceTrustClass=OfficialExternal` and stable
adapter-specific `sourceAdapterId`:

- HTTPS only;
- any number of approved adapter-compatible records;
- exact allowlisted scheme, domain, port, path and query for every PDF/CSV;
- anonymous public source without userinfo, query token/signature,
  `Authorization`, API key, client certificate or environment credential;
- redirects disabled in the MVP;
- each physical connection resolves/authorises DNS/IP once, connects only to an
  approved IP and preserves host/SNI without new hostname resolution;
- TLS validation cannot fetch out-of-policy AIA, CRL or OCSP; `STATE-02`
  decides trust, revocation, chain downloads and possible local material, and
  every auxiliary destination requires its own allowlist;
- timeout, maximum bytes/pages/rows, PDF/CSV media type/signature/structure,
  concurrency and rate limit;
- terms, licence and robots reviewed before first synchronisation;
- immutable content snapshot with `sourceKey`, `snapshotId`, canonical URL,
  capture-observed ETag/Last-Modified, hash, `retrievedAt` and licence;
- snapshot bytes persisted by `IDocumentContentStore`;
- derived PDF pages persisted in the same content store only after specific
  rights and rendering authority;
- append-only revalidation observations with `observationId`, `snapshotId`,
  sent conditional validators, HTTP status, observed ETag/Last-Modified,
  `revalidatedAt`, `maxAge`, outcome and sanitised evidence;
- synchronisation to a governed snapshot before retrieval;
- visible local/official origin without splitting default retrieval space.

Snapshot content never changes. Configured source binding has `Current`,
`Stale`, `Withdrawn` or `Deactivated` state derived from the observation named
by `CorpusActivationRecord`, not simply the last written observation. Expired,
withdrawn or deactivated content is not presented as current; status and
freshness accompany citations. The default MVP policy fails the document
closed when the active record binds no eligible `Current` snapshot/observation;
other active documents remain eligible and degraded coverage is explicit.

Query receives no unrestricted web access. Synchronised content changes no
policy, system prompt or authority.

Official synchronisation is a manual administrative use case per record:

1. load approved `OfficialSourceRegistrationId`; no URL comes from the question;
2. canonicalise the credential-free public URL, validate its allowlist,
   resolve A/AAAA and reject mixed/prohibited responses; connect to the
   approved IP preserving host/SNI with no lateral TLS-validation egress;
3. make a conditional request using active-binding validators and persist
   sent/received validators and status; redirects remain disabled;
4. on `304` or identical hash, persist an immutable observation and, only when
   it names the same immutable registration and active-manifest snapshot,
   create a complete new record revision with `sourceObservationId` and
   recalculated `activationBindingSetDigest`; publish by compare-and-swap with
   atomic audit or fail closed;
5. for new content, download to quarantine, bound bytes/parser work, validate
   PDF/CSV, hash, persist/reopen the snapshot and create a Candidate document
   version;
6. build and validate a candidate generation with the new ordered set;
7. atomically activate database/document when applicable and replace the
   complete `CorpusActivationRecord` with sanitised audit.

An authoritative `404`/`410`, as defined by source policy, creates a
`Withdrawn` observation bound to the active snapshot. An explicit audited
administrative operation creates `Deactivated` without fetch. In both cases,
CAS changes only the compatible record binding, record digest/revision and
audit. It preserves manifest, `sourceBindingSetDigest`, `generationSpecDigest`,
`IndexGenerationId`, `catalogueRevision`, `generationActivatedAt` and snapshot
when only freshness makes the document ineligible; no reindex occurs. A
transient DNS/transport/`5xx` failure records the attempt but does not replace
a `Current` observation; the snapshot becomes `Stale` through `maxAge`.
Returning to `Current` requires eligible synchronisation/revalidation and,
after `Deactivated`, explicit administrative reactivation.

Transient failure or rejected synchronisation never changes active generation,
snapshot or observation. A prior snapshot serves only while `Current`; after
`maxAge` it leaves retrieval and the answer exposes degraded coverage without
silently presenting another origin as a substitute.

## MVP update strategy

1. Resolve active databases/documents plus explicitly selected candidates and
   bound official snapshots.
2. Validate each document's format, provenance, licence, identity and hash.
3. Persist or reuse the immutable object by hash, reopen it through
   `IDocumentContentStore` and verify its bytes.
4. Explicitly select PDF binding: text-first with `renderManifestId=null`, or
   visual with a complete finalised manifest verified before activation.
5. Build one generation with every eligible chunk and database, document,
   format, origin and trust metadata.
6. Validate manifest, reopenable references, compatibility, eligibility,
   coverage, both binding domains and smoke queries.
7. Compare-and-swap the complete `CorpusActivationRecord` in
   `IIndexGenerationStore`, including document/source bindings, rights
   snapshots and applicable render manifests.
8. Keep the active generation and at least one prior validated generation until
   explicit cleanup after the defined rollback window.

The MVP may rebuild the complete generation. It need not implement per-chunk
diff, scheduler, queue or distributed synchronisation.

By default, every question retrieves across all active eligible bindings in the
resolved record. Origin is not an implicit filter; optional administrative
database/document filters, when introduced, must be explicit.

Joint-generation invariants:

- a candidate represents a coherent snapshot of the complete selected catalogue;
- each update preserves unchanged databases/documents by identity/version in
  the new manifest;
- content updates are serialised per corpus;
- an active database has at least one active/eligible document;
- removing the final active document requires explicit atomic database
  deactivation;
- `VectorSearchRequest` requires `CorpusId` and `IndexGenerationId`; declared
  administrative filters and eligible-binding selectors derived from the
  resolved record also apply before top-k;
- post-filtering after global search violates the contract;
- rollback replaces the complete generation; partial rollback creates a new
  candidate;
- official freshness is revalidated outside the index and does not become
  `Current` merely through rollback.

## Future incremental update

```text
discover snapshot
  -> compare source keys, hashes and versions
  -> classify Added / Changed / Removed / Unchanged
  -> parse and embed Added + Changed
  -> tombstone Removed in the candidate generation
  -> reuse compatible Unchanged artefacts
  -> validate candidate generation
  -> atomically activate
```

Requirements:

- idempotent operation;
- checkpoint and safe resume;
- batch, timeout, cancellation and backpressure;
- no query reads a partially written generation;
- removal preserves audit and retention;
- incompatible provider/model/chunking forces a controlled rebuild.

## Rollback

Index rollback targets a preserved validated prior generation and its complete
generation-bound projection. It never restores bytes from a historical
`CorpusActivationRecord`; it constructs a new current revision, recalculates
`activationBindingSetDigest` and publishes by compare-and-swap. It neither
edits vectors in place nor combines a prior generation with an arbitrary
official binding.

For every target official registration/snapshot, the administrative
transaction explicitly receives and validates an existing append-only
observation that is compatible and eligible under current policy; it does not
implicitly select “the latest”. If the set cannot keep every active database
with eligible evidence, rollback is rejected without changing the current
record. Historical observation timestamp, `maxAge` and state are never
rewritten; correction requires a new append and activation revision.

Rollback also receives current evidence bindings and revalidates rights,
source objects, textual/vector generation and render manifests; it does not
blindly copy a historical snapshot. Freshness-only rebinding preserves
immutable bindings only when document, version, generation and manifest remain
identical.

Document rollback selects a prior version and creates a new candidate for the
complete manifest. A prior generation may be reactivated only when its complete
generation-bound set and compatibility key match the target and selected
observations satisfy current policy; reactivation never makes an old snapshot
`Current` again.

Application, configuration, catalogue and index rollback are separate
procedures. MVP cleanup never removes the active generation or sole eligible
rollback target before the approved window ends. After explicit cleanup or
proved runtime incompatibility, the path is controlled rebuild, not a false
rollback promise.

Activation and return must test:

1. reading the expected record;
2. whole-record compare-and-swap to the validated candidate;
3. safe rejection on concurrency conflict;
4. query using only the resolved generation;
5. construction and compare-and-swap of a new record pointing to the prior
   generation with compatible currently eligible observations;
6. validation of document, generation-bound source and complete-binding digests
   with atomic record, observation and audit event;
7. crash before, during and after each persistence boundary;
8. preservation of complete historical records without freshness replay;
9. actor, reason, origin, target and outcome audit.

## Future multiple corpora

- Vector namespace and metadata always include `corpusId`.
- Query receives explicit corpus scope.
- Activation and version are independent per corpus.
- Adding, removing or deactivating a corpus does not change the core.
- Authorisation filters apply before retrieval.
- Results from different corpora are not silently mixed.

The MVP fixes one corpus by configuration and exposes no remote administration.

## Retrieval and generation

- Normalise and bound the question.
- Require `questionLanguage=pt-BR|en-GB` as `SupportedQueryLanguage` and
  validate before any external call; do not infer another language for short
  or ambiguous questions.
- Accept no URL, domain, database, document, origin or adapter as an authority
  field in the question.
- Resolve `CorpusActivationRecord` exactly once at query start.
- Resolve all active/current bindings and coverage before query embedding or
  any provider call.
- Use `CorpusId`, `IndexGenerationId` and generation-bound eligible-binding
  selectors derived from the resolved record in one `VectorSearchRequest`
  throughout retrieval, validation and citation; no stage silently rereads the
  record.
- Use top-k and thresholds defined through evaluation, not guesswork.
- Apply `CorpusId`, `IndexGenerationId`, eligible bindings and optional
  administrative filters before top-k/ranking.
- Clearly separate trusted instructions from untrusted evidence.
- Instruct the model to answer exactly in `questionLanguage` even when evidence
  `contentLanguage` differs.
- Bound passage count and total size.
- Require a reference for every material factual claim.
- Reject a citation outside the retrieved set.
- Do not fill gaps with uncited parametric knowledge.
- Return `INSUFFICIENT_EVIDENCE` without sufficient support.

The model has no direct vector, file, network or catalogue access. Application
selects and bounds evidence.

A stale/unavailable official document does not participate in retrieval and
appears in degraded coverage. Query continues only when at least one active
eligible document exists, without claiming another origin replaced the absent
one.

### Per-capability readiness

- Liveness depends only on the process being able to respond.
- Global readiness requires the control plane, a compatible active generation,
  at least one servable database/document, vector store, query embedding and LLM.
- `Stale|Unavailable|Withdrawn|Deactivated` sources/documents degrade coverage
  and do not make the instance unavailable while another active eligible
  document remains servable.
- Administrative synchronisation-egress availability is separate diagnostics,
  not query-path readiness.

## Answers and citations

`AnswerOutcome` contains only completed outcomes:

- `Answered`;
- `InsufficientEvidence`.

Query failures are separate typed outcomes:

- `InvalidInput`;
- `CorpusUnavailable`;
- `SourceUnavailable`;
- `SourceStale`;
- `SourcePolicyViolation`;
- `EmbeddingUnavailable`;
- `IndexUnavailable`;
- `LanguageModelUnavailable`;
- `RateLimited`;
- `OperationCancelled`;
- `UnexpectedFailure`.

These names are a subset of the canonical Application taxonomy. `STATE-02`
defines one `CH_*`/HTTP/Problem Details table; adapters do not translate one
failure into competing categories.

A citation always includes:

- `corpusId`;
- `databaseProductId` and `databaseProductRevision`;
- `indexGenerationId`;
- `documentId`;
- `documentVersion`;
- `documentFormat`;
- `contentLanguage`;
- chunk ID;
- `sourceAdapterId` and `SourceTrustClass`.

When available, it also includes title, PDF page/block or CSV
row/column/header, and a safe display locator.

Title, section, passage, page label and all source-derived text remain in the
original `DocumentContentLanguage`. Generation may explain evidence in the
question language but does not rewrite or translate citation content. In
implemented v1, `contentLanguage` remains closed to `pt-BR|en-GB`; a broader
tag is not coerced or activated through that surface.

Implemented v2 preserves closed `questionLanguage`/`answerLanguage`, broadens
`CitationV2.contentLanguage` to BCP 47, preserves `sourceDeclaredLanguage` and
adds `PageImageEvidenceV1` references. After validating a grounded answer and
citations, text-first activation may render on demand only the distinct cited
physical pages, one to five per answer. The sparse manifest must match that set
exactly; visual failure preserves the grounded textual answer and invents no
image reference. For the notice-bearing profile, each materialised page carries
`obligationSetId` and the citation carries a complete matching
`DerivativeObligationPresentationV1`. The response embeds neither PNG nor path;
the same-origin endpoint revalidates active binding, manifest, rights,
obligation and unexpired `AnswerEvidenceRecordV1` authority before serving
bounded bytes. Adjacent textual evidence and accessible obligation remain. The
LLM receives text only; images require separate provider/egress/data/cost
authority.

The response includes technical metadata:

- `evidenceCoverage` summary and actually cited origins;
- `indexGenerationId`;
- `retrievalPolicyVersion`;
- `promptVersion`;
- `answerLanguage`, always equal to accepted `questionLanguage`;
- language-model provider and revision;
- `correlationId`.

For an official source, citation also includes the credential-free public
canonical URL, `snapshotId`, `revalidatedAt`, state and freshness. These allow
answer reproduction without exposing prompts, secret configuration or full
content.

### Internal answer-evidence persistence

Only `Answered`, after language, limit, coverage, citation and binding
validation, creates `AnswerEvidenceRecordV1`. The complete record is persisted
and reopened before returning v1. `InsufficientEvidence` and failures create no
record.

The immutable aggregate binds answer hash/length to corpus, activation
revision, catalogue, generation, both binding digests, retrieval policy,
prompt, model and coverage. Each citation preserves exact database, document,
version, format, language, chunk, source/provenance, source object and location
identities. When a cited page was rendered on demand, the same record persists
the exact sparse manifest, profile/renderer and complete PNG identity; without
visual materialisation these fields are absent.

The record contains no question or question hash, answer, citation
title/excerpt/URL, prompt or provider payload, scores/vectors, user identity/IP,
secret, path or bytes. Fixed `answer-evidence-p30d-v1` expires at
`createdAt + P30D` without refresh. Until then, bound source and PNGs remain
reachable; afterwards, cleanup still requires `cleanup-plan-v1` reservation
and complete revalidation before deletion.

Header, citations, pages and sanitised audit form one Control transaction.
Replay of the same ID/digest is `AlreadyApplied`; divergent content under the
same ID conflicts without mutation. Persistence/readback failure prevents
`Answered` and uses the existing v1 taxonomy. OpenAPI v1 does not change.
`S04-CORR-04-E` implemented this locally; gate, homologation, v2 and serving
remain separate.

Raw scores from different providers are not presented as universal confidence
without calibration.

## Security

- Document prompt injection is an explicit threat.
- PDFs and CSVs are untrusted; parsing is bounded. Attachments, actions, links,
  formulae and embedded instructions receive no authority and are not executed.
- PDF rendering is bounded by bytes, pages, time, memory, dimensions and
  concurrency; output and manifests are revalidated before serving.
- Rights to render, derive, retain, display and distribute are independent from
  permission to read, index or cite text.
- Every official connection uses only a previously resolved authorised IP,
  preserves host/SNI and does not resolve hostname again in the socket.
- The official source is anonymous and TLS validation initiates no AIA/CRL/OCSP
  or out-of-allowlist egress.
- Public upload is outside the MVP.
- Questions cannot select provider, path, URL or system prompt.
- Retrieved context uses delimiters and explicit non-authority instructions.
- Answers cannot execute tools or create administrative actions.
- Logs do not store full text by default.
- Caches and indexes are derived data with controlled access and retention.

## Evaluation

Before homologating a provider or version:

- representative questions and no-answer cases;
- relevance, faithfulness and citation-quality rubric;
- retrieval recall/precision under approved criteria;
- answer groundedness and absence of unsupported claims;
- answer in question language and citation in original language;
- prompt-injection and malicious-content security;
- per-database/document/format coverage proportional to the active set;
- adversarial search where chunks excluded by explicit database/document
  filter outrank correct chunks, proving pre-filter before top-k;
- adversarial search where another generation's and, when applicable, another
  corpus's chunks outrank correct chunks, proving pre-top-k isolation;
- official-source SSRF, redirect, domain/path, size and freshness;
- canonical vectors proving that changing only `sourceObservationId` changes
  `activationBindingSetDigest` but not `sourceBindingSetDigest`,
  `generationSpecDigest` or `IndexGenerationId`; changing snapshot, adapter,
  trust or immutable registration requires a new generation;
- `304`/identical hash updates the observation and creates a complete new
  record revision without snapshot/index only when the observation names the
  compatible registration/snapshot; preserved/changed fields follow ADR-0007;
- observation/registration/snapshot mismatch fails before activation; retry
  after conflict is idempotent and uses no implicit “latest observation”;
- one source degrades while others remain servable, followed by `304`
  revalidation preserving snapshot and restoring eligibility without mixing
  generations;
- crash before, during and after observation append, digest calculation, audit
  and compare-and-swap, proving `CorpusActivationRecord` atomicity;
- rollback creates a new record with compatible eligible observations,
  preserves history and fails closed when the evidence invariant cannot hold;
- operation latency, failure, rate limit and cost;
- regression between document, prompt, model and index versions.

The deterministic suite covers the complete question/evidence language matrix:
`pt-BR→pt-BR`, `en-GB→en-GB`, `pt-BR→en-GB` and `en-GB→pt-BR`. When the
approved real corpus lacks an evidence language, unit, contract and integration
tests use authorised synthetic fixtures clearly separated from product corpus.
This matrix does not decide interface visual language.

Every other `DocumentContentLanguage` in the scored corpus creates an exact
additional evidence-language stratum without silent grouping. For PostgreSQL
candidate `en`, the campaign separates at least `pt-BR→en` and `en-GB→en`;
these are not `en-GB` evidence and do not replace the mandatory matrix. Reports
name exact tags, documents, dataset, provider and environment.

Initial dataset, rubric and thresholds belong to `STATE-02`. `STATE-07` runs
the campaign; revision requires a formal recorded decision before the first
execution that could reveal results. No threshold may be selected or changed
after seeing a result to make it pass.

## MVP × evolution matrix

| Capability | MVP | Evolution |
|---|---|---|
| Logical corpus | One, with administrable catalogue | Several with their own authorisation/RBAC |
| Databases and documents | 51 initial; open cardinality through records | Compatible new items without core change |
| Format | PDF and CSV | Authorised Markdown, HTML, Office and others |
| Update | Manual administration and official synchronisation | Incremental diff and scheduler |
| Providers | One per port | Catalogue and multiple implementations |
| Index | Immutable generation, one prior retained and bounded rollback | Migration, compaction and distribution |
| Answer evidence | Minimum internal record, `P30D` retention and reachability after authorised implementation | Different retention, user history or analytics require their own decision |
| Online sources | Allowlisted official records and snapshots | New authentication/protocol classes under their own decision |
| Access | Bounded anonymous query | RBAC and per-corpus scope |
| DB-Notifier integration | None | Versioned adapter or module |
