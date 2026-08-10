# ADR-0008 - Product Corpus Storage and Page-Image Evidence

- Status: accepted
- Date: 2026-08-07
- Accepted: 2026-08-07
- Decision authority: explicit product-owner acceptance on baseline
  `main@5c151c64ae4d3049d68fee6788502d439aa25251`, corpus `4.9.4`
- Proposal authority: owner-authorised documentary preparation on baseline
  `main@2696bb7162b0823cead7e391b6259b123142b517`, corpus `4.9.4`
- Owners: RAG-Challenge product, RAG architecture, data governance and security
- State: `STATE-07 TESTING_HOMOLOGATION` accepted architecture decision
- Supersession effect: refines the content-store and governed-document
  decisions; accepted ADR-0011 further refines rights-evidence mapping and the
  same-origin derivative-display boundary; accepted ADR-0012 further refines
  the derivative format and obligation-delivery model without changing the
  current executable contract or schema

## Purpose and authority

This decision defines durable product storage for authorised source documents
and deterministic PDF page images, together with the minimum identity,
rights, lifecycle and response boundaries required to use a relevant page
image as visual evidence.

This ADR records an owner requirement and was explicitly accepted on
2026-08-07. Acceptance does not amend other normative documents by itself,
change the instruction corpus, move or track document bytes, generate images,
materialise a dataset, activate content, change a public contract or authorise
runtime, provider, network or external action.

This ADR refines the content-store decision in
[ADR-0002](ADR-0002-RAG-Lifecycle-Providers-And-Source-Separation.md) and the
governed-document decision in
[ADR-0004](ADR-0004-MVP-Corpus-Official-Source-And-Evaluation.md).
Reconciliation of normative architecture, security, data and API documents
remains a separately authorised increment.

Accepted
[ADR-0011](ADR-0011-Source-Rights-Evidence-Mapping-And-Same-Origin-Derivative-Display-Boundary.md)
now refines this ADR's rights semantics. Its authorised documentary
reconciliation changes neither the ten-right schema nor the public v2 contract,
and it does not reclassify a source candidate or authorise implementation.

## Context

[ADR-0002](ADR-0002-RAG-Lifecycle-Providers-And-Source-Separation.md) makes
immutable, reopenable source bytes the rebuild and rollback authority and
assigns them to `IDocumentContentStore`.
[ADR-0004](ADR-0004-MVP-Corpus-Official-Source-And-Evaluation.md) treats PDF
and CSV documents as governed product-corpus inputs and permits retained
official snapshots only after source-specific rights and provenance
verification. [Security and access](../../prompts/governance/Security-And-Access.md)
classifies raw bytes and snapshots with their source and assigns them to a
durable content store outside Git.

The [current query contract](STATE-02-Canonical-Contracts.md) and
[RAG module](../../prompts/foundation/RAG-Module.md) retrieve textual chunks
and identify PDF evidence by page range. They have no contract for a page-image
artefact. A local ignored intake directory is suitable only for bounded
quarantine: it is not a durable runtime store, deployment input, backup
authority or product record.

The first authorised PostgreSQL intake illustrates the scale. One immutable
PDF contains 3,130 pages. A deterministic page-image policy would therefore
produce 3,130 additional binary objects and a manifest before that document
could offer complete visual evidence.

## Owner requirement

The owner requires that:

- authorised PDFs and CSVs are permanent, central product data rather than
  temporary engineering artefacts;
- original documents and governed derivatives remain reproducible and
  available to the RAG runtime;
- a PDF page may have a persistent PNG representation bound to the exact
  document version, physical page and SHA-256 identities;
- an answer may present the relevant page image as visual evidence without
  sending that image to a language model unless separately authorised;
- every source proves rights for rendering, derivative creation, retention
  and display in addition to parsing, indexing, quotation and citation; and
- an ignored local directory is never the final product store.

## Decision drivers

- Immutable source-of-truth bytes must remain reopenable for rebuild, audit
  and rollback.
- Runtime storage must support content addressing, verified readback,
  deduplication, reference-aware retention and deterministic backup/restore.
