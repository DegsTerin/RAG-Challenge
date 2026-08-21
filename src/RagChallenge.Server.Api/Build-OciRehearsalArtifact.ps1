# Purpose: Cross-publishes the bounded STATE-06 Linux ARM64 rehearsal artefact from an already restored graph; it never restores, contacts OCI, or publishes externally.
[CmdletBinding()]
param(
    [string]$OutputRoot = "artifacts-local/s06-oci-rehearsal"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$serverRoot = $PSScriptRoot
$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $serverRoot "../.."))
. (Join-Path $repositoryRoot "eng/owned-output-policy.ps1")

$dashboardRoot = Join-Path $repositoryRoot "src/RagChallenge.Dashboard.Web"
$canonicalOutputPath = "artifacts-local/s06-oci-rehearsal"
$outputPurpose = "s06-oci-rehearsal-artifact"
$outputOwner = "src/RagChallenge.Server.Api/Build-OciRehearsalArtifact.ps1"
$resolvedOutput = Resolve-OwnedOutputRoot `
    -RepositoryRoot $repositoryRoot `
    -RequestedOutputRoot $OutputRoot `
    -CanonicalRelativePath $canonicalOutputPath

$assetsPath = Join-Path $serverRoot "obj/project.assets.json"

if (-not (Test-Path -LiteralPath $assetsPath -PathType Leaf)) {
    throw "The restored project assets required for linux-arm64 are absent."
}

$assets = Get-Content -LiteralPath $assetsPath -Raw | ConvertFrom-Json
$targets = @($assets.targets.PSObject.Properties.Name)

if ($targets -notcontains "net10.0/linux-arm64") {
    throw "The net10.0/linux-arm64 target must be restored before the rehearsal build."
}

$expectedDownloads = [ordered]@{
    "Microsoft.AspNetCore.App.Runtime.linux-arm64" = "[10.0.10, 10.0.10]"
    "Microsoft.NETCore.App.Host.linux-arm64" = "[10.0.10, 10.0.10]"
    "Microsoft.NETCore.App.Runtime.linux-arm64" = "[10.0.10, 10.0.10]"
}
$downloads = @($assets.project.frameworks.'net10.0'.downloadDependencies)

if ($downloads.Count -ne $expectedDownloads.Count) {
    throw "The linux-arm64 resolver closure contains an unexpected runtime pack."
}

foreach ($download in $downloads) {
    if (-not $expectedDownloads.Contains($download.name) -or
        $expectedDownloads[$download.name] -cne $download.version) {
        throw "The linux-arm64 resolver closure differs from the approved identity and version matrix."
    }
}

$contentRoot = Join-Path $resolvedOutput "content"
$archiveName = "rag-challenge-s06-linux-arm64.zip"
$archivePath = Join-Path $resolvedOutput $archiveName
$archiveDigestPath = "$archivePath.sha256"

$resolvedOutput = Reset-OwnedOutputRoot `
    -RepositoryRoot $repositoryRoot `
    -RequestedOutputRoot $OutputRoot `
    -CanonicalRelativePath $canonicalOutputPath `
    -Purpose $outputPurpose `
    -Owner $outputOwner

New-Item -ItemType Directory -Path $contentRoot -Force | Out-Null
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

Push-Location $repositoryRoot

