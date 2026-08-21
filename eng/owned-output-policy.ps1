# Purpose: Provides one exact marker-owned local artefact boundary so repository scripts cannot remove foreign data through configurable output paths.

$script:OwnedOutputMarkerName = ".rag-challenge-owned-output.json"
$script:OwnedOutputUtf8 = [System.Text.UTF8Encoding]::new($false)

function Get-OwnedOutputPathComparison {
    if ([System.OperatingSystem]::IsWindows()) {
        return [System.StringComparison]::OrdinalIgnoreCase
    }

    return [System.StringComparison]::Ordinal
}

function Assert-OwnedOutputToken {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Value,

        [Parameter(Mandatory)]
        [string]$Name
    )

    if ($Value.Length -gt 160 -or $Value -notmatch '^[A-Za-z0-9./-]+$') {
        throw "The owned-output $Name is invalid."
    }
}

function Get-OwnedOutputMarkerContent {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Purpose,

        [Parameter(Mandatory)]
        [string]$Owner,

        [Parameter(Mandatory)]
        [string]$CanonicalRelativePath
    )

    Assert-OwnedOutputToken -Value $Purpose -Name "purpose"
    Assert-OwnedOutputToken -Value $Owner -Name "owner"
    Assert-OwnedOutputToken -Value $CanonicalRelativePath -Name "canonical path"
    return (
        '{"schemaVersion":1,"purpose":"' + $Purpose + '",' +
        '"owner":"' + $Owner + '",' +
        '"canonicalPath":"' + $CanonicalRelativePath + '"}' + "`n")
}

function Assert-OwnedOutputExistingComponentsAreSafe {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$RepositoryRoot,

        [Parameter(Mandatory)]
        [string]$Path
    )

    $resolvedRepositoryRoot = [System.IO.Path]::GetFullPath($RepositoryRoot).TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar)
    $current = [System.IO.Path]::GetFullPath($Path).TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar)
    $comparison = Get-OwnedOutputPathComparison
    $repositoryPrefix = $resolvedRepositoryRoot + [System.IO.Path]::DirectorySeparatorChar

    if (-not [string]::Equals($current, $resolvedRepositoryRoot, $comparison) -and
        -not $current.StartsWith($repositoryPrefix, $comparison)) {
        throw "The owned output escaped its repository boundary."
    }

    while ($true) {
        $item = Get-Item -LiteralPath $current -Force -ErrorAction SilentlyContinue
        if ($null -ne $item -and
            (-not $item.PSIsContainer -or
                ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0)) {
            throw "The owned output path contains an unsafe component."
        }

        if ([string]::Equals($current, $resolvedRepositoryRoot, $comparison)) {
            break
        }

        $parent = [System.IO.Path]::GetDirectoryName($current)
        if ([string]::IsNullOrWhiteSpace($parent)) {
            throw "The owned output has no safe repository ancestor."
        }

        $current = $parent.TrimEnd(
            [System.IO.Path]::DirectorySeparatorChar,
            [System.IO.Path]::AltDirectorySeparatorChar)
    }
}

function Resolve-OwnedOutputRoot {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$RepositoryRoot,

        [Parameter(Mandatory)]
        [string]$RequestedOutputRoot,

        [Parameter(Mandatory)]
        [string]$CanonicalRelativePath
    )

    Assert-OwnedOutputToken -Value $CanonicalRelativePath -Name "canonical path"
    if ([System.IO.Path]::IsPathFullyQualified($CanonicalRelativePath) -or
        $CanonicalRelativePath.Split('/') -contains '..') {
        throw "The owned-output canonical path must be repository-relative."
    }

    $resolvedRepositoryRoot = [System.IO.Path]::GetFullPath($RepositoryRoot)
    $canonical = [System.IO.Path]::GetFullPath((Join-Path (
                $resolvedRepositoryRoot) $CanonicalRelativePath))
    $requested = if ([System.IO.Path]::IsPathFullyQualified($RequestedOutputRoot)) {
        [System.IO.Path]::GetFullPath($RequestedOutputRoot)
    }
    else {
        [System.IO.Path]::GetFullPath((Join-Path $resolvedRepositoryRoot $RequestedOutputRoot))
    }

    if (-not [string]::Equals(
            $requested,
            $canonical,
            (Get-OwnedOutputPathComparison))) {
        throw "The owned output must use its exact canonical path."
    }

    Assert-OwnedOutputExistingComponentsAreSafe `
        -RepositoryRoot $resolvedRepositoryRoot `
        -Path $canonical
    return $canonical
}

function Assert-OwnedOutputTreeIsSafe {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Root)

    $rootItem = Get-Item -LiteralPath $Root -Force
    if (-not $rootItem.PSIsContainer -or
        ($rootItem.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw "The owned output is unsafe and was preserved."
    }

    $pending = [System.Collections.Generic.Stack[string]]::new()
    $pending.Push($rootItem.FullName)
    $current = $null
    while ($pending.TryPop([ref]$current)) {
        foreach ($item in Get-ChildItem -LiteralPath $current -Force) {
            if (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
                throw "The owned output contains a reparse point and was preserved."
            }

            if ($item.PSIsContainer) {
                $pending.Push($item.FullName)
            }
        }
    }
}