- A repository clone must not accumulate every historical revision of large
  binaries as ordinary Git objects.
- Offline operation must not depend on query-time network access.
- Distribution rights differ by source and cannot be inferred from software
  or repository licensing.
- Visual evidence must preserve document, page, provenance and activation
  identity without becoming a new instruction channel.
- Existing `QueryResponseV1` rejects unknown fields and cannot silently gain
  an image contract.

## Options considered

| Option | Strengths | Material limitations | Proposed outcome |
|---|---|---|---|
| Ordinary Git objects | Simple local checkout; native commit history; no additional client | Every binary revision remains in repository history; clones and CI transfer unrelated corpus bytes; deletion and licence withdrawal conflict with distributed history; runtime mutation and reference-aware retention are poor fits | Rejected as the product content store |
| Git LFS | Small Git pointers; binary transfer is separated from ordinary objects; versions remain associated with commits | Requires an LFS service, credentials, quotas and network hydration; a clone is not necessarily offline-complete; pointer availability is not runtime readback; retention and activation remain repository-coupled | Rejected as the product content store; a future distribution channel would require its own decision |
| `IDocumentContentStore` | Existing architectural port; immutable content addressing; idempotent writes; verified reopen; deduplication; reachability-aware deletion; local and deployed implementations can differ | Requires an explicit seed/import, backup and deployment procedure; a Git clone alone does not contain the corpus | Selected |

## Decision

Use `IDocumentContentStore` as the sole system of record for authorised source
bytes and persistent page-image bytes. Ordinary Git and Git LFS are not product
content stores.

Git tracks only stable software, schemas, contract documentation, rights and
provenance records, deterministic fixture content that is explicitly approved
for repository distribution, and sanitised integrity evidence. Runtime source
bytes, official snapshots, page images, indexes and activation records remain
in their owning governed stores.

`artifacts-local/` remains an intake quarantine. A document in that directory
is not durable product content and cannot become `Active`. Import into
`IDocumentContentStore`, verified reopen, catalogue registration, derivative
validation and explicit activation are required before query use.

This storage decision does not prohibit separately authorised, licence-safe
export bundles. Such a bundle is a distribution and recovery artefact, not the
runtime system of record and not an implicit Git payload.

## Content and render artefacts

### Source content object

`DocumentContentObject` is the existing immutable object addressed by the
SHA-256 of its exact bytes. Its descriptor contains:

- `contentObjectId`;
- `sha256`;
- `byteLength`;
- validated `mediaType`;
- content-store implementation descriptor; and
- verified-write and verified-reopen evidence.

The object carries no mutable filename, local path, URL authority, secret or
activation state. Catalogue and official-source records own those concerns.

### Page render profile

The initial accepted profile is `pdf-page-png-v1`:

- input is one verified PDF content object;
- output media type is exactly `image/png`;
- physical pages are numbered from `1`, matching PDF citation page numbers;
- every page is rendered at 144 DPI without cropping or rotation beyond the
  page's declared PDF rotation;
- aspect ratio is preserved;
- output uses 8-bit RGB with an opaque white background;
- metadata that can reveal a workstation path, host or tool command is
  removed;
- width and height are positive and individually bounded at 4,096 pixels;
- renderer ID, renderer version, profile ID and all non-secret render settings
  are recorded; and
- one failed, missing, oversized or unverifiable page fails the complete
  render candidate. It never silently produces a partial active manifest.

Changing DPI, colour model, page transform, size bound, renderer semantics or
output encoding creates a new render profile and manifest. It never overwrites
existing image objects.

#### Accepted notice-bearing successor profile

Accepted ADR-0012 defines `pdf-page-png-notice-v1` as an independent future
profile. It does not reinterpret `pdf-page-png-v1` or make an existing manifest
notice-bearing. For one source page and one immutable
`DerivativeObligationSetV1`, it produces one opaque RGB PNG with:

- a source-page region that is pixel-for-pixel identical to the independently
  validated `pdf-page-png-v1` raster for the same source, page and renderer;
- a separate visible notice panel appended below the final source-page row;
- the complete reviewed attribution, copyright and permission notices, ordered
  disclaimers, trademark or non-endorsement treatment and change marking; and
