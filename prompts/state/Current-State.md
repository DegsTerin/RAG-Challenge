# Estado Atual

Este documento é o snapshot factual vigente do workspace em 2026-08-04. Ele
não concede autoridade. Evolução e decisões no contexto original pertencem ao
[`State-Transition-Log.md`](State-Transition-Log.md) e aos relatórios
proprietários.

## Lifecycle e gates

- Posição: `STATE-00 DISCOVERY` encerrado; `GATE-B01
  ARCHITECTURE_BOOTSTRAP_DECISION` aprovado e encerrado; `STATE-01
  PROJECT_SETUP` encerrado após Human Gate aprovado sem ressalvas em
  2026-07-31; entrada em `STATE-02 ARCHITECTURE` autorizada em 2026-07-31,
  com execução documental e local, sequencial, dos lotes `S02-A` e `S02-B`;
  `STATE-02` encerrado após Human Gate aprovado sem ressalvas em 2026-08-02;
  entrada em `STATE-03 DATA_AND_INDEX_MODELING` autorizada em 2026-08-02;
  `S03-A` e `S03-B0` a `S03-B5` concluídos; Automatic Quality Gate de
  `STATE-03` aprovado sem achados; `STATE-03` encerrado após Human Gate
  aprovado sem ressalvas em 2026-08-02; entrada em `STATE-04
  BACKEND_IMPLEMENTATION` autorizada e registrada em 2026-08-03. O
  proprietário autorizou em 2026-08-04 o fechamento de `S04-A0`, o pin
  offline dos dois parsers selecionados, a execução sequencial de `S04-A` a
  `S04-D` e, depois, o Automatic Quality Gate de `STATE-04`. `S04-A0` foi
  encerrado documentalmente. A precondição da fonte offline incompleta foi
  resolvida por seed somente leitura e allowlisted para um cache isolado; os
  pins foram aplicados, o restore locked passou e o primeiro gate runtime
  sintético de `S04-A` foi aprovado. `S04-A` está em execução sequencial.
- Fechamento de `S04-A0`: `PdfPig` `0.1.15` e `CsvHelper` `33.1.0` foram
  selecionados condicionalmente para desenvolvimento local;
  `Sylvan.Data.Csv` `1.4.4` permanece fallback não selecionado e não
  autorizável por substituição automática. O adapter OpenAI será HTTP direto,
  sem package `OpenAI` ou `System.ClientModel`. Hashes, gates, limitações,
  evidências, resolução do bloqueio pré-pin e gate runtime estão registrados no
  [relatório de STATE-04](../../docs/STATE-04-Backend-Implementation-Report.md).
  A assinatura dos parsers permanece
  `CONDITIONAL_REVOCATION_NOT_CURRENT`, e a semântica incompleta dos domínios
  de hash NuGet foi aceita somente para desenvolvimento local em `STATE-04`.
- Escopo concluído de `STATE-01`: registrar a entrada e executar localmente,
  de forma sequencial, os lotes `S01-A`, `S01-B` e `S01-C`, sem lógica RAG ou
  funcional. A autoridade adicional de 2026-07-30 permite exclusivamente
  acesso HTTPS a `https://registry.npmjs.org/` e
  `https://api.nuget.org/v3/index.json`, instalação local das dependências
  fixadas, lockfile npm, auditorias npm/.NET e loopback para smoke de health.
- Automatic Quality Gate documental: `APROVADO` para a baseline `3.4.0` que
  encerrou `STATE-00` e para o incremento transversal `3.5.0`, sem reabertura
  do gate; correções `3.5.1` a `3.5.4` também `APROVADAS`; migração de
  identidade `4.0.0`, correção factual `4.0.1` e padrão copiável `4.1.0`
  também `APROVADOS`. O incremento normativo de eficiência decisória foi
  registrado como `4.2.0` (`MINOR`), e sua reorganização semanticamente
  equivalente como `4.2.1` (`PATCH`), em 2026-08-01. A decisão de suporte
  bilíngue para consulta foi formalizada como corpus `4.3.0` (`MINOR`) na
  mesma data, e a decisão separada de suporte visual a `pt-BR` e `en-GB` foi
  formalizada como corpus `4.4.0` (`MINOR`). A seleção posterior dos temas
  `Light` e `Dark` foi formalizada como corpus `4.5.0` (`MINOR`); as auditorias
  incrementais foram `APROVADAS`. A remoção posterior dos tetos de 12 sistemas
  e 120 páginas foi formalizada como corpus `4.6.0` (`MINOR`), com validação
  documental direcionada. A reconciliação posterior do catálogo inicial de 51
  bancos, 9 categorias, 54 associações, PDF/CSV e recuperação unificada foi
  formalizada como corpus `4.7.0` (`MINOR`). A aceitação humana explícita e
  independente de ADR-0002 e ADR-0004 a ADR-0006 foi registrada como corpus
  `4.8.0` (`MINOR`). A auditoria combinada posterior foi executada sobre
  `main@a01a765d177efb6c4013c6846c5f54c8adbe7e0f` e resultou
  `REPROVADA`, com um achado P1, um P2 e um P3. Depois da aceitação e
  reconciliação do ADR-0007, a nova auditoria combinada sobre
  `main@3978a17201cf5f6ac4ddc189862736fc3646457b`, corpus `4.9.1`, resultou
  `APROVADA`, dispôs os três achados como `RESOLVIDOS` e não encontrou novo
  P0, P1, P2 ou P3. Nenhum desses incrementos transitou o lifecycle ou aceitou
  ADR por implicação.
