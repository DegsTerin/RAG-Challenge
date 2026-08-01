# Arquitetura da Solução RAG-Challenge

## Status

Baseline proposta no `STATE-00 DISCOVERY`. Os detalhes que exigem decisão
formal estão nos ADRs em
[`../../docs/architecture/`](../../docs/architecture/README.md). Proposta
documental não representa implementação, teste, deploy ou homologação.

## Princípios

- Monólito modular proporcional ao MVP.
- Dependências voltadas para dentro.
- Domínio independente de IA, persistência, UI e infraestrutura.
- Portas tipadas para fontes, parsing, embeddings, vetores e LLM.
- Configuração fail-closed, sem segredos persistidos.
- Proveniência e versão preservadas do documento à citação.
- Pergunta e resposta suportadas em `pt-BR` e `en-GB`, com idioma explícito e
  conteúdo citado preservado no idioma da fonte.
- Índices construídos de forma imutável antes da ativação.
- Falha externa isolada e explicitamente classificada.
- Origem local e oficial preservadas como proveniência e confiança, com todos
  os documentos ativos elegíveis na recuperação unificada.
- Contrato externo versionado pertencente ao RAG-Challenge; adapters
  consumidores futuros pertencem aos respectivos repositórios.

## Contexto do sistema

```text
Question author / evaluator
          |
          v
  RAG-Challenge Dashboard
          |
       HTTPS
          |
          v
    RAG-Challenge Server/API
          |
          v
 Application use cases
   |       |        |        |
   |       |        |        +--> language-model adapter
   |       |        +-----------> vector-store adapter
   |       +--------------------> document/embedding adapters
   +----------------------------> governed local/official source adapters
```

No MVP, o servidor pode hospedar a API e os arquivos estáticos do Dashboard no
mesmo deploy. Isso reduz operação sem acoplar a interface aos casos de uso.

## Direção de dependências

```text
RagChallenge.Domain
        ^
        |
RagChallenge.Rag.Abstractions
        ^
        |
RagChallenge.Application
        ^
        |
Infrastructure / Persistence / API

RagChallenge.Dashboard.Web -- versioned HTTP --> RagChallenge.Server.Api
```

- `RagChallenge.Domain` possui identidades, versões, estados e invariantes.
- `RagChallenge.Rag.Abstractions` possui contratos RAG e depende apenas da
  semântica canônica necessária.
- `RagChallenge.Application` implementa casos de uso e orquestra portas.
- Infrastructure e Persistence implementam adapters.
- API é composition root e não contém regras de negócio.
- Dashboard não possui referência de código ao Application; consome somente
  contratos HTTP versionados.

## Módulos canônicos

| ID | Módulo | Responsabilidade |
|---|---|---|
| `CH-MOD-01` | `CORPUS_CATALOG` | Identidade, categorias muitos-para-muitos, versões e estado de bancos/documentos. |
| `CH-MOD-02` | `DOCUMENT_INGESTION` | Descoberta, validação, parsing PDF/CSV e normalização. |
| `CH-MOD-03` | `INDEXING_RETRIEVAL` | Chunking, embeddings, gerações e recuperação. |
| `CH-MOD-04` | `ANSWER_GENERATION` | Contexto grounded, resposta, citações e recusa. |
| `CH-MOD-05` | `QUERY_EXPERIENCE` | API e interface de consulta. |
| `CH-MOD-06` | `OPERATIONS_GOVERNANCE` | Configuração, health, logs, auditoria e gates. |
| `CH-MOD-07` | `OFFICIAL_EXTERNAL_SOURCES` | Registro e sincronização manual governada de fontes oficiais compatíveis. |
| `CH-MOD-08` | `EXTERNAL_INTEGRATION_CONTRACTS` | Contrato HTTP/OpenAPI versionado do RAG-Challenge; adapters consumidores ficam fora deste repositório. |

IDs não devem ser reutilizados com outro significado.
`CH-MOD-08` conserva a fronteira de integração da baseline anterior, mas o
rótulo foi corrigido antes do Human Gate para deixar explícito que o
RAG-Challenge é owner do contrato e não do adapter consumidor.

