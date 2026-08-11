# STATE-02 Canonical Contracts

## Purpose, responsibility and authority

This document defines the canonical application, provider and public contract
semantics accepted for `STATE-02 ARCHITECTURE`. It refines accepted ADR-0002,
ADR-0006, corrective ADR-0007, ADR-0008, ADR-0009 and ADR-0010. Acceptance
freezes the architecture semantics; it does not itself prove or authorise an
implementation. Separately authorised corrective increments implement the
language/content model, durable content store, rights gates, deterministic PDF
rendering and immutable activation-evidence bindings described below while the
v1 public surface and its OpenAPI artefact remain unchanged. Persistent answer
evidence is implemented locally by the separately authorised
`S04-CORR-04-E`. Subsequent separately authorised increments implemented v2,
same-origin page-image serving and the local notice-bearing profile while
preserving OpenAPI v1 byte for byte. Their product-data/browser homologation
remains separate.

The contracts preserve inward dependencies: Domain owns identities and
invariants; Application owns ports, use cases and failure semantics;
Infrastructure owns adapters; Server owns HTTP mapping and composition.

## Contract conventions

- Identifiers are opaque, stable, case-sensitive strings with a documented
  prefix or slug format.
- Instants use UTC ISO 8601 and durations use explicit units.
- Public payloads use camelCase; .NET concepts below use PascalCase.
- Query and answer language values use the exact closed tags `pt-BR` and
  `en-GB`. Document and citation content languages use a separate canonical
  BCP 47 value; a source-declared tag is preserved exactly as provenance and is
  never made more specific by inference.
- Required bounds are validated before external or persistence work.
- Every asynchronous port accepts cancellation and an explicit operation
  budget; no port reads ambient authority from model or document content.
- Provider descriptors contain provider ID, model ID/revision, adapter ID and
  non-secret compatibility metadata only.
- Result unions are explicit. Expected failures do not cross an Application
  boundary as provider exceptions.

## Core identities and value contracts

| Contract | Minimum semantics |
|---|---|
| `CorpusId` | Stable lower-case slug; MVP value is fixed by configuration. |
| `DatabaseProductId` | Stable opaque identity independent of display name. |
| `DatabaseProductRevision` | Immutable display/provenance revision. |
| `DatabaseCategoryAssignment` | Many-to-many link; one database identity may belong to several categories. |
| `CatalogueItemStatus` | `Candidate`, `Active`, `Deactivated` or logical tombstone `Removed`. |
| `DocumentId` | Stable logical identity independent of filename. |
| `DocumentVersion` | Database ID, SHA-256, byte length, format `Pdf` or `Csv`, media type, canonical BCP 47 `contentLanguage`, optional exact `sourceDeclaredLanguage`, source metadata and licence/provenance state. |
| `ContentObjectId` | Lower-case SHA-256 identity for immutable reopened bytes. |
| `SupportedQueryLanguage` | Closed enum backed by exact tags `pt-BR` or `en-GB`; no neutral, inferred or fallback value. |
| `DocumentContentLanguage` | Validated canonical BCP 47 value for document/evidence content; distinct from the supported query enum. |
| `SourceDeclaredLanguage` | Exact bounded BCP 47 tag observed from publisher or embedded metadata and retained with its evidence. |
| `SourceTrustClass` | Closed enum `LocalAuthorised` or `OfficialExternal`. |
| `OfficialSourceRegistrationId` | Identity of an immutable/versioned trusted administrative record containing one exact canonical allowlisted URL and policy reference. |
| `OfficialSnapshotId` | Immutable source key, canonical URL and content hash identity. |
| `OfficialObservationId` | Append-only revalidation/freshness observation identity. |
| `CandidateBuildId` | Temporary random/ULID-style build identity; never queryable. |
| `IndexGenerationId` | Deterministic prefix plus complete-manifest content digest. |
| `CorrelationId` | Sanitised request correlation identifier, generated server-side when absent/invalid. |
| `OperationId` | Idempotency and audit identity for administration operations. |

Value types reject empty, overlong, malformed or normalisation-ambiguous
input when constructed. Domain has no filesystem path, URI parser, SQL, PDF,
HTTP or provider SDK type.

