# Controle e Changelog do Sistema de Instruções

## Versão atual

- Versão: `4.4.0`
- Data: 2026-08-01
- Status: interface e consulta em `pt-BR` e `en-GB` formalizadas por contratos
  independentes; `STATE-02` ativo, com fatos públicos reconciliados e decisões
  de ADR pendentes
- Escopo: 13 arquivos ativos em `prompts/`

A versão do corpus é independente da versão futura do software.

## Política SemVer

- `MAJOR`: mudança incompatível de autoridade, precedência, estados ou
  estrutura.
- `MINOR`: nova capacidade, módulo, playbook ou gate sem quebra do fluxo.
- `PATCH`: clareza, correção ou referência sem mudança de autoridade.

Toda alteração atualiza este arquivo e, quando necessário,
[`../Start-Here.md`](../Start-Here.md).

## 4.4.0 — 2026-08-01

- Registra a decisão explícita e separada do proprietário de suportar a
  interface do Dashboard em português do Brasil (`pt-BR`) e inglês britânico
  (`en-GB`).
- Exige escolha visual explícita e localização de rótulos, orientações,
  estados, validações e erros pertencentes ao produto, preservando
  acessibilidade e evitando mistura não intencional de idiomas.
- Mantém `interfaceLanguage` independente de `questionLanguage`,
  `answerLanguage` e `contentLanguage`: qualquer idioma de consulta suportado
  pode ser usado em qualquer idioma visual, e textos derivados das fontes
  permanecem sem tradução nas citações.
- Acrescenta `RF-022`, `RNF-016`, `AC-MVP-017` e `BL-M17`, e reconcilia visão,
  arquitetura, contratos, lifecycle, Quality Gates, roadmap, threat model,
  relatório, índices e estado factual com a decisão.
- Exige testes de componente e fluxo para as quatro combinações entre idioma
  visual e `questionLanguage`: `pt-BR×pt-BR`, `pt-BR×en-GB`,
  `en-GB×pt-BR` e `en-GB×en-GB`, sem traduzir citações.
- Mantém idioma inicial, persistência da preferência e fallback como decisões
  futuras de frontend; nenhum desses mecanismos foi inferido da seleção do
  conjunto suportado.
- Mantém ADR-0002 e ADR-0004 a ADR-0006 como `proposed`; a decisão não aceita
  corpus, fonte, provider, persistência, egress, OCI ou qualquer outro
  conteúdo desses ADRs.
- Não altera comportamento executável, instala dependência, acessa rede,
  executa provider, modifica recurso externo nem avança o lifecycle.
- Classifica a mudança como `MINOR` por adicionar uma capacidade funcional e
  critérios de homologação compatíveis antes da implementação pública.
- A auditoria documental aprovou 83 arquivos não ignorados, 30 Markdown, 13
  arquivos em `prompts/`, 22 RF, 16 RNF, 17 critérios de aceitação, 33 itens
  de backlog, 88 definições estáveis sem duplicidade, H1, links, UTF-8/LF,
  status dos ADRs, 30 ameaças, 12 grupos de testes de segurança e
  `git diff --check`.

## 4.3.0 — 2026-08-01

- Registra a decisão explícita do proprietário de suportar perguntas e
  respostas em português do Brasil (`pt-BR`) e inglês britânico (`en-GB`).
- Exige `questionLanguage` canônico em cada consulta, `answerLanguage` igual
  na resposta e `contentLanguage` na evidência/citação, preservando título,
  seção, trecho e demais textos derivados da fonte no idioma original.
- Acrescenta `RF-021`, `RNF-015`, `AC-MVP-016` e `BL-M16`, e reconcilia visão,
  arquitetura, contratos, lifecycle, Quality Gates, roadmap, threat model,
  relatório e estado factual com a decisão.
- Define a matriz determinística `pt-BR→pt-BR`, `en-GB→en-GB`,
  `pt-BR→en-GB` e `en-GB→pt-BR` entre idioma da pergunta e idioma da
  evidência, sem confundir fixtures de teste com o corpus do produto.
