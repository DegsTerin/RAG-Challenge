# Estado Atual

Este documento é o snapshot factual vigente do workspace em 2026-08-06. Ele
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
  sintético de `S04-A` foi aprovado. `S04-A` concluiu administração, ingestão
  PDF/CSV, sync por transporte falso, snapshots, chunks e idempotência;
  `S04-B` concluiu embeddings por port com fake determinístico, staging,
  finalização canônica, commit, ativação CAS, hard pre-filter e replay
  idempotente; `S04-C` concluiu recuperação sobre uma única revisão ativa,
  elegibilidade/freshness, recusa, resposta grounded, citações e a matriz
  `pt-BR`/`en-GB`; `S04-D` concluiu API pública v1, OpenAPI versionado,
  health fail-closed, Problem Details, limites, cancelamento, rate limit e os
  adapters OpenAI por HTTP direto exercitados somente com handler falso. Os
  quatro lotes estão concluídos. O Automatic Quality Gate de `STATE-04` foi
  aprovado sem achados abertos; `AQG-S04-001` (P2) foi resolvido por um teste
  integrado do fluxo sintético completo. O Human Gate de `STATE-04` foi
  aprovado com as ressalvas documentadas em 2026-08-04; `STATE-04` está
  encerrado.
- Uma auditoria local posterior ao Human Gate, sobre
  `main@f71343291b942c66d0ff417a8764b032bbd63bff`, identificou os achados
  `AUD-S04-001` a `AUD-S04-004`. O proprietário autorizou o incremento
  corretivo consolidado `S04-CORR-01`: rebinding transacional de observações
  em `304`/hash idêntico (`a674560ed1093e96d533012f1b11a292c3f641b5`),
  chunking integral `paragraph-window-v1`
  (`b875eac6e9ce4c72783d4e4bb72a59686ca58248`), administração one-shot
  governada com journal durável (`ac34c085a499a34ea8ee1c9106675482e38790c3`)
  e esta reconciliação documental. As correções executáveis estão
  implementadas e os documentos factuais foram reconciliados. O Automatic
  Quality Gate corretivo foi aprovado sobre
  `main@114ea6f7f76936dac991553588660fc986bd0f10`; a disposição posterior dos
  quatro achados integra o resultado consolidado abaixo. Isso não reabriu o
  lifecycle, não alterou o Human Gate histórico e não autorizou `STATE-05`.
- A retomada posterior da auditoria identificou `AUD-S04-005` a
  `AUD-S04-009`. `S04-CORR-02` implementou alcance global antes da limpeza,
  replay exato nos domínios persistidos, validação e falhas tipadas dos
  adapters OpenAI, classificação administrativa por fase e reconciliação de
  comentários. A nova passagem encontrou o residual `AUD-S04-005-R1` na
  recuperação de uma reserva após crash. `S04-CORR-03`, no commit
  `19889f560dad0f011006ff17fc7414c807838149`, adicionou o plano interno
  versionado e a reconciliação transacional das reservas antes do planejamento
  e da finalização. Seu Automatic Quality Gate foi aprovado com 169 testes,
  92,04% de linhas e 66,46% de branches. A auditoria completa reiniciada foi
  `APROVADA`, sem novo P0, P1, P2 ou P3, e dispôs `AUD-S04-001` a
  `AUD-S04-009`, incluindo `AUD-S04-005-R1`, como `RESOLVIDOS`. O lifecycle e
  o Human Gate histórico não foram alterados; `STATE-05` permaneceu sem
  autorização naquele resultado.
- Entrada em `STATE-05 FRONTEND_IMPLEMENTATION`: registrada documentalmente
  em 2026-08-04. Depois, sobre
  `main@cab336ada60866083f3e688fe1a13cff348a3335`, corpus `4.9.2` e working
  tree limpa, o proprietário autorizou a execução local, offline, sequencial e
  limitada de `S05-A0` a `S05-A4`. Os cinco lotes foram concluídos nos commits
  `9c27cc49442ff467486c93febf7144e6d3a652b7`,
  `2fd7526f0907361d6c03552379341b877e88c236`,
  `7a42d332ddf6646c575c7cae16cfe9085120e18d`,
  `a8835b94ab485e542f7cfe23355283c92de17fc8` e
  `5865a225cdab9bd92f9befa00c7ee581b2aa0877`. O Dashboard implementa o
  contrato cliente v1 existente, estados, consulta same-origin, localização
  `pt-BR`/`en-GB`, temas `Light`/`Dark`, cobertura, proveniência, citações,
  falhas seguras e acessibilidade dentro do escopo aprovado. As verificações
  finais aprovaram lint, typecheck, 28 testes offline e build. O Automatic
  Quality Gate, o Human Gate e `STATE-06` não foram autorizados nem executados.
- Automatic Quality Gate de `STATE-05`: autorizado e iniciado em 2026-08-05
  sobre `main@f6df67a67657af891e4831a616b142d8da9fb584`, corpus `4.9.2` e
  working tree limpa. A auditoria parou conforme a condição do proprietário e
  resultou `REPROVADO` com `AQG-S05-001` (P1): o cliente aceita
  `canonicalUrl` com scheme `javascript:` em citação `LocalAuthorised` e a
  apresenta como link. A reprodução local em memória confirmou que o decoder
  aceitou o payload e o SSR emitiu o `href` inseguro. Nenhuma correção, mudança
  de produto, instalação ou ação externa foi executada; lint, typecheck,
  testes, build e browser do gate não foram alcançados depois do achado. O
  Human Gate e `STATE-06` permanecem sem autorização e sem execução.
- Correção `S05-CORR-01`: autorizada e concluída em 2026-08-05 sobre
  `main@7ee2241049dc68f16a38e85bd622928e64a317e7`, corpus `4.9.2` e working
  tree limpa. O commit `654fce6e0a09d6e7196e434de0ff6f5d6ccd5b04`
  rejeita qualquer URL de citação não HTTPS, exige `canonicalUrl` nula para
  `LocalAuthorised`, limita links apresentados a `OfficialExternal` com HTTPS
  validado e adiciona regressões de contrato e apresentação. Lint, typecheck,
  29 testes e build passaram; package, lockfile, OpenAPI, contratos externos e
  backend permaneceram inalterados. `AQG-S05-001` está
  `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`; a correção não repetiu nem aprovou
  o Automatic Quality Gate.
- Reinício integral do Automatic Quality Gate de `STATE-05`: autorizado e
  iniciado em 2026-08-05 sobre
  `main@f7e7f4a9d4afd234c9f3fcc725e7093653bc3363`, corpus `4.9.2` e working
  tree limpa. A inspeção estática confirmou a barreira implementada para
  `AQG-S05-001`, mas a condição de parada ocorreu antes do reteste npm e sua
  disposição permanece `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`. O reinício
  resultou `REPROVADO` com `AQG-S05-002` (P2), porque o limite do corpo HTTP é
  aplicado somente depois de `response.text()` materializar toda a resposta,
  e `AQG-S05-003` (P2), porque o título visual do documento permanece fixo em
  inglês quando `interfaceLanguage` é `pt-BR`. Preflight executável, lint,
  typecheck, testes, build, cobertura percentual, browser, viewport estreito,
  acessibilidade visual, teclado e matriz das oito combinações não foram
  alcançados. Nenhum código, teste, listener ou configuração foi alterado.
