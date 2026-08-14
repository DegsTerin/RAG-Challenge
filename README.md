# RAG-Challenge

Assistente RAG independente para consultar documentação sobre bancos de dados
em linguagem natural, com respostas fundamentadas e referências às fontes.

> Status em 2026-08-14: `STATE-07 TESTING_HOMOLOGATION` está ativo. O produto
> local possui PostgreSQL 18.4 `LocalAuthorised` materializado e a geração
> text-first
> `idxgen-ec39244b021c90fceea1b3a628fe793a99f74650cad451f16ffbcd414af636f6`
> ativada na revisão `1`, com 3.282 chunks, 3.282 vetores e
> `renderManifestId=null`. O pacote privado candidato para Render Hobby com
> instância Free foi preparado e validado localmente, inclusive em contêiner
> Linux x64 e após restart. Nenhuma imagem foi publicada e nenhum serviço
> Render, OCI ou GitHub foi criado ou alterado. Uma consulta real de produto
> por `/v1/responses`, o deploy público e a homologação de produção ainda não
> foram executados.

## Problema

Documentação técnica costuma estar distribuída entre arquivos extensos e
fontes diferentes. O RAG-Challenge pretende reduzir o tempo de busca oferecendo
uma interface de perguntas e respostas que recupera trechos relevantes antes
de solicitar uma resposta ao modelo de linguagem.

O produto nasce independente para cumprir o Challenge da Alura/ONE. A
arquitetura preserva compatibilidade conceitual e tecnológica com o
DB-Notifier para uma possível integração futura, sem criar dependência entre
os repositórios. O RAG-Challenge será proprietário do OpenAPI público; o
futuro adapter consumidor pertencerá ao DB-Notifier e aos gates desse
repositório.

## Escopo do MVP

O primeiro produto funcional deverá:

- manter o catálogo canônico inicial de 51 bancos de dados, com categorias
  muitos-para-muitos e sem lista hard-coded no produto;
- permitir ao administrador adicionar, versionar, ativar, desativar e remover
  logicamente bancos e qualquer quantidade de documentos PDF/CSV associados;
- exigir ao menos um documento ativo e validado para cada banco ativo;
- sincronizar manualmente fontes oficiais allowlisted para snapshots
  versionados e ingerir documentos locais autorizados com a mesma governança;
- pesquisar, por padrão, todos os documentos ativos em um único espaço de
  recuperação, preservando origem `LocalAuthorised` ou `OfficialExternal` nas
  citações;
- usar adapters próprios para PDF e CSV sem acoplar o núcleo aos parsers;
- preservar bytes imutáveis reabríveis para rebuild e rollback;
- dividir, vetorizar e indexar o conteúdo com estratégias versionadas;
- responder perguntas usando somente evidências recuperadas;
- aceitar perguntas em `pt-BR` e `en-GB` e responder no idioma declarado da
  pergunta;
- permitir alternar a interface entre `pt-BR` e `en-GB`, independentemente do
  idioma da pergunta;
- permitir alternar o tema da interface entre `Light` e `Dark`,
  independentemente do idioma visual e da consulta;
- apresentar documento e localização usados na resposta;
- preservar nas citações o idioma original do conteúdo da fonte;
- declarar evidência insuficiente quando o acervo não sustentar a resposta;
- permitir versionar documentos, construir índices candidatos e ativá-los com
  segurança, sem tornar staging parcial consultável;
- executar no computador local;
- ser publicável em OCI com evidência verificável;
- possuir testes, configuração segura e documentação de execução;
- publicar um contrato OpenAPI v1 versionado pertencente ao RAG-Challenge.

O Dashboard implementa o idioma visual, o tema e o idioma da consulta como
seleções independentes. As oito combinações de `pt-BR`/`en-GB` e
`Light`/`Dark` possuem evidência local sintética do estado responsável pelo
frontend; isso não constitui homologação do produto com corpus ou providers
reais.

O acervo de referência fornecido pelo curso não é usado automaticamente: ele
permanece em `reference-materials/`, fora do Git. Antes de qualquer ativação de
produto, o proprietário deverá fornecer ou autorizar um acervo com direitos de
uso, proveniência e idioma verificados.

