# Visão do Projeto RAG-Challenge

## Contexto

Os materiais do Challenge da Alura/ONE descrevem uma empresa que possui
documentos difíceis de consultar e precisa de um agente capaz de responder
perguntas em linguagem natural. A entrega mínima exige código organizado em
repositório público, um agente funcional fundamentado em documento, README e
evidência de deploy usando ao menos um serviço OCI.

O produto será especializado em documentação sobre bancos de dados. Os
exemplos originais BimBam Buy, Santo Pegasus e Mercado Central 24h permanecem
como referência local e não constituem o acervo do produto.

## Problema

Profissionais precisam localizar rapidamente diferenças, características e
orientações presentes em documentação extensa de bancos de dados. Buscas
manuais consomem tempo, e respostas de modelos sem evidência podem inventar
informações.

## Proposta de valor

Fornecer uma experiência simples de perguntas e respostas que:

- pesquisa um acervo controlado;
- recupera trechos relevantes;
- gera uma resposta limitada às evidências;
- aceita perguntas em `pt-BR` e `en-GB` e responde no idioma declarado da
  pergunta;
- apresenta citações rastreáveis;
- preserva nas citações o idioma original do conteúdo referenciado;
- declara quando a evidência é insuficiente.

## Classificação

| Dimensão | Classificação inicial |
|---|---|
| Fase | Discovery de um MVP |
| Porte | Pequeno, com arquitetura de produto evolutivo |
| Criticidade | Moderada |
| Exposição | Local no desenvolvimento; pública no deploy do RAG-Challenge |
| Dados | Acervo publicável e perguntas de usuários não confiáveis |
| Disponibilidade | Best effort no MVP |
| Arquitetura candidata | Monólito modular com API e interface web |
| Modelo de entrega | Aplicação independente implantável em OCI |

## Usuários e stakeholders

- participante que desenvolve e mantém o RAG-Challenge;
- avaliador que instala, consulta e verifica a entrega;
- pessoa interessada em documentação de bancos de dados;
- futuro mantenedor do DB-Notifier, somente quando uma integração separada for
  autorizada.

O proprietário do projeto decide prioridades, aceita Human Gates, escolhe a
licença e autoriza ações externas.

## Objetivos

- Entregar um MVP local e online comprovadamente funcional.
- Produzir respostas grounded, com fontes e recusa segura.
- Suportar perguntas e respostas em `pt-BR` e `en-GB`, com a resposta no
  idioma declarado da pergunta e sem traduzir o conteúdo citado.
- Manter baixo acoplamento entre domínio e provedores de IA.
- Permitir trocar embeddings, banco vetorial e modelo sem reescrever os casos
  de uso.
- Permitir administrar bancos e versões documentais, construir candidatos e
  ativar um conjunto coerente sem interromper a geração vigente.
- Entregar consulta unificada a documentos PDF/CSV locais autorizados e fontes
  oficiais controladas, sem navegação livre.
- Preparar, sem implementar agora, múltiplos acervos e atualização incremental
  agendada.
- Preservar um caminho de integração futura ao DB-Notifier sem dependência
  direta.

## Escopo do MVP

- Um corpus lógico configurado com catálogo administrativo de bancos e
  documentos.
- Catálogo inicial canônico de 51 bancos de dados e 54 associações a 9
  categorias, sem hard-code no produto.
- Qualquer quantidade de documentos PDF/CSV locais autorizados ou oficiais por
  banco, desde que cada banco ativo possua ao menos um documento ativo.
- Um adapter de parser por formato inicial: PDF e CSV.
- Sincronização manual server-side de cada fonte oficial aprovada para snapshot
  imutável, com proveniência, frescor e rollback.
- Recuperação unificada de todos os documentos ativos. Origem local/oficial e
  classe de confiança permanecem visíveis em metadados e citações.
- Uma estratégia versionada de normalização e chunking compartilhada.
- Um provider de embeddings.
- Um banco ou índice vetorial.
- Um modelo de linguagem.
- Consulta por API e uma interface web mínima.
- Contrato OpenAPI v1 versionado pertencente ao RAG-Challenge.
- Resposta com citações e resultado explícito de evidência insuficiente.
- Idioma da pergunta explicitamente declarado como `pt-BR` ou `en-GB`, com a
  resposta no mesmo idioma e citações preservadas no idioma da evidência.
