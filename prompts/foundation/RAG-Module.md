# Módulo RAG

## Status

Contrato arquitetural proposto para o MVP e sua evolução. Nenhum pipeline,
provider, índice, corpus ou modelo está implementado ou ativo.

## Objetivo e limites

O módulo recupera evidências de um acervo governado e gera respostas
fundamentadas. Ele não:

- administra bancos de dados;
- executa SQL ou shell;
- acessa secrets diretamente;
- decide autorização por saída do modelo;
- consulta a internet sem adapter e política próprios;
- trata texto recuperado como instrução confiável.

## Pipeline canônico

```text
Local source or governed official snapshot
  -> Discovery
  -> Validation
  -> Content-addressed persistence and verified reopen
  -> Parsing
  -> Normalisation
  -> Chunking
  -> Embeddings
  -> Index generation
  -> Activation

Question
  -> Validation
  -> Source-scope selection
  -> Source availability/freshness validation
  -> Query embedding
  -> Retrieval
  -> Evidence policy
  -> Grounded generation
  -> Citation validation
  -> Answer outcome
```

## Conceitos

### Corpus

Unidade lógica de conhecimento com:

- `corpusId` estável;
- nome e descrição;
- estado `Active`, `Inactive` ou `Unavailable`;
- política de fonte;
- idioma;
- revisão declarada;
- referência lógica à geração de índice ativa, cujo registro canônico pertence
  exclusivamente ao `IIndexGenerationStore`.

O MVP possui um corpus configurado. A existência do conceito não implementa
gestão de múltiplos acervos.

O corpus possui dois `SourceScope`s fixos: `Local` e `OfficialOnline`. O scope
integra identidade de documento/chunk, digests, metadados vetoriais, filtros e
citações. Ele não representa dois acervos administráveis nem permite `All`.

### Documento e versão

`DocumentId` identifica o documento lógico. Cada conteúdo imutável recebe
`DocumentVersion` com:

- hash SHA-256;
- tamanho e media type;
- versão declarada quando disponível;
- data da fonte;
- data de ingestão;
- `sourceAdapterId`;
- `SourceTrustClass`;
- locator sanitizado;
- estado de licenciamento/proveniência.

Renomear um arquivo não deve criar silenciosamente uma nova identidade lógica
quando o catálogo já tiver um mapeamento estável. Conteúdo alterado sempre
produz nova versão.

Parser e configuração não alteram `DocumentVersion`. Um artefato textual
derivado é identificado separadamente por `DocumentVersion` mais descriptor
do parser/normalização e sua configuração não secreta; essa compatibilidade
integra a geração do índice.

### Chunk

Cada chunk preserva:

- corpus, `SourceScope`, documento e versão;
- estratégia e versão de chunking;
- ordem;
- página, seção ou localização disponível;
- hash do texto normalizado;
- metadados permitidos para filtro e citação.

Chunks não carregam secrets nem instruções confiáveis.

### Geração de índice e compatibilidade

Uma geração é imutável e identificada por um manifesto canônico finalizado que
inclui:

```text
manifestSchemaVersion
corpusId
corpusRevision
documentSetDigest
officialSnapshotSetDigest
indexCompatibilityKey
generationSpecDigest
chunkCount
vectorCount
logicalArtifactDigest
```

O `documentSetDigest` é calculado sobre a lista ordenada de identidades e
versões dos documentos de ambos os `SourceScope`s. O
`officialSnapshotSetDigest` identifica somente os snapshots oficiais
referenciados; a revisão local permanece no `documentSetDigest`.

`generationSpecDigest` é o SHA-256 da representação canônica dos seis primeiros
campos do manifesto e identifica a especificação de build. O candidato usa um
`candidateBuildId` temporário; depois de produzir todos os chunks e vetores, a
finalização calcula `logicalArtifactDigest` sobre registros lógicos ordenados e
canônicos, registra contagens e produz o manifesto completo.

O manifesto usa schema versionado, nomes e ordem de propriedades fixos,
serialização UTF-8 e ordenação ordinal de conjuntos. O
`generationContentDigest` é o SHA-256 do manifesto completo, e
`IndexGenerationId` deriva dele com prefixo estável. Mesma especificação e
mesmos artefatos lógicos produzem a mesma identidade; outputs diferentes nunca
colidem sob um ID já finalizado. `createdAt`, status operacional e observações
de freshness são metadados fora do digest de conteúdo.

