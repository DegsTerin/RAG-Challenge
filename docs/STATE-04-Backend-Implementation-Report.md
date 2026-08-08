# STATE-04 Backend Implementation Report

## Purpose and boundary

This report is the durable, sanitised record for `STATE-04
BACKEND_IMPLEMENTATION`. It records the `S04-A0` dependency assessment and
resolution, selected parsing packages, accepted limitations, sequential
implementation of `S04-A` through `S04-D` and the Automatic Quality Gate. It
is evidence, not an independent authority to install a package, execute a
later state or perform an external action.

Temporary packages, raw catalogue and registration responses, restored
assemblies, caches, launchers, raw logs and local paths remain outside the
repository. They must remain preserved until the owner separately authorises
their cleanup.

## Baseline and authority

- Initial baseline: branch `main`, commit
  `fe6c9028f061a7f0a98fc3debecffb0de3ad69bc`, corpus `4.9.2`, clean working
  tree.
- Consolidated implementation baseline: branch `main`, commit
  `1bb4368ff59521ab61b0d9224b806805e84c6287`, corpus `4.9.2`, clean working
  tree, subject to initial revalidation and cache-seed approval.
- State position: `STATE-04 BACKEND_IMPLEMENTATION` active.
- Owner authority dated 2026-08-04: close `S04-A0`, pin the selected parsers,
  execute `S04-A` through `S04-D` sequentially and then execute the
  `STATE-04` Automatic Quality Gate.
- Negative authority: no network, external provider, account, secret, real
  corpus, real official product source, GitHub, OCI, Dashboard, DB-Notifier,
  publication, deployment, Human Gate or entry into a later lifecycle state.
- Runtime preflight was `NOT_APPLICABLE` for documentary and cache-seed work.
  The targeted implementation preflight was applied before the first
  executable project action and found no RAG-Challenge-owned process or
  listener requiring termination.

## S04-A0 outcome

`S04-A0` is closed for local exploratory development with the following
selection:

- `PdfPig` `0.1.15`: selected PDF parser candidate.
- `CsvHelper` `33.1.0`: selected and sole CSV parser candidate.
- `Sylvan.Data.Csv` `1.4.4`: retained only as a non-selected fallback; it must
  not be referenced, restored or installed under the current authority.
- OpenAI integration: direct HTTP adapter as permitted by ADR-0005; no
  `OpenAI` or `System.ClientModel` package is selected.

The selection is conditional on the independent gates below. It is suitable
only for local `STATE-04` work and is not a production approval.

## Package identity and hash record

| Package | Bytes | SHA-256 of nupkg | Published/raw nupkg SHA-512 (Base64) | Signed/lock content hash (Base64) |
| --- | ---: | --- | --- | --- |
| `PdfPig` `0.1.15` | 11,867,960 | `D35768B69F86CD06CA14B849B8DF3673FD9D2DE3014C391E7C1E69E6B21C59C8` | `M5PHyQHujFuKMOuLQhyp9LNQz36E6r/qrCl86B/YCwM7gCxjW4IFQTxzOnbdoi1BVuvA2LFCJV93+TjjfEtKpg==` | `Bf0NO4o2ZSVnemMyj21KDU1sfSgmamFC0pD4aAv19CDNXQsoO1Z6O99gNqpiK/XWWZ5d5i7JkQoT7PUBxEPz5g==` |
| `CsvHelper` `33.1.0` | 903,000 | `A447D7E28EB7C6EA48EEEBB07BAA9AD393A77B4A49ED46F3CC9A8B8784A18EEF` | `1ZVz2+JluZlqNhJWK2K5CkFKv2MVUmCmmwbZl1XkB6sqdtRoyEJlM+HjT5SGD5IzbUjUew0ZLD2PM5HnebSdeA==` | `kqfTOZGrn7NarNeXgjh86JcpTHUoeQDMB8t9NVa/ZtlSYiV1rxfRnQ49WaJsob4AiGrbK0XDzpyKkBwai4F8eg==` |
| `Sylvan.Data.Csv` `1.4.4` (fallback only) | 154,727 | `809816C0E83533255EE1CA99E177C038537F657ACB933380D2BDB5814D32A814` | `/HQ5JgAeNtlu+dRYJINl+wF1SBw1FzEy6scxQZSO/9CinAZ83b1fT3bYzDaS8CeMdh0O1MfsR/gLOW4QcZ6Bnw==` | `1xZkDQ29uW2Y37NMN2nNkD59pFv8icaiu1WGmbE98m9n6jNL3IO3aM3rK4Uf6RgI+BFKbf7S5DdZA6nu5g9COw==` |

The nupkg sizes and raw hashes were recalculated from the preserved inputs
before this report was written. The independent operational gates are:

1. `RAW_NUPKG_HASH`: the SHA-512 of all nupkg bytes must match the published
   package hash.
2. `CACHE_NUPKG_HASH`: the isolated cache `.nupkg.sha512` value must match
   `RAW_NUPKG_HASH`.
3. `SIGNED_CONTENT_HASH`: the value preserved by offline signature
   verification remains a distinct domain.
4. `LOCK_CONTENT_HASH`: `packages.lock.json` `contentHash` must match
   `SIGNED_CONTENT_HASH`.
5. `SIGNATURE`: the result remains
   `CONDITIONAL_REVOCATION_NOT_CURRENT` because revocation freshness was not
   available offline.
6. `GRAPH`: package identity, exact version, selected target-framework asset
   and applicable dependency graph must remain exact, with no applicable
   transitive package for either selected parser.

## Evidence summary

### Observed locally

- Structural inspection found bounded, safe archive paths and the expected
  NuSpec identities and versions. Licence metadata was reviewed; the
  packaged Sylvan licence was separately checked only because it remained a
  candidate at that time.
- Offline `dotnet nuget verify --all` completed with exit code zero for all
  three candidates. Current revocation status was not established offline.
- Disposable locked restores completed in the corrected `S1-R` evidence.
  The selected assets were `lib/net8.0` for PdfPig, `lib/net9.0` for
  CsvHelper and `lib/net6.0` for the non-selected Sylvan fallback; no
  applicable transitive package was observed.
- Metadata-only inspection, Release `net10.0` compilation and
  framework-dependent `linux-arm64` cross-publish completed in `S1-C1` for
  the exploratory harnesses. No unexpected native import or module
  initialiser was observed. This did not execute the parser assemblies on
  Linux ARM64.
- Controlled runtime testing in `S1-C2` did not reach package loading. The
  attempts were blocked before package resolution by
  `NuGet.Configuration.ConfigurationDefaults` when the child environment was
  cleared too aggressively. This is an orchestration limitation, not an
  observed parser incompatibility.

### Primary-source research and accepted limitation

The four authorised Microsoft Learn pages were retrieved successfully and
reviewed within the fixed allowlist. They documented catalogue and
registration package hashes and PackageReference/cache behaviour, but did not
fully define the relationship among `packages.lock.json` `contentHash`, the
signed content hash and `.nupkg.sha512` for the local observations.

The owner accepted that incomplete primary-source definition exclusively for
local `STATE-04` development. This acceptance does not establish normative
NuGet semantics, does not approve production use and does not remove
`CONDITIONAL_REVOCATION_NOT_CURRENT`.

## First implementation gate

Before parser adapters can be accepted in `S04-A`, deterministic synthetic
runtime tests must cover:

- one-page and two-page valid PDFs, a truncated PDF and a pre-parser
  oversized PDF;
- quoted UTF-8 CSV, the literal cell `=1+1`, an unterminated quote and a
  pre-parser oversized CSV;
