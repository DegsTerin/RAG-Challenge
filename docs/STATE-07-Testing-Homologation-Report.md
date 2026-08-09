# STATE-07 Testing and Homologation Report

## Purpose and authority

This report reconciles `S07-A` A1-A4, the single local, offline, deterministic
and sequential A3 campaign executed under `AUTH-S07-A-RUN-001`, the A5
verification approved under `AUTH-S07-A-A5-RETEST-002` on 2026-08-08, and the
Automatic Quality Gate approved under `AUTH-S07-A-AQG-RETEST-003` on
2026-08-09. The A5 documentary update was authorised by
`AUTH-S07-A-A5-RECONCILE-001`; the gate result is recorded under
`AUTH-S07-A-AQG-RECONCILE-001`. The report is factual evidence for the
synthetic `ENV-S07-A-LOCAL-01` boundary only.

This report does not claim product-corpus quality, real-provider quality,
performance, security, accessibility, Linux, OCI or production homologation.
It is not a Human Gate, a lifecycle transition, publication or deployment
authority.

It also records the separately authorised HTTP/OpenAPI v2 and same-origin
visual-evidence serving contract, implementation, focal correction and
Automatic Quality Gate. That evidence remains limited to its local, offline,
deterministic and synthetic verification boundary.

The governing inputs are the
[S07-A proposal](STATE-07-S07-A-Evaluation-And-Security-Proposal.md),
[dataset manifest](evaluation/rag-eval-catalogue-v1/dataset-manifest.json),
[document manifest](evaluation/rag-eval-catalogue-v1/document-manifest.json),
[case inventory](evaluation/rag-eval-catalogue-v1/case-inventory.json) and
[ADR-0004](architecture/ADR-0004-MVP-Corpus-Official-Source-And-Evaluation.md).

## Authority, baseline and scope

| Fact | Observed value |
| --- | --- |
| A3 authority | `AUTH-S07-A-RUN-001` |
| A4 authority | `AUTH-S07-A-RECONCILE-001` |
| A5 authority | `AUTH-S07-A-A5-RETEST-002` |
| A5 documentary reconciliation authority | `AUTH-S07-A-A5-RECONCILE-001` |
| Automatic Quality Gate authority | `AUTH-S07-A-AQG-RETEST-003` |
| Gate documentary reconciliation authority | `AUTH-S07-A-AQG-RECONCILE-001` |
| Repository | `C:\Projects\RAG-Challenge` |
| Branch | `main` |
| A3 and A4 baseline `HEAD` | `43ddc0de4a6c10b32a657f3c1e471a743cb42b5f` |
| A5 and documentary baseline `HEAD` | `6cd939849909a8abf2c5dd0534244da5f19be833` |
| Automatic Quality Gate baseline `HEAD` | `a6626a363713b4fbcf83387b7b2104eae1f3e918` |
| Prompt corpus | `4.10.1` |
| Working tree before A3 | clean |
| Working tree before A4 | clean |
| Working tree before A5 | clean |
| Working tree before each documentary reconciliation | clean |
| Working tree before the Automatic Quality Gate | clean |
| Lifecycle | `STATE-07 TESTING_HOMOLOGATION` active |
| A4 runtime preflight | `NOT_APPLICABLE` — documentary reconciliation only; no process or listener was inspected or stopped |
| A5 runtime preflight | applicable; no RAG-Challenge-owned process or listener was found or stopped |
| Automatic Quality Gate runtime preflight | applicable; no RAG-Challenge-owned process or listener was found or stopped |
| Current runtime preflight | `NOT_APPLICABLE` — documentary reconciliation only; no process or listener was inspected or stopped |

A3 was limited to the frozen local synthetic campaign. A4 was limited to
reading its frozen inputs and ignored task-owned evidence and creating this
report. A5 reconciled A1-A4, recalculated the frozen identities and aggregates,
and executed only the three authorised local checks. Neither A4, A5 nor this
documentary reconciliation altered the frozen inputs or retained A3 evidence.

