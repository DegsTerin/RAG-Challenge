# ADR-0014 — Deterministic Retrieval Ranking and Retrieval-Only Baseline

- Status: proposed
- Date: 2026-08-11
- Preparation authority:
  `AUTH-STATE07-RETRIEVAL-DETERMINISM-ADR-PREP-001`, granted by the product
  owner on clean
  `main@5dbb1cd786785bd1394f45066ebfacc7de674cdc`, corpus `4.10.26`
- Decision authority: none; this proposal has not been accepted
- Owners: RAG-Challenge product, architecture, evaluation, security and
  operations
- State: `STATE-07 TESTING_HOMOLOGATION` documentary proposal only
- Proposed relationship: clarify the retrieval and evaluation semantics in
  [ADR-0002](ADR-0002-RAG-Lifecycle-Providers-And-Source-Separation.md),
  [ADR-0004](ADR-0004-MVP-Corpus-Official-Source-And-Evaluation.md),
  [ADR-0005](ADR-0005-MVP-Providers-Persistence-And-OCI-Deployment.md),
  [ADR-0006](ADR-0006-Security-Egress-Administration-And-HTTP-Contracts.md)
  and the
  [canonical contracts](STATE-02-Canonical-Contracts.md) without superseding
  them
- Protected contract baseline:
  - OpenAPI v1 SHA-256
    `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`,
    Git blob `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`;
  - OpenAPI v2 SHA-256
    `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733`,
    Git blob `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`
- Verification status: documentary and static source inspection only; no
  implementation, executable test, notebook, provider, network, product
  corpus, scored campaign, Automatic Quality Gate or Human Gate was run

## Purpose and authority

This proposal records the smallest governed increment needed to make the
existing `retrieval-v1` ranking contract explicit, reject invalid ranking
states deterministically and establish a retrieval-only baseline before
considering retrieval expansion. It distinguishes a comparative pilot from a
product-representative baseline rather than allowing aggregate volume to imply
representativeness.

The current SQLite implementation already applies the complete ordering
`Score DESC, global ChunkOrdinal ASC` before `top-k` for valid finite scores.
This proposal therefore does not treat the existing algorithm as a
demonstrated non-determinism defect and does not propose an additional
document, digest or provider-specific tie-breaker.

If separately accepted, this ADR would authorise no implementation by itself.
It would define the contract and gate sequence for later, separately
authorised increments. While this ADR remains `proposed`, it changes no code,
configuration, public contract, dataset, generation, provider authority,
gate disposition or lifecycle state.

`retrieval-v1` remains unchanged for valid inputs. The non-canonical
`retrieval-multi-query-v1-candidate` remains parked: it is neither an active
policy nor part of the deterministic baseline defined here.

## Context

### Observed implementation

Static inspection of the preparation baseline established the following
facts:

1. `CorpusIndexingService` orders documents by `DatabaseProductId`,
   `DocumentId` and `DocumentVersion`, flattens their chunks and assigns the
   flattened array index as the persisted `ChunkOrdinal`. The ordinal is
   therefore global within the candidate build, not local to a document.
2. SQLite uses `(CandidateBuildId, ChunkOrdinal)` as the primary key for vector
   chunks. A validated build has a unique `IndexGenerationId`, so an ordinal
   is unique within the generation searched.
3. Finalisation orders logical artefacts by `ChunkOrdinal`, rejects duplicate
   ordinals and includes each ordinal in the canonical logical-artifact
   digest.
4. `SqliteVectorIndexStore.SearchExactAsync` resolves one validated build,
   hard-filters its eligible document bindings, calculates cosine scores and
   applies `Score DESC, ChunkOrdinal ASC` before `Take(MaximumResults)`.
5. The implemented `QuestionAnsweringService` requests eight vector hits,
   preserves their order, skips passages that exceed the remaining
   16,000-scalar evidence budget and selects at most six passages. The only
   concrete host composition of that service passes `minimumScore: 0.25`; the
   default non-integration host uses `DisabledQuestionAnsweringService`. The
   code emits `retrieval-v1`, but does not yet encode `0.25` in an implemented
   minimum-score-policy identifier or manifest.
6. The existing `IVectorIndexStore` implementation returns a raw list. Its
   Application port does not declare the ordering, finite-score guarantee or
   expected failure outcomes, although the canonical STATE-02 contract
   already describes a typed `VectorSearchResult` and a versioned minimum-
   score policy.
7. Query vectors are checked for finite float components, but an all-zero
   vector reaches the store as an argument failure. Decoded stored vectors,
   calculated norms and calculated scores are not all revalidated as finite
   before ranking. A non-finite score is not valid cosine evidence and must
   not be converted silently into an empty or insufficient-evidence result.
8. Existing tracked tests cover functional ranking, isolation and the
   synthetic 10,000-by-1,536 fixture; no executable test was run under this
   preparation authority. The tracked suite does not contain a focused proof
   of an exact tie at the `top-k` boundary across multiple documents, changed
   write batches, store reopen and invalid numerical states.

These facts are visible in the current
[indexing service](../../src/RagChallenge.Application/IndexingRetrieval/IndexingServices.cs),
[generation canonicaliser](../../src/RagChallenge.Domain/IndexingRetrieval/IndexGenerationCanonicalizer.cs),
[SQLite mapping](../../src/RagChallenge.Infrastructure/Persistence/VectorStoreDbContext.cs),
[SQLite vector store](../../src/RagChallenge.Infrastructure/Persistence/SqliteVectorIndexStore.cs),
[Application persistence contracts](../../src/RagChallenge.Application/Persistence/PersistenceContracts.cs)
and
[query service](../../src/RagChallenge.Application/IndexingRetrieval/QueryServices.cs).

### Evaluation boundary

The accepted dataset ID is `rag-eval-catalogue-v1`. The current frozen
candidate contains zero scored product documents and zero scored product
cases; its 11 cases are synthetic fixtures. The later provider-preparation
revision also contains zero product documents and zero product cases; its 60
cases are synthetic and unscored. Neither revision can establish product
retrieval quality or serve as the representative retrieval-only baseline.

