# RAG-Challenge — Instruction System

## Purpose

This is the mandatory entry point for governed work in RAG-Challenge.
The corpus separates vision, architecture, RAG, governance, security, quality,
factual state, history, templates and evidence.

Agents also start with [`../AGENTS.md`](../AGENTS.md), the source of permanent
and reusable rules. This document routes to thematic authorities; it does not
turn plans, templates or historical reports into execution authority.

All owner communication and every project artefact apply the thematic
authority defined in
[`governance/Language-Policy.md`](governance/Language-Policy.md).

## Minimum reading order

1. [`foundation/Prompt-New-Project.md`](foundation/Prompt-New-Project.md):
   vision, scope, requirements and boundaries.
2. [`state/Current-State.md`](state/Current-State.md): current factual state.
3. [`governance/Governance.md`](governance/Governance.md): states, authority
   and protocol.
4. Open only the other documents required for the task.

## Routing

| Need | Document |
|---|---|
| Permanent working rules | [`../AGENTS.md`](../AGENTS.md) |
| Conversation, artefact and interface language | [`governance/Language-Policy.md`](governance/Language-Policy.md) |
| Vision, problem, MVP and requirements | [`foundation/Prompt-New-Project.md`](foundation/Prompt-New-Project.md) |
| Architecture, modules, projects and infrastructure | [`foundation/Solution-Architecture-Document.md`](foundation/Solution-Architecture-Document.md) |
| RAG pipeline, documents, indexes and providers | [`foundation/RAG-Module.md`](foundation/RAG-Module.md) |
| Authority, states, blocking and rollback | [`governance/Governance.md`](governance/Governance.md) |
| Deliverables and criteria by state | [`governance/Lifecycle.md`](governance/Lifecycle.md) |
| Evidence, tests, CI and gates | [`governance/Quality-Gates.md`](governance/Quality-Gates.md) |
| Security, access, logging and audit | [`governance/Security-And-Access.md`](governance/Security-And-Access.md) |
| Current factual state | [`state/Current-State.md`](state/Current-State.md) |
| Append-only history | [`state/State-Transition-Log.md`](state/State-Transition-Log.md) |
| Hand-off, continuity, reasoning and parallelism semantics | [`governance/Governance.md`](governance/Governance.md) |
| Format, order, copy-ready text and forms | [`templates/Templates.md`](templates/Templates.md) |
| Instruction corpus version | [`system/Prompt-System-Change-Log.md`](system/Prompt-System-Change-Log.md) |
| Public documentation index | [`../docs/README.md`](../docs/README.md) |
| `STATE-00` evidence | [`../docs/STATE-00-Discovery-Report.md`](../docs/STATE-00-Discovery-Report.md) |
| Roadmap and backlog | [`../docs/MVP-Roadmap-And-Backlog.md`](../docs/MVP-Roadmap-And-Backlog.md) |
| Architectural decisions | [`../docs/architecture/README.md`](../docs/architecture/README.md) |

## Precedence

If authorities conflict:

1. platform, system and developer instructions;
2. the owner's current explicit request;
3. security, data-protection and external-authority boundaries;
4. applicable directory instructions, from most specific to most general;
5. current factual state;
6. accepted architecture, governance, security and quality decisions;
7. vision and lifecycle;
8. templates, roadmap and historical evidence;
9. inferred conventions.

Do not resolve a conflict by reducing security, inventing approval or
broadening scope. Request direction when the choice would materially change
the result or require new authority.

## Universal rules

- Do not invent implementation, support, testing, environment, credentials,
  licensing, model availability or approval.
- Distinguish planned, implemented, tested, homologated, deployed and publicly
  available capabilities.
- Do not use ignored local materials as a silent product dependency.
- Do not claim coverage of every database; the catalogue is open and evolves
  through verifiable versions.
- Do not send content or questions to an external provider without explicit
  configuration and authority.
- Do not deploy, publish, install, consume paid services, change a secret or
  access an online source without its own authority.
- Inspect the state before executing a phase.
- Update state and history only when a factual change occurs.
- Keep the Human Gate separate from automatic audit.
- Apply the permanent enforcement in [`../AGENTS.md`](../AGENTS.md), the
  hand-off, continuity, reasoning and parallelism semantics in
  [`governance/Governance.md`](governance/Governance.md), and the format in
  [`templates/Templates.md`](templates/Templates.md), without redefining them
  here.
- Classify runtime preflight under AGENTS and Governance before any inspection;
  documentation and read-only analysis remain `NOT_APPLICABLE`.
- Apply the [language policy](governance/Language-Policy.md) in full, without
  reproducing or weakening it in another document.

## Active structure

The corpus contains 13 active files under `prompts/`. A new normative file may
exist only when it has genuinely different authority, lifecycle, owner or
audience. Otherwise, update the owning thematic document and the corpus change
log.

DB-Notifier is not an external runtime authority for this repository. Its
patterns were used as a Discovery reference and adapted for an independent
MVP.
