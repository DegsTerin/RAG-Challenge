# Controle e Changelog do Sistema de Instruções

## Versão atual

- Versão: `4.10.29`
- Data: 2026-08-11
- Status: `STATE-07` ativo; ADR-0011 `accepted`, reconciliado e com a correção
  interna da política de serving implementada; o A0 candidato-específico mantém
  o primeiro documento `BLOCKED/EXCLUDED`, com quatro operações visuais
  `UNPROVEN` e distribuição/publicação externa `DENIED`; ADR-0012 `accepted`,
  reconciliado, com revisão do contrato v2 congelada, schema/migrations e
  comportamento notice-bearing implementados; a verificação focal desse
  comportamento não substitui seu Automatic Quality Gate, ainda `NOT_RUN`, e
  as operações do A0 permanecem `UNPROVEN` por falta desse gate e de um novo
  A0, não por inexistência do mecanismo;
  ADR-0013 `accepted` e
  reconciliado semanticamente, com compatibilidade do adaptador implementada e
  Automatic Quality Gate específico aprovado somente na fronteira local,
  offline e com handlers falsos; a campanha candidata de provider possui uma
  revisão sucessora sintética congelada, com Automatic Quality Gate aprovado
  somente na fronteira local, offline, determinística e com handlers falsos;
  o preflight operacional inicial terminou `BLOQUEADO`, sem campanha real; o
  fluxo experimental Coordinator/Docker/C3 foi revogado e não constitui
  autoridade ou pendência; o caminho temporário da Admin key de provisionamento
  está fechado, com
  revogação, estado histórico somente Inactive e remoção verificada do target
  local registradas de forma sanitizada; ADR-0014 `accepted`, com
  `DR-2 — Determinism implementation` concluído no commit focal
  `fabb24cad16201070e3b95fffb22467cd55963ab` e MultiQuery estacionado;
  `DR-3 — Determinism Automatic Quality Gate` executado sob autoridade
  separada e `REPROVADO`, com `DR3-FIND-001` P1 e `DR3-FIND-002` a
  `DR3-FIND-004` P2 abertos;
  homologação de produto, Human Gate e mudança de lifecycle não executados
- Escopo: 13 arquivos ativos em `prompts/`

A versão do corpus é independente da versão futura do software.

## Política SemVer

- `MAJOR`: mudança incompatível de autoridade, precedência, estados ou
  estrutura.
- `MINOR`: nova capacidade, módulo, playbook ou gate sem quebra do fluxo.
- `PATCH`: clareza, correção ou referência sem mudança de autoridade.

Toda alteração atualiza este arquivo e, quando necessário,
[`../Start-Here.md`](../Start-Here.md).

## 4.10.29 — 2026-08-11

- Reconcilia, sob a autoridade documental explícita concedida pelo
  proprietário, sobre branch `main`, commit
  `272a868c2f2a90eba21ee422ba5a2c34aa2337d5`, corpus `4.10.28`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas, o resultado factual de
  `DR-3 — Determinism Automatic Quality Gate`.
- Registra `DR-3` como `REPROVADO`, com um P1 e três P2:
  `DR3-FIND-001` P1 observa que vetores admissíveis idênticos
  `[1f, 1f, 1f]` produzem score `1.0000000000000002`, convertido em
  `InvalidIndexData`/`CH_INDEX_UNAVAILABLE`; `DR3-FIND-002` P2 registra que o
  teste de determinismo não prova adversarialmente o sort completo antes de
  `Take(k)`; `DR3-FIND-003` P2 registra ausência de prova dos filtros antes de
  score/top-k com hits elegíveis e inelegíveis concorrentes; e
  `DR3-FIND-004` P2 registra ausência de regressão executável para
  `ChunkOrdinal < 0`.
- A revisão observou que a implementação aplica os filtros e depois
  `OrderByDescending(Score)`, `ThenBy(ChunkOrdinal)` e `Take(k)`. Os três P2
  são lacunas de prova, não defeitos comportamentais observados. O P1 é um
  defeito comportamental de entrada admissível. Nenhum achado foi corrigido e
  nenhuma semântica numérica foi definida ou alterada dentro do gate.
- Evidência executada no gate: build Release com zero avisos e zero erros;
  74/74 testes unitários focais, 35/35 de integração focais e 11/11 de
  arquitetura; 3/3 execuções independentes do caso de empate/reopen; CI
  offline completa com 201 testes unitários, 197 de integração, 11 de
  arquitetura e 45 do Dashboard; cobertura de 95,53% de linhas e 68,34% de
  branches; e auditoria de 279 arquivos não ignorados. As versões registradas
  foram .NET SDK `10.0.302`, Node `24.19.0`, npm `11.17.0` e PowerShell
  `7.6.4`. Os checks aprovados não superam os quatro achados.
- O runtime preflight e o postflight do gate executável foram aplicáveis:
  encontraram zero processo candidato do RAG-Challenge, encerraram zero e
  deixaram zero remanescente. O gate começou e terminou na mesma baseline
  limpa, não alterou arquivo rastreado, dataset, contrato ou configuração e
  materializou somente outputs ignorados dos checks. Esta reconciliação
  puramente documental não reexecutou os checks do gate; seu runtime preflight
  foi `NÃO APLICÁVEL`.
- Limites preservados: nenhum código ou teste foi corrigido; nenhuma semântica
  numérica, dataset, `retrieval-evaluation-scorer-v1`, campanha, provider,
  credencial, rede, chamada paga, corpus real, OpenAPI, schema, migration,
  dependência, lockfile, MultiQuery, novo Automatic Quality Gate, Human Gate,
  lifecycle, push, publicação, deploy ou release foi executado ou alterado por
  esta reconciliação. Os gates posteriores não foram iniciados.
- Artefatos protegidos: OpenAPI v1 permaneceu no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; OpenAPI v2 permaneceu no
  SHA-256 `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` e blob
  `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`.
- Validação documental: `git diff --check` terminou com exit code `0`;
  `eng/check-repository.ps1` aprovou 279 arquivos não ignorados; somente os
  quatro documentos autorizados mudaram; UTF-8/LF, newline final, espaços
  finais, links, formato e o prefixo append-only do histórico passaram.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.28` para
  `4.10.29`. O histórico preservou byte a byte seu prefixo anterior no SHA-256
  `6972e5b4c245c0ed7e6c83b5bad51a1a1aa15d846859380c27f9014c7a041d9f`.
- Próxima condição: obter autoridade humana separada para preparar a decisão
  versionada de semântica numérica e o plano corretivo dos quatro achados.
  Qualquer decisão, implementação corretiva, repetição de DR-3, dataset,
  campanha, provider, gate posterior ou reconsideração de MultiQuery permanece
  independente.

## 4.10.28 — 2026-08-11

- Reconcilia, sob a autoridade documental explícita concedida pelo
  proprietário, sobre branch `main`, commit
  `fabb24cad16201070e3b95fffb22467cd55963ab`, corpus `4.10.27`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas, o
  `DR-2 — Determinism implementation`, autorizado separadamente sobre
  `main@ade89d737975f65c38e88b35758f8c6091e57406`.
- Registra a porta Application retrieval-only tipada, o request e o resultado
  vinculados à ativação, geração, `IndexCompatibilityKey`, descritor esperado,
  limites e digests; a validação de query, vetores, normas, scores finitos em
  `[-1, 1]`, ordinal global, identidades, contagem e ordem total; os outcomes
  fail-closed; e o uso do mesmo executor pelo query path antes do language
  model.
- Preserva `retrieval-v1` para entradas válidas: ordem
  `Score DESC, global ChunkOrdinal ASC`, top-k `8`, mínimo `0.25` inclusivo,
  máximo de seis evidências e orçamento de 16.000 escalares. Stored zero-vector
  mantém score `0`; nenhuma tolerância, clamp, nova chave de desempate ou
  alteração da aritmética de cosseno foi introduzida.
- Evidência focal registrada do turno de implementação: build Debug com zero
  avisos e zero erros; 74 de 74 testes unitários, 8 de 8 testes de integração
  locais/SQLite e 11 de 11 testes de arquitetura aprovados; auditoria do
  repositório aprovada para 279 arquivos não ignorados. A reconciliação
  documental não reexecutou testes e não constitui `DR-3` ou Automatic Quality
  Gate; o runtime preflight documental foi `NÃO APLICÁVEL`.
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
- Validação documental: `git diff --check` terminou com exit code `0`;
  `eng/check-repository.ps1` aprovou 279 arquivos não ignorados; somente os
  quatro documentos autorizados mudaram; UTF-8/LF, newline final, espaços
  finais, links, formato e o prefixo append-only do histórico passaram.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.27` para
  `4.10.28`. O histórico preservou byte a byte seu prefixo anterior no SHA-256
  `7955af11982aefe5d5cc850e6182e3f73c03562fce50ee84a08894eea58260fd`.
- Próxima condição: `DR-3 — Determinism Automatic Quality Gate` exige
  autoridade humana separada; dataset, campanha, provider, cada gate posterior
  e qualquer reconsideração de MultiQuery permanecem independentes.

## 4.10.27 — 2026-08-11

- Reconcilia sob
  `AUTH-STATE07-RETRIEVAL-DETERMINISM-ADR-RECONCILE-001`, sobre branch `main`,
  commit `52e1ac7d9bc61be196549a8ee61399fde477b8fb`, corpus `4.10.26`, working tree
  inicialmente limpa e OpenAPI v1/v2 protegidas, a decisão explícita do
  proprietário `ADR-0014: ACEITAR.`.
- Torna ADR-0014 `accepted` exclusivamente como autoridade arquitetural,
  registra a ordenação existente `Score DESC, global ChunkOrdinal ASC` e
  preserva `retrieval-v1` para entradas válidas.
- Define como requisitos futuros a porta Application retrieval-only tipada,
  scores finitos, ordinal global único, falhas fail-closed, scorer, métricas,
  replay determinístico e os freezes separados de design, dataset e
  `campaign-input`. Nenhum desses requisitos foi implementado ou executado.
- Mantém `retrieval-multi-query-v1-candidate` não canônico e estacionado. Uma
  comparação futura exige autoridade própria, o mesmo denominador exato de
  casos e resultados executados separadamente.
- Nenhum código, teste executável, corpus real, dataset, campanha, provider,
  credencial, rede, chamada paga, OpenAPI, schema, migration, Automatic Quality
  Gate, Human Gate, lifecycle, push ou publicação foi autorizado ou executado.
