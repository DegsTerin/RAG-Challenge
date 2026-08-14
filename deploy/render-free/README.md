# Render Free private deployment package

## Purpose and boundary

This package defines a zero-hosting-cost Render deployment boundary. Its local
builder does not create a Render service, publish an image or change billing.
The deployed shape uses one Render web service with `plan: free`, one instance,
no Render database, no persistent disk and no automatic deployment.

The product store contains third-party source bytes and derived retrieval data.
It must never enter the public Git repository or a public container registry.
The builder stages it only beneath ignored `artifacts-local/` and the generated
container context is private deployment material.

This homologation deployment does not replace the accepted OCI target in
ADR-0005. Selecting Render as the final production target requires a separate
factual and architectural reconciliation.

## Runtime model

Render Free has an ephemeral filesystem and can stop an idle web service. The
image therefore carries a verified, read-only seed containing the activated
PostgreSQL 18.4 product store. Every process start:

1. verifies every seed file with SHA-256;
2. creates a fresh writable copy under `/tmp/rag-challenge-store`;
3. verifies that copy before starting the API;
4. starts the same-origin Dashboard and API as an unprivileged user; and
5. fails closed when the seed or provider credential is absent.

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

The builder performs no restore, reads no real credential, invokes no provider
and does not call Docker or Render. Output is written to
`artifacts-local/render-free-package/` and contains:

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

`OPENAI_API_KEY` is supplied only as a Render secret environment variable. It
is not part of the image or package. Provider use is billed independently from
Render hosting and remains subject to its own authority and spending controls.

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
