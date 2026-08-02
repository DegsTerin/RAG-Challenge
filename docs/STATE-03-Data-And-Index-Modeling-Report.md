# STATE-03 Data and Index Modeling Report

## Report status

- Lifecycle state: `STATE-03 DATA_AND_INDEX_MODELING` active.
- Executed increment: `S03-A` complete; `S03-B0` complete; `S03-B1` resumed
  after an owner-approved closure reconciliation.
- Entry baseline: `main@35b67c194f6ea2459833420b8bc2143fadfe75df`.
- Instruction corpus: `4.9.1`.
- Entry authority recorded locally: commit `5efaa37`.
- S03-A implementation recorded locally: commit `ace780a`.
- Report date: 2026-08-02.
- Automatic Quality Gate: pending and not inferred from the checks below.
- Human Gate: pending.
- State closure and entry into `STATE-04`: not authorised.

This is a factual partial-state execution report. It does not claim that
`STATE-03` is complete. S03-B is authorised only within its recorded local,
sequential envelope and its physical persistence work is not yet complete.

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

- `RagChallenge.Domain` contains pure logical values, invariant-bearing records
  and canonical digest calculation.
- `RagChallenge.Application` contains activation-record construction and
  pre-CAS policy validation.
- `RagChallenge.Infrastructure` was not changed.
- Existing architecture tests confirm inward project references and prohibit
  outer framework/provider dependencies in Domain and Application.
- No `PackageReference`, central package version, lockfile, migration or store
  was added or changed.

## Security and data-protection review

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

## Verification evidence

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

## Deferred and blocked work

S03-B1 is authorised to complete only after a locked restore confirms the
accepted 40-package project graph and the four genuinely affected lockfiles.
S03-B2 through S03-B5 remain sequenced behind that check and have not started.

The following also remain prohibited or unperformed: network access, provider
calls, accounts, real product corpus, official-source synchronisation,
operational storage, GitHub or OCI mutation, publication, deployment,
DB-Notifier integration, `STATE-04` entry, Automatic Quality Gate, Human Gate
and `STATE-03` closure.

## Risks and limitations

| Item | Current disposition |
|---|---|
| Physical enforcement of logical uniqueness and relationships | Authorised for sequenced S03-B work; no DDL exists yet. |
| Atomic current-record CAS, audit and history | Application preconditions are modelled; transaction implementation and crash evidence have not yet been produced. |
| Durable immutable content, readback and cleanup | Reachability is modelled; no content store exists. |
| Migration and recovery verification | Not tested and not authorised in S03-A. |
| Canonical manifest/spec/artifact serialisers beyond the two binding domains | Their typed digests and identity constraints are modelled; physical finalisation remains future work. |
| Real catalogue documents and licence evidence | No real corpus was used; selection and ingestion remain future authorised work. |

## Rollback of this local increment

There is no operational data or deployment to roll back. If the owner rejects
the S03-A implementation before later persistence depends on it, the focused
local implementation commit can be reverted with an ordinary forward Git
revert. The append-only lifecycle entry remains historical evidence and must
not be silently deleted or rewritten.

## Lifecycle conclusion

S03-A is implemented and verified within `STATE-03`; S03-B0 is approved and
the package-resolution distinction is explicitly accepted. S03-B1 through
S03-B5 still need to complete sequentially. This does not close the state. A
future Automatic Quality Gate must assess the complete authorised STATE-03
deliverables and factual report before a separate complete Human Gate summary
can request closure.
