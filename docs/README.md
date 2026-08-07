# Documentação do RAG-Challenge

## Status

Este índice descreve o corpus documental vigente `4.9.3`. `STATE-00`,
`GATE-B01` e `STATE-01` a `STATE-06` estão encerrados com seus gates
registrados. `STATE-07 TESTING_HOMOLOGATION` está ativo exclusivamente por
entrada documental sobre
`main@3240a4b13acd82a1cf5815ac64f6997b2a7f89bf`, sem lote autorizado ou
executado.

Dataset, avaliação RAG, testes, carga, segurança dinâmica, browser,
providers, fontes reais, rede, OCI, GitHub, publicação e deploy permanecem
não autorizados e não executados. Relatórios são evidência histórica; o
presente factual pertence a
[`Current-State.md`](../prompts/state/Current-State.md).

## Comece aqui

1. [`../README.md`](../README.md): apresentação pública e limites atuais.
2. [`../AGENTS.md`](../AGENTS.md): regras permanentes.
3. [`../prompts/Start-Here.md`](../prompts/Start-Here.md): roteamento e
   precedência.
4. [`../prompts/governance/Language-Policy.md`](../prompts/governance/Language-Policy.md):
   idioma da comunicação e dos artefatos.
5. [`STATE-00-Discovery-Report.md`](STATE-00-Discovery-Report.md): fatos,
   achados e gate da descoberta.
6. [`MVP-Roadmap-And-Backlog.md`](MVP-Roadmap-And-Backlog.md): evolução
   incremental.
7. [`PROJECT-SETUP.md`](PROJECT-SETUP.md): onboarding e checks do scaffold
   autorizado.
8. [`STATE-01-Project-Setup-Report.md`](STATE-01-Project-Setup-Report.md):
   evidência factual e Automatic Quality Gate do setup.
9. [`STATE-02-Architecture-Report.md`](STATE-02-Architecture-Report.md):
   execução factual, propostas e bloqueios do estado de arquitetura.
10. [`STATE-03-Data-And-Index-Modeling-Report.md`](STATE-03-Data-And-Index-Modeling-Report.md):
    execução factual parcial de `S03-A` e bloqueio explícito de `S03-B`.

## Produto e arquitetura

- [Visão, escopo e requisitos](../prompts/foundation/Prompt-New-Project.md)
- [Arquitetura da solução](../prompts/foundation/Solution-Architecture-Document.md)
- [Módulo RAG](../prompts/foundation/RAG-Module.md)
- [Índice de arquitetura](architecture/README.md)
- [ADR-0001 — Runtime Stack and Modular Monolith](architecture/ADR-0001-Runtime-Stack-And-Modular-Monolith.md)
- [ADR-0002 — RAG Lifecycle, Provider Boundaries and Source Separation](architecture/ADR-0002-RAG-Lifecycle-Providers-And-Source-Separation.md)
- [ADR-0003 — Product and Technical Naming](architecture/ADR-0003-Product-And-Technical-Naming.md)
- [ADR-0004 — MVP Catalogue, Governed Documents, Official Sources and Evaluation](architecture/ADR-0004-MVP-Corpus-Official-Source-And-Evaluation.md)
- [ADR-0005 — MVP Providers, Persistence and OCI Deployment](architecture/ADR-0005-MVP-Providers-Persistence-And-OCI-Deployment.md)
- [ADR-0006 — Security, Egress, Administration and HTTP Contracts](architecture/ADR-0006-Security-Egress-Administration-And-HTTP-Contracts.md)
- [ADR-0007 — Generation Identity and Freshness Observation Rebinding](architecture/ADR-0007-Generation-Identity-And-Freshness-Observation-Rebinding.md)
  (`accepted`; semantic reconciliation applied; renewed audit approved)
- [Contratos canônicos de STATE-02](architecture/STATE-02-Canonical-Contracts.md)
- [Dicionário lógico de dados e índice de S03-A](data/STATE-03-S03-A-Data-Dictionary.md)
- [Threat model de STATE-02](security/STATE-02-Threat-Model.md)

## Governança

- [Governança e estados](../prompts/governance/Governance.md)
- [Lifecycle](../prompts/governance/Lifecycle.md)
- [Qualidade e gates](../prompts/governance/Quality-Gates.md)
- [Segurança e acesso](../prompts/governance/Security-And-Access.md)
- [Política de idioma](../prompts/governance/Language-Policy.md)
- [Estado factual](../prompts/state/Current-State.md)
- [Histórico append-only](../prompts/state/State-Transition-Log.md)
- [Templates](../prompts/templates/Templates.md)
- [Changelog do corpus](../prompts/system/Prompt-System-Change-Log.md)

## Autoridade dos documentos

| Tipo | Autoridade |
|---|---|
| `AGENTS.md` | Regras permanentes e transversais. |
| `prompts/foundation/` | Visão e contratos de alto nível. |
| `prompts/governance/` | Estados, segurança, qualidade e execução. |
| `prompts/governance/Language-Policy.md` | Autoridade temática única para idioma da comunicação e dos artefatos. |
| `prompts/state/Current-State.md` | Presente factual. |
| `prompts/state/State-Transition-Log.md` | História append-only. |
| `docs/architecture/ADR-*` | Decisão conforme status próprio. |
| `docs/STATE-*` | Evidência de uma execução específica. |
| Roadmap e templates | Planejamento; não concedem execução. |

## Materiais locais

`reference-materials/` contém os arquivos originais recebidos e permanece
ignorado pelo Git. A documentação pública não depende desses arquivos para
resolver links ou executar o futuro produto.