- Mantém o idioma visual da interface como decisão de produto separada e não
  infere rótulos, navegação ou localização do Dashboard a partir do contrato
  bilíngue de consulta.
- Mantém ADR-0002 e ADR-0004 a ADR-0006 como `proposed`; a restrição linguística
  decidida vincula a futura baseline aceita, mas não aceita corpus, fonte,
  provider, persistência, egress, OCI ou qualquer outro conteúdo desses ADRs.
- Não altera comportamento executável, instala dependência, acessa rede,
  executa provider, modifica recurso externo nem avança o lifecycle.
- Classifica a mudança como `MINOR` por adicionar uma capacidade funcional e
  critérios de homologação compatíveis antes da implementação pública.
- A auditoria documental aprovou 83 arquivos não ignorados, 30 Markdown, 13
  arquivos em `prompts/`, 21 RF, 15 RNF, 16 critérios de aceitação, 32 itens
  de backlog, 84 definições estáveis sem duplicidade, H1, links, UTF-8/LF,
  status dos ADRs, 30 ameaças, 12 grupos de testes de segurança e
  `git diff --check`.

## 4.2.1 — 2026-08-01

- Executa a reorganização documental aprovada sobre a baseline
  `fb93cf9514c010325d29b07646aecdd36cb0afda`, preservando a equivalência
  integral registrada na matriz `EQ-01` a `EQ-10`.
- Consolida ownership sem criar regra: Governance permanece autoridade para
  semântica de handoff, continuidade, raciocínio e paralelismo; Templates para
  formato e formulários; Quality Gates para resultados auditáveis; AGENTS
  para enforcement transversal mínimo; Start Here para roteamento; Language
  Policy para idioma; Current State para fatos vigentes.
- Condensa somente duplicações normativas ou detalhes temáticos; gatilhos,
  exceções, condições de parada, Human Gate, runtime preflight, payload
  copiável, isolamento paralelo, rastreabilidade e critérios de auditoria
  permanecem explícitos nas respectivas autoridades ou referências.
- Atualiza `AGENTS.md`, `Start-Here.md`, `Language-Policy.md`,
  `Quality-Gates.md`, `Templates.md`, `Current-State.md`, este changelog e o
  índice documental; acrescenta somente uma entrada append-only ao histórico
  de estados.
- Mantém 13 arquivos ativos em `prompts/` e não altera precedência,
  autoridade, segurança, lifecycle, gate, arquitetura, escopo funcional,
  status de ADR ou comportamento executável.
- Classifica a mudança como `PATCH`: clareza, referências e distribuição de
  responsabilidades foram melhoradas sem introdução, remoção ou mudança de
  comportamento normativo.
- A revisão semântica aprovou `EQ-01` a `EQ-10`; a auditoria documental
  aprovou 84 arquivos não ignorados, e escopo, histórico append-only, links,
  formato, versão, status dos ADRs e `git diff --check` permaneceram coerentes.
- A reorganização permaneceu sequencial e local; `Organize.md` continuou não
  rastreado, e os artefatos protegidos permaneceram sem alterações.

## 4.2.0 — 2026-08-01

- Registra o incremento normativo já materializado em `AGENTS.md` pelo commit
  `9d5adba65aea462465c475f311880e5d9afe2b46`.
- Torna permanente a otimização por progresso decisório útil: identificar a
  decisão ou entrega antes da coleta e começar pela resposta, conjunto de
  candidatos ou inspeção local mais simples que possa resolvê-la.
- Separa fatos capazes de alterar seleção, aceite ou execução segura de
  contexto apenas informativo e calibra a profundidade de verificação ao risco
  da decisão.
- Estabelece composição e verificação em duas etapas, preferência por uma
  proposta de autoridade limitada e completa, parada por valor decisório
  decrescente e alternativa defensável quando uma fonte falha repetidamente.
- Preserva sem relaxamento factualidade, segurança, qualidade, lifecycle e
  autoridade explícita; nenhuma regra substituída, decisão arquitetural,
  capability executável ou nova fonte temática foi criada.
