# RAG-Challenge documentation

## Status

This index describes the current `4.15.0` documentation corpus. `STATE-00`,
`GATE-B01` and `STATE-01` through `STATE-06` are closed with their gates
recorded. `STATE-07 TESTING_HOMOLOGATION` is active; local, offline,
deterministic and synthetic increments have been executed and reconciled
without constituting product homologation or general execution authority.

PostgreSQL 18.4 `LocalAuthorised` was activated, and a private image was
deployed once on Render Free without a product query. The scored campaign,
`RB-4`, the `STATE-07` Human Gate, production and OCI remain within their own
boundaries. Under the selected historical quarantine, RB-2 does not satisfy
its gate and RB-3 cannot be consumed by RB-4; any successor requires separate
human authority and review. Reports are historical evidence; present facts
belong in [`Current-State.md`](../prompts/state/Current-State.md).

Stage 2 implemented and validated the deterministic development orchestrator
accepted by ADR-0016. ADR-0017 was explicitly `accepted` and replaced only the
SDK transport with the stable Codex App Server in `@openai/codex` `0.147.0`.
The CLI now offers start and resume through `--runner codex`, persists
`thread.id` before `turn/start`, requires `--authority-reference`, uses the
validated local ChatGPT session and does not inherit the product provider
credential. Contract tests, the canonical gate and one real read-only
validation passed; tooling readiness is `MULTI_AGENT_READY`. Every future run
remains subject to its own plan and authority without changing the product,
Human Gate or lifecycle.

## Start here

1. [`../README.md`](../README.md): public overview and current boundaries.
2. [`../AGENTS.md`](../AGENTS.md): permanent rules.
3. [`../prompts/Start-Here.md`](../prompts/Start-Here.md): routing and
   precedence.
4. [`../prompts/governance/Language-Policy.md`](../prompts/governance/Language-Policy.md):
   language for communication and artefacts.
5. [`STATE-00-Discovery-Report.md`](STATE-00-Discovery-Report.md): Discovery
   facts, findings and gate.
6. [`MVP-Roadmap-And-Backlog.md`](MVP-Roadmap-And-Backlog.md): incremental
   evolution.
7. [`PROJECT-SETUP.md`](PROJECT-SETUP.md): onboarding and checks for the
   authorised scaffold.
8. [`STATE-01-Project-Setup-Report.md`](STATE-01-Project-Setup-Report.md):
   factual evidence and the setup Automatic Quality Gate.
9. [`STATE-02-Architecture-Report.md`](STATE-02-Architecture-Report.md):
   factual execution, proposals and Architecture-state blockers.
10. [`STATE-03-Data-And-Index-Modeling-Report.md`](STATE-03-Data-And-Index-Modeling-Report.md):
    partial factual execution of `S03-A` and the explicit `S03-B` blocker.
11. [`STATE-07-S07-A-Evaluation-And-Security-Proposal.md`](STATE-07-S07-A-Evaluation-And-Security-Proposal.md):
    confirmed planning baseline for the dataset, thresholds, language matrix,
    environment, checks and first-batch boundaries, without execution authority.
12. [`Multi-Agent-Readiness-Audit.md`](Multi-Agent-Readiness-Audit.md): Stage 1
    audit, corrections, ownership, isolation, findings and gate for the future
    orchestrator implementation.
13. [`Stage-2-Multi-Agent-Orchestrator-Report.md`](Stage-2-Multi-Agent-Orchestrator-Report.md):
    implementation, validations, dry run, recovery, security, limitations and
    operational readiness of the development orchestrator.

## Product and architecture

