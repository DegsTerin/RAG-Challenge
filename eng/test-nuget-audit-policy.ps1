# Purpose: Exercises the structured NuGet vulnerability-audit parser with clean, vulnerable, malformed and incomplete local fixtures.
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

function ConvertTo-AuditJson {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [object[]]$Projects,

        [int]$Version = 1,

        [string[]]$Sources = @('https://api.nuget.org/v3/index.json')
    )

    return @{
        version = $Version
        parameters = '--vulnerable --include-transitive'
        sources = @($Sources)
        projects = @($Projects)
    } | ConvertTo-Json -Depth 16 -Compress
}

$expectedProjects = @(
    Join-Path $PSScriptRoot "../src/RagChallenge.Domain/RagChallenge.Domain.csproj"
    Join-Path $PSScriptRoot "../src/RagChallenge.Application/RagChallenge.Application.csproj"
)
$cleanProjects = @(
    @{ path = [System.IO.Path]::GetFullPath($expectedProjects[0]) }
    @{ path = [System.IO.Path]::GetFullPath($expectedProjects[1]) }
)
$cleanJson = ConvertTo-AuditJson -Projects $cleanProjects

Invoke-ExpectedSuccess -Name "clean-exact-project-set" -Action {
    Assert-NuGetVulnerabilityAuditJson `
        -AuditJson $cleanJson `
        -ExpectedProjectPaths $expectedProjects
}

$vulnerableProjects = @(
    @{
        path = [System.IO.Path]::GetFullPath($expectedProjects[0])
        frameworks = @(
            @{
                framework = 'net10.0'
                transitivePackages = @(
                    @{
                        id = 'Synthetic.Package'
                        resolvedVersion = '1.0.0'
                        vulnerabilities = @(
                            @{
                                severity = 'High'
                                advisoryurl = 'https://example.invalid/advisory'
                            }
                        )
                    }
                )
            }
        )
    }
    @{ path = [System.IO.Path]::GetFullPath($expectedProjects[1]) }
)
$vulnerableJson = ConvertTo-AuditJson -Projects $vulnerableProjects
Invoke-ExpectedFailure `
    -Name "reported-vulnerability-fails" `
    -Action {
        Assert-NuGetVulnerabilityAuditJson `
            -AuditJson $vulnerableJson `
            -ExpectedProjectPaths $expectedProjects
    } `
    -ExpectedPattern "reported 1 vulnerability entries"

Invoke-ExpectedFailure `
    -Name "empty-output-fails" `
    -Action {
        Assert-NuGetVulnerabilityAuditJson `
            -AuditJson ' ' `
            -ExpectedProjectPaths $expectedProjects
    } `
    -ExpectedPattern "empty or oversized JSON"

Invoke-ExpectedFailure `
    -Name "malformed-output-fails" `
    -Action {
        Assert-NuGetVulnerabilityAuditJson `
            -AuditJson '{not-json' `
            -ExpectedProjectPaths $expectedProjects
    } `
    -ExpectedPattern "malformed JSON"

Invoke-ExpectedFailure `
    -Name "duplicate-property-fails" `
    -Action {
        Assert-NuGetVulnerabilityAuditJson `
            -AuditJson '{"version":1,"version":1}' `
            -ExpectedProjectPaths $expectedProjects
    } `
    -ExpectedPattern "duplicate property names"

Invoke-ExpectedFailure `
    -Name "unsupported-schema-fails" `
    -Action {
        Assert-NuGetVulnerabilityAuditJson `
            -AuditJson (ConvertTo-AuditJson -Projects $cleanProjects -Version 2) `
            -ExpectedProjectPaths $expectedProjects
    } `
    -ExpectedPattern "unsupported root schema"

Invoke-ExpectedFailure `
    -Name "truncated-project-set-fails" `
    -Action {
        Assert-NuGetVulnerabilityAuditJson `
            -AuditJson (ConvertTo-AuditJson -Projects @($cleanProjects[0])) `
            -ExpectedProjectPaths $expectedProjects
    } `
    -ExpectedPattern "does not cover the exact expected project set"

$problemJson = @{
    version = 1
    parameters = '--vulnerable --include-transitive'
    sources = @('https://api.nuget.org/v3/index.json')
    problems = @(
        @{
            project = 'SENSITIVE-PROJECT-SENTINEL'
            level = 'warning'
            text = 'SENSITIVE-DIAGNOSTIC-SENTINEL'
        }
    )
    projects = $cleanProjects
} | ConvertTo-Json -Depth 16 -Compress
Invoke-ExpectedFailure `
    -Name "diagnostic-problem-fails-without-echo" `
    -Action {
        Assert-NuGetVulnerabilityAuditJson `
            -AuditJson $problemJson `
            -ExpectedProjectPaths $expectedProjects
    } `
    -ExpectedPattern '^The NuGet vulnerability audit reported diagnostic problems[.]$'

$invalidPackageProjects = @(
    @{
        path = [System.IO.Path]::GetFullPath($expectedProjects[0])
        frameworks = @(
            @{
                framework = 'net10.0'
                topLevelPackages = @(
                    @{
                        id = 'Synthetic.Package'
                        resolvedVersion = '1.0.0'
                    }
                )
            }
        )
    }
    @{ path = [System.IO.Path]::GetFullPath($expectedProjects[1]) }
)
Invoke-ExpectedFailure `
    -Name "malformed-package-entry-fails" `
    -Action {
        Assert-NuGetVulnerabilityAuditJson `
            -AuditJson (ConvertTo-AuditJson -Projects $invalidPackageProjects) `
            -ExpectedProjectPaths $expectedProjects
    } `
    -ExpectedPattern "invalid vulnerable package entry"

Invoke-ExpectedFailure `
    -Name "non-https-source-fails" `
    -Action {
        Assert-NuGetVulnerabilityAuditJson `
            -AuditJson (ConvertTo-AuditJson -Projects $cleanProjects -Sources @('http://example.invalid/v3/index.json')) `
            -ExpectedProjectPaths $expectedProjects
    } `
    -ExpectedPattern "invalid package source"

Write-Output "All NuGet vulnerability-audit policy tests passed."
