# Estado Atual

Este documento é o snapshot factual vigente do workspace em 2026-08-12. Ele
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
- Novo reinício integral do Automatic Quality Gate de `STATE-06`: autorizado
  pelo proprietário e iniciado localmente, offline e de forma sequencial em
  2026-08-06 sobre
  `main@f92e26c7008a2d124bd10edb2e3f03c0c9ad2bf6`, corpus `4.9.3` e working
  tree limpa. A reconciliação inventariou os oito commits e 25 arquivos após
  `bfc3aefc3a731b1b49b47458374cb903860faf6f`; o preflight encontrou zero
  processo do produto e zero listener nas portas da tarefa. A inspeção
  estática identificou `AQG-S06-005` (P2): os dois únicos testes PowerShell
  dos controles fail-closed, `eng/test-assert-coverage.ps1` e
  `eng/test-ci-policy.ps1`, não são invocados por nenhum entry point; o
  workflow chama somente `eng/ci.ps1`, que também não os executa. Portanto, a
  CI canônica pode aprovar sem testar o agregador de cobertura ou a política
  que afirma impor. O gate parou sem correção silenciosa antes de restore,
  build, suítes, coverage, migration, ARM64 ou comandos do README e está
  `REPROVADO`. `AQG-S06-005` permanece `ABERTO`; Human Gate continua
  prematuro, e `STATE-07` e ações externas permanecem não autorizados.
- Correção focal de `AQG-S06-005`: autorizada pelo proprietário em 2026-08-06
  sobre `main@000dca0210e220a9f247159178c6d97d9fc4fd55`, corpus `4.9.3` e
  working tree limpa. `eng/ci-policy.ps1` agora fornece uma invocação
  obrigatória que falha quando o script não existe e propaga qualquer exceção
  com contexto; `eng/ci.ps1` executa os testes de cobertura e política uma vez
  cada, antes de restore; e `eng/test-ci-policy.ps1` prova sucesso, propagação
  de falha, script ausente, invocação única dos dois testes e consumo único do
  entry point canônico pelo workflow. O workflow permaneceu inalterado. A
  verificação focal aprovou parsing dos três scripts, 11 casos de coverage, 14
  controles de política/integração, `git diff --check` e auditoria de 203
  arquivos. O Automatic Quality Gate completo não foi reiniciado e nenhuma
  outra suíte foi executada. `AQG-S06-005` está
  `CORRECTED_PENDING_GATE_RETEST`; Human Gate permanece prematuro.
- Reinício integral posterior à correção de `AQG-S06-005`: autorizado pelo
  proprietário e executado localmente, offline e de forma sequencial em
  2026-08-06 sobre
  `main@616bef4e2ae8c0b26c10781cd728dc6089136a60`, corpus `4.9.3` e working
  tree limpa. A fonte local dos três runtime packs ARM64 `10.0.10`, suas
  assinaturas e o restore locked isolado foram revalidados sem mudança de
  lockfile. O entry point automático executou e aprovou os 11 casos de
  coverage e os 14 controles de política antes do gate técnico. Build Release,
  206 testes .NET, cobertura de 93,11% de linhas e 66,89% de branches, 38
  testes npm, persistência/migration, EF sem mudança pendente,
  cancelamento/resiliência, duas reproduções ARM64 idênticas, verificador
  estático, comandos do README, segurança e higiene passaram. O gate está
  `APROVADO`, sem novo P0, P1, P2 ou P3; `AQG-S06-005` está `RESOLVIDO` e
  `AQG-S06-001` a `AQG-S06-005` permanecem `RESOLVIDOS`. `STATE-06` continua
  ativo; Human Gate, `STATE-07`, rede externa e OCI não foram executados.
- Human Gate de `STATE-06`: o proprietário recebeu e revisou na mesma
  conversa o resumo completo sobre
  `main@2f70705dcbe293b22ccd039d0764b2b9ca4b2e8a`, corpus `4.9.3` e working
  tree limpa, incluindo entregáveis, Automatic Quality Gate, achados
  históricos, amostras técnicas, limitações, rollback e escopo negativo. A
  decisão foi `APROVADO COM RESSALVAS` após a confirmação canônica
  `Confirmo a decisão acima exclusivamente para STATE-06`. As ressalvas
  preservam a ausência de execução Linux ARM64, OCI real, providers, corpus e
  fontes reais, armazenamento operacional, cobertura percentual JavaScript,
  observação de rede em nível de pacotes e migration em banco real.
  `STATE-06 INTEGRATION` está encerrado. A decisão não autorizou nem iniciou
  `STATE-07`, rede, OCI, publicação ou deploy.
- Entrada documental em `STATE-07 TESTING_HOMOLOGATION`: autorizada e
  registrada em 2026-08-06 sobre
  `main@3240a4b13acd82a1cf5815ac64f6997b2a7f89bf`, corpus `4.9.3` e working
  tree limpa. A autoridade abrange somente o snapshot factual, o histórico
  append-only, os blocos de status público estritamente necessários e um
  commit local focal. `STATE-07` está ativo sem lote autorizado ou executado.
  Dataset, avaliação RAG, testes, carga, segurança dinâmica, browser,
  providers, fontes reais, rede, OCI, GitHub, publicação, deploy, `STATE-08`
  e qualquer ação externa permanecem não autorizados e não executados.
- Baseline de planejamento de `S07-A`: a proposta documental
  [`STATE-07-S07-A-Evaluation-And-Security-Proposal.md`](../../docs/STATE-07-S07-A-Evaluation-And-Security-Proposal.md)
  foi criada no commit `183c8cd9fe303096a355ab731e72dc81748eb626` e
  confirmada pelo proprietário em 2026-08-07 exclusivamente como baseline de
  planejamento. A confirmação não concedeu `AUTH-S07-A-DATASET-001`,
  `AUTH-S07-A-RUN-001`, materialização de dataset, avaliação, testes, carga,
  segurança dinâmica, browser, providers, fontes reais, rede ou ação externa.
- Execução factual de `S07-A` A1-A5: A1 foi materializado no commit
  `968f69c2d9c37959d617742af5ac48aee5ca09d5`; a preparação do harness e sua
  correção freeze-safe estão em `ae8d96487fe719d89741aa33e5607e532301d60e` e
  `18994db15d963b321ace93b0069436ffc4813b53`; A2 foi congelado em
  `43ddc0de4a6c10b32a657f3c1e471a743cb42b5f`; A3 executou 11 casos sintéticos
  com sucesso sob `AUTH-S07-A-RUN-001`, preservando oito arquivos ignorados e o
  resultado no SHA-256
  `9efc2eef05388433af58e01242a1b1589556c43620eeec509f583fba0c2073bc`; e A4
  foi reconciliado em `760bbcf4626b7890ffdfb0eeb0a8c5419b5feec7`. As correções
  focais da validação do workspace retido e das terminações de linha de futuras
  evidências estão em `275becfb04a4d0f7a1703c3be3f4c59d87550cc2` e
  `6cd939849909a8abf2c5dd0534244da5f19be833`. A5 foi `APROVADO` sob
  `AUTH-S07-A-A5-RETEST-002`: os três comandos autorizados terminaram com exit
  code `0`; passaram 146 testes unitários, 164 de integração, 10 de arquitetura
  e 38 do Dashboard; a cobertura foi 94,91% de linhas e 67,42% de branches.
  Todos os digests e agregados congelados foram recalculados e conferiram.
  `S07-A-FIND-001` permanece `OPEN`; `S07-A-FIND-004` permanece `OPEN`
  histórico, com causa corrigida somente para futuras evidências; e
  `S07-A-FIND-002`, `S07-A-FIND-003` e `S07-A-FIND-005` estão `RESOLVIDOS`.
  A evidência A3 histórica permanece imutável. A5 não foi Automatic Quality
  Gate, Human Gate ou mudança de lifecycle e não amplia a homologação além da
  fronteira sintética local, offline, determinística e sequencial.
- Automatic Quality Gate de `S07-A`: reiniciado integralmente e `APROVADO` em
  2026-08-09 sob `AUTH-S07-A-AQG-RETEST-003`, sobre
  `main@a6626a363713b4fbcf83387b7b2104eae1f3e918`, corpus `4.10.1`, working
  tree inicialmente limpa e OpenAPI v1 no SHA-256 protegido. A auditoria
  estática confirmou A1-A5, commits, estado factual, histórico append-only,
  manifests congelados, oito arquivos ignorados sem reparse point, todos os
  digests e os sete agregados sintéticos em `1.000000`. O preflight encontrou
  zero processo e zero listener pertencente ao RAG-Challenge. Os três comandos
  autorizados terminaram com exit code `0`: auditoria de 244 arquivos,
  `Validate` com 6 de 6 testes e CI offline com 146 testes unitários, 164 de
  integração, 10 de arquitetura e 38 do Dashboard; cobertura de 94,91% de
  linhas e 67,42% de branches, build sem avisos ou erros. `AQG-S07-001` a
  `AQG-S07-004` estão `RESOLVIDOS`; nenhum novo achado surgiu.
  `S07-A-FIND-001` e `S07-A-FIND-004` permanecem `OPEN`, enquanto
  `S07-A-FIND-002`, `S07-A-FIND-003` e `S07-A-FIND-005` permanecem
  `RESOLVIDOS`. A aprovação vale somente para a fronteira sintética local;
  thresholds de produto, provider, fonte, browser, segurança dinâmica, carga,
  recuperação, acessibilidade, Linux, OCI e produção permanecem `NOT_RUN`.
  Nenhum Human Gate ou avanço de lifecycle é inferido.
- Contrato HTTP/OpenAPI v2 e serving visual same-origin: o contrato foi
  congelado no commit `54bab1aa5f25b778093bea62ffecf7c479557f9a`, implementado
  localmente no commit `c01abf525f4cc113baa389982da3b419d07556b6` e corrigido
  focalmente no commit `5505a85253aa4a8a7a3690caf3dd7a762175cab9`. O Automatic
  Quality Gate reiniciado sob `AUTH-STATE07-V2-SERVING-AQG-RETEST-001` foi
  `APROVADO` sobre essa última baseline limpa, corpus `4.10.1`. A auditoria
  estática confirmou os 33 paths da implementação, a correção do roteamento de
  `pageNumber` malformado, as fronteiras públicas e a preservação byte a byte
  de OpenAPI v1 e v2. O preflight encontrou zero processo ou listener
  pertencente ao produto. Todos os comandos focais e a CI offline terminaram
  com exit code `0`; passaram 147 testes unitários, 171 de integração, 11 de
  arquitetura e 42 do Dashboard, com cobertura de 94,80% de linhas e 67,14% de
  branches. `AQG-S07-V2-001` e `AQG-S07-V2-002` estão `RESOLVIDOS`, sem novo
  achado. Browser/tecnologia assistiva, dado, renderer, provider, fonte e rede
  reais, carga, crash/recovery, Linux, OCI e produção permanecem `NOT_RUN`.
  Nenhum Human Gate ou avanço de lifecycle é inferido.
- Integração, restart e recuperação fria do runtime composto v2: autorizados
  sob `AUTH-STATE07-V2-INTEGRATION-RECOVERY-IMPL-001` sobre a baseline limpa
  `main@a47bd40b1873920c7660abb14acd68de45a7dde4`, corpus `4.10.1`, e
  concluídos no commit `e5dae7ee5a786417fba2c6ef0555686816b0b330`. A
  composição permanece fail-closed fora do perfil `Integration`; dentro dele,
  a mesma instância sintética atende query, readiness e o reader visual
  verificado sobre PDF/PNG project-owned em memória. Passaram 52 de 52 testes
  focais. O harness publicado em `127.0.0.1:5086` resultou `Passed`: serving
  PNG `200`, revalidação `304`, mesma geração após restart e cold restore,
  fingerprints idênticos das cópias confinadas, teto visual de 64 MiB e token
  bucket com dez acessos aceitos e o décimo primeiro rejeitado por `429`. Dois
  builds offline produziram o mesmo ZIP SHA-256
  `e27c64571b63538e4cba21f552df500c24a4bab3a6365e6229e2d9dd033f2f7d`.
  O cleanup removeu runtime, stores, backup, restore e temporários task-owned;
  nenhum host ou listener permaneceu. OpenAPI v1/v2, contrato, schema,
  migration, ADR, dependência e lockfile não mudaram. Essa verificação focal
  não foi Automatic Quality Gate, Human Gate ou lifecycle; a evidência não
  homologa produto, dado/renderer/provider/fonte real, browser, tecnologia
  assistiva, carga, crash injection abrangente, recuperação operacional,
  Linux, OCI ou produção.
- Correção factual da ordem de dependência de `STATE-07` em Lifecycle:
  autorizada sob `AUTH-STATE07-V2-INTEGRATION-RECOVERY-LIFECYCLE-CORR-001`
  sobre `main@de40a93e0023f854fec840a93934c199c294f9c6`, corpus `4.10.2` e
  working tree limpa. Somente as anotações de estado foram reconciliadas:
  `S04-CORR-04-E` possui Automatic Quality Gate corretivo aprovado;
  contrato/serving v2 estão implementados e possuem Automatic Quality Gate
  aprovado; integração, restart, cold backup/restore confinado e limites foram
  implementados e verificados focalmente no commit
  `e5dae7ee5a786417fba2c6ef0555686816b0b330`; naquele registro, seu Automatic
  Quality Gate permanecia `NOT_RUN`. Dataset e homologação continuavam
  posteriores e não autorizados. A ordem normativa, os estados e os critérios
  não mudaram; nenhum runtime, teste, gate ou lifecycle foi executado nessa
  correção documental.
