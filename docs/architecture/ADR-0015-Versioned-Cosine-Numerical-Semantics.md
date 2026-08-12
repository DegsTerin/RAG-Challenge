# ADR-0015: Versioned Cosine Numerical Semantics

## Status

`accepted`

## Date

2026-08-11

## Accepted

2026-08-11

## Owners

- Product owner: RAG-Challenge owner
- Architecture owner: RAG-Challenge
- Technical owner: `CH-MOD-03 INDEXING_RETRIEVAL`

## Preparation, decision, implementation and reconciliation authority

- Authority: `AUTH-DR3-NUMERIC-SEMANTICS-PROPOSAL-001`
- Branch: `main`
- Commit: `ce9ba622e7e11214c200482ca50169afb987ee00`
- Prompt corpus before this proposal: `4.10.29`
- Decision authority: explicit product-owner decision
  `ADR-0015: ACEITAR.` on clean
  `main@46de807148d5b547f56a0f7265b32428b232100f`, corpus `4.10.30`
- Implementation authority: `AUTH-DR3-NUMERIC-SEMANTICS-IMPLEMENTATION-001`
- Implementation baseline: clean
  `main@9735ff5bc243d9a517b2cceb7ca8bfe16f24b438`, corpus `4.10.31`
- Implementation commit: `9addb166e82dd04581beee7b4276a74977fe04c5`
- Reconciliation authority:
  `AUTH-DR3-NUMERIC-SEMANTICS-IMPLEMENTATION-RECONCILE-001`
- Reconciliation baseline: clean
  `main@9addb166e82dd04581beee7b4276a74977fe04c5`, corpus `4.10.31`
- Lifecycle position: `STATE-07 TESTING_HOMOLOGATION`
- Gate position: `DR-3 — Determinism Automatic Quality Gate` remains
  `REPROVADO`
- Implementation runtime preflight: applicable; it found zero owned
  RAG-Challenge process candidates and stopped zero processes
- Reconciliation runtime preflight: `NOT_APPLICABLE` because this increment is
  documentary
- Protected OpenAPI v1 SHA-256:
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`
- Protected OpenAPI v1 Git blob:
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`
- Protected OpenAPI v2 SHA-256:
  `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733`
- Protected OpenAPI v2 Git blob:
  `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`

## Purpose and authority boundary

This decision defines versioned numerical semantics for exact
cosine scoring after `DR3-FIND-001` demonstrated that an admissible identical
vector pair can produce a finite binary64 result immediately above the closed
cosine range. It also records a verifiable correction plan for
`DR3-FIND-001` to `DR3-FIND-004`.

Acceptance selects the numerical semantic, successor retrieval-policy identity
and compatibility consequences defined below as architecture authority only.
It does not change `retrieval-v1`, correct code or tests, create a generation,
dataset, scorer or campaign, execute a gate, or change lifecycle state.

The later implementation authority and commit recorded above implemented the
selected source-code and test changes. That implementation evidence does not
amend the decision, activate a product generation or dispose any DR-3 finding.

The accepted
[ADR-0014](ADR-0014-Deterministic-Retrieval-Ranking-And-Retrieval-Only-Baseline.md)
remains authoritative. It requires a new retrieval-policy version and an
advanced vector-store compatibility descriptor when score arithmetic changes,
and prohibits silently adding an epsilon, clamp or arithmetic change under
`retrieval-v1`.

## Context

The `DR-2` implementation validates every successful score as finite and
inside `[-1, 1]`. Its current SQLite exact-search arithmetic:

1. accepts finite `float32` vector components;
2. evaluates each component product in `float32`;
3. widens each rounded product to `float64` and accumulates it serially in
   vector-index order;
4. applies the same multiplication and accumulation shape to both squared
   norms;
5. takes each square root in `float64`;
6. multiplies the two norms and divides the dot product in `float64`; and
7. returns exact score order `Score DESC, global ChunkOrdinal ASC` before
   `Take(k)`.

For identical admissible vectors `[1f, 1f, 1f]`, the current pipeline produces
`1.0000000000000002`, which is `Math.BitIncrement(1d)`. The range validation
therefore returns `InvalidIndexData`, and the query path maps that result to
`CH_INDEX_UNAVAILABLE`. This is an observed valid-input failure, not merely a
missing proof.

