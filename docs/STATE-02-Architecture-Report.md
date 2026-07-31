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
- Official OpenAI discovery results and the GPT-4.1 launch publication, up to
  the first mandatory stop condition described below.
- PostgreSQL `robots.txt` and a direct local offline-chain TLS assessment.
- Current read-only NuGet registry version, registration and catalogue
  metadata for PdfPig, without downloading a package.

No provider endpoint, OCI service, account, credential or paid API was
accessed. No package, corpus, model or complete PDF was downloaded, and no
external state was changed.

## Bounded external verification record

| Area | Observed result | Reconciliation status |
|---|---|---|
| PostgreSQL candidate PDF | Exact `HEAD` returned `200`, no `Location`, `application/pdf`, 15,771,040 bytes, ETag `"6a05c867-f0a5a0"`, last modified `2026-05-14T13:04:39Z` and byte-range support. Exact `GET bytes=0-65535` returned `206`, 65,536 bytes, range total 15,771,040 and `%PDF-1.4`. | Verified for immediate response, media, size, validators, range and signature; no body persisted. |
| PostgreSQL source rights | The [documentation index](https://www.postgresql.org/docs/) identifies version 18 as current. The [PostgreSQL Licence](https://www.postgresql.org/about/licence/) covers use, copy, modification and distribution of software and documentation with required notices. The [official policies index](https://www.postgresql.org/about/policies/) lists no separate general terms-of-use policy. | Published use-right basis verified; no conflicting general terms policy was found. |
| PostgreSQL robots/rate | [`robots.txt`](https://www.postgresql.org/robots.txt) returned `200` without redirect and does not disallow `/files/documentation/`. Targeted official search found no publisher download-rate guidance. | No policy conflict found; the proposed daily ceiling is a conservative project limit, not a publisher commitment. |
| PostgreSQL TLS | Three DNS answers were public; direct TLS negotiated TLS 1.3 with `TLS_AES_256_GCM_SHA384`. A four-element chain validated locally with certificate downloads disabled and revocation `NoCheck`; the leaf was valid from `2026-06-02T17:09:47Z` to `2026-08-31T17:09:46Z`, SHA-256 `2738805715f7b32e7850b33dc2319e9d7b39d9acfb44936d56a1d18a6ef805ac`. | Current local no-lateral-download validation verified; offline-revocation, future rotation and clean OCI reproduction remain explicit residual/later-test concerns. |
| Proposed local-corpus licence | The [CC BY 4.0 deed](https://creativecommons.org/licenses/by/4.0/) and [Portuguese legal code](https://creativecommons.org/licenses/by/4.0/legalcode.pt) confirm sharing/adaptation rights and attribution, licence-link, change-marking and no-additional-restriction duties. | Licence terms verified; the corpus is not authored and the owner has not granted the licence on it. |
| PdfPig | The live [NuGet version index](https://api.nuget.org/v3-flatcontainer/pdfpig/index.json), [0.1.15 registration](https://api.nuget.org/v3/registration5-semver1/pdfpig/0.1.15.json) and catalogue metadata identify 0.1.15 as the newest stable release, published `2026-06-25T14:57:32.300Z`, listed, Apache-2.0 and tied to allowlisted repository commit `f131f642976936e06ee91cb19d3ed728f9dd18b6`; 0.1.16 builds are pre-release. | Corrects the earlier 0.1.14 observation. No version selected or installed. The catalogue exposed no `vulnerabilities` property, which does not prove absence; broader security/runtime evidence remains incomplete. |
| OpenAI | The [GPT-4.1 launch publication](https://openai.com/index/gpt-4-1/) corroborates the `gpt-4.1-mini` family and historical launch pricing. A second authority allowed the previously blocked documentation host. | No second-round OpenAI request occurred before the new stop condition. Current model/snapshot, dimensions, endpoints, data controls, price, quota and SDK evidence remains incomplete. |
| OCI | No request made before either mandatory browsing stop. | Region, shape, capacity, price, quotas, storage, Vault and endpoint evidence remains unverified. |

The initial HTTPS request used normal operating-system certificate validation.
The second round added the separate offline-chain assessment recorded above;
application integration and clean OCI reproduction remain untested. No
certificate, PDF body or external response was saved in the repository.

The second round stopped when a PdfPig security search returned results for
GitHub repositories and hosts outside the explicit allowlist, including
`github.com/bcgov/entity`, `github.github.com`, `docs.github.com` and
`raw.github.com`. None of those results was opened or used as evidence. No
subsequent OpenAI or OCI request was attempted.

## S02-A — Blocking decisions

### Prepared decision package

| Area | Proposed selection | Decision/evidence state |
|---|---|---|
| RAG lifecycle and source separation | ADR-0002 as written | `proposed`; explicit human decision required. |
| Local corpus | Owner-authored `Database Systems Catalogue — MVP`, 12 named systems, `CC BY 4.0` | `proposed`; content not authored and licence not yet granted. |
| Official source | Versioned PostgreSQL 18 A4 PDF candidate | `proposed`; URL, media, size, redirect behaviour, licence basis, robots and local offline TLS verified with documented rate/revocation qualifications. |
| Parser | PdfPig adapter | `proposed`; 0.1.15 is the current verified stable candidate under Apache-2.0; version selection, vulnerability and runtime evidence pending. |
| Normalisation | Unicode NFC and deterministic whitespace/control policy | `proposed`; locally specified. |
| Chunking | `paragraph-window-v1`, target 3,200 scalars, overlap 480, hard max 4,000 | `proposed`; must pass evaluation. |
| Embeddings | OpenAI `text-embedding-3-small`, 1,536 dimensions, cosine | `proposed`; current contract, dimensions, terms, price, quota and availability were not reached before the second stop. |
| Language model | OpenAI `gpt-4.1-mini-2025-04-14` | `proposed`; family/historical launch price corroborated, but the second round did not reach current snapshot, contract, data policy, price or quota evidence. |
| Catalogue/control persistence | EF Core SQLite | `proposed`; exact packages remain unapproved. |
| Raw content | Durable content-addressed filesystem | `proposed`; contract specified. |
| Vector store | Local `SqliteExactVectorStore`, hard SQL pre-filter and exact cosine ranking | `proposed`; 10,000-chunk cap requires later performance proof. |
| OCI | Single ARM64 OCI Compute instance in `sa-saopaulo-1`, durable block volume | `proposed`; verification not reached before either mandatory browsing stop. |
| Evaluation | `rag-eval-mvp-v1`, 80 cases and pre-registered thresholds | `proposed`; dataset not authored or run. |

### Alternatives retained

- local embedding/LLM if provider data terms or cost are rejected;
- PostgreSQL/pgvector if the exact SQLite adapter fails the performance gate;
- another versioned official PDF only through an amended source ADR;
- another OCI shape/region when verified capacity or budget requires it.

None is an active fallback. Switching requires a decision and a new
compatibility/evaluation baseline.

### S02-A status

`PARCIAL`: the architecture recommendation, alternatives, consequences,
owners and blocking evidence are documented. Completion requires the
outstanding primary-source verification and explicit human decisions for
ADR-0002, ADR-0004 and ADR-0005.

## S02-B — Contracts and security

### Prepared contracts

- core identity and provenance contracts;
- source, parser, chunker, embedding, vector, language-model and persistence
  ports;
- complete `CorpusActivationRecord` compare-and-swap semantics;
- query request/response and citation v1;
- canonical `ApplicationFailure → CH_* → HTTP/Problem Details` table;
- readiness semantics that keep Local serviceable during official-source
  degradation;
- one-shot administration commands, inputs, idempotency and exit categories;
- OpenAPI v1 compatibility rules;
- required architecture, contract, adversarial, crash and negative tests.

### Prepared security design

- four independent deny-by-default egress profiles;
- exact official URI, DNS-set rejection, approved-IP connection and Host/SNI
  preservation;
- redirects, proxy, ambient credentials and certificate downloads disabled;
- local-only trust with an explicit unverified revocation residual risk;
- bounded public query, rate, concurrency, deadline, retrieval and evidence;
- local OS-identity administration with enable flag, reason, lease,
  compare-and-swap and sanitised audit;
- 30 registered threats and 12 security test groups;
- explicit risk-acceptance boundaries for data disclosure, TLS, licence,
  provider, OCI and P0/P1 findings.

### S02-B status

`PREPARADO PARA DECISÃO`, not accepted: ADR-0006, the canonical contracts and
threat model are complete as proposals. Their external endpoint values and
residual-risk acceptances depend on ADR-0004/ADR-0005 evidence and owner
decisions.

## Deliverable map

| Lifecycle deliverable | Artefact | Status |
|---|---|---|
| ADRs accepted or rejected | ADR-0002 and ADR-0004 to ADR-0006 | Proposed; human decisions pending. |
| Canonical contracts and diagrams | `STATE-02-Canonical-Contracts.md`; data-flow diagrams in threat model | Prepared. |
| Detailed threat model | `security/STATE-02-Threat-Model.md` | Prepared. |
| Parser, embedding, vector and LLM selection | ADR-0005 | Proposed; current parser metadata verified, parser security/runtime and provider evidence incomplete. |
| Corpus and corpus licence | ADR-0004 | Proposed; corpus not authored. |
| Official PDF, URL, terms, licence, maxAge and limits | ADR-0004 | External facts substantially verified with explicit rate and offline-revocation qualifications. |
| Durable content/catalogue/index persistence | ADR-0005 | Proposed. |
| Four egress policies | ADR-0006 | Prepared; official-source URI observed, AI/OCI paths and endpoints incomplete. |
| Vector search, failures, readiness and OpenAPI | Contract document and ADR-0006 | Prepared. |
| SSRF and DNS/IP pinning | ADR-0006 and threat model | Prepared; not implemented/tested. |
| Evaluation, OCI and rollback | ADR-0004, ADR-0005 and contracts | Proposed; external evidence and later tests pending. |

## Blockers and required authority

### Remaining external verification blocker

The first authorised round stopped when OpenAI documentation discovery
produced the then-unallowlisted canonical host `developers.openai.com`. The
owner authorised that host for a second round. The second round then stopped
when the PdfPig security search returned GitHub hosts and repositories outside
the explicit allowlist. No result outside the allowlist was opened and no later
external request was attempted.

Completion requires a renewed instruction that either permits those search
results to be ignored without treating them as accessed authority or requires
direct exact-URL retrieval without general web search. It does not require
access to the unrelated results themselves.

The remaining evidence is:

- PdfPig vulnerability/security posture and authorised runtime compatibility;
- OpenAI current embedding/model availability, immutable snapshot,
  dimensions, API paths, data retention/training/residency, current pricing,
  quotas and official .NET integration;
- OCI region/shape availability, pricing, capacity, block-volume durability,
  Vault/secret integration and exact service endpoints.

Any resumed verification remains read-only and must not install or download a
package, model or complete corpus, call a paid AI API, create an OCI/GitHub
resource, persist a real source snapshot or expose a secret.

### Human decision blocker

After external evidence is reconciled, the owner must explicitly accept,
reject or request changes to each ADR. A `STATE-02` Human Gate does not accept
them by implication.

## Risks and residual decisions

- The official-source candidate still carries publisher-frequency uncertainty
  and offline-revocation/future-certificate risk despite passing the current
  URL, licence, robots and local TLS checks.
- External AI may be rejected due to data terms, cost, quota or region.
- Local-only certificate revocation checking has a residual risk requiring
  explicit acceptance.
- Exact SQLite vector search may miss latency/memory thresholds at the final
  corpus size.
- ARM64 OCI capacity or package compatibility may require the documented
  alternative shape.
- The owner-authored corpus schedule is a delivery risk and remains the first
  material product-data dependency.
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
| PdfPig identity/release/licence review | Corrected to stable candidate 0.1.15, published 2026-06-25, listed and Apache-2.0; no version selected and security/runtime evidence pending. |
| OpenAI verification | Historical family evidence only; the second round did not reach OpenAI before the new stop condition. |
| OCI verification | Not run before either mandatory external stop condition. |
| Prohibited external actions | None performed: no login, credential, paid API, complete artefact download, mutation or resource creation. |
| Post-reconciliation repository audit | Passed for 83 non-ignored files and 30 Markdown files; 30 unique threat IDs, 12 security-test groups, four proposed ADR statuses and clean diff-format checks. |
| Second-round reconciliation audit | Passed for the same 83-file/30-Markdown baseline, 30 threat IDs, 12 security-test groups, four unchanged `proposed` ADR statuses and clean structural/diff checks. |
| Executable spike, build or runtime test | Not run; documentary scope and no implementation change. |
| ADR decisions | Pending human decisions. |

## Current gate assessment

Automatic Quality Gate for `STATE-02`: `BLOQUEADO` until external facts are
verified, ADRs are explicitly decided and the combined documents are audited
against the resulting accepted baseline.

Human Gate for `STATE-02`: `PENDENTE` and must not be requested while the
Automatic Quality Gate is blocked.

`STATE-02 ARCHITECTURE` remains active. `STATE-03 DATA_AND_INDEX_MODELING` is
not authorised.