- Human Gate de `STATE-00`: `APROVADO` sem ressalvas em 2026-07-30.
- `GATE-B01`: `APROVADO` sem ressalvas em 2026-07-30.
- Transição para `STATE-01 PROJECT_SETUP`: autorizada em 2026-07-30.
- Automatic Quality Gate de `STATE-01`: `APROVADO`; lockfiles, restore,
  format, build, testes, cobertura, Dashboard, auditorias, health em loopback,
  higiene e reprodução em clone limpo foram validados em 2026-07-30. O gate
  offline, o health smoke e a reprodução limpa foram repetidos sobre a
  baseline renomeada.
- Human Gate de `STATE-01`: `APROVADO` sem ressalvas em 2026-07-31.
- Transição para `STATE-02 ARCHITECTURE`: autorizada em 2026-07-31,
  exclusivamente nos limites, entregáveis, verificações, riscos, rollback e
  escopo negativo do resumo completo apresentado na conversa coordenadora.
  A autorização não aceita ADR por implicação nem concede rede, instalação,
  serviço pago, GitHub, OCI, publicação, deploy ou mudança no DB-Notifier.
- Execução de `STATE-02`: pacote documental sequencial de `S02-A` e `S02-B`
  preparado no commit `979677fa1f4d7324340b8be15d88eb8b5b802a1a` em
  2026-07-31, com contratos canônicos, threat model e relatório factual. A
  verificação pública
  autorizada foi registrada nos commits
  `f1066c3509f5f48d4fe6e21c9e36403e642c1431`,
  `e80f8c41bea3f28deff3d8cdccafccbca5dcc016` e
  `9cc62746ea2ba861676a2d5bfee317eaf66dad7c`: nenhum item de fonte primária
  pública permanece pendente no escopo autorizado. Fatos dependentes de
  conta, entitlement, capacidade ou execução continuam não verificados e
  exigem autoridade futura própria. Em 2026-08-01, o proprietário aceitou
  explicitamente e de forma independente ADR-0002 e ADR-0004 a ADR-0006 sobre
  `main@39e2f803bf73cb4e2b59e56a0596e2858a3aed51`, corpus `4.7.0`; nenhuma
  escolha decorreu de outra e nada foi implementado por implicação.
- Automatic Quality Gate de `STATE-02`: `APROVADO` após nova auditoria
  combinada da baseline reconciliada
  `main@3978a17201cf5f6ac4ddc189862736fc3646457b`, corpus `4.9.1`.
  `AQG-S02-001` (P1), `AQG-S02-002` (P2) e `AQG-S02-003` (P3), registrados
  historicamente pela auditoria anterior, foram dispostos como `RESOLVIDOS`;
  a nova auditoria não encontrou novo P0, P1, P2 ou P3.
- Human Gate de `STATE-02`: `APROVADO` sem ressalvas em 2026-08-02 sobre
  `main@6e61c4cf4429e2a62145d43bec3783146f01e37f`, corpus `4.9.1`, após
  revisão do relatório automático, das amostras críticas, limitações, riscos
  residuais, condições pendentes e escopo negativo. A decisão encerra somente
  `STATE-02` e não autoriza entrada em `STATE-03`.
- Transição para `STATE-03 DATA_AND_INDEX_MODELING`: autorizada em 2026-08-02
  sobre `main@35b67c194f6ea2459833420b8bc2143fadfe75df`, corpus `4.9.1` e
  working tree limpa. A autoridade permite registrar a entrada e executar
  localmente, de forma sequencial, somente `S03-A`: modelo e dicionário,
  identidades, estados, relações, constraints, revisões, serialização
  canônica, vetores de referência dos dois domínios de digest, três validações
  pré-CAS, invariantes de ativação/retenção/rollback e fixtures
  determinísticas. `S03-B`, migrations, stores persistentes, novas
  dependências e instalação permanecem sem autorização.
