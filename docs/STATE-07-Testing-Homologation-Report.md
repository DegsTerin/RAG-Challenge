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

It additionally records the separately authorised composed v2 integration,
restart, cold backup/restore and contractual-limit implementation completed in
commit `e5dae7ee5a786417fba2c6ef0555686816b0b330`, its focal correction in commit
`f6c648c40cf8d0280cfceca5509a381bddb9fc8f` and the subsequently approved
Automatic Quality Gate. This remains synthetic integration evidence only.

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

## V2 integration, restart and cold-recovery increment

### Authority, implementation and protected baseline

`AUTH-STATE07-V2-INTEGRATION-RECOVERY-IMPL-001` authorised only the local,
offline, deterministic, synthetic and sequential composed v2 runtime boundary
on clean
`main@a47bd40b1873920c7660abb14acd68de45a7dde4`, prompt corpus `4.10.1`.
The focused implementation was committed as
`e5dae7ee5a786417fba2c6ef0555686816b0b330`.

The implementation composes the query, readiness and verified page-image
reader through the explicit `Integration` profile. Outside that profile the
visual reader remains disabled and fail-closed. The project-owned fixture is a
deterministic in-memory PDF with a complete 1 × 1 PNG, final render manifest,
permitted synthetic rights and immutable content identities; no real corpus,
source, renderer, provider or product data was used.

OpenAPI v1 and v2 remained byte-for-byte unchanged:

| Artefact | SHA-256 | Git blob | Result |
| --- | --- | --- | --- |
| OpenAPI v1 | `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` | `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160` | preserved byte for byte |
| OpenAPI v2 | `01ab26ae8066971af2e5ae83ec828fae556951d5ce6c335b42f6e7cf7b062640` | `8d31b200375ea834f148ea625664091cd5cdc84f` | preserved byte for byte |

### Focused observed evidence

| Boundary | Observed result |
| --- | --- |
| Focused contract, activation, composition and end-to-end tests | 52 of 52 passed |
| Published artefact harness | `Passed` on the exclusive `http://127.0.0.1:5086` origin |
| Same-origin v2 flow | query returned the exact PDF page selector; verified PNG serving returned `200`; full conditional revalidation returned `304` |
| Restart | the active index generation and visual selector remained unchanged after reopening the original store |
| Cold backup/restore | the host was stopped before each confined copy; exact store fingerprints matched; the restored store reopened the same active generation and visual evidence |
| Contractual visual limits | the 64 MiB response ceiling remained enforced; ten immediate visual requests were accepted and the eleventh returned `429`/`CH_VISUAL_EVIDENCE_RATE_LIMITED` with `Retry-After: 10` |
| Deterministic artefact | two offline builds produced the same ZIP SHA-256 `e27c64571b63538e4cba21f552df500c24a4bab3a6365e6229e2d9dd033f2f7d` |
| Cleanup | task-owned runtime, store, backup, restore and temporary paths were removed; no RAG-Challenge host or listener remained |

The focused result was `PASSED` only for this synthetic integration boundary
and did not itself constitute an Automatic Quality Gate, product homologation,
Human Gate or lifecycle transition.

### Automatic Quality Gate retest

The complete Automatic Quality Gate restarted under
`AUTH-STATE07-V2-INTEGRATION-RECOVERY-AQG-RETEST-001` on 2026-08-09 from clean
`main@f6c648c40cf8d0280cfceca5509a381bddb9fc8f`, prompt corpus `4.10.3` and
the protected OpenAPI v1 and v2 identities recorded above. The baseline
included the focal correction for `AQG-S07-V2-IR-001`.

The observed gate evidence was:

| Boundary | Observed result |
| --- | --- |
| Repository audit | 255 non-ignored files passed |
| Runtime preflight | zero RAG-Challenge-owned process and zero owned listener; nothing was stopped |
| Focused Release verification | 53 of 53 tests passed |
| Deterministic published artefact | two builds produced the same ZIP SHA-256 `ab5e450efe1b606f2b8e50e2f5885a3c1ae19bf4ad90dd96d096e00506daec28` |
| Published artefact harness | `Passed` exclusively on loopback, with all three readiness observations `Ready` |
| Restart and cold restore | the active generation was preserved after restart and after the confined cold restore |
| Same-origin visual flow | PNG serving and conditional `304` passed |
| Contractual limits | the 64 MiB ceiling passed; ten immediate visual requests were accepted and the eleventh was rejected by the token bucket |
| Complete offline CI | 147 unit, 174 integration, 11 architecture and 42 Dashboard tests passed; line coverage was 94.81% and branch coverage was 67.24%; the build had zero warnings and zero errors |
| Cleanup | task-owned cleanup completed; no RAG-Challenge runtime or listener remained |

