# Estado Atual

Este documento é o snapshot factual vigente do workspace em 2026-08-01. Ele
não concede autoridade. Evolução e decisões no contexto original pertencem ao
[`State-Transition-Log.md`](State-Transition-Log.md) e aos relatórios
proprietários.

## Lifecycle e gates

- Posição: `STATE-00 DISCOVERY` encerrado; `GATE-B01
  ARCHITECTURE_BOOTSTRAP_DECISION` aprovado e encerrado; `STATE-01
  PROJECT_SETUP` encerrado após Human Gate aprovado sem ressalvas em
  2026-07-31; entrada em `STATE-02 ARCHITECTURE` autorizada em 2026-07-31,
  com execução documental e local, sequencial, dos lotes `S02-A` e `S02-B`.
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
  formalizada como corpus `4.7.0` (`MINOR`); a auditoria combinada de
  `STATE-02` permanece pendente. Nenhum desses incrementos transitou o
  lifecycle ou aceitou ADR por implicação.
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
  2026-07-31, com ADR-0002 e ADR-0004 a ADR-0006 ainda `proposed`, contratos
  canônicos, threat model e relatório factual. A verificação pública
  autorizada foi registrada nos commits
  `f1066c3509f5f48d4fe6e21c9e36403e642c1431`,
  `e80f8c41bea3f28deff3d8cdccafccbca5dcc016` e
  `9cc62746ea2ba861676a2d5bfee317eaf66dad7c`: nenhum item de fonte primária
  pública permanece pendente no escopo autorizado. Fatos dependentes de
  conta, entitlement, capacidade ou execução continuam não verificados e
  exigem autoridade futura própria; nenhuma escolha foi aceita por
  implicação.
- Automatic Quality Gate de `STATE-02`: `BLOQUEADO` até decisões humanas
  explícitas de ADR-0002 e ADR-0004 a ADR-0006, reconciliação da baseline
  aceita/rejeitada e auditoria combinada dos documentos resultantes. Human
  Gate de `STATE-02`: `PENDENTE` e ainda não pode ser solicitado.
- ADR-0001: `superseded` pelo ADR-0003, após aceitação original no
  `GATE-B01`; ADR-0002: `proposed`; ADR-0003: `accepted` pela solicitação
  humana explícita de renomear o projeto para `RAG-Challenge`, incorporando
  sem alteração todas as decisões não relacionadas a nomenclatura do
  ADR-0001; ADR-0004, ADR-0005 e ADR-0006: `proposed`.

## Baseline documental

- Os 20 arquivos da estrutura originalmente aprovada permanecem preservados;
  a política de idioma acrescentou o 21º documento público por incremento
  versionado, e o ADR-0003 acrescentou o 22º.
- A baseline aprovada no Human Gate de `STATE-00` permanece `3.4.0`.
- O corpus de instruções vigente possui versão `4.7.0` e 13 arquivos em
  `prompts/`.
- Visão, requisitos, arquitetura, RAG, segurança, qualidade, lifecycle,
  roadmap, backlog, estado, histórico e templates estão documentados.
- `STATE-02` acrescentou seis artefatos técnicos propostos: três ADRs, um
  contrato canônico, um threat model e um relatório de execução. Eles não são
  decisões aceitas nem evidência de implementação.
- A auditoria do pacote proposto confirmou 83 arquivos não ignorados, 30
  Markdown, links e formato válidos, quatro ADRs com status `proposed`, 30 IDs
  de ameaça e 12 grupos de testes de segurança. As verificações posteriores
  reconciliaram os fatos públicos de fonte oficial, parser/package,
  provider/model e OCI sem resolver fatos dependentes de conta ou runtime e
  sem substituir decisões humanas.
- A auditoria do corpus `4.1.0` confirmou 22 documentos, 114 links locais
  válidos, 20 RF, 14 RNF, 15 critérios de aceitação, 31 itens de backlog, 8
  módulos, 13 riscos, formato consistente e rastreabilidade. A implementação
  existente continua limitada ao scaffold entregue pelo `STATE-01`
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
  decisão própria. Nada foi implementado, ingerido, indexado ou ativado, e os
  quatro ADRs permanecem propostos.
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
  sequencialmente na conversa coordenadora, sem lanes paralelas.
- A execução foi exclusivamente documental; runtime preflight permaneceu
  `NÃO APLICÁVEL`, sem inspeção ou encerramento de processos.
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
  projetos .NET de testes, conforme o ADR-0003, que incorpora as decisões
  não relacionadas a nomenclatura do ADR-0001. Eles contêm somente markers,
  composição de setup, health e verificações estruturais; nenhuma lógica RAG
  ou funcional.