- Interface web disponível em `pt-BR` e `en-GB`, com seleção explícita e
  independente do idioma da pergunta.
- Temas visuais `Light` e `Dark`, com seleção explícita e independente dos
  idiomas da interface, pergunta, resposta e evidência.
- Catálogo local de metadados, versão de documento e geração de índice.
- Armazenamento durável e content-addressed dos bytes necessários a rebuild e
  rollback.
- Administração local de bancos e documentos, indexação candidata e ativação
  explícita e segura.
- Execução local reproduzível.
- Deploy autorizado em OCI e evidência da execução.
- Testes, observabilidade mínima e documentação pública.

## Fora do escopo do MVP

- Prometer cobertura de todos os bancos de dados existentes além do catálogo
  administrativamente ativado.
- Ingerir Word, Excel, PowerPoint, Markdown, JSON, HTML ou outros formatos além
  de PDF e CSV sem adapter e decisão compatíveis.
- Upload público e administração remota de acervos.
- Mais de um acervo ativo.
- Sincronização incremental agendada e distribuída.
- URL arbitrária, crawling genérico ou consulta direta à internet durante cada
  pergunta.
- Autenticação corporativa, RBAC completo ou multi-tenancy.
- Microserviços, filas distribuídas ou loader dinâmico de plug-ins.
- Integração executável com o DB-Notifier.
- Start, Stop, administração ou conexão com bancos de dados reais.

## Acervo inicial

O acervo lógico é `Catálogo de Bancos de Dados — MVP`. Sua revisão inicial
possui 51 entidades únicas e 54 associações em 9 categorias. Categorias são
muitos-para-muitos; Redis, SAP HANA e SingleStore são entidades únicas em duas
categorias cada.

| Categoria | Bancos canônicos |
|---|---|
| Relacionais (SQL) | PostgreSQL; MySQL; MariaDB; Microsoft SQL Server; Oracle Database; SQLite; IBM Db2; SAP HANA; Firebird; Teradata; CockroachDB; YugabyteDB; SingleStore; TiDB; Amazon Aurora |
| Documentos (NoSQL) | MongoDB; Couchbase; CouchDB; RavenDB; Amazon DocumentDB; Azure Cosmos DB |
| Chave-valor | Redis; Valkey; Amazon DynamoDB; Riak KV; Aerospike |
| Wide-column | Apache Cassandra; ScyllaDB; Apache HBase; Google Bigtable |
| Grafos | Neo4j; Amazon Neptune; TigerGraph; JanusGraph; ArangoDB |
| Busca | Elasticsearch; OpenSearch; Apache Solr |
| Séries temporais | InfluxDB; TimescaleDB; QuestDB; VictoriaMetrics |
| Data Warehouse / Analytics | Snowflake; Google BigQuery; Databricks SQL; Amazon Redshift; ClickHouse; Vertica; DuckDB; Apache Doris; StarRocks |
| Em memória | Redis; SAP HANA; SingleStore |

A lista é dado canônico inicial, não enum, constante ou condição hard-coded.
O administrador pode acrescentar bancos e documentos compatíveis sem mudança
de código ou ADR por item. Cada inclusão registra proveniência, licença,
idioma, fonte/URL allowlisted quando externa, snapshot imutável, hash, adapter,
validação, indexação candidata e ativação. Uma nova classe de formato,
protocolo, autenticação ou confiança pode exigir implementação e decisão
arquitetural própria.

Não existe teto de produto para bancos, documentos ou páginas. Cada versão é
finita, registra suas contagens observadas e deve caber com segurança no
ambiente homologado. Limites de arquivo, linha, página, memória, tempo e
concorrência são controles operacionais e não limites do catálogo.

## Requisitos funcionais