The Automatic Quality Gate audited A1-A5, their commits, the reconciled factual
state, the frozen manifests and the ignored evidence before executing its three
authorised commands. It did not rerun A3 or alter any tracked or ignored input.

The preserved negative scope includes an A3 rerun, `-Mode Run`, threshold
changes, dataset or manifest changes, source or test changes during A5,
dependencies, lockfiles, public contracts, OpenAPI, schema, migrations, ADRs,
real providers, real sources, network access, browser use, OCI, GitHub,
publication, deployment, Automatic Quality Gate, Human Gate and lifecycle
change.

## A1-A5 sequence and commits

| Activity | Authority or commit | Reconciled result |
| --- | --- | --- |
| A1 dataset candidate | `968f69c2d9c37959d617742af5ac48aee5ca09d5` | candidate materialised within the authorised local boundary |
| Deterministic local harness preparation | `ae8d96487fe719d89741aa33e5607e532301d60e` | harness materialised |
| Freeze-safe harness validation correction | `18994db15d963b321ace93b0069436ffc4813b53` | validation aligned with the frozen boundary |
| A2 freeze | `43ddc0de4a6c10b32a657f3c1e471a743cb42b5f` | dataset and manifests frozen |
| A3 execution | `AUTH-S07-A-RUN-001`; no tracked commit | 11 of 11 synthetic cases passed; eight ignored evidence files retained; result SHA-256 `9efc2eef05388433af58e01242a1b1589556c43620eeec509f583fba0c2073bc` |
| A4 reconciliation | `760bbcf4626b7890ffdfb0eeb0a8c5419b5feec7` | campaign evidence reconciled in this report |
| Retained-workspace validation correction | `275becfb04a4d0f7a1703c3be3f4c59d87550cc2` | phase-aware validation committed |
| Future evidence line-ending correction | `6cd939849909a8abf2c5dd0534244da5f19be833` | deterministic UTF-8/LF serialisation and pure validation regression committed; retained A3 evidence unchanged |
| A5 integral verification | `AUTH-S07-A-A5-RETEST-002` | `APPROVED`; no new finding |
| Validation-owned testhost shutdown correction | `a6626a363713b4fbcf83387b7b2104eae1f3e918` | `Validate` tracks its process tree, reports output and waits up to 90 seconds for its owned testhost processes to exit |
| Automatic Quality Gate | `AUTH-S07-A-AQG-RETEST-003` | `APPROVED`; no new finding |

During A5, every frozen file identity and embedded digest listed below was
recalculated and matched. The eight ignored task-owned files, Git exclusions,
absence of reparse points and the seven frozen aggregate values were also
revalidated without rewriting the campaign workspace.

## Frozen dataset and evidence identities

| Artefact | Identity or SHA-256 | Reconciliation |
| --- | --- | --- |
| Dataset | `rag-eval-catalogue-v1` | matched |
| Revision | `rag-eval-catalogue-v1-candidate-001` | matched |
| Freeze | `frozen-a2-unscored` under `AUTH-S07-A-FREEZE-001` | matched |
| Dataset manifest file | `7275056bac4c3d545df0502494f36739704e21714f9499e9497250d3dd31261a` | matched exact retained bytes |
| Dataset manifest embedded digest | `ea51362782d171005d8ffc47bf9bc5c9885b4bccb294b101941f2da8d2183a7b` | matched exact zeroed-field digest rule |
| Case inventory file | `549474bbdc75100e37fe17af7d691f805c7362e523c9ec9abd2e354d0570a0be` | matched |
| Case inventory embedded manifest | `1430c0f9ce3fed6f223b6f3b2e299cb41ba774e8dc2c3830f22aa71b4b50c912` | matched |
| Document manifest file | `4dc91aa62d84d0c4acb4cc03b7fb11c25b3621c9b063f67c37c5ca062bad4dfd` | matched |
| Document manifest embedded digest | `9a26cd0b1cc6341eb0a18c3bdb5c071c6d0e1ec72336d336bfcd404d470257b4` | matched |
| Sanitised A3 result | `9efc2eef05388433af58e01242a1b1589556c43620eeec509f583fba0c2073bc` | matched owner-provided digest and exact retained bytes |

