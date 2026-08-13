# Purpose: Generates deterministic local administration plans for the Oracle Database 19c MVP while keeping every other canonical product Candidate.
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $OutputDirectory,

    [string] $RenderManifestId = ''
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
$evidenceReference = 'owner-oracle19-public-source-approval-2026-08-12'
$sourceReference = 'https://docs.oracle.com/en/database/oracle/oracle-database/19/cncpt/database-concepts.pdf'
$assessedAt = '2026-08-12T12:00:00.0000000+00:00'
$rights = @(
    [ordered]@{ right = 'SourcePossessionOrDownload'; state = 'Permitted'; evidenceReference = $evidenceReference },
    [ordered]@{ right = 'ParsingAndTextualTransformation'; state = 'Permitted'; evidenceReference = $evidenceReference },
    [ordered]@{ right = 'Indexing'; state = 'Permitted'; evidenceReference = $evidenceReference },
    [ordered]@{ right = 'SourceByteRetention'; state = 'Permitted'; evidenceReference = $evidenceReference },
    [ordered]@{ right = 'QuotationAndCitation'; state = 'Permitted'; evidenceReference = $evidenceReference },
    [ordered]@{ right = 'PageRendering'; state = 'Permitted'; evidenceReference = $evidenceReference },
    [ordered]@{ right = 'DerivativeImageCreationAndRetention'; state = 'Permitted'; evidenceReference = $evidenceReference },
    [ordered]@{ right = 'RuntimeDerivativeImageDisplay'; state = 'Permitted'; evidenceReference = $evidenceReference },
    [ordered]@{ right = 'SourceAndDerivativeByteDistributionOrPublication'; state = 'Denied'; evidenceReference = $evidenceReference },
    [ordered]@{ right = 'AttributionNoticeTrademarkAndChangeMarkingRequirements'; state = 'Permitted'; evidenceReference = $evidenceReference }
)

function Write-Plan([string] $Name, [object] $Value) {
    $path = Join-Path $output $Name
    $json = $Value | ConvertTo-Json -Depth 20
    [System.IO.File]::WriteAllText($path, $json + "`n", [System.Text.UTF8Encoding]::new($false))
}

function Add-CanonicalField([System.Text.StringBuilder] $Builder, [string] $Name, [string] $Value) {
    $length = [System.Text.Encoding]::UTF8.GetByteCount($Value)
    [void]$Builder.Append($Name).Append(':').Append($length).Append(':').Append($Value).Append("`n")
}

function Get-Sha256([string] $Value) {
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Value)
    $hash = [System.Security.Cryptography.SHA256]::HashData($bytes)
    return [Convert]::ToHexString($hash).ToLowerInvariant()
}

