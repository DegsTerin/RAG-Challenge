# Governance and Controlled Execution

## Authority

Precedence and routing are defined in
[`../Start-Here.md`](../Start-Here.md). This document is the thematic authority
for states, transitions, controlled execution and factual memory. No template,
roadmap, proposed ADR or historical report changes the project state.

Owner communication and project artefacts apply the thematic authority of the
[language policy](Language-Policy.md).

## Canonical states

1. `STATE-00 DISCOVERY`
2. `STATE-01 PROJECT_SETUP`
3. `STATE-02 ARCHITECTURE`
4. `STATE-03 DATA_AND_INDEX_MODELING`
5. `STATE-04 BACKEND_IMPLEMENTATION`
6. `STATE-05 FRONTEND_IMPLEMENTATION`
7. `STATE-06 INTEGRATION`
8. `STATE-07 TESTING_HOMOLOGATION`
9. `STATE-08 PRODUCTION_RELEASE`

The normal flow is sequential. A transition requires:

1. state deliverables;
2. the applicable automatic audit;
3. a factual report;
4. an explicit Human Gate for one state;
5. an append-only entry;
6. a factual-state update.

An approved audit, accepted ADR or authorised batch does not advance the
lifecycle automatically.

Between closing `STATE-00` and entering `STATE-01`,
`GATE-B01 ARCHITECTURE_BOOTSTRAP_DECISION` must:

1. explicitly accept or reject ADR-0001;
2. select the repository licence;
3. record the physical project decomposition proportionate to the MVP;
4. map conceptual modules to namespaces/directories, permitted dependencies
   and architecture tests;
5. decide whether the one-shot administrative operation uses the main host or
   justifies a separate tool project.

When approved, ADR-0001 is the canonical record of the physical map, the
module/namespace mapping, dependencies/tests and administrative form. The
human gate decision, selected licence and evidence belong in a new append-only
entry in [`../state/State-Transition-Log.md`](../state/State-Transition-Log.md);
[`../state/Current-State.md`](../state/Current-State.md) receives only the
resulting factual snapshot.

This gate decides only the bootstrap. It does not accept ADR-0002, select
providers or corpus, or authorise Git, scaffolding or entry into `STATE-01`.
After the gate, entry into `STATE-01` still requires separate human authority.

## Canonical module IDs

- `CH-MOD-01 CORPUS_CATALOG`
- `CH-MOD-02 DOCUMENT_INGESTION`
- `CH-MOD-03 INDEXING_RETRIEVAL`
- `CH-MOD-04 ANSWER_GENERATION`
- `CH-MOD-05 QUERY_EXPERIENCE`
- `CH-MOD-06 OPERATIONS_GOVERNANCE`
- `CH-MOD-07 OFFICIAL_EXTERNAL_SOURCES`
- `CH-MOD-08 EXTERNAL_INTEGRATION_CONTRACTS`

IDs must not be reused with another meaning. Module 07 integrates the MVP
through governed records for compatible official sources, without crawling or
arbitrary public URLs. In the MVP, module 08 owns only the public
RAG-Challenge HTTP/OpenAPI contract; any consuming adapter, including a future
DB-Notifier adapter, belongs to the consuming repository and its own decisions.

The corrective `3.0.1` baseline, still before the Human Gate, replaced the
ambiguous `DB_NOTIFIER_ADAPTER` label with
`EXTERNAL_INTEGRATION_CONTRACTS`. The responsibility remains the versioned
integration boundary; the ID was not reused for an unrelated capability.

## Execution protocol

1. Read instructions, vision, state and thematic rules.
2. Inspect the workspace, version control and pre-existing changes.
3. Confirm state, authority, deliverables and negative scope.
4. Classify runtime preflight as `NOT_APPLICABLE` without inspecting processes
   for documentation/read-only analysis; execute it only when the next action
   changes or validates executable behaviour.
5. Plan the smallest coherent change and proportionate validation.
6. Implement only the authorised scope.
7. Run real checks and preserve sanitised evidence.
8. Review the diff, security, links and claims.
9. Update state and history only after the fact occurs.
10. In the final response to the request, deliver the compact hand-off exactly
    once, covering the request, next recommended work, state/gate, owner
    action, routing, copy-ready text, Codex reasoning and safe parallelism.

## Continuity between conversations

Repository documentation is the source of truth. Conversations are temporary
working contexts and do not replace `Current-State.md`, append-only history,
ADRs, reports or future commits.

