# Qualidade, Evidências e Gates

## Padrão de evidências

Toda alegação técnica registra, conforme aplicável:

- comando e diretório;
- versão das ferramentas;
- data e ambiente;
- exit code;
- escopo;
- resultado resumido;
- artefato sanitizado;
- distinção entre observado, inferido, não testado e bloqueado.

Ausência aparente de erro, compilação isolada ou resposta de um modelo não
prova funcionamento completo.

## Definition of Ready

Um lote está pronto quando:

- requisito e critério de aceite possuem IDs;
- estado e autoridade são claros;
- escopo positivo e negativo estão definidos;
- dependências e decisões bloqueadoras estão resolvidas;
- dados, secrets e ações externas estão autorizados;
- plano de teste, evidência e rollback é proporcional ao risco.
- se houver paralelismo, coordenadora, lanes independentes, ownership,
  isolamento, mensagens e ordem de integração estão definidos.
- a próxima conversa e cada lane possuem raciocínio do Codex recomendado,
  justificativa e alternativa caso o nível esteja indisponível.
- a [`política de idioma`](Language-Policy.md) é aplicável ao lote, sem
  tradução histórica ou idioma de interface inferidos.

## Definition of Done

- Requisitos e critérios atendidos.
- Build, formatação, lint, tipos e testes aplicáveis aprovados.
- Direção de dependências preservada.
- Segurança, falhas, limites e compatibilidade avaliados.
- Logs, health e erros adequados ao lote.
- Documentação e rollback atualizados.
- Nenhum secret, conteúdo sem licença ou evidência falsa.
- Itens não testados e riscos residuais explícitos.
- Mudanças preexistentes não relacionadas preservadas.
- Diff revisado e entrega focal.
- O handoff cumpre integralmente a semântica de
  [Governance](Governance.md) e o formato de
  [Templates](../templates/Templates.md): ocorre uma vez na resposta final,
  usa somente campos aplicáveis e mantém resultado, entrega futura,
  lifecycle, ação humana e roteamento distintos.
- Atualizações intermediárias acrescentam informação materialmente nova e não
  repetem nem antecipam o handoff.
- Rota, target, ação e payload são coerentes; texto obrigatório fica completo
  e sem placeholders imediatamente após a conversa, com rótulo/cercas fora do
  conteúdo, inclusive para Human Gate de uma linha ou payload com cerca
  interna. Ausência só é aceita quando nenhuma ação depende de mensagem.
- Runtime preflight foi classificado antes de qualquer inspeção e é aplicado
  somente a mudança ou validação executável; em documentação/read-only não há
  anúncio, enumeração ou encerramento de processo.
- A classificação paralela, quando aplicável, satisfaz o gate específico
  abaixo; raciocínio usa um valor canônico, justificativa e fallback sem
  ampliar autoridade ou alegar configuração automática.
- Comunicação e artefatos cumprem a
  [`política de idioma`](Language-Policy.md).

## Política de cobertura

- Piso inicial futuro: 70% de linhas e 45% de branches na suíte .NET
  abrangida.
- 80% de linhas é meta direcional baseada em risco.
- Cobertura não substitui testes funcionais, negativos, integração, contrato,
  RAG, segurança, acessibilidade, recuperação ou desempenho.
- Código crítico pode exigir cobertura superior.
- Exclusões devem ser estreitas, justificadas e verificáveis.
- Em `STATE-00`, cobertura de código é `NÃO APLICÁVEL`.

## Estratégia de testes

| Tipo | Finalidade |
|---|---|
| Unitário | Invariantes, versionamento, hashing, políticas, limites e falhas. |
| Arquitetura | Dependências para dentro e superfícies proibidas. |
| Contrato | Adapters PDF/CSV, catálogo, conteúdo fonte/PNG, render manifest, idiomas separados, answer-evidence interno, embeddings, vetor, LLM, OpenAPI v1 preservado e v2 planejado. |
| Integração | Persistência, digests separados de geração/ativação, rebinding de observação, render/lifecycle, ativação atômica, answer-evidence/retenção/reachability, rollback por novo registro, restart e HTTP. |
| RAG evaluation | Recuperação, groundedness, citações, recusa, matriz `pt-BR`/`en-GB` e estratos adicionais por idioma documental exato. |
| Segurança | PDF/CSV/renderer malicioso, binding de imagem, language coercion, poisoning de registro, prompt injection, SSRF, source leakage, secrets e abuso. |
| Acessibilidade | Teclado, foco, semântica, contraste, reflow, localização `pt-BR`/`en-GB`, temas `Light`/`Dark` e equivalente textual da evidência visual. |
| E2E | Documento até resposta e deploy até smoke. |
| Performance | Latência, limites, memória, custo e carga definidos. |
| Recuperação | Falha de indexação, geração incompatível, mismatch de observação e rollback sem replay de freshness. |

