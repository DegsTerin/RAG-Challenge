# Roadmap e Backlog do MVP

## Objetivo

Entregar um agente RAG independente, reproduzível localmente e implantado em
OCI, que responda sobre um PDF local publicável ou um PDF oficial sincronizado
online, com escopo explícito, citações e recusa segura.

Este documento planeja trabalho; não autoriza entrada em estado, código,
consumo externo ou deploy.

## Definição do MVP

Incluído:

- um corpus lógico;
- um PDF local;
- um PDF oficial obtido de uma URL HTTPS allowlisted;
- sincronização manual para snapshot versionado;
- seleção `Local`/`OfficialOnline` sem mistura ou fallback silencioso;
- catálogo e versões;
- armazenamento imutável e reabrível do conteúdo bruto;
- parsing e chunking versionados;
- um embedding provider;
- um vector store;
- um LLM;
- geração imutável de índice;
- consulta com citações e `INSUFFICIENT_EVIDENCE`;
- API, artefato OpenAPI v1 versionado e interface web mínima;
- execução local;
- CI;
- deploy OCI e evidência;
- documentação e testes.

Não incluído:

- todos os formatos;
- múltiplos acervos ativos;
- sincronização incremental/agendada ou mais de uma fonte oficial;
- crawling, HTML genérico ou URL fornecida pelo usuário;
- vários providers em produção;
- autenticação corporativa;
- microserviços;
- integração executável ao DB-Notifier.

## Roadmap por estado

| Estado | Resultado incremental | Condição de saída |
|---|---|---|
| `STATE-00` | Discovery, requisitos, arquitetura proposta, riscos, backlog e governança. | Quality Gate documental e Human Gate explícito. |
| `GATE-B01` | ADR-0001, licença do repositório e mapa físico de projetos decididos. | Decisão humana registrada; nenhuma implementação autorizada. |
| `STATE-01` | Repositório e scaffold reproduzível sem lógica RAG, conforme bootstrap aceito. | Clone limpo compila/testa e CI estrutural passa. |
| `STATE-02` | ADR-0002, providers, corpus/licença, URL oficial/termos, threat model e OCI decididos. | Decisões aceitas e riscos críticos tratados. |
| `STATE-03` | Modelo de documento/snapshot/índice, freshness, source scope, migrations e rollback. | Geração/ativação e isolamento são verificáveis sem serviço produtivo. |
| `STATE-04` | Ingestão local, sync oficial, pipeline RAG e API funcionais localmente. | Perguntas por escopo e falhas passam testes. |
| `STATE-05` | Interface mínima com seletor e freshness. | Fluxos Local/OfficialOnline e citações validados humanamente. |
| `STATE-06` | E2E offline, artefato e smoke online autorizado. | Execução reproduzível sem corrupção, leak ou secret. |
| `STATE-07` | Homologação RAG, SSRF, isolamento, carga, recuperação e acessibilidade. | Thresholds prévios atendidos e riscos aceitos. |
| `STATE-08` | Deploy OCI, egress oficial, smoke, evidência e README final. | Entrega pública atende aos critérios oficiais. |

## Etapas pequenas de desenvolvimento

### Lote S00-DOC — Baseline documental

- Criar os 20 arquivos originalmente aprovados. Documentos normativos
  acrescentados depois são incrementos versionados e não reescrevem esse
  escopo histórico.
- Validar links, formato, escopo e separação de autoridade.
- Reconciliar a promoção da fonte oficial online ao MVP.
- Produzir relatório automático.
- Solicitar Human Gate somente do `STATE-00`.

### GATE-B01 — Decisão de bootstrap

- Aceitar ou rejeitar explicitamente o ADR-0001.
- Escolher a licença do repositório, separada da licença do corpus.
- Confirmar se cada assembly candidato possui responsabilidade ou boundary de
  dependência/teste suficiente; registrar o mapa físico aprovado.
- Mapear `CH-MOD-*` para namespaces/pastas/projetos, dependências permitidas e
  testes arquiteturais.
- Escolher host principal em modo one-shot ou justificar um projeto
  administrativo separado, sem ainda definir identidade/permissões.
- Consolidar mapa, módulos e forma administrativa no ADR-0001 aceito; registrar
  decisão/licença/evidência em entrada append-only do State Transition Log e
  atualizar o Current State apenas como snapshot.

Critério: decisão humana registrada. Este gate não inicializa Git, não cria
scaffold, não aceita o ADR-0002 e não autoriza `STATE-01`.