The implementation uses the closed `SupportedQueryLanguage` type for v1
questions, answers and transport values, and the distinct
`DocumentContentLanguage` type for canonical document BCP 47 tags.
`SourceDeclaredLanguage` preserves the exact observed tag alongside its
canonical comparison value. Existing persisted `pt-BR` and `en-GB` values are
read without conversion; `en` remains distinct from `en-GB` and is ineligible
for indexing, activation and query in runtime v1.

## Source and parsing ports

### `IDocumentSource`

```text
DiscoverAsync(DocumentDiscoveryRequest, CancellationToken)
  -> DocumentDiscoveryResult

OpenReadAsync(DiscoveredDocument, BoundedReadPolicy, CancellationToken)
  -> BoundedDocumentContent
```

`DocumentDiscoveryRequest` carries configured `CorpusId`, database/document
identities and a trusted source configuration reference. Public input cannot
provide a path. The local adapter proves canonical-root containment before
opening content.

### `IOfficialSourceSynchroniser`

```text
SynchroniseAsync(OfficialSynchronisationRequest, CancellationToken)
  -> OfficialSynchronisationResult
```

The request carries only an approved source-registration ID, expected activation revision,
reason, actor/operation identities and bounded policy. It does not carry a URL
from the caller. Results are one of:

- `UnchangedObservationCreated`;
- `SnapshotCreatedRebuildRequired`;
- `WithdrawnObservationCreated`;
- `DeactivatedObservationCreated`;
- a canonical Application failure.

A result is not active until the complete `CorpusActivationRecord` is changed
by the Application transaction.

### `IDocumentParser`

```text
ParseAsync(VerifiedContentObject, ParserPolicy, CancellationToken)
  -> ParsedDocumentArtifact
```

The parser receives reopened, hash-verified content. Output contains ordered
units, safe location metadata, parser descriptor and warnings. PDF units use
page/block locations; CSV units use record ranges, columns and headers. It
cannot return an executable attachment/formula, raw link authority or
filesystem path.

### PDF render model and pipeline contract

PDF visual evidence is a separate deterministic derivative boundary. The
accepted `pdf-page-png-v1` profile consumes one verified PDF content object and
produces one immutable `image/png` content object for every one-based physical
page. The renderer records its stable ID/version and non-secret settings;
missing, failed, oversized or unverifiable pages fail the complete candidate.

```text
DocumentPageImage
  documentId
  documentVersion
  sourceContentObjectId
  pageNumber
  renderProfileId
  rendererDescriptor
  imageContentObjectId
  imageSha256
  byteLength
  mediaType
  widthPixels
  heightPixels

DocumentRenderManifest
  schemaVersion
  documentId
  documentVersion
  sourceContentObjectId
  sourcePageCount
  renderProfileId
  rendererDescriptor
  orderedPageImages[]
  manifestSha256
  generatedAt
```

`orderedPageImages[]` contains exactly one entry per physical page in
consecutive order. The versioned canonical UTF-8 manifest digest excludes only
`generatedAt`; finalisation recalculates every hash, validates PNG signatures,
dimensions, counts and bindings, and verifies reopen of every source and image
object. CSV has no implicit render contract. `DocumentPageImage`,
`DocumentRenderManifest`, their typed identities, canonical digest and exact
Control-plane source/image bindings are implemented by `S03-CORR-01`.
`S04-CORR-04-C` implements bounded rendering, PNG signature validation and
verified manifest finalisation. `S04-CORR-04-D` implements immutable activation
binding and fail-closed readback. Later v2 increments implement bounded
same-origin evidence serving; the notice-bearing behaviour is implemented in
`f682827d1a26b08fa8c450a1fadb3bd0e1fa1700` with focused synthetic evidence,
not product homologation.

### `IChunkingStrategy`

```text
Chunk(ParsedDocumentArtifact, NormalisationPolicy, ChunkingPolicy)
  -> OrderedChunkSet
```

Output is deterministic for the complete input descriptors. Every chunk
contains corpus, database/revision, document/version/format, source adapter,
trust, official registration/snapshot identities when applicable, inherited
`contentLanguage`, stable order, format-specific location, text hash and policy
versions.

## Provider ports

### `IEmbeddingProvider`

```text
Describe() -> EmbeddingProviderDescriptor

EmbedAsync(BoundedEmbeddingBatch, ProviderCallBudget, CancellationToken)
  -> EmbeddingBatchResult
```