Testes padrão usam fixtures sintéticas ou corpus pequeno autorizado e não
dependem de rede ou cobrança. Testes externos são opt-in, isolados e exigem
autoridade/configuração próprias.

## Auditoria automática comum

1. Confirmar estado, escopo e autoridade.
2. Conferir arquivos esperados e mudanças não relacionadas.
3. Descobrir e executar os comandos reais.
4. Validar formatação, build, testes, cobertura e arquitetura aplicáveis.
5. Validar dependências, lockfiles, secrets e licenças aplicáveis.
6. Verificar links, UTF-8/LF e trailing whitespace.
7. Verificar configuração fail-closed e ausência de material local privado.
8. Classificar cada gate como `APROVADO`, `REPROVADO`, `BLOQUEADO` ou
   `NÃO APLICÁVEL`.
9. Auditar os resultados de handoff definidos na Definition of Done contra
   Governance e Templates, incluindo unicidade, campos condicionais,
   comentário intermediário, vocabulário, rota/target/ação, payload copiável,
   cerca interna, Human Gate, raciocínio e fallback.
10. Confirmar que runtime preflight foi classificado antes de qualquer
    inspeção e que a decisão observada corresponde ao tipo de trabalho.
11. Registrar achados com severidade, impacto, reprodução e recomendação.

Auditoria não corrige silenciosamente, não inventa evidência e não promove
estado.

## Gate para trabalho paralelo

Uma recomendação `PARALLEL_OPTIONAL` ou `PARALLEL_RECOMMENDED` somente passa
quando:

- a conversa coordenadora possui título ou label confirmado;
- baseline e envelope de autoridade são comuns e explícitos; cada lane recebe
  somente seu subconjunto autorizado, o escopo negativo global e restrições
  adicionais próprias;
- dependências formam frentes independentes, sem consumir output ainda não
  integrado de outra lane;
- cada path, artefato e recurso mutável possui um único owner;
- antes de Git rastreado, todas as workers simultâneas são `read-only`;
- depois da autorização de Git, escritas usam worktrees/branches separados e
  isolamento de runtime, dados, temporários e outputs aplicáveis;
- cada lane possui mensagem exata, checks, entrega, stop conditions e retorno
  para a coordenadora;
- coordenadora e lanes possuem níveis de raciocínio próprios, com
  justificativa e alternativa se indisponíveis;
- workers não atualizam estado/histórico, integram outras lanes, tomam decisão
  humana ou ampliam autoridade;
- a coordenadora integra uma entrega por vez e executa os checks transversais
  sobre o resultado combinado;
- conflito, baseline stale ou isolamento insuficiente reclassifica o restante
  como `SEQUENTIAL_ONLY`.

Human Gate, transição de lifecycle e decisão de ADR nunca são decididos em
paralelo. A evidência pode ser produzida por lanes independentes, mas a
decisão e seu registro pertencem à coordenadora após integração.
`Ultra` somente pode ser recomendado quando este gate permite trabalho
paralelo; sua indisponibilidade não remove nenhum requisito de isolamento,
coordenação ou validação.

## Gate documental do STATE-00

- Estrutura corresponde à lista aprovada.
- Links locais resolvem.
- Nomes canônicos, IDs e headings são consistentes.
- Arquivos usam UTF-8, LF, newline final e não têm trailing whitespace.
- Nenhum secret, host real ou dado pessoal desnecessário.
- Requisitos oficiais, interpretação do MVP e evolução são rastreáveis.
- Riscos, premissas, critérios, backlog e roadmap existem.
- Current State contém presente; log contém história.
- ADRs permanecem `proposed`.
- Nenhuma capacidade é apresentada como implementada.
- Durante a execução do gate, o Human Gate permanece `PENDENTE` até revisão
  do relatório automático e decisão humana; depois disso, Current State e o
  histórico preservam a decisão realmente registrada.