- Mantém 13 arquivos ativos em `prompts/`; `Start-Here.md` continua roteando
  `AGENTS.md` como enforcement transversal e não requer alteração normativa
  para aplicar o incremento.
- Classifica a mudança como `MINOR`: adiciona um playbook transversal de
  eficiência e proporcionalidade sem quebra de precedência, autoridade,
  lifecycle, arquitetura ou escopo funcional.
- A reconciliação factual concomitante registra no snapshot e nos índices que
  a verificação pública autorizada de `STATE-02` terminou; ela não compõe o
  motivo SemVer, não aceita ADR, não altera lifecycle/gate e não transforma
  evidência histórica em autoridade vigente.
- A auditoria documental do registro passou para 84 arquivos não ignorados e
  31 Markdown, incluindo `Organize.md` local e não rastreado; confirmou 13
  arquivos em `prompts/`, links e formato válidos, histórico append-only,
  status dos seis ADRs preservados e `git diff --check` limpo.

## 4.1.0 — 2026-07-31

- Torna obrigatório destacar todo payload de `Texto para copiar e enviar` em
  bloco cercado Markdown com identificador `text`, imediatamente abaixo do
  rótulo em linha própria.
- Define que rótulo, cercas de abertura/fechamento e orientação externa não
  integram o conteúdo a copiar; o proprietário copia somente o interior do
  bloco.
- Aplica o mesmo destaque a payloads de uma linha e à frase canônica de Human
  Gate, preservando `nenhum texto é necessário` como sentinela inline sem
  bloco vazio.
- Exige cerca externa alternativa ou mais longa quando o payload contém
  blocos cercados, evitando limite ambíguo ou truncamento da mensagem.
- Mantém `Templates.md` como autoridade temática do formato e atualiza
  `AGENTS.md`, Start Here, Governance e Quality Gates apenas para enforcement,
  roteamento e auditoria coerentes.
- Automatic Quality Gate documental aprovado para 77 arquivos não ignorados,
  24 arquivos Markdown rastreados, 114 links locais válidos, cercas Markdown
  balanceadas, UTF-8/LF/newline final, ausência de trailing whitespace e
  `git diff --check` limpo.
- Classifica a mudança como MINOR porque adiciona um padrão permanente de
  apresentação owner-facing sem alterar autoridade, lifecycle, arquitetura,
  comportamento executável ou escopo funcional.
- `STATE-01 PROJECT_SETUP` permanece encerrado; a entrada em `STATE-02`,
  ADR-0002, rede, GitHub, OCI, providers, corpus, fonte oficial,
  infraestrutura, publicação e deploy permanecem sem autorização.

## 4.0.1 — 2026-07-30

- Corrige o snapshot factual depois da renomeação manual do diretório físico
  do checkout para `RAG-Challenge` e registra a ausência do diretório irmão
  `Challenge`.
- Registra a remoção das sete árvores técnicas legadas após comprovar que
  estavam dentro do checkout, fora de `reference-materials/` e continham zero
  arquivos. As árvores somavam 149 diretórios, incluindo suas sete raízes.
- Remove 15 raízes ignoradas de build e teste que conservavam metadados com o
  path absoluto anterior. A primeira passagem removeu cumulativamente 529
  arquivos gerados: um `*.csproj.FileListAbsolute.txt` durante a validação do
  procedimento e os 528 restantes na limpeza integral das raízes. Desses 529,
  68 continham 501 ocorrências do path anterior; 186 diretórios gerados
  também foram eliminados. Nenhum artefato técnico rastreado foi removido ou
  alterado pela limpeza.
- Verificações .NET recriaram transitoriamente 14 raízes canônicas com 35
  arquivos e zero ocorrência do path anterior; a segunda passagem removeu
  esses 35 arquivos e 56 diretórios reutilizados. O snapshot final não
  conserva `bin/`, `obj/` ou `TestResults/` nos sete projetos.
