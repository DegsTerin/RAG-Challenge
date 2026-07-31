# ADR-0005 — MVP Providers, Persistence and OCI Deployment

- Status: proposed
- Date: 2026-07-31
- Owners: RAG-Challenge architecture, data and operations
- State: `STATE-02 ARCHITECTURE`
- Verification status: package, provider, model, pricing and OCI facts require
  separately authorised primary-source verification

## Purpose and authority

This ADR proposes one implementation for each replaceable MVP capability and
one durable deployment shape. It does not install a package, call a provider,
create an OCI resource, approve spend or accept itself. All external product
facts are explicitly unverified because this state currently has no network
authority.

## Context

The accepted bootstrap uses .NET 10, ASP.NET Core, React/TypeScript and one
modular-monolith deployable. Domain and Application must remain independent of
parser libraries, provider SDKs, persistence engines and OCI.

The MVP is small enough to favour deterministic local persistence and exact
vector search. A managed vector service would add data egress, credentials,
tenancy and lifecycle coordination before scale demonstrates a need.

## Proposed decision

If accepted after verification:

### PDF parsing and normalisation

- Select the `PdfPig` .NET library as the single PDF text parser adapter.
- Pin the exact NuGet package ID and version only after current official
  package metadata, source repository, licence and vulnerability evidence are
  verified. No version is selected by this offline proposal.
- Reject encrypted PDFs, files with an invalid PDF signature, parser output
  beyond configured limits and pages without extractable text when the
  configured minimum coverage is not met.
- Preserve page number and ordered text blocks. Do not execute attachments,
  JavaScript, actions, forms, links or embedded files.
- Normalise text with Unicode NFC, LF line endings, removal of disallowed
  control characters and deterministic horizontal-whitespace collapse.
- Preserve paragraph and page boundaries. Do not apply dictionary-based
  spelling correction, translation or speculative de-hyphenation.

### Chunking

- Select strategy ID `paragraph-window-v1`.
- Prefer section, paragraph and sentence boundaries in that order.
- Target 3,200 Unicode scalar values per chunk, use 480 scalar values of
  overlap and enforce a hard maximum of 4,000 scalar values.
- Never cross a document, document version, page-range discontinuity or
  `SourceScope` boundary.
- Include the strategy ID, limits, separator policy and normalisation version
  in `IndexCompatibilityKey`.
- Treat these values as the pre-evaluation baseline. Any adjustment creates a
  new strategy version and index generation.

### Embeddings

- Select provider ID `openai` and model ID `text-embedding-3-small` as the MVP
  candidate.
- Use 1,536 dimensions and cosine similarity. Reject a response whose
  descriptor or vector dimension differs from active configuration.
- Batch only within a configured byte/token ceiling and preserve input/output
  order explicitly.
- Cache embeddings only by content hash plus complete compatibility key; a
  cache entry never becomes an activation authority.
- Use a project-owned `IEmbeddingProvider`; keep the official provider SDK or
  HTTP adapter inside Infrastructure.

The model's continued availability, dimension contract, API endpoint,
retention/training policy, data residency, pricing, quota and current official
.NET integration have not been verified and block human acceptance.

### Language model

- Select provider ID `openai` and versioned model candidate
  `gpt-4.1-mini-2025-04-14`.
- Use deterministic settings where the provider supports them: temperature
  `0`, no tool access, bounded output and one response candidate.
- Send only the validated question, the minimum retrieved evidence, trusted
  response instructions and non-secret citation identifiers.
- Require structured output that identifies cited chunk IDs; validate every
  ID against retrieved evidence before producing `Answered`.
- Treat invalid structure, unsupported citations and provider refusal as typed
  failures or `InsufficientEvidence` according to the canonical contract; do
  not repair factual content with an ungrounded second call.

The versioned model's continued availability, endpoint, data policy, pricing,
quota and structured-output support require current primary-source evidence.
No fallback model is active in the MVP.

### Control-plane persistence

- Use SQLite through an Infrastructure adapter for corpus catalogue,
  document/version metadata, official snapshots and observations, generation
  manifests, the complete activation history and sanitised administration
  audit.
- Use EF Core SQLite for migrations and relational mapping, while keeping EF
  types out of Domain and Application.
- Configure WAL mode, foreign keys, busy timeout and explicit transactions.
- Serialise content-changing administration by corpus and use optimistic
  compare-and-swap on `recordRevision`.
- Treat the database file as durable state requiring backup, restore and
  filesystem permissions. Do not store raw document bytes or secrets in it.

The local NuGet cache contains EF Core SQLite `10.0.9`, but cache presence is
not supply-chain approval. Exact package versions and vulnerability evidence
remain an implementation-authority decision after official verification.

### Immutable content store

- Use a filesystem content store rooted at a configured durable directory.
- Address content by lowercase SHA-256 and store it under a two-level hash
  prefix. The catalogue stores opaque content IDs, not absolute paths.
- Write to a same-volume quarantine file, enforce size limits while writing,
  flush, hash, atomically rename and reopen/verify before catalogue promotion.
- Deny overwrite, symlink/reparse-point traversal and paths outside the
  canonical root.
