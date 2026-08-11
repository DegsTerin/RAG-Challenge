# ADR-0011 — Source Rights Evidence Mapping and Same-Origin Derivative Display Boundary

- Status: accepted
- Date: 2026-08-09
- Accepted: 2026-08-09
- Preparation authority: `AUTH-S07-A-RIGHTS-POLICY-CORR-PREP-001` on
  `main@17c41a78cbe853473860403d476797064b77c78a`, corpus `4.10.7`
- Decision authority: explicit product-owner decision `ADR-0011: ACEITAR.` on
  `main@09f6760cb1a41d907da42b8c01cb34a7425030b9`, corpus `4.10.8`
- Owners: RAG-Challenge product, architecture, data governance and security
- State: `STATE-07 TESTING_HOMOLOGATION` accepted architecture decision
- Implementation status: not authorised; this decision changes no code, public
  contract, schema, migration, dataset, rights disposition or runtime behaviour

## Purpose and authority

This decision defines how authoritative primary evidence expressed through
broad grants, restrictions and conditions can be mapped to the ten technical
rights decisions already required by
[ADR-0008](ADR-0008-Product-Corpus-Storage-And-Page-Image-Evidence.md). It also
defines the boundary between same-origin runtime display and separate source or
derivative distribution or publication.

The decision replaces neither primary evidence nor qualified legal advice. It
does not decide whether any specific licence is sufficient, reclassify the
PostgreSQL candidate, change the frozen v2 contract or authorise
implementation. It refines the evidence-mapping semantics of ADR-0004 and
ADR-0008 without weakening their independent decisions or fail-closed rules.
Any documentary reconciliation, internal correction or candidate reassessment
remains separately authorised.

## Context

[ADR-0004](ADR-0004-MVP-Corpus-Official-Source-And-Evaluation.md) requires
source-specific evidence before a document can be activated.
[ADR-0008](ADR-0008-Product-Corpus-Storage-And-Page-Image-Evidence.md) expands
that evidence into ten independent rights decisions and blocks visual
activation when applicable rendering, derivative-retention or display rights
are ambiguous.

Primary licences normally use legal or ordinary-language verbs such as use,
copy, modify, distribute and display. They do not normally repeat the exact
names of internal product operations. Requiring a literal textual match between
those external words and each internal enum can therefore leave a broad grant
unmapped even when the evidence may be relevant. Automatically treating a
broad grant as permission for every technical operation would create the
opposite error and weaken the fail-closed boundary.

The product's v2 path streams an exact page PNG from a read-only, same-origin
endpoint to the user's browser after active-state and evidence checks. This is
a delivery of derivative bytes for runtime display. Same-origin and
`Cross-Origin-Resource-Policy: same-origin` are security controls; they are not
licence grants and do not prove that no distribution has occurred.

## Previously observed static policy mismatch