- Artefatos protegidos: OpenAPI v1 permaneceu no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; OpenAPI v2 permaneceu no
  SHA-256 `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` e blob
  `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`.
- Validação documental: `git diff --check` terminou com exit code `0`;
  `eng/check-repository.ps1` aprovou 276 arquivos não ignorados; somente os
  cinco documentos autorizados mudaram; UTF-8/LF, newline final, espaços
  finais, links, formato e o prefixo append-only do histórico passaram.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.26` para
  `4.10.27`. O histórico preservou byte a byte seu prefixo anterior no SHA-256
  `33f360c67c53e0b089f6a6e17f3a3278f06fde7e6678e09f79382b7efbdbf87c`.
- Próxima condição: obter autoridade humana separada para
  `DR-2 — Determinism implementation`; cada dataset, campanha, provider, gate
  e reconsideração de MultiQuery permanece sob autoridade independente.

## 4.10.26 — 2026-08-11

- Reconcilia sob
  `AUTH-STATE07-PREFLIGHT-BOUNDARY-REAUDIT-RECONCILE-001`, sobre branch `main`,
  commit `1629df7cac27f48b21f64b1a0f1e440cc1cf7f20`, corpus `4.10.25`, working
  tree inicialmente limpa e OpenAPI v1/v2 protegidas, as reauditorias das
  tarefas “Preflight operacional GPT-5.4-mini” e “Next Homologation Boundary”.
- Registra que o preflight operacional inicial terminou `BLOQUEADO`, sem
  campanha real, provider ou `/v1/responses`, e que o cleanup da Admin key e da
  credencial local está encerrado.
- Revoga factualmente o fluxo experimental Coordinator/Docker/C3: ele não
  constitui autoridade vigente nem pendência canônica.
- Corrige a causalidade da disposição do A0: o mecanismo notice-bearing foi
  implementado no commit
  `f682827d1a26b08fa8c450a1fadb3bd0e1fa1700`, mas não reclassifica
  retroativamente o A0. As quatro operações visuais permanecem `UNPROVEN`
  porque o Automatic Quality Gate notice-bearing e um novo A0 não foram
  executados, não porque o mecanismo continue inexistente.
- Mantém separados e `NOT_RUN` o Automatic Quality Gate notice-bearing, a
  eventual reconciliação documental de seu resultado e o novo A0
  candidato-específico.
- O runtime preflight foi `NÃO APLICÁVEL`. Nenhum código, runtime, teste
  comportamental, Automatic Quality Gate, A0, provider, rede, browser,
  credencial, billing, Docker, cleanup de host, corpus real, Human Gate ou
  lifecycle foi autorizado ou executado.
- Artefatos protegidos: OpenAPI v1 permaneceu no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; OpenAPI v2 permaneceu no
  SHA-256 `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` e blob
  `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`.
- Validação documental: `git diff --check` terminou com exit code `0` e
  `eng/check-repository.ps1` aprovou 275 arquivos não ignorados.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.25` para
  `4.10.26`. O histórico preservou byte a byte seu prefixo anterior no SHA-256
  `d08bf87f6fd28690598623cfe3531ec8d96053cb1229c38e392b9d18ed66fff4`.
- Próxima condição: o Automatic Quality Gate notice-bearing exige autoridade
  humana separada; somente seu resultado pode ser reconciliado em lote próprio,
  e somente depois cabe um novo A0 candidato-específico sob outra autoridade.

## 4.10.25 — 2026-08-11

- Reconcilia sob
  `AUTH-S07-A-PROVIDER-ADMIN-KEY-CLEANUP-RECONCILE-001`, sobre branch `main`,
  commit `b2654088d11ab94c23cdf19e2aa57d89f0b3ae49`, corpus `4.10.24`, working
  tree inicialmente limpa e OpenAPI v1/v2 protegidas, o fechamento concluído
  sob `AUTH-S07-A-PROVIDER-ADMIN-KEY-CLEANUP-002`.
- Registra de forma sanitizada que a Admin key com label exato
  `s07-a-provider-gpt54m-candidate-001-admin-provisioning` foi revogada, está
  ausente do inventário Active e aparece historicamente somente como Inactive;
  `Last used` permaneceu `Never` e o gasto permaneceu `USD 0.00`.
- Registra que o target
  `RAG-Challenge/OpenAI/AdminKey/s07-a-provider-gpt54m-candidate-001` foi
  removido do Windows Credential Manager e que sua ausência foi verificada no
  cleanup autorizado. Nenhum secret, fragmento, fingerprint ou representação
  mascarada foi incluído.
- A fonte factual é o registro de fechamento sanitizado fornecido pelo
  proprietário. Esta reconciliação puramente documental não reacessou OpenAI,
  Credential Manager, provider, billing, projeto ou qualquer credencial; o
  runtime preflight foi `NÃO APLICÁVEL`.
- Não houve chamada de provider ou `/v1/responses`, custo novo, alteração de
  billing, limites, allowlist ou projeto, Human Gate ou mudança de lifecycle.
- Artefatos protegidos: OpenAPI v1 permaneceu no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`; OpenAPI v2 permaneceu no
  SHA-256 `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` e
  blob `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`.
- Validação documental: `git diff --check` terminou com exit code `0`,
  `eng/check-repository.ps1` aprovou 275 arquivos não ignorados, somente os
  quatro documentos autorizados mudaram e o prefixo append-only do histórico
  conferiu no SHA-256 anterior.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.24` para
  `4.10.25`.
- Próxima condição diretamente relacionada: nenhuma; o cleanup administrativo
  e sua reconciliação factual estão encerrados sem gate ou lifecycle.

## 4.10.24 — 2026-08-11

- Reconcilia sob
  `AUTH-S07-A-NOTICE-BEARING-PROFILE-IMPL-RECONCILE-001`, após a solicitação
  explícita do proprietário para auditar e finalizar as pendências das duas
  tarefas, sobre `main@7f363a3e2036e4a76626eff482052bf7343c3cd7`, corpus
  `4.10.23`, working tree inicialmente limpa e OpenAPI v1/v2 protegidas, a
  implementação notice-bearing do commit
  `f682827d1a26b08fa8c450a1fadb3bd0e1fa1700`.
- Corrige as afirmações factuais atuais em 18 superfícies nomeadas: README e
  índices documentais, ADR-0008/0011/0012, contratos canônicos, dicionário de
  dados, threat model, proposta v2, registro de elegibilidade, relatório de
  homologação e os proprietários ativos em `prompts/`. Relatos cronológicos de
  incrementos anteriores permanecem inalterados.
- Registra `DerivativeObligationSetV1`, composição determinística do PNG com a
  região da página preservada pixel a pixel, vínculo ao manifest, persistência
  e reachability, readback e serving v2 same-origin fail-closed e apresentação
  acessível do texto integral no Dashboard. Registros legados não foram
  reclassificados ou preenchidos por inferência.
- Evidência focal observada no incremento: build Release sem avisos ou erros;
  47 de 47 testes unitários, 40 de 40 testes de integração/contrato v1/v2, 11
  de 11 testes de arquitetura e 45 de 45 testes do Dashboard; build e lint do
  Dashboard aprovados; cleanup sem processo ou listener residual do projeto.
  Essa evidência não é Automatic Quality Gate.
- Artefatos protegidos: OpenAPI v1 permaneceu no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e
  OpenAPI v2 permaneceu no SHA-256
  `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733`.
- Limites preservados: nenhum dado/corpus real, obrigação ou derivado do
  PostgreSQL, novo A0, browser, tecnologia assistiva, provider, fonte/rede,
  OCI, deploy, Automatic Quality Gate, Human Gate ou mudança de lifecycle foi
  executado por esta reconciliação puramente documental. O runtime preflight
  foi `NÃO APLICÁVEL`.
- Versionamento: corpus elevado por `PATCH` factual de `4.10.23` para
  `4.10.24`. O histórico preservou byte a byte seu prefixo anterior no SHA-256
  `c3c3463ad029286cb5c66b53c8954da44502113d7ac8f5406316495908a8d2f2`.
- Próxima condição: o Automatic Quality Gate local, offline, determinístico e
  sintético do comportamento notice-bearing exige autoridade humana separada;
  somente após eventual aprovação e reconciliação cabe um novo A0
  candidato-específico, também separado.

## 4.10.23 — 2026-08-10

- Reconcilia sob `AUTH-STATE07-S07-A-PROVIDER-PREP-AQG-RECONCILE-001`, sobre
  `main@5d74c9c9190b0b3465b11dc6864e3dd519cc88f9`, corpus `4.10.22`, working
  tree inicialmente limpa e OpenAPI v1/v2 protegidas, o Automatic Quality Gate
  autorizado sob `AUTH-S07-A-PROVIDER-PREP-AQG-001`.
- Registra o resultado `APROVADO`, sem achado P0, P1, P2 ou P3. A auditoria
  confirmou o escopo isolado dos commits, a preservação da predecessora, os
  cinco manifests e seus digests, a matriz de 60 casos, 12 casos de prompt
  injection, 20 casos de insuficiência, configuração congelada, máximo de 109
  chamadas e orçamento operacional/absoluto de `USD 16`/`USD 20`.
- Registra 2 de 2 testes do harness e 20 de 20 testes combinados aprovados com
  handlers falsos. A CI offline completa aprovou 154 testes unitários, 193 de
  integração, 11 de arquitetura e 45 do Dashboard, com 95,63% de cobertura de
  linhas, 67,66% de branches e build sem avisos ou erros; formatação,
  `git diff --check` e auditoria de 275 arquivos também passaram.
- Limita a aprovação à preparação local, offline, determinística e com handlers
  falsos. Conta, credencial, provider, chamada paga, corpus/fonte real,
  avaliação real, qualidade bilíngue, groundedness, citações, insuficiência de
  evidência real, prompt injection, latência, custo observado, OCI, deploy,
  Human Gate e lifecycle permanecem `NOT_RUN`.
- Esta reconciliação altera somente proprietários documentais factuais. O
  corpus avança por `PATCH` de `4.10.22` para `4.10.23`; código, testes,
  configuração, Domain, Application, ADRs e OpenAPI permanecem inalterados.

## 4.10.22 — 2026-08-10

