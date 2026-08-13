# Solicitação histórica de robustecimento da ingestão

## Classificação e limite de autoridade

Este arquivo é um registro histórico, inativo e não normativo da solicitação
do proprietário que originou o pacote local, offline e sintético de
robustecimento da ingestão. Ele não integra o corpus ativo de instruções, não
participa da precedência ou do roteamento e não concede autoridade presente ou
futura para executar ou repetir qualquer ação. A seção “Pacote atual” abaixo
descreve somente o contexto original já concluído.

O corpo original em `pt-BR`, cujo SHA-256 antes desta classificação era
`88c6cc12f51cdfd7cd665c9625a546726f551cf90316c67cf23257b384d93dac`, é
preservado como proveniência histórica. Sua implementação local está associada
aos commits `df3175db326dcef2cd73e6e4d1ea0c81a1863edf` e
`92c808c4f91dc943eac2ba674aa2b759626302de`, integrados em
`8107a8c550fad2a486b4162c501ca1f65221474c`.

Todo trabalho vigente continua regido por [`AGENTS.md`](../AGENTS.md),
[`Start-Here.md`](../prompts/Start-Here.md),
[`Current-State.md`](../prompts/state/Current-State.md) e pelas autoridades
temáticas ali roteadas. O idioma do texto histórico é preservado conforme
[`Language-Policy.md`](../prompts/governance/Language-Policy.md), e a versão do
corpus permanece definida por
[`Prompt-System-Change-Log.md`](../prompts/system/Prompt-System-Change-Log.md).
Este registro não redefine idioma, qualidade, handoff, lifecycle ou
autoridade. Em caso de divergência, prevalecem as autoridades vigentes e o
pedido atual e explícito do proprietário.

## Texto original preservado

Trabalhe neste projeto de forma autónoma, criteriosa e eficiente, procurando minimizar leituras repetidas, chamadas desnecessárias e consumo de contexto.

## 1. Contexto inicial

Antes de qualquer alteração:

1. Leia o `AGENTS.md` aplicável ao projeto e siga-o como instrução principal.
2. Identifique rapidamente:

   * arquitetura atual;
   * estrutura de diretórios;
   * tecnologias utilizadas;
   * fluxo principal da aplicação;
   * testes existentes;
   * documentação relevante;
   * configuração e dependências.
3. Consulte `docs/`, `prompts/`, README e outros documentos somente quando forem relevantes para a tarefa.
4. Não faça uma varredura completa repetidamente durante a mesma sessão.
5. Reutilize o contexto já obtido enquanto ele continuar válido.

## 2. Estratégia de execução

Trate solicitações relacionadas como um único pacote de trabalho.

Quando houver várias mudanças relacionadas:

1. Analise todas antes de começar.
2. Identifique arquivos e componentes compartilhados.
3. Planeje mentalmente a ordem correta das alterações.
4. Execute as mudanças em sequência sem solicitar confirmação entre etapas triviais.
5. Evite reler os mesmos arquivos sem necessidade.
6. Evite executar os mesmos comandos ou testes repetidamente quando uma execução posterior puder validar várias alterações ao mesmo tempo.
7. Agrupe validações, testes, linting e verificações sempre que possível.

Prefira:

`analisar uma vez → implementar o pacote → validar o conjunto → corrigir problemas → validar novamente`

Evite:

`analisar → alterar → testar → analisar novamente → alterar → testar → repetir`

## 3. Escopo

Faça apenas alterações necessárias para cumprir o objetivo solicitado.

Não:

* refatore partes não relacionadas;
* altere a arquitetura sem necessidade;
* substitua bibliotecas funcionais sem justificativa;
* adicione dependências desnecessárias;
* invente requisitos;
* remova funcionalidades existentes sem motivo;
* faça mudanças cosméticas em arquivos não relacionados;
* gere novos arquivos quando os existentes puderem ser utilizados adequadamente.

Preserve compatibilidade com o comportamento existente, salvo quando a própria tarefa exigir mudança.

## 4. Qualidade

Toda implementação deve considerar, quando aplicável:

* clareza;
* simplicidade;
* modularidade;
* manutenção;
* tratamento de erros;
* validação de entradas;
* segurança;
* desempenho;
* observabilidade;
* testabilidade;
* edge cases;
* comportamento determinístico.

