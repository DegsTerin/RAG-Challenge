# ADR-0009 - Document, Evidence and Query Language Taxonomy

- Status: accepted
- Date: 2026-08-07
- Accepted: 2026-08-07
- Decision authority: explicit product-owner acceptance on baseline
  `main@89994e82d246b1cc0a240e99a2d09942e316f7cc`, corpus `4.9.4`, with
  the exact decision:
  `Confirmo a decisão proposta em ADR-0009 — Document, Evidence and Query Language Taxonomy e aceito a ADR exclusivamente como autoridade arquitetural.`
- Proposal authority: owner-authorised documentary preparation on baseline
  `main@8b4e98dc336b13183b936c5ac974968714e43765`, corpus `4.9.4`
- Owners: RAG-Challenge product, RAG architecture, evaluation and API
- State: `STATE-07 TESTING_HOMOLOGATION` accepted architecture decision
- Reconciliation dependency: semantic reconciliation remains separately
  authorised for this ADR and
  [ADR-0008](ADR-0008-Product-Corpus-Storage-And-Page-Image-Evidence.md)

## Purpose and authority

This decision separates the language of a question and answer from the
language declared by a source document and the language of cited evidence. It
resolves the architectural ambiguity exposed by the PostgreSQL 18 candidate,
whose PDF declares BCP 47 language tag `en`, while the current canonical
contracts restrict `contentLanguage` to `pt-BR` or `en-GB` and prohibit a
neutral, inferred or fallback value.

The owner explicitly accepted this ADR on 2026-08-07 exclusively as
architectural authority. Acceptance does not change the current language
contract, make `en` an active runtime value, reconcile ADR-0008, change
OpenAPI v1, activate the PostgreSQL document or authorise implementation,
dataset materialisation, evaluation, indexing, providers, browser, network or
any external action.

## Context

The current product has three different language concerns:

1. `questionLanguage` and `answerLanguage` form a closed product capability
   with exact values `pt-BR` and `en-GB`; an answer must use the declared
   question language.
2. `interfaceLanguage` is separately closed to `pt-BR` and `en-GB` and does
   not select the question, answer or evidence language.
3. `contentLanguage` describes source-derived evidence and is currently also
   closed to `pt-BR` and `en-GB`.

The third constraint is not compatible with every truthful source-language
record. The locally retained PostgreSQL 18.4 PDF declares `/Lang` as `en`.
That declaration is a valid, less specific BCP 47 tag. Rewriting it as
`en-GB` would add a regional claim that the publisher did not make. Treating
it as `en-GB` only for evaluation would also collapse distinct evidence
strata and overstate coverage.

ADR-0008 additionally requires a future `QueryResponseV2` and `CitationV2`
before page-image evidence is implemented. This creates one controlled future
contract boundary in which document-language semantics and page-image evidence
can converge without changing the implemented and strict OpenAPI v1 contract.

## Decision drivers

- Preserve the source's exact language evidence without invented specificity.
- Keep the accepted `pt-BR` and `en-GB` question/answer capability unchanged.
- Preserve source-derived citation text without translation.
- Prevent one language tag from satisfying a different evaluation stratum.
- Keep OpenAPI v1 and existing strict clients unchanged.
- Permit authorised documentation in other BCP 47 languages without requiring
  an ADR per language tag, while requiring explicit evaluation before support
  claims or activation.
- Keep language metadata as untrusted data rather than locale, path, provider
  or policy authority.

## Options considered

| Option | Benefits | Material limitations | Decision outcome |
|---|---|---|---|
| Keep document languages closed to `pt-BR` and `en-GB` | No change to current types, fixtures or OpenAPI v1 | Truthful documents declared as `en` or another valid tag remain ineligible; accepting them would require unsupported reclassification or exclusion | Rejected |
| Separate query/answer languages from broader BCP 47 document languages | Preserves exact source evidence, keeps the user-facing language contract closed and permits language-specific evaluation strata | Requires a future model, dataset and v2 contract migration before broader-language documents can become active | Selected |
| Map a less-specific or different tag to the nearest supported product language | Appears to preserve the current two-value model | Invents regional or script specificity, hides coverage gaps and makes citation/evaluation metadata misleading | Rejected |

## Decision

Adopt separate language domains. Their reconciliation and implementation
remain subject to later, explicit authorities.

### Query, answer and interface languages

- `SupportedQueryLanguage` remains a closed value with exact external tags
  `pt-BR` and `en-GB`.