The A3 result remains at
`artifacts-local/state-07/s07-a/s07-a-local-candidate-001/evidence/synthetic-campaign-result.json`.
The fixed campaign root contains eight files and no reparse point. It is
ignored, task-owned evidence and is not a tracked product artefact.

The manifest intentionally remains the immutable pre-result freeze record.
Its `scoredResultObserved: false`, `scoredRunCount: 0` and pre-A3 review text
mean that no scored product-corpus result existed at freeze time. They were not
mutated after the synthetic result was observed. Current A3 execution facts are
recorded in this report instead.

## Dataset, provenance and rights disposition

The frozen revision contains no scored product-corpus document or case. The
PostgreSQL candidate `postgresql-18-reference-a4`, version `18.4`, SHA-256
`cea7b845568095eb56dee1b51bfa145c6c6637bc4377c986019971577efefae4`,
format `Pdf`, exact language `en` and trust class `OfficialExternal` remained
excluded with zero cases. It was not indexed or active; no catalogue revision,
document ID, index generation or activation binding existed; OpenAPI v1 could
not represent its exact document language by coercion; and visual-evidence
rights plus a finalised render manifest were absent. A3 did not access its URL
or bytes.

The campaign used only these project-owned `LocalAuthorised` fixtures under
`AUTH-S07-A-DATASET-001`:

| Document ID | Format | Exact content language | SHA-256 | Visual boundary |
| --- | --- | --- | --- | --- |
| `fixture-aurora-operations-pt-br-csv` | `Csv` | `pt-BR` | `261fbbae23b99e5a0761b92e876cbfe0e7649c009e73b8af12f5aa57a2771097` | not materialised |
| `fixture-beacon-operations-en-gb-pdf` | `Pdf` | `en-GB` | `5ef95f25d5ebbab2d838ccc8fea69992c78aa5ec4763b45bf0b9e590eb147b23` | logical locations only; not visual evidence |
| `fixture-cinder-operations-en-pdf` | `Pdf` | `en` | `d95cdea5a53a93e732d4b1af3dd9812d78a8c0430fcabb64504acac42b487750` | logical locations only; not visual evidence |

No PDF or PNG binary, render manifest, page-image identity, real source byte,
personal data, customer data, confidential data or secret was introduced by
this campaign.

## Environment, providers and command

| Field | Observed value |
| --- | --- |
| Environment | `ENV-S07-A-LOCAL-01` |
| Campaign | `s07-a-local-candidate-001` |
| Boundary | `local-offline-deterministic-synthetic` |
| Configuration | `Release` |
| Network policy | `deny-all-v1` / `deny-all` |
| Store policy | `task-owned-artifacts-local-v1` |
| Embedding provider | `deterministic-local` / `token-hash-embedding-v1` / `auth-s07-a-harness-001` / 64 dimensions |
| Language model | `deterministic-local` / `evidence-template-v1` / `auth-s07-a-harness-001` |
| Test target | `.NETCoreApp,Version=v10.0`; 64-bit .NET `10.0.10` reported by the test runner |
| Operating system | Windows local environment; real host or device identity not retained |
| CPU and memory capacity | `NOT_RECORDED` in retained A3 evidence |
| Release artefact identity | `NOT_RECORDED` as a digest in retained A3 evidence |

The exact command was:

```powershell
pwsh -NoProfile -File tests/RagChallenge.IntegrationTests/S07ALocalHarness/Invoke-S07ALocalHarness.ps1 -Mode Run -AuthorityId AUTH-S07-A-RUN-001
```

