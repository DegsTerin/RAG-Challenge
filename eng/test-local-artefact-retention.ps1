# Purpose: Verifies the local retention boundary in bounded synthetic repositories; it never authorises or applies cleanup to the real project.

[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "Invoke-LocalArtefactRetention.ps1")

$utf8 = [System.Text.UTF8Encoding]::new($false)
$temporaryParent = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath()).TrimEnd(
    [System.IO.Path]::DirectorySeparatorChar,
    [System.IO.Path]::AltDirectorySeparatorChar)
$temporaryRoot = Join-Path $temporaryParent (
    "rag-challenge-retention-test-" + [guid]::NewGuid().ToString("N"))
$expectedPrefix = $temporaryParent + [System.IO.Path]::DirectorySeparatorChar +
    "rag-challenge-retention-test-"
$repositoryRoot = Join-Path $temporaryRoot "repository"
$externalRoot = Join-Path $temporaryRoot "external"
$createdLinks = [System.Collections.Generic.List[string]]::new()

function Write-TestFile {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Content
    )

    $parent = [System.IO.Path]::GetDirectoryName($Path)
    [System.IO.Directory]::CreateDirectory($parent) | Out-Null
    [System.IO.File]::WriteAllText($Path, $Content, $utf8)
}

function Invoke-TestGit {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string[]]$Arguments)

    & git -C $repositoryRoot @Arguments | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Synthetic Git setup failed."
    }
}

function Assert-TestCondition {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][bool]$Condition,
        [Parameter(Mandatory)][string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

function Invoke-ExpectedFailure {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][scriptblock]$Action,
        [Parameter(Mandatory)][string]$MessagePart
    )

    $failed = $false
    try {
        & $Action
    }
    catch {
        $failed = $true
        if (-not $_.Exception.Message.Contains(
                $MessagePart,
                [System.StringComparison]::Ordinal)) {
            throw "Expected failure '$MessagePart', received '$($_.Exception.Message)'."
        }
    }
    if (-not $failed) {
        throw "Expected failure '$MessagePart' did not occur."
    }
}

function Get-TestPlanEntry {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]$Plan,
        [Parameter(Mandatory)][string]$RelativePath
    )

    return @($Plan.entries | Where-Object RelativePath -ceq $RelativePath)[0]
}

function Invoke-TestApply {
    [CmdletBinding()]
    param([Parameter(Mandatory)]$Plan)

    return Invoke-LocalArtefactRetention `
        -RepositoryRoot $repositoryRoot `
        -Apply `
        -ApprovedPlanSha256 $Plan.planSha256 `
        -ApprovedGitStatusSha256 $Plan.gitStatusSha256 `
        -ApprovedWorktreeIdentitySha256 $Plan.worktreeIdentitySha256 `
        -ApprovedLegacyOwnershipAttestationSha256 (
            $Plan.legacyOwnershipAttestationSha256)
}