The current query service keeps retrieval selection private inside the
answer-generation use case. Measuring only raw vector-store hits would omit
eligibility validation, the minimum score, the scalar budget and the final
evidence limit. Driving the whole answer path would introduce an unnecessary
language-model call. A typed internal pre-generation boundary is therefore
required.

## Decision drivers

- Preserve valid-input behaviour already emitted as `retrieval-v1`.
- Make `top-k` independent of storage enumeration and write-batch order.
- Reject invalid numerical or identity states before they can resemble a
  successful no-evidence outcome.
- Use one Application-owned retrieval implementation for product query and
  retrieval-only evaluation.
- Keep the public OpenAPI v1 and v2 contracts byte-for-byte unchanged.
- Bind every scored result to one immutable corpus, generation, policy,
  embedding descriptor, query vector, dataset revision and environment.
- Preserve the accepted Recall@5 and MRR@5 threshold values while proposing an
  explicit scorer and prohibiting threshold or scorer selection after seeing a
  result.
- Keep synthetic contract evidence separate from product quality evidence.
- Obtain a measured `retrieval-v1` baseline before evaluating MultiQuery or
  another retrieval expansion.

## Proposed decision

### Total ranking contract

For one validated `CandidateBuildId` and its unique `IndexGenerationId`, a hit
`a` precedes a hit `b` exactly when:

1. `a.Score` is numerically greater than `b.Score`; or
2. the finite scores compare equal and `a.ChunkOrdinal` is numerically less
   than `b.ChunkOrdinal`.

In compact form, the ranking key is:

```text
Score DESC, global ChunkOrdinal ASC
```

The following rules complete that contract:

- every ranked score is a finite `double` in the closed interval `[-1, 1]`;
- every persisted vector component is a finite `float`;
- the query vector has the configured dimensions, finite components and
  non-zero finite magnitude;
- a zero-magnitude stored vector preserves the current valid behaviour and
  receives score `0`; only a zero-magnitude query vector is invalid;
- `ChunkOrdinal` is non-negative and unique within the candidate build;
- global ordinals are assigned before persistence from the deterministic
  flattened chunk sequence and are part of the generation's logical-artifact
  digest;
- required corpus, generation, eligible-binding and administrative filters
  are applied before scoring and `top-k`;
- sorting is complete before `Take(k)`;
- no score epsilon, bucketing, rounding or provider-specific tolerance creates
  an implicit tie; and
- no tertiary key is applied after `ChunkOrdinal`. A repeated ordinal is an
  invalid index state, not a reason to mask the violation with another field.

For valid current inputs these rules describe the behaviour already
implemented. They do not change the returned order.

### Numerical and contract validation

A future implementation increment must validate, without changing the current
cosine arithmetic for valid values:

- the query vector and its norm before search;
- every decoded stored vector and its norm;
- every calculated score before constructing a successful hit;
- hit count, identity, unique global ordinal and strict lexicographic
  `(Score DESC, global ChunkOrdinal ASC)` order at the Application boundary;
  equivalently, scores are non-increasing and equal-score runs have strictly
  increasing global ordinals; and
- the selected-evidence order after threshold and scalar-budget processing.

Changing multiplication precision, score normalisation, cosine calculation,
rounding or another numerical ranking semantic is not part of the smallest
increment. Such a change can alter valid rankings and follows the successor
rules below.

If current arithmetic produces a finite score outside `[-1, 1]` for otherwise
admissible vectors, the implementation increment stops. It must not introduce
an epsilon, clamp or arithmetic change without a separately versioned
numerical-semantics decision.

### Typed outcomes and fail-closed mapping

Expected retrieval conditions must cross the Application boundary as typed
outcomes rather than adapter-specific exceptions. A raw vector-search
`Succeeded` result may contain zero hits; the post-policy executor outcome is
then distinct and cannot also be `Succeeded`. The minimum disjoint executor
classification is:

| Retrieval condition | Typed internal outcome | Existing query disposition |
|---|---|---|
| Valid ordered result with at least one passage selected by the complete policy | `Succeeded` | Continue to grounded answer generation |
| No passage is selected by the complete policy | `NoSelectedEvidenceUnderPolicy` with `NoRawHits`, `BelowMinimumScore` or `ScalarBudgetExcludedAll` | Completed `InsufficientEvidence` |
| Provider vector is malformed, non-finite or zero magnitude | `InvalidQueryVector` | `EmbeddingUnavailable` / `CH_EMBEDDING_UNAVAILABLE` |
| Generation is absent or not validated, or its manifest `IndexCompatibilityKey` differs from the frozen expected key | `GenerationUnavailable` | `IndexUnavailable` / `CH_INDEX_UNAVAILABLE` |
| Stored vector, norm, score, ordinal or identity is invalid | `InvalidIndexData` | `IndexUnavailable` / `CH_INDEX_UNAVAILABLE` |
| Adapter violates count or ordering guarantees | `ContractViolation` | `IndexUnavailable` / `CH_INDEX_UNAVAILABLE` |
| Retrieval-policy version, limits, expected descriptor or expected compatibility identity is absent or internally inconsistent | `InvalidConfiguration` | `ConfigurationInvalid` / `CH_CONFIGURATION_INVALID` |
| Cancellation is observed | `OperationCancelled` | `OperationCancelled` / `CH_OPERATION_CANCELLED` |
| An unclassified fault occurs | `UnexpectedFailure` | `UnexpectedFailure` / `CH_UNEXPECTED_FAILURE` |

The empty-selection reason is selected after ordered stages: `NoRawHits` when
the raw count is zero; otherwise `BelowMinimumScore` when the threshold leaves
zero items; otherwise `ScalarBudgetExcludedAll` when threshold-eligible items
exist but none fits the scalar budget. An invalid score or contract violation
never becomes `NoSelectedEvidenceUnderPolicy`/`InsufficientEvidence`. No new
public error code or OpenAPI shape is required.

### Internal retrieval-only port

Application should own one internal port, provisionally named
`IRetrievalPolicyExecutor`:

```text
ExecuteAsync(RetrievalPolicyRequest, CancellationToken)
  -> RetrievalPolicyResult
```

`RetrievalPolicyRequest` must bind at least:

- corpus ID and the one resolved activation identity;
- the corresponding finalised generation manifest, including
  `IndexGenerationId`, `IndexCompatibilityKey` and generation-manifest digest;
- the eligible generation-bound selectors and exact
  `sourceBindingSetDigest`/`activationBindingSetDigest` that the executor may
  enforce but cannot expand;
- the query vector, its exact representation and SHA-256 identity; only an
  evaluation manifest makes that vector frozen;
- expected embedding provider/model/revision/dimensions;
- declared question language, authorised database/document filters and the
  exact eligibility-policy identity. That identity is currently
  `QueryContractVersion`; a future decoupled eligibility policy requires its
  own version rather than silently replacing this binding;
- retrieval-policy and minimum-score-policy versions; and
- the fixed policy limits applicable to the request.

`RetrievalPolicyResult` must expose, without full document logging:

- one typed outcome;
- the ordered vector `top-k` identities and finite scores;
- the ordered selected-evidence identities after the complete policy;
- corpus, activation, finalised generation, validated
  `IndexCompatibilityKey`, eligibility-policy and retrieval-policy identities;
- generation-manifest, `sourceBindingSetDigest`,
  `activationBindingSetDigest`, query-vector and policy-manifest digests; and
- sanitised failure identity when no successful result exists.

The product query path must call this same executor before grounded answer
generation. A retrieval-only harness must stop at its result and must not call
the language model. Infrastructure remains responsible for exact SQLite
search, but it cannot define competing Application outcomes or policy
versions.

### Frozen `retrieval-v1` semantics

If separately accepted and materialised, the first retrieval-policy manifest
would newly bind the following semantics:

| Field | `retrieval-v1` value |
|---|---|
| Ranking | `Score DESC, global ChunkOrdinal ASC` |
| Vector search | exact cosine over the eligible validated generation |
| Vector `top-k` | `8` |
| Minimum-score policy | proposed `minimum-score-v1`; not currently implemented |
| Minimum score | `0.25`, inclusive; observed in the only concrete enabled host composition and proposed to become normative |
| Maximum selected evidence | `6` |
| Maximum selected evidence size | `16,000` Unicode scalar values |
| Invalid ranking state | typed fail-closed result; never insufficient evidence |

This proposal does not claim that the current generic constructor parameter is
already a versioned `0.25` contract. Acceptance would establish that
architecture semantic; implementation, readiness and evidence would remain
future work. The policy-manifest digest is evaluation evidence. It does not add
a public response field in this proposal. `retrievalPolicyVersion` remains the
public and answer-evidence identity already carried by the implemented
contracts.

## Versioning and compatibility

`retrieval-v1` remains the version for a future increment that documents,
validates and proves the successful ordering already implemented and rejects
only states outside the valid contract. A previously admitted invalid
generation is blocked, quarantined or rebuilt; it is neither served as a
successful `retrieval-v1` result nor migrated silently.

The following versioning rules apply:

| Change | Required identity consequence |
|---|---|
| Documentation, typed outcomes or validation that rejects only invalid contract states and leaves every valid successful ordered result unchanged | Retain `retrieval-v1` |
| Tie-break key, score comparison, score tolerance, threshold, vector `top-k`, scalar budget, selected-evidence limit or filter semantics changes | New retrieval-policy version |
| Application-only selection policy changes without changing stored vectors or exact-search semantics | New retrieval-policy version; existing compatible generation may remain usable |
| Distance metric, vector normalisation, score arithmetic, vector representation or index algorithm changes | New retrieval-policy version; explicitly advance the concrete vector-store descriptor consumed by `IndexCompatibilityProfile`, producing a new `IndexCompatibilityKey`, generation and evaluation baseline |
| Parser, chunker, embedding model/revision/dimensions or another existing compatibility input changes | New `IndexCompatibilityKey`, new generation and new evaluation baseline under existing accepted rules |
| Evaluation schema, scorer, cut-off, formula, aggregation, threshold or sampling rule changes after design freeze | New evaluation-design-contract revision |
| Dataset case, qrel, normalised original question, eligible selector or materialisation identity changes after materialisation freeze, whether or not a result exists | New immutable dataset revision and new campaign |
| Policy-specific query, expansion or vector input, provider identity or execution environment changes after campaign-input freeze | New campaign-input-manifest revision and campaign; the unchanged dataset revision may be reused |

Two observably different valid-input rankings must never share
`retrieval-v1`. A successor name is selected only in the separately authorised
decision that introduces the changed semantic.

`retrieval-multi-query-v1-candidate` remains a parked, non-canonical candidate
name. It is not a successor version, must not appear as an active response
policy and cannot reuse `retrieval-v1` results as evidence of its own quality.
In a future separately authorised paired comparison, it must use exactly the
same frozen dataset revision, intended case denominator, qrels, corpus and
generation. It requires its own policy-specific campaign inputs and results;
paired deltas join by `caseId`, and technical or quality failures never remove
a case from either denominator.

## Retrieval-only evaluation boundary

### Primary boundary

The primary baseline is labelled:

```text
retrieval-v1@frozen-query-vector
```

It begins after the query vector, activation, finalised generation and eligible
selectors have been resolved and frozen and ends at the ordered selected-
evidence result immediately before language-model generation. It therefore
measures enforcement of:

- the one frozen activation and eligible binding set;
- hard pre-filtering;
- exact vector ranking and total tie-break;
- `top-k=8`;
- inclusive proposed `minimumScore=0.25`;
- the 16,000-scalar budget; and
- the maximum of six selected evidence items.

It does not measure activation resolution, freshness resolution or creation of
the eligible projection. Those require separate integration evidence. Cases
for different `QueryContractVersion`/eligibility-policy identities are never
mixed in one metric denominator.

The accepted-threshold retrieval metrics defined below are scored against the
first five items of that final selected-evidence sequence, not against raw
vector-store hits. Raw-vector recall remains a diagnostic. No post-result
sorting or canonicalisation is allowed to repair an adapter result.

