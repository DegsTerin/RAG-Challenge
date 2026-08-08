# ADR-0010 - Persistent Answer-Evidence Records and Bounded Retention

- Status: accepted
- Date: 2026-08-07
- Accepted: 2026-08-07
- Decision authority: explicit product-owner acceptance in the conversation
  `RAG-Challenge — STATE-07 — definição arquitetural de S04-CORR-04-E`, on
  baseline `main@745304051c113c86f5ebbaaaf625fbde74c50c6a`, corpus `4.9.11`
- Registration authority: owner-authorised local, offline and sequential
  documentary registration on the same baseline
- Owners: RAG-Challenge product, RAG architecture, corrective `STATE-04`
  backend, data governance and security
- State: `STATE-07 TESTING_HOMOLOGATION` accepted architecture decision
- Implementation status: not started; this record grants no implementation,
  migration, test, API, serving, gate, lifecycle or external-action authority

## Purpose and authority

This decision assigns `S04-CORR-04-E` one responsibility: define and later
implement the internal persistent `AnswerEvidenceRecordV1`, its bounded
retention and its participation in content reachability. The record preserves
the exact governed evidence bindings used by a successfully completed answer
without retaining the question, answer text, citation text or binary content.

The owner explicitly accepted this architecture in the identified conversation
and separately authorised its documentary registration. This ADR does not
start `S04-CORR-04-E`, select a migration, change an executable contract, prove
a control, create OpenAPI v2 or visual serving, use real data, run a gate, move
the lifecycle or authorise network, Git-remote or other external action.

## Context

[ADR-0008](ADR-0008-Product-Corpus-Storage-And-Page-Image-Evidence.md) requires
physical deletion to account for answer evidence that still reaches source or
page-image objects. The implemented `S04-CORR-04-A` to `S04-CORR-04-D`
increments now provide verified content objects, rights eligibility,
deterministic render manifests and immutable per-activation evidence bindings.
They deliberately introduced no persistent answer-evidence record.

The public v1 response already exposes a completed answer, validated citations,
coverage and generation/provider descriptors. Returning `Answered` without an
atomic durable binding would leave no bounded internal authority to protect the
specific source and page objects used by that answer. Persisting full questions,
answers or excerpts would, however, create unnecessary privacy and retention
exposure.

## Decision drivers

- Preserve the exact evidence identity behind an `Answered` outcome.
- Make retained answer evidence an explicit root in content reachability.
- Keep retention finite and deterministic rather than extending it on reads.
- Avoid storing user text, generated text, source excerpts or provider payloads.
- Fail closed before returning `Answered` when the durable binding cannot be
  committed.
- Preserve the public OpenAPI v1 artefact byte for byte.
- Reuse the existing Control-plane transaction and governed cleanup protocol.

## Decision

### Responsibility and ownership

`S04-CORR-04-E` corresponds exactly to the internal persistent
`AnswerEvidenceRecordV1`, its `P30D` retention rule and its reachability edges.
It does not own OpenAPI v2, page-image serving, answer-history UI, analytics,
long-term audit archives, model-payload capture or evaluation datasets.

Domain owns the immutable identity and invariants. Application owns validation,
creation timing and typed outcomes. Infrastructure owns the Control-plane
mapping, transaction and cleanup traversal. Server composition may invoke the
Application boundary but must not expose the record as a new public v1 field or
endpoint.

### Identity and canonical record

The server creates `answerEvidenceRecordId` as `ans-evidence-` followed by the
32 lower-case hexadecimal characters of a UUID in `N` format. The ID is opaque
and never supplied by a public query caller.

The record uses `schemaVersion = 1`. Its `recordSha256` is the lower-case
SHA-256 of a versioned, length-delimited canonical UTF-8 serialisation with
domain separator `rag-challenge/answer-evidence-record/v1`. Collection order is
canonical and duplicates are rejected. `createdAt` and `expiresAt` participate
in the digest; database-generated row IDs and storage order do not.

```text
AnswerEvidenceRecordV1
  schemaVersion                         # exactly 1
  answerEvidenceRecordId
  recordSha256
  corpusId
  activationRecordRevision
  catalogueRevision
  sourceBindingSetDigest
  activationBindingSetDigest
  indexGenerationId
  outcome                              # exactly Answered
  questionLanguage                     # pt-BR | en-GB
  answerLanguage                       # equals questionLanguage
  answerSha256
  answerUtf8ByteLength
  evidenceCoverageDigest
  retrievalPolicyVersion
  promptVersion
  languageModelDescriptor
  correlationId
  retentionPolicyId                    # answer-evidence-p30d-v1
  createdAt
  expiresAt                            # createdAt + P30D
  citations[]
  pageImages[]
```

`answerSha256` proves which validated generated bytes were returned without
retaining those bytes. It is not an analytics or lookup key. The coverage
digest binds the canonical sanitised `EvidenceCoverageV1` value used for the
response; the record does not copy unbounded coverage text.

### Citation, source, manifest and page bindings

