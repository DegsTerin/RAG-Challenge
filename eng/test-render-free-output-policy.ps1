# Purpose: Exercises the exact owned-output policy for the private Render Free package with disposable local fixtures and preservation checks.
[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot "render-free-output-policy.ps1")

function Invoke-ExpectedFailure {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [scriptblock]$Action,

        [Parameter(Mandatory)]
        [string]$ExpectedPattern
    )

    try {
        & $Action
    }
    catch {
        if ($_.Exception.Message -notmatch $ExpectedPattern) {
            throw "The policy failed with an unexpected message: $($_.Exception.Message)"
        }

        return
    }

    throw "The policy unexpectedly accepted an unsafe output."
}

function New-SyntheticRepository {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Name)

    $root = Join-Path $temporaryRoot $Name
    [System.IO.Directory]::CreateDirectory($root) | Out-Null
    return $root
}

$temporaryBase = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
$temporaryRoot = [System.IO.Path]::GetFullPath((Join-Path $temporaryBase (
            "rag-challenge-render-output-policy-tests-{0}" -f [guid]::NewGuid())))
$temporaryLeaf = Split-Path -Leaf $temporaryRoot
$temporaryPrefix = $temporaryBase.TrimEnd(
    [System.IO.Path]::DirectorySeparatorChar,
    [System.IO.Path]::AltDirectorySeparatorChar) +
    [System.IO.Path]::DirectorySeparatorChar

if (-not $temporaryRoot.StartsWith(
        $temporaryPrefix,
        [System.StringComparison]::OrdinalIgnoreCase) -or
    -not $temporaryLeaf.StartsWith(
        "rag-challenge-render-output-policy-tests-",
        [System.StringComparison]::Ordinal)) {
    throw "The disposable Render output fixture is outside its task-owned root."
}

$createdLinks = [System.Collections.Generic.List[string]]::new()