- Correção `S05-CORR-02`: autorizada e concluída em 2026-08-05 sobre
  `main@651b4ad9edba79b3fc8a16e550fc2a357b6b85d2`, corpus `4.9.2` e working
  tree limpa. O commit `ec5ecf41b113853fc2863a94cbfe77dbe4741828`
  aplica o teto de 262.144 bytes durante a leitura da resposta, antecipa a
  rejeição de `Content-Length` decimal excedido, interrompe no primeiro
  overflow e preserva cancelamento. O commit
  `20458c8189b132b775786b2fc8f9b44ee5c2f7b8` localiza o título visual por
  `interfaceLanguage`, sem acoplamento ao idioma da pergunta ou ao tema.
  Lint, typecheck, 34 testes e build passaram; a validação loopback confirmou
  os títulos `pt-BR` e `en-GB`, e o listener foi encerrado. Package, lockfile,
  OpenAPI, contratos externos e backend permaneceram inalterados.
  `AQG-S05-001`, `AQG-S05-002` e `AQG-S05-003` estão corrigidos, mas pendem de
  reteste e disposição por um reinício integral do Automatic Quality Gate
  sob autoridade humana posterior e separada.
- Reinício integral do Automatic Quality Gate após `S05-CORR-02`: autorizado
  e iniciado em 2026-08-05 sobre
  `main@3f120aaf3cbc199c821685b161ece95a1988a659`, corpus `4.9.2` e working
  tree limpa. A inspeção estática confirmou as barreiras implementadas para
  `AQG-S05-001`, `AQG-S05-002` e `AQG-S05-003`, mas encontrou
  `AQG-S05-004` (P2): o backend canônico emite `sourceFreshness: "Local"` para
  citações `LocalAuthorised`, enquanto o Dashboard não localiza `Local` e
  apresenta o fallback de estado desconhecido; a fixture local usa
  incorretamente `Current` e mascara a divergência. A condição de parada foi
  acionada antes do preflight executável, lint, typecheck, testes, build,
  cobertura e browser. Nenhum código, teste, processo, listener ou
  configuração foi alterado. O gate foi `REPROVADO`; os quatro achados
  permanecem sem disposição final por um gate completo aprovado.
- Correção `S05-CORR-03`: autorizada e concluída em 2026-08-05 sobre
  `main@800e6dc92d2a3555dbe92bc4e3b6b16e6411726b`, corpus `4.9.2` e working
  tree limpa. O commit `9ef937744302044ee3cd9105c9a23ddd3557a861`
  restringe `sourceFreshness` ao conjunto canônico, aceita
  `LocalAuthorised` somente com `Local` e URL nula, rejeita `Local` para
  `OfficialExternal`, localiza `Local` em `pt-BR` e `en-GB` e corrige a
  fixture sintética. Lint, typecheck, 35 testes e build passaram; a validação
  loopback confirmou a alternância localizada e terminou sem listener.
  Package, lockfile, OpenAPI, contratos externos e backend permaneceram
  inalterados. `AQG-S05-001` a `AQG-S05-004` estão corrigidos, mas pendem de
  reteste e disposição por um reinício integral do Automatic Quality Gate sob
  autoridade humana posterior e separada.
- Reinício integral do Automatic Quality Gate após `S05-CORR-03`: autorizado
  e iniciado em 2026-08-05 sobre
  `main@b457970aed4564d5a654bb4e8d38439c98f29522`, corpus `4.9.2` e working
  tree limpa. A inspeção estática confirmou as barreiras implementadas para
  `AQG-S05-001` a `AQG-S05-004`, mas encontrou `AQG-S05-005` (P2): o cliente
  aceita uma conclusão cujo `answerLanguage` é um idioma suportado diferente
  do `questionLanguage` enviado. A condição de parada foi acionada antes do
  preflight executável, lint, typecheck, testes, build, cobertura e browser.
  Nenhum código, teste, processo, listener ou configuração foi alterado. O
  gate foi `REPROVADO`; os quatro achados corrigidos continuam pendentes de
  reteste executável e `AQG-S05-005` permanece aberto.
- Correção `S05-CORR-04`: autorizada e concluída em 2026-08-05 sobre
  `main@fb59861a8367749f2a11ac279add5007989d27e0`, corpus `4.9.2` e working
  tree limpa. O commit `bed8ec03d670ed4e76a556f7df723c30db320a24`
  exige que `answerLanguage` corresponda ao `questionLanguage` efetivamente
  enviado e faz o cliente falhar fechado nas duas direções incompatíveis. As
  fixtures e regressões de contrato e transporte cobrem conclusões válidas em
  `pt-BR` e `en-GB`; o teste de limite exato não aceita mais a divergência.
  Lint, typecheck, 37 testes e build passaram na instalação existente e
  offline. Package, lockfile, OpenAPI, contratos externos e backend
  permaneceram inalterados. `AQG-S05-001` a `AQG-S05-005` estão corrigidos,
  mas pendem de reteste e disposição por um reinício integral do Automatic
  Quality Gate sob autoridade humana posterior e separada.
- Reinício integral do Automatic Quality Gate após `S05-CORR-04`: autorizado
  e iniciado em 2026-08-05 sobre
  `main@a58c4038fb14e656c95303d914e02c7f8ad75c17`, corpus `4.9.2` e working
  tree limpa. Inspeção estática, lint, typecheck, 37 testes, build e repetição
  byte a byte do build passaram; `AQG-S05-001` a `AQG-S05-005` foram
  dispostos como `RESOLVIDOS`. A validação de teclado encontrou
  `AQG-S05-006` (P2): o skip link recebe foco visível e altera o fragmento
  para `#main-content`, mas não transfere o foco ao `<main>`; o elemento ativo
  volta a ser `<body>`, sem oferecer bypass de foco confiável. A condição de
  parada foi acionada antes de viewport estreito/reflow, alternância completa
  Light/Dark e matriz browser das oito combinações. O gate foi `REPROVADO`.
  O listener pertencente à tarefa escutou somente em `127.0.0.1:4173`, foi
  identificado e encerrado; a porta terminou livre. Nenhuma correção, mudança
  de frontend, código, teste, dependência, contrato ou backend foi executada.
