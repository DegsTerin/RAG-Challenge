# STATE-06 Automatic Quality Gate Corrective Proposal

## Purpose and authority

This document proposes one bounded corrective increment, `S06-CORR-01`, for
`AQG-S06-001`, `AQG-S06-002` and `AQG-S06-003`. It identifies the intended
changes, dependencies, checks, risks, rollback and exact future authority
boundaries. It does not implement a correction, accept a dependency, change a
normative decision, repeat the Automatic Quality Gate, conduct a Human Gate or
authorise an external action.

The proposal is subordinate to [`AGENTS.md`](../AGENTS.md), the current factual
state, the accepted ADRs and the governance corpus. A future approval of this
proposal must name the approved authority envelopes. Approval of one envelope
must not be inferred as approval of another.

## Confirmed baseline

The local, read-only inspection on 2026-08-06 confirmed:

| Fact | Observed value |
| --- | --- |
| Repository branch | `main` |
| Current `HEAD` | `547cbb8f7ac423293896ed9c69a3af6486e8cd33` |
| Gate execution baseline | `a6f0480b7f229b63c5ac24d65e61f55de1c6483a` |
| Prompt corpus | `4.9.2` |
| Working tree before this proposal | clean |
| Lifecycle | `STATE-06 INTEGRATION` active |
| Automatic Quality Gate | `REPROVADO` |
| Open findings | `AQG-S06-001`, `AQG-S06-002`, `AQG-S06-003`; all P2 |

The gate evidence is recorded in
[`STATE-06-Integration-Report.md`](STATE-06-Integration-Report.md). The
current state and append-only history agree with that report. No concurrent or
unrelated change was present before this proposal was written.

Runtime preflight was `NOT_APPLICABLE` to preparation of this proposal because
the work was documentation and read-only analysis. No process or listener was
inspected or stopped.

## Corrective outcome

`S06-CORR-01` should produce the following outcome without broadening product
scope:

1. a state-owned OCI readiness plan and an offline, non-production Linux ARM64
   packaging rehearsal, with no OCI contact;
2. deterministic composition-level evidence for cancellation and bounded
   failures, followed by continued query service and restart from the same
   active generation; and
3. one explicit normative ownership rule for README examples, followed by a
   factually current README containing only verified local/synthetic examples.

Successful corrective checks should place the findings in
`CORRECTED_PENDING_GATE_RETEST`. Only a separately authorised, complete
Automatic Quality Gate may dispose them as `RESOLVED` or approve `STATE-06`.

## Classification by finding

| Finding | Correction available offline | Additional decision or authority |
| --- | --- | --- |
| `AQG-S06-001` | Readiness plan, rehearsal scripts, manifest and static checks can be authored offline. | The literal self-contained Linux ARM64 rehearsal requires an exact runtime-pack intake and locked restore because the required packs are absent locally. No OCI authority is required. |
| `AQG-S06-002` | All implementation and verification can use the existing solution, synthetic providers, fake loopback HTTP and temporary stores. | Bounded corrective authority to change internal composition seams and integration tests. No dependency, network or contract authority is required. |
| `AQG-S06-003` | README and roadmap edits can be made offline after ownership is decided. | Explicit normative decision `NORM-S06-001` is required before editing Lifecycle or the roadmap. No ADR is required if the accepted architecture and lifecycle order are preserved. |

## Required invariants and negative scope

Every future corrective envelope must preserve these invariants:

- except for exact package-source access explicitly approved under
  `AUTH-S06-DEP-001`, no external network; and no OCI, provider, account,
  secret, real corpus, real official source, GitHub, publication, deployment
  or DB-Notifier action under any corrective envelope;
- no public API, OpenAPI, Domain or Application contract change;
- no new product capability, state transition, Human Gate or `STATE-07` work;
- no weakening of tests, coverage floors, security controls or the
  higher-precedence Lifecycle requirement;
- no production fault switch, environment option or public route for test
  injection;
- no claim that Windows cross-publishing proves Linux ARM64 execution or OCI
  compatibility; and
- no claim that a synthetic example is real corpus, real provider or
  production evidence.

