# STATE-07 S07-A Evaluation and Security Proposal

## Purpose and authority

This document proposes the first bounded `STATE-07 TESTING_HOMOLOGATION`
batch, the roadmap-owned `S07-A — Evaluation and security`. It defines the
dataset boundary, document provenance and rights gate, pre-registered
acceptance thresholds, language matrix, candidate environments, checks,
evidence, negative scope, execution order and stop conditions.

The owner authorised only preparation of this proposal. This document does
not materialise or freeze a dataset, acquire or activate a document, execute
RAG evaluation, run a test or load campaign, inspect a browser, use a provider
or real source, access a network, mutate OCI or GitHub, publish, deploy, enter
`STATE-08`, conduct an Automatic Quality Gate or conduct a Human Gate.

The proposal is subordinate to
[ADR-0004](architecture/ADR-0004-MVP-Corpus-Official-Source-And-Evaluation.md)
and the accepted architecture. A later approval must name the exact execution
envelope. Approval of this proposal must not be interpreted as execution
authority. Internal lifecycle and agent-governance material is intentionally
not part of the public runtime repository.

## Planning baseline decision

On 2026-08-07, the owner confirmed this proposal exclusively as the `S07-A`
planning baseline on clean `main@183c8cd9fe303096a355ab731e72dc81748eb626`,
prompt corpus `4.9.3`.

The confirmation did not grant `AUTH-S07-A-DATASET-001`,
`AUTH-S07-A-RUN-001`, dataset materialisation, evaluation, tests, load,
dynamic security, browser, provider, real-source, network or external-action
authority. All proposed authority identifiers remain ungranted until the
owner supplies a later explicit and bounded approval.

On 2026-08-07, prompt corpus `4.9.5` reconciled the accepted ADR-0008 and
ADR-0009 architecture into this planning baseline. The reconciliation adds
preconditions for durable source/page-image content, rights, exact BCP 47
document languages and evaluation strata. It does not grant either authority,
materialise data, implement v2 or change the byte-for-byte OpenAPI v1 surface.

## Confirmed proposal baseline

The local read-only inspection on 2026-08-07 confirmed:

| Fact | Observed value |
| --- | --- |
| Repository location | `C:\Projects\RAG-Challenge` |
| Git top-level | `C:/Projects/RAG-Challenge` |
| Git directory | `.git` |
| Branch | `main` |
| `HEAD` | `4e18cbc161b85e0217d7d91db1978e313bfdca45` |
| Prompt corpus | `4.9.3` |
| Working tree before this proposal | clean |
| Lifecycle | `STATE-07 TESTING_HOMOLOGATION` active by documentary entry only |
| `S07-A` execution | not authorised and not executed |
| `STATE-07` Automatic Quality Gate | pending and not executed |
| `STATE-07` Human Gate | pending and not prepared |

Runtime preflight was `NOT_APPLICABLE` because this increment is documentation
and read-only analysis only. No process, listener, runtime, provider or
browser was inspected or stopped.

## Proposed outcome and gate boundary

`S07-A` should produce a reproducible, reviewable campaign that:

1. freezes the accepted `rag-eval-catalogue-v1` dataset and its exact
   catalogue/document manifest before any scored run;
2. proves the four `questionLanguage` to evidence-language pairs;
3. measures retrieval, citation, groundedness, insufficient-evidence,
   isolation, latency and cost against the thresholds accepted by ADR-0004;
4. exercises prompt injection, abuse, rate limiting, bounded failures, SSRF,
   DNS and HTTP controls, stale data, source leakage, crash boundaries,
   recovery, rollback and accessibility;
5. records the exact environment, providers, manifests, commands and
   sanitised evidence for every result; and
6. leaves no residual P0 or P1 finding before an Automatic Quality Gate may be
   proposed.

Completion of the batch would provide implementation evidence only. It would
not approve the `STATE-07` Automatic Quality Gate, prepare or decide the Human
Gate, close `STATE-07`, enter `STATE-08` or authorise publication or deploy.

## Preconditions and present blockers

No execution authority should be requested until all applicable preconditions
are resolved on a newly confirmed clean baseline:

- the owner has supplied or explicitly authorised every PDF/CSV document in
  the candidate product corpus;
- provenance, content language, immutable version, hash and intended-use
  rights are evidenced for every candidate document;
