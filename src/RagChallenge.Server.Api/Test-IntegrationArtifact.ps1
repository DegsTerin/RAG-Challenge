# Purpose: Reproduces the local STATE-07 v2 artefact over loopback, verifies same-origin visual serving, restart and confined cold restore, and removes only task-owned runtime data.
[CmdletBinding()]
param(
    [string]$OutputRoot = "artifacts-local/state07-v2-integration",
    [ValidateRange(1024, 65535)]
    [int]$Port = 5086
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$serverRoot = $PSScriptRoot
$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $serverRoot "../.."))
$allowedRoot = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot "artifacts-local"))
$resolvedOutput = if ([System.IO.Path]::IsPathFullyQualified($OutputRoot)) {
    [System.IO.Path]::GetFullPath($OutputRoot)
}
else {
    [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $OutputRoot))
}
$allowedPrefix = $allowedRoot.TrimEnd(
    [System.IO.Path]::DirectorySeparatorChar,
    [System.IO.Path]::AltDirectorySeparatorChar) +
    [System.IO.Path]::DirectorySeparatorChar

if (-not $resolvedOutput.StartsWith(
        $allowedPrefix,
        [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "The integration artefact path must remain under artifacts-local."
}

$archivePath = Join-Path $resolvedOutput "rag-challenge-state07-v2-integration.zip"

if (-not (Test-Path -LiteralPath $archivePath -PathType Leaf)) {
    throw "The integration artefact archive does not exist."
}

if (Get-NetTCPConnection -State Listen -LocalPort $Port -ErrorAction SilentlyContinue) {
    throw "The requested loopback port is already in use."
}

$runtimeRoot = Join-Path $resolvedOutput "runtime"
$extractRoot = Join-Path $runtimeRoot "app"
$storeRoot = Join-Path $runtimeRoot "store"
$backupRoot = Join-Path $runtimeRoot "backup"
$restoredStoreRoot = Join-Path $runtimeRoot "restored"
$baseUri = "http://127.0.0.1:$Port"
$process = $null

if (Test-Path -LiteralPath $runtimeRoot) {
    $verifiedRuntime = [System.IO.Path]::GetFullPath($runtimeRoot)

    if (-not $verifiedRuntime.StartsWith(
            $allowedPrefix,
            [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "The existing runtime path failed containment validation."
    }

    Remove-Item -LiteralPath $verifiedRuntime -Recurse -Force
}

New-Item -ItemType Directory -Path $extractRoot -Force | Out-Null
[System.IO.Compression.ZipFile]::ExtractToDirectory($archivePath, $extractRoot)

function Stop-TaskProcess {
    if ($null -ne $script:process -and -not $script:process.HasExited) {
        Stop-Process -Id $script:process.Id -Force

        if (-not $script:process.WaitForExit(10000)) {
            throw "The task-owned integration host did not stop within 10 seconds."
        }
    }

    $script:process = $null
}

function Start-TaskProcess {
    param([Parameter(Mandatory)][string]$TaskStoreRoot)

    $resolvedStore = [System.IO.Path]::GetFullPath($TaskStoreRoot)
    $runtimePrefix = [System.IO.Path]::GetFullPath($runtimeRoot).TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar) +
        [System.IO.Path]::DirectorySeparatorChar

    if (-not $resolvedStore.StartsWith(
            $runtimePrefix,
            [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "The integration store must remain inside the task-owned runtime root."
    }

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = (Get-Command dotnet).Source
    $startInfo.WorkingDirectory = $extractRoot
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.ArgumentList.Add((Join-Path $extractRoot "RagChallenge.Server.Api.dll"))
    $startInfo.Environment["DOTNET_ENVIRONMENT"] = "Integration"
    $startInfo.Environment["ASPNETCORE_URLS"] = $baseUri
    $startInfo.Environment["RagChallenge__Integration__Enabled"] = "true"
    $startInfo.Environment["RagChallenge__Integration__StoreRoot"] = $resolvedStore
    $startInfo.Environment["RagChallenge__Setup__AllowExternalServices"] = "false"
    $startInfo.Environment["Logging__LogLevel__Default"] = "Warning"
    $script:process = [System.Diagnostics.Process]::Start($startInfo)
    $readinessTimeoutSeconds = 30
    $deadline = [System.DateTimeOffset]::UtcNow.AddSeconds($readinessTimeoutSeconds)
    $lastReadinessError = "No readiness response was received."

    while ([System.DateTimeOffset]::UtcNow -lt $deadline) {
        if ($script:process.HasExited) {
            throw "The integration host exited before readiness."
        }

        try {
            $remainingReadinessSeconds = [Math]::Max(
                1,
                [int][Math]::Ceiling(
                    ($deadline - [System.DateTimeOffset]::UtcNow).TotalSeconds))
            $ready = Invoke-RestMethod `
                -Uri "$baseUri/api/v1/health/ready" `
                -TimeoutSec $remainingReadinessSeconds

            if ($ready.status -eq "Ready") {
                return $ready
            }

            $lastReadinessError = "Readiness returned status '$($ready.status)'."
        }
        catch {
            $lastReadinessError = $_.Exception.Message
            Start-Sleep -Milliseconds 100
        }
    }

    $failedProcess = $script:process
    Stop-TaskProcess
    $standardError = $failedProcess.StandardError.ReadToEnd()
    $standardOutput = $failedProcess.StandardOutput.ReadToEnd()
    $diagnostic = ($standardError + "`n" + $standardOutput).Trim()

    if ($diagnostic.Length -gt 2000) {
        $diagnostic = $diagnostic.Substring($diagnostic.Length - 2000)
    }

    throw "The integration host did not become ready within 30 seconds. Last request: $lastReadinessError Process output: $diagnostic"
}

function Invoke-SyntheticQuestion {
    param(
        [Parameter(Mandatory)][string]$QuestionLanguage,
        [ValidateSet("v1", "v2")][string]$Version = "v1"
    )

    $body = @{
        corpusId = "database-systems-catalogue-mvp"
        questionLanguage = $QuestionLanguage
        question = if ($QuestionLanguage -eq "pt-BR") {
            "Qual evidência de persistência está disponível?"
        }
        else {
            "What persistence evidence is available?"
        }
    } | ConvertTo-Json -Compress
    return Invoke-RestMethod `
        -Uri "$baseUri/api/$Version/questions" `
        -Method Post `
        -ContentType "application/json" `
        -Body $body `
        -TimeoutSec 10
}

function Get-VisualSelector {
    param([Parameter(Mandatory)][object]$QueryResponse)

    $citation = @($QueryResponse.citations)[0]
    $page = @($citation.pageImages)[0]

    if ($citation.documentFormat -ne "Pdf" -or
        $null -eq $page -or
        $page.imageContentObjectId -ne $page.contentSha256) {
        throw "The v2 query did not return one exact synthetic PDF page selector."
    }

    return [pscustomobject]@{
        IndexGenerationId = $QueryResponse.indexGenerationId
        RenderManifestId = $page.renderManifestId
        PageNumber = $page.pageNumber
        ImageContentObjectId = $page.imageContentObjectId
        Path = "/api/v2/evidence/page-images/$($QueryResponse.indexGenerationId)/$($page.renderManifestId)/$($page.pageNumber)/$($page.imageContentObjectId)"
    }
}

function Invoke-VerifiedVisualEvidence {
    param(
        [Parameter(Mandatory)][object]$Selector,
        [switch]$VerifyConditional
    )

    $client = [System.Net.Http.HttpClient]::new()

    try {
        $response = $client.GetAsync("$baseUri$($Selector.Path)").GetAwaiter().GetResult()

        try {
            $bytes = $response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
            $digest = [System.Convert]::ToHexString(
                [System.Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
            $cacheControl = $response.Headers.CacheControl
            $correlationPolicy = @($response.Headers.GetValues(
                "Cross-Origin-Resource-Policy"))

            if ($response.StatusCode -ne [System.Net.HttpStatusCode]::OK -or
                $response.Content.Headers.ContentType.MediaType -ne "image/png" -or
                $bytes.LongLength -ne $response.Content.Headers.ContentLength -or
                $bytes.LongLength -gt 64MB -or
                $digest -ne $Selector.ImageContentObjectId -or
                -not $cacheControl.Private -or
                -not $cacheControl.NoCache -or
                $correlationPolicy -notcontains "same-origin") {
                throw "The same-origin visual evidence response violated its frozen bounds."
            }

            $etag = $response.Headers.ETag.ToString()

            if ($etag -ne "`"sha256-$digest`"") {
                throw "The visual evidence ETag did not bind the exact PNG digest."
            }
        }
        finally {
            $response.Dispose()
        }

        if ($VerifyConditional) {
            $request = [System.Net.Http.HttpRequestMessage]::new(
                [System.Net.Http.HttpMethod]::Get,
                "$baseUri$($Selector.Path)")
            $request.Headers.TryAddWithoutValidation("If-None-Match", $etag) | Out-Null

            try {
                $conditional = $client.SendAsync($request).GetAwaiter().GetResult()

                try {
                    $conditionalBytes = $conditional.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()

                    if ($conditional.StatusCode -ne [System.Net.HttpStatusCode]::NotModified -or
                        $conditionalBytes.Length -ne 0 -or
                        $conditional.Headers.ETag.ToString() -ne $etag) {
                        throw "The conditional visual request was not fully revalidated as 304."
                    }
                }
                finally {
                    $conditional.Dispose()
                }
            }
            finally {
                $request.Dispose()
            }
        }

        return [pscustomobject]@{
            ContentSha256 = $digest
            ByteLength = $bytes.LongLength
            ETag = $etag
        }
    }
    finally {
        $client.Dispose()
    }
}

function Get-StoreFingerprint {
    param([Parameter(Mandatory)][string]$TaskStoreRoot)

    return @(Get-ChildItem -LiteralPath $TaskStoreRoot -Recurse -File |
        ForEach-Object {
            $relative = [System.IO.Path]::GetRelativePath($TaskStoreRoot, $_.FullName)
            $digest = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
            "$relative`:$($_.Length):$digest"
        } |
        Sort-Object)
}

function Copy-ColdStore {
    param(
        [Parameter(Mandatory)][string]$Source,
        [Parameter(Mandatory)][string]$Destination
    )

    if ($null -ne $script:process) {
        throw "A cold store copy requires the task-owned host to be stopped."
    }

    $runtimePrefix = [System.IO.Path]::GetFullPath($runtimeRoot).TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar) +
        [System.IO.Path]::DirectorySeparatorChar
    $resolvedSource = [System.IO.Path]::GetFullPath($Source)
    $resolvedDestination = [System.IO.Path]::GetFullPath($Destination)

    if (-not $resolvedSource.StartsWith(
            $runtimePrefix,
            [System.StringComparison]::OrdinalIgnoreCase) -or
        -not $resolvedDestination.StartsWith(
            $runtimePrefix,
            [System.StringComparison]::OrdinalIgnoreCase) -or
        -not (Test-Path -LiteralPath $resolvedSource -PathType Container) -or
        (Test-Path -LiteralPath $resolvedDestination)) {
        throw "A cold store copy must use distinct task-owned paths."
    }

    if ((Get-Item -LiteralPath $resolvedSource -Force).Attributes -band
        [System.IO.FileAttributes]::ReparsePoint) {
        throw "A cold store copy cannot use a reparse-point source root."
    }

    $entries = @(Get-ChildItem -LiteralPath $resolvedSource -Recurse -Force)

    if ($entries | Where-Object {
            ($_.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0
        }) {
        throw "A cold store copy cannot traverse a reparse point."
    }

    New-Item -ItemType Directory -Path $resolvedDestination | Out-Null

    foreach ($directory in @($entries | Where-Object PSIsContainer)) {
        $relative = [System.IO.Path]::GetRelativePath(
            $resolvedSource,
            $directory.FullName)
        New-Item -ItemType Directory -Path (
            Join-Path $resolvedDestination $relative) -Force | Out-Null
    }

    foreach ($file in @($entries | Where-Object { -not $_.PSIsContainer })) {
        $relative = [System.IO.Path]::GetRelativePath($resolvedSource, $file.FullName)
        [System.IO.File]::Copy(
            $file.FullName,
            (Join-Path $resolvedDestination $relative),
            $false)
    }
}

function Assert-SameFingerprint {
    param(
        [Parameter(Mandatory)][string[]]$Expected,
        [Parameter(Mandatory)][string[]]$Actual
    )

    if (Compare-Object -ReferenceObject $Expected -DifferenceObject $Actual) {
        throw "The cold copy did not preserve the exact store file set and bytes."
    }
}

try {
    $firstReadiness = Start-TaskProcess -TaskStoreRoot $storeRoot
    $html = (Invoke-WebRequest -Uri "$baseUri/" -TimeoutSec 10).Content

    if ($html -notmatch '<div id="root"></div>') {
        throw "The artefact did not serve the Dashboard shell from the API origin."
    }

    $firstEnglish = Invoke-SyntheticQuestion -QuestionLanguage "en-GB"
    $firstPortuguese = Invoke-SyntheticQuestion -QuestionLanguage "pt-BR"
    $firstV2 = Invoke-SyntheticQuestion -QuestionLanguage "en-GB" -Version "v2"
    $firstSelector = Get-VisualSelector -QueryResponse $firstV2
    $firstVisual = Invoke-VerifiedVisualEvidence `
        -Selector $firstSelector `
        -VerifyConditional

    if ($firstEnglish.outcome -ne "Answered" -or
        $firstEnglish.answerLanguage -ne "en-GB" -or
        $firstEnglish.citations.Count -ne 1 -or
        $firstPortuguese.outcome -ne "Answered" -or
        $firstPortuguese.answerLanguage -ne "pt-BR" -or
        $firstPortuguese.citations.Count -ne 1 -or
        $firstV2.outcome -ne "Answered" -or
        $firstV2.answerLanguage -ne "en-GB" -or
        $firstSelector.IndexGenerationId -ne $firstEnglish.indexGenerationId) {
        throw "The first same-origin synthetic query flow did not satisfy the v1/v2 contracts."
    }

    $firstGeneration = $firstEnglish.indexGenerationId
    Stop-TaskProcess

    $controlPath = Join-Path $storeRoot "control.db"
    $vectorPath = Join-Path $storeRoot "vectors.db"
    $contentFiles = @(Get-ChildItem -LiteralPath (Join-Path $storeRoot "content") -Recurse -File)

    if (-not (Test-Path -LiteralPath $controlPath -PathType Leaf) -or
        -not (Test-Path -LiteralPath $vectorPath -PathType Leaf) -or
        $contentFiles.Count -lt 1) {
        throw "The first run did not persist control, vector, and raw content state."
    }

    $storeFingerprint = @(Get-StoreFingerprint -TaskStoreRoot $storeRoot)
    Copy-ColdStore -Source $storeRoot -Destination $backupRoot
    Assert-SameFingerprint `
        -Expected $storeFingerprint `
        -Actual @(Get-StoreFingerprint -TaskStoreRoot $backupRoot)

    $secondReadiness = Start-TaskProcess -TaskStoreRoot $storeRoot
    $secondEnglish = Invoke-SyntheticQuestion -QuestionLanguage "en-GB"
    $secondV2 = Invoke-SyntheticQuestion -QuestionLanguage "en-GB" -Version "v2"
    $secondSelector = Get-VisualSelector -QueryResponse $secondV2
    $secondVisual = Invoke-VerifiedVisualEvidence -Selector $secondSelector

    if ($secondEnglish.outcome -ne "Answered" -or
        $secondEnglish.indexGenerationId -ne $firstGeneration -or
        $secondReadiness.activeGenerationId -ne $firstGeneration -or
        $secondSelector.IndexGenerationId -ne $firstSelector.IndexGenerationId -or
        $secondSelector.RenderManifestId -ne $firstSelector.RenderManifestId -or
        $secondSelector.ImageContentObjectId -ne $firstSelector.ImageContentObjectId -or
        $secondVisual.ContentSha256 -ne $firstVisual.ContentSha256) {
        throw "The restarted host did not reopen the persisted v1/v2 activation and visual evidence."
    }

    Stop-TaskProcess
    Copy-ColdStore -Source $backupRoot -Destination $restoredStoreRoot
    Assert-SameFingerprint `
        -Expected $storeFingerprint `
        -Actual @(Get-StoreFingerprint -TaskStoreRoot $restoredStoreRoot)
    $restoredReadiness = Start-TaskProcess -TaskStoreRoot $restoredStoreRoot
    $restoredV1 = Invoke-SyntheticQuestion -QuestionLanguage "en-GB"
    $restoredV2 = Invoke-SyntheticQuestion -QuestionLanguage "en-GB" -Version "v2"
    $restoredSelector = Get-VisualSelector -QueryResponse $restoredV2

    if ($restoredV1.indexGenerationId -ne $firstGeneration -or
        $restoredReadiness.activeGenerationId -ne $firstGeneration -or
        $restoredSelector.IndexGenerationId -ne $firstSelector.IndexGenerationId -or
        $restoredSelector.RenderManifestId -ne $firstSelector.RenderManifestId -or
        $restoredSelector.ImageContentObjectId -ne $firstSelector.ImageContentObjectId) {
        throw "The cold-restored host did not reopen the exact persisted v1/v2 evidence."
    }

    $client = [System.Net.Http.HttpClient]::new()

    try {
        for ($requestIndex = 1; $requestIndex -le 10; $requestIndex++) {
            $accepted = $client.GetAsync("$baseUri$($restoredSelector.Path)").GetAwaiter().GetResult()

            try {
                if ($accepted.StatusCode -ne [System.Net.HttpStatusCode]::OK) {
                    throw "Visual request $requestIndex did not fit the initial token bucket."
                }
            }
            finally {
                $accepted.Dispose()
            }
        }

        $rejected = $client.GetAsync("$baseUri$($restoredSelector.Path)").GetAwaiter().GetResult()

        try {
            $rejectionBody = $rejected.Content.ReadAsStringAsync().GetAwaiter().GetResult() |
                ConvertFrom-Json
            $retryAfter = @($rejected.Headers.GetValues("Retry-After"))

            if ($rejected.StatusCode -ne [System.Net.HttpStatusCode]::TooManyRequests -or
                $rejectionBody.code -ne "CH_VISUAL_EVIDENCE_RATE_LIMITED" -or
                $retryAfter -notcontains "10") {
                throw "The visual token bucket did not reject the eleventh immediate request."
            }
        }
        finally {
            $rejected.Dispose()
        }
    }
    finally {
        $client.Dispose()
    }

    [pscustomobject]@{
        Status = "Passed"
        Origin = $baseUri
        DashboardServed = $true
        AnswerLanguages = @("en-GB", "pt-BR")
        IndexGenerationId = $firstGeneration
        RestartPreservedGeneration = $true
        ColdRestorePreservedGeneration = $true
        VisualEvidenceSha256 = $firstVisual.ContentSha256
        VisualConditionalRevalidation = $true
        VisualMaximumByteLength = 64MB
        VisualTokenBucket = "10 accepted; eleventh rejected"
        ControlStore = "control.db"
        VectorStore = "vectors.db"
        RawContentObjects = $contentFiles.Count
        FirstReadiness = $firstReadiness.status
        SecondReadiness = $secondReadiness.status
        RestoredReadiness = $restoredReadiness.status
    } | ConvertTo-Json -Compress
}
finally {
    Stop-TaskProcess

    if (Test-Path -LiteralPath $runtimeRoot) {
        Remove-Item -LiteralPath $runtimeRoot -Recurse -Force
    }
}