- a deterministic renderer descriptor that includes the font-asset identity,
  obligation-set SHA-256 and every layout input.

The panel never overlays, crops, scales, recolours or substitutes a source-page
pixel. Missing text or glyphs, font drift, truncation, an oversized composite or
failure of the pixel-region proof rejects the complete candidate. The current
schema and executable v2 contract do not yet represent this profile; their
revision and migration remain mandatory separate increments.

### Page image object

`DocumentPageImage` binds one rendered page to its source:

```text
DocumentPageImage
  documentId
  documentVersion
  sourceContentObjectId
  pageNumber                 # one-based physical PDF page
  renderProfileId
  rendererDescriptor
  imageContentObjectId       # SHA-256 content address of exact PNG bytes
  imageSha256
  byteLength
  mediaType                  # image/png
  widthPixels
  heightPixels
```

`imageContentObjectId` is the immutable byte identity. The complete tuple of
source content object, page number, render profile and renderer descriptor is
the reproducibility identity. Two equal PNG byte sequences may share one
content object, while each document/page binding remains explicit.

Canonical identity rules are:

- `sourceContentObjectId` and `imageContentObjectId` are the existing lower-case
  64-character SHA-256 content identities;
- `renderProfileId` is the stable identifier of the exact versioned profile;
  existing manifests remain `pdf-page-png-v1`, while future notice-bearing
  manifests require `pdf-page-png-notice-v1`; and
- `renderManifestId` is `rendermanifest-` followed by the lower-case
  64-character `manifestSha256`.

### Document render manifest

`DocumentRenderManifest` contains:

```text
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

`orderedPageImages[]` contains exactly one entry for every physical page,
ordered by `pageNumber`. `manifestSha256` uses a versioned canonical UTF-8
serialisation over all identity and measurement fields except `generatedAt`.
Finalisation requires expected page count, consecutive numbering, unique
bindings, byte and dimension limits, image signature validation, SHA-256
recalculation and verified reopen of every referenced content object.

For `pdf-page-png-notice-v1`, a future manifest schema revision also binds one
`obligationSetId`, its canonical SHA-256, source-region width and height,
notice-region height and composite PNG identity. `DerivativeObligationSetV1`
is an immutable control-plane record identified by `obligationset-<sha256>`.
It binds the exact source document/version/object and rights-mapping revision to
ordered evidence references, source language, attribution, notices,
disclaimers, trademark treatment, change marking and placement mode
`VisibleInBinaryAndAccessibleContext`. Any change creates a new obligation set,
new complete manifest and regenerated page bindings; no legacy row is inferred
or mutated.

## Lifecycle and activation

- A new PDF `DocumentVersion` and its render manifest begin as `Candidate`.
- Importing source bytes or rendering PNGs does not make either queryable.
- A PDF candidate is visually complete only when its full render manifest is
  finalised and every image object passes verified reopen.
- Candidate indexing continues to use parsed text. Page images are evidence
  derivatives and are not embedded by default.
- Activation atomically binds the document version, source content object,
  finalised text/index generation and finalised render manifest when visual
  evidence is required for that PDF.
- A future notice-bearing activation additionally binds the exact current
  ten-decision rights snapshot, rights-mapping revision and immutable
  obligation set in the same atomic authority. Missing, stale or mismatched
  obligation evidence fails closed.
- `Deactivated` and `Removed` documents cannot serve page images, even when
  their bytes remain retained.
- Logical removal preserves catalogue, provenance, activation and manifest
  history. Physical deletion requires approved retention expiry and proof
  that no active or retained document, render manifest, answer evidence record
  or rollback target reaches the object.
- A changed source PDF creates a new document version and a new complete render
  candidate. No page-image binding is silently reused across different source
  content identities.

CSV remains a governed source format but has no implicit page-image rendering
under this decision. A future tabular visualisation class requires its own
rights, identity, accessibility and contract decision.

## Query and visual-evidence contract

`QueryResponseV1` and `CitationV1` remain unchanged. The separately versioned
`QueryResponseV2` and visual-evidence route are frozen and implemented in the
current local v2 boundary, with synthetic AQG evidence only.
`QueryResponseV2` retains the completed response semantics and uses
`CitationV2[]` in place of `CitationV1[]`. ADR-0012 does not modify those
current public bytes; its successor fields require a new protected freeze.

`CitationV2` retains every `CitationV1` field and adds
`pageImages: PageImageEvidenceV1[]`. The collection is empty for CSV. An active
PDF citation without an eligible image binding for each referenced page fails
closed instead of returning an incomplete visual reference. Each entry is:

```text
PageImageEvidenceV1
  pageNumber
  renderManifestId
  imageContentObjectId
  mediaType                  # image/png
  widthPixels
  heightPixels
  contentSha256
