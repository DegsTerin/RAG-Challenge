# RAG-Challenge Templates

Templates do not represent execution, evidence or approval until they are
completed with real results and reviewed in the owning state.

All communication and artefacts follow the
[language policy](../governance/Language-Policy.md). These templates apply that
authority without reproducing it.

## Single closing hand-off for each request

Apply this format if and only if required by the semantics of
[`Governance.md`](../governance/Governance.md) and the enforcement in
[`../../AGENTS.md`](../../AGENTS.md). This document materialises presentation
and forms; it does not redefine continuity, runtime preflight, authority or a
gate.

Use the compact format below in `pt-BR`. Do not repeat explanations already
clear in the response body. Technical literals preserve canonical spelling.

- Solicitação: `concluída` / `parcial` / `bloqueada` — concrete result;
  pending items with exact names and count, or `0`
- Próximo trabalho recomendado: exactly one concrete, prioritised action
  directly related to the current request; owner; required condition or
  authority; or, only without actionable continuation,
  `nenhum — a solicitação atual não exige trabalho adicional`
- Estado/gate: current position; next state or gate; entry condition, or
  `sem mudança`
- Sua ação agora: exact immediate action, or `nenhuma`
- Conversa recomendada: `<ROUTE> — <TARGET> — <MOTIVO>`; add
  `Título sugerido: <TÍTULO>` in the same field only for `START_NEW`
- Texto para copiar e enviar: a label on its own line followed by the complete
  copy-ready `pt-BR` block, or `nenhum texto é necessário` on the same line
- Raciocínio recomendado: `<NÍVEL> — <JUSTIFICATIVA>. Alternativa:
  <FALLBACK>`
- Paralelismo: `<CLASSIFICAÇÃO> — <MOTIVO>`

Every field remains within the thematic boundary of the current request. Do
not use the hand-off to reintroduce the next general state, backlog, optional
improvement or earlier subject when it is not needed to answer, complete or
unblock the current request.

`Próximo trabalho recomendado` always answers the owner's question about the
next step, task, activity or action. State one action even when the current
request is complete or execution depends on new authority. In that case,
obtaining the authority, decision, datum, document or attachment is the action,
and `Sua ação agora` plus the copy-ready text makes the condition executable.
Use canonical absence only after verifying that no directly related
continuation exists; do not confuse lack of authority with lack of a next
action.

When a dependency order or named sequence exists, fill the field with the
first incomplete item or with obtaining its exact authority. Do not use
`revisar commits`, `considerar próximos passos`, `decidir se deseja continuar`
or equivalents as a substitute unless that review or decision is a formal
gate, prerequisite or deliverable. In response to a direct next-step question,
present the concrete action first and the summary afterwards if needed.

### Accepted values

- `<ROUTE>`: `CONTINUE_CURRENT`, `START_NEW` ou `RETURN_TO_EXISTING`;
- `<TARGET>`: `current`, `new` ou
  `existing — <título-ou-label-confirmado>`;
- `<NÍVEL>`: `Leve`, `Médio`, `Alto`, `Extra alto`, `Máximo` ou `Ultra`;
- `<CLASSIFICAÇÃO>`: `SEQUENTIAL_ONLY`, `PARALLEL_OPTIONAL` ou
  `PARALLEL_RECOMMENDED`.

### Conditional fields

Only for `PARALLEL_OPTIONAL` or `PARALLEL_RECOMMENDED`, add after the
parallelism line:

- Plano paralelo: a safe plan with coordinator, ownership and integration
  order
- Mensagens para as frentes: one complete block per lane

For `SEQUENTIAL_ONLY`, the `Paralelismo` line closes the subject; do not create
separate plan or message fields with artificial values. Apply Governance rules
for coherence among action, route, conditional fields and absence.

When Governance requires `Texto para copiar e enviar`, place it immediately
after `Conversa recomendada`, without intervening content, and fill the whole
payload without placeholders. The absence form remains inline only in cases
permitted by that authority.

### Mandatory presentation of copy-ready text

When a payload exists, put the `Texto para copiar e enviar:` label on its own
line and immediately below present the complete content inside a fenced
Markdown block identified as `text`. The owner copies only the content between
the opening and closing fences. The label, opening line, closing line and any
guidance outside the block are not part of the message.

Use this form even for a one-line payload and the canonical Human Gate phrase:

````markdown
Texto para copiar e enviar:

```text
Confirmo a decisão acima exclusivamente para STATE-01
```
````