- Automatic Quality Gate da integração e recuperação v2: reiniciado
  integralmente e `APROVADO` em 2026-08-09 sob
  `AUTH-STATE07-V2-INTEGRATION-RECOVERY-AQG-RETEST-001`, sobre
  `main@f6c648c40cf8d0280cfceca5509a381bddb9fc8f`, corpus `4.10.3`, working
  tree inicialmente limpa e OpenAPI v1/v2 protegidas. A auditoria de 255
  arquivos passou; o preflight encontrou zero processo e zero listener
  pertencente ao RAG-Challenge; e 53 de 53 testes focais passaram. Dois builds
  determinísticos produziram o mesmo ZIP SHA-256
  `ab5e450efe1b606f2b8e50e2f5885a3c1ae19bf4ad90dd96d096e00506daec28`.
  O harness publicado resultou `Passed`, com três readiness `Ready`, geração
  preservada após restart e cold restore, serving PNG e `304`, teto de 64 MiB
  e token bucket com dez acessos aceitos e o décimo primeiro rejeitado. A CI
  offline aprovou 147 testes unitários, 174 de integração, 11 de arquitetura e
  42 do Dashboard, cobertura de 94,81% de linhas e 67,24% de branches, e build
  sem avisos ou erros. O cleanup foi concluído sem runtime ou listener
  remanescente. O gate foi aprovado sem novo achado e
  `AQG-S07-V2-IR-001` está `RESOLVIDO`. A aprovação permanece sintética e não
  constitui homologação de produto, Human Gate ou mudança de lifecycle.
- Correção factual subsequente de Lifecycle: autorizada sob
  `AUTH-STATE07-V2-INTEGRATION-RECOVERY-LIFECYCLE-CORR-002` sobre
  `main@7ad6bae369eb1efbf6429902a2fd1f4441b60a32`, corpus `4.10.4` e working
  tree limpa. As duas claims correntes desatualizadas foram reconciliadas para
  registrar somente o Automatic Quality Gate da integração e recuperação v2
  `APROVADO` sob `AUTH-STATE07-V2-INTEGRATION-RECOVERY-AQG-RETEST-001`, sem
  novo achado, e `AQG-S07-V2-IR-001` `RESOLVIDO`. A ordem normativa, os
  estados e os critérios não mudaram; dataset e homologação de produto
  permanecem posteriores, `NOT_RUN` e não autorizados. Nenhum runtime, teste,
  gate, Human Gate ou lifecycle foi executado nessa correção documental.
- A0 de prontidão do primeiro documento de produto: executado localmente,
  offline, sequencialmente e sem comportamento de produto sob
  `AUTH-S07-A-PRODUCT-A0-001`, sobre
  `main@78d49e135d7b517c7ff89a9e5edcbcc7839e4043`, corpus `4.10.5`, working
  tree inicialmente limpa e OpenAPI v1/v2 protegidas. O candidato ignorado
  `postgresql-18-reference-a4`, PostgreSQL `18.4`, permaneceu confinado ao
  intake autorizado, como arquivo regular não rastreado e sem reparse point.
  Os `15.771.040` bytes, `%PDF-1.4`, EOF e SHA-256
  `cea7b845568095eb56dee1b51bfa145c6c6637bc4377c986019971577efefae4`
  conferiram com o registro. Proveniência, `contentLanguage=en`,
  `sourceDeclaredLanguage=en`, publisher e atribuição permaneceram
  consistentes. Parsing, indexing, source-byte retention, quotation e citation
  possuem disposição explícita; page rendering, derivative-image creation,
  derivative-image retention, runtime derivative display e a intended source
  or derivative distribution boundary permanecem `UNPROVEN`. Sem inferir
  direitos a partir da permissão geral, a disposição factual foi
  `BLOCKED/EXCLUDED`, não `READY_FOR_PRODUCT_ACTIVATION`. Nenhum dataset,
  manifest, derivado, indexação, ativação, parser, renderer, runtime, teste,
  gate, Human Gate ou lifecycle foi executado ou alterado.
- Decisão arquitetural de mapeamento de direitos: preparada sob
  `AUTH-S07-A-RIGHTS-POLICY-CORR-PREP-001` sobre
  `main@17c41a78cbe853473860403d476797064b77c78a`, corpus `4.10.7` e working
  tree inicialmente limpa e aceita explicitamente pelo proprietário mediante
  `ADR-0011: ACEITAR.` sobre
  `main@09f6760cb1a41d907da42b8c01cb34a7425030b9`, corpus `4.10.8`. O
  [ADR-0011](../../docs/architecture/ADR-0011-Source-Rights-Evidence-Mapping-And-Same-Origin-Derivative-Display-Boundary.md)
  está `accepted`: preserva as dez decisões independentes e o
  fail-closed, estabelece mapeamento explícito, auditável e condicionado de
  concessões primárias amplas e separa a exibição same-origin no runtime da
  distribuição/publicação externa de bytes. A decisão registra a
  incompatibilidade estática entre o contrato v2, que exige reavaliar a
  intended distribution boundary, e
  `DocumentRightsEligibilityPolicy.PdfVisualEvidence`, que não avalia
  `SourceAndDerivativeByteDistributionOrPublication`. A aceitação estabelece
  somente autoridade arquitetural: não alterou contrato público ou
  comportamento e não reclassificou os cinco direitos `UNPROVEN` nem a
  disposição `BLOCKED/EXCLUDED` do PostgreSQL. A aceitação por si só não
  autorizou reconciliação semântica, correção interna ou novo A0; a
  reconciliação posterior está registrada abaixo, e as duas etapas executáveis
  continuam separadamente autorizadas.
- Reconciliação semântica do ADR-0011: autorizada sob
  `AUTH-S07-A-RIGHTS-POLICY-CORR-RECONCILE-001` sobre
  `main@6fc81b973ca217693a286479df3ff6db0f4577e9`, corpus `4.10.9`, working
  tree inicialmente limpa e OpenAPI v1/v2 protegidas. ADR-0004, ADR-0008, o
  registro de elegibilidade e o contrato documental v2 agora aplicam o
  mapeamento explícito, auditável e condicionado, preservam as dez decisões e
  o fail-closed, distinguem same-origin runtime display da distribuição ou
  publicação externa e vinculam attribution, notices, disclaimers, trademark
  e change marking à linhagem de cada derivado. Nenhum contrato público ou
  comportamento mudou. `postgresql-18-reference-a4` permanece
  `BLOCKED/EXCLUDED`, com os cinco direitos `UNPROVEN`; nenhum novo A0 foi
  executado. Naquela baseline, a incompatibilidade executável ainda permanecia;
  a correção interna posterior está registrada abaixo.
- Correção interna da política de serving do ADR-0011: implementada sob
  `AUTH-S07-A-RIGHTS-POLICY-CORR-IMPL-001` no commit
  `b9c3e5f3a72c2dd7762c256198452ae2c217b2d2` e reconciliada
  documentalmente sob `AUTH-S07-A-RIGHTS-POLICY-CORR-IMPL-RECONCILE-001`, a
  partir desse `main`, corpus `4.10.10`, working tree limpa e OpenAPI v1/v2
  protegidas. O gate interno `PdfVisualEvidenceServing` avalia as dez decisões:
  `RuntimeDerivativeImageDisplay` deve estar `Permitted`;
  `SourceAndDerivativeByteDistributionOrPublication` `Unproven` bloqueia; e
  `Denied` é compatível somente com `RuntimeDerivativeImageDisplay`
  `Permitted` na fronteira same-origin aceita. A verificação focal aprovou 19
  testes da política, 23 regressões dos gates existentes, três testes do leitor
  real e seis testes contratuais v1/v2. Nenhum runtime ou listener permaneceu.
  Naquela baseline, nenhum novo A0 ou gate havia sido executado e
  `postgresql-18-reference-a4` permanecia `BLOCKED/EXCLUDED` com os cinco
  direitos `UNPROVEN`; a reavaliação posterior está registrada abaixo.
- Reavaliação A0 candidato-específica sob ADR-0011: executada localmente,
  offline, sequencialmente e sem comportamento de produto sob
  `AUTH-S07-A-PRODUCT-A0-002`, sobre
  `main@f21cdea2052d28de1e2ffb86b1629c1c10bc6b6a`, corpus `4.10.11`, working
  tree inicialmente limpa e OpenAPI v1/v2 protegidas. O PDF ignorado permaneceu
  arquivo regular, sem reparse point, com `15.771.040` bytes e SHA-256
  `cea7b845568095eb56dee1b51bfa145c6c6637bc4377c986019971577efefae4`.
  A concessão oficial já registrada é relevante às cinco operações, mas exige
  copyright, permission notice e dois disclaimers em todas as cópias. O
  contrato atual não oferece mecanismo determinado para esses textos no PNG ou
  na citação pública. Page rendering, derivative-image creation,
  derivative-image retention e `RuntimeDerivativeImageDisplay` permanecem
  `UNPROVEN`; `SourceAndDerivativeByteDistributionOrPublication` está `DENIED`
  fora da fronteira de runtime-display por exclusão deliberada de download,
  hosting público, CORS permissivo, CDN, export, bundles, Git/Git LFS e
  republicação. A disposição permanece `BLOCKED/EXCLUDED`, não
  `READY_FOR_PRODUCT_ACTIVATION`. Nenhum dataset, manifest, derivado, parser,
  renderer, indexação, ativação, teste, runtime, gate, Human Gate ou lifecycle
  foi executado ou alterado.
- Decisão arquitetural de imagem derivada autocontida: preparada sob
  `AUTH-S07-A-NOTICE-BEARING-PROFILE-ADR-PREP-001` sobre
  `main@1b64ca88a0efebd7ab450f5bdc22004a72f3dc53`, corpus `4.10.12`, working
  tree inicialmente limpa e aceita explicitamente pelo proprietário mediante
  `ADR-0012: ACEITAR.` sobre
  `main@243a448823a114190f68a25f9d521e1849eddacf`, corpus `4.10.13`, working
  tree limpa e OpenAPI v1/v2 protegidas. O
  [ADR-0012](../../docs/architecture/ADR-0012-Notice-Bearing-Page-Image-Profile-And-Derivative-Obligation-Delivery.md)
  está `accepted`: define um único perfil versionado de PNG composto, no
  qual a região da página preserva cada pixel e um painel separado carrega os
  avisos completos. A decisão também define `DerivativeObligationSetV1`, seu
  vínculo imutável ao render manifest, armazenamento, backup/cold restore,
  serving same-origin, apresentação acessível e os impactos necessários de
  schema, migration e contrato v2. A aceitação concede somente autoridade
  arquitetural: não reclassifica o PostgreSQL, altera OpenAPI, código, schema,
  migration, dataset ou comportamento; nenhum renderer, runtime, teste, gate,
  Human Gate ou lifecycle foi executado.
- Reconciliação semântica do ADR-0012: autorizada sob
  `AUTH-S07-A-NOTICE-BEARING-PROFILE-RECONCILE-001` sobre
  `main@5c2cea66e45f13479486a345552e5cc3cd47fefe`, corpus `4.10.14`, working
  tree inicialmente limpa e OpenAPI v1/v2 protegidas. ADR-0008, o contrato
  documental v2, data dictionary, Security-And-Access, threat model e registro
  de elegibilidade agora aplicam `pdf-page-png-notice-v1`, o
  `DerivativeObligationSetV1`, vínculo ao manifest, storage/reachability,
  backup/cold restore, serving same-origin e apresentação acessível. A
  reconciliação identifica como futuros obrigatórios a revisão protegida do
  contrato v2, schema e migration, sem executá-los. As dez decisões e o
  fail-closed permanecem independentes; `postgresql-18-reference-a4` continua
  `BLOCKED/EXCLUDED`, com quatro operações visuais `UNPROVEN` e distribuição/
  publicação externa `DENIED`. Nenhum novo A0, código, teste, renderer,
  runtime, gate, Human Gate ou lifecycle foi executado.
- Revisão protegida do contrato v2 notice-bearing: congelada sob
  `AUTH-S07-A-NOTICE-BEARING-V2-CONTRACT-001` sobre
  `main@6982b0643468aee0a97c3bea6b5bbe9018f0804c`, corpus `4.10.15`, working
  tree inicialmente limpa e OpenAPI v1/v2 protegidas. OpenAPI v1 permaneceu
  byte a byte no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; a nova revisão OpenAPI v2 possui
  SHA-256 `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733`
  e blob `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`. O contrato acrescenta somente
  `obligationSetId` às imagens e `DerivativeObligationPresentationV1` à citação
  PDF: valores `null` preservam a projeção legada, enquanto páginas
  notice-bearing exigem um único ID e apresentação completa coincidentes. Os
  tipos C# e o decoder estrito do Dashboard foram atualizados. Cinco testes do
  decoder e seis testes contratuais .NET passaram focalmente. Ao fim daquele
  incremento, schema, migration, renderer, armazenamento e comportamento
  notice-bearing ainda não estavam implementados. O PostgreSQL continua
  `BLOCKED/EXCLUDED`; nenhum novo A0, runtime, gate, Human Gate ou lifecycle foi
  executado.
