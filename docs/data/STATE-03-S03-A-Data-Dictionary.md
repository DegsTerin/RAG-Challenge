# STATE-03 S03-A Data and Index Model Dictionary

## Status and authority

This document records the provider-neutral logical model implemented by the
authorised `S03-A` increment of `STATE-03 DATA_AND_INDEX_MODELING` and its
separately authorised `S03-CORR-01` corrective delta. It is normatively
constrained by the accepted architecture and canonical contracts. Physical
implementation facts are limited to the explicit mapping boundary below; this
document does not grant schema or migration authority.

Accepted ADR-0008 and ADR-0009 define the successor delta for durable
page-image evidence and separate query/document-language domains.
`S03-CORR-01` implements the domain types, runtime-v1 eligibility boundary,
Control schema and migration, persisted vector-language compatibility and
render-manifest reachability slice recorded below. It does not implement the
content/rendering pipeline, real PNG validation or evidence serving.

Later separately authorised `S04-CORR-04-A` to `S04-CORR-04-E` increments
implement verified content storage, rights gates, deterministic render
finalisation and immutable activation-evidence persistence without rewriting
the historical S03-A or S03-CORR-01 evidence. The latest increment implements
the ADR-0010 `AnswerEvidenceRecordV1`, fixed retention and reachability successor
in the existing Control store without changing the public v1 contract.

`S03-B` was blocked when this S03-A artefact was created, so this document
contains no ORM mapping, DDL, migration, persistent store, lockfile change,
dependency selection or package installation. The later S03-B implementation
and evidence are recorded in Current State and their owning report; they did
not rewrite this logical S03-A baseline. The later `S03-CORR-01` physical delta
is recorded separately and leaves the Vector schema unchanged.

## Ownership boundaries

| Layer | S03-A responsibility | Explicit boundary |
|---|---|---|
| Domain | Typed identities, lifecycle values, immutable catalogue/source/index records, digest canonicalisation, activation authority and retention reachability. | No ORM, SQL, filesystem, HTTP, provider SDK, vector-store SDK or transport type. |
| Application | Construction of complete activation-record revisions and fail-closed pre-CAS validation. | No store implementation, transaction API, network access or implicit observation selection. |
| Infrastructure | No S03-A implementation. It remains the future owner of persistence and adapter mappings. | No migration or store may be inferred from this model. |

## Canonical scalar types

| Logical type | Representation | Constraint |
|---|---|---|
| `StableIdentifier` | String | 1–128 ASCII letters, digits, `.`, `_`, `:` or `-`; begins with a letter or digit. |
| `CorpusId` | Lower-case slug | 1–128 lower-case ASCII letters, digits, `.`, `_` or `-`; begins with a letter or digit. |
| Positive revision | Signed 64-bit integer in memory; invariant decimal in canonical text | Greater than zero. Each revision domain is a distinct type. |
| SHA-256 value | String | Exactly 64 lower-case hexadecimal characters. |
| `SupportedQueryLanguage` | Closed value | Exact `pt-BR` or `en-GB` for v1 question and answer contracts. |
| `DocumentContentLanguage` | Canonical BCP 47 string | Locally validated ASCII, 1–128 characters, bounded and never made more specific by inference. |
| `SourceDeclaredLanguage` | Exact observed BCP 47 string plus canonical comparison value | Preserved with publisher/embedded provenance; optional when no declaration exists. |
| `IndexGenerationId` | String | `idxgen-` followed by the complete manifest's 64-character lower-case SHA-256 digest. |
| UTC instant | `DateTimeOffset` | Offset must be zero. Technical exchange uses ISO 8601. |
| Duration | `TimeSpan` | Positive where used for observation `maxAge`. |

Typed identifiers prevent accidental interchange between corpus, database,
category, document, source registration, snapshot, observation, build,
generation, operation, content object and digest domains. Secrets, credentials,
connection strings and workstation identities are not model fields.

## Closed classifications

| Classification | Canonical values | Rule |
|---|---|---|
| `CatalogueItemStatus` | `Candidate`, `Active`, `Deactivated`, `Removed` | `Removed` is a logical tombstone. |
| `DocumentFormat` | `Pdf`, `Csv` | No other document format belongs to the MVP. |
| `SupportedQueryLanguage` | `PtBr`, `EnGb` | Canonical external tags are exactly `pt-BR` and `en-GB`. |
| `SourceTrustClass` | `LocalAuthorised`, `OfficialExternal` | Trust remains explicit provenance; it does not form a separate corpus. |
| `OfficialObservationState` | `Current`, `Stale`, `Withdrawn`, `Deactivated` | Only a `Current` observation inside `maxAge` is eligible. |
| `IndexBuildStatus` | `Candidate`, `Validated`, `Failed` | Only `Validated` with a final manifest is queryable. |