- Execução de `S03-A`: concluída localmente no commit
  `ace780a25edb2749046e9897b8af36e0719e3e54` com modelo lógico em Domain,
  construção e validação pré-CAS em Application, dicionário permanente,
  vetores canônicos executáveis, fixtures 51/54/9 e invariantes de staging,
  ativação, observação, retenção e rollback. Infrastructure, projetos,
  dependências e lockfiles não foram alterados; não existem migration ou
  store persistente.
- Verificação de `S03-A`: format e build Release sem restore aprovados; 68
  testes aprovados (53 unitários, 10 de arquitetura e 5 de integração), com
  cobertura de 95,55% de linhas e 89,93% de branches; lint, typecheck, 2
  testes e build do Dashboard também aprovados sobre a instalação existente;
  auditoria aprovada para 104 arquivos não ignorados e diff hygiene aprovado.
  O proprietário aceitou explicitamente Node.js `24.18.1` somente como
  variação local de verificação; os pins do repositório permanecem em
  `24.18.0`/npm `11.16.0`. O agregado `eng/ci.ps1 -Offline` não foi executado
  porque faria restore e `npm ci`, ações bloqueadas com `S03-B`.
- Autoridade de `S03-B`: registrada no commit
  `381d1cd297580476e461a242ce5b66c4884e521b` após repetição aprovada de
  `S03-B0`. O conjunto conservador de supply chain contém 42 nupkgs, todos
  verificados por SHA-512 do catálogo, repository signature, advisory e
  licença. O asset Linux ARM64 é ELF64 AArch64 e contém SQLite `3.53.3`.
- Reconciliação do fechamento: o restore real de `net10.0` materializou 40
  packages no `project.assets.json` e instalou separadamente a ferramenta
  local `dotnet-ef 10.0.10`, totalizando 41 itens. `System.Memory 4.5.3`,
  declarado somente no grupo `.NETStandard2.0` de `SQLitePCLRaw.core 2.1.12`,
  permaneceu evidência conservadora verificada e não foi pinado, referenciado
  ou materializado. O proprietário aceitou explicitamente essa distinção e
  autorizou retomar `S03-B1` na working tree interrompida.
- Execução de `S03-B1` a `S03-B4`: `S03-B1` concluiu locked restore com 40
  packages materializados, ferramenta `dotnet-ef 10.0.10`, ausência de
  `System.Memory` e exatamente quatro lockfiles afetados. `S03-B2` registrou
  os modelos físicos e migrations iniciais separados de `control.db` e
  `vectors.db`. `S03-B3` implementou ports em Application e stores locais em
  Infrastructure para autoridade de controle, vetores derivados, conteúdo
  imutável, CAS, retenção, cleanup e recuperação. `S03-B4` adicionou fixtures
  e testes determinísticos, inclusive concorrência, rollback, corrupção,
  recuperação e o caso funcional sintético de 10.000 vetores por 1.536
  dimensões. A finalização canônica calcula digests de especificação,
  artefatos lógicos e manifesto completo a partir do readback do SQLite.
- Execução de `S03-B5`: a divergência anterior de descoberta da migration de
  Control não se reproduziu após clean e build Release novos sobre
  `main@c72c8b967667f72e8971f4887174585d3640a36e`. A evidência é compatível
  com output incremental stale usado por `--no-build`, sem provar causa
  histórica mais profunda e sem exigir alteração de model, migration,
  snapshot ou contrato. Control e Vector passaram list, apply, rollback para
  zero, reapply e pending-model check em stores temporários separados, depois
  removidos. O agregado offline passou 82 testes .NET, cobertura de 94,83% de
  linhas e 72,34% de branches, lint, typecheck, dois testes e build do
  Dashboard, auditoria de 130 arquivos e diff hygiene. A consulta NuGet
  vigente não encontrou package vulnerável. `S03-B5` e S03-B estão concluídos;
  isso não executa Automatic Quality Gate nem Human Gate.
- Automatic Quality Gate de `STATE-03`: `APROVADO` sobre
  `main@3d0731fdf3f5004fb185dc760b5f74e4d73b4aa5`, corpus `4.9.1`, com zero
  achados P0, P1, P2 ou P3. Preflight encontrou zero processos/listeners do
  produto. O gate local e offline confirmou catálogo 51/54/9, dois domínios de
  digest, três validações pré-CAS, fronteiras arquiteturais, conteúdo
  reabrível, staging não consultável, CAS, retenção, rollback por nova revisão,
  recuperação, 40 packages materializados em Infrastructure sem
  `System.Memory`, migrations Control/Vector e baseline limpa. O agregado
  aprovou 82 testes, 94,83% de linhas, 72,34% de branches, Dashboard e
  auditoria de 130 arquivos. Nenhum arquivo rastreado foi alterado durante a
  coleta; os stores temporários foram removidos. Human Gate não foi executado.
