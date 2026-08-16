# Security, Identity and Access

## Principles

- Least privilege and deny by default.
- Incomplete configuration disables the capability.
- Secrets remain external to the repository and index.
- Retrieved content never has authority.
- Input, cost, time, memory, network and per-operation/batch cardinality are
  bounded; the catalogue has no numerical product ceiling.
- Every sensitive change records actor, target, reason, time and outcome.
- Logs and evidence are sanitised.

## Data classification

| Data | Initial classification | Rule |
|---|---|---|
| Publishable corpus | Public or authorised internal | Licence and provenance are mandatory. |
| Original material in `reference-materials/` | Local-only | Do not version or use in public runtime. |
| User question | Untrusted; potentially confidential | Do not persist/log in full by default. |
| Passage and answer | Derived from the corpus | Bound, cite and apply retention. |
| Raw bytes and snapshots | Same classification as origin | Durable immutable content store outside Git with referential retention. |
| Page PNGs and render manifests | Derived with the same classification as origin | Durable immutable content store outside Git/Git LFS; serve only through a validated active binding. |
| `DerivativeObligationSetV1` and notice panel | Control and derived text with the same classification as origin | Immutable record bound to source, mapping, manifest and activation; untrusted text that is never executable. |
| Embedding/index | Derived data | Protect like the source corpus. |
| Provider budget envelope, reservations and usage ledger | Protected internal control plane | Durable, revisioned and sanitised; never contains secrets, questions, passages, answers or provider payloads. |
| Renderer sandbox profile and attestation | Protected internal control plane | Bind exact profile, worker, asset and capability digests without host paths, user/device identity or security-token details. |
| API key/token | Secret | Secret store; never log or persist in plaintext. |
| Telemetry | Sanitised internal | Minimisation and retention. |

Do not use real customer data or personal information without a purpose,
basis, authority and specific controls.

## Trust boundaries

- User/browser ↔ Dashboard.
- Dashboard ↔ API.
- API ↔ Application.
- Application ↔ document parser.
- Application ↔ embedding provider.
- Application ↔ persistent provider-budget control plane.
- Application ↔ vector store.
- Application ↔ document content store.
- Application ↔ isolated and bounded PDF renderer.
- PDF renderer worker ↔ host operating-system sandbox boundary.
- Application ↔ language model.
- Application ↔ catalogue/persistence.
- Every registered external official source ↔ governed synchroniser.
- CI/deployment ↔ GitHub and OCI.

## MVP access model

The Challenge materials permit open queries. Therefore:

- the question route may be anonymous, with limits and abuse protection;
- database/document/source administration, ingestion, activation, rollback and
  configuration are not anonymous public operations;
- accepted ADR-0006 selected a non-public local administrative surface in the
  main host's one-shot mode; it uses operating-system identity, minimum
  permissions, a mandatory reason, idempotency and sanitised audit, and is
  implemented in the main host under explicit fail-closed configuration;
- start-up only verifies and loads the active generation; mutation requires an
  explicitly configured and invoked one-shot administrative mode;
- official synchronisation uses the same administrative surface; public query
  resolves the active manifest but never starts a download;
- secrets are not sent to the browser;
- authorisation remains server-side even when the UI hides a function;
- the MVP implements neither user management nor multi-tenancy.

## Future permission model

| Role | Permissions |
|---|---|
| `Reader` | Query authorised corpora and view citations. |
| `Curator` | Manage documents and versions for assigned corpora. |
| `Operator` | Run synchronisation, activation and rollback. |
| `SecurityAdministrator` | Manage policies, external sources and audit. |
| `PlatformAdministrator` | Manage providers, configuration and environments. |

Future scopes: environment and corpus. Access filters are applied before
retrieval, never only after generation.

## Secrets and configuration

- Development: user-secrets, an approved local vault or protected variables.
- CI: environment secrets with least privilege and no exposure in forks.
- OCI: secret manager or approved environment mechanism.
- Persist only an opaque reference, variable name, provider and rotation
  metadata.
- Supply examples without real values.
- Do not print complete configuration or the entire environment.
- Rotate exposed material immediately and preserve sanitised evidence.

## Product provider credential boundary

