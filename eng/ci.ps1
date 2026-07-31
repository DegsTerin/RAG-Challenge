# Purpose: Runs the reproducible setup gate locally or in CI while keeping deployment and external product services out of scope.
[CmdletBinding()]
param(
    [switch]$Offline
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$dashboardRoot = Join-Path $repositoryRoot "src/Challenge.Dashboard.Web"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:DOTNET_NOLOGO = "1"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

function Assert-LastExitCode {
    param([Parameter(Mandatory)][string]$Operation)

    if ($LASTEXITCODE -ne 0) {
        throw "$Operation failed with exit code $LASTEXITCODE."
    }
}

Push-Location $repositoryRoot

try {
    if ($Offline) {
        dotnet restore Challenge.sln `
            --configfile eng/NuGet.Offline.config `
            --locked-mode
        Assert-LastExitCode "Offline .NET restore"
    }
    else {
        dotnet restore Challenge.sln --locked-mode
        Assert-LastExitCode ".NET restore"
    }

    dotnet format Challenge.sln --verify-no-changes --no-restore
    Assert-LastExitCode ".NET format verification"

    dotnet build Challenge.sln --configuration Release --no-restore
    Assert-LastExitCode ".NET Release build"

    $coverageRun = Join-Path "TestResults" ([guid]::NewGuid().ToString("N"))
    dotnet test Challenge.sln `
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
    }
    finally {
        Pop-Location
    }

    if (-not $Offline) {
        dotnet list Challenge.sln package --vulnerable --include-transitive
        Assert-LastExitCode ".NET dependency audit"
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
