# Templates do Challenge

Templates não representam execução, evidência ou aprovação até serem
preenchidos com resultados reais e revisados no estado proprietário.

Toda comunicação e todo artefato seguem a
[`política de idioma`](../governance/Language-Policy.md). Estes templates
aplicam essa autoridade sem reproduzi-la.

## Encerramento único de cada solicitação

Emitir este bloco exatamente uma vez, somente no final da resposta que
conclui, pausa ou bloqueia a solicitação do proprietário. Não o usar em
atualizações intermediárias da mesma solicitação. Essas atualizações podem
informar progresso ou evidência de forma breve somente quando acrescentarem
informação materialmente nova. Não repetir nem parafrasear uma conclusão já
comunicada e não antecipar roteamento, texto para copiar, raciocínio ou
paralelismo.

Em documentação e análise somente leitura, runtime preflight é
`NÃO APLICÁVEL`: não anunciar shutdown, enumerar processos ou encerrar nada.

Usar o formato compacto abaixo em `pt-BR`. Não repetir nele explicações já
claras no corpo da resposta. Literais técnicos preservam a grafia canônica.

- Solicitação: `concluída` / `parcial` / `bloqueada` — resultado concreto;
  pendências com nomes e quantidade exatos, ou `0`
- Próximo trabalho recomendado: uma entrega concreta; responsável; condição
  ou autoridade necessária
- Estado/gate: posição atual; próximo estado ou gate; condição de entrada, ou
  `sem mudança`
- Sua ação agora: ação imediata exata, ou `nenhuma`
- Conversa recomendada: `<ROUTE> — <TARGET> — <MOTIVO>`; acrescentar
  `Título sugerido: <TÍTULO>` no mesmo campo somente para `START_NEW`
- Texto para copiar e enviar: bloco completo em `pt-BR`, ou
  `nenhum texto é necessário`
- Raciocínio recomendado: `<NÍVEL> — <JUSTIFICATIVA>. Alternativa:
  <FALLBACK>`
- Paralelismo: `<CLASSIFICAÇÃO> — <MOTIVO>`

### Valores aceitos

- `<ROUTE>`: `CONTINUE_CURRENT`, `START_NEW` ou `RETURN_TO_EXISTING`;
- `<TARGET>`: `current`, `new` ou
  `existing — <título-ou-label-confirmado>`;
- `<NÍVEL>`: `Leve`, `Médio`, `Alto`, `Extra alto`, `Máximo` ou `Ultra`;
- `<CLASSIFICAÇÃO>`: `SEQUENTIAL_ONLY`, `PARALLEL_OPTIONAL` ou
  `PARALLEL_RECOMMENDED`.

### Campos condicionais

Somente para `PARALLEL_OPTIONAL` ou `PARALLEL_RECOMMENDED`, acrescentar depois
da linha de paralelismo:

- Plano paralelo: plano seguro com coordenadora, ownership e ordem de
  integração
- Mensagens para as frentes: um bloco completo por lane

Para `SEQUENTIAL_ONLY`, a linha `Paralelismo` já encerra o tema; não criar
campos separados de plano ou mensagens com valores artificiais. Se não houver
ação do proprietário, não propor título, tarefa ou mensagem apenas para
preencher o bloco. Nunca combinar `Sua ação agora: nenhuma` com orientação
para iniciar, retomar ou enviar mensagem a uma conversa.

`Texto para copiar e enviar` aparece no próprio handoff, imediatamente após
`Conversa recomendada`, sem rótulo, prosa, título ou recomendação interpostos.
Se `Sua ação agora` orientar continuar, iniciar, retomar, responder,
confirmar, decidir, autorizar ou enviar algo em uma conversa, o campo é
obrigatório e contém ali mesmo todo o payload pronto, sem placeholders. Não
adiar o texto, não remeter a mensagem anterior ou a outra parte da resposta e
não usar `nenhum texto é necessário`. Essa sentinela só é válida quando
nenhuma ação imediata do proprietário depende de mensagem.

Quando for necessário anexar arquivo ou fornecer dado que não deva ser
reproduzido, o bloco contém o texto acompanhante completo, mas não incorpora
binário nem secret. Mensagens adicionais de frentes paralelas ficam após o
plano próprio e não substituem o texto principal.

### Vocabulário e situação