- Preserva integralmente `reference-materials/`: 24 arquivos, 7.065.607 bytes
  e SHA-256 agregado
  `699708516083ad2e3274098f43352c7ac93280fc6c5a0e6b0a73eaf120e319fe`
  permaneceram idênticos antes e depois da limpeza.
- A auditoria final confirmou 77 arquivos não ignorados, 22 documentos
  governados, 111 links locais válidos, 13 arquivos em `prompts/`, zero raiz
  técnica legada e zero referência ao path absoluto anterior fora de
  `.git/` e `reference-materials/`.
- Classifica a mudança como PATCH porque corrige o presente factual e remove
  artefatos locais reproduzíveis sem alterar autoridade, lifecycle, gates,
  requisitos, arquitetura, comportamento executável ou dependências.
- `STATE-01 PROJECT_SETUP` permanece ativo, com Automatic Quality Gate
  `APROVADO` e Human Gate `PENDENTE`; nenhuma ação remota, mutação de recurso
  externo ou progressão para `STATE-02` foi autorizada.

## 4.0.0 — 2026-07-30

- Registra a solicitação humana explícita `Gostaria de mudar o nome do projeto
  de Challenge para RAG-Challenge` como decisão da identidade canônica.
- Adota `RAG-Challenge` para produto e solution, `RagChallenge` para projetos,
  assemblies, namespaces e configuração .NET, e
  `rag-challenge-dashboard-web` para o package privado npm.
- Aceita o ADR-0003 como registro vigente e marca o ADR-0001 como
  `superseded`; o novo ADR incorpora sem modificação todas as decisões
  anteriores não relacionadas a nomenclatura. A normalização `RagChallenge`
  materializa a mesma identidade em sintaxe válida de C# e não cria outro
  nome público.
- Preserva a decisão histórica do ADR-0001, o histórico append-only, os
  relatórios de execução, as referências ao Challenge da Alura/ONE, os IDs
  `CH-MOD-*`, os códigos `CH_*` e
  `reference-materials/challenge-original/`.
- Migra solution, sete projetos .NET, namespaces, configuração, testes,
  lockfiles, scripts, Dashboard e documentos canônicos sem alterar versão de
  dependência, lógica funcional ou escopo de lifecycle.
- O commit técnico
  `8c347c0fa73fead3e03a1eb979deba9fe3617379` passou no gate offline integral:
  build sem warning ou erro, 15 testes .NET, 88% de linhas, 100% de branches,
  dois testes web, lint, typecheck, build Vite e auditoria de 77 arquivos.
- Health smoke em loopback retornou `200` para liveness e readiness; o
  processo pertencente ao projeto foi encerrado e nenhum listener permaneceu.
  Um clone limpo sem materiais locais reproduziu o gate com worktree limpo.
- O corpus `4.0.0` contém 22 documentos governados, 111 links locais válidos e
  13 arquivos em `prompts/`; links e formato foram validados pela auditoria
  local.
- Classifica a mudança como MAJOR porque altera a identidade canônica e os
  identificadores técnicos existentes. Não encerra `STATE-01`, não decide seu
  Human Gate, não autoriza `STATE-02`, ADR-0002, GitHub, OCI, push,
  publicação, deploy, provider, corpus, fonte oficial ou DB-Notifier.

## 3.5.4 — 2026-07-30

- Renomeia o rótulo owner-facing para `Texto para copiar e enviar`, tornando
  explícito que o payload completo cabe no próprio encerramento.
- Posiciona esse campo imediatamente após `Conversa recomendada`, sem conteúdo
  intermediário; o título sugerido de `START_NEW` permanece dentro do campo de
  conversa.
- Torna o texto obrigatório quando `Sua ação agora` exige continuar, iniciar,
  retomar, responder, confirmar, decidir, autorizar ou enviar algo em uma
  conversa.
- Proíbe adiar o payload, apontar para mensagem anterior ou declarar ausência
  quando a continuidade depende de envio.
- Restringe `nenhum texto é necessário` aos casos em que não existe ação
  imediata do proprietário dependente de mensagem.
