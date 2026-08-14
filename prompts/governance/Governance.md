# Governança e Execução Controlada

## Autoridade

A precedência e o roteamento estão em
[`../Start-Here.md`](../Start-Here.md). Este documento possui a autoridade
temática sobre estados, transições, execução controlada e memória factual.
Nenhum template, roadmap, ADR proposto ou relatório histórico altera o estado
do projeto.

A comunicação com o proprietário e os artefatos aplicam a autoridade temática
da [`política de idioma`](Language-Policy.md).

## Estados canônicos

1. `STATE-00 DISCOVERY`
2. `STATE-01 PROJECT_SETUP`
3. `STATE-02 ARCHITECTURE`
4. `STATE-03 DATA_AND_INDEX_MODELING`
5. `STATE-04 BACKEND_IMPLEMENTATION`
6. `STATE-05 FRONTEND_IMPLEMENTATION`
7. `STATE-06 INTEGRATION`
8. `STATE-07 TESTING_HOMOLOGATION`
9. `STATE-08 PRODUCTION_RELEASE`

O fluxo normal é sequencial. Uma transição exige:

1. entregáveis do estado;
2. auditoria automática aplicável;
3. relatório factual;
4. Human Gate explícito para um único estado;
5. entrada append-only;
6. atualização do estado factual.

Auditoria aprovada, ADR aceito ou autorização de um lote não promove o
lifecycle automaticamente.

Entre o encerramento de `STATE-00` e a entrada em `STATE-01`, o
`GATE-B01 ARCHITECTURE_BOOTSTRAP_DECISION` deve:

1. aceitar ou rejeitar explicitamente o ADR-0001;
2. selecionar a licença do repositório;
3. registrar a decomposição física de projetos proporcional ao MVP;
4. mapear módulos conceituais para namespaces/pastas, dependências permitidas
   e testes arquiteturais;
5. escolher se a operação administrativa one-shot usará o host principal ou
   justificará um projeto de ferramenta separado.

Quando aprovado, o ADR-0001 é o registro canônico do mapa físico, do
mapeamento módulo/namespace, das dependências/testes e da forma administrativa.
A decisão humana do gate, a licença selecionada e suas evidências ficam em nova
entrada append-only de
[`../state/State-Transition-Log.md`](../state/State-Transition-Log.md);
[`../state/Current-State.md`](../state/Current-State.md) recebe apenas o
snapshot factual resultante.

Esse gate decide apenas o bootstrap. Ele não aceita o ADR-0002, não seleciona
providers ou corpus e não autoriza Git, scaffold ou entrada em `STATE-01`.
Depois do gate, a entrada em `STATE-01` ainda exige uma autorização humana
separada.

## IDs canônicos de módulos

- `CH-MOD-01 CORPUS_CATALOG`
- `CH-MOD-02 DOCUMENT_INGESTION`
- `CH-MOD-03 INDEXING_RETRIEVAL`
- `CH-MOD-04 ANSWER_GENERATION`
- `CH-MOD-05 QUERY_EXPERIENCE`
- `CH-MOD-06 OPERATIONS_GOVERNANCE`
- `CH-MOD-07 OFFICIAL_EXTERNAL_SOURCES`
- `CH-MOD-08 EXTERNAL_INTEGRATION_CONTRACTS`

IDs não podem ser reutilizados com outro significado. O módulo 07 integra o
MVP por registros governados de fontes oficiais compatíveis, sem crawling ou
URL pública arbitrária. O módulo 08 possui no MVP somente o
contrato HTTP/OpenAPI público do RAG-Challenge; qualquer adapter consumidor,
inclusive o futuro adapter do DB-Notifier, pertence ao repositório consumidor
e a decisões próprias.

A baseline corretiva `3.0.1`, ainda antes do Human Gate, substituiu o rótulo
ambíguo `DB_NOTIFIER_ADAPTER` por `EXTERNAL_INTEGRATION_CONTRACTS`. A
responsabilidade continua sendo a fronteira de integração versionada; o ID não
foi reutilizado para uma capacidade sem relação.

## Protocolo de execução

1. Ler instruções, visão, estado e regras temáticas.
2. Inspecionar workspace, versionamento e mudanças preexistentes.
3. Confirmar estado, autoridade, entregáveis e escopo negativo.
4. Classificar runtime preflight como `NÃO APLICÁVEL` sem inspecionar
   processos em documentação/análise read-only; executá-lo somente quando a
   próxima ação alterar ou validar comportamento executável.
5. Planejar a menor mudança coerente e a validação proporcional.
6. Implementar somente o escopo autorizado.
7. Executar checks reais e preservar evidência sanitizada.
8. Revisar diff, segurança, links e afirmações.
9. Atualizar estado e histórico somente quando o fato ocorreu.
10. Na resposta final da solicitação, entregar uma única vez o handoff
    compacto com solicitação, próximo trabalho recomendado, estado/gate, ação
    do proprietário, roteamento, texto para copiar e enviar, raciocínio do
    Codex e paralelismo seguro.

