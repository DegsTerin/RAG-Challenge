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
- Permitir substituir documentos e reconstruir o índice com segurança.
- Entregar consulta a uma fonte oficial online controlada, sem navegação livre.
- Preparar, sem implementar agora, múltiplos acervos, múltiplas fontes oficiais
  e atualização incremental agendada.
- Preservar um caminho de integração futura ao DB-Notifier sem dependência
  direta.

## Escopo do MVP

- Um corpus configurado.
- Um documento PDF autoral ou com direitos de uso e redistribuição
  confirmados.
- Uma fonte oficial online selecionada no `STATE-02`, limitada a uma URL
  canônica HTTPS de PDF em domínio aprovado.
- Um parser de PDF compartilhado pelo documento local e pelo snapshot oficial.
- Sincronização manual server-side da fonte oficial para snapshot imutável,
  com proveniência, frescor e rollback.
- Seleção explícita, por pergunta, entre evidência `Local` e
  `OfficialOnline`; o MVP não mistura as duas silenciosamente.
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
- Substituição manual do acervo e reconstrução segura.
- Execução local reproduzível.
- Deploy autorizado em OCI e evidência da execução.
- Testes, observabilidade mínima e documentação pública.

## Fora do escopo do MVP

- Prometer todos os bancos de dados existentes.
- Ingerir simultaneamente PDF, Word, Excel, PowerPoint, Markdown, CSV, JSON e
  HTML.
- Upload público e administração remota de acervos.
- Mais de um acervo ativo.
- Sincronização incremental agendada e distribuída.
- Mais de uma fonte ou URL oficial online.
- URL arbitrária, crawling genérico ou consulta direta à internet durante cada
  pergunta.
- Autenticação corporativa, RBAC completo ou multi-tenancy.
- Microserviços, filas distribuídas ou loader dinâmico de plug-ins.
- Integração executável com o DB-Notifier.
- Start, Stop, administração ou conexão com bancos de dados reais.

## Acervo inicial

O acervo conceitual candidato é `Catálogo de Bancos de Dados — MVP`. Ele deve
ser finito em cada versão, versionado, publicável e organizado por categorias
como:

- relacionais;
- documentos;
- chave-valor;
- wide-column;
- grafos;
- séries temporais;
- busca;
- data warehouse e serviços gerenciados.

A lista exata de produtos, a licença do documento local e a fonte oficial
online permanecem decisões do `STATE-02 ARCHITECTURE`. Não existe teto de
escopo do produto para a quantidade de sistemas nem para a quantidade de
páginas do corpus; cada versão registra suas contagens observadas. A fonte
online do MVP deve ser um único PDF oficial, ter scheme/host/porta/path
allowlisted, termos/licença revisados e snapshot sincronizado antes da
recuperação.

### Catálogo candidato em ondas

Esta lista não exaustiva preserva exemplos já discutidos sem declarar
cobertura, suporte, prioridade ou limite. Ela não substitui a lista integral
aprovada para cada versão do catálogo.

| Referência | Categorias e exemplos candidatos |
|---|---|
| Exemplos relacionais e gerais | PostgreSQL, MySQL, MariaDB, Microsoft SQL Server, Oracle Database, SQLite, IBM Db2, SAP HANA, Firebird e CockroachDB. |
| Exemplos NoSQL e distribuídos | MongoDB, Redis/Valkey, Apache Cassandra, Couchbase, CouchDB e ScyllaDB. |
| Exemplos especializados e cloud | Elasticsearch/OpenSearch, Neo4j, InfluxDB, TimescaleDB, data warehouses e serviços gerenciados de AWS, Azure, Google Cloud e Oracle Cloud. |

O catálogo arquitetural é aberto; inclusão no documento não significa
integração, homologação, recomendação ou suporte operacional.

## Requisitos funcionais

