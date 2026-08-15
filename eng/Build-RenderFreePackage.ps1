# Purpose: Builds a private Render Free Docker context from the activated PostgreSQL product store without publishing, restoring or reading credentials.
[CmdletBinding()]
param(
    [string]$OutputRoot = "artifacts-local/render-free-package"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$allowedOutputRoot = [System.IO.Path]::GetFullPath(
    (Join-Path $repositoryRoot "artifacts-local"))
$resolvedOutputRoot = if ([System.IO.Path]::IsPathFullyQualified($OutputRoot)) {
    [System.IO.Path]::GetFullPath($OutputRoot)
}
else {
    [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $OutputRoot))
}
$allowedOutputPrefix = $allowedOutputRoot.TrimEnd(
    [System.IO.Path]::DirectorySeparatorChar,
    [System.IO.Path]::AltDirectorySeparatorChar) +
    [System.IO.Path]::DirectorySeparatorChar

if (-not $resolvedOutputRoot.StartsWith(
        $allowedOutputPrefix,
        [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "The Render Free package output must remain under artifacts-local."
}

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
    "idxgen-ec39244b021c90fceea1b3a628fe793a99f74650cad451f16ffbcd414af636f6"
$expectedOpenApiV1 =
    "d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34"
$expectedOpenApiV2 =
    "f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733"

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

    if (Test-Path -LiteralPath $resolvedOutputRoot) {
        $verifiedExistingOutput = [System.IO.Path]::GetFullPath($resolvedOutputRoot)
        if (-not $verifiedExistingOutput.StartsWith(
                $allowedOutputPrefix,
                [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "The existing Render Free package path failed containment validation."
        }

        Remove-Item -LiteralPath $verifiedExistingOutput -Recurse -Force
    }

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

    Copy-Item -LiteralPath (Join-Path $expectedStoreRoot "control.db") -Destination $seedRoot
    Copy-Item -LiteralPath (Join-Path $expectedStoreRoot "vectors.db") -Destination $seedRoot
    Copy-Item -LiteralPath (Join-Path $expectedStoreRoot "content") -Destination $seedRoot -Recurse

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
        $deadline = [System.DateTimeOffset]::UtcNow.AddSeconds(60)
        $readiness = $null

        do {
            if ($readinessProcess.HasExited) {
                $sanitisedError = $readinessProcess.StandardError.ReadToEnd().Trim()
                throw "The package readiness probe exited early: $sanitisedError"
            }

            try {
                $readiness = Invoke-RestMethod `
                    -Uri "http://127.0.0.1:$readinessPort/api/v1/health/ready" `
                    -TimeoutSec 3
            }
            catch {
                Start-Sleep -Milliseconds 150
            }
        } while ($null -eq $readiness -and [System.DateTimeOffset]::UtcNow -lt $deadline)

        if ($null -eq $readiness -or
            $readiness.status -cne "Ready" -or
            $readiness.activeGenerationId -cne $expectedGeneration -or
            $readiness.activeDatabaseCount -ne 1 -or
            $readiness.eligibleDocumentCount -ne 1 -or
            $readiness.configurationRevision -cne "postgresql-18.4-product-v1") {
            throw "The package readiness identity diverged."
        }

        $liveness = Invoke-RestMethod `
            -Uri "http://127.0.0.1:$readinessPort/api/v1/health/live" `
            -TimeoutSec 3
        if ($liveness.status -cne "Live") {
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
            corpus = "4.10.40"
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
            expectedActiveDatabaseCount = 1
            expectedEligibleDocumentCount = 1
            answerEvidencePersistence = "ephemeral-per-process-lifetime"
            loopbackReadinessValidated = $true
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
            providerCalled = $false
            credentialRead = $false
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

    $verifiedRawPublishRoot = [System.IO.Path]::GetFullPath($rawPublishRoot)
    if (-not $verifiedRawPublishRoot.StartsWith(
            $allowedOutputPrefix,
            [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "The temporary publish path failed containment validation."
    }
    Remove-Item -LiteralPath $verifiedRawPublishRoot -Recurse -Force
    $verifiedReadinessStoreRoot = [System.IO.Path]::GetFullPath($readinessStoreRoot)
    if (-not $verifiedReadinessStoreRoot.StartsWith(
            $allowedOutputPrefix,
            [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "The temporary readiness store path failed containment validation."
    }
    Remove-Item -LiteralPath $verifiedReadinessStoreRoot -Recurse -Force

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
