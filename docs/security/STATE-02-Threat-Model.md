# STATE-02 Threat Model

## Purpose, responsibility and authority

This document is the architecture-stage security threat model accepted for the
RAG-Challenge MVP. It identifies assets, trust boundaries, abuse cases,
controls and required evidence before implementation. It does not prove a
control, enable network access or authorise an external action.

Security-And-Access remains the policy authority. Accepted ADR-0006 owns the
egress, administration and HTTP decisions; this model tests those decisions
against concrete threats. Its recorded residual-risk boundaries were included
in the explicit ADR acceptance. That acceptance settles the architecture risk
boundary only: runtime controls, account facts, egress authority and test
evidence remain unverified or absent as stated below.

## Scope

Included:

- anonymous public query and health endpoints;
- owner-authorised local PDF/CSV ingestion;
- administration of database products, categories, documents and versions;
- manual synchronisation of exact allowlisted official PDF/CSV registrations;
- parsing, chunking, embeddings, exact vector retrieval and grounded answer;
- SQLite control/vector stores and filesystem content store;
- one-shot local administration;
- external AI candidate and single-instance OCI candidate;
- build, configuration, secrets, logs, backups and rollback.

Excluded until a later decision:

- public upload or administration;
- multiple corpora, users, tenants or active providers;
- general web browsing/crawling;
- dynamic plug-ins, tools or agent actions;
- DB-Notifier integration;
- production deployment execution.

## Assets and security objectives

| Asset | Objective |
|---|---|
| Repository and build inputs | Integrity, reproducibility and no secrets. |
| Database catalogue, PDF/CSV documents and official snapshots | Authorised provenance, integrity, bounded disclosure and retention. |
| Content objects and backups | Immutability, confidentiality matching source, availability and verified recovery. |
| Catalogue and activation history | Atomic integrity, traceability and rollback availability. |
| Vectors and chunks | Catalogue/generation isolation and source-equivalent protection. |
| Provider credentials | Confidentiality, least privilege and revocability. |
| User question | Minimisation, bounded use and no default full-content logging. |
| Answer and citations | Grounded integrity, provenance and safe rendering. |
| Egress policy | Exact destination separation and fail-closed enforcement. |
| Administration authority | Authenticated local actor, purpose limitation and auditability. |
| OCI runtime | Minimal exposure, durable state and recoverability. |

## Trust boundaries and data flow

```text
Untrusted browser
  -> HTTPS/reverse proxy
  -> Server/API validation
  -> Application policy
     -> SQLite control plane
     -> local exact vector adapter
     -> immutable content store
     -> external AI adapter (separate egress)

Local authorised operator
  -> one-shot administration mode
  -> catalogue + local source / registered official source synchroniser
  -> quarantine -> parser -> content/catalogue/index candidate
  -> validated compare-and-swap activation

Each exact official publisher registration
  -> DNS/IP/TLS pinned connection
  -> bounded quarantine snapshot
```

Retrieved documents, provider responses, DNS responses, HTTP metadata, model
output and local file content remain untrusted after crossing their boundary.

## Assumptions requiring validation

- Every active database has at least one active authorised PDF/CSV document.
- Each candidate official URL is anonymous, stable and legally usable.
- The external AI provider's current terms permit the approved data classes.
- The chosen OCI shape and volume meet durability and performance needs.
- The operating-system trust store can validate the official source without
  lateral certificate downloads under the accepted policy.

ADR acceptance does not verify these assumptions as facts. Failure of an
assumption blocks the dependent candidate or activation and may require an ADR
change rather than weakening a control.

## Threat register