## Continuidade entre conversas

A documentação do repositório é a fonte de verdade. Conversas são contextos de
trabalho temporários e não substituem `Current-State.md`, o histórico
append-only, ADRs, relatórios ou commits futuros.

Cada solicitação do proprietário recebe exatamente um handoff, somente na
resposta final do turno lógico. Ele informa a continuidade sem misturar
trabalho, lifecycle, ação humana e roteamento, e fornece uma mensagem completa
em `pt-BR` dentro do próprio encerramento quando a continuidade exigir que o
proprietário copie e envie texto.

O limite temático da resposta é o pedido atual e explícito do proprietário.
Pergunta de confirmação, esclarecimento, correção ou follow-up restrito não
reativa por si só o próximo estado do projeto, backlog futuro, melhoria
opcional ou assunto anteriormente discutido. Corpo e handoff respondem ao
mesmo tema; só introduzem trabalho adicional quando ele decorre diretamente
da solicitação atual ou é necessário para concluí-la ou desbloqueá-la. Quando
o proprietário estreitar o tema ou apontar mistura de assuntos, aplicar
imediatamente o recorte mais estreito e não repetir a derivação rejeitada.

Atualizações intermediárias dentro da mesma solicitação não são novos
handoffs. Elas permanecem breves, limitadas a progresso, evidência observada,
premissa não bloqueante ou bloqueio, e cada uma acrescenta informação
materialmente nova. Não repetir, parafrasear ou ecoar uma conclusão já
comunicada, inclusive resultado de worker, salvo para corrigi-la ou explicar
uma consequência alterada. Também não antecipar a sequência completa de
situação, continuidade, mensagem, raciocínio e paralelismo. Se o proprietário
complementar a solicitação antes da resposta final, o novo contexto é
incorporado e continua existindo apenas um encerramento.

### Vocabulário de continuidade

Usar estes termos sem intercâmbio:

- `Solicitação`: combina situação (`concluída`, `parcial` ou `bloqueada`),
  resultado concreto e pendências da solicitação atual; não inclui backlog
  futuro, outro gate ou melhoria opcional;
- `Próximo trabalho recomendado`: uma única entrega concreta formulada como
  ação priorizada e diretamente relacionada à solicitação atual que pode
  ocorrer depois desta resposta, com responsável e condição/autoridade; é a
  resposta canônica à pergunta do proprietário sobre o próximo passo, tarefa,
  atividade ou ação. Não é a ação de navegação do proprietário nem autorização
  ou transição automática. Quando faltar dado, decisão ou autoridade para a
  continuação relacionada, obtê-lo é o próximo trabalho e o handoff informa a
  condição exata. Somente quando nenhuma continuação acionável diretamente
  relacionada existir, usar `nenhum — a solicitação atual não exige trabalho
  adicional`; não buscar item sem relação no lifecycle ou backlog apenas para
  preencher o campo;
- `Estado/gate`: posição atual do lifecycle, próximo estado ou gate nomeado e
  condição de entrada somente quando material para o pedido atual; informar
  `sem mudança` quando não houver transição aplicável ao tema;
- `Sua ação agora`: somente a resposta, decisão, autorização, dado ou
  navegação que o proprietário precisa executar imediatamente para viabilizar
  o próximo trabalho; usar `nenhuma` apenas quando a próxima ação não depender
  do proprietário ou quando realmente não existir continuação acionável;
- `Conversa recomendada`: local sugerido para o próximo trabalho, com rota,
  target e motivo; não descreve a entrega e não concede autoridade;
- `Texto para copiar e enviar`: payload completo que materializa a ação do
  proprietário na conversa recomendada; aparece imediatamente depois dela e
  segue o destaque copiável definido em
  [`../templates/Templates.md`](../templates/Templates.md); só pode ser
  declarado desnecessário quando nenhuma ação imediata depender de mensagem.

`Lote` é uma unidade governada que agrupa trabalho. `Tarefa` é uma subunidade
de plano com entrega verificável. `Atividade` é uma operação interna e
`passo` é um item ordenado de procedimento. `Etapa` não é sinônimo genérico:
usar o estado ou gate canônico. Nenhum desses termos substitui solicitação,
próximo trabalho recomendado, estado/gate ou ação do proprietário.

Aplicar a situação da solicitação:

- `concluída`: pendências `0`; o próximo trabalho, quando existir dentro do
  mesmo limite temático, é recomendação futura e não restante da solicitação;
- `parcial`: pendências listam o que falta na solicitação; o próximo trabalho
  é o primeiro item pendente;
- `bloqueada`: pendências identificam o bloqueio; o próximo trabalho é a
  condição de desbloqueio e `Sua ação agora` informa exatamente o que o
  proprietário precisa fornecer, quando aplicável.