The permitted catalogue transitions are:

```text
Candidate   -> Active
Candidate   -> Removed
Active      -> Deactivated
Deactivated -> Active
Deactivated -> Removed
```

All other transitions fail. In particular, an active item must first be
deactivated before logical removal.

## Catalogue entities

### `DatabaseCategory`

| Field | Type | Null | Invariant |
|---|---|---:|---|
| `id` | `DatabaseCategoryId` | No | Unique within one catalogue snapshot. |
| `displayName` | String | No | Non-blank owner-facing name. |

### `DatabaseProduct`

| Field | Type | Null | Invariant |
|---|---|---:|---|
| `id` | `DatabaseProductId` | No | One current product revision per ID in a catalogue snapshot. |
| `revision` | `DatabaseProductRevision` | No | Positive and independent from catalogue, observation and activation revisions. |
| `displayName` | String | No | Non-blank. |
| `status` | `CatalogueItemStatus` | No | Closed lifecycle value. |
| `categoryIds[]` | Set of `DatabaseCategoryId` | No | At least one, no duplicate assignment, and every ID exists in the same snapshot. |

The category relationship is many-to-many and data-driven. The production
model contains no branch for any named database product.

### `DocumentVersion`

| Field | Type | Null | Invariant |
|---|---|---:|---|
| `id` | `DocumentId` | No | Stable logical document identity. |
| `version` | `DocumentVersionNumber` | No | Positive; unique with `id`. |
| `databaseProductId` | `DatabaseProductId` | No | Names a product in the same catalogue snapshot. |
| `databaseProductRevision` | `DatabaseProductRevision` | No | Exactly matches that snapshot's product revision. |
| `format` | `DocumentFormat` | No | `Pdf` or `Csv`. |
| `contentLanguage` | `DocumentContentLanguage` | No | Canonical BCP 47 tag; only exact `pt-BR` or `en-GB` is eligible for runtime v1. |
| `sourceDeclaredLanguage` | `SourceDeclaredLanguage` | Yes | Exact observed declaration; absent values remain null and no region is inferred. |
| `status` | `CatalogueItemStatus` | No | Independent from the product lifecycle. |
| `contentObjectId` | `ContentObjectId` | No | SHA-256 content-addressed identity for immutable, reopenable bytes. |
| `byteLength` | Positive integer | No | Greater than zero and verified against reopened content before store success. |
| `mediaType` | `ContentMediaType` | No | Canonical bounded ASCII type/subtype without parameters; document ingestion additionally requires a value compatible with `format`. |
| `sourceAdapterId` | `SourceAdapterId` | No | Stable adapter identity, never a dynamic plug-in reference. |
| `sourceTrustClass` | `SourceTrustClass` | No | Preserved into generation and citation provenance. |
| `officialSourceRegistrationId` | `OfficialSourceRegistrationId` | Yes | Forbidden for local content; required for official content. |
| `officialSnapshotId` | `OfficialSnapshotId` | Yes | Forbidden for local content; required before an official version becomes active. |

A logical document has at most one active version in a catalogue snapshot.
Every active document belongs to an active product. Every active product has at
least one active document; removing or deactivating its last active document
therefore requires the product deactivation in the same future transaction.

## Corrective successor model — partially implemented

`S03-CORR-01` split the former language responsibility as follows without
rewriting existing `pt-BR` or `en-GB` values:

| Implemented contract | Field | Invariant |
|---|---|---|
| `SupportedQueryLanguage` | `questionLanguage` / `answerLanguage` | Closed exact values `pt-BR` and `en-GB`; answer equals the accepted question language. |
| `DocumentContentLanguage` | `DocumentVersion.contentLanguage` | Required canonical BCP 47 tag; distinct from query support. |
| `SourceDeclaredLanguage` | `DocumentVersion.sourceDeclaredLanguage?` | Exact observed publisher/embedded tag tied to the existing document/source provenance; `en` is never inferred as `en-GB`. No separate language-evidence schema is introduced. |

Broader document-language values are catalogue data, not code branches or
provider selections. A candidate whose language is outside v1 remains
ineligible for v1 activation until compatible model, dataset, runtime and
planned v2 contracts are separately implemented and verified.

### `DocumentPageImage`