- Automatic Quality Gate documental aprovado com 21 documentos públicos, 100
  links locais válidos, 8 rótulos canônicos na ordem definida, 0 problema de
  formato e 0 achado P0–P3 em duas revisões independentes.
- Classifica a mudança como PATCH porque corrige completude e coerência do
  handoff `3.5.3` sem alterar autoridade, lifecycle, gates ou escopo
  funcional.
- Preserva a baseline `3.4.0` que encerrou `STATE-00`; não decide `GATE-B01`,
  não aceita ADR e não autoriza `STATE-01`, Git, código, dependência, rede,
  OCI, GitHub ou ação externa.

## 3.5.3 — 2026-07-30

- Elimina a sobreposição entre próximo passo, etapa, tarefa, atividade e ação
  no encerramento destinado ao proprietário.
- Adota quatro conceitos separados: `Solicitação`, `Próximo trabalho
  recomendado`, `Estado/gate` e `Sua ação agora`.
- Define conversa recomendada apenas como roteamento e mensagem exata apenas
  como payload da ação humana; nenhuma delas substitui a entrega ou concede
  autoridade.
- Define `lote` como unidade governada, `tarefa` como subunidade verificável,
  `atividade` como operação interna e `passo` como item ordenado; nenhum é
  rótulo de handoff.
- Reduz o encerramento para 8 linhas compactas e mantém o encerramento único,
  os seis níveis de
  raciocínio, o paralelismo governado e a regra especial de Human Gate.
- Automatic Quality Gate documental aprovado com 21 documentos públicos, 100
  links locais válidos, 8 rótulos canônicos, 0 problema de formato e 0 achado
  P0–P3 na revisão independente.
- Classifica a mudança como PATCH porque corrige clareza terminológica do
  playbook `3.5.2` sem alterar autoridade, lifecycle, gates ou escopo
  funcional.
- Preserva a baseline `3.4.0` que encerrou `STATE-00`; não decide `GATE-B01`,
  não aceita ADR e não autoriza `STATE-01`, Git, código, dependência, rede,
  OCI, GitHub ou ação externa.

## 3.5.2 — 2026-07-30

- Exige que cada atualização intermediária acrescente informação
  materialmente nova e proíbe repetir, parafrasear ou ecoar conclusões já
  comunicadas sem correção ou consequência alterada.
- Consolida resultados equivalentes, inclusive de workers, em uma única
  atualização em vez de publicar variações semânticas da mesma informação.
- Determina a aplicabilidade do runtime preflight antes de qualquer inspeção
  de processos.
- Classifica documentação e análise read-only como `NÃO APLICÁVEL` para
  runtime preflight: nenhum shutdown é anunciado, nenhum processo é enumerado
  e nada é encerrado.
- Reforça que nome genérico de processo não comprova ownership do Challenge.
- Automatic Quality Gate documental aprovado com 21 documentos públicos, 100
  links locais válidos, 0 problema de formato e 0 achado P0–P3 na revisão
  independente; nenhum processo foi inspecionado ou encerrado.
- Classifica a mudança como PATCH porque corrige redundância e aplicabilidade
  operacional do playbook `3.5.1` sem alterar autoridade, lifecycle, gates ou
  escopo funcional.
- Preserva a baseline `3.4.0` que encerrou `STATE-00`; não decide `GATE-B01`,
  não aceita ADR e não autoriza `STATE-01`, Git, código, dependência, rede,
  OCI, GitHub ou ação externa.

## 3.5.1 — 2026-07-30

- Corrige a cadência do handoff: cada solicitação do proprietário recebe
  exatamente um bloco, somente na resposta final do turno lógico.
- Proíbe repetir ou antecipar o bloco em atualizações intermediárias, que
  permanecem breves e limitadas a progresso, evidência, premissa ou bloqueio.
- Substitui 17 campos separados por um encerramento compacto que agrupa
  continuidade, rota, raciocínio e paralelismo sem perder informação
  obrigatória.
- Torna condicionais o título de conversa, a mensagem para copiar e os
  detalhes de lanes; valores artificiais de ausência deixam de ser repetidos.