`OPENAI_API_KEY` identifies a product-runtime secret only. Its value must never
be received, inherited, read, validated, copied or transported by Stage 0,
Stage 1, Stage 2, any agent or subagent, the orchestrator, CodexRunner, CI,
ordinary tests or development tooling. Plans, prompts, task envelopes,
arguments, files, commits, logs, evidence and artefacts must contain neither
the value nor secret-shaped substitutes. Agent and governed child-process
environments are closed and exclude the product credential.

The identifier itself may occur only in approved product runtime or deployment
configuration, this security policy, executable enforcement that prevents the
credential from entering development processes, synthetic enforcement tests,
and immutable historical records already protected by an exact digest or
append-only identity. Every permitted current path is an exact, classified
allowance in the repository policy; wildcards, directory allowances and
implicit exceptions are prohibited. An allowance neither permits a secret
value nor grants credential lookup, provider egress or operational authority.

The canonical CI entry point may name the identifier solely as executable
enforcement of this policy: it removes that exact process-scope variable,
without reading it, before invoking policy code or any child process. This
exception cannot be used to look up, validate, copy, log or forward a value.

Only an approved secret store, service manager or operational environment may
inject the value directly into the product process. Administrative index
embedding, query embedding and grounded generation each require their own
bounded, operation-specific `AUTH-*` request reference. Requested references
are non-secret evidence only and never create trusted grants. A deny-all
in-memory grant validator receives independently supplied, explicitly approved
operational grants and must bind the exact operation/reference pair immediately
before credential lookup or provider egress. Missing, untrusted or
cross-operation grants stop with `HUMAN_DECISION_REQUIRED`. Grants are not
persisted and product defaults never derive them from requested configuration.
Code completion, deployment, product activation and credential availability do
not grant operational authority. Readiness and visual-evidence serving must not
read the credential. Synthetic enforcement tests use injected readers, grant
maps and fake handlers only; real provider calls require separate explicit
authority.

## Persistent provider budget admission

[Accepted ADR-0018](../../docs/architecture/ADR-0018-Persistent-Provider-Budget-Admission-And-Explicit-Rearming.md)
establishes the internal `ProviderBudgetEnvelopeV1` architecture. Acceptance
arms no budget, selects no price or account fact and authorises no provider
call. No operational envelope or ledger is implemented; the effective
aggregate limit and all operation allocations therefore remain zero, and
provider capability remains `Disarmed`.

The durable closed states are `Disarmed`, `Armed`, `Tripped`, `Exhausted`,
`ReconciliationRequired` and `Expired`. Absence, unreadability, corruption,
incompatible version, scope mismatch, expired authority or an unknown state is
equivalent to `Disarmed` and creates no replacement record. No state returns to
`Armed` automatically.

Any future provider attempt must satisfy all of these controls before
credential lookup or network egress:

- the exact environment, provider, non-secret billing scope, model, currency,
  immutable cost-schedule identity and SHA-256 are bound to one durable
  versioned envelope;
- the aggregate authorised limit and the strict allocations for
  `AdministrativeIndexEmbedding`, `QueryEmbedding` and
  `GroundedGeneration` use non-negative integer accounting units; missing,
  drifting, incompatible or overflowing cost data fails closed;
- one stable opaque `providerRequestId` binds the exact operation authority,
  request plan and conservative maximum charge;
- one serialisable or compare-and-swap control-plane transaction verifies the
  armed state, runtime-session identity, revision, expiry and remaining
  aggregate/operation amounts, then persists a unique idempotent reservation;
- durable readback of both reservation and envelope succeeds; and
- the independently trusted operation-specific grant is revalidated
  immediately before credential lookup and egress.

A process-local counter, lock, cache or provider dashboard is not admission
evidence. Concurrent instances share the durable envelope and cannot reserve
against stale capacity. The same request ID with divergent authority, request,
schedule or maximum-charge identity trips the envelope without egress.

After an observed response, a second durable transaction commits the calculated
charge within the reservation and records bounded usage evidence. Usage or
charge above the admitted maximum trips the envelope, commits the full maximum
reservation and stops further egress.