| Field | Type | Null | Invariant |
|---|---|---:|---|
| `documentId` | `DocumentId` | No | Names the immutable source document. |
| `documentVersion` | `DocumentVersionNumber` | No | Exact source version. |
| `sourceContentObjectId` | `ContentObjectId` | No | Verified PDF source bytes. |
| `pageNumber` | Positive integer | No | One-based physical PDF page. |
| `renderProfileId` | Stable identifier | No | Initial accepted value `pdf-page-png-v1`. |
| `rendererDescriptor` | Non-secret descriptor | No | Stable renderer ID/version and canonical settings. |
| `imageContentObjectId` | `ContentObjectId` | No | SHA-256 identity of exact PNG bytes. |
| `imageSha256` | SHA-256 value | No | Equals the image content identity. |
| `byteLength` | Positive integer | No | Bounded and verified on reopen. |
| `mediaType` | String | No | Exactly `image/png`. |
| `widthPixels` / `heightPixels` | Positive integer | No | Each at most 4,096 for the accepted profile. |

### `DocumentRenderManifest`

| Field | Type | Null | Invariant |
|---|---|---:|---|
| `schemaVersion` | Positive integer | No | Versioned canonical schema. |
| `documentId` / `documentVersion` | Typed identity pair | No | Exact PDF version. |
| `sourceContentObjectId` | `ContentObjectId` | No | Matches the rendered source. |
| `sourcePageCount` | Positive integer | No | Equals the complete physical page count. |
| `renderProfileId` | Stable identifier | No | Matches every page binding. |
| `rendererDescriptor` | Non-secret descriptor | No | Matches every page binding. |
| `orderedPageImages[]` | Ordered `DocumentPageImage` | No | Exactly one unique entry for every consecutive page. |
| `manifestSha256` | SHA-256 value | No | Canonical UTF-8 digest over identity/measurement fields excluding `generatedAt`. |
| `generatedAt` | UTC instant | No | Operational evidence outside manifest identity. |

Source and page-image objects share the immutable `ContentObjectId` domain and
the durable `IDocumentContentStore`; catalogue, Git, Git LFS, quarantine and
vector storage are not binary systems of record. A PDF visual-evidence
candidate is complete only after every object and the canonical manifest pass
verified reopen. CSV has no implicit page-image model.

The domain identities, model validation and versioned canonical digest are
implemented. The Control model has empty durable tables for exact manifest,
source-version and page-image bindings. The renderer, content publication,
PNG signature/hash recalculation and verified reopen remain outside
`S03-CORR-01`.

### `S04-CORR-04-A` executable content-object boundary

`S04-CORR-04-A` implements the provider-neutral `IDocumentContentStore`
boundary without adding a database field or changing cleanup authority:

- `PutAndVerifyAsync(BoundedContentInput)` requires a positive caller limit,
  accepts an optional expected SHA-256 identity, writes to same-volume
  quarantine, hashes while writing, flushes, publishes by atomic move, treats
  an identical existing object idempotently and reopens the published object
  before returning success;
- `OpenVerifiedAsync(ContentObjectId, ExpectedHashAndLength)` requires the
  expected SHA-256 and byte length, recomputes the entire object, rejects an
  identity or length mismatch and returns a readable seekable stream at
  position zero;
- `ContentObjectDescriptor` returns the content identity, equal SHA-256,
  verified byte length, validated media type, stable non-secret
  `filesystem-sha256-v1` implementation identifier, write outcome and explicit
  write/reopen verification outcomes; and
- the descriptor and its operation evidence are not new catalogue or Control
  persistence. Existing document and official-snapshot records continue to
  persist their governed identity, length and media type only.

The executable store accepts the typed `image/png` media value, but this
increment creates no renderer, PNG bytes, signature validation or render
manifest persistence. `IStorageMaintenance`, the versioned cleanup plan and
reservation/finalisation protocol remain the sole existing physical-deletion
authority and retain their prior semantics.

### `S04-CORR-04-B` document-rights eligibility boundary

`S04-CORR-04-B` implements a provider-neutral, non-persisted rights contract
for one exact document version:

| Contract field | Type | Invariant |
|---|---|---|
| `schemaVersion` | Positive integer | Exactly `1` for `DocumentRightsEligibilityRecordV1`. |
| `documentId` | `DocumentId` | Exact governed document identity. |
| `documentVersion` | `DocumentVersionNumber` | Exact immutable document version. |
| `decisions[]` | Ordered `DocumentRightDecision` | Contains every schema-v1 right exactly once. |

