# STATE-02 Architecture Report

## Purpose and scope

This report records the factual local and documentary execution of
`STATE-02 ARCHITECTURE` begun on 2026-07-31. It distinguishes prepared
proposals from accepted decisions, verified facts and blocked external
evidence.

The authorised work is sequential `S02-A` and `S02-B`. It excludes functional
RAG code, migrations, `STATE-03`, network access, installation, paid provider
use, secrets, GitHub, OCI mutation, publication, deployment, CD and
DB-Notifier changes.

## Baseline and authority

| Item | Observed value |
|---|---|
| Entry date | `2026-07-31` |
| Entry baseline | `47435930727fc344298d84658b1ad9b2da9b5b62` |
| Entry registration commit | `e9175b193b98bd0d8f464be7ed129da5af2de6aa` |
| Architecture decision-package commit | `979677fa1f4d7324340b8be15d88eb8b5b802a1a` |
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

## Local evidence inspected

- The accepted .NET/React bootstrap and dependency boundaries.
- Current requirements, architecture, RAG lifecycle, security, gates,
  roadmap, state/history and ADR pack.
- Current central package manifest and project package references.
- Local NuGet cache presence for EF Core SQLite `10.0.9`; cache presence was
  treated only as local availability evidence, not package approval.
- Git branch, commit, status, diff hygiene and repository audit.

No online provider, package registry, publisher, source URL, price, licence,
terms page or OCI service was accessed.

## S02-A — Blocking decisions

### Prepared decision package

| Area | Proposed selection | Decision/evidence state |
|---|---|---|
| RAG lifecycle and source separation | ADR-0002 as written | `proposed`; explicit human decision required. |
| Local corpus | Owner-authored `Database Systems Catalogue — MVP`, 12 named systems, `CC BY 4.0` | `proposed`; content not authored and licence not yet granted. |
| Official source | Versioned PostgreSQL 18 A4 PDF candidate | `proposed`; URL/licence/terms/media/size/TLS unverified. |
| Parser | PdfPig adapter | `proposed`; exact package/version/licence/vulnerability evidence unverified. |
| Normalisation | Unicode NFC and deterministic whitespace/control policy | `proposed`; locally specified. |
| Chunking | `paragraph-window-v1`, target 3,200 scalars, overlap 480, hard max 4,000 | `proposed`; must pass evaluation. |
| Embeddings | OpenAI `text-embedding-3-small`, 1,536 dimensions, cosine | `proposed`; current contract/terms/pricing/availability unverified. |
| Language model | OpenAI `gpt-4.1-mini-2025-04-14` | `proposed`; current contract/terms/pricing/availability unverified. |
| Catalogue/control persistence | EF Core SQLite | `proposed`; exact packages remain unapproved. |
| Raw content | Durable content-addressed filesystem | `proposed`; contract specified. |
| Vector store | Local `SqliteExactVectorStore`, hard SQL pre-filter and exact cosine ranking | `proposed`; 10,000-chunk cap requires later performance proof. |
| OCI | Single ARM64 OCI Compute instance in `sa-saopaulo-1`, durable block volume | `proposed`; shape, cost, capacity and services unverified. |
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
owners and blocking evidence are documented. Completion requires current
primary-source verification and explicit human decisions for ADR-0002,
ADR-0004 and ADR-0005.

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
| Parser, embedding, vector and LLM selection | ADR-0005 | Proposed; external verification blocked. |
| Corpus and corpus licence | ADR-0004 | Proposed; corpus not authored. |
| Official PDF, URL, terms, licence, maxAge and limits | ADR-0004 | Candidate and limits prepared; external evidence blocked. |
| Durable content/catalogue/index persistence | ADR-0005 | Proposed. |
| Four egress policies | ADR-0006 | Prepared; exact external paths/endpoints incomplete. |
| Vector search, failures, readiness and OpenAPI | Contract document and ADR-0006 | Prepared. |
| SSRF and DNS/IP pinning | ADR-0006 and threat model | Prepared; not implemented/tested. |
| Evaluation, OCI and rollback | ADR-0004, ADR-0005 and contracts | Proposed; external evidence and later tests pending. |

## Blockers and required authority

### External verification blocker

Separate read-only HTTPS authority is required to consult primary official
sources for:

- PostgreSQL PDF URL, response behaviour, licence, terms and robots/rate
  guidance;
- PdfPig package identity, current version, licence and security posture;
- OpenAI embedding/model availability, immutable identifiers, dimensions,
  API paths, data retention/training/residency, pricing and quotas;
- OCI region/shape availability, pricing, capacity, block-volume durability,
  Vault/secret integration and exact service endpoints.

The verification must not install, download a product corpus, call a paid AI
API, create an OCI/GitHub resource, persist a real source snapshot or expose a
secret. Any redirect or non-allowlisted host is a stop condition.

### Human decision blocker

After external evidence is reconciled, the owner must explicitly accept,
reject or request changes to each ADR. A `STATE-02` Human Gate does not accept
them by implication.

## Risks and residual decisions

- The official-source candidate may not meet licence, URL or TLS constraints.
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
| External source/provider/OCI verification | Not run; no authority. |
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