Only a locally proven pre-send failure may release a reservation through a
durable audited transition. Timeout or cancellation after send, connection
loss after possible acceptance, response-parse failure, process crash during
an attempt or missing completion readback commits the complete maximum amount,
sets `ReconciliationRequired` and prohibits retry. Historical admission is not
rewritten and capacity is not restored without separately authorised evidence.

`Armed` is valid only for the exact runtime-session identity in the latest
durable rearm record. Start-up, restart, elapsed time, credential availability,
provider recovery or readiness never rearms automatically. Rearming requires
an explicit local administrative authority and durable readback; it cannot
increase a limit, move an allocation, change provider/model/schedule, extend
expiry, reset committed use, release a reservation or resolve an indeterminate
attempt. `Exhausted` requires a new budget-envelope authority rather than a
rearm.

Liveness is independent from the ledger. Readiness performs no provider call
and exposes only a sanitised state from the closed set `Disarmed`, `Armed`,
`Tripped`, `Exhausted`, `ReconciliationRequired` or `Expired`; it exposes no
limit, remaining amount, account scope, schedule, actor or request identity.

## RAG threats

### Prompt injection in documents

- Delimit evidence as untrusted data.
- Instruct the model to ignore commands in content.
- Provide no tools to the model in the MVP.
- Validate that citations belong to the retrieved set.
- Include malicious cases in evaluation.

### Hallucination and false citation

- Restrict the answer to evidence.
- Require a citation for each material factual claim.
- Verify IDs and location before returning.
- Use an explicit insufficient-evidence outcome.

### Exfiltration and leakage

- Do not mix a corpus, generation or item outside the active manifest.
- Send no more context than necessary.
- Do not log full prompts, passages or answers by default.
- Treat embeddings as potentially sensitive derivatives.

### Poisoning and document supply chain

- Identify origin, hash, version, licence and ingestion actor.
- Build an inactive generation and validate before activation.
- Preserve rollback.
- Do not ingest unapproved material automatically.
- Treat administrative database, category, document and source records as a
  trusted control plane; every change requires actor, reason, validation,
  candidate and activation.

## Files and parsing

- Allowlist formats and media types.
- Validate signature/structure, not only extension.
- Per-operation limits for size, pages, rows, columns, cells, text, chunks and
  time, without a product ceiling for catalogue cardinality.
- Canonicalise paths and prevent traversal/symlink escape.
- Do not execute macros, scripts, attachments, links or CSV formulae.
- Isolate the parser when risk or library requires it.
- Update dependencies and respond to vulnerabilities.

Public upload remains outside the MVP.

## Rendering and visual evidence

- PDF is untrusted input to the renderer; bound bytes, pages, time, memory,
  dimensions, concurrency and total per-operation quantity.
- `pdf-page-png-v1` removes metadata capable of exposing path, host or command
  and produces only opaque RGB PNG within accepted limits.
- Implemented `pdf-page-png-notice-v1` preserves the page region pixel for
  pixel and adds a separate visible panel below it with the complete
  `DerivativeObligationSetV1`. It neither alters nor covers source pixels and
  does not reinterpret legacy manifests.
- The immutable obligation set binds source, mapping revision, evidence,
  attribution, copyright/permission notices, disclaimers, trademark treatment
  and change marking. The renderer receives only this verified record: it does
  not extract PDF terms, access the network, translate, complete or reduce text.
- Deterministic font, glyphs, layout, dimensions and region/composite PNG hashes
  are validated. Absence, divergence, truncation, unapproved font, out-of-bounds
  panel or pixel alteration fails closed.
- Recalculate hashes, validate PNG signature, dimensions, page count,
  consecutive numbering and canonical manifest; reopen source and every object
  before finalising the candidate.
- `IDocumentContentStore` is the sole product binary authority for persistent
  sources and PNGs. Git, Git LFS, quarantine, catalogue and vector store prove
  neither durability nor readback.
- Every new activation revision persists the exact binding among document,
  source object, complete rights snapshot, generation and PDF render manifest.
  Historical revisions without that set receive no backfill and fail closed
  for visual readiness/query.
- An image may be served only when a validated citation references the same
  document version, page, active generation and finalised render manifest. A
  `Deactivated` or `Removed` document never serves an image.