The frozen vector must have been produced for the exact normalised question by
the named embedding provider/model/revision and dimensions under separate
authority. Its exact float32 bytes and SHA-256 digest become campaign inputs.
Once frozen, deterministic repetitions make no provider or paid call.

### Complementary provider-inclusive boundary

A later provider-inclusive retrieval lane may measure normalisation, query
embedding and the same policy executor together. It requires explicit account,
secret-reference, egress, provider and spend authority. Its cases, failures,
latency and cost remain a separate denominator and never replace the frozen-
vector determinism result.

Neither boundary calls the answer language model. End-to-end grounded-answer
quality remains owned by its existing evaluation campaign.

## Relevance model and formulas

### Atomic judgement and eligibility identity

Each `JudgementRecord` binds one case and one exact evidence unit, independently
of whether that unit is relevant. A grade-0 record has no positive association.
A grade-1 or grade-2 record has at least one separate
`PositiveEvidenceAssociation`; every such association binds one required fact,
and every grade-2 association also binds one relevant source-location group.
Fields may be referenced through an immutable frozen manifest instead of
repeated in every row, but the resolved join must yield at least:

```text
JudgementRecord:
judgementRecordId
evaluationDesignContractRevision
datasetRevision
caseId
catalogueRevision
corpusId
activationRevision
sourceBindingSetDigest
activationBindingSetDigest
indexGenerationId
indexCompatibilityKey
databaseProductId
databaseProductRevision
documentId
documentVersion
documentFormat
contentLanguage
sourceAdapterId
sourceTrustClass
officialSourceRegistrationId or Local sentinel
officialSnapshotId when applicable
globalChunkOrdinal
chunkDigest
exactSourceLocation (PDF page/region or CSV row/column identity)
relevanceGrade
eligibilityDisposition
reviewerOneDecision
reviewerTwoDecision
adjudicationDecision

PositiveEvidenceAssociation when relevanceGrade is 1 or 2:
judgementRecordId
requiredFactId
relevantLocationGroupId when relevanceGrade is 2
```

The only relevance grades are:

- `2`: direct evidence sufficient for the associated required fact;
- `1`: useful context that is insufficient by itself; and
- `0`: judged not relevant.

`eligibilityDisposition` is separately `Eligible` or `Ineligible`. Returning
an ineligible item is leakage and a hard failure, regardless of its relevance
grade. A grade-0 judgement is therefore complete without a fact or location
group and counts as judged for `JudgementCoverage@5`. If one chunk supports
more than one required fact, it has one explicit positive association per fact;
the fact identity is never inferred from a shared chunk alone.

Overlapping or equivalent chunks that point to the same canonical source
location share one `relevantLocationGroupId`. Distinct relevant source
locations remain distinct groups even when they support the same fact. This
prevents chunk overlap from inflating the denominator without collapsing
independent evidence locations.

Negative cases are labelled separately as either:

- `NoEvidenceInEligibleCorpus`, supported by source-first review of the frozen
  eligible projection; or
- `EvidenceOnlyOutsideEligibleProjection`, where evidence exists only outside
  the frozen corpus, generation, database, document or filter projection.

### Selected-evidence relevant-location Recall@5

For positive case `i`, let `T_i^5` be the first five final selected-evidence
items and `L_i` be its non-empty set of eligible grade-2 relevant source-
location groups:

```text
SelectedEvidenceRelevantLocationRecall@5_i =
  count(groups in L_i represented by a grade-2 association in T_i^5)
  / count(L_i)
```

The cut-off `5` belongs exclusively to the evaluation design contract and
`retrieval-evaluation-scorer-v1`, not to the retrieval-policy manifest. The
proposed scorer applies the accepted numeric threshold values to this selected-
evidence metric: at least `0.90` over the simple case macro, and at least `0.85`
separately for every reportable database stratum, every reportable source
stratum and every mandatory question-language × exact-content-language row.
ADR-0014 proposes the previously unspecified location-group, language-row and
aggregation semantics; earlier results calculated with another formula are not
silently comparable.

### Selected-evidence MRR@5

Let `rank_i` be the first rank from 1 to 5 whose selected item has an eligible
grade-2 association to any group in `L_i`:

```text
SelectedEvidenceMRR@5_i = 1 / rank_i, when rank_i <= 5
SelectedEvidenceMRR@5_i = 0, otherwise
```

The proposed scorer applies the accepted numeric threshold value of at least
`0.75` separately to every reportable database stratum, every reportable source
stratum and every mandatory question-language × exact-content-language row.

### Diagnostic retrieval measures

The following measures are diagnostic and have no acceptance threshold in
this proposal:

- `RawVectorRelevantLocationRecall@5`, using the first five ordered raw vector
  hits and the same `L_i` denominator;
- `RequiredFactGroupCoverage@5`, equal to required facts having at least one
  selected grade-2 association divided by all required facts for the case;
- `SelectedEvidencePrecision@5`, equal to selected ranks 1 through 5 having an
  eligible grade-2 association divided by `5`, with absent ranks contributing
  zero; and
- `nDCG@5` over eligible grades 0, 1 and 2, with absent ranks at grade 0:

```text
DCG@5 = sum from rank r=1..5 of (2^grade_r - 1) / log2(r + 1)
nDCG@5 = DCG@5 / IDCG@5
```

A zero ideal DCG produces `nDCG@5 = 0` and is reported only in the separate
negative-case denominator.

### Aggregation

- the canonical overall Recall@5 result is the simple arithmetic mean over all
  positive cases, so every frozen case has equal weight;
- mandatory question-language × exact-content-language rows, database strata,
  source strata, format, contract-version and filter strata remain separately
  visible and are never collapsed into a combined `database/source` bucket;
- translated variants share one `semanticCaseGroupId`, but averaging variants
  within that group before averaging groups is a diagnostic semantic-concept
  macro and does not replace the canonical case macro;
- positive, `NoEvidenceInEligibleCorpus` and
  `EvidenceOnlyOutsideEligibleProjection` denominators remain separate;