function Assert-OwnedOutputRoot {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$OutputRoot,

        [Parameter(Mandatory)]
        [string]$Purpose,

        [Parameter(Mandatory)]
        [string]$Owner,

        [Parameter(Mandatory)]
        [string]$CanonicalRelativePath
    )

    if (-not (Test-Path -LiteralPath $OutputRoot -PathType Container)) {
        throw "The owned output is not a directory."
    }

    Assert-OwnedOutputTreeIsSafe -Root $OutputRoot
    $markerPath = Join-Path $OutputRoot $script:OwnedOutputMarkerName
    $markerItem = Get-Item -LiteralPath $markerPath -Force -ErrorAction SilentlyContinue
    $expectedBytes = $script:OwnedOutputUtf8.GetBytes((
            Get-OwnedOutputMarkerContent `
                -Purpose $Purpose `
                -Owner $Owner `
                -CanonicalRelativePath $CanonicalRelativePath))
    if ($null -eq $markerItem -or
        $markerItem.PSIsContainer -or
        ($markerItem.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0 -or
        $markerItem.Length -ne $expectedBytes.Length) {
        throw "The owned output marker is missing or invalid and the directory was preserved."
    }

    $actualBytes = [System.IO.File]::ReadAllBytes($markerPath)
    if (-not [System.Linq.Enumerable]::SequenceEqual(
            [byte[]]$actualBytes,
            [byte[]]$expectedBytes)) {
        throw "The owned output marker is missing or invalid and the directory was preserved."
    }
}

function New-OwnedOutputRoot {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$OutputRoot,

        [Parameter(Mandatory)]
        [string]$RepositoryRoot,

        [Parameter(Mandatory)]
        [string]$Purpose,

        [Parameter(Mandatory)]
        [string]$Owner,

        [Parameter(Mandatory)]
        [string]$CanonicalRelativePath
    )

    [System.IO.Directory]::CreateDirectory($OutputRoot) | Out-Null
    Assert-OwnedOutputExistingComponentsAreSafe `
        -RepositoryRoot $RepositoryRoot `
        -Path $OutputRoot
    $markerBytes = $script:OwnedOutputUtf8.GetBytes((
            Get-OwnedOutputMarkerContent `
                -Purpose $Purpose `
                -Owner $Owner `
                -CanonicalRelativePath $CanonicalRelativePath))
    $markerPath = Join-Path $OutputRoot $script:OwnedOutputMarkerName
    $stream = [System.IO.File]::Open(
        $markerPath,
        [System.IO.FileMode]::CreateNew,
        [System.IO.FileAccess]::Write,
        [System.IO.FileShare]::None)
    try {
        $stream.Write($markerBytes, 0, $markerBytes.Length)
        $stream.Flush($true)
    }
    finally {
        $stream.Dispose()
    }
}

function Reset-OwnedOutputRoot {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$RepositoryRoot,

        [Parameter(Mandatory)]
        [string]$RequestedOutputRoot,

        [Parameter(Mandatory)]
        [string]$CanonicalRelativePath,

        [Parameter(Mandatory)]
        [string]$Purpose,

        [Parameter(Mandatory)]
        [string]$Owner
    )

    $outputRoot = Resolve-OwnedOutputRoot `
        -RepositoryRoot $RepositoryRoot `
        -RequestedOutputRoot $RequestedOutputRoot `
        -CanonicalRelativePath $CanonicalRelativePath
    $existing = Get-Item -LiteralPath $outputRoot -Force -ErrorAction SilentlyContinue
    if ($null -ne $existing) {
        Assert-OwnedOutputRoot `
            -OutputRoot $outputRoot `
            -Purpose $Purpose `
            -Owner $Owner `
            -CanonicalRelativePath $CanonicalRelativePath
        Assert-OwnedOutputTreeIsSafe -Root $outputRoot
        Remove-Item -LiteralPath $outputRoot -Recurse -Force
        if ($null -ne (Get-Item -LiteralPath $outputRoot -Force -ErrorAction SilentlyContinue)) {
            throw "The owned output could not be removed."
        }
    }

    New-OwnedOutputRoot `
        -OutputRoot $outputRoot `
        -RepositoryRoot $RepositoryRoot `
        -Purpose $Purpose `
        -Owner $Owner `
        -CanonicalRelativePath $CanonicalRelativePath
    return $outputRoot
}

function Remove-OwnedOutputRoot {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$RepositoryRoot,

        [Parameter(Mandatory)]
        [string]$RequestedOutputRoot,

        [Parameter(Mandatory)]
        [string]$CanonicalRelativePath,

        [Parameter(Mandatory)]
        [string]$Purpose,

        [Parameter(Mandatory)]
        [string]$Owner
    )

    $outputRoot = Resolve-OwnedOutputRoot `
        -RepositoryRoot $RepositoryRoot `
        -RequestedOutputRoot $RequestedOutputRoot `
        -CanonicalRelativePath $CanonicalRelativePath
    if ($null -eq (Get-Item -LiteralPath $outputRoot -Force -ErrorAction SilentlyContinue)) {
        return
    }

    Assert-OwnedOutputRoot `
        -OutputRoot $outputRoot `
        -Purpose $Purpose `
        -Owner $Owner `
        -CanonicalRelativePath $CanonicalRelativePath
    Assert-OwnedOutputTreeIsSafe -Root $outputRoot
    Remove-Item -LiteralPath $outputRoot -Recurse -Force
}