- Schema e migrations notice-bearing: implementados sob
  `AUTH-S07-A-NOTICE-BEARING-SCHEMA-MIGRATION-001` no commit
  `98036f3c8c496544f4532d1fe48c981f836a1871`, sobre
  `main@564d9efd72285bb41545a5e60b63fcd44f9705fd`, corpus `4.10.16`, working
  tree inicialmente limpa e OpenAPI v1/v2 protegidas. O Control schema agora
  persiste `DerivativeObligationSetV1` imutável e seus blocos ordenados, aceita
  `pdf-page-png-notice-v1` junto ao perfil legado e vincula
  `obligationSetId`/digest e dimensões source/notice ao render manifest. As
  migrations `20260810033026_AddNoticeBearingObligationSchema` e
  `20260810034537_SealNoticeBearingObligationBindings` aplicam constraints,
  foreign keys e sealing triggers fail-closed sem backfill inferido ou mutação
  de registros, manifests, hashes ou ativações legados. Sete de sete testes
  focais passaram; não havia pending model changes; `foreign_key_check`,
  upgrade e rollback/reapply foram aprovados em stores SQLite temporários
  task-owned. O cleanup foi concluído. Renderer, PNG, comportamento
  notice-bearing, dataset, novo A0, gate, Human Gate e lifecycle não foram
  executados; o PostgreSQL permanece `BLOCKED/EXCLUDED`.
- Decisão arquitetural de armazenamento do corpus e evidência visual:
  [ADR-0008](../../docs/architecture/ADR-0008-Product-Corpus-Storage-And-Page-Image-Evidence.md)
  foi aceita explicitamente pelo proprietário em 2026-08-07 sobre
  `main@5c151c64ae4d3049d68fee6788502d439aa25251`, corpus `4.9.4` e working
  tree limpa. A aceitação estabelece autoridade arquitetural somente; não
  reconcilia ADR-0002, ADR-0004, segurança, módulo RAG, contratos, data
  dictionary, threat model, OpenAPI ou outro documento normativo, e não
  autoriza implementação, movimentação do PDF, geração de PNGs, dataset,
  indexação, ativação, testes, providers, rede, publicação ou ação externa.
- Decisão arquitetural de taxonomia de idiomas documentais:
  [ADR-0009](../../docs/architecture/ADR-0009-Document-Evidence-And-Query-Language-Taxonomy.md)
  foi aceita explicitamente pelo proprietário em 2026-08-07 sobre
  `main@89994e82d246b1cc0a240e99a2d09942e316f7cc`, corpus `4.9.4` e working
  tree limpa. `SupportedQueryLanguage` permanece restrito a `pt-BR` e
  `en-GB`; `DocumentContentLanguage` é um domínio BCP 47 distinto; o `en`
  declarado pelo PDF PostgreSQL não é inferido como `en-GB`; citações
  preservam o idioma original; e OpenAPI v1 permanece inalterada. A aceitação
  removeu somente o bloqueio decisório. Naquele registro, a reconciliação
  semântica da ADR-0008 ainda não havia sido executada; contratos, corpus
  normativo, dataset e runtime permaneceram inalterados, e a implementação
  continuou não autorizada.
- Reconciliação semântica conjunta de ADR-0008/0009: autorizada pelo
  proprietário em 2026-08-07 sobre
  `main@3d15ad4f2726f715c8dcf880491927ad0ff37b2f`, corpus `4.9.4` e working
  tree limpa. O corpus `4.9.5` alinha os 18 documentos canônicos confirmados
  confirmados para armazenamento permanente de fonte/PNG, content addressing,
  render lifecycle e direitos; separa `SupportedQueryLanguage=pt-BR|en-GB` de
  `DocumentContentLanguage` BCP 47; preserva `en`, citações no idioma original
  e estratos exatos de avaliação. OpenAPI v1 foi preservada byte a byte; v2 é
  somente contrato planejado e não implementado. O lote não alterou código,
  testes, schema, migrations, dados, dataset, registro de elegibilidade,
  dependências, lockfiles ou PDF; não gerou PNGs, indexou, ativou, executou
  provider/browser/rede nem realizou ação externa.
- Implementação corretiva `S03-CORR-01`: autorizada por
  `AUTH-S03-CORR-001` em 2026-08-07 sobre
  `main@ffc7bef913dda2699b072ef172188291f6ac0500`, corpus `4.9.5` e working
  tree limpa, com owner técnico de `STATE-03`. O runtime preflight dirigido
  encontrou zero processo e zero listener comprovadamente pertencente ao
  RAG-Challenge e nada encerrou. O commit
  `5fdbbc36d8eee29fdeec4b179564bd1eff322558` separa
  `SupportedQueryLanguage` de `DocumentContentLanguage`, preserva
  `SourceDeclaredLanguage` observado, mantém `en` distinto de `en-GB`, modela
  `DocumentPageImage`/`DocumentRenderManifest`, adiciona a única migration
  Control `20260807161323_AddDocumentLanguageAndRenderManifestModel`, propaga
  a separação por ingestão, indexação, consulta, provider, metadados vetoriais
  e Server, e protege fontes/imagens alcançadas por manifestos na limpeza.
- Verificação de `S03-CORR-01`: 19 testes unitários e 6 casos de integração
  focais passaram; `eng/ci.ps1 -Offline` aprovou 106 testes unitários, 116 de
  integração, 10 de arquitetura, 38 do Dashboard, cobertura de 93,74% de
  linhas e 67,11% de branches e auditoria de 212 arquivos. Upgrade legado,
  rollback/reapply, `foreign_key_check`, leitura vetorial e os dois pending
  model checks passaram somente em SQLite descartável. OpenAPI v1 permanece
  byte a byte no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  Git `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`. As novas tabelas ficaram
  vazias; não houve renderer, PNG, import, alteração do candidato PostgreSQL,
  dataset, ativação, serving, v2, dependência, lockfile, rede, ação externa,
  Automatic Quality Gate, Human Gate ou mudança de lifecycle.
- Implementação corretiva `S04-CORR-04-A`: autorizada por
  `AUTH-S04-CORR-04-A-001` em 2026-08-07 sobre
  `main@ea7fc582f991bb9290e26a7e2d4e074abc46bf3c`, corpus `4.9.7` e working
  tree limpa, com owner técnico de `STATE-04`. O runtime preflight dirigido
  encontrou zero processo de produto e zero listener do RAG-Challenge; nada
  foi encerrado antes da implementação. O commit
  `26f2e154b736687693b31ab02ca59cfb8ba86655` substitui o resultado mínimo do
  store por descritores tipados, implementa escrita bounded, idempotente,
  atômica e verificada por reabertura, exige hash/comprimento na reabertura e
  migra ingestão, composição e validação do control plane para o novo port.
  `IStorageMaintenance`, `cleanup-plan-v1` e o protocolo de reserva/finalização
  permanecem como única autoridade existente de exclusão física.
- Verificação de `S04-CORR-04-A`: 3 testes unitários e 57 casos de integração
  focais passaram; `eng/ci.ps1 -Offline` aprovou 109 testes unitários, 118 de
  integração, 10 de arquitetura, 38 do Dashboard, cobertura de 93,76% de
  linhas e 67,15% de branches e auditoria de 213 arquivos. OpenAPI v1
  permaneceu byte a byte no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
  Packages, lockfiles, schema, migrations, renderer, PNG, direitos, manifestos
  persistidos, digests de ativação, v2, dados reais e ações externas não foram
  alterados. O corpus `4.9.8` registra somente esse fato; `STATE-07` permanece
  ativo, sem gate ou transição, e nenhum incremento posterior foi iniciado.
- Implementação corretiva `S04-CORR-04-B`: autorizada por
  `AUTH-S04-CORR-04-B-001` em 2026-08-07 sobre
  `main@196bbcafcb493ce4e45a2c9e784965ff933f124d`, corpus `4.9.8` e working
  tree limpa, com owner técnico corretivo de `STATE-04`. O runtime preflight
  dirigido encontrou zero processo de produto e zero listener do
  RAG-Challenge; nada foi encerrado. O commit
  `a886a944ecd1ce485eee9c072385e96210e90520` introduz o registro tipado
  `DocumentRightsEligibilityRecordV1`, as dez decisões independentes de
  ADR-0008, os estados fechados `Permitted`, `Denied` e `Unproven`, referências
  estáveis de evidência e gates fixos textual/visual que aceitam somente
  permissões explícitas. Distribuição/publicação permanece decisão separada e
  não é inferida dos demais direitos.
- Verificação de `S04-CORR-04-B`: 14 casos unitários sintéticos focais passaram;
  `eng/ci.ps1 -Offline` aprovou 123 testes unitários, 118 de integração, 10 de
  arquitetura, 38 do Dashboard, cobertura de 93,72% de linhas e 67,20% de
  branches e auditoria de 216 arquivos. OpenAPI v1 permaneceu byte a byte no
  SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
  Packages, lockfiles, schema, migrations, renderer, PNG, persistência de
  direitos/manifests, ativação, v2, fonte/licença/direito/dado real, rede e
  ações externas não mudaram. O corpus `4.9.9` registra somente esse fato;
  `STATE-07` permanece ativo, sem gate ou transição, e `S04-CORR-04-C` ou
  incremento posterior não foi iniciado.
- Implementação corretiva `S04-CORR-04-C`: autorizada por
  `AUTH-S04-CORR-04-C-001` em 2026-08-07 sobre
  `main@75475c391c7fc1fb5ff298492a5d1da4c4f99fbb`, corpus `4.9.9` e working
  tree limpa, com owner técnico corretivo de `STATE-04`. O runtime preflight
  dirigido encontrou zero processo de produto e zero listener comprovadamente
  pertencente ao RAG-Challenge; nada foi encerrado. O gate de supply chain
  usou caches, CLI home e artefactos isolados, verificou as oito identidades e
  versões selecionadas, assinaturas, hashes, licenças, commits upstream,
  grafo, ausência de advisory/depreciação material e assets nativos Windows e
  Linux AArch64 antes da implementação. A evidência temporária permanece fora
  do Git, sem cleanup material.
- O commit `981e61c3308ee3407769d10ab1fa554007f12799` implementa o port de renderer,
  política explícita de limites, worker interno de um documento antes do host
  HTTP, framing binário privado, contenção por Job Object no Windows e
  `rlimit`/`prctl` no Linux, o perfil determinístico `pdf-page-png-v1`,
  validação estrutural fail-closed dos PNGs e finalização verificada,
  idempotente e atômica de `DocumentRenderManifest` nas tabelas existentes.
  O gate visual de direitos, a reabertura verificada da fonte e de cada PNG e
  a validação completa de todas as páginas precedem a persistência do manifest.
  `IStorageMaintenance`, `cleanup-plan-v1` e o protocolo de
  reserva/finalização permanecem a única autoridade de exclusão física.
- Verificação de `S04-CORR-04-C`: 7 casos unitários focais e 10 casos de
  integração focais passaram com bytes PDF/PNG sintéticos. O publish
  framework-dependent `linux-arm64` em modo locked/offline selecionou
  `libpdfium.so` e `libSkiaSharp.so` ELF64 AArch64 (`e_machine=183`). O
  `eng/ci.ps1 -Offline` aprovou 130 testes unitários, 128 de integração, 10 de
  arquitetura e 38 do Dashboard, com 93,53% de linhas e 66,80% de branches,
  build Release sem aviso e auditoria de 223 arquivos. Somente quatro
  lockfiles previstos mudaram; não houve projeto, schema, migration, model
  snapshot, endpoint ou contrato v2. OpenAPI v1 permaneceu byte a byte no
  SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
  Nenhum dado, fonte, licença ou direito real foi usado; não houve import,
  indexação, ativação, serving, cleanup, provider, ação externa, Automatic
  Quality Gate, Human Gate ou mudança de lifecycle. O corpus `4.9.10` registra
  somente esses fatos; `STATE-07` permanece ativo e `S04-CORR-04-D` ou
  incremento posterior não foi iniciado.
- Implementação corretiva `S04-CORR-04-D`: autorizada por
  `AUTH-S04-CORR-04-D-001` em 2026-08-07 sobre
  `main@548a817e2db4d9bad2d1a63e7dc9e9bb9ace418c`, corpus `4.9.10` e working
  tree limpa, com owner técnico corretivo de `STATE-04`. O runtime preflight
  dirigido encontrou zero processo de produto e zero listener comprovadamente
  pertencente ao RAG-Challenge; nada foi encerrado. O commit
  `d18224e46f559229a58e82b097abbf16ea9f359a` persiste por revisão o binding
  documental/fonte exato, snapshot imutável schema-v1 das dez decisões de
  direitos e render manifest obrigatório para PDF/ausente para CSV; exige os
  vínculos em Initial, Replacement e Rollback; revalida rollback e restringe
  ObservationRebind à mudança exclusiva de observação com evidência idêntica.
- O pre-CAS agora confere identidade completa, idioma documental suportado,
  gate textual/visual, geração finalizada, fonte reaberta e, para PDF, manifest
  finalizado, páginas físicas consecutivas e todos os PNGs reabertos. Replay
  compara os novos vínculos e direitos. Uma transação Control persiste revisão,
  bindings, evidência/direitos, retenção, head, auditoria e completion do
  journal aplicável; o readback de consulta falha fechado diante de revisão
  corrente incompleta ou divergente.
