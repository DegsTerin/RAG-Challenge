# Relatório de Discovery do STATE-00

## Status

- Estado: `STATE-00 DISCOVERY`
- Data: 2026-07-29
- Escopo: documentação, análise e preparação de `STATE-01`
- Implementação: nenhuma
- Automatic Quality Gate: `APROVADO` para a baseline documental `3.4.0`
- Human Gate: `PENDENTE`
- Transição: não autorizada

## Execução

O trabalho consolidou:

- requisitos fornecidos pelo proprietário na conversa;
- 23 materiais originais e 1 prompt genérico de governança mantidos
  localmente;
- análise somente leitura do DB-Notifier como referência;
- escopo do MVP e evolução;
- arquitetura, RAG, segurança, qualidade e governança;
- inclusão, por decisão do proprietário, de uma fonte oficial online funcional
  no MVP;
- correção dos contratos de conteúdo bruto, manifesto final, ativação,
  isolamento vetorial, OpenAPI, egress e segurança TLS;
- playbook de continuidade que orienta conversa atual, nova ou anterior,
  informa o próximo passo e fornece mensagem exata em `pt-BR` pronta para
  copiar;
- playbook de paralelismo seguro que classifica a oportunidade, reserva uma
  conversa coordenadora e limita cada frente por responsabilidade exclusiva,
  isolamento, verificações, condições de parada e ordem de integração;
- política de idioma como autoridade temática única, com comunicação em
  `pt-BR`, novos artefatos técnicos em `en-GB`, preservação da evidência
  existente e idioma de interface separado;
- roadmap incremental e backlog;
- estrutura documental de 21 arquivos, composta pelos 20 documentos
  originalmente aprovados e pela política acrescentada em incremento
  versionado.

Não foram criados código, API, banco, índice, interface, infraestrutura,
repositório Git, commit, deploy ou recurso externo.

## Pré-condições e segurança

- A pasta original foi preservada em `reference-materials/challenge-original/`.
- `reference-materials/` permanece excluída pelo `.gitignore`.
- Os 23 materiais originais continuaram legíveis.
- O prompt genérico de coordenação foi arquivado em
  `reference-materials/governance-inputs/` com o SHA-256 original preservado.
- A análise do DB-Notifier usou somente operações de leitura. A ausência de
  alteração é controle observado da sessão, não prova reproduzível a partir
  deste workspace.
- Nenhum secret foi solicitado ou utilizado.
- Nenhum nome real de host foi registrado.
- A aprovação recebida autorizou a documentação; não foi interpretada como
  Human Gate ou entrada em `STATE-01`.
- A decisão posterior de tornar a fonte oficial online funcional no MVP
  autorizou apenas a revisão deste desenho documental. Não autorizou acesso à
  rede, escolha da fonte, implementação, segredo, custo ou infraestrutura.

## Fontes inspecionadas

### Materiais do Challenge

Inventário observado:

- 8 Markdown;
- 14 PDFs;
- 1 PNG.

Fontes principais:

- `Sobre o desafio.md`;
- `RAG ONE BR.md`;
- `Entregáveis do projeto.md`;
- `Entrega do Projeto.md`;
- `Cria sua documentação.md`;
- `Opções de documentação.md`;
- `Para saber mais cursos, artigos e Alura.md`;
- `Trello do Desafio.md`;
- `Trello.png`;
- 14 PDFs de BimBam Buy, Santo Pegasus e Mercado Central 24h.

Os materiais são locais e ignorados; seus caminhos não são links públicos
desta documentação.

### Registro sanitizado das fontes examinadas

Materiais locais são identificados apenas por nome-base, SHA-256 e linhas da
versão examinada. Não se registra caminho absoluto, nome de usuário, host ou
link para `reference-materials/`. O hash identifica a evidência, mas não
publica nem concede licença sobre seu conteúdo.

| ID | Arquivo e SHA-256 | Localizadores usados | Classificação |
|---|---|---|---|
| `SRC-CH-001` | `Sobre o desafio.md` — `8B555DCB3263E03DDD4EABAC8863CD0A50C8431C606AD713A97983AD2DB3975F` | História 9–17; Objetivo 65–73; Requisitos 77–89; Trello 101–121 | Requisito geral; formatos têm interpretação divergente. |
| `SRC-CH-002` | `RAG ONE BR.md` — `BF9EA508E47739CAB447F0E4F9FF88CB509F5598D8C85700CDF523487B8FD43F` | Etapas 25–39; tecnologias/entregas 43–49; avaliação 53–67 | Fluxo mínimo e entrega; tecnologias são sugestões. |
| `SRC-CH-003` | `Entregáveis do projeto.md` — `C4DB7267921E2FB6AE1A83AFF07E3120A5E4BD40D76F4D0829EC5B8242FFB401` | GitHub 11–17; README 21–37; agente 41–45; deploy 48–58 | Entregáveis oficiais. |
| `SRC-CH-004` | `Entrega do Projeto.md` — `098B351E2FF14FF1177D27058B11C891EEEE7E78BFAAAFF3AF5FD3C37FD1C27B` | Restrição 9–17; submissão 29–33 | Regra de submissão, não comportamento do produto. |
| `SRC-CH-005` | `Cria sua documentação.md` — `475ED63862854F925D9EAA71EE8A298A5BBE5FBBB91B5E387A9538B6DD4AD1FC` | Recomendação 99–101 | Liberdade de corpus e formato mínimo sugerido. |
| `SRC-CH-006` | `Opções de documentação.md` — `69B79F8C96EF92F61B481AC63CFA52147BBEBB8C6708C40757F601F44BCFE904` | Exemplos 9–53 | Contextual; não normativa e não usada como corpus. |

O registro contém cinco fontes normativas e uma fonte contextual. A contagem
de seis representa IDs examinados, não seis autoridades normativas.

O manifesto canônico dos 23 materiais possui SHA-256
`DFA42E52BF38DEA990C8FD560B7D0E01C4CB5C95E525A9234460221ACD4AD17F`.
Ele usa linhas `relative/path|bytes|SHA256`, ordenação ordinal, UTF-8, LF e
newline final.
`Trello do Desafio.md`, o material de cursos, o PNG e os 14 PDFs são apoio ou
exemplos e não originam requisitos funcionais. As solicitações próprias do
owner são normalizadas em
[`Prompt-New-Project.md`](../prompts/foundation/Prompt-New-Project.md); a
conversa não recebe hash inventado.

### Referência DB-Notifier

Foram inspecionados, entre outros:

- `AGENTS.md` e `prompts/Start-Here.md`;
- visão, arquitetura e estado factual;
- governança, lifecycle, Quality Gates, segurança e templates;
- Prompt Mestre e changelog do corpus;
- solução, projects, build properties, package management e CI;
- arquitetura, ADRs, testes e convenções.

A referência foi usada para adaptar princípios, não para copiar produto,
estado, módulos ou dependências.