The adapter must preserve item order, return exact model/revision/dimensions
and classify rate limit, cancellation, invalid response and unavailability.
Application rejects descriptor mismatch before storing a vector.

### `IVectorStore`

```text
WriteCandidateAsync(CandidateVectorBatch, CancellationToken)
  -> CandidateWriteResult

FinaliseCandidateAsync(CandidateFinalisationRequest, CancellationToken)
  -> FinalisedGenerationArtifacts

SearchAsync(VectorSearchRequest, CancellationToken)
  -> VectorSearchResult

DeleteOrphanAsync(OrphanCleanupRequest, CancellationToken)
  -> OrphanCleanupResult
```

`VectorSearchRequest` requires:

```text
CorpusId
IndexGenerationId
QueryVector with expected dimensions
TopK in range 1..8
MinimumScorePolicyVersion
EligibleGenerationBindingSelectors from the resolved activation record
Optional authorised database/document filters
```

Each eligible selector is the generation-bound source projection defined below
and therefore contains no observation identity. Application derives the set by
evaluating the observations in the one activation record resolved at query
start; a caller cannot expand it. The adapter hard-filters corpus, generation,
eligible selectors and any declared administrative filters before ranking/top-k
or uses an equivalent physical partition. It returns the selectors used so
Application can validate them. It has no activate/deactivate API.

### `ILanguageModel`

```text
Describe() -> LanguageModelDescriptor

GenerateAsync(GroundedGenerationRequest, ProviderCallBudget,
              CancellationToken)
  -> GroundedGenerationResult
```

The request contains trusted instructions, bounded question, explicit
`questionLanguage`, bounded untrusted evidence with `contentLanguage` and
allowed chunk IDs. It exposes no tool. Output contains plain answer text,
`answerLanguage` and cited chunk IDs. Application requires
`answerLanguage == questionLanguage`, validates all citations and returns no
answer when evidence, language or output fails policy.

## Persistence ports

### `IDocumentContentStore`

```text
PutAndVerifyAsync(BoundedContentInput, CancellationToken)
  -> ContentObjectDescriptor

OpenVerifiedAsync(ContentObjectId, ExpectedHashAndLength,
                  CancellationToken)
  -> VerifiedContentObject

DeleteUnreferencedAsync(ContentObjectId, CleanupAuthority,
                        CancellationToken)
  -> ContentCleanupResult
```

Writes are idempotent by content hash, never overwrite and reopen before
success. The same store is the sole product system of record for authorised
source bytes and persistent page-image bytes; Git, Git LFS, intake quarantine
and vector storage are not substitutes. Deletion requires evidence that no
active/retained document, render manifest, answer-evidence record or rollback
target reaches the object.

### `IDocumentCatalog`

Owns database identities/revisions/statuses, category assignments, document
identities/versions/statuses, source registrations/descriptors, snapshots,
append-only observations, exact language evidence, rights/provenance and
render-manifest bindings. It does not own the active generation pointer.
Data-driven compatible additions do not add code branches.

### `IIndexGenerationStore`

```text
GetCurrentRecordAsync(CorpusId, CancellationToken)
  -> CorpusActivationRecord?

GetGenerationAsync(IndexGenerationId, CancellationToken)
  -> IndexGenerationManifest?

CompareExchangeRecordAsync(ExpectedRecordRevision,
                           NewCompleteRecord,
                           AuditEvent,
                           CancellationToken)
  -> ActivationExchangeResult
```

The compare-and-swap transaction atomically stores the new complete record,
preserves complete previous/new revisions and writes sanitised audit. It fails
without changing authority if expected revision, manifest validation or audit
write fails.

## Canonical binding integrity domains

The generation-bound source projection contains these fields in this fixed
order:

```text
databaseProductId
databaseProductRevision
documentId
documentVersion
documentFormat
sourceAdapterId
sourceTrustClass
officialSourceRegistrationId?
sourceSnapshotId?
```

`sourceBindingSetDigest` is the lower-case SHA-256 digest of the ordered set of
those projections. `sourceObservationId` is excluded from this digest,
`generationSpecDigest`, the complete manifest digest and
`IndexGenerationId`.

The activation-bound projection appends `sourceObservationId?` after those
nine fields. `activationBindingSetDigest` is the lower-case SHA-256 digest of
the ordered set of complete projections and belongs only to the activation
record and audit history.

