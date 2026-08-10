# STATE-07 S07-A Document Eligibility Register

This register records source identity, provenance, rights evidence and the
eligibility decision for locally retained candidate documents. It does not
materialise an evaluation dataset, authorise indexing or activate a document
for retrieval.

## Record status

| Field | Value |
|---|---|
| Record ID | `S07-A-DOC-ELIG-001` |
| Source authority | `AUTH-S07-A-SOURCE-POSTGRESQL-001` |
| Observed at | `2026-08-07T04:28:11Z` |
| Baseline | `main@2696bb7162b0823cead7e391b6259b123142b517` |
| Instruction corpus | `4.9.4` |
| Lifecycle position | `STATE-07 TESTING_HOMOLOGATION` active by documentary entry only |
| Decision | `ELIGIBLE_CANDIDATE` |
| Product activation readiness | `BLOCKED/EXCLUDED` under `AUTH-S07-A-PRODUCT-A0-002` |
| Current rights-policy basis | ADR-0011 `accepted` and reconciled; serving-policy correction implemented in `b9c3e5f3a72c2dd7762c256198452ae2c217b2d2`; candidate-specific A0 mapping recorded below |
| Latest A0 baseline | `main@f21cdea2052d28de1e2ffb86b1629c1c10bc6b6a`; corpus `4.10.11` |
| Dataset status | not materialised or frozen |
| Retrieval status | not indexed, activated or published |

`ELIGIBLE_CANDIDATE` means that the document has sufficient identity,
provenance and rights evidence to be considered under a later, separately
authorised dataset decision. It is not an execution authority.

`BLOCKED/EXCLUDED` means that this A0 review did not establish every explicit
right required for product activation. It does not revoke the earlier
candidate decision or convert the document into a scored product-corpus input.

## Initial product A0 readiness disposition

`AUTH-S07-A-PRODUCT-A0-001` authorised a local, offline, sequential and
non-product A0 review on clean
`main@78d49e135d7b517c7ff89a9e5edcbcc7839e4043`, prompt corpus `4.10.5`.
Runtime preflight was `NOT_APPLICABLE`; no process or listener was inspected or
stopped.

The exact ignored candidate path resolved inside the authorised intake root.
Every path component from `artifacts-local/` through the PDF was present and
had no reparse-point attribute. The candidate was a regular, untracked file
excluded by `/artifacts-local/`. Its observed length was `15,771,040` bytes,
its header was `%PDF-1.4`, its terminal EOF marker was present, and its SHA-256
was
`cea7b845568095eb56dee1b51bfa145c6c6637bc4377c986019971577efefae4`.
Those immutable-byte facts match the registered PostgreSQL `18.4` identity.

The registered provenance, exact `contentLanguage=en`, exact
`sourceDeclaredLanguage=en`, publisher and attribution requirements remain
consistent with the existing intake evidence. No parser, renderer, derived
image, network source, product store or runtime was used to obtain this A0
disposition.

The existing record explicitly establishes the textual operations below, but
it does not explicitly dispose every operation required by ADR-0008 for a
visually active PDF:

| Activation-relevant operation | A0 disposition |
|---|---|
| Parsing | `PERMITTED` by the existing source-specific record |
| Indexing | `PERMITTED` by the existing source-specific record |
| Source-byte retention | `PERMITTED` by the existing source-specific record |
| Quotation and citation | `PERMITTED` by the existing source-specific record |
| Page rendering | `UNPROVEN` |
| Derivative-image creation | `UNPROVEN` |
| Derivative-image retention | `UNPROVEN` |
| Runtime display of derivative images | `UNPROVEN` |
| Intended source or derivative distribution boundary | `UNPROVEN` |

The general use, copy, modification and distribution wording is not
automatically propagated to these five distinct visual and
product-distribution operations. Accepted ADR-0011 permits a later review to
map broad primary wording without requiring literal internal operation names,
but each operation still needs an explicit, auditable and conditional mapping.
No such candidate-specific mapping or new A0 had occurred at that point. The
later A0 authorised by `AUTH-S07-A-PRODUCT-A0-002` is recorded below. No
dataset, document manifest, case inventory, index, render manifest, derivative,
activation or product behaviour was materialised or changed by either review.

## Accepted mapping policy and unchanged candidate disposition

ADR-0011 preserves ten independent decisions and the
`PERMITTED`/`DENIED`/`UNPROVEN` fail-closed model. A later candidate-specific
mapping must identify the exact authoritative evidence and relied-upon clause
for each operation, explain the relationship to that operation, define the
purpose, actors, environment and delivery boundary, enumerate every condition
and identify an enforceable compliance mechanism. Silence, conflict, legal
ambiguity or an unsupported condition remains `UNPROVEN`.