## Requisitos observados do Challenge

Os materiais estabelecem:

- repositório público GitHub;
- código organizado e histórico de commits;
- agente funcional baseado em documento;
- leitura e processamento do documento;
- README com visão, arquitetura, tecnologias, execução, perguntas e respostas
  de exemplo;
- uso de pelo menos um serviço OCI;
- link, imagem ou vídeo que evidencie execução online.

A submissão aceita uma URL do GitHub, e o material local informa limite de
cinco tentativas. O Trello é apoio de organização e não integra os critérios
de avaliação.

Python, LangChain, pypdf, pandas, modelos específicos, Colab e OCI Compute são
sugestões, não obrigações.

## Matriz de rastreabilidade do MVP

| Origem sanitizada | RF/RNF | Critérios | Backlog | Estado e teste/evidência |
|---|---|---|---|---|
| `SRC-CH-001`–`SRC-CH-004` — entrega, GitHub e README | `RF-011`, `RNF-008`, `RNF-010`, `RNF-011`, `RNF-013` | `AC-MVP-001`, `AC-MVP-007`, `AC-MVP-009`, `AC-MVP-013` | `BL-M02`, `BL-M11`, `BL-M13` | S01/S06/S08; clone, CI, E2E e inspeção documental. |
| `SRC-CH-002`, `SRC-CH-003`, `SRC-CH-005` — documento e agente | `RF-001`–`RF-004`, `RNF-005`, `RNF-011` | `AC-MVP-002` | `BL-M01`, `BL-M03`–`BL-M05` | S02–S04/S06; unitário, contrato, integração e E2E. |
| `SRC-CH-002`, `SRC-CH-003` — perguntas baseadas no documento | `RF-005`–`RF-008`, `RNF-004`, `RNF-005`, `RNF-007`, `RNF-008` | `AC-MVP-003`, `AC-MVP-004` | `BL-M06`, `BL-M07`, `BL-M10` | S04/S07; contrato, RAG evaluation, segurança e desempenho. |
| Visão normalizada do owner — substituição segura | `RF-009`, `RNF-005`, `RNF-012` | `AC-MVP-005` | `BL-M03`, `BL-M05`, `BL-M14` | S03/S04/S07; concorrência, idempotência e rollback. |
| Governança, OpenAPI e exposição pública | `RF-010`, `RF-017`, `RNF-002`–`RNF-004`, `RNF-006`, `RNF-007` | `AC-MVP-006`, `AC-MVP-007`, `AC-MVP-011` | `BL-M08`, `BL-M10` | S01/S02/S04/S07; contrato HTTP/OpenAPI do Challenge, segurança, compatibilidade e logs; adapters consumidores são futuros. |
| `SRC-CH-002` e escopo aprovado — UI simples | `RF-005`, `RF-007`, `RF-008`, `RNF-009` | `AC-MVP-010` | `BL-M09`, `BL-M10` | S05–S07; componente, acessibilidade, XSS e E2E. |
| `SRC-CH-001`–`SRC-CH-003` — OCI | `RF-012`, `RNF-003`, `RNF-004`, `RNF-010`, `RNF-011` | `AC-MVP-006`, `AC-MVP-008` | `BL-M12` | S06/S08; artefato, deploy autorizado, smoke e health. |
| Compatibilidade normalizada e ADRs propostos | `RF-015`, `RNF-001`, `RNF-002`, `RNF-012` | `AC-MVP-012` | `BL-M02`, `BL-M05`, `BL-M10`, `BL-C05` | S01/S02/S04; arquitetura, contrato e doubles. |
| Decisão explícita do owner — documentação oficial online no MVP | `RF-016`, `RF-020`, `RNF-014` | `AC-MVP-014`, `AC-MVP-015` | `BL-M15`, `BL-C04` | S02–S08; o Must cobre uma fonte PDF no MVP, enquanto múltiplas fontes/crawling permanecem evolução; contrato, fake local, integração, segurança, RAG evaluation, E2E e smoke sob autoridade própria. |
| Evoluções explicitamente fora do MVP | `RF-013`, `RF-014`, `RF-018`, `RF-019` | Não aplicável ao MVP | `BL-C01`–`BL-C06`, `BL-C08` | Futuro; múltiplas fontes, HTML/crawling, agenda automática, adapters consumidores e demais expansões exigem ADR, autoridade e testes próprios. |

Os thresholds de `AC-MVP-003` e `AC-MVP-004` continuam pendentes e devem ser
definidos antes da campanha. `BL-C07` é opção de hospedagem estática, não
requisito funcional. A matriz registra planejamento, não cobertura executada.

## Ambiguidade de requisitos

`Sobre o desafio.md` formula suporte a PDF, Word, Excel, PowerPoint, Markdown,
CSV, JSON e HTML. Entretanto, `RAG ONE BR.md`, `Entregáveis do projeto.md` e
`Cria sua documentação.md` definem o fluxo mínimo como um PDF ou CSV e tratam
mais documentos como opcionais.

Tratamento proposto:

- MVP: um formato, PDF, com um documento local controlado pelo proprietário e
  um documento oficial obtido de uma URL HTTPS exata e aprovada;
- evolução: demais formatos por adapters e gates próprios;
- risco: confirmar a interpretação antes do Human Gate de arquitetura.

## Conclusão sobre o acervo

Os 14 PDFs formam três acervos fictícios coerentes:

- BimBam Buy: pagamentos, garantia, envios, afiliados e reembolsos;
- Santo Pegasus: engenharia, onboarding, microsserviços e resiliência;
- Mercado Central 24h: compras, atendimento e operação.

Nenhum é um acervo específico de bancos de dados. Como também estão fora do
Git, não podem ser dependência silenciosa de um clone público.

Proposta:

- criar ou selecionar no estado apropriado um PDF autoral/licenciado;
- nome conceptual `Catálogo de Bancos de Dados — MVP`;
- escopo representativo e finito por categorias;
- versão, data, proveniência e licença;
- evitar a alegação impossível de cobrir todos os bancos conhecidos;
- validar fatos com documentação oficial na autoria;
- escolher separadamente uma documentação oficial em PDF, com URL, termos e
  política de atualização aprovados, para o segundo escopo consultável do MVP.

## Padrões do DB-Notifier adotados

- entrada por `AGENTS.md` e `prompts/Start-Here.md`;
- separação de presente, história, decisão e evidência;
- lifecycle `STATE-00` a `STATE-08`;
- Quality Gate e Human Gate independentes;
- Domain/Application com dependências para dentro;
- providers/adapters tipados e fail-closed;
- configuração, erros e observabilidade explícitos;
- testes unitários, arquitetura e integração;
- .NET 10, ASP.NET Core e React/TypeScript como proposta sujeita ao ADR-0001;
- gestão central de dependências, lockfiles e CI seguro;
- status planejado/implementado/testado/deployed separados.

## Padrões do DB-Notifier não copiados