- For the notice-bearing profile, `200` and `304` also revalidate mapping
  revision, obligation set, region measurements and composite hash. ETag uses
  the composite PNG SHA-256; changing an obligation requires new identities.
- The v2 contract embeds no bytes and exposes no path. The same-origin
  read-only endpoint revalidates binding, bounds the body, uses immutable ETag,
  `X-Content-Type-Options: nosniff`, suitable cache policy and authorisation
  equivalent to textual evidence.
- Adjacent textual evidence remains accessible; a PNG is never the sole bearer
  of a claim or navigation meaning.
- Complete obligations also appear as escaped, selectable text associated with
  the figure. They are not hidden in `alt`, metadata or a link and accept no
  source-created HTML, Markdown or URL.
- The LLM receives text only. Sending an image or derivative to a provider
  requires its own egress, classification, retention, residency and cost
  authority.
- The right to read, index or cite does not imply rendering, creating/retaining
  a derivative, displaying or distributing. Ambiguity in any applicable right
  blocks visual activation.
- Reachability, backup and cold restore jointly protect and verify source,
  composite PNG, obligation set, mapping, manifest, activation and answer
  evidence. Readiness and serving fail closed on absence or divergence.
- The protected v2 contract revision, schema/migrations and local profile
  behaviour are implemented under separate authorities. No field, backfill or
  compatibility is inferred from legacy rows; notice-bearing AQG, new A0 and
  operational homologation remain separate.

### Operating-system sandbox target

[Accepted ADR-0019](../../docs/architecture/ADR-0019-Cross-Platform-PDF-Renderer-Sandbox-Boundary.md)
establishes `pdf-render-sandbox-v1` as an internal security profile independent
from the pixel-affecting render profiles. Acceptance does not implement the
profile or prove a platform. Current Windows Job Object and Linux
`rlimit`/non-dumpable containment remain useful but are not a complete
operating-system sandbox.

The target boundary requires:

- a dedicated minimal renderer worker with no API-host composition, provider,
  administration, catalogue, persistence, HTTP or Dashboard capability;
- a fresh sandbox per document or explicitly bounded page selection, created
  and authoritatively attested by the parent before any untrusted PDF byte is
  delivered;
- no provider credential, ambient environment, user profile, secret store,
  network capability (including loopback), product store, repository,
  catalogue, arbitrary filesystem, registry, device, named pipe, inherited
  handle or child-process escape;
- read-only access only to exact worker/runtime/renderer/font assets, one
  private bounded scratch area and the pre-created bounded standard-I/O
  protocol handles;
- parent-owned limits and complete-tree termination for CPU, memory, process
  count, handles/file descriptors, output, dimensions, pages and elapsed time;
  no core dump or debugger attachment; verified task-owned scratch clean-up
  that preserves rather than broadly removes an uncertain path; and
- parent verification of profile, platform/RID, worker, asset and capability
  digests before input, followed by independent validation of every returned
  descriptor and PNG.

On Windows the worker must be created suspended, assigned to a fully configured
Job Object before its first instruction and run in an AppContainer or
equivalently proven restricted identity with no network capability, unrelated
filesystem/registry/device access or unapproved handle. On Linux ARM64 a
trusted minimal launcher must install an unprivileged user/mount/PID/network
namespace boundary, drop all capabilities, set `no_new_privs`, apply a reviewed
seccomp allowlist, cgroup v2 and supporting `rlimit` controls before renderer
code receives control.

Missing or partially effective primitives make visual rendering unavailable.
The system never falls back to the current worker, in-process rendering, a
weaker sandbox or a generic OCI boundary. Windows evidence does not prove
Linux ARM64, static cross-publish does not prove native execution and sandbox
success does not replace rights, manifest, activation or output validation.

## Persistent answer evidence

- Implemented `AnswerEvidenceRecordV1` is an internal persistence contract,
  not conversation history, analytics, an endpoint or a public v1 field.
- Only `Answered`, after complete validation and before response, creates a
  record; commit/readback failure prevents public success.
- Persist only identities/digests, answer hash/length, non-secret descriptors
  and exact citation, source, manifest and page bindings. Do not persist the
  question or its hash, answer, excerpt/URL, prompt, provider payload,
  score/vector, user identity/IP, secret, path or bytes.
