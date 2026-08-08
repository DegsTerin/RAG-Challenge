# Módulo RAG

## Status

Contrato arquitetural vigente para o MVP e sua evolução, reconciliado com os
ADRs aceitos. O estado implementado e testado pertence ao Current State e aos
relatórios dos estados; a semântica de armazenamento visual e idiomas dos
ADRs 0008/0009 permanece planejada e não implementada. Nenhum corpus real,
provider real ou conteúdo real está ativo.

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
Governed local document or official snapshot (PDF/CSV)
  -> Discovery
  -> Validation
  -> Content-addressed persistence and verified reopen
  -> Deterministic complete page rendering for PDF visual evidence
  -> Parsing
  -> Normalisation
  -> Chunking
  -> Embeddings
  -> Index generation
  -> Activation

Question
  -> Validation
  -> Question-language validation
  -> Active catalogue and coverage resolution
  -> Per-document availability/freshness validation
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
- idiomas de conteúdo declarados por tags BCP 47 canônicas; isso não amplia os
  idiomas de pergunta/resposta suportados;
- revisão declarada;
- referência lógica à geração de índice ativa, cujo registro canônico pertence
  exclusivamente ao `IIndexGenerationStore`.

O MVP possui um corpus lógico configurado. Ele contém um catálogo administrável
de bancos e documentos; isso não implementa gestão de múltiplos acervos.

Origem `LocalAuthorised` ou `OfficialExternal` integra proveniência, digests,
metadados vetoriais e citações, mas não divide a consulta em corpora
mutuamente exclusivos. Todos os documentos ativos e elegíveis participam da
recuperação padrão.

### Banco de dados e categoria

`DatabaseProductId` identifica uma entidade lógica independente de seu nome de
exibição. `DatabaseProductRevision` é imutável e associa zero ou mais categorias
por `DatabaseCategoryAssignment`. Categorias são muitos-para-muitos.

Bancos percorrem `Candidate → Active ↔ Deactivated`; de `Candidate` ou
`Deactivated` podem seguir para `Removed`. `Removed` é tombstone lógico
auditável. Um banco só pode ficar
`Active` quando ao menos um documento associado também estiver `Active` e
elegível. Retirar o último documento ativo exige desativação explícita e
atômica do banco.

### Documento e versão

`DocumentId` identifica um documento lógico associado a um
`DatabaseProductId`. Cada conteúdo imutável recebe `DocumentVersion` com:

- hash SHA-256;
- tamanho e media type;
- formato `Pdf` ou `Csv`;
- versão declarada quando disponível;
- data da fonte;
- data de ingestão;
- `contentLanguage` como `DocumentContentLanguage` BCP 47 canônico;
- `sourceDeclaredLanguage` exato quando publisher ou metadado embutido fornecer
  uma tag, sem inferir região ou script;
- `sourceAdapterId`;
- `SourceTrustClass`;
- locator sanitizado;
- estado de licenciamento/proveniência.

Documentos e versões usam `Candidate`, `Active`, `Deactivated` e `Removed` com
a mesma semântica do catálogo. Uma nova versão é candidata enquanto a anterior
permanece ativa. Desativação retira a versão da recuperação sem apagar
histórico; remoção é lógica e bytes só são eliminados após retenção e prova de
que nenhuma revisão ativa/retida os alcança.

Renomear um arquivo não deve criar silenciosamente uma nova identidade lógica
quando o catálogo já tiver um mapeamento estável. Conteúdo alterado sempre
produz nova versão.

Parser e configuração não alteram `DocumentVersion`. Um artefato textual
derivado é identificado separadamente por `DocumentVersion` mais descriptor
do parser/normalização e sua configuração não secreta; essa compatibilidade
integra a geração do índice.

O runtime v1 atualmente implementado conserva o tipo fechado
`SupportedLanguage` para `pt-BR` e `en-GB`. A separação em
`SupportedQueryLanguage` e `DocumentContentLanguage`, inclusive o valor `en`
do candidato PostgreSQL, requer implementação própria e o contrato v2
planejado; esta reconciliação não altera tipo, schema ou dados.

### Conteúdo e evidência visual de página