| ID | Requisito | MVP |
|---|---|---|
| `RF-001` | Carregar documentos PDF/CSV autorizados sem depender de `reference-materials/`. | Sim |
| `RF-002` | Validar tipo, tamanho, identidade e integridade do documento antes do processamento. | Sim |
| `RF-003` | Extrair conteúdo de PDF e CSV e produzir chunks com localização e metadados de origem específicos do formato. | Sim |
| `RF-004` | Gerar embeddings e construir uma geração de índice identificável. | Sim |
| `RF-005` | Consultar o índice com uma pergunta em linguagem natural. | Sim |
| `RF-006` | Gerar resposta somente a partir dos trechos recuperados. | Sim |
| `RF-007` | Retornar citações com documento, versão e localização disponível. | Sim |
| `RF-008` | Retornar `INSUFFICIENT_EVIDENCE` quando a recuperação não sustentar a resposta. | Sim |
| `RF-009` | Versionar manualmente um documento e construir candidata sem destruir previamente a versão ativa. | Sim |
| `RF-010` | Expor liveness, readiness e diagnóstico sanitizado de dependências. | Sim |
| `RF-011` | Executar localmente por procedimento documentado. | Sim |
| `RF-012` | Executar em OCI e produzir evidência verificável. | Sim |
| `RF-013` | Adicionar, remover, versionar, ativar e desativar múltiplos acervos. | Futuro |
| `RF-014` | Sincronizar alterações por documento de forma incremental e agendada. | Futuro |
| `RF-015` | Trocar embeddings, armazenamento vetorial e LLM por configuração/composição. | Preparado; uma implementação no MVP |
| `RF-016` | Sincronizar manualmente cada fonte oficial registrada por adapter compatível, allowlist e snapshot versionado, preservando URL e frescor. | Sim |
| `RF-017` | Publicar no MVP o contrato HTTP/OpenAPI versionado do RAG-Challenge; qualquer adapter consumidor, inclusive do DB-Notifier, pertence ao repositório consumidor e a gates próprios futuros. | Contrato no MVP; adapters consumidores no futuro |
| `RF-018` | Processar PDF e CSV por adapters próprios sem alterar os casos de uso do núcleo; formatos adicionais permanecem futuros. | Sim para PDF/CSV |
| `RF-019` | Aplicar RBAC e escopo por corpus antes da recuperação. | Futuro |
| `RF-020` | Recuperar por padrão em todos os documentos ativos, registrar a proveniência local/oficial de cada evidência e nunca substituir silenciosamente uma fonte indisponível por outra. | Sim |
| `RF-021` | Aceitar perguntas com idioma declarado `pt-BR` ou `en-GB`, responder no mesmo idioma e preservar no idioma original todo conteúdo derivado da fonte exibido em citações. | Sim |
| `RF-022` | Permitir selecionar `pt-BR` ou `en-GB` para a interface e localizar todo texto visual pertencente ao produto sem alterar `questionLanguage`, `answerLanguage` ou conteúdo citado. | Sim |
| `RF-023` | Permitir selecionar o tema visual `Light` ou `Dark` sem alterar `interfaceLanguage`, `questionLanguage`, `answerLanguage`, evidência ou citações. | Sim |
| `RF-024` | Permitir ao administrador adicionar, versionar, ativar, desativar e remover logicamente bancos do catálogo, com estado Candidate antes da ativação. | Sim |
| `RF-025` | Permitir qualquer quantidade de documentos por banco e administrar suas versões/estados; cada banco ativo exige ao menos um documento ativo e todos os documentos ativos participam da recuperação. | Sim |

## Requisitos não funcionais

| ID | Requisito |
|---|---|
| `RNF-001` | O núcleo não depende de SDK de IA, parser, vetor, UI, transporte ou DB-Notifier. |
| `RNF-002` | Configuração é tipada, validada no startup e falha fechada. |
| `RNF-003` | Segredos não entram no repositório, logs, respostas ou evidências. |
| `RNF-004` | Operações externas têm timeout, cancelamento e limites de tamanho/custo. |
| `RNF-005` | Documento, conteúdo bruto, snapshot, chunk, provider e índice têm proveniência, identidade e versão rastreáveis. A identidade da geração cobre o binding de fonte sem observação; cada revisão do registro de ativação cobre separadamente o binding completo com `sourceObservationId`. Bytes imutáveis permanecem reabríveis enquanto forem necessários para rebuild ou rollback. |
| `RNF-006` | Logs são estruturados, sanitizados e correlacionáveis. |
| `RNF-007` | O produto diferencia indisponibilidade, conteúdo inválido e evidência insuficiente. |
| `RNF-008` | Testes são determinísticos e não exigem serviços pagos na suíte padrão. |
| `RNF-009` | A interface mínima atende teclado, contraste e estados de loading, vazio e erro. |
| `RNF-010` | Build, dependências e toolchains são reproduzíveis e versionados. |
| `RNF-011` | O clone público não depende de arquivos ignorados ou dados privados. |
| `RNF-012` | Mudanças de documento ou provider não exigem refatoração do núcleo. |
| `RNF-013` | O repositório público possui estrutura compreensível e histórico incremental de commits. |
| `RNF-014` | O egress da fonte oficial falha fechado, mantém os escopos distinguíveis e aplica HTTPS, allowlist, limites, pinning da conexão ao DNS/IP autorizado, redirects bloqueados, validação TLS sem destinos laterais e proteção SSRF. |
| `RNF-015` | Contratos, recuperação e geração tratam `pt-BR` e `en-GB` por tags BCP 47 explícitas; a homologação cobre perguntas e evidências no mesmo idioma e nas duas direções cruzadas. |
| `RNF-016` | A interface não mistura idiomas em textos pertencentes ao produto, conserva acessibilidade nas duas localizações e mantém o idioma visual independente do idioma da consulta. |
| `RNF-017` | Os temas `Light` e `Dark` preservam contraste, foco visível, semântica, reflow e todos os estados da interface, sem comunicar informação somente por cor. |
| `RNF-018` | Bancos, categorias, documentos e fontes compatíveis são registros administráveis, não listas hard-coded; inclusão por item não exige código nem ADR, mas uma nova classe de integração pode exigir ambos. |

