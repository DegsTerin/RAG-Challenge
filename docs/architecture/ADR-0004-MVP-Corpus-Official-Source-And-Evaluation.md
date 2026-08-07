# ADR-0004 — MVP Catalogue, Governed Documents, Official Sources and Evaluation

- Status: accepted
- Date: 2026-07-31
- Accepted: 2026-08-01
- Decision authority: explicit product-owner acceptance on baseline
  `main@39e2f803bf73cb4e2b59e56a0596e2858a3aed51`, corpus `4.7.0`
- Owners: RAG-Challenge product, RAG evaluation and security
- State: `STATE-02 ARCHITECTURE`
- Refined by: accepted ADR-0008 for durable source/page-image storage and
  visual-evidence rights, and accepted ADR-0009 for separate query and
  document-language domains; implementation remains separately authorised
- Verification status: substantially complete only for the first PostgreSQL
  source candidate; no conflicting robots or terms policy was found, no
  publisher rate guidance was located, and every later source requires its own
  evidence before activation

## Purpose and authority

This ADR defines the governed knowledge sources and pre-registered evaluation
baseline for the MVP. Acceptance does not license content, approve an external
URL or authorise network access. Every actual document/source must also pass
the evidence gate below under separate authority before activation.

## Context

The MVP requires one logical corpus with an administrator-managed database and
document catalogue. Documents may be locally supplied authorised content or
snapshots of official external sources. PDF and CSV are the initial formats.
All active/current documents participate in unified query-time retrieval;
origin and trust remain explicit provenance. Query-time retrieval never fetches
from the network or silently hides degraded coverage.

The local Challenge materials are provenance inputs only and cannot be used as
the product corpus. Evaluation criteria must be frozen before the first
homologation run that could influence thresholds.

## Owner-decided query-language constraint

On 2026-08-01, the owner explicitly selected Brazilian Portuguese (`pt-BR`)
and British English (`en-GB`) for questions and answers. An answer uses the
declared question language; source-derived citation text remains in its
original language. Deterministic tests cover same-language retrieval and both
cross-language directions. This constraint was accepted independently of the
corpus, source, licence and threshold decisions below, did not at that time
decide the Dashboard language or accept this ADR as a whole.
A later, separate owner decision selected `pt-BR` and `en-GB` as the supported
Dashboard languages without coupling them to the query-language matrix.

## Owner-decided corpus-scale constraint

On 2026-08-01, the owner explicitly removed the proposed ceilings of twelve
database systems and 120 pages. Each published corpus version remains finite
and records its actual system and page counts, but the product contract sets no
maximum for either count. Operational capacity, parser safety and resource
budgets must be validated against the selected corpus rather than used to
exclude systems or pages by a fixed product rule. This constraint was accepted
independently of the source, licence and evaluation decisions below and did
not at that time accept this ADR as a whole.

## Owner-decided catalogue and document constraint

On 2026-08-01, the owner explicitly established 51 unique database products,
9 categories and 54 many-to-many assignments as the initial canonical
catalogue. Redis, SAP HANA and SingleStore are single entities assigned to two
categories each. Administrators may add, version, activate, deactivate and
logically remove any number of compatible database products and associated PDF
or CSV documents without a hard-coded list, code change or ADR per item.

Every active database must have at least one active document. New databases,
documents and versions begin as `Candidate`; only validation, candidate
indexing and explicit administrative activation make them queryable.
Deactivation preserves history/provenance, removal is a logical auditable
tombstone, and physical deletion follows retention. Removing the last active
document requires explicit atomic deactivation of its database. These
constraints did not at that time accept this ADR as a whole and do not
authorise implementation, network access or source synchronisation.

## Decision

The accepted decision is:

### Canonical catalogue and governed documents

- Use stable corpus ID `database-systems-catalogue-mvp`.
- Represent the following names exactly as stable catalogue data, not code
  enums, constants or adapter branches:

| Category | Canonical database products |
|---|---|
| Relacionais (SQL) | PostgreSQL; MySQL; MariaDB; Microsoft SQL Server; Oracle Database; SQLite; IBM Db2; SAP HANA; Firebird; Teradata; CockroachDB; YugabyteDB; SingleStore; TiDB; Amazon Aurora |
| Documentos (NoSQL) | MongoDB; Couchbase; CouchDB; RavenDB; Amazon DocumentDB; Azure Cosmos DB |
| Chave-valor | Redis; Valkey; Amazon DynamoDB; Riak KV; Aerospike |
| Wide-column | Apache Cassandra; ScyllaDB; Apache HBase; Google Bigtable |
| Grafos | Neo4j; Amazon Neptune; TigerGraph; JanusGraph; ArangoDB |
| Busca | Elasticsearch; OpenSearch; Apache Solr |
| Séries temporais | InfluxDB; TimescaleDB; QuestDB; VictoriaMetrics |
| Data Warehouse / Analytics | Snowflake; Google BigQuery; Databricks SQL; Amazon Redshift; ClickHouse; Vertica; DuckDB; Apache Doris; StarRocks |
| Em memória | Redis; SAP HANA; SingleStore |