- Correção `S05-CORR-05`: autorizada e concluída em 2026-08-05 sobre
  `main@3ff7002b394199bbf253139836827231c1988116`, corpus `4.9.2` e working
  tree limpa. O commit `8b543eb85907b5aa4023f109dabb4bb11100da3e`
  torna `main#main-content` programaticamente focável, transfere o foco ao
  alvo quando o skip link é ativado e adiciona regressão focal de componente.
  Lint, typecheck, 38 testes e build passaram na instalação existente e
  offline. No build loopback, `Tab` focou o skip link, `Enter` focou o
  `<main>` e o `Tab` seguinte avançou ao rádio selecionado de idioma da
  pergunta dentro do conteúdo principal; não houve warning ou erro no console.
  O listener pertencente à tarefa foi revalidado e encerrado, e a porta
  terminou livre. `AQG-S05-006` está
  `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`; a correção não reiniciou nem
  aprovou o Automatic Quality Gate.
- Reinício integral do Automatic Quality Gate após `S05-CORR-05`: autorizado
  e executado em 2026-08-05 sobre
  `main@8ee1213eed3522493204c68b4f843e9c438e0f69`, corpus `4.9.2` e working
  tree limpa. Inspeção estática, lint, typecheck, 38 testes, build e repetição
  byte a byte do build passaram; `AQG-S05-001` a `AQG-S05-006` foram
  dispostos como `RESOLVIDOS`. A matriz browser padrão aprovou as oito
  combinações de idioma da interface, idioma da pergunta e tema. Na matriz
  estreita, as quatro combinações `pt-BR` produziram overflow horizontal a
  320 CSS px (`scrollWidth` 355 para `clientWidth` 303), enquanto as quatro
  combinações `en-GB` não produziram overflow. `AQG-S05-007` (P2) registra a
  falha de reflow da interface portuguesa. A condição de parada foi acionada
  sem correção. O gate foi `REPROVADO`, sem novo P0/P1. O listener pertencente
  à tarefa escutou somente em `127.0.0.1:4173`, foi revalidado e encerrado, e
  a porta terminou livre.
- Correção `S05-CORR-06`: autorizada e concluída em 2026-08-05 sobre
  `main@c32953eceb149efa3cfeb952f1dbfdbe0c00e2eb`, corpus `4.9.2` e working
  tree limpa. O commit `e34e73c7bbe8fabf96d5a5683df35935a3266e37`
  mantém reduzível a coluna única da hero e limita a escala tipográfica do H1
  no breakpoint compacto. A regressão focal cobre as oito combinações de
  idioma da interface, idioma da pergunta e tema. Lint, typecheck, 38 testes
  e build passaram offline. Em Chrome temporário com extensões desativadas e
  zero alvo de extensão, as oito combinações passaram a 320 CSS px com
  `scrollWidth` e `clientWidth` iguais a 305; reflow visual, Light/Dark e a
  sequência completa de foco e teclado foram preservados. Os listeners da
  tarefa foram encerrados e as portas terminaram livres. `AQG-S05-007` está
  `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`; o Automatic Quality Gate não foi
  reiniciado nem aprovado.
- Reinício integral do Automatic Quality Gate após `S05-CORR-06`: autorizado
  e iniciado em 2026-08-05 sobre
  `main@bc2ddd6bf64fc82f7d68eb518c3013d85655c16a`, corpus `4.9.2` e working
  tree limpa. A inspeção estática repetiu autoridade, lifecycle, escopo,
  contratos e segurança e encontrou `AQG-S05-008` (P2): resposta, título e
  trecho de citação derivados da API aceitam tokens contínuos válidos pelo
  contrato, mas suas superfícies de apresentação não permitem a quebra desses
  tokens no viewport estreito. A condição de parada foi acionada antes do
  preflight executável, checks npm, build e browser; nenhum processo ou
  listener foi iniciado. `AQG-S05-001` a `AQG-S05-006` conservam a disposição
  `RESOLVIDOS`; `AQG-S05-007` permanece
  `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`, pois o reteste executável não foi
  alcançado. O gate foi `REPROVADO`, sem novo P0/P1 e sem correção de produto
  ou teste.
- Correção `S05-CORR-07`: autorizada e concluída em 2026-08-05 sobre
  `main@dfa31d02e8ba3fd171986ea2c1d06c70101d07a3`, corpus `4.9.2` e working
  tree limpa. O commit `3f003b9db67eefeccc7e677c319ca37a26d49fa7`
  aplica quebra segura, sem truncamento, à resposta, ao título e ao trecho de
  citação e amplia a regressão das oito combinações com tokens contínuos
  válidos pelo decoder. Lint, typecheck, 38 testes e build passaram offline.
  A primeira tentativa headless ativou por erro do harness a URL oficial
  sintética antes de confirmar o foco, gerando acesso externo não autorizado;
  a tarefa parou, encerrou os runtimes e informou o proprietário. Após o
  proprietário permitir a continuação headless, a repetição controlada usou
  somente citação local sem URL, bloqueio de qualquer requisição não loopback
  e guarda do elemento ativo antes de `Enter`. As oito combinações passaram a
  320 CSS px com documento 305/305, tokens intactos e refluídos, foco, teclado,
  idiomas e temas preservados, zero alvo de extensão e zero tentativa ou URL
  externa. Chrome e preview foram encerrados e as portas ficaram livres. A
  política recusou excluir quatro diretórios temporários; o primeiro perfil
  pode conservar cache da navegação acidental. `AQG-S05-008` está
  `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`; o gate não foi reiniciado.
- Reinício integral do Automatic Quality Gate após `S05-CORR-07`: autorizado
  e concluído em 2026-08-05 sobre
  `main@97ea076da84d7afdb3330aa05dcb39fc7b44ce0f`, corpus `4.9.2` e working
  tree limpa. Autoridade, lifecycle, escopo, contratos e segurança foram
  reinspecionados; lint, typecheck, 38 testes, build e repetição byte a byte do
  build passaram. Chrome headless `151.0.7922.75`, em perfil temporário sem
  extensões, executou as oito combinações a 1280 CSS px e novamente a 320 CSS
  px. A fixture continha somente citação local sem URL, a interceptação
  bloqueava qualquer destino não loopback e cada `Enter` foi precedido pela
  guarda do elemento ativo. Houve zero tentativa ou URL externa, zero exceção
  runtime e zero achado novo P0, P1, P2 ou P3. Reflow, tokens contínuos,
  escaping, foco, teclado, Light/Dark, `pt-BR`/`en-GB` e idioma original da
  citação passaram; `AQG-S05-001` a `AQG-S05-008` estão `RESOLVIDOS` e o gate
  está `APROVADO`. Os listeners da tarefa foram encerrados e as portas
  terminaram livres. Percentuais de cobertura JavaScript, reprodução no Node
  exato, engine externa de acessibilidade e browser em janela visível
  permanecem limitações. O Human Gate não foi autorizado nem executado, e
  `STATE-06` permanece sem autorização.
