# Purpose: Proves that Oracle plan generation is deterministic, task-scoped and non-overwriting without touching owner data.
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$generator = Join-Path $PSScriptRoot 'New-Oracle19ProductPlans.ps1'
$approvedRoot = [System.IO.Path]::GetFullPath((
        Join-Path $repositoryRoot 'artifacts-local/oracle19-product-plans'))
$runPrefix = 'policy-{0}' -f [guid]::NewGuid().ToString('N')
$ownedTaskIds = [System.Collections.Generic.HashSet[string]]::new(
    [System.StringComparer]::Ordinal)
$utf8WithoutBom = [System.Text.UTF8Encoding]::new($false)

function Register-TestTask([string] $Suffix) {
    $taskId = '{0}-{1}' -f $runPrefix, $Suffix
    if (-not $ownedTaskIds.Add($taskId)) {
        throw 'A disposable Oracle plan task ID was registered twice.'
    }

    return $taskId
}

function Get-TestOutput([string] $TaskId) {
    $path = [System.IO.Path]::GetFullPath((Join-Path $approvedRoot $TaskId))
    if ([System.IO.Path]::GetFullPath((Split-Path -Parent $path)) -cne $approvedRoot -or
        (Split-Path -Leaf $path) -cne $TaskId -or
        -not $ownedTaskIds.Contains($TaskId)) {
        throw 'A disposable Oracle plan path escaped the approved test root.'
    }

    return $path
}

function Invoke-ExpectedFailure([string] $Name, [scriptblock] $Action) {
    $failed = $false
    try {
        & $Action
    }
    catch {
        $failed = $true
    }

    if (-not $failed) {
        throw "Case '$Name' unexpectedly succeeded."
    }

    Write-Output "PASS: $Name"
}