- Human Gate de `STATE-03`: `APROVADO` sem ressalvas em 2026-08-02 sobre
  `main@a88dc1f296bb9117dd8e869b83d1665cee99634f`, corpus `4.9.1`, após
  revisão, na mesma conversa, do resumo completo da baseline vigente, dos
  entregáveis, resultados automáticos, limitações, riscos residuais, escopo
  negativo e rollback. A frase canônica foi `Confirmo a decisão acima
  exclusivamente para STATE-03`. A decisão encerra somente `STATE-03` e não
  autoriza entrada em `STATE-04` nem qualquer ação externa.
- Transição para `STATE-04 BACKEND_IMPLEMENTATION`: autorizada e registrada
  em 2026-08-03 sobre `main@e62fbc4da7e580dc1f5449689699374e42ea8ab4`,
  corpus `4.9.2` e working tree limpa. A autoridade permite somente atualizar
  o snapshot factual e o histórico append-only e criar o commit local focal
  desse registro. `S04-A`, `S04-B`, `S04-C` e `S04-D`, código, dependências,
  packages, lockfiles, migrations, contratos, ADRs, rede, providers, contas,
  secrets, corpus real, fontes oficiais, armazenamento operacional, Dashboard,
  GitHub, OCI, publicação, deploy e DB-Notifier permanecem sem autorização.
- Lote corretivo de `STATE-02`: sobre
  `main@9707b87d75a6acb14c8993ff0283a4221bc6c762`, corpus `4.8.0`, foi
  preparado o ADR-0007, recomendando separar identidade de geração
  e identidade do registro de ativação. As fontes de `AQG-S02-002` e
  `AQG-S02-003` foram reconciliadas factualmente, sem registrar aceitação do
  ADR, alterar os contratos semânticos aceitos ou repetir o Automatic Quality
  Gate. Nesse lote, o resultado do gate permaneceu `REPROVADO`.
- Decisão corretiva de `STATE-02`: em 2026-08-02, sobre
  `main@664187c6926be5ce4bef3734603f8d936626d535`, corpus `4.8.1`, o
  proprietário aceitou explicitamente o ADR-0007 com a decisão
  `ADR-0007: ACEITAR.`. A aceitação corrige a autoridade arquitetural de
  identidade/freshness e rollback, mas não autoriza nem executa a
  reconciliação semântica rastreada, não dispôs `AQG-S02-001` naquele registro
  e não repetiu o Automatic Quality Gate.
- Reconciliação semântica de `STATE-02`: em 2026-08-02, a partir de
  `main@9aa90c012e3bc973330f5a79678fc358c81809df`, corpus `4.9.0`, a
  semântica aceita do ADR-0007 foi aplicada transversalmente ao baseline
  documental como corpus `4.9.1`. Esse lote não repetiu o Automatic Quality
  Gate; o resultado então permaneceu `REPROVADO` e os achados ainda não
  receberam nova disposição.
- Nova auditoria combinada de `STATE-02`: em 2026-08-02, sobre a baseline limpa
  `main@3978a17201cf5f6ac4ddc189862736fc3646457b`, corpus `4.9.1`, todas as
  áreas documentais aplicáveis foram `APROVADAS`. Os dois domínios de digest,
  três validações pré-CAS, revalidação, hard pre-filter, rollback, proveniência,
  riscos e documentos roteados convergem; `AQG-S02-001` a `AQG-S02-003` estão
  `RESOLVIDOS`, sem novo achado classificado. O trabalho não implementou nem
  executou comportamento, não solicitou Human Gate e não autorizou `STATE-03`.
- ADR-0001: `superseded` pelo ADR-0003, após aceitação original no
  `GATE-B01`; ADR-0002: `accepted`; ADR-0003: `accepted` pela solicitação
  humana explícita de renomear o projeto para `RAG-Challenge`, incorporando
  sem alteração todas as decisões não relacionadas a nomenclatura do
  ADR-0001; ADR-0004, ADR-0005 e ADR-0006: `accepted` por decisões humanas
  explícitas e independentes em 2026-08-01; ADR-0007: `accepted` por decisão
  humana explícita em 2026-08-02.

## Baseline documental

- Os 20 arquivos da estrutura originalmente aprovada permanecem preservados;
  a política de idioma acrescentou o 21º documento público por incremento
  versionado, e o ADR-0003 acrescentou o 22º.
- A baseline aprovada no Human Gate de `STATE-00` permanece `3.4.0`.
- O corpus de instruções vigente possui versão `4.9.2` e 13 arquivos em
  `prompts/`.
- Visão, requisitos, arquitetura, RAG, segurança, qualidade, lifecycle,
  roadmap, backlog, estado, histórico e templates estão documentados.
