# ADR-0006 — Security, Egress, Administration and HTTP Contracts

- Status: proposed
- Date: 2026-07-31
- Owners: RAG-Challenge security, API and operations
- State: `STATE-02 ARCHITECTURE`
- Dependencies: ADR-0002, ADR-0004 and ADR-0005
- Verification status: partial; official-source response and local offline-TLS
  policy were observed, while AI and OCI endpoint evidence remains incomplete

## Purpose and authority

This ADR proposes the fail-closed security profiles, local administration
surface and public HTTP compatibility policy for the MVP. It does not enable
egress, create credentials, expose administration, call a source/provider or
accept itself.

## Context

The application has four independent outbound purposes and two public query
capabilities. It must prevent a question or retrieved document from changing
policy, source, provider or administration. It also needs a stable OpenAPI v1
contract without exposing Domain, persistence or provider types.

## Proposed decision

If accepted:

### Egress profiles

Every profile is deny by default and independently evaluated. An allowed host
in one profile grants no access through another.

#### `AI_PROVIDER_EGRESS`

- Candidate destination: exact DNS host `api.openai.com`, port `443`, HTTPS
  only.
- Permit only the path families required by the verified embedding and
  response APIs. Record those exact prefixes after official verification;
  until then this profile remains disabled.
- The candidate host was not contacted. `developers.openai.com` was authorised
  for a second documentation round, but that round stopped during the prior
  PdfPig search. No AI API path or destination is therefore verified.
- Reject userinfo, non-default ports, redirects, proxies and ambient
  credentials.
- Send only bounded authorised chunks for indexing, the bounded question for
  query embedding, and the bounded question plus selected evidence for answer
  generation.
- Apply connect and total timeouts, cancellation, request-size/token budgets,
  concurrency limits and a monetary circuit breaker.

#### `VECTOR_STORE_EGRESS`

- Configure no destination for the MVP because the selected vector adapter is
  local.
- Treat any configured destination as `ConfigurationInvalid`.

#### `OFFICIAL_SOURCE_EGRESS`

- Candidate destination: exact canonical URI declared by ADR-0004.
- Enable only in the explicit administration synchronisation profile.
- Apply the complete SSRF, DNS/IP, TLS, redirect, content and rate policy
  below. Query and normal startup profiles cannot resolve or connect to the
  source.
- The exact URI returned `200` to `HEAD` and `206` to the authorised 64 KiB
  range request without a redirect on 2026-07-31. This verifies the candidate
  authority, path, media type, byte-range behaviour, current size and leading
  signature only; it does not enable the profile or prove the complete
  application network policy.
- `robots.txt` does not disallow the exact documentation path. Direct TLS
  negotiated TLS 1.3 and an offline four-element chain validated using local
  trust with certificate downloads disabled and revocation `NoCheck`. This
  supports the proposed no-lateral-download policy while retaining its
  explicit offline-revocation and future-clean-environment risks.

#### `OCI_RUNTIME_EGRESS`

- Compose only destinations already authorised for AI, official source,
  secret retrieval, certificate operation and sanitised telemetry.
- Do not include a generic `0.0.0.0/0` or `::/0` application egress rule as a
  substitute for destination policy.
- Keep metadata-service, private, link-local and loopback destinations denied
  from all untrusted URL flows.
- Exact regional OCI service endpoints remain disabled until primary-source
  verification and separate deployment authority identify them.
- OCI documentation checks were not reached before either mandatory browsing
  stop; no endpoint from this profile is currently verified.

### Official-source network policy

- Canonicalise once and compare scheme, IDNA ASCII host, port, path and query
  against the configured exact URI.
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
  operation to 120 seconds. Enforce transferred, decompressed/working and page
  limits from ADR-0004.
- Validate PDF media type, `%PDF-` signature, structure and page limit before
  promotion. ETag and Last-Modified never replace SHA-256.

### Public HTTP/OpenAPI v1

- Publish `POST /api/v1/questions`, `GET /api/v1/health/live` and
  `GET /api/v1/health/ready` only.
- Require `application/json`, an 8 KiB request-body limit and a 4 KiB UTF-8
  question limit. Reject unknown `sourceScope`, URL, host, path, provider,
  model or adapter fields.
- Require exactly one configured `corpusId` and one of `Local` or
  `OfficialOnline`.
- Return completed `Answered` and `InsufficientEvidence` outcomes with HTTP
  `200`; map every failure through the canonical table in the contract
  document.
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
  the selected `Local` content/index is readable, the vector adapter can
  search it and AI provider configuration/circuit state permits an attempt.
- Readiness does not make a billable provider call or synchronise a source.
- `OfficialOnline` stale, unavailable, withdrawn or deactivated is a typed
  per-scope degradation and does not make the instance globally unready while
  `Local` remains serviceable.
- Return HTTP `200` for ready/degraded-global-with-local and `503` when the
  mandatory Local path cannot serve. Expose only sanitised capability states.

### Local administration

- Use the accepted explicit one-shot mode of `RagChallenge.Server.Api`.
- Proposed syntax:

  ```text
  RagChallenge.Server.Api admin <command> --reason <bounded-text>
  ```

- Permit only `synchronise-official`, `build-index`, `activate-generation`,
  `rollback-generation`, `deactivate-official` and read-only `status`.
- Require a local operating-system identity, an explicit administration
  enable flag, a non-empty reason of at most 512 characters and the minimum
  filesystem/database permissions needed by the command.
- Refuse administration during normal server mode and refuse a public HTTP
  mapping for every administration command.
- Use a database-backed lease plus compare-and-swap to serialise mutations by
  corpus. Make each command idempotent under an operation ID.
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

## Security and operations

- Retrieved content, model output and public input are always data, never
  policy or authority.
- No provider, URL, model or administration command is selected from a public
  request.
- A source-policy violation is never retried against a different destination.
- Rate limits and monetary circuit breakers are not bypassed by retry.
- Questions, passages and answers are not logged by default.
- A P0/P1 security finding blocks state progression until remediation or an
  explicit risk decision allowed by governance.

## Acceptance checks

- Each egress profile has exact destinations or is explicitly empty/disabled.
- Tests prove mixed DNS answers, forbidden addresses, rebinding attempts,
  IP-pinned connection, Host/SNI preservation, redirect refusal and zero
  certificate-validation egress.
- Query tests prove no cross-scope fallback and no source fetch.
- OpenAPI compatibility tests cover schemas, statuses, stable codes and
  provider/Domain type exclusion.
- Readiness tests cover healthy Local with every official-source degradation.
- Administration tests prove OS identity capture, enable flag, reason,
  idempotency, lease conflict, audit failure and absence of HTTP routes.
- The owner explicitly accepts the offline revocation residual risk and all
  externally disclosed data categories.
- Acceptance does not enable egress or create an external resource.