- Apply `answer-evidence-p30d-v1`: `expiresAt = createdAt + P30D`, without
  refresh on read, replay or inspection.
- During retention, bound source and PNGs remain reachable. Expiry deletes
  nothing; `cleanup-plan-v1` reserves and revalidates all roots before physical
  removal, including under concurrency.
- Header, citations, pages and sanitised audit are atomic. Same ID/digest is a
  replay; same ID with divergent content is a conflict without mutation.
- ADR-0010 architectural authority did not itself implement the increment;
  later `S04-CORR-04-E` authority implemented the contract and migration
  locally without changing OpenAPI v1, v2 or serving and without running a gate.

## Egress policies

Egress is divided into four independent policies.

### `AI_PROVIDER_EGRESS`

Remains deny by default. Accepted ADR-0005 selected conditional external
providers and a disclosure boundary but authorised neither egress, account use
nor execution. A future local provider may leave this policy with no external
destinations. For an accepted external provider:

- only explicitly allowlisted endpoints and ports;
- documented review of retention, training use, residency, terms, permitted
  data classification and opt-out mechanism;
- data minimisation and an environment-appropriate user notice;
- corpus chunks and a minimised/normalised question may leave only for the
  selected embedding provider, respectively for indexing and query embedding;
- question and retrieved passages may leave only for the selected language
  model;
- throughout indexing, the embedding provider may cumulatively receive all
  authorised content in bounded chunks; this is corpus disclosure and requires
  corresponding classification and authority;
- the language model receives only the question and minimum retrieved evidence,
  never the complete corpus;
- secrets, local paths, unnecessary metadata and a complete file in one request
  are not sent;
- timeout, cancellation, token/byte limit, budget and sanitised audit are
  mandatory; and
- credential lookup and every attempt additionally require the durable,
  read-back reservation and matching operation-specific grant defined by the
  persistent provider-budget boundary.

Configuring a credential or provider alone does not grant egress authority.

### `VECTOR_STORE_EGRESS`

Remains destination-free when the vector store is local. If `STATE-02` selects
a managed service:

- endpoints, ports and TLS are allowlisted and validated;
- embeddings, chunks and metadata receive the source corpus classification and
  protection;
- residency, retention, backup, deletion, secondary use and tenant isolation
  are reviewed;
- credentials have least privilege and remain outside index, logs and frontend;
- timeouts, cancellation, limits, sanitised audit and unavailability procedure
  are mandatory.

Configuring a managed adapter grants neither `VECTOR_STORE_EGRESS` nor
`OCI_RUNTIME_EGRESS` authority.

### `OFFICIAL_SOURCE_EGRESS`

Part of the MVP but remains deny by default. A source can be enabled in the
synchronisation profile only after approval of its record, canonical URL,
licence/terms, `maxAge`, allowlist and limits, and after network execution
receives specific authority. Adding a record does not enable egress; neither
does a public question. This policy is distinct from AI-provider calls.

### `OCI_RUNTIME_EGRESS`

OCI runtime uses a separate allowlist containing only individually authorised
destinations for AI provider, official sources, secret store, vector store,
telemetry or selected operational services. An official URL must be permitted
by both `OFFICIAL_SOURCE_EGRESS` and `OCI_RUNTIME_EGRESS`; a managed vector
store requires both `VECTOR_STORE_EGRESS` and `OCI_RUNTIME_EGRESS`. One policy
does not broaden another. Generic internet access, metadata service and
unauthorised private destinations remain blocked. The policy is validated in
the target environment.

## Network and MVP official source

- HTTPS is mandatory.
- Public canonicalised URL without userinfo, fragment, token, signature or
  query credential; exact scheme, normalised IDN host, port, path and query for
  every official PDF/CSV are allowlisted.
- The adapter sends no `Authorization`, API key, client certificate,
  pre-authentication or environment credential; an authenticated source is
  outside the MVP because canonical URL is public citation metadata.
- A/AAAA records are resolved and authorised for each new physical connection;
  reject the entire set when any response points to loopback, link-local,
  unauthorised private network, metadata service or prohibited destination.
