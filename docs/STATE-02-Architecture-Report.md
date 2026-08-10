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
| Architecture acceptance baseline | `39e2f803bf73cb4e2b59e56a0596e2858a3aed51` |
| Combined-audit result commit | `9707b87d75a6acb14c8993ff0283a4221bc6c762` |
| Corrective proposal baseline | `9707b87d75a6acb14c8993ff0283a4221bc6c762` |
| ADR-0007 decision baseline | `664187c6926be5ce4bef3734603f8d936626d535` |
| ADR-0007 semantic-reconciliation baseline | `9aa90c012e3bc973330f5a79678fc358c81809df` |
| Renewed combined-audit baseline | `3978a17201cf5f6ac4ddc189862736fc3646457b` |
| ADR-0013 decision baseline | `f03162bad0fc166a597739b22e55fbc46ec59535` |
| ADR-0013 semantic-reconciliation baseline | `a08aa83c7319b97ead6c91a92ae8cbb4da5c28cc` |
| External-verification baseline | `8ba91889c0517d78747ae2980fb766c36268edf6` |
| External-verification completion baseline | `f1066c3509f5f48d4fe6e21c9e36403e642c1431` |
| Direct-URL verification resumption baseline | `e80f8c41bea3f28deff3d8cdccafccbca5dcc016` |
| Branch | `main` |
| Entry instruction corpus | `4.1.0` |
| Corrective proposal corpus | `4.8.1` |
| ADR-0007 decision corpus | `4.9.0` |
| ADR-0007 semantic-reconciliation corpus | `4.9.1` |
| ADR-0013 decision corpus | `4.10.17` |
| ADR-0013 semantic-reconciliation corpus | `4.10.18` |
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

