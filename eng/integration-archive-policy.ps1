# Purpose: Authenticates and bounds the local integration archive before any extraction or execution of its payload.

$script:IntegrationArchiveName = "rag-challenge-state07-v2-integration.zip"
$script:IntegrationManifestName = "artifact-manifest.sha256"
$script:IntegrationMaximumArchiveBytes = 768MB
$script:IntegrationMaximumEntries = 512
$script:IntegrationMaximumEntryBytes = 128MB
$script:IntegrationMaximumExpandedBytes = 768MB
$script:IntegrationMaximumCompressionRatio = 20
$script:IntegrationMaximumManifestBytes = 256KB
$script:IntegrationMaximumPathLength = 240
$script:IntegrationStrictUtf8 = [System.Text.UTF8Encoding]::new($false, $true)

function Assert-IntegrationArchiveRelativePath {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$RelativePath)

    $segments = $RelativePath.Split('/')
    if ($RelativePath.Length -gt $script:IntegrationMaximumPathLength -or
        $RelativePath -notmatch '^[A-Za-z0-9._+/-]+$' -or
        $RelativePath.StartsWith('/', [System.StringComparison]::Ordinal) -or
        $RelativePath.Contains('\', [System.StringComparison]::Ordinal) -or
        $segments.Count -eq 0 -or
        @($segments | Where-Object { $_.Length -eq 0 -or $_ -in '.', '..' }).Count -gt 0) {
        throw "The integration archive contains an unsafe entry path."
    }

    foreach ($segment in $segments) {
        if ($segment -match '^(?i:con|prn|aux|nul|com[1-9]|lpt[1-9])(?:[.]|$)') {
            throw "The integration archive contains an unsafe entry path."
        }
    }
}

function Get-IntegrationArchiveEntrySha256 {
    [CmdletBinding()]
    param([Parameter(Mandatory)]$Entry)

    $stream = $Entry.Open()
    try {
        return [System.Convert]::ToHexString(
            [System.Security.Cryptography.SHA256]::HashData($stream)).ToLowerInvariant()
    }
    finally {
        $stream.Dispose()
    }
}

function Read-IntegrationManifestText {
    [CmdletBinding()]
    param([Parameter(Mandatory)]$Entry)

    if ($Entry.Length -le 0 -or
        $Entry.Length -gt $script:IntegrationMaximumManifestBytes) {
        throw "The integration archive manifest exceeds its bounded size."
    }

    $bytes = [byte[]]::new([int]$Entry.Length)
    $stream = $Entry.Open()
    try {
        $offset = 0
        while ($offset -lt $bytes.Length) {
            $read = $stream.Read($bytes, $offset, $bytes.Length - $offset)
            if ($read -le 0) {
                throw "The integration archive manifest is truncated."
            }

            $offset += $read
        }

        if ($stream.ReadByte() -ne -1) {
            throw "The integration archive manifest exceeds its declared size."
        }
    }
    finally {
        $stream.Dispose()
    }

    return $script:IntegrationStrictUtf8.GetString($bytes)
}

function Assert-IntegrationExtractionTreeIsSafe {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Root)

    $rootItem = Get-Item -LiteralPath $Root -Force
    if (-not $rootItem.PSIsContainer -or
        ($rootItem.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw "The integration extraction root is unsafe."
    }

    foreach ($item in Get-ChildItem -LiteralPath $Root -Recurse -Force) {
        if (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
            throw "The integration extraction tree contains a reparse point."
        }
    }
}