Do not put explanations, hand-off continuation or non-copyable content inside
the block. If the payload contains a backtick or tilde sequence used as a
Markdown fence, choose another character or a longer sequence than any
equivalent inner sequence for the outer fence. Code blocks within the message
then remain copyable without closing the outer block early.

When no useful payload exists, retain only the inline form
`Texto para copiar e enviar: nenhum texto é necessário`; do not create an
empty block or fence the sentinel.

When an attachment or datum that must not be reproduced is required, the block
contains the complete accompanying text but does not embed binary content or a
secret. Additional parallel-front messages follow their own plan and do not
replace the main text.

### Vocabulary and status

Use the concepts and `concluída`, `parcial` and `bloqueada` statuses exactly as
defined in Governance. The fields above materialise that separation; they do
not turn a batch, task, activity or step into competing hand-off labels.

## State-history entry

- Date and title:
- Previous state:
- Requested state:
- Authority:
- Decision:
- Scope:
- Negative scope:
- Preconditions:
- Changes:
- Checks/evidence:
- Limitations/risks:
- Quality Gate:
- Human Gate:
- Resulting state:
- Next condition:
- Approver:

## State hand-off

- Closed state:
- Recommended state:
- Delivered objective and scope:
- Changed files/artefacts:
- Requirements and decisions:
- Checks and results:
- Interfaces/schemas/providers:
- Security and data:
- Risks and debt:
- Rollback:
- Next-phase preconditions:
- Automatic audit:
- Human Gate:
- Steps and execution location:
- Expected result:
- Restrictions:
- Evidence/response the owner must return:
- Closing hand-off: use the compact block above exactly once in the final
  response.

## Conversation routing

Use this worksheet to prepare routing when useful continuity exists. Fill the
applicable fields, remove placeholders and consolidate the result in the
compact lines of the final closing hand-off.

- Reference state/gate:
- Objective/batch:
- Route: `CONTINUE_CURRENT` / `START_NEW` / `RETURN_TO_EXISTING`
- Target:
  - `current`; ou
  - `new`; ou
  - `existing — <título-ou-label-confirmado>`.
- Suggested non-canonical title, only for `START_NEW`:
- Reason:
- Recommended Codex reasoning: `Leve` / `Médio` / `Alto` /
  `Extra alto` / `Máximo` / `Ultra`
- Reasoning justification:
- Alternative if unavailable:
- Owner navigation instruction:
- Complete copy-ready text:

### Text for `CONTINUE_CURRENT`

```text
Continue nesta conversa o trabalho do RAG-Challenge no estado/gate <STATE/GATE>,
objetivo/lote <OBJETIVO/LOTE>.
Use integralmente todas as regras, controles e capacidades aplicáveis
incorporadas de Stage 0, Stage 1 e Stage 2. Não crie agentes sem necessidade,
não paralelize atividades SEQUENTIAL_ONLY, não interprete prontidão
multiagente como autoridade contínua e não amplie escopo, lifecycle, Human
Gate, provider ou autoridade externa.
Comunique-se comigo em pt-BR. Produza novos artefatos técnicos permanentes em
en-GB e preserve localização funcional, fontes, citações, literais canônicos,
histórico Git, evidências históricas e registros append-only.
Antes de agir, releia AGENTS.md, prompts/Start-Here.md,
prompts/state/Current-State.md e <DOCUMENTOS-TEMÁTICOS>.

Raciocínio do Codex recomendado: <NÍVEL>.
Justificativa do raciocínio: <JUSTIFICATIVA>.
Alternativa se indisponível: <ALTERNATIVA>.
Objetivo autorizado: <OBJETIVO>.
Autoridade já registrada: <AUTORIDADE>.
Escopo permitido: <ESCOPO-POSITIVO>.
Escopo negativo: <ESCOPO-NEGATIVO>.
Verificações e resultado esperado: <CHECKS-E-RESULTADO>.

Confirme primeiro que o estado factual continua compatível. Se houver
divergência, mudança externa ou falta de autoridade, pare e informe.
```

### Text for `START_NEW`

Título proposto: `RAG-Challenge — <STATE-OU-GATE> — <OBJETIVO-CURTO>`.