`IDocumentContentStore` é a única fonte de verdade binária para bytes exatos
da fonte e PNGs persistentes. Cada objeto é imutável, identificado pelo SHA-256
dos próprios bytes, gravado idempotentemente e reaberto com hash e tamanho
verificados. Git, Git LFS, `artifacts-local/`, catálogo e vector store não são
substitutos.

Para PDF, o perfil aceito `pdf-page-png-v1` produz uma imagem `image/png` por
página física, numerada a partir de 1, a 144 DPI, RGB de 8 bits, fundo branco,
aspect ratio preservado e dimensões de até 4.096 pixels por eixo. O binding
`DocumentPageImage` liga documento/versão, conteúdo fonte, página,
perfil/renderer e conteúdo PNG. `DocumentRenderManifest` registra o conjunto
completo e ordinal, page count, descriptors e digest canônico. Falha, lacuna,
duplicidade, limite excedido, assinatura inválida ou readback inconsistente
reprova a candidata inteira. CSV não recebe visualização implícita.

Importar ou renderizar não ativa conteúdo. Um PDF com evidência visual só
pode ficar `Active` quando direitos, objeto fonte, manifesto completo, todos os
PNGs e geração textual/indexada finalizada forem vinculados atomicamente.
Documento `Deactivated` ou `Removed` não serve imagem, e cleanup precisa provar
ausência de reachability por documento ativo/retido, manifesto, evidência de
resposta e rollback.

### Chunk

Cada chunk preserva:

- corpus, banco/revisão, documento/versão, formato e classe de confiança;
- `contentLanguage` herdado da versão documental;
- estratégia e versão de chunking;
- ordem;
- página/bloco para PDF ou linha/coluna/cabeçalho para CSV;
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
catalogueRevision
activeDocumentSetDigest
sourceBindingSetDigest
indexCompatibilityKey
generationSpecDigest
chunkCount
vectorCount
logicalArtifactDigest
```

O `activeDocumentSetDigest` cobre a lista ordinal de banco/revisão,
documento/versão e formato. O `sourceBindingSetDigest` cobre a projeção
generation-bound ordinal de banco/revisão, documento/versão/formato,
`sourceAdapterId`, trust, registro de fonte imutável/versionado e snapshot
imutável. `sourceObservationId` é excluído de `sourceBindingSetDigest`,
`generationSpecDigest`, digest do manifesto completo e `IndexGenerationId`.
A sobreposição entre os dois digests é deliberada: um prova o conjunto de
documentos e o outro prova origem, trust, registro e snapshot materializados
nos artefatos e citações.

`generationSpecDigest` é o SHA-256 da representação canônica dos sete primeiros
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

O binding completo pertence ao registro de ativação e é protegido por
`activationBindingSetDigest`. Esse digest cobre a mesma projeção ordinal de
fonte e acrescenta `sourceObservationId`; usa domínio canônico versionado
distinto, propriedades fixas, UTF-8, ordem ordinal e null inequívoco. Alterar
somente a observação muda o digest do registro, mas não os digests do
manifesto nem `IndexGenerationId`. Alterar snapshot, adapter, trust, registro
imutável, documento ou compatibilidade exige nova geração.

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
| `IVectorStore` | Escrever gerações imutáveis e consultar por `VectorSearchRequest` com `CorpusId`, `IndexGenerationId`, seletores generation-bound elegíveis e filtros administrativos opcionais; provar hard pre-filter antes do top-k e não gerir ativação. |
| `ILanguageModel` | Gerar resposta limitada ao prompt e às evidências. |
| `IDocumentContentStore` | Persistir e reabrir bytes imutáveis content-addressed de fontes, snapshots oficiais e PNGs de páginas. |
| `IDocumentCatalog` | Persistir identidades, versões, proveniência e estado. |
| `IIndexGenerationStore` | Persistir manifestos e ser a única fonte de verdade do `CorpusActivationRecord`, com compare-and-swap e rollback. |

Cada implementação declara identificador, versão, capacidades, limites e
configuração não secreta. O registro é estático por dependency injection no
MVP; plug-ins dinâmicos ficam fora do escopo.

O `IDocumentContentStore` usa gravação idempotente por hash, valida o conteúdo
reaberto e impede sobrescrita. Catálogo e manifesto guardam referências
estáveis; a política de retenção impede remover conteúdo fonte ou imagem
alcançável por documento/manifests/evidência ativos ou retidos e pelo único
alvo de rollback. Vector store guarda derivados e não substitui a fonte bruta
necessária para reconstrução.

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
catalogueRevision
activationBindingSetDigest
documentBindings[]
  databaseProductId/databaseProductRevision
  documentId/documentVersion/documentFormat
  sourceTrustClass/sourceAdapterId
  officialSourceRegistrationId?/sourceSnapshotId?/sourceObservationId?
evidenceBindings[]
  documentBinding completo
  sourceContentObjectId
  rightsSchemaVersion: 1
  rightsDecisions[10]
  renderManifestId? # obrigatório para PDF, ausente para CSV
generationActivatedAt
recordUpdatedAt
```