- every exact publisher-declared language is preserved; `contentLanguage` is a
  canonical BCP 47 tag and no `en` value is inferred as `en-GB`;
- source bytes reside in the durable content store with verified reopen; a PDF
  intended to provide visual evidence also has compatible rendering/derivative
  rights, a complete finalised render manifest and verified PNG objects;
- the implemented runtime/API can represent every activated document language
  and visual-evidence binding. Until a separately authorised v2 implementation
  exists, v1 remains closed to its existing values and cannot activate a
  broader-language document by coercion;
- every database claimed as active has at least one active validated document;
- the dataset manifest and case inventory have been reviewed but not exposed
  to a scored result;
- the named homologation environment and its resource limits are recorded;
- each provider, account, secret reference, network destination and spend
  ceiling needed by the selected campaign is separately authorised;
- the exact test commands, task-owned stores, evidence paths and cleanup
  boundaries are known; and
- no unresolved dependency, contract, OpenAPI, schema, migration or ADR
  change is required by the campaign.

At proposal time, no real product corpus, frozen evaluation dataset, real
provider campaign or real-source campaign is evidenced. These are blockers to
product-level homologation, not reasons to substitute synthetic fixtures or
weaken an accepted threshold.

## Dataset definition

### Stable identity and freeze rule

The scored dataset must use the ADR-accepted ID `rag-eval-catalogue-v1`. A
campaign-specific immutable revision must add a version and manifest digest,
for example `<dataset-version>@<sha256>`, without changing the stable dataset
ID. The exact revision, catalogue revision, active generation and document
binding set must be frozen before the first scored command.

Changing a question, expected fact, relevant location, document binding,
language annotation, scoring rule or threshold after a result has been
observed creates a new dataset revision and a new campaign. A failed revision
must remain preserved as historical evidence.

### Dataset layers

The dataset must keep two evidence layers separate:

| Layer | Purpose | Permitted claim |
| --- | --- | --- |
| Scored product-corpus cases | Evaluate the exact approved active documents and generation. | Product-corpus results only for the named manifest, environment and providers. |
| Deterministic synthetic fixtures | Prove contracts, failure handling, security cases and missing language combinations. | Contract/integration coverage only; never product-source, provider-quality or real-corpus coverage. |

Synthetic fixtures must not be counted in product-corpus retrieval,
groundedness, latency or source-coverage denominators. Evaluation questions,
expected answers and attack instructions must remain outside the runtime
corpus and production artefact.

### Required case inventory

The dataset has no fixed total case count. It must grow with every active
database, document, format and source. For each active database, include:

- answerable cases;
- insufficient-evidence cases;
- citation-boundary and exact-location cases;
- prohibited-extrapolation cases;
- prompt-injection and provenance-confusion cases; and
- adversarial filter cases where an ineligible database, document, generation
  or corpus would otherwise rank above eligible evidence.

PDF and CSV require format-specific location cases whenever each format is
present. Every reportable database/source stratum must expose its own
denominator and metric result. A stratum with inadequate cases is reported as
underpowered and cannot be silently merged or omitted to obtain a pass.

### Required case metadata

Every case must have a stable case ID and record:

- dataset revision and case classification;
- database ID and catalogue revision;
- document ID, immutable version, content hash and format;
- canonical BCP 47 `contentLanguage`, exact `sourceDeclaredLanguage` when
  observed and the evidence/method for any curator-assigned language;
- source adapter and `LocalAuthorised` or `OfficialExternal` trust class;
- declared `questionLanguage`;
- one or more relevant document locations;
- required facts and prohibited extrapolations;
- expected provenance and citation identity;
- applicable database/document/generation/corpus filters;
- expected answerable or insufficient-evidence outcome;
- for an eligible visually evidenced PDF, render-manifest ID and exact cited
  page-image identities without binary content; and
- reviewer identity by stable role, review date and disposition without a
  workstation or device name.

The frozen manifest must contain sorted case and document entries, hashes,
counts by stratum and its own SHA-256 digest.

## Provenance and rights gate

Only owner-authored, owner-authorised or verified official external documents
may enter the scored product-corpus layer. Each document must have an evidence
record containing:

| Evidence | Required disposition before inclusion |
| --- | --- |
| Identity | Stable database/document IDs, immutable version, hash and format. |
| Provenance | Original owner/publisher, acquisition channel, trust class and source adapter. |
| Rights | Primary evidence for parsing, indexing, source-byte retention, quotation, citation and the intended distribution/publication boundary; for visual PDF evidence, page rendering, derivative-image creation/retention and runtime display are separately disposed. |
| Attribution | Required notices, licence link, change notice and preserved copyright text. |
| Language | Exact BCP 47 content language used by the evaluation matrix. |
| Approval | Owner decision, date, scope and any restriction or expiry. |

An unknown, ambiguous, expired or incompatible right is a hard exclusion. A
software licence, repository licence, catalogue entry or prior ADR does not
license a document by inference. `reference-materials/` remains local-only,
ignored and ineligible as product corpus. The dataset must contain no personal,
customer, confidential or secret material.

Official-source evidence may be collected only under a later authority that
names the exact allowlisted HTTPS URL and security boundary. This proposal
does not refresh the historical PostgreSQL evidence, download a snapshot or
authorise any other source.

## Pre-registered acceptance thresholds

The following thresholds are copied unchanged from accepted ADR-0004. They
are not new decisions and must not be relaxed after observing a result:

| Measure | Threshold |
|---|---:|
| Recall@5 for answerable cases | at least `0.90` overall and `0.85` for every reportable database/source stratum |
| Mean reciprocal rank at 5 | at least `0.75` for every reportable database/source stratum |
| Citation identity and location validity | `1.00` |
| Answer language equals declared question language | `1.00` |
| Source-derived citation text preserved in its original language | `1.00` |
| Supported factual claims | at least `0.95` |
| Correct insufficient-evidence outcome | at least `0.95` |
| Unsupported high-impact factual claims | `0` |
| Cross-database filter, cross-generation or cross-corpus leakage | `0` |
| Incorrect provenance or silent substitution of a degraded source | `0` |
| Successful instruction override from retrieved content | `0` |
| Stale, withdrawn or deactivated source calls to embedding/LLM | `0` |
| Query p95 on the named homologation environment | at most `12 s` |
| Query p99 on the named homologation environment | at most `20 s` |
| Evaluation campaign provider spend | at most `USD 20` |

In addition, the roadmap acceptance criterion requires all prior thresholds
and no residual P0 or P1 finding. A threshold amendment is a separate formal
decision made before a new campaign begins, with its reason and new baseline
recorded. A model judge may supplement evidence but cannot be the sole gate
authority.

## Language matrix

The scored product cases and clearly separated deterministic fixtures must
together cover all four compatibility pairs:

| Question language | Evidence language | Required answer language | Citation text |
| --- | --- | --- | --- |
| `pt-BR` | `pt-BR` | `pt-BR` | Preserve original `pt-BR`. |
| `en-GB` | `en-GB` | `en-GB` | Preserve original `en-GB`. |
| `pt-BR` | `en-GB` | `pt-BR` | Preserve original `en-GB`. |
| `en-GB` | `pt-BR` | `en-GB` | Preserve original `pt-BR`. |

Every case declares exactly one `questionLanguage`. The Dashboard language is
independent and must not change answer-language scoring. If the approved real
corpus lacks one evidence language, authorised synthetic fixtures may prove
the missing contract direction, but the report must disclose the real-corpus
coverage gap.

This fixed matrix remains mandatory but covers only the `pt-BR` and `en-GB`
evidence strata. Every other exact `DocumentContentLanguage` in the product
corpus creates two additional reported strata, one for each supported question
language. Distinct tags are not merged. For the PostgreSQL candidate tagged
`en`, the product campaign must report at least `pt-BR -> en` and
`en-GB -> en`; neither row counts as `en-GB` evidence, so an independently
authorised `en-GB` document or clearly separated fixture must still supply the
mandatory `en-GB` evidence rows.

## Environment and provider matrix

The campaign report must identify each environment by a stable placeholder,
never by a real workstation or device name:

| Candidate lane | Intended evidence | Current status and authority boundary |
| --- | --- | --- |
| `ENV-S07-A-LOCAL-01` | Local, offline, Release-mode campaign using loopback-only fakes, task-owned stores and deterministic providers/fixtures. | Candidate only. Requires separate execution authority; cannot establish real provider, real source, Linux or OCI behaviour. |
| `ENV-S07-A-PROVIDER-01` | Quality, latency and cost for one named provider/model/configuration against the frozen product dataset. | Unselected and unauthorised. Requires provider, account, secret-reference, egress and spend authority. |
| `ENV-S07-A-SOURCE-01` | Exact-source security and freshness checks for one approved official source. | Unselected and unauthorised. Requires the exact URL and source-specific egress authority. |
| `ENV-S07-A-BROWSER-01` | Accessibility and representative user-flow samples in the named browser/version. | Unselected and unauthorised. Requires separate browser execution authority. |

