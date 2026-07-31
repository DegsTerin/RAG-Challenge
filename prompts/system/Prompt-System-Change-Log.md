# Controle e Changelog do Sistema de Instruções

## Versão atual

- Versão: `3.5.4`
- Data: 2026-07-30
- Status: correção de completude do texto para copiar auditada; `GATE-B01`
  pendente
- Escopo: 13 arquivos ativos em `prompts/`

A versão do corpus é independente da versão futura do software.

## Política SemVer

- `MAJOR`: mudança incompatível de autoridade, precedência, estados ou
  estrutura.
- `MINOR`: nova capacidade, módulo, playbook ou gate sem quebra do fluxo.
- `PATCH`: clareza, correção ou referência sem mudança de autoridade.

Toda alteração atualiza este arquivo e, quando necessário,
[`../Start-Here.md`](../Start-Here.md).

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