- bounded input before parser invocation, sanitised failures, no arbitrary
  path access, literal preservation and no unexpected assembly, dependency or
  side effect.

A valid-case failure requires rollback of the pin and stops the sequence. If
a malformed input is accepted, the owning adapter requires an explicit guard.
CsvHelper must not be replaced automatically by Sylvan.

## Offline pin precondition and resolution

The preserved offline source contains the three parser nupkgs and their
metadata, but it does not contain the repository's already locked .NET
dependency set. An isolated product restore would therefore require at least
one of the following currently unauthorised actions:

- read-only seeding of a task-isolated cache from the existing global NuGet
  cache; or
- an exact, separately authorised offline feed or download operation.

Network use and global-cache access were both prohibited by the initial
authority. Consequently, at that point:

- no parser PackageReference or central version pin was added;
- no product lockfile was changed;
- no `dotnet` restore, build, test or parser loading was started;
- no implementation in `S04-A`, `S04-B`, `S04-C` or `S04-D` was started;
- the `STATE-04` Automatic Quality Gate remains pending.

Attempting the restore with the known incomplete source would not produce
useful evidence and could not meet the owner's isolation gate. Execution must
resume only after the offline dependency source is complete under explicit
authority and the initial baseline is reconciled.

The owner subsequently authorised read-only, allowlisted seeding from the
existing global NuGet cache into a new task-isolated cache. The seed copied
exactly 53 non-project package/version pairs already present in the seven
tracked lockfiles, comprising 2,189 files and 370,721,153 bytes. Every copied
file matched its source SHA-256, each nupkg matched its cache SHA-512, each
cache metadata content hash matched the tracked lockfile and no reparse point
or unexpected expanded file was accepted. The global cache was not changed.

PdfPig and CsvHelper came exclusively from their preserved D1 nupkgs. The
isolated local source contained exactly those two files and did not contain
Sylvan. An offline restore and a second `--locked-mode` restore then passed:

- only `CsvHelper` `33.1.0` and `PdfPig` `0.1.15` were added to the 53-package
  baseline union;
- both selected parser entries have an empty applicable dependency graph;
- CsvHelper selected `lib/net9.0/CsvHelper.dll`;
- PdfPig selected the seven previously inventoried `lib/net8.0` assemblies;
- raw, cache, signed-content and lock-content hash gates remained independent
  and matched their expected values;
- HTTP and plug-in caches remained empty and restored assets contained no
  network source.

The central exact pins and Infrastructure references were applied. The first
`S04-A` runtime gate passed all required synthetic PDF and CSV cases in
memory. Oversized inputs were rejected before any parser read, malformed PDF
and CSV inputs were rejected by explicit adapter guards, the CSV formula
`=1+1` remained literal and no unexpected package assembly or filesystem
side effect was observed.

## S04-A outcome

`S04-A` completed locally with provider-neutral Application use cases and
outer parser/storage adapters:

- local PDF/CSV ingestion writes immutable SHA-256-addressed content, reopens
  and revalidates it before parsing, then emits deterministic traceable chunks;
- catalogue administration requires bounded actor, command, reason, operation
  and UTC context, commits by expected revision and records only a digest of
  the supplemental audit material;
- the fake official-source transport receives a trusted immutable
  registration rather than public URL authority and applies the same parser,
  byte and chunk limits;
- changed content creates an immutable snapshot and append-only observation;
  an identical content hash or `NotModified` response creates only a new
  observation against the existing snapshot;
- response status and sent/received validators contribute to the durable audit
  digest without persisting raw values in the schema;
- parse failure occurs before snapshot or observation commit and cannot
  replace active state;
- repeated content and operation identities are idempotent. A store defect
  found by the test was corrected so two documents in the same catalogue
  revision may safely reference one already tracked content object.

The S04-A verification passed format checking, a zero-warning Release build
and 84 tests: 56 unit, 10 architecture and 18 integration. All sources and
fixtures were synthetic and the official transport was an in-process fake;
no listener or network request was used.

## S04-B outcome

`S04-B` completed the provider-neutral indexing and activation path without
calling or packaging an external embedding provider:

- `IEmbeddingProvider` owns bounded ordered batches and returns an observed
  provider/model/revision/dimension descriptor; any descriptor, count,
  ordering, dimension or finite-value divergence fails the candidate;
- the indexing service orders generation-bound document metadata and chunks,
  creates an inactive candidate, embeds through a deterministic test fake,
  writes bounded batches and finalises only after exact durable readback;
- the final manifest, canonical digests, bindings and embedding descriptor
  digest are committed before activation; activation remains an explicit
  compare-and-swap operation in the control plane;
- exact replay of the same candidate, chunks, generation operation and
  activation operation is idempotent, while any immutable-input divergence is
  rejected;
- vector search now requires `CorpusId`, `IndexGenerationId` and the eligible
  generation-bound bindings. Explicit database/document filters are reduced
  to exact document-version keys and translated into the SQLite query before
  vectors are loaded or cosine ranking and top-k are applied;
- the active binding preserves database, document version, format, adapter and
  trust identity for later retrieval and citation mapping.

The integrated workflow proved inactive staging, validated finalisation,
manifest commit, initial activation, hard denial by a non-authorised database
filter, corpus isolation and exact replay. Format checking, a zero-warning
Release build and 85 tests passed: 56 unit, 10 architecture and 19 integration.
The only embedding implementation exercised was an in-process deterministic
fake; no provider, listener or network request was used.

## S04-C outcome

`S04-C` completed the provider-neutral retrieval and response policy:

- every query validates the configured corpus, canonical `questionLanguage`,
  bounded normalised question and correlation identity before either provider
  port is invoked;
- a query resolves exactly one complete activation record and joins only its
  exact document, product, registration and named observation metadata from
  the local control store; it never selects a separate latest observation or
  fetches an official source;
- local evidence is eligible directly, while official evidence is evaluated
  from the bound observation as current, stale, withdrawn or deactivated;
  coverage records active/eligible databases and documents plus sanitised
  degraded source identities;
- embedding and language-model ports enforce exact observed descriptors,
  dimensions, ordered evidence, fixed top-k/context bounds and typed provider
  unavailability, with deterministic fakes in the standard suite;
- model instructions and untrusted evidence are separate typed fields. Model
  output must preserve `answerLanguage == questionLanguage` and cite only
  retrieved chunk IDs; unsupported citations, invalid language or malformed
  output fail closed to `InsufficientEvidence` with no answer;
- citations are rebuilt server-side from the resolved activation and retrieved
  metadata. They preserve source text and `contentLanguage`, database,
  document/version, format, trust, PDF page or CSV record/column location and
  official URL/snapshot/freshness when applicable;
- chunk citation metadata is stored as a bounded derived envelope in the
  existing vector text column and decoded before canonical generation
  readback, so no schema change or migration was introduced.

Tests cover all four question/evidence language pairs, untranslated citation
content, local PDF and official CSV provenance, prompt-injection text as data,
insufficient evidence, stale-only coverage, embedding/model outages and
hallucinated citation IDs. Recovery readback was updated to decode the derived
metadata envelope before recomputing canonical generation identity. Format
checking, a zero-warning Release build and 96 tests passed: 67 unit, 10
architecture and 19 integration.

## S04-D outcome

`S04-D` completed the bounded public HTTP v1 surface and its provider adapters
without enabling external access:

- `POST /api/v1/questions` accepts only corpus identity, canonical
  `questionLanguage` and a bounded question; strict JSON rejects unknown URL,
  provider, adapter, model, theme and interface-authority fields;