function Invoke-TestRecoveryApply {
    [CmdletBinding()]
    param([Parameter(Mandatory)]$Plan)

    return Invoke-LocalRetentionRecovery `
        -RepositoryRoot $repositoryRoot `
        -TransactionId $Plan.transactionId `
        -ApplyRecovery `
        -ApprovedRecoveryPlanSha256 $Plan.recoveryPlanSha256 `
        -ApprovedRecoveryJournalSha256 $Plan.journalSha256 `
        -ApprovedGitStatusSha256 $Plan.gitStatusSha256 `
        -ApprovedWorktreeIdentitySha256 $Plan.worktreeIdentitySha256
}

try {
    $resolvedTemporaryRoot = [System.IO.Path]::GetFullPath($temporaryRoot)
    if (-not $resolvedTemporaryRoot.StartsWith(
            $expectedPrefix,
            [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "The synthetic test root escaped its bounded prefix."
    }

    [System.IO.Directory]::CreateDirectory($repositoryRoot) | Out-Null
    [System.IO.Directory]::CreateDirectory($externalRoot) | Out-Null
    & git -C $repositoryRoot init -b main | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Synthetic Git initialisation failed."
    }
    Invoke-TestGit -Arguments @("config", "user.name", "Retention Test")
    Invoke-TestGit -Arguments @("config", "user.email", "retention-test@example.invalid")

    Write-TestFile -Path (Join-Path $repositoryRoot ".gitignore") -Content @"
**/bin/
**/dist/
TestResults/
artifacts-local/
.env.local
/corpus/
/reference-materials/
**/obj/
**/node_modules/
ignored-parent/
owned/output/
/linked-parent
/safe-tree/
"@
    Write-TestFile -Path (Join-Path $repositoryRoot "tracked-sentinel.txt") -Content "tracked`n"
    Invoke-TestGit -Arguments @("add", ".gitignore", "tracked-sentinel.txt")
    Invoke-TestGit -Arguments @("commit", "-m", "test: establish retention fixture")

    $applicationBin = Join-Path $repositoryRoot "src\RagChallenge.Application\bin"
    $domainBin = Join-Path $repositoryRoot "src\RagChallenge.Domain\bin"
    $testResults = Join-Path $repositoryRoot "TestResults"
    Write-TestFile -Path (Join-Path $applicationBin "generated.dll") -Content "generated`n"
    Write-TestFile -Path (Join-Path $testResults "coverage.xml") -Content "coverage`n"

    $protectedFiles = @(
        ".env.local",
        "corpus\preserve.pdf",
        "reference-materials\preserve.txt",
        "artifacts-local\preserve.txt",
        "src\RagChallenge.Application\obj\preserve.txt",
        "src\RagChallenge.Dashboard.Web\node_modules\preserve.txt",
        "tools\ai-orchestrator\node_modules\preserve.txt"
    )
    foreach ($relativePath in $protectedFiles) {
        Write-TestFile -Path (Join-Path $repositoryRoot $relativePath) -Content "preserve`n"
    }

    Write-TestFile -Path (Join-Path $repositoryRoot "tracked-sentinel.txt") -Content "owner change`n"
    $ownerWipPath = Join-Path $repositoryRoot "owner-wip.md"
    Write-TestFile -Path $ownerWipPath -Content "untracked owner work`n"

    foreach ($unsafePath in @(
            "../escape",
            "src/*/bin",
            ".git",
            ".env.local",
            "corpus",
            "reference-materials",
            "artifacts-local/output",
            "src/RagChallenge.Application/obj",
            "src/RagChallenge.Dashboard.Web/node_modules")) {
        Invoke-ExpectedFailure -MessagePart $(if ($unsafePath -eq "src/*/bin") {
                "literal repository-relative path"
            }
            elseif ($unsafePath -eq "../escape") {
                "unsafe segment"
            }
            else {
                "protected"
            }) -Action {
            Resolve-LocalRetentionPath `
                -ResolvedRepositoryRoot $repositoryRoot `
                -RelativePath $unsafePath | Out-Null
        }
    }

    Invoke-ExpectedFailure -MessagePart "literal repository-relative path" -Action {
        Resolve-LocalRetentionPath `
            -ResolvedRepositoryRoot $repositoryRoot `
            -RelativePath ([System.IO.Path]::GetFullPath((Join-Path (
                        $repositoryRoot) "outside"))) | Out-Null
    }

    $ignoredChild = Join-Path $repositoryRoot "ignored-parent\child"
    [System.IO.Directory]::CreateDirectory($ignoredChild) | Out-Null
    Invoke-ExpectedFailure -MessagePart "ignored parent" -Action {
        Assert-LocalRetentionGitBoundary `
            -ResolvedRepositoryRoot $repositoryRoot `
            -RelativePath "ignored-parent/child"
    }

    $ownedCandidate = [pscustomobject]@{
        RelativePath = "owned/output"
        RetentionClass = "Generated"
        OwnershipKind = "MarkerOwned"
        MarkerPurpose = "retention-test-output"
        MarkerOwner = "eng/test-local-artefact-retention.ps1"
    }
    $ownedOutput = Join-Path $repositoryRoot "owned\output"
    [System.IO.Directory]::CreateDirectory($ownedOutput) | Out-Null
    Invoke-ExpectedFailure -MessagePart "marker is missing or invalid" -Action {
        Assert-LocalRetentionOwnership -Candidate $ownedCandidate -TargetPath $ownedOutput
    }
    Remove-Item -LiteralPath $ownedOutput -Recurse -Force
    New-LocalRetentionOwnedOutputRoot `
        -OutputRoot $ownedOutput `
        -RepositoryRoot $repositoryRoot `
        -Purpose $ownedCandidate.MarkerPurpose `
        -Owner $ownedCandidate.MarkerOwner `
        -CanonicalRelativePath $ownedCandidate.RelativePath
    Assert-LocalRetentionOwnership -Candidate $ownedCandidate -TargetPath $ownedOutput
    $wrongOwnerCandidate = $ownedCandidate.PSObject.Copy()
    $wrongOwnerCandidate.MarkerOwner = "eng/wrong-owner.ps1"
    Invoke-ExpectedFailure -MessagePart "marker is missing or invalid" -Action {
        Assert-LocalRetentionOwnership -Candidate $wrongOwnerCandidate -TargetPath $ownedOutput
    }
    Write-TestFile `
        -Path (Join-Path $ownedOutput $script:LocalRetentionMarkerName) `
        -Content "corrupt`n"
    Invoke-ExpectedFailure -MessagePart "marker is missing or invalid" -Action {
        Assert-LocalRetentionOwnership -Candidate $ownedCandidate -TargetPath $ownedOutput
    }

    $linkCreated = $false
    $linkItemType = "SymbolicLink"
    $parentLink = Join-Path $repositoryRoot "linked-parent"
    try {
        New-Item -ItemType SymbolicLink -Path $parentLink -Target $externalRoot -ErrorAction Stop |
            Out-Null
        $createdLinks.Add($parentLink)
        $linkCreated = $true
    }
    catch {
        try {
            $linkItemType = "Junction"
            New-Item -ItemType Junction -Path $parentLink -Target $externalRoot -ErrorAction Stop |
                Out-Null
            $createdLinks.Add($parentLink)
            $linkCreated = $true
        }
        catch {
            Write-Output "SKIP: reparse-point creation is not permitted on this host."
        }
    }
    if ($linkCreated) {
        Invoke-ExpectedFailure -MessagePart "unsafe component" -Action {
            Assert-LocalRetentionExistingComponentsAreSafe `
                -RepositoryRoot $repositoryRoot `
                -Path (Join-Path $parentLink "child")
        }
        $safeTree = Join-Path $repositoryRoot "safe-tree"
        [System.IO.Directory]::CreateDirectory($safeTree) | Out-Null
        $childLink = Join-Path $safeTree "linked-child"
        New-Item -ItemType $linkItemType -Path $childLink -Target $externalRoot -ErrorAction Stop |
            Out-Null
        $createdLinks.Add($childLink)
        Invoke-ExpectedFailure -MessagePart "contains a reparse point" -Action {
            Assert-LocalRetentionTreeIsSafe -Root $safeTree
        }
    }

    $alternateShareControlFile = Join-Path $temporaryRoot (
        "alternate-share-control.bin")
    $alternateShareControlStream = $alternateShareControlFile + ":owner-wip"
    Write-TestFile -Path $alternateShareControlFile -Content "control`n"
    $alternateShareControlHandle = [System.IO.FileStream]::new(
        $alternateShareControlStream,
        [System.IO.FileMode]::OpenOrCreate,
        [System.IO.FileAccess]::Write,
        ([System.IO.FileShare]::Read -bor [System.IO.FileShare]::Delete))
    try {
        $alternateShareControlHandle.WriteByte(1)
    }
    finally {
        $alternateShareControlHandle.Dispose()
    }
    Assert-TestCondition `
        -Condition ([System.IO.File]::Exists($alternateShareControlStream)) `
        -Message "The compatible-share ADS control could not create its stream."
    [System.IO.File]::Delete($alternateShareControlStream)
    [System.IO.File]::Delete($alternateShareControlFile)

    $nativeRacedRoot = Join-Path $temporaryRoot "native-delete-raced"
    $nativeRacedFile = Join-Path $nativeRacedRoot "approved.bin"
    $nativeRacedStream = $nativeRacedFile + ":compatible-owner-wip"
    Write-TestFile -Path $nativeRacedFile -Content "approved bytes`n"
    $nativeRacedMeasurement = Get-LocalRetentionTreeMeasurement `
        -TargetPath $nativeRacedRoot
    $script:CompatibleAlternateStreamCreatedBeforeArm = $false
    $compatibleRaceHook = {
        param($lockedRoot)

        $compatibleStream = $null
        try {
            $compatibleStream = [System.IO.FileStream]::new(
                (Join-Path $lockedRoot "approved.bin") + ":compatible-owner-wip",
                [System.IO.FileMode]::OpenOrCreate,
                [System.IO.FileAccess]::Write,
                ([System.IO.FileShare]::Read -bor [System.IO.FileShare]::Delete))
            $compatibleStream.WriteByte(1)
            $script:CompatibleAlternateStreamCreatedBeforeArm = $true
        }
        finally {
            if ($null -ne $compatibleStream) { $compatibleStream.Dispose() }
        }
    }
    Invoke-ExpectedFailure -MessagePart "named alternate data stream" -Action {
        Remove-LocalRetentionStagedTarget `
            -Path $nativeRacedRoot `
            -ExpectedStructuralTreeSha256 $nativeRacedMeasurement.StructuralTreeSha256 `
            -ExpectedByteLength $nativeRacedMeasurement.ByteLength `
            -BeforeDispositionTestHook $compatibleRaceHook
    }
    Assert-TestCondition `
        -Condition $script:CompatibleAlternateStreamCreatedBeforeArm `
        -Message "The compatible-share ADS race fixture did not create its stream."
    Assert-TestCondition -Condition (Test-Path -LiteralPath $nativeRacedRoot) `
        -Message "A pre-arm ADS race did not preserve the synthetic target root."
    Assert-TestCondition -Condition ([System.IO.File]::Exists($nativeRacedFile)) `
        -Message "A pre-arm ADS race did not preserve the approved base file."
    Assert-TestCondition -Condition ([System.IO.File]::Exists($nativeRacedStream)) `
        -Message "A pre-arm ADS race did not preserve the late alternate stream."
    [System.IO.File]::Delete($nativeRacedStream)
    [System.IO.File]::Delete($nativeRacedFile)
    [System.IO.Directory]::Delete($nativeRacedRoot, $false)

    $nativeCancelledRoot = Join-Path $temporaryRoot "native-delete-cancelled"
    $nativeCancelledFile = Join-Path $nativeCancelledRoot "approved.bin"
    Write-TestFile -Path $nativeCancelledFile -Content "approved bytes`n"
    (Get-Item -LiteralPath $nativeCancelledFile -Force).Attributes =
        [System.IO.FileAttributes]::ReadOnly
    (Get-Item -LiteralPath $nativeCancelledRoot -Force).Attributes =
        ((Get-Item -LiteralPath $nativeCancelledRoot -Force).Attributes -bor
            [System.IO.FileAttributes]::ReadOnly)
    $nativeCancelledMeasurement = Get-LocalRetentionTreeMeasurement `
        -TargetPath $nativeCancelledRoot
    $cancellationHook = {
        throw "synthetic deletion-pending cancellation"
    }
    Invoke-ExpectedFailure `
        -MessagePart "synthetic deletion-pending cancellation" `
        -Action {
            Remove-LocalRetentionStagedTarget `
                -Path $nativeCancelledRoot `
                -ExpectedStructuralTreeSha256 (
                    $nativeCancelledMeasurement.StructuralTreeSha256) `
                -ExpectedByteLength $nativeCancelledMeasurement.ByteLength `
                -AfterDeletePendingTestHook $cancellationHook
        }
    Assert-TestCondition -Condition (Test-Path -LiteralPath $nativeCancelledRoot) `
        -Message "A cancelled reversible deletion did not preserve its root."
    Assert-TestCondition -Condition ([System.IO.File]::Exists($nativeCancelledFile)) `
        -Message "A cancelled reversible deletion did not preserve its file."
    Assert-TestCondition -Condition (((Get-Item `
                -LiteralPath $nativeCancelledFile `
                -Force).Attributes -band
            [System.IO.FileAttributes]::ReadOnly) -ne 0) `
        -Message "A cancelled reversible deletion changed a ReadOnly file."
    Assert-TestCondition -Condition (((Get-Item `
                -LiteralPath $nativeCancelledRoot `
                -Force).Attributes -band
            [System.IO.FileAttributes]::ReadOnly) -ne 0) `
        -Message "A cancelled reversible deletion changed a ReadOnly root."
    (Get-Item -LiteralPath $nativeCancelledFile -Force).Attributes =
        [System.IO.FileAttributes]::Normal
    $cancelledRootItem = Get-Item -LiteralPath $nativeCancelledRoot -Force
    $cancelledRootItem.Attributes = [System.IO.FileAttributes](
        [int]$cancelledRootItem.Attributes -band
        (-bnot [int][System.IO.FileAttributes]::ReadOnly))
    [System.IO.File]::Delete($nativeCancelledFile)
    [System.IO.Directory]::Delete($nativeCancelledRoot, $false)

    $nativeDeleteRoot = Join-Path $temporaryRoot "native-delete"
    $nativeApprovedFile = Join-Path $nativeDeleteRoot "approved.bin"
    Write-TestFile -Path $nativeApprovedFile -Content "approved bytes`n"
    (Get-Item -LiteralPath $nativeApprovedFile -Force).Attributes =
        [System.IO.FileAttributes]::ReadOnly
    (Get-Item -LiteralPath $nativeDeleteRoot -Force).Attributes =
        ((Get-Item -LiteralPath $nativeDeleteRoot -Force).Attributes -bor
            [System.IO.FileAttributes]::ReadOnly)
    $nativeDeleteMeasurement = Get-LocalRetentionTreeMeasurement `
        -TargetPath $nativeDeleteRoot
    $script:SamePathMoveBlocked = $false
    $script:SamePathReplacementBlocked = $false
    $script:SamePathAlternateStreamBlocked = $false
    $script:SamePathCompatibleAlternateStreamBlocked = $false
    $samePathHook = {
        param($lockedRoot)

        $approvedPath = Join-Path $lockedRoot "approved.bin"
        try {
            [System.IO.File]::Move(
                $approvedPath,
                (Join-Path $lockedRoot "approved-old.bin"))
        }
        catch {
            $script:SamePathMoveBlocked = $true
        }
        try {
            [System.IO.File]::WriteAllText(
                $approvedPath,
                "replacement bytes`n",
                $utf8)
        }
        catch {
            $script:SamePathReplacementBlocked = $true
        }
        try {
            [System.IO.File]::WriteAllText(
                $approvedPath + ":owner-wip",
                "late alternate bytes`n",
                $utf8)
        }
        catch {
            $script:SamePathAlternateStreamBlocked = $true
        }
    }
    $afterDeletePendingHook = {
        param($lockedRoot)

        $approvedPath = Join-Path $lockedRoot "approved.bin"
        $compatibleStream = $null
        try {
            $compatibleStream = [System.IO.FileStream]::new(
                $approvedPath + ":compatible-owner-wip",
                [System.IO.FileMode]::OpenOrCreate,
                [System.IO.FileAccess]::Write,
                ([System.IO.FileShare]::Read -bor [System.IO.FileShare]::Delete))
            $compatibleStream.WriteByte(1)
        }
        catch {
            $script:SamePathCompatibleAlternateStreamBlocked = $true
        }
        finally {
            if ($null -ne $compatibleStream) { $compatibleStream.Dispose() }
        }
    }
    Remove-LocalRetentionStagedTarget `
        -Path $nativeDeleteRoot `
        -ExpectedStructuralTreeSha256 $nativeDeleteMeasurement.StructuralTreeSha256 `
        -ExpectedByteLength $nativeDeleteMeasurement.ByteLength `
        -BeforeDispositionTestHook $samePathHook `
        -AfterDeletePendingTestHook $afterDeletePendingHook
    Assert-TestCondition -Condition $script:SamePathMoveBlocked `
        -Message "A locked approved file could be renamed before handle-bound deletion."
    Assert-TestCondition -Condition $script:SamePathReplacementBlocked `
        -Message "A same-path replacement could be created before handle-bound deletion."
    Assert-TestCondition -Condition $script:SamePathAlternateStreamBlocked `
        -Message "A named alternate stream could be created after deletion locks."
    Assert-TestCondition `
        -Condition $script:SamePathCompatibleAlternateStreamBlocked `
        -Message "A delete-sharing ADS writer bypassed reversible delete-pending."
    Assert-TestCondition -Condition (-not (Test-Path -LiteralPath $nativeDeleteRoot)) `
        -Message "Handle-bound deletion did not remove the approved synthetic tree."

    $stagingGuardRoot = Join-Path $temporaryRoot "staging-guard"
    $stagingGuardMoved = Join-Path $temporaryRoot "staging-guard-moved"
    [System.IO.Directory]::CreateDirectory($stagingGuardRoot) | Out-Null
    $stagingGuard = [RagChallenge.LocalRetention.NativePathHandle]::OpenDirectoryGuard(
        $stagingGuardRoot)
    $stagingIdentity = $stagingGuard.IdentityToken
    $stagingMoveBlocked = $false
    try {
        try {
            [System.IO.Directory]::Move($stagingGuardRoot, $stagingGuardMoved)
        }
        catch {
            $stagingMoveBlocked = $true
        }
        $stagingCurrent = [RagChallenge.LocalRetention.NativePathHandle]::OpenIdentity(
            $stagingGuardRoot,
            $true)
        try {
            Assert-TestCondition `
                -Condition ($stagingCurrent.IdentityToken -ceq $stagingIdentity) `
                -Message "The guarded staging pathname changed filesystem identity."
        }
        finally {
            $stagingCurrent.Dispose()
        }
    }
    finally {
        $stagingGuard.Dispose()
    }
    Assert-TestCondition -Condition $stagingMoveBlocked `
        -Message "The staging-directory handle did not block a pathname swap."
    [System.IO.Directory]::Move($stagingGuardRoot, $stagingGuardMoved)
    Assert-TestCondition -Condition (Test-Path -LiteralPath $stagingGuardMoved) `
        -Message "The staging rename did not succeed after the directory guard was released."
    [System.IO.Directory]::Delete($stagingGuardMoved, $false)

    $staleNativeTypeProbe = Join-Path $temporaryRoot "stale-native-type-probe.ps1"
    $executorLiteral = (Join-Path $PSScriptRoot (
            "Invoke-LocalArtefactRetention.ps1")).Replace("'", "''")
    Write-TestFile -Path $staleNativeTypeProbe -Content @"
Add-Type -TypeDefinition @'
namespace RagChallenge.LocalRetention
{
    public sealed class NativePathHandle { }
}
'@
try
{
    . '$executorLiteral'
    exit 91
}
catch
{
    if (`$_.Exception.Message.Contains('preloaded local-retention native type'))
    {
        exit 0
    }
    [Console]::Error.WriteLine(`$_.Exception.Message)
    exit 92
}
"@
    & (Join-Path $PSHOME "pwsh.exe") `
        -NoLogo `
        -NoProfile `
        -NonInteractive `
        -File $staleNativeTypeProbe
    Assert-TestCondition -Condition ($LASTEXITCODE -eq 0) `
        -Message "A stale preloaded native type was not refused before dry-run."

    $sensitivePath = Join-Path $domainBin ".env.local"
    Write-TestFile -Path $sensitivePath -Content "must remain secret`n"
    $sensitivePlan = Get-LocalArtefactRetentionPlan -RepositoryRoot $repositoryRoot
    $sensitiveEntry = Get-TestPlanEntry $sensitivePlan "src/RagChallenge.Domain/bin"
    Assert-TestCondition -Condition $sensitivePlan.blocked `
        -Message "Local configuration inside a generated tree did not block Apply."
    Assert-TestCondition -Condition ($sensitiveEntry.Disposition -ceq "PRESERVE_UNCERTAIN") `
        -Message "Sensitive generated-tree content was not classified as uncertain."
    Assert-TestCondition -Condition (Test-Path -LiteralPath $sensitivePath) `
        -Message "Sensitive generated-tree content was removed during dry-run."
    Remove-Item -LiteralPath $domainBin -Recurse -Force

    $generatedFile = Join-Path $applicationBin "generated.dll"
    $alternateStreamPath = $generatedFile + ":owner-wip"
    $alternateStreamCreated = $false
    try {
        [System.IO.File]::WriteAllText(
            $alternateStreamPath,
            "preserve alternate bytes`n",
            $utf8)
        $alternateStreamCreated = $true
        $alternateStreamPlan = Get-LocalArtefactRetentionPlan `
            -RepositoryRoot $repositoryRoot
        $alternateStreamEntry = Get-TestPlanEntry `
            $alternateStreamPlan `
            "src/RagChallenge.Application/bin"
        Assert-TestCondition -Condition $alternateStreamPlan.blocked `
            -Message "A named alternate data stream did not block Apply."
        Assert-TestCondition `
            -Condition ($alternateStreamEntry.Disposition -ceq "PRESERVE_UNCERTAIN") `
            -Message "A named alternate data stream was not preserved as uncertain."
        Assert-TestCondition -Condition (-not $alternateStreamEntry.ContentRead) `
            -Message "Alternate data stream content was read during inventory."
        Assert-TestCondition -Condition ([System.IO.File]::Exists($alternateStreamPath)) `
            -Message "Dry-run removed a named alternate data stream."
    }
    catch [System.NotSupportedException] {
        Write-Output "SKIP: named alternate data streams are not supported on this volume."
    }
    finally {
        if ($alternateStreamCreated -and
            [System.IO.File]::Exists($alternateStreamPath)) {
            [System.IO.File]::Delete($alternateStreamPath)
        }
    }

    $envPath = Join-Path $repositoryRoot ".env.local"
    $envStream = [System.IO.File]::Open(
        $envPath,
        [System.IO.FileMode]::Open,
        [System.IO.FileAccess]::ReadWrite,
        [System.IO.FileShare]::None)
    try {
        $secretBoundaryPlan = Get-LocalArtefactRetentionPlan -RepositoryRoot $repositoryRoot
        Assert-TestCondition -Condition (-not $secretBoundaryPlan.blocked) `
            -Message ".env.local content was opened or treated as a deletion boundary."
        $envEntry = @($secretBoundaryPlan.protected |
            Where-Object RelativePath -ceq ".env.local")[0]
        Assert-TestCondition -Condition (-not $envEntry.ContentRead) `
            -Message ".env.local was not explicitly reported as unread."
    }
    finally {
        $envStream.Dispose()
    }

    $visibleSecret = Join-Path $repositoryRoot "appsettings.Local.json"
    Write-TestFile -Path $visibleSecret -Content "sensitive canary`n"
    $visibleSecretStream = [System.IO.File]::Open(
        $visibleSecret,
        [System.IO.FileMode]::Open,
        [System.IO.FileAccess]::ReadWrite,
        [System.IO.FileShare]::None)
    try {
        $visibleSecretPlan = Get-LocalArtefactRetentionPlan -RepositoryRoot $repositoryRoot
        Assert-TestCondition -Condition (-not $visibleSecretPlan.blocked) `
            -Message "Git-visible configuration was not preserved through the no-read WIP boundary."
        $visibleSecretRecord = @($visibleSecretPlan.gitVisibleWip |
            Where-Object path -ceq "appsettings.Local.json")[0]
        Assert-TestCondition -Condition (-not $visibleSecretRecord.contentRead) `
            -Message "Git-visible configuration content was read."
    }
    finally {
        $visibleSecretStream.Dispose()
        Remove-Item -LiteralPath $visibleSecret -Force
    }

    $unknownConfiguration = Join-Path $repositoryRoot ".npmrc"
    Write-TestFile -Path $unknownConfiguration -Content "configuration canary`n"
    $unknownConfigurationStream = [System.IO.File]::Open(
        $unknownConfiguration,
        [System.IO.FileMode]::Open,
        [System.IO.FileAccess]::ReadWrite,
        [System.IO.FileShare]::None)
    try {
        $unknownConfigurationPlan = Get-LocalArtefactRetentionPlan `
            -RepositoryRoot $repositoryRoot
        Assert-TestCondition -Condition (-not $unknownConfigurationPlan.blocked) `
            -Message "Unknown untracked configuration was not structurally preserved."
        $unknownConfigurationRecord = @($unknownConfigurationPlan.gitVisibleWip |
            Where-Object path -ceq ".npmrc")[0]
        Assert-TestCondition -Condition (-not $unknownConfigurationRecord.contentRead) `
            -Message "Unknown untracked configuration content was read."
    }
    finally {
        $unknownConfigurationStream.Dispose()
        Remove-Item -LiteralPath $unknownConfiguration -Force
    }

    $untrackedCanary = Join-Path $repositoryRoot "eng\unapproved-wip-canary.ps1"
    Write-TestFile -Path $untrackedCanary -Content "must remain unread`n"
    $untrackedCanaryStream = [System.IO.File]::Open(
        $untrackedCanary,
        [System.IO.FileMode]::Open,
        [System.IO.FileAccess]::ReadWrite,
        [System.IO.FileShare]::None)
    try {
        $untrackedCanaryPlan = Get-LocalArtefactRetentionPlan `
            -RepositoryRoot $repositoryRoot
        Assert-TestCondition -Condition (-not $untrackedCanaryPlan.blocked) `
            -Message "Unread untracked WIP was not structurally preserved."
        $untrackedCanaryRecord = @($untrackedCanaryPlan.gitVisibleWip |
            Where-Object path -ceq "eng/unapproved-wip-canary.ps1")[0]
        Assert-TestCondition -Condition (-not $untrackedCanaryRecord.contentRead) `
            -Message "An untracked WIP canary was opened for content."
    }
    finally {
        $untrackedCanaryStream.Dispose()
        Remove-Item -LiteralPath $untrackedCanary -Force
    }

    $serverBin = Join-Path $repositoryRoot "src\RagChallenge.Server.Api\bin"
    $generatedConfiguration = Join-Path $serverBin "appsettings.json"
    Write-TestFile -Path $generatedConfiguration -Content "generated configuration canary`n"
    $generatedConfigurationStream = [System.IO.File]::Open(
        $generatedConfiguration,
        [System.IO.FileMode]::Open,
        [System.IO.FileAccess]::ReadWrite,
        [System.IO.FileShare]::None)
    try {
        $configurationPlan = Get-LocalArtefactRetentionPlan -RepositoryRoot $repositoryRoot
        $configurationEntry = Get-TestPlanEntry `
            $configurationPlan `
            "src/RagChallenge.Server.Api/bin"
        Assert-TestCondition `
            -Condition ($configurationEntry.Disposition -ceq "PRESERVE_CONFIGURATION") `
            -Message "Generated configuration copies were not preserved as a complete root."
        Assert-TestCondition -Condition (-not $configurationPlan.blocked) `
            -Message "Known preserved configuration was incorrectly treated as uncertain."
        Assert-TestCondition -Condition ($null -eq $configurationEntry.StructuralTreeSha256) `
            -Message "Configuration content was hashed despite the no-read boundary."
    }
    finally {
        $generatedConfigurationStream.Dispose()
        Remove-Item -LiteralPath $serverBin -Recurse -Force
    }

    $plan = Get-LocalArtefactRetentionPlan -RepositoryRoot $repositoryRoot
    Assert-TestCondition -Condition (-not $plan.blocked) `
        -Message "The valid dry-run was unexpectedly blocked."
    Assert-TestCondition -Condition ($plan.gitStatusEntryCount -eq 2) `
        -Message "Dirty tracked and untracked work was not represented exactly."
    Assert-TestCondition `
        -Condition ((Get-TestPlanEntry $plan (
                    "src/RagChallenge.Application/bin")).Disposition -ceq
            "DELETE_CANDIDATE_REQUIRES_ATTESTATION") `
        -Message "Known generated output was not bound to one-shot owner attestation."
    Assert-TestCondition `
        -Condition ((Get-TestPlanEntry $plan "TestResults").Disposition -ceq
            "PRESERVE_RETENTION_WINDOW") `
        -Message "Fresh test evidence did not retain its seven-day window."
    Assert-TestCondition `
        -Condition (@($plan.coverage | Where-Object ClassId -ceq (
                    "superseded-generations")).Count -eq 1) `
        -Message "The executable plan omitted a required retention class."

    $fakeGitRepository = Join-Path $externalRoot "fake-git-repository"
    [System.IO.Directory]::CreateDirectory($fakeGitRepository) | Out-Null
    & git -C $fakeGitRepository init -b main | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Fake Git environment setup failed." }
    & git -C $fakeGitRepository config user.name "Retention Test" | Out-Null
    & git -C $fakeGitRepository config user.email `
        "retention-test@example.invalid" | Out-Null
    Write-TestFile `
        -Path (Join-Path $fakeGitRepository "fake.txt") `
        -Content "fake repository`n"
    & git -C $fakeGitRepository add fake.txt | Out-Null
    & git -C $fakeGitRepository commit -m "test: fake Git boundary" | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Fake Git environment commit failed." }
    $hostileGitEnvironment = [ordered]@{
        GIT_DIR = Join-Path $fakeGitRepository ".git"
        GIT_WORK_TREE = $fakeGitRepository
        GIT_INDEX_FILE = Join-Path $fakeGitRepository ".git\index"
        GIT_OBJECT_DIRECTORY = Join-Path $fakeGitRepository ".git\objects"
        GIT_CONFIG_COUNT = "1"
        GIT_CONFIG_KEY_0 = "core.fsmonitor"
        GIT_CONFIG_VALUE_0 = "hostile-monitor"
    }
    $originalGitEnvironment = @{}
    foreach ($environmentName in $hostileGitEnvironment.Keys) {
        $originalGitEnvironment[$environmentName] =
            [Environment]::GetEnvironmentVariable(
                $environmentName,
                [EnvironmentVariableTarget]::Process)
        [Environment]::SetEnvironmentVariable(
            $environmentName,
            $hostileGitEnvironment[$environmentName],
            [EnvironmentVariableTarget]::Process)
    }
    try {
        $hostileEnvironmentPlan = Get-LocalArtefactRetentionPlan `
            -RepositoryRoot $repositoryRoot
        Assert-TestCondition `
            -Condition ($hostileEnvironmentPlan.baseline -ceq $plan.baseline) `
            -Message "Inherited GIT_* variables redirected the canonical baseline."
        Assert-TestCondition `
            -Condition ($hostileEnvironmentPlan.gitStatusSha256 -ceq (
                    $plan.gitStatusSha256)) `
            -Message "Inherited GIT_* variables redirected canonical status."
        Assert-TestCondition `
            -Condition ($hostileEnvironmentPlan.worktreeIdentitySha256 -ceq (
                    $plan.worktreeIdentitySha256)) `
            -Message "Inherited GIT_* variables redirected canonical WIP identity."
        Assert-TestCondition `
            -Condition ($hostileEnvironmentPlan.planSha256 -ceq $plan.planSha256) `
            -Message "Inherited GIT_* variables changed the hermetic plan."
    }
    finally {
        foreach ($environmentName in $hostileGitEnvironment.Keys) {
            [Environment]::SetEnvironmentVariable(
                $environmentName,
                $originalGitEnvironment[$environmentName],
            [EnvironmentVariableTarget]::Process)
        }
    }

    $generatedIdentityFile = Join-Path $applicationBin "generated.dll"
    $generatedIdentityItem = Get-Item -LiteralPath $generatedIdentityFile -Force
    $generatedCreationUtc = $generatedIdentityItem.CreationTimeUtc
    $generatedLastWriteUtc = $generatedIdentityItem.LastWriteTimeUtc
    $generatedAttributes = $generatedIdentityItem.Attributes
    $targetIdentityPlanBefore = Get-LocalArtefactRetentionPlan `
        -RepositoryRoot $repositoryRoot
    Write-TestFile -Path $generatedIdentityFile -Content "mutated!!`n"
    $generatedIdentityItem = Get-Item -LiteralPath $generatedIdentityFile -Force
    $generatedIdentityItem.CreationTimeUtc = $generatedCreationUtc
    $generatedIdentityItem.LastWriteTimeUtc = $generatedLastWriteUtc
    $generatedIdentityItem.Attributes = $generatedAttributes
    $targetIdentityPlanAfter = Get-LocalArtefactRetentionPlan `
        -RepositoryRoot $repositoryRoot
    $targetIdentityBefore = Get-TestPlanEntry `
        $targetIdentityPlanBefore `
        "src/RagChallenge.Application/bin"
    $targetIdentityAfter = Get-TestPlanEntry `
        $targetIdentityPlanAfter `
        "src/RagChallenge.Application/bin"
    Assert-TestCondition `
        -Condition ($targetIdentityBefore.ByteLength -eq $targetIdentityAfter.ByteLength) `
        -Message "The equal-length target-drift fixture changed byte length."
    Assert-TestCondition `
        -Condition ($targetIdentityBefore.StructuralTreeSha256 -cne (
                $targetIdentityAfter.StructuralTreeSha256)) `
        -Message "Equal-length target-byte drift did not change structural identity."
    Assert-TestCondition `
        -Condition ($targetIdentityPlanBefore.legacyOwnershipAttestationSha256 -cne (
                $targetIdentityPlanAfter.legacyOwnershipAttestationSha256)) `
        -Message "Equal-length target-byte drift did not invalidate attestation."
    Assert-TestCondition `
        -Condition ($targetIdentityPlanBefore.planSha256 -cne (
                $targetIdentityPlanAfter.planSha256)) `
        -Message "Equal-length target-byte drift did not invalidate the plan."

    $innocuousCanary = Join-Path $applicationBin "runtime.dat"
    Write-TestFile -Path $innocuousCanary -Content "arbitrarily named canary`n"
    $innocuousCanaryStream = [System.IO.File]::Open(
        $innocuousCanary,
        [System.IO.FileMode]::Open,
        [System.IO.FileAccess]::ReadWrite,
        [System.IO.FileShare]::None)
    try {
        $noReadLegacyPlan = Get-LocalArtefactRetentionPlan `
            -RepositoryRoot $repositoryRoot
        $noReadLegacyEntry = Get-TestPlanEntry `
            $noReadLegacyPlan `
            "src/RagChallenge.Application/bin"
        Assert-TestCondition `
            -Condition ($noReadLegacyEntry.Disposition -ceq (
                    "DELETE_CANDIDATE_REQUIRES_ATTESTATION")) `
            -Message "An unread legacy generated tree was not bound to owner attestation."
        Assert-TestCondition -Condition (-not $noReadLegacyPlan.blocked) `
            -Message "A metadata-only legacy-tree identity was incorrectly blocked."
        Assert-TestCondition -Condition (-not $noReadLegacyEntry.ContentRead) `
            -Message "Legacy generated content was read before owner attestation."
    }
    finally {
        $innocuousCanaryStream.Dispose()
        Remove-Item -LiteralPath $innocuousCanary -Force
    }

    $oldPlan = Get-LocalArtefactRetentionPlan -RepositoryRoot $repositoryRoot
    $ownerWipItem = Get-Item -LiteralPath $ownerWipPath -Force
    $ownerWipCreationUtc = $ownerWipItem.CreationTimeUtc
    $ownerWipLastWriteUtc = $ownerWipItem.LastWriteTimeUtc
    $ownerWipAttributes = $ownerWipItem.Attributes
    Write-TestFile -Path $ownerWipPath -Content "changedxx owner work`n"
    $ownerWipItem = Get-Item -LiteralPath $ownerWipPath -Force
    $ownerWipItem.CreationTimeUtc = $ownerWipCreationUtc
    $ownerWipItem.LastWriteTimeUtc = $ownerWipLastWriteUtc
    $ownerWipItem.Attributes = $ownerWipAttributes
    $changedWipPlan = Get-LocalArtefactRetentionPlan -RepositoryRoot $repositoryRoot
    $oldWipRecord = @($oldPlan.gitVisibleWip |
        Where-Object path -ceq "owner-wip.md")[0]
    $changedWipRecord = @($changedWipPlan.gitVisibleWip |
        Where-Object path -ceq "owner-wip.md")[0]
    Assert-TestCondition `
        -Condition ($oldWipRecord.byteLength -eq $changedWipRecord.byteLength) `
        -Message "The equal-length WIP-drift fixture changed byte length."
    Assert-TestCondition `
        -Condition ($changedWipPlan.gitStatusSha256 -ceq $oldPlan.gitStatusSha256) `
        -Message "The same-path WIP fixture unexpectedly changed porcelain status."
    Assert-TestCondition `
        -Condition ($changedWipPlan.worktreeIdentitySha256 -cne (
                $oldPlan.worktreeIdentitySha256)) `
        -Message "Equal-length WIP-byte drift did not change the identity boundary."
    Assert-TestCondition -Condition ($changedWipPlan.planSha256 -cne $oldPlan.planSha256) `
        -Message "Changed WIP bytes did not invalidate the plan."
    Invoke-ExpectedFailure -MessagePart "plan SHA-256" -Action {
        Invoke-TestApply -Plan $oldPlan | Out-Null
    }
    Assert-TestCondition -Condition (Test-Path -LiteralPath $applicationBin) `
        -Message "A stale WIP approval removed generated output."
    Write-TestFile -Path $ownerWipPath -Content "untracked owner work`n"

    $originalExecutorPath = $script:LocalRetentionExecutorPath
    $executorCopy = Join-Path $temporaryRoot "executor-copy.ps1"
    [System.IO.File]::Copy($originalExecutorPath, $executorCopy)
    try {
        $script:LocalRetentionExecutorPath = $executorCopy
        $executorPlanBefore = Get-LocalArtefactRetentionPlan -RepositoryRoot $repositoryRoot
        [System.IO.File]::AppendAllText($executorCopy, "# changed executor bytes`n", $utf8)
        $executorPlanAfter = Get-LocalArtefactRetentionPlan -RepositoryRoot $repositoryRoot
        Assert-TestCondition `
            -Condition ($executorPlanBefore.executorSha256 -cne (
                    $executorPlanAfter.executorSha256)) `
            -Message "Executor bytes were not bound to the plan."
        Assert-TestCondition `
            -Condition ($executorPlanBefore.planSha256 -cne $executorPlanAfter.planSha256) `
            -Message "Executor drift did not invalidate the plan."
    }
    finally {
        $script:LocalRetentionExecutorPath = $originalExecutorPath
    }

    $oldTimestamp = [datetime]::UtcNow.Subtract([timespan]::FromDays(8))
    (Get-Item -LiteralPath (Join-Path $testResults "coverage.xml")).LastWriteTimeUtc = $oldTimestamp
    (Get-Item -LiteralPath $testResults).LastWriteTimeUtc = $oldTimestamp
    $oldEvidencePlan = Get-LocalArtefactRetentionPlan -RepositoryRoot $repositoryRoot
    $oldEvidenceEntry = Get-TestPlanEntry $oldEvidencePlan "TestResults"
    Assert-TestCondition -Condition $oldEvidencePlan.blocked `
        -Message "Old evidence without canonical gate release did not block Apply."
    Assert-TestCondition -Condition ($oldEvidenceEntry.Disposition -ceq "PRESERVE_UNCERTAIN") `
        -Message "Age alone incorrectly authorised evidence deletion."
    (Get-Item -LiteralPath (Join-Path $testResults "coverage.xml")).LastWriteTimeUtc = [datetime]::UtcNow
    (Get-Item -LiteralPath $testResults).LastWriteTimeUtc = [datetime]::UtcNow

    $fakeEntry = [pscustomobject]@{ Path = $applicationBin }
    Assert-TestCondition -Condition (Test-LocalRetentionPotentialWriterProcess `
            -Process ([pscustomobject]@{
                Name = "dotnet.exe"
                CommandLine = "dotnet build"
                ExecutablePath = "C:\Program Files\dotnet\dotnet.exe"
            }) `
            -ResolvedRepositoryRoot $repositoryRoot `
            -DeletionEntries @($fakeEntry)) `
        -Message "An external dotnet writer was not classified fail-closed."
    Assert-TestCondition -Condition (-not (Test-LocalRetentionPotentialWriterProcess `
                -Process ([pscustomobject]@{
                    Name = "node.exe"
                    CommandLine = "node ./mcp/server.cjs --stdio"
                    ExecutablePath = "C:\Program Files\nodejs\node.exe"
                }) `
                -ResolvedRepositoryRoot $repositoryRoot `
                -DeletionEntries @($fakeEntry))) `
        -Message "An unrelated Node process was incorrectly classified as repository-owned."
    Assert-TestCondition -Condition (Test-LocalRetentionPotentialWriterProcess `
            -Process ([pscustomobject]@{
                Name = "node.exe"
                CommandLine = "node npm run build"
                ExecutablePath = "C:\Program Files\nodejs\node.exe"
            }) `
            -ResolvedRepositoryRoot $repositoryRoot `
            -DeletionEntries @($fakeEntry)) `
        -Message "A Node build writer was not classified fail-closed."
    Assert-TestCondition -Condition (Test-LocalRetentionPotentialWriterProcess `
            -Process ([pscustomobject]@{
                Name = "node.exe"
                CommandLine = $null
                ExecutablePath = $null
            }) `
            -ResolvedRepositoryRoot $repositoryRoot `
            -DeletionEntries @($fakeEntry)) `
        -Message "A Node process with inaccessible metadata was not classified fail-closed."

    $approvedPlan = Get-LocalArtefactRetentionPlan -RepositoryRoot $repositoryRoot
    Invoke-ExpectedFailure -MessagePart "plan SHA-256" -Action {
        Invoke-LocalArtefactRetention `
            -RepositoryRoot $repositoryRoot `
            -Apply `
            -ApprovedPlanSha256 ("0" * 64) `
            -ApprovedGitStatusSha256 $approvedPlan.gitStatusSha256 `
            -ApprovedWorktreeIdentitySha256 $approvedPlan.worktreeIdentitySha256 `
            -ApprovedLegacyOwnershipAttestationSha256 (
                $approvedPlan.legacyOwnershipAttestationSha256) | Out-Null
    }
    Invoke-ExpectedFailure -MessagePart "Git-status SHA-256" -Action {
        Invoke-LocalArtefactRetention `
            -RepositoryRoot $repositoryRoot `
            -Apply `
            -ApprovedPlanSha256 $approvedPlan.planSha256 `
            -ApprovedGitStatusSha256 ("0" * 64) `
            -ApprovedWorktreeIdentitySha256 $approvedPlan.worktreeIdentitySha256 `
            -ApprovedLegacyOwnershipAttestationSha256 (
                $approvedPlan.legacyOwnershipAttestationSha256) | Out-Null
    }
    Invoke-ExpectedFailure -MessagePart "worktree-identity SHA-256" -Action {
        Invoke-LocalArtefactRetention `
            -RepositoryRoot $repositoryRoot `
            -Apply `
            -ApprovedPlanSha256 $approvedPlan.planSha256 `
            -ApprovedGitStatusSha256 $approvedPlan.gitStatusSha256 `
            -ApprovedWorktreeIdentitySha256 ("0" * 64) `
            -ApprovedLegacyOwnershipAttestationSha256 (
                $approvedPlan.legacyOwnershipAttestationSha256) | Out-Null
    }
    Invoke-ExpectedFailure -MessagePart "legacy ownership attestation" -Action {
        Invoke-LocalArtefactRetention `
            -RepositoryRoot $repositoryRoot `
            -Apply `
            -ApprovedPlanSha256 $approvedPlan.planSha256 `
            -ApprovedGitStatusSha256 $approvedPlan.gitStatusSha256 `
            -ApprovedWorktreeIdentitySha256 $approvedPlan.worktreeIdentitySha256 `
            -ApprovedLegacyOwnershipAttestationSha256 ("0" * 64) | Out-Null
    }

    $applyResult = Invoke-TestApply -Plan $approvedPlan
    Assert-TestCondition -Condition ($applyResult.deletedTargetCount -eq 1) `
        -Message "The exact approved generated directory was not deleted once."
    Assert-TestCondition -Condition (-not (Test-Path -LiteralPath $applicationBin)) `
        -Message "The approved generated directory still exists."
    Assert-TestCondition -Condition (Test-Path -LiteralPath $testResults) `
        -Message "Fresh test evidence was removed."
    Assert-TestCondition -Condition (Test-Path -LiteralPath (
            $applyResult.transactionRecordPath)) `
        -Message "The material deletion did not produce a durable transaction record."
    foreach ($relativePath in $protectedFiles) {
        Assert-TestCondition -Condition (Test-Path -LiteralPath (Join-Path (
                    $repositoryRoot) $relativePath)) `
            -Message "A protected sentinel was changed or removed."
    }
    $gitAfterApply = Get-LocalRetentionGitState -ResolvedRepositoryRoot $repositoryRoot
    Assert-TestCondition `
        -Condition ($gitAfterApply.StatusSha256 -ceq $approvedPlan.gitStatusSha256) `
        -Message "The dirty Git tree changed during approved retention."
    Assert-TestCondition `
        -Condition ($gitAfterApply.WorktreeIdentitySha256 -ceq (
                $approvedPlan.worktreeIdentitySha256)) `
        -Message "The dirty WIP identity changed during approved retention."
    Invoke-ExpectedFailure -MessagePart "already consumed" -Action {
        Assert-LocalRetentionPlanNotConsumed `
            -ResolvedRepositoryRoot $repositoryRoot `
            -PlanSha256 $approvedPlan.planSha256
    }
    $unexpectedHistoryDirectory = Join-Path $repositoryRoot (
        "artifacts-local\retention-history\unexpected")
    [System.IO.Directory]::CreateDirectory($unexpectedHistoryDirectory) | Out-Null
    try {
        Invoke-ExpectedFailure -MessagePart "unexpected subdirectory" -Action {
            Assert-LocalRetentionPlanNotConsumed `
                -ResolvedRepositoryRoot $repositoryRoot `
                -PlanSha256 ("f" * 64)
        }
    }
    finally {
        [System.IO.Directory]::Delete($unexpectedHistoryDirectory, $false)
    }

    Write-TestFile -Path (Join-Path $applicationBin "generated.dll") -Content "first`n"
    $readOnlyPartialFile = Join-Path $domainBin "generated.dll"
    Write-TestFile -Path $readOnlyPartialFile -Content "second`n"
    (Get-Item -LiteralPath $readOnlyPartialFile -Force).Attributes =
        [System.IO.FileAttributes]::ReadOnly
    (Get-Item -LiteralPath $domainBin -Force).Attributes =
        ((Get-Item -LiteralPath $domainBin -Force).Attributes -bor
            [System.IO.FileAttributes]::ReadOnly)
    $readOnlyPartialPlan = Get-LocalArtefactRetentionPlan `
        -RepositoryRoot $repositoryRoot
    Assert-TestCondition `
        -Condition ($readOnlyPartialPlan.deletionCandidateCount -eq 2) `
        -Message "The ReadOnly recovery fixture did not create two candidates."
    $script:ReadOnlyPartialRemovalCount = 0
    $originalReadOnlyStagedRemoval =
        ${function:Remove-LocalRetentionStagedTarget}
    function Remove-LocalRetentionStagedTarget {
        [CmdletBinding()]
        param(
            [Parameter(Mandatory)][string]$Path,
            [Parameter(Mandatory)][string]$ExpectedStructuralTreeSha256,
            [Parameter(Mandatory)][long]$ExpectedByteLength
        )

        $script:ReadOnlyPartialRemovalCount++
        if ($script:ReadOnlyPartialRemovalCount -eq 2) {
            $children = @(Get-ChildItem -LiteralPath $Path -Recurse -Force)
            foreach ($file in @($children | Where-Object {
                        -not $_.PSIsContainer
                    })) {
                $file.Attributes = [System.IO.FileAttributes]::Normal
                [System.IO.File]::Delete($file.FullName)
            }
            foreach ($directory in @($children |
                    Where-Object PSIsContainer |
                    Sort-Object @{ Expression = {
                                $_.FullName.Length
                            }; Descending = $true })) {
                [System.IO.Directory]::Delete($directory.FullName, $false)
            }
            throw "synthetic ReadOnly partial root failure"
        }
        & $originalReadOnlyStagedRemoval `
            -Path $Path `
            -ExpectedStructuralTreeSha256 $ExpectedStructuralTreeSha256 `
            -ExpectedByteLength $ExpectedByteLength
    }
    try {
        Invoke-ExpectedFailure -MessagePart "partial material deletion" -Action {
            Invoke-TestApply -Plan $readOnlyPartialPlan | Out-Null
        }
    }
    finally {
        Set-Item `
            -Path Function:\Remove-LocalRetentionStagedTarget `
            -Value $originalReadOnlyStagedRemoval
    }
    $readOnlyTransactionRoot = Join-Path $repositoryRoot (
        "artifacts-local\retention-transactions")
    $readOnlyTransactions = @(Get-ChildItem `
            -LiteralPath $readOnlyTransactionRoot `
            -Directory `
            -Force)
    Assert-TestCondition -Condition ($readOnlyTransactions.Count -eq 1) `
        -Message "The ReadOnly partial fixture did not retain one transaction."
    $readOnlyTransactionId = $readOnlyTransactions[0].Name
    $readOnlyRecoveryPlan = Get-LocalRetentionRecoveryPlan `
        -RepositoryRoot $repositoryRoot `
        -TransactionId $readOnlyTransactionId
    Assert-TestCondition -Condition (-not $readOnlyRecoveryPlan.blocked) `
        -Message "A valid empty ReadOnly remainder blocked recovery dry-run."
    Assert-TestCondition `
        -Condition ($readOnlyRecoveryPlan.recoveryTargetCount -eq 1) `
        -Message "Recovery did not isolate the one empty ReadOnly root."
    Assert-TestCondition `
        -Condition ($readOnlyRecoveryPlan.recoveryTargetBytes -eq 0) `
        -Message "The empty ReadOnly recovery root reported content bytes."
    $emptyRecoveryEntry = @($readOnlyRecoveryPlan.entries |
        Where-Object disposition -ceq (
            "RECOVERY_DELETE_EMPTY_PARTIAL_ROOT_REQUIRES_APPROVAL"))[0]
    Assert-TestCondition `
        -Condition ($null -ne $emptyRecoveryEntry) `
        -Message "The empty ReadOnly remainder was not classified for approval."
    Assert-TestCondition `
        -Condition (($emptyRecoveryEntry.currentIdentity.attributes -band
                [uint32][System.IO.FileAttributes]::ReadOnly) -ne 0) `
        -Message "The recovery fixture lost its ReadOnly root attribute."
    Invoke-ExpectedFailure -MessagePart "approval does not match" -Action {
        Invoke-LocalRetentionRecovery `
            -RepositoryRoot $repositoryRoot `
            -TransactionId $readOnlyTransactionId `
            -ApplyRecovery `
            -ApprovedRecoveryPlanSha256 ("0" * 64) `
            -ApprovedRecoveryJournalSha256 $readOnlyRecoveryPlan.journalSha256 `
            -ApprovedGitStatusSha256 $readOnlyRecoveryPlan.gitStatusSha256 `
            -ApprovedWorktreeIdentitySha256 (
                $readOnlyRecoveryPlan.worktreeIdentitySha256) | Out-Null
    }
    $recoveryResult = Invoke-TestRecoveryApply -Plan $readOnlyRecoveryPlan
    Assert-TestCondition `
        -Condition ($recoveryResult.deletedTargetCount -eq 1) `
        -Message "Recovery did not delete exactly the approved empty root."
    Assert-TestCondition `
        -Condition ($recoveryResult.logicalBytesRemoved -eq 0) `
        -Message "Empty-root recovery reported content deletion."
    Assert-TestCondition `
        -Condition (-not (Test-Path -LiteralPath (
                    $readOnlyTransactions[0].FullName))) `
        -Message "Completed recovery left its active transaction root."
    $recoveryHistoryText = [System.IO.File]::ReadAllText(
        $recoveryResult.transactionRecordPath,
        $utf8)
    Assert-TestCondition `
        -Condition ($recoveryHistoryText.Contains(
                '"event":"RECOVERY_COMPLETED"')) `
        -Message "Completed recovery omitted its durable terminal event."
    Invoke-ExpectedFailure -MessagePart "already consumed" -Action {
        Assert-LocalRetentionPlanNotConsumed `
            -ResolvedRepositoryRoot $repositoryRoot `
            -PlanSha256 $readOnlyPartialPlan.planSha256
    }
    Invoke-ExpectedFailure -MessagePart "exactly the named" -Action {
        Get-LocalRetentionRecoveryPlan `
            -RepositoryRoot $repositoryRoot `
            -TransactionId $readOnlyTransactionId | Out-Null
    }

    Write-TestFile -Path (Join-Path $applicationBin "generated.dll") -Content "first`n"
    Write-TestFile -Path (Join-Path $domainBin "generated.dll") -Content "second`n"
    $partialPlan = Get-LocalArtefactRetentionPlan -RepositoryRoot $repositoryRoot
    Assert-TestCondition -Condition ($partialPlan.deletionCandidateCount -eq 2) `
        -Message "The partial-failure fixture did not create two candidates."
    $script:LocalRetentionRemovalCount = 0
    $originalStagedRemoval = ${function:Remove-LocalRetentionStagedTarget}
    function Remove-LocalRetentionStagedTarget {
        [CmdletBinding()]
        param(
            [Parameter(Mandatory)][string]$Path,
            [Parameter(Mandatory)][string]$ExpectedStructuralTreeSha256,
            [Parameter(Mandatory)][long]$ExpectedByteLength
        )

        $script:LocalRetentionRemovalCount++
        if ($script:LocalRetentionRemovalCount -eq 2) {
            Write-TestFile `
                -Path (Join-Path $Path "late-writer.bin") `
                -Content "unapproved late bytes`n"
        }
        & $originalStagedRemoval `
            -Path $Path `
            -ExpectedStructuralTreeSha256 $ExpectedStructuralTreeSha256 `
            -ExpectedByteLength $ExpectedByteLength
    }
    Invoke-ExpectedFailure -MessagePart "partial material deletion" -Action {
        Invoke-TestApply -Plan $partialPlan | Out-Null
    }
    $transactionRoot = Join-Path $repositoryRoot (
        "artifacts-local\retention-transactions")
    $activeTransactions = @(Get-ChildItem -LiteralPath $transactionRoot -Directory -Force)
    Assert-TestCondition -Condition ($activeTransactions.Count -eq 1) `
        -Message "A partial failure did not leave exactly one recoverable transaction."
    $journal = Join-Path $activeTransactions[0].FullName "transaction.jsonl"
    $journalText = [System.IO.File]::ReadAllText($journal, $utf8)
    Assert-TestCondition -Condition ($journalText.Contains('"event":"DELETED"')) `
        -Message "The partial transaction omitted the completed deletion event."
    Assert-TestCondition `
        -Condition ($journalText.Contains('"event":"PARTIAL_DELETE_FAILURE"')) `
        -Message "The partial transaction omitted its failure event."
    Assert-TestCondition -Condition ($journalText.Contains('"state":"MEASURED_PARTIAL"')) `
        -Message "The partial transaction did not record the measurable remainder."
    $lateWriter = Get-ChildItem `
        -LiteralPath $activeTransactions[0].FullName `
        -Recurse `
        -File `
        -Filter "late-writer.bin"
    Assert-TestCondition -Condition (@($lateWriter).Count -eq 1) `
        -Message "Unapproved late bytes were deleted from the staged transaction."
    $blockedAfterPartial = Get-LocalArtefactRetentionPlan -RepositoryRoot $repositoryRoot
    Assert-TestCondition -Condition $blockedAfterPartial.blocked `
        -Message "An incomplete transaction did not block the next dry-run."
    $lateWriterRecoveryPlan = Get-LocalRetentionRecoveryPlan `
        -RepositoryRoot $repositoryRoot `
        -TransactionId $activeTransactions[0].Name
    Assert-TestCondition -Condition $lateWriterRecoveryPlan.blocked `
        -Message "Recovery dry-run accepted a remainder with unapproved data."
    Assert-TestCondition `
        -Condition (@($lateWriterRecoveryPlan.entries |
                Where-Object disposition -ceq "PRESERVE_UNCERTAIN").Count -eq 1) `
        -Message "Recovery did not preserve a late-writer remainder as uncertain."
    Assert-TestCondition `
        -Condition (@($lateWriterRecoveryPlan.boundaryBlockingReasons |
                Where-Object { $_.Contains(
                        "contains unapproved data",
                        [System.StringComparison]::Ordinal) }).Count -eq 1) `
        -Message "Recovery did not explain the unapproved late-writer boundary."

    Write-Output "All local artefact retention policy tests passed."
}
finally {
    foreach ($linkPath in $createdLinks) {
        if (Test-Path -LiteralPath $linkPath) {
            $linkItem = Get-Item -LiteralPath $linkPath -Force
            if (($linkItem.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
                Remove-Item -LiteralPath $linkPath -Force
            }
        }
    }

    if (Test-Path -LiteralPath $temporaryRoot) {
        $resolvedForRemoval = [System.IO.Path]::GetFullPath($temporaryRoot)
        if (-not $resolvedForRemoval.StartsWith(
                $expectedPrefix,
                [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "The synthetic test root failed its removal boundary."
        }
        Remove-Item -LiteralPath $resolvedForRemoval -Recurse -Force
    }
}
