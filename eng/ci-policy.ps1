# Purpose: Supplies read-only, fail-closed policy checks shared by the local CI entry point and its workflow.

function Assert-VersionSatisfiesRange {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Name,

        [Parameter(Mandatory)]
        [string]$ActualVersion,

        [Parameter(Mandatory)]
        [string]$Range
    )

    $rangePattern = '^>=(?<minimum>\d+\.\d+\.\d+) <(?<maximumMajor>\d+)$'
    $versionPattern = '^\d+\.\d+\.\d+$'

    if ($Range -notmatch $rangePattern) {
        throw "$Name has an unsupported version policy '$Range'."
    }

    $minimumText = $Matches.minimum
    $maximumMajor = [int]$Matches.maximumMajor

    if ($ActualVersion -notmatch $versionPattern) {
        throw "$Name reported an invalid version '$ActualVersion'."
    }

    $actual = [System.Version]$ActualVersion
    $minimum = [System.Version]$minimumText
    $maximum = [System.Version]::new($maximumMajor, 0, 0)

    if ($minimum -ge $maximum) {
        throw "$Name has an invalid version policy '$Range'."
    }

    if ($actual -lt $minimum -or $actual -ge $maximum) {
        throw "$Name version '$ActualVersion' is outside the supported range '$Range'."
    }
}

function Assert-FilesUseLfOnly {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string[]]$Paths
    )

    if ($Paths.Count -eq 0) {
        throw "No files were supplied for LF validation."
    }

    $invalidFiles = [System.Collections.Generic.List[string]]::new()

    foreach ($path in $Paths) {
        if (-not [System.IO.File]::Exists($path)) {
            throw "The required file '$path' does not exist."
        }

        $bytes = [System.IO.File]::ReadAllBytes($path)

        if ([System.Array]::IndexOf($bytes, [byte]13) -ge 0) {
            $invalidFiles.Add($path)
        }
    }

    if ($invalidFiles.Count -gt 0) {
        throw (
            "The following files contain carriage-return bytes and must already use LF: " +
            ($invalidFiles -join ", "))
    }
}

function Invoke-RequiredPolicyTest {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Name,

        [Parameter(Mandatory)]
        [string]$ScriptPath
    )

    if ([string]::IsNullOrWhiteSpace($Name)) {
        throw "A required policy test must have a name."
    }

    if (-not [System.IO.File]::Exists($ScriptPath)) {
        throw "Required policy test '$Name' does not exist at '$ScriptPath'."
    }

    try {
        & $ScriptPath
    }
    catch {
        throw [System.InvalidOperationException]::new(
            "Required policy test '$Name' failed: $($_.Exception.Message)",
            $_.Exception)
    }
}