| ID | Threat and path | Impact | Required controls | Verification owner/state | Residual status |
|---|---|---|---|---|---|
| `THR-S02-001` | Prompt injection in local/official PDF or CSV directs the model to ignore policy or reveal data. | Ungrounded answer, policy override or leakage. | Treat evidence as delimited data; no tools; bounded context; validate citations; malicious evaluation cases. | Backend S04; homologation S07. | Open; design mitigated. |
| `THR-S02-002` | Model invents or forges a citation. | Misleading factual claim. | Server builds citations from retrieved IDs; reject unknown IDs; insufficient-evidence outcome. | S04/S07. | Open. |
| `THR-S02-003` | Retrieval mixes generation, ineligible activation binding, database filter or future corpus. | Cross-source disclosure and false provenance. | Validate manifest/record digests; derive eligible generation-bound selectors from one resolved record; SQL/physical hard pre-filter before ranking; adversarial higher-score tests. | S03/S04/S07. | Open. |
| `THR-S02-004` | Stale/unavailable source is silently represented as covered by another origin or query-time "latest observation" changes eligibility. | Misleading completeness/provenance. | Validate observation registration/snapshot relation; evaluate only the observation bound by the resolved record; hard pre-filter eligible bindings; explicit coverage and no substitution. | S04/S07. | Open. |
| `THR-S02-005` | User supplies URL/host/provider/model through query JSON. | SSRF, cost abuse or authority expansion. | Closed schema; reject unknown fields; trusted configuration only. | S04. | Open. |
| `THR-S02-006` | DNS rebinding or mixed A/AAAA reaches loopback, private or metadata service. | Internal service access or credential theft. | Atomic address-set rejection; resolve each connection; connect to approved IP; no second resolution. | S04/S07. | Open. |
| `THR-S02-007` | HTTP redirect escapes the approved official source. | SSRF or unauthorised content. | Automatic redirects disabled; any future hop needs a new decision. | S04/S07. | Open. |
| `THR-S02-008` | TLS chain validation fetches AIA/CRL/OCSP laterally. | Hidden egress and policy bypass. | Disable certificate downloads and online revocation; local trust only; test zero auxiliary connection. | S04/S07. | Architecture residual accepted; control and clean-environment evidence open. |
| `THR-S02-009` | Malicious/compressed PDF or oversized/malformed CSV exhausts CPU, memory, disk or parser. | Denial of service or parser exploit. | Signature/structure/media/page/row/column/cell/byte/time limits; quarantine; no active content/formula execution; dependency review. | S04/S07. | Open. |
| `THR-S02-010` | Path traversal, symlink or reparse point escapes local/content root. | Read/write of unrelated files. | Canonical root containment; open-handle checks where supported; no caller paths; deny links/reparse points. | S04/S07. | Open. |
| `THR-S02-011` | Partial candidate or record with mismatched binding digest becomes queryable or replaces active generation. | Corrupt/mixed answers and lost rollback. | Candidate identity only; final digest/count/readback; validate document, generation-bound `sourceBindingSetDigest` and complete `activationBindingSetDigest`; single activation authority; compare-and-swap. | S03/S04/S07. | Open. |
| `THR-S02-012` | Concurrent catalogue/sync/build/rebinding/rollback loses an update. | Split-brain catalogue/generation/bindings. | Per-corpus lease; separate observation-journal and catalogue revisions; expected record revision; digest plus complete transaction/history/audit. | S03/S04/S07. | Open. |
| `THR-S02-013` | Cleanup removes active/retained content, observation evidence or the only rollback target. | Irrecoverable service loss. | Reachability across retained generations, activation history and observation journal; retention window; explicit audited cleanup; restore test. | S03/S07. | Open. |
| `THR-S02-014` | External embedding discloses the whole authorised corpus over batches. | Third-party disclosure, terms/privacy breach. | Public/authorised corpus only; provider/data decision; minimal metadata; explicit AI egress and budget. | S02 decision; S07 evidence. | Disclosure boundary accepted; account, egress, budget and runtime evidence open. |
| `THR-S02-015` | External LLM receives confidential question or excessive evidence. | User/corpus data disclosure. | Explicit no-confidential-content notice; minimum passages; provider terms; bounded request. | S02/S05/S07. | Disclosure boundary accepted; user notice, account, egress and runtime evidence open. |
| `THR-S02-016` | Provider credential leaks to Git, client, logs or errors. | Account abuse and cost. | Secret store; server-only injection; scanning; redaction; least-privilege key; rotation procedure. | S04/S06/S08. | Open. |
| `THR-S02-017` | Anonymous query floods provider or exhausts budget. | Cost and availability loss. | Body/question/context limits; per-client/global rate/concurrency; deadlines; monetary circuit breaker. | S04/S07/S08. | Open. |
| `THR-S02-018` | Provider response or exception injects sensitive details into Problem Details/logs. | Secret, endpoint or data leakage. | Adapter classification; allowlisted public fields; generic details; no raw payload/stack. | S04/S07. | Open. |
| `THR-S02-019` | Model/document content produces XSS or unsafe citation URL. | Browser code execution/phishing. | Plain text; contextual encoding; catalogue-built HTTPS citation URL; CSP; XSS tests. | S05/S07. | Open. |
| `THR-S02-020` | Public caller invokes administration or normal startup mutates state. | Unauthorised corpus/index change. | No admin HTTP route; explicit one-shot mode; OS identity; enable flag; reason; least privilege. | S04/S07. | Open. |
| `THR-S02-021` | Local operator repeats an uncertain command after timeout/crash. | Duplicate snapshot/observation/build or ambiguous activation. | Operation ID; idempotent append/retry and records; explicit status; no implicit latest-observation selection; activation only after durable audit. | S03/S04/S07. | Open. |
| `THR-S02-022` | Log captures questions, passages, answers, paths or host identity. | Confidentiality/privacy leakage. | Structured allowlist logging; IDs/hashes/counts; sanitised placeholders; retention. | S04/S07/S08. | Open. |
| `THR-S02-023` | Malicious/transitive dependency or build script compromises artefact. | Supply-chain execution. | Lockfiles; no lifecycle scripts where possible; pinned trusted packages/actions; audit; SBOM candidate. | S01/S04/S06/S08. | Setup controls exist; new packages unverified. |
| `THR-S02-024` | SQLite/vector/content volume or observation journal is corrupted or lost. | Catalogue/index/freshness provenance loss and unavailable service. | Durable volume; integrity check across manifest, activation digests and journal; application-consistent backup; isolated restore; raw content re-open verification. | S03/S06/S07/S08. | Open. |
| `THR-S02-025` | Backup retains deleted or secret material indefinitely. | Data/secret exposure. | No secrets in stores; classified backup; explicit retention/deletion; encryption and access policy. | S06/S08. | Open. |
| `THR-S02-026` | OCI instance or metadata credentials are reachable from untrusted flow. | Cloud account compromise. | Metadata destination deny; unprivileged service; NSG; minimal instance permissions; SSRF tests. | S07/S08. | Open. |
| `THR-S02-027` | Readiness calls external services repeatedly or leaks diagnostics. | Cost, outage amplification or information disclosure. | No billable probe; local config/circuit state only; sanitised capability status. | S04/S07. | Open. |
| `THR-S02-028` | Evaluation thresholds are changed after a failing result. | False quality claim. | Pre-register dataset/threshold version; append-only change before new campaign. | S02/S07. | Design mitigated. |
| `THR-S02-029` | Trade-mark, licence or terms violation in a document/snapshot. | Legal/removal risk and release block. | Per-document rights verification, attribution and provenance record; activation gate. | S02/S08. | Open per document. |
| `THR-S02-030` | DB-Notifier concepts or dependencies enter the core. | Boundary erosion and independent-runtime failure. | Architecture tests; OpenAPI-only future boundary; no project reference. | S04/S06. | Setup tests exist; later code open. |
| `THR-S02-031` | A catalogue/source registration is poisoned with an unauthorised URL, licence claim or trust class. | SSRF, illegal content or false provenance. | Trusted local admin plane; closed fields; exact allowlist; independent validation; Candidate state; audit. | S03/S04/S07. | Open. |
| `THR-S02-032` | Name/category duplication creates several identities for Redis, SAP HANA or SingleStore. | Inconsistent retrieval, lifecycle or reporting. | Stable opaque database ID; unique initial seed; many-to-many assignment constraint; 51/54/9 fixture. | S03/S07. | Open. |
| `THR-S02-033` | One failed source makes aggregate coverage look complete. | User assumes unsupported evidence coverage. | Per-source status, degraded counts/IDs, citation provenance and no completeness claim. | S04/S05/S07. | Open. |
| `THR-S02-034` | Deactivation/removal or a withdrawal observation leaves stale vectors queryable, or cleanup removes historical bytes prematurely. | False answers or irrecoverable audit/rollback. | New generation for catalogue membership/lifecycle changes; new activation record for observation-only withdrawal; eligible-binding hard filter; last-document invariant, tombstone, reachability/retention and adversarial query. | S03/S04/S07. | Open. |
| `THR-S02-035` | Unbounded catalogue additions exhaust storage, queue, parser or provider budget. | Denial of service or unexpected cost. | Per-operation limits, quotas/budget circuit, capacity gate and activation refusal without truncation. | S03/S04/S07/S08. | Open. |
| `THR-S02-036` | Database/document activation races produce an active database without active evidence. | Broken invariant and misleading availability. | One lease/transaction, expected revision, complete binding validation and crash matrix. | S03/S04/S07. | Open. |