The other three DR-3 findings are proof gaps. Review observed that filters are
applied before scoring and that complete score/ordinal ordering precedes
`Take(k)`, while negative ordinals are rejected. The existing tests do not
prove those behaviours with the adversarial cases required by the gate.

## Decision drivers

- Restore successful handling of admissible cosine inputs at the mathematical
  range boundary.
- Preserve every representable in-range score produced with finite
  intermediates by the current arithmetic, including its binary64 bit pattern
  and signed zero.
- Preserve exact comparison, the existing total order and the absence of
  epsilon-based implicit ties.
- Keep non-finite query, stored-vector, norm, denominator and score states
  fail-closed.
- Make every behaviourally observable arithmetic change explicit in retrieval,
  vector-store, index-generation and evaluation identities.
- Avoid implying that a focused boundary correction provides numerical
  robustness over the complete finite `float32` domain.
- Make all four DR-3 findings independently reproducible and closable by
  future executable evidence.
- Keep OpenAPI v1 and v2 byte-for-byte unchanged.

## Decision

The owner's explicit decision `ADR-0015: ACEITAR.` selects the numerical
semantic
`cosine-f32mul-f64acc-boundary-canonical-v1` and the successor retrieval
policy `retrieval-v2`.

Both identifiers are accepted architecture identities. They must not appear as
implemented, active, evaluated or serving identities before a separately
authorised implementation, compatible generation and activation.

The selected successor vector-store descriptor is:

```text
sqlite-exact-vector-store/2;schema=1;distance=cosine;algorithm=exact-scan;vector=float32;score=cosine-f32mul-f64acc-boundary-canonical-v1
```

The `/2` component advances the concrete scoring compatibility identity. The
SQLite schema remains `1`; this decision requires no schema or migration
change.

### Selected algorithm contract

`cosine-f32mul-f64acc-boundary-canonical-v1` means exactly:

1. Input dimensions must match and all query and stored components must be
   finite IEEE 754 binary32 values.
2. A zero-magnitude query remains invalid. A zero-magnitude stored vector
   returns exact positive binary64 zero, preserving the current explicit
   stored-zero rule.
3. For every index in ascending vector order, component multiplication occurs
   in binary32 using round-to-nearest, ties-to-even. The resulting binary32
   product is widened exactly to binary64 and added to a binary64 accumulator
   using round-to-nearest, ties-to-even.
4. Dot product and both sum-of-squares accumulations are serial and preserve
   source index order. Fused multiply-add, reassociation, parallel reduction,
   compensated summation and provider-specific accumulation are excluded.
5. Square roots, norm multiplication and the final quotient occur in binary64
   using the runtime's IEEE 754 operations.
6. Every non-finite intermediate or raw quotient remains invalid and fails
   closed. Boundary canonicalisation never converts `NaN` or infinity into a
   score.
7. A finite raw quotient greater than `+1` becomes exact binary64 `+1`. A
   finite raw quotient less than `-1` becomes exact binary64 `-1`. Every raw
   quotient already in `[-1, 1]` is preserved bit-for-bit, including `-0`.
8. Ranking compares the canonical score exactly. No epsilon, ULP bucket,
   rounding bucket or approximate equality is introduced. Exact score ties
   continue to use global `ChunkOrdinal ASC`, with no tertiary key.

The projection in item 7 is a codomain operation, not evidence that the raw
arithmetic is accurate for every finite vector. A future implementation must
expose neither raw out-of-range values nor a second comparison semantic.

### Preserved retrieval-policy fields

This decision changes the scoring semantic and its governed
identities only. The following accepted fields remain unchanged:

| Field | `retrieval-v2` value |
|---|---|
| Ranking | `Score DESC, global ChunkOrdinal ASC` |
| Vector search | exact cosine over the eligible validated generation |
| Vector `top-k` | `8` |
| Minimum-score policy | `minimum-score-v1` |
| Minimum score | `0.25`, inclusive |
| Maximum selected evidence | `6` |
| Scalar budget | `16,000` Unicode scalar values |
| Eligibility policy | unchanged hard pre-filter before scoring and `top-k` |
| Stored zero-vector | exact positive score `0` |
| Query vector | exactly one original-question vector |
| MultiQuery | parked and non-canonical |

## Alternatives

### Alternative A — exact one-ULP boundary corridor

Not selected. This alternative remains recorded for traceability.

