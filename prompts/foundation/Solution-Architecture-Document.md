# RAG-Challenge Solution Architecture

## Status

High-level baseline proposed in `STATE-00 DISCOVERY` and reconciled with
architectural decisions accepted through `STATE-02 ARCHITECTURE`. The current
physical map belongs to ADR-0003, which incorporates ADR-0001 decisions not
related to naming; the RAG lifecycle and conditional selections belong to
ADR-0002 and ADR-0004 through ADR-0006. ADRs are indexed in
[`../../docs/architecture/`](../../docs/architecture/README.md). This view does
not itself represent implementation, testing, deployment or homologation.
ADR-0007 corrects generation identity/freshness; accepted ADRs 0008 and 0009
refine storage/visual evidence and document languages. Implemented state
remains in the factual snapshot; later increments implemented those
refinements, v2/same-origin serving and the notice-bearing profile at the local
synthetic boundary without constituting product homologation.

## Principles

- A modular monolith proportionate to the MVP.
- Dependencies point inwards.
- Domain is independent of AI, persistence, UI and infrastructure.
- Typed ports for sources, parsing, embeddings, vectors and LLM.
- Fail-closed configuration with no persisted secrets.
- Provenance and version preserved from document to citation.
- `SupportedQueryLanguage` closed to `pt-BR` and `en-GB`, with an answer in the
  explicit question language; separate `DocumentContentLanguage` open to
  canonical BCP 47 tags without inferring `en` as `en-GB`.
- Source-derived content preserved in the original citation language.
- Persistent content-addressed source bytes and PNGs, with deterministic PDF
  rendering and a complete manifest before active visual evidence.
- Indexes built immutably before activation.
- External failure isolated and explicitly classified.
- Local and official origin preserved as provenance and trust, with all active
  documents eligible for unified retrieval.
- A versioned external contract owned by RAG-Challenge; future consuming
  adapters belong to their respective repositories.

## System context

```text
Question author / evaluator
          |
          v
  RAG-Challenge Dashboard
          |
       HTTPS
          |
          v
    RAG-Challenge Server/API
          |
          v
 Application use cases
   |       |        |        |
   |       |        |        +--> language-model adapter
   |       |        +-----------> vector-store adapter
   |       +--------------------> document/embedding adapters
   +----------------------------> governed local/official source adapters
```

In the MVP, the server may host the API and static Dashboard files in the same
deployment. This reduces operational work without coupling the interface to
use cases.

## Dependency direction

```text
RagChallenge.Domain
        ^
        |
RagChallenge.Application
(includes RAG abstractions)
        ^
        |
RagChallenge.Infrastructure / RagChallenge.Server.Api

RagChallenge.Dashboard.Web -- versioned HTTP --> RagChallenge.Server.Api
```

- `RagChallenge.Domain` owns identities, versions, states and invariants.
- `RagChallenge.Application` owns RAG contracts, implements use cases and
  orchestrates ports; it references only Domain.
- `RagChallenge.Infrastructure` implements adapters and persistence.
- The API is the composition root and contains no business rules.
- The Dashboard has no code reference to Application; it consumes only
  versioned HTTP contracts.

## Canonical modules

| ID | Module | Responsibility |
|---|---|---|
| `CH-MOD-01` | `CORPUS_CATALOG` | Identity, many-to-many categories, versions and database/document state. |
| `CH-MOD-02` | `DOCUMENT_INGESTION` | Discovery, validation, PDF/CSV parsing and normalisation. |
| `CH-MOD-03` | `INDEXING_RETRIEVAL` | Chunking, embeddings, generations and retrieval. |
| `CH-MOD-04` | `ANSWER_GENERATION` | Grounded context, answer, citations and refusal. |
| `CH-MOD-05` | `QUERY_EXPERIENCE` | Query API and interface. |
| `CH-MOD-06` | `OPERATIONS_GOVERNANCE` | Configuration, health, logs, audit and gates. |
| `CH-MOD-07` | `OFFICIAL_EXTERNAL_SOURCES` | Governed manual registration and synchronisation of compatible official sources. |
| `CH-MOD-08` | `EXTERNAL_INTEGRATION_CONTRACTS` | Versioned RAG-Challenge HTTP/OpenAPI contract; consuming adapters remain outside this repository. |