Todo handoff informa exatamente um próximo trabalho recomendado. Concluir a
solicitação atual, poder aguardar ou ainda não possuir autoridade de execução
não elimina uma continuação diretamente relacionada. Antes de usar a ausência
canônica, consultar o estado factual e os documentos proprietários para
identificar a primeira ação útil e governada. Se ela depender de autoridade,
dado, documento, decisão ou anexo do proprietário, o handoff nomeia essa
obtenção como próximo trabalho, preenche `Sua ação agora` e fornece o payload
completo. A regra não autoriza importar estado, gate, backlog ou melhoria sem
relação direta.

Quando um documento proprietário registrar uma ordem de dependência ou uma
sequência nomeada de incrementos e o item atual estiver concluído, o primeiro
item ainda não concluído dessa ordem tem prioridade como próximo trabalho. Se
ele ainda não possuir autoridade, a próxima ação é obter do proprietário a
autoridade delimitada, não revisar genericamente o resultado já concluído.
`Revisar os commits`, `considerar a continuidade`, `avaliar se deseja seguir`
ou formulação equivalente só pode ocupar o campo quando essa revisão ou
decisão for um gate, pré-requisito ou entregável explicitamente nomeado. Ao
perguntar diretamente qual é o próximo passo, tarefa, atividade ou ação, o
proprietário recebe essa ação antes da recapitulação do trabalho encerrado.

O handoff final classifica explicitamente a próxima interação:

- `CONTINUE_CURRENT`: o mesmo estado/lote e objetivo continuam ativos, o
  contexto atual é útil e não há benefício material em reiniciar;
- `START_NEW`: começa outro estado/gate ou objetivo/lote relevante, o assunto é
  independente, a conversa ficou excessivamente longa ou contraditória, uma
  revisão precisa de isolamento, ou não existe referência confiável para uma
  conversa anterior;
- `RETURN_TO_EXISTING`: o trabalho pertence claramente a uma conversa anterior
  ainda aplicável, identificada por título ou label que o proprietário
  forneceu ou confirmou.

O agente recomenda; o proprietário navega manualmente. O agente não afirma que
abriu, renomeou, localizou ou mudou de conversa. Se título, label ou ID não for
conhecido com segurança, não o inventa: recomenda `START_NEW` e propõe um
título descritivo no formato
`RAG-Challenge — <STATE-OU-GATE> — <OBJETIVO-CURTO>`.
Esse título é apenas sugestão, não identificador canônico. A mensagem inicial
da nova conversa o repete como `Identificação da conversa`; quando o
proprietário envia essa mensagem, a identificação torna-se referência
confirmada para handoffs futuros, mesmo que a interface exiba outro título.

Quando a continuidade depender de mensagem, o handoff final fornece um bloco
`Texto para copiar e enviar` pronto para uso. O texto:

1. identifica `RAG-Challenge`, o estado/gate e o lote pretendido;
2. manda reler `AGENTS.md`, `prompts/Start-Here.md`,
   `prompts/state/Current-State.md` e documentos temáticos relevantes;
3. declara objetivo, autoridade já registrada, escopo positivo e negativo;
4. informa checks, resultado esperado e condição de parada;
5. exige confirmação do estado factual antes de agir;
6. não inventa aprovação nem amplia autoridade por transportar contexto de
   outra conversa.

O target é coerente com a ação: `current`, `new` ou
`existing — <título-ou-label-confirmado>`. `START_NEW` acrescenta um título
sugerido e não canônico no próprio campo `Conversa recomendada`;
verificabilidade de conversa existente é exigida somente para
`RETURN_TO_EXISTING`.

Ao retornar a uma conversa antiga, o texto manda reconciliar seu contexto com
o `Current-State.md`; qualquer divergência é resolvida a favor do estado
factual e das autoridades atuais. Ao iniciar conversa nova, todos os
placeholders do template são preenchidos e o handoff propõe um título.

Quando `Sua ação agora` orientar continuar, iniciar, retomar, responder,
confirmar, decidir, autorizar ou enviar algo em uma conversa, `Texto para
copiar e enviar` é obrigatório, aparece imediatamente após `Conversa
recomendada` e contém o payload integral em `pt-BR`. Não interpor outro
rótulo, prosa, título ou recomendação; o título sugerido de `START_NEW`
permanece no próprio campo de conversa. Não adiar o texto para outra resposta,
não apontar para mensagem fornecida anteriormente ou em outra parte da
resposta e não usar sentinela de ausência. Rota, destino, título, ação e
conteúdo devem ser coerentes entre si. O rótulo fica em linha própria e o
payload imediatamente abaixo no bloco cercado visualmente copiável do
template; cercas e orientação externa nunca integram o texto a enviar.

Anexo, arquivo ou dado que não deva ser reproduzido no chat não substitui o
texto: quando seu envio for necessário, o bloco contém a instrução completa
que o acompanha, sem incorporar binário ou secret. Mensagens adicionais de
lanes paralelas aparecem somente na seção condicional própria e nunca
substituem o texto principal do handoff.