Each owner request receives exactly one hand-off, only in the final response of
the logical turn. It communicates continuity without mixing work, lifecycle,
human action and routing, and includes a complete `pt-BR` message in the same
closing block when continuity requires the owner to copy and send text.

The response's thematic boundary is the owner's current explicit request. A
confirmation, clarification, correction or narrow follow-up does not itself
reactivate the next project state, future backlog, optional improvement or a
previously discussed subject. Body and hand-off address the same topic; they
introduce additional work only when it follows directly from the current
request or is needed to complete or unblock it. When the owner narrows the
topic or identifies mixed subjects, apply the narrower boundary immediately
and do not repeat the rejected derivation.

Intermediate updates within the same request are not new hand-offs. They stay
brief, limited to progress, observed evidence, a non-blocking assumption or a
blocker, and each adds materially new information. Do not repeat, paraphrase
or echo an already communicated conclusion, including a worker result, except
to correct it or explain a changed consequence. Do not preview the complete
sequence of status, continuity, message, reasoning and parallelism. If the
owner supplements the request before the final response, incorporate the new
context and still provide only one closing hand-off.

### Continuity vocabulary

Use these terms without interchange:

- `Solicitação`: combines status (`concluída`, `parcial` or `bloqueada`), the
  concrete result and pending items for the current request; it does not
  include future backlog, another gate or an optional improvement;
- `Próximo trabalho recomendado`: one concrete deliverable expressed as a
  prioritised action directly related to the current request that may occur
  after this response, with owner and condition/authority. It is the canonical
  answer to the owner's question about the next step, task, activity or action.
  It is neither the owner's navigation action nor automatic authority or
  transition. When a related continuation lacks data, decision or authority,
  obtaining it is the next work and the hand-off states the exact condition.
  Use `nenhum — a solicitação atual não exige trabalho adicional` only when no
  directly related actionable continuation exists; do not import an unrelated
  lifecycle or backlog item merely to fill the field;
- `Estado/gate`: current lifecycle position, next named state or gate and entry
  condition only when material to the current request; use `sem mudança` when
  no transition applies to the topic;
- `Sua ação agora`: only the response, decision, authority, datum or navigation
  that the owner must perform immediately to enable the next work; use
  `nenhuma` only when the next action does not depend on the owner or when no
  actionable continuation genuinely exists;
- `Conversa recomendada`: suggested location for the next work, with route,
  target and reason; it does not describe the deliverable or grant authority;
- `Texto para copiar e enviar`: complete payload that materialises the owner's
  action in the recommended conversation; it appears immediately afterwards
  and uses the copy-ready presentation defined in
  [`../templates/Templates.md`](../templates/Templates.md). It may be declared
  unnecessary only when no immediate action depends on a message.

`Lote` is a governed unit that groups work. `Tarefa` is a plan subunit with a
verifiable deliverable. `Atividade` is an internal operation and `passo` is an
ordered procedural item. `Etapa` is not a generic synonym: use the canonical
state or gate. None of these terms replaces request, next recommended work,
state/gate or owner action.

Apply the request status as follows:

- `concluída`: pending items are `0`; any next work within the same thematic
  boundary is a future recommendation, not unfinished work in the request;
- `parcial`: pending items list what remains in the request; the next work is
  the first pending item;
- `bloqueada`: pending items identify the blocker; the next work is the
  unblocking condition and `Sua ação agora` states exactly what the owner must
  provide, when applicable.

Every hand-off states exactly one next recommended work item. Completing the
current request, being able to wait or not yet having execution authority does
not remove a directly related continuation. Before using canonical absence,
inspect factual state and the owning documents to identify the first useful,
governed action. If it depends on owner authority, data, a document, a decision
or an attachment, the hand-off names obtaining it as the next work, fills
`Sua ação agora` and provides the complete payload. This rule does not
authorise importing an unrelated state, gate, backlog item or improvement.

When an owning document records a dependency order or named increment sequence
and the current item is complete, the first incomplete item in that order has
priority as the next work. If it lacks authority, the next action is to obtain
bounded owner authority, not to review the completed result generically.
`Revisar os commits`, `considerar a continuidade`, `avaliar se deseja seguir`
or equivalent wording may occupy the field only when that review or decision
is an explicitly named gate, prerequisite or deliverable. When the owner asks
directly for the next step, task, activity or action, present that action
before recapping completed work.