The closed schema-v1 right set independently covers source possession or
download, parsing and textual transformation, indexing, source-byte retention,
quotation and citation, page rendering, derivative-image creation and
retention, runtime derivative display, source/derivative distribution or
publication, and attribution/notice/trademark/change-marking requirements.
Each decision contains one closed state — `Permitted`, `Denied` or `Unproven`
— and one stable `DocumentRightsEvidenceReference`. The reference carries no
licence text, URL, path, policy authority or persistence semantics.

Application exposes fixed `TextualEvidence` and `PdfVisualEvidence` gates.
The textual gate requires possession/download, parsing/textual transformation,
indexing, source-byte retention, quotation/citation and the attribution/notice
requirement. The PDF visual gate additionally requires page rendering,
derivative creation/retention and runtime display. Only `Permitted` satisfies a
required decision; `Denied` and `Unproven` both block. Distribution/publication
remains an independent decision and is never inferred from textual or visual
eligibility.

This increment adds no catalogue field, Control row, schema, migration or real
rights evidence. It performs no registration, rendering, indexing, activation,
serving, distribution or cleanup operation.

### `CatalogueSnapshot`

| Field | Type | Null | Invariant |
|---|---|---:|---|
| `corpusId` | `CorpusId` | No | MVP uses one configured logical corpus. |
| `revision` | `CatalogueRevision` | No | Immutable generation-bound catalogue revision. |
| `databaseCategories[]` | `DatabaseCategory` | No | Unique category identities. |
| `databaseProducts[]` | `DatabaseProduct` | No | Unique product identities. |
| `documentVersions[]` | `DocumentVersion` | No | Unique `(documentId, documentVersion)` pairs and valid product ownership. |

## Official-source provenance entities

### `OfficialSourceRegistration`

| Field | Type | Null | Invariant |
|---|---|---:|---|
| `id` | `OfficialSourceRegistrationId` | No | Immutable/versioned registration identity. |
| `revision` | `SourceRegistrationRevision` | No | Positive and distinct from other revision domains. |
| `databaseProductId` | `DatabaseProductId` | No | Governed product relationship. |
| `documentId` | `DocumentId` | No | Governed logical document relationship. |
| `sourceAdapterId` | `SourceAdapterId` | No | Stable bounded integration class. |
| `canonicalHttpsUrl` | Absolute URI string | No | HTTPS, default port, no user information and no fragment. The future adapter must additionally enforce the accepted exact allowlist and SSRF controls. |
| `status` | `CatalogueItemStatus` | No | Closed lifecycle value; it does not itself enable egress. |

### `OfficialSourceSnapshot`

| Field | Type | Null | Invariant |
|---|---|---:|---|
| `id` | `OfficialSnapshotId` | No | Immutable snapshot identity. |
| `registrationId` | `OfficialSourceRegistrationId` | No | Names the immutable registration captured. |
| `contentObjectId` | `ContentObjectId` | No | Immutable content hash. |
| `byteLength` | Positive integer | No | Greater than zero. |
| `mediaType` | String | No | Non-blank validated media type. |
| `retrievedAt` | UTC instant | No | Capture time, outside content identity. |

### `OfficialSourceObservation`

| Field | Type | Null | Invariant |
|---|---|---:|---|
| `id` | `OfficialObservationId` | No | Append-only observation identity. |
| `registrationId` | `OfficialSourceRegistrationId` | No | Must match the bound immutable registration. |
| `snapshotId` | `OfficialSnapshotId` | No | Must match the bound immutable snapshot. |
| `journalRevision` | `ObservationJournalRevision` | No | Positive revision in the append-only observation journal. |
| `state` | `OfficialObservationState` | No | Closed freshness state. |
| `revalidatedAt` | UTC instant | No | Never rewritten to revive historical freshness. |
| `maxAge` | Positive duration | No | Eligibility upper bound. |

An official observation is eligible at time `t` exactly when its state is
`Current`, `t >= revalidatedAt`, and `t - revalidatedAt <= maxAge`. Historical
observations remain evidence and are never selected implicitly as “latest”.

## Index-generation entities

### `DocumentBinding`

The first nine fields form the generation-bound source projection. The tenth
field extends it to the complete activation projection.