O `STATE-03` fecha staging e finalização idempotentes. A ativação só admite
manifesto finalizado após conferir contagens, digest dos payloads produzidos e
readback/sentinelas suportados pelo adapter. Candidato parcial nunca recebe
`IndexGenerationId` nem fica consultável; retry pode reaproveitar apenas
staging comprovadamente compatível ou remover o órfão por cleanup explícito.

O `IndexCompatibilityKey` é o SHA-256 de uma serialização canônica,
versionada e sem secrets dos campos:

```text
parserAdapterId/version/nonSecretConfigDigest
sourceAdapterDescriptorSetDigest
normalisationId/version/nonSecretConfigDigest
chunkerId/version/nonSecretConfigDigest
chunkSize/chunkOverlap/separatorPolicy
embeddingAdapterId/version/nonSecretConfigDigest
embeddingProviderId/modelRevision/dimensions/vectorNormalisation
vectorStoreAdapterId/version/schemaVersion
distanceMetric/indexAlgorithm/nonSecretIndexParametersDigest
```

Qualquer mudança em um desses campos produz chave diferente e exige nova
geração. A aplicação recusa usar índice cuja chave não corresponda à
configuração ativa; igualdade de versão nominal não permite reutilização
silenciosa de artefatos incompatíveis.

## Portas substituíveis

| Porta | Responsabilidade |
|---|---|
| `IDocumentSource` | Enumerar e abrir documentos sem interpretar conteúdo. |
| `IOfficialSourceSynchroniser` | Buscar somente fontes oficiais configuradas e produzir snapshot imutável. |
| `IDocumentParser` | Transformar bytes validados em unidades textuais e localização. |
| `IChunkingStrategy` | Produzir chunks determinísticos e versionados. |
| `IEmbeddingProvider` | Gerar vetores com descriptor de modelo e dimensão. |
| `IVectorStore` | Escrever gerações imutáveis e consultar por `VectorSearchRequest` com `CorpusId`, `IndexGenerationId` e `SourceScope`; provar hard pre-filter dos três seletores ou partição física equivalente e não gerir ativação. |
| `ILanguageModel` | Gerar resposta limitada ao prompt e às evidências. |
| `IDocumentContentStore` | Persistir e reabrir bytes imutáveis content-addressed de versões locais e snapshots oficiais. |
| `IDocumentCatalog` | Persistir identidades, versões, proveniência e estado. |
| `IIndexGenerationStore` | Persistir manifestos e ser a única fonte de verdade do `CorpusActivationRecord`, com compare-and-swap e rollback. |

Cada implementação declara identificador, versão, capacidades, limites e
configuração não secreta. O registro é estático por dependency injection no
MVP; plug-ins dinâmicos ficam fora do escopo.

O `IDocumentContentStore` usa gravação idempotente por hash, valida o conteúdo
reaberto e impede sobrescrita. Catálogo e manifesto guardam referências
estáveis; a política de retenção impede remover conteúdo alcançável pela
geração ativa ou pelo único alvo de rollback. Vector store guarda derivados e
não substitui a fonte bruta necessária para reconstrução.

O Application orquestra ativação. O vector store não mantém uma segunda
autoridade de estado ativo; eventual alias técnico de um adapter é somente
projeção recuperável, nunca o system of record. Busca global seguida de
post-filter também não satisfaz o contrato de isolamento.

### Estado e registro de ativação

Uma geração percorre `Candidate → Validated`; falha antes da validação leva a
`Failed`. `Active` e `Retained` são projeções derivadas, respectivamente, do
registro de ativação corrente e das revisões completas preservadas para
rollback, nunca estados mutáveis concorrentes em outro store. Somente uma
geração `Validated` pode integrar um registro ativo.

O `CorpusActivationRecord` contém ao menos:

```text
corpusId
recordRevision
previousRecordRevision?
indexGenerationId
officialSnapshotId?
officialObservationId?
generationActivatedAt
recordUpdatedAt
```

O compare-and-swap altera o registro inteiro em uma transação do plano de
controle que também preserva as representações completas anterior e nova no
histórico versionado de ativação e grava o evento de auditoria sanitizado. A
observação e o snapshot já devem existir de forma imutável antes da transação.
Falha ou conflito deixa registro e histórico anteriores intactos; conteúdo,
observação e vetores candidatos permanecem órfãos auditáveis até cleanup
explícito. A consulta lê o registro corrente uma vez e não combina geração com
a “última observação” obtida separadamente.

