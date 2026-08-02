# ADR-0007 — Generation Identity and Freshness Observation Rebinding

- Status: accepted
- Date: 2026-08-02
- Accepted: 2026-08-02
- Decision authority: explicit product-owner acceptance on baseline
  `main@664187c6926be5ce4bef3734603f8d936626d535`, corpus `4.8.1`
- Proposal baseline:
  `main@9707b87d75a6acb14c8993ff0283a4221bc6c762`, corpus `4.8.0`
- Owners: RAG-Challenge RAG / data / security architecture
- Corrects: the architectural decision model identified by `AQG-S02-001`
- Supersedes: the observation-inclusive generation-identity and
  exact-record rollback clauses of
  [ADR-0002](ADR-0002-RAG-Lifecycle-Providers-And-Source-Separation.md)

## Context

Accepted ADR-0002 makes `sourceBindingSetDigest` part of the canonical
generation build specification and states that this digest covers
`sourceObservationId`. The same decision requires a successful `304` or an
identical content hash to append a new observation and update only the active
binding without creating a new snapshot or `IndexGenerationId`. It also says
that freshness observations remain outside generation identity.

Those statements cannot describe one deterministic manifest and activation
algorithm. A changed observation either changes the generation digest, or the
activation record points to a binding that is not covered by that digest. The
combined `STATE-02` audit recorded this contradiction as `AQG-S02-001` with
severity P1.

The correction must preserve all of these accepted properties:

- immutable, content-addressed source snapshots and index generations;
- append-only source observations;
- deterministic manifest and `IndexGenerationId` calculation;
- complete compare-and-swap of one `CorpusActivationRecord` authority;
- explicit provenance and freshness at query time;
- no source fetch during a query;
- a bounded generation rollback path that never makes old evidence fresh;
- no in-place mutation of manifest, snapshot, observation or vector data.

Preparing and committing the proposal did not accept it. The later explicit
owner decision accepts the architecture correction only; it does not change
the current gate result, authorise the follow-on semantic reconciliation,
authorise implementation or authorise any external action.

## Decision

Use separate integrity domains for a generation and an activation record.

### Generation-bound identity

Keep the existing manifest field name `sourceBindingSetDigest`, but define it
as the digest of the ordered **generation-bound source projection**. For each
document binding, that projection contains:

```text
databaseProductId
databaseProductRevision
documentId
documentVersion
documentFormat
sourceAdapterId
sourceTrustClass
officialSourceRegistrationId?  # immutable/versioned registration identity
sourceSnapshotId?              # immutable content snapshot identity
```

`sourceObservationId` is explicitly excluded from
`sourceBindingSetDigest`, `generationSpecDigest`, the complete manifest
content digest and `IndexGenerationId`.

`activeDocumentSetDigest` continues to cover the ordered database/document
identity, revision, version and format projection. The overlap is deliberate:
`activeDocumentSetDigest` proves the active document set, while
`sourceBindingSetDigest` proves the source, trust, registration and snapshot
metadata carried by the generated artefacts and citations.

An official source registration used by this projection must be immutable or
versioned. A change to its canonical URL, adapter, trust class or other
generation-relevant policy produces a new registration identity and therefore
a new generation. A mutable record identifier without a revision is not a
valid manifest input.

The canonical manifest remains:

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

Its existing canonical serialisation and finalisation rules remain unchanged.
Consequently, identical generation-bound inputs and logical artefacts retain
the same `IndexGenerationId` even when later freshness observations change.

### Activation-record identity