Keep the current arithmetic, but canonicalise only the two immediately
adjacent binary64 values outside the cosine range:

- `Math.BitIncrement(1d)` becomes exact `+1`; and
- `Math.BitDecrement(-1d)` becomes exact `-1`.

Every other finite out-of-range result remains invalid and fail-closed. This
alternative corrects the observed `[1f, 1f, 1f]` reproduction and has the
smallest numerical blast radius. It also retains a diagnostic distinction
between a one-ULP boundary excursion and a larger excursion.

Its weakness is that no proof currently bounds every legitimate excursion of
the present binary32-product/binary64-accumulation pipeline to one ULP for all
authorised dimensions and finite input magnitudes. Selecting it without that
proof would encode the observed example as a rule rather than establish a
complete admissible-input semantic.

Future reconsideration condition: a successor ADR may select this alternative
only if a separately reviewed numerical argument establishes the one-ULP bound
for the complete configured input domain, and executable property evidence
independently confirms the bound at both signs, across dimensions and
adversarial magnitudes.

Compatibility impact: it still changes a valid-input failure into success and
can alter a valid ranking at the endpoints. It therefore still requires a new
retrieval-policy version, advanced scoring descriptor, new
`IndexCompatibilityKey`, new generation and new evaluation baseline.

### Alternative B — scaled arithmetic in binary64

Not selected. This alternative remains recorded for traceability.

Convert each binary32 component exactly to binary64, scale each non-zero vector
independently by its maximum absolute component, and then compute dot product,
squared norms, square roots and quotient using serial binary64 operations.
Apply an explicitly specified finite boundary projection after the quotient and
retain the same exact ranking rule.

This alternative materially improves resistance to overflow and underflow and
is the strongest candidate when the product requirement covers the complete
finite binary32 magnitude range. It changes interior scores as well as boundary
scores because component products no longer round in binary32. Those changes
can alter threshold inclusion and ranking even for inputs that succeed under
`retrieval-v1`.

Future reconsideration condition: a successor ADR may select this alternative
if a product requirement or adversarial evidence establishes that current
arithmetic is not sufficiently stable beyond the observed boundary excursion,
especially across extreme finite magnitudes or large dimensions. Before that
decision, freeze the scaling, accumulation, signed-zero, non-finite and
boundary rules precisely.

Compatibility impact: this is the broadest change. It requires a new
retrieval-policy version, a separately named numerical semantic, advanced
vector-store descriptor, new `IndexCompatibilityKey`, new generation and a
new evaluation baseline with explicit threshold/ranking comparison against
historical results.

### Alternative C — keep `retrieval-v1` rejection unchanged

Not selected. It preserves the current identity and fail-closed range
validation, but leaves an observed admissible input mapped to
`CH_INDEX_UNAVAILABLE` and therefore does not dispose `DR3-FIND-001`.

### Alternative D — add an epsilon or approximate tie

Not selected. An epsilon is scale- and policy-dependent, can merge distinct
scores, can alter ordering away from the mathematical boundary and conflicts
with ADR-0014's exact comparison contract. A tolerance also does not define
which score value is persisted, hashed or exposed to later policy stages.

### Alternative E — regroup the denominator only

Not selected. Reassociating `queryNorm * vectorNorm`, changing evaluation order
or special-casing identical vectors can correct one reproduction without
defining a general numerical contract. Such a change is runtime- and
optimiser-sensitive unless fully versioned and still leaves other boundary
cases unspecified.

## Compatibility matrix

| Concern | `retrieval-v1` | Selected decision | One-ULP corridor | Scaled binary64 |
|---|---|---|---|---|
| Interior score bits | Current bits | Preserved exactly | Preserved exactly | May change |
| Finite out-of-range score | Invalid | Project any finite value to the nearest endpoint | Project only the adjacent one-ULP value; reject farther values | Determined by separately frozen scaled semantic |
| Non-finite state | Fail closed | Fail closed | Fail closed | Fail closed |
| Signed zero | Current behaviour | Preserved for in-range quotient; stored zero is `+0` | Same as selected decision | Must be frozen before selection |
| Exact ranking/tie-break | Score, then ordinal | Unchanged | Unchanged | Unchanged after new scores |
| Threshold/top-k/evidence/budget | Current fixed values | Unchanged | Unchanged | Unchanged unless separately decided |
| Retrieval identity | `retrieval-v1` | `retrieval-v2` | New version required | New version required |
| Store descriptor | `/1`, no score semantic field | `/2` descriptor with named score semantic | Advanced descriptor required | Advanced descriptor required |
| Existing generation reusable under successor | Yes only under its current v1 identity | No | No | No |
| New `IndexCompatibilityKey` and generation | No for unchanged v1 | Required before serving | Required before serving | Required before serving |
| New evaluation baseline | No | Required | Required | Required, with wider comparison |
| OpenAPI/schema/migration | Unchanged | Unchanged | Unchanged | Unchanged unless a later decision adds another need |