- the endpoint applies an 8 KiB request-body limit, the Application-owned
  4 KiB UTF-8 question limit, a 25-second linked deadline, cancellation, a
  20-query process ceiling and a 30-per-minute token bucket with burst 10;
- completed answers and explicit insufficient-evidence outcomes map to the
  versioned response contract, while the full public Application failure
  taxonomy maps to sanitised RFC 9457 Problem Details with stable `CH_*`
  codes and correlation identity;
- `GET /api/v1/health/live` remains dependency-free. `GET
  /api/v1/health/ready` returns the sanitised readiness contract and fails
  closed until an explicit local composition supplies the control-store,
  activation, document, vector and provider-circuit probe;
- the repository-owned OpenAPI 3.1 v1 artefact includes only the three public
  routes and transport schemas, with no Domain entities, administration
  operation, provider authority or Dashboard state;
- the OpenAI embedding and Responses adapters use direct HTTP only, exact
  approved routes, bounded JSON, structured answer output, provider-side
  storage disabled and handlers with redirects, proxies and automatic
  decompression disabled. They are not enabled by the default composition.

The focused API, health and direct-HTTP contract set passed 27 tests through
in-process services and a fake `HttpMessageHandler`. The complete Release
suite then passed 118 tests: 67 unit, 10 architecture and 41 integration. The
Release build completed with zero warnings and zero errors. No listener,
provider, network request, external source or real corpus was used.

## Automatic Quality Gate

The `STATE-04` Automatic Quality Gate was executed locally and offline on
2026-08-04 over implementation baseline
`main@7f236542133719481a02f507cf802a1dd385f328`, whose Git tree is
`8ef34586d567008510d2d633a927ebc9a9d7f766`.

| Gate | Result | Observed evidence |
| --- | --- | --- |
| Authority and sequence | `APPROVED` | Five implementation commits preserve the authorised order: parser pin, `S04-A`, `S04-B`, `S04-C` and `S04-D`; a sixth focal test commit resolves the gate finding below. No later state or external action was executed. |
| Runtime preflight | `APPROVED` | The targeted preflight was applied before the executable sequence; no RAG-Challenge-owned process or listener required termination. No server listener was started by the gate. |
| Formatting and build | `APPROVED` | `dotnet format --verify-no-changes --no-restore` passed. The .NET 10.0.302 Release build completed with zero warnings and zero errors using the isolated offline cache. |
| Unit, architecture and integration | `APPROVED` | 119 tests passed: 67 unit, 10 architecture and 42 integration; zero failed or skipped. |
| Coverage | `APPROVED` | Merged Cobertura coverage was 92.37% of lines (10,441/11,303) and 65.73% of branches (1,260/1,917), above the 70% and 45% floors. |
| STATE-04 functional scope | `APPROVED` | Tests cover catalogue administration, bounded PDF/CSV ingestion, fake official sync, immutable snapshots, idempotency, candidate indexing, hard pre-filter, CAS activation, retrieval, refusal, bilingual evidence, citations, OpenAPI, health, failures and direct-HTTP provider contracts. One synthetic CSV corpus now crosses ingestion, catalogue commit, index build, activation, query, grounded generation and citation reconstruction in one test. |
| Parser supply chain and locks | `APPROVED_WITH_ACCEPTED_LIMITATION` | Selected D1 nupkg raw hashes were revalidated. Seven tracked lockfiles preserve the accepted signed/lock hashes, exact versions and empty parser dependency graphs. Signature status remains `CONDITIONAL_REVOCATION_NOT_CURRENT`; the accepted NuGet hash-semantics limitation remains local-development-only. |
| API and egress boundary | `APPROVED` | OpenAPI contains exactly the three approved routes and has SHA-256 `D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34`. External composition remains disabled; HTTP adapters were exercised only through a fake handler. |
| Repository hygiene and security | `APPROVED` | A scoped audit excluding the explicitly out-of-scope Dashboard passed for 148 non-ignored files: strict UTF-8/LF, final newline, whitespace, local links, ignored private material and apparent secret assignments. No source or lock reference to Sylvan, `OpenAI` or `System.ClientModel` was found. |
| Dashboard and external validation | `NOT_APPLICABLE` | Dashboard access, network, real providers, real corpus, official sources, GitHub, OCI, DB-Notifier, publication and deployment were explicitly outside this gate. |

The overall Automatic Quality Gate result is `APPROVED`, with zero open
findings. `AQG-S04-001` was classified `P2 Medium` because the first audit pass
found stage-specific coverage but no single test crossing the complete
synthetic backend flow. The authorised local correction added that test and
the repeated build, complete suite and coverage gate passed; the finding is
`RESOLVED`. No P0, P1 or P3 finding was identified. This result does not
execute or imply the Human Gate and does not authorise entry into `STATE-05`.

## Human Gate

The owner reviewed the complete current-baseline summary and confirmed the
canonical phrase for `STATE-04` on 2026-08-04. The Human Gate result is
`APPROVED_WITH_DOCUMENTED_CAVEATS` over
`main@6d141decdf5f40661bb9f408d6aa97f9f322cfcf`, corpus `4.9.2` and a clean
working tree.

The decision accepts the delivered `STATE-04` scope and the residual
limitations below. It closes `STATE-04 BACKEND_IMPLEMENTATION` only. It does
not approve production use, remove the parser signature or NuGet hash-domain
limitations, authorise cleanup of temporary evidence, or authorise entry into
or execution of `STATE-05`.

### Residual limitations and risks

- Online certificate revocation and a current online advisory query were not
  performed; parser signature status remains conditional.
- The primary sources reviewed for NuGet did not normatively reconcile every
  observed lock, signed-content and cache-hash domain. The owner accepted this
  only for local `STATE-04` development.
- Parser runtime behaviour was proved on the local Windows environment with
  synthetic fixtures. Linux ARM64 evidence remains compile/cross-publish only.
- Provider contracts were proved with deterministic fakes. Account
  entitlement, model availability, spend controls, real latency and provider
  output remain unverified and require separate authority.
- No real corpus, source licence activation, real official synchronisation,
  listener-level HTTP E2E, benchmark, deployment or operational recovery was
  tested. Those remain owned by later authorised states and external gates.
- The default host remains deliberately unready until an explicit valid local
  store, activation, vector and provider-circuit composition is supplied.

## Post-Human-Gate audit and corrective increment

A subsequent local, offline audit began on 2026-08-04 over
`main@f71343291b942c66d0ff417a8764b032bbd63bff`, corpus `4.9.2` and a clean
working tree. It identified four actionable findings and stopped under its
authorised finding stop condition before completing the full audit. The owner
then authorised the consolidated, sequential `S04-CORR-01` increment, its
corrective Automatic Quality Gate and, only after that gate passes, resumption
of the complete audit.

| Finding | Corrective evidence | Status before the corrective gate |
| --- | --- | --- |
| `AUD-S04-001` | Commit `a674560ed1093e96d533012f1b11a292c3f641b5` makes unchanged official-source observation rebinding and the new complete activation record one immediate SQLite transaction. Exact replay, withdrawn/mismatched bindings, activation conflicts and injected persistence failures are covered. | `IMPLEMENTED_PENDING_VALIDATION` |
| `AUD-S04-002` | Commit `ac34c085a499a34ea8ee1c9106675482e38790c3` implements the explicit one-shot administrative host mode, strict command lifecycle mutations, per-corpus lease, stable-intent idempotency, bounded single-handle input and durable journal. Successful mutations complete the journal in the same transaction; administrative HTTP routes remain absent. | `IMPLEMENTED_PENDING_VALIDATION` |
| `AUD-S04-003` | Commit `b875eac6e9ce4c72783d4e4bb72a59686ca58248` aligns deterministic chunking to the accepted `paragraph-window-v1` scalar, overlap, hard-limit, boundary, normalisation and compatibility-key contract. | `IMPLEMENTED_PENDING_VALIDATION` |
| `AUD-S04-004` | The current factual state, this owner report and the append-only history now distinguish the historical gate/Human Gate from the later audit and corrections, and no longer claim that the delivered backend is absent. | `RECONCILED_PENDING_VALIDATION` |