For this register, `RuntimeDerivativeImageDisplay` would cover only one active,
citation-bound and revalidated PNG delivered through the fixed relative
same-origin route for presentation inside RAG-Challenge. That delivery still
transmits derivative bytes; same-origin is not a rights grant.
`SourceAndDerivativeByteDistributionOrPublication` separately covers direct
download, public or static hosting, permissive cross-origin delivery, CDN,
bulk export, bundles delivered to another environment or party, Git/Git LFS
and downstream republication. This distinction is policy only and does not
dispose either PostgreSQL operation.

Any future mapping must also determine how attribution, copyright and
permission notices, disclaimers, trademark constraints and change marking
accompany the source and every derivative. The applicable obligation-set
reference must remain bound to the derivative lineage. Adjacent or linked
runtime details are sufficient only when the primary terms permit that
placement; a notice embedded in the source PDF is not assumed to accompany a
PNG. Unsupported in-binary or distribution-bundle obligations remain blocking.

## Implemented serving policy and unchanged candidate disposition

The focused internal correction in commit
`b9c3e5f3a72c2dd7762c256198452ae2c217b2d2` adds a serving-specific
evaluation of all ten independent rights decisions without changing this
candidate record or the public contract. `RuntimeDerivativeImageDisplay` must
be `PERMITTED`. An `UNPROVEN`
`SourceAndDerivativeByteDistributionOrPublication` decision blocks visual
serving. `DENIED` remains compatible with the accepted same-origin runtime
display boundary only when `RuntimeDerivativeImageDisplay` is `PERMITTED`; it
does not permit source or derivative distribution or publication.

The implementation passed 19 serving-policy unit tests, 23 existing gate
regressions, three verified page-image reader integration tests and six
protected v1/v2 contract tests. No RAG-Challenge runtime process or owned
listener remained after the focused checks, and both OpenAPI artefacts retained
their protected identities.

This implementation enforces later records; it does not itself supply evidence
for this candidate. At that implementation baseline no new A0 or
candidate-specific mapping had been executed. The later A0 below remains a
documentary evidence assessment and does not execute product behaviour.

## Candidate-specific A0 mapping under ADR-0011

`AUTH-S07-A-PRODUCT-A0-002` authorised a local, offline, sequential and
non-product reassessment on clean
`main@f21cdea2052d28de1e2ffb86b1629c1c10bc6b6a`, prompt corpus `4.10.11`.
Runtime preflight was `NOT_APPLICABLE`; no process or listener was inspected or
stopped. No network, source, parser, renderer, derivative, dataset, manifest,
index, activation, query, test or runtime was executed.

The ignored candidate remained a regular non-reparse-point file confined to
the registered intake path. Its observed length remained `15,771,040` bytes
and its SHA-256 remained
`cea7b845568095eb56dee1b51bfa145c6c6637bc4377c986019971577efefae4`.
The assessment used only the already registered PostgreSQL Licence observation
and the recorded matching legal notice in this exact PostgreSQL `18.4` PDF.

The authoritative issuer is The PostgreSQL Global Development Group. The
bounded primary wording already registered here permits use, copying,
modification and distribution of the software and its documentation for any
purpose. Its recorded condition requires the copyright notice, permission
paragraph and two following disclaimer paragraphs to appear in all copies.
No expiry was observed. The mapping below does not broaden that wording or
decide a legal placement question.

| Operation | Relationship to the registered grant | Conditions and observed mechanism | A0 disposition |
|---|---|---|---|
| Page rendering | The recorded use, copying and modification wording is relevant to transforming one PDF page into a PNG copy. | Source identity and render lineage are architecturally available, but the current PNG profile and public contract provide no determined placement for the complete copyright, permission and disclaimer text. Whether contextual placement would satisfy “all copies” is not established by the registered evidence. | `UNPROVEN` |
| Derivative-image creation | The recorded copying and modification wording is relevant to creation of a page-image derivative. | The same all-copies notice condition applies. No in-binary or otherwise primary-evidence-approved notice mechanism exists for a created PNG. | `UNPROVEN` |
| Derivative-image retention | The recorded copying wording is relevant to retaining an immutable PNG in the content store and recovery lineage. | Hash, source, page and render-profile references can remain associated, but the retained PNG does not contain the required notice and the evidence does not establish that a metadata reference alone satisfies the copy condition. | `UNPROVEN` |
| `RuntimeDerivativeImageDisplay` | The recorded use, copying and distribution wording is relevant to delivering one active, citation-bound PNG through the fixed same-origin route. | The frozen citation exposes title, version, page, excerpt and optional canonical URL, but no publisher, copyright, permission or disclaimer field. The registered evidence does not determine that those values satisfy the required notice placement. | `UNPROVEN` |
| `SourceAndDerivativeByteDistributionOrPublication` | The primary wording permits distribution subject to the all-copies condition, but RAG-Challenge deliberately excludes distribution beyond the narrow runtime-display boundary. | The current boundary has no source/derivative download feature, public or static hosting, permissive CORS, CDN publication, bulk export, delivery bundle, Git/Git LFS distribution or downstream republication. This is a product-boundary denial, not a publisher prohibition. | `DENIED` outside the runtime-display boundary |