## Fora do MVP

- múltiplos acervos ativos ao mesmo tempo;
- sincronização incremental agendada;
- crawling genérico ou URL fornecida pelo usuário;
- navegação livre na internet durante uma pergunta;
- formatos documentais além de PDF e CSV;
- loader dinâmico de plug-ins;
- múltiplos provedores ativos de embeddings, vetores ou modelos;
- autenticação corporativa, RBAC completo e multi-tenancy;
- integração executável com o DB-Notifier.

Essas capacidades são previstas por contratos e fronteiras, mas não serão
implementadas antecipadamente.

## Arquitetura de bootstrap aceita

```text
Browser
   |
   v
RAG-Challenge Dashboard
   |
   v
RAG-Challenge API
   |
   v
Application use cases
   |
   +--> local document source --> immutable content store --> parser --> chunker
   |
   +--> allowlisted official source --> governed snapshot/content --> parser --> chunker
   |
   +--> embedding provider --> vector store
   |
   +--> retriever --> language model --> answer with citations
```

A direção de dependências será voltada ao núcleo:

```text
RagChallenge.Domain
        ^
        |
RagChallenge.Application
(inclui RAG abstractions)
        ^
        |
Infrastructure / Persistence / API

Dashboard -- versioned HTTP --> API
```

O desenho detalhado está em
[`Solution-Architecture-Document.md`](prompts/foundation/Solution-Architecture-Document.md)
e as regras específicas de RAG em
[`RAG-Module.md`](prompts/foundation/RAG-Module.md).

## Execução local, GitHub e hospedagem

As toolchains fixadas, os restores governados e os checks completos estão
documentados em [`PROJECT-SETUP.md`](docs/PROJECT-SETUP.md). Um cache ausente
não autoriza fallback para a rede.

Com as dependências já restauradas, o exemplo integrado local é executado da
raiz do repositório:

```powershell
./src/RagChallenge.Server.Api/Build-IntegrationArtifact.ps1
./src/RagChallenge.Server.Api/Test-IntegrationArtifact.ps1
```

O resultado sanitizado verificado em `STATE-06` contém:

```json
{
  "Status": "Passed",
  "DashboardServed": true,
  "AnswerLanguages": ["en-GB", "pt-BR"],
  "RestartPreservedGeneration": true,
  "ControlStore": "control.db",
  "VectorStore": "vectors.db"
}
```

Esse exemplo usa somente uma fixture CSV sintética, providers determinísticos,
stores SQLite temporários e um listener loopback no Windows. Ele demonstra o
fluxo local integrado e a reabertura da mesma geração após restart; não alega
corpus, provider ou fonte oficial reais, execução Linux, OCI, suporte de
produção ou deploy.

O rehearsal separado de empacotamento Linux ARM64 pode ser construído e
verificado estaticamente, também sem restore:

```powershell
./src/RagChallenge.Server.Api/Build-OciRehearsalArtifact.ps1
./src/RagChallenge.Server.Api/Test-OciRehearsalArtifact.ps1
```

O verificador confere manifesto, hashes, configuração fail-closed e identidade
ELF AArch64. O binário ARM64 não é executado no Windows e nenhuma operação OCI
é realizada. O plano e as limitações estão em
[`STATE-06-OCI-Readiness-And-Rehearsal.md`](docs/STATE-06-OCI-Readiness-And-Rehearsal.md).

O candidato alternativo para Render Hobby/Free prepara uma imagem privada com
o snapshot PostgreSQL ativado e restaura um store efêmero verificado em cada
boot. Ele não publica o PDF, o store ou a imagem e não cria serviço externo:

```powershell
./eng/Build-RenderFreePackage.ps1
./eng/Test-RenderFreePackage.ps1
```

O procedimento, as limitações de persistência e a barreira de custo estão em
[`deploy/render-free/README.md`](deploy/render-free/README.md). Esse candidato
não substitui silenciosamente o requisito OCI registrado pelos materiais do
Challenge; a seleção final de Render exige reconciliação própria antes do
deploy.