- A única migration Control
  `20260808004846_AddDocumentRightsAndActivationEvidenceBindings` cria somente
  `activation_evidence_bindings` e `activation_rights_decisions`, sem operação
  de dados ou backfill. O histórico conserva os campos existentes e não recebe
  direitos/manifests inferidos; a base Vector e os domínios de
  `sourceBindingSetDigest`/`activationBindingSetDigest` permanecem inalterados.
- Verificação de `S04-CORR-04-D`: seleções unitárias focais e 15 casos de
  integração focais passaram. Upgrade, rollback/reapply,
  `foreign_key_check`, compatibilidade histórica e os dois pending model checks
  passaram em SQLite descartável. `eng/ci.ps1 -Offline` aprovou 135 testes
  unitários, 137 de integração, 10 de arquitetura e 38 do Dashboard, com
  94,34% de linhas e 67,25% de branches, build Release e auditoria de 226
  arquivos aprovados. OpenAPI v1 permaneceu byte a byte no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e
  blob Git `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`. Nenhum dado/direito real,
  v2, serving, `AnswerEvidenceRecord`, rede, ação externa, gate ou mudança de
  lifecycle ocorreu. O corpus `4.9.11` registra somente esses fatos;
  `S04-CORR-04-E` não foi iniciado.
- Decisão arquitetural de evidência persistente de resposta:
  [ADR-0010](../../docs/architecture/ADR-0010-Persistent-Answer-Evidence-Records-And-Bounded-Retention.md)
  foi aceita explicitamente pelo proprietário em 2026-08-07 e registrada sob
  autoridade documental local, offline e sequencial sobre
  `main@745304051c113c86f5ebbaaaf625fbde74c50c6a`, corpus `4.9.11` e working
  tree limpa. A decisão fixa `S04-CORR-04-E` como o contrato interno persistente
  `AnswerEvidenceRecordV1`, somente para `Answered`, com retenção `P30D` sem
  refresh e participação em reachability. O corpus `4.10.0` registra somente a
  autoridade arquitetural e reconcilia fatos atuais; não implementa E, não cria
  migration/teste, não altera OpenAPI v1 e não inicia v2, serving, dado real,
  gate, lifecycle, rede ou ação externa.
- Implementação corretiva de evidência persistente de resposta: em 2026-08-08,
  sobre `main@fc83e1ea6922a519baf527efc3f0a219e2674453`, corpus `4.10.0`, working
  tree limpa e OpenAPI v1 no SHA-256 protegido, o proprietário autorizou
  exclusivamente a implementação local, offline e sequencial de
  `S04-CORR-04-E`. O runtime preflight dirigido encontrou zero processo ou
  listener comprovadamente pertencente ao produto. O incremento materializa o
  modelo/serialização canônica, o port e store Control `Answered`-only, commit e
  readback antes da resposta v1, replay/conflito, auditoria allowlisted e a
  migration vazia `20260808033247_AddAnswerEvidenceRecords`, sem backfill ou
  inferência histórica. `P30D` sem refresh participa do `cleanup-plan-v1`, da
  reserva, revalidação, finalização e reachability de fonte/PNGs. Build Release
  sem aviso, 146 testes unitários, 153 de integração, 10 de arquitetura e os
  dois pending-model checks passaram com fixtures e stores descartáveis. Essas
  verificações diretas não executaram Automatic Quality Gate. O corpus
  `4.10.1` registra somente esses fatos; OpenAPI v1 permanece byte a byte no
  SHA-256 protegido e v2, serving, dados reais, gates, lifecycle, rede e ações
  externas permanecem fora do escopo.
- Automatic Quality Gate corretivo de `S04-CORR-04-E`: executado localmente,
  offline e de forma sequencial em 2026-08-08 sobre
  `main@990d14172954567456d9ad90b6a767f6b6e0da78`, corpus `4.10.1`, working
  tree limpa e OpenAPI v1 no SHA-256 protegido. A auditoria estática identificou
  `AQG-S04-002` (P2): o contrato canônico afirma nas linhas 12–13 que a
  persistência de evidência permanece não implementada, mas o mesmo documento
  registra nas linhas 537 e 597–600 que `S04-CORR-04-E` a implementou
  localmente. O achado permanece `ABERTO` e o gate é `REPROVADO`. Pela parada
  obrigatória, runtime preflight, `eng/ci.ps1 -Offline` e os checks executáveis
  de build, testes, coverage, migration, restart, concorrência, falhas,
  retenção, cleanup, privacidade e reachability não foram iniciados. Nenhuma
  correção, Human Gate, mudança de lifecycle, rede ou ação externa ocorreu.
- Correção documental focal de `AQG-S04-002`: autorizada pelo proprietário e
  executada em 2026-08-08 sobre
  `main@3f42214c5c3554b6b341ab4c75a0a8e3cdb93f2a`, corpus `4.10.1`, working
  tree limpa e OpenAPI v1 no SHA-256 protegido. O parágrafo de finalidade do
  contrato canônico agora registra que a evidência persistente de resposta foi
  implementada localmente pelo incremento separadamente autorizado
  `S04-CORR-04-E`; image serving e v2 permanecem não implementados. A semântica
  aceita da ADR-0010 e a implementação não mudaram. `AQG-S04-002` está
  `CORRECTED_PENDING_GATE_RETEST`; o Automatic Quality Gate histórico permanece
  `REPROVADO` e não foi reiniciado. Nenhum source, teste, comportamento, schema,
  migration, ADR, OpenAPI, Human Gate, lifecycle, rede ou ação externa mudou.
- Reinício integral do Automatic Quality Gate corretivo após a correção de
  `AQG-S04-002`: autorizado e iniciado em 2026-08-08 sobre
  `main@da569d8dae6990db72e43df69f1ff0bb8861ac54`, corpus `4.10.1`, working
  tree completamente limpa e OpenAPI v1 no SHA-256 protegido. O runtime
  preflight dirigido encontrou zero processo ou listener comprovadamente
  pertencente ao produto; nada foi encerrado. A inspeção estática confirmou a
  correção e dispôs `AQG-S04-002` como `RESOLVIDO`, mas identificou
  `AQG-S04-003` (P2): a seção de verificações obrigatórias do contrato canônico
  ainda descreve nas linhas 788–791 os testes de answer-evidence como futuros,
  enquanto o baseline contém e registra as suítes unitárias e de integração já
  implementadas e diretamente executadas. A condição de parada foi acionada;
  `eng/ci.ps1 -Offline`, build, testes, coverage, migration, restart,
  concorrência, falhas, retenção, cleanup, privacidade e reachability não foram
  executados neste reinício. O gate é `REPROVADO`, `AQG-S04-003` permanece
  `ABERTO` e nenhuma correção, Human Gate, mudança de lifecycle, rede ou ação
  externa ocorreu.
- Correção documental focal de `AQG-S04-003`: autorizada pelo proprietário e
  executada em 2026-08-08 sobre
  `main@cb67c7f752521f416f46d9cb4f2bb6a189ca1a48`, corpus `4.10.1`, working
  tree completamente limpa e OpenAPI v1 no SHA-256 protegido. A seção de
  verificações obrigatórias do contrato canônico agora classifica os testes de
  answer-evidence como requisitos, não como trabalho futuro, sem converter o
  documento arquitetural em evidência de implementação ou execução. O escopo e
  a cobertura descritos não mudaram. `AQG-S04-003` está
  `CORRECTED_PENDING_GATE_RETEST`; o Automatic Quality Gate histórico permanece
  `REPROVADO` e não foi reiniciado. Nenhum source, teste, comportamento, schema,
  migration, ADR-0010, OpenAPI, v2, serving, Human Gate, lifecycle, rede ou ação
  externa mudou.
- Reinício integral do Automatic Quality Gate corretivo após a correção de
  `AQG-S04-003`: autorizado e iniciado em 2026-08-08 sobre
  `main@baa85553f9d48c7c833b1e875699817849360bab`, corpus `4.10.1`, working
  tree completamente limpa e OpenAPI v1 no SHA-256 protegido. A inspeção
  estática confirmou a correção e dispôs `AQG-S04-003` como `RESOLVIDO`, mas
  identificou `AQG-S04-004` (P2): ADR-0010 exige testes diretos de rejeição para
  mismatches de citação, fonte, ativação, manifest e página, enquanto a suíte
  focal de persistência testa somente um mismatch do digest no header de
  ativação. O teste de domínio das páginas verifica ausência/excesso contra o
  próprio registro, sem confrontar valores divergentes com a autoridade Control
  persistida. Os ramos fail-closed existem na implementação, mas a matriz de
  regressão exigida está incompleta. A condição de parada foi acionada antes do
  runtime preflight, `eng/ci.ps1 -Offline`, build, testes, coverage, migration,
  restart, concorrência, falhas, retenção, cleanup, privacidade e reachability.
  O gate é `REPROVADO`, `AQG-S04-004` permanece `ABERTO` e nenhuma correção,
  Human Gate, mudança de lifecycle, rede ou ação externa ocorreu.
- Correção focal de `AQG-S04-004`: autorizada e executada em 2026-08-08 sobre
  `main@fd2e164ef1d8b1a90d867f4e77beea0e43fba9c3`, corpus `4.10.1`, working
  tree completamente limpa e OpenAPI v1 no SHA-256 protegido. A suíte focal de
  persistência SQLite agora confronta, um valor por caso, mismatches de
  citação, fonte, ativação, manifest e página com a autoridade Control
  persistida. Os cinco casos rejeitam com `InvalidDataException` e comprovam
  ausência de header, citações, páginas, operação administrativa e auditoria
  de criação de answer-evidence. O arquivo focal aprovou 14 casos e o projeto
  de integração afetado aprovou 157 casos em Release, sem restore. Nenhum
  defeito de implementação foi demonstrado e nenhuma mudança de produto foi
  necessária. `AQG-S04-004` está `CORRECTED_PENDING_GATE_RETEST`; o Automatic
  Quality Gate histórico permanece `REPROVADO` e não foi reiniciado. Nenhum
  source, comportamento, schema, migration, ADR-0010, OpenAPI, v2, serving,
  Human Gate, lifecycle, rede ou ação externa mudou.
- Reinício integral do Automatic Quality Gate corretivo após a correção de
  `AQG-S04-004`: autorizado e executado em 2026-08-08 sobre
  `main@5a2dcbafdc0a3925338043b079f9eacc9e70ca1b`, corpus `4.10.1`, working
  tree completamente limpa e OpenAPI v1 no SHA-256 protegido. A inspeção
  estática dispôs `AQG-S04-004` como `RESOLVIDO`; o preflight dirigido encontrou
  zero processo e zero listener TCP comprovadamente pertencentes ao produto e
  não parou nada. `eng/ci.ps1 -Offline` aprovou restore locked, formato, build
  Release sem aviso ou erro, 146 testes unitários, 157 de integração, 10 de
  arquitetura e 38 do Dashboard. A cobertura .NET foi 94,91% de linhas e
  67,42% de branches; lint, typecheck, build web e auditoria dos 235 arquivos
  também passaram. O gate está `APROVADO`, `AQG-S04-002` a `AQG-S04-004` estão
  `RESOLVIDOS` e nenhum novo P0, P1, P2 ou P3 foi identificado. A baseline
  protegida permaneceu intacta e sem runtime residual. Nenhum source, teste,
  comportamento, schema, migration, ADR-0010, OpenAPI, v2, serving, Human Gate,
  lifecycle, rede ou ação externa mudou.
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
  humana explícita em 2026-08-02; ADR-0008 e ADR-0009: `accepted` por decisões
  humanas explícitas e independentes em 2026-08-07, com reconciliação semântica
  conjunta aplicada no corpus `4.9.5`; ADR-0010: `accepted` por decisão humana
  explícita em 2026-08-07, com registro e reconciliação documental no corpus
  `4.10.0`. A aceitação de ADR não substitui as autoridades de implementação:
  `S04-CORR-04-A` a `S04-CORR-04-E` estão concluídos na fronteira local,
  offline e sintética autorizada; isso não constitui gate ou homologação.
- ADR-0011: `accepted` pela decisão humana explícita
  `ADR-0011: ACEITAR.` em 2026-08-09. A decisão refina o mapeamento de
  evidência de ADR-0004/ADR-0008; sua reconciliação semântica nos proprietários
  documentais nomeados foi aplicada no corpus `4.10.10`. A correção interna do
  serving foi implementada no commit
  `b9c3e5f3a72c2dd7762c256198452ae2c217b2d2`; o A0 candidato-específico
  posterior preservou o PostgreSQL `BLOCKED/EXCLUDED`, sem comportamento com
  dado de produto.
- ADR-0012: `accepted` pela decisão humana explícita
  `ADR-0012: ACEITAR.` em 2026-08-09. A decisão estabelece a imagem composta
  autocontida e as mudanças necessárias de schema, migration e contrato v2.
  Sua reconciliação semântica nos seis proprietários documentais foi aplicada
  no corpus `4.10.15`, e a revisão protegida do contrato v2 foi congelada no
  corpus `4.10.16`. Schema e migrations foram implementados no commit
  `98036f3c8c496544f4532d1fe48c981f836a1871`; o comportamento notice-bearing
  foi implementado no commit
  `f682827d1a26b08fa8c450a1fadb3bd0e1fa1700` e possui verificação focal, mas
  seu Automatic Quality Gate próprio permanece `NOT_RUN`. O PostgreSQL não foi
  reclassificado.