## Fontes locais e externas

### Fonte local do MVP

Usa `sourceAdapterId=local-directory` e
`SourceTrustClass=LocalAuthorised`. O ID do adapter é extensível; a
classificação de confiança é fechada e não concede autorização por si só.

- raiz configurada e canonicalizada;
- sem acesso fora da raiz;
- allowlist de extensão/media type;
- tamanho e quantidade limitados;
- conteúdo hashado antes da indexação;
- bytes validados promovidos idempotentemente ao `IDocumentContentStore` e
  reabertos com hash verificado antes de qualquer ativação;
- nenhuma dependência de `reference-materials/`.

### Fonte oficial externa do MVP

Implementação separada com `SourceTrustClass=OfficialExternal` e
`sourceAdapterId` estável específico do adapter:

- somente HTTPS;
- exatamente uma fonte oficial aprovada no MVP;
- scheme, domínio, porta, path e query exatos do PDF em allowlist;
- fonte pública anônima, sem userinfo, token/assinatura em query,
  `Authorization`, API key, client certificate ou credencial ambiente;
- redirects desativados no MVP;
- cada conexão física resolve e autoriza DNS/IP uma vez, conecta somente ao IP
  aprovado e preserva host/SNI sem nova resolução por hostname;
- validação TLS não pode buscar AIA, CRL ou OCSP fora da política; trust,
  revogação, downloads de cadeia e eventual material local são decididos no
  `STATE-02`, e qualquer destino auxiliar exige allowlist própria;
- timeout, máximo de bytes/páginas, media type PDF, concorrência e rate limit;
- termos, licença e robots revisados antes da primeira sincronização;
- snapshot de conteúdo imutável com `sourceKey`, `snapshotId`, URL canônica,
  ETag/Last-Modified observados na captura, hash, `retrievedAt` e licença;
- bytes do snapshot persistidos pelo `IDocumentContentStore`;
- observações de revalidação append-only com `observationId`, `snapshotId`,
  validators condicionais enviados, status HTTP, ETag/Last-Modified
  observados, `revalidatedAt`, `maxAge`, resultado e evidência sanitizada;
- sincronização para um snapshot governado antes da recuperação;
- distinção visível entre evidência local e online.

O conteúdo do snapshot nunca muda. O vínculo configurado da fonte possui
estado `Current`, `Stale`, `Withdrawn` ou `Deactivated`, derivado da observação
apontada pelo `CorpusActivationRecord`, e não simplesmente da última
observação gravada. Conteúdo expirado, retirado ou desativado não é apresentado
como atual; status e frescor acompanham a citação. A política padrão do MVP
falha fechada para `OfficialOnline` quando o registro ativo não vincula
snapshot e observação elegível `Current`.

A consulta não recebe acesso irrestrito à web. O conteúdo sincronizado não
altera políticas, prompts de sistema ou autorização.

Sincronização oficial é um caso de uso administrativo manual:

1. carregar uma configuração aprovada; nenhuma URL vem da pergunta;
2. canonicalizar a URL pública sem credenciais, validar allowlist, resolver
   A/AAAA e rejeitar atomicamente respostas mistas/proibidas; conectar ao IP
   aprovado preservando host/SNI, sem egress lateral da validação TLS;
3. fazer request condicional usando os validators da observação vinculada ao
   registro ativo e persistir os validators enviados/recebidos e o status;
   redirects permanecem desativados;
4. em resposta `304`, persistir observação imutável e atualizar por
   compare-and-swap somente o `officialObservationId` do registro ativo se ele
   já referenciar o mesmo snapshot e uma geração compatível; caso contrário,
   seguir para reconstrução controlada;
5. baixar para quarentena, limitar também bytes descomprimidos, validar media
   type, assinatura e páginas, e calcular hash antes de promover conteúdo;
6. se o hash for igual ao snapshot conhecido, aplicar a mesma regra
   transacional do passo 4;
7. se o conteúdo mudou, persistir bytes e novo snapshot bruto imutáveis,
   reabrir e conferir o hash, acrescentar observação validada ainda não ativa e
   processar pelo mesmo parser PDF da fonte local;