It ran from `C:\Projects\RAG-Challenge` with `--no-restore` inside the frozen
entry point. The observed exit code was `0`. The test runner selected exactly
one test and passed
`RagChallenge.IntegrationTests.S07ALocalHarness.S07ALocalHarnessCampaignTests.ExecuteFrozenCandidateAsync`.
The runner reported `5.7293` seconds total test time; the command orchestrator
reported `35.1` seconds wall time. The result file was written at
`2026-08-08T18:21:58.1351599Z`. Exact command start and completion instants
were not retained.

The campaign implementation exposed no network-capable adapter and used only
the deterministic providers above. `deny-all-v1` is the frozen harness policy;
packet-level observation was not part of A3, so the report makes no packet
capture claim.

## Sanitised case and language inventory

The campaign executed 11 synthetic cases: eight expected `answered` and three
expected `insufficient-evidence`. All 11 passed.

| Question to evidence language | Cases | Passed |
| --- | ---: | ---: |
| `pt-BR -> pt-BR` | 2 | 2 |
| `en-GB -> pt-BR` | 1 | 1 |
| `pt-BR -> en-GB` | 2 | 2 |
| `en-GB -> en-GB` | 3 | 3 |
| `pt-BR -> en` | 1 | 1 |
| `en-GB -> en` | 2 | 2 |

The exact `en` stratum remains separate from `en-GB`. The case inventory also
contained four `answerable`, two `citation-boundary-exact-location`, two
`insufficient-evidence`, one `prohibited-extrapolation`, one
`prompt-injection-provenance-confusion` and one `adversarial-filter` case; each
classification passed its frozen deterministic expectation.

Questions, expected facts, prohibited extrapolations, fixture content and
generated answers are deliberately omitted from this tracked report.

## Synthetic fixture observations

These values describe deterministic fixture contract behaviour only. They are
excluded from product-corpus denominators.

| Observation | Denominator | Value |
| --- | ---: | ---: |
| Retrieved relevant location within top five for expected answered cases | 8 | `1.000000` |
| Mean reciprocal rank at five for expected answered cases | 8 | `1.000000` |
| Harness citation-location validity field | 11 | `1.000000` |
| Answer language equals declared question language | 11 | `1.000000` |
| Required facts supported for expected answered cases | 8 | `1.000000` |
| Correct insufficient-evidence outcome | 3 | `1.000000` |
| Frozen case expectation passed | 11 | `1.000000` |

The result does not independently expose citation identity, source-derived
citation text, individual factual claims, high-impact claim classification,
provider calls, latency samples or dynamic-security telemetry.

## Pre-registered threshold disposition

The accepted thresholds were frozen before A3 and were not changed. Because
the revision contains zero scored product-corpus cases, synthetic observations
cannot be used to pass product thresholds.

| Measure | Threshold | Product-campaign status | A3 observation |
| --- | ---: | --- | --- |
| Recall@5 | `>= 0.90` overall; `>= 0.85` per reportable stratum | `NOT_RUN` | synthetic-only top-five location rate `1.000000` over 8 cases |
| MRR@5 | `>= 0.75` per reportable stratum | `NOT_RUN` | synthetic-only `1.000000` over 8 cases |
| Citation identity and location validity | `1.00` | `NOT_RUN` | location field `1.000000`; identity not independently exposed |
| Answer language equals question language | `1.00` | `NOT_RUN` | synthetic-only `1.000000` over 11 cases |
| Source-derived citation text preserves source language | `1.00` | `NOT_RUN` | not exposed in retained result |
| Supported factual claims | `>= 0.95` | `NOT_RUN` | synthetic required-fact field `1.000000` over 8 answered cases |
| Correct insufficient-evidence outcome | `>= 0.95` | `NOT_RUN` | synthetic-only `1.000000` over 3 cases |
| Unsupported high-impact factual claims | `0` | `NOT_RUN` | no reportable product denominator |
| Cross-database, generation or corpus leakage | `0` | `NOT_RUN` | one synthetic adversarial-filter expectation passed |
| Incorrect provenance or silent degraded substitution | `0` | `NOT_RUN` | no real-source campaign |
| Successful instruction override | `0` | `NOT_RUN` | one deterministic prompt-injection/provenance expectation passed; no dynamic campaign |
| Stale, withdrawn or deactivated source provider calls | `0` | `NOT_RUN` | no real source or external provider |
| Query p95 | `<= 12 s` | `NOT_RUN` | no latency sample set |
| Query p99 | `<= 20 s` | `NOT_RUN` | no latency sample set |
| Provider spend | `<= USD 20` | `NOT_RUN` | deterministic local providers only; no billing evidence |

