# Templates do RAG-Challenge

Templates não representam execução, evidência ou aprovação até serem
preenchidos com resultados reais e revisados no estado proprietário.

Toda comunicação e todo artefato seguem a
[`política de idioma`](../governance/Language-Policy.md). Estes templates
aplicam essa autoridade sem reproduzi-la.

## Encerramento único de cada solicitação

Aplicar este formato quando e somente quando exigido pela semântica de
[`Governance.md`](../governance/Governance.md) e pelo enforcement de
[`../../AGENTS.md`](../../AGENTS.md). Este documento materializa apresentação
e formulários; não redefine continuidade, runtime preflight, autoridade ou
gate.

Usar o formato compacto abaixo em `pt-BR`. Não repetir nele explicações já
claras no corpo da resposta. Literais técnicos preservam a grafia canônica.

- Solicitação: `concluída` / `parcial` / `bloqueada` — resultado concreto;
  pendências com nomes e quantidade exatos, ou `0`
- Próximo trabalho recomendado: exatamente uma ação concreta, priorizada e
  diretamente relacionada à solicitação atual; responsável; condição ou
  autoridade necessária; ou, somente sem continuação acionável,
  `nenhum — a solicitação atual não exige trabalho adicional`
- Estado/gate: posição atual; próximo estado ou gate; condição de entrada, ou
  `sem mudança`
- Sua ação agora: ação imediata exata, ou `nenhuma`
- Conversa recomendada: `<ROUTE> — <TARGET> — <MOTIVO>`; acrescentar
  `Título sugerido: <TÍTULO>` no mesmo campo somente para `START_NEW`
- Encaminhamento automático: `<STATUS> — <EVIDÊNCIA-OU-CONDIÇÃO>`
- Texto para copiar e enviar: rótulo em linha própria seguido pelo bloco
  copiável completo em `pt-BR`, ou `nenhum texto é necessário` na mesma linha
- Raciocínio recomendado: `<NÍVEL> — <JUSTIFICATIVA>. Alternativa:
  <FALLBACK>`
- Paralelismo: `<CLASSIFICAÇÃO> — <MOTIVO>`

Todos os campos permanecem no limite temático da solicitação atual. Não usar o
handoff para reintroduzir o próximo estado geral, backlog, melhoria opcional ou
assunto anterior quando eles não forem necessários para responder, concluir
ou desbloquear o pedido presente.

O campo `Próximo trabalho recomendado` responde sempre à pergunta do
proprietário sobre o próximo passo, tarefa, atividade ou ação. Informar uma
única ação mesmo quando a solicitação atual estiver concluída ou a execução
depender de nova autoridade. Nesse caso, obter a autoridade, decisão, dado,
documento ou anexo é a ação, e `Sua ação agora` mais o texto copiável tornam a
condição executável. Usar a ausência canônica somente depois de verificar que
não existe continuação diretamente relacionada; não confundir falta de
autoridade com falta de próxima ação.

Se houver ordem de dependência ou sequência nomeada, preencher o campo com o
primeiro item ainda não concluído ou com a obtenção de sua autoridade exata.
Não usar `revisar commits`, `considerar próximos passos`, `decidir se deseja
continuar` ou equivalentes como substituto, salvo quando essa revisão ou
decisão for um gate, pré-requisito ou entregável formal. Em resposta a uma
pergunta direta sobre o próximo passo, apresentar primeiro a ação concreta e
depois, se necessário, o resumo.

### Valores aceitos

- `<ROUTE>`: `CONTINUE_CURRENT`, `START_NEW` ou `RETURN_TO_EXISTING`;
- `<TARGET>`: `current`, `new` ou
  `existing — <título-ou-label-confirmado>`;
- `<STATUS>`: `EXECUTADO`, `AGUARDANDO_DECISÃO_HUMANA`, `INDISPONÍVEL` ou
  `NÃO_APLICÁVEL`;
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
campos separados de plano ou mensagens com valores artificiais. Aplicar as
regras de coerência entre ação, rota, campos condicionais e ausência definidas
em Governance.

Posicionar `Encaminhamento automático` imediatamente após
`Conversa recomendada`. Quando Governance exigir `Texto para copiar e enviar`,
posicioná-lo imediatamente depois do status, sem conteúdo interposto, e
preencher todo o payload sem placeholders. A forma de ausência permanece
inline somente nos casos permitidos por essa autoridade.

### Destaque obrigatório do texto copiável

Quando uma decisão humana ou fallback manual exigir payload, colocar o rótulo
`Texto para copiar e enviar:` em uma linha própria e, imediatamente abaixo,
apresentar o conteúdo integral dentro de um bloco cercado Markdown com
identificador `text`. O proprietário copia somente o conteúdo entre a cerca de
abertura e a cerca de fechamento. O rótulo, a linha de abertura, a linha de
fechamento e qualquer orientação fora do bloco não integram a mensagem.

Usar este formato inclusive para payload de uma única linha e para a frase
canônica de Human Gate:

````markdown
Texto para copiar e enviar:

```text
Confirmo a decisão acima exclusivamente para STATE-01
```
````