- cases governed by different `QueryContractVersion` or eligibility-policy
  identities remain in separate denominators; and
- an underpowered mandatory stratum is never merged or omitted to obtain a
  pass.

## Minimum metrics and deterministic replay

The retrieval-only campaign must report at least:

| Measure | Required disposition |
|---|---:|
| Raw ordered top-k `DigestMatchRate` | `1.00` |
| Ordered selected-evidence `DigestMatchRate` | `1.00` |
| `FiniteScoreRate` | `1.00` |
| `ExecutionCompleteness` | `1.00` |
| `JudgementCoverage@5` | `1.00` |
| `ContractMatrixCoverage` | `1.00` |
| `ContractFixturePassRate` | `1.00` |
| `LeakageMatrixCoverage` | `1.00` |
| `LeakageCaseRate` | `0` |
| `NegativeEmptySelectionRate` | at least `0.95` overall, per negative subtype and per supported question language |
| `SelectedEvidenceRelevantLocationRecall@5` for positive product cases | at least `0.90` overall and `0.85` separately per reportable database stratum, reportable source stratum and mandatory language row |
| `SelectedEvidenceMRR@5` for positive product cases | at least `0.75` separately per reportable database stratum, reportable source stratum and mandatory language row |

The deterministic, coverage and negative-selection dispositions preceding
Recall@5 and MRR@5 are new hard gates proposed by ADR-0014; they are not
inherited thresholds from ADR-0004. The `0.95` negative-selection value is a
new retrieval-only threshold and does not satisfy the separate end-to-end
insufficient-evidence threshold. Applying the accepted Recall@5/MRR@5 numbers
to mandatory language rows is also part of this proposed scorer definition.
The normative formulas are:

```text
DigestMatchRate =
  replay comparisons whose ordered digest equals the reference digest
  / all planned replay comparisons

FiniteScoreRate =
  finite scores in all raw returned hit records for frozen quality cases
  / all scores in those raw returned hit records

ExecutionCompleteness =
  frozen quality cases reaching Succeeded or NoSelectedEvidenceUnderPolicy
  / all frozen quality cases

JudgementCoverage@5 =
  judged items in all final selected-evidence top-five sequences
  / all items in those final selected-evidence top-five sequences

ContractMatrixCoverage =
  populated required contract-fixture cells
  / all required contract-fixture cells

ContractFixturePassRate =
  frozen contract fixtures returning their exact expected typed outcome
  / all frozen contract fixtures

LeakageMatrixCoverage =
  populated required eligibility-boundary cells
  / all required eligibility-boundary cells

LeakageCaseRate =
  adversarial eligibility cases returning at least one ineligible item
  / all adversarial eligibility cases

NegativeEmptySelectionRate =
  frozen negative quality cases returning NoSelectedEvidenceUnderPolicy
  / all frozen negative quality cases
```

Before either pass rate can be calculated, the design contract freezes a
minimum matrix. Contract fixtures include at least one cell for `Succeeded`,
each `NoSelectedEvidenceUnderPolicy` reason and every typed failure outcome.
They also include distinct cells for malformed dimensions, non-finite and zero-
magnitude query vectors; non-finite stored components or norms; non-finite and
finite-out-of-range scores; valid stored zero-vector score `0`; generation
absent, not validated and compatibility-key mismatch; duplicate ordinal;
identity, count and composite-order violations; missing/inconsistent policy
configuration; cancellation; and injected unexpected failure. Eligibility
fixtures include at least one adversarial exclusion at each corpus, generation,
database and document boundary in each supported question language. Additional
filter classes add cells rather than replacing these minimum ones.

A zero denominator for any required hard rate is `NOT_EVALUABLE` and fails its
gate. An empty individual selected-evidence sequence contributes no item to
either side of `JudgementCoverage@5`; only zero selected items across the whole
quality denominator makes that measure `NOT_EVALUABLE`. Fixtures that expect
`InvalidIndexData`, `ContractViolation` or another typed technical outcome use
the separate contract-fixture denominator and never reduce quality-campaign
`ExecutionCompleteness`. For this rate, `Succeeded` and
`NoSelectedEvidenceUnderPolicy` are non-technical terminal dispositions;
retrieval-quality and leakage metrics decide whether the result is good.
Absence of any mandatory contract or leakage matrix cell is `NOT_EVALUABLE`,
even if every populated fixture passes.

A technical failure in a positive quality case contributes zero to both
accepted-threshold retrieval metrics and fails execution completeness. A
technical failure in a negative quality case never counts as a correct empty-
selection disposition.

### Canonical digest representation

Both ordered-result digests use
`retrieval-result-canonicalisation-v1`. The corresponding manifest binds that
version and the ordered schema. The byte stream begins with the ASCII domain
`RAG-CHALLENGE\0retrieval-result-canonicalisation-v1\0`. Each subsequent field
in schema order is encoded as:

```text
uint32_le(field-name UTF-8 byte length)
|| field-name UTF-8 bytes
|| uint8(type tag)
|| uint64_le(payload byte length)
|| payload bytes
```

Text and enum payloads use their exact case-sensitive frozen UTF-8 bytes;
canonicalisation performs no locale conversion or implicit Unicode
normalisation. Type tags are `0x01` UTF-8 text, `0x02` UTF-8 enum, `0x03`
unsigned 32-bit integer, `0x04` unsigned 64-bit integer, `0x05` opaque bytes,
`0x06` ordered `float32` sequence and `0x07` `float64`. Non-negative ranks,
counts and ordinals use fixed-width unsigned little-endian payloads declared by
the schema. The query-vector digest uses the original component order with each
IEEE-754 `float32` bit pattern encoded as four little-endian bytes. Every
result score uses its exact IEEE-754 `double` bit pattern encoded as eight
little-endian bytes. Numerically equal `+0` and `-0` are tied for ranking and
therefore resolved by global ordinal, while their different bit patterns remain
different in the digest. SHA-256 is applied to the complete domain-separated
stream. Passage text is never included.