No threshold is reported as passed for the product campaign.

## A5 verification results

A5 ran locally, offline, deterministically and sequentially on the clean
authorised baseline. The commands and observed results were:

| Command | Exit code | Observed result |
| --- | ---: | --- |
| `pwsh -NoProfile -File eng/check-repository.ps1` | `0` | `Repository audit passed for 244 non-ignored files.` |
| `pwsh -NoProfile -File tests/RagChallenge.IntegrationTests/S07ALocalHarness/Invoke-S07ALocalHarness.ps1 -Mode Validate` | `0` | 6 of 6 validation tests passed; reported test time `5.1489` seconds |
| `pwsh -NoProfile -File eng/ci.ps1 -Offline` | `0` | locked restore, build, policy and coverage checks, .NET and Dashboard tests, lint, typecheck and production build passed |

The offline CI run passed 146 unit tests, 164 integration tests, 10
architecture tests and 38 Dashboard tests. The measured .NET coverage was
94.91% of lines (`32116/33837`) and 67.42% of branches (`3536/5245`). The build
completed with zero warnings and zero errors. These results verify A5 only;
they do not substitute for an Automatic Quality Gate or expand the product
homologation claim.

## Automatic Quality Gate results

The complete Automatic Quality Gate restarted on 2026-08-09 under
`AUTH-S07-A-AQG-RETEST-003` on clean
`main@a6626a363713b4fbcf83387b7b2104eae1f3e918`, prompt corpus `4.10.1` and
the protected OpenAPI v1 SHA-256
`d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
The directed runtime preflight found zero independently running
RAG-Challenge-owned process and zero owned listener; nothing was stopped.

The static audit confirmed the A1-A5 commits and subsequent focal corrections,
the factual records, append-only history, frozen dataset boundary and absence
of OpenAPI, dependency or lockfile changes. The three frozen JSON manifests
were strict UTF-8/LF without BOM, their exact and embedded digests matched, and
the dataset directory remained unchanged after A2. It also revalidated:

- zero scored product-corpus documents and cases, one excluded real-source
  candidate, three deterministic fixtures and 11 synthetic cases;
- all six exact question-to-evidence language strata, keeping `en` separate
  from `en-GB`;
- all eight ignored, untracked task-owned evidence/store files and their
  hashes, with zero reparse point;
- the immutable A3 result SHA-256
  `9efc2eef05388433af58e01242a1b1589556c43620eeec509f583fba0c2073bc`;
  and
- all seven synthetic aggregates at `1.000000`, without including them in a
  product-corpus denominator.

The authorised commands and observed results were:

| Command | Exit code | Observed result |
| --- | ---: | --- |
| `pwsh -NoProfile -File eng/check-repository.ps1` | `0` | repository audit passed for 244 non-ignored files; command wall time 1.6 seconds |
| `pwsh -NoProfile -File tests/RagChallenge.IntegrationTests/S07ALocalHarness/Invoke-S07ALocalHarness.ps1 -Mode Validate` | `0` | 6 of 6 validation tests passed; runner test time 8.7199 seconds and command wall time 36.5 seconds |
| `pwsh -NoProfile -File eng/ci.ps1 -Offline` | `0` | locked restore, policy checks, build, .NET and Dashboard tests, coverage, lint, typecheck, production build and the repository audit passed; command wall time 142.6 seconds |

The offline CI run passed 146 unit tests, 164 integration tests, 10
architecture tests and 38 Dashboard tests. The build completed with zero
warnings and zero errors. Measured .NET coverage was 94.91% of lines
(`32116/33837`) and 67.42% of branches (`3536/5245`). The build immediately
after `Validate` completed without the assembly-lock failure previously caused
by a retained validation-owned testhost, confirming the correction in
`a6626a363713b4fbcf83387b7b2104eae1f3e918`.

The Automatic Quality Gate result is `APPROVED` with no new finding. This
approval applies only to the named local, offline, deterministic synthetic
boundary and does not change any `NOT_RUN` product-campaign threshold.

## Security, recovery, load and accessibility disposition

The single deterministic prompt-injection/provenance case and the single
adversarial-filter case passed their frozen expectations. This is fixture
contract evidence only. A3 did not execute dynamic SSRF, DNS rebinding, mixed
DNS, IP/Host/SNI pinning, redirect, media-type, decompression, authentication,
AIA/CRL/OCSP, renderer, page-image serving, stale-source, abuse, rate-limit,
crash, restart, rollback, load, accessibility or browser checks.

No real source, provider, account, secret, URL, DNS destination, browser,
network lane, cost centre or external action was selected or authorised.

## Findings

| ID | Severity | Disposition | Finding and impact |
| --- | --- | --- | --- |
| `S07-A-FIND-001` | `P2 Medium` | `OPEN` | The retained A3 JSON omits the authority, commit/corpus baseline, exact command start/end UTC instants, exit code, OS/runtime/architecture and CPU/memory capacity required by the reporting contract. The command output and this A4 reconciliation recover some fields, but the raw evidence does not independently reproduce the complete execution envelope. No correction or rerun was attempted. |
| `S07-A-FIND-002` | not separately graded | `RESOLVED` | Validation originally required the campaign root to be absent in every phase. Commit `275becfb04a4d0f7a1703c3be3f4c59d87550cc2` preserved that invariant before freeze and validates the retained post-A3/A5 workspace when present. |
| `S07-A-FIND-003` | not separately graded | `RESOLVED` | Compiler error `CS1061` and analyser finding `CA1859` in the correction candidate were corrected before the focal retained-workspace commit. |
| `S07-A-FIND-004` | `P3 Low` | `OPEN` historical | The immutable A3 result uses CRLF and remains at SHA-256 `9efc2eef05388433af58e01242a1b1589556c43620eeec509f583fba0c2073bc`. Commit `6cd939849909a8abf2c5dd0534244da5f19be833` corrects the cause for future JSON evidence by enforcing deterministic UTF-8/LF serialisation; it deliberately does not normalise or rewrite the historical result. |
| `S07-A-FIND-005` | not separately graded | `RESOLVED` | Analyser finding `CA1861` in the line-ending regression candidate was corrected before commit `6cd939849909a8abf2c5dd0534244da5f19be833`. |

No P0 or P1 finding was observed within the narrow synthetic boundary. This
does not assert that the unexecuted product, provider, source, browser,
security, load, recovery or accessibility boundaries contain no such finding.

The earlier Automatic Quality Gate findings have these final dispositions:

| ID | Disposition | Factual disposition |
| --- | --- | --- |
| `AQG-S07-001` | `RESOLVED` | A5 was subsequently completed, approved under `AUTH-S07-A-A5-RETEST-002` and reconciled in commit `da3fb024174db2a1e2b47a23bed69412bf3287df`. |
| `AQG-S07-002` | `RESOLVED` | The historical planning paragraph now distinguishes its then-current unexecuted status from the later A1-A5 execution. |
| `AQG-S07-003` | `RESOLVED` | The current pending-evidence item now records completed A1-A5 execution instead of the obsolete no-execution claim; both state corrections are in commit `d784da599949ca5cc0dcedb2a7d6faee3060ae97`. |
| `AQG-S07-004` | `RESOLVED` | Commit `a6626a363713b4fbcf83387b7b2104eae1f3e918` waits for validation-owned testhost shutdown; the complete gate passed `Validate` followed immediately by the offline CI build without an assembly lock. |

## Limitations and residual risks

- There are zero scored product-corpus documents and cases.
- The one real-source candidate remains excluded and unaccessed.
- The fixtures do not prove real PDF parsing, PNG rendering, visual-evidence
  rights, manifest integrity or serving.
- Citation identity and source-text preservation are not independently exposed
  by the retained result.
- No human two-person answer-quality rubric was executed.
- No external-provider quality, account, entitlement, secret, cost or latency
  evidence exists.
- No dynamic-security, load, crash, recovery, rollback, accessibility, browser,
  Linux, OCI or production evidence exists.
- Packet-level network observation was not performed.
- `S07-A-FIND-001` remains open because the immutable retained result does not
  contain the complete execution envelope.
- `S07-A-FIND-004` remains open as a historical property of the immutable A3
  result; its cause is corrected only for future evidence.

These limitations block product-level homologation and any public claim beyond
the named deterministic fixture boundary.

## Cleanup, rollback and lifecycle effect

A3 left its eight task-owned ignored evidence/store files intact for A4, A5
and the Automatic Quality Gate. A4, A5 and the gate performed no cleanup and
changed no frozen input or retained evidence. This documentary reconciliation
also leaves those files intact.

The `S07-A` Automatic Quality Gate is approved. No Human Gate ran;
`STATE-07 TESTING_HOMOLOGATION` remains active and `STATE-08` was not entered.

## Outcome

The exact authorised A3 local synthetic command completed with exit code `0`,
and all 11 frozen synthetic cases passed. A5 subsequently passed all three
authorised commands on the reconciled baseline. The result is accepted only as
deterministic fixture contract evidence for `ENV-S07-A-LOCAL-01`.

The complete Automatic Quality Gate approved the committed `S07-A` increment
with no new finding and resolved `AQG-S07-001` through `AQG-S07-004`.
`S07-A` nevertheless remains incomplete for product homologation: every
product-corpus threshold is `NOT_RUN`, the real provider/source/browser and
broader local security/load/recovery/accessibility boundaries are unexecuted,
and `S07-A-FIND-001` plus historical `S07-A-FIND-004` remain open. No Human
Gate or lifecycle transition is implied.

## HTTP v2 and visual-evidence serving increment

### Authority, commits and baseline

The separately versioned HTTP/OpenAPI v2 and same-origin visual-evidence
serving contract was frozen in commit
`54bab1aa5f25b778093bea62ffecf7c479557f9a`. The implementation was completed
in commit `c01abf525f4cc113baa389982da3b419d07556b6`. The focal correction for a
malformed `pageNumber` selector and its in-memory HTTP routing regression was
completed in commit `5505a85253aa4a8a7a3690caf3dd7a762175cab9`.

The complete Automatic Quality Gate restarted under
`AUTH-STATE07-V2-SERVING-AQG-RETEST-001` on 2026-08-09 from clean
`main@5505a85253aa4a8a7a3690caf3dd7a762175cab9`, prompt corpus `4.10.1`. The
runtime preflight found zero RAG-Challenge-owned process and zero owned
listener; nothing was stopped. No task-owned process remained after the gate.

The protected OpenAPI artefacts remained unchanged throughout the gate:

| Artefact | SHA-256 | Git blob | Result |
| --- | --- | --- | --- |
| OpenAPI v1 | `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` | `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160` | preserved byte for byte |
| OpenAPI v2 | `01ab26ae8066971af2e5ae83ec828fae556951d5ce6c335b42f6e7cf7b062640` | `8d31b200375ea834f148ea625664091cd5cdc84f` | preserved byte for byte |

### Static audit and verification results

The static audit reviewed all 33 implementation paths and the two-path focal
correction. It confirmed the frozen v2 transport shape, coexistence with v1,
exact BCP 47 document-language projection without coercing `en` to `en-GB`,
the active-generation/rights/manifest/page/content authority chain, uniform
visual failures, full revalidation before `200` or `304`, bounded response and
concurrency controls, strong ETag and private revalidation policy, same-origin
headers and CSP, accessible Dashboard presentation, and the absence of a
public internal evidence ID, storage path, rights record, arbitrary URL or JSON
byte payload. No schema, migration, dependency or lockfile change was present.

The audit specifically confirmed that all four composite-selector components,
including a malformed `pageNumber`, are handled inside the visual endpoint and
return the uniform `404`/`CH_VISUAL_EVIDENCE_NOT_AVAILABLE` outcome without
reaching the Dashboard fallback. It also confirmed the corrected generic
constraint required by the in-memory `IServer` regression.

The authorised local and offline commands and their observed results were:

| Command | Exit code | Observed result |
| --- | ---: | --- |
| `pwsh -NoProfile -File eng/check-repository.ps1` | `0` | repository audit passed for 255 non-ignored files |
| `dotnet test tests/RagChallenge.UnitTests/RagChallenge.UnitTests.csproj --configuration Release --no-restore --filter "FullyQualifiedName~QuestionAnsweringServiceTests\|FullyQualifiedName~DocumentLanguageAndRenderingTests" --verbosity minimal` | `0` | 46 of 46 focused unit tests passed |
| `dotnet test tests/RagChallenge.IntegrationTests/RagChallenge.IntegrationTests.csproj --configuration Release --no-restore --filter "FullyQualifiedName~ApiV2ContractTests\|FullyQualifiedName~SqliteActivationLifecycleTests" --verbosity minimal` | `0` | 20 of 20 focused integration tests passed |
| `dotnet test tests/RagChallenge.Architecture.Tests/RagChallenge.Architecture.Tests.csproj --configuration Release --no-restore --filter "FullyQualifiedName~V2TransportDoesNotExposeInternalEvidenceAuthorityOrStorage\|FullyQualifiedName~DashboardDoesNotDeclareProviderOrServerDependencies\|FullyQualifiedName~CoreAssembliesDoNotReferenceOuterFrameworksOrAdapters" --verbosity minimal` | `0` | 3 of 3 focused architecture tests passed |
| `node --test test/api-v2-contract.test.mjs test/result-presentation.test.mjs` from `src/RagChallenge.Dashboard.Web` | `0` | 9 of 9 focused Dashboard tests passed |
| `pwsh -NoProfile -File eng/ci.ps1 -Offline` | `0` | locked restore, policy checks, Release build, all .NET and Dashboard tests, coverage, lint, typecheck, production build and final repository audit passed |

The complete offline CI passed 147 unit tests, 171 integration tests, 11
architecture tests and 42 Dashboard tests. The Release build completed with
zero warnings and zero errors. Measured .NET coverage was 94.80% of lines
(`32620/34408`) and 67.14% of branches (`3664/5457`).

### Findings, limits and outcome

| ID | Disposition | Factual disposition |
| --- | --- | --- |
| `AQG-S07-V2-001` | `RESOLVED` | Commit `5505a85253aa4a8a7a3690caf3dd7a762175cab9` moved malformed `pageNumber` handling inside the endpoint and added the real in-memory HTTP routing regression for the uniform visual `404`. |
| `AQG-S07-V2-002` | `RESOLVED` | The `CS8633` and `CS8714` compilation blockage in that regression was corrected by the required `notnull` generic constraint before the same focal commit and complete gate retest. |

The Automatic Quality Gate result is `APPROVED`, with no new P0, P1, P2 or P3
finding. This approval verifies only the implemented local, offline,
deterministic and synthetic v2/serving boundary. It does not constitute product
homologation or a Human Gate and does not advance `STATE-07`.

Browser and assistive-technology execution, real product data, real rendering,
real source/provider/network access, load, crash/recovery, Linux, OCI,
production, Human Gate, lifecycle transition, push, publication and deployment
were `NOT_RUN`. No contract, OpenAPI, schema, migration, dependency, lockfile,
ADR, dataset or retained evidence was changed by the gate.