- Agent distribuído;
- Desktop WPF/Tray;
- service control;
- mTLS e protocolo Agent/API;
- dupla persistência SQLite/PostgreSQL;
- SignalR;
- legado PowerShell;
- hosts de sandbox especializados;
- AIOps/automação e governança criptográfica avançada.

Esses elementos não resolvem um requisito do MVP.

## Proposta de arquitetura

- Monólito modular.
- .NET 10/C# e ASP.NET Core, condicionados à aceitação do ADR-0001.
- React/TypeScript para interface mínima, na mesma condição.
- Um deployable inicial.
- Portas para fonte, conteúdo bruto, parser, chunker, embeddings, vetor, LLM,
  catálogo e gerações.
- Documento e geração de índice imutáveis/versionados.
- Bytes persistidos por hash e reabertos antes da ativação; vector store guarda
  apenas derivados.
- Um acervo lógico com dois escopos fixos no MVP: `Local` e
  `OfficialOnline`.
- Um PDF local controlado pelo proprietário e um PDF oficial em uma URL HTTPS
  exata aprovada, ambos processados pelo mesmo parser.
- Sincronização manual da fonte oficial para snapshot imutável; consultas
  usam o snapshot e nunca acessam a rede durante a pergunta.
- Filtro obrigatório por escopo antes do `top-k`, sem mistura ou fallback
  implícito entre fontes.
- Staging não consultável, manifesto final com digest/contagens dos artefatos e
  uma geração combinada de índice.
- `CorpusActivationRecord` versionado vincula atomicamente geração, snapshot e
  observação; histórico completo preserva rollback dos dois escopos.
- API/OpenAPI v1 pertencentes ao Challenge e Dashboard sem acesso direto a
  providers; adapters consumidores ficam em seus próprios repositórios.
- Dashboard com escolha explícita entre `Documento local` e
  `Documentação oficial online — snapshot sincronizado`.
- OCI como runtime público obrigatório.
- GitHub Pages somente como frontend estático opcional.
- Zero dependência do DB-Notifier.

ADRs permanecem `proposed`.

## Riscos e tratamento

| ID | Risco | Impacto | Tratamento |
|---|---|---|---|
| `RISK-001` | Corpus sem licença de redistribuição. | Bloqueia GitHub/deploy. | Aprovar conteúdo e licença antes de implementar. |
| `RISK-002` | Expandir formatos ou fontes além dos dois escopos aprovados. | Prazo e testes comprometidos. | Fixar PDF, uma fonte local e uma fonte oficial; adapters futuros. |
| `RISK-003` | Resposta sem evidência. | Informação incorreta. | Citações, validação e recusa explícita. |
| `RISK-004` | Acoplamento a um SDK/provider. | Refatoração futura. | Portas tipadas e testes de arquitetura. |
| `RISK-005` | Conteúdo bruto ausente, staging parcial, índice corrompido ou ativação dividida. | Restart/rebuild/rollback inviável, indisponibilidade ou respostas mistas. | Conteúdo reabrível por hash, manifesto final com integridade/contagens, candidato não consultável e histórico atômico do `CorpusActivationRecord`. |
| `RISK-006` | Secret no Git ou frontend. | Exposição de conta/custo. | Secret store, scan e configuração fail-closed. |
| `RISK-007` | Prompt injection em documento. | Desvio de resposta/política. | Contexto não confiável, sem tools e testes. |
| `RISK-008` | Sincronização externa vulnerável a SSRF, DNS/TOCTOU, autenticação indevida, egress lateral TLS, redirecionamento ou conteúdo impróprio. | Exposição de rede/segredo, licença, indisponibilidade e instabilidade. | URL pública exata, HTTPS, DNS/IP/Host/SNI pinados, política TLS decidida/testada, redirects bloqueados, limites, assinatura PDF, termos e snapshot. |
| `RISK-009` | Confundir GitHub Pages com backend. | Entrega não funcional/sem OCI. | Backend OCI; Pages opcional. |
| `RISK-010` | Provider, vector store gerenciado, custo ou região indisponível/incompatível. | Bloqueia E2E/deploy ou expõe dados por egress indevido. | Decisão, classificação de dados, política de egress e spike controlado no STATE-02. |
| `RISK-011` | Testes externos não determinísticos. | CI instável e custos. | Fakes offline; externos opt-in. |
| `RISK-012` | Integração prematura ao DB-Notifier ou ownership ambíguo do contrato. | Acoplamento e expansão de escopo. | Challenge publica OpenAPI; adapter futuro pertence ao DB-Notifier e a seus gates. |
| `RISK-013` | Fonte oficial removida, alterada, desatualizada, desativada ou com termos incompatíveis. | Escopo oficial indisponível ou evidência inadequada. | Observações versionadas, transições explícitas de retirada/desativação, frescor fail-closed e nova aprovação para trocar URL/termos. |

## Premissas

- O produto continuará independente.
- O proprietário aceitará uma decisão separada de licença.
- Um PDF publicável será criado ou fornecido antes da implementação RAG.
- Existirá uma documentação oficial em PDF compatível com os limites técnicos,
  os termos de uso e a política de redistribuição/snapshot aprovada.
- Consulta anônima limitada é suficiente para o MVP.
- A interface será intencionalmente simples.
- Providers externos serão escolhidos somente após custo e disponibilidade
  serem verificados.
- Testes normais da fonte oficial usarão servidor falso local; qualquer teste
  com a URL real dependerá de autoridade explícita no estado proprietário.

## Decisões pendentes

1. ADR-0001, licença, mapa físico/módulos e forma administrativa no
   `GATE-B01`.
2. Conteúdo, lista de bancos e licença do PDF inicial.
3. URL oficial pública exata, termos/licença, limites, `maxAge`, trust,
   revogação e eventual material TLS/egress auxiliar.
4. Parser, normalização e chunking.
5. Embedding provider/model.
6. Vector store e, se gerenciado, egress/tratamento de dados.
7. Language model.
8. Persistência e retenção duráveis de conteúdo bruto, catálogo e índice.
9. Serviço, região e orçamento OCI.
10. Dataset, rubrica e thresholds de avaliação.

Esses dez grupos de decisão não bloqueiam o encerramento documental do
Discovery, mas
bloqueiam a implementação nos estados proprietários.

O ADR-0001 e o grupo 1 pertencem ao `GATE-B01`, antes de `STATE-01`. A
licença do corpus e os grupos 2–10 permanecem em `STATE-02` ou no estado
proprietário. O ADR-0002 também permanece proposto para decisão no
`STATE-02`. Nenhuma delas é aceita pelo Human Gate de `STATE-00`.

## Entregáveis

Foram definidos 21 arquivos documentais. Os itens 1–20 compõem a estrutura
originalmente aprovada; o item 21 foi acrescentado pelo incremento `3.4.0`:

1. `AGENTS.md`
2. `README.md`
3. `prompts/Start-Here.md`
4. `prompts/foundation/Prompt-New-Project.md`
5. `prompts/foundation/Solution-Architecture-Document.md`
6. `prompts/foundation/RAG-Module.md`
7. `prompts/governance/Governance.md`
8. `prompts/governance/Lifecycle.md`
9. `prompts/governance/Quality-Gates.md`
10. `prompts/governance/Security-And-Access.md`
11. `prompts/state/Current-State.md`
12. `prompts/state/State-Transition-Log.md`
13. `prompts/templates/Templates.md`
14. `prompts/system/Prompt-System-Change-Log.md`
15. `docs/README.md`
16. `docs/STATE-00-Discovery-Report.md`
17. `docs/MVP-Roadmap-And-Backlog.md`
18. `docs/architecture/README.md`
19. `docs/architecture/ADR-0001-Runtime-Stack-And-Modular-Monolith.md`
20. `docs/architecture/ADR-0002-RAG-Lifecycle-Providers-And-Source-Separation.md`
21. `prompts/governance/Language-Policy.md`

## Auditoria automática

### Iteração 2.0.0 — evidência histórica

Esta aprovação foi válida para a baseline anterior, na qual a documentação
oficial online ainda era uma evolução futura. Ela foi reaberta quando o
proprietário incluiu essa funcionalidade no MVP e não representa a
classificação vigente.

Ambiente da auditoria:

| Item | Valor |
|---|---|
| Data | 2026-07-29 |
| Diretório | `<challenge-root>` |
| Sistema | Windows NT `10.0.26200.0` |
| PowerShell | `7.6.4` |
| ripgrep | `15.2.0` |
| Git CLI | `2.55.0.windows.3`; nenhum repositório inicializado |

Todos os comandos aprovados abaixo terminaram com exit code `0`.

| Gate | Comando/operação somente leitura | Resultado | Classificação |
|---|---|---|---|
| `QG-S00-01` Autoridade | Leitura de `AGENTS.md`, Start Here, Current State, governance, lifecycle e status dos ADRs | `STATE-00` ativo; Human Gate pendente; ADR-0001/0002 `proposed`; `STATE-01` não autorizado. | `APROVADO` |
| `QG-S00-02` Inventário | `Get-ChildItem -Recurse -File` e `rg --files` | 20 Markdown esperados, 20 encontrados, 0 ausentes, 0 inesperados; 21 arquivos fora de `reference-materials/`, sendo `.gitignore` mais os 20 documentos. | `APROVADO` |
| `QG-S00-03` Links e formato | Auditoria PowerShell inline de links, UTF-8, LF, newline final, trailing whitespace, NUL, H1, fences e tabelas | 69 links locais, 0 quebrados; 0 problema de formato, fence, tabela ou heading duplicado. | `APROVADO` |
| `QG-S00-04` IDs e rastreabilidade | Parser PowerShell das definições e matriz `fonte → requisito → aceite → backlog → estado/teste` | 19 RF, 13 RNF, 13 AC, 30 itens de backlog, 8 módulos, 12 riscos e 6 fontes normativas; 0 ID duplicado. | `APROVADO` |
| `QG-S00-05` Segurança documental | `rg` e auditoria PowerShell para padrões de secrets, caminhos pessoais, hosts e referências diretas de projeto | 0 achado de secret/host/caminho pessoal; 0 `ProjectReference` ou dependência do DB-Notifier. | `APROVADO` |
| `QG-S00-06` Escopo negativo | `Test-Path .git` e inventário de extensões fora dos materiais locais | Sem `.git`; 0 arquivo de código, projeto, dependência, lockfile, API, banco, UI, container ou pipeline. | `APROVADO` |
| `QG-S00-07` Materiais locais | `Get-FileHash -Algorithm SHA256`, contagem e manifesto canônico | 23 arquivos legíveis, 7.054.476 bytes: 8 Markdown, 14 PDFs e 1 PNG; manifesto `DFA42E52BF38DEA990C8FD560B7D0E01C4CB5C95E525A9234460221ACD4AD17F`; uma regra `/reference-materials/` no `.gitignore`. | `APROVADO` |
| `QG-S00-08` Revisão semântica | Três revisões independentes somente leitura e reconciliação central | Ambiguidades de autoridade, rollback, ativação, egress, compatibilidade, rastreabilidade e SemVer corrigidas; reauditoria arquitetural sem P0–P3 residual. | `APROVADO` |

Build, testes de código, cobertura, runtime, Git diff e OCI são
`NÃO APLICÁVEL` porque este lote não contém implementação nem repositório Git.

Na linha histórica `QG-S00-04`, “6 fontes normativas” foi uma terminologia
imprecisa da iteração `2.0.0`. O inventário sempre possuiu cinco fontes
normativas e uma contextual; nenhum ID ou material foi alterado.

### Iteração 3.0.0 — revalidação histórica

Esta aprovação permanece evidência da baseline `3.0.0`, mas foi sucedida pela
correção `3.0.1`. Ela não representa a classificação vigente, não aprova o
Human Gate e não autoriza transição.

Ambiente revalidado:

| Item | Valor |
|---|---|
| Data | 2026-07-29 |
| Diretório | `<challenge-root>` |
| Sistema | Windows NT `10.0.26200.0` |
| PowerShell | `7.6.4` |
| ripgrep | `15.2.0` |
| Git CLI | `2.55.0.windows.3`; nenhum repositório inicializado |

Os checks finais abaixo terminaram com exit code `0`.

| Gate | Comando/operação somente leitura | Resultado | Classificação |
|---|---|---|---|
| `QG-S00-01` Autoridade | Releitura de instruções, estado, governança, lifecycle, gates e ADRs | `STATE-00` ativo; Human Gate pendente; ADR-0001/0002 `proposed`; rede, Git, implementação e `STATE-01` não autorizados. | `APROVADO` |
| `QG-S00-02` Inventário | Comparação exata do inventário aprovado com `Get-ChildItem` | 20 documentos esperados e encontrados; 0 ausente ou inesperado; 21 arquivos fora dos materiais locais, sendo `.gitignore` mais os 20 documentos. | `APROVADO` |
| `QG-S00-03` Links e formato | Parser PowerShell de links, UTF-8, LF, newline final, whitespace, NUL, headings, fences e tabelas | 69 links locais, 0 quebrado; 0 URL externa real e 0 problema de formato ou estrutura. | `APROVADO` |
| `QG-S00-04` IDs e rastreabilidade | Parser das definições e matriz `fonte → requisito → aceite → backlog → estado/teste` | 20 RF, 14 RNF, 15 AC, 31 itens de backlog, 8 módulos, 13 riscos e 6 fontes registradas — 5 normativas e 1 contextual; 0 ID duplicado ou inexistente. | `APROVADO` |
| `QG-S00-05` Segurança documental | Scan de padrões de secrets, caminhos pessoais, hosts, URLs reais e referências de projeto | 0 achado; nenhuma dependência ou `ProjectReference` do DB-Notifier. | `APROVADO` |
| `QG-S00-06` Escopo negativo | `Test-Path .git`, inventário de extensões e inspeção de autoridade | Sem `.git`; 0 código, projeto, dependência, lockfile, API, banco, UI, container, pipeline, acesso de rede ou transição. | `APROVADO` |
| `QG-S00-07` Materiais locais | SHA-256, contagem, bytes, tipos e regra do `.gitignore` | 23 arquivos, 7.054.476 bytes: 8 Markdown, 14 PDFs e 1 PNG; manifesto `DFA42E52BF38DEA990C8FD560B7D0E01C4CB5C95E525A9234460221ACD4AD17F`; uma regra `/reference-materials/`. | `APROVADO` |
| `QG-S00-08` Revisão semântica | Três revisões independentes somente leitura, correção central e rechecagem focal | Autoridade, freshness, snapshot/observação, isolamento, rollback, egress, HTTP, critérios e rastreabilidade reconciliados; 0 achado P0–P3 residual. | `APROVADO` |

