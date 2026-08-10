# ADR-0012 — Notice-Bearing Page-Image Profile and Derivative Obligation Delivery

- Status: proposed
- Date: 2026-08-09
- Preparation authority:
  `AUTH-S07-A-NOTICE-BEARING-PROFILE-ADR-PREP-001` on
  `main@1b64ca88a0efebd7ab450f5bdc22004a72f3dc53`, corpus `4.10.12`
- Decision authority: not granted; explicit owner acceptance remains required
- Owners: RAG-Challenge product, architecture, data governance, security and
  accessibility
- State: `STATE-07 TESTING_HOMOLOGATION` proposed architecture correction
- Implementation status: not authorised; this proposal changes no code,
  OpenAPI, schema, migration, dataset, rights disposition or runtime behaviour

## Purpose and authority

This proposal defines one deterministic mechanism for a complete copyright,
permission-notice and disclaimer set to accompany every derived page-image
copy while the source-page pixels remain intact. It also identifies the
schema, migration and public v2 contract changes that the mechanism would
require if this ADR were later accepted.

The proposed mechanism is a self-contained notice-bearing PNG: an unchanged
source-page raster occupies one bounded region and a separate visible notice
panel occupies an appended region of the same immutable PNG. The governed
runtime also presents the same obligation text as accessible HTML adjacent to
the image.

This ADR is a proposal only. It does not accept the design, interpret a
licence, determine that a particular notice placement is legally sufficient,
reclassify `postgresql-18-reference-a4`, modify the protected OpenAPI
artefacts, authorise a migration or permit rendering or product activation.
Any ambiguity in primary evidence or required placement remains `Unproven`
under
[ADR-0011](ADR-0011-Source-Rights-Evidence-Mapping-And-Same-Origin-Derivative-Display-Boundary.md).

## Context

ADR-0011 requires every applicable attribution, notice, disclaimer, trademark
and change-marking condition to have an explicit technical enforcement
mechanism. It also requires an immutable obligation-set reference to remain
associated with the source and each derivative manifest. Contextual display is
insufficient when primary evidence requires notice content inside every binary
copy.

The candidate-specific A0 under `AUTH-S07-A-PRODUCT-A0-002` mapped the
registered PostgreSQL Licence to each visual operation. The broad grant is
relevant, but the recorded condition requires the copyright notice, permission
paragraph and two disclaimer paragraphs to appear in all copies. The current
PNG and frozen v2 citation provide no determined mechanism for those complete
texts. Page rendering, derivative-image creation and retention, and
`RuntimeDerivativeImageDisplay` therefore remain `UNPROVEN`.

The implemented system has four relevant constraints:

1. `pdf-page-png-v1` renders exactly one source page at 144 DPI without
   cropping, overlays or an appended panel. Changing page transform, size,
   renderer semantics or output encoding requires a new profile and manifest.
2. Domain validation, render-candidate composition and SQLite constraints
   currently accept only `pdf-page-png-v1`.
3. The PNG validator rejects textual ancillary chunks and requires output
   dimensions to match the source page. PNG metadata therefore cannot carry
   the notices under the current profile and would not provide the required
   visible, accessible presentation.
4. `CitationV2` and `PageImageEvidenceV1` expose source identity, page and
   binary identity but no obligation-set identity or complete human-readable
   notice material. The Dashboard cannot reconstruct source-specific
   obligations without a forbidden hard-coded document list or a public
   contract change.

These constraints make an in-place change to `pdf-page-png-v1` invalid and
make an adjacent-only workaround incomplete. A new versioned render profile,
persistent obligation record and explicit v2 presentation contract are
required for the proposed mechanism.

## Decision drivers

- Keep every source-page pixel unchanged, in its original order and colour.
- Make the complete applicable notice material part of every PNG byte copy.
- Keep the notice material readable without relying on PNG metadata, an
  external source or network access.
- Present the same material accessibly in the governed Dashboard context.
- Preserve the ten independent rights decisions and all fail-closed rules.
- Keep obligation content source-specific and data-driven; never hard-code a
  product or licence in application or Dashboard code.
