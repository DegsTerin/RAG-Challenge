# STATE-02 Architecture Report

## Purpose and scope

This report records the factual local and documentary execution of
`STATE-02 ARCHITECTURE` begun on 2026-07-31. It distinguishes prepared
proposals from accepted decisions, verified facts and blocked external
evidence.

The authorised work is sequential `S02-A` and `S02-B`. A later authority
permitted bounded read-only HTTPS verification against named official hosts.
The scope still excludes functional RAG code, migrations, `STATE-03`,
installation, paid provider use, secrets, external mutation, publication,
deployment, CD and DB-Notifier changes.

## Baseline and authority

| Item | Observed value |
|---|---|
| Entry date | `2026-07-31` |
| Entry baseline | `47435930727fc344298d84658b1ad9b2da9b5b62` |
| Entry registration commit | `e9175b193b98bd0d8f464be7ed129da5af2de6aa` |
| Architecture decision-package commit | `979677fa1f4d7324340b8be15d88eb8b5b802a1a` |
| External-verification baseline | `8ba91889c0517d78747ae2980fb766c36268edf6` |
| External-verification completion baseline | `f1066c3509f5f48d4fe6e21c9e36403e642c1431` |
| Direct-URL verification resumption baseline | `e80f8c41bea3f28deff3d8cdccafccbca5dcc016` |
| Branch | `main` |
| Instruction corpus | `4.1.0` |
| Entry working tree | Clean |
| Runtime preflight | `NÃO APLICÁVEL` |

The owner authorised entry and local/documentary execution with this exact
statement:

```text
Autorizo a entrada no STATE-02 ARCHITECTURE e a execução documental e local, de forma sequencial, dos lotes S02-A e S02-B, exclusivamente nos limites, entregáveis, verificações, riscos, rollback e escopo negativo apresentados nesta conversa.
```

The entry was recorded append-only before architecture artefacts were
created. The entry does not accept an ADR or authorise an external action.

The owner later authorised read-only HTTPS verification on 2026-07-31 for a
closed list of PostgreSQL, NuGet, two GitHub repositories, OpenAI, Oracle and
Creative Commons hosts. Redirect, authentication, another host, unverifiable
price or unexpected authority was an explicit stop condition. Installation,
full package/model/corpus download, paid API calls, credentials, external
mutation and resource creation remained prohibited.

The owner then authorised `developers.openai.com` and resumption of the
remaining allowlisted checks, with the same negative scope. A new host,
repository, redirect, authentication or unexpected authority remained a
mandatory stop condition.

On 2026-08-01, the owner separately fixed the product query-language boundary
to `pt-BR` and `en-GB`: answers use the declared question language,
source-derived citation text remains in its original language and tests cover
same-language and both cross-language directions. This documentary decision
does not select the Dashboard language, accept an ADR or authorise executable
work.

The owner then independently selected `pt-BR` and `en-GB` as the supported
Dashboard languages. The interface must provide an explicit choice, localise
project-owned visual text in the selected language and remain independent
from `questionLanguage`; source-derived citations stay untranslated. Initial
selection, persistence and fallback were not decided. This documentary
decision does not accept an ADR or authorise frontend implementation.

The owner subsequently selected `Light` and `Dark` as the closed set of
Dashboard themes. Theme choice must remain independent from interface and
query languages, preserve content and query context, and meet contrast, focus,
state and non-colour-only accessibility requirements. Initial theme, system
preference, persistence and fallback were not decided. This documentary
decision does not accept an ADR or authorise frontend implementation.

The owner also removed the proposed product ceilings of twelve database
systems and 120 PDF pages. Each corpus version remains finite and records its
observed counts, but catalogue coverage and page count have no fixed product
maximum. Runtime safety and capacity controls remain conditional on the actual
corpus and deployment environment. This independent constraint does not accept
ADR-0004 or authorise corpus creation.

