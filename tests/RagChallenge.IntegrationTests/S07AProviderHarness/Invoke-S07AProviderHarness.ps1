# Purpose: Runs only the local frozen-manifest and fake-handler checks for the provider-candidate preparation, with no real-run mode or external transport composition.
[CmdletBinding()]
param(
    [ValidateSet('Validate')]
    [string] $Mode = 'Validate'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..'))
$projectPath = Join-Path $repositoryRoot 'tests\RagChallenge.IntegrationTests\RagChallenge.IntegrationTests.csproj'

if (-not (Test-Path -LiteralPath (Join-Path $repositoryRoot 'RAG-Challenge.sln') -PathType Leaf)) {
    throw 'The RAG-Challenge repository root could not be resolved.'
}

Push-Location $repositoryRoot
try {
    & dotnet test $projectPath `
        --configuration Release `
        --no-restore `
        --filter 'FullyQualifiedName~RagChallenge.IntegrationTests.S07AProviderHarness' `
        --logger 'console;verbosity=normal'
    if ($LASTEXITCODE -ne 0) {
        throw "The S07-A provider preparation validation failed with exit code $LASTEXITCODE."
    }
}
finally {
    Pop-Location
}