- Reconcilia sob `AUTH-STATE07-S07-A-PROVIDER-PREP-RECONCILE-001`, sobre
  `main@422286863e7a3c213e96db18144769bd0458a75b`, corpus `4.10.21`, working
  tree inicialmente limpa e OpenAPI v1/v2 protegidas, a preparação autorizada
  sob `AUTH-S07-A-PROVIDER-PREP-001`.
- Registra a campanha `s07-a-provider-gpt54m-candidate-001` e sua revisão
  sucessora imutável
  `rag-eval-catalogue-v1-provider-gpt54m-candidate-001`, preservando
  `rag-eval-catalogue-v1-candidate-001`. A revisão contém dois documentos
  sintéticos e 60 casos: 40 respondíveis, dez em cada uma das quatro direções
  obrigatórias `pt-BR`/`en-GB`; 20 de insuficiência; e 12 de prompt injection,
  cobrindo seis classes por idioma de pergunta.
- Registra o snapshot `gpt-5.4-mini-2026-03-17`, prompt, schema, configuração,
  limites, 4 chamadas de smoke, 5 de warm-up, 100 medidas, máximo de 109,
  retry zero, concorrência um, orçamento operacional de `USD 16` e teto
  absoluto de `USD 20` como valores congelados e não executados.
- Registra as verificações observadas na preparação: 2 de 2 testes do novo
  harness e 20 de 20 testes combinados do harness e do contrato OpenAI
  passaram somente com handlers falsos; formatação não exigiu mudança;
  `git diff --check` passou; e a auditoria aprovou 275 arquivos não ignorados.
- Não registra disponibilidade de conta ou provider, qualidade bilíngue,
  groundedness, citações, insuficiência de evidência real, resistência a
  prompt injection, latência ou custo observado. Nenhuma credencial, chamada
  externa ou paga, corpus/fonte real, OCI, deploy, Automatic Quality Gate,
  Human Gate ou mudança de lifecycle foi executada.
- Esta reconciliação altera somente proprietários documentais factuais. O
  corpus avança por `PATCH` de `4.10.21` para `4.10.22`; código, testes,
  configuração, Domain, Application, ADRs e OpenAPI permanecem inalterados.

## 4.10.21 — 2026-08-10

- Reconcilia sob `AUTH-STATE07-LLM-ADAPTER-COMPAT-AQG-RECONCILE-001`, sobre
  `main@6e6fdabb91e2fb4c5186c464ce08f5da390d727a`, corpus `4.10.20`, working
  tree inicialmente limpa e OpenAPI v1/v2 protegidas, o Automatic Quality Gate
  autorizado sob `AUTH-STATE07-LLM-ADAPTER-COMPAT-AQG-001`.
- Registra o resultado `APROVADO`, sem achado P0, P1, P2 ou P3. A auditoria
  estática confirmou os sete requisitos de compatibilidade do ADR-0013, os
  dois paths executáveis esperados, ausência de diff executável posterior e
  ausência dos identificadores legado ou futuro inativo em `src/` e `tests/`.
- Registra 266 arquivos aprovados na auditoria do repositório, formatação sem
  mudança, 18 de 18 testes focais e 11 de 11 testes de arquitetura aprovados.
  A CI offline completa aprovou 154 testes unitários, 191 de integração, 11 de
  arquitetura e 45 do Dashboard, com 95,63% de cobertura de linhas, 67,65% de
  branches e build sem avisos ou erros.
- Limita a aprovação à compatibilidade local, offline, determinística e com
  handlers falsos. Avaliação real, conta, credencial, provider, qualidade
  bilíngue, groundedness, citações, insuficiência de evidência, prompt
  injection, latência, custo, corpus real, OCI, deploy, Human Gate e lifecycle
  permanecem `NOT_RUN`.
- Esta reconciliação altera somente proprietários documentais factuais. O
  corpus avança por `PATCH` de `4.10.20` para `4.10.21`; código, testes,
  configuração, Domain, Application, ADRs e OpenAPI permanecem inalterados.

## 4.10.20 — 2026-08-10

- Reconcilia sob `AUTH-STATE07-LLM-ADAPTER-COMPAT-RECONCILE-001`, sobre
  `main@b6d6f9102ecf0ea93309f8080acebad02cf16584`, corpus `4.10.19`, working
  tree inicialmente limpa e OpenAPI v1/v2 protegidas, o incremento local,
  offline e determinístico autorizado sob
  `AUTH-STATE07-LLM-ADAPTER-COMPAT-001`.
- Registra configuração tipada e imutável de reasoning effort/context, o
  snapshot exato `gpt-5.4-mini-2026-03-17`, emissão limitada a parâmetros
  suportados, `store=false`, ausência de `tools` e validação estrita do modelo,
  status, mensagem, role e único `output_text` estruturado observado.
- Registra como evidência focal final 18 de 18 testes de contrato do adaptador
  com handler falso, 11 de 11 testes de arquitetura, verificação de formatação
  sem mudança e auditoria de 266 arquivos não ignorados aprovada. Somente o
  adaptador de Infrastructure e seu teste de contrato compõem o commit de
  implementação.
- Não registra disponibilidade em conta, comportamento real do provider,
  qualidade bilíngue, groundedness, citações, insuficiência de evidência,
  resistência a prompt injection ou latência real. Nenhuma chamada externa ou
  paga, credencial, corpus real, OCI, deploy, Automatic Quality Gate, Human
  Gate ou mudança de lifecycle foi executada.
- Esta reconciliação altera apenas os proprietários documentais factuais. O
  versionamento do corpus é um `PATCH` de `4.10.19` para `4.10.20`; código,
  testes, configuração, Domain, Application, ADRs e OpenAPI permanecem
  inalterados durante esta etapa.

## 4.10.19 — 2026-08-10

- Reconcilia sob `AUTH-STATE07-LLM-CANDIDATE-ADR-RECONCILE-001`, sobre
  `main@a08aa83c7319b97ead6c91a92ae8cbb4da5c28cc`, corpus `4.10.18`, working
  tree inicialmente limpa e OpenAPI v1/v2 protegidas, a decisão aceita no
  ADR-0013.
- Aplica `gpt-5.4-mini-2026-03-17` como único candidato de LLM do MVP ao
  ADR-0005, ao relatório arquitetural de `STATE-02` e ao índice de arquitetura.
  A seleção anterior `gpt-4.1-mini-2025-04-14` permanece identificada somente
  como fato histórico.
- Mantém `gpt-5.6-sol` como candidato futuro inativo, sem fallback ou troca
  dinâmica, e preserva seu risco de identificador móvel e todos os gates
  bilíngues, de groundedness, citação, insuficiência de evidência, prompt
  injection e latência.
- Preserva todas as demais decisões do ADR-0005. Os proprietários de RAG e
  segurança permanecem semanticamente compatíveis porque já expressam portas,
  provider e controles sem fixar o modelo substituído.
- Nenhum código, teste, OpenAPI, configuração, conta, credencial, provider,
  chamada paga, corpus real, avaliação, OCI, deploy, Automatic Quality Gate,
  Human Gate ou lifecycle foi alterado, acessado ou executado. A mudança do
  corpus é `PATCH` documental.

## 4.10.18 — 2026-08-10

- Registra a decisão humana explícita `ADR-0013: ACEITAR.` sobre
  `main@f03162bad0fc166a597739b22e55fbc46ec59535`, corpus `4.10.17`, working
  tree inicialmente limpa e OpenAPI v1/v2 protegidas.
- Torna o ADR-0013 `accepted` exclusivamente como autoridade arquitetural,
  seleciona `gpt-5.4-mini-2026-03-17` como único candidato de LLM do MVP e
  substitui somente a seleção anterior de LLM do ADR-0005.
- Mantém `gpt-5.6-sol` como candidato futuro inativo, registra o risco de seu
  identificador móvel e preserva os requisitos bilíngues, de groundedness,
  citações, insuficiência de evidência, prompt injection e latência.
- Preserva todas as demais decisões do ADR-0005, inclusive controles de dados,
  secrets, egress, gastos, persistência e OCI. Custo permanece fora do ranking,
  sem remoção dos controles de gasto aceitos.
- Nenhuma reconciliação semântica, alteração de adaptador, configuração,
  OpenAPI, acesso a conta, credencial, provider, chamada paga, corpus real,
  avaliação, OCI, deploy, Automatic Quality Gate, Human Gate ou lifecycle foi
  autorizada ou executada. A mudança do corpus é `PATCH` factual.

## 4.10.17 — 2026-08-10

- Reconcilia sob
  `AUTH-S07-A-NOTICE-BEARING-SCHEMA-MIGRATION-RECONCILE-001`, sobre
  `main@98036f3c8c496544f4532d1fe48c981f836a1871`, corpus `4.10.16` e working
  tree inicialmente limpa, os fatos do incremento de schema e migrations
  implementado sob `AUTH-S07-A-NOTICE-BEARING-SCHEMA-MIGRATION-001`.
- Registra a persistência imutável de `DerivativeObligationSetV1` e blocos
  ordenados, a coexistência de `pdf-page-png-notice-v1` com o perfil legado, o
  vínculo de obligation-set ID/digest e dimensões source/notice ao render
  manifest, e constraints, foreign keys e sealing triggers fail-closed.
- Registra as migrations
  `20260810033026_AddNoticeBearingObligationSchema` e
  `20260810034537_SealNoticeBearingObligationBindings`, sem backfill inferido ou
  mutação de registros, manifests, hashes ou ativações legados.
- Registra 7/7 testes focais aprovados, ausência de pending model changes,
  `foreign_key_check`, upgrade e rollback/reapply em stores SQLite temporários
  task-owned, além do cleanup concluído.
- Preserva OpenAPI v1/v2, `postgresql-18-reference-a4` `BLOCKED/EXCLUDED`, as
  dez decisões e o fail-closed. Nenhum novo A0, renderer, PNG, serving,
  Dashboard, dataset, Automatic Quality Gate, Human Gate ou lifecycle foi
  executado.
- A mudança do corpus é `PATCH` factual e não concede autoridade para o próximo
  incremento.

## 4.10.16 — 2026-08-10

- Congela sob `AUTH-S07-A-NOTICE-BEARING-V2-CONTRACT-001`, sobre
  `main@6982b0643468aee0a97c3bea6b5bbe9018f0804c`, corpus `4.10.15` e working
  tree inicialmente limpa, a revisão pública v2 exigida pelo ADR-0012.
- OpenAPI v1 permanece byte a byte no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e blob
  `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`. A nova OpenAPI v2 possui SHA-256
  `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733` e blob
  `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`.
