# STATE-07 HTTP v2 and Visual-Evidence Serving Contract

## Purpose and authority

This document freezes the separately versioned HTTP/OpenAPI v2 contract and
the same-origin page-image serving boundary authorised by
`AUTH-STATE07-V2-CONTRACT-001` on 2026-08-09. The confirmed baseline was
`main@73ff53f714eab03c2eb2918f68284f17139ff804`, prompt corpus `4.10.1` and a
clean working tree. The protected OpenAPI v1 artefact had SHA-256
`d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` and Git
blob `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`.

The versioned machine-readable contract is
[`openapi-v2.json`](api/openapi-v2.json). The original 2026-08-09 freeze was
documentary contract authority only. At that point it did not implement an
endpoint, type, resolver, Dashboard component, migration or test; did not prove
accessibility or security; and did not authorise runtime, a gate, a provider, a
source, a browser, network access, an external action or a lifecycle
transition.

The notice-bearing revision was separately frozen under
`AUTH-S07-A-NOTICE-BEARING-V2-CONTRACT-001` on 2026-08-10 from clean
`main@6982b0643468aee0a97c3bea6b5bbe9018f0804c`, corpus `4.10.15`. It
preserves the protected OpenAPI v1 SHA-256
`d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` and Git
blob `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`. The revised OpenAPI v2 has
SHA-256 `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733`
and Git blob `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`.

This contract applies the accepted direction in
[ADR-0008](architecture/ADR-0008-Product-Corpus-Storage-And-Page-Image-Evidence.md),
[ADR-0009](architecture/ADR-0009-Document-Evidence-And-Query-Language-Taxonomy.md),
[ADR-0011](architecture/ADR-0011-Source-Rights-Evidence-Mapping-And-Same-Origin-Derivative-Display-Boundary.md)
and the [canonical contracts](architecture/STATE-02-Canonical-Contracts.md).
The internal `AnswerEvidenceRecordV1` defined by
[ADR-0010](architecture/ADR-0010-Persistent-Answer-Evidence-Records-And-Bounded-Retention.md)
remains an internal persistence and reachability contract, not a public HTTP
resource.

Accepted
[ADR-0012](architecture/ADR-0012-Notice-Bearing-Page-Image-Profile-And-Derivative-Obligation-Delivery.md)
defines the notice-bearing derivative direction applied by this revision.
`obligationSetId` and `DerivativeObligationPresentationV1` are now frozen v2
transport fields. The later schema and migration increment was implemented in
focused commit `98036f3c8c496544f4532d1fe48c981f836a1871`. The later local behaviour
increment was implemented in focused commit
`f682827d1a26b08fa8c450a1fadb3bd0e1fa1700`, including renderer, projection,
serving validation and accessible Dashboard presentation. Neither increment
authorises candidate reclassification or notice-bearing product serving with
real data by itself.

## Frozen decisions

### Version coexistence

- `POST /api/v2/questions` is the v2 query endpoint.
- `/api/v1/questions`, both v1 health endpoints, every v1 payload and every v1
  failure semantic continue unchanged.
- [`openapi-v1.json`](api/openapi-v1.json) remains byte-for-byte protected at
  its recorded SHA-256 and Git blob.
- V1 and v2 coexist. This contract does not deprecate, redirect, negotiate or
  remove v1 and does not add v2 health endpoints.
- Requests select a contract only through the explicit versioned path. Media
  types, headers, query language or document language never select a version.

### QueryRequestV2

`QueryRequestV2` contains exactly `corpusId`, `questionLanguage` and
`question`, rejects unknown members and preserves the existing public query
authority boundary. `questionLanguage` remains exactly `pt-BR|en-GB`; the
accepted answer language must equal it. The request body is at most 8 KiB and
the UTF-8 question is at most 4 KiB. URL, source, adapter, provider, model,
path, render, image and administration selectors remain prohibited.

The query endpoint preserves the existing token-bucket limit of 30 requests
per minute per derived client key with burst 10, the global ceiling of 20
concurrent queries and the 25-second end-to-end deadline. Its completed JSON
body is bounded to 262,144 bytes. Cancellation is propagated and no automatic
retry is introduced.

### QueryResponseV2 and CitationV2

`QueryResponseV2` preserves the completed v1 semantics and fields, replaces
`CitationV1[]` with `CitationV2[]`, and rejects unknown members. It does not
expose `answerEvidenceRecordId`, a storage row, a path, an image URL, binary
bytes, a rights record or another administration identity.

`CitationV2` preserves every v1 identity, provenance, display and location
field and adds:

- `contentLanguage`, as the exact governed canonical BCP 47 document-language
  tag;
- optional `sourceDeclaredLanguage`, as the exact valid tag observed from the
  source without inferred specificity; and
- required `pageImages: PageImageEvidenceV1[]`; and
- required nullable
  `derivativeObligationPresentation: DerivativeObligationPresentationV1`.

`questionLanguage` and `answerLanguage` do not broaden with document language.
In particular, `en` is not `en-GB`. Source-derived title, excerpt and location
text remain in the governed document language and are not translated as
citation evidence.

For CSV, `pageImages` is always empty. For PDF, every returned page reference
must cover a physical page cited by the same `CitationV2` and match the exact
source content object, document version, active generation and final render
manifest. A cited PDF page without that complete binding fails closed before
an `Answered` v2 response is returned.

One response contains at most five page references globally, ordered by first
citation occurrence and then physical page order. It contains no duplicate
`documentId`/`documentVersion`/`pageNumber` tuple. Remaining cited pages stay
available as textual evidence and are not silently represented by another
page.

### PageImageEvidenceV1

Each `PageImageEvidenceV1` contains exactly:

- `pageNumber`;
- `renderManifestId`;
- `imageContentObjectId`;
- `mediaType`, exactly `image/png`;
- `widthPixels` and `heightPixels`, each from 1 through 4,096; and
- `contentSha256`; and
- required nullable `obligationSetId`, using
  `obligationset-<lower-case SHA-256>` for `pdf-page-png-notice-v1` and `null`
  for the existing `pdf-page-png-v1` projection.

`imageContentObjectId` and `contentSha256` are the same lower-case SHA-256 of
the exact PNG bytes. The reference is created by trusted server orchestration
from the validated citation and persisted readback, never by the language
model, source text, a language tag or caller input. The language model remains
text-only.

### Frozen notice-bearing v2 revision

This protected revision retains every prior field and the fixed same-origin
route, and adds:

- `obligationSetId` to every `pdf-page-png-notice-v1` page-image reference; and
- one `DerivativeObligationPresentationV1` to the owning PDF citation, carrying
  the same complete publisher/source attribution, copyright and permission
  notices, ordered disclaimers, trademark treatment and change marking as the
  immutable obligation set.

A PDF citation with notice-bearing images names exactly one obligation set,
every page reference names that same set and the presentation repeats that
identity. `contentLanguage` also matches the owning citation. Missing,
duplicate, mixed, mismatched, unsupported-language, empty or oversized
obligation content fails strict decoding closed. The trusted producer must
also verify the exact unabridged text against the immutable obligation set;
that semantic equality cannot be inferred by a public client from text alone.

Compatibility is explicit and narrow: a legacy `pdf-page-png-v1` page has
`obligationSetId: null` and its citation has
`derivativeObligationPresentation: null`. A citation cannot mix legacy and
notice-bearing pages. A PDF without page images and every CSV citation also
uses a null presentation; CSV continues to forbid page images. The null legacy
case preserves the existing v2 projection but never represents, upgrades or
authorises a notice-bearing derivative.

`DerivativeObligationPresentationV1` is strict and rejects unknown members. It
contains the obligation-set identity, exact `contentLanguage`, bounded
publisher/author, title, version and source reference, complete attribution,
copyright and permission text, up to 16 ordered unabridged disclaimers,
trademark treatment `Required|Prohibited|NotApplicable`, the corresponding
trademark or non-endorsement text and change marking. All values are untrusted
plain text; no field grants navigation, markup or execution authority.

## Same-origin visual-evidence endpoint

The frozen route is:

```text
GET /api/v2/evidence/page-images/{indexGenerationId}/{renderManifestId}/{pageNumber}/{imageContentObjectId}
```

The selector deliberately contains the active generation, final manifest,
one-based physical page and immutable PNG identity. `renderManifestId` resolves
the document version and source object. The complete selector prevents equal
PNG bytes reused by another document or page from becoming authority for the
requested citation, and it makes a URL from an earlier generation fail closed
after activation changes.

The Dashboard may construct only this fixed relative same-origin path from an
already validated `QueryResponseV2`. It must not accept an absolute URL or
construct an image source from model text, source-derived text, language,
document URL or filesystem data.

Under accepted ADR-0011, this exact operation belongs to
`RuntimeDerivativeImageDisplay`: one active, citation-bound and revalidated PNG
is returned to the user's browser for presentation inside the governed
RAG-Challenge citation context. The HTTP response still delivers derivative
bytes. Same-origin, private revalidation and
`Cross-Origin-Resource-Policy: same-origin` are security controls, not evidence
that no copying or distribution occurs and not a rights grant.

