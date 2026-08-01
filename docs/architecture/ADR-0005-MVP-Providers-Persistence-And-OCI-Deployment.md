# ADR-0005 — MVP Providers, Persistence and OCI Deployment

- Status: proposed
- Date: 2026-07-31
- Owners: RAG-Challenge architecture, data and operations
- State: `STATE-02 ARCHITECTURE`
- Verification status: public primary-source verification completed on
  2026-07-31 within the authorised read-only scope; account-specific
  entitlement, capacity and controls plus runtime spikes remain untested

## Purpose and authority

This ADR proposes one implementation for each replaceable MVP capability and
one durable deployment shape. It does not install a package, call a provider,
create an OCI resource, approve spend or accept itself. Bounded direct-URL
read-only verification was completed on 2026-07-31. The proposal remains
undecided, and tenancy-specific facts and executable evidence remain pending
where stated below.

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
- The official NuGet package ID is `PdfPig`. The live registry index lists
  `0.1.15`, published on 2026-06-25, as the newest stable version and
  `0.1.16-alpha-*` as pre-release builds. Treat `0.1.15` only as the current
  verified stable candidate; this proposed ADR does not select or install it.
- The NuGet gallery and source repository identify Apache-2.0 as the licence.
  Registry metadata for `0.1.15` identifies repository commit
  `f131f642976936e06ee91cb19d3ed728f9dd18b6` and target groups for .NET 6.0,
  .NET 8.0, .NET Standard 2.0 and .NET Framework 4.6.2/4.7.1. The project
  warns that minor versions may change the public API before 1.0.0.