Build, testes de código, cobertura, runtime, Git diff, rede e OCI são
`NÃO APLICÁVEL` porque este lote não contém implementação, repositório Git ou
ação externa.

### Iteração 3.0.1 — revalidação corretiva histórica

Esta aprovação permanece evidência da baseline `3.0.1`, mas foi sucedida pelo
playbook transversal `3.1.0`. Ela não representa a classificação vigente, não
aprova o Human Gate e não autoriza transição.

Ambiente revalidado:

| Item | Valor |
|---|---|
| Data | 2026-07-29 |
| Diretório | `<challenge-root>` |
| Sistema | Windows NT `10.0.26200.0` |
| PowerShell | `7.6.4` |
| ripgrep | `15.2.0` |
| Git CLI | `2.55.0.windows.3`; nenhum repositório inicializado |

Os checks finais abaixo terminaram com exit code `0`.

| Gate | Comando/operação somente leitura | Resultado | Classificação |
|---|---|---|---|
| `QG-S00-01` Autoridade | Releitura de instruções, estado, governança, lifecycle, gates e ADRs | `STATE-00` ativo; Human Gate pendente; ADR-0001/0002 `proposed`; rede, Git, implementação e `STATE-01` não autorizados. | `APROVADO` |
| `QG-S00-02` Inventário | Comparação exata do inventário aprovado com `Get-ChildItem` e `rg --files` | 20 documentos esperados e encontrados; 0 ausente ou inesperado; 21 arquivos fora dos materiais locais, sendo `.gitignore` mais os 20 documentos. | `APROVADO` |
| `QG-S00-03` Links e formato | Parser PowerShell de links, UTF-8, LF, newline final, whitespace, NUL, headings, fences e tabelas | 71 links locais, 0 quebrado; 0 URL externa real e 0 problema de formato ou estrutura. | `APROVADO` |
| `QG-S00-04` IDs e rastreabilidade | Parser das definições e matriz `fonte → requisito → aceite → backlog → estado/teste` | 20 RF, 14 RNF, 15 AC, 31 itens de backlog, 8 módulos, 13 riscos e 6 fontes registradas — 5 normativas e 1 contextual; 0 ID duplicado ou inexistente. | `APROVADO` |
| `QG-S00-05` Segurança documental | Scan de secrets, caminhos pessoais, hosts, URLs reais, dependências e contratos de egress | 0 achado de exposição; nenhuma dependência ou `ProjectReference` do DB-Notifier; URL pública sem credenciais e políticas de IA, vetor, fonte oficial e OCI separadas. | `APROVADO` |
| `QG-S00-06` Escopo negativo | `Test-Path .git`, inventário de extensões e inspeção de autoridade | Sem `.git`; 0 código, projeto, dependência, lockfile, API, banco, UI, container, pipeline, acesso de rede ou transição. | `APROVADO` |
| `QG-S00-07` Materiais locais | SHA-256, contagem, bytes, tipos e regra do `.gitignore` | 23 arquivos, 7.054.476 bytes: 8 Markdown, 14 PDFs e 1 PNG; manifesto `DFA42E52BF38DEA990C8FD560B7D0E01C4CB5C95E525A9234460221ACD4AD17F`; uma regra `/reference-materials/`. | `APROVADO` |
| `QG-S00-08` Revisão semântica | Três revisões independentes somente leitura, correção central e rechecagem focal | Conteúdo reabrível, manifesto final, ativação/freshness, isolamento, rollback, TLS/egress, OpenAPI/ownership, governança e rastreabilidade reconciliados; 0 achado P0–P3 residual. | `APROVADO` |

Build, testes de código, cobertura, runtime, Git diff, rede e OCI são
`NÃO APLICÁVEL` porque este lote não contém implementação, repositório Git ou
ação externa.

### Iteração 3.1.0 — continuidade entre conversas histórica

Esta aprovação permanece evidência da baseline `3.1.0`, mas foi sucedida pelo
playbook de paralelismo seguro `3.2.0`. Ela não representa a classificação
vigente, não aprova o Human Gate e não autoriza transição.

Classificação global: `APROVADO` para o escopo documental de `STATE-00`.
Isso não aprova o Human Gate e não autoriza transição.

Ambiente revalidado:

| Item | Valor |
|---|---|
| Data | 2026-07-29 |
| Diretório | `<challenge-root>` |
| Sistema | Windows NT `10.0.26200.0` |
| PowerShell | `7.6.4` |
| ripgrep | `15.2.0` |
| Git CLI | `2.55.0.windows.3`; nenhum repositório inicializado |

Os checks finais abaixo terminaram com exit code `0`.

| Gate | Comando/operação somente leitura | Resultado | Classificação |
|---|---|---|---|
| `QG-S00-01` Autoridade | Releitura de AGENTS, Start Here, estado, governança, quality gates e templates | `STATE-00` ativo; Human Gate pendente; ADR-0001/0002 `proposed`; o roteamento de conversa não concede autoridade. | `APROVADO` |
| `QG-S00-02` Inventário | Comparação exata do inventário aprovado com `Get-ChildItem` e `rg --files` | 20 documentos esperados e encontrados; 0 ausente ou inesperado; 21 arquivos fora dos materiais locais. | `APROVADO` |
| `QG-S00-03` Links e formato | Parser PowerShell de links, UTF-8, LF, newline final, whitespace, NUL, headings, fences e tabelas | 73 links locais, 0 quebrado; 0 URL externa real e 0 problema de formato ou estrutura. | `APROVADO` |
| `QG-S00-04` IDs e rastreabilidade | Parser das definições e matriz vigente | 20 RF, 14 RNF, 15 AC, 31 itens de backlog, 8 módulos, 13 riscos e 6 fontes registradas; 0 ID duplicado ou inexistente. | `APROVADO` |
| `QG-S00-05` Continuidade | Revisão de AGENTS, governança, template e Quality Gate | As três ações possuem target coerente, motivo e texto exato; retorno exige referência confirmada; Human Gate permanece com seu resumo; contexto antigo é reconciliado com Current State; 0 ampliação de autoridade. | `APROVADO` |
| `QG-S00-06` Segurança e escopo negativo | Scan de secrets, caminhos pessoais, URLs reais, extensões e `.git` | 0 exposição; sem `.git`, código, dependência, rede, OCI, GitHub, DB-Notifier ou transição. | `APROVADO` |
| `QG-S00-07` Materiais locais | SHA-256, contagem, bytes, tipos e `.gitignore` | 23 arquivos, 7.054.476 bytes; manifesto `DFA42E52BF38DEA990C8FD560B7D0E01C4CB5C95E525A9234460221ACD4AD17F`; regra `/reference-materials/` preservada. | `APROVADO` |
| `QG-S00-08` Revisão semântica | Revisão independente e reconciliação central do snapshot final | Human Gate, targets, títulos propostos, placeholders e autoridade reconciliados; 0 achado P0–P3 residual. | `APROVADO` |

