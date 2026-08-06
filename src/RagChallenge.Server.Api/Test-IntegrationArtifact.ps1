# Purpose: Reproduces the local STATE-06 artefact over loopback, verifies the same-origin Dashboard/API flow twice across process restart, and removes only task-owned runtime data.
[CmdletBinding()]
param(
    [string]$OutputRoot = "artifacts-local/s06-a",
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

$archivePath = Join-Path $resolvedOutput "rag-challenge-s06-a.zip"

if (-not (Test-Path -LiteralPath $archivePath -PathType Leaf)) {
    throw "The integration artefact archive does not exist."
}

if (Get-NetTCPConnection -State Listen -LocalPort $Port -ErrorAction SilentlyContinue) {
    throw "The requested loopback port is already in use."
}

$runtimeRoot = Join-Path $resolvedOutput "runtime"
$extractRoot = Join-Path $runtimeRoot "app"
$storeRoot = Join-Path $runtimeRoot "store"
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
        $script:process.WaitForExit(10000) | Out-Null
    }

    $script:process = $null
}

function Start-TaskProcess {
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
    $startInfo.Environment["RagChallenge__Integration__StoreRoot"] = $storeRoot
    $startInfo.Environment["RagChallenge__Setup__AllowExternalServices"] = "false"
    $script:process = [System.Diagnostics.Process]::Start($startInfo)
    $deadline = [System.DateTimeOffset]::UtcNow.AddSeconds(30)
    $lastReadinessError = "No readiness response was received."

    while ([System.DateTimeOffset]::UtcNow -lt $deadline) {
        if ($script:process.HasExited) {
            throw "The integration host exited before readiness."
        }

        try {
            $ready = Invoke-RestMethod -Uri "$baseUri/api/v1/health/ready" -TimeoutSec 2

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
    param([Parameter(Mandatory)][string]$QuestionLanguage)

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
        -Uri "$baseUri/api/v1/questions" `
        -Method Post `
        -ContentType "application/json" `
        -Body $body `
        -TimeoutSec 10
}

try {
    $firstReadiness = Start-TaskProcess
    $html = (Invoke-WebRequest -Uri "$baseUri/" -TimeoutSec 10).Content

    if ($html -notmatch '<div id="root"></div>') {
        throw "The artefact did not serve the Dashboard shell from the API origin."
    }

    $firstEnglish = Invoke-SyntheticQuestion -QuestionLanguage "en-GB"
    $firstPortuguese = Invoke-SyntheticQuestion -QuestionLanguage "pt-BR"

    if ($firstEnglish.outcome -ne "Answered" -or
        $firstEnglish.answerLanguage -ne "en-GB" -or
        $firstEnglish.citations.Count -ne 1 -or
        $firstPortuguese.outcome -ne "Answered" -or
        $firstPortuguese.answerLanguage -ne "pt-BR" -or
        $firstPortuguese.citations.Count -ne 1) {
        throw "The first same-origin synthetic query flow did not satisfy the v1 contract."
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

    $secondReadiness = Start-TaskProcess
    $secondEnglish = Invoke-SyntheticQuestion -QuestionLanguage "en-GB"

    if ($secondEnglish.outcome -ne "Answered" -or
        $secondEnglish.indexGenerationId -ne $firstGeneration -or
        $secondReadiness.activeGenerationId -ne $firstGeneration) {
        throw "The restarted host did not reopen the persisted activation and index."
    }

    [pscustomobject]@{
        Status = "Passed"
        Origin = $baseUri
        DashboardServed = $true
        AnswerLanguages = @("en-GB", "pt-BR")
        IndexGenerationId = $firstGeneration
        RestartPreservedGeneration = $true
        ControlStore = "control.db"
        VectorStore = "vectors.db"
        RawContentObjects = $contentFiles.Count
        FirstReadiness = $firstReadiness.status
        SecondReadiness = $secondReadiness.status
    } | ConvertTo-Json -Compress
}
finally {
    Stop-TaskProcess

    if (Test-Path -LiteralPath $runtimeRoot) {
        Remove-Item -LiteralPath $runtimeRoot -Recurse -Force
    }
}