The raw-result stream binds the case, policy, eligibility policy, exact query-
vector digest, corpus, activation, finalised generation, compatibility key,
filters, terminal outcome or failure reason, hit count and each rank, evidence
identity and score. The selected-evidence stream additionally binds the
selection limits and the selected ranks. Any schema or encoding change
requires a successor canonicalisation version.

Each frozen-vector case runs at least twice consecutively in the same process,
followed by one complete cold-start replay. The first run is the reference;
the second and cold-start runs are planned comparisons. `Cold-start` means a
new process reopening the same frozen persisted stores. All ordered digests
must match exactly. Focused contract tests additionally permute document input,
write batches and storage enumeration while preserving the same finalised
logical generation and identities.

The following are also reported without a new threshold until a separate
decision pre-registers one:

- `PositiveEmptySelectionRate`;
- `RawVectorRelevantLocationRecall@5`,
  `RequiredFactGroupCoverage@5`, `SelectedEvidencePrecision@5` and `nDCG@5`;
- the diagnostic semantic-case-group macro;
- retrieval-only p50, p95 and p99 latency;
- candidate and selected-evidence counts; and
- score distributions and distance from the inclusive threshold.

The accepted end-to-end insufficient-evidence threshold and 12/20-second query
thresholds remain separate requirements. Passing the proposed retrieval-only
`NegativeEmptySelectionRate` does not establish end-to-end insufficient-
evidence behaviour.

## Two-stage dataset

### Stage 1 — design freeze

Create a future immutable evaluation-design-contract revision, provisionally
exemplified as `retrieval-v1-evaluation-design-v1`. It is not a dataset
revision and does not reuse the `rag-eval-catalogue-v1` revision namespace.

The design contract remains `unmaterialised` and `unscored`. It contains zero
product documents, zero product cases, no qrels, no query vectors and no
`IndexGenerationId`. It freezes:

- schemas, `retrieval-result-canonicalisation-v1` and scorer versions;
- qrel association identity, relevance grades and eligibility dispositions;
- relevant-location, required-fact and semantic-case-group rules;
- metric formulas, aggregation and acceptance thresholds;
- sampling, negative subtype, reportability and stratum rules;
- question-normalisation identity and byte-level freeze rules;
- failure, zero-denominator and missing-judgement dispositions;
- mandatory contract-fixture and eligibility-boundary matrix cells;
- unscored pooling, independent-review and adjudication rules;
- the policy-specific campaign-input manifest schema;
- required artefacts; and
- the ordered gate sequence below.

Any change after the design freeze creates a new design-contract revision.
Unscored source-first authoring and pooling may then materialise candidates
under that contract, but no scored execution result may be used to complete or
alter the frozen design.

The current 11-case and 60-case synthetic revisions remain immutable
historical evidence. They are not rewritten or promoted into the product
denominator.

The retrieval-only subset also does not by itself satisfy the complete S07-A
inventory for citation-boundary, extrapolation, prompt-injection, provenance or
end-to-end answer evaluation. Those remain separate denominators and gates.

### Stage 2 — materialisation freeze

After an authorised product corpus and validated active generation exist, a
new immutable product dataset revision under the stable
`rag-eval-catalogue-v1` ID binds before its first scored run:

- repository commit and prompt corpus version;
- design-contract revision and dataset, case, qrel, sampling-plan, rubric,
  scorer and canonicalisation digests;
- the exact intended product-case denominator;
- catalogue revision, corpus ID, activation revision, generation ID,
  `IndexCompatibilityKey`, generation-manifest digest,
  `sourceBindingSetDigest` and `activationBindingSetDigest`;
- every eligible document ID, version, format, content hash, exact
  `contentLanguage`, source-declared language, adapter and trust class;
- parser, chunker, embedding and vector-store compatibility descriptors;
- each case's exact normalised-question bytes and digest, question-
  normalisation version, `QueryContractVersion`, eligibility-policy identity,
  authorised selectors and filters;
- the unscored judgement-pool systems, versions, depth and pool digest;
- a declaration that evaluation questions, qrels and expected facts are
  absent from the runtime corpus and release artefact.

Unscored pooling and adjudication finish before this materialisation freeze;
scored execution begins only after the subsequent campaign-input freeze. Any
change to a Stage 2-bound field after materialisation freeze creates a
successor dataset revision and campaign, even when no result has yet been
observed. A failed or incomplete predecessor remains immutable evidence.

### Policy-specific campaign-input freeze

The materialised dataset remains policy-independent. After Stage 2 and before
any scored execution, each policy receives a separate immutable campaign-input
manifest that binds:

- campaign ID, dataset revision/digest and the exact full intended `caseId`
  denominator;
- retrieval-policy and minimum-score-policy manifests and digests;
- each baseline query vector's exact original-order IEEE-754 `float32` little-
  endian bytes, digest and embedding provider/model/revision/dimensions;
- any policy-specific normalised expansion query and vector, with its complete
  generation identity and digest, when a future policy separately authorises
  such expansion;
- environment, runtime, OS and architecture placeholders without a real host
  name; and
- repetitions, commands, network policy and task-owned evidence paths.

The `retrieval-v1` manifest contains exactly the original-question vector for
each case. A future MultiQuery comparison uses another campaign-input manifest
for its additional queries/vectors, while both policies execute independently
over the identical dataset revision and intended `caseId` set. A paired report
joins results by `caseId`; a failure remains in both the policy denominator and
the paired-delta disposition. Previously observed baseline results are not
substituted for the independently executed baseline side of that paired
campaign.

Any change after campaign-input freeze creates a new campaign-input-manifest
revision and campaign, even before a result exists. It does not require a new
dataset revision when every Stage 2 identity is unchanged. The parked
`retrieval-multi-query-v1-candidate` receives no manifest or execution authority
from this proposal.

### Sampling floor

ADR-0004's no-fixed-total rule remains: the dataset grows with every active
database, document, source and format. The following simultaneous invariants
define a comparative pilot baseline:

```text
N_positive >= 200
N_negative >= max(50, ceiling(0.25 * N_positive))

for every negative subtype n:
  negative(n) >= 25

for every supported question language q:
  negative(q) >= 25

for every active database d and supported question language q:
  positive(d, q) >= 2
  negative(d, q) >= 1

for every active document a and supported question language q:
  positive(a, q) >= 1

for every mandatory question-language x exact-content-language row l:
  positive(l) >= 25

for every active format f:
  positive(f) >= 25

for every required source stratum s:
  positive(s) >= 25
```

The supported question languages are exactly `pt-BR` and `en-GB`; both are
mandatory regardless of the evidence language. A mandatory language row is
each supported question language crossed with each exact `contentLanguage`
present in the eligible active documents. A source stratum is the exact tuple
`(SourceTrustClass, SourceAdapterId,
OfficialSourceRegistrationIdOrLocalSentinel)`. One case may satisfy several
different quota families. Within one family it is counted once under its
frozen primary database, document, question-language, language-row, format or
source-stratum assignment; additional qrel associations do not double-count it
within that quota family.

Passing these pilot quotas permits only the label `COMPARATIVE_PILOT`. A
product-representative or product-homologation claim additionally requires at
least 30 positive cases for every active database and every required source
stratum, with both supported question languages represented. The 30-case floor
is an operational reportability rule, not a statistical-power claim. Any
mandatory database or source stratum below it is labelled `UNDERPOWERED` and
blocks product representativeness and homologation even when aggregate metrics
pass. For example, 51 active database strata imply at least 1,530 positive
cases from the database floor alone.

These floors are not statistical proof for all future questions and are not a
catalogue ceiling.

### Ground-truth control

- Cases are authored source-first, not only from results retrieved by the
  candidate under test.
- Negative cases include both declared subtypes and record the review evidence
  supporting absence from, or exclusion by, the eligible projection.
- Two reviewers independently judge each qrel without seeing policy rank.
- Both decisions and any adjudication are retained; disagreement is resolved
  before materialisation freeze.
- The pre-freeze judgement pool combines source-first anchors and unscored
  candidate pools at frozen systems and depths without allowing one candidate
  to define all relevance.
- An unjudged item in a scored top five blocks the campaign; it is not silently
  treated as irrelevant.
- Full document text, restricted questions and query vectors stay in approved
  ignored task-owned storage; tracked evidence uses identities, hashes,
  counts, timings and sanitised outcomes.

### Required artefacts

The design/materialisation boundary must define at least:

- `evaluation-design-contract.json`;
- `contract-fixture-matrix.json`;
- `eligibility-fixture-matrix.json`;
- `dataset-manifest.json`;
- `document-manifest.json`;
- `case-inventory.json`;
- `qrels.jsonl`;
- `sampling-plan.json`;
- `question-normalisation-manifest.json`;
- `scorer-manifest.json`;
- `canonicalisation-manifest.json`;
- `rubric.json`; and
- `materialisation-freeze-manifest.json`.

Each policy campaign then defines at least:

- `retrieval-policy-manifest.json`;
- `query-vector-manifest.json`;
- `campaign-input-manifest.json`; and
- `campaign-input-freeze-manifest.json`.

Each run then creates a sanitised `run-manifest.json`, ordered retrieval
results and metrics. Exact paths, schemas and retention are determined in the
separately authorised design-materialisation increment; this proposal creates
none of those artefacts.

## Ordered gates and authority envelopes

The sequence is strictly sequential because policy, dataset, generation and
thresholds are shared immutable inputs:

| Gate | Deliverable and pass condition | Stop condition |
|---|---|---|
| `DR-0 — ADR preparation` | This one proposed ADR, static checks and a focused local commit | Baseline, scope, protected hash or file ownership diverges |
| `DR-1 — Architecture decision` | Explicit owner acceptance or rejection naming ADR-0014 | No implementation authority is inferred from acceptance |
| `DR-2 — Determinism implementation` | Separately authorised typed port, finite-state validation, fail-closed mapping and focused tests with no valid-input ranking change | Any successful valid-input order changes or another file/contract boundary becomes necessary without authority |
| `DR-3 — Determinism Automatic Quality Gate` | Independent review and focused/full applicable checks prove the total-order contract | Finding is recorded; it is not corrected inside the gate |
| `RB-1 — Evaluation design freeze` | Immutable unmaterialised evaluation-design-contract revision with schemas, formulas, quotas and zero product data/results | Product cases, generation or scored result enters the design contract |
| `RB-2 — Dataset materialisation readiness` | Authorised corpus, rights, active generation, completed unscored pooling/adjudication, qrels, complete required matrices, exact intended case denominator, declared pilot/representative tier and dataset manifests freeze | Any identity, right, judgement or required matrix cell is missing or drifts; an underpowered mandatory stratum blocks a representative claim |
| `RB-3 — Campaign-input freeze` | Separately authorised policy manifests, one exact original vector per baseline case, environment, commands and complete intended denominator freeze before scoring | Provider provenance, vector, policy, case or environment identity is missing or a result already exists |
| `RB-4 — Retrieval-only campaign` | Named frozen-vector boundary executes all intended cases and repetitions with no answer-LLM call and produces sanitised evidence | Network/provider use, drift, unjudged top-five item, leakage, omitted case or incomplete execution appears |
| `RB-5 — Report and independent gate` | Factual report, threshold table, matrix coverage, coverage classification and independently authorised Automatic Quality Gate | Any frozen threshold, qrel, denominator, dataset or campaign-input identity changes |
| `MQ-0 — Candidate reconsideration` | Only a separately authorised decision may unpark MultiQuery after the accepted baseline identifies a material retrieval gap | No measured gap, failed determinism gate or authority absence keeps the candidate parked |

No gate above is a lifecycle Human Gate. Each requires its own explicit
authority. Passing one gate does not authorise or pass the next.

## Alternatives

### Add document identity or chunk digest as a tertiary tie-breaker

Rejected. The global ordinal is already unique within the build and generation.
A tertiary key adds no ordering information and could hide a corrupted
duplicate ordinal that should fail closed.

### Change cosine arithmetic while documenting determinism

Rejected for the smallest increment. A precision or normalisation change can
alter valid scores and ranks and therefore requires the successor and index-
compatibility consequences defined above.

