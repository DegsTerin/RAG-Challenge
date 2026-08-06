# STATE-06 OCI Readiness and Linux ARM64 Rehearsal

## Purpose and authority

This document defines the bounded `STATE-06` readiness plan and offline Linux
ARM64 cross-publish rehearsal for `RagChallenge.Server.Api` with the compiled
Dashboard. It implements no OCI resource, tenancy policy, secret, public
endpoint, service installation, publication or deployment. It is subordinate
to accepted ADR-0005, the current factual state and the explicit
`AUTH-S06-DEP-001` and `AUTH-S06-CORR-001` envelopes.

The rehearsal is static Windows-hosted evidence for a self-contained
`linux-arm64` payload. It is not evidence that the payload ran on Linux, that
an OCI shape is available, or that the application is production-ready.

## Evidence classification

| Level | Current status | Meaning |
| --- | --- | --- |
| Architecture decision | Accepted | ADR-0005 conditionally selects one self-contained Linux ARM64 API/Dashboard deployment. |
| Runtime-pack supply chain | Verified locally | The three authorised .NET/ASP.NET Core `10.0.10` packs passed identity, catalogue SHA-512, signature, licence, advisory and resolver-closure checks. |
| Locked `linux-arm64` restore | Verified locally | The four production lockfiles record the approved RID and the locked restore uses only the verified local source and isolated cache. |
| Rehearsal implementation | Implemented | Repository-owned build and verification scripts are bounded to ignored local output and contain no OCI operation. |
| Rehearsal reproduction | Pending corrective verification | C4 must produce two byte-identical archives and pass manifest, configuration and AArch64 checks on the final correction baseline. |
| Linux ARM64 execution | Not tested | No ARM64 executable is run on the Windows rehearsal host. |
| OCI verification or deployment | Not authorised or tested | No OCI account, endpoint, capacity, IAM, network, storage, cost or runtime evidence exists in this increment. |

## Conditional deployment target

The planning target remains `sa-saopaulo-1`, `VM.Standard.A1.Flex`, one OCPU
and 6 GiB memory. These values are conditional planning inputs, not a capacity,
entitlement, availability or zero-cost claim. Before any future deployment,
the owner must separately authorise and verify tenancy limits, shape
availability, subscription, billing, IAM, egress and recovery controls.

The candidate release comprises one self-contained `linux-arm64` build of the
API with the compiled Dashboard under the same origin. The rehearsal does not
create an image and does not select a container or instance provisioning
mechanism.

## Offline rehearsal

### Prerequisites

- clean authorised repository baseline;
- installed .NET SDK `10.0.302` and the already installed Dashboard toolchain;
- `net10.0/linux-arm64` restored in the four production project assets from
  the verified local source;
- the exact three runtime packs at version `10.0.10` in the isolated task
  cache; and
- no RAG-Challenge process or listener that could interfere with the build.

The builder fails before the Dashboard or server build if the RID target or
the exact implicit runtime-pack matrix is absent. It invokes `dotnet publish`
with `--runtime linux-arm64`, `--self-contained true` and `--no-restore`.
Output remains under `artifacts-local/s06-oci-rehearsal/`.

From the repository root, the corrective verification commands are:

```powershell
./src/RagChallenge.Server.Api/Build-OciRehearsalArtifact.ps1
./src/RagChallenge.Server.Api/Test-OciRehearsalArtifact.ps1
```

The builder creates a deterministic ZIP, a sorted SHA-256/byte-size manifest
and a separate archive digest. The verifier reads the archive back, checks
every manifest record, rejects unsafe paths and Windows runtime payloads,
requires the compiled Dashboard, and validates every native payload as ELF64
little-endian AArch64. It explicitly records that the ARM64 app host was not
executed and that OCI was not contacted.

## Planned host and storage boundary

The future host must use a dedicated unprivileged operating-system account
`<service-account>`. Release files belong under a read-only
`<release-root>/<release-id>` directory. Runtime state belongs on one dedicated
durable volume rooted at `<store-root>`:

```text
<store-root>/
├── control.db
├── vectors.db
├── content/
└── temporary/
```