Historical generations, manifests, answer-evidence records and evaluation
results remain immutable under their original retrieval-policy and
compatibility identities. Even if stored vector bytes are unchanged, a
generation built with the `/1` descriptor must not be relabelled or served as
the `/2` successor. A separately authorised implementation must build and
validate a new generation and fail closed on cross-version mismatch.

The existing v1/v2 public field `retrievalPolicyVersion` can carry a successor
string only after implementation and activation. This decision changes neither
OpenAPI document and introduces no public field.

## Verifiable correction plan

Acceptance did not authorise or execute the following work. It was later
implemented under the separately bounded authority and commit recorded above;
the requirements and pass conditions remain the correction contract for the
independent DR-3 retest.

### `DR3-FIND-001 — P1`

Implementation target:

- implement exactly the accepted numerical semantic in the SQLite exact store;
- advance the retrieval-policy, vector-store descriptor and expected
  compatibility identity together;
- preserve fail-closed non-finite handling and all unchanged policy fields;
- reject a v1 generation when a v2 compatibility identity is required; and
- keep raw and canonical scoring as one internal operation without exposing a
  competing public or Application comparison rule.

Required executable proof:

1. identical `[1f, 1f, 1f]` vectors return exact `+1` and succeed;
2. corresponding anti-parallel vectors return exact `-1` and succeed;
3. representative in-range values, including positive zero and negative zero
   if it is constructible through the accepted operation sequence, preserve
   the accepted bit-level rule;
4. stored zero-vector remains exact `+0`; zero query remains invalid;
5. non-finite components, norms, intermediates and quotients remain
   fail-closed;
6. every accepted boundary rule is covered at both signs, including the first
   representable values immediately inside and outside each endpoint;
7. old/new descriptor and `IndexCompatibilityKey` mismatches fail closed;
8. new-generation finalisation, reopen and repeated searches preserve exact
   ordered score bits and canonical digests; and
9. product query maps the corrected admissible case to successful retrieval,
   not `CH_INDEX_UNAVAILABLE`.

Pass condition: all accepted semantic vectors produce the exact prescribed
binary64 score bits and disposition under a new compatible generation; every
invalid or mismatched state retains its typed fail-closed outcome.

### `DR3-FIND-002 — P2`

Implementation target: strengthen
[`SqliteVectorRetrievalDeterminismTests`](../../tests/RagChallenge.IntegrationTests/SqliteVectorRetrievalDeterminismTests.cs)
with a top-k adversary whose storage/enumeration order cannot satisfy the
expected result accidentally.

Required executable proof:

- persist at least nine eligible chunks for `top-k = 8`;
- make the highest score belong to a late or highest ordinal and the lowest
  score belong to an early or lowest ordinal, with equal-score middle cases to
  exercise the ordinal tie-break;
- assert that the highest-scoring late item is included, the lowest-scoring
  early item is excluded and every returned position follows
  `Score DESC, global ChunkOrdinal ASC`;
- repeat across at least two write-batch permutations, same-store replay and
  store reopen; and
- compare ordered score bits and identities, not only set membership.

Pass condition: every permutation and reopen produces one byte-equivalent
ordered result and proves complete sorting before `Take(k)`.

### `DR3-FIND-003 — P2`

Implementation target: extend
[`BackendIndexingWorkflowTests`](../../tests/RagChallenge.IntegrationTests/BackendIndexingWorkflowTests.cs)
and, where the concrete SQL boundary needs direct evidence, the SQLite
retrieval integration suite.

Required executable proof:

- create eligible lower-scoring chunks and ineligible higher-scoring chunks
  that would consume every top-k position if filtering occurred afterwards;
- cover the resolved generation/candidate boundary, eligible binding set,
  database selector and document selector independently;