Both domains use a distinct version discriminator and the same deterministic
token encoding: UTF-8 without BOM; fixed field order; bindings sorted by the
ordinal value of their generation-bound fields with null before non-null;
duplicate generation-bound projections rejected; and every token encoded as
its invariant decimal UTF-8 byte length, `:`, then its bytes, with null encoded
as `-1:`. The first token is respectively
`rag-challenge/source-binding-set/v1` or
`rag-challenge/activation-binding-set/v1`, followed by the binding count and
then each binding's fields. This length-prefix scheme supplies unambiguous null,
empty-string and record boundaries. `STATE-03` must publish executable golden
vectors for both domains before persistence is accepted.

Before compare-and-swap, Application and the Control store must:

1. recompute `activeDocumentSetDigest` from the proposed bindings and match the
   referenced finalised manifest;
2. recompute the generation-bound `sourceBindingSetDigest`, excluding
   observations, and match that manifest;
3. recompute `activationBindingSetDigest`, including observations, and match
   the proposed record; and
4. verify that every referenced append-only observation exists and names the
   same immutable registration and snapshot as its official binding;
5. match each activation evidence binding to the exact corpus, document,
   version, format, source object and finalised textual/vector generation;
6. require the runtime-supported document content language and the complete
   schema-v1 rights snapshot, with `TextualEvidence` permitted for CSV and
   `PdfVisualEvidence` permitted for PDF; and
7. for PDF, match and reopen the finalised render manifest, its consecutive
   physical-page rows and every referenced PNG object. CSV forbids a render
   manifest binding.

The operation fails without changing the current record when any projection,
observation relation, manifest state, audit write or expected revision fails.
`catalogueRevision` identifies the immutable generation-bound catalogue
snapshot. An observation-only append advances the separate observation journal
and activation `recordRevision`, never `catalogueRevision`.

## Canonical activation record

```text
CorpusActivationRecord
  corpusId
  recordRevision
  previousRecordRevision?
  indexGenerationId
  catalogueRevision
  activationBindingSetDigest
  documentBindings[]       # ordinal canonical ordering
    databaseProductId
    databaseProductRevision
    documentId
    documentVersion
    documentFormat: Pdf | Csv
    sourceAdapterId
    sourceTrustClass
    officialSourceRegistrationId?
    sourceSnapshotId?
    sourceObservationId?
  evidenceBindings[]       # one exact entry per document binding
    documentBinding
    sourceContentObjectId
    rightsSchemaVersion: 1
    rightsDecisions[10]     # state and evidence reference for every right
    renderManifestId?       # required for PDF; absent for CSV
  generationActivatedAt
  recordUpdatedAt
```

The evidence binding and rights snapshot are immutable parts of one activation
revision. They introduce no global rights identity, administrative rights
revision or canonical rights digest. Neither `sourceBindingSetDigest` nor
`activationBindingSetDigest` includes these fields or changes its existing
domain. Exact `OperationId` replay compares the complete evidence binding and
all ten rights decisions in addition to the existing activation fields.

The Control migration preserves historical activation rows without inference
or backfill. A historical revision without a complete evidence binding can be
rehydrated for compatibility, but it fails closed as active query or visual
readiness authority.

The active record is read once at query start. Retrieval, per-binding freshness
checks, response coverage and citations use only identities from that snapshot.
No component combines it with separately fetched catalogue state or a
"latest" observation. Every active database has at least one active/eligible
document binding.

A `304`, identical-content revalidation, authoritative withdrawal or explicit
source deactivation for the same immutable registration/snapshot appends an
observation and creates a new complete record revision. It changes
`recordRevision`, `previousRecordRevision`, `recordUpdatedAt`, the affected
`sourceObservationId` and `activationBindingSetDigest`. It preserves manifest
bytes, `indexGenerationId`, `sourceBindingSetDigest`,
`generationSpecDigest`, `catalogueRevision`, `generationActivatedAt` and the
immutable evidence bindings when document, version, generation and render
manifest remain identical. Any change beyond the freshness observation
requires the applicable full activation path.
Content/snapshot, adapter, trust, immutable registration,
document membership/version/format or `IndexCompatibilityKey` changes require
a new finalised candidate generation.

## Application use cases