- `STATE-02` acrescentou sete artefatos técnicos: quatro novos ADRs, um
  contrato canônico, um threat model e um relatório de execução. ADR-0002 e
  ADR-0004 a ADR-0007 estão aceitos. Os artefatos não são evidência de
  implementação.
- A auditoria do pacote proposto confirmou 83 arquivos não ignorados, 30
  Markdown, links e formato válidos, quatro ADRs com status `proposed`, 30 IDs
  de ameaça e 12 grupos de testes de segurança. As verificações posteriores
  reconciliaram os fatos públicos de fonte oficial, parser/package,
  provider/model e OCI sem resolver fatos dependentes de conta ou runtime e
  sem substituir decisões humanas.
- A auditoria do corpus `4.1.0` confirmou 22 documentos, 114 links locais
  válidos, 20 RF, 14 RNF, 15 critérios de aceitação, 31 itens de backlog, 8
  módulos, 13 riscos, formato consistente e rastreabilidade. Naquele snapshot,
  a implementação ainda estava limitada ao scaffold entregue pelo `STATE-01`
  encerrado.
- O corpus `4.2.0` acrescenta a `AGENTS.md` regras permanentes de eficiência
  decisória e proporcionalidade: identificar a entrega antes da coleta,
  separar fatos decisivos de contexto, calibrar profundidade ao risco,
  verificar candidatos em duas etapas, preferir autoridade limitada completa,
  parar por valor decrescente e preservar integralmente segurança, qualidade,
  lifecycle e autoridade explícita.
- O corpus `4.2.1` consolida ownership normativo sem alterar comportamento:
  Governance conserva a semântica de handoff, continuidade, raciocínio e
  paralelismo; Templates conserva o formato; Quality Gates conserva os
  resultados auditáveis; AGENTS mantém enforcement transversal mínimo; Start
  Here mantém roteamento; Language Policy conserva somente convenções de
  idioma.
- O corpus `4.3.0` formaliza a decisão explícita do proprietário de aceitar
  perguntas e respostas em `pt-BR` e `en-GB`: cada consulta declara o idioma,
  a resposta usa o mesmo idioma, textos derivados da fonte permanecem no
  idioma original da citação e a matriz de testes cobre os pares iguais e as
  duas direções cruzadas. A decisão não define o idioma da interface e não
  aceita os ADRs ainda propostos.
- O corpus `4.4.0` formaliza a decisão posterior e independente de suportar a
  interface em `pt-BR` e `en-GB`, com escolha visual explícita e localização
  dos textos pertencentes ao produto. O idioma visual não altera nem é
  inferido de `questionLanguage`, `answerLanguage` ou `contentLanguage`; as
  citações continuam no idioma original da fonte. Idioma inicial,
  persistência da preferência e fallback permanecem para decisão futura de
  frontend. A decisão não aceita os ADRs ainda propostos.
- O corpus `4.5.0` formaliza a decisão posterior de suportar os temas `Light`
  e `Dark`, com escolha explícita e independente dos idiomas da interface e da
  consulta. A matriz de quatro combinações entre `interfaceLanguage` e
  `questionLanguage` deve ser executada nos dois temas. Tema inicial,
  preferência do sistema, persistência e fallback permanecem para decisão
  futura de frontend. A decisão não aceita os ADRs ainda propostos.
- O corpus `4.6.0` formaliza a decisão independente de não impor teto de
  produto à quantidade de sistemas ou de páginas do corpus. Cada versão
  permanece finita e registra as contagens observadas; controles de segurança
  e capacidade são condicionais ao corpus e ao ambiente, não um recorte fixo
  de cobertura. A decisão não aceita o ADR-0004.
- O corpus `4.7.0` formaliza a decisão posterior de usar os 51 nomes exatos
  fornecidos pelo proprietário como catálogo inicial canônico, em 9 categorias
  e 54 associações muitos-para-muitos. Cada banco ativo exige ao menos um
  documento ativo PDF e/ou CSV; não há teto de documentos. Todos os documentos
  ativos/current participam da recuperação unificada, enquanto origem
  local/oficial permanece proveniência. Itens compatíveis são administráveis
  sem hard-code, código ou ADR por item; novas classes de integração conservam
  decisão própria. Nada foi implementado, ingerido, indexado ou ativado; nesse
  incremento, os quatro ADRs ainda permaneceram propostos.
- O corpus `4.8.0` registra a aceitação explícita e independente dos quatro
  ADRs sobre a baseline reconciliada. ADR-0005 conserva OCI, versões de
  packages e metas operacionais como condicionais, com backup consistente,
  instance principal somente-leitura, divulgação limitada à OpenAI e bloqueio
  de nova indexação diante de drift do alias mutável. A aceitação não instala,
  executa, testa ou autoriza nenhum desses componentes.