The final hand-off explicitly classifies the next interaction:

- `CONTINUE_CURRENT`: the same state/batch and objective remain active, the
  current context is useful and restarting has no material benefit;
- `START_NEW`: another relevant state/gate or objective/batch begins, the topic
  is independent, the conversation became excessively long or contradictory,
  a review needs isolation, or no reliable previous-conversation reference
  exists;
- `RETURN_TO_EXISTING`: the work clearly belongs to a still-applicable earlier
  conversation identified by a title or label that the owner supplied or
  confirmed.

The agent recommends; the owner navigates manually. The agent does not claim to
have opened, renamed, located or switched conversations. If a title, label or
ID is not known reliably, do not invent it: recommend `START_NEW` and propose a
descriptive title in the form
`RAG-Challenge — <STATE-OU-GATE> — <OBJETIVO-CURTO>`.
This title is only a suggestion, not a canonical identifier. The new
conversation's initial message repeats it as `Identificação da conversa`;
when the owner sends that message, the identification becomes a confirmed
reference for future hand-offs even if the interface displays another title.

When continuity depends on a message, the final hand-off provides a ready-to-use
`Texto para copiar e enviar` block. The text:

1. identifies `RAG-Challenge`, the state/gate and intended batch;
2. requires rereading `AGENTS.md`, `prompts/Start-Here.md`,
   `prompts/state/Current-State.md` and relevant thematic documents;
3. states the objective, already recorded authority, positive and negative
   scope;
4. states checks, expected result and stop condition;
5. requires factual-state confirmation before acting;
6. does not invent approval or broaden authority by carrying context from
   another conversation.

### Stage 0/1/2 hand-off rule

A copy-ready payload that authorises, prepares, continues, resumes or
coordinates executable or documentary RAG-Challenge work must explicitly
require the receiving coordinator to apply in full all applicable controls and
capabilities incorporated from Stage 0, Stage 1 and Stage 2.

`Stage 0`, `Stage 1` and `Stage 2` are descriptive labels for controls already
incorporated into tracked project authorities and tooling. The original Stage
owner-input documents are historical, inactive and non-normative inputs. They
never constitute a parallel authority, and a payload must not direct an agent
to treat or reread them as normative instructions.

Applying the controls in full means applying every control relevant to the
authorised work. Depending on the task, this includes authority reconstruction,
proportionate specialist-role selection, parallel-work classification, closed
task envelopes, isolated branches, worktrees and mutable resources for write
lanes, independent review, canonical stop conditions, durable state and
checkpoints, deterministic sequential integration, and the applicable
documentary or canonical gates.

The rule has exactly these limitations and exceptions:

1. It does not create a copy-ready message when no useful message is required.
2. It does not require creating agents artificially for simple work.
3. It does not convert `SEQUENTIAL_ONLY` work into parallel work.
4. It does not treat multi-agent readiness as continuous authority.
5. It does not broaden scope, lifecycle, a Human Gate, provider authority or
   any external-action authority.
6. It does not add content to a payload whose only permitted content is the
   canonical Human Gate confirmation phrase.
7. It does not apply to a purely decisional payload that neither authorises nor
   coordinates project work.

Repeating the Stage instruction transports no authority beyond the authority
already recorded for the work. Templates materialises the applicable payloads
and the permitted exclusions without redefining this rule.

The target must match the action: `current`, `new` or
`existing — <título-ou-label-confirmado>`. `START_NEW` adds a suggested,
non-canonical title within `Conversa recomendada`; verifiability of an existing
conversation is required only for `RETURN_TO_EXISTING`.

When returning to an earlier conversation, the text requires reconciling its
context with `Current-State.md`; factual state and current authorities prevail
over any divergence. When starting a new conversation, fill every template
placeholder and propose a title.

When `Sua ação agora` tells the owner to continue, start, resume, respond,
confirm, decide, authorise or send something in a conversation, `Texto para
copiar e enviar` is mandatory, appears immediately after `Conversa
recomendada` and contains the complete `pt-BR` payload. Do not interpose
another label, prose, title or recommendation; the suggested `START_NEW` title
stays in the conversation field itself. Do not defer the text to another
response, point to a message supplied earlier or elsewhere in the response, or
use the absence sentinel. Route, destination, title, action and content must
be coherent. The label occupies its own line and the payload immediately below
uses the template's visually copyable fenced block; fences and external
guidance never form part of the text to send.