SQLite databases, immutable content, vectors and temporary files used for
atomic replacement must remain on that volume. The service account receives
only the file permissions required for the release and store roots. It must
not run as root, own the reverse proxy, or receive interactive cloud
administration credentials.

Kestrel binds only to `http://127.0.0.1:<kestrel-port>`. A separately approved
minimal reverse proxy terminates TLS and forwards only the public query,
health and Dashboard surface. Administration stays outside public HTTP. The
host firewall admits public HTTPS only, plus any separately approved and
source-restricted administration channel.

## Configuration and egress

Committed configuration remains fail-closed: external services,
administration and the synthetic Integration runtime are disabled by default.
Future production configuration must persist only typed non-secret values and
opaque references such as `<secret-reference>`. Secret values must come from
an approved secret store and must never enter a release archive, command
history, log, screenshot or repository file.

All egress profiles remain empty unless separately authorised. Provider and
official-source configuration does not itself grant egress. A future enablement
must use exact allowlists, bounded responses, redirects disabled, TLS and SSRF
controls, timeouts, cancellation, rate limits and sanitised logging.

## Planned operating procedure

The following is a future runbook with placeholders. It was not executed by
this rehearsal.

1. Verify the release digest, manifest, ownership and mode under
   `<release-root>/<release-id>` before it becomes selectable.
2. Verify that `<store-root>` is mounted from the intended durable volume and
   that the service account cannot traverse outside the configured roots.
3. Resolve approved secret references without printing their values, retain
   deny-by-default egress, and bind Kestrel to loopback.
4. Start `<service-name>` through `<service-manager>` as `<service-account>`.
5. Check liveness and readiness only at
   `http://127.0.0.1:<kestrel-port>/api/v1/health/live` and
   `http://127.0.0.1:<kestrel-port>/api/v1/health/ready`; do not expose a
   release until readiness identifies the expected configuration and active
   generation.
6. Stop by removing the release from the reverse proxy, allowing bounded
   in-flight work to drain, and requesting a managed service stop. Escalate to
   a forced stop only after the bounded shutdown interval and record it.
7. Restart only after store paths, free space, permissions and the last active
   generation are rechecked. Readiness must reopen that generation before
   traffic returns.
8. For application rollback, stop traffic and the service, select the prior
   verified release, preserve the same compatible durable store, restart and
   recheck readiness. Generation rollback is a separate audited activation
   operation; never replace SQLite or content files by hand.

If a release changes schema or storage compatibility, this procedure is
insufficient and must stop for separately authorised migration and recovery
evidence.

## Conditional backup and recovery set

The future independent target remains regional OCI Block Volume Backup, with
daily backups plus one before a release or migration, 14-day retention, an RPO
target of 24 hours and a restore objective of 8 hours. These are unverified
planning targets, not an SLA.

An application-consistent recovery set must be created under an administration
lease: stop catalogue/index mutations, complete or abandon candidates,
checkpoint SQLite WAL files, record the active revision and manifest/content
digests, and snapshot the control database, vector database and content volume
together. A copy on the same volume is not a recovery backup.

A restore must target an isolated path first. It must pass SQLite integrity,
content-hash, manifest/binding reachability and read-only query checks before
it can be declared recoverable. No backup, snapshot or restore was executed in
`S06-CORR-01`.

## Stop conditions and residual risk

Stop a future rehearsal or deployment when identity, version, archive digest,
manifest, native architecture, configuration, path containment or expected
payload differs; when a network, OCI or secret action lacks authority; or when
the active generation cannot be reopened.

Remaining unverified areas include Linux execution, filesystem permissions
after Linux extraction, reverse-proxy and TLS behaviour, system service
hardening, OCI tenancy/IAM/capacity/billing, real egress, operational storage,
backup consistency, restore time, performance, real corpus and provider
behaviour. Static cross-publishing reduces packaging uncertainty but proves
none of those areas.

## Rollback

The repository rollback is a focused revert of the rehearsal document,
scripts, tests and four RID lockfile additions. Ignored task cache and rehearsal
output are non-authoritative local artefacts and may be removed only from their
validated task-owned paths under explicit cleanup authority. No external state
exists to roll back.