## Estrutura candidata para o `STATE-01`

Esta árvore não é autoridade de scaffold. Ela só poderá orientar `STATE-01`
se o ADR-0001 for aceito no `GATE-B01`, o mapa físico de projetos for
registrado e a entrada no estado receber autorização separada.

```text
/
├── AGENTS.md
├── README.md
├── RAG-Challenge.sln
├── Directory.Build.props
├── Directory.Packages.props
├── global.json
├── NuGet.config
├── .editorconfig
├── .gitattributes
├── .gitignore
├── docs/
├── prompts/
├── src/
│   ├── RagChallenge.Domain/
│   ├── RagChallenge.Rag.Abstractions/
│   ├── RagChallenge.Application/
│   ├── RagChallenge.Infrastructure/
│   ├── RagChallenge.Persistence.Sqlite/
│   ├── RagChallenge.Server.Api/
│   └── RagChallenge.Dashboard.Web/
└── tests/
    ├── RagChallenge.UnitTests/
    ├── RagChallenge.Architecture.Tests/
    └── RagChallenge.IntegrationTests/
```

Se autorizada, a fase cria apenas scaffold, configuração, checks e hosts
mínimos. Não implementa ingestão, recuperação ou geração. Cada assembly deve
possuir responsabilidade e boundary de dependência ou teste distintos. No
`GATE-B01`, `RagChallenge.Rag.Abstractions` poderá ser combinado com
`RagChallenge.Application`, e `RagChallenge.Persistence.Sqlite` com
`RagChallenge.Infrastructure`, se a separação física não se justificar. As
fronteiras conceituais permanecem mesmo quando dois papéis compartilham um
assembly. O mesmo gate registra o mapa
`CH-MOD-* → namespace/pasta/projeto`, dependências permitidas e testes de
arquitetura. Também decide se a administração one-shot usa um modo explícito
do host principal ou justifica um projeto `RagChallenge.Tools.Admin`; a escolha
da identidade e das permissões continua pertencendo ao `STATE-02`.

## Convenções de nomenclatura

- Nome público e arquivo da solution: `RAG-Challenge` e
  `RAG-Challenge.sln`.
- Prefixo técnico de projeto, assembly e namespace: `RagChallenge`.
- Projetos: `RagChallenge.<Responsibility>`.
- Testes: `RagChallenge.<TestScope>Tests`.
- Tipos e membros C#: PascalCase; variáveis e parâmetros: camelCase.
- API inicial: `/api/v1`.
- Configuração: seções `RagChallenge:<Capability>`.
- IDs de erro: `CH_<AREA>_<CONDITION>`, estáveis e sem detalhe secreto; o
  prefixo permanece por compatibilidade após a renomeação do produto.
- IDs de corpus: slug estável em minúsculas; nomes de exibição não são
  identificadores.
- Timestamps de contrato: UTC em ISO 8601.
- Documentos: nomes descritivos; ADRs em inglês conforme o padrão do
  DB-Notifier.

As convenções de idioma pertencem à
[`política de idioma`](../governance/Language-Policy.md). Este documento não
as redefine.

## Componentes e responsabilidades

### Domain

Conceitos candidatos:

- `CorpusId`, `CorpusStatus` e `CorpusRevision`;
- `DatabaseProductId`, `DatabaseProductRevision`, `DatabaseProductStatus` e
  `DatabaseCategoryAssignment`;
- `DocumentId`, `DocumentVersion`, `DocumentStatus`, `DocumentFormat` e
  `SourceDescriptor`;
- `ContentObjectId` e referência imutável ao conteúdo bruto;
- `SourceTrustClass`, `OfficialSourceRegistration`, `OfficialSourceSnapshot`,
  `OfficialSourceObservation` e `SourceFreshness`;
- `ChunkIdentity` e `Citation`;
- `CandidateBuildId`, `IndexGenerationId`, `IndexGenerationStatus` e
  `CorpusActivationRecord`;
- `ProviderDescriptor`;
- `SupportedLanguage`, restrito a `pt-BR` e `en-GB` no MVP;
- `QueryRequest`, `RetrievedEvidence` e `AnswerOutcome`.

