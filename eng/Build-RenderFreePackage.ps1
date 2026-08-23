# Purpose: Builds a private Render Free Docker context from the activated PostgreSQL product store without publishing, restoring or reading credentials.
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

$expectedStoreRoot = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot (
            "artifacts-local/state-07/product-materialisation/" +
            "postgresql-18-reference-a4/" +
            "product-store-recovery-postgresql-18_4-local-v1")))
$dashboardRoot = Join-Path $repositoryRoot "src/RagChallenge.Dashboard.Web"
$serverProject = Join-Path $repositoryRoot (
    "src/RagChallenge.Server.Api/RagChallenge.Server.Api.csproj")
$deploymentRoot = Join-Path $repositoryRoot "deploy/render-free"
$contextRoot = Join-Path $resolvedOutputRoot "context"
$rawPublishRoot = Join-Path $resolvedOutputRoot ".publish-raw"
$releaseRoot = Join-Path $contextRoot "release"
$seedRoot = Join-Path $contextRoot "seed"
$readinessStoreRoot = Join-Path $resolvedOutputRoot ".readiness-store"
$utf8WithoutBom = [System.Text.UTF8Encoding]::new($false)
$expectedGeneration =
    "idxgen-4b417b79a9d8cd2472cb657a5fe7509f297b39f4831215f62143080d896e4f0d"
$expectedGenerationContentDigest =
    "4b417b79a9d8cd2472cb657a5fe7509f297b39f4831215f62143080d896e4f0d"
$expectedLogicalArtefactDigest =
    "af207b4c359b985bb51b91ec39a40ab22cde93bd3fbbb667741c4b1172461558"
$expectedPreparedStoreSha256 =
    "dc1aa3a21056a5094be99f7a46b9ab738a139bd0b121907b117cad3eac7dfce6"
$expectedOpenApiV1 =
    "d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34"
$expectedOpenApiV2 =
    "f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733"

function Get-RenderStoreStructuralTreeSha256 {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Root)

    $relativePaths = [string[]](Get-ChildItem -LiteralPath $Root -Recurse -File -Force |
        ForEach-Object {
            [System.IO.Path]::GetRelativePath($Root, $_.FullName).Replace('\', '/')
        })
    [System.Array]::Sort($relativePaths, [System.StringComparer]::Ordinal)

    $lines = foreach ($relativePath in $relativePaths) {
        $fullPath = Join-Path $Root $relativePath
        $digest = (Get-FileHash -LiteralPath $fullPath -Algorithm SHA256).
            Hash.ToLowerInvariant()
        $length = (Get-Item -LiteralPath $fullPath -Force).Length
        "$digest  $length  $relativePath"
    }
    $payload = [System.Text.Encoding]::UTF8.GetBytes(
        [string]::Join("`n", $lines) + "`n")
    return [System.Convert]::ToHexString(
        [System.Security.Cryptography.SHA256]::HashData($payload)).ToLowerInvariant()
}

Push-Location $repositoryRoot