function Get-PlanFingerprints([string] $Output) {
    return @(
        Get-ChildItem -LiteralPath $Output -File -Filter 'catalogue-*.json' |
            Sort-Object Name |
            ForEach-Object {
                '{0}:{1}' -f $_.Name, (
                    Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
            })
}

function Assert-Utf8LfFile([string] $Path) {
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    if ($bytes.Length -eq 0 -or
        $bytes[$bytes.Length - 1] -ne 0x0A -or
        ($bytes.Length -ge 3 -and
            $bytes[0] -eq 0xEF -and
            $bytes[1] -eq 0xBB -and
            $bytes[2] -eq 0xBF) -or
        $bytes.Contains([byte]0x0D)) {
        throw "'$Path' is not UTF-8 without BOM using LF and a final newline."
    }
}

function Remove-DisposableTask([string] $TaskId) {
    $path = Get-TestOutput -TaskId $TaskId
    $item = Get-Item -LiteralPath $path -Force -ErrorAction SilentlyContinue
    if ($null -eq $item) {
        return
    }

    if (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
        Remove-Item -LiteralPath $path -Force
    }
    else {
        if (-not $item.PSIsContainer) {
            throw 'A disposable Oracle plan output unexpectedly became a file.'
        }

        Remove-Item -LiteralPath $path -Recurse -Force
    }

    if (Test-Path -LiteralPath $path) {
        throw "Disposable Oracle plan output '$TaskId' could not be removed."
    }
}

$firstTaskId = Register-TestTask -Suffix 'first'
$secondTaskId = Register-TestTask -Suffix 'second'
$foreignTaskId = Register-TestTask -Suffix 'foreign'
$linkTaskId = Register-TestTask -Suffix 'link'
$linkTargetTaskId = Register-TestTask -Suffix 'link-target'

try {
    & $generator -TaskId $firstTaskId | Out-Null
    $firstOutput = Get-TestOutput -TaskId $firstTaskId
    $firstFiles = @(Get-ChildItem -LiteralPath $firstOutput -File)
    $firstPlans = @($firstFiles | Where-Object { $_.Name -like 'catalogue-*.json' })
    $markerPath = Join-Path $firstOutput '.rag-challenge-owned-output.json'

    if ($firstPlans.Count -ne 52 -or
        $firstFiles.Count -ne 53 -or
        -not (Test-Path -LiteralPath $markerPath -PathType Leaf)) {
        throw 'Oracle plan generation did not create exactly one marker and 52 plans.'
    }

    $marker = Get-Content -LiteralPath $markerPath -Raw | ConvertFrom-Json
    if ($marker.schemaVersion -ne 1 -or
        $marker.taskId -cne $firstTaskId -or
        $marker.purpose -cne 'oracle19-product-plans') {
        throw 'The Oracle plan ownership marker is invalid.'
    }

    foreach ($file in $firstFiles) {
        Assert-Utf8LfFile -Path $file.FullName
    }
    Write-Output 'PASS: creates exactly one marker and 52 UTF-8/LF plans'

    $beforeReplay = @(Get-PlanFingerprints -Output $firstOutput)
    Invoke-ExpectedFailure -Name 'refuses replay without overwriting' -Action {
        & $generator -TaskId $firstTaskId | Out-Null
    }
    $afterReplay = @(Get-PlanFingerprints -Output $firstOutput)
    if (Compare-Object -ReferenceObject $beforeReplay -DifferenceObject $afterReplay) {
        throw 'A refused Oracle plan replay changed existing plan bytes.'
    }

    & $generator -TaskId $secondTaskId | Out-Null
    $secondOutput = Get-TestOutput -TaskId $secondTaskId
    $secondFingerprints = @(Get-PlanFingerprints -Output $secondOutput)
    if (Compare-Object -ReferenceObject $beforeReplay -DifferenceObject $secondFingerprints) {
        throw 'Oracle plan payloads differ across distinct task namespaces.'
    }
    Write-Output 'PASS: produces deterministic payloads across task namespaces'

    $foreignOutput = Get-TestOutput -TaskId $foreignTaskId
    [System.IO.Directory]::CreateDirectory($foreignOutput) | Out-Null
    $sentinelPath = Join-Path $foreignOutput 'owner-sentinel.txt'
    $sentinelBytes = $utf8WithoutBom.GetBytes("owner sentinel`n")
    [System.IO.File]::WriteAllBytes($sentinelPath, $sentinelBytes)
    $sentinelHash = (Get-FileHash -LiteralPath $sentinelPath -Algorithm SHA256).Hash
    Invoke-ExpectedFailure -Name 'refuses an existing foreign directory' -Action {
        & $generator -TaskId $foreignTaskId | Out-Null
    }
    if ((Get-FileHash -LiteralPath $sentinelPath -Algorithm SHA256).Hash -cne $sentinelHash -or
        @(Get-ChildItem -LiteralPath $foreignOutput -Force).Count -ne 1) {
        throw 'A refused foreign output changed its sentinel.'
    }

    Invoke-ExpectedFailure -Name 'rejects invalid task traversal before mutation' -Action {
        & $generator -TaskId '../escape' | Out-Null
    }

    $linkTarget = Get-TestOutput -TaskId $linkTargetTaskId
    [System.IO.Directory]::CreateDirectory($linkTarget) | Out-Null
    $linkTargetSentinel = Join-Path $linkTarget 'target-sentinel.txt'
    [System.IO.File]::WriteAllBytes($linkTargetSentinel, $sentinelBytes)
    $linkPath = Get-TestOutput -TaskId $linkTaskId
    try {
        $null = New-Item -ItemType SymbolicLink -Path $linkPath -Target $linkTarget -ErrorAction Stop
        Invoke-ExpectedFailure -Name 'rejects a reparse-point output' -Action {
            & $generator -TaskId $linkTaskId | Out-Null
        }
        if (-not (Test-Path -LiteralPath $linkTargetSentinel -PathType Leaf)) {
            throw 'The refused reparse-point output changed its target.'
        }
    }
    catch [System.UnauthorizedAccessException] {
        Write-Output 'SKIP: reparse-point output creation is unavailable on this host'
    }
    catch [System.ComponentModel.Win32Exception] {
        Write-Output 'SKIP: reparse-point output creation is unavailable on this host'
    }

    Write-Output 'All Oracle plan generator policy tests passed.'
}
finally {
    foreach ($taskId in @($ownedTaskIds)) {
        Remove-DisposableTask -TaskId $taskId
    }
}