- `questionLanguage` is required and must be one of those two values.
- `answerLanguage` must equal the accepted `questionLanguage`.
- `interfaceLanguage` remains a separate closed UI value with `pt-BR` and
  `en-GB`; it never changes document or citation language.

### Document and evidence languages

- `DocumentContentLanguage` is a distinct, open BCP 47 value domain; it is not
  the closed `SupportedQueryLanguage` enumeration.
- Every document version records a required canonical
  `DocumentContentLanguage` as `contentLanguage`, describing its
  source-derived content.
- When a publisher or embedded document property declares a language, the
  governed provenance record also preserves that exact observed tag as
  `sourceDeclaredLanguage` and records its evidence.
- A declared tag is never made more specific by inference. In particular,
  `en` is not converted to `en-GB`, `en-US` or another regional tag.
- When the source provides no usable declaration, a curator may assign
  `contentLanguage` only with recorded evidence, actor, method and review
  disposition. An ambiguous or mixed-language document remains `Candidate`
  until its language policy is resolved.
- A materially multilingual document records a primary content tag only when
  the evidence supports it. Per-unit language metadata or a new multilingual
  policy requires a later compatible contract decision; the parser must not
  guess silently.
- Compatible BCP 47 document tags are catalogue data, not code branches or
  provider selections. Their presence does not by itself establish retrieval,
  generation or UI support for that language.

For the PostgreSQL 18.4 candidate, both the preserved declared tag and the
current document content tag remain `en`. The document is not relabelled as
`en-GB`.

### Citation preservation

- A citation carries the exact governed `contentLanguage` of the cited
  document or evidence unit.
- Source-derived title, section, excerpt, page label and quoted text remain in
  that language and are not translated or rewritten by the language model.
- The generated answer may explain evidence in `questionLanguage`, but it must
  distinguish explanation from source-derived citation content.
- A language tag cannot select a filesystem path, resource, locale file,
  provider, prompt, model, source adapter or authorisation policy.

## Evaluation and dataset stratification

The accepted four-pair compatibility matrix remains mandatory:

| Question language | Evidence language | Required answer language |
|---|---|---|
| `pt-BR` | `pt-BR` | `pt-BR` |
| `en-GB` | `en-GB` | `en-GB` |
| `pt-BR` | `en-GB` | `pt-BR` |
| `en-GB` | `pt-BR` | `en-GB` |

That matrix proves the two supported question languages against the two
pre-registered evidence-language strata. It does not allow a document tagged
`en` to count as `en-GB` evidence.

Every additional document language in a scored product corpus creates its own
reported evidence-language stratum. For the PostgreSQL candidate, product
cases would therefore exercise at least `pt-BR -> en` and `en-GB -> en`, while
the required `en-GB` evidence rows must be supplied by an independently
authorised `en-GB` document or by clearly separated deterministic fixtures.
Synthetic fixtures never count as PostgreSQL, real-corpus or source coverage.

Dataset manifests record the exact BCP 47 tag for every document and case.
Results from distinct content-language tags are not silently merged. A support
claim names the exact query languages, evidence languages, documents, dataset
revision, provider and environment that were evaluated.

## HTTP and OpenAPI compatibility

OpenAPI v1 remains unchanged. Its current `QueryRequestV1`, `QueryResponseV1`
and `CitationV1` continue to accept and emit only their existing closed
language values. A document whose governed `contentLanguage` is outside that
contract cannot become active through the v1 query surface by inference or
silent coercion.

ADR-0009 and ADR-0008 are both accepted as architectural authority. If their
semantics are later reconciled under separate authority, the planned v2
contract should use:

```text
QueryRequestV2
  questionLanguage: pt-BR | en-GB

QueryResponseV2
  answerLanguage: pt-BR | en-GB
  citations: CitationV2[]

CitationV2
  contentLanguage: canonical BCP 47 tag
  sourceDeclaredLanguage?: exact observed BCP 47 tag
  pageImages: PageImageEvidenceV1[]
```

This is accepted architecture direction, not an OpenAPI artefact or
implemented schema.
The eventual v2 contract must be created, implemented and compatibility-tested
under a separate authority. V1 and V2 coexistence, deprecation and consumer
migration require explicit implementation and release evidence; neither ADR
silently removes v1.

## Security and validation boundaries

- Language metadata is untrusted input with bounded length and a strict BCP 47
  parser; arbitrary locale names, paths or Unicode free text are rejected.
