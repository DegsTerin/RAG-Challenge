# Purpose: Exercises trusted-digest, manifest, path, link and resource bounds before an integration archive can be extracted or executed.

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot "integration-archive-policy.ps1")

$temporaryParent = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath()).TrimEnd(
    [System.IO.Path]::DirectorySeparatorChar,
    [System.IO.Path]::AltDirectorySeparatorChar)
$temporaryRoot = Join-Path $temporaryParent (
    "rag-challenge-integration-archive-policy-{0}" -f [guid]::NewGuid())
$resolvedTemporaryRoot = [System.IO.Path]::GetFullPath($temporaryRoot)
if (-not [string]::Equals(
        [System.IO.Path]::GetDirectoryName($resolvedTemporaryRoot),
        $temporaryParent,
        [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "The integration archive policy fixture escaped the temporary directory."
}

[System.IO.Directory]::CreateDirectory($resolvedTemporaryRoot) | Out-Null
$fixtureUtf8 = [System.Text.UTF8Encoding]::new($false)

function Get-FixtureSha256 {
    param([Parameter(Mandatory)][byte[]]$Bytes)

    return [System.Convert]::ToHexString(
        [System.Security.Cryptography.SHA256]::HashData($Bytes)).ToLowerInvariant()
}

function New-ArchivePolicyFixture {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Name,
        [string]$UnsafeEntryName,
        [switch]$DuplicateEntry,
        [switch]$LinkAttributes,
        [switch]$ManifestMismatch,
        [ValidateRange(0, 600)][int]$ExtraEntryCount = 0,
        [ValidateRange(0, 4MB)][int]$CompressibleBytes = 0,
        [string]$ServerPayload = "fixture-server"
    )

    $fixtureRoot = Join-Path $resolvedTemporaryRoot $Name
    [System.IO.Directory]::CreateDirectory($fixtureRoot) | Out-Null
    $archivePath = Join-Path $fixtureRoot $script:IntegrationArchiveName
    $digestPath = "$archivePath.sha256"
    $payloads = [ordered]@{
        'RagChallenge.Server.Api.dll' = $fixtureUtf8.GetBytes($ServerPayload)
        'appsettings.json' = $fixtureUtf8.GetBytes("{}`n")
        'appsettings.Integration.json' = $fixtureUtf8.GetBytes("{}`n")
        'wwwroot/index.html' = $fixtureUtf8.GetBytes("<div id=`"root`"></div>`n")
    }

    if (-not [string]::IsNullOrWhiteSpace($UnsafeEntryName)) {
        $payloads[$UnsafeEntryName] = $fixtureUtf8.GetBytes("unsafe`n")
    }

    for ($index = 0; $index -lt $ExtraEntryCount; $index++) {
        $payloads[("extra/{0:D4}.bin" -f $index)] = [byte[]]@([byte]($index % 251))
    }

    if ($CompressibleBytes -gt 0) {
        $payloads['wwwroot/assets/compressible.bin'] = [byte[]]::new($CompressibleBytes)
    }

    $manifestLines = foreach ($payload in $payloads.GetEnumerator()) {
        "$(Get-FixtureSha256 -Bytes $payload.Value)  $($payload.Value.LongLength)  $($payload.Key)"
    }
    $manifestBytes = $fixtureUtf8.GetBytes(($manifestLines -join "`n") + "`n")

    if ($ManifestMismatch) {
        $payloads['RagChallenge.Server.Api.dll'] = $fixtureUtf8.GetBytes(
            "$ServerPayload-tampered-after-manifest")
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
            foreach ($payload in $payloads.GetEnumerator()) {
                $compression = if ($payload.Key -ceq 'wwwroot/assets/compressible.bin') {
                    [System.IO.Compression.CompressionLevel]::Optimal
                }
                else {
                    [System.IO.Compression.CompressionLevel]::NoCompression
                }
                $entry = $archive.CreateEntry($payload.Key, $compression)
                $entry.ExternalAttributes = if (
                    $LinkAttributes -and
                    $payload.Key -ceq 'RagChallenge.Server.Api.dll') {
                    -1610612736
                }
                else {
                    0
                }
                $entryStream = $entry.Open()
                try {
                    $entryStream.Write($payload.Value, 0, $payload.Value.Length)
                }
                finally {
                    $entryStream.Dispose()
                }
            }

            if ($DuplicateEntry) {
                $duplicate = $archive.CreateEntry(
                    'appsettings.json',
                    [System.IO.Compression.CompressionLevel]::NoCompression)
                $duplicate.ExternalAttributes = 0
                $duplicateStream = $duplicate.Open()
                try {
                    $duplicateBytes = $fixtureUtf8.GetBytes("duplicate`n")
                    $duplicateStream.Write($duplicateBytes, 0, $duplicateBytes.Length)
                }
                finally {
                    $duplicateStream.Dispose()
                }
            }

            $manifestEntry = $archive.CreateEntry(
                $script:IntegrationManifestName,
                [System.IO.Compression.CompressionLevel]::NoCompression)
            $manifestEntry.ExternalAttributes = 0
            $manifestStream = $manifestEntry.Open()
            try {
                $manifestStream.Write($manifestBytes, 0, $manifestBytes.Length)
            }
            finally {
                $manifestStream.Dispose()
            }
        }
        finally {
            $archive.Dispose()
        }
    }
    finally {
        $archiveStream.Dispose()
    }

    $archiveSha256 = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash.ToLowerInvariant()
    [System.IO.File]::WriteAllText(
        $digestPath,
        "$archiveSha256  $script:IntegrationArchiveName`n",
        $fixtureUtf8)
    return [pscustomobject]@{
        Root = $fixtureRoot
        ArchivePath = $archivePath
        DigestPath = $digestPath
        Sha256 = $archiveSha256
    }
}

function Invoke-ExpectedPolicyFailure {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$ExpectedPattern,
        [Parameter(Mandatory)][scriptblock]$Action,
        [string]$ForbiddenPath
    )

    $failedAsExpected = $false
    try {
        & $Action
    }
    catch {
        if ($_.Exception.Message -notmatch $ExpectedPattern) {
            throw "The '$Name' case failed for an unexpected reason."
        }

        $failedAsExpected = $true
    }

    if (-not $failedAsExpected) {
        throw "Expected '$Name' to fail closed."
    }

    if (-not [string]::IsNullOrWhiteSpace($ForbiddenPath) -and
        (Test-Path -LiteralPath $ForbiddenPath)) {
        throw "The '$Name' case created its forbidden extraction root."
    }

    Write-Output "PASS: $Name"
}