The owner subsequently supplied and confirmed the exact initial canonical
catalogue: 51 unique database products, 9 categories and 54 many-to-many
assignments. Redis, SAP HANA and SingleStore remain single entities assigned to
two categories each. Compatible database products and any number of associated
documents are administrator-managed records rather than hard-coded branches.

The owner also confirmed that every active database must have at least one
active PDF and/or CSV document and may have any number of additional documents.
All active/current documents participate in unified retrieval. Local or
official origin remains explicit provenance and trust metadata rather than a
mutually exclusive query corpus. New records start `Candidate`; deactivation
preserves history, removal is a logical tombstone, physical deletion follows
retention, and the last active document can leave only with explicit atomic
database deactivation. This documentary authority does not accept an ADR or
authorise implementation, network, download, provider or lifecycle progress.

The final resumption instruction required direct HTTPS requests to exact URLs
on the existing allowlist and prohibited general web search and automatic link
following. Textual references to other hosts were not access. A performed
request redirecting to an unauthorised host, requiring authentication or
revealing unexpected authority remained a mandatory stop condition. The owner
limited reconciliation to proposed ADRs and this report.

## Evidence inspected

- The accepted .NET/React bootstrap and dependency boundaries.
- Current requirements, architecture, RAG lifecycle, security, gates,
  roadmap, state/history and ADR pack.
- Current central package manifest and project package references.
- Local NuGet cache presence for EF Core SQLite `10.0.9`; cache presence was
  treated only as local availability evidence, not package approval.
- Git branch, commit, status, diff hygiene and repository audit.
- Official PostgreSQL documentation index, candidate PDF response and licence.
- Official CC BY 4.0 deed and Portuguese legal code.
- Official NuGet package metadata and the allowlisted PdfPig source repository,
  licence and releases.
- Public PdfPig security/advisory pages and current .NET compatibility
  metadata.
- Direct official OpenAI model, embedding, Responses, pricing, data-control,
  quota and .NET SDK documentation.
- Direct official OCI region, shape, Always Free, price-list, service-limit,
  block-storage, backup, Secret Management and API-index documentation.
- PostgreSQL `robots.txt` and a direct local offline-chain TLS assessment.
- Current read-only NuGet registry version, registration and catalogue
  metadata for PdfPig, without downloading a package.

No provider endpoint, OCI service, account, credential or paid API was
accessed. `api.openai.com` and the documented OCI service endpoints were not
contacted. No package, corpus, model or complete PDF was downloaded, and no
external state was changed.

## Bounded external verification record