O código poderá ser hospedado em um repositório público no GitHub. GitHub
Pages, sozinho, hospeda apenas conteúdo estático e não executa o backend RAG
nem protege credenciais de modelos. A entrega online deverá executar o
backend em um serviço OCI autorizado. Uma interface estática em GitHub Pages
poderá ser avaliada depois, desde que consuma uma API segura publicada
separadamente; ela não substitui o requisito de OCI.

## Requisitos de entrega conhecidos

Os materiais locais do Challenge estabelecem como resultado mínimo:

- repositório público no GitHub, organizado e com histórico de commits;
- agente funcional baseado em pelo menos um documento;
- README com visão, arquitetura, tecnologias, execução e exemplos;
- uso de pelo menos um serviço OCI;
- link público ou captura de tela que comprove a execução online.

Os mesmos materiais permitem PDF ou CSV e sugerem formatos adicionais. O MVP
adota PDF e CSV como formatos iniciais; os demais ficam no roadmap até uma
decisão explícita e adapter compatível.

## Organização atual

```text
.
├── .github/workflows/  # definição de CI, sem deploy
├── deploy/render-free/ # pacote privado local, sem publicação
├── eng/                # checks reproduzíveis do setup
├── src/
│   ├── RagChallenge.Domain/
│   ├── RagChallenge.Application/
│   ├── RagChallenge.Infrastructure/
│   ├── RagChallenge.Server.Api/
│   └── RagChallenge.Dashboard.Web/
├── tests/
│   ├── RagChallenge.UnitTests/
│   ├── RagChallenge.Architecture.Tests/
│   └── RagChallenge.IntegrationTests/
├── AGENTS.md
├── RAG-Challenge.sln
├── LICENSE
├── README.md
├── docs/
├── prompts/
├── reference-materials/   # conteúdo local ignorado pelo Git
└── .gitignore
```

Domain e Application contêm os modelos e casos de uso; Infrastructure contém
migrations SQLite, stores persistentes, parsers PDF/CSV, adapters de provider e
transporte governado. A API preserva health e consulta v1 e também implementa o
fluxo v2 local, incluindo referências de página, serving PNG same-origin
fail-closed e o perfil notice-bearing; o Dashboard consome os dois contratos e
apresenta as obrigações acessíveis junto da imagem. A evidência executável
permanece local e sintética; o AQG notice-bearing, corpus/provider real e
homologação de produto continuam separados. Administração permanece one-shot
fora de HTTP, e nenhum provider ou acervo real está configurado.

## Governança

Comece por [`AGENTS.md`](AGENTS.md) e
[`prompts/Start-Here.md`](prompts/Start-Here.md). O estado factual está em
[`Current-State.md`](prompts/state/Current-State.md), e o relatório da
integração está em
[`STATE-06-Integration-Report.md`](docs/STATE-06-Integration-Report.md).
A comunicação com o proprietário e os novos artefatos seguem a
[`política de idioma`](prompts/governance/Language-Policy.md).

## Segurança

- Segredos não serão versionados nem exibidos em logs.
- Documentos, perguntas, trechos recuperados e respostas do modelo são dados
  não confiáveis.
- A resposta deverá citar evidências e falhar de forma explícita quando elas
  forem insuficientes.
- A fonte oficial do MVP será habilitada somente após aprovação do domínio,
  URL canônica exata, termos/licença, allowlist, limites e proteção contra
  SSRF e prompt injection.
- Cada conexão oficial deverá usar somente um IP previamente resolvido e
  autorizado, preservando Host/SNI; redirects permanecerão desativados no MVP.
- Perguntas públicas selecionam um snapshot; não fornecem URL nem disparam
  crawling.

## Licença

O `GATE-B01` selecionou a licença MIT para o conteúdo autoral do repositório,
com o aviso
`Copyright (c) 2026 Bruno Araújo - DegsTerin.`. O arquivo
[`LICENSE`](LICENSE) foi materializado após a autorização de entrada em
`STATE-01`.

A licença não abrange o corpus, snapshots oficiais, materiais de terceiros ou
`reference-materials/`. A licença e a proveniência do corpus são decisões
separadas de `STATE-02`.