- Acrescenta somente `obligationSetId` a `PageImageEvidenceV1` e
  `DerivativeObligationPresentationV1` a `CitationV2`, preservando a rota e
  todos os campos anteriores. O contrato mantém a projeção legada com ambos os
  novos valores `null`; o caso notice-bearing exige um único ID, apresentação
  completa e idioma coincidentes e rejeita mistura, ausência ou divergência.
- Atualiza os tipos C# e TypeScript, o decoder estrito e os testes contratuais
  diretamente responsáveis. Cinco testes do decoder, seis testes .NET e o
  typecheck do Dashboard passaram focalmente.
- Preserva `postgresql-18-reference-a4` `BLOCKED/EXCLUDED`, as dez decisões e o
  fail-closed. Nenhum novo A0, schema, migration, renderer, armazenamento,
  runtime de produto, dataset, rede, fonte, provider, Automatic Quality Gate,
  Human Gate ou lifecycle foi executado.
- A mudança do corpus é `PATCH` factual e não concede autoridade para o próximo
  incremento.

## 4.10.15 — 2026-08-09

- Reconcilia sob
  `AUTH-S07-A-NOTICE-BEARING-PROFILE-RECONCILE-001`, sobre
  `main@5c2cea66e45f13479486a345552e5cc3cd47fefe`, corpus `4.10.14` e working
  tree inicialmente limpa, a decisão aceita no ADR-0012.
- Aplica `pdf-page-png-notice-v1` e `DerivativeObligationSetV1` a ADR-0008, ao
  contrato documental v2, data dictionary, Security-And-Access, threat model e
  registro de elegibilidade, preservando a região da página pixel a pixel e o
  fail-closed.
- Registra vínculo imutável a rights mapping, manifest e ativação; content
  storage e reachability conjuntos; backup/cold restore verificável; revalidação
  same-origin antes de `200`/`304`; ETag do PNG composto; e texto completo,
  escapado e acessível junto da figura.
- Identifica como revisões futuras obrigatórias e separadamente autorizadas o
  contrato público v2, schema e migration. OpenAPI v1/v2, contrato executável,
  schema e implementação permanecem inalterados.
- Preserva as dez decisões independentes e não executa novo A0.
  `postgresql-18-reference-a4` continua `BLOCKED/EXCLUDED`: quatro operações
  visuais permanecem `UNPROVEN` e distribuição/publicação externa permanece
  `DENIED` pela fronteira interna registrada.
- Nenhum código, teste, renderer, dataset, runtime, rede, fonte, provider,
  Automatic Quality Gate, Human Gate, lifecycle ou ação externa foi executado.
  A mudança do corpus é `PATCH` documental.

## 4.10.14 — 2026-08-09

- Registra a decisão humana explícita `ADR-0012: ACEITAR.` sobre
  `main@243a448823a114190f68a25f9d521e1849eddacf`, corpus `4.10.13`, working
  tree inicialmente limpa e OpenAPI v1/v2 protegidas.
- Torna o ADR-0012 `accepted` exclusivamente como autoridade arquitetural para
  `pdf-page-png-notice-v1`, `DerivativeObligationSetV1`, vínculo ao render
  manifest, armazenamento/recovery, serving same-origin e apresentação
  acessível das obrigações.
- Preserva a região da página pixel a pixel, as dez decisões independentes, o
  fail-closed e OpenAPI v1 byte a byte. Registra que schema, migration e revisão
  pública do contrato v2 permanecem mudanças futuras separadamente
  autorizadas; OpenAPI v2 não foi alterada nesta aceitação.
- A aceitação não reclassifica `postgresql-18-reference-a4`: sua disposição
  permanece `BLOCKED/EXCLUDED`, quatro operações visuais permanecem `UNPROVEN`
  e a distribuição/publicação externa permanece `DENIED` pela fronteira
  interna já registrada.
- Nenhuma reconciliação semântica, revisão de contrato, schema, migration,
  implementação, código, teste, renderer, dataset, runtime, Automatic Quality
  Gate, Human Gate, lifecycle ou ação externa foi autorizada ou executada. A
  mudança do corpus é `PATCH` factual.

## 4.10.13 — 2026-08-09

- Prepara sob `AUTH-S07-A-NOTICE-BEARING-PROFILE-ADR-PREP-001`, sobre
  `main@1b64ca88a0efebd7ab450f5bdc22004a72f3dc53`, corpus `4.10.12` e working
  tree inicialmente limpa, o ADR-0012 como proposta arquitetural.
- Define um único perfil determinístico `pdf-page-png-notice-v1`: a região da
  página preserva pixel a pixel o raster de origem, enquanto um painel separado
  e pertencente ao mesmo PNG contém attribution, copyright/permission notices,
  disclaimers, trademark treatment e change marking completos.
- Propõe `DerivativeObligationSetV1`, vínculo imutável ao render manifest,
  persistência e reachability próprias, backup/cold restore verificável,
  serving same-origin com revalidação e apresentação textual acessível no
  Dashboard.
- Expõe como mudanças futuras obrigatórias um novo schema de manifest e
  obligation set, a migration dos constraints de perfil e uma revisão pública
  do contrato v2. OpenAPI v1 permanece protegida; OpenAPI v2 não foi alterada
  nesta preparação.
- Mantém o ADR-0012 `proposed`, o candidato
  `postgresql-18-reference-a4` `BLOCKED/EXCLUDED` e as disposições A0
  inalteradas. Não aceita o ADR nem autoriza reconciliação, contrato, schema,
  migration, código, teste, renderer, dataset, runtime, Automatic Quality Gate,
  Human Gate ou lifecycle. A mudança do corpus é `PATCH` documental.

## 4.10.12 — 2026-08-09

- Registra sob `AUTH-S07-A-PRODUCT-A0-002`, sobre
  `main@f21cdea2052d28de1e2ffb86b1629c1c10bc6b6a`, corpus `4.10.11` e working
  tree inicialmente limpa, o A0 candidato-específico local, offline,
  sequencial e sem comportamento de produto de
  `postgresql-18-reference-a4`.
- Confirma o PDF ignorado como arquivo regular sem reparse point, com
  `15.771.040` bytes e SHA-256
  `cea7b845568095eb56dee1b51bfa145c6c6637bc4377c986019971577efefae4`.
- Mapeia separadamente a concessão oficial já registrada para page rendering,
  derivative-image creation, derivative-image retention, runtime display e a
  intended distribution boundary. A condição registrada exige copyright,
  permission notice e dois disclaimers em todas as cópias.
- Mantém page rendering, derivative-image creation, derivative-image retention
  e `RuntimeDerivativeImageDisplay` como `UNPROVEN`, pois o contrato e o PNG
  atuais não fornecem mecanismo determinado para os avisos completos. Registra
  `SourceAndDerivativeByteDistributionOrPublication` como `DENIED` fora do
  runtime-display pela fronteira interna deliberadamente excluída, não por
  proibição do publisher.
- Preserva `BLOCKED/EXCLUDED`, não registra
  `READY_FOR_PRODUCT_ACTIVATION` e não altera o manifest congelado, dataset,
  código, teste, OpenAPI, contrato, schema, migration, ADR, dependência ou
  lockfile. Nenhum parser, renderer, derivado, indexação, ativação, query,
  runtime, rede, fonte, provider, Automatic Quality Gate, Human Gate ou
  lifecycle foi executado. A mudança do corpus é `PATCH` factual.

## 4.10.11 — 2026-08-09

- Registra sob
  `AUTH-S07-A-RIGHTS-POLICY-CORR-IMPL-RECONCILE-001`, sobre
  `main@b9c3e5f3a72c2dd7762c256198452ae2c217b2d2`, corpus `4.10.10` e working
  tree inicialmente limpa, a implementação focal da correção interna do
  serving conforme o ADR-0011.
- O gate interno `PdfVisualEvidenceServing` avalia as dez decisões:
  `RuntimeDerivativeImageDisplay` deve estar `Permitted`;
  `SourceAndDerivativeByteDistributionOrPublication` `Unproven` bloqueia; e
  `Denied` é compatível somente com `RuntimeDerivativeImageDisplay`
  `Permitted` na fronteira same-origin aceita.
- Registra 19 testes da política, 23 regressões dos gates existentes, três
  testes do leitor real e seis testes contratuais v1/v2 aprovados. Nenhum
  runtime ou listener permaneceu, e OpenAPI v1/v2 conservaram hashes e blobs
  protegidos.
- Não reclassifica `postgresql-18-reference-a4`, não executa novo A0 e mantém
  sua disposição `BLOCKED/EXCLUDED` com os cinco direitos `UNPROVEN`.
- Nenhum código, teste, OpenAPI, contrato público, schema, migration, ADR,
  dependência, lockfile ou dataset foi alterado por esta reconciliação. Nenhum
  parser, renderer, runtime, rede, fonte, provider, Automatic Quality Gate,
  Human Gate ou lifecycle foi executado. A mudança do corpus é `PATCH` factual.

## 4.10.10 — 2026-08-09

- Aplica sob `AUTH-S07-A-RIGHTS-POLICY-CORR-RECONCILE-001`, sobre
  `main@6fc81b973ca217693a286479df3ff6db0f4577e9`, corpus `4.10.9` e working
  tree inicialmente limpa, a reconciliação documental do ADR-0011 aceito.
- ADR-0004, ADR-0008, o registro S07-A de elegibilidade e o contrato documental
  v2 agora aplicam o mapeamento explícito, auditável e condicionado de
  concessões primárias amplas, preservando as dez decisões independentes e o
  fail-closed.
- Distingue a entrega same-origin de um PNG ativo e citation-bound como
  `RuntimeDerivativeImageDisplay` da distribuição/publicação externa sob
  `SourceAndDerivativeByteDistributionOrPublication`, sem tratar same-origin
  como concessão de direitos ou ausência de transmissão de bytes.
- Registra que attribution, copyright/permission notices, disclaimers,
  trademark e change marking permanecem associados à origem, à linhagem de
  cada derivado e ao contexto de entrega determinado pelo mapeamento.
- Preserva a incompatibilidade executável: o contrato v2 exige reavaliar a
  intended distribution boundary, mas a política interna `PdfVisualEvidence`
  ainda não avalia
  `SourceAndDerivativeByteDistributionOrPublication`. A correção permanece
  posterior e separadamente autorizada.