O compare-and-swap altera o registro inteiro em uma transação do plano de
controle que também preserva as representações completas anterior e nova no
histórico versionado de ativação e grava o evento de auditoria sanitizado. Cada
binding é ordenado ordinalmente. `activeDocumentSetDigest` e
`sourceBindingSetDigest`, sem observação, precisam corresponder ao manifesto;
`activationBindingSetDigest`, com observação, precisa corresponder ao registro.
Cada observação oficial já deve existir imutavelmente e nomear o mesmo registro
e snapshot do binding antes da transação. Falha ou conflito deixa registro e
histórico anteriores intactos; conteúdo, observações e vetores candidatos
permanecem órfãos auditáveis até cleanup explícito. A consulta lê o registro
corrente uma vez e não combina a geração com estado de catálogo ou “última
observação” obtidos separadamente.

Cada nova revisão persiste um binding de evidência imutável por documento: o
`DocumentBinding` e objeto fonte exatos, snapshot completo das dez decisões de
direitos em schema `1` e, para PDF, o render manifest exato. Esses campos não
criam identidade/revisão administrativa ou digest global de direitos e não
alteram os domínios de `sourceBindingSetDigest` ou
`activationBindingSetDigest`. Replay pelo mesmo `OperationId` compara também
todos os vínculos e decisões. Revisão histórica sem o conjunto completo é
preservada sem inferência, mas falha fechada como autoridade corrente de
consulta ou prontidão visual.

Antes do CAS, a implementação confere corpus, documento, versão, formato,
objeto fonte, idioma documental suportado, geração textual/vector finalizada e
bindings idênticos ao manifesto. CSV exige `TextualEvidence` integralmente
`Permitted`. PDF exige `PdfVisualEvidence` integralmente `Permitted`, manifesto
finalizado da mesma fonte, uma linha consecutiva por página física e reabertura
verificada da fonte e de todos os PNGs. A transação Control grava revisão,
bindings, evidência/direitos, retenção, head, auditoria e completion do journal
administrativo aplicável como uma única mudança atômica.

`catalogueRevision` identifica o snapshot imutável do catálogo que integra a
especificação da geração. O journal append-only de observações possui revisão
própria. Rebinding de freshness avança o journal e `recordRevision`, nunca a
`catalogueRevision`; versão transacional interna de uma linha também não se
confunde com essa revisão canônica.

## Fontes locais e externas

### Fontes locais autorizadas

Usa `sourceAdapterId=local-directory` e
`SourceTrustClass=LocalAuthorised`. O ID do adapter é extensível; a
classificação de confiança é fechada e não concede autorização por si só.

- raiz configurada e canonicalizada;
- sem acesso fora da raiz;
- allowlist de extensão/media type;
- limites por arquivo/operação de tamanho, páginas/linhas e concorrência, sem
  teto de produto para a quantidade total de documentos;
- conteúdo hashado antes da indexação;
- bytes validados promovidos idempotentemente ao `IDocumentContentStore` e
  reabertos com hash verificado antes de qualquer ativação;
- para PDF visual, renderização completa e persistência/reabertura verificadas
  de todos os PNGs antes de qualquer ativação;
- nenhuma dependência de `reference-materials/`.

### Fontes oficiais externas

Implementação separada com `SourceTrustClass=OfficialExternal` e
`sourceAdapterId` estável específico do adapter:

- somente HTTPS;
- qualquer quantidade de registros aprovados compatíveis com o adapter;
- scheme, domínio, porta, path e query exatos de cada PDF/CSV em allowlist;
- fonte pública anônima, sem userinfo, token/assinatura em query,
  `Authorization`, API key, client certificate ou credencial ambiente;
