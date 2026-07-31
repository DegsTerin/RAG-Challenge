# ADR-0004 — MVP Corpus, Official Source and Evaluation Baseline

- Status: proposed
- Date: 2026-07-31
- Owners: RAG-Challenge product, RAG evaluation and security
- State: `STATE-02 ARCHITECTURE`
- Verification status: blocked for external facts; no network access was
  authorised for this decision package

## Purpose and authority

This ADR proposes the bounded knowledge sources and pre-registered evaluation
baseline for the MVP. It does not license content, approve an external URL,
authorise network access or accept itself. The product owner must decide the
ADR explicitly after the external evidence identified below has been verified
under separate authority.

## Context

The MVP requires one logical corpus with two evidence scopes. `Local` uses one
owner-authorised PDF. `OfficialOnline` uses one immutable snapshot of one exact
official HTTPS PDF. Query-time retrieval must never fetch from the network,
mix scopes or fall back silently.

The local Challenge materials are provenance inputs only and cannot be used as
the product corpus. Evaluation criteria must be frozen before the first
homologation run that could influence thresholds.

## Proposed decision

If accepted after verification:

### Local corpus

- Use one owner-authored document named `Database Systems Catalogue — MVP`.
- Use stable corpus ID `database-systems-catalogue-mvp`.
- Author the document in British English and publish it as an accessible PDF
  generated from a tracked source artefact.
- License the owner-authored corpus under `CC BY 4.0`, separately from the MIT
  software repository. Include the licence, author, version and generation
  method in the source and PDF metadata.
- Cover a finite representative set of twelve systems:
  PostgreSQL, MySQL, MariaDB, Microsoft SQL Server, Oracle Database, SQLite,
  MongoDB, Valkey, Apache Cassandra, OpenSearch, Neo4j and InfluxDB.
- For each system, cover only: data model, primary workload, consistency and
  transaction characteristics, deployment shape, query interface, scaling
  model, operational constraints and explicit non-recommendation caveats.
- Cite the public source used to substantiate each factual section in the
  authored source document. Do not copy third-party prose, logos or diagrams.
- Treat trade-marked product names as nominative references and include a
  non-affiliation notice.
- Limit the first authorised PDF to 120 pages and 20 MiB. A larger document
  requires a recorded impact review before ingestion.

The document does not exist yet. Acceptance of this ADR selects its contract
and licence but does not claim that the corpus has been authored, reviewed or
licensed in fact.

### Official source candidate

- Use PostgreSQL documentation as the single `OfficialOnline` source.
- Candidate source ID: `postgresql-18-reference-a4`.
- Candidate canonical URL:
  `https://www.postgresql.org/files/documentation/pdf/18/postgresql-18-A4.pdf`.
- Require an empty query and fragment, implicit port `443`, exact ASCII host
  `www.postgresql.org` and exact case-sensitive path shown above.
- Use source adapter ID `postgresql-official-pdf-v1` and trust class
  `OfficialExternal`.
- Recommend `maxAge=168h` and manual revalidation no more frequently than once
  every 24 hours, except an explicitly authorised incident check.
- Limit a response to 64 MiB transferred, 256 MiB parser working input and
  5,000 pages. Permit one in-flight synchronisation and no automatic retry
  after a policy, content or validation failure.
- Disable redirects, authentication, cookies, proxies and ambient
  credentials. Preserve the canonical public URL in citations.

The candidate URL, current availability, publisher identity, licence, terms,
robots policy, expected media type, observed size, certificate chain and
redirect behaviour have not been verified in this state because external
network access was explicitly excluded. They are blocking evidence, not
assumptions. A changed major version or URL requires an amended ADR rather
than silent substitution.

### Evaluation dataset

- Freeze dataset ID `rag-eval-mvp-v1` before the first scored run.
- Use 80 cases split equally between `Local` and `OfficialOnline`.
- Per scope, include 24 answerable questions, 8 insufficient-evidence cases,
  4 citation-boundary cases and 4 prompt-injection or scope-leakage cases.