- A auditoria combinada autorizada de `STATE-02` confirmou a baseline
  mecânica — 83 arquivos não ignorados, 30 Markdown, 13 arquivos em
  `prompts/`, catálogo idêntico 51/54/9, 25 RF, 18 RNF, 20 critérios, 19 itens
  Must, 36 ameaças e 15 grupos de testes —, mas reprovou o gate por
  `AQG-S02-001` (P1), `AQG-S02-002` (P2) e `AQG-S02-003` (P3). Nenhum achado
  foi corrigido silenciosamente.
- O corpus `4.8.1` registra o pacote corretivo sem nova decisão arquitetural:
  ADR-0007 compara modelos de identidade e recomenda excluir
  `sourceObservationId` da geração, protegendo o binding completo por
  `activationBindingSetDigest`. Threat model, visão, arquitetura e segurança
  foram reconciliados com os fatos já aceitos para `AQG-S02-002` e
  `AQG-S02-003`. Nesse snapshot, a decisão ainda estava pendente; o gate não
  foi repetido e seus achados históricos não foram reclassificados por
  inferência.
- O corpus `4.9.0` registra a aceitação explícita do ADR-0007. A decisão torna
  autoritativa a separação entre identidade de geração e identidade do
  registro de ativação e substitui somente as cláusulas conflitantes de
  identidade e rollback do ADR-0002. Nesse snapshot, a reconciliação semântica
  ainda estava pendente e o gate continuava reprovado.
- O corpus `4.9.1` aplica a reconciliação aceita: `sourceBindingSetDigest`
  exclui `sourceObservationId`; `activationBindingSetDigest` protege o binding
  completo; `catalogueRevision` fica separado do journal de observações;
  `304`/hash idêntico preserva manifesto e geração; consulta filtra bindings
  elegíveis antes do top-k; rollback constrói registro novo com observações
  compatíveis e atualmente elegíveis. ADR-0002, contratos canônicos,
  arquitetura da solução, módulo RAG, requisitos, lifecycle, Quality Gates,
  roadmap, threat model e registros factuais agora convergem. A validação
  dirigida não repetiu o gate nem comprovou implementação. A nova auditoria
  combinada posterior dispôs `AQG-S02-001`, `AQG-S02-002` e `AQG-S02-003`
  como `RESOLVIDOS` e aprovou a baseline documental, sem comprovar
  implementação.
- O corpus `4.9.2` corrige o isolamento temático das respostas e handoffs:
  confirmação, esclarecimento ou follow-up restrito permanece dentro do pedido
  atual; `Próximo trabalho recomendado` não importa lifecycle, backlog ou
  melhoria opcional sem relação direta e usa ausência canônica quando não
  existir trabalho adicional pertinente. A correção não altera produto,
  lifecycle, autoridade ou estado executável.
- O Human Gate de `STATE-02` foi confirmado na mesma conversa que apresentou
  o resumo completo da baseline vigente `main@6e61c4c`, corpus `4.9.1`. A
  decisão aceitou a arquitetura documental sem ressalvas, preservou todas as
  limitações e riscos residuais declarados e não autorizou `STATE-03` ou ação
  externa.
- A arquitetura adota princípios compatíveis com o DB-Notifier sem criar
  referência ou dependência entre os projetos.
- O Human Gate de `STATE-00` foi confirmado na conversa coordenadora que
  continha o resumo completo da baseline `3.4.0`; a decisão não aceita ADR,
  não decide `GATE-B01` e não autoriza `STATE-01`.
- O `GATE-B01` foi confirmado na conversa coordenadora que continha o resumo
  completo vigente. A decisão aceitou o ADR-0001, selecionou a licença MIT
  com o aviso exato
  `Copyright (c) 2026 Bruno Araújo - DegsTerin.`, consolidou RAG abstractions
  em Application e persistence em Infrastructure, aprovou o mapa
  `CH-MOD-*`, as dependências/testes arquiteturais e o modo administrativo
  one-shot no host principal.
- A aprovação de `GATE-B01` não criou licença, solution ou projetos, não
  aceitou o ADR-0002 e não autorizou `STATE-01`.
- Git local existe. A reorganização de governança `4.2.1` foi executada
  sequencialmente na conversa coordenadora, sem lanes paralelas; aquela
  execução foi exclusivamente documental e seu runtime preflight permaneceu
  `NÃO APLICÁVEL`.
- A política, o enforcement, o roteamento, os critérios e os templates de
  continuidade permanecem vigentes em suas autoridades temáticas; o snapshot
  não os redefine.

## Workspace

- `.gitignore` exclui `reference-materials/`.
- `reference-materials/` preserva 24 arquivos locais: 23 materiais originais
  do Challenge e 1 prompt genérico de governança arquivado sem alteração.