Quando nenhuma ação imediata depender de mensagem, o handoff declara
`Texto para copiar e enviar: nenhum texto é necessário`. Se também não existir
ação do proprietário, declara uma única vez `Sua ação agora: nenhuma` e não
cria tarefa, título, plano ou mensagem artificial. A ausência de texto só é
válida quando nenhuma continuidade útil depende de envio. Campos condicionais
ausentes não são substituídos por listas repetitivas de `nenhum`.
`Sua ação agora: nenhuma` é incompatível com qualquer instrução para iniciar,
retomar ou enviar mensagem a outra conversa.

Quando não houver entrega posterior diretamente relacionada, declarar
`Próximo trabalho recomendado: nenhum — a solicitação atual não exige trabalho
adicional`. Essa ausência é preferível a importar o próximo estado geral do
projeto, criar uma decisão para o proprietário ou transformar uma resposta
informativa em autorização implícita. Solicitação concluída, projeto em espera
ou falta de autoridade vigente não bastam para essa ausência quando ainda
existir uma ação concreta diretamente relacionada.

O formato padrão agrupa dados relacionados em linhas compactas: rota, target,
título quando aplicável e motivo em `Conversa recomendada`; nível,
justificativa e fallback em `Raciocínio recomendado`; classificação e motivo
em `Paralelismo`. Plano e mensagens por lane aparecem somente para
`PARALLEL_OPTIONAL` ou `PARALLEL_RECOMMENDED`.

Uma frase de Human Gate só pode ser solicitada em `CONTINUE_CURRENT`, target
`current`, junto do resumo completo e da baseline vigente no mesmo handoff. Se
a recomendação for `START_NEW` ou `RETURN_TO_EXISTING`, a mensagem manda
reemitir e revisar o resumo completo na conversa alvo; a frase de confirmação
não é transportada isoladamente. Autorizações externas e decisões
arquiteturais também continuam sujeitas aos protocolos próprios. Mesmo com
uma única linha, a frase aparece no bloco copiável obrigatório. Roteamento de
conversa não concede essa autoridade.

## Recomendação de raciocínio do Codex por conversa

Todo handoff recomenda um nível para a próxima conversa coordenadora. Cada
conversa auxiliar recebe sua própria recomendação. O nível pertence à
conversa ou à lane, não ao lifecycle inteiro, e deve ser reavaliado quando
objetivo, risco, incerteza, abrangência ou forma de execução mudarem.

Usar o menor nível suficiente para produzir resultado verificável. Os valores
canônicos destinados ao proprietário e suas correspondências usuais, quando a
superfície e o modelo as oferecerem, são:

| Nível canônico | Correspondência usual | Recomendar quando |
|---|---|---|
| `Leve` | `Light` / `low` | Status, roteamento, extração, formatação ou verificação mecânica curta, com escopo inequívoco e baixo risco. |
| `Médio` | `Medium` / `medium` | Trabalho normal, limitado e bem especificado, com poucas decisões locais e validação direta. |
| `Alto` | `High` / `high` | Diagnóstico relevante, mudança multiarquivo, integração ou análise com alternativas, casos-limite e vários checks. |
| `Extra alto` | `Extra High` / `xhigh` | Arquitetura, segurança, contratos ou análise transversal complexa, com ambiguidades materiais e consequências entre áreas. |
| `Máximo` | `Max` / `max` | Problema excepcionalmente difícil e fortemente acoplado, decisão profunda de ADR/gate, migração ou ação de alto impacto em que a profundidade de uma única conversa prevalece sobre tempo e uso. |
| `Ultra` | `Ultra` / `ultra` | Trabalho excepcional, crítico e decomponível em frentes independentes, no qual coordenação proativa e revisão multiagente trazem ganho material. |

Os critérios são cumulativos: risco e irreversibilidade; incerteza e
ambiguidade; abrangência e número de contratos afetados; profundidade exigida;
possibilidade real de decomposição; e custo de verificação. Um gate ou ADR não
recebe `Máximo` automaticamente: a recomendação depende da dificuldade e do
impacto observados. `Máximo` privilegia profundidade em uma tarefa acoplada;
`Ultra` privilegia decomposição e coordenação. `Ultra` só pode ser recomendado
quando o gate de trabalho paralelo permitir `PARALLEL_OPTIONAL` ou
`PARALLEL_RECOMMENDED`; nunca serve para decidir em paralelo um ADR, Human
Gate ou transição.

Toda recomendação registra:

1. `Raciocínio do Codex recomendado`: exatamente um dos seis valores;
2. `Justificativa do raciocínio`: por que esse é o menor nível suficiente;
3. `Alternativa se indisponível`: nível suportado e compensação de validação.

A disponibilidade varia por superfície, conta, modelo e configuração. Os
nomes técnicos da tabela são correspondências informativas, não promessa de
que um seletor ou valor de configuração exista naquele contexto. O agente
recomenda, mas não afirma que alterou a configuração; o proprietário seleciona
o nível quando o controle estiver disponível.

Não substituir silenciosamente um nível indisponível. Usar como orientação:

- `Leve` indisponível: `Médio`;
- `Médio` indisponível: `Alto`, quando disponível;
- `Alto` indisponível: `Médio` com checks adicionais;
- `Extra alto` indisponível: `Alto` com revisão independente;
- `Máximo` indisponível: `Extra alto` com revisão independente;
- `Ultra` indisponível: `Máximo` na coordenadora e decomposição governada
  explícita; se `Máximo` também faltar, `Extra alto` com revisão independente.

