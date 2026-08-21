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

function Invoke-SyntheticGit {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Repository,

        [Parameter(Mandatory)]
        [string[]]$Arguments,

        [Parameter(Mandatory)]
        [string]$TemporaryDirectory,

        [switch]$AllowFailure
    )

    $gitExecutable = (Get-Command git -CommandType Application -ErrorAction Stop |
            Select-Object -First 1).Source
    $nullDevice = if ($IsWindows) { "NUL" } else { "/dev/null" }
    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $gitExecutable
    $startInfo.WorkingDirectory = $Repository
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardInput = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.Environment.Clear()
    $startInfo.Environment["GIT_CONFIG_NOSYSTEM"] = "1"
    $startInfo.Environment["GIT_CONFIG_GLOBAL"] = $nullDevice
    $startInfo.Environment["GIT_ATTR_NOSYSTEM"] = "1"
    $startInfo.Environment["GIT_TERMINAL_PROMPT"] = "0"
    $startInfo.Environment["GCM_INTERACTIVE"] = "Never"
    $startInfo.Environment["GIT_PAGER"] = "cat"
    $startInfo.Environment["TEMP"] = $TemporaryDirectory
    $startInfo.Environment["TMP"] = $TemporaryDirectory
    if ($IsWindows) {
        $startInfo.Environment["SystemRoot"] = [Environment]::GetFolderPath(
            [Environment+SpecialFolder]::Windows)
    }

    foreach ($argument in @(
            "--no-optional-locks",
            "-c", "core.hooksPath=$nullDevice",
            "-c", "core.attributesFile=$nullDevice",
            "-c", "init.templateDir=$nullDevice",
            "-c", "core.fsmonitor=false",
            "-c", "credential.helper=",
            "-c", "core.askPass=",
            "-c", "protocol.allow=never",
            "-c", "user.name=CI Policy Fixture",
            "-c", "user.email=ci-policy@example.invalid",
            "-C", $Repository) + $Arguments) {
        $startInfo.ArgumentList.Add($argument)
    }

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    try {
        if (-not $process.Start()) {
            throw "Synthetic Git process did not start."
        }
        $process.StandardInput.Close()
        $standardOutput = $process.StandardOutput.ReadToEnd()
        $standardError = $process.StandardError.ReadToEnd()
        if (-not $process.WaitForExit(30000)) {
            $process.Kill($true)
            throw "Synthetic Git process exceeded its time limit."
        }
        if ($standardOutput.Length -gt 65536 -or $standardError.Length -gt 65536) {
            throw "Synthetic Git output exceeded its bounded limit."
        }
        if ($process.ExitCode -ne 0 -and -not $AllowFailure) {
            throw "Synthetic Git command failed closed."
        }
        return [pscustomobject]@{
            ExitCode = $process.ExitCode
            StandardOutput = $standardOutput
        }
    }
    finally {
        $process.Dispose()
    }
}