- The socket connects only to an approved `IPEndPoint`, preserving the original
  hostname for Host, SNI and certificate validation without a new hostname
  resolution during connection.
- Certificate validation cannot create AIA, CRL, OCSP or other out-of-policy
  egress. Accepted ADR-0006 selected local trust with chain downloads and
  online revocation disabled and accepted residual revocation risk; every
  auxiliary destination still requires its own allowlist, decision and
  authority. Missing configuration fails closed, and the selected policy must
  still be implemented and proved in a clean local clone and OCI without
  silent weakening.
- Redirects are disabled in the MVP. Future enablement requires a new decision,
  allowlist and complete per-hop validation/pinning.
- No proxy, cookies or environment credentials by default.
- Bound timeout, transferred/decompressed bytes, PDF/CSV media type/structure,
  pages/rows/columns/cells, rate and concurrency.
- ETag/Last-Modified do not replace hashing and validation.
- Respect licence, terms, robots and permitted frequency.
- Synchronise to a governed snapshot; do not browse during a question.
- Raw snapshots and derivatives remain outside Git when redistribution is not
  licensed.
- Standard tests use a local fake server. A real-URL test is opt-in, sanitised
  and requires its own network authority.

## API and abuse

- Validate and bound question and payload size.
- Reject client-supplied URL, host, path, provider, adapter or
  catalogue-authority field.
- Rate-limit by an environment-appropriate origin/key.
- Flow timeout and cancellation.
- Limit top-k, tokens and external calls.
- Restrict CORS to the authorised frontend.
- TLS in the public environment.
- Problem Details without stack trace or sensitive data.
- OpenAPI v1 and its language enums remain unchanged; no additional BCP 47
  value or image reference is accepted/emitted through coercion.
- Cheap liveness independent of external services.
- Sanitised readiness distinguishes core, query dependencies and
  per-source/document coverage. A stale/unavailable item degrades coverage and
  does not fail globally when another active database/document remains servable.

## Dashboard and untrusted output

- Render answers and citations as plain text by default.
- If Markdown is necessary, accept only a sanitised subset.
- Block raw HTML, handlers, scripts, active styles and dangerous URLs.
- Permit only approved URL schemes, with no executable citation link derived
  directly from the model.
- Apply Content Security Policy and contextual encoding.
- For visual evidence, accept only the server-created same-origin reference
  with known size/MIME; never construct `src` from model text, language tag,
  document URL or path.
- Preserve original citation text and an accessible alternative alongside the
  displayed page; do not translate source-derived content.
- For `pdf-page-png-notice-v1`, present complete
  `DerivativeObligationPresentationV1` as escaped text alongside the figure and
  verify that its `obligationSetId` matches every citation image. Validation
  failure blocks the image, not the textual citation.
- Test stored/reflected XSS in document, question, answer, error and citation
  metadata.

## Logging and observability

May record:

- correlation ID and operation ID;
- a stable anonymous actor only when necessary and lawful;
- corpus/document/index IDs;
- provider/model version;
- duration, count, status and error code;
- hash or size, never a secret or full content;
- canonical BCP 47 tag, render profile, image count/dimensions and hashes,
  without bytes or full text;
- answer-evidence ID, corpus/activation/generation IDs/digests, counts,
  duration, expiry and sanitised retention/cleanup outcome;
- provider budget envelope/revision, operation class, authority reference,
  request digest, maximum reservation, state transition and sanitised outcome;
  and
- renderer sandbox profile/platform, worker/capability digests, policy
  revision, bounded resource counts, exit class and typed failure.

Must not record:

- API keys, tokens or authorisation headers;
- full prompts and answers by default;
- question, question hash or answer hash by default;
- complete document text;
- source/image bytes and raw renderer metadata;
- provider payload, budget account scope, public cost details or renderer
  security-token/kernel-object details;
- absolute paths containing a user/host name;
- stack trace or payload in a public response.

## Audit

Minimum events:

- sanitised configuration change;
- ingestion start, completion and failure;
- generation creation and activation;
- content import/reopen, render-manifest finalisation/rejection and visual
  serving refused by binding or lifecycle;