`SourceAndDerivativeByteDistributionOrPublication` independently governs
availability beyond this narrow display boundary, including direct downloads,
public or static hosting, permissive cross-origin delivery, CDN publication,
bulk export, seed or deployment bundles delivered to another environment or
party, Git/Git LFS distribution and downstream republication. This semantic
clarification changes no route, field, response, error or OpenAPI byte.

### Serving authority and validation order

The endpoint is authorised by the current active product state, not by an
answer-history session or by possession of an internal answer-evidence ID. On
every GET, including a conditional GET that may return `304`, the server must:

1. parse all four selector members within their exact formats and bounds;
2. resolve the single configured corpus and its current activation revision;
3. require the selector's `indexGenerationId` to be the current active
   generation;
4. resolve `renderManifestId` as a final complete `pdf-page-png-v1` manifest
   exactly bound by that activation to one active PDF document version and
   source content object;
5. require the document and its database product to be active, never
   `Deactivated` or `Removed`;
6. re-evaluate the complete ten-decision rights snapshot and its current
   evidence mappings for source use, rendering, derivative creation and
   retention, runtime display, the intended distribution boundary and every
   applicable attribution, notice, disclaimer, trademark or change-marking
   condition;
7. require the exact page tuple to contain the requested page number,
   `imageContentObjectId`, SHA-256, byte length, PNG media type and bounded
   dimensions;
8. reopen the object through `IDocumentContentStore` with the expected hash and
   length, validate the PNG identity and reject symlinks, reparse points and
   paths as authority; and
9. only then compare `If-None-Match` or stream the exact bytes.

An expired or deleted `AnswerEvidenceRecordV1` neither grants nor removes
serving authority. Its `P30D` rule remains a reachability concern. The endpoint
does not expose the record ID and introduces no public retention or history
semantics. If later requirements need answer-scoped grants rather than the
active-state selector above, implementation must stop for a new contract and
possible persistence decision.

`RuntimeDerivativeImageDisplay` must be `Permitted` for this exact endpoint
operation. The independent intended distribution boundary must be explicitly
determined and cannot be `Unproven`. A `Denied`
`SourceAndDerivativeByteDistributionOrPublication` decision is compatible only
when its audited mapping confines the denial outside this runtime-display
boundary; `Permitted` distribution does not substitute for permitted runtime
display. Missing, stale, conflicting, legally ambiguous or unenforceable
mappings fail closed before `200` or `304`.

For the notice-bearing profile, a later serving implementation of this frozen
contract additionally
requires the exact current rights-mapping revision, immutable obligation-set
identity and digest, notice-bearing manifest schema, source/notice-region
measurements and composite PNG identity. The strong ETag is the composite PNG
SHA-256, so any obligation change creates new image, manifest and ETag
identities. A conditional `304` still follows the full obligation, rights and
lifecycle revalidation. These checks cannot be enabled without the separately
authorised schema and runtime implementation or inferred from legacy data.

The endpoint has the same caller access policy as textual query evidence,
publishes no permissive CORS authority and adds
`Cross-Origin-Resource-Policy: same-origin`. A later authentication policy must
apply equivalently to query and visual evidence and requires separate
authority.

### Successful and conditional responses

A `200` response is exactly `image/png` and carries:

- the exact positive `Content-Length`, no greater than 67,108,864 bytes;
- a strong `ETag` formatted as `"sha256-{contentSha256}"`;
- `Cache-Control: private, no-cache`;
- `X-Content-Type-Options: nosniff`; and
- `Cross-Origin-Resource-Policy: same-origin`.

The ETag is immutable for those bytes, but the cache policy deliberately
requires lifecycle revalidation before reuse. After completing the full
authority checks, an exact `If-None-Match` produces `304` with the same ETag,
cache, `nosniff` and resource-policy headers and no body. Weak, malformed,
multiple or non-matching validators do not bypass validation and result in a
normal authorised `200` when the object is available.

Range serving, `206`, content negotiation to another media type, compression,
redirects and automatic retries are not supported. A `Range` header does not
select partial content and the endpoint returns the complete authorised PNG.
Each request returns at most one object, has a 30-second complete-operation
deadline, a token bucket of 30 requests per minute per derived client key with
burst 10, and a global ceiling of four simultaneous visual transfers.

### Uniform failures

The endpoint does not reveal whether a selector once existed or which
authority check failed.