| Order | Field | Type | Null | Invariant |
|---:|---|---|---:|---|
| 1 | `databaseProductId` | `DatabaseProductId` | No | Generation-bound. |
| 2 | `databaseProductRevision` | `DatabaseProductRevision` | No | Generation-bound. |
| 3 | `documentId` | `DocumentId` | No | Generation-bound. |
| 4 | `documentVersion` | `DocumentVersionNumber` | No | Generation-bound. |
| 5 | `documentFormat` | `DocumentFormat` | No | Generation-bound. |
| 6 | `sourceAdapterId` | `SourceAdapterId` | No | Generation-bound. |
| 7 | `sourceTrustClass` | `SourceTrustClass` | No | Generation-bound. |
| 8 | `officialSourceRegistrationId` | `OfficialSourceRegistrationId` | Yes | Null for local; required for official. Generation-bound. |
| 9 | `officialSnapshotId` | `OfficialSnapshotId` | Yes | Null for local; required for official. Generation-bound. |
| 10 | `sourceObservationId` | `OfficialObservationId` | Yes | Null for local; required for an official activation binding. Activation-bound only. |

Duplicate generation-bound projections are rejected. A single active
document-version projection cannot be represented by two source bindings.

### `FinalisedIndexGenerationManifest`

| Field | Type | Null | Invariant |
|---|---|---:|---|
| `manifestSchemaVersion` | Positive integer | No | Versioned canonical manifest schema. |
| `corpusId` | `CorpusId` | No | Matches the activation record. |
| `corpusRevision` | `CorpusRevision` | No | Immutable corpus specification revision. |
| `catalogueRevision` | `CatalogueRevision` | No | Exact generation-bound catalogue snapshot. |
| `activeDocumentSetDigest` | `ActiveDocumentSetDigest` | No | Ordered product/document projection digest. |
| `sourceBindingSetDigest` | `SourceBindingSetDigest` | No | Ordered nine-field source projection digest. |
| `indexCompatibilityKey` | `IndexCompatibilityKey` | No | Versioned, non-secret compatibility digest. |
| `generationSpecDigest` | `GenerationSpecDigest` | No | Canonical build-specification digest. |
| `chunkCount` | Positive integer | No | Final logical chunk count. |
| `vectorCount` | Positive integer | No | Equals `chunkCount` for the MVP. |
| `logicalArtifactDigest` | `LogicalArtifactDigest` | No | Digest of ordered logical artefacts. |
| `generationContentDigest` | `GenerationContentDigest` | No | Digest of the complete final manifest. |
| `indexGenerationId` | `IndexGenerationId` | No | Exactly `idxgen-` plus `generationContentDigest`. |

`IndexBuildRecord` holds a temporary `CandidateBuildId`, a status and an
optional final manifest. A `Candidate` or `Failed` record has no final
manifest and is not queryable. Only a successful transition from `Candidate`
to `Validated` supplies an immutable manifest and queryable generation
identity. `Active` and `Retained` are projections of activation history, not
independent mutable generation states.

## Canonical digest serialisation

All binding domains use UTF-8 without BOM, fixed field order and invariant
decimal lengths. Bindings are sorted ordinally by the nine generation-bound
fields, with null before non-null. The serializer emits each token as:

```text
<UTF-8 byte length>:<UTF-8 bytes>
```

Null is emitted as `-1:`. Empty strings would be `0:`, but typed identity
constraints reject empty identity fields. The first token is the versioned
domain discriminator, followed by the invariant binding count, followed by
each ordered record's fields without an ambiguous separator.

| Digest | Domain discriminator | Covered fields |
|---|---|---|
| `activeDocumentSetDigest` | `rag-challenge/active-document-set/v1` | Fields 1–5. |
| `sourceBindingSetDigest` | `rag-challenge/source-binding-set/v1` | Fields 1–9; explicitly excludes observation. |
| `activationBindingSetDigest` | `rag-challenge/activation-binding-set/v1` | Fields 1–10; includes observation. |

The executable source and activation golden vectors are stored in
[`binding-digest-golden-v1.json`](../../tests/RagChallenge.UnitTests/TestData/binding-digest-golden-v1.json)
and executed by
[`BindingDigestCanonicalizerTests.cs`](../../tests/RagChallenge.UnitTests/BindingDigestCanonicalizerTests.cs).
They deliberately provide their inputs in non-canonical order and contain both
local null fields and a complete official binding.

## Canonical activation authority

### `CorpusActivationRecord`