- Não reclassifica `postgresql-18-reference-a4`, não executa novo A0 e mantém
  sua disposição `BLOCKED/EXCLUDED` com os cinco direitos `UNPROVEN`.
- Nenhum código, teste, OpenAPI, contrato público, schema, migration,
  dependência, lockfile, dataset, parser, renderer, runtime, provider, fonte,
  rede, gate, Human Gate, lifecycle ou ação externa foi alterado ou executado.
  A mudança do corpus é `PATCH` documental.

## 4.10.9 — 2026-08-09

- Registra a decisão humana explícita `ADR-0011: ACEITAR.` sobre
  `main@09f6760cb1a41d907da42b8c01cb34a7425030b9`, corpus `4.10.8` e working
  tree inicialmente limpa.
- Torna o ADR-0011 `accepted` exclusivamente como autoridade arquitetural para
  o mapeamento explícito, auditável e condicionado de evidência primária, a
  fronteira entre runtime same-origin display e distribuição/publicação e as
  obrigações que acompanham derivados.
- Preserva as dez decisões independentes e o fail-closed e mantém registrada a
  incompatibilidade estática entre o contrato v2 e a política interna.
- A aceitação não reclassifica `postgresql-18-reference-a4`: sua disposição
  permanece `BLOCKED/EXCLUDED`, e os cinco direitos visuais e de distribuição
  permanecem `UNPROVEN`.
- Nenhuma reconciliação semântica dos documentos normativos proprietários,
  correção de código, teste, OpenAPI, contrato, schema, migration, dataset,
  runtime, gate, Human Gate, lifecycle ou ação externa foi autorizada ou
  executada. A mudança do corpus é `PATCH` factual.

## 4.10.8 — 2026-08-09

- Registra sob `AUTH-S07-A-RIGHTS-POLICY-CORR-PREP-001`, sobre
  `main@17c41a78cbe853473860403d476797064b77c78a`, corpus `4.10.7` e working
  tree inicialmente limpa, a preparação documental do ADR-0011 com status
  `proposed`.
- A proposta preserva as dez decisões independentes e o fail-closed, substitui
  a correspondência literal por um mapeamento explícito, auditável e
  condicionado e separa runtime same-origin display de distribuição ou
  publicação externa de bytes.
- Define como atribuição, copyright/permission notices, disclaimers, trademark
  e change marking permanecem associados a cada derivado e ao seu contexto de
  entrega.
- Registra a incompatibilidade estática entre o contrato v2, que exige
  reavaliar a intended distribution boundary, e
  `DocumentRightsEligibilityPolicy.PdfVisualEvidence`, que não exige
  `SourceAndDerivativeByteDistributionOrPublication`.
- O ADR não foi aceito; o PostgreSQL permanece `BLOCKED/EXCLUDED` com cinco
  direitos `UNPROVEN`. Nenhum código, teste, OpenAPI, contrato, schema,
  migration, dataset, runtime, gate, Human Gate, lifecycle ou ação externa foi
  alterado ou executado. A mudança do corpus é `PATCH` documental, sem mudança
  de autoridade vigente.

## 4.10.7 — 2026-08-09

- Registra a instrução explícita do proprietário para que toda explicação seja
  apresentada primeiro de forma prática, concisa e compreensível por uma
  pessoa sem conhecimento técnico especializado.
- Exige que termos técnicos necessários tenham significado e consequência
  explicados em `pt-BR`, com exemplo concreto quando ele melhorar a
  compreensão.
- Preserva precisão factual: simplificação não pode ocultar incerteza, risco,
  limite de autoridade ou fato não verificado.
- Centraliza a regra em `Language-Policy.md`, sem duplicá-la em AGENTS,
  Governance ou Templates. A mudança é `PATCH` e não altera lifecycle, gate,
  código, contrato, OpenAPI ou comportamento do produto.

## 4.10.6 — 2026-08-09

- Registra sob `AUTH-S07-A-PRODUCT-A0-001`, sobre
  `main@78d49e135d7b517c7ff89a9e5edcbcc7839e4043`, corpus `4.10.5` e working
  tree inicialmente limpa, o A0 local, offline, sequencial e sem comportamento
  de produto do candidato `postgresql-18-reference-a4`.
- Registra confinamento, ausência de reparse point, exclusão do Git, tamanho,
  assinatura PDF, EOF e SHA-256 exato, além da consistência da proveniência,
  dos idiomas `en`/`en`, do publisher e da atribuição já documentados.
- Preserva as permissões textuais explicitamente registradas e não infere page
  rendering, derivative-image creation/retention, runtime display ou a
  intended source/derivative distribution boundary a partir da permissão geral
  de uso, cópia, modificação e distribuição.
- Dispõe o candidato como `BLOCKED/EXCLUDED`, não
  `READY_FOR_PRODUCT_ACTIVATION`, porque as cinco operações permanecem
  `UNPROVEN`. Dataset, manifests congelados, derivados, indexação, ativação e
  comportamento de produto permanecem inalterados e `NOT_RUN`.
- A mudança é `PATCH` factual. Preserva OpenAPI v1/v2 byte a byte e não executa
  parser, renderer, runtime, teste, Automatic Quality Gate, Human Gate,
  lifecycle, provider, fonte online, rede ou ação externa.

## 4.10.5 — 2026-08-09

- Corrige sob
  `AUTH-STATE07-V2-INTEGRATION-RECOVERY-LIFECYCLE-CORR-002`, sobre
  `main@7ad6bae369eb1efbf6429902a2fd1f4441b60a32`, corpus `4.10.4` e working
  tree limpa, as duas claims correntes desatualizadas da ordem de dependência
  em Lifecycle.
- Registra somente que o Automatic Quality Gate da integração e recuperação v2
  foi `APROVADO` sob
  `AUTH-STATE07-V2-INTEGRATION-RECOVERY-AQG-RETEST-001`, sem novo achado, e que
  `AQG-S07-V2-IR-001` está `RESOLVIDO`.
- Preserva a ordem normativa, os estados, os critérios e OpenAPI v1/v2 byte a
  byte. Dataset e homologação de produto permanecem posteriores, `NOT_RUN` e
  não autorizados.
- A mudança é `PATCH` factual e não executa runtime, testes, Automatic Quality
  Gate, Human Gate ou lifecycle; não altera código, teste, harness, contrato,
  schema, migration, ADR, dependência, lockfile ou dataset.

## 4.10.4 — 2026-08-09

- Reconcilia sob
  `AUTH-STATE07-V2-INTEGRATION-RECOVERY-AQG-RECONCILE-001` o Automatic Quality
  Gate reiniciado e aprovado sob
  `AUTH-STATE07-V2-INTEGRATION-RECOVERY-AQG-RETEST-001`, sobre
  `main@f6c648c40cf8d0280cfceca5509a381bddb9fc8f`, corpus `4.10.3` e working
  tree inicialmente limpa.
- Registra auditoria de 255 arquivos, preflight com zero processo e listener
  pertencente ao RAG-Challenge e 53 de 53 testes focais aprovados.
- Registra dois builds determinísticos no ZIP SHA-256
  `ab5e450efe1b606f2b8e50e2f5885a3c1ae19bf4ad90dd96d096e00506daec28` e
  harness publicado `Passed`, com três readiness `Ready`, restart, cold
  restore, PNG, `304`, teto de 64 MiB e token bucket 10/11 aprovados.
- Registra CI offline com 147 testes unitários, 174 de integração, 11 de
  arquitetura e 42 do Dashboard, cobertura de 94,81% de linhas e 67,24% de
  branches, e build sem avisos ou erros; cleanup completo e nenhum runtime ou
  listener remanescente.
- Dispõe o gate como `APROVADO`, sem novo achado, e
  `AQG-S07-V2-IR-001` como `RESOLVIDO`. Preserva OpenAPI v1/v2 byte a byte e
  não converte a aprovação sintética em homologação de produto, Human Gate ou
  mudança de lifecycle.
- A mudança é `PATCH` factual. Esta reconciliação não executa runtime, testes,
  Automatic Quality Gate, Human Gate ou lifecycle e não altera código, teste,
  harness, contrato, schema, migration, ADR, dependência, lockfile ou dataset.

## 4.10.3 — 2026-08-09

- Corrige somente as anotações factuais desatualizadas da ordem de dependência
  em Lifecycle sob
  `AUTH-STATE07-V2-INTEGRATION-RECOVERY-LIFECYCLE-CORR-001`, sobre
  `main@de40a93e0023f854fec840a93934c199c294f9c6`, corpus `4.10.2` e working
  tree limpa.
- Registra que `S04-CORR-04-E` possui Automatic Quality Gate corretivo
  aprovado e que contrato/serving v2 estão implementados e possuem Automatic
  Quality Gate aprovado.
- Registra que integração, restart, cold backup/restore confinado e limites
  foram implementados e verificados focalmente no commit
  `e5dae7ee5a786417fba2c6ef0555686816b0b330`, mas seu Automatic Quality Gate
  permanece `NOT_RUN`; dataset e homologação continuam posteriores e não
  autorizados.
- Preserva a ordem normativa, os estados, os critérios e OpenAPI v1/v2 byte a
  byte. A mudança é `PATCH` factual e não executa runtime, testes, Automatic
  Quality Gate, Human Gate ou lifecycle.

## 4.10.2 — 2026-08-09

- Registra a implementação local, offline, determinística, sintética e
  sequencial da integração e recuperação v2 sob
  `AUTH-STATE07-V2-INTEGRATION-RECOVERY-IMPL-001`, concluída no commit
  `e5dae7ee5a786417fba2c6ef0555686816b0b330`.
- Registra composição fail-closed fora do perfil `Integration` e, dentro dele,
  query, readiness e reader visual verificado sobre PDF/PNG project-owned em
  memória, sem corpus ou dado de produto e sem fonte, renderer ou provider
  real.
- Registra 52 de 52 testes focais aprovados e harness publicado `Passed`, com
  serving PNG `200`, revalidação `304`, geração preservada após restart e cold
  restore, cópias confinadas com fingerprints idênticos, teto de 64 MiB e token
  bucket que aceita dez acessos e rejeita o décimo primeiro por `429`.
- Registra duas construções offline byte a byte determinísticas no ZIP SHA-256
  `e27c64571b63538e4cba21f552df500c24a4bab3a6365e6229e2d9dd033f2f7d`,
  cleanup completo e ausência final de host ou listener da tarefa.