These statuses are factual implementation and documentation states, not audit
dispositions. None of the findings is `RESOLVED` until the corrective
Automatic Quality Gate and the resumed complete audit both support that
disposition. The historical Human Gate remains an immutable lifecycle fact;
the corrective increment neither reruns it nor authorises `STATE-05`.

The correction added no external provider call, real corpus, publication or
deployment. The NuGet signature status remains
`CONDITIONAL_REVOCATION_NOT_CURRENT`, all temporary `S04-A0` evidence remains
retained, and the existing real-provider, Linux ARM64 runtime, listener E2E,
performance and operational limitations continue to apply.

## Corrective Automatic Quality Gate

The corrective Automatic Quality Gate was executed locally and offline on
2026-08-04 over `main@114ea6f7f76936dac991553588660fc986bd0f10`, Git tree
`b4d14fab9346574d7db7d92c11ca1e5c0ee363d4`, corpus `4.9.2` and a clean
working tree.

| Gate | Result | Observed evidence |
| --- | --- | --- |
| Authority and corrective sequence | `APPROVED` | C1, C2, C3 and C4 are isolated in commits `a674560ed1093e96d533012f1b11a292c3f641b5`, `b875eac6e9ce4c72783d4e4bb72a59686ca58248`, `ac34c085a499a34ea8ee1c9106675482e38790c3` and `114ea6f7f76936dac991553588660fc986bd0f10`. No package, provider, later state or external action was added. |
| Runtime preflight | `APPROVED` | The directed preflight found no RAG-Challenge-owned `dotnet` or server process and no owned listener. Generic and unrelated processes were not inspected or stopped. |
| Offline restore, format and build | `APPROVED` | Seven lockfiles remained byte-stable after locked restore from a cleared-source configuration and isolated cache. Format verification passed. The .NET SDK 10.0.302 Release build completed with zero warnings and zero errors in an isolated artefact root. |
| Unit, integration and architecture | `APPROVED` | 150 applicable tests passed: 74 unit, 67 integration and 9 architecture; zero failed or skipped in the accepted runs. The Dashboard-specific architecture test was `NOT_APPLICABLE` under the explicit negative scope. |
| Coverage | `APPROVED` | Merged Cobertura coverage was 92.26% of lines (16,580/17,970) and 65.07% of branches (2,079/3,195), above the 70% and 45% floors. |
| Corrective behaviour | `APPROVED` | Tests cover transactional observation rebinding and injected failures; complete `paragraph-window-v1`; strict one-shot commands; exact replay and divergent intent; lifecycle isolation; leases; durable journal success/refusal/unavailability/failure; migration upgrade; and absence of administrative HTTP routes. |
| Migrations | `APPROVED` | Control and vector contexts report no pending model changes. The journal migration creates exactly one table and no columns outside it; both corrective `Up` migrations contain no destructive operation. The initial-to-latest upgrade and backfill are integration-tested. |
| Parsers and lock hashes | `APPROVED_WITH_ACCEPTED_LIMITATION` | Raw D1 and isolated-cache nupkg SHA-512 values for `PdfPig` 0.1.15 and `CsvHelper` 33.1.0 match the accepted values. Lock content hashes match in every occurrence and each parser has no applicable dependency. Signature status remains `CONDITIONAL_REVOCATION_NOT_CURRENT`. |
| API, configuration and repository hygiene | `APPROVED` | OpenAPI retains exactly three public routes and SHA-256 `D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34`; no administrative HTTP route was found. A strict local audit of 161 non-Dashboard files passed UTF-8/LF, newline, whitespace, local links, ignored private material and apparent-secret checks. The Git tree remained clean. |
| External and Dashboard validation | `NOT_APPLICABLE` | No network, provider, account, real corpus, official source, listener, Dashboard, GitHub, OCI, DB-Notifier, publication or deployment was used. |

The first isolated unit-test invocation was invalid as gate evidence because
the alternate artefact root was not an ancestor of two tracked deterministic
fixtures. It produced 72 passes and two fixture-not-found errors before any
product assertion failed. The fixtures were copied into the temporary mirror,
verified byte-for-byte by SHA-256, and the accepted coverage invocation then
passed all 74 unit tests. No source, test or product behaviour was changed.

The overall corrective Automatic Quality Gate result is `APPROVED`, with no
P0 or P1 finding and no open gate failure. `AUD-S04-001` to `AUD-S04-004` are
`CORRECTED_PENDING_AUDIT_DISPOSITION`; only the resumed complete audit may
classify them as resolved. This gate does not rerun the Human Gate, reopen
`STATE-04` or authorise `STATE-05`.

## S04-CORR-02 outcome

The first resumed audit identified five additional persistence, provider and
administration findings and stopped before completing its matrix. The
authorised `S04-CORR-02` sequence corrected them without changing packages,
lockfiles, migrations, public contracts, OpenAPI or ADRs:

- commit `7299722b4259c7384287e5b86f1eec65626a6842` makes physical content
  cleanup consider document versions and official snapshots globally, keeps
  deletion recoverable in quarantine and revalidates reachability before the
  physical action;
- commit `8c661ba094302c551182a6da853306036b50b83d` requires persisted,
  operation-specific replay evidence for official-source, observation and
  generation commits. Generation replay compares the complete
  generation-bound projection defined by ADR-0007; observation identity
  remains activation-bound and is checked by observation/activation replay;
- commit `a4baa22052d7c0fd7787d44820d6a2471a6f5d65` validates exact OpenAI HTTP
  authority, timeout, response status, redirect evidence, media type, body
  limit, model identity, embedding order/dimensions and structured response
  fields, mapping transport and payload failures to sanitised typed outcomes;
- commit `c230c80bd6bdb19752ec7d6f4fb4aec5c76b7ae3` classifies one-shot
  administrative failures by the phase actually reached and the canonical
  exit categories `0`, `2`, `3`, `4`, `5` and `10`;
- commit `3e9d6f9b2c7d7a92d9f1cbaf94d55490bd564092` reconciles only comments and
  evidence known to have become stale.

The subsequent read-only audit found one residual crash-recovery flaw,
`AUD-S04-005-R1`: a reservation could survive a crash before deletion commit,
later regain a durable reference and still be physically deleted by the old
unconditional finalisation path. The audit stopped before disposition, as
required.

## S04-CORR-03 outcome

Commit `19889f560dad0f011006ff17fc7414c807838149` corrects only
`AUD-S04-005-R1`. It adds no migration, dependency, package, lockfile, public
contract, OpenAPI or ADR change. The implementation now:

- publishes an internal canonical `cleanup-plan-v1` atomically before any
  reservation, and adopts it only when operation, corpus, instant, identities,
  byte lengths, ordering and planning audit digest match exactly;
- inventories reservation files as typed content identities, rejects unsafe
  paths, reparse points, duplicate/conflicting canonical files and unplanned
  artefacts, and verifies content length and SHA-256 before restoration or
  finalisation;
