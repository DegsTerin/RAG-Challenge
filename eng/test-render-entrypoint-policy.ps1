# Purpose: Exercises Render entrypoint containment with a transformed task-scoped fixture and no host mounts or external services.
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$approvedRoot = [System.IO.Path]::GetFullPath((
        Join-Path $repositoryRoot 'artifacts-local/entrypoint-policy-tests'))
$testRoot = [System.IO.Path]::GetFullPath((
        Join-Path $approvedRoot ('run-{0}' -f [guid]::NewGuid().ToString('N'))))
$approvedPrefix = $approvedRoot.TrimEnd(
    [System.IO.Path]::DirectorySeparatorChar,
    [System.IO.Path]::AltDirectorySeparatorChar) +
    [System.IO.Path]::DirectorySeparatorChar
$utf8WithoutBom = [System.Text.UTF8Encoding]::new($false)

if (-not $testRoot.StartsWith(
        $approvedPrefix,
        [System.StringComparison]::OrdinalIgnoreCase) -or
    -not (Split-Path -Leaf $testRoot).StartsWith(
        'run-',
        [System.StringComparison]::Ordinal)) {
    throw 'The Render entrypoint test root escaped its approved namespace.'
}

function Convert-ToShellPath([string] $Path) {
    $fullPath = [System.IO.Path]::GetFullPath($Path)
    if ($IsWindows) {
        if ($fullPath -notmatch '^(?<drive>[A-Za-z]):\\(?<tail>.*)$') {
            throw 'The Windows test path cannot be represented for Git sh.'
        }

        return ('/{0}/{1}' -f
            $Matches.drive.ToLowerInvariant(),
            $Matches.tail.Replace('\', '/'))
    }

    return $fullPath
}

function Resolve-Shell {
    if (-not $IsWindows) {
        return (Get-Command sh -CommandType Application -ErrorAction Stop |
                Select-Object -First 1).Source
    }

    foreach ($candidate in @(
            'C:\Program Files\Git\bin\sh.exe',
            'C:\Program Files\Git\usr\bin\sh.exe')) {
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            return $candidate
        }
    }

    return $null
}

function Write-Utf8Lf([string] $Path, [string] $Content) {
    [System.IO.File]::WriteAllBytes(
        $Path,
        $utf8WithoutBom.GetBytes($Content.Replace("`r`n", "`n")))
}

function Write-SeedManifest([string] $SeedRoot) {
    $records = foreach ($relativePath in @(
            'control.db',
            'vectors.db',
            'content/chunk.bin')) {
        $fullPath = Join-Path $SeedRoot $relativePath
        $digest = (Get-FileHash -LiteralPath $fullPath -Algorithm SHA256).
            Hash.ToLowerInvariant()
        '{0}  {1}' -f $digest, $relativePath
    }

    Write-Utf8Lf `
        -Path (Join-Path $SeedRoot 'seed-manifest.sha256') `
        -Content (($records -join "`n") + "`n")
}

function Invoke-Entrypoint([hashtable] $Environment = @{}) {
    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $shellPath
    $startInfo.WorkingDirectory = $testRoot
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.Environment.Clear()
    $startInfo.Environment['PATH'] = '/usr/bin:/bin'
    $startInfo.Environment['PORT'] = '10000'
    $startInfo.Environment['ENTRYPOINT_TEST_RESULT'] = $resultShellPath
    if ($IsWindows) {
        $startInfo.Environment['SystemRoot'] = [Environment]::GetFolderPath(
            [Environment+SpecialFolder]::Windows)
    }
    foreach ($entry in $Environment.GetEnumerator()) {
        $startInfo.Environment[[string]$entry.Key] = [string]$entry.Value
    }
    $startInfo.ArgumentList.Add($scriptShellPath)

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    try {
        if (-not $process.Start()) {
            throw 'The Render entrypoint fixture did not start.'
        }
        $standardOutput = $process.StandardOutput.ReadToEnd()
        $standardError = $process.StandardError.ReadToEnd()
        if (-not $process.WaitForExit(15000)) {
            $process.Kill($true)
            throw 'The Render entrypoint fixture exceeded its time limit.'
        }
        if ($standardOutput.Length -gt 65536 -or $standardError.Length -gt 65536) {
            throw 'The Render entrypoint fixture output exceeded its limit.'
        }

        return [pscustomobject]@{
            ExitCode = $process.ExitCode
            StandardError = $standardError
        }
    }
    finally {
        $process.Dispose()
    }
}

function Reset-RuntimeStore {
    if (-not (Test-Path -LiteralPath $runtimeStore)) {
        return
    }

    $resolvedRuntime = [System.IO.Path]::GetFullPath($runtimeStore)
    $expectedParent = [System.IO.Path]::GetFullPath((Split-Path -Parent $resolvedRuntime))
    if ($expectedParent -cne [System.IO.Path]::GetFullPath($testRoot) -or
        (Split-Path -Leaf $resolvedRuntime) -cne 'runtime-store') {
        throw 'The disposable runtime store failed containment validation.'
    }

    $item = Get-Item -LiteralPath $resolvedRuntime -Force
    if (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
        Remove-Item -LiteralPath $resolvedRuntime -Force
    }
    else {
        Remove-Item -LiteralPath $resolvedRuntime -Recurse -Force
    }
}

$shellPath = Resolve-Shell
if ($null -eq $shellPath) {
    Write-Output 'SKIP: a POSIX shell is unavailable for Render entrypoint policy tests'
    exit 0
}

$seedRoot = Join-Path $testRoot 'seed'
$runtimeStore = Join-Path $testRoot 'runtime-store'
$scriptPath = Join-Path $testRoot 'entrypoint.sh'
$resultPath = Join-Path $testRoot 'result.txt'
$seedShellPath = Convert-ToShellPath -Path $seedRoot
$runtimeShellPath = Convert-ToShellPath -Path $runtimeStore
$runtimeParentShellPath = Convert-ToShellPath -Path $testRoot
$scriptShellPath = Convert-ToShellPath -Path $scriptPath
$resultShellPath = Convert-ToShellPath -Path $resultPath

try {
    [System.IO.Directory]::CreateDirectory((Join-Path $seedRoot 'content')) | Out-Null
    Write-Utf8Lf -Path (Join-Path $testRoot '.rag-challenge-test-root-v1') -Content "entrypoint-policy-v1`n"
    Write-Utf8Lf -Path (Join-Path $seedRoot 'control.db') -Content "control`n"
    Write-Utf8Lf -Path (Join-Path $seedRoot 'vectors.db') -Content "vectors`n"
    Write-Utf8Lf -Path (Join-Path $seedRoot 'content/chunk.bin') -Content "chunk`n"
    Write-SeedManifest -SeedRoot $seedRoot

    $source = Get-Content -LiteralPath (
        Join-Path $repositoryRoot 'deploy/render-free/entrypoint.sh') -Raw
    $fixture = $source.
        Replace('/opt/rag-challenge/seed', $seedShellPath).
        Replace('runtime_parent="/tmp"', ('runtime_parent="{0}"' -f $runtimeParentShellPath)).
        Replace('/tmp/rag-challenge-store', $runtimeShellPath).
        Replace(
            'exec dotnet /app/RagChallenge.Server.Api.dll',
            'printf ''%s\n'' "${RagChallenge__Product__StoreRoot:-}" > "${ENTRYPOINT_TEST_RESULT}"')
    Write-Utf8Lf -Path $scriptPath -Content $fixture

    $syntax = [System.Diagnostics.Process]::Start($shellPath, "-n `"$scriptPath`"")
    $syntax.WaitForExit()
    if ($syntax.ExitCode -ne 0) {
        throw 'The Render entrypoint failed POSIX shell syntax validation.'
    }
    $syntax.Dispose()
    Write-Output 'PASS: POSIX shell syntax'

    $unsafeOverride = Invoke-Entrypoint -Environment @{
        RAG_CHALLENGE_RUNTIME_STORE = '/app'
    }
    if ($unsafeOverride.ExitCode -ne 23 -or
        $unsafeOverride.StandardError -notmatch 'CH_DEPLOY_RUNTIME_STORE_UNSAFE' -or
        (Test-Path -LiteralPath $runtimeStore)) {
        throw 'A non-canonical runtime override did not fail before mutation.'
    }
    Write-Output 'PASS: rejects a non-canonical runtime override before mutation'

    [System.IO.Directory]::CreateDirectory($runtimeStore) | Out-Null
    $runtimeSentinel = Join-Path $runtimeStore 'owner-sentinel.txt'
    Write-Utf8Lf -Path $runtimeSentinel -Content "owner`n"
    $unmarked = Invoke-Entrypoint
    if ($unmarked.ExitCode -ne 24 -or
        -not (Test-Path -LiteralPath $runtimeSentinel -PathType Leaf)) {
        throw 'An unmarked runtime store was not preserved.'
    }
    Write-Output 'PASS: preserves an unmarked runtime store'

    Write-Utf8Lf `
        -Path (Join-Path $runtimeStore '.rag-challenge-runtime-store-v1') `
        -Content "corrupt`n"
    $corruptMarker = Invoke-Entrypoint
    if ($corruptMarker.ExitCode -ne 24 -or
        -not (Test-Path -LiteralPath $runtimeSentinel -PathType Leaf)) {
        throw 'A runtime store with a corrupt marker was not preserved.'
    }
    Write-Output 'PASS: preserves a runtime store with a corrupt marker'

    Reset-RuntimeStore
    $firstStart = Invoke-Entrypoint
    if ($firstStart.ExitCode -ne 0 -or
        (Get-Content -LiteralPath $resultPath -Raw).Trim() -cne $runtimeShellPath -or
        (Get-Content -LiteralPath (
            Join-Path $runtimeStore '.rag-challenge-runtime-store-v1') -Raw).Trim() -cne
            'rag-challenge-runtime-store-v1') {
        throw 'The valid first start did not create the owned runtime store.'
    }
    Write-Output 'PASS: creates and marks only the canonical runtime store'

    $outsideSentinel = Join-Path $testRoot 'outside-sentinel.txt'
    $stalePath = Join-Path $runtimeStore 'stale.txt'
    Write-Utf8Lf -Path $outsideSentinel -Content "outside`n"
    Write-Utf8Lf -Path $stalePath -Content "stale`n"
    $restart = Invoke-Entrypoint
    if ($restart.ExitCode -ne 0 -or
        (Test-Path -LiteralPath $stalePath) -or
        -not (Test-Path -LiteralPath $outsideSentinel -PathType Leaf)) {
        throw 'A valid restart changed content outside its owned runtime store.'
    }
    Write-Output 'PASS: replaces only the correctly marked runtime store'

    Write-Utf8Lf -Path (Join-Path $runtimeStore 'preserve-on-seed-failure.txt') -Content "preserve`n"
    Write-Utf8Lf -Path (Join-Path $seedRoot 'control.db') -Content "tampered`n"
    $badSeed = Invoke-Entrypoint
    if ($badSeed.ExitCode -ne 21 -or
        -not (Test-Path -LiteralPath (
            Join-Path $runtimeStore 'preserve-on-seed-failure.txt') -PathType Leaf)) {
        throw 'Seed integrity failure changed the existing runtime store.'
    }
    Write-Output 'PASS: validates seed integrity before runtime-store removal'

    Write-Output 'All Render entrypoint policy tests passed.'
}
finally {
    if (Test-Path -LiteralPath $testRoot) {
        $resolvedTestRoot = [System.IO.Path]::GetFullPath($testRoot)
        $rootItem = Get-Item -LiteralPath $resolvedTestRoot -Force
        $markerPath = Join-Path $resolvedTestRoot '.rag-challenge-test-root-v1'
        if (-not $resolvedTestRoot.StartsWith(
                $approvedPrefix,
                [System.StringComparison]::OrdinalIgnoreCase) -or
            ($rootItem.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0 -or
            -not (Test-Path -LiteralPath $markerPath -PathType Leaf) -or
            (Get-Content -LiteralPath $markerPath -Raw).Trim() -cne 'entrypoint-policy-v1') {
            throw 'The Render entrypoint test root is not safe to remove.'
        }

        Remove-Item -LiteralPath $resolvedTestRoot -Recurse -Force
    }
}
