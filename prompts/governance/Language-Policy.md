# Language Policy

## Purpose and authority

This document is the single thematic authority for language conventions in
the RAG-Challenge repository. It applies to the entire workspace.

[`../../AGENTS.md`](../../AGENTS.md) remains the permanent repository
instruction authority, and [`../Start-Here.md`](../Start-Here.md) defines
precedence and routing. Other documents reference this policy instead of
restating it.

## Owner communication

- Always communicate with the owner in Brazilian Portuguese (`pt-BR`).
- Questions, explanations, progress updates, approvals, warnings,
  recommendations, hand-offs and ready-to-copy messages use `pt-BR`.
- Explain the practical result first and use plain language suitable for an
  owner without specialist software, architecture, legal or infrastructure
  knowledge.
- Do not assume that the owner knows technical terminology. When a technical
  literal is necessary, explain its meaning and consequence briefly in
  `pt-BR`, using a concrete example when that improves understanding.
- Keep the initial explanation concise and add technical evidence only when it
  is needed for the decision or requested by the owner. Simplification must
  never hide uncertainty, risk, an authority boundary or an unverified fact.
- Present headings, labels, guidance and explanatory values intended for the
  owner in `pt-BR`. Do not add bilingual labels merely to mirror an internal
  canonical name.
- Keep a literal filename, path, command, code fragment, API name, protocol
  field or canonical enum in its required language when technically
  necessary. Explain its meaning in `pt-BR` when it may not be clear.
- Apply the conversation and ready-to-copy structures owned by
  [Governance](Governance.md) and [Templates](../templates/Templates.md); all
  of their owner-facing labels, guidance and payload prose use `pt-BR`.
- When those authorities require an explicit absence of owner action or
  follow-up message, state it in `pt-BR` instead of inventing work.

## Project artefacts

Write new project-owned artefacts in British English (`en-GB`), including:

- source code and project-owned technical terminology;
- comments, docstrings and code documentation;
- technical and public documentation;
- README files;
- API and configuration descriptions;
- test names and descriptions;
- project-owned human-readable logs and error messages;
- commit messages.

Use British spelling in project-owned prose. Technical identifiers use clear
English and follow the repository's established conventions.

## External conventions

Preserve mandatory names and spellings imposed by:

- programming languages;
- frameworks and libraries;
- protocols and standards;
- external APIs;
- third-party products.

Do not translate or rename external contracts, canonical fields, error codes,
payload data or externally supplied content merely to apply `en-GB`.

## Existing content

- Do not automatically translate or rewrite existing documentation, source
  history or historical evidence.
- Preserve the current language of an existing file when making a limited
  amendment, avoiding mixed-language documents.
- A full language migration requires a separately authorised and planned
  change.
- Never rewrite Git history to translate previous commit messages.

## User interface

The owner separately selected Brazilian Portuguese (`pt-BR`) and British
English (`en-GB`) as the closed set of supported user-interface languages on
2026-08-01.

- The interface exposes an explicit language choice and renders
  project-owned labels, guidance, status, validation and error messages in the
  selected interface language.
- The selected interface language is independent from `questionLanguage`. A
  user can ask in either supported query language while using either supported
  interface language.
- Source-derived citation content remains in `contentLanguage`; interface
  localisation does not translate evidence.
- Initial selection, persistence and fallback mechanics require their own
  later product decision and must not be inferred by an implementation.
- The separately selected `Light` and `Dark` themes do not select or alter
  the interface language. Theme initialisation, system preference,
  persistence and fallback remain later frontend decisions.

Do not infer the interface language from:

- the conversation language;
- the engineering language;
- the documentation language;
- `questionLanguage` or `answerLanguage`;
- the selected `Light` or `Dark` theme.

## Governance

This policy defines language conventions only. It does not authorise:

- implementation;
- documentation migration;
- lifecycle transitions;
- Git operations;
- external actions;
- releases or deployments.

Language compliance is verified by the applicable
[Quality Gates](Quality-Gates.md). Conversation routing and ready-to-copy
message structures are owned by
[Governance](Governance.md) and [Templates](../templates/Templates.md).