```

The answer JSON does not inline PNG bytes or expose a filesystem path. A
separately defined, same-origin, read-only evidence endpoint resolves an
authorised `imageContentObjectId`, revalidates the active citation binding and
streams the exact PNG with bounded length, immutable ETag and
`X-Content-Type-Options: nosniff`.

Only page images referenced by validated citations may be offered with an
answer. The server, not the language model, creates the image reference from
the active catalogue, generation and render manifest. Page images do not
change retrieval score, factual grounding or citation validity.

The Dashboard presents each image beside its textual citation with a
source-derived title, document version and page label. The adjacent textual
evidence remains available to assistive technology; a page image is never the
only carrier of a factual claim or navigation meaning.

The initial response policy returns at most five distinct page-image
references and never more than one reference for the same document version and
page. A response requiring more visual evidence reports the remaining cited
pages textually rather than expanding the binary response without bound.

ADR-0012 requires a separately frozen v2 contract revision before a
notice-bearing image can be served. That future revision retains the fixed
same-origin route, adds `obligationSetId` to each notice-bearing page-image
reference and adds one `DerivativeObligationPresentationV1` to the owning PDF
citation. The presentation contains the same complete, bounded content as the
immutable obligation set. OpenAPI v1 remains byte-for-byte unchanged, and the
current OpenAPI v2 remains unchanged until that separate contract increment.

The language model receives textual evidence only. Sending a page image or an
image-derived representation to any provider requires separate provider,
egress, data-use, retention, residency and spend authority.

## Rights and provenance

Every document eligibility record must independently decide and evidence:

- download or owner-supplied possession;
- parsing and textual transformation;
- indexing;
- source-byte retention;
- quotation and citation;
- page rendering;
- creation and retention of derivative images;
- runtime display of derivative images;
- distribution or publication of source and derivative bytes; and
- attribution, notice, trademark and change-marking requirements.

The ten decisions remain separate. One authoritative primary clause may
support multiple decisions, but each operation requires its own explicit,
auditable and conditional mapping. The mapping identifies the exact document,
issuer, evidence reference and relied-upon clause, then records the operation,
purpose, actors, environment, delivery boundary, conditions and enforcement
mechanism. Primary evidence need not contain the literal project-owned right
name, but a broad grant never propagates automatically to another operation.

Each decision remains `Permitted`, `Denied` or `Unproven`. `Permitted` requires
an applicable grant and a determined mechanism for every condition. `Denied`
requires an evidenced prohibition or an explicitly excluded boundary; it does
not substitute for an unassessed operation. Missing issuer authority,
applicability, mapping, scope, condition, notice mechanism, expiry, revocation
or enforcement remains `Unproven`. Conflict or legal ambiguity also remains
`Unproven` pending authoritative resolution. Every dependent gate fails closed
on `Unproven`.

Failure or ambiguity in rendering, derivative retention or runtime display
blocks activation of a PDF document under this decision. It does not silently
infer rights from permission to read, cite or index text. A future policy that
permits text-only PDF activation would require a separately accepted decision
and explicit capability disclosure.

`RuntimeDerivativeImageDisplay` covers only the current application returning
one active, citation-bound and revalidated PNG through the fixed relative
same-origin route for presentation inside its governed citation context. This
HTTP response delivers derivative bytes; same-origin and
`Cross-Origin-Resource-Policy: same-origin` are security controls, not proof
that no copying or distribution occurs. The source-specific mapping must
explicitly permit this delivery act.

`SourceAndDerivativeByteDistributionOrPublication` independently covers
availability beyond that narrow runtime-display boundary, including direct
downloads, public or static hosting, permissive cross-origin delivery, CDN
publication, bulk export, seed or deployment bundles delivered to another
environment or party, Git/Git LFS distribution and downstream republication.
A `Denied` decision for that external boundary does not automatically deny
runtime display when the mapping explicitly separates the scopes. A
`Permitted` distribution decision does not automatically permit runtime
display. An `Unproven` intended distribution boundary blocks v2 image serving.

The source rights record owns the obligation mapping, and the source content
record and every derivative manifest retain an immutable reference to its
applicable obligation set. The mapping disposes attribution, copyright and
permission notices, disclaimers, trademark constraints and change marking
separately. Runtime presentation supplies the required accessible source
details beside or directly linked to the image only when the primary terms
permit that placement. Distribution bundles carry every notice and disclaimer
required for each copy. Where the accepted mapping requires in-binary
placement, only the separately implemented and verified
`pdf-page-png-notice-v1` mechanism may satisfy it; until its contract, schema
and implementation exist, generation and serving remain blocked rather than
approximating the obligation.
Rendering never removes attribution from the governed record or creates an
endorsement claim. An embedded source-PDF notice is not assumed to accompany a
PNG derivative, and Git distribution remains separately governed even when
runtime retention and display are permitted.

## Offline availability, deployment and recovery

- A development or deployed environment uses a configured durable
  `IDocumentContentStore` implementation, never `artifacts-local/` as its
  final root.
- A governed import or seed bundle contains source objects, derivative objects,
  an ordered export manifest, rights/provenance references and hashes. Import
  verifies every object before catalogue or activation use.
- After import, query and visual-evidence serving require no source-network or
  Git LFS access.
- Deployment publishes software separately from corpus data and fails closed
  when an activation record references an absent or mismatched object.
- Backup includes reachable source and image objects plus catalogue,
  observation, render-manifest, index-generation and activation records. For a
  notice-bearing lineage it also includes the immutable obligation set, rights
  mapping and their canonical hashes.
- Restore proves manifest integrity, object readback and activation
  consistency before readiness becomes healthy. A notice-bearing cold restore
  additionally proves obligation-set and manifest digest equality,
  source/mapping/obligation/manifest binding, source/notice-region
  measurements and active/rollback reachability.
- Reachability protects both the composite PNG and its obligation record while
  any active or retained manifest, answer-evidence record or rollback target
  reaches them. Physical deletion requires both to be unreachable.
- Content addressing deduplicates exact source or image bytes without erasing
  document/page provenance.

## Security and operational boundaries

- PDFs, CSVs, rendered pixels, extracted text and image metadata remain
  untrusted content.
- Rendering runs before activation with byte, page, pixel, memory, time and
  concurrency limits and no network access.
- Active content cannot name an arbitrary local path, URL, image object or
  provider input.
- Page-image endpoints enforce active generation, document version, page and
  render-manifest binding before reading bytes.
- Responses never expose content-store paths, internal bucket names, secrets,
  signed URLs or inactive object identifiers.
- Image bytes inherit the source classification and retention policy.
- Content withdrawal or licence restriction fails closed for new display and
  distribution while preserving only the history required by approved
  retention and audit rules.
- Logs contain IDs, hashes, sizes, dimensions, timing and typed outcomes, not
  complete source text or image bytes.

## Current PostgreSQL candidate

The locally downloaded PostgreSQL PDF remains a quarantine candidate at:

`artifacts-local/state-07/source-intake/postgresql-18-reference-a4/postgresql-18-A4.pdf`

Its observed SHA-256 is
`cea7b845568095eb56dee1b51bfa145c6c6637bc4377c986019971577efefae4`.
This acceptance neither imports that file into `IDocumentContentStore` nor
changes its Git status. No retained page PNG exists under this authority.

Its latest candidate-specific A0 mapped the official evidence under ADR-0011.
Page rendering, derivative-image creation, retention and runtime display remain
`UNPROVEN`; distribution/publication outside the accepted same-origin runtime
boundary is `DENIED` by the internal product boundary. The disposition remains
`BLOCKED/EXCLUDED`. Acceptance and this semantic reconciliation of ADR-0012 do
not reclassify those decisions, create an obligation set or execute a new A0.
Only after the mandatory contract, schema, migration and implementation
increments are completed and verified may a separately authorised A0 evaluate
the new mechanism for this candidate.

## Consequences

- Documents and page images become permanent governed product data without
  coupling runtime correctness to a Git checkout.
- A Git clone alone is intentionally not a product-corpus backup or deployment
  package.
- Full PDF rendering increases storage, ingestion time, backup volume and
  activation work in direct proportion to actual page count.
- Visual evidence becomes reproducible and bound to citations, but requires a
  new API contract, content endpoint, security checks and accessibility design.
- Each source requires broader rights evidence than the current textual
  eligibility matrix.
- Withdrawal and physical deletion remain governable because binaries do not
  become irrevocably distributed through ordinary Git history.
- Git LFS may be reconsidered only as a separately authorised distribution
  channel, never as an implicit replacement for catalogue, content-store or
  activation authority.

## Rejected approaches

### Treat an ignored workstation directory as the product store

Rejected because it is neither a durable runtime contract nor a deployable,
backed-up and reference-aware authority.

### Commit every PDF and PNG as ordinary Git objects

Rejected because repository history, clone size, CI transfer and licence
withdrawal scale with every binary revision and do not implement runtime
activation or retention semantics.

### Use Git LFS pointers as runtime content identities

Rejected because LFS hydration, credentials and service availability do not
prove content-store readback, activation eligibility or offline runtime
availability.

### Render pages on demand without persistence

Rejected as the sole strategy because renderer drift would weaken answer
reproducibility and a query would perform unbounded derivative work.

### Send page images to the language model automatically

Rejected because visual display does not grant provider disclosure authority
and the current retrieval/generation contracts are textual.

## Acceptance and follow-on authority

The owner explicitly accepted this ADR on 2026-08-07 on baseline
`main@5c151c64ae4d3049d68fee6788502d439aa25251`, corpus `4.9.4`. Acceptance
establishes architectural authority only; it does not implement the decision,
reconcile other normative documents or authorise corpus movement.

Separate follow-on increments are required for:

1. semantic reconciliation of ADR-0002, ADR-0004,
   `Security-And-Access.md`, `RAG-Module.md`, canonical contracts, data
   dictionary, threat model and public API versioning;
2. implementation and tests for content storage, deterministic rendering,
   manifests, lifecycle and evidence serving;
3. source-specific rights expansion for rendering and derivative images;
4. import of each authorised source object;
5. generation and validation of each render manifest;
6. candidate indexing and atomic activation;
7. dataset materialisation and the `STATE-07` evaluation campaign; and
8. deployment, backup/restore and any external provider or publication action.

Each increment retains its own baseline, scope, checks, stop conditions and
Human Gate where required. No item above is authorised by this acceptance.

The separately authorised semantic reconciliation on 2026-08-07 applies this
decision together with ADR-0009 across its named normative owners as prompt
corpus `4.9.5`. It preserves OpenAPI v1 byte for byte and does not implement,
import, render, index, activate, evaluate, publish or deploy any content.

The separately authorised ADR-0011 reconciliation on 2026-08-09 applies the
accepted mapping, boundary and derivative-obligation semantics to the named
STATE-07 documentary owners. It preserves the public contracts and the
candidate's disposition and does not correct the internal serving policy,
execute a new A0 or authorise product behaviour.

The separately authorised ADR-0012 reconciliation on 2026-08-09 applies the
accepted notice-bearing profile, immutable obligation-set, manifest,
reachability, recovery, same-origin and accessible-presentation semantics to
the named documentary owners. It preserves OpenAPI v1/v2, the ten independent
rights decisions, fail-closed behaviour and the candidate disposition. A
protected v2 contract revision, schema design, migration and implementation
remain mandatory future increments under separate authority.