8. reconstruir a geração candidata e validar proveniência, frescor,
   isolamento e smoke queries;
9. em uma única transação do plano de controle, ativar por compare-and-swap o
   `CorpusActivationRecord` que vincula geração, snapshot e observação, junto
   do registro de auditoria.

Uma resposta autoritativa `404`/`410`, quando assim definida pela política da
fonte, cria observação `Withdrawn` vinculada ao snapshot ativo. Uma operação
administrativa explícita e auditada cria observação `Deactivated` sem fetch.
Nos dois casos, o compare-and-swap muda somente a observação do registro
compatível e preserva geração/snapshot; nenhuma reindexação ocorre. Falha
transitória de DNS/transporte/`5xx` registra a tentativa, mas não substitui uma
observação `Current`; o snapshot passa a `Stale` pelo `maxAge`. Voltar a
`Current` exige nova sincronização/revalidação elegível e, após
`Deactivated`, reativação administrativa explícita.

Falha transitória ou sincronização rejeitada nunca altera geração, snapshot ou
observação ativos. Um snapshot anterior pode continuar servindo somente
enquanto `Current`; após `maxAge`, o resultado é `SourceStale`, sem fallback
silencioso para `Local`.

## Estratégia do MVP para atualização

O MVP mantém o fluxo simples:

1. descobrir o documento local e o snapshot oficial referenciado pelo registro
   ativo, quando existente, independentemente de freshness;
2. validar, classificar e hashear cada documento;
3. persistir ou reutilizar o objeto imutável por hash, reabri-lo pelo
   `IDocumentContentStore` e conferir seus bytes;
4. construir uma geração única com chunks marcados como `Local` ou
   `OfficialOnline`;
5. validar manifesto, referências reabríveis, compatibilidade, isolamento de
   escopo e smoke queries;
6. trocar por compare-and-swap o `CorpusActivationRecord` completo no
   `IIndexGenerationStore`, incluindo o vínculo oficial aplicável;
7. manter a geração ativa e ao menos uma geração anterior validada até cleanup
   explícito após a janela de rollback definida.

O MVP pode reconstruir a geração completa. Ele não precisa implementar diff
por chunk, scheduler, fila ou sincronização distribuída.

Cada pergunta exige `sourceScope=Local|OfficialOnline`. A recuperação filtra o
escopo antes de selecionar candidatos. O valor `Combined` não existe no MVP.

Invariantes da geração conjunta:

- uma candidata representa snapshot coerente dos dois escopos;
- uma atualização de `Local` preserva a revisão oficial do registro ativo,
  mesmo quando a fonte oficial está `Stale`; freshness não remove conteúdo do
  manifesto;
- atualizações operacionais de conteúdo são serializadas e no máximo um
  escopo muda por geração;
- bootstrap inicial e migração global da chave de compatibilidade podem
  reconstruir ambos os escopos, são classificadas como operações globais e
  validam o conjunto inteiro;
- ambos os escopos e o pre-filter são validados antes da ativação;
- `VectorSearchRequest` exige `CorpusId`, `IndexGenerationId` e `SourceScope`;
  o vector store prova filtro pelos três seletores antes do top-k; se não
  suportar hard pre-filter, usa partições físicas equivalentes;
- post-filter depois de busca global é violação de contrato;
- rollback troca a geração inteira; rollback parcial cria uma nova candidata;
- freshness oficial é metadado revalidado fora do índice e não volta a
  `Current` apenas por rollback.

## Atualização incremental futura

```text
discover snapshot
  -> compare source keys, hashes and versions
  -> classify Added / Changed / Removed / Unchanged
  -> parse and embed Added + Changed
  -> tombstone Removed in the candidate generation
  -> reuse compatible Unchanged artefacts
  -> validate candidate generation
  -> atomically activate
```

Requisitos:

- operação idempotente;
- checkpoint e retomada segura;
- lote, timeout, cancelamento e backpressure;
- nenhuma consulta lê geração parcialmente escrita;
- remoção preserva auditoria e retenção;
- provider/model/chunking incompatível força reconstrução controlada.

## Rollback

Rollback de índice usa como alvo uma revisão anterior completa e preservada do
`CorpusActivationRecord`, ainda compatível e verificada, e cria a nova revisão
corrente por compare-and-swap. Não edita vetores no lugar nem combina a
geração anterior com um vínculo oficial arbitrário.