The Automatic Quality Gate result is `APPROVED`, with no new finding.
`AQG-S07-V2-IR-001` is `RESOLVED`.

This approval remains limited to the local, offline, deterministic, synthetic
and sequential integration boundary. Browser and assistive-technology
execution, real corpus, source, renderer, data and provider access, external
network, benchmark, load, p95/p99, broad crash injection, operational
backup/restore, Linux, OCI, production, publication and deployment remain
`NOT_RUN`. No product homologation, Human Gate or lifecycle transition is
implied. No contract, OpenAPI, schema, migration, ADR, dependency, lockfile or
retained product evidence was changed by the gate.

## ADR-0011 visual-serving rights-policy correction

### Authority, implementation and protected baseline

The internal rights-policy correction authorised under
`AUTH-S07-A-RIGHTS-POLICY-CORR-IMPL-001` was completed in focused commit
`b9c3e5f3a72c2dd7762c256198452ae2c217b2d2`. This documentary
reconciliation started from that clean `main` commit under
`AUTH-S07-A-RIGHTS-POLICY-CORR-IMPL-RECONCILE-001`, prompt corpus `4.10.10`.

The protected OpenAPI artefacts remained unchanged:

| Artefact | SHA-256 | Git blob | Result |
| --- | --- | --- | --- |
| OpenAPI v1 | `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` | `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160` | preserved byte for byte |
| OpenAPI v2 | `01ab26ae8066971af2e5ae83ec828fae556951d5ce6c335b42f6e7cf7b062640` | `8d31b200375ea834f148ea625664091cd5cdc84f` | preserved byte for byte |

### Implemented fail-closed boundary

The serving-specific `PdfVisualEvidenceServing` policy evaluates all ten
independent rights decisions. `RuntimeDerivativeImageDisplay` must be
`Permitted`. `SourceAndDerivativeByteDistributionOrPublication` set to
`Unproven` blocks visual serving. A `Denied` distribution decision is
compatible with the accepted same-origin display boundary only when
`RuntimeDerivativeImageDisplay` is `Permitted`; it does not authorise external
distribution or publication.

The verified page-image reader applies this serving-specific policy before it
opens the immutable PNG content. An ineligible record therefore remains
fail-closed before the unchanged HTTP endpoint can produce `200` or `304`.
The activation-oriented `PdfVisualEvidence` gate remains independent, and no
rights field, storage mapping, schema, migration or public contract was added.

### Focused observed verification

The local, offline, synthetic Release checks observed during the implementation
were:

| Boundary | Observed result |
| --- | --- |
| Serving-policy unit tests | 19 of 19 passed |
| Existing activation and render-candidate gate regressions | 23 of 23 passed |
| Verified page-image reader integration tests | 3 of 3 passed |
| Protected v1/v2 HTTP contract tests | 6 of 6 passed |

The final implementation check found no RAG-Challenge runtime process and no
owned listener. The working tree was clean after the focused commit.

These results are focused implementation evidence only. They are not an
Automatic Quality Gate, product homologation, Human Gate or lifecycle
transition. No new A0 was executed. `postgresql-18-reference-a4` remains
`BLOCKED/EXCLUDED`: page rendering, derivative-image creation,
derivative-image retention and runtime derivative display remain `UNPROVEN`,
while external source or derivative distribution/publication remains `DENIED`
under the recorded boundary.

## ADR-0012 notice-bearing schema and migration implementation

### Authority, implementation and protected baseline

The schema and migration increment authorised under
`AUTH-S07-A-NOTICE-BEARING-SCHEMA-MIGRATION-001` was completed in focused
commit `98036f3c8c496544f4532d1fe48c981f836a1871` from clean
`main@564d9efd72285bb41545a5e60b63fcd44f9705fd`, prompt corpus `4.10.16`.

The protected OpenAPI artefacts remained unchanged:

| Artefact | SHA-256 | Git blob | Result |
| --- | --- | --- | --- |
| OpenAPI v1 | `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` | `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160` | preserved byte for byte |
| OpenAPI v2 | `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` | `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8` | preserved byte for byte |

### Implemented persistence boundary

