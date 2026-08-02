# STATE-03 Data and Index Modeling Report

## Report status

- Lifecycle state: `STATE-03 DATA_AND_INDEX_MODELING` active.
- Executed increment: `S03-A` and `S03-B0` through `S03-B5` complete.
- Entry baseline: `main@35b67c194f6ea2459833420b8bc2143fadfe75df`.
- Instruction corpus: `4.9.1`.
- Entry authority recorded locally: commit `5efaa37`.
- S03-A implementation recorded locally: commit `ace780a`.
- S03-B resumption baseline: `main@381d1cd297580476e461a242ce5b66c4884e521b`.
- S03-B5 diagnostic resumption baseline:
  `main@c72c8b967667f72e8971f4887174585d3640a36e`.
- Report date: 2026-08-02.
- Automatic Quality Gate: pending and not inferred from the checks below.
- Human Gate: pending.
- State closure and entry into `STATE-04`: not authorised.

This is a factual increment-completion report. It records S03-B as complete
but does not close `STATE-03`; no Automatic Quality Gate or Human Gate has
been executed or inferred.

## Authority and preconditions

Before the first write, the repository was rechecked as branch `main`, commit
`35b67c194f6ea2459833420b8bc2143fadfe75df`, corpus `4.9.1`, with a clean
working tree. `STATE-02` was closed with its Automatic Quality Gate and Human
Gate approved without reservations; ADR-0002 and ADR-0004 through ADR-0007
were accepted. No entry authority for `STATE-04` existed.

The entry into `STATE-03` was recorded append-only before executable S03-A
work. Runtime preflight then found zero RAG-Challenge-owned processes and zero
owned listeners, so nothing was stopped.

## Implemented S03-A result

| Authorised concern | Result |
|---|---|
| Catalogue model and dictionary | Provider-neutral Domain model plus the permanent [S03-A data dictionary](data/STATE-03-S03-A-Data-Dictionary.md). |
| Identities and statuses | Typed catalogue/source/build/generation/revision/digest identities; closed catalogue, format, language, trust, freshness and build states. |
| Relations and constraints | Many-to-many product/category model; document-version ownership; one active document version; active product/document evidence constraints; local/official provenance separation. |
| Revision separation | Distinct `CatalogueRevision`, `ObservationJournalRevision`, `ActivationRecordRevision` and source-registration revision types; physical row version explicitly deferred. |
| Canonical serialisation | Deterministic UTF-8 length-prefix serialiser with fixed fields, ordinal/null-first order, duplicate rejection and versioned domains. |
| Golden vectors | Executable JSON vectors for generation-bound `sourceBindingSetDigest` and observation-inclusive `activationBindingSetDigest`. |
| Three pre-CAS validations | Application independently recomputes active-document, source-binding and activation-binding digests, then verifies manifest identity, runtime compatibility, lineage and observation relations. |
| Activation invariants | One complete canonical activation authority; candidate staging is not queryable; only validated final manifests can become candidates for activation. |
| Observation-only rebinding | New record revision changes only permitted activation fields and preserves generation-bound identity and activation time. |
| Retention and rollback | Active plus one distinct rollback generation protect reachable raw-content identities; rollback constructs a new revision from explicitly selected observations and is rejected when current eligibility or coverage fails. |
| Deterministic fixtures | Non-production catalogue fixture with exactly 51 products, 9 categories and 54 assignments, plus the two-domain digest fixture. |

## Layering evidence

- `RagChallenge.Domain` contains provider-neutral logical values, invariant
  records and canonical digest calculation, including canonical generation
  specification, logical-artefact and complete-manifest identity.
- `RagChallenge.Application` owns the persistence ports, explicit mutation
  outcomes, activation construction and pre-CAS policy validation.
- `RagChallenge.Infrastructure` implements EF Core/SQLite contexts,
  migrations, control/vector/content stores, cleanup and recovery.
- Architecture tests continue to enforce inward project references and
  prohibit EF Core, SQLite and outer providers in Domain and Application.
- A compiler-generated namespace violation discovered during B5 was corrected
  in `8b3a6ac`; the targeted architecture suite then passed 10/10.