- Reinício integral do Automatic Quality Gate após `S05-CORR-08`: autorizado
  e concluído em 2026-08-05 sobre
  `main@b68cf2d8a9a6c735781529f1f3fb63d5cd515f95`, corpus `4.9.2` e working
  tree limpa. Autoridade, lifecycle, escopo, contratos e segurança foram
  reinspecionados; lint, typecheck, 38 testes e dois builds idênticos byte a
  byte passaram. Chrome headless `151.0.7922.75`, em perfil temporário sem
  extensões, repetiu as oito combinações em 1280 e 320 CSS px com citação
  local sem URL, interceptação não loopback e guarda antes de cada `Enter`.
  Reflow, tokens contínuos, escaping, foco, teclado, Light/Dark,
  `pt-BR`/`en-GB`, idioma original da citação e a hero simplificada passaram.
  Houve zero tentativa ou URL externa, zero exceção runtime e zero novo P0,
  P1, P2 ou P3. `AQG-S05-001` a `AQG-S05-008` estão `RESOLVIDOS` e o gate
  está `APROVADO`. Preview e Chrome foram encerrados e as portas 4173, 5173 e
  9230 ficaram livres. O Human Gate não foi executado, e `STATE-06` permanece
  sem autorização.
- Human Gate de `STATE-05`: `APROVADO` sem ressalvas em 2026-08-05 sobre
  `main@192613364429a79ce82a208f072f5005209e6f52`, corpus `4.9.2` e working
  tree limpa. O proprietário recebeu e revisou na mesma conversa o resumo
  completo da baseline, do Automatic Quality Gate aprovado, de
  `AQG-S05-001` a `AQG-S05-008`, de `S05-CORR-08`, das amostras críticas,
  verificações, limitações, riscos residuais, escopo negativo e rollback, e
  confirmou a frase canônica `Confirmo a decisão acima exclusivamente para
  STATE-05`. Nenhum novo achado ou ressalva foi registrado. O preview humano
  exclusivamente loopback foi encerrado, e as portas 4173, 5173 e 9230
  terminaram livres. `STATE-05` está encerrado; `STATE-06` permanece sem
  autorização e sem execução.
- Entrada em `STATE-06 INTEGRATION`: autorizada e registrada em 2026-08-05
  sobre `main@8fb3b93532a569af953cdf24e190b82998020464`, corpus `4.9.2` e
  working tree limpa, depois de reconfirmar localização
  `C:\Projects\RAG-Challenge`, Git top-level `C:/Projects/RAG-Challenge`,
  Git directory `.git`, branch `main`, HEAD e corpus. A autoridade permite,
  depois deste registro, executar localmente, offline e sequencialmente apenas
  `S06-A`: fluxo sintético documento → índice → pergunta → resposta entre
  backend e frontend; sincronização oficial somente por servidor HTTP falso
  e loopback; restart e persistência; configuração não secreta por ambiente;
  artefato local reproduzível; reprodução em baseline limpa; checks .NET/npm,
  integração/E2E, build, higiene, documentação e commits locais focais.
  Dependências, manifests/lockfiles, contratos, OpenAPI, ADRs, rede externa,
  providers/contas reais, secrets, corpus/fonte oficial reais, GitHub, OCI
  real, publicação, deploy, DB-Notifier, Automatic Quality Gate, Human Gate e
  `STATE-07` permanecem fora da autoridade. `STATE-06` está ativo; `S06-A`
  está autorizado e ainda não executado neste registro.
- Lote `S06-A`: concluído localmente, offline e sequencialmente em 2026-08-05.
  O registro de entrada está no commit
  `ad218b58210e41d0c3a2c76ef81b5886498fd01a`; a composição executável, os
  testes E2E/loopback e os scripts de artefato estão no commit
  `8041e25a554a7cc47ecebf4abe1fc8b94b12d12d`. O perfil explicitamente
  habilitado no ambiente `Integration` usa stores SQLite e conteúdo imutável
  existentes, fixture CSV sintética e providers determinísticos locais para
  documento → índice → pergunta → resposta. O Dashboard publicado e a API v1
  funcionam na mesma origem; respostas `pt-BR` e `en-GB`, citação, cobertura,
  restart, catálogo, ativação, índice e conteúdo bruto persistido passaram. A
  sincronização oficial foi exercitada somente por servidor HTTP falso em
  loopback, com proxy e redirects desativados. O artefato local de 58 arquivos
  foi produzido duas vezes sobre a baseline rastreada limpa
  `main@8041e25a554a7cc47ecebf4abe1fc8b94b12d12d` com SHA-256 idêntico
  `b2b6f50352c29a89f91640870564df263a2a5888f2009a94dc9a0ec1bb33b3c4`,
  e a segunda cópia foi reproduzida com a mesma geração ativa após restart.
  Format, build Release, 174 testes .NET, cobertura .NET aplicável, lint,
  typecheck, 38 testes npm, build Vite e submissão pela UI publicada em
  Chrome passaram. Ports 5086/5096 e runtimes temporários terminaram limpos.
  O relatório proprietário é
  [`STATE-06-Integration-Report.md`](../../docs/STATE-06-Integration-Report.md).
  `STATE-06` permanece ativo; Automatic Quality Gate, Human Gate, `STATE-07`
  e ações externas não foram autorizados nem executados.
- Correção focal da política de toolchain do Dashboard: autorizada e concluída
  em 2026-08-05 no commit
  `a7d50d8e72d5f5600ae41e3fdd313f4f1e502188`. `engines` e `devEngines`
  agora aceitam e impõem Node.js `>=24.18.0 <25` e npm `>=11.16.0 <12`, com
  `onFail: "error"`; o `packageManager` exato não aplicado foi removido e o
  metadata raiz do lockfile foi reconciliado. `.nvmrc` conserva `24.18.0`
  como seletor opcional do limite inferior, sem restringir o intervalo
  suportado. Na instalação já atualizada para Node.js `24.19.0` e npm
  `11.17.0`, lint, typecheck, 38 testes e build passaram offline. Duas
  construções sobre a baseline limpa do commit produziram o mesmo ZIP de 58
  arquivos e SHA-256
  `65b405c690a1c66c374296745613217717d7fd38f04cbefb15994323da1ffc98`;
  a reprodução loopback e o restart passaram. Nenhuma dependência, instalação,
  contrato, OpenAPI, ADR, lifecycle ou ação externa mudou.
- Automatic Quality Gate de `STATE-06`: autorizado e executado localmente,
  offline e de forma sequencial em 2026-08-06 sobre
  `main@a6f0480b7f229b63c5ac24d65e61f55de1c6483a`, corpus `4.9.2` e working
  tree limpa. Format, build Release, 174 testes .NET, cobertura combinada de
  92,38% de linhas e 66,59% de branches, lint, typecheck, 38 testes npm,
  build Vite, E2E/restart, sync por HTTP falso loopback, configuração sem
  secret, duas construções idênticas do ZIP e reprodução do artefato passaram.
  O gate foi `REPROVADO` por três achados P2 abertos: `AQG-S06-001`, ausência
  do plano/ensaio não produtivo de OCI pertencente ao estado;
  `AQG-S06-002`, cobertura integrada parcial de resiliência e cancelamento; e
  `AQG-S06-003`, ausência dos exemplos reais no README, seu estado factual
  obsoleto e a divergência entre Lifecycle (`STATE-06`) e roadmap
  (`S08-B`/`BL-M13`). Nenhum P0, P1 ou P3 foi identificado. A auditoria não
  corrigiu produto, testes, README, Lifecycle, roadmap, ADRs ou contratos e
  não executou Human Gate, OCI, ação externa ou `STATE-07`. `STATE-06`
  permanece ativo e o Human Gate é prematuro. Processos, listeners e stores
  temporários da tarefa terminaram ausentes; a política de execução recusou a
  remoção recursiva do diretório de cobertura ignorado sob `TestResults/`, que
  conserva somente evidência gerada.
