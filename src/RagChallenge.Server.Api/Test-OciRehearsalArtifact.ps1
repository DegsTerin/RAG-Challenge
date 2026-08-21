# Purpose: Verifies the offline STATE-06 Linux ARM64 rehearsal archive, manifest, native identities and fail-closed configuration without executing the ARM64 binary or contacting OCI.
[CmdletBinding()]
param(
    [string]$OutputRoot = "artifacts-local/s06-oci-rehearsal",
    [Parameter(Mandatory)]
    [ValidatePattern('^[0-9a-f]{64}$')]
    [string]$ExpectedArchiveSha256
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$serverRoot = $PSScriptRoot
$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $serverRoot "../.."))
. (Join-Path $repositoryRoot "eng/owned-output-policy.ps1")

$canonicalOutputPath = "artifacts-local/s06-oci-rehearsal"
$outputPurpose = "s06-oci-rehearsal-artifact"
$outputOwner = "src/RagChallenge.Server.Api/Build-OciRehearsalArtifact.ps1"
$maximumArchiveBytes = 256MB
$maximumArchiveEntries = 1024
$maximumEntryBytes = 64MB
$maximumExpandedBytes = 256MB
$maximumCompressionRatio = 20
$maximumManifestBytes = 256KB
$resolvedOutput = Resolve-OwnedOutputRoot `
    -RepositoryRoot $repositoryRoot `
    -RequestedOutputRoot $OutputRoot `
    -CanonicalRelativePath $canonicalOutputPath
Assert-OwnedOutputRoot `
    -OutputRoot $resolvedOutput `
    -Purpose $outputPurpose `
    -Owner $outputOwner `
    -CanonicalRelativePath $canonicalOutputPath

$archiveName = "rag-challenge-s06-linux-arm64.zip"
$archivePath = Join-Path $resolvedOutput $archiveName
$archiveDigestPath = "$archivePath.sha256"

if (-not (Test-Path -LiteralPath $archivePath -PathType Leaf) -or
    -not (Test-Path -LiteralPath $archiveDigestPath -PathType Leaf)) {
    throw "The OCI rehearsal archive or its digest file is absent."
}

$archiveItem = Get-Item -LiteralPath $archivePath -Force
$archiveDigestItem = Get-Item -LiteralPath $archiveDigestPath -Force
if (($archiveItem.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0 -or
    $archiveItem.Length -le 0 -or
    $archiveItem.Length -gt $maximumArchiveBytes -or
    ($archiveDigestItem.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0 -or
    $archiveDigestItem.Length -le 0 -or
    $archiveDigestItem.Length -gt 160) {
    throw "The OCI rehearsal archive or digest file exceeds its safe boundary."
}

$expectedDigestRecord = [System.Text.UTF8Encoding]::new($false, $true).GetBytes(
    "$ExpectedArchiveSha256  rag-challenge-s06-linux-arm64.zip`n")
$actualDigestRecord = [System.IO.File]::ReadAllBytes($archiveDigestPath)
if (-not [System.Linq.Enumerable]::SequenceEqual(
        [byte[]]$actualDigestRecord,
        [byte[]]$expectedDigestRecord)) {
    throw "The OCI rehearsal archive digest record does not match the trusted digest."
}

function Read-ArchiveEntryText {
    param(
        [Parameter(Mandatory)]$Entry,
        [ValidateRange(1, 4MB)]
        [long]$MaximumBytes = $maximumManifestBytes
    )

    if ($Entry.Length -le 0 -or $Entry.Length -gt $MaximumBytes) {
        throw "The OCI rehearsal text entry exceeds its bounded size."
    }

    $stream = $Entry.Open()
    $reader = [System.IO.StreamReader]::new(
        $stream,
        [System.Text.UTF8Encoding]::new($false, $true),
        $true,
        4096,
        $false)

    try {
        return $reader.ReadToEnd()
    }
    finally {
        $reader.Dispose()
    }
}

function Get-ArchiveEntrySha256 {
    param([Parameter(Mandatory)]$Entry)

    $stream = $Entry.Open()

    try {
        return [Convert]::ToHexString(
            [System.Security.Cryptography.SHA256]::HashData($stream)).ToLowerInvariant()
    }
    finally {
        $stream.Dispose()
    }
}

function Assert-AArch64Elf {
    param([Parameter(Mandatory)]$Entry)

    $stream = $Entry.Open()
    $header = [byte[]]::new(20)

    try {
        $read = $stream.Read($header, 0, $header.Length)
    }
    finally {
        $stream.Dispose()
    }

    $machine = $header[18] -bor ($header[19] -shl 8)

    if ($read -ne $header.Length -or
        $header[0] -ne 0x7f -or
        $header[1] -ne 0x45 -or
        $header[2] -ne 0x4c -or
        $header[3] -ne 0x46 -or
        $header[4] -ne 2 -or
        $header[5] -ne 1 -or
        $machine -ne 183) {
        throw "Archive entry '$($Entry.FullName)' is not ELF64 little-endian AArch64."
    }
}

