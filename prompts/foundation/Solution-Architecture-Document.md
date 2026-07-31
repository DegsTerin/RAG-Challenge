# Arquitetura da Solução Challenge

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
- Índices construídos de forma imutável antes da ativação.
- Falha externa isolada e explicitamente classificada.
- Fonte local e fonte oficial externa separadas.
- Contrato externo versionado pertencente ao Challenge; adapters consumidores
  futuros pertencem aos respectivos repositórios.

## Contexto do sistema

```text
Question author / evaluator
          |
          v
   Challenge Dashboard
          |
       HTTPS
          |
          v
    Challenge Server/API
          |
          v
 Application use cases
   |       |        |        |
   |       |        |        +--> language-model adapter
   |       |        +-----------> vector-store adapter
   |       +--------------------> document/embedding adapters
   +----------------------------> allowlisted official-PDF adapter
```

No MVP, o servidor pode hospedar a API e os arquivos estáticos do Dashboard no
mesmo deploy. Isso reduz operação sem acoplar a interface aos casos de uso.

## Direção de dependências

```text
Challenge.Domain
        ^
        |
Challenge.Rag.Abstractions
        ^
        |
Challenge.Application
        ^
        |
Infrastructure / Persistence / API

Challenge.Dashboard.Web -- versioned HTTP --> Challenge.Server.Api
```

- `Challenge.Domain` possui identidades, versões, estados e invariantes.
- `Challenge.Rag.Abstractions` possui contratos RAG e depende apenas da
  semântica canônica necessária.
- `Challenge.Application` implementa casos de uso e orquestra portas.
- Infrastructure e Persistence implementam adapters.
- API é composition root e não contém regras de negócio.
- Dashboard não possui referência de código ao Application; consome somente
  contratos HTTP versionados.

## Módulos canônicos

| ID | Módulo | Responsabilidade |
|---|---|---|
| `CH-MOD-01` | `CORPUS_CATALOG` | Identidade, configuração, versão e estado do acervo. |
| `CH-MOD-02` | `DOCUMENT_INGESTION` | Descoberta local, validação, parsing e normalização. |
| `CH-MOD-03` | `INDEXING_RETRIEVAL` | Chunking, embeddings, gerações e recuperação. |
| `CH-MOD-04` | `ANSWER_GENERATION` | Contexto grounded, resposta, citações e recusa. |
| `CH-MOD-05` | `QUERY_EXPERIENCE` | API e interface de consulta. |
| `CH-MOD-06` | `OPERATIONS_GOVERNANCE` | Configuração, health, logs, auditoria e gates. |
| `CH-MOD-07` | `OFFICIAL_EXTERNAL_SOURCES` | Sincronização manual e governada de um PDF oficial no MVP. |
| `CH-MOD-08` | `EXTERNAL_INTEGRATION_CONTRACTS` | Contrato HTTP/OpenAPI versionado do Challenge; adapters consumidores ficam fora deste repositório. |

IDs não devem ser reutilizados com outro significado.
`CH-MOD-08` conserva a fronteira de integração da baseline anterior, mas o
rótulo foi corrigido antes do Human Gate para deixar explícito que o Challenge
é owner do contrato e não do adapter consumidor.

## Estrutura candidata para o `STATE-01`

Esta árvore não é autoridade de scaffold. Ela só poderá orientar `STATE-01`
se o ADR-0001 for aceito no `GATE-B01`, o mapa físico de projetos for
registrado e a entrada no estado receber autorização separada.

```text
/
├── AGENTS.md
├── README.md
├── Challenge.sln
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
│   ├── Challenge.Domain/
│   ├── Challenge.Rag.Abstractions/
│   ├── Challenge.Application/
│   ├── Challenge.Infrastructure/
│   ├── Challenge.Persistence.Sqlite/
│   ├── Challenge.Server.Api/
│   └── Challenge.Dashboard.Web/
└── tests/
    ├── Challenge.UnitTests/
    ├── Challenge.Architecture.Tests/
    └── Challenge.IntegrationTests/
```

Se autorizada, a fase cria apenas scaffold, configuração, checks e hosts
mínimos. Não implementa ingestão, recuperação ou geração. Cada assembly deve
possuir responsabilidade e boundary de dependência ou teste distintos. No
`GATE-B01`, `Challenge.Rag.Abstractions` poderá ser combinado com
`Challenge.Application`, e `Challenge.Persistence.Sqlite` com
`Challenge.Infrastructure`, se a separação física não se justificar. As
fronteiras conceituais permanecem mesmo quando dois papéis compartilham um
assembly. O mesmo gate registra o mapa
`CH-MOD-* → namespace/pasta/projeto`, dependências permitidas e testes de
arquitetura. Também decide se a administração one-shot usa um modo explícito
do host principal ou justifica um projeto `Challenge.Tools.Admin`; a escolha
da identidade e das permissões continua pertencendo ao `STATE-02`.

