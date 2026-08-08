# STATE-07 Testing and Homologation Report

## Purpose and authority

This report reconciles the single local, offline, deterministic and sequential
`S07-A` A3 campaign executed under `AUTH-S07-A-RUN-001`, and the documentary A4
reconciliation authorised by `AUTH-S07-A-RECONCILE-001` on 2026-08-08. It is
factual evidence for the synthetic `ENV-S07-A-LOCAL-01` boundary only.

This report does not claim product-corpus quality, real-provider quality,
performance, security, accessibility, Linux, OCI or production homologation.
It is not A5 verification, an Automatic Quality Gate, a Human Gate, a lifecycle
transition, publication or deployment authority.

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
| Repository | `C:\Projects\RAG-Challenge` |
| Branch | `main` |
| Authorised and reconciled `HEAD` | `43ddc0de4a6c10b32a657f3c1e471a743cb42b5f` |
| Prompt corpus | `4.10.1` |
| Working tree before A3 | clean |
| Working tree before A4 | clean |
| Lifecycle | `STATE-07 TESTING_HOMOLOGATION` active |
| A4 runtime preflight | `NOT_APPLICABLE` — documentary reconciliation only; no process or listener was inspected or stopped |

A3 was limited to the frozen local synthetic campaign. A4 was limited to
reading its frozen inputs and ignored task-owned evidence and creating this
report. A4 did not execute a test, provider, browser, source, network path or
runtime and did not alter the frozen inputs.

The preserved negative scope includes A5, corrections, threshold changes,
source or test changes, dependencies, lockfiles, public contracts, OpenAPI,
schema, migrations, ADRs, real providers, real sources, network access,
browser use, OCI, GitHub, publication, deployment, Automatic Quality Gate,
Human Gate and lifecycle change.

## Frozen dataset and evidence identities

| Artefact | Identity or SHA-256 | Reconciliation |
| --- | --- | --- |
| Dataset | `rag-eval-catalogue-v1` | matched |
| Revision | `rag-eval-catalogue-v1-candidate-001` | matched |
| Freeze | `frozen-a2-unscored` under `AUTH-S07-A-FREEZE-001` | matched |
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

No P0 or P1 finding was observed within the narrow synthetic boundary. This
does not assert that the unexecuted product, provider, source, browser,
security, load, recovery or accessibility boundaries contain no such finding.

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
- `S07-A-FIND-001` remains open; findings were not corrected during A4.

These limitations block product-level homologation and any public claim beyond
the named deterministic fixture boundary.

## Cleanup, rollback and lifecycle effect

A3 left its eight task-owned ignored evidence/store files intact for A4 and a
future authorised A5. A4 performed no cleanup and changed no frozen input.
The documentary rollback for this uncommitted A4 increment is removal of this
report only; it must not remove or rewrite the retained A3 evidence.

No Automatic Quality Gate or Human Gate ran. `STATE-07
TESTING_HOMOLOGATION` remains active and `STATE-08` was not entered.

## Outcome

The exact authorised A3 local synthetic command completed with exit code `0`,
and all 11 frozen synthetic cases passed. The result is accepted only as
deterministic fixture contract evidence for `ENV-S07-A-LOCAL-01`.

`S07-A` remains incomplete for product homologation: every product-corpus
threshold is `NOT_RUN`, the real provider/source/browser and broader local
security/load/recovery/accessibility boundaries are unexecuted, and
`S07-A-FIND-001` remains open. A5 verification and any local commit require a
separate explicit authority.
