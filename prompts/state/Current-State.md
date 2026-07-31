# Estado Atual

Este documento é o snapshot factual vigente do workspace em 2026-07-31. Ele
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
  também `APROVADOS`.
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
  preparado em 2026-07-31, com ADR-0002 e ADR-0004 a ADR-0006 ainda
  `proposed`, contratos canônicos, threat model e relatório factual. Fatos de
  fonte oficial, package/parser, provider/model e OCI dependentes de rede não
  foram verificados e nenhuma escolha foi aceita por implicação.
- Automatic Quality Gate de `STATE-02`: `BLOQUEADO` até verificação externa
  autorizada, decisões humanas explícitas de cada ADR e nova auditoria da
  baseline resultante. Human Gate de `STATE-02`: `PENDENTE` e ainda não pode
  ser solicitado.
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
- O corpus de instruções vigente possui versão `4.1.0` e 13 arquivos em
  `prompts/`.
- Visão, requisitos, arquitetura, RAG, segurança, qualidade, lifecycle,
  roadmap, backlog, estado, histórico e templates estão documentados.
- `STATE-02` acrescentou seis artefatos técnicos propostos: três ADRs, um
  contrato canônico, um threat model e um relatório de execução. Eles não são
  decisões aceitas nem evidência de implementação.
- A auditoria do pacote proposto confirmou 83 arquivos não ignorados, 30
  Markdown, links e formato válidos, quatro ADRs com status `proposed`, 30 IDs
  de ameaça e 12 grupos de testes de segurança. A aprovação estrutural não
  resolve os bloqueios externos nem substitui decisões humanas.
- A auditoria do corpus `4.1.0` confirmou 22 documentos, 114 links locais
  válidos, 20 RF, 14 RNF, 15 critérios de aceitação, 31 itens de backlog, 8
  módulos, 13 riscos, formato consistente e rastreabilidade. A implementação
  existente continua limitada ao scaffold entregue pelo `STATE-01`
  encerrado.
- A arquitetura adota princípios compatíveis com o DB-Notifier sem criar
  referência ou dependência entre os projetos.
- Cada solicitação do proprietário recebe exatamente um encerramento compacto
  em `pt-BR`, somente na resposta final. Atualizações intermediárias informam
  somente progresso materialmente novo: não repetem nem parafraseiam
  conclusões, o bloco, o roteamento, o texto para copiar, o raciocínio ou o
  paralelismo.
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
- O handoff final classifica o paralelismo como sequencial, opcional ou
  recomendado; plano e mensagens por lane aparecem somente quando aplicáveis.
- O handoff distingue solicitação atual, próximo trabalho recomendado,
  posição de estado/gate, ação imediata do proprietário e conversa
  recomendada. `Lote`, `tarefa`, `atividade` e `passo` ficam restritos a seus
  papéis internos e não competem como rótulos do encerramento.
- Quando a ação imediata exige continuar, iniciar, retomar, responder,
  confirmar, decidir, autorizar ou enviar algo em uma conversa, o texto
  completo para copiar e enviar aparece dentro do próprio encerramento,
  imediatamente depois da conversa recomendada, destacado em bloco cercado
  cujo rótulo e delimitadores ficam fora do payload. A regra inclui textos de
  uma linha e Human Gate; a ausência só é válida quando nenhuma ação depende
  de mensagem.
- Todo handoff recomenda para a próxima conversa um dos níveis `Leve`,
  `Médio`, `Alto`, `Extra alto`, `Máximo` ou `Ultra`, com justificativa e
  alternativa se indisponível. A orientação não configura o Codex nem amplia
  autoridade, e cada lane recebe recomendação própria.
- Git local existe. Escritas paralelas futuras exigem branches e worktrees
  separados, ownership disjunto e isolamento de runtime/dados; esta execução
  permaneceu sequencial na conversa coordenadora.
- Runtime preflight é `NÃO APLICÁVEL` para documentação e análise read-only:
  nenhum shutdown é anunciado, nenhum processo é enumerado e nada é
  encerrado.
- [`Language-Policy.md`](../governance/Language-Policy.md) é a autoridade
  temática única para idioma; os demais documentos apontam para ela.
- Títulos, rótulos, orientações e mensagens destinados ao proprietário usam
  `pt-BR`; literais técnicos preservam a grafia exigida.
- Evidências e documentos existentes do `STATE-00` preservam o idioma do
  próprio arquivo; novos artefatos técnicos usam `en-GB`, enquanto tradução
  integral e idioma da interface permanecem decisões separadas.

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
2. Confirmar ou substituir o corpus autoral, a lista de bancos e a licença
   `CC BY 4.0` propostos pelo ADR-0004.
3. Verificar e decidir a fonte PostgreSQL 18 candidata, URL pública, licença,
   termos, limites, `maxAge`, trust e política TLS/revogação.
4. Verificar e decidir PdfPig, normalização e `paragraph-window-v1`.
5. Verificar e decidir OpenAI `text-embedding-3-small`.
6. Verificar e decidir `SqliteExactVectorStore` e seus limites.
7. Verificar e decidir OpenAI `gpt-4.1-mini-2025-04-14` e a divulgação de
   dados ao provider.
8. Decidir SQLite/filesystem duráveis, retenção, backup e rollback.
9. Verificar e decidir OCI Compute em `sa-saopaulo-1`, shape, volume, secrets,
   TLS e orçamento.
10. Decidir dataset `rag-eval-mvp-v1`, rubrica e thresholds propostos.
11. Decidir ADR-0006, incluindo egress, risco residual de revogação TLS,
    administração, falhas/readiness e compatibilidade OpenAPI.

## Próxima autoridade

Obter autoridade separada e limitada para verificar, em fontes primárias, os
fatos externos enumerados no relatório de `STATE-02`, sem instalar, baixar
corpus, chamar API paga ou criar recurso. Depois, reconciliar a evidência e
submeter ADR-0002 e ADR-0004 a ADR-0006 a decisões humanas explícitas. O Human
Gate e o encerramento de `STATE-02` continuarão exigindo Automatic Quality Gate
e resumo completos próprios. GitHub, OCI, publicação, deploy, demais ações
externas e mudanças no DB-Notifier continuam sem autorização.