O fallback preserva autoridade, escopo negativo, checks e condição de parada.
Nível de raciocínio não escolhe modelo, não autoriza subagente, não altera a
classificação de paralelismo e nunca amplia lifecycle, permissões, sandbox,
rede, consumo externo ou mutações.

## Ações permitidas por estado

| Estado | Permitido | Não permitido sem nova autoridade |
|---|---|---|
| `STATE-00` | Inspeção, inventário, requisitos, riscos, documentação, propostas e validação documental | Scaffold, código, dependências, API, índice, UI, Git init ou deploy |
| `GATE-B01` | Decisão do ADR-0001, licença, mapa físico/módulos e forma administrativa | Git init, scaffold, dependências, código ou aceitação de outros ADRs |
| `STATE-01` | Git/scaffold separadamente autorizados, solution/projects aceitos, configuração, restore, build, lint, testes estruturais e CI | Regras funcionais de ingestão ou consulta |
| `STATE-02` | ADRs, contratos, threat model, provider selection, diagramas e spikes descartáveis autorizados | Produto funcional ou consumo externo não aprovado |
| `STATE-03` | Modelo de catálogo, documento, índice, migrations e rollback não produtivo | Aplicar migration ou modificar armazenamento operacional |
| `STATE-04` | Domain, Application, RAG adapters, persistência, API e testes | Interface completa ou deploy público |
| `STATE-05` | Dashboard, acessibilidade e testes de UI | Integração externa/deploy não autorizado |
| `STATE-06` | Integração local, E2E em sandbox, artefato candidato e configuração de OCI sem publicação | Produção ou anúncio de suporte |
| `STATE-07` | Avaliação RAG, segurança, carga, recuperação, acessibilidade e homologação autorizada | Publicação |
| `STATE-08` | Release, OCI, smoke, observabilidade, evidência e rollback no alvo autorizado | Funcionalidade não registrada |

Secrets, consumo pago, ação remota, publicação e deploy sempre exigem
autoridade específica, independentemente do estado.

## Decisões arquiteturais

- ADRs começam como `proposed`.
- Uma decisão humana explícita pode torná-los `accepted`.
- Substituição usa novo ADR e preserva o anterior como `superseded`.
- Aceitação não autoriza implementação.
- Mudanças de stack, contratos, persistência, fonte online, segurança,
  implantação ou integração ao DB-Notifier exigem ADR.
- O Human Gate de um estado não aceita ADRs por implicação. Cada decisão
  arquitetural identifica o ADR e a decisão solicitada.

## Estado bloqueado

Registrar:

- causa e evidência;
- impacto e escopo;
- tentativas seguras;
- trabalho independente possível;
- dependência ou owner;
- condição objetiva de desbloqueio.

Bloqueio não autoriza saltar estado, enfraquecer gate ou inventar resultado.

## Rollback

- Definir gatilho, owner, versão alvo e validação.
- Separar rollback de aplicação, configuração, catálogo, documento, índice e
  deploy.
- Preservar auditoria e proveniência.
- Preferir geração nova e ativação atômica a mutação no lugar.
- Nunca alterar um banco de dados documentado ou consultado como efeito do
  rollback do RAG-Challenge.
- Usar forward-fix quando rollback aumentar o risco, registrando a decisão.

## Memória do projeto

- [`../state/Current-State.md`](../state/Current-State.md): somente presente
  factual.
- [`../state/State-Transition-Log.md`](../state/State-Transition-Log.md):
  histórico append-only.
- ADRs: decisões e substituições.
- `docs/STATE-*`: evidência de execução e gates.
- [`../system/Prompt-System-Change-Log.md`](../system/Prompt-System-Change-Log.md):
  evolução do corpus.

Não reescrever evidência histórica para parecer atual.

## Trabalho paralelo e multiagente

O handoff classifica o paralelismo separadamente da rota da conversa
coordenadora:

- `SEQUENTIAL_ONLY`: existe dependência entre tarefas, sobreposição de
  ownership, decisão/gate compartilhado, contrato ainda instável, competição
  por runtime/dados ou isolamento insuficiente;
- `PARALLEL_OPTIONAL`: existem frentes independentes e seguras, mas o ganho é
  pequeno ou o custo de coordenação pode ser equivalente;
- `PARALLEL_RECOMMENDED`: duas ou mais frentes independentes, limitadas e
  verificáveis reduzem materialmente o tempo sem aumentar o risco.

Usar o menor número útil de frentes. Uma recomendação paralela define:

1. uma única conversa coordenadora, identificada por título ou label
   confirmado pelo proprietário e responsável por baseline, decisões,
   integração, estado, histórico e gates;
2. identificador, rota/target e label sugerido ou confirmado de cada conversa
   worker;
3. objetivo, autoridade e pré-condições comuns;
4. snapshot-base identificável, com versão do corpus e commit/hash quando
   existir;