Add `activationBindingSetDigest` to `CorpusActivationRecord`. It is the
SHA-256 digest of the canonical ordinal collection of the complete binding
projection:

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
sourceObservationId?
```

The digest uses an explicitly versioned UTF-8 canonical representation with
fixed property order, ordinal collection order and unambiguous null handling.
It belongs to the activation record and its audit history; it is not copied
into the generation manifest and does not affect `IndexGenerationId`.

Before compare-and-swap, Application must validate all three projections:

1. recompute `activeDocumentSetDigest` from the proposed record and match the
   referenced finalised manifest;
2. recompute the generation-bound `sourceBindingSetDigest` from the proposed
   record, excluding observations, and match the manifest;
3. recompute the complete `activationBindingSetDigest`, including
   observations, and match the proposed record.

For each official binding, the referenced append-only observation must exist
and must name the same immutable registration and snapshot as the binding.
The observation state and `maxAge` are then evaluated by query-time policy
from the activation record resolved once at query start. No query reads a
separate "latest observation".

`catalogueRevision` identifies the immutable generation-bound catalogue
snapshot. Appending a freshness observation advances the observation journal
and activation-record revision, not that catalogue revision. An
implementation-specific database transaction or row version must not be
confused with the canonical `catalogueRevision`.

### `304` and identical-content revalidation

A successful revalidation for the same registration and snapshot performs
these steps:

1. append the immutable observation and persist its validators, status,
   `revalidatedAt`, `maxAge` and sanitised evidence;
2. verify that it references the registration and snapshot already represented
   by the active manifest;
3. create a new complete `CorpusActivationRecord` revision with the new
   `sourceObservationId` and recomputed `activationBindingSetDigest`;
4. compare-and-swap that record and its audit event atomically.

The operation preserves `IndexGenerationId`, manifest bytes,
`generationSpecDigest`, `sourceBindingSetDigest`, `catalogueRevision` and
`generationActivatedAt`. It changes `recordRevision`, `recordUpdatedAt`, the
affected `sourceObservationId` and `activationBindingSetDigest`.

If content, snapshot identity, source adapter, trust class, immutable source
registration, document set or any `IndexCompatibilityKey` input changes, a
new candidate generation is required. A revalidation that cannot prove the
same registration/snapshot relation fails closed and does not rebind the
active record.

An authoritative withdrawal or explicit source deactivation may also append
and bind an observation without changing generation artefacts. Query-time
eligibility and coverage reflect that observation. Changing the catalogue
membership or document lifecycle itself still requires a new generation.

### Rollback

Generation rollback targets a retained, validated generation and its complete
generation-bound binding projection; it never restores a historical
`CorpusActivationRecord` byte for byte.

The rollback operation creates a new record revision. For each official
registration/snapshot in the target generation, it binds an existing
observation that is compatible and eligible under current policy. The
selection occurs in the authorised administrative transaction before the new
record is published; query execution still resolves only that record. If the
target cannot satisfy the active-database and eligible-document invariants,
rollback is rejected without changing the current record.

Historical observations remain append-only evidence. Rollback never changes
their timestamps, `maxAge` or status and never marks an old snapshot fresh.
Correcting an erroneous observation requires a new append-only observation
and a new activation-record revision.

### Canonical transition table

| Change | New observation | New snapshot | New activation-record revision | New generation |
|---|---:|---:|---:|---:|
| `304` for the bound registration/snapshot | Yes | No | Yes | No |
| Identical hash for the bound registration/snapshot | Yes | No | Yes | No |
| Authoritative withdrawal or source deactivation only | Yes | No | Yes | No |
| New content hash or snapshot | Yes | Yes | Yes, after candidate validation | Yes |
| Source adapter, trust or immutable registration changes | As applicable | As applicable | Yes, after candidate validation | Yes |
| Document membership/version/format changes | As applicable | As applicable | Yes, after candidate validation | Yes |
| Rollback to retained generation | No, unless separately revalidated | No | Yes | No new generation artefacts |

## Alternatives

### Include `sourceObservationId` in generation identity

This is internally coherent only if every observation rebinding finalises a
new manifest and produces a different `IndexGenerationId`, including for a
`304`. Logical chunk/vector artefacts could be reused, but the design would
still create a new generation authority for a freshness-only event.

Not recommended because it conflates source availability evidence with
derived content identity, creates avoidable manifest and rollback churn, and
makes operational freshness events appear to be index rebuilds.

### Mutate the active manifest or observation in place

Rejected because it destroys deterministic identity, append-only provenance
and crash-safe auditability.

### Exclude observations without an activation-record digest

Rejected because an observation-only rebinding would then lack an explicit
canonical integrity value covering the complete active binding set.

## Consequences

- Generation identity remains stable across freshness-only observations.
- Every active observation remains integrity-covered by the activation record
  and audit transaction rather than by the manifest.
- `STATE-03` must model a separate observation-journal revision and must not
  increment canonical `catalogueRevision` for freshness-only writes.
- `STATE-03` must define canonical serialisation test vectors for both source
  digests and must persist `activationBindingSetDigest` with every activation
  revision.
- `STATE-04` synchronisation can restore eligibility after `304` without
  rewriting vectors, while mismatched registration/snapshot relations fail
  closed.
- Rollback becomes a new validated record construction rather than replay of
  stale freshness metadata.
- Existing accepted selections for providers, persistence, egress, HTTP,
  catalogue and evaluation are unchanged.
- Multiple active corpora, incremental indexing and provider expansion remain
  outside the MVP.

## Security and operations

- A forged or mismatched observation cannot be rebound merely because it is
  newer; its registration and snapshot relation must match the manifest
  projection.
- The complete binding digest must be written in the same compare-and-swap
  transaction as the record and sanitised audit event.
- Freshness-only conflicts leave the previous record active and the new
  observation as an auditable orphan until retry or retention cleanup.
- Logs contain IDs, digests, revisions, status and timing, not document
  content, validators containing credentials or unrestricted URLs.
- No change in this ADR enables `OFFICIAL_SOURCE_EGRESS`, `AI_PROVIDER_EGRESS`,
  `VECTOR_STORE_EGRESS` or `OCI_RUNTIME_EGRESS`.
- `THR-S02-003`, `THR-S02-004`, `THR-S02-011`, `THR-S02-012`,
  `THR-S02-013`, `THR-S02-021`, `THR-S02-024` and `THR-S02-034` require
  tests against the corrected model.

## Compatibility and migration

This ADR was accepted before implementation, so it is a documentary
compatibility correction with no runtime data migration. The following
accepted-baseline documents must be reconciled in one separately authorised
change before another combined audit:

| Artefact | Required reconciliation |
|---|---|
| ADR-0002 | Exclude observation identity from `sourceBindingSetDigest`; replace exact-record rollback with new-record construction. |
| Canonical contracts | Add `activationBindingSetDigest`; state the three projection validations and `304` field changes. |
| RAG module | Separate generation and activation digests; clarify observation-journal/catalogue revisions and rollback eligibility. |
| Vision requirements | Refine `AC-MVP-005`, `AC-MVP-014` and `RNF-005` to name the two integrity domains without weakening the outcomes. |
| Lifecycle and Quality Gates | Require both canonical digests, observation compatibility and freshness-safe rollback in `STATE-03`/`STATE-04`. |
| Roadmap/backlog | Refine S03/S04 and `BL-M14` to test new-record rollback and observation-only rebinding. |
| Threat model | Add the binding-digest and mismatch checks to the affected threats/test groups without claiming implementation. |
| Architecture report | Record the owner decision and the exact post-decision reconciliation/validation evidence. |

Until those edits are completed and a separately authorised combined audit
passes, the Automatic Quality Gate remains `REPROVADO` and `STATE-03` remains
unauthorised.

## Acceptance checks

- Canonical test vectors prove that changing only `sourceObservationId`
  changes `activationBindingSetDigest` but not `sourceBindingSetDigest`,
  `generationSpecDigest` or `IndexGenerationId`.
- Changing `sourceSnapshotId`, source trust, adapter or immutable registration
  changes `sourceBindingSetDigest` and requires a new generation.
- A `304`/identical-hash compare-and-swap changes only the fields permitted by
  this ADR and preserves the exact manifest bytes.
- An observation naming another registration or snapshot is rejected before
  activation.
- Retry after an activation conflict is idempotent and never selects an
  observation implicitly at query time.
- Rollback creates a new record, preserves historical records and refuses an
  ineligible source/document set.
- Crash tests cover observation append, digest calculation, audit write and
  compare-and-swap boundaries.
- Architecture and contract tests prevent the vector store or catalogue from
  becoming a second activation authority.
- Documentary reconciliation preserves the accepted provider, persistence,
  security and catalogue decisions and introduces no implementation claim.
- A new combined `STATE-02` audit passes before any Human Gate is prepared.

## Decision outcome

The product owner explicitly accepted ADR-0007 on 2026-08-02 with the exact
decision `ADR-0007: ACEITAR.`. The acceptance makes this ADR the authority for
the corrected generation/activation identity and rollback clauses, but does
not by itself reconcile the affected documents, dispose of `AQG-S02-001`,
repeat the Automatic Quality Gate or authorise any implementation.