- Preserve immutable source, derivative, manifest and rights lineage.
- Preserve OpenAPI v1 byte for byte and keep v1 behaviour unchanged.
- Make the required v2, schema and migration changes explicit rather than
  hiding them in an implementation increment.
- Preserve bounded generation, serving, backup, restore and deletion.

## Proposed decision

### New render profile

Introduce the stable profile ID `pdf-page-png-notice-v1`. It is independent of
`pdf-page-png-v1`; neither profile overwrites, reinterprets or reuses the
other's manifest identity.

For one physical PDF page and one immutable derivative obligation set, the new
profile produces exactly one opaque 8-bit RGB `image/png` with two vertically
ordered regions:

1. **Source-page region** — the exact `pdf-page-png-v1` raster for the same
   verified source object, physical page, 144 DPI settings, renderer versions
   and runtime identifier. Every pixel is copied without scaling, cropping,
   recolouring, rotation, overlay or substitution.
2. **Notice region** — a separate opaque white panel appended below the source
   region. It contains the complete ordered human-readable obligation blocks
   and a project-owned derivative marker. It never covers or changes a source
   pixel.

The composite width equals the source-page raster width. The profile uses a
fixed separator, padding, line-breaking algorithm, text direction, font asset,
font size, colour and block order. The renderer descriptor records every
setting plus the exact font-asset identity and obligation-set SHA-256. Locale,
workstation fonts, host paths and current time cannot affect output.

The notice region contains, in this order:

1. a bounded statement that the image is a rendered derivative and identifies
   its source document version and physical page;
2. authoritative publisher or author attribution and source reference;
3. the exact applicable copyright notice;
4. the exact applicable permission notice;
5. every applicable disclaimer in its authoritative order and without
   abridgement;
6. an applicable trademark or non-endorsement statement, or an explicit
   `NotApplicable` treatment owned by the reviewed obligation set; and
7. the required change-marking statement.

The renderer receives these values only from a verified immutable obligation
set. It never extracts legal terms from arbitrary source pages during
rendering, translates source terms, obtains content from the network or
generates missing wording.

The complete composite remains subject to the existing maximum dimension,
pixel, byte, memory, CPU and elapsed-time limits. If the unabridged panel does
not fit, the font asset or a required glyph is unavailable, a block is empty,
or the source-page pixels cannot remain intact, the entire candidate fails
closed. The renderer must not shrink the source page, reduce required text
below the fixed readable size, omit content, split one derivative across
files, or fall back to contextual-only notice.

### Fidelity proof

The new profile requires a deterministic region-level proof before immutable
publication:

- the source-page region dimensions equal the independently validated
  `pdf-page-png-v1` dimensions for the same source page;
- a pixel-by-pixel digest of the source-page region equals the digest of that
  reference raster;
- the notice region begins only after the last source-page row;
- no alpha, crop, resample, colour conversion or overlay affects the
  source-page region; and
- the manifest records source-region width and height, notice-region height,
  obligation-set identity and composite-image identity.

The composite PNG is a marked derivative, not a claim that its complete canvas
is an unmodified publisher page. The source-page region nevertheless remains
pixel-identical visual evidence.

### Immutable derivative obligation set

Introduce `DerivativeObligationSetV1` as a source-specific, immutable
control-plane record. Its ID is `obligationset-` followed by the lower-case
SHA-256 of a versioned canonical UTF-8 serialisation.

The canonical record contains:

```text
DerivativeObligationSetV1
  schemaVersion
  obligationSetId
  documentId
  documentVersion
  sourceContentObjectId
  rightsMappingRevision
  orderedEvidenceReferences[]
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
  placementMode                 # VisibleInBinaryAndAccessibleContext
  canonicalSha256
  assessedAt
  assessorId
```

Each obligation is disposed explicitly as `Required`, `Prohibited` or
`NotApplicable` in its owning rights mapping. Required text is exact and
unabridged. `NotApplicable` is permitted only when supported by the mapping;
silence never becomes `NotApplicable`. Secrets, workstation paths and inferred
legal conclusions are prohibited.