| Area | Observed result | Reconciliation status |
|---|---|---|
| PostgreSQL candidate PDF | Exact `HEAD` returned `200`, no `Location`, `application/pdf`, 15,771,040 bytes, ETag `"6a05c867-f0a5a0"`, last modified `2026-05-14T13:04:39Z` and byte-range support. Exact `GET bytes=0-65535` returned `206`, 65,536 bytes, range total 15,771,040 and `%PDF-1.4`. | Verified for immediate response, media, size, validators, range and signature; no body persisted. |
| PostgreSQL source rights | The [documentation index](https://www.postgresql.org/docs/) identifies version 18 as current. The [PostgreSQL Licence](https://www.postgresql.org/about/licence/) covers use, copy, modification and distribution of software and documentation with required notices. The [official policies index](https://www.postgresql.org/about/policies/) lists no separate general terms-of-use policy. | Published use-right basis verified; no conflicting general terms policy was found. |
| PostgreSQL robots/rate | [`robots.txt`](https://www.postgresql.org/robots.txt) returned `200` without redirect and does not disallow `/files/documentation/`. Targeted official search found no publisher download-rate guidance. | No policy conflict found; the proposed daily ceiling is a conservative project limit, not a publisher commitment. |
| PostgreSQL TLS | Three DNS answers were public; direct TLS negotiated TLS 1.3 with `TLS_AES_256_GCM_SHA384`. A four-element chain validated locally with certificate downloads disabled and revocation `NoCheck`; the leaf was valid from `2026-06-02T17:09:47Z` to `2026-08-31T17:09:46Z`, SHA-256 `2738805715f7b32e7850b33dc2319e9d7b39d9acfb44936d56a1d18a6ef805ac`. | Current local no-lateral-download validation verified; offline-revocation, future rotation and clean OCI reproduction remain explicit residual/later-test concerns. |
| Proposed owner-authored-document licence | The [CC BY 4.0 deed](https://creativecommons.org/licenses/by/4.0/) and [Portuguese legal code](https://creativecommons.org/licenses/by/4.0/legalcode.pt) confirm sharing/adaptation rights and attribution, licence-link, change-marking and no-additional-restriction duties. | Licence terms verified; no owner-authored product document currently exists and the owner has not granted the licence on one. |
| PdfPig | The live [NuGet version index](https://api.nuget.org/v3-flatcontainer/pdfpig/index.json), [0.1.15 registration](https://api.nuget.org/v3/registration5-semver1/pdfpig/0.1.15.json), catalogue, [package page](https://www.nuget.org/packages/PdfPig/0.1.15), [release](https://github.com/UglyToad/PdfPig/releases/tag/v0.1.15) and [security page](https://github.com/UglyToad/PdfPig/security) identify 0.1.15 as newest stable, listed, Apache-2.0, computed compatible with `net10.0` and tied to commit `f131f642976936e06ee91cb19d3ed728f9dd18b6`. GitHub reports no security policy and no published advisory; the release adds nesting-depth enforcement support. | Public package/security metadata verified. Empty advisory/catalogue vulnerability fields do not prove absence of vulnerability; no version was selected or installed, and extraction/runtime evidence still requires a separately authorised spike. |
| OpenAI models/contracts | The current [embedding model](https://developers.openai.com/api/docs/models/text-embedding-3-small), [embedding guide](https://developers.openai.com/api/docs/guides/embeddings), [GPT-4.1 mini model](https://developers.openai.com/api/docs/models/gpt-4.1-mini) and API references verify the proposed IDs, 1,536 embedding dimensions, `POST /v1/embeddings`, `POST /v1/responses`, Structured Outputs, model limits and current public quota tables. The embedding model exposes only a mutable alias; `gpt-4.1-mini-2025-04-14` remains the default and only listed snapshot. | Public contract facts verified. Current prices are USD 0.02 per million embedding input tokens and USD 0.40/0.10/1.60 per million GPT input/cached/output tokens. Actual project tier, spend limit and account availability remain unverified without login. |
| OpenAI data/SDK | The [data-control guide](https://developers.openai.com/api/docs/guides/your-data) states no training unless opt-in, default abuse monitoring for up to 30 days, no embedding application state and at least 30 days of Responses application state by default or with `store=true`. ZDR/MAM and non-US residency require approval; Brazil is not listed. The [official .NET repository](https://github.com/openai/openai-dotnet), releases and NuGet metadata identify stable `OpenAI` 2.12.0, MIT, targeting `net10.0`, with embedding and Responses clients. | Public policy and SDK metadata verified. The proposal now requires Responses `store=false`, no provider state/tools and explicit owner acceptance of default retention/disclosure and no Brazilian residency. No SDK was installed and no provider endpoint was contacted. |
| OCI region/compute | The [regions table](https://docs.oracle.com/en-us/iaas/Content/General/Concepts/regions.htm) verifies `sa-saopaulo-1`, realm `OC1`, with one availability domain. The [shape reference](https://docs.oracle.com/en-us/iaas/Content/Compute/References/computeshapes.htm) verifies the ARM64 A1 shape and candidate 1-OCPU/6-GiB configuration. The public default is 16 A1 OCPUs/96 GiB per availability domain for Pay As You Go or Trial. | Region and configuration verified; future tenancy capacity is not public and requires authenticated `ListShapes`/provisioning. Always Free documentation warns about host-capacity exhaustion. |
| OCI price/capacity | The [Always Free page](https://docs.oracle.com/en-us/iaas/Content/FreeTier/freetier_topic-Always_Free_Resources.htm) states 1,500 A1 OCPU-hours and 9,000 GB-hours, equivalent to 2 OCPUs/12 GiB. The [live price-list JSON](https://www.oracle.com/a/ocom/docs/pricing/cloud-price-list.json), build 350 dated 2026-07-16, instead prices zero through 3,000/18,000 and then USD 0.01/OCPU-hour plus USD 0.0015/GB-hour. | Primary sources conflict on the free allowance. The candidate fits the lower figure, but zero-cost entitlement and billing cannot be claimed before tenancy verification. |
| OCI storage/Vault/endpoints | Block Volume documents persistence, encryption, one-AD access, redundant copies and a 99.99% annual-durability objective while recommending regular backups. Backups are encrypted and regionally stored in Object Storage. Public prices are USD 0.0255/GB-month capacity plus USD 0.0017/VPU-GB-month; Secret Management and software-protected keys are free. The [API index](https://docs.oracle.com/en-us/iaas/api/specs/index.json) publishes exact Sao Paulo Core, KMS, Secret Management and Secret Retrieval endpoints; retrieval uses `GET /20190301/secretbundles/{secretId}`. | Public storage, secret and endpoint facts verified without contacting an OCI service. The conditional 50-GiB target, regional daily and pre-change backup, 14-day retention, 24-hour RPO, eight-hour restore objective and read-only instance-principal policy are proposed; tenancy limits, IAM enforcement, application consistency and restore remain untested. |

The initial HTTPS request used normal operating-system certificate validation.
The second round added the separate offline-chain assessment recorded above;
application integration and clean OCI reproduction remain untested. No
certificate, PDF body or external response was saved in the repository.

The earlier second round stopped when a PdfPig security search returned
unrelated external results. The final resumption replaced search with direct
exact-URL retrieval and clarified that unrequested textual references do not
constitute access. That round completed without an unauthorised-host redirect,
authentication challenge or unexpected authority. Allowed-host Oracle
redirects were not followed automatically; each exact target was inspected
and requested separately.

## S02-A — Blocking decisions

### Prepared decision package

| Area | Proposed selection | Decision/evidence state |
|---|---|---|
| RAG lifecycle and source separation | One logical corpus, data-driven catalogue, unified active-document retrieval and ordered activation bindings | ADR-0002 remains `proposed`; owner cardinality/lifecycle constraints are reconciled but explicit ADR decision is required. |
| Catalogue/documents | Initial 51 unique products, 9 categories, 54 assignments; any number of PDF/CSV documents per database | Owner-decided contract; no product document is acquired, validated, indexed or active, and rights remain per-document evidence. |
| Official sources | Any number of compatible exact allowlisted registrations; PostgreSQL 18 A4 PDF is the first verified candidate | `proposed`; only PostgreSQL URL/media/size/redirect/licence/robots/local TLS facts are verified. No other source or egress is authorised. |
| Parsers | PdfPig PDF candidate plus a separate CSV adapter | `proposed`; PdfPig 0.1.15 public facts are recorded, CSV package/version is unselected, and both require executable quality/security evidence. |
| Normalisation | Unicode NFC and deterministic whitespace/control policy | `proposed`; locally specified. |
| Chunking | `paragraph-window-v1`, target 3,200 scalars, overlap 480, hard max 4,000 | `proposed`; must pass evaluation. |
| Embeddings | OpenAI `text-embedding-3-small`, 1,536 dimensions, cosine | `proposed`; current availability, dimensions, endpoint, price and public quotas verified. No immutable dated snapshot exists; actual tier, owner acceptance of retention/disclosure/residency and `pt-BR`/`en-GB` retrieval quality remain pending. |
| Language model | OpenAI `gpt-4.1-mini-2025-04-14` | `proposed`; snapshot, Responses/Structured Outputs, price, public quotas and data policy verified. The owner must decide default provider retention/disclosure and absence of Brazilian residency; answer-language compliance remains untested. |
| Catalogue/control persistence | EF Core SQLite | `proposed`; exact packages remain unapproved. |
| Raw content | Durable content-addressed filesystem | `proposed`; contract specified. |
| Vector store | Local `SqliteExactVectorStore`, hard SQL pre-filter and exact cosine ranking | `proposed`; 10,000 chunks is an initial benchmark point, not a product limit. Representative catalogue performance remains untested. |
| OCI | Conditional ARM64 OCI Compute target in `sa-saopaulo-1`, 1 OCPU/6 GiB, initial 50 GiB volume | `proposed`; public facts verified, while tenancy capacity/entitlement/IAM/cost and application-consistent daily/pre-change backup with 14-day retention, RPO 24 h and restore objective 8 h remain untested. |
| Evaluation | Extensible `rag-eval-catalogue-v1` stratified by active database/document/source/format and pre-registered thresholds | `proposed`; dataset not authored or run. No fixed total case count limits catalogue growth. |

### Alternatives retained

- local embedding/LLM if provider data terms or cost are rejected;
- PostgreSQL/pgvector if the exact SQLite adapter fails the performance gate;
- another compatible exact PDF/CSV registration through the same governed
  controls; a new integration class may require an amended ADR;
- another OCI shape/region when verified capacity or budget requires it.

None is an active fallback. Switching requires a decision and a new
compatibility/evaluation baseline.

### S02-A status

`PREPARADO PARA DECISÕES HUMANAS`: the architecture recommendation,
alternatives, consequences, owners and all public primary-source evidence in
the authorised scope are documented. Account-specific entitlement, capacity
and controls are explicitly not verifiable without prohibited login or
resource access. Completion requires explicit human decisions for ADR-0002,
ADR-0004 and ADR-0005, followed by reconciliation of the chosen baseline.

## S02-B — Contracts and security

### Prepared contracts

- database/category/document identity, lifecycle and provenance contracts;
- source, parser, chunker, embedding, vector, language-model and persistence
  ports;
- complete ordered-binding `CorpusActivationRecord` compare-and-swap semantics;
- query request/response and citation v1;
- explicit `questionLanguage`, `answerLanguage` and citation
  `contentLanguage` semantics for `pt-BR` and `en-GB`;
- canonical `ApplicationFailure → CH_* → HTTP/Problem Details` table;
- readiness and coverage semantics that keep remaining active documents
  serviceable during per-source degradation;
- one-shot catalogue/document/source administration commands, inputs,
  invariants, idempotency and exit categories;
- OpenAPI v1 compatibility rules;
- required architecture, contract, adversarial, crash and negative tests.

### Prepared security design

- four independent deny-by-default egress profiles;
- exact URI per source registration, DNS-set rejection, approved-IP connection
  and Host/SNI preservation;
- redirects, proxy, ambient credentials and certificate downloads disabled;
- local-only trust with an explicit unverified revocation residual risk;
- bounded public query, rate, concurrency, deadline, retrieval and evidence;
- local OS-identity administration with enable flag, reason, lease,
  compare-and-swap and sanitised audit;
- 36 registered threats and 15 security test groups;
- explicit risk-acceptance boundaries for data disclosure, TLS, licence,
  provider, OCI and P0/P1 findings.

### S02-B status

`PREPARADO PARA DECISÃO`, not accepted: ADR-0006, the canonical contracts and
threat model are complete as proposals. Exact AI paths and Sao Paulo OCI
service endpoints are now documented; residual-risk acceptance still depends
on ADR-0004/ADR-0005 and explicit owner decisions. The separately decided
query-language constraint is already binding on whichever proposal baseline
is later accepted; it does not accept the remaining contents of ADR-0004 or
ADR-0006.

The provider-disclosure threats remain blocked by human acceptance or selection
of an alternative, not by absence of public documentation. No provider call or
account verification occurred during this reconciliation.

## Deliverable map

| Lifecycle deliverable | Artefact | Status |
|---|---|---|
| ADRs accepted or rejected | ADR-0002 and ADR-0004 to ADR-0006 | Proposed; human decisions pending. |
| Canonical contracts and diagrams | `STATE-02-Canonical-Contracts.md`; data-flow diagrams in threat model | Prepared. |
| Detailed threat model | `security/STATE-02-Threat-Model.md` | Prepared. |
| Parser, embedding, vector and LLM selection | ADR-0005 | Proposed; public parser, provider, model, data-control, price, quota and SDK facts verified. Runtime spikes and human decisions remain pending. |
| Catalogue, documents and licences | ADR-0004 | Initial 51/54/9 and PDF/CSV lifecycle owner-decided; documents and per-document rights not materialised. |
| Official source records, URLs, terms, licences, maxAge and limits | ADR-0004 | PostgreSQL candidate facts verified; every additional registration requires its own evidence and activation. |
| Durable content/catalogue/index persistence | ADR-0005 | Proposed. |
| Four egress policies | ADR-0006 | Prepared; per-source exact URI, AI methods and candidate OCI regional endpoints documented. Profiles remain disabled and untested. |
| Vector search, failures, readiness and OpenAPI | Contract document and ADR-0006 | Prepared. |
| `pt-BR`/`en-GB` question, answer, evidence and citation semantics | Requirements, contract document, ADR-0004 and ADR-0006 | Owner decided; documented for later implementation and homologation, not yet tested at runtime. |
| `pt-BR`/`en-GB` Dashboard localisation | Language Policy, requirements, lifecycle and roadmap | Owner decided supported set and independent explicit choice; initial selection, persistence, fallback, implementation and runtime tests remain pending. |
| `Light`/`Dark` Dashboard themes | Requirements, architecture, lifecycle and roadmap | Owner decided supported set and independent explicit choice; initial theme, system preference, persistence, fallback, implementation and runtime tests remain pending. |
| SSRF and DNS/IP pinning | ADR-0006 and threat model | Prepared; not implemented/tested. |
| Evaluation, OCI and rollback | ADR-0004, ADR-0005 and contracts | Proposed; public facts verified. Extensible dataset, account capacity/entitlement, IAM, backup consistency and restore evidence remain pending. |

## Blockers and required authority

### Remaining account and executable boundary

No public primary-source verification item remains outstanding within the
authorised S02-A/S02-B scope. The following facts cannot be established by
anonymous documentation access and are intentionally not represented as
verified:

- the future OpenAI project's usage tier, spend limit, model entitlement,
  ZDR/MAM eligibility or contractual residency setting;
- future `VM.Standard.A1.Flex` host capacity, tenancy entitlement, effective
  service limits, invoice currency/tax and the free allowance applied to that
  tenancy;
- runtime IAM for Secret Retrieval, created vault/secret state and actual
  endpoint reachability from the deployment network;
- PdfPig extraction quality/security under adversarial documents, provider
  response behaviour, SQLite vector performance, backup restore and clean OCI
  compatibility.

Those items require later, separately authorised account access, spikes or
implementation tests. They do not justify inventing an external fact or
assuming zero cost in this architecture decision.

### Human decision blocker

The owner must now explicitly accept, reject or request changes to each ADR.
A `STATE-02` Human Gate does not accept them by implication.

The current decision packet requires these explicit outcomes:

- ADR-0002: accept, reject or change the unified active-document lifecycle,
  ordered activation bindings, provider boundaries and RAG-Challenge-owned API
  contract.
- ADR-0004: accept, reject or change the catalogue/document/licence proposal,
  first PostgreSQL 18 source, per-source manual frequency and
  offline-revocation residual risks, extensible evaluation dataset and
  thresholds. The exact 51/54/9 catalogue, PDF/CSV cardinality and absence of
  fixed ceilings are already independent owner constraints.
- ADR-0005: accept, reject or change conditional PdfPig 0.1.15 and the
  unselected CSV parser package subject to later spikes;
  `paragraph-window-v1`; the mutable `text-embedding-3-small` alias; OpenAI
  `gpt-4.1-mini-2025-04-14`; default provider retention/disclosure and absence
  of Brazilian residency; compliance with the owner-decided `pt-BR`/`en-GB`
  retrieval and answer contract; OpenAI SDK 2.12.0 candidate;
  SQLite/filesystem/exact vector persistence; and the conditional Sao Paulo A1
  deployment subject to capacity, conflicting free-allowance evidence, 50 GiB
  initial volume, application-consistent regional backups, 14-day retention,
  RPO 24 h, restore objective 8 h and read-only instance-principal Secret
  Management design.
- ADR-0006: accept, reject or change the four deny-by-default egress profiles,
  exact AI methods, registered source allowlists, documented OCI endpoints,
  runtime-only secret-retrieval boundary, offline certificate-revocation risk,
  catalogue administration, coverage/readiness and HTTP/OpenAPI v1 policy.

No one decision implies another, installs a candidate, enables egress,
creates a resource or authorises `STATE-03`.

## Risks and residual decisions

- Each official-source candidate carries its own licence, publisher-frequency,
  freshness, availability and TLS risk. Only PostgreSQL has the existing public
  evidence record.
- External AI may be rejected because default abuse-monitoring retention is up
  to 30 days, Brazil has no documented residency option, account eligibility
  and limits are unknown, or the owner rejects the cost/disclosure model.
- `text-embedding-3-small` has no immutable dated snapshot; undetected alias
  drift would undermine generation reproducibility.
- Local-only certificate revocation checking has a residual risk requiring
  explicit acceptance.
- Exact SQLite vector search may miss latency/memory thresholds at catalogue
  scale; 10,000 chunks is a benchmark point rather than a product ceiling.
- ARM64 OCI capacity or package compatibility may require the documented
  alternative shape. Sao Paulo has one availability domain and public
  Always Free sources conflict on the allowance.
- A same-volume copy does not protect the catalogue/content/index from
  availability-domain loss. The 99.99% block-volume durability objective and
  Oracle's backup recommendation require an explicit independent regional
  backup choice and tested restore.
- Obtaining, licensing, validating and maintaining at least one PDF/CSV
  document for each of the 51 initial databases is the first material
  product-data dependency.
- Open cardinality makes final catalogue scale a capacity, cost, evaluation and
  schedule risk. Capacity failure must block activation rather than truncate
  the canonical catalogue.
- A poisoned source registration, stale vectors after deactivation or a race
  that leaves an active database without evidence would violate the new
  lifecycle and requires dedicated contract/security tests.
- Evaluation thresholds are pre-registered but untested; failure must drive a
  design change, not post-hoc threshold relaxation.

## Rollback

- State-entry history remains append-only.
- Proposed artefacts can be changed or reverted by focused commits before
  acceptance.
- An accepted ADR is replaced only by a later ADR; history is not rewritten.
- No package, provider, data migration, snapshot, OCI resource or external
  system exists from this execution, so there is no operational rollback.
- Disposable future spikes must use isolated paths/data and be removed or
  retained as explicitly labelled evidence without entering product runtime.

## Verification performed

| Check | Result |
|---|---|
| Baseline branch/commit/status before entry | Passed. |
| Append-only entry record and factual-state update | Passed and committed. |
| Repository format/local links/ignored materials/secret-pattern audit after entry | Passed for 77 non-ignored files. |
| `git diff --check` after entry | Passed. |
| Combined STATE-02 document audit | Passed for 83 non-ignored files and 30 Markdown files; six new artefacts, 30 unique threat IDs and 12 unique security-test groups. |
| New-document H1, fence, unresolved-marker and absolute-user-path checks | Passed. |
| ADR status consistency | ADR-0002 and ADR-0004 to ADR-0006 each have exactly one `proposed` status. |
| PostgreSQL exact PDF `HEAD` and 64 KiB range | Passed within the authorised byte limit; no redirect or persistence. |
| PostgreSQL and CC BY 4.0 licence review | Passed for the stated use-right and attribution basis; the future corpus grant remains an owner action. |
| PostgreSQL robots and local TLS review | Passed without redirect: exact path not disallowed; no rate guidance located; TLS 1.3 and a four-element offline chain validated with downloads disabled and revocation `NoCheck`. |
| PdfPig identity/release/licence/security review | Stable candidate 0.1.15, published 2026-06-25, listed, Apache-2.0 and computed `net10.0` compatibility verified. GitHub exposes neither `SECURITY.md` nor a published advisory; this negative public evidence is not proof of safety. |
| OpenAI model/contract/price/quota review | Proposed models, 1,536 dimensions, exact API methods, Structured Outputs, current public prices and quota schedules verified from direct developer documentation. Actual account tier/entitlement remains unverified. |
| OpenAI data/SDK review | Default no-training, retention, ZDR/MAM, residency boundaries and official .NET SDK 2.12.0 metadata verified. Brazil is not listed; no SDK install or provider call occurred. |
| OCI region/shape/capacity/price/limit review | Region, one-AD topology, valid A1 configuration, public capacity warning, service limits and live price data verified. Conflicting free-allowance sources and tenancy-specific capacity/entitlement are explicitly unresolved. |
| OCI storage/Vault/endpoint review | Block durability/encryption/backup, public prices/limits, Secret Management and exact Sao Paulo service endpoints verified from documentation only; no OCI endpoint was contacted. |
| Prohibited external actions | None performed: no login, credential, paid API, complete artefact download, mutation or resource creation. |
| Post-reconciliation repository audit | Passed for 83 non-ignored files and 30 Markdown files; 30 unique threat IDs, 12 security-test groups, four proposed ADR statuses and clean diff-format checks. |
| Second-round reconciliation audit | Passed for the same 83-file/30-Markdown baseline, 30 threat IDs, 12 security-test groups, four unchanged `proposed` ADR statuses and clean structural/diff checks. |
| Final direct-URL reconciliation audit | Passed for 83 non-ignored files; only ADR-0005, ADR-0006 and this report changed, all with LF. Repository audit, `git diff --check`, four `proposed` ADR statuses, 30 threat IDs and 12 security-test groups passed. |
| Executable spike, build or runtime test | Not run; documentary scope and no implementation change. |
| ADR decisions | Pending human decisions. |
| Query-language requirement and contract consistency | Documented for `pt-BR` and `en-GB`, with answer-language equality, original-language citations and a four-pair question/evidence test matrix; executable behaviour not run. |
| Dashboard-language requirement and separation | Documented for `pt-BR` and `en-GB`, with an explicit independent selection and a four-pair UI/query-language test matrix; frontend behaviour not implemented or run. |
| Dashboard-theme requirement and separation | Documented for `Light` and `Dark`, with explicit independent selection and the four-pair UI/query-language matrix executed in both themes; frontend behaviour not implemented or run. |
| Corpus-scale constraint | Fixed ceilings removed; exact 51 names, 9 categories and 54 assignments reconciled twice with expected order and duplicates. No corpus or runtime capacity test was performed. |
| Corpus `4.7.0` catalogue/document reconciliation | Directed local validation passed for exactly 22 authorised files: 51 unique names/54 assignments/9 categories in vision and ADR-0004; 25 RF, 18 RNF, 20 acceptance criteria, 19 Must backlog IDs, 36 threat IDs and 15 security-test groups; four ADRs remain `proposed`; H1/fences/local links, LF/final newline, table structure and `git diff --check` passed. This was not the combined `STATE-02` audit. |

## Current gate assessment

Automatic Quality Gate for `STATE-02`: `BLOQUEADO`. Public external facts in
the authorised scope are reconciled, but ADR-0002 and ADR-0004 to ADR-0006
remain `proposed`. The owner must decide each ADR, and the combined documents
must then be audited against the resulting accepted/rejected baseline.

Human Gate for `STATE-02`: `PENDENTE` and must not be requested while the
Automatic Quality Gate is blocked.

`STATE-02 ARCHITECTURE` remains active. `STATE-03 DATA_AND_INDEX_MODELING` is
not authorised.
