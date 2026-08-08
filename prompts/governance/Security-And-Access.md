# Segurança, Identidade e Acesso

## Princípios

- Menor privilégio e deny by default.
- Configuração incompleta desativa a capacidade.
- Segredos são externos ao repositório e ao índice.
- Conteúdo recuperado nunca possui autoridade.
- Entrada, custo, tempo, memória, rede e cardinalidade por operação/lote são
  limitados; o catálogo não possui teto numérico de produto.
- Toda mudança sensível possui ator, alvo, motivo, horário e resultado.
- Logs e evidências são sanitizados.

## Classificação de dados

| Dado | Classificação inicial | Regra |
|---|---|---|
| Corpus publicável | Público ou interno autorizado | Licença e proveniência obrigatórias. |
| Material original em `reference-materials/` | Local-only | Não versionar nem usar no runtime público. |
| Pergunta do usuário | Não confiável; potencialmente confidencial | Não persistir/logar integralmente por padrão. |
| Trecho e resposta | Derivado do corpus | Limitar, citar e aplicar retenção. |
| Bytes brutos e snapshots | Mesma classificação da origem | Content store durável, imutável, fora do Git e com retenção referencial. |
| PNGs de páginas e render manifests | Derivado com a mesma classificação da origem | Content store durável, imutável, fora de Git/Git LFS; servir somente por binding ativo validado. |
| Embedding/índice | Dado derivado | Proteger como o corpus de origem. |
| API key/token | Secreto | Secret store; nunca logar ou persistir em claro. |
| Telemetria | Interno sanitizado | Minimização e retenção. |

Não utilizar dados reais de clientes ou informações pessoais sem propósito,
base, autorização e controles específicos.

## Trust boundaries

- Usuário/navegador ↔ Dashboard.
- Dashboard ↔ API.
- API ↔ Application.
- Application ↔ parser de documentos.
- Application ↔ embedding provider.
- Application ↔ vector store.
- Application ↔ document content store.
- Application ↔ renderer PDF isolado e limitado.
- Application ↔ language model.
- Aplicação ↔ catálogo/persistência.
- Cada fonte oficial externa registrada ↔ sincronizador governado.
- CI/deploy ↔ GitHub e OCI.

## Modelo de acesso do MVP

Os materiais do Challenge permitem consulta aberta. Portanto:

- a rota de pergunta pode ser anônima, com limites e proteção contra abuso;
- administração de bancos/documentos/fontes, ingestão, ativação, rollback e
  configuração não são operações públicas anônimas;
- o ADR-0006 aceito escolheu uma superfície administrativa local não pública
  no modo one-shot do host principal; ela usa identidade do sistema
  operacional, permissões mínimas, motivo obrigatório, idempotência e
  auditoria sanitizada, ainda sem implementação;
- o startup apenas verifica e carrega a geração ativa; mutação exige modo
  administrativo one-shot explicitamente configurado e invocado;
- sincronização oficial usa a mesma superfície administrativa; consulta
  pública resolve o manifesto ativo, mas nunca inicia download;
- secrets não são enviados ao navegador;
- autorização continua server-side mesmo que a UI esconda uma função;
- o MVP não implementa gestão de usuários ou multi-tenancy.

## Modelo de permissões futuro

| Papel | Permissões |
|---|---|
| `Reader` | Consultar acervos autorizados e ver citações. |
| `Curator` | Gerir documentos e versões de acervos atribuídos. |
| `Operator` | Executar sincronização, ativação e rollback. |
| `SecurityAdministrator` | Gerir políticas, fontes externas e auditoria. |
| `PlatformAdministrator` | Gerir providers, configuração e ambientes. |

Escopos futuros: ambiente e corpus. Filtros de acesso são aplicados antes da
recuperação, nunca apenas depois da geração.

## Segredos e configuração

- Desenvolvimento: user-secrets, cofre local aprovado ou variáveis protegidas.
- CI: secrets do ambiente, com menor privilégio e sem exposição em forks.
- OCI: secret manager ou mecanismo aprovado do ambiente.
- Persistir apenas referência opaca, nome de variável, provider e metadados de
  rotação.
- Fornecer exemplos sem valores reais.
- Não imprimir configuração completa nem ambiente inteiro.
- Rotacionar imediatamente material exposto e preservar evidência sanitizada.

## Ameaças RAG