Each citation binding records the exact validated identity used to construct
the public citation:

```text
AnswerEvidenceCitationBindingV1
  ordinal
  databaseProductId
  databaseProductRevision
  documentId
  documentVersion
  documentFormat
  contentLanguage
  chunkId
  sourceAdapterId
  sourceTrustClass
  officialSourceRegistrationId?
  sourceSnapshotId?
  sourceObservationId?
  sourceContentObjectId
  pageStart?                            # PDF only
  pageEnd?                              # PDF only
  recordStart?                          # CSV only
  recordEnd?                            # CSV only
  columns?                              # bounded canonical names, CSV only
  sectionLocator?                      # bounded structural locator only
  renderManifestId?                    # required for PDF
```

No title, excerpt, quotation, canonical URL or other source-derived display
text is persisted. PDF locations require the exact finalised manifest bound to
the resolved activation revision. CSV locations prohibit manifest and page
bindings.

Every distinct physical PDF page covered by a retained citation has one page
binding:

```text
AnswerEvidencePageBindingV1
  documentId
  documentVersion
  sourceContentObjectId
  pageNumber
  renderManifestId
  renderProfileId
  rendererDescriptor
  imageContentObjectId
  imageSha256
  byteLength
  mediaType                            # exactly image/png
  widthPixels
  heightPixels
```

The page tuple must match the cited document, source object, manifest and
activation evidence exactly. It does not grant serving authority and it cannot
name a path or model-controlled URL.

### Creation and covered outcomes

Only a completed `Answered` result creates a record. `InsufficientEvidence`,
invalid input, dependency failure, cancellation, rate limiting and unexpected
failure create no `AnswerEvidenceRecordV1`.

Application constructs the record only after answer-language, answer bounds,
coverage, citations and every source/manifest/page binding have passed their
existing fail-closed validation. It persists and reads back the complete record
before the Server may return the existing v1 `Answered` response. The public
response remains byte-semantically identical to the current v1 contract and
does not include the internal record ID.

### Idempotency, atomicity and failure semantics

One Control-plane transaction writes the record header, all citation bindings,
all page bindings and the required sanitised audit event. No partial record is
observable. Commit or readback failure leaves no successful record and the
query must not return `Answered`.

Replay with the same `answerEvidenceRecordId` and identical `recordSha256`
returns `AlreadyApplied`. Reuse of that ID with different canonical content is
a deterministic conflict and changes nothing. Different server-generated IDs
are not silently deduplicated by question, answer hash or correlation ID.

Existing public failure semantics remain authoritative. Cancellation maps to
`OperationCancelled`/`CH_OPERATION_CANCELLED`; a post-generation persistence,
integrity or readback failure maps to sanitised
`UnexpectedFailure`/`CH_UNEXPECTED_FAILURE`. Earlier validation and dependency
failures keep their current typed mappings. No new public `CH_*` code or v1
outcome is introduced.

### Retention, expiry, reachability and deletion

The fixed policy `answer-evidence-p30d-v1` sets `expiresAt` to exactly `P30D`
after `createdAt`. Read, replay, citation use or operational inspection never
refreshes either instant. A non-expired record is an immutable reachability root
for every bound source content object and PDF page-image object.

Expiry removes that record from the reachability root set; it does not itself
delete the record or any object and grants no caller deletion authority.
Physical deletion remains exclusively governed by `IStorageMaintenance`,
`cleanup-plan-v1` and the existing reservation/finalisation protocol. A cleanup
candidate must capture the expired record and candidate object identities,
reserve them, revalidate expiry and the complete current root set immediately
before deletion, and fail closed if a new or retained reference exists. A
sanitised audit event may outlive the deleted record but contains no user or
source text.

Activation history and rollback retention remain independent roots. Deleting
an expired answer record can never weaken their existing protection or bypass
rights, withdrawal, backup or restoration policy.

### Privacy and logging

The persistent record must not contain:

- the question or a question hash;
- answer text, citation title, excerpt, quotation or canonical URL;
- prompts, retrieved passages, provider requests/responses or exception data;
- raw scores, embeddings or vectors;
- client IP, user identity, session identity or user-agent value;
- secrets, headers, local paths, host names, source bytes or image bytes.

Operational logs use only the record ID, correlation ID, corpus/generation
identities, counts, timings, retention outcome and sanitised failure code. They
do not copy the answer hash by default and never log the excluded content.

### Public compatibility

