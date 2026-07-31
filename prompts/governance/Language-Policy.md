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
- Present headings, labels, guidance and explanatory values intended for the
  owner in `pt-BR`. Do not add bilingual labels merely to mirror an internal
  canonical name.
- Keep a literal filename, path, command, code fragment, API name, protocol
  field or canonical enum in its required language when technically
  necessary. Explain its meaning in `pt-BR` when it may not be clear.
- Every project communication states the immediate next step, the
  conversation in which it belongs and a complete `pt-BR` message ready for
  the owner to copy and send.
- When no owner action or follow-up message is needed, say so explicitly
  instead of inventing work.

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

Treat the user-interface language as a separate product decision.

Do not infer the interface language from:

- the conversation language;
- the engineering language;
- the documentation language.

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