| ID | Requisito | MVP |
|---|---|---|
| `RF-001` | Carregar o acervo local configurado sem depender de `reference-materials/`. | Sim |
| `RF-002` | Validar tipo, tamanho, identidade e integridade do documento antes do processamento. | Sim |
| `RF-003` | Extrair texto de um PDF e produzir chunks com metadados de origem. | Sim |
| `RF-004` | Gerar embeddings e construir uma geração de índice identificável. | Sim |
| `RF-005` | Consultar o índice com uma pergunta em linguagem natural. | Sim |
| `RF-006` | Gerar resposta somente a partir dos trechos recuperados. | Sim |
| `RF-007` | Retornar citações com documento, versão e localização disponível. | Sim |
| `RF-008` | Retornar `INSUFFICIENT_EVIDENCE` quando a recuperação não sustentar a resposta. | Sim |
| `RF-009` | Substituir manualmente o documento e construir nova geração sem destruir previamente a ativa. | Sim |
| `RF-010` | Expor liveness, readiness e diagnóstico sanitizado de dependências. | Sim |
| `RF-011` | Executar localmente por procedimento documentado. | Sim |
| `RF-012` | Executar em OCI e produzir evidência verificável. | Sim |
| `RF-013` | Adicionar, remover, versionar, ativar e desativar múltiplos acervos. | Futuro |
| `RF-014` | Sincronizar alterações por documento de forma incremental e agendada. | Futuro |
| `RF-015` | Trocar embeddings, armazenamento vetorial e LLM por configuração/composição. | Preparado; uma implementação no MVP |
| `RF-016` | Sincronizar manualmente uma fonte oficial online por adapter separado, allowlist e snapshot versionado, preservando URL e frescor. | Sim |
| `RF-017` | Publicar no MVP o contrato HTTP/OpenAPI versionado do RAG-Challenge; qualquer adapter consumidor, inclusive do DB-Notifier, pertence ao repositório consumidor e a gates próprios futuros. | Contrato no MVP; adapters consumidores no futuro |
| `RF-018` | Adicionar formatos documentais por adapters próprios sem alterar os casos de uso do núcleo. | Futuro |
| `RF-019` | Aplicar RBAC e escopo por corpus antes da recuperação. | Futuro |
| `RF-020` | Exigir que cada pergunta selecione `Local` ou `OfficialOnline`, recuperar somente desse escopo e falhar sem fallback silencioso quando a fonte estiver indisponível ou stale. | Sim |
| `RF-021` | Aceitar perguntas com idioma declarado `pt-BR` ou `en-GB`, responder no mesmo idioma e preservar no idioma original todo conteúdo derivado da fonte exibido em citações. | Sim |
| `RF-022` | Permitir selecionar `pt-BR` ou `en-GB` para a interface e localizar todo texto visual pertencente ao produto sem alterar `questionLanguage`, `answerLanguage` ou conteúdo citado. | Sim |
| `RF-023` | Permitir selecionar o tema visual `Light` ou `Dark` sem alterar `interfaceLanguage`, `questionLanguage`, `answerLanguage`, evidência ou citações. | Sim |

## Requisitos não funcionais

| ID | Requisito |
|---|---|
| `RNF-001` | O núcleo não depende de SDK de IA, parser, vetor, UI, transporte ou DB-Notifier. |
| `RNF-002` | Configuração é tipada, validada no startup e falha fechada. |
| `RNF-003` | Segredos não entram no repositório, logs, respostas ou evidências. |
| `RNF-004` | Operações externas têm timeout, cancelamento e limites de tamanho/custo. |
| `RNF-005` | Documento, conteúdo bruto, snapshot, chunk, provider e índice têm proveniência, identidade e versão rastreáveis; bytes imutáveis permanecem reabríveis enquanto forem necessários para rebuild ou rollback. |
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

## Critérios de aceitação do MVP

