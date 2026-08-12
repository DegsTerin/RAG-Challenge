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
| Product activation readiness | `ELIGIBLE_CANDIDATE` under `AUTH-S07-A-PRODUCT-A0-003`; product materialisation and activation remain `NOT_RUN` |
| Current rights-policy basis | ADR-0011 `accepted` and reconciled; serving-policy correction implemented in `b9c3e5f3a72c2dd7762c256198452ae2c217b2d2`; ADR-0012 `accepted` and reconciled, with protected v2 contract, schema/migrations and local notice-bearing behaviour implemented; its Automatic Quality Gate is `APPROVED`; the candidate-specific A0 mapping under `AUTH-S07-A-PRODUCT-A0-003` is current |
| Latest A0 baseline | `main@f5bea053e12b189c472559142107331ad3b2e9d9`; corpus `4.10.35` |
| Dataset status | not materialised or frozen |
| Retrieval status | not indexed, activated or published |

`ELIGIBLE_CANDIDATE` means that the document has sufficient identity,
provenance and rights evidence to be considered under a later, separately
authorised dataset decision. It is not an execution authority.

The `BLOCKED/EXCLUDED` descriptions retained below are the immutable outcomes
of A0-001 and A0-002. A0-003 removes that rights block only for the exact
conditional mapping recorded in its current section. `ELIGIBLE_CANDIDATE` does
not make the document a scored input, create product artefacts or activate it.

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
ADR-0012 now supplies an accepted technical design for carrying the complete
recorded notice set without changing source-page pixels. It does not prove that
the design satisfies this candidate's conditions and does not change this A0.
The protected contract and schema/migration increments are now implemented.
The local notice-bearing behaviour is also implemented in
`f682827d1a26b08fa8c450a1fadb3bd0e1fa1700`. Resolution still requires its
separately authorised Automatic Quality Gate followed by a new
candidate-specific A0, or additional authoritative evidence that changes the
mapping.

## Accepted notice-bearing mechanism and unchanged A0

The accepted and semantically reconciled ADR-0012 defines
`pdf-page-png-notice-v1`. Each composite PNG retains an independently
validated source-page region pixel for pixel and appends a separate panel with
the complete reviewed attribution, copyright and permission notices, ordered
disclaimers, trademark/non-endorsement treatment and change marking. Nothing in
this reconciliation generates that PNG or decides that the placement satisfies
the PostgreSQL terms.

`DerivativeObligationSetV1` is the immutable source-specific record for
those exact blocks. Its canonical identity binds the PostgreSQL document
version and source object to the rights-mapping revision and evidence
references. The notice-bearing manifest, activation, content-store
lineage, reachability, backup/cold restore, same-origin `200`/`304` validation
and accessible adjacent presentation must all reference that same set and fail
closed on absence, staleness, mismatch or truncation.

The protected v2 contract can expose `obligationSetId` and
`DerivativeObligationPresentationV1`, and the schema implemented in
`98036f3c8c496544f4532d1fe48c981f836a1871` can persist the immutable model,
its ordered blocks, manifest identity/digest and source/notice-region
dimensions. Existing `pdf-page-png-v1` manifests were not backfilled or
reclassified. The generic local behaviour implemented in
`f682827d1a26b08fa8c450a1fadb3bd0e1fa1700` composes and persists the bound
obligation set, finalises the notice-bearing manifest and image, revalidates
the bindings during readback/serving and presents the exact text accessibly.
No PostgreSQL candidate-specific obligation set, notice-bearing manifest or
derivative was created.

Accordingly, page rendering, derivative-image creation, derivative-image
retention and `RuntimeDerivativeImageDisplay` remain `UNPROVEN` for
`postgresql-18-reference-a4`; external distribution/publication remains
`DENIED` by the recorded internal boundary. The candidate remains
`BLOCKED/EXCLUDED`, no new A0 was executed and no obligation set, manifest,
derivative, dataset, index or activation was created for that candidate.

## Candidate-specific A0 after the approved notice-bearing gate

