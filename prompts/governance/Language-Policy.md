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

Write current and new editable project-owned technical artefacts in British
English (`en-GB`), including:

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

Owner-facing communication and copy-ready payloads embedded as examples in a
technical artefact remain in `pt-BR`. This is an intentional audience boundary,
not mixed technical prose.

## Commit messages

Every new Git commit created for this repository must use British English
(`en-GB`) throughout its project-owned message. This is a mandatory invariant
for every commit, regardless of change type, scope, authoring tool or delivery
stage. It applies to:

- the description after the Conventional Commit type and optional scope;
- every project-owned sentence in an optional commit body; and
- project-owned footer prose.

Conventional Commit types and scopes, source identifiers, paths, commands,
protocol fields, external product names, issue IDs and canonical Git trailers
retain their required spelling. They are technical or external literals and
must not be renamed merely to resemble British prose.

The complete message must be checked before the commit is created. Prefer
British forms such as `serialise`, `initialise`, `behaviour` and `authorised`
when those words occur in project-owned prose. For example:

```text
test(integration): serialise shared SQLite state
```

is compliant; the same description using `serialize` is not.

A non-compliant local commit message is a failed pre-delivery check. Stop and
correct it before hand-off only when the applicable Git authority permits the
required operation. This language rule does not itself authorise amend,
rebase or history rewriting.

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

- Migrate editable, current, project-owned technical prose to `en-GB` only
  through an explicitly authorised, reviewed and gated change. A limited
  amendment outside such a migration preserves the existing file language and
  avoids introducing mixed technical prose.
- Never rewrite Git history or amend previous commit messages for translation.
- Preserve append-only records and their integrity-protected prefixes. New
  append-only entries follow this policy without translating earlier entries.
- Preserve historical evidence and original owner inputs in their original
  language. An `en-GB` provenance record may describe them but does not replace,
  translate or reactivate them.
- Preserve source content and citations in their original language.
- Preserve external names, APIs, protocols, canonical fields, identifiers,
  error codes and other mandatory literals.
- Preserve functional `pt-BR`/`en-GB` localisation. Changing the supported
  interface or query languages requires a separate product decision.

## Automated enforcement

The repository applies the schema-validated policy in
[`../../eng/language-policy.json`](../../eng/language-policy.json) and the
temporary migration inventory in
[`../../eng/language-migration-baseline.json`](../../eng/language-migration-baseline.json).
Exclusions are exact, classified paths rather than generic ignore patterns.
While migration is `IN_PROGRESS`, only an identical fingerprinted occurrence
may remain; new or changed debt fails. A `COMPLETE` baseline must contain zero
debt. The checker also protects accepted append-only prefixes and validates
each new commit message without reinterpreting earlier Git history.

Automated lexical checks are deliberately conservative. Passing them is
necessary but not sufficient: an independent semantic review still confirms
audience, ownership, external naming, source language and idiomatic British
English.

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
