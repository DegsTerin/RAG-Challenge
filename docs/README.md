# Documentação do RAG-Challenge

## Status

Este índice descreve o corpus documental vigente `4.11.0`. `STATE-00`,
`GATE-B01` e `STATE-01` a `STATE-06` estão encerrados com seus gates
registrados. `STATE-07 TESTING_HOMOLOGATION` está ativo; incrementos locais,
offline, determinísticos e sintéticos foram executados e reconciliados, sem
constituir homologação de produto ou autorização geral de execução.

O PostgreSQL 18.4 `LocalAuthorised` foi ativado e uma imagem privada foi
implantada uma vez no Render Free, sem consulta de produto. Campanha pontuada,
`RB-4`, Human Gate de `STATE-07`, produção e OCI permanecem sob fronteiras
próprias. A reauditoria de governança bloqueia o consumo dos freezes RB-2/RB-3
até disposição humana. Relatórios são evidência histórica; o presente factual
pertence a
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
11. [`STATE-07-S07-A-Evaluation-And-Security-Proposal.md`](STATE-07-S07-A-Evaluation-And-Security-Proposal.md):
    baseline de planejamento confirmada para dataset, thresholds, matriz de
    idioma, ambiente, verificações e limites do primeiro lote, sem autoridade
    de execução.
12. [`Multi-Agent-Readiness-Audit.md`](Multi-Agent-Readiness-Audit.md):
    auditoria da Etapa 1, correções, ownership, isolamento, findings e gate da
    futura implementação do orchestrator.

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
- [ADR-0008 — Product Corpus Storage and Page-Image Evidence](architecture/ADR-0008-Product-Corpus-Storage-And-Page-Image-Evidence.md)
  (`accepted`; semantic reconciliation applied in corpus `4.9.5`;
  local rendering, activation, v2 serving and notice-bearing increments later
  implemented; product homologation remains separate)
- [ADR-0009 — Document, Evidence and Query Language Taxonomy](architecture/ADR-0009-Document-Evidence-And-Query-Language-Taxonomy.md)
  (`accepted`; semantic reconciliation applied in corpus `4.9.5`;
  internal split and v2 projection later implemented)
- [ADR-0010 — Persistent Answer-Evidence Records and Bounded Retention](architecture/ADR-0010-Persistent-Answer-Evidence-Records-And-Bounded-Retention.md)
  (`accepted`; local implementation and reconciliation completed under
  separate authority)
- [ADR-0011 — Source Rights Evidence Mapping and Same-Origin Derivative Display Boundary](architecture/ADR-0011-Source-Rights-Evidence-Mapping-And-Same-Origin-Derivative-Display-Boundary.md)
  (`accepted`; conditional rights mapping and same-origin boundary)
- [ADR-0012 — Notice-Bearing Page-Image Profile and Derivative Obligation Delivery](architecture/ADR-0012-Notice-Bearing-Page-Image-Profile-And-Derivative-Obligation-Delivery.md)
  (`accepted`; contract, schema/migrations, local behaviour, its synthetic AQG
  and the A0-003 rights disposition completed; product/browser homologation
  remains separate)
- [ADR-0013 — MVP Language-Model Candidate and Deferred Frontier Evaluation](architecture/ADR-0013-MVP-Language-Model-Candidate-And-Deferred-Frontier-Evaluation.md)
  (`accepted`; semantic reconciliation, fake-handler adapter compatibility and
  its focused AQG completed; provider execution remains separately governed)
- [ADR-0014 — Deterministic Retrieval Ranking and Retrieval-Only Baseline](architecture/ADR-0014-Deterministic-Retrieval-Ranking-And-Retrieval-Only-Baseline.md)
  (`accepted`; RB-2/RB-3 are mechanically intact but unavailable pending human
  disposition of the adjudication-authority conflict)
- [ADR-0015 — Versioned Cosine Numerical Semantics](architecture/ADR-0015-Versioned-Cosine-Numerical-Semantics.md)
  (`accepted`; implementation and corrective retest completed separately)
- [ADR-0016 — Deterministic Development Orchestrator and Codex Runner Boundary](architecture/ADR-0016-Deterministic-Development-Orchestrator-And-Codex-Runner-Boundary.md)
  (`proposed`; no Stage 2 implementation authority)
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