- redirects desativados no MVP;
- cada conexão física resolve e autoriza DNS/IP uma vez, conecta somente ao IP
  aprovado e preserva host/SNI sem nova resolução por hostname;
- validação TLS não pode buscar AIA, CRL ou OCSP fora da política; trust,
  revogação, downloads de cadeia e eventual material local são decididos no
  `STATE-02`, e qualquer destino auxiliar exige allowlist própria;
- timeout, máximo de bytes/páginas/linhas, media type/assinatura ou estrutura
  PDF/CSV, concorrência e rate limit;
- termos, licença e robots revisados antes da primeira sincronização;
- snapshot de conteúdo imutável com `sourceKey`, `snapshotId`, URL canônica,
  ETag/Last-Modified observados na captura, hash, `retrievedAt` e licença;
- bytes do snapshot persistidos pelo `IDocumentContentStore`;
- páginas PDF derivadas persistidas no mesmo content store somente depois de
  direitos específicos e autoridade de renderização;
- observações de revalidação append-only com `observationId`, `snapshotId`,
  validators condicionais enviados, status HTTP, ETag/Last-Modified
  observados, `revalidatedAt`, `maxAge`, resultado e evidência sanitizada;
- sincronização para snapshot governado antes da recuperação;
- origem local/oficial visível sem separar o espaço padrão de recuperação.

O conteúdo de cada snapshot nunca muda. O vínculo configurado da fonte possui
estado `Current`, `Stale`, `Withdrawn` ou `Deactivated`, derivado da observação
apontada pelo binding do `CorpusActivationRecord`, e não simplesmente da última
observação gravada. Conteúdo expirado, retirado ou desativado não é apresentado
como atual; status e frescor acompanham a citação. A política padrão do MVP
falha fechada para o documento quando o registro ativo não vincula snapshot e
observação elegível `Current`; os demais documentos ativos continuam elegíveis
e a cobertura degradada é explícita.

A consulta não recebe acesso irrestrito à web. O conteúdo sincronizado não
altera políticas, prompts de sistema ou autorização.

Sincronização oficial é um caso de uso administrativo manual por registro:

1. carregar `OfficialSourceRegistrationId` aprovado; nenhuma URL vem da
   pergunta;
2. canonicalizar a URL pública sem credenciais, validar a allowlist específica,
   resolver A/AAAA e rejeitar respostas mistas/proibidas; conectar ao IP
   aprovado preservando host/SNI, sem egress lateral da validação TLS;
3. fazer request condicional usando os validators do binding ativo e persistir
   os validators enviados/recebidos e o status; redirects ficam desativados;
4. em `304` ou hash idêntico, persistir observação imutável e, somente quando
   ela nomear o mesmo registro imutável e snapshot do manifesto ativo, criar
   nova revisão completa do registro com `sourceObservationId` e
   `activationBindingSetDigest` recalculado; publicar por compare-and-swap com
   auditoria atômica ou falhar fechado;
5. para conteúdo novo, baixar para quarentena, limitar bytes e trabalho de
   parser, validar PDF/CSV, calcular hash, persistir/reabrir o snapshot e criar
   uma versão documental `Candidate`;
6. construir e validar geração candidata com o novo conjunto ordenado;
7. em uma transação, ativar banco/documento quando aplicável e trocar o
   `CorpusActivationRecord` completo com auditoria sanitizada.

Uma resposta autoritativa `404`/`410`, quando assim definida pela política da
fonte, cria observação `Withdrawn` vinculada ao snapshot ativo. Uma operação
administrativa explícita e auditada cria observação `Deactivated` sem fetch.
Nos dois casos, o compare-and-swap muda somente o binding do registro
compatível, o digest/revisão do registro e a auditoria. Preserva manifesto,
`sourceBindingSetDigest`, `generationSpecDigest`, `IndexGenerationId`,
`catalogueRevision`, `generationActivatedAt` e snapshot quando o documento
deixa de ser elegível apenas por freshness; nenhuma reindexação ocorre. Falha
transitória de DNS/transporte/`5xx` registra a tentativa, mas não substitui uma
observação `Current`; o snapshot passa a `Stale` pelo `maxAge`. Voltar a
`Current` exige nova sincronização/revalidação elegível e, após
`Deactivated`, reativação administrativa explícita.

