# Purpose: Specialises the shared marker-owned output policy for the one exact private Render Free package and its transient build directories.

. (Join-Path $PSScriptRoot "owned-output-policy.ps1")

$script:RenderFreePackageCanonicalPath = "artifacts-local/render-free-package"
$script:RenderFreePackagePurpose = "render-free-package"
$script:RenderFreePackageOwner = "eng/Build-RenderFreePackage.ps1"

function Resolve-RenderFreePackageOutputRoot {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$RepositoryRoot,

        [Parameter(Mandatory)]
        [string]$RequestedOutputRoot
    )

    return Resolve-OwnedOutputRoot `
        -RepositoryRoot $RepositoryRoot `
        -RequestedOutputRoot $RequestedOutputRoot `
        -CanonicalRelativePath $script:RenderFreePackageCanonicalPath
}

function Assert-RenderFreePackageTreeIsSafe {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Root)

    Assert-OwnedOutputTreeIsSafe -Root $Root
}

function Assert-RenderFreePackageOwnedOutput {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$OutputRoot)

    Assert-OwnedOutputRoot `
        -OutputRoot $OutputRoot `
        -Purpose $script:RenderFreePackagePurpose `
        -Owner $script:RenderFreePackageOwner `
        -CanonicalRelativePath $script:RenderFreePackageCanonicalPath
}

function Reset-RenderFreePackageOutput {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$RepositoryRoot,

        [Parameter(Mandatory)]
        [string]$RequestedOutputRoot
    )

    return Reset-OwnedOutputRoot `
        -RepositoryRoot $RepositoryRoot `
        -RequestedOutputRoot $RequestedOutputRoot `
        -CanonicalRelativePath $script:RenderFreePackageCanonicalPath `
        -Purpose $script:RenderFreePackagePurpose `
        -Owner $script:RenderFreePackageOwner
}

function Remove-RenderFreePackageTransientDirectory {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$OutputRoot,

        [Parameter(Mandatory)]
        [ValidateSet(".publish-raw", ".readiness-store")]
        [string]$LeafName
    )

    Assert-RenderFreePackageOwnedOutput -OutputRoot $OutputRoot
    $child = [System.IO.Path]::GetFullPath((Join-Path $OutputRoot $LeafName))
    if ((Split-Path -Leaf $child) -cne $LeafName -or
        -not [string]::Equals(
            [System.IO.Path]::GetDirectoryName($child),
            [System.IO.Path]::GetFullPath($OutputRoot),
            (Get-OwnedOutputPathComparison))) {
        throw "The Render Free transient path failed its exact identity check."
    }

    if ($null -ne (Get-Item -LiteralPath $child -Force -ErrorAction SilentlyContinue)) {
        Assert-OwnedOutputTreeIsSafe -Root $child
        Remove-Item -LiteralPath $child -Recurse -Force
    }
}
