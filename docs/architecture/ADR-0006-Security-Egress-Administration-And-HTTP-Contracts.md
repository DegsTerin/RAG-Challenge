# ADR-0006 — Security, Egress, Administration and HTTP Contracts

- Status: accepted
- Date: 2026-07-31
- Accepted: 2026-08-01
- Decision authority: explicit product-owner acceptance on baseline
  `main@39e2f803bf73cb4e2b59e56a0596e2858a3aed51`, corpus `4.7.0`
- Owners: RAG-Challenge security, API and operations
- State: `STATE-02 ARCHITECTURE`
- Dependencies: ADR-0002, ADR-0004 and ADR-0005
- Verification status: public primary-source verification completed for the
  first PostgreSQL source candidate, AI API contracts/data controls and OCI
  regional endpoints; every later source requires its own evidence, profiles
  remain disabled and account/runtime evidence remains pending

## Purpose and authority

This ADR defines the fail-closed security profiles, local administration
surface and public HTTP compatibility policy for the MVP. Acceptance does not
enable egress, create credentials, expose administration or call a
source/provider.

## Context

The application has four independent outbound purposes and a public unified
query capability over an administrator-managed catalogue. It must prevent a
question or retrieved document from changing policy, catalogue, source,
provider or administration. It also needs a stable OpenAPI v1 contract without
exposing Domain, persistence or provider types.

## Owner-decided query-language constraint

On 2026-08-01, the owner explicitly required question and answer support for
Brazilian Portuguese (`pt-BR`) and British English (`en-GB`). Each request
declares the question language, the answer uses that same language, and
source-derived citation text remains in the source language. Tests cover
same-language and both cross-language directions. This decision does not
select the Dashboard language and did not at that time accept the remaining
decisions in this ADR.
A later, separate owner decision selected `pt-BR` and `en-GB` as the supported
Dashboard languages; the HTTP query contract still does not choose between
them.
A further independent owner decision selected `Light` and `Dark` as the
supported Dashboard themes; the HTTP query contract does not choose a theme.

## Decision

The accepted decision is:

### Egress profiles

Every profile is deny by default and independently evaluated. An allowed host
in one profile grants no access through another.

#### `AI_PROVIDER_EGRESS`

- Candidate destination: exact DNS host `api.openai.com`, port `443`, HTTPS
  only.
- Permit only `POST /v1/embeddings` and `POST /v1/responses`. The official
  model pages and API references verify both routes for the selected models.
  No batch, chat-completions, files, tools or model-list path is required by
  the MVP.
- The candidate host was not contacted. Its authority and paths were observed
  only as text in documentation retrieved from `developers.openai.com`; this
  verifies the accepted allowlist but does not enable it.
- Reject userinfo, non-default ports, redirects, proxies and ambient
  credentials.
- Send only bounded authorised chunks for indexing, the bounded question for
  query embedding, and the bounded question plus selected evidence for answer
  generation.
- Apply connect and total timeouts, cancellation, request-size/token budgets,
  concurrency limits and a monetary circuit breaker.
- Set Responses `store=false`; do not send conversation,
  `previous_response_id`, background mode, file input or hosted-tool
  configuration. Use no training-data opt-in.
- Assume up to 30 days of abuse-monitoring retention unless the owner later
  obtains and authorises Modified Abuse Monitoring or Zero Data Retention.
  Brazil is not a documented data-residency region for these services. A
  regional OpenAI endpoint would be a different exact authority and requires
  an amended decision and allowlist; it cannot be substituted silently.

#### `VECTOR_STORE_EGRESS`

- Configure no destination for the MVP because the selected vector adapter is
  local.
- Treat any configured destination as `ConfigurationInvalid`.

#### `OFFICIAL_SOURCE_EGRESS`

- Candidate destinations: the exact active canonical URIs held by approved
  `OfficialSourceRegistration` records. PostgreSQL is the first verified
  candidate; it does not authorise any other URI.
- Enable only in the explicit administration synchronisation profile.
- Apply the complete SSRF, DNS/IP, TLS, redirect, content and rate policy
  below. Query and normal startup profiles cannot resolve or connect to the
  source.
