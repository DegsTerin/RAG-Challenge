# RAG-Challenge — Sistema de Instruções

## Finalidade

Este é o ponto de entrada obrigatório para trabalhos governados no
RAG-Challenge.
O corpus separa visão, arquitetura, RAG, governança, segurança, qualidade,
estado factual, histórico, templates e evidências.

Agentes começam também por [`../AGENTS.md`](../AGENTS.md), fonte das regras
permanentes e reutilizáveis. Este documento encaminha para autoridades
temáticas; não transforma planos, templates ou relatórios históricos em
permissão de execução.

Toda comunicação com o proprietário e todo artefato do projeto aplicam a
autoridade temática definida em
[`governance/Language-Policy.md`](governance/Language-Policy.md).

## Ordem mínima de leitura

1. [`foundation/Prompt-New-Project.md`](foundation/Prompt-New-Project.md):
   visão, escopo, requisitos e limites.
2. [`state/Current-State.md`](state/Current-State.md): situação factual
   vigente.
3. [`governance/Governance.md`](governance/Governance.md): estados,
   autoridade e protocolo.
4. Abrir somente os demais documentos necessários à tarefa.

## Roteamento

| Necessidade | Documento |
|---|---|
| Regras permanentes de trabalho | [`../AGENTS.md`](../AGENTS.md) |
| Idioma da conversa, dos artefatos e da interface | [`governance/Language-Policy.md`](governance/Language-Policy.md) |
| Visão, problema, MVP e requisitos | [`foundation/Prompt-New-Project.md`](foundation/Prompt-New-Project.md) |
| Arquitetura, módulos, projetos e infraestrutura | [`foundation/Solution-Architecture-Document.md`](foundation/Solution-Architecture-Document.md) |
| Pipeline RAG, documentos, índices e providers | [`foundation/RAG-Module.md`](foundation/RAG-Module.md) |
| Autoridade, estados, bloqueio e rollback | [`governance/Governance.md`](governance/Governance.md) |
| Entregáveis e critérios por estado | [`governance/Lifecycle.md`](governance/Lifecycle.md) |
| Evidências, testes, CI e gates | [`governance/Quality-Gates.md`](governance/Quality-Gates.md) |
| Segurança, acesso, logging e auditoria | [`governance/Security-And-Access.md`](governance/Security-And-Access.md) |
| Estado factual atual | [`state/Current-State.md`](state/Current-State.md) |
| Histórico append-only | [`state/State-Transition-Log.md`](state/State-Transition-Log.md) |
| Semântica de handoff, continuidade, raciocínio e paralelismo | [`governance/Governance.md`](governance/Governance.md) |
| Formato, ordem, texto copiável e formulários | [`templates/Templates.md`](templates/Templates.md) |
| Versão do corpus de instruções | [`system/Prompt-System-Change-Log.md`](system/Prompt-System-Change-Log.md) |
| Índice público da documentação | [`../docs/README.md`](../docs/README.md) |
| Evidência do `STATE-00` | [`../docs/STATE-00-Discovery-Report.md`](../docs/STATE-00-Discovery-Report.md) |
| Roadmap e backlog | [`../docs/MVP-Roadmap-And-Backlog.md`](../docs/MVP-Roadmap-And-Backlog.md) |
| Decisões arquiteturais | [`../docs/architecture/README.md`](../docs/architecture/README.md) |

## Precedência

Em caso de conflito:

1. instruções da plataforma, sistema e desenvolvedor;
2. pedido atual e explícito do proprietário;
3. segurança, proteção de dados e limites de autoridade externa;
4. instruções aplicáveis ao diretório, da mais específica para a mais geral;
5. estado factual vigente;
6. decisões aceitas de arquitetura, governança, segurança e qualidade;
7. visão e lifecycle;
8. templates, roadmap e evidência histórica;
9. convenções inferidas.

Não resolver um conflito reduzindo segurança, inventando aprovação ou
ampliando escopo. Pedir direção quando a escolha alterar materialmente o
resultado ou exigir nova autoridade.

## Regras universais

- Não inventar implementação, suporte, teste, ambiente, credencial,
  licenciamento, disponibilidade de modelo ou aprovação.
- Distinguir capacidade planejada, implementada, testada, homologada,
  implantada e publicamente disponível.
- Não usar os materiais locais ignorados como dependência silenciosa do
  produto.
- Não declarar cobertura de todos os bancos de dados; o catálogo é aberto e
  evolui por versões verificáveis.
- Não enviar conteúdo ou perguntas a um provedor externo sem configuração e
  autoridade explícitas.
- Não executar deploy, publicação, instalação, consumo pago, alteração de
  secret ou acesso a fonte online sem autorização própria.
- Consultar o estado antes de executar uma fase.
- Atualizar estado e histórico somente quando houver mudança factual.
- Manter o Human Gate separado da auditoria automática.
- Aplicar o enforcement permanente de [`../AGENTS.md`](../AGENTS.md), a
  semântica de handoff, continuidade, raciocínio e paralelismo de
  [`governance/Governance.md`](governance/Governance.md), e o formato de
  [`templates/Templates.md`](templates/Templates.md), sem redefini-los aqui.
- Classificar runtime preflight conforme AGENTS e Governance antes de qualquer
  inspeção; documentação e análise somente leitura permanecem
  `NÃO APLICÁVEIS`.
- Aplicar integralmente a
  [`política de idioma`](governance/Language-Policy.md), sem reproduzi-la ou
  enfraquecê-la em outro documento.

## Estrutura ativa

O corpus contém 13 arquivos ativos em `prompts/`. Um novo arquivo normativo só
deve existir quando houver autoridade, ciclo de vida, owner ou público
realmente diferente. Caso contrário, atualizar o documento temático
proprietário e o changelog do corpus.

O DB-Notifier não é uma autoridade externa em tempo de execução para este
repositório. Seus padrões foram usados como referência no Discovery e foram
adaptados para um MVP independente.