- [Vision, scope and requirements](../prompts/foundation/Prompt-New-Project.md)
- [Solution architecture](../prompts/foundation/Solution-Architecture-Document.md)
- [RAG module](../prompts/foundation/RAG-Module.md)
- [Architecture index](architecture/README.md)
- [ADR-0001 — Runtime Stack and Modular Monolith](architecture/ADR-0001-Runtime-Stack-And-Modular-Monolith.md)
- [ADR-0002 — RAG Lifecycle, Provider Boundaries and Source Separation](architecture/ADR-0002-RAG-Lifecycle-Providers-And-Source-Separation.md)
- [ADR-0003 — Product and Technical Naming](architecture/ADR-0003-Product-And-Technical-Naming.md)
- [ADR-0004 — MVP Catalogue, Governed Documents, Official Sources and Evaluation](architecture/ADR-0004-MVP-Corpus-Official-Source-And-Evaluation.md)
- [ADR-0005 — MVP Providers, Persistence and OCI Deployment](architecture/ADR-0005-MVP-Providers-Persistence-And-OCI-Deployment.md)
- [ADR-0006 — Security, Egress, Administration and HTTP Contracts](architecture/ADR-0006-Security-Egress-Administration-And-HTTP-Contracts.md)
- [ADR-0007 — Generation Identity and Freshness Observation Rebinding](architecture/ADR-0007-Generation-Identity-And-Freshness-Observation-Rebinding.md)
  (`accepted`; semantic reconciliation applied; renewed audit approved)
- [ADR-0008 — Product Corpus Storage and Page-Image Evidence](architecture/ADR-0008-Product-Corpus-Storage-And-Page-Image-Evidence.md)
  (`accepted`; semantic reconciliation applied in corpus `4.9.5`; local
  rendering, activation, v2 serving and notice-bearing increments later
  implemented; product homologation remains separate)
- [ADR-0009 — Document, Evidence and Query Language Taxonomy](architecture/ADR-0009-Document-Evidence-And-Query-Language-Taxonomy.md)
  (`accepted`; semantic reconciliation applied in corpus `4.9.5`; internal
  split and v2 projection later implemented)
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
  (`accepted`; RB-2/RB-3 are mechanically intact but quarantined as historical
  evidence and are unavailable to RB-4)
- [ADR-0015 — Versioned Cosine Numerical Semantics](architecture/ADR-0015-Versioned-Cosine-Numerical-Semantics.md)
  (`accepted`; implementation and corrective retest completed separately)
- [ADR-0016 — Deterministic Development Orchestrator and Codex Runner Boundary](architecture/ADR-0016-Deterministic-Development-Orchestrator-And-Codex-Runner-Boundary.md)
  (`accepted`; Stage 2 implementation and exact package validation completed;
  ADR-0017 replaces only the former SDK transport limitation)
- [ADR-0017 — Codex App Server Pre-Turn Checkpoint Runner](architecture/ADR-0017-Codex-App-Server-Pre-Turn-Checkpoint-Runner.md)
  (`accepted` and implemented; one controlled real validation passed)
- [Canonical `STATE-02` contracts](architecture/STATE-02-Canonical-Contracts.md)
- [`S03-A` logical data dictionary and index](data/STATE-03-S03-A-Data-Dictionary.md)
- [`STATE-02` threat model](security/STATE-02-Threat-Model.md)

## Governance

- [Governance and states](../prompts/governance/Governance.md)
- [Lifecycle](../prompts/governance/Lifecycle.md)
- [Quality and gates](../prompts/governance/Quality-Gates.md)
- [Security and access](../prompts/governance/Security-And-Access.md)
- [Language policy](../prompts/governance/Language-Policy.md)
- [Factual state](../prompts/state/Current-State.md)
- [Append-only history](../prompts/state/State-Transition-Log.md)
- [Templates](../prompts/templates/Templates.md)
- [Corpus changelog](../prompts/system/Prompt-System-Change-Log.md)

## Document authority

| Type | Authority |
|---|---|
| `AGENTS.md` | Permanent and cross-cutting rules. |
| `prompts/foundation/` | Vision and high-level contracts. |
| `prompts/governance/` | States, security, quality and execution. |
| `prompts/governance/Language-Policy.md` | Single thematic authority for the language of communication and artefacts. |
| `prompts/state/Current-State.md` | Factual present. |
| `prompts/state/State-Transition-Log.md` | Append-only history. |
| `docs/architecture/ADR-*` | A decision according to its own status. |
| `docs/STATE-*` | Evidence from a specific execution. |
| Roadmap and templates | Planning; they do not grant execution. |

## Local materials

`reference-materials/` contains the original received files and remains
ignored by Git. Public documentation does not depend on these files to resolve
links or run the future product.