### Prompt injection em documentos

- Delimitar evidência como dados não confiáveis.
- Instruir o modelo a ignorar comandos presentes no conteúdo.
- Não disponibilizar ferramentas ao modelo no MVP.
- Validar que citações pertencem ao conjunto recuperado.
- Incluir casos maliciosos na avaliação.

### Alucinação e citação falsa

- Restringir a resposta às evidências.
- Exigir citação por afirmação factual relevante.
- Verificar IDs e localização antes de retornar.
- Usar resultado explícito de evidência insuficiente.

### Exfiltração e vazamento

- Não misturar corpus, geração ou item fora do manifesto ativo.
- Não enviar mais contexto do que o necessário.
- Não logar prompt, trecho ou resposta integral por padrão.
- Tratar embeddings como derivados potencialmente sensíveis.

### Poisoning e supply chain documental

- Identificar origem, hash, versão, licença e ator da ingestão.
- Construir geração inativa e validar antes de ativar.
- Preservar rollback.
- Não ingerir automaticamente material não aprovado.
- Tratar o registro administrativo de bancos, categorias, documentos e fontes
  como plano de controle confiável; toda alteração exige ator, motivo,
  validação, candidata e ativação.

## Arquivos e parsing

- Allowlist de formatos e media types.
- Validar assinatura/estrutura, não apenas extensão.
- Limites por operação de tamanho, páginas, linhas, colunas, células, texto,
  chunks e tempo, sem teto de produto para a cardinalidade do catálogo.
- Canonicalizar caminho e impedir traversal/symlink escape.
- Não executar macros, scripts, anexos, links ou fórmulas de CSV.
- Isolar parser quando o risco ou biblioteca exigir.
- Atualizar dependências e responder a vulnerabilidades.

Upload público permanece fora do MVP.

## Renderização e evidência visual

- PDF é entrada não confiável também para o renderer; limitar bytes, páginas,
  tempo, memória, dimensões, concorrência e quantidade total por operação.
- O perfil `pdf-page-png-v1` remove metadados capazes de revelar path, host ou
  comando e produz somente PNG RGB opaco dentro dos limites aceitos.
- Recalcular hash, validar assinatura PNG, dimensões, page count, numeração
  consecutiva e manifesto canônico; reabrir fonte e todos os objetos antes de
  finalizar a candidata.
- `IDocumentContentStore` é a única autoridade binária de produto para fontes e
  PNGs persistentes. Git, Git LFS, quarentena, catálogo e vector store não
  provam durabilidade ou readback.
- Cada nova revisão de ativação persiste o vínculo exato entre documento,
  objeto fonte, snapshot completo de direitos, geração e render manifest PDF.
  Revisões históricas sem esse conjunto permanecem sem backfill e falham
  fechadas para prontidão/consulta visual.
- Uma imagem só pode ser servida quando uma citação validada referencia a mesma
  versão documental, página, geração ativa e render manifest finalizado.
  Documento `Deactivated` ou `Removed` nunca serve imagem.
- O contrato planejado não embute bytes nem expõe path. Um futuro endpoint
  same-origin, read-only, revalida o binding, limita o corpo, usa ETag imutável,
  `X-Content-Type-Options: nosniff`, política de cache adequada e autorização
  equivalente à evidência textual.
- Evidência textual adjacente permanece acessível; PNG nunca é o único portador
  de uma afirmação ou significado de navegação.
- O LLM recebe somente texto. Enviar imagem ou derivado a provider exige
  autoridade própria de egress, classificação, retenção, residência e custo.
- Direito de ler, indexar ou citar não implica renderizar, criar/reter derivado,
  exibir ou distribuir. Ambiguidade em qualquer direito aplicável bloqueia a
  ativação visual.

## Evidência persistente de resposta

- O `AnswerEvidenceRecordV1` implementado é um contrato interno de persistência,
  não histórico de conversa, analytics, endpoint ou campo público v1.
- Somente `Answered`, após validação integral e antes da resposta, cria o
  registro; falha de commit/readback impede sucesso público.
- Persistir apenas identidades/digests, hash/comprimento da resposta,
  descritores não secretos e vínculos exatos de citação, fonte, manifest e
  página. Não persistir pergunta nem seu hash, resposta, excerto/URL, prompt,
  payload de provider, score/vetor, identidade/IP do usuário, secret, path ou
  bytes.