Changing any text, evidence reference, source identity, mapping revision,
placement, font-affecting input or treatment creates a new obligation-set ID
and a new complete render manifest. Page-image bindings are regenerated; the
content store may deduplicate only when the exact composite PNG bytes are
equal. No active object is mutated in place.

### Render-manifest and activation binding

Introduce a new manifest schema revision for the notice-bearing profile. The
manifest binds exactly one `obligationSetId` and its canonical SHA-256 to the
document version, source content object, render profile and renderer
descriptor. Every page entry records its source-region dimensions,
notice-region height and composite byte identity.

Activation atomically binds:

- the active document version and verified source object;
- the current ten-decision rights snapshot and mapping revision;
- the exact derivative obligation set;
- the final complete notice-bearing render manifest; and
- the active text/index generation.

The obligation set must reference the same document version, source object and
rights mapping revision as the activation. Any stale, absent, mismatched,
conflicting or `Unproven` value blocks activation and later serving.

Existing `pdf-page-png-v1` manifests remain immutable historical objects. They
are not reclassified or backfilled with inferred obligation sets. A document
that requires notice inside every image cannot use those manifests for product
activation.

### Persistence schema and migration

Acceptance would require a separately designed and authorised schema revision
and migration. At minimum it must:

- persist immutable obligation sets and their ordered exact text blocks;
- permit `pdf-page-png-notice-v1` alongside the legacy profile in render
  manifest and page-image constraints;
- add the obligation-set identity and digest to notice-bearing manifests;
- add source-region and notice-region measurements needed for fidelity proof;
- include the new fields in canonical manifest identity;
- add foreign keys and conditional checks that require a complete obligation
  set for the notice-bearing profile and prohibit one from being silently
  inferred for a legacy profile;
- extend activation validation and storage reachability to the obligation
  set; and
- preserve every existing manifest, page-image hash and historical activation
  record without mutation.

SQLite table rebuilds required to replace the current exact-profile check
constraints are migrations, not implementation detail. No migration may infer
notice text, reclassify a right or mark a legacy PNG compliant. An absent
obligation set after migration remains a fail-closed absence.

### Content storage, retention and deletion

`IDocumentContentStore` remains the sole system of record for the exact
notice-bearing PNG bytes. The control plane owns the immutable obligation set,
mapping revision and manifest association. Intake quarantine, Git, Git LFS,
the vector store and browser cache are not authorities for either.

The composite image inherits the source classification, retention rule and
withdrawal state. Reachability includes active and retained manifests,
answer-evidence records, rollback targets and the referenced obligation set.
Physical deletion requires proof that no retained authority reaches either the
PNG or obligation record.

### Backup and cold restore

A cold backup includes source objects, composite PNG objects, obligation sets,
rights mappings, render manifests, activation records, answer evidence and
their canonical hashes. It never relies on the original website, browser cache
or a locally ignored intake directory.

Cold restore remains unready until it proves:

- exact readback of every reachable source and composite image object;
- canonical obligation-set and manifest digest equality;
- document/source/mapping/obligation/manifest binding consistency;
- source-region and notice-region measurements;
- active-generation and rollback reachability; and
- absence of an active reference to a missing, legacy-only or stale
  obligation mechanism.

A restored mismatch fails readiness closed and cannot serve a page image.

### Same-origin serving

The existing fixed page-image route and selector shape remain unchanged. For a
notice-bearing image, every `200` and `304` re-evaluates the current activation,
ten rights decisions, mapping revision, obligation-set identity, manifest,
page tuple, composite hash, byte length and dimensions before returning an
authorised outcome.

`RuntimeDerivativeImageDisplay` must be `Permitted` for the exact same-origin
delivery. `SourceAndDerivativeByteDistributionOrPublication` must be
determined and may be `Denied` only for the already accepted boundary outside
runtime display. Same-origin remains a security control, not a rights grant.

