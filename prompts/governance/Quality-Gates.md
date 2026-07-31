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
- Cada solicitação possui exatamente um handoff compacto, somente na resposta
  final; comentários intermediários não repetem seus rótulos, roteamento,
  mensagem, raciocínio ou paralelismo.
- Cada comentário intermediário contém informação materialmente nova; não
  repete nem parafraseia conclusão já comunicada sem correção ou consequência
  nova.
- Runtime preflight é aplicado somente a mudança ou validação executável;
  documentação e análise read-only não anunciam shutdown, não enumeram
  processos e não encerram nada.
- Handoff final informa rota, destino coerente e motivo; quando a continuidade
  exige mensagem, traz imediatamente depois o texto completo para copiar e
  enviar. Somente `RETURN_TO_EXISTING` exige conversa verificável, e nenhuma
  rota inventa referência ou amplia autoridade.
- `Sua ação agora`, `Conversa recomendada` e `Texto para copiar e enviar` são
  coerentes: uma orientação de continuar, iniciar, retomar, responder,
  confirmar, decidir, autorizar ou enviar torna o texto integral obrigatório;
  ausência só é válida sem ação dependente de mensagem.
- O texto obrigatório está integralmente no próprio handoff, sem placeholders
  ou remissão a conteúdo anterior; título de `START_NEW` permanece no campo de
  conversa, e mensagens de lanes não substituem o texto principal.
- Handoff separa `Solicitação`, `Próximo trabalho recomendado`, `Estado/gate`
  e `Sua ação agora`; lote, tarefa, atividade, passo, lifecycle, roteamento e
  ação humana não são tratados como sinônimos.
- Handoff classifica o trabalho paralelo; qualquer plano paralelo possui
  coordenadora confirmada, ownership sem sobreposição, mensagem por lane,
  condições de parada, integração serializada e checks transversais finais.
- Handoff recomenda exatamente um dos seis níveis canônicos de raciocínio para
  a próxima conversa, justifica o menor nível suficiente e informa uma
  alternativa; não afirma configuração automática nem usa o nível para
  ampliar autoridade.
- Comunicação e artefatos cumprem a
  [`política de idioma`](Language-Policy.md), incluindo a continuidade
  recomendada e o texto completo em `pt-BR` destinado ao proprietário.

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
| Contrato | Adapters de parser, conteúdo bruto, embeddings, vetor, LLM, OpenAPI e API. |
| Integração | Persistência, geração/ativação atômica, rollback, restart e HTTP. |
| RAG evaluation | Recuperação, groundedness, citações e recusa. |
| Segurança | Arquivo malicioso, prompt injection, SSRF, source leakage, secrets e abuso. |
| Acessibilidade | Teclado, foco, semântica, contraste e reflow. |
| E2E | Documento até resposta e deploy até smoke. |
| Performance | Latência, limites, memória, custo e carga definidos. |
| Recuperação | Falha de indexação, geração incompatível e rollback. |

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
9. Verificar um único encerramento final por solicitação, ausência do bloco em
   atualizações intermediárias e uso apenas dos campos condicionais
   aplicáveis.
10. Verificar que atualizações intermediárias acrescentam informação nova,
    sem repetição semântica, e que runtime preflight foi classificado antes de
    qualquer inspeção de processos.
11. Validar a separação entre solicitação atual, próximo trabalho recomendado,
    estado/gate, ação do proprietário e conversa recomendada; conferir que o
    texto integral está dentro do próprio handoff e aparece imediatamente
    depois da conversa quando há navegação, resposta, decisão ou envio, sem
    combinar ação `nenhuma` ou sentinela de ausência com essa orientação e sem
    placeholders ou remissão a outra mensagem.
12. Validar presença e coerência da recomendação de raciocínio, de sua
    justificativa e de sua alternativa nos handoffs e lanes aplicáveis.
13. Registrar achados com severidade, impacto, reprodução e recomendação.

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
- Continuidade de conversa possui classificação, target, motivo e texto
  completo para copiar e enviar imediatamente depois da conversa quando há
  ação dependente de mensagem; retorno antigo exige referência confirmada e
  reconciliação com Current State; frase de Human Gate permanece na conversa
  do resumo completo.
- Cada solicitação possui um único encerramento compacto na resposta final;
  atualizações intermediárias não repetem o bloco e campos de título, mensagem
  ou plano aparecem somente quando aplicáveis.
- Paralelismo entre conversas possui classificação, coordenadora, ownership,
  mensagens por lane, fallback sequencial e gate que proíbe escrita concorrente
  no workspace atual sem Git/worktree.
- Raciocínio do Codex por conversa usa somente os seis valores canônicos, com
  justificativa, alternativa, caráter consultivo e compatibilidade entre
  `Ultra` e o gate de paralelismo.
- A [`política de idioma`](Language-Policy.md) é a única autoridade temática,
  está corretamente roteada e não conflita com instruções, templates ou
  evidências existentes.

## Verificações por estado

| Estado | Verificações adicionais |
|---|---|
| `STATE-01` | Clone/bootstrap limpo, lockfiles, configuração, CI e ausência de domínio prematuro. |
| `STATE-02` | ADRs, contratos, threat model, providers, corpus/licença, URL oficial/termos, quatro políticas de egress, persistência durável, erros/readiness/OpenAPI e rollback. |
| `STATE-03` | Constraints, conteúdo reabrível, hashes, snapshot imutável, observações/freshness, source scope/digests, staging não consultável, manifesto final com integridade/contagens, retenção, migrations e `CorpusActivationRecord` atômico. |
| `STATE-04` | Arquitetura, sync oficial manual, `304`/hash idêntico e retirada/desativação condicionados ao registro ativo, hard pre-filter antes do top-k, OpenAPI versionado, citações, recusa, idempotência, falhas e adapters. |
| `STATE-05` | Seletor Local/OfficialOnline, freshness, estados de UI, teclado, contraste e acessibilidade. |
| `STATE-06` | E2E com HTTP falso, smoke real opt-in autorizado, restart, artefato e sandbox OCI. |
| `STATE-07` | Dataset por escopo, source leakage, DNS rebinding/pinning/redirect, stale, groundedness, carga, crash boundaries e recuperação. |
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
- `sourceScope`, snapshot/freshness e política de isolamento;
- conjunto de perguntas e casos sem resposta;
- providers, modelos, prompts e parâmetros;
- versão do índice;
- rubrica e thresholds;
- ambiente e orçamento;
- critérios de parada.

Medidas candidatas:

- relevância/recall da recuperação;
- precisão de citações;
- groundedness;
- taxa de respostas indevidas em casos sem evidência;
- latência e custo;
- estabilidade entre versões;
- resistência a prompt injection;
- taxa de mistura/fallback indevido entre fontes;

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
- `STATE-02`: walkthrough de threats, providers, corpus, fonte oficial e rollback.
- `STATE-03`: revisar modelo, snapshot, source scope, geração e recuperação.
- `STATE-04`: sync oficial, perguntas por escopo, sem evidência e falhas.
- `STATE-05`: seletor, freshness, teclado, reflow, citações e erros.
- `STATE-06`: fluxo local/official, restart e configuração de ambiente.
- `STATE-07`: amostra por escopo, SSRF, stale, ataque, carga e rollback.
- `STATE-08`: egress, deploy, smoke, health e recuperação.