### Lote S01-A — Fundação do repositório

- Inicializar Git somente após autorização.
- Aplicar a licença de repositório já decidida, `.editorconfig` e
  `.gitattributes`.
- Completar `.gitignore`.
- Fixar as toolchains aceitas no ADR-0001.
- Criar gestão central de pacotes e lockfiles.

Critério: nenhuma lógica funcional e nenhum secret.

### Lote S01-B — Fronteiras vazias

- Criar solution e projetos.
- Adicionar referências na direção aprovada.
- Criar testes de arquitetura.
- Adicionar hosts mínimos e health sem dependências externas.

Critério: restore, build, format e testes locais aprovados.

### Lote S01-C — CI inicial

- Build/test/format.
- Cobertura estrutural.
- Dashboard lint/type/test/build.
- Dependency e secret scans.
- Links Markdown e higiene do diff.

Critério: pipeline localmente reproduzível; CI não faz deploy.

### Lote S02-A — Decisões bloqueadoras

- Aceitar/rejeitar ADR-0002 e decisões adicionais necessárias.
- Escolher a licença do corpus; não reabrir silenciosamente a licença do
  repositório.
- Congelar escopo do PDF.
- Escolher uma URL PDF oficial, termos/licença, maxAge e limites.
- Selecionar parser, embeddings, vector store e LLM.
- Definir persistência durável e retenção de conteúdo bruto, catálogo e
  índice, incluindo restart e armazenamento OCI.
- Se o vector store for externo, definir e autorizar separadamente seu egress
  e tratamento de dados; manter adapter local como alternativa simples.
- Selecionar serviço/região OCI.

Critério: cada escolha tem alternativa, consequência e owner.

### Lote S02-B — Contratos e segurança

- Especificar entidades, ports, `VectorSearchRequest`,
  `IDocumentContentStore`, `CorpusActivationRecord` e resultados.
- Detalhar threat model e as políticas separadas
  `AI_PROVIDER_EGRESS`, `VECTOR_STORE_EGRESS`, `OFFICIAL_SOURCE_EGRESS` e
  `OCI_RUNTIME_EGRESS`.
- Definir canonicalização e pinning DNS/IP por conexão, Host/SNI e redirects
  desativados para a fonte oficial.
- Exigir fonte pública sem credenciais e decidir trust, revogação, downloads de
  cadeia e eventual provisão de material TLS sem egress auxiliar não
  autorizado, com prova prevista em clone local limpo e OCI.
- Escolher a superfície administrativa local não pública, sua identidade,
  permissões, idempotência, motivo obrigatório e auditoria.
- Definir configuração, tabela canônica de erros, readiness global/por scope,
  logging e auditoria.
- Definir ownership, schemas, metadados e política de compatibilidade do
  OpenAPI v1.
- Definir dataset, rubrica e thresholds antes da execução.
- Definir source scope, ausência de fallback e autorização dos testes reais.

Critério: implementação pode começar sem decisão material em aberto.

### Lote S03-A — Modelo de catálogo

- Modelar corpus, documento, versão e proveniência.
- Modelar snapshot oficial imutável, observações de revalidação, freshness e
  `SourceScope`.
- Modelar especificação e manifesto final canônicos, staging não consultável,
  digest/contagens dos artefatos lógicos, identidade determinística da geração
  finalizada e separação entre snapshot selecionado e freshness.
- Modelar estados de build e as projeções `Active`/`Retained` derivadas do
  `CorpusActivationRecord` e de seu histórico completo.
- Definir constraints, índices, UTC e concorrência.

Critério: modelo não contém secret nem SDK/provider type.

### Lote S03-B — Persistência e rollback

- Criar migrations não produtivas.
- Testar create/upgrade/failure/rollback.
- Provar compare-and-swap atômico do registro completo de geração, snapshot,
  observação e auditoria.
- Preservar e reabrir bytes content-addressed alcançáveis; limpar somente
  órfãos comprovados após retenção.
- Preservar a geração ativa e ao menos uma geração anterior validada até
  cleanup explícito após a janela de rollback aprovada.

Critério: falha preserva geração anterior e o retorno ativação → geração
anterior é testado.

### Lote S04-A — Ingestão local e sincronização oficial

- Validar arquivo e raiz.
- Validar URL allowlisted e sincronizar manualmente o PDF oficial para
  snapshot.