## Critérios de aceitação do MVP

| ID | Critério |
|---|---|
| `AC-MVP-001` | Um clone limpo pode ser configurado, compilado, testado e executado pelo procedimento publicado. |
| `AC-MVP-002` | Cada documento autorizado é persistido/reaberto por hash, processado e incorporado a uma candidata validada; staging parcial permanece não consultável. |
| `AC-MVP-003` | Perguntas representativas aprovadas recuperam citações corretas. |
| `AC-MVP-004` | Perguntas fora do acervo não recebem resposta factual inventada. |
| `AC-MVP-005` | Mudança de banco ou documento cria candidata validada e ativa atomicamente o manifesto completo com todos os bindings documentais aplicáveis. A ativação valida `activeDocumentSetDigest`, o `sourceBindingSetDigest` generation-bound e o `activationBindingSetDigest` completo; preserva geração anterior elegível e testa rollback por novo registro, sem replay de freshness histórica. |
| `AC-MVP-006` | Nenhum secret ou material local ignorado integra o repositório. |
| `AC-MVP-007` | Os checks automáticos aplicáveis ao estado são aprovados. |
| `AC-MVP-008` | A aplicação é executada em OCI com link ou evidência visual sanitizada. |
| `AC-MVP-009` | O README contém arquitetura, tecnologias, execução e exemplos reais verificados após a implementação. |
| `AC-MVP-010` | A interface mínima permite perguntar, consultar citações e compreender loading, vazio, erro, indisponibilidade e evidência insuficiente, com teclado, foco e contraste adequados. |
| `AC-MVP-011` | A API expõe consulta, health e artefato OpenAPI v1 versionados, com configuração fail-closed, limites, cancelamento, metadados reproduzíveis, erros canônicos e diagnóstico sanitizado; compatibilidade do contrato é testada. |
| `AC-MVP-012` | Testes de arquitetura e contrato demonstram que Domain/Application não dependem de SDKs ou adapters concretos e que providers são compostos pelas bordas. |
| `AC-MVP-013` | O repositório público possui estrutura compreensível e histórico incremental de commits, sem secrets ou materiais locais ignorados. |
| `AC-MVP-014` | A sincronização autorizada de cada fonte allowlisted produz snapshot/observação versionados. Alteração de conteúdo exige candidata; `304`/hash idêntico para o mesmo registro/snapshot cria nova revisão completa do registro e `activationBindingSetDigest`, mas preserva manifesto, `sourceBindingSetDigest`, `generationSpecDigest`, `IndexGenerationId`, `catalogueRevision` e `generationActivatedAt`. Mismatch falha fechado; citações expõem fonte, URL pública quando aplicável, snapshot e frescor. |
| `AC-MVP-015` | Cada fonte externa rejeita domínio, IP, porta, path, query, resposta DNS mista, redirect ou destino TLS lateral fora de sua política. Consulta não faz fetch e considera somente documentos ativos/current, expondo cobertura degradada sem fallback silencioso. |
| `AC-MVP-016` | Perguntas declaradas como `pt-BR` recebem respostas em `pt-BR`, perguntas declaradas como `en-GB` recebem respostas em `en-GB`, e citações não traduzem título, seção, trecho ou outro conteúdo proveniente da fonte. Testes determinísticos cobrem `pt-BR→pt-BR`, `en-GB→en-GB`, `pt-BR→en-GB` e `en-GB→pt-BR` entre idioma da pergunta e idioma da evidência. |
| `AC-MVP-017` | A pessoa consegue alternar explicitamente a interface entre `pt-BR` e `en-GB`; labels, instruções, validações e estados pertencentes ao produto usam integralmente o idioma visual selecionado. Testes de componente e fluxo cobrem cada idioma da interface combinado com cada `questionLanguage`, sem traduzir citações. |
| `AC-MVP-018` | A pessoa consegue alternar explicitamente a interface entre `Light` e `Dark`; conteúdo, idioma e contexto da consulta permanecem inalterados. Testes de componente, acessibilidade e fluxo executam nos dois temas as quatro combinações entre `interfaceLanguage` e `questionLanguage`, totalizando oito combinações, e validam contraste, foco, estados e ausência de informação dependente apenas de cor. |
| `AC-MVP-019` | O catálogo inicial contém exatamente 51 entidades e 54 associações nas 9 categorias aprovadas, preservando Redis, SAP HANA e SingleStore como entidades únicas multiclasse. |
| `AC-MVP-020` | Banco/documento novo começa Candidate; somente validação e ativação explícita permitem consulta. Desativação preserva histórico; remoção é lógica; o último documento ativo só pode sair em operação que também desative explicitamente o banco. |