Any unexpected package, lockfile, schema, migration, public contract or
external-action requirement is a stop condition.

## AQG-S06-001 — OCI readiness and Linux ARM64 rehearsal

### Observed gap

ADR-0005 conditionally selects a self-contained Linux ARM64 deployment for
`RagChallenge.Server.Api` with the compiled Dashboard. Lifecycle assigns an
OCI plan and non-production rehearsal to `STATE-06`. The repository currently
has neither a state-owned readiness/rehearsal document nor a Linux ARM64
candidate artefact.

Read-only inspection found only the `net10.0` target in
`src/RagChallenge.Server.Api/obj/project.assets.json`. No tracked lockfile
contains a Linux ARM64 runtime pack. The installed .NET SDK `10.0.302` reports
the following expected pack identities for the selected target at runtime
version `10.0.10`:

- `Microsoft.NETCore.App.Runtime.linux-arm64`;
- `Microsoft.AspNetCore.App.Runtime.linux-arm64`; and
- `Microsoft.NETCore.App.Host.linux-arm64`.

None of those three package identities is present in the current global NuGet
cache. Their hashes, signatures, licences and actual resolver closure were not
verified by this proposal and must not be inferred. The already approved
`SQLitePCLRaw.lib.e_sqlite3` `2.1.12` graph contains the Linux ARM64 SQLite
native asset, but that does not supply the .NET or ASP.NET Core runtime packs.

### Proposed changes

The correction should add:

- `docs/STATE-06-OCI-Readiness-And-Rehearsal.md`, containing:
  - the conditionally accepted `sa-saopaulo-1` / `VM.Standard.A1.Flex`
    planning target;
  - the distinction among decided, implemented, verified locally, rehearsed
    for Linux ARM64, verified in OCI and deployed;
  - an unprivileged service account, Kestrel loopback binding, reverse-proxy
    boundary, durable volume layout, same-volume temporary writes and
    fail-closed egress/configuration model;
  - startup, readiness, stop, restart and rollback procedures using only
    placeholders such as `<host>`, `<store-root>` and `<secret-reference>`;
  - the conditional backup/recovery-set design without claiming a backup or
    OCI restore was executed; and
  - explicit limitations for tenancy, IAM, capacity, cost, Linux execution,
    TLS and operational storage.
- `src/RagChallenge.Server.Api/Build-OciRehearsalArtifact.ps1`, separate from
  the accepted `S06-A` artefact builder, which should:
  - require an already restored `net10.0/linux-arm64` target and fail before
    build when it is absent;
  - publish `linux-arm64`, self-contained and Release with `--no-restore`;
  - include the compiled Dashboard;
  - write a sorted file/hash/size manifest and a deterministic local archive;
  - keep output under a validated ignored path such as
    `artifacts-local/s06-oci-rehearsal/`; and
  - perform no network, installation, OCI or service mutation.
- `src/RagChallenge.Server.Api/Test-OciRehearsalArtifact.ps1`, which should:
  - validate archive and manifest integrity;
  - validate ELF64 little-endian AArch64 identity for the app host and the
    SQLite native library;
  - reject Windows-native or unexpected runtime payloads;
  - scan committed configuration and the archive for apparent secrets and
    unsafe absolute paths;
  - confirm that external capabilities remain disabled by default; and
  - state that the ARM64 binary was not executed on Windows.
- focused structural tests in
  `tests/RagChallenge.IntegrationTests/SetupHostArtefactTests.cs` for script
  containment, no-restore behaviour, required manifest checks, disabled
  external configuration and absence of an OCI command/API call.

The existing `Build-IntegrationArtifact.ps1` and its accepted Windows artefact
must remain intact unless a shared helper is proven byte-for-byte neutral to
that artefact.

### Dependency authority

`AUTH-S06-DEP-001` is required before any restore or runtime-pack intake. It
must name:

- `linux-arm64` as the only new restore target;
- the exact resolved pack identities and versions;
- the permitted source: either a verified owner-supplied offline source or an
  exact, separately authorised NuGet HTTPS boundary;
- an isolated task cache and no mutation of the global cache;
- SHA-512 catalogue comparison, repository-signature verification,
  licence/advisory review and complete resolver closure;
