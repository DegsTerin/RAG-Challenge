# Render Free private deployment package

## Purpose and boundary

This package defines a Render Free-tier deployment boundary. Its local builder
does not create a Render service, publish an image or change billing. The
observed 2026-08-14 deployment used one web service with `plan: free`, one
instance, no Render database, no persistent disk and no automatic deployment;
that historical zero-charge observation is not a guarantee of future pricing
or billing.

The product store contains third-party source bytes and derived retrieval data.
It must never enter the public Git repository or a public container registry.
The builder stages it only beneath ignored `artifacts-local/` and the generated
container context is private deployment material.

Under accepted ADR-0020, Render remains the secondary public deployment and
homologation surface, while OCI remains the durable MVP target selected by
ADR-0005. The two instances may coexist only after OCI is separately
authorised, implemented and verified, with isolated configuration, secrets,
storage, mutable state and deployment lifecycles. OCI is not deployed by this
package.

## Runtime model

Render Free has an ephemeral filesystem and can stop an idle web service. The
image therefore carries a verified, read-only seed containing the activated
PostgreSQL 18.4 product store. The entrypoint uses only the fixed seed
`/opt/rag-challenge/seed` and runtime store `/tmp/rag-challenge-store`. Every
process start:

1. rejects root overrides, symbolic links and an existing runtime store without
   the exact product ownership marker;
2. verifies every seed file with SHA-256 before replacing an owned runtime
   store;
3. recreates the exact runtime store with private creation permissions;
4. verifies the writable copy before starting the API;
5. starts the same-origin Dashboard and API as an unprivileged user; and
6. leaves provider-authority and credential validation to the product runtime.

The active generation is restored after every restart. Answer-evidence records
written after startup are intentionally disposable and are lost on a restart,
redeploy or idle spin-down. This is a demonstration boundary, not durable
production persistence.

## Local preparation

From a clean `main` checkout with the restored .NET and npm graphs:

```powershell
./eng/Build-RenderFreePackage.ps1
./eng/Test-RenderFreePackage.ps1
```

The builder refuses a dirty or non-`main` source tree. It performs no restore,
reads no real credential, invokes no provider and does not call Docker or
Render. Before replacing output, it accepts only the exact canonical
`artifacts-local/render-free-package/` leaf with the matching
`.rag-challenge-owned-output.json` marker; an unmarked, malformed or
reparse-point tree is preserved and rejected.

The local preflight uses a disposable copy of the staged store. It requires the
offline administrative status to report `CH_ADMIN_STATUS_AVAILABLE` with the
expected corpus and a positive configuration revision. It then starts the
public Product host without a provider credential or grant and requires:

- liveness HTTP `200` with status `Live`;
- readiness HTTP `503` with status `Unready`;
- exactly one sanitised `provider-budget` check in state `Disarmed`;
- no active generation identifier and zero active/eligible counts; and
- no submitted provider query, configured provider credential or trusted grant.

The readiness flow is expected to stop at the disarmed budget before provider
access, and the separate Product regression test observes zero credential-reader
and HTTP-handler invocations. The package preflight itself does not independently
observe operating-system outbound traffic and does not claim that evidence.

This fail-closed result is the current zero-budget package contract. It does not
contradict the dated, separately authorised 2026-08-14 deployment observation
below, which used the then-current package and reported `Ready`.

The canonical output contains:

- `.rag-challenge-owned-output.json`: bounded ownership marker for safe replay;
- `context/`: the private Docker build context;
- `context/package-manifest.json`: sanitised release and seed identity;
- `context/context-manifest.sha256`: full context integrity manifest; and
- `render.yaml.template`: a non-deployable template with an image placeholder.

## Publication and deployment boundary

A separately authorised publication built the image locally and published it
to an access-controlled registry. The subsequent one-shot deployment used the
immutable digest rather than a mutable tag. The template remains deliberately
non-deployable and retains its placeholder. The Render dashboard showed `Free`
before service creation. No disk, database, paid instance, autoscaling or paid
workspace feature was added.

The product credential is supplied only as a Render secret environment
variable and is not part of the image or package. The entrypoint does not
inspect its value. The non-deployable template also requires distinct bounded
operation-specific `AUTH-*` request references for query embedding and grounded
generation, plus independently supplied trusted grants for those exact
operation/reference pairs. Requested references never create trusted grants.
The product validates the exact operation and trusted grant immediately before
credential lookup or provider egress. Provider use is billed independently
from Render hosting and remains subject to its own authority and spending
controls.

## Observed Render Free deployment

On 2026-08-14, the owner separately authorised one private-image publication
and one Render deployment. The resulting evidence is:

- private image:
  `ghcr.io/degsterin/rag-challenge@sha256:536e431126470a51370bf9aeb4c769ff1d75313c67643c3922cf0fd2e2688c08`;
- Render service: `rag-challenge`, service ID
  `srv-d9v9gju417fc73cf69i0`;
- deployment: `dep-d9v9gke417fc73cf6br0`, `Deploy succeeded | Live`,
  duration 46.8 seconds;
- public base URL: <https://rag-challenge-ac09.onrender.com>;
- liveness: HTTP `200`, status `Live`;
- readiness: HTTP `200`, status `Ready`, one active database, one eligible
  document, zero degraded documents and configuration revision
  `postgresql-18.4-product-v1`;
- active generation:
  `idxgen-ec39244b021c90fceea1b3a628fe793a99f74650cad451f16ffbcd414af636f6`;
- hosting resources: Free plan, one instance, autoscaling off, zero persistent
  disks and zero Render databases; and
- observed billing: no card, services `USD 0.00`, month-to-date `USD 0.00` and
  projected total `USD 0.00`.

The deployment verification called only the public liveness and readiness
endpoints. It did not execute a product query, Responses request or embedding
request. Render Free may spin the service down when idle, and the first request
afterwards can take 50 seconds or more.

Sanitised visual evidence is stored in
[`../../docs/assets/render/rag-challenge-render-live.png`](../../docs/assets/render/rag-challenge-render-live.png)
and
[`../../docs/assets/render/rag-challenge-deployment.gif`](../../docs/assets/render/rag-challenge-deployment.gif).