- The [NuGet package page](https://www.nuget.org/packages/PdfPig/0.1.15)
  computes compatibility with `net10.0` from the published targets. The
  [GitHub release](https://github.com/UglyToad/PdfPig/releases/tag/v0.1.15)
  is marked latest and includes a change that exposes `StackDepthGuard` so
  indirect-reference resolution can enforce its nesting-depth limit.
- The repository's [public security page](https://github.com/UglyToad/PdfPig/security)
  reports no `SECURITY.md` and no published security advisories. Its
  [advisory list](https://github.com/UglyToad/PdfPig/security/advisories) is
  empty. The NuGet catalogue entry does not expose a `vulnerabilities`
  property. These are bounded public-metadata observations, not proof that no
  vulnerability exists.
- Pin an exact version only after the owner decides this ADR and a separately
  authorised compatibility/extraction spike verifies .NET 10 loading,
  adversarial parser limits and extraction quality. No package was downloaded
  or installed during architecture verification.
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
- Treat cross-language retrieval quality as unverified until the candidate
  passes the `pt-BR`/`en-GB` same-language and bidirectional cross-language
  evaluation matrix; public model availability is not that evidence.
- Use 1,536 dimensions and cosine similarity. Reject a response whose
  descriptor or vector dimension differs from active configuration.
- Batch only within a configured byte/token ceiling and preserve input/output
  order explicitly.
- Cache embeddings only by content hash plus complete compatibility key; a
  cache entry never becomes an activation authority.
- Use a project-owned `IEmbeddingProvider`; keep the official provider SDK or
  HTTP adapter inside Infrastructure.

The current
[model page](https://developers.openai.com/api/docs/models/text-embedding-3-small)
and [embedding guide](https://developers.openai.com/api/docs/guides/embeddings)
verify continued availability, a default length of 1,536, an 8,192-token
per-input limit and USD 0.02 per million input tokens. The
[create-embeddings contract](https://developers.openai.com/api/reference/resources/embeddings/methods/create)
defines `POST /v1/embeddings`, optional `dimensions`, ordered response indexes
and a maximum of 300,000 input tokens summed across one request. The public
Tier 1 schedule is 3,000 RPM, 1,000,000 TPM and a 3,000,000-token batch queue;
the actual project tier and limits remain account-specific and unverified.

The model page publishes only the mutable `text-embedding-3-small` alias and
no dated immutable snapshot. Acceptance therefore requires the owner to
accept alias drift as an explicit reproducibility risk, with the observed
response model/dimensions and complete compatibility descriptor persisted per
generation. A changed descriptor, dimensions or evaluation result requires a
new generation and blocks silent reuse.

### Language model

- Select provider ID `openai` and versioned model candidate
  `gpt-4.1-mini-2025-04-14`.
- Use deterministic settings where the provider supports them: temperature
  `0`, no tool access, bounded output and one response candidate. Use the
  Responses API with `store=false`; do not use background mode,
  conversations, previous-response state or hosted tools.
- Send only the validated question, the minimum retrieved evidence, trusted
  response instructions and non-secret citation identifiers.
- Include the trusted `questionLanguage` instruction and require structured
  `answerLanguage` output equal to it. Do not ask the model to translate
  source-derived citation text.
- Require structured output that identifies cited chunk IDs; validate every
  ID against retrieved evidence before producing `Answered`.
- Treat invalid structure, unsupported citations and provider refusal as typed
  failures or `InsufficientEvidence` according to the canonical contract; do
  not repair factual content with an ungrounded second call.

The current
[model page](https://developers.openai.com/api/docs/models/gpt-4.1-mini)
identifies `gpt-4.1-mini-2025-04-14` as the default and only listed snapshot,
supports Responses and Structured Outputs, and confirms a 1,047,576-token
context window, 32,768 maximum output tokens and current prices of USD 0.40
input, USD 0.10 cached input and USD 1.60 output per million tokens. The
[Responses contract](https://developers.openai.com/api/reference/resources/responses/methods/create)
defines `POST /v1/responses`, bounded `max_output_tokens`, temperature from 0
to 2, `store` and JSON-schema structured output. Public Tier 1 limits are 500
RPM, 10,000 RPD, 200,000 TPM and a 2,000,000-token batch queue; long-context
requests above 128,000 input tokens have a separate 200 RPM and 400,000 TPM
Tier 1 schedule. The actual account tier and spend limit remain unverified.

The [OpenAI data-control documentation](https://developers.openai.com/api/docs/guides/your-data)
states that API data is not used for model training unless the customer opts
in. Default abuse-monitoring logs may contain customer content and are
retained for up to 30 days. `/v1/embeddings` has no application-state
retention; `/v1/responses` stores application state for at least 30 days by
default or with `store=true`, while approved Zero Data Retention forces
`store=false`. Modified Abuse Monitoring, Zero Data Retention and non-US data
residency require eligibility and contractual approval. The selected
embedding and language-model services support United States and European
regional processing, but Brazil is not a listed data-residency region. The
proposal must therefore assume the default abuse-monitoring retention and no
Brazilian residency unless separately contracted. The owner must explicitly
accept that disclosure/residency model; no fallback model is active.

The official [OpenAI .NET repository](https://github.com/openai/openai-dotnet)
identifies the `OpenAI` NuGet library as generated from OpenAI's OpenAPI
specification and exposes `EmbeddingClient` and `ResponsesClient`. The latest
stable [release list](https://github.com/openai/openai-dotnet/releases) and
[NuGet index](https://api.nuget.org/v3-flatcontainer/openai/index.json)
identify `2.12.0`, published on 2026-07-01. Its catalogue metadata is listed,
MIT-licensed and targets `net8.0`, `net10.0` and .NET Standard 2.0. Treat
`2.12.0` only as the current SDK candidate; no version was selected or
installed.

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
  vector data and temporary same-volume atomic writes. An archive or copy on
  that same volume is not an availability-domain recovery backup.
- Run the application under a dedicated unprivileged operating-system account
  as a managed service. Put a minimal TLS reverse proxy in front of Kestrel;
  bind Kestrel to loopback only.
- Restrict inbound traffic to HTTPS, temporary HTTP only when required by the
  separately approved certificate procedure, and administrator SSH from an
  exact owner-controlled source range. Deny public administration endpoints.
- Store provider secrets in the OCI Secret Management service, backed by an
  owner-approved virtual-vault key, or another explicitly approved OCI secret
  mechanism. Persist only opaque secret references and inject values at
  runtime.
- Take an identified application-consistent backup before release and before
  a migration. The owner must select regional OCI Block Volume Backup or
  another independent backup target before accepting the deployment decision;
  restore into an isolated path and validate before declaring a rollback
  target.

The [regions table](https://docs.oracle.com/en-us/iaas/Content/General/Concepts/regions.htm)
verifies `sa-saopaulo-1` as Brazil East (Sao Paulo), commercial realm `OC1`,
with one availability domain. The
[shape reference](https://docs.oracle.com/en-us/iaas/Content/Compute/References/computeshapes.htm)
verifies `VM.Standard.A1.Flex` as an Ampere Altra ARM shape, with one OCPU
equal to one core, 1 GiB minimum memory and up to 64 GiB per OCPU; one OCPU
and 6 GiB is a valid configuration. The reference also states that only the
authenticated `ListShapes` operation establishes shapes available to a
tenancy, and capacity can lag or be temporarily exhausted. Public evidence
therefore verifies the candidate's existence and validity, not deployable
capacity in the future tenancy.

The current official sources conflict on the free allowance. The
[Always Free resource page](https://docs.oracle.com/en-us/iaas/Content/FreeTier/freetier_topic-Always_Free_Resources.htm)
states 1,500 A1 OCPU-hours and 9,000 GB-hours per month, equivalent to two
OCPUs and 12 GiB, and warns that Always Free host capacity can be temporarily
unavailable. The [live price-list data](https://www.oracle.com/a/ocom/docs/pricing/cloud-price-list.json),
build 350 dated 2026-07-16, prices the first 3,000 OCPU-hours and 18,000
GB-hours at zero, then USD 0.01 per OCPU-hour and USD 0.0015 per GB-hour. The
candidate 1 OCPU/6 GiB fits the lower published allowance, but no single free
entitlement or zero-cost claim is accepted until the future tenancy confirms
its own subscription, limits and invoice model. Without a free allowance,
730 hours of candidate compute is USD 13.87 before storage, network, tax and
other services.

The [service-limits table](https://docs.oracle.com/en-us/iaas/Content/General/service-limits/default.htm)
publishes Pay As You Go or Trial defaults of 16 A1 OCPUs and 96 GiB memory per
availability domain; actual tenancy limits remain authoritative. It also
publishes a 200 GiB aggregate block-volume allowance for Trial and limits of
10 virtual vaults per region, 150 secrets and 40 versions per secret for
Always Free or Trial, versus 5,000 secrets and 60 versions per secret for paid
accounts. The candidate fits the public compute defaults but storage size and
backup retention remain undecided.

[Block Volume documentation](https://docs.oracle.com/en-us/iaas/Content/Block/Concepts/overview.htm)
states that volumes persist independently of instances, remain accessible
only within their availability domain, are encrypted at rest, store redundant
copies across storage servers and target 99.99% annual durability. Oracle
still recommends regular backups for availability-domain failure. The
[backup documentation](https://docs.oracle.com/en-us/iaas/Content/Block/Concepts/blockvolumebackups.htm)
defines encrypted regional Object Storage backups restorable to any
availability domain in the region. The live price list publishes USD 0.0255
per GB-month of block capacity plus USD 0.0017 per VPU-GB-month, making the
balanced 10-VPU tier USD 0.017 per GB-month in performance units. It publishes
the first 10 GB of standard Object Storage at zero and USD 0.0255 per
GB-month thereafter; the exact billed backup bytes and retention remain a
budget input.

The [Secret Management page](https://docs.oracle.com/en-us/iaas/Content/secret-management/home.htm)
confirms secure storage and retrieval of API keys, tokens and other secrets;
the previous Vault secrets page now routes this functionality to that service.
The price list identifies Secret Management and software-protected vault keys
as free, with the first 20 HSM-protected key versions free. The official
[API index](https://docs.oracle.com/en-us/iaas/api/specs/index.json) publishes
these `sa-saopaulo-1` endpoints: Core Services
`https://iaas.sa-saopaulo-1.oraclecloud.com`, Key Management
`https://kms.sa-saopaulo-1.oraclecloud.com`, Secret Management
`https://vaults.sa-saopaulo-1.oci.oraclecloud.com` and runtime Secret
Retrieval `https://secrets.vaults.sa-saopaulo-1.oci.oraclecloud.com`. The
retrieval contract uses API version `20190301` and
`GET /20190301/secretbundles/{secretId}`. These endpoint strings were read
from documentation only; none was contacted, and no tenancy or resource was
accessed.

## Primary-source verification record

The following bounded read-only evidence was observed on 2026-07-31:

| Area | Status | Primary evidence or remaining boundary |
|---|---|---|
| PdfPig package | Public metadata verified | The [NuGet version index](https://api.nuget.org/v3-flatcontainer/pdfpig/index.json), [0.1.15 registration](https://api.nuget.org/v3/registration5-semver1/pdfpig/0.1.15.json), catalogue, package page, allowlisted source/release and GitHub security pages verify the current stable candidate, publication instant, listed status, repository identity, Apache-2.0, computed .NET 10 compatibility and absence of a published security policy or advisory. Absence of public advisory evidence is not absence of vulnerability; executable parser evidence remains a separately authorised spike. |
| OpenAI embedding/model contracts | Public facts verified | Current model pages and API references verify availability, selected IDs, dimensions, limits, endpoints, structured output and price. `text-embedding-3-small` has no immutable dated snapshot, and the actual project tier remains account-specific. |
| OpenAI data controls | Public policy verified | Default no-training, up-to-30-day abuse monitoring, endpoint application-state behaviour, ZDR/MAM eligibility and regional storage/processing support are documented. Brazil is not a listed residency region; account eligibility and any modified-retention agreement are unverified without login or contract. |
| OpenAI .NET SDK | Public metadata verified | The official repository and NuGet catalogue identify stable candidate `OpenAI` 2.12.0, MIT, with `net10.0`, `EmbeddingClient` and `ResponsesClient`. No package was selected, downloaded or installed. |
| OCI region, shape, limits and prices | Verified with account boundary | Public documentation verifies the region, one-AD topology, valid A1 configuration, default limits and price rates. The two current official free-allowance sources conflict, and actual entitlement/capacity is tenancy-specific. |
| OCI storage, Secret Management and endpoints | Public facts verified | Durability, regional backups, storage prices, vault/secret limits and the four exact Sao Paulo service endpoints are documented. Backup target/retention and runtime IAM remain undecided and untested. |

This resumed round used direct HTTPS requests only, disabled redirects and did
not use general web search. Same-allowlisted-host redirects from obsolete
Oracle documentation/pricing URLs were inspected and their exact targets were
requested separately. No request redirected to an unauthorised host or
required authentication.

No package, model or corpus was downloaded or installed, and no external
resource was created or changed. `api.openai.com` and every documented OCI
service endpoint remained uncontacted.

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
  chunks and questions under a separately governed policy, with default abuse
  monitoring for up to 30 days and no verified Brazilian residency.
- The embedding candidate lacks a dated immutable snapshot, so provider alias
  drift must be detected and evaluated rather than assumed stable.
- Either provider candidate failing the accepted `pt-BR`/`en-GB` language
  matrix blocks that candidate; it does not relax the product requirement.
- One Sao Paulo availability domain and 99.99% block-volume durability make a
  regional independent backup and tested restore material, not optional
  copies on the data volume.
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
  contracts, dimensions, endpoints, data controls, pricing, public quotas,
  SDK metadata and OCI candidate facts within the unauthenticated scope.
- The owner explicitly accepts or changes the mutable embedding alias, default
  provider retention/disclosure, absence of Brazilian data residency, public
  pricing and account-specific quota boundary.
- The owner resolves the OCI free-allowance source conflict without assuming
  zero cost, accepts the one-AD/capacity risk and selects block-volume size,
  independent regional backup target, retention and restore objective.
- The owner selects the exact OCI secret mechanism and runtime retrieval/IAM
  design; documentation evidence does not create a vault or grant access.
- A disposable authorised spike confirms PDF extraction quality, embedding
  dimensions, structured model output and the exact vector-store latency cap;
  no spike artefact becomes product implementation.
- The authorised evaluation proves answer-language equality, original-language
  citation preservation and all four question/evidence language pairs before
  either provider is described as supporting the product requirement.
- A clean local environment and the named OCI architecture can reopen raw
  content, catalogue and vector data after restart.
- Backup/restore and activation rollback have distinct procedures and owners.
- No Domain/Application type depends on PdfPig, EF Core, SQLite, OpenAI or OCI.
- No external package, model or resource is installed or accessed merely by
  accepting this ADR.