O Domain não conhece caminhos de arquivo, PDF, SQL, HTTP, SDKs ou modelos.

### RAG abstractions

Portas candidatas:

- `IDocumentSource`;
- `IOfficialSourceSynchroniser`;
- `IDocumentParser`;
- `IChunkingStrategy`;
- `IEmbeddingProvider`;
- `IVectorStore`;
- `ILanguageModel`;
- `IDocumentContentStore`;
- `IDocumentCatalog`;
- `IIndexGenerationStore`;
- `IClock` ou `TimeProvider` na borda apropriada.

Contratos carregam `CancellationToken`, limites e resultados tipados.
`IVectorStore` recebe um `VectorSearchRequest` com `CorpusId`,
`IndexGenerationId`, vetor de consulta, limites e filtros administrativos
opcionais por banco/documento. O adapter prova hard pre-filter de corpus,
geração e filtros antes do top-k ou usa partição física equivalente; post-filter
de busca global não satisfaz o contrato. Ele não possui autoridade de ativação.

`IDocumentContentStore` persiste e reabre bytes imutáveis endereçados por
conteúdo. `IDocumentCatalog` mantém identidades e referências, e
`IIndexGenerationStore` é a única fonte de verdade do
`CorpusActivationRecord`, que vincula atomicamente a geração ativa e um
conjunto ordenado de bindings de banco, documento, versão, snapshot e observação
aplicável. Revisões completas anterior e nova ficam no histórico versionado da
mesma transação; `Active` e `Retained` são projeções, não autoridades paralelas.

### Application

Casos de uso candidatos:

- `BuildCorpusIndex`;
- `ActivateIndexGeneration`;
- `RollbackIndexGeneration`;
- `RegisterDatabaseProduct`, `VersionDatabaseProduct`,
  `ActivateDatabaseProduct`, `DeactivateDatabaseProduct` e
  `RemoveDatabaseProduct`;
- `RegisterDocument`, `VersionDocument`, `ActivateDocument`,
  `DeactivateDocument` e `RemoveDocument`;
- `SynchroniseOfficialSource`;
- `AskQuestion`;
- `GetSystemReadiness`.

Atualização incremental agendada e gestão de múltiplos acervos não integram os
casos de uso do MVP. Bancos, documentos e fontes compatíveis são registros
administráveis; sincronização oficial é manual, limitada às URLs allowlisted e
não ocorre no fluxo de pergunta.

### Infrastructure e Persistence

- Um adapter concreto por porta no MVP.
- SQLite candidato apenas para catálogo, metadados e histórico local; a
  decisão final pertence ao `STATE-02`.
- O armazenamento bruto é durável, content-addressed e separado do catálogo;
  filesystem local é candidato para o MVP, com caminho durável equivalente no
  alvo OCI. Vector store não substitui os bytes necessários a rebuild,
  retenção ou rollback.
- O armazenamento vetorial permanece atrás de `IVectorStore`.
- Vector store gerenciado exige política de egress e tratamento de dados
  próprios; escolher implementação local evita esse egress no MVP.
- EF Core ou detalhes de schema não vazam para Domain/Application.
- A fonte local aplica raiz configurada, canonicalização de caminho e limites.
- Cada fonte oficial aplica URL PDF/CSV pública sem credenciais, allowlist
  completa, proteção SSRF, pinning DNS/IP/Host/SNI, política TLS sem egress
  lateral, limites e snapshot antes de usar o parser do formato declarado.
- Chamadas externas usam cliente tipado, timeout, retry somente quando seguro
  e sanitização.

### Server/API

Contratos conceituais, ainda não implementados:

| Método e rota | Uso |
|---|---|
| `POST /api/v1/questions` | Enviar pergunta e receber resposta/citações. |
| `GET /api/v1/health/live` | Confirmar que o processo está vivo. |
| `GET /api/v1/health/ready` | Informar prontidão e dependências sem segredos. |