- ADR-0013: `accepted` pela decisão humana explícita
  `ADR-0013: ACEITAR.` em 2026-08-10 sobre
  `main@f03162bad0fc166a597739b22e55fbc46ec59535`, corpus `4.10.17`. A decisão
  seleciona `gpt-5.4-mini-2026-03-17` como único candidato de LLM do MVP,
  substitui somente a seleção anterior de LLM do ADR-0005 e mantém
  `gpt-5.6-sol` inativo para avaliação futura. Sua reconciliação semântica no
  ADR-0005, no relatório arquitetural de `STATE-02` e no índice de arquitetura
  foi aplicada no corpus `4.10.19`. O incremento local, offline e
  determinístico de compatibilidade do adaptador foi implementado sob
  `AUTH-STATE07-LLM-ADAPTER-COMPAT-001` no commit
  `b6d6f9102ecf0ea93309f8080acebad02cf16584` e reconciliado factualmente no
  corpus `4.10.20`. O Automatic Quality Gate específico foi `APROVADO`, sem
  achado P0, P1, P2 ou P3, sob
  `AUTH-STATE07-LLM-ADAPTER-COMPAT-AQG-001` na baseline
  `main@6e6fdabb91e2fb4c5186c464ce08f5da390d727a`, corpus `4.10.20`. A evidência
  permanece limitada a Infrastructure, testes locais offline e handlers
  falsos; não houve configuração operacional, conta, credencial, chamada ao
  provider, corpus real, OCI, deploy, avaliação real, Human Gate ou mudança de
  lifecycle. A preparação local, offline e determinística da campanha
  `s07-a-provider-gpt54m-candidate-001` foi concluída sob
  `AUTH-S07-A-PROVIDER-PREP-001` no commit
  `422286863e7a3c213e96db18144769bd0458a75b` e reconciliada factualmente no
  corpus `4.10.22`. Ela materializa uma revisão sucessora sintética, imutável e
  não pontuada, com harness limitado a handlers falsos; não executa ou homologa
  o provider. O Automatic Quality Gate específico da preparação foi
  `APROVADO`, sem achado P0, P1, P2 ou P3, sob
  `AUTH-S07-A-PROVIDER-PREP-AQG-001` na baseline
  `main@5d74c9c9190b0b3465b11dc6864e3dd519cc88f9`, corpus `4.10.22`, somente na
  fronteira local, offline, determinística e com handlers falsos.
- ADR-0014: `accepted` pela decisão humana explícita `ADR-0014: ACEITAR.` em
  2026-08-11 sobre `main@52e1ac7d9bc61be196549a8ee61399fde477b8fb`, corpus
  `4.10.26`, working tree limpa e OpenAPI v1/v2 protegidas. A decisão registra
  a ordenação existente `Score DESC, global ChunkOrdinal ASC`, preserva
  `retrieval-v1` para entradas válidas, define a porta Application retrieval-
  only tipada e fail-closed e estabelece o desenho governado da baseline de
  retrieval. `retrieval-multi-query-v1-candidate` permanece estacionado. O
  corpus `4.10.27` reconcilia somente essa autoridade arquitetural sob
  `AUTH-STATE07-RETRIEVAL-DETERMINISM-ADR-RECONCILE-001`. Sob autoridade humana
  posterior e separada, concedida sobre
  `main@ade89d737975f65c38e88b35758f8c6091e57406`, corpus `4.10.27`, o
  `DR-2 — Determinism implementation` foi concluído no commit focal
  `fabb24cad16201070e3b95fffb22467cd55963ab`. O corpus `4.10.28` reconcilia
  factualmente a porta Application retrieval-only tipada, a configuração fixa
  integral, as validações finitas e de ordem total e os outcomes fail-closed.
  A evidência focal registrada — build sem avisos ou erros, 74 testes unitários
  focais, 8 de integração locais/SQLite, 11 de arquitetura e auditoria de 279
  arquivos — não constituía `DR-3` ou Automatic Quality Gate. Posteriormente,
  sob autoridade humana separada sobre
  `main@272a868c2f2a90eba21ee422ba5a2c34aa2337d5`, corpus `4.10.28`, o
  `DR-3 — Determinism Automatic Quality Gate` foi executado localmente, offline
  e de forma determinística e terminou `REPROVADO`, com `DR3-FIND-001` P1 e
  `DR3-FIND-002`, `DR3-FIND-003` e `DR3-FIND-004` P2. Os checks focais e a CI
  offline completa passaram, mas não superam o defeito numérico P1 nem as três
  lacunas de prova P2. Dataset, campanha, provider, rede, chamada paga, OpenAPI,
  schema, migration, Human Gate e lifecycle não foram executados ou alterados;
  MultiQuery continua estacionado. Após a correção versionada e sua
  reconciliação, o reteste corretivo independente autorizado por
  `AUTH-DR3-NUMERIC-SEMANTICS-AQG-RETEST-001` foi `APROVADO` sobre
  `main@bf8a156e7c5eea801f29fb6e7742cac880783bc0`, corpus `4.10.32`, sem novo
  achado P0, P1, P2 ou P3; `DR3-FIND-001` a `DR3-FIND-004` estão `RESOLVED`.
- `RB-1 — Evaluation design freeze`: concluído documentalmente sob
  `AUTH-RB1-EVALUATION-DESIGN-FREEZE-001` sobre a baseline limpa
  `main@45cbcf2624262572abf8180498ac63709a9130e4`, corpus `4.10.33`, com as
  quatro identidades protegidas de OpenAPI preservadas. A revisão imutável
  `retrieval-v2-evaluation-design-v1` está
  `frozen-unmaterialised-unscored` em 28 artefatos normativos — oito instâncias
  de desenho e 20 schemas Draft 2020-12 — vinculados por inventário fechado e
  SHA-256. O contrato-raiz possui self-digest
  `0e8d928aee055211773d83eb33f2d54485033c81cfad15dd95b0fdd551f8ed08`,
  38 células contratuais e 10 células de elegibilidade somente definidas e os
  sete contadores de materialização em zero. Nenhum documento/caso de produto,
  pergunta, qrel, vetor, geração, resultado ou pontuação foi criado. Build,
  testes executáveis, scorer, campanha, Automatic Quality Gate, Human Gate,
  lifecycle e ação externa permaneceram `NOT_RUN`. `RB-2` continua sem
  autorização.
- ADR-0015: `accepted` pela decisão humana explícita `ADR-0015: ACEITAR.` em
  2026-08-11 sobre `main@46de807148d5b547f56a0f7265b32428b232100f`, corpus
  `4.10.30`, working tree limpa e OpenAPI v1/v2 protegidas. A decisão seleciona
  `cosine-f32mul-f64acc-boundary-canonical-v1`, `retrieval-v2` e o descritor
  `/2`, com novo `IndexCompatibilityKey`, geração e baseline de avaliação antes
  de servir; corredor exato de 1 ULP e aritmética escalada em binary64
  permanecem alternativas não selecionadas. Posteriormente, sob
  `AUTH-DR3-NUMERIC-SEMANTICS-IMPLEMENTATION-001`, o commit
  `9addb166e82dd04581beee7b4276a74977fe04c5` implementou a semântica, a política,
  a compatibilidade fail-closed e as quatro correções de prova. A implementação
  não criou nem ativou geração de produto e, naquele incremento, não repetiu o
  gate: `DR-3` continuou `REPROVADO`, com os quatro achados
  `CORRECTED_PENDING_GATE_RETEST`. O reteste independente posterior foi
  `APROVADO` e dispôs os quatro achados como `RESOLVED`, preservando a evidência
  histórica anterior.
- Fechamento sanitizado da chave administrativa de provisionamento: o cleanup
  concluído sob `AUTH-S07-A-PROVIDER-ADMIN-KEY-CLEANUP-002` foi reconciliado
  documentalmente sob
  `AUTH-S07-A-PROVIDER-ADMIN-KEY-CLEANUP-RECONCILE-001`, sobre
  `main@b2654088d11ab94c23cdf19e2aa57d89f0b3ae49`, corpus `4.10.24`, working
  tree inicialmente limpa e OpenAPI v1/v2 protegidas. Segundo o registro de
  fechamento sanitizado fornecido pelo proprietário, a Admin key com label
  exato `s07-a-provider-gpt54m-candidate-001-admin-provisioning` foi revogada,
  está ausente do inventário Active e aparece historicamente somente como
  Inactive; `Last used` permaneceu `Never` e o gasto permaneceu `USD 0.00`. O
  target `RAG-Challenge/OpenAI/AdminKey/s07-a-provider-gpt54m-candidate-001`
  foi removido do Windows Credential Manager e sua ausência foi verificada no
  cleanup autorizado. Esta reconciliação não reacessou esses sistemas e não
  reteve secret, fragmento, fingerprint ou representação mascarada. Não houve
  chamada de provider ou `/v1/responses`, custo novo, alteração de billing,
  limites, allowlist ou projeto, Human Gate ou lifecycle.
- Reauditoria das fronteiras de preflight e homologação: o preflight operacional
  inicial de `s07-a-provider-gpt54m-candidate-001` foi finalizado como
  `BLOQUEADO`, sem campanha real, chamada de provider ou `/v1/responses`. O
  cleanup posterior da Admin key e de sua credencial local está encerrado. O
  fluxo experimental posterior de Coordinator/Docker/C3 foi revogado e não
  constitui autoridade vigente nem pendência canônica. O mecanismo
  notice-bearing existe desde o commit
  `f682827d1a26b08fa8c450a1fadb3bd0e1fa1700`, mas não reclassifica
  retroativamente o A0: as quatro operações visuais permanecem `UNPROVEN`
  porque o Automatic Quality Gate notice-bearing e um novo A0
  candidato-específico não foram executados. O gate, sua eventual
  reconciliação documental e o novo A0 são três etapas separadas e permanecem
  `NOT_RUN`.

## Baseline documental

- Os 20 arquivos da estrutura originalmente aprovada permanecem preservados;
  a política de idioma acrescentou o 21º documento público por incremento
  versionado, e o ADR-0003 acrescentou o 22º.
- A baseline aprovada no Human Gate de `STATE-00` permanece `3.4.0`.
- O corpus de instruções vigente possui versão `4.10.34` e 13 arquivos em
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
- O corpus `4.10.7` registra a instrução permanente do proprietário de receber
  primeiro uma explicação prática, concisa e em linguagem simples, adequada a
  quem não possui conhecimento técnico especializado. Termos técnicos
  necessários passam a exigir significado e consequência explicados em
  `pt-BR`, sem ocultar incerteza, risco, limite de autoridade ou fato não
  verificado.
- O corpus `4.10.8` registra somente a preparação do ADR-0011 como proposta.
  A mudança documenta o mapeamento condicionado de evidência primária, a
  fronteira entre serving same-origin e distribuição/publicação, as obrigações
  que acompanham derivados e a incompatibilidade estática entre o contrato v2
  e a política interna. Não aceita o ADR, muda direitos, contrato público ou
  comportamento de produto.
- O corpus `4.10.9` registra a decisão explícita `ADR-0011: ACEITAR.` somente
  como autoridade arquitetural. A aceitação não executa a reconciliação
  semântica, a correção interna da política, novo A0 ou qualquer mudança de
  direito, contrato público, comportamento, gate ou lifecycle.
- O corpus `4.10.10` aplica a reconciliação documental autorizada do ADR-0011
  a ADR-0004, ADR-0008, ao registro de elegibilidade e ao contrato documental
  v2. A reconciliação preserva o candidato PostgreSQL bloqueado, não altera
  OpenAPI ou comportamento e mantém a correção interna e o novo A0 sob
  autoridades posteriores.
- O corpus `4.10.11` registra a correção interna posteriormente implementada no
  commit `b9c3e5f3a72c2dd7762c256198452ae2c217b2d2`. A política de serving
  passa a avaliar as dez decisões e a falhar fechado diante de distribution
  boundary `Unproven`, sem alterar OpenAPI, contrato público, candidato ou
  lifecycle. Novo A0 permanece posterior e separadamente autorizado.
- O corpus `4.10.12` registra o A0 candidato-específico autorizado sob
  `AUTH-S07-A-PRODUCT-A0-002`. Quatro operações visuais permanecem `UNPROVEN`
  por ausência de mecanismo determinado para os avisos exigidos em todas as
  cópias, a distribuição/publicação externa está `DENIED` pela fronteira
  interna excluída e o candidato permanece `BLOCKED/EXCLUDED`.
- O corpus `4.10.13` registra somente a preparação do ADR-0012 como proposta.
  O novo perfil preserva a região da página pixel a pixel e acrescenta um
  painel autocontido de obrigações, com registro imutável, manifest, recovery,
  serving e apresentação acessível. A proposta identifica mudanças futuras de
  schema, migration e contrato v2, preserva v1 e o fail-closed e não aceita o
  ADR, reclassifica o candidato ou altera comportamento.
- O corpus `4.10.14` registra a decisão explícita `ADR-0012: ACEITAR.` somente
  como autoridade arquitetural. A aceitação não executa reconciliação,
  revisão do contrato v2, schema, migration, implementação, novo A0, renderer,
  dataset, gate ou lifecycle e mantém OpenAPI v1/v2 protegidas.
