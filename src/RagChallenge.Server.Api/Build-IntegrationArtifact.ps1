# Purpose: Builds the reproducible local STATE-07 v2 integration artefact from the existing offline installation; it never restores dependencies, contacts a network, or publishes externally.
[CmdletBinding()]
param(
    [string]$OutputRoot = "artifacts-local/state07-v2-integration"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$serverRoot = $PSScriptRoot
$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $serverRoot "../.."))
$dashboardRoot = Join-Path $repositoryRoot "src/RagChallenge.Dashboard.Web"
$allowedRoot = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot "artifacts-local"))
$resolvedOutput = if ([System.IO.Path]::IsPathFullyQualified($OutputRoot)) {
    [System.IO.Path]::GetFullPath($OutputRoot)
}
else {
    [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $OutputRoot))
}
$allowedPrefix = $allowedRoot.TrimEnd(
    [System.IO.Path]::DirectorySeparatorChar,
    [System.IO.Path]::AltDirectorySeparatorChar) +
    [System.IO.Path]::DirectorySeparatorChar

if (-not $resolvedOutput.StartsWith(
        $allowedPrefix,
        [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "The integration artefact output must remain under artifacts-local."
}

$contentRoot = Join-Path $resolvedOutput "content"
$archivePath = Join-Path $resolvedOutput "rag-challenge-state07-v2-integration.zip"
$archiveDigestPath = Join-Path $resolvedOutput "rag-challenge-state07-v2-integration.zip.sha256"

if (Test-Path -LiteralPath $resolvedOutput) {
    $verifiedOutput = [System.IO.Path]::GetFullPath($resolvedOutput)

    if (-not $verifiedOutput.StartsWith(
            $allowedPrefix,
            [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "The existing integration artefact path failed containment validation."
    }

    Remove-Item -LiteralPath $verifiedOutput -Recurse -Force
}

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
        --no-restore `
        --output $contentRoot `
        -p:UseAppHost=false

    if ($LASTEXITCODE -ne 0) {
        throw "The server publish failed with exit code $LASTEXITCODE."
    }
}
finally {
    Pop-Location
}

$webRoot = Join-Path $contentRoot "wwwroot"
New-Item -ItemType Directory -Path $webRoot -Force | Out-Null
Copy-Item -Path (Join-Path $dashboardRoot "dist/*") -Destination $webRoot -Recurse -Force

$utf8WithoutBom = [System.Text.UTF8Encoding]::new($false)
$manifestPath = Join-Path $contentRoot "artifact-manifest.sha256"
$manifestLines = Get-ChildItem -LiteralPath $contentRoot -Recurse -File |
    Where-Object { $_.FullName -ne $manifestPath } |
    Sort-Object { [System.IO.Path]::GetRelativePath($contentRoot, $_.FullName) } |
    ForEach-Object {
        $relative = [System.IO.Path]::GetRelativePath($contentRoot, $_.FullName).Replace('\', '/')
        $digest = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
        "$digest  $($_.Length)  $relative"
    }
[System.IO.File]::WriteAllLines($manifestPath, $manifestLines, $utf8WithoutBom)

Add-Type -AssemblyName System.IO.Compression
if (Test-Path -LiteralPath $archivePath) {
    Remove-Item -LiteralPath $archivePath -Force
}

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
        $files = Get-ChildItem -LiteralPath $contentRoot -Recurse -File |
            Sort-Object { [System.IO.Path]::GetRelativePath($contentRoot, $_.FullName) }

        foreach ($file in $files) {
            $relative = [System.IO.Path]::GetRelativePath(
                $contentRoot,
                $file.FullName).Replace('\', '/')
            $entry = $archive.CreateEntry(
                $relative,
                [System.IO.Compression.CompressionLevel]::NoCompression)
            $entry.LastWriteTime = $fixedInstant
            $entry.ExternalAttributes = 0
            $entryStream = $entry.Open()
            $sourceStream = [System.IO.File]::OpenRead($file.FullName)

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
    "$archiveDigest  rag-challenge-state07-v2-integration.zip`n",
    $utf8WithoutBom)

[pscustomobject]@{
    Artefact = $archivePath
    Sha256 = $archiveDigest
    Files = (Get-ChildItem -LiteralPath $contentRoot -Recurse -File).Count
    Bytes = (Get-Item -LiteralPath $archivePath).Length
} | ConvertTo-Json -Compress
