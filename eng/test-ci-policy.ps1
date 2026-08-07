# Purpose: Exercises the read-only CI policy checks with disposable local fixtures.
[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot "ci-policy.ps1")

function Invoke-ExpectedSuccess {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Name,

        [Parameter(Mandatory)]
        [scriptblock]$Action
    )

    & $Action
    Write-Output "PASS: $Name"
}

function Invoke-ExpectedFailure {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Name,

        [Parameter(Mandatory)]
        [scriptblock]$Action,

        [Parameter(Mandatory)]
        [string]$ExpectedPattern
    )

    $failure = $null

    try {
        & $Action
    }
    catch {
        $failure = $_.Exception.Message
    }

    if ($null -eq $failure) {
        throw "Case '$Name' unexpectedly succeeded."
    }

    if ($failure -notmatch $ExpectedPattern) {
        throw "Case '$Name' produced unexpected evidence: $failure"
    }

    Write-Output "PASS: $Name"
}

$temporaryBase = [System.IO.Path]::GetFullPath(
    [System.IO.Path]::GetTempPath())
$temporaryRoot = Join-Path $temporaryBase (
    "rag-challenge-ci-policy-tests-{0}" -f [guid]::NewGuid())
$temporaryRoot = [System.IO.Path]::GetFullPath($temporaryRoot)
$temporaryLeaf = Split-Path -Leaf $temporaryRoot

if (-not $temporaryRoot.StartsWith(
        $temporaryBase,
        [System.StringComparison]::OrdinalIgnoreCase) -or
    -not $temporaryLeaf.StartsWith(
        "rag-challenge-ci-policy-tests-",
        [System.StringComparison]::Ordinal)) {
    throw "The disposable test directory is outside the expected temporary root."
}

try {
    $null = New-Item -ItemType Directory -Path $temporaryRoot

    Invoke-ExpectedSuccess -Name "node-lower-bound" -Action {
        Assert-VersionSatisfiesRange `
            -Name "Node.js" `
            -ActualVersion "24.18.0" `
            -Range ">=24.18.0 <25"
    }

    Invoke-ExpectedSuccess -Name "node-compatible-patch" -Action {
        Assert-VersionSatisfiesRange `
            -Name "Node.js" `
            -ActualVersion "24.18.1" `
            -Range ">=24.18.0 <25"
    }

    Invoke-ExpectedSuccess -Name "npm-compatible-minor" -Action {
        Assert-VersionSatisfiesRange `
            -Name "npm" `
            -ActualVersion "11.17.0" `
            -Range ">=11.16.0 <12"
    }

    Invoke-ExpectedFailure `
        -Name "node-below-lower-bound" `
        -Action {
            Assert-VersionSatisfiesRange `
                -Name "Node.js" `
                -ActualVersion "24.17.9" `
                -Range ">=24.18.0 <25"
        } `
        -ExpectedPattern "outside the supported range"

    Invoke-ExpectedFailure `
        -Name "node-at-exclusive-upper-bound" `
        -Action {
            Assert-VersionSatisfiesRange `
                -Name "Node.js" `
                -ActualVersion "25.0.0" `
                -Range ">=24.18.0 <25"
        } `
        -ExpectedPattern "outside the supported range"

    Invoke-ExpectedFailure `
        -Name "malformed-version" `
        -Action {
            Assert-VersionSatisfiesRange `
                -Name "Node.js" `
                -ActualVersion "v24.18.0" `
                -Range ">=24.18.0 <25"
        } `
        -ExpectedPattern "invalid version"

    Invoke-ExpectedFailure `
        -Name "unsupported-range" `
        -Action {
            Assert-VersionSatisfiesRange `
                -Name "Node.js" `
                -ActualVersion "24.18.0" `
                -Range "^24.18.0"
        } `
        -ExpectedPattern "unsupported version policy"

    $lfPath = Join-Path $temporaryRoot "lf-packages.lock.json"
    $crlfPath = Join-Path $temporaryRoot "crlf-packages.lock.json"
    $missingPath = Join-Path $temporaryRoot "missing-packages.lock.json"
    [System.IO.File]::WriteAllBytes(
        $lfPath,
        [System.Text.Encoding]::UTF8.GetBytes("{`n}`n"))
    [System.IO.File]::WriteAllBytes(
        $crlfPath,
        [System.Text.Encoding]::UTF8.GetBytes("{`r`n}`r`n"))

    $lfHashBefore = (Get-FileHash -Algorithm SHA256 -LiteralPath $lfPath).Hash
    Invoke-ExpectedSuccess -Name "lf-file-remains-readable" -Action {
        Assert-FilesUseLfOnly -Paths @($lfPath)
    }
    $lfHashAfter = (Get-FileHash -Algorithm SHA256 -LiteralPath $lfPath).Hash

    if ($lfHashAfter -cne $lfHashBefore) {
        throw "LF validation modified its input file."
    }

    $crlfHashBefore = (Get-FileHash -Algorithm SHA256 -LiteralPath $crlfPath).Hash
    Invoke-ExpectedFailure `
        -Name "crlf-file-fails-closed" `
        -Action {
            Assert-FilesUseLfOnly -Paths @($crlfPath)
        } `
        -ExpectedPattern "contain carriage-return bytes"
    $crlfHashAfter = (Get-FileHash -Algorithm SHA256 -LiteralPath $crlfPath).Hash

    if ($crlfHashAfter -cne $crlfHashBefore) {
        throw "Failed LF validation modified its input file."
    }

    Invoke-ExpectedFailure `
        -Name "missing-file-fails-closed" `
        -Action {
            Assert-FilesUseLfOnly -Paths @($missingPath)
        } `
        -ExpectedPattern "does not exist"

    $ciScript = Get-Content -LiteralPath (Join-Path $PSScriptRoot "ci.ps1") -Raw
    $workflow = Get-Content -LiteralPath (
        Join-Path (Split-Path -Parent $PSScriptRoot) ".github/workflows/ci.yml") -Raw

    if ($ciScript -match 'Convert-NuGetLockFileLineEndings|WriteAllText|Normalised NuGet lockfiles') {
        throw "The CI entry point still contains lockfile rewriting behaviour."
    }

    if ($ciScript -notmatch 'Assert-FilesUseLfOnly') {
        throw "The CI entry point does not invoke the fail-closed LF policy."
    }

    $lockfilePolicyCalls = [regex]::Matches(
        $ciScript,
        '(?m)^\s*Assert-NuGetLockFileLineEndings\s*$').Count

    if ($lockfilePolicyCalls -ne 2) {
        throw "The CI entry point must validate lockfiles before and after restore."
    }

    if ($workflow -notmatch 'Assert-VersionSatisfiesRange' -or
        $workflow -notmatch '\$dashboardPackage\.engines\.node' -or
        $workflow -notmatch '\$dashboardPackage\.engines\.npm') {
        throw "The workflow does not validate the package engine ranges."
    }

    Write-Output "PASS: CI consumers use the shared fail-closed policy"
    Write-Output "All CI policy tests passed."
}
finally {
    if (Test-Path -LiteralPath $temporaryRoot) {
        Remove-Item -LiteralPath $temporaryRoot -Recurse -Force
    }

    if (Test-Path -LiteralPath $temporaryRoot) {
        throw "The disposable test directory could not be removed."
    }
}