- The first PostgreSQL URI returned `200` to `HEAD` and `206` to the authorised 64 KiB
  range request without a redirect on 2026-07-31. This verifies the candidate
  authority, path, media type, byte-range behaviour, current size and leading
  signature only; it does not enable the profile or prove the complete
  application network policy.
- `robots.txt` does not disallow the exact documentation path. Direct TLS
  negotiated TLS 1.3 and an offline four-element chain validated using local
  trust with certificate downloads disabled and revocation `NoCheck`. This
  supports the accepted no-lateral-download policy while retaining its
  explicit offline-revocation and future-clean-environment risks.

#### `OCI_RUNTIME_EGRESS`

- Compose only destinations already authorised for AI, official sources,
  secret retrieval, certificate operation and sanitised telemetry.
- Do not include a generic `0.0.0.0/0` or `::/0` application egress rule as a
  substitute for destination policy.
- Keep metadata-service, private, link-local and loopback destinations denied
  from all untrusted URL flows.
- The official API index publishes Core Services
  `https://iaas.sa-saopaulo-1.oraclecloud.com`, Key Management
  `https://kms.sa-saopaulo-1.oraclecloud.com`, Secret Management
  `https://vaults.sa-saopaulo-1.oci.oraclecloud.com` and Secret Retrieval
  `https://secrets.vaults.sa-saopaulo-1.oci.oraclecloud.com` for the candidate
  region. Those strings were read from documentation only; no endpoint was
  contacted.
- Normal application runtime permits only Secret Retrieval when configured:
  exact host `secrets.vaults.sa-saopaulo-1.oci.oraclecloud.com`, HTTPS port
  `443`, `GET /20190301/secretbundles/{configuredSecretId}` and bounded query
  keys for a configured version/stage. The host and secret ID are trusted
  configuration, never public input.
- Core, Key Management and Secret Management endpoints belong only to
  separately authorised provisioning or administration. They are not normal
  application-runtime egress. All four destinations remain disabled until an
  accepted ADR, exact IAM design and separate deployment authority exist.

### Official-source network policy

- Resolve a trusted source-registration ID, then canonicalise once and compare
  scheme, IDNA ASCII host, port, path and query against that record's exact
  URI. Public input never supplies or mutates a registration.
- Resolve A and AAAA for each new physical connection. Reject the whole answer
  atomically if any address is loopback, link-local, private, multicast,
  unspecified, documentation-only, metadata-service or otherwise prohibited.
- Select only from the approved resolved set and connect through an explicit
  `IPEndPoint`; preserve the original hostname for HTTP `Host`, TLS SNI and
  certificate name validation.
- Do not perform a second hostname resolution inside the connection path.
- Disable automatic redirects, proxy discovery, cookies, authentication and
  connection coalescing to a different authority.
- Use operating-system trusted roots with certificate downloads disabled and
  online revocation disabled. Validate chain, validity period, EKU and host
  name locally. This accepts the residual risk that a newly revoked
  certificate is not learned until trusted material is updated.
- Provision trust-store and base-image updates through a separately governed
  operating-system maintenance procedure; the application does not fetch AIA,
  CRL or OCSP data.
- Fail closed on unknown chain material. A future online revocation design
  requires an ADR and exact auxiliary allowlists.
- Limit DNS/connect to 10 seconds, response headers to 30 seconds and the full
  operation to 120 seconds. Enforce transferred, decompressed/working and
  PDF-page or CSV-row/column/cell limits from ADR-0004.
- Validate PDF media/signature/structure or CSV media/encoding/dialect/structure
  before promotion. ETag and Last-Modified never replace SHA-256.

### Public HTTP/OpenAPI v1

- Publish `POST /api/v1/questions`, `GET /api/v1/health/live` and
  `GET /api/v1/health/ready` only.
- Require `application/json`, an 8 KiB request-body limit and a 4 KiB UTF-8
  question limit. Reject URL, host, path, source registration, provider, model,
  adapter or any public field that attempts to mutate catalogue authority.
- Require exactly one configured `corpusId`; the server resolves all active
  document bindings from one activation-record revision.