An attachment, file or datum that must not be reproduced in chat does not
replace the text: when sending it is necessary, the block contains the complete
accompanying instruction without embedding binary content or a secret.
Additional parallel-lane messages appear only in their conditional section and
never replace the main hand-off text.

When no immediate action depends on a message, the hand-off states
`Texto para copiar e enviar: nenhum texto é necessário`. If no owner action
exists either, it states `Sua ação agora: nenhuma` once and does not create an
artificial task, title, plan or message. Text absence is valid only when no
useful continuity depends on sending. Missing conditional fields are not
replaced by repetitive `nenhum` lists. `Sua ação agora: nenhuma` is
incompatible with any instruction to start, resume or send a message to
another conversation.

When no directly related later deliverable exists, state `Próximo trabalho
recomendado: nenhum — a solicitação atual não exige trabalho adicional`. This
absence is preferable to importing the project's next general state, creating
an owner decision or turning an informational response into implicit
authority. A completed request, a waiting project or missing current authority
is insufficient for absence when a concrete directly related action remains.

The standard format groups related data in compact lines: route, target, title
when applicable and reason in `Conversa recomendada`; level, justification and
fallback in `Raciocínio recomendado`; classification and reason in
`Paralelismo`. A plan and per-lane messages appear only for
`PARALLEL_OPTIONAL` or `PARALLEL_RECOMMENDED`.

A Human Gate phrase may be requested only with `CONTINUE_CURRENT`, target
`current`, alongside the complete summary and current baseline in the same
hand-off. For `START_NEW` or `RETURN_TO_EXISTING`, the message requires the
complete summary to be reissued and reviewed in the target conversation; it
never carries the confirmation phrase in isolation. External authorities and
architectural decisions remain subject to their own protocols. Even as a
single line, the phrase appears in the mandatory copy-ready block.
Conversation routing does not grant that authority.

## Codex reasoning recommendation by conversation

Every hand-off recommends a level for the next coordinating conversation. Each
auxiliary conversation receives its own recommendation. The level belongs to
the conversation or lane, not the entire lifecycle, and must be reassessed
when objective, risk, uncertainty, breadth or execution form changes.

Use the lowest level sufficient to produce a verifiable result. The canonical
owner-facing values and their usual correspondences, when the surface and
model provide them, are:

| Canonical level | Usual correspondence | Recommend when |
|---|---|---|
| `Leve` | `Light` / `low` | Status, routing, extraction, formatting or a short mechanical check with unambiguous scope and low risk. |
| `Médio` | `Medium` / `medium` | Normal, bounded, well-specified work with few local decisions and direct validation. |
| `Alto` | `High` / `high` | Material diagnosis, multi-file change, integration or analysis with alternatives, edge cases and several checks. |
| `Extra alto` | `Extra High` / `xhigh` | Architecture, security, contracts or complex cross-cutting analysis with material ambiguities and cross-area consequences. |
| `Máximo` | `Max` / `max` | An exceptionally difficult, tightly coupled problem, deep ADR/gate decision, migration or high-impact action where depth in one conversation outweighs time and usage. |
| `Ultra` | `Ultra` / `ultra` | Exceptional, critical work decomposable into independent fronts where proactive coordination and multi-agent review provide material benefit. |

The criteria are cumulative: risk and irreversibility; uncertainty and
ambiguity; breadth and number of affected contracts; required depth; genuine
decomposability; and verification cost. A gate or ADR does not receive
`Máximo` automatically: the recommendation depends on observed difficulty and
impact. `Máximo` favours depth in a coupled task; `Ultra` favours decomposition
and coordination. Recommend `Ultra` only when the parallel-work gate permits
`PARALLEL_OPTIONAL` or `PARALLEL_RECOMMENDED`; it never permits deciding an
ADR, Human Gate or transition in parallel.

Every recommendation records:

1. `Raciocínio do Codex recomendado`: exactly one of the six values;
2. `Justificativa do raciocínio`: why it is the lowest sufficient level;
3. `Alternativa se indisponível`: a supported level and compensating
   validation.

Availability varies by surface, account, model and configuration. Technical
names in the table are informative correspondences, not a promise that a
selector or configuration value exists in that context. The agent recommends
but does not claim to have changed configuration; the owner selects the level
when the control is available.

Do not silently substitute an unavailable level. Use this guidance:

- `Leve` unavailable: `Médio`;
- `Médio` unavailable: `Alto`, when available;
- `Alto` unavailable: `Médio` with additional checks;
- `Extra alto` unavailable: `Alto` with independent review;
- `Máximo` unavailable: `Extra alto` with independent review;
- `Ultra` unavailable: `Máximo` in the coordinator plus explicit governed
  decomposition; if `Máximo` is also unavailable, `Extra alto` with
  independent review.

The fallback preserves authority, negative scope, checks and stop condition. A
reasoning level does not choose a model, authorise a subagent, change the
parallelism classification, or broaden lifecycle, permissions, sandbox,
network, external consumption or mutations.

## Actions permitted by state

| State | Permitted | Not permitted without new authority |
|---|---|---|
| `STATE-00` | Inspection, inventory, requirements, risks, documentation, proposals and documentary validation | Scaffolding, code, dependencies, API, index, UI, Git initialisation or deployment |
| `GATE-B01` | ADR-0001 decision, licence, physical/module map and administrative form | Git initialisation, scaffolding, dependencies, code or acceptance of other ADRs |
| `STATE-01` | Separately authorised Git/scaffolding, accepted solution/projects, configuration, restore, build, lint, structural tests and CI | Functional ingestion or query rules |
| `STATE-02` | ADRs, contracts, threat model, provider selection, diagrams and authorised disposable spikes | Functional product or unapproved external consumption |
| `STATE-03` | Catalogue, document and index model, migrations and non-production rollback | Applying a migration or modifying operational storage |
| `STATE-04` | Domain, Application, RAG adapters, persistence, API and tests | Complete interface or public deployment |
| `STATE-05` | Dashboard, accessibility and UI tests | Unauthorised external integration/deployment |
| `STATE-06` | Local integration, sandboxed E2E, candidate artefact and unpublished OCI configuration | Production or support announcement |
| `STATE-07` | Authorised RAG evaluation, security, load, recovery, accessibility and homologation | Publication |
| `STATE-08` | Release, OCI, smoke, observability, evidence and rollback in the authorised target | Unrecorded functionality |

Secrets, paid consumption, remote action, publication and deployment always
require specific authority regardless of state.

## Architectural decisions

- ADRs start as `proposed`.
- An explicit human decision may make them `accepted`.
- Replacement uses a new ADR and preserves the previous one as `superseded`.
- Acceptance does not authorise implementation.
- Changes to stack, contracts, persistence, online sources, security,
  deployment or DB-Notifier integration require an ADR.
- A state's Human Gate does not accept ADRs by implication. Each architectural
  decision identifies the ADR and requested decision.

## Blocked state

Record:

- cause and evidence;
- impact and scope;
- safe attempts;
- possible independent work;
- dependency or owner;
- objective unblocking condition.

A blocker does not authorise skipping a state, weakening a gate or inventing a
result.

## Rollback

- Define the trigger, owner, target version and validation.
- Separate application, configuration, catalogue, document, index and
  deployment rollback.
- Preserve auditability and provenance.
- Prefer a new generation and atomic activation to in-place mutation.
- Never alter a documented or queried database as an effect of RAG-Challenge
  rollback.
- Use a forward fix when rollback increases risk, recording the decision.

## Project memory

- [`../state/Current-State.md`](../state/Current-State.md): factual present
  only.
- [`../state/State-Transition-Log.md`](../state/State-Transition-Log.md):
  append-only history.
- ADRs: decisions and replacements.
- `docs/STATE-*`: execution and gate evidence.
- [`../system/Prompt-System-Change-Log.md`](../system/Prompt-System-Change-Log.md):
  corpus evolution.

Do not rewrite historical evidence to appear current.

## Parallel and multi-agent work

The hand-off classifies parallelism separately from the coordinating
conversation route:

- `SEQUENTIAL_ONLY`: tasks depend on each other, ownership overlaps, a
  decision/gate is shared, a contract is unstable, runtime/data compete or
  isolation is insufficient;
- `PARALLEL_OPTIONAL`: independent safe fronts exist, but the gain is small or
  coordination cost may be equivalent;
- `PARALLEL_RECOMMENDED`: two or more independent, bounded and verifiable
  fronts materially reduce time without increasing risk.

Use the smallest useful number of fronts. A parallel recommendation defines:

1. one coordinating conversation, identified by an owner-confirmed title or
   label and responsible for baseline, decisions, integration, state, history
   and gates;