- place ineligible competitors at adversarial ordinals and write positions;
- assert that no ineligible identity appears in raw ranked hits or selected
  evidence;
- assert that all expected eligible hits remain available in exact score and
  ordinal order; and
- repeat after reopen without broadening the frozen selector set.

Pass condition: each filter class is proven to constrain the candidate set
before scoring and top-k, with zero leakage and no loss caused by ineligible
competitors.

### `DR3-FIND-004 — P2`

Implementation target: add both contract-level and concrete-store negative
ordinal regressions without changing the schema.

Required executable proof:

- make the Application retrieval fake return a hit with
  `ChunkOrdinal = -1` and assert `InvalidIndexData`, mapped to
  `CH_INDEX_UNAVAILABLE` before any language-model call;
- in a task-owned temporary SQLite store, inject a negative ordinal by a
  narrowly controlled corruption fixture that bypasses normal write
  validation without changing production schema or migration history;
- assert that exact search returns `InvalidIndexData`, exposes no ranked hit or
  selected evidence and does not continue to answer generation; and
- prove cleanup and isolation of the corrupted temporary store.

Pass condition: both the replaceable-port contract and the concrete SQLite
read boundary reject a negative global ordinal deterministically and
fail-closed.

## Corrective implementation record

Commit `9addb166e82dd04581beee7b4276a74977fe04c5` implements the selected
semantic and compatibility boundary as follows:

- `RetrievalPolicyConfiguration` and the query/runtime composition now require
  `retrieval-v2` and `RetrievalV2PolicyExecutor`;
- `SqliteVectorIndexStore` publishes the exact descriptor
  `sqlite-exact-vector-store/2;schema=1;distance=cosine;algorithm=exact-scan;vector=float32;score=cosine-f32mul-f64acc-boundary-canonical-v1`;
- every component product is fixed in a `float` local before serial `double`
  accumulation in vector-index order; square roots, norm multiplication and
  division remain ordered binary64 operations;
- every finite raw quotient above `+1` is canonicalised to exact `+1`, every
  finite raw quotient below `-1` to exact `-1`, and every in-range bit pattern,
  including signed zero, is preserved;
- non-finite components or intermediates, zero query vectors, malformed
  ordinals and incompatible generation identities remain typed fail-closed;
- stored zero vectors retain exact positive zero; and
- candidate/generation and binding filters precede scoring, while the complete
  `Score DESC, global ChunkOrdinal ASC` order precedes `Take(k)`.

The implementation introduced no epsilon, one-ULP corridor, scaled-binary64
arithmetic, FMA, reassociation, bucket or tertiary ranking key. The compatibility
profile now incorporates the `/2` descriptor, so a generation or
`IndexCompatibilityKey` produced with the `/1` descriptor returns
`GenerationUnavailable` rather than being relabelled or served.

The corrective evidence maps to the findings as follows:

| Finding | Implemented evidence | Disposition before independent DR-3 retest |
|---|---|---|
| `DR3-FIND-001 — P1` | Bit-exact endpoint, adjacent-boundary, signed-zero, interior-score, zero-vector, non-finite, overflow, reopen and `/1` compatibility tests; the synthetic end-to-end `[1f, 1f, 1f]` path completes under `retrieval-v2`. | `CORRECTED_PENDING_GATE_RETEST` |
| `DR3-FIND-002 — P2` | Nine-chunk adversarial top-k test with two write permutations, equal-score tie cases, same-store replay, reopen and ordered score-bit/identity assertions. | `CORRECTED_PENDING_GATE_RETEST` |
| `DR3-FIND-003 — P2` | Competing higher-score ineligible chunks cover candidate/generation, eligible-binding, database and document filters before score/top-k, including reopen and selected-evidence assertions. | `CORRECTED_PENDING_GATE_RETEST` |
| `DR3-FIND-004 — P2` | Application fake returns `ChunkOrdinal = -1` before any language-model call; a task-owned temporary SQLite corruption fixture proves the concrete read boundary returns `InvalidIndexData` without hits or evidence. | `CORRECTED_PENDING_GATE_RETEST` |

The implementation turn recorded a Release build with zero warnings and zero
errors and a local, offline solution test run with 202 unit, 203 integration and
11 architecture tests passing: 416 total, with zero failures or skips. Those
checks are focused implementation evidence, not an Automatic Quality Gate. The
fixtures are synthetic and the SQLite stores are task-owned and temporary; no
product generation, dataset, scorer, campaign, provider, credential, network,
paid call or real corpus was created, activated or used.