function Invoke-TrustedIntegrationArchive {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$ArchivePath,

        [Parameter(Mandatory)]
        [string]$DigestPath,

        [Parameter(Mandatory)]
        [ValidatePattern('^[0-9a-f]{64}$')]
        [string]$ExpectedArchiveSha256,

        [string]$ExtractRoot
    )

    $archiveItem = Get-Item -LiteralPath $ArchivePath -Force -ErrorAction SilentlyContinue
    $digestItem = Get-Item -LiteralPath $DigestPath -Force -ErrorAction SilentlyContinue
    if ($null -eq $archiveItem -or
        $archiveItem.PSIsContainer -or
        ($archiveItem.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0 -or
        $archiveItem.Length -le 0 -or
        $archiveItem.Length -gt $script:IntegrationMaximumArchiveBytes -or
        $null -eq $digestItem -or
        $digestItem.PSIsContainer -or
        ($digestItem.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0 -or
        $digestItem.Length -le 0 -or
        $digestItem.Length -gt 160) {
        throw "The integration archive or digest record is absent or unsafe."
    }

    $expectedDigestRecord = $script:IntegrationStrictUtf8.GetBytes(
        "$ExpectedArchiveSha256  $script:IntegrationArchiveName`n")
    $actualDigestRecord = [System.IO.File]::ReadAllBytes($digestItem.FullName)
    if (-not [System.Linq.Enumerable]::SequenceEqual(
            [byte[]]$actualDigestRecord,
            [byte[]]$expectedDigestRecord)) {
        throw "The integration archive digest record does not match the trusted digest."
    }

    $archiveStream = [System.IO.File]::Open(
        $archiveItem.FullName,
        [System.IO.FileMode]::Open,
        [System.IO.FileAccess]::Read,
        [System.IO.FileShare]::Read)
    try {
        $actualArchiveSha256 = [System.Convert]::ToHexString(
            [System.Security.Cryptography.SHA256]::HashData($archiveStream)).ToLowerInvariant()
        if ($actualArchiveSha256 -cne $ExpectedArchiveSha256) {
            throw "The integration archive does not match the trusted digest."
        }

        $archiveStream.Position = 0
        $archive = [System.IO.Compression.ZipArchive]::new(
            $archiveStream,
            [System.IO.Compression.ZipArchiveMode]::Read,
            $true)
        try {
            $entries = @($archive.Entries)
            if ($entries.Count -lt 2 -or
                $entries.Count -gt $script:IntegrationMaximumEntries) {
                throw "The integration archive entry count exceeds its bounds."
            }

            $entriesByName = [System.Collections.Generic.Dictionary[string, object]]::new(
                [System.StringComparer]::Ordinal)
            $caseFoldedEntryNames = [System.Collections.Generic.HashSet[string]]::new(
                [System.StringComparer]::OrdinalIgnoreCase)
            $entryDigests = [System.Collections.Generic.Dictionary[string, string]]::new(
                [System.StringComparer]::Ordinal)
            $totalExpandedBytes = [long]0

            foreach ($entry in $entries) {
                Assert-IntegrationArchiveRelativePath -RelativePath $entry.FullName
                if ($entry.FullName.EndsWith('/', [System.StringComparison]::Ordinal) -or
                    $entry.ExternalAttributes -ne 0 -or
                    $entry.Length -lt 0 -or
                    $entry.CompressedLength -lt 0 -or
                    $entry.Length -gt $script:IntegrationMaximumEntryBytes -or
                    ($entry.Length -gt 0 -and $entry.CompressedLength -eq 0) -or
                    ($entry.CompressedLength -gt 0 -and
                        $entry.Length -gt
                        ($entry.CompressedLength * $script:IntegrationMaximumCompressionRatio)) -or
                    $totalExpandedBytes -gt
                    ($script:IntegrationMaximumExpandedBytes - $entry.Length)) {
                    throw "The integration archive contains an unsafe or oversized entry."
                }

                if ($entriesByName.ContainsKey($entry.FullName) -or
                    -not $caseFoldedEntryNames.Add($entry.FullName)) {
                    throw "The integration archive contains a duplicate entry."
                }

                $entriesByName.Add($entry.FullName, $entry)
                $totalExpandedBytes += $entry.Length
            }

            foreach ($requiredPath in @(
                    $script:IntegrationManifestName,
                    'RagChallenge.Server.Api.dll',
                    'appsettings.json',
                    'appsettings.Integration.json',
                    'wwwroot/index.html')) {
                if (-not $entriesByName.ContainsKey($requiredPath)) {
                    throw "The integration archive omits a required payload."
                }
            }

            $manifestText = Read-IntegrationManifestText `
                -Entry $entriesByName[$script:IntegrationManifestName]
            $manifestLines = @($manifestText -split "`n" |
                ForEach-Object { $_.TrimEnd("`r") } |
                Where-Object { $_.Length -gt 0 })
            $manifest = [System.Collections.Generic.Dictionary[string, object]]::new(
                [System.StringComparer]::Ordinal)

            foreach ($line in $manifestLines) {
                $match = [System.Text.RegularExpressions.Regex]::Match(
                    $line,
                    '^(?<hash>[0-9a-f]{64})  (?<size>[0-9]+)  (?<path>[A-Za-z0-9._+/-]+)$')
                if (-not $match.Success) {
                    throw "The integration archive manifest contains a malformed record."
                }

                $relativePath = $match.Groups['path'].Value
                Assert-IntegrationArchiveRelativePath -RelativePath $relativePath
                $byteLength = [long]0
                if ($relativePath -ceq $script:IntegrationManifestName -or
                    -not $entriesByName.ContainsKey($relativePath) -or
                    $manifest.ContainsKey($relativePath) -or
                    -not [long]::TryParse(
                        $match.Groups['size'].Value,
                        [System.Globalization.NumberStyles]::None,
                        [System.Globalization.CultureInfo]::InvariantCulture,
                        [ref]$byteLength) -or
                    $byteLength -lt 0 -or
                    $byteLength -gt $script:IntegrationMaximumEntryBytes) {
                    throw "The integration archive manifest contains an unsafe record."
                }

                $manifest.Add($relativePath, [pscustomobject]@{
                        Sha256 = $match.Groups['hash'].Value
                        ByteLength = $byteLength
                    })
            }

            if ($manifest.Count + 1 -ne $entries.Count) {
                throw "The integration archive manifest does not cover every payload exactly once."
            }

            foreach ($entry in $entries) {
                $entryDigest = Get-IntegrationArchiveEntrySha256 -Entry $entry
                $entryDigests.Add($entry.FullName, $entryDigest)
                if ($entry.FullName -cne $script:IntegrationManifestName) {
                    $record = $manifest[$entry.FullName]
                    if ($null -eq $record -or
                        $entry.Length -ne $record.ByteLength -or
                        $entryDigest -cne $record.Sha256) {
                        throw "The integration archive payload does not match its manifest."
                    }
                }
            }

            if (-not [string]::IsNullOrWhiteSpace($ExtractRoot)) {
                $resolvedExtractRoot = [System.IO.Path]::GetFullPath($ExtractRoot)
                if ($null -ne (Get-Item -LiteralPath $resolvedExtractRoot -Force -ErrorAction SilentlyContinue)) {
                    throw "The integration extraction root must not already exist."
                }

                $extractParent = [System.IO.Path]::GetDirectoryName($resolvedExtractRoot)
                $extractParentItem = Get-Item -LiteralPath $extractParent -Force
                if (-not $extractParentItem.PSIsContainer -or
                    ($extractParentItem.Attributes -band
                        [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
                    throw "The integration extraction parent is unsafe."
                }

                [System.IO.Directory]::CreateDirectory($resolvedExtractRoot) | Out-Null
                $extractPrefix = $resolvedExtractRoot.TrimEnd(
                    [System.IO.Path]::DirectorySeparatorChar,
                    [System.IO.Path]::AltDirectorySeparatorChar) +
                    [System.IO.Path]::DirectorySeparatorChar
                $copyBuffer = [byte[]]::new(1MB)

                foreach ($entry in $entries) {
                    $targetPath = [System.IO.Path]::GetFullPath((Join-Path (
                                $resolvedExtractRoot) $entry.FullName))
                    if (-not $targetPath.StartsWith(
                            $extractPrefix,
                            [System.StringComparison]::OrdinalIgnoreCase)) {
                        throw "The integration archive extraction escaped its root."
                    }

                    $targetParent = [System.IO.Path]::GetDirectoryName($targetPath)
                    [System.IO.Directory]::CreateDirectory($targetParent) | Out-Null
                    $sourceStream = $entry.Open()
                    $targetStream = [System.IO.File]::Open(
                        $targetPath,
                        [System.IO.FileMode]::CreateNew,
                        [System.IO.FileAccess]::Write,
                        [System.IO.FileShare]::None)
                    try {
                        $written = [long]0
                        while (($read = $sourceStream.Read(
                                    $copyBuffer,
                                    0,
                                    $copyBuffer.Length)) -gt 0) {
                            $written += $read
                            if ($written -gt $entry.Length) {
                                throw "The integration archive entry exceeded its declared size."
                            }

                            $targetStream.Write($copyBuffer, 0, $read)
                        }

                        if ($written -ne $entry.Length) {
                            throw "The integration archive entry was truncated during extraction."
                        }

                        $targetStream.Flush($true)
                    }
                    finally {
                        $targetStream.Dispose()
                        $sourceStream.Dispose()
                    }

                    $extractedDigest = (Get-FileHash `
                            -LiteralPath $targetPath `
                            -Algorithm SHA256).Hash.ToLowerInvariant()
                    if ($extractedDigest -cne $entryDigests[$entry.FullName]) {
                        throw "The extracted integration payload differs from the trusted archive."
                    }
                }

                Assert-IntegrationExtractionTreeIsSafe -Root $resolvedExtractRoot
            }

            return [pscustomobject]@{
                ArchiveSha256 = $actualArchiveSha256
                ServerAssemblySha256 = $entryDigests['RagChallenge.Server.Api.dll']
                Entries = $entries.Count
                ExpandedBytes = $totalExpandedBytes
                Extracted = -not [string]::IsNullOrWhiteSpace($ExtractRoot)
            }
        }
        finally {
            $archive.Dispose()
        }
    }
    finally {
        $archiveStream.Dispose()
    }
}

function Assert-TrustedIntegrationArchive {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$ArchivePath,
        [Parameter(Mandatory)][string]$DigestPath,
        [Parameter(Mandatory)][ValidatePattern('^[0-9a-f]{64}$')]
        [string]$ExpectedArchiveSha256
    )

    return Invoke-TrustedIntegrationArchive `
        -ArchivePath $ArchivePath `
        -DigestPath $DigestPath `
        -ExpectedArchiveSha256 $ExpectedArchiveSha256
}

function Expand-TrustedIntegrationArchive {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$ArchivePath,
        [Parameter(Mandatory)][string]$DigestPath,
        [Parameter(Mandatory)][ValidatePattern('^[0-9a-f]{64}$')]
        [string]$ExpectedArchiveSha256,
        [Parameter(Mandatory)][string]$ExtractRoot
    )

    return Invoke-TrustedIntegrationArchive `
        -ArchivePath $ArchivePath `
        -DigestPath $DigestPath `
        -ExpectedArchiveSha256 $ExpectedArchiveSha256 `
        -ExtractRoot $ExtractRoot
}