5. paths, artefatos lógicos e recursos mutáveis exclusivos ou classificação
   `read-only`;
6. inputs somente leitura, dependências e outputs esperados;
7. arquivos e ações explicitamente proibidos;
8. checks, evidência, condição de parada e mensagem exata de retorno;
9. raciocínio recomendado, justificativa e alternativa para a coordenadora e
   para cada worker;
10. ordem determinística de integração e checks globais finais;
11. fallback para execução sequencial.

São bons candidatos: inventários, pesquisa e auditorias somente leitura;
revisões independentes; documentação em áreas sem sobreposição; e, depois de
contratos congelados e isolamento autorizado, módulos ou testes que não
compartilham arquivos nem estado mutável.

Não paralelizar:

- uma frente que depende do output ainda não integrado de outra;
- alterações concorrentes no mesmo arquivo, contrato, schema, migration,
  lockfile, manifesto, project/solution file, configuração ou pipeline;
- decisões de ADR, transições de lifecycle e Human Gates;
- operações sobre o mesmo branch/worktree, porta, processo, banco, índice,
  corpus mutável, secret ou recurso externo;
- tarefas cujo conflito só seria descoberto depois de uma alteração
  irreversível.

`AGENTS.md`, Start Here, Current State, histórico, changelog, registros de
gate e decisão permanecem sob ownership da conversa coordenadora durante o
lote. Um contrato técnico compartilhado pode pertencer a uma única frente
designada; todas as outras o tratam como input congelado e somente leitura.

Antes de existir repositório Git rastreado, conversas simultâneas podem fazer
somente análise, revisão e auditoria read-only. Escritas no workspace são
sequenciais e pertencem à coordenadora. Depois de Git e do workflow
correspondente serem autorizados, frentes de escrita usam branches e
worktrees separados, ownership disjunto e isolamento de portas, stores,
temporários e outputs de build. Branch diferente no mesmo worktree não
constitui isolamento.

Cada worker:

- relê as autoridades e confirma baseline antes de agir;
- recebe raciocínio recomendado, justificativa e alternativa próprios, sem
  presumir herança da configuração da coordenadora;
- executa somente sua frente e não integra trabalho alheio;
- não amplia autoridade, não recebe secrets e não toma decisão humana;
- para diante de sobreposição, mudança inesperada, dependência ausente,
  baseline stale, colisão de runtime ou nova autoridade necessária;
- retorna arquivos/artefatos, checks, limitações e um bloco pronto para a
  coordenadora.

A coordenadora integra uma frente por vez, reconcilia o resultado com a
baseline vigente, resolve conflitos, repete checks locais necessários e
executa a auditoria transversal final. Somente depois atualiza estado,
histórico, relatório e eventual resumo de Human Gate.

`Complete` em uma worker significa candidato entregue, nunca lote integrado
ou estado concluído. O fallback congela somente a frente afetada, preserva sua
evidência e a retoma sequencialmente a partir da última baseline confirmada;
nunca usa last-write-wins ou reversão automática de trabalho alheio.

### Envelope obrigatório de tarefa

Toda delegação executável ou lane paralela recebe um envelope fechado antes
de começar. A conversa coordenadora é owner do envelope; uma worker não
preenche lacunas concedendo autoridade a si própria. O mínimo obrigatório é:

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

- `authority` identifica a solicitação, requisito, ADR aceito e autorização de
  execução aplicáveis, além do escopo negativo; conversa anterior é contexto,
  não autoridade persistente.
- `baseline` fixa branch, HEAD, estado da árvore, versão do corpus e contratos
  protegidos relevantes.
- `execution_surface` fixa `cwd`, worktree, writable roots, sandbox, approval,
  network, política de ambiente e allowlist efetiva de tools, MCPs e skills;
  configuração omitida ou herdada nunca é presumida segura.
- `allowed_paths` e `forbidden_paths` são conjuntos explícitos; ausência de um
  path em `allowed_paths` não concede escrita implícita.
- `dependencies` distingue inputs já integrados de trabalho ainda pendente.
- `shared_resources` declara ownership, mutabilidade, namespace/lease e forma
  de isolamento de cada recurso.
- `required_tests` diferencia checks focais, gate integrado, checks externos e
  evidência humana; executar zero testes nunca é PASS.
- `stop_conditions` inclui os códigos canônicos abaixo e qualquer limite
  adicional da tarefa.
- `deliverables` exige arquivos/artefatos, diff, comandos, resultados,
  limitações e mensagem de retorno à coordenadora.

Uma tarefa sem envelope completo permanece `NOT_READY`. A coordenadora pode
pedir exploração read-only para fechar os campos, mas não pode delegar escrita
nem reservar recurso mutável até o envelope ser verificável.