- Aplicar `answer-evidence-p30d-v1`: `expiresAt = createdAt + P30D`, sem refresh
  por leitura, replay ou inspeção.
- Durante a retenção, fonte e PNGs vinculados permanecem alcançáveis. Expiração
  não exclui nada; `cleanup-plan-v1` reserva e revalida todas as raízes antes de
  qualquer remoção física, inclusive sob concorrência.
- Header, citações, páginas e auditoria sanitizada são atômicos. Mesmo ID/digest
  é replay; mesmo ID/conteúdo divergente é conflito sem mutação.
- A autoridade arquitetural de ADR-0010 não implementou o incremento por si; a
  autoridade posterior de `S04-CORR-04-E` implementou o contrato e a migration
  localmente sem alterar OpenAPI v1, v2 ou serving e sem executar gate.

## Políticas de egress

Egress é dividido em quatro políticas independentes:

### `AI_PROVIDER_EGRESS`

Permanece deny by default. O ADR-0005 aceito selecionou os providers externos
condicionais e o limite de divulgação, mas não autorizou egress, uso de conta
ou execução. Um provider local futuro pode manter essa política sem destinos
externos. Para o provider externo aceito:

- somente endpoints e portas explicitamente allowlisted;
- revisão documentada de retenção, uso para treino, residência, termos,
  classificação de dado permitida e mecanismo de exclusão;
- minimização de dados e aviso ao usuário apropriado ao ambiente;
- chunks do corpus e a pergunta minimizada/normalizada podem sair apenas para
  o embedding provider selecionado, respectivamente para indexação e query
  embedding;
- pergunta e trechos recuperados podem sair apenas para o language model
  selecionado;
- ao longo da indexação, o embedding provider pode receber cumulativamente
  todo o conteúdo autorizado, dividido em chunks limitados; isso conta como
  divulgação do corpus e exige classificação e autorização correspondentes;
- o language model recebe somente a pergunta e as evidências mínimas
  recuperadas, nunca o corpus completo;
- secrets, caminhos locais, metadados desnecessários e arquivo integral em uma
  única requisição não são enviados;
- timeout, cancelamento, limite de tokens/bytes, orçamento e auditoria
  sanitizada são obrigatórios.

Configurar credencial ou provider não concede sozinho autoridade de egress.

### `VECTOR_STORE_EGRESS`

Permanece sem destinos quando o vector store é local. Se o `STATE-02`
selecionar serviço gerenciado:

- endpoints, portas e TLS são allowlisted e validados;
- embeddings, chunks e metadados recebem a mesma classificação e proteção do
  corpus de origem;
- residência, retenção, backup, exclusão, uso secundário e isolamento de
  tenant são revisados;
- credenciais possuem menor privilégio e ficam fora do índice, logs e
  frontend;
- timeouts, cancelamento, limites, auditoria sanitizada e procedimento de
  indisponibilidade são obrigatórios.

Configurar um adapter gerenciado não concede autoridade de
`VECTOR_STORE_EGRESS` nem de `OCI_RUNTIME_EGRESS`.

### `OFFICIAL_SOURCE_EGRESS`

Integra o MVP, mas permanece deny by default. Cada fonte só pode ser habilitada
no perfil de sincronização depois que seu registro, URL canônica,
licença/termos, maxAge, allowlist e limites forem aprovados e a execução de
rede receber autoridade específica. Incluir um registro não habilita egress; a
pergunta pública também não.
Essa política não se confunde com chamadas ao provider de IA.

### `OCI_RUNTIME_EGRESS`

O runtime OCI usa uma allowlist separada que agrega somente os destinos
  individualmente autorizados para provider de IA, fontes oficiais, secret store,
vector store, telemetria ou serviços operacionais selecionados. A URL oficial
precisa ser permitida simultaneamente por `OFFICIAL_SOURCE_EGRESS` e
`OCI_RUNTIME_EGRESS`; vector store gerenciado exige
`VECTOR_STORE_EGRESS` e `OCI_RUNTIME_EGRESS`. Uma política não amplia a outra.
Acesso genérico à internet, metadata service e destinos privados não
autorizados permanece bloqueado. A política é validada no ambiente alvo.

## Rede e fonte oficial do MVP