| Use case | Success boundary |
|---|---|
| `BuildCorpusIndex` | Produces a validated immutable generation; does not activate. |
| `ActivateIndexGeneration` | Compare-and-swap of the complete activation record and audit. |
| `RollbackIndexGeneration` | Creates a new record revision targeting a retained, validated generation and its generation-bound projection, with explicitly selected compatible and currently eligible observations; it never replays a retained record byte for byte. |
| `AdministerDatabaseProduct` | Adds/versions/activates/deactivates/logically removes a database under catalogue invariants. |
| `AdministerDocument` | Adds/versions/activates/deactivates/logically removes a PDF/CSV document under retention and last-document invariants. |
| `RegisterOfficialSource` | Creates/versions a trusted exact-URL registration; does not enable egress or activate content. |
| `SynchroniseOfficialSource` | Creates immutable snapshot/observation and rebuilds or rebinds only under recorded compatibility rules. |
| `AskQuestion` | Returns `Answered`, `InsufficientEvidence` or canonical failure over all active/current bindings. |
| `GetSystemReadiness` | Returns sanitised global and per-document/source coverage without external probing. |

For the implemented internal activation boundary, PDF activation additionally requires
the verified source content object, applicable rendering/derivative rights, a
complete finalised render manifest, every referenced page-image object and the
finalised text/index generation. Import or rendering alone grants no active
status. Initial activation, replacement and rollback require all evidence
bindings explicitly; rollback builds and validates a new revision against
current source, rights, generation and manifest state. Deactivated and removed
documents cannot serve page images.

## Query contract v1

### Request

```text
QueryRequestV1
  corpusId: string, required, configured MVP value
  questionLanguage: pt-BR | en-GB, required
  question: string, required, 1..4096 UTF-8 bytes after normalisation
```

Unknown properties are rejected to prevent silent interpretation of a URL,
provider or future authority-bearing field.

### Completed response

```text
QueryResponseV1
  outcome: Answered | InsufficientEvidence
  answerLanguage: pt-BR | en-GB
  answer?: plain string
  citations: CitationV1[]
  evidenceCoverage: EvidenceCoverageV1
  indexGenerationId
  retrievalPolicyVersion
  promptVersion
  languageModelDescriptor
  correlationId
```

`answerLanguage` is always equal to the accepted `questionLanguage`. `answer`
is required only for `Answered` and must use `answerLanguage`. `citations` is
empty for `InsufficientEvidence`. `evidenceCoverage` records active/eligible
database/document counts and sanitised degraded source IDs/statuses. Official
evidence requires a current snapshot/observation and includes canonical URL,
snapshot and revalidation metadata in its citation.

### Citation

```text
CitationV1
  corpusId
  indexGenerationId
  databaseProductId
  databaseProductRevision
  documentId
  documentVersion
  documentFormat: Pdf | Csv
  contentLanguage: pt-BR | en-GB
  chunkId
  sourceAdapterId
  sourceTrustClass
  title?
  pageStart?
  pageEnd?
  recordStart?             # CSV only
  recordEnd?               # CSV only
  columns?                 # CSV only; bounded header names
  section?
  canonicalUrl?          # official only
  sourceSnapshotId?      # official only
  revalidatedAt?         # official only
  sourceFreshness?       # official only
```

The server builds citations from validated catalogue/evidence records, not
from free-form model fields. Source-derived title, section, excerpt or other
citation text remains in `contentLanguage`; it is never replaced by a model
translation. This query-language contract does not determine Dashboard labels,
navigation or the selected `interfaceLanguage`. The Dashboard separately
supports `pt-BR` and `en-GB`, and its separate visual state supports `Light`
and `Dark`; no public query field selects its locale or theme.

`EvidenceCoverageV1` is derived from the activation record and contains only
sanitised IDs/counts/statuses. It never exposes a local path, unapproved URL,
licence text, provider configuration or reason for an administrative action.

The complete `QueryRequestV1`, `QueryResponseV1` and `CitationV1` semantics
above remain unchanged, as does the versioned OpenAPI v1 artefact. In
particular, `CitationV1.contentLanguage` retains its closed `pt-BR | en-GB`
values. A document governed with another BCP 47 tag cannot become active
through v1 by coercion or inference.

## Internal answer-evidence persistence contract — implemented locally

Accepted ADR-0010 assigns `S04-CORR-04-E` to an internal persistent
`AnswerEvidenceRecordV1`. This is not an HTTP version, endpoint or public v1
field. Only a fully validated `Answered` result creates a record, and the
complete record must be committed and read back before that existing v1 result
is returned. `InsufficientEvidence` and every query failure create no record.