2. each worker conversation's identifier, route/target and suggested or
   confirmed label;
3. common objective, authority and preconditions;
4. an identifiable base snapshot, with corpus version and commit/hash when one
   exists;
5. exclusive paths, logical artefacts and mutable resources, or `read-only`
   classification;
6. read-only inputs, dependencies and expected outputs;
7. explicitly forbidden files and actions;
8. checks, evidence, stop condition and exact return message;
9. recommended reasoning, justification and alternative for the coordinator
   and each worker;
10. deterministic integration order and final global checks;
11. fallback to sequential execution.

Good candidates include inventories, research and read-only audits;
independent reviews; documentation in non-overlapping areas; and, after
contracts are frozen and isolation is authorised, modules or tests that share
neither files nor mutable state.

Do not parallelise:

- a front that depends on another front's not-yet-integrated output;
- concurrent changes to the same file, contract, schema, migration, lockfile,
  manifest, project/solution file, configuration or pipeline;
- ADR decisions, lifecycle transitions and Human Gates;
- operations on the same branch/worktree, port, process, database, index,
  mutable corpus, secret or external resource;
- tasks whose conflict would be discovered only after an irreversible change.

`AGENTS.md`, Start Here, Current State, history, the change log, and gate and
decision records remain under the coordinating conversation's ownership during
the batch. A shared technical contract may belong to one designated front;
every other front treats it as a frozen read-only input.

Before a tracked Git repository exists, simultaneous conversations may perform
only read-only analysis, review and audit. Workspace writes are sequential and
belong to the coordinator. After Git and the corresponding workflow are
authorised, writing fronts use separate branches and worktrees, disjoint
ownership and isolated ports, stores, temporaries and build outputs. A
different branch in the same worktree is not isolation.

Each worker:

- rereads the authorities and confirms the baseline before acting;
- receives its own reasoning recommendation, justification and alternative,
  without assuming inheritance from coordinator configuration;
- executes only its front and does not integrate another front's work;
- does not broaden authority, receive secrets or take a human decision;
- stops on overlap, unexpected change, missing dependency, stale baseline,
  runtime collision or required new authority;
- returns files/artefacts, checks, limitations and a coordinator-ready block.

The coordinator integrates one front at a time, reconciles the result with the
current baseline, resolves conflicts, repeats necessary local checks and runs
the final cross-cutting audit. Only afterwards does it update state, history,
the report and any Human Gate summary.

`Complete` for a worker means a delivered candidate, never an integrated batch
or completed state. The fallback freezes only the affected front, preserves
its evidence and resumes it sequentially from the last confirmed baseline; it
never uses last-write-wins or automatically reverts another owner's work.

### Mandatory task envelope

Every executable delegation or parallel lane receives a closed envelope before
starting. The coordinating conversation owns the envelope; a worker does not
fill gaps by granting itself authority. The mandatory minimum is:

```text
TASK_ID
objective
authority
owner
baseline
execution_surface
allowed_paths
forbidden_paths
dependencies
shared_resources
acceptance_criteria
required_tests
stop_conditions
deliverables
```

- `authority` identifies the applicable request, requirement, accepted ADR and
  execution authority, plus negative scope; an earlier conversation is context,
  not persistent authority.
- `baseline` fixes the branch, HEAD, tree state, corpus version and relevant
  protected contracts.
- `execution_surface` fixes `cwd`, worktree, writable roots, sandbox, approval,
  network, environment policy and the effective allowlist of tools, MCPs and
  skills; omitted or inherited configuration is never presumed safe.
- `allowed_paths` and `forbidden_paths` are explicit sets; absence from
  `allowed_paths` grants no implicit write.
- `dependencies` distinguishes already integrated inputs from pending work.
- `shared_resources` declares each resource's ownership, mutability,
  namespace/lease and isolation method.
- `required_tests` distinguishes focused checks, integrated gate, external
  checks and human evidence; running zero tests is never PASS.
- `stop_conditions` includes the canonical codes below and any additional task
  boundary.
- `deliverables` requires files/artefacts, diff, commands, results, limitations
  and a return message to the coordinator.

A task without a complete envelope remains `NOT_READY`. The coordinator may
request read-only exploration to close the fields but may not delegate writing
or reserve a mutable resource until the envelope is verifiable.

