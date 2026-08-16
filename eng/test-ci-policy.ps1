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

    $passingPolicyTest = Join-Path $temporaryRoot "passing-policy-test.ps1"
    $failingPolicyTest = Join-Path $temporaryRoot "failing-policy-test.ps1"
    [System.IO.File]::WriteAllText(
        $passingPolicyTest,
        'Write-Output "fixture policy test passed"',
        [System.Text.UTF8Encoding]::new($false))
    [System.IO.File]::WriteAllText(
        $failingPolicyTest,
        'throw "fixture policy failure"',
        [System.Text.UTF8Encoding]::new($false))

    Invoke-ExpectedSuccess -Name "required-policy-test-success" -Action {
        Invoke-RequiredPolicyTest `
            -Name "passing fixture" `
            -ScriptPath $passingPolicyTest
    }

    Invoke-ExpectedFailure `
        -Name "required-policy-test-failure-propagates" `
        -Action {
            Invoke-RequiredPolicyTest `
                -Name "failing fixture" `
                -ScriptPath $failingPolicyTest
        } `
        -ExpectedPattern "Required policy test 'failing fixture' failed: fixture policy failure"

    Invoke-ExpectedFailure `
        -Name "required-policy-test-missing-fails-closed" `
        -Action {
            Invoke-RequiredPolicyTest `
                -Name "missing fixture" `
                -ScriptPath (Join-Path $temporaryRoot "missing-policy-test.ps1")
        } `
        -ExpectedPattern "Required policy test 'missing fixture' does not exist"

    $ciScript = Get-Content -LiteralPath (Join-Path $PSScriptRoot "ci.ps1") -Raw
    $workflow = Get-Content -LiteralPath (
        Join-Path (Split-Path -Parent $PSScriptRoot) ".github/workflows/ci.yml") -Raw

    if ($ciScript -match 'Convert-NuGetLockFileLineEndings|WriteAllText|Normalised NuGet lockfiles') {
        throw "The CI entry point still contains lockfile rewriting behaviour."
    }

    $credentialScrub = [regex]::Match(
        $ciScript,
        '(?s)SetEnvironmentVariable\(\s*''OPENAI_API_KEY'',\s*\$null,\s*\[System[.]EnvironmentVariableTarget\]::Process\)')
    $firstPolicyUse = $ciScript.IndexOf('. (Join-Path $PSScriptRoot "ci-policy.ps1")', [System.StringComparison]::Ordinal)

    if (-not $credentialScrub.Success -or
        $firstPolicyUse -lt 0 -or
        $credentialScrub.Index -gt $firstPolicyUse -or
        $ciScript -match 'GetEnvironmentVariable\([^)]*OPENAI_API_KEY') {
        throw "The CI entry point must remove the product credential without reading it before invoking policy or child processes."
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

    foreach ($requiredTest in @(
            "test-assert-coverage.ps1",
            "test-ci-policy.ps1")) {
        $requiredTestPattern =
            '(?m)^\s*-ScriptPath\s+\(Join-Path\s+\$PSScriptRoot\s+"' +
            [regex]::Escape($requiredTest) +
            '"\)\s*$'
        $requiredTestCalls = [regex]::Matches(
            $ciScript,
            $requiredTestPattern).Count

        if ($requiredTestCalls -ne 1) {
            throw (
                "The CI entry point must invoke '$requiredTest' exactly once through " +
                "the fail-closed policy helper.")
        }
    }

    $languageTestCalls = [regex]::Matches(
        $ciScript,
        '(?m)^\s*node\s+\(Join-Path\s+\$PSScriptRoot\s+"test-language-policy[.]mjs"\)\s*$').Count
    $languageCheckCalls = [regex]::Matches(
        $ciScript,
        '(?m)^\s*node\s+@languageArguments\s*$').Count
    $firstRestore = $ciScript.IndexOf('dotnet restore RAG-Challenge.sln', [System.StringComparison]::Ordinal)
    $languageTestIndex = $ciScript.IndexOf('"test-language-policy.mjs"', [System.StringComparison]::Ordinal)
    $languageCheckIndex = $ciScript.IndexOf('node @languageArguments', [System.StringComparison]::Ordinal)

    if ($languageTestCalls -ne 1 -or
        $languageCheckCalls -ne 1 -or
        $firstRestore -lt 0 -or
        $languageTestIndex -lt 0 -or
        $languageCheckIndex -lt 0 -or
        $languageTestIndex -gt $firstRestore -or
        $languageCheckIndex -gt $firstRestore) {
        throw "The CI entry point must run the language tests and checker exactly once before restore."
    }

    $workflowEntryPointCalls = [regex]::Matches(
        $workflow,
        '(?m)^\s*run:\s*\./eng/ci\.ps1\s*$').Count

    if ($workflowEntryPointCalls -ne 1) {
        throw "The workflow must invoke the canonical CI entry point exactly once."
    }

    $selectedHeadExpression = '${{ github.event_name == ''pull_request'' && github.event.pull_request.head.sha || github.sha }}'
    $selectedHeadPattern = [regex]::Escape($selectedHeadExpression)
    $checkoutHeadBindings = [regex]::Matches(
        $workflow,
        "(?m)^\s*ref:\s*$selectedHeadPattern\s*$").Count
    $boundaryHeadBindings = [regex]::Matches(
        $workflow,
        "(?m)^\s*SELECTED_HEAD:\s*$selectedHeadPattern\s*$").Count
    $actualHeadCommandIndex = $workflow.IndexOf(
        '$headOutput = & git rev-parse --verify HEAD',
        [System.StringComparison]::Ordinal)
    $actualHeadBindingIndex = $workflow.IndexOf(
        '$actualHead -notmatch $fullSha -or $actualHead -cne $env:SELECTED_HEAD',
        [System.StringComparison]::Ordinal)
    $firstLanguageBoundaryExport = $workflow.IndexOf(
        'RAG_LANGUAGE_COMMIT_BASE=',
        [System.StringComparison]::Ordinal)

    if ($checkoutHeadBindings -ne 1 -or
        $boundaryHeadBindings -ne 1 -or
        $actualHeadCommandIndex -lt 0 -or
        $actualHeadBindingIndex -lt $actualHeadCommandIndex -or
        $firstLanguageBoundaryExport -lt $actualHeadBindingIndex) {
        throw "The workflow must check out and verify the exact event-selected head before exporting a language boundary."
    }

    foreach ($forbiddenMergeBinding in @(
            'refs/pull/',
            'pull_request.merge_commit_sha',
            'allow-merge',
            'allow_merge',
            '--first-parent')) {
        if ($workflow.IndexOf(
                $forbiddenMergeBinding,
                [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            throw "The workflow must not bind or bypass a synthetic or general merge commit."
        }
    }

    function Assert-SyntheticWorkflowHeadBinding {
        [CmdletBinding()]
        param(
            [Parameter(Mandatory)]
            [string]$SelectedHead,

            [Parameter(Mandatory)]
            [string]$ActualHead
        )

        if ($SelectedHead -notmatch '^[0-9a-f]{40}$' -or
            $SelectedHead -eq ('0' * 40) -or
            $ActualHead -notmatch '^[0-9a-f]{40}$' -or
            $ActualHead -cne $SelectedHead) {
            throw "Synthetic checked-out head mismatch."
        }
    }

    $pullRequestHead = 'a' * 40
    $syntheticMergeHead = 'b' * 40
    Invoke-ExpectedSuccess -Name "pull-request-head-binding" -Action {
        Assert-SyntheticWorkflowHeadBinding `
            -SelectedHead $pullRequestHead `
            -ActualHead $pullRequestHead
    }
    Invoke-ExpectedFailure `
        -Name "pull-request-synthetic-merge-rejected" `
        -Action {
            Assert-SyntheticWorkflowHeadBinding `
                -SelectedHead $pullRequestHead `
                -ActualHead $syntheticMergeHead
        } `
        -ExpectedPattern "Synthetic checked-out head mismatch"

    if ($workflow -notmatch 'fetch-depth:\s*0' -or
        $workflow -notmatch 'github[.]event[.]pull_request[.]base[.]sha' -or
        $workflow -notmatch 'github[.]event[.]pull_request[.]head[.]sha' -or
        $workflow -notmatch 'github[.]event[.]before' -or
        $workflow -notmatch 'github[.]sha' -or
        $workflow -notmatch 'RAG_LANGUAGE_COMMIT_BASE' -or
        $workflow -notmatch 'RAG_LANGUAGE_COMMIT_HEAD') {
        throw "The workflow must fetch full history and resolve the exact event-specific language boundary."
    }

    if ($workflow -notmatch 'Assert-VersionSatisfiesRange' -or
        $workflow -notmatch '\$dashboardPackage\.engines\.node' -or
        $workflow -notmatch '\$dashboardPackage\.engines\.npm' -or
        $workflow -notmatch '\$orchestratorPackage\.engines\.node' -or
        $workflow -notmatch '\$orchestratorPackage\.engines\.npm') {
        throw "The workflow does not validate the package engine ranges."
    }

    if ($ciScript -notmatch '\$orchestratorRoot' -or
        $ciScript -notmatch 'npm run check' -or
        $ciScript -notmatch 'Offline orchestrator restore') {
        throw "The CI entry point does not run the locked orchestrator checks."
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