try {
    [System.IO.Directory]::CreateDirectory($temporaryRoot) | Out-Null

    $freshRepository = New-SyntheticRepository -Name "fresh"
    $freshOutput = Reset-RenderFreePackageOutput `
        -RepositoryRoot $freshRepository `
        -RequestedOutputRoot "artifacts-local/render-free-package"
    Assert-RenderFreePackageOwnedOutput -OutputRoot $freshOutput

    $stalePath = Join-Path $freshOutput "stale-sentinel.txt"
    [System.IO.File]::WriteAllText($stalePath, "stale", [System.Text.UTF8Encoding]::new($false))
    $replayedOutput = Reset-RenderFreePackageOutput `
        -RepositoryRoot $freshRepository `
        -RequestedOutputRoot $freshOutput
    if ((Test-Path -LiteralPath $stalePath) -or $replayedOutput -cne $freshOutput) {
        throw "An owned Render output replay did not replace only its canonical directory."
    }

    $foreignRepository = New-SyntheticRepository -Name "foreign"
    $foreignOutput = Join-Path $foreignRepository "artifacts-local/render-free-package"
    [System.IO.Directory]::CreateDirectory($foreignOutput) | Out-Null
    $foreignSentinel = Join-Path $foreignOutput "owner-data.txt"
    [System.IO.File]::WriteAllText(
        $foreignSentinel,
        "preserve-owner-data",
        [System.Text.UTF8Encoding]::new($false))
    Invoke-ExpectedFailure `
        -Action {
            Reset-RenderFreePackageOutput `
                -RepositoryRoot $foreignRepository `
                -RequestedOutputRoot $foreignOutput
        } `
        -ExpectedPattern "marker is missing"
    if ((Get-Content -LiteralPath $foreignSentinel -Raw) -cne "preserve-owner-data") {
        throw "A foreign Render output was not preserved byte-for-byte."
    }

    $corruptRepository = New-SyntheticRepository -Name "corrupt-marker"
    $corruptOutput = Reset-RenderFreePackageOutput `
        -RepositoryRoot $corruptRepository `
        -RequestedOutputRoot "artifacts-local/render-free-package"
    $corruptSentinel = Join-Path $corruptOutput "preserved.txt"
    [System.IO.File]::WriteAllText(
        $corruptSentinel,
        "preserved",
        [System.Text.UTF8Encoding]::new($false))
    [System.IO.File]::WriteAllText(
        (Join-Path $corruptOutput ".rag-challenge-owned-output.json"),
        "corrupt",
        [System.Text.UTF8Encoding]::new($false))
    Invoke-ExpectedFailure `
        -Action {
            Reset-RenderFreePackageOutput `
                -RepositoryRoot $corruptRepository `
                -RequestedOutputRoot $corruptOutput
        } `
        -ExpectedPattern "marker is missing or invalid"
    if (-not (Test-Path -LiteralPath $corruptSentinel -PathType Leaf)) {
        throw "A Render output with a corrupt marker was not preserved."
    }

    $invalidRepository = New-SyntheticRepository -Name "invalid-paths"
    foreach ($invalidRequest in @(
            "artifacts-local",
            "artifacts-local/render-free-package-extra",
            "artifacts-local/render-free-package/child",
            "../escape",
            (Join-Path $temporaryRoot "outside-output"))) {
        Invoke-ExpectedFailure `
            -Action {
                Reset-RenderFreePackageOutput `
                    -RepositoryRoot $invalidRepository `
                    -RequestedOutputRoot $invalidRequest
            } `
            -ExpectedPattern "exact canonical path"
    }

    $fileRepository = New-SyntheticRepository -Name "file-output"
    $fileOutput = Join-Path $fileRepository "artifacts-local/render-free-package"
    [System.IO.Directory]::CreateDirectory((Split-Path -Parent $fileOutput)) | Out-Null
    [System.IO.File]::WriteAllText(
        $fileOutput,
        "owner-file",
        [System.Text.UTF8Encoding]::new($false))
    Invoke-ExpectedFailure `
        -Action {
            Reset-RenderFreePackageOutput `
                -RepositoryRoot $fileRepository `
                -RequestedOutputRoot $fileOutput
        } `
        -ExpectedPattern "unsafe component"
    if ((Get-Content -LiteralPath $fileOutput -Raw) -cne "owner-file") {
        throw "A foreign file at the canonical leaf was not preserved."
    }

    $symlinkCasesSkipped = 0
    $leafLinkRepository = New-SyntheticRepository -Name "leaf-link"
    $leafLinkTarget = Join-Path $temporaryRoot "leaf-link-target"
    $leafLinkOutput = Join-Path $leafLinkRepository "artifacts-local/render-free-package"
    [System.IO.Directory]::CreateDirectory($leafLinkTarget) | Out-Null
    [System.IO.Directory]::CreateDirectory((Split-Path -Parent $leafLinkOutput)) | Out-Null
    [System.IO.File]::WriteAllText(
        (Join-Path $leafLinkTarget "owner-data.txt"),
        "linked-owner-data",
        [System.Text.UTF8Encoding]::new($false))
    $leafLinkCreated = $false
    try {
        $null = [System.IO.Directory]::CreateSymbolicLink($leafLinkOutput, $leafLinkTarget)
        $leafLinkCreated = $true
        $createdLinks.Add($leafLinkOutput)
    }
    catch [System.UnauthorizedAccessException] {
        $symlinkCasesSkipped++
    }
    catch [System.PlatformNotSupportedException] {
        $symlinkCasesSkipped++
    }
    catch [System.IO.IOException] {
        $symlinkCasesSkipped++
    }
    if ($leafLinkCreated) {
        Invoke-ExpectedFailure `
            -Action {
                Reset-RenderFreePackageOutput `
                    -RepositoryRoot $leafLinkRepository `
                    -RequestedOutputRoot $leafLinkOutput
            } `
            -ExpectedPattern "unsafe component"
        if ((Get-Content -LiteralPath (Join-Path $leafLinkTarget "owner-data.txt") -Raw) -cne
            "linked-owner-data") {
            throw "A leaf symlink target was not preserved."
        }
    }

    $parentLinkRepository = New-SyntheticRepository -Name "parent-link"
    $parentLinkTarget = Join-Path $temporaryRoot "parent-link-target"
    $parentLinkPath = Join-Path $parentLinkRepository "artifacts-local"
    [System.IO.Directory]::CreateDirectory($parentLinkTarget) | Out-Null
    [System.IO.File]::WriteAllText(
        (Join-Path $parentLinkTarget "owner-data.txt"),
        "parent-linked-owner-data",
        [System.Text.UTF8Encoding]::new($false))
    $parentLinkCreated = $false
    try {
        $null = [System.IO.Directory]::CreateSymbolicLink($parentLinkPath, $parentLinkTarget)
        $parentLinkCreated = $true
        $createdLinks.Add($parentLinkPath)
    }
    catch [System.UnauthorizedAccessException] {
        $symlinkCasesSkipped++
    }
    catch [System.PlatformNotSupportedException] {
        $symlinkCasesSkipped++
    }
    catch [System.IO.IOException] {
        $symlinkCasesSkipped++
    }
    if ($parentLinkCreated) {
        Invoke-ExpectedFailure `
            -Action {
                Reset-RenderFreePackageOutput `
                    -RepositoryRoot $parentLinkRepository `
                    -RequestedOutputRoot "artifacts-local/render-free-package"
            } `
            -ExpectedPattern "unsafe component"
        if ((Get-Content -LiteralPath (Join-Path $parentLinkTarget "owner-data.txt") -Raw) -cne
            "parent-linked-owner-data") {
            throw "A parent symlink target was not preserved."
        }
    }

    $nestedLinkRepository = New-SyntheticRepository -Name "nested-link"
    $nestedLinkOutput = Reset-RenderFreePackageOutput `
        -RepositoryRoot $nestedLinkRepository `
        -RequestedOutputRoot "artifacts-local/render-free-package"
    $nestedLinkTarget = Join-Path $temporaryRoot "nested-link-target"
    $nestedLinkPath = Join-Path $nestedLinkOutput "linked-owner-data"
    [System.IO.Directory]::CreateDirectory($nestedLinkTarget) | Out-Null
    [System.IO.File]::WriteAllText(
        (Join-Path $nestedLinkTarget "owner-data.txt"),
        "nested-linked-owner-data",
        [System.Text.UTF8Encoding]::new($false))
    $nestedLinkCreated = $false
    try {
        $null = [System.IO.Directory]::CreateSymbolicLink($nestedLinkPath, $nestedLinkTarget)
        $nestedLinkCreated = $true
        $createdLinks.Add($nestedLinkPath)
    }
    catch [System.UnauthorizedAccessException] {
        $symlinkCasesSkipped++
    }
    catch [System.PlatformNotSupportedException] {
        $symlinkCasesSkipped++
    }
    catch [System.IO.IOException] {
        $symlinkCasesSkipped++
    }
    if ($nestedLinkCreated) {
        Invoke-ExpectedFailure `
            -Action {
                Reset-RenderFreePackageOutput `
                    -RepositoryRoot $nestedLinkRepository `
                    -RequestedOutputRoot $nestedLinkOutput
            } `
            -ExpectedPattern "contains a reparse point"
        if ((Get-Content -LiteralPath (Join-Path $nestedLinkTarget "owner-data.txt") -Raw) -cne
            "nested-linked-owner-data") {
            throw "A nested symlink target was not preserved."
        }
    }

    Write-Output "PASS: Render Free output uses one marked canonical path and preserves foreign data"
    if ($symlinkCasesSkipped -ne 0) {
        Write-Output "SKIP: $symlinkCasesSkipped symlink case(s) were unavailable on this host"
    }
    Write-Output "All Render Free output policy tests passed."
}
finally {
    foreach ($linkPath in $createdLinks) {
        if (Test-Path -LiteralPath $linkPath) {
            $linkItem = Get-Item -LiteralPath $linkPath -Force
            if (($linkItem.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
                Remove-Item -LiteralPath $linkPath -Force
            }
        }
    }

    if (Test-Path -LiteralPath $temporaryRoot) {
        Remove-Item -LiteralPath $temporaryRoot -Recurse -Force
    }
}