- Require `questionLanguage` with exactly `pt-BR` or `en-GB`; reject missing,
  unsupported or non-canonical language tags before any provider call.
- Return completed `Answered` and `InsufficientEvidence` outcomes with HTTP
  `200`. Every completed response includes `answerLanguage` equal to the
  accepted `questionLanguage`; an `Answered` payload uses that language. Map
  every failure through the canonical table in the contract document.
- Include `contentLanguage` in every citation and preserve source-derived
  title, section, excerpt and other citation text without model translation.
- Include database/document/version/format/trust identity and a sanitised
  evidence-coverage summary. PDF citations use page/block locations; CSV
  citations use record/column/header locations.
- Use RFC 9457 Problem Details with stable `CH_*` extension code and
  correlation ID. Never include stack, provider payload, prompt, passage,
  path, endpoint or secret.
- Version the generated OpenAPI artefact in the repository. A change that
  removes or renames a field, tightens accepted input incompatibly, changes a
  status/code semantic or changes a required enum value is breaking and needs
  `/api/v2` or an explicitly accepted compatibility ADR.
- Additive optional fields and new non-breaking Problem Details codes are
  permitted in v1 only after contract tests and changelog review.
- Keep Domain entities, provider SDK types and administration operations out
  of OpenAPI.
- Keep the query-language contract independent from Dashboard labels,
  navigation, `interfaceLanguage` and theme state; the Dashboard supports
  `pt-BR`/`en-GB` and `Light`/`Dark` through its own product state rather than
  API authority fields.

### Query limits and abuse controls

- Use a token-bucket limit of 30 requests per minute per derived client key,
  burst 10, with a global concurrency ceiling of 20 queries per instance.
- Limit retrieval to top-k 8, at most 6 passages in model context and 16,000
  Unicode scalar values of total evidence.
- Set an end-to-end server deadline of 25 seconds and propagate cancellation.
- Configure CORS with exact approved Dashboard origins, methods and headers;
  do not allow credentials for anonymous query.
- Render output as plain text in the MVP. Citation URLs come from validated
  catalogue metadata, never directly from model output.

### Readiness

- Liveness checks only that the process can respond and has no external
  dependency.
- Global readiness is healthy only when configuration is valid, the
  control-plane store is readable, one compatible activation record exists,
  at least one active database/document binding is readable, the vector
  adapter can search it and AI provider configuration/circuit state permits an
  attempt.
- Readiness does not make a billable provider call or synchronise a source.
- A stale, unavailable, withdrawn or deactivated source/document is a typed
  per-item coverage degradation and does not make the instance globally unready
  while another active document remains serviceable.
- Return HTTP `200` for ready/degraded-with-partial-coverage and `503` when no
  active document path can serve. Expose only sanitised capability states.

### Local administration

- Use the accepted explicit one-shot mode of `RagChallenge.Server.Api`.
- Proposed syntax:

  ```text
  RagChallenge.Server.Api admin <command> --reason <bounded-text>
  ```

- Permit catalogue commands `add-database`, `version-database`,
  `activate-database`, `deactivate-database`, `remove-database`,
  `add-document`, `version-document`, `activate-document`,
  `deactivate-document`, `remove-document`, `register-official-source`,
  `synchronise-official`, `build-index`, `activate-generation`,
  `rollback-generation` and read-only `status`.
- Require a local operating-system identity, an explicit administration
  enable flag, a non-empty reason of at most 512 characters and the minimum
  filesystem/database permissions needed by the command.
- Refuse administration during normal server mode and refuse a public HTTP
  mapping for every administration command.
- Use a database-backed lease plus compare-and-swap to serialise mutations by
  corpus. Make each command idempotent under an operation ID.
- Enforce `Candidate` before activation, logical tombstones for removal and the
  invariant that the last active document can leave only in the same explicit
  transaction that deactivates its database.
- Record actor identifier, command, reason hash or sanitised reason, source and
  target IDs, start/end instants, result code and correlation/operation IDs.
  Do not log document, prompt, answer or secret content.
- Return stable process exit codes documented with the command contract; a
  process crash or audit-write failure never implies activation success.

## Alternatives