| ID | Critério |
|---|---|
| `AC-MVP-001` | Um clone limpo pode ser configurado, compilado, testado e executado pelo procedimento publicado. |
| `AC-MVP-002` | O documento autorizado é persistido/reaberto por hash, processado e uma geração finalizada com integridade validada fica ativa; staging parcial permanece não consultável. |
| `AC-MVP-003` | Perguntas representativas aprovadas recuperam citações corretas. |
| `AC-MVP-004` | Perguntas fora do acervo não recebem resposta factual inventada. |
| `AC-MVP-005` | A troca manual do documento cria uma geração validada, ativa atomicamente o registro completo que vincula geração, snapshot e observação oficial, preserva a revisão completa anterior elegível e permite retorno testado dentro da janela de rollback. |
| `AC-MVP-006` | Nenhum secret ou material local ignorado integra o repositório. |
| `AC-MVP-007` | Os checks automáticos aplicáveis ao estado são aprovados. |
| `AC-MVP-008` | A aplicação é executada em OCI com link ou evidência visual sanitizada. |
| `AC-MVP-009` | O README contém arquitetura, tecnologias, execução e exemplos reais verificados após a implementação. |
| `AC-MVP-010` | A interface mínima permite perguntar, consultar citações e compreender loading, vazio, erro, indisponibilidade e evidência insuficiente, com teclado, foco e contraste adequados. |
| `AC-MVP-011` | A API expõe consulta, health e artefato OpenAPI v1 versionados, com configuração fail-closed, limites, cancelamento, metadados reproduzíveis, erros canônicos e diagnóstico sanitizado; compatibilidade do contrato é testada. |
| `AC-MVP-012` | Testes de arquitetura e contrato demonstram que Domain/Application não dependem de SDKs ou adapters concretos e que providers são compostos pelas bordas. |
| `AC-MVP-013` | O repositório público possui estrutura compreensível e histórico incremental de commits, sem secrets ou materiais locais ignorados. |
| `AC-MVP-014` | Uma sincronização autorizada busca o PDF oficial allowlisted; conteúdo alterado produz snapshot versionado e ativa atomicamente geração, snapshot e observação validados. `304` ou hash idêntico atualiza somente a observação quando a geração ativa já referencia o snapshot compatível; retirada autoritativa ou desativação administrativa também muda somente a observação do mesmo registro. Os casos incompatíveis exigem reconstrução controlada. As citações expõem URL canônica pública, snapshot e frescor. |
| `AC-MVP-015` | A sincronização usa fonte pública sem autenticação ou segredo na URL, rejeita domínio, IP, porta, path, query, resposta DNS mista, redirect ou destino lateral de validação TLS fora da política com `SourcePolicyViolation`, e conecta somente ao IP previamente autorizado preservando Host/SNI. O usuário escolhe `Local` ou `OfficialOnline`, cada modo recupera somente o escopo selecionado, e falha, retirada ou expiração retornam estado tipado sem fallback silencioso. |
| `AC-MVP-016` | Perguntas declaradas como `pt-BR` recebem respostas em `pt-BR`, perguntas declaradas como `en-GB` recebem respostas em `en-GB`, e citações não traduzem título, seção, trecho ou outro conteúdo proveniente da fonte. Testes determinísticos cobrem `pt-BR→pt-BR`, `en-GB→en-GB`, `pt-BR→en-GB` e `en-GB→pt-BR` entre idioma da pergunta e idioma da evidência. |
| `AC-MVP-017` | A pessoa consegue alternar explicitamente a interface entre `pt-BR` e `en-GB`; labels, instruções, validações e estados pertencentes ao produto usam integralmente o idioma visual selecionado. Testes de componente e fluxo cobrem cada idioma da interface combinado com cada `questionLanguage`, sem traduzir citações. |
| `AC-MVP-018` | A pessoa consegue alternar explicitamente a interface entre `Light` e `Dark`; conteúdo, idioma e contexto da consulta permanecem inalterados. Testes de componente, acessibilidade e fluxo executam nos dois temas as quatro combinações entre `interfaceLanguage` e `questionLanguage`, totalizando oito combinações, e validam contraste, foco, estados e ausência de informação dependente apenas de cor. |

## Premissas

- A primeira interface será simples; o valor principal é o fluxo RAG.
- O acesso a perguntas pode ser anônimo no MVP, conforme os materiais do
  Challenge.
- Operações administrativas de ingestão não serão expostas anonimamente.
- A sincronização oficial é manual e administrativa; uma pergunta pública não
  inicia crawling nem escolhe URL.
- A fonte oficial do MVP é publicamente acessível sem autenticação; URL,
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

## Limitações e decisões pendentes

- Licença do repositório e do acervo.
- Conteúdo exato do PDF inicial.
- Fonte oficial, URL canônica, termos/licença, maxAge e limites.
- Provider de embeddings, armazenamento vetorial e LLM.
- Persistência durável e retenção dos bytes documentais, catálogo e índice
  entre reinicializações.
- Política de egress e tratamento de dados caso o vector store selecionado
  seja externo.
- Serviço OCI e região alvo.
- Orçamento e limites dos providers externos.
- Dataset de avaliação e thresholds de recuperação/groundedness.

Essas decisões devem ser resolvidas no estado proprietário; não são
implementação implícita.