```text
Projeto: RAG-Challenge.
Diretório do projeto: <rag-challenge-root>.
Estado/gate de referência: <STATE/GATE>.
Objetivo/lote pretendido: <OBJETIVO/LOTE>.
Identificação da conversa: RAG-Challenge — <STATE-OU-GATE> — <OBJETIVO-CURTO>.

Use integralmente todas as regras, controles e capacidades aplicáveis
incorporadas de Stage 0, Stage 1 e Stage 2. Não crie agentes sem necessidade,
não paralelize atividades SEQUENTIAL_ONLY, não interprete prontidão
multiagente como autoridade contínua e não amplie escopo, lifecycle, Human
Gate, provider ou autoridade externa.
Comunique-se comigo em pt-BR. Produza novos artefatos técnicos permanentes em
en-GB e preserve localização funcional, fontes, citações, literais canônicos,
histórico Git, evidências históricas e registros append-only.
Antes de agir, leia integralmente AGENTS.md, prompts/Start-Here.md,
prompts/state/Current-State.md, prompts/state/State-Transition-Log.md e
<DOCUMENTOS-TEMÁTICOS>. A documentação vigente prevalece sobre qualquer
resumo desta mensagem.

Raciocínio do Codex recomendado: <NÍVEL>.
Justificativa do raciocínio: <JUSTIFICATIVA>.
Alternativa se indisponível: <ALTERNATIVA>.
Objetivo autorizado: <OBJETIVO>.
Autoridade já registrada: <AUTORIDADE>.
Escopo permitido: <ESCOPO-POSITIVO>.
Escopo negativo: <ESCOPO-NEGATIVO>.
Verificações e resultado esperado: <CHECKS-E-RESULTADO>.

Confirme o estado e a autoridade antes de alterar arquivos ou sistemas. Não
avance estado, aceite ADR, execute ação externa ou amplie escopo por
inferência. Se houver divergência ou bloqueio, pare e informe.
```

### Text for `RETURN_TO_EXISTING`

Use only a title or identification supplied or confirmed by the owner.

```text
Retome nesta conversa identificada como <TÍTULO-OU-LABEL-CONFIRMADO> o
trabalho do RAG-Challenge no estado/gate <STATE/GATE>, objetivo/lote
<OBJETIVO/LOTE>.

Use integralmente todas as regras, controles e capacidades aplicáveis
incorporadas de Stage 0, Stage 1 e Stage 2. Não crie agentes sem necessidade,
não paralelize atividades SEQUENTIAL_ONLY, não interprete prontidão
multiagente como autoridade contínua e não amplie escopo, lifecycle, Human
Gate, provider ou autoridade externa.
Comunique-se comigo em pt-BR. Produza novos artefatos técnicos permanentes em
en-GB e preserve localização funcional, fontes, citações, literais canônicos,
histórico Git, evidências históricas e registros append-only.
Antes de agir, releia AGENTS.md, prompts/Start-Here.md,
prompts/state/Current-State.md, prompts/state/State-Transition-Log.md e
<DOCUMENTOS-TEMÁTICOS>. Reconcilie qualquer contexto antigo com o estado
factual vigente; a documentação atual prevalece.

Raciocínio do Codex recomendado: <NÍVEL>.
Justificativa do raciocínio: <JUSTIFICATIVA>.
Alternativa se indisponível: <ALTERNATIVA>.
Objetivo autorizado: <OBJETIVO>.
Autoridade já registrada: <AUTORIDADE>.
Escopo permitido: <ESCOPO-POSITIVO>.
Escopo negativo: <ESCOPO-NEGATIVO>.
Verificações e resultado esperado: <CHECKS-E-RESULTADO>.

Se a conversa estiver indisponível, não corresponder à identificação ou
conflitar com o estado atual, pare e recomende uma nova conversa sem inventar
referência.
```

The texts above carry only existing authority or a request that the owner has
explicitly decided to make. They do not replace a Human Gate, ADR, state
authority or external authority. Their Stage instruction invokes only
applicable incorporated controls and never creates continuous authority.

### Special Human Gate rule

- Request the confirmation phrase only with `CONTINUE_CURRENT`, target
  `current`, and a complete current-baseline summary in the same hand-off.
- In that case, `Texto para copiar e enviar` contains only the canonical gate
  phrase, presented in the copy-ready block even though it is one line. The
  Stage instruction is intentionally absent because no other content is
  permitted in this payload.
- For `START_NEW` or `RETURN_TO_EXISTING`, the message requests rereading and
  republication of the complete summary in the target conversation. It does
  not include the confirmation phrase; that phrase appears only in a later
  hand-off in the same conversation.

## Parallel-conversation plan

When Governance classifies work as `PARALLEL_OPTIONAL` or
`PARALLEL_RECOMMENDED`, complete the section below. The classification grants
no additional authority.