Build, testes de código, cobertura, runtime, Git diff, rede e OCI são
`NÃO APLICÁVEL` porque este lote não contém implementação, repositório Git ou
ação externa.

### Iteração 3.2.0 — paralelismo seguro entre conversas histórica

Esta aprovação permanece evidência da baseline `3.2.0`, mas foi sucedida pela
política linguística `3.3.0`. Ela não representa a classificação vigente, não
aprova o Human Gate e não autoriza transição.

Classificação global: `APROVADO` para o escopo documental de `STATE-00`.
Isso não aprova o Human Gate e não autoriza transição.

Ambiente revalidado:

| Item | Valor |
|---|---|
| Data | 2026-07-29 |
| Diretório | `<challenge-root>` |
| Sistema | Windows NT `10.0.26200.0` |
| PowerShell | `7.6.4` |
| ripgrep | `15.2.0` |
| Git CLI | `2.55.0.windows.3`; nenhum repositório inicializado |

Os checks finais abaixo terminaram com exit code `0`.

| Gate | Comando/operação somente leitura | Resultado | Classificação |
|---|---|---|---|
| `QG-S00-01` Autoridade | Releitura de AGENTS, Start Here, estado, governança, Quality Gates e templates | `STATE-00` ativo; Human Gate pendente; ADR-0001/0002 `proposed`; paralelismo não concede autoridade. | `APROVADO` |
| `QG-S00-02` Inventário | Comparação exata do inventário aprovado com `Get-ChildItem` e `rg --files` | 20 documentos esperados e encontrados; 0 ausente ou inesperado; 21 arquivos fora dos materiais locais. | `APROVADO` |
| `QG-S00-03` Links e formato | Parser PowerShell de links, UTF-8, LF, newline final, whitespace, NUL, headings, fences e tabelas | 73 links locais, 0 quebrado; 0 URL externa real e 0 problema de formato ou estrutura. | `APROVADO` |
| `QG-S00-04` IDs e rastreabilidade | Parser das definições e matriz vigente | 20 RF, 14 RNF, 15 AC, 31 itens de backlog, 8 módulos, 13 riscos e 6 fontes registradas; 0 ID duplicado ou inexistente. | `APROVADO` |
| `QG-S00-05` Conversas e paralelismo | Revisão de AGENTS, governança, templates e Quality Gates | Rota principal preservada; três classificações de paralelismo coerentes; coordenadora confirmada, snapshot, ownership, mensagens, stop/fallback e integração serializada obrigatórios. | `APROVADO` |
| `QG-S00-06` Segurança e escopo negativo | Scan de secrets, caminhos pessoais, URLs reais, extensões e `.git` | 0 exposição; sem `.git`, código, dependência, rede, OCI, GitHub, DB-Notifier ou transição; workers read-only na situação vigente. | `APROVADO` |
| `QG-S00-07` Materiais locais | SHA-256, contagem, bytes, tipos e `.gitignore` | 23 arquivos, 7.054.476 bytes; manifesto `DFA42E52BF38DEA990C8FD560B7D0E01C4CB5C95E525A9234460221ACD4AD17F`; regra `/reference-materials/` preservada. | `APROVADO` |
| `QG-S00-08` Revisão semântica | Revisão independente e reconciliação central do snapshot final | Ownership físico/lógico, dependências, isolamento, workers, retorno, integração, Human Gate e autoridade reconciliados; 0 achado P0–P3 residual. | `APROVADO` |

Build, testes de código, cobertura, runtime, Git diff, rede e OCI são
`NÃO APLICÁVEL` porque este lote não contém implementação, repositório Git ou
ação externa. A política foi validada documentalmente; escrita paralela em
worktrees será verificável somente depois de Git e do estado correspondente
serem autorizados.

### Iteração 3.3.0 — política linguística histórica

Esta aprovação foi sucedida pela correção `3.3.1` depois que a revisão
independente encontrou uma regra residual e sentinelas owner-facing
inconsistentes. A versão permanece como evidência histórica, não representa o
snapshot vigente, não aprova o Human Gate e não autoriza transição.

Classificação global: `APROVADO` para o escopo documental de `STATE-00`.
Isso não aprova o Human Gate e não autoriza transição.

Ambiente revalidado:

| Item | Valor |
|---|---|
| Data | 2026-07-29 |
| Diretório | `<challenge-root>` |
| Sistema | Windows NT `10.0.26200.0` |
| PowerShell | `7.6.4` |
| ripgrep | `15.2.0` |
| Git CLI | `2.55.0.windows.3`; nenhum repositório inicializado |

Os checks finais abaixo terminaram com exit code `0`.