| Field | Type | Null | Invariant |
|---|---|---:|---|
| `corpusId` | `CorpusId` | No | Same corpus as the referenced manifest and prior record. |
| `recordRevision` | `ActivationRecordRevision` | No | First revision is 1; later revisions advance by one. |
| `previousRecordRevision` | `ActivationRecordRevision` | Yes | Null only for revision 1; otherwise the immediate predecessor. |
| `indexGenerationId` | `IndexGenerationId` | No | References one validated final manifest. |
| `catalogueRevision` | `CatalogueRevision` | No | Matches the referenced manifest. |
| `activationBindingSetDigest` | `ActivationBindingSetDigest` | No | Recomputed over the complete ordered bindings. |
| `documentBindings[]` | Ordered `DocumentBinding` | No | Non-empty, canonical order, no duplicate generation projection. |
| `generationActivatedAt` | UTC instant | No | Time this generation became current in this lineage. |
| `recordUpdatedAt` | UTC instant | No | Not earlier than `generationActivatedAt`. |

The current activation record is the one control-plane authority. The vector
store has no activation API or independent active alias authority. The
Infrastructure transaction compares the expected revision, persists the whole
new record and sanitised audit event atomically, and preserves the full
previous/new history.

### `DocumentActivationEvidenceBinding`

| Field | Type | Null | Invariant |
|---|---|---:|---|
| `documentBinding` | `DocumentBinding` | No | Exact member of the same activation revision. |
| `sourceContentObjectId` | `ContentObjectId` | No | Matches the exact `DocumentVersion` source object. |
| `rightsSchemaVersion` | Positive integer | No | Exactly `1`; no global rights identity or administrative rights revision is introduced. |
| `rightsDecisions[]` | Ten `DocumentRightDecision` values | No | Complete immutable snapshot with one state and evidence reference for every schema-v1 right. |
| `renderManifestId` | `RenderManifestId` | Conditional | Required for PDF and forbidden for CSV. |

Every new activation revision has exactly one evidence binding for every
document binding. The evidence binding does not participate in
`sourceBindingSetDigest` or `activationBindingSetDigest`; both canonical digest
domains retain their existing projections and semantics. Exact operation replay
compares the complete evidence binding and all rights decisions.

### Revision-domain separation

| Revision | Changes when | Must remain unchanged when |
|---|---|---|
| `CatalogueRevision` | A generation-bound catalogue snapshot changes. | Only an observation is appended or rebound. |
| `ObservationJournalRevision` | An immutable official observation is appended. | Catalogue-only and activation transaction mechanics do not change it implicitly. |
| `ActivationRecordRevision` | A new complete activation authority is proposed, including observation-only rebinding. | It is never reused or rewritten. |
| Future internal transaction version | A physical store performs concurrency bookkeeping. | It must never be exposed as any canonical revision above. |

## Fail-closed pre-CAS validation

Application performs the following independent recomputations before any
compare-and-swap:

1. recompute `activeDocumentSetDigest` from proposed bindings and match the
   final manifest;
2. recompute `sourceBindingSetDigest` from fields 1–9 and match the final
   manifest;
3. recompute `activationBindingSetDigest` from fields 1–10 and match the
   proposed activation record.

It also verifies corpus, generation, catalogue, the required runtime
`IndexCompatibilityKey`, expected revision lineage, exact document/version/
format/source identity, runtime-supported `DocumentContentLanguage`, complete
schema-v1 rights, exact content rows and verified source-object reopen.
For every official binding, the referenced append-only observation must exist
and name the same immutable registration and snapshot. At the evaluation
instant, every active database represented by the record must retain at least
one eligible local or official document binding. A mismatch produces a typed
failure result and grants no authority to change the current record.

CSV requires the complete `TextualEvidence` gate to be `Permitted` and has no
render-manifest binding. PDF requires the complete `PdfVisualEvidence` gate to
be `Permitted`, one finalised render manifest for the same document, version
and source, one consecutive row per physical page, verified reopen of every PNG
and a finalised textual/vector generation whose bindings are identical to its
manifest.

## Rebinding, replacement and rollback invariants

- An observation-only rebind targets one exact official document version and
  requires the same registration and snapshot.
- It advances `recordRevision`, sets `previousRecordRevision`, updates
  `recordUpdatedAt`, changes the selected `sourceObservationId` and recomputes
  `activationBindingSetDigest`.
- It preserves manifest bytes, `indexGenerationId`, `catalogueRevision`,
  generation-bound source digest and `generationActivatedAt`.
- A generation replacement or rollback creates a new complete record revision
  and a new `generationActivatedAt`; it never replays historical activation
  record bytes.
- Initial activation, replacement and rollback explicitly supply every current
  evidence binding. Rollback revalidates current rights, source objects,
  generation and render manifests instead of copying a historical snapshot.
- An observation-only rebind may preserve immutable evidence bindings only when
  document, version, generation and render manifest are identical and only the
  freshness observation changes.
