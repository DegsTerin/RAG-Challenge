# ADR-0019: Cross-Platform PDF Renderer Sandbox Boundary

## Status

`proposed`

## Date

2026-08-16

## Owners

- Product owner: RAG-Challenge owner
- Architecture owner: RAG-Challenge
- Technical owners: security, document rendering and runtime operations

## Preparation authority and baseline

- Preparation authority: `SEC-CORR-ADR-PREP-01`
- Prior read-only design authority: `SEC-CORR-DESIGN-01`
- Proposed permanent corrective identity: `SEC-CORR-002`
- Branch: `main`
- Commit: `334053e0101ce882767ccba29c69da7882917280`
- Prompt corpus before preparation: `4.17.1`
- Lifecycle position: `STATE-07 TESTING_HOMOLOGATION`; unchanged
- Runtime preflight: `NOT_APPLICABLE` for documentary preparation
- Protected OpenAPI v1 SHA-256:
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`
- Protected OpenAPI v2 SHA-256:
  `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733`

## Identity and authority boundary

`SEC-CORR-002` is reserved by this proposal for the cross-platform renderer
sandbox. It is independent from the existing `SEC-001` dependency-audit
finding and from `SEC-CORR-001`, the proposed provider-budget boundary.

This proposal neither changes the renderer nor claims that a sandbox is
currently implemented. It does not create a process, execute a PDF, add a
project or dependency, alter a schema or migration, modify a public contract,
or prove Windows, Linux ARM64 or OCI runtime behaviour. Acceptance would
establish architecture authority only; every executable change and platform
proof would require separate authority.

## Context

[ADR-0008](ADR-0008-Product-Corpus-Storage-And-Page-Image-Evidence.md)
classifies PDF as untrusted renderer input and requires bounded rendering with
no network access. [ADR-0012](ADR-0012-Notice-Bearing-Page-Image-Profile-And-Derivative-Obligation-Delivery.md)
adds deterministic notice-bearing composition while preserving the same
untrusted-input boundary. `THR-S02-037` in the
[threat model](../security/STATE-02-Threat-Model.md) remains open for renderer
exploitation, resource exhaustion and partial output.

The current implementation provides useful containment but not a complete
operating-system sandbox:

- the parent starts the existing Server.Api executable in private worker mode,
  clears its environment, attaches a Windows Job Object and sends untrusted
  bytes only after attachment;
- the Windows Job Object limits CPU time, process memory, active process count
  and lifetime, but does not by itself deny network, filesystem, registry,
  token or device access;
- on Linux, the worker applies `rlimit` and non-dumpable process state after a
  bounded trusted header and before reading the PDF body, but those controls do
  not provide filesystem, namespace, capability or syscall isolation;
- the implementation report explicitly records that it did not claim an
  operating-system network sandbox; and
- the Linux ARM64 publish selected AArch64 native libraries statically, but no
  Linux ARM64 renderer execution or sandbox escape test was performed.

The worker still runs with the product process's operating-system identity and
runtime visibility. A defect in PDFium, SkiaSharp, PDFtoImage, the managed
worker or a native transitive component could therefore attempt to access host
resources after receiving a malicious PDF even when the application contains
no intentional renderer network call.

## Decision drivers

- Make the operating system, rather than application intent, deny network and
  unrelated host-resource access.
- Install and attest the sandbox before any untrusted PDF byte is delivered.
- Preserve the existing private, bounded, path-free binary protocol.
- Apply equivalent security invariants on supported Windows and Linux ARM64
  runtimes while allowing different native enforcement primitives.
- Minimise the executable and dependency surface inside the sandbox.
- Preserve deterministic render profiles, manifests and content identities.
- Fail closed on an unsupported host, unavailable primitive or incomplete
  attestation.
- Keep public contracts, persistence schemas and source-rights decisions
  unchanged.
- Avoid treating a static cross-publish as runtime security evidence.

## Proposed decision

### Versioned sandbox profile

Introduce the internal security profile `pdf-render-sandbox-v1`, independent
from the pixel-affecting `pdf-page-png-v1` and
`pdf-page-png-notice-v1` render profiles. A render descriptor binds the exact
sandbox profile ID and a sanitised capability digest, but host-specific paths,
user names, process IDs and mutable machine details remain excluded.

The sandbox profile has one invariant contract across platforms:

- a dedicated minimal renderer worker, not the API host composition root;
- one document or one explicitly bounded page selection per worker lifetime;
- no provider credential, ambient environment, user profile, secret store,
  browser state or application configuration;
- no network access or capability, including loopback and local service
  discovery;
- no access to product stores, repository, catalogue, arbitrary filesystem,
  registry, device, named pipe or inherited handle;
- read-only access only to the exact worker, managed runtime, renderer native
  assets and deterministic font assets required by the selected profile;
- one private, bounded scratch area with no path returned to the parent and
  complete deletion by the sandbox owner;
- only the pre-created standard-input, standard-output and sanitised-error
  protocol handles crossing the boundary;
- bounded CPU, memory, process count, file descriptors or handles, output
  bytes, dimensions, pages and elapsed time;
- no core dump, debugger attachment or child-process escape; and
- parent-owned complete-tree termination and orphan detection.

An implementation may use different operating-system primitives only when it
proves every invariant. Missing or partially effective controls do not degrade
to the current worker automatically.

### Dedicated worker artefact

The selected boundary uses a dedicated renderer worker artefact with a minimal
entry point and dependency graph. It contains the private protocol,
deterministic rendering and required validation only. It does not reference
the API host, product composition, provider adapters, administration,
catalogue persistence, secret loading, HTTP hosting or Dashboard.

Application continues to own the typed renderer port, render policy,
descriptors and failure outcomes. Infrastructure owns the platform sandbox
launcher and protocol adapter. The dedicated worker remains an outer-layer
implementation detail; Domain and Application do not depend on operating-
system sandbox types.

Creating the artefact would be a later project and implementation change. This
proposal does not create it or select an additional third-party package.

### Pre-input bootstrap and attestation

The parent creates a fresh sandbox for every worker. The worker receives no PDF
body, source path or content-store handle until the following sequence is
complete:

1. resolve and verify the exact worker and native-asset identities from trusted
   deployment configuration;
2. create the operating-system sandbox and resource container;
3. start the worker in a suspended or bootstrap-only state with an explicit
   handle allowlist and closed environment;
4. apply identity, filesystem, network, syscall, process, memory and lifetime
   controls outside the untrusted renderer code path;
5. resume only the minimal bootstrap and receive a bounded attestation over the
   private protocol;
6. verify profile version, platform/RID, worker digest, asset digests and every
   required capability; and
7. only then send the bounded request header and exact verified source bytes.

Attestation is evidence that the expected setup path completed; it is not a
worker assertion trusted on its own. The parent derives it from authoritative
operating-system handles and configuration and binds it to the worker instance.
Timeout, malformed attestation, unknown capability, stale worker identity or
any setup race terminates the complete sandbox before a source byte is sent.

### Windows profile

The Windows implementation must create the worker suspended, assign it to the
fully configured Job Object before the first worker instruction, and resume it
only after all parent-side controls succeed. The Job Object retains kill-on-
close, one-process, CPU-time and memory limits.

The process also runs in an AppContainer or equivalently proven restricted
token boundary with:

- no network capabilities;
- low-integrity, least-privilege identity with unnecessary privileges removed;
- an explicit inherited-handle list;
- no user-profile or environment loading;
- no access to unrelated filesystem paths, registry hives, devices, window
  stations or named objects; and
- a task-owned scratch directory whose access control grants only the sandbox
  identity and coordinator clean-up owner.

A restricted token without proven network and filesystem denial is
insufficient. Assigning a normally started process to a Job Object after it can
execute is also insufficient for this profile, even when no PDF has yet been
sent.

### Linux ARM64 profile

The Linux ARM64 implementation must create the worker through a trusted minimal
launcher that installs the boundary before renderer code receives control. The
profile requires:

- unprivileged user and group identity with all capabilities dropped;
- `no_new_privs` and non-dumpable process state;
- isolated user, mount, PID and network namespaces, with no network interface;
- read-only exact runtime and renderer assets, a private minimal root and a
  bounded task-owned `tmpfs` scratch area;
- no host mounts, product stores, repository, device nodes or ambient Unix
  sockets;
- a seccomp allowlist derived from observed required syscalls and reviewed
  fail-closed on drift;
- cgroup v2 limits for memory, CPU and process count, supplemented by `rlimit`
  for file size, core dumps and file descriptors; and
- parent-owned cgroup/process-lifetime clean-up and orphan verification.

If required namespace, seccomp or cgroup controls are unavailable in the
target kernel or deployment shape, rendering is unavailable. Running with
`rlimit` alone or inheriting the surrounding OCI container's generic boundary
does not satisfy this profile without evidence that every invariant is
equivalent.

Static AArch64 library inspection and cross-publish remain supply-chain and
packaging evidence only. The profile requires native Linux ARM64 runtime tests
on the exact target class before that platform can be declared eligible.

### Filesystem and protocol boundary

The parent continues to supply verified source bytes through the bounded
binary protocol, never a PDF-controlled path or URL. The worker returns only
the declared descriptor, page measurements and bounded PNG bytes. It cannot
open a content store, commit a manifest, activate a document or delete an
object.

All protocol lengths, counts and numeric policy fields are checked by both
sides before allocation. The parent independently validates PNG structure,
hash, dimensions, page order, output total and descriptor before any durable
publication. Sandbox success never substitutes for output validation or
rights eligibility.

The scratch root is empty at creation except for explicitly staged immutable
assets. Symlinks, junctions, reparse points, hard-link substitution, alternate
data streams and device paths are rejected where applicable. Clean-up follows
verified task ownership; an uncertain path is preserved and reported rather
than removed broadly.

### Failure and availability semantics

Unsupported platform, missing kernel or Windows primitive, policy drift,
worker or asset digest mismatch, setup failure, attestation failure, resource
limit, escape attempt, crash, protocol violation or incomplete clean-up returns
a typed renderer-unavailable or renderer-failed outcome. It never retries in a
weaker sandbox, falls back in-process or activates partial visual evidence.

Text-first query behaviour may continue only through its already accepted
boundary and without inventing a page-image reference. A sandbox failure does
not weaken source rights, manifest, citation, activation or serving checks.

### Observability and evidence

Sanitised audit may record sandbox profile, platform/RID, worker and capability
digests, policy revision, duration, counts, peak bounded resources, exit class
and typed failure. It must not record source bytes, PDF text, PNG bytes,
absolute host paths, user or device name, command line, environment, security
token details or kernel object names.

Evidence is platform-specific. A Windows PASS does not prove Linux ARM64, an
OCI PASS does not prove a local host, and a synthetic fixture does not prove a
real source or product homologation.

### Compatibility and implementation sequence

The proposal preserves OpenAPI v1 and v2, render-profile IDs, manifest schemas,
database schemas, migrations, content identities, rights mappings and public
serving behaviour. The sandbox profile is an internal renderer descriptor and
deployment compatibility input.

If accepted, separate sequential authorities remain required for:

1. the dedicated worker project and closed dependency graph;
2. exact Windows launcher, identity and filesystem design;
3. exact Linux ARM64 launcher, namespace, seccomp and cgroup design;
4. any new native helper, package or deployment capability after supply-chain
   review;
5. integration without public contract, schema or migration change;
6. adversarial platform-specific tests and independent security review;
7. a clean Automatic Quality Gate on each supported runtime class; and
8. product-data, recovery, load, OCI and operational homologation under their
   own authorities.

Acceptance would not authorise any item in this sequence.

## Alternatives considered

### Retain Job Object and `rlimit` containment only

Rejected as the target sandbox. These controls bound important resources but
do not prove network, filesystem, registry, capability, syscall or host-
identity isolation.

### Continue using the complete Server.Api executable as the worker

Rejected. It carries a broader composition and dependency surface than the
renderer requires and weakens evidence that provider, administration, HTTP and
persistence capabilities are absent.

### Run the renderer in-process

Rejected. A native parser or renderer fault would share memory, identity,
handles and lifetime with the product process.

### Require an OCI container for every render

Not selected as the universal boundary. It adds a daemon/runtime and image
supply chain, may not be available for local Windows operation and does not by
itself prove the exact nested filesystem, capability, seccomp or network
policy. A later target may use OCI primitives only with equivalent evidence.

### Remote renderer service

Rejected for the MVP. It introduces a network protocol, authentication,
deployment, data egress and another operational service without a demonstrated
need.

### Support only the currently tested Windows host

Rejected. The selected deployment direction includes Linux ARM64, and static
packaging evidence cannot justify silently weakening or omitting its renderer
boundary.

### Disable PDF visual rendering

Retained as the safe fallback if this ADR is rejected, remains proposed or
cannot be implemented on a target. Textual evidence remains subject to its
existing independent controls; visual evidence is unavailable.

## Consequences and risks

### Positive

- Malicious PDF processing receives no intentional network or host-resource
  authority.
- The security invariant is explicit and testable on both target platforms.
- A minimal worker reduces the reachable product and dependency surface.
- Fail-closed attestation prevents input delivery to an incomplete sandbox.
- Existing deterministic rendering and manifest validation remain independent
  defence layers.

### Costs and residual risks

- Windows AppContainer/restricted-token and Linux namespace/seccomp/cgroup
  implementations are materially different and require specialist review.
- A dedicated worker and possible minimal native launcher add build and supply-
  chain responsibilities under later authority.
- Seccomp policy drift and native-library updates can make rendering
  unavailable until revalidated.
- Kernel, hypervisor and sandbox-primitive defects remain outside application
  control.
- Cross-platform tests require suitable Windows and native Linux ARM64 hosts;
  cross-publish alone is insufficient.

## Acceptance and stop conditions

Future acceptance must decide the complete proposed boundary, including:

- the dedicated minimal worker artefact;
- parent-established and attested sandbox before input;
- Windows suspended launch, Job Object and no-network restricted identity;
- Linux ARM64 namespaces, no capabilities, seccomp and cgroup controls;
- no weaker fallback on an unsupported or partially controlled host; and
- separate native runtime evidence for each supported platform.

The decision remains pending. The permitted decision phrases are:

```text
ADR-0019: ACEITAR.
ADR-0019: REJEITAR.
```

Preparation records neither phrase. Acceptance must stop if a required
platform primitive cannot be established without broad host privilege, if a
new helper or dependency has not received its own supply-chain decision, if
the boundary would require a public contract, schema or migration change, or
if Windows evidence would be used to infer Linux ARM64 behaviour.

Implementation must later stop if any untrusted byte can reach renderer code
before the sandbox is authoritatively installed and verified, or if the worker
can access network, host identity, product stores, repository, secrets,
arbitrary paths or child-process authority outside this profile.