- Correção `S06-CORR-01`: autorizada pelo proprietário em 2026-08-06 sobre
  `main@140c0516e4dbfc02808a90f0496550eb6b09da1b`, corpus `4.9.2` e working
  tree limpa. A decisão `NORM-S06-001` mantém em `STATE-06` um README
  factualmente atual com exemplo local/sintético realmente verificado e
  reserva para `STATE-08` sua finalização pública com evidência própria de OCI
  e execução real do produto; a reconciliação normativa está registrada como
  corpus `4.9.3`. A supply chain dos três runtime packs Linux ARM64 `10.0.10`
  aprovou identidade, versão, SHA-512 de catálogo, assinaturas
  author/repository em revogação offline, licença MIT, fechamento sem
  dependências e zero advisory aplicável. Depois das ampliações explícitas de
  autoridade, os quatro lockfiles de produção registraram somente esse RID e
  esses três packs no commit
  `4b808319b0c1abf0970f9f41c77fb1e08d295585`; o rehearsal ARM64, as provas
  compostas de cancelamento/resiliência e o README local/sintético foram
  implementados nos commits
  `405ab20d3e76a75f1a0f50fd625ec71831b9134b`,
  `801f77625e68692fe7b4691798694b4e8d92433a` e
  `9d72a1bb93325f6303516592fb4ff352a0a531ca`. `AUTH-S06-DEP-002` adicionou
  somente `linux-arm64` aos quatro projetos de produção e completou o cache
  isolado com os 13 packages de teste já locked; o commit
  `f1a02cd7c7acb50bcd3fa8b00e69e6c3f59b88c3` materializa as quatro
  declarações de projeto. O restore locked da solução aprovou somente com a
  fonte local verificada e o cache isolado, sem mudar qualquer lockfile ou
  grafo. C4 aprovou format, build Release sem warning, 179 testes .NET,
  cobertura de 92,40% de linhas e 66,60% de branches, lint, typecheck, 38
  testes npm, build Vite, auditoria de 198 arquivos, duas reproduções ARM64
  idênticas, verificação estática e os comandos do README. O fluxo local
  preservou a mesma geração após restart; os processos e listeners terminaram
  ausentes. `AQG-S06-001` a `AQG-S06-003` estão
  `CORRECTED_PENDING_GATE_RETEST`; o Automatic Quality Gate histórico permanece
  `REPROVADO` e não foi repetido. Human Gate, `STATE-07`, execução Linux, OCI
  real e demais ações externas permanecem não autorizados.
- Reinício integral do Automatic Quality Gate de `STATE-06`: autorizado por
  `AUTH-S06-AQG-RETEST-001` e executado localmente, offline e de forma
  sequencial em 2026-08-06 sobre
  `main@9d7c4ce816eca049ba09942ab7fe8b1148aa73c9`, corpus `4.9.3` e working
  tree limpa. O preflight encontrou zero processo pertencente ao produto e
  zero listener nas portas da tarefa. A auditoria repetiu desde o início a
  supply chain dos três runtime packs ARM64, restore locked com fonte local
  verificada e cache isolado, inspeções estáticas, o gate técnico completo,
  cobertura, testes focais de integração/cancelamento/resiliência, duas
  reproduções ARM64, o verificador estático, os comandos publicados no README
  e a higiene de segurança. Format e build Release sem warning, 179 testes
  .NET, cobertura de 92,40% de linhas e 66,60% de branches, lint, typecheck,
  38 testes npm e build Vite passaram. As duas reproduções ARM64 foram
  idênticas, com SHA-256
  `d539f0dd27553859966fe45f373363d32ffd34c61cd59618fe7cf61dcd9b2369`, e o
  verificador aprovou 17 payloads ELF64 AArch64 sem executar Linux nem contatar
  OCI. Os comandos do README produziram e reproduziram localmente o artefato
  sintético, inclusive a mesma geração ativa após restart. O gate foi
  `APROVADO`, sem novo P0, P1, P2 ou P3; `AQG-S06-001` a `AQG-S06-003` estão
  `RESOLVIDOS`. `STATE-06` permanece ativo. Human Gate, `STATE-07`, execução
  Linux, OCI real e demais ações externas não foram autorizados nem executados.
- Auditoria técnica estática posterior ao gate: os achados `AST-001`,
  `AST-002` e `AST-003` foram confirmados e corrigidos sequencialmente sobre o
  anchor pré-correção
  `main@bfc3aefc3a731b1b49b47458374cb903860faf6f`. O commit
  `0b3c5be2c80f0f1ee83af82d2158e87360c33ea7` vincula a resolução de registro
  oficial à revisão imutável do snapshot; o commit
  `d3fa9d77863092918dbef6fa7afee12992c2053f` exige e valida a autoridade
  generation-bound completa na busca vetorial; o commit
  `cfb93892571bec1beae3087b1f5ff44932d24693` valida transacionalmente o
  conjunto completo de bindings ativos; e o commit
  `dc3dde2437ad3cbb50b397358fcda043c9d6f4b3` adiciona a migration local de
  integridade referencial pós-snapshot. A verificação consolidada na última
  baseline aprovou build Release sem warning/erro, 87 testes unitários, 10 de
  arquitetura e 109 de integração, total 206 sem falha ou skip, format,
  ausência de mudança pendente no modelo EF e higiene Git. A revisão
  pós-correção sobre `main@dc3dde2437ad3cbb50b397358fcda043c9d6f4b3`,
  corpus `4.9.3` e working tree limpa dispôs `AST-001` a `AST-003` como
  `RESOLVIDOS`. Contrato canônico v1, API pública e OpenAPI permaneceram
  inalterados. Nenhum banco real foi migrado ou reparado. Essa disposição
  pertence à auditoria AST e não repetiu nem substituiu o Automatic Quality
  Gate aprovado na baseline anterior; `STATE-06` permanece ativo, sem Human
  Gate, `STATE-07` ou ação externa.
