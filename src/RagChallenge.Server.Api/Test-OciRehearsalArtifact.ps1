# Purpose: Verifies the offline STATE-06 Linux ARM64 rehearsal archive, manifest, native identities and fail-closed configuration without executing the ARM64 binary or contacting OCI.
[CmdletBinding()]
param(
    [string]$OutputRoot = "artifacts-local/s06-oci-rehearsal"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$serverRoot = $PSScriptRoot
$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $serverRoot "../.."))
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
    throw "The OCI rehearsal path must remain under artifacts-local."
}

$archiveName = "rag-challenge-s06-linux-arm64.zip"
$archivePath = Join-Path $resolvedOutput $archiveName
$archiveDigestPath = "$archivePath.sha256"

if (-not (Test-Path -LiteralPath $archivePath -PathType Leaf) -or
    -not (Test-Path -LiteralPath $archiveDigestPath -PathType Leaf)) {
    throw "The OCI rehearsal archive or its digest file is absent."
}

$digestRecord = (Get-Content -LiteralPath $archiveDigestPath -Raw).Trim()
$digestMatch = [System.Text.RegularExpressions.Regex]::Match(
    $digestRecord,
    "^(?<hash>[0-9a-f]{64})  rag-challenge-s06-linux-arm64[.]zip$")

if (-not $digestMatch.Success) {
    throw "The OCI rehearsal archive digest record is malformed."
}

$actualArchiveDigest = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash.ToLowerInvariant()

if ($actualArchiveDigest -cne $digestMatch.Groups["hash"].Value) {
    throw "The OCI rehearsal archive digest does not match its recorded value."
}

function Read-ArchiveEntryText {
    param([Parameter(Mandatory)]$Entry)

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
$archive = [System.IO.Compression.ZipFile]::OpenRead($archivePath)

try {
    $entries = @($archive.Entries | Where-Object { -not $_.FullName.EndsWith("/") })
    $entryNames = [string[]]@($entries.FullName)
    $sortedNames = [string[]]@($entryNames)
    [System.Array]::Sort($sortedNames, [System.StringComparer]::Ordinal)

    if (-not [System.Linq.Enumerable]::SequenceEqual[string]($entryNames, $sortedNames) -or
        @($entryNames | Select-Object -Unique).Count -ne $entryNames.Count) {
        throw "The OCI rehearsal archive is not sorted or contains duplicate entries."
    }

    $entriesByName = [System.Collections.Generic.Dictionary[string, object]]::new(
        [System.StringComparer]::Ordinal)

    foreach ($entry in $entries) {
        $entriesByName.Add($entry.FullName, $entry)

        if ($entry.FullName.StartsWith("/", [System.StringComparison]::Ordinal) -or
            $entry.FullName.Contains("\", [System.StringComparison]::Ordinal) -or
            $entry.FullName.Split('/') -contains "..") {
            throw "The OCI rehearsal archive contains an unsafe entry path."
        }

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

        $text = Read-ArchiveEntryText $entry

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
    $archive.Dispose()
}