The Control schema now persists immutable `DerivativeObligationSetV1` rows and
their ordered evidence-reference, disclaimer and exact-text blocks. The
`pdf-page-png-notice-v1` profile coexists with the legacy profile and binds one
obligation-set identity and digest, source-region dimensions and notice-region
dimensions to each notice-bearing render manifest and page.

The two migrations
`20260810033026_AddNoticeBearingObligationSchema` and
`20260810034537_SealNoticeBearingObligationBindings` add the relational model
and seal its bindings after the SQLite table rebuild. Conditional constraints,
foreign keys and triggers fail closed on absent, mixed, mutable or mismatched
notice-bearing state. Legacy records, manifests, hashes and activations remain
unchanged; no inferred notice, right or backfill was introduced.

### Focused observed verification

The local, offline and synthetic verification observed during implementation
was:

| Boundary | Observed result |
| --- | --- |
| Focused migration and compatibility tests | 7 of 7 passed |
| Entity Framework model check | no pending model changes |
| Relational integrity | `foreign_key_check`, upgrade, rollback to zero and reapply passed in task-owned temporary SQLite stores |
| Cleanup | task-owned cleanup completed |

These results are focused implementation evidence only. No test, runtime,
Automatic Quality Gate, Human Gate or lifecycle action was executed during this
documentary reconciliation. Renderer, notice-bearing PNG creation, manifest
composition, storage/reachability behaviour, serving, Dashboard presentation,
dataset and product activation remain unimplemented or `NOT_RUN` as applicable.
No new A0 was executed, and `postgresql-18-reference-a4` remains
`BLOCKED/EXCLUDED` with its recorded rights disposition unchanged.

## ADR-0013 OpenAI language-model adapter compatibility

### Authority, implementation and protected baseline

The local, offline and deterministic adapter increment authorised under
`AUTH-STATE07-LLM-ADAPTER-COMPAT-001` was completed in focused commit
`b6d6f9102ecf0ea93309f8080acebad02cf16584` from clean
`main@27b385d0f534739ccbc4e8d946eea654e00df9fe`, prompt corpus `4.10.19`.
Runtime preflight found no RAG-Challenge-owned process or listener, so nothing
was stopped.

The protected OpenAPI artefacts remained unchanged:

| Artefact | SHA-256 | Git blob | Result |
| --- | --- | --- | --- |
| OpenAPI v1 | `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` | `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160` | preserved byte for byte |
| OpenAPI v2 | `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` | `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8` | preserved byte for byte |

### Implemented compatibility boundary

`OpenAiLanguageModelOptions` now fixes provider ID `openai`, model ID and
observed revision to the dated `gpt-5.4-mini-2026-03-17` snapshot. Reasoning
effort and context are typed, non-secret and immutable for the adapter
instance; the accepted values serialise as `reasoning.effort=none` and
`reasoning.context=current_turn`.

The Responses API request preserves `store=false`, bounded input and output,
the strict structured-output schema and the existing cancellation policy. It
does not emit `tools`, background mode, previous-response state or
`temperature`; no support for `temperature` was assumed for the selected
reasoning configuration.

The response parser requires the exact dated observed model, root and message
status `completed`, one assistant message and one `output_text` content item.
Unexpected reasoning items, refusals, incomplete responses, mutable aliases,
invalid structures and unsupported citations fail with the existing typed,
sanitised provider outcome. No repair call or fallback model was added.

No operational configuration or runtime composition was added. The adapter
remains exercised only through an in-process fake HTTP handler.

### Focused observed verification

| Boundary | Observed result |
| --- | --- |
| OpenAI adapter contract tests | 18 of 18 passed with an in-process fake handler |
| Architecture tests | 11 of 11 passed |
| Formatting verification | passed with `--no-restore` and no changes required |
| Repository audit | 266 non-ignored files passed |
| Scope | only the Infrastructure adapter and its integration contract tests changed |
| Final repository state | focused commit on `main`; working tree clean |

These are focused compatibility results only. They do not establish account
availability, provider behaviour, bilingual quality, groundedness, citation
quality, insufficient-evidence behaviour, prompt-injection resistance,
latency, spend or real-corpus suitability. No account, credential, external
provider, real corpus, OCI or paid service was accessed. No Automatic Quality
Gate, Human Gate, deployment, product homologation or lifecycle transition was
executed or implied.

### Automatic Quality Gate