Ingestão, sincronização oficial, ativação e rollback não serão endpoints
públicos anônimos no MVP. Poderão ser executados somente por uma superfície
local não pública escolhida no `STATE-02`. A operação identifica o operador ou
ambiente por identidade do sistema operacional, usa permissões mínimas, exige
motivo, é idempotente e gera auditoria sanitizada. O startup apenas carrega e
verifica a geração ativa; não ingere, sincroniza, ativa nem executa rollback,
salvo em modo administrativo one-shot explicitamente configurado e invocado.

O contrato público inicial para consumidores externos é HTTP/OpenAPI v1,
pertence ao RAG-Challenge e não expõe entidades Domain nem portas de provider.
Conceitualmente:

```text
QueryRequestV1
  corpusId
  questionLanguage: pt-BR | en-GB
  question

QueryResponseV1
  outcome: Answered | InsufficientEvidence
  answerLanguage: pt-BR | en-GB
  answer?
  citations[]
  evidenceCoverage
  indexGenerationId
  retrievalPolicyVersion
  promptVersion
  languageModelDescriptor
  correlationId
```

Cada citação preserva corpus, banco, documento, versão, formato, classe de
confiança, geração e localização. Citação oficial inclui URL canônica,
snapshot, `revalidatedAt` e freshness; PDF usa páginas/blocos e CSV usa
linhas/colunas/cabeçalhos.
Toda citação declara `contentLanguage=pt-BR|en-GB`; títulos, seções e trechos
derivados da fonte permanecem nesse idioma original. `answerLanguage` é sempre
igual ao `questionLanguage` aceito, inclusive quando a evidência usa o outro
idioma.
`languageModelDescriptor` contém apenas provider, modelo e revisão não
secretos; não contém endpoint, credencial ou configuração interna.

O artefato OpenAPI v1 pertence ao RAG-Challenge, é gerado e versionado com a API,
inclui pergunta, respostas concluídas, citações e Problem Details, e passa por
teste de compatibilidade. A política de breaking changes pertence ao
`STATE-02`; a implementação e a prova do artefato pertencem ao `STATE-04`.

Os campos de idioma pertencem ao contrato de consulta e não selecionam o
idioma visual. O Dashboard suporta separadamente `interfaceLanguage=pt-BR` ou
`interfaceLanguage=en-GB`.

O tema também pertence ao estado local do Dashboard, com `Light` e `Dark`
como valores suportados. Ele não integra o contrato público de consulta e não
altera idioma, conteúdo, escopo, resposta, evidência ou citação.

`QueryResponseV1` representa apenas uma consulta concluída com `Answered` ou
`InsufficientEvidence`. `evidenceCoverage` identifica o conjunto ativo
consultado e eventuais fontes degradadas sem substituir evidência
silenciosamente. Entrada inválida, ausência de qualquer conjunto servível,
violação de política, rate limit, indisponibilidade de provider e falha interna
são resultados tipados da Application mapeados pela API para Problem Details
não `2xx`, com código estável e sem detalhes sensíveis.

### Dashboard

- Interface mínima e responsiva.
- Seletor explícito de `interfaceLanguage` entre `pt-BR` e `en-GB`, sem
  inferência a partir de `questionLanguage` ou `answerLanguage`.
- Labels, instruções, validações, estados e erros pertencentes ao produto
  integralmente localizados no idioma visual selecionado; citações preservam
  `contentLanguage`.
- Seletor explícito de tema entre `Light` e `Dark`, independente de
  `interfaceLanguage`, `questionLanguage` e do conteúdo consultado.
- Tokens visuais de fundo, superfície, texto, borda, foco e estado
  preservam contraste, hierarquia e informação não dependente apenas de cor
  nos dois temas.
- Indicador acessível de cobertura e proveniência local/oficial das evidências,
  sem criar corpora mutuamente exclusivos.
- Estados de carregamento, vazio, erro, rate limit, cobertura degradada, fonte
  indisponível/stale e evidência insuficiente.
- Navegação por teclado e foco visível.
- Citações acessíveis e separadas da resposta.
- Nenhum acesso direto ao vetor, ao LLM ou a secrets.
- Seleção inicial, persistência e fallback de `interfaceLanguage`, assim como
  tema inicial, preferência do sistema, persistência e fallback de tema,
  pertencem ao `STATE-05` e permanecem sem decisão.