function Add-BindingToken([System.Text.StringBuilder] $Builder, [AllowNull()][object] $Value) {
    if ($null -eq $Value) {
        [void]$Builder.Append('-1:')
        return
    }
    $text = [string]$Value
    [void]$Builder.Append([System.Text.Encoding]::UTF8.GetByteCount($text)).Append(':').Append($text)
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

$activeProducts = @($products | ForEach-Object {
    $copy = [ordered]@{}
    foreach ($entry in $_.GetEnumerator()) { $copy[$entry.Key] = $entry.Value }
    if ($copy.id -eq 'oracle-database') { $copy.status = 'Active' }
    $copy
})
$activeDocument = [ordered]@{}
foreach ($entry in $document.GetEnumerator()) { $activeDocument[$entry.Key] = $entry.Value }
$activeDocument.status = 'Active'
$revision++
Write-Plan 'catalogue-53-activate-oracle.json' ([ordered]@{
    targetId = 'oracle-database'
    targetVersion = $null
    expectedCurrentRevision = $revision - 1
    revision = $revision
    categories = $allCategories
    databaseProducts = $activeProducts
    documentVersions = @($activeDocument)
})

$rightsBuilder = [System.Text.StringBuilder]::new()
Add-CanonicalField $rightsBuilder 'canonicalDomain' 'rag-challenge/document-rights-mapping/v1'
Add-CanonicalField $rightsBuilder 'rightsSchemaVersion' '1'
Add-CanonicalField $rightsBuilder 'documentId' $documentId
Add-CanonicalField $rightsBuilder 'documentVersion' '1'
foreach ($decision in $rights) {
    Add-CanonicalField $rightsBuilder 'decision.right' $decision.right
    Add-CanonicalField $rightsBuilder 'decision.state' $decision.state
    Add-CanonicalField $rightsBuilder 'decision.evidenceReference' $decision.evidenceReference
}
$rightsRevision = 'rights-map-v1:' + (Get-Sha256 $rightsBuilder.ToString())

$obligation = [ordered]@{
    contentLanguage = 'en'
    authoritativePublisherOrAuthor = 'Oracle Corporation and/or its affiliates'
    documentTitle = 'Oracle Database Concepts 19c'
    documentVersionLabel = '19c'
    sourceReference = $sourceReference
    attributionText = "Source: Oracle Corporation; document: Oracle Database Concepts 19c; source reference: $sourceReference"
    copyrightNotice = 'Copyright (c) 1996, 2026, Oracle and/or its affiliates.'
    permissionNotice = 'Locally retained and processed for the owner-approved RAG-Challenge Oracle Database 19c MVP. Redistribution is not permitted by this record.'
    orderedDisclaimers = @('Oracle and Java are registered trademarks of Oracle and/or its affiliates. Other names may be trademarks of their respective owners.')
    trademarkTreatment = 'Required'
    trademarkOrNonEndorsementText = 'Oracle is a registered trademark of Oracle Corporation and/or its affiliates. RAG-Challenge is independent and does not imply Oracle endorsement.'
    changeMarkingText = 'The notice-bearing composite PNG is a RAG-Challenge derivative. The source-page region is retained as visual evidence and is not presented as an Oracle-created composite.'
    assessedAt = $assessedAt
    assessorId = 'assessor:rag-challenge-owner-2026-08-12'
}
$obligationBuilder = [System.Text.StringBuilder]::new()
Add-CanonicalField $obligationBuilder 'canonicalDomain' 'rag-challenge/derivative-obligation-set/v1'
Add-CanonicalField $obligationBuilder 'schemaVersion' '1'
Add-CanonicalField $obligationBuilder 'documentId' $documentId
Add-CanonicalField $obligationBuilder 'documentVersion' '1'
Add-CanonicalField $obligationBuilder 'sourceContentObjectId' $contentObjectId
Add-CanonicalField $obligationBuilder 'rightsMappingRevision' $rightsRevision
Add-CanonicalField $obligationBuilder 'evidenceReference' $evidenceReference
foreach ($name in 'contentLanguage','authoritativePublisherOrAuthor','documentTitle','documentVersionLabel','sourceReference','attributionText','copyrightNotice','permissionNotice') {
    Add-CanonicalField $obligationBuilder $name ([string]$obligation[$name])
}
foreach ($disclaimer in $obligation.orderedDisclaimers) { Add-CanonicalField $obligationBuilder 'disclaimer' $disclaimer }
foreach ($name in 'trademarkTreatment','trademarkOrNonEndorsementText','changeMarkingText') {
    Add-CanonicalField $obligationBuilder $name ([string]$obligation[$name])
}
Add-CanonicalField $obligationBuilder 'placementMode' 'VisibleInBinaryAndAccessibleContext'
Add-CanonicalField $obligationBuilder 'assessedAt' $assessedAt
Add-CanonicalField $obligationBuilder 'assessorId' $obligation.assessorId
$obligationSha = Get-Sha256 $obligationBuilder.ToString()

$rightsPayload = [ordered]@{
    rightsSchemaVersion = 1
    rightsDecisions = $rights
    documentId = $documentId
    documentVersion = 1
}
Write-Plan 'render-oracle-document.json' ([ordered]@{
    documentId = $documentId
    documentVersion = 1
    sourceContentObjectId = $contentObjectId
    sourceByteLength = $byteLength
    generatedAt = $assessedAt
    rights = $rightsPayload
    renderPolicy = [ordered]@{
        maximumSourceByteLength = 33554432
        maximumPageCount = 1000
        maximumTotalPixels = 8000000000
        maximumPageOutputByteLength = 20971520
        maximumTotalOutputByteLength = 68719476736
        maximumWorkerMemoryBytes = 1073741824
        maximumWorkerCpuMilliseconds = 1800000
        workerTimeoutMilliseconds = 3600000
    }
    obligationSet = [ordered]@{
        schemaVersion = 1
        expectedObligationSetId = 'obligationset-' + $obligationSha
        expectedCanonicalSha256 = $obligationSha
        expectedRightsMappingRevision = $rightsRevision
        orderedEvidenceReferences = @($evidenceReference)
        contentLanguage = $obligation.contentLanguage
        authoritativePublisherOrAuthor = $obligation.authoritativePublisherOrAuthor
        documentTitle = $obligation.documentTitle
        documentVersionLabel = $obligation.documentVersionLabel
        sourceReference = $obligation.sourceReference
        attributionText = $obligation.attributionText
        copyrightNotice = $obligation.copyrightNotice
        permissionNotice = $obligation.permissionNotice
        orderedDisclaimers = $obligation.orderedDisclaimers
        trademarkTreatment = $obligation.trademarkTreatment
        trademarkOrNonEndorsementText = $obligation.trademarkOrNonEndorsementText
        changeMarkingText = $obligation.changeMarkingText
        assessedAt = $obligation.assessedAt
        assessorId = $obligation.assessorId
    }
})

if (-not [string]::IsNullOrWhiteSpace($RenderManifestId)) {
    $activeBinding = [System.Text.StringBuilder]::new()
    Add-BindingToken $activeBinding 'rag-challenge/active-document-set/v1'
    Add-BindingToken $activeBinding '1'
    foreach ($value in 'oracle-database','1',$documentId,'1','Pdf') { Add-BindingToken $activeBinding $value }
    $sourceBinding = [System.Text.StringBuilder]::new()
    Add-BindingToken $sourceBinding 'rag-challenge/source-binding-set/v1'
    Add-BindingToken $sourceBinding '1'
    foreach ($value in 'oracle-database','1',$documentId,'1','Pdf','local-authorised-pdf-v1','LocalAuthorised') {
        Add-BindingToken $sourceBinding $value
    }
    Add-BindingToken $sourceBinding $null
    Add-BindingToken $sourceBinding $null

    Write-Plan 'build-oracle-index.json' ([ordered]@{
        candidateBuildId = 'oracle-database-19c-build-v4'
        corpusRevision = 1
        catalogueRevision = 53
        activeDocumentSetDigest = Get-Sha256 $activeBinding.ToString()
        sourceBindingSetDigest = Get-Sha256 $sourceBinding.ToString()
        expectedIndexCompatibilityKey = 'd0890e6a252b37a84451bebc0814b897e0e146e3a7ec397a51f6475a25f45ddb'
        # Keeps each 1,536-dimensional JSON response below the adapter's 2 MiB limit.
        maximumEmbeddingBatchUtf8Bytes = 32768
        documents = @([ordered]@{
            binding = [ordered]@{
                databaseProductId = 'oracle-database'
                databaseProductRevision = 1
                documentId = $documentId
                documentVersion = 1
                documentFormat = 'Pdf'
                sourceAdapterId = 'local-authorised-pdf-v1'
                sourceTrustClass = 'LocalAuthorised'
                officialSourceRegistrationId = $null
                officialSnapshotId = $null
                sourceObservationId = $null
            }
            contentLanguage = 'en'
            sourceContentObjectId = $contentObjectId
            byteLength = $byteLength
            mediaType = 'application/pdf'
            parserPolicy = [ordered]@{
                maximumByteLength = 33554432
                maximumUnits = 1000
                maximumTextCharacters = 16000000
                maximumFieldsPerRecord = 256
                maximumFieldCharacters = 16384
            }
            rights = $rightsPayload
        })
        activationPlan = [ordered]@{
            expectedCurrentRevision = 0
            previousGenerationRetentionDays = 14
            documentRenderManifests = @([ordered]@{
                documentId = $documentId
                documentVersion = 1
                renderManifestId = $RenderManifestId
            })
        }
    })
}

$indexDescription = if ([string]::IsNullOrWhiteSpace($RenderManifestId)) { '' } else { ', and one index plan' }
Write-Output "Generated 53 catalogue plans, one render plan$indexDescription in $output."
