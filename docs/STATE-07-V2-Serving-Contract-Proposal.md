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
[`openapi-v2.json`](api/openapi-v2.json). This freeze is documentary contract
authority only. It does not implement an endpoint, type, resolver, Dashboard
component, migration or test; does not prove accessibility or security; and
does not authorise runtime, a gate, a provider, a source, a browser, network
access, an external action or a lifecycle transition.

This contract applies the accepted direction in
[ADR-0008](architecture/ADR-0008-Product-Corpus-Storage-And-Page-Image-Evidence.md),
[ADR-0009](architecture/ADR-0009-Document-Evidence-And-Query-Language-Taxonomy.md)
and the [canonical contracts](architecture/STATE-02-Canonical-Contracts.md).
The internal `AnswerEvidenceRecordV1` defined by
[ADR-0010](architecture/ADR-0010-Persistent-Answer-Evidence-Records-And-Bounded-Retention.md)
remains an internal persistence and reachability contract, not a public HTTP
resource.

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
- required `pageImages: PageImageEvidenceV1[]`.

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
- `contentSha256`.

`imageContentObjectId` and `contentSha256` are the same lower-case SHA-256 of
the exact PNG bytes. The reference is created by trusted server orchestration
from the validated citation and persisted readback, never by the language
model, source text, a language tag or caller input. The language model remains
text-only.

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
6. re-evaluate the complete rights snapshot for source use, rendering,
   derivative creation and retention, runtime display and the intended
   distribution boundary;
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

## SEC-IMG-02 acceptance matrix

| Requirement | Required implementation evidence |
|---|---|
| Citation-to-image binding | Only pages covered by the same validated citation are emitted; missing, extra and duplicate page tuples fail closed. |
| Active generation and lifecycle | Current generation succeeds; prior, guessed, deactivated and removed selectors return the uniform `404`. |
| Exact manifest and page | Cross-document, cross-version, cross-manifest, wrong-page and equal-bytes/different-binding selectors return the uniform `404`. |
| Rights | Missing, incompatible, expired or differently scoped display/derivative rights return the uniform `404`. |
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
- a new public outcome or contract field outside this freeze appears necessary;
- tests require weakening an existing control; or
- the work expands to real documents, renderer changes, dataset/homologation,
  browser execution, provider, source, network, integration/restart,
  backup/restore, load, OCI, gate, Human Gate, lifecycle, publication or deploy.

This contract freeze does not correct existing factual-state wording, record
an implementation result or grant the next implementation authority.