try {
    $valid = New-ArchivePolicyFixture -Name 'valid'
    $validation = Assert-TrustedIntegrationArchive `
        -ArchivePath $valid.ArchivePath `
        -DigestPath $valid.DigestPath `
        -ExpectedArchiveSha256 $valid.Sha256
    $extractRoot = Join-Path $valid.Root 'extracted'
    $expanded = Expand-TrustedIntegrationArchive `
        -ArchivePath $valid.ArchivePath `
        -DigestPath $valid.DigestPath `
        -ExpectedArchiveSha256 $valid.Sha256 `
        -ExtractRoot $extractRoot
    if ($validation.ArchiveSha256 -cne $valid.Sha256 -or
        $validation.ServerAssemblySha256 -cne
        (Get-FixtureSha256 -Bytes $fixtureUtf8.GetBytes('fixture-server')) -or
        -not $expanded.Extracted -or
        -not (Test-Path -LiteralPath (
                Join-Path $extractRoot 'RagChallenge.Server.Api.dll') -PathType Leaf)) {
        throw "The valid trusted integration archive did not validate and extract."
    }
    Write-Output "PASS: trusted archive validates and extracts"

    $tampered = New-ArchivePolicyFixture -Name 'tampered-archive'
    $append = [System.IO.File]::Open(
        $tampered.ArchivePath,
        [System.IO.FileMode]::Append,
        [System.IO.FileAccess]::Write,
        [System.IO.FileShare]::None)
    try {
        $append.WriteByte(0x41)
    }
    finally {
        $append.Dispose()
    }
    $tamperedExtractRoot = Join-Path $tampered.Root 'forbidden-extract'
    Invoke-ExpectedPolicyFailure `
        -Name 'tampered archive' `
        -ExpectedPattern 'does not match the trusted digest' `
        -ForbiddenPath $tamperedExtractRoot `
        -Action {
            Expand-TrustedIntegrationArchive `
                $tampered.ArchivePath `
                $tampered.DigestPath `
                $tampered.Sha256 `
                $tamperedExtractRoot
        }

    $forgedSidecar = New-ArchivePolicyFixture -Name 'forged-sidecar'
    $append = [System.IO.File]::Open(
        $forgedSidecar.ArchivePath,
        [System.IO.FileMode]::Append,
        [System.IO.FileAccess]::Write,
        [System.IO.FileShare]::None)
    try {
        $append.WriteByte(0x42)
    }
    finally {
        $append.Dispose()
    }
    $forgedDigest = (Get-FileHash `
            -LiteralPath $forgedSidecar.ArchivePath `
            -Algorithm SHA256).Hash.ToLowerInvariant()
    [System.IO.File]::WriteAllText(
        $forgedSidecar.DigestPath,
        "$forgedDigest  $script:IntegrationArchiveName`n",
        $fixtureUtf8)
    Invoke-ExpectedPolicyFailure `
        -Name 'archive and adjacent sidecar forged together' `
        -ExpectedPattern 'digest record does not match' `
        -Action {
            Assert-TrustedIntegrationArchive `
                $forgedSidecar.ArchivePath `
                $forgedSidecar.DigestPath `
                $forgedSidecar.Sha256
        }

    $rebuilt = New-ArchivePolicyFixture `
        -Name 'rebuilt-payload' `
        -ServerPayload 'attacker-rebuilt-server'
    Invoke-ExpectedPolicyFailure `
        -Name 'payload and manifests rebuilt without coordinator digest' `
        -ExpectedPattern 'digest record does not match' `
        -Action {
            Assert-TrustedIntegrationArchive $rebuilt.ArchivePath $rebuilt.DigestPath $valid.Sha256
        }

    $manifestMismatch = New-ArchivePolicyFixture -Name 'manifest-mismatch' -ManifestMismatch
    $manifestMismatchExtractRoot = Join-Path $manifestMismatch.Root 'forbidden-extract'
    Invoke-ExpectedPolicyFailure `
        -Name 'trusted archive with mismatched internal manifest' `
        -ExpectedPattern 'payload does not match its manifest' `
        -ForbiddenPath $manifestMismatchExtractRoot `
        -Action {
            Expand-TrustedIntegrationArchive `
                $manifestMismatch.ArchivePath `
                $manifestMismatch.DigestPath `
                $manifestMismatch.Sha256 `
                $manifestMismatchExtractRoot
        }

    $traversal = New-ArchivePolicyFixture -Name 'traversal' -UnsafeEntryName '../escape.bin'
    Invoke-ExpectedPolicyFailure `
        -Name 'path traversal entry' `
        -ExpectedPattern 'unsafe entry path' `
        -Action {
            Assert-TrustedIntegrationArchive $traversal.ArchivePath $traversal.DigestPath $traversal.Sha256
        }

    $reserved = New-ArchivePolicyFixture -Name 'reserved-device' -UnsafeEntryName 'CON'
    Invoke-ExpectedPolicyFailure `
        -Name 'reserved device entry' `
        -ExpectedPattern 'unsafe entry path' `
        -Action {
            Assert-TrustedIntegrationArchive $reserved.ArchivePath $reserved.DigestPath $reserved.Sha256
        }

    $duplicate = New-ArchivePolicyFixture -Name 'duplicate' -DuplicateEntry
    Invoke-ExpectedPolicyFailure `
        -Name 'duplicate entry' `
        -ExpectedPattern 'duplicate entry' `
        -Action {
            Assert-TrustedIntegrationArchive $duplicate.ArchivePath $duplicate.DigestPath $duplicate.Sha256
        }

    $link = New-ArchivePolicyFixture -Name 'link-attributes' -LinkAttributes
    Invoke-ExpectedPolicyFailure `
        -Name 'link-like entry attributes' `
        -ExpectedPattern 'unsafe or oversized entry' `
        -Action {
            Assert-TrustedIntegrationArchive $link.ArchivePath $link.DigestPath $link.Sha256
        }

    $tooMany = New-ArchivePolicyFixture -Name 'entry-count' -ExtraEntryCount 508
    Invoke-ExpectedPolicyFailure `
        -Name 'entry-count limit' `
        -ExpectedPattern 'entry count exceeds' `
        -Action {
            Assert-TrustedIntegrationArchive $tooMany.ArchivePath $tooMany.DigestPath $tooMany.Sha256
        }

    $compressed = New-ArchivePolicyFixture -Name 'compression-ratio' -CompressibleBytes 1MB
    Invoke-ExpectedPolicyFailure `
        -Name 'compression-ratio limit' `
        -ExpectedPattern 'unsafe or oversized entry' `
        -Action {
            Assert-TrustedIntegrationArchive $compressed.ArchivePath $compressed.DigestPath $compressed.Sha256
        }

    Write-Output "All trusted integration archive policy tests passed."
}
finally {
    if (Test-Path -LiteralPath $resolvedTemporaryRoot) {
        $finalRoot = [System.IO.Path]::GetFullPath($resolvedTemporaryRoot)
        if (-not [string]::Equals(
                [System.IO.Path]::GetDirectoryName($finalRoot),
                $temporaryParent,
                [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "The integration archive policy cleanup target escaped its temporary parent."
        }

        Remove-Item -LiteralPath $finalRoot -Recurse -Force
    }
}
