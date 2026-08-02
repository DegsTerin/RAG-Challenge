# Purpose: Runs the reproducible setup gate locally or in CI while keeping deployment and external product services out of scope.
[CmdletBinding()]
param(
    [switch]$Offline
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$dashboardRoot = Join-Path $repositoryRoot "src/RagChallenge.Dashboard.Web"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:DOTNET_NOLOGO = "1"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

function Assert-LastExitCode {
    param([Parameter(Mandatory)][string]$Operation)

    if ($LASTEXITCODE -ne 0) {
        throw "$Operation failed with exit code $LASTEXITCODE."
    }
}

function Convert-NuGetLockFileLineEndings {
    $trackedLockFiles = & git ls-files -- "*packages.lock.json"
    Assert-LastExitCode "Tracked NuGet lockfile discovery"

    $utf8WithoutBom = [System.Text.UTF8Encoding]::new($false)
    $normalisedFiles = [System.Collections.Generic.List[string]]::new()

    foreach ($relativePath in $trackedLockFiles) {
        $absolutePath = Join-Path $repositoryRoot $relativePath
        $content = [System.IO.File]::ReadAllText($absolutePath)
        $normalisedContent = $content.Replace("`r`n", "`n").Replace("`r", "`n")

        if ($content -cne $normalisedContent) {
            [System.IO.File]::WriteAllText(
                $absolutePath,
                $normalisedContent,
                $utf8WithoutBom)
            $normalisedFiles.Add($relativePath)
        }
    }

    if ($normalisedFiles.Count -gt 0) {
        Write-Output (
            "Normalised NuGet lockfiles to repository LF endings: " +
            ($normalisedFiles -join ", "))
    }
}

Push-Location $repositoryRoot

try {
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

    Convert-NuGetLockFileLineEndings

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
    }
    finally {
        Pop-Location
    }

    if (-not $Offline) {
        dotnet list RAG-Challenge.sln package --vulnerable --include-transitive
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