- HTTPS obrigatório.
- URL pública canonicalizada sem userinfo, fragment, token, assinatura ou
  credencial na query; scheme, host IDN normalizado, porta, path e query exatos
  de cada PDF/CSV oficial em allowlist.
- O adapter não envia `Authorization`, API key, client certificate,
  pre-authentication ou credencial ambiente; fonte autenticada fica fora do
  MVP porque a URL canônica é metadado público de citação.
- A/AAAA resolvidos e autorizados a cada nova conexão física; o conjunto
  inteiro é rejeitado se qualquer resposta apontar para loopback, link-local,
  rede privada não autorizada, metadata service ou destino proibido.
- O socket conecta somente a um `IPEndPoint` aprovado, preservando o hostname
  original para Host, SNI e validação do certificado, sem nova resolução por
  hostname durante a conexão.
- Validação de certificado não pode criar egress AIA, CRL, OCSP ou outro
  destino fora de política. O ADR-0006 aceito escolheu trust local, downloads
  de cadeia e revogação online desativados e aceitou o risco residual de
  revogação; qualquer destino auxiliar continua exigindo allowlist, decisão e
  autoridade próprias. Configuração ausente falha fechada, e a política
  escolhida ainda deve ser implementada e provada em clone local limpo e OCI
  sem afrouxamento silencioso.
- Redirects desativados no MVP. Habilitação futura exige nova decisão,
  allowlist e validação/pinning completo a cada salto.
- Sem proxy, cookies ou credenciais ambientais por padrão.
- Timeout, bytes transferidos e descomprimidos, media type/estrutura PDF/CSV,
  páginas/linhas/colunas/células, taxa e concorrência limitados.
- ETag/Last-Modified não substituem hash e validação.
- Respeitar licença, termos, robots e frequência permitida.
- Sincronizar para snapshot governado; não navegar livremente durante a
  pergunta.
- Snapshot bruto e derivados ficam fora do Git quando a licença não autorizar
  redistribuição.
- Testes padrão usam servidor falso local. Teste contra a URL real é opt-in,
  sanitizado e exige autorização de rede própria.

## API e abuso

- Validar e limitar tamanho de pergunta e payload.
- Rejeitar URL, host, path, provider, adapter ou campo de autoridade sobre o
  catálogo enviados pelo cliente.
- Rate limit por origem/chave adequada ao ambiente.
- Timeout e cancelamento do fluxo.
- Limite de top-k, tokens e chamadas externas.
- CORS restrito ao frontend autorizado.
- TLS no ambiente público.
- Problem Details sem stack trace ou dado sensível.
- OpenAPI v1 e seus enums de idioma permanecem inalterados; nenhum valor BCP 47
  adicional ou referência de imagem é aceito/emitido por coerção.
- Liveness barato e independente de serviço externo.
- Readiness sanitizado distingue núcleo, dependências de consulta e cobertura
  por fonte/documento. Item stale/indisponível degrada cobertura e não falha
  global quando outro banco/documento ativo permanece atendível.

## Dashboard e saída não confiável

- Renderizar resposta e citação como texto puro por padrão.
- Se Markdown for necessário, aceitar somente um subconjunto sanitizado.
- Bloquear HTML cru, handlers, scripts, estilos ativos e URLs perigosas.
- Permitir apenas schemes de URL aprovados, sem links de citação executáveis
  derivados diretamente do modelo.
- Aplicar Content Security Policy e codificação contextual.
- Para a futura evidência visual, aceitar somente a referência same-origin
  criada pelo servidor, com tamanho/mime conhecidos; nunca construir `src` a
  partir de texto do modelo, tag de idioma, URL documental ou path.
- Preservar o texto original da citação e alternativa acessível junto da página
  exibida; não traduzir conteúdo derivado da fonte.
- Testar XSS armazenado/refletido em documento, pergunta, resposta, erro e
  metadados de citação.

## Logging e observabilidade

Pode registrar:

- correlation ID e operation ID;
- ator anônimo estável apenas quando necessário e legal;
- corpus/document/index IDs;
- provider/model version;
- duração, contagem, status e código de erro;
- hash ou tamanho, nunca secret ou conteúdo integral.
- tag BCP 47 canônica, render profile, contagem/dimensões e hashes de imagem,
  sem bytes ou texto integral.
- ID de answer-evidence, IDs/digests de corpus/ativação/geração, contagens,
  duração, expiração e resultado sanitizado de retenção/cleanup.

