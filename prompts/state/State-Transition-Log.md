# Histórico de Estados e Incrementos

## Regras

- Este arquivo é append-only.
- Datas usam ISO 8601.
- Correções são novas entradas que referenciam a entrada corrigida.
- Uma recomendação, proposta ou próxima ação é histórica no contexto da
  entrada e não substitui
  [`Current-State.md`](Current-State.md).
- Uma entrada sem `Estado resultante` diferente não representa transição.
- Quality Gate e Human Gate são registrados separadamente.
- Nenhum estado é promovido por inferência.

## 2026-07-29 — Organização dos materiais originais

- Estado anterior: pré-governança formal do Challenge.
- Estado resultante: sem transição.
- Autoridade: solicitação explícita para organizar os arquivos antes do
  desenvolvimento.
- Escopo: mover os materiais originais para
  `reference-materials/challenge-original/` sem alterar conteúdo.
- Resultado observado: 23 arquivos preservados e legíveis.
- Escopo negativo: nenhum código, documento de produto, Git, commit, DB-Notifier
  ou sistema externo alterado.
- Evidência: inventário local e verificação de SHA-256.

## 2026-07-29 — Materiais locais excluídos do futuro versionamento

- Estado anterior: pré-governança formal do Challenge.
- Estado resultante: sem transição.
- Autoridade: solicitação explícita para que os materiais não fossem enviados
  ao GitHub.
- Escopo: criar `.gitignore` com `/reference-materials/`.
- Resultado observado: 23 arquivos permaneceram locais; nenhum repositório Git
  foi iniciado.
- Escopo negativo: nenhum arquivo original alterado e nenhuma ação no GitHub.

## 2026-07-29 — STATE-00 documental autorizado

- Estado anterior: pré-governança formal do Challenge.
- Estado resultante: `STATE-00 DISCOVERY`.
- Autoridade humana exata:
  `Aprovo a criação da documentação, aprovo a estrutura documental para
  iniciar o STATE-00 — DISCOVERY`.
- Escopo autorizado: criar a estrutura documental aprovada, concluir o
  Discovery e preparar `STATE-01`.
- Escopo negativo preservado: zero código de negócio, API, banco, interface,
  dependência, Git init, consumo externo, deploy ou transição para `STATE-01`.
- Efeito: autorização de trabalho documental; não é Human Gate de encerramento
  do estado.
- Próxima condição: produzir e auditar a baseline documental antes de
  apresentar o Human Gate exclusivo de `STATE-00`.

O modelo para futuras entradas está em
[`../templates/Templates.md`](../templates/Templates.md). Este histórico
contém somente fatos cronológicos.

## 2026-07-29 — Automatic Quality Gate documental concluído

- Estado anterior: `STATE-00 DISCOVERY`.
- Estado resultante: `STATE-00 DISCOVERY`; sem transição.
- Autoridade: escopo documental aprovado pelo proprietário.
- Escopo: auditar a baseline de 20 documentos e reconciliar revisões
  independentes.
- Resultado observado: gate automático documental `APROVADO`; 69 links locais
  válidos, 0 problema de formato, 0 ID duplicado, 0 achado de secret/host e 0
  arquivo de implementação.
- Materiais locais: 23 arquivos legíveis, 7.054.476 bytes, preservados sob a
  regra `/reference-materials/`.
- ADRs: ADR-0001 e ADR-0002 permaneceram `proposed`.
- Escopo negativo: nenhum Git init, scaffold, dependência, código, API, banco,
  interface, consumo externo, OCI, GitHub ou DB-Notifier alterado.
- Limitações: sem renderização visual dos PDFs; ausência de alteração externa
  no DB-Notifier controlada pela sessão, mas não reproduzível neste workspace.
- Human Gate: `PENDENTE`.
- Próxima condição: revisão humana do relatório e frase inequívoca exclusiva
  para `STATE-00`.

## 2026-07-29 — Fonte oficial online promovida ao MVP

- Estado anterior: `STATE-00 DISCOVERY`, Automatic Quality Gate documental
  aprovado e Human Gate pendente.
- Estado resultante: `STATE-00 DISCOVERY`; sem transição.
- Autoridade humana exata:
  `Sim quero funcionando já na entrega do MVP`.
- Contexto: resposta à confirmação de que a opção de consultar documentação
  oficial online estava apenas prevista para evolução futura.
- Decisão: promover uma opção funcional de fonte oficial online ao escopo do
  MVP.
- Recorte documental adotado: uma fonte oficial allowlisted, sincronização
  manual para snapshot versionado e seleção explícita `Local` ou
  `OfficialOnline`; sem URL arbitrária, crawling genérico ou navegação livre.
- Efeito no gate: o resultado automático anterior permanece histórico, mas a
  baseline modificada exige nova auditoria antes do Human Gate.
- Escopo negativo preservado: nenhum acesso real à internet, Git init,
  scaffold, dependência, código, API, banco, interface, deploy ou transição.
- Próxima condição: atualizar os documentos proprietários e revalidar o
  Automatic Quality Gate de `STATE-00`.

## 2026-07-29 — Automatic Quality Gate 3.0.0 revalidado

- Estado anterior: `STATE-00 DISCOVERY`, baseline `3.0.0` em revalidação.
- Estado resultante: `STATE-00 DISCOVERY`; sem transição.
- Autoridade: decisão explícita do proprietário para incluir uma fonte oficial
  online funcional no MVP, limitada ao trabalho documental vigente.
- Escopo: reconciliar requisitos, arquitetura, RAG, segurança, lifecycle,
  roadmap, critérios, riscos, estado e evidência; repetir o gate documental.
- Resultado observado: gate automático `APROVADO` para a baseline `3.0.0`;
  20 documentos, 69 links locais válidos, 20 RF, 14 RNF, 15 critérios de
  aceitação, 31 itens de backlog, 8 módulos, 13 riscos e 0 achado P0–P3
  residual após três rechecagens independentes.
- Materiais locais: 23 arquivos, 7.054.476 bytes e manifesto
  `DFA42E52BF38DEA990C8FD560B7D0E01C4CB5C95E525A9234460221ACD4AD17F`,
  preservados sob a regra `/reference-materials/`.
- Arquitetura oficial no MVP: uma URL HTTPS PDF exata, sync administrativo
  manual, snapshot imutável, observações de revalidação, escopo explícito
  `Local`/`OfficialOnline`, isolamento antes do top-k e nenhum fetch durante a
  pergunta.
- ADRs: ADR-0001 e ADR-0002 permaneceram `proposed`.
- Escopo negativo: nenhum acesso de rede, Git init, scaffold, dependência,
  código, API, banco, interface, OCI, GitHub, deploy ou transição.
- Human Gate: `PENDENTE`.
- Próxima condição: revisão humana do relatório e confirmação inequívoca
  exclusiva para `STATE-00`.

## 2026-07-29 — Correção documental e Quality Gate 3.0.1

- Estado anterior: `STATE-00 DISCOVERY`, baseline `3.0.0` aprovada
  automaticamente e Human Gate pendente.
- Estado resultante: `STATE-00 DISCOVERY`; sem transição.
- Autoridade humana exata:
  `Seguir com a sugestão do: "Fluxo correto para avançar"`.
- Interpretação limitada: executar a correção documental recomendada,
  revalidar a baseline e reapresentar o Human Gate; não inferir a confirmação
  exclusiva exigida para encerrá-lo.
- Escopo: corrigir e reconciliar conteúdo bruto reabrível, manifesto/staging,
  ativação/freshness/rollback, isolamento vetorial, TLS/egress, OpenAPI,
  ownership de integração, governança e rastreabilidade.
- Resultado observado: corpus `3.0.1`; gate automático documental `APROVADO`;
  20 documentos, 71 links locais válidos, 20 RF, 14 RNF, 15 critérios de
  aceitação, 31 itens de backlog, 8 módulos, 13 riscos e 0 achado P0–P3
  residual após três revisões independentes.
- Materiais locais: 23 arquivos, 7.054.476 bytes e manifesto
  `DFA42E52BF38DEA990C8FD560B7D0E01C4CB5C95E525A9234460221ACD4AD17F`,
  preservados sob a regra `/reference-materials/`.
- ADRs: ADR-0001 e ADR-0002 permaneceram `proposed`.
- Escopo negativo: nenhum acesso de rede, Git init, scaffold, dependência,
  código, API, banco, interface, OCI, GitHub, deploy ou alteração no
  DB-Notifier.
- Human Gate: `PENDENTE`, sem ressalva técnica proposta.
- Próxima condição: revisão humana do relatório e confirmação inequívoca
  exclusiva para `STATE-00`; depois, `GATE-B01` separado.

## 2026-07-29 — Playbook de continuidade e Quality Gate 3.1.0

- Estado anterior: `STATE-00 DISCOVERY`, baseline `3.0.1` aprovada
  automaticamente e Human Gate pendente.
- Estado resultante: `STATE-00 DISCOVERY`; sem transição.
- Autoridade humana exata:
  `Boa dica, incluir na documentação de sempre informar a recomendação,
  orientação de iniciar nova conversa ou continuar na atual, voltar para a
  conversa xyz e de sempre informar qual texto usar nas novas conversas e
  conversas anteriores, antigas.`
- Escopo: tornar obrigatório o roteamento
  `CONTINUE_CURRENT`/`START_NEW`/`RETURN_TO_EXISTING`, com target, motivo,
  orientação manual e mensagem exata pronta para copiar.
- Guard rails: conversa anterior exige título/label confirmado; referência
  inexistente não é inventada; toda retomada relê autoridades e reconcilia
  contexto antigo com Current State; confirmação de Human Gate não sai da
  conversa que contém seu resumo completo vigente.
- Resultado observado: corpus `3.1.0`; gate automático documental `APROVADO`;
  20 documentos, 73 links locais válidos, 20 RF, 14 RNF, 15 critérios de
  aceitação, 31 itens de backlog, 8 módulos, 13 riscos e 0 achado P0–P3
  residual após revisão independente.
- Materiais locais: 23 arquivos, 7.054.476 bytes e manifesto
  `DFA42E52BF38DEA990C8FD560B7D0E01C4CB5C95E525A9234460221ACD4AD17F`,
  preservados sob a regra `/reference-materials/`.
- ADRs: ADR-0001 e ADR-0002 permaneceram `proposed`.
- Escopo negativo: nenhum Git init, código, dependência, rede, OCI, GitHub,
  deploy, alteração no DB-Notifier ou transição.
- Human Gate: `PENDENTE`, sem ressalva técnica proposta.
- Próxima condição: confirmação inequívoca exclusiva para `STATE-00`; depois,
  `GATE-B01` separado.

## 2026-07-29 — Paralelismo seguro e Quality Gate 3.2.0

- Estado anterior: `STATE-00 DISCOVERY`, baseline `3.1.0` aprovada
  automaticamente e Human Gate pendente.
- Estado resultante: `STATE-00 DISCOVERY`; sem transição.
- Autoridade humana exata:
  `E também, quando possivel, recomedação e orientação de varias conversas
  rodando simultaneamente multiplas tarefas ao mesmo tempo, claro que quando
  possivel para não gerar erro, conflito ou falha no fluxo de desenvolvimento
  do Challenge.`
- Interpretação limitada: acrescentar orientação documental de paralelismo
  seguro, sem iniciar conversas, Git, código ou qualquer execução externa.
- Escopo: classificar o trabalho como sequencial, paralelo opcional ou
  paralelo recomendado; definir coordenadora confirmada, snapshot-base,
  ownership exclusivo, mensagens por lane, stop conditions, fallback e
  integração serializada.
- Guard rails: workers não ampliam autoridade, não integram outras lanes e
  não atualizam memória factual, ADR, lifecycle ou Human Gate; sem Git,
  conversas simultâneas permanecem read-only e toda escrita fica na
  coordenadora.
- Resultado observado: corpus `3.2.0`; gate automático documental `APROVADO`;
  20 documentos, 73 links locais válidos, 20 RF, 14 RNF, 15 critérios de
  aceitação, 31 itens de backlog, 8 módulos, 13 riscos e 0 achado P0–P3
  residual após revisão independente.
- Materiais locais: 23 arquivos, 7.054.476 bytes e manifesto
  `DFA42E52BF38DEA990C8FD560B7D0E01C4CB5C95E525A9234460221ACD4AD17F`,
  preservados sob a regra `/reference-materials/`.
- Escopo negativo: nenhum Git init, branch/worktree, código, dependência,
  rede, OCI, GitHub, deploy, alteração no DB-Notifier ou transição.
- Human Gate: `PENDENTE`, sem ressalva técnica proposta.
- Próxima condição: confirmação inequívoca exclusiva para `STATE-00`; depois,
  `GATE-B01` separado.

## 2026-07-29 — Política linguística e Quality Gate 3.3.0

- Estado anterior: `STATE-00 DISCOVERY`, baseline `3.2.0` aprovada
  automaticamente e Human Gate pendente.
- Estado resultante: `STATE-00 DISCOVERY`; sem transição.
- Autoridade humana exata: `Tornar isso padrão`.
- Contexto confirmado pelo proprietário: conversas em `pt-BR`, projeto em
  `en-GB`, preservação da evidência existente do `STATE-00` e idioma de
  interface tratado separadamente.
- Interpretação limitada: tornar permanente a política linguística sem
  traduzir a baseline existente, iniciar desenvolvimento ou inferir idioma de
  interface.
- Escopo: atualizar instruções, roteamento, templates, Quality Gates, versão,
  estado, histórico, índice e relatório documental.
- Resultado observado: corpus `3.3.0`; gate automático documental `APROVADO`;
  20 documentos, 73 links locais válidos, 20 RF, 14 RNF, 15 critérios de
  aceitação, 31 itens de backlog, 8 módulos, 13 riscos e 0 achado P0–P3
  residual após revisão independente.
- Materiais locais: 23 arquivos, 7.054.476 bytes e manifesto
  `DFA42E52BF38DEA990C8FD560B7D0E01C4CB5C95E525A9234460221ACD4AD17F`,
  preservados sob a regra `/reference-materials/`.
- Escopo negativo: nenhuma tradução integral, decisão de idioma da interface,
  Git init, branch/worktree, código, dependência, rede, OCI, GitHub, deploy,
  alteração no DB-Notifier ou transição.
- Human Gate: `PENDENTE`, sem ressalva técnica proposta.
- Próxima condição: republicar a baseline `3.3.0` na nova conversa e obter a
  confirmação inequívoca exclusiva de `STATE-00`; depois, `GATE-B01`
  separado.

## 2026-07-29 — Correção de coerência linguística 3.3.0

- Entrada relacionada: `Política linguística e Quality Gate 3.3.0`.
- Estado resultante: `STATE-00 DISCOVERY`; sem transição.
- Achado: a arquitetura de solução ainda permitia português em documentação
  futura de produto e governança, em conflito com a nova regra `en-GB`.
- Correção: o documento existente permaneceu em `pt-BR`, mas sua regra futura
  passou a exigir novos artefatos técnicos/públicos em `en-GB`, preservar
  nomes externos e manter tradução integral e idioma de interface como
  decisões separadas.
- Resultado: conflito P2 reconciliado; 0 achado P0–P3 residual após
  rechecagem independente.
- Escopo negativo: nenhuma tradução, implementação, alteração de lifecycle,
  Git ou ação externa.

## 2026-07-29 — Correção de sentinelas owner-facing 3.3.0

- Entrada relacionada: `Política linguística e Quality Gate 3.3.0`.
- Estado resultante: `STATE-00 DISCOVERY`; sem transição.
- Achado: valores explicativos obrigatórios de ausência de mensagem e
  paralelismo ainda estavam em inglês nos handoffs destinados ao proprietário.
- Correção: nomes de campo e a sentinela técnica `None` foram preservados,
  enquanto a explicação passou a `pt-BR`.
- Resultado: conflito P2 reconciliado; 0 achado P0–P3 residual após
  rechecagem independente.
- Escopo negativo: nenhuma mudança de autoridade, tradução integral,
  implementação, Git ou ação externa.

## 2026-07-29 — Quality Gate corretivo 3.3.1

- Entradas relacionadas: `Política linguística e Quality Gate 3.3.0` e suas
  duas correções.
- Estado anterior e resultante: `STATE-00 DISCOVERY`; sem transição.
- Autoridade preservada: solicitação `Tornar isso padrão`, limitada à política
  linguística documental.
- Correção histórica: as duas entradas anteriores declararam 0 achado
  residual antes de a revisão identificar que o mesmo número `3.3.0`
  representava snapshots diferentes.
- Decisão SemVer: consolidar as correções como `3.3.1` PATCH, sem reescrever
  as entradas históricas.
- Resultado observado: corpus `3.3.1`; gate automático documental `APROVADO`;
  20 documentos, 73 links locais válidos, 20 RF, 14 RNF, 15 critérios de
  aceitação, 31 itens de backlog, 8 módulos, 13 riscos e 0 achado P0–P3
  residual após duas revisões independentes e reconciliação central.
- Política vigente: comunicação owner-facing em `pt-BR`; novos artefatos
  técnicos/públicos permanentes em `en-GB`; nomes externos preservados;
  documentos existentes mantêm o próprio idioma; tradução integral e UI
  exigem decisões separadas.
- Materiais locais: 23 arquivos, 7.054.476 bytes e manifesto
  `DFA42E52BF38DEA990C8FD560B7D0E01C4CB5C95E525A9234460221ACD4AD17F`,
  preservados sob a regra `/reference-materials/`.
- Escopo negativo: nenhuma tradução integral, decisão de UI, Git init,
  branch/worktree, código, dependência, rede, OCI, GitHub, deploy, alteração
  no DB-Notifier ou transição.
- Human Gate: `PENDENTE`, sem ressalva técnica proposta.
- Próxima condição: republicar a baseline `3.3.1` na nova conversa e obter a
  confirmação inequívoca exclusiva de `STATE-00`; depois, `GATE-B01`
  separado.

## 2026-07-29 — Autoridade temática de idioma e Quality Gate 3.4.0

- Estado anterior e resultante: `STATE-00 DISCOVERY`; sem transição.
- Autoridade humana exata:
  `Aprovo o plano revisado: mover Language-Policy.md para
  prompts/governance, arquivar Conversation-Coordination-Prompt.md em
  reference-materials/governance-inputs, integrar a política como autoridade
  temática e atualizar a baseline para 3.4.0. Inclua também como regra
  permanente que toda comunicação do projeto informe o próximo passo e
  forneça um texto exato, completo e em pt-BR para eu copiar e enviar na
  conversa indicada. Não execute Git, código ou ações externas.`
- Escopo: mover e consolidar a política de idioma, arquivar o meta-prompt sem
  torná-lo normativo, atualizar instruções, roteamento, arquitetura,
  governança, templates, Quality Gates, índices, versão, relatório, estado e
  histórico.
- Autoridade temática: `prompts/governance/Language-Policy.md` tornou-se a
  única proprietária das convenções de idioma; os demais documentos apenas
  apontam para ela e aplicam suas regras.
- Comunicação: toda interação do projeto informa próximo passo, conversa
  recomendada e mensagem completa em `pt-BR` pronta para copiar, ou declara
  explicitamente que nenhuma ação ou mensagem é necessária.
- Prompt de coordenação: preservado em
  `reference-materials/governance-inputs/Conversation-Coordination-Prompt.md`
  com 11.131 bytes e SHA-256
  `0019950242314908762CAD3E2AEA01C122023E3867885289E04FB3A70CA912D4`;
  conteúdo e hash permaneceram inalterados.
- Resultado observado: corpus `3.4.0`; gate automático documental `APROVADO`;
  21 documentos, 96 links locais válidos, 20 RF, 14 RNF, 15 critérios de
  aceitação, 31 itens de backlog, 8 módulos, 13 riscos e 0 achado P0–P3
  residual após revisão independente e reconciliação central.
- Materiais locais: 24 arquivos e 7.065.607 bytes sob
  `/reference-materials/`; os 23 materiais originais e seu manifesto
  permaneceram inalterados.
- Escopo negativo: nenhuma tradução em massa, decisão de idioma da interface,
  Git, código, dependência, rede, OCI, GitHub, deploy, alteração no DB-Notifier
  ou transição.
- Human Gate: `PENDENTE`, sem ressalva técnica proposta.
- Próxima condição: revisar o resumo completo da baseline `3.4.0` e decidir o
  Human Gate exclusivo de `STATE-00`; depois, `GATE-B01` separado.

## 2026-07-30 — Human Gate de STATE-00 aprovado

- Estado anterior: `STATE-00 DISCOVERY`, Automatic Quality Gate documental
  `APROVADO` para a baseline `3.4.0` e Human Gate `PENDENTE`.
- Estado solicitado: encerrar exclusivamente o Discovery documental de
  `STATE-00`.
- Autoridade humana exata:
  `Confirmo a decisão acima exclusivamente para STATE-00`.
- Decisão: `APROVADO` sem ressalvas, exclusivamente para `STATE-00`.
- Escopo: aceitar os entregáveis documentais, riscos, limitações, arquitetura
  proposta, roadmap e backlog da baseline `3.4.0` como conclusão do
  Discovery.
- Escopo negativo: a decisão não aceita ADR-0001 ou ADR-0002, não decide
  `GATE-B01`, não escolhe licença, providers, corpus ou fonte oficial e não
  autoriza `STATE-01`, Git init, scaffold, dependências, código, API, banco,
  interface, rede, OCI, GitHub, consumo externo ou deploy.
- Pré-condições: resumo completo da baseline `3.4.0` apresentado na mesma
  conversa coordenadora; revalidação somente leitura concluída; frase
  inequívoca recebida sem alteração.
- Mudanças: Human Gate de `STATE-00` encerrado; `GATE-B01` tornou-se a próxima
  decisão pendente; nenhuma implementação ou transição para `STATE-01`
  ocorreu.
- Verificações/evidências: 21 documentos públicos e 13 arquivos em
  `prompts/`; 96 links locais válidos; formato consistente; 20 RF, 14 RNF, 15
  critérios de aceitação, 31 itens de backlog, 8 módulos e 13 riscos; ausência
  de `.git` e artefatos de implementação; manifesto dos 23 materiais originais
  `DFA42E52BF38DEA990C8FD560B7D0E01C4CB5C95E525A9234460221ACD4AD17F`;
  prompt arquivado com SHA-256
  `0019950242314908762CAD3E2AEA01C122023E3867885289E04FB3A70CA912D4`.
- Limitações/riscos: não existe manifesto histórico dos 21 documentos
  públicos para comparação binária; a integridade foi revalidada pelos
  invariantes documentais. Runtime, build, rede, OCI, GitHub e comportamento
  executável permaneceram não aplicáveis ou sem autoridade.
- Quality Gate: `APROVADO` para a baseline documental `3.4.0`.
- Human Gate: `APROVADO` sem ressalvas em 2026-07-30.
- Estado resultante: `STATE-00 DISCOVERY` encerrado; `GATE-B01
  ARCHITECTURE_BOOTSTRAP_DECISION` pendente; sem entrada em `STATE-01`.
- Próxima condição: decisão humana explícita de `GATE-B01`; depois de seu
  registro, autorização humana separada para entrada em `STATE-01`.
- Aprovador: proprietário do Challenge.

## 2026-07-30 — Orientação de raciocínio por conversa e Quality Gate 3.5.0

- Estado anterior: `STATE-00 DISCOVERY` encerrado pela baseline `3.4.0`;
  `GATE-B01 ARCHITECTURE_BOOTSTRAP_DECISION` pendente; corpus de instruções
  `3.4.0`.
- Estado solicitado: nenhum; incorporar um incremento documental transversal
  sem transição de lifecycle.
- Autoridade humana exata:
  `Incluir na documentação a orientação, sugestão na de qual raciocinio do
  Codex usar em cada conversa: Leve, Médio, Alto, Extra alto, Máximo ou
  Ultra.`
- Decisão: autorizado o incremento documental `3.5.0`; a solicitação não
  constitui decisão de `GATE-B01`.
- Escopo: centralizar em Governance a recomendação do raciocínio do Codex por
  conversa; definir seis valores canônicos, critérios, disponibilidade e
  fallback; aplicar nível, justificativa e alternativa a handoffs, rotas,
  coordenadora e workers; atualizar Quality Gates, índices, versão e estado.
- Escopo negativo: nenhuma reabertura do Human Gate de `STATE-00`, decisão de
  `GATE-B01`, aceitação de ADR, entrada em `STATE-01`, escolha de modelo,
  configuração automática do Codex, Git, código, dependência, API, banco,
  interface, rede, OCI, GitHub, consumo externo, deploy ou alteração no
  DB-Notifier.
- Pré-condições: Human Gate de `STATE-00` aprovado sem ressalvas em
  2026-07-30; baseline de encerramento `3.4.0`; ausência de `.git` e de
  implementação; pedido explícito limitado à documentação.
- Mudanças: corpus vigente elevado a `3.5.0`, ainda com 13 arquivos em
  `prompts/`; Governance mantém a autoridade temática; Templates exige
  `Raciocínio do Codex recomendado`, `Justificativa do raciocínio` e
  `Alternativa se indisponível`; nenhum arquivo normativo novo foi criado.
- Verificações/evidências: auditoria PowerShell 7.6.4 e ripgrep 15.2.0
  somente de leitura em `<challenge-root>`; 21 documentos públicos, 22
  arquivos fora de `reference-materials/`, 13 arquivos em `prompts/`, 100
  links locais válidos e 0 problema de UTF-8, BOM, LF, newline final,
  trailing whitespace, NUL, H1 ou fence; 20 RF, 14 RNF, 15 critérios de
  aceitação, 31 itens de backlog, 8 módulos e 13 riscos; seis níveis e seis
  fallbacks presentes; campos completos nas três rotas e no template de
  worker; 0 achado P0–P3 na revisão independente.
- Materiais locais: 23 materiais originais, 7.054.476 bytes e manifesto
  `DFA42E52BF38DEA990C8FD560B7D0E01C4CB5C95E525A9234460221ACD4AD17F`;
  prompt arquivado com 11.131 bytes e SHA-256
  `0019950242314908762CAD3E2AEA01C122023E3867885289E04FB3A70CA912D4`;
  todos preservados.
- Limitações/riscos: disponibilidade dos níveis varia por superfície, conta,
  modelo e configuração; os nomes técnicos são correspondências
  informativas. Nenhum acesso à rede foi executado, e a recomendação não
  prova nem altera a configuração ativa do Codex.
- Quality Gate: `APROVADO` para o incremento documental `3.5.0`; não substitui
  nem reabre o Human Gate de `STATE-00`.
- Human Gate de `STATE-00`: permanece `APROVADO` sem ressalvas para a baseline
  `3.4.0`.
- Estado resultante: `STATE-00 DISCOVERY` encerrado; corpus vigente `3.5.0`;
  `GATE-B01 ARCHITECTURE_BOOTSTRAP_DECISION` pendente; sem entrada em
  `STATE-01`.
- Próxima condição: revisar e decidir explicitamente `GATE-B01`; depois de seu
  registro, autorização humana separada para entrada em `STATE-01`.
- Aprovador do incremento documental: proprietário do Challenge.

## 2026-07-30 — Encerramento único por solicitação e Quality Gate 3.5.1

- Estado anterior: `STATE-00 DISCOVERY` encerrado; corpus vigente `3.5.0`;
  `GATE-B01 ARCHITECTURE_BOOTSTRAP_DECISION` pendente.
- Estado solicitado: nenhum; corrigir frequência e apresentação do handoff
  sem transição de lifecycle.
- Autoridade humana exata:

  ```text
  Lembrando: texto informativo e sugestões, exemplo abaixo, é apenas para informar no final de cada solicitação na conversa, e não varias vezes na mesma solicitação como ocorreu na ultima solicitação, corrigir isso na documentação, objetivo é otimização, praticidade, e também melhorar estes texto:

  Situação: parcial
  Concluído: 5 documentos normativos alterados.
  Restante para este objetivo: 5 documentos de versão/estado/índice e 2 rodadas de auditoria.
  Próximo passo: executar a pré-auditoria documental somente de leitura.
  Próxima etapa: GATE-B01, ainda pendente de decisão explícita após este incremento.
  Sua ação agora: nenhuma.
  Ação da conversa: CONTINUE_CURRENT
  Destino da conversa: current
  Título sugerido: nenhum.
  Motivo da conversa: concluir e auditar o incremento documental já autorizado.
  Mensagem exata para copiar: nenhuma mensagem é necessária.
  Raciocínio do Codex recomendado: Alto.
  Justificativa do raciocínio: alteração normativa multiarquivo com consistência transversal e verificações documentais, sem exigir decisão arquitetural excepcional.
  Alternativa se indisponível: Médio, com revisão independente e repetição integral dos checks.
  Classificação do trabalho paralelo: SEQUENTIAL_ONLY.
  Plano paralelo: nenhuma escrita paralela; revisão final independente somente de leitura.
  Mensagens paralelas exatas: nenhuma — trabalho paralelo não é recomendado neste momento.
  ```

- Decisão: autorizado o ajuste documental `3.5.1`; a solicitação não decide
  `GATE-B01`.
- Escopo: emitir exatamente um encerramento por solicitação, somente na
  resposta final; manter comentários intermediários breves; compactar os
  campos obrigatórios; tornar título, plano e mensagens de lanes
  condicionais; preservar roteamento, Human Gate, raciocínio e paralelismo.
- Escopo negativo: nenhuma supressão de informação obrigatória, alteração da
  frase canônica de Human Gate, reabertura de `STATE-00`, decisão de
  `GATE-B01`, aceitação de ADR, entrada em `STATE-01`, Git, código,
  dependência, rede, OCI, GitHub, ação externa ou alteração no DB-Notifier.
- Mudanças: corpus vigente elevado a `3.5.1`; o encerramento passou de 17
  campos separados para 10 linhas compactas; exatamente um bloco final por
  solicitação; ausência do bloco em atualizações intermediárias; detalhes
  condicionais aparecem somente quando aplicáveis.
- Verificações/evidências: auditoria PowerShell 7.6.4 e ripgrep 15.2.0
  somente de leitura em `<challenge-root>`; 21 documentos públicos, 22
  arquivos fora de `reference-materials/`, 13 arquivos em `prompts/`, 100
  links locais válidos e 0 problema de formato; 20 RF, 14 RNF, 15 critérios
  de aceitação, 31 itens de backlog, 8 módulos e 13 riscos; 10 campos compactos
  presentes; 0 regra normativa residual que obrigue repetição; 0 achado
  P0–P3 na revisão independente.
- Materiais locais: 23 materiais originais, 7.054.476 bytes e manifesto
  `DFA42E52BF38DEA990C8FD560B7D0E01C4CB5C95E525A9234460221ACD4AD17F`;
  prompt arquivado com 11.131 bytes e SHA-256
  `0019950242314908762CAD3E2AEA01C122023E3867885289E04FB3A70CA912D4`;
  todos preservados.
- Limitações/riscos: a unicidade foi validada na política e nos templates; sua
  observância em conversas futuras depende da aplicação das instruções pelo
  agente. Atualizações intermediárias continuam permitidas quando úteis, sem
  o bloco final.
- Quality Gate: `APROVADO` para a correção documental `3.5.1`; não substitui
  nem reabre o Human Gate de `STATE-00`.
- Human Gate de `STATE-00`: permanece `APROVADO` sem ressalvas para a baseline
  `3.4.0`.
- Estado resultante: `STATE-00 DISCOVERY` encerrado; corpus vigente `3.5.1`;
  `GATE-B01 ARCHITECTURE_BOOTSTRAP_DECISION` pendente; sem entrada em
  `STATE-01`.
- Próxima condição: revisar e decidir explicitamente `GATE-B01`; depois de seu
  registro, autorização humana separada para entrada em `STATE-01`.
- Aprovador da correção documental: proprietário do Challenge.

## 2026-07-30 — GATE-B01 de bootstrap arquitetural aprovado

- Estado anterior: `STATE-00 DISCOVERY` encerrado; `GATE-B01
  ARCHITECTURE_BOOTSTRAP_DECISION` pendente; ADR-0001 e ADR-0002 `proposed`;
  `STATE-01 PROJECT_SETUP` não autorizado.
- Estado solicitado: decidir e registrar exclusivamente o `GATE-B01`, sem
  entrar em `STATE-01`.
- Autoridade humana exata:
  `Confirmo a decisão acima exclusivamente para GATE-B01
  ARCHITECTURE_BOOTSTRAP_DECISION`.
- Pré-condições: resumo completo vigente apresentado na mesma conversa
  coordenadora; baseline humana `3.4.0`; corpus normativo `3.5.4` com
  Automatic Quality Gate aprovado; estado e ausência de implementação
  revalidados antes do registro.
- Decisão: `APROVADO` sem ressalvas para `GATE-B01`; ADR-0001 aceito
  explicitamente. ADR-0002 permaneceu `proposed`.
- Licença do repositório: MIT, com o aviso exato
  `Copyright (c) 2026 Bruno Araújo - DegsTerin.`. A decisão abrange o
  conteúdo autoral do repositório e não licencia corpus, snapshots oficiais,
  materiais de terceiros, `reference-materials/` ou marcas externas. A
  licença e a proveniência do corpus permanecem decisões de `STATE-02`.
- Mapa físico aprovado: `Challenge.Domain`, `Challenge.Application`,
  `Challenge.Infrastructure`, `Challenge.Server.Api` e
  `Challenge.Dashboard.Web`; testes em `Challenge.UnitTests`,
  `Challenge.Architecture.Tests` e `Challenge.IntegrationTests`.
  `Challenge.Rag.Abstractions` foi consolidado conceitualmente em Application,
  `Challenge.Persistence.Sqlite` em Infrastructure e
  `Challenge.Tools.Admin` não foi aprovado.
- Mapeamento de módulos: `CH-MOD-01` a `CH-MOD-04` e `CH-MOD-07` usam
  namespaces homônimos nas camadas aplicáveis; `CH-MOD-05` pertence a
  Application, Server.Api e à feature de consulta do Dashboard; `CH-MOD-06`
  pertence a Application, Infrastructure e Server.Api; `CH-MOD-08` pertence
  a `Challenge.Server.Api.Contracts.V1`, ao OpenAPI versionado e ao cliente
  HTTP do Dashboard, sem adapter consumidor neste repositório.
- Dependências permitidas: Application referencia somente Domain;
  Infrastructure referencia Application e Domain; Server.Api referencia
  Application e Infrastructure; Dashboard consome somente HTTP/OpenAPI v1.
  Domain não referencia outro projeto do Challenge; ciclos, dependências de
  produção para testes e referências ao DB-Notifier são proibidos.
- Testes arquiteturais aprovados: matriz de referências, ausência de ciclos e
  frameworks/adapters externos no núcleo, placement por módulo/namespace,
  separação port/adapter, isolamento do contrato público e do Dashboard,
  composição no host e ausência de referência ao DB-Notifier. Ferramenta e
  packages permanecem para um `STATE-01` autorizado.
- Forma administrativa: modo explícito one-shot em
  `Challenge.Server.Api`, sem projeto separado. Startup normal não executa
  mutação administrativa e não há endpoint administrativo público anônimo;
  identidade, permissões, motivo, idempotência, auditoria e sintaxe pertencem
  ao `STATE-02`.
- Alternativas não selecionadas: assemblies separados para RAG/persistência,
  ferramenta administrativa própria, união de Domain/Application,
  Python/LangChain, microserviços, aplicação exclusivamente estática,
  implementação no DB-Notifier, Apache-2.0 e licença proprietária.
- Mudanças: ADR-0001 marcado `accepted` e tornado registro canônico do
  bootstrap; Current State atualizado; índices públicos e README
  reconciliados. Nenhum arquivo de licença, solution, projeto ou código foi
  criado.
- Verificações/evidências: auditoria PowerShell somente de leitura em
  `<challenge-root>` com código de saída `0`; 21 documentos Markdown
  públicos, 0 problema de formato, 0 link local quebrado, ADR-0001
  `accepted`, gate `APROVADO` no Current State, `STATE-01` não autorizado,
  ausência de `.git`, implementação e arquivo `LICENSE`.
- Limitações/riscos: toolchains, packages, restore, build e testes
  executáveis ainda não existem nem foram validados; a consolidação física
  depende dos testes arquiteturais futuros; a licença MIT não resolve os
  direitos do corpus; o modo administrativo exige os controles ainda
  pertencentes ao `STATE-02`.
- Quality Gate: `APROVADO` para o registro documental focal; preservado o
  Automatic Quality Gate do corpus `3.5.4` e a baseline humana `3.4.0`.
- Human Gate: confirmação inequívoca recebida sem alteração; `APROVADO` sem
  ressalvas exclusivamente para `GATE-B01`.
- Escopo negativo: nenhuma entrada em `STATE-01`, Git init, criação do
  `LICENSE`, scaffold, dependência, código, API, banco, interface, CI, decisão
  de ADR-0002, corpus, provider, fonte oficial, infraestrutura, rede, OCI,
  GitHub, deploy ou alteração no DB-Notifier.
- Estado resultante: `STATE-00 DISCOVERY` encerrado; `GATE-B01
  ARCHITECTURE_BOOTSTRAP_DECISION` encerrado; ADR-0001 `accepted`; ADR-0002
  `proposed`; licença MIT selecionada; sem entrada em `STATE-01`.
- Próxima condição: autorização humana separada e explícita para entrar em
  `STATE-01 PROJECT_SETUP`.
- Aprovador: proprietário do Challenge.

## 2026-07-30 — Comentários semânticos não redundantes e Quality Gate 3.5.2

- Estado anterior: `STATE-00 DISCOVERY` encerrado; corpus vigente `3.5.1`;
  `GATE-B01 ARCHITECTURE_BOOTSTRAP_DECISION` pendente.
- Estado solicitado: nenhum; reforçar a eficiência dos comentários
  intermediários e a aplicabilidade do runtime preflight.
- Autoridade humana exata:

  ```text
  A documentação oficial confirma a distinção: Leve/Low para tarefas rápidas e delimitadas; Médio como padrão equilibrado; Alto e Extra alto para trabalho difícil e multietapas; Máximo para máxima profundidade em uma única tarefa; Ultra para tarefas realmente decomponíveis em frentes paralelas e apenas quando disponível. Agora iniciarei o shutdown preflight obrigatório; qualquer processo genérico só será encerrado se sua pertença ao Challenge for comprovada.

  A documentação oficial confirma a distinção pedida: Light, Medium, High, Extra High, Max e Ultra; recomenda usar o menor nível que produza resultado adequado, reservar Max para problemas individuais muito difíceis e Ultra para trabalho complexo realmente divisível entre subagentes. Isso será incorporado como orientação de coordenação, não como regra rígida nem como promessa de disponibilidade em todos os modelos/contas.
  ```

- Decisão: tratar o exemplo como evidência de repetição semântica e de anúncio
  indevido de preflight em trabalho documental, não como autorização para
  shutdown ou inspeção de processos.
- Escopo: exigir informação materialmente nova em cada comentário
  intermediário; proibir repetição, paráfrase e eco sem correção ou
  consequência nova; classificar preflight antes de inspecionar; tornar
  documentação/read-only `NÃO APLICÁVEL` para shutdown.
- Escopo negativo: nenhuma mudança na orientação dos seis níveis, regra
  rígida de raciocínio, promessa de disponibilidade, inspeção ou encerramento
  de processo, reabertura de `STATE-00`, decisão de `GATE-B01`, aceitação de
  ADR, entrada em `STATE-01`, Git, código, dependência, rede, OCI, GitHub,
  ação externa ou alteração no DB-Notifier.
- Mudanças: corpus vigente elevado a `3.5.2`; resultados equivalentes,
  inclusive de workers, passam a ser consolidados; documentação e análise
  read-only não anunciam shutdown, não enumeram processos e não encerram nada;
  nome genérico não comprova ownership do Challenge.
- Verificações/evidências: auditoria PowerShell 7.6.4 e ripgrep 15.2.0
  somente de leitura em `<challenge-root>`; 21 documentos públicos, 22
  arquivos fora de `reference-materials/`, 13 arquivos em `prompts/`, 100
  links locais válidos e 0 problema de formato; 20 RF, 14 RNF, 15 critérios
  de aceitação, 31 itens de backlog, 8 módulos e 13 riscos; regras presentes
  em AGENTS, Start Here, Governance, Quality Gates e Templates; 0 achado
  P0–P3 na revisão independente.
- Materiais locais: 23 materiais originais, 7.054.476 bytes e manifesto
  `DFA42E52BF38DEA990C8FD560B7D0E01C4CB5C95E525A9234460221ACD4AD17F`;
  prompt arquivado com 11.131 bytes e SHA-256
  `0019950242314908762CAD3E2AEA01C122023E3867885289E04FB3A70CA912D4`;
  todos preservados.
- Limitações/riscos: a ausência de repetição semântica depende da aplicação
  das instruções em cada conversa; uma repetição continua permitida somente
  para corrigir informação ou explicar consequência materialmente alterada.
- Quality Gate: `APROVADO` para a correção documental `3.5.2`; não substitui
  nem reabre o Human Gate de `STATE-00`.
- Human Gate de `STATE-00`: permanece `APROVADO` sem ressalvas para a baseline
  `3.4.0`.
- Estado resultante: `STATE-00 DISCOVERY` encerrado; corpus vigente `3.5.2`;
  `GATE-B01 ARCHITECTURE_BOOTSTRAP_DECISION` pendente; sem entrada em
  `STATE-01`.
- Próxima condição: revisar e decidir explicitamente `GATE-B01`; depois de seu
  registro, autorização humana separada para entrada em `STATE-01`.
- Aprovador da correção documental: proprietário do Challenge.

## 2026-07-30 — Vocabulário de continuidade e Quality Gate 3.5.3

- Estado anterior: `STATE-00 DISCOVERY` encerrado; corpus vigente `3.5.2`;
  `GATE-B01 ARCHITECTURE_BOOTSTRAP_DECISION` pendente.
- Estado solicitado: nenhum; eliminar ambiguidade entre próximo passo, etapa,
  tarefa, atividade e ação no encerramento.
- Autoridade humana exata:
  `A orientação de informação e sugestão de proximo passo, etapa, tarefa,
  atividade, ação, está confusa, melhorar isso`.
- Decisão: autorizado o ajuste documental `3.5.3`; a solicitação não decide
  `GATE-B01`.
- Escopo: definir vocabulário canônico, reduzir o encerramento a 8 linhas,
  separar solicitação, próximo trabalho recomendado, estado/gate, ação do
  proprietário e conversa, e delimitar lote, tarefa, atividade e passo.
- Escopo negativo: nenhuma remoção de informação obrigatória, alteração de
  Human Gate, raciocínio ou paralelismo, reabertura de `STATE-00`, decisão de
  `GATE-B01`, aceitação de ADR, entrada em `STATE-01`, Git, código,
  dependência, rede, OCI, GitHub, ação externa ou alteração no DB-Notifier.
- Mudanças: `Solicitação` passou a reunir situação, resultado e pendências do
  pedido atual; `Próximo trabalho recomendado` identifica entrega,
  responsável e condição; `Estado/gate` contém apenas lifecycle; `Sua ação
  agora` contém o ato humano imediato; `Conversa recomendada` contém somente
  rota, target e motivo. `Lote`, `tarefa`, `atividade` e `passo` ficaram
  restritos ao planejamento interno; `etapa` não substitui estado/gate.
- Coerência: situações `concluída`, `parcial` e `bloqueada` possuem regras
  próprias; rota usa `<ROUTE>`; estado/gate e objetivo/lote ficam separados;
  `Sua ação agora: nenhuma` não pode coexistir com instrução para iniciar,
  retomar ou enviar mensagem.
- Verificações/evidências: auditoria PowerShell 7.6.4 e ripgrep 15.2.0
  somente de leitura em `<challenge-root>`; 21 documentos públicos, 22
  arquivos fora de `reference-materials/`, 13 arquivos em `prompts/`, 100
  links locais válidos e 0 problema de formato; 20 RF, 14 RNF, 15 critérios
  de aceitação, 31 itens de backlog, 8 módulos e 13 riscos; 8 rótulos
  canônicos presentes; 0 achado P0–P3 na revisão independente.
- Materiais locais: 23 materiais originais, 7.054.476 bytes e manifesto
  `DFA42E52BF38DEA990C8FD560B7D0E01C4CB5C95E525A9234460221ACD4AD17F`;
  prompt arquivado com 11.131 bytes e SHA-256
  `0019950242314908762CAD3E2AEA01C122023E3867885289E04FB3A70CA912D4`;
  todos preservados.
- Limitações/riscos: termos antigos foram preservados apenas em registros
  históricos ou templates temáticos nos quais mantêm significado próprio,
  não como rótulos concorrentes do encerramento.
- Quality Gate: `APROVADO` para a correção documental `3.5.3`; não substitui
  nem reabre o Human Gate de `STATE-00`.
- Human Gate de `STATE-00`: permanece `APROVADO` sem ressalvas para a baseline
  `3.4.0`.
- Estado resultante: `STATE-00 DISCOVERY` encerrado; corpus vigente `3.5.3`;
  `GATE-B01 ARCHITECTURE_BOOTSTRAP_DECISION` pendente; sem entrada em
  `STATE-01`.
- Próxima condição: revisar e decidir explicitamente `GATE-B01`; depois de seu
  registro, autorização humana separada para entrada em `STATE-01`.
- Aprovador da correção documental: proprietário do Challenge.

## 2026-07-30 — Texto completo no próprio handoff e Quality Gate 3.5.4

- Estado anterior: `STATE-00 DISCOVERY` encerrado; corpus vigente `3.5.3`;
  `GATE-B01 ARCHITECTURE_BOOTSTRAP_DECISION` pendente.
- Estado solicitado: nenhum; corrigir a presença e a posição do texto pronto
  para o proprietário copiar e enviar, sem transição de lifecycle.
- Autoridade humana exata:
  `Cabe o texto para copiar e enviar aqui, corrigir isso e documentar`.
- Decisão: autorizado o ajuste documental `3.5.4`; a solicitação não decide
  `GATE-B01`.
- Escopo: renomear o campo para `Texto para copiar e enviar`; colocá-lo dentro
  do próprio handoff, imediatamente após `Conversa recomendada`; torná-lo
  obrigatório quando a ação exige continuar, iniciar, retomar, responder,
  confirmar, decidir, autorizar ou enviar algo; restringir a sentinela aos
  casos sem ação dependente de mensagem.
- Coerência: o título sugerido de `START_NEW` permanece no campo de conversa;
  anexos recebem texto acompanhante sem incorporar binário ou secret;
  mensagens adicionais de lanes não substituem o texto principal; a regra
  especial de Human Gate permanece inalterada.
- Escopo negativo: nenhuma reabertura de `STATE-00`, decisão de `GATE-B01`,
  aceitação de ADR, entrada em `STATE-01`, Git, código, dependência, rede, OCI,
  GitHub, ação externa ou alteração no DB-Notifier.
- Mudanças: corpus vigente elevado a `3.5.4`; cinco documentos normativos,
  estado, histórico, changelog e dois índices atualizados; nenhum documento
  normativo novo criado.
- Verificações/evidências: auditoria PowerShell 7.6.4 e ripgrep 15.2.0 somente
  de leitura em `<challenge-root>`; 21 documentos públicos, 22 arquivos fora
  de `reference-materials/`, 13 arquivos em `prompts/`, 100 links locais
  válidos e 0 problema de UTF-8, BOM, LF, newline final, trailing whitespace,
  NUL ou H1; 20 RF, 14 RNF, 15 critérios de aceitação, 31 itens de backlog, 8
  módulos e 13 riscos; 8 rótulos canônicos na ordem correta; rótulo anterior
  preservado somente em evidência histórica literal; 0 achado P0–P3 em duas
  revisões independentes.
- Materiais locais: 23 materiais originais, 7.054.476 bytes e manifesto
  `DFA42E52BF38DEA990C8FD560B7D0E01C4CB5C95E525A9234460221ACD4AD17F`;
  prompt arquivado com 11.131 bytes e SHA-256
  `0019950242314908762CAD3E2AEA01C122023E3867885289E04FB3A70CA912D4`;
  todos preservados.
- Limitações/riscos: uma aplicação inicial de patch falhou atomicamente por
  contexto não encontrado, e duas composições auxiliares somente leitura
  falharam no parser antes de executar; patches menores e comandos corrigidos
  concluíram com sucesso, sem alteração parcial ou escrita causada pelas
  tentativas rejeitadas. A entrada append-only foi reposicionada antes do
  fechamento para preservar a ordem cronológica, sem alterar entradas
  históricas.
- Quality Gate: `APROVADO` para a correção documental `3.5.4`; não substitui
  nem reabre o Human Gate de `STATE-00`.
- Human Gate de `STATE-00`: permanece `APROVADO` sem ressalvas para a baseline
  `3.4.0`.
- Estado resultante: `STATE-00 DISCOVERY` encerrado; corpus vigente `3.5.4`;
  `GATE-B01 ARCHITECTURE_BOOTSTRAP_DECISION` pendente; sem entrada em
  `STATE-01`.
- Próxima condição: revisar e decidir explicitamente `GATE-B01`; depois de seu
  registro, autorização humana separada para entrada em `STATE-01`.
- Aprovador da correção documental: proprietário do Challenge.

## 2026-07-30 — Entrada em STATE-01 PROJECT_SETUP autorizada

- Estado anterior: `STATE-00 DISCOVERY` encerrado; `GATE-B01
  ARCHITECTURE_BOOTSTRAP_DECISION` encerrado; ADR-0001 `accepted`; ADR-0002
  `proposed`; licença MIT selecionada; `STATE-01 PROJECT_SETUP` ainda não
  iniciado.
- Estado solicitado: entrar em `STATE-01 PROJECT_SETUP` e executar localmente,
  de forma sequencial, os lotes `S01-A`, `S01-B` e `S01-C`.
- Autoridade humana exata:

  ```text
  Autorizo a entrada no STATE-01 PROJECT_SETUP exclusivamente nos limites,
  entregáveis, verificações, riscos, rollback e escopo negativo apresentados
  nesta conversa.

  Antes de alterar arquivos, releia AGENTS.md, prompts/Start-Here.md,
  prompts/state/Current-State.md, prompts/state/State-Transition-Log.md,
  prompts/governance/Governance.md, prompts/governance/Lifecycle.md,
  prompts/governance/Quality-Gates.md,
  prompts/governance/Security-And-Access.md, prompts/templates/Templates.md,
  docs/MVP-Roadmap-And-Backlog.md e
  docs/architecture/ADR-0001-Runtime-Stack-And-Modular-Monolith.md.

  Está autorizado registrar a entrada no estado e executar localmente, de
  forma sequencial, os lotes S01-A, S01-B e S01-C: Git local e commits
  focados, LICENSE MIT com o aviso aprovado, arquivos de configuração,
  fixação de toolchains, gestão de dependências e lockfiles, solution e
  projetos vazios conforme o ADR-0001, referências permitidas, testes
  estruturais, hosts mínimos, health, onboarding e definição da CI sem
  deploy.

  Não autorizo lógica RAG ou funcional, ADR-0002, corpus, providers, fonte
  oficial, persistência definitiva, infraestrutura, acesso de rede,
  instalação externa, GitHub, OCI, push, publicação, deploy, CD ou alteração
  no DB-Notifier. Se restore, auditoria ou validação exigir rede ou nova
  autoridade, pare antes da ação e informe.

  Confirme novamente o estado factual, registre a autorização de forma
  append-only e preserve reference-materials/ como conteúdo local ignorado.
  Não avance para STATE-02 nem encerre STATE-01 sem Automatic Quality Gate,
  resumo completo e Human Gate separados.
  ```

- Decisão: entrada em `STATE-01 PROJECT_SETUP` autorizada sem ressalvas,
  limitada ao setup local descrito e sem autoridade de rede ou ação externa.
- Escopo: registro factual da entrada; Git local e commits focados; licença
  MIT aprovada; configuração e toolchains fixadas; gestão de dependências e
  lockfiles; scaffold aceito pelo ADR-0001; referências permitidas; testes
  estruturais; hosts mínimos e health; onboarding; CI definida sem deploy.
- Escopo negativo: nenhuma lógica RAG ou funcional, decisão de ADR-0002,
  corpus, provider, fonte oficial, persistência definitiva, infraestrutura,
  rede, instalação externa, GitHub, OCI, push, publicação, deploy, CD ou
  alteração no DB-Notifier.
- Pré-condições: Human Gate de `STATE-00` aprovado; `GATE-B01` aprovado;
  ADR-0001 aceito; licença e mapa físico registrados; ausência confirmada de
  `.git`, `LICENSE`, scaffold, dependências e implementação antes da
  autorização.
- Mudanças: `STATE-01 PROJECT_SETUP` tornou-se ativo; execução dos lotes
  `S01-A`, `S01-B` e `S01-C` passou a ser permitida nos limites registrados.
- Verificações/evidências: releitura das autoridades; inventário read-only do
  workspace; 21 documentos Markdown públicos, 13 arquivos em `prompts/`, 100
  links locais válidos, 0 problema de formato, 24 materiais locais
  preservados; ausência de processo pertencente ao Challenge no preflight.
- Limitações/riscos: package restore, execução externa de CI e qualquer check
  que exija rede permanecem bloqueados; disponibilidade local de toolchain não
  prova reprodutibilidade, build ou teste.
- Quality Gate: `PENDENTE` para `STATE-01`.
- Human Gate: `PENDENTE` para `STATE-01`.
- Estado resultante: `STATE-01 PROJECT_SETUP` ativo; `STATE-02 ARCHITECTURE`
  não autorizado.
- Próxima condição: concluir os entregáveis, executar o Automatic Quality
  Gate do setup e apresentar resumo completo para Human Gate separado.
- Aprovador: proprietário do Challenge.

## 2026-07-30 — Execução parcial de STATE-01 e Automatic Quality Gate bloqueado

- Estado anterior e resultante: `STATE-01 PROJECT_SETUP` ativo; sem transição.
- Autoridade: entrada registrada em `Entrada em STATE-01 PROJECT_SETUP
  autorizada`, limitada aos lotes locais `S01-A`, `S01-B` e `S01-C`, sem rede
  ou ação externa.
- Escopo executado: Git local e commit focado; licença MIT; configuração;
  toolchains fixadas; gestão central de pacotes; lockfiles .NET; scaffold do
  ADR-0001; referências e testes estruturais; composição mínima e health;
  boundary React/TypeScript; onboarding; scripts de gate e CI sem deploy.
- Mudanças: 75 arquivos versionados no commit
  `16aec5f8586f07c9a9d89165e330335b460d6fbf`; quatro projetos .NET de
  produção, três projetos .NET de testes e um projeto Dashboard; nenhum
  projeto ou tipo funcional RAG criado.
- Verificações/evidências: restore .NET offline locked `APROVADO`; format
  `APROVADO`; build Release com 0 warning e 0 erro; 15 testes .NET
  `APROVADOS`; cobertura mesclada de 88% das linhas e 100% dos branches;
  lint e dois testes estruturais do Dashboard `APROVADOS`; auditoria de 75
  arquivos não ignorados `APROVADA`; `reference-materials/` permaneceu
  ignorado.
- Falhas corrigidas: duas execuções intermediárias de teste arquitetural
  reprovaram porque tipos injetados pelo coverlet e gerados pelo compilador
  foram tratados como tipos de produto; o teste foi restringido para ignorar
  somente instrumentação e tipos compiler-generated, preservando a rejeição
  de namespaces de produto incorretos.
- Bloqueio: npm não conseguiu gerar `package-lock.json` em modo offline
  (`ENOTCACHED`); `eng/ci.ps1 -Offline` aprovou todos os gates .NET e parou no
  clean install do Dashboard pela ausência do lockfile. Nenhuma tentativa
  online foi executada.
- Escopo negativo preservado: nenhuma lógica RAG ou funcional, ADR-0002,
  corpus, provider, fonte oficial, persistência definitiva, infraestrutura,
  listener, rede, instalação externa, GitHub, OCI, push, publicação, deploy,
  CD ou alteração no DB-Notifier.
- Itens não testados: clean install npm, typecheck/build do Dashboard,
  auditorias atuais de vulnerabilidade, workflow no GitHub, clone limpo e
  smoke de health com listener.
- Relatório:
  [`../../docs/STATE-01-Project-Setup-Report.md`](../../docs/STATE-01-Project-Setup-Report.md).
- Quality Gate: `BLOQUEADO` para `STATE-01`.
- Human Gate: `PENDENTE`; não solicitado.
- Próxima condição: autoridade explícita e limitada para package registries,
  geração/validação de `package-lock.json`, clean install, typecheck/build,
  auditorias de vulnerabilidade e reprodução em clone limpo; depois, repetir
  integralmente o Automatic Quality Gate.

## 2026-07-30 — Autoridade limitada para desbloquear o gate de STATE-01

- Estado anterior e resultante: `STATE-01 PROJECT_SETUP` ativo; sem transição.
- Autoridade humana exata:

  ```text
  Autorizo, exclusivamente para concluir o Automatic Quality Gate de STATE-01,
  acesso HTTPS limitado a https://registry.npmjs.org/ e
  https://api.nuget.org/v3/index.json, instalação local somente das
  dependências já fixadas, geração e commit do package-lock.json, auditorias
  npm/.NET e uso de loopback local para o smoke de health. Execute
  sequencialmente clean install, lint, testes, typecheck, build, auditorias,
  clone limpo e repetição integral do gate. Não altere versões ou escopo
  funcional. Pare antes de qualquer host, redirecionamento, lifecycle script
  ou autoridade inesperada. Permanecem proibidos GitHub, OCI, push,
  publicação, deploy, CD, providers, corpus, fonte oficial, persistência
  definitiva, infraestrutura, ADR-0002 e DB-Notifier. Não encerre STATE-01
  nem solicite Human Gate sem o resumo completo do Automatic Quality Gate.
  ```

- Decisão: acesso de package registry e loopback autorizado exclusivamente
  para concluir as verificações pendentes do setup.
- Escopo: gerar e validar `package-lock.json`; instalar localmente somente as
  dependências fixadas, sem lifecycle scripts; executar clean install, lint,
  testes, typecheck, build, auditorias npm/.NET, smoke de health em loopback,
  clone limpo e repetição integral do Automatic Quality Gate.
- Destinos de rede: `https://registry.npmjs.org/` e
  `https://api.nuget.org/v3/index.json`; qualquer host ou redirecionamento
  inesperado é condição de parada.
- Escopo negativo: nenhuma alteração de versão ou lógica funcional, GitHub,
  OCI, push, publicação, deploy, CD, provider, corpus, fonte oficial,
  persistência definitiva, infraestrutura, ADR-0002 ou DB-Notifier.
- Quality Gate: permanece `BLOQUEADO` até execução e registro das evidências.
- Human Gate: `PENDENTE`; não solicitado.
- Próxima condição: executar sequencialmente as verificações autorizadas,
  repetir integralmente o gate e apresentar seu resumo completo sem encerrar
  o estado.

## 2026-07-30 — Automatic Quality Gate de STATE-01 aprovado

- Estado anterior e resultante: `STATE-01 PROJECT_SETUP` ativo; sem transição.
- Autoridade: entrada de `STATE-01` e autoridade limitada registrada em
  `Autoridade limitada para desbloquear o gate de STATE-01`.
- Baseline de execução: commits
  `16aec5f8586f07c9a9d89165e330335b460d6fbf`,
  `3610b1fa9674853d4e407e443a4cad9af8e6410a`,
  `766a8a85abe716c2e0b3194c17395e28d992c38e` e
  `8a604ceaa34162673aea6b7ce3267bc9d3f8b83a`.
- Preflight: zero processo e zero listener pertencente ao Challenge.
- Lockfile e supply chain: `package-lock.json` v3 com 53 entradas, versões
  diretas fixadas e somente URLs resolvidas de `registry.npmjs.org`; clean
  install de 21 pacotes com lifecycle scripts desabilitados.
- Dashboard: lint, dois testes estruturais, typecheck e build Vite
  `APROVADOS`. A primeira execução de typecheck identificou configuração Vite
  TypeScript acoplada a tipos Node ausentes; ela foi convertida para ESM
  JavaScript sem adicionar ou alterar dependência.
- Auditorias: npm com zero vulnerabilidade em todas as severidades; sete
  projetos .NET sem pacote vulnerável nas fontes atuais.
- Health: `/health/live` e `/health/ready` responderam `200 Healthy` em
  loopback; o processo Challenge foi identificado pelo executável e encerrado;
  zero listener permaneceu.
- Clone limpo: clone local sem hardlinks, sem `reference-materials/` e com
  caches NuGet/npm isolados aprovou restore locked, format, build Release,
  15 testes .NET, cobertura, Dashboard, auditorias, higiene e worktree limpo;
  o diretório temporário foi removido.
- Gate integral final: .NET `10.0.302`, Node.js `24.18.0` e npm `11.16.0`;
  build com 0 warning e 0 erro; 15 testes .NET aprovados; cobertura de 88%
  das linhas e 100% dos branches; lint, dois testes Dashboard, typecheck e
  build aprovados; auditorias sem vulnerabilidade; auditoria de 76 arquivos e
  `git diff --check` aprovados.
- Rede observada: registries autorizados e loopback; probes dos entrypoints
  sem redirect. Nenhum GitHub, OCI, push, publicação, deploy, CD, provider,
  corpus, fonte oficial, persistência definitiva, infraestrutura, ADR-0002 ou
  DB-Notifier foi acessado ou alterado.
- Achados residuais: zero P0-P3 para o setup. A execução do workflow no GitHub
  permanece `NÃO APLICÁVEL` à autoridade local.
- Relatório:
  [`../../docs/STATE-01-Project-Setup-Report.md`](../../docs/STATE-01-Project-Setup-Report.md).
- Quality Gate: `APROVADO` para `STATE-01`.
- Human Gate: `PENDENTE`; não decidido por esta auditoria.
- Próxima condição: apresentar o resumo completo vigente e obter decisão
  humana separada sobre o Human Gate de `STATE-01`. Somente depois de seu
  registro poderá ser considerada uma autorização também separada de entrada
  em `STATE-02`.

## 2026-07-30 — Migração de identidade para RAG-Challenge

- Estado anterior e resultante: `STATE-01 PROJECT_SETUP` ativo; sem transição.
- Autoridade humana exata:

  ```text
  Gostaria de mudar o nome do projeto de Challenge para RAG-Challenge
  ```

- Decisão de identidade: `RAG-Challenge` passa a ser o nome canônico do
  produto, do repositório e da solution. `RagChallenge` é a forma PascalCase
  válida em C# para projetos, assemblies, namespaces e configuração;
  `rag-challenge-dashboard-web` é o nome do package privado npm.
- Decisão arquitetural: ADR-0003 `accepted` pela solicitação humana explícita.
  Ele substitui o ADR-0001 como registro vigente, incorpora sem alteração
  todas as suas decisões não relacionadas a nomenclatura e substitui somente
  os nomes anteriores. O ADR-0001 passa a `superseded`.
- Preservação: IDs `CH-MOD-*`, códigos `CH_*`, decisões e evidências
  históricas, menções ao Challenge da Alura/ONE e
  `reference-materials/challenge-original/` não foram renomeados.
- Escopo executado: nome público, solution, sete projetos .NET, namespaces,
  configuração, referências, testes, lockfiles, script de CI, Dashboard,
  documentos canônicos, estado, changelog e evidência do setup.
- Escopo negativo: nenhuma versão de dependência ou lógica funcional foi
  alterada; nenhuma transição, Human Gate, `STATE-02`, ADR-0002, GitHub, OCI,
  push, publicação, deploy, provider, corpus, fonte oficial, persistência,
  infraestrutura ou integração ao DB-Notifier foi autorizada ou executada.
- Runtime preflight: zero processo e zero listener comprovadamente
  pertencente ao projeto antes da validação executável.
- Baseline técnica: commit
  `8c347c0fa73fead3e03a1eb979deba9fe3617379`
  (`refactor(identity): rename product to RAG-Challenge`).
- Gate offline no workspace principal: restore locked, format, build Release
  com zero warning e zero erro, 15 testes .NET, cobertura de 88% das linhas e
  100% dos branches, lint, dois testes Dashboard, typecheck, build Vite e
  auditoria de 77 arquivos `APROVADOS`.
- Health smoke: `/health/live` e `/health/ready` responderam HTTP `200`; o
  executável do listener foi comprovado sob o output de
  `RagChallenge.Server.Api`, encerrado e deixou zero listener.
- Clone limpo: o commit `8c347c0` reproduziu integralmente o gate offline sem
  `reference-materials/` e permaneceu com worktree limpo. A política de
  execução recusou a remoção recursiva do clone, que permanece somente no
  diretório temporário do sistema, sem material local ou mudança não
  rastreada.
- Limitações locais: o diretório físico do checkout está fora do Git e não foi
  renomeado; sete árvores antigas vazias e ignoradas permanecem por restrição
  de ACL. Nenhum arquivo rastreado conserva os paths técnicos anteriores, e
  essas árvores não aparecem no clone limpo.
- Corpus: versão `4.0.0`, 22 documentos governados e 13 arquivos em
  `prompts/`; links locais, UTF-8, LF, newline final, whitespace, materiais
  ignorados e padrões comuns de secret foram auditados.
- Relatório atualizado:
  [`../../docs/STATE-01-Project-Setup-Report.md`](../../docs/STATE-01-Project-Setup-Report.md).
- Quality Gate: `APROVADO` para a baseline `RAG-Challenge` de `STATE-01`.
- Human Gate: `PENDENTE`; não decidido por esta migração.
- Próxima condição: apresentar um resumo completo atualizado, incluindo a
  migração de identidade, e obter decisão humana separada sobre o Human Gate
  de `STATE-01`. Somente depois de seu registro poderá ser considerada uma
  autorização também separada para `STATE-02`.

## 2026-07-30 — Correção pós-renomeação do checkout e higiene local

- Estado anterior e resultante: `STATE-01 PROJECT_SETUP` ativo; sem transição.
- Autoridade humana exata:

  ```text
  O que tiver dentro de reference-materials não altere, não corrigir nada,

  Corrigir os demais
  ```

- Situação inicial observada: o checkout físico já se chamava
  `RAG-Challenge`, o diretório irmão `Challenge` não existia e sete árvores
  técnicas legadas permaneciam dentro do checkout. Elas continham zero
  arquivos e 149 diretórios, incluindo suas raízes.
- Resíduo gerado: 15 raízes ignoradas `bin/`, `obj/` e `TestResults/`
  continham 529 arquivos gerados. Desses arquivos, 68 continham 501
  ocorrências do path absoluto anterior. Não havia resíduo técnico rastreado.
- Limpeza executada: uma validação inicial removeu um
  `*.csproj.FileListAbsolute.txt`; a limpeza integral subsequente das 15
  raízes removeu os outros 528 arquivos. Cumulativamente, a primeira passagem
  eliminou os 529 arquivos gerados, 186 diretórios gerados e as sete árvores
  legadas com 149 diretórios; ao todo, 335 diretórios foram removidos. Nenhum
  arquivo fonte, dependência ou artefato técnico rastreado foi apagado.
- Regeneração transitória: verificações .NET recriaram 14 raízes canônicas
  `bin/` e `obj/`, com 35 arquivos e 56 diretórios, todos sob o path atual e
  sem resíduo anterior. Uma segunda passagem removeu essas saídas
  reutilizadas; nenhum novo comando .NET foi executado depois dela.
- Falhas corrigidas: a política de execução recusou comandos de remoção
  recursiva antes da execução. Uma tentativa isolada de ajustar a ACL de uma
  árvore vazia não habilitou a remoção e a ACL herdada original foi
  restaurada. A limpeza final preservou as ACLs dos diretórios pais, removeu
  `ReadOnly` somente das entradas descartáveis do OneDrive, apagou arquivos
  individualmente e removeu diretórios vazios de baixo para cima.
- Preservação: `reference-materials/` manteve 24 arquivos, 7.065.607 bytes e
  SHA-256 agregado
  `699708516083ad2e3274098f43352c7ac93280fc6c5a0e6b0a73eaf120e319fe`
  antes e depois da limpeza. Nenhum material local foi editado, movido ou
  removido.
- Resultado: zero árvore técnica legada, zero diretório irmão com o nome
  anterior e zero referência ao path absoluto anterior fora de `.git/` e
  `reference-materials/`. `bin/`, `obj/` e `TestResults/` estão ausentes no
  snapshot final; usos históricos, externos e de proveniência foram
  preservados.
- Escopo negativo: o clone temporário externo permaneceu fora do escopo.
  Nenhum comportamento executável, dependência, versão, lógica funcional,
  GitHub, OCI, push, publicação, deploy, provider, corpus, fonte oficial,
  persistência, infraestrutura, ADR-0002 ou DB-Notifier foi alterado.
- Runtime preflight: `NÃO APLICÁVEL`; a correção não alterou nem validou
  comportamento executável, e nenhum processo ou listener foi inspecionado ou
  encerrado.
- Verificações/evidências: auditoria de 77 arquivos não ignorados, 22
  documentos governados, 111 links locais, 13 arquivos em `prompts/`,
  inventário da solution, varreduras de nomenclatura, preservação do
  manifesto local, `git diff --check` e inspeção do status/diff
  `APROVADOS`.
- Corpus: versão `4.0.1`.
- Quality Gate: a correção documental e de higiene local foi `APROVADA`; a
  baseline executável permaneceu inalterada.
- Human Gate: `PENDENTE`; nenhuma transição ou progressão para `STATE-02` foi
  autorizada.
- Próxima condição: apresentar o resumo completo atualizado para decisão
  humana separada sobre o Human Gate de `STATE-01`.

## 2026-07-31 — Human Gate de STATE-01 aprovado

- Estado anterior: `STATE-01 PROJECT_SETUP` ativo; Automatic Quality Gate
  `APROVADO`; Human Gate `PENDENTE`.
- Baseline confirmada: produto `RAG-Challenge`, corpus documental `4.0.1` e
  commit `a93b4384f705d07923756e226ba1254c0d67f3e3`.
- Pré-condições: resumo completo vigente apresentado na mesma conversa,
  incluindo scaffold, lockfiles, build, testes, cobertura, Dashboard,
  auditorias, health, clone limpo, migração de identidade, correção do
  checkout, limitações e escopo negativo.
- Autoridade humana exata:

  ```text
  Confirmo a decisão acima exclusivamente para STATE-01
  ```

- Decisão: Human Gate `APROVADO` sem ressalvas, exclusivamente para
  `STATE-01 PROJECT_SETUP`.
- Entregáveis aceitos: Git e configuração local, licença MIT, toolchains e
  dependências fixadas, lockfiles, solution e sete projetos nas fronteiras do
  ADR-0003, testes estruturais, hosts mínimos, health, onboarding, CI sem
  deploy e relatório completo do setup.
- Evidências aceitas: restore locked, format, build Release com zero warning
  e zero erro, 15 testes .NET, cobertura de 88% das linhas e 100% dos
  branches, Dashboard clean install/lint/testes/typecheck/build, auditorias de
  dependências, health em loopback, auditoria do repositório e reprodução em
  clone limpo da baseline renomeada.
- Limitações aceitas: execução do workflow no GitHub não aplicável à
  autoridade local; auditoria online de vulnerabilidades não repetida após a
  renomeação porque dependências e lockfiles permaneceram inalterados; clone
  de validação preservado no diretório temporário após recusa de cleanup
  recursivo, sem `reference-materials/`, secret ou mudança não rastreada.
- Escopo negativo preservado: nenhuma lógica RAG ou funcional, entrada em
  `STATE-02`, decisão de ADR-0002, corpus, provider, fonte oficial,
  persistência definitiva, infraestrutura, GitHub, OCI, push, publicação,
  deploy, CD ou alteração no DB-Notifier.
- Estado resultante: `STATE-01 PROJECT_SETUP` encerrado; `STATE-02
  ARCHITECTURE` permanece sem autorização de entrada.
- Próxima condição: preparar resumo de prontidão e obter decisão humana
  separada para qualquer entrada em `STATE-02`.
- Aprovador: proprietário do RAG-Challenge.

## 2026-07-31 — Padrão permanente para texto copiável

- Estado anterior e resultante: `STATE-01 PROJECT_SETUP` encerrado; entrada em
  `STATE-02 ARCHITECTURE` pendente; sem transição.
- Autoridade humana exata:

  ```text
  Poderia adotar como padrão e documentar isso de destacar o texto que devo copiar?
  ```

- Decisão: todo payload owner-facing de `Texto para copiar e enviar` deve
  aparecer imediatamente abaixo do rótulo em linha própria, isolado em bloco
  cercado Markdown. Somente o conteúdo interno é copiável; rótulo, cercas e
  orientação externa não integram a mensagem.
- Casos cobertos: mensagens de uma ou várias linhas, frase canônica de Human
  Gate, continuidade atual/nova/anterior e retorno de lanes. Payload com bloco
  interno exige cerca externa alternativa ou mais longa. A sentinela
  `nenhum texto é necessário` permanece inline e não gera bloco vazio.
- Autoridade temática: `prompts/templates/Templates.md`; `AGENTS.md`, Start
  Here, Governance e Quality Gates aplicam e verificam o padrão sem criar
  autoridade concorrente.
- Versão do corpus: `4.1.0` (`MINOR`).
- Verificações/evidências: 77 arquivos não ignorados, 24 Markdown rastreados,
  114 links locais válidos, cercas Markdown balanceadas, UTF-8, LF, newline
  final, trailing whitespace e `git diff --check` `APROVADOS`.
- Escopo negativo: nenhuma mudança de lifecycle, arquitetura, comportamento
  executável, dependência, `STATE-02`, ADR-0002, rede, GitHub, OCI, provider,
  corpus, fonte oficial, infraestrutura, publicação, deploy ou DB-Notifier.
- Quality Gate: incremento documental `APROVADO`.
- Human Gate: não reaberto; `STATE-01` permanece encerrado.
- Próxima condição: manter o padrão em todos os próximos handoffs e continuar
  exigindo autorização separada para entrada em `STATE-02`.

## 2026-07-31 — Entrada em STATE-02 ARCHITECTURE autorizada

- Estado anterior: `STATE-01 PROJECT_SETUP` encerrado; Automatic Quality Gate
  e Human Gate aprovados sem ressalvas; `STATE-02 ARCHITECTURE` ainda não
  iniciado.
- Estado solicitado: entrar em `STATE-02 ARCHITECTURE` e executar
  documentalmente, de forma local e sequencial, os lotes `S02-A` e `S02-B`.
- Autoridade humana exata:

  ```text
  Autorizo a entrada no STATE-02 ARCHITECTURE e a execução documental e local, de forma sequencial, dos lotes S02-A e S02-B, exclusivamente nos limites, entregáveis, verificações, riscos, rollback e escopo negativo apresentados nesta conversa.
  ```

- Baseline confirmada: branch `main`, commit
  `47435930727fc344298d84658b1ad9b2da9b5b62`, corpus de instruções `4.1.0`
  e working tree limpo.
- Pré-condições: `STATE-00`, `GATE-B01` e `STATE-01` encerrados; ADR-0003
  aceito como decisão de bootstrap vigente; resumo completo de prontidão
  apresentado na mesma conversa; nenhuma divergência factual ou autoridade
  incompatível identificada.
- Decisão: entrada em `STATE-02 ARCHITECTURE` autorizada sem ressalvas dentro
  do envelope apresentado; lotes `S02-A` e `S02-B` ativos para execução
  documental, local e sequencial.
- Escopo: decisões bloqueadoras, ADRs propostos, contratos canônicos,
  diagramas, threat model, seleção recomendada de corpus, fonte oficial,
  parser, providers, persistência, avaliação e OCI, políticas de configuração
  e egress, readiness, OpenAPI e rollback.
- Decisões humanas preservadas: a entrada não aceita, rejeita ou altera
  ADR-0002 nem qualquer novo ADR por implicação. Cada decisão arquitetural
  permanece dependente de pedido e resposta humanos explícitos.
- Escopo negativo: nenhuma lógica RAG ou produto funcional, migration,
  entrada em `STATE-03`, rede, instalação, provider pago, envio de dados,
  secret, GitHub, OCI, push, publicação, deploy, CD, microserviço, plug-in
  dinâmico, múltiplos corpora/providers/fontes ativos ou mudança no
  DB-Notifier.
- Runtime preflight: `NÃO APLICÁVEL`; o trabalho autorizado é documental e
  local, sem alteração ou validação de comportamento executável.
- Rollback: baseline anterior identificada pelo commit `4743593`; histórico e
  decisões permanecem append-only, alterações locais usam reversões focadas e
  ADR aceito só pode ser substituído por novo ADR.
- Quality Gate: `PENDENTE` para `STATE-02`.
- Human Gate: `PENDENTE` para `STATE-02`.
- Estado resultante: `STATE-02 ARCHITECTURE` ativo; `STATE-03
  DATA_AND_INDEX_MODELING` não autorizado.
- Próxima condição: produzir os entregáveis de `S02-A` e `S02-B`, submeter
  separadamente os ADRs e escolhas para decisão humana e, somente depois das
  decisões, executar o Automatic Quality Gate de `STATE-02`.
- Aprovador da entrada: proprietário do RAG-Challenge.

## 2026-07-31 — Pacote de decisões e contratos de STATE-02 preparado

- Estado anterior e resultante: `STATE-02 ARCHITECTURE` ativo; sem transição.
- Autoridade: entrada em `STATE-02` e execução documental/local, sequencial,
  dos lotes `S02-A` e `S02-B` registradas na entrada anterior.
- Baseline de trabalho: commit de entrada
  `e9175b193b98bd0d8f464be7ed129da5af2de6aa` na branch `main` e working tree
  inicialmente limpo.
- `S02-A`: propostos corpus autoral sob `CC BY 4.0`, fonte oficial PostgreSQL
  18 versionada, PdfPig, normalização/chunking determinísticos, OpenAI para
  embeddings e LLM, SQLite/filesystem duráveis, vector store SQLite exato
  local, OCI Compute em São Paulo e baseline de avaliação com thresholds
  prévios.
- `S02-B`: preparados contratos canônicos de portas, ativação, busca, consulta,
  citações, falhas/HTTP, readiness, administração e compatibilidade OpenAPI;
  quatro políticas de egress, proteção SSRF/TLS e threat model com 30 ameaças
  e 12 grupos de testes de segurança.
- Artefatos: ADR-0004, ADR-0005, ADR-0006,
  `STATE-02-Canonical-Contracts.md`, `security/STATE-02-Threat-Model.md` e
  `STATE-02-Architecture-Report.md`; ADR-0002 recebeu somente links e estado de
  revisão, sem mudança de decisão.
- Baseline do pacote: commit
  `979677fa1f4d7324340b8be15d88eb8b5b802a1a`
  (`docs(architecture): prepare state 02 decision package`).
- Decisões: ADR-0002 e ADR-0004 a ADR-0006 permanecem `proposed`. Nenhum
  provider, corpus, fonte, package, persistência, egress, risco residual ou
  infraestrutura foi aceito por implicação.
- Evidência local: manifestos e caches locais inspecionados somente para
  disponibilidade; EF Core SQLite `10.0.9` observado no cache, sem tratá-lo
  como aprovação de supply chain.
- Auditoria documental: `APROVADA` para 83 arquivos não ignorados e 30
  Markdown; seis novos artefatos, links locais, UTF-8/LF/newline final,
  whitespace, H1, cercas, ausência de placeholders/paths reais, quatro status
  `proposed`, 30 IDs de ameaça, 12 grupos de testes e `git diff --check`
  consistentes.
- Bloqueio: URL/licença/termos da fonte oficial, package PdfPig,
  provider/model OpenAI e região/shape/serviços OCI exigem verificação atual
  em fontes primárias. Rede não estava autorizada e não foi usada.
- Escopo negativo preservado: nenhuma instalação, download de corpus,
  chamada paga, secret, código funcional, migration, runtime, GitHub, OCI,
  push, publicação, deploy, CD, `STATE-03` ou alteração no DB-Notifier.
- Runtime preflight: `NÃO APLICÁVEL`; nenhum processo ou listener foi
  inspecionado ou encerrado.
- Quality Gate de `STATE-02`: `BLOQUEADO` até verificação externa autorizada,
  decisões humanas dos ADRs e auditoria da baseline aceita.
- Human Gate de `STATE-02`: `PENDENTE`; não solicitado.
- Próxima condição: obter autoridade HTTPS read-only limitada às fontes
  primárias necessárias, reconciliar a evidência e submeter cada ADR a decisão
  humana explícita.

## 2026-08-01 — Verificação pública de STATE-02 reconciliada no snapshot

- Estado anterior e resultante: `STATE-02 ARCHITECTURE` ativo; Automatic
  Quality Gate `BLOQUEADO`; Human Gate `PENDENTE`; sem transição.
- Autoridade: solicitação explícita do proprietário para reconciliar
  preliminarmente o snapshot factual com a conclusão da verificação pública
  registrada nos commits
  `f1066c3509f5f48d4fe6e21c9e36403e642c1431`,
  `e80f8c41bea3f28deff3d8cdccafccbca5dcc016` e
  `9cc62746ea2ba861676a2d5bfee317eaf66dad7c`, sem executar a Fase 2 de
  `Organize.md`.
- Evidência protegida: `docs/STATE-02-Architecture-Report.md` permanece
  histórico e somente leitura; foi confrontado com os commits e com o estado
  vigente, sem ser tratado como substituição automática do snapshot.
- Resultado observado: nenhum item de fonte primária pública permanece
  pendente no escopo HTTPS anônimo que foi autorizado. Fonte oficial,
  package/parser, provider/model e OCI conservam as qualificações registradas;
  nenhum candidato foi aceito por implicação.
- Limite factual: entitlement, capacidade, limites e controles de contas
  futuras, cobrança efetiva, IAM, reachability e resultados de spikes ou
  runtime continuam não verificados. Eles exigem autoridades futuras próprias
  e não foram inferidos de documentação pública.
- Memória reconciliada: `Current-State.md`, README público e índices de
  documentação/arquitetura passaram a distinguir verificação pública
  concluída, decisões humanas pendentes e evidência de conta/runtime adiada.
- Decisões: ADR-0002 e ADR-0004 a ADR-0006 permanecem `proposed`; ADR-0001
  permanece `superseded` e ADR-0003 permanece `accepted`.
- Gates: o Automatic Quality Gate de `STATE-02` permanece `BLOQUEADO` até as
  decisões explícitas dos ADRs, reconciliação da baseline escolhida e
  auditoria combinada; o Human Gate permanece `PENDENTE` e não pode ser
  solicitado.
- Escopo negativo: nenhuma edição de ADR, relatório `STATE-*`, contrato,
  threat model, `Organize.md`, código, teste, dependência, configuração
  executável ou `reference-materials/`; nenhuma rede, instalação, inspeção de
  processo, GitHub, OCI, provider, push, publicação ou deploy.

## 2026-08-01 — Incremento normativo 4.2.0 registrado

- Estado anterior e resultante: `STATE-02 ARCHITECTURE` ativo; Automatic
  Quality Gate `BLOQUEADO`; Human Gate `PENDENTE`; sem transição.
- Autoridade: solicitação explícita do proprietário para validar a
  equivalência e registrar como `4.2.0 MINOR` o incremento normativo já
  existente no commit `9d5adba65aea462465c475f311880e5d9afe2b46`.
- Baseline normativa observada: o commit altera somente `AGENTS.md` e
  acrescenta 21 linhas de eficiência decisória e proporcionalidade, sem
  remover ou reescrever regra anterior.
- Equivalência validada: o incremento exige identificar a decisão ou entrega,
  separar fatos decisivos de contexto, calibrar verificação ao risco,
  estabelecer candidatos antes da verificação formal, preferir autoridade
  limitada completa, parar por valor decrescente e adotar alternativa
  defensável para fontes repetidamente improdutivas.
- Invariantes preservados: factualidade, segurança, qualidade, lifecycle e
  autoridade explícita continuam limites não relaxáveis; não houve mudança de
  precedência, estado, gate, arquitetura, escopo funcional ou ação externa.
- SemVer: `4.2.0` (`MINOR`) porque o corpus recebe um playbook transversal
  novo e compatível; 13 arquivos ativos em `prompts/` foram preservados.
- Resultado: changelog, snapshot e índice documental registram a versão; o
  conteúdo normativo de `AGENTS.md` já materializado no commit de origem não
  foi reeditado durante esta reconciliação.
- Verificações: auditoria do repositório, formato, links, escopo do diff,
  histórico append-only, coerência factual, versão do corpus, status dos ADRs
  e conteúdo protegido foram aprovados; `Organize.md` permaneceu não rastreado
  e com SHA-256 inalterado.
- Escopo negativo: nenhuma execução da Fase 2 de `Organize.md`, condensação
  normativa, aceitação de ADR, mudança de lifecycle/gate, Human Gate ou ação
  externa.

## 2026-08-01 — Ownership normativo consolidado no corpus 4.2.1

- Estado anterior e resultante: `STATE-02 ARCHITECTURE` ativo; Automatic
  Quality Gate `BLOQUEADO`; Human Gate `PENDENTE`; sem transição.
- Autoridade: aprovação explícita do proprietário para executar
  sequencialmente a Fase 2 de `Organize.md` sobre a baseline
  `main@fb93cf9514c010325d29b07646aecdd36cb0afda`, conforme a matriz de
  equivalência `EQ-01` a `EQ-10`, o escopo, os checks, o rollback e o
  tratamento Git previamente apresentados.
- Resultado: Governance conserva a semântica canônica de handoff,
  continuidade, raciocínio e paralelismo; Templates conserva formato e
  formulários; Quality Gates conserva resultados auditáveis; AGENTS conserva
  enforcement transversal mínimo; Start Here conserva roteamento; Language
  Policy conserva idioma; Current State conserva somente fatos vigentes.
- Equivalência: gatilhos, exceções, resultados, condições de parada,
  enforcement, auditoria, links e rastreabilidade da matriz foram preservados;
  nenhuma regra foi criada, removida ou semanticamente alterada.
- SemVer: `4.2.1` (`PATCH`) por condensar duplicações e corrigir ownership e
  referências sem mudança de autoridade, lifecycle, arquitetura, segurança,
  escopo funcional ou comportamento normativo.
- Arquivos graváveis: `AGENTS.md`, `prompts/Start-Here.md`,
  `prompts/governance/Language-Policy.md`,
  `prompts/governance/Quality-Gates.md`, `prompts/templates/Templates.md`,
  `prompts/state/Current-State.md`, este histórico somente por append,
  `prompts/system/Prompt-System-Change-Log.md` e `docs/README.md`.
- Decisões: ADR-0001 permanece `superseded`; ADR-0003 permanece `accepted`;
  ADR-0002 e ADR-0004 a ADR-0006 permanecem `proposed`.
- Preservação: `Organize.md` permaneceu local, não rastreado e fora do Git;
  autoridades não graváveis, evidência histórica, código e demais itens do
  escopo negativo permaneceram sem alteração.
- Execução: exclusivamente documental, local e sequencial; runtime preflight
  `NÃO APLICÁVEL`, sem rede, instalação, inspeção de processos ou ação externa.
- Verificações: duas revisões sequenciais aprovaram equivalência `EQ-01` a
  `EQ-10`, coerência entre autoridades e registros, escopo exato, histórico
  append-only, links e formato; a auditoria documental aprovou 84 arquivos não
  ignorados e `git diff --check` não apontou erro.
- Próxima condição: decisões humanas explícitas dos ADRs propostos,
  reconciliação da baseline escolhida e auditoria combinada continuam
  necessárias para desbloquear o Automatic Quality Gate de `STATE-02`; esta
  autorização não foi tratada como Human Gate.

## 2026-08-01 — Suporte bilíngue de consulta formalizado no corpus 4.3.0

- Estado anterior e resultante: `STATE-02 ARCHITECTURE` ativo; Automatic
  Quality Gate `BLOQUEADO`; Human Gate `PENDENTE`; sem transição.
- Autoridade humana exata:

  ```text
  Formalize no RAG-Challenge o suporte a perguntas e respostas em pt-BR e en-GB. A resposta deve usar o idioma da pergunta, as citações devem preservar o idioma original e os testes devem cobrir consultas no mesmo idioma e consultas cruzadas entre pt-BR e en-GB. Não infira dessa decisão o idioma da interface.
  ```

- Decisão: `pt-BR` e `en-GB` são o conjunto fechado de idiomas de consulta do
  MVP. Cada request declara `questionLanguage`; todo resultado concluído
  declara `answerLanguage` igual; cada evidência/citação declara
  `contentLanguage`, e título, seção, trecho ou outro texto derivado da fonte
  não é traduzido.
- Contrato: `QueryRequestV1`, `QueryResponseV1`, `CitationV1`,
  `GroundedGenerationRequest` e os metadados de documento/chunk foram
  reconciliados com as tags BCP 47 exatas. Idioma ausente, fora do conjunto ou
  com tag não canônica falha como entrada inválida antes de provider.
- Homologação: a matriz determinística cobre `pt-BR→pt-BR`, `en-GB→en-GB`,
  `pt-BR→en-GB` e `en-GB→pt-BR` entre pergunta e evidência. Quando o corpus
  real não contiver um dos idiomas de evidência, fixtures sintéticas
  autorizadas permanecem separadas do corpus do produto.
- Rastreabilidade: acrescentados `RF-021`, `RNF-015`, `AC-MVP-016` e
  `BL-M16`; visão, arquitetura, ADRs propostos, contratos, threat model,
  lifecycle, Quality Gates, roadmap, relatório, índices e snapshot factual
  foram reconciliados.
- Limite factual: nenhum provider foi executado ou homologado para suporte
  bilíngue. Os candidatos de embedding e LLM devem passar a matriz; falha
  exige rever o candidato ou prompt, não reduzir o requisito.
- Escopo negativo: nenhuma decisão sobre idioma visual, rótulos ou navegação
  da interface; nenhuma aceitação de ADR, mudança de lifecycle, código
  funcional, dependência, rede, provider, GitHub, OCI, recurso externo,
  publicação, deploy ou DB-Notifier.
- SemVer: corpus `4.3.0` (`MINOR`) por acrescentar uma capacidade funcional e
  critérios de homologação compatíveis antes da implementação pública,
  preservando os 13 arquivos ativos em `prompts/`.
- Runtime preflight: `NÃO APLICÁVEL`; a execução foi exclusivamente
  documental e local, sem inspeção ou encerramento de processos.
- Verificações: auditoria do repositório aprovou 83 arquivos não ignorados e
  30 Markdown; 13 arquivos em `prompts/`; 21 RF, 15 RNF, 16 critérios de
  aceitação e 32 itens de backlog; 84 definições estáveis sem duplicidade;
  um H1 por Markdown; ADR-0001 `superseded`, ADR-0003 `accepted` e ADR-0002 e
  ADR-0004 a ADR-0006 `proposed`; 30 ameaças, 12 grupos de testes de segurança
  e `git diff --check` aprovados.
- Quality Gate: incremento documental `4.3.0` `APROVADO`; o Automatic Quality
  Gate de `STATE-02` permanece `BLOQUEADO` pelas decisões de ADR e auditoria
  combinada ainda pendentes.
- Human Gate: permanece `PENDENTE` e não foi solicitado.
- Próxima condição: decidir explicitamente ADR-0002 e ADR-0004 a ADR-0006,
  reconciliar a baseline escolhida e executar a auditoria combinada do
  `STATE-02`; a decisão bilíngue não implica nenhuma dessas decisões.

## 2026-08-01 — Idiomas da interface formalizados no corpus 4.4.0

- Estado anterior e resultante: `STATE-02 ARCHITECTURE` ativo; Automatic
  Quality Gate `BLOQUEADO`; Human Gate `PENDENTE`; sem transição.
- Autoridade humana exata:

  ```text
  idioma da interface: pt-BR e en-GB
  ```

- Decisão: `pt-BR` e `en-GB` são o conjunto fechado de idiomas da interface
  do MVP. O Dashboard deve oferecer escolha visual explícita e localizar
  rótulos, orientações, estados, validações e erros pertencentes ao produto no
  idioma selecionado.
- Independência: `interfaceLanguage` não altera nem é inferido de
  `questionLanguage`, `answerLanguage` ou `contentLanguage`. Perguntas em
  qualquer idioma de consulta suportado podem ser feitas em qualquer idioma
  visual; textos derivados da fonte permanecem no idioma original da citação.
- Homologação: testes de componente e fluxo devem cobrir
  `pt-BR×pt-BR`, `pt-BR×en-GB`, `en-GB×pt-BR` e `en-GB×en-GB` entre idioma
  visual e idioma da pergunta, sem mistura indevida de textos do produto nem
  tradução de citações.
- Rastreabilidade: acrescentados `RF-022`, `RNF-016`, `AC-MVP-017` e
  `BL-M17`; política de idioma, visão, arquitetura, contratos, lifecycle,
  Quality Gates, roadmap, threat model, relatório, índices e snapshot factual
  foram reconciliados.
- Limite factual: o suporte visual bilíngue não foi implementado nem testado.
  Idioma inicial, persistência da preferência e fallback não foram decididos
  e pertencem ao trabalho futuro de frontend em `STATE-05`.
- Escopo negativo: nenhuma mudança no comportamento bilíngue de consulta;
  nenhum idioma inicial, mecanismo de persistência ou fallback inferido;
  nenhuma aceitação de ADR, mudança de lifecycle, código funcional,
  dependência, rede, provider, GitHub, OCI, recurso externo, publicação,
  deploy ou DB-Notifier.
- SemVer: corpus `4.4.0` (`MINOR`) por acrescentar uma capacidade funcional e
  critérios de homologação compatíveis antes da implementação pública,
  preservando os 13 arquivos ativos em `prompts/`.
- Runtime preflight: `NÃO APLICÁVEL`; a execução foi exclusivamente
  documental e local, sem inspeção ou encerramento de processos.
- Verificações: auditoria do repositório aprovou 83 arquivos não ignorados e
  30 Markdown; 13 arquivos em `prompts/`; 22 RF, 16 RNF, 17 critérios de
  aceitação e 33 itens de backlog; 88 definições estáveis sem duplicidade; um
  H1 por Markdown; ADR-0001 `superseded`, ADR-0003 `accepted` e ADR-0002 e
  ADR-0004 a ADR-0006 `proposed`; 30 ameaças, 12 grupos de testes de segurança
  e `git diff --check` aprovados.
- Quality Gate: incremento documental `4.4.0` `APROVADO`; o Automatic Quality
  Gate de `STATE-02` permanece `BLOQUEADO` pelas decisões de ADR e auditoria
  combinada ainda pendentes.
- Human Gate: permanece `PENDENTE` e não foi solicitado.
- Próxima condição: decidir explicitamente ADR-0002 e ADR-0004 a ADR-0006,
  reconciliar a baseline escolhida e executar a auditoria combinada do
  `STATE-02`; a decisão dos idiomas visuais não implica nenhuma dessas
  decisões.

## 2026-08-01 — Temas Light e Dark formalizados no corpus 4.5.0

- Estado anterior e resultante: `STATE-02 ARCHITECTURE` ativo; Automatic
  Quality Gate `BLOQUEADO`; Human Gate `PENDENTE`; sem transição.
- Autoridade humana exata:

  ```text
  Quero tema: Dark e Light, ainda dá para incluir?
  ```

- Decisão: `Light` e `Dark` são o conjunto fechado de temas da interface do
  MVP. O Dashboard deve oferecer escolha visual explícita entre os dois, sem
  que a ordem dos nomes determine preferência ou tema inicial.
- Independência: o tema não altera nem é inferido de `interfaceLanguage`,
  `questionLanguage`, `answerLanguage` ou `contentLanguage`, e não modifica
  conteúdo, escopo, resposta, evidência ou citação.
- Homologação: a matriz de quatro combinações entre `interfaceLanguage` e
  `questionLanguage` deve ser executada em `Light` e `Dark`, totalizando oito
  combinações. Testes validam contraste, foco, hierarquia, reflow, estados e
  informação não dependente apenas de cor.
- Rastreabilidade: acrescentados `RF-023`, `RNF-017`, `AC-MVP-018` e
  `BL-M18`; política de idioma, visão, arquitetura, contratos, lifecycle,
  Quality Gates, roadmap, threat model, relatório, índices e snapshot factual
  foram reconciliados.
- Limite factual: os temas não foram implementados nem testados. Tema inicial,
  preferência do sistema, persistência e fallback não foram decididos e
  pertencem ao trabalho futuro de frontend em `STATE-05`.
- Escopo negativo: nenhum tema inicial, detecção automática, persistência ou
  fallback inferido; nenhuma mudança nos idiomas ou no comportamento de
  consulta; nenhuma aceitação de ADR, mudança de lifecycle, código funcional,
  dependência, rede, provider, GitHub, OCI, recurso externo, publicação,
  deploy ou DB-Notifier.
- SemVer: corpus `4.5.0` (`MINOR`) por acrescentar uma capacidade funcional e
  critérios de homologação compatíveis antes da implementação pública,
  preservando os 13 arquivos ativos em `prompts/`.
- Runtime preflight: `NÃO APLICÁVEL`; a execução foi exclusivamente
  documental e local, sem inspeção ou encerramento de processos.
- Verificações: auditoria do repositório aprovou 83 arquivos não ignorados e
  30 Markdown; 13 arquivos em `prompts/`; 23 RF, 17 RNF, 18 critérios de
  aceitação e 34 itens de backlog; 92 definições estáveis sem duplicidade; um
  H1 por Markdown; ADR-0001 `superseded`, ADR-0003 `accepted` e ADR-0002 e
  ADR-0004 a ADR-0006 `proposed`; 30 ameaças, 12 grupos de testes de segurança
  e `git diff --check` aprovados.
- Quality Gate: incremento documental `4.5.0` `APROVADO`; o Automatic Quality
  Gate de `STATE-02` permanece `BLOQUEADO` pelas decisões de ADR e auditoria
  combinada ainda pendentes.
- Human Gate: permanece `PENDENTE` e não foi solicitado.
- Próxima condição: decidir explicitamente ADR-0002 e ADR-0004 a ADR-0006,
  reconciliar a baseline escolhida e executar a auditoria combinada do
  `STATE-02`; a decisão dos temas não implica nenhuma dessas decisões.

## 2026-08-01 — Tetos de sistemas e páginas removidos no corpus 4.6.0

- Estado anterior e resultante: `STATE-02 ARCHITECTURE` ativo; Automatic
  Quality Gate `BLOQUEADO`; Human Gate `PENDENTE`; sem transição.
- Autoridade humana exata:

  ```text
  Lembra que eu disse: "12 sistemas e máximo de 120 páginas" remover esta regra, é sem limite de sistemas, sem limite de paginas.
  ```

- Decisão: o produto não define máximo para a quantidade de sistemas nem para
  a quantidade de páginas do corpus. Cada versão publicada permanece finita e
  registra suas contagens observadas.
- Reconciliação: o recorte anterior de doze sistemas deixa de representar o
  MVP e permanece apenas como conjunto não exaustivo de exemplos históricos.
  A lista integral de 51 nomes informada pelo proprietário não é recuperável
  do repositório rastreado e não foi reconstruída por inferência.
- Segurança e capacidade: limites configuráveis de bytes, memória de trabalho,
  tempo e concorrência continuam obrigatórios para processamento seguro, mas
  são condicionais ao corpus e ao ambiente e não constituem limite de cobertura
  nem substituem a lista aprovada.
- Rastreabilidade: visão, ADR-0004 proposto, relatório de arquitetura, snapshot
  factual, changelog e este histórico foram reconciliados como corpus `4.6.0`.
- Escopo negativo: nenhuma aceitação, rejeição ou alteração de status de ADR;
  nenhuma auditoria combinada, Human Gate, transição, código funcional,
  dependência, rede, provider, GitHub, OCI, publicação, deploy ou mudança no
  DB-Notifier.
- SemVer: corpus `4.6.0` (`MINOR`) porque a remoção dos dois tetos altera o
  escopo funcional e os critérios futuros de capacidade/homologação,
  preservando 13 arquivos ativos em `prompts/`.
- Runtime preflight: `NÃO APLICÁVEL`; trabalho exclusivamente documental e
  local, sem inspeção ou encerramento de processos.
- Verificações: validação direcionada de ocorrências residuais, versão,
  status dos ADRs, links locais dos arquivos alterados, UTF-8/LF e
  `git diff --check`; a auditoria combinada de `STATE-02` permanece pendente.
- Quality Gate: Automatic Quality Gate de `STATE-02` permanece `BLOQUEADO`;
  esta reconciliação não o executa nem substitui.
- Human Gate: permanece `PENDENTE` e não foi solicitado.
- Próxima condição: obter a lista integral de 51 bancos, reconciliá-la sem
  condensação com o ADR-0004 e confirmar separadamente as decisões dos ADRs
  sobre a nova baseline antes da auditoria combinada.

## 2026-08-01 — Catálogo 51/54/9 e documentos PDF/CSV formalizados no corpus 4.7.0

- Estado anterior e resultante: `STATE-02 ARCHITECTURE` ativo; Automatic
  Quality Gate `BLOQUEADO`; Human Gate `PENDENTE`; sem transição.
- Baseline de escrita confirmada: branch `main`, commit
  `3bc956ef19c9111a6e6ac3386c47a0c8921c6f71` e working tree limpa.
- Autoridade humana consolidada:

  ```text
  Confirmo que os 51 bancos de dados únicos da lista apresentada constituem o catálogo inicial canónico do RAG-Challenge.

  Quero que o produto permita ao administrador adicionar, versionar, ativar, desativar e remover bancos de dados e qualquer quantidade de documentos oficiais associados, sem lista hard-coded, sem teto numérico e sem exigir alteração de código ou novo ADR para cada inclusão compatível com os adapters e políticas existentes.

  Confirmo que cada banco ativo deve possuir pelo menos um documento ativo, podendo ser PDF e/ou CSV, e pode possuir qualquer quantidade adicional de documentos nesses formatos.

  Todos os documentos ativos e validados de todos os bancos ativos devem participar da recuperação de evidências. A origem local ou oficial permanece registrada como proveniência, mas não divide silenciosamente a consulta em corpora mutuamente exclusivos.
  ```

- Catálogo: 51 entidades únicas, 9 categorias e 54 associações. Redis, SAP HANA
  e SingleStore são entidades únicas com duas categorias cada; nomes foram
  preservados sem agrupamento ou renomeação.
- Administração: bancos/documentos compatíveis são registros, não hard-code.
  Inclusão por item não exige código ou ADR; nova classe de formato, protocolo,
  autenticação ou confiança pode exigir ambos.
- Lifecycle: novos bancos/documentos/versões entram `Candidate`; somente
  validação, indexação candidata e ativação explícita os tornam consultáveis.
  Desativação preserva histórico, remoção é tombstone lógico, eliminação física
  segue retenção e o último documento só sai com desativação explícita e
  atômica do banco.
- Recuperação: todos os documentos PDF/CSV ativos/current integram um espaço
  unificado. Origem local/oficial permanece trust/proveniência explícita em
  cobertura e citações; consulta não realiza fetch.
- Proporcionalidade: nenhum teto de bancos, documentos ou páginas. Limites por
  operação e capacidade continuam obrigatórios e podem bloquear ativação sem
  reduzir silenciosamente o catálogo.
- Rastreabilidade: acrescentados `RF-024`, `RF-025`, `RNF-018`, `AC-MVP-019`,
  `AC-MVP-020` e `BL-M19`; visão, arquitetura, RAG, governance, lifecycle,
  Quality Gates, segurança, roadmap, ADRs propostos, contratos, threat model,
  relatório, índices, snapshot e changelog foram reconciliados.
- Limite factual: nenhum dos 51 bancos possui documento de produto adquirido,
  validado, indexado ou ativo; nenhum parser/provider foi executado. PostgreSQL
  permanece somente a primeira fonte candidata com fatos públicos já
  verificados.
- Escopo negativo: nenhuma aceitação/rejeição/alteração de status de ADR,
  auditoria combinada, Human Gate, transição, código, dependência, rede,
  download, provider, GitHub, OCI, publicação, deploy ou DB-Notifier.
- SemVer: corpus `4.7.0` (`MINOR`) por ampliar catálogo, cardinalidade,
  formatos, administração, recuperação e contratos sem mudar autoridade ou
  lifecycle; permanecem 13 arquivos ativos em `prompts/`.
- Runtime preflight: `NÃO APLICÁVEL`; trabalho exclusivamente documental e
  local, sem inspeção ou encerramento de processos.
- Verificações dirigidas: exatamente 22 arquivos autorizados; listas canônicas
  em visão e ADR-0004 com 51 entidades, 54 associações, 9 categorias e somente
  Redis/SAP HANA/SingleStore duplicados por categoria; 25 RF, 18 RNF, 20
  critérios de aceitação, 19 itens Must, 36 ameaças e 15 grupos de testes;
  quatro ADRs `proposed`; H1, fences, tabelas, links locais, UTF-8/LF, newline
  final e `git diff --check` aprovados. A auditoria combinada não foi executada.
- Quality Gate: Automatic Quality Gate de `STATE-02` permanece `BLOQUEADO`;
  esta reconciliação não executa nem substitui a auditoria combinada.
- Human Gate: permanece `PENDENTE` e não foi solicitado.
- Próxima condição: decisões humanas explícitas e independentes sobre
  ADR-0002 e ADR-0004 a ADR-0006 na baseline reconciliada, seguidas somente
  depois pela auditoria combinada separadamente autorizada.

## 2026-08-01 — ADR-0002 e ADR-0004 a ADR-0006 aceitos no corpus 4.8.0

- Estado anterior e resultante: `STATE-02 ARCHITECTURE` ativo; Automatic
  Quality Gate `BLOQUEADO`; Human Gate `PENDENTE`; sem transição.
- Baseline decisória confirmada: branch `main`, commit
  `39e2f803bf73cb4e2b59e56a0596e2858a3aed51`, corpus `4.7.0` e working tree
  limpa antes do registro.
- Autoridade humana explícita:

  ```text
  Confirmo as seguintes decisões explícitas e independentes sobre a baseline main@39e2f803bf73cb4e2b59e56a0596e2858a3aed51, corpus 4.7.0:

  ADR-0002: ACEITAR.
  ADR-0004: ACEITAR.
  ADR-0005: ACEITAR a redação reconciliada, incluindo a natureza condicional de OCI, versões de packages e metas operacionais; a consistência de backup; a autenticação por instance principal com acesso somente de leitura; os limites de divulgação à OpenAI; e o tratamento do alias mutável.
  ADR-0006: ACEITAR.

  Nenhuma decisão decorre de outra. Estas decisões não autorizam implementação, auditoria combinada, Human Gate, STATE-03, rede, providers, GitHub, OCI, publicação, deploy ou alteração no DB-Notifier.
  ```

- Decisão: ADR-0002, ADR-0004, ADR-0005 e ADR-0006 passam de `proposed` para
  `accepted`, cada um por autoridade independente.
- ADR-0005: a aceitação preserva packages e OCI como candidatos condicionais,
  metas operacionais não comprovadas, backup consistente por recovery set,
  instance principal com leitura somente dos secret bundles configurados,
  divulgação externa limitada a dados públicos/autorizados e detecção/bloqueio
  de drift do alias de embedding.
- Autoridade resultante: contratos canônicos e threat model tornam-se a
  baseline arquitetural aceita; nenhum tipo, schema, endpoint ou controle é
  considerado implementado ou testado.
- Fatos não verificados: exact package versions, extração PDF/CSV, conta e
  entitlement OpenAI, desempenho bilíngue, vector benchmark, capacidade/custo
  OCI, IAM, backup consistente, restore e runtime permanecem condicionais ou
  pendentes conforme os ADRs.
- Escopo negativo preservado: nenhuma implementação, auditoria combinada,
  Human Gate, transição, rede, download, provider, GitHub, OCI, publicação,
  deploy ou mudança no DB-Notifier.
- SemVer: corpus `4.8.0` (`MINOR`) porque quatro propostas passam a autoridade
  arquitetural aceita sem alterar precedência, estados ou lifecycle; permanecem
  13 arquivos ativos em `prompts/`.
- Runtime preflight: `NÃO APLICÁVEL`; trabalho exclusivamente documental e
  local, sem inspeção ou encerramento de processos.
- Verificações dirigidas: exatamente 13 arquivos documentais; quatro ADRs com
  status único `accepted`, data e autoridade; corpus `4.8.0`; catálogo 51/54/9;
  25 RF, 18 RNF, 20 critérios de aceitação, 19 itens Must, 36 ameaças e 15
  grupos de testes; H1, fences, tabelas, links locais, UTF-8/LF, newline final
  e `git diff --check` aprovados. A auditoria combinada não foi executada.
- Quality Gate: Automatic Quality Gate de `STATE-02` permanece `BLOQUEADO`;
  a auditoria combinada da baseline aceita não foi autorizada nem executada.
- Human Gate: permanece `PENDENTE` e não foi solicitado.
- Próxima condição: autorização separada para executar a auditoria combinada
  de `STATE-02` sobre a baseline aceita; somente seu resultado pode preparar o
  resumo completo do Automatic Quality Gate.

## 2026-08-01 — Auditoria combinada de STATE-02 executada e reprovada

- Estado anterior e resultante: `STATE-02 ARCHITECTURE` ativo; Automatic
  Quality Gate `BLOQUEADO` antes da execução e `REPROVADO` depois dela; Human
  Gate `PENDENTE`; sem transição.
- Baseline confirmada antes da inspeção e novamente antes do registro: branch
  `main`, commit `a01a765d177efb6c4013c6846c5f54c8adbe7e0f`, corpus `4.8.0`
  e working tree limpa.
- Autoridade: solicitação explícita do proprietário para executar localmente a
  auditoria combinada da baseline arquitetural aceita, registrar o resultado
  documental e criar commit focado, sem Human Gate ou progressão de estado.
- Escopo: inspeção e verificações documentais locais de rastreabilidade e
  consistência entre ADRs aceitos, contratos, threat model, relatório,
  lifecycle, requisitos, RAG, segurança, estado e histórico.
- Escopo negativo preservado: nenhuma implementação, rede, provider, conta,
  GitHub, OCI, publicação, deploy, DB-Notifier, Human Gate ou autorização de
  `STATE-03`.
- Runtime preflight: `NÃO APLICÁVEL`; trabalho exclusivamente documental e
  local, sem inspeção ou encerramento de processos.
- Verificações mecânicas aprovadas: 83 arquivos não ignorados, 30 Markdown com
  H1 e fences válidos, 131 links locais, 13 arquivos em `prompts/`, UTF-8/LF,
  newline final, whitespace, marcadores não resolvidos, padrões comuns de
  secret e `git diff --check`; quatro ADRs com um status `accepted`, data e
  autoridade; catálogo idêntico 51/54/9; 25 RF, 18 RNF, 20 critérios de
  aceitação, 19 itens Must, 36 ameaças e 15 grupos de testes.
- `AQG-S02-001` — P1: ADR-0002 inclui a identidade da observação de freshness
  em `sourceBindingSetDigest`, que participa da identidade da geração, mas
  também exige trocar somente o binding para uma nova observação após `304` ou
  hash idêntico, sem nova geração. As duas regras não definem um manifesto e
  um `CorpusActivationRecord` simultaneamente coerentes.
- `AQG-S02-002` — P2: `THR-S02-008`, `THR-S02-014` e `THR-S02-015` ainda
  apresentam como pendentes decisões de risco que a aceitação explícita dos
  ADRs já registrou; controles de runtime e egress continuam não implementados
  e não autorizados.
- `AQG-S02-003` — P3: documentos propostos preservados do `STATE-00` ainda
  exibem boundary físico e status de escolhas superados pelos ADRs aceitos.
  A precedência vigente evita autoridade concorrente, mas o drift aumenta o
  risco de leitura e implementação incorretas.
- Limitações: nenhuma fonte pública foi reconsultada; nenhum package, parser,
  provider, conta, corpus, índice, benchmark, backup, restore, IAM ou runtime
  foi executado. Fatos condicionais permanecem condicionais e não foram
  tratados como falha automática da auditoria documental.
- Quality Gate: `REPROVADO` para `STATE-02`, com zero P0, um P1, um P2 e um P3.
- Validação do registro: exatamente sete arquivos documentais alterados; audit
  do repositório aprovado para 83 arquivos não ignorados, 30 Markdown, 138
  links locais, H1 e fences; `git diff --check` aprovado.
- Human Gate: permanece `PENDENTE`, não foi solicitado e não pode ser
  preparado enquanto o Automatic Quality Gate estiver reprovado.
- Próxima condição: ADR corretivo aceito para `AQG-S02-001`, reconciliação de
  `AQG-S02-002` e `AQG-S02-003` e nova auditoria combinada autorizada sobre
  baseline limpa. `STATE-03` permanece sem autorização.

## 2026-08-02 — Pacote corretivo de STATE-02 preparado sem decisão de ADR

- Estado anterior e resultante: `STATE-02 ARCHITECTURE` ativo; Automatic
  Quality Gate `REPROVADO`; Human Gate `PENDENTE`; sem transição.
- Baseline confirmada antes da releitura e da escrita: branch `main`, commit
  `9707b87d75a6acb14c8993ff0283a4221bc6c762`, corpus `4.8.0` e working tree
  limpa.
- Autoridade humana exata:

  ```text
  Objetivo autorizado: preparar localmente um pacote de decisão para um novo ADR
  corretivo que resolva AQG-S02-001 e definir a reconciliação factual de
  AQG-S02-002 e AQG-S02-003. Registrar a proposta documental e criar commit
  focado, parando antes de qualquer decisão de aceitação.
  ```

- Autoridade preservada: ADR-0002 e ADR-0004 a ADR-0006 permanecem aceitos;
  a auditoria combinada permanece reprovada. A solicitação não aceita ou
  rejeita o novo ADR nem autoriza a repetição do gate.
- Proposta: ADR-0007 criado com status `proposed`. Ele compara três modelos e
  recomenda manter `sourceObservationId` fora de
  `sourceBindingSetDigest`, `generationSpecDigest` e `IndexGenerationId`,
  protegendo o binding completo por `activationBindingSetDigest` no
  `CorpusActivationRecord`.
- Semântica proposta: `304`/hash idêntico preserva manifesto,
  `catalogueRevision`, `generationActivatedAt` e geração, mas cria nova
  revisão/digest do registro de ativação. Conteúdo/snapshot/trust/adapter/
  registro imutável/documento/compatibilidade alterados exigem candidata nova.
  Rollback cria registro novo com observação atualmente compatível e nunca
  restaura freshness histórica.
- Alternativas: incluir observação na identidade é coerente somente criando
  nova geração para toda revalidação, com churn não recomendado; mutação ou
  binding sem digest foi rejeitado por quebrar identidade, proveniência ou
  integridade.
- `AQG-S02-002`: threat model reconciliado para registrar como aceitos os
  boundaries residuais de TLS e divulgação, mantendo implementação, conta,
  egress, orçamento, aviso e evidência abertos.
- `AQG-S02-003`: visão, arquitetura e segurança existentes foram preservadas
  em `pt-BR` e reconciliadas com o mapa físico do ADR-0003, abstrações RAG em
  Application, persistência em Infrastructure e status aceito/condicional de
  administração, providers, TLS, persistência e OCI.
- Limite decisório: ADR-0002, contratos canônicos e módulo RAG não receberam a
  semântica proposta antes da decisão humana. O ADR-0007 rastreia as mudanças
  necessárias nesses artefatos, requisitos, lifecycle, Quality Gates,
  roadmap, threat model e rollback caso seja aceito.
- Corpus: `4.8.1` (`PATCH`) por corrigir status factual e registrar uma
  proposta compatível sem alterar autoridade, lifecycle ou semântica aceita.
  Permanecem 13 arquivos em `prompts/`.
- Verificações dirigidas: PowerShell `7.6.4`, Git
  `2.55.0.windows.3` e ripgrep `15.2.0` em `<rag-challenge-root>`;
  `eng/check-repository.ps1` e `git diff --check` com exit `0`; 84 arquivos
  não ignorados, 31 Markdown, 13 prompts e 142 links locais; um status
  `proposed` no ADR-0007; zero frase stale alvo; zero mudança pré-decisão em
  ADR-0002, contratos canônicos ou módulo RAG. A validação foi repetida após
  este registro; não constituiu Automatic Quality Gate.
- Limitações: nenhuma implementação, build, parser, provider, conta, corpus,
  índice, benchmark, backup, restore, IAM, rede ou recurso externo foi
  executado. Os achados históricos não foram reclassificados por inferência.
- Escopo negativo preservado: nenhuma decisão de ADR, repetição do Automatic
  Quality Gate, Human Gate, `STATE-03`, GitHub, OCI, publicação, deploy,
  DB-Notifier ou ação externa.
- Quality Gate: permanece `REPROVADO`; `AQG-S02-001` continua aberto até
  decisão e reconciliação semântica. As fontes de `AQG-S02-002` e
  `AQG-S02-003` estão factualmente reconciliadas, com disposição pendente da
  próxima auditoria combinada.
- Human Gate: permanece `PENDENTE`, não solicitado e indisponível enquanto o
  Automatic Quality Gate estiver reprovado.
- Próxima condição: decisão humana explícita e independente sobre ADR-0007.
  Se aceito, executar a reconciliação semântica rastreada e somente depois
  obter autoridade separada para nova auditoria combinada sobre baseline
  limpa. `STATE-03` permanece sem autorização.

## 2026-08-02 — ADR-0007 aceito sem reconciliação semântica

- Estado anterior e resultante: `STATE-02 ARCHITECTURE` ativo; Automatic
  Quality Gate `REPROVADO`; Human Gate `PENDENTE`; sem transição.
- Baseline decisória confirmada: branch `main`, commit
  `664187c6926be5ce4bef3734603f8d936626d535`, corpus `4.8.1`, working tree
  limpa e ADR-0007 com um único status `proposed` antes do registro.
- Autoridade humana exata:

  ```text
  ADR-0007: ACEITAR.
  ```

- Decisão: ADR-0007 passa de `proposed` para `accepted`. A decisão torna
  autoritativa a separação entre identidade imutável da geração e identidade
  revisionada do registro de ativação, incluindo
  `activationBindingSetDigest` para o binding completo com observação.
- Efeito sobre ADR-0002: somente suas cláusulas conflitantes de identidade
  inclusiva da observação e rollback por registro exato são substituídas. As
  demais decisões aceitas de lifecycle, catálogo, providers e fontes
  permanecem vigentes.
- Limite da decisão: ADR-0002, contratos canônicos, módulo RAG, requisitos,
  lifecycle, Quality Gates, roadmap e threat model não foram reconciliados
  semanticamente neste registro. A aceitação não dispõe `AQG-S02-001` e não
  transforma o pacote proposto em evidência de implementação.
- Corpus: `4.9.0` (`MINOR`) porque uma proposta corretiva passa a autoridade
  arquitetural aceita sem mudança de lifecycle ou escopo funcional.
- Verificações dirigidas executadas antes e repetidas após esta entrada:
  auditoria do repositório aprovada para 84 arquivos não ignorados; 31
  Markdown; 13 arquivos em `prompts/`; ADR-0007 com exatamente um status
  `accepted` e zero `proposed`; zero mudança em ADR-0002, contratos canônicos
  ou módulo RAG; e `git diff --check` com exit `0`. A validação é documental e
  não constitui repetição do Automatic Quality Gate.
- Escopo negativo preservado: nenhuma reconciliação semântica, implementação,
  build, parser, provider, conta, corpus, índice, rede, GitHub, OCI,
  publicação, deploy, Human Gate, `STATE-03` ou alteração no DB-Notifier.
- Quality Gate: permanece `REPROVADO`; `AQG-S02-001` continua aberto até a
  reconciliação semântica e nova auditoria combinada separadamente autorizada.
  `AQG-S02-002` e `AQG-S02-003` preservam a reconciliação factual de suas
  fontes, ainda sem nova disposição de gate.
- Human Gate: permanece `PENDENTE`, não solicitado e indisponível enquanto o
  Automatic Quality Gate estiver reprovado.
- Próxima condição: obter autoridade separada para executar a reconciliação
  semântica rastreada pelo ADR-0007 aceito; depois, obter autoridade também
  separada para nova auditoria combinada sobre baseline limpa. `STATE-03`
  permanece sem autorização.

## 2026-08-02 — Semântica aceita do ADR-0007 reconciliada sem repetir o gate

- Estado anterior e resultante: `STATE-02 ARCHITECTURE` ativo; Automatic
  Quality Gate `REPROVADO`; Human Gate `PENDENTE`; sem transição.
- Baseline confirmada antes da releitura e da escrita: branch `main`, commit
  `9aa90c012e3bc973330f5a79678fc358c81809df`, corpus `4.9.0`, working tree
  limpa e ADR-0007 com status `accepted`.
- Autoridade humana exata:

  ```text
  Objetivo autorizado: aplicar localmente a semântica aceita do ADR-0007 em
  ADR-0002, contratos canônicos, módulo RAG, requisitos, lifecycle, Quality
  Gates, roadmap, threat model e registros factuais necessários; validar o diff
  documental e criar commit focado.
  ```

- Identidade da geração: ADR-0002, contratos e módulo RAG agora excluem
  `sourceObservationId` de `sourceBindingSetDigest`, `generationSpecDigest`,
  digest do manifesto completo e `IndexGenerationId`. A projeção
  generation-bound conserva banco/revisão, documento/versão/formato, adapter,
  trust, registro oficial imutável/versionado e snapshot imutável.
- Integridade da ativação: `CorpusActivationRecord` passa a registrar
  `activationBindingSetDigest` sobre o binding completo com observação. Os dois
  domínios usam versões canônicas distintas, UTF-8, ordem fixa/ordinal e null
  inequívoco; `STATE-03` deve materializar vetores executáveis.
- Validação transacional: antes do compare-and-swap, a Application confere
  `activeDocumentSetDigest` e `sourceBindingSetDigest` contra o manifesto,
  confere `activationBindingSetDigest` contra o registro e prova que cada
  observação nomeia o mesmo registro/snapshot imutável. Mismatch falha fechado.
- Revisões e revalidação: `catalogueRevision` permanece generation-bound e
  separado do journal de observações e da revisão transacional. `304`/hash
  idêntico compatível cria nova revisão completa e digest de ativação, mas
  preserva manifesto, geração, digest de especificação/fonte,
  `catalogueRevision` e `generationActivatedAt`.
- Consulta e proveniência: uma consulta resolve um registro, avalia somente as
  observações nele vinculadas e envia ao vector store os bindings
  generation-bound elegíveis como hard pre-filter anterior ao top-k; não existe
  leitura implícita de “última observação”.
- Rollback: a geração retida/validada e sua projeção generation-bound formam o
  alvo, mas a operação constrói registro novo com observações explicitamente
  selecionadas, compatíveis e atualmente elegíveis. Registro e freshness
  históricos nunca são reproduzidos byte a byte; invariante de evidência
  insatisfeita preserva o registro corrente.
- Rastreabilidade: `RNF-005`, `AC-MVP-005`, `AC-MVP-014`, arquitetura da
  solução, lifecycle, Quality Gates, S03/S04/S07, `BL-M14`, template de corpus,
  threats `003`, `004`, `011`, `012`, `013`, `021`, `024`, `034` e grupos de
  teste associados foram reconciliados sem mudar os limites de catálogo,
  PDF/CSV, providers, persistência, OCI, segurança ou egress.
- Corpus: `4.9.1` (`PATCH`) porque torna corrente uma autoridade arquitetural
  já aceita, sem nova capacidade, alteração de lifecycle ou escopo funcional.
  Permanecem 13 arquivos ativos em `prompts/`.
- Verificação dirigida anterior a esta entrada: PowerShell `7.6.4`, Git
  `2.55.0.windows.3` e ripgrep `15.2.0` em `<rag-challenge-root>`;
  `eng/check-repository.ps1` com exit `0` para 84 arquivos não ignorados;
  `git diff --check` com exit `0`; assertions semânticas com exit `0` para 17
  paths, 25 RF, 18 RNF, 20 critérios, 19 itens Must, 36 threats e 15 grupos de
  segurança. Os checks foram repetidos depois desta entrada sobre o diff
  completo; não constituem Automatic Quality Gate.
- Limitações: nenhum código, migration, store, parser, provider, conta, corpus,
  índice, build, teste de runtime, benchmark, backup, restore, IAM, rede ou
  recurso externo foi implementado, acessado ou executado.
- Escopo negativo preservado: nenhuma repetição do Automatic Quality Gate,
  solicitação de Human Gate, autorização de `STATE-03`, GitHub, OCI,
  DB-Notifier, publicação ou deploy.
- Quality Gate: permanece `REPROVADO`. A fonte documental de `AQG-S02-001`
  agora está corrigida, assim como já estavam as fontes de `AQG-S02-002` e
  `AQG-S02-003`, mas somente nova auditoria combinada separadamente autorizada
  pode dar disposição aos achados.
- Human Gate: permanece `PENDENTE`, não solicitado e indisponível enquanto o
  Automatic Quality Gate estiver reprovado.
- Próxima condição: autoridade separada para nova auditoria combinada sobre a
  baseline limpa reconciliada. `STATE-03` permanece sem autorização.

## 2026-08-02 — Nova auditoria combinada de STATE-02 aprovada

- Estado anterior e resultante: `STATE-02 ARCHITECTURE` ativo; Automatic
  Quality Gate `REPROVADO` antes da execução e `APROVADO` depois dela; Human
  Gate `PENDENTE`; sem transição.
- Baseline confirmada antes da releitura e novamente antes do registro: branch
  `main`, commit `3978a17201cf5f6ac4ddc189862736fc3646457b`, corpus `4.9.1`
  e working tree limpa.
- Autoridade humana exata:

  ```text
  Objetivo autorizado: executar localmente a nova auditoria combinada do
  Automatic Quality Gate de STATE-02 sobre a baseline reconciliada, registrar o
  resultado factual e criar commit documental focado, parando após o relatório.
  ```

- Escopo: inspeção documental local e rastreabilidade entre ADR-0002 e
  ADR-0004 a ADR-0007, contratos, requisitos, lifecycle, Quality Gates,
  arquitetura, módulo RAG, roadmap, threat model, relatório, índices, estado e
  histórico; classificação e disposição factual dos achados anteriores.
- Escopo negativo preservado: nenhuma implementação, rede, provider, conta,
  GitHub, OCI, publicação, deploy, DB-Notifier, solicitação de Human Gate ou
  autorização de `STATE-03`.
- Runtime preflight: `NÃO APLICÁVEL`; o trabalho foi exclusivamente documental
  e local, sem inspeção ou encerramento de processos.
- Verificações mecânicas: PowerShell `7.6.4`, Git `2.55.0.windows.3` e ripgrep
  `15.2.0` em `<rag-challenge-root>`; `eng/check-repository.ps1` e
  `git diff --check` com exit `0`; 84 arquivos não ignorados, 31 Markdown, 143
  links locais, um H1 e fences balanceadas por Markdown, sem marcador de merge;
  cinco ADRs aceitos com status/data/autoridade únicos; catálogo idêntico
  51/54/9; 25 RF, 18 RNF, 20 critérios, 19 itens Must, 36 threats e 15 grupos
  de testes de segurança.
- Verificação semântica: assertions retornaram zero falhas para separação entre
  `sourceBindingSetDigest` e `activationBindingSetDigest`, domínios canônicos,
  três validações pré-CAS, campos exatos de `304`/hash idêntico, revisões,
  hard pre-filter anterior ao top-k, rollback por novo registro, proveniência,
  status de risco e atualidade dos documentos roteados.
- `AQG-S02-001` — `RESOLVIDO`: ADR-0007 aceito e corpus `4.9.1` tornam
  coerentes manifesto, identidade da geração, binding completo, revalidação e
  rollback; a contradição P1 histórica não foi reproduzida.
- `AQG-S02-002` — `RESOLVIDO`: `THR-S02-008`, `THR-S02-014`, `THR-S02-015`
  e a seção de aceitação de riscos distinguem a boundary arquitetural aceita
  de controles, conta, egress, orçamento, aviso e evidência ainda ausentes.
- `AQG-S02-003` — `RESOLVIDO`: os documentos roteados do `STATE-00` preservam
  seu idioma e contexto proposto, mas refletem o mapa físico aceito e o status
  aceito/condicional de administração, TLS, providers, persistência e OCI.
- Achados novos: zero P0, zero P1, zero P2 e zero P3. As severidades anteriores
  permanecem preservadas como evidência histórica; a auditoria não corrigiu
  achado silenciosamente.
- Limitações e riscos residuais: nenhuma fonte foi reconsultada; nenhum build,
  parser, provider, conta, corpus, índice, benchmark, backup, restore, IAM,
  egress ou runtime foi executado. Packages PDF/CSV, qualidade bilíngue,
  capacidade do vector store, controles de segurança, dados/licenças do corpus
  e fatos da tenancy permanecem para seus estados e autoridades próprios. Os
  36 threats continuam requisitos de implementação/teste, e os riscos
  residuais aceitos não habilitam egress nem provam controles.
- Corpus: permanece `4.9.1`; o lote registra resultado factual de gate sem
  mudar autoridade normativa, capacidade, lifecycle ou escopo funcional.
- Validação do registro: exatamente sete arquivos documentais alterados;
  auditoria do repositório, assertions semânticas e `git diff --check`
  repetidos com os mesmos resultados aprovados após esta entrada.
- Quality Gate: `APROVADO` para `STATE-02` sobre a baseline reconciliada.
- Human Gate: permanece `PENDENTE`, não foi solicitado e não recebeu decisão.
- Próxima condição: autoridade separada do proprietário para preparar e
  apresentar o resumo completo do Human Gate de `STATE-02`. A aprovação
  automática não promove o lifecycle; `STATE-03` permanece sem autorização.

## 2026-08-02 — Human Gate de STATE-02 aprovado

- Estado anterior: `STATE-02 ARCHITECTURE` ativo; Automatic Quality Gate
  `APROVADO`; Human Gate `PENDENTE`.
- Baseline confirmada: branch `main`, commit
  `6e61c4cf4429e2a62145d43bec3783146f01e37f`, corpus `4.9.1` e working tree
  limpa.
- Pré-condições: resumo completo vigente apresentado na mesma conversa,
  incluindo relatório automático, disposição de `AQG-S02-001` a
  `AQG-S02-003`, amostras críticas, limitações, riscos residuais, condições
  pendentes, escopo negativo e decisão recomendada.
- Autoridade humana exata:

  ```text
  Confirmo a decisão acima exclusivamente para STATE-02
  ```

- Decisão: Human Gate `APROVADO` sem ressalvas, exclusivamente para
  `STATE-02 ARCHITECTURE`.
- Entregáveis aceitos: ADR-0002 e ADR-0004 a ADR-0007; contratos canônicos;
  threat model; catálogo inicial 51/54/9; lifecycle PDF/CSV; fontes oficiais;
  providers/persistência/OCI condicionais; quatro perfis de egress; contratos
  bilíngues, de proveniência, ativação, readiness, OpenAPI e rollback.
- Evidências aceitas: nova auditoria combinada `APROVADA`; cinco ADRs aceitos
  com autoridade única; 84 arquivos não ignorados, 31 Markdown e 143 links
  locais; 25 RF, 18 RNF, 20 critérios, 19 itens Must, 36 threats e 15 grupos
  de testes; `AQG-S02-001`, `AQG-S02-002` e `AQG-S02-003` `RESOLVIDOS`; zero
  novo P0, P1, P2 ou P3.
- Amostras críticas revisadas: catálogo/formatos/fontes; decisões condicionais
  de parser, OpenAI, SQLite/vector e OCI; threat model e egress deny-by-default;
  contrato bilíngue/proveniência; dois domínios de digest; revalidação
  `304`/hash idêntico; hard pre-filter; e rollback por registro novo sem replay
  de freshness.
- Amostras não repetidas: nenhum comportamento executável de parser, provider,
  conta, corpus, índice, backup, restore, IAM, egress, OCI ou interface existe
  em `STATE-02`; sua execução não era aplicável ao Human Gate documental e
  permanece dependente de estados e autoridades futuros.
- Limitações e riscos aceitos: packages PDF/CSV, qualidade/segurança dos
  parsers, documentos/direitos do produto, dataset de avaliação, conta e
  comportamento bilíngue dos providers, capacidade do vector store,
  backup/restore, tenancy/IAM/cobrança OCI e efetividade dos 36 controles
  permanecem não verificados. Riscos residuais aceitos não habilitam egress
  nem provam controles.
- Escopo negativo preservado: nenhuma entrada em `STATE-03`, implementação,
  migration, build, parser, provider, conta, corpus, índice, rede, GitHub, OCI,
  publicação, deploy, DB-Notifier ou ação externa.
- Automatic Quality Gate: `APROVADO` para a baseline documental reconciliada.
- Human Gate: `APROVADO` sem ressalvas por confirmação inequívoca.
- Estado resultante: `STATE-02 ARCHITECTURE` encerrado; `STATE-03
  DATA_AND_INDEX_MODELING` permanece sem autorização de entrada.
- Próxima condição: preparar resumo de prontidão e obter autorização humana
  separada e explícita para qualquer entrada em `STATE-03`.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-02 — Entrada limitada em STATE-03 DATA_AND_INDEX_MODELING autorizada

- Estado anterior: `STATE-02 ARCHITECTURE` encerrado; Automatic Quality Gate
  e Human Gate aprovados sem ressalvas; `STATE-03 DATA_AND_INDEX_MODELING`
  ainda não iniciado.
- Estado solicitado: entrar em `STATE-03` e executar localmente, de forma
  sequencial, somente o lote `S03-A`.
- Baseline confirmada antes do registro: branch `main`, commit
  `35b67c194f6ea2459833420b8bc2143fadfe75df`, corpus `4.9.1` e working tree
  limpa.
- Autoridade humana exata:

  ```text
  Continue nesta conversa o trabalho do RAG-Challenge.

  Autorizo a entrada no STATE-03 DATA_AND_INDEX_MODELING sobre a baseline
  main@35b67c194f6ea2459833420b8bc2143fadfe75df, corpus 4.9.1,
  exclusivamente no envelope de prontidão apresentado nesta conversa.

  Registre primeiro a entrada de forma append-only. Depois, execute localmente e
  de forma sequencial somente o lote S03-A: modelo e dicionário do catálogo,
  identidades e estados, relações e constraints, separação das revisões,
  serialização canônica, golden vectors dos dois domínios de digest, três
  validações pré-CAS, invariantes de ativação/retensão/rollback e fixtures
  determinísticas. Preserve as fronteiras aceitas de Domain, Application e
  Infrastructure.

  S03-B permanece bloqueado até decisão separada sobre o conjunto e as versões
  exatas das dependências, verificação de supply chain, restore/instalação,
  lockfiles, migrations e stores. Nesta autorização, não crie migrations, não
  materialize stores persistentes e não adicione ou instale dependências.

  Permanecem proibidos rede, providers, contas, corpus real, fontes oficiais,
  armazenamento operacional, GitHub, OCI, publicação, deploy, DB-Notifier,
  entrada em STATE-04 e encerramento de STATE-03. Não avance o lifecycle sem
  Automatic Quality Gate, relatório factual, resumo completo e Human Gate
  separados.

  Antes de qualquer escrita, confirme novamente branch, commit, corpus, working
  tree, estado factual e autoridade. Se houver divergência, mudança externa,
  dependência inesperada ou necessidade de ampliar o escopo, pare e informe.
  ```

- Decisão: entrada em `STATE-03 DATA_AND_INDEX_MODELING` autorizada dentro do
  envelope apresentado; somente `S03-A` está ativo para execução local e
  sequencial.
- Escopo autorizado: modelo e dicionário do catálogo; identidades, estados,
  relações e constraints; separação entre `catalogueRevision`, revisão do
  journal de observações e `recordRevision`; serialização canônica e vetores
  de referência de `sourceBindingSetDigest` e
  `activationBindingSetDigest`; três validações pré-CAS; invariantes de
  ativação, retenção e rollback; fixtures determinísticas; verificações e
  commits locais focados.
- Escopo bloqueado: `S03-B`, seleção/adição/instalação de dependências,
  restore, lockfiles, migrations, stores persistentes e armazenamento
  operacional.
- Escopo negativo preservado: nenhuma rede, provider, conta, corpus real,
  fonte oficial, GitHub, OCI, publicação, deploy, DB-Notifier, entrada em
  `STATE-04` ou encerramento de `STATE-03`.
- Runtime preflight: aplicável antes da primeira alteração ou validação de
  comportamento executável de `S03-A`; o registro documental da entrada o
  precede deliberadamente.
- Quality Gate de `STATE-03`: `PENDENTE`.
- Human Gate de `STATE-03`: `PENDENTE`.
- Estado resultante: `STATE-03 DATA_AND_INDEX_MODELING` ativo somente para
  `S03-A`; `STATE-04 BACKEND_IMPLEMENTATION` não autorizado.
- Próxima condição: concluir e verificar `S03-A`; depois, obter decisão
  separada para qualquer execução de `S03-B`.
- Aprovador da entrada: proprietário do RAG-Challenge.

## 2026-08-02 — S03-A concluído sem promover STATE-03

- Estado anterior: `STATE-03 DATA_AND_INDEX_MODELING` ativo somente para
  `S03-A`; `S03-B`, Automatic Quality Gate, Human Gate, encerramento e
  `STATE-04` bloqueados.
- Baseline de execução: entrada append-only registrada no commit
  `5efaa37d2e3e6533d54dafece34158dd5f4adbd1`, derivado da baseline autorizada
  `main@35b67c194f6ea2459833420b8bc2143fadfe75df`, corpus `4.9.1`.
- Runtime preflight: zero processos e zero listeners pertencentes ao
  RAG-Challenge; nada foi encerrado.
- Divergência externa observada: Node.js instalado `24.18.1`, enquanto
  `.nvmrc` e `package.json` fixam `24.18.0`. A execução parou antes do commit
  até autoridade humana explícita; nenhum pin foi alterado e nenhuma
  instalação foi feita.
- Autoridade humana adicional exata:

  ```text
  Autorizo excepcionalmente concluir o S03-A usando Node.js 24.18.1 somente para as verificações locais, mantendo os pins vigentes em 24.18.0 e registrando a divergência factual no relatório. Não autoriza instalação, alteração de dependências ou lockfiles, S03-B, Automatic Quality Gate, Human Gate, encerramento de STATE-03 ou entrada em STATE-04. Preserve todo o escopo negativo vigente.
  ```

- Resultado implementado: modelo e dicionário lógico do catálogo; identidades,
  estados, relações e constraints; revisões separadas; serialização canônica;
  golden vectors dos domínios `sourceBindingSetDigest` e
  `activationBindingSetDigest`; três validações pré-CAS; invariantes de
  staging, ativação, retenção e rollback; fixtures determinísticas 51/54/9.
- Fronteiras: Domain contém valores, registros e digests puros; Application
  contém construção de revisões e validação pré-CAS; Infrastructure não foi
  alterada. Nenhum projeto, package, lockfile, migration ou store foi criado
  ou modificado.
- Commit funcional: `ace780a25edb2749046e9897b8af36e0719e3e54`
  (`feat(model): implement state 03 s03-a`).
- Verificações finais, executadas localmente sem restore ou instalação:
  format aprovado; build Release aprovado com zero warning/erro; 68 testes
  aprovados e nenhum falhou ou foi ignorado; cobertura 95,55% de linhas
  (1051/1100) e 89,93% de branches (268/298); Dashboard lint, typecheck, 2
  testes e build aprovados sob a variação Node.js `24.18.1`; auditoria do
  repositório aprovada para 104 arquivos não ignorados; `git diff --check`
  aprovado.
- Verificação não executada: `eng/ci.ps1 -Offline`, pois o agregado executa
  restore .NET e `npm ci`; essas ações permanecem bloqueadas com `S03-B`. Seus
  componentes autorizados foram executados individualmente sem restore ou
  instalação.
- Escopo negativo preservado: nenhuma rede, provider, conta, corpus real,
  fonte oficial, armazenamento operacional, GitHub, OCI, publicação, deploy,
  DB-Notifier, `S03-B`, Automatic Quality Gate, Human Gate, encerramento de
  `STATE-03` ou entrada em `STATE-04`.
- Quality Gate de `STATE-03`: `PENDENTE`; as verificações de S03-A não o
  substituem.
- Human Gate de `STATE-03`: `PENDENTE` e não solicitado.
- Estado resultante: `S03-A` concluído; `STATE-03` permanece ativo e aberto;
  `S03-B` continua sem autorização.
- Próxima condição: decisão humana separada sobre o conjunto e as versões
  exatas de dependências e sobre a execução restrita de `S03-B`.
- Aprovador da variação local: proprietário do RAG-Challenge.

## 2026-08-02 — S03-B autorizado após nova aprovação de supply chain

- Estado anterior: `STATE-03 DATA_AND_INDEX_MODELING` ativo; `S03-A`
  concluído; `S03-B`, Automatic Quality Gate, Human Gate, encerramento e
  `STATE-04` bloqueados.
- Baseline confirmada antes do registro: branch `main`, commit
  `7e56569cab214f95a1af1a4df46019efb4a5a3fe`, corpus `4.9.1` e working tree
  limpa.
- Autoridade humana exata:

  ```text
  Continue nesta conversa o trabalho do RAG-Challenge.

  Autorizo uma nova execução local e sequencial de S03-B sobre a baseline
  main@7e56569cab214f95a1af1a4df46019efb4a5a3fe, corpus 4.9.1,
  exclusivamente no envelope decisório apresentado nesta resposta.

  O conjunto exato autorizado é:
  - Microsoft.EntityFrameworkCore.Sqlite 10.0.10;
  - Microsoft.EntityFrameworkCore.Design 10.0.10 com PrivateAssets=all;
  - Microsoft.Data.Sqlite.Core 10.0.10;
  - pin central transitivo SQLitePCLRaw.bundle_e_sqlite3 2.1.12;
  - ferramenta local dotnet-ef 10.0.10.

  Aceito como limitações factuais que a verificação de revogação X.509 foi
  mantida offline e que não foi provada uma ligação criptográfica ou build
  reproduzível entre os commits dos mantenedores e os nupkgs. Esta aceitação não
  dispensa hash, repository signature, advisory, licença, fechamento transitivo,
  asset Linux ARM64 ou qualquer outra condição de parada.

  Antes de escrever no repositório, reconfirme branch, HEAD, corpus, working
  tree, estado factual e autoridade e execute novamente S03-B0. Autorize HTTPS
  somente para a fonte NuGet configurada e para fontes primárias dos
  mantenedores. Revalide as 42 versões esperadas, SHA-512 do catálogo,
  repository signatures, índice vigente de certificados, advisories, licenças e
  o asset linux-arm64 com SQLite 3.53.3. Pare se houver qualquer divergência ou
  se o fechamento real diferir.

  Somente se S03-B0 for aprovado, altere Directory.Packages.props,
  RagChallenge.Infrastructure.csproj, o tool manifest local e apenas os
  lockfiles realmente afetados; restaure e instale localmente somente as versões
  exatas; crie e aplique migrations apenas em stores temporários não produtivos;
  e execute sequencialmente S03-B1 a S03-B5 no envelope já apresentado.

  Implemente control.db como autoridade única de catálogo, manifesto, ativação,
  histórico, auditoria e retenção; vectors.db como store derivado sem autoridade
  ativa; e content store imutável por SHA-256. Preserve CAS por revisão
  esperada, três digests, observações explícitas, uma geração anterior por janela
  mínima de 14 dias, cleanup manual auditado, rollback por nova revisão e
  recuperação isolada.

  Não use providers, contas, corpus real, fontes oficiais, armazenamento
  operacional, GitHub, OCI, publicação, deploy ou DB-Notifier. Não execute
  Automatic Quality Gate, Human Gate, encerramento de STATE-03 ou entrada em
  STATE-04. Pare diante de qualquer divergência de pacote, lockfile, migration,
  constraint, atomicidade, path safety, teste ou autoridade.
  ```

- S03-B0 repetido antes deste registro: `APROVADO`. O fechamento compatível
  com `net10.0` contém exatamente 42 versões e 89 relações de dependência;
  nenhum package esperado ficou ausente ou inalcançável.
- Supply chain observada: 42/42 hashes SHA-512 iguais ao catálogo NuGet;
  42/42 nupkgs com repository signature válida em modo de revogação offline;
  índice NuGet com `allRepositorySigned=true`, três certificados históricos e
  um certificado vigente; zero advisory aplicável; 38 licenças MIT e quatro
  Apache-2.0.
- Asset nativo: `runtimes/linux-arm64/native/libe_sqlite3.so` presente como
  ELF64 little-endian AArch64, com 1.534.296 bytes, SHA-256
  `707fff6b18c1f083158e7a543c8d2545d5485f547ffd96db29679a95f52878d5`,
  SQLite `3.53.3` e source ID
  `2026-06-26 20:14:12 d4c0e51e4aeb96955b99185ab9cde75c339e2c29c3f3f12428d364a10d782c62`.
- Fontes: única fonte NuGet configurada
  `https://api.nuget.org/v3/index.json`; catálogo, índice de assinaturas e
  advisories do NuGet; advisory e tag `v2.1.12` do mantenedor; página primária
  de CVEs do SQLite.
- Limitações aceitas e preservadas: revogação X.509 verificada offline; nenhuma
  ligação criptográfica ou reprodução de build entre os commits dos
  mantenedores e os nupkgs foi provada.
- Runtime preflight após S03-B0: zero processos executáveis pertencentes ao
  RAG-Challenge e nada encerrado.
- Decisão: executar localmente e de modo sequencial `S03-B1` a `S03-B5`
  somente com o conjunto exato autorizado e apenas após este registro.
- Escopo negativo preservado: nenhuma ampliação de pacote ou versão, provider,
  conta, corpus real, fonte oficial, armazenamento operacional, GitHub, OCI,
  publicação, deploy, DB-Notifier, Automatic Quality Gate, Human Gate,
  encerramento de `STATE-03` ou entrada em `STATE-04`.
- Estado resultante: `STATE-03` permanece ativo e aberto; `S03-B` está
  autorizado para a execução local delimitada; os gates e a transição
  subsequente continuam bloqueados.
- Próxima condição: executar e verificar sequencialmente `S03-B1` a `S03-B5`,
  registrar relatório factual e parar antes de qualquer gate ou promoção.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-02 — Fechamento materializado de S03-B reconciliado

- Estado anterior: `S03-B0` aprovado para o conjunto conservador de 42
  nupkgs; execução de `S03-B1` interrompida ao observar 40 packages no
  `project.assets.json` de `net10.0` mais a ferramenta local
  `dotnet-ef 10.0.10`, totalizando 41 itens materializados.
- Baseline de retomada confirmada: branch `main`, commit
  `381d1cd297580476e461a242ce5b66c4884e521b`, corpus `4.9.1` e working tree
  contendo somente as sete entradas preservadas da interrupção de `S03-B1`.
- Autoridade humana exata:

  ```text
  Continue nesta conversa o trabalho do RAG-Challenge.

  Aceito a distinção factual entre o conjunto conservador de supply chain de 42 nupkgs verificados e o fechamento efetivamente materializado para net10.0, composto por 40 packages de projeto e pela ferramenta local dotnet-ef 10.0.10, totalizando 41 itens. Aceito que System.Memory 4.5.3 permaneça somente como evidência conservadora verificada, sem pin, referência ou restauração forçada, pois o NuGet não o incluiu no project.assets.json de net10.0.

  Autorizo retomar sequencialmente S03-B a partir de main@381d1cd297580476e461a242ce5b66c4884e521b e da working tree interrompida, preservando exclusivamente as alterações já autorizadas. Registre primeiro a divergência factual no relatório e no estado vigente. Depois, conclua S03-B1 com locked restore e validação dos quatro lockfiles realmente afetados. Somente se essas verificações forem aprovadas, prossiga sequencialmente com S03-B2 a S03-B5 no envelope já autorizado.

  Não adicione System.Memory nem qualquer outra dependência, não amplie versões ou escopo e preserve todas as condições de parada e todo o escopo negativo vigente. Não execute Automatic Quality Gate, Human Gate, encerramento de STATE-03 ou entrada em STATE-04.
  ```

- Decisão: distinguir formalmente o conjunto conservador de evidência de
  supply chain do fechamento materializado pelo NuGet para `net10.0`;
  `System.Memory 4.5.3` não integra o grafo materializado e não será pinado,
  referenciado ou restaurado à força.
- Condição de retomada: concluir `S03-B1` com locked restore e validação dos
  quatro lockfiles realmente afetados antes de iniciar `S03-B2`.
- Escopo negativo preservado: nenhuma dependência ou versão adicional,
  provider, conta, corpus real, fonte oficial do produto, armazenamento
  operacional, GitHub, OCI, publicação, deploy, DB-Notifier, Automatic
  Quality Gate, Human Gate, encerramento de `STATE-03` ou entrada em
  `STATE-04`.
- Estado resultante: `STATE-03` permanece ativo e aberto; `S03-B1` pode ser
  retomado sequencialmente sob a reconciliação aceita.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-02 — S03-B5 interrompido por divergência de migration de Control

- Estado anterior: `S03-B1` a `S03-B4` executados sequencialmente após a
  reconciliação humana do fechamento de packages; `S03-B5` pendente.
- Baseline factual do início de `S03-B5`: branch `main`, commit
  `8b3a6ac8ddf0fdd92995fe73db32b56f81ae1036`, corpus `4.9.1`, working tree
  limpa e zero processos pertencentes ao RAG-Challenge.
- Evidência aprovada antes da divergência: locked restore com 40 packages de
  projeto, ausência de `System.Memory`, ferramenta local `dotnet-ef 10.0.10`,
  nenhum package vulnerável reportado pela fonte NuGet vigente, format sem
  mudança e build Release sem warnings ou errors.
- Evidência de testes: 56 testes unitários e 16 de integração aprovados. A
  primeira passagem encontrou um tipo auxiliar do compilador fora do namespace
  raiz; a correção local focada foi registrada em `8b3a6ac` e a repetição
  dirigida aprovou 10/10 testes de arquitetura. O agregado completo não foi
  repetido depois da condição de parada.
- Divergência bloqueante: `dotnet-ef migrations list` informou zero migrations
  para `ControlPlaneDbContext`, apesar de migration, designer com atributos e
  snapshot rastreados. A migration `20260802171400_InitialVectorStore` foi
  descoberta e passou apply, rollback para zero e reapply no store temporário;
  o contexto de Control informou mudanças pendentes no modelo e retornou exit
  code 1. A verificação de modelo pendente de Vector e os demais checks de
  `S03-B5` não foram executados.
- Disposição: execução interrompida sem diagnóstico corretivo, sem mudança de
  migration e sem inferir resultado de gate. O diretório temporário validado,
  contendo somente `control.db` e `vectors.db` não produtivos, foi removido.
- Estado resultante: `STATE-03` permanece ativo e aberto; `S03-B5` está
  incompleto e bloqueado até nova autoridade humana explícita para retomada.
  Automatic Quality Gate, Human Gate, encerramento de `STATE-03` e entrada em
  `STATE-04` permanecem não executados e não autorizados.
- Escopo negativo preservado: nenhum provider, conta, corpus real, fonte
  oficial do produto, armazenamento operacional, GitHub, OCI, publicação,
  deploy ou DB-Notifier foi usado ou alterado.
- Aprovador da autoridade de execução interrompida: proprietário do
  RAG-Challenge.

## 2026-08-02 — S03-B5 retomado e concluído sem promover STATE-03

- Estado anterior: `STATE-03 DATA_AND_INDEX_MODELING` ativo; `S03-A` e
  `S03-B0` a `S03-B4` concluídos; `S03-B5` interrompido pela divergência de
  descoberta/snapshot da migration de Control.
- Baseline de retomada: branch `main`, commit
  `c72c8b967667f72e8971f4887174585d3640a36e`, corpus `4.9.1`, working tree
  limpa.
- Autoridade humana exata, emitida após a apresentação do escopo de retomada:

  ```text
  Pode fazer agora?
  ```

- Interpretação limitada da autoridade: diagnosticar a divergência, corrigir
  somente o necessário e repetir integralmente `S03-B5`; Automatic Quality
  Gate, Human Gate, encerramento de `STATE-03` e entrada em `STATE-04`
  permaneceram fora do escopo.
- Runtime preflight: zero processos e zero listeners pertencentes ao
  RAG-Challenge; nada foi encerrado.
- Diagnóstico observado: após clean e build Release novos, o EF descobriu as
  migrations Control `20260802171743_InitialControlPlane` e Vector
  `20260802171400_InitialVectorStore`. A divergência anterior não se
  reproduziu; a evidência é compatível com output incremental stale consumido
  por `--no-build`, sem provar causa histórica mais profunda.
- Verificação de migrations: os dois contextos passaram sequencialmente
  list, apply, rollback para zero, reapply e
  `has-pending-model-changes`, sem mudança pendente. `control.db` possuía
  380.928 bytes e `vectors.db`, 49.152 bytes; o diretório temporário validado
  foi removido.
- Correção de tooling: o primeiro `eng/ci.ps1 -Offline` retomado passou os
  checks funcionais e parou na auditoria porque NuGet reserializou os sete
  lockfiles rastreados com CRLF no Windows. O entry point passou a reportar e
  normalizar somente esses arquivos gerados para UTF-8/LF depois do locked
  restore, sem ocultar mudança lógica do grafo.
- Verificação agregada repetida: `eng/ci.ps1 -Offline` retornou exit code `0`;
  82 testes .NET aprovados (56 unitários, 10 de arquitetura e 16 de
  integração), cobertura de 94,83% de linhas (8.481/8.943) e 72,34% de
  branches (688/951), lint, typecheck, dois testes e build Vite aprovados,
  auditoria do repositório aprovada para 130 arquivos não ignorados e diff
  hygiene aprovado. Node.js `24.18.1` permaneceu somente como variação local
  anteriormente aceita; os pins não mudaram.
- Auditoria de dependências: a consulta vigente à fonte NuGet autorizada não
  encontrou package direto ou transitivo vulnerável em nenhum projeto; não
  houve alteração de package, versão ou lockfile lógico.
- Resultado: `S03-B5` e o incremento `S03-B` estão concluídos. Nenhuma
  migration foi aplicada a armazenamento operacional; os stores foram
  exclusivamente temporários e não produtivos.
- Limitações preservadas: não houve process-crash injection, benchmark/SLA,
  provider, conta, corpus real, fonte oficial do produto, armazenamento
  operacional, GitHub, OCI, publicação, deploy ou DB-Notifier.
- Quality Gate de `STATE-03`: `PENDENTE` e não executado.
- Human Gate de `STATE-03`: `PENDENTE` e não solicitado.
- Estado resultante: `STATE-03` permanece ativo e aberto; entrada em
  `STATE-04` continua não autorizada.
- Próxima condição: obter autoridade separada para executar o Automatic
  Quality Gate de `STATE-03` sobre baseline limpa; somente depois preparar o
  resumo completo do Human Gate.
- Aprovador da retomada: proprietário do RAG-Challenge.

## 2026-08-02 — Automatic Quality Gate de STATE-03 aprovado

- Estado anterior: `STATE-03 DATA_AND_INDEX_MODELING` ativo; `S03-A` e
  `S03-B0` a `S03-B5` concluídos; Automatic Quality Gate e Human Gate
  pendentes.
- Baseline auditada: branch `main`, commit
  `3d0731fdf3f5004fb185dc760b5f74e4d73b4aa5`, corpus `4.9.1`, working tree
  limpa.
- Autoridade humana exata:

  ```text
  Continue nesta conversa o trabalho do RAG-Challenge.

  Autorizo exclusivamente a execução local e sequencial do Automatic Quality Gate de STATE-03 sobre main@3d0731fdf3f5004fb185dc760b5f74e4d73b4aa5, corpus 4.9.1 e working tree limpa.

  Audite os entregáveis de STATE-03, execute as verificações offline aplicáveis, repita as migrations somente em stores temporários não produtivos e produza o resultado factual do gate. Não corrija achados silenciosamente: diante de qualquer achado, pare, classifique-o e informe.

  Não execute Human Gate, encerramento de STATE-03, entrada em STATE-04, provider, corpus real, fonte oficial, GitHub, OCI, publicação, deploy ou qualquer outra ação externa.
  ```

- Runtime preflight: zero processos e zero listeners pertencentes ao
  RAG-Challenge; nada foi encerrado.
- Escopo estático: modelo/dicionário, constraints, índices, catálogo 51/54/9,
  snapshots, observações e revisão própria, dois domínios de digest, três
  validações pré-CAS, manifesto/generation identity, staging, conteúdo
  imutável, CAS, retenção, cleanup, rollback e recuperação reconciliados com
  Lifecycle, Quality Gates, contratos aceitos, source e testes.
- Arquitetura: Domain/Application sem EF Core, SQLite, Infrastructure ou
  adapters externos; `control.db` conserva autoridade exclusiva e
  `vectors.db` permanece derivado. Nenhum material local, database runtime ou
  build output está rastreado.
- Gate agregado: `eng/ci.ps1 -Offline` retornou exit code `0`; locked restore,
  format, build Release sem warnings/errors, 82 testes .NET (56 unitários, 10
  de arquitetura e 16 de integração), cobertura de 94,83% de linhas
  (8.481/8.943) e 72,34% de branches (688/951), lint, typecheck, dois testes e
  build Vite, auditoria de 130 arquivos e diff hygiene aprovados.
- Migrations: Control e Vector passaram separadamente list, apply, rollback
  para zero, reapply e `has-pending-model-changes` após clean/build novos.
  Os stores não produtivos possuíam 380.928 e 49.152 bytes e foram removidos
  com o diretório temporário validado.
- Supply chain offline: Infrastructure materializou exatamente 40 packages,
  sem `System.Memory`; lockfiles permaneceram logicamente idênticos à
  baseline. As evidências anteriores de assinatura/licença e vulnerabilidade
  continuam aplicáveis porque o grafo não mudou; o gate não acessou rede.
- Toolchains: .NET SDK `10.0.302`, EF CLI `10.0.10`, Git
  `2.55.0.windows.3`, PowerShell `7.6.4`, npm `11.16.0`; Node.js `24.18.1`
  permaneceu somente como variação local já aceita, sem alterar os pins.
- Resultado por gate: `APROVADO`.
- Achados: P0 `0`; P1 `0`; P2 `0`; P3 `0`. Nenhuma correção foi executada
  durante a auditoria.
- Limitações não bloqueantes e de estados futuros: ausência de process-crash
  injection, benchmark/SLA, corpus real, provider/conta, fonte oficial,
  armazenamento operacional e OCI. Nenhuma foi tratada como evidência de
  implementação ou como redução do critério de `STATE-03`.
- Human Gate de `STATE-03`: `PENDENTE` e não executado.
- Estado resultante: `STATE-03` permanece ativo e aberto; `STATE-04` continua
  não autorizado.
- Próxima condição: apresentar o resumo completo da baseline vigente e obter
  a frase canônica do Human Gate exclusivamente para `STATE-03`.
- Autorizador do gate: proprietário do RAG-Challenge.

## 2026-08-02 — Human Gate de STATE-03 aprovado sem ressalvas

- Estado anterior: `STATE-03 DATA_AND_INDEX_MODELING` ativo; `S03-A` e
  `S03-B0` a `S03-B5` concluídos; Automatic Quality Gate aprovado; Human Gate
  pendente.
- Baseline da decisão: branch `main`, commit
  `a88dc1f296bb9117dd8e869b83d1665cee99634f`, corpus `4.9.1`, working tree
  limpa. A execução do Automatic Quality Gate foi auditada sobre
  `main@3d0731fdf3f5004fb185dc760b5f74e4d73b4aa5`; o commit da decisão apenas
  registrou sua evidência factual.
- Resumo completo apresentado na mesma conversa: entregáveis de `S03-A` e
  `S03-B0` a `S03-B5`; resultado automático `APROVADO` com P0 `0`, P1 `0`,
  P2 `0` e P3 `0`; 82 testes .NET; cobertura de 94,83% de linhas e 72,34% de
  branches; Dashboard aprovado; migrations Control e Vector repetidas apenas
  em stores temporários removidos; limitações, riscos residuais, escopo
  negativo e rollback.
- Confirmação humana exata:

  ```text
  Confirmo a decisão acima exclusivamente para STATE-03
  ```

- Decisão: Human Gate de `STATE-03` `APROVADO` sem ressalvas.
- Runtime preflight: `NOT_APPLICABLE`; o registro do Human Gate altera somente
  documentação e memória de lifecycle, sem validar ou mudar comportamento
  executável.
- Estado resultante: `STATE-03 DATA_AND_INDEX_MODELING` encerrado.
- Limite da decisão: a confirmação se aplica exclusivamente a `STATE-03` e
  não autoriza entrada ou execução de `STATE-04`.
- Escopo externo preservado: nenhum provider, conta, corpus real, fonte
  oficial, rede, GitHub, OCI, publicação, deploy, armazenamento operacional ou
  DB-Notifier foi acessado ou autorizado.
- Próxima condição: apresentar uma proposta completa e limitada de entrada em
  `STATE-04 BACKEND_IMPLEMENTATION` e obter autorização humana explícita e
  separada antes de qualquer execução.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-03 — Correção do isolamento temático de respostas e handoffs

- Estado anterior: `STATE-03 DATA_AND_INDEX_MODELING` encerrado; entrada em
  `STATE-04` não autorizada; corpus `4.9.1`.
- Baseline: branch `main`, commit
  `a146deda762e9d39cef0f5518fac6b6e8886d261`, working tree limpa.
- Autoridade do proprietário: `Corrigir isso, pois não é a primeira vez` após
  apontar que a resposta havia misturado a decisão específica sobre hospedagem
  com outro assunto do projeto.
- Mudança: corpo e handoff passam a compartilhar o limite temático do pedido
  atual; follow-up estreito não reintroduz lifecycle, backlog, melhoria
  opcional ou tópico anterior por implicação.
- Formato: `Próximo trabalho recomendado` aceita a ausência canônica quando
  não houver entrega diretamente relacionada; `Estado/gate` usa `sem mudança`
  quando a transição não for material para o tema.
- Corpus resultante: `4.9.2` (`PATCH`).
- Runtime preflight: `NOT_APPLICABLE`; a correção altera somente instruções e
  memória documental.
- Escopo negativo: nenhum código de produto, dependência, runtime, provider,
  corpus real, fonte oficial, GitHub, OCI, publicação, deploy, DB-Notifier,
  gate ou transição foi executado ou autorizado.
- Estado resultante: sem mudança; `STATE-03` permanece encerrado e a entrada
  em `STATE-04` permanece não autorizada.
- Próxima condição: nenhuma decorrente desta correção; cada solicitação futura
  será tratada apenas no próprio limite temático.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-03 — Entrada em STATE-04 registrada sem lote executável

- Estado anterior: `STATE-03 DATA_AND_INDEX_MODELING` encerrado após
  Automatic Quality Gate aprovado sem achados e Human Gate aprovado sem
  ressalvas; entrada em `STATE-04` não autorizada.
- Baseline confirmada antes do registro: `Get-Location`
  `C:\Projects\RAG-Challenge`; Git top-level
  `C:/Projects/RAG-Challenge`; Git directory
  `C:/Projects/RAG-Challenge/.git`; branch `main`; commit
  `e62fbc4da7e580dc1f5449689699374e42ea8ab4`; corpus `4.9.2`; working tree
  limpa.
- Proposta revisada: objetivo, lotes sequenciais `S04-A` a `S04-D`,
  entregáveis, dependências, verificações, critérios de aceite, riscos,
  rollback, escopo permitido, escopo negativo e condições de parada foram
  apresentados na mesma conversa. A proposta não constituiu autoridade de
  entrada ou execução.
- Autoridade humana exata:

  ```text
  Autorizo exclusivamente o registro documental da entrada do RAG-Challenge em STATE-04 BACKEND_IMPLEMENTATION sobre a baseline main@e62fbc4da7e580dc1f5449689699374e42ea8ab4, corpus 4.9.2 e working tree limpa.

  Antes de agir, reconfirme Get-Location, Git top-level, Git directory, branch main, HEAD e working tree; releia AGENTS.md, prompts/Start-Here.md, prompts/state/Current-State.md, prompts/governance/Governance.md, prompts/governance/Lifecycle.md, prompts/governance/Quality-Gates.md e a proposta apresentada nesta conversa.

  Registre a entrada somente em prompts/state/Current-State.md e, de forma append-only, em prompts/state/State-Transition-Log.md. O estado resultante deve ser STATE-04 ativo, sem lote de implementação autorizado. Valide idioma, links, UTF-8/LF, whitespace, diff e coerência factual; depois crie um commit local focal com a mensagem `docs(state): enter state 04 backend implementation`. Reconfirme a baseline final e pare.

  Esta autorização não permite executar S04-A, S04-B, S04-C ou S04-D; alterar código, dependências, packages, lockfiles, migrations, contratos ou ADRs; acessar rede, provider, conta, secret, corpus real ou fonte oficial; usar armazenamento operacional; alterar Dashboard, GitHub, OCI ou DB-Notifier; publicar ou realizar deploy. Qualquer execução de STATE-04 exigirá autorização humana explícita e separada após o registro da entrada e o fechamento das dependências aplicáveis.
  ```

- Interpretação limitada da autoridade: registrar somente a entrada do
  estado em `Current-State.md` e neste histórico append-only, validar o
  incremento e criar um commit local focal. A entrada não concede autoridade
  implícita a nenhum lote, gate, ADR, ação executável ou ação externa.
- Runtime preflight: `NOT_APPLICABLE`; o registro altera somente documentação
  e memória de lifecycle, sem alterar ou validar comportamento executável.
- Mudanças: o snapshot factual registra `STATE-04 BACKEND_IMPLEMENTATION`
  ativo sem lote autorizado; esta entrada preserva a autoridade, a baseline,
  o escopo negativo e a condição de continuidade.
- Escopo negativo preservado: nenhuma execução de `S04-A`, `S04-B`, `S04-C`
  ou `S04-D`; nenhum código, dependência, package, lockfile, migration,
  contrato, ADR, rede, provider, conta, secret, corpus real, fonte oficial,
  armazenamento operacional, Dashboard, GitHub, OCI, publicação, deploy ou
  DB-Notifier foi acessado, alterado ou autorizado.
- Automatic Quality Gate de `STATE-04`: `PENDENTE` e não executado.
- Human Gate de `STATE-04`: `PENDENTE` e não solicitado.
- Estado resultante: `STATE-04 BACKEND_IMPLEMENTATION` ativo exclusivamente
  pelo registro documental de entrada; nenhum lote de implementação está
  autorizado.
- Próxima condição: fechar as dependências aplicáveis sob autoridade própria
  e obter autorização humana explícita e separada para o lote ou envelope
  sequencial pretendido antes de qualquer execução.
- Aprovador da entrada: proprietário do RAG-Challenge.

## 2026-08-04 — S04-A0 encerrado; execução interrompida antes do pin offline

- Estado anterior: `STATE-04 BACKEND_IMPLEMENTATION` ativo somente pelo
  registro documental de entrada; nenhum lote executável autorizado.
- Baseline inicial confirmada: branch `main`, commit
  `fe6c9028f061a7f0a98fc3debecffb0de3ad69bc`, corpus `4.9.2`, working tree
  limpa.
- Autoridade do proprietário: encerrar documentalmente `S04-A0`, fixar
  offline `PdfPig` `0.1.15` e `CsvHelper` `33.1.0`, executar sequencialmente
  `S04-A` a `S04-D` e, somente depois, o Automatic Quality Gate de
  `STATE-04`. `Sylvan.Data.Csv` `1.4.4` permanece fallback não selecionado.
- Decisão de `S04-A0`: `PdfPig` `0.1.15` e `CsvHelper` `33.1.0` são os
  candidatos selecionados condicionalmente para desenvolvimento local; o
  provider OpenAI deve usar adapter HTTP direto, sem packages `OpenAI` ou
  `System.ClientModel`.
- Gates preservados: hash integral do nupkg contra o valor publicado; hash do
  nupkg no cache contra o hash integral; `contentHash` do lockfile contra o
  signed content hash preservado; identidade, versão, grafo, TFM e assets
  exatos; assinatura `CONDITIONAL_REVOCATION_NOT_CURRENT`.
- Limitação aceita: as fontes primárias consultadas não definem completamente
  `packages.lock.json` `contentHash`, signed content hash e `.nupkg.sha512`.
  A aceitação vale exclusivamente para desenvolvimento local de `STATE-04`,
  não estabelece semântica normativa do NuGet e não aprova produção.
- Evidência durável: hashes, evidência observada, limitações, riscos, primeiro
  gate runtime e retenção estão no
  [relatório de STATE-04](../../docs/STATE-04-Backend-Implementation-Report.md).
- Bloqueio observado antes do pin: a fonte offline preservada contém somente
  os três packages candidatos de parsing. Os lockfiles vigentes do produto
  exigem dependências .NET adicionais; rede e acesso ao cache NuGet global
  estavam explicitamente proibidos. Um restore isolado válido não poderia ser
  concluído com a fonte autorizada.
- Ação executada: somente o fechamento documental e sanitizado de `S04-A0`.
  Nenhum PackageReference, versão central, lockfile, código, teste ou contrato
  foi alterado, e nenhum `dotnet`, restore, build, loading ou teste foi
  iniciado.
- Runtime preflight: `NOT_APPLICABLE` para este incremento exclusivamente
  documental; nenhum processo ou listener foi inspecionado.
- Estado resultante: `STATE-04 BACKEND_IMPLEMENTATION` permanece ativo;
  `S04-A` a `S04-D` e o Automatic Quality Gate possuem autoridade sequencial,
  mas permanecem não executados e condicionados a uma fonte offline isolada
  completa.
- Próxima condição: autoridade explícita para semear um cache novo, de forma
  somente leitura e allowlisted, a partir dos packages já fixados no cache
  global, ou para obter uma fonte offline exata equivalente. A autoridade
  sequencial preservada pode ser retomada somente após essa precondição e a
  reconciliação da baseline.
- Escopo negativo preservado: sem rede, provider, conta, secret, corpus real,
  fonte oficial real do produto, GitHub, OCI, Dashboard, DB-Notifier,
  publicação, deploy, Human Gate ou estado posterior.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-04 — Cache offline semeado; pins e primeiro gate runtime aprovados

- Estado anterior: `STATE-04 BACKEND_IMPLEMENTATION` ativo, `S04-A0`
  encerrado documentalmente e execução interrompida antes do pin por fonte
  offline incompleta.
- Baseline inicial: branch `main`, commit
  `1bb4368ff59521ab61b0d9224b806805e84c6287`, corpus `4.9.2`, working tree
  limpa.
- Autoridade: acesso somente leitura aos paths determinísticos das identidades
  e versões não-project já registradas nos lockfiles; cópia para cache de
  tarefa isolado; pins de `PdfPig` `0.1.15` e `CsvHelper` `33.1.0`; retomada
  sequencial do envelope de `STATE-04` somente após aprovação do seed.
- Seed observado: 53 pares package/versão, 2.189 arquivos e 370.721.153 bytes
  foram copiados e comparados por SHA-256. Nupkg, `.nupkg.sha512`, NuSpec,
  metadados de cache, identidade, versão, content hash, estrutura expandida e
  ausência de reparse points foram validados. O cache global não foi alterado.
- Fonte dos parsers: continha exclusivamente os nupkgs preservados de
  `PdfPig` `0.1.15` e `CsvHelper` `33.1.0`; `Sylvan.Data.Csv` não foi copiado,
  referenciado, restaurado ou instalado.
- Restore offline: somente os dois parsers foram acrescentados à união de 53
  packages preexistentes. O grafo aplicável de cada parser ficou vazio;
  `PdfPig` selecionou os sete assets previamente inventariados de `lib/net8.0`
  e `CsvHelper` selecionou `lib/net9.0/CsvHelper.dll`. O segundo restore
  locked não alterou os sete lockfiles.
- Gates de hash: `RAW_NUPKG_HASH`, `CACHE_NUPKG_HASH`,
  `SIGNED_CONTENT_HASH` e `LOCK_CONTENT_HASH` passaram em seus domínios
  independentes; assinatura permanece
  `CONDITIONAL_REVOCATION_NOT_CURRENT`.
- Primeiro gate de `S04-A`: adapters limitados a streams em memória passaram
  PDFs válidos de uma e duas páginas, PDF truncado, PDF oversize, CSV quoted
  UTF-8, fórmula literal `=1+1`, aspas malformadas e CSV oversize. Entradas
  oversize foram recusadas antes da leitura pelo parser, inputs malformados
  foram recusados por guards explícitos e nenhuma assembly externa ou escrita
  inesperada foi observada.
- Verificação: format check aprovado; build Release com zero warnings e zero
  erros; 83 testes aprovados, sendo 56 unitários, 10 de arquitetura e 17 de
  integração.
- Runtime preflight: aprovado antes do primeiro `dotnet`, sem processo ou
  listener RAG-Challenge encontrado ou encerrado.
- Estado resultante: pins aplicados; `S04-A` em execução sequencial. `S04-B`,
  `S04-C`, `S04-D` e Automatic Quality Gate permanecem pendentes e ordenados.
- Escopo negativo preservado: sem rede, provider, conta, secret, corpus real,
  fonte oficial real, GitHub, OCI, Dashboard, DB-Notifier, publicação, deploy,
  Human Gate ou estado posterior.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-04 — S04-A concluído localmente com fixtures sintéticas

- Estado anterior: pins aprovados e `S04-A` em execução sequencial.
- Escopo implementado: administração de revisões de catálogo com expected
  revision, contexto de auditoria bounded e idempotência; ingestão PDF/CSV em
  conteúdo imutável reaberto por hash; parsing e chunks determinísticos; sync
  oficial por transporte falso; snapshots e observações append-only.
- Segurança: os parsers recebem somente streams em memória; URL é resolvida
  exclusivamente do registro confiável; inputs e outputs são limitados;
  reason, validators e status contribuem somente para digest de auditoria;
  falha de parsing ocorre antes do commit de snapshot/observação.
- Idempotência: bytes repetidos reutilizam o mesmo objeto; replay da operação
  de catálogo retorna `AlreadyApplied`; hash oficial idêntico cria nova
  observação sem novo snapshot. A deduplicação no mesmo DbContext foi
  corrigida para aceitar dois documentos referenciando o mesmo conteúdo.
- Evidência integrada: conteúdo local repetido produziu o mesmo hash/chunks;
  o primeiro sync criou um snapshot; o segundo criou somente uma observação;
  conteúdo CSV malformado não alterou a contagem de snapshots ou observações.
- Verificação: format check aprovado; build Release com zero warnings e zero
  erros; 84 testes aprovados, sendo 56 unitários, 10 de arquitetura e 18 de
  integração.
- Fontes: somente fixtures sintéticas e transporte falso em processo; nenhum
  listener, rede, provider ou fonte oficial real foi acessado.
- Estado resultante: `S04-A` concluído; `S04-B` é o próximo lote sequencial.
- Escopo negativo preservado: sem migration, package adicional, provider,
  corpus real, GitHub, OCI, Dashboard, DB-Notifier, publicação, deploy, Human
  Gate ou estado posterior.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-04 — S04-B concluído com indexação e ativação determinísticas

- Estado anterior: `S04-A` concluído; `S04-B` era o próximo lote sequencial.
- Escopo implementado: port de embeddings com descritor observado, fake
  determinístico, staging inativo, batches limitados, finalização canônica,
  commit do manifesto e ativação explícita por compare-and-swap.
- Hard pre-filter: a busca exige corpus, geração e bindings elegíveis; filtros
  administrativos opcionais de database/documento são convertidos em pares
  document/version e aplicados no SQLite antes de carregar vetores, ranquear
  por cosine ou selecionar top-k.
- Idempotência: candidato, chunks, commit de geração e ativação aceitam replay
  exato; divergência de input imutável é rejeitada e nenhuma promoção parcial
  substitui a ativação vigente.
- Evidência integrada: staging permaneceu sem ativação; finalização e commit
  precederam CAS; filtro não autorizado retornou vazio; corpus divergente foi
  recusado; o replay preservou geração e revisão de ativação.
- Verificação: format check aprovado; build Release com zero warnings e zero
  erros; 85 testes aprovados, sendo 56 unitários, 10 de arquitetura e 19 de
  integração.
- Fontes: somente fake determinístico e dados sintéticos em processo; nenhum
  listener, rede ou provider foi acessado.
- Estado resultante: `S04-B` concluído; `S04-C` é o próximo lote sequencial.
- Escopo negativo preservado: sem migration, package adicional, provider real,
  corpus real, GitHub, OCI, Dashboard, DB-Notifier, publicação, deploy, Human
  Gate ou estado posterior.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-04 — S04-C concluído com recuperação grounded bilíngue

- Estado anterior: `S04-B` concluído; `S04-C` era o próximo lote sequencial.
- Escopo implementado: validação de pergunta/idioma; resolução única da revisão
  ativa; elegibilidade por observação bound; recuperação em todo o conjunto
  elegível; cobertura/proveniência; geração constrained; citações validadas;
  recusa explícita por evidência insuficiente.
- Integridade: o query path usa somente bindings e a observação nomeada no
  `CorpusActivationRecord`; não lê “última observação”, não consulta fonte
  oficial e não permite que pergunta, evidência ou modelo escolham autoridade.
- Citações: são reconstruídas de metadados locais, preservam `contentLanguage`
  e texto original e carregam identidade de corpus/geração/database/documento,
  formato, trust e localização PDF/CSV; URL/snapshot/freshness aparecem somente
  para origem oficial validada.
- Compatibilidade física: localização do chunk foi codificada de forma bounded
  na coluna derivada existente e decodificada no readback canônico e recovery;
  nenhuma migration ou mudança de schema foi necessária.
- Evidência: testes cobrem `pt-BR→pt-BR`, `en-GB→en-GB`, `pt-BR→en-GB` e
  `en-GB→pt-BR`, origem local PDF/oficial CSV, injection tratada como dados,
  evidência insuficiente, stale, provider down e citação não suportada.
- Verificação: format check aprovado; build Release com zero warnings e zero
  erros; 96 testes aprovados, sendo 67 unitários, 10 de arquitetura e 19 de
  integração.
- Estado resultante: `S04-C` concluído; `S04-D` é o próximo lote sequencial.
- Escopo negativo preservado: sem package, migration, rede, provider real,
  corpus real, GitHub, OCI, Dashboard, DB-Notifier, publicação, deploy, Human
  Gate ou estado posterior.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-04 — S04-D concluído com API pública v1 fail-closed

- Estado anterior: `S04-C` concluído; `S04-D` era o próximo lote sequencial.
- Superfície: somente `POST /api/v1/questions`, `GET /api/v1/health/live` e
  `GET /api/v1/health/ready`; administração, provider, URL, modelo, adapter e
  estado do Dashboard não são autoridade pública.
- Contrato: OpenAPI 3.1 v1 versionado; request/response, citações, cobertura,
  readiness e Problem Details possuem schemas transport-only e rejeitam
  campos desconhecidos.
- Limites: 8 KiB por body, 4 KiB UTF-8 por pergunta, deadline de 25 segundos,
  cancelamento propagado, máximo de 20 consultas concorrentes e token bucket
  de 30 por minuto por chave derivada, com burst 10 e fila zero.
- Falhas e health: taxonomia pública mapeada para códigos `CH_*` sanitizados;
  liveness não depende de serviço externo e readiness fica `Unready` até uma
  composição explícita fornecer probe sanitizado completo.
- Providers: adapters OpenAI por HTTP direto, sem `OpenAI` ou
  `System.ClientModel`, com rotas exatas, JSON limitado, `store=false`, sem
  redirects, proxy ou decompression. Somente handler falso foi exercitado.
- Verificação: 27 testes focais aprovados; build Release com zero warnings e
  zero erros; suíte completa com 118 testes aprovados — 67 unitários, 10 de
  arquitetura e 41 de integração.
- Estado resultante: `S04-D` concluído; o Automatic Quality Gate de `STATE-04`
  é a próxima ação autorizada e deve preceder qualquer decisão humana.
- Escopo negativo preservado: sem rede, provider real, conta, secret, corpus
  real, fonte oficial real, GitHub, OCI, Dashboard, DB-Notifier, publicação,
  deploy, Human Gate ou estado posterior.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-04 — Automatic Quality Gate de STATE-04 aprovado

- Baseline auditada: `main@7f236542133719481a02f507cf802a1dd385f328`,
  corpus `4.9.2` e Git tree
  `8ef34586d567008510d2d633a927ebc9a9d7f766`.
- Sequência confirmada: pin de parsers, `S04-A`, `S04-B`, `S04-C` e `S04-D`
  foram integrados em cinco commits focais e na ordem autorizada; um sexto
  commit focal contém somente a correção de evidência do gate.
- Build e testes: format aprovado; build Release no SDK .NET `10.0.302` com
  zero warnings e zero erros; 119 testes aprovados — 67 unitários, 10 de
  arquitetura e 42 de integração.
- Cobertura: 92,37% de linhas (10.441/11.303) e 65,73% de branches
  (1.260/1.917), acima dos pisos de 70% e 45%.
- Supply chain: hashes raw dos dois nupkgs selecionados revalidados; sete
  lockfiles preservam versões e content hashes aceitos; nenhum package
  Sylvan, `OpenAI` ou `System.ClientModel` foi referenciado. Revogação de
  assinatura permanece `CONDITIONAL_REVOCATION_NOT_CURRENT`.
- Contratos: OpenAPI contém exatamente as três rotas aprovadas e possui
  SHA-256 `D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34`;
  API, health, falhas `CH_*`, limites e adapters HTTP fake passaram.
- Higiene: auditoria local de 148 arquivos não ignorados e fora do Dashboard
  aprovou UTF-8/LF, newline final, whitespace, links, material privado e busca
  de secrets aparentes. Dashboard permaneceu fora do escopo explícito.
- Achado: `AQG-S04-001` (P2) identificou ausência inicial de um teste único
  que atravessasse o fluxo sintético completo. O teste foi acrescentado sob a
  autoridade corretiva local, passou e tornou o achado `RESOLVIDO`.
- Resultado: `APROVADO`, sem achados abertos; nenhum P0, P1 ou P3 foi
  identificado.
- Limitações: sem rede, revogação atual, provider real, corpus real, fonte
  oficial real, runtime Linux ARM64, listener E2E, benchmark, GitHub, OCI,
  Dashboard, DB-Notifier, publicação ou deploy.
- Estado resultante: `STATE-04 BACKEND_IMPLEMENTATION` permanece ativo; Human
  Gate pendente. Não houve entrada em `STATE-05`.
- Resultado automático: emitido pelo processo local de Quality Gate sob
  autoridade do proprietário; decisão humana não executada.

## 2026-08-04 — Human Gate de STATE-04 aprovado com ressalvas documentadas

- Estado anterior: `STATE-04 BACKEND_IMPLEMENTATION` ativo; `S04-A0`, pin de
  parsers e `S04-A` a `S04-D` concluídos; Automatic Quality Gate `APROVADO`;
  Human Gate `PENDENTE`.
- Baseline da decisão: branch `main`, commit
  `6d141decdf5f40661bb9f408d6aa97f9f322cfcf`, corpus `4.9.2` e working tree
  limpa. O Automatic Quality Gate foi executado sobre
  `main@7f236542133719481a02f507cf802a1dd385f328`; o commit posterior registrou
  somente a evidência documental do gate.
- Pré-condições: resumo completo apresentado na mesma conversa, incluindo
  entregáveis de `S04-A0` e `S04-A` a `S04-D`, resultado automático, amostras
  críticas, hashes e lockfiles, limitações, riscos residuais, rollback, escopo
  negativo e decisão proposta.
- Confirmação humana exata:

  ```text
  Confirmo a decisão acima exclusivamente para STATE-04
  ```

- Decisão: Human Gate de `STATE-04` `APROVADO COM RESSALVAS`, exclusivamente
  para o escopo local e offline entregue pelo estado.
- Entregáveis aceitos: parsers `PdfPig` `0.1.15` e `CsvHelper` `33.1.0` com
  pins exatos; administração e ingestão PDF/CSV; sincronização manual por
  transporte falso; snapshots, chunks e idempotência; indexação, hard
  pre-filter, ativação CAS e embeddings fake; recuperação, recusa, respostas
  grounded, citações e matriz `pt-BR`/`en-GB`; API v1, OpenAPI, Problem
  Details, limites, cancelamento, rate limit, health e adapters OpenAI por HTTP
  direto exercitados somente com fakes.
- Evidências aceitas: build Release sem warnings ou erros; 119 testes — 67
  unitários, 10 de arquitetura e 42 de integração — sem falha ou skip; 92,37%
  de linhas e 65,73% de branches; fluxo sintético completo; OpenAPI com três
  rotas; supply chain e sete lockfiles; higiene local; `AQG-S04-001` (P2)
  `RESOLVIDO`; nenhum achado aberto.
- Ressalvas aceitas: assinatura NuGet permanece
  `CONDITIONAL_REVOCATION_NOT_CURRENT`; a relação normativa entre todos os
  domínios de hash NuGet permanece incompleta e aceita somente para
  desenvolvimento local; runtime Linux ARM64, provider/conta reais, corpus e
  fontes oficiais reais, listener E2E, benchmark, deploy e recuperação
  operacional não foram verificados; o host padrão permanece `Unready` sem
  composição operacional explícita.
- Retenção: todas as evidências temporárias permanecem preservadas; sua
  limpeza exige autoridade posterior e separada. Nupkgs, catálogos brutos,
  caches, assemblies restauradas, launchers, logs temporários e paths locais
  continuam não versionáveis.
- Runtime preflight: `NOT_APPLICABLE`; o registro altera somente documentação
  e memória de lifecycle e não inspeciona nem muda comportamento executável.
- Escopo negativo preservado: nenhuma entrada ou execução de `STATE-05`,
  produção, rede, provider, conta, secret, corpus real, fonte oficial real,
  GitHub, OCI, Dashboard, DB-Notifier, publicação, deploy ou ação externa.
- Estado resultante: `STATE-04 BACKEND_IMPLEMENTATION` encerrado; `STATE-05
  FRONTEND_IMPLEMENTATION` permanece sem autorização de entrada.
- Próxima condição: preparar proposta limitada e obter autorização humana
  explícita e separada para qualquer entrada em `STATE-05`.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-04 — S04-CORR-01 implementado; gate corretivo pendente

- Estado anterior: `STATE-04 BACKEND_IMPLEMENTATION` encerrado após Automatic
  Quality Gate aprovado e Human Gate aprovado com ressalvas; nenhum estado
  posterior autorizado.
- Baseline da auditoria: branch `main`, commit
  `f71343291b942c66d0ff417a8764b032bbd63bff`, corpus `4.9.2` e working tree
  limpa.
- Auditoria: a primeira passagem local e offline identificou
  `AUD-S04-001` a `AUD-S04-004` e parou conforme a condição de achado antes de
  completar a matriz integral.
- Autoridade corretiva: implementação consolidada e sequencial de
  `S04-CORR-01`, Automatic Quality Gate corretivo e, somente após aprovação
  integral desse gate, retomada completa da auditoria. `STATE-05` permaneceu
  explicitamente proibido.
- C1: commit `a674560ed1093e96d533012f1b11a292c3f641b5` implementa o
  rebinding transacional de observação em `304`/hash idêntico e a nova revisão
  completa de ativação, com replay exato e falhas atômicas testadas.
- C2: commit `b875eac6e9ce4c72783d4e4bb72a59686ca58248` alinha o chunking ao
  contrato integral `paragraph-window-v1`.
- C3: commit `ac34c085a499a34ea8ee1c9106675482e38790c3` implementa a
  administração one-shot governada, lifecycle estrito por comando, lease por
  corpus, idempotência por intenção estável, input limitado e journal durável;
  a conclusão de mutações bem-sucedidas participa da mesma transação.
- C4: Current State, relatório proprietário e este histórico foram
  reconciliados; a memória factual não afirma mais ausência do backend já
  entregue e separa o Human Gate histórico da auditoria corretiva posterior.
- Dependências e escopo: nenhuma nova dependência, package, provider, corpus
  real, ação externa, publicação ou deploy foi introduzido; Dashboard, GitHub,
  OCI e DB-Notifier permaneceram fora do acesso.
- Disposição: `AUD-S04-001` a `AUD-S04-003` estão
  `IMPLEMENTADOS_PENDENTES_DE_VALIDAÇÃO`; `AUD-S04-004` está
  `RECONCILIADO_PENDENTE_DE_VALIDAÇÃO`. Nenhum deles está resolvido antes do
  gate corretivo e da auditoria retomada.
- Estado resultante: `STATE-04` permanece encerrado; Automatic Quality Gate
  corretivo é a próxima etapa autorizada; Human Gate não foi repetido e
  `STATE-05` continua sem autorização.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-04 — Automatic Quality Gate corretivo de STATE-04 aprovado

- Baseline auditada: `main@114ea6f7f76936dac991553588660fc986bd0f10`,
  Git tree `b4d14fab9346574d7db7d92c11ca1e5c0ee363d4`, corpus `4.9.2` e working
  tree limpa.
- Sequência corretiva: C1, C2, C3 e C4 preservados nos commits focais
  `a674560ed1093e96d533012f1b11a292c3f641b5`,
  `b875eac6e9ce4c72783d4e4bb72a59686ca58248`,
  `ac34c085a499a34ea8ee1c9106675482e38790c3` e
  `114ea6f7f76936dac991553588660fc986bd0f10`.
- Preflight e isolamento: nenhum processo ou listener RAG-Challenge estava
  ativo; restore, cache, CLI home, resultados e artefatos ficaram isolados e
  sem fonte NuGet ou proxy.
- Build e testes: format aprovado; build Release no SDK .NET `10.0.302` com
  zero warnings e zero erros; 150 testes aplicáveis aprovados — 74 unitários,
  67 de integração e 9 de arquitetura — sem falha ou skip. O teste exclusivo
  do Dashboard foi `NÃO APLICÁVEL`.
- Cobertura: 92,26% de linhas (16.580/17.970) e 65,07% de branches
  (2.079/3.195), acima dos pisos de 70% e 45%.
- Migrations: Control e Vector sem mudança de modelo pendente; upgrade e
  backfill aprovados por integração; migration do journal limitada a uma nova
  tabela e sem operação destrutiva no `Up`.
- Supply chain: hashes raw D1/cache e content hashes dos lockfiles de `PdfPig`
  `0.1.15` e `CsvHelper` `33.1.0` aprovados; grafo aplicável vazio; nenhuma
  mudança de package no incremento. Assinatura permanece
  `CONDITIONAL_REVOCATION_NOT_CURRENT`.
- Contratos e higiene: OpenAPI conserva três rotas e SHA-256
  `D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34`;
  nenhuma rota HTTP administrativa foi encontrada; 161 arquivos fora do
  Dashboard passaram encoding, whitespace, links, material privado e busca de
  secrets aparentes; working tree final limpa.
- Limitação de orquestração: uma primeira execução unitária com artefatos fora
  da árvore não localizou duas fixtures rastreadas e não foi aceita como
  evidência. As duas cópias temporárias foram verificadas por SHA-256 e a
  execução válida posterior aprovou 74/74 sem alterar produto ou teste.
- Resultado: `APROVADO`, sem P0/P1 ou falha de gate. `AUD-S04-001` a
  `AUD-S04-004` estão `CORRIGIDOS_PENDENTES_DE_DISPOSIÇÃO`; a auditoria
  completa retomada é a próxima etapa autorizada e única que pode encerrá-los.
- Lifecycle: `STATE-04` permanece encerrado; Human Gate não foi repetido;
  `STATE-05` continua sem autorização.
- Escopo negativo preservado: sem rede, provider, conta, secret, corpus real,
  fonte oficial real, listener, Dashboard, GitHub, OCI, DB-Notifier,
  publicação ou deploy.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-04 — S04-CORR-02 e S04-CORR-03 aprovados; auditoria integral concluída

- Estado preservado: `STATE-04 BACKEND_IMPLEMENTATION` continuou encerrado
  após o Human Gate histórico; nenhum estado posterior foi autorizado ou
  executado.
- Baseline inicial de `S04-CORR-02`:
  `main@405d386fc4a4433ac7922e620706c082c0835ab9`, corpus `4.9.2` e working
  tree limpa.
- `S04-CORR-02`: os commits
  `7299722b4259c7384287e5b86f1eec65626a6842`,
  `8c661ba094302c551182a6da853306036b50b83d`,
  `a4baa22052d7c0fd7787d44820d6a2471a6f5d65`,
  `c230c80bd6bdb19752ec7d6f4fb4aec5c76b7ae3` e
  `3e9d6f9b2c7d7a92d9f1cbaf94d55490bd564092` corrigiram
  `AUD-S04-005` a `AUD-S04-009`: alcance global de referências antes da
  limpeza, replay persistido exato, validação e falhas tipadas dos adapters
  OpenAI, classificação administrativa por fase e comentários obsoletos.
- Residual: a auditoria reiniciada encontrou `AUD-S04-005-R1`, no qual uma
  reserva sobrevivente a crash poderia voltar a ser referenciada antes da
  finalização física incondicional. A auditoria parou antes de corrigir, como
  exigido.
- `S04-CORR-03`: sobre
  `main@3e9d6f9b2c7d7a92d9f1cbaf94d55490bd564092`, o commit focal
  `19889f560dad0f011006ff17fc7414c807838149` adicionou plano interno
  `cleanup-plan-v1`, inventário tipado, reconciliação transacional antes do
  planejamento e da finalização, restauração de conteúdo novamente alcançável,
  dupla revalidação global, contenção TOCTOU, replay exato e comportamento
  fail-closed. Nenhuma migration, dependência, package, lockfile, contrato
  público, OpenAPI ou ADR mudou.
- Automatic Quality Gate de `S04-CORR-03`: `APROVADO` sobre o Git tree
  `40b04e737ebea6e00dab003ff2403e4aa94c4ad2`. Restore locked offline, format
  e build Release passaram; 169 testes aplicáveis foram aprovados — 74
  unitários, 86 de integração e 9 de arquitetura — sem falha ou skip; a
  cobertura foi 92,04% de linhas (17.423/18.929) e 66,46% de branches
  (2.421/3.643).
- Supply chain e contratos: sete lockfiles permaneceram byte a byte estáveis;
  hashes D1/cache de `PdfPig` `0.1.15` e `CsvHelper` `33.1.0` corresponderam;
  a assinatura continua `CONDITIONAL_REVOCATION_NOT_CURRENT`; OpenAPI manteve
  somente três rotas e SHA-256
  `D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34`.
- Auditoria completa: reiniciada do começo, em modo somente leitura, após o
  gate. Autoridade, lifecycle, commits, arquitetura, dependências, parsers,
  persistência, limpeza, indexação, ativação, hard pre-filter, recuperação,
  geração, citações, idiomas, API, health, limites, cancelamento, rate limit,
  adapters HTTP, configuração, segurança, testes, cobertura, integração,
  documentação e higiene foram aprovados.
- Disposição: `AUD-S04-001` a `AUD-S04-009`, incluindo
  `AUD-S04-005-R1`, estão `RESOLVIDOS`. Nenhum novo P0, P1, P2 ou P3 foi
  identificado.
- Limitações preservadas: sem revogação online atual, sem semântica normativa
  completa dos hashes NuGet, sem runtime Linux ARM64, provider/conta reais,
  corpus ou fonte oficial reais, listener E2E, benchmark, recuperação
  operacional, Dashboard, GitHub, OCI, DB-Notifier, publicação ou deploy.
- Evidências: todos os artefatos temporários de `S04-A0` e das correções
  permanecem preservados e não versionáveis até autoridade específica de
  limpeza.
- Resultado: `STATE-04` permanece encerrado; o Human Gate não foi repetido e
  `STATE-05` continua sem autorização.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-04 — Entrada documental em STATE-05 FRONTEND_IMPLEMENTATION autorizada

- Estado anterior: `STATE-04 BACKEND_IMPLEMENTATION` encerrado após Automatic
  Quality Gate e Human Gate aprovados com as ressalvas documentadas; auditoria
  corretiva completa aprovada e todos os achados `AUD-S04-*` resolvidos;
  `STATE-05` ainda sem autorização.
- Baseline da decisão: branch `main`, commit
  `23c5bc56e3cb06d9beaeb1af974c51668731ab01`, corpus `4.9.2` e working tree
  limpa, reconfirmados imediatamente antes da alteração documental.
- Preparação revisada: proposta completa e limitada apresentada na mesma
  conversa, com objetivo, lotes sequenciais `S05-A0` a `S05-A4`, contrato da
  API v1, entregáveis, dependências, verificações, acessibilidade, localização
  `pt-BR`/`en-GB`, temas `Light`/`Dark`, cobertura, proveniência, citações,
  falhas, testes, aceite, riscos, rollback, escopos e condições de parada.
- Decisão humana: o proprietário revisou e aprovou a proposta e autorizou
  exclusivamente registrar documentalmente a entrada em `STATE-05` sobre a
  baseline identificada. A decisão não autoriza nenhum lote executável.
- Decisões de frontend aprovadas, mas não implementadas: interface inicia em
  `pt-BR`, persiste somente a seleção explícita e usa fallback `pt-BR`;
  `questionLanguage` permanece independente. Na ausência de tema persistido,
  a preferência do sistema seleciona `Light` ou `Dark`; `Light` é o fallback,
  e escolha explícita de tema permanece independente de idioma e consulta.
- Mudanças autorizadas: somente atualizar
  `prompts/state/Current-State.md`, acrescentar esta entrada append-only em
  `prompts/state/State-Transition-Log.md` e criar um commit local focal.
- Runtime preflight: `NÃO APLICÁVEL`; a ação altera apenas documentação e
  memória de lifecycle, sem inspecionar ou validar comportamento executável.
- Verificações: identidade do workspace, Git top-level, Git directory,
  branch, HEAD, corpus e working tree foram reconfirmados; revisão limitada a
  diff, formato, links locais e higiene dos dois documentos alterados.
- Escopo negativo: sem implementação ou execução do frontend, Dashboard,
  código, dependência, package, lockfile, contrato, OpenAPI, ADR, `dotnet`,
  teste, instalação, rede, provider, conta, secret, corpus real, fonte
  oficial, GitHub, OCI, publicação, deploy, DB-Notifier, Automatic Quality
  Gate, Human Gate ou estado posterior.
- Estado resultante: `STATE-05 FRONTEND_IMPLEMENTATION` ativo exclusivamente
  no lifecycle documental; `S05-A0` a `S05-A4` e qualquer outra execução
  permanecem sem autorização.
- Próxima condição: apresentar a nova baseline limpa e obter autorização
  humana posterior, explícita e separada, que nomeie os lotes executáveis,
  checks, runtime permitido, escopo negativo e condições de parada.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-04 — S05-A0 a S05-A4 concluídos localmente

- Estado preservado: `STATE-05 FRONTEND_IMPLEMENTATION` permaneceu ativo; o
  Automatic Quality Gate, o Human Gate e `STATE-06` não foram autorizados nem
  executados.
- Baseline inicial: branch `main`, commit
  `cab336ada60866083f3e688fe1a13cff348a3335`, corpus `4.9.2` e working tree
  limpa, reconfirmados imediatamente antes da primeira alteração.
- Autoridade: execução local, offline, sequencial e limitada de `S05-A0` a
  `S05-A4`, com alterações somente no Dashboard, testes próprios, relatório e
  memória factual; instalação, rede externa, backend, contratos, dependências
  e estados posteriores permaneceram excluídos.
- `S05-A0`: o commit `9c27cc49442ff467486c93febf7144e6d3a652b7`
  congelou tipos e validação do cliente v1, estados determinísticos,
  preferências versionadas e fixtures sintéticas.
- `S05-A1`: o commit `2fd7526f0907361d6c03552379341b877e88c236`
  implementou shell semântico, localização `pt-BR`/`en-GB`, seletores
  independentes, temas `Light`/`Dark`, foco e regras responsivas.
- `S05-A2`: o commit `7a42d332ddf6646c575c7cae16cfe9085120e18d`
  implementou validação UTF-8, cliente HTTP same-origin fail-closed,
  cancelamento, proteção contra resposta tardia e fluxo de consulta.
- `S05-A3`: o commit `a8835b94ab485e542f7cfe23355283c92de17fc8`
  implementou resposta e insuficiência de evidência, cobertura avaliada,
  proveniência local/oficial, citações PDF/CSV, 12 falhas localizadas, escape
  de saída e semântica acessível.
- `S05-A4`: o commit `5865a225cdab9bd92f9befa00c7ee581b2aa0877`
  concluiu a matriz das oito combinações, mensagens de falha e contraste
  textual WCAG AA e preparou a verificação final.
- Verificação final: `npm run lint`, `npm run typecheck`, `npm test` e
  `npm run build` passaram na instalação existente; foram 28 testes, sem
  falha, skip ou cancelamento, e 20 módulos transformados no build. Nenhum
  comando de instalação ou `dotnet` foi executado.
- Contratos e dependências: `package.json`, `package-lock.json` e OpenAPI
  permaneceram sem diff; o OpenAPI conservou SHA-256
  `D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34`.
- Browser local: preflight dirigido encontrou apenas o runtime do Codex, que
  não pertence ao produto. Um listener da tarefa em `127.0.0.1:4173` validou
  o build com stylesheet sob CSP, localização, tema, validação, idioma da
  pergunta, contagem UTF-8, falha incompatível segura, foco, landmarks,
  rótulos e skip link; os dois processos Vite usados foram identificados e
  encerrados, sem listener residual.
- Limitações: Node.js observado `24.18.1` contra pin `24.18.0`; sem percentuais
  de cobertura JavaScript por ausência de instrumentação instalada; sem
  screenshot aproveitável do build estilizado; o override de viewport estreito
  do navegador não foi aplicado; sem engine externa de acessibilidade; sem API,
  provider, corpus ou fonte real.
- Resultado: `S05-A0` a `S05-A4` concluídos dentro do escopo autorizado, sem
  P0/P1 observado. O relatório factual pertence a
  [`docs/STATE-05-Frontend-Implementation-Report.md`](../../docs/STATE-05-Frontend-Implementation-Report.md).
- Escopo negativo preservado: sem dependência, package, lockfile, contrato,
  OpenAPI, ADR, backend, Domain, Application, Infrastructure, API, provider,
  instalação, rede externa, conta, secret, corpus real, fonte oficial real,
  GitHub, OCI, publicação, deploy, DB-Notifier, Automatic Quality Gate, Human
  Gate ou estado posterior.
- Próxima condição: nova baseline limpa e autorização humana explícita,
  posterior e separada, limitada ao Automatic Quality Gate de `STATE-05`.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-05 — Automatic Quality Gate de STATE-05 reprovado

- Estado preservado: `STATE-05 FRONTEND_IMPLEMENTATION` permaneceu ativo; o
  Human Gate e `STATE-06` não foram autorizados nem executados.
- Baseline do gate: branch `main`, commit
  `f6df67a67657af891e4831a616b142d8da9fb584`, corpus `4.9.2` e working tree
  limpa, reconfirmados antes da auditoria.
- Autoridade: Automatic Quality Gate local, offline e sequencial, sem correção
  automática, mudança de produto ou teste, instalação, rede externa, ação
  remota, Human Gate ou estado posterior.
- Escopo inicial: inspeção de autoridade, lifecycle, diff, contrato cliente,
  saída não confiável, segurança, acessibilidade, cobertura e
  reprodutibilidade; checks npm e browser loopback somente enquanto nenhuma
  condição de parada fosse encontrada.
- Preflight runtime: antes da reprodução executável, foi observado somente o
  runtime de controle do navegador do Codex associado ao workspace, sem
  listener. Ele não era processo do produto e não foi encerrado.
- `AQG-S05-001` (P1): o decoder aplica validação HTTPS de `canonicalUrl`
  somente a `OfficialExternal`, enquanto a apresentação cria um link para
  qualquer valor não nulo. Uma resposta sintética alterada apenas em memória,
  com `javascript:alert(document.domain)` na segunda citação
  `LocalAuthorised`, foi aceita e o SSR emitiu
  `href="javascript:alert(document.domain)"`; o React também registrou warning
  de URL insegura.
- Teste ausente: o teste de URL perigosa existente altera apenas a primeira
  citação, `OfficialExternal`; os testes de saída hostil cobrem texto de
  resposta, título e trecho, mas não `canonicalUrl` da citação local.
- Contexto e impacto: Application exige URL nula para evidência local, o que
  reduz a probabilidade no fluxo normal, mas não elimina a fronteira de
  confiança Dashboard/API nem a obrigação frontend de falhar fechado diante
  de resposta malformada. A CSP atual pode mitigar execução, mas a ativação do
  link não foi testada depois da parada e não substitui a validação do scheme.
- Condição de parada: acionada imediatamente após reprodução e classificação.
  `npm run lint`, `npm run typecheck`, `npm test`, `npm run build`, cobertura,
  screenshot, reflow estreito, teclado/browser e reprodutibilidade não foram
  executados neste gate. Nenhum listener foi iniciado.
- Resultado do Automatic Quality Gate: `REPROVADO`, com um P1 aberto e nenhum
  P0, P2 ou P3 registrado antes da parada obrigatória.
- Mudanças: somente relatório e memória factual do gate; nenhum arquivo de
  frontend, código, teste, dependência, package, lockfile, contrato, OpenAPI,
  ADR, backend ou configuração foi alterado.
- Limitações preservadas: percentuais de cobertura JavaScript, screenshot do
  build estilizado, viewport estreito direto, engine externa de acessibilidade
  e reprodução no Node exato permanecem sem disposição por este gate.
- Próxima condição: autoridade humana explícita e separada para corrigir
  `AQG-S05-001`; depois de uma baseline corretiva limpa, nova autoridade
  separada para reiniciar integralmente o Automatic Quality Gate.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-05 — S05-CORR-01 corrige AQG-S05-001

- Estado preservado: `STATE-05 FRONTEND_IMPLEMENTATION` permaneceu ativo; o
  Automatic Quality Gate não foi repetido, e Human Gate e `STATE-06` não foram
  autorizados nem executados.
- Baseline inicial: branch `main`, commit
  `7ee2241049dc68f16a38e85bd622928e64a317e7`, corpus `4.9.2` e working tree
  limpa, reconfirmados imediatamente antes da primeira alteração.
- Autoridade: incremento local, offline, sequencial e limitado
  `S05-CORR-01`, exclusivamente para corrigir `AQG-S05-001`, adicionar
  regressões de contrato/apresentação, executar os quatro checks npm existentes
  e atualizar relatório e memória factual.
- Correção: o commit `654fce6e0a09d6e7196e434de0ff6f5d6ccd5b04`
  rejeita toda URL de citação não nula que não seja HTTPS validado, rejeita
  qualquer `canonicalUrl` não nula em `LocalAuthorised` e mantém uma barreira
  de apresentação que cria links somente para `OfficialExternal` com HTTPS
  validado.
- Regressões: o contrato rejeita `javascript:` e HTTPS em citação local,
  preserva URL HTTPS oficial e URL nula local; a apresentação não cria link
  para estado local malformado e conserva o link oficial válido.
- Preflight runtime: antes dos checks, foi observado somente o runtime de
  controle do navegador do Codex associado ao workspace, sem listener. Ele não
  pertencia ao produto e não foi encerrado.
- Verificações: `npm run lint`, `npm run typecheck`, `npm test` e
  `npm run build` passaram na instalação existente. Foram 29 testes, sem
  falha, skip ou cancelamento, e 20 módulos transformados no build.
- Ambiente: Node.js observado `24.18.1`, contra pin `24.18.0`, e npm
  `11.16.0`. Nenhuma instalação ou execução `dotnet` ocorreu.
- Contratos e dependências: package, lockfile e OpenAPI permaneceram sem diff;
  o OpenAPI conservou SHA-256
  `D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34`.
- Disposição: `AQG-S05-001` está
  `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`. A correção e seus testes não
  substituem nem aprovam o Automatic Quality Gate reprovado.
- Limitações preservadas para o gate: percentuais de cobertura JavaScript,
  screenshot do build estilizado, viewport estreito direto, engine externa de
  acessibilidade e reprodução no Node exato continuam sem disposição.
- Escopo negativo preservado: sem dependência, package, lockfile, contrato
  externo, OpenAPI, ADR, backend, Domain, Application, Infrastructure, API,
  provider, instalação, rede externa, conta, secret, corpus real, fonte
  oficial real, GitHub, OCI, publicação, deploy, DB-Notifier, Automatic
  Quality Gate, Human Gate ou estado posterior.
- Próxima condição: nova baseline limpa e autorização humana explícita e
  separada para reiniciar integralmente o Automatic Quality Gate de
  `STATE-05`.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-05 — Automatic Quality Gate após S05-CORR-02 reprovado

- Estado preservado: `STATE-05 FRONTEND_IMPLEMENTATION` permaneceu ativo; o
  Human Gate e `STATE-06` não foram autorizados nem executados.
- Baseline do reinício: branch `main`, commit
  `3f120aaf3cbc199c821685b161ece95a1988a659`, corpus `4.9.2` e working tree
  limpa, reconfirmados imediatamente antes da auditoria.
- Autoridade: reinício integral, local, offline e sequencial do Automatic
  Quality Gate, sem correção automática, mudança de produto/teste,
  dependência, instalação, rede externa, ação remota, Human Gate ou estado
  posterior.
- Escopo iniciado: releitura integral das autoridades e inspeção desde o
  começo de lifecycle, escopo, contratos e segurança; os checks npm e o
  browser loopback somente prosseguiriam sem condição de parada.
- Reavaliação de `AQG-S05-001` a `AQG-S05-003`: a inspeção estática confirmou
  URL local nula/HTTPS oficial, leitura incremental limitada e título
  localizado por `interfaceLanguage`. As regressões focais permanecem
  presentes, mas a parada ocorreu antes do reteste executável; a disposição
  permanece pendente de um gate completo posterior.
- `AQG-S05-004` (P2): Application exige `SourceFreshness.Local` para
  `LocalAuthorised`, e a API serializa o valor `Local`. O Dashboard não inclui
  `Local` em `knownSourceStates`, aceita qualquer string não vazia no decoder
  e apresenta o fallback `Estado não reconhecido`/`Unrecognised state`. A
  fixture local usa incorretamente `Current` e impede os testes atuais de
  revelar a divergência.
- Impacto: uma resposta v1 válida com evidência local autorizada apresenta
  freshness incorreto nas duas interfaces, enfraquecendo proveniência,
  factualidade e localização completa.
- Condição de parada: acionada durante a inspeção estática. Preflight
  executável, lint, typecheck, testes, build, cobertura percentual, browser,
  acessibilidade visual, viewport estreito/reflow, teclado, temas, idiomas,
  matriz das oito combinações e reprodutibilidade não foram executados. Nenhum
  processo ou listener foi inspecionado, iniciado ou encerrado.
- Resultado do Automatic Quality Gate reiniciado: `REPROVADO`, com um P2
  aberto e nenhum novo P0/P1 observado antes da parada. `AQG-S05-001` a
  `AQG-S05-003` permanecem corrigidos, mas pendentes de reteste executável e
  disposição.
- Mudanças: somente relatório e memória factual do gate; nenhum arquivo de
  frontend, código, teste, dependência, package, lockfile, contrato, OpenAPI,
  ADR, backend ou configuração foi alterado.
- Limitações preservadas: percentuais de cobertura JavaScript, styled visual
  review, viewport estreito direto, engine externa de acessibilidade, teclado,
  matriz browser e reprodução no Node exato permanecem sem disposição.
- Próxima condição: autoridade humana explícita e separada para corrigir
  `AQG-S05-004`; depois de baseline corretiva limpa, nova autoridade separada
  para reiniciar integralmente o Automatic Quality Gate.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-05 — Reinício integral do Automatic Quality Gate de STATE-05 reprovado

- Estado preservado: `STATE-05 FRONTEND_IMPLEMENTATION` permaneceu ativo; o
  Human Gate e `STATE-06` não foram autorizados nem executados.
- Baseline do reinício: branch `main`, commit
  `f7e7f4a9d4afd234c9f3fcc725e7093653bc3363`, corpus `4.9.2` e working tree
  limpa, reconfirmados antes da auditoria.
- Autoridade: reinício integral, local, offline e sequencial do Automatic
  Quality Gate, sem correção automática, mudança de produto/teste,
  dependência, instalação, rede externa, ação remota, Human Gate ou estado
  posterior.
- Escopo iniciado: releitura de autoridades e inspeção desde o começo de
  lifecycle, escopo, contratos, segurança, cobertura, acessibilidade,
  viewport, matriz e reprodutibilidade; checks npm e browser loopback somente
  enquanto nenhuma condição de parada fosse encontrada.
- Reavaliação de `AQG-S05-001`: a inspeção estática confirmou rejeição de URL
  não HTTPS, URL nula obrigatória para `LocalAuthorised`, link restrito a
  `OfficialExternal` HTTPS e as regressões focais esperadas. Como a parada
  ocorreu antes de `npm test`, a disposição permanece
  `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`.
- `AQG-S05-002` (P2): `query-client.ts` chama `response.text()` antes de
  conferir o teto de 262.144 bytes. Não existe precheck de `Content-Length`,
  leitura incremental ou contador de bytes; o teste oversized comprova
  rejeição somente depois de alocar/materializar o corpo completo. Uma
  resposta same-origin inesperadamente grande pode consumir memória sem o
  limite declarado antes de ser rejeitada.
- `AQG-S05-003` (P2): `index.html` fixa o título
  `RAG-Challenge — Database documentation`, e não existe atualização de
  `document.title` ligada a `interfaceLanguage`. A interface padrão `pt-BR`
  mantém um rótulo visual inglês na aba do navegador.
- Condição de parada: acionada durante a inspeção estática. Preflight
  executável, lint, typecheck, testes, build, cobertura percentual, browser,
  acessibilidade visual, viewport estreito/reflow, teclado, matriz das oito
  combinações e reprodutibilidade não foram executados. Nenhum processo ou
  listener foi inspecionado, iniciado ou encerrado.
- Resultado do Automatic Quality Gate reiniciado: `REPROVADO`, com dois P2
  abertos, nenhum novo P0/P1 observado antes da parada e
  `AQG-S05-001` ainda pendente de reteste executável.
- Mudanças: somente relatório e memória factual do gate; nenhum arquivo de
  frontend, código, teste, dependência, package, lockfile, contrato, OpenAPI,
  ADR, backend ou configuração foi alterado.
- Limitações preservadas: percentuais de cobertura JavaScript, styled visual
  review, viewport estreito direto, engine externa de acessibilidade, matriz
  browser e reprodução no Node exato permanecem sem disposição.
- Próxima condição: autoridade humana explícita e separada para corrigir
  `AQG-S05-002` e `AQG-S05-003`; depois de baseline corretiva limpa, nova
  autoridade separada para reiniciar integralmente o Automatic Quality Gate.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-05 — S05-CORR-02 corrige AQG-S05-002 e AQG-S05-003

- Estado preservado: `STATE-05 FRONTEND_IMPLEMENTATION` permaneceu ativo; o
  Automatic Quality Gate não foi repetido, e Human Gate e `STATE-06` não foram
  autorizados nem executados.
- Baseline inicial: branch `main`, commit
  `651b4ad9edba79b3fc8a16e550fc2a357b6b85d2`, corpus `4.9.2` e working tree
  limpa, reconfirmados imediatamente antes da primeira alteração.
- Autoridade: incremento local, offline, sequencial e limitado
  `S05-CORR-02`, exclusivamente para corrigir `AQG-S05-002` e
  `AQG-S05-003`, preservar `AQG-S05-001`, adicionar regressões
  determinísticas, executar os quatro checks npm existentes e validar o
  título em listener loopback pertencente à tarefa.
- Limite da resposta: o commit
  `ec5ecf41b113853fc2863a94cbfe77dbe4741828` rejeita `Content-Length`
  decimal superior a 262.144 bytes antes de obter o reader, conta bytes
  incrementalmente, cancela a leitura no primeiro overflow e preserva
  cancelamento, media types, same-origin e falha fechada.
- Título visual: o commit
  `20458c8189b132b775786b2fc8f9b44ee5c2f7b8` define fallback `pt-BR` e
  atualiza `document.title` e o atributo `lang` exclusivamente por
  `interfaceLanguage`. As oito combinações provam independência de
  `questionLanguage` e tema.
- Regressões: limite exato, overflow incremental, `Content-Length` excedido e
  cancelamento foram exercitados deterministicamente; os casos de título
  `pt-BR`/`en-GB` e as regressões de `AQG-S05-001` também passaram.
- Verificações: `npm run lint`, `npm run typecheck`, `npm test` e
  `npm run build` passaram na instalação existente. Foram 34 testes, sem
  falha, skip ou cancelamento, e 20 módulos transformados no build.
- Ambiente: Node.js observado `24.18.1`, contra pin `24.18.0`, e npm
  `11.16.0`. Nenhuma instalação ou execução `dotnet` ocorreu.
- Validação loopback: depois do preflight dirigido, o Vite preview da tarefa
  escutou somente em `127.0.0.1:4173`. O browser confirmou os títulos em
  português e inglês após alternância, sem warning ou erro no console. A
  sessão foi finalizada, o processo foi identificado por executável e linha
  de comando antes de ser encerrado, e a porta terminou sem listener.
- Contratos e dependências: package, lockfile e OpenAPI permaneceram sem diff;
  o OpenAPI conservou SHA-256
  `D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34`.
- Disposição: `AQG-S05-002` e `AQG-S05-003` estão
  `CORRIGIDOS_PENDENTES_DE_RETESTE_DO_GATE`. A suíte completa confirmou as
  regressões de `AQG-S05-001`, mas este incremento não repetiu nem aprovou o
  gate; os três achados pendem de disposição pelo reinício integral.
- Limitações preservadas: percentuais de cobertura JavaScript, styled visual
  review completo, viewport estreito/reflow, engine externa de
  acessibilidade, teclado, matriz browser integral e reprodução no Node exato
  continuam sem disposição por este incremento.
- Escopo negativo preservado: sem dependência, package, lockfile, contrato
  externo, OpenAPI, ADR, backend, Domain, Application, Infrastructure, API,
  provider, instalação, rede externa, conta, secret, corpus real, fonte
  oficial real, GitHub, OCI, publicação, deploy, DB-Notifier, Automatic
  Quality Gate, Human Gate ou estado posterior.
- Próxima condição: nova baseline limpa e autorização humana explícita e
  separada para reiniciar integralmente o Automatic Quality Gate de
  `STATE-05`.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-05 — S05-CORR-03 corrige AQG-S05-004

- Estado preservado: `STATE-05 FRONTEND_IMPLEMENTATION` permaneceu ativo; o
  Automatic Quality Gate não foi repetido, e Human Gate e `STATE-06` não foram
  autorizados nem executados.
- Baseline inicial: localização `C:\Projects\RAG-Challenge`, Git top-level
  `C:/Projects/RAG-Challenge`, Git directory `.git`, branch `main`, commit
  `800e6dc92d2a3555dbe92bc4e3b6b16e6411726b`, corpus `4.9.2` e working tree
  limpa, reconfirmados imediatamente antes da primeira alteração.
- Autoridade: incremento local, offline, sequencial e limitado
  `S05-CORR-03`, exclusivamente para corrigir `AQG-S05-004`, preservar
  `AQG-S05-001` a `AQG-S05-003`, adicionar regressões determinísticas,
  executar os quatro checks npm existentes e validar a apresentação
  localizada em listener loopback pertencente à tarefa.
- Correção: o commit `9ef937744302044ee3cd9105c9a23ddd3557a861`
  restringe `sourceFreshness` ao conjunto canônico, exige `Local` e URL nula
  para `LocalAuthorised`, rejeita `Local` para `OfficialExternal`, localiza o
  estado nas duas interfaces e corrige a fixture local sem alterar a oficial.
- Regressões: citação local válida, relações cross-class incompatíveis e
  freshness desconhecido foram exercitados no decoder; a apresentação da
  citação local com `Local` passou em `pt-BR` e `en-GB`. A suíte completa
  preservou as regressões de `AQG-S05-001`, `AQG-S05-002` e `AQG-S05-003`.
- Verificações: `npm run typecheck`, `npm run lint`, `npm test` e
  `npm run build` passaram na instalação existente. Foram 35 testes, sem
  falha, skip ou cancelamento, e 20 módulos transformados no build.
- Ambiente: Node.js observado `24.18.1`, contra pin `24.18.0`, e npm
  `11.16.0`. Nenhuma instalação ou execução `dotnet` ocorreu.
- Validação loopback: depois do preflight dirigido, o Vite preview da tarefa
  escutou somente em `127.0.0.1:4173`. O browser confirmou títulos e headings
  em inglês, português após alternância e inglês novamente, sem erro no
  console. A apresentação da citação local foi validada pelos testes de
  componente com fixture sintética. A sessão foi finalizada, o processo foi
  identificado por executável e linha de comando antes de ser encerrado, e a
  porta terminou sem listener.
- Contratos e dependências: package, lockfile e OpenAPI permaneceram sem diff;
  o OpenAPI conservou SHA-256
  `D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34`.
- Disposição: `AQG-S05-004` está
  `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`. Os quatro achados pendem de reteste
  e disposição por um reinício integral do Automatic Quality Gate sob nova
  autoridade humana explícita e separada.
- Limitações preservadas: percentuais de cobertura JavaScript, styled visual
  review completo, viewport estreito/reflow, engine externa de
  acessibilidade, teclado, matriz browser integral e reprodução no Node exato
  continuam sem disposição por este incremento.
- Escopo negativo preservado: sem dependência, package, lockfile, contrato
  externo, OpenAPI, ADR, backend, Domain, Application, Infrastructure, API,
  provider, instalação, rede externa, conta, secret, corpus real, fonte
  oficial real, GitHub, OCI, publicação, deploy, DB-Notifier, Automatic
  Quality Gate, Human Gate ou estado posterior.
- Próxima condição: nova baseline limpa e autorização humana explícita e
  separada para reiniciar integralmente o Automatic Quality Gate de
  `STATE-05`.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-05 — Automatic Quality Gate após S05-CORR-03 reprovado

- Estado preservado: `STATE-05 FRONTEND_IMPLEMENTATION` permaneceu ativo; o
  Human Gate e `STATE-06` não foram autorizados nem executados.
- Baseline do reinício: localização `C:\Projects\RAG-Challenge`, Git
  top-level `C:/Projects/RAG-Challenge`, Git directory `.git`, branch `main`,
  commit `b457970aed4564d5a654bb4e8d38439c98f29522`, corpus `4.9.2` e working
  tree limpa, reconfirmados imediatamente antes da auditoria.
- Autoridade: reinício integral, local, offline e sequencial do Automatic
  Quality Gate, sem correção automática, mudança de produto/teste,
  dependência, instalação, rede externa, ação remota, Human Gate ou estado
  posterior.
- Escopo iniciado: releitura das autoridades e inspeção desde o começo de
  lifecycle, escopo, contratos e segurança; os checks npm e o browser
  loopback somente prosseguiriam sem condição de parada.
- Reavaliação de `AQG-S05-001` a `AQG-S05-004`: a inspeção estática confirmou
  URL local nula/HTTPS oficial, leitura incremental limitada, título por
  `interfaceLanguage` e relação canônica de freshness local/oficial. As
  regressões focais permanecem presentes, mas a parada ocorreu antes do
  reteste executável; a disposição continua pendente de um gate completo.
- `AQG-S05-005` (P2): o decoder valida `answerLanguage` somente contra o
  conjunto suportado e o cliente não o compara ao `questionLanguage` enviado.
  O teste de limite exato envia `en-GB`, recebe a fixture concluída `pt-BR` e
  aceita o resultado. Uma resposta incompatível pode, portanto, apresentar um
  idioma diferente do selecionado pelo usuário em violação do contrato v1.
- Condição de parada: acionada durante a inspeção estática. Preflight
  executável, lint, typecheck, testes, build, cobertura percentual, browser,
  acessibilidade visual, viewport estreito/reflow, teclado, temas, idiomas,
  matriz das oito combinações e reprodutibilidade não foram executados. Nenhum
  processo ou listener foi inspecionado, iniciado ou encerrado.
- Resultado do Automatic Quality Gate reiniciado: `REPROVADO`, com um P2
  aberto e nenhum novo P0/P1 observado antes da parada. `AQG-S05-001` a
  `AQG-S05-004` permanecem corrigidos, mas pendentes de reteste executável e
  disposição.
- Mudanças: somente relatório e memória factual do gate; nenhum arquivo de
  frontend, código, teste, dependência, package, lockfile, contrato, OpenAPI,
  ADR, backend ou configuração foi alterado.
- Limitações preservadas: percentuais de cobertura JavaScript, styled visual
  review, viewport estreito direto, engine externa de acessibilidade, teclado,
  matriz browser e reprodução no Node exato permanecem sem disposição.
- Próxima condição: autoridade humana explícita e separada para corrigir
  `AQG-S05-005`; depois de baseline corretiva limpa, nova autoridade separada
  para reiniciar integralmente o Automatic Quality Gate.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-05 — S05-CORR-04 corrige AQG-S05-005

- Estado preservado: `STATE-05 FRONTEND_IMPLEMENTATION` permaneceu ativo; o
  Automatic Quality Gate não foi repetido, e Human Gate e `STATE-06` não foram
  autorizados nem executados.
- Baseline inicial: localização `C:\Projects\RAG-Challenge`, Git top-level
  `C:/Projects/RAG-Challenge`, Git directory `.git`, branch `main`, commit
  `fb59861a8367749f2a11ac279add5007989d27e0`, corpus `4.9.2` e working tree
  limpa, reconfirmados imediatamente antes da primeira alteração.
- Autoridade: incremento local, offline, sequencial e limitado
  `S05-CORR-04`, exclusivamente para corrigir `AQG-S05-005`, preservar
  `AQG-S05-001` a `AQG-S05-004`, adicionar regressões determinísticas e
  executar os quatro checks npm existentes.
- Correção: o commit `bed8ec03d670ed4e76a556f7df723c30db320a24`
  vincula a conclusão ao `questionLanguage` enviado, exige
  `answerLanguage` idêntico e rejeita divergências nas duas direções. As
  fixtures cobrem os dois idiomas, e o transporte no limite exato deixou de
  aceitar a divergência anterior.
- Verificações: `npm run lint`, `npm run typecheck`, `npm test` e
  `npm run build` passaram na instalação existente e offline. Foram 37
  testes, sem falha, skip ou cancelamento, e 20 módulos transformados.
- Ambiente: Node.js observado `24.18.1`, contra pin `24.18.0`, e npm
  `11.16.0`. O preflight não encontrou processo ou listener pertencente ao
  RAG-Challenge; nenhum listener foi iniciado. Nenhuma instalação ou execução
  `dotnet` ocorreu.
- Contratos e dependências: package, lockfile e OpenAPI permaneceram sem diff;
  o OpenAPI conservou SHA-256
  `D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34`.
- Disposição: `AQG-S05-001` a `AQG-S05-005` estão
  `CORRIGIDOS_PENDENTES_DE_RETESTE_DO_GATE`. O incremento não executou nem
  aprovou o Automatic Quality Gate.
- Limitações preservadas: percentuais de cobertura JavaScript, styled visual
  review completo, viewport estreito/reflow, engine externa de
  acessibilidade, teclado, matriz browser integral e reprodução no Node exato
  continuam sem disposição por este incremento.
- Escopo negativo preservado: sem dependência, package, lockfile, contrato
  externo, OpenAPI, ADR, backend, Domain, Application, Infrastructure, API,
  provider, instalação, rede externa, conta, secret, corpus real, fonte
  oficial real, GitHub, OCI, publicação, deploy, DB-Notifier, Automatic
  Quality Gate, Human Gate ou estado posterior.
- Próxima condição: nova baseline limpa e autorização humana explícita e
  separada para reiniciar integralmente o Automatic Quality Gate de
  `STATE-05`.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-05 — Automatic Quality Gate após S05-CORR-04 reprovado

- Estado preservado: `STATE-05 FRONTEND_IMPLEMENTATION` permaneceu ativo; o
  Human Gate e `STATE-06` não foram autorizados nem executados.
- Baseline do reinício: localização `C:\Projects\RAG-Challenge`, Git
  top-level `C:/Projects/RAG-Challenge`, Git directory `.git`, branch `main`,
  commit `a58c4038fb14e656c95303d914e02c7f8ad75c17`, corpus `4.9.2` e working
  tree limpa, reconfirmados imediatamente antes da auditoria.
- Autoridade: reinício integral, local, offline e sequencial do Automatic
  Quality Gate, usando somente instalação existente, fixtures sintéticas,
  fetch falso e listener loopback pertencente à tarefa; sem correção
  automática, mudança de produto/teste, dependência, instalação, rede externa,
  ação remota, Human Gate ou estado posterior.
- Escopo executado antes da parada: releitura das autoridades e ADRs
  aplicáveis; inspeções de lifecycle, escopo, contratos e segurança; preflight
  dirigido; lint, typecheck, suíte de 37 testes, build, repetição reprodutível
  do build e validação inicial de semântica e teclado no browser.
- Disposição de `AQG-S05-001` a `AQG-S05-005`: `RESOLVIDOS`. A inspeção e a
  suíte completa confirmaram URL HTTPS/local nula, leitura incremental
  limitada, título por `interfaceLanguage`, freshness local/oficial e
  igualdade entre `answerLanguage` e o `questionLanguage` enviado.
- `AQG-S05-006` (P2): o skip link foi o primeiro alvo de `Tab` e apresentou
  outline sólido visível. Sua ativação alterou o fragmento para
  `#main-content`, mas `document.activeElement` tornou-se `<body>` em vez do
  `<main>`, que não é programaticamente focável. O bypass do cabeçalho não é,
  portanto, confiável para teclado.
- Verificações: `npm run lint`, `npm run typecheck`, `npm test` e
  `npm run build` passaram com exit code 0, 37 testes aprovados e 20 módulos
  transformados. A segunda construção produziu os mesmos nomes, tamanhos e
  SHA-256 dos três arquivos de `dist/`. Node.js observado `24.18.1`, contra
  pin `24.18.0`, e npm `11.16.0`.
- Runtime e cleanup: o preflight não encontrou processo ou listener do
  produto. O preview da tarefa escutou somente em `127.0.0.1:4173`; seu
  executável, comando, endereço e PID foram revalidados antes do encerramento,
  e a porta terminou livre. Um diretório de logs sanitizados permaneceu no
  temporário do sistema porque a política de execução recusou sua exclusão;
  ele não contém listener, secret, corpus ou mudança rastreada.
- Condição de parada: acionada na validação de teclado. Viewport estreito,
  reflow, alternância browser Light/Dark, `pt-BR`/`en-GB` e matriz browser das
  oito combinações não foram alcançados. Os testes determinísticos de matriz,
  contraste e fake fetch já haviam passado.
- Limitações: percentuais de cobertura JavaScript permanecem indisponíveis na
  instalação existente; screenshot do browser expirou; engine externa de
  acessibilidade não foi instalada; reprodução no Node exato não foi provada.
- Resultado do Automatic Quality Gate reiniciado: `REPROVADO`, com
  `AQG-S05-006` (P2) aberto, nenhum novo P0/P1 observado e
  `AQG-S05-001` a `AQG-S05-005` resolvidos.
- Mudanças: somente relatório e memória factual do gate; nenhum arquivo de
  frontend, código, teste, dependência, package, lockfile, contrato, OpenAPI,
  ADR, backend ou configuração foi alterado.
- Próxima condição: autoridade humana explícita e separada para corrigir
  `AQG-S05-006`; depois de baseline corretiva limpa, nova autoridade separada
  para reiniciar integralmente o Automatic Quality Gate.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-05 — S05-CORR-05 corrige AQG-S05-006

- Estado preservado: `STATE-05 FRONTEND_IMPLEMENTATION` permaneceu ativo; o
  Automatic Quality Gate não foi reiniciado, e Human Gate e `STATE-06` não
  foram autorizados nem executados.
- Baseline inicial: localização `C:\Projects\RAG-Challenge`, Git top-level
  `C:/Projects/RAG-Challenge`, Git directory `.git`, branch `main`, commit
  `3ff7002b394199bbf253139836827231c1988116`, corpus `4.9.2` e working tree
  limpa, reconfirmados imediatamente antes do preflight e da primeira
  alteração.
- Autoridade: incremento local, offline, sequencial e limitado
  `S05-CORR-05`, exclusivamente para corrigir `AQG-S05-006`, adicionar
  regressão focal, executar os quatro checks npm existentes e validar o foco
  em listener loopback pertencente à tarefa.
- Correção: o commit `8b543eb85907b5aa4023f109dabb4bb11100da3e`
  torna `main#main-content` programaticamente focável e transfere o foco ao
  alvo quando o skip link é ativado. A regressão de componente exercita a
  transferência e a ordem estrutural até o controle seguinte no conteúdo.
- Verificações: `npm run lint`, `npm run typecheck`, `npm test` e
  `npm run build` passaram na instalação existente e offline. Foram 38 testes,
  sem falha, skip ou cancelamento, e 20 módulos transformados. O primeiro
  typecheck levou a uma correção tipada local dentro do escopo, sem dependência
  ou declaração compartilhada nova.
- Browser: no build servido exclusivamente em `127.0.0.1:4173`, o primeiro
  `Tab` focou `Skip to content`, `Enter` transferiu o foco ao
  `MAIN#main-content` e o `Tab` seguinte focou o rádio selecionado `en-GB` de
  idioma da pergunta dentro do conteúdo principal. Não houve warning ou erro
  no console.
- Runtime e cleanup: o preflight não encontrou listener do produto nas portas
  4173 ou 5173. O preview pertencente à tarefa foi revalidado por PID,
  executável, comando, endereço e porta antes de ser encerrado; a porta 4173
  terminou livre.
- Ambiente: Node.js observado `24.18.1`, contra pin `24.18.0`, e npm
  `11.16.0`. Package, lockfile, OpenAPI, contratos e backend permaneceram sem
  alteração.
- Disposição: `AQG-S05-006` está
  `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`; `AQG-S05-001` a `AQG-S05-005`
  conservam a disposição `RESOLVIDOS`. O incremento não executou nem aprovou o
  Automatic Quality Gate.
- Limitações preservadas: percentuais de cobertura JavaScript, reprodução no
  Node exato, viewport estreito/reflow, matriz browser completa de temas e
  idiomas, screenshot e engine externa de acessibilidade não foram repetidos
  por esta correção.
- Escopo negativo preservado: sem dependência, package, lockfile, contrato,
  OpenAPI, ADR, backend, Domain, Application, Infrastructure, API, `dotnet`,
  instalação, rede externa, provider, conta, secret, corpus real, fonte
  oficial real, GitHub, OCI, publicação, deploy, DB-Notifier, Automatic
  Quality Gate, Human Gate ou estado posterior.
- Próxima condição: baseline corretiva limpa e autoridade humana explícita e
  separada para reiniciar integralmente o Automatic Quality Gate de
  `STATE-05` e dispor `AQG-S05-006`.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-05 — Automatic Quality Gate após S05-CORR-05 reprovado

- Estado preservado: `STATE-05 FRONTEND_IMPLEMENTATION` permaneceu ativo; o
  Human Gate e `STATE-06` não foram autorizados nem executados.
- Baseline do reinício: localização `C:\Projects\RAG-Challenge`, Git
  top-level `C:/Projects/RAG-Challenge`, Git directory `.git`, branch `main`,
  commit `8ee1213eed3522493204c68b4f843e9c438e0f69`, corpus `4.9.2` e working
  tree limpa, reconfirmados imediatamente antes do preflight e da auditoria.
- Autoridade: reinício integral, local, offline e sequencial do Automatic
  Quality Gate, usando somente instalação existente, fixtures sintéticas,
  fetch falso e listener loopback pertencente à tarefa; sem correção
  automática, mudança de produto/teste, dependência, instalação, rede externa,
  ação remota, Human Gate ou estado posterior.
- Verificações: inspeções de autoridade, lifecycle, escopo, contratos e
  segurança; `npm run lint`, `npm run typecheck`, `npm test` e
  `npm run build`; repetição reprodutível do build; semântica e controles
  rotulados; foco e teclado; localização, temas e matriz browser das oito
  combinações; viewport estreito e reflow. Os quatro checks npm passaram, com
  38 testes e 20 módulos transformados, e os três arquivos construídos foram
  idênticos entre as duas construções.
- Disposição: `AQG-S05-001` a `AQG-S05-006` estão `RESOLVIDOS`. O browser
  confirmou a transferência do skip link a `MAIN#main-content` e o avanço do
  `Tab` seguinte ao primeiro controle interno.
- `AQG-S05-007` (P2): em viewport observado de 320 CSS px, todas as quatro
  combinações com interface `pt-BR` produziram `scrollWidth` 355 para
  `clientWidth` 303, em ambos os temas e idiomas da pergunta. A região hero
  cresceu para aproximadamente 348 pixels dentro de um contêiner de 287
  pixels. As quatro combinações `en-GB` permaneceram sem overflow. A inspeção
  visual confirmou conteúdo cortado e rolagem horizontal.
- Condição de parada: acionada sem qualquer correção de frontend, código ou
  teste. O resultado do Automatic Quality Gate reiniciado é `REPROVADO`, com
  um P2 aberto e nenhum novo P0/P1 observado.
- Runtime e cleanup: o preflight não encontrou listener do produto. O preview
  pertencente à tarefa escutou somente em `127.0.0.1:4173`; PID, executável,
  comando e porta foram revalidados antes do encerramento, e a porta terminou
  livre.
- Limitações: percentuais de cobertura JavaScript permanecem indisponíveis;
  Node.js observado `24.18.1` diverge do pin `24.18.0`; nenhuma engine externa
  de acessibilidade, backend real, provider, conta, secret, corpus real, fonte
  oficial real ou rede externa foi usada.
- Mudanças: somente relatório e memória factual do gate; nenhum frontend,
  código, teste, dependência, package, lockfile, contrato, OpenAPI, ADR,
  backend ou configuração foi alterado.
- Próxima condição: autoridade humana explícita e separada para corrigir
  `AQG-S05-007`; depois de baseline corretiva limpa, nova autoridade separada
  para reiniciar integralmente o Automatic Quality Gate.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-05 — S05-CORR-06 corrige AQG-S05-007

- Estado preservado: `STATE-05 FRONTEND_IMPLEMENTATION` permaneceu ativo; o
  Automatic Quality Gate não foi reiniciado, e Human Gate e `STATE-06` não
  foram autorizados nem executados.
- Baseline inicial: localização `C:\Projects\RAG-Challenge`, Git top-level
  `C:/Projects/RAG-Challenge`, Git directory `.git`, branch `main`, commit
  `c32953eceb149efa3cfeb952f1dbfdbe0c00e2eb`, corpus `4.9.2` e working tree
  limpa, reconfirmados imediatamente antes do preflight e da primeira
  alteração.
- Autoridade: incremento local, offline, sequencial e limitado
  `S05-CORR-06`, exclusivamente para corrigir `AQG-S05-007`, adicionar
  regressão focal das oito combinações, executar os quatro checks npm
  existentes e validar em listener loopback pertencente à tarefa.
- Correção: o commit `e34e73c7bbe8fabf96d5a5683df35935a3266e37`
  mantém a coluna única da hero reduzível com `minmax(0, 1fr)` e reduz o termo
  fluido do H1 compacto de `14vw` para `11vw`, preservando o mínimo de
  2,25 rem. A regressão focal fixa ambos os limites na matriz existente das
  oito combinações.
- Verificações: `npm run lint`, `npm run typecheck`, `npm test` e
  `npm run build` passaram na instalação existente e offline. Foram 38 testes,
  sem falha, skip ou cancelamento, e 20 módulos transformados.
- Browser isolado: um perfil Chrome temporário, com extensões e componentes
  de extensão desativados, apresentou zero alvo de extensão; Dark Reader não
  participou. Nas oito combinações a 320 CSS px, `clientWidth` e
  `scrollWidth` foram 305, e H1 `clientWidth`/`scrollWidth` foram 289/289 a
  36 px. Idiomas, títulos, estado selecionado e temas permaneceram coerentes.
  A inspeção visual em pt-BR Light/Dark mostrou reflow vertical sem corte ou
  rolagem horizontal.
- Foco e teclado: nas oito combinações, o primeiro `Tab` exibiu o skip link
  com outline sólido, `Enter` focou `MAIN#main-content` e o `Tab` seguinte
  focou o rádio selecionado do idioma da pergunta. Nenhuma exceção runtime
  ocorreu. A requisição não material e preexistente de `/favicon.ico`
  retornou 404; CSS e JavaScript retornaram 200.
- Runtime e cleanup: preview e Chrome isolado foram revalidados por processo,
  comando, perfil, endereço e porta antes do encerramento; as portas 4173 e
  9230 terminaram livres. A política de execução recusou excluir cinco
  diretórios temporários sanitizados da tarefa; eles não contêm processo,
  listener, secret, corpus real ou mudança rastreada.
- Ambiente e limites: Node.js observado `24.18.1`, contra pin `24.18.0`, e npm
  `11.16.0`; percentuais de cobertura JavaScript continuam indisponíveis e
  nenhuma engine externa de acessibilidade foi instalada. Package, lockfile,
  OpenAPI, contratos e backend permaneceram sem alteração.
- Disposição: `AQG-S05-007` está
  `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`; `AQG-S05-001` a `AQG-S05-006`
  conservam a disposição `RESOLVIDOS`. O incremento não executou nem aprovou o
  Automatic Quality Gate.
- Escopo negativo preservado: sem dependência, package, lockfile, contrato,
  OpenAPI, ADR, backend, Domain, Application, Infrastructure, API, `dotnet`,
  instalação, rede externa, provider, conta, secret, corpus real, fonte
  oficial real, GitHub, OCI, publicação, deploy, DB-Notifier, Automatic
  Quality Gate, Human Gate ou estado posterior.
- Próxima condição: baseline corretiva limpa e autoridade humana explícita e
  separada para reiniciar integralmente o Automatic Quality Gate de
  `STATE-05` e dispor `AQG-S05-007`.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-05 — Automatic Quality Gate após S05-CORR-06 reprovado

- Estado preservado: `STATE-05 FRONTEND_IMPLEMENTATION` permaneceu ativo; o
  Human Gate e `STATE-06` não foram autorizados nem executados.
- Baseline do reinício: localização `C:\Projects\RAG-Challenge`, Git
  top-level `C:/Projects/RAG-Challenge`, Git directory `.git`, branch `main`,
  commit `bc2ddd6bf64fc82f7d68eb518c3013d85655c16a`, corpus `4.9.2` e working
  tree limpa, reconfirmados antes da auditoria.
- Autoridade: reinício integral, local, offline e sequencial do Automatic
  Quality Gate; sem correção automática, mudança de produto/teste,
  dependência, instalação, rede externa, ação remota, Human Gate ou estado
  posterior.
- Inspeção estática: autoridade, lifecycle, escopo, contratos e segurança
  foram repetidos desde o início. Os controles de `AQG-S05-001` a
  `AQG-S05-007` estavam presentes; o OpenAPI permaneceu com SHA-256
  `D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34`, e
  package, lockfile, contrato, ADR e backend não tinham diff no escopo.
- `AQG-S05-008` (P2): resposta, título e trecho de citação derivados da API
  permitem tokens contínuos válidos pelo contrato, mas `.answer-copy`,
  `.citation-card h4` e `.citation-card blockquote` não permitem sua quebra.
  Um token longo pode ampliar o conteúdo além do viewport estreito. A
  regressão hostil cobre escaping, e a matriz estreita cobre as restrições da
  hero, mas nenhuma cobre esses valores contínuos no resultado concluído.
- Condição de parada: acionada antes do preflight executável. Nenhum processo
  ou listener foi inspecionado, iniciado ou encerrado; lint, typecheck, testes,
  build, reprodutibilidade, cobertura, browser sem extensões, acessibilidade,
  viewport, reflow, teclado, foco, temas, idiomas e matriz das oito combinações
  não foram executados neste reinício.
- Disposição: o gate é `REPROVADO`, com `AQG-S05-008` aberto e nenhum novo
  P0/P1. `AQG-S05-001` a `AQG-S05-006` conservam `RESOLVIDOS`;
  `AQG-S05-007` permanece `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE` porque o
  reteste executável não foi alcançado.
- Mudanças: somente relatório e memória factual do gate; nenhum frontend,
  código, teste, dependência, package, lockfile, contrato, OpenAPI, ADR,
  backend ou configuração foi alterado.
- Próxima condição: autoridade humana explícita e separada para corrigir
  `AQG-S05-008`; depois de baseline corretiva limpa, nova autoridade separada
  para reiniciar integralmente o Automatic Quality Gate.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-05 — S05-CORR-07 corrige AQG-S05-008

- Estado preservado: `STATE-05 FRONTEND_IMPLEMENTATION` permaneceu ativo; o
  Automatic Quality Gate não foi reiniciado, e Human Gate e `STATE-06` não
  foram autorizados nem executados.
- Baseline inicial: localização `C:\Projects\RAG-Challenge`, Git top-level
  `C:/Projects/RAG-Challenge`, Git directory `.git`, branch `main`, commit
  `dfa31d02e8ba3fd171986ea2c1d06c70101d07a3`, corpus `4.9.2` e working tree
  limpa, reconfirmados imediatamente antes do preflight e da primeira
  alteração.
- Correção: o commit `3f003b9db67eefeccc7e677c319ca37a26d49fa7`
  aplica `overflow-wrap: anywhere` à resposta, ao título e ao trecho de
  citação, sem truncamento, e cobre nas oito combinações tokens contínuos
  distintos, válidos pelo decoder e preservados integralmente na saída React.
- Verificações: `npm run lint`, `npm run typecheck`, `npm test` e
  `npm run build` passaram offline na instalação existente. Foram 38 testes,
  sem falha, skip ou cancelamento, e 20 módulos transformados. Package,
  lockfile, OpenAPI, contratos e backend permaneceram sem alteração.
- Incidente: a primeira tentativa no Chrome headless isolado enviou `Enter`
  antes de confirmar o alvo após `blur`; a citação oficial sintética recebeu
  foco e abriu o PDF público do PostgreSQL. Esse acesso externo não era
  autorizado. A tarefa parou imediatamente, encerrou Chrome e preview, limpou
  as portas e informou o proprietário. Não participaram credencial, conta,
  secret, corpus real do produto ou pergunta real.
- Retomada: após o proprietário permitir continuar em headless, a baseline
  suja foi reconfirmada com somente os dois arquivos esperados. A repetição
  usou exclusivamente citação `LocalAuthorised` sem URL, interceptação que
  permitia apenas `127.0.0.1:4173` e guarda do foco no skip link antes de cada
  `Enter`.
- Browser final: Chrome `151.0.7922.75`, zero alvo de extensão, zero tentativa
  bloqueada ou URL externa e zero exceção runtime. Nas oito combinações a
  `innerWidth` 320, o documento mediu `clientWidth`/`scrollWidth` 305/305; os
  três tokens permaneceram íntegros, quebraram em múltiplas linhas e não
  excederam suas superfícies. Light/Dark, `pt-BR`/`en-GB`, títulos e estados
  selecionados permaneceram coerentes.
- Foco e teclado: em todas as combinações, o primeiro `Tab` focou o skip link
  com outline sólido de três pixels, o `Enter` guardado focou
  `MAIN#main-content` e o `Tab` seguinte focou o rádio selecionado. Capturas
  pt-BR Light/Dark confirmaram reflow vertical sem corte ou rolagem horizontal.
- Runtime e cleanup: Chrome e preview foram revalidados e encerrados; portas
  4173 e 9230 terminaram livres. A política recusou excluir quatro diretórios
  temporários sanitizados; o perfil inicial pode conservar cache do PDF
  público acessado acidentalmente, sem processo, listener, credencial, secret,
  corpus real do produto ou mudança rastreada.
- Disposição: `AQG-S05-008` está
  `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`; `AQG-S05-007` conserva o mesmo
  status e `AQG-S05-001` a `AQG-S05-006` conservam `RESOLVIDOS`. O lote não
  executou nem aprovou o Automatic Quality Gate.
- Limitações: percentuais de cobertura JavaScript continuam indisponíveis;
  Node.js observado `24.18.1` diverge do pin `24.18.0`; nenhuma engine externa
  de acessibilidade ou passagem browser em janela visível foi executada.
- Próxima condição: baseline corretiva limpa e autoridade humana explícita e
  separada para reiniciar integralmente o Automatic Quality Gate de
  `STATE-05` e dispor `AQG-S05-007` e `AQG-S05-008`.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-05 — Automatic Quality Gate após S05-CORR-07 aprovado

- Estado preservado: `STATE-05 FRONTEND_IMPLEMENTATION` permanece ativo; o
  Human Gate e `STATE-06` não foram autorizados nem executados.
- Baseline do reinício: localização `C:\Projects\RAG-Challenge`, Git
  top-level `C:/Projects/RAG-Challenge`, Git directory `.git`, branch `main`,
  commit `97ea076da84d7afdb3330aa05dcb39fc7b44ce0f`, corpus `4.9.2` e working
  tree limpa. As sete condições foram reconfirmadas antes da auditoria, e
  branch, commit e limpeza foram reconfirmados antes deste registro.
- Autoridade: reinício integral, local, offline e sequencial do Automatic
  Quality Gate, usando somente a instalação existente, fixtures sintéticas,
  fetch falso, listener loopback e Chrome headless temporário sem Dark Reader
  ou extensão modificadora. A fixture não continha link externo interativo; a
  interceptação bloqueava qualquer destino não loopback e toda ativação por
  `Enter` exigia guarda prévia do elemento ativo.
- Inspeção estática: autoridade, lifecycle, escopo, contratos e segurança
  foram repetidos desde o início. Package, lockfile, OpenAPI, ADRs, backend e
  demais superfícies protegidas não tinham diff no intervalo de `STATE-05`; o
  OpenAPI reteve SHA-256
  `D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34`.
  CSP/same-origin, escaping, URLs HTTPS, limite de resposta, idioma da
  resposta, freshness, foco e reflow conservaram seus controles.
- Checks npm: `npm run lint`, `npm run typecheck`, `npm test` e
  `npm run build` passaram offline na instalação existente. Foram 38 testes,
  sem falha, skip ou cancelamento, e 20 módulos transformados.
- Reprodutibilidade: dois builds consecutivos produziram os mesmos três
  arquivos byte a byte: HTML 977 bytes,
  `53B4C11EED457043B6FDEFC6437A1F7539DEEE28E02BD7D9A718065FDD885BF1`;
  CSS 12.053 bytes,
  `5AF87FA05E947BC21DEFB8A4BCE69A202E7BC77D5A85AC85574578B723633B9A`;
  JavaScript 171.973 bytes,
  `1D1E4981B8B34FB585260DACCB32646C0594A3C0F4CF080DA35A73625CC7DCCA`.
- Browser isolado: Chrome `151.0.7922.75` iniciou com perfil novo, extensões e
  background de componentes desativados e zero alvo de extensão. A matriz das
  oito combinações passou a 1280 CSS px e foi repetida integralmente a 320 CSS
  px. No estreito, documento `clientWidth`/`scrollWidth` foi 305/305; resposta,
  título e trecho com tokens contínuos íntegros mediram 243/243, 213/213 e
  210/210. Capturas `pt-BR`/`en-GB` em Light/Dark confirmaram reflow vertical,
  hierarquia e ausência de corte ou panning.
- Acessibilidade: um header, main, footer e H1, IDs únicos e controles
  rotulados passaram nas 16 execuções. O primeiro `Tab` focou o skip link com
  outline sólido de três pixels; após a guarda explícita, `Enter` focou
  `MAIN#main-content`; o `Tab` seguinte focou o rádio selecionado. Contrastes
  runtime observados foram no mínimo 14,35 no body, 10,48 no controle
  selecionado e 12,93 no textarea.
- Rede e segurança: o fetch falso devolveu exclusivamente uma citação
  `LocalAuthorised` sem URL. Foram observadas zero tentativa bloqueada, zero
  URL externa, zero exceção runtime e zero entrada significativa de console.
  Somente HTML, JavaScript, CSS e `/favicon.ico` passaram pelo loopback; o 404
  já conhecido do favicon foi não material.
- Disposição: o Automatic Quality Gate é `APROVADO`, sem novo P0, P1, P2 ou
  P3. `AQG-S05-001` a `AQG-S05-008` estão `RESOLVIDOS`.
- Runtime e cleanup: preview e Chrome foram identificados pelos PIDs,
  comando/perfil e portas da tarefa antes do encerramento. As portas 4173,
  5173 e 9230 terminaram livres. Um diretório temporário sanitizado conserva
  logs, perfil isolado loopback-only, harness e quatro screenshots; não contém
  processo, listener, credencial, secret, corpus real ou mudança rastreada.
- Limitações: percentuais de cobertura JavaScript continuam indisponíveis;
  Node.js observado `24.18.1` diverge do pin `24.18.0`; nenhuma engine externa
  de acessibilidade, janela visível, backend real, provider, conta, secret,
  corpus real, fonte oficial real ou rede externa foi usada. Uma tentativa
  inicial do harness temporário falhou antes do CDP por modo de módulo
  incompatível e não iniciou navegação nem requisição; o invólucro temporário
  foi corrigido sem mudar produto ou teste.
- Mudanças: somente relatório e memória factual do gate; nenhum frontend,
  código, teste, dependência, package, lockfile, contrato, OpenAPI, ADR,
  backend ou configuração foi alterado.
- Próxima condição: autoridade humana explícita e separada para executar o
  Human Gate de `STATE-05` sobre o relatório automático aprovado e a baseline
  limpa. O gate automático não concede essa autoridade nem entrada em
  `STATE-06`.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-05 — S05-CORR-08 corrige observação visual do Human Gate

- Estado preservado: `STATE-05 FRONTEND_IMPLEMENTATION` permanece ativo. O
  Automatic Quality Gate não foi reiniciado, o Human Gate não recebeu decisão
  registrada e `STATE-06` não foi autorizado nem executado.
- Baseline inicial: localização `C:\Projects\RAG-Challenge`, Git top-level
  `C:/Projects/RAG-Challenge`, Git directory `.git`, branch `main`, commit
  `3bf97915d623f2e4c5c3d86da52e724ad906ea35`, corpus `4.9.2` e working tree
  limpa, reconfirmados imediatamente antes da ação.
- Correção: o commit `b65d3b45a0ad32f0f7db1e97ccf415bdef5bb113`
  remove o rótulo promocional, o título grande anterior e a ilustração
  decorativa da hero. A introdução localizada é seu único conteúdo visível e
  único H1, com tipografia proporcional. O equivalente `en-GB` e o rótulo de
  workspace usado como nome acessível foram preservados.
- Regressões: os testes focais cobrem os dois H1 localizados, Light/Dark,
  exatamente um H1 nas oito combinações, ausência do título/orbit removidos e
  a tipografia proporcional sem override estreito. As regressões existentes
  de escaping, tokens longos, idiomas, foco, teclado e matriz permaneceram
  ativas.
- Checks: a primeira sequência passou lint e encontrou no typecheck a
  reutilização acessível de `workspaceLabel`; o campo foi restaurado dentro
  do escopo e os quatro checks foram reiniciados. `npm run lint`,
  `npm run typecheck`, `npm test` e `npm run build` então passaram offline na
  instalação existente, com 38 testes e 20 módulos transformados.
- Browser: Chrome headless `151.0.7922.75`, perfil novo sem extensões, fetch
  falso, citação local sem URL, interceptação somente loopback e guarda antes
  de cada `Enter`. As oito combinações passaram em 1280 e 320 CSS px, sem
  overflow, truncamento, URL externa, exceção runtime ou entrada significativa
  de console. Temas, localização, idioma da pergunta, foco e sequência de
  teclado permaneceram coerentes; quatro capturas estreitas confirmaram o
  reflow visual em `pt-BR`/`en-GB` e Light/Dark.
- Runtime e cleanup: preview e Chrome foram identificados e encerrados; portas
  4173, 5173 e 9230 terminaram livres. A política de execução recusou remover
  o diretório temporário sanitizado, que permanece fora do repositório sem
  processo, listener, credencial, secret, corpus real ou mudança rastreada.
- Escopo negativo preservado: sem dependência, instalação, package, lockfile,
  contrato, OpenAPI, ADR, backend, Domain, Application, Infrastructure, API,
  `dotnet`, rede externa, provider, conta, secret, corpus real, fonte oficial
  real, GitHub, OCI, publicação, deploy, DB-Notifier, Automatic Quality Gate,
  registro do Human Gate ou estado posterior.
- Consequência: a aprovação automática sobre a baseline anterior permanece
  histórica, mas não cobre a correção. A próxima condição é uma baseline
  corretiva limpa e autoridade humana explícita e separada para reiniciar
  integralmente o Automatic Quality Gate; somente após nova aprovação a
  revisão do Human Gate pode ser retomada.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-05 — Automatic Quality Gate após S05-CORR-08 aprovado

- Estado preservado: `STATE-05 FRONTEND_IMPLEMENTATION` permanece ativo; o
  Human Gate e `STATE-06` não foram autorizados nem executados.
- Baseline: localização `C:\Projects\RAG-Challenge`, Git top-level
  `C:/Projects/RAG-Challenge`, Git directory `.git`, branch `main`, commit
  `b68cf2d8a9a6c735781529f1f3fb63d5cd515f95`, corpus `4.9.2` e working tree
  limpa, reconfirmados antes da auditoria.
- Autoridade: reinício integral, local, offline e sequencial do Automatic
  Quality Gate, com instalação existente, fixtures sintéticas, fetch falso,
  listeners loopback e Chrome headless temporário sem Dark Reader ou extensão
  modificadora. Produto/teste, dependências, instalação, contratos, backend,
  rede externa, ações remotas, Human Gate e estado posterior permaneceram fora
  do escopo.
- Inspeção estática: autoridade, lifecycle, escopo, decisões aceitas,
  contratos e segurança foram repetidos desde o início. Package, lockfile,
  OpenAPI, ADRs, backend e superfícies protegidas não tinham diff desde a
  entrada de `STATE-05`; o OpenAPI conservou SHA-256
  `D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34`.
- Checks: `npm run lint`, `npm run typecheck`, `npm test` e dois
  `npm run build` passaram offline na instalação existente. Foram 38 testes,
  zero falha/skip/cancelamento e 20 módulos por build; os três artefatos foram
  idênticos byte a byte nas duas execuções.
- Disposição: `AQG-S05-001` a `AQG-S05-008` estão `RESOLVIDOS` nesta
  baseline. HTTPS/null URL, limite de resposta, metadata da interface,
  freshness, vínculo de idioma, foco do skip link, reflow e tokens contínuos
  passaram. A hero corrigida também passou com uma única introdução localizada
  como H1, sem rótulo, título antigo ou ilustração.
- Browser: Chrome `151.0.7922.75`, perfil novo sem extensões, citação local sem
  URL, interceptação somente loopback e guarda antes de cada `Enter`. As oito
  combinações passaram em 1280 e 320 CSS px. Houve zero tentativa bloqueada,
  zero URL externa, zero exceção runtime e zero console significativo. Quatro
  capturas estreitas confirmaram Light/Dark e `pt-BR`/`en-GB` sem overflow,
  corte ou panning.
- Acessibilidade: um header/main/footer/H1, IDs únicos e controles rotulados
  passaram nas 16 execuções. O primeiro `Tab` focou o skip link com outline
  sólido de pelo menos três pixels; `Enter` guardado focou
  `MAIN#main-content`; o próximo `Tab` focou o rádio selecionado. Os contrastes
  runtime mínimos foram 14,35 no body, 10,48 no controle selecionado e 12,93
  no textarea.
- Runtime e cleanup: preview e Chrome foram identificados e encerrados; portas
  4173, 5173 e 9230 terminaram livres. A política recusou remover o diretório
  temporário sanitizado; ele permanece fora do repositório sem processo,
  listener, credencial, secret, corpus real ou mudança rastreada.
- Limitações: percentuais de cobertura JavaScript continuam indisponíveis;
  Node.js observado `24.18.1` diverge do pin `24.18.0`; não houve engine
  externa de acessibilidade, janela visível, backend real, provider, conta,
  secret, corpus real, fonte oficial real, rede externa ou captura de pacotes
  no host.
- Resultado: Automatic Quality Gate `APROVADO`, sem novo P0, P1, P2 ou P3.
  Isso não registra o Human Gate nem autoriza `STATE-06`.
- Mudanças: somente relatório e memória factual do gate; nenhum frontend,
  código, teste, dependência, package, lockfile, contrato, OpenAPI, ADR,
  backend ou configuração foi alterado.
- Próxima condição: autoridade humana explícita e separada para retomar a
  revisão e decidir o Human Gate de `STATE-05` sobre o relatório aprovado e a
  baseline limpa posterior ao registro.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-05 — Human Gate de STATE-05 aprovado sem ressalvas

- Estado anterior: `STATE-05 FRONTEND_IMPLEMENTATION` ativo, Automatic Quality
  Gate `APROVADO`, `AQG-S05-001` a `AQG-S05-008` `RESOLVIDOS` e Human Gate
  `PENDENTE`.
- Baseline: localização `C:\Projects\RAG-Challenge`, Git top-level
  `C:/Projects/RAG-Challenge`, Git directory `.git`, branch `main`, commit
  `192613364429a79ce82a208f072f5005209e6f52`, corpus `4.9.2` e working tree
  limpa, reconfirmados antes da ação e preservados até a decisão.
- Autoridade: preparar e conduzir localmente e de forma sequencial somente o
  Human Gate de `STATE-05`, apresentar o resumo completo, orientar as amostras
  humanas aplicáveis, aguardar a decisão inequívoca e somente então registrar
  o resultado nos três artefatos factuais autorizados e em commit local focal.
- Resumo revisado: baseline, Automatic Quality Gate aprovado,
  `AQG-S05-001` a `AQG-S05-008`, `S05-CORR-08`, amostras críticas, checks,
  build reproduzível, matriz das oito combinações, acessibilidade, limitações,
  riscos residuais, escopo negativo e rollback foram apresentados na mesma
  conversa.
- Amostras humanas: o build real foi disponibilizado temporariamente em Vite
  preview pertencente à tarefa e restrito a `127.0.0.1:4173`. A orientação
  cobriu hero localizada, Light/Dark sem Dark Reader ou extensão modificadora,
  `pt-BR`/`en-GB`, idioma da pergunta, foco do skip link, sequência de teclado,
  validação, viewport de 320 CSS px e reflow. Nenhum novo achado de produto ou
  ressalva foi relatado.
- Amostras não repetidas: resposta fundamentada e citações preenchidas não
  estavam disponíveis no listener real porque backend, corpus, provider e
  fetch falso não integravam a revisão humana. O proprietário aceitou a
  evidência automática sintética e as limitações já declaradas.
- Decisão: Human Gate `APROVADO` sem ressalvas. A confirmação inequívoca foi
  `Confirmo a decisão acima exclusivamente para STATE-05`.
- Runtime e cleanup: o Vite preview foi reidentificado por PID, executável,
  comando, endereço e porta antes do encerramento. As portas 4173, 5173 e 9230
  terminaram livres.
- Limitações e riscos preservados: sem percentuais de cobertura JavaScript,
  Node.js exato, engine externa de acessibilidade, browser automático em
  janela visível, backend/provider/conta/corpus/fonte oficial reais ou captura
  de pacotes no host. Os diretórios temporários sanitizados anteriormente
  registrados permanecem fora do repositório.
- Escopo negativo preservado: sem reinício do Automatic Quality Gate,
  correção ou mudança de frontend, código, teste, dependência, contrato,
  backend, instalação, rede externa, GitHub, OCI, publicação, deploy,
  DB-Notifier ou `STATE-06`.
- Mudanças: somente
  `docs/STATE-05-Frontend-Implementation-Report.md`,
  `prompts/state/Current-State.md` e este histórico append-only.
- Rollback: não executado nem necessário. Reversão futura exige autoridade
  própria, reverts focais ordinários e preservação deste histórico.
- Estado resultante: `STATE-05 FRONTEND_IMPLEMENTATION` encerrado após
  Automatic Quality Gate e Human Gate aprovados.
- Próxima condição: autoridade humana explícita e separada antes de entrar ou
  executar `STATE-06`; esta decisão não concede essa autoridade.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-05 — Entrada em STATE-06 e S06-A autorizados

- Estado anterior: `STATE-05 FRONTEND_IMPLEMENTATION` encerrado após
  Automatic Quality Gate aprovado e Human Gate aprovado sem ressalvas;
  `STATE-06` ainda não iniciado.
- Estado solicitado: `STATE-06 INTEGRATION`.
- Autoridade: solicitação explícita do proprietário para registrar a entrada
  e, somente depois desse registro, executar localmente, offline e
  sequencialmente o lote `S06-A`.
- Decisão: entrada em `STATE-06 INTEGRATION` autorizada e registrada; `S06-A`
  autorizado nos limites descritos abaixo.
- Escopo: integrar e verificar o fluxo sintético documento → índice → pergunta
  → resposta entre backend e frontend; sincronizar fonte oficial somente por
  servidor HTTP falso e loopback; validar restart e persistência de conteúdo
  bruto, catálogo, ativação e índice; preparar configuração por ambiente sem
  secrets; produzir artefato local reproduzível; demonstrar sua reprodução em
  baseline limpa; executar checks .NET e npm, testes de integração/E2E, build,
  higiene, documentação e commits locais focais.
- Escopo negativo: sem dependência ou instalação; sem alteração de
  `package.json`, `package-lock.json`, contratos, OpenAPI ou ADRs; sem rede
  externa, provider ou conta real, secret, corpus ou fonte oficial real,
  GitHub, OCI real, publicação, deploy, DB-Notifier, Automatic Quality Gate,
  Human Gate ou `STATE-07`.
- Pré-condições: localização `C:\Projects\RAG-Challenge`, Git top-level
  `C:/Projects/RAG-Challenge`, Git directory `.git`, branch `main`, HEAD
  `8fb3b93532a569af953cdf24e190b82998020464`, corpus `4.9.2` e working tree
  limpa foram reconfirmados antes deste registro; nenhuma divergência material
  foi observada.
- Mudanças: somente o snapshot factual e este histórico append-only registram
  a entrada antes da execução do lote.
- Verificações/evidências: releitura integral das autoridades e relatórios
  exigidos; baseline Git e corpus reconfirmados por comandos locais somente
  leitura.
- Limitações/riscos: os limites históricos de provider real, corpus real,
  fonte oficial real, OCI, Linux ARM64 runtime e Node.js exato permanecem; o
  lote deve parar diante das condições materiais definidas pelo proprietário.
- Quality Gate: não autorizado nem executado.
- Human Gate: não autorizado nem executado.
- Estado resultante: `STATE-06 INTEGRATION` ativo; `S06-A` autorizado e ainda
  não executado neste registro.
- Próxima condição: concluir `S06-A` dentro do envelope autorizado e registrar
  resultados, verificações, limitações, riscos, rollback e baseline final.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-05 — S06-A E2E e artefato concluído

- Estado preservado: `STATE-06 INTEGRATION` permanece ativo; esta entrada não
  executa Automatic Quality Gate, Human Gate ou transição para `STATE-07`.
- Autoridade: execução exclusivamente local, offline e sequencial de `S06-A`,
  depois do registro anterior de entrada, com instalação existente, fixtures
  sintéticas, stores temporários e listeners loopback pertencentes à tarefa.
- Baseline de implementação: `main@8041e25a554a7cc47ecebf4abe1fc8b94b12d12d`,
  corpus `4.9.2` e working tree rastreada limpa. O registro de entrada está no
  commit `ad218b58210e41d0c3a2c76ef81b5886498fd01a` e a implementação focal
  está no commit `8041e25a554a7cc47ecebf4abe1fc8b94b12d12d`.
- Implementação: perfil `Integration` explícito e fail-closed, configuração
  não secreta desabilitada por default, fixture CSV sintética, conteúdo
  imutável, catálogo SQLite, geração vetorial determinística, ativação e
  resposta grounded determinística nos idiomas `pt-BR` e `en-GB`. Dashboard e
  API v1 são servidos na mesma origem somente nesse perfil; contratos públicos
  e OpenAPI não mudaram.
- E2E e persistência: listener Kestrel real em loopback integrou documento →
  índice → pergunta → resposta, serviu o shell, devolveu citação/cobertura e
  reabriu depois de restart o mesmo conteúdo bruto, catálogo, ativação, índice
  e geração. A submissão visível pelo Dashboard em Chrome apresentou resposta
  fundamentada `pt-BR`, cobertura 1/1 e citação CSV local, sem warning/erro de
  console.
- Fonte oficial: sincronização executada somente contra servidor HTTP falso em
  `127.0.0.1`, com proxy e redirects desativados e rejeição test-only de alvo
  não loopback. Snapshot, conteúdo imutável e observação foram persistidos;
  nenhuma URL oficial real foi acessada.
- Artefato: duas construções consecutivas na baseline limpa produziram 58
  arquivos e o mesmo ZIP de 47.234.158 bytes, SHA-256
  `b2b6f50352c29a89f91640870564df263a2a5888f2009a94dc9a0ec1bb33b3c4`.
  A segunda cópia foi reproduzida em `127.0.0.1:5086`, respondeu nos dois
  idiomas e preservou após restart a geração
  `idxgen-795825d3ad7afad1acd3a16ef48f2448270dda36ea71725fe6f6231956ced2c5`.
- Checks aceitos: format sem mudança; build Release com zero warning/erro; 74
  testes unitários, 10 de arquitetura e 90 de integração aprovados; coletor
  de cobertura da integração com 90,72% de linhas e 62,35% de branches; lint,
  typecheck, 38 testes npm e build Vite aprovados. Uma invocação exploratória
  `npm test -- --run` foi inválida na CLI e substituída pelo comando
  proprietário correto; duas reproduções exploratórias anteriores expiraram
  antes da correção do handling/diagnóstico do processo e não integram a
  evidência aceita.
- Higiene: UTF-8/LF/newline, diff, scan aparente de secrets e arquivos
  protegidos passaram. `package.json`, `package-lock.json`, packages/lockfiles
  .NET, contratos, OpenAPI e ADRs permaneceram inalterados; o hash OpenAPI
  continua
  `D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34`.
  Os ports 5086 e 5096 terminaram livres e os runtimes da tarefa foram
  removidos.
- Limitações/riscos: a reprodução usa baseline Git limpa e instalação
  existente, não clone/máquina sem assets restaurados; Node.js observado
  `24.18.1` difere do pin `24.18.0`; cobertura percentual JavaScript continua
  indisponível; providers, fonte oficial, corpus, OCI, Linux ARM64,
  armazenamento/backup operacional, desempenho e captura de pacotes reais não
  foram testados. Nenhum P0/P1 foi encontrado.
- Escopo negativo preservado: sem dependência, instalação, rede externa,
  provider/conta/secret/corpus/fonte oficial reais, GitHub, OCI real,
  publicação, deploy, DB-Notifier, Automatic Quality Gate, Human Gate ou
  `STATE-07`.
- Relatório: [`STATE-06-Integration-Report.md`](../../docs/STATE-06-Integration-Report.md).
- Rollback: não executado. Reversão futura exige autoridade própria, reverts
  focais ordinários em ordem inversa e preservação append-only deste histórico;
  o ZIP ignorado pode ser regenerado ou removido somente pelo caminho local
  validado.
- Próxima condição: nova autoridade humana explícita e separada antes de
  qualquer outro lote, gate, estado ou ação externa.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-05 — Política de atualizações compatíveis da toolchain corrigida

- Estado preservado: `STATE-06 INTEGRATION` permanece ativo com `S06-A`
  concluído; nenhum gate ou estado foi iniciado ou transitado.
- Autoridade: correção focal explícita para aceitar atualizações automáticas
  compatíveis dentro de Node.js 24 e npm 11, limitada a `package.json`,
  `package-lock.json`, teste de toolchain e documentação factual necessária.
- Baseline: `main@4cb02527bad1f2a8a7c548382abf990a4ef6f55c`, corpus `4.9.2`,
  working tree limpa, Node.js `24.19.0`, npm `11.17.0` e
  `engine-strict=false`, reconfirmados antes da correção. O preflight dirigido
  encontrou zero listener e zero processo pertencente ao Dashboard.
- Implementação: commit
  `a7d50d8e72d5f5600ae41e3fdd313f4f1e502188` altera `engines` para Node.js
  `>=24.18.0 <25` e npm `>=11.16.0 <12`, aplica os mesmos intervalos em
  `devEngines` com `onFail: "error"`, remove o `packageManager` exato não
  aplicado, reconcilia o metadata raiz do lockfile e atualiza o teste para
  exigir a política completa. `.nvmrc` conserva `24.18.0` como seletor
  opcional do limite inferior; nenhuma dependência ou integridade mudou.
- Verificações: `npm run lint`, `npm run typecheck`, 38 testes npm e
  `npm run build` passaram offline na instalação existente. `devEngines`
  aceitou Node.js `24.19.0` e npm `11.17.0`.
- Reprodução: duas construções consecutivas sobre a baseline rastreada limpa
  `main@a7d50d8e72d5f5600ae41e3fdd313f4f1e502188` produziram o mesmo ZIP de
  58 arquivos e 47.234.166 bytes, SHA-256
  `65b405c690a1c66c374296745613217717d7fd38f04cbefb15994323da1ffc98`.
  A segunda cópia passou pelo fluxo loopback, respondeu em `pt-BR` e `en-GB`
  e preservou stores e geração ativa após restart.
- Escopo negativo preservado: sem instalação, restore, nova dependência, rede
  externa, contrato, OpenAPI, ADR, lifecycle, provider, conta, secret, corpus
  ou fonte oficial real, GitHub, OCI, publicação, deploy, DB-Notifier,
  Automatic Quality Gate, Human Gate ou `STATE-07`.
- Limite deliberado: Node.js 25 e npm 12 permanecem fora dos intervalos e
  exigem decisão e validação próprias; cobertura percentual JavaScript
  continua indisponível na instalação existente.
- Rollback: não executado. Reversão futura exige autoridade própria e revert
  focal ordinário do commit, preservando este histórico append-only.
- Relatório reconciliado:
  [`STATE-06-Integration-Report.md`](../../docs/STATE-06-Integration-Report.md).
- Próxima condição: nenhuma dentro desta correção concluída; nova autoridade
  é necessária antes de outro lote, gate, estado ou ação externa.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-06 — Automatic Quality Gate de STATE-06 reprovado

- Estado anterior: `STATE-06 INTEGRATION` ativo, `S06-A` concluído e
  Automatic Quality Gate ainda não executado.
- Estado solicitado: Automatic Quality Gate completo de `STATE-06
  INTEGRATION`.
- Autoridade: solicitação explícita do proprietário para auditoria local,
  offline, sequencial e sem correção sobre
  `main@a6f0480b7f229b63c5ac24d65e61f55de1c6483a`, corpus `4.9.2` e working
  tree limpa, com instalação existente, fixtures sintéticas, stores
  temporários e listeners loopback pertencentes à tarefa.
- Decisão: Automatic Quality Gate `REPROVADO`; `AQG-S06-001` a
  `AQG-S06-003` permanecem abertos, todos P2. Nenhum P0, P1 ou P3 foi
  identificado.
- Escopo: releitura integral das autoridades e ADRs aplicáveis; reconciliação
  dos entregáveis e aceites; preflight dirigido; checks .NET/npm; testes de
  integração/E2E; fluxo sintético documento → índice → pergunta → resposta;
  sincronização apenas por HTTP falso loopback; restart/persistência;
  configuração sem secret; reprodução e integridade do artefato; higiene e
  reprodução sobre baseline rastreada limpa.
- Escopo negativo: sem correção de código, configuração, teste, README,
  Lifecycle, roadmap, ADR ou contrato; sem restore, instalação ou atualização
  de dependência; sem rede externa, provider, conta, secret, corpus ou fonte
  oficial real, GitHub, OCI real, publicação, deploy, DB-Notifier, Human Gate
  ou `STATE-07`.
- Pré-condições: localização `C:\Projects\RAG-Challenge`, Git top-level
  `C:/Projects/RAG-Challenge`, Git directory `.git`, branch `main`, HEAD
  autorizado, corpus `4.9.2` e zero status entries foram reconfirmados. O
  preflight encontrou zero processo do produto e zero listener nas portas
  4173, 5086, 5096, 5173 e 9230; um `node.exe` alheio ao produto não foi
  encerrado.
- Verificações/evidências: format aprovou 0/118 alterações; build Release
  aprovou com zero warning/erro; 74 testes unitários, 10 de arquitetura e 90
  de integração passaram, total 174; cobertura combinada foi 92,38% de linhas
  e 66,59% de branches; lint, typecheck, 38 testes npm e build Vite passaram
  offline. Os dois testes focais de E2E/restart e HTTP falso passaram. Duas
  construções produziram o mesmo ZIP de 58 arquivos e 47.234.166 bytes,
  SHA-256
  `7b934d3fc8a099683c6599c3663c82d04de19ccdbf89fdeca885895821ade17f`;
  57/57 payloads conferiram com o manifesto, o hash declarado conferiu e a
  reprodução loopback preservou a geração ativa depois do restart.
- `AQG-S06-001` (P2): ADR-0005 contém direção OCI condicional, mas não existe
  plano/ensaio não produtivo pertencente a `STATE-06`. Os assets existentes
  não contêm target restaurado `net10.0/linux-arm64`; ensaiá-lo exigiria
  restore ou dependência adicional e não foi tentado.
- `AQG-S06-002` (P2): falhas, idempotência, concorrência e cancelamento têm
  cobertura em camadas inferiores/frontend, porém o host E2E de `STATE-06`
  não injeta cancelamento, deadline ou falha de sync/provider/store e não
  prova nesse nível que a geração ativa permanece íntegra e servível.
- `AQG-S06-003` (P2): Lifecycle exige exemplos reais no README em
  `STATE-06`, enquanto o roadmap os posiciona em `S08-B`/`BL-M13`. A
  divergência normativa foi preservada sem alteração; Lifecycle mantém maior
  precedência. O README não contém o exemplo e ainda descreve `STATE-03` como
  ativo, sem produto RAG funcional, migrations, stores ou parsers.
- Limitações/riscos: a reprodução limpa reutilizou os assets já restaurados da
  instalação existente; Windows x64 foi exercitado, não Linux ARM64 ou OCI.
  Cobertura percentual JavaScript, OCI/IAM/storage, backup/restore
  operacional, capacidade, desempenho, providers, fonte oficial, TLS/SSRF e
  corpus real permanecem não testados. Não houve captura de pacotes do host.
- Runtime e cleanup: todos os listeners, processos e stores temporários da
  tarefa foram encerrados/removidos. O artefato ignorado permanece no path
  validado `artifacts-local/s06-a/` e é reproduzível a partir da baseline.
  A política de execução recusou remover recursivamente o diretório exato de
  cobertura do gate sob `TestResults/`; ele permanece ignorado e contém
  somente evidência gerada, sem processo ou listener.
- Mudanças: somente
  `docs/STATE-06-Integration-Report.md`,
  `prompts/state/Current-State.md` e este histórico append-only registram o
  resultado factual; nenhum produto ou teste foi corrigido.
- Rollback: não executado nem necessário. Uma reversão futura exige
  autoridade própria e revert focal ordinário do commit de evidência,
  preservando a história append-only; não existe estado externo a reverter.
- Quality Gate: `REPROVADO` por três achados P2 abertos. O resultado não é
  `BLOQUEADO`, pois a evidência disponível permite concluir que há entregáveis
  de `STATE-06` não atendidos.
- Human Gate: prematuro, não solicitado e não executado.
- Estado resultante: `STATE-06 INTEGRATION` permanece ativo.
- Próxima condição: autoridade corretiva própria para `AQG-S06-001` a
  `AQG-S06-003`, seguida de reinício integral e separadamente autorizado do
  Automatic Quality Gate antes de qualquer resumo de Human Gate.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-06 — NORM-S06-001 e autoridade para S06-CORR-01

- Estado anterior e resultante: `STATE-06 INTEGRATION` ativo; Automatic
  Quality Gate `REPROVADO`; `AQG-S06-001` a `AQG-S06-003` abertos, todos P2;
  sem transição.
- Baseline autorizada: `main@140c0516e4dbfc02808a90f0496550eb6b09da1b`,
  corpus `4.9.2` e working tree limpa, reconfirmados no diretório e Git
  top-level canônicos antes de qualquer mudança ou acesso externo.
- Autoridade humana exata:

  ```text
  Decido NORM-S06-001: STATE-06 é responsável por um README factualmente atual e por pelo menos um exemplo cujo comando e resultado tenham sido realmente verificados contra o artefato integrado local/sintético. O texto deve explicitar essa fronteira e não pode alegar corpus, provider, fonte oficial, runtime Linux, OCI ou produção reais. STATE-08 permanece responsável pelo README público final e por complementar ou substituir esses exemplos com evidência própria de OCI e execução real do produto.

  Autorizo, em execução sequencial, AUTH-S06-NORM-001, AUTH-S06-DEP-001 e AUTH-S06-CORR-001 conforme a proposta.

  Para AUTH-S06-DEP-001, autorizo HTTPS somente por https://api.nuget.org/v3/index.json, sem host ou redirecionamento inesperado, e apenas para verificar e obter em cache isolado os candidatos Microsoft.NETCore.App.Runtime.linux-arm64 10.0.10, Microsoft.AspNetCore.App.Runtime.linux-arm64 10.0.10 e Microsoft.NETCore.App.Host.linux-arm64 10.0.10. Verifique fechamento do resolver, SHA-512 de catálogo, repository signatures em modo de revogação offline, licenças e advisories. Use C:\Projects\RAG-Challenge\artifacts-local\s06-dependencies\nuget-packages como cache isolado. Pare antes do restore se identidade, versão, origem, assinatura, hash ou fechamento diferir. Não altere NuGet.config nem qualquer lockfile fora de src/RagChallenge.Server.Api/packages.lock.json; pare se outro lockfile precisar mudar.

  Implemente somente S06-CORR-01, execute as verificações corretivas previstas e registre o resultado como CORRECTED_PENDING_GATE_RETEST em commits locais focais. Não execute AUTH-S06-AQG-RETEST-001, Human Gate ou STATE-07. Permanecem proibidos OCI, providers, contas, corpus ou fontes reais, GitHub, publicação, deploy, mudanças de ADR, contratos públicos, OpenAPI, schema ou migrations.
  ```

- Decisão normativa: `NORM-S06-001` aceita. `STATE-06` conserva um README
  factualmente atual e ao menos um exemplo local/sintético realmente
  verificado; `STATE-08` conserva a finalização pública com evidência própria
  de OCI e execução real do produto.
- Corpus: `4.9.3` (`PATCH`), sem mudança de arquitetura, ordem do lifecycle,
  capacidade ou estado. Lifecycle, roadmap, changelog e snapshot factual foram
  reconciliados; o histórico original do gate reprovado permanece intacto.
- Dependência autorizada: somente os três candidatos Linux ARM64 `10.0.10`,
  origem NuGet exata, cache isolado, verificação de supply chain e restore
  delimitado. Qualquer divergência de identidade, versão, origem, redirect,
  hash, assinatura, fechamento, target ou lockfile exige parada.
- Correção autorizada: plano/rehearsal OCI somente local, seam interno de teste,
  testes compostos de cancelamento/falha/preservação, README local/sintético,
  verificações completas, reconciliação factual e commits locais focais.
- Escopo negativo: sem novo Automatic Quality Gate, Human Gate, `STATE-07`,
  OCI real, providers, contas, secrets, corpus ou fontes reais, GitHub,
  publicação, deploy, ADR, contrato público, OpenAPI, schema, migration ou
  DB-Notifier.
- Runtime preflight: `NÃO APLICÁVEL` a este registro normativo; nenhuma
  inspeção ou interrupção de processo/listener foi realizada.
- Quality Gate: permanece `REPROVADO`; os três achados permanecem abertos até
  evidência corretiva e futura disposição por gate separadamente autorizado.
- Human Gate: prematuro, não solicitado e não autorizado.
- Próxima condição: concluir sequencialmente o intake delimitado e as correções
  de `S06-CORR-01`; parar antes de qualquer novo gate.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-06 — S06-CORR-01 bloqueado no gate de dependência Linux ARM64

- Estado anterior e resultante: `STATE-06 INTEGRATION` ativo; Automatic
  Quality Gate `REPROVADO`; `AQG-S06-001` a `AQG-S06-003` abertos, todos P2;
  sem transição e sem nova disposição.
- Baseline de execução: commit normativo
  `358a78fcf99179d5d5985cf356ced8ff7c4c959d`, corpus `4.9.3` e working tree
  limpa.
- Coleta externa autorizada: somente `api.nuget.org`, por HTTPS, sem redirect,
  com limites de tamanho e destino. Uma rota opcional presumida para
  `.nupkg.sha512` respondeu `404`, não foi repetida e foi substituída pelo
  `packageHash` SHA-512 da entrada primária de catálogo no mesmo host.
- Supply chain: os três packages Linux ARM64 `10.0.10` conferiram em identidade,
  versão, estado listed/stable, SHA-512 de catálogo, assinatura author e
  repository em revogação offline, licença MIT e fechamento com zero
  dependência declarada. A base NuGet continha advisories históricos para as
  famílias; avaliação exata dos ranges encontrou zero advisory aplicável a
  `10.0.10`, e o update vigente estava vazio.
- Cache: 42 packages de produção já locked foram copiados somente leitura do
  cache existente para o cache isolado autorizado. Os três nupkgs verificados
  formaram a única fonte local do restore; o cache global não foi alterado.
- Restore: execução local com `eng/NuGet.Offline.config`, RID `linux-arm64`,
  cache isolado e locked mode terminou com exit `1`/`NU1004`. Domain,
  Application, Infrastructure e Server precisam registrar o RID nos
  respectivos lockfiles.
- Condição de parada: a autoridade permite alterar somente
  `src/RagChallenge.Server.Api/packages.lock.json` e exige parada se outro
  lockfile precisar mudar. Nenhum lockfile ou outro arquivo rastreado mudou;
  não se tentou `--force-evaluate` nem uma alternativa que contornasse o grafo
  completo.
- Artefatos locais: nupkgs, catálogos, advisories, fonte verificada, cache
  isolado e assets do restore falho permanecem ignorados sob
  `artifacts-local/s06-dependencies/` ou `obj/`. Os targets RID gerados são não
  autoritativos e não constituem evidência de build.
- Runtime preflight: `NÃO APLICÁVEL`; nenhum comportamento executável foi
  alterado ou validado e nenhum processo/listener foi inspecionado ou parado.
- Resultado: `S06-CORR-01` está bloqueado no gate de dependência; C3/C4 não
  foram iniciados. O resultado esperado
  `CORRECTED_PENDING_GATE_RETEST` não foi alcançado.
- Escopo negativo preservado: sem OCI, provider, conta, secret, corpus ou
  fonte real, GitHub, publicação, deploy, ADR, contrato público, OpenAPI,
  schema, migration, novo Automatic Quality Gate, Human Gate ou `STATE-07`.
- Próxima condição: nova autoridade do proprietário para permitir exatamente
  os lockfiles de Domain, Application, Infrastructure e Server no restore do
  mesmo fechamento já verificado; qualquer package, versão, fonte ou lockfile
  adicional continua sendo condição de parada.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-06 — S06-CORR-01 corrigido e pendente de reteste do gate

- Estado anterior e resultante: `STATE-06 INTEGRATION` permanece ativo; o
  Automatic Quality Gate histórico permanece `REPROVADO`; não houve transição
  nem execução de novo gate.
- Reconciliação histórica: `Current-State` e o relatório ainda terminavam no
  bloqueio anterior de dependência porque C5 não havia sido alcançado. Este
  registro preserva aquela parada e incorpora as autoridades, commits e
  evidências corretivas posteriores sem reescrever o resultado original.
- Primeira ampliação de dependência: sobre
  `main@872c62a093f4df6549357f3a601f2f1d61943e0d`, corpus `4.9.3` e working
  tree limpa, o proprietário permitiu que somente os lockfiles de Domain,
  Application, Infrastructure e Server incorporassem `linux-arm64` por
  `--force-evaluate`, seguido de restore locked. O commit
  `4b808319b0c1abf0970f9f41c77fb1e08d295585` adicionou apenas o target RID e
  os três runtime packs Microsoft .NET/ASP.NET Core `10.0.10` já verificados.
- Correções offline: o commit
  `405ab20d3e76a75f1a0f50fd625ec71831b9134b` adicionou o plano e rehearsal
  ARM64; `801f77625e68692fe7b4691798694b4e8d92433a` adicionou a seam interna e
  as provas compostas de cancelamento, falha de provider/fonte, consulta
  posterior e restart; `9d72a1bb93325f6303516592fb4ff352a0a531ca` tornou o README factual e
  limitado ao exemplo local/sintético verificado.
- Bloqueio intermediário de C4: o primeiro gate agregado parou no restore com
  `NU1004`, porque os quatro projetos ainda não declaravam o RID contido nos
  lockfiles. C5 não foi executado nem antecipado naquele ponto.
- `AUTH-S06-DEP-002`: autorizada sobre
  `main@9d72a1bb93325f6303516592fb4ff352a0a531ca`, corpus `4.9.3` e working
  tree limpa, exclusivamente para declarar `linux-arm64` nos quatro projetos
  de produção e copiar por leitura ao cache isolado os packages já locked
  `coverlet.collector` `10.0.1`, `Microsoft.CodeCoverage`,
  `Microsoft.NET.Test.Sdk`, `Microsoft.TestPlatform.ObjectModel` e
  `Microsoft.TestPlatform.TestHost` `18.7.0`, `xunit` `2.9.3`,
  `xunit.abstractions` `2.0.3`, `xunit.analyzers` `1.18.0`, `xunit.assert`,
  `xunit.core`, `xunit.extensibility.core` e
  `xunit.extensibility.execution` `2.9.3`, e `xunit.runner.visualstudio`
  `3.1.5`.
- Cache e restore: os 13 diretórios de origem existiam, os destinos estavam
  ausentes e os digests SHA-256 determinísticos de cada árvore foram idênticos
  entre origem e cópia; a origem permaneceu inalterada. O restore da solução
  usou somente a fonte local verificada, cache isolado, revogação offline,
  `--no-cache` e locked mode, terminou com exit `0` e não alterou bytes ou
  grafos de qualquer um dos sete lockfiles. Os quatro projetos de produção
  materializaram somente `net10.0` e `net10.0/linux-arm64`; os projetos de
  teste conservaram somente `net10.0`. O commit
  `f1a02cd7c7acb50bcd3fa8b00e69e6c3f59b88c3` registra apenas as quatro
  declarações de projeto.
- Gate técnico corretivo C4: `eng/ci.ps1 -Offline` passou com restore locked,
  format, build Release sem warning/erro, 74 testes unitários, 10 de
  arquitetura e 95 de integração, total 179 sem falha ou skip, cobertura de
  92,40% de linhas e 66,60% de branches, `npm ci --offline`, lint, typecheck,
  38 testes npm, build Vite, auditoria de 198 arquivos e higiene Git.
- Reprodução ARM64: duas construções consecutivas no mesmo commit limpo
  produziram ZIP idêntico de 133.379.066 bytes e 361 entradas, SHA-256
  `0dfdf1c0604e8ccf9e3064d8131e48ae463cf655c0723dc57ebab4b06d2a2880`.
  O manifesto de 360 payloads também foi idêntico, SHA-256
  `ceafd82aadbf6552d16fd427dde534fc3feac54b2bcdf3069501ccbb8be54f65`;
  o verificador aprovou 17 payloads ELF64 AArch64, integridade, paths,
  Dashboard, configuração fail-closed e scan aparente de secrets. O binário
  ARM64 não foi executado e OCI não foi contatado.
- README: os dois comandos publicados foram executados literalmente. O
  artefato local possui 58 arquivos e 47.234.206 bytes, SHA-256
  `147586466ca5a92ac18760c77822d78899200ea4c72b0898a66b45b9aafb7301`;
  a reprodução retornou `Passed`, serviu o Dashboard, respondeu em `en-GB` e
  `pt-BR` e preservou a mesma geração ativa após restart. Fixture, providers,
  stores e listener permaneceram locais/sintéticos.
- Higiene final de C4: working tree limpa, `git diff --check` aprovado, zero
  processo pertencente ao RAG-Challenge e zero listener nas portas 4173, 5086,
  5096, 5173 e 9230.
- Disposição corretiva: `AQG-S06-001`, `AQG-S06-002` e `AQG-S06-003` estão
  `CORRECTED_PENDING_GATE_RETEST`. Essa classificação não os resolve, não
  aprova o gate e não substitui sua repetição integral.
- Escopo negativo preservado: sem rede nova, Linux executado, OCI, provider,
  conta, secret, corpus ou fonte real, GitHub, publicação, deploy, ADR,
  contrato público, OpenAPI, schema, migration, novo Automatic Quality Gate,
  Human Gate, `STATE-07` ou DB-Notifier.
- Rollback: não executado. Reversão futura exige autoridade própria, reverts
  focais ordinários e registro compensatório append-only; não existe estado
  externo a reverter.
- Relatório reconciliado:
  [`STATE-06-Integration-Report.md`](../../docs/STATE-06-Integration-Report.md).
- Próxima condição: `AUTH-S06-AQG-RETEST-001` separada sobre baseline limpa
  para reiniciar integralmente o Automatic Quality Gate; Human Gate continua
  prematuro.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-06 — Reinício integral do Automatic Quality Gate de STATE-06 aprovado

- Estado preservado: `STATE-06 INTEGRATION` ativo.
- Baseline auditada:
  `main@9d7c4ce816eca049ba09942ab7fe8b1148aa73c9`, corpus `4.9.3` e working
  tree limpa.
- Autoridade: `AUTH-S06-AQG-RETEST-001`, concedida pelo proprietário para
  reiniciar integralmente o gate localmente, offline, sem correção silenciosa
  e com parada diante de achado, divergência, mudança concorrente ou nova
  necessidade de autoridade.
- Preflight restrito: zero processo pertencente ao RAG-Challenge e zero
  listener nas portas 4173, 5086, 5096, 5173 e 9230; o editor aberto no
  repositório não era processo do produto e permaneceu intacto.
- Auditoria estática: o diff corretivo completo, seam interno de composição,
  defaults de produção, testes focais, projetos, lockfiles, scripts de
  rehearsal/CI, configuração e comandos publicados foram revistos sem achado.
- Supply chain e restore: os três runtime packs ARM64 `10.0.10` corresponderam
  ao catálogo local, hashes, assinaturas author/repository, licença MIT e zero
  dependências. O restore locked usou somente fonte verificada e caches
  isolados, com revogação offline e `--no-cache`, sem mudar qualquer um dos
  sete lockfiles.
- Gate técnico: `eng/ci.ps1 -Offline` aprovou restore, format, build Release
  sem warning/erro, 74 testes unitários, 10 de arquitetura e 95 de integração,
  total 179 sem falha ou skip; cobertura combinada de 92,40% de linhas e
  66,60% de branches; `npm ci --offline`, lint, typecheck, 38 testes npm, build
  Vite, auditoria de 198 arquivos e higiene Git.
- Testes focais: quatro provas no host composto aprovaram sucesso/restart,
  cancelamento, falhas limitadas de provider/fonte oficial, recuperação e novo
  restart contra a mesma geração ativa.
- Reprodução ARM64: duas construções consecutivas produziram arquivos
  idênticos de 133.379.066 bytes e 361 entradas, SHA-256
  `d539f0dd27553859966fe45f373363d32ffd34c61cd59618fe7cf61dcd9b2369`;
  os manifestos de 360 payloads foram idênticos, SHA-256
  `ba2ba62001b6da0fb4c9405fcd419d398d491dee0557fa1ceb035394c865fddb`.
  O verificador aprovou 17 payloads ELF64 AArch64 sem executar Linux nem
  contatar OCI.
- README: os dois comandos locais publicados produziram um ZIP de 58 arquivos
  e 47.234.206 bytes, SHA-256
  `fc3604a8d99a87c0f0d71b37309c125f7645ba1516ee833cba30ff3310a39a2f`;
  a reprodução retornou `Passed`, serviu o Dashboard, respondeu em `en-GB` e
  `pt-BR` e preservou a mesma geração ativa após restart.
- Segurança e higiene: nenhum contrato/OpenAPI, dependência, schema, migration
  ou ADR protegido mudou no intervalo corretivo; zero `reference-materials/`
  rastreado, zero runtime pertencente ao produto, zero listener de tarefa e
  `git diff --check` aprovado. O OpenAPI permaneceu com SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
- Disposição: `AQG-S06-001`, `AQG-S06-002` e `AQG-S06-003` estão
  `RESOLVIDOS`. Nenhum novo P0, P1, P2 ou P3 foi identificado.
- Resultado: Automatic Quality Gate `APROVADO`; o gate reprovado e as
  disposições corretivas permanecem como histórico, sem confusão entre
  correção e aprovação.
- Limitações preservadas: sem execução Linux ARM64, OCI, rede, cache global,
  providers, contas, secrets, corpus ou fonte real, GitHub, publicação, deploy,
  alteração de contrato público/OpenAPI/schema/migration/ADR, Human Gate ou
  `STATE-07`. Cobertura percentual JavaScript e observação de pacotes de rede
  não foram executadas.
- Relatório atualizado:
  [`STATE-06-Integration-Report.md`](../../docs/STATE-06-Integration-Report.md).
- Resultado de estado: `STATE-06` permanece ativo; o Human Gate não foi
  executado e requer autoridade humana separada sobre baseline limpa.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-06 — Reconciliação factual pós-auditoria de AST-001 a AST-003

- Estado anterior e resultante: `STATE-06 INTEGRATION` permanece ativo; não
  houve transição, Automatic Quality Gate, Human Gate ou entrada em
  `STATE-07`.
- Autoridade: o proprietário autorizou exclusivamente registrar a baseline,
  os commits, as verificações, as limitações, os riscos residuais e a
  disposição dos três achados na documentação técnica proprietária, no
  snapshot factual e neste histórico append-only.
- Baseline: os quatro commits corretivos formam um intervalo contínuo depois
  do anchor pré-correção
  `main@bfc3aefc3a731b1b49b47458374cb903860faf6f`. A revisão
  pós-correção ocorreu sobre
  `main@dc3dde2437ad3cbb50b397358fcda043c9d6f4b3`, corpus `4.9.3` e working
  tree limpa.
- `AST-001`: o commit
  `cfb93892571bec1beae3087b1f5ff44932d24693` passou a reconstruir e validar,
  na transação de commit, a igualdade exata entre o conjunto completo de
  documentos ativos e os nove campos generation-bound de cada binding. O
  commit `dc3dde2437ad3cbb50b397358fcda043c9d6f4b3` adicionou a migration
  `StrengthenOfficialBindingReferences`, com referências compostas de
  snapshot/registro para observações, manifests e ativações. Disposição:
  `RESOLVIDO`.
- `AST-002`: o commit
  `0b3c5be2c80f0f1ee83af82d2158e87360c33ea7` passou a resolver primeiro o
  snapshot imutável e a exigir a revisão exata de registro, produto, documento
  e adapter vinculada a ele. Disposição: `RESOLVIDO`.
- `AST-003`: o commit
  `d3fa9d77863092918dbef6fa7afee12992c2053f` introduziu o seletor vetorial
  generation-bound de nove campos, incluiu corpus, geração e seletor em cada
  hit e passou a validar toda a autoridade antes de threshold ou uso do modelo
  de linguagem. Disposição: `RESOLVIDO`.
- Evidência executável: 23 de 23 testes dirigidos de commit de geração, 5 de 5
  testes dirigidos da migration e 35 de 35 na regressão ampliada passaram. A
  consolidação aprovou build Release sem warning/erro, 87 testes unitários, 10
  de arquitetura e 109 de integração, total 206 sem falha ou skip,
  `dotnet format --verify-no-changes --no-restore`, ausência de mudança
  pendente no modelo EF e `git diff --check`.
- Compatibilidade: contrato canônico v1, API pública e OpenAPI permaneceram
  inalterados; o contrato vetorial mudou somente no port interno entre
  Application e Infrastructure.
- Limitações e riscos residuais: a migration foi exercitada somente em bancos
  SQLite descartáveis; nenhum banco real foi migrado. Inconsistência legada em
  observação, manifest ou ativação bloqueia o upgrade e não é reparada
  automaticamente. `document_versions` permanece sem FK direta ao snapshot
  para preservar a ordem catálogo → snapshot; o commit de geração falha
  fechado antes de persistir manifesto incompatível. Downgrade operacional ou
  reparo de dados exige autoridade separada.
- Limites externos: nenhuma fonte externa de vulnerabilidade, provider,
  conta, secret, corpus ou fonte real, Linux executado, OCI, GitHub,
  publicação, push, deploy ou DB-Notifier foi acessado ou alterado.
- Gate boundary: o Automatic Quality Gate aprovado em
  `main@9d7c4ce816eca049ba09942ab7fe8b1148aa73c9` permanece histórico, mas
  não cobre os quatro commits AST posteriores. Esta reconciliação não repetiu
  o gate e não autoriza Human Gate ou `STATE-07`.
- Relatório atualizado:
  [`STATE-06-Integration-Report.md`](../../docs/STATE-06-Integration-Report.md).
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-06 — Automatic Quality Gate pós-AST de STATE-06 aprovado

- Estado anterior e resultante: `STATE-06 INTEGRATION` permanece ativo; não
  houve Human Gate, transição ou entrada em `STATE-07`.
- Autoridade e baseline: o proprietário autorizou o reinício integral, local,
  offline e sequencial sobre
  `main@726546dbe0302b9664a62e890b6a27f19bf0c6e4`, corpus `4.9.3` e working
  tree inicialmente limpa, com revisão do diff completo de 20 arquivos após o
  gate aprovado em `9d7c4ce816eca049ba09942ab7fe8b1148aa73c9`.
- Parada inicial: a primeira tentativa identificou `AQG-S06-004`, porque as
  working copies de
  `20260806193919_StrengthenOfficialBindingReferences.Designer.cs` e
  `ControlPlaneDbContextModelSnapshot.cs` apresentavam EOL misto apesar do
  índice LF. O gate parou sem correção silenciosa.
- Remediação autorizada: somente os bytes dessas duas working copies foram
  normalizados para LF. Seus Git blob object IDs coincidiram com `HEAD`, o
  status voltou a ficar limpo, ambos os arquivos reportaram `i/lf w/lf` e
  `eng/check-repository.ps1` aprovou 200 arquivos não ignorados. Não houve
  mudança semântica, staged diff, schema, migration ou modelo EF novo.
  Disposição de `AQG-S06-004`: `RESOLVIDO`.
- Limpeza intermediária: o diretório de cobertura
  `TestResults/f019fb042ddb46e8855900f132859c70` da tentativa interrompida foi
  removido antes do reinício integral e a baseline limpa foi reconfirmada.
- Execução aceita: entre `2026-08-06T21:34:47.2595338Z` e
  `2026-08-06T21:41:17.7811145Z`, o preflight encontrou zero processo do
  produto e zero listener nas portas 4173, 5086, 5096, 5173 e 9230. A
  toolchain foi PowerShell `7.6.4`, Git `2.55.0.windows.3`, .NET SDK
  `10.0.302`, Node.js `24.19.0` e npm `11.17.0`.
- Supply chain: os três runtime packs Linux ARM64 Microsoft `10.0.10` já
  disponíveis passaram as verificações offline de assinatura author e
  repository. Os hashes dos sete lockfiles permaneceram idênticos antes e
  depois; nenhuma dependência foi instalada ou alterada.
- Gate técnico: `eng/ci.ps1 -Offline` aprovou restore locked, format, build
  Release sem warning/erro, 87 testes unitários, 10 de arquitetura e 109 de
  integração, total 206 sem falha ou skip, cobertura .NET de 93,11% de linhas
  e 66,89% de branches, `npm ci --offline`, lint, typecheck, 38 testes npm,
  build Vite e auditoria de 200 arquivos.
- Verificações focais: quatro testes de host composto/loopback e cinco testes
  de migration e referências compostas passaram; estes últimos usaram
  somente bancos SQLite descartáveis. A invocação EF aceita usou o
  `dotnet-ef` `10.0.10` já instalado, Infrastructure como projeto e startup e
  store root temporário explícito, e informou ausência de mudança pendente no
  modelo.
- Invocações não aceitas como evidência: o tool home isolado não expôs o
  `dotnet-ef` global e solicitou restore, que não foi executado; o startup
  project Server.Api não continha o package EF Design; e Infrastructure sem
  `RAGCHALLENGE_DESIGN_TIME_STORE_ROOT` falhou fechado como esperado. Nenhuma
  dessas tentativas demonstrou consistência de migration.
- Reprodução ARM64: duas construções produziram arquivos idênticos de
  133.455.866 bytes e 361 arquivos, SHA-256
  `059dec2e653d16a85ec173db535f796110df305a59943420b0f3375d84f17c66`;
  os manifestos de 360 payloads também foram idênticos, SHA-256
  `22d9c4b3b0028defed869c336fa3ea75a37ae1b4b073a174438ba19af67dbd4a`.
  O verificador aprovou 17 ELF64 AArch64 sem executar Linux nem contatar OCI.
- README: os comandos literais de integração produziram um artefato ignorado
  de 58 arquivos e 47.324.398 bytes, SHA-256
  `fd9efb4636e13337fb7b77245d8a88b9d58703c82fc49e7b67971a32f409b297`;
  a verificação retornou `Passed`, serviu o Dashboard em loopback, respondeu
  em `en-GB` e `pt-BR`, preservou a geração ativa após restart e confirmou os
  bancos persistentes sintéticos.
- Segurança e compatibilidade: zero mudança inesperada em path protegido,
  zero aparente secret e nenhum `reference-materials/` rastreado. Os Git blob
  object IDs de OpenAPI e contrato canônico v1 permaneceram
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160` e
  `eed1021776fc6513d0054c5a6a8babe3a4534150`, respectivamente.
- Limpeza final: removidos `artifacts-local/aqg-s06-726546d` e
  `TestResults/2d56d4ed0a754d539894ae348a1f686e`; zero processo/listener da
  tarefa, branch e commit preservados e staged/unstaged diff limpos. O
  artefato ignorado criado pelo comando literal do README permanece em
  `artifacts-local/s06-a`; demais outputs ignorados são não autoritativos.
- Resultado: Automatic Quality Gate `APROVADO`, sem novo P0, P1, P2 ou P3;
  `AQG-S06-004` está `RESOLVIDO`.
- Limitações e riscos residuais: sem execução Linux ARM64, OCI real e seus
  controles de IAM/capacidade/rede/storage/custo/TLS, provider, conta, secret,
  corpus ou fonte oficial real, armazenamento operacional, cobertura
  percentual JavaScript, observação de pacotes de rede, migration em banco
  real ou reparo de dados.
- Escopo negativo preservado: sem contrato, schema, migration, ADR, código,
  lifecycle, Human Gate, `STATE-07`, ação externa, publicação, push ou deploy.
- Relatório atualizado:
  [`STATE-06-Integration-Report.md`](../../docs/STATE-06-Integration-Report.md).
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-06 — Automatic Quality Gate em f92e26c reprovado por AQG-S06-005

- Estado anterior e resultante: `STATE-06 INTEGRATION` permanece ativo; não
  houve Human Gate, transição ou entrada em `STATE-07`.
- Autoridade e baseline: o proprietário autorizou reconciliar os oito commits
  posteriores a `bfc3aefc3a731b1b49b47458374cb903860faf6f` e reiniciar
  integralmente o gate localmente, offline e de forma sequencial sobre
  `main@f92e26c7008a2d124bd10edb2e3f03c0c9ad2bf6`, corpus `4.9.3` e working
  tree limpa, com parada no primeiro achado ou divergência.
- Intervalo reconciliado: oito commits, 25 arquivos, 4.030 inserções e 167
  remoções, abrangendo as correções AST, sua documentação factual e os dois
  commits finais de controles de cobertura/CI.
- Preflight restrito: zero processo pertencente ao RAG-Challenge e zero
  listener nas portas 4173, 5086, 5096, 5173 e 9230.
- Higiene estática inicial: `git diff --check` do intervalo aprovado; zero path
  de dependência ou OpenAPI alterado; nenhum `reference-materials/` rastreado.
- Achado `AQG-S06-005` (P2): os dois únicos testes PowerShell dos controles
  fail-closed, `eng/test-assert-coverage.ps1` e `eng/test-ci-policy.ps1`, não
  possuem invocação fora de suas próprias definições. O workflow chama somente
  `./eng/ci.ps1`, e esse entry point também não executa os testes.
- Impacto: a CI canônica pode aprovar sem exercitar a regressão do parser de
  cobertura ou da política compartilhada que pretende garantir o
  comportamento fail-closed do próprio gate.
- Disposição: `AQG-S06-005` permanece `ABERTO`. A correção exige autoridade
  separada para integrar os dois testes ao entry point automático com
  propagação fail-closed, seguida por novo reinício integral sobre baseline
  limpa.
- Parada obrigatória: nenhuma verificação executável posterior foi iniciada;
  restore, build, suítes .NET/npm, coverage, persistence/migration, EF,
  cancelamento/resiliência, duas reproduções ARM64, verificador estático e
  comandos do README permanecem não executados neste gate.
- Resultado: Automatic Quality Gate `REPROVADO`, com um novo P2 e nenhum novo
  P0, P1 ou P3 identificado antes da parada.
- Escopo negativo preservado: nenhuma correção de source/test/workflow,
  dependência, lockfile, contrato, OpenAPI, schema, migration ou ADR; sem rede,
  cache global, OCI, provider, conta, secret, corpus ou fonte real, GitHub,
  publicação, deploy, Human Gate ou `STATE-07`.
- Relatório atualizado:
  [`STATE-06-Integration-Report.md`](../../docs/STATE-06-Integration-Report.md).
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-06 — Correção focal de AQG-S06-005 concluída

- Estado anterior e resultante: `STATE-06 INTEGRATION` permanece ativo; o
  Automatic Quality Gate continua historicamente `REPROVADO` e não houve
  Human Gate, transição ou entrada em `STATE-07`.
- Autoridade e baseline: o proprietário autorizou somente a correção focal de
  `AQG-S06-005` sobre
  `main@000dca0210e220a9f247159178c6d97d9fc4fd55`, corpus `4.9.3` e working
  tree limpa, com verificações focais locais/offline e commit local.
- Preflight restrito: zero processo pertencente ao RAG-Challenge e zero
  listener nas portas 4173, 5086, 5096, 5173 e 9230.
- Implementação: `eng/ci-policy.ps1` ganhou o helper
  `Invoke-RequiredPolicyTest`, que falha se o script estiver ausente e propaga
  exceção do teste com contexto; `eng/ci.ps1` chama os testes de coverage e
  política exatamente uma vez antes de restore; `eng/test-ci-policy.ps1`
  valida sucesso, falha propagada, script ausente, as duas invocações únicas e
  o workflow como consumidor único do entry point canônico.
- Workflow: `.github/workflows/ci.yml` permaneceu sem mudança e continua
  chamando somente `./eng/ci.ps1`.
- Verificação focal aceita: parsing dos três scripts alterados; 11 casos de
  `eng/test-assert-coverage.ps1`; 14 controles de `eng/test-ci-policy.ps1`;
  `git diff --check`; e auditoria de 203 arquivos não ignorados passaram.
- Tentativa diagnóstica não aceita: a primeira wrapper de verificação tinha
  interpolação PowerShell inválida e parou antes de analisar arquivo do projeto
  ou executar teste. A invocação corrigida produziu toda a evidência aceita.
- Disposição: `AQG-S06-005` está `CORRECTED_PENDING_GATE_RETEST`; a correção
  não resolve o achado nem substitui novo reinício integral do gate sobre
  baseline limpa.
- Verificações não executadas: restore, build, suítes .NET/npm, coverage de
  produção, persistence/migration, EF, cancelamento/resiliência, ARM64 e
  comandos do README ficaram fora da correção focal.
- Escopo negativo preservado: sem produto, dependência, lockfile, contrato,
  OpenAPI, schema, migration, ADR, rede, cache global, OCI, provider, conta,
  secret, corpus ou fonte real, GitHub, publicação, deploy, Human Gate ou
  `STATE-07`.
- Relatório atualizado:
  [`STATE-06-Integration-Report.md`](../../docs/STATE-06-Integration-Report.md).
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-06 — Automatic Quality Gate aprovado após correção de AQG-S06-005

- Estado anterior e resultante: `STATE-06 INTEGRATION` permanece ativo; não
  houve Human Gate, transição ou entrada em `STATE-07`.
- Autoridade e baseline: o proprietário autorizou o reinício integral, local,
  offline, sequencial e sem correção silenciosa sobre
  `main@616bef4e2ae8c0b26c10781cd728dc6089136a60`, corpus `4.9.3` e working
  tree limpa, com parada diante de achado, divergência ou mudança concorrente.
- Preflight restrito: zero processo pertencente ao RAG-Challenge e zero
  listener nas portas 4173, 5086, 5096, 5173 e 9230 no início e no fim.
- Supply chain: os três runtime packs Linux ARM64 `10.0.10` foram reconciliados
  com catálogo local, SHA-256, SHA-512, tamanho, licença, identidade e
  assinatura; o restore locked usou somente a fonte verificada e caches .NET
  isolados, sem alteração dos sete lockfiles .NET.
- Controles automáticos: `eng/ci.ps1 -Offline` executou antes do restore os 11
  casos de `test-assert-coverage.ps1` e os 14 controles de
  `test-ci-policy.ps1`, incluindo sucesso, falha propagada, script ausente,
  invocação única e workflow como consumidor do entry point canônico.
- Gate técnico: restore, format, build Release sem warning/erro, 87 testes
  unitários, 10 de arquitetura e 109 de integração passaram, total 206 sem
  falha ou skip; cobertura .NET foi 93,11% de linhas e 66,89% de branches;
  `npm ci --offline`, lint, typecheck, 38 testes npm, build Vite e auditoria de
  203 arquivos passaram.
- Persistência e resiliência: cinco testes focais de migration/referências
  compostas e quatro de host/loopback passaram; EF `10.0.10` informou ausência
  de mudança pendente no modelo com store root temporário explícito. Nenhuma
  migration foi aplicada a banco real.
- Reprodução ARM64: duas construções produziram ZIPs idênticos de 133.455.866
  bytes e 361 arquivos, SHA-256
  `539a187debc1f9a39cf95ee8519763434e7e96e76e6eeaae0812ad697b8200a9`;
  manifests de 360 payloads idênticos tiveram SHA-256
  `4c3c18da88658dbb2671f7183249fe1db1ca1b442d9e4224f1762c8c5dc74ea3`.
  O verificador aprovou 17 ELF64 AArch64 sem executar Linux nem contatar OCI.
- Integridade da evidência ARM64: uma asserção ad hoc inicialmente tratou o
  JSON compactado do verificador como objeto PowerShell e produziu falso
  status nulo; comparação direta de ZIP, manifest e conteúdo e duas leituras
  com `ConvertFrom-Json` confirmaram equivalência e aprovação. A tentativa
  intermediária com parâmetro inexistente não produziu evidência do projeto.
- README: os comandos literais produziram artefato ignorado de 58 arquivos e
  47.324.394 bytes, SHA-256
  `260f072109bf44b9ea09f737995ff9cf036c7f1de853c67307e1c2f2d245a763`;
  a verificação passou com Dashboard loopback, `en-GB`/`pt-BR`, geração
  preservada após restart, `control.db` e `vectors.db`.
- Segurança e higiene: nenhum path protegido inesperado mudou, nenhum
  `reference-materials/` está rastreado, os blobs de OpenAPI e contrato
  canônico v1 permaneceram
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160` e
  `eed1021776fc6513d0054c5a6a8babe3a4534150`; auditoria e
  `git diff --check` passaram. Os diretórios exclusivos do gate e da cobertura
  foram removidos; fonte/cache local verificada e artefato ignorado do README
  foram preservados.
- Disposição: `AQG-S06-005` está `RESOLVIDO`; o Automatic Quality Gate está
  `APROVADO`, sem novo P0, P1, P2 ou P3. `AQG-S06-001` a `AQG-S06-005`
  permanecem `RESOLVIDOS`.
- Limitações: sem execução Linux ARM64, OCI real, provider, conta, secret,
  corpus ou fonte real, armazenamento operacional, cobertura percentual
  JavaScript, observação de rede em nível de pacotes, migration em banco real
  ou reparo de dados.
- Escopo negativo preservado: sem correção silenciosa, dependência, lockfile,
  contrato, OpenAPI, schema, migration, ADR, rede externa, OCI, GitHub,
  publicação, push, deploy, Human Gate ou `STATE-07`.
- Relatório atualizado:
  [`STATE-06-Integration-Report.md`](../../docs/STATE-06-Integration-Report.md).
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-06 — Human Gate de STATE-06 aprovado com ressalvas

- Estado anterior: `STATE-06 INTEGRATION` ativo, Automatic Quality Gate
  `APROVADO`, `AQG-S06-001` a `AQG-S06-005` e `AST-001` a `AST-003`
  `RESOLVIDOS`, Human Gate `PENDENTE` e `STATE-07` não autorizado.
- Baseline: `main@2f70705dcbe293b22ccd039d0764b2b9ca4b2e8a`, corpus `4.9.3` e working
  tree limpa, reconfirmados antes e depois da preparação do resumo.
- Autoridade: preparar e apresentar somente o resumo completo do Human Gate de
  `STATE-06`, sem executar `STATE-07`, rede, OCI ou ação externa. Runtime
  preflight foi `NÃO APLICÁVEL` ao trabalho documental/read-only.
- Resumo revisado: entregáveis e critérios de aceite de `STATE-06`, Automatic
  Quality Gate aprovado, achados históricos e disposições, supply chain e
  restore locked isolado, 206 testes .NET, 38 testes npm, cobertura, stores e
  restart, cancelamento/resiliência, migration/EF, duas reproduções ARM64,
  comandos do README, segurança, rollback, limitações e escopo negativo foram
  apresentados na mesma conversa.
- Amostras técnicas repetidas pelo Automatic Quality Gate: fluxo same-origin
  nos dois idiomas, fonte oficial falsa em loopback, persistência e restart,
  cancelamento, falhas limitadas de provider/fonte, recuperação, migration em
  SQLite descartável, verificação EF, duas reproduções ARM64 idênticas e os
  comandos literais do README.
- Amostras humanas não executadas nesta autoridade: fluxo PDF ao vivo,
  execução Linux ARM64, OCI real, providers, corpus e fontes oficiais reais,
  armazenamento operacional e migration de banco real. O proprietário aceitou
  a evidência automática e manteve essas ausências como ressalvas explícitas.
- Confirmação humana exata:

  ```text
  Confirmo a decisão acima exclusivamente para STATE-06
  ```

- Decisão: Human Gate de `STATE-06` `APROVADO COM RESSALVAS`, exclusivamente
  para a fronteira local, offline, sintética e estática documentada.
- Ressalvas preservadas: sem execução Linux ARM64; sem OCI real e seus
  controles de tenancy, IAM, capacidade, rede, TLS, storage, custo, backup ou
  restore; sem provider, conta, secret, corpus, fonte oficial ou armazenamento
  operacional reais; sem cobertura percentual JavaScript ou observação de
  rede em nível de pacotes; migration somente em SQLite descartável, sem
  aplicação ou reparo em banco real; artefactos ignorados não autoritativos.
- Resultado do lifecycle: `STATE-06 INTEGRATION` está encerrado. A decisão não
  autoriza nem inicia `STATE-07` e não concede autoridade externa.
- Mudanças deste registro: somente README e os três artefatos factuais de
  `STATE-06`; nenhum código, teste, workflow, dependência, lockfile, contrato,
  OpenAPI, schema, migration ou ADR foi alterado.
- Escopo negativo preservado: sem `STATE-07`, rede, OCI, provider, conta,
  secret, fonte real, GitHub, publicação, push ou deploy.
- Relatório atualizado:
  [`STATE-06-Integration-Report.md`](../../docs/STATE-06-Integration-Report.md).
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-06 — Entrada documental em STATE-07 TESTING_HOMOLOGATION autorizada

- Estado anterior: `STATE-06 INTEGRATION` encerrado após Automatic Quality
  Gate `APROVADO` e Human Gate `APROVADO COM RESSALVAS`; `STATE-07` ainda não
  iniciado nem autorizado.
- Estado solicitado: `STATE-07 TESTING_HOMOLOGATION`.
- Baseline confirmada antes da escrita: localização
  `C:\Projects\RAG-Challenge`, Git top-level `C:/Projects/RAG-Challenge`, Git
  directory `.git`, branch `main`, commit
  `3240a4b13acd82a1cf5815ac64f6997b2a7f89bf`, corpus `4.9.3` e working tree
  limpa.
- Autoridade: solicitação explícita do proprietário para retomar e registrar
  exclusivamente a entrada documental em `STATE-07`, reconciliar os blocos
  de status estritamente necessários em `README.md` e `docs/README.md`,
  validar o incremento e criar um commit local focal.
- Achado reconciliado: o bloco `Status` de `docs/README.md` ainda declarava
  corpus `4.9.1`, `STATE-03` ativo e `STATE-04` pendente. A execução anterior
  parou antes de editar; o proprietário autorizou explicitamente corrigir
  somente esse bloco no mesmo incremento.
- Decisão: registrar `STATE-07 TESTING_HOMOLOGATION` como ativo exclusivamente
  no lifecycle documental, sem autorizar ou executar qualquer lote.
- Escopo: atualizar o snapshot factual e este histórico append-only;
  reconciliar somente os blocos públicos de status necessários; validar
  escopo, links, UTF-8/LF, whitespace, coerência factual e diff; criar um
  único commit local focal.
- Escopo negativo: sem dataset, avaliação RAG, testes, carga, segurança
  dinâmica, browser, providers, fontes reais, rede, OCI, GitHub, publicação,
  deploy, `STATE-08` ou ação externa; sem código, testes, dependências,
  lockfiles, contratos, OpenAPI, schema, migrations ou ADRs.
- Pré-condições: fechamento de `STATE-06` e suas ressalvas preservados;
  baseline exata e limpa reconfirmada; divergência do índice público disposta
  por autoridade adicional na mesma conversa.
- Runtime preflight: `NÃO APLICÁVEL`; o incremento altera somente documentação
  e memória de lifecycle, sem alterar ou validar comportamento executável.
- Mudanças: `prompts/state/Current-State.md` registra `STATE-07` ativo sem
  lote; esta entrada preserva autoridade, baseline e limites; `README.md` e
  `docs/README.md` refletem somente o status factual vigente.
- Verificações/evidências: branch, HEAD, corpus, localização, Git top-level,
  Git directory e working tree foram reconfirmados antes da escrita; o diff
  inicial permaneceu restrito aos documentos autorizados e passou em
  `git diff --check`.
- Validação final: `eng/check-repository.ps1` aprovou 203 arquivos não
  ignorados; o diff permaneceu restrito aos quatro documentos autorizados; o
  histórico possui um único hunk terminal e zero remoção; UTF-8 sem BOM, LF,
  newline final, whitespace, NUL, links, assertions factuais e
  `git diff --check` foram aprovados.
- Limitações/riscos: todas as ressalvas do Human Gate de `STATE-06`
  permanecem. A entrada documental não materializa dataset, threshold,
  ambiente, provider, corpus, fonte, teste ou evidência de homologação.
- Automatic Quality Gate de `STATE-07`: `PENDENTE` e não executado.
- Human Gate de `STATE-07`: `PENDENTE`, não preparado e não executado.
- Estado resultante: `STATE-07 TESTING_HOMOLOGATION` ativo exclusivamente por
  entrada documental; nenhum lote está autorizado ou executado.
- Próxima condição: autoridade humana explícita e separada para qualquer lote
  delimitado de `STATE-07`, com dataset, checks, ambiente, escopo negativo e
  condições de parada próprios.
- Aprovador da entrada: proprietário do RAG-Challenge.

## 2026-08-07 — Proposta S07-A confirmada como baseline de planejamento

- Estado anterior: `STATE-07 TESTING_HOMOLOGATION` ativo exclusivamente por
  entrada documental; a proposta de `S07-A` estava registrada no commit
  `183c8cd9fe303096a355ab731e72dc81748eb626`, sem autoridade de execução.
- Baseline: branch `main`, commit
  `183c8cd9fe303096a355ab731e72dc81748eb626`, corpus `4.9.3` e working tree
  limpa, reconfirmados antes do registro.
- Confirmação do proprietário:

  ```text
  Confirmo a proposta documental de S07-A exclusivamente como baseline de planejamento.

  Esta confirmação não autoriza AUTH-S07-A-DATASET-001, AUTH-S07-A-RUN-001, materialização de dataset, avaliação, testes, carga, segurança dinâmica, browser, providers, fontes reais, rede ou qualquer ação externa.
  ```

- Decisão: confirmar a proposta documental somente como baseline de
  planejamento; nenhum identificador de autoridade proposto foi concedido.
- Escopo do registro: atualizar o status da proposta, o índice público, o
  estado factual e este histórico append-only; validar o incremento e criar
  um commit local focal.
- Escopo negativo: sem materialização de dataset, avaliação, testes, carga,
  segurança dinâmica, browser, providers, fontes reais, rede, OCI, GitHub,
  publicação, deploy, `STATE-08`, Automatic Quality Gate, Human Gate ou ação
  externa; sem código, dependências, lockfiles, contratos, OpenAPI, schema,
  migrations ou ADRs.
- Runtime preflight: `NÃO APLICÁVEL`; o incremento registra somente uma
  decisão documental e não altera nem valida comportamento executável.
- Conteúdo confirmado: identidade e freeze do dataset, proveniência/direitos,
  os 15 thresholds aceitos de ADR-0004, matriz `pt-BR`/`en-GB`, ambientes,
  verificações, evidências, autoridades futuras propostas, escopo negativo e
  condições de parada.
- Limitações: não existe evidência de corpus real, dataset materializado ou
  congelado, campanha pontuada, provider, fonte real, ambiente homologado ou
  ação externa. Fixtures sintéticas não substituem evidência de produto.
- Resultado: `S07-A` possui baseline de planejamento confirmada, mas permanece
  não autorizado para execução e não executado. Automatic Quality Gate e
  Human Gate de `STATE-07` permanecem `PENDENTES` e não executados.
- Próxima condição: autoridade humana explícita e separada para
  `AUTH-S07-A-DATASET-001` ou outro envelope delimitado; a confirmação desta
  proposta não satisfaz essa condição.
- Aprovador da baseline de planejamento: proprietário do RAG-Challenge.

## 2026-08-07 — Próxima ação concreta tornada obrigatória no handoff

- Baseline anterior: branch `main`, commit
  `66c47d94d423abf4f0c1509ba04b8064d3efd8ca`, corpus `4.9.3` e working tree
  limpa.
- Autoridade: solicitação explícita do proprietário para corrigir e documentar
  permanentemente que todo handoff deve informar qual é o próximo passo,
  tarefa, atividade ou ação.
- Problema corrigido: a ausência canônica de `4.9.2` foi aplicada de forma
  excessiva após uma solicitação concluída, embora ainda existisse uma ação
  diretamente relacionada dependente de documentos e autoridade.
- Decisão: todo handoff informa exatamente uma próxima ação concreta,
  priorizada, diretamente relacionada, com responsável e condição/autoridade.
  Falta de dado, documento, anexo, decisão ou autoridade torna sua obtenção a
  próxima ação; não autoriza execução por implicação.
- Exceção restrita: `nenhum — a solicitação atual não exige trabalho
  adicional` só é válido depois de consultar o estado e os documentos
  proprietários e confirmar que não existe continuação acionável diretamente
  relacionada. A regra não permite importar lifecycle, gate, backlog ou
  melhoria sem relação.
- Corpus resultante: `4.9.4` (`PATCH`), com 13 arquivos ativos em `prompts/`.
  AGENTS aplica o enforcement transversal; Governance possui a semântica;
  Templates materializa o formato; o changelog registra a correção.
- Escopo negativo: sem materialização de dataset, avaliação, testes, carga,
  segurança dinâmica, browser, providers, fontes reais, rede, OCI, GitHub,
  publicação, deploy, `STATE-08`, Automatic Quality Gate, Human Gate ou ação
  externa; sem código, dependências, lockfiles, contratos, OpenAPI, schema,
  migrations ou ADRs.
- Runtime preflight: `NÃO APLICÁVEL`; a mudança é exclusivamente documental e
  não altera nem valida comportamento executável.
- Estado resultante: `STATE-07 TESTING_HOMOLOGATION` permanece ativo; `S07-A`
  mantém somente a baseline de planejamento confirmada, sem autoridade de
  execução.
- Próxima ação concreta: o proprietário fornece ou indica os documentos
  PDF/CSV candidatos e autoriza somente a verificação local de elegibilidade,
  proveniência e direitos antes de qualquer `AUTH-S07-A-DATASET-001`.
- Aprovador da correção permanente: proprietário do RAG-Challenge.

## 2026-08-07 — ADR-0008 aceita como autoridade arquitetural

- Baseline: branch `main`, commit
  `5c151c64ae4d3049d68fee6788502d439aa25251`, corpus `4.9.4` e working tree
  limpa, confirmados antes do registro.
- Decisão explícita do proprietário:

  ```text
  Confirmo a decisão proposta em ADR-0008 — Product Corpus Storage and Page-Image Evidence e aceito a ADR exclusivamente como autoridade arquitetural.

  Esta aceitação não autoriza reconciliação normativa, implementação, movimentação do PDF, geração de PNGs, dataset, indexação, ativação, testes, providers, rede, publicação ou ação externa. Cada incremento exigirá autorização separada.
  ```

- Decisão: aceitar
  [`ADR-0008`](../../docs/architecture/ADR-0008-Product-Corpus-Storage-And-Page-Image-Evidence.md)
  exclusivamente como autoridade arquitetural para armazenamento permanente
  do corpus e evidências visuais de páginas.
- Limite de autoridade: a aceitação não reconcilia ADR-0002, ADR-0004,
  `Security-And-Access.md`, `RAG-Module.md`, contratos canônicos, data
  dictionary, threat model, OpenAPI ou demais documentos normativos; não
  autoriza implementação ou execução de qualquer incremento subsequente.
- Escopo do registro: atualizar somente a ADR-0008, o índice arquitetural, o
  estado factual e este histórico append-only; validar o incremento e criar um
  commit local focal.
- Escopo negativo: sem alteração do corpus normativo ou de sua versão; sem
  código, contratos, OpenAPI, schema, migrations, testes, dependências,
  lockfiles, `.gitignore`, PDF ou registro de elegibilidade; sem movimentação
  de documento, geração de PNG, dataset, indexação, ativação, providers, rede,
  browser, publicação, deploy ou ação externa.
- Runtime preflight: `NÃO APLICÁVEL`; o incremento registra somente uma
  decisão documental e não altera nem valida comportamento executável.
- Estado resultante: ADR-0008 `accepted`; `STATE-07 TESTING_HOMOLOGATION`
  permanece ativo somente dentro da autoridade vigente, sem lote adicional,
  dataset, indexação, ativação ou gate autorizado por esta decisão.
- Próxima condição: autoridade humana explícita e separada para reconciliar
  semanticamente a ADR-0008 antes de qualquer implementação ou execução.
- Aprovador da decisão: proprietário do RAG-Challenge.

## 2026-08-07 — ADR-0009 proposta para taxonomia de idiomas documentais

- Baseline: branch `main`, commit
  `8b4e98dc336b13183b936c5ac974968714e43765`, corpus `4.9.4` e working tree
  limpa, confirmados antes da preparação.
- Autoridade: solicitação explícita do proprietário para preparar somente a
  proposta documental de ADR-0009, sem aceitar a decisão, reconciliar ADR-0008
  ou alterar contratos, OpenAPI, modelo, dataset ou runtime.
- Bloqueio observado: o PDF PostgreSQL 18.4 declara a tag BCP 47 `en`, enquanto
  os contratos e o data dictionary vigentes restringem `contentLanguage` a
  `pt-BR` e `en-GB` e proíbem valor neutro, inferido ou de fallback.
- Alternativas comparadas: manter documentos restritos a `pt-BR`/`en-GB`;
  separar idiomas de pergunta/resposta de tags documentais BCP 47 mais amplas;
  ou mapear tags menos específicas para uma variante suportada.
- Decisão proposta: selecionar a taxonomia separada, preservar o `en`
  declarado sem mapeamento para `en-GB`, manter `questionLanguage` e
  `answerLanguage` fechados em `pt-BR`/`en-GB`, conservar a matriz obrigatória
  de quatro pares e criar estratos adicionais por tag documental exata.
- Compatibilidade proposta: OpenAPI v1 permanece inalterado; qualquer tag
  documental mais ampla e a evidência visual de ADR-0008 podem integrar
  somente um futuro contrato v2 separadamente reconciliado, implementado e
  testado.
- Escopo do registro: criar ADR-0009 com status `proposed`, atualizar o índice
  arquitetural, registrar a proposta e o bloqueio no estado factual e
  acrescentar esta entrada exclusivamente no EOF.
- Escopo negativo: sem aceitação de ADR, reconciliação semântica, alteração do
  corpus normativo ou de sua versão, contratos canônicos, OpenAPI, data
  dictionary, código, testes, schema, migrations, dependências, lockfiles,
  PDF ou registro de elegibilidade; sem dataset, indexação, ativação,
  providers, browser, rede, renderização, PNGs ou ação externa.
- Runtime preflight: `NÃO APLICÁVEL`; a mudança é exclusivamente documental e
  não altera nem valida comportamento executável.
- Estado resultante: ADR-0009 `proposed`; ADR-0008 permanece `accepted`, mas
  sua reconciliação semântica continua bloqueada até decisão humana explícita
  sobre a taxonomia. `STATE-07 TESTING_HOMOLOGATION` permanece ativo sem novo
  lote ou execução autorizada.
- Próxima condição: o proprietário aceita, rejeita ou solicita revisão da
  ADR-0009 por decisão explícita e independente.
- Aprovador da preparação: proprietário do RAG-Challenge.

## 2026-08-07 — ADR-0009 aceita como autoridade arquitetural

- Baseline: branch `main`, commit
  `89994e82d246b1cc0a240e99a2d09942e316f7cc`, corpus `4.9.4` e working tree
  limpa, confirmados antes do registro.
- Autoridade da decisão: o proprietário declarou exatamente
  `Confirmo a decisão proposta em ADR-0009 — Document, Evidence and Query Language Taxonomy e aceito a ADR exclusivamente como autoridade arquitetural.`
- Decisão: aceitar
  [ADR-0009](../../docs/architecture/ADR-0009-Document-Evidence-And-Query-Language-Taxonomy.md)
  exclusivamente como autoridade arquitetural para a taxonomia separada de
  idiomas documentais, evidência, pergunta e resposta.
- Semântica aceita: `SupportedQueryLanguage` permanece restrito a `pt-BR` e
  `en-GB`; `DocumentContentLanguage` é um domínio BCP 47 distinto; o idioma
  `en` declarado pelo PDF PostgreSQL não é inferido como `en-GB`; citações
  preservam o idioma original; e OpenAPI v1 permanece inalterada.
- Limite de autoridade: a aceitação não reconcilia ADR-0008, contratos
  canônicos, OpenAPI, data dictionary, `RAG-Module.md` ou outros documentos
  normativos; não autoriza implementação, API v2, dataset, indexação,
  ativação, provider, rede ou ação externa.
- Escopo do registro: atualizar somente ADR-0009, o índice arquitetural, o
  estado factual e este histórico append-only; validar o incremento e criar um
  commit local focal.
- Escopo negativo: sem alteração do corpus normativo ou de sua versão; sem
  código, contratos, OpenAPI, data dictionary, schema, migrations, testes,
  dependências, lockfiles, PDF, registro de elegibilidade ou dados; sem API
  v2, dataset, indexação, ativação, providers, browser, rede ou ação externa.
- Runtime preflight: `NÃO APLICÁVEL`; o incremento registra somente uma
  decisão documental e não altera nem valida comportamento executável.
- Estado resultante: ADR-0009 `accepted`; o bloqueio decisório da taxonomia
  está resolvido, mas a reconciliação semântica permanece não executada e
  exige autoridade posterior e separada. `STATE-07 TESTING_HOMOLOGATION`
  permanece ativo sem novo lote, execução ou gate autorizado.
- Próxima condição: autoridade humana explícita e separada para reconciliar
  semanticamente ADR-0008 e ADR-0009 com os documentos normativos aplicáveis.
- Aprovador da decisão: proprietário do RAG-Challenge.

## 2026-08-07 — Reconciliação semântica conjunta de ADR-0008 e ADR-0009

- Baseline: branch `main`, commit
  `3d15ad4f2726f715c8dcf880491927ad0ff37b2f`, corpus `4.9.4` e working tree
  limpa, confirmados antes de qualquer edição.
- Autoridade: solicitação explícita do proprietário para reconciliar somente a
  semântica já aceita dos ADRs 0008/0009 nos documentos canônicos nomeados,
  preservar OpenAPI v1 byte a byte, validar o pacote e criar um único commit
  local focal somente se todos os checks fossem aprovados.
- Classificação: corpus `4.9.5` (`PATCH`), pois a mudança torna correntes
  decisões arquiteturais já aceitas sem nova autoridade, capacidade
  executável, estado ou contrato implementado; permanecem 13 arquivos ativos
  em `prompts/`.
- Escopo canônico: ADR-0002, ADR-0004, ADR-0008, ADR-0009, índice
  arquitetural, contratos canônicos, data dictionary, Solution Architecture,
  RAG Module, Security and Access, Lifecycle, Quality Gates, threat model,
  planejamento S07-A, índice documental, Current State, este histórico
  append-only e Prompt System Change Log: exatamente 18 arquivos.
- Semântica aplicada: `IDocumentContentStore` é a autoridade permanente para
  fonte/PNG content-addressed; PDF visual exige `pdf-page-png-v1`, manifesto
  completo, lifecycle/reachability e direitos específicos; query/answer
  permanecem `pt-BR|en-GB`, conteúdo documental usa BCP 47 separado,
  `sourceDeclaredLanguage=en` não é inferido como `en-GB`, citações preservam
  a fonte e avaliação adiciona estratos exatos.
- Compatibilidade: OpenAPI v1 permaneceu byte a byte idêntica, com SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  Git `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; v2 permanece somente planejado
  e não implementado, sem artefato OpenAPI v2.
- Escopo negativo observado: sem alteração de código, testes, schema,
  migrations, dados, dataset, registro de elegibilidade, dependências,
  lockfiles, PDF ou OpenAPI; sem geração de PNG, indexação, ativação,
  provider, browser, rede, OCI, GitHub, publicação, deploy ou ação externa.
- Runtime preflight: `NÃO APLICÁVEL`; a mudança e as verificações foram
  exclusivamente documentais, sem inspeção ou encerramento de processo.
- Validação: baseline/escopo, rastreabilidade ADR-owner, preservação append-only,
  links, UTF-8 sem BOM, LF, newline final, whitespace, NUL, coerência factual,
  hash/blob do OpenAPI v1, `eng/check-repository.ps1` e `git diff --check`
  foram aprovados; o diff integral foi revisado antes do commit.
- Estado resultante: ADR-0008 e ADR-0009 permanecem `accepted`; reconciliação
  semântica concluída no corpus `4.9.5`; implementação, direitos específicos
  pendentes, import, renderização, v2, dataset, avaliação e qualquer execução
  continuam não autorizados. `STATE-07 TESTING_HOMOLOGATION` permanece ativo
  sem Automatic Quality Gate ou Human Gate executado.
- Próxima condição: autoridade humana explícita e separada para um incremento
  implementável que nomeie tipos, schema/migration, content store, renderer,
  lifecycle, serving, v2 e testes aplicáveis, ou um subconjunto coerente deles.
- Aprovador da reconciliação: proprietário do RAG-Challenge.

## 2026-08-07 — S03-CORR-01 modelo ADR-0008/0009

- Baseline: branch `main`, commit
  `ffc7bef913dda2699b072ef172188291f6ac0500`, corpus `4.9.5` e working tree
  limpa, confirmados antes da primeira alteração.
- Autoridade: `AUTH-S03-CORR-001`, exclusivamente para o primeiro incremento
  corretivo local, offline e sequencial do modelo reconciliado pelos ADRs 0008
  e 0009, com owner técnico de `STATE-03` e sem autoridade de gate ou avanço.
- Runtime preflight: inspeção dirigida encontrou zero processo e zero listener
  comprovadamente pertencente ao RAG-Challenge; nada foi encerrado.
- Implementação: commit
  `5fdbbc36d8eee29fdeec4b179564bd1eff322558`; separa idiomas de query e
  documento, preserva a declaração observada, valida BCP 47 localmente, mantém
  runtime v1 fechado a `pt-BR|en-GB`, modela página/manifesto e digest,
  propaga os tipos por ingestão, indexação, consulta, provider, vetor e Server,
  e amplia a reachability fail-closed de fonte/imagem.
- Modelo físico: migration Control única
  `20260807161323_AddDocumentLanguageAndRenderManifestModel`, com coluna
  declarada nullable sem backfill e tabelas vazias de manifesto/página ligadas
  à versão, fonte e imagem exatas. Valores legados `pt-BR`/`en-GB` foram
  preservados, e o modelo/migrations Vector não mudou.
- Verificações focais: 19 testes unitários e 6 casos de integração passaram,
  inclusive `en` distinto de `en-GB`, digest/bindings, upgrade legado,
  rollback/reapply, leitura vetorial, elegibilidade v1 e limpeza.
- Verificação integral: `eng/ci.ps1 -Offline` passou 106 testes unitários, 116
  de integração, 10 de arquitetura e 38 do Dashboard; cobertura de 93,74% de
  linhas e 67,11% de branches; auditoria de 212 arquivos aprovada. Control e
  Vector não têm mudança pendente; `foreign_key_check` não encontrou violação.
- Compatibilidade: OpenAPI v1 permaneceu byte a byte no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  Git `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; dependências, lockfiles,
  Dashboard, ADRs e `reference-materials/` permaneceram sem diff.
- Escopo negativo observado: sem renderer, PNG, import, mudança do candidato
  PostgreSQL, dataset, ativação, serving, v2, `AnswerEvidenceRecord`, nova
  dependência, rede, ação externa, Automatic Quality Gate, Human Gate ou
  mudança de lifecycle.
- Corpus: `4.9.6` (`PATCH`), exclusivamente para reconciliar os seis registros
  factuais autorizados com a implementação e os testes observados.
- Estado resultante: `STATE-07 TESTING_HOMOLOGATION` permanece ativo;
  `S03-CORR-01` está implementado e validado somente na fronteira local,
  offline, sintética e estática descrita. Nenhum incremento posterior foi
  iniciado.

## 2026-08-07 — Próxima ação ordenada não pode ser substituída por revisão genérica

- Baseline: branch `main`, commit
  `28bc0e347cb65c3daef1d3dacdcac3632c6f048e`, corpus `4.9.6` e working tree
  limpa.
- Autoridade: solicitação explícita do proprietário para corrigir e documentar
  permanentemente a obrigação de informar o próximo passo, tarefa, atividade
  ou ação real.
- Recorrência observada: o handoff de `S03-CORR-01` indicou `revisar os dois
  commits locais`, embora o item 1 estivesse concluído e Lifecycle registrasse
  uma ordem de dependência com item 2 ainda não executado.
- Regra corrigida: o primeiro item não concluído de uma ordem governada tem
  prioridade; quando sua execução não estiver autorizada, obter a autoridade
  delimitada é a próxima ação. Revisão genérica só é válida quando constitui
  gate, pré-requisito ou entregável formal.
- Apresentação: perguntas diretas sobre o próximo passo recebem a ação concreta
  antes de qualquer recapitulação.
- Próxima ação factual: o proprietário autoriza um incremento delimitado, com
  owner técnico de `STATE-04`, para preparar o envelope executável do segundo
  refinamento — content store de fonte/PNG, renderer determinístico, direitos,
  ativação atômica e v2, preservando v1 — ou a execução permanece parada.
- Corpus: `4.9.7` (`PATCH`), com alteração somente de AGENTS, Governance,
  Templates, Current State, este histórico append-only e Prompt System Change
  Log.
- Escopo negativo: sem implementar o segundo refinamento, aceitar ADR, alterar
  código, dependência, lockfile, contrato, OpenAPI, schema, migration ou dados;
  sem Automatic Quality Gate, Human Gate, lifecycle, rede ou ação externa.
- Runtime preflight: `NÃO APLICÁVEL`; a correção é exclusivamente documental.
- Estado resultante: `STATE-07 TESTING_HOMOLOGATION` permanece ativo;
  `S03-CORR-01` continua concluído e nenhum incremento posterior foi iniciado.

## 2026-08-07 — S04-CORR-04-A verified content-object descriptors

- Baseline: branch `main`, commit
  `ea7fc582f991bb9290e26a7e2d4e074abc46bf3c`, corpus `4.9.7` e working tree
  limpa, confirmados antes da primeira alteração. OpenAPI v1 correspondia ao
  SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
- Autoridade: `AUTH-S04-CORR-04-A-001`, exclusivamente para a implementação
  local, offline e sequencial de verified content-object descriptors, com
  owner técnico corretivo de `STATE-04` e sem gate ou avanço de lifecycle.
- Runtime preflight: a inspeção dirigida encontrou zero processo de produto e
  zero listener comprovadamente pertencente ao RAG-Challenge; nada foi
  encerrado antes da implementação.
- Implementação: commit
  `26f2e154b736687693b31ab02ca59cfb8ba86655`; introduz o port executável
  `IDocumentContentStore`, descritores tipados com identidade/hash/tamanho/media
  type/implementação/verificações, escrita bounded e idempotente com publicação
  atômica e reabertura, e leitura com recomputação integral, comprimento exato
  e stream reposicionado.
- Integração: ingestão local e oficial falsa, `IntegrationRuntime` e validação
  do control plane usam o novo contrato. `IStorageMaintenance`, o plano
  `cleanup-plan-v1` e o protocolo de reserva/finalização permanecem inalterados
  como única autoridade existente de exclusão física.
- Verificações focais: 3 testes unitários e 57 casos de integração passaram,
  cobrindo media type, descritor, limites, hash, comprimento, identidade
  esperada, deduplicação, tamper, reabertura, ingestão e cleanup.
- Verificação integral: `pwsh -NoProfile -File eng/ci.ps1 -Offline` passou 109
  testes unitários, 118 de integração, 10 de arquitetura e 38 do Dashboard;
  cobertura de 93,76% de linhas e 67,15% de branches; build Release sem aviso;
  auditoria de 213 arquivos aprovada. Uma invocação preliminar expirada pelo
  executor foi rejeitada como evidência; somente seu shell e filho
  `dotnet format`, comprovadamente da tarefa, foram encerrados antes da
  repetição integral aceita.
- Compatibilidade: nenhum package, lockfile, schema, migration, endpoint ou
  contrato público v1 mudou. OpenAPI v1 permaneceu byte a byte no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
- Escopo negativo observado: sem renderer, PNG, registro de direitos, render
  manifest persistido, digest de ativação, v2, fonte/dado real, rede, provider,
  ação externa, Automatic Quality Gate, Human Gate ou mudança de lifecycle.
- Corpus: `4.9.8` (`PATCH`), exclusivamente para reconciliar os registros
  factuais com a implementação e a evidência observada.
- Estado resultante: `STATE-07 TESTING_HOMOLOGATION` permanece ativo;
  `S04-CORR-04-A` está concluído somente na fronteira local, offline, sintética
  e estática descrita. Nenhum incremento posterior de `S04-CORR-04` foi
  iniciado.
- Próxima condição: autoridade humana explícita e separada para o primeiro
  incremento ainda não executado do envelope `S04-CORR-04`, preservando o
  escopo negativo e a ordem de dependência vigente.

## 2026-08-07 — S04-CORR-04-B document rights eligibility contracts

- Baseline: branch `main`, commit
  `196bbcafcb493ce4e45a2c9e784965ff933f124d`, corpus `4.9.8` e working tree
  limpa, confirmados antes da primeira alteração. OpenAPI v1 correspondia ao
  SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
- Autoridade: `AUTH-S04-CORR-04-B-001`, exclusivamente para a implementação
  local, offline e sequencial dos contratos/gates de elegibilidade de direitos,
  com owner técnico corretivo de `STATE-04` e sem gate ou avanço de lifecycle.
- Runtime preflight: a inspeção dirigida encontrou zero processo de produto e
  zero listener comprovadamente pertencente ao RAG-Challenge; nada foi
  encerrado antes da implementação.
- Implementação: commit
  `a886a944ecd1ce485eee9c072385e96210e90520`; introduz
  `DocumentRightsEligibilityRecordV1` vinculado a uma versão documental exata,
  exige uma decisão para cada um dos dez domínios de ADR-0008, fecha os estados
  em `Permitted`, `Denied` e `Unproven` e exige uma referência estável de
  evidência por decisão.
- Gates: `TextualEvidence` e `PdfVisualEvidence` aceitam somente direitos
  requeridos explicitamente `Permitted` e retornam todas as decisões
  bloqueantes. Posse/download, parsing/transformação, indexação, retenção,
  citação, rendering, criação/retenção de derivado, display,
  distribuição/publicação e requisitos de atribuição/notice permanecem
  independentes; distribuição não é inferida de elegibilidade textual/visual.
- Verificações focais: 14 casos unitários sintéticos passaram, cobrindo registro
  completo, ausência/duplicidade, estados fechados, referência de evidência,
  os dez direitos individuais, gates textual/visual e bloqueio por `Denied` ou
  `Unproven`.
- Verificação integral: `pwsh -NoProfile -File eng/ci.ps1 -Offline` passou 123
  testes unitários, 118 de integração, 10 de arquitetura e 38 do Dashboard;
  cobertura de 93,72% de linhas e 67,20% de branches; build Release sem aviso;
  auditoria de 216 arquivos aprovada.
- Compatibilidade: nenhum package, lockfile, schema, migration, renderer, PNG,
  persistência de direitos/render manifest, ativação, endpoint ou contrato
  público mudou. OpenAPI v1 permaneceu byte a byte no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
- Escopo negativo observado: nenhuma fonte, documento, licença, direito ou dado
  real foi cadastrado ou alterado; sem import, indexação, ativação, serving,
  v2, rede, provider, ação externa, Automatic Quality Gate, Human Gate ou
  mudança de lifecycle.
- Corpus: `4.9.9` (`PATCH`), exclusivamente para reconciliar os registros
  factuais com a implementação e a evidência observada.
- Estado resultante: `STATE-07 TESTING_HOMOLOGATION` permanece ativo;
  `S04-CORR-04-B` está concluído somente na fronteira local, offline, sintética
  e estática descrita. `S04-CORR-04-C` e incrementos posteriores não foram
  iniciados.
- Próxima condição: autoridade humana explícita e separada para
  `S04-CORR-04-C`, primeiro incremento ainda não executado do envelope
  `S04-CORR-04-PREP`, preservando v1, o escopo negativo e as condições de
  parada vigentes.

## 2026-08-07 — S04-CORR-04-C deterministic PDF rendering and verified render-candidate finalisation

- Baseline: branch `main`, commit
  `75475c391c7fc1fb5ff298492a5d1da4c4f99fbb`, corpus `4.9.9` e working tree
  limpa, confirmados antes da primeira alteração. OpenAPI v1 correspondia ao
  SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
- Autoridade: `AUTH-S04-CORR-04-C-001`, exclusivamente para implementação
  local e sequencial do renderer selecionado, isolamento, validação de PNG e
  finalização verificada do render candidate, com owner técnico corretivo de
  `STATE-04` e sem gate ou avanço de lifecycle.
- Runtime preflight: a inspeção dirigida encontrou zero processo de produto e
  zero listener comprovadamente pertencente ao RAG-Challenge; nada foi
  encerrado antes da implementação.
- Supply chain: caches, CLI home e artefactos isolados foram usados para as oito
  identidades e versões exatas. Assinaturas, hashes raw/cache, `contentHash`,
  licenças, repositórios/commits upstream, grafo, build targets e assets nativos
  foram conferidos antes da edição. Não houve advisory ou depreciação material;
  os assets Linux arm64 de PDFium e SkiaSharp são ELF64 AArch64. A distinção
  oficial entre licença Apache-2.0 dos nupkgs PDFium e licença MIT do repositório
  de empacotamento foi registrada sem divergência da seleção autorizada.
- Implementação: commit
  `981e61c3308ee3407769d10ab1fa554007f12799`; adiciona o adapter
  `pdfium-pdftoimage-v1`, opções fixas do perfil `pdf-page-png-v1`, worker
  interno de um documento no executável existente, framing privado limitado,
  contenção antes do envio dos bytes, ambiente sanitizado, validação estrutural
  de PNG, publicação/reabertura verificadas e commit/readback idempotente e
  atômico do manifest nas tabelas existentes.
- Verificações focais: 7 casos unitários e 10 de integração passaram com bytes
  exclusivamente sintéticos, cobrindo gates de direitos, identidade da fonte,
  limites, determinismo, rotação, dimensões, fundo branco, annotations/form
  fill desativados, PNG, falhas do worker, reabertura de objetos e atomicidade
  do manifest. A suíte de arquitetura passou 10/10.
- Verificação de publicação: restore locked/offline e publish
  framework-dependent `linux-arm64` passaram; o diretório publicado contém
  `libpdfium.so` e `libSkiaSharp.so` ELF64 AArch64 (`e_machine=183`) e nenhum
  asset nativo de RID estrangeiro.
- Verificação integral: `pwsh -NoProfile -File eng/ci.ps1 -Offline` passou 130
  testes unitários, 128 de integração, 10 de arquitetura e 38 do Dashboard;
  cobertura de 93,53% de linhas e 66,80% de branches; build Release sem aviso;
  auditoria de 223 arquivos aprovada.
- Compatibilidade: somente os quatro lockfiles previstos mudaram. Não houve
  projeto novo, schema, migration, model snapshot, ativação, endpoint, v2 ou
  alteração pública v1. OpenAPI v1 permaneceu byte a byte no SHA-256 exigido.
- Escopo negativo observado: nenhuma fonte, PDF, PNG, licença, direito ou dado
  real foi usado; sem importação, indexação, ativação, serving, cleanup,
  provider, conta, secret, OCI, GitHub autenticado, publicação, deploy, push,
  Automatic Quality Gate, Human Gate ou mudança de lifecycle. A evidência
  temporária de supply chain permanece fora do Git e não foi apagada.
- Corpus: `4.9.10` (`PATCH`), exclusivamente para reconciliar os registros
  factuais com a implementação e a evidência observada.
- Estado resultante: `STATE-07 TESTING_HOMOLOGATION` permanece ativo;
  `S04-CORR-04-C` está concluído somente na fronteira local, sintética e
  estática descrita. `S04-CORR-04-D` e incrementos posteriores não foram
  iniciados.
- Próxima condição: autoridade humana explícita e separada para
  `S04-CORR-04-D`, primeiro incremento ainda não executado do envelope
  `S04-CORR-04-PREP`, preservando v1, o escopo negativo e as condições de
  parada vigentes.

## 2026-08-07 — S04-CORR-04-D immutable rights and activation-evidence binding

- Baseline: branch `main`, commit
  `548a817e2db4d9bad2d1a63e7dc9e9bb9ace418c`, corpus `4.9.10` e working tree
  limpa, confirmados antes da primeira alteração. OpenAPI v1 correspondia ao
  SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e
  blob Git `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`.
- Autoridade: `AUTH-S04-CORR-04-D-001`, exclusivamente para implementação
  local, offline e sequencial da persistência de direitos e binding atômico de
  fonte, geração textual e render manifest, com owner técnico corretivo de
  `STATE-04` e sem gate ou avanço de lifecycle.
- Runtime preflight: a inspeção dirigida encontrou zero processo de produto e
  zero listener comprovadamente pertencente ao RAG-Challenge; nada foi
  encerrado antes da implementação.
- Implementação: commit
  `d18224e46f559229a58e82b097abbf16ea9f359a`; cada nova revisão vincula o
  `DocumentBinding`, objeto fonte, snapshot completo schema-v1 das dez decisões
  de direitos e render manifest exigido para PDF/ausente para CSV. Initial,
  Replacement e Rollback exigem todos os vínculos; Rollback revalida o estado
  atual e ObservationRebind só preserva evidência quando exclusivamente a
  observação muda.
- Validação/atomicidade: pre-CAS confere identidade, idioma documental,
  direitos, fonte reaberta, geração finalizada e, para PDF, manifest/páginas e
  todos os PNGs reabertos. Replay compara os novos campos. A transação Control
  persiste revisão/bindings, evidência/direitos, retenção, head, auditoria e
  journal completion aplicável ou conserva integralmente a autoridade anterior.
- Migration: a única migration Control
  `20260808004846_AddDocumentRightsAndActivationEvidenceBindings` cria somente
  as duas tabelas novas e suas chaves/constraints. Não executa operação de
  dados, não preenche histórico e não altera a base Vector. Revisão histórica
  sem vínculos completos é preservada sem inferência e falha fechada como
  autoridade corrente de consulta/prontidão visual.
- Verificações focais: seleções unitárias e 15 casos de integração passaram,
  cobrindo direitos/idioma/fonte, manifest/página/objeto, Initial, Replacement,
  Rollback, ObservationRebind, replay, CAS, falhas injetadas, one-shot, restart
  e upgrade histórico. Rollback/reapply, `foreign_key_check` e pending model
  checks Control/Vector passaram em bancos descartáveis.
- Verificação integral: `pwsh -NoProfile -File eng/ci.ps1 -Offline` passou 135
  testes unitários, 137 de integração, 10 de arquitetura e 38 do Dashboard;
  cobertura de 94,34% de linhas e 67,25% de branches; build Release e auditoria
  de 226 arquivos aprovados.
- Compatibilidade: nenhum package, lockfile, Dashboard, ADR, Governance,
  Lifecycle, Quality Gate, metadado vetorial ou digest canônico mudou. OpenAPI
  v1 permaneceu byte a byte no SHA-256 e blob Git exigidos.
- Escopo negativo observado: nenhum dado, direito, fonte, PDF ou PNG real foi
  usado; sem v2, serving, `AnswerEvidenceRecord`, provider, rede, ação externa,
  Automatic Quality Gate, Human Gate ou mudança de lifecycle.
- Corpus: `4.9.11` (`PATCH`), exclusivamente para reconciliar os registros
  factuais com a implementação e a evidência observada.
- Estado resultante: `STATE-07 TESTING_HOMOLOGATION` permanece ativo;
  `S04-CORR-04-D` está concluído somente na fronteira local, offline, sintética
  e estática descrita. `S04-CORR-04-E` não foi iniciado.
- Próxima condição: autoridade humana explícita, separada e com envelope
  completo para `S04-CORR-04-E`, preservando v1, o escopo negativo e as
  condições de parada vigentes até nova disposição expressa.

## 2026-08-07 — ADR-0010 persistent answer-evidence architecture registered

- Baseline: branch `main`, commit
  `745304051c113c86f5ebbaaaf625fbde74c50c6a`, corpus `4.9.11` e working tree
  limpa, confirmados antes da primeira alteração. OpenAPI v1 correspondia ao
  SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
- Decisão: o proprietário confirmou integralmente a proposta arquitetural e
  aceitou, exclusivamente como autoridade arquitetural, que `S04-CORR-04-E`
  corresponda ao contrato persistente `AnswerEvidenceRecordV1`, à sua retenção
  limitada e à sua participação em reachability.
- Autoridade deste registro: exclusivamente documentação local, offline e
  sequencial da decisão já aceita, incluindo ADR-0010 e reconciliação somente
  dos proprietários normativos/factuais previamente identificados.
- Contrato: registro interno schema-v1 criado somente para `Answered`, depois da
  validação completa e antes da resposta; identidade/digest canônicos, vínculos
  exatos de citação/fonte/manifest/página, `P30D` sem refresh, uma transação
  Control, replay idempotente, conflito divergente e falha pública pela taxonomia
  v1 existente.
- Privacidade e cleanup: nenhum texto de pergunta/resposta/citação, payload de
  provider, score/vetor, identidade de usuário, secret, path ou bytes é
  persistido. Durante retenção, os objetos vinculados permanecem alcançáveis;
  expiração não exclui e `cleanup-plan-v1` continua a exigir
  reserva/revalidação/finalização.
- Reconciliação factual: os fatos correntes reconhecem que os incrementos
  `S04-CORR-04-A` a `S04-CORR-04-D` implementaram content store, direitos,
  renderer/PNG/manifests e vínculos de ativação. ADR-0008, ADR-0009, relatórios
  de execução e todas as entradas históricas foram preservados integralmente.
- Compatibilidade: OpenAPI v1 permanece byte a byte no SHA-256 protegido; não
  foi criado v2, serving, endpoint, resultado ou código `CH_*` público.
- Corpus: `4.10.0` (`MINOR`), por introduzir nova autoridade normativa sem
  alterar a precedência, o fluxo ou o lifecycle.
- Escopo negativo observado: nenhuma implementação, migration, teste de
  produto, dado real, gate, lifecycle, processo, rede, ação externa, push, PR,
  merge, release ou deploy foi executado.
- Estado resultante: `STATE-07 TESTING_HOMOLOGATION` permanece ativo;
  `S04-CORR-04-E` está definido arquiteturalmente e não iniciado.
- Próxima condição: autoridade humana explícita, separada e com envelope
  completo para a implementação local/offline de `S04-CORR-04-E`, preservando
  OpenAPI v1 e todas as condições de parada registradas.

## 2026-08-08 — S04-CORR-04-E persistent answer-evidence implementation

- Baseline: branch `main`, commit
  `fc83e1ea6922a519baf527efc3f0a219e2674453`, corpus `4.10.0` e working tree
  limpa, confirmados antes da primeira alteração. OpenAPI v1 correspondia ao
  SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
- Autoridade: exclusivamente implementação local, offline, sequencial e
  sintética de `S04-CORR-04-E` conforme ADR-0010, sem Automatic Quality Gate,
  Human Gate, lifecycle ou ação externa.
- Runtime preflight: a inspeção dirigida encontrou zero processo ou listener
  comprovadamente pertencente ao RAG-Challenge; nada foi encerrado.
- Contrato/composição: modelo e serialização canônica schema-v1, identidade
  `ans-evidence-<uuid-n>`, digest e retenção fixa `P30D`; somente `Answered`
  validado integralmente cria o registro, que é persistido e reaberto antes da
  resposta pública v1 inalterada. Outros outcomes e falhas não criam registro.
- Persistência: uma transação Control grava operação, header, citações, páginas
  e auditoria sanitizada. Mesmo ID/conteúdo canônico retorna `AlreadyApplied`;
  conteúdo divergente sob o mesmo ID retorna conflito sem mutação. Falha de
  commit/readback impede `Answered` pela taxonomia v1 existente.
- Migration: `20260808033247_AddAnswerEvidenceRecords` cria somente as tabelas
  vazias `answer_evidence_records`, `answer_evidence_citations` e
  `answer_evidence_pages`, sem backfill, inferência histórica ou alteração da
  base Vector.
- Retenção/cleanup: registro não expirado é raiz independente para fontes e
  PNGs vinculados. Expiração não refresca nem autoriza exclusão; o plano
  `cleanup-plan-v1`, a reserva, a revalidação integral e a finalização continuam
  exclusivos, inclusive sob race entre plano antigo e novo registro.
- Privacidade: registro, auditoria e logging obedecem à allowlist de ADR-0010;
  não persistem pergunta/hash da pergunta, resposta/texto/URL de citação,
  prompt/payload de provider, score/vetor, identidade/IP do usuário, secret,
  path ou bytes.
- Verificação direta: build Release passou sem aviso; 146 testes unitários, 153
  de integração e 10 de arquitetura passaram com fixtures sintéticas e stores
  descartáveis. Os testes cobrem serialização, restart, concorrência, replay,
  conflito, falhas injetadas, migration/rollback/reapply, privacidade, retenção,
  reachability e cleanup race. Control e Vector não possuem mudança de modelo
  pendente. Isso não constitui Automatic Quality Gate.
- Compatibilidade: OpenAPI v1 permaneceu byte a byte no SHA-256 protegido; não
  houve endpoint, payload, outcome ou código `CH_*` público novo.
- Corpus: `4.10.1` (`PATCH`), exclusivamente para reconciliar os fatos correntes
  com a implementação e a evidência observada.
- Escopo negativo observado: nenhum dado, fonte, PDF ou PNG real foi usado; sem
  v2, serving, rede, provider pago, ação externa, push, PR, merge, release,
  deploy, Automatic Quality Gate, Human Gate ou mudança de lifecycle.
- Estado resultante: `STATE-07 TESTING_HOMOLOGATION` permanece ativo;
  `S04-CORR-04-E` está concluído somente na fronteira local, offline, sintética
  e estática descrita.
- Próxima condição: autoridade humana explícita e separada para o Automatic
  Quality Gate de `S04-CORR-04-E` sobre o commit local focado, sem inferir v2,
  serving, Human Gate ou transição de lifecycle.

## 2026-08-08 — Automatic Quality Gate corretivo de S04-CORR-04-E reprovado

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; não houve Human Gate nem mudança de lifecycle.
- Autoridade e baseline: o proprietário autorizou exclusivamente o Automatic
  Quality Gate corretivo de `S04-CORR-04-E`, local, offline e sequencial, sobre
  `main@990d14172954567456d9ad90b6a767f6b6e0da78`, corpus `4.10.1`, working
  tree limpa e OpenAPI v1 no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
- Baseline confirmada: branch `main`; HEAD exato; corpus `4.10.1`; working tree
  completamente limpa; OpenAPI v1 byte a byte no hash protegido.
- Achado `AQG-S04-002` (P2):
  [`STATE-02-Canonical-Contracts.md`](../../docs/architecture/STATE-02-Canonical-Contracts.md)
  afirma nas linhas 12–13 que a persistência de evidência permanece uma
  capacidade sucessora não implementada, enquanto o mesmo contrato nas linhas
  537 e 597–600 declara que `S04-CORR-04-E` implementa localmente esse contrato.
  O modelo de dados, o módulo RAG, o Current State e o relatório de STATE-04
  também registram a implementação local.
- Impacto: uma autoridade arquitetural canônica vigente fornece estados
  factuais mutuamente exclusivos para o próprio objeto do gate, criando risco
  relevante de manutenção e auditoria. O achado permanece `ABERTO`.
- Parada obrigatória: a auditoria parou após confirmar o primeiro achado.
  Runtime preflight não foi alcançado porque nenhum check executável foi
  iniciado; nenhum processo ou listener foi inspecionado ou parado.
  `eng/ci.ps1 -Offline`, build, testes, coverage, migration, restart,
  concorrência, injeção de falhas, retenção, cleanup, privacidade e reachability
  permanecem não executados neste gate.
- Evidência reproduzível: `Select-String` e leituras delimitadas confirmaram as
  afirmações contraditórias no arquivo rastreado; `git branch --show-current`,
  `git rev-parse HEAD`, `git status --porcelain=v1 --untracked-files=all` e
  `Get-FileHash -Algorithm SHA256 docs/api/openapi-v1.json` confirmaram a
  baseline e a proteção da OpenAPI v1.
- Resultado: Automatic Quality Gate `REPROVADO`, com um novo P2 e nenhum P0,
  P1 ou P3 identificado antes da parada.
- Escopo negativo preservado: nenhuma correção de source, teste, contrato,
  OpenAPI, schema, migration ou ADR; sem v2, serving, dados reais, rede, ação
  externa, push, PR, merge, release, deploy, Human Gate ou mudança de
  lifecycle.
- Relatório atualizado:
  [`STATE-04-Backend-Implementation-Report.md`](../../docs/STATE-04-Backend-Implementation-Report.md).
- Próxima condição: autoridade corretiva separada para resolver
  `AQG-S04-002`, seguida de nova autoridade sobre baseline limpa para reiniciar
  integralmente o Automatic Quality Gate corretivo de `S04-CORR-04-E`.

## 2026-08-08 — Correção documental focal de AQG-S04-002 concluída

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; o Automatic Quality Gate corretivo de `S04-CORR-04-E` continua
  historicamente `REPROVADO`, sem Human Gate ou mudança de lifecycle.
- Autoridade e baseline: o proprietário autorizou exclusivamente a correção
  documental mínima de `AQG-S04-002` sobre branch `main`, commit
  `3f42214c5c3554b6b341ab4c75a0a8e3cdb93f2a`, corpus `4.10.1`, working tree
  completamente limpa e OpenAPI v1 no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
- Correção: o parágrafo de finalidade de
  [`STATE-02-Canonical-Contracts.md`](../../docs/architecture/STATE-02-Canonical-Contracts.md)
  agora registra que a evidência persistente de resposta está implementada
  localmente pelo incremento separadamente autorizado `S04-CORR-04-E`; image
  serving e v2 permanecem capacidades sucessoras não implementadas.
- Semântica preservada: a aceitação da ADR-0010 continua sem constituir
  implementação, gate ou homologação; a implementação posterior e já existente
  de `S04-CORR-04-E` não foi alterada.
- Verificação focal: leituras estáticas delimitadas e busca dirigida pela
  alegação contraditória confirmaram a convergência do parágrafo corrigido com a
  seção implementada do mesmo contrato; a auditoria do repositório aprovou 235
  arquivos não ignorados. OpenAPI v1 permaneceu byte a byte no SHA-256
  protegido.
- Disposição: `AQG-S04-002` está `CORRECTED_PENDING_GATE_RETEST`. A correção não
  resolve o achado, não aprova o gate e não substitui seu reinício integral sob
  autoridade separada.
- Escopo negativo preservado: nenhuma alteração de source, teste,
  comportamento, schema, migration, ADR-0010, OpenAPI v1, v2 ou serving; sem
  Automatic Quality Gate, Human Gate, lifecycle, rede, ação externa, push, PR,
  merge, release ou deploy.
- Relatório atualizado:
  [`STATE-04-Backend-Implementation-Report.md`](../../docs/STATE-04-Backend-Implementation-Report.md).
- Próxima condição: nova autoridade explícita e separada sobre baseline limpa
  para reiniciar integralmente o Automatic Quality Gate corretivo de
  `S04-CORR-04-E`.

- Aprovador: proprietário do RAG-Challenge.

## 2026-08-08 — Reinício integral do AQG de S04-CORR-04-E reprovado

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; não houve Human Gate nem mudança de lifecycle.
- Autoridade e baseline: o proprietário autorizou exclusivamente o reinício
  integral do Automatic Quality Gate corretivo de `S04-CORR-04-E`, local,
  offline e sequencial, sobre branch `main`, commit
  `da569d8dae6990db72e43df69f1ff0bb8861ac54`, corpus `4.10.1`, working tree
  completamente limpa e OpenAPI v1 no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
- Baseline confirmada: branch, HEAD, corpus, ausência de entradas no porcelain
  e hash protegido corresponderam integralmente ao envelope esperado.
- Runtime preflight: depois de excluir o shell da própria inspeção e sua cadeia
  de execução, a verificação dirigida por path, command line e assembly
  encontrou zero processo e zero listener comprovadamente pertencentes ao
  RAG-Challenge; nada foi encerrado.
- Disposição anterior: a inspeção estática confirmou a correção focal e dispôs
  `AQG-S04-002` como `RESOLVIDO`.
- Achado `AQG-S04-003` (P2): a seção `Required verification` de
  [`STATE-02-Canonical-Contracts.md`](../../docs/architecture/STATE-02-Canonical-Contracts.md)
  ainda descreve nas linhas 788–791 os testes de answer-evidence como futuros,
  enquanto o baseline contém as suítes unitárias e de integração e os registros
  factuais declaram sua implementação e execução direta.
- Impacto: uma autoridade arquitetural canônica vigente fornece status de teste
  stale para o próprio objeto do gate, criando risco relevante de manutenção e
  auditoria. O achado permanece `ABERTO`.
- Parada obrigatória: a auditoria parou após confirmar o primeiro novo achado.
  `eng/ci.ps1 -Offline`, build, testes, coverage, migration, restart,
  concorrência, injeção de falhas, retenção, cleanup, privacidade e reachability
  não foram executados neste reinício. Evidência direta anterior não foi
  reclassificada como evidência deste gate.
- Evidência reproduzível: busca dirigida, leituras delimitadas do contrato
  canônico, arquivos de teste e registros factuais confirmaram a divergência;
  `git diff --check fc83e1ea6922a519baf527efc3f0a219e2674453..HEAD` passou antes
  do achado. Branch, HEAD, porcelain e SHA-256 da OpenAPI v1 confirmaram a
  baseline antes da auditoria.
- Resultado: Automatic Quality Gate `REPROVADO`, com `AQG-S04-002` resolvido,
  um novo P2 e nenhum P0, P1 ou P3 identificado antes da parada.
- Escopo negativo preservado: nenhuma correção de source, teste, comportamento,
  schema, migration, ADR-0010, OpenAPI v1, v2 ou serving; sem rede, ação externa,
  push, PR, merge, release, deploy, Human Gate ou mudança de lifecycle.
- Relatório atualizado:
  [`STATE-04-Backend-Implementation-Report.md`](../../docs/STATE-04-Backend-Implementation-Report.md).
- Próxima condição: autoridade corretiva separada para resolver
  `AQG-S04-003`, seguida de nova autoridade sobre baseline limpa para reiniciar
  integralmente o Automatic Quality Gate corretivo de `S04-CORR-04-E`.

## 2026-08-08 — Correção documental focal de AQG-S04-003 concluída

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; o Automatic Quality Gate corretivo de `S04-CORR-04-E` continua
  historicamente `REPROVADO`, sem Human Gate ou mudança de lifecycle.
- Autoridade e baseline: o proprietário autorizou exclusivamente a correção
  documental mínima de `AQG-S04-003` sobre branch `main`, commit
  `cb67c7f752521f416f46d9cb4f2bb6a189ca1a48`, corpus `4.10.1`, working tree
  completamente limpa e OpenAPI v1 no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
- Correção: a seção `Required verification` de
  [`STATE-02-Canonical-Contracts.md`](../../docs/architecture/STATE-02-Canonical-Contracts.md)
  agora classifica os testes de answer-evidence como requisitos, em vez de
  trabalho futuro. Identidade/digest canônicos, criação somente para
  `Answered`, bindings completos, atomicidade, replay/conflito/falhas, retenção
  fixa, privacidade e cleanup races permanecem no mesmo escopo.
- Semântica preservada: a seção continua definindo verificações exigidas e não
  se torna evidência de implementação ou execução; esses fatos permanecem nos
  registros proprietários. ADR-0010, implementação e testes não mudaram.
- Verificação focal: leituras estáticas delimitadas e uma busca dirigida pela
  classificação stale confirmaram a convergência semântica; a auditoria do
  repositório aprovou 235 arquivos não ignorados. OpenAPI v1 permaneceu byte a
  byte no SHA-256 protegido.
- Disposição: `AQG-S04-003` está `CORRECTED_PENDING_GATE_RETEST`. A correção não
  resolve o achado, não aprova o gate e não substitui seu reinício integral sob
  autoridade separada.
- Escopo negativo preservado: nenhuma alteração de source, teste,
  comportamento, schema, migration, ADR-0010, OpenAPI v1, v2 ou serving; sem
  Automatic Quality Gate, Human Gate, lifecycle, rede, ação externa, push, PR,
  merge, release ou deploy.
- Relatório atualizado:
  [`STATE-04-Backend-Implementation-Report.md`](../../docs/STATE-04-Backend-Implementation-Report.md).
- Próxima condição: nova autoridade explícita e separada sobre baseline limpa
  para reiniciar integralmente o Automatic Quality Gate corretivo de
  `S04-CORR-04-E`.
- Aprovador: proprietário do RAG-Challenge.

## 2026-08-08 — Reinício integral do AQG após AQG-S04-003 reprovado

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; não houve Human Gate nem mudança de lifecycle.
- Autoridade e baseline: o proprietário autorizou exclusivamente o reinício
  integral do Automatic Quality Gate corretivo de `S04-CORR-04-E`, local,
  offline e sequencial, sobre branch `main`, commit
  `baa85553f9d48c7c833b1e875699817849360bab`, corpus `4.10.1`, working tree
  completamente limpa e OpenAPI v1 no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
- Baseline confirmada: branch, HEAD, corpus, ausência de entradas no porcelain
  e hash protegido corresponderam integralmente ao envelope esperado.
- Disposição anterior: a inspeção estática confirmou a correção focal e dispôs
  `AQG-S04-003` como `RESOLVIDO`.
- Achado `AQG-S04-004` (P2): ADR-0010 exige testes diretos de rejeição para
  mismatches de citação, fonte, ativação, manifest e página. A suíte focal
  [`SqliteAnswerEvidenceStoreTests.cs`](../../tests/RagChallenge.IntegrationTests/SqliteAnswerEvidenceStoreTests.cs)
  testa somente um mismatch do digest no header de ativação; o teste unitário
  de páginas verifica ausência e excesso contra o próprio registro, sem
  confrontar citation/source/manifest/page divergentes com a autoridade Control
  persistida.
- Impacto: os ramos fail-closed existem na implementação, mas a ausência da
  matriz de regressão requerida cria risco relevante de regressão de integridade
  e auditoria. Nenhum defeito runtime foi estabelecido. O achado permanece
  `ABERTO`.
- Parada obrigatória: a auditoria parou após confirmar o primeiro novo achado.
  Runtime preflight não foi alcançado porque nenhum check executável foi
  iniciado; nenhum processo ou listener foi inspecionado ou parado.
  `eng/ci.ps1 -Offline`, build, testes, coverage, migration, restart,
  concorrência, injeção de falhas, retenção, cleanup, privacidade e reachability
  não foram executados neste reinício. Evidência direta anterior não foi
  reclassificada como evidência deste gate.
- Evidência reproduzível: leituras delimitadas da ADR-0010, do store e das
  suítes focalizadas, além de busca dirigida pelos casos de mismatch,
  confirmaram a lacuna. Branch, HEAD, porcelain e SHA-256 da OpenAPI v1
  confirmaram a baseline antes da auditoria.
- Resultado: Automatic Quality Gate `REPROVADO`, com `AQG-S04-003` resolvido,
  um novo P2 e nenhum P0, P1 ou P3 identificado antes da parada.
- Escopo negativo preservado: nenhuma correção de source, teste, comportamento,
  schema, migration, ADR-0010, OpenAPI v1, v2 ou serving; sem rede, ação externa,
  push, PR, merge, release, deploy, Human Gate ou mudança de lifecycle.
- Relatório atualizado:
  [`STATE-04-Backend-Implementation-Report.md`](../../docs/STATE-04-Backend-Implementation-Report.md).
- Próxima condição: autoridade corretiva separada para resolver
  `AQG-S04-004`, seguida de nova autoridade sobre baseline limpa para reiniciar
  integralmente o Automatic Quality Gate corretivo de `S04-CORR-04-E`.

## 2026-08-08 — Correção focal de AQG-S04-004 concluída

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; não houve Human Gate nem mudança de lifecycle.
- Autoridade e baseline: o proprietário autorizou exclusivamente a matriz
  mínima de regressão exigida pela ADR-0010 para corrigir `AQG-S04-004`, sobre
  branch `main`, commit `fd2e164ef1d8b1a90d867f4e77beea0e43fba9c3`, corpus
  `4.10.1`, working tree completamente limpa e OpenAPI v1 no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
- Baseline e preflight: os cinco elementos da baseline corresponderam ao
  envelope esperado; o runtime preflight dirigido encontrou zero processo e
  zero listener TCP comprovadamente pertencentes ao RAG-Challenge e não parou
  nada.
- Correção: somente
  [`SqliteAnswerEvidenceStoreTests.cs`](../../tests/RagChallenge.IntegrationTests/SqliteAnswerEvidenceStoreTests.cs)
  foi alterado entre os artefatos executáveis. A matriz cria registros
  estruturalmente válidos com mismatch único de citação, fonte, ativação,
  manifest ou página contra a autoridade Control persistida.
- Falha fechada e atomicidade: cada um dos cinco casos recebeu
  `InvalidDataException` e confirmou contagem zero em headers, citações,
  páginas, operações administrativas de answer-evidence e auditorias
  `AnswerEvidenceCreated`.
- Verificação proporcional: o arquivo focal aprovou 14/14 casos em Release,
  com `--no-restore`; o projeto completo de integração afetado aprovou 157/157
  casos reutilizando o mesmo build, também sem restore. Nenhum defeito de
  implementação foi demonstrado e nenhuma mudança de produto foi necessária.
- Disposição: `AQG-S04-004` está `CORRECTED_PENDING_GATE_RETEST`. A correção
  não resolve o achado, não aprova o gate e não substitui seu reinício integral
  sob autoridade separada.
- Escopo negativo preservado: nenhuma alteração de source, comportamento,
  schema, migration, ADR-0010, OpenAPI v1, v2 ou serving; sem Automatic Quality
  Gate, Human Gate, lifecycle, rede, ação externa, push, PR, merge, release ou
  deploy.
- Relatório atualizado:
  [`STATE-04-Backend-Implementation-Report.md`](../../docs/STATE-04-Backend-Implementation-Report.md).
- Próxima condição: nova autoridade explícita e separada sobre baseline limpa
  para reiniciar integralmente o Automatic Quality Gate corretivo de
  `S04-CORR-04-E`.

## 2026-08-08 — Reinício integral do AQG após AQG-S04-004 aprovado

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; não houve Human Gate nem mudança de lifecycle.
- Autoridade e baseline: o proprietário autorizou exclusivamente o reinício
  integral do Automatic Quality Gate corretivo de `S04-CORR-04-E`, local,
  offline e sequencial, sobre branch `main`, commit
  `5a2dcbafdc0a3925338043b079f9eacc9e70ca1b`, corpus `4.10.1`, working tree
  completamente limpa e OpenAPI v1 no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
- Baseline confirmada: branch, HEAD, corpus, ausência de entradas no porcelain
  e hash protegido corresponderam integralmente ao envelope esperado antes dos
  checks e permaneceram intactos depois deles.
- Disposição anterior: a inspeção estática confirmou que os cinco casos
  confrontam isoladamente citação, fonte, ativação, manifest e página com a
  autoridade Control persistida, rejeitam com `InvalidDataException` e provam
  ausência de qualquer mutação. `AQG-S04-004` está `RESOLVIDO`.
- Runtime preflight: a inspeção dirigida excluiu a ancestralidade do shell,
  encontrou zero processo e zero listener TCP comprovadamente pertencentes ao
  RAG-Challenge e não encerrou nada.
- Gate técnico: `pwsh -NoProfile -File eng/ci.ps1 -Offline` terminou com exit
  code `0`. Restore locked, formato, build Release sem aviso/erro, 146 testes
  unitários, 157 de integração, 10 de arquitetura e 38 do Dashboard passaram
  sem falha ou skip. A cobertura .NET foi 94,91% de linhas
  (32.116/33.837) e 67,42% de branches (3.536/5.245); lint, typecheck, build web
  e auditoria dos 235 arquivos também passaram.
- Verificações específicas: a suíte aprovada abrange digest canônico,
  `Answered`-only, restart, concorrência, replay/conflito, cinco boundaries de
  falha, matriz completa de autoridade persistida, migration e pending model,
  retenção fixa, privacidade, cleanup races e reachability de fonte/PNGs.
- Resultado: Automatic Quality Gate `APROVADO`, com `AQG-S04-002` a
  `AQG-S04-004` `RESOLVIDOS` e nenhum novo P0, P1, P2 ou P3.
- Escopo negativo preservado: nenhuma alteração de source, teste, comportamento,
  schema, migration, ADR-0010, OpenAPI v1, v2 ou serving; sem rede, ação externa,
  push, PR, merge, release, deploy, Human Gate ou mudança de lifecycle.
- Relatório atualizado:
  [`STATE-04-Backend-Implementation-Report.md`](../../docs/STATE-04-Backend-Implementation-Report.md).
- Próxima condição: autoridade explícita e separada para preparar e apresentar
  o resumo completo do Human Gate corretivo de `S04-CORR-04-E`, sem inferir a
  decisão humana, mudança de lifecycle ou ação externa.

## 2026-08-08 — Encaminhamento não canônico de Human Gate corrigido

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Human Gate ou lifecycle foi executado ou alterado.
- Autoridade e baseline: o proprietário autorizou exclusivamente a correção
  documental focal sobre branch `main`, commit
  `b1ea89c73dedec9cfe01e3aaa32d8aec0bcc4646`, corpus `4.10.1`, working tree
  completamente limpa e OpenAPI v1 no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
- Conflito reconciliado: `AGENTS.md`, Governance, Quality Gates e Templates
  exigem que Human Gate pertença a um único `STATE-ID` e use a frase canônica
  vinculada a esse estado. `S04-CORR-04-E` é um incremento, não um estado.
- Correção factual: `Current-State.md` deixa de encaminhar um “Human Gate
  corretivo de S04-CORR-04-E”. O AQG corretivo permanece `APROVADO`, os achados
  `AQG-S04-002` a `AQG-S04-004` permanecem `RESOLVIDOS`, o Human Gate histórico
  de `STATE-04` permanece inalterado e `STATE-07` não recebe decisão por
  implicação.
- Preservação append-only: a condição registrada na entrada anterior permanece
  como fato histórico do encaminhamento emitido; esta nova entrada a corrige
  prospectivamente sem reescrever o histórico.
- Escopo negativo preservado: nenhuma alteração de source, teste, comportamento,
  schema, migration, ADR-0010, OpenAPI v1, v2, serving ou outra autoridade; sem
  teste executável, runtime preflight, Human Gate, lifecycle, rede, ação
  externa, push, PR, merge, release ou deploy.
- Próxima condição: nenhuma continuação diretamente relacionada a
  `S04-CORR-04-E`; qualquer objetivo posterior exige autoridade própria no
  estado ou lote que o possua.

## 2026-08-08 — S07-A A1-A5 reconciliado após verificação integral

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado por esta reconciliação.
- Autoridade e baseline: `AUTH-S07-A-A5-RECONCILE-001` autorizou somente o
  registro documental sobre branch `main`, commit
  `6cd939849909a8abf2c5dd0534244da5f19be833`, corpus `4.10.1`, working tree
  limpa e OpenAPI v1 preservada no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
- Sequência reconciliada: A1 em
  `968f69c2d9c37959d617742af5ac48aee5ca09d5`; preparação do harness em
  `ae8d96487fe719d89741aa33e5607e532301d60e`; correção freeze-safe em
  `18994db15d963b321ace93b0069436ffc4813b53`; A2 em
  `43ddc0de4a6c10b32a657f3c1e471a743cb42b5f`; A3 sob
  `AUTH-S07-A-RUN-001`; e A4 em
  `760bbcf4626b7890ffdfb0eeb0a8c5419b5feec7`.
- Evidência A3 preservada: os 11 casos sintéticos passaram; os oito arquivos
  task-owned continuam ignorados; o resultado histórico permanece imutável no
  SHA-256 `9efc2eef05388433af58e01242a1b1589556c43620eeec509f583fba0c2073bc`.
- Correções focais reconciliadas: validação consciente da fase no commit
  `275becfb04a4d0f7a1703c3be3f4c59d87550cc2` e serialização determinística
  UTF-8/LF de futuras evidências no commit
  `6cd939849909a8abf2c5dd0534244da5f19be833`, sem reescrever a evidência A3.
- A5 foi `APROVADO` sob `AUTH-S07-A-A5-RETEST-002`. Todos os digests, oito
  paths e sete agregados congelados foram recalculados e conferiram. O preflight
  aplicável não encontrou nem interrompeu processo ou listener pertencente ao
  RAG-Challenge.
- Comandos reais de A5: `pwsh -NoProfile -File eng/check-repository.ps1`,
  `pwsh -NoProfile -File tests/RagChallenge.IntegrationTests/S07ALocalHarness/Invoke-S07ALocalHarness.ps1 -Mode Validate`
  e `pwsh -NoProfile -File eng/ci.ps1 -Offline`; todos terminaram com exit code
  `0`. Passaram 146 testes unitários, 164 de integração, 10 de arquitetura e 38
  do Dashboard; a cobertura foi 94,91% de linhas e 67,42% de branches.
- Disposição dos achados: `S07-A-FIND-001` permanece `OPEN`;
  `S07-A-FIND-004` permanece `OPEN` histórico, embora sua causa esteja corrigida
  para futuras evidências; `S07-A-FIND-002`, `S07-A-FIND-003` e
  `S07-A-FIND-005` estão `RESOLVIDOS`. Nenhum novo achado surgiu no reteste.
- Escopo negativo preservado: sem repetição de A3, `-Mode Run`, alteração de
  evidência, dataset, manifests, thresholds, código, testes, dependências,
  lockfiles, contratos, OpenAPI, schema, migrations ou ADRs; sem provider ou
  fonte real, browser, rede, Automatic Quality Gate, Human Gate, lifecycle,
  ação externa, push, publicação ou deploy.
- Próxima condição: autoridade separada e delimitada para reiniciar o Automatic
  Quality Gate de `S07-A` sobre baseline limpa que inclua esta reconciliação;
  nenhum Human Gate ou avanço de lifecycle é inferido.

## 2026-08-09 — Automatic Quality Gate de S07-A aprovado

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; o Automatic Quality Gate de `S07-A` foi aprovado, sem Human Gate ou
  mudança de lifecycle.
- Autoridade e baseline: `AUTH-S07-A-AQG-RETEST-003` autorizou o reinício
  integral local, offline, determinístico e sequencial sobre branch `main`,
  commit `a6626a363713b4fbcf83387b7b2104eae1f3e918`, corpus `4.10.1`, working
  tree inicialmente limpa e OpenAPI v1 preservada no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
- Preflight: zero processo e zero listener comprovadamente pertencente ao
  RAG-Challenge; nada foi encerrado.
- Auditoria estática: A1-A5, seus commits, correções focais, estado factual,
  histórico append-only, manifests congelados e evidência ignorada conferiram.
  Não houve mudança pós-A2 no dataset, nem alteração de dependência, lockfile
  ou OpenAPI. Os três manifests permaneceram UTF-8/LF sem BOM, com hashes
  diretos e embutidos correspondentes.
- Evidência e agregados: os oito arquivos task-owned permaneceram ignorados e
  não rastreados, sem reparse point; o resultado A3 permaneceu imutável no
  SHA-256 `9efc2eef05388433af58e01242a1b1589556c43620eeec509f583fba0c2073bc`;
  os sete agregados sintéticos recalculados permaneceram em `1.000000`.
- Comandos reais: `pwsh -NoProfile -File eng/check-repository.ps1`,
  `pwsh -NoProfile -File tests/RagChallenge.IntegrationTests/S07ALocalHarness/Invoke-S07ALocalHarness.ps1 -Mode Validate`
  e `pwsh -NoProfile -File eng/ci.ps1 -Offline`; todos terminaram com exit code
  `0`. A auditoria cobriu 244 arquivos, `Validate` aprovou 6 de 6 testes e a CI
  aprovou build sem avisos ou erros, 146 testes unitários, 164 de integração,
  10 de arquitetura e 38 do Dashboard, com 94,91% de linhas e 67,42% de
  branches.
- Correção focal confirmada: o build imediatamente posterior a `Validate`
  passou sem bloqueio de assembly por `testhost`; `AQG-S07-004` está
  `RESOLVIDO` pelo commit `a6626a363713b4fbcf83387b7b2104eae1f3e918`.
- Disposição: Automatic Quality Gate `APROVADO`, sem novo achado;
  `AQG-S07-001` a `AQG-S07-004` estão `RESOLVIDOS`;
  `S07-A-FIND-001` e `S07-A-FIND-004` permanecem `OPEN`, enquanto
  `S07-A-FIND-002`, `S07-A-FIND-003` e `S07-A-FIND-005` permanecem
  `RESOLVIDOS`.
- Limitações preservadas: todos os thresholds de produto continuam `NOT_RUN`;
  não houve A3, `-Mode Run`, alteração de evidência, dataset, manifests,
  thresholds, código, testes, dependências, lockfiles, contratos, OpenAPI,
  schema, migrations ou ADRs; sem provider, fonte real, browser, rede,
  segurança dinâmica, carga, recuperação, acessibilidade, Linux, OCI, Human
  Gate, lifecycle, ação externa, push, publicação ou deploy.
- Autoridade deste registro: `AUTH-S07-A-AQG-RECONCILE-001`, limitada ao
  relatório de homologação, Current State e acréscimo append-only deste
  histórico.
- Próxima condição: qualquer continuação de `S07-A` ou `STATE-07` deve nomear
  e autorizar separadamente a fronteira ainda `NOT_RUN`; a aprovação deste
  gate sintético não prepara Human Gate nem promove lifecycle.

## 2026-08-09 — Automatic Quality Gate de v2 e serving visual aprovado

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; o contrato HTTP/OpenAPI v2 e o serving visual same-origin foram
  implementados e tiveram Automatic Quality Gate aprovado, sem Human Gate ou
  mudança de lifecycle.
- Autoridade e baseline: `AUTH-STATE07-V2-SERVING-AQG-RETEST-001` autorizou o
  reinício integral local, offline, determinístico e sequencial sobre branch
  `main`, commit `5505a85253aa4a8a7a3690caf3dd7a762175cab9`, corpus `4.10.1` e
  working tree inicialmente limpa.
- Sequência reconciliada: contrato congelado no commit
  `54bab1aa5f25b778093bea62ffecf7c479557f9a`, implementação no commit
  `c01abf525f4cc113baa389982da3b419d07556b6` e correção focal no commit
  `5505a85253aa4a8a7a3690caf3dd7a762175cab9`.
- OpenAPI protegido: v1 permaneceu byte a byte no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  Git `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; v2 permaneceu byte a byte no
  SHA-256 `01ab26ae8066971af2e5ae83ec828fae556951d5ce6c335b42f6e7cf7b062640` e
  blob Git `8d31b200375ea834f148ea625664091cd5cdc84f`.
- Preflight: zero processo e zero listener comprovadamente pertencente ao
  RAG-Challenge; nada foi encerrado e nenhum processo da tarefa permaneceu ao
  fim do gate.
- Auditoria estática: os 33 paths de implementação, os dois paths da correção,
  os contratos v1/v2, projeção BCP 47 sem coerção, cadeia de autoridade de
  serving, revalidação de ETag, limites, falhas uniformes, headers same-origin,
  CSP, apresentação acessível e fronteiras públicas conferiram. Não houve
  schema, migration, dependência ou lockfile novo.
- Correção focal confirmada: todos os quatro componentes do seletor, inclusive
  `pageNumber` malformado, são tratados na fronteira do endpoint com o
  `404`/`CH_VISUAL_EVIDENCE_NOT_AVAILABLE` uniforme sem alcançar o fallback do
  Dashboard. A restrição genérica exigida pelo servidor HTTP em memória também
  foi confirmada.
- Comandos focais reais: os testes unitários de
  `QuestionAnsweringServiceTests`/`DocumentLanguageAndRenderingTests`, os testes
  de integração de `ApiV2ContractTests`/`SqliteActivationLifecycleTests`, os
  três testes focais de arquitetura e os testes Dashboard
  `api-v2-contract.test.mjs`/`result-presentation.test.mjs` terminaram com exit
  code `0`, aprovando respectivamente 46, 20, 3 e 9 casos.
- Comandos integrais reais: `pwsh -NoProfile -File eng/check-repository.ps1` e
  `pwsh -NoProfile -File eng/ci.ps1 -Offline` terminaram com exit code `0`. A
  auditoria cobriu 255 arquivos; a CI aprovou build sem avisos ou erros, 147
  testes unitários, 171 de integração, 11 de arquitetura e 42 do Dashboard,
  com 94,80% de linhas e 67,14% de branches.
- Disposição: Automatic Quality Gate `APROVADO`, sem novo P0, P1, P2 ou P3;
  `AQG-S07-V2-001` e `AQG-S07-V2-002` estão `RESOLVIDOS`.
- Limitações e escopo negativo: browser e tecnologia assistiva, dados e
  renderer reais, provider, fonte, rede, carga, crash/recovery, Linux, OCI,
  produção, Human Gate, lifecycle, push, publicação e deploy permaneceram
  `NOT_RUN`; o gate não alterou contrato, OpenAPI, schema, migration,
  dependência, lockfile, ADR, dataset ou evidência retida.
- Autoridade deste registro: `AUTH-STATE07-V2-SERVING-AQG-RECONCILE-001`,
  limitada ao relatório de homologação, Current State e acréscimo append-only
  deste histórico.
- Próxima condição: qualquer fronteira ainda `NOT_RUN` ou novo incremento de
  `STATE-07` exige autoridade humana separada e delimitada; esta aprovação não
  prepara Human Gate nem promove lifecycle.

## 2026-08-09 — Integração e recuperação v2 reconciliadas após verificação focal

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Automatic Quality Gate deste incremento, Human Gate ou
  lifecycle foi executado ou alterado.
- Autoridade e baseline de implementação:
  `AUTH-STATE07-V2-INTEGRATION-RECOVERY-IMPL-001` autorizou somente a fronteira
  local, offline, determinística, sintética e sequencial sobre branch `main`,
  commit `a47bd40b1873920c7660abb14acd68de45a7dde4`, corpus `4.10.1`, working
  tree inicialmente limpa e OpenAPI v1/v2 protegidas. A implementação foi
  concluída no commit `e5dae7ee5a786417fba2c6ef0555686816b0b330`.
- Composição reconciliada: fora do perfil `Integration`, o reader visual
  permanece desabilitado e fail-closed; dentro dele, a mesma instância
  sintética atende query, readiness e leitura visual verificada sobre PDF/PNG
  project-owned em memória, manifest final, direitos sintéticos e conteúdo
  imutável. Nenhum corpus ou dado de produto, fonte, renderer ou provider real
  foi usado.
- Evidência focal: 52 de 52 testes passaram. O harness publicado em
  `http://127.0.0.1:5086` resultou `Passed`; serving PNG retornou `200`, a
  revalidação integral retornou `304`, e a geração e o seletor visual foram
  preservados após restart e cold restore.
- Recuperação e limites: o host estava encerrado antes das cópias confinadas;
  os fingerprints dos stores original, backup e restaurado conferiram. O teto
  visual de 64 MiB permaneceu aplicado; dez acessos imediatos foram aceitos e o
  décimo primeiro retornou `429`/`CH_VISUAL_EVIDENCE_RATE_LIMITED` com
  `Retry-After: 10`.
- Determinismo e cleanup: duas construções offline produziram o mesmo ZIP
  SHA-256 `e27c64571b63538e4cba21f552df500c24a4bab3a6365e6229e2d9dd033f2f7d`.
  Runtime, stores, backup, restore e temporários task-owned foram removidos;
  nenhum host ou listener permaneceu.
- Contratos protegidos: OpenAPI v1 permaneceu no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; OpenAPI v2 permaneceu no
  SHA-256 `01ab26ae8066971af2e5ae83ec828fae556951d5ce6c335b42f6e7cf7b062640` e
  blob `8d31b200375ea834f148ea625664091cd5cdc84f`. Não houve mudança de contrato,
  schema, migration, ADR, dependência ou lockfile.
- Limites preservados: browser e tecnologia assistiva, corpus, fonte,
  renderer, provider e dado reais, rede externa, benchmark, carga, p95/p99,
  crash injection abrangente, recuperação operacional, Linux, OCI, produção,
  ação externa, push, publicação e deploy permaneceram `NOT_RUN`. A evidência
  sintética não constitui homologação de produto.
- Autoridade deste registro:
  `AUTH-STATE07-V2-INTEGRATION-RECOVERY-RECONCILE-001`, limitada ao relatório
  de homologação, Current State, versão `4.10.2` do corpus e acréscimo
  append-only deste histórico.
- Próxima condição: o Automatic Quality Gate desta integração e recuperação v2
  exige autoridade separada sobre baseline documental limpa; nenhuma decisão
  humana ou promoção de lifecycle é inferida.

## 2026-08-09 — Ordem factual de Lifecycle reconciliada para v2

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Automatic Quality Gate deste incremento, Human Gate ou
  lifecycle foi executado ou alterado.
- Autoridade e baseline:
  `AUTH-STATE07-V2-INTEGRATION-RECOVERY-LIFECYCLE-CORR-001`, branch `main`,
  commit `de40a93e0023f854fec840a93934c199c294f9c6`, corpus `4.10.2` e working
  tree inicialmente limpa.
- OpenAPI protegido: v1 permaneceu byte a byte no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  Git `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; v2 permaneceu byte a byte no
  SHA-256 `01ab26ae8066971af2e5ae83ec828fae556951d5ce6c335b42f6e7cf7b062640` e
  blob Git `8d31b200375ea834f148ea625664091cd5cdc84f`.
- Correção factual: a ordem normativa foi preservada e somente suas anotações
  de estado foram atualizadas para registrar que `S04-CORR-04-E` possui
  Automatic Quality Gate corretivo aprovado; contrato/serving v2 estão
  implementados e possuem Automatic Quality Gate aprovado; integração,
  restart, cold backup/restore confinado e limites foram implementados e
  verificados focalmente no commit
  `e5dae7ee5a786417fba2c6ef0555686816b0b330`, mas seu Automatic Quality Gate
  permanece `NOT_RUN`; dataset e homologação continuam posteriores e não
  autorizados.
- Limites preservados: nenhuma ordem normativa, estado, critério, código,
  teste, harness, OpenAPI, contrato, schema, migration, ADR, dependência ou
  lockfile foi alterado. Runtime, testes, Automatic Quality Gate, Human Gate e
  lifecycle permaneceram `NOT_RUN`.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.2` para `4.10.3`,
  sem mudança de autoridade, precedência, estado ou critério.
- Verificação documental estática: `git diff --check` e
  `pwsh -NoProfile -File eng/check-repository.ps1` terminaram com exit code
  `0`; a auditoria cobriu 255 arquivos não ignorados. O histórico preservou
  integralmente o prefixo anterior, com somente adição EOF; UTF-8, LF final,
  escopo dos quatro paths e os hashes/blobs protegidos de OpenAPI v1/v2
  conferiram.
- Próxima condição: o Automatic Quality Gate da integração e recuperação v2
  exige autoridade humana separada sobre baseline documental limpa; dataset e
  homologação permanecem posteriores e não autorizados.

## 2026-08-09 — Automatic Quality Gate da integração e recuperação v2 aprovado

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; o Automatic Quality Gate da integração e recuperação v2 foi aprovado,
  sem Human Gate ou mudança de lifecycle.
- Autoridade e baseline:
  `AUTH-STATE07-V2-INTEGRATION-RECOVERY-AQG-RETEST-001` autorizou o reinício
  integral local, offline, determinístico, sintético e sequencial sobre branch
  `main`, commit `f6c648c40cf8d0280cfceca5509a381bddb9fc8f`, corpus `4.10.3` e
  working tree inicialmente limpa.
- OpenAPI protegido: v1 permaneceu byte a byte no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  Git `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; v2 permaneceu byte a byte no
  SHA-256 `01ab26ae8066971af2e5ae83ec828fae556951d5ce6c335b42f6e7cf7b062640` e
  blob Git `8d31b200375ea834f148ea625664091cd5cdc84f`.
- Auditoria e preflight: a auditoria estática de 255 arquivos foi aprovada; o
  preflight encontrou zero processo e zero listener comprovadamente pertencente
  ao RAG-Challenge, e nada foi encerrado.
- Verificação focal e determinismo: 53 de 53 testes focais passaram. Dois
  builds produziram o mesmo ZIP SHA-256
  `ab5e450efe1b606f2b8e50e2f5885a3c1ae19bf4ad90dd96d096e00506daec28`.
- Harness publicado: o resultado foi `Passed`, exclusivamente em loopback, com
  três readiness `Ready`, geração preservada após restart e cold restore,
  serving PNG e `304`, teto de 64 MiB e token bucket com dez acessos aceitos e
  o décimo primeiro rejeitado.
- CI offline: build sem avisos ou erros, 147 testes unitários, 174 de
  integração, 11 de arquitetura e 42 do Dashboard aprovados, com 94,81% de
  linhas e 67,24% de branches.
- Cleanup: concluído; nenhum runtime ou listener pertencente ao RAG-Challenge
  permaneceu.
- Disposição: Automatic Quality Gate `APROVADO`, sem novo achado;
  `AQG-S07-V2-IR-001` está `RESOLVIDO`.
- Limitações preservadas: corpus, fonte, renderer, provider e dado reais,
  rede externa, browser, tecnologia assistiva, benchmark, carga, p95/p99,
  crash injection abrangente, recuperação operacional, Linux, OCI e produção
  permaneceram `NOT_RUN`. A aprovação sintética não constitui homologação de
  produto, Human Gate ou promoção de lifecycle.
- Autoridade deste registro:
  `AUTH-STATE07-V2-INTEGRATION-RECOVERY-AQG-RECONCILE-001`, limitada ao
  relatório de homologação, Current State, versão `4.10.4` do corpus e
  acréscimo append-only deste histórico. Esta reconciliação não executou
  runtime, testes, Automatic Quality Gate, Human Gate ou lifecycle.
- Verificação documental estática: `git diff --check` e
  `pwsh -NoProfile -File eng/check-repository.ps1` terminaram com exit code
  `0`; a auditoria cobriu 255 arquivos não ignorados. O histórico preservou o
  prefixo anterior com somente adição EOF; UTF-8 sem BOM, LF, newline final,
  escopo dos quatro paths e OpenAPI v1/v2 protegidas conferiram.
- Próxima condição: dataset e homologação de produto permanecem posteriores,
  `NOT_RUN` e não autorizados; qualquer entrada exige autoridade humana
  separada e delimitada.

## 2026-08-09 — Lifecycle reconciliado após aprovação do AQG de integração v2

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum gate, Human Gate ou lifecycle foi executado ou alterado nesta
  correção documental.
- Autoridade e baseline:
  `AUTH-STATE07-V2-INTEGRATION-RECOVERY-LIFECYCLE-CORR-002`, branch `main`,
  commit `7ad6bae369eb1efbf6429902a2fd1f4441b60a32`, corpus `4.10.4` e working
  tree inicialmente limpa.
- OpenAPI protegido: v1 permaneceu byte a byte no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  Git `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; v2 permaneceu byte a byte no
  SHA-256 `01ab26ae8066971af2e5ae83ec828fae556951d5ce6c335b42f6e7cf7b062640` e
  blob Git `8d31b200375ea834f148ea625664091cd5cdc84f`.
- Correção factual: as duas claims correntes desatualizadas de Lifecycle agora
  registram que o Automatic Quality Gate da integração e recuperação v2 foi
  `APROVADO` sob
  `AUTH-STATE07-V2-INTEGRATION-RECOVERY-AQG-RETEST-001`, sem novo achado, e que
  `AQG-S07-V2-IR-001` está `RESOLVIDO`.
- Limites preservados: a ordem normativa, os estados e os critérios não
  mudaram; dataset e homologação de produto permanecem posteriores, `NOT_RUN`
  e não autorizados. Código, testes, harness, OpenAPI, contrato, schema,
  migration, ADR, dependência, lockfile e dataset não foram alterados.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.4` para `4.10.5`,
  sem mudança de autoridade, precedência, estado ou critério.
- Autoridade deste registro limitada a Lifecycle, Current State, changelog do
  corpus e acréscimo append-only deste histórico; nenhum runtime, teste,
  Automatic Quality Gate, Human Gate ou lifecycle foi executado.
- Verificação documental estática: `git diff --check` e
  `pwsh -NoProfile -File eng/check-repository.ps1` terminaram com exit code
  `0`; a auditoria cobriu 255 arquivos não ignorados. O histórico preservou o
  prefixo anterior com somente adição EOF; UTF-8 sem BOM, LF, newline final,
  escopo dos quatro paths e OpenAPI v1/v2 protegidas conferiram.

## 2026-08-09 — A0 do primeiro documento de produto bloqueado por direitos não dispostos

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado.
- Autoridade e baseline: `AUTH-S07-A-PRODUCT-A0-001`, branch `main`, commit
  `78d49e135d7b517c7ff89a9e5edcbcc7839e4043`, corpus `4.10.5`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas.
- Candidato verificado: `postgresql-18-reference-a4`, PostgreSQL `18.4`, no
  path ignorado
  `artifacts-local/state-07/source-intake/postgresql-18-reference-a4/postgresql-18-A4.pdf`.
  O arquivo regular permaneceu confinado, não rastreado e sem reparse point;
  seus `15.771.040` bytes, header `%PDF-1.4`, EOF e SHA-256
  `cea7b845568095eb56dee1b51bfa145c6c6637bc4377c986019971577efefae4`
  conferiram com o registro.
- Metadados reconciliados: proveniência, `contentLanguage=en`,
  `sourceDeclaredLanguage=en`, publisher e atribuição permaneceram
  consistentes com a evidência de intake existente.
- Direitos: parsing, indexing, source-byte retention, quotation e citation
  conservam a disposição explícita existente. Page rendering,
  derivative-image creation, derivative-image retention, runtime derivative
  display e a intended source or derivative distribution boundary permanecem
  `UNPROVEN`; nenhuma delas foi inferida da permissão geral de uso, cópia,
  modificação e distribuição.
- Disposição factual: `BLOCKED/EXCLUDED`, não
  `READY_FOR_PRODUCT_ACTIVATION`. `ELIGIBLE_CANDIDATE` permanece somente como
  elegibilidade anterior de consideração/textual; não existe autoridade de
  dataset, import, render, indexação, ativação, consulta ou produto.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.5` para `4.10.6`,
  sem mudança de autoridade, precedência, estado ou critério.
- Limites preservados: os três manifests congelados e a evidência sintética
  não mudaram. Nenhum parser, renderer, derivado, runtime, teste, harness,
  provider, fonte online, rede, browser, tecnologia assistiva, carga,
  benchmark, recuperação operacional, Linux, OCI, produção, OpenAPI, contrato,
  código, schema, migration, ADR, dependência ou lockfile foi alterado ou
  executado.
- Verificação documental estática: o verificador dirigido, `git diff --check`
  e `pwsh -NoProfile -File eng/check-repository.ps1` terminaram com exit code
  `0`; a auditoria cobriu 255 arquivos não ignorados. O histórico preservou
  byte a byte o prefixo anterior no SHA-256
  `c33a09af090e828ed7906efccbb907100ef47d2f7eec036c948abb6cbd171d2f`;
  UTF-8 sem BOM, LF, newline final, escopo dos quatro paths, manifests
  congelados, candidato e OpenAPI v1/v2 protegidas conferiram.
- Próxima condição: uma nova disposição A0 exige evidência explícita e
  separada por operação para os cinco direitos ainda `UNPROVEN`, sob autoridade
  humana própria; nenhuma continuação de produto é inferida.

## 2026-08-09 — Comunicação com o proprietário passa a exigir linguagem simples

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado.
- Autoridade: instrução explícita do proprietário nesta conversa para sempre
  explicar de forma simples para uma pessoa leiga e documentar essa regra.
- Regra registrada: a comunicação apresenta primeiro o resultado prático em
  linguagem concisa e compreensível por quem não possui conhecimento técnico
  especializado; termos técnicos necessários têm significado e consequência
  explicados em `pt-BR`, com exemplo quando útil.
- Limite preservado: simplificação não omite incerteza, risco, autoridade ou
  fato não verificado. A regra foi centralizada em `Language-Policy.md`, sem
  duplicação em AGENTS, Governance ou Templates.
- Versionamento: corpus elevado por `PATCH` de `4.10.6` para `4.10.7`, sem
  alterar lifecycle, gate, código, teste, OpenAPI, contrato, schema, migration,
  ADR, dependência, lockfile, dataset, runtime ou ação externa.
- Verificação documental estática: `git diff --check` e
  `pwsh -NoProfile -File eng/check-repository.ps1` terminaram com exit code
  `0`; a auditoria cobriu 255 arquivos não ignorados. O histórico preservou o
  prefixo anterior no SHA-256
  `62549f9060a3777525e13bfd08bd5eaad1a9f9f8657c1fb67d91401aece96700`, e
  OpenAPI v1/v2 permaneceram nos hashes e blobs protegidos.

## 2026-08-09 — ADR-0011 preparado como proposta de mapeamento de direitos

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado.
- Autoridade e baseline:
  `AUTH-S07-A-RIGHTS-POLICY-CORR-PREP-001`, branch `main`, commit
  `17c41a78cbe853473860403d476797064b77c78a`, corpus `4.10.7`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas.
- Proposta: ADR-0011 preparado com status `proposed`, preservando as dez
  decisões independentes e `Permitted`/`Denied`/`Unproven` fail-closed. O
  mapeamento proposto liga cada operação a evidência primária, escopo,
  condições e mecanismo de cumprimento sem exigir correspondência literal e
  sem ampliar a concessão observada.
- Fronteira: serving same-origin continua sendo entrega de bytes, mas pode ser
  avaliado como `RuntimeDerivativeImageDisplay` somente dentro da rota
  existente, do estado ativo e do contexto governado. Exportação,
  redistribuição, hosting público, bundle ou publicação permanecem sob
  `SourceAndDerivativeByteDistributionOrPublication`.
- Obrigações: a proposta determina associação rastreável de attribution,
  notices, disclaimers, trademark e change marking com o source record, cada
  derivative manifest e o contexto de display ou distribuição aplicável.
- Incompatibilidade estática: o contrato v2 exige reavaliar a intended
  distribution boundary antes de servir a imagem, enquanto
  `DocumentRightsEligibilityPolicy.PdfVisualEvidence` não avalia
  `SourceAndDerivativeByteDistributionOrPublication`. A proposta exige uma
  correção interna separadamente autorizada se vier a ser aceita; nenhum
  contrato público foi alterado.
- Não decisão: ADR-0011 não foi aceito e não reclassificou
  `postgresql-18-reference-a4`. A disposição permanece `BLOCKED/EXCLUDED`, e
  os cinco direitos visuais e de distribuição permanecem `UNPROVEN`.
- Versionamento: corpus elevado por `PATCH` documental de `4.10.7` para
  `4.10.8`, sem mudança de autoridade, estado, critério aceito, contrato ou
  comportamento.
- Limites preservados: nenhum código, teste, OpenAPI, contrato, schema,
  migration, ADR aceito, dependência, lockfile, dataset, parser, renderer,
  runtime, provider, fonte, rede, gate, Human Gate, lifecycle ou ação externa
  foi alterado ou executado.
- Verificação documental estática: `git diff --check` e
  `pwsh -NoProfile -File eng/check-repository.ps1` terminaram com exit code
  `0`; a auditoria cobriu 256 arquivos não ignorados. O histórico preservou
  byte a byte o prefixo anterior no SHA-256
  `153cfebc4b6b15106c6e8af087818e8f282487dfc6f1c79cbb20361d596f21e0`;
  UTF-8 sem BOM, LF, newline final e OpenAPI v1/v2 protegidas conferiram.
- Próxima condição: decisão humana explícita e separada sobre o ADR-0011. Uma
  eventual aceitação estabelecerá somente autoridade arquitetural e não
  autorizará reconciliação, implementação, novo A0 ou ativação.

## 2026-08-09 — ADR-0011 aceito como autoridade arquitetural

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado.
- Decisão e baseline: o proprietário declarou exatamente
  `ADR-0011: ACEITAR.` sobre branch `main`, commit
  `09f6760cb1a41d907da42b8c01cb34a7425030b9`, corpus `4.10.8`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas.
- Disposição arquitetural: ADR-0011 passou de `proposed` para `accepted`. A
  decisão estabelece o mapeamento explícito, auditável e condicionado entre
  evidência primária ampla e as dez decisões técnicas independentes, preserva
  `Permitted`/`Denied`/`Unproven` fail-closed e define a fronteira entre runtime
  same-origin display e distribuição/publicação externa de bytes.
- Obrigações preservadas: attribution, copyright/permission notices,
  disclaimers, trademark e change marking continuam vinculados à origem, ao
  derivative manifest e ao contexto de entrega aplicável, sem inferência de
  cumprimento.
- Incompatibilidade preservada: o contrato v2 exige reavaliar a intended
  distribution boundary, enquanto a política interna ainda não avalia
  `SourceAndDerivativeByteDistributionOrPublication`. A aceitação não corrige
  essa diferença e não modifica o contrato público.
- Não decisão: `postgresql-18-reference-a4` permanece `BLOCKED/EXCLUDED`; page
  rendering, derivative-image creation, derivative-image retention, runtime
  derivative display e a intended source or derivative distribution boundary
  permanecem `UNPROVEN`.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.8` para `4.10.9`,
  sem alteração de comportamento, gate, lifecycle ou autoridade executável.
- Limites preservados: nenhuma reconciliação semântica dos documentos
  normativos proprietários, código, teste, OpenAPI, contrato, schema,
  migration, dependência, lockfile, dataset, parser, renderer, runtime,
  provider, fonte, rede, gate, Human Gate, lifecycle ou ação externa foi
  autorizada ou executada.
- Verificação documental estática: `git diff --check` e
  `pwsh -NoProfile -File eng/check-repository.ps1` terminaram com exit code
  `0`; a auditoria cobriu 256 arquivos não ignorados. O histórico preservou
  byte a byte o prefixo anterior no SHA-256
  `ab134f14ff7b4126e34ca34b10a4bbc6cadb49ea5debfa5bebe0509a6bee2e52`;
  UTF-8 sem BOM, LF, newline final e OpenAPI v1/v2 protegidas conferiram.
- Próxima condição: autoridade humana separada para reconciliar semanticamente
  o ADR-0011 aceito nos documentos normativos proprietários. Essa futura
  reconciliação não autorizará por si só correção de código ou novo A0.

## 2026-08-09 — Semântica do ADR-0011 reconciliada documentalmente

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado.
- Autoridade e baseline:
  `AUTH-S07-A-RIGHTS-POLICY-CORR-RECONCILE-001`, branch `main`, commit
  `6fc81b973ca217693a286479df3ff6db0f4577e9`, corpus `4.10.9`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas.
- Mapeamento reconciliado: ADR-0004, ADR-0008, o registro de elegibilidade
  S07-A e o contrato documental v2 passam a exigir uma ligação explícita,
  auditável e condicionada entre cada operação e a evidência primária, sem
  correspondência literal obrigatória e sem propagação automática de uma
  concessão ampla.
- Fail-closed preservado: as dez decisões continuam independentes em
  `Permitted`/`Denied`/`Unproven`; ausência, conflito, ambiguidade jurídica,
  condição não determinada ou mecanismo não executável permanece `Unproven` e
  bloqueia o gate dependente.
- Fronteira reconciliada: a entrega do PNG ativo, citation-bound e revalidado
  pela rota relativa same-origin pertence a `RuntimeDerivativeImageDisplay`,
  mas continua sendo transmissão de bytes. Downloads, hosting público,
  cross-origin permissivo, CDN, exports, bundles, Git/Git LFS e republicação
  permanecem sob `SourceAndDerivativeByteDistributionOrPublication`.
- Obrigações reconciliadas: attribution, copyright/permission notices,
  disclaimers, trademark e change marking permanecem associados à evidência
  de direitos e à linhagem de cada derivado. Se os campos públicos já
  congelados ou o formato PNG não puderem cumprir a colocação exigida, o
  derivado permanece inelegível; nenhum campo ou endpoint público foi criado.
- Incompatibilidade executável preservada:
  `DocumentRightsEligibilityPolicy.PdfVisualEvidence` ainda não avalia
  `SourceAndDerivativeByteDistributionOrPublication`, embora o contrato v2
  exija reavaliar a intended distribution boundary antes de `200` ou `304`.
  A correção interna e seus testes permanecem posteriores e separadamente
  autorizados.
- Candidato inalterado: `postgresql-18-reference-a4` permanece
  `BLOCKED/EXCLUDED`; page rendering, derivative-image creation,
  derivative-image retention, runtime derivative display e a intended source
  or derivative distribution boundary permanecem `UNPROVEN`. Nenhum novo A0
  foi executado.
- Versionamento: corpus elevado por `PATCH` documental de `4.10.9` para
  `4.10.10`, sem mudança de contrato público, comportamento, gate, lifecycle
  ou autoridade executável.
- Limites preservados: exatamente os sete documentos autorizados foram
  alterados. Nenhum código, teste, OpenAPI, contrato público, schema,
  migration, dependência, lockfile, dataset, parser, renderer, runtime,
  provider, fonte, rede, gate, Human Gate, lifecycle ou ação externa foi
  alterado ou executado.
- Verificação documental estática: `git diff --check` e
  `pwsh -NoProfile -File eng/check-repository.ps1` terminaram com exit code
  `0`; a auditoria cobriu 256 arquivos não ignorados. O histórico preservou
  byte a byte o prefixo anterior no SHA-256
  `792980801536730e5c2cce1afd3b31c135fc9b7bbde70855f740acf3494a0c90`;
  UTF-8 sem BOM, LF, newline final e OpenAPI v1/v2 protegidas conferiram.
- Próxima condição: autoridade humana separada para a correção focal da
  política interna e de seus testes, preservando o contrato público e sem
  executar novo A0.

## 2026-08-09 — Correção interna da política de serving do ADR-0011 reconciliada

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado.
- Autoridade e baseline:
  `AUTH-S07-A-RIGHTS-POLICY-CORR-IMPL-RECONCILE-001`, branch `main`, commit
  `b9c3e5f3a72c2dd7762c256198452ae2c217b2d2`, corpus `4.10.10`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas.
- Implementação registrada: o commit focal
  `b9c3e5f3a72c2dd7762c256198452ae2c217b2d2` adiciona o gate interno
  `PdfVisualEvidenceServing` e o aplica no leitor verificado antes da abertura
  do PNG. As dez decisões permanecem independentes;
  `RuntimeDerivativeImageDisplay` deve estar `Permitted`;
  `SourceAndDerivativeByteDistributionOrPublication` `Unproven` bloqueia; e
  `Denied` é compatível somente com `RuntimeDerivativeImageDisplay`
  `Permitted` na fronteira same-origin aceita.
- Evidência focal registrada: 19 testes da política, 23 regressões dos gates
  existentes, três testes do leitor real e seis testes contratuais v1/v2 foram
  aprovados. Nenhum runtime ou listener permaneceu após a implementação.
- Contratos protegidos: OpenAPI v1 permaneceu no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; OpenAPI v2 permaneceu no
  SHA-256 `01ab26ae8066971af2e5ae83ec828fae556951d5ce6c335b42f6e7cf7b062640` e
  blob `8d31b200375ea834f148ea625664091cd5cdc84f`.
- Candidato inalterado: `postgresql-18-reference-a4` permanece
  `BLOCKED/EXCLUDED`; page rendering, derivative-image creation,
  derivative-image retention, runtime derivative display e a intended source
  or derivative distribution boundary permanecem `UNPROVEN`. Nenhum novo A0
  ou mapeamento candidato-específico foi executado.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.10` para
  `4.10.11`, sem mudança de contrato público, comportamento com dado de
  produto, gate ou lifecycle.
- Limites preservados: exatamente os cinco documentos autorizados foram
  alterados. Nenhum código, teste, OpenAPI, contrato público, schema,
  migration, ADR, dependência, lockfile ou dataset foi alterado. Nenhum parser,
  renderer, runtime, rede, fonte, provider, Automatic Quality Gate, Human Gate,
  lifecycle ou ação externa foi executado nesta reconciliação.
- Verificação documental estática: `git diff --check` e
  `pwsh -NoProfile -File eng/check-repository.ps1` terminaram com exit code
  `0`; a auditoria cobriu 256 arquivos não ignorados. O histórico preservou
  byte a byte o prefixo anterior no SHA-256
  `9fe053986ece430a4293fa363c793acdcd1d7bb178f9c0a3c990632e6484eeee`.
- Próxima condição: um novo A0 exige autoridade humana separada e mapeamento
  candidato-específico auditável; esta reconciliação não concede nenhum dos
  dois.

## 2026-08-09 — A0 candidato-específico do PostgreSQL repetido sob ADR-0011

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado.
- Autoridade e baseline: `AUTH-S07-A-PRODUCT-A0-002`, branch `main`, commit
  `f21cdea2052d28de1e2ffb86b1629c1c10bc6b6a`, corpus `4.10.11`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas.
- Evidência local observada: o único arquivo regular do candidato foi
  `postgresql-18-A4.pdf`, confinado ao diretório ignorado, sem reparse point,
  com 15.771.040 bytes e SHA-256
  `cea7b845568095eb56dee1b51bfa145c6c6637bc4377c986019971577efefae4`.
  Nenhuma fonte online foi consultada.
- Concessão primária aplicada: a PostgreSQL Licence oficial já registrada
  concede uso, cópia, modificação e distribuição do software e de sua
  documentação para qualquer finalidade, condicionados à presença do aviso
  de copyright, do parágrafo de permissão e dos dois parágrafos de disclaimer
  em todas as cópias.
- Mapeamento candidato-específico: page rendering, derivative-image creation,
  derivative-image retention e `RuntimeDerivativeImageDisplay` receberam
  `UNPROVEN`. A concessão ampla é pertinente a cada operação, mas não há
  mecanismo determinado no PNG nem nos campos públicos congelados que faça o
  conjunto completo de avisos e disclaimers acompanhar cada cópia derivada.
- Fronteira de distribuição:
  `SourceAndDerivativeByteDistributionOrPublication` recebeu `DENIED` fora da
  fronteira de display same-origin. Esta é uma negação deliberada da política
  interna do produto, não uma afirmação de proibição pelo titular, e não
  autoriza download, hosting público, CDN, exportação ou publicação.
- Condições preservadas: atribuição e os avisos/disclaimers estão
  identificados; nenhuma obrigação adicional de trademark foi inferida; e o
  change marking depende da linhagem auditável do derivado. Como o mecanismo
  completo de apresentação dos avisos permanece indeterminado, nenhum
  snapshot de direitos, obligation set, render manifest ou derivado foi
  criado.
- Disposição A0: `postgresql-18-reference-a4` permanece `BLOCKED/EXCLUDED` e
  não recebeu `READY_FOR_PRODUCT_ACTIVATION`. O manifest de avaliação
  congelado não foi alterado e continua sendo evidência histórica, não a
  autoridade atual do registro.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.11` para
  `4.10.12`, sem mudança de contrato público, comportamento de produto, gate
  ou lifecycle.
- Limites preservados: somente os quatro documentos autorizados foram
  alterados. Nenhum código, teste, OpenAPI, contrato, schema, migration, ADR,
  dependência, lockfile, dataset ou manifest foi alterado. Nenhum parser,
  renderer, derivado, indexação, ativação, query, runtime, provider, browser,
  rede, fonte, Automatic Quality Gate, Human Gate, lifecycle ou ação externa
  foi executado.
- Verificação documental estática: `git diff --check` e
  `pwsh -NoProfile -File eng/check-repository.ps1` terminaram com exit code
  `0`; a auditoria cobriu 256 arquivos não ignorados. O histórico preservou
  byte a byte o prefixo anterior no SHA-256
  `7c49b7a66666a4e8c1cdd4882431ae54591fc52901a9021753408671a1fd5e63`.
- Próxima condição: o candidato somente pode ser reavaliado após evidência
  autoritativa que determine a colocação aceitável dos avisos em cada cópia ou
  após decisão arquitetural separada que estabeleça um mecanismo executável e
  compatível, sem inferir a interpretação jurídica ausente.

## 2026-08-09 — ADR-0012 proposto para imagens derivadas autocontidas

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado.
- Autoridade e baseline:
  `AUTH-S07-A-NOTICE-BEARING-PROFILE-ADR-PREP-001`, branch `main`, commit
  `1b64ca88a0efebd7ab450f5bdc22004a72f3dc53`, corpus `4.10.12`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas.
- Proposta preparada: ADR-0012 registra o perfil determinístico
  `pdf-page-png-notice-v1`. Cada PNG proposto mantém o raster da página em uma
  região pixel a pixel intacta e acrescenta, fora dessa região, um painel com
  o conjunto completo de obrigações aplicáveis.
- Obrigações e acessibilidade: o `DerivativeObligationSetV1` proposto conserva
  attribution, copyright/permission notices, disclaimers, trademark treatment
  e change marking exatos. O mesmo conjunto acompanha o manifest, o PNG e a
  apresentação textual acessível adjacente no Dashboard.
- Persistência e recuperação: a proposta vincula o obligation set ao source
  object, rights mapping, render manifest e ativação; inclui esse vínculo no
  reachability, backup e cold restore e bloqueia readiness ou serving diante
  de ausência, divergência ou staleness.
- Impactos explícitos: uma implementação futura exigiria novo schema de
  manifest e obligation set, migration dos constraints SQLite que hoje aceitam
  somente `pdf-page-png-v1` e revisão pública do contrato v2 para transportar
  a identidade e o texto acessível. OpenAPI v1 permaneceria byte a byte
  protegida.
- Disposição: ADR-0012 permanece `proposed` e não foi aceito. O candidato
  `postgresql-18-reference-a4` permanece `BLOCKED/EXCLUDED`; seus quatro
  direitos visuais continuam `UNPROVEN` e a distribuição/publicação externa
  continua `DENIED` pela fronteira interna já registrada.
- Versionamento: corpus elevado por `PATCH` documental de `4.10.12` para
  `4.10.13`, sem mudança de autoridade executável, contrato, schema, migration,
  comportamento de produto, gate ou lifecycle.
- Limites preservados: somente o ADR proposto, o índice arquitetural,
  `Current-State.md`, este EOF append-only e o changelog do corpus foram
  alterados. Nenhum código, teste, OpenAPI, contrato, schema, migration,
  dependência, lockfile ou dataset foi alterado. Nenhum renderer, runtime,
  rede, fonte, provider, Automatic Quality Gate, Human Gate, lifecycle ou ação
  externa foi executado.
- Verificação documental estática: `git diff --check` e
  `pwsh -NoProfile -File eng/check-repository.ps1` terminaram com exit code
  `0`; o histórico preservou byte a byte o prefixo anterior no SHA-256
  `50b091900650543f7f5a73017a83e8bf11b7622adf9164c53a3a68a43f9b2957`.
- Próxima condição: uma decisão humana explícita pode aceitar ou rejeitar o
  ADR-0012. Uma eventual aceitação estabelecerá somente autoridade
  arquitetural e não autorizará reconciliação, contrato, schema, migration ou
  implementação.

## 2026-08-09 — ADR-0012 aceito como autoridade arquitetural

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado.
- Decisão e baseline: o proprietário declarou explicitamente
  `ADR-0012: ACEITAR.` sobre branch `main`, commit
  `243a448823a114190f68a25f9d521e1849eddacf`, corpus `4.10.13`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas.
- Autoridade arquitetural: ADR-0012 passa de `proposed` para `accepted` e
  seleciona `pdf-page-png-notice-v1` como o único mecanismo de imagem
  autocontida, com região da página pixel a pixel intacta e painel separado de
  obrigações completas no mesmo PNG.
- Modelo aceito: `DerivativeObligationSetV1` permanece imutável e vinculado ao
  source object, rights mapping, render manifest e ativação. O mesmo conteúdo
  acompanha armazenamento, backup/cold restore, serving same-origin e a
  apresentação textual acessível adjacente no Dashboard.
- Impactos preservados como futuros: novo schema de obligation set/manifest,
  migration dos constraints SQLite e revisão pública do contrato v2 continuam
  necessários e separadamente autorizados. OpenAPI v1 permanece byte a byte
  protegida e OpenAPI v2 não foi alterada por esta aceitação.
- Candidato inalterado: `postgresql-18-reference-a4` permanece
  `BLOCKED/EXCLUDED`; page rendering, derivative-image creation/retention e
  runtime display permanecem `UNPROVEN`, e a distribuição/publicação externa
  permanece `DENIED` pela fronteira interna já registrada. Nenhum novo A0 foi
  executado.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.13` para
  `4.10.14`, sem mudança de contrato, schema, migration, implementação,
  comportamento de produto, gate ou lifecycle.
- Limites preservados: somente ADR-0012, o índice arquitetural,
  `Current-State.md`, este EOF append-only e o changelog do corpus foram
  alterados. Nenhum código, teste, OpenAPI, contrato, schema, migration,
  dependência, lockfile ou dataset foi alterado. Nenhum renderer, runtime,
  rede, fonte, provider, Automatic Quality Gate, Human Gate, lifecycle ou ação
  externa foi executado.
- Verificação documental estática: `git diff --check` e
  `pwsh -NoProfile -File eng/check-repository.ps1` terminaram com exit code
  `0`; o histórico preservou byte a byte o prefixo anterior no SHA-256
  `d6c503c268ddabd9a832de9b15b160100aed9f76492669834dd372f27fd6af96`.
- Próxima condição: a reconciliação semântica do ADR-0012 aceito exige
  autoridade humana separada e deve preceder qualquer revisão de contrato,
  schema, migration ou implementação.

## 2026-08-09 — ADR-0012 reconciliado semanticamente

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado.
- Autoridade e baseline:
  `AUTH-S07-A-NOTICE-BEARING-PROFILE-RECONCILE-001`, branch `main`, commit
  `5c2cea66e45f13479486a345552e5cc3cd47fefe`, corpus `4.10.14`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas.
- Reconciliação aplicada: ADR-0008, o contrato documental v2, data dictionary,
  Security-And-Access, threat model e registro de elegibilidade agora aplicam
  semanticamente `pdf-page-png-notice-v1` e o
  `DerivativeObligationSetV1` aceitos no ADR-0012.
- Integridade e obrigações: a região da página permanece pixel a pixel intacta;
  um painel separado do mesmo PNG carrega attribution, copyright/permission
  notices, disclaimers, trademark treatment e change marking completos. O
  obligation set é imutável e vinculado a source object, rights mapping,
  manifest e ativação.
- Persistência e entrega: storage e reachability protegem em conjunto PNG e
  obligation set; backup/cold restore verificam objetos, digests, regiões e
  vínculos; serving same-origin revalida antes de `200`/`304`, usa o hash
  composto como ETag e exige apresentação textual completa e acessível junto
  da figura.
- Fronteiras futuras: revisão protegida do contrato v2, schema e migration são
  obrigatórios antes da implementação. Nenhum deles foi executado; OpenAPI
  v1/v2 e o contrato público executável permanecem inalterados.
- Disposição preservada: as dez decisões continuam independentes e fail-closed.
  `postgresql-18-reference-a4` permanece `BLOCKED/EXCLUDED`, com quatro
  operações visuais `UNPROVEN` e distribuição/publicação externa `DENIED` pela
  fronteira interna registrada. Nenhum novo A0 foi executado.
- Versionamento: corpus elevado por `PATCH` documental de `4.10.14` para
  `4.10.15`, sem código, teste, renderer, dataset, runtime, rede, fonte,
  provider, gate ou mudança de lifecycle.
- Verificação documental estática: `git diff --check` e
  `pwsh -NoProfile -File eng/check-repository.ps1` terminaram com exit code
  `0`; o histórico preservou byte a byte o prefixo anterior no SHA-256
  `41a0b7c19360786c4249bb195b89a2b8c4605110163c3f397568cfeece909d8a`.
- Próxima condição: uma revisão protegida e separadamente autorizada do contrato
  v2 deve definir `obligationSetId` e
  `DerivativeObligationPresentationV1` antes de schema, migration ou
  implementação.

## 2026-08-10 — Contrato público v2 notice-bearing congelado

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado.
- Autoridade e baseline: `AUTH-S07-A-NOTICE-BEARING-V2-CONTRACT-001`, branch
  `main`, commit `6982b0643468aee0a97c3bea6b5bbe9018f0804c`, corpus `4.10.15`,
  working tree inicialmente limpa e OpenAPI v1/v2 protegidas.
- Contrato congelado: `PageImageEvidenceV1` acrescenta somente o required
  nullable `obligationSetId`; `CitationV2` acrescenta somente o required
  nullable `DerivativeObligationPresentationV1`. A rota same-origin e todos os
  campos anteriores permanecem.
- Compatibilidade e fail-closed: a projeção legada usa ambos os valores `null`.
  Uma citação notice-bearing exige o mesmo ID não nulo em todas as suas páginas
  e uma apresentação completa com ID e `contentLanguage` coincidentes. Mistura,
  ausência, divergência, campo desconhecido, texto vazio ou acima dos limites
  falha no decoder estrito; a projeção confiável permanece responsável por
  conferir o texto integral contra o obligation set. CSV permanece sem imagens
  ou obrigações.
- Identidades: OpenAPI v1 permaneceu byte a byte no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`. A nova OpenAPI v2 possui SHA-256
  `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` e blob
  `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`.
- Verificação focal: lint e typecheck do Dashboard passaram; os cinco testes do
  decoder v2 e os seis testes .NET de `ApiV2ContractTests` passaram; a auditoria
  estática aprovou 257 arquivos não ignorados e `git diff --check` terminou com
  exit code `0`.
- Disposição preservada: `postgresql-18-reference-a4` continua
  `BLOCKED/EXCLUDED`; nenhum novo A0, schema, migration, renderer, armazenamento,
  runtime de produto, dataset, rede, fonte, provider, Automatic Quality Gate,
  Human Gate ou lifecycle foi executado.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.15` para
  `4.10.16`. O histórico preservou byte a byte seu prefixo anterior no SHA-256
  `5b59d6eedcf59daf525d499d8739e087c82d406469aaa732f67f7240dc716dc6`.
- Próxima condição: o design e a migration de schema exigidos pelo ADR-0012
  permanecem sob autoridade separada e devem preceder a implementação do
  obligation set, renderer e serving notice-bearing.

## 2026-08-10 — Schema e migrations notice-bearing reconciliados

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado.
- Autoridade e baseline:
  `AUTH-S07-A-NOTICE-BEARING-SCHEMA-MIGRATION-RECONCILE-001`, branch `main`,
  commit `98036f3c8c496544f4532d1fe48c981f836a1871`, corpus `4.10.16`, working
  tree inicialmente limpa e OpenAPI v1/v2 protegidas.
- Implementação reconciliada: o incremento
  `AUTH-S07-A-NOTICE-BEARING-SCHEMA-MIGRATION-001`, concluído no commit focal
  `98036f3c8c496544f4532d1fe48c981f836a1871`, persiste
  `DerivativeObligationSetV1` imutável e seus blocos ordenados, permite
  `pdf-page-png-notice-v1` ao lado do perfil legado e vincula
  `obligationSetId`/digest e dimensões source/notice ao render manifest.
- Migrations e fail-closed: as migrations
  `20260810033026_AddNoticeBearingObligationSchema` e
  `20260810034537_SealNoticeBearingObligationBindings` aplicam constraints,
  foreign keys e sealing triggers depois do rebuild SQLite. Nenhum notice ou
  direito foi inferido; registros, manifests, hashes e ativações legados não
  receberam backfill nem mutação.
- Evidência focal observada: 7/7 testes passaram; Entity Framework não informou
  pending model changes; `foreign_key_check`, upgrade, rollback para zero,
  reapply passaram em stores SQLite temporários task-owned.
- Integridade protegida: OpenAPI v1 permaneceu no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; OpenAPI v2 permaneceu no SHA-256
  `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` e blob
  `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`.
- Cleanup e escopo negativo: o cleanup task-owned foi concluído. Nenhum código,
  teste, OpenAPI, contrato público, schema, migration, renderer, serving,
  Dashboard, dataset, dependência ou lockfile foi alterado nesta reconciliação.
  Nenhum novo A0, rede, fonte, provider, Automatic Quality Gate, Human Gate ou
  lifecycle foi executado.
- Disposição preservada: `postgresql-18-reference-a4` continua
  `BLOCKED/EXCLUDED`, com as cinco decisões candidato-específicas registradas
  sem reclassificação.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.16` para
  `4.10.17`. O histórico preservou byte a byte seu prefixo anterior no SHA-256
  `3f73437db25258856be047f291e002c1738e2b7b764afbb9199bf2f91d4fdb23`.
- Próxima condição: a implementação do obligation-set model, renderer,
  manifest, storage, reachability, serving e Dashboard notice-bearing exige
  autoridade humana separada antes da verificação focal ou de um novo A0.

## 2026-08-10 — ADR-0013 aceito como autoridade arquitetural

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado.
- Decisão e baseline: o proprietário declarou explicitamente
  `ADR-0013: ACEITAR.` sobre branch `main`, commit
  `f03162bad0fc166a597739b22e55fbc46ec59535`, corpus `4.10.17`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas.
- Autoridade arquitetural: ADR-0013 passa de `proposed` para `accepted`,
  seleciona `gpt-5.4-mini-2026-03-17` como único candidato de LLM do MVP e
  substitui somente a seleção anterior de LLM do ADR-0005. Todas as demais
  decisões do ADR-0005 permanecem inalteradas.
- Candidato diferido: `gpt-5.6-sol` permanece inativo e não é fallback, modelo
  secundário ou alvo configurável do runtime. Seu identificador móvel continua
  registrado como risco de reprodutibilidade para uma avaliação futura.
- Requisitos preservados: a matriz `pt-BR`/`en-GB`, groundedness, identidade de
  citações, insuficiência de evidência, prompt injection e limites de latência
  permanecem gates obrigatórios. Custo continua excluído do ranking sem
  remover os controles de gasto já aceitos.
- Limites preservados: não foram autorizados ou executados reconciliação
  semântica, código, adaptador, configuração, OpenAPI, conta, credencial,
  provider, chamada paga, corpus real, avaliação, OCI, deploy ou comportamento
  de produto.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.17` para
  `4.10.18`. O histórico preservou byte a byte seu prefixo anterior no SHA-256
  `b5a989543edc1895388997b2395934fd3a0827fac8811a85b8d8118a1eb09e4b`.
- Próxima condição: uma reconciliação semântica separadamente autorizada deve
  aplicar somente a seleção de LLM do ADR-0013 ao ADR-0005 e aos proprietários
  documentais aplicáveis, sem alterar código, OpenAPI, configuração, provider,
  corpus real, OCI ou lifecycle.

## 2026-08-10 — ADR-0013 reconciliado semanticamente

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado.
- Autoridade e baseline:
  `AUTH-STATE07-LLM-CANDIDATE-ADR-RECONCILE-001`, branch `main`, commit
  `a08aa83c7319b97ead6c91a92ae8cbb4da5c28cc`, corpus `4.10.18`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas.
- Reconciliação aplicada: ADR-0005, o relatório arquitetural de `STATE-02` e o
  índice de arquitetura passam a selecionar `gpt-5.4-mini-2026-03-17` como
  único candidato de LLM do MVP. A seleção anterior
  `gpt-4.1-mini-2025-04-14` permanece identificada somente nos contextos
  históricos pertinentes.
- Candidato diferido: `gpt-5.6-sol` permanece futuro e inativo, não é fallback,
  modelo secundário ou alvo de troca dinâmica e conserva o risco registrado de
  identificador móvel.
- Limites preservados: todas as demais decisões do ADR-0005 permanecem
  inalteradas. Os documentos temáticos de RAG e segurança já eram compatíveis e
  não fixavam o modelo substituído. Nenhum código, teste, OpenAPI, configuração,
  conta, credencial, provider, chamada paga, corpus real, avaliação, OCI,
  deploy ou comportamento de produto foi alterado, acessado ou executado.
- Versionamento: corpus elevado por `PATCH` documental de `4.10.18` para
  `4.10.19`. O histórico preservou byte a byte seu prefixo anterior no SHA-256
  `3401d62915f81170512152471c2ebf5b357991bd943b6f432be39e1daaaff610`.
- Próxima condição: o incremento de compatibilidade do adaptador descrito no
  ADR-0013 exige autoridade humana separada. Essa futura autoridade não pode
  inferir acesso a conta, credencial, provider, corpus real, OCI, chamada paga,
  avaliação, gate ou lifecycle.

## 2026-08-10 — Compatibilidade local do adaptador GPT-5.4 mini reconciliada

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado.
- Autoridade e baseline documental:
  `AUTH-STATE07-LLM-ADAPTER-COMPAT-RECONCILE-001`, branch `main`, commit
  `b6d6f9102ecf0ea93309f8080acebad02cf16584`, corpus `4.10.19`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas. O runtime preflight desta
  reconciliação puramente documental foi `NÃO APLICÁVEL`.
- Implementação reconciliada: o incremento autorizado sob
  `AUTH-STATE07-LLM-ADAPTER-COMPAT-001` partiu de
  `main@27b385d0f534739ccbc4e8d946eea654e00df9fe` e alterou somente o adaptador
  OpenAI em Infrastructure e seus testes de contrato. O commit focal
  `b6d6f9102ecf0ea93309f8080acebad02cf16584` exige o snapshot exato
  `gpt-5.4-mini-2026-03-17`, configura reasoning effort/context de modo tipado
  e imutável, preserva `store=false`, omite `tools`, `temperature` e estado de
  resposta anterior e valida estritamente a mensagem estruturada final.
- Evidência focal observada: 18 de 18 testes do adaptador com handler falso e
  11 de 11 testes de arquitetura passaram; a verificação de formatação passou
  sem mudanças e a auditoria do repositório aprovou 266 arquivos não
  ignorados. O commit final deixou a working tree limpa.
- Artefatos protegidos: OpenAPI v1 permaneceu no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; OpenAPI v2 permaneceu no
  SHA-256 `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` e
  blob `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`.
- Limites preservados: não houve configuração operacional, acesso a conta,
  credencial, provider, corpus real ou OCI, chamada externa ou paga, avaliação
  real, deploy, Automatic Quality Gate, Human Gate ou mudança de lifecycle. A
  evidência não homologa qualidade bilíngue, groundedness, citações,
  insuficiência de evidência, prompt injection ou latência do provider.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.19` para
  `4.10.20`. O histórico preservou byte a byte seu prefixo anterior no SHA-256
  `75b34802ec60281caeef1386f85cc1a61a8a48cae6d647df2eb91305bd5f9504`.
- Próxima condição: o Automatic Quality Gate específico deste incremento exige
  autoridade humana separada e permanece limitado à fronteira local, offline,
  determinística e com handler falso; não autoriza provider, conta,
  credencial, corpus real, OCI ou chamada paga.

## 2026-08-10 — Automatic Quality Gate do adaptador GPT-5.4 mini aprovado

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Human Gate ou lifecycle foi executado ou alterado.
- Autoridade e baseline documental:
  `AUTH-STATE07-LLM-ADAPTER-COMPAT-AQG-RECONCILE-001`, branch `main`, commit
  `6e6fdabb91e2fb4c5186c464ce08f5da390d727a`, corpus `4.10.20`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas. O runtime preflight desta
  reconciliação puramente documental foi `NÃO APLICÁVEL`.
- Gate reconciliado: o Automatic Quality Gate autorizado sob
  `AUTH-STATE07-LLM-ADAPTER-COMPAT-AQG-001` auditou o commit de implementação
  `b6d6f9102ecf0ea93309f8080acebad02cf16584` somente na fronteira local,
  offline, determinística e com handlers falsos. O resultado foi `APROVADO`,
  sem achado P0, P1, P2 ou P3.
- Evidência observada: o preflight do gate não encontrou processo ou listener
  do RAG-Challenge; a auditoria aprovou 266 arquivos, a formatação não exigiu
  mudanças, 18 de 18 testes focais e 11 de 11 testes de arquitetura passaram.
  A CI offline completa aprovou 154 testes unitários, 191 de integração, 11 de
  arquitetura e 45 do Dashboard, com cobertura de 95,63% de linhas e 67,65%
  de branches e build sem avisos ou erros.
- Auditoria estática: os sete requisitos do ADR-0013 foram atendidos, o
  incremento conservou somente os dois paths executáveis esperados, não houve
  diff executável posterior nem padrão de secret, e nenhum identificador de
  modelo legado ou candidato futuro inativo permaneceu em `src/` ou `tests/`.
- Artefatos protegidos: OpenAPI v1 permaneceu no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; OpenAPI v2 permaneceu no
  SHA-256 `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` e
  blob `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`.
- Limites preservados: avaliação real, conta, credencial, provider, qualidade
  bilíngue, groundedness, citações, insuficiência de evidência, prompt
  injection, latência, custo, corpus real, OCI, deploy, Human Gate e mudança
  de lifecycle permanecem `NOT_RUN`.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.20` para
  `4.10.21`. O histórico preservou byte a byte seu prefixo anterior no SHA-256
  `a35fdb41fb9eb8798d35b0aa96b4d5e7afe36719e3fcc913b6ace2b84c44c755`.
- Próxima condição: qualquer proposta de avaliação real do modelo exige
  autoridade humana separada e deve congelar dataset, prompt, configuração,
  ambiente, limites, orçamento e critérios de parada antes de acessar conta,
  credencial, provider, corpus real ou OCI ou executar chamada paga.

## 2026-08-10 — Preparação da campanha candidata GPT-5.4 mini reconciliada

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado.
- Autoridade e baseline documental:
  `AUTH-STATE07-S07-A-PROVIDER-PREP-RECONCILE-001`, branch `main`, commit
  `422286863e7a3c213e96db18144769bd0458a75b`, corpus `4.10.21`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas. O runtime preflight desta
  reconciliação puramente documental foi `NÃO APLICÁVEL`.
- Preparação reconciliada: o incremento autorizado sob
  `AUTH-S07-A-PROVIDER-PREP-001` criou no commit focal
  `422286863e7a3c213e96db18144769bd0458a75b` somente a revisão sucessora
  imutável `rag-eval-catalogue-v1-provider-gpt54m-candidate-001` e seu harness
  de integração. A revisão anterior permaneceu preservada.
- Matriz sintética: dois documentos sintéticos e zero documento real suportam
  60 casos; 40 respondíveis, dez por cada direção obrigatória
  `pt-BR`/`en-GB`; 20 de insuficiência, divididos igualmente entre zero chamada
  e evidência presente porém insuficiente; e 12 de prompt injection, com seis
  classes por idioma de pergunta. Esses números são inventário preparado, não
  resultado de qualidade.
- Configuração congelada: snapshot `gpt-5.4-mini-2026-03-17`, prompt e schema
  com digests, `store=false`, tools e parâmetros não comprovados omitidos,
  `reasoning.effort=none`, `reasoning.context=current_turn`, limites aceitos,
  agenda de 4 smoke + 5 warm-up + 100 medidas, máximo 109, retry zero,
  concorrência um, orçamento operacional de `USD 16` e teto absoluto de
  `USD 20`. Nenhum desses valores constitui uso ou custo observado.
- Evidência focal observada na preparação: 2 de 2 testes do novo harness e 20
  de 20 testes combinados com o contrato OpenAI passaram somente com handlers
  falsos; a formatação não exigiu mudança; `git diff --check` passou; e a
  auditoria aprovou 275 arquivos não ignorados. A reconciliação atual
  recomputou os cinco digests de arquivo e de manifest e conferiu as três
  identidades da revisão predecessora sem executar o harness.
- Artefatos protegidos: OpenAPI v1 permaneceu no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; OpenAPI v2 permaneceu no
  SHA-256 `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` e
  blob `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`.
- Limites preservados: conta, credencial, provider, corpus/fonte real, OCI,
  chamada externa ou paga, avaliação real, qualidade bilíngue, groundedness,
  citações, insuficiência de evidência real, resistência a prompt injection,
  latência, custo observado, deploy, Automatic Quality Gate, Human Gate e
  mudança de lifecycle permanecem `NOT_RUN`.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.21` para
  `4.10.22`. O histórico preservou byte a byte seu prefixo anterior no SHA-256
  `b933841bef5680cd7c4616ae68169a0740607fa938bd19ae627d2a35b1c443c6`.
- Próxima condição: um Automatic Quality Gate local e offline da preparação e
  qualquer execução real da campanha exigem autoridades humanas separadas. A
  execução real também exige conta, referência de secret, egress, spend e
  corpus de produto explicitamente autorizados.

## 2026-08-10 — Automatic Quality Gate da preparação da campanha GPT-5.4 mini aprovado

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Human Gate ou lifecycle foi executado ou alterado.
- Autoridade e baseline documental:
  `AUTH-STATE07-S07-A-PROVIDER-PREP-AQG-RECONCILE-001`, branch `main`, commit
  `5d74c9c9190b0b3465b11dc6864e3dd519cc88f9`, corpus `4.10.22`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas. O runtime preflight desta
  reconciliação puramente documental foi `NÃO APLICÁVEL`.
- Gate reconciliado: o Automatic Quality Gate autorizado sob
  `AUTH-S07-A-PROVIDER-PREP-AQG-001` auditou a preparação implementada no
  commit `422286863e7a3c213e96db18144769bd0458a75b` e reconciliada no commit
  `5d74c9c9190b0b3465b11dc6864e3dd519cc88f9`. O resultado foi `APROVADO`, sem
  achado P0, P1, P2 ou P3, somente na fronteira local, offline, determinística
  e com handlers falsos.
- Auditoria estática: a revisão predecessora permaneceu inalterada; os cinco
  manifests e seus digests conferiram; a sucessora preservou 60 casos, 12 de
  prompt injection, 20 de insuficiência, configuração congelada, agenda máxima
  de 109 chamadas, orçamento operacional de `USD 16` e teto absoluto de
  `USD 20`. Nenhuma chamada da agenda foi executada.
- Evidência observada: o preflight do gate encontrou zero processo do
  RAG-Challenge; 2 de 2 testes do harness e 20 de 20 testes combinados passaram
  com handlers falsos. A CI offline aprovou 154 testes unitários, 193 de
  integração, 11 de arquitetura e 45 do Dashboard, com cobertura de 95,63% de
  linhas e 67,66% de branches e build sem avisos ou erros. Formatação,
  `git diff --check`, verificações de transporte/secrets e auditoria de 275
  arquivos também passaram; o runtime terminou com zero processo pertencente
  ao projeto.
- Artefatos protegidos: OpenAPI v1 permaneceu no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; OpenAPI v2 permaneceu no
  SHA-256 `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` e
  blob `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`.
- Limites preservados: conta, credencial, provider, chamada paga, corpus/fonte
  real, avaliação real, qualidade bilíngue, groundedness, citações,
  insuficiência de evidência real, resistência a prompt injection, latência,
  custo observado, OCI, deploy, Human Gate e mudança de lifecycle permanecem
  `NOT_RUN`.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.22` para
  `4.10.23`. O histórico preservou byte a byte seu prefixo anterior no SHA-256
  `38c8a5b6baa6890f13800cef3d7eb2e553512f713e46ae72e09925ab2502cb26`.
- Próxima condição: qualquer avaliação real da campanha exige autoridade
  humana separada para conta, referência de secret, egress, provider, chamadas
  pagas, corpus/fonte real, limites operacionais e execução do plano de parada;
  este gate não autoriza Human Gate nem mudança de lifecycle.

## 2026-08-11 — Implementação notice-bearing reconciliada factualmente

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado.
- Autoridade e baseline documental:
  `AUTH-S07-A-NOTICE-BEARING-PROFILE-IMPL-RECONCILE-001`, concedida pela
  solicitação explícita do proprietário para auditar e finalizar as pendências
  das duas tarefas, branch `main`, commit
  `7f363a3e2036e4a76626eff482052bf7343c3cd7`, corpus `4.10.23`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas. O runtime preflight desta
  reconciliação puramente documental foi `NÃO APLICÁVEL`.
- Escopo documental fechado: `README.md`, `docs/README.md`, o índice de
  arquitetura, ADR-0008, ADR-0011, ADR-0012, contratos canônicos, dicionário de
  dados, threat model, proposta v2, registro de elegibilidade, relatório de
  homologação, RAG Module, Solution Architecture, Security and Access, Current
  State, este histórico append-only e Prompt System Change Log; 18 arquivos ao
  todo. Afirmações cronológicas de incrementos anteriores foram preservadas.
- Implementação reconciliada: o commit focal
  `f682827d1a26b08fa8c450a1fadb3bd0e1fa1700` implementou o
  `DerivativeObligationSetV1`, a composição determinística de
  `pdf-page-png-notice-v1` com região da página preservada pixel a pixel, o
  vínculo ao manifest e à persistência/reachability, a revalidação fail-closed
  em readback e serving v2 same-origin e a apresentação acessível do conteúdo
  integral no Dashboard. O commit é ancestral do baseline atual e seus 22
  paths de código/testes permanecem byte-idênticos no `HEAD` auditado.
- Evidência histórica observada no incremento: build Release sem avisos ou
  erros; 47 de 47 testes unitários, 40 de 40 testes de integração/contrato
  v1/v2, 11 de 11 testes de arquitetura e 45 de 45 testes do Dashboard; build
  e lint do Dashboard aprovados; cleanup sem processo ou listener residual do
  projeto. Essa evidência é focal e não constitui Automatic Quality Gate.
- Artefatos protegidos: OpenAPI v1 permaneceu no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e
  OpenAPI v2 permaneceu no SHA-256
  `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733`.
- Limites preservados: registros legados e a disposição do PostgreSQL não
  mudaram; nenhum obligation set, manifest, derivado, dataset, índice ou
  ativação candidato-específico foi criado. Novo A0, dado/corpus real,
  browser/tecnologia assistiva, provider, fonte/rede, OCI, deploy, Automatic
  Quality Gate, Human Gate e mudança de lifecycle permanecem `NOT_RUN`.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.23` para
  `4.10.24`. O histórico preservou byte a byte seu prefixo anterior no SHA-256
  `c3c3463ad029286cb5c66b53c8954da44502113d7ac8f5406316495908a8d2f2`.
- Próxima condição: obter autoridade humana separada para executar o Automatic
  Quality Gate local, offline, determinístico e sintético do comportamento
  notice-bearing. Somente após eventual aprovação e reconciliação cabe um novo
  A0 candidato-específico, também sob autoridade separada.

## 2026-08-11 — Fechamento sanitizado da Admin key reconciliado factualmente

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado.
- Autoridade e baseline documental:
  `AUTH-S07-A-PROVIDER-ADMIN-KEY-CLEANUP-RECONCILE-001`, branch `main`, commit
  `b2654088d11ab94c23cdf19e2aa57d89f0b3ae49`, corpus `4.10.24`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas. O runtime preflight desta
  reconciliação puramente documental foi `NÃO APLICÁVEL`.
- Fonte factual: registro de fechamento sanitizado fornecido pelo proprietário
  para o cleanup concluído sob `AUTH-S07-A-PROVIDER-ADMIN-KEY-CLEANUP-002`.
  Esta reconciliação não reacessou OpenAI, Credential Manager, provider,
  billing, projeto ou qualquer credencial e não constitui reobservação live
  independente.
- Fechamento registrado: a Admin key com label exato
  `s07-a-provider-gpt54m-candidate-001-admin-provisioning` foi revogada, está
  ausente do inventário Active e aparece historicamente somente como Inactive;
  `Last used` permaneceu `Never` e o gasto permaneceu `USD 0.00`.
- Cleanup local registrado: o target
  `RAG-Challenge/OpenAI/AdminKey/s07-a-provider-gpt54m-candidate-001` foi
  removido do Windows Credential Manager e sua ausência foi verificada no
  cleanup autorizado. Nenhum secret, fragmento, fingerprint ou representação
  mascarada foi incluído nos registros.
- Limites preservados: não houve chamada de provider ou `/v1/responses`, custo
  novo, alteração de billing, limites, allowlist ou projeto, Automatic Quality
  Gate, Human Gate ou mudança de lifecycle.
- Artefatos protegidos: OpenAPI v1 permaneceu no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; OpenAPI v2 permaneceu no
  SHA-256 `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` e blob
  `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`.
- Escopo documental fechado: somente o relatório de homologação, Current State,
  este histórico append-only no EOF e Prompt System Change Log foram alterados.
- Verificação documental: `git diff --check` terminou com exit code `0` e
  `eng/check-repository.ps1` aprovou 275 arquivos não ignorados. O diff conteve
  somente os quatro documentos autorizados; UTF-8/LF, newline final, links e
  formato passaram; e OpenAPI v1/v2 permaneceram byte a byte protegidas.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.24` para
  `4.10.25`. O histórico preservou byte a byte seu prefixo anterior no SHA-256
  `1eea14d98fcd7014d99c070b4c6b1a92c096ef0f8dfec927f96bafda45492a63`.
- Próxima condição diretamente relacionada: nenhuma; o cleanup administrativo
  e sua reconciliação factual estão encerrados sem gate ou lifecycle.

## 2026-08-11 — Reauditorias das fronteiras de preflight e homologação reconciliadas

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Automatic Quality Gate, A0, Human Gate ou lifecycle foi
  executado ou alterado.
- Autoridade e baseline documental:
  `AUTH-STATE07-PREFLIGHT-BOUNDARY-REAUDIT-RECONCILE-001`, branch `main`, commit
  `1629df7cac27f48b21f64b1a0f1e440cc1cf7f20`, corpus `4.10.25`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas. O runtime preflight desta
  reconciliação puramente documental foi `NÃO APLICÁVEL`.
- Reauditorias reconciliadas: “Preflight operacional GPT-5.4-mini” e “Next
  Homologation Boundary”. O preflight operacional inicial terminou
  `BLOQUEADO`, sem campanha real, chamada de provider ou `/v1/responses`.
- Cleanup encerrado: o fechamento da Admin key e da credencial local permanece
  conforme o registro sanitizado já reconciliado. Este lote não reacessou
  OpenAI, Credential Manager, provider, billing, projeto ou qualquer secret.
- Revogação factual: o fluxo experimental Coordinator/Docker/C3 não constitui
  autoridade vigente nem pendência canônica.
- Fronteira notice-bearing: o mecanismo foi implementado no commit
  `f682827d1a26b08fa8c450a1fadb3bd0e1fa1700`, mas não reclassifica
  retroativamente o A0. Page rendering, derivative-image creation,
  derivative-image retention e runtime derivative display permanecem
  `UNPROVEN` porque o Automatic Quality Gate notice-bearing e um novo A0
  candidato-específico não foram executados, não porque o mecanismo continue
  inexistente.
- Separação preservada: o Automatic Quality Gate notice-bearing, a eventual
  reconciliação documental de seu resultado e o novo A0 candidato-específico
  permanecem etapas distintas e `NOT_RUN`.
- Escopo documental fechado: somente o relatório de homologação, Current State,
  este histórico append-only no EOF e Prompt System Change Log foram alterados.
  Nenhum código, runtime, teste comportamental, gate, A0, provider, rede,
  browser, credencial, billing, Docker, cleanup de host, corpus real, Human Gate
  ou lifecycle foi autorizado ou executado.
- Artefatos protegidos: OpenAPI v1 permaneceu no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; OpenAPI v2 permaneceu no
  SHA-256 `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` e blob
  `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`.
- Verificação documental: `git diff --check` terminou com exit code `0` e
  `eng/check-repository.ps1` aprovou 275 arquivos não ignorados.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.25` para
  `4.10.26`. O histórico preservou byte a byte seu prefixo anterior no SHA-256
  `d08bf87f6fd28690598623cfe3531ec8d96053cb1229c38e392b9d18ed66fff4`.
- Próxima condição: obter autoridade humana separada para o Automatic Quality
  Gate notice-bearing. Sua eventual reconciliação e um novo A0
  candidato-específico exigem autoridades posteriores e independentes.

## 2026-08-11 — ADR-0014 aceito como autoridade arquitetural

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado.
- Autoridade e baseline documental:
  `AUTH-STATE07-RETRIEVAL-DETERMINISM-ADR-RECONCILE-001`, decisão explícita do
  proprietário `ADR-0014: ACEITAR.`, branch `main`, commit
  `52e1ac7d9bc61be196549a8ee61399fde477b8fb`, corpus `4.10.26`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas. O runtime preflight desta
  reconciliação puramente documental foi `NÃO APLICÁVEL`.
- Autoridade arquitetural: ADR-0014 passa de `proposed` para `accepted` e
  registra como contrato explícito a ordenação já existente
  `Score DESC, global ChunkOrdinal ASC`. `retrieval-v1` permanece inalterado
  para entradas válidas; nenhum defeito de não determinismo em entradas válidas
  foi alegado ou corrigido por um novo tie-break.
- Fronteira interna: a decisão define a futura porta Application retrieval-only,
  scores finitos em `[-1, 1]`, ordinal global único, outcomes tipados e falha
  fechada. Esses requisitos não foram implementados nem testados nesta
  reconciliação.
- Baseline de avaliação: a decisão separa contrato de design, dataset de
  produto materializado e manifests de `campaign-input` específicos por
  política; define scorer, métricas, denominadores, matrizes mínimas, replay
  determinístico e as classificações `COMPARATIVE_PILOT` e representativa de
  produto. Nenhum dataset, vetor, campanha, score ou resultado foi
  materializado ou executado.
- MultiQuery: `retrieval-multi-query-v1-candidate` permanece não canônico e
  estacionado. Uma comparação futura exige autoridade própria, o mesmo
  denominador exato de casos e resultados executados separadamente.
- Limites preservados: nenhum código, teste executável, corpus real, dataset,
  campanha, provider, credencial, rede, chamada paga, OpenAPI, schema,
  migration, Automatic Quality Gate, Human Gate, lifecycle, push ou publicação
  foi autorizado ou executado.
- Artefatos protegidos: OpenAPI v1 permaneceu no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; OpenAPI v2 permaneceu no
  SHA-256 `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` e blob
  `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`.
- Escopo documental fechado: somente ADR-0014, o índice de arquitetura,
  Current State, este histórico append-only no EOF e Prompt System Change Log
  foram alterados.
- Verificação documental: `git diff --check` terminou com exit code `0`;
  `eng/check-repository.ps1` aprovou 276 arquivos não ignorados; o diff conteve
  somente os cinco documentos autorizados; UTF-8/LF, newline final, espaços
  finais, links e formato passaram; e o prefixo append-only deste histórico foi
  preservado byte a byte.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.26` para
  `4.10.27`. O histórico preservou byte a byte seu prefixo anterior no SHA-256
  `33f360c67c53e0b089f6a6e17f3a3278f06fde7e6678e09f79382b7efbdbf87c`.
- Próxima condição: obter autoridade humana separada para
  `DR-2 — Determinism implementation`. Dataset, campanha, provider, rede,
  chamada paga, OpenAPI, schema, migration e cada gate posterior exigem
  autoridades independentes; MultiQuery permanece estacionado.

## 2026-08-11 — DR-2 reconciliado factualmente

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado.
- Autoridade e baseline documental: solicitação explícita do proprietário
  `Autorizo exclusivamente a reconciliação documental factual de DR-2 —
  Determinism implementation.`, branch `main`, commit
  `fabb24cad16201070e3b95fffb22467cd55963ab`, corpus `4.10.27`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas. O runtime preflight desta
  reconciliação puramente documental foi `NÃO APLICÁVEL`.
- Autoridade e resultado de implementação: `DR-2 — Determinism implementation`
  foi autorizado separadamente sobre
  `main@ade89d737975f65c38e88b35758f8c6091e57406`, corpus `4.10.27`, e
  concluído no commit focal
  `fabb24cad16201070e3b95fffb22467cd55963ab`, com 14 paths de código e testes.
- Fronteira implementada: a porta Application retrieval-only tipada vincula
  ativação, geração finalizada, `IndexCompatibilityKey`, descritor esperado,
  política fixa, limites e digests. O mesmo executor é chamado pelo query path
  antes do language model. Query, vetores armazenados, normas, scores finitos
  em `[-1, 1]`, ordinal global, identidades, contagem e ordem total são
  validados; estados inválidos produzem outcomes tipados e fail-closed.
- Semântica preservada: entradas válidas de `retrieval-v1` mantêm
  `Score DESC, global ChunkOrdinal ASC`, top-k `8`, mínimo `0.25` inclusivo,
  máximo de seis evidências e orçamento de 16.000 escalares. Stored zero-vector
  mantém score `0`; não foi introduzido clamp, epsilon, nova chave de desempate
  ou alteração da aritmética de cosseno.
- Evidência focal registrada do turno de implementação: build Debug com zero
  avisos e zero erros; 74 de 74 testes unitários, 8 de 8 testes de integração
  locais/SQLite e 11 de 11 testes de arquitetura aprovados; auditoria do
  repositório aprovada para 279 arquivos não ignorados. Esta reconciliação não
  reexecutou testes e essa evidência não constitui `DR-3` ou Automatic Quality
  Gate.
- Limites preservados: nenhum dataset, `retrieval-evaluation-scorer-v1`,
  `campaign-input`, campanha, provider real, credencial, rede, chamada paga,
  corpus real, OpenAPI, schema, migration, dependência, lockfile, nova geração,
  Automatic Quality Gate, Human Gate, lifecycle, push, publicação, deploy ou
  release foi executado ou alterado. `retrieval-multi-query-v1-candidate`
  permanece não canônico e estacionado.
- Artefatos protegidos: OpenAPI v1 permaneceu no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; OpenAPI v2 permaneceu no
  SHA-256 `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` e blob
  `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`.
- Escopo documental fechado: somente ADR-0014, Current State, este histórico
  append-only no EOF e Prompt System Change Log foram alterados; nenhum código
  ou teste foi modificado nesta reconciliação.
- Verificação documental: `git diff --check` terminou com exit code `0`;
  `eng/check-repository.ps1` aprovou 279 arquivos não ignorados; o diff conteve
  somente os quatro documentos autorizados; UTF-8/LF, newline final, espaços
  finais, links e formato passaram; e o prefixo append-only deste histórico foi
  preservado byte a byte.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.27` para
  `4.10.28`. O histórico preservou byte a byte seu prefixo anterior no SHA-256
  `7955af11982aefe5d5cc850e6182e3f73c03562fce50ee84a08894eea58260fd`.
- Próxima condição: `DR-3 — Determinism Automatic Quality Gate` exige
  autoridade humana separada para revisão independente e checks aplicáveis.
  Dataset, campanha, provider, cada gate posterior e qualquer reconsideração
  de MultiQuery permanecem sob autoridades independentes.

## 2026-08-11 — DR-3 reconciliado factualmente como reprovado

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo. `DR-3 — Determinism Automatic Quality Gate` foi `REPROVADO`; nenhum
  Human Gate ou lifecycle foi executado ou alterado.
- Autoridade e baseline documental: solicitação explícita do proprietário
  `Autorizo exclusivamente a reconciliação documental factual de DR-3 —
  Determinism Automatic Quality Gate.`, branch `main`, commit
  `272a868c2f2a90eba21ee422ba5a2c34aa2337d5`, corpus `4.10.28`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas. O runtime preflight desta
  reconciliação puramente documental foi `NÃO APLICÁVEL`.
- Fonte factual e resultado: o hand-off único do gate, executado sob autoridade
  humana separada na mesma baseline, registra `DR-3` como `REPROVADO`, com um
  P1 e três P2. Os achados foram registrados e não corrigidos dentro do gate.
- `DR3-FIND-001 — P1`: vetores admissíveis idênticos `[1f, 1f, 1f]`
  produzem score `1.0000000000000002`. A validação atual converte o resultado
  em `InvalidIndexData` e, no query path, `CH_INDEX_UNAVAILABLE`, mudando uma
  entrada admissível de sucesso para falha. Nenhum epsilon, clamp, alteração de
  aritmética ou nova semântica numérica foi introduzido.
- `DR3-FIND-002 — P2`: o teste de determinismo não prova adversarialmente o
  sort completo antes de `Take(k)`; todos os scores são iguais e uma enumeração
  já ordenada por ordinal pode mascarar regressão.
- `DR3-FIND-003 — P2`: o teste do backend indexing workflow não prova os
  filtros antes de score/top-k quando hits elegíveis e inelegíveis concorrem.
- `DR3-FIND-004 — P2`: a suíte cobre ordinal global duplicado, mas não contém
  regressão executável para `ChunkOrdinal < 0`.
- Semântica observada: a implementação aplica os filtros e depois
  `OrderByDescending(Score)`, `ThenBy(ChunkOrdinal)` e `Take(k)`. Os três P2
  são lacunas de prova, não defeitos comportamentais observados; o P1 é um
  defeito comportamental observado para entrada admissível.
- Evidência executada no gate: build Release com zero avisos e zero erros;
  74/74 testes unitários focais, 35/35 de integração focais e 11/11 de
  arquitetura; 3/3 execuções independentes do caso de empate/reopen; CI
  offline completa com 201 testes unitários, 197 de integração, 11 de
  arquitetura e 45 do Dashboard; cobertura de 95,53% de linhas e 68,34% de
  branches; e auditoria de 279 arquivos não ignorados. As versões registradas
  foram .NET SDK `10.0.302`, Node `24.19.0`, npm `11.17.0` e PowerShell
  `7.6.4`. Esta reconciliação não reexecutou esses checks; seus resultados
  aprovados não superam os quatro achados.
- Runtime do gate: o preflight e o postflight foram aplicáveis à validação
  executável, encontraram zero processo candidato do RAG-Challenge, encerraram
  zero e deixaram zero remanescente. O gate começou e terminou na mesma
  baseline limpa, sem alterar arquivos rastreados, dataset, contrato ou
  configuração; somente outputs ignorados dos checks foram materializados.
- Limitações e escopo negativo: nem o gate nem esta reconciliação provaram
  produto, dataset, `retrieval-evaluation-scorer-v1`, campanha, provider ou
  corpus real. Esta reconciliação não corrigiu código ou testes, não definiu
  semântica numérica e não executou dataset, scorer, campanha, provider,
  credencial, rede, chamada paga, OpenAPI, schema, migration, dependência,
  lockfile, MultiQuery, novo Automatic Quality Gate, Human Gate, lifecycle,
  push, publicação, deploy ou release.
- Artefatos protegidos: OpenAPI v1 permaneceu no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; OpenAPI v2 permaneceu no
  SHA-256 `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` e blob
  `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`.
- Escopo documental fechado: somente ADR-0014, Current State, este histórico
  append-only no EOF e Prompt System Change Log foram alterados; nenhum código
  ou teste foi modificado nesta reconciliação.
- Verificação documental: `git diff --check` terminou com exit code `0`;
  `eng/check-repository.ps1` aprovou 279 arquivos não ignorados; o diff conteve
  somente os quatro documentos autorizados; UTF-8/LF, newline final, espaços
  finais, links e formato passaram; e o prefixo append-only deste histórico foi
  preservado byte a byte.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.28` para
  `4.10.29`. O histórico preservou byte a byte seu prefixo anterior no SHA-256
  `6972e5b4c245c0ed7e6c83b5bad51a1a1aa15d846859380c27f9014c7a041d9f`.
- Próxima condição: obter autoridade humana separada para preparar a decisão
  versionada de semântica numérica e o plano corretivo dos quatro achados.
  Qualquer decisão, implementação corretiva, repetição de DR-3, dataset,
  campanha, provider, gate posterior ou reconsideração de MultiQuery permanece
  independente.

## 2026-08-11 — ADR-0015 preparado como proposta de semântica numérica

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo. `DR-3 — Determinism Automatic Quality Gate` permanece `REPROVADO`,
  com `DR3-FIND-001` P1 e `DR3-FIND-002` a `DR3-FIND-004` P2 abertos; nenhum
  Human Gate ou lifecycle foi executado ou alterado.
- Autoridade e baseline documental:
  `AUTH-DR3-NUMERIC-SEMANTICS-PROPOSAL-001`, branch `main`, commit
  `ce9ba622e7e11214c200482ca50169afb987ee00`, corpus `4.10.29`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas. O runtime preflight desta
  atividade documental foi `NÃO APLICÁVEL`.
- Proposta: ADR-0015 foi criado com status `proposed`. A semântica
  `cosine-f32mul-f64acc-boundary-canonical-v1` e a política `retrieval-v2`
  foram registradas somente como alternativa recomendada se o proprietário
  aceitar a proposta sem alterações. Nenhuma decisão arquitetural foi tomada
  por esta preparação.
- Semântica recomendada: preservar multiplicação binary32, acumulação serial
  binary64, operações IEEE 754 ordenadas e todos os scores já internos ao
  intervalo, inclusive signed zero; canonizar quociente finito acima de `+1`
  para `+1` e abaixo de `-1` para `-1`; manter qualquer estado não finito
  fail-closed; e conservar comparação exata e desempate por ordinal, sem
  epsilon ou bucket.
- Alternativas preservadas: corredor exato de 1 ULP, condicionado a prova do
  limite para todo o domínio configurado; e aritmética escalada em binary64,
  condicionada a requisito ou evidência de robustez para magnitudes finitas
  extremas. Rejeição vigente e epsilon aproximado foram mantidos como opções
  não recomendadas com seus impactos explícitos.
- Compatibilidade proposta: a alternativa recomendada exigiria o descritor
  `sqlite-exact-vector-store/2;schema=1;distance=cosine;algorithm=exact-scan;vector=float32;score=cosine-f32mul-f64acc-boundary-canonical-v1`,
  novo `IndexCompatibilityKey`, nova geração e nova baseline de avaliação.
  Gerações v1 não podem ser relabeladas ou servidas sob a sucessora; OpenAPI,
  schema e migration permanecem inalterados.
- Plano corretivo: `DR3-FIND-001` recebe matriz bit a bit de limites,
  não finitos, zero, compatibilidade, geração e reopen; `DR3-FIND-002` recebe
  top-k adversarial com score/ordinal e permutações de escrita; `DR3-FIND-003`
  recebe concorrência de hits elegíveis/inelegíveis em cada filtro antes de
  score/top-k; e `DR3-FIND-004` recebe ordinal negativo nas fronteiras
  Application e SQLite task-owned. Cada item possui condição executável de
  aprovação, mas nenhum teste foi alterado ou executado nesta preparação.
- Limites preservados: nenhuma alternativa foi aceita ou rejeitada; nenhum
  código, teste, geração, dataset, `retrieval-evaluation-scorer-v1`, campanha,
  provider, credencial, rede, chamada paga, corpus real, OpenAPI, schema,
  migration, dependência, lockfile, MultiQuery, novo Automatic Quality Gate,
  Human Gate, lifecycle, push, publicação, deploy ou release foi executado ou
  alterado.
- Artefatos protegidos: OpenAPI v1 permaneceu no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; OpenAPI v2 permaneceu no
  SHA-256 `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` e blob
  `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`.
- Escopo documental fechado: somente ADR-0015, índice de arquitetura, Current
  State, este histórico append-only no EOF e Prompt System Change Log foram
  alterados.
- Verificação documental: `git diff --check` terminou com exit code `0`;
  `eng/check-repository.ps1` aprovou 280 arquivos não ignorados; o conjunto de
  trabalho conteve somente os cinco documentos autorizados; UTF-8/LF, newline
  final, espaços finais, links, formato e o prefixo append-only deste histórico
  passaram. Nenhum build, teste executável ou Automatic Quality Gate foi
  executado.
- Versionamento: corpus elevado por `PATCH` documental de `4.10.29` para
  `4.10.30`. O histórico preservou byte a byte seu prefixo anterior no SHA-256
  `c925e9b81fafac81c2fa65cd587ac739c0556a9a5f827f2589e724009edce908`.
- Próxima condição: decisão humana explícita sobre ADR-0015. Aceitação,
  rejeição ou revisão da proposta não é inferida deste registro; eventual
  implementação, nova geração e reteste independente de DR-3 continuam
  dependentes de autoridades posteriores e separadas.

## 2026-08-11 — ADR-0015 aceito como autoridade arquitetural

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo. `DR-3 — Determinism Automatic Quality Gate` permanece `REPROVADO`,
  com `DR3-FIND-001` P1 e `DR3-FIND-002` a `DR3-FIND-004` P2 abertos; nenhum
  Human Gate ou lifecycle foi executado ou alterado.
- Autoridade e baseline documental: decisão humana explícita
  `ADR-0015: ACEITAR.`, branch `main`, commit
  `46de807148d5b547f56a0f7265b32428b232100f`, corpus `4.10.30`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas. O runtime preflight desta
  reconciliação documental foi `NÃO APLICÁVEL`.
- Decisão arquitetural: ADR-0015 passa de `proposed` para `accepted` e seleciona
  `cosine-f32mul-f64acc-boundary-canonical-v1` como semântica numérica,
  `retrieval-v2` como política sucessora e o descritor
  `sqlite-exact-vector-store/2;schema=1;distance=cosine;algorithm=exact-scan;vector=float32;score=cosine-f32mul-f64acc-boundary-canonical-v1`.
- Compatibilidade: a decisão exige novo `IndexCompatibilityKey`, nova geração
  e nova baseline de avaliação antes de servir. Gerações `/1` permanecem
  históricas sob `retrieval-v1` e não podem ser relabeladas, migradas ou
  servidas sob a identidade sucessora.
- Semântica selecionada: multiplicação binary32, acumulação serial binary64 e
  scores internos com intermediários finitos permanecem bit a bit; quociente
  finito acima de `+1` é canonizado para `+1`, abaixo de `-1` para `-1`; estado
  não finito permanece fail-closed; comparação exata e desempate por ordinal
  permanecem sem epsilon, bucket ou chave terciária.
- Alternativas não selecionadas: corredor exato de 1 ULP e aritmética escalada
  em binary64 permanecem rastreáveis com suas condições objetivas. Uma mudança
  posterior exige ADR sucessor que superseda explicitamente o ADR-0015.
- Plano corretivo preservado: `DR3-FIND-001` a `DR3-FIND-004` mantêm suas
  matrizes e condições executáveis futuras. A aceitação não altera código ou
  testes, não cria geração e não reexecuta DR-3.
- Limites preservados: nenhuma implementação, geração, dataset,
  `retrieval-evaluation-scorer-v1`, campanha, provider, credencial, rede,
  chamada paga, corpus real, OpenAPI, schema, migration, dependência, lockfile,
  MultiQuery, novo Automatic Quality Gate, Human Gate, lifecycle, push,
  publicação, deploy ou release foi executado ou alterado.
- Artefatos protegidos: OpenAPI v1 permaneceu no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; OpenAPI v2 permaneceu no
  SHA-256 `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` e blob
  `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`.
- Escopo documental fechado: somente ADR-0015, índice de arquitetura, Current
  State, este histórico append-only no EOF e Prompt System Change Log foram
  alterados.
- Verificação documental: `git diff --check` terminou com exit code `0`;
  `eng/check-repository.ps1` aprovou 280 arquivos não ignorados; somente os
  cinco documentos autorizados mudaram; UTF-8/LF, newline final, espaços
  finais, links, formato e o prefixo append-only deste histórico passaram.
  Build, testes executáveis e Automatic Quality Gate permaneceram `NOT_RUN`.
- Versionamento: corpus elevado por `PATCH` documental de `4.10.30` para
  `4.10.31`. O histórico preservou byte a byte seu prefixo anterior no SHA-256
  `cc67d95fca2ef846fa3f7e747e054164c14522b09639ef81a4379b6e893521ef`.
- Próxima condição: obter autoridade humana separada
  `AUTH-DR3-NUMERIC-SEMANTICS-IMPLEMENTATION-001` para implementar a decisão e
  as quatro correções verificáveis. Essa autoridade não inclui novo DR-3,
  dataset, campanha, provider, rede, OpenAPI, schema, migration, MultiQuery,
  Human Gate ou lifecycle.

## 2026-08-11 — Implementação da semântica numérica reconciliada e pendente de reteste

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo. `DR-3 — Determinism Automatic Quality Gate` permanece `REPROVADO`;
  nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado ou
  alterado por esta reconciliação.
- Autoridade e baseline da implementação:
  `AUTH-DR3-NUMERIC-SEMANTICS-IMPLEMENTATION-001`, branch `main`, commit inicial
  `9735ff5bc243d9a517b2cceb7ca8bfe16f24b438`, corpus `4.10.31`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas. O incremento focal foi
  concluído no commit `9addb166e82dd04581beee7b4276a74977fe04c5`,
  `fix(retrieval): implement versioned cosine semantics`, com dez arquivos de
  código/teste alterados, 701 inserções e 98 remoções.
- Autoridade e baseline desta reconciliação factual:
  `AUTH-DR3-NUMERIC-SEMANTICS-IMPLEMENTATION-RECONCILE-001`, branch `main`, HEAD
  `9addb166e82dd04581beee7b4276a74977fe04c5`, corpus `4.10.31`, working tree
  inicialmente limpa e identidades OpenAPI v1/v2 protegidas. O runtime
  preflight desta atividade documental foi `NÃO APLICÁVEL`.
- Semântica implementada: `SqliteVectorIndexStore` fixa cada multiplicação em
  binary32 por meio de local `float`, acumula serialmente em binary64 na ordem
  do índice e mantém raiz, produto das normas e divisão como operações binary64
  ordenadas. Quociente finito acima de `+1` é canonizado para `+1`, abaixo de
  `-1` para `-1`; bits internos, inclusive signed zero, permanecem inalterados.
  Estados não finitos, zero query e dados inválidos permanecem fail-closed;
  stored zero-vector permanece `+0`.
- Identidades implementadas: `retrieval-v2`, `RetrievalV2PolicyExecutor` e o
  descritor
  `sqlite-exact-vector-store/2;schema=1;distance=cosine;algorithm=exact-scan;vector=float32;score=cosine-f32mul-f64acc-boundary-canonical-v1`.
  O perfil interno produz novo `IndexCompatibilityKey`; geração ou chave `/1`
  incompatível retorna `GenerationUnavailable`, sem relabel, migração ou
  serving cruzado.
- `DR3-FIND-001 — P1`: limites positivos/negativos, valores imediatamente
  internos/externos, signed zero, scores internos, zero-vector, não finitos,
  overflow, compatibilidade, finalização e reopen possuem assertions bit a bit.
  O caso sintético `[1f, 1f, 1f]` termina com score exato `+1` e o fluxo
  end-to-end responde sob `retrieval-v2`. Disposição:
  `CORRECTED_PENDING_GATE_RETEST`.
- `DR3-FIND-002 — P2`: nove chunks disputam top-k `8`; o maior score pertence
  ao ordinal tardio `8`, o menor ao ordinal inicial `0`, empates exercitam o
  ordinal, e duas permutações de escrita, replay e reopen comparam ordem,
  identidades e bits dos scores. O sort total precede `Take(k)` na
  implementação. Disposição: `CORRECTED_PENDING_GATE_RETEST`.
- `DR3-FIND-003 — P2`: concorrentes inelegíveis com score maior cobrem
  geração/candidato, binding elegível, seletor de database e seletor de
  documento antes de score/top-k; raw hits, evidência selecionada e reopen
  preservam somente o alvo elegível. Disposição:
  `CORRECTED_PENDING_GATE_RETEST`.
- `DR3-FIND-004 — P2`: a fronteira Application rejeita
  `ChunkOrdinal = -1` como índice indisponível antes de qualquer chamada ao
  language model, e uma corrupção controlada em SQLite temporária task-owned
  retorna `InvalidIndexData` sem hits ou evidência. A validação normal de escrita
  também rejeita ordinal negativo. Disposição:
  `CORRECTED_PENDING_GATE_RETEST`.
- Evidência executável do turno de implementação: runtime preflight aplicável
  encontrou zero processo candidato do RAG-Challenge, encerrou zero e deixou
  zero; build Release terminou com zero avisos e zero erros; o teste local e
  offline da solução aprovou 202 testes unitários, 203 de integração e 11 de
  arquitetura — 416 no total, com zero falhas e zero skips. Esses resultados
  são evidência focal de implementação, não constituem Automatic Quality Gate
  e não foram reexecutados nesta reconciliação documental.
- Limites preservados: não houve epsilon, corredor de 1 ULP, aritmética escalada
  em binary64, FMA, reassociação, bucket ou semântica alternativa. Nenhuma
  geração de produto, dataset, scorer, campanha, provider, credencial, rede,
  chamada paga, corpus real, OpenAPI, schema, migration, MultiQuery, novo
  Automatic Quality Gate, Human Gate, lifecycle, push, publicação, deploy ou
  release foi criada, ativada, executada ou alterada. Fixtures foram sintéticas
  e stores foram temporários task-owned.
- Disposição do gate: os quatro achados têm correção implementada, mas não estão
  resolvidos. `DR-3` permanece `REPROVADO` até um reteste independente sob nova
  autoridade dispor explicitamente cada achado.
- Artefatos protegidos: OpenAPI v1 permaneceu no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; OpenAPI v2 permaneceu no
  SHA-256 `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` e blob
  `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`.
- Escopo documental fechado: somente ADR-0015, Current State, este histórico
  append-only no EOF e Prompt System Change Log foram alterados; nenhum código,
  teste, contrato ou configuração foi modificado nesta reconciliação.
- Verificação documental: `git diff --check` terminou com exit code `0`;
  `eng/check-repository.ps1` aprovou 280 arquivos não ignorados; o diff conteve
  somente os quatro documentos autorizados; UTF-8/LF, newline final, espaços
  finais, links, formato e prefixo append-only passaram. Build, testes
  executáveis e Automatic Quality Gate permaneceram `NOT_RUN` nesta
  reconciliação.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.31` para
  `4.10.32`. O histórico preservou byte a byte seu prefixo anterior de 471.585
  bytes no SHA-256
  `e5f092d7e942dcffa1954f7ac589a7efe64938c2fb58cbef86eae6efbdf011c5`.
- Próxima condição: obter autoridade humana separada e independente
  `AUTH-DR3-NUMERIC-SEMANTICS-AQG-RETEST-001` para repetir integralmente DR-3
  sobre baseline limpa e dispor `DR3-FIND-001` a `DR3-FIND-004`. A autoridade
  de reteste não inclui correção adicional, dataset, scorer, campanha,
  provider, rede, OpenAPI, schema, migration, MultiQuery, Human Gate ou
  lifecycle.

## 2026-08-11 — Reteste corretivo independente de DR-3 aprovado e reconciliado

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo; nenhum Human Gate ou lifecycle foi executado ou alterado. A reprovação
  inicial de DR-3 e as disposições intermediárias
  `CORRECTED_PENDING_GATE_RETEST` permanecem como evidência histórica.
- Autoridade e baseline do reteste:
  `AUTH-DR3-NUMERIC-SEMANTICS-AQG-RETEST-001`, branch `main`, commit
  `bf8a156e7c5eea801f29fb6e7742cac880783bc0`, corpus `4.10.32`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas.
- Autoridade e baseline desta reconciliação factual:
  `AUTH-DR3-NUMERIC-SEMANTICS-AQG-RETEST-RECONCILE-001`, branch `main`, HEAD
  `bf8a156e7c5eea801f29fb6e7742cac880783bc0`, corpus `4.10.32`, working tree
  inicialmente limpa e identidades OpenAPI v1/v2 protegidas. O runtime
  preflight desta atividade exclusivamente documental foi `NÃO APLICÁVEL`.
- Fonte factual e resultado: o hand-off único do revisor independente registra
  o reteste corretivo local, offline e determinístico como `APROVADO`, sem novo
  achado P0, P1, P2 ou P3. `DR3-FIND-001`, `DR3-FIND-002`, `DR3-FIND-003` e
  `DR3-FIND-004` estão `RESOLVED`.
- `DR3-FIND-001 — P1`: limites exatos positivos/negativos, valores adjacentes,
  signed zero, não finitos, overflow, stored zero-vector, reopen, o fluxo
  `[1f, 1f, 1f]` sob `retrieval-v2` e a incompatibilidade fail-closed com
  descritor/chave `/1` passaram. Disposição final: `RESOLVED`.
- `DR3-FIND-002 — P2`: o adversário de nove chunks produziu a ordem
  `8, 7, 6, 5, 3, 4, 2, 1`; duas permutações de escrita, replay, reopen e três
  execuções independentes preservaram ordem, identidades e bits antes de
  `Take(k)`. Disposição final: `RESOLVED`.
- `DR3-FIND-003 — P2`: geração/candidato, binding elegível, seletor de database
  e seletor de documento filtraram concorrentes inelegíveis de score superior
  antes de score/top-k, inclusive após reopen. Disposição final: `RESOLVED`.
- `DR3-FIND-004 — P2`: a fronteira Application rejeitou `ChunkOrdinal = -1`
  antes de qualquer chamada ao language model; a corrupção controlada na
  SQLite temporária task-owned retornou `InvalidIndexData`, sem hit ou evidência
  selecionada. Disposição final: `RESOLVED`.
- Evidência focal do reteste: build Release com zero avisos e zero erros; 75
  testes Application/query; 12 testes SQLite, end-to-end e indexing; e duas
  repetições frias adicionais de 10 testes da matriz SQLite, todas aprovadas.
  As três execuções independentes da matriz registraram `10,136 s`, `7,539 s`
  e `7,284 s`.
- Evidência integral do reteste: a solução aprovou 202 testes unitários, 203 de
  integração e 11 de arquitetura — 416 no total, sem falha ou skip. O entry
  point canônico `eng/ci.ps1 -Offline` passou integralmente, incluindo mais 45
  testes do Dashboard, lint, typecheck, build web, cobertura de 95,53% das
  linhas e 68,47% dos branches e auditoria de 280 arquivos não ignorados. As
  versões registradas foram .NET SDK `10.0.302`, Node `24.19.0`, npm `11.17.0`
  e PowerShell `7.6.4`.
- Runtime do reteste: preflight e postflight encontraram zero processo ou
  listener pertencente ao RAG-Challenge, encerraram zero e deixaram zero. O
  gate começou e terminou na mesma baseline limpa e não alterou arquivo
  rastreado.
- Limite da evidência e escopo negativo: Windows x64 local, offline,
  determinístico e sintético, com fixtures e stores temporários task-owned.
  Nenhuma geração de produto, dataset, scorer, campanha, provider, credencial,
  rede, chamada paga, corpus real, OpenAPI, schema, migration, dependência,
  lockfile, MultiQuery, Human Gate, lifecycle, push, publicação, deploy ou
  release foi criada, ativada, executada ou alterada.
- Artefatos protegidos: OpenAPI v1 permaneceu no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; OpenAPI v2 permaneceu no
  SHA-256 `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` e blob
  `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`.
- Escopo documental fechado: somente ADR-0014, ADR-0015, Current State, este
  histórico append-only no EOF e Prompt System Change Log foram alterados;
  nenhum código, teste, contrato ou configuração foi modificado nesta
  reconciliação.
- Verificação documental: `git diff --check` e
  `eng/check-repository.ps1` passaram; somente os cinco documentos autorizados
  mudaram; UTF-8/LF, newline final, espaços finais, links, formato e prefixo
  append-only passaram. Build, testes executáveis e Automatic Quality Gate
  permaneceram `NOT_RUN` nesta reconciliação.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.32` para
  `4.10.33`. Este histórico preservou byte a byte seu prefixo anterior de
  478.019 bytes no SHA-256
  `8ec2d5dd7b4778966e5087f880a70055405173db986714c97349ce5f40099643`.
- Próxima condição: obter autoridade humana separada para
  `RB-1 — Evaluation design freeze`, limitado a um contrato de desenho
  imutável, não materializado e não pontuado, com zero documento/caso de
  produto, qrel, vetor de consulta, geração ou resultado. Dataset de produto,
  scorer executado, campanha, provider, rede, OpenAPI, MultiQuery, Human Gate
  e lifecycle permanecem posteriores e independentes.

## 2026-08-12 — RB-1 evaluation design freeze concluído documentalmente

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo. Nenhum Human Gate ou lifecycle foi executado ou alterado.
- Autoridade e baseline: `AUTH-RB1-EVALUATION-DESIGN-FREEZE-001`, branch
  `main`, HEAD `45cbcf2624262572abf8180498ac63709a9130e4`, corpus `4.10.33` e working
  tree inicialmente limpa. O runtime preflight desta atividade exclusivamente
  documental foi `NÃO APLICÁVEL`; nenhum processo ou listener foi enumerado ou
  encerrado.
- Artefatos protegidos antes da primeira escrita: OpenAPI v1 permaneceu no
  SHA-256 `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; OpenAPI v2 permaneceu no
  SHA-256 `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` e blob
  `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`.
- Resultado: `RB-1 — Evaluation design freeze` está concluído somente como
  freeze documental. A revisão imutável
  `retrieval-v2-evaluation-design-v1`, em estado
  `frozen-unmaterialised-unscored`, contém exatamente 28 artefatos normativos
  em `docs/evaluation/retrieval-v2-evaluation-design-v1/`: oito instâncias de
  desenho e 20 schemas JSON Draft 2020-12. O contrato-raiz vincula os outros 27
  artefatos por path ordinal, tipo e SHA-256 e possui self-digest
  `0e8d928aee055211773d83eb33f2d54485033c81cfad15dd95b0fdd551f8ed08`.
- Identidades congeladas: `retrieval-v2@frozen-query-vector`, `retrieval-v2`,
  `cosine-f32mul-f64acc-boundary-canonical-v1`, descritor integral
  `sqlite-exact-vector-store/2;schema=1;distance=cosine;algorithm=exact-scan;vector=float32;score=cosine-f32mul-f64acc-boundary-canonical-v1`,
  `retrieval-evaluation-scorer-v1`,
  `retrieval-result-canonicalisation-v1`,
  `retrieval-question-normalisation-v1`, `Score DESC, global ChunkOrdinal ASC`,
  top-k `8`, mínimo `0.25` inclusivo, máximo de seis evidências e budget
  agregado de 16.000 escalares. Um vetor futuro por caso permanece exclusivo
  de RB-3; MultiQuery continua `parked-non-canonical`.
- Matrizes e ausência de materialização: 38 cellIds contratuais únicos e 10
  eligibility cellIds únicos foram congelados somente como condições e
  assertions. Os contadores de documento/caso de produto, qrel, vetor de
  consulta, geração, run pontuado e resultado observado permanecem todos em
  zero. Nenhum documento, caso, pergunta, qrel, vetor, geração, resultado ou
  métrica observada foi materializado.
- Conteúdo governado: normalização de perguntas, canonicalização bit a bit,
  fórmulas, denominadores, thresholds, diagnósticos, quotas, rubric,
  versionamento, retenção, paths futuros, sequência de gates, stop conditions e
  escopo negativo foram congelados. Os schemas futuros não materializam suas
  instâncias nem concedem autoridade para os gates correspondentes.
- Validação estática: os 28 JSON fizeram parse; `JsonSchema.Net 7.0.0.0`
  validou os 20 schemas contra o metaschema Draft 2020-12 local e as oito
  instâncias contra seus schemas aplicáveis. Objetos fechados, ausência de
  `examples`/`default`, inventário de 27 companions, digests, dois recálculos
  idênticos do self-digest, cardinalidade/identidade das matrizes e os sete
  contadores zero passaram. O namespace histórico
  `docs/evaluation/rag-eval-catalogue-v1` permaneceu inalterado.
- Escopo documental fechado: foram criados somente os 28 caminhos novos e
  alterados somente ADR-0014, relatório STATE-07, Current State, este histórico
  por acréscimo no EOF e Prompt System Change Log. Nenhum código, teste,
  OpenAPI, contrato público, schema de produto, migration, dependência,
  lockfile ou revisão sintética histórica foi alterado.
- Limite de execução: build, testes executáveis, `eng/ci.ps1`, scorer,
  campanha, provider, credencial, rede, chamada paga, Automatic Quality Gate,
  Human Gate, lifecycle, push, publicação, release e deploy permaneceram
  `NOT_RUN`.
- Verificação documental: `git diff --check` e
  `eng/check-repository.ps1` passaram; o diff contém exatamente os 33 paths
  autorizados; UTF-8/LF, newline final, espaços finais, links, formato,
  inventário, digests e prefixo append-only passaram; OpenAPI v1/v2
  permaneceram byte a byte protegidas.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.33` para
  `4.10.34`. Este histórico preservou byte a byte seu prefixo anterior de
  483.419 bytes no SHA-256
  `aa0ff7a6829b9f4df704569471bc58df806aa5d69bf3d483a8ce797e31887740`.
- Próxima condição: obter autoridade humana separada para
  `RB-2 — Dataset materialisation readiness`, condicionada a corpus de produto
  autorizado, direitos verificados, geração ativa validada, pooling e
  adjudicação não pontuados completos, qrels, matrizes requeridas completas,
  denominador exato e tier declarado. RB-2, RB-3, RB-4, RB-5, scorer executado,
  campanha, provider, rede, OpenAPI, MultiQuery, Human Gate e lifecycle
  permanecem independentes, `NOT_RUN` e não autorizados.

## 2026-08-12 — Automatic Quality Gate notice-bearing aprovado e reconciliado

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo. Nenhum Human Gate ou lifecycle foi executado ou alterado.
- Autoridade e baseline do reteste:
  `AUTH-S07-A-NOTICE-BEARING-PROFILE-AQG-RETEST-001`, branch `main`, commit
  `8327f5070d0646a845da821a92a2286203aef797`, corpus `4.10.34`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas.
- Autoridade e baseline desta reconciliação factual:
  `AUTH-S07-A-NOTICE-BEARING-PROFILE-AQG-RECONCILE-001`, branch `main`, HEAD
  `8327f5070d0646a845da821a92a2286203aef797`, corpus `4.10.34`, working tree
  inicialmente limpa e identidades OpenAPI v1/v2 protegidas. O runtime
  preflight desta atividade exclusivamente documental foi `NÃO APLICÁVEL`;
  nenhum processo ou listener foi enumerado ou encerrado.
- Resultado: o Automatic Quality Gate de `DerivativeObligationSetV1`,
  `pdf-page-png-notice-v1`, preservação pixel a pixel, manifest, persistence,
  reachability, ativação, serving v2 fail-closed incluindo `304`,
  compatibilidade legada/v1 e Dashboard acessível está `APROVADO`, sem achado
  P0, P1, P2 ou P3.
- Evidência focal: `eng/check-repository.ps1` aprovou 308 arquivos não
  ignorados; o build Release terminou com zero aviso e zero erro; dois testes
  unitários, dez de integração, um de arquitetura e 45 do Dashboard passaram,
  assim como lint, typecheck e build web.
- Evidência integral: `eng/ci.ps1 -Offline` aprovou 202 testes unitários, 203
  de integração e 11 de arquitetura, sem falha ou skip, e repetiu os 45 testes
  do Dashboard, lint, typecheck, build web e auditoria dos 308 arquivos. A
  cobertura combinada foi 95,53% de linhas e 68,47% de branches.
- Runtime do reteste: o preflight dirigido encontrou zero processo e zero
  listener pertencente ao RAG-Challenge e encerrou zero. A barreira pré-CI e o
  postflight encontraram zero processo do projeto. O gate começou e terminou
  no mesmo HEAD, com árvore rastreada limpa.
- Limite da aprovação e escopo negativo: Windows local, offline, determinístico
  e sintético. O resultado não reclassifica o A0 anterior e não autoriza novo
  A0, corpus ou dado de produto, RB-2, provider, rede, Human Gate, lifecycle,
  push, publicação ou deploy. As quatro operações visuais permanecem
  `UNPROVEN` até avaliação candidato-específica separadamente autorizada.
- Artefatos protegidos: OpenAPI v1 permaneceu no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; OpenAPI v2 permaneceu no
  SHA-256 `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` e blob
  `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`.
- Escopo documental fechado: somente relatório STATE-07, Current State, este
  histórico append-only no EOF e Prompt System Change Log foram alterados;
  nenhum código, teste, contrato ou configuração foi modificado nesta
  reconciliação.
- Verificação documental: `git diff --check` terminou com exit code `0` e
  `eng/check-repository.ps1` aprovou 308 arquivos não ignorados; o diff conteve
  somente os quatro documentos autorizados, UTF-8/LF, newline final, espaços
  finais, links, formato e prefixo append-only passaram. Build, testes
  executáveis, `eng/ci.ps1` e Automatic Quality Gate não foram repetidos nesta
  reconciliação.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.34` para
  `4.10.35`. Este histórico preservou byte a byte seu prefixo anterior de
  488.490 bytes no SHA-256
  `e667ef60f9e6f05e916fa80114efdaeeb05e5e5ec4030bb2bffeaf49e8e9cd2f`.
- Próxima condição diretamente relacionada: obter autoridade humana separada
  para um novo A0 candidato-específico que reavalie page rendering,
  derivative-image creation, derivative-image retention e runtime derivative
  display com o mecanismo aprovado. O resultado do gate não predetermina a
  disposição desse A0; RB-2, Human Gate e lifecycle permanecem independentes
  e não autorizados.

## 2026-08-12 — A0-003 remove bloqueio de direitos do candidato PostgreSQL

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo. Nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado nesta atividade.
- Autoridade e baseline: `AUTH-S07-A-PRODUCT-A0-003`, branch `main`, commit
  `f5bea053e12b189c472559142107331ad3b2e9d9`, corpus `4.10.35`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas. O runtime preflight desta
  atividade documental foi `NÃO APLICÁVEL`; nenhum processo ou listener foi
  enumerado ou encerrado.
- Identidade revalidada: o candidato ignorado
  `postgresql-18-reference-a4` permaneceu arquivo regular, sem reparse point,
  com 15.771.040 bytes e SHA-256
  `cea7b845568095eb56dee1b51bfa145c6c6637bc4377c986019971577efefae4`.
- Evidência e limite de inferência: somente a PostgreSQL Licence e o notice já
  registrados, ADR-0011, ADR-0012 e o gate notice-bearing aprovado foram
  usados. A concessão cobre uso, cópia, modificação e distribuição e exige
  copyright notice, permission paragraph e dois disclaimers completos em todas
  as cópias. Nenhuma suficiência apenas contextual foi inferida.
- Page rendering: `PERMITTED` somente por `pdf-page-png-notice-v1`, com source
  pixels intactos, avisos completos dentro do PNG, change marking e falha
  fechada em ausência, mismatch, truncation ou limite. O perfil legado é
  inelegível para este candidato.
- Derivative-image creation: `PERMITTED` somente com
  `DerivativeObligationSetV1` exato, imutável e vinculado por identidade/digest
  a source, mapping revision, manifest e composite bytes.
- Derivative-image retention: `PERMITTED` somente no content store governado,
  com lineage, retention, reachability e backup/cold restore verificados; Git,
  Git LFS, intake e browser cache não são autoridades.
- `RuntimeDerivativeImageDisplay`: `PERMITTED` somente para o PNG ativo e
  citation-bound na rota same-origin, com revalidação de direitos, mapping,
  obligation, manifest, hash, tamanho e dimensões antes de `200` ou `304` e
  apresentação acessível do conteúdo completo.
- `SourceAndDerivativeByteDistributionOrPublication`: permanece `DENIED` fora
  da fronteira runtime-display. Download, public/static hosting, CORS
  permissivo, CDN, bulk export, bundles, Git/Git LFS e republicação continuam
  excluídos por política de produto, não por proibição atribuída ao publisher.
- Disposição: o candidato deixa de estar `BLOCKED/EXCLUDED` por essas cinco
  decisões e permanece `ELIGIBLE_CANDIDATE`. Não está materializado, indexado,
  ativo ou `READY_FOR_PRODUCT_ACTIVATION`; qualquer drift, conflito, texto
  incompleto, mismatch ou ampliação de fronteira volta a falhar fechado.
- Escopo negativo: nenhum import, parse, render, obligation set, manifest,
  dataset, qrel, vetor, geração, indexação, ativação, query, teste executável,
  `eng/ci.ps1`, RB-2, provider, rede, Human Gate, lifecycle, push, publicação
  ou deploy foi criado, executado ou alterado.
- Artefatos protegidos: OpenAPI v1 permaneceu no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; OpenAPI v2 permaneceu no
  SHA-256 `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` e blob
  `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`.
- Escopo documental fechado: somente o Document Eligibility Register, Current
  State, este histórico append-only no EOF e Prompt System Change Log foram
  alterados; nenhum código, contrato, schema ou configuração foi modificado.
- Verificação documental: `git diff --check` terminou com exit code `0` e
  `eng/check-repository.ps1` aprovou 308 arquivos não ignorados; o diff conteve
  somente os quatro documentos autorizados, UTF-8/LF, newline final, espaços
  finais, links, formato e prefixo append-only passaram. Build, testes
  executáveis e `eng/ci.ps1` permaneceram `NOT_RUN`.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.35` para
  `4.10.36`. Este histórico preservou byte a byte seu prefixo anterior de
  492.561 bytes no SHA-256
  `a2a75e74e6ca9044071235e41a404d2c23517d5adec79a984c0694d1c6955530`.
- Próxima condição: obter autoridade humana separada para
  `RB-2 — Dataset materialisation readiness`. O A0 satisfaz somente a
  disposição de direitos; corpus de produto autorizado, geração ativa
  validada, pooling/adjudicação não pontuados, qrels, matrizes, denominador e
  tier permanecem requisitos independentes e não materializados.

## 2026-08-12 — Reteste da composição administrativa de produto aprovado e reconciliado

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo. Nenhum Human Gate ou lifecycle foi executado ou alterado.
- Autoridade e baseline do reteste:
  `AUTH-S07-A-PRODUCT-ADMIN-COMPOSITION-AQG-RETEST-001`, branch `main`, commit
  `e63f061d0bce4e48cd3b32294c20e29727cd7156`, corpus `4.10.36`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas.
- Autoridade e baseline desta reconciliação factual:
  `AUTH-S07-A-PRODUCT-ADMIN-COMPOSITION-AQG-RETEST-RECONCILE-001`, branch
  `main`, HEAD `e63f061d0bce4e48cd3b32294c20e29727cd7156`, corpus `4.10.36`, working
  tree inicialmente limpa e identidades OpenAPI v1/v2 protegidas. O runtime
  preflight desta atividade exclusivamente documental foi `NÃO APLICÁVEL`;
  nenhum processo ou listener foi enumerado ou encerrado.
- Escopo auditado: composição local, offline, determinística e exclusivamente
  sintética dos comandos one-shot `synchronise-official` e `build-index` pelo
  caminho efetivamente chamado por `Program`, incluindo ausência e divergência
  fail-closed, direitos, parsing PDF/CSV, chunking determinístico, content
  store verificado, transporte e embedding falsos fornecidos somente por
  ports, `IndexCompatibilityKey`, geração candidata, lease, journal e replay.
- Inspeção estática: `Program` encaminha administração a
  `OneShotAdministrationHost.RunProductionAsync`; o host resolve somente o
  perfil sintético explícito no ambiente `Integration` e chama
  `AdministrativeMaterialisationComposition.CreateExecutor`. Capacidade
  ausente, par de ports incompleto ou drift de perfil falha fechado, sem
  seleção implícita de transporte ou provider real.
- Seleção focal: a forma anterior do filtro do auditor selecionou zero testes
  e não foi aceita como evidência. O reteste usou seis nomes totalmente
  qualificados, contador TRX explícito e fail-on-zero; executou exatamente
  6/6 testes, todos aprovados, sem falha, skip ou caso não executado. Os casos
  cobriram pares incompletos, ausência de perfil, ambiente divergente,
  composição de indexação e drift de compatibilidade, sincronização oficial e
  drift de autoridade, além do lease dedicado. Os casos de `Program`
  iniciaram o host de produção copiado pelos testes.
- Evidência integral: somente depois do resultado focal positivo,
  `eng/ci.ps1 -Offline` foi executado exatamente uma vez e terminou com exit
  code `0`. O build Release teve zero aviso e zero erro; passaram 202 testes
  unitários, 208 de integração, 11 de arquitetura e 45 do Dashboard, sem falha
  ou skip. A cobertura combinada foi 95,58% de linhas e 68,07% de branches;
  lint, typecheck, build web e auditoria de 311 arquivos também passaram.
- Incidentes de localização read-only: duas tentativas preliminares usaram
  paths presumidos incorretos para ADRs e OpenAPI. O inventário rastreado
  resolveu os paths canônicos antes da confirmação da baseline e dos checks do
  gate. Não houve escrita, execução de produto, repetição da seleção focal ou
  segunda CI; esses eventos não são achados de produto.
- Resultado: o Automatic Quality Gate está `APROVADO`, sem achado P0, P1, P2
  ou P3, exclusivamente na fronteira Windows local, offline, determinística e
  sintética. Stores temporários task-owned e outputs ignorados de teste não
  constituem materialização de produto.
- Escopo negativo preservado: nenhuma materialização ou ativação de produto,
  dataset, RB-2, provider, rede, Human Gate, lifecycle, push, publicação ou
  deploy foi criado, executado ou alterado.
- Artefatos protegidos: OpenAPI v1 permaneceu no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; OpenAPI v2 permaneceu no
  SHA-256 `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` e blob
  `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`.
- Escopo documental fechado: somente relatório STATE-07, Current State, este
  histórico append-only no EOF e Prompt System Change Log foram alterados;
  nenhum código, teste, contrato, configuração ou OpenAPI foi modificado.
- Verificação documental: `git diff --check` terminou com exit code `0` e
  `eng/check-repository.ps1` aprovou 311 arquivos não ignorados; o diff conteve
  somente os quatro documentos autorizados, UTF-8/LF, newline final, espaços
  finais, links, formato e prefixo append-only passaram. Build, testes
  executáveis, `eng/ci.ps1` e Automatic Quality Gate não foram repetidos nesta
  reconciliação.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.36` para
  `4.10.37`. Este histórico preservou byte a byte seu prefixo anterior de
  497.149 bytes no SHA-256
  `682626fe33eeef5e1df9ea8313e45c578712aeeb34fc66ca1f7450c8195b8fd7`.
- Próxima condição diretamente relacionada: obter autoridade humana separada
  e delimitada para materializar, sem ativação implícita, a sincronização
  oficial e a geração candidata do produto pelos dois comandos agora
  auditados. Este gate não concede essa autoridade; ativação, RB-2, Human Gate
  e lifecycle permanecem separados e não autorizados.

## 2026-08-12 — Disposição project-owned notice-bearing aprovada e reconciliada

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo. Nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado.
- Autoridade e baseline:
  `AUTH-S07-A-PRODUCT-ADMIN-NOTICE-BEARING-PROJECT-OWNED-DISPOSITION-RECONCILE-001`,
  branch `main`, HEAD `0de07426f37c4427f419c0523e5342aac476c0f6`, corpus
  `4.10.37`, working tree inicialmente limpa e identidades OpenAPI v1/v2
  protegidas. O runtime preflight desta atividade exclusivamente documental
  foi `NÃO APLICÁVEL`; nenhum processo ou listener foi enumerado ou encerrado.
- Decisão do proprietário: a proposta read-only sob
  `AUTH-S07-A-PRODUCT-ADMIN-NOTICE-BEARING-PROJECT-OWNED-DISPOSITION-PROPOSAL-001`
  foi aprovada com os seis valores exatos abaixo:

  ```json
  {
    "attributionText": "Source: The PostgreSQL Global Development Group; document: PostgreSQL 18.4 Documentation; version: 18.4; source reference: https://www.postgresql.org/files/documentation/pdf/18/postgresql-18-A4.pdf",
    "trademarkTreatment": "Required",
    "trademarkOrNonEndorsementText": "Do not imply PostgreSQL project endorsement. No trademark permission is inferred from the documentation licence.",
    "changeMarkingText": "The composite PNG is a marked derivative, not a claim that its complete canvas is an unmodified publisher page. The source-page region nevertheless remains pixel-identical visual evidence.",
    "assessedAt": "2026-08-12T04:05:14.0000000+00:00",
    "assessorId": "assessor:auth-s07-a-product-a0-003"
  }
  ```

- Proveniência semântica: o pacote é disposição de controle pertencente ao
  projeto. Não substitui nem altera copyright, permissão ou disclaimers da
  evidência primária, não concede trademark permission e não constitui nova
  conclusão jurídica. `trademarkTreatment=Required` exige o texto exato de
  não endosso; `assessedAt` adota o instante UTC do commit documental A0-003;
  `assessorId` identifica a avaliação pela autoridade governada sem dado
  pessoal.
- Escopo fechado: somente o Document Eligibility Register, Current State,
  este histórico append-only no EOF e Prompt System Change Log foram
  alterados. O corpus avança por `PATCH` factual de `4.10.37` para `4.10.38`.
- Escopo negativo preservado: nenhum secret ou credencial foi lido; nenhuma
  rede, cálculo de `rightsMappingRevision`, `obligationSetId` ou
  `canonicalSha256`, alteração do bundle ignorado, teste, build,
  materialização, renderização, embedding, indexação, ativação, AQG, Human
  Gate, lifecycle, publicação, push ou deploy foi executado ou autorizado.
- Integridade append-only: este histórico preserva byte a byte seu prefixo
  anterior de 502.405 bytes no SHA-256
  `d2d63b9bdd2847597dfbd02774b0303b7f1273b5cc486c2192133a529dabee93`.

## 2026-08-13 — Contrato text-first reconciliado com a geração PostgreSQL preparada

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo. Nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado.
- Autoridade e baseline:
  `AUTH-S07-LOCAL-PRODUCT-TEXT-FIRST-CONTRACT-RECONCILIATION-PREFLIGHT-001`,
  branch `main`, HEAD `87f191c733715198451ee63da21ef24e121b0ac8`, corpus
  `4.10.38`, working tree rastreada inicialmente limpa e OpenAPI v1/v2
  protegidas. O runtime preflight desta atividade documental foi
  `NÃO APLICÁVEL`; nenhum processo ou listener foi enumerado ou encerrado.
- Divergência reconciliada: o contrato ativo ainda impunha renderização completa
  de todas as páginas antes de qualquer ativação PDF, embora o pipeline
  implementado já aceite `renderManifestId=null` sob `TextualEvidence` e
  preserve o modo visual completo sob `PdfVisualEvidence`.
- Contrato resultante: PDF text-first pode ser ativado sem pré-renderização; uma
  resposta v2 fundamentada pode materializar sob demanda somente as páginas
  físicas efetivamente citadas, entre uma e cinco por resposta. O manifesto
  esparso pertence exclusivamente ao `AnswerEvidenceRecordV1` persistido, nunca
  ao binding de ativação. Falha visual preserva a resposta textual e não cria
  referência de imagem.
- Estado factual de produto: a geração PostgreSQL 18.4
  `idxgen-ec39244b021c90fceea1b3a628fe793a99f74650cad451f16ffbcd414af636f6`
  contém 3.282 chunks e 3.282 vetores validados, com
  `logicalArtifactDigest=abba9604bf19bae0349e0d72e7973a386371458eacffac66eeb803a9dabf30fe`,
  `generationContentDigest=ec39244b021c90fceea1b3a628fe793a99f74650cad451f16ffbcd414af636f6`
  e zero render manifest. A entrada `07-activate-generation.runtime.json` está
  preparada com um binding LocalAuthorised e `renderManifestId=null`;
  `activate-generation` não foi executado.
- Escopo documental fechado: somente o contrato RAG, Current State, este
  histórico append-only, Prompt System Change Log e o relatório factual de
  `STATE-07` são alterados no commit focal. Corpus elevado por `PATCH` factual
  de `4.10.38` para `4.10.39`.
- Artefatos locais ignorados: somente autoridade, corpus e baseline do bundle,
  handoff e marcador do store-alvo serão alinhados ao HEAD resultante. O
  preflight posterior desse HEAD é read-only e reportado sanitizadamente fora
  do commit único; ele não altera a geração nem concede autoridade de ativação.
- Escopo negativo preservado: nenhum código, OpenAPI, PDF, SQLite, vetor,
  content object, store protegido ou outro store é alterado. Build, restore,
  testes, credencial, rede, renderização, embeddings, Responses, consulta,
  `activate-generation`, publicação, push e deploy permanecem `NOT_RUN`.
- Integridade append-only: este histórico preserva byte a byte seu prefixo
  anterior de 505.282 bytes no SHA-256
  `a6706a49bb1f2c9f5267e0d7ba133ff497149701ab7292ffacb36faa09373f4f`.
- Próxima condição diretamente relacionada: obter autoridade humana separada e
  delimitada para executar exatamente uma vez `activate-generation` somente
  após o preflight post-commit confirmar integralmente geração, input, store e
  stores protegidos. Consulta e qualquer etapa posterior permanecem separadas.

## 2026-08-14 — Ativação PostgreSQL reconciliada e pacote Render Free preparado localmente

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo. Nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado.
- Ativação persistida: sob
  `AUTH-S07-LOCAL-PRODUCT-TEXT-FIRST-ACTIVATE-GENERATION-001`, sobre
  `main@4123413c49b3c1bdd587dce9380f44136c79c7f5`, corpus `4.10.39`, a operação
  única `s07-postgresql-18-a4-local-v1-07-activate-generation` retornou
  `CH_ADMIN_APPLIED` e aplicou a revisão ativa `1` para a geração
  `idxgen-ec39244b021c90fceea1b3a628fe793a99f74650cad451f16ffbcd414af636f6`.
  O `activationBindingSetDigest` persistido é
  `234a1ff2e0e84c07574878ebd67b7015e4775a0256876d9cfc18f9adb0a6e2d6`;
  permanecem 3.282 chunks, 3.282 vetores, um binding `LocalAuthorised` e
  `renderManifestId=null`. Nenhuma consulta ou Responses foi executada.
- Autoridade de pacote: em 2026-08-14 o proprietário autorizou preparar
  localmente o pacote RAG-Challenge para Render Hobby com instância Free,
  custo de hospedagem zero e bloqueio de recurso pago, proibindo publicação ou
  criação de serviço externo. A autorização não acessa billing e não concede
  deploy, registry, GitHub, secret remoto ou provider.
- Implementação: o commit focal
  `d72914ba79dcb482a2e5d2f1ce9cc8a812315c2b` cria Dockerfile fixado por digest,
  entrypoint fail-closed, template Render não implantável, builder, verificador,
  documentação e cinco testes focais. `plan: free`, uma instância, ausência de
  disco/banco e auto-deploy desligado são verificações obrigatórias.
- Persistência deliberada: o seed operacional de 83.539.360 bytes permanece
  somente em `artifacts-local/` e em imagem privada local. Cada boot verifica o
  seed, cria um store gravável novo em `/tmp`, verifica a cópia e inicia como
  usuário não privilegiado. Reinício restaura a geração ativa e descarta
  `AnswerEvidenceRecordV1` produzidos desde o boot; isso é demonstração
  efêmera, não persistência de produção.
- Evidência local: build Dashboard, publish .NET sem restore, readiness
  loopback, auditoria do contexto e cinco testes focais passaram. O contexto
  contém 44 arquivos, release de 35.689.002 bytes e três arquivos operacionais
  de seed totalizando 83.539.360 bytes. A imagem Linux x64 local
  `6f676930e5051e60d89aafd817d34763dcfc82e7877a129a6cc0c9bf9f049000`
  respondeu Dashboard HTTP `200`, readiness `Ready`, preservou a geração ativa
  após restart e removeu o probe transitório do store.
- Diagnóstico do harness: a primeira prova de restart não releu uma porta
  aleatória atribuída pelo Docker. Logs sanitizados mostraram o contêiner
  reiniciado e escutando. A repetição com porta loopback explicitamente
  reservada passou; nenhum código de produto foi alterado por esse falso
  negativo.
- Escopo negativo observado: nenhum segredo real foi lido; nenhuma chamada de
  provider, Responses ou produto foi executada; nenhum recurso Render, OCI ou
  GitHub foi criado; nenhuma imagem foi publicada; billing, PDF e stores
  protegidos permaneceram inalterados. O Docker Desktop iniciado pela tarefa
  foi encerrado e não restou contêiner da tarefa.
- Arquitetura e entrega: o candidato Render Free não substitui ADR-0005 nem o
  requisito OCI registrado pelos materiais do Challenge. Seleção final de
  Render, publicação privada da imagem, credential de registry, secret de
  provider e criação do serviço exigem reconciliação e autoridade externas
  posteriores.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.39` para
  `4.10.40`, sem alterar autoridade, OpenAPI ou lifecycle.

## 2026-08-14 — Imagem privada publicada e serviço Render Free implantado

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo. Nenhum Automatic Quality Gate, Human Gate ou avanço para `STATE-08`
  foi executado.
- Autoridades externas: o proprietário autorizou separadamente a publicação
  privada única da imagem no GHCR, a remoção posterior da credencial local e a
  implantação única de um Render Web Service Free. A autoridade exigiu digest
  imutável, uma instância, zero disco persistente, zero banco Render,
  autoscaling desligado, nenhum recurso pago, nenhuma alteração de billing e
  auto-deploy desativado.
- Publicação privada: a imagem foi publicada como
  `ghcr.io/degsterin/rag-challenge@sha256:536e431126470a51370bf9aeb4c769ff1d75313c67643c3922cf0fd2e2688c08`.
  O pacote GHCR permaneceu `Private`, com budget observado `USD 0` e stop usage
  habilitado. A credencial GHCR local foi removida após a verificação.
- Preflight Render: a baseline era
  `main@bf26d0703b421ffcdc48facf6d6d21ba1669e2a3`, corpus `4.10.40` e árvore
  rastreada limpa. Os hashes do package manifest e context manifest eram,
  respectivamente,
  `d46af8e33605dd8395d6207a1587cc36b44b57cd0b0f5618caf5e6e944b19778` e
  `83c48eb8bebb52bb6bee3e8a52837826437c02b6166c20190bd1bc065fe3d8ed`.
  Nenhum processo ou listener RAG-Challenge-owned conflitante foi encontrado.
- Serviço criado: `rag-challenge`, ID `srv-d9v9gju417fc73cf69i0`, no workspace
  Hobby, plano `Free`, região Frankfurt, uma instância, autoscaling `Off`, zero
  disco persistente e zero banco Render. O formulário confirmou todos os
  tipos pagos desmarcados, health path `/api/v1/health/ready`, nenhum Docker
  command override, secret file ou pre-deploy command. A fonte por digest
  imutável não expõe automatic deployment configurável.
- Segurança de credenciais: o proprietário inseriu diretamente na interface
  segura do Render a credential GHCR com `read:packages` e o valor de
  `OPENAI_API_KEY`. Nenhum valor foi lido, exibido, registrado ou incorporado
  ao repositório, à imagem ou às evidências.
- Deploy e validação: `dep-d9v9gke417fc73cf6br0` terminou
  `Deploy succeeded | Live` em 46,8 segundos. A URL pública é
  <https://rag-challenge-ac09.onrender.com>. `GET /api/v1/health/live` retornou
  HTTP `200` e `Live`; `GET /api/v1/health/ready` retornou HTTP `200`, `Ready`,
  um banco ativo, um documento elegível, zero degradado, revisão
  `postgresql-18.4-product-v1` e geração
  `idxgen-ec39244b021c90fceea1b3a628fe793a99f74650cad451f16ffbcd414af636f6`.
- Custo e escopo negativo: a leitura final de billing mostrou nenhum cartão,
  serviços `USD 0.00`, total mensal `USD 0.00` e projeção `USD 0.00`. Nenhuma
  consulta de produto, Responses, embedding, alteração de billing, recurso
  pago, segundo deploy, banco, disco, autoscaling ou automatic deployment foi
  executado.
- Pacote documental: o proprietário autorizou README detalhado, imagem e GIF
  sanitizados sem alterar o serviço. A captura Render possui 58.894 bytes e
  SHA-256
  `703bdcec24a9b4dce33edd4182d2cbeff35ba5fb406f978db9ccd83d7187056c`;
  o GIF de quatro quadros possui 155.188 bytes e SHA-256
  `9fd244cb859d99da420ce5540ac0bba56cc254ea17db24ae772898705dc662b4`.
  Os ativos não contêm segredo, conteúdo documental, prompts, respostas,
  chunks ou vetores.
- Limite arquitetural: a implantação é evidência pública de homologação em
  `STATE-07`, com filesystem efêmero e cold start possível após inatividade.
  Ela não substitui ADR-0005, não satisfaz sozinha o requisito OCI e não
  constitui homologação de produção.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.40` para
  `4.10.41`, sem alterar OpenAPI, código ou lifecycle.

## 2026-08-14 — Freezes RB-2 e RB-3 concluídos e reconciliados sem pontuação

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo. Nenhum Automatic Quality Gate, Human Gate ou lifecycle foi executado
  ou alterado.
- Autoridade e baseline desta reconciliação factual:
  `AUTH-S07-RB3-CAMPAIGN-INPUT-FREEZE-001`, branch `main`, HEAD
  `0dbc415bad6532842aa6c4d1bb45ecd915bf5022`, corpus `4.10.41`, árvore
  rastreada inicialmente limpa e OpenAPI v1/v2 protegidas. O runtime preflight
  desta atividade exclusivamente documental foi `NÃO APLICÁVEL`; nenhum
  processo ou listener foi enumerado ou encerrado.
- RB-2 congelado: a revisão
  `rag-eval-catalogue-v1-postgresql-18-rb2-20260814-001`, concluída sob
  `AUTH-S07-RB2-REVIEW-ADJUDICATION-FREEZE-PACKAGE-001`, possui
  materialisation-freeze manifest SHA-256
  `daede1db869f7daf784fa2f3fc3b55e037cf4f3bb59a22f94e2026175858bfe4`,
  status `frozen-unscored-rb2-complete` e denominador exato de 252 casos
  únicos: 200 positivos, 52 negativos, 126 perguntas `pt-BR` e 126 `en-GB`.
  Review, adjudicação, qrels, matrizes contratuais/de elegibilidade, quotas,
  identidades de catálogo, ativação e geração estão vinculados. O
  `observedResultCount` é zero.
- Tier de RB-2: `REPRESENTATIVE_HOMOLOGATION` qualifica somente a suficiência
  e reportabilidade do denominador congelado não pontuado. Não é resultado de
  métrica, Automatic Quality Gate, Human Gate ou homologação de produto.
- RB-3 congelado: a campanha
  `rag-eval-catalogue-v1-postgresql-18-rb3-20260814-001`, concluída sob
  `AUTH-S07-RB3-CAMPAIGN-INPUT-FREEZE-001`, possui campaign-input-freeze
  manifest SHA-256
  `ac7b5763bc9e571b6365449b340c8256790c5fe57ba79142b592b854cf25303c`,
  status `frozen-unscored-rb3-complete` e exatamente um vetor original por
  caso. Os 252 vetores possuem 1.536 componentes `float32` little-endian,
  6.144 bytes cada e SHA-256 individual vinculado.
- Evidência retida: o receipt registra materialização de embeddings concluída
  uma única vez, sem retry, e as três identidades de árvore dos stores
  protegidos idênticas antes e depois. Scorer, campanha Responses, `RB-4` e
  Human Gate possuem contadores zero.
- Escopo negativo preservado nesta reconciliação: nenhum provider, credencial,
  rede, scorer, consulta, RB-4, build, teste de produto, deploy, publicação,
  push, billing, Human Gate ou mudança de lifecycle foi executado. OpenAPI,
  código, schemas, corpus de produto, geração ativa, runtime, stores e
  artefatos locais ignorados permaneceram inalterados.
- Escopo documental fechado: somente ADR-0014, relatório factual de STATE-07,
  Current State, este histórico por acréscimo no EOF e Prompt System Change
  Log foram alterados. O corpus avança por `PATCH` factual de `4.10.41` para
  `4.10.42`.
- Verificação documental: os manifests e inventários RB-2/RB-3 completos
  fizeram parse; os dois hashes de freeze, todos os artefatos referenciados,
  os 252 IDs de caso, os 252 IDs/arquivos de vetor, comprimentos e digests
  conferiram sem erro. `git diff --check` passou e
  `eng/check-repository.ps1` aprovou 341 arquivos não ignorados. O diff fechado
  contém somente os cinco documentos canônicos autorizados; OpenAPI v1/v2
  permaneceram byte a byte protegidas. Build, testes de produto e validação
  executável permaneceram `NOT_RUN`.
- Integridade append-only: este histórico preserva byte a byte seu prefixo
  anterior de 516.099 bytes no SHA-256
  `dc12cf5e0e8c328ac004b46db5b7f76aed28bf2a43a4165336c02d2c1a44f2ec`.
- Próxima condição diretamente relacionada: obter autoridade humana separada
  e delimitada para `RB-4 — Retrieval-only campaign` sobre os inputs
  congelados, com todos os 252 casos e repetições, sem answer-LLM. Esta
  reconciliação não concede essa autoridade.

## 2026-08-14 — Etapa 1 de governança e prontidão multiagente concluída

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo. Esta auditoria não executou Automatic Quality Gate ou Human Gate de
  lifecycle, não autorizou `STATE-08` e não iniciou a Etapa 2.
- Autoridade e sequência: os prompts do proprietário `Stage 0 - Instructions
  developing AI agents`, `Stage 1 - Governance and Multi-Agent Readiness
  Audit` e `Stage 2 - Multi-Agent Orchestrator Implementation` foram lidos
  integralmente e aplicados como um plano sequencial. Stage 0 foi incluído por
  disposição explícita do proprietário após aparecer como novo input local.
- Baseline inicial: `main@9f309e1b6a21a33cbd24b4b6498e840dd26585c9`,
  um commit à frente de `origin/main`, árvore rastreada limpa, nenhum tag e
  somente os três prompts do proprietário não rastreados. A Etapa 1 foi
  executada na branch `codex/stage1-multi-agent-readiness`; os prompts
  permaneceram não rastreados e sem alteração.
- Escopo lido: instruções globais e do repositório, Start Here, governança,
  requisitos, lifecycle, estado e histórico, todos os ADRs, contratos,
  OpenAPI, schemas, migrations, scripts, workflows, manifests, testes e os
  pacotes locais completos de RB-1, RB-2 e RB-3. Worktrees estrangeiras foram
  inventariadas e não foram limpas, reutilizadas ou podadas.
- Achado bloqueante de governança: o checkpoint de RB-2 exige duas revisões
  humanas independentes e adjudicação humana sem decisões de agente, mas o
  pacote congelado registra `humanAttribution=false` para ambos os revisores,
  vinte decisões de agente e zero decisões humanas, enquanto afirma
  contraditoriamente `no agent-authored adjudication`. Os bytes congelados não
  foram alterados; RB-2 não satisfaz seu gate, RB-3 não pode ser consumido por
  RB-4 e a disposição exige autoridade humana.
- Achado bloqueante de arquitetura: a Etapa 2 materializaria stack, fronteira
  física, runner, persistência e controles de segurança ainda sem ADR aceita.
  ADR-0016 foi preparado somente como `proposed`, recomendando coordenador
  TypeScript/Node 24 e `@openai/codex-sdk` direto atrás de `AgentRunner` com
  `FakeAgentRunner`; nenhuma dependência ou implementação foi materializada.
- Correção de governança `1055934`: introduziu envelope verificável,
  paralelismo operacional, ownership, isolamento, locks, stop conditions,
  configuração `.codex` e seis papéis project-scoped; tornou branch coverage
  sem observações uma falha, reconciliou o estado factual e preservou os
  limites de lifecycle, segurança e autoridade humana.
- Primeiro gate limpo: `./eng/ci.ps1 -Offline` sobre `1055934` passou 278 de
  279 testes de integração e falhou ao abrir uma conexão com
  `ObjectDisposedException` em `SQLitePCL.sqlite3`. O mesmo caso passou 1/1
  isoladamente. A inspeção encontrou treze chamadas em sete arquivos que
  limpavam pools SQLite process-wide enquanto classes xUnit podiam executar em
  paralelo.
- Correção de teste `b64291d`: adicionou serialização assembly-level das
  classes de integração sem alterar produto, assertions ou thresholds. Essa
  identidade substitui localmente `a0def61` após reescrita excepcional
  autorizada somente para corrigir `serialize` para `serialise`; ambas possuem
  a árvore idêntica `896659a5c4f40e57e954dc3980d4ba2377d9acda`. A suíte
  focal passou 279/279 em aproximadamente 72 segundos.
- Gate final observado: runtime preflight não encontrou processo
  RAG-Challenge-owned nem listener conflitante. Em worktree limpa e detached
  na identidade pré-reescrita
  `a0def61bf39471fd7647198d29bbcd2702171fca`, equivalente pela árvore à
  identidade atual `b64291d637b198120314f3152fc171b7904bb888`, com .NET `10.0.303`,
  Node `24.19.0`, npm `11.17.0` e PowerShell `7.6.4`,
  `./eng/ci.ps1 -Offline` iniciou em `2026-08-14T21:42:36.9170657Z`, terminou
  com exit code zero em aproximadamente 144 segundos e passou: build Release
  com zero warnings/errors; 215 testes unitários, 11 de arquitetura e 279 de
  integração; 95,38% de linhas (50.110/52.539); 67,23% de branches
  (5.164/7.681); lint, typecheck, 45 testes web, web build e auditoria de 351
  arquivos não ignorados. O modo offline não executa auditorias online de
  dependências e não equivale ao workflow hospedado completo.
- Integridade preservada: OpenAPI v1/v2 mantiveram SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e
  `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733`.
  Os freezes RB-2/RB-3 mantiveram
  `daede1db869f7daf784fa2f3fc3b55e037cf4f3bb59a22f94e2026175858bfe4` e
  `ac7b5763bc9e571b6365449b340c8256790c5fe57ba79142b592b854cf25303c`.
- Disposição final da readiness: `HUMAN_DECISION_REQUIRED`. A Etapa 2 não
  pode começar até o proprietário dispor o conflito RB-2 e aceitar ou rejeitar
  explicitamente ADR-0016, e até as condições resultantes serem reavaliadas na
  baseline real deixada por essas decisões.
- Escopo negativo: nenhuma feature de produto, OpenAPI, schema, migration,
  corpus, índice, provider, secret, billing, deploy, publicação, push, merge,
  release, Human Gate ou lifecycle foi alterado. Nenhum artefato de
  `tools/ai-orchestrator` ou dependência da Etapa 2 foi criado.
- Integridade append-only: este histórico preserva byte a byte seu prefixo
  anterior de 519.952 bytes no SHA-256
  `b1651b1043008d78fdcf19e21bb802bbeb801c7decad9867e101c01d4f91fbf5`.

## 2026-08-14 — Disposição humana de RB-2/RB-3, aceitação de ADR-0016 e reavaliação da Etapa 2

- Baseline de decisão: branch `codex/stage1-multi-agent-readiness`, HEAD
  `355bd6cd731528bcdb8fccfe71ee93b70acb1d1e`, corpus `4.11.1`, árvore
  rastreada limpa e somente os três prompts do proprietário não rastreados.
  Nenhum worktree estrangeiro foi alterado, reutilizado ou removido.
- Disposição RB-2/RB-3: o proprietário selecionou explicitamente
  `OPÇÃO 1 — QUARENTENA HISTÓRICA E SUCESSOR HUMANO`. Os freezes atuais ficam
  preservados sem edição in-place apenas como evidência histórica. RB-2 não
  satisfaz seu gate, RB-3 permanece indisponível para RB-4 e nenhum dos dois
  pode ser reinterpretado como válido por sua completude mecânica.
- Condição de sucessor: qualquer sucessor RB-2 exige autoridade separada, duas
  revisões humanas independentes e adjudicação humana real; um sucessor RB-3
  também é necessário se seus inputs vinculados mudarem. Esta decisão não
  autoriza criar esse sucessor nem executar RB-4.
- Decisão arquitetural: o proprietário aceitou ADR-0016 pela frase exata
  `ADR-0016: ACEITAR.`. A decisão seleciona somente o orchestrator
  determinístico TypeScript/Node 24 e a fronteira `AgentRunner` com
  `FakeAgentRunner` e `CodexRunner`. Ela não constitui Human Gate e não
  materializa a implementação por si só.
- Reavaliação objetiva: a quarentena remove a pendência RB-2/RB-3 do caminho
  crítico da Etapa 2 sem validar os freezes, e a ADR aceita satisfaz a
  pré-condição arquitetural. Não resta condição humana da Etapa 1 para a
  implementação local e offline-first do orchestrator; a classificação passa
  a `READY_FOR_STAGE_2`.
- Autoridade resultante: a solicitação original das Etapas 1 e 2, combinada
  com a instrução explícita de retomar se todas as condições fossem
  satisfeitas, autoriza a Etapa 2 somente depois da validação deste registro e
  sob o prompt da Etapa 2, ADR-0016 e a baseline Git resultante. A Etapa 2 não
  havia sido iniciada neste registro de decisão.
- Escopo negativo preservado: nenhum freeze foi editado; nenhum sucessor,
  RB-4, provider, chamada real de agente, secret, network, billing, produção,
  push, merge, release, Automatic Quality Gate ou Human Gate de lifecycle foi
  executado ou autorizado. `STATE-07 TESTING_HOMOLOGATION` permanece ativo e
  nenhuma transição de lifecycle ocorreu.
- Runtime preflight: `NOT_APPLICABLE` para este incremento exclusivamente
  documental; nenhum processo ou listener foi inspecionado ou encerrado.
- Integridade append-only: este histórico preserva byte a byte seu prefixo
  anterior de 525.470 bytes no SHA-256
  `699434c669414071a273acc3f08d01b6cf41dacbb0d2f4ab3fe7eb967ff21c50`.

## 2026-08-14 — Condição de dependência da Etapa 2 identificada antes da implementação

- Baseline observada: branch `codex/stage1-multi-agent-readiness`, HEAD
  `60ccbdc4ec1e53bd456ba91c339846d65ada95e3`, corpus `4.12.0`, árvore
  rastreada limpa e somente os três prompts do proprietário não rastreados.
- A releitura integral do prompt da Etapa 2 confirmou o requisito de versão
  exata, lockfile reproduzível e testes contratuais de `CodexRunner` sobre a
  interface oficial selecionada por ADR-0016.
- A documentação oficial vigente confirmou `@openai/codex-sdk` como biblioteca
  TypeScript server-side, com start, continue e resume de threads. Node
  `24.19.0` e npm `11.17.0` satisfazem a faixa aceita.
- O inventário local observou zero entrada ou tarball de
  `@openai/codex-sdk` no cache npm e nenhum pacote instalado utilizável. Sem o
  pacote e seu grafo não é possível gerar/verificar legitimamente o
  `package-lock.json`, compilar o adapter ou provar seus testes contratuais.
- A autoridade vigente permite a implementação original somente dentro do
  escopo local/offline-first e não concede aquisição externa por implicação.
  Não houve `npm install`, acesso ao registry, criação de lockfile, scaffold,
  código, diretório `tools/ai-orchestrator/` ou execução de agente.
- A readiness é refinada para `HUMAN_DECISION_REQUIRED`, pois sua satisfação
  depende de nova autoridade ou input do proprietário. A condição ainda não
  satisfeita é receber autoridade delimitada para adquirir e
  verificar a versão exata do SDK e dependências de desenvolvimento no npm
  registry, com lifecycle scripts desabilitados, ou receber cache offline
  completo e verificável.
- Não são alternativas válidas fabricar lockfile, omitir `CodexRunner`, usar
  parsing de terminal ou declarar evidência sem pacote. Se o SDK verificado
  não suportar start/resume, working directory isolado, sandbox compatível e
  resultado validável, aplica-se `ARCHITECTURE_CHANGE_REQUIRED`.
- Escopo negativo preservado: nenhuma chamada real de Codex, secret, provider
  de produto, billing, produção, push, merge, release, RB-4, Human Gate ou
  mudança de lifecycle foi autorizada ou executada. Os freezes permanecem em
  quarentena histórica e `STATE-07 TESTING_HOMOLOGATION` permanece ativo.
- Runtime preflight: `NOT_APPLICABLE`, pois somente documentação, inventário
  de cache e versões de toolchain foram inspecionados; nenhum comportamento
  executável do projeto foi alterado ou validado.
- Integridade append-only: este histórico preserva byte a byte seu prefixo
  anterior de 528.251 bytes no SHA-256
  `b1e6925a12b6bfc3b0c996678e76074255c462c3346e807c45a6be8d1d27baa6`.

## 2026-08-15 — Etapa 2 de orchestrator multiagente implementada e validada

- Estado anterior e resultante: `STATE-07 TESTING_HOMOLOGATION` permanece
  ativo. A Etapa 2 implementou tooling de desenvolvimento fora do runtime de
  produto; não executou Automatic Quality Gate ou Human Gate de lifecycle e
  não autorizou `STATE-08`.
- Baseline da Etapa 2: branch `codex/stage1-multi-agent-readiness`, HEAD
  `5f98f128a605a577eb987be3f951d90d9453b193`, árvore rastreada limpa e somente
  os três prompts Stage 0/1/2 do proprietário não rastreados e preservados.
- Autoridade de dependências: depois do stop `HUMAN_DECISION_REQUIRED`, o
  proprietário autorizou exclusivamente HTTPS ao `registry.npmjs.org` para
  metadados e aquisição de `@openai/codex-sdk`, transitivas e dependências de
  desenvolvimento estritamente necessárias, com versões exatas, lifecycle
  scripts desabilitados, sem audit ou fund e com geração/verificação do
  lockfile. A autorização excluiu Codex real, API/provider, secrets, tracing,
  billing e qualquer outro host.
- Grafo adquirido: `@openai/codex-sdk` e `@openai/codex` `0.147.0`, TypeScript
  `5.7.3`, `@types/node` `24.13.3` e `undici` `7.18.2`. As validações
  posteriores restauraram do cache local em modo offline.
- Implementação: os commits `b10d8ac`, `9433962`, `ac41b13`, `a4889c3`,
  `5da6b9a` e `94ea9b7` materializam o coordenador determinístico, contratos e
  schemas fechados, DAG/scheduler/state machine, `AgentRunner`,
  `FakeAgentRunner`, `CodexRunner`, persistência e journal, checkpoints,
  locks, Git/worktrees, integração sequencial, CLI, observabilidade,
  recovery, segurança, testes e integração no CI canônico.
- Isolamento e recovery: cada write lane exige branch/worktree/root/recurso
  exclusivos; overlaps iguais, ancestrais ou descendentes são rejeitados.
  Estado, tentativa, thread e outcome usam write-ahead; retry é cumulativo;
  cleanup e rollback preservam qualquer path, marker, branch ou conteúdo cuja
  ownership não possa ser provada.
- Revisões finais: três revisões read-only independentes sobre o snapshot
  `ef8bdf27baf8212699ea7cb030c1556a78811809` encontraram zero P0, P1, P2 ou
  P3 e retornaram `READY_FOR_GATES`, `SECURITY_REVIEW_PASS` e
  `MULTI_AGENT_READY`.
- Gate executável: runtime preflight encontrou zero processo
  RAG-Challenge-owned. Em worktree limpa e detached no commit
  `94ea9b794f041c047363c85b0102e11e34fb2c9f`, `./eng/ci.ps1 -Offline` passou
  com build sem warning/erro, 215 testes unitários, 11 de arquitetura, 279 de
  integração, 45 web e 81 do orchestrator; cobertura .NET 95,38% de linhas e
  67,23% de branches, cobertura do orchestrator 84,27% e 76,78%, e auditoria
  de 404 arquivos aprovada. Dois testes leaf-symlink ficaram skipped porque o
  host Windows não permitiu criação de file symlink; os testes de junction e
  demais fronteiras físicas passaram.
- Dry run e E2E: o plano controlado vinculado ao baseline exato produziu quatro
  waves (`map-repository` + `check-architecture`; `independent-review`;
  `quality-gate`; `human-gate`) sem criar estado ou executar agente. O E2E
  controlado com `FakeAgentRunner` passou 1/1 e comprovou candidate binding,
  revisões, integração e quality gate determinísticos.
- Limitação arquitetural: o SDK `0.147.0` expõe o ID de nova thread apenas
  depois do primeiro turn. Como o contrato exige identidade durável antes do
  turn, novo start Codex real falha fechado com
  `ARCHITECTURE_CHANGE_REQUIRED`. Resume de identidade já persistida está
  mapeado e testado; o CLI não habilita execução Codex real.
- Readiness operacional: `MULTI_AGENT_READY_WITH_CONDITIONS` para o
  orchestrator local com fake neste host. O resume persistido está mapeado e
  testado por contrato, mas não exposto pelo CLI nem exercitado contra Codex
  real; exige autoridades separadas de execução, provider, rede e credencial.
  Nova thread real exige sucessor aceito de ADR-0016 ou SDK compatível e
  autoridade de execução separada. Em host que permita file symlink, os dois
  testes leaf-symlink precisam passar antes de transportar a classificação.
- Escopo negativo preservado: nenhuma chamada Codex/OpenAI/provider, secret,
  tracing externo, billing, corpus, RB-4, Human Gate, produção, deploy, push,
  merge, release ou mudança de lifecycle foi executada. OpenAPI v1/v2 e
  projetos do produto permaneceram inalterados.
- Git: nenhum push ou merge foi executado. O worktree temporário exclusivo de
  validação foi removido somente depois de HEAD e árvore limpa serem
  comprovados; worktrees estrangeiros permaneceram intocados.
- Integridade append-only: este histórico preserva byte a byte seu prefixo
  anterior de 530.978 bytes no SHA-256
  `f5a86f67f8ce9959366cd79e3225e0890f370951fbc907b10932b07160214e7c`.

## 2026-08-15 — Disposição fake-only de ADR-0016 preservada após avaliação documental

- Baseline observada: branch `codex/stage1-multi-agent-readiness`, HEAD
  `0bf65c7b15e402d12ab8f7df5855bc5aebaf1bba`, árvore rastreada e staged
  limpas, com somente os três prompts Stage 0/1/2 do proprietário não
  rastreados e preservados.
- Autoridade: o proprietário autorizou exclusivamente avaliação documental,
  sem materialização, de um sucessor de ADR-0016 ou de uma versão verificada
  do SDK que fornecesse identidade durável de nova thread antes do primeiro
  turn. Depois, autorizou somente esta reconciliação factual em quatro
  documentos canônicos.
- Evidência: o SDK locked `@openai/codex-sdk` `0.147.0` expõe identidade de
  nova thread somente depois do início do primeiro turn. Nenhuma versão
  adicional foi localmente verificada, e nenhum acesso de rede foi executado.
- Decisão humana: ADR-0016 permanece `accepted` e não `superseded`;
  `FakeAgentRunner` permanece a única baseline operacional validada; ADR-0017
  não foi criado ou aceito; `NEW_REAL_START` permanece
  `ARCHITECTURE_CHANGE_REQUIRED`.
- Condição futura: antes de qualquer primeiro turn ou efeito externo, uma
  interface oficialmente suportada e verificável deve fornecer identidade
  externa resumível e durável por mecanismo client-supplied, idempotente ou
  deterministicamente reconciliável, além de preservar start/resume, working
  directory isolado, sandbox, approval/network deny e structured output
  validável. Qualquer nova avaliação ou sucessor exige autoridade própria.
- Estado resultante: `MULTI_AGENT_READY_WITH_CONDITIONS` permanece inalterado
  somente para a baseline local com `FakeAgentRunner`; `STATE-07
  TESTING_HOMOLOGATION`, Human Gate e lifecycle permanecem inalterados.
- Escopo negativo preservado: nenhum ADR sucessor, implementação, aquisição de
  SDK, Codex/provider, rede, credencial, billing, produção, push, merge,
  release, RB-4, Human Gate ou mudança de lifecycle foi autorizado ou
  executado.
- Runtime preflight e testes executáveis: `NOT_APPLICABLE`, pois a ação foi
  exclusivamente documental e não alterou comportamento executável.
- Integridade append-only: esta entrada preserva byte a byte o prefixo anterior
  de 535.811 bytes no SHA-256
  `ab069efbfa989519ad7519a68c3dd36cbbd206ec492015c52a35cb8d14755972`.

## 2026-08-15 — ADR-0017 preparado para checkpoint Codex pré-turno

- Autoridade: `AUTH-MULTI-AGENT-REAL-RUNNER-PREP-001`, derivada da solicitação
  explícita do proprietário para tornar Stage 0, Stage 1 e Stage 2
  operacionais, mantendo a tarefa limitada e sem uso de `OPENAI_API_KEY`.
- Baseline seletiva: worktree limpa `codex/main-stage-integration`, HEAD
  `0854d46717214321783423370601ba0a0d045e7e`, corpus `4.13.1`. Os três
  documentos Stage 0/1/2 do proprietário permanecem não rastreados e
  preservados no worktree principal.
- Verificação registrada: a documentação oficial mantém a SDK como superfície
  para automação, mas a release estável mais recente no registry npm continua
  `@openai/codex-sdk` `0.147.0` e seu ID permanece disponível somente após o
  primeiro turn começar. A interface oficial Codex App Server retorna
  `thread.id` em `thread/start` e inicia geração separadamente em `turn/start`.
- Proposta: ADR-0017 permanece `proposed` e preserva o núcleo determinístico de
  ADR-0016. O único boundary substituído é o transporte real do runner, com
  sequência `thread/start` → checkpoint persistido e relido → `turn/start`.
- Autenticação: o candidato usa somente o estado de autenticação Codex local
  já provisionado, cuja validade ainda deverá ser comprovada de forma
  sanitizada. Nenhuma `OPENAI_API_KEY` foi lida, usada, alterada, copiada ou
  autorizada.
- Disposição: `HUMAN_DECISION_REQUIRED`. A próxima decisão diretamente
  relacionada é aceitar ou rejeitar ADR-0017. Implementação e uma validação
  real permanecem ações posteriores e delimitadas.
- Validação: escopo, links, UTF-8/LF, newline final, trailing whitespace,
  `git diff --check` e higiene documental são os checks aplicáveis. Runtime
  preflight, build e testes executáveis são `NOT_APPLICABLE` e `NOT_RUN` neste
  incremento documental.
- Escopo negativo preservado: nenhum código, dependency, chamada Codex,
  secret, billing, provider de produto, Automatic Quality Gate, Human Gate,
  lifecycle, produção, push, merge, release ou deploy foi executado.
- Integridade append-only: esta entrada preserva byte a byte o prefixo anterior
  de 538.200 bytes no SHA-256
  `e5ef54624e8cf6b917a0f402754f325406671a595c5ccdcb4e1b78f135586838`.

## 2026-08-15 — ADR-0017 aceito para o runner Codex App Server

- Baseline da decisão: branch `codex/main-stage-integration`, HEAD
  `f150b2d0523a92c25a40501f48c7247be3f7c36f`, corpus `4.14.0`, worktree
  rastreada e staged limpa.
- Decisão humana: o proprietário aceitou ADR-0017 mediante a frase exata
  `ADR-0017: ACEITAR.`. A decisão seleciona somente a substituição do
  transporte real pelo Codex App Server com checkpoint persistido e relido
  entre `thread/start` e `turn/start`.
- Autoridade separada: o pedido anterior do proprietário para tornar Stage 0,
  Stage 1 e Stage 2 operacionais permanece como envelope delimitado para a
  implementação e uma validação real controlada, sem `OPENAI_API_KEY`.
- Limites: a aceitação não é Human Gate e não concede secret, provider de
  produto, produção, push, merge, release, deploy ou lifecycle.
- Execução nesta reconciliação: runtime preflight, build, testes e chamada
  Codex permaneceram `NOT_RUN`; nenhum código executável foi alterado.
- Integridade append-only: esta entrada preserva byte a byte o prefixo anterior
  de 540.501 bytes no SHA-256
  `7dcd8c3e7afecfc0c34e2fb738caae24d18c2f88d86ea3c9f1a5a320c8661dca`.

## 2026-08-15 — ADR-0017 implementado e runner Codex ativado

- Autoridade: aceitação explícita `ADR-0017: ACEITAR.` e pedido delimitado
  anterior do proprietário para tornar Stage 0, Stage 1 e Stage 2 operacionais,
  incluindo uma validação real controlada e excluindo `OPENAI_API_KEY`.
- Runtime preflight: aplicável antes da alteração executável; zero processo ou
  listener pertencente ao RAG-Challenge foi encontrado, portanto nenhum
  processo foi encerrado.
- Implementação: os commits de origem `76d40b3` e `bf31821`, integrados
  seletivamente como `583c3b4` e `9512d6e`, substituíram o SDK pelo cliente
  JSONL do Codex App Server, mantiveram o núcleo determinístico e
  `FakeAgentRunner`, adicionaram `--runner codex` com
  `--authority-reference`, start/resume, checkpoint pré-turno, structured
  output, timeout, encerramento do processo próprio e negação de approval,
  user input, web search, agent network, MCP, plugins e capabilities externas.
- Dependência e autenticação: `@openai/codex` permanece locked diretamente em
  `0.147.0`. O `account/read` sanitizado confirmou somente o modo `chatgpt`;
  o ambiente filho usa allowlist fechada e não herda `OPENAI_API_KEY`.
- Correção de protocolo: a primeira tentativa orquestrada parou antes de criar
  thread ou turn porque `runtimeWorkspaceRoots` exigia `experimentalApi`. O
  diagnóstico seguinte mostrou a mesma exigência para `environments`. Ambos e
  os demais campos experimentais foram removidos; `cwd` e a sandbox policy
  explícita do turn preservam o isolamento estável. Um preflight posterior
  obteve identidade durável sem iniciar turn.
- Validação real: o run
  `run-38b7dabe-491d-40f8-baaf-ce11906bd78e`, sobre o commit técnico de origem
  `bf318213cccbddfa91d10dc5e7555e0f547b3431`, executou uma task read-only e
  exatamente um turn real. A revisão `4` persistiu a identidade da thread
  enquanto a task ainda estava `RUNNING`; as revisões `5` e `6` registraram
  `PASS`; a validação persistida confirmou zero locks e nenhum stop condition.
- Quality Gate: `npm run check` passou com 87 testes, 85 aprovados, zero falha
  e dois skips condicionados à permissão de criar file symlink; cobertura do
  orchestrator 81,89% de linhas e 75,85% de branches. O gate canônico limpo
  passou 215 testes unitários, 11 de arquitetura, 279 de integração, 45 web e
  87 do orchestrator; cobertura .NET 95,38%/67,23%; auditoria aprovada para 442
  arquivos não ignorados.
- Disposição: `MULTI_AGENT_READY` somente para o tooling de desenvolvimento.
  Cada execução futura permanece deny-by-default e exige plano fechado,
  baseline limpa e autoridade delimitada próprios. Produto, provider de
  produto, dados reais, Human Gate, lifecycle, produção, push, merge, release
  e deploy não foram alterados ou autorizados.
- Preservação: os três documentos Stage 0/1/2 do proprietário permaneceram
  não rastreados e sem alteração.
- Integridade append-only: esta entrada preserva byte a byte o prefixo anterior
  de 541.704 bytes no SHA-256
  `fd577ffcea1eaa3bb78eca0247a79e23135e8b6ba7882874e5561c09eefaabb3`.

## 2026-08-16 — Stage 0/1/2 governance, security and en-GB candidate reconciled

- Authority: `AUTH-STAGE012-GOV-SEC-ENGB-IMPL-001` and the owner’s
  supplemental decision for the instruction-system version ledger.
- Baseline and integration: the work was isolated in exclusive branches and
  worktrees, reviewed one candidate at a time and integrated sequentially on
  `codex/stage012-integration`. Candidate
  `6b2d95c98fc84835154e3d35bda78a2b684fade1` is clean and remains separate
  from local `main` pending the final integrated gate.
- Governance: the Stage 0/1/2 hand-off rule is normative only where a
  copy-ready development hand-off is applicable. Its seven exceptions retain
  simple and sequential work, Human Gate and purely decisional boundaries;
  multi-agent readiness never grants continuous authority.
- Product credential boundary: administrative indexing, query embedding and
  grounded generation use distinct operation-specific request references and
  independently trusted in-memory grants. Agents, the orchestrator, CI,
  common tests and development tools receive closed environments and cannot
  receive product credential material. Local launchers no longer read
  `.env.local`; all verification used synthetic readers, maps, values and
  fake handlers, with no provider call or credential use.
- Language: current project-owned technical prose and new commit messages are
  governed by en-GB enforcement over immutable Git objects, protected control
  paths and structured exclusions. The migration baseline is `COMPLETE` with
  zero findings. Owner-facing pt-BR, functional localisation, source and
  citation languages, canonical contracts, Git history, historical evidence
  and append-only records remain preserved.
- Provenance: the three original Stage owner inputs were moved byte for byte
  to ignored local paths under `reference-materials/governance-inputs/`.
  Their tracked en-GB manifest records exact names, byte lengths, SHA-256
  identities and lineage and classifies them as historical, inactive,
  non-normative and local-only. No tracked translation or second authority was
  created.
- Version ledger: corpus `4.16.0` is a `MINOR` change. Only the current header
  of `Prompt-System-Change-Log.md` remains mutable and new entries are inserted
  newest-first. The complete `4.15.0` and earlier region is digest-bound and
  unchanged; `State-Transition-Log.md` and every other formally append-only
  region retain exact prefix protection.
- Verification before final gate: focused implementation checks, language
  tests, repository hygiene, protected OpenAPI identities and independent lane
  reviews passed. Independent completion and security reviews of the version
  decision also returned `MANUAL_PASS`. Final integrated governance,
  architecture, security and result reviews and the canonical offline gate
  remain required before local `main` may be updated.
- Negative scope: no provider call, credential use or inspection, external
  network, billing, ADR, Human Gate, lifecycle transition, push, pull request,
  release or deployment was performed or authorised.
- Append-only integrity: this entry preserves the complete previous prefix of
  544,842 bytes at SHA-256
  `8b9567ff3cec6bb79c220af0eacc669824485d717cf69df92bcc1ecb9875545e`.

## 2026-08-16 — Product credential identifier enforcement reconciled

- Authority: `AUTH-STAGE012-CREDENTIAL-ID-CORR-001`,
  `AUTH-STAGE012-CREDENTIAL-ID-PARSER-CORR-001` and
  `AUTH-STAGE012-CREDENTIAL-ID-PARSER-CORR-002`, preserving the authority and
  negative scope of `AUTH-STAGE012-GOV-SEC-ENGB-IMPL-001`.
- Enforcement: commit `45bcf7c` restricts the product credential identifier
  to 16 exact current or digest-bound historical paths under five closed
  classifications. Editable descriptive documentation uses the generic term
  “product provider credential”; no credential value is accepted or exposed.
- Parser: commit `7ee0df1` makes the orchestrator’s trusted policy parser
  require and validate the allowance field without relaxing exact keys,
  canonical envelope or digest checks. It rejects empty arrays, unsafe or
  duplicate paths, wildcards, directories, unknown classifications and
  invalid current/historical digest combinations.
- Protected test correction: commit `6a52054` moves the two new synthetic
  parser tests outside the digest-bound enforcement region without changing
  their bytes. The protected region again has SHA-256
  `597567640cead55e7557283bf50f2bda97194a507f3d45ac5f4fc5229f61a322`.
- Verification: on clean candidate
  `6a520545b12a93b04f1e45fedc637c885ac963e6`, `npm run check` passed 105 of
  107 orchestrator tests with zero failures and two host
  symlink-permission skips; `node eng/test-language-policy.mjs` passed
  100/100. The compiled parser loaded all 16 real manifest permissions.
  Independent governance, security and result reviews passed.
- Pending gate: the documentary gate and `eng/ci.ps1 -Offline` remain required
  before local `main` can be updated.
- Negative scope: no provider call, credential read or use, external network,
  billing, ADR, Human Gate, lifecycle transition, push, pull request, release
  or deployment occurred.
- Append-only integrity: this entry preserves the complete previous file of
  548,144 bytes at SHA-256
  `fef88ce45058ad967bee2faaffc6cdc62ebe24356d9bf332d97ba00c0f3d9288`.

## 2026-08-16 — Stage 0/1/2 canonical offline gate approved

- Authority: `AUTH-STAGE012-CANONICAL-GATE-FRESH-NODE-001`, followed by
  `AUTH-STAGE012-FINAL-RECONCILIATION-MAIN-001` for this factual record and
  the separately gated local integration sequence.
- Baseline: clean candidate
  `codex/stage012-integration@311f115e5b080b1d5c1cc55f43dc91426e9fcdd2`;
  local `main` remained clean at
  `d9968cac893f70989553fe9b8ae07ad7a3dbdaae` during the recorded gate.
- Runtime preflight: zero process proved to belong to RAG-Challenge, so no
  process was stopped. Existing MSBuild workers were neither interrupted nor
  deliberately reused.
- Closed environment: the single canonical invocation preserved only the
  approved non-secret Windows path inputs and set
  `MSBUILDDISABLENODEREUSE=1`. No environment enumeration, credential read or
  credential validation occurred.
- Canonical gate: `pwsh -NoProfile -File eng/ci.ps1 -Offline` ran exactly once
  and passed with exit code `0` in 279,679 ms. Sanitised capture contained
  stdout, no stderr and no failed stage. It contained no marker of a connection
  to a previously reusable node, and no reusable worker remained afterwards.
- .NET evidence: Release build and all 215 unit, 11 architecture and 294
  integration tests passed. Merged coverage was 95.41% of lines
  (`50,227/52,642`) and 67.29% of branches (`5,197/7,723`).
- Web and orchestrator evidence: all 45 Dashboard tests, lint, typecheck and
  Vite build passed. The orchestrator passed 105 of 107 tests with zero
  failures and two host symlink-permission skips; coverage was 82.04% of
  lines, 76.83% of branches and 88.74% of functions.
- Policy and repository evidence: the language-policy suite passed 100/100;
  the immutable language check covered 419 files with zero accepted migration
  findings and one commit message; coverage-policy and CI-policy tests passed;
  the repository audit passed for 419 non-ignored files; and Git diff hygiene
  passed.
- Disposition: the Stage 0/1/2 governance, product-credential isolation,
  en-GB migration and credential-identifier enforcement candidate satisfied
  its authorised local documentary and canonical offline gates. This result
  is local evidence and does not constitute an online dependency audit,
  product homologation, Human Gate or lifecycle transition.
- Negative scope: no provider call, credential use or inspection, external
  network, billing, ADR, Human Gate, lifecycle transition, push, pull request,
  release or deployment occurred.
- Append-only integrity: this entry preserves the complete previous file of
  550,227 bytes at SHA-256
  `1a1c6317bdc126bf244cfa50f397e3d54aa24c3188dd7d1f01c1be6b2da13c38`.

## 2026-08-16 — en-GB repository completion candidate reconciled

- Authority: `AUTH-ENGB-REPOSITORY-COMPLETION-IMPL-001`, with the owner’s
  binding decision to preserve the enumerated canonical family and stated
  public scripts exactly.
- Baseline and isolation: the coordinator matched the exact clean baseline
  `main@8882ab8a58e1db58fb0148b967894f1b8388adc2`. The internal and
  enforcement lanes used the exclusive branches and worktrees named by the
  owner; the enforcement lane was created only after the internal candidate
  had been reviewed and integrated. Mutations, integration and gates remained
  sequential; reviewers were read-only.
- Internal completion: source commit `f7b51dd` was independently approved and
  integrated as `172575da`. It changed only six files and 54 proven private,
  editable, non-serialised, non-persisted, non-hash-bound and non-contractual
  lexical occurrences. Focused unit tests passed 21/21 and integration tests
  passed 3/3.
- Enforcement completion: source commits `1ae50b3` and `db57d68` were
  integrated one at a time as `b9031d5` and `fe7f9f0`. Policy v2 classifies
  all tracked blobs, exact binary and immutable-text identities, digest-bound
  regions, legacy-spelled filenames, canonical identifiers and every new
  commit message. Its baseline remains `COMPLETE` with zero accepted debt.
- Independent correction: the first enforcement review found a P1 generic
  skip for any line containing a `"pt-BR"` key. Integration stopped. The skip
  was removed, its former reproduction became a negative regression test and
  the two localisation fixtures exposed by the correction received exact
  digest-bound classifications. The closing review passed with zero P0–P3
  findings.
- Preserved identities: OpenAPI v1/v2 blobs and SHA-256 values, the migrations
  and evaluation trees, accepted ADRs, product requirements, localisation,
  sources, citations, append-only prefixes, the binding canonical family and
  the four owner-listed public script paths remain unchanged.
- Current gate status: focused lane checks and independent lane reviews passed.
  Integrated final reviews, the documentary gate and exactly one sequential
  `./eng/ci.ps1 -Offline` execution on the reviewed integrated baseline remain
  pending.
- Negative scope: no provider, product credential identifier or value,
  billing, network, installation, OCI, GitHub, push, pull request, merge,
  release, deploy, ADR, Human Gate or lifecycle action occurred.
- Append-only integrity: this entry preserves the complete previous file of
  552,927 bytes at SHA-256
  `4bc73464a8c1e0002fa01e2b5952f5b0030fcfdd943e1efbe86ef318088da086`.

## 2026-08-16 — Backticked commit-literal enforcement corrected

- Authority: the correction remains within
  `AUTH-ENGB-REPOSITORY-COMPLETION-IMPL-001` and its exceptional,
  manually reviewed enforcement boundary. It creates no reusable control
  update authority.
- Finding: the first integrated result review found one P1 after documentary
  reconciliation. The repository checker and orchestrator removed every
  backticked span before checking legacy-spelled identifiers, so arbitrary
  private identifiers or American prose could evade commit-message
  inspection. Gates remained stopped and the canonical CI remained `NOT_RUN`.
- Correction: source commit `ac4f7e5`, integrated as `08a2c96`, derives the
  technical-literal exception only from complete, exact values in the
  validated canonical-identifier allowances. Unclassified backticked content
  returns to lexical and identifier inspection. The trusted orchestrator
  parser validates the same allowance envelope, paths, classes, kinds, values,
  counts, context hashes and uniqueness before deriving its set.
- Verification: negative tests in both implementations reject a private
  identifier and American prose inside backticks, while the complete exact
  canonical value remains accepted. The language-policy suite passed 105/105;
  orchestrator lint and typecheck passed; its suite passed 105 of 107 tests
  with zero failures and the two existing host symlink-permission skips. The
  repository inspection retained zero findings.
- Independent review: the reviewer who reported the P1 reproduced the two
  negative and one positive outcomes, validated the policy, baseline and two
  affected region digests, and approved the correction with zero residual
  P0–P3 findings.
- Preservation: OpenAPI v1/v2, migrations, evaluation data, localisation,
  sources, citations, accepted ADRs, product requirements, owner-listed
  canonical identities and public scripts remain unchanged.
- Current gate status: integrated final reviews, documentary gate and exactly
  one sequential `./eng/ci.ps1 -Offline` execution remain pending.
- Negative scope: no provider, product credential identifier or value,
  network, billing, installation, OCI, GitHub, push, pull request, merge,
  release, deploy, ADR, Human Gate or lifecycle action occurred.
- Append-only integrity: this entry preserves the complete previous file of
  555,592 bytes at SHA-256
  `6174ea06a9b3147c36daaa882966c98f5646863befc3552e86c0c8187501fb30`.

## 2026-08-16 — Repository-completion canonical gate failed closed

- Authority and baseline: the only canonical execution authorised by
  `AUTH-ENGB-REPOSITORY-COMPLETION-IMPL-001` ran on the exact clean branch
  `codex/en-gb-repository-completion@2c2b80c106be6a9b69884e2267c3d7a84d7c11f9`.
  Both integrated final reviews had passed with zero P0–P3 findings, and the
  documentary gate had passed for 420 non-ignored files.
- Runtime and isolation: preflight found zero process or listener proved to
  belong to RAG-Challenge. The command used a closed child environment,
  disabled MSBuild node reuse and bound offline NuGet, npm and temporary paths
  to the coordinator worktree and task ID. No product credential identifier
  or value was inherited or used.
- Single command: `pwsh -NoProfile -File eng/ci.ps1 -Offline` ran exactly once,
  exited `1` after 6,085 ms and was not retried.
- Observed failure: the first language-policy stage passed 84 of 105 tests and
  failed 21. Every failure reported the same sanitised synthetic Git outcome
  at `git add .`. The script stopped before restore, formatting, build, .NET
  tests, coverage, Dashboard, orchestrator and final repository-audit stages.
- Attributed cause: the isolated temporary base was 153 characters. A
  generated repository reached 220 characters and its longest fixture and Git
  lock paths reached 264. In a bounded diagnostic under the same closed Git
  environment, both long and short repositories initialised; the long
  `git add .` returned `128` with a path-length classification, while the
  identical add passed in a shorter task-owned location.
- Disposition: `TEST_BASELINE_BROKEN`. The failure is attributable to the
  coordinator’s temporary-path envelope rather than to an integrated lane,
  but the canonical gate is failed and no downstream PASS may be inferred.
  The authorised single execution is consumed. A corrected canonical run
  requires a new exact baseline and new explicit owner authority.
- Negative scope: no provider, product credential identifier or value,
  external network, billing, installation, OCI, GitHub, push, pull request,
  merge, release, deploy, ADR, Human Gate or lifecycle action occurred.
- Append-only integrity: this entry preserves the complete previous file of
  558,076 bytes at SHA-256
  `0436dfda55e5222320dc89e62649f422b32949f0f3b71f53598e4ed39d644093`.

## 2026-08-16 — Repository-completion corrected canonical gate passed

- Authority and baseline: the owner granted
  `AUTH-ENGB-REPOSITORY-COMPLETION-CANONICAL-GATE-CORR-001` for exactly one
  corrected sequential offline gate on exact clean
  `codex/en-gb-repository-completion@6662aa02a7ee9be0f4d2dbe61cebfd7846462edf`.
- Isolation and offline availability: exclusive task root
  `C:\t\engb-corr-001` reduced the temporary path from 153 to 23 characters.
  Seven NuGet lockfiles required 63 packages, all present in the copied
  task-specific cache; the copied npm cache passed local integrity verification.
  The child environment used an explicit variable allowlist, disabled MSBuild
  node reuse and inherited no product credential identifier or value.
- Preflight and postflight: each found zero process or listener proved to belong
  to RAG-Challenge. The exact branch, HEAD and clean tracked tree were preserved
  before and after the gate.
- Auxiliary precheck: a cache-location query used an unsupported option and
  exited `1` before the canonical command. It ran no gate stage and was
  corrected without a CI retry. A sanitised certificate-store inspection
  confirmed that the SDK first-run banner referred only to the pre-existing
  development certificate dated 2026-07-27; no certificate was created.
- Single command: `pwsh -NoProfile -File eng/ci.ps1 -Offline` ran exactly once
  from `2026-08-16T15:16:46.4947443Z` to
  `2026-08-16T15:21:24.1317617Z`, exited `0` after 277,634 ms and was not
  retried.
- Language and policy results: all 105 language-policy tests passed; the checker
  covered 420 tracked files (418 text and two binary) with zero accepted
  migration findings; coverage-policy and shared-CI-policy tests passed.
- .NET results: locked offline restore, lockfile checks, format verification and
  Release build passed with zero warnings and zero errors. All 215 unit, 11
  architecture and 294 integration tests passed. Merged coverage was 95.41% of
  lines (50,227/52,642) and 67.29% of branches (5,197/7,723).
- Dashboard results: offline npm restore, lint, typecheck, all 45 tests and the
  production build passed.
- Orchestrator results: offline npm restore, lint, typecheck and build passed.
  The suite passed 105 of 107 tests with zero failures and two host
  symlink-permission skips; coverage was 82.12% of lines, 76.71% of branches and
  88.78% of functions.
- Closing checks and disposition: the repository audit passed for 420
  non-ignored files and Git diff hygiene passed. Corrected canonical disposition
  is `PASS`. The earlier `TEST_BASELINE_BROKEN` result remains historical for
  its consumed authority and baseline.
- Negative scope: no online dependency audit, provider, product credential
  identifier or value, external network, billing, toolchain or system
  installation, OCI, GitHub, push, pull request, merge, release, deploy, ADR,
  Human Gate or lifecycle action occurred. Locked offline restores materialised
  project dependencies only inside the exclusive worktree and task caches.
- Append-only integrity: this entry preserves the complete previous file of
  560,466 bytes at SHA-256
  `ccbf45ddaa6438ae3cbed82f17bff4b685ce7a0162567cc0e5ba7409299f506f`.

## 2026-08-16 — ADR-0018 and ADR-0019 accepted as architecture authority

- Previous and resulting state: `STATE-07 TESTING_HOMOLOGATION` remains active.
  No Automatic Quality Gate, Human Gate or lifecycle transition was executed
  or changed.
- Decision and baseline: the owner explicitly declared
  `ADR-0018: ACEITAR.` and `ADR-0019: ACEITAR.` on clean branch `main`, commit
  `89be70aba4de556611c9bdda8da62d1d4f9a1e41`, corpus `4.17.2`, with both
  protected OpenAPI identities unchanged.
- Provider-budget architecture: ADR-0018 changes from `proposed` to `accepted`
  and assigns permanent identity `SEC-CORR-001` to durable transactional
  admission, conservative commitment of uncertain outcomes, strict operation
  allocations and explicit rearming for every runtime session.
- Renderer architecture: ADR-0019 changes from `proposed` to `accepted` and
  assigns permanent identity `SEC-CORR-002` to a dedicated minimal worker,
  parent-established pre-input attestation and equivalent fail-closed Windows
  and Linux ARM64 sandbox invariants without a weaker fallback.
- Identity preservation: the pre-existing `SEC-001` dependency-audit finding
  and its historical fixtures remain unchanged. It is not reused or resolved
  by either corrective decision.
- Operational boundary: the provider budget remains exactly zero and disarmed.
  No external price, account, credential, billing surface or provider was
  consulted or used. Existing renderer containment and static AArch64
  packaging are not reclassified as complete sandbox or native runtime proof.
- Acceptance boundary: both decisions establish architecture authority only
  and accept no risk. They grant no semantic reconciliation, public contract,
  persistence, schema, migration, code, test, project, dependency, renderer,
  launcher, platform execution, provider, credential, network, billing,
  deployment or operational authority.
- Protected artefacts: OpenAPI v1 remains at SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` and
  OpenAPI v2 remains at SHA-256
  `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733`.
- Versioning: the instruction corpus advances by factual `PATCH` from
  `4.17.2` to `4.17.3` without a public-contract, executable-behaviour,
  operational-authority, gate or lifecycle change.
- Closed documentary scope: only ADR-0018, ADR-0019, the architecture index,
  Current State, this append-only EOF and the Prompt System Change Log are
  changed.
- Negative scope: no build, executable test, runtime process preflight,
  provider preflight, Human Gate, transition, external action, GitHub action,
  push, amend, rebase, publication, release or deployment occurred.
- Documentary verification: `git diff --check` exited `0`; the repository
  audit passed for 422 non-ignored files; the language policy passed for 422
  tracked files with zero accepted migration findings; the semantic check
  confirmed both accepted statuses, both permanent corrective identities,
  corpus `4.17.3`, six authorised paths and no unexpected path. Build,
  executable tests and Automatic Quality Gate remain `NOT_RUN`.
- Runtime preflight: `NOT_APPLICABLE`, because acceptance recording is purely
  documentary and validates no executable RAG-Challenge behaviour.
- Append-only integrity: this entry preserves the complete previous file of
  563,694 bytes at SHA-256
  `fd0bd6347341bc286cf2c90738f9fa7671a10e2ecad10b68d7627761b07c58ef`.
- Next condition: obtain separate bounded owner authority for documentary
  semantic reconciliation of both accepted decisions into the security and
  threat-model owners. That authority would not include risk acceptance,
  persistence, schema, migration, implementation, dependency, budget arming,
  provider, network, platform execution, Human Gate or lifecycle activity.

## 2026-08-16 — Accepted security decisions semantically reconciled

- Authority and baseline: `SEC-CORR-ADR-RECONCILE-01` authorised only
  documentary semantic reconciliation of accepted ADR-0018 and ADR-0019 on
  clean `main@a99bf6efbb9b627f2f9115050112c7d4726eb2b5`, corpus `4.17.3`,
  with protected OpenAPI v1/v2 identities unchanged.
- Previous and resulting state: `STATE-07 TESTING_HOMOLOGATION` remains active.
  No Automatic Quality Gate, Human Gate or lifecycle transition was executed
  or changed.
- Security owner: Security-And-Access now records the accepted persistent
  `ProviderBudgetEnvelopeV1`, aggregate and strict operation allocations,
  serialisable maximum reservation/readback before credential lookup,
  conservative treatment of uncertain outcomes, explicit runtime-session
  rearming, sanitised readiness and protected audit.
- Budget disposition: no operational envelope, ledger, cost schedule, price,
  account fact or nonzero authority was created or selected. The effective
  aggregate limit and all operation allocations remain zero; provider
  capability remains `Disarmed`; no provider or external price was consulted.
- Renderer owner: Security-And-Access now records `pdf-render-sandbox-v1`, a
  dedicated minimal worker, parent-established pre-input attestation, exact
  host-resource denial, Windows suspended/Job Object/restricted-identity
  controls, Linux ARM64 namespace/seccomp/cgroup controls and no weaker
  fallback.
- Renderer disposition: current Windows Job Object and Linux
  `rlimit`/non-dumpable containment remain incomplete against the accepted
  sandbox. No worker, launcher, sandbox, OCI capability, platform execution or
  native evidence was created or reclassified.
- Threat owner: `THR-S02-009`, `THR-S02-014`, `THR-S02-016`, `THR-S02-017`,
  `THR-S02-027`, `THR-S02-035` and `THR-S02-037` now reference the accepted
  controls while retaining open implementation and evidence boundaries.
  `SEC-BUD-01` and strengthened `SEC-IMG-01` define future verification only;
  all 44 threat IDs and 22 security-test IDs remain unique.
- Risk boundary: ADR-0018 and ADR-0019 are explicitly not additions to the
  risk-acceptance list. Uncertain spend is not waived, current containment is
  not a complete sandbox and Windows/static evidence does not prove native
  Linux ARM64.
- Closed documentary scope: only Security-And-Access, the STATE-02 threat
  model, Current State, this append-only EOF and the Prompt System Change Log
  are changed. ADRs, the architecture index, code, tests, projects,
  dependencies, public contracts, schema and migrations remain unchanged.
- Protected artefacts: OpenAPI v1 remains at SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` and
  OpenAPI v2 remains at SHA-256
  `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733`.
- Documentary verification: `git diff --check` exited `0`; the repository
  audit passed for 422 non-ignored files; the language policy passed for 422
  tracked files with zero accepted migration findings; semantic checks passed
  for the zero/disarmed budget, open risks, unique IDs, protected paths and
  unchanged OpenAPI identities. Build, executable tests and Automatic Quality
  Gate remain `NOT_RUN`.
- Runtime preflight: `NOT_APPLICABLE`, because this reconciliation changes and
  validates no executable RAG-Challenge behaviour.
- Versioning: the instruction corpus advances by factual `PATCH` from
  `4.17.3` to `4.17.4` without changing authority, public contracts, executable
  behaviour, operational posture, gate or lifecycle.
- Append-only integrity: this entry preserves the complete previous file of
  567,534 bytes at SHA-256
  `e996c15db7c3641835d150860ae60422d0060d5273233185e1583634b4583f16`.
- Next condition: obtain separate bounded authority for ADR-0018
  implementation-sequence item 1, beginning with the exact internal
  persistence schema and migration design. That future authority must stop
  before changing schema, migration, code, dependencies, price, nonzero
  budget, provider, network, Human Gate or lifecycle.

## 2026-08-20 — Executable baseline and security audit facts reconciled

- Authority: `AUTH-AUDIT-STATE-RECONCILE-20260820-01` authorised only factual
  documentary reconciliation of exact clean
  `main@5cd240f30fa4409a65eeb242ca4f210bd42c2eab` in `STATE-07
  TESTING_HOMOLOGATION`. It authorised no executable correction, retrospective
  approval, homologation, Automatic Quality Gate, Human Gate, lifecycle,
  provider, network or external action.
- Preflight: the canonical repository, branch and HEAD matched the authority;
  the tracked tree was clean, no Git operation or Git lock was active, and the
  task-owned orchestrator area contained no lock-like artefact. The separate
  worktree below `<user-home>\.codex` and all preserved branches were outside
  scope and unchanged. No conflicting canonical execution authority was found.
- Factual provenance: commits
  `9c7b888c8f29f3e3767de93de3649b51859e6f84`,
  `09bf5fcbc5d70793427c8d22f5132c34c49d6f91` and
  `e67805d9f464c9ed71286d5d0396dc0fb3485023` are ancestors of the authorised
  `main` baseline. They respectively materialise the ADR-0018 schema and
  migration, Application contracts plus deterministic fake, and a SQLite
  ledger plus rearming/provider integration candidate. The third increment is
  only part of implementation-sequence item 3 and remains zero-only and
  fail-closed in Product composition.
- Authority gap: the canonical owners contain no evidenced implementation
  authority for those three executable increments. This entry records the
  executable facts and that gap; it does not approve the commits
  retrospectively, accept risk, homologate behaviour or classify any item as
  gated. ADR-0018 items 1 and 2 and part of item 3 are therefore implemented
  candidates, not approved or homologated increments.
- Blocked item 4 candidate: commit
  `7b031a5c36761404cafe35a80c50f72da500fc6e` remains one commit beyond `main`
  only on `codex/adr0018-item4-failure-recovery-tests`. Its post-dispatch crash
  expectation retains `Armed` plus `DispatchStarted`, rather than committing
  the maximum and entering `ReconciliationRequired` as ADR-0018 requires. It
  is blocked and not integrable in its recorded form; the branch and commit
  remain preserved.
- Budget audit finding: the SQLite ledger has no startup/read recovery that
  converts an orphaned `DispatchStarted` reservation into an indeterminate
  maximum commitment plus `ReconciliationRequired`. It also rejects an
  expired admission without persisting `Expired`, while the deterministic fake
  advances to that state. The rearm path accepts only `Disarmed` or `Tripped`,
  and Product admission remains limited to unconfigured zero envelopes. These
  gaps have no correction authority in this reconciliation.
- Renderer audit finding `SEC-PDF-001`: Product composition still selects the
  existing Server.Api renderer worker. Windows Job Object and Linux
  `rlimit`/non-dumpable controls do not implement ADR-0019's dedicated,
  parent-attested, pre-input `pdf-render-sandbox-v1` boundary. ADR-0019 remains
  unimplemented; future visual rendering must remain unavailable fail-closed
  until the separate sandbox is implemented and proven, while text-first
  behaviour remains independent.
- Additional audit disposition: pre-existing `SEC-001` and `SCRIPT-001` remain
  open. Mutable tag references for two setup actions and an assignment-pattern
  repository secret check leave supply-chain verification incomplete. The
  effect of proxy topology on IP-partitioned rate limiting remains conditional
  and `NOT_VERIFIED`. No workflow, scanner, script or host correction occurred.
- Canonical gate result: the 2026-08-20 canonical offline gate stopped at .NET
  format verification because import ordering in `OpenAiHttpAdapters.cs`
  diverged. Exit `1` remains `FAIL` and is not an Automatic Quality Gate.
  Separately classified diagnostics passed a Release build, 540 .NET tests,
  95.71% line and 66.58% branch coverage, 45 Dashboard tests and 105 of 107
  orchestrator tests; the two skips reflect unavailable symlink creation on
  the Windows host. Repository audit, `git diff --check`, protected OpenAPI
  identities and Git object integrity passed within their recorded scopes.
- Verification limits: online dependency audits, remote CI, provider,
  deployment, Linux/OCI and external systems remained `NOT_RUN`. `.env.local`
  remained ignored and unread. Supplemental successes neither override the
  canonical gate failure nor prove the unimplemented security boundaries.
- Closed documentary scope: only Current State, Security-And-Access, the
  STATE-02 threat model, Prompt System Change Log and this append-only EOF are
  changed. ADRs, architecture records, code, tests, schema, migrations,
  dependencies, workflows, branches, worktrees, runtime and external resources
  remain unchanged.
- Protected artefacts: OpenAPI v1 remains at SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` and
  OpenAPI v2 remains at SHA-256
  `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733`.
- Documentary verification: `git diff --check` exited `0`; the repository
  audit passed for 432 non-ignored files; the language-policy checker passed
  for 432 tracked files with zero accepted migration findings. Semantic checks
  confirmed the exact commit identities, blocked candidate, unchanged ADRs and
  executable paths, protected OpenAPI identities and append-only log prefix.
- Runtime preflight: `NOT_APPLICABLE`, because this reconciliation changes and
  validates no executable RAG-Challenge behaviour. No process enumeration or
  shutdown was performed for this documentary activity.
- Versioning: the instruction corpus advances by factual `PATCH` from `4.17.4`
  to `4.17.5` without changing architecture, authority, public contracts,
  executable behaviour, operational posture, gate or lifecycle.
- Append-only integrity: this entry preserves the complete previous file of
  571,640 bytes at SHA-256
  `faf1a41740dd25eae10d67e11a933ec8603226647617286fb3d087103bbf2916`.
- Negative scope: no fetch, push, merge, rebase, deployment, provider, network,
  paid service, Human Gate, lifecycle transition or remote action occurred.
- Next condition: obtain a separate bounded authority for `SEC-CORR-002`
  containment that makes Product visual rendering unavailable fail-closed
  while preserving text-first behaviour. A later, independently authorised
  provider-budget correction must resolve orphaned dispatch recovery and
  persistent expiry semantics before item 4 is rebuilt or any nonzero budget
  is considered.

## 2026-08-20 — Reconciliation wording corrected

- Authority: `AUTH-AUDIT-STATE-RECONCILE-20260820-01-CORR-01` authorised only
  the two P3 documentary corrections identified by the exceptional semantic
  review of commit `6685c4251fdd1bc4bf2eda82ff5d7d40576da779`.
- Security wording: the implementation candidates are now described
  idiomatically as having received neither approval nor a gate disposition.
  The correction changes no authority, risk, implementation, homologation or
  gate fact.
- Append-only clarification: the phrase `one commit beyond main` in the
  immediately preceding entry referred exclusively to the audited
  `main@5cd240f30fa4409a65eeb242ca4f210bd42c2eab` baseline. At clean correction
  baseline `main@6685c4251fdd1bc4bf2eda82ff5d7d40576da779`, the graph
  `main...codex/adr0018-item4-failure-recovery-tests` is `1/1`. Candidate
  commit `7b031a5c36761404cafe35a80c50f72da500fc6e` remains outside `main`,
  blocked and not integrable in its recorded form.
- Closed scope: only Security-And-Access, Current State, Prompt System Change
  Log and this append-only EOF are changed. Code, tests, ADRs, schema,
  migrations, dependencies, workflows, branches, worktrees, runtime and
  external resources remain unchanged.
- Runtime preflight: `NOT_APPLICABLE`, because this correction changes and
  validates no executable RAG-Challenge behaviour.
- Versioning: the instruction corpus advances by factual `PATCH` from `4.17.5`
  to `4.17.6` without changing architecture, authority, public contracts,
  executable behaviour, operational posture, gate or lifecycle.
- Append-only integrity: this entry preserves the complete previous file of
  578,260 bytes at SHA-256
  `3c945538ff11588bb1593acc65f43f2dce417e36579773c8bbb3e8f7a287c5de`.
- Negative scope: no amend, rebase, fetch, push, merge, provider, network,
  deployment, Automatic Quality Gate, Human Gate, lifecycle transition or
  remote action occurred.

## 2026-08-20 — Concurrent Render and OCI deployment targets accepted

- Decision authority: the owner explicitly instructed,
  `Então apenas documente a decisão de usar Render e OCI`, on clean
  `main@eccffff56abbd23d37378a5bde7a76d2a1d06bc9`, corpus `4.17.6`.
- Decision: accepted ADR-0020 retains Render as the already observed secondary
  public homologation surface and OCI as ADR-0005's durable MVP target. Both
  may remain available concurrently after separately authorised OCI deployment.
- Isolation boundary: each provider keeps separate configuration, secrets,
  storage, mutable state, evidence and deployment lifecycle. No shared SQLite,
  content-store or vector-store files, active-active writes, replication,
  automatic failover or traffic-management design is selected.
- OCI boundary: no tenancy, console, API, service or runtime was accessed. The
  authenticated console region mentioned by the owner is not deployment
  evidence or a selected region. ADR-0005's `sa-saopaulo-1` candidate remains
  conditional; exact region, capacity, entitlement, IAM, billing, backup and
  restore require separate authority and evidence.
- Existing authority preserved: Render does not replace the OCI requirement;
  ADR-0005, ADR-0006, ADR-0018 and ADR-0019 retain their controls.
  `SEC-CORR-002` remains the first recorded corrective implementation
  candidate and is neither postponed nor authorised by this decision.
- Closed documentary scope: only ADR-0020, the architecture index, Solution
  Architecture, Current State, Prompt System Change Log and this append-only
  EOF are changed. No code, test, schema, migration, dependency, workflow,
  OpenAPI contract, branch, worktree, runtime or external resource is changed.
- Runtime preflight: `NOT_APPLICABLE`, because the decision changes and
  validates no executable RAG-Challenge behaviour.
- Protected artefacts: OpenAPI v1 remains at SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` and
  OpenAPI v2 remains at SHA-256
  `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733`.
- Documentary verification: `git diff --check`, the repository audit for 433
  non-ignored files and the language-policy test suite passed. The worktree
  language checker stopped automatically because baseline commit `eccffff`
  changes the protected Security-And-Access control; no bypass was used. The
  full previous append-only prefix and both protected OpenAPI hashes were
  verified exactly.
- Versioning: the instruction corpus advances by `MINOR` from `4.17.6` to
  `4.18.0`, establishing a concurrent deployment topology without changing
  executable behaviour, operational authority, gate or lifecycle.
- Append-only integrity: this entry preserves the complete previous file of
  580,204 bytes at SHA-256
  `aa56532fd33f40be5ab2ac184163e6aa0fd4566f014a5aed655d1d922b64e13e`.
- Negative scope: no Render or OCI access, fetch, push, merge, rebase, amend,
  deployment, provider, network, billing, Automatic Quality Gate, Human Gate,
  lifecycle transition or remote action occurred.

## 2026-08-20 — Product visual materialisation contained fail-closed

- Authority: `AUTH-SEC-CORR-002-VISUAL-FAIL-CLOSED-20260820-01` authorised a
  focal Product composition correction on clean
  `main@f3113d8062391d163854ee2b38c689798ed4c017`, corpus `4.18.0`, with no
  Git lock or product-owned listener.
- Runtime preflight: no Product process or listener was active. The inspection
  observed only its own transient PowerShell process with no listener; it was
  not a product runtime and no process was stopped.
- Implementation: Product query composition no longer creates the existing
  Server.Api renderer worker, `DocumentRenderCandidateService` or
  `OnDemandVisualEvidenceMaterializer`. Its optional
  `IQueryVisualEvidenceMaterializer` is fixed to `null`, so an untrusted PDF
  cannot reach that incomplete worker through Product query execution.
- Preserved behaviour: the accepted text-first query path remains independent,
  and `VerifiedPageImageEvidenceReader` remains only as the read-only boundary
  for already persisted, fully bound page-image evidence. No image, manifest,
  source, activation or runtime data was changed.
- Focused verification: the exact new containment test passed 1/1; the existing
  textual-v2 preservation test passed 1/1; and the complete
  `ProductQueryRuntimeTests` class passed 18/18. The first compilation attempt
  exposed a still-required Application.Documents namespace import; restoring
  that import was the only correction before the successful rerun.
- Broader local/offline verification: locked restore, format verification for
  both changed projects and Release build passed; all 227 unit, 303 integration
  and 11 architecture tests passed; merged coverage passed at 95.68% of lines
  and 66.53% of branches; 105 language-policy tests, worktree language
  inspection, coverage/CI policy tests, repository audit across 433 non-ignored
  files and `git diff --check` passed. Protected OpenAPI v1/v2 digests remained
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`
  and `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733`.
  Solution-wide format verification retained the already recorded,
  out-of-scope import-ordering failure in `OpenAiHttpAdapters.cs`; neither
  changed project had a format finding. This verification was not an Automatic
  Quality Gate.
- Security disposition: `SEC-PDF-001` is contained at the Product query
  composition boundary. ADR-0019 remains unimplemented: the dedicated worker,
  pre-input `pdf-render-sandbox-v1`, Windows/Linux ARM64 controls and native
  platform evidence remain open and separately governed.
- Protected artefacts: OpenAPI v1/v2, schema, migrations, dependencies and
  workflows are unchanged. No renderer sandbox, provider, credential, source,
  network, Render, OCI, deployment, Automatic Quality Gate, Human Gate or
  lifecycle action occurred.
- Versioning: the instruction corpus advances by factual `PATCH` from `4.18.0`
  to `4.18.1` without changing architecture, public contracts, persistence,
  lifecycle or external authority.
- Append-only integrity: this entry preserves the complete previous file of
  583,290 bytes at SHA-256
  `a584f86258b6ddbd5973d705f985522fac0ae0bce483827d74234b32cf06aae6`.
- Negative scope: no fetch, push, merge, rebase, amend, deployment, provider,
  network, billing, external service or remote action occurred.

## 2026-08-20 — Provider-budget expiry and orphan recovery corrected

- Authority: `AUTH-SEC-BUDGET-001-RECOVERY-CORR-20260820-01` authorised the
  focal ADR-0018 recovery corrective on exact clean
  `main@fb9328d8d0ec12304289cdee6275ac82c1927bec`, corpus `4.18.1`, in
  `STATE-07 TESTING_HOMOLOGATION`. It authorised no schema, migration, OpenAPI,
  dependency, nonzero budget, provider, credential, network, billing,
  Automatic Quality Gate, Human Gate, lifecycle or deployment action.
- Preconditions: the main worktree and index were clean; no Git or orchestrator
  lock existed; runtime preflight found no RAG-Challenge-owned process or
  listener and stopped nothing. Protected OpenAPI v1/v2 retained SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`
  and `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733`.
- Parallel evidence: three owner-authorised fronts mapped ledger transactions,
  the recovery test matrix and ADR-0018/security conformance in read-only mode.
  They created no branch, worktree, lock, file change, integration, gate or
  canonical-memory mutation. The coordinator synthesised all three results
  before becoming the only writer; implementation and integration remained
  sequential in the main worktree.
- Expiry correction: the SQLite admission transaction now persists and reads
  back one `Expired` ledger revision, matching `EnvelopeExpired` audit event and
  envelope head before rejecting the expired request or returning an identical
  reservation replay after expiry. Repeated expired admissions append no
  duplicate expiry transition and create no additional reservation.
- Crash recovery: an exact administrative rearm request proving a different
  runtime-session identity detects every durable `DispatchStarted`, commits
  each complete admitted maximum as `IndeterminateMaximum`, records the
  terminal reservation transition and audit, advances the envelope to
  `ReconciliationRequired` and rejects rearming in the same immediate
  transaction. Recovery remains available after the authority window expires
  and from an already persisted `Expired` state. Repetition is idempotent, new
  admission remains blocked and a synthetic failure on the second orphan proved
  that the complete multi-orphan transaction rolls back before a clean retry.
- Terminal-state precedence: a divergent replay against a non-armed envelope
  records one idempotent, sanitised `ReservationConflict` audit on the existing
  complete ledger revision and returns the matching terminal rejection. It does
  not reduce `ReconciliationRequired`, `Expired`, `Exhausted` or another
  dominant state to rearmable `Tripped`.
- Clean-restart restriction: same-session requests cannot classify a
  potentially live dispatch as orphaned; `Armed` remains ineligible for rearm;
  and pending `Reserved` or `DispatchStarted` attempts block rearming from
  otherwise permitted `Disarmed` or `Tripped`. The existing trigger and schema
  remain unchanged. Candidate `7b031a5` was inspected only through immutable
  Git objects, not integrated; its incompatible crash test was replaced.
- Focused verification: the first run passed 7/8 and exposed that an
  envelope-scoped expiry audit cannot bind a request without a reservation.
  The audit was corrected to retain the authority while omitting request-bound
  fields. A first independent review then found that reservation replay and
  recovery after expiry still bypassed the correction and requested an
  intermediate batch-failure proof. A second review found that a divergent
  replay could reduce `ReconciliationRequired` to rearmable `Tripped`. Those
  findings were corrected; the final focused class passed 14/14, and isolated
  format verification passed for both changed code files.
- Complete applicable .NET verification: Release build passed with zero
  warnings or errors; 227 unit, 314 integration and 11 architecture tests all
  passed; fail-closed merged coverage passed at 95.73% of lines
  (61,564/64,310) and 66.74% of branches (5,822/8,724). Solution-wide format
  verification retained the already recorded, out-of-scope import-ordering
  failure in `OpenAiHttpAdapters.cs`; that file was not changed and this result
  is not an Automatic Quality Gate.
- Versioning: the instruction corpus advances by factual `PATCH` from `4.18.1`
  to `4.18.2` without changing architecture, public contracts, persistence
  shape, lifecycle or external authority.
- Append-only integrity: this entry preserves the complete previous file of
  586,694 bytes at SHA-256
  `4d08845bcae5512305c9bb7c6174497766f03c7e9e9cf89503fc28451cc0c2ed`.
- Negative scope: no fetch, push, merge, rebase, amend, schema, migration,
  dependency, budget value, credential, provider, network, billing, Render,
  OCI, deployment, Automatic Quality Gate, Human Gate, lifecycle transition
  or remote action occurred.

## 2026-08-20 — Provider-budget recovery Automatic Quality Gate rejected

- Authority: `AUTH-SEC-BUDGET-001-RECOVERY-AQG-20260820-01` authorised only the
  local, offline and deterministic Automatic Quality Gate of the recovery
  corrective on exact `main@65275304a3d42727e95458f2bbb3db6cc6324d02`, corpus
  `4.18.2`, plus its factual evidence and at most one local documentary commit.
- Baseline and preflight: branch, HEAD, clean tracked/untracked tree, empty
  index, protected OpenAPI v1/v2 identities, absence of Git locks and absence
  of orchestrator locks matched the authority. The separate detached worktree
  remained untouched. Directed runtime preflight found zero RAG-Challenge-owned
  process and zero owned listener and stopped nothing. Candidate `7b031a5`
  remained outside the target ancestry.
- Independent review: a read-only reviewer inspected the complete
  `fb9328d8d0ec12304289cdee6275ac82c1927bec..65275304a3d42727e95458f2bbb3db6cc6324d02`
  diff, the accepted ADR-0018 boundary, the security owner and the regression
  matrix before any executable gate command.
- `AQG-SEC-BUDGET-RECOVERY-001` (`P1`, `OPEN`): divergent dispatch,
  commitment or release replay is handled before normal transition validation.
  The conflict path persists `Tripped` unconditionally, so an orphan recovered
  to `ReconciliationRequired` can be reduced to a rearmable state and later
  return to `Armed` through an otherwise exact rearm after its reservation is
  terminal. The existing regression covers divergent admission replay, not
  divergent transition replay.
- `AQG-SEC-BUDGET-RECOVERY-002` (`P2`, `OPEN`): the preserved-state admission
  conflict audit ID incorporates `RequestedAtUtc`, while replay binding
  equality excludes that instant. Repeating the same logical divergence with a
  new timestamp can append another `ReservationConflict` event and does not
  satisfy the recorded one-event idempotency claim. The existing regression
  repeats the same request object and timestamp.
- Mandatory stop and disposition: the gate stopped immediately after the
  static findings. `eng/ci.ps1 -Offline`, restore, format, build, tests,
  coverage, Dashboard and orchestrator checks were not executed. The Automatic
  Quality Gate is `REPROVADO`, with zero P0, one P1, one P2 and zero P3; no
  finding was corrected within the gate.
- Preserved boundaries: the corrective diff changes no schema, migration,
  OpenAPI or dependency. The operational budget remains zero and `Disarmed`.
  No credential, provider, network, billing, Render, OCI, deployment, Human
  Gate, lifecycle, fetch, push, merge, rebase or amend action occurred.
- Closed documentary scope: only Security-And-Access, Current State, Prompt
  System Change Log and this append-only EOF are changed. Source, tests, ADRs,
  schema, migrations, dependencies, workflows, branches, worktrees, runtime
  and external resources remain unchanged.
- Versioning: the instruction corpus advances by factual `PATCH` from `4.18.2`
  to `4.18.3` without changing architecture, executable behaviour, public
  contracts, operational authority, Human Gate or lifecycle.
- Append-only integrity: this entry preserves the complete previous file of
  591,601 bytes at SHA-256
  `2094296b21dd2bb0303669aae463c130d3ddada2b0fc2eb3c72644209538b129`.
- Next condition: obtain separate bounded corrective authority for both open
  findings. A complete AQG retest then requires a new explicit authority on an
  exact clean baseline; no correction or retest is inferred from this record.

## 2026-08-20 — Provider-budget recovery findings corrected locally

- Authority: `AUTH-SEC-BUDGET-001-RECOVERY-CORR-20260820-02` authorised only
  the local, offline, deterministic and sequential correction of
  `AQG-SEC-BUDGET-RECOVERY-001` and `AQG-SEC-BUDGET-RECOVERY-002` on exact
  `main@adcc0c553e8b203e74133f748f05aa33b4b1c79d`, corpus `4.18.3`, in
  `STATE-07 TESTING_HOMOLOGATION`.
- Baseline and preflight: branch, HEAD, clean tracked/untracked tree, empty
  index, corpus identity and protected OpenAPI v1/v2 SHA-256 identities
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`
  and `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733`
  matched the authority. No Git or orchestrator lock, RAG-Challenge-owned
  process or known product listener existed; the separate detached worktree
  remained untouched.
- Terminal-state correction: divergent dispatch, commitment and release replay
  against any non-`Armed` envelope now records the conflict and returns the
  terminal rejection before the `Tripped` persistence path. The ledger
  revision, envelope head, reservation and `ReconciliationRequired` state stay
  unchanged, so exact rearming remains rejected and no path to `Armed` reopens.
- Audit correction: admission and transition conflict fingerprints bind the
  logical replay divergence while excluding attempt timestamps and the current
  ledger revision. The stable audit identity records the first observation and
  appends at most one sanitised `ReservationConflict` for repeated attempts at
  different instants.
- Regression evidence: the focused provider-budget ledger class passed 17/17.
  Its matrix covers admission plus divergent dispatch, commitment and release,
  repeats each logical conflict with a different timestamp, asserts one audit,
  preserves state/revision/hash/reservation and proves that rearming remains
  rejected.
- Complete applicable verification: Release build passed with zero warnings or
  errors; all 227 unit, 317 integration and 11 architecture tests passed; and
  fail-closed merged coverage passed at 95.75% of lines (61,625/64,363) and
  67.03% of branches (5,856/8,736). Focused format, coverage and CI policy
  tests, all 105 language-policy tests, repository audit across 433 non-ignored
  files and `git diff --check` passed. Solution-wide format verification
  retained only the recorded, out-of-scope import-ordering failure in
  `OpenAiHttpAdapters.cs`; that file was not changed. No Automatic Quality Gate
  was executed.
- Independent review: the sequential read-only review confirmed the exact
  baseline and two-file executable diff and reported zero P0, zero P1, zero P2
  and zero P3. It executed no build, test, runtime, network or Git mutation.
- Disposition: both findings are locally `CORRECTED_PENDING_GATE_RETEST`. The
  prior Automatic Quality Gate remains historically `REPROVADO`; this
  correction neither reruns that gate nor advances Human Gate or lifecycle.
- Closed corrective scope: only the SQLite provider-budget ledger, its focused
  integration tests, Security-And-Access, Current State, Prompt System Change
  Log and this append-only EOF are changed. Schema, migration, OpenAPI,
  dependency, budget values, cost schedule, provider, credential, billing and
  workflows remain unchanged.
- Versioning: the instruction corpus advances by factual `PATCH` from `4.18.3`
  to `4.18.4` without changing architecture, public contracts, persistence
  shape, lifecycle or external authority.
- Append-only integrity: this entry preserves the complete previous file of
  595,145 bytes at SHA-256
  `5cc6c349c62fb3a20d40ca3a79fe976bc0a650e735ff5f4fb5a4fafdeda10312`.
- Runtime postflight: no RAG-Challenge-owned process or known product listener
  remained after verification; no process was stopped.
- Negative scope: no fetch, push, merge, rebase, amend, network, billing,
  Render, OCI, deployment, Automatic Quality Gate, Human Gate, lifecycle
  transition or remote action occurred.
- Next condition: a complete AQG retest requires new explicit bounded authority
  on the exact clean corrective baseline; no retest is inferred from this
  record.

## 2026-08-20 — Provider-budget recovery AQG retest stopped at protected language control

- Authority: `AUTH-SEC-BUDGET-001-RECOVERY-AQG-RETEST-20260820-01` authorised
  only the complete local, offline, deterministic and sequential Automatic
  Quality Gate retest of `AQG-SEC-BUDGET-RECOVERY-001` and
  `AQG-SEC-BUDGET-RECOVERY-002` on exact clean
  `main@79640d04301dd3c895862ffca2003387dbf188a7`, corpus `4.18.4`, plus factual
  evidence and at most one focused local documentary commit.
- Baseline and preflight: branch, HEAD, clean tracked/untracked tree, empty
  index, corpus identity, protected OpenAPI v1/v2, append-only prefix, absence
  of Git locks and zero active orchestrator locks matched the authority.
  Directed runtime preflight found no RAG-Challenge-owned process or known
  listener and stopped nothing. Blocked candidate `7b031a5` remained outside
  the target ancestry.
- Independent review: the sequential read-only reviewer inspected the complete
  `adcc0c553e8b203e74133f748f05aa33b4b1c79d..79640d04301dd3c895862ffca2003387dbf188a7`
  diff and reported zero P0, zero P1, zero P2 and zero P3. It confirmed that
  divergent admission, dispatch, commitment and release preserve dominant
  non-`Armed` state, that conflict identities remain stable across attempt
  timestamps, and that `ReconciliationRequired` cannot be rearmed.
- Canonical execution: `pwsh -NoProfile -File .\eng\ci.ps1 -Offline` ran exactly
  once from `2026-08-20T12:10:08.0884584Z` to
  `2026-08-20T12:10:57.1881887Z`, elapsed `49,100` ms and exited `1`. All 105
  language-policy tests passed. The following immutable commit inspection then
  returned `Language policy FAIL` because commit `79640d0` changes protected
  control `Security-And-Access.md` and requires exceptional manual review.
- Mandatory stop: CI/coverage policy tests, lockfile verification, restore,
  format, Release build, .NET tests, coverage, Dashboard, orchestrator,
  repository audit and `git diff --check` were not executed by the canonical
  run. The command was not repeated, no bypass was used and no source or test
  was corrected.
- Disposition: the Automatic Quality Gate is `REPROVADO`. No new P0-P3 finding
  was recorded, but the mandatory canonical stage did not pass;
  `AQG-SEC-BUDGET-RECOVERY-001` and `AQG-SEC-BUDGET-RECOVERY-002` therefore
  remain `CORRECTED_PENDING_GATE_RETEST`.
- Postflight: the tracked and untracked tree remained clean immediately after
  the failure, with no RAG-Challenge-owned process or known product listener.
- Closed documentary scope: only Current State, Prompt System Change Log and
  this append-only EOF are changed. Security policy, source, tests, schema,
  migrations, OpenAPI, dependencies, workflows, budget values and cost
  schedule remain unchanged.
- Versioning: the instruction corpus advances by factual `PATCH` from `4.18.4`
  to `4.18.5` without changing architecture, executable behaviour, public
  contracts, persistence, lifecycle or external authority.
- Append-only integrity: this entry preserves the complete previous file of
  599,319 bytes at SHA-256
  `a7ae42f2e3767d3eb796fe5dd709f50af1cb29dc03815e3711e233a7324364c2`.
- Negative scope: no fetch, push, merge, rebase, amend, source/test correction,
  provider, credential, network, billing, Render, OCI, deployment, Human Gate,
  lifecycle transition or remote action occurred.
- Next condition: a further complete AQG retest requires new explicit bounded
  authority on the exact clean factual-reconciliation baseline; no rerun or
  approval is inferred from the independent review.
