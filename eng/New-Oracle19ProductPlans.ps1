# Purpose: Generates deterministic local administration plans for the Oracle Database 19c MVP while keeping every other canonical product Candidate.
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $OutputDirectory
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$fixturePath = Join-Path $repositoryRoot 'tests/RagChallenge.UnitTests/TestData/initial-catalogue-v1.json'
$fixture = Get-Content -LiteralPath $fixturePath -Raw | ConvertFrom-Json
$output = [System.IO.Path]::GetFullPath($OutputDirectory)
[System.IO.Directory]::CreateDirectory($output) | Out-Null

$documentId = 'oracle-database-19c-concepts'
$contentObjectId = '6a10b7840c42a1dd6ea9b69337532ed3f903d17af24f144c2a104b925f6533d2'
$byteLength = 9322921

function Write-Plan([string] $Name, [object] $Value) {
    $path = Join-Path $output $Name
    $json = $Value | ConvertTo-Json -Depth 20
    [System.IO.File]::WriteAllText($path, $json + "`n", [System.Text.UTF8Encoding]::new($false))
}

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
    Write-Plan ('catalogue-{0:D2}-add-{1}.json' -f $revision, $sourceProduct.id) ([ordered]@{
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
Write-Plan 'catalogue-52-add-oracle-document.json' ([ordered]@{
    targetId = $documentId
    targetVersion = 1
    expectedCurrentRevision = $revision - 1
    revision = $revision
    categories = $allCategories
    databaseProducts = @($products)
    documentVersions = @($document)
})

Write-Output "Generated 52 candidate catalogue plans in $output. Rights-dependent activation, rendering and indexing remain unavailable until an approved Oracle evidence reference is registered."