Add-Type -AssemblyName System.IO.Compression
$archiveStream = [System.IO.File]::Open(
    $archivePath,
    [System.IO.FileMode]::Open,
    [System.IO.FileAccess]::Read,
    [System.IO.FileShare]::Read)
$archive = $null

try {
    $actualArchiveDigest = [System.Convert]::ToHexString(
        [System.Security.Cryptography.SHA256]::HashData($archiveStream)).ToLowerInvariant()
    if ($actualArchiveDigest -cne $ExpectedArchiveSha256) {
        throw "The OCI rehearsal archive does not match the trusted digest."
    }

    $archiveStream.Position = 0
    $archive = [System.IO.Compression.ZipArchive]::new(
        $archiveStream,
        [System.IO.Compression.ZipArchiveMode]::Read,
        $true)
    $entries = @($archive.Entries)
    if ($entries.Count -lt 2 -or
        $entries.Count -gt $maximumArchiveEntries -or
        @($entries | Where-Object { $_.FullName.EndsWith("/") }).Count -gt 0) {
        throw "The OCI rehearsal archive entry count or shape exceeds its bounds."
    }

    $entryNames = [string[]]@($entries.FullName)
    $sortedNames = [string[]]@($entryNames)
    [System.Array]::Sort($sortedNames, [System.StringComparer]::Ordinal)

    if (-not [System.Linq.Enumerable]::SequenceEqual[string]($entryNames, $sortedNames) -or
        @($entryNames | Select-Object -Unique).Count -ne $entryNames.Count) {
        throw "The OCI rehearsal archive is not sorted or contains duplicate entries."
    }

    $entriesByName = [System.Collections.Generic.Dictionary[string, object]]::new(
        [System.StringComparer]::Ordinal)
    $totalExpandedBytes = [long]0

    foreach ($entry in $entries) {
        $entriesByName.Add($entry.FullName, $entry)

        if ($entry.FullName.StartsWith("/", [System.StringComparison]::Ordinal) -or
            $entry.FullName.Contains("\", [System.StringComparison]::Ordinal) -or
            $entry.FullName.Split('/') -contains "..") {
            throw "The OCI rehearsal archive contains an unsafe entry path."
        }

        $unixFileType = ($entry.ExternalAttributes -shr 16) -band 0xf000
        if ($unixFileType -eq 0xa000 -or
            $entry.Length -lt 0 -or
            $entry.CompressedLength -lt 0 -or
            $entry.Length -gt $maximumEntryBytes -or
            ($entry.Length -gt 0 -and $entry.CompressedLength -eq 0) -or
            ($entry.CompressedLength -gt 0 -and
                $entry.Length -gt ($entry.CompressedLength * $maximumCompressionRatio)) -or
            $totalExpandedBytes -gt ($maximumExpandedBytes - $entry.Length)) {
            throw "The OCI rehearsal archive contains an unsafe or oversized entry."
        }

        $totalExpandedBytes += $entry.Length

        if ($entry.FullName -match "(?i)(^|/)runtimes/win-" -or
            $entry.FullName -match "(?i)[.](exe|com|cmd|bat|ps1)$") {
            throw "The OCI rehearsal archive contains a Windows-native or executable payload."
        }
    }

    if (-not $entriesByName.ContainsKey("artifact-manifest.sha256")) {
        throw "The OCI rehearsal archive does not contain its file manifest."
    }

    $manifestText = Read-ArchiveEntryText $entriesByName["artifact-manifest.sha256"]
    $manifestLines = @($manifestText -split "`n" |
        ForEach-Object { $_.TrimEnd("`r") } |
        Where-Object { $_.Length -gt 0 })
    $manifest = [System.Collections.Generic.Dictionary[string, object]]::new(
        [System.StringComparer]::Ordinal)
    $manifestPaths = [System.Collections.Generic.List[string]]::new()

    foreach ($line in $manifestLines) {
        $match = [System.Text.RegularExpressions.Regex]::Match(
            $line,
            "^(?<hash>[0-9a-f]{64})  (?<size>[0-9]+)  (?<path>.+)$")

        if (-not $match.Success) {
            throw "The OCI rehearsal manifest contains a malformed record."
        }

        $relative = $match.Groups["path"].Value

        if ($relative -ceq "artifact-manifest.sha256" -or
            $relative.StartsWith("/", [System.StringComparison]::Ordinal) -or
            $relative.Contains("\", [System.StringComparison]::Ordinal) -or
            $relative.Split('/') -contains ".." -or
            -not $entriesByName.ContainsKey($relative)) {
            throw "The OCI rehearsal manifest contains an unsafe or missing path."
        }

        $manifestPaths.Add($relative)
        $manifest.Add($relative, [pscustomobject]@{
            Hash = $match.Groups["hash"].Value
            Size = [long]::Parse(
                $match.Groups["size"].Value,
                [System.Globalization.CultureInfo]::InvariantCulture)
        })
    }

    $sortedManifestPaths = [string[]]@($manifestPaths)
    [System.Array]::Sort($sortedManifestPaths, [System.StringComparer]::Ordinal)

    if (-not [System.Linq.Enumerable]::SequenceEqual[string](
            [string[]]$manifestPaths,
            $sortedManifestPaths)) {
        throw "The OCI rehearsal manifest is not sorted by ordinal path."
    }

    if ($manifest.Count + 1 -ne $entries.Count) {
        throw "The OCI rehearsal manifest does not cover every payload entry exactly once."
    }

    foreach ($record in $manifest.GetEnumerator()) {
        $entry = $entriesByName[$record.Key]

        if ($entry.Length -ne $record.Value.Size -or
            (Get-ArchiveEntrySha256 $entry) -cne $record.Value.Hash) {
            throw "The OCI rehearsal manifest does not match '$($record.Key)'."
        }
    }

    foreach ($required in @(
            "RagChallenge.Server.Api",
            "libe_sqlite3.so",
            "wwwroot/index.html",
            "appsettings.json",
            "appsettings.Integration.json")) {
        if (-not $entriesByName.ContainsKey($required)) {
            throw "The OCI rehearsal archive omitted required entry '$required'."
        }
    }

    if (-not ($entryNames | Where-Object {
                $_.StartsWith("wwwroot/assets/", [System.StringComparison]::Ordinal)
            })) {
        throw "The OCI rehearsal archive omitted the compiled Dashboard assets."
    }

    $nativeEntries = @($entries | Where-Object {
            $_.FullName -ceq "RagChallenge.Server.Api" -or
            $_.FullName -ceq "createdump" -or
            $_.FullName.EndsWith(".so", [System.StringComparison]::Ordinal)
        })

    foreach ($entry in $nativeEntries) {
        Assert-AArch64Elf $entry
    }

    foreach ($requiredExecutable in @("RagChallenge.Server.Api", "libe_sqlite3.so")) {
        $mode = ($entriesByName[$requiredExecutable].ExternalAttributes -shr 16) -band 0xffff

        if (($mode -band 0x49) -eq 0) {
            throw "Required Linux entry '$requiredExecutable' lacks an archive execute bit."
        }
    }

    $settings = (Read-ArchiveEntryText $entriesByName["appsettings.json"]) |
        ConvertFrom-Json
    $integrationSettings = (
        Read-ArchiveEntryText $entriesByName["appsettings.Integration.json"]) |
        ConvertFrom-Json

    if ($settings.RagChallenge.Setup.AllowExternalServices -ne $false -or
        $settings.RagChallenge.Administration.Enabled -ne $false -or
        $integrationSettings.RagChallenge.Integration.Enabled -ne $false -or
        $integrationSettings.RagChallenge.Integration.StoreRoot -cne "") {
        throw "The OCI rehearsal archive does not retain fail-closed default configuration."
    }

    $textExtensions = @(
        ".config", ".css", ".html", ".js", ".json", ".md", ".sha256", ".txt", ".xml")

    foreach ($entry in $entries) {
        if ($entry.Length -gt 4MB -or
            $textExtensions -notcontains [System.IO.Path]::GetExtension($entry.FullName)) {
            continue
        }

        $text = Read-ArchiveEntryText -Entry $entry -MaximumBytes 4MB

        if ($text -match "-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----" -or
            $text -match '(?i)(?:api[_-]?key|client[_-]?secret|password|connectionstring)[\s"'']*[:=][\s"'']+[^"''\s]{4,}' -or
            $text -match "(?i)\b(?:sk|api)-[a-z0-9_-]{20,}\b") {
            throw "The OCI rehearsal archive contains an apparent secret assignment."
        }

        if ($text -match "(?i)[a-z]:[\\/](?:users|projects)[\\/]" -or
            $text -match "(?i)/(?:home|users)/[^/\s]+/") {
            throw "The OCI rehearsal archive contains an unsafe workstation path."
        }
    }

    [pscustomobject]@{
        Status = "Passed"
        Artefact = $archivePath
        Sha256 = $actualArchiveDigest
        ManifestFiles = $manifest.Count
        ArchiveFiles = $entries.Count
        NativeElfAArch64Files = $nativeEntries.Count
        DashboardCompiled = $true
        ExternalServicesEnabledByDefault = $false
        LinuxArm64Executed = $false
        OciContacted = $false
    } | ConvertTo-Json -Compress
}
finally {
    if ($null -ne $archive) {
        $archive.Dispose()
    }

    $archiveStream.Dispose()
}