This factual reconciliation re-inspected the implementation commit and the
finding-specific evidence but did not rerun executable validation. `DR-3`
therefore remains `REPROVADO`, and all four findings remain pending disposition
by a separately authorised independent corrective retest.

## Future verification sequence

The correction sequence is strictly ordered:

1. `ADR-0015` owner decision: **completed** by the explicit decision
   `ADR-0015: ACEITAR.`.
2. Corrective implementation: **completed** under
   `AUTH-DR3-NUMERIC-SEMANTICS-IMPLEMENTATION-001` in commit
   `9addb166e82dd04581beee7b4276a74977fe04c5`.
3. Focused implementation evidence and factual reconciliation: **completed**
   with the recorded local/offline validation and
   `AUTH-DR3-NUMERIC-SEMANTICS-IMPLEMENTATION-RECONCILE-001`.
4. `DR-3` corrective retest: obtain a new independent Automatic Quality Gate
   authority, `AUTH-DR3-NUMERIC-SEMANTICS-AQG-RETEST-001`, and rerun the
   applicable focused and repository-wide checks.
5. Preserve `DR-3` as `REPROVADO` until that independently authorised retest
   disposes all four findings.

No step grants authority to the next. Dataset, scorer, campaign, provider,
network, paid calls, product corpus, OpenAPI, schema, migration, MultiQuery,
Human Gate, lifecycle, push and publication remain outside this sequence unless
separately authorised.

## Consequences

Acceptance establishes that:

- the observed valid-input failure receives an exact and deterministic
  boundary disposition;
- every current in-range score bit remains unchanged, limiting ranking drift
  to cases whose raw score lies outside the mathematical cosine codomain;
- `retrieval-v2` and the `/2` vector-store descriptor become required
  identities for the new scoring semantic;
- a new `IndexCompatibilityKey`, generation and evaluation baseline become
  mandatory before the successor can serve;
- historical v1 generations and evidence remain valid only under their frozen
  identities;
- tests gain adversarial evidence for complete sort, pre-filtering and negative
  ordinal rejection; and
- full-domain numerical robustness remains explicitly unproven because the
  selected decision preserves the current arithmetic and only defines its
  finite codomain boundary.

## Security and operations

- Non-finite numerical states and compatibility mismatches continue to fail
  closed.
- A generation is never relabelled in place; candidate build, validation and
  activation remain atomic and auditable under existing rules.
- No score tolerance can silently admit or reorder evidence.
- No public contract, secret, network, provider, database engine, operational
  store or real corpus is touched by this decision.
- Runtime and cross-platform reproducibility require the future implementation
  to prohibit FMA, reassociation and unordered parallel reductions for the
  named semantic and to verify exact score bits on every supported runtime
  architecture.

## Acceptance record

The owner's explicit decision `ADR-0015: ACEITAR.` was made on clean
`main@46de807148d5b547f56a0f7265b32428b232100f`, corpus `4.10.30`, with both
protected OpenAPI identities unchanged. It confirms the following as one
architectural choice:

1. any finite raw cosine quotient outside the mathematical codomain is
   projected to the exact nearest endpoint, while every in-range bit pattern is
   preserved;
2. the scope deliberately preserves binary32 multiplication and serial
   binary64 accumulation rather than promising full finite-domain stability;
3. `retrieval-v2`, the named `/2` descriptor, a new
   `IndexCompatibilityKey`, a new generation and a new evaluation baseline are
   mandatory consequences rather than optional migration details; and
4. the four-finding correction plan is sufficient for a later independent
   `DR-3` retest.

Changing to the one-ULP or scaled-binary64 alternative now requires a successor
ADR that satisfies its recorded objective condition and explicitly supersedes
this decision.

## Acceptance negative scope

Acceptance and its documentary reconciliation do not authorise:

- source-code or test changes;
- implementation or activation of `retrieval-v2` or the selected numerical
  semantic;
- generation creation, rebuild, validation, migration or activation;
- dataset, scorer or campaign creation or execution;
- provider, credential, network, paid call or product-corpus activity;
- OpenAPI, schema, migration, dependency, lockfile or MultiQuery changes;
- Automatic Quality Gate, Human Gate or lifecycle transition; or
- push, publication, deployment or release.