- Preserva OpenAPI v1 e v2 byte a byte, sem mudança de contrato, schema,
  migration, ADR, dependência ou lockfile. Browser/tecnologia assistiva, dado,
  renderer, provider, fonte e rede reais, carga, crash injection abrangente,
  recuperação operacional, Linux, OCI e produção permanecem `NOT_RUN`.
- A mudança é `PATCH`, exclusivamente factual. Não executa Automatic Quality
  Gate deste incremento, Human Gate ou lifecycle e não converte evidência
  sintética em homologação de produto.

## 4.10.1 — 2026-08-08

- Registra a implementação local, offline e sequencial de `S04-CORR-04-E`
  conforme ADR-0010: modelo e serialização canônica de
  `AnswerEvidenceRecordV1`, criação somente para `Answered` depois da validação
  integral e persistência/readback antes da resposta v1 inalterada.
- Registra a transação Control atômica de header, citações, páginas, operação e
  auditoria sanitizada, com replay `AlreadyApplied`, conflito divergente sem
  mutação e falha fechada pela taxonomia v1 existente.
- Registra a migration Control
  `20260808033247_AddAnswerEvidenceRecords`, que cria somente três tabelas
  vazias, sem backfill, inferência histórica ou alteração da base Vector.
- Integra a retenção fixa `P30D` sem refresh ao `cleanup-plan-v1`, reserva,
  revalidação, finalização e reachability de fonte/PNGs, inclusive contra a race
  entre plano antigo e novo registro de resposta.
- Registra build Release sem aviso, 146 testes unitários, 153 de integração e
  10 de arquitetura aprovados e os dois contextos EF sem mudança de modelo
  pendente. Essas verificações diretas não constituem Automatic Quality Gate.
- Preserva `docs/api/openapi-v1.json` byte a byte no SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34` e
  mantém v2, serving, dados reais, rede, ações externas, gates e lifecycle fora
  do escopo executado.
- Reconcilia somente os registros factuais correntes necessários. A mudança é
  `PATCH`; a autoridade arquitetural histórica de ADR-0010 permanece
  distinguida da implementação posterior.

## 4.10.0 — 2026-08-07

- Registra a decisão arquitetural aceita em ADR-0010: `S04-CORR-04-E`
  corresponde ao contrato interno persistente `AnswerEvidenceRecordV1`, à
  retenção fixa `P30D` sem refresh e à sua participação em reachability.
- Define identidade e digest canônicos, criação somente para `Answered` antes
  da resposta, vínculos exatos de citação/fonte/manifest/página, transação
  Control atômica, replay/conflito e failure semantics existentes.
- Estabelece minimização: nenhuma pergunta/hash da pergunta, resposta, texto ou
  URL de citação, prompt/payload de provider, score/vetor, identidade/IP do
  usuário, secret, path ou bytes no registro ou logging padrão.
- Mantém `cleanup-plan-v1` e reserva/revalidação/finalização como única
  autoridade de exclusão física; expiração remove apenas a raiz temporária e
  não autoriza exclusão por si.
- Corrige somente declarações factuais correntes sobre a implementação já
  concluída de idiomas, content store, direitos, renderer/PNG, manifests e
  vínculos de ativação, sem reescrever ADR-0008/0009 nem evidência histórica.
- Preserva `docs/api/openapi-v1.json` byte a byte e mantém implementação E,
  migration, testes executáveis, v2, serving, dados reais, gates, lifecycle,
  rede e ações externas fora do escopo executado.
- Atualiza os doze artefatos documentais autorizados. A mudança é `MINOR` por
  registrar nova autoridade normativa sem quebrar o fluxo vigente.

## 4.9.11 — 2026-08-07

- Registra a implementação local, offline e sequencial de `S04-CORR-04-D` sob
  `AUTH-S04-CORR-04-D-001` no commit
  `d18224e46f559229a58e82b097abbf16ea9f359a`.
- Registra o binding imutável por revisão entre documento/fonte, snapshot
  schema-v1 das dez decisões de direitos, geração e render manifest PDF, com
  gates CSV/PDF, reabertura verificada e readback fail-closed.
- Registra CAS/replay/rollback/ObservationRebind, retenção, head, auditoria e
  journal completion atômicos, preservando os campos e a semântica de
  `sourceBindingSetDigest` e `activationBindingSetDigest`.
- Registra a migration Control
  `20260808004846_AddDocumentRightsAndActivationEvidenceBindings`, sem backfill
  ou inferência sobre activation history e sem alteração da base Vector.
- Registra testes focais, upgrade/rollback/reapply, `foreign_key_check`, dois
  pending model checks e CI offline integral com 282 testes .NET, 38 do
  Dashboard, 94,34% de linhas e 67,25% de branches.
- Preserva OpenAPI v1 byte a byte e mantém dados/direitos reais, v2, serving,
  `AnswerEvidenceRecord`, gates, lifecycle e ações externas fora da autoridade
  executada. `S04-CORR-04-E` não foi iniciado.
- Atualiza Current State, histórico append-only, contratos, Data Dictionary,
  módulo RAG, segurança e relatório de `STATE-04`. A mudança é `PATCH`,
  exclusivamente factual.

## 4.9.10 — 2026-08-07

- Registra a implementação local e sequencial de `S04-CORR-04-C` sob
  `AUTH-S04-CORR-04-C-001` no commit
  `981e61c3308ee3407769d10ab1fa554007f12799`.
- Registra o renderer `pdfium-pdftoimage-v1`, `PDFtoImage` `5.3.0`, PDFium
  `153.0.7988` e SkiaSharp `4.151.1`, com pins transitivos exatos, quatro
  lockfiles afetados e gate de supply chain isolado.
- Registra o worker interno de um documento, contenção antes do envio dos bytes,
  framing privado limitado, perfil determinístico `pdf-page-png-v1`, validação
  estrutural de PNG e publicação verificada de todas as páginas.
- Registra a finalização fail-closed e idempotente de
  `DocumentRenderManifest` nas tabelas existentes, sem schema, migration,
  ativação, serving, cleanup ou alteração do contrato público v1.
- Registra 7 testes unitários focais, 10 testes de integração focais, publish
  framework-dependent `linux-arm64` com assets ELF64 AArch64 e CI offline
  integral com 268 testes .NET e 38 do Dashboard.
- Mantém direitos/dados reais, ativação, v2, gates, lifecycle e ações externas
  fora da autoridade executada. A mudança é `PATCH`, exclusivamente factual.

## 4.9.9 — 2026-08-07

- Registra a implementação local, offline e sequencial de `S04-CORR-04-B` sob
  `AUTH-S04-CORR-04-B-001` no commit
  `a886a944ecd1ce485eee9c072385e96210e90520`.
- Registra `DocumentRightsEligibilityRecordV1`, as dez decisões independentes
  de ADR-0008, os estados `Permitted`, `Denied` e `Unproven` e a referência
  estável de evidência exigida por decisão.
- Registra os gates fixos `TextualEvidence` e `PdfVisualEvidence`: somente
  `Permitted` satisfaz um requisito; distribuição/publicação permanece
  independente e não é inferida de elegibilidade textual ou visual.
- Registra 14 testes sintéticos focais e o CI offline integral com 251 testes
  .NET, 38 do Dashboard, 93,72% de linhas e 67,20% de branches, além da
  preservação byte a byte da OpenAPI v1.
- Mantém persistência de direitos, schema/migration, renderer, PNG, render
  manifest persistido, ativação, v2, fonte/dado/direito real, gates, lifecycle
  e ações externas fora da autoridade executada.
- Atualiza Current State, histórico append-only, Data Dictionary, relatório de
  `STATE-04` e este changelog. A mudança é `PATCH`, factual, sem alterar
  autoridade, lifecycle ou a ordem dos incrementos restantes.

## 4.9.8 — 2026-08-07

- Registra a implementação local, offline e sequencial de `S04-CORR-04-A` sob
  `AUTH-S04-CORR-04-A-001` no commit
  `26f2e154b736687693b31ab02ca59cfb8ba86655`.
- Registra o port `IDocumentContentStore`, descritores verificados,
  `PutAndVerifyAsync`, `OpenVerifiedAsync`, media type validado e migração dos
  consumidores internos sem schema, migration, package ou lockfile.
- Registra a preservação de `IStorageMaintenance`, `cleanup-plan-v1` e do
  protocolo existente de reserva/finalização como única autoridade física de
  cleanup.
- Registra os checks focais e o CI offline integral com 237 testes .NET, 38 do
  Dashboard, 93,76% de linhas e 67,15% de branches, além da preservação byte a
  byte da OpenAPI v1.
- Mantém renderer, PNG, direitos, render manifest persistido, ativação, v2,
  dados reais, gates, lifecycle e ações externas fora da autoridade executada.
- Atualiza Current State, histórico append-only, Data Dictionary, relatório
  de `STATE-04` e este changelog. A mudança é `PATCH`, factual, sem alterar a
  autoridade ou a ordem de dependência dos refinamentos restantes.

## 4.9.7 — 2026-08-07

- Corrige a recorrência em que o handoff de `S03-CORR-01` indicou uma revisão
  genérica de commits, embora Lifecycle registrasse um próximo item concreto
  na ordem de dependência.
- Determina que o primeiro item ainda não concluído de uma ordem governada tem
  prioridade como próximo trabalho; se faltar autoridade, obtê-la é a ação.
- Proíbe substituir esse avanço por `revisar commits`, `considerar próximos
  passos` ou decidir opcionalmente se deseja continuar, salvo quando a revisão
  ou decisão for gate, pré-requisito ou entregável formal.
- Exige que respostas diretas sobre próximo passo, tarefa, atividade ou ação
  apresentem primeiro a ação concreta e somente depois a recapitulação.
- Registra como próxima ação atual a obtenção de autoridade delimitada para
  preparar o segundo refinamento da ordem, com owner técnico de `STATE-04`,
  preservando v1 e sem inferir execução.
- Atualiza AGENTS, Governance, Templates, Current State, histórico e este
  changelog. A mudança é `PATCH`, exclusivamente documental, sem produto,
  dependência, lockfile, OpenAPI, gate, lifecycle ou ação externa.

## 4.9.6 — 2026-08-07

- Registra a implementação local e offline de `S03-CORR-01` sob
  `AUTH-S03-CORR-001` no commit
  `5fdbbc36d8eee29fdeec4b179564bd1eff322558`.
- Registra a separação implementada entre `SupportedQueryLanguage=pt-BR|en-GB`,
  `DocumentContentLanguage` BCP 47 e `SourceDeclaredLanguage` observado, sem
  inferir `en` como `en-GB` e sem ampliar o runtime v1.
- Registra `DocumentPageImage`, `DocumentRenderManifest`, digest canônico,
  bindings físicos Control, migration única, compatibilidade vetorial e
  reachability de fonte/imagem; renderer, bytes PNG, serving e v2 permanecem
  não implementados.
- Registra upgrade/rollback/reapply somente em SQLite descartável, ausência de
  mudanças pendentes nos modelos Control e Vector e `eng/ci.ps1 -Offline`
  aprovado com 232 testes .NET, 38 do Dashboard, 93,74% de linhas e 67,11% de
  branches.
- Preserva OpenAPI v1 byte a byte, dependências, lockfiles, Dashboard, ADRs,
  dados do produto e candidato PostgreSQL; nenhuma ação externa foi executada.
- A mudança é `PATCH`: reconcilia os registros factuais com uma implementação
  já autorizada, sem alterar autoridade, gate, lifecycle ou o significado dos
  ADRs aceitos.

## 4.9.5 — 2026-08-07

- Aplica, sob autoridade separada, a reconciliação semântica conjunta dos ADRs
  0008 e 0009 sobre `main@3d15ad4f2726f715c8dcf880491927ad0ff37b2f`,
  corpus `4.9.4` e working tree limpa.
- Confirma `IDocumentContentStore` como única autoridade permanente de produto
  para bytes de fonte e PNGs content-addressed, separados de Git, Git LFS,
  quarentena, catálogo e vector store.
- Torna explícitos `pdf-page-png-v1`, `DocumentPageImage`,
  `DocumentRenderManifest`, renderização integral fail-closed, binding de
  lifecycle, reachability e backup/restore, todos ainda não implementados.
- Amplia o gate de direitos por documento para retenção da fonte, rendering,
  criação/retenção de derivados, display e distribuição/publicação pretendida;
  a expansão específica do candidato PostgreSQL permanece pendente.
- Separa `SupportedQueryLanguage=pt-BR|en-GB` de
  `DocumentContentLanguage` BCP 47, preserva `sourceDeclaredLanguage` e proíbe
  inferir `en` como `en-GB`.
- Preserva textos derivados da fonte no idioma original das citações e mantém
  a matriz obrigatória de quatro pares, acrescentando estratos exatos por cada
  idioma documental adicional sem fusão silenciosa.
- Preserva OpenAPI v1 byte a byte com SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`.
  `QueryResponseV2`, `CitationV2`, `PageImageEvidenceV1` e o endpoint visual
  permanecem somente contratos planejados e não implementados; nenhum artefato
  OpenAPI v2 foi criado.