The separately authorised Automatic Quality Gate ran on 2026-08-10 under
`AUTH-STATE07-LLM-ADAPTER-COMPAT-AQG-001` from clean
`main@6e6fdabb91e2fb4c5186c464ce08f5da390d727a`, prompt corpus `4.10.20`.
Runtime preflight found no RAG-Challenge-owned process or listener, so nothing
was stopped. The gate used PowerShell `7.6.4`, .NET SDK `10.0.302`, Node.js
`24.19.0` and npm `11.17.0`.

The static audit confirmed that implementation commit
`b6d6f9102ecf0ea93309f8080acebad02cf16584` has the required parent
`27b385d0f534739ccbc4e8d946eea654e00df9fe`, changes only the Infrastructure
adapter and its integration contract tests, and has no later executable diff.
It confirmed all seven ADR-0013 adapter requirements, no secret-like value in
the implementation diff, and no legacy or inactive future model identifier in
`src/` or `tests/`.

The authorised local and offline commands produced these observed results:

| Command or boundary | Exit code | Observed result |
| --- | --- | --- |
| `pwsh -NoProfile -File eng/check-repository.ps1` | `0` | repository audit passed for 266 non-ignored files |
| `dotnet format RAG-Challenge.sln --verify-no-changes --no-restore --verbosity minimal` | `0` | no formatting change required |
| focused `OpenAiHttpAdapterContractTests` in Release with `--no-restore` | `0` | 18 of 18 passed through in-process fake handlers |
| Release architecture tests with `--no-restore` | `0` | 11 of 11 passed |
| `pwsh -NoProfile -File eng/ci.ps1 -Offline` | `0` | restore policy, build, tests, coverage, lint, typecheck, Dashboard build and final repository audit passed |

The complete offline CI passed 154 unit, 191 integration, 11 architecture and
45 Dashboard tests. Merged .NET coverage was 95.63% of lines and 67.65% of
branches; the Release build had zero warnings and zero errors. The protected
OpenAPI v1 and v2 SHA-256 and Git blob identities remained unchanged. The
working tree was clean at the end, and no RAG-Challenge-owned process remained.

The Automatic Quality Gate result is `APPROVED`, with no P0, P1, P2 or P3
finding. Approval is limited to the local, offline, deterministic adapter
compatibility boundary and fake handlers. Account availability, real-provider
behaviour, bilingual quality, groundedness, citation quality, insufficient-
evidence behaviour, prompt-injection resistance, latency, spend, real-corpus
suitability, OCI, deployment, product homologation, Human Gate and lifecycle
transition remain `NOT_RUN`.

## GPT-5.4-mini provider-candidate campaign preparation

### Authority, commit and protected baseline

The local, offline and deterministic preparation authorised under
`AUTH-S07-A-PROVIDER-PREP-001` was completed in focused commit
`422286863e7a3c213e96db18144769bd0458a75b` from clean
`main@b28952b4ee875b65b18465396563e036aa7f39b0`, prompt corpus `4.10.21`.
Runtime preflight found no RAG-Challenge-owned process or listener, so nothing
was stopped. The commit added nine paths limited to the immutable successor
revision and the integration-test harness; it did not change Domain,
Application, production configuration, ADRs or OpenAPI.

The protected OpenAPI artefacts remained unchanged:

| Artefact | SHA-256 | Git blob | Result |
| --- | --- | --- | --- |
| OpenAPI v1 | `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` | `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160` | preserved byte for byte |
| OpenAPI v2 | `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` | `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8` | preserved byte for byte |

### Frozen successor revision

Stable dataset ID `rag-eval-catalogue-v1` now has the immutable, unscored
successor revision
`rag-eval-catalogue-v1-provider-gpt54m-candidate-001`. Its predecessor
`rag-eval-catalogue-v1-candidate-001` remains preserved and unchanged. The
successor records `providerRunCount=0`, `scoredResultObserved=false`, two
project-owned synthetic fixture documents and no real-source or product-corpus
document.

The frozen manifest identities are:

| File | File SHA-256 | Embedded manifest SHA-256 |
| --- | --- | --- |
| `dataset-manifest.json` | `60c58571fd8c15c6313d7ca6997be1fb8d06d82611096a363474144aafb8def0` | `cb5c4f5a2b3f2a26056158aafc33d9e30bcd2deef22a8db1fdcbd708f184690c` |
| `document-manifest.json` | `18e2771fafa5cf2b54266031d3676ababb609312826a159babaa0bc7a38b473e` | `85063c59b873ab7046c42eb27a0457246d7cd22457c8fab7c726d3855d5d41a4` |
| `case-inventory.json` | `e8801e964f3026ea15ffe6b6806ea7e1546e7ab0e18df53ba75e769f24ad2c3b` | `ceb860fe4b4a0847a788412a5d5af1a89fc9f1644374123bdcf56d89be3e63cc` |
| `campaign-contract.json` | `1b5f50e2d9885ffc4a3a4f84df5081574c940045ac2744e07ada84373b11b2ae` | `cd67240154a81170d17f00d06e32d898a5b2598f6b866eb40bf5347165f1fb54` |
| `call-schedule.json` | `6ace9c204e44ab997d1a57ec122609e7aa1dae012e5645e430d5271efe5553e7` | `5e586ed4c948d3ac6401d441c3ceaabd7880c2569ed99060fcef51b861f568d9` |

The inventory contains 60 deterministic synthetic cases. Forty are
answerable, with ten cases in each mandatory `pt-BR`/`en-GB` question and
evidence direction. Twenty expect insufficient evidence: ten stop before a
provider call because no evidence was retrieved, and ten contain evidence that
does not support the requested fact. Twelve answerable cases contain retrieved
prompt-injection text, covering six attack classes for each question language.
The matrix and attack inventory are prepared inputs only; they are not observed
model-quality or attack-resistance results.

### Frozen provider profile, limits and schedule

Campaign `s07-a-provider-gpt54m-candidate-001` identifies logical environment
`ENV-S07-A-PROVIDER-01`, provider `openai` and the exact dated model and
observed revision `gpt-5.4-mini-2026-03-17`. The frozen Responses API profile
uses `store=false`, omits tools, temperature, background mode and previous-
response state, and fixes `reasoning.effort=none` and
`reasoning.context=current_turn`. Prompt `grounded-answer-v1`, its exact text
digest, the strict `grounded_answer` response schema and its digest are frozen.

The existing limits remain frozen at 4,096 UTF-8 question bytes, six evidence
chunks, 16,000 evidence Unicode scalars, retrieval top eight, 32,768 answer
characters, 8,192 maximum output tokens, a 2 MiB response, 10-second connect
timeout, 25-second end-to-end deadline, p95 at most 12 seconds and p99 at most
20 seconds.

The call schedule contains four contract-smoke calls, five warm-up calls and
100 measured calls, for an absolute maximum of 109. It uses no retry and
concurrency one. The 100 measured calls repeat each of the 50 provider-calling
unique cases twice; only the first repetition contributes to a quality
denominator, while repeats are reserved for future latency and stability
observation. The operational budget is frozen at `USD 16`, with an absolute
ceiling of `USD 20`. These values are limits, not observed calls, latency or
spend. Only the opaque placeholder `<provider-secret-reference>` is recorded;
no credential value is present or resolved.

### Fake-handler validation observed during preparation

The provider-candidate harness accepts only an injected `HttpMessageHandler`.
Its committed validation entry point exposes only `-Mode Validate`; there is no
real-run mode or external transport composition. The in-process fake handler
validated the exact request property set, dated model, `store=false`, reasoning
profile, output-token limit, prompt digest, strict schema digest, evidence and
structured response for the 50 provider-calling unique cases. Fake responses
were constructed from the frozen expectations, so this is adapter and campaign
contract evidence, not semantic model evaluation.

The final preparation checks produced these observed results:

| Command or boundary | Exit code | Observed result |
| --- | --- | --- |
| `pwsh -NoProfile -File tests/RagChallenge.IntegrationTests/S07AProviderHarness/Invoke-S07AProviderHarness.ps1 -Mode Validate` | `0` | 2 of 2 provider-preparation tests passed with the in-process fake handler |
| focused Release tests for `S07AProviderHarness` and `OpenAiHttpAdapterContractTests` with `--no-restore` | `0` | 20 of 20 passed with fake handlers |
| `dotnet format RAG-Challenge.sln --verify-no-changes --no-restore --verbosity minimal` | `0` | no formatting change required |
| `pwsh -NoProfile -File eng/check-repository.ps1` | `0` | repository audit passed for 275 non-ignored files |
| `git diff --cached --check` | `0` | no whitespace error |

The final preparation commit left `main` clean and no RAG-Challenge-owned
process remained. The present documentary reconciliation independently
recomputed every successor file and embedded-manifest digest and matched the
three recorded predecessor file identities without running the harness.

### Boundary and disposition