- Associate each document with a database product and record its immutable
  version, PDF/CSV format, canonical BCP 47 `DocumentContentLanguage`, exact
  publisher-declared language evidence when available, provenance,
  licence/use rights, hash, source adapter and trust classification. Language
  values are catalogue evidence and do not select a provider or imply query
  support.
- Require each active database to have at least one active document; permit any
  number of additional documents without a product ceiling.
- Permit owner-authored, owner-authorised or official external documents only
  when their rights and provenance permit the intended parsing, indexing,
  source-byte retention, quotation, citation and any publication. A PDF that
  will provide visual evidence additionally requires explicit rights for page
  rendering, creation and retention of derivative images, runtime display and
  the intended source/derivative distribution boundary.
- Persist authorised source bytes and page-image derivatives only through the
  content-addressed `IDocumentContentStore`. An intake path, Git checkout, Git
  LFS pointer, catalogue row or vector index is not durable product content.
- Keep `reference-materials/` excluded. Its contents do not become product
  corpus by catalogue registration.
- Apply bounded byte, page/line, parser-working-set, time and concurrency
  safeguards per operation. Capacity failure blocks candidate activation and
  never silently removes catalogue items.

The catalogue is a documentary contract, not an implemented dataset. No
document for the 51 products has been acquired, authored, validated, indexed
or activated by this decision. `CC BY 4.0` remains a valid candidate for
owner-authored documentation, but every actual document retains its own
verified licence/provenance record and is not relicensed by inference.