- Saída é texto puro por padrão. Se Markdown for autorizado, usa subconjunto
  sanitizado, bloqueia HTML cru, permite somente schemes de URL aprovados e
  opera sob Content Security Policy.

## Fluxo de indexação

```text
configured logical corpus and catalogue
  -> resolve Candidate database/document versions and retained source snapshots
  -> validate and hash
  -> persist content by hash and reopen/verify
  -> parse
  -> chunk with versioned strategy
  -> embed
  -> write under temporary candidate build identity
  -> finalise canonical manifest with logical artifact digest/counts
  -> validate readback, manifest and smoke queries
  -> compare-and-swap complete CorpusActivationRecord in IIndexGenerationStore
```

Cada binding oficial selecionado para a candidata preserva snapshot e
observação. Somente bindings `Current` e itens `Active` participam do conjunto
consultável. Falha antes ou durante o compare-and-swap mantém geração e
bindings anteriores. Vetores, conteúdo ou observações candidatos que não forem
ativados são órfãos auditáveis até cleanup explícito. A estratégia detalhada
está em [`RAG-Module.md`](RAG-Module.md).

Candidato parcial nunca possui `IndexGenerationId` nem é consultável. A
identidade final deriva da especificação e do digest/contagens dos artefatos
lógicos produzidos; outputs distintos não reutilizam silenciosamente o mesmo
ID. Staging, finalização idempotente e evidência mínima de readback pertencem
ao `STATE-03`.

A candidata contém um conjunto ordenado de todos os bancos e documentos que se
pretende ativar. Atualizações são serializadas por corpus; o vector store deve
filtrar `CorpusId`, `IndexGenerationId` e quaisquer filtros administrativos
declarados antes do top-k. Rollback troca o manifesto inteiro. Desativar ou
remover o último documento ativo exige desativar explicitamente o banco na
mesma operação atômica.

## Fluxo de consulta

```text
question
  -> validate and bound
  -> validate pt-BR | en-GB question language
  -> resolve active generation once
  -> resolve all active/current document bindings and coverage
  -> embed query
  -> retrieve top candidates across all active documents by explicit generation ID
  -> apply score/policy checks
  -> build untrusted evidence context
  -> generate constrained answer in question language
  -> validate citations
  -> answer or INSUFFICIENT_EVIDENCE
```

## Configuração

- Configuração comum em arquivos sem segredo.
- Overrides por ambiente e variáveis protegidas.
- Secrets somente por referências ou nomes de variáveis.
- Startup valida provider, modelo, dimensão, limites, catálogo, content store,
  durabilidade mínima e compatibilidade do índice. O perfil oficial valida cada
  registro de URL/allowlist, política de egress e freshness sem executar
  sincronização.
- Capacidade incompleta permanece desativada; não há fallback silencioso.
- Um `.env.example` futuro contém apenas nomes e valores fictícios.

Seções candidatas:

```text
RagChallenge:Corpus
RagChallenge:OfficialSource
RagChallenge:ContentStore
RagChallenge:Parsing
RagChallenge:Chunking
RagChallenge:Embeddings
RagChallenge:VectorStore
RagChallenge:LanguageModel
RagChallenge:Query
RagChallenge:Observability
RagChallenge:Egress
```

## Tratamento de erros

Resultados concluídos de `AnswerOutcome`:

- `Answered`;
- `InsufficientEvidence`.

Categorias canônicas iniciais de falha:

- `InvalidInput`;
- `CorpusUnavailable`;
- `UnsupportedDocument`;
- `SourceUnavailable`;
- `SourceStale`;
- `SourcePolicyViolation`;
- `ParseFailed`;
- `EmbeddingUnavailable`;
- `IndexUnavailable`;
- `LanguageModelUnavailable`;
- `RateLimited`;
- `ConfigurationInvalid`;
- `OperationCancelled`;
- `UnexpectedFailure`.