Preparation status is `frozen-provider-candidate-preparation-unscored`. It
does not establish account availability, provider entitlement or behaviour,
bilingual quality, groundedness, citation quality, correct real-model
insufficient-evidence behaviour, prompt-injection resistance, latency, cost or
real-corpus suitability. No account, credential value, external provider,
real corpus, real source, OCI or paid service was accessed. No real evaluation,
deployment, Automatic Quality Gate, Human Gate, product homologation or
lifecycle transition was executed or implied.

### Automatic Quality Gate of the frozen preparation

The separately authorised Automatic Quality Gate ran on 2026-08-10 under
`AUTH-S07-A-PROVIDER-PREP-AQG-001` from clean
`main@5d74c9c9190b0b3465b11dc6864e3dd519cc88f9`, prompt corpus `4.10.22`.
It audited implementation commit
`422286863e7a3c213e96db18144769bd0458a75b` and its factual documentary
reconciliation commit `5d74c9c9190b0b3465b11dc6864e3dd519cc88f9`. Runtime preflight found zero
RAG-Challenge-owned process before the checks and after their completion.

The static audit confirmed the required parentage and isolated scopes: the
implementation commit added only the five successor-revision manifests and
four provider-harness paths, while the reconciliation commit changed only the
four canonical factual documents. The predecessor revision remained byte for
byte unchanged from the authorised baseline. All five successor file SHA-256
values and all five embedded manifest SHA-256 values matched the frozen
identities listed above. The audit also confirmed 60 synthetic cases, 40
answerable cases with ten in each mandatory bilingual direction, 20
insufficient-evidence cases split equally between the zero-call and
evidence-present pathways, and 12 prompt-injection cases covering all six
attack classes for each question language.

The frozen campaign contract continued to require the exact
`gpt-5.4-mini-2026-03-17` snapshot, `store=false`, omitted tools, zero retry,
concurrency one, a maximum schedule of 109 calls and the fail-closed `USD 16`
operational and `USD 20` absolute budget limits. Static scans found no
default external transport path and no secret-like value in the audited
campaign paths. The executable harness continued to receive its only transport
through an injected `HttpMessageHandler`; the 109-call schedule was inspected
but never run.

The authorised local and offline commands produced these observed results:

| Command or boundary | Exit code | Observed result |
| --- | --- | --- |
| read-only static verifier for commit scope, predecessor identity, manifest digests, case matrix, schedule, configuration and budget | `0` | all frozen identities and counts matched; predecessor unchanged |
| `pwsh -NoProfile -File tests/RagChallenge.IntegrationTests/S07AProviderHarness/Invoke-S07AProviderHarness.ps1 -Mode Validate` | `0` | 2 of 2 provider-preparation tests passed with the in-process fake handler |
| focused Release tests for `S07AProviderHarness` and `OpenAiHttpAdapterContractTests` with `--no-restore` | `0` | 20 of 20 passed with fake handlers |
| `pwsh -NoProfile -File eng/ci.ps1 -Offline` | `0` | restore policy, build, tests, coverage, lint, typecheck, Dashboard tests/build and repository audit passed |
| `dotnet format RAG-Challenge.sln --verify-no-changes --no-restore --verbosity minimal` | `0` | no formatting change required |
| `pwsh -NoProfile -File eng/check-repository.ps1` | `0` | repository audit passed for 275 non-ignored files |
| `git diff --check` | `0` | no whitespace error |

The complete offline CI passed 154 unit, 193 integration, 11 architecture and
45 Dashboard tests. Merged .NET coverage was 95.63% of lines and 67.66% of
branches; the Release build had zero warnings and zero errors. Two preliminary
ad hoc assertions used incorrect auditor-side assumptions about the predecessor
path and corpus-version occurrence count. Their decomposed read-only reruns
passed and found no repository divergence; they were not product findings.
The protected OpenAPI v1 and v2 SHA-256 and Git blob identities remained
unchanged, the final working tree was clean and no RAG-Challenge-owned process
remained.

The Automatic Quality Gate result is `APPROVED`, with no P0, P1, P2 or P3
finding. Approval is restricted to the local, offline, deterministic and
fake-handler-only campaign-preparation boundary. Account access, credentials,
provider access or behaviour, paid calls, real corpus or source, real
evaluation, bilingual quality, groundedness, citation quality, real-model
insufficient-evidence behaviour, prompt-injection resistance, observed
latency, observed cost, OCI, deployment, Human Gate and lifecycle transition
remain `NOT_RUN`.
