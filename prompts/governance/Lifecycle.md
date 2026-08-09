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
- contrato bilíngue de consulta com `pt-BR`/`en-GB`, resposta no idioma da
  pergunta e citação no idioma original;
- threat model detalhado;
- seleção de parser, embeddings, vetor e LLM;
- definição do corpus e da licença do corpus;
- catálogo canônico inicial, ciclo administrativo de bancos/documentos,
  formatos PDF/CSV e registros de fontes oficiais com URLs, termos/licenças,
  maxAge e limites individuais;
- decisão de persistência durável para conteúdo bruto, catálogo e índice;
- política de configuração, `AI_PROVIDER_EGRESS`, `VECTOR_STORE_EGRESS`,
  `OFFICIAL_SOURCE_EGRESS` e `OCI_RUNTIME_EGRESS`;
- contratos canônicos de busca vetorial, falhas, readiness e OpenAPI;
- proteção SSRF com canonicalização e pinning DNS/IP por conexão;
- estratégia de avaliação, OCI e rollback.

Aceite:

- dependências apontam para o núcleo;
- providers são substituíveis por portas;
- origem local/oficial permanece rastreável sem dividir silenciosamente a
  recuperação unificada;
- cada fonte oficial possui URL PDF/CSV allowlisted, sem crawling ou fallback
  silencioso;
- limites, custos, falhas e segurança foram tratados;
- thresholds são definidos antes da homologação.
- idioma da pergunta, resposta e evidência possui semântica canônica sem
  decidir o idioma da interface.

## STATE-03 DATA_AND_INDEX_MODELING

Objetivo: modelar bancos, categorias muitos-para-muitos, documentos, versões,
chunks, snapshots oficiais, freshness, manifestos, gerações, auditoria e
persistência.

Entregáveis:

- modelo e dicionário;
- constraints, índices e concorrência;
- migrations não produtivas;
- retenção e recuperação;
- estados Candidate/Active/Deactivated/Removed, formato PDF/CSV, snapshot
  imutável, observações de revalidação, URL canônica, freshness e retirada;
- manifesto canônico versionado, staging/finalização idempotentes, digest e
  contagens dos artefatos lógicos e identidade determinística da geração
  finalizada;
- `sourceBindingSetDigest` generation-bound sem observação,
  `activationBindingSetDigest` do binding completo e vetores canônicos de ambos;
- revisão própria do journal append-only de observações, separada de
  `catalogueRevision` e da revisão transacional interna;
- `CorpusActivationRecord` e algoritmo transacional de ativação/rollback por
  construção de nova revisão, sem replay de registro histórico;
- retenção do conteúdo bruto alcançável e cleanup de órfãos;
- fixtures determinísticas.

Aceite:

- documento e índice têm versões independentes;
- conteúdo bruto permanece reabrível para rebuild e rollback autorizados;
- candidato parcial nunca é consultável e a finalização valida
  digest/contagens/readback antes da ativação;
- secrets não integram o modelo;
- geração parcial ou observação não vinculada nunca fica ativa;
- mismatch entre observação e registro/snapshot falha fechado; mudança apenas
  de `sourceObservationId` altera somente o digest/revisão de ativação;
- rollback liga observações explicitamente selecionadas, compatíveis e
  atualmente elegíveis, sem reviver freshness histórica;
- todo documento ativo integra o manifesto; origem/trust integram identidade,
  digest e citação sem formar corpora mutuamente exclusivos;
- migrations e recuperação são verificáveis;
- corpus do produto não é confundido com banco documentado.

## STATE-04 BACKEND_IMPLEMENTATION

Objetivo: implementar administração/ingestão PDF/CSV, sincronização manual
oficial, indexação, recuperação unificada, geração e API.

Entregáveis:

- Domain e Application;
- adapters autorizados;
- adapters PDF/CSV e registros allowlisted de fontes oficiais com snapshots
  governados;
- persistência;
- API versionada;
- validação de `questionLanguage`, geração em `answerLanguage` e propagação de
  `contentLanguage` nas citações;