Antes da primeira escrita, a coordenadora confirma independentemente o `cwd`
resolvido, a raiz do worktree, branch, HEAD, estado da árvore, writable roots,
sandbox, approval, network, política de ambiente, tools, MCPs, skills e todos
os campos do envelope devolvidos pela worker. Overrides vivos do processo pai
podem substituir defaults de custom agents; qualquer superfície mais ampla que
a allowlist do envelope impede o dispatch. Instrução textual e `sandbox_mode`
limitam comportamento, mas não provam isolamento nem desabilitam papéis
embutidos da ferramenta. Somente os papéis definidos por este projeto são
materializados abaixo; a coordenadora não despacha escrita a um papel embutido
genérico.

### Taxonomia operacional de paralelismo

Esta taxonomia classifica operações; ela não substitui a classificação
owner-facing `SEQUENTIAL_ONLY`, `PARALLEL_OPTIONAL` ou
`PARALLEL_RECOMMENDED` do handoff.

| Classe | Regra |
|---|---|
| `SAFE_PARALLEL` | Análise, inventário, revisão ou teste com inputs somente leitura e outputs/recursos integralmente isolados. |
| `CONTRACT_FROZEN_PARALLEL` | Escrita em lanes disjuntas somente depois de contratos compartilhados possuírem owner, identidade/hash e baseline congelados. |
| `SINGLE_OWNER` | Um único owner escreve o artefato ou recurso; outras lanes podem apenas consumi-lo como input congelado. |
| `SEQUENTIAL_ONLY` | Decisão, mutação ou integração com dependência, autoridade humana, estado compartilhado, irreversibilidade, one-shot ou isolamento insuficiente. |

`SAFE_PARALLEL` deixa de ser seguro diante de output compartilhado, cache
mutável não isolado, porta fixa, ação global de processo ou dirty tree
inesperada. `CONTRACT_FROZEN_PARALLEL` termina no primeiro pedido de mudança do
contrato; a lane para com o código correspondente e o owner replaneja. Uma
operação `SINGLE_OWNER` pode coexistir apenas com trabalho realmente disjunto;
ela nunca permite dois autores alternarem o mesmo arquivo ou store.

São sempre `SEQUENTIAL_ONLY` a aceitação/substituição de ADR, Human Gate,
transição de lifecycle, adjudicação humana, integração de candidates, mudança
de contrato compartilhado, migration ordenada, release, deploy, rollback,
operação destrutiva e qualquer campanha one-shot. A produção paralela de
evidência não paraleliza a decisão nem seu registro.

### Ownership de artefatos

Cada path e artefato lógico de uma lane recebe exatamente uma classe:

| Classe | Semântica e exemplos |
|---|---|
| `READ_ONLY_FOR_WORKERS` | Autoridades e inputs que workers inspecionam sem editar, inclusive `AGENTS.md`, Start Here e decisões fora de sua lane. |
| `SINGLE_OWNER` | OpenAPI, DTO/contrato compartilhado, schema, migration com designer/snapshot, solution/project, lockfile, CI, configuração ou manifesto mutável. |
| `LANE_OWNED` | Implementação/testes/documentação explicitamente atribuídos a uma única lane, branch e worktree. |
| `SHARED_BUT_FROZEN` | Contrato, fixture, corpus ou golden input identificado por versão/hash e somente leitura para todas as lanes durante o lote. |
| `GENERATED` | Build, coverage, package, cache, temp ou output reproduzível, sempre task-owned e nunca fonte de autoridade. |
| `HUMAN_CONTROLLED` | Requisito, mudança de escopo, aceitação de ADR/risco, adjudicação, Human Gate, lifecycle, provider, billing, produção e release. |
| `COORDINATOR_ONLY` | Current State, histórico, changelog, registros de gate, integração e relatório consolidado do lote. |

O owner de um contrato compartilhado não se torna owner de requisito, gate ou
decisão humana. Arquivo gerado não pode ser promovido a evidência sem readback,
identidade e vínculo ao comando/baseline. Uma worker nunca remove output,
branch, worktree ou store que não possua marcador e namespace task-owned.

### Recursos mutáveis e isolamento

Antes de despachar escrita ou validação executável, a coordenadora inventaria
os recursos abaixo e registra o isolamento no envelope:

| Recurso | Regra mínima |
|---|---|
| Worktree e branch | Exclusivos por lane gravável; branch distinta no mesmo worktree é insuficiente. |
| `bin/`, `obj/`, `node_modules/`, `dist/` e caches | Um worktree por execução; cache global mutável exige namespace próprio ou execução sequencial. |
| Coverage, TestResults, artefatos, temporários e golden outputs | Root task-owned único; nenhum default fixo pode ser compartilhado entre execuções. |
| SQLite, PostgreSQL, vector store, corpus e índice | Database/store exclusivo ou lease único; corpus/index congelado é somente leitura. |
| Portas, listeners, processos, browser profiles e containers | Porta/profile/container exclusivo e ownership verificável; precheck isolado não substitui lease. |
| Secrets, credentials, providers e recursos externos | Nunca compartilhados com worker; uso somente sob autoridade específica, menor privilégio e gate próprio. |
| Tools, MCPs, skills, apps, connectors e plugins | Allowlist exata por lane; herança, descoberta ou disponibilidade não concede uso. Superfície externa inesperada bloqueia o dispatch. |

