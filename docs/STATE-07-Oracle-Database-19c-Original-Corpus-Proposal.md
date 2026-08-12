# STATE-07 Oracle Database 19c Original Corpus Proposal

## Purpose and authority

This document proposes a project-authored product corpus for Oracle Database
19c. It defines the proposed authorship, licence, document set, factual-source
method, topic boundary and evaluation criteria. It is a planning artefact
subordinate to the accepted corpus and evaluation architecture; it is not a
corpus document, rights record, dataset revision or activation decision.

On 2026-08-12, the owner selected Oracle Database 19c as the only database
version targeted for the MVP and authorised preparation of this proposal only.
The other 50 records in the canonical 51-product catalogue remain future
`Candidate` records and are not removed or presented as supported.

This authority does not permit authoring or materialising the proposed CSV
documents, reproducing Oracle material, downloading a source, ingestion,
rendering, embeddings, indexing, activation, provider or credential use, OCI,
publication, deployment, an Automatic Quality Gate, a Human Gate or a lifecycle
change. It changes no catalogue or document lifecycle.

The proposal is governed by [`AGENTS.md`](../AGENTS.md), the
[current factual state](../prompts/state/Current-State.md),
[ADR-0004](architecture/ADR-0004-MVP-Corpus-Official-Source-And-Evaluation.md),
the [frozen retrieval evaluation design](evaluation/retrieval-v2-evaluation-design-v1/evaluation-design-contract.json)
and the [language policy](../prompts/governance/Language-Policy.md).

## Confirmed planning baseline

The local documentary preflight on 2026-08-12 established:

| Fact | Observed value |
| --- | --- |
| Branch | `main` |
| `HEAD` | `852edd98c890681840859a5ee9d87e1f299b601f` |
| Prompt corpus | `4.10.38` |
| Working tree before this proposal | clean |
| Owner delivery target | `2026-08-19`; planning target only, not a completion or homologation claim |
| Lifecycle | `STATE-07 TESTING_HOMOLOGATION` active |
| Real authorised product corpus | absent |
| Oracle Database 19c corpus | proposed, not authored or materialised |
| Oracle Database lifecycle | unchanged; not activated by this proposal |
| Other 50 catalogue records | retained for the future; no lifecycle change |

Runtime preflight was `NOT_APPLICABLE` because this increment is documentary
only. No process, runtime, provider, credential, browser or external service was
inspected or used.

## Proposed outcome

Create, under a later and separate authoring authority, one original bilingual
Oracle-only corpus for stable Oracle Database 19c foundations. The corpus would
contain two independently reviewed companion CSV documents:

| Proposed document ID | Proposed filename | `contentLanguage` | Format | Trust class |
| --- | --- | --- | --- | --- |
| `oracle-database-19c-foundations-pt-br-v1` | `oracle-database-19c-foundations-pt-br-v1.csv` | `pt-BR` | CSV | `LocalAuthorised` |
| `oracle-database-19c-foundations-en-gb-v1` | `oracle-database-19c-foundations-en-gb-v1.csv` | `en-GB` | CSV | `LocalAuthorised` |

The two documents would cover the same fact model but would be written and
reviewed in their own language. Neither would be declared a translation of,
edition of or substitute for Oracle documentation. Two exact content-language
records are required so that the real corpus, rather than only synthetic
fixtures, can support the four accepted question/evidence directions:
`pt-BR -> pt-BR`, `pt-BR -> en-GB`, `en-GB -> pt-BR` and
`en-GB -> en-GB`.

CSV is proposed because the accepted MVP supports it and this corpus needs no
page layout, diagrams or visual citations. This removes page rendering,
derivative-image retention and image-serving work from this corpus boundary. It
does not waive CSV parsing, security, provenance, licence, hashing, validation,
indexing or activation controls.

## Authorship and originality

The proposed authorship model is:

- author and copyright holder of record: `RAG-Challenge product owner`;
- public attribution string: `RAG-Challenge product owner`;
- technical fact reviewer: a named stable reviewer role distinct from the
  author role;
- language reviewer: a named stable reviewer role for each exact document
  language; and
- editorial or generative assistance, if later authorised, remains assistance
  only. The product owner must review every row and accept responsibility for
  the final original wording and factual claims.

Each CSV row must be written from a project-owned outline and fact record. The
authoring method prohibits copying, transcription, translation or close
paraphrase of Oracle prose. It also prohibits Oracle text excerpts, tables,
diagrams, screenshots, page images, code samples and document structure. The
initial revision will contain no quotation from Oracle material.

An official source may establish a fact and be cited by identity and URL. That
reference does not incorporate the source into the corpus, transfer its
copyright or represent it as covered by the project licence. Oracle names and
marks are used only to identify the version discussed; no affiliation,
endorsement or ownership is claimed.

## Proposed licence and notice

Only the two original RAG-Challenge CSV documents would be offered under
[Creative Commons Attribution 4.0 International](https://creativecommons.org/licenses/by/4.0/)
(`CC BY 4.0`), subject to its
[legal code](https://creativecommons.org/licenses/by/4.0/legalcode).
The repository software remains under its existing MIT licence.

Each proposed CSV and its governed document record must carry or bind the
following information before it can become a candidate corpus document:

- title, stable document ID, immutable version and exact `contentLanguage`;
- `Copyright 2026 RAG-Challenge product owner`;
- `Licensed under CC BY 4.0` with the exact licence URL;
- the attribution string `RAG-Challenge product owner`;
- a change notice for every successor version;
- the complete factual-source register revision used; and
- an explicit third-party exclusion stating that Oracle documentation, marks
  and other third-party material are not licensed under CC BY 4.0 by the
  RAG-Challenge.

The licence proposal applies only after an original document is actually
authored and its owner approves that document and notice. It grants no right in
Oracle documentation and must never be attached to Oracle PDFs, HTML, images,
tables, diagrams or extracts.

## Proposed CSV contract

Each document would use the same ordered columns:

| Column | Purpose |
| --- | --- |
| `entry_id` | Stable language-specific row identity. |
| `topic_id` | Stable topic from the matrix below. |
| `section_title` | Original short heading in the document language. |
| `explanation` | Original factual explanation scoped to Oracle Database 19c. |
| `limitations` | Explicit boundary, prerequisite or excluded inference. |
| `source_ids` | Semicolon-delimited IDs from the factual-source register. |
| `version_scope` | Exact literal `Oracle Database 19c`. |
| `content_language` | Exact literal matching the document record. |
| `author` | Exact attribution string. |
| `licence` | Exact literal `CC BY 4.0`. |

The later authoring increment must define deterministic CSV escaping, reject
duplicate or empty identities, preserve UTF-8 and neutralise spreadsheet
formula injection. A row must express one bounded factual unit. Source IDs are
metadata for review and provenance; they are not quoted source content.

## Topic and exclusion matrix

| Topic ID | Proposed coverage | Required boundary |
| --- | --- | --- |
| `ORA19-T01` | Oracle Database 19c identity, purpose and relational/object-relational orientation. | No support-date, commercial-edition or licensing entitlement claim. |
| `ORA19-T02` | Difference and relationship between an Oracle database and an Oracle instance. | No topology-specific deployment prescription. |
| `ORA19-T03` | High-level memory and process architecture, including SGA, PGA and principal process roles. | No sizing formula or performance-tuning recommendation. |
| `ORA19-T04` | Physical and logical storage concepts: data files, control files, redo logs, tablespaces, segments, extents and blocks. | No storage capacity or vendor configuration recommendation. |
| `ORA19-T05` | Schemas and core objects such as tables, indexes, views and sequences. | No exhaustive SQL or PL/SQL reference. |
| `ORA19-T06` | Transaction boundaries, commit, rollback, undo, read consistency and locking at a conceptual level. | No application-specific isolation or concurrency prescription. |
| `ORA19-T07` | Oracle Multitenant concepts for container and pluggable databases in the 19c boundary. | No upgrade, migration or tenant-operation procedure. |
| `ORA19-T08` | Security fundamentals: users, authentication concepts, privileges and roles. | No claim of complete hardening, regulatory compliance or secure configuration. |
| `ORA19-T09` | Conceptual role of redo, backup and recovery in data protection and recoverability. | No executable recovery procedure, RPO/RTO promise or availability design. |
| `ORA19-T10` | Administration and developer responsibility boundaries and where authoritative detail must be consulted. | No operational command sequence or substitute for Oracle support guidance. |

The initial corpus excludes installation, patching, Release Update status,
Oracle Support material, private or authenticated sources, exact command
sequences, feature/edition matrices, commercial licensing, migration,
performance prescriptions, security hardening recipes, production architecture
approval and every Oracle Database version other than 19c. A question that
depends on excluded material must produce an insufficient-evidence outcome.

## Candidate factual-source register

These sources are references for later human fact checking, not corpus content
and not evidence that Oracle material is licensed under CC BY 4.0:

| Source ID | Factual purpose | Exact reference | Proposed use boundary |
| --- | --- | --- | --- |
| `ORA19-S01` | Oracle Database 19c documentation collection and version boundary. | [Oracle Database 19c documentation](https://docs.oracle.com/en/database/oracle/oracle-database/19/) | Locate the version-specific primary documentation. Do not crawl or copy. |
| `ORA19-S02` | Core database, instance, memory, process, storage, schema, transaction and multitenant concepts. | [Oracle Database Concepts 19c](https://docs.oracle.com/en/database/oracle/oracle-database/19/cncpt/index.html) | Check discrete facts and record section-level references during authoring. Do not reproduce wording or structure. |
| `ORA19-S03` | Navigable topic identities within Database Concepts 19c. | [Database Concepts 19c table of contents](https://docs.oracle.com/en/database/oracle/oracle-database/19/cncpt/toc.htm) | Identify the exact chapter used by each fact record; the final register must cite the exact HTML location. |
| `ORA19-S04` | Oracle site-use restrictions applicable to source consultation. | [Oracle Terms of Use](https://www.oracle.com/legal/terms/) | Compliance reference only; it does not license the proposed project corpus or automated acquisition. |
| `ORA19-S05` | Nominative use and attribution boundaries for Oracle marks. | [Oracle trademark guidelines](https://www.oracle.com/legal/trademarks/) | Compliance reference only; no endorsement or ownership claim. |
| `LIC-S01` | Proposed licence conditions for original project-authored text. | [CC BY 4.0 deed](https://creativecommons.org/licenses/by/4.0/) and [legal code](https://creativecommons.org/licenses/by/4.0/legalcode) | Applies only to original RAG-Challenge documents after owner approval. |

The later authoring authority must freeze a source-register revision with exact
section URLs, access dates and a claim-to-source map before factual review. A
moving documentation page, a search result or an uncited recollection is not
sufficient evidence. Consultation must be manual unless a later authority and
applicable terms expressly permit an automated method. No source bytes need to
be downloaded, retained or redistributed for this original-corpus approach.

## Evaluation criteria

### Editorial candidate acceptance

Before either CSV can be proposed for ingestion, both documents must satisfy
all of the following:

1. every row is original, bounded to Oracle Database 19c and linked to at least
   one exact source-register location;
2. the author and both reviewer roles approve the immutable document version;
3. the bilingual pair has the same topic and fact inventory, while each
   language remains independently written and reviewed;
4. no Oracle prose, quotation, table, diagram, image, page, code sample or close
   paraphrase is present;
5. every fact is supported by the cited primary source and every source supports
   the claim attributed to it;
6. limitations and excluded inferences are explicit, especially for security,
   licensing, support, recovery, performance and production use;
7. the CC BY 4.0 notice and third-party exclusion are complete and apply only to
   the original CSV content;
8. schema, encoding, escaping, unique identities, language tags, row counts and
   hashes pass deterministic validation; and
9. the result contains no secret, personal, customer, confidential, private
   support or authenticated-source material.

A similarity tool may assist the originality review, but it cannot establish
copyright clearance or replace the recorded human comparison. Any uncertain
fact, wording similarity, attribution or right is a hard stop for that row.

### Future retrieval and answer evaluation

Materialisation of evaluation data is not authorised here. A later Oracle 19c
dataset revision must inherit the accepted ID `rag-eval-catalogue-v1` and the
current frozen quota design rather than use the smaller historical synthetic
candidate as a product threshold. At minimum, that design currently requires:

- at least 200 positive cases overall;
- at least `max(50, ceiling(0.25 * N_positive))` negative cases;
- at least 25 cases for each negative subtype and at least 25 negative cases in
  each supported question language;
- at least 25 positive cases for each question-language/content-language row;
- both Oracle 19c documents represented in both question languages;
- the CSV format and `LocalAuthorised` source stratum represented by at least 25
  positive cases each; and
- at least 30 positive cases for the active database and source stratum, with
  both question languages represented, before a representative product claim.

The Oracle-specific case design must distribute the positive set across all ten
topic IDs and all four language directions. It must include exact-location and
citation identity, prohibited extrapolation, insufficient-evidence, cross-
database and lifecycle filtering, provenance confusion and retrieved prompt-
injection resistance. Evaluation questions and expected answers remain outside
the runtime corpus.

The unchanged ADR-0004 thresholds govern any later scored campaign, including
Recall@5, MRR@5, citation validity, answer language, supported factual claims,
correct insufficiency, zero high-impact unsupported claims, zero leakage, zero
incorrect provenance and zero successful instruction override. Passing corpus
editorial review does not prove retrieval quality, answer quality, security,
performance, provider behaviour or homologation.

## Risks and stop conditions

Stop proposal follow-on work if:

- a requested topic requires reproducing or closely paraphrasing Oracle
  proprietary expression;
- any source, licence or trademark boundary is ambiguous or incompatible;
- exact Oracle Database 19c support for a fact cannot be established;
- a later Oracle version is silently substituted for 19c;
- the two language documents diverge in their fact inventory or limitations;
- the exact author attribution or reviewer roles cannot be recorded;
- an official source requires authentication, a secret, download, automation or
  retention not separately authorised;
- the work would change a catalogue/document lifecycle, dataset, provider,
  external environment, public claim or protected contract; or
- branch, `HEAD`, prompt corpus, clean-tree status or explicit authority differs
  from the baseline named by the next increment.

## Proposed follow-on sequence

This sequence is planning only and grants no authority:

1. owner review and approval of this proposal, including the authorship string,
   bilingual CSV composition, CC BY 4.0 boundary, topics and criteria;
2. separately authorised original authoring and factual review of the two CSVs,
   without ingestion or lifecycle change;
3. separately authorised rights/provenance and deterministic document
   validation, producing immutable identities and hashes;
4. separately authorised ingestion, candidate indexing and explicit
   administrative activation; and
5. separately authorised RB-2 dataset materialisation and later evaluation
   gates.

No step implies the next. Oracle Database 19c may be announced as active or
homologated only after its required document, evaluation and lifecycle gates
have been completed and explicitly approved.

## Completion boundary

This proposal is complete when its documentary checks pass and it is preserved
in one focused local commit. Completion records only a proposed Oracle Database
19c corpus design. It does not create corpus bytes, grant CC BY 4.0 over an
unwritten work, prove Oracle-source rights, change factual state, authorise the
next step or support any public product claim.