## Convenções de nomenclatura

- Solution e prefixo de namespace: `Challenge`.
- Projetos: `Challenge.<Responsibility>`.
- Testes: `Challenge.<TestScope>Tests`.
- Tipos e membros C#: PascalCase; variáveis e parâmetros: camelCase.
- API inicial: `/api/v1`.
- Configuração: seções `Challenge:<Capability>`.
- IDs de erro: `CH_<AREA>_<CONDITION>`, estáveis e sem detalhe secreto.
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
- `DocumentId`, `DocumentVersion` e `SourceDescriptor`;
- `ContentObjectId` e referência imutável ao conteúdo bruto;
- `SourceScope`, `OfficialSourceSnapshot`, `OfficialSourceObservation` e
  `SourceFreshness`;
- `ChunkIdentity` e `Citation`;
- `CandidateBuildId`, `IndexGenerationId`, `IndexGenerationStatus` e
  `CorpusActivationRecord`;
- `ProviderDescriptor`;
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
`IndexGenerationId`, `SourceScope`, vetor de consulta, limites e política de
score. O adapter prova hard pre-filter dos três seletores antes do top-k ou usa
partição física equivalente; post-filter de busca global não satisfaz o
contrato. Ele não possui autoridade de ativação.

`IDocumentContentStore` persiste e reabre bytes imutáveis endereçados por
conteúdo. `IDocumentCatalog` mantém identidades e referências, e
`IIndexGenerationStore` é a única fonte de verdade do
`CorpusActivationRecord`, que vincula atomicamente geração ativa, snapshot
oficial e observação de freshness aplicável. Revisões completas anterior e
nova do registro ficam no histórico versionado da mesma transação; `Active` e
`Retained` são projeções desse estado, não autoridades paralelas.

### Application

Casos de uso candidatos:

- `BuildCorpusIndex`;
- `ActivateIndexGeneration`;
- `RollbackIndexGeneration`;
- `SynchroniseOfficialSource`;
- `AskQuestion`;
- `GetSystemReadiness`.

Atualização incremental agendada e gestão de múltiplos acervos não integram os
casos de uso do MVP. A sincronização oficial é manual, limitada a uma URL
configurada e não ocorre no fluxo de pergunta.

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
- A fonte oficial aplica URL PDF pública sem credenciais, allowlist completa,
  proteção SSRF, pinning DNS/IP/Host/SNI, política TLS sem egress lateral,
  limites e snapshot antes de reutilizar o mesmo parser PDF.
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
pertence ao Challenge e não expõe entidades Domain nem portas de provider.
Conceitualmente:

```text
QueryRequestV1
  corpusId
  sourceScope: Local | OfficialOnline
  question

QueryResponseV1
  sourceScope
  outcome: Answered | InsufficientEvidence
  answer?
  citations[]
  sourceSnapshotId?
  sourceFreshness?
  indexGenerationId
  retrievalPolicyVersion
  promptVersion
  languageModelDescriptor
  correlationId
```

Cada citação preserva corpus, escopo, documento, versão, geração e localização.
Citação oficial inclui URL canônica, snapshot, `revalidatedAt` e freshness.
`languageModelDescriptor` contém apenas provider, modelo e revisão não
secretos; não contém endpoint, credencial ou configuração interna.

O artefato OpenAPI v1 pertence ao Challenge, é gerado e versionado com a API,
inclui pergunta, respostas concluídas, citações e Problem Details, e passa por
teste de compatibilidade. A política de breaking changes pertence ao
`STATE-02`; a implementação e a prova do artefato pertencem ao `STATE-04`.

`QueryResponseV1` representa apenas uma consulta concluída com
`Answered` ou `InsufficientEvidence`. Entrada inválida, fonte
stale/indisponível, violação de política, rate limit, indisponibilidade de
provider e falha interna são resultados tipados da Application mapeados pela
API para Problem Details não `2xx`, com código estável e sem detalhes
sensíveis. O mapeamento HTTP exato pertence ao `STATE-02`.

### Dashboard

- Interface mínima e responsiva.
- Seletor obrigatório `Local` ou
  `Documentação oficial online — snapshot sincronizado`.
- Estados de carregamento, vazio, erro, rate limit, fonte indisponível/stale e
  evidência insuficiente.
- Navegação por teclado e foco visível.
- Citações acessíveis e separadas da resposta.
- Nenhum acesso direto ao vetor, ao LLM ou a secrets.
- Saída é texto puro por padrão. Se Markdown for autorizado, usa subconjunto
  sanitizado, bloqueia HTML cru, permite somente schemes de URL aprovados e
  opera sob Content Security Policy.

## Fluxo de indexação

```text
configured corpus
  -> discover local document and selected retained official snapshot
  -> validate and hash
  -> persist content by hash and reopen/verify
  -> parse
  -> chunk with versioned strategy
  -> embed
  -> write under temporary candidate build identity
  -> finalise canonical manifest with logical artifact digest/counts
  -> validate readback, manifest and smoke queries
  -> compare-and-swap CorpusActivationRecord in IIndexGenerationStore
```