Não registrar:

- API keys, tokens ou headers de autorização;
- prompts e respostas integrais por padrão;
- pergunta, hash da pergunta ou hash da resposta por padrão;
- texto completo do documento;
- bytes de fonte ou imagem e metadados brutos do renderer;
- caminhos absolutos com nome de usuário/host;
- stack trace ou payload em resposta pública.

## Auditoria

Eventos mínimos:

- mudança de configuração sanitizada;
- início, conclusão e falha de ingestão;
- criação e ativação de geração;
- import/reopen de conteúdo, finalização/rejeição de render manifest e serving
  visual recusado por binding ou lifecycle;
- troca atômica do `CorpusActivationRecord`, incluindo o conjunto de bindings;
- criação/replay/conflito de answer-evidence e cleanup após expiração, apenas
  com IDs, contagens e resultados sanitizados;
- rollback;
- mudança de banco, categoria, documento, versão ou fonte;
- mudança de provider/model;
- sincronização, revalidação, stale, retirada e falha da fonte oficial;
- acesso administrativo do MVP e, futuramente, RBAC/gestão de usuários.

O registro deve ser pesquisável, protegido contra alteração indevida e retido
por política. No MVP anônimo, consultas comuns geram métricas agregadas, não
trilha nominal.

## Tratamento de erros

- Falhas esperadas retornam estados tipados.
- Exceptions de provider são sanitizadas na fronteira.
- Retry usa backoff/jitter apenas para falhas transitórias e idempotentes.
- Rate limit e orçamento não são contornados.
- Falha de nova indexação não desativa a geração anterior.
- Estado ambíguo retorna `Unavailable` ou `Failed`, nunca sucesso.
- Incidentes P0/P1 bloqueiam progressão até tratamento ou decisão formal.

## Segurança de CI/CD

- Permissões mínimas.
- Actions e toolchains fixadas.
- Checkout sem credenciais persistentes.
- Lockfiles obrigatórios.
- Dependency e secret scanning.
- Artefatos identificáveis e sem secrets.
- Deploy somente por ambiente/gate autorizado.
- Secrets de pull requests externos indisponíveis.
- Testes padrão não acessam a fonte oficial real; jobs opt-in exigem ambiente,
  egress e autoridade próprios.
- Logs e evidências sanitizados antes de retenção.

## Checklist

- Threat model e trust boundaries atualizados.
- Licença/proveniência do corpus verificadas.
- Direitos de retenção da fonte e, para PDF visual, rendering, criação/retenção
  de derivados, display e distribuição pretendida verificados por documento.
- Secret e dependency scans aprovados.
- Arquivos e payloads limitados.
- Prompt injection e citação falsa testadas.
- Rate limit, timeout e cancelamento exercitados.
- Logs sem dados sensíveis.
- Rollback de geração verificado.
- Conteúdo fonte/PNG, render manifest, reachability, backup/restore e serving
  visual fail-closed verificados quando a capacidade for implementada.
- `AnswerEvidenceRecordV1` permanece `Answered`-only, atômico, minimizado,
  expira em `P30D` sem refresh e protege reachability contra races de cleanup;
  a evidência local não substitui gate nem validação operacional.
- `AI_PROVIDER_EGRESS` é local ou possui provider, classificação e endpoints
  explicitamente autorizados e testados.
- `VECTOR_STORE_EGRESS` permanece vazio para adapter local ou possui endpoint,
  classificação, residência, retenção e credencial explicitamente aprovados.
- `OFFICIAL_SOURCE_EGRESS` permanece deny by default e, quando autorizado,
  restringe-se às URLs exatas dos registros ativos aprovados.
- Integridade do manifesto, proveniência, freshness, falha parcial de sync e
  ausência de fallback silencioso são testados.
- `OCI_RUNTIME_EGRESS` possui allowlist mínima validada no alvo.
- DNS rebinding, resposta DNS mista, pinning de IP, Host/SNI, redirects
  bloqueados, ausência de autenticação e egress de AIA/CRL/OCSP são testados
  com servidor controlado.
- Saída do Dashboard é codificada/sanitizada e testada contra XSS.
- Tags de idioma são BCP 47 limitadas, não selecionam recurso/provider e
  preservam a declaração exata; `en` não é inferido como `en-GB`.
- Permissões negativas testadas quando introduzidas.