- A ordem `Human Gate STATE-00` → `GATE-B01` → autorização de entrada em
  `STATE-01` está explícita, sem aceitação implícita de ADR.
- A fonte oficial está no MVP com uma única URL PDF, snapshot e escopo de
  consulta; não restam alegações ativas de que está desativada/futura.
- O desenho diferencia requisito planejado de autorização real de egress.
- O RAG-Challenge é owner do OpenAPI; adapters consumidores pertencem aos
  repositórios consumidores.
- Lacunas identificadas por auditoria posterior estão reconciliadas ou
  registradas como ressalvas explícitas do Human Gate.
- Os resultados transversais de handoff, continuidade, Human Gate,
  paralelismo, raciocínio e idioma satisfazem a Definition of Done e o gate de
  trabalho paralelo acima, com autoridades e templates corretamente roteados.

## Verificações por estado

Os itens de ADR-0008/0009/0010 acrescentados abaixo são critérios para
incrementos corretivos e para claims que dependam deles. Não reescrevem
resultados históricos dos estados já encerrados nem constituem evidência de
implementação; o Current State conserva essa separação factual.

| Estado | Verificações adicionais |
|---|---|
| `STATE-01` | Clone/bootstrap limpo, lockfiles, configuração, CI e ausência de domínio prematuro. |
| `STATE-02` | ADRs, contratos, threat model, providers, catálogo inicial 51/54/9, PDF/CSV, fontes/licenças/allowlists, query `pt-BR`/`en-GB`, idioma documental BCP 47, content/page-image storage, quatro políticas de egress, persistência durável, erros/readiness/OpenAPI e rollback. |
| `STATE-03` | Constraints, bancos/categorias/documentos/estados, idioma documental/source declaration, conteúdo fonte/PNG reabrível, render manifest/reachability, hashes, snapshots, journal de observações/freshness separado de `catalogueRevision`, vetores canônicos para `sourceBindingSetDigest` sem observação e `activationBindingSetDigest` completo, staging não consultável, três validações de projeção, manifesto íntegro, retenção, migrations e `CorpusActivationRecord` atômico; rollback constrói registro novo com observações compatíveis/elegíveis. |
| `STATE-04` | Arquitetura, administração de bancos/documentos, parsers PDF/CSV, renderer/manifests/serving de imagem quando autorizados, sync oficial manual, `304`/hash idêntico com campos preservados/alterados exatos, rejeição de mismatch, retry idempotente, hard pre-filter de bindings elegíveis antes do top-k, recuperação unificada, `AnswerEvidenceRecordV1` atômico/minimizado com retenção `P30D` e reachability quando autorizado, OpenAPI v1 preservado e v2 separadamente versionado, query bilíngue, citações, recusa, falhas e adapters. |
| `STATE-05` | Cobertura/proveniência, `interfaceLanguage` `pt-BR`/`en-GB`, temas `Light`/`Dark`, independência de `questionLanguage`, freshness, evidência visual com alternativa textual, estados de UI, teclado, contraste e acessibilidade. |
| `STATE-06` | E2E com HTTP falso, source/render/index restart, backup/restore, serving visual, smoke real opt-in autorizado, artefato e sandbox OCI. |
| `STATE-07` | Dataset estratificado por banco/documento/formato e idioma documental exato, matriz `pt-BR`/`en-GB`, visual-evidence rights/integrity/accessibility, answer-evidence privacy/atomicity/expiry/cleanup race quando implementado, source leakage, language coercion, DNS rebinding/pinning/redirect, stale, groundedness, carga, crash boundaries e recuperação. |
| `STATE-08` | Artefato, egress oficial autorizado, deploy, smoke, health, evidência e rollback. |

## Estratégia de CI

O pipeline inicial deve usar menor privilégio e:

- cancelar execução anterior da mesma ref quando seguro;
- aplicar timeout por job;
- fixar toolchains e actions confiáveis;
- não persistir credenciais do checkout;
- restaurar por lockfile;
- executar build Release, testes, cobertura e format;
- executar lint, type checking, testes e build do Dashboard;
- auditar dependências e secrets;
- validar links Markdown e `git diff --check`;
- não fazer deploy em evento de pull request.

CD exige ambiente, secrets, gate e autorização próprios. CI aprovada não prova
deploy.

## Qualidade RAG

A campanha define antes da execução:

- corpus e versão;
- catálogo/bancos/documentos ativos, formatos, snapshots/freshness e cobertura;
- conjunto de perguntas e casos sem resposta;
- `questionLanguage` e `contentLanguage` de cada caso, com os pares
  `pt-BR→pt-BR`, `en-GB→en-GB`, `pt-BR→en-GB` e `en-GB→pt-BR` cobertos;
- `SupportedQueryLanguage` fechado e `DocumentContentLanguage` BCP 47 exato;
  cada tag documental adicional gera estrato próprio para `pt-BR` e `en-GB`,
  sem coerção ou fusão silenciosa;
- identidade do render manifest e das páginas citadas quando evidência visual
  integrar a candidata;
- providers, modelos, prompts e parâmetros;
- versão do índice;
- rubrica e thresholds;
- ambiente e orçamento;
- critérios de parada.

Medidas candidatas:

- relevância/recall da recuperação;
- precisão de citações;
- correspondência exata entre idioma da resposta e da pergunta;
- preservação do idioma original do texto derivado da fonte nas citações;
- métricas por tag documental exata; `en` nunca integra o denominador `en-GB`;
- integridade/rights do binding página-citação, serving bounded e equivalente
  textual acessível quando a capacidade visual estiver implementada;
- groundedness;
- taxa de respostas indevidas em casos sem evidência;
- latência e custo;
- estabilidade entre versões;
- resistência a prompt injection;
- taxa de proveniência incorreta, cobertura omitida ou fallback indevido;

Não escolher thresholds depois de observar o resultado.

## Severidade de achados

- `P0 Crítica`: exposição ativa, execução indevida, perda/corrupção severa ou
  vazamento de secret.
- `P1 Alta`: defeito grave, resposta perigosa não fundamentada ou regressão
  provável.
- `P2 Média`: impacto limitado ou risco de manutenção relevante.
- `P3 Baixa`: melhoria útil sem risco imediato.

## Human Gate

O validador humano:

- revisa o relatório automático;
- repete amostras críticas;
- confirma experiência, mensagens e limitações;
- verifica distinção entre local/online e planejado/implementado;
- registra decisão, data, ressalvas e evidência sanitizada.

Decisões:

- `PENDENTE`;
- `APROVADO`;
- `APROVADO COM RESSALVAS`;
- `REPROVADO`.

O gate exige um resumo de um único estado que identifica relatório, amostras,
pendências, ressalvas e a decisão solicitada, seguido da frase inequívoca
definida no template. Uma palavra ou confirmação abreviada nunca constitui
Human Gate.

## Amostras humanas por estado

- `STATE-00`: revisar escopo, riscos, arquitetura, ADRs e backlog.
- `STATE-01`: repetir onboarding, build e testes de clone limpo.
- `STATE-02`: walkthrough de threats, providers, catálogo, formatos, fontes e rollback.
- `STATE-03`: revisar bancos/categorias/documentos, snapshots, os dois domínios
  de digest, journal de observações, geração, novo registro de rollback e
  recuperação.
- `STATE-04`: administração, sync oficial, perguntas por banco/formato/idioma,
  recuperação unificada,
  citações no idioma original, sem evidência e falhas.
- `STATE-05`: cobertura/proveniência, idioma e tema; matriz entre
  `interfaceLanguage` e `questionLanguage` executada em `Light` e `Dark`;
  ausência de mistura, contraste, freshness, teclado, reflow, citações e
  erros.
- `STATE-06`: fluxo PDF/CSV local/oficial, restart e configuração de ambiente.
- `STATE-07`: amostra por banco/documento/formato e pelos quatro pares
  `pt-BR`/`en-GB`, SSRF, stale, mismatch/rebinding de observação, ataque, carga
  e rollback sem replay de freshness.
- `STATE-08`: egress, deploy, smoke, health e recuperação.
