# Purpose: Starts the PostgreSQL 18.4 product runtime from the governed notice-bearing store without reading or mutating the retained Oracle corpus.
[CmdletBinding()]
param(
    [ValidateRange(1024, 65535)]
    [int] $Port = 5189,

    [Parameter(Mandatory)]
    [ValidatePattern('^AUTH-[A-Z0-9][A-Z0-9-]{2,122}$')]
    [string] $QueryEmbeddingAuthorityReference,

    [Parameter(Mandatory)]
    [ValidatePattern('^AUTH-[A-Z0-9][A-Z0-9-]{2,122}$')]
    [string] $GroundedGenerationAuthorityReference
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$serverDll = Join-Path $repositoryRoot 'src/RagChallenge.Server.Api/bin/Release/net10.0/RagChallenge.Server.Api.dll'
$dashboardRoot = Join-Path $repositoryRoot 'src/RagChallenge.Dashboard.Web/dist'
$storeRoot = Join-Path $repositoryRoot 'artifacts-local/state-07/product-materialisation/postgresql-18-reference-a4/product-store'
$approvedRightsEvidenceReference = 'auth-s07-a-product-a0-003'

foreach ($requiredPath in $serverDll, (Join-Path $dashboardRoot 'index.html'), $storeRoot) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Required product runtime path is unavailable: $requiredPath"
    }
}

$credentialName = 'OPENAI_API' + '_KEY'
[System.Environment]::SetEnvironmentVariable(
    $credentialName,
    $null,
    [System.EnvironmentVariableTarget]::Process)
$env:ASPNETCORE_URLS = "http://127.0.0.1:$Port"
$env:ASPNETCORE_ENVIRONMENT = 'Production'
$env:ASPNETCORE_WEBROOT = [System.IO.Path]::GetFullPath($dashboardRoot)
$env:RagChallenge__Setup__AllowExternalServices = 'true'
$env:RagChallenge__Product__Enabled = 'true'
$env:RagChallenge__Product__ApplyMigrations = 'true'
$env:RagChallenge__Product__StoreRoot = [System.IO.Path]::GetFullPath($storeRoot)
$env:RagChallenge__Product__CatalogueProfile = 'postgresql-18.4'
$env:RagChallenge__Product__ApprovedRightsEvidenceReference =
    $approvedRightsEvidenceReference
$env:RagChallenge__Product__CredentialEnvironmentVariable = $credentialName
$env:RagChallenge__Product__QueryEmbeddingAuthorityReference =
    $QueryEmbeddingAuthorityReference
$env:RagChallenge__Product__GroundedGenerationAuthorityReference =
    $GroundedGenerationAuthorityReference

& dotnet $serverDll
exit $LASTEXITCODE