IDs must not be reused with another meaning. `CH-MOD-08` preserves the prior
baseline's integration boundary, but its label was corrected before the Human
Gate to make explicit that RAG-Challenge owns the contract, not the consuming
adapter.

## Physical map accepted at bootstrap

`GATE-B01` accepted the physical map later incorporated by ADR-0003. The
structure below records that current boundary; it neither authorises functional
logic nor claims that capabilities described in this document are implemented.

```text
/
├── AGENTS.md
├── README.md
├── RAG-Challenge.sln
├── Directory.Build.props
├── Directory.Packages.props
├── global.json
├── NuGet.config
├── .editorconfig
├── .gitattributes
├── .gitignore
├── docs/
├── prompts/
├── src/
│   ├── RagChallenge.Domain/
│   ├── RagChallenge.Application/
│   ├── RagChallenge.Infrastructure/
│   ├── RagChallenge.Server.Api/
│   └── RagChallenge.Dashboard.Web/
└── tests/
    ├── RagChallenge.UnitTests/
    ├── RagChallenge.Architecture.Tests/
    └── RagChallenge.IntegrationTests/
```

The `STATE-01` scaffold initially materialised only configuration, checks,
minimal hosts and boundary markers. Later separately authorised increments
implemented ingestion, retrieval, generation and one-shot administration in
the main host. RAG abstractions remain separated within
`RagChallenge.Application`, and persistence within
`RagChallenge.Infrastructure`. The executed state and its boundaries belong to
Current State and factual reports, not this architectural view.

## Naming conventions

- Public name and solution file: `RAG-Challenge` and `RAG-Challenge.sln`.
- Technical project, assembly and namespace prefix: `RagChallenge`.
- Projects: `RagChallenge.<Responsibility>`.
- Tests: `RagChallenge.<TestScope>Tests`.
- C# types and members: PascalCase; variables and parameters: camelCase.
- Initial API: `/api/v1`.
- Configuration: `RagChallenge:<Capability>` sections.
- Error IDs: `CH_<AREA>_<CONDITION>`, stable and without secret detail; the
  prefix remains for compatibility after product renaming.
- Corpus IDs: stable lowercase slugs; display names are not identifiers.
- Contract timestamps: UTC in ISO 8601.
- Documents: descriptive names; ADRs in English following the DB-Notifier
  pattern.

Language conventions belong to the
[`language policy`](../governance/Language-Policy.md). This document does not
redefine them.

## Components and responsibilities

### Domain

Candidate concepts:

- `CorpusId`, `CorpusStatus` and `CorpusRevision`;
- `DatabaseProductId`, `DatabaseProductRevision`, `DatabaseProductStatus` and
  `DatabaseCategoryAssignment`;
- `DocumentId`, `DocumentVersion`, `DocumentStatus`, `DocumentFormat` and
  `SourceDescriptor`;
- `ContentObjectId`, `DocumentPageImage`, `DocumentRenderManifest` and
  immutable references to source content and visual derivatives;
- `SourceTrustClass`, `OfficialSourceRegistration`, `OfficialSourceSnapshot`,
  `OfficialSourceObservation` and `SourceFreshness`;
- `ChunkIdentity` and `Citation`;
- `CandidateBuildId`, `IndexGenerationId`, `IndexGenerationStatus` and
  `CorpusActivationRecord`;
- `ProviderDescriptor`;
- `SupportedQueryLanguage`, restricted to `pt-BR` and `en-GB`, and separate
  canonical BCP 47 `DocumentContentLanguage`;
- `QueryRequest`, `RetrievedEvidence` and `AnswerOutcome`.

Domain knows no file paths, PDF, SQL, HTTP, SDKs or models.

### RAG abstractions in Application

Candidate ports:

- `IDocumentSource`;
- `IOfficialSourceSynchroniser`;
- `IDocumentParser`;
- `IChunkingStrategy`;
- `IEmbeddingProvider`;
- `IVectorStore`;
- `ILanguageModel`;
- `IDocumentContentStore`;
- `IDocumentCatalog`;
- `IIndexGenerationStore`;
- `IClock` or `TimeProvider` at the appropriate boundary.

Contracts carry `CancellationToken`, limits and typed outcomes.
`IVectorStore` receives a `VectorSearchRequest` with `CorpusId`,
`IndexGenerationId`, query vector, limits, generation-bound selectors for
eligible bindings and optional administrative database/document filters. The
adapter proves hard pre-filtering by corpus, generation, eligible bindings and
filters before top-k, or uses equivalent physical partitioning; post-filtering
a global search does not satisfy the contract. It has no activation authority.

`IDocumentContentStore` is the only product authority that persists and
reopens immutable content-addressed bytes for sources and page PNGs. Git, Git
LFS, quarantine and the vector store do not replace it. `IDocumentCatalog`
maintains identities, language/provenance/rights, rendering manifests and
references. `IIndexGenerationStore` is the sole source of truth for the
`CorpusActivationRecord`, which atomically binds the active generation to an
ordered set of applicable database, document, version, snapshot and
observation bindings. Complete old and new revisions remain in the versioned
history of the same transaction; `sourceBindingSetDigest` protects the
observation-free generation-bound projection and `activationBindingSetDigest`
protects the complete binding. `Active` and `Retained` are projections, not
parallel authorities.

### Application

Candidate use cases:

- `BuildCorpusIndex`;
- `ActivateIndexGeneration`;
- `RollbackIndexGeneration`;
- `RegisterDatabaseProduct`, `VersionDatabaseProduct`,
  `ActivateDatabaseProduct`, `DeactivateDatabaseProduct` and
  `RemoveDatabaseProduct`;
- `RegisterDocument`, `VersionDocument`, `ActivateDocument`,
  `DeactivateDocument` and `RemoveDocument`;
- `SynchroniseOfficialSource`;
- `AskQuestion`;
- `GetSystemReadiness`.

Scheduled incremental updating and multiple-corpus management are not MVP use
cases. Compatible databases, documents and sources are administrable records;
official synchronisation is manual, limited to allowlisted URLs and does not
occur in the question flow.

### Infrastructure and Persistence

- One concrete adapter per port in the MVP.
- ADR-0005 conditionally accepted EF Core SQLite for the local catalogue,
  metadata and history, subject to exact versions, implementation and future
  evidence.
- Raw storage is durable, content-addressed and separate from the catalogue; a
  local filesystem is the MVP candidate, with an equivalent durable path in
  the OCI target. The vector store does not replace bytes required for rebuild,
  retention or rollback.
- The same content store preserves deterministic page PNGs. Every PDF uses the
  `pdf-page-png-v1` profile, one immutable binding per physical page and one
  complete canonical `DocumentRenderManifest`; CSV receives no implicit image.
- Rendering, persistent PNG and v2 same-origin serving are implemented locally.
  The `pdf-page-png-notice-v1` successor preserves page pixels, binds the
  obligation set to manifest/reachability and presents the exact text in the
  Dashboard; notice-bearing AQG and real product remain separate.
- Vector storage remains behind `IVectorStore`.
- A managed vector store requires its own egress and data-handling policy;
  selecting a local implementation avoids that MVP egress.
- EF Core and schema details do not leak into Domain/Application.
- The local source applies a configured root, path canonicalisation and limits.
- Every official source applies a credential-free public PDF/CSV URL, complete
  allowlist, SSRF protection, DNS/IP/Host/SNI pinning, TLS policy without
  lateral egress, limits and a snapshot before using the declared-format parser.
- External calls use a typed client, timeout, retry only when safe and
  sanitisation.

### Server/API

Implemented and versioned HTTP v1 surface, described here only at the
architectural level:

| Method and route | Use |
|---|---|
| `POST /api/v1/questions` | Send a question and receive an answer/citations. |
| `GET /api/v1/health/live` | Confirm that the process is alive. |
| `GET /api/v1/health/ready` | Report readiness and dependencies without secrets. |