### Measure raw `IVectorIndexStore` hits only

Rejected as the primary product baseline. It omits the minimum score, scalar
budget and final selected-evidence limit that this ADR proposes to bind as the
complete `retrieval-v1` policy. Raw hits remain diagnostic evidence.

### Drive the answer path with a fake or real language model

Rejected. It is not retrieval-only, couples the result to prompt/generation
behaviour and can introduce provider calls or a synthetic answer boundary that
does not improve retrieval measurement.

### Reuse the existing synthetic dataset as product evidence

Rejected. Both frozen revisions contain zero scored product documents and
cases. Synthetic fixtures remain valuable for contract and failure tests but
cannot establish product recall, rank quality or source coverage.

### Implement MultiQuery before the baseline

Deferred. It changes query orchestration, cost, failure modes and the causal
interpretation of any quality result. It requires a stable baseline and its
own later architecture and evaluation decision.

## Consequences

- No valid successful `retrieval-v1` rank changes merely to obtain
  determinism.
- The current SQLite ordering becomes an explicit replaceable-port contract
  rather than an adapter accident.
- Invalid numerical and ordering states become index/provider failures rather
  than false insufficient-evidence outcomes.
- Product query and evaluation share one pre-generation Application policy
  implementation.
- A frozen-vector campaign isolates retrieval-policy determinism from provider
  availability and answer generation, while a later provider-inclusive lane
  remains separately measurable.
- A product-data baseline requires materially more curated evidence than the
  current synthetic fixtures and remains `NOT_RUN` until a product corpus is
  authorised, active and frozen.
- The 200-positive comparative pilot can still leave database or source strata
  underpowered. It may support a pilot comparison but cannot support product-
  representative or homologation classification while any mandatory stratum
  remains `UNDERPOWERED`.
- MultiQuery remains a hypothesis rather than an optimisation implemented
  before measurement; a later paired evaluation reuses frozen inputs, not
  baseline results.

## Security, privacy and operations

- Retrieval requests, documents, vectors and scores remain untrusted data.
- A returned hit cannot expand the activation-derived eligible selector set.
- Non-finite or out-of-range scores, duplicate ordinals, identity mismatch and
  ordering violation fail closed.
- Full questions, passages, vectors and qrels are not logged or committed by
  default.
- Sanitised evidence records stable IDs, digests, ranks, score bit patterns,
  counts, timings and typed outcomes.
- Evaluation content stays outside the runtime corpus and release artefact.
- A frozen vector is not a credential but can encode restricted question
  information and therefore inherits the campaign input's storage and access
  boundary.
- Provider-inclusive vector creation requires exact provider, account, secret-
  reference, egress and spend authority. This proposal grants none.
- No metric denominator is merged across dataset revisions, generations,
  policies, providers or environments. A pre-registered paired report may link
  complete, separately executed policy results by `caseId` without merging
  their denominators.

## Compatibility and migration

- OpenAPI v1 and v2 remain byte-for-byte unchanged.
- Existing v1/v2 response fields and `AnswerEvidenceRecordV1` continue to use
  `retrievalPolicyVersion`; no schema or migration is required to propose this
  decision.
- Formalising the existing valid ordering alone does not require a new index
  generation.
- A vector-store scoring change requires a successor retrieval policy and an
  explicitly advanced descriptor consumed by `IndexCompatibilityProfile`,
  producing a new `IndexCompatibilityKey` and generation before serving.
- A future typed-port implementation must reconcile the implemented raw-list
  signature with the accepted canonical `VectorSearchResult` direction
  without exposing provider or Infrastructure types to Domain, HTTP or
  OpenAPI.
- Existing synthetic dataset revisions and historical reports remain
  immutable and retain their original claims.

## Acceptance checks

Before a future explicit decision can accept this ADR, verify that it:

- identifies the current total ordering as
  `Score DESC, global ChunkOrdinal ASC` and does not claim a demonstrated
  valid-input ranking defect;
- preserves `retrieval-v1` for unchanged valid successful results;
- requires finite scores in `[-1, 1]`, preserves stored zero-vector score `0`
  and rejects invalid scores or duplicate ordinals without adding a silent
  tie-break, clamp or epsilon;
- keeps expected retrieval failures typed and maps invalid index state to the
  existing fail-closed taxonomy;
- places the retrieval-only port in Application and stops it before answer
  generation, with finalised generation, compatibility and eligibility-policy
  identities bound;
- applies the accepted Recall@5/MRR@5 threshold values through the proposed
  `retrieval-evaluation-scorer-v1` over the first five final selected-evidence
  items, including mandatory language rows, and does not treat differently
  scored history as comparable;
- defines every hard rate denominator, negative-selection disposition,
  mandatory contract/leakage matrix and canonical digest byte encoding;
- separates an evaluation-design-contract freeze from product dataset
  materialisation and policy-specific campaign-input freezes;
- keeps synthetic and product denominators separate;
- pre-registers formulas, simultaneous sampling quotas, qrels,
  representativeness classification and stop conditions before results;
- requires a successor policy and, when applicable, a new compatibility key
  and generation for observable ranking changes;
- keeps `retrieval-multi-query-v1-candidate` parked while preserving a future
  exact-case paired design with policy-specific vectors and separate results;
- preserves both protected OpenAPI artefacts; and
- grants no implementation, execution, gate, external-action or lifecycle
  authority.

An explicit future owner decision naming `ADR-0014` is required for acceptance.
Acceptance would establish architecture authority only and would not itself
authorise implementation, dataset materialisation, a campaign, an Automatic
Quality Gate, a Human Gate or a lifecycle transition.

## Proposal negative scope

Preparation of this ADR excludes:

- ADR acceptance or rejection;
- changes to source, tests, existing documents, manifests, configuration,
  dependencies or lockfiles;
- executable tests, notebooks, provider calls, network or paid activity;
- product corpus access, import, indexing, activation or evaluation;
- OpenAPI, schema, migration or public-contract changes;
- Automatic Quality Gate, Human Gate or lifecycle transition; and
- push, pull request, publication, deployment or release.