| HTTP | Stable code | Frozen meaning |
|---:|---|---|
| `404` | `CH_VISUAL_EVIDENCE_NOT_AVAILABLE` | Malformed, unknown, guessed, stale, cross-generation, tuple-mismatched, inactive, removed or rights-ineligible selector. All cases are indistinguishable. |
| `429` | `CH_VISUAL_EVIDENCE_RATE_LIMITED` | The visual request or concurrency budget is exhausted; `Retry-After` is present. |
| `503` | `CH_VISUAL_EVIDENCE_UNAVAILABLE` | A currently authorised tuple cannot be verified or read back safely because its required local authority is unavailable or corrupt. |
| `500` | `CH_UNEXPECTED_FAILURE` | An unexpected sanitised failure occurred. |

Failures use the v2 RFC 9457 Problem Details schema, contain a correlation ID
and never contain a path, source text, URL, rights content, exception, stack,
provider payload or indication of which selector component matched. Query v2
continues to use the existing canonical query failure mappings.

## Dashboard presentation and accessibility

The Dashboard presents each image inside its owning citation, adjacent to the
source-derived title, document version, physical page label and textual
excerpt. The title and excerpt retain their source language. Product-owned
controls and status text follow `interfaceLanguage`, independently of query,
answer and evidence language.

The activation-bound rights evidence and derivative lineage retain the
source-specific attribution, copyright and permission notices, disclaimers,
trademark constraints and change marking required by the current mapping. The
existing citation context supplies only its already frozen source title,
version, page, excerpt and applicable canonical URL. If those existing values
cannot satisfy the mapped presentation obligation, the image is ineligible;
this reconciliation does not add a public notice field or endpoint. An
embedded source-PDF notice is not assumed to accompany a PNG response, and a
requirement for unsupported in-binary placement also makes the image
ineligible.

ADR-0012 accepts the mechanism for that in-binary case: the complete
reviewed obligation content appears in the separate panel of the same PNG and
as escaped, selectable text immediately adjacent to the owning figure. The
source-page pixels remain intact; the concise accessible name still identifies
only document version and physical page, while the full notice is never hidden
in `alt`, metadata or a link. Failure to decode, validate or present the exact
obligation object blocks the image and leaves the textual citation usable.
The transport fields are frozen by this revision. Their local accessible
presentation was subsequently implemented in
`f682827d1a26b08fa8c450a1fadb3bd0e1fa1700`; browser and assistive-technology
homologation remain `NOT_RUN`.

The page image is supplemental evidence, never the only carrier of a factual
claim, navigation destination, error or status. The image has known width and
height, a concise product-owned accessible name identifying the document
version and page, and an adjacent textual equivalent that remains present when
the image is loading, unavailable, blocked or cannot be perceived. A failed
image load does not remove or replace the citation.

Rendering remains text-safe. Raw HTML, SVG, `data:` URLs, model-created URLs,
source-created URLs and executable image fallbacks are prohibited. The
Dashboard CSP must permit the same-origin PNG route without broadening scripts,
styles, objects, connections or external image origins.

## Bounded implementation surface

A later, separately authorised implementation is expected to add or update
only the responsibility owners needed for:

- v2 transport contracts and endpoints under
  `src/RagChallenge.Server.Api/Contracts/V2/`;
- query result projection and a visual-serving Application boundary under
  `src/RagChallenge.Application/IndexingRetrieval/`;
- exact BCP 47 and `sourceDeclaredLanguage` projection in the existing query
  activation reader;
- a focused Infrastructure reader over existing activation, rights, manifest,
  page, answer-evidence and content-store authorities;
- Server composition, rate/concurrency controls and disabled fail-closed
  defaults;
- Dashboard v2 decoding, same-origin image selection, accessible presentation,
  localisation and CSP; and
- focused unit, API contract, SQLite integration, architecture and Dashboard
  tests, including byte-for-byte v1 regression.

The existing schema already stores document language, source-declared
language, rights snapshots, activation evidence, render manifests, page
bindings and answer-evidence pages. The minimum implementation therefore
requires no migration, inferred backfill, new dependency or lockfile change.
Discovery of a need for a persisted public grant, token, session or different
authority model is a stop condition, not permission to add a migration.

That no-migration statement applies only to the frozen and already implemented
`pdf-page-png-v1` contract. ADR-0012 required a separate schema and migration
for immutable `DerivativeObligationSetV1` rows, ordered exact text, profile
constraints, manifest obligation identity/digest, region measurements, foreign
keys and reachability. That increment is implemented in
`98036f3c8c496544f4532d1fe48c981f836a1871`, alongside the separately frozen
public v2 revision above. Legacy rows and hashes remain immutable and received
no inferred backfill.