- Retain every object reachable from the active activation record or its one
  eligible rollback predecessor. Cleanup is explicit, audited and refuses an
  object with an unresolved reference.

### Vector store

- Use a project-owned local adapter named `SqliteExactVectorStore`.
- Persist float32 vectors and allowed metadata in SQLite under an immutable
  candidate/generation identity. The vector database is a derived store and
  not the activation system of record.
- Execute a SQL hard filter on `corpusId`, `indexGenerationId` and
  `sourceScope` before loading candidate vectors for exact cosine ranking.
- Reject a global scan followed by scope filtering. Use a composite index on
  the three selectors plus deterministic chunk identity.
- Load only the bounded filtered partition, rank exactly in process and apply
  the configured top-k and score policy.
- Cap the MVP at 10,000 active chunks and 1,536 dimensions. Exceeding either
  limit fails readiness for that generation and requires a new ADR or proven
  optimisation.
- Keep `VECTOR_STORE_EGRESS` empty.

This design trades scale for minimal operational and security surface. State
07 must prove memory, latency and restart behaviour on the named environment.

### OCI deployment

- Deploy one self-contained Linux ARM64 build of `RagChallenge.Server.Api`
  with the compiled Dashboard on one OCI Compute instance.
- Candidate region: `sa-saopaulo-1`.
- Candidate shape: `VM.Standard.A1.Flex`, one OCPU and 6 GiB memory.
- Use a dedicated durable block volume for SQLite databases, content objects,
  vector data, backups and temporary same-volume atomic writes.
- Run the application under a dedicated unprivileged operating-system account
  as a managed service. Put a minimal TLS reverse proxy in front of Kestrel;
  bind Kestrel to loopback only.
- Restrict inbound traffic to HTTPS, temporary HTTP only when required by the
  separately approved certificate procedure, and administrator SSH from an
  exact owner-controlled source range. Deny public administration endpoints.
- Store provider secrets in OCI Vault or another owner-approved OCI secret
  mechanism and inject only secret references/values at runtime.
- Take an identified application-consistent backup before release and before
  a migration. Restore into an isolated path and validate before declaring a
  rollback target.

Shape availability, Always Free eligibility, pricing, capacity, service
limits, block-volume durability, Vault integration, region availability and
TLS/certificate options are not verified. No OCI account, tenancy or resource
was accessed.

## Configuration decisions

- Validate every capability at startup and fail closed.
- Use typed sections beneath `RagChallenge:*`; files contain no secret values.
- Treat provider, model, dimensions, endpoint policy, corpus root, content
  root, database path, vector schema and compatibility key as immutable for a
  running process.
- Disable `OfficialOnline`, administration and external AI independently when
  their complete configuration is absent.
- Do not support dynamic plug-in loading or runtime provider switching.

## Alternatives

### Managed vector database

Rejected for the MVP because it creates a second external data plane,
credential and egress policy without demonstrated scale need.

### PostgreSQL with pgvector

Viable but not selected. It adds database operation and deployment complexity
for a single bounded corpus. It is the preferred reconsideration if the exact
SQLite scan fails the pre-registered performance threshold.

### Local embedding and language models

Not selected for the first proposal. They avoid data egress but increase
model-distribution, licence, memory, CPU and OCI capacity risks. They remain
the preferred fallback if provider data terms or recurring cost are rejected.

### OCI Generative AI

Not selected without current region, model, SDK, data-policy and cost
evidence. It may reduce cross-provider operations but does not remove the need
for explicit AI egress and data review.

### Mutable filesystem index

Rejected because partial writes, mixed generations and rollback ambiguity
conflict with the accepted lifecycle model.

## Consequences

- All durable product state fits one secured volume and one process boundary.
- SQLite remains a replaceable Infrastructure detail but becomes important to
  backup, concurrency and corruption testing.
- Exact vector search is predictable and proves isolation, but the explicit
  chunk/dimension limits constrain corpus growth.
- External AI minimises OCI compute requirements but discloses authorised
  chunks and questions under a separately governed policy.
- A provider or model change always requires a new compatibility key, index
  generation and evaluation baseline.

## Security and operations

- Database and content paths are never returned in public responses or logs.
- Provider credentials remain server-side and are scoped only to the selected
  API.
- Backups inherit corpus classification, are encrypted by the selected OCI
  mechanism and have a recorded retention/deletion procedure.
- The OCI metadata service, private address ranges and generic internet access
  remain denied unless an exact operational dependency is approved.
- Standard tests use deterministic fakes and make no provider or OCI call.

## Acceptance checks

- Current primary evidence verifies parser/package licence and provider/model
  contracts, terms, pricing and quotas.
- The owner explicitly accepts the external disclosure and budget model.
- A disposable authorised spike confirms PDF extraction quality, embedding
  dimensions, structured model output and the exact vector-store latency cap;
  no spike artefact becomes product implementation.
- A clean local environment and the named OCI architecture can reopen raw
  content, catalogue and vector data after restart.
- Backup/restore and activation rollback have distinct procedures and owners.
- No Domain/Application type depends on PdfPig, EF Core, SQLite, OpenAI or OCI.
- No external package, model or resource is installed or accessed merely by
  accepting this ADR.