- Persistir e reabrir por hash os bytes locais/oficiais antes de ativar e
  registrar status e validators HTTP enviados/recebidos em cada observação.
- Em `304` ou hash idêntico, registrar nova observação de revalidação sem
  criar snapshot ou índice somente se o registro ativo já referenciar o
  snapshot compatível; caso contrário, reconstruir de forma controlada.
- Extrair ambos os PDFs pelo mesmo parser.
- Normalizar e produzir chunks de forma determinística.
- Persistir bytes brutos, metadados e hashes de forma idempotente.

Critério: fixtures local/HTTP geram chunks rastreáveis; falha de sync preserva
o snapshot e a geração ativos.

### Lote S04-B — Indexação

- Integrar embedding provider.
- Construir staging inativo por `candidateBuildId`.
- Incluir `SourceScope` em identidade, digest e metadados vetoriais.
- Exigir `CorpusId`, `IndexGenerationId` e `SourceScope` no contrato de busca
  e provar hard pre-filter dos três seletores ou partição física equivalente.
- Finalizar digest/contagens/readback, derivar `IndexGenerationId`, validar o
  manifesto final e ativar.
- Reexecutar idempotentemente sem promover candidato parcial.

Critério: conteúdo idêntico não cria inconsistência; falha não substitui ativo.

### Lote S04-C — Recuperação e resposta

- Validar pergunta.
- Exigir `Local` ou `OfficialOnline` e aplicar pre-filter antes do top-k.
- Recuperar somente evidências do escopo escolhido.
- Gerar resposta constrained.
- Validar citações.
- Retornar evidência insuficiente.

Critério: testes cobrem os dois escopos, sem resposta, stale, indisponível,
source leakage, provider down e injection.

### Lote S04-D — API

- Implementar `/api/v1/questions`.
- Exigir `sourceScope`; rejeitar URL/domínio/adapter no payload.
- Implementar liveness/readiness.
- Mapear a taxonomia canônica para códigos `CH_*` e Problem Details.
- Gerar e versionar o artefato OpenAPI v1 com schemas de consulta, resposta,
  citações e falhas.
- Incluir metadados não secretos de política, prompt e modelo e executar teste
  de compatibilidade/breaking change.
- Aplicar limites, timeout, cancelamento e rate limit.

Critério: API não expõe secret, stack trace ou conteúdo indevido.

### Lote S05-A — Interface mínima

- Formulário de pergunta.
- Seletor `Local`/`Documentação oficial online — snapshot sincronizado`.
- Resposta e lista de citações.
- URL/snapshot/freshness nas citações oficiais.
- Loading, vazio, erro, stale, indisponível e sem evidência.
- Texto puro por padrão; qualquer Markdown usa subconjunto sanitizado, sem
  HTML cru, com schemes de URL permitidos e CSP.

Critério: fluxo funciona por teclado e viewport reduzido.

### Lote S06-A — E2E e artefato

- Executar documento → índice → pergunta → resposta.
- Executar sync por servidor HTTP falso; smoke real somente quando autorizado.
- Validar restart/persistência de conteúdo bruto, catálogo, ativação e índice.
- Produzir artefato reproduzível.
- Preparar configuração de ambiente sem secret.

Critério: clone limpo reproduz o caminho documentado.

### Lote S07-A — Avaliação e segurança

- Executar dataset congelado.
- Medir recuperação, groundedness, citações, latência e custo.
- Testar prompt injection, abuso, rate limit e falhas.
- Testar SSRF, DNS rebinding, respostas mistas, pinning IP/Host/SNI, redirect,
  path, media type, bytes descomprimidos, autenticação recusada, ausência de
  egress AIA/CRL/OCSP, stale e isolamento.
- Testar crash em cada fronteira de ativação, rollback e acessibilidade.

Critério: thresholds prévios e nenhum P0/P1 residual.

### Lote S08-A — Deploy OCI

- Autorizar alvo e custos.
- Restringir `OFFICIAL_SOURCE_EGRESS` à URL oficial exata e compor
  `OCI_RUNTIME_EGRESS` somente com destinos separadamente autorizados.
- Manter `VECTOR_STORE_EGRESS` vazio para adapter local ou validar sua
  allowlist específica quando houver serviço gerenciado.
- Provisionar/configurar secret.
- Publicar artefato.
- Executar smoke e health.
- Ensaiar recuperação.

Critério: aplicação pública funcional e identificável.