## S03-A security and data-protection review

- Inputs remain bounded typed metadata; no document body, prompt, answer,
  secret, credential or connection string is present in the model or fixtures.
- Local and official provenance cannot be silently interchanged.
- An official activation binding requires immutable registration, snapshot and
  observation identities.
- Observation registration/snapshot mismatches, missing observations and loss
  of per-database eligible coverage fail closed before a future CAS.
- Canonical digest domains are distinct, versioned and unambiguous for null and
  record boundaries.
- No network, provider, account, official source or real corpus was accessed.

## S03-A verification evidence

All commands were run locally from the repository root on 2026-08-02. The
final evidence table is populated only with observed results. The environment
used .NET SDK `10.0.302`, Node.js `24.18.1` and npm `11.16.0`. The repository
pins remain unchanged at Node.js `24.18.0` and npm `11.16.0`; the owner
explicitly accepted Node.js `24.18.1` only as a local verification variance
after it was observed as the installed winget-managed LTS version.

| Check | Result |
|---|---|
| Runtime preflight | Passed: 0 owned processes, 0 owned listeners, nothing stopped. |
| Release build without restore | Passed with 0 warnings and 0 errors. |
| .NET tests without restore | Passed: 68 total (53 unit, 10 architecture, 5 integration), 0 failed or skipped. |
| Coverage enforcement without restore | Passed: 95.55% lines (1051/1100) and 89.93% branches (268/298), above the 70%/45% floors. |
| Format verification | Passed with no changes required. |
| Dashboard lint, typecheck, tests and build using the existing installation | Passed under the explicitly accepted Node.js `24.18.1` verification variance; 2 tests passed and Vite produced the ignored local build output. |
| Repository audit | Passed for 104 non-ignored files. |
| Tracked diff hygiene | Passed. No package declaration, project file or lockfile changed. |
| `eng/ci.ps1 -Offline` aggregate | Not run: it necessarily performs .NET restore and `npm ci`; restore/installation is explicitly blocked with S03-B. Every authorised component was instead run without restore or installation. |

The successful tests establish deterministic in-memory model behaviour. They
do not establish database transaction atomicity, durable readback, restart,
migration or recovery behaviour because those capabilities are outside S03-A.

## S03-B supply chain and closure reconciliation

S03-B0 was repeated before repository changes against the exact authorised
package set. The conservative verification set contained 42 nupkgs. All 42
matched the SHA-512 value in the NuGet catalogue, passed repository-signature
verification with X.509 revocation kept offline, had no applicable advisory,
and had an identified MIT or Apache-2.0 licence. The current NuGet repository
certificate index declared `allRepositorySigned=true` and contained one
certificate valid at verification time.

The `SQLitePCLRaw.lib.e_sqlite3 2.1.12` package contained
`runtimes/linux-arm64/native/libe_sqlite3.so` as a 1,534,296-byte ELF64
little-endian AArch64 asset. Its SHA-256 was
`707fff6b18c1f083158e7a543c8d2545d5485f547ffd96db29679a95f52878d5` and
its embedded SQLite version was `3.53.3`, with source ID
`2026-06-26 20:14:12 d4c0e51e4aeb96955b99185ab9cde75c339e2c29c3f3f12428d364a10d782c62`.

The first real `net10.0` restore then materialised 40 project packages and the
separate local tool `dotnet-ef 10.0.10`, for 41 materialised items.
`System.Memory 4.5.3` was present only in the `.NETStandard2.0` dependency
group of `SQLitePCLRaw.core 2.1.12`; NuGet did not include it in the
`net10.0` `project.assets.json`. Execution stopped before migrations or stores
because this differed from the conservative count.

The owner subsequently accepted the distinction between the 42 verified
nupkgs and the 41 materialised items. `System.Memory 4.5.3` remains verified
conservative supply-chain evidence and must not be pinned, directly
referenced, or forcibly restored. The approved S03-B1 resumption starts from
`main@381d1cd297580476e461a242ce5b66c4884e521b` and the preserved interrupted
working tree. The accepted limitations remain: X.509 revocation was checked
offline, and no cryptographic source-to-nupkg link or reproducible build was
proved.