Rollback de documento seleciona uma versão anterior e cria uma nova candidata
quando somente um escopo retorna. Uma geração anterior só pode ser reativada
quando o conjunto completo `Local` + `OfficialOnline`, a chave de
compatibilidade e a observação de freshness elegível correspondem exatamente
ao alvo; reativação nunca torna snapshot antigo novamente `Current`.

Rollback de aplicação, configuração, catálogo e índice são procedimentos
separados. No MVP, cleanup nunca remove a geração ativa nem o único alvo de
rollback elegível antes do fim da janela aprovada. Depois de cleanup explícito,
ou diante de incompatibilidade comprovada com o runtime, o caminho é
reconstrução controlada e não uma promessa falsa de rollback.

Ativação e retorno devem testar:

1. leitura do registro esperado;
2. compare-and-swap do registro inteiro para a candidata validada;
3. rejeição segura em conflito de concorrência;
4. consulta usando somente a geração resolvida;
5. compare-and-swap de retorno para a anterior;
6. atomicidade entre geração, snapshot, observação e evento de auditoria;
7. crash antes, durante e depois de cada fronteira de persistência;
8. preservação do registro completo anterior no histórico de ativação;
9. auditoria de ator, motivo, origem, alvo e resultado.

## Múltiplos acervos futuros

- Namespace de vetor e metadados sempre inclui `corpusId`.
- Consulta deve receber escopo de corpus explícito.
- Ativação e versão são independentes por corpus.
- Adicionar, remover ou desativar corpus não altera o núcleo.
- Filtros de autorização são aplicados antes da recuperação.
- Resultados de acervos diferentes não são misturados silenciosamente.

O MVP fixa um único corpus por configuração e não expõe administração remota.

## Recuperação e geração

- Normalizar e limitar a pergunta.
- Validar `sourceScope`; não aceitar URL, domínio ou adapter na pergunta.
- Resolver o `CorpusActivationRecord` uma única vez no início da consulta.
- Validar disponibilidade e freshness do escopo selecionado antes de gerar o
  query embedding ou chamar qualquer provider.
- Usar `CorpusId`, `IndexGenerationId` e `SourceScope` resolvidos em um
  `VectorSearchRequest` durante toda recuperação, validação e citação; nenhuma
  etapa relê silenciosamente o registro.
- Usar top-k e thresholds definidos por avaliação, não por palpite.
- Aplicar os filtros obrigatórios de `CorpusId`, `IndexGenerationId` e
  `SourceScope` antes do top-k/ranking.
- Separar claramente instruções confiáveis de evidências não confiáveis.
- Limitar número e tamanho total dos trechos.
- Exigir referência de cada afirmação factual relevante.
- Rejeitar citação que não pertença ao conjunto recuperado.
- Não preencher lacunas com conhecimento paramétrico não citado.
- Retornar `INSUFFICIENT_EVIDENCE` quando não houver suporte suficiente.

O modelo não recebe acesso direto ao vetor, arquivo, rede ou catálogo. A
Application seleciona e limita as evidências.

`OfficialOnline` stale/indisponível falha antes de retrieval/LLM. `Local`
continua operacional, mas só é usado depois de ação explícita do usuário.

### Readiness por capacidade

- Liveness depende apenas da capacidade do processo responder.
- Readiness global exige plano de controle, geração ativa compatível, vector
  store, embedding de query e LLM necessários para servir ao menos `Local`.
- `OfficialOnline=Stale|Unavailable|Withdrawn|Deactivated` é degradação por
  scope e não torna a instância globalmente indisponível quando `Local`
  continua saudável.
- Disponibilidade do egress administrativo de sincronização é diagnóstico
  separado e não participa da readiness do caminho de consulta.

## Respostas e citações

`AnswerOutcome` possui somente resultados concluídos:

- `Answered`;
- `InsufficientEvidence`.

Falhas de consulta são resultados tipados separados:

- `InvalidInput`;
- `CorpusUnavailable`;
- `SourceUnavailable`;
- `SourceStale`;
- `SourcePolicyViolation`;
- `EmbeddingUnavailable`;
- `IndexUnavailable`;
- `LanguageModelUnavailable`;
- `RateLimited`;
- `OperationCancelled`;
- `UnexpectedFailure`.

Esses nomes são subconjunto da taxonomia canônica da Application. O
`STATE-02` define uma única tabela para código `CH_*`, HTTP e Problem Details;
adapters não traduzem a mesma falha para categorias concorrentes.