O gate completo `eng/ci.ps1` não executa concorrentemente no mesmo worktree.
Restore/build/test e `npm ci` compartilham outputs mesmo quando o diretório de
coverage é único. O gate final roda sequencialmente sobre a baseline integrada.

Locks são proporcionais ao risco, vinculados a `TASK_ID`, lane, recurso,
owner e instante de aquisição. Lock stale não é roubado automaticamente.
Retomada ou cleanup revalida processo, baseline, path, marcador task-owned e
estado externo; dúvida preserva o recurso e escala. Lock global que elimina
todo paralelismo é proibido quando namespaces disjuntos resolvem o risco.

### Stop conditions canônicas

Toda agente para antes da ação bloqueada, preserva a evidência observada e
retorna um destes códigos sem retry, fallback ou ampliação silenciosa:

| Código | Condição |
|---|---|
| `AMBIGUOUS_AUTHORITY` | Não é possível identificar autoridade ou escopo negativo inequívoco. |
| `CONFLICTING_REQUIREMENTS` | Fontes materiais e aplicáveis exigem resultados incompatíveis. |
| `ARCHITECTURE_CHANGE_REQUIRED` | A tarefa depende de nova stack, boundary ou decisão arquitetural. |
| `PUBLIC_CONTRACT_CHANGE_REQUIRED` | O resultado exige alterar contrato público ou compartilhado congelado. |
| `SCHEMA_CHANGE_REQUIRED` | É necessária mudança de schema sem owner/autoridade próprios. |
| `MIGRATION_REQUIRED` | É necessária migration ou alteração de sequência já atribuída. |
| `DESTRUCTIVE_OPERATION` | A continuação apagaria, sobrescreveria ou tornaria dado/estado dificilmente recuperável. |
| `SECRET_REQUIRED` | A continuação depende de secret não autorizado ou indisponível. |
| `PROVIDER_CHANGE_REQUIRED` | A continuação muda provider, modelo, egress, custo ou superfície externa. |
| `HUMAN_DECISION_REQUIRED` | A continuação depende de requisito, aceitação de ADR/risco, adjudicação ou outra decisão humana que não seja o Human Gate de lifecycle. |
| `HUMAN_GATE_REQUIRED` | A continuação depende especificamente do Human Gate de um único `STATE-ID`, com resumo completo e frase canônica. |
| `UNEXPECTED_DIRTY_TREE` | Branch, HEAD, diff ou untracked mudou fora do envelope observado. |
| `SHARED_RESOURCE_COLLISION` | Outro owner/processo usa o mesmo recurso mutável ou seu isolamento não pode ser provado. |
| `OUT_OF_SCOPE_CHANGE_REQUIRED` | O aceite exige arquivo, comportamento ou autoridade fora da tarefa. |
| `TEST_BASELINE_BROKEN` | O baseline ou gate obrigatório falha antes de poder atribuir a falha à lane. |

Mudança material de baseline exige novo envelope ou revalidação explícita pela
coordenadora. `HUMAN_DECISION_REQUIRED` não converte uma decisão do proprietário
em Human Gate. `HUMAN_GATE_REQUIRED` não significa que uma worker possa pedir
ou registrar a frase de gate. O retorno identifica fato, impacto, trabalho
seguro independente, owner e condição objetiva de desbloqueio.

### Papéis especializados

A configuração project-scoped em `.codex/agents/` define estes papéis do
projeto sem desabilitar papéis embutidos da ferramenta nem lhes conceder
autoridade adicional:

- `governance_guard`: read-only; reconstrói autoridade, lifecycle, ADRs,
  gates e stop conditions;
- `code_mapper`: read-only; mapeia dependências, ownership, testes e recursos;
- `architect`: read-only; identifica boundaries, contratos e necessidade de
  ADR, sem aceitar a própria proposta;
- `implementation_worker`: `workspace-write` somente na lane isolada e no
  envelope recebido;
- `independent_reviewer`: read-only e independente da implementação julgada;
- `security_reviewer`: read-only; revisa secrets, trust boundaries, inputs,
  filesystem, subprocess, provider, logging e supply chain.

Nenhum papel aceita requisito, risco, ADR, adjudicação, Human Gate, lifecycle,
provider, billing, produção ou release. O sandbox limita ferramentas; não
substitui ownership, escopo ou autoridade.

## Guard rails

- Não inventar evidência, runtime, licença, modelo, preço ou aprovação.
- Não misturar conteúdo recuperado com instruções confiáveis.
- Não apresentar dado stale, índice incompatível ou fonte indisponível como
  saudável.
- Não anunciar provider ou formato antes de implementação e homologação.
- Não iniciar Git, instalar dependência, acessar modelo ou publicar por
  inferência.
- Não acessar fonte oficial apenas porque ela integra o MVP; cada execução de
  rede exige estado, configuração e autoridade próprios.
- Não confundir funcionamento local, CI, deploy e release.
- Não criar dependência direta do DB-Notifier.