Before the first write, the coordinator independently confirms the resolved
`cwd`, worktree root, branch, HEAD, tree state, writable roots, sandbox,
approval, network, environment policy, tools, MCPs, skills and every envelope
field returned by the worker. Live parent-process overrides may replace custom
agent defaults; any surface broader than the envelope allowlist prevents
dispatch. Textual instruction and `sandbox_mode` constrain behaviour but do
not prove isolation or disable built-in tool roles. Only the project-defined
roles below are materialised; the coordinator does not dispatch writing to a
generic built-in role.

### Operational parallelism taxonomy

This taxonomy classifies operations; it does not replace the hand-off's
owner-facing `SEQUENTIAL_ONLY`, `PARALLEL_OPTIONAL` or
`PARALLEL_RECOMMENDED` classification.

| Class | Rule |
|---|---|
| `SAFE_PARALLEL` | Analysis, inventory, review or testing with read-only inputs and fully isolated outputs/resources. |
| `CONTRACT_FROZEN_PARALLEL` | Writing in disjoint lanes only after shared contracts have an owner, identity/hash and frozen baseline. |
| `SINGLE_OWNER` | One owner writes the artefact or resource; other lanes may consume it only as a frozen input. |
| `SEQUENTIAL_ONLY` | A decision, mutation or integration involving dependency, human authority, shared state, irreversibility, one-shot execution or insufficient isolation. |

`SAFE_PARALLEL` ceases to be safe with shared output, a non-isolated mutable
cache, a fixed port, global process action or unexpected dirty tree.
`CONTRACT_FROZEN_PARALLEL` ends at the first contract-change request; the lane
stops with the corresponding code and the owner replans. A `SINGLE_OWNER`
operation may coexist only with genuinely disjoint work; it never permits two
authors to alternate writes to the same file or store.

ADR acceptance/replacement, a Human Gate, lifecycle transition, human
adjudication, candidate integration, shared-contract change, ordered migration,
release, deployment, rollback, destructive operation and any one-shot campaign
are always `SEQUENTIAL_ONLY`. Parallel evidence production does not
parallelise the decision or its record.

### Artefact ownership

Each path and logical lane artefact receives exactly one class:

| Class | Semantics and examples |
|---|---|
| `READ_ONLY_FOR_WORKERS` | Authorities and inputs workers inspect without editing, including `AGENTS.md`, Start Here and decisions outside their lane. |
| `SINGLE_OWNER` | OpenAPI, shared DTO/contract, schema, migration with designer/snapshot, solution/project, lockfile, CI, configuration or mutable manifest. |
| `LANE_OWNED` | Implementation/tests/documentation explicitly assigned to one lane, branch and worktree. |
| `SHARED_BUT_FROZEN` | Contract, fixture, corpus or golden input identified by version/hash and read-only for every lane during the batch. |
| `GENERATED` | Build, coverage, package, cache, temporary or reproducible output, always task-owned and never a source of authority. |
| `HUMAN_CONTROLLED` | Requirement, scope change, ADR/risk acceptance, adjudication, Human Gate, lifecycle, provider, billing, production and release. |
| `COORDINATOR_ONLY` | Current State, history, change log, gate records, integration and the consolidated batch report. |

The owner of a shared contract does not become owner of a requirement, gate or
human decision. A generated file cannot be promoted to evidence without
readback, identity and command/baseline binding. A worker never removes an
output, branch, worktree or store without a task-owned marker and namespace.

### Mutable resources and isolation

Before dispatching writing or executable validation, the coordinator
inventories the resources below and records their isolation in the envelope:

| Resource | Minimum rule |
|---|---|
| Worktree and branch | Exclusive to each writing lane; a distinct branch in the same worktree is insufficient. |
| `bin/`, `obj/`, `node_modules/`, `dist/` and caches | One worktree per execution; a mutable global cache requires its own namespace or sequential execution. |
| Coverage, TestResults, artefacts, temporaries and golden outputs | One task-owned root; no fixed default may be shared between executions. |
| SQLite, PostgreSQL, vector store, corpus and index | Exclusive database/store or single lease; a frozen corpus/index is read-only. |
| Ports, listeners, processes, browser profiles and containers | Exclusive port/profile/container with verifiable ownership; an isolated precheck does not replace a lease. |
| Secrets, credentials, providers and external resources | Never shared with a worker; use only under specific authority, least privilege and its own gate. |
| Tools, MCPs, skills, apps, connectors and plugins | Exact per-lane allowlist; inheritance, discovery or availability grants no use. An unexpected external surface blocks dispatch. |