## Abuse cases

### Query abuse

- Maximum-sized Unicode and JSON payloads.
- Repeated expensive questions across the complete active catalogue.
- Invalid enum/unknown fields containing URLs or provider names.
- Questions requesting system prompts, secrets, files or administration.
- Questions designed to select a malicious high-scoring chunk.

Expected result: bounded rejection, rate limit, grounded response or explicit
insufficient evidence; no source fetch or authority change.

### Document abuse

- PDF bomb, malformed xref, extreme pages, embedded files and active actions.
- CSV with ambiguous encoding/dialect, extreme rows/columns/cells, formula-like
  payloads or malicious multiline quoting.
- Instruction text imitating system messages.
- Hidden/overlaid text and misleading page order.
- Content that cites an unauthorised external URL.

Expected result: parser/policy failure or untrusted bounded evidence; no active
content execution and no policy change.

### Network abuse

- Mixed public/private DNS answer, rebinding between validation and connect,
  IPv4-mapped IPv6, alternative numeric address syntax and redirect chain.
- Certificate chain missing an intermediate to provoke AIA download.
- Proxy or ambient credential discovery.

Expected result: `SourcePolicyViolation`, no alternate destination and a
sanitised audit record.

### Administration abuse

- Normal server invoked with administration arguments accidentally.
- Missing reason, repeated operation ID, stale record revision, concurrent
  mutation and audit-store failure.