- artefato OpenAPI v1 versionado e testes de compatibilidade;
- configuração;
- citações e recusa;
- testes unitários, arquitetura, contrato e integração.

Aceite:

- um corpus é processado ponta a ponta;
- providers não vazam para o núcleo;
- falhas são tipadas e sanitizadas;
- hard pre-filter integra o contrato do vector store e precede o top-k;
- o hard pre-filter inclui os bindings generation-bound elegíveis derivados do
  único registro de ativação resolvido pela consulta;
- geração anterior sobrevive a falha de reconstrução;
- `304`/hash idêntico compatível cria nova revisão íntegra do registro,
  preserva manifesto/geração/`catalogueRevision` e rejeita mismatch antes do
  compare-and-swap;
- falha/stale de uma fonte reduz cobertura explicitamente sem apresentar outra
  origem como substituta;
- perguntas sem evidência recusam;
- perguntas `pt-BR` e `en-GB` respondem no mesmo idioma e citações preservam
  o idioma da fonte, inclusive na recuperação cruzada;
- suíte padrão não exige serviço pago.

## STATE-05 FRONTEND_IMPLEMENTATION

Objetivo: implementar a interface web mínima.

Entregáveis:

- pergunta e resposta;
- interface localizada em `pt-BR` e `en-GB`, com seletor explícito e estado
  visual independente de `questionLanguage`;
- temas `Light` e `Dark`, com seletor explícito e estado independente de
  `interfaceLanguage` e `questionLanguage`;
- indicador de cobertura e proveniência das fontes efetivamente consultadas;
- citações;
- estados loading, vazio, erro, fonte stale/indisponível, rate limit e sem
  evidência;
- responsividade e acessibilidade;
- testes de componente e fluxo.

Aceite:

- teclado, foco, contraste e semântica adequados;
- nenhuma lógica de autorização ou acesso direto a provider no cliente;
- informação de fonte não depende apenas de cor;
- cobertura degradada é explícita e citações exibem origem, snapshot/freshness
  e localização PDF/CSV;
- mensagens pertencentes ao produto são factuais e integralmente localizadas
  no `interfaceLanguage` selecionado;
- os fluxos `pt-BR` e `en-GB` preservam teclado, foco, semântica, reflow e
  ausência de mistura de idiomas;
- `Light` e `Dark` preservam contraste, foco, hierarquia, reflow, estados e
  informação que não dependa apenas de cor;
- a matriz de quatro combinações entre `interfaceLanguage` e
  `questionLanguage` é executada nos dois temas;
- `interfaceLanguage` nunca é inferido do contrato bilíngue de consulta e não
  traduz conteúdo de citação.

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
- README factualmente atual, com ao menos um exemplo cujo comando e resultado
  tenham sido verificados no artefato integrado local/sintético e com essa
  fronteira explicitada.

Aceite:

- fluxo completo é reproduzível;
- reinicialização e persistência são conhecidas;
- erros externos não corrompem o índice ativo;
- consulta nunca faz fetch, usa somente bindings ativos e expõe a proveniência
  de cada evidência sem mistura de geração;
- nenhum secret no artefato;
- evidências não são confundidas com produção.

## STATE-07 TESTING_HOMOLOGATION

Objetivo: validar qualidade RAG, segurança, desempenho, recuperação e
experiência representativa.

Entregáveis:

- dataset e relatório de avaliação;
- matriz de idioma pergunta/evidência para `pt-BR` e `en-GB`, nos pares iguais
  e nas duas direções cruzadas;
- estratos adicionais por tag BCP 47 documental exata, sem inferir `en` como
  `en-GB` ou fundir resultados;
- quando implementada e elegível, evidência de direitos, render manifest,
  serving e acessibilidade dos PNGs de páginas citadas;
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
- respostas usam o idioma declarado da pergunta e textos de citação mantêm o
  idioma original em todos os quatro pares da matriz;
- cada idioma documental adicional é reportado separadamente para as duas
  linguagens de pergunta suportadas, sem substituir os quatro pares
  obrigatórios;