Não inserir explicação, continuação do handoff ou conteúdo não copiável dentro
do bloco. Se o payload contiver uma sequência de crases ou tils usada como
cerca Markdown, escolher para a cerca externa outro caractere ou uma sequência
mais longa do que qualquer sequência equivalente interna. Assim, blocos de
código que façam parte da mensagem permanecem copiáveis sem encerrar o bloco
externo antes da hora.

Quando a ferramenta já tiver entregue o payload ou não existir payload útil,
manter somente a forma inline
`Texto para copiar e enviar: nenhum texto é necessário`; não duplicar o texto
enviado, criar bloco vazio nem cercar a sentinela. O status imediatamente
anterior distingue entrega comprovada de ausência de continuidade.

Quando for necessário anexar arquivo ou fornecer dado que não deva ser
reproduzido, o bloco contém o texto acompanhante completo, mas não incorpora
binário nem secret. Mensagens adicionais de frentes paralelas ficam após o
plano próprio e não substituem o texto principal.

### Vocabulário e situação

Usar os conceitos e as situações `concluída`, `parcial` e `bloqueada`
exatamente como definidos em Governance. Os campos acima materializam essa
separação; não transformam lote, tarefa, atividade ou passo em rótulos
concorrentes do handoff.

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
- Autoridade de automação vigente:
- Operação nativa pretendida:
- Status observado: `EXECUTADO` / `AGUARDANDO_DECISÃO_HUMANA` /
  `INDISPONÍVEL` / `NÃO_APLICÁVEL`
- Identificador, título e host realmente retornados, quando aplicável:
- Evidência ou condição de parada:
- Raciocínio do Codex recomendado: `Leve` / `Médio` / `Alto` /
  `Extra alto` / `Máximo` / `Ultra`
- Justificativa do raciocínio:
- Alternativa se indisponível:
- Raciocínio efetivamente aplicado ou fallback manual:
- Texto completo para copiar e enviar:

Os três textos abaixo são entradas integrais para a operação nativa de envio.
Exibi-los no handoff somente quando uma decisão humana ou o fallback manual
exigir cópia; após entrega automática comprovada, registrar apenas o recibo e
a forma inline de ausência. Quando `CONTINUE_CURRENT` for absorvido pela mesma
tarefa dentro do turno lógico, não emitir handoff intermediário nem recibo de
operação separada; outro target exige `RETURN_TO_EXISTING`.

### Texto para `CONTINUE_CURRENT`

```text
Continue nesta conversa o trabalho do RAG-Challenge no estado/gate <STATE/GATE>,
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

Título proposto: `RAG-Challenge — <STATE-OU-GATE> — <OBJETIVO-CURTO>`.

```text
Projeto: RAG-Challenge.
Diretório do projeto: <rag-challenge-root>.
Estado/gate de referência: <STATE/GATE>.
Objetivo/lote pretendido: <OBJETIVO/LOTE>.
Identificação da conversa: RAG-Challenge — <STATE-OU-GATE> — <OBJETIVO-CURTO>.

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
trabalho do RAG-Challenge no estado/gate <STATE/GATE>, objetivo/lote
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
Gate, ADR, autorização de estado ou autorização externa. Nenhum texto que
declare em primeira pessoa uma decisão, autorização ou aceitação exclusiva do
proprietário pode ser encaminhado automaticamente, mesmo depois da
manifestação humana. Human Gate permanece na conversa atual. Outra decisão
válida destinada a trabalho downstream usa um registro factual atribuído ao
proprietário, com origem, autoridade e escopo exato, nunca uma voz simulada.

### Regra especial para Human Gate

- Solicitar a frase de confirmação somente com `CONTINUE_CURRENT`, destino
  `current`, e resumo completo da baseline vigente no mesmo handoff.
- Registrar `Encaminhamento automático: AGUARDANDO_DECISÃO_HUMANA`; o Codex
  nunca origina nem envia a frase como se fosse o proprietário.
- Nesse caso, `Texto para copiar e enviar` contém apenas a frase canônica do
  gate, destacada no bloco copiável mesmo sendo uma única linha.
- Para `START_NEW` ou `RETURN_TO_EXISTING`, a mensagem solicita releitura e
  republicação do resumo completo na conversa alvo. Não inclui a frase de
  confirmação; ela só será apresentada em um handoff posterior naquela mesma
  conversa.

## Plano de conversas paralelas

Quando Governance classificar o trabalho como `PARALLEL_OPTIONAL` ou
`PARALLEL_RECOMMENDED`, preencher a seção abaixo. A classificação não concede
autoridade adicional.

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
`RAG-Challenge — <STATE-OU-GATE> — <LANE-OBJETIVO>`.
Para `RETURN_TO_EXISTING`, copiar exatamente o título ou a identificação
confirmada pelo proprietário; nunca substituí-lo pelo formato proposto.

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

Preencher ownership, isolamento e retorno conforme Governance. O bloco de
retorno contém resultados reais e nunca declara integração, transição ou gate
concluído por uma conversa auxiliar.

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
- Source binding set digest generation-bound:
- Activation binding set digest com observação:
- Catalogue revision e observation-journal revision:
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
- Campos preservados/alterados em rebinding de observação:
- Ativação transacional e auditoria:
- Rollback target e nova revisão construída:
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
