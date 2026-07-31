# Segurança, Identidade e Acesso

## Princípios

- Menor privilégio e deny by default.
- Configuração incompleta desativa a capacidade.
- Segredos são externos ao repositório e ao índice.
- Conteúdo recuperado nunca possui autoridade.
- Entrada, custo, tempo, memória, rede e cardinalidade são limitados.
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
- Application ↔ language model.
- Aplicação ↔ catálogo/persistência.
- Fonte oficial externa do MVP ↔ sincronizador governado.
- CI/deploy ↔ GitHub e OCI.

## Modelo de acesso do MVP

Os materiais do Challenge permitem consulta aberta. Portanto:

- a rota de pergunta pode ser anônima, com limites e proteção contra abuso;
- ingestão, ativação, rollback e configuração não são operações públicas
  anônimas;
- no `STATE-02`, deve ser escolhida uma superfície administrativa local não
  pública; ela usa identidade do sistema operacional, permissões mínimas,
  motivo obrigatório, idempotência e auditoria sanitizada;
- o startup apenas verifica e carrega a geração ativa; mutação exige modo
  administrativo one-shot explicitamente configurado e invocado;
- sincronização oficial usa a mesma superfície administrativa; consulta
  pública seleciona o snapshot, mas nunca inicia download;
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

- Não misturar corpus ou escopo.
- Não enviar mais contexto do que o necessário.
- Não logar prompt, trecho ou resposta integral por padrão.
- Tratar embeddings como derivados potencialmente sensíveis.

### Poisoning e supply chain documental

- Identificar origem, hash, versão, licença e ator da ingestão.
- Construir geração inativa e validar antes de ativar.
- Preservar rollback.
- Não ingerir automaticamente material não aprovado.

## Arquivos e parsing

- Allowlist de formatos e media types.
- Validar assinatura/estrutura, não apenas extensão.
- Limites de tamanho, páginas, texto, chunks e tempo.
- Canonicalizar caminho e impedir traversal/symlink escape.
- Não executar macros, scripts, anexos ou links do documento.
- Isolar parser quando o risco ou biblioteca exigir.
- Atualizar dependências e responder a vulnerabilidades.

Upload público permanece fora do MVP.

## Políticas de egress

Egress é dividido em quatro políticas independentes:

### `AI_PROVIDER_EGRESS`

Fica deny by default até que o `STATE-02` selecione o provider e o owner
autorize seu uso. Um provider local pode manter essa política sem destinos
externos. Para provider externo:

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

Integra o MVP, mas permanece deny by default. Só pode ser habilitada no perfil
de sincronização depois que ADR-0002, URL canônica, licença/termos, maxAge,
allowlist e limites forem aprovados no estado proprietário e a execução de
rede receber autoridade específica. A pergunta pública não habilita egress.
Essa política não se confunde com chamadas ao provider de IA.

### `OCI_RUNTIME_EGRESS`

O runtime OCI usa uma allowlist separada que agrega somente os destinos
individualmente autorizados para provider de IA, fonte oficial, secret store,
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
  do único PDF oficial em allowlist.
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
  destino fora de política. O `STATE-02` deve decidir trust, revogação,
  downloads de cadeia e eventual provisão/atualização de material; qualquer
  destino auxiliar exige allowlist e autoridade próprias. Configuração ausente
  falha fechada, e a política escolhida deve ser provada em clone local limpo e
  OCI sem afrouxamento silencioso.
- Redirects desativados no MVP. Habilitação futura exige nova decisão,
  allowlist e validação/pinning completo a cada salto.
- Sem proxy, cookies ou credenciais ambientais por padrão.
- Timeout, bytes transferidos e descomprimidos, media type/assinatura PDF,
  páginas, taxa e concorrência limitados.
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
- Aceitar apenas `sourceScope=Local|OfficialOnline`; rejeitar URL, host, path,
  provider ou adapter enviados pelo cliente.
- Rate limit por origem/chave adequada ao ambiente.
- Timeout e cancelamento do fluxo.
- Limite de top-k, tokens e chamadas externas.
- CORS restrito ao frontend autorizado.
- TLS no ambiente público.
- Problem Details sem stack trace ou dado sensível.
- Liveness barato e independente de serviço externo.
- Readiness sanitizado distingue núcleo, dependências de consulta e estado por
  scope. `OfficialOnline` stale/indisponível é degradação e não falha global
  quando `Local` permanece atendível.

## Dashboard e saída não confiável

- Renderizar resposta e citação como texto puro por padrão.
- Se Markdown for necessário, aceitar somente um subconjunto sanitizado.
- Bloquear HTML cru, handlers, scripts, estilos ativos e URLs perigosas.
- Permitir apenas schemes de URL aprovados, sem links de citação executáveis
  derivados diretamente do modelo.
- Aplicar Content Security Policy e codificação contextual.
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

Não registrar:

- API keys, tokens ou headers de autorização;
- prompts e respostas integrais por padrão;
- texto completo do documento;
- caminhos absolutos com nome de usuário/host;
- stack trace ou payload em resposta pública.

## Auditoria

Eventos mínimos:

- mudança de configuração sanitizada;
- início, conclusão e falha de ingestão;
- criação e ativação de geração;
- troca atômica do `CorpusActivationRecord`, incluindo snapshot e observação;
- rollback;
- mudança de corpus/documento;
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
- Secret e dependency scans aprovados.
- Arquivos e payloads limitados.
- Prompt injection e citação falsa testadas.
- Rate limit, timeout e cancelamento exercitados.
- Logs sem dados sensíveis.
- Rollback de geração verificado.
- `AI_PROVIDER_EGRESS` é local ou possui provider, classificação e endpoints
  explicitamente autorizados e testados.
- `VECTOR_STORE_EGRESS` permanece vazio para adapter local ou possui endpoint,
  classificação, residência, retenção e credencial explicitamente aprovados.
- `OFFICIAL_SOURCE_EGRESS` permanece deny by default e, quando autorizado,
  restringe-se à única URL oficial aprovada.
- Isolamento `Local`/`OfficialOnline`, freshness, falha de sync e ausência de
  fallback silencioso são testados.
- `OCI_RUNTIME_EGRESS` possui allowlist mínima validada no alvo.
- DNS rebinding, resposta DNS mista, pinning de IP, Host/SNI, redirects
  bloqueados, ausência de autenticação e egress de AIA/CRL/OCSP são testados
  com servidor controlado.
- Saída do Dashboard é codificada/sanitizada e testada contra XSS.
- Permissões negativas testadas quando introduzidas.
