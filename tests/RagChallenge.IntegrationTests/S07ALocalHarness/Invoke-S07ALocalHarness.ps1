# Purpose: Runs either the non-campaign harness checks or the single explicitly authorised A3 entry point without restore, network, or non-task-owned stores.
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet("Validate", "Run")]
    [string]$Mode,

    [string]$AuthorityId
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = [System.IO.Path]::GetFullPath(
    (Join-Path $PSScriptRoot "..\..\.."))
$projectPath = Join-Path $repositoryRoot (
    "tests\RagChallenge.IntegrationTests\RagChallenge.IntegrationTests.csproj")
$authorityVariable = "RAGCHALLENGE_S07_A_RUN_AUTHORITY"
$expectedAuthority = "AUTH-S07-A-RUN-001"
$previousAuthority = [System.Environment]::GetEnvironmentVariable(
    $authorityVariable,
    [System.EnvironmentVariableTarget]::Process)
$testHostExitTimeout = [TimeSpan]::FromSeconds(90)

function Update-OwnedProcessTree {
    param(
        [Parameter(Mandatory)]
        [System.Collections.Generic.HashSet[int]]$OwnedProcessIds,

        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.HashSet[int]]$OwnedTestHostIds
    )

    $snapshot = @(Get-CimInstance Win32_Process -ErrorAction Stop)
    $changed = $true

    while ($changed) {
        $changed = $false

        foreach ($candidate in $snapshot) {
            $processId = [int]$candidate.ProcessId
            $parentProcessId = [int]$candidate.ParentProcessId

            if (-not $OwnedProcessIds.Contains($parentProcessId) -or
                $OwnedProcessIds.Contains($processId)) {
                continue
            }

            [void]$OwnedProcessIds.Add($processId)
            $changed = $true

            if ([string]::Equals(
                    $candidate.Name,
                    "testhost.exe",
                    [StringComparison]::OrdinalIgnoreCase)) {
                [void]$OwnedTestHostIds.Add($processId)
            }
        }
    }
}

function Invoke-ValidationTest {
    param(
        [Parameter(Mandatory)]
        [string]$ProjectPath,

        [Parameter(Mandatory)]
        [string]$TestFilter,

        [Parameter(Mandatory)]
        [string]$WorkingDirectory,

        [Parameter(Mandatory)]
        [TimeSpan]$TestHostTimeout
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = "dotnet"
    $startInfo.WorkingDirectory = $WorkingDirectory
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true

    foreach ($argument in @(
            "test",
            $ProjectPath,
            "--configuration",
            "Release",
            "--no-restore",
            "--filter",
            $TestFilter,
            "--logger",
            "console;verbosity=normal")) {
        [void]$startInfo.ArgumentList.Add($argument)
    }

    $process = [System.Diagnostics.Process]::Start($startInfo)

    if ($null -eq $process) {
        throw "The S07-A validation process could not be started."
    }

    $standardOutputTask = $process.StandardOutput.ReadToEndAsync()
    $standardErrorTask = $process.StandardError.ReadToEndAsync()
    $ownedProcessIds = [System.Collections.Generic.HashSet[int]]::new()
    $ownedTestHostIds = [System.Collections.Generic.HashSet[int]]::new()
    $processTreeParameters = @{
        OwnedProcessIds = $ownedProcessIds
        OwnedTestHostIds = $ownedTestHostIds
    }
    [void]$ownedProcessIds.Add($process.Id)

    try {
        do {
            Update-OwnedProcessTree @processTreeParameters
        }
        while (-not $process.WaitForExit(100))

        Update-OwnedProcessTree @processTreeParameters
        $exitCode = $process.ExitCode
        $standardOutput = $standardOutputTask.GetAwaiter().GetResult()
        $standardError = $standardErrorTask.GetAwaiter().GetResult()
        [Console]::Out.Write($standardOutput)
        [Console]::Error.Write($standardError)
    }
    finally {
        $process.Dispose()
    }

    $deadline = [DateTime]::UtcNow.Add($TestHostTimeout)

    while ($true) {
        $runningTestHostIds = @(
            Get-CimInstance Win32_Process -ErrorAction Stop |
                Where-Object {
                    $ownedTestHostIds.Contains([int]$_.ProcessId)
                } |
                ForEach-Object { [int]$_.ProcessId })

        if ($runningTestHostIds.Count -eq 0) {
            return $exitCode
        }

        if ([DateTime]::UtcNow -ge $deadline) {
            throw (
                "S07-A validation-owned testhost processes did not exit within " +
                "$([int]$TestHostTimeout.TotalSeconds) seconds: " +
                ($runningTestHostIds -join ", "))
        }

        Start-Sleep -Milliseconds 100
    }
}

if (-not (Test-Path -LiteralPath (Join-Path $repositoryRoot "RAG-Challenge.sln") -PathType Leaf)) {
    throw "The RAG-Challenge repository root could not be resolved."
}

if ($Mode -eq "Run" -and $AuthorityId -cne $expectedAuthority) {
    throw "The future A3 command requires the exact AUTH-S07-A-RUN-001 authority."
}

if ($Mode -eq "Validate" -and -not [string]::IsNullOrEmpty($AuthorityId)) {
    throw "Harness validation does not accept run authority."
}

$filter = if ($Mode -eq "Run") {
    "FullyQualifiedName=RagChallenge.IntegrationTests.S07ALocalHarness.S07ALocalHarnessCampaignTests.ExecuteFrozenCandidateAsync"
}
else {
    "FullyQualifiedName~RagChallenge.IntegrationTests.S07ALocalHarness.S07ALocalHarnessTests"
}

Push-Location $repositoryRoot

try {
    if ($Mode -eq "Run") {
        [System.Environment]::SetEnvironmentVariable(
            $authorityVariable,
            $expectedAuthority,
            [System.EnvironmentVariableTarget]::Process)
    }
    else {
        [System.Environment]::SetEnvironmentVariable(
            $authorityVariable,
            $null,
            [System.EnvironmentVariableTarget]::Process)
    }

    $testExitCode = if ($Mode -eq "Validate") {
        Invoke-ValidationTest `
            -ProjectPath $projectPath `
            -TestFilter $filter `
            -WorkingDirectory $repositoryRoot `
            -TestHostTimeout $testHostExitTimeout
    }
    else {
        & dotnet test $projectPath `
            --configuration Release `
            --no-restore `
            --filter $filter `
            --logger "console;verbosity=normal"
        $LASTEXITCODE
    }

    if ($testExitCode -ne 0) {
        throw "The S07-A local harness command failed with exit code $testExitCode."
    }
}
finally {
    [System.Environment]::SetEnvironmentVariable(
        $authorityVariable,
        $previousAuthority,
        [System.EnvironmentVariableTarget]::Process)
    Pop-Location
}