- Fechamento de `S04-A0`: `PdfPig` `0.1.15` e `CsvHelper` `33.1.0` foram
  selecionados condicionalmente para desenvolvimento local;
  `Sylvan.Data.Csv` `1.4.4` permanece fallback não selecionado e não
  autorizável por substituição automática. O adapter OpenAI é HTTP direto,
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
- Automatic Quality Gate de `STATE-04`: `APROVADO` sobre
  `main@7f236542133719481a02f507cf802a1dd385f328`, corpus `4.9.2`, com zero
  achados abertos. `AQG-S04-001` (P2), ausência inicial de uma prova única do
  fluxo sintético ingestão→ativação→consulta, foi `RESOLVIDO`. O gate offline
  aprovou format, build Release sem warnings, 119 testes, 92,37% de linhas,
  65,73% de branches, arquitetura,
  contratos, integração, parsers, hashes, lockfiles, OpenAPI, falhas, health,
  segurança e higiene. Dashboard e validações externas foram `NÃO
  APLICÁVEIS` pelo escopo negativo explícito. O resultado automático não
  executou o Human Gate.
- Human Gate de `STATE-04`: `APROVADO COM RESSALVAS` em 2026-08-04 sobre
  `main@6d141decdf5f40661bb9f408d6aa97f9f322cfcf`, corpus `4.9.2` e working
  tree limpa, após a apresentação do resumo completo do gate, entregáveis,
  amostras críticas, limitações, riscos, rollback e escopo negativo. A frase
  canônica foi `Confirmo a decisão acima exclusivamente para STATE-04`. A
  decisão encerra somente `STATE-04`; não autoriza entrada ou execução de
  `STATE-05`, produção, ação externa ou limpeza das evidências temporárias.
- Auditoria corretiva posterior de `STATE-04`: a primeira passagem, iniciada
  sobre `main@f71343291b942c66d0ff417a8764b032bbd63bff`, identificou
  `AUD-S04-001` a `AUD-S04-004` e foi interrompida conforme sua condição de
  parada. `S04-CORR-01` implementou C1, C2 e C3 nos commits focais
  `a674560ed1093e96d533012f1b11a292c3f641b5`,
  `b875eac6e9ce4c72783d4e4bb72a59686ca58248` e
  `ac34c085a499a34ea8ee1c9106675482e38790c3`; C4 reconcilia os registros
  factuais. O Automatic Quality Gate corretivo foi `APROVADO` sobre
  `main@114ea6f7f76936dac991553588660fc986bd0f10`, com 150 testes aplicáveis,
  92,26% de linhas e 65,07% de branches; a auditoria completa permanece
  pendente e obrigatória antes da disposição dos achados. Nenhum novo Human
  Gate foi executado.
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
- O corpus de instruções vigente possui versão `4.9.3` e 13 arquivos em
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
- O corpus `4.9.3` registra `NORM-S06-001`: `STATE-06` conserva o README
  factualmente atual e ao menos um exemplo realmente verificado no artefato
  integrado local/sintético, enquanto `STATE-08` conserva sua finalização
  pública com evidência separadamente verificada de OCI e execução real do
  produto. A mudança elimina a divergência de ownership sem alterar a ordem do
  lifecycle, dispor os achados ou repetir o gate.
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
  contém a persistência SQLite local, conteúdo imutável, catálogo, snapshots,
  observações, gerações, ativações, leases e journal administrativo. O backend
  local de `STATE-04` implementa administração one-shot, ingestão PDF/CSV,
  sincronização por transporte controlado, chunking, indexação, recuperação,
  geração grounded e API pública v1; os caminhos externos permanecem
  fail-closed e foram exercitados somente com fakes e fixtures sintéticas.
- SDK .NET `10.0.302` e C# `14.0` estão fixados. O Dashboard suporta Node.js
  `>=24.18.0 <25` e npm `>=11.16.0 <12`, com enforcement por `devEngines`;
  `.nvmrc` seleciona o limite inferior `24.18.0`. NuGet usa gestão central e
  sete lockfiles reproduzidos offline.
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
- Existem API, ingestão, recuperação, persistência SQLite e vector store
  exato funcionais localmente dentro do escopo sintético de `STATE-04`. Não
  existem corpus real autorizado, container, infraestrutura operacional ou
  deploy homologados.
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
3. Preparar qualquer eventual resumo do Human Gate de `STATE-06` somente sob
   autoridade humana separada, sobre o relatório do Automatic Quality Gate
   aprovado e uma baseline limpa; o reteste não executou nem autorizou o Human
   Gate.
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

## Próxima autoridade

`STATE-04 BACKEND_IMPLEMENTATION` está encerrado após Automatic Quality Gate
aprovado e Human Gate aprovado com as ressalvas documentadas em 2026-08-04. O
fechamento documental de
`S04-A0`, o pin offline de `PdfPig` `0.1.15` e `CsvHelper` `33.1.0`, a
execução sequencial de `S04-A` a `S04-D` e o Automatic Quality Gate posterior
foram autorizados pelo proprietário em 2026-08-04. A fonte offline isolada foi
completada por cópia somente leitura e allowlisted das identidades e versões
já fixadas, sem alterar o cache global. `PdfPig` `0.1.15` e `CsvHelper`
`33.1.0` foram fixados com grafo aplicável vazio, restore locked e hashes
aprovados; o primeiro gate runtime sintético passou. `S04-A`, `S04-B`, `S04-C`
e `S04-D` foram concluídos sequencialmente. O Automatic Quality Gate de
`STATE-04` foi aprovado sem achados abertos; o Human Gate subsequente aceitou
o estado com as limitações e os riscos residuais já registrados.

A auditoria local posterior identificou `AUD-S04-001` a `AUD-S04-009` e o
residual `AUD-S04-005-R1`. `S04-CORR-01`, `S04-CORR-02` e `S04-CORR-03`
implementaram as correções autorizadas sem ampliar o lifecycle. O último
Automatic Quality Gate corretivo foi aprovado e a auditoria completa
reiniciada resolveu todos os achados, sem identificar novo P0, P1, P2 ou P3.
Depois da auditoria e da entrada documental, o proprietário autorizou a
execução local, sequencial e limitada de `S05-A0` a `S05-A4` sobre
`main@cab336ada60866083f3e688fe1a13cff348a3335`, corpus `4.9.2` e working
tree limpa. Os lotes foram concluídos com fixtures sintéticas, fetch falso e
verificações offline na instalação existente. A matriz das oito combinações,
lint, typecheck, 28 testes e build foram aprovados. A validação do build em
loopback confirmou preferências, validação, fluxo fail-closed, foco, landmarks
e controles rotulados; o listener foi encerrado. Cobertura percentual
JavaScript, screenshot do build estilizado e observação direta de viewport
estreito permanecem limitações, conforme o
[relatório de STATE-05](../../docs/STATE-05-Frontend-Implementation-Report.md).

A Automatic Quality Gate de `STATE-05` foi autorizada sobre
`main@f6df67a67657af891e4831a616b142d8da9fb584`, iniciou pela inspeção
estática e foi `REPROVADA` com `AQG-S05-001` (P1). A reprodução mostrou que
uma citação local malformada com `canonicalUrl` `javascript:` atravessa o
decoder e se torna link interativo. A condição de parada cancelou as demais
verificações antes de lint, typecheck, testes, build ou listener; nenhuma
correção foi executada.