- O corpus `4.10.15` aplica a reconciliação semântica autorizada do ADR-0012
  aos seis proprietários documentais. Registra o perfil notice-bearing, o
  obligation set, seus vínculos de manifest/storage/reachability/recovery/
  serving/acessibilidade e as futuras revisões obrigatórias de contrato v2,
  schema e migration. Não altera OpenAPI, implementação, direitos do candidato,
  gate ou lifecycle.
- O corpus `4.10.16` registra o contrato público v2 notice-bearing congelado.
  OpenAPI v2 e seus tipos/decoders estritos acrescentam somente a identidade do
  obligation set e sua apresentação completa; a rota e todos os campos
  anteriores permanecem. A compatibilidade legada usa valores `null`, enquanto
  o caso notice-bearing falha fechado em mistura, ausência ou divergência.
  OpenAPI v1, schema, migration, direitos, dataset, gate e lifecycle não mudam.
- O corpus `4.10.17` reconcilia a implementação do schema e das duas migrations
  notice-bearing no commit `98036f3c8c496544f4532d1fe48c981f836a1871`.
  Registra obrigação imutável e blocos ordenados, coexistência dos perfis,
  vínculo e digest do obligation set, dimensões source/notice, constraints,
  foreign keys e sealing triggers fail-closed, sem backfill ou mutação legada.
  Renderer, PNG, serving notice-bearing, Dashboard, direitos, dataset, novo A0,
  gate e lifecycle permanecem inalterados ou `NOT_RUN`.
- O corpus `4.10.18` registra a decisão explícita `ADR-0013: ACEITAR.` somente
  como autoridade arquitetural. `gpt-5.4-mini-2026-03-17` passa a ser o único
  candidato de LLM do MVP e `gpt-5.6-sol` permanece inativo para avaliação
  futura, com risco de identificador móvel registrado. A aceitação não executa
  reconciliação semântica, implementação, acesso a conta, credencial, provider,
  chamada paga, corpus real, OCI, deploy, gate ou lifecycle e mantém OpenAPI
  v1/v2 protegidas.
- O corpus `4.10.19` aplica sob
  `AUTH-STATE07-LLM-CANDIDATE-ADR-RECONCILE-001` a reconciliação semântica
  documental do ADR-0013 aceito. ADR-0005 e o relatório arquitetural de
  `STATE-02` agora selecionam `gpt-5.4-mini-2026-03-17`, preservam a seleção
  anterior como fato histórico e mantêm `gpt-5.6-sol` somente como candidato
  futuro inativo. Nenhuma outra decisão do ADR-0005 muda; código, testes,
  OpenAPI, configuração, provider, conta, credencial, chamada paga, corpus
  real, OCI, deploy, gate e lifecycle permanecem inalterados ou `NOT_RUN`.
- O corpus `4.10.20` reconcilia sob
  `AUTH-STATE07-LLM-ADAPTER-COMPAT-RECONCILE-001` o incremento implementado no
  commit `b6d6f9102ecf0ea93309f8080acebad02cf16584`. O adaptador exige o snapshot
  exato `gpt-5.4-mini-2026-03-17`, usa configuração tipada e imutável para
  `reasoning.effort=none` e `reasoning.context=current_turn`, preserva
  `store=false`, não emite `tools` nem parâmetros não comprovados e valida
  estritamente a mensagem estruturada final. Os testes locais com handler
  falso aprovaram 18 de 18 casos, e os 11 testes de arquitetura também
  passaram. Esses resultados não constituem chamada ao provider, avaliação
  bilíngue ou de qualidade, homologação, Automatic Quality Gate, Human Gate,
  deploy ou mudança de lifecycle.
- O corpus `4.10.21` reconcilia sob
  `AUTH-STATE07-LLM-ADAPTER-COMPAT-AQG-RECONCILE-001` o Automatic Quality Gate
  aprovado sem achados na baseline
  `main@6e6fdabb91e2fb4c5186c464ce08f5da390d727a`. A auditoria confirmou os sete
  requisitos do ADR-0013, 18 de 18 testes focais, 11 de 11 testes de
  arquitetura e a CI offline completa com 154 testes unitários, 191 de
  integração, 11 de arquitetura e 45 do Dashboard; cobertura de 95,63% de
  linhas e 67,65% de branches; build sem avisos ou erros. A aprovação vale
  somente para compatibilidade local, offline, determinística e com handlers
  falsos. Provider real, avaliação bilíngue, groundedness, citações,
  insuficiência de evidência, prompt injection, latência, custo, corpus real,
  OCI, deploy, Human Gate e lifecycle permanecem `NOT_RUN`.
- O corpus `4.10.22` reconcilia sob
  `AUTH-STATE07-S07-A-PROVIDER-PREP-RECONCILE-001` a preparação concluída no
  commit `422286863e7a3c213e96db18144769bd0458a75b`. A revisão sucessora
  `rag-eval-catalogue-v1-provider-gpt54m-candidate-001` preserva a revisão
  congelada anterior e registra dois documentos sintéticos, 60 casos, 40
  casos respondíveis distribuídos em dez por cada direção obrigatória
  `pt-BR`/`en-GB`, 20 casos de insuficiência e 12 casos de prompt injection.
  Prompt, schema, snapshot `gpt-5.4-mini-2026-03-17`, configuração, limites,
  agenda máxima de 109 chamadas, orçamento operacional de `USD 16` e teto
  absoluto de `USD 20` estão congelados. O harness e os testes usaram somente
  handlers falsos; provider real, qualidade bilíngue, groundedness, citações,
  insuficiência de evidência real, resistência a prompt injection, latência,
  custo observado, Automatic Quality Gate, Human Gate e lifecycle permanecem
  `NOT_RUN`.
- O corpus `4.10.23` reconcilia sob
  `AUTH-STATE07-S07-A-PROVIDER-PREP-AQG-RECONCILE-001` o Automatic Quality Gate
  aprovado sem achados na baseline
  `main@5d74c9c9190b0b3465b11dc6864e3dd519cc88f9`. A auditoria preservou a
  predecessora, confirmou os cinco manifests e seus digests, os 60 casos, 12
  casos de prompt injection, 20 casos de insuficiência, agenda máxima de 109
  chamadas e orçamento congelado de `USD 16`/`USD 20`. Passaram 2 de 2 testes
  focais, 20 de 20 testes combinados e a CI offline completa com 154 testes
  unitários, 193 de integração, 11 de arquitetura e 45 do Dashboard; cobertura
  de 95,63% de linhas e 67,66% de branches; build sem avisos ou erros. A
  aprovação vale somente para preparação local, offline, determinística e com
  handlers falsos. Conta, credencial, provider, chamada paga, corpus/fonte real,
  avaliação real, qualidade bilíngue, groundedness, citações, insuficiência de
  evidência real, resistência a prompt injection, latência, custo observado,
  OCI, deploy, Human Gate e lifecycle permanecem `NOT_RUN`.
- O corpus `4.10.24` reconcilia sob
  `AUTH-S07-A-NOTICE-BEARING-PROFILE-IMPL-RECONCILE-001` a implementação
  notice-bearing do commit
  `f682827d1a26b08fa8c450a1fadb3bd0e1fa1700`. O obligation set imutável, o
  compositor PNG com região da página preservada, os vínculos de manifest,
  persistence/reachability, readback/serving v2 fail-closed e a apresentação
  acessível do Dashboard estão implementados. A evidência focal observada foi
  build Release limpo, 47 testes unitários, 40 de integração/contrato, 11 de
  arquitetura e 45 do Dashboard, além de build e lint do Dashboard. Isso não é
  Automatic Quality Gate. Novo A0, dado de produto, browser/tecnologia
  assistiva, Human Gate e lifecycle permanecem `NOT_RUN`; o PostgreSQL continua
  `BLOCKED/EXCLUDED`.
- O corpus `4.10.25` reconcilia sob
  `AUTH-S07-A-PROVIDER-ADMIN-KEY-CLEANUP-RECONCILE-001` o fechamento concluído
  sob `AUTH-S07-A-PROVIDER-ADMIN-KEY-CLEANUP-002`. O registro sanitizado
  documenta revogação e estado histórico Inactive da Admin key nomeada,
  `Last used` `Never`, gasto `USD 0.00` e remoção verificada do target do
  Windows Credential Manager. Nenhum secret é retido. A reconciliação não
  reacessa OpenAI, Credential Manager, provider, billing ou projeto e não
  executa chamada, custo, mudança de configuração, gate ou lifecycle.
- O corpus `4.10.26` reconcilia sob
  `AUTH-STATE07-PREFLIGHT-BOUNDARY-REAUDIT-RECONCILE-001` as reauditorias das
  tarefas “Preflight operacional GPT-5.4-mini” e “Next Homologation Boundary”.
  O preflight inicial permanece encerrado como `BLOQUEADO`, sem campanha real;
  o cleanup administrativo e local está encerrado; e o fluxo experimental
  Coordinator/Docker/C3 está revogado, sem autoridade ou pendência canônica.
  O mecanismo notice-bearing implementado em `f682827d` não altera o A0
  histórico. Automatic Quality Gate notice-bearing, eventual reconciliação de
  seu resultado e novo A0 permanecem separados e `NOT_RUN`.
- O corpus `4.10.27` registra sob
  `AUTH-STATE07-RETRIEVAL-DETERMINISM-ADR-RECONCILE-001` a decisão explícita
  `ADR-0014: ACEITAR.` somente como autoridade arquitetural. A ordenação
  `Score DESC, global ChunkOrdinal ASC` torna-se contrato explícito sem alterar
  resultados válidos de `retrieval-v1`. Naquela reconciliação, a porta tipada,
  as falhas fail-closed, o `retrieval-evaluation-scorer-v1` e os freezes de
  design, dataset e `campaign-input` permaneciam requisitos futuros não
  implementados; implementação, teste executável, dataset, campanha, provider,
  rede, chamada paga, OpenAPI, schema, migration, gate, Human Gate e lifecycle
  estavam `NOT_RUN`. MultiQuery continuava estacionado.
- O corpus `4.10.28` reconcilia factualmente o
  `DR-2 — Determinism implementation`, autorizado separadamente sobre
  `main@ade89d737975f65c38e88b35758f8c6091e57406` e implementado no commit
  `fabb24cad16201070e3b95fffb22467cd55963ab`. A porta Application retrieval-
  only tipada, as identidades fixas de política e geração, a validação de query,
  vetores, normas, scores finitos em `[-1, 1]`, ordinal global, identidades e
  ordem `Score DESC, global ChunkOrdinal ASC`, além dos outcomes fail-closed,
  estão implementados. `retrieval-v1` preserva top-k `8`, mínimo inclusivo
  `0.25`, máximo de seis evidências, orçamento de 16.000 escalares e score `0`
  para stored zero-vector. A evidência focal observada foi build sem avisos ou
  erros, 74 testes unitários focais, 8 de integração locais/SQLite, 11 de
  arquitetura e auditoria documental aprovada para 279 arquivos. Isso não
  executa nem aprova `DR-3`.
  Dataset, `retrieval-evaluation-scorer-v1`, campanha, provider, corpus real,
  rede, chamada paga, OpenAPI, schema, migration, Human Gate e lifecycle
  permanecem `NOT_RUN`; MultiQuery continua não canônico e estacionado.
- O corpus `4.10.29` reconcilia factualmente o
  `DR-3 — Determinism Automatic Quality Gate`, executado sob autoridade humana
  separada sobre `main@272a868c2f2a90eba21ee422ba5a2c34aa2337d5`, corpus
  `4.10.28`, e encerrado `REPROVADO`. `DR3-FIND-001` P1 registra que vetores
  admissíveis idênticos `[1f, 1f, 1f]` produzem score
  `1.0000000000000002`, convertido em `InvalidIndexData` e
  `CH_INDEX_UNAVAILABLE`. `DR3-FIND-002` P2 registra que o teste de
  determinismo não prova adversarialmente o sort completo antes de `Take(k)`;
  `DR3-FIND-003` P2 registra ausência de prova dos filtros antes de score/top-k
  com hits elegíveis e inelegíveis concorrentes; e `DR3-FIND-004` P2 registra
  ausência de regressão executável para `ChunkOrdinal < 0`. A implementação
  observada aplica filtros e depois `OrderByDescending(Score)`,
  `ThenBy(ChunkOrdinal)` e `Take(k)`; os três P2 são lacunas de prova, não
  defeitos comportamentais observados.
  O gate registrou build Release sem avisos ou erros; 74/74 testes unitários
  focais, 35/35 de integração focais e 11/11 de arquitetura; 3/3 execuções
  independentes do caso de empate/reopen; e CI offline completa com 201 testes
  unitários, 197 de integração, 11 de arquitetura e 45 do Dashboard, cobertura
  de 95,53% de linhas e 68,34% de branches e auditoria de 279 arquivos. Esses
  checks aprovados não superam os quatro achados. Nenhum arquivo rastreado,
  dataset, contrato ou configuração foi alterado pelo gate; somente outputs
  ignorados dos checks foram materializados. Nenhuma semântica numérica ou
  correção foi definida; dataset, scorer, campanha, provider, corpus real,
  rede, chamada paga, OpenAPI, schema, migration, MultiQuery, Human Gate e
  lifecycle não foram executados ou alterados.