- Canonical comparison is explicit and deterministic, while the original
  publisher-declared tag remains preserved in provenance evidence.
- A language tag never changes trust class, rights, activation, egress,
  provider, prompt or model authority.
- Parser or model language detection may be supporting evidence but is never
  the sole authority for a missing or conflicting publisher declaration.
- Logs use document IDs and canonical language tags, not full document text.
- Displaying a tag or source-derived label uses text-safe rendering and does
  not load a resource named by the tag.
- Translation or language normalisation does not create quotation,
  redistribution or derivative rights.

## Lifecycle and activation

- A document may remain a rights-eligible `Candidate` while language-contract,
  dataset, renderer or API preconditions are unresolved.
- Catalogue registration does not make a broader-language document queryable.
- Activation requires a runtime contract that can preserve the exact governed
  content language, an evaluation stratum for the intended query languages and
  all other document, rights, index and render conditions.
- Deactivation and removal preserve the historical language and provenance
  evidence associated with each immutable document version.

The PostgreSQL candidate therefore remains `ELIGIBLE_CANDIDATE`, not indexed,
active or included in a frozen dataset. Its `en` tag is preserved while the
current v1 contract remains unchanged.

## Compatibility and migration

Acceptance requires separately authorised reconciliation of ADR-0004,
ADR-0008, canonical contracts, the data dictionary, RAG module, security,
threat model, lifecycle, quality gates, S07-A planning and factual state.

A later implementation would:

1. split the current closed `SupportedLanguage` responsibility into a closed
   query/answer language type and a validated document-language value;
2. preserve existing `pt-BR` and `en-GB` document values without change;
3. add publisher-declared language evidence without rewriting existing source
   records;
4. version dataset and manifest schemas that acquire broader language tags;
5. introduce the broader citation language only through the separately
   implemented v2 HTTP contract; and
6. prove that v1 behaviour and its OpenAPI snapshot remain unchanged.

No data migration, contract edit or OpenAPI v2 file is authorised by this
decision.

## Consequences

- Source language is recorded truthfully without inventing region or script.
- The product retains its intentionally small query and answer language set.
- Product-corpus evaluation grows by exact document-language strata.
- A document in a new language can be catalogued as a candidate without an ADR
  per tag, but activation and support claims require evidence for the named
  language/provider/environment boundary.
- Strict v1 consumers do not receive a new enum value unexpectedly.
- V2 becomes the first possible public contract for both broader document
  language tags and ADR-0008 page-image evidence, reducing competing public
  version transitions.
- The PostgreSQL document cannot satisfy the `en-GB` evidence stratum merely
  because both tags belong to English.

## Rejected approaches

### Treat `en` as an alias for `en-GB`

Rejected because BCP 47 language tags carry different specificity and the
source did not declare the regional variant.

### Translate citation content into the answer language

Rejected because it would replace source evidence, weaken quotation fidelity
and obscure the language actually evaluated.

### Broaden `questionLanguage` whenever a document language is added

Rejected because corpus cardinality must not silently expand the supported
user-facing generation contract, provider authority or UI localisation.

### Add `en` directly to OpenAPI v1

Rejected because strict clients currently validate a closed enum and the
existing implementation has not been changed, tested or homologated for that
value.

## Decision and follow-on authority

The decision selects the separated taxonomy and rejects implicit language
mapping. The owner explicitly accepted ADR-0009 on 2026-08-07 exclusively as
architectural authority.

Acceptance establishes architecture authority only. It does not reconcile
ADR-0008 or other documents, change corpus version, modify v1, implement v2,
materialise a dataset, index or activate the PostgreSQL candidate, call a
provider, render page images or authorise an external action.

## Acceptance checks

- The PostgreSQL declared tag remains exactly `en` and is not counted as
  `en-GB` evidence.
- `questionLanguage` and `answerLanguage` remain closed to `pt-BR` and
  `en-GB`, with exact equality for completed answers.
- Interface language remains independent from query, answer and evidence.
- Every citation preserves the governed content-language tag and unmodified
  source-derived text.
- Dataset reports separate exact language strata and preserves the mandatory
  four-pair compatibility matrix.
- OpenAPI v1 remains byte-for-byte unchanged until a separately authorised
  implementation changes another versioned contract.
- No broader-language document becomes active before compatible contracts,
  evaluation and lifecycle preconditions are implemented and verified.
- Any semantic reconciliation must cite this recorded owner decision and
  remains subject to separate authority.