On 2026-08-01, against
`main@39e2f803bf73cb4e2b59e56a0596e2858a3aed51`, corpus `4.7.0`, the owner
then explicitly and independently accepted ADR-0002, ADR-0004, ADR-0005 and
ADR-0006. ADR-0005 acceptance expressly preserves conditional OCI, package
versions and operational targets; application-consistent backup; read-only
instance-principal authentication; bounded OpenAI disclosure; and mutable
embedding-alias controls. No decision implies another or authorises
implementation, audit, Human Gate, `STATE-03` or an external action.

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
| OpenAI models/contracts | The [embedding model](https://developers.openai.com/api/docs/models/text-embedding-3-small), [embedding guide](https://developers.openai.com/api/docs/guides/embeddings), [GPT-4.1 mini model](https://developers.openai.com/api/docs/models/gpt-4.1-mini) and API references observed on 2026-07-31 verified the original proposed IDs, 1,536 embedding dimensions, `POST /v1/embeddings`, `POST /v1/responses`, Structured Outputs, model limits and then-current public quota tables. The embedding model exposed only a mutable alias; `gpt-4.1-mini-2025-04-14` was the default and only listed snapshot in that historical observation. | Historical public contract facts verified. Accepted ADR-0013 separately records the 2026-08-10 public-documentation review and superseding `gpt-5.4-mini-2026-03-17` selection. The historical prices were USD 0.02 per million embedding input tokens and USD 0.40/0.10/1.60 per million GPT input/cached/output tokens. Actual project tier, spend limit, account availability and runtime compatibility remain unverified without login or a provider call. |
| OpenAI data/SDK | The [data-control guide](https://developers.openai.com/api/docs/guides/your-data) states no training unless opt-in, default abuse monitoring for up to 30 days, no embedding application state and at least 30 days of Responses application state by default or with `store=true`. ZDR/MAM and non-US residency require approval; Brazil is not listed. The [official .NET repository](https://github.com/openai/openai-dotnet), releases and NuGet metadata identify stable `OpenAI` 2.12.0, MIT, targeting `net10.0`, with embedding and Responses clients. | Public policy and SDK metadata verified. The accepted decision requires Responses `store=false`, no provider state/tools and records acceptance of default retention/disclosure and no verified Brazilian residency. No SDK was installed and no provider endpoint was contacted. |
| OCI region/compute | The [regions table](https://docs.oracle.com/en-us/iaas/Content/General/Concepts/regions.htm) verifies `sa-saopaulo-1`, realm `OC1`, with one availability domain. The [shape reference](https://docs.oracle.com/en-us/iaas/Content/Compute/References/computeshapes.htm) verifies the ARM64 A1 shape and candidate 1-OCPU/6-GiB configuration. The public default is 16 A1 OCPUs/96 GiB per availability domain for Pay As You Go or Trial. | Region and configuration verified; future tenancy capacity is not public and requires authenticated `ListShapes`/provisioning. Always Free documentation warns about host-capacity exhaustion. |
| OCI price/capacity | The [Always Free page](https://docs.oracle.com/en-us/iaas/Content/FreeTier/freetier_topic-Always_Free_Resources.htm) states 1,500 A1 OCPU-hours and 9,000 GB-hours, equivalent to 2 OCPUs/12 GiB. The [live price-list JSON](https://www.oracle.com/a/ocom/docs/pricing/cloud-price-list.json), build 350 dated 2026-07-16, instead prices zero through 3,000/18,000 and then USD 0.01/OCPU-hour plus USD 0.0015/GB-hour. | Primary sources conflict on the free allowance. The candidate fits the lower figure, but zero-cost entitlement and billing cannot be claimed before tenancy verification. |
| OCI storage/Vault/endpoints | Block Volume documents persistence, encryption, one-AD access, redundant copies and a 99.99% annual-durability objective while recommending regular backups. Backups are encrypted and regionally stored in Object Storage. Public prices are USD 0.0255/GB-month capacity plus USD 0.0017/VPU-GB-month; Secret Management and software-protected keys are free. The [API index](https://docs.oracle.com/en-us/iaas/api/specs/index.json) publishes exact Sao Paulo Core, KMS, Secret Management and Secret Retrieval endpoints; retrieval uses `GET /20190301/secretbundles/{secretId}`. | Public storage, secret and endpoint facts verified without contacting an OCI service. The 50-GiB target, regional daily and pre-change backup, 14-day retention, 24-hour RPO, eight-hour restore objective and read-only instance-principal policy are accepted conditionally; tenancy limits, IAM enforcement, application consistency and restore remain untested. |

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

## S02-A — Architecture decisions

### Accepted decision package

| Area | Accepted selection | Decision/evidence state |
|---|---|---|
| RAG lifecycle and source separation | One logical corpus, data-driven catalogue, unified active-document retrieval and ordered activation bindings | ADR-0002 `accepted` as amended by accepted ADR-0007; implementation and executable evidence remain absent. |
| Catalogue/documents | Initial 51 unique products, 9 categories, 54 assignments; any number of PDF/CSV documents per database | Owner-decided contract; no product document is acquired, validated, indexed or active, and rights remain per-document evidence. |
| Official sources | Any number of compatible exact allowlisted registrations; PostgreSQL 18 A4 PDF is the first verified candidate | ADR-0004 `accepted`; only PostgreSQL URL/media/size/redirect/licence/robots/local TLS facts are verified. No other source or egress is authorised. |
| Parsers | Conditional PdfPig PDF candidate plus a separate CSV adapter | ADR-0005 `accepted`; PdfPig 0.1.15 public facts are recorded, CSV package/version is unselected, and both require executable quality/security evidence. |
| Normalisation | Unicode NFC and deterministic whitespace/control policy | ADR-0005 `accepted`; not implemented. |
| Chunking | `paragraph-window-v1`, target 3,200 scalars, overlap 480, hard max 4,000 | ADR-0005 `accepted`; must pass evaluation. |
| Embeddings | OpenAI `text-embedding-3-small`, 1,536 dimensions, cosine | ADR-0005 `accepted` with mutable-alias and disclosure controls; actual tier and `pt-BR`/`en-GB` retrieval quality remain unverified. |
| Language model | OpenAI `gpt-5.4-mini-2026-03-17` | ADR-0013 `accepted`, superseding only ADR-0005's earlier language-model candidate; local adapter contract compatibility passed deterministic fake-handler tests in `b6d6f9102ecf0ea93309f8080acebad02cf16584`, while account availability, real-provider behaviour, bilingual quality and latency remain untested. Default retention/disclosure and no verified Brazilian residency remain accepted. |
| Catalogue/control persistence | EF Core SQLite | ADR-0005 `accepted`; exact packages remain conditional. |
| Raw content | Durable content-addressed filesystem | ADR-0005 `accepted`; contract specified, not implemented. |
| Vector store | Local `SqliteExactVectorStore`, hard SQL pre-filter and exact cosine ranking | ADR-0005 `accepted`; 10,000 chunks is an initial benchmark point, not a product limit. Representative catalogue performance remains untested. |
| OCI | Conditional ARM64 OCI Compute target in `sa-saopaulo-1`, 1 OCPU/6 GiB, initial 50 GiB volume | ADR-0005 `accepted` conditionally; tenancy capacity/entitlement/IAM/cost and application-consistent daily/pre-change backup with 14-day retention, RPO 24 h and restore objective 8 h remain untested. |
| Evaluation | Extensible `rag-eval-catalogue-v1` stratified by active database/document/source/format and pre-registered thresholds | ADR-0004 `accepted`; dataset not authored or run. No fixed total case count limits catalogue growth. |

### Alternatives retained

- local embedding/LLM if provider data terms or cost are rejected;
- PostgreSQL/pgvector if the exact SQLite adapter fails the performance gate;
- another compatible exact PDF/CSV registration through the same governed
  controls; a new integration class may require an amended ADR;
- another OCI shape/region when verified capacity or budget requires it.

None is an active fallback. Switching requires a decision and a new
compatibility/evaluation baseline.

### S02-A status

`DECISÕES HUMANAS REGISTRADAS`: ADR-0002, ADR-0004 and ADR-0005 are accepted
independently. Account-specific entitlement, capacity and controls remain
explicitly unverified without separate authority; acceptance neither resolves
those facts nor authorises executable work.

## S02-B — Contracts and security

### Prepared contracts

- database/category/document identity, lifecycle and provenance contracts;
- source, parser, chunker, embedding, vector, language-model and persistence
  ports;
- separate generation-bound and complete activation-binding digests, with
  complete ordered-binding `CorpusActivationRecord` compare-and-swap semantics;
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

`DECISÃO HUMANA REGISTRADA`: ADR-0006 is accepted independently; the canonical
contracts and threat model form the accepted architecture baseline. Exact AI
paths and Sao Paulo OCI service endpoints remain documented but disabled.
Accepted residual-risk boundaries do not prove controls, enable egress or
authorise implementation.

The provider-disclosure risk boundary is now accepted, but provider use remains
blocked by disabled egress, absent account/runtime evidence and separate
execution authority. No provider call or account verification occurred during
this reconciliation.

## Deliverable map

| Lifecycle deliverable | Artefact | Status |
|---|---|---|
| ADRs accepted or rejected | ADR-0002 and ADR-0004 to ADR-0006 | All four accepted explicitly and independently. |
| Canonical contracts and diagrams | `STATE-02-Canonical-Contracts.md`; data-flow diagrams in threat model | Accepted architecture baseline; not implemented. |
| Detailed threat model | `security/STATE-02-Threat-Model.md` | Accepted architecture baseline; controls untested. |
| Parser, embedding, vector and LLM selection | ADR-0005 | Accepted conditionally; public facts verified, while exact packages, runtime spikes and account evidence remain pending. |
| Catalogue, documents and licences | ADR-0004 | Initial 51/54/9 and PDF/CSV lifecycle owner-decided; documents and per-document rights not materialised. |
| Official source records, URLs, terms, licences, maxAge and limits | ADR-0004 | PostgreSQL candidate facts verified; every additional registration requires its own evidence and activation. |
| Durable content/catalogue/index persistence | ADR-0005 | Accepted; not implemented or restore-tested. |
| Four egress policies | ADR-0006 | Accepted; per-source exact URI, AI methods and candidate OCI regional endpoints documented. Profiles remain disabled and untested. |
| Vector search, failures, readiness and OpenAPI | Contract document and ADR-0006 | Accepted; not implemented. |
| `pt-BR`/`en-GB` question, answer, evidence and citation semantics | Requirements, contract document, ADR-0004 and ADR-0006 | Owner decided; documented for later implementation and homologation, not yet tested at runtime. |
| `pt-BR`/`en-GB` Dashboard localisation | Language Policy, requirements, lifecycle and roadmap | Owner decided supported set and independent explicit choice; initial selection, persistence, fallback, implementation and runtime tests remain pending. |
| `Light`/`Dark` Dashboard themes | Requirements, architecture, lifecycle and roadmap | Owner decided supported set and independent explicit choice; initial theme, system preference, persistence, fallback, implementation and runtime tests remain pending. |
| SSRF and DNS/IP pinning | ADR-0006 and threat model | Accepted; not implemented/tested. |
| Evaluation, OCI and rollback | ADR-0004, ADR-0005 and contracts | Accepted conditionally; extensible dataset, account capacity/entitlement, IAM, backup consistency and restore evidence remain pending. |

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

### Human decision outcome

The owner explicitly and independently accepted all four ADRs against the
reconciled `4.7.0` baseline:

- ADR-0002: `accepted` with its unified active-document lifecycle, ordered
  activation bindings, provider boundaries and RAG-Challenge-owned API
  contract.
- ADR-0004: `accepted` with catalogue/document/licence governance, the first
  PostgreSQL 18 source candidate, per-source manual controls, residual TLS
  risk and the extensible evaluation baseline.
- ADR-0005: `accepted` with conditional PdfPig and CSV package versions,
  `paragraph-window-v1`, mutable-alias controls for
  `text-embedding-3-small`, OpenAI disclosure boundaries,
  `gpt-4.1-mini-2025-04-14`, SQLite/filesystem/exact-vector persistence and
  the conditional Sao Paulo A1/backup/Secret Management direction.
- ADR-0006: `accepted` with four deny-by-default egress profiles, exact AI
  methods, per-source allowlists, documented OCI endpoints, read-only runtime
  secret retrieval, residual TLS risk, local administration and HTTP/OpenAPI
  v1 policy.

The ADR-0005 bullet above remains the historical 2026-08-01 decision record.
Accepted ADR-0013 later supersedes only its `gpt-4.1-mini-2025-04-14`
language-model candidate with `gpt-5.4-mini-2026-03-17`; every other ADR-0005
decision remains unchanged. `gpt-5.6-sol` is an inactive future evaluation
candidate, not a fallback or runtime switch target. This reconciliation does
not change code, configuration, OpenAPI, provider state or lifecycle.

No one decision implies another, installs a candidate, enables egress, creates
a resource, executes the combined audit, requests the Human Gate or authorises
`STATE-03`.

## Risks and residual evidence

- Each official-source candidate carries its own licence, publisher-frequency,
  freshness, availability and TLS risk. Only PostgreSQL has the existing public
  evidence record.
- External AI may require the documented fallback if future account terms,
  eligibility, limits or effective cost violate the accepted disclosure and
  spend boundaries.
- `text-embedding-3-small` has no immutable dated snapshot; undetected alias
  drift would undermine generation reproducibility.
- Local-only certificate revocation checking retains the residual risk
  included in ADR-0006 acceptance.
- Exact SQLite vector search may miss latency/memory thresholds at catalogue
  scale; 10,000 chunks is a benchmark point rather than a product ceiling.
- ARM64 OCI capacity or package compatibility may require the documented
  alternative shape. Sao Paulo has one availability domain and public
  Always Free sources conflict on the allowance.
- A same-volume copy does not protect the catalogue/content/index from
  availability-domain loss. The accepted conditional regional-backup direction
  still requires application-consistency and restore evidence.
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
- The accepted architecture changes only through a later ADR; corrective
  commits may repair its documentary record without erasing decision history.
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
| Initial ADR status consistency | ADR-0002 and ADR-0004 to ADR-0006 each had exactly one `proposed` status before human decisions. |
| PostgreSQL exact PDF `HEAD` and 64 KiB range | Passed within the authorised byte limit; no redirect or persistence. |
| PostgreSQL and CC BY 4.0 licence review | Passed for the stated use-right and attribution basis; the future corpus grant remains an owner action. |
| PostgreSQL robots and local TLS review | Passed without redirect: exact path not disallowed; no rate guidance located; TLS 1.3 and a four-element offline chain validated with downloads disabled and revocation `NoCheck`. |
| PdfPig identity/release/licence/security review | Stable candidate 0.1.15, published 2026-06-25, listed, Apache-2.0 and computed `net10.0` compatibility verified. GitHub exposes neither `SECURITY.md` nor a published advisory; this negative public evidence is not proof of safety. |
| OpenAI model/contract/price/quota review | Selected model candidates, 1,536 dimensions, exact API methods, Structured Outputs, current public prices and quota schedules verified from direct developer documentation. Actual account tier/entitlement remains unverified. |
| OpenAI data/SDK review | Default no-training, retention, ZDR/MAM, residency boundaries and official .NET SDK 2.12.0 metadata verified. Brazil is not listed; no SDK install or provider call occurred. |
| OCI region/shape/capacity/price/limit review | Region, one-AD topology, valid A1 configuration, public capacity warning, service limits and live price data verified. Conflicting free-allowance sources and tenancy-specific capacity/entitlement are explicitly unresolved. |
| OCI storage/Vault/endpoint review | Block durability/encryption/backup, public prices/limits, Secret Management and exact Sao Paulo service endpoints verified from documentation only; no OCI endpoint was contacted. |
| Prohibited external actions | None performed: no login, credential, paid API, complete artefact download, mutation or resource creation. |
| Post-reconciliation repository audit | Passed for 83 non-ignored files and 30 Markdown files; 30 unique threat IDs, 12 security-test groups, four proposed ADR statuses and clean diff-format checks. |
| Second-round reconciliation audit | Passed for the same 83-file/30-Markdown baseline, 30 threat IDs, 12 security-test groups, four unchanged `proposed` ADR statuses and clean structural/diff checks. |
| Final direct-URL reconciliation audit | Passed for 83 non-ignored files; only ADR-0005, ADR-0006 and this report changed, all with LF. Repository audit, `git diff --check`, four `proposed` ADR statuses, 30 threat IDs and 12 security-test groups passed. |
| Executable spike, build or runtime test | Not run; documentary scope and no implementation change. |
| ADR decisions | ADR-0002 and ADR-0004 to ADR-0006 accepted explicitly and independently on the reconciled `4.7.0` baseline. |
| Query-language requirement and contract consistency | Documented for `pt-BR` and `en-GB`, with answer-language equality, original-language citations and a four-pair question/evidence test matrix; executable behaviour not run. |
| Dashboard-language requirement and separation | Documented for `pt-BR` and `en-GB`, with an explicit independent selection and a four-pair UI/query-language test matrix; frontend behaviour not implemented or run. |
| Dashboard-theme requirement and separation | Documented for `Light` and `Dark`, with explicit independent selection and the four-pair UI/query-language matrix executed in both themes; frontend behaviour not implemented or run. |
| Corpus-scale constraint | Fixed ceilings removed; exact 51 names, 9 categories and 54 assignments reconciled twice with expected order and duplicates. No corpus or runtime capacity test was performed. |
| Corpus `4.7.0` catalogue/document reconciliation | Directed local validation passed for exactly 22 authorised files: 51 unique names/54 assignments/9 categories in vision and ADR-0004; 25 RF, 18 RNF, 20 acceptance criteria, 19 Must backlog IDs, 36 threat IDs and 15 security-test groups; four ADRs remain `proposed`; H1/fences/local links, LF/final newline, table structure and `git diff --check` passed. This was not the combined `STATE-02` audit. |
| Corpus `4.8.0` ADR decision registration | Directed local validation passed for exactly 13 changed files: all four ADRs have one `accepted` status, acceptance date and decision authority; 51/54/9 catalogue and requirement/threat counts remain intact; H1/fences/tables/local links, UTF-8/LF and `git diff --check` passed. This was not the combined `STATE-02` audit. |

## Combined accepted-baseline audit

### Authority, baseline and method

The owner separately authorised the combined local documentary audit and its
factual registration, with no network, provider, account, GitHub, OCI,
publication, deployment, DB-Notifier change, Human Gate request or `STATE-03`
authority.

The audit started from clean `main` at
`a01a765d177efb6c4013c6846c5f54c8adbe7e0f`, instruction corpus `4.8.0`.
The first recorded command instant was `2026-08-02T01:00:11.7558597Z`, which
was 2026-08-01 in the owner timezone. PowerShell `7.6.4`, Git
`2.55.0.windows.3` and ripgrep `15.2.0` performed the local checks from the
repository root.

The audit read the complete governing, lifecycle, state/history, accepted-ADR,
contract, threat, report, requirement, RAG, security, roadmap, language and
corpus-version documents. It then traced the accepted semantics rather than
relying only on keyword or count agreement. Runtime preflight remained
`NÃO APLICÁVEL` because no executable behaviour was changed or validated.

### Results by gate

| Gate area | Result | Observed evidence |
|---|---|---|
| Baseline and authority | `APROVADO` | Branch, commit, corpus and clean working tree matched the authorised baseline before inspection and again before recording. |
| Repository structure and hygiene | `APROVADO` | `eng/check-repository.ps1` passed for 83 non-ignored files; 30 Markdown files each had one H1 and balanced fences; 131 local Markdown links resolved; no unresolved marker was found; `git diff --check` passed. |
| ADR decision state | `APROVADO` | ADR-0002 and ADR-0004 to ADR-0006 each contain one `accepted` status, one 2026-08-01 acceptance field and one decision-authority field. |
| Catalogue, requirements and test traceability | `APROVADO` | The vision and ADR-0004 contain identical ordered catalogues with 51 unique products, 54 assignments and 9 categories; only Redis, SAP HANA and SingleStore occur twice. Counts are 25 RF, 18 RNF, 20 acceptance criteria, 19 Must backlog items, 36 unique threats and 15 unique security-test groups. |
| PDF/CSV, provider, persistence and OCI qualification | `APROVADO` for documentary qualification | PDF/CSV adapters, conditional package selection, bounded OpenAI disclosure, local exact-vector persistence and conditional OCI targets remain explicitly distinguished from installation, account, runtime, capacity, IAM, cost and restore evidence. |
| Activation, freshness and generation identity | `REPROVADO` | `AQG-S02-001` leaves the accepted activation and manifest semantics internally contradictory. |
| Threat and risk-decision status | `REPROVADO` | `AQG-S02-002` leaves three risk-register statuses inconsistent with the accepted ADR decisions. |
| Routed foundation-document currency | `REPROVADO` | `AQG-S02-003` leaves low-severity proposal/status drift in documents routed as current high-level context. |
| Executable, provider, account and remote checks | `NÃO APLICÁVEL` | The authorised audit was local and documentary. No build, parser spike, provider call, account access, source fetch, OCI action or external mutation was performed. |

### Classified findings

#### `AQG-S02-001` — `P1 Alta` — observation identity contradicts freshness-only rebinding

[Accepted ADR-0002](architecture/ADR-0002-RAG-Lifecycle-Providers-And-Source-Separation.md)
places `sourceBindingSetDigest` in the canonical build specification and
therefore in `generationSpecDigest` and the final `IndexGenerationId`. It also
states that `sourceBindingSetDigest` covers the official observation
identities. The [RAG module](../prompts/foundation/RAG-Module.md) confirms that
every ordered activation binding is covered by the manifest digests.

The same accepted decision requires a `304` or identical content hash to append
a new observation and update only the observation binding, without creating a
new index generation. It also says that freshness observations remain outside
generation identity. These rules cannot all hold simultaneously: changing
`sourceObservationId` either changes the digest and generation identity or
leaves the activation record pointing to a binding not represented by the
active manifest digest.

Impact: `STATE-03` cannot define one deterministic compare-and-swap and
manifest-validation algorithm from the accepted semantics. An implementation
could reject a valid revalidation, silently break digest coverage or serve
freshness/provenance metadata that no longer agrees with the active generation.
This affects the core rollback and provenance integrity boundary, so it blocks
the Automatic Quality Gate.

Reproduction: trace ADR-0002's `304` rule, seven-field build specification,
full-manifest generation identity and observation-inclusive
`sourceBindingSetDigest`, then compare them with the observation-only
compare-and-swap in the RAG module and the `sourceObservationId` binding in the
[canonical activation record](architecture/STATE-02-Canonical-Contracts.md).

Required correction: a later accepted ADR must choose one coherent identity
model. It can either keep revalidation observations outside generation
identity and define the exact snapshot/source fields covered by the manifest,
or keep observation identity in the digest and require a newly finalised
manifest/generation identity for each rebinding. The correction must reconcile
ADR-0002, the RAG module, canonical contracts, acceptance criteria, threat
model and rollback semantics before the combined audit is repeated.

#### `AQG-S02-002` — `P2 Média` — accepted residual-risk decisions remain marked pending

The [threat model](security/STATE-02-Threat-Model.md) states that its
residual-risk boundaries were included in the explicit ADR acceptance, and
the accepted ADR/report evidence confirms owner acceptance of the local-only
TLS revocation residual and bounded OpenAI disclosure/residency model. However,
`THR-S02-008` still says the revocation risk needs an owner decision, while
`THR-S02-014` and `THR-S02-015` still say they are blocked by an owner
disclosure/risk decision.

Impact: a Human Gate summary derived from the threat register would
misrepresent which security decisions are complete and which implementation,
account and runtime controls remain open. The register must distinguish an
accepted architecture risk boundary from unimplemented mitigations and
separately unauthorised egress.

Required correction: reconcile those three residual statuses and the risk
acceptance section with the accepted ADR authority, without claiming that any
control was implemented or that provider use is authorised.

#### `AQG-S02-003` — `P3 Baixa` — routed proposal documents retain superseded decision status

The `STATE-00` foundation documents are explicitly marked as proposals and the
accepted ADRs have higher precedence, so they do not create a competing
architecture authority. The routed
[solution architecture](../prompts/foundation/Solution-Architecture-Document.md),
[vision and requirements](../prompts/foundation/Prompt-New-Project.md) and
[security policy](../prompts/governance/Security-And-Access.md) nevertheless
remain current high-level context while showing a separate
`RagChallenge.Rag.Abstractions` project, describing the administration/TLS
choices as still belonging to `STATE-02`, and listing provider, persistence
and OCI selection as pending without the accepted conditional qualification.

Impact: a reader can reach stale status or physical-boundary guidance before
following the ADR links. Current State and the architecture pack prevent an
authority ambiguity, but the drift increases implementation and maintenance
risk.

Required correction: preserve the existing `pt-BR` language and historical
proposal context while adding concise accepted-baseline qualifications or
replacing superseded status wording. This is a documentary reconciliation,
not authority to implement the decisions.

No P0 finding was observed. The audit did not silently correct any finding.

### Limitations and residual risks

- Existing public-source evidence was reviewed locally and was not re-fetched;
  network access was explicitly prohibited.
- Exact PDF/CSV package versions, parser quality/security, OpenAI account tier,
  entitlement, spend controls and bilingual behaviour remain unverified.
- The representative `SqliteExactVectorStore` envelope, restart, concurrency,
  corruption, application-consistent backup and restore remain untested.
- OCI tenancy capacity, entitlement, IAM enforcement, effective billing,
  egress and recovery remain unverified.
- No authorised product corpus, per-document rights set, evaluation dataset or
  active index exists. These remain later activation and execution inputs, not
  evidence supplied by this documentary audit.
- The 36 threat controls remain design requirements until their owning states
  implement and test them. Accepted risk boundaries do not enable egress or
  make those controls effective.

### Pending conditions

1. Accept a corrective ADR that resolves `AQG-S02-001` without weakening
   provenance, rollback or activation atomicity.
2. Reconcile `AQG-S02-002` and `AQG-S02-003` while preserving factual
   limitations and the existing language of `STATE-00` documents.
3. Repeat the combined local documentary audit on a named clean baseline and
   record its result before any Human Gate summary is prepared.

The result-registration diff changed exactly seven documentary files. The
post-registration repository audit passed for the same 83 non-ignored files,
30 Markdown files, 138 resolving local links, one H1 per Markdown file,
balanced fences and no unresolved marker; `git diff --check` also passed.

## Corrective decision package

### Authority and baseline

The owner authorised a local documentary package on clean
`main@9707b87d75a6acb14c8993ff0283a4221bc6c762`, corpus `4.8.0`, to propose a
corrective ADR for `AQG-S02-001` and factually reconcile `AQG-S02-002` and
`AQG-S02-003`. The authority expressly excluded acceptance or rejection of the
new ADR, a repeated Automatic Quality Gate, a Human Gate, `STATE-03`,
implementation, network/provider/account access and external mutation.

The package introduces
[ADR-0007](architecture/ADR-0007-Generation-Identity-And-Freshness-Observation-Rebinding.md)
with status `proposed`. Committing or reviewing that document does not alter
accepted ADR-0002.

### Identity models compared

| Model | Coherence | Consequence | Assessment |
|---|---|---|---|
| Separate generation and activation identities | `sourceBindingSetDigest` covers source/trust/registration/snapshot; a new `activationBindingSetDigest` also covers `sourceObservationId`. | `304` changes the activation record without changing manifest or vectors. | Recommended: preserves deterministic artefact identity and append-only freshness integrity. |
| Observation-inclusive generation identity | `sourceObservationId` remains in `sourceBindingSetDigest`. | Every `304`, identical hash, withdrawal or observation-only change finalises another manifest and `IndexGenerationId`, even if vectors are reused. | Coherent but not recommended: freshness is conflated with derived artefacts and creates avoidable churn. |
| Mutable or undigested observation binding | Manifest/record is mutated in place or the observation is excluded from every canonical digest. | Identity or activation integrity becomes unverifiable. | Rejected. |

ADR-0007 recommends the first model. Its exact canonical boundary is:

- `sourceBindingSetDigest` covers the ordinal generation-bound projection of
  database/document identity, source adapter, trust class, immutable/versioned
  source registration and immutable snapshot;
- `sourceObservationId` is excluded from the manifest,
  `generationSpecDigest` and `IndexGenerationId`;
- `activationBindingSetDigest` covers the complete ordinal activation binding,
  including `sourceObservationId`, and is stored/audited with every complete
  `CorpusActivationRecord` revision;
- appending an observation advances the observation journal and activation
  revision, not the generation-bound `catalogueRevision`.

A compatible `304` or identical hash therefore preserves manifest bytes,
`sourceBindingSetDigest`, `generationSpecDigest`, `IndexGenerationId`,
`catalogueRevision` and `generationActivatedAt`. It changes
`sourceObservationId`, `activationBindingSetDigest`, `recordRevision` and
`recordUpdatedAt` through compare-and-swap. New content, snapshot, trust,
adapter, immutable source registration, document membership/version/format or
compatibility input still requires a new candidate generation.

Rollback targets a retained validated generation but constructs a new
activation-record revision with observations that are compatible and eligible
under current policy. It never replays an old record byte for byte, rewrites an
observation or makes an expired snapshot fresh. The operation fails closed if
the target would violate the active-database/evidence invariant.

### Cross-cutting impact trace

| Area | Current impact | Required action if ADR-0007 is accepted |
|---|---|---|
| ADR-0002 | Contains the conflicting observation-inclusive digest and exact-record rollback wording. | Supersede only those clauses; preserve all other accepted lifecycle/provider decisions. |
| Canonical contracts | Defines `sourceObservationId` but no record-level digest. | Add `activationBindingSetDigest`, three projection validations and the exact `304` field transition. |
| RAG module | Says all bindings are manifest-covered while freshness is outside generation identity. | Separate the two digests and the observation-journal/catalogue revisions; use freshness-safe new-record rollback. |
| Requirements | `RNF-005`, `AC-MVP-005` and `AC-MVP-014` express traceability, complete activation and observation-only rebinding without naming both integrity domains. | Refine those outcomes without weakening provenance or rollback. |
| Lifecycle and Quality Gates | `STATE-03`/`STATE-04` require bindings/digests and `304`, but not their exact split. | Require canonical vectors, observation compatibility and rollback that cannot revive freshness. |
| Roadmap/backlog | S03/S04 and `BL-M14` assume a complete record and observation rebinding. | Add the record digest and new-record rollback checks. |
| Threat model | Generation mixing, stale coverage, partial activation, concurrency, cleanup and rollback threats depend on the boundary. | Link the corrected digest/mismatch tests to the existing affected threat and security-test IDs. |

No semantic edit in that table is applied before an explicit ADR-0007
decision and follow-on authority.

### Factual reconciliation of the remaining findings

`AQG-S02-002` source text is reconciled without changing a risk decision:

- `THR-S02-008` now records the accepted local-only TLS residual boundary and
  keeps implementation plus clean-environment evidence open;
- `THR-S02-014` and `THR-S02-015` now record the accepted disclosure boundary
  while keeping account, egress, budget, user-notice and runtime evidence open;
- the risk-acceptance section distinguishes the 2026-08-01 architecture
  decisions from authority/evidence still required for external execution.

`AQG-S02-003` source text is reconciled while preserving the existing
Brazilian Portuguese and `STATE-00` proposal context:

- the solution architecture now points to the accepted ADR-0003 physical map,
  places RAG abstractions in Application and persistence in Infrastructure,
  and records the accepted one-shot administration and conditional SQLite
  direction without claiming implementation;
- the vision now distinguishes accepted provider/persistence/OCI/egress
  decisions from missing documents, account, package, performance, IAM,
  backup, restore and evaluation evidence;
- Security and Access now records the accepted administration, provider and
  local-only TLS boundaries while preserving deny-by-default egress and future
  test requirements.

These source corrections remove the identified stale decision labels. They do
not change the historical finding, pass the failed gate or prove a control.
Their disposition remains subject to the next separately authorised combined
audit.

### Package status and validation boundary

- At package validation, ADR-0007 was `proposed`; no owner decision had yet
  been recorded.
- `AQG-S02-001`: open and blocking until a corrective ADR is accepted and the
  accepted semantic documents are reconciled.
- `AQG-S02-002` and `AQG-S02-003`: source documents factually reconciled;
  audit disposition pending the next combined gate.
- Automatic Quality Gate: not repeated and remains `REPROVADO`.
- Human Gate: not requested and remains `PENDENTE`.
- `STATE-03`: not authorised.

Directed validation of this proposal package is documentary diff validation,
not an Automatic Quality Gate. From `<rag-challenge-root>` at
`2026-08-02T03:55:59.7207833Z`, PowerShell `7.6.4`, Git
`2.55.0.windows.3` and ripgrep `15.2.0` produced these observed results:

| Check | Exit/result |
|---|---|
| `eng/check-repository.ps1` | Exit `0`; 84 non-ignored files passed format, local-link, ignored-material and common-secret checks. |
| `git diff --check` | Exit `0`. |
| Directed Markdown/ADR/reconciliation script | Exit `0`; 84 non-ignored files, 31 Markdown, 13 prompt files and 142 local Markdown links; one ADR-0007 `proposed` status; zero stale target phrases; zero pre-decision changes to ADR-0002, canonical contracts or the RAG module. |

After the append-only record was added, the checks were repeated at
`2026-08-02T03:59:10.4784506Z` with the same successful counts and results;
the directed scope check also confirmed exactly 12 changed documentary paths.
No build, runtime, provider, account, network or remote check is part of this
package.

## ADR-0007 decision registration

### Authority and baseline

The owner explicitly accepted ADR-0007 on 2026-08-02 with the exact decision
`ADR-0007: ACEITAR.`. Before registration, the repository remained on clean
`main@664187c6926be5ce4bef3734603f8d936626d535`, instruction corpus `4.8.1`,
and ADR-0007 had one `proposed` status and no `accepted` status.

### Decision and boundary

ADR-0007 is `accepted`. It supersedes only the observation-inclusive
generation-identity and exact-record rollback clauses of accepted ADR-0002.
The generation-bound `sourceBindingSetDigest` excludes
`sourceObservationId`; the complete activation binding, including the
observation, is protected separately by `activationBindingSetDigest`.

This registration does not apply the traced semantic edits to ADR-0002,
canonical contracts, the RAG module, requirements, lifecycle, Quality Gates,
roadmap or threat model. It therefore does not dispose of `AQG-S02-001` or
make the accepted baseline ready for another audit. The factual source
reconciliation for `AQG-S02-002` and `AQG-S02-003` is unchanged and still
awaits disposition by a later combined audit.

No implementation, build, parser, provider, account, corpus, index, network,
GitHub, OCI, publication, deployment or DB-Notifier action was authorised or
performed. The Automatic Quality Gate remains `REPROVADO`, the Human Gate
remains `PENDENTE`, and `STATE-03` remains unauthorised.

The decision registration is corpus `4.9.0` (`MINOR`) because a proposed
architecture correction becomes accepted authority without changing the
lifecycle or functional scope. Directed validation of the registration is a
documentary scope check and does not repeat the Automatic Quality Gate.

The repository audit passed for 84 non-ignored files. `git diff --check`
returned exit `0`; the registration changed exactly eight documentary paths;
ADR-0007 has one `accepted` status, one acceptance date and no `proposed`
status; the State Transition Log changed only by append; and ADR-0002,
canonical contracts and the RAG module have zero pre-reconciliation diff.

## ADR-0007 semantic reconciliation

### Authority, baseline and scope

The owner authorised this local documentary reconciliation on clean
`main@9aa90c012e3bc973330f5a79678fc358c81809df`, corpus `4.9.0`. The authority
applies accepted ADR-0007 semantics to ADR-0002, canonical contracts, the RAG
module, requirements, lifecycle, Quality Gates, roadmap, threat model and the
necessary factual records. It expressly excludes implementation, a repeated
Automatic Quality Gate, a Human Gate request, `STATE-03`, network/provider/
account access, GitHub, OCI, DB-Notifier, publication and deployment.

This documentary correction is corpus `4.9.1` (`PATCH`): it makes existing
accepted authority internally current without adding a capability, changing a
lifecycle boundary or claiming executable behaviour.

### Reconciled canonical semantics

- `sourceBindingSetDigest` covers the ordered generation-bound projection of
  database/document identity and revision, document format, source adapter,
  trust class, immutable/versioned official registration and immutable
  snapshot. `sourceObservationId` is excluded from that digest,
  `generationSpecDigest`, the complete manifest digest and
  `IndexGenerationId`.
- `activationBindingSetDigest` is stored with every complete activation-record
  revision and covers the same projection plus `sourceObservationId`. The two
  domains have distinct version discriminators, fixed UTF-8 field encoding,
  ordinal order and unambiguous null handling; `STATE-03` must materialise
  golden vectors.
- Before compare-and-swap, Application validates
  `activeDocumentSetDigest` and the generation-bound
  `sourceBindingSetDigest` against the finalised manifest, validates
  `activationBindingSetDigest` against the proposed record, and proves every
  observation names the binding's immutable registration and snapshot.
- `catalogueRevision` remains the immutable generation-bound catalogue
  snapshot. An observation-only append advances the observation journal and
  activation-record revision, not that catalogue revision or a generation ID.
- A compatible `304` or identical hash appends an observation and creates a
  new complete activation-record revision. It changes `recordRevision`,
  `previousRecordRevision`, `recordUpdatedAt`, the affected
  `sourceObservationId` and `activationBindingSetDigest`; it preserves
  manifest bytes, `sourceBindingSetDigest`, `generationSpecDigest`,
  `IndexGenerationId`, `catalogueRevision` and `generationActivatedAt`.
- Withdrawal or source deactivation can use the same observation-only record
  transition. Query resolves one activation record, derives eligible
  generation-bound binding selectors from its observations and requires the
  vector store to hard-filter them before ranking/top-k. It never reads a
  separate "latest observation".
- Content/snapshot, source adapter, trust, immutable registration, document
  membership/version/format or compatibility changes require a new candidate
  generation.
- Rollback targets a retained, validated generation and its generation-bound
  projection but constructs a new complete record. Explicitly selected
  observations must be compatible and currently eligible; historical record
  bytes and freshness are never replayed. Failure of the active-database and
  eligible-evidence invariant leaves the current record unchanged.

### Traceability and limits

ADR-0002 is marked as amended only for the clauses superseded by ADR-0007.
The contracts, routed solution architecture, RAG module, `RNF-005`,
`AC-MVP-005`, `AC-MVP-014`, lifecycle, Quality Gates, S03/S04/S07 roadmap
work, `BL-M14` and the affected threat/test IDs now require the same
dual-digest, revalidation, pre-filter and new-record rollback model. The
corpus-change template records both digest domains and revision scopes.

No type, migration, store, provider, parser, index, corpus or runtime test was
implemented or executed. The directed checks for this reconciliation validate
documentary structure, traceability and diff consistency only; they are not a
new Automatic Quality Gate and cannot dispose any historical finding.

Directed validation from `<rag-challenge-root>` used PowerShell `7.6.4`, Git
`2.55.0.windows.3` and ripgrep `15.2.0`. At
`2026-08-02T05:21:42.2850391Z`, the semantic assertions passed across 17
changed paths before the append-only state record: both digest names and the
corrected observation boundary were present in every owning semantic artefact;
the canonical domain versions, eligible-binding pre-filter, exact revalidation
field set, mismatch/idempotency and new-record rollback assertions passed; and
the existing counts remained 25 functional requirements, 18 non-functional
requirements, 20 acceptance criteria, 19 Must backlog items, 36 threats and 15
security-test groups. `eng/check-repository.ps1` returned exit `0` for 84
non-ignored files, and `git diff --check` returned exit `0`. These checks are
repeated after the factual record below is appended.

After the append-only state record, the full check set was repeated at
`2026-08-02T05:24:01.9744437Z`. The repository audit and
`git diff --check` again returned exit `0`; the semantic/traceability script
returned exit `0` for all 18 changed paths with the same requirement, backlog,
threat and security-test counts, and separately confirmed that the State
Transition Log retained its entire prior content unchanged before the new
entry.

## Renewed combined audit after ADR-0007 reconciliation

### Authority, baseline and method

The owner separately authorised a new combined local documentary Automatic
Quality Gate audit of the reconciled `STATE-02` baseline and its factual
registration. The authority excluded implementation, network/provider/account
access, GitHub, OCI, DB-Notifier changes, publication, deployment, a Human Gate
request and `STATE-03` authorisation.

The audit started from clean `main` at
`3978a17201cf5f6ac4ddc189862736fc3646457b`, instruction corpus `4.9.1`. The
first recorded command instant was `2026-08-02T12:23:41.9941263Z`. PowerShell
`7.6.4`, Git `2.55.0.windows.3` and ripgrep `15.2.0` performed the checks from
`<rag-challenge-root>`. Runtime preflight remained `NÃO APLICÁVEL` because the
audit changed or validated no executable behaviour.

The audit reread the complete governing, lifecycle, state/history, accepted
ADR, canonical-contract, requirement, architecture, RAG, roadmap, threat and
report baseline. It then re-executed repository checks and traced the two
canonical digest domains, generation identity, observation freshness,
activation, rollback, provenance and the three historical findings across
their owning artefacts.

### Results by gate

| Gate area | Result | Observed evidence |
|---|---|---|
| Baseline and authority | `APROVADO` | Branch `main`, commit, corpus `4.9.1` and clean working tree matched the authorised baseline before inspection and again before registration. |
| Repository structure and hygiene | `APROVADO` | `eng/check-repository.ps1` returned exit `0` for 84 non-ignored files. All 31 Markdown files had one H1 and balanced fences; 143 local links resolved; UTF-8/LF, final newline, trailing whitespace, ignored-material, common-secret, merge-marker and diff-hygiene checks passed. |
| ADR decision state | `APROVADO` | ADR-0002 and ADR-0004 to ADR-0007 each contain exactly one `accepted` status, one acceptance date and one decision-authority field; no `proposed` status remains in those records. |
| Catalogue, requirements and test traceability | `APROVADO` | The vision and ADR-0004 retain identical ordered catalogues with 51 unique products, 54 assignments and 9 categories; only Redis, SAP HANA and SingleStore occur twice. Counts remain 25 RF, 18 RNF, 20 acceptance criteria, 19 Must backlog items, 36 unique threats and 15 unique security-test groups. |
| PDF/CSV, providers, persistence, OCI and egress | `APROVADO` for documentary qualification | PDF/CSV adapters, conditional package selection, bounded provider disclosure, durable catalogue/content/vector persistence, conditional OCI targets and four deny-by-default egress profiles remain explicit and are not represented as installed, enabled or runtime-verified. |
| Digest domains and generation identity | `APROVADO` | `sourceBindingSetDigest` excludes `sourceObservationId`; `activationBindingSetDigest` covers the complete binding. Distinct version discriminators, fixed field/order/null encoding, duplicate rejection and the three pre-CAS projection validations are defined consistently. |
| Freshness, activation and rollback | `APROVADO` | Compatible `304`/identical-hash rebinding changes the complete activation record and its digest while preserving the exact generation-bound fields. Query uses one resolved record and hard pre-filters eligible generation-bound selectors before top-k. Rollback constructs a new record with explicitly selected compatible/currently eligible observations and never replays historical freshness. |
| Provenance, security and routed-document currency | `APROVADO` | Citation/coverage contracts preserve source, snapshot, observation and trust. The threat register distinguishes accepted risk boundaries from missing controls/evidence, and routed foundation documents use the accepted ADR-0003 physical map and conditional provider/persistence/OCI status. |
| Executable, provider, account and remote checks | `NÃO APLICÁVEL` | No build, parser, provider, account, corpus, index, source fetch, OCI action or external mutation was authorised or performed. |

### Historical finding disposition

| Finding | Previous severity | Disposition | Factual basis |
|---|---|---|---|
| `AQG-S02-001` | `P1 Alta` | `RESOLVED` | Accepted ADR-0007 and corpus `4.9.1` separate generation-bound and activation-bound identity, define exact canonical domains and field transitions, and reconcile query/rollback semantics. The former contradiction is not reproducible. |
| `AQG-S02-002` | `P2 Média` | `RESOLVED` | `THR-S02-008`, `THR-S02-014`, `THR-S02-015` and the risk-acceptance section now record the accepted architecture boundaries while keeping control, account, egress, budget, notice and runtime evidence open. |
| `AQG-S02-003` | `P3 Baixa` | `RESOLVED` | The routed `STATE-00` documents preserve their language and proposal context while pointing to the accepted physical map and accepted/conditional administration, TLS, provider, persistence and OCI decisions. |

The earlier severities remain historical evidence in the first combined-audit
section. This audit observed zero new P0, P1, P2 or P3 findings and did not
silently correct a finding.

### Limitations and residual risks

- Existing public-source evidence was inspected only from the repository; no
  source was re-fetched and no current external fact was inferred.
- Exact PDF/CSV package versions, parser quality/security, provider account
  entitlement, spend controls and bilingual runtime behaviour remain
  unverified.
- The representative `SqliteExactVectorStore` envelope, restart, concurrency,
  corruption, application-consistent backup and restore remain untested.
- OCI tenancy capacity, entitlement, IAM enforcement, effective billing,
  egress and recovery remain unverified.
- No authorised product document set, complete per-document rights evidence,
  evaluation dataset, active corpus or index exists.
- The 36 threats remain design requirements until their owning states
  implement and test the controls. Accepted TLS, provider-disclosure and
  conditional OCI risk boundaries neither enable egress nor prove control
  effectiveness.

### Pending conditions and registration validation

The Automatic Quality Gate for `STATE-02` is approved on this documentary
baseline. The Human Gate remains `PENDENTE` and was not requested by this
audit. `STATE-02` remains active and `STATE-03` remains unauthorised. Preparing
and presenting a complete Human Gate summary requires separate owner authority;
state progression still requires the explicit Human Gate and its factual
append-only registration.

The result-registration diff changed exactly seven documentary files. The
post-registration repository audit returned exit `0` for 84 non-ignored files
and 31 Markdown files; all 143 local links resolved, every Markdown file had
one H1 and balanced fences, semantic assertions returned zero failures, and
`git diff --check` returned exit `0`.

## Current gate assessment

Automatic Quality Gate for `STATE-02`: `APROVADO`. The renewed combined audit
disposed `AQG-S02-001`, `AQG-S02-002` and `AQG-S02-003` as `RESOLVED` and
observed zero new P0, P1, P2 or P3 findings. The approval applies to the
documentary architecture baseline only and does not convert any accepted
decision into implementation or runtime evidence.

Human Gate for `STATE-02`: `APROVADO` without reservations on 2026-08-02,
after the complete current-baseline summary and critical documentary samples
were presented to the product owner.

`STATE-02 ARCHITECTURE` is closed. `STATE-03 DATA_AND_INDEX_MODELING` is not
authorised.

## 2026-08-02 Human Gate decision

- State: `STATE-02 ARCHITECTURE`.
- Decision baseline: clean `main` at
  `6e61c4cf4429e2a62145d43bec3783146f01e37f`, instruction corpus `4.9.1`.
- Validator and date: RAG-Challenge product owner, 2026-08-02.
- Automatic report reviewed: the renewed combined audit was `APROVADO`,
  disposed `AQG-S02-001`, `AQG-S02-002` and `AQG-S02-003` as `RESOLVED`, and
  observed zero new P0, P1, P2 or P3 findings.
- Critical samples reviewed: the 51/54/9 catalogue and PDF/CSV lifecycle; the
  PostgreSQL source candidate; conditional parser, OpenAI, SQLite/vector and
  OCI selections; four deny-by-default egress profiles; 36 threats and 15
  security-test groups; bilingual query/provenance semantics; separate
  generation/activation digest domains; exact revalidation transitions;
  eligible-binding hard pre-filtering; and freshness-safe new-record rollback.
- Samples not repeated: no executable parser, provider, account, corpus,
  index, backup, restore, IAM, egress, OCI or user-interface sample exists at
  this state. Repeating those behaviours was not applicable to this
  documentary Human Gate and would require later-state authority.
- Experience and messages: the reviewed architecture distinguishes planned,
  accepted, implemented and tested status; degraded coverage and insufficient
  evidence remain explicit contract outcomes; interface language and theme
  decisions remain independent and unimplemented.
- Security and authority: accepted TLS, provider-disclosure and conditional
  OCI risk boundaries neither prove controls nor enable egress. No network,
  provider, account, GitHub, OCI, publication, deployment or DB-Notifier
  action is included.
- Accepted limitations and residual risks: exact PDF/CSV packages, parser
  quality/security, product documents and rights, evaluation data, provider
  entitlement/spend/bilingual behaviour, exact-vector capacity,
  backup/restore, OCI tenancy/IAM/billing and all runtime threat controls
  remain evidence for their owning later states.
- Reservations: none.
- Decision: `APROVADO` without reservations.
- Unambiguous confirmation:
  `Confirmo a decisão acima exclusivamente para STATE-02`.
- Lifecycle effect: `STATE-02` is closed. Entry into `STATE-03` requires a
  separate explicit owner authorisation and was not granted by this Human
  Gate.