try {
    dotnet publish "src/RagChallenge.Server.Api/RagChallenge.Server.Api.csproj" `
        --configuration Release `
        --runtime linux-arm64 `
        --self-contained true `
        --no-restore `
        --output $contentRoot `
        -p:ContinuousIntegrationBuild=true `
        -p:Deterministic=true `
        -p:DebugSymbols=false `
        -p:DebugType=None

    if ($LASTEXITCODE -ne 0) {
        throw "The Linux ARM64 server publish failed with exit code $LASTEXITCODE."
    }
}
finally {
    Pop-Location
}

$appHostPath = Join-Path $contentRoot "RagChallenge.Server.Api"
$sqlitePath = Join-Path $contentRoot "libe_sqlite3.so"

if (-not (Test-Path -LiteralPath $appHostPath -PathType Leaf) -or
    -not (Test-Path -LiteralPath $sqlitePath -PathType Leaf)) {
    throw "The self-contained publish omitted the app host or Linux ARM64 SQLite library."
}

$webRoot = Join-Path $contentRoot "wwwroot"
New-Item -ItemType Directory -Path $webRoot -Force | Out-Null
Copy-Item -Path (Join-Path $dashboardRoot "dist/*") -Destination $webRoot -Recurse -Force

$utf8WithoutBom = [System.Text.UTF8Encoding]::new($false)
$manifestPath = Join-Path $contentRoot "artifact-manifest.sha256"
$relativePaths = [string[]](Get-ChildItem -LiteralPath $contentRoot -Recurse -File |
    Where-Object { $_.FullName -ne $manifestPath } |
    ForEach-Object {
        [System.IO.Path]::GetRelativePath($contentRoot, $_.FullName).Replace('\', '/')
    })
[System.Array]::Sort($relativePaths, [System.StringComparer]::Ordinal)
$manifestLines = foreach ($relative in $relativePaths) {
    $fullPath = Join-Path $contentRoot $relative
    $digest = (Get-FileHash -LiteralPath $fullPath -Algorithm SHA256).Hash.ToLowerInvariant()
    "$digest  $((Get-Item -LiteralPath $fullPath).Length)  $relative"
}
[System.IO.File]::WriteAllText(
    $manifestPath,
    ([string]::Join("`n", $manifestLines) + "`n"),
    $utf8WithoutBom)

Add-Type -AssemblyName System.IO.Compression
$archiveStream = [System.IO.File]::Open(
    $archivePath,
    [System.IO.FileMode]::CreateNew,
    [System.IO.FileAccess]::Write,
    [System.IO.FileShare]::None)

try {
    $archive = [System.IO.Compression.ZipArchive]::new(
        $archiveStream,
        [System.IO.Compression.ZipArchiveMode]::Create,
        $false)

    try {
        $fixedInstant = [System.DateTimeOffset]::new(
            2000,
            1,
            1,
            0,
            0,
            0,
            [System.TimeSpan]::Zero)
        $archiveRelativePaths = [string[]](Get-ChildItem -LiteralPath $contentRoot -Recurse -File |
            ForEach-Object {
                [System.IO.Path]::GetRelativePath($contentRoot, $_.FullName).Replace('\', '/')
            })
        [System.Array]::Sort($archiveRelativePaths, [System.StringComparer]::Ordinal)

        foreach ($relative in $archiveRelativePaths) {
            $filePath = Join-Path $contentRoot $relative
            $entry = $archive.CreateEntry(
                $relative,
                [System.IO.Compression.CompressionLevel]::NoCompression)
            $entry.LastWriteTime = $fixedInstant
            $isExecutable = $relative -ceq "RagChallenge.Server.Api" -or
                $relative -ceq "createdump" -or
                $relative.EndsWith(".so", [System.StringComparison]::Ordinal)
            $unixMode = if ($isExecutable) { 0x81ed } else { 0x81a4 }
            $entry.ExternalAttributes = $unixMode -shl 16
            $entryStream = $entry.Open()
            $sourceStream = [System.IO.File]::OpenRead($filePath)

            try {
                $sourceStream.CopyTo($entryStream)
            }
            finally {
                $sourceStream.Dispose()
                $entryStream.Dispose()
            }
        }
    }
    finally {
        $archive.Dispose()
    }
}
finally {
    $archiveStream.Dispose()
}

$archiveDigest = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash.ToLowerInvariant()
[System.IO.File]::WriteAllText(
    $archiveDigestPath,
    "$archiveDigest  $archiveName`n",
    $utf8WithoutBom)

[pscustomobject]@{
    Artefact = $archivePath
    Sha256 = $archiveDigest
    Files = (Get-ChildItem -LiteralPath $contentRoot -Recurse -File).Count
    Bytes = (Get-Item -LiteralPath $archivePath).Length
    RuntimeIdentifier = "linux-arm64"
    SelfContained = $true
    LinuxArm64Executed = $false
} | ConvertTo-Json -Compress