Ingestion, official synchronisation, activation and rollback will not be
anonymous public endpoints in the MVP. ADR-0006 accepted the non-public local
surface in the main host's one-shot administrative mode. The operation
identifies the operator or environment through operating-system identity,
uses minimum permissions, requires a reason, is idempotent and produces a
sanitised audit. Start-up only loads and verifies the active generation; it
does not ingest, synchronise, activate or roll back except in this explicitly
configured and invoked mode. The decision is not implementation evidence.

The initial public contract for external consumers is HTTP/OpenAPI v1. It is
owned by RAG-Challenge and exposes neither Domain entities nor provider ports.
Conceptually:

```text
QueryRequestV1
  corpusId
  questionLanguage: pt-BR | en-GB
  question

QueryResponseV1
  outcome: Answered | InsufficientEvidence
  answerLanguage: pt-BR | en-GB
  answer?
  citations[]
  evidenceCoverage
  indexGenerationId
  retrievalPolicyVersion
  promptVersion
  languageModelDescriptor
  correlationId
```

Every citation preserves corpus, database, document, version, format, trust
class, generation and location. An official citation includes canonical URL,
snapshot, `revalidatedAt` and freshness; PDF uses pages/blocks and CSV uses
rows/columns/headers. In v1, every citation continues to declare
`contentLanguage=pt-BR|en-GB`; source-derived titles, sections and passages
remain in that original language. `answerLanguage` always equals accepted
`questionLanguage`, including when evidence uses the other language.
`languageModelDescriptor` contains only non-secret provider, model and revision;
it contains no endpoint, credential or internal configuration.

The OpenAPI v1 artefact is owned by RAG-Challenge, generated and versioned with
the API, includes questions, completed responses, citations and Problem
Details, and passes a compatibility test. It remains byte-for-byte unchanged
in this reconciliation. Breaking-change policy belongs to `STATE-02`;
implementation and artefact proof belong to `STATE-04`.

The accepted successor is implemented under later authorities:

```text
QueryRequestV2
  questionLanguage: pt-BR | en-GB

QueryResponseV2
  answerLanguage: pt-BR | en-GB
  citations: CitationV2[]

CitationV2
  contentLanguage: canonical BCP 47 tag
  sourceDeclaredLanguage?: exact observed BCP 47 tag
  pageImages: PageImageEvidenceV1[]
  derivativeObligationPresentation: DerivativeObligationPresentationV1 | null

PageImageEvidenceV1
  obligationSetId: exact immutable obligation-set identity | null
```

`CitationV2` preserves other v1 fields, exposes only PNG references validated
by the active binding and preserves all source text in its original language.
JSON embeds neither bytes nor paths. The read-only same-origin endpoint
revalidates citation, manifest, rights and obligation set before serving the
bounded PNG. Complete presentation remains accessible text alongside the
figure. The language model continues to receive only textual evidence.
OpenAPI v1 remains protected; v2 and notice-bearing implementation authorise
neither a real corpus/provider nor product homologation.

Language fields belong to the query contract and do not select visual
language. The Dashboard separately supports `interfaceLanguage=pt-BR` or
`interfaceLanguage=en-GB`.

Theme also belongs to local Dashboard state, with supported `Light` and `Dark`
values. It is not part of the public query contract and changes no language,
content, scope, answer, evidence or citation.

`QueryResponseV1` represents only a completed query with `Answered` or
`InsufficientEvidence`. `evidenceCoverage` identifies the queried active set
and degraded sources without silently substituting evidence. Invalid input,
absence of any servable set, policy violation, rate limit, provider
unavailability and internal failure are typed Application outcomes mapped by
the API to non-`2xx` Problem Details with stable codes and no sensitive detail.

### Dashboard

- Minimal responsive interface.
- Explicit `interfaceLanguage` selector between `pt-BR` and `en-GB`, with no
  inference from `questionLanguage` or `answerLanguage`.