try {
    $branch = (git branch --show-current).Trim()
    $head = (git rev-parse HEAD).Trim()
    $status = @(git status --porcelain=v1 --untracked-files=all)
    $openApiV1 = (Get-FileHash -LiteralPath "docs/api/openapi-v1.json" -Algorithm SHA256).
        Hash.ToLowerInvariant()
    $openApiV2 = (Get-FileHash -LiteralPath "docs/api/openapi-v2.json" -Algorithm SHA256).
        Hash.ToLowerInvariant()

    if ($branch -cne "main" -or $status.Count -ne 0) {
        throw "The Render Free package requires a clean main checkout."
    }

    if ($openApiV1 -cne $expectedOpenApiV1 -or $openApiV2 -cne $expectedOpenApiV2) {
        throw "A protected OpenAPI identity diverged."
    }

    if (-not (Test-Path -LiteralPath $expectedStoreRoot -PathType Container)) {
        throw "The activated PostgreSQL product store is unavailable."
    }

    $storeRootItem = Get-Item -LiteralPath $expectedStoreRoot -Force
    if (($storeRootItem.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw "The activated PostgreSQL product store cannot be a reparse point."
    }

    Assert-OwnedOutputTreeIsSafe -Root $expectedStoreRoot

    foreach ($requiredPath in @(
            (Join-Path $expectedStoreRoot "control.db"),
            (Join-Path $expectedStoreRoot "vectors.db"),
            (Join-Path $expectedStoreRoot "content"),
            (Join-Path $expectedStoreRoot "prepared-store.json"))) {
        if (-not (Test-Path -LiteralPath $requiredPath)) {
            throw "The activated PostgreSQL product store is incomplete."
        }
    }

    $transientStoreFiles = @(Get-ChildItem -LiteralPath $expectedStoreRoot -Recurse -File |
        Where-Object { $_.Name.EndsWith("-wal") -or $_.Name.EndsWith("-shm") })
    if ($transientStoreFiles.Count -ne 0) {
        throw "The product store contains an open SQLite WAL or shared-memory file."
    }

    $preparedStorePath = Join-Path $expectedStoreRoot "prepared-store.json"
    $preparedStoreSha256 = (Get-FileHash -LiteralPath $preparedStorePath -Algorithm SHA256).
        Hash.ToLowerInvariant()
    if ($preparedStoreSha256 -cne $expectedPreparedStoreSha256) {
        throw "The prepared product-store attestation identity diverged."
    }

    try {
        $preparedStore = Get-Content -LiteralPath $preparedStorePath -Raw |
            ConvertFrom-Json -Depth 10
    }
    catch {
        throw "The prepared product-store attestation is invalid JSON."
    }

    $controlDbSha256 = (Get-FileHash -LiteralPath (
            Join-Path $expectedStoreRoot "control.db") -Algorithm SHA256).Hash.ToLowerInvariant()
    $vectorsDbSha256 = (Get-FileHash -LiteralPath (
            Join-Path $expectedStoreRoot "vectors.db") -Algorithm SHA256).Hash.ToLowerInvariant()
    $contentStructuralTreeSha256 = Get-RenderStoreStructuralTreeSha256 `
        -Root (Join-Path $expectedStoreRoot "content")
    if ($preparedStore.schemaVersion -ne 1 -or
        $preparedStore.catalogueProfile -cne "postgresql-18.4" -or
        $preparedStore.corpusId -cne "rag-challenge-product" -or
        $preparedStore.corpusVersion -cne "4.19.5" -or
        $preparedStore.rightsEvidenceReference -cne "auth-s07-a-product-a0-003" -or
        $preparedStore.activeGenerationId -cne $expectedGeneration -or
        $preparedStore.generationContentDigest -cne $expectedGenerationContentDigest -or
        $preparedStore.logicalArtefactDigest -cne $expectedLogicalArtefactDigest -or
        $preparedStore.chunkCount -ne 3282 -or
        $preparedStore.vectorCount -ne 3282 -or
        $preparedStore.embeddingModel -cne "text-embedding-3-small" -or
        $preparedStore.embeddingDimensions -ne 1536 -or
        $preparedStore.controlDbSha256 -cne $controlDbSha256 -or
        $preparedStore.vectorsDbSha256 -cne $vectorsDbSha256 -or
        $preparedStore.contentStructuralTreeSha256 -cne $contentStructuralTreeSha256 -or
        $preparedStore.sourcePdfSha256 -cne
            "cea7b845568095eb56dee1b51bfa145c6c6637bc4377c986019971577efefae4" -or
        $preparedStore.providerRequests -ne 52 -or
        $preparedStore.providerCommittedMicroUsd -ne 149629) {
        throw "The prepared product-store attestation does not match the current candidate."
    }

    $resolvedOutputRoot = Reset-RenderFreePackageOutput `
        -RepositoryRoot $repositoryRoot `
        -RequestedOutputRoot $OutputRoot

    New-Item -ItemType Directory -Path $releaseRoot -Force | Out-Null
    New-Item -ItemType Directory -Path $seedRoot -Force | Out-Null

    $env:npm_config_offline = "true"
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
    $env:DOTNET_NOLOGO = "1"
    $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

    Push-Location $dashboardRoot
    try {
        npm run build
        if ($LASTEXITCODE -ne 0) {
            throw "The Dashboard build failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }

    dotnet publish $serverProject `
        --configuration Release `
        --no-restore `
        --output $rawPublishRoot `
        -p:ContinuousIntegrationBuild=true `
        -p:Deterministic=true `
        -p:DebugSymbols=false `
        -p:DebugType=None
    if ($LASTEXITCODE -ne 0) {
        throw "The portable server publish failed with exit code $LASTEXITCODE."
    }

    Get-ChildItem -LiteralPath $rawPublishRoot -File |
        Where-Object { $_.Extension -notin @(".exe", ".pdb") } |
        Copy-Item -Destination $releaseRoot

    foreach ($directory in Get-ChildItem -LiteralPath $rawPublishRoot -Directory |
            Where-Object { $_.Name -cne "runtimes" }) {
        Copy-Item -LiteralPath $directory.FullName -Destination $releaseRoot -Recurse
    }

    $linuxRuntimeRoot = Join-Path $rawPublishRoot "runtimes/linux-x64"
    if (-not (Test-Path -LiteralPath $linuxRuntimeRoot -PathType Container)) {
        throw "The portable publish omitted the Linux x64 native runtime assets."
    }

    New-Item -ItemType Directory -Path (Join-Path $releaseRoot "runtimes") -Force |
        Out-Null
    Copy-Item -LiteralPath $linuxRuntimeRoot -Destination (
        Join-Path $releaseRoot "runtimes") -Recurse

    New-Item -ItemType Directory -Path (Join-Path $releaseRoot "wwwroot") -Force |
        Out-Null
    Copy-Item -Path (Join-Path $dashboardRoot "dist/*") -Destination (
        Join-Path $releaseRoot "wwwroot") -Recurse -Force

    foreach ($requiredNativePath in @(
            "runtimes/linux-x64/native/libe_sqlite3.so",
            "runtimes/linux-x64/native/libpdfium.so",
            "runtimes/linux-x64/native/libSkiaSharp.so")) {
        if (-not (Test-Path -LiteralPath (Join-Path $releaseRoot $requiredNativePath) -PathType Leaf)) {
            throw "The Linux x64 release omitted '$requiredNativePath'."
        }
    }

    Assert-OwnedOutputTreeIsSafe -Root $expectedStoreRoot
    Copy-Item -LiteralPath (Join-Path $expectedStoreRoot "control.db") -Destination $seedRoot
    Copy-Item -LiteralPath (Join-Path $expectedStoreRoot "vectors.db") -Destination $seedRoot
    Copy-Item -LiteralPath (Join-Path $expectedStoreRoot "content") -Destination $seedRoot -Recurse

    Assert-OwnedOutputTreeIsSafe -Root $seedRoot
    $seedPaths = [string[]](Get-ChildItem -LiteralPath $seedRoot -Recurse -File |
        ForEach-Object {
            [System.IO.Path]::GetRelativePath($seedRoot, $_.FullName).Replace('\', '/')
        })
    [System.Array]::Sort($seedPaths, [System.StringComparer]::Ordinal)
    $seedManifestLines = foreach ($relativePath in $seedPaths) {
        $digest = (Get-FileHash -LiteralPath (Join-Path $seedRoot $relativePath) `
                -Algorithm SHA256).Hash.ToLowerInvariant()
        "$digest  $relativePath"
    }
    [System.IO.File]::WriteAllText(
        (Join-Path $seedRoot "seed-manifest.sha256"),
        ([string]::Join("`n", $seedManifestLines) + "`n"),
        $utf8WithoutBom)

    New-Item -ItemType Directory -Path $readinessStoreRoot -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $seedRoot "control.db") `
        -Destination $readinessStoreRoot
    Copy-Item -LiteralPath (Join-Path $seedRoot "vectors.db") `
        -Destination $readinessStoreRoot
    Copy-Item -LiteralPath (Join-Path $seedRoot "content") `
        -Destination $readinessStoreRoot -Recurse

    $offlineOperationId = "render-package-status-$($head.Substring(0, 12))"
    $administrativeStartInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $administrativeStartInfo.FileName = (Get-Command dotnet).Source
    $administrativeStartInfo.WorkingDirectory = $rawPublishRoot
    $administrativeStartInfo.UseShellExecute = $false
    $administrativeStartInfo.CreateNoWindow = $true
    $administrativeStartInfo.RedirectStandardOutput = $true
    $administrativeStartInfo.RedirectStandardError = $true
    foreach ($argument in @(
            "RagChallenge.Server.Api.dll",
            "admin",
            "status",
            "--operation-id",
            $offlineOperationId,
            "--corpus-id",
            "rag-challenge-product",
            "--reason",
            "Verify the private Render Free seed offline.")) {
        $administrativeStartInfo.ArgumentList.Add($argument)
    }

    $administrativeStartInfo.Environment.Clear()
    $temporaryDirectory = [System.IO.Path]::GetTempPath()
    $administrativeStartInfo.Environment["TEMP"] = $temporaryDirectory
    $administrativeStartInfo.Environment["TMP"] = $temporaryDirectory
    if ($IsWindows) {
        $windowsDirectory = [System.IO.Directory]::GetParent(
            [System.Environment]::SystemDirectory).FullName
        $administrativeStartInfo.Environment["SystemRoot"] = $windowsDirectory
        $administrativeStartInfo.Environment["WINDIR"] = $windowsDirectory
    }
    $administrativeStartInfo.Environment["DOTNET_ENVIRONMENT"] = "Production"
    $administrativeStartInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Production"
    $administrativeStartInfo.Environment[
        "RagChallenge__Administration__Enabled"] = "true"
    $administrativeStartInfo.Environment[
        "RagChallenge__Administration__ApplyMigrations"] = "false"
    $administrativeStartInfo.Environment[
        "RagChallenge__Administration__StoreRoot"] = $readinessStoreRoot
    $administrativeStartInfo.Environment["Logging__LogLevel__Default"] = "Warning"

    $administrativeProcess = [System.Diagnostics.Process]::Start($administrativeStartInfo)
    try {
        $administrativeOutputTask = $administrativeProcess.StandardOutput.ReadToEndAsync()
        $administrativeErrorTask = $administrativeProcess.StandardError.ReadToEndAsync()
        if (-not $administrativeProcess.WaitForExit(60000)) {
            $administrativeProcess.Kill($true)
            throw "The offline package status command exceeded its bounded duration."
        }

        $administrativeOutput = $administrativeOutputTask.GetAwaiter().GetResult().Trim()
        $administrativeError = $administrativeErrorTask.GetAwaiter().GetResult().Trim()
        if ($administrativeProcess.ExitCode -ne 0 -or
            -not [string]::IsNullOrWhiteSpace($administrativeError) -or
            [string]::IsNullOrWhiteSpace($administrativeOutput) -or
            [System.Text.Encoding]::UTF8.GetByteCount($administrativeOutput) -gt 32768 -or
            @($administrativeOutput -split "`r?`n").Count -ne 1) {
            throw "The offline package status command failed its bounded contract."
        }

        try {
            $administrativeStatus = $administrativeOutput | ConvertFrom-Json -Depth 10
        }
        catch {
            throw "The offline package status command returned invalid JSON."
        }

        if ($administrativeStatus.status -cne "Applied" -or
            $administrativeStatus.resultCode -cne "CH_ADMIN_STATUS_AVAILABLE" -or
            $administrativeStatus.command -cne "status" -or
            $administrativeStatus.operationId -cne $offlineOperationId -or
            $administrativeStatus.corpusId -cne "rag-challenge-product" -or
            $administrativeStatus.resultRevision -isnot [long] -or
            $administrativeStatus.resultRevision -le 0 -or
            $null -ne $administrativeStatus.resultPayload) {
            throw "The offline package status identity diverged."
        }

        $offlineStatusRevision = [long]$administrativeStatus.resultRevision
    }
    finally {
        if (-not $administrativeProcess.HasExited) {
            $administrativeProcess.Kill($true)
            $null = $administrativeProcess.WaitForExit(10000)
        }

        $administrativeProcess.Dispose()
    }

    $portReservation = [System.Net.Sockets.TcpListener]::new(
        [System.Net.IPAddress]::Loopback,
        0)
    $portReservation.Start()
    $readinessPort = ([System.Net.IPEndPoint]$portReservation.LocalEndpoint).Port
    $portReservation.Stop()

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = (Get-Command dotnet).Source
    $startInfo.WorkingDirectory = $rawPublishRoot
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.ArgumentList.Add("RagChallenge.Server.Api.dll")
    $startInfo.Environment.Clear()
    $temporaryDirectory = [System.IO.Path]::GetTempPath()
    $startInfo.Environment["TEMP"] = $temporaryDirectory
    $startInfo.Environment["TMP"] = $temporaryDirectory
    if ($IsWindows) {
        $windowsDirectory = [System.IO.Directory]::GetParent(
            [System.Environment]::SystemDirectory).FullName
        $startInfo.Environment["SystemRoot"] = $windowsDirectory
        $startInfo.Environment["WINDIR"] = $windowsDirectory
    }
    $startInfo.Environment["DOTNET_ENVIRONMENT"] = "Production"
    $startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Production"
    $startInfo.Environment["ASPNETCORE_URLS"] = "http://127.0.0.1:$readinessPort"
    $startInfo.Environment["RagChallenge__Integration__Enabled"] = "false"
    $startInfo.Environment["RagChallenge__Setup__AllowExternalServices"] = "true"
    $startInfo.Environment["RagChallenge__Product__Enabled"] = "true"
    $startInfo.Environment["RagChallenge__Product__ApplyMigrations"] = "false"
    $startInfo.Environment["RagChallenge__Product__StoreRoot"] = $readinessStoreRoot
    $startInfo.Environment["RagChallenge__Product__CatalogueProfile"] = "postgresql-18.4"
    $startInfo.Environment[
        "RagChallenge__Product__ApprovedRightsEvidenceReference"] =
        "auth-s07-a-product-a0-003"
    $startInfo.Environment[
        "RagChallenge__Product__CredentialEnvironmentVariable"] = "OPENAI_API_KEY"
    $startInfo.Environment[
        "RagChallenge__Product__QueryEmbeddingAuthorityReference"] =
        "AUTH-QUERY-EMBEDDING-RENDER-PACKAGE-READINESS"
    $startInfo.Environment[
        "RagChallenge__Product__GroundedGenerationAuthorityReference"] =
        "AUTH-GROUNDED-GENERATION-RENDER-PACKAGE-READINESS"
    $startInfo.Environment["Logging__LogLevel__Default"] = "Warning"

    $readinessProcess = [System.Diagnostics.Process]::Start($startInfo)
    try {
        $readinessOutputTask = $readinessProcess.StandardOutput.ReadToEndAsync()
        $readinessErrorTask = $readinessProcess.StandardError.ReadToEndAsync()
        $deadline = [System.DateTimeOffset]::UtcNow.AddSeconds(60)
        $readiness = $null
        $readinessResponse = $null

        do {
            if ($readinessProcess.HasExited) {
                throw "The package readiness probe exited before a bounded response."
            }

            try {
                $readinessResponse = Invoke-WebRequest `
                    -Uri "http://127.0.0.1:$readinessPort/api/v1/health/ready" `
                    -TimeoutSec 3 `
                    -SkipHttpErrorCheck
                $readiness = $readinessResponse.Content | ConvertFrom-Json -Depth 10
            }
            catch {
                Start-Sleep -Milliseconds 150
            }
        } while ($null -eq $readiness -and [System.DateTimeOffset]::UtcNow -lt $deadline)

        $readinessChecks = if ($null -eq $readiness) {
            @()
        }
        else {
            @($readiness.checks)
        }
        if ($null -eq $readiness -or
            $null -eq $readinessResponse -or
            [int]$readinessResponse.StatusCode -ne 503 -or
            $readiness.status -cne "Unready" -or
            $null -ne $readiness.activeGenerationId -or
            $readiness.activeDatabaseCount -ne 0 -or
            $readiness.eligibleDocumentCount -ne 0 -or
            $readiness.configurationRevision -cne "postgresql-18.4-product-v1" -or
            $readinessChecks.Count -ne 1 -or
            $readinessChecks[0].capability -cne "provider-budget" -or
            $readinessChecks[0].state -cne "Disarmed") {
            throw "The package fail-closed readiness identity diverged."
        }

        $livenessResponse = Invoke-WebRequest `
            -Uri "http://127.0.0.1:$readinessPort/api/v1/health/live" `
            -TimeoutSec 3 `
            -SkipHttpErrorCheck
        $liveness = $livenessResponse.Content | ConvertFrom-Json -Depth 10
        if ([int]$livenessResponse.StatusCode -ne 200 -or $liveness.status -cne "Live") {
            throw "The package liveness probe diverged."
        }
    }
    finally {
        if (-not $readinessProcess.HasExited) {
            $readinessProcess.Kill($true)
            if (-not $readinessProcess.WaitForExit(10000)) {
                throw "The package readiness process did not stop within ten seconds."
            }
        }

        $null = $readinessOutputTask.GetAwaiter().GetResult()
        $null = $readinessErrorTask.GetAwaiter().GetResult()
        $readinessProcess.Dispose()
    }

    Copy-Item -LiteralPath (Join-Path $deploymentRoot "Dockerfile") -Destination $contextRoot
    Copy-Item -LiteralPath (Join-Path $deploymentRoot "entrypoint.sh") -Destination $contextRoot
    Copy-Item -LiteralPath (Join-Path $deploymentRoot "render.yaml.template") `
        -Destination $resolvedOutputRoot

    $releaseFiles = @(Get-ChildItem -LiteralPath $releaseRoot -Recurse -File)
    $seedFiles = @(Get-ChildItem -LiteralPath $seedRoot -Recurse -File |
        Where-Object { $_.Name -cne "seed-manifest.sha256" })
    $packageManifest = [ordered]@{
        schemaVersion = 1
        source = [ordered]@{
            branch = $branch
            head = $head
            corpus = "4.19.5"
        }
        hosting = [ordered]@{
            provider = "render"
            workspacePlan = "hobby"
            servicePlan = "free"
            instances = 1
            persistentDisk = $false
            managedDatabase = $false
            autoDeploy = $false
        }
        product = [ordered]@{
            catalogueProfile = "postgresql-18.4"
            configurationRevision = "postgresql-18.4-product-v1"
            activeGenerationId = $expectedGeneration
            generationContentDigest = $expectedGenerationContentDigest
            logicalArtefactDigest = $expectedLogicalArtefactDigest
            preparedStoreSha256 = $preparedStoreSha256
            controlDbSha256 = $controlDbSha256
            vectorsDbSha256 = $vectorsDbSha256
            contentStructuralTreeSha256 = $contentStructuralTreeSha256
            expectedActiveDatabaseCount = 1
            expectedEligibleDocumentCount = 1
            answerEvidencePersistence = "ephemeral-per-process-lifetime"
            offlineAdministrativeStatusValidated = $true
            administrativeStatusResultCode = "CH_ADMIN_STATUS_AVAILABLE"
            administrativeStatusCorpusId = "rag-challenge-product"
            administrativeStatusRevision = $offlineStatusRevision
            failClosedReadinessValidated = $true
            providerBudgetState = "Disarmed"
            loopbackLivenessValidated = $true
        }
        release = [ordered]@{
            framework = "net10.0"
            containerRuntime = "linux-x64"
            baseImage = (
                "mcr.microsoft.com/dotnet/aspnet:10.0.11@" +
                "sha256:207cc51496778557731c81ff670333d8ade4a4fec22768fd1be8e78474a84ecf")
            files = $releaseFiles.Count
            bytes = [long](($releaseFiles | Measure-Object Length -Sum).Sum)
        }
        seed = [ordered]@{
            files = $seedFiles.Count
            bytes = [long](($seedFiles | Measure-Object Length -Sum).Sum)
            containsSourceBytes = $true
            publicDistributionAllowed = $false
        }
        externalActions = [ordered]@{
            dockerInvoked = $false
            imagePublished = $false
            renderContacted = $false
            providerQuerySubmitted = $false
            providerCredentialConfigured = $false
            trustedProviderGrantConfigured = $false
            egressObservationPerformed = $false
        }
    }
    [System.IO.File]::WriteAllText(
        (Join-Path $contextRoot "package-manifest.json"),
        (($packageManifest | ConvertTo-Json -Depth 10) + "`n"),
        $utf8WithoutBom)

    $contextManifestPath = Join-Path $contextRoot "context-manifest.sha256"
    $contextPaths = [string[]](Get-ChildItem -LiteralPath $contextRoot -Recurse -File |
        Where-Object { $_.FullName -cne $contextManifestPath } |
        ForEach-Object {
            [System.IO.Path]::GetRelativePath($contextRoot, $_.FullName).Replace('\', '/')
        })
    [System.Array]::Sort($contextPaths, [System.StringComparer]::Ordinal)
    $contextManifestLines = foreach ($relativePath in $contextPaths) {
        $fullPath = Join-Path $contextRoot $relativePath
        $digest = (Get-FileHash -LiteralPath $fullPath -Algorithm SHA256).
            Hash.ToLowerInvariant()
        $length = (Get-Item -LiteralPath $fullPath).Length
        "$digest  $length  $relativePath"
    }
    [System.IO.File]::WriteAllText(
        $contextManifestPath,
        ([string]::Join("`n", $contextManifestLines) + "`n"),
        $utf8WithoutBom)

    Remove-RenderFreePackageTransientDirectory `
        -OutputRoot $resolvedOutputRoot `
        -LeafName ".publish-raw"
    Remove-RenderFreePackageTransientDirectory `
        -OutputRoot $resolvedOutputRoot `
        -LeafName ".readiness-store"

    [pscustomobject]@{
        Status = "Prepared"
        OutputRoot = $resolvedOutputRoot
        SourceHead = $head
        ActiveGenerationId = $expectedGeneration
        ReleaseFiles = $releaseFiles.Count
        ReleaseBytes = [long](($releaseFiles | Measure-Object Length -Sum).Sum)
        SeedFiles = $seedFiles.Count
        SeedBytes = [long](($seedFiles | Measure-Object Length -Sum).Sum)
        ServicePlan = "free"
        DockerInvoked = $false
        ImagePublished = $false
        RenderContacted = $false
        ProviderCalled = $false
        CredentialRead = $false
    } | ConvertTo-Json -Compress
}
finally {
    Pop-Location
}