Each executed environment record must include the commit, prompt corpus,
dataset and document-manifest digests, index generation, provider/model and
non-secret configuration, OS/runtime/architecture, CPU and memory capacity,
Release artefact identity, task-owned store, loopback ports, network policy,
start/end UTC instants and command list. Hostnames, secrets and ambient account
identifiers are prohibited.

Results from different environments, providers, models, dataset revisions or
generations must not be merged into one passing denominator. `ENV-S07-A-LOCAL-01`
is a control boundary; it cannot substitute for any real boundary required by
a later product claim.

## Proposed verification matrix

### Dataset and retrieval

- validate manifest schema, stable IDs, sorted entries, hashes and counts;
- validate exact document/source-declared language tags and per-language
  strata without coercion or silent aggregation;
- when visual evidence is in scope, validate the source content object,
  `pdf-page-png-v1` profile, complete canonical render manifest, every PNG
  hash/signature/dimension/readback and retention reachability;
- prove that every active database/document/format/source is represented;
- prove the evaluation dataset is absent from the runtime corpus and artefact;
- compute Recall@5 and MRR@5 overall and per reportable stratum; and
- prove hard pre-filtering before top-k for database, document, generation and
  corpus isolation.

### Answer, citation and provenance

- score required facts, prohibited extrapolations and insufficient evidence;
- validate citation document identity, version and exact location;
- validate each page-image reference against the same active document version,
  cited page, generation and finalised render manifest;
- execute all four language pairs and preserve citation text in its original
  language without translation;
- reject degraded-source substitution and disclose coverage/freshness; and
- use a documented two-person rubric for the human answer-quality sample; and
- prove that OpenAPI v1 is byte-for-byte unchanged and that broader language or
  image fields exist only in a separately implemented/versioned v2 boundary.

### Security and abuse

- test retrieved prompt injection, instruction override, malicious document
  text, source leakage, abuse and rate limiting;
- test SSRF, DNS rebinding, mixed DNS answers, IP/Host/SNI pinning, redirects,
  URL/path rules, media type, compressed/decompressed limits and refusal of
  authentication or ambient credentials;
- prove absence of lateral AIA/CRL/OCSP egress under the selected TLS policy;
- test bounded renderer failure, malicious PDF page structures, partial
  manifests, guessed/cross-generation image IDs, deactivated/removed denial,
  PNG-only same-origin serving, immutable ETag, `nosniff` and cache policy; and
- verify that stale, withdrawn, deactivated or mismatched sources do not call
  embedding or language-model providers.

Real DNS, TLS or official-source evidence is excluded unless its exact target
and egress are separately authorised. Loopback/fake evidence must be labelled
as synthetic.

### Load, recovery and accessibility

- record warm-up, concurrency, duration, request mix, resource limits and raw
  sample count before measuring p95/p99;
- test bounded provider/source failures, cancellation, rate limits and
  continued service;
- test crash boundaries for observation append, digest, audit, activation and
  rollback by a new `CorpusActivationRecord`, without replaying historical
  freshness;
- prove restart and recovery against the intended eligible generation;
- repeat the supported `pt-BR`/`en-GB`, `Light`/`Dark`, keyboard, focus,
  contrast, reflow, loading, empty and error-state accessibility matrix; and
- when visual evidence is implemented, prove adjacent source-language textual
  evidence, keyboard/focus/reflow behaviour and absence of image-only factual
  or navigation meaning.

The exact executable commands must be frozen in the future authority after
the dataset and harness paths exist. Missing or ambiguous commands are a stop
condition; this proposal does not invent passing executions.

## Evidence and reporting contract

A future execution should create one tracked factual report,
`docs/STATE-07-Testing-Homologation-Report.md`, and keep bulky raw output under
a validated ignored task-owned path such as
`artifacts-local/state-07/s07-a/<campaign-id>/`. The report must record:

- authority ID, baseline, date, scope and negative scope;
- dataset, catalogue, document and generation identities and digests;
- exact content/source-declared language strata and, when applicable, render
  profile, manifest and cited page-image digests;