`S05-CORR-01` foi autorizado e concluído no commit
`654fce6e0a09d6e7196e434de0ff6f5d6ccd5b04`. O decoder agora rejeita scheme
não HTTPS e qualquer URL em citação local; a apresentação mantém somente link
oficial HTTPS validado. Lint, typecheck, 29 testes e build passaram.
`AQG-S05-001` está `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`, sem disposição
automática.

O reinício integral posterior do gate, autorizado sobre
`main@f7e7f4a9d4afd234c9f3fcc725e7093653bc3363`, foi `REPROVADO` durante a
inspeção estática. `AQG-S05-002` (P2) registra que o limite da resposta é
verificado somente depois da materialização integral por `response.text()`;
`AQG-S05-003` (P2) registra que o título do documento permanece em inglês na
interface `pt-BR`. A parada obrigatória ocorreu antes do preflight executável,
dos checks npm e da validação em browser; nenhum produto ou teste foi alterado.

`S05-CORR-02` foi concluído nos commits
`ec5ecf41b113853fc2863a94cbfe77dbe4741828` e
`20458c8189b132b775786b2fc8f9b44ee5c2f7b8`. A leitura HTTP agora aplica o
teto de 262.144 bytes antes da materialização integral e o título visual segue
somente `interfaceLanguage`. Lint, typecheck, 34 testes e build passaram; a
validação loopback confirmou os dois títulos e terminou sem listener. Os três
achados estão corrigidos, mas pendem de reteste e disposição pelo gate.

O reinício integral autorizado sobre
`main@3f120aaf3cbc199c821685b161ece95a1988a659` foi `REPROVADO` durante a
inspeção estática. `AQG-S05-004` (P2) registra que a citação local válida usa
o valor canônico de freshness `Local`, mas o Dashboard não o localiza e a
fixture sintética o substitui por `Current`, mascarando o estado desconhecido
na apresentação. A parada obrigatória ocorreu antes do preflight executável,
dos checks npm e da validação em browser; nenhuma correção foi executada.

`S05-CORR-03` foi concluído no commit
`9ef937744302044ee3cd9105c9a23ddd3557a861`. O decoder agora restringe
freshness ao conjunto canônico, exige `Local` e URL nula para
`LocalAuthorised`, rejeita relações cross-class incompatíveis e apresenta
`Local` nas interfaces `pt-BR` e `en-GB`. A fixture local foi corrigida. Lint,
typecheck, 35 testes e build passaram; a validação loopback confirmou a
alternância localizada e terminou sem listener. As regressões de
`AQG-S05-001` a `AQG-S05-003` permaneceram verdes.

O reinício integral autorizado sobre
`main@b457970aed4564d5a654bb4e8d38439c98f29522` foi `REPROVADO` durante a
inspeção estática. `AQG-S05-005` (P2) registra que o decoder aceita
`answerLanguage` apenas por pertencer ao conjunto suportado, sem compará-lo ao
`questionLanguage` enviado. O teste de limite exato demonstra que uma
pergunta `en-GB` aceita a fixture concluída `pt-BR`. A parada obrigatória
ocorreu antes do preflight executável, dos checks npm e da validação em
browser; nenhuma correção foi executada.

`S05-CORR-04` foi concluído no commit
`bed8ec03d670ed4e76a556f7df723c30db320a24`. O decoder exige que o idioma da
resposta concluída corresponda ao idioma enviado, o cliente preserva esse
binding e as quatro combinações válidas/incompatíveis foram exercitadas.
Lint, typecheck, 37 testes e build passaram.

O reinício integral posterior sobre
`main@a58c4038fb14e656c95303d914e02c7f8ad75c17` dispôs
`AQG-S05-001` a `AQG-S05-005` como `RESOLVIDOS`, mas foi `REPROVADO` por
`AQG-S05-006` (P2). A reprodução em browser mostrou que o skip link visível
não transfere o foco ao conteúdo principal. A parada obrigatória impediu a
conclusão das verificações browser de viewport estreito/reflow, temas e matriz
das oito combinações. O Human Gate continua prematuro e `STATE-06` não está
autorizado. `S05-CORR-05` corrigiu o foco do skip link e está
`RESOLVIDO` pelo reinício integral posterior sobre
`main@8ee1213eed3522493204c68b4f843e9c438e0f69`. Esse gate foi
`REPROVADO` por `AQG-S05-007` (P2): em viewport de 320 CSS px, todas as quatro
combinações `pt-BR` geravam overflow horizontal, enquanto as quatro
combinações `en-GB` refluíam sem overflow. `S05-CORR-06`, no commit
`e34e73c7bbe8fabf96d5a5683df35935a3266e37`, tornou reduzíveis a coluna da
hero e sua tipografia compacta; os quatro checks npm e a matriz browser
isolada das oito combinações passaram a 320 CSS px, com foco, teclado e temas
preservados. `AQG-S05-007` está
`CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE`. O reinício integral posterior sobre
`main@bc2ddd6bf64fc82f7d68eb518c3013d85655c16a` foi `REPROVADO` durante a
inspeção estática por `AQG-S05-008` (P2): tokens contínuos válidos pelo
contrato em resposta, título ou trecho de citação podem forçar overflow no
viewport estreito porque as superfícies correspondentes não permitem sua
quebra. A parada ocorreu antes de preflight executável, checks npm, build ou
browser, sem processo ou listener iniciado e sem correção. `AQG-S05-001` a
`AQG-S05-006` conservam `RESOLVIDOS`; `AQG-S05-007` permanece corrigido
pendente de reteste. `S05-CORR-07`, no commit
`3f003b9db67eefeccc7e677c319ca37a26d49fa7`, tornou quebráveis sem truncamento
os três textos não confiáveis e adicionou tokens contínuos válidos à matriz
das oito combinações. Os quatro checks npm e a repetição headless controlada
passaram a 320 CSS px com reflow, idiomas, temas, foco e teclado preservados.
A primeira tentativa browser gerou acesso externo não autorizado ao ativar a
URL oficial sintética antes da guarda de foco; o incidente, a parada e a
retomada autorizada estão registrados no relatório. A repetição final usou
somente citação local sem URL e bloqueio não loopback, sem nova tentativa
externa. `AQG-S05-008` ficou `CORRIGIDO_PENDENTE_DE_RETESTE_DO_GATE` naquele
lote. O reinício integral posterior, sobre
`main@97ea076da84d7afdb3330aa05dcb39fc7b44ce0f`, repetiu desde o início as
inspeções estáticas, os quatro checks npm, o build reprodutível e a validação
browser completa. As oito combinações passaram em largura padrão e a 320 CSS
px, com guarda antes de cada `Enter`, citação local sem URL, bloqueio de
destinos não loopback e zero tentativa ou URL externa. O gate foi `APROVADO`,
sem novo P0, P1, P2 ou P3, e `AQG-S05-001` a `AQG-S05-008` estão
`RESOLVIDOS` naquela baseline. Durante a revisão humana posterior, o
proprietário solicitou simplificar a hero. `S05-CORR-08`, no commit
`b65d3b45a0ad32f0f7db1e97ccf415bdef5bb113`, removeu o rótulo promocional,
o título anterior e a ilustração decorativa, mantendo como único conteúdo
visível da área a introdução localizada promovida a H1 proporcional. Os
quatro checks npm passaram após a restauração do rótulo de workspace usado
somente como nome acessível; a matriz browser isolada passou nas oito
combinações em 1280 e 320 CSS px, com reflow, temas, idiomas, foco e teclado
preservados e sem URL ou tentativa externa. O listener e o Chrome temporário
foram encerrados, e as portas 4173, 5173 e 9230 ficaram livres.