- reconciles crash-surviving reservations under an immediate SQLite
  transaction before planning continues and again before operation
  completion;
- restores a reservation whenever its content row or a durable document or
  official-snapshot reference exists, and refuses a durable reference without
  its content row;
- removes the content row only while the global reference set is empty, then
  revalidates under an immediate transaction before the reservation can be
  physically finalised;
- keeps replay exact to the persisted plan and fails closed on missing bytes,
  hash, path, plan, audit, operation or reference inconsistencies.

Ten new deterministic regression tests cover applied and in-progress crash
states, document and official-snapshot references, content shared across
corpora, divergent replay, canonical/reservation conflict, missing or corrupt
bytes, unexpected reservation paths and a reference committed in the TOCTOU
window. The pre-existing cleanup/rollback workflow remains covered.

## S04-CORR-03 Automatic Quality Gate

The complete corrective gate ran locally and offline on 2026-08-04 over
`main@19889f560dad0f011006ff17fc7414c807838149`, Git tree
`40b04e737ebea6e00dab003ff2403e4aa94c4ad2`, corpus `4.9.2` and a clean
working tree.

| Gate | Result | Observed evidence |
| --- | --- | --- |
| Authority and diff | `APPROVED` | The implementation commit changes only the immutable content store, SQLite maintenance implementation and one integration-test file. No prohibited project, package, lockfile, migration, public contract, OpenAPI or ADR changed. |
| Runtime preflight and isolation | `APPROVED` | The directed preflight found no RAG-Challenge-owned application process or listener to stop. Restore, CLI home, package cache, HTTP cache, plug-in cache and artefact outputs were isolated under a task-specific temporary root; global caches were neither changed nor inspected. |
| Offline restore, format and build | `APPROVED` | All seven lockfiles remained byte-identical after locked restore from the allowlisted offline seed. Format verification passed and the .NET SDK `10.0.302` Release build completed with zero warnings and zero errors. |
| Unit, integration and architecture | `APPROVED` | The accepted runs passed 169 applicable tests: 74 unit, 86 integration and 9 architecture; zero failed or skipped. The single Dashboard-specific architecture test was `NOT_APPLICABLE` under the explicit negative scope. |
| Coverage | `APPROVED` | Merged Cobertura coverage was 92.04% of lines (17,423/18,929) and 66.46% of branches (2,421/3,643), above the 70% and 45% floors. |
| Cleanup crash recovery | `APPROVED` | Eleven focused cleanup tests passed, including every newly authorised persisted-crash, global-reference, replay, integrity, path and deterministic concurrency case. No reservation is finalised while its content row or a durable global reference exists. |
| Supply chain and locks | `APPROVED_WITH_ACCEPTED_LIMITATION` | D1 and isolated-cache SHA-512 values for `PdfPig` `0.1.15` and `CsvHelper` `33.1.0` match the accepted raw hashes; cache hash files match those raw bytes and tracked lock hashes remain unchanged. Signature status remains `CONDITIONAL_REVOCATION_NOT_CURRENT`. |
| API, configuration and hygiene | `APPROVED` | OpenAPI still contains exactly the three approved public routes with SHA-256 `D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34`. External and administrative composition remain disabled by default. A strict audit of 162 applicable non-Dashboard files found no UTF-8/LF, final-newline, trailing-whitespace, NUL, local-link, private-material or apparent-secret failure. |
| External and Dashboard validation | `NOT_APPLICABLE` | No network, provider, account, real corpus, official source, listener, Dashboard, GitHub, OCI, DB-Notifier, publication or deployment was used. |

An initial all-test invocation placed outputs outside the repository ancestry
used by repository-fixture discovery. It produced 158 passes and ten
root/fixture discovery errors and was rejected as gate evidence. Exact
SHA-256-validated copies of the required repository fixtures were then placed
in the isolated mirror, and the accepted 169-test sequence passed without a
source or product change.

## Complete post-correction audit

After the corrective gate passed, the complete `STATE-04` audit restarted
from the beginning in read-only mode. It inspected authority, lifecycle,
commits and diffs; architecture and dependency direction; packages, hashes and
lockfiles; parser bounds; ingestion, immutable content and SQLite invariants;
index staging, finalisation, activation and hard pre-filtering; retrieval,
refusal, grounded generation, citations and both supported languages; API v1,
OpenAPI, health, limits, cancellation and rate limiting; direct-HTTP adapters,
fail-closed configuration, sanitisation, tests, coverage, end-to-end evidence,
documentation and repository hygiene.

| Audit area | Result | Disposition and evidence |
| --- | --- | --- |
| Authority and lifecycle | `APPROVED` | Every corrective commit is within an explicit owner authority. `STATE-04` remains historically closed after its Human Gate; no later lifecycle entry or Human Gate was executed. |
| `AUD-S04-001` to `AUD-S04-004` | `RESOLVED` | Transactional observation rebinding, governed one-shot administration, complete `paragraph-window-v1` and factual documentation are implemented and exercised by the accepted full suite. |
| `AUD-S04-005` | `RESOLVED` | Global reachability covers document versions and official snapshots across corpora; physical cleanup is quarantined, revalidated and fail-closed. |
| `AUD-S04-005-R1` | `RESOLVED` | Versioned cleanup plans and transactional reservation reconciliation restore newly referenced content and prevent unconditional post-crash deletion. |
| `AUD-S04-006` | `RESOLVED` | Official-source, observation and generation operations require exact persisted replay evidence in their respective ADR-0007 identity domains. |
| `AUD-S04-007` | `RESOLVED` | OpenAI HTTP transport and response-policy violations are bounded, typed and sanitised; only fake handlers were exercised. |
| `AUD-S04-008` | `RESOLVED` | Administrative outcomes are classified by phase and map to the canonical exit categories. |
| `AUD-S04-009` | `RESOLVED` | Proved-obsolete comments were reconciled without broad documentary rewriting. |
| Functional and quality matrix | `APPROVED` | Format, Release build, all applicable unit/architecture/integration tests, coverage floors, full synthetic backend flow, parser gates, migration regressions, OpenAPI and hygiene checks pass. |

No new P0, P1, P2 or P3 finding was identified. The complete audit result is
`APPROVED`; all findings `AUD-S04-001` through `AUD-S04-009`, including
`AUD-S04-005-R1`, are `RESOLVED`. This disposition does not rerun or amend the
historical Human Gate, authorise production, remove accepted caveats, reopen
`STATE-04` or authorise `STATE-05`.

The audit did not access the Dashboard or external systems and did not test a
real provider, account, corpus, official source, listener-level deployment,
Linux ARM64 runtime, benchmark or operational recovery. Provider handlers and
credentials remain a future explicit composition responsibility; the default
host is fail-closed and unready. No separate `dotnet-ef` model-diff command was
run because that additional tool was not present in the isolated offline
allowlist; the correction contains no model change, tracked migrations and
snapshots are unchanged, and the full migration integration tests passed.

## Retention and risk

- Preserve all temporary `S04-A0` evidence until separate cleanup authority.
- Never version nupkgs, raw catalogues, raw registrations, caches, restored
  assemblies, launchers, temporary logs or local paths.
- The retained evidence is exploratory and workstation-local. It does not
  prove current online revocation, Linux ARM64 runtime behaviour, production
  suitability, parser quality over a real corpus or provider behaviour.
- The consolidated `S04-A` to `S04-D` authority has been consumed
  sequentially; its Automatic Quality Gate and subsequent Human Gate are
  approved with the documented limitations. `S04-CORR-01`, `S04-CORR-02` and
  `S04-CORR-03` passed their authorised corrective gates, and the complete
  restarted audit resolved every recorded `AUD-S04-*` finding. `STATE-04`
  remains closed and no later state is authorised.