| Gate | Comando/operação somente leitura | Resultado | Classificação |
|---|---|---|---|
| `QG-S00-01` Autoridade | Releitura de AGENTS, Start Here, estado, templates, Quality Gates e changelog | `STATE-00` ativo; Human Gate pendente; ADR-0001/0002 `proposed`; política de idioma não concede autoridade. | `APROVADO` |
| `QG-S00-02` Inventário | Comparação exata do inventário aprovado com `Get-ChildItem` e `rg --files` | 20 documentos esperados e encontrados; 0 ausente ou inesperado; 21 arquivos fora dos materiais locais. | `APROVADO` |
| `QG-S00-03` Links e formato | Parser PowerShell de links, UTF-8, LF, newline final, whitespace, NUL, headings, fences e tabelas | 73 links locais, 0 quebrado; 0 URL externa real e 0 problema de formato ou estrutura. | `APROVADO` |
| `QG-S00-04` IDs e rastreabilidade | Parser das definições e matriz vigente | 20 RF, 14 RNF, 15 AC, 31 itens de backlog, 8 módulos, 13 riscos e 6 fontes registradas; 0 ID duplicado ou inexistente. | `APROVADO` |
| `QG-S00-05` Política linguística | Revisão de AGENTS, Start Here, Solution Architecture, templates, Quality Gates, estado e histórico | Comunicação `pt-BR`; novos artefatos técnicos `en-GB`; nomes externos preservados; baseline existente não traduzida; interface separada. | `APROVADO` |
| `QG-S00-06` Segurança e escopo negativo | Scan de secrets, caminhos pessoais, URLs reais, extensões e `.git` | 0 exposição; sem `.git`, código, dependência, tradução em massa, rede, OCI, GitHub, DB-Notifier ou transição. | `APROVADO` |
| `QG-S00-07` Materiais locais | SHA-256, contagem, bytes, tipos e `.gitignore` | 23 arquivos, 7.054.476 bytes; manifesto `DFA42E52BF38DEA990C8FD560B7D0E01C4CB5C95E525A9234460221ACD4AD17F`; regra `/reference-materials/` preservada. | `APROVADO` |
| `QG-S00-08` Revisão semântica | Revisão independente e reconciliação central do snapshot final | Conversa, artefatos, exceções, documentos existentes, interface, templates, versão e autoridade reconciliados; 0 achado P0–P3 residual. | `APROVADO` |

Build, testes de código, cobertura, runtime, Git diff, rede e OCI são
`NÃO APLICÁVEL` porque este lote não contém implementação, repositório Git ou
ação externa. A política será verificável sobre artefatos `en-GB` quando eles
forem autorizados e existirem.

### Iteração 3.3.1 — correção linguística vigente

Classificação global: `APROVADO` para o escopo documental de `STATE-00`.
Isso não aprova o Human Gate e não autoriza transição.

Ambiente revalidado:

| Item | Valor |
|---|---|
| Data | 2026-07-29 |
| Diretório | `<challenge-root>` |
| Sistema | Windows NT `10.0.26200.0` |
| PowerShell | `7.6.4` |
| ripgrep | `15.2.0` |
| Git CLI | `2.55.0.windows.3`; nenhum repositório inicializado |

Os checks finais abaixo terminaram com exit code `0`.

| Gate | Comando/operação somente leitura | Resultado | Classificação |
|---|---|---|---|
| `QG-S00-01` Autoridade | Releitura de AGENTS, Start Here, estado, arquitetura, templates, Quality Gates e changelog | `STATE-00` ativo; Human Gate pendente; ADR-0001/0002 `proposed`; correção de idioma não concede autoridade. | `APROVADO` |
| `QG-S00-02` Inventário | Comparação exata do inventário aprovado com `Get-ChildItem` e `rg --files` | 20 documentos esperados e encontrados; 0 ausente ou inesperado; 21 arquivos fora dos materiais locais. | `APROVADO` |
| `QG-S00-03` Links e formato | Parser PowerShell de links, UTF-8, LF, newline final, whitespace, NUL, headings, fences e tabelas | 73 links locais, 0 quebrado; 0 URL externa real e 0 problema de formato ou estrutura. | `APROVADO` |
| `QG-S00-04` IDs e rastreabilidade | Parser das definições e matriz vigente | 20 RF, 14 RNF, 15 AC, 31 itens de backlog, 8 módulos, 13 riscos e 6 fontes registradas; 0 ID duplicado ou inexistente. | `APROVADO` |
| `QG-S00-05` Política linguística | Revisão transversal e busca de regras/sentinelas conflitantes | Conversa `pt-BR`; novos artefatos `en-GB`; nomes externos preservados; documentos existentes sem mistura; UI separada; prosa owner-facing em `pt-BR`. | `APROVADO` |
| `QG-S00-06` Segurança e escopo negativo | Scan de secrets, caminhos pessoais, URLs reais, extensões e `.git` | 0 exposição; sem `.git`, código, dependência, tradução em massa, rede, OCI, GitHub, DB-Notifier ou transição. | `APROVADO` |
| `QG-S00-07` Materiais locais | SHA-256, contagem, bytes, tipos e `.gitignore` | 23 arquivos, 7.054.476 bytes; manifesto `DFA42E52BF38DEA990C8FD560B7D0E01C4CB5C95E525A9234460221ACD4AD17F`; regra `/reference-materials/` preservada. | `APROVADO` |
| `QG-S00-08` Revisão semântica | Duas revisões independentes, correção central e rechecagem final | Regra de arquitetura, sentinelas, SemVer, estado, histórico, relatório, autoridade e escopo reconciliados; 0 achado P0–P3 residual. | `APROVADO` |

Build, testes de código, cobertura, runtime, Git diff, rede e OCI são
`NÃO APLICÁVEL` porque este lote não contém implementação, repositório Git ou
ação externa. Artefatos futuros em `en-GB` serão verificados quando existirem.

### Iteração 3.4.0 — autoridade temática de idioma vigente

Classificação global: `APROVADO` para o escopo documental de `STATE-00`.
Isso não aprova o Human Gate e não autoriza transição.

Esta iteração substitui somente a classificação vigente da baseline `3.3.1`.
Todas as iterações anteriores permanecem como evidência histórica.

Ambiente revalidado:

| Item | Valor |
|---|---|
| Data | 2026-07-29 |
| Diretório | `<challenge-root>` |
| Sistema | Windows NT `10.0.26200.0` |
| PowerShell | `7.6.4` |
| ripgrep | `15.2.0` |
| Git | não executado; ausência de `.git` confirmada pelo inventário local |

As verificações documentais locais abaixo terminaram com código de saída `0`.

| Gate | Operação somente leitura | Resultado | Classificação |
|---|---|---|---|
| `QG-S00-01` Autoridade | Releitura de AGENTS, Start Here, política de idioma, estado, governança, templates, Quality Gates e changelog | `STATE-00` ativo; Human Gate pendente; ADR-0001/0002 `proposed`; a política temática não concede execução. | `APROVADO` |
| `QG-S00-02` Inventário | Comparação exata com `Get-ChildItem` e `rg --files` | 21 documentos públicos, 22 arquivos fora dos materiais locais, 13 documentos ativos em `prompts/` e 24 materiais locais. | `APROVADO` |
| `QG-S00-03` Links e formato | Parser PowerShell de links, UTF-8, LF, newline final, whitespace e NUL | 96 links locais, 0 quebrado e 0 problema de formato. | `APROVADO` |
| `QG-S00-04` IDs e rastreabilidade | Contagem de identificadores estáveis e matriz vigente | 20 RF, 14 RNF, 15 AC, 31 itens de backlog, 8 módulos e 13 riscos; contagens funcionais inalteradas. | `APROVADO` |
| `QG-S00-05` Idioma e comunicação | Revisão de autoridade, referências, labels e mensagens destinadas ao proprietário | Uma autoridade temática; comunicação, próximo passo e mensagem exata em `pt-BR`; literais técnicos preservados; nenhuma tradução em massa ou decisão de interface. | `APROVADO` |
| `QG-S00-06` Segurança e escopo negativo | Busca de secrets, caminhos pessoais, URLs reais, implementação e autoridade externa | 0 exposição; sem `.git`, código, dependência, rede, OCI, GitHub, DB-Notifier ou transição. | `APROVADO` |
| `QG-S00-07` Materiais locais | Contagem, bytes, SHA-256 e regra `.gitignore` | 23 materiais originais, 7.054.476 bytes e manifesto anterior preservados; prompt arquivado com 11.131 bytes e SHA-256 `0019950242314908762CAD3E2AEA01C122023E3867885289E04FB3A70CA912D4`; regra `/reference-materials/` preservada. | `APROVADO` |
| `QG-S00-08` Revisão semântica | Revisão independente somente leitura e reconciliação central | Dois achados P2 foram corrigidos: um qualificador residual em AGENTS restringia o alcance da regra e Templates não possuía o bloco canônico completo em `pt-BR`; a rechecagem deixou 0 achado P0–P3 residual. | `APROVADO` |