```text
AnswerEvidenceRecordV1
  schemaVersion: 1
  answerEvidenceRecordId        # ans-evidence- + UUID N lower-case hex
  recordSha256                  # SHA-256 under rag-challenge/answer-evidence-record/v1
  corpusId
  activationRecordRevision
  catalogueRevision
  sourceBindingSetDigest
  activationBindingSetDigest
  indexGenerationId
  outcome: Answered
  questionLanguage: pt-BR | en-GB
  answerLanguage: pt-BR | en-GB
  answerSha256
  answerUtf8ByteLength
  evidenceCoverageDigest
  retrievalPolicyVersion
  promptVersion
  languageModelDescriptor
  correlationId
  retentionPolicyId: answer-evidence-p30d-v1
  createdAt
  expiresAt
  citations[]
  pageImages[]
```

Citation bindings preserve exact database/document revision, format, governed
language, chunk, source adapter/trust, official registration/snapshot/
observation when applicable, source content object, bounded PDF/CSV location
and PDF render-manifest identity. Each cited physical PDF page additionally
binds the exact source object, manifest, render profile/renderer descriptor and
immutable PNG identity/hash/length/media type/dimensions. CSV has no manifest
or page binding.

The record stores no question or question hash, answer text, source-derived
title/excerpt/URL, prompt, provider payload, score, vector, user/client
identity, secret, path or binary content. `answerSha256` and a canonical
coverage digest bind validated response values without retaining those values.

The fixed `P30D` duration is never refreshed. Until `expiresAt`, the record is
an immutable reachability root for its source and page-image objects. Expiry
only removes that root; physical deletion still requires the existing
`cleanup-plan-v1` reservation, complete revalidation and finalisation boundary.
The header, citations, pages and sanitised audit event are written atomically in
one Control transaction. Same-ID/same-digest replay is `AlreadyApplied`;
same-ID/different-digest is a no-change conflict.

Failure to commit or read back after generation prevents `Answered` and maps to
the existing sanitised `UnexpectedFailure`/`CH_UNEXPECTED_FAILURE`; cancellation
keeps `OperationCancelled`/`CH_OPERATION_CANCELLED`. No public outcome or code
is added. OpenAPI v1 remains byte for byte unchanged. The ADR itself granted no
implementation authority; the separately authorised `S04-CORR-04-E` increment
now implements this internal contract, migration and local synthetic tests
without creating a gate or homologation result.

## Implemented query contract v2

ADR-0008 and ADR-0009 established a single successor boundary. This document
did not itself create runtime behaviour; separately authorised later increments
froze and implemented OpenAPI v2, its endpoint/types and same-origin visual
evidence serving. ADR-0012 then added and implemented the notice-bearing fields
without changing v1.

```text
QueryRequestV2
  corpusId
  questionLanguage: pt-BR | en-GB
  question

QueryResponseV2
  outcome: Answered | InsufficientEvidence
  answerLanguage: pt-BR | en-GB
  answer?
  citations: CitationV2[]
  evidenceCoverage
  indexGenerationId
  retrievalPolicyVersion
  promptVersion
  languageModelDescriptor
  correlationId

CitationV2
  every CitationV1 identity, provenance and location field
  contentLanguage: canonical BCP 47 tag
  sourceDeclaredLanguage?: exact observed BCP 47 tag
  pageImages: PageImageEvidenceV1[]
  derivativeObligationPresentation: DerivativeObligationPresentationV1 | null

PageImageEvidenceV1
  pageNumber
  renderManifestId
  imageContentObjectId
  mediaType: image/png
  widthPixels
  heightPixels
  contentSha256
  obligationSetId: exact immutable obligation-set identity | null

DerivativeObligationPresentationV1
  obligationSetId
  contentLanguage
  authoritativePublisherOrAuthor
  documentTitle
  documentVersionLabel
  sourceReference
  attributionText
  copyrightNotice
  permissionNotice
  orderedDisclaimers[]
  trademarkTreatment
  trademarkOrNonEndorsementText
  changeMarkingText
```

`CitationV2` preserves source-derived title, section, excerpt, page label and
quotation in the original governed language. `pageImages` is empty for CSV and
contains only distinct pages referenced by validated citations, with at most
five references per response and no duplicate document-version/page tuple.
The JSON never inlines PNG bytes or exposes a path.

