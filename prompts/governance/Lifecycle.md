# Ciclo de Desenvolvimento

## Regra geral

Cada estado exige entradas, entregáveis, auditoria automática, decisão humana
e handoff. Correções pertencem ao estado dono do defeito. Os formatos de
evidência estão em [`../templates/Templates.md`](../templates/Templates.md).

## STATE-00 DISCOVERY

Objetivo: compreender o Challenge, delimitar o problema e o MVP, inventariar
fontes, propor arquitetura e preparar o setup.

Entradas:

- requisitos do proprietário;
- materiais originais locais;
- análise somente leitura do DB-Notifier.

Entregáveis:

- visão, escopo, requisitos e critérios;
- riscos, premissas e decisões pendentes;
- arquitetura e módulo RAG de alto nível;
- ADRs propostos;
- governança, gates e memória;
- roadmap e backlog;
- relatório automático do estado.

Aceite:

- requisitos oficiais e interpretações estão rastreáveis;
- MVP e evolução futura estão separados;
- materiais ignorados não são dependência do produto;
- propostas não são apresentadas como implementação;
- links e estrutura documental são válidos;
- Human Gate é explícito e separado.

## STATE-01 PROJECT_SETUP

Objetivo: preparar o repositório, solução, convenções, builds, testes,
configuração segura e CI sem implementar RAG.

Pré-condições de entrada:

- Human Gate de `STATE-00` concluído;
- `GATE-B01` concluído, com ADR-0001 aceito, licença do repositório escolhida
  e mapa físico de projetos registrado;
- entrada em `STATE-01` autorizada explicitamente em decisão separada;
- Git init, scaffold e dependências ainda não executados antes dessa
  autorização.

Entregáveis:

- Git e branch inicial autorizados;
- `.editorconfig`, `.gitattributes` e `.gitignore` completos;
- SDK/toolchains fixados;
- solution e projetos vazios nas fronteiras aprovadas;
- gestão central de dependências e lockfiles;
- hosts mínimos com health sem regra funcional;
- testes estruturais e pipeline CI;
- documentação de onboarding;
- relatório do setup.

Aceite:

- clone limpo restaura, compila e testa;
- arquitetura de dependências é verificada;
- configuração inválida falha fechada;
- nenhum secret ou corpus privado;
- nenhuma ingestão, recuperação ou geração prematura;
- Human Gate do setup concluído.

## STATE-02 ARCHITECTURE

Objetivo: aceitar limites, providers do MVP, contratos, dados, segurança,
implantação e avaliação.

Entregáveis:

- ADRs aceitos ou rejeitados;
- contratos canônicos e diagramas;
- threat model detalhado;
- seleção de parser, embeddings, vetor e LLM;
- definição do corpus e da licença do corpus;
- seleção do PDF oficial, URL canônica, termos/licença, maxAge e limites;
- decisão de persistência durável para conteúdo bruto, catálogo e índice;
- política de configuração, `AI_PROVIDER_EGRESS`, `VECTOR_STORE_EGRESS`,
  `OFFICIAL_SOURCE_EGRESS` e `OCI_RUNTIME_EGRESS`;
- contratos canônicos de busca vetorial, falhas, readiness e OpenAPI;
- proteção SSRF com canonicalização e pinning DNS/IP por conexão;
- estratégia de avaliação, OCI e rollback.

Aceite:

- dependências apontam para o núcleo;
- providers são substituíveis por portas;
- fonte local e oficial online estão separadas;
- o recorte oficial é uma URL PDF, sem crawling ou fallback silencioso;
- limites, custos, falhas e segurança foram tratados;
- thresholds são definidos antes da homologação.

## STATE-03 DATA_AND_INDEX_MODELING

Objetivo: modelar catálogo, versões, chunks, snapshots oficiais, freshness,
manifestos, gerações, auditoria e persistência.

Entregáveis:

- modelo e dicionário;
- constraints, índices e concorrência;
- migrations não produtivas;
- retenção e recuperação;
- `SourceScope`, snapshot imutável, observações de revalidação, URL canônica,
  freshness e estados de retirada;
- manifesto canônico versionado, staging/finalização idempotentes, digest e
  contagens dos artefatos lógicos e identidade determinística da geração
  finalizada;
- `CorpusActivationRecord` e algoritmo transacional de ativação/rollback;
- retenção do conteúdo bruto alcançável e cleanup de órfãos;
- fixtures determinísticas.

Aceite:

- documento e índice têm versões independentes;
- conteúdo bruto permanece reabrível para rebuild e rollback autorizados;
- candidato parcial nunca é consultável e a finalização valida
  digest/contagens/readback antes da ativação;
- secrets não integram o modelo;
- geração parcial ou observação não vinculada nunca fica ativa;
- `Local` e `OfficialOnline` integram identidades/digests e não se misturam;
- migrations e recuperação são verificáveis;
- corpus do produto não é confundido com banco documentado.

## STATE-04 BACKEND_IMPLEMENTATION

Objetivo: implementar ingestão local, sincronização manual oficial,
indexação, recuperação por escopo, geração e API.

Entregáveis:

- Domain e Application;
- adapters autorizados;
- adapter allowlisted para um PDF oficial e snapshot governado;
- persistência;
- API versionada;
- artefato OpenAPI v1 versionado e testes de compatibilidade;
- configuração;
- citações e recusa;
- testes unitários, arquitetura, contrato e integração.

Aceite:

- um corpus é processado ponta a ponta;
- providers não vazam para o núcleo;
- falhas são tipadas e sanitizadas;
- hard pre-filter integra o contrato do vector store e precede o top-k;
- geração anterior sobrevive a falha de reconstrução;
- falha/stale oficial não faz fallback para `Local`;
- perguntas sem evidência recusam;
- suíte padrão não exige serviço pago.

## STATE-05 FRONTEND_IMPLEMENTATION

Objetivo: implementar a interface web mínima.

Entregáveis:

- pergunta e resposta;
- seletor obrigatório `Local`/`OfficialOnline`;
- citações;
- estados loading, vazio, erro, fonte stale/indisponível, rate limit e sem
  evidência;
- responsividade e acessibilidade;
- testes de componente e fluxo.

Aceite:

- teclado, foco, contraste e semântica adequados;
- nenhuma lógica de autorização ou acesso direto a provider no cliente;
- informação de fonte não depende apenas de cor;
- troca de escopo exige ação explícita e citações exibem snapshot/freshness;
- mensagens são factuais e localizadas.

## STATE-06 INTEGRATION

Objetivo: integrar API, interface, providers e artefato em ambiente
controlado.

Entregáveis:

- E2E local/sandbox;
- servidor falso para sync oficial e smoke real opt-in somente autorizado;
- configuração por ambiente;
- resiliência e cancelamento;
- artefato reproduzível;
- plano e ensaio não produtivo de OCI;
- exemplos reais para o README.

Aceite:

- fluxo completo é reproduzível;
- reinicialização e persistência são conhecidas;
- erros externos não corrompem o índice ativo;
- consulta nunca faz fetch e os dois escopos permanecem isolados;
- nenhum secret no artefato;
- evidências não são confundidas com produção.

## STATE-07 TESTING_HOMOLOGATION

Objetivo: validar qualidade RAG, segurança, desempenho, recuperação e
experiência representativa.

Entregáveis:

- dataset e relatório de avaliação;
- testes negativos e prompt injection;
- SSRF, DNS rebinding, resposta DNS mista, pinning IP/Host/SNI, redirect, URL,
  media type, tamanho, freshness e source leakage;
- carga e limites;
- recuperação/rollback;
- acessibilidade;
- matriz de ambiente/provider;
- riscos residuais.

Aceite:

- thresholds previamente aprovados são atendidos;
- limitações e custos são explícitos;
- nenhuma vulnerabilidade bloqueadora;
- fonte oficial real é testada somente quando egress específico foi autorizado;
- afirmações públicas correspondem à matriz testada;
- Human Gate repete amostras críticas.

## STATE-08 PRODUCTION_RELEASE

Objetivo: publicar a entrega do Challenge no alvo OCI autorizado.

Entregáveis:

- release candidate identificável;
- configuração e secrets externos;
- `OFFICIAL_SOURCE_EGRESS` restrito à URL oficial exata e
  `OCI_RUNTIME_EGRESS` composto somente pelos destinos separadamente
  autorizados;
- `VECTOR_STORE_EGRESS` vazio para adapter local ou restrito ao serviço
  gerenciado aprovado;
- deploy e smoke test;
- health e observabilidade;
- rollback;
- link/captura de execução;
- README final e submissão GitHub.

Aceite:

- alvo e ação autorizados;
- aplicação pública funcional;
- evidência sanitizada e reproduzível;
- nenhum material local ignorado requerido;
- rollback ou recuperação ensaiados;
- sincronização oficial, freshness e runbook operacionais verificados;
- critérios formais do Challenge atendidos.

## Matriz módulo × estado

| Módulo | S02 | S03 | S04 | S05 | S06 | S07 | S08 |
|---|---|---|---|---|---|---|---|
| Corpus Catalog | Contratos | Modelo | Casos de uso | Visão | Persistência | Recuperação | Operação |
| Document Ingestion | Adapters | Versões | Parser | Estado | E2E | Segurança/carga | Runbook |
| Indexing/Retrieval | Providers | Manifesto | Pipeline | Diagnóstico | Compatibilidade | Evals | Operação |
| Answer Generation | Política | Evidência | LLM/citações | Resposta | E2E | Groundedness | Limites |
| Query Experience | API/UX | N/A | API | Interface | Integração | A11y/carga | Publicação |
| Operations/Governance | Segurança | Auditoria | Health/logs | Erros | Ambiente | Homologação | Release |
| Official Sources | Contrato/allowlist | Snapshot/freshness | Adapter/sync | Seletor/citação | E2E controlado | SSRF/stale | Egress/runbook |
| External Integration Contracts | Política OpenAPI | N/A | Artefato/testes | Cliente Web | Compatibilidade | Regressão | Publicação |

Desenho anterior à fase não autoriza implementação antecipada.
