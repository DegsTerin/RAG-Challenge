# STATE-02 Canonical Contracts

## Purpose, responsibility and authority

This document defines the proposed canonical application, provider and public
contract semantics for `STATE-02 ARCHITECTURE`. It refines ADR-0002 and
ADR-0006 without implementing types, schemas or endpoints. Every contract is
proposed until its owning ADR is explicitly accepted.

The contracts preserve inward dependencies: Domain owns identities and
invariants; Application owns ports, use cases and failure semantics;
Infrastructure owns adapters; Server owns HTTP mapping and composition.

## Contract conventions

- Identifiers are opaque, stable, case-sensitive strings with a documented
  prefix or slug format.
- Instants use UTC ISO 8601 and durations use explicit units.
- Public payloads use camelCase; .NET concepts below use PascalCase.
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
| `DocumentId` | Stable logical identity independent of filename. |
| `DocumentVersion` | SHA-256, byte length, media type, source metadata and licence/provenance state. |
| `ContentObjectId` | Lower-case SHA-256 identity for immutable reopened bytes. |
| `SourceScope` | Closed enum `Local` or `OfficialOnline`; no combined value. |
| `SourceTrustClass` | Closed enum `LocalAuthorised` or `OfficialExternal`. |
| `OfficialSnapshotId` | Immutable source key, canonical URL and content hash identity. |
| `OfficialObservationId` | Append-only revalidation/freshness observation identity. |
| `CandidateBuildId` | Temporary random/ULID-style build identity; never queryable. |
| `IndexGenerationId` | Deterministic prefix plus complete-manifest content digest. |
| `CorrelationId` | Sanitised request correlation identifier, generated server-side when absent/invalid. |
| `OperationId` | Idempotency and audit identity for administration operations. |

Value types reject empty, overlong, malformed or normalisation-ambiguous
input when constructed. Domain has no filesystem path, URI parser, SQL, PDF,
HTTP or provider SDK type.

## Source and parsing ports

### `IDocumentSource`

```text
DiscoverAsync(DocumentDiscoveryRequest, CancellationToken)
  -> DocumentDiscoveryResult

OpenReadAsync(DiscoveredDocument, BoundedReadPolicy, CancellationToken)
  -> BoundedDocumentContent
```

`DocumentDiscoveryRequest` carries configured `CorpusId`, `SourceScope` and a
trusted source configuration reference. Public input cannot provide a path.
The local adapter proves canonical-root containment before opening content.

### `IOfficialSourceSynchroniser`

```text
SynchroniseAsync(OfficialSynchronisationRequest, CancellationToken)
  -> OfficialSynchronisationResult
```

The request carries only an approved source ID, expected activation revision,
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
page/block units, safe location metadata, parser descriptor and warnings. It
cannot return an executable attachment, raw link authority or filesystem
path.

### `IChunkingStrategy`

```text
Chunk(ParsedDocumentArtifact, NormalisationPolicy, ChunkingPolicy)
  -> OrderedChunkSet
```

Output is deterministic for the complete input descriptors. Every chunk
contains corpus, scope, document/version, stable order, page/location, text
hash and policy versions.

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
SourceScope
QueryVector with expected dimensions
TopK in range 1..8
MinimumScorePolicyVersion
```

The adapter hard-filters corpus, generation and scope before ranking/top-k or
uses an equivalent physical partition. It returns the selectors used in its
result so Application can validate them. It has no activate/deactivate API.

### `ILanguageModel`

```text
Describe() -> LanguageModelDescriptor

GenerateAsync(GroundedGenerationRequest, ProviderCallBudget,
              CancellationToken)
  -> GroundedGenerationResult
```

The request contains trusted instructions, bounded question, bounded
untrusted evidence and allowed chunk IDs. It exposes no tool. Output contains
plain answer text and cited chunk IDs. Application validates all citations and
returns no answer when evidence or output fails policy.

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
success. Deletion requires evidence that no active/retained record references
the object.

### `IDocumentCatalog`

Owns document identities, immutable versions, source descriptors, snapshots,
append-only observations and provenance. It does not own the active generation
pointer.

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

## Canonical activation record

```text
CorpusActivationRecord
  corpusId
  recordRevision
  previousRecordRevision?
  indexGenerationId
  officialSnapshotId?
  officialObservationId?
  generationActivatedAt
  recordUpdatedAt
```

The active record is read once at query start. Retrieval, freshness checks,
response metadata and citations use only identities from that snapshot. No
component combines it with a separately fetched "latest" observation.

## Application use cases

| Use case | Success boundary |
|---|---|
| `BuildCorpusIndex` | Produces a validated immutable generation; does not activate. |
| `ActivateIndexGeneration` | Compare-and-swap of the complete activation record and audit. |
| `RollbackIndexGeneration` | Creates a new record revision targeting a complete retained record. |
| `SynchroniseOfficialSource` | Creates immutable snapshot/observation and rebuilds or rebinds only under recorded compatibility rules. |
| `AskQuestion` | Returns `Answered`, `InsufficientEvidence` or canonical failure for one scope. |
| `GetSystemReadiness` | Returns sanitised global and per-scope capability state without external probing. |

## Query contract v1

### Request

```text
QueryRequestV1
  corpusId: string, required, configured MVP value
  sourceScope: Local | OfficialOnline, required
  question: string, required, 1..4096 UTF-8 bytes after normalisation