## S04-CORR-04-A outcome

`S04-CORR-04-A` ran locally, offline and sequentially under
`AUTH-S04-CORR-04-A-001` on 2026-08-07. The mandatory baseline was
`main@ea7fc582f991bb9290e26a7e2d4e074abc46bf3c`, corpus `4.9.7` and a clean
working tree. The directed runtime preflight found no RAG-Challenge-owned
product process or listener on the known project ports, so nothing was
stopped before implementation.

Commit `26f2e154b736687693b31ab02ca59cfb8ba86655` implements only verified
content-object descriptors:

- Application now owns `IDocumentContentStore`, `BoundedContentInput`,
  `ExpectedHashAndLength`, `ContentObjectDescriptor`, `VerifiedContentObject`
  and the validated media-type, implementation and verification value types;
- the filesystem implementation hashes a bounded stream while writing to
  same-volume quarantine, flushes it, publishes by atomic move, verifies an
  idempotently pre-existing object and always reopens and fully rehashes the
  object before reporting success;
- verified reopen checks the requested identity, expected SHA-256 and byte
  length, recomputes the complete SHA-256 and returns the stream at position
  zero;
- ingestion accepts only media types compatible with PDF or CSV, parses only
  the independently reopened stream and propagates the verified descriptor to
  local and fake official-source consumers;
- `IntegrationRuntime` and activation validation use the new port and exact
  expected length; and
- storage maintenance uses the same verified reopen where content is restored,
  while `IStorageMaintenance`, `cleanup-plan-v1` and the existing reservation
  and finalisation protocol remain unchanged as deletion authority.

Focused verification passed three unit tests for the provider-neutral value
contracts and 57 directly affected integration tests for content storage,
ingestion, control-plane activation, restart, official-source and cleanup
paths. The accepted `pwsh -NoProfile -File eng/ci.ps1 -Offline` run passed 109
unit, 118 integration, 10 architecture and 38 Dashboard tests, with 93.76% line
coverage (23,147/24,688), 67.15% branch coverage (2,752/4,098), a zero-warning
Release build and a successful audit of 213 non-ignored files.

One preliminary CI invocation was rejected as evidence because its command
executor expired after approximately five seconds. The only surviving
processes were the task-owned CI shell and its `dotnet format` child; both
were identified by command line and stopped, no known project listener was
open, and the complete accepted invocation above then passed.

No package, lockfile, schema, migration, persisted render manifest, activation
digest, endpoint or public v1 contract changed. `docs/api/openapi-v1.json`
remained byte-identical at SHA-256
`d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
No renderer or PNG was produced, no rights record or real source was touched,
and no network, provider, dataset, indexing, activation, external action,
Automatic Quality Gate, Human Gate or lifecycle transition occurred.

## S04-CORR-04-B outcome

`S04-CORR-04-B` ran locally, offline and sequentially under
`AUTH-S04-CORR-04-B-001` on 2026-08-07. The mandatory baseline was
`main@196bbcafcb493ce4e45a2c9e784965ff933f124d`, corpus `4.9.8` and a clean
working tree. The directed runtime preflight found no RAG-Challenge-owned
product process or listener on the known project ports, so nothing was
stopped before implementation.

Commit `a886a944ecd1ce485eee9c072385e96210e90520` implements only typed
document-rights eligibility contracts and fixed fail-closed gates:

- `DocumentRightsEligibilityRecordV1` binds schema version `1` to one exact
  `DocumentId` and `DocumentVersionNumber` and requires every independent
  ADR-0008 rights decision exactly once;
- `DocumentRightDecisionState` is closed to `Permitted`, `Denied` and
  `Unproven`; each decision has a stable evidence reference while licence text,
  paths, URLs, policy authority and persistence remain outside the contract;
- the rights set covers possession/download, parsing/text transformation,
  indexing, source retention, quotation/citation, page rendering, derivative
  creation/retention, runtime display, distribution/publication and
  attribution/notice/trademark/change-marking requirements;
- the fixed `TextualEvidence` and `PdfVisualEvidence` policies accept only
  `Permitted` required decisions and return every blocking `Denied` or
  `Unproven` decision; and
- distribution/publication remains independently gated and is not inferred
  from textual or visual eligibility.

Focused verification passed all 14 synthetic rights-contract and gate cases,
including complete/duplicate/missing records, closed states, evidence-reference
bounds, every individual right, textual requirements, visual requirements and
both blocking states. The accepted
`pwsh -NoProfile -File eng/ci.ps1 -Offline` run passed 123 unit, 118
integration, 10 architecture and 38 Dashboard tests, with 93.72% line coverage
(23,263/24,821), 67.20% branch coverage (2,766/4,116), a zero-warning Release
build and a successful audit of 216 non-ignored files.

No package, lockfile, schema, migration, renderer, PNG, persisted rights or
render-manifest record, activation binding/digest, endpoint or public contract
changed. `docs/api/openapi-v1.json` remained byte-identical at SHA-256
`d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
No real source, document, licence, right or data was registered or changed;
there was no import, indexing, activation, serving, network, provider, external
action, Automatic Quality Gate, Human Gate or lifecycle transition.

## S04-CORR-04-C outcome

`S04-CORR-04-C` ran locally and sequentially under
`AUTH-S04-CORR-04-C-001` on 2026-08-07. The mandatory baseline was
`main@75475c391c7fc1fb5ff298492a5d1da4c4f99fbb`, corpus `4.9.9` and a clean
working tree. The directed runtime preflight found no RAG-Challenge-owned
product process or listener, so nothing was stopped.

### Selected supply chain

The mandatory supply-chain gate completed before the first repository edit.
It used isolated NuGet packages, HTTP cache, CLI home and artefact roots. The
temporary evidence remains outside Git and was not cleaned up. Only the eight
authorised nupkgs were downloaded from the official NuGet v3 endpoint.

| Package | Exact version | Raw nupkg SHA-256 | Published repository commit |
|---|---:|---|---|
| PDFtoImage | 5.3.0 | `0264d39c019cff547071f212f433165a3d48cdc43dbf4fbfd86f5e64b7cead8e` | `cf26039d5808ecc276d32217f473b014f78068fe` |
| bblanchon.PDFium.Linux | 153.0.7988 | `1a0e738a22ca3d732af758d7f33707a02975ed010f45a21a55985412152a7751` | `c6529b58791d142002f819beb46e370e668797d7` |
| bblanchon.PDFium.Win32 | 153.0.7988 | `0232ff7c55caa16fd4d5f6423687eb005374cb8cb5d016123a56c48ae1bd5d29` | `c6529b58791d142002f819beb46e370e668797d7` |
| bblanchon.PDFium.macOS | 153.0.7988 | `ed3731eb546141d62ac22384b8a87f99642ddc0dc0fa59568e1a4fe3f2ec2739` | `c6529b58791d142002f819beb46e370e668797d7` |
| SkiaSharp | 4.151.1 | `2d1feef23f28e55864cad8449f7b60abf5d6db1aa61ec07aef837e9e0eaee73e` | `279f93f4ffa7f9fe4e9c0bc298bedc3c9e439764` |
| SkiaSharp.NativeAssets.Linux.NoDependencies | 4.151.1 | `f33aa111ff4241cf8cb03797101defc0e5aadeb1d6bb008077788543fb8b029a` | `279f93f4ffa7f9fe4e9c0bc298bedc3c9e439764` |
| SkiaSharp.NativeAssets.Win32 | 4.151.1 | `a6e9479555440ed8fd30ee7378470144521847aea479e1efd885f3c8013fe458` | `279f93f4ffa7f9fe4e9c0bc298bedc3c9e439764` |
| SkiaSharp.NativeAssets.macOS | 4.151.1 | `9ede7cfbfb783b29d8a98f7db233b1b10c032c917cf270fc67e5b08f90dabdd5` | `279f93f4ffa7f9fe4e9c0bc298bedc3c9e439764` |

