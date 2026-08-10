# ADR-0013 — MVP Language-Model Candidate and Deferred Frontier Evaluation

- Status: proposed
- Date: 2026-08-10
- Preparation authority:
  `AUTH-STATE07-LLM-CANDIDATE-ADR-PREP-001`, granted by the product owner on
  `main@f682827d1a26b08fa8c450a1fadb3bd0e1fa1700`, corpus `4.10.17`
- Decision authority: none; this proposal has not been accepted
- Owners: RAG-Challenge product, architecture, security and operations
- State: `STATE-07 TESTING_HOMOLOGATION` documentary proposal only
- Proposed relationship: if separately accepted, supersede only the
  language-model candidate selection in
  [ADR-0005](ADR-0005-MVP-Providers-Persistence-And-OCI-Deployment.md); all
  other ADR-0005 decisions remain unchanged
- Verification status: current unauthenticated public model documentation was
  reviewed on 2026-08-10; account availability, provider behaviour, bilingual
  quality, latency and runtime compatibility remain unverified

## Purpose and authority

This proposal re-evaluates the language-model candidate selected by ADR-0005.
It proposes `gpt-5.4-mini-2026-03-17` for the first online MVP and retains
`gpt-5.6-sol` only as a future evaluation candidate.

The proposal prioritises predictable delivery, a dated model snapshot,
adapter fit, bilingual grounded-answer quality, citation integrity,
insufficient-evidence behaviour, prompt-injection resistance and the accepted
query-latency thresholds. Cost is deliberately excluded from the comparative
ranking. That exclusion does not remove any existing spend control or grant
authority to incur provider charges.

While this ADR remains `proposed`, ADR-0005 continues to select
`gpt-4.1-mini-2025-04-14`. This document does not change configuration, code,
the OpenAPI contracts, lifecycle state or provider authority. It does not
authorise an account inspection, credential access, real-corpus processing,
network egress, a paid call, OCI access, implementation, evaluation or
deployment.

## Context

ADR-0005 conditionally selected provider ID `openai` and the versioned
`gpt-4.1-mini-2025-04-14` candidate. It also established the stateless
Responses API boundary, `store=false`, no tools, bounded evidence and output,
structured answers, exact citation-ID validation and typed failure outcomes.
Those safety and grounding requirements remain appropriate.

The current implementation uses a project-owned direct-HTTP adapter in
[Infrastructure](../../src/RagChallenge.Infrastructure/Providers/OpenAiHttpAdapters.cs).
It sends `POST /v1/responses`, requests strict JSON-schema output, fixes
`temperature=0`, omits reasoning configuration and accepts exactly one output
message. It has been exercised only through an in-process fake handler, not
against an OpenAI account or model.

The current official model pages establish these public facts:

- [`gpt-5.4-mini`](https://developers.openai.com/api/docs/models/gpt-5.4-mini)
  is intended as a faster, efficient GPT-5.4-family model, supports the
  Responses API and Structured Outputs, defaults to `reasoning.effort=none`,
  and publishes the dated `gpt-5.4-mini-2026-03-17` snapshot;
- [`gpt-5.6-sol`](https://developers.openai.com/api/docs/models/gpt-5.6-sol)
  is the GPT-5.6 frontier model for complex professional work, supports the
  Responses API and Structured Outputs, and currently publishes only the
  mutable `gpt-5.6-sol` identifier rather than a dated snapshot;
- the current
  [GPT-5.6 guidance](https://developers.openai.com/api/docs/guides/latest-model)
  recommends explicit reasoning-effort selection and representative
  evaluation rather than assuming that the highest effort is the right
  production setting.

Public positioning is not RAG-Challenge evidence. No current source supplies
project-specific groundedness, citation, prompt-injection or latency results.

## Decision drivers

- Ship the first online MVP against a dated and reproducible model snapshot.
- Preserve the existing stateless, tool-free and fail-closed provider
  boundary.
- Preserve exact `pt-BR` and `en-GB` answer-language behaviour and original
  citation language.
- Preserve exact retrieved-chunk citation validation and never fill evidence
  gaps with uncited parametric knowledge.
- Meet the accepted end-to-end query-latency thresholds and 25-second
  deadline.
- Avoid adding mutable-model drift to the first release without a measured
  product benefit.
- Keep a later frontier-model evaluation possible without creating a silent
  runtime fallback or dynamic provider switch.
- Exclude cost from model ranking without weakening existing spend and
  external-action controls.

## Proposed decision

### MVP candidate

If this ADR is accepted, select:

- provider ID: `openai`;
- model ID: `gpt-5.4-mini-2026-03-17`;
- model revision: `gpt-5.4-mini-2026-03-17`;
- API: Responses API;
- execution mode: standard, stateless and tool-free;
- storage: `store=false`;
- initial reasoning baseline: `reasoning.effort=none` and
  `reasoning.context=current_turn`;
- response: the existing strict structured-answer schema with bounded output
  and retrieved chunk IDs only.

The implementation may send `temperature=0` only when the accepted provider
contract supports it for the selected reasoning configuration. It must not
simulate determinism with an ungrounded repair call or silently substitute a
different model.

This model is a candidate, not an approved provider runtime. Acceptance would
authorise the architecture selection only. Adapter implementation, account
configuration, secret references, egress, paid evaluation and deployment each
remain separately authorised work.

### Deferred frontier candidate

Retain `gpt-5.6-sol` only as a future evaluation candidate. It is not an
active fallback, secondary model or runtime switch target.

A future proposal may promote it only after:

1. a dated snapshot is published, or the owner explicitly accepts mutable
   alias drift as a reproducibility risk;
2. its exact request and response contract is supported by the adapter;
3. it passes the same frozen bilingual, grounding, citation, insufficient-
   evidence and prompt-injection evaluation as the MVP candidate;
4. it meets the accepted latency thresholds on the named homologation
   environment; and
5. it demonstrates a material and repeatable quality improvement.

The generic `gpt-5.6` alias must not be used as a substitute for the explicit
`gpt-5.6-sol` candidate.

## Adapter compatibility

The existing `ILanguageModel` port and public query contract remain suitable.
The documented models both support `/v1/responses` and Structured Outputs, so
no Domain, Application or OpenAPI change is proposed.

Before the proposed MVP candidate can be called, a separately authorised
Infrastructure increment must:

1. make reasoning effort and reasoning context typed, non-secret and
   immutable for the running process;
2. emit only parameters supported by the selected model configuration;
3. preserve `store=false`, no tools, bounded input and output, the exact API
   authority and the existing cancellation policy;
4. accept the authorised final structured message while safely handling or
   rejecting any other documented output-item type;
5. validate the observed model against the exact dated snapshot;
6. keep invalid structure, unsupported citations, refusal and transport
   failures typed and sanitised; and
7. add fake-handler contract tests before any separately authorised provider
   call.

These requirements describe future acceptance and implementation work; they
do not authorise a code change under this proposal.

## Bilingual and grounded-answer evaluation

The candidate must use the frozen matrix and thresholds from
[ADR-0004](ADR-0004-MVP-Corpus-Official-Source-And-Evaluation.md) and the
[RAG module](../../prompts/foundation/RAG-Module.md). Each question/evidence
pair remains independently reportable:

| Question language | Evidence language | Required answer language | Citation text |
|---|---|---|---|
| `pt-BR` | `pt-BR` | `pt-BR` | Preserve original `pt-BR`. |
| `en-GB` | `en-GB` | `en-GB` | Preserve original `en-GB`. |
| `pt-BR` | `en-GB` | `pt-BR` | Preserve original `en-GB`. |
| `en-GB` | `pt-BR` | `en-GB` | Preserve original `pt-BR`. |

Every exact additional document-language tag remains a separate stratum.
`en` is never counted as `en-GB`.

The model candidate is blocked if any of these accepted hard requirements
fail:

- citation identity and location validity: `1.00`;
- answer language equals the declared question language: `1.00`;
- source-derived citation text remains in its original language: `1.00`;
- supported factual claims: at least `0.95`;
- correct insufficient-evidence outcome: at least `0.95`;
- unsupported high-impact factual claims: `0`;
- successful instruction override from retrieved content: `0`;
- incorrect provenance or silent source substitution: `0`; and
- stale, withdrawn or deactivated source calls to embedding or language-model
  providers: `0`.

Prompt-injection cases must attempt to replace trusted instructions, change
the answer language, cite an unretrieved chunk, disclose hidden configuration,
use parametric knowledge or invoke a tool. The acceptable outcomes are a
grounded structured answer, `InsufficientEvidence` or a typed failure; the
retrieved instruction never gains authority.

A documented two-person human rubric remains the answer-quality authority. A
model judge may supplement but cannot solely decide the result.

## Latency and selection rule

The accepted thresholds remain:

- query p95 at most `12 s`;
- query p99 at most `20 s`; and
- end-to-end server deadline `25 s`, with cancellation propagated.

No model-specific latency has been observed. A future comparison must use the
same frozen dataset, prompt version, evidence order and limits, release
artefact, environment and sample-count rule. It records p50, p95, p99,
timeouts, refusals and invalid-structure outcomes separately.

`gpt-5.4-mini-2026-03-17` remains the proposed MVP candidate unless a later
decision is supported by a frontier candidate that passes every hard gate and
shows a material, repeatable quality improvement within the latency limits.
Cost does not break a quality tie in this proposal. Reproducibility and lower
observed latency do.

## Mutable-identifier risk

The public `gpt-5.6-sol` page currently lists no dated snapshot. Behaviour may
therefore drift while the configured identifier remains unchanged. Exact
string matching cannot detect every behavioural change.

Any future use of that identifier requires a complete compatibility record
covering model ID and observed revision, prompt version, structured-output
schema digest, reasoning effort, reasoning context, execution mode and all
bounded generation settings. A model or behaviour change requires a new
evaluation baseline and must not silently alter the active runtime.

## Alternatives

### Retain `gpt-4.1-mini-2025-04-14`

Viable and currently accepted by ADR-0005, but not proposed for the first
online MVP because the owner has selected the newer dated 5.4 mini candidate
for evaluation. It remains the architectural status quo until this ADR is
separately accepted.

### Select `gpt-5.6-sol` immediately

Deferred. Its frontier positioning is a quality hypothesis, but the mutable
identifier, explicit reasoning contract, adapter changes and unobserved
latency introduce avoidable first-release risk without project evidence of a
material gain.

### Activate both models

Rejected for the MVP. A runtime fallback or dynamic selection would violate
the one-language-model MVP boundary, complicate reproducibility and create
unmeasured behavioural differences.

### Select a local language model

Not reconsidered by this proposal. Local models remain the separately
documented fallback if external-provider data terms or recurring cost are
rejected; no local runtime, licence, capacity or quality evidence is added
here.

## Consequences if accepted

- Only ADR-0005's language-model candidate changes; its provider disclosure,
  data-control, secret, egress, persistence, embedding and OCI decisions stay
  unchanged.
- `gpt-5.4-mini-2026-03-17` gains architectural-candidate status, not runtime
  or homologation status.
- The adapter requires a separately authorised compatibility increment before
  a provider call.
- The exact model and generation configuration remain traceable in answer
  evidence.
- `gpt-5.6-sol` remains inactive and cannot be enabled by configuration alone.
- A model change continues to require a new compatibility key and evaluation
  baseline as required by ADR-0005.
- Existing provider spend limits remain enforceable even though cost was not
  used to rank the two candidates.

## Acceptance checks

Before this proposal can be presented for a decision, confirm that:

- it remains `proposed` and records no acceptance date or decision authority;
- it changes no current factual state, lifecycle record, OpenAPI artefact,
  source, test, configuration, provider resource or corpus;
- its public model facts still match the official OpenAI documentation;
- the dated 5.4 mini snapshot and mutable Sol identifier are stated exactly;
- it preserves the accepted bilingual, citation, grounding, insufficient-
  evidence, prompt-injection and latency thresholds;
- it does not infer account availability, runtime compatibility or model
  quality from public documentation; and
- any later acceptance uses an explicit product-owner decision naming
  `ADR-0013` and does not authorise implementation, provider access or
  lifecycle advancement by implication.
