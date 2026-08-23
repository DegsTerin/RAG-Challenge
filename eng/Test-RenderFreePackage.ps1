# Purpose: Verifies the private Render Free package, full context integrity and zero-paid-resource guard without running Docker or contacting external services.
[CmdletBinding()]
param(
    [string]$OutputRoot = "artifacts-local/render-free-package"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot "render-free-output-policy.ps1")

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$resolvedOutputRoot = Resolve-RenderFreePackageOutputRoot `
    -RepositoryRoot $repositoryRoot `
    -RequestedOutputRoot $OutputRoot
Assert-RenderFreePackageOwnedOutput -OutputRoot $resolvedOutputRoot

$contextRoot = Join-Path $resolvedOutputRoot "context"
$contextManifestPath = Join-Path $contextRoot "context-manifest.sha256"
$packageManifestPath = Join-Path $contextRoot "package-manifest.json"
$renderTemplatePath = Join-Path $resolvedOutputRoot "render.yaml.template"
$expectedGeneration =
    "idxgen-4b417b79a9d8cd2472cb657a5fe7509f297b39f4831215f62143080d896e4f0d"
$expectedGenerationContentDigest =
    "4b417b79a9d8cd2472cb657a5fe7509f297b39f4831215f62143080d896e4f0d"
$expectedLogicalArtefactDigest =
    "af207b4c359b985bb51b91ec39a40ab22cde93bd3fbbb667741c4b1172461558"
$expectedPreparedStoreSha256 =
    "dc1aa3a21056a5094be99f7a46b9ab738a139bd0b121907b117cad3eac7dfce6"

foreach ($requiredPath in @(
        $contextRoot,
        $contextManifestPath,
        $packageManifestPath,
        $renderTemplatePath,
        (Join-Path $contextRoot "Dockerfile"),
        (Join-Path $contextRoot "entrypoint.sh"),
        (Join-Path $contextRoot "seed/control.db"),
        (Join-Path $contextRoot "seed/vectors.db"),
        (Join-Path $contextRoot "seed/seed-manifest.sha256"),
        (Join-Path $contextRoot "release/RagChallenge.Server.Api.dll"),
        (Join-Path $contextRoot "release/wwwroot/index.html"))) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "The Render Free package omitted a required path."
    }
}