- Reconciliam-se ADR-0002, ADR-0004, ADR-0008, ADR-0009, índice arquitetural,
  contratos canônicos, data dictionary, arquitetura da solução, módulo RAG,
  segurança, lifecycle, Quality Gates, threat model, planejamento S07-A,
  índice documental, estado factual e histórico append-only: 18 arquivos.
- A mudança é `PATCH`: torna correntes decisões arquiteturais já aceitas sem
  introduzir nova autoridade, capacidade executável, estado ou contrato
  implementado.
- Não altera código, testes, schema, migrations, dados, dataset, registro de
  elegibilidade, dependências, lockfiles, PDF ou OpenAPI; não gera PNGs,
  indexa, ativa, executa provider/browser/rede, publica, faz deploy ou realiza
  ação externa.
- `STATE-07 TESTING_HOMOLOGATION` permanece ativo; `S07-A` conserva somente a
  baseline de planejamento confirmada, sem autoridade de dataset ou execução.

## 4.9.4 — 2026-08-07

- Corrige a aplicação excessiva da ausência canônica introduzida em `4.9.2`,
  que permitiu omitir uma continuação diretamente relacionada apenas porque a
  solicitação estava concluída ou a execução ainda não tinha autoridade.
- Exige que todo handoff responda com exatamente uma ação concreta, priorizada,
  responsável e condição/autoridade à pergunta do proprietário sobre o
  próximo passo, tarefa, atividade ou ação.
- Determina que dado, documento, anexo, decisão ou autoridade ausente seja
  apresentado como a próxima ação quando ele desbloquear a continuação
  diretamente relacionada.
- Restringe `nenhum — a solicitação atual não exige trabalho adicional` aos
  casos em que a consulta ao estado factual e aos documentos proprietários não
  encontra nenhuma continuação acionável diretamente relacionada.
- Preserva a proibição de importar lifecycle, gate, backlog ou melhoria sem
  relação direta e não transforma recomendação em autoridade implícita.
- Atualiza AGENTS, Governance e Templates sem criar estado, gate, capacidade
  executável ou ação externa. A mudança é `PATCH` porque corrige enforcement e
  clareza do handoff sem alterar autoridade ou lifecycle.
- `STATE-07 TESTING_HOMOLOGATION` permanece ativo; `S07-A` conserva somente sua
  baseline de planejamento confirmada. Dataset, avaliação, testes, carga,
  segurança dinâmica, browser, providers, fontes reais, rede, OCI, GitHub,
  publicação, deploy, Automatic Quality Gate, Human Gate e `STATE-08`
  permanecem não autorizados e não executados.

## 4.9.3 — 2026-08-06

- Registra a decisão explícita `NORM-S06-001` do proprietário sobre
  `main@140c0516e4dbfc02808a90f0496550eb6b09da1b`, corpus `4.9.2` e working
  tree limpa.
- Mantém em `STATE-06` o entregável obrigatório de README factualmente atual e
  exige ao menos um exemplo cujo comando e resultado sejam realmente
  verificados no artefato integrado local/sintético, com essa fronteira
  declarada.
- Mantém em `STATE-08` a responsabilidade pelo README público final, que
  complementa ou substitui os exemplos anteriores com evidência própria,
  separadamente verificada em OCI e na execução real do produto.
- Reconciliam-se Lifecycle, roadmap, `S06-CORR-01` e `BL-M13` sem alterar a
  ordem dos estados, criar capacidade ou enfraquecer o gate reprovado.
- Classifica-se como `PATCH`: a mudança elimina uma divergência de ownership e
  torna explícitos os níveis de evidência já exigidos, sem decisão
  arquitetural nova.
- O registro não implementa as correções, não dispõe `AQG-S06-001` a
  `AQG-S06-003`, não repete o Automatic Quality Gate e não executa Human Gate,
  OCI ou `STATE-07`.

## 4.9.2 — 2026-08-03

- Corrige uma falha recorrente em que respostas de confirmação ou
  esclarecimento reintroduziam o próximo estado geral do projeto no corpo ou
  no handoff, misturando assuntos que o proprietário não pediu.
- Torna o pedido atual e explícito o limite temático comum do corpo e do
  handoff; um follow-up estreito não reativa lifecycle, backlog ou melhoria
  opcional por implicação.
- Restringe `Próximo trabalho recomendado` a uma entrega diretamente
  relacionada à solicitação atual e introduz a ausência canônica
  `nenhum — a solicitação atual não exige trabalho adicional`.
- Restringe a exposição de próximo estado/gate aos casos em que ela é material
  para o tema atual e exige `sem mudança` nos demais casos.
- Preserva o formato de encerramento, roteamento, autoridade, Human Gates,
  lifecycle, paralelismo e política de idioma; não altera escopo funcional do
  produto.
- Classifica-se como `PATCH`: a alteração corrige a aplicação da semântica já
  pretendida de não misturar trabalho, sem criar capacidade ou estado.
- Não autoriza implementação, entrada em `STATE-04`, rede, provider, GitHub,
  OCI, publicação, deploy ou mudança no DB-Notifier.

## 4.9.1 — 2026-08-02

- Aplica, sob autoridade separada, a semântica aceita do ADR-0007 à baseline
  limpa `main@9aa90c012e3bc973330f5a79678fc358c81809df`, corpus `4.9.0`.
- Reconciliam-se ADR-0002, contratos canônicos e módulo RAG para excluir
  `sourceObservationId` da identidade da geração e proteger o binding completo
  por `activationBindingSetDigest` no registro de ativação.
- Separam-se `catalogueRevision`, revisão do journal de observações e revisão
  transacional; `304`/hash idêntico preserva manifesto e geração e cria nova
  revisão íntegra do registro.
- Rollback passa a construir registro novo para geração retida e validada, com
  observações explicitamente selecionadas, compatíveis e atualmente elegíveis,
  sem replay de freshness histórica.
- Arquitetura da solução, requisitos, lifecycle, Quality Gates, roadmap,
  backlog, threat model e template de alteração de corpus rastreiam os dois
  digests, mismatch, filtros de bindings elegíveis, idempotência e crash
  boundaries.
- Atualizam-se relatório, índices e estado factual sem alegar implementação,
  teste de runtime ou disposição dos achados históricos.
- Classifica-se como `PATCH`: a mudança torna corrente uma autoridade já
  aceita, sem nova capacidade, alteração de lifecycle ou escopo funcional.
- A validação dirigida do diff não repete o Automatic Quality Gate. O gate
  permanece `REPROVADO`, Human Gate permanece `PENDENTE` e `STATE-03` continua
  sem autorização.
- Não autoriza implementação, rede, provider, conta, GitHub, OCI,
  DB-Notifier, publicação ou deploy.

## 4.9.0 — 2026-08-02

- Registra a decisão explícita `ADR-0007: ACEITAR.` do proprietário sobre
  `main@664187c6926be5ce4bef3734603f8d936626d535`, corpus `4.8.1`.
- Torna autoritativa a separação entre a identidade imutável da geração e a
  identidade revisionada do registro de ativação, protegida pelo novo
  `activationBindingSetDigest` que inclui `sourceObservationId`.
- Substitui somente as cláusulas conflitantes de identidade da geração e
  rollback exato do ADR-0002; preserva as demais decisões aceitas de catálogo,
  providers, persistência, segurança, egress, avaliação e OCI.
- Registra que `304`/hash idêntico preserva manifesto e geração, mas cria nova
  revisão íntegra do registro de ativação; rollback constrói registro novo com
  observações atualmente compatíveis e elegíveis.
- Classifica a alteração como `MINOR` porque uma proposta corretiva passa a
  autoridade arquitetural aceita sem alterar lifecycle ou escopo funcional.