Falha transitória ou sincronização rejeitada nunca altera geração, snapshot ou
observação ativos. Um snapshot anterior pode continuar servindo somente
enquanto `Current`; após `maxAge`, ele deixa a recuperação e a resposta expõe
cobertura degradada, sem apresentar outra origem como substituta silenciosa.

## Estratégia do MVP para atualização

O MVP mantém o fluxo simples:

1. resolver bancos/documentos ativos mais candidatos explicitamente escolhidos
   e os snapshots oficiais vinculados;
2. validar formato, proveniência, licença, identidade e hash de cada documento;
3. persistir ou reutilizar o objeto imutável por hash, reabri-lo pelo
   `IDocumentContentStore` e conferir seus bytes;
4. para PDF com evidência visual, finalizar e verificar o manifesto completo
   de páginas antes de considerar a candidata visualmente completa;
5. construir uma geração única com todos os chunks elegíveis e metadados de
   banco, documento, formato, origem e confiança;
6. validar manifesto, referências reabríveis, compatibilidade, elegibilidade,
   cobertura, os dois domínios de binding e smoke queries;
7. trocar por compare-and-swap o `CorpusActivationRecord` completo no
   `IIndexGenerationStore`, incluindo todos os bindings documentais, fontes,
   snapshots de direitos e render manifests aplicáveis;
8. manter a geração ativa e ao menos uma geração anterior validada até cleanup
   explícito após a janela de rollback definida.

O MVP pode reconstruir a geração completa. Ele não precisa implementar diff
por chunk, scheduler, fila ou sincronização distribuída.

Cada pergunta recupera, por padrão, em todos os bindings ativos e elegíveis do
registro resolvido. Origem não é um filtro implícito; filtros administrativos
opcionais por banco/documento, quando introduzidos, devem ser explícitos.

Invariantes da geração conjunta:

- uma candidata representa snapshot coerente do catálogo inteiro selecionado;
- toda atualização preserva bancos/documentos não alterados por identidade e
  versão no novo manifesto;
- atualizações de conteúdo são serializadas por corpus;
- um banco ativo possui ao menos um documento ativo/elegível;
- a saída do último documento ativo exige desativação explícita e atômica do
  banco;
- `VectorSearchRequest` exige `CorpusId` e `IndexGenerationId`; filtros
  administrativos declarados e os seletores de bindings elegíveis derivados do
  registro resolvido também são aplicados antes do top-k;
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

Rollback de índice usa como alvo uma geração anterior preservada, validada e
sua projeção generation-bound completa. A operação nunca restaura bytes de um
`CorpusActivationRecord` histórico. Ela constrói nova revisão corrente,
recalcula `activationBindingSetDigest` e publica por compare-and-swap. Não edita
vetores no lugar nem combina a geração anterior com um vínculo oficial
arbitrário.

Para cada registro/snapshot oficial do alvo, a transação administrativa recebe
e valida explicitamente uma observação append-only já existente, compatível e
elegível pela política atual; não seleciona “a mais recente” implicitamente. Se
o conjunto não mantiver cada banco ativo com evidência elegível, o rollback é
rejeitado sem alterar o registro corrente. Observações históricas nunca têm
timestamp, `maxAge` ou estado reescritos; corrigir uma observação exige novo
append e nova revisão de ativação.

O rollback também recebe todos os bindings de evidência atuais e revalida
direitos, objetos fonte, geração textual/vector e render manifests; não copia
cegamente o snapshot histórico. Um rebind exclusivamente de freshness só
preserva os bindings imutáveis quando documento, versão, geração e manifesto
permanecem idênticos.

Rollback de documento seleciona uma versão anterior e cria nova candidata para
o manifesto completo. Uma geração anterior só pode ser reativada quando o
conjunto generation-bound completo e a chave de compatibilidade correspondem
ao alvo e as observações selecionadas satisfazem a política atual; reativação
nunca torna snapshot antigo novamente `Current`.

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
5. construção e compare-and-swap de novo registro apontando para a geração
   anterior, com observações compatíveis e atualmente elegíveis;
6. validação dos digests do documento, da fonte generation-bound e do binding
   completo, com atomicidade entre registro, observação e evento de auditoria;