`QueryRequestV1`, `QueryResponseV1`, `CitationV1`, Problem Details and
`docs/api/openapi-v1.json` remain unchanged. The protected OpenAPI v1 SHA-256 is
`d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
`AnswerEvidenceRecordV1` is an internal persistence contract, not an HTTP
version. It neither creates nor requires OpenAPI v2 or visual-evidence serving.

## Dependencies and future implementation surface

Implementation requires the completed `S04-CORR-04-A` content-store boundary,
`S04-CORR-04-B` rights gates, `S04-CORR-04-C` finalised render manifests and
`S04-CORR-04-D` immutable activation-evidence bindings. It must preserve the
existing digest domains and cleanup protocol.

A separately authorised implementation is expected to touch only the
responsibility owners needed for:

- a Domain `AnswerEvidenceRecordV1` model and canonical serialisation;
- an Application persistence port and query-orchestration boundary;
- Control-plane rows, mapping, context, store transaction, model snapshot and
  one migration without inferred historical backfill;
- `SqliteStorageMaintenance` reachability and cleanup revalidation;
- Server composition behind the unchanged v1 endpoint; and
- focused unit, integration, migration, restart, concurrency, cleanup-race and
  OpenAPI-regression tests.

The expected future file surface is:

- new Domain/Application contracts under
  `src/RagChallenge.Domain/IndexingRetrieval/AnswerEvidenceRecord.cs` and
  `src/RagChallenge.Application/IndexingRetrieval/AnswerEvidencePersistence.cs`;
- query orchestration in
  `src/RagChallenge.Application/IndexingRetrieval/QueryServices.cs`;
- Control persistence in `ControlPlaneRows.cs`, `ControlPlaneMapping.cs`,
  `ControlPlaneDbContext.cs`, a focused `SqliteAnswerEvidenceStore.cs`, the
  Control model snapshot and one new Control migration under
  `src/RagChallenge.Infrastructure/Persistence/`;
- reachability in
  `src/RagChallenge.Infrastructure/Persistence/SqliteStorageMaintenance.cs`;
- unchanged-v1 composition in
  `src/RagChallenge.Server.Api/OperationsGovernance/QueryRuntime.cs`; and
- focused `AnswerEvidenceRecordPolicyTests.cs`,
  `SqliteAnswerEvidenceStoreTests.cs`, existing storage-maintenance reservation
  tests and existing API-v1 contract tests under `tests/`.

These paths identify responsibility and review scope; they do not create the
files, choose the timestamped migration identity or authorise executable work.
A later implementation envelope must confirm the then-current baseline and may
adjust a new file split without changing the accepted contract or expanding
scope.

## Required verification for a future implementation

- Canonical golden vectors for identity, collection ordering and every digest.
- `Answered` persistence before response, and no record for every other outcome.
- Citation/source/activation/manifest/page mismatch rejection.
- Same-ID replay, divergent conflict and injected failure at every transaction
  and readback boundary.
- Fixed `P30D` expiry with no refresh and boundary-instant tests.
- Reachability protection before expiry and safe cleanup after expiry, including
  reserve/revalidate races with concurrent record creation.
- Upgrade, rollback/reapply, foreign-key and model-snapshot checks on disposable
  stores, with no historical inference or real-data migration.
- Privacy allowlist tests for stored rows, audit and logs.
- Byte-for-byte OpenAPI v1 regression at the protected SHA-256.

These are acceptance requirements, not evidence that any test or behaviour now
exists.

## Consequences

- Successful answers gain a reproducible, bounded evidence identity without a
  persistent user-question or answer-history feature.
- Source and PDF page-image objects used by recent answers remain protected for
  30 days even if other roots disappear.
- Cleanup must traverse one additional temporary root and handle concurrent
  creation safely.
- A persistence outage after generation prevents an `Answered` response; this
  favours evidence integrity over partial availability.
- Storage grows with completed answers and citation/page cardinality but not
  with copied answer, question, prompt, excerpt or binary payloads.

## Rejected approaches

### Treat `S04-CORR-04-E` as OpenAPI v2 or visual serving

Rejected because the missing responsibility is durable answer-evidence and
reachability. Public v2 and same-origin serving have separate owners and
authorities.

### Persist every query outcome

Rejected because failures and `InsufficientEvidence` do not create source/page
reachability and would retain unnecessary interaction metadata.

### Persist full questions, answers or citation excerpts

Rejected because evidence identity can be reproduced through hashes and stable
bindings with materially lower privacy and retention exposure.

### Refresh retention on read

Rejected because observation would create unbounded retention and
non-deterministic cleanup eligibility.

### Delete immediately at expiry

Rejected because expiry is only an eligibility condition; safe physical
deletion still requires the governed reservation, complete reachability
revalidation and audit boundary.

## Acceptance and stop conditions

The owner accepted `S04-CORR-04-E` as the persistent
`AnswerEvidenceRecordV1`, its bounded retention and its reachability role. This
ADR and corpus `4.10.0` register that authority without starting the increment.

A future executor must stop before implementation if the protected baseline is
stale, an existing accepted contract conflicts with this schema, a new public
outcome or `CH_*` code appears necessary, v1 bytes would change, historic data
would require inference, cleanup cannot preserve atomic reserve/revalidate
semantics, a new architecture decision is required, or the work would expand
into v2, serving, real data, a gate, lifecycle, network or external action.

Separate explicit authority is required for implementation, migration and
executable tests. Still later and separate authorities are required for v2,
visual serving, real source/data use, Automatic Quality Gate, Human Gate,
lifecycle transition, provider/network activity, publication and deployment.