- atomic `CorpusActivationRecord` replacement, including the binding set;
- answer-evidence creation/replay/conflict and post-expiry cleanup, with only
  IDs, counts and sanitised outcomes;
- rollback;
- database, category, document, version or source change;
- provider/model change;
- provider-budget reservation, commitment, indeterminate outcome, trip,
  reconciliation, rearm, expiry and closure;
- renderer sandbox setup, attestation refusal, resource limit, escape attempt,
  crash and incomplete clean-up;
- official-source synchronisation, revalidation, staleness, withdrawal and
  failure;
- MVP administrative access and, later, RBAC/user management.

The record must be searchable, protected from unauthorised modification and
retained by policy. In the anonymous MVP, ordinary queries produce aggregate
metrics, not a named audit trail.

## Error handling

- Expected failures return typed states.
- Provider exceptions are sanitised at the boundary.
- Retry uses backoff/jitter only for transient idempotent failures.
- Rate limits and budgets are not bypassed.
- An uncertain provider outcome commits its admitted maximum, requires
  reconciliation and is not retried or automatically rearmed.
- An unavailable or incomplete renderer sandbox returns a typed failure and
  never selects a weaker execution path.
- A new-indexing failure does not deactivate the prior generation.
- Ambiguous state returns `Unavailable` or `Failed`, never success.
- P0/P1 incidents block progression until treatment or a formal decision.

## CI/CD security

- Minimum permissions.
- Pinned Actions and toolchains.
- Checkout without persisted credentials.
- Mandatory lockfiles.
- Dependency and secret scanning.
- Identifiable artefacts without secrets.
- Deployment only through an authorised environment/gate.
- Secrets unavailable to external pull requests.
- Standard tests do not access the real official source; opt-in jobs require
  their own environment, egress and authority.
- Logs and evidence sanitised before retention.

## Checklist

- Threat model and trust boundaries updated.
- Corpus licence/provenance verified.
- Per-document source-retention rights and, for visual PDF, intended rendering,
  derivative creation/retention, display and distribution rights verified.
- Secret and dependency scans passed.
- Files and payloads bounded.
- Prompt injection and false citation tested.
- Rate limit, timeout and cancellation exercised.
- Logs contain no sensitive data.
- Generation rollback verified.
- Source/PNG content, render manifest, reachability, backup/restore and
  fail-closed visual serving verified at every applicable boundary; local
  evidence replaces neither AQG nor operational recovery.
- For the notice-bearing profile, immutable obligation, page-region fidelity,
  complete panel, composite ETag, accessible presentation and backup/cold
  restore bindings verified; absence or truncation fails closed.
- `pdf-render-sandbox-v1` is installed and authoritatively attested before
  input, denies network and unrelated host resources, has no weaker fallback
  and is proved independently on Windows and native Linux ARM64.
- `AnswerEvidenceRecordV1` remains `Answered`-only, atomic and minimised,
  expires in `P30D` without refresh and protects reachability from cleanup
  races; local evidence replaces neither a gate nor operational validation.
- `AI_PROVIDER_EGRESS` is local or has explicitly authorised and tested
  provider, classification and endpoints.
- Provider budget absence/corruption is `Disarmed`; concurrent reservations,
  crash/restart, replay/conflict, indeterminate outcomes and explicit rearming
  are proved against the durable envelope before any nonzero authority.
- `VECTOR_STORE_EGRESS` remains empty for a local adapter or has explicitly
  approved endpoint, classification, residency, retention and credential.
- `OFFICIAL_SOURCE_EGRESS` remains deny by default and, when authorised, is
  restricted to exact URLs of approved active records.
- Manifest integrity, provenance, freshness, partial synchronisation failure
  and absence of silent fallback tested.
- `OCI_RUNTIME_EGRESS` has a minimal allowlist validated at the target.
- DNS rebinding, mixed DNS response, IP pinning, Host/SNI, blocked redirects,
  absence of authentication and AIA/CRL/OCSP egress tested with a controlled
  server.
- Dashboard output encoded/sanitised and tested against XSS.
- Language tags are bounded BCP 47 values, select no resource/provider and
  preserve the exact declaration; `en` is not inferred as `en-GB`.
- Negative permissions tested when introduced.