- `reference-materials/challenge-original/` mantém 8 Markdown, 14 PDFs e 1
  PNG.
- Os materiais originais não são o corpus do produto e não serão enviados ao
  GitHub.
- Existe repositório Git local inicializado na branch `main`; o scaffold está
  no commit `16aec5f8586f07c9a9d89165e330335b460d6fbf` e o lockfile npm no
  commit `8a604ceaa34162673aea6b7ce3267bc9d3f8b83a`; a migração técnica de
  identidade está no commit
  `8c347c0fa73fead3e03a1eb979deba9fe3617379`.
- Existem `RAG-Challenge.sln`, quatro projetos .NET de produção sob o prefixo
  `RagChallenge`, um boundary React/TypeScript para o Dashboard e três
  projetos .NET de testes, conforme o ADR-0003. Domain e Application contêm o
  modelo, as identidades canônicas e os ports de persistência; Infrastructure
  contém a persistência SQLite local de `S03-B`. Não existe fluxo funcional de
  ingestão, recuperação RAG, geração ou API do `STATE-04`.
- SDK .NET `10.0.302`, C# `14.0`, Node.js `24.18.0` e npm `11.16.0` estão
  fixados. NuGet usa gestão central e sete lockfiles reproduzidos offline.
- O gate histórico de setup aprovou restore .NET offline locked, format,
  build Release, 15 testes e cobertura de 88% de linhas/100% de branches.
  `S03-A` foi verificado sem restore ou instalação: 68 testes aprovados e
  cobertura de 95,55% de linhas/89,93% de branches.
- `S03-B5` repetiu o agregado offline com 82 testes aprovados, cobertura de
  94,83% de linhas/72,34% de branches e migrations Control/Vector sem mudança
  pendente. O CI normaliza explicitamente para LF somente os sete lockfiles
  NuGet rastreados que o restore pode reserializar no Windows.
- O Dashboard possui `package-lock.json` v3 e passou clean install sem
  lifecycle scripts, lint, dois testes estruturais, typecheck e build Vite.
- As auditorias npm e .NET não encontraram vulnerabilidades nas fontes atuais.
- O clone limpo da baseline renomeada, sem `reference-materials/`, reproduziu
  restore locked, format, build, 15 testes, cobertura, Dashboard e higiene;
  liveness e readiness responderam `200` em loopback, e o listener pertencente
  ao projeto foi encerrado.
- O clone temporário dessa reprodução permanece no diretório temporário do
  sistema porque a política de execução recusou sua remoção recursiva. Ele não
  contém `reference-materials/`, secret ou mudança não rastreada.
- O diretório físico do checkout, externo ao Git, foi renomeado manualmente
  para `RAG-Challenge`; não existe um diretório irmão `Challenge`.
- As sete árvores técnicas legadas, que continham zero arquivos, foram
  removidas após validação dos alvos. As 15 raízes ignoradas de build e teste
  que conservavam o path absoluto anterior também foram removidas como
  artefatos reproduzíveis.
- Verificações .NET posteriores recriaram transitoriamente 14 raízes
  canônicas `bin/` e `obj/`, sem o path anterior; uma segunda passagem
  removeu essas saídas. No snapshot final, `bin/`, `obj/` e `TestResults/`
  estão ausentes nos sete projetos.
- Nenhum arquivo ou path técnico ativo conserva o prefixo ou o path absoluto
  anterior. `reference-materials/` permaneceu ignorado e preservou
  integralmente seus 24 arquivos; usos históricos, externos e de proveniência
  permanecem deliberadamente inalterados.
- O pipeline CI está definido localmente, com menor privilégio e sem deploy;
  não foi executado no GitHub.
- O arquivo `LICENSE` materializa a licença MIT aprovada.
- Não existem API funcional, ingestão, recuperação, banco persistente, vector
  store, corpus real, container, infraestrutura operacional ou deploy.
- Nenhum recurso OCI ou GitHub foi criado ou alterado.
- O DB-Notifier permaneceu somente leitura.

## Escopo corrente do produto

- Aplicação RAG independente para documentação de bancos de dados.
- MVP com um corpus lógico, catálogo inicial de 51 bancos, 9 categorias e 54
  associações, administrável por registros e sem hard-code.
- Cada banco ativo possui ao menos um documento ativo PDF/CSV e pode possuir
  qualquer quantidade adicional.
- Nenhum teto de produto para quantidade de sistemas ou páginas do corpus;
  cada versão finita registra suas contagens e precisa caber com segurança no
  ambiente homologado sem redução silenciosa do catálogo aprovado.
- Recuperação unificada de todos os documentos ativos/current; origem local ou
  oficial permanece explícita em proveniência, cobertura e citações.