- Product-owned labels, instructions, validation, states and errors fully
  localised in the selected visual language; citations preserve
  `contentLanguage`.
- Explicit theme selector between `Light` and `Dark`, independent of
  `interfaceLanguage`, `questionLanguage` and queried content.
- Background, surface, text, border, focus and state visual tokens preserve
  contrast, hierarchy and information that does not depend on colour alone in
  both themes.
- Accessible coverage and local/official provenance indicator without creating
  mutually exclusive corpora.
- Loading, empty, error, rate-limit, degraded-coverage, unavailable/stale source
  and insufficient-evidence states.
- Keyboard navigation and visible focus.
- Accessible citations separated from the answer.
- No direct access to vector, LLM or secrets.
- Initial selection, persistence and fallback of `interfaceLanguage`, as well
  as initial theme, system preference, persistence and theme fallback, belong
  to `STATE-05` and remain undecided.
- Output is plain text by default. If Markdown is authorised, it uses a
  sanitised subset, blocks raw HTML, permits only approved URL schemes and
  operates under Content Security Policy.

## Indexing flow

```text
configured logical corpus and catalogue
  -> resolve Candidate database/document versions and retained source snapshots
  -> validate and hash
  -> persist content by hash and reopen/verify
  -> for PDF visual evidence, render and verify the complete page-image manifest
  -> parse
  -> chunk with versioned strategy
  -> embed
  -> write under temporary candidate build identity
  -> finalise canonical manifest with logical artefact digest/counts
  -> validate readback, manifest and smoke queries
  -> atomically bind eligible source/render/index artefacts and compare-and-swap
     the complete CorpusActivationRecord in IIndexGenerationStore
```

Every official binding selected for the candidate preserves snapshot and
observation. Only `Current` bindings and `Active` items participate in the
queryable set. Failure before or during compare-and-swap preserves the prior
generation and bindings. Candidate vectors, content or observations that are
not activated remain auditable orphans until explicit cleanup. The detailed
strategy is in [`RAG-Module.md`](RAG-Module.md).

A partial candidate never has `IndexGenerationId` and is never queryable. Final
identity derives from the specification and logical artefact digest/counts;
distinct outputs do not silently reuse one ID. Staging, idempotent finalisation
and minimum readback evidence belong to `STATE-03`.

The candidate contains an ordered set of all databases and documents intended
for activation. Updates are serialised per corpus; the vector store must filter
`CorpusId`, `IndexGenerationId`, eligible bindings and declared administrative
filters before top-k. Rollback selects a whole retained generation but creates
a new record revision with compatible, currently eligible observations,
without restoring historical freshness. Deactivating or removing the final
active document requires explicit database deactivation in the same atomic
operation.

## Query flow

```text
question
  -> validate and bound
  -> validate pt-BR | en-GB question language
  -> resolve active generation once
  -> resolve all active/current document bindings and coverage
  -> embed query
  -> retrieve top candidates across all active documents by explicit generation ID
  -> apply score/policy checks
  -> build untrusted evidence context
  -> generate constrained answer in question language
  -> validate citations
  -> answer or INSUFFICIENT_EVIDENCE
```

## Configuration

- Common configuration in non-secret files.
- Per-environment overrides and protected variables.
- Secrets only through references or variable names.
- Start-up validates provider, model, dimension, limits, catalogue, content
  store, minimum durability, index compatibility and, when visual capability
  is enabled, profile/renderer/manifests. The official profile validates each
  URL/allowlist record, egress policy and freshness without synchronising.
- An incomplete capability remains disabled; there is no silent fallback.
- A future `.env.example` contains only names and fictitious values.

Candidate sections:

```text
RagChallenge:Corpus
RagChallenge:OfficialSource
RagChallenge:ContentStore
RagChallenge:Parsing
RagChallenge:Chunking
RagChallenge:Embeddings
RagChallenge:VectorStore
RagChallenge:LanguageModel
RagChallenge:Query
RagChallenge:Observability
RagChallenge:Egress
```

## Error handling

Completed `AnswerOutcome` results:

- `Answered`;
- `InsufficientEvidence`.

Initial canonical failure categories:

- `InvalidInput`;
- `CorpusUnavailable`;
- `UnsupportedDocument`;
- `SourceUnavailable`;
- `SourceStale`;
- `SourcePolicyViolation`;
- `ParseFailed`;
- `EmbeddingUnavailable`;
- `IndexUnavailable`;
- `LanguageModelUnavailable`;
- `RateLimited`;
- `ConfigurationInvalid`;
- `OperationCancelled`;
- `UnexpectedFailure`.

The API maps failures to Problem Details without a stack trace, prompt,
document, token or secret. Both `AnswerOutcome` values remain typed `2xx`
responses; failures are not presented as success. Retry occurs only for a
transient failure and idempotent operation. `STATE-02` closes one
`ApplicationFailure → CH_* → HTTP/Problem Details` table; adapters do not
create parallel taxonomies.

## Observability, logging and audit

- Structured logs with correlation ID, operation ID and stable codes.
- Ingestion/synchronisation metrics: duration, databases, documents, formats,
  pages/rows, source/image bytes, render manifests, chunks, freshness, failures
  and version, without raw content.
- Query metrics: per-stage latency, candidates, refusals and provider failures.
- Cost/tokens only when the provider supplies safe metadata.
- Liveness checks only that the process responds and does not depend on an
  external service.
- Global readiness requires at least one active database with one active
  document and a compatible servable generation. `Stale`, `Unavailable`,
  `Withdrawn` or `Deactivated` sources appear as degraded per-source/document
  coverage; only absence of any servable set makes the instance globally
  unavailable. Synchronisation egress is not in the readiness path.
- Audit records relevant sanitised configuration, start/end of
  indexing/synchronisation, snapshot, activation, rollback and provider/version.
- Full questions, passages and answers are not logged by default.

## Infrastructure and CI/CD

`STATE-01` must prepare CI, not automatic deployment. Minimum pipeline:

1. checkout without persisting credentials;
2. pinned toolchains and versions;
3. lockfile restore;
4. format, lint and type checking;
5. Release build;
6. tests and coverage;
7. architecture tests;
8. dependency audit;
9. secret scan;
10. link validation and diff hygiene.

Standard tests use local fake HTTP sources and do not access real official
URLs. Real smoke is opt-in and requires its own network authority.

CD and OCI belong to later states. The initial candidate deployment is one
artefact on the selected OCI service, with external configuration and secrets.
`OFFICIAL_SOURCE_EGRESS` is limited to the exact separately approved active URL
set; `OCI_RUNTIME_EGRESS` separately aggregates only approved official-source,
AI, external vector-store, secret-store, telemetry and operations destinations.
A managed vector store also requires `VECTOR_STORE_EGRESS`; runtime authority
does not broaden the specific policy. GitHub Pages may host only an optional
static frontend; it replaces neither the backend nor OCI use.

## Future DB-Notifier integration

Compatibility is achieved through:

- .NET 10 and equivalent conventions;
- Domain/Application/Infrastructure boundaries;
- typed outcomes and errors;
- fail-closed configuration and providers;
- a versioned HTTP/OpenAPI contract;
- provenance, UTC, cancellation and observability.

The first integration boundary will be a DB-Notifier-owned HTTP adapter that
consumes the RAG-Challenge-owned OpenAPI v1 contract. Request, typed response,
coverage/provenance, citations, reproducibility metadata,
`indexGenerationId` and `correlationId` belong to the public contract; Domain
entities, RAG ports and SDK types do not cross this boundary.

In-process packaging of contracts or use cases may arise only later under its
own ADR and evidence of need. The integration decision will also be recorded
in the consuming repository. RAG-Challenge does not reference DB-Notifier
assemblies, database, events or configuration in the MVP and does not
implement the consuming adapter.

## Independent deployment and operation

- Local: API, Dashboard and dependencies configured by the developer.
- OCI: the same application and contracts, with environment-appropriate
  secrets and storage.
- GitHub: code, documentation and CI.
- GitHub Pages: optional static interface only.

The product must remain functional without DB-Notifier installed.