- O corpus `4.10.30` materializa somente a proposta arquitetural ADR-0015 sob
  `AUTH-DR3-NUMERIC-SEMANTICS-PROPOSAL-001`. A alternativa recomendada, ainda
  não aceita, preserva multiplicação binary32, acumulação serial binary64 e
  scores internos bit a bit, mas canoniza quocientes finitos fora do codomínio
  para `-1` ou `+1`; ela exigiria `retrieval-v2`, descritor de vector store
  avançado, novo `IndexCompatibilityKey`, nova geração e nova baseline de
  avaliação. O ADR preserva corredor exato de 1 ULP e aritmética escalada em
  binary64 como alternativas condicionais e define provas executáveis futuras
  para os quatro achados. `DR-3` permanece `REPROVADO`; decisão, implementação,
  reteste, dataset, scorer, campanha, provider, rede, chamada paga, OpenAPI,
  schema, migration, MultiQuery, Human Gate e lifecycle continuam separados e
  `NOT_RUN` neste incremento.
- O corpus `4.10.31` registra a decisão explícita `ADR-0015: ACEITAR.` somente
  como autoridade arquitetural. A semântica
  `cosine-f32mul-f64acc-boundary-canonical-v1` canoniza todo quociente finito
  fora do codomínio para o endpoint exato e preserva os bits internos; a
  política sucessora é `retrieval-v2` e o descritor selecionado é
  `sqlite-exact-vector-store/2;schema=1;distance=cosine;algorithm=exact-scan;vector=float32;score=cosine-f32mul-f64acc-boundary-canonical-v1`.
  Novo `IndexCompatibilityKey`, nova geração e nova baseline de avaliação são
  obrigatórios antes de servir. As alternativas de 1 ULP e binary64 escalado
  não foram selecionadas. A aceitação não implementou código ou testes, não
  criou geração, dataset, scorer ou campanha e não executou provider, rede,
  chamada paga, OpenAPI, schema, migration, MultiQuery, Automatic Quality Gate,
  Human Gate ou lifecycle; `DR-3` permanece `REPROVADO` com os quatro achados
  abertos.
- O corpus `4.10.32` reconcilia factualmente o incremento implementado sob
  `AUTH-DR3-NUMERIC-SEMANTICS-IMPLEMENTATION-001`, na baseline limpa
  `main@9735ff5bc243d9a517b2cceb7ca8bfe16f24b438`, pelo commit
  `9addb166e82dd04581beee7b4276a74977fe04c5`. A implementação materializa
  `cosine-f32mul-f64acc-boundary-canonical-v1`, `retrieval-v2` e o descritor
  `sqlite-exact-vector-store/2;schema=1;distance=cosine;algorithm=exact-scan;vector=float32;score=cosine-f32mul-f64acc-boundary-canonical-v1`;
  avança a chave interna de compatibilidade e falha fechado para geração ou
  `IndexCompatibilityKey` `/1`; preserva multiplicação binary32, acumulação
  serial binary64, comparação exata e desempate por ordinal; e canoniza somente
  quocientes finitos fora do codomínio para `-1` ou `+1` exatos. A prova
  corretiva inclui limites bit a bit e reopen, top-k adversarial com nove chunks
  e duas permutações, filtros concorrentes antes de score/top-k e ordinal
  negativo nas fronteiras Application e SQLite task-owned. O turno de
  implementação registrou build Release sem avisos ou erros e 416 testes
  locais/offline aprovados — 202 unitários, 203 de integração e 11 de
  arquitetura —, sem falhas ou skips; essa evidência não é Automatic Quality
  Gate. `DR3-FIND-001` a `DR3-FIND-004` estão
  `CORRECTED_PENDING_GATE_RETEST`; `DR-3` permanece `REPROVADO` até reteste
  independente e disposição explícita. Nenhuma geração de produto, dataset,
  scorer, campanha, provider, credencial, rede, chamada paga, corpus real,
  OpenAPI, schema, migration, MultiQuery, Human Gate ou lifecycle foi criada,
  ativada, executada ou alterada.
- O corpus `4.10.33` reconcilia sob
  `AUTH-DR3-NUMERIC-SEMANTICS-AQG-RETEST-RECONCILE-001` o hand-off aprovado do
  reteste corretivo independente executado sob
  `AUTH-DR3-NUMERIC-SEMANTICS-AQG-RETEST-001` na baseline limpa
  `main@bf8a156e7c5eea801f29fb6e7742cac880783bc0`, corpus `4.10.32`. `DR-3`
  está `APROVADO`, `DR3-FIND-001` a `DR3-FIND-004` estão `RESOLVED` e nenhum
  novo achado P0, P1, P2 ou P3 foi identificado. Build Release, testes focais,
  três execuções independentes da matriz SQLite, os 416 testes da solução e a
  CI offline completa passaram; a CI registrou mais 45 testes do Dashboard,
  95,53% de cobertura de linhas, 68,47% de branches e auditoria de 280 arquivos.
  A evidência permanece local, offline, sintética e Windows x64. Nenhum arquivo
  rastreado foi alterado pelo reteste; geração de produto, dataset, scorer,
  campanha, provider, rede, chamada paga, corpus real, OpenAPI, schema,
  migration, MultiQuery, Human Gate e lifecycle não foram criados, ativados,
  executados ou alterados.
- O corpus `4.10.34` registra sob
  `AUTH-RB1-EVALUATION-DESIGN-FREEZE-001` a conclusão exclusivamente documental
  de `RB-1 — Evaluation design freeze`. A revisão imutável
  `retrieval-v2-evaluation-design-v1` congela o desenho não materializado e não
  pontuado em 28 artefatos normativos, com oito instâncias, 20 schemas Draft
  2020-12, 27 companions vinculados por SHA-256, self-digest determinístico,
  fórmulas, thresholds, quotas, matrizes, versionamento, retenção, gates, stop
  conditions e escopo negativo. Os sete contadores permanecem em zero. Nenhum
  dado/caso de produto, pergunta, qrel, vetor, geração, resultado, métrica
  observada, scorer, campanha, provider, rede, MultiQuery, Automatic Quality
  Gate, Human Gate ou lifecycle foi criado, executado ou alterado. `RB-2`
  permanece `NOT_RUN` e não autorizado.
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
- O corpus `4.9.4` corrige o enforcement do próximo trabalho recomendado:
  todo handoff informa exatamente uma ação concreta, priorizada, diretamente
  relacionada, com responsável e condição/autoridade. Solicitação concluída,
  projeto em espera ou falta de autoridade não justificam omissão quando dado,
  documento, decisão ou autorização ainda puder desbloquear a continuidade.
  A ausência canônica fica restrita à inexistência real de continuação
  acionável e não permite importar lifecycle ou backlog sem relação.
- O corpus `4.9.7` corrige uma recorrência posterior: revisão genérica de
  commits ou resultados concluídos não pode substituir o primeiro item ainda
  não concluído de uma ordem de dependência. Quando faltar autoridade para
  esse item, obtê-la do proprietário é a próxima ação e o handoff fornece o
  payload delimitado. Perguntas diretas sobre o próximo passo recebem a ação
  antes da recapitulação.
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
- Arquitetura aceita separa `SupportedQueryLanguage` fechado em `pt-BR` e
  `en-GB` de `DocumentContentLanguage` BCP 47; preserva a declaração exata
  `sourceDeclaredLanguage` e não converte `en` em `en-GB`. O runtime v1 ainda
  conserva o modelo fechado implementado.
- Interface com seleção explícita entre `pt-BR` e `en-GB`, independente do
  idioma da pergunta, da resposta e da evidência.
- Interface com seleção explícita entre `Light` e `Dark`, independente dos
  idiomas da interface, da pergunta, da resposta e da evidência.
- Ciclo Candidate/Active/Deactivated/Removed para bancos e documentos,
  versionamento manual e nova geração candidata.
- Conteúdo bruto imutável/reabrível, staging não consultável, manifesto final
  íntegro, digest generation-bound separado do digest completo de ativação e
  ativação/rollback por nova revisão do registro completo versionado.
- Arquitetura aceita torna `IDocumentContentStore` a autoridade permanente para
  fonte e PNGs content-addressed, exige render manifest PDF completo, direitos
  específicos e serving visual vinculado à citação. A fronteira executável do
  content store, os contratos/gates de direitos, o renderer/PNG, a finalização
  de manifest e os vínculos atômicos de ativação estão implementados em
  incrementos corretivos separados. `AnswerEvidenceRecordV1`, sua retenção e
  reachability também estão implementados localmente. O contrato HTTP/OpenAPI
  v2 e o serving visual same-origin foram implementados e aprovados pelo
  Automatic Quality Gate na fronteira local, offline, determinística e
  sintética registrada acima.
- Contrato HTTP/OpenAPI v1 versionado pertencente ao RAG-Challenge; adapters
  consumidores permanecem fora deste repositório.
- Contratos HTTP/OpenAPI v1 e v2 coexistem. V2 projeta o idioma documental BCP
  47 e a evidência visual same-origin; ambos os artefatos OpenAPI permanecem
  preservados byte a byte.
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

1. O A0 candidato-específico de `postgresql-18-reference-a4` foi disposto como
   `BLOCKED/EXCLUDED`: identidade, proveniência e idiomas conferiram; a
   concessão ampla foi mapeada sob ADR-0011. O mecanismo para transportar
   copyright, permission notice e dois disclaimers em cada PNG foi determinado
   pelo ADR-0012 e implementado localmente no commit
   `f682827d1a26b08fa8c450a1fadb3bd0e1fa1700`, com verificação focal. Essa
   implementação não altera retroativamente o A0: page rendering,
   derivative-image creation/retention e runtime display continuam `UNPROVEN`
   para o candidato porque o Automatic Quality Gate notice-bearing e um novo
   A0 candidato-específico não foram executados, não porque o mecanismo
   continue inexistente. A distribuição/publicação fora do runtime está
   `DENIED` pela fronteira interna deliberadamente excluída. O gate, sua
   eventual reconciliação e o novo A0 são etapas separadas e `NOT_RUN`.
   Cada documento posterior mantém o mesmo gate
   independente de direitos, proveniência e idioma.
2. Validar e ativar individualmente novos registros de fonte oficial; a
   aceitação arquitetural não autoriza URL, rede, download ou crawling.
3. `S07-A` A1-A5 e seu Automatic Quality Gate foram concluídos e aprovados na
   fronteira local, offline, determinística e sintética autorizada. A
   integração v2, restart, cold backup/restore confinado e limites também foram
   implementados; seu Automatic Quality Gate posterior foi `APROVADO` na
   fronteira sintética correspondente, e `AQG-S07-V2-IR-001` está `RESOLVIDO`.
   Homologação de produto e as fronteiras de provider, fonte, browser,
   segurança dinâmica, carga, recuperação operacional, acessibilidade, Linux,
   OCI e produção permanecem `NOT_RUN`; qualquer outro lote de `STATE-07`
   continua não executado e não autorizado.
4. A preparação sintética da campanha
   `s07-a-provider-gpt54m-candidate-001` está congelada no commit
   `422286863e7a3c213e96db18144769bd0458a75b`; seu Automatic Quality Gate foi
   `APROVADO` sem achados somente na fronteira local, offline, determinística e
   com handlers falsos. Não houve chamada real ou resultado pontuado. Ainda é
   necessário verificar tier, entitlement, spend limit e controles da conta
   OpenAI, além da recuperação/geração bilíngue, antes de usar ou anunciar os
   providers.
5. Homologar desempenho e capacidade do `SqliteExactVectorStore`; a fixture
   funcional de 10.000 × 1.536 passou, mas não é benchmark, SLA ou teto de
   produto.
6. Testar process-crash boundaries abrangentes no `STATE-07`; restart e cold
   backup/restore do store v2 task-owned já possuem evidência local sintética e
   confinada, sem representar armazenamento ou recuperação operacional.
7. Verificar capacidade, entitlement, IAM, restore, custo e cobrança reais da
   tenancy OCI; as fontes públicas ainda divergem sobre a franquia gratuita.
8. A revisão sucessora sintética de `rag-eval-catalogue-v1` materializa a
   matriz `pt-BR`/`en-GB`, prompt, schema, configuração, limites, agenda e
   orçamento da candidata sem pontuação. Antes de qualquer execução real, a
   revisão pontuada ainda deve congelar o corpus de produto autorizado, a
   rubrica e os thresholds, acrescentar estratos por tag documental exata e
   nunca contar `en` como `en-GB`.
9. `S03-CORR-01` concluiu o primeiro item da ordem de dependência.
   `S04-CORR-04-A` concluiu descritores verificados do content store e
   `S04-CORR-04-B` concluiu contratos/gates de direitos;
   `S04-CORR-04-C` concluiu renderização determinística e finalização
   verificada da candidata; `S04-CORR-04-D` concluiu persistência e ativação
   atômica dos vínculos de fonte, direitos, geração e manifest; e
   `S04-CORR-04-E` concluiu `AnswerEvidenceRecordV1`, retenção fixa `P30D` e
   reachability na fronteira local/offline. O contrato v2 foi congelado, sua
   implementação e o serving visual same-origin foram concluídos e o Automatic
   Quality Gate correspondente foi `APROVADO` na fronteira local, offline,
   determinística e sintética. OpenAPI v1 e v2 permanecem protegidas. As
   integração v2, restart, cold backup/restore confinado e limites foram
   concluídos no commit `e5dae7ee5a786417fba2c6ef0555686816b0b330`; a
   correção focal está no commit
   `f6c648c40cf8d0280cfceca5509a381bddb9fc8f`, e o Automatic Quality Gate
   próprio foi `APROVADO` sem novo achado, com `AQG-S07-V2-IR-001`
   `RESOLVIDO`. As fronteiras de browser/tecnologia assistiva, dados, renderer,
   provider, fonte e rede reais, carga, crash injection abrangente,
   recuperação operacional, Linux, OCI e produção continuam `NOT_RUN`.