- A rollback generation must differ from the current generation and its
  compatibility key must equal the explicitly required runtime key.
- Rollback inputs explicitly provide every selected official observation.
  Pre-CAS validation requires each to be compatible and currently eligible;
  rollback cannot make an expired, withdrawn or deactivated observation
  current.
- A conflict or validation failure leaves the current record unchanged. The
  Control transaction persists activation record/bindings, evidence bindings,
  rights snapshots, render-manifest links, retention, head, sanitised audit and
  applicable administration-journal completion atomically.

## Retention and recoverability invariants

`GenerationRetentionReference` names a protected generation and a non-empty,
deduplicated set of reopenable `ContentObjectId` values. `RetentionReachability`
protects the active generation and at most one distinct rollback generation.
`S03-CORR-01` additionally traverses every durable render-manifest source and
page-image binding. Physical deletion is permitted only for content unreachable
from every applicable root. No new persistent `AnswerEvidenceRecord` contract
is introduced by this increment.

This logical rule is not evidence of durable storage, readback, backup or
restore. Those require the S03-B physical model and later authorised
integration/recovery evidence.

## Deterministic fixtures

| Fixture | Purpose | Fixed expectations |
|---|---|---|
| [`initial-catalogue-v1.json`](../../tests/RagChallenge.UnitTests/TestData/initial-catalogue-v1.json) | Non-production candidate catalogue. | 51 unique products, 9 categories and 54 assignments; Redis, SAP HANA and SingleStore each have the additional `in-memory` category. |
| [`binding-digest-golden-v1.json`](../../tests/RagChallenge.UnitTests/TestData/binding-digest-golden-v1.json) | Executable canonical digest inputs and outputs. | One local binding, one official binding, source digest excluding observation and activation digest including observation. |

All initial catalogue products remain `Candidate` and have no document bytes.
The fixture is model evidence, not a hard-coded runtime catalogue and not a
real product corpus.

## Physical mapping boundary

The completed S03-B increment mapped the original implemented model under its
own authority and evidence. `S03-CORR-01` then added one Control migration,
`AddDocumentLanguageAndRenderManifestModel`, which:

- broadens `document_versions.content_language` to the validated document BCP
  47 domain and adds nullable `source_declared_language` without backfill;
- preserves existing `pt-BR` and `en-GB` rows exactly;
- creates empty `document_render_manifests` and `document_page_images` tables
  with exact version/source/image foreign keys, profile, media, dimension,
  identity and reproducibility constraints;
- makes render-manifest source and page-image objects durable cleanup roots;
- leaves the Vector schema and `IDocumentContentStore` contract unchanged.

`S04-CORR-04-D` adds the single Control migration
`AddDocumentRightsAndActivationEvidenceBindings`. It creates only
`activation_evidence_bindings` and `activation_rights_decisions`, with exact
activation-binding, document-version/source-object and optional render-manifest
foreign keys plus the closed schema-v1 constraints. It performs no data
operation and does not infer or backfill rights, manifests or bindings for
historical activation rows. The Vector schema remains unchanged.

`S04-CORR-04-E` adds the single Control migration
`20260808033247_AddAnswerEvidenceRecords`. It creates empty
`answer_evidence_records`, `answer_evidence_citations` and
`answer_evidence_pages` tables with exact Control-plane constraints and foreign
keys. It performs no data operation and does not infer or backfill historical
answers or activations. The Vector schema remains unchanged.

The combined mapping preserves these invariants:

- immutable uniqueness for typed identities and version pairs;
- the category many-to-many key;
- one active version per logical document;
- product/document ownership and last-active-document atomicity;
- append-only snapshot, observation and activation histories;
- one current activation authority per corpus with compare-and-swap;
- final-manifest uniqueness and immutable generation identity;
- content reachability, orphan cleanup authority and reopen/readback evidence;
- mappings for the implemented language split, render manifests and page-image
  relationships; `S04-CORR-04-C` now materialises verified page objects and
  finalises those existing records, and `S04-CORR-04-D` now binds them to new
  activation revisions without image serving;
- immutable answer-evidence identity, exact citation/page bindings, fixed
  expiry and independent non-expired reachability roots; and
- migration, recovery, lock and transaction semantics.

No new dependency or store technology was introduced for answer evidence; it
uses the existing SQLite Control boundary.

## S04-CORR-04-C physical finalisation evidence