Não usar `próximo passo`, `próxima etapa`, `tarefa`, `atividade` e `ação` como
sinônimos no encerramento. `Lote` agrupa trabalho governado, `tarefa` é
subunidade verificável de plano, `atividade` é operação interna e `passo` é
item ordenado de procedimento. `Etapa` deve ser substituída pelo estado/gate
canônico. O próximo trabalho recomendado descreve a entrega; `Estado/gate`
descreve lifecycle; `Sua ação agora` descreve somente o ato imediato do
proprietário.

- `concluída`: pendências `0`; o próximo trabalho é recomendação futura;
- `parcial`: pendências exatas; o próximo trabalho é o primeiro item ainda
  necessário para concluir a solicitação;
- `bloqueada`: pendência e condição de desbloqueio explícitas; `Sua ação
  agora` informa o dado, decisão ou autorização humana necessária.

## Entrada do histórico de estados

- Data e título:
- Estado anterior:
- Estado solicitado:
- Autoridade:
- Decisão:
- Escopo:
- Escopo negativo:
- Pré-condições:
- Mudanças:
- Verificações/evidências:
- Limitações/riscos:
- Quality Gate:
- Human Gate:
- Estado resultante:
- Próxima condição:
- Aprovador:

## Handoff de estado

- Estado encerrado:
- Estado recomendado:
- Objetivo e escopo entregues:
- Arquivos/artefatos alterados:
- Requisitos e decisões:
- Verificações e resultados:
- Interfaces/schemas/providers:
- Segurança e dados:
- Riscos e dívida:
- Rollback:
- Pré-condições da próxima fase:
- Auditoria automática:
- Human Gate:
- Passos e local de execução:
- Resultado esperado:
- Restrições:
- Evidência/resposta que o usuário deve retornar:
- Encerramento: usar uma única vez o bloco compacto acima, na resposta final.

## Roteamento de conversa

Usar esta ficha para preparar o roteamento quando houver continuidade útil.
Preencher os campos aplicáveis, remover placeholders e consolidar o resultado
nas linhas compactas do encerramento final.

- Estado/gate de referência:
- Objetivo/lote:
- Rota: `CONTINUE_CURRENT` / `START_NEW` / `RETURN_TO_EXISTING`
- Alvo:
  - `current`; ou
  - `new`; ou
  - `existing — <título-ou-label-confirmado>`.
- Título sugerido e não canônico, somente para `START_NEW`:
- Motivo:
- Raciocínio do Codex recomendado: `Leve` / `Médio` / `Alto` /
  `Extra alto` / `Máximo` / `Ultra`
- Justificativa do raciocínio:
- Alternativa se indisponível:
- Instrução de navegação ao proprietário:
- Texto completo para copiar e enviar:

### Texto para `CONTINUE_CURRENT`

```text
Continue nesta conversa o trabalho do Challenge no estado/gate <STATE/GATE>,
objetivo/lote <OBJETIVO/LOTE>.
Comunique-se comigo em pt-BR. Produza novos artefatos técnicos permanentes em
en-GB e preserve o idioma dos documentos existentes do STATE-00.
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

### Texto para `START_NEW`

Título proposto: `Challenge — <STATE-OU-GATE> — <OBJETIVO-CURTO>`.

```text
Projeto: Challenge.
Diretório do projeto: <challenge-root>.
Estado/gate de referência: <STATE/GATE>.
Objetivo/lote pretendido: <OBJETIVO/LOTE>.
Identificação da conversa: Challenge — <STATE-OU-GATE> — <OBJETIVO-CURTO>.

Comunique-se comigo em pt-BR. Produza novos artefatos técnicos permanentes em
en-GB e preserve o idioma dos documentos existentes do STATE-00.
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

### Texto para `RETURN_TO_EXISTING`

Usar somente um título ou uma identificação que o proprietário forneceu ou
confirmou.

