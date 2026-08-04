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

## Retention and risk

- Preserve all temporary `S04-A0` evidence until separate cleanup authority.
- Never version nupkgs, raw catalogues, raw registrations, caches, restored
  assemblies, launchers, temporary logs or local paths.
- The retained evidence is exploratory and workstation-local. It does not
  prove current online revocation, Linux ARM64 runtime behaviour, production
  suitability, parser quality over a real corpus or provider behaviour.
- The consolidated `S04-A` to `S04-D` authority has been consumed
  sequentially; its Automatic Quality Gate and subsequent Human Gate are
  approved with the documented limitations. The later corrective increment
  passed its Automatic Quality Gate and is pending the resumed audit.
  `STATE-04` remains closed and no later state is authorised.