`S04-CORR-04-C` did not change this dictionary's logical identities or the
physical schema. It implemented the previously mapped render-manifest write
boundary against the existing `document_render_manifests`,
`document_page_images`, `content_objects` and document-version relationships.

The Application-owned finaliser now enforces this order:

1. `PdfVisualEvidence` rights eligibility is explicitly permitted;
2. the source content object is fully reopened against its expected SHA-256 and
   byte length;
3. one isolated renderer worker returns a complete, consecutive page set for
   one exact deterministic descriptor;
4. every PNG is structurally validated before any page object is published;
5. each page object is written and reopened through `IDocumentContentStore`;
6. the canonical manifest and all page bindings are committed in one existing
   SQLite transaction and then read back against their canonical identity.

Exact replay returns `AlreadyApplied`; a different manifest for the same
document version, source object, profile and renderer descriptor returns a
revision conflict. A failed transaction leaves neither a partial manifest nor
partial page rows. Immutable PNG objects created before a later failure may be
orphans, but they grant no deletion authority: `IStorageMaintenance`,
`cleanup-plan-v1` and the reservation/finalisation protocol remain unchanged
and exclusive.

The implemented renderer descriptor binds `pdfium-pdftoimage-v1`,
`PDFtoImage` 5.3.0, PDFium 153.0.7988, SkiaSharp 4.151.1, the effective RID,
`pdf-page-png-v1`, every pixel-affecting option and every enforced limit. It
contains no host name, path, command, credential or workstation version.

This evidence uses only synthetic PDF and PNG bytes. It does not establish a
real document's rights, activate a document or generation, change an activation
digest, serve image evidence or introduce a v2 contract.

## S04-CORR-04-D activation evidence persistence

`S04-CORR-04-D` makes the per-revision source, rights and render-manifest
binding executable without changing the accepted digest domains or the public
v1 contract. The pre-CAS readback validates the exact document source and
generation, applies the fixed CSV/PDF rights gate, reopens the source and, for
PDF, rehydrates the complete finalised render manifest and reopens every page
image. Query activation readback rejects a current revision that lacks any new
binding or whose persisted source, rights or manifest no longer matches.

Historical activation rows are retained exactly in their existing columns and
receive no evidence rows. They remain readable as historical records but are
ineligible to authorise current query or visual readiness. No new rights digest,
rights administration identity, vector metadata field, `AnswerEvidenceRecord`,
v2 contract or image-serving boundary is introduced.

## ADR-0010 answer-evidence successor model — implemented locally

`AnswerEvidenceRecordV1` is an immutable, internal Control-plane aggregate for
one fully validated `Answered` outcome. Its logical header contains the
server-generated `ans-evidence-<uuid-n>` identity, schema version, canonical
record digest, corpus/activation/catalogue/generation identities and both
binding digests, answer hash/UTF-8 length, canonical coverage digest,
query/answer languages, retrieval/prompt/model descriptors, correlation ID and
the fixed `answer-evidence-p30d-v1` creation/expiry instants.

The aggregate owns ordered citation rows and deduplicated page rows. Citation
rows preserve the exact database/document/version/format/language/chunk,
source/provenance/content-object, bounded location and render-manifest binding
used by the response. Page rows preserve the exact cited PDF page's
source/manifest/profile/renderer/image/hash/media/length/dimension tuple. CSV
rows have no render manifest or page rows.

Only `Answered` creates this aggregate, after all response and binding
validation and before the public v1 response. Header, citations, pages and
sanitised audit are one atomic Control transaction. Same identity and digest is
`AlreadyApplied`; reuse of the identity with different canonical content is a
no-change conflict. No question, question hash, answer text, citation text/URL,
prompt/provider payload, score/vector, user/client identity, secret, path or
binary content belongs to the model.

A non-expired aggregate is a reachability root for every referenced source and
page-image content object. `expiresAt` is exactly `createdAt + P30D` and never
refreshes. Expiry removes only this root and does not itself delete anything.
The existing `cleanup-plan-v1` reservation/finalisation boundary must revalidate
all roots before physical deletion. Activation history and rollback roots remain
independent.

The separately authorised `S04-CORR-04-E` implementation maps the aggregate to
the three Control tables named above. Header identity is also the corresponding
administration-operation identity; citations and pages cascade only with their
header, while source and image content-object references remain restrictive.
The store revalidates current activation/generation/source/manifest authority
before insertion and reads back the canonical aggregate plus its allowlisted
audit evidence before commit. The migration contains no backfill or historical
inference. The public v1 schema and OpenAPI artefact remain unchanged; v2 and
image serving are not part of this aggregate.