7. crash antes, durante e depois de cada fronteira de persistência;
8. preservação dos registros completos históricos sem replay de freshness;
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
- Exigir `questionLanguage=pt-BR|en-GB` como `SupportedQueryLanguage` e
  validá-lo antes de qualquer chamada
  externa; não inferir silenciosamente outro idioma em perguntas curtas ou
  ambíguas.
- Não aceitar URL, domínio, banco, documento, origem ou adapter como campo de
  autoridade na pergunta.
- Resolver o `CorpusActivationRecord` uma única vez no início da consulta.
- Resolver todos os bindings ativos/current e a cobertura antes de gerar o
  query embedding ou chamar qualquer provider.
- Usar `CorpusId`, `IndexGenerationId` e os seletores generation-bound dos
  bindings elegíveis, todos derivados do registro resolvido, em um
  `VectorSearchRequest` durante toda recuperação, validação e citação; nenhuma
  etapa relê silenciosamente o registro.
- Usar top-k e thresholds definidos por avaliação, não por palpite.
- Aplicar `CorpusId`, `IndexGenerationId`, bindings elegíveis e filtros
  administrativos opcionais antes do top-k/ranking.
- Separar claramente instruções confiáveis de evidências não confiáveis.
- Instruir o modelo a gerar a resposta exatamente em `questionLanguage`,
  mesmo quando `contentLanguage` das evidências for diferente.
- Limitar número e tamanho total dos trechos.
- Exigir referência de cada afirmação factual relevante.
- Rejeitar citação que não pertença ao conjunto recuperado.
- Não preencher lacunas com conhecimento paramétrico não citado.
- Retornar `INSUFFICIENT_EVIDENCE` quando não houver suporte suficiente.

O modelo não recebe acesso direto ao vetor, arquivo, rede ou catálogo. A
Application seleciona e limita as evidências.

Documento oficial stale/indisponível não participa da recuperação e aparece na
cobertura degradada. A consulta continua somente quando existe ao menos um
documento ativo/elegível, sem afirmar que outra origem substituiu a ausente.

### Readiness por capacidade

- Liveness depende apenas da capacidade do processo responder.
- Readiness global exige plano de controle, geração ativa compatível, ao menos
  um banco/documento servível, vector store, embedding de query e LLM.
- Fontes/documentos `Stale|Unavailable|Withdrawn|Deactivated` produzem
  degradação de cobertura e não tornam a instância indisponível enquanto outro
  documento ativo/elegível permanece servível.
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
- `databaseProductId` e `databaseProductRevision`;
- `indexGenerationId`;
- `documentId`;
- `documentVersion`;
- `documentFormat`;
- `contentLanguage`;
- chunk ID;
- `sourceAdapterId` e `SourceTrustClass`.

Quando disponíveis, também inclui:

- título;
- página/bloco para PDF ou linha/coluna/cabeçalho para CSV;
- locator seguro para exibição.

Título, seção, trecho, rótulo de página e qualquer outro texto derivado da
fonte permanecem no `DocumentContentLanguage` original. A geração pode explicar
a evidência no idioma da pergunta, mas não reescreve nem traduz o conteúdo
apresentado como citação. No contrato v1 implementado, `contentLanguage`
continua fechado em `pt-BR|en-GB`; uma tag mais ampla não é coagida nem ativada
por essa superfície.

O contrato v2 aceito permanece planejado e não implementado. Ele conserva
`questionLanguage`/`answerLanguage` fechados, amplia `CitationV2.contentLanguage`
para BCP 47, preserva `sourceDeclaredLanguage` e adiciona referências
`PageImageEvidenceV1`. A resposta não embute PNG nem path; no máximo cinco
páginas distintas citadas são referenciadas, e um futuro endpoint same-origin
revalida binding ativo/manifests antes de servir bytes limitados. Evidência
textual adjacente continua acessível. O LLM recebe somente texto; imagem exige
autoridade separada de provider/egress/dados/custo.

A resposta inclui metadados técnicos:

- resumo de `evidenceCoverage` e origens efetivamente citadas;
- `indexGenerationId`;
- `retrievalPolicyVersion`;
- `promptVersion`;
- `answerLanguage`, sempre igual ao `questionLanguage` aceito;
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
- PDFs e CSVs são entrada não confiável; parsing ocorre com limites. Anexos,
  ações, links, fórmulas e instruções embutidas não recebem autoridade nem são
  executados.
