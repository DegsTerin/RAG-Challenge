# Purpose: Exercises fail-closed Cobertura aggregation with disposable local fixtures.
[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$coverageScript = Join-Path $PSScriptRoot "assert-coverage.ps1"
$utf8WithoutBom = [System.Text.UTF8Encoding]::new($false)

function Invoke-CoverageCase {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Name,

        [Parameter(Mandatory)]
        [hashtable]$Reports,

        [Parameter(Mandatory)]
        [bool]$ShouldSucceed,

        [Parameter(Mandatory)]
        [string]$ExpectedPattern,

        [double]$MinimumLineRate = 0.70,

        [double]$MinimumBranchRate = 0.45
    )

    $caseDirectory = Join-Path $script:temporaryRoot $Name
    $null = New-Item -ItemType Directory -Path $caseDirectory

    foreach ($report in $Reports.GetEnumerator()) {
        $reportPath = Join-Path $caseDirectory $report.Key
        $reportDirectory = Split-Path -Parent $reportPath
        $null = New-Item -ItemType Directory -Path $reportDirectory -Force
        [System.IO.File]::WriteAllText(
            $reportPath,
            [string]$report.Value,
            $script:utf8WithoutBom)
    }

    $succeeded = $false
    $observed = @()

    try {
        $observed = @(& $script:coverageScript `
            -ResultsDirectory $caseDirectory `
            -MinimumLineRate $MinimumLineRate `
            -MinimumBranchRate $MinimumBranchRate)
        $succeeded = $true
    }
    catch {
        $observed = @($_.Exception.Message)
    }

    if ($succeeded -ne $ShouldSucceed) {
        throw "Case '$Name' had unexpected success state '$succeeded': $($observed -join ' ')"
    }

    if (($observed -join [Environment]::NewLine) -notmatch $ExpectedPattern) {
        throw "Case '$Name' did not produce the expected evidence: $($observed -join ' ')"
    }

    Write-Output "PASS: $Name"
}

$validBranchless = @'
<?xml version="1.0" encoding="utf-8"?>
<coverage>
  <packages>
    <package>
      <classes>
        <class filename="src/Example.cs">
          <lines>
            <line number="10" hits="1" branch="False" />
            <line number="11" hits="0" branch="False" />
          </lines>
        </class>
      </classes>
    </package>
  </packages>
</coverage>
'@

$validBranches = @'
<?xml version="1.0" encoding="utf-8"?>
<coverage>
  <packages>
    <package>
      <classes>
        <class filename="src/Branch.cs">
          <lines>
            <line number="20" hits="1" branch="True" condition-coverage="50% (1/2)" />
          </lines>
        </class>
      </classes>
    </package>
  </packages>
</coverage>
'@

$malformedBranches = $validBranches.Replace(
    'condition-coverage="50% (1/2)"',
    'condition-coverage="unknown"')
$impossibleBranches = $validBranches.Replace(
    'condition-coverage="50% (1/2)"',
    'condition-coverage="150% (3/2)"')
$inconsistentBranches = $validBranches.Replace(
    'condition-coverage="50% (1/2)"',
    'condition-coverage="33.3% (1/3)"')

$temporaryBase = [System.IO.Path]::GetFullPath(
    [System.IO.Path]::GetTempPath())
$temporaryRoot = Join-Path $temporaryBase (
    "rag-challenge-coverage-tests-{0}" -f [guid]::NewGuid())
$temporaryRoot = [System.IO.Path]::GetFullPath($temporaryRoot)
$temporaryLeaf = Split-Path -Leaf $temporaryRoot

if (-not $temporaryRoot.StartsWith(
        $temporaryBase,
        [System.StringComparison]::OrdinalIgnoreCase) -or
    -not $temporaryLeaf.StartsWith(
        "rag-challenge-coverage-tests-",
        [System.StringComparison]::Ordinal)) {
    throw "The disposable test directory is outside the expected temporary root."
}

try {
    $null = New-Item -ItemType Directory -Path $temporaryRoot

    Invoke-CoverageCase `
        -Name "no-report" `
        -Reports @{} `
        -ShouldSucceed $false `
        -ExpectedPattern "No Cobertura reports"

    Invoke-CoverageCase `
        -Name "empty-report" `
        -Reports @{ "coverage.cobertura.xml" = "<coverage><packages /></coverage>" } `
        -ShouldSucceed $false `
        -ExpectedPattern "no valid instrumented lines"

    Invoke-CoverageCase `
        -Name "empty-report-cannot-be-masked" `
        -Reports @{
            "empty/coverage.cobertura.xml" = "<coverage><packages /></coverage>"
            "valid/coverage.cobertura.xml" = $validBranchless
        } `
        -ShouldSucceed $false `
        -ExpectedPattern "no valid instrumented lines"

    Invoke-CoverageCase `
        -Name "branchless-equal-line-floor" `
        -Reports @{ "coverage.cobertura.xml" = $validBranchless } `
        -ShouldSucceed $true `
        -MinimumLineRate 0.50 `
        -MinimumBranchRate 1.00 `
        -ExpectedPattern "lines 50.00% \(1/2\); branches 100.00% \(0/0\)"

    Invoke-CoverageCase `
        -Name "below-line-floor" `
        -Reports @{ "coverage.cobertura.xml" = $validBranchless } `
        -ShouldSucceed $false `
        -MinimumLineRate 0.51 `
        -ExpectedPattern "Line coverage is below"

    Invoke-CoverageCase `
        -Name "above-line-floor" `
        -Reports @{ "coverage.cobertura.xml" = $validBranchless } `
        -ShouldSucceed $true `
        -MinimumLineRate 0.49 `
        -ExpectedPattern "lines 50.00% \(1/2\)"

    Invoke-CoverageCase `
        -Name "equal-branch-floor" `
        -Reports @{ "coverage.cobertura.xml" = $validBranches } `
        -ShouldSucceed $true `
        -MinimumLineRate 1.00 `
        -MinimumBranchRate 0.50 `
        -ExpectedPattern "branches 50.00% \(1/2\)"

    Invoke-CoverageCase `
        -Name "below-branch-floor" `
        -Reports @{ "coverage.cobertura.xml" = $validBranches } `
        -ShouldSucceed $false `
        -MinimumBranchRate 0.51 `
        -ExpectedPattern "Branch coverage is below"

    Invoke-CoverageCase `
        -Name "malformed-branch-metadata" `
        -Reports @{ "coverage.cobertura.xml" = $malformedBranches } `
        -ShouldSucceed $false `
        -ExpectedPattern "malformed branch coverage"

    Invoke-CoverageCase `
        -Name "impossible-branch-metadata" `
        -Reports @{ "coverage.cobertura.xml" = $impossibleBranches } `
        -ShouldSucceed $false `
        -ExpectedPattern "impossible branch coverage"

    Invoke-CoverageCase `
        -Name "inconsistent-branch-totals" `
        -Reports @{
            "first/coverage.cobertura.xml" = $validBranches
            "second/coverage.cobertura.xml" = $inconsistentBranches
        } `
        -ShouldSucceed $false `
        -ExpectedPattern "inconsistent branch totals"

    Write-Output "All assert-coverage tests passed."
}
finally {
    if (Test-Path -LiteralPath $temporaryRoot) {
        Remove-Item -LiteralPath $temporaryRoot -Recurse -Force
    }

    if (Test-Path -LiteralPath $temporaryRoot) {
        throw "The disposable test directory could not be removed."
    }
}