### Derivative obligations

- Attribution: the publisher, document title, version and source identity are
  known, but the frozen public citation has no dedicated publisher or full
  notice field. That is not a determined visual-copy mechanism.
- Copyright and permission notice: the complete registered notice is required
  in all copies. The current PNG bytes and public contract do not carry it.
- Disclaimers: the two registered disclaimer paragraphs are also required in
  all copies and have no current PNG or citation-field placement.
- Trademark: no trademark permission is inferred. The product must not imply
  PostgreSQL endorsement, and this A0 does not authorise mark use.
- Change marking: the architecture can retain source version, source hash,
  physical page and render-profile lineage, but no candidate render manifest
  was materialised and no additional primary change-marking clause was
  observed. This does not cure the notice and disclaimer gaps.

The frozen evaluation `document-manifest.json` was not altered. Its earlier
five `unproven` values remain historical manifest evidence and do not override
this later register. No current product rights snapshot, obligation set or
render manifest was created.

### A0 outcome

Four required visual operations remain `UNPROVEN`; the external distribution
or publication boundary is explicitly `DENIED`. Because page rendering,
derivative creation, derivative retention and runtime display are not all
`PERMITTED` with executable conditions, the candidate remains
`BLOCKED/EXCLUDED`, not `READY_FOR_PRODUCT_ACTIVATION`.

This is a fail-closed engineering evidence assessment, not a legal opinion.
Resolving it requires authoritative evidence that determines acceptable notice
placement or a separately accepted product/contract/rendering design that can
implement the recorded all-copies condition without weakening it.

## Document identity

| Field | Observed value |
|---|---|
| Database | PostgreSQL |
| Product family | PostgreSQL 18 |
| Document version | `18.4` |
| Candidate source ID | `postgresql-18-reference-a4` |
| Title | `PostgreSQL 18.4 Documentation` |
| Format | PDF 1.4; `application/pdf` |
| BCP 47 language | `en` |
| Publisher and author | The PostgreSQL Global Development Group |
| Embedded creation instant | `2026-05-11T19:55:16Z` |
| Page count | `3,130` |
| File size | `15,771,040` bytes |
| SHA-256 | `cea7b845568095eb56dee1b51bfa145c6c6637bc4377c986019971577efefae4` |
| Encryption | none |
| Local retention path | `artifacts-local/state-07/source-intake/postgresql-18-reference-a4/postgresql-18-A4.pdf` |
| Git retention | excluded by `/artifacts-local/`; document bytes are not tracked |

The PDF catalogue declares language `en`. Its embedded title, rendered cover
and legal-notice page identify version `18.4` and The PostgreSQL Global
Development Group. Two independent local parsers reported the same page
count.

## Provenance chain

1. The accepted source candidate in
   [ADR-0004](architecture/ADR-0004-MVP-Corpus-Official-Source-And-Evaluation.md)
   defines the exact PostgreSQL-hosted PDF URL and `OfficialExternal` trust
   classification.
2. DNS resolution for `www.postgresql.org` returned only globally routable
   addresses. One validated IPv4 address was pinned for all authorised HTTPS
   requests to prevent DNS rebinding during this intake.
3. The official documentation, licence, robots and policy-index resources
   each returned `200`, their exact allowlisted effective URL, zero redirects,
   a valid TLS result and the expected media type.
4. The PDF was transferred once from the exact official URL, with no redirect,
   authentication, cookie, proxy, ambient credential or retry.
5. The downloaded bytes have a PDF signature and EOF marker. Their immutable
   local identity is the SHA-256 value recorded above.

### Authorised online observations