- Mantém a regra especial de Human Gate, os seis níveis de raciocínio, a
  classificação de paralelismo, o roteamento verificável e o escopo negativo.
- Automatic Quality Gate documental aprovado com 21 documentos públicos, 100
  links locais válidos, 10 campos compactos, nenhuma regra normativa residual
  de repetição e 0 achado P0–P3 na revisão independente.
- Classifica a mudança como PATCH porque corrige clareza, frequência e
  apresentação do playbook `3.5.0` sem alterar autoridade, lifecycle, gates ou
  escopo funcional.
- Preserva a baseline `3.4.0` que encerrou `STATE-00`; não decide `GATE-B01`,
  não aceita ADR e não autoriza `STATE-01`, Git, código, dependência, rede,
  OCI, GitHub ou ação externa.

## 3.5.0 — 2026-07-30

- Introduz o playbook transversal de recomendação do raciocínio do Codex por
  conversa, sem criar nova autoridade temática ou novo arquivo normativo.
- Define os seis valores canônicos destinados ao proprietário: `Leve`,
  `Médio`, `Alto`, `Extra alto`, `Máximo` e `Ultra`.
- Exige em todo handoff e em cada lane uma recomendação, a justificativa do
  menor nível suficiente e uma alternativa explícita caso o nível não esteja
  disponível.
- Distingue `Máximo`, voltado à profundidade excepcional de uma tarefa
  acoplada, de `Ultra`, reservado a trabalho crítico decomponível que passe no
  gate de paralelismo.
- Registra que disponibilidade varia por superfície, conta, modelo e
  configuração; a recomendação não configura o Codex, não escolhe modelo, não
  concede autoridade e não substitui verificações.
- Atualiza `AGENTS.md`, Start Here, Governance, Templates e Quality Gates para
  aplicar a regra de forma consistente.
- Automatic Quality Gate documental aprovado com 21 documentos públicos, 100
  links locais válidos, 13 arquivos em `prompts/`, cobertura integral dos
  templates de conversa e nenhuma alteração nos requisitos, backlog, módulos,
  riscos ou materiais locais.
- Classifica a mudança como MINOR porque acrescenta um playbook de coordenação
  sem alterar lifecycle, precedência, autoridade de execução ou escopo
  funcional.
- Preserva a baseline `3.4.0` que encerrou o Human Gate de `STATE-00`; não
  reabre esse gate, não decide `GATE-B01`, não aceita ADR e não autoriza
  `STATE-01`, Git, código, dependência, rede, OCI, GitHub ou ação externa.

## 3.4.0 — 2026-07-29

- Estabelece
  [`Language-Policy.md`](../governance/Language-Policy.md) como autoridade
  temática única para idioma da comunicação, artefatos, conteúdo existente,
  nomes externos e interface.
- Move a política da raiz para `prompts/governance/` e faz `AGENTS.md`,
  Start Here, arquitetura, templates, Quality Gates e índices apontarem para
  ela sem duplicar integralmente suas regras.
- Exige que toda comunicação do projeto informe o próximo passo, a
  conversa recomendada e uma mensagem completa em `pt-BR` para o proprietário
  copiar, ou declare explicitamente que nenhuma ação ou mensagem é necessária.
- Mantém em `pt-BR` todos os títulos, rótulos, orientações e valores
  explicativos destinados ao proprietário; literais técnicos preservam a
  grafia exigida.
- Arquiva o prompt genérico de incorporação da coordenação de conversas em
  `reference-materials/governance-inputs/`, fora do futuro versionamento e sem
  transformá-lo em autoridade paralela.
- Preserva os playbooks já incorporados em `AGENTS.md`, Governance, Templates
  e Quality Gates, sem repetir o meta-prompt no corpus normativo.
- Classifica a mudança como MINOR porque introduz uma autoridade temática e
  seu roteamento sem alterar lifecycle, autoridade de execução ou escopo
  funcional do MVP.
- Não traduz a baseline existente, decide idioma de interface, aceita Human
  Gate ou ADR nem autoriza Git, código, dependência, rede ou deploy.