### Public administration API

Rejected because it adds authentication, authorisation and attack surface not
required by the MVP.

### One combined egress allowlist

Rejected because it would allow a source synchroniser, provider adapter or
runtime profile to reuse unrelated authority.

### Follow safe redirects

Rejected for the MVP. Every hop would require full canonicalisation,
resolution, pinning and policy evaluation and weakens the exact-source
contract.

### Online certificate revocation

Not selected because uncontrolled AIA/CRL/OCSP conflicts with exact egress.
The residual risk of local-only trust material must be explicitly accepted.

### Provider call from readiness

Rejected because it creates cost, load and an external dependency for health
polling.

## Consequences

- External AI and official-source access stay disabled until exact endpoint
  evidence and separate execution authority exist.
- Local-only certificate validation requires disciplined base-image and trust
  material maintenance.
- A public anonymous query endpoint needs strict rate, payload, context and
  concurrency limits even though it has no user accounts.
- Administration remains operationally simple but depends on secure local/SSH
  access to the host.
- OpenAPI compatibility becomes a testable product contract owned solely by
  RAG-Challenge.
- Explicit language tags avoid ambiguous server-side detection for short
  questions, while cross-language retrieval remains an evaluated RAG
  capability.

## Security and operations

- Retrieved content, model output and public input are always data, never
  policy or authority.
- No provider, URL, model or administration command is selected from a public
  request.
- Catalogue and source registrations are trusted control-plane records;
  compatible cardinality does not weaken per-item provenance, licence,
  allowlist, validation or activation requirements.
- A source-policy violation is never retried against a different destination.
- Rate limits and monetary circuit breakers are not bypassed by retry.
- Questions, passages and answers are not logged by default.
- Provider requests use only the two exact API paths, disable provider-side
  response storage and hosted tools, and disclose only the minimum authorised
  question/evidence. These controls do not eliminate default abuse-monitoring
  retention.
- External AI accepts only public/authorised corpus data. The public query
  experience warns against confidential, personal or secret questions; this
  ADR authorises no such disclosure.
- Runtime secret retrieval uses an OCI instance principal permitted only to
  read configured secret bundles. It cannot list unrelated bundles or manage
  secrets, keys or vaults; management endpoints and generic OCI egress remain
  unavailable to the application process.
- A P0/P1 security finding blocks state progression until remediation or an
  explicit risk decision allowed by governance.

## Acceptance checks

- Each egress profile has exact destinations or is explicitly empty/disabled.
- AI egress tests permit only the exact embedding/Responses methods and prove
  `store=false`, no hosted tools/state fields, bounded disclosure and refusal
  of a substituted regional authority.
- OCI tests permit only the configured Secret Retrieval GET in normal runtime
  and prove that Core, Key Management and Secret Management endpoints remain
  administrative and disabled; IAM denies unrelated bundle reads and all
  secret/key/vault mutations.
- Tests prove mixed DNS answers, forbidden addresses, rebinding attempts,
  IP-pinned connection, Host/SNI preservation, redirect refusal and zero
  certificate-validation egress.
- Query tests prove unified retrieval over all active bindings, explicit
  partial coverage, no source fetch, exact
  `answerLanguage == questionLanguage`, and the full `pt-BR→pt-BR`,
  `en-GB→en-GB`, `pt-BR→en-GB` and `en-GB→pt-BR` question/evidence matrix
  with untranslated source-derived citation text.
- OpenAPI compatibility tests cover schemas, statuses, stable codes and
  provider/Domain type exclusion.
- Readiness tests cover remaining healthy documents with each per-source
  degradation and fail when no active document is serviceable.
- Administration tests prove OS identity capture, enable flag, reason,
  idempotency, Candidate/Active/Deactivated/Removed transitions, last-document
  invariant, lease conflict, audit failure and absence of HTTP routes.
- The acceptance explicitly includes the offline revocation residual risk,
  bounded externally disclosed data categories, up-to-30-day default abuse
  monitoring and absence of verified Brazilian provider data residency; a
  later alternative still requires separate verification and an architectural
  change when material.
- Acceptance does not enable egress or create an external resource.