- locked restore and the exact lockfiles allowed to change, if any; and
- a stop on any package, version, source, redirect, target or lockfile outside
  the approved matrix.

The three identities above are expected candidates, not an approved closure.
If the actual resolver requires a different identity or version, dependency
authority must be revised before intake. A framework-dependent build is not a
silent fallback because ADR-0005 currently selects self-contained deployment;
changing that choice would require a separate architectural decision.

### Acceptance checks

After dependency intake is approved and completed, the corrective increment
should require:

1. locked restore into an isolated cache and a resulting
   `net10.0/linux-arm64` target;
2. two consecutive no-restore rehearsal builds from the same clean commit
   with identical archive hash, byte count, file count and manifest;
3. complete manifest readback and no apparent secret;
4. ELF AArch64 checks for the app host and SQLite library;
5. Dashboard payload and fail-closed Integration configuration present;
6. no OCI CLI, OCI endpoint, provider, official source or non-loopback
   request; and
7. a report that distinguishes static cross-publish evidence from untested
   Linux/OCI runtime behaviour.

### Risks and rollback

- Runtime-pack supply-chain drift is controlled by exact identity, version,
  hash, signature, source and lockfile gates.
- Cross-publish success can be overclaimed; the report must label Linux
  execution and OCI as untested.
- A shared artefact helper could change the accepted `S06-A` ZIP; preserve the
  current builder or prove neutrality before reuse.
- Rollback is a focused revert of the OCI plan/scripts/tests. Any task cache
  and ignored rehearsal output remain non-authoritative local artefacts and
  may be removed only from their validated task-owned paths under the future
  cleanup authority. No external state exists to roll back.

## AQG-S06-002 — composition-level resilience and cancellation

### Observed gap

`IntegrationHostEndToEndTests` proves only successful requests and restart.
`OfficialSourceLoopbackTests` proves only successful fake synchronisation.
Lower layers cover cancellation, provider failures, persistence faults,
idempotency and concurrency, but the composed `STATE-06` boundary does not
prove that one active generation remains serviceable and restartable after an
in-flight cancellation or a bounded provider/synchronisation failure.

### Proposed changes

The smallest defensible correction should change only internal integration
composition and tests:

- `src/RagChallenge.Server.Api/OperationsGovernance/IntegrationRuntime.cs`:
  add an internal constructor/factory seam for deterministic integration
  embedding and language-model adapters. Production composition must retain
  the current private deterministic defaults. The seam must remain internal,
  unavailable through configuration, environment variables, HTTP or public
  contracts.
- `tests/RagChallenge.IntegrationTests/IntegrationHostEndToEndTests.cs`:
  add deterministic cases that:
  1. establish and record active generation `G` through a successful request;
  2. arm a test language-model adapter that waits on a handshake and observes
     cancellation without timing sleeps;
  3. cancel an in-flight HTTP request and prove the cancellation token reached
     the adapter;
  4. arm a one-shot `ProviderStageUnavailableException("generation", ...)`
     and assert sanitised `CH_LANGUAGE_MODEL_UNAVAILABLE` behaviour;
  5. issue a later successful query and assert generation `G`; and
  6. restart the host on the same store, query successfully and assert
     generation `G` again.
- `tests/RagChallenge.IntegrationTests/OfficialSourceLoopbackTests.cs`:
  add one bounded fake-server/transport failure after an active generation
  exists. Assert that no new snapshot, observation, activation revision or
  binding digest is committed and that the previously active generation
  remains queryable. The failure must use loopback only and deterministic
  handshakes, not wall-clock races.

No production fault setting, public contract or external provider adapter is
needed. If a proposed test cannot reach the actual composed query service and
instead only stubs the whole `IQuestionAnsweringService`, it is insufficient
for this finding and must stop for redesign.

### Corrective authority and checks

`AUTH-S06-CORR-001` may authorise these internal code and test changes using
the existing locked dependency graph and already available local
installation/cache. It needs no dependency or external authority.

Required checks are:

- focused host cancellation/provider-failure tests;
- focused fake official-source failure test;
- the pre-existing successful E2E/restart and fake-loopback tests;
- the complete .NET test suite with no failure or skip;
- line and branch coverage above the repository floors;
- format and Release build with no warning/error; and
- architecture/contract assertions proving no public/configuration fault
  surface and no external access.

### Risks and rollback

- Cancellation tests can be flaky if based on delay; use explicit entered,
  cancel-observed and release handshakes with bounded timeouts.
- A test hook can become a production capability; keep it internal and
  construct it only through the existing test-visible composition boundary.
- A failed request must not be accepted as proof of integrity without the
  subsequent query and restart assertions against generation `G`.
- Rollback is a focused revert of the internal seam and the new tests. Stores
  are temporary and no migration or external state is involved.

## AQG-S06-003 — normative ownership and README evidence

### Observed gap

Lifecycle requires `examples reais para o README` in `STATE-06`. The roadmap
places `README final com exemplos reais` in `S08-B` and `BL-M13`. The root
README is also factually stale: it presents `STATE-03`/`S03-A` as current and
states that migrations, persistent stores, parsers and a functional RAG flow
do not exist.

Lifecycle has higher precedence than the roadmap. Silently moving the
Lifecycle deliverable to `STATE-08` would weaken the failed gate and is not
recommended.

### Required normative decision

Before editing the owning documents, the owner must explicitly decide
`NORM-S06-001`:

> `STATE-06` owns a factually current README and at least one example whose
> command and result were actually verified against the local/synthetic
> integrated artefact. The README must label that boundary and must not imply
> a real corpus, provider, official source, Linux runtime, OCI deployment or
> production support. `STATE-08` owns the final public README and supplements
> or replaces those examples with separately verified OCI and real-product
> execution evidence.

This decision clarifies evidence ownership without changing architecture,
state order or the rule that evidence must precede a claim. It does not require
an ADR. If the owner rejects this decision and instead requires real corpus or
provider examples in `STATE-06`, the current correction cannot proceed without
new corpus/provider/external authorities and must stop.

### Proposed changes after the decision

Under `AUTH-S06-NORM-001`, update:

- `prompts/governance/Lifecycle.md` to replace the ambiguous `real examples`
  wording with the verified local/synthetic boundary above while retaining a
  mandatory `STATE-06` README deliverable;
- `docs/MVP-Roadmap-And-Backlog.md` so `S06-A` or a named corrective `S06`
  lot owns the current factual README and verified local/synthetic example,
  while `S08-B` and `BL-M13` own finalisation with public OCI/real-product
  evidence;
- `prompts/system/Prompt-System-Change-Log.md`,
  `prompts/state/Current-State.md` and the append-only
  `prompts/state/State-Transition-Log.md` for the resulting corpus patch and
  factual authority record.

Then, under `AUTH-S06-CORR-001`, update `README.md`, preserving its existing
`pt-BR`, to record the actual `STATE-06` position and only commands/results
reverified during the correction.

The expected corpus change is a `PATCH` because it resolves ownership and
ambiguity without adding a capability or moving a lifecycle requirement. The
actual version must be confirmed against the then-current baseline before
writing.

The README example should use the repository-owned local artefact build and
reproduction paths, state that the fixture/providers are synthetic and show a
sanitised result actually observed during the correction. It must not include
a stable archive hash unless that exact hash was reproduced on the final
corrective commit. It must not claim Linux ARM64 execution merely because the
rehearsal artefact passed static checks.

### Checks, risks and rollback

Required checks are:

- zero remaining active ownership conflict among Lifecycle, roadmap,
  `S08-B`, `BL-M13` and the README;
- exact corpus-version consistency and append-only history;
- README commands executed locally on the final correction baseline;
- README facts consistent with Current State and the `STATE-06` report;
- links, UTF-8/LF, final newline, whitespace and repository audit; and
- explicit separation of local/synthetic, Linux ARM64 rehearsal, OCI-verified
  and deployed states.

The main risk is retroactively weakening a gate. The normative wording must
keep the `STATE-06` deliverable and distinguish evidence levels rather than
defer all examples to `STATE-08`. Rollback is a focused revert of the
normative/README commits plus a compensating append-only history entry; it
must not rewrite the original failed-gate record.