The complete `eng/ci.ps1` gate does not run concurrently in one worktree.
Restore/build/test and `npm ci` share outputs even when the coverage directory
is unique. The final gate runs sequentially on the integrated baseline.

Locks are proportionate to risk and bound to `TASK_ID`, lane, resource, owner
and acquisition instant. A stale lock is not stolen automatically. Resume or
cleanup revalidates process, baseline, path, task-owned marker and external
state; doubt preserves the resource and escalates. A global lock that removes
all parallelism is prohibited when disjoint namespaces resolve the risk.

### Canonical stop conditions

Every agent stops before the blocked action, preserves observed evidence and
returns one of these codes without retry, fallback or silent broadening:

| Code | Condition |
|---|---|
| `AMBIGUOUS_AUTHORITY` | Unambiguous authority or negative scope cannot be identified. |
| `CONFLICTING_REQUIREMENTS` | Material applicable sources require incompatible results. |
| `ARCHITECTURE_CHANGE_REQUIRED` | The task depends on a new stack, boundary or architectural decision. |
| `PUBLIC_CONTRACT_CHANGE_REQUIRED` | The result requires changing a public or frozen shared contract. |
| `SCHEMA_CHANGE_REQUIRED` | A schema change is required without its own owner/authority. |
| `MIGRATION_REQUIRED` | A migration or a change to an already assigned sequence is required. |
| `DESTRUCTIVE_OPERATION` | Continuing would delete, overwrite or make data/state difficult to recover. |
| `SECRET_REQUIRED` | Continuing depends on an unavailable or unauthorised secret. |
| `PROVIDER_CHANGE_REQUIRED` | Continuing changes provider, model, egress, cost or external surface. |
| `HUMAN_DECISION_REQUIRED` | Continuing depends on a requirement, ADR/risk acceptance, adjudication or other human decision that is not a lifecycle Human Gate. |
| `HUMAN_GATE_REQUIRED` | Continuing specifically depends on the Human Gate of one `STATE-ID`, with a complete summary and canonical phrase. |
| `UNEXPECTED_DIRTY_TREE` | Branch, HEAD, diff or untracked state changed outside the observed envelope. |
| `SHARED_RESOURCE_COLLISION` | Another owner/process uses the same mutable resource or its isolation cannot be proved. |
| `OUT_OF_SCOPE_CHANGE_REQUIRED` | Acceptance requires a file, behaviour or authority outside the task. |
| `TEST_BASELINE_BROKEN` | The baseline or mandatory gate fails before the failure can be attributed to the lane. |

A material baseline change requires a new envelope or explicit coordinator
revalidation. `HUMAN_DECISION_REQUIRED` does not convert an owner decision into
a Human Gate. `HUMAN_GATE_REQUIRED` does not mean a worker may request or
record the gate phrase. The return identifies the fact, impact, safe
independent work, owner and objective unblocking condition.

### Specialist roles

The project-scoped configuration in `.codex/agents/` defines these project
roles without disabling built-in tool roles or granting additional authority:

- `governance_guard`: read-only; reconstructs authority, lifecycle, ADRs,
  gates and stop conditions;
- `code_mapper`: read-only; maps dependencies, ownership, tests and resources;
- `architect`: read-only; identifies boundaries, contracts and ADR need
  without accepting its own proposal;
- `implementation_worker`: `workspace-write` only in the isolated lane and
  received envelope;
- `independent_reviewer`: read-only and independent of the judged
  implementation;
- `security_reviewer`: read-only; reviews secrets, trust boundaries, inputs,
  filesystem, subprocesses, provider, logging and supply chain.

No role accepts a requirement, risk, ADR, adjudication, Human Gate, lifecycle,
provider, billing, production or release. The sandbox limits tools; it does not
replace ownership, scope or authority.

## Guard rails

- Do not invent evidence, runtime, licence, model, price or approval.
- Do not mix retrieved content with trusted instructions.
- Do not present stale data, an incompatible index or an unavailable source as
  healthy.
- Do not announce a provider or format before implementation and homologation.
- Do not initialise Git, install a dependency, access a model or publish by
  inference.
- Do not access an official source merely because it belongs to the MVP; every
  network execution requires its own state, configuration and authority.
- Do not confuse local operation, CI, deployment and release.
- Do not create a direct DB-Notifier dependency.