function Resolve-SyntheticPullRequestMergeBase {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Repository,

        [Parameter(Mandatory)]
        [string]$EventBase,

        [Parameter(Mandatory)]
        [string]$SelectedHead,

        [Parameter(Mandatory)]
        [string]$TemporaryDirectory
    )

    $fullSha = '^[0-9a-f]{40}$'
    if ($EventBase -notmatch $fullSha -or
        $EventBase -eq ('0' * 40) -or
        $SelectedHead -notmatch $fullSha -or
        $SelectedHead -eq ('0' * 40)) {
        throw "Synthetic pull-request identities are missing or invalid."
    }

    $mergeBaseResult = Invoke-SyntheticGit `
        -Repository $Repository `
        -Arguments @("merge-base", "--", $EventBase, $SelectedHead) `
        -TemporaryDirectory $TemporaryDirectory `
        -AllowFailure
    $mergeBaseCandidates = @($mergeBaseResult.StandardOutput -split "`r?`n" | Where-Object { $_.Length -gt 0 })
    if ($mergeBaseResult.ExitCode -ne 0 -or
        $mergeBaseCandidates.Count -ne 1 -or
        $mergeBaseCandidates[0] -notmatch $fullSha -or
        $mergeBaseCandidates[0] -eq ('0' * 40)) {
        throw "Synthetic pull-request merge base is missing or invalid."
    }

    $allMergeBaseResult = Invoke-SyntheticGit `
        -Repository $Repository `
        -Arguments @("merge-base", "--all", "--", $EventBase, $SelectedHead) `
        -TemporaryDirectory $TemporaryDirectory `
        -AllowFailure
    $allMergeBaseCandidates = @($allMergeBaseResult.StandardOutput -split "`r?`n" | Where-Object { $_.Length -gt 0 })
    if ($allMergeBaseResult.ExitCode -ne 0 -or
        $allMergeBaseCandidates.Count -ne 1 -or
        $allMergeBaseCandidates[0] -cne $mergeBaseCandidates[0]) {
        throw "Synthetic pull-request merge base is ambiguous."
    }

    $ancestorResult = Invoke-SyntheticGit `
        -Repository $Repository `
        -Arguments @("merge-base", "--is-ancestor", "--", $mergeBaseCandidates[0], $SelectedHead) `
        -TemporaryDirectory $TemporaryDirectory `
        -AllowFailure
    if ($ancestorResult.ExitCode -ne 0) {
        throw "Synthetic pull-request merge base is not an ancestor."
    }
    return $mergeBaseCandidates[0]
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
            "test-ci-policy.ps1",
            "test-nuget-audit-policy.ps1",
            "test-new-oracle19-product-plans.ps1",
            "test-render-entrypoint-policy.ps1",
            "test-render-free-output-policy.ps1",
            "test-integration-archive-policy.ps1")) {
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

    $actionUses = [regex]::Matches(
        $workflow,
        '(?m)^\s*uses:\s*(?<action>[^@\s]+)@(?<revision>[^\s#]+)')
    $approvedActionPins = @{
        'actions/checkout' = '11bd71901bbe5b1630ceea73d27597364c9af683'
        'actions/setup-dotnet' = '67a3573c9a986a3f9c594539f4ab511d57bb3ce9'
        'actions/setup-node' = '49933ea5288caeca8642d1e84afbd3f7d6820020'
    }
    if ($actionUses.Count -ne $approvedActionPins.Count -or
        @($actionUses | Where-Object {
                $action = $_.Groups['action'].Value
                $revision = $_.Groups['revision'].Value
                $revision -notmatch '^[0-9a-f]{40}$' -or
                -not $approvedActionPins.ContainsKey($action) -or
                $approvedActionPins[$action] -cne $revision
            }).Count -ne 0 -or
        $workflow -notmatch '(?m)^\s*node-version:\s*\d+\.\d+\.\d+\s*$') {
        throw "The workflow must use only approved Action identities and pin Node.js by exact patch."
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

    $mergeBaseCommandIndex = $workflow.IndexOf(
        '$mergeBaseOutput = @(& git merge-base -- $env:PULL_REQUEST_BASE $env:SELECTED_HEAD)',
        [System.StringComparison]::Ordinal)
    $completeMergeBaseCommandIndex = $workflow.IndexOf(
        '$allMergeBaseOutput = @(& git merge-base --all -- $env:PULL_REQUEST_BASE $env:SELECTED_HEAD)',
        [System.StringComparison]::Ordinal)
    $ancestorCommandIndex = $workflow.IndexOf(
        '& git merge-base --is-ancestor -- $mergeBase $env:SELECTED_HEAD',
        [System.StringComparison]::Ordinal)
    $mergeBaseExportIndex = $workflow.IndexOf(
        'RAG_LANGUAGE_COMMIT_BASE=$mergeBase',
        [System.StringComparison]::Ordinal)

    if ($mergeBaseCommandIndex -lt $actualHeadBindingIndex -or
        $completeMergeBaseCommandIndex -lt $mergeBaseCommandIndex -or
        $ancestorCommandIndex -lt $completeMergeBaseCommandIndex -or
        $mergeBaseExportIndex -lt $ancestorCommandIndex -or
        $workflow.IndexOf(
            'RAG_LANGUAGE_COMMIT_BASE=$env:PULL_REQUEST_BASE',
            [System.StringComparison]::Ordinal) -ge 0) {
        throw "The workflow must derive one verified pull-request merge-base boundary without exporting the event base directly."
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

    $divergedRepository = Join-Path $temporaryRoot "diverged-repository"
    $null = New-Item -ItemType Directory -Path $divergedRepository
    $utf8WithoutBom = [System.Text.UTF8Encoding]::new($false)
    $null = Invoke-SyntheticGit `
        -Repository $divergedRepository `
        -Arguments @("init", "--initial-branch=main") `
        -TemporaryDirectory $temporaryRoot
    [System.IO.File]::WriteAllText(
        (Join-Path $divergedRepository "common.txt"),
        "common`n",
        $utf8WithoutBom)
    $null = Invoke-SyntheticGit -Repository $divergedRepository -Arguments @("add", "--", "common.txt") -TemporaryDirectory $temporaryRoot
    $null = Invoke-SyntheticGit -Repository $divergedRepository -Arguments @("commit", "-m", "test(ci): create common ancestor") -TemporaryDirectory $temporaryRoot
    $commonAncestor = (Invoke-SyntheticGit -Repository $divergedRepository -Arguments @("rev-parse", "HEAD") -TemporaryDirectory $temporaryRoot).StandardOutput.Trim()
    $null = Invoke-SyntheticGit -Repository $divergedRepository -Arguments @("checkout", "-b", "event-base") -TemporaryDirectory $temporaryRoot
    [System.IO.File]::WriteAllText((Join-Path $divergedRepository "base.txt"), "base`n", $utf8WithoutBom)
    $null = Invoke-SyntheticGit -Repository $divergedRepository -Arguments @("add", "--", "base.txt") -TemporaryDirectory $temporaryRoot
    $null = Invoke-SyntheticGit -Repository $divergedRepository -Arguments @("commit", "-m", "test(ci): advance event base") -TemporaryDirectory $temporaryRoot
    $eventBase = (Invoke-SyntheticGit -Repository $divergedRepository -Arguments @("rev-parse", "HEAD") -TemporaryDirectory $temporaryRoot).StandardOutput.Trim()
    $null = Invoke-SyntheticGit -Repository $divergedRepository -Arguments @("checkout", "-b", "pull-request-head", $commonAncestor) -TemporaryDirectory $temporaryRoot
    [System.IO.File]::WriteAllText((Join-Path $divergedRepository "head.txt"), "head`n", $utf8WithoutBom)
    $null = Invoke-SyntheticGit -Repository $divergedRepository -Arguments @("add", "--", "head.txt") -TemporaryDirectory $temporaryRoot
    $null = Invoke-SyntheticGit -Repository $divergedRepository -Arguments @("commit", "-m", "test(ci): advance pull-request head") -TemporaryDirectory $temporaryRoot
    $pullRequestDivergedHead = (Invoke-SyntheticGit -Repository $divergedRepository -Arguments @("rev-parse", "HEAD") -TemporaryDirectory $temporaryRoot).StandardOutput.Trim()

    Invoke-ExpectedSuccess -Name "diverged-pull-request-merge-base" -Action {
        $resolvedMergeBase = Resolve-SyntheticPullRequestMergeBase `
            -Repository $divergedRepository `
            -EventBase $eventBase `
            -SelectedHead $pullRequestDivergedHead `
            -TemporaryDirectory $temporaryRoot
        if ($resolvedMergeBase -cne $commonAncestor -or $resolvedMergeBase -ceq $eventBase) {
            throw "The diverged pull request did not select its common ancestor."
        }
    }
    Invoke-ExpectedFailure `
        -Name "invalid-pull-request-merge-base-input" `
        -Action {
            Resolve-SyntheticPullRequestMergeBase `
                -Repository $divergedRepository `
                -EventBase ('0' * 40) `
                -SelectedHead $pullRequestDivergedHead `
                -TemporaryDirectory $temporaryRoot
        } `
        -ExpectedPattern "identities are missing or invalid"
    Invoke-ExpectedFailure `
        -Name "missing-pull-request-merge-base" `
        -Action {
            Resolve-SyntheticPullRequestMergeBase `
                -Repository $divergedRepository `
                -EventBase ('f' * 40) `
                -SelectedHead $pullRequestDivergedHead `
                -TemporaryDirectory $temporaryRoot
        } `
        -ExpectedPattern "merge base is missing or invalid"

    $null = Invoke-SyntheticGit `
        -Repository $divergedRepository `
        -Arguments @("checkout", "-b", "ambiguous-event-base", $eventBase) `
        -TemporaryDirectory $temporaryRoot
    $null = Invoke-SyntheticGit `
        -Repository $divergedRepository `
        -Arguments @("merge", "--no-ff", $pullRequestDivergedHead, "-m", "test(ci): merge pull-request side into event side") `
        -TemporaryDirectory $temporaryRoot
    $ambiguousEventBase = (Invoke-SyntheticGit -Repository $divergedRepository -Arguments @("rev-parse", "HEAD") -TemporaryDirectory $temporaryRoot).StandardOutput.Trim()
    $null = Invoke-SyntheticGit `
        -Repository $divergedRepository `
        -Arguments @("checkout", "pull-request-head") `
        -TemporaryDirectory $temporaryRoot
    $null = Invoke-SyntheticGit `
        -Repository $divergedRepository `
        -Arguments @("merge", "--no-ff", $eventBase, "-m", "test(ci): merge event side into pull-request side") `
        -TemporaryDirectory $temporaryRoot
    $ambiguousSelectedHead = (Invoke-SyntheticGit -Repository $divergedRepository -Arguments @("rev-parse", "HEAD") -TemporaryDirectory $temporaryRoot).StandardOutput.Trim()
    Invoke-ExpectedFailure `
        -Name "ambiguous-pull-request-merge-base" `
        -Action {
            Resolve-SyntheticPullRequestMergeBase `
                -Repository $divergedRepository `
                -EventBase $ambiguousEventBase `
                -SelectedHead $ambiguousSelectedHead `
                -TemporaryDirectory $temporaryRoot
        } `
        -ExpectedPattern "merge base is ambiguous"

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

    $npmAuditCalls = [regex]::Matches(
        $ciScript,
        '(?m)^\s*npm audit --audit-level=high\s*$').Count
    if ($npmAuditCalls -ne 2 -or
        $ciScript -notmatch 'Orchestrator dependency audit' -or
        $ciScript -notmatch 'NOT_RUN: orchestrator dependency audit' -or
        $ciScript -notmatch 'NOT_RUN: dashboard dependency audit') {
        throw "The CI entry point must audit both npm graphs online and report both offline omissions."
    }

    if ($ciScript -notmatch '--format json' -or
        $ciScript -notmatch '--output-version 1' -or
        $ciScript -notmatch 'Assert-NuGetVulnerabilityAuditJson' -or
        $ciScript -notmatch 'NOT_RUN: [. ]NET dependency audit') {
        throw "The CI entry point must parse structured NuGet audit output and report its offline omission."
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
