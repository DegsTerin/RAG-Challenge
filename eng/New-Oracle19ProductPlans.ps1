# Purpose: Generates deterministic local administration plans for the Oracle Database 19c MVP while keeping every other canonical product Candidate.
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[a-z0-9](?:[a-z0-9-]{0,62}[a-z0-9])?$')]
    [string] $TaskId
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
Set-StrictMode -Version Latest

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$fixturePath = Join-Path $repositoryRoot 'tests/RagChallenge.UnitTests/TestData/initial-catalogue-v1.json'
$fixture = Get-Content -LiteralPath $fixturePath -Raw | ConvertFrom-Json
$taskOutputRoot = [System.IO.Path]::GetFullPath((
        Join-Path $repositoryRoot 'artifacts-local/oracle19-product-plans'))
$output = [System.IO.Path]::GetFullPath((Join-Path $taskOutputRoot $TaskId))
$expectedParent = [System.IO.Path]::GetFullPath((Split-Path -Parent $output))
$utf8WithoutBom = [System.Text.UTF8Encoding]::new($false)
$plans = [System.Collections.Generic.List[object]]::new()

if ($expectedParent -cne $taskOutputRoot -or
    (Split-Path -Leaf $output) -cne $TaskId) {
    throw 'The Oracle plan output escaped its owned task namespace.'
}

function Assert-ExistingPathComponentsAreSafe([string] $Path) {
    $relativePath = [System.IO.Path]::GetRelativePath($repositoryRoot, $Path)
    if ($relativePath -eq '..' -or
        $relativePath.StartsWith(
            '..' + [System.IO.Path]::DirectorySeparatorChar,
            [System.StringComparison]::Ordinal)) {
        throw 'The Oracle plan output escaped the repository root.'
    }

    $currentPath = [System.IO.Path]::GetFullPath($repositoryRoot)
    foreach ($segment in $relativePath.Split(
            [System.IO.Path]::DirectorySeparatorChar,
            [System.StringSplitOptions]::RemoveEmptyEntries)) {
        $currentPath = Join-Path $currentPath $segment
        if (-not (Test-Path -LiteralPath $currentPath)) {
            continue
        }

        $item = Get-Item -LiteralPath $currentPath -Force
        if (-not $item.PSIsContainer -or
            ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
            throw "The Oracle plan output contains an unsafe existing path component."
        }
    }
}

function Add-Plan([string] $Name, [object] $Value) {
    if ($Name -notmatch '^catalogue-[0-9]{2}-[a-z0-9-]+[.]json$') {
        throw "The Oracle plan name is invalid."
    }

    $json = ($Value | ConvertTo-Json -Depth 20).Replace("`r`n", "`n") + "`n"
    $plans.Add([pscustomobject]@{
            Name = $Name
            Content = $json
        })
}

function Write-NewUtf8File([string] $Path, [string] $Content) {
    $bytes = $utf8WithoutBom.GetBytes($Content)
    $stream = [System.IO.File]::Open(
        $Path,
        [System.IO.FileMode]::CreateNew,
        [System.IO.FileAccess]::Write,
        [System.IO.FileShare]::None)
    try {
        $stream.Write($bytes, 0, $bytes.Length)
        $stream.Flush($true)
    }
    finally {
        $stream.Dispose()
    }
}

$documentId = 'oracle-database-19c-concepts'
$contentObjectId = '6a10b7840c42a1dd6ea9b69337532ed3f903d17af24f144c2a104b925f6533d2'
$byteLength = 9322921

$products = @()
$introducedCategoryIds = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
$revision = 0

foreach ($sourceProduct in $fixture.products) {
    $revision++
    foreach ($categoryId in $sourceProduct.categoryIds) {
        [void]$introducedCategoryIds.Add([string]$categoryId)
    }

    $products += [ordered]@{
        id = [string]$sourceProduct.id
        revision = 1
        displayName = [string]$sourceProduct.displayName
        status = 'Candidate'
        categoryIds = @($sourceProduct.categoryIds)
    }
    $categories = @($fixture.categories | Where-Object { $introducedCategoryIds.Contains([string]$_.id) } | ForEach-Object {
        [ordered]@{ id = [string]$_.id; displayName = [string]$_.displayName }
    })
    Add-Plan ('catalogue-{0:D2}-add-{1}.json' -f $revision, $sourceProduct.id) ([ordered]@{
        targetId = [string]$sourceProduct.id
        targetVersion = $null
        expectedCurrentRevision = $revision - 1
        revision = $revision
        categories = $categories
        databaseProducts = @($products)
        documentVersions = @()
    })
}

$allCategories = @($fixture.categories | ForEach-Object {
    [ordered]@{ id = [string]$_.id; displayName = [string]$_.displayName }
})
$document = [ordered]@{
    id = $documentId
    version = 1
    databaseProductId = 'oracle-database'
    databaseProductRevision = 1
    format = 'Pdf'
    contentLanguage = 'en'
    sourceDeclaredLanguage = 'en'
    status = 'Candidate'
    contentObjectId = $contentObjectId
    byteLength = $byteLength
    mediaType = 'application/pdf'
    sourceAdapterId = 'local-authorised-pdf-v1'
    sourceTrustClass = 'LocalAuthorised'
    officialSourceRegistrationId = $null
    officialSnapshotId = $null
}

$revision++
Add-Plan 'catalogue-52-add-oracle-document.json' ([ordered]@{
    targetId = $documentId
    targetVersion = 1
    expectedCurrentRevision = $revision - 1
    revision = $revision
    categories = $allCategories
    databaseProducts = @($products)
    documentVersions = @($document)
})

if ($plans.Count -ne 52 -or
    @($plans.Name | Sort-Object -Unique).Count -ne $plans.Count) {
    throw 'The Oracle plan set is incomplete or contains duplicate names.'
}

Assert-ExistingPathComponentsAreSafe -Path $output
if (Test-Path -LiteralPath $output) {
    throw 'The Oracle plan task output already exists and will not be overwritten.'
}

[System.IO.Directory]::CreateDirectory($taskOutputRoot) | Out-Null
Assert-ExistingPathComponentsAreSafe -Path $taskOutputRoot
[System.IO.Directory]::CreateDirectory($output) | Out-Null
Assert-ExistingPathComponentsAreSafe -Path $output

$marker = [ordered]@{
    schemaVersion = 1
    taskId = $TaskId
    purpose = 'oracle19-product-plans'
} | ConvertTo-Json -Compress
Write-NewUtf8File `
    -Path (Join-Path $output '.rag-challenge-owned-output.json') `
    -Content ($marker + "`n")

foreach ($plan in $plans) {
    Write-NewUtf8File `
        -Path (Join-Path $output $plan.Name) `
        -Content $plan.Content
}

Write-Output "Generated 52 candidate catalogue plans in the owned task namespace '$TaskId'. Rights-dependent activation, rendering and indexing remain unavailable until an approved Oracle evidence reference is registered."