- Duplicate database identity, invalid category assignment, unverified source,
  activation without a document and removal of the last active document.

Expected result: no mutation unless every precondition and the atomic boundary
succeeds; a deterministic status allows safe operator recovery.

## Security test catalogue

| Test group | Minimum evidence |
|---|---|
| `SEC-SRC-01` | Exact URI canonicalisation table, including IDNA, port, path and query negatives. |
| `SEC-SRC-02` | Controlled DNS mixed/rebinding cases and proof of connected approved IP plus Host/SNI. |
| `SEC-SRC-03` | Redirect, proxy, credential and certificate-download refusal. |
| `SEC-PDF-01` | Malformed, oversized, compressed and active-content fixture limits. |
| `SEC-CSV-01` | Encoding, delimiter, quoting, row/column/cell limits, formula non-execution and deterministic locator fixtures. |
| `SEC-RAG-01` | Prompt-injection corpus and citation-forgery rejection across `pt-BR`, `en-GB` and cross-language evidence. |
| `SEC-RAG-02` | Higher-scoring ineligible binding/wrong database filter/generation/corpus pre-filter proof. |
| `SEC-DATA-01` | Content-store traversal, overwrite, symlink/reparse and deletion refusal, including reachability from retained generations and observation history. |
| `SEC-ACT-01` | Canonical `sourceBindingSetDigest`/`activationBindingSetDigest` vectors; observation registration/snapshot mismatch; exact `304` field transition; idempotent retry; new-record rollback; crash/concurrency around observation append, manifest, digest, audit and activation writes. |
| `SEC-CAT-01` | Exact 51/54/9 seed, many-to-many uniqueness, lifecycle, tombstone and last-active-document invariant. |
| `SEC-COV-01` | Unified retrieval, observation-only withdrawal/`304` eligibility, per-source degradation, explicit coverage and no silent provenance substitution. |
| `SEC-API-01` | Payload bounds, exact `questionLanguage` enum, unknown fields, CORS, rate, cancellation and sanitised errors. |
| `SEC-UI-01` | XSS through question, evidence, answer, citation and Problem Details; localisation completeness in `pt-BR` and `en-GB`; independence between interface and query languages; and contrast, focus and state visibility in `Light` and `Dark`. |
| `SEC-ADM-01` | OS identity, enable flag, reason, idempotency, lease and no HTTP route. |
| `SEC-OCI-01` | NSG, metadata denial, unprivileged process, secret injection and restore isolation. |

## Risk acceptance boundaries

Explicit owner acceptance of ADR-0005 and ADR-0006 settled these architecture
boundaries on 2026-08-01:

- bounded disclosure of authorised corpus chunks and user questions to the
  selected OpenAI embedding and language-model candidates;
- the documented public provider retention/training/residency posture, subject
  to future account and contract verification;
- local-only TLS validation with certificate downloads and online revocation
  disabled, including its residual revocation risk;
- the conditional OCI region, exposure, backup-retention and cost direction
  recorded by ADR-0005.

The following remain separate evidence or authority requirements and were not
resolved merely by accepting those boundaries:

- account entitlement, spend limits, effective provider terms and enabled AI
  egress;
- implementation and clean local/OCI evidence for zero lateral TLS traffic;
- each actual official-source licence/terms, synchronisation frequency and
  network authority;
- tenancy-specific OCI capacity, billing, IAM, backup consistency and restore
  evidence;
- disposition of any later P0/P1 residual finding.

P2/P3 findings may be accepted only with owner, expiry/review date, scope and
compensating control recorded in the state report. No risk acceptance enables
an external action by itself or proves a control effective.

## Review and maintenance

- Revisit this model when an ADR changes a source, provider, persistence,
  public contract, administration surface or deployment boundary.
- Link implementation tests to threat and security-test IDs.
- Keep observed evidence in the owning state report rather than rewriting a
  documented control as already effective.
- Preserve historical residual decisions append-only.