O reinício integral posterior a `S05-CORR-08`, sobre
`main@b68cf2d8a9a6c735781529f1f3fb63d5cd515f95`, repetiu as inspeções
estáticas, os quatro checks npm, o build reprodutível e a matriz browser em
largura padrão e estreita. A hero simplificada, as oito combinações, foco,
teclado, temas, idiomas, reflow e tokens contínuos passaram sem tentativa ou
URL externa. O gate foi `APROVADO`, sem novo P0, P1, P2 ou P3, e
`AQG-S05-001` a `AQG-S05-008` estão `RESOLVIDOS` na baseline. O Human Gate
posterior foi `APROVADO` sem ressalvas sobre
`main@192613364429a79ce82a208f072f5005209e6f52`, corpus `4.9.2` e working
tree limpa, depois do resumo completo e da confirmação canônica do
proprietário. O preview humano exclusivamente loopback foi encerrado e as
portas 4173, 5173 e 9230 terminaram livres. `STATE-05` está encerrado.

`STATE-06 INTEGRATION` está ativo por autorização explícita do proprietário
sobre `main@8fb3b93532a569af953cdf24e190b82998020464`, corpus `4.9.2` e
working tree limpa. O único lote autorizado, `S06-A`, foi concluído no limite
local, offline, sintético e sequencial, com implementação focal em
`8041e25a554a7cc47ecebf4abe1fc8b94b12d12d` e relatório próprio. O Automatic
Quality Gate posterior foi executado sobre
`main@a6f0480b7f229b63c5ac24d65e61f55de1c6483a` e ficou `REPROVADO` por
`AQG-S06-001` a `AQG-S06-003`, todos P2. O proprietário autorizou
`S06-CORR-01` sobre `main@140c0516e4dbfc02808a90f0496550eb6b09da1b` e
aceitou `NORM-S06-001`; o corpus `4.9.3` reconcilia o ownership do README. As
ampliações posteriores de `AUTH-S06-DEP-001` e `AUTH-S06-DEP-002` permitiram
somente os quatro lockfiles/projetos de produção, o RID `linux-arm64`, os três
runtime packs verificados `10.0.10` e os 13 packages de teste já locked no
cache isolado. Restore locked, implementação e C4 foram aprovados sem mudança
de lockfile ou grafo além do fechamento previamente autorizado. Os commits
`4b808319b0c1abf0970f9f41c77fb1e08d295585`,
`405ab20d3e76a75f1a0f50fd625ec71831b9134b`,
`801f77625e68692fe7b4691798694b4e8d92433a`,
`9d72a1bb93325f6303516592fb4ff352a0a531ca` e
`f1a02cd7c7acb50bcd3fa8b00e69e6c3f59b88c3` materializam a correção e a
compatibilização final. O reinício integral autorizado por
`AUTH-S06-AQG-RETEST-001` foi executado sobre
`main@9d7c4ce816eca049ba09942ab7fe8b1148aa73c9`, corpus `4.9.3` e working tree
limpa. O gate foi `APROVADO`, sem novo P0, P1, P2 ou P3, e `AQG-S06-001` a
`AQG-S06-003` estão `RESOLVIDOS`. `STATE-06` continua ativo. Human Gate,
`STATE-07`, execução Linux, OCI real, providers, contas, secrets, corpus ou
fontes reais, armazenamento operacional, GitHub, publicação, deploy,
DB-Notifier e estados posteriores continuam sem autorização.

Depois desse gate, a auditoria técnica AST confirmou três problemas na
persistência e na autoridade de recuperação. Os commits
`0b3c5be2c80f0f1ee83af82d2158e87360c33ea7`,
`d3fa9d77863092918dbef6fa7afee12992c2053f`,
`cfb93892571bec1beae3087b1f5ff44932d24693` e
`dc3dde2437ad3cbb50b397358fcda043c9d6f4b3` corrigiram `AST-002`, `AST-003`,
`AST-001` e o reforço referencial complementar, respectivamente. A revisão
pós-correção dispôs os três achados como `RESOLVIDOS` sobre
`main@dc3dde2437ad3cbb50b397358fcda043c9d6f4b3`, corpus `4.9.3` e working
tree limpa.

O reinício integral posterior do Automatic Quality Gate sobre
`main@726546dbe0302b9664a62e890b6a27f19bf0c6e4`, corpus `4.9.3` e working
tree inicialmente limpa, revisou todo o diff de 20 arquivos após
`9d7c4ce816eca049ba09942ab7fe8b1148aa73c9`. A primeira tentativa parou com
`AQG-S06-004`: duas working copies geradas do Entity Framework estavam com
EOL misto. A remediação autorizada normalizou somente seus bytes para LF, sem
diff semântico, e o reinício integral subsequente foi `APROVADO`, sem novo P0,
P1, P2 ou P3; `AQG-S06-004` está `RESOLVIDO`.

O gate consolidado aprovou 206 testes .NET e 38 testes npm, cobertura .NET de
93,11% de linhas e 66,89% de branches, cinco testes focais de migration apenas
em SQLite descartável, quatro testes focais do host composto, verificação EF
sem mudança pendente, duas reproduções ARM64 idênticas e os comandos literais
de integração do README. Invocações preliminares de EF com tool home isolado,
startup project incorreto ou sem store root explícito não foram aceitas como
evidência. A limpeza removeu os diretórios exclusivos do gate e da cobertura,
encerrou sem processo/listener da tarefa e preservou limpa a baseline Git; o
artefato ignorado do comando literal do README permanece não autoritativo.

Persistem como limitações e riscos residuais: ausência de execução Linux
ARM64, OCI real, providers, contas, secrets, corpus ou fonte oficial real,
armazenamento operacional, cobertura percentual JavaScript, observação de
rede em nível de pacotes, migration em banco real e reparo de dados. O gate
não executou Human Gate, não alterou o lifecycle e não autorizou `STATE-07`,
ação externa, publicação, push ou deploy. `STATE-06` continua ativo.