- provenance/rights disposition and excluded documents without document
  content;
- environment/provider matrix and exact sanitised commands;
- per-check start/end time, exit code, denominator and result;
- threshold table with observed values and explicit pass/fail/not-run/blocked
  status;
- findings with stable IDs, severity and factual disposition;
- residual risks, limitations, cost and cleanup outcome; and
- hashes for retained sanitised evidence required to reproduce a conclusion.

Logs and committed evidence must not contain full documents, evaluation
questions/answers when restricted, prompts, generated answers, secrets,
tokens, account identifiers or real host/device names. Use stable IDs, hashes,
counts, timings and sanitised error codes. Raw evidence is non-authoritative
until its relevant facts are represented in the tracked report.

## Proposed execution order and authority envelopes

The batch should remain sequential because dataset freeze, environment
identity and thresholds are shared inputs.

1. **A0 — Reconfirm authority and baseline.** Confirm clean `main`, expected
   commit, corpus, documents, rights and execution envelope.
2. **A1 — Materialise and review the dataset.** Under a separately approved
   `AUTH-S07-A-DATASET-001`, create the case inventory and manifests without a
   scored run, provider, real-source access or threshold change.
3. **A2 — Freeze the campaign.** Record dataset/document digests, environment,
   provider/model configuration, commands and thresholds before observing a
   scored result.
4. **A3 — Execute one authorised boundary.** Under
   `AUTH-S07-A-RUN-001`, run only the named local, provider, source and browser
   lanes. Any external lane requires its own explicit sub-authority.
5. **A4 — Reconcile evidence.** Produce the factual report and findings
   without correcting them silently or changing thresholds.
6. **A5 — Verify the increment.** Run repository hygiene, applicable existing
   checks, report consistency and complete diff review; create focused local
   commits if the authorised changes can be isolated.
7. **Stop.** A complete Automatic Quality Gate requires a new authority after
   `S07-A` evidence is committed on a clean baseline.

`AUTH-S07-A-DATASET-001` and `AUTH-S07-A-RUN-001` are proposed identifiers,
not granted authorities. Any implementation, test, dependency, lockfile,
contract, OpenAPI, schema, migration, ADR, network, provider, browser or
external-action permission must be named explicitly in the later envelope.

## Negative scope

This proposal and its preparation exclude:

- dataset materialisation, evaluation, test, load or dynamic-security
  execution;
- browser use, provider/model calls, real source access and any network;
- dependency installation or changes to source, tests, manifests, lockfiles,
  contracts, OpenAPI, schema, migrations or ADRs;
- account, secret, paid-service, global-cache or workstation mutation;
- OCI, GitHub, publication, push, deploy and `STATE-08`;
- Automatic Quality Gate, Human Gate or lifecycle closure; and
- claims of product-corpus, provider, performance, security, accessibility,
  Linux, OCI or production homologation.

## Stop conditions

Stop before writing or continuing a future execution if:

- branch, commit, corpus, working tree or explicit authority diverges;
- concurrent or unrelated work appears;
- any document lacks stable identity, hash, provenance, language or compatible
  intended-use rights;
- the active corpus and frozen dataset manifest do not agree;
- a case, scoring rule or threshold changes after a result is observed;
- an unapproved provider, account, secret, URL, redirect, DNS address, network
  path, cost or browser becomes necessary;
- an unexpected dependency, lockfile, public contract, OpenAPI, schema,
  migration or ADR change is required;
- an evaluation input appears in the runtime corpus or product artefact;
- an environment, provider, model, dataset or generation drifts during the
  campaign;
- evidence is incomplete, unsanitised, irreproducible or would require
  overclaiming a synthetic boundary;
- a P0/P1 or blocking vulnerability is found; or
- cleanup cannot be restricted to validated task-owned ignored paths.

No finding may be corrected during an audit or campaign unless a separate
corrective authority is granted after the finding is recorded.

## Rollback and completion of this proposal

The proposal creates documentation only. Its rollback is a focused local
revert of this document and its index entry; there is no runtime, dataset,
provider, network or external state to roll back.

This proposal is complete when its documentary checks pass and it is recorded
in one focused local commit. That completion does not change the factual
status of `S07-A`: execution remains unauthorised and unexecuted, and
`STATE-07` remains active only within the documentary boundary already
recorded.
