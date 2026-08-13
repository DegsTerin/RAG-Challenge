# Purpose: Starts the local Oracle Database 19c product runtime with the built Dashboard, persisted product stores and an existing non-secret credential reference.
[CmdletBinding()]
param(
    [ValidateRange(1024, 65535)]
    [int] $Port = 5189
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$serverDll = Join-Path $repositoryRoot 'src/RagChallenge.Server.Api/bin/Release/net10.0/RagChallenge.Server.Api.dll'
$dashboardRoot = Join-Path $repositoryRoot 'src/RagChallenge.Dashboard.Web/dist'
$storeRoot = Join-Path $repositoryRoot 'artifacts-local/state-07/oracle-19c-product/product-store'
$environmentFile = Join-Path $repositoryRoot '.env.local'

foreach ($requiredPath in $serverDll, (Join-Path $dashboardRoot 'index.html'), $storeRoot, $environmentFile) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Required product runtime path is unavailable: $requiredPath"
    }
}

$credentialName = 'OPENAI_API' + '_KEY'
$keyLine = Get-Content -LiteralPath $environmentFile |
    Where-Object { $_.StartsWith($credentialName + '=', [System.StringComparison]::Ordinal) } |
    Select-Object -First 1
if (-not $keyLine) {
    throw 'OPENAI_API_KEY is unavailable in .env.local.'
}

[System.Environment]::SetEnvironmentVariable(
    $credentialName,
    $keyLine.Substring($keyLine.IndexOf('=') + 1).Trim(),
    [System.EnvironmentVariableTarget]::Process)
$env:ASPNETCORE_URLS = "http://127.0.0.1:$Port"
$env:ASPNETCORE_ENVIRONMENT = 'Production'
$env:ASPNETCORE_WEBROOT = [System.IO.Path]::GetFullPath($dashboardRoot)
$env:RagChallenge__Setup__AllowExternalServices = 'true'
$env:RagChallenge__Product__Enabled = 'true'
$env:RagChallenge__Product__ApplyMigrations = 'true'
$env:RagChallenge__Product__StoreRoot = [System.IO.Path]::GetFullPath($storeRoot)
$env:RagChallenge__Product__CredentialEnvironmentVariable = $credentialName

try {
    & dotnet $serverDll
    exit $LASTEXITCODE
}
finally {
    [System.Environment]::SetEnvironmentVariable(
        $credentialName,
        $null,
        [System.EnvironmentVariableTarget]::Process)
}