```text
Retome nesta conversa identificada como <TÍTULO-OU-LABEL-CONFIRMADO> o
trabalho do Challenge no estado/gate <STATE/GATE>, objetivo/lote
<OBJETIVO/LOTE>.

Comunique-se comigo em pt-BR. Produza novos artefatos técnicos permanentes em
en-GB e preserve o idioma dos documentos existentes do STATE-00.
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

Os textos acima transportam somente autoridade já existente ou uma solicitação
que o proprietário decidiu fazer explicitamente. Eles não substituem Human
Gate, ADR, autorização de estado ou autorização externa.

### Regra especial para Human Gate

- Solicitar a frase de confirmação somente com `CONTINUE_CURRENT`, destino
  `current`, e resumo completo da baseline vigente no mesmo handoff.
- Nesse caso, `Texto para copiar e enviar` contém apenas a frase canônica do
  gate.
- Para `START_NEW` ou `RETURN_TO_EXISTING`, a mensagem solicita releitura e
  republicação do resumo completo na conversa alvo. Não inclui a frase de
  confirmação; ela só será apresentada em um handoff posterior naquela mesma
  conversa.

## Plano de conversas paralelas

Avaliar paralelismo em toda solicitação, mas preencher a seção detalhada
abaixo somente para `PARALLEL_OPTIONAL` ou `PARALLEL_RECOMMENDED`.
Paralelismo é recomendação, não autorização adicional.

- Classificação do trabalho paralelo: `SEQUENTIAL_ONLY` /
  `PARALLEL_OPTIONAL` /
  `PARALLEL_RECOMMENDED`
- Motivo:
- Conversa coordenadora e título/identificação confirmada:
- Raciocínio recomendado para a conversa coordenadora:
- Justificativa e alternativa da coordenadora:
- Snapshot-base e versão do corpus:
- Estado/gate de referência:
- Objetivo/lote comum:
- Autoridade comum:
- Condições que precisam ser verdadeiras antes de abrir as conversas
  auxiliares:
- Ownership de paths, artefatos lógicos, recursos mutáveis e arquivos
  canônicos/compartilhados:
- Isolamento de Git/worktree/runtime/dados:
- Ordem de integração:
- Verificações transversais depois da integração:
- Fallback sequencial:

Para `SEQUENTIAL_ONLY`, usar somente a linha compacta
`Paralelismo: SEQUENTIAL_ONLY — <MOTIVO>` no encerramento. Não acrescentar
plano ou mensagens inexistentes.

Para `PARALLEL_OPTIONAL` ou `PARALLEL_RECOMMENDED`, preencher uma linha por
conversa. Nenhum path ou artefato gravável pode aparecer em duas linhas
ativas.

| Frente | Rota, destino e identificação | Raciocínio, justificativa e alternativa | Objetivo | Pré-condições/dependências | Escrita exclusiva ou somente leitura | Entradas somente leitura e proibições | Verificações/resultado | Condição de parada | Ordem de integração |
|---|---|---|---|---|---|---|---|---|---|
| `<LANE-ID>` | `<ROUTE>`; `<TARGET>`; `<LABEL>` | `<NÍVEL>`; `<JUSTIFICATIVA>`; `<ALTERNATIVA>` | `<OBJETIVO>` | `<DEPENDÊNCIAS>` | `<OWNERSHIP>` | `<LIMITES>` | `<RESULTADO>` | `<STOP>` | `<ORDEM>` |

Fornecer nas mensagens paralelas exatas um bloco completo por frente. Se a
coordenadora ainda não possui título ou identificação confirmada, estabelecer
essa referência antes de abrir conversas auxiliares.

### Texto para uma conversa auxiliar paralela

Título proposto, quando a rota for `START_NEW`:
`Challenge — <STATE-OU-GATE> — <LANE-OBJETIVO>`.
Para `RETURN_TO_EXISTING`, copiar exatamente o título ou a identificação
confirmada pelo proprietário; nunca substituí-lo pelo formato proposto.

```text
Projeto: Challenge.
Diretório do projeto: <challenge-root>.
Papel: responsável pela frente <LANE-ID>; não é a conversa coordenadora.
Identificação da conversa: <LANE-LABEL-SUGERIDO-OU-CONFIRMADO>.
Conversa coordenadora confirmada: <TÍTULO-OU-LABEL-DA-COORDENADORA>.
Linha de base e versão do corpus: <BASELINE-E-VERSÃO>.
Estado/gate de referência: <STATE/GATE>.
Objetivo/lote comum: <OBJETIVO/LOTE>.
Raciocínio do Codex recomendado: <NÍVEL>.
Justificativa do raciocínio: <JUSTIFICATIVA>.
Alternativa se indisponível: <ALTERNATIVA>.

Comunique-se comigo em pt-BR. Produza novos artefatos técnicos permanentes em
en-GB e preserve o idioma dos documentos existentes do STATE-00.
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

Antes de Git rastreado e worktrees autorizados, o campo de escrita de toda
conversa auxiliar simultânea deve ser somente leitura. Depois disso, uma frente
de escrita exige branch/worktree próprio, responsabilidade exclusiva e
isolamento de recursos mutáveis. O integrador aplica uma entrega por vez e
repete as verificações globais. Uma conversa auxiliar concluída entrega
somente um candidato pronto para revisão. O bloco de retorno deve conter os
resultados reais; não pode declarar integração, transição ou gate concluído.