- Renderização PDF ocorre com limites de bytes, páginas, tempo, memória,
  dimensões e concorrência; saída e manifests são revalidados antes de servir.
- Direitos de renderização, derivados, retenção, exibição e distribuição são
  independentes da permissão de ler, indexar ou citar texto.
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
- idioma: resposta no idioma da pergunta e citação no idioma original;
- segurança: prompt injection e conteúdo malicioso;
- cobertura por banco, documento e formato, com casos proporcionais ao conjunto
  ativo;
- busca adversarial em que chunks de banco/documento excluído por filtro
  explícito pontuam acima dos corretos, provando pre-filter antes do top-k;
- busca adversarial com chunks de outra geração e, quando aplicável, de outro
  corpus pontuando acima dos corretos, provando isolamento antes do top-k;
- SSRF, redirect, domínio/path, tamanho e freshness da fonte oficial;
- vetores canônicos provam que mudar somente `sourceObservationId` altera
  `activationBindingSetDigest`, sem alterar `sourceBindingSetDigest`,
  `generationSpecDigest` ou `IndexGenerationId`; mudar snapshot, adapter,
  trust ou registro imutável exige geração nova;
- `304` ou hash idêntico atualiza observação e cria nova revisão completa do
  registro sem novo snapshot ou índice somente quando a observação nomeia o
  registro/snapshot compatível; os campos preservados e alterados seguem
  ADR-0007;
- mismatch entre observação e registro/snapshot falha antes da ativação; retry
  depois de conflito é idempotente e não usa “última observação” implícita;
- degradação de uma fonte enquanto outras permanecem servíveis, seguida de
  revalidação `304`, preservando snapshot e restaurando elegibilidade sem
  mistura de geração;
- crash antes, durante e depois do append da observação, cálculo dos digests,
  auditoria e compare-and-swap, provando atomicidade do
  `CorpusActivationRecord`;
- rollback cria registro novo com observações compatíveis e elegíveis, preserva
  históricos e falha fechado quando a invariante de evidência não pode ser
  satisfeita;
- operação: latência, falha, rate limit e custo;
- regressão entre versões de documento, prompt, modelo e índice.

A suíte determinística cobre a matriz completa entre idioma da pergunta e
idioma da evidência: `pt-BR→pt-BR`, `en-GB→en-GB`, `pt-BR→en-GB` e
`en-GB→pt-BR`. Quando o corpus real aprovado não contiver um dos idiomas de
evidência, testes unitários, de contrato e de integração usam fixtures
sintéticas autorizadas e claramente separadas do corpus do produto. Essa
matriz não decide o idioma visual da interface.

Cada outro `DocumentContentLanguage` presente no corpus pontuado cria um
estrato exato adicional por idioma de evidência, sem agrupamento silencioso.
Para o candidato PostgreSQL `en`, a campanha deve separar ao menos
`pt-BR→en` e `en-GB→en`; essas linhas não contam como evidência `en-GB` e não
substituem a matriz obrigatória. Relatórios nomeiam tags, documentos, dataset,
provider e ambiente exatos.

Dataset, rubrica e thresholds iniciais pertencem ao `STATE-02`. O `STATE-07`
executa a campanha; qualquer revisão exige decisão formal registrada antes da
primeira execução que possa revelar resultados. Nenhum threshold pode ser
escolhido ou alterado depois de observar o resultado para fazê-lo passar.

## Matriz MVP × evolução

| Capacidade | MVP | Evolução |
|---|---|---|
| Corpus lógico | Um, com catálogo administrável | Vários com autorização/RBAC próprios |
| Bancos e documentos | 51 iniciais; cardinalidade aberta por registros | Novos itens compatíveis sem mudança do núcleo |
| Formato | PDF e CSV | Markdown, HTML, Office e outros autorizados |
| Atualização | Administração e sincronização oficiais manuais | Diff incremental e scheduler |
| Providers | Um por porta | Catálogo e múltiplas implementações |
| Índice | Geração imutável, uma anterior retida e rollback limitado | Migração, compactação e distribuição |
| Fontes online | Registros oficiais allowlisted e snapshots | Novas classes de autenticação/protocolo por decisão própria |
| Acesso | Consulta anônima limitada | RBAC e escopo por corpus |
| Integração DB-Notifier | Nenhuma | Adapter ou módulo versionado |
