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

function Assert-UniqueJsonPropertyNames {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [System.Text.Json.JsonElement]$Element
    )

    if ($Element.ValueKind -eq [System.Text.Json.JsonValueKind]::Object) {
        $names = [System.Collections.Generic.HashSet[string]]::new(
            [System.StringComparer]::Ordinal)
        foreach ($property in $Element.EnumerateObject()) {
            if (-not $names.Add($property.Name)) {
                throw "The NuGet vulnerability audit JSON contains duplicate property names."
            }

            Assert-UniqueJsonPropertyNames -Element $property.Value
        }
    }
    elseif ($Element.ValueKind -eq [System.Text.Json.JsonValueKind]::Array) {
        foreach ($item in $Element.EnumerateArray()) {
            Assert-UniqueJsonPropertyNames -Element $item
        }
    }
}

function Assert-NuGetVulnerabilityAuditJson {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$AuditJson,

        [Parameter(Mandatory)]
        [string[]]$ExpectedProjectPaths
    )

    if ([string]::IsNullOrWhiteSpace($AuditJson) -or $AuditJson.Length -gt 8MB) {
        throw "The NuGet vulnerability audit returned empty or oversized JSON."
    }

    if ($ExpectedProjectPaths.Count -eq 0) {
        throw "The NuGet vulnerability audit requires an expected project set."
    }

    $jsonOptions = [System.Text.Json.JsonDocumentOptions]::new()
    $jsonOptions.AllowTrailingCommas = $false
    $jsonOptions.CommentHandling = [System.Text.Json.JsonCommentHandling]::Disallow
    $jsonOptions.MaxDepth = 32
    $jsonDocument = $null
    try {
        $jsonDocument = [System.Text.Json.JsonDocument]::Parse($AuditJson, $jsonOptions)
    }
    catch [System.Text.Json.JsonException] {
        throw [System.IO.InvalidDataException]::new(
            "The NuGet vulnerability audit returned malformed JSON.",
            $_.Exception)
    }

    try {
        Assert-UniqueJsonPropertyNames -Element $jsonDocument.RootElement
    }
    finally {
        $jsonDocument.Dispose()
    }

    try {
        $document = $AuditJson | ConvertFrom-Json -AsHashtable -Depth 32 -ErrorAction Stop
    }
    catch {
        throw [System.IO.InvalidDataException]::new(
            "The NuGet vulnerability audit returned malformed JSON.",
            $_.Exception)
    }

    if ($document -isnot [System.Collections.IDictionary] -or
        -not $document.Contains('version') -or
        $document['version'] -isnot [int] -and $document['version'] -isnot [long] -or
        [long]$document['version'] -ne 1 -or
        -not $document.Contains('parameters') -or
        $document['parameters'] -isnot [string] -or
        $document['parameters'] -cne '--vulnerable --include-transitive' -or
        -not $document.Contains('sources') -or
        $document['sources'] -isnot [object[]] -or
        @($document['sources']).Count -eq 0 -or
        -not $document.Contains('projects') -or
        $document['projects'] -isnot [object[]]) {
        throw "The NuGet vulnerability audit JSON has an unsupported root schema."
    }

    if ($document.Contains('problems')) {
        if ($document['problems'] -isnot [object[]]) {
            throw "The NuGet vulnerability audit JSON contains an invalid problem collection."
        }

        if (@($document['problems']).Count -gt 0) {
            throw "The NuGet vulnerability audit reported diagnostic problems."
        }
    }

    foreach ($source in @($document['sources'])) {
        $sourceUri = $null
        if ($source -isnot [string] -or
            -not [System.Uri]::TryCreate(
                $source,
                [System.UriKind]::Absolute,
                [ref]$sourceUri) -or
            $sourceUri.Scheme -cne [System.Uri]::UriSchemeHttps) {
            throw "The NuGet vulnerability audit JSON contains an invalid package source."
        }
    }

    $pathComparison = if ($IsWindows) {
        [System.StringComparer]::OrdinalIgnoreCase
    }
    else {
        [System.StringComparer]::Ordinal
    }
    $expectedProjects = [System.Collections.Generic.HashSet[string]]::new($pathComparison)
    foreach ($expectedPath in $ExpectedProjectPaths) {
        if ([string]::IsNullOrWhiteSpace($expectedPath) -or
            -not $expectedProjects.Add([System.IO.Path]::GetFullPath($expectedPath))) {
            throw "The expected NuGet audit project set is invalid or duplicated."
        }
    }

    $actualProjects = [System.Collections.Generic.HashSet[string]]::new($pathComparison)
    $vulnerabilityCount = 0
    foreach ($project in @($document['projects'])) {
        if ($project -isnot [System.Collections.IDictionary] -or
            -not $project.Contains('path') -or
            $project['path'] -isnot [string] -or
            [string]::IsNullOrWhiteSpace($project['path']) -or
            -not $actualProjects.Add([System.IO.Path]::GetFullPath($project['path']))) {
            throw "The NuGet vulnerability audit JSON contains an invalid project identity."
        }

        if (-not $project.Contains('frameworks')) {
            continue
        }

        if ($project['frameworks'] -isnot [object[]] -or
            @($project['frameworks']).Count -eq 0) {
            throw "The NuGet vulnerability audit JSON contains an invalid framework collection."
        }

        foreach ($framework in @($project['frameworks'])) {
            if ($framework -isnot [System.Collections.IDictionary] -or
                -not $framework.Contains('framework') -or
                $framework['framework'] -isnot [string] -or
                [string]::IsNullOrWhiteSpace($framework['framework'])) {
                throw "The NuGet vulnerability audit JSON contains an invalid framework."
            }

            $packageCount = 0
            foreach ($collectionName in @('topLevelPackages', 'transitivePackages')) {
                if (-not $framework.Contains($collectionName)) {
                    continue
                }

                $packages = $framework[$collectionName]
                if ($packages -isnot [object[]]) {
                    throw "The NuGet vulnerability audit JSON contains an invalid package collection."
                }

                foreach ($package in @($packages)) {
                    $packageCount++
                    if ($package -isnot [System.Collections.IDictionary] -or
                        -not $package.Contains('id') -or
                        $package['id'] -isnot [string] -or
                        [string]::IsNullOrWhiteSpace($package['id']) -or
                        -not $package.Contains('resolvedVersion') -or
                        $package['resolvedVersion'] -isnot [string] -or
                        [string]::IsNullOrWhiteSpace($package['resolvedVersion']) -or
                        -not $package.Contains('vulnerabilities') -or
                        $package['vulnerabilities'] -isnot [object[]] -or
                        @($package['vulnerabilities']).Count -eq 0) {
                        throw "The NuGet vulnerability audit JSON contains an invalid vulnerable package entry."
                    }

                    $vulnerabilityCount += @($package['vulnerabilities']).Count
                }
            }

            if ($packageCount -eq 0) {
                throw "The NuGet vulnerability audit JSON contains an empty reported framework."
            }
        }
    }

    if (-not $actualProjects.SetEquals($expectedProjects)) {
        throw "The NuGet vulnerability audit JSON does not cover the exact expected project set."
    }

    if ($vulnerabilityCount -gt 0) {
        throw "The NuGet dependency audit reported $vulnerabilityCount vulnerability entries."
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
