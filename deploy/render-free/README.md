# Render Free private deployment package

## Purpose and boundary

This package prepares a zero-hosting-cost Render candidate without creating a
Render service, publishing an image or changing billing. It uses one Render
web service with `plan: free`, one instance, no Render database, no persistent
disk and no automatic deployment.

The product store contains third-party source bytes and derived retrieval data.
It must never enter the public Git repository or a public container registry.
The builder stages it only beneath ignored `artifacts-local/` and the generated
container context is private deployment material.

This candidate does not replace the accepted OCI target in ADR-0005. Selecting
Render as the final public target requires a separate factual and architectural
reconciliation before deployment.

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

## Later publication boundary

A separately authorised publication must build the private image locally,
publish it to an access-controlled registry and replace the template image
placeholder with an immutable digest. The Render dashboard must still show
`Free` before service creation. Do not add a disk, database, paid instance,
autoscaling or paid workspace feature.

`OPENAI_API_KEY` is supplied only as a Render secret environment variable. It
is not part of the image or package. Provider use is billed independently from
Render hosting and remains subject to its own authority and spending controls.