`AUTH-S07-A-PRODUCT-A0-003` authorised a local, offline, sequential and
non-product reassessment on clean
`main@f5bea053e12b189c472559142107331ad3b2e9d9`, prompt corpus `4.10.35`.
Runtime preflight was `NOT_APPLICABLE`; no process or listener was enumerated
or stopped. The assessment used only the already registered PostgreSQL
Licence observation, the matching notice recorded for this exact PostgreSQL
`18.4` PDF, ADR-0011, ADR-0012 and the reconciled `APPROVED` result of
`AUTH-S07-A-NOTICE-BEARING-PROFILE-AQG-RETEST-001`.

The ignored candidate remained a regular, non-reparse-point file at the exact
registered path. Its observed length was `15,771,040` bytes and its SHA-256
remained
`cea7b845568095eb56dee1b51bfa145c6c6637bc4377c986019971577efefae4`.
No network source, parser, renderer, derivative, dataset, manifest, index,
activation, query, executable test or runtime was used.

The authoritative primary wording already registered here permits use,
copying, modification and distribution of the software and its documentation
for any purpose. Its recorded condition is explicit: the copyright notice,
permission paragraph and two following disclaimer paragraphs must appear in
all copies. No expiry, conflicting evidence or additional placement condition
was observed in the registered evidence. This A0 does not infer that contextual
display alone is sufficient. It relies only on the stricter approved mechanism
that makes the complete registered blocks visible pixels inside every
notice-bearing PNG and also presents the same content accessibly.

| Operation | Relationship to the registered grant | Exact enforceable boundary and conditions | A0-003 disposition |
|---|---|---|---|
| Page rendering | The recorded use, copying and modification wording applies to transforming one verified PDF page into a PNG copy. | `PERMITTED` only through `pdf-page-png-notice-v1`, with the exact verified source identity, intact source-page pixels, complete unabridged registered notice blocks inside the PNG, explicit derivative/change marking and fail-closed rejection of absence, mismatch, truncation or limit failure. The legacy profile is ineligible for this candidate. | `PERMITTED` with the recorded conditions |
| Derivative-image creation | The recorded copying and modification wording applies to creating the marked page-image derivative. | `PERMITTED` only when one immutable `DerivativeObligationSetV1` supplies the complete exact blocks and is bound by identity and digest to the source, rights-mapping revision, notice-bearing manifest and composite bytes. No inferred or generated wording is allowed. | `PERMITTED` with the recorded conditions |
| Derivative-image retention | The recorded copying wording applies to retaining the immutable composite in the governed content store. | `PERMITTED` only with immutable obligation, manifest and content identities; source classification and retention; reachability across active/retained manifests, evidence and rollback; verified backup/cold restore; and no Git, Git LFS, intake directory or browser cache as authority. | `PERMITTED` with the recorded conditions |
| `RuntimeDerivativeImageDisplay` | The recorded use, copying and distribution wording applies to delivering one active, citation-bound derivative through the fixed same-origin route. | `PERMITTED` only for the accepted runtime-display boundary: the exact stored notice-bearing PNG, active citation and generation, current ten-decision snapshot, mapping/obligation/manifest tuple, hash, length and dimensions are revalidated before `200` or `304`; the complete obligation text is presented accessibly; no download, permissive CORS, static hosting, bulk enumeration or export is introduced. | `PERMITTED` with the recorded conditions |
| `SourceAndDerivativeByteDistributionOrPublication` | The primary wording permits distribution subject to the all-copies condition, but RAG-Challenge deliberately excludes delivery beyond the narrow runtime-display boundary. | Direct download, public/static hosting, permissive cross-origin access, CDN, bulk export, dataset/corpus or deployment bundles, Git/Git LFS and downstream republication remain outside the intended product boundary. This is a determined product-policy denial, not a publisher prohibition. | `DENIED` outside the runtime-display boundary |

### A0-003 outcome

The four assessed visual operations are `PERMITTED` only under the complete
conditions above. The external distribution/publication boundary remains
explicitly `DENIED` and is compatible with the separately `PERMITTED` narrow
same-origin runtime display under ADR-0011 and ADR-0012. No condition was
omitted, no broad grant was propagated silently and no legal conclusion about
contextual-only placement was required.