```

Unknown properties are rejected to prevent silent interpretation of a URL,
provider or future authority-bearing field.

### Completed response

```text
QueryResponseV1
  sourceScope
  outcome: Answered | InsufficientEvidence
  answer?: plain string
  citations: CitationV1[]
  sourceSnapshotId?: string
  sourceFreshness?: Current
  indexGenerationId
  retrievalPolicyVersion
  promptVersion
  languageModelDescriptor
  correlationId
```

`answer` is required only for `Answered`. `citations` is empty for
`InsufficientEvidence`. An official completed response requires a current
snapshot/observation and includes canonical URL, snapshot and revalidation
metadata in each citation.

### Citation

```text
CitationV1
  corpusId
  sourceScope
  indexGenerationId
  documentId
  documentVersion
  chunkId
  sourceAdapterId
  sourceTrustClass
  title?
  pageStart?
  pageEnd?
  section?
  canonicalUrl?          # official only
  sourceSnapshotId?      # official only
  revalidatedAt?         # official only
  sourceFreshness?       # official only
```

The server builds citations from validated catalogue/evidence records, not
from free-form model fields.

## Failure taxonomy and HTTP mapping

| Application failure | Stable code | HTTP | Public meaning |
|---|---|---:|---|
| `InvalidInput` | `CH_QUERY_INVALID_INPUT` | `400` | Request is invalid or outside bounds. |
| `CorpusUnavailable` | `CH_CORPUS_UNAVAILABLE` | `503` | Configured corpus cannot serve. |
| `UnsupportedDocument` | `CH_DOCUMENT_UNSUPPORTED` | n/a | Local administration failure only. |
| `SourceUnavailable` | `CH_SOURCE_UNAVAILABLE` | `503` | Selected scope is unavailable. |
| `SourceStale` | `CH_SOURCE_STALE` | `503` | Official evidence is not current. |
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
  local: Ready | Unavailable | Incompatible
  officialOnline: Ready | Stale | Unavailable | Withdrawn | Deactivated
  activeGenerationId?
  configurationRevision
  checks: SanitisedCapabilityCheckV1[]
  observedAt
```

No endpoint, exception, file path, SQL detail, provider payload or secret
reference is included. `Degraded` is HTTP 200 only when `Local` remains ready;
`Unready` is HTTP 503.

## Administration command contract

| Command | Required inputs | Idempotent output |
|---|---|---|
| `status` | corpus ID | Current sanitised record and capability state. |
| `synchronise-official` | corpus ID, reason, operation ID | Observation/snapshot outcome and rebuild requirement. |
| `build-index` | corpus ID, reason, operation ID | Candidate or finalised generation identity; never activates. |
| `activate-generation` | generation ID, expected record revision, reason, operation ID | New activation record revision. |
| `rollback-generation` | retained record revision, expected current revision, reason, operation ID | New current record revision targeting complete retained state. |
| `deactivate-official` | expected record revision, reason, operation ID | New observation binding without index mutation. |

Commands use typed exit categories: `0` success, `2` invalid input, `3`
configuration/authority denied, `4` conflict, `5` dependency unavailable and
`10` unexpected failure. Exact Application `CH_*` codes remain in sanitised
structured stderr/audit without secret content.

## Compatibility rules

- Any change to an identifier semantic, required field, enum member meaning,
  outcome classification or `CH_*` meaning needs an ADR.
- OpenAPI v1 breaking changes require v2 unless the owner explicitly accepts a
  pre-release reset before any external consumer exists.
- Parser, normalisation, chunking, embedding or vector schema changes require
  a new `IndexCompatibilityKey` and generation.
- Prompt/model changes require a new prompt/model descriptor and evaluation
  baseline even when vectors remain compatible.
- A provider adapter can change without a Domain/Application contract change
  only when its declared semantics and compatibility descriptor remain exact.

## Required verification

- Contract tests for every port against deterministic fakes and selected
  adapters.
- Architecture tests for inward dependencies and SDK/type isolation.
- OpenAPI snapshot and compatibility tests.
- Adversarial vector tests proving pre-filter before top-k.
- Crash/concurrency tests for every compare-and-swap boundary.
- Negative tests for unknown request fields, bounds, stale source, policy
  violations, invalid provider responses and citation forgery.
- Readiness tests proving official degradation never causes scope fallback.

No item in this document is implementation or test evidence.