## Próxima autoridade

O ADR-0014 foi aceito explicitamente mediante `ADR-0014: ACEITAR.` somente
como autoridade arquitetural e reconciliado documentalmente sob
`AUTH-STATE07-RETRIEVAL-DETERMINISM-ADR-RECONCILE-001`. `DR-0`, `DR-1` e
`DR-2` estão concluídos. O `DR-3 — Determinism Automatic Quality Gate` foi
inicialmente `REPROVADO`; a preparação e a aceitação do ADR-0015, a
implementação corretiva no commit
`9addb166e82dd04581beee7b4276a74977fe04c5` e sua reconciliação factual foram
concluídas. O reteste corretivo independente posterior, autorizado por
`AUTH-DR3-NUMERIC-SEMANTICS-AQG-RETEST-001`, foi `APROVADO`; `DR3-FIND-001` a
`DR3-FIND-004` estão `RESOLVED`, sem novo achado P0, P1, P2 ou P3. `RB-1 —
Evaluation design freeze` também está concluído sob
`AUTH-RB1-EVALUATION-DESIGN-FREEZE-001`, exclusivamente como revisão de desenho
imutável, não materializada e não pontuada. A próxima condição diretamente
relacionada na ordem do ADR-0014 é obter autoridade humana separada para
`RB-2 — Dataset materialisation readiness`, condicionada a corpus de produto
autorizado com direitos verificados, geração ativa validada, pooling não
pontuado e adjudicação completos, qrels, matrizes requeridas completas,
denominador exato congelado e tier declarado. `RB-2` permanece `NOT_RUN` e não
autorizado. Scorer executado, inputs congelados de campanha, campanha,
provider, rede, chamada paga, OpenAPI, schema de produto, migration,
MultiQuery, Human Gate e lifecycle permanecem fora dessa condição, e
`retrieval-multi-query-v1-candidate` continua estacionado.

O ADR-0013 foi aceito explicitamente mediante `ADR-0013: ACEITAR.` somente
como autoridade arquitetural. Ele seleciona `gpt-5.4-mini-2026-03-17` para o
MVP e mantém `gpt-5.6-sol` apenas como candidato futuro inativo. A reconciliação
semântica documental foi concluída sob
`AUTH-STATE07-LLM-CANDIDATE-ADR-RECONCILE-001`, e o incremento local, offline e
determinístico de compatibilidade do adaptador foi implementado sob
`AUTH-STATE07-LLM-ADAPTER-COMPAT-001` no commit
`b6d6f9102ecf0ea93309f8080acebad02cf16584`. O Automatic Quality Gate
específico foi `APROVADO` sem achados sob
`AUTH-STATE07-LLM-ADAPTER-COMPAT-AQG-001`, somente na fronteira local, offline,
determinística e com handlers falsos. Isso não constitui avaliação real nem
homologação. A campanha `s07-a-provider-gpt54m-candidate-001` possui preparação
sintética congelada sob `AUTH-S07-A-PROVIDER-PREP-001` no commit
`422286863e7a3c213e96db18144769bd0458a75b`, com agenda e orçamento somente
planejados e zero execução real. Seu Automatic Quality Gate foi `APROVADO`, sem
achados, sob `AUTH-S07-A-PROVIDER-PREP-AQG-001`, exclusivamente na fronteira
local, offline, determinística e com handlers falsos. Qualquer avaliação com
conta, credencial, provider, chamada paga, corpus real ou OCI exige autoridade
humana separada; Human Gate e lifecycle permanecem sem alteração.

`S07-A` A1-A5 e seu Automatic Quality Gate estão concluídos e aprovados na
fronteira sintética local autorizada, com `S07-A-FIND-001` e o histórico
`S07-A-FIND-004` ainda abertos nas disposições acima. Isso não satisfaz a
homologação de produto nem prepara Human Gate: qualquer continuação deve nomear
e autorizar separadamente a fronteira ainda `NOT_RUN`, sem inferir avanço de
lifecycle.

O A0 candidato-específico de `postgresql-18-reference-a4` foi repetido sob
`AUTH-S07-A-PRODUCT-A0-002` e manteve a disposição `BLOCKED/EXCLUDED`. A
concessão oficial já registrada foi mapeada operação por operação: page
rendering, derivative-image creation, derivative-image retention e runtime
derivative display permanecem `UNPROVEN` porque o Automatic Quality Gate
notice-bearing e um novo A0 candidato-específico ainda não foram executados,
não porque o mecanismo de avisos continue inexistente. O mecanismo foi
implementado no commit `f682827d1a26b08fa8c450a1fadb3bd0e1fa1700`, sem
reclassificação retroativa do A0. A intended source/derivative distribution
boundary está `DENIED` fora do runtime-display. Dataset, import, render,
indexação e ativação continuam não autorizados por esse resultado.

O ADR-0012 foi aceito explicitamente mediante `ADR-0012: ACEITAR.` somente
como autoridade arquitetural, e sua reconciliação semântica nos seis
proprietários documentais foi concluída sob
`AUTH-S07-A-NOTICE-BEARING-PROFILE-RECONCILE-001`. Ela registra o mecanismo
autocontido. A revisão protegida do contrato v2 foi congelada sob
`AUTH-S07-A-NOTICE-BEARING-V2-CONTRACT-001`, preservando v1 e a rota v2. Isso
não reclassifica o candidato. O schema e as migrations foram implementados no
commit `98036f3c8c496544f4532d1fe48c981f836a1871`, preservando registros legados
e falhando fechado. O obligation set, renderer composto, manifest, storage,
reachability, serving v2 fail-closed e Dashboard notice-bearing foram
implementados no commit `f682827d1a26b08fa8c450a1fadb3bd0e1fa1700`, com
evidência focal que não constitui gate. O Automatic Quality Gate local,
offline, determinístico e sintético desse comportamento, a eventual
reconciliação documental de seu resultado e um novo A0 candidato-específico
exigem autoridades próprias, permanecem separados e estão `NOT_RUN`.

O ADR-0011 foi aceito, sua semântica foi reconciliada e a política interna de
serving v2 foi corrigida no commit
`b9c3e5f3a72c2dd7762c256198452ae2c217b2d2`. A verificação agora ocorre antes
de `200` ou `304`, sem alteração do contrato público. O A0 posterior confirmou
que o candidato PostgreSQL continua `BLOCKED/EXCLUDED`: quatro operações
visuais estão `UNPROVEN` e a distribuição/publicação externa está `DENIED`.

O segundo refinamento arquitetural da ordem registrada em Lifecycle está
implementado até integração v2, restart, cold backup/restore confinado e
limites na fronteira local, offline, sintética e sequencial. `AQG-S04-002` a
`AQG-S04-004`, `AQG-S07-V2-001`, `AQG-S07-V2-002` e
`AQG-S07-V2-IR-001` estão `RESOLVIDOS`; os Automatic Quality Gates corretivo
de `S04-CORR-04-E`, do incremento de contrato/serving v2 e da integração e
recuperação v2 estão `APROVADOS`. Não existe novo Human Gate canonicamente
aplicável a esses incrementos: Human Gate pertence a um único `STATE-ID`; o
Human Gate histórico de `STATE-04` permanece inalterado e `STATE-07` não
recebe decisão por implicação. Dataset e homologação de produto continuam
posteriores, `NOT_RUN` e não autorizados; nenhum avanço posterior está
autorizado.

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

Depois disso, os commits
`8ab79d59f4dfe9d35e73a25f05612fd244e31393` e
`f92e26c7008a2d124bd10edb2e3f03c0c9ad2bf6` alteraram o agregador de
cobertura e a política fail-closed de CI. O novo reinício integral autorizado
sobre `main@f92e26c7008a2d124bd10edb2e3f03c0c9ad2bf6`, corpus `4.9.3` e working
tree limpa inventariou os oito commits posteriores a `bfc3aefc` e parou na
auditoria estática com `AQG-S06-005` (P2):
`eng/test-assert-coverage.ps1` e `eng/test-ci-policy.ps1` não são chamados por
nenhum entry point automático, inclusive `eng/ci.ps1` e o workflow. O gate
está `REPROVADO`, o achado permanece `ABERTO` e nenhuma etapa executável
posterior foi iniciada. `STATE-06` continua ativo; Human Gate, `STATE-07` e
ações externas permanecem não autorizados.

A correção focal posterior, autorizada sobre
`main@000dca0210e220a9f247159178c6d97d9fc4fd55`, integrou os dois testes ao
início de `eng/ci.ps1` por um helper fail-closed compartilhado e ampliou o
teste de política para provar sucesso, propagação de falha, ausência de script,
invocação única e consumo canônico pelo workflow. Parsing, 11 casos de
coverage, 14 controles de política/integração, `git diff --check` e auditoria
de 203 arquivos passaram localmente e offline. O workflow não mudou. O gate
integral não foi reiniciado; `AQG-S06-005` está
`CORRECTED_PENDING_GATE_RETEST`, e Human Gate continua prematuro.

O reinício integral subsequente, autorizado sobre
`main@616bef4e2ae8c0b26c10781cd728dc6089136a60`, corpus `4.9.3` e working
tree limpa, repetiu desde o início supply chain, restore locked isolado,
controles fail-closed, gate técnico, cobertura, persistência/migration,
cancelamento/resiliência, duas reproduções ARM64, verificador estático,
comandos do README, segurança e higiene. Todos os controles passaram: 206
testes .NET, 38 testes npm, 93,11% de linhas, 66,89% de branches e dois ZIPs
ARM64 idênticos com 17 ELF64 AArch64. O Automatic Quality Gate está
`APROVADO`, sem novo P0, P1, P2 ou P3, e `AQG-S06-005` está `RESOLVIDO`.
Human Gate e `STATE-07` não foram executados; `STATE-06` continua ativo.

O proprietário recebeu o resumo completo do Human Gate sobre
`main@2f70705dcbe293b22ccd039d0764b2b9ca4b2e8a`, corpus `4.9.3` e working
tree limpa e confirmou exatamente
`Confirmo a decisão acima exclusivamente para STATE-06`. O Human Gate foi
`APROVADO COM RESSALVAS` para a fronteira local, offline, sintética e
estática documentada. As limitações de Linux ARM64, OCI, providers, corpus,
fontes e armazenamento reais, cobertura percentual JavaScript, observação de
pacotes e migration real permanecem explícitas. `STATE-06 INTEGRATION` está
encerrado; `STATE-07` não foi autorizado nem iniciado, e nenhuma ação externa
foi executada.

Em 2026-08-06, sobre
`main@3240a4b13acd82a1cf5815ac64f6997b2a7f89bf`, corpus `4.9.3` e working
tree limpa, o proprietário autorizou exclusivamente a entrada documental em
`STATE-07 TESTING_HOMOLOGATION` e a reconciliação dos blocos de status
público necessários. `STATE-07` está ativo sem lote autorizado ou executado.
Qualquer dataset, avaliação RAG, teste, carga, segurança dinâmica, browser,
provider, fonte real, rede, OCI, GitHub, publicação, deploy, `STATE-08` ou
ação externa exige autoridade humana explícita e separada.

Em 2026-08-07, sobre
`main@183c8cd9fe303096a355ab731e72dc81748eb626`, corpus `4.9.3` e working
tree limpa, o proprietário confirmou a proposta documental de `S07-A`
exclusivamente como baseline de planejamento. A confirmação não autorizou
`AUTH-S07-A-DATASET-001`, `AUTH-S07-A-RUN-001`, materialização de dataset,
avaliação, testes, carga, segurança dinâmica, browser, providers, fontes
reais, rede ou ação externa. Naquele registro histórico, `S07-A` ainda não
havia sido executado; a execução posterior de A1-A5 e a aprovação de A5 estão
registradas no estado factual vigente acima.

Em 2026-08-07, sobre
`main@66c47d94d423abf4f0c1509ba04b8064d3efd8ca`, corpus `4.9.3` e working
tree limpa, o proprietário determinou a correção permanente do handoff para
sempre informar exatamente uma próxima ação concreta. O corpus `4.9.4`
registra essa regra em AGENTS, Governance e Templates, preservando o limite
temático e a ausência de autoridade para `S07-A` ou qualquer execução.

Em 2026-08-07, sobre
`main@3d15ad4f2726f715c8dcf880491927ad0ff37b2f`, corpus `4.9.4` e working
tree limpa, o proprietário autorizou exclusivamente a reconciliação semântica
conjunta dos ADRs 0008/0009. O corpus `4.9.5` registra a arquitetura aceita nos
18 arquivos confirmados, preserva OpenAPI v1 byte a byte e mantém implementação,
dataset, conteúdo, renderização, indexação, ativação, execução e ação externa
fora da autoridade.