## SEC-IMG-02 acceptance matrix

| Requirement | Required implementation evidence |
|---|---|
| Citation-to-image binding | Only pages covered by the same validated citation are emitted; missing, extra and duplicate page tuples fail closed. |
| Active generation and lifecycle | Current generation succeeds; prior, guessed, deactivated and removed selectors return the uniform `404`. |
| Exact manifest and page | Cross-document, cross-version, cross-manifest, wrong-page and equal-bytes/different-binding selectors return the uniform `404`. |
| Rights | The ten decisions remain independent. Missing, incompatible, expired, `Unproven`, stale-mapped or differently scoped display, derivative, intended-distribution or obligation decisions return the uniform `404`; a distribution `Denied` is compatible only when its mapping expressly excludes the boundary beyond same-origin runtime display. |
| Notice-bearing contract | Legacy pages use both new values as null. Notice-bearing pages share one non-null `obligationSetId` and one matching complete citation presentation; mixed, missing, mismatched, unknown, empty or oversized values fail strict decoding, while the trusted producer verifies unabridged semantic equality. |
| PNG integrity and bounds | Media type, signature, hash, stored length, dimensions, verified reopen and the 64 MiB serving ceiling are checked before headers or bytes. |
| Conditional cache safety | Every `304` follows full revalidation; deactivation between requests changes the result to the uniform `404`. |
| Same-origin boundary | No permissive CORS; `Cross-Origin-Resource-Policy: same-origin`; no redirects, paths or arbitrary URL selectors. |
| Response hardening | Exact `image/png`, strong ETag, `private, no-cache`, `nosniff`, one object and bounded deadline/concurrency. |
| Accessible equivalent | Source title/version/page and textual citation remain adjacent and usable without the image in both interface languages and themes. |
| Untrusted output | XSS and URL-injection cases across answer, citation, metadata, language tags and failures remain text-safe. |
| V1 compatibility | Protected OpenAPI v1 SHA-256 and blob, endpoint behaviour, closed languages and strict client tests remain unchanged. |

Implementation verification must include focused pure and SQLite tests, v1/v2
contract tests, Dashboard lint/typecheck/tests/build, repository audit and the
complete offline CI. Browser, assistive-technology, product-data, provider,
source, load, recovery, Linux, OCI and production evidence remain separately
authorised `NOT_RUN` boundaries.

## Stop conditions and negative scope

A future executor must stop before or during implementation if:

- OpenAPI v1 or v1 behaviour would change;
- the composite selector cannot prove one exact active citation-to-page binding;
- an internal answer-evidence ID, path, URL, byte payload or rights record would
  become public;
- a public grant, new persistence model, schema change, migration, dependency
  or lockfile appears necessary;
- BCP 47 handling would infer specificity or broaden query/answer languages;
- lifecycle and rights cannot be revalidated before both `200` and `304`;
- the intended distribution boundary or any required derivative obligation is
  absent, `Unproven`, stale, conflicting or not enforceable in the current
  delivery context;
- a new public outcome or contract field outside this freeze appears necessary;
- tests require weakening an existing control; or
- the work expands to real documents, renderer changes, dataset/homologation,
  browser execution, provider, source, network, integration/restart,
  backup/restore, load, OCI, gate, Human Gate, lifecycle, publication or deploy.

This contract freeze does not correct existing factual-state wording, record
an implementation result or grant the next implementation authority.

The ADR-0011 documentary reconciliation records the accepted rights semantics
without changing this frozen public contract. The previously observed internal
mismatch was corrected in focal commit
`b9c3e5f3a72c2dd7762c256198452ae2c217b2d2`: serving now evaluates all ten
decisions and fails closed on an `Unproven` intended distribution boundary.

The ADR-0012 contract revision is now frozen in OpenAPI v2 and the strict
server/Dashboard transport owners. Schema design and migration are implemented
in `98036f3c8c496544f4532d1fe48c981f836a1871`; obligation-set composition,
renderer, manifest finalisation, storage/reachability behaviour, serving and
accessible Dashboard presentation are implemented locally in
`f682827d1a26b08fa8c450a1fadb3bd0e1fa1700`. Focused implementation evidence
does not replace the separate Automatic Quality Gate, a new candidate-specific
A0 or product-data/browser/assistive-technology homologation. The current
PostgreSQL candidate remains `BLOCKED/EXCLUDED`; no new A0 is performed here.