The frozen
[v2 serving contract](../STATE-07-V2-Serving-Contract-Proposal.md#serving-authority-and-validation-order)
requires every page-image request to re-evaluate the complete rights snapshot,
including the intended distribution boundary, before returning `200` or `304`.

The `DocumentRightsEligibilityPolicy.PdfVisualEvidence` gate at the time
required textual rights, `PageRendering`,
`DerivativeImageCreationAndRetention` and
`RuntimeDerivativeImageDisplay`, but did not require or otherwise evaluate
`SourceAndDerivativeByteDistributionOrPublication`. The unit contract then
also treated that visual gate as eligible when the distribution decision was
`Denied`.

This was a static incompatibility between the serving contract and internal
policy coverage. The separately authorised correction in
`b9c3e5f3a72c2dd7762c256198452ae2c217b2d2` added the serving-specific
fail-closed decision and focused tests without changing the public contract.
The later notice-bearing implementation in
`f682827d1a26b08fa8c450a1fadb3bd0e1fa1700` also revalidates the mapping and
bound obligation set during readback and serving. Neither change classifies a
particular source or authorises product activation.

## Decision drivers

- Preserve ten independent decisions rather than collapsing them into a single
  licence outcome.
- Preserve `Permitted`, `Denied` and `Unproven` as fail-closed states.
- Permit explicit semantic mapping without pretending that an internal enum
  must appear literally in primary evidence.
- Prevent a broad grant from silently propagating to operations outside its
  evidenced scope or conditions.
- Treat same-origin as a security boundary, not a rights conclusion.
- Keep runtime display distinct from export, redistribution and publication.
- Make notices, disclaimers, attribution, trademark constraints and change
  marking reproducible for every derivative.
- Reconcile the internal policy without changing OpenAPI v1 or v2.

## Proposed decision

### Ten decisions remain independent

Every document version continues to require exactly one independently
evidenced decision for each existing right:

1. `SourcePossessionOrDownload`;
2. `ParsingAndTextualTransformation`;
3. `Indexing`;
4. `SourceByteRetention`;
5. `QuotationAndCitation`;
6. `PageRendering`;
7. `DerivativeImageCreationAndRetention`;
8. `RuntimeDerivativeImageDisplay`;
9. `SourceAndDerivativeByteDistributionOrPublication`; and
10. `AttributionNoticeTrademarkAndChangeMarkingRequirements`.

No decision inherits the state of another. One primary clause may support more
than one decision only when each operation has its own recorded mapping,
scope, conditions and rationale.

### Explicit evidence mapping

The literal wording of an internal right is not required to appear in primary
evidence. Instead, each decision must point to an auditable mapping containing:

- the exact document identity and version to which the evidence applies;
- the authoritative issuer and immutable evidence reference;
- the exact clause or bounded passage relied upon;
- the internal operation being assessed;
- the relationship between the primary wording and that operation;
- the permitted or prohibited purpose, actors, environment and delivery
  boundary;
- every condition, restriction, notice, expiry, revocation or approval
  dependency;
- the technical mechanism that enforces each applicable condition; and
- the assessor, assessment instant and version of the mapping.

The mapping is an evidence assessment, not a new licence and not a legal
opinion. It must not broaden the primary grant, omit a condition or use silence
as permission.

### Fail-closed state rules

A decision is `Permitted` only when authoritative primary evidence is
applicable to the exact document and the mapping establishes the intended
technical operation within a precise, enforceable boundary. Every applicable
condition must have a determined compliance mechanism.

A decision is `Denied` when authoritative evidence prohibits the operation or
when the intended boundary deliberately excludes it. The evidence reference
must state the exact scope of that denial; a denial cannot silently substitute
for an unassessed operation.

A decision remains `Unproven` when issuer authority, document applicability,
operation mapping, scope, condition, notice mechanism, expiry, revocation or
technical enforcement is absent or ambiguous. `Unproven` blocks every gate
that depends on that operation.

Conflicting primary evidence is `Unproven` until the conflict is resolved by
an authoritative source. Where completing a mapping would require a legal
conclusion rather than an engineering evidence assessment, the review stops
without changing the decision.

### Runtime display boundary

`RuntimeDerivativeImageDisplay` covers only the following product operation:

- the current RAG-Challenge application resolves an image already bound to a
  validated citation and active document version;
- the existing fixed relative same-origin endpoint returns that one bounded
  PNG to the user's browser for presentation inside the application;
- the endpoint repeats current activation, rights, manifest, object identity
  and length checks before `200` or `304`;
- no permissive CORS, public static hosting, unauthorised absolute URL, bulk
  enumeration or export capability is introduced; and
- the applicable source identity and obligation presentation accompany the
  image in the same governed user experience.

This classification does not claim that browser delivery is not copying or
transmission. It means only that the delivery is the narrowly defined technical
act being assessed under `RuntimeDerivativeImageDisplay`. The mapping must
explicitly cover that act; permission merely to retain a derivative is
insufficient.

`SourceAndDerivativeByteDistributionOrPublication` independently covers
making source or derivative bytes available beyond that runtime-display
boundary, including:

- direct source-document download or a derivative-download feature;
- public or static hosting, permissive cross-origin access or CDN publication;
- bulk export, dataset or corpus bundles and offline redistribution;
- Git, Git LFS, release, deployment or seed bundles delivered to another
  environment or party; and
- downstream reuse or republication outside the active RAG-Challenge response.

A `Denied` distribution/publication decision does not automatically deny the
narrow runtime display operation when authoritative evidence and the mapping
explicitly distinguish the two boundaries. A `Permitted` distribution decision
does not automatically permit runtime display. An `Unproven` intended
distribution boundary blocks v2 image serving because the frozen contract
requires that boundary to be re-evaluated before bytes are returned.

### Derivative obligations

For each document version, the mapping must dispose the following obligations
separately as required, prohibited or not applicable, with primary evidence:

| Obligation | Required derivative treatment |
|---|---|
| Attribution | Identify the authoritative publisher or author, document title, version and source reference in the governed derivative context. Never imply endorsement. |
| Copyright and permission notices | Preserve the exact required notice content and associate its immutable reference with the source and every derivative manifest. If the primary terms require the notice inside each binary copy, contextual display alone is insufficient. |
| Disclaimers | Preserve every required disclaimer without abridgement in the location and delivery form established by the mapping. |
| Trademark | Preserve required product naming, marks and non-endorsement constraints. Do not use a mark as a RAG-Challenge brand or source of authority. |
| Change marking | Identify the page image as a rendered derivative and preserve its exact source version, source hash, physical page and render-profile identity. Apply any additional wording or placement required by the primary evidence. |

The source rights record owns the obligation mapping. The source content record
and every derivative manifest retain an immutable reference to the applicable
obligation set. Backup, cold restore and internal retention preserve that
association. Runtime display presents required human-readable material in an
accessible source-details context adjacent or directly linked to the image,
but only when the primary evidence permits that placement.

A distribution bundle must carry every notice and disclaimer required for each
copy and preserve the mapping reference. If a condition requires modifying the
PNG, embedding text in the binary or using a placement the current render
profile cannot produce, generation and serving remain blocked pending a
separately authorised design. The product never assumes that a notice embedded
only in the source PDF automatically accompanies a derivative PNG.

### Implemented internal enforcement

The accepted decision required a separately authorised internal correction
that preserved the frozen public v2 contract and the ten-right schema:

- bind the reviewed mapping and obligation-set identity to the applicable
  rights evidence reference and derivative manifest lineage;
- retain the existing `PdfVisualEvidence` permission checks;
- add a serving-specific fail-closed check that explicitly evaluates
  `SourceAndDerivativeByteDistributionOrPublication` and rejects `Unproven`;
- allow `Denied` at that serving check only when the separately reviewed
  mapping explicitly confines the denial outside the runtime-display boundary;
- reject stale, absent, conflicting or conditionally unenforceable mappings;
  and
- add focused tests for `Permitted`, boundary-confined `Denied`, `Unproven`,
  notice failures and mismatched mapping revisions.

That correction was implemented in
`b9c3e5f3a72c2dd7762c256198452ae2c217b2d2`. The later notice-bearing
behaviour binds and revalidates the exact mapping/obligation lineage in
`f682827d1a26b08fa8c450a1fadb3bd0e1fa1700`. Focused evidence does not replace
the separate notice-bearing AQG or candidate-specific A0.

No OpenAPI field, route, response, schema version or public behaviour is
changed by this decision. If implementing the accepted decision would require
a public-contract change, implementation must stop for separate architecture
and contract authority.

## Candidate-specific non-decision

This decision does not reclassify `postgresql-18-reference-a4`. Its current A0
disposition remains `BLOCKED/EXCLUDED`: four visual operations remain
`UNPROVEN`, while external distribution/publication is `DENIED` under the
recorded internal-only boundary.

Acceptance, reconciliation and local implementation are complete, but a new
separately authorised A0 must still map the already recorded authoritative
PostgreSQL evidence operation by operation. It must preserve all conditions and
may return `Permitted`, `Denied` or `Unproven` independently for each decision.
Neither acceptance nor implementation predetermines that result.

## Alternatives considered

### Keep literal correspondence

Not proposed. It is simple but can reject relevant broad primary wording solely
because the publisher does not use project-owned technical labels.

### Treat a broad grant as permission for all operations

Rejected. It erases independent scope and conditions, can confuse runtime
display with publication and weakens fail-closed evidence handling.

### Treat same-origin as proof that distribution does not occur

Rejected. Same-origin constrains browser access but does not itself grant
rights or eliminate byte transmission.

### Require distribution/publication to be `Permitted` for every runtime image

Not proposed. It collapses the two independent decisions and makes a narrowly
permitted runtime display impossible when broader export or publication is
expressly excluded. The distribution boundary must instead be determined and
compatible, never ignored or `Unproven`.

## Consequences and risks

- Primary evidence can be assessed against technical operations without
  demanding project-specific literal wording.
- Reviews become more explicit and repeatable but require more detailed
  evidence records and condition enforcement.
- Same-origin display remains narrow and fail-closed; it does not become an
  export or publication authority.
- The internal policy correction makes v2 serving coverage match the frozen
  contract; product activation remains blocked by the candidate disposition and
  the still-separate notice-bearing AQG/new A0 sequence.
- Notice placement can require source-specific implementation. Ambiguity or an
  unsupported placement blocks generation or serving.
- This architecture cannot resolve a genuinely legal ambiguity. Such a case
  remains `Unproven` pending owner-supplied authoritative evidence or qualified
  advice outside this ADR.

## Acceptance record

The owner's explicit decision `ADR-0011: ACEITAR.` confirms that this ADR:

1. preserves all ten independent decisions and `Permitted`/`Denied`/
   `Unproven` fail-closed semantics;
2. permits semantic mapping without broadening primary evidence;
3. recognises same-origin delivery as a narrow runtime-display operation, not
   proof of no copying or distribution;
4. preserves a separate, determined distribution/publication boundary;
5. binds notices, disclaimers, attribution, trademark and change marking to
   each derivative lineage and delivery context;
6. records rather than conceals the current contract/policy mismatch;
7. changes no public contract or existing candidate disposition; and
8. leaves implementation, A0 reassessment and all external action under later
   separate authority.

Acceptance establishes architecture authority only. It does not authorise
code, schema, migration, dataset, renderer, runtime, test, gate, source access,
network action, PostgreSQL reclassification or lifecycle progression.