- evidência visual, quando fizer parte da candidata, deriva somente de citação
  validada, mantém alternativa textual acessível e falha fechada diante de
  manifesto, binding, rights ou lifecycle incompatível;
- limitações e custos são explícitos;
- nenhuma vulnerabilidade bloqueadora;
- fonte oficial real é testada somente quando egress específico foi autorizado;
- afirmações públicas correspondem à matriz testada;
- Human Gate repete amostras críticas.

## Refinamentos arquiteturais aceitos durante STATE-07

Os ADRs 0008, 0009 e 0010 foram aceitos depois do encerramento dos estados donos
das implementações originais. A reconciliação documental não reabre nem
reescreve evidência histórica e não declara uma capacidade implementada. Cada
item exige autoridade corretiva própria. A ordem de dependência e o estado
factual vigente são:

1. `S03-CORR-01`, concluído: modelo lógico/físico compatível para idiomas,
   `DocumentPageImage`, `DocumentRenderManifest` e reachability, sem inferência
   de dados;
2. owner corretivo de `STATE-04`, preservando v1:
   - `S04-CORR-04-A`, concluído: content store permanente e readback verificado;
   - `S04-CORR-04-B`, concluído: contratos e gates de direitos;
   - `S04-CORR-04-C`, concluído: renderização determinística e finalização de
     fonte/PNGs/manifest;
   - `S04-CORR-04-D`, concluído: persistência e ativação atômica de fonte,
     direitos, geração e manifest; e
   - `S04-CORR-04-E`, concluído na fronteira local/offline: contrato persistente
     `AnswerEvidenceRecordV1`, retenção fixa `P30D` e participação em
     reachability, com Automatic Quality Gate corretivo aprovado, ainda sem
     homologação de produto;
3. contrato v2 separadamente versionado e apresentação same-origin segura e
   acessível de evidência visual, implementados e com Automatic Quality Gate
   aprovado;
4. integração, restart, cold backup/restore confinado e limites, implementados
   e verificados focalmente no commit
   `e5dae7ee5a786417fba2c6ef0555686816b0b330`, com Automatic Quality Gate
   aprovado sob `AUTH-STATE07-V2-INTEGRATION-RECOVERY-AQG-RETEST-001`, sem
   novo achado, e `AQG-S07-V2-IR-001` `RESOLVIDO`; e
5. dataset/homologação estratificados por idioma documental exato e pelas
   capacidades realmente implementadas, posteriores e não autorizados.

Essas responsabilidades pertencem aos owners técnicos nomeados de `STATE-03` a
`STATE-07`, mas não promovem, retrocedem ou encerram estado por si sós.
`STATE-07` permanece ativo; enquanto a implementação/evidência correspondente
faltar, a claim permanece ausente. OpenAPI v1 conserva byte a byte a superfície
fechada `pt-BR|en-GB`; contrato/serving v2 estão implementados e possuem
Automatic Quality Gate aprovado; integração, restart, cold backup/restore
confinado e limites estão implementados e verificados focalmente; seu
Automatic Quality Gate foi aprovado sob
`AUTH-STATE07-V2-INTEGRATION-RECOVERY-AQG-RETEST-001`, sem novo achado, e
`AQG-S07-V2-IR-001` está `RESOLVIDO`; dataset e homologação de produto
continuam posteriores, `NOT_RUN` e não autorizados.

## STATE-08 PRODUCTION_RELEASE

Objetivo: publicar a entrega do RAG-Challenge no alvo OCI autorizado.

Entregáveis:

- release candidate identificável;
- configuração e secrets externos;
- `OFFICIAL_SOURCE_EGRESS` restrito ao conjunto exato de URLs ativas e
  `OCI_RUNTIME_EGRESS` composto somente pelos destinos separadamente
  autorizados;
- `VECTOR_STORE_EGRESS` vazio para adapter local ou restrito ao serviço
  gerenciado aprovado;
- deploy e smoke test;
- health e observabilidade;
- rollback;
- link/captura de execução;
- README público final, complementando ou substituindo os exemplos
  locais/sintéticos com comandos e evidência separadamente verificados em OCI
  e na execução real do produto, e submissão GitHub.

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
