# Purpose: Exercises the public CI policy and workflow with disposable local fixtures.
[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot "ci-policy.ps1")

function Assert-ExpectedFailure {
    [CmdletBinding()]
    param(
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

    if ($null -eq $failure -or $failure -notmatch $ExpectedPattern) {
        throw "The expected fail-closed outcome was not observed."
    }
}

$temporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) (
    "rag-challenge-ci-policy-tests-{0}" -f [guid]::NewGuid())

try {
    $null = New-Item -ItemType Directory -Path $temporaryRoot

    Assert-VersionSatisfiesRange `
        -Name "Node.js" `
        -ActualVersion "24.19.1" `
        -Range ">=24.18.0 <25"
    Assert-ExpectedFailure `
        -Action {
            Assert-VersionSatisfiesRange `
                -Name "Node.js" `
                -ActualVersion "25.0.0" `
                -Range ">=24.18.0 <25"
        } `
        -ExpectedPattern "outside the supported range"

    $lfPath = Join-Path $temporaryRoot "lf.txt"
    $crlfPath = Join-Path $temporaryRoot "crlf.txt"
    [System.IO.File]::WriteAllBytes(
        $lfPath,
        [System.Text.UTF8Encoding]::new($false).GetBytes("one`ntwo`n"))
    [System.IO.File]::WriteAllBytes(
        $crlfPath,
        [System.Text.UTF8Encoding]::new($false).GetBytes("one`r`ntwo`r`n"))
    Assert-FilesUseLfOnly -Paths @($lfPath)
    Assert-ExpectedFailure `
        -Action { Assert-FilesUseLfOnly -Paths @($crlfPath) } `
        -ExpectedPattern "carriage-return bytes"

    $ciScript = Get-Content -LiteralPath (Join-Path $PSScriptRoot "ci.ps1") -Raw
    $workflow = Get-Content -LiteralPath (
        Join-Path (Split-Path -Parent $PSScriptRoot) ".github/workflows/ci.yml") -Raw

    foreach ($removedInternalReference in @(
            "test-language-policy.mjs",
            "check-language.mjs",
            "test-new-oracle19-product-plans.ps1",
            "tools/ai-orchestrator")) {
        if ($ciScript.Contains(
                $removedInternalReference,
                [System.StringComparison]::Ordinal) -or
            $workflow.Contains(
                $removedInternalReference,
                [System.StringComparison]::Ordinal)) {
            throw "CI still references removed internal tooling."
        }
    }

    foreach ($requiredTest in @(
            "test-assert-coverage.ps1",
            "test-ci-policy.ps1",
            "test-nuget-audit-policy.ps1",
            "test-render-entrypoint-policy.ps1",
            "test-render-free-output-policy.ps1",
            "test-integration-archive-policy.ps1")) {
        if ([regex]::Matches($ciScript, [regex]::Escape($requiredTest)).Count -ne 1) {
            throw "The CI entry point must invoke '$requiredTest' exactly once."
        }
    }

    $credentialScrub = [regex]::Match(
        $ciScript,
        '(?s)SetEnvironmentVariable\(\s*''OPENAI_API_KEY'',\s*\$null,\s*\[System[.]EnvironmentVariableTarget\]::Process\)')
    $firstPolicyUse = $ciScript.IndexOf(
        '. (Join-Path $PSScriptRoot "ci-policy.ps1")',
        [System.StringComparison]::Ordinal)
    if (-not $credentialScrub.Success -or
        $firstPolicyUse -lt 0 -or
        $credentialScrub.Index -gt $firstPolicyUse -or
        $ciScript -match 'GetEnvironmentVariable\([^)]*OPENAI_API_KEY') {
        throw "CI must clear the provider credential before invoking policy or children."
    }

    $approvedPins = @{
        "actions/checkout" = "11bd71901bbe5b1630ceea73d27597364c9af683"
        "actions/setup-dotnet" = "67a3573c9a986a3f9c594539f4ab511d57bb3ce9"
        "actions/setup-node" = "49933ea5288caeca8642d1e84afbd3f7d6820020"
    }
    $actionUses = [regex]::Matches(
        $workflow,
        '(?m)^\s*uses:\s*(?<action>[^@\s]+)@(?<revision>[^\s#]+)')
    if ($actionUses.Count -ne $approvedPins.Count -or
        @($actionUses | Where-Object {
                $action = $_.Groups["action"].Value
                -not $approvedPins.ContainsKey($action) -or
                $approvedPins[$action] -cne $_.Groups["revision"].Value
            }).Count -ne 0) {
        throw "The workflow must use only the approved immutable Action revisions."
    }

    if ($workflow -notmatch '(?m)^\s*permissions:\s*\r?\n\s*contents:\s*read\s*$' -or
        $workflow -notmatch '(?m)^\s*persist-credentials:\s*false\s*$' -or
        $workflow -notmatch '(?m)^\s*node-version:\s*24[.]19[.]0\s*$' -or
        [regex]::Matches($workflow, '(?m)^\s*run:\s*[.]\/eng\/ci[.]ps1\s*$').Count -ne 1) {
        throw "The workflow's least-privilege or canonical-entrypoint boundary diverged."
    }

    if ([regex]::Matches($ciScript, '(?m)^\s*npm audit --audit-level=high\s*$').Count -ne 1 -or
        $ciScript -notmatch 'NOT_RUN: dashboard dependency audit' -or
        $ciScript -notmatch '--format json' -or
        $ciScript -notmatch 'Assert-NuGetVulnerabilityAuditJson' -or
        $ciScript -notmatch 'NOT_RUN: [. ]NET dependency audit') {
        throw "The public dependency-audit boundary is incomplete."
    }

    Write-Output "All CI policy tests passed."
}
finally {
    if (Test-Path -LiteralPath $temporaryRoot) {
        Remove-Item -LiteralPath $temporaryRoot -Recurse -Force
    }
}