- Parallel-work classification: `SEQUENTIAL_ONLY` /
  `PARALLEL_OPTIONAL` /
  `PARALLEL_RECOMMENDED`
- Reason:
- Coordinating conversation and confirmed title/identification:
- Recommended reasoning for the coordinating conversation:
- Coordinator justification and alternative:
- Base snapshot and corpus version:
- Reference state/gate:
- Common objective/batch:
- Common authority:
- Conditions that must hold before opening auxiliary conversations:
- Ownership of paths, logical artefacts, mutable resources and
  canonical/shared files:
- Git/worktree/runtime/data isolation:
- Integration order:
- Cross-cutting checks after integration:
- Sequential fallback:

For `SEQUENTIAL_ONLY`, use only the compact line
`Paralelismo: SEQUENTIAL_ONLY — <MOTIVO>` in the closing hand-off. Do not add a
non-existent plan or messages.

For `PARALLEL_OPTIONAL` or `PARALLEL_RECOMMENDED`, fill one row per
conversation. No writable path or artefact may appear in two active rows.

| Front | Route, target and identification | Reasoning, justification and alternative | Objective | Preconditions/dependencies | Exclusive writing or read-only | Read-only inputs and prohibitions | Checks/result | Stop condition | Integration order |
|---|---|---|---|---|---|---|---|---|---|
| `<LANE-ID>` | `<ROUTE>`; `<TARGET>`; `<LABEL>` | `<NÍVEL>`; `<JUSTIFICATIVA>`; `<ALTERNATIVA>` | `<OBJETIVO>` | `<DEPENDÊNCIAS>` | `<OWNERSHIP>` | `<LIMITES>` | `<RESULTADO>` | `<STOP>` | `<ORDEM>` |

Provide one complete block per front in the exact parallel messages. If the
coordinator lacks a confirmed title or identification, establish that
reference before opening auxiliary conversations.

### Text for a parallel auxiliary conversation

Proposed title when the route is `START_NEW`:
`RAG-Challenge — <STATE-OU-GATE> — <LANE-OBJETIVO>`.
For `RETURN_TO_EXISTING`, copy exactly the title or identification confirmed by
the owner; never replace it with the proposed form.

```text
Projeto: RAG-Challenge.
Diretório do projeto: <rag-challenge-root>.
Papel: responsável pela frente <LANE-ID>; não é a conversa coordenadora.
Identificação da conversa: <LANE-LABEL-SUGERIDO-OU-CONFIRMADO>.
Conversa coordenadora confirmada: <TÍTULO-OU-LABEL-DA-COORDENADORA>.
Linha de base e versão do corpus: <BASELINE-E-VERSÃO>.
Estado/gate de referência: <STATE/GATE>.
Objetivo/lote comum: <OBJETIVO/LOTE>.
Raciocínio do Codex recomendado: <NÍVEL>.
Justificativa do raciocínio: <JUSTIFICATIVA>.
Alternativa se indisponível: <ALTERNATIVA>.

Use integralmente todas as regras, controles e capacidades aplicáveis
incorporadas de Stage 0, Stage 1 e Stage 2. Não crie agentes sem necessidade,
não paralelize atividades SEQUENTIAL_ONLY, não interprete prontidão
multiagente como autoridade contínua e não amplie escopo, lifecycle, Human
Gate, provider ou autoridade externa.
Comunique-se comigo em pt-BR. Produza novos artefatos técnicos permanentes em
en-GB e preserve localização funcional, fontes, citações, literais canônicos,
histórico Git, evidências históricas e registros append-only.
Antes de agir, leia integralmente AGENTS.md, prompts/Start-Here.md,
prompts/state/Current-State.md e <DOCUMENTOS-TEMÁTICOS>. Confirme que a
baseline e a autoridade continuam vigentes.

Autoridade já registrada: <AUTORIDADE>.
Objetivo exclusivo da lane: <OBJETIVO>.
Pré-condições e dependências congeladas: <DEPENDÊNCIAS>.
Escrita exclusiva permitida: <PATHS-OU-READ-ONLY>.
Entradas somente leitura: <INPUTS>.
Arquivos e ações proibidos: <ESCOPO-NEGATIVO>.
Verificações e resultado esperado: <CHECKS-E-ENTREGA>.

Pare sem editar fora do ownership se detectar baseline alterada, mudança
concorrente, dependência ausente, colisão de runtime/dados, conflito ou nova
autoridade necessária. Não integre outras lanes, não atualize estado ou
histórico, não decida ADR/Human Gate e não execute ação externa não
autorizada.

No handoff, informe arquivos/artefatos, checks, limitações e riscos. Produza
também um bloco exato para eu copiar na conversa coordenadora confirmada,
que fará a integração na ordem <ORDEM>.
```