Uma citação sempre inclui:

- `corpusId`;
- `SourceScope`;
- `indexGenerationId`;
- `documentId`;
- `documentVersion`;
- chunk ID;
- `sourceAdapterId` e `SourceTrustClass`.

Quando disponíveis, também inclui:

- título;
- página ou seção;
- locator seguro para exibição.

A resposta inclui metadados técnicos:

- `SourceScope`;
- `indexGenerationId`;
- `retrievalPolicyVersion`;
- `promptVersion`;
- provider e revisão do modelo de linguagem;
- `correlationId`.

Para fonte oficial, a citação inclui também a URL canônica pública sem
credenciais, `snapshotId`, `revalidatedAt`, estado e frescor. Esses metadados
permitem reproduzir uma resposta sem expor prompts, configuração secreta ou
conteúdo integral.

Scores brutos de diferentes providers não são apresentados como confiança
universal sem calibração.

## Segurança

- Prompt injection em documento é ameaça explícita.
- PDFs são entrada não confiável; parsing ocorre com limites e sem macros.
- O PDF oficial também é entrada não confiável; anexos, links e instruções
  embutidas não recebem autoridade.
- Cada conexão oficial usa somente IP previamente resolvido e autorizado,
  preserva host/SNI e não refaz resolução por hostname no socket.
- A fonte oficial é anônima e sua validação TLS não inicia downloads
  AIA/CRL/OCSP ou qualquer egress fora das allowlists.
- Upload público não integra o MVP.
- Perguntas não podem selecionar provider, caminho, URL ou prompt de sistema.
- Contexto recuperado usa delimitadores e instruções explícitas de
  não autoridade.
- Respostas não podem executar ferramentas ou gerar ações administrativas.
- Logs não armazenam texto integral por padrão.
- Caches e índices são dados derivados, com acesso e retenção controlados.

## Avaliação

Antes de homologar um provider ou versão:

- conjunto de perguntas representativas e casos sem resposta;
- rubrica para relevância, fidelidade e qualidade da citação;
- recuperação: recall/precision em critérios aprovados;
- resposta: groundedness e ausência de afirmações não sustentadas;
- segurança: prompt injection e conteúdo malicioso;
- isolamento entre `Local` e `OfficialOnline`;
- busca adversarial em que chunks do scope incorreto pontuam acima dos
  corretos, provando pre-filter antes do top-k;
- busca adversarial com chunks de outra geração e, quando aplicável, de outro
  corpus pontuando acima dos corretos, provando isolamento antes do top-k;
- SSRF, redirect, domínio/path, tamanho e freshness da fonte oficial;
- `304` ou hash idêntico atualiza a observação de revalidação sem criar novo
  snapshot ou índice somente quando o registro ativo já referencia o snapshot
  compatível;
- atualização local enquanto `OfficialOnline` está stale, seguida de
  revalidação `304`, preservando o snapshot e restaurando elegibilidade sem
  mistura de geração;
- crash antes, durante e depois da ativação, provando atomicidade do
  `CorpusActivationRecord`;
- operação: latência, falha, rate limit e custo;
- regressão entre versões de documento, prompt, modelo e índice.

Dataset, rubrica e thresholds iniciais pertencem ao `STATE-02`. O `STATE-07`
executa a campanha; qualquer revisão exige decisão formal registrada antes da
primeira execução que possa revelar resultados. Nenhum threshold pode ser
escolhido ou alterado depois de observar o resultado para fazê-lo passar.

## Matriz MVP × evolução

| Capacidade | MVP | Evolução |
|---|---|---|
| Acervo local | Um | Vários |
| Formato | PDF local + PDF oficial sincronizado | CSV, Markdown, HTML, Office e outros autorizados |
| Atualização | Substituição local e sincronização oficial manuais | Diff incremental e scheduler |
| Providers | Um por porta | Catálogo e múltiplas implementações |
| Índice | Geração imutável, uma anterior retida e rollback limitado | Migração, compactação e distribuição |
| Fontes online | Uma fonte oficial allowlisted e snapshot | Múltiplas fontes e catálogo |
| Acesso | Consulta anônima limitada | RBAC e escopo por corpus |
| Integração DB-Notifier | Nenhuma | Adapter ou módulo versionado |
