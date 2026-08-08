# Purpose: Runs either the non-campaign harness checks or the single explicitly authorised A3 entry point without restore, network, or non-task-owned stores.
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet("Validate", "Run")]
    [string]$Mode,

    [string]$AuthorityId
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = [System.IO.Path]::GetFullPath(
    (Join-Path $PSScriptRoot "..\..\.."))
$projectPath = Join-Path $repositoryRoot (
    "tests\RagChallenge.IntegrationTests\RagChallenge.IntegrationTests.csproj")
$authorityVariable = "RAGCHALLENGE_S07_A_RUN_AUTHORITY"
$expectedAuthority = "AUTH-S07-A-RUN-001"
$previousAuthority = [System.Environment]::GetEnvironmentVariable(
    $authorityVariable,
    [System.EnvironmentVariableTarget]::Process)

if (-not (Test-Path -LiteralPath (Join-Path $repositoryRoot "RAG-Challenge.sln") -PathType Leaf)) {
    throw "The RAG-Challenge repository root could not be resolved."
}

if ($Mode -eq "Run" -and $AuthorityId -cne $expectedAuthority) {
    throw "The future A3 command requires the exact AUTH-S07-A-RUN-001 authority."
}

if ($Mode -eq "Validate" -and -not [string]::IsNullOrEmpty($AuthorityId)) {
    throw "Harness validation does not accept run authority."
}

$filter = if ($Mode -eq "Run") {
    "FullyQualifiedName=RagChallenge.IntegrationTests.S07ALocalHarness.S07ALocalHarnessCampaignTests.ExecuteFrozenCandidateAsync"
}
else {
    "FullyQualifiedName~RagChallenge.IntegrationTests.S07ALocalHarness.S07ALocalHarnessTests"
}

Push-Location $repositoryRoot

try {
    if ($Mode -eq "Run") {
        [System.Environment]::SetEnvironmentVariable(
            $authorityVariable,
            $expectedAuthority,
            [System.EnvironmentVariableTarget]::Process)
    }
    else {
        [System.Environment]::SetEnvironmentVariable(
            $authorityVariable,
            $null,
            [System.EnvironmentVariableTarget]::Process)
    }

    & dotnet test $projectPath `
        --configuration Release `
        --no-restore `
        --filter $filter `
        --logger "console;verbosity=normal"

    if ($LASTEXITCODE -ne 0) {
        throw "The S07-A local harness command failed with exit code $LASTEXITCODE."
    }
}
finally {
    [System.Environment]::SetEnvironmentVariable(
        $authorityVariable,
        $previousAuthority,
        [System.EnvironmentVariableTarget]::Process)
    Pop-Location
}