An authorised `200` streams the exact stored composite PNG. Its strong ETag is
the composite SHA-256, so the notice bytes are part of cache identity. A `304`
is allowed only after full rights, obligation and lifecycle revalidation. A
changed obligation set therefore creates different image and manifest
identities; it never reuses an old ETag.

Direct downloads, public/static hosting, permissive CORS, CDN publication,
bulk export, delivery bundles, Git/Git LFS distribution and downstream
republication remain outside this mechanism and unauthorised.

### Public v2 contract revision

OpenAPI v1 and all v1 behaviour remain byte-for-byte unchanged. The current v2
contract cannot supply the complete accessible obligation material and cannot
prove which obligation set belongs to a page image. Acceptance would therefore
require a separately frozen OpenAPI v2 revision before implementation.

The proposed v2 revision retains every existing query, citation and page-image
field and adds:

- `obligationSetId` to each notice-bearing page-image reference; and
- one `DerivativeObligationPresentationV1` on the owning PDF citation,
  containing the same bounded publisher/source identification, exact
  attribution, copyright, permission, ordered disclaimers, trademark treatment
  and change-marking text represented by that immutable set.

The citation-level presentation is `null` only when `pageImages` is empty. A
PDF citation containing one or more images must carry exactly one obligation
presentation, and every page reference must name that same set. CSV citations
cannot carry it. Unknown properties, duplicates, mismatches, truncation,
unsupported language or excess size fail response construction closed.

This is a public contract change even though the existing route is preserved.
It requires an explicit contract-freeze increment, regenerated strict clients
and v1/v2 regression evidence. This proposal does not modify the current
OpenAPI v2 bytes or claim backwards compatibility for strict v2 clients.

### Accessible Dashboard presentation

The Dashboard renders the complete obligation presentation as escaped text in
the source-details context immediately adjacent to each notice-bearing figure.
The notice content retains its declared source language; only product-owned
labels follow `interfaceLanguage`.

The image keeps a concise accessible name identifying the document version and
physical page. The full notice is not placed in `alt`, hidden in image metadata
or replaced by a link. It is available as ordinary selectable text, grouped by
heading and associated with the figure. Repeated images using the same set may
share one adjacent presentation only when the association remains explicit for
each figure.

Failure to decode, validate or present the complete obligation object blocks
the image while leaving the textual citation usable. Raw HTML, Markdown,
source-created URLs and executable content remain untrusted text and are never
rendered as markup.

### Attribution, notices, disclaimers, trademark and change marking

- **Attribution:** the exact reviewed publisher/author, document title,
  version and source reference appear in both the PNG panel and accessible
  context. Presentation must not imply endorsement.
- **Copyright and permission notices:** complete reviewed text appears in the
  PNG pixels and accessible context without abbreviation, translation or
  reconstruction from a URL.
- **Disclaimers:** every reviewed paragraph appears in authoritative order and
  remains part of the obligation-set and composite-image digests.
- **Trademark:** no permission is inferred. Required naming and
  non-endorsement wording are explicit; product branding never adopts the
  source's mark.
- **Change marking:** the derivative is labelled as rendered evidence and is
  bound to the exact source version, source SHA-256, physical page, renderer,
  render profile and obligation-set identity.

These mechanisms implement only a mapping already supported by authoritative
evidence. They cannot decide what a licence means, supply missing text or turn
an ambiguous placement requirement into permission.

### Rights and activation consequence

If this ADR is accepted, implemented and verified, a later candidate-specific
A0 may evaluate whether the mechanism satisfies the already mapped technical
conditions. That later A0 independently disposes `PageRendering`,
`DerivativeImageCreationAndRetention`, `RuntimeDerivativeImageDisplay`,
`SourceAndDerivativeByteDistributionOrPublication` and
`AttributionNoticeTrademarkAndChangeMarkingRequirements`.

This proposal does not predetermine any `Permitted` decision.
`postgresql-18-reference-a4` remains `BLOCKED/EXCLUDED`; no source object,
obligation set, manifest, image, dataset, index or activation is created.

## Required follow-on increments if accepted

Acceptance would establish architecture authority only. It would still require
separate sequential authorities for:

1. semantic reconciliation of ADR-0008, the v2 serving contract, the data
   dictionary, security/threat owners and candidate register without changing
   a rights disposition;
2. a protected v2 contract revision and strict client updates while preserving
   v1;
3. the schema design and migration with legacy-record preservation;
4. implementation of the obligation-set model, notice-bearing renderer,
   manifest, storage, reachability, serving and accessible Dashboard;
5. focused verification and an Automatic Quality Gate using only synthetic,
   project-owned fixtures;
6. a separately authorised candidate-specific A0;
7. product rendering, import, indexing and activation only after that A0 is
   eligible; and
8. product-data, browser, assistive-technology and recovery homologation under
   their own authorities.

No item is authorised merely by preparing or accepting this ADR.

## Alternatives considered

### Modify `pdf-page-png-v1` in place

Rejected. It would change established renderer semantics, invalidate
reproducibility and reinterpret existing manifest identities.

### Store notices only in PNG metadata

Rejected. The current validator prohibits textual chunks, metadata is not a
reliable visible or accessible presentation, and using it would still require
a new encoding profile.

### Display notices only beside the existing PNG

Rejected for the stated mechanism. It would not make the material part of each
PNG copy and would require a legal conclusion that contextual placement alone
is sufficient.

### Hard-code the PostgreSQL notice in the Dashboard

Rejected. It couples source eligibility to software releases, bypasses the
governed catalogue, does not scale to compatible documents and does not put the
notice in the binary copy.

### Add the licence page as another cited image

Rejected. The extra page is not necessarily answer evidence, would violate the
exact citation-to-page binding and would still leave the requested PNG without
the notice.

### Stamp or replace source-page pixels

Rejected. Overlay, cropping, scaling or substitution would weaken visual
fidelity and violate the owner's explicit intact-pixel boundary.

## Consequences and risks

- Each derived copy carries its obligations without depending on network or
  contextual placement.
- Exact source-page pixels remain available, but every notice-bearing image is
  taller and larger than the corresponding legacy image.
- Rendering, storage, backup volume and query-response size increase with the
  full obligation content.
- A long obligation set may exceed pixel or byte limits. The result is a
  blocked candidate, never truncated or reduced notices.
- A deterministic, distributable font asset becomes part of the renderer
  trust and reproducibility boundary and requires separate evidence.
- Existing v2 strict clients require coordinated updates because the public
  contract changes.
- SQLite migration risk is material because current constraints admit only one
  profile. Existing rows and hashes must remain immutable.
- Withdrawal or corrected obligations require a new set, new images, a new
  manifest and explicit activation; serving of the prior generation fails
  closed after the switch.
- The design increases accessibility clarity but still requires later browser
  and assistive-technology evidence.
- No technical mechanism resolves uncertain issuer authority, scope or legal
  meaning. Those decisions remain outside this ADR.

## Stop conditions

Any later acceptance, reconciliation or implementation must stop if:

- authoritative evidence does not supply the exact complete obligation text
  and its applicable placement;
- a legal interpretation is required to complete the mapping;
- any source-page pixel would be cropped, scaled, recoloured, covered or
  replaced;
- required text would be abridged, translated, hidden only in metadata or made
  dependent on a network link;
- the complete panel cannot satisfy the existing bounded size and resource
  limits;
- an approved deterministic font asset and its rights cannot be established;
- legacy manifest, image or activation identities cannot remain immutable;
- OpenAPI v1 or v1 behaviour would change;
- the v2 change cannot be explicitly versioned, frozen and verified before
  product use;
- obligation, rights, manifest, activation, backup or restore bindings cannot
  fail closed; or
- implementation would broaden external distribution, provider, network,
  dataset, lifecycle or deployment authority.

## Proposal disposition

`ADR-0012` remains `proposed`. The owner has not accepted it. Preparation
records one technically executable design and its unavoidable impacts; it
does not authorise reconciliation, contract revision, schema work, migration,
code, tests, rendering, dataset materialisation, candidate reclassification,
Automatic Quality Gate, Human Gate or lifecycle progression.