## Relatório de execução

- Estado/lote:
- Versão e commit:
- Ambiente, data e executor:
- Autoridade e escopo:
- Escopo negativo:
- Pré-condições:
- Configuração sanitizada:
- Comandos/testes e resultados:
- Mudanças:
- Falhas e correções:
- Itens não testados:
- Riscos residuais:
- Rollback/cleanup:
- Decisão do gate:
- Evidências:

## Auditoria automática

- Estado e escopo:
- Baseline:
- Entregáveis esperados:
- Verificações executadas:
- Resultado por gate: `APROVADO` / `REPROVADO` / `BLOQUEADO` /
  `NÃO APLICÁVEL`
- Achados por severidade:
- Evidências:
- Limitações do ambiente:
- Recomendação:

## Human Gate

- Estado:
- Validador e data:
- Relatório automático revisado:
- Amostras críticas repetidas:
- Amostras não repetidas e motivo:
- Experiência:
- Segurança/autorização:
- Cobertura pendente:
- Ressalvas aceitas:
- Decisão: `PENDENTE` / `APROVADO` / `APROVADO COM RESSALVAS` /
  `REPROVADO`
- Justificativa e evidências:
- Confirmação inequívoca:
  `Confirmo a decisão acima exclusivamente para <STATE-ID>`

Somente a frase inequívoca acima, vinculada ao resumo completo, preenche a
confirmação. Uma palavra, confirmação abreviada ou autorização para continuar
não constitui Human Gate. Cada estado exige decisão separada.

## ADR

- ID e título:
- Status: `proposed` / `accepted` / `superseded` / `rejected`
- Data:
- Owners:
- Context:
- Decision:
- Alternatives:
- Consequences:
- Security and operations:
- Compatibility and migration:
- Acceptance checks:

## Alteração de corpus

- Corpus ID:
- Source scope:
- Ator e autoridade:
- `sourceAdapterId` e `SourceTrustClass`:
- Licença/proveniência:
- Documento lógico:
- Versão anterior:
- Nova versão e SHA-256:
- Content object ID e armazenamento:
- Parser e chunking:
- Document set digest:
- Official snapshot set digest:
- Manifest schema version e generation spec digest:
- Candidate build ID:
- Chunk/vector counts e logical artifact digest:
- Generation content digest e final IndexGenerationId:
- Index compatibility key:
- Geração candidata:
- Provider/model/dimensão:
- Vector store/schema:
- Validações e smoke queries:
- Geração anterior preservada:
- Activation record esperado/novo:
- Snapshot/observação oficial vinculados:
- Ativação transacional e auditoria:
- Rollback target:
- Falhas/limitações:
- Evidência:

## Relatório de avaliação RAG

- Corpus e versão:
- Source scope e snapshot/freshness:
- Geração de índice:
- Dataset/rubrica:
- Providers, modelos e parâmetros:
- Prompt/version:
- Ambiente e data:
- Thresholds definidos antes da execução:
- Recuperação:
- Groundedness:
- Citações:
- Casos sem evidência:
- Segurança/prompt injection:
- Isolamento/fallback entre scopes:
- Latência e custo:
- Falhas:
- Itens não testados:
- Resultado:
- Riscos e recomendação:

## Registro da fonte oficial do MVP

- Source ID:
- Source adapter ID:
- Source trust class:
- Owner:
- Domínio oficial:
- URL canônica:
- Scheme/porta/path allowlisted:
- URI canonicalizada e redirects desativados:
- DNS A/AAAA autorizados e IP conectado:
- Host/SNI preservados:
- Fonte pública anônima e URL/query sem credencial:
- Política TLS/revogação e ausência de egress auxiliar:
- Finalidade:
- Licença/termos/robots:
- Escopo e versão:
- Snapshot ID:
- Snapshot de conteúdo imutável:
- `retrievedAt`:
- Validators enviados, status HTTP e ETag/Last-Modified observados:
- SHA-256:
- Media type e tamanho:
- Revalidation observation ID:
- `revalidatedAt`:
- `maxAge`:
- Política de egress:
- Autoridade da execução:
- Frequência e rate limit:
- Sanitização:
- Estado: `Declared` / `Approved` / `Candidate` / `Current` / `Stale` /
  `Withdrawn` / `Rejected` / `Failed` / `Deactivated`
- Evidência:

Este template não autoriza acesso à rede.