O snapshot oficial selecionado para a candidata é a referência preservada pelo
registro ativo, independentemente de seu estado `Current` ou `Stale`.
Freshness decide elegibilidade de consulta, não pertencimento ao manifesto.
Falha antes ou durante o compare-and-swap mantém geração, snapshot e
observação anteriores. Vetores, conteúdo ou observações candidatos que não
forem ativados são órfãos auditáveis até cleanup explícito. A estratégia
detalhada está em [`RAG-Module.md`](RAG-Module.md).

Candidato parcial nunca possui `IndexGenerationId` nem é consultável. A
identidade final deriva da especificação e do digest/contagens dos artefatos
lógicos produzidos; outputs distintos não reutilizam silenciosamente o mesmo
ID. Staging, finalização idempotente e evidência mínima de readback pertencem
ao `STATE-03`.

A candidata contém os dois `SourceScope`s. Em atualização operacional de
conteúdo, somente um deles muda por operação serializada; bootstrap e migração
global da chave de compatibilidade podem reconstruir ambos e validam o
conjunto inteiro. O vector store deve filtrar `CorpusId`,
`IndexGenerationId` e `SourceScope` antes do top-k; se não oferecer pre-filter
comprovável, o adapter mantém partições físicas equivalentes. Rollback troca o
conjunto inteiro.

## Fluxo de consulta

```text
question
  -> validate and bound
  -> validate Local | OfficialOnline
  -> resolve active generation once
  -> validate selected scope availability and freshness
  -> embed query
  -> filter source scope
  -> retrieve top candidates by explicit generation ID
  -> apply score/policy checks
  -> build untrusted evidence context
  -> generate constrained answer
  -> validate citations
  -> answer or INSUFFICIENT_EVIDENCE
```

## Configuração

- Configuração comum em arquivos sem segredo.
- Overrides por ambiente e variáveis protegidas.
- Secrets somente por referências ou nomes de variáveis.
- Startup valida provider, modelo, dimensão, limites, caminhos do corpus e do
  content store, durabilidade mínima e compatibilidade do índice. O perfil
  oficial valida a URL canônica, política de egress e freshness sem executar
  sincronização.
- Capacidade incompleta permanece desativada; não há fallback silencioso.
- Um `.env.example` futuro contém apenas nomes e valores fictícios.

Seções candidatas:

```text
Challenge:Corpus
Challenge:OfficialSource
Challenge:ContentStore
Challenge:Parsing
Challenge:Chunking
Challenge:Embeddings
Challenge:VectorStore
Challenge:LanguageModel
Challenge:Query
Challenge:Observability
Challenge:Egress
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
- Métricas de ingestão/sincronização: duração, documentos, páginas, bytes,
  chunks, freshness, falhas e versão.
- Métricas de consulta: latência por estágio, candidatos, recusas e falhas de
  provider.
- Custo/tokens somente quando o provider oferece metadados seguros.
- Liveness verifica somente que o processo responde e não depende de serviço
  externo.
- Readiness global exige que o núcleo e as dependências necessárias consigam
  servir a capacidade obrigatória `Local`. `OfficialOnline=Stale`,
  `Unavailable`, `Withdrawn` ou `Deactivated` aparece como degradação tipada
  por scope e não retira uma instância que ainda atende `Local`. Egress de
  sincronização não integra o caminho de readiness de consulta.
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

Testes padrão usam fonte HTTP falsa local e não acessam a URL oficial real.
Smoke real é opt-in e exige autoridade de rede própria.

CD e OCI pertencem aos estados posteriores. O deploy inicial candidato é um
único artefato no serviço OCI selecionado, com configuração e secrets
externos. `OFFICIAL_SOURCE_EGRESS` fica limitado à URL oficial exata;
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
DB-Notifier que consome o contrato OpenAPI v1 pertencente ao Challenge.
Requisição, resposta tipada, `sourceScope`, citações, metadados de
reprodutibilidade, `indexGenerationId` e `correlationId` pertencem ao contrato
público; entidades Domain, portas RAG e tipos de SDK não atravessam essa
fronteira.

Empacotamento in-process de contratos ou casos de uso só poderá nascer depois,
por ADR próprio e evidência de necessidade. A decisão de integrar também será
registrada no repositório consumidor. O Challenge não referencia assemblies,
banco, eventos ou configuração do DB-Notifier no MVP e não implementa o
adapter consumidor.

## Implantação e funcionamento independente

- Local: API, Dashboard e dependências configuradas pelo desenvolvedor.
- OCI: a mesma aplicação e contratos, com secrets e armazenamento adequados
  ao ambiente.
- GitHub: código, documentação e CI.
- GitHub Pages: somente interface estática opcional.

O produto deve continuar funcional sem DB-Notifier instalado.
