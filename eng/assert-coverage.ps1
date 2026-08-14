# Purpose: Merges Cobertura line and branch observations across test projects and enforces the initial repository floors.
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$ResultsDirectory,

    [double]$MinimumLineRate = 0.70,

    [double]$MinimumBranchRate = 0.45
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$reports = @(Get-ChildItem `
    -LiteralPath $ResultsDirectory `
    -Recurse `
    -Filter "coverage.cobertura.xml")

if ($reports.Count -eq 0) {
    throw "No Cobertura reports were found in '$ResultsDirectory'."
}

$lineHits = @{}
$branchCoverage = @{}

foreach ($report in $reports) {
    [xml]$document = Get-Content -LiteralPath $report.FullName -Raw
    $reportLineCount = 0

    foreach ($class in $document.SelectNodes(
        "/coverage/packages/package/classes/class")) {
        $file = ([string]$class.filename).Replace("\", "/")

        if ([string]::IsNullOrWhiteSpace($file)) {
            throw "Cobertura report '$($report.FullName)' contains a class without a filename."
        }

        foreach ($line in $class.SelectNodes("lines/line")) {
            [long]$lineNumber = 0
            [long]$hits = 0

            if (-not [long]::TryParse(
                [string]$line.number,
                [ref]$lineNumber) -or
                $lineNumber -le 0) {
                throw "Cobertura report '$($report.FullName)' contains an invalid line number for '$file'."
            }

            if (-not [long]::TryParse(
                [string]$line.hits,
                [ref]$hits) -or
                $hits -lt 0) {
                throw "Cobertura report '$($report.FullName)' contains invalid hit data for '${file}:$lineNumber'."
            }

            $reportLineCount++
            $lineKey = "${file}:$lineNumber"

            if (-not $lineHits.ContainsKey($lineKey) -or
                $hits -gt $lineHits[$lineKey]) {
                $lineHits[$lineKey] = $hits
            }

            $isBranch = $false

            if ($line.HasAttribute("branch") -and
                -not [bool]::TryParse(
                    $line.GetAttribute("branch"),
                    [ref]$isBranch)) {
                throw "Cobertura report '$($report.FullName)' contains invalid branch metadata for '$lineKey'."
            }

            if (-not $isBranch) {
                continue
            }

            $match = [regex]::Match(
                $line.GetAttribute("condition-coverage"),
                "^\s*(?:\d+(?:\.\d+)?%\s*)?\((\d+)/(\d+)\)\s*$")

            if (-not $match.Success) {
                throw "Cobertura report '$($report.FullName)' contains malformed branch coverage for '$lineKey'."
            }

            [long]$covered = 0
            [long]$valid = 0

            if (-not [long]::TryParse(
                $match.Groups[1].Value,
                [ref]$covered) -or
                -not [long]::TryParse(
                    $match.Groups[2].Value,
                    [ref]$valid) -or
                $valid -le 0 -or
                $covered -lt 0 -or
                $covered -gt $valid) {
                throw "Cobertura report '$($report.FullName)' contains impossible branch coverage for '$lineKey'."
            }

            if ($branchCoverage.ContainsKey($lineKey) -and
                $valid -ne $branchCoverage[$lineKey].Valid) {
                throw "Cobertura reports contain inconsistent branch totals for '$lineKey'."
            }

            if (-not $branchCoverage.ContainsKey($lineKey) -or
                $covered -gt $branchCoverage[$lineKey].Covered) {
                $branchCoverage[$lineKey] = [PSCustomObject]@{
                    Covered = $covered
                    Valid = $valid
                }
            }
        }
    }

    if ($reportLineCount -eq 0) {
        throw "Cobertura report '$($report.FullName)' contains no valid instrumented lines."
    }
}

$validLines = $lineHits.Count

if ($validLines -eq 0) {
    throw "Cobertura reports contain no valid instrumented lines."
}

$coveredLines = @($lineHits.Values | Where-Object { $_ -gt 0 }).Count
$validBranches = 0L
$coveredBranches = 0L

foreach ($coverage in $branchCoverage.Values) {
    $validBranches += $coverage.Valid
    $coveredBranches += $coverage.Covered
}

$lineRate = $coveredLines / $validLines
if ($validBranches -eq 0) {
    throw "Cobertura reports contain no valid instrumented branches."
}

$branchRate = $coveredBranches / $validBranches

Write-Output (
    "Merged coverage: lines {0:P2} ({1}/{2}); branches {3:P2} ({4}/{5})." -f
    $lineRate,
    $coveredLines,
    $validLines,
    $branchRate,
    $coveredBranches,
    $validBranches)

if ($lineRate -lt $MinimumLineRate) {
    throw "Line coverage is below the required $($MinimumLineRate.ToString('P0')) floor."
}

if ($branchRate -lt $MinimumBranchRate) {
    throw "Branch coverage is below the required $($MinimumBranchRate.ToString('P0')) floor."
}