The candidate is no longer `BLOCKED/EXCLUDED` by these five rights decisions.
It remains an `ELIGIBLE_CANDIDATE` that may enter a separately authorised
product-materialisation decision. It is not indexed, rendered, included in a
product dataset, activated or `READY_FOR_PRODUCT_ACTIVATION`: those states
still require actual source/obligation/manifest/generation artefacts and their
own gates. No candidate-specific obligation set, render manifest, derivative,
dataset, qrel, vector, index generation or activation was created by this A0.

This is a fail-closed engineering evidence assessment, not legal advice. Any
future source drift, different notice text, conflicting evidence, missing
glyph, incomplete panel, stale mapping, identity mismatch or boundary
broadening returns the affected decision to `UNPROVEN` or requires a new A0.

This A0 changed only this register, Current State, the append-only State
Transition Log and the Prompt System Change Log. Documentary validation passed
`git diff --check` with exit code `0` and `eng/check-repository.ps1` with 308
non-ignored files. Build, executable tests, `eng/ci.ps1` and product behaviour
remained `NOT_RUN`.

### Approved project-owned derivative-obligation disposition

Under
`AUTH-S07-A-PRODUCT-ADMIN-NOTICE-BEARING-PROJECT-OWNED-DISPOSITION-PROPOSAL-001`,
the product owner reviewed a read-only proposal and explicitly approved the
following exact candidate-specific values. The approval is a project-owned
control disposition. It is not primary-source wording, a new licence
interpretation or evidence that an obligation set, render manifest, generation
or activation exists.

```json
{
  "attributionText": "Source: The PostgreSQL Global Development Group; document: PostgreSQL 18.4 Documentation; version: 18.4; source reference: https://www.postgresql.org/files/documentation/pdf/18/postgresql-18-A4.pdf",
  "trademarkTreatment": "Required",
  "trademarkOrNonEndorsementText": "Do not imply PostgreSQL project endorsement. No trademark permission is inferred from the documentation licence.",
  "changeMarkingText": "The composite PNG is a marked derivative, not a claim that its complete canvas is an unmodified publisher page. The source-page region nevertheless remains pixel-identical visual evidence.",
  "assessedAt": "2026-08-12T04:05:14.0000000+00:00",
  "assessorId": "assessor:auth-s07-a-product-a0-003"
}
```

`attributionText` is the approved project-owned presentation of the separately
registered publisher, title, version and source reference.
`trademarkTreatment=Required` requires the accompanying exact
non-endorsement text and does not grant or imply trademark permission.
`trademarkOrNonEndorsementText` and `changeMarkingText` preserve the exact
existing project-owned wording selected by the owner. `assessedAt` adopts the
UTC-normalised author and committer instant of the A0-003 documentary commit
as the immutable assessment anchor; `assessorId` identifies that governed
assessment by authority without introducing a personal identifier.

These values remain only a disposed input until a separately authorised
runtime operation creates and persistently reads back the corresponding
`DerivativeObligationSetV1`. This disposition does not calculate or anticipate
`rightsMappingRevision`, `obligationSetId`, `canonicalSha256`,
`renderManifestId`, generation-manifest identity or evidence bindings. It does
not authorise mutation of the ignored materialisation bundle or any product
operation.

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

Those broad terms were first mapped under `AUTH-S07-A-PRODUCT-A0-002`, whose
historical outcome remains preserved above. After the notice-bearing mechanism
was implemented and its Automatic Quality Gate was approved, A0-003 mapped the
same registered evidence without broadening it. The detailed current mapping
therefore makes the four visual operations conditionally `PERMITTED` only
through `pdf-page-png-notice-v1` and preserves the external
distribution/publication boundary as `DENIED`.

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
- A0-003 permits visual operations only through `pdf-page-png-notice-v1`, with
  the exact registered copyright/permission notice and disclaimers inside
  every composite PNG and in the governed accessible context. The embedded PDF
  notice alone is still not treated as satisfying a PNG copy or runtime
  display.
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