- Annotate each answerable case with the allowed document version, one or more
  relevant locations, required facts, prohibited extrapolations and the
  expected evidence scope.
- Keep evaluation questions and expected answers out of the runtime corpus.
- Use deterministic retrieval evaluation and a documented two-person human
  rubric for answer quality. A model judge may be supplementary but cannot be
  the sole gate authority.

### Pre-registered thresholds

| Measure | Threshold |
|---|---:|
| Recall@5 for answerable cases | at least `0.90` overall and `0.85` per scope |
| Mean reciprocal rank at 5 | at least `0.75` per scope |
| Citation identity and location validity | `1.00` |
| Supported factual claims | at least `0.95` |
| Correct insufficient-evidence outcome | at least `0.95` |
| Unsupported high-impact factual claims | `0` |
| Cross-scope, cross-generation or cross-corpus leakage | `0` |
| Silent fallback from `OfficialOnline` to `Local` | `0` |
| Successful instruction override from retrieved content | `0` |
| Stale, withdrawn or deactivated source calls to embedding/LLM | `0` |
| Query p95 on the named homologation environment | at most `12 s` |
| Query p99 on the named homologation environment | at most `20 s` |
| Evaluation campaign provider spend | at most `USD 20` |

Latency and cost results are not yet observed. A threshold may be amended only
before a new campaign begins, with the reason and new dataset/model baseline
recorded. It cannot be changed after observing a failing run merely to pass.

## External verification required before decision

The following evidence must be collected from primary publisher sources under
separately authorised, allowlisted HTTPS access:

1. the candidate PDF responds from the exact URL without redirect;
2. the official publisher controls the host and path;
3. the current licence and terms permit the intended download, local snapshot,
   parsing, indexing, quotation and citation;
4. robots and published rate guidance permit the proposed manual frequency;
5. the response is an anonymous PDF within the proposed limits;
6. certificate validation can operate with the no-lateral-egress TLS policy;
7. a sanitised record captures date, status, media type, size, validators and
   evidence references without downloading into the repository.

No real product snapshot may be retained during architecture verification.

## Alternatives

### Use third-party or course material as the local corpus

Rejected because redistribution and product-runtime rights are not
established, and `reference-materials/` is explicitly local-only.

### Use a vendor manual as both local and official evidence

Rejected because it would obscure the trust and provenance distinction
between owner-authored and externally governed sources.

### Use a changing `current` documentation URL

Rejected because a moving path weakens reproducibility. A versioned PDF is
preferred even when the source publishes a newer version.

### Use several official PDFs or crawl HTML

Rejected for the MVP. It increases legal, freshness, SSRF and operational
surface without meeting a current requirement.

### License the corpus under the repository MIT licence

Not recommended. `CC BY 4.0` communicates attribution and documentation reuse
more clearly while remaining separate from the software licence.

## Consequences

- The owner must author and review the local corpus before implementation can
  claim a usable `Local` source.
- A single versioned official manual gives a reproducible and finite security
  boundary but does not represent all supported database systems.
- The evaluation set is intentionally modest and must be expanded if the
  corpus or supported claims expand.
- Exact thresholds may expose provider or chunking weaknesses early; that is a
  desired gate outcome rather than a reason to relax the baseline.

## Security, privacy and operations

- Both documents remain untrusted parser and prompt inputs.
- No question, URL or model output can alter the configured source.
- The source is anonymous and must not receive a secret-bearing header,
  environment credential or signed query.
- Snapshot bytes stay outside Git unless the verified licence expressly
  permits redistribution and the owner separately authorises it.
- The evaluation dataset must contain no personal, customer or confidential
  information.

## Acceptance checks

- The owner explicitly decides this ADR after the seven external verification
  items have evidence.
- The local corpus source and generated PDF are reproducible and have matching
  version/hash metadata.
- Rights and attribution are independently checked for the software, local
  corpus and official snapshot.
- Dataset membership and thresholds are frozen before any scored run.
- Every case declares exactly one `SourceScope`.
- The decision does not authorise a real network synchronisation or create a
  product snapshot.