- SDK .NET `10.0.302`, C# `14.0`, Node.js `24.18.0` e npm `11.16.0` estão
  fixados. NuGet usa gestão central e sete lockfiles reproduzidos offline.
- Restore .NET offline locked, format, build Release, 15 testes e cobertura
  mesclada foram aprovados; cobertura observada: 88% de linhas e 100% de
  branches.
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
- Não existem API funcional, ingestão, recuperação, banco, vector store,
  modelo, corpus, container, infraestrutura ou deploy.
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
  íntegro e ativação/rollback pelo registro completo versionado.
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

## Decisões pendentes

1. Decidir ADR-0002 explicitamente.
2. Aceitar, rejeitar ou alterar o catálogo 51/54/9, o ciclo administrativo,
   PDF/CSV, licenças por documento e avaliação extensível reconciliados no
   ADR-0004. Os antigos tetos e o PDF único não integram a baseline atual.
3. Aceitar, rejeitar ou alterar a primeira fonte PostgreSQL 18 candidata e a
   política aplicável a registros oficiais compatíveis, frequência
   manual, `maxAge`, limites, trust e risco residual de revogação TLS; URL,
   media type, tamanho, licença, robots e TLS local foram verificados no
   escopo público autorizado.
4. Aceitar, rejeitar ou alterar PdfPig 0.1.15 condicionado, o parser CSV ainda
   sem package selecionado, normalização e `paragraph-window-v1`; qualidade e
   segurança dependem de spikes futuros separadamente autorizados.
5. Aceitar, rejeitar ou alterar OpenAI `text-embedding-3-small`, incluindo o
   risco de alias mutável e a obrigação de passar a recuperação cruzada
   `pt-BR`/`en-GB`; tier, entitlement, limites da conta futura e desempenho
   bilíngue não foram verificados.
6. Aceitar, rejeitar ou alterar `SqliteExactVectorStore`; 10.000 chunks é ponto
   inicial de benchmark, não teto, e a prova de desempenho pertence a teste
   futuro sobre catálogo representativo.
7. Aceitar, rejeitar ou alterar OpenAI `gpt-4.1-mini-2025-04-14`, retenção,
   divulgação de dados, ausência documentada de residência brasileira e a
   obrigação de responder em `questionLanguage`; elegibilidade, controles da
   conta futura e desempenho bilíngue não foram verificados.
8. Decidir SQLite/filesystem duráveis, retenção, backup consistente e rollback.
9. Aceitar, rejeitar ou alterar o alvo OCI condicional em `sa-saopaulo-1`, A1
   ARM64 1 OCPU/6 GiB, volume inicial 50 GiB, backup regional diário/pré-mudança,
   retenção 14 dias, RPO 24 h, objetivo de restauração 8 h, Secret Management
   e instance principal somente-leitura; capacidade, entitlement, IAM, restore
   e cobrança reais não foram verificados, e fontes públicas divergem sobre a
   franquia gratuita.
10. Decidir dataset extensível `rag-eval-catalogue-v1`, rubrica e thresholds,
    preservando a matriz `pt-BR`/`en-GB` já decidida para perguntas,
    respostas e idioma original das citações.
11. Decidir ADR-0006, incluindo egress, risco residual de revogação TLS,
    administração, falhas/readiness e compatibilidade OpenAPI.
12. Decidir no `STATE-05` o idioma inicial da interface, a persistência da
    preferência e o comportamento de fallback; o conjunto suportado
    `pt-BR`/`en-GB` e sua independência do idioma da consulta já estão
    decididos.
13. Decidir no `STATE-05` o tema inicial, eventual preferência do sistema,
    persistência e fallback; o conjunto suportado `Light`/`Dark` e sua
    independência dos idiomas já estão decididos.

## Próxima autoridade

Submeter ADR-0002 e ADR-0004 a ADR-0006 a decisões humanas explícitas de
aceitação, rejeição ou alteração, sem inferir uma decisão a partir de outra.
Depois, reconciliar a baseline escolhida e executar a auditoria combinada
exigida pelo Automatic Quality Gate. Fatos dependentes de conta, entitlement,
capacidade, spike ou runtime permanecem para autoridades futuras próprias e
não bloqueiam a decisão documental por ausência de fonte pública. O Human Gate
e o encerramento de `STATE-02` continuarão exigindo Automatic Quality Gate e
resumo completos próprios. GitHub, OCI, publicação, deploy, demais ações
externas e mudanças no DB-Notifier continuam sem autorização.