$manifestLines = @(Get-Content -LiteralPath $contextManifestPath |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
$manifestPaths = [System.Collections.Generic.HashSet[string]]::new(
    [System.StringComparer]::Ordinal)

foreach ($line in $manifestLines) {
    $match = [System.Text.RegularExpressions.Regex]::Match(
        $line,
        "^(?<hash>[0-9a-f]{64})  (?<size>[0-9]+)  (?<path>.+)$")
    if (-not $match.Success) {
        throw "The Render Free context manifest contains a malformed record."
    }

    $relativePath = $match.Groups["path"].Value
    if ($relativePath.StartsWith("/", [System.StringComparison]::Ordinal) -or
        $relativePath.Contains("\", [System.StringComparison]::Ordinal) -or
        $relativePath.Split('/') -contains ".." -or
        -not $manifestPaths.Add($relativePath)) {
        throw "The Render Free context manifest contains an unsafe or duplicate path."
    }

    $fullPath = [System.IO.Path]::GetFullPath((Join-Path $contextRoot $relativePath))
    $contextPrefix = $contextRoot.TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar) +
        [System.IO.Path]::DirectorySeparatorChar
    if (-not $fullPath.StartsWith(
            $contextPrefix,
            [System.StringComparison]::OrdinalIgnoreCase) -or
        -not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
        throw "The Render Free context manifest points outside the context or to a missing file."
    }

    $item = Get-Item -LiteralPath $fullPath
    $digest = (Get-FileHash -LiteralPath $fullPath -Algorithm SHA256).
        Hash.ToLowerInvariant()
    if ($item.Length -ne [long]$match.Groups["size"].Value -or
        $digest -cne $match.Groups["hash"].Value) {
        throw "The Render Free context differs from its integrity manifest."
    }
}

$contextFiles = @(Get-ChildItem -LiteralPath $contextRoot -Recurse -File |
    Where-Object { $_.FullName -cne $contextManifestPath })
if ($contextFiles.Count -ne $manifestPaths.Count) {
    throw "The Render Free context manifest does not cover every payload file exactly once."
}

$packageManifest = Get-Content -LiteralPath $packageManifestPath -Raw |
    ConvertFrom-Json -Depth 20
$head = (git -C $repositoryRoot rev-parse HEAD).Trim()
$status = @(git -C $repositoryRoot status --porcelain=v1 --untracked-files=all)

if ($packageManifest.source.branch -cne "main" -or
    $packageManifest.source.head -cne $head -or
    $packageManifest.source.corpus -cne "4.19.5" -or
    $status.Count -ne 0 -or
    $packageManifest.hosting.workspacePlan -cne "hobby" -or
    $packageManifest.hosting.servicePlan -cne "free" -or
    $packageManifest.hosting.instances -ne 1 -or
    $packageManifest.hosting.persistentDisk -ne $false -or
    $packageManifest.hosting.managedDatabase -ne $false -or
    $packageManifest.hosting.autoDeploy -ne $false -or
    $packageManifest.product.activeGenerationId -cne $expectedGeneration -or
    $packageManifest.product.generationContentDigest -cne
        $expectedGenerationContentDigest -or
    $packageManifest.product.logicalArtefactDigest -cne $expectedLogicalArtefactDigest -or
    $packageManifest.product.preparedStoreSha256 -cne $expectedPreparedStoreSha256 -or
    $packageManifest.product.controlDbSha256 -notmatch '^[0-9a-f]{64}$' -or
    $packageManifest.product.vectorsDbSha256 -notmatch '^[0-9a-f]{64}$' -or
    $packageManifest.product.contentStructuralTreeSha256 -notmatch '^[0-9a-f]{64}$' -or
    $packageManifest.product.offlineAdministrativeStatusValidated -ne $true -or
    $packageManifest.product.administrativeStatusResultCode -cne
        "CH_ADMIN_STATUS_AVAILABLE" -or
    $packageManifest.product.administrativeStatusCorpusId -cne
        "rag-challenge-product" -or
    $packageManifest.product.administrativeStatusRevision -le 0 -or
    $packageManifest.product.failClosedReadinessValidated -ne $true -or
    $packageManifest.product.providerBudgetState -cne "Disarmed" -or
    $packageManifest.product.loopbackLivenessValidated -ne $true -or
    $packageManifest.externalActions.dockerInvoked -ne $false -or
    $packageManifest.externalActions.imagePublished -ne $false -or
    $packageManifest.externalActions.renderContacted -ne $false -or
    $packageManifest.externalActions.providerQuerySubmitted -ne $false -or
    $packageManifest.externalActions.providerCredentialConfigured -ne $false -or
    $packageManifest.externalActions.trustedProviderGrantConfigured -ne $false -or
    $packageManifest.externalActions.egressObservationPerformed -ne $false) {
    throw "The Render Free package identity or external-action boundary diverged."
}

$renderTemplate = Get-Content -LiteralPath $renderTemplatePath -Raw
if ($renderTemplate -notmatch "(?m)^\s*plan:\s+free\s*$" -or
    $renderTemplate -notmatch "(?m)^\s*numInstances:\s+1\s*$" -or
    $renderTemplate -notmatch "(?m)^\s*runtime:\s+image\s*$" -or
    $renderTemplate -notmatch "(?m)^\s*autoDeployTrigger:\s+off\s*$" -or
    $renderTemplate -notmatch "<private-image-reference-by-digest>" -or
    $renderTemplate -match "(?im)^\s*(?:disk|databases|scaling):" -or
    $renderTemplate -match "(?im)^\s*plan:\s+(?:starter|standard|pro|pro plus|pro max|pro ultra)\s*$") {
    throw "The Render template does not enforce the free-only placeholder boundary."
}

$dockerfile = Get-Content -LiteralPath (Join-Path $contextRoot "Dockerfile") -Raw
$entrypoint = Get-Content -LiteralPath (Join-Path $contextRoot "entrypoint.sh") -Raw
if ($dockerfile -notmatch "aspnet:10[.]0[.]11@sha256:[0-9a-f]{64}" -or
    $dockerfile -notmatch "(?m)^USER app\s*$" -or
    $dockerfile -notmatch "chmod -R a-w /opt/rag-challenge/seed" -or
    $entrypoint -notmatch "sha256sum -c seed-manifest[.]sha256" -or
    $entrypoint -notmatch 'runtime_store="/tmp/rag-challenge-store"' -or
    $entrypoint -notmatch 'runtime_marker="[.]rag-challenge-runtime-store-v1"' -or
    $entrypoint -notmatch "CH_DEPLOY_RUNTIME_STORE_UNSAFE" -or
    $entrypoint -notmatch "umask 077" -or
    $entrypoint -match 'runtime_store="[$][{]RAG_CHALLENGE_RUNTIME_STORE:-' -or
    $entrypoint.IndexOf('runtime_marker_value', [System.StringComparison]::Ordinal) -gt
        $entrypoint.IndexOf('rm -rf --', [System.StringComparison]::Ordinal) -or
    $entrypoint -match "OPENAI_API_KEY|CredentialEnvironmentVariable" -or
    ($dockerfile + $entrypoint) -match "(?im)^\s*(?:curl|wget|Invoke-WebRequest|Invoke-RestMethod)\b") {
    throw "The container boundary is not pinned, unprivileged, offline-seeded and fail-closed."
}

$forbiddenPayloads = @(Get-ChildItem -LiteralPath $contextRoot -Recurse -File |
    Where-Object {
        $_.Name -match "(?i)(?:^|[.])env(?:[.]|$)|[.]pdf$|prepared-store[.]json$|[.]runtime[.]json$"
    })
if ($forbiddenPayloads.Count -ne 0) {
    throw "The Render Free context contains a forbidden named payload."
}

[pscustomobject]@{
    Status = "Passed"
    SourceHead = $head
    ActiveGenerationId = $packageManifest.product.activeGenerationId
    ContextFiles = $contextFiles.Count + 1
    ReleaseFiles = $packageManifest.release.files
    ReleaseBytes = $packageManifest.release.bytes
    SeedFiles = $packageManifest.seed.files
    SeedBytes = $packageManifest.seed.bytes
    ServicePlan = $packageManifest.hosting.servicePlan
    Instances = $packageManifest.hosting.instances
    PersistentDisk = $packageManifest.hosting.persistentDisk
    ManagedDatabase = $packageManifest.hosting.managedDatabase
    ImagePublished = $false
    RenderContacted = $false
    ProviderCalled = $false
    CredentialRead = $false
} | ConvertTo-Json -Compress