## Implemented S03-B1 to S03-B4 result

S03-B1 completed a locked restore with the accepted graph: 40 project packages
for `net10.0` plus local tool `dotnet-ef 10.0.10`. `System.Memory` was absent.
Only the four expected dependent lockfiles changed. The dependency declaration
and lockfile increment is `e12fff2`.

S03-B2 added separate EF Core models and initial migrations for `control.db`
and `vectors.db` in `2a2e7e0`. `control.db` contains the authoritative corpus,
catalogue revisions, immutable document/content identities, official-source
registrations and snapshots, observation journal, final manifests, activation
history and head, retention, operations, audit and maintenance leases.
`vectors.db` contains only rebuildable candidate builds and chunks; it has no
activation or retention authority. Foreign keys, check constraints, partial
unique indexes, WAL, `synchronous=FULL`, `foreign_keys=ON`,
`trusted_schema=OFF` and a bounded busy timeout are configured.

S03-B3 added Application-owned persistence ports and local Infrastructure
implementations in `43a4627`. Catalogue and observation writes use expected
revisions. Activation uses an immediate SQLite transaction, expected-record
CAS, the three independent binding digest checks, explicit observation
relations, full vector/content readback, durable history and audit. Retention
has one `Active`, at most one `Previous`, a minimum 14-day previous window and
expired `Hold` rows removable only through a leased, audited manual cleanup.
Rollback creates a new activation revision and can target only the retained,
currently eligible `Previous` generation.

The immutable content store publishes same-volume quarantine files atomically
under lower-case SHA-256 paths, never overwrites, bounds writes, rejects
reparse traversal and verifies SHA-256 on every reopen. Recovery uses SQLite
online backup, copies content into a new isolated root, records per-file hashes
and lengths, verifies database integrity/foreign keys, active authority links,
canonical vector readback and absence of active-authority tables in
`vectors.db`. Recovery and cleanup are leased and audited.

Cross-cutting review found that the initial vector API accepted a caller-made
generation identity. Commits `b0d9325` and `7a7b545` corrected and tested this:
finalisation now computes versioned canonical `generationSpecDigest`, ordered
logical artefact digest, complete manifest digest and `IndexGenerationId` from
SQLite readback. Same inputs finalise idempotently; text, vector or specification
changes produce a different identity. Manifest commit, CAS and recovery reject
canonical readback divergence.

S03-B4 is covered by deterministic temporary-store tests in `e3a079a` and
`7a7b545`. Observed scenarios include schema authority separation, constraints,
content idempotence and corruption, candidate isolation, exact search,
three-digest failure, missing and current observations, concurrent CAS with one
winner, bounded retention, replacement, rollback by new revision, expired-hold
cleanup, audit, isolated recovery and corruption detection. A synthetic
10,000-chunk by 1,536-dimension fixture completed functional write,
finalisation and exact-search readback; it is not performance homologation or a
product ceiling.

## S03-B5 stop and authorised resumption

S03-B5 began from clean `main@8b3a6ac8ddf0fdd92995fe73db32b56f81ae1036`.
Runtime preflight found no RAG-Challenge-owned process. Locked restore passed,
reconfirmed 40 materialised project packages, no `System.Memory`, bundle
`SQLitePCLRaw.bundle_e_sqlite3 2.1.12` and EF Core SQLite `10.0.10`, with no
tracked change. The current NuGet vulnerability query reported no vulnerable
package in any project. Format verification and Release build passed with zero
warnings and errors.

The first aggregate test attempt passed 56 unit tests and 16 integration tests
but failed one of 10 architecture tests because a C# collection expression
emitted a helper type outside the owning root namespace. The source form was
replaced without semantic change; Release build and the targeted architecture
suite then passed with 10/10. The full aggregate was not repeated after the
later migration stop, so these component results do not constitute the
Automatic Quality Gate.

The temporary migration sequence then diverged:

- `dotnet-ef migrations list` reported no migrations for
  `ControlPlaneDbContext`, although the tracked control migration, designer
  metadata and model snapshot exist;