### Lote S08-B — Evidência e entrega

- Registrar link/captura sanitizada.
- Atualizar README com comandos e exemplos reais.
- Conferir histórico, licença e material versionado.
- Submeter URL GitHub segundo as regras do Challenge.

Critério: checklist oficial completo.

## Backlog priorizado

### Must — obrigatório para o MVP

| ID | Item | Estado proprietário |
|---|---|---|
| `BL-M01` | Definir e licenciar o PDF `Catálogo de Bancos de Dados — MVP`. | S02 |
| `BL-M02` | Scaffold .NET 10 modular e CI. | S01 |
| `BL-M03` | Catálogo, conteúdo bruto reabrível, documento, manifesto e índice versionados. | S03 |
| `BL-M04` | Ingestão PDF local segura. | S04 |
| `BL-M05` | Embeddings e geração imutável de índice. | S04 |
| `BL-M06` | Recuperação, resposta grounded e citações. | S04 |
| `BL-M07` | Resultado de evidência insuficiente. | S04 |
| `BL-M08` | API com limites, health, erros sanitizados e artefato OpenAPI v1 versionado/testado. | S04 |
| `BL-M09` | Interface web mínima e acessível. | S05 |
| `BL-M10` | Testes e avaliação RAG. | S04/S07 |
| `BL-M11` | Execução local reproduzível. | S06 |
| `BL-M12` | Deploy em OCI e evidência. | S08 |
| `BL-M13` | README final com exemplos reais. | S08 |
| `BL-M14` | Preservar uma geração anterior elegível e testar ativação/rollback atômicos do `CorpusActivationRecord` por compare-and-swap. | S03/S04/S07 |
| `BL-M15` | Sincronizar um PDF oficial allowlisted com pinning DNS/IP e consultar por `OfficialOnline` com snapshot, freshness e isolamento. | S02–S08 |

### Should — se não comprometer a entrega

| ID | Item | Observação |
|---|---|---|
| `BL-S02` | Cache seguro de embeddings por hash. | Somente após medir benefício. |
| `BL-S03` | Métricas de custo/tokens. | Se provider expuser dados seguros. |
| `BL-S04` | Interface de diagnóstico do corpus. | Read-only e sanitizada. |

### Could — evolução

| ID | Item |
|---|---|
| `BL-C01` | CSV, Markdown, HTML e formatos Office (`RF-018`). |
| `BL-C02` | Múltiplos acervos e ativação individual. |
| `BL-C03` | Sincronização incremental e scheduler. |
| `BL-C04` | Múltiplas fontes oficiais, HTML/crawling e sincronização agendada. |
| `BL-C05` | Mais providers de embeddings, vetor e LLM. |
| `BL-C06` | RBAC e escopo por corpus (`RF-019`). |
| `BL-C07` | Frontend estático opcional no GitHub Pages. |
| `BL-C08` | Adapter consumidor pertencente ao DB-Notifier, sob ADR e gates do repositório consumidor; o Challenge fornece somente OpenAPI versionado. |

### Won't — não neste Challenge

| ID | Item |
|---|---|
| `BL-W01` | Cobertura literal de todos os bancos de dados conhecidos. |
| `BL-W02` | Microserviços e orquestração distribuída sem necessidade medida. |
| `BL-W03` | Execução de SQL ou administração de banco pelo agente. |
| `BL-W04` | Navegação livre na web durante perguntas. |
| `BL-W05` | Dependência direta do repositório DB-Notifier. |

## Riscos do roadmap

- O corpus e sua licença são o primeiro bloqueio material.
- A URL oficial, seus termos/licença e estabilidade são bloqueios próprios.
- Egress/SSRF e freshness exigem testes sem tornar a suíte padrão dependente
  da internet.
- Provider externo pode exigir conta, quota, região e custo.
- Escolha tardia de dimensão/vector store pode forçar reindexação.
- Vector store gerenciado pode expor chunks/embeddings sem política própria.
- GitHub Pages pode ser confundido com backend; a documentação deve manter a
  separação.
- Suporte prematuro a muitos formatos ameaça o prazo do MVP.
- Avaliação sem dataset congelado pode produzir sucesso não reproduzível.

## Regra de progressão

Concluir um lote não autoriza o seguinte. Cada estado precisa dos gates
descritos em
[`Quality-Gates.md`](../prompts/governance/Quality-Gates.md), e toda ação
externa conserva autorização própria.