A API mapeia falhas para Problem Details sem stack trace, prompt, documento,
token ou segredo. Os dois `AnswerOutcome` permanecem respostas tipadas `2xx`;
falhas não são apresentadas como sucesso. Retry só
ocorre para falha transitória e operação idempotente. O `STATE-02` fecha uma
tabela única `ApplicationFailure → CH_* → HTTP/Problem Details`; adapters não
criam taxonomias paralelas.

## Observabilidade, logging e auditoria

- Logs estruturados com correlation ID, operation ID e códigos estáveis.
- Métricas de ingestão/sincronização: duração, bancos, documentos, formatos,
  páginas/linhas, bytes, chunks, freshness, falhas e versão.
- Métricas de consulta: latência por estágio, candidatos, recusas e falhas de
  provider.
- Custo/tokens somente quando o provider oferece metadados seguros.
- Liveness verifica somente que o processo responde e não depende de serviço
  externo.
- Readiness global exige ao menos um banco ativo com um documento ativo e uma
  geração compatível servível. Fontes `Stale`, `Unavailable`, `Withdrawn` ou
  `Deactivated` aparecem como cobertura degradada por fonte/documento; somente
  ausência de qualquer conjunto servível torna a instância globalmente
  indisponível. Egress de sincronização não integra o caminho de readiness.
- Auditoria registra configuração relevante sanitizada, início/fim de
  indexação/sincronização, snapshot, ativação, rollback e provider/version.
- Perguntas, trechos e respostas completas não são logados por padrão.

## Infraestrutura e CI/CD

O `STATE-01` deve preparar CI, não deploy automático. Pipeline mínimo:

1. checkout sem persistir credenciais;
2. toolchains e versões fixadas;
3. restore por lockfile;
4. formatação, lint e type checking;
5. build Release;
6. testes e cobertura;
7. testes de arquitetura;
8. auditoria de dependências;
9. secret scan;
10. validação de links e higiene do diff.

Testes padrão usam fontes HTTP falsas locais e não acessam URLs oficiais reais.
Smoke real é opt-in e exige autoridade de rede própria.

CD e OCI pertencem aos estados posteriores. O deploy inicial candidato é um
único artefato no serviço OCI selecionado, com configuração e secrets
externos. `OFFICIAL_SOURCE_EGRESS` fica limitado ao conjunto exato de URLs
ativas e separadamente aprovadas;
`OCI_RUNTIME_EGRESS` agrega separadamente somente os destinos aprovados de
fonte oficial, IA, vector store externo, secret store, telemetria e operação.
Um vector store gerenciado exige também `VECTOR_STORE_EGRESS`; autorização do
runtime não amplia a política específica. GitHub Pages pode hospedar apenas
um frontend estático opcional; não substitui o backend nem o uso de OCI.

## Integração futura ao DB-Notifier

Compatibilidade é obtida por:

- .NET 10 e convenções equivalentes;
- boundaries Domain/Application/Infrastructure;
- resultados e erros tipados;
- configuração e providers fail-closed;
- contrato HTTP/OpenAPI versionado;
- proveniência, UTC, cancelamento e observabilidade.

A primeira fronteira de integração será um adapter HTTP pertencente ao
DB-Notifier que consome o contrato OpenAPI v1 pertencente ao RAG-Challenge.
Requisição, resposta tipada, cobertura/proveniência, citações, metadados de
reprodutibilidade, `indexGenerationId` e `correlationId` pertencem ao contrato
público; entidades Domain, portas RAG e tipos de SDK não atravessam essa
fronteira.

Empacotamento in-process de contratos ou casos de uso só poderá nascer depois,
por ADR próprio e evidência de necessidade. A decisão de integrar também será
registrada no repositório consumidor. O RAG-Challenge não referencia
assemblies, banco, eventos ou configuração do DB-Notifier no MVP e não implementa o
adapter consumidor.

## Implantação e funcionamento independente

- Local: API, Dashboard e dependências configuradas pelo desenvolvedor.
- OCI: a mesma aplicação e contratos, com secrets e armazenamento adequados
  ao ambiente.
- GitHub: código, documentação e CI.
- GitHub Pages: somente interface estática opcional.

O produto deve continuar funcional sem DB-Notifier instalado.