## 3.3.1 — 2026-07-29

- Corrige a regra residual da arquitetura que ainda permitia português em
  documentação futura de produto e governança.
- Mantém o arquivo existente em `pt-BR`, mas alinha novos artefatos técnicos e
  públicos a `en-GB`, preserva nomes externos e separa tradução e idioma da
  interface.
- Traduz para `pt-BR` a prosa explicativa das sentinelas owner-facing de
  ausência de mensagem e paralelismo, preservando nomes de campos e `None`.
- Cria um snapshot corretivo distinto para não reutilizar `3.3.0` após
  alterações documentais.
- Classifica a mudança como PATCH porque reconcilia a política `3.3.0` sem
  alterar estados, autoridade, escopo funcional ou decisão de idioma.
- Não traduz a baseline, aceita Human Gate ou ADR e não autoriza Git, código,
  dependência, rede ou deploy.

## 3.3.0 — 2026-07-29

- Torna `pt-BR` o idioma padrão de toda comunicação com o proprietário,
  incluindo handoffs e mensagens prontas para copiar.
- Torna `en-GB` o idioma padrão de novos artefatos técnicos permanentes,
  documentação técnica/pública, comentários, descrições de API/configuração,
  testes e commits do projeto.
- Preserva nomes impostos por linguagens, frameworks, protocolos, padrões e
  produtos externos.
- Preserva no idioma atual a evidência e os documentos de governança
  existentes do `STATE-00`; tradução integral exige incremento próprio.
- Mantém o idioma da interface como decisão de produto separada.
- Atualiza templates e Quality Gates para transportar e verificar a política.
- Classifica a mudança como MINOR porque acrescenta uma política transversal
  de colaboração e artefatos, sem alterar estados, autoridade de execução ou
  escopo funcional do MVP.
- Não traduz documentos, aceita Human Gate ou ADR e não autoriza Git, código,
  dependência, rede ou deploy.

## 3.2.0 — 2026-07-29

- Introduz o playbook obrigatório de paralelismo seguro entre conversas.
- Todo handoff passa a classificar o trabalho como `SEQUENTIAL_ONLY`,
  `PARALLEL_OPTIONAL` ou `PARALLEL_RECOMMENDED`.
- Define uma conversa coordenadora confirmada, snapshot-base, lanes,
  ownership exclusivo de paths/artefatos/recursos, dependências, checks,
  stop conditions, fallback e ordem de integração.
- Exige mensagem completa por worker e retorno pronto para copiar à
  coordenadora com a evidência real da lane.
- Centraliza integração, memória factual, ADRs, lifecycle e Human Gate na
  coordenadora; worker concluída representa somente candidato.
- Antes de Git rastreado, limita paralelismo a trabalho read-only. Escrita
  paralela futura exige autorização, branch/worktree separado e isolamento de
  recursos mutáveis.
- Classifica a mudança como MINOR porque acrescenta um playbook transversal
  sem alterar estados, autoridade de execução ou escopo funcional do MVP.
- Não aceita Human Gate ou ADR e não autoriza Git, código, dependência, rede ou
  deploy.

## 3.1.0 — 2026-07-29

- Introduz o playbook obrigatório de continuidade entre conversas.
- Todo handoff passa a classificar a próxima interação como
  `CONTINUE_CURRENT`, `START_NEW` ou `RETURN_TO_EXISTING`.
- Exige target, motivo, instrução de navegação e mensagem exata pronta para o
  proprietário copiar em conversas atuais, novas ou anteriores.
- Proíbe inventar conversa existente, link ou ID e deixa explícito que o
  proprietário navega manualmente.
- Mantém a confirmação de Human Gate na conversa que contém o resumo completo;
  nova/antiga conversa deve republicar o gate antes de solicitar a frase.
- Faz cada nova ou retomada reler as autoridades e reconciliar contexto antigo
  com `Current-State.md`.
- Classifica a mudança como MINOR porque adiciona um playbook transversal sem
  alterar estados, gates, autoridade de execução ou escopo funcional do MVP.