The [CC BY 4.0 deed](https://creativecommons.org/licenses/by/4.0/) and
[Portuguese legal code](https://creativecommons.org/licenses/by/4.0/legalcode.pt)
were inspected on 2026-07-31. They permit sharing and adaptation, including
commercial use, subject to attribution, licence-link and change-notice
conditions. This verifies only the candidate licence terms; it grants no rights
over an unwritten or third-party document.

### First official source candidate

- Use PostgreSQL documentation as the first verified official source candidate,
  not as the exclusive source or a template that authorises other URLs.
- Candidate source ID: `postgresql-18-reference-a4`.
- Candidate canonical URL:
  `https://www.postgresql.org/files/documentation/pdf/18/postgresql-18-A4.pdf`.
- Require an empty query and fragment, implicit port `443`, exact ASCII host
  `www.postgresql.org` and exact case-sensitive path shown above.
- Use generic compatible source adapter ID `https-official-pdf-v1` and trust class
  `OfficialExternal`.
- Recommend `maxAge=168h` and manual revalidation no more frequently than once
  every 24 hours, except an explicitly authorised incident check.
- Limit a response to 64 MiB transferred, 256 MiB parser working input and
  5,000 pages. Permit one in-flight synchronisation and no automatic retry
  after a policy, content or validation failure.
- Disable redirects, authentication, cookies, proxies and ambient
  credentials. Preserve the canonical public URL in citations.

The exact candidate URL and its immediate response behaviour were verified on
2026-07-31 under bounded read-only authority. The URL returned `200` to
`HEAD` without a `Location` header, `Content-Type: application/pdf`,
`Content-Length: 15771040`, `ETag: "6a05c867-f0a5a0"`,
`Last-Modified: 2026-05-14T13:04:39Z` and `Accept-Ranges: bytes`. A single
`GET` for bytes `0-65535` returned `206`, the matching content range, exactly
65,536 bytes and `%PDF-1.4`; no snapshot or response body was persisted.

The [PostgreSQL documentation index](https://www.postgresql.org/docs/) lists
PostgreSQL 18 as the current manual and links an A4 PDF. The
[PostgreSQL Licence](https://www.postgresql.org/about/licence/) expressly
covers use, copy, modification and distribution of the software and its
documentation, subject to retaining the required notices. This supplies the
accepted use-right basis but does not waive attribution/notice handling.

The [published robots policy](https://www.postgresql.org/robots.txt) returned
`200` without redirect and does not disallow the versioned documentation PDF
path. The [official policies index](https://www.postgresql.org/about/policies/)
does not list a separate general terms-of-use policy, and targeted official
search found no download-rate guidance. Therefore the PostgreSQL Licence is
the published use-right basis, while the accepted once-per-24-hours limit is a
project restraint rather than a publisher commitment.

A direct TLS check resolved three public addresses, negotiated TLS 1.3 and
validated a four-element chain against local trust with certificate downloads
disabled and revocation set to `NoCheck`. This demonstrates current local
no-lateral-download validation, not future certificate availability or the
clean OCI environment. It preserves the explicit residual risk that offline
validation cannot learn a new revocation until trust material is updated. A
changed major version or URL requires a new Candidate registration/version and
the same verification/activation controls. It needs an amended ADR only when it
introduces a new integration or policy class.

### Evaluation dataset

- Freeze dataset ID `rag-eval-catalogue-v1` and its catalogue/document manifest
  before the first scored run.
- Do not impose a fixed total case count. Grow the dataset with every active
  database, document, source and format.
- For each active database, include answerable, insufficient-evidence,
  citation-boundary and prompt-injection/provenance cases appropriate to its
  documents. PDF and CSV each require format-specific location cases whenever
  present in the active set.
- Annotate each answerable case with database/revision, allowed document
  version, one or more relevant locations, required facts, prohibited
  extrapolations and expected provenance.
- Annotate every case with `questionLanguage` and the expected
  exact canonical BCP 47 `contentLanguage` of its evidence, plus the preserved
  `sourceDeclaredLanguage` when present. Include answerable questions in both
  supported query languages, `pt-BR` and `en-GB`, against the approved evidence
  across the dataset.
- Supplement the scored product-corpus cases with deterministic contract and
  integration fixtures that cover `pt-BR→pt-BR`, `en-GB→en-GB`,
  `pt-BR→en-GB` and `en-GB→pt-BR` between question and evidence. Fixtures are
  not product corpus and are never reported as product-source coverage.
- Keep each additional exact document-language tag in its own reported
  evidence stratum. Do not count `en` as `en-GB` or silently merge language
  strata. A PostgreSQL product-corpus campaign therefore requires at least
  `pt-BR -> en` and `en-GB -> en`; the mandatory `en-GB` evidence rows require
  an independently authorised `en-GB` document or clearly separated fixtures.
- Keep evaluation questions and expected answers out of the runtime corpus.
- Use deterministic retrieval evaluation and a documented two-person human
  rubric for answer quality. A model judge may be supplementary but cannot be
  the sole gate authority.

### Pre-registered thresholds

| Measure | Threshold |
|---|---:|
| Recall@5 for answerable cases | at least `0.90` overall and `0.85` for every reportable database/source stratum |
| Mean reciprocal rank at 5 | at least `0.75` for every reportable database/source stratum |
| Citation identity and location validity | `1.00` |
| Answer language equals declared question language | `1.00` |
| Source-derived citation text preserved in its original language | `1.00` |
| Supported factual claims | at least `0.95` |
| Correct insufficient-evidence outcome | at least `0.95` |
| Unsupported high-impact factual claims | `0` |
| Cross-database filter, cross-generation or cross-corpus leakage | `0` |
| Incorrect provenance or silent substitution of a degraded source | `0` |
| Successful instruction override from retrieved content | `0` |
| Stale, withdrawn or deactivated source calls to embedding/LLM | `0` |
| Query p95 on the named homologation environment | at most `12 s` |
| Query p99 on the named homologation environment | at most `20 s` |
| Evaluation campaign provider spend | at most `USD 20` |

Latency and cost results are not yet observed. A threshold may be amended only
before a new campaign begins, with the reason and new dataset/model baseline
recorded. It cannot be changed after observing a failing run merely to pass.

## Verification required before each source activation

The following evidence must be collected for each official source from primary
publisher sources under separately authorised, allowlisted HTTPS access. The
record below covers only the PostgreSQL candidate and grants no authority to
another source:

1. the candidate PDF responds from the exact URL without redirect;
2. the official publisher controls the host and path;
3. the current licence and terms permit the intended download, local snapshot,
   parsing, indexing, source-byte retention, quotation and citation, and, when
   visual evidence is required, page rendering, derivative-image creation and
   retention, runtime display and the intended distribution boundary;
4. robots and published rate guidance permit the accepted manual frequency;
5. the response is an anonymous PDF within the accepted limits;
6. certificate validation can operate with the no-lateral-egress TLS policy;
7. a sanitised record captures date, status, media type, size, validators and
   evidence references without downloading into the repository.

Evidence status on 2026-07-31:

| Item | Status | Evidence or remaining work |
|---|---|---|
| 1 | Verified | Exact URL returned without redirect. |
| 2 | Verified | The official documentation index and PDF use the exact `www.postgresql.org` authority and versioned path. |
| 3 | Partially verified; visual rights expansion pending | The PostgreSQL Licence supplies the recorded use/copy basis and required notices. Before rendering or visual activation, the eligibility record must explicitly dispose page rendering, derivative-image creation/retention, runtime display and the intended distribution boundary. |
| 4 | Verified with qualification | `robots.txt` does not disallow the exact path; no publisher download-rate guidance was located, so the accepted daily ceiling remains a project-owned conservative limit. |
| 5 | Verified | Anonymous PDF, `application/pdf`, 15,771,040 bytes and valid leading signature. |
| 6 | Verified locally | TLS 1.3 and a four-element offline chain validated with certificate downloads disabled and revocation `NoCheck`; clean-environment and OCI reproduction remain later acceptance tests. |
| 7 | Verified | This ADR and the state report contain the sanitised response record and primary-source references; no snapshot was retained. |

No real product snapshot may be retained during architecture verification.

## Alternatives

### Use third-party or course material as the local corpus

Rejected because redistribution and product-runtime rights are not
established, and `reference-materials/` is explicitly local-only.

### Collapse local and official provenance

Rejected because unified retrieval does not justify erasing trust,
licence, acquisition and freshness differences from evidence metadata.

### Use a changing `current` documentation URL

Rejected because a moving path weakens reproducibility. A versioned PDF is
preferred even when the source publishes a newer version.

### Crawl HTML or accept arbitrary URLs

Rejected for the MVP. Administrative cardinality does not grant generic web
authority; every official PDF/CSV source remains an exact allowlisted record.

### License the corpus under the repository MIT licence

Not recommended. `CC BY 4.0` communicates attribution and documentation reuse
more clearly while remaining separate from the software licence.

## Consequences

- The owner must supply or authorise at least one valid PDF/CSV document per
  database before implementation can claim that database as active/queryable.
- Removing fixed system and page ceilings increases delivery, evaluation and
  capacity uncertainty. Corpus size must be measured per version, and a
  candidate environment that cannot process it safely must be scaled or
  changed rather than silently reducing the approved catalogue.
- Every versioned official document has its own finite security boundary;
  adding a compatible record increases evaluation, legal and operational work
  without requiring a per-item ADR.
- The evaluation set grows with the active catalogue; its size is an observed
  consequence rather than a product ceiling.
- Exact thresholds may expose provider or chunking weaknesses early; that is a
  desired gate outcome rather than a reason to relax the baseline.
- Cross-language retrieval or answer-language failures reject the candidate
  provider or prompt baseline; they do not justify translating citations or
  weakening the accepted language requirement.
- Broader document-language support increases dataset strata without expanding
  the closed `pt-BR`/`en-GB` query-language contract. Results name exact tags,
  documents, dataset revision, provider and environment.
- PDF visual evidence increases durable storage, rendering, backup, security,
  accessibility and rights work; a partial or unverifiable render candidate
  blocks activation rather than degrading silently.

## Security, privacy and operations

- Every PDF/CSV remains an untrusted parser and prompt input.
- No question, URL or model output can alter the catalogue or configured
  sources.
- Initial external sources are anonymous and must not receive a secret-bearing header,
  environment credential or signed query.
- Source and page-image bytes stay outside ordinary Git and Git LFS as product
  storage. A separately authorised licence-safe export remains a distribution
  artefact, never the runtime system of record.
- Any retained PostgreSQL documentation copy preserves the licence copyright
  notice and required paragraphs in the governed source record or
  distribution bundle.
- The evaluation dataset must contain no personal, customer or confidential
  information.

## Acceptance checks

- The explicit acceptance follows reconciliation of the seven external
  verification items for the first PostgreSQL source candidate.
- The catalogue contains exactly the 51 approved unique names and 54 category
  assignments for its initial revision; no abbreviated example list replaces
  it.
- Every active database has at least one active, hash-verified PDF/CSV document;
  every document records format, language, rights, provenance and location
  semantics.
- Every document records a canonical BCP 47 `contentLanguage`; any observed
  publisher tag is preserved exactly, and no generic `en` value is inferred as
  `en-GB`.
- Rights and attribution are independently checked for software and each
  document/snapshot.
- Every visually active PDF has compatible rendering and derivative rights, a
  complete finalised render manifest and verified content-addressed PNGs; CSV
  receives no implicit page-image derivative.
- Candidate, activation, deactivation and logical removal preserve history;
  the last active document cannot leave an active database.
- Dataset membership and thresholds are frozen before any scored run.
- Every case declares its database, document/version, format and expected
  provenance; the active dataset covers every active database.
- Every case declares exactly one `questionLanguage`; the combined scored and
  deterministic suites cover both same-language pairs and both cross-language
  directions without changing citation text. Additional exact
  document-language tags are reported as separate strata and never substitute
  for a mandatory matrix row.
- The decision does not authorise a real network synchronisation or create a
  product snapshot.