Build, testes de código, cobertura, runtime, Git diff, rede e OCI são
`NÃO APLICÁVEL` porque este lote não contém implementação, repositório Git ou
ação externa.

## Limitações

- Os 14 PDFs tiveram inventário, tamanho, hash e contagem estrutural de páginas
  verificados. Uma análise de conteúdo separada cobriu o texto dos documentos.
- Poppler não estava disponível. Como alternativa, PyMuPDF renderizou todas as
  páginas e contact sheets foram inspecionadas visualmente, sem anomalia
  evidente; isso não equivale a comparação pixel-perfect em tamanho integral.
- O PNG do Trello foi inspecionado visualmente e no inventário de requisitos.
- A ausência de alterações no DB-Notifier foi controlada pelo uso somente
  leitura durante a sessão, mas não é reproduzível a partir deste workspace
  isolado.
- Três tentativas auxiliares somente leitura foram descartadas: uma expressão
  regular inválida em `-replace`, um uso posicional incorreto de
  `Select-String` e um scan que encontrou a palavra de evidência
  `ProjectReference` no próprio relatório. Nenhuma escreveu arquivos. As
  execuções corrigidas terminaram com sucesso.
- Nesta revalidação, três composições auxiliares somente leitura também foram
  descartadas por interpolação PowerShell, propagação de exit code e
  agrupamento incorreto de arrays. Nenhuma escreveu arquivos; os comandos
  finais e as rechecagens independentes terminaram com sucesso.
- Na correção `3.0.1`, tentativas auxiliares somente leitura com base incorreta
  do manifesto, regex de faixa/fonte, interpolação e nome de ADR incorreto
  também foram descartadas e corrigidas. Aplicações de patch que não
  encontraram o contexto falharam atomicamente, sem alteração parcial.
- Uma composição JavaScript da auditoria final falhou antes de invocar os
  checks por conflito de delimitador, e a primeira contagem auxiliar de backlog
  omitiu a classe Won't. Ambas foram rejeitadas; chamadas diretas e parser
  corrigido confirmaram os 31 itens sem escrever arquivos.
- Nenhuma versão externa de SDK, package, modelo ou OCI foi consultada; essas
  escolhas permanecem pendentes para o estado apropriado.
- Nenhuma fonte oficial, URL, licença ou termo externo foi consultado nesta
  revisão. A escolha e qualquer acesso real à rede continuam pendentes de
  autoridade no estado proprietário.
- O playbook paralelo foi validado por consistência documental, não por
  execução em branches/worktrees, pois o repositório Git ainda não existe.
- A baseline existente não foi traduzida. Além da nova política normativa,
  ainda não existem código ou artefatos técnicos de produto em `en-GB` para
  inspeção.
- O novo documento normativo em `en-GB` teve formato, referências e
  consistência inspecionados; os documentos existentes mantiveram o próprio
  idioma.
- Uma inspeção preliminar somente leitura dos dois arquivos de entrada falhou
  por composição inválida de pipeline PowerShell, e uma busca posterior falhou
  por expressão regular inválida. Uma composição auxiliar da revisão
  independente também parou no parser. As versões corrigidas terminaram com
  código de saída `0` sem escrever arquivos.
- Uma aplicação de patch em Templates não encontrou um trecho de contexto e
  falhou atomicamente. Patches menores foram aplicados depois sem alteração
  parcial da tentativa rejeitada.
- A revisão semântica independente encontrou um qualificador residual em
  AGENTS que limitava o protocolo a comunicações governadas. A frase foi
  reconciliada com a autorização para toda comunicação do projeto e
  revalidada.
- A mesma revisão identificou que Templates ainda não definia todos os rótulos
  em `pt-BR` exigidos por AGENTS. Um bloco canônico único foi acrescentado e o
  handoff de estado passou a reutilizá-lo.
- Nenhum comando Git foi executado nesta iteração; a ausência de `.git` foi
  verificada pelo sistema de arquivos.
- Git, build, testes de código, runtime e OCI são não aplicáveis neste lote
  documental.

## Human Gate

`PENDENTE`.

Resumo para decisão humana:

- relatório automático: `APROVADO` para a baseline `3.4.0`, com 21 documentos
  públicos, 96 links locais válidos e 0 problema de formato;
- amostra humana solicitada: revisar escopo do MVP, rastreabilidade, riscos,
  arquitetura proposta, backlog, dez grupos de decisão pendentes e limitações;
- amostra de runtime: não aplicável, pois não existe implementação;
- cobertura pendente:
  bootstrap/providers/corpus/fonte oficial/OCI/avaliação;
- ADR-0001 e ADR-0002: continuam `proposed`;
- continuidade e paralelismo entre conversas: playbooks vigentes, sem
  autoridade própria; escrita paralela permanece bloqueada sem Git/worktree;
- política de idioma: autoridade temática única; toda comunicação do projeto
  informa próximo passo, conversa recomendada e mensagem exata em `pt-BR`;
  novos artefatos técnicos usam `en-GB`, a baseline existente foi preservada e
  a interface ainda não foi decidida;
- prompt genérico de coordenação: arquivado localmente, com hash preservado e
  sem autoridade normativa;
- ressalvas propostas para esta decisão: nenhuma; decisões técnicas pendentes
  continuam nos gates/estados proprietários e não são aceitas por este gate;
- decisão solicitada: `APROVADO` para encerrar exclusivamente o Discovery
  documental.

A confirmação exigida é:

`Confirmo a decisão acima exclusivamente para STATE-00`

Ela encerrará somente `STATE-00`. Não aceitará ADR, não executará o
`GATE-B01` e não autorizará `STATE-01`, Git init, scaffold, dependência,
código, API, banco, interface, acesso à fonte oficial, consumo externo ou
deploy. A confirmação dada sobre a baseline anterior, se houver, não cobre a
mudança de escopo e a autorização para criar documentação também não constitui
essa confirmação.