Para componentes relacionados a RAG, considere especialmente:

* ingestão de documentos;
* normalização;
* chunking;
* metadata;
* embeddings;
* indexação;
* recuperação;
* ranking;
* reranking;
* construção de contexto;
* geração;
* grounding;
* citações;
* prevenção de hallucinations;
* avaliação de retrieval;
* avaliação das respostas;
* latência;
* custo;
* limites de contexto;
* tratamento de documentos duplicados;
* tratamento de consultas sem resposta adequada no corpus.

Não implemente complexidade adicional apenas porque ela seria tecnicamente interessante.

## 5. Código

Respeite completamente os padrões já existentes no projeto.

Todos os comentários adicionados ou modificados no código devem ser exclusivamente em inglês britânico (`en-GB`).

Comentários devem explicar apenas aspectos úteis, como:

* propósito;
* decisões não óbvias;
* pressupostos;
* limitações;
* casos especiais;
* efeitos relevantes.

Evite comentários redundantes que apenas descrevam literalmente o código.

## 6. Testes e validação

Depois de concluir todas as alterações relacionadas:

1. Execute os testes relevantes existentes.
2. Execute linting, type checking e outras validações configuradas no projeto, quando existirem.
3. Teste primeiro o escopo diretamente afetado.
4. Quando apropriado, execute a suíte completa depois.
5. Corrija regressões causadas pelas próprias alterações.
6. Não masque falhas pré-existentes.

Caso encontre uma falha anterior às alterações, identifique-a claramente como pré-existente.

## 7. Problemas encontrados durante o trabalho

Se descobrir um problema diretamente relacionado à tarefa e cuja correção seja pequena, segura e necessária para completar corretamente o objetivo, corrija-o no mesmo pacote.

Se descobrir algo relevante, mas fora do escopo:

1. não expanda automaticamente o trabalho;
2. registre-o no resumo final;
3. explique brevemente o impacto;
4. recomende uma próxima tarefa separada, se necessário.

## 8. Ambiguidades

Não interrompa o trabalho por decisões triviais que possam ser inferidas com segurança a partir:

* do código existente;
* dos testes;
* da documentação;
* das convenções do projeto.

Faça perguntas somente quando houver uma ambiguidade real que possa resultar em:

* comportamento funcional significativamente diferente;
* perda de dados;
* quebra de compatibilidade;
* alteração arquitetural importante;
* requisito impossível de inferir com segurança.

## 9. Uso eficiente de contexto

Priorize eficiência.

Não apresente longas explicações durante a execução.

Não repita informações já estabelecidas.

Não faça análises extensas de componentes que não serão alterados.

Não reabra ou releia arquivos apenas para confirmar algo que já esteja claro no contexto atual.

Não produza documentação adicional, relatórios extensos ou artefactos intermediários sem necessidade.

Use o contexto disponível para executar o máximo possível de trabalho relacionado em uma única passagem coerente.

## 10. Resultado final

Ao terminar, forneça um resumo conciso contendo:

### Alterações

O que foi efetivamente modificado.

### Arquivos

Arquivos principais criados, modificados ou removidos.

### Validação

Testes, linting, type checking e outras verificações executadas, com seus resultados.

### Decisões

Somente decisões técnicas relevantes que precisem ser conhecidas posteriormente.

### Pendências

Problemas conhecidos ou melhorias relevantes que ficaram propositalmente fora deste pacote.

Não forneça uma descrição extensa de cada pequena edição.

---

## Pacote atual

Objetivo:

Finalizar e robustecer o pipeline de ingestão do RAG-Challenge.

Requisitos:

1. Revisar o carregamento de documentos.
2. Corrigir problemas encontrados no parsing.
3. Revisar a estratégia de chunking.
4. Preservar metadata relevante.
5. Tratar documentos vazios e duplicados.
6. Melhorar tratamento de erros.
7. Criar ou atualizar testes relacionados.
8. Atualizar a documentação somente se o comportamento público tiver mudado.

Critérios de aceitação:

1. Pipeline funcionando do início ao fim.
2. Nenhuma regressão nos testes existentes.
3. Novos edge cases cobertos por testes.
4. Lint e type checking aprovados.
5. Sem dependências novas desnecessárias.

Execute todo o pacote de forma integrada seguindo as instruções acima.
