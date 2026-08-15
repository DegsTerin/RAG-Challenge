# Purpose: Starts the local Oracle Database 19c product runtime with the built Dashboard, persisted product stores and an existing non-secret credential reference.
[CmdletBinding()]
param(
    [ValidateRange(1024, 65535)]
    [int] $Port = 5189,

    [string] $ApprovedRightsEvidenceReference = '',

    [Parameter(Mandatory)]
    [ValidatePattern('^AUTH-[A-Z0-9][A-Z0-9-]{2,122}$')]
    [string] $QueryEmbeddingAuthorityReference,

    [Parameter(Mandatory)]
    [ValidatePattern('^AUTH-[A-Z0-9][A-Z0-9-]{2,122}$')]
    [string] $GroundedGenerationAuthorityReference
)

$ErrorActionPreference = 'Stop'
$supersededUnverifiedRightsEvidenceReference =
    'owner-oracle19-public-source-approval-2026-08-12'
if ([string]::IsNullOrWhiteSpace($ApprovedRightsEvidenceReference) -or
    $ApprovedRightsEvidenceReference -notmatch '^[A-Za-z0-9][A-Za-z0-9._:-]{0,127}$' -or
    $ApprovedRightsEvidenceReference -ceq $supersededUnverifiedRightsEvidenceReference) {
    throw 'An exact approved Oracle rights evidence reference is required before product startup.'
}

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$serverDll = Join-Path $repositoryRoot 'src/RagChallenge.Server.Api/bin/Release/net10.0/RagChallenge.Server.Api.dll'
$dashboardRoot = Join-Path $repositoryRoot 'src/RagChallenge.Dashboard.Web/dist'
$storeRoot = Join-Path $repositoryRoot 'artifacts-local/state-07/oracle-19c-product/product-store'

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
$env:RagChallenge__Product__CatalogueProfile = 'oracle-database-19c'
$env:RagChallenge__Product__ApprovedRightsEvidenceReference =
    $ApprovedRightsEvidenceReference
$env:RagChallenge__Product__CredentialEnvironmentVariable = $credentialName
$env:RagChallenge__Product__QueryEmbeddingAuthorityReference =
    $QueryEmbeddingAuthorityReference
$env:RagChallenge__Product__GroundedGenerationAuthorityReference =
    $GroundedGenerationAuthorityReference

& dotnet $serverDll
exit $LASTEXITCODE