- Sincronização manual e governada de cada fonte oficial registrada para
  snapshot versionado.
- Fontes oficiais públicas sem credenciais, egress deny-by-default por URL
  exata e validação TLS sem destinos laterais não autorizados.
- Resposta grounded com citações e evidência insuficiente explícita.
- Perguntas e respostas com idioma explícito `pt-BR` ou `en-GB`, resposta no
  idioma da pergunta e citações preservadas no idioma original da fonte.
- Interface com seleção explícita entre `pt-BR` e `en-GB`, independente do
  idioma da pergunta, da resposta e da evidência.
- Interface com seleção explícita entre `Light` e `Dark`, independente dos
  idiomas da interface, da pergunta, da resposta e da evidência.
- Ciclo Candidate/Active/Deactivated/Removed para bancos e documentos,
  versionamento manual e nova geração candidata.
- Conteúdo bruto imutável/reabrível, staging não consultável, manifesto final
  íntegro, digest generation-bound separado do digest completo de ativação e
  ativação/rollback por nova revisão do registro completo versionado.
- Contrato HTTP/OpenAPI v1 versionado pertencente ao RAG-Challenge; adapters
  consumidores permanecem fora deste repositório.
- Execução local e futuro deploy OCI.
- GitHub Pages somente como frontend estático opcional.

## Capacidades futuras inativas

- múltiplos acervos;
- formatos além de PDF e CSV;
- sincronização incremental agendada;
- crawling genérico e novas classes de fonte/autenticação;
- múltiplos providers;
- RBAC/multi-tenancy;
- integração ao DB-Notifier.

Nenhuma dessas capacidades está implementada, testada, implantada ou
autorizada.

## Evidências e decisões futuras pendentes

1. Fornecer ou autorizar documentos PDF/CSV, direitos, proveniência e idioma
   para cada banco antes de sua ativação.
2. Validar e ativar individualmente novos registros de fonte oficial; a
   aceitação arquitetural não autoriza URL, rede, download ou crawling.
3. Concluir `S04-A` a `S04-D` sequencialmente sob o envelope autorizado e
   executar o Automatic Quality Gate somente após os quatro lotes.
4. Verificar tier, entitlement, spend limit e controles da conta OpenAI, além
   da recuperação/geração bilíngue, antes de usar ou anunciar os providers.
5. Homologar desempenho e capacidade do `SqliteExactVectorStore`; a fixture
   funcional de 10.000 × 1.536 passou, mas não é benchmark, SLA ou teto de
   produto.
6. Testar process-crash boundaries no `STATE-07`;
   migrations, concorrência, corrupção, backup e recuperação isolada já
   possuem evidência local sintética, sem representar armazenamento
   operacional.
7. Verificar capacidade, entitlement, IAM, restore, custo e cobrança reais da
   tenancy OCI; as fontes públicas ainda divergem sobre a franquia gratuita.
8. Materializar `rag-eval-catalogue-v1`, a rubrica e thresholds antes de cada
   execução pontuada, preservando a matriz `pt-BR`/`en-GB` aceita.
9. Decidir no `STATE-05` idioma inicial da interface, persistência da
   preferência e fallback; o conjunto `pt-BR`/`en-GB` já está decidido.
10. Decidir no `STATE-05` tema inicial, eventual preferência do sistema,
    persistência e fallback; o conjunto `Light`/`Dark` já está decidido.

## Próxima autoridade

`STATE-04 BACKEND_IMPLEMENTATION` está ativo. O fechamento documental de
`S04-A0`, o pin offline de `PdfPig` `0.1.15` e `CsvHelper` `33.1.0`, a
execução sequencial de `S04-A` a `S04-D` e o Automatic Quality Gate posterior
foram autorizados pelo proprietário em 2026-08-04. A fonte offline isolada foi
completada por cópia somente leitura e allowlisted das identidades e versões
já fixadas, sem alterar o cache global. `PdfPig` `0.1.15` e `CsvHelper`
`33.1.0` foram fixados com grafo aplicável vazio, restore locked e hashes
aprovados; o primeiro gate runtime sintético passou. `S04-A` pode continuar e
deve preceder estritamente `S04-B`, `S04-C`, `S04-D` e o Automatic Quality
Gate.

Nenhuma nova autoridade é necessária para o restante desse envelope enquanto
baseline, packages, versões, arquitetura, contratos e escopo negativo
permanecerem reconciliados. Qualquer condição de parada expressa exige nova
decisão humana.

Rede, providers, contas, secrets, corpus real, fontes oficiais reais do
produto, armazenamento operacional, GitHub, OCI, Dashboard, publicação,
deploy, Human Gate, estados posteriores e mudanças no DB-Notifier continuam
sem autorização.