## Premissas

- A primeira interface será simples; o valor principal é o fluxo RAG.
- O acesso a perguntas pode ser anônimo no MVP, conforme os materiais do
  Challenge.
- Operações administrativas de ingestão não serão expostas anonimamente.
- A sincronização oficial é manual e administrativa; uma pergunta pública não
  inicia crawling nem escolhe URL.
- Fontes oficiais iniciais são publicamente acessíveis sem autenticação; URL,
  headers e query não carregam token, assinatura ou credencial.
- Um serviço OCI de hospedagem é suficiente para o requisito mínimo, desde que
  a execução seja real e documentada.
- As tecnologias sugeridas pelo curso são opcionais.
- A interface suporta `pt-BR` e `en-GB` por decisão separada do suporte
  bilíngue de consulta; seleção inicial, persistência e fallback permanecem
  detalhes futuros próprios.
- A interface suporta `Light` e `Dark` por decisão própria; tema inicial,
  preferência do sistema, persistência e fallback permanecem detalhes futuros
  do frontend.

## Limitações e evidências pendentes

- A licença MIT do repositório foi aceita e materializada. ADR-0004 aceitou
  `CC BY 4.0` para documentos autorais futuros, mas nenhum documento inicial
  de produto, concessão de licença, idioma ou direito por banco foi
  materializado.
- PostgreSQL 18 é a primeira fonte oficial candidata verificada. Cada registro
  adicional ainda exige URL canônica, termos/licença, `maxAge`, limites,
  autoridade de rede e ativação próprios.
- ADR-0005 aceitou condicionalmente OpenAI para embeddings e LLM,
  `SqliteExactVectorStore`, EF Core SQLite e filesystem content-addressed.
  Versões exatas de packages, conta, entitlement, custo, qualidade bilíngue,
  desempenho, restart, backup e restore permanecem sem evidência executável.
- ADR-0006 aceitou as quatro políticas de egress deny-by-default e o limite de
  divulgação. Nenhum egress, provider, conta ou destino está habilitado por
  essa decisão.
- ADR-0005 aceitou condicionalmente o alvo OCI em `sa-saopaulo-1`; capacidade,
  entitlement, IAM, cobrança, consistência de backup e restore da tenancy
  permanecem não verificados.
- O dataset de avaliação e os thresholds de recuperação/groundedness ainda
  precisam ser materializados antes da primeira campanha pontuada.
- ADR-0007 foi aceito e sua semântica foi reconciliada nos contratos
  documentais. `AQG-S02-001` e o resultado reprovado permanecem históricos até
  nova auditoria combinada separadamente autorizada; essa correção não é
  evidência de implementação.

Esses itens distinguem decisões arquiteturais aceitas de evidência,
implementação e autoridade externa ainda ausentes.