- Não aplica ainda a reconciliação semântica em ADR-0002, contratos, RAG,
  requisitos, lifecycle, Quality Gates, roadmap ou threat model; esse trabalho
  exige autoridade separada.
- Não dispõe `AQG-S02-001`, não repete o Automatic Quality Gate, não solicita
  Human Gate e não autoriza `STATE-03`, implementação ou ação externa.

## 4.8.1 — 2026-08-02

- Registra o ADR-0007 como proposta corretiva de `AQG-S02-001`, sem decisão
  humana e sem alterar ainda a autoridade aceita do ADR-0002.
- Compara o modelo recomendado de identidades separadas com o modelo que gera
  novo `IndexGenerationId` para cada observação e rejeita mutação ou binding
  sem digest.
- Recomenda manter `sourceObservationId` fora de `sourceBindingSetDigest`,
  `generationSpecDigest` e `IndexGenerationId`, acrescentando
  `activationBindingSetDigest` ao registro completo de ativação.
- Define, apenas para futura aceitação, que `304`/hash idêntico preserva
  manifesto, `catalogueRevision` e geração, mas cria nova revisão/digest do
  registro; rollback constrói novo registro com observação atualmente
  compatível e nunca restaura freshness histórica.
- Reconcilia `AQG-S02-002` no threat model: os riscos de revogação TLS e
  divulgação OpenAI passam a distinguir a boundary arquitetural já aceita de
  controles, conta, egress e evidência ainda ausentes.
- Reconcilia `AQG-S02-003` nos documentos existentes do `STATE-00`, preservando
  `pt-BR`: mapa físico do ADR-0003, abstrações RAG em Application,
  persistência em Infrastructure e status aceito/condicional de administração,
  TLS, providers, persistência e OCI.
- Classifica a alteração como `PATCH`: corrige status factual e prepara uma
  decisão compatível, sem aceitar o ADR proposto, mudar lifecycle ou alterar a
  semântica aceita antes da decisão.
- Mantém 13 arquivos ativos em `prompts/`; o Automatic Quality Gate não foi
  repetido, permanece `REPROVADO`, e o Human Gate e `STATE-03` continuam sem
  autorização.
- A validação dirigida do pacote é documental e não substitui a auditoria
  combinada; seus resultados observados ficam registrados no relatório e no
  histórico append-only.
- Não autoriza implementação, rede, provider, conta, GitHub, OCI,
  DB-Notifier, publicação ou deploy.

## 4.8.0 — 2026-08-01

- Registra a aceitação humana explícita e independente de ADR-0002, ADR-0004,
  ADR-0005 e ADR-0006 sobre
  `main@39e2f803bf73cb4e2b59e56a0596e2858a3aed51`, corpus `4.7.0`.
- Preserva no ADR-0005 a natureza condicional de OCI, versões de packages e
  metas operacionais, além de consistência de backup, instance principal
  somente-leitura, divulgação limitada à OpenAI e controle do alias mutável.
- Promove contratos canônicos e threat model de propostas preparadas para a
  baseline arquitetural aceita, sem apresentá-los como implementação ou teste.
- Mantém fatos de conta, entitlement, capacidade, packages, spikes, IAM,
  restore, custo e runtime como não verificados ou condicionais conforme os
  ADRs.
- Remove somente o bloqueio de decisão arquitetural; a auditoria combinada
  permanece separadamente não autorizada, o Automatic Quality Gate continua
  `BLOQUEADO` e o Human Gate permanece `PENDENTE`.
- Classifica a alteração como `MINOR` porque quatro propostas passam a
  autoridade arquitetural aceita sem alterar estados, precedência ou
  lifecycle.
- A validação dirigida confirma exatamente 13 arquivos alterados, quatro ADRs
  com status único `accepted`, data e autoridade; corpus `4.8.0`; catálogo
  51/54/9; contagens de requisitos/ameaças preservadas; e Markdown, links,
  tabelas, UTF-8/LF e `git diff --check` válidos. Ela não executa a auditoria
  combinada de `STATE-02`.
- Não autoriza implementação, rede, download, providers, GitHub, OCI,
  publicação, deploy, DB-Notifier, auditoria combinada, Human Gate ou
  `STATE-03`.

## 4.7.0 — 2026-08-01

- Registra os 51 bancos exatos fornecidos pelo proprietário como catálogo
  inicial canônico, com 9 categorias e 54 associações muitos-para-muitos;
  Redis, SAP HANA e SingleStore permanecem entidades únicas multiclasse.
- Exige que cada banco ativo tenha ao menos um documento ativo PDF e/ou CSV e
  permite qualquer quantidade adicional, sem teto de produto.
- Torna bancos, categorias, documentos, versões e fontes compatíveis registros
  administráveis, sem lista hard-coded, código ou ADR por item. Novas classes
  de formato, protocolo, autenticação ou confiança preservam decisão própria.
- Define `Candidate`, `Active`, `Deactivated` e `Removed`: desativação preserva
  história, remoção é lógica e a saída do último documento ativo exige
  desativação explícita e atômica do banco.
- Substitui os corpora de consulta mutuamente exclusivos por recuperação
  unificada de todos os documentos ativos/current, mantendo origem local ou
  oficial como proveniência, trust, cobertura e citação explícitos.
- Reconcilia visão, arquitetura, RAG, governance, lifecycle, Quality Gates,
  segurança, roadmap, ADRs propostos, contratos, threat model, relatório,
  índices, estado factual e histórico.
- Mantém PostgreSQL como primeira fonte publicamente verificada, sem autorizar
  as outras fontes, rede, download ou crawling.
- Torna PDF e CSV formatos iniciais; PdfPig, pacote CSV, versões de packages,
  SQLite vector, OCI e metas operacionais permanecem candidatos condicionais.
- Mantém ADR-0002 e ADR-0004 a ADR-0006 como `proposed`; não registra decisão
  ADR por inferência nem executa auditoria combinada ou Human Gate.
- Classifica a alteração como `MINOR` por ampliar cardinalidade, formatos,
  administração, recuperação, contratos e homologação sem mudar autoridade,
  precedência ou lifecycle.
- A validação dirigida confirma exatamente 22 arquivos alterados, catálogo
  51/54/9 idêntico na visão e no ADR-0004, 25 RF, 18 RNF, 20 critérios de
  aceitação, 19 itens Must, 36 ameaças, 15 grupos de testes, quatro ADRs
  `proposed`, links/H1/fences/tabelas, LF/newline final e `git diff --check`.
  Ela não executa a auditoria combinada de `STATE-02`.
- Não altera código, instala dependência, acessa rede/provider/conta, modifica
  GitHub/OCI/DB-Notifier, publica, faz deploy ou autoriza `STATE-03`.

## 4.6.0 — 2026-08-01

- Registra a decisão explícita e independente do proprietário de remover os
  tetos propostos de 12 sistemas e 120 páginas do corpus.
- Mantém cada versão publicada finita e exige registrar suas contagens reais,
  sem definir máximo de produto para sistemas ou páginas.
- Substitui o recorte de doze sistemas por uma lista integral aprovada pelo
  proprietário; a lista de 51 nomes anteriormente informada não é recuperável
  dos arquivos rastreados e não pode ser reconstruída por inferência.
- Preserva limites configuráveis de bytes, memória de trabalho, tempo e
  concorrência como controles de segurança e capacidade condicionais ao
  ambiente. Esses controles não definem elegibilidade do catálogo nem
  reintroduzem os tetos removidos.
- Reconcilia visão, ADR-0004 proposto, relatório de arquitetura, snapshot
  factual, histórico e este changelog sem alterar código, dependências ou
  runtime.
- Mantém ADR-0002 e ADR-0004 a ADR-0006 como `proposed`; não registra as
  decisões ainda não materializadas desses ADRs nem executa a auditoria
  combinada de `STATE-02`.
- Não acessa rede, provider, GitHub, OCI ou DB-Notifier; não publica, implanta,
  solicita Human Gate nem autoriza `STATE-03`.
- Classifica a mudança como `MINOR` porque remove dois limites funcionais da
  baseline proposta e altera os critérios futuros de escala e homologação.
- A validação direcionada cobre ocorrências residuais dos tetos, versão do
  corpus, status dos ADRs, links locais dos arquivos alterados, UTF-8/LF e
  `git diff --check`; não substitui a auditoria combinada pendente.

## 4.5.0 — 2026-08-01

- Registra a decisão explícita do proprietário de suportar `Light` e `Dark`
  como o conjunto fechado de temas do Dashboard.
- Exige escolha visual explícita, independente de `interfaceLanguage`,
  `questionLanguage`, `answerLanguage` e `contentLanguage`, sem alterar
  conteúdo, escopo, resposta, evidência ou citações.
- Exige que fundo, superfície, texto, borda, foco e estados preservem
  contraste, hierarquia, reflow e informação que não dependa apenas de cor nos
  dois temas.
- Acrescenta `RF-023`, `RNF-017`, `AC-MVP-018` e `BL-M18`, e reconcilia visão,
  arquitetura, contratos, lifecycle, Quality Gates, roadmap, threat model,
  relatório, índices e estado factual com a decisão.
- Executa nos dois temas a matriz de quatro combinações entre
  `interfaceLanguage` e `questionLanguage`, totalizando oito combinações de
  componente, acessibilidade e fluxo.
- Mantém tema inicial, preferência do sistema, persistência e fallback como
  decisões futuras de frontend; a ordem `Light`/`Dark` não define preferência
  ou valor inicial.
- Mantém ADR-0002 e ADR-0004 a ADR-0006 como `proposed`; a decisão não aceita
  corpus, fonte, provider, persistência, egress, OCI ou qualquer outro
  conteúdo desses ADRs.
- Não altera comportamento executável, instala dependência, acessa rede,
  executa provider, modifica recurso externo nem avança o lifecycle.
- Classifica a mudança como `MINOR` por adicionar uma capacidade funcional e
  critérios de homologação compatíveis antes da implementação pública.
- A auditoria documental aprovou 83 arquivos não ignorados, 30 Markdown, 13
  arquivos em `prompts/`, 23 RF, 17 RNF, 18 critérios de aceitação, 34 itens
  de backlog, 92 definições estáveis sem duplicidade, H1, links, UTF-8/LF,
  status dos ADRs, 30 ameaças, 12 grupos de testes de segurança e
  `git diff --check`.

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