The same-origin read-only evidence endpoint revalidates
the active citation and render-manifest binding before streaming exact bounded
PNG bytes with immutable ETag and `X-Content-Type-Options: nosniff`. For the
notice-bearing profile it also revalidates the current rights mapping,
obligation-set identity/digest and notice-bearing dimensions before `200` or
`304`. Textual evidence and the exact escaped obligation presentation remain
available to assistive technology. The language model receives text only;
image disclosure to a provider requires separate authority.

## Failure taxonomy and HTTP mapping

| Application failure | Stable code | HTTP | Public meaning |
|---|---|---:|---|
| `InvalidInput` | `CH_QUERY_INVALID_INPUT` | `400` | Request is invalid or outside bounds. |
| `CorpusUnavailable` | `CH_CORPUS_UNAVAILABLE` | `503` | Configured corpus cannot serve. |
| `UnsupportedDocument` | `CH_DOCUMENT_UNSUPPORTED` | n/a | Local administration failure only. |
| `CatalogueInvariantViolation` | `CH_CATALOGUE_INVARIANT` | n/a | Administration would leave invalid active state. |
| `SourceUnavailable` | `CH_SOURCE_UNAVAILABLE` | `503` | No eligible active source can serve. |
| `SourceStale` | `CH_SOURCE_STALE` | `503` | No eligible official evidence remains current. |
| `SourcePolicyViolation` | `CH_SOURCE_POLICY_VIOLATION` | `503` | Source capability failed closed. |
| `ParseFailed` | `CH_DOCUMENT_PARSE_FAILED` | n/a | Local administration failure only. |
| `EmbeddingUnavailable` | `CH_EMBEDDING_UNAVAILABLE` | `503` | Query embedding cannot be produced. |
| `IndexUnavailable` | `CH_INDEX_UNAVAILABLE` | `503` | Compatible active index cannot serve. |
| `LanguageModelUnavailable` | `CH_LANGUAGE_MODEL_UNAVAILABLE` | `503` | Grounded generation cannot run. |
| `RateLimited` | `CH_QUERY_RATE_LIMITED` | `429` | Query or provider budget is exhausted. |
| `ConfigurationInvalid` | `CH_CONFIGURATION_INVALID` | `503` | Capability is disabled by invalid configuration. |
| `OperationCancelled` | `CH_OPERATION_CANCELLED` | `503` | Server-side cancellation completed before response. |
| `UnexpectedFailure` | `CH_UNEXPECTED_FAILURE` | `500` | Sanitised unexpected server failure. |

Client disconnect normally produces no response. A server deadline maps to
the failure of the stage that exceeded its bounded budget; the API does not
invent a successful completed outcome.

Problem Details includes only:

```text
type
title
status
detail                    # generic and localised later
instance?                 # safe request path only
code                      # CH_* value
correlationId
retryAfterSeconds?        # bounded and applicable only
```

## Readiness contract

```text
ReadinessV1
  status: Ready | Degraded | Unready
  activeDatabaseCount
  eligibleDocumentCount
  degradedDocumentCount
  sourceStates: SanitisedSourceStateV1[]
  activeGenerationId?
  configurationRevision
  checks: SanitisedCapabilityCheckV1[]
  observedAt
```

No endpoint, exception, file path, SQL detail, provider payload or secret
reference is included. `Degraded` is HTTP 200 only while at least one active
database/document binding remains servable; `Unready` is HTTP 503.

## Administration command contract

| Command | Required inputs | Idempotent output |
|---|---|---|
| `status` | corpus ID | Current sanitised record and capability state. |
| `add-database` / `version-database` | database descriptor, categories, reason, operation ID | Candidate database identity/revision. |
| `activate-database` / `deactivate-database` / `remove-database` | database ID, expected revision, reason, operation ID | New catalogue revision or invariant conflict. |
| `add-document` / `version-document` | database ID, PDF/CSV descriptor, provenance, reason, operation ID | Candidate document/version identity. |
| `activate-document` / `deactivate-document` / `remove-document` | document ID/version, expected revision, reason, operation ID | New catalogue revision or last-document invariant conflict. |
| `register-official-source` | document ID, exact trusted source policy, reason, operation ID | Candidate source-registration identity; no egress. |
| `synchronise-official` | source-registration ID, reason, operation ID | Observation/snapshot outcome and rebuild requirement. |
| `build-index` | corpus ID, reason, operation ID | Candidate or finalised generation identity; never activates. |
| `activate-generation` | generation ID, expected record revision, reason, operation ID | New activation record revision. |
| `rollback-generation` | retained generation ID, explicit compatible observation selections, expected current revision, reason, operation ID | New current record revision targeting the retained generation, or invariant/eligibility conflict without authority change. |