## Consolidated execution order

The correction should be sequential because the normative baseline, package
graph, shared integration composition and factual records are common inputs.

1. **C0 — Reconfirm baseline and authority.** Require clean `main`, the
   expected corpus and the three open findings. Stop on concurrent change.
2. **C1 — Record `NORM-S06-001`.** Apply the accepted ownership rule and
   corpus patch before writing the README example.
3. **C2 — Close the ARM64 dependency gate.** Under `AUTH-S06-DEP-001`, verify
   and restore only the approved resolver closure into an isolated cache.
   Stop on any mismatch.
4. **C3 — Implement offline corrections.** Add the OCI plan/rehearsal, the
   resilience/cancellation evidence and the README update under
   `AUTH-S06-CORR-001`.
5. **C4 — Verify the correction.** Run focused checks, the complete existing
   .NET/npm checks, coverage, two ARM64 rehearsal builds, manifest/static
   checks, the verified README commands, security/hygiene and diff review.
6. **C5 — Record factual outcome.** Update the `STATE-06` report, Current
   State and append-only history as `CORRECTED_PENDING_GATE_RETEST`, preserving
   the original P2 findings and failed gate as history. Create focused local
   commits.
7. **Stop.** Do not repeat the Automatic Quality Gate. A complete gate restart
   requires `AUTH-S06-AQG-RETEST-001` on a separately confirmed clean
   baseline.

Suggested focused commits, subject to the authorised final diff, are:

1. `docs(governance): align state 06 readme ownership`;
2. `build(integration): add linux arm64 rehearsal`;
3. `test(integration): cover cancellation and bounded failures`;
4. `docs(readme): document verified state 06 flow`; and
5. `docs(state-06): record corrective evidence`.

No commit may combine an unapproved dependency graph with implementation.

## Exact future authority envelopes

### `AUTH-S06-NORM-001`

Authorises only decision `NORM-S06-001`, the Lifecycle/roadmap ownership
reconciliation, the corresponding prompt-corpus patch and its factual record.
It does not authorise code, tests, dependencies, README claims not covered by
the decision, a gate or an external action.

### `AUTH-S06-DEP-001`

Authorises only the exact Linux ARM64 runtime-pack evidence, isolated intake
and locked restore described above. It must name source access explicitly. It
does not authorise product implementation, OCI, deployment or substitution of
the accepted self-contained target.

### `AUTH-S06-CORR-001`

Authorises the bounded offline OCI plan/rehearsal scripts, internal test seam,
composition-level tests, README correction after `NORM-S06-001`, full
corrective checks, factual reconciliation and focused local commits. It does
not authorise a dependency outside the closure accepted by
`AUTH-S06-DEP-001` or repeat the Automatic Quality Gate.

### `AUTH-S06-AQG-RETEST-001`

May be requested only after all three findings are recorded as corrected on a
clean baseline. It authorises a complete local/offline Automatic Quality Gate
restart from the beginning, with no silent correction. It does not authorise
a Human Gate, OCI, an external action or `STATE-07`.

## Consolidated stop conditions

Stop before writing or continuing if any of these occurs:

- branch, commit, corpus, working tree or finding disposition diverges from
  the authorised baseline;
- the owner has not made `NORM-S06-001` before normative/README changes;
- a runtime-pack identity, version, hash, signature, source, target or lockfile
  differs from the approved dependency matrix;
- a restore, installation, network destination or external action is needed
  outside explicit authority;
- a public contract, OpenAPI, ADR, migration or schema change becomes
  necessary;
- the test seam is reachable from production configuration or HTTP;
- cancellation/failure evidence cannot prove a subsequent query and restart
  against the same generation;
- the ARM64 rehearsal cannot be reproduced twice or contains an unexpected
  native/runtime payload;
- README text would overstate synthetic, Windows, cross-publish or OCI
  evidence; or
- unrelated or concurrent work appears.

## Gate boundary

This proposal is complete when it identifies an executable correction and its
authorities. It does not change the current finding dispositions. The current
Automatic Quality Gate remains `REPROVADO`, `STATE-06` remains active and its
Human Gate remains premature.