- the Vector context discovered `20260802171400_InitialVectorStore`, and its
  apply, rollback-to-zero and reapply operations succeeded in the temporary
  store;
- Control database update reported no migrations and created no authoritative
  schema;
- `has-pending-model-changes` for Control returned exit code 1;
- Vector pending-model verification and the remaining B5 checks were not run;
- the exact temporary directory, containing only the two non-production DB
  files, was verified under the system temporary root and removed.

This was a mandatory migration stop. No repair was attempted in that stopped
execution and no result was inferred from it.

The owner subsequently authorised immediate resumption in the current
conversation with `Pode fazer agora?`, after the intended S03-B scope and
runtime preflight had been stated. The resumed execution started from clean
`main@c72c8b967667f72e8971f4887174585d3640a36e`; runtime preflight again found
zero RAG-Challenge-owned processes and zero owned listeners.

A fresh clean and Release build made both tracked migrations discoverable.
The complete temporary sequence then passed independently for Control and
Vector: list, apply, rollback to zero, reapply and
`has-pending-model-changes`. Control discovered
`20260802171743_InitialControlPlane`; Vector discovered
`20260802171400_InitialVectorStore`; neither context had a pending model
change. The resulting non-production files were 380,928-byte `control.db` and
49,152-byte `vectors.db`; their validated temporary directory was removed.

The earlier divergence did not reproduce after the clean rebuild. The
observed evidence is consistent with stale incremental Release output used by
`--no-build`; it does not prove a deeper historical root cause. No migration,
snapshot, model or persistence contract required correction.

The first resumed `eng/ci.ps1 -Offline` run passed restore, format, Release
build, all 82 .NET tests, coverage and all Dashboard checks, then stopped when
the repository audit detected that Windows NuGet restore had rewritten the
seven tracked `packages.lock.json` files with CRLF. The CI entry point now
normalises only those tracked generated lockfiles to UTF-8/LF after locked
restore and reports every affected path. The repeated aggregate passed with
82/82 .NET tests, 94.83% line coverage (8,481/8,943), 72.34% branch coverage
(688/951), two Dashboard tests, lint, typecheck, Vite build, repository audit
for 130 non-ignored files and diff hygiene. A current NuGet vulnerability
query reported no vulnerable direct or transitive package in any project.
Node.js `24.18.1` remained the previously accepted local verification
variance; repository pins remain `24.18.0` and npm `11.16.0`.

## Deferred and blocked work

S03-B has no remaining implementation item. Automatic Quality Gate, Human
Gate, `STATE-03` closure and entry into `STATE-04` remain separate and
unauthorised. Provider calls, accounts, real product corpus, official-source
synchronisation, operational storage, GitHub or OCI mutation, publication,
deployment and DB-Notifier integration also remain prohibited.

## Risks and limitations

| Item | Current disposition |
|---|---|
| Physical enforcement of logical uniqueness and relationships | Model, DDL and both fresh migration cycles passed locally in non-production temporary stores. |
| Atomic current-record CAS, audit and history | Temporary integration tests passed, including concurrent one-winner CAS; process-crash injection was not performed. |
| Durable immutable content, readback and cleanup | Temporary tests passed for publication, reopen hash, reachability, cleanup and corruption; no operational store or real corpus was used. |
| Migration and recovery verification | Fresh Control and Vector apply/rollback/reapply and pending-model checks passed; isolated recovery tests passed. The earlier stale-output inference remains unproven. |
| Canonical manifest/spec/artifact serialisers | Implemented and tested from ordered logical SQLite readback; no adapter/provider output was used. |
| Real catalogue documents and licence evidence | No real corpus was used; selection and ingestion remain future authorised work. |

## Rollback of this local increment

There is no operational data or deployment to roll back. The exact temporary
migration store was removed. If the owner rejects the S03-B implementation,
its focused local commits can be reverted through ordinary forward Git reverts
in reverse dependency order; append-only lifecycle evidence must remain.

## Lifecycle conclusion

S03-A and S03-B0 through S03-B5 are implemented and verified within
`STATE-03`. This completes the authorised S03-B increment but does not close
the state. Automatic Quality Gate, Human Gate and `STATE-04` remain pending
and unauthorised.
