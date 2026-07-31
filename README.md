# Challenge

Assistente RAG independente para consultar documentação sobre bancos de dados
em linguagem natural, com respostas fundamentadas e referências às fontes.

> Status em 2026-07-30: `STATE-01 PROJECT_SETUP` ativo; `STATE-00 DISCOVERY`
> e `GATE-B01 ARCHITECTURE_BOOTSTRAP_DECISION` estão encerrados. O scaffold
> .NET, as fronteiras, os testes estruturais e a definição de CI existem sem
> lógica RAG. O Automatic Quality Gate de `STATE-01` está aprovado após
> reprodução em clone limpo; seu Human Gate permanece pendente. Não existe
> produto funcional ou deploy.

## Problema

Documentação técnica costuma estar distribuída entre arquivos extensos e
fontes diferentes. O Challenge pretende reduzir o tempo de busca oferecendo
uma interface de perguntas e respostas que recupera trechos relevantes antes
de solicitar uma resposta ao modelo de linguagem.

O produto nasce independente para cumprir o Challenge da Alura/ONE. A
arquitetura preserva compatibilidade conceitual e tecnológica com o
DB-Notifier para uma possível integração futura, sem criar dependência entre
os repositórios. O Challenge será proprietário do OpenAPI público; o futuro
adapter consumidor pertencerá ao DB-Notifier e aos gates desse repositório.

## MVP proposto

O primeiro produto funcional deverá:

- processar um PDF local e publicável sobre bancos de dados;
- sincronizar manualmente uma fonte oficial online allowlisted para snapshot
  versionado;
- permitir escolher, em cada pergunta, entre `Local` e `OfficialOnline`;
- usar o mesmo parser para o PDF local e um PDF oficial obtido por URL
  canônica HTTPS;
- preservar bytes imutáveis reabríveis para rebuild e rollback;
- dividir, vetorizar e indexar o conteúdo com estratégias versionadas;
- responder perguntas usando somente evidências recuperadas;
- apresentar documento e localização usados na resposta;
- declarar evidência insuficiente quando o acervo não sustentar a resposta;
- permitir substituir documentos e reconstruir o índice com segurança;
- executar no computador local;
- ser publicável em OCI com evidência verificável;
- possuir testes, configuração segura e documentação de execução;
- publicar um contrato OpenAPI v1 versionado pertencente ao Challenge.

O acervo de referência fornecido pelo curso não será usado automaticamente:
ele permanece em `reference-materials/`, fora do Git. Antes da implementação,
será escolhido ou criado um acervo de bancos de dados com direitos de uso e
redistribuição verificados.

## Fora do MVP

- múltiplos acervos ativos ao mesmo tempo;
- sincronização incremental agendada;
- múltiplas fontes oficiais, crawling genérico ou URL fornecida pelo usuário;
- navegação livre na internet durante uma pergunta;
- suporte simultâneo a todos os formatos documentais;
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
Challenge Dashboard
   |
   v
Challenge API
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
Challenge.Domain
        ^
        |
Challenge.Application
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

## Execução local, GitHub e OCI

O scaffold local, as toolchains fixadas, o restore offline .NET, o build e os
testes estruturais estão documentados em
[`PROJECT-SETUP.md`](docs/PROJECT-SETUP.md). Esse procedimento valida apenas
o setup e os endpoints de health; não representa uma aplicação RAG
funcional.

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

Os mesmos materiais divergem entre exigir um PDF ou CSV para o resultado
mínimo e sugerir oito formatos. O MVP adota um formato inicial; os demais
ficam no roadmap até uma decisão explícita.

## Organização atual

```text
.
├── .github/workflows/  # definição de CI, sem deploy
├── eng/                # checks reproduzíveis do setup
├── src/
│   ├── Challenge.Domain/
│   ├── Challenge.Application/
│   ├── Challenge.Infrastructure/
│   ├── Challenge.Server.Api/
│   └── Challenge.Dashboard.Web/
├── tests/
│   ├── Challenge.UnitTests/
│   ├── Challenge.Architecture.Tests/
│   └── Challenge.IntegrationTests/
├── AGENTS.md
├── Challenge.sln
├── LICENSE
├── README.md
├── docs/
├── prompts/
├── reference-materials/   # conteúdo local ignorado pelo Git
└── .gitignore
```

Esses projetos contêm somente marcadores de boundary, composição mínima,
health e verificações estruturais autorizadas para `STATE-01`.

## Governança

Comece por [`AGENTS.md`](AGENTS.md) e
[`prompts/Start-Here.md`](prompts/Start-Here.md). O estado factual está em
[`Current-State.md`](prompts/state/Current-State.md), e o relatório da
descoberta está em
[`STATE-00-Discovery-Report.md`](docs/STATE-00-Discovery-Report.md).
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
