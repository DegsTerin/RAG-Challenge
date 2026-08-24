# Purpose: Runs the reproducible setup gate locally or in CI while keeping deployment and external product services out of scope.
[CmdletBinding()]
param(
    [switch]$Offline
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

# Executable policy enforcement: remove the exact product credential from the
# CI process without reading it before any policy or child-process invocation.
[System.Environment]::SetEnvironmentVariable(
    'OPENAI_API_KEY',
    $null,
    [System.EnvironmentVariableTarget]::Process)

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$dashboardRoot = Join-Path $repositoryRoot "src/RagChallenge.Dashboard.Web"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:DOTNET_NOLOGO = "1"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

. (Join-Path $PSScriptRoot "ci-policy.ps1")

function Assert-LastExitCode {
    param([Parameter(Mandatory)][string]$Operation)

    if ($LASTEXITCODE -ne 0) {
        throw "$Operation failed with exit code $LASTEXITCODE."
    }
}

function Assert-NuGetLockFileLineEndings {
    $trackedLockFiles = @(& git ls-files -- "*packages.lock.json")
    Assert-LastExitCode "Tracked NuGet lockfile discovery"

    if ($trackedLockFiles.Count -eq 0) {
        throw "No tracked NuGet lockfiles were found."
    }

    $absolutePaths = @(
        $trackedLockFiles | ForEach-Object {
            Join-Path $repositoryRoot $_
        })

    Assert-FilesUseLfOnly -Paths $absolutePaths
}

function Assert-NpmLockFileLineEndings {
    $trackedLockFiles = @(& git ls-files -- "*package-lock.json")
    Assert-LastExitCode "Tracked npm lockfile discovery"

    if ($trackedLockFiles.Count -eq 0) {
        throw "No tracked npm lockfiles were found."
    }

    $absolutePaths = @(
        $trackedLockFiles | ForEach-Object {
            Join-Path $repositoryRoot $_
        })

    Assert-FilesUseLfOnly -Paths $absolutePaths
}

Push-Location $repositoryRoot

try {
    Invoke-RequiredPolicyTest `
        -Name "fail-closed coverage aggregation" `
        -ScriptPath (Join-Path $PSScriptRoot "test-assert-coverage.ps1")
    Invoke-RequiredPolicyTest `
        -Name "shared CI policy" `
        -ScriptPath (Join-Path $PSScriptRoot "test-ci-policy.ps1")
    Invoke-RequiredPolicyTest `
        -Name "structured NuGet vulnerability audit" `
        -ScriptPath (Join-Path $PSScriptRoot "test-nuget-audit-policy.ps1")
    Invoke-RequiredPolicyTest `
        -Name "contained Render runtime-store recreation" `
        -ScriptPath (Join-Path $PSScriptRoot "test-render-entrypoint-policy.ps1")
    Invoke-RequiredPolicyTest `
        -Name "owned Render package output" `
        -ScriptPath (Join-Path $PSScriptRoot "test-render-free-output-policy.ps1")
    Invoke-RequiredPolicyTest `
        -Name "trusted integration archive" `
        -ScriptPath (Join-Path $PSScriptRoot "test-integration-archive-policy.ps1")

    Assert-NuGetLockFileLineEndings
    Assert-NpmLockFileLineEndings

    if ($Offline) {
        dotnet restore RAG-Challenge.sln `
            --configfile eng/NuGet.Offline.config `
            --locked-mode
        Assert-LastExitCode "Offline .NET restore"
    }
    else {
        dotnet restore RAG-Challenge.sln --locked-mode
        Assert-LastExitCode ".NET restore"
    }

    Assert-NuGetLockFileLineEndings

    dotnet format RAG-Challenge.sln --verify-no-changes --no-restore
    Assert-LastExitCode ".NET format verification"

    dotnet build RAG-Challenge.sln --configuration Release --no-restore
    Assert-LastExitCode ".NET Release build"

    $coverageRun = Join-Path "TestResults" ([guid]::NewGuid().ToString("N"))
    dotnet test RAG-Challenge.sln `
        --configuration Release `
        --no-build `
        --no-restore `
        --collect:"XPlat Code Coverage" `
        --results-directory $coverageRun
    Assert-LastExitCode ".NET tests"
    & "$PSScriptRoot/assert-coverage.ps1" -ResultsDirectory $coverageRun

    Push-Location $dashboardRoot

    try {
        if ($Offline) {
            npm ci --offline --ignore-scripts --no-audit --no-fund
            Assert-LastExitCode "Offline dashboard restore"
        }
        else {
            npm ci --ignore-scripts --no-audit --no-fund
            Assert-LastExitCode "Dashboard restore"
        }

        npm run lint
        Assert-LastExitCode "Dashboard lint"
        npm run typecheck
        Assert-LastExitCode "Dashboard type check"
        npm test
        Assert-LastExitCode "Dashboard tests"
        npm run build
        Assert-LastExitCode "Dashboard build"

        if (-not $Offline) {
            npm audit --audit-level=high
            Assert-LastExitCode "Dashboard dependency audit"
        }
        else {
            Write-Output "NOT_RUN: dashboard dependency audit requires online registry metadata."
        }
    }
    finally {
        Pop-Location
    }

    Assert-NpmLockFileLineEndings

    if (-not $Offline) {
        $trackedProjects = @(& git ls-files -- "*.csproj")
        Assert-LastExitCode "Tracked .NET project discovery"
        if ($trackedProjects.Count -eq 0) {
            throw "No tracked .NET projects were found for dependency auditing."
        }

        $expectedProjectPaths = @(
            $trackedProjects | ForEach-Object {
                Join-Path $repositoryRoot $_
            })
        $auditOutput = @(& dotnet list RAG-Challenge.sln package `
                --vulnerable `
                --include-transitive `
                --format json `
                --output-version 1 `
                --no-restore)
        $auditExitCode = $LASTEXITCODE
        if ($auditExitCode -ne 0) {
            throw ".NET dependency audit failed with exit code $auditExitCode."
        }

        Assert-NuGetVulnerabilityAuditJson `
            -AuditJson ($auditOutput -join [System.Environment]::NewLine) `
            -ExpectedProjectPaths $expectedProjectPaths
    }
    else {
        Write-Output "NOT_RUN: .NET dependency audit requires online NuGet metadata."
    }

    & "$PSScriptRoot/check-repository.ps1"

    if ($LASTEXITCODE -ne 0) {
        throw "Repository audit failed with exit code $LASTEXITCODE."
    }

    git diff --check
    Assert-LastExitCode "Git diff hygiene"
}
finally {
    Pop-Location
}
