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

$reports = Get-ChildItem `
    -LiteralPath $ResultsDirectory `
    -Recurse `
    -Filter "coverage.cobertura.xml"

if ($reports.Count -eq 0) {
    throw "No Cobertura reports were found in '$ResultsDirectory'."
}

$lineHits = @{}
$branchCoverage = @{}

foreach ($report in $reports) {
    [xml]$document = Get-Content -LiteralPath $report.FullName -Raw

    foreach ($class in $document.SelectNodes(
        "/coverage/packages/package/classes/class")) {
        $file = $class.filename.Replace("\", "/")

        foreach ($line in $class.SelectNodes("lines/line")) {
            $lineKey = "${file}:$($line.number)"
            $hits = [int]$line.hits

            if (-not $lineHits.ContainsKey($lineKey) -or
                $hits -gt $lineHits[$lineKey]) {
                $lineHits[$lineKey] = $hits
            }

            if ($line.branch -ne "True") {
                continue
            }

            $match = [regex]::Match(
                [string]$line."condition-coverage",
                "\((\d+)/(\d+)\)")

            if (-not $match.Success) {
                continue
            }

            $covered = [int]$match.Groups[1].Value
            $valid = [int]$match.Groups[2].Value

            if (-not $branchCoverage.ContainsKey($lineKey) -or
                $covered -gt $branchCoverage[$lineKey].Covered) {
                $branchCoverage[$lineKey] = [PSCustomObject]@{
                    Covered = $covered
                    Valid = $valid
                }
            }
        }
    }
}

$validLines = $lineHits.Count
$coveredLines = @($lineHits.Values | Where-Object { $_ -gt 0 }).Count
$validBranches = (
    $branchCoverage.Values |
        Measure-Object -Property Valid -Sum
).Sum
$coveredBranches = (
    $branchCoverage.Values |
        Measure-Object -Property Covered -Sum
).Sum

$lineRate = if ($validLines -eq 0) {
    1.0
}
else {
    $coveredLines / $validLines
}
$branchRate = if ($validBranches -eq 0) {
    1.0
}
else {
    $coveredBranches / $validBranches
}

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