- Não aceita Human Gate ou ADR e não autoriza Git, código, dependência, rede ou
  deploy.

## 3.0.1 — 2026-07-29

- Corrige lacunas encontradas pela auditoria adicional da baseline `3.0.0`.
- Define armazenamento content-addressed para bytes documentais reabríveis,
  staging não consultável, manifesto final com integridade/contagens e
  identidade determinística de geração finalizada.
- Substitui o ponteiro isolado por `CorpusActivationRecord` transacional que
  vincula geração, snapshot, observação e auditoria.
- Torna hard pre-filter parte do contrato vetorial e alinha erros, readiness e
  metadados públicos.
- Fecha DNS rebinding/TOCTOU por canonicalização e pinning de IP/Host/SNI e
  cria `VECTOR_STORE_EGRESS` para adapters gerenciados; URL oficial é pública
  sem credenciais e validação TLS não cria egress lateral não autorizado.
- Define validators HTTP, retirada/desativação, histórico completo de ativação
  e rollback sem segunda autoridade de estado.
- Corrige o ownership da integração: Challenge possui OpenAPI; adapters
  consumidores pertencem aos repositórios consumidores.
- Amplia o checklist do `GATE-B01` com mapa módulo/físico e forma de execução
  administrativa e define os registros canônicos do gate, sem tomar essas
  decisões.
- Classifica a mudança como PATCH porque corrige e torna verificáveis contratos
  já propostos, sem alterar estados, autoridade ou escopo funcional do MVP.
- Não aceita ADR, não promove estado e não autoriza Git, código, dependência,
  rede ou deploy.

## 3.0.0 — 2026-07-29

- Promove uma fonte oficial online de evolução futura para requisito do MVP.
- Define um corpus lógico com `SourceScope=Local|OfficialOnline`, sem mistura,
  fallback ou navegação web no fluxo de pergunta.
- Limita o MVP a um PDF oficial em URL HTTPS exata, sincronização manual,
  snapshot, freshness e geração conjunta.
- Ativa `CH-MOD-07`, `OFFICIAL_SOURCE_EGRESS` deny-by-default e as
  responsabilidades de implementação/teste entre `STATE-02` e `STATE-08`.
- Eleva MAJOR porque muda escopo obrigatório, contratos, segurança e critérios
  de saída de vários estados após uma baseline já auditada.
- Não autoriza acesso de rede, ADR, Git, scaffold, código, dependência ou
  transição.

## 2.0.0 — 2026-07-29

- Introduz o `GATE-B01` entre o Human Gate de `STATE-00` e a autorização
  separada de entrada em `STATE-01`.
- Eleva MAJOR porque altera autoridade e a sequência obrigatória de gates.
- Remove autoridade implícita de ADR proposto sobre stack e scaffold.
- Define uma única autoridade de ativação do índice, rollback obrigatório e
  chave completa de compatibilidade.
- Separa egress de providers de IA, fontes oficiais e runtime OCI.
- Detalha contrato HTTP futuro do DB-Notifier, frescor de fontes, operação
  administrativa local e proteção de saída contra XSS.
- Acrescenta IDs de aceite e rastreabilidade de requisitos até backlog,
  estados e testes.

## 1.0.0 — 2026-07-29

- Cria a entrada única e o roteamento do corpus.
- Registra visão, requisitos, escopo do MVP e evolução futura.
- Propõe arquitetura modular independente e compatível com os princípios do
  DB-Notifier.
- Define o módulo RAG, proveniência, versões de documento e índice,
  reconstrução segura, rollback, múltiplos acervos futuros e fontes oficiais
  externas desativadas.
- Adapta o lifecycle `STATE-00` a `STATE-08` ao Challenge.
- Define Quality Gates, Human Gates, segurança, acesso, logging, auditoria,
  testes e CI.
- Cria Current State, histórico append-only e templates.
- Mantém `STATE-00 DISCOVERY`, sem autorizar Git init, código, dependências,
  API, banco, UI, consumo externo, deploy ou transição.