Raw nupkg and isolated-cache hashes matched. `dotnet nuget verify --all`
returned exit code zero for every package. PDFtoImage and SkiaSharp carry
author and NuGet repository signatures; the PDFium packages carry NuGet
repository signatures. The current verification emitted no revocation
limitation warning, but it is point-in-time local evidence and does not assert
future revocation status.

PDFtoImage and SkiaSharp declare MIT. The PDFium nupkgs declare Apache-2.0;
their official packaging repository is separately MIT. This distinction is
consistent with the authorised licence set. The PDFium archives contain no
embedded licence or notice file; the SkiaSharp native archives include licence
and third-party notices. Package metadata, upstream repositories and commits
were mutually consistent.

The resolved `net10.0` graph contains only the selected direct package and its
seven centrally pinned transitive packages. Every resolved version and
`contentHash` in the four affected repository lockfiles matches the isolated
gate lock. Structural archive inspection found no executable download target.
The applicable Windows PDFium and SkiaSharp assets were present. The Linux
arm64 PDFium and SkiaSharp libraries are ELF64 AArch64 (`e_machine=183`). The
resolved graph reported no current vulnerability or deprecation.

### Implementation

Commit `981e61c3308ee3407769d10ab1fa554007f12799` implements the bounded
render-candidate path:

- Application owns typed render limits, renderer/page ports, deterministic
  descriptor identity, sanitised failure outcomes and the finalisation use
  case;
- the use case fails closed at `PdfVisualEvidence`, reopens the expected source
  object, validates the complete page set before publication, writes and
  reopens every PNG, then commits and reads back one canonical manifest;
- the selected adapter fixes 144 DPI, no requested bounds or dimensions,
  `Rotate0`, annotations and form fill disabled, all anti-aliasing, white
  background, no tiling, absolute DPI and non-grayscale rendering;
- the existing Server.Api executable returns into internal
  `--pdf-render-worker-v1` mode before HTTP host construction and processes at
  most one document, one page at a time;
- the parent applies an elapsed timeout and complete-tree termination. A
  Windows Job Object applies process CPU, process-memory, one-process and
  kill-on-close limits before source bytes are sent. The Linux worker applies
  CPU, address-space, output-file, core-dump and file-descriptor limits plus
  non-dumpable process state after its bounded fixed header and before the PDF
  body is read;
- the worker environment is cleared before launch and receives only disabled
  .NET diagnostics settings. The private binary protocol accepts bytes and
  bounded numeric policy fields, never a PDF-supplied path, URL or ambient
  authority;
- PNG validation recomputes CRCs, enforces chunk ordering, 8-bit opaque RGB,
  no transparency chunk, positive dimensions no greater than 4096, expected
  rendered dimensions/aspect ratio, complete scanlines, a technical ancillary
  allowlist and rejection of metadata-bearing or unknown chunks; and
- the existing SQLite schema atomically writes the manifest, page bindings and
  required content-object rows. Exact replay is idempotent; a conflicting
  identity fails closed; readback recomputes the canonical identity.

The renderer descriptor binds the exact renderer/package versions, effective
RID, profile, pixel-affecting options, worker concurrency and every limit by a
canonical digest. It contains no host, path, command, credential or workstation
version. The worker contains no runtime network operation or network
configuration; this increment did not claim an operating-system network
sandbox.

`IDocumentContentStore`, `ContentObjectDescriptor`, `IStorageMaintenance`,
`cleanup-plan-v1` and the existing reservation/finalisation protocol were
preserved. The render path does not call cleanup. An immutable PNG left orphaned
by a later failure grants no deletion authority.

### Verification and boundary

Seven focused Application tests passed for policy/descriptor identity, both
blocking rights states, source mismatch, all-pages-before-publication,
incomplete ordering and verified idempotent finalisation. Ten focused
integration tests passed for synthetic one- and multi-page PDFs, 144 DPI,
declared rotation, aspect ratio, white background, disabled annotations/form
fill, 4096/4097 dimensions, PNG policy, repeat byte equality, worker mode,
cancellation/crash/truncated framing, memory limit, content reopen, replay and
atomic manifest rollback. Architecture tests passed 10/10.

A locked/offline framework-dependent `linux-arm64` restore and publish passed.
The publish output selected only `libpdfium.so` and `libSkiaSharp.so` as the
renderer native assets; both are ELF64 AArch64 and no Windows or macOS native
renderer asset was published.

The accepted `pwsh -NoProfile -File eng/ci.ps1 -Offline` run passed 130 unit,
128 integration, 10 architecture and 38 Dashboard tests, with 93.53% line
coverage (24,240/25,918), 66.80% branch coverage (3,006/4,500), a zero-warning
Release build and a successful audit of 223 non-ignored files.