Fill ownership, isolation and return under Governance. The return block
contains real results and never claims integration, transition or a gate
completed by an auxiliary conversation.

## Execution report

- State/batch:
- Version and commit:
- Environment, date and executor:
- Authority and scope:
- Negative scope:
- Preconditions:
- Sanitised configuration:
- Commands/tests and results:
- Changes:
- Failures and corrections:
- Untested items:
- Residual risks:
- Rollback/cleanup:
- Gate decision:
- Evidence:

## Automatic audit

- State and scope:
- Baseline:
- Expected deliverables:
- Checks performed:
- Result by gate: `APROVADO` / `REPROVADO` / `BLOQUEADO` /
  `NÃO APLICÁVEL`
- Findings by severity:
- Evidence:
- Environment limitations:
- Recommendation:

## Human Gate

- State:
- Validator and date:
- Automatic report reviewed:
- Critical samples repeated:
- Samples not repeated and reason:
- Experience:
- Security/authority:
- Pending coverage:
- Accepted reservations:
- Decision: `PENDENTE` / `APROVADO` / `APROVADO COM RESSALVAS` /
  `REPROVADO`
- Justification and evidence:
- Unambiguous confirmation:
  `Confirmo a decisão acima exclusivamente para <STATE-ID>`

Only the unambiguous phrase above, bound to the complete summary, satisfies the
confirmation. A word, abbreviated confirmation or authority to continue does
not constitute a Human Gate. Each state requires a separate decision.

## ADR

- ID and title:
- Status: `proposed` / `accepted` / `superseded` / `rejected`
- Date:
- Owners:
- Context:
- Decision:
- Alternatives:
- Consequences:
- Security and operations:
- Compatibility and migration:
- Acceptance checks:

## Corpus change

- Corpus ID:
- Source scope:
- Actor and authority:
- `sourceAdapterId` e `SourceTrustClass`:
- Licence/provenance:
- Logical document:
- Previous version:
- New version and SHA-256:
- Content object ID and storage:
- Parser and chunking:
- Document set digest:
- Source binding set digest generation-bound:
- Activation binding set digest with observation:
- Catalogue revision and observation-journal revision:
- Manifest schema version and generation spec digest:
- Candidate build ID:
- Chunk/vector counts and logical artefact digest:
- Generation content digest and final IndexGenerationId:
- Index compatibility key:
- Candidate generation:
- Provider/model/dimension:
- Vector store/schema:
- Validations and smoke queries:
- Previous generation preserved:
- Expected/new activation record:
- Bound official snapshot/observation:
- Preserved/changed fields in observation rebinding:
- Transactional activation and audit:
- Rollback target and newly built revision:
- Failures/limitations:
- Evidence:

## RAG evaluation report

- Corpus and version:
- Source scope and snapshot/freshness:
- Index generation:
- Dataset/rubric:
- Providers, models and parameters:
- Prompt/version:
- Environment and date:
- Thresholds defined before execution:
- Retrieval:
- Groundedness:
- Citations:
- Cases without evidence:
- Security/prompt injection:
- Isolation/fallback between scopes:
- Latency and cost:
- Failures:
- Untested items:
- Result:
- Risks and recommendation:

## MVP official-source record

- Source ID:
- Source adapter ID:
- Source trust class:
- Owner:
- Official domain:
- Canonical URL:
- Allowlisted scheme/port/path:
- Canonicalised URI and redirects disabled:
- Authorised DNS A/AAAA and connected IP:
- Host/SNI preservados:
- Anonymous public source and credential-free URL/query:
- TLS/revocation policy and absence of auxiliary egress:
- Purpose:
- Licence/terms/robots:
- Scope and version:
- Snapshot ID:
- Immutable content snapshot:
- `retrievedAt`:
- Validators sent, HTTP status and observed ETag/Last-Modified:
- SHA-256:
- Media type and size:
- Revalidation observation ID:
- `revalidatedAt`:
- `maxAge`:
- Egress policy:
- Execution authority:
- Frequency and rate limit:
- Sanitisation:
- State: `Declared` / `Approved` / `Candidate` / `Current` / `Stale` /
  `Withdrawn` / `Rejected` / `Failed` / `Deactivated`
- Evidence:

This template does not authorise network access.