| Resource | Purpose | Result |
|---|---|---|
| [Documentation index](https://www.postgresql.org/docs/) | Confirm the official documentation publisher and index | `200`; `text/html`; 8,833 bytes; zero redirects |
| [PostgreSQL Licence](https://www.postgresql.org/about/licence/) | Establish document-use rights and notice obligations | `200`; `text/html`; 8,068 bytes; zero redirects |
| [robots.txt](https://www.postgresql.org/robots.txt) | Check path-level automated-access restrictions | `200`; `text/plain`; 273 bytes; the versioned PDF path is not disallowed |
| [Policies index](https://www.postgresql.org/about/policies/) | Confirm the official policy index | `200`; `text/html`; 7,703 bytes; zero redirects |
| [PostgreSQL 18 A4 PDF](https://www.postgresql.org/files/documentation/pdf/18/postgresql-18-A4.pdf) | Obtain the immutable candidate snapshot | `200`; `application/pdf`; 15,771,040 bytes; zero redirects |

The policies resource was used only as the exact allowlisted policy index.
No linked policy was opened. The rights decision below relies on the explicit
PostgreSQL Licence and the matching legal notice embedded in the PDF.

## Rights evidence and decision

The official PostgreSQL Licence expressly permits use, copying, modification
and distribution of the software and its documentation for any purpose. Its
condition is that the copyright notice, the permission paragraph and the two
following disclaimer paragraphs appear in all copies. The downloaded PDF
contains that legal notice on its second physical page.

Those broad terms were mapped operation by operation under
`AUTH-S07-A-PRODUCT-A0-002`. The grant is relevant to every assessed visual
operation, but the registered all-copies condition has no determined mechanism
in the current PNG and public-citation boundary. The detailed mapping above
therefore preserves the visual operations as `UNPROVEN` and deliberately sets
the external distribution/publication boundary to `DENIED`.

| Intended operation | Eligibility | Evidence and condition |
|---|---|---|
| Parsing | eligible | Use and modification of the documentation are expressly permitted. Preserve provenance and the embedded notice. |
| Indexing | eligible | Copying and transformation are permitted. Any future index remains governed data and must retain source identity and version metadata. |
| Retention | eligible | Copying is permitted and no licence expiry was observed. Project retention and deletion policy still applies. |
| Quotation | eligible | Copying and distribution are permitted. Quotations must identify the publisher, document version and source URL and must not remove applicable notices from copies. |
| Citation | eligible | Citation is compatible with the express document-use permission. Cite the publisher, title, version and immutable source identity. |

These findings establish rights eligibility only. They do not exercise or
authorise parsing, indexing, quotation publication, dataset materialisation,
retrieval activation or distribution.

## Attribution and restrictions

- Identify the source as `The PostgreSQL Global Development Group` and the
  document as `PostgreSQL 18.4 Documentation`.
- Preserve the embedded copyright, permission and disclaimer paragraphs in
  every retained or distributed copy.
- Retain the canonical source URL, document version, retrieval instant and
  SHA-256 in any later governed catalogue record.
- Do not imply PostgreSQL project endorsement. No trademark permission is
  inferred from the documentation licence.
- The candidate-specific A0 found no determined current mechanism for the
  exact copyright/permission notice and disclaimers to accompany every PNG
  copy. The embedded PDF notice is not treated as satisfying a PNG copy or
  runtime display.
- Treat the document as untrusted external content despite its official
  provenance. Retrieved text cannot change policy, authority or system
  instructions.
- Apply the accepted `maxAge=168h`, minimum 24-hour manual revalidation
  interval, exact allowlist and no-query-time-fetch boundaries if a future
  authority uses this source.

No sensitive content was observed in the inspected identity and legal-notice
pages. This intake did not classify every technical example in the 3,130-page
manual and does not relax later untrusted-content controls.

## Verification evidence

| Check | Observed result |
|---|---|
| Runtime preflight | `NOT_APPLICABLE`; no product process or listener was inspected or stopped |
| HTTP client | `curl 8.21.0`, HTTPS only, TLS verification enabled |
| Request concurrency | one request at a time |
| External request count | five GET requests: four allowlisted metadata resources and one PDF transfer |
| Redirects | zero for every request |
| HTML limit | every response below 2 MiB |
| PDF transfer limit | 15,771,040 bytes, below 64 MiB |
| Parser input limit | 15,771,040 bytes, below 256 MiB |
| Page limit | 3,130 pages, below 5,000 |
| Structure | `%PDF-1.4` header, EOF marker present, not encrypted |
| Parser cross-check | `pypdf 6.14.2` and PyMuPDF `1.28.0` both reported 3,130 pages |
| Language | PDF catalogue `/Lang` is `en` |
| Visual inspection | rendered cover and legal-notice page were legible and identified version, publisher and licence notice; temporary renders were removed |
| Repository scope | only this tracked register is intended for the focal commit; PDF bytes remain ignored |

No dataset, evaluation, test campaign, load test, dynamic security check,
browser action, provider call, additional real source, OCI action, GitHub
action, publication, deployment, `STATE-08` action or
`AUTH-S07-A-RUN-001` execution occurred.