Commands use typed exit categories: `0` success, `2` invalid input, `3`
configuration/authority denied, `4` conflict, `5` dependency unavailable and
`10` unexpected failure. Exact Application `CH_*` codes remain in sanitised
structured stderr/audit without secret content.

## Compatibility rules

- Any change to an identifier semantic, required field, enum member meaning,
  outcome classification or `CH_*` meaning needs an ADR.
- OpenAPI v1 breaking changes require v2 unless the owner explicitly accepts a
  pre-release reset before any external consumer exists.
- Broader document-language values and page-image references belong only to the
  implemented v2 boundary; they never widen v1 by inference.
- Parser, normalisation, chunking, embedding or vector schema changes require
  a new `IndexCompatibilityKey` and generation.
- Prompt/model changes require a new prompt/model descriptor and evaluation
  baseline even when vectors remain compatible.
- A provider adapter can change without a Domain/Application contract change
  only when its declared semantics and compatibility descriptor remain exact.
- Adding a database, category assignment, PDF/CSV document or anonymous exact
  HTTPS source that conforms to existing contracts is data administration and
  needs neither code nor an ADR per item. A new format, protocol,
  authentication or trust semantic may require both.

## Required verification

- Contract tests for every port against deterministic fakes and selected
  adapters.
- Architecture tests for inward dependencies and SDK/type isolation.
- OpenAPI snapshot and compatibility tests.
- Canonical golden vectors for both binding digests, including a case where
  only `sourceObservationId` changes: only `activationBindingSetDigest` may
  change. Snapshot, adapter, trust or immutable registration changes must
  change `sourceBindingSetDigest` and require a new generation.
- Adversarial vector tests proving corpus/generation, eligible
  generation-binding selectors and any declared database/document pre-filter
  before top-k.
- Crash/concurrency tests around observation append, both digest validations,
  audit and every compare-and-swap boundary; retry is idempotent and cannot
  select a query-time "latest observation".
- Rebinding tests prove the exact permitted field changes for `304`/identical
  hash and reject an observation for another registration or snapshot.
- Rollback tests construct a new record, preserve historical records, bind only
  explicitly selected compatible/currently eligible observations and fail
  closed when the active-database/evidence invariant cannot be met.
- Negative tests for unknown request fields, bounds, stale source, policy
  violations, invalid provider responses and citation forgery.
- Catalogue lifecycle tests for 51 unique initial identities, 54 category
  assignments, Candidate activation, logical removal, retention and the
  last-active-document invariant; parser contracts cover PDF and CSV locators.
- Language-contract tests for `pt-BR→pt-BR`, `en-GB→en-GB`,
  `pt-BR→en-GB` and `en-GB→pt-BR` between question and evidence, including
  exact answer-language equality and preservation of source-derived citation
  text.
- Dataset/evaluation tests retain that mandatory matrix and report every
  additional exact `DocumentContentLanguage` as its own evidence stratum. `en`
  is never counted as `en-GB`.
- Visual-evidence tests cover deterministic full-page rendering,
  canonical manifest/hash validation, verified reopen, rights gating,
  lifecycle/reachability, citation-to-image binding, bounded same-origin
  serving, cache headers and text-equivalent accessibility.
- Answer-evidence verification requires tests covering canonical
  identity/digest vectors,
  `Answered`-only creation before response, complete citation/source/manifest/
  page binding, atomic replay/conflict/failure, fixed `P30D` expiry, privacy
  allowlists and cleanup races with reserve/revalidate semantics.
- OpenAPI regression proves the v1 artefact is byte-for-byte unchanged; v2 and
  notice-bearing compatibility evidence remain separately scoped from product
  homologation.
- Readiness tests proving per-source degradation is explicit, never silently
  substituted and remains servable only while eligible evidence exists.

No item in this document is implementation or test evidence.