Exactly four lockfiles changed: Infrastructure, Server.Api, Architecture.Tests
and IntegrationTests. No project, solution, schema, migration, model snapshot,
activation binding/digest, endpoint, v2 contract or public v1 behaviour
changed. `docs/api/openapi-v1.json` remained byte-identical at SHA-256
`d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.

All PDFs and PNGs used by tests were synthetic and disposable. No real source,
document, licence, right or data was registered or altered. No import,
indexing, activation, serving, distribution, cleanup, provider, account,
secret, OCI, authenticated GitHub operation, publication, deployment, push,
Automatic Quality Gate, Human Gate or lifecycle transition occurred. Linux
arm64 evidence is static publish/asset evidence, not execution of the worker on
Linux or production suitability.

## S04-CORR-04-D outcome

`S04-CORR-04-D` ran locally, offline and sequentially under
`AUTH-S04-CORR-04-D-001` on 2026-08-07. The mandatory baseline was
`main@548a817e2db4d9bad2d1a63e7dc9e9bb9ace418c`, corpus `4.9.10` and a
clean working tree. The directed runtime preflight found no product process or
listener proved to belong to RAG-Challenge, so nothing was stopped.

Commit `d18224e46f559229a58e82b097abbf16ea9f359a` implements the complete
internal activation-evidence binding:

- every new activation revision contains the exact `DocumentBinding`, source
  `ContentObjectId`, immutable schema-v1 snapshot of all ten rights decisions
  and a render-manifest ID required for PDF and forbidden for CSV;
- Initial, Replacement and Rollback require every evidence binding explicitly.
  Rollback constructs a new revision and revalidates current rights, source,
  generation and render manifest; observation-only rebinding preserves the
  immutable evidence only when document, version, generation and manifest are
  identical;
- pre-CAS readback matches the document/version/format/source identity and
  runtime-supported content language, applies `TextualEvidence` for CSV or
  `PdfVisualEvidence` for PDF, matches the finalised textual/vector generation
  and, for PDF, rehydrates the consecutive finalised page set and reopens every
  PNG through `IDocumentContentStore`;
- exact `OperationId` replay compares all new evidence and rights fields;
- one Control transaction writes the activation record and document bindings,
  evidence and rights rows, retention, activation head, sanitised audit and
  applicable administration-journal completion; and
- the query activation reader fails closed when the current revision lacks a
  complete new binding or its source, rights or render manifest diverges.

The single Control migration is
`20260808004846_AddDocumentRightsAndActivationEvidenceBindings`. It creates
only `activation_evidence_bindings` and `activation_rights_decisions` with the
required keys, foreign keys and closed constraints. It performs no data
operation, does not backfill historical activation rows and leaves all existing
activation columns and the Vector schema unchanged. Historical revisions can
still be rehydrated without inference but cannot authorise current query or
visual readiness unless their complete new bindings exist.

Focused unit selections and 15 focused integration cases passed for PDF/CSV
rights and source eligibility, render-manifest/page/object failures, all four
activation mutation kinds, exact/divergent replay, CAS and injected persistence
failures, one-shot administration, restart/readback and legacy migration
compatibility. Disposable SQLite verification passed previous-migration
upgrade, rollback, reapplication, `foreign_key_check` and no-pending-model
checks for both Control and Vector.

The accepted `pwsh -NoProfile -File eng/ci.ps1 -Offline` run passed 135 unit,
137 integration, 10 architecture and 38 Dashboard tests. Merged coverage was
94.34% of lines (27,189/28,819) and 67.25% of branches (3,174/4,720); the
Release build and audit of 226 non-ignored files passed. Earlier invocations
that ended at the command timeout, generated-migration formatting check or
encoding audit were not accepted as final evidence; the generated artefacts
were normalised and the complete check was rerun after the final source-binding
foreign-key/readback hardening.

No dependency, package, lockfile, Dashboard, accepted ADR, Governance,
Lifecycle, Quality Gate, vector metadata or canonical digest semantics changed.
`sourceBindingSetDigest` and `activationBindingSetDigest` retain their existing
fields. OpenAPI v1 remained byte-identical at SHA-256
`d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`
and Git blob `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`.

All evidence was local, offline and synthetic. No real source, document,
licence, right, PDF, PNG or product data was imported or changed; no v2
contract, image serving, `AnswerEvidenceRecord`, provider, network, account,
external action, Automatic Quality Gate, Human Gate or lifecycle transition
occurred. `S04-CORR-04-E` was not started.

## S04-CORR-04-E outcome

`S04-CORR-04-E` ran locally, offline and sequentially on 2026-08-08 under the
owner's explicit ADR-0010 implementation authority. The mandatory baseline was
`main@fc83e1ea6922a519baf527efc3f0a219e2674453`, corpus `4.10.0`, a clean
working tree and the protected OpenAPI v1 SHA-256. The directed runtime
preflight found no product process or listener proved to belong to
RAG-Challenge, so nothing was stopped.

The implementation adds the immutable `AnswerEvidenceRecordV1` model and its
canonical length-delimited UTF-8 serialisation. The canonical digest excludes
only its self-referential field; the record ID is a server-generated
`ans-evidence-<uuid-n>` value. Creation validates the fixed schema, exact
activation/catalogue/generation and binding identities, answer hash and byte
length, canonical coverage, ordered citations, and every cited PDF page against
the complete render manifest. `expiresAt` is exactly `createdAt + P30D` and is
never refreshed.

Application composition creates a record only after a complete `Answered`
result and all language, coverage, citation and activation-evidence checks have
passed. It persists and reads the complete record back before returning the
existing public v1 response. `InsufficientEvidence`, validation failures,
provider failures and cancellation do not create a record. Persistence or
readback failure prevents `Answered` and uses the existing public failure
taxonomy.

The SQLite Control store uses one immediate transaction for the administration
operation, header, citation rows, page rows and sanitised audit event. Exact
same-ID canonical replay returns `AlreadyApplied`; a divergent record under the
same ID returns a no-change conflict. Authoritative activation, source,
generation, manifest and page bindings are revalidated before insertion, and
the canonical record plus its allowlisted operation/audit evidence are read
back before commit.

The single Control migration is
`20260808033247_AddAnswerEvidenceRecords`. It creates only the empty
`answer_evidence_records`, `answer_evidence_citations` and
`answer_evidence_pages` tables with their constraints, indexes and foreign
keys. It performs no data operation, backfill or historical inference and does
not change the Vector schema.

Non-expired answer-evidence records are independent reachability roots for
their bound source and page-image objects. Expired records are captured in the
existing `cleanup-plan-v1` and removed only inside the reservation transaction
after exact plan adoption and complete reachability revalidation. A record
created after a stale plan blocks deletion. Expiry removes only that temporary
root and never grants physical deletion authority by itself.

The persisted record, default log and audit surfaces retain only the ADR-0010
allowlist. They omit the question and its hash, answer text, citation title,
excerpt and URL, prompts and provider payloads, raw scores and vectors,
user/client identity or IP, secrets, paths and document/image bytes.

A zero-warning Release build passed. The complete direct test run passed 146
unit, 153 integration and 10 architecture tests. Focused coverage includes the
canonical golden digest, restart, concurrency, exact and divergent replay,
five injected persistence boundaries, migration rollback/reapply, privacy at
the SQLite byte level, fixed-retention boundaries, independent reachability
and the stale-cleanup-plan race. Both Control and Vector reported no pending
model change from fresh Debug output. These direct checks were not an Automatic
Quality Gate.

All fixtures and stores were synthetic and disposable. OpenAPI v1 remained
byte-identical at SHA-256
`d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
No endpoint, payload, outcome or public `CH_*` code changed. No v2 contract,
serving, real data, network access, external action, Automatic Quality Gate,
Human Gate or lifecycle transition occurred.

## S04-CORR-04-E Automatic Quality Gate

The corrective Automatic Quality Gate was executed locally, offline and
sequentially on 2026-08-08 over `main@990d14172954567456d9ad90b6a767f6b6e0da78`,
corpus `4.10.1` and a clean working tree. The protected OpenAPI v1 artefact was
confirmed before the audit at SHA-256
`d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.

The mandatory static inspection identified `AQG-S04-002` (`P2 Medium`) in
[`STATE-02-Canonical-Contracts.md`](architecture/STATE-02-Canonical-Contracts.md).
Lines 12–13 state that persistent answer evidence remains an unimplemented
successor capability, while the same authority at lines 537 and 597–600 states
that the internal contract is implemented locally by `S04-CORR-04-E`. The data
dictionary, RAG module, Current State and this report also record that local
implementation. The canonical contract is therefore internally contradictory
about the implementation status of the gate subject.

The finding has limited runtime impact but creates a material maintenance and
audit risk because one current architecture authority supplies mutually
exclusive factual status. It remains `OPEN`. No wording or other implementation
was corrected under the gate authority.

The owner-mandated stop condition was applied immediately after confirming the
finding. Runtime preflight was not reached because no executable check was
started; accordingly, no process or listener was inspected or stopped.
`eng/ci.ps1 -Offline`, build, tests, coverage, migration, restart, concurrency,
failure-injection, retention, cleanup, privacy and reachability checks were not
executed by this gate. Their prior direct implementation evidence was not
reclassified as Automatic Quality Gate evidence.

The reproducible static evidence was obtained with `Select-String` and bounded
line reads of the tracked canonical contract, Current State, data dictionary,
RAG module and this report. Git branch, HEAD and porcelain status plus
`Get-FileHash -Algorithm SHA256 docs/api/openapi-v1.json` confirmed the five
baseline elements before and after the inspection.

The overall corrective Automatic Quality Gate result is `REJECTED`, with one
new P2 finding and no P0, P1 or P3 finding identified before the mandatory
stop. OpenAPI v1 remained byte-identical. No v2 contract, image serving, real
data, correction, Human Gate, lifecycle transition, network access or external
action occurred.
