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
  mesma data; sua auditoria incremental foi `APROVADA`, sem transição de
  lifecycle ou aceitação implícita de ADR.
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
- O corpus de instruções vigente possui versão `4.3.0` e 13 arquivos em
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
- MVP com um corpus, um PDF local publicável e uma fonte oficial online
  allowlisted.
- Seletor explícito de consulta entre `Local` e `OfficialOnline`, sem mistura
  silenciosa de evidências.
- Sincronização manual e governada da fonte oficial para snapshot versionado.
- Fonte oficial pública sem credenciais, egress deny-by-default e validação TLS
  sem destinos laterais não autorizados.
- Resposta grounded com citações e evidência insuficiente explícita.
- Perguntas e respostas com idioma explícito `pt-BR` ou `en-GB`, resposta no
  idioma da pergunta e citações preservadas no idioma original da fonte.
- Substituição manual de documento e nova geração de índice.
- Conteúdo bruto imutável/reabrível, staging não consultável, manifesto final
  íntegro e ativação/rollback pelo registro completo versionado.
- Contrato HTTP/OpenAPI v1 versionado pertencente ao RAG-Challenge; adapters
  consumidores permanecem fora deste repositório.
- Execução local e futuro deploy OCI.
- GitHub Pages somente como frontend estático opcional.

## Capacidades futuras inativas

- múltiplos acervos;
- formatos adicionais;
- sincronização incremental agendada;
- múltiplas fontes oficiais e crawling genérico;
- múltiplos providers;
- RBAC/multi-tenancy;
- integração ao DB-Notifier.

Nenhuma dessas capacidades está implementada, testada, implantada ou
autorizada.

## Decisões pendentes

1. Decidir ADR-0002 explicitamente.
2. Aceitar, rejeitar ou alterar o corpus autoral, a lista de bancos e a
   licença `CC BY 4.0` propostos pelo ADR-0004.
3. Aceitar, rejeitar ou alterar a fonte PostgreSQL 18 candidata, frequência
   manual, `maxAge`, limites, trust e risco residual de revogação TLS; URL,
   media type, tamanho, licença, robots e TLS local foram verificados no
   escopo público autorizado.
4. Aceitar, rejeitar ou alterar PdfPig 0.1.15, normalização e
   `paragraph-window-v1`; qualidade e segurança de extração dependem de spike
   futuro separadamente autorizado.
5. Aceitar, rejeitar ou alterar OpenAI `text-embedding-3-small`, incluindo o
   risco de alias mutável e a obrigação de passar a recuperação cruzada
   `pt-BR`/`en-GB`; tier, entitlement, limites da conta futura e desempenho
   bilíngue não foram verificados.
6. Aceitar, rejeitar ou alterar `SqliteExactVectorStore` e seus limites; a
   prova de desempenho pertence a teste futuro.
7. Aceitar, rejeitar ou alterar OpenAI `gpt-4.1-mini-2025-04-14`, retenção,
   divulgação de dados, ausência documentada de residência brasileira e a
   obrigação de responder em `questionLanguage`; elegibilidade, controles da
   conta futura e desempenho bilíngue não foram verificados.
8. Decidir SQLite/filesystem duráveis, retenção, backup e rollback.
9. Aceitar, rejeitar ou alterar OCI Compute em `sa-saopaulo-1`, shape,
   volume, backup, secrets, TLS e orçamento; capacidade, entitlement, limites
   efetivos e cobrança da tenancy futura não foram verificados, e fontes
   públicas divergem sobre a franquia gratuita.
10. Decidir dataset `rag-eval-mvp-v1`, rubrica e thresholds propostos,
    preservando a matriz `pt-BR`/`en-GB` já decidida para perguntas,
    respostas e idioma original das citações.
11. Decidir ADR-0006, incluindo egress, risco residual de revogação TLS,
    administração, falhas/readiness e compatibilidade OpenAPI.

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
