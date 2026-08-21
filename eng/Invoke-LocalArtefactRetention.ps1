# Purpose: Owns fail-closed repository-local generated-artefact retention; it is dry-run by default and has no product-data, lifecycle, provider or external authority.

[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot),
    [switch]$Apply,
    [ValidatePattern('^[0-9a-f]{64}$')]
    [string]$ApprovedPlanSha256,
    [ValidatePattern('^[0-9a-f]{64}$')]
    [string]$ApprovedGitStatusSha256,
    [ValidatePattern('^[0-9a-f]{64}$')]
    [string]$ApprovedWorktreeIdentitySha256,
    [ValidatePattern('^[0-9a-f]{64}$')]
    [string]$ApprovedLegacyOwnershipAttestationSha256,
    [ValidatePattern('^[0-9a-f]{16}-[0-9a-f]{32}$')]
    [string]$RecoveryTransactionId,
    [switch]$ApplyRecovery,
    [ValidatePattern('^[0-9a-f]{64}$')]
    [string]$ApprovedRecoveryPlanSha256,
    [ValidatePattern('^[0-9a-f]{64}$')]
    [string]$ApprovedRecoveryJournalSha256
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$script:LocalRetentionUtf8 = [System.Text.UTF8Encoding]::new($false)
$script:LocalRetentionStrictUtf8 = [System.Text.UTF8Encoding]::new($false, $true)
$script:LocalRetentionPathComparison = if ([System.OperatingSystem]::IsWindows()) {
    [System.StringComparison]::OrdinalIgnoreCase
}
else {
    [System.StringComparison]::Ordinal
}
$script:LocalRetentionSevenDays = [System.TimeSpan]::FromDays(7)
$script:LocalRetentionExecutorPath = $MyInvocation.MyCommand.Path
$script:LocalRetentionMarkerName = ".rag-challenge-retention-owner.json"
$script:LocalRetentionOwner = "eng/Invoke-LocalArtefactRetention.ps1"

if ([System.OperatingSystem]::IsWindows()) {
    if ($null -ne ("RagChallenge.LocalRetention.NativePathHandle" -as [type])) {
        throw "A preloaded local-retention native type is not trusted; start a fresh PowerShell process."
    }
    Add-Type -TypeDefinition @'
// Purpose: Provides Windows handle-bound identity, locking and deletion for the local retention executor; it never reads file content or deletes by pathname.
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace RagChallenge.LocalRetention
{
    public sealed class NativePathHandle : IDisposable
    {
        private const uint DeleteAccess = 0x00010000;
        private const uint FileListDirectory = 0x00000001;
        private const uint FileReadAttributes = 0x00000080;
        private const uint FileShareRead = 0x00000001;
        private const uint FileShareWrite = 0x00000002;
        private const uint FileShareDelete = 0x00000004;
        private const uint OpenExisting = 3;
        private const uint FileFlagOpenReparsePoint = 0x00200000;
        private const uint FileFlagBackupSemantics = 0x02000000;
        private const uint FileAttributeReparsePoint = 0x00000400;
        private const int FileBasicInfo = 0;
        private const int FileStandardInfo = 1;
        private const int FileAttributeTagInfo = 9;
        private const int FileIdInfo = 18;
        private const int FileStreamInfo = 7;
        private const int FileDispositionInfoEx = 21;
        private const uint FileDispositionFlagDelete = 0x00000001;
        private const uint FileDispositionFlagPosixSemantics = 0x00000002;
        private const uint FileDispositionFlagIgnoreReadonlyAttribute = 0x00000010;
        private const int ErrorNoMoreFiles = 18;
        private const int ErrorHandleEof = 38;
        private const int ErrorInsufficientBuffer = 122;
        private const int ErrorMoreData = 234;

        [StructLayout(LayoutKind.Sequential)]
        private struct FileBasicInformation
        {
            public long CreationTime;
            public long LastAccessTime;
            public long LastWriteTime;
            public long ChangeTime;
            public uint FileAttributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FileStandardInformation
        {
            public long AllocationSize;
            public long EndOfFile;
            public uint NumberOfLinks;
            [MarshalAs(UnmanagedType.U1)] public bool DeletePending;
            [MarshalAs(UnmanagedType.U1)] public bool Directory;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FileAttributeTagInformation
        {
            public uint FileAttributes;
            public uint ReparseTag;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FileIdInformation
        {
            public ulong VolumeSerialNumber;
            public ulong FileIdLow;
            public ulong FileIdHigh;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FileDispositionInformationEx
        {
            public uint Flags;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct Win32FindStreamData
        {
            public long StreamSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 296)]
            public string StreamName;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFileW(
            string fileName,
            uint desiredAccess,
            uint shareMode,
            IntPtr securityAttributes,
            uint creationDisposition,
            uint flagsAndAttributes,
            IntPtr templateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetFileInformationByHandleEx(
            SafeFileHandle file,
            int informationClass,
            out FileBasicInformation information,
            uint bufferSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetFileInformationByHandleEx(
            SafeFileHandle file,
            int informationClass,
            out FileStandardInformation information,
            uint bufferSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetFileInformationByHandleEx(
            SafeFileHandle file,
            int informationClass,
            out FileAttributeTagInformation information,
            uint bufferSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetFileInformationByHandleEx(
            SafeFileHandle file,
            int informationClass,
            out FileIdInformation information,
            uint bufferSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetFileInformationByHandleEx(
            SafeFileHandle file,
            int informationClass,
            IntPtr information,
            uint bufferSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetFileInformationByHandle(
            SafeFileHandle file,
            int informationClass,
            ref FileDispositionInformationEx information,
            uint bufferSize);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr FindFirstStreamW(
            string fileName,
            int informationLevel,
            out Win32FindStreamData findStreamData,
            uint flags);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FindNextStreamW(
            IntPtr findStream,
            out Win32FindStreamData findStreamData);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FindClose(IntPtr findFile);

        private readonly SafeFileHandle handle;
        private readonly bool directory;
        private bool disposed;

        private NativePathHandle(SafeFileHandle handle, bool directory)
        {
            this.handle = handle;
            this.directory = directory;
            Refresh();
        }

        public string VolumeSerialNumberHex { get; private set; }
        public string FileIdHex { get; private set; }
        public long CreationTimeTicks { get; private set; }
        public long LastWriteTimeTicks { get; private set; }
        public long ChangeTimeTicks { get; private set; }
        public long Length { get; private set; }
        public uint Attributes { get; private set; }
        public uint ReparseTag { get; private set; }
        public bool IsDirectory { get { return directory; } }
        public string IdentityToken { get { return VolumeSerialNumberHex + ":" + FileIdHex; } }

        public static NativePathHandle OpenIdentity(string path, bool directory)
        {
            return Open(
                path,
                directory,
                FileReadAttributes | (directory ? FileListDirectory : 0),
                FileShareRead | FileShareWrite | FileShareDelete);
        }

        public static NativePathHandle OpenDeletion(string path, bool directory)
        {
            return Open(
                path,
                directory,
                DeleteAccess | FileReadAttributes | (directory ? FileListDirectory : 0),
                FileShareRead);
        }

        public static NativePathHandle OpenDirectoryGuard(string path)
        {
            return Open(
                path,
                true,
                DeleteAccess | FileReadAttributes | FileListDirectory,
                FileShareRead | FileShareWrite);
        }

        public static NativePathHandle OpenWorktreeGuard(string path, bool directory)
        {
            return Open(
                path,
                directory,
                FileReadAttributes | (directory ? FileListDirectory : 0),
                FileShareRead);
        }

        public static void EnsureNoNamedDataStreams(string path)
        {
            Win32FindStreamData data;
            IntPtr find = FindFirstStreamW(path, 0, out data, 0);
            if (find == new IntPtr(-1))
            {
                int error = Marshal.GetLastWin32Error();
                if (error == ErrorHandleEof)
                {
                    return;
                }
                throw new Win32Exception(
                    error,
                    "Unable to enumerate retention stream metadata.");
            }

            try
            {
                while (true)
                {
                    if (!String.Equals(
                            data.StreamName,
                            "::$DATA",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            "A retention target contains a named alternate data stream.");
                    }
                    if (FindNextStreamW(find, out data))
                    {
                        continue;
                    }
                    int error = Marshal.GetLastWin32Error();
                    if (error == ErrorNoMoreFiles || error == ErrorHandleEof)
                    {
                        break;
                    }
                    throw new Win32Exception(
                        error,
                        "Unable to enumerate retention stream metadata.");
                }
            }
            finally
            {
                FindClose(find);
            }
        }

        private static NativePathHandle Open(
            string path,
            bool directory,
            uint desiredAccess,
            uint shareMode)
        {
            uint flags = FileFlagOpenReparsePoint |
                (directory ? FileFlagBackupSemantics : 0);
            SafeFileHandle handle = CreateFileW(
                path,
                desiredAccess,
                shareMode,
                IntPtr.Zero,
                OpenExisting,
                flags,
                IntPtr.Zero);
            if (handle == null || handle.IsInvalid)
            {
                int error = Marshal.GetLastWin32Error();
                if (handle != null)
                {
                    handle.Dispose();
                }
                throw new Win32Exception(error, "Unable to acquire a retention path handle.");
            }

            try
            {
                return new NativePathHandle(handle, directory);
            }
            catch
            {
                handle.Dispose();
                throw;
            }
        }

        public void Refresh()
        {
            ThrowIfDisposed();
            FileBasicInformation basic;
            FileStandardInformation standard;
            FileAttributeTagInformation tag;
            FileIdInformation id;
            if (!GetFileInformationByHandleEx(
                    handle,
                    FileBasicInfo,
                    out basic,
                    (uint)Marshal.SizeOf<FileBasicInformation>()) ||
                !GetFileInformationByHandleEx(
                    handle,
                    FileStandardInfo,
                    out standard,
                    (uint)Marshal.SizeOf<FileStandardInformation>()) ||
                !GetFileInformationByHandleEx(
                    handle,
                    FileAttributeTagInfo,
                    out tag,
                    (uint)Marshal.SizeOf<FileAttributeTagInformation>()) ||
                !GetFileInformationByHandleEx(
                    handle,
                    FileIdInfo,
                    out id,
                    (uint)Marshal.SizeOf<FileIdInformation>()))
            {
                throw new Win32Exception(
                    Marshal.GetLastWin32Error(),
                    "Unable to establish retention path identity.");
            }

            if ((tag.FileAttributes & FileAttributeReparsePoint) != 0)
            {
                throw new InvalidOperationException(
                    "A retention path handle resolved to a reparse point.");
            }
            if (standard.Directory != directory)
            {
                throw new InvalidOperationException(
                    "A retention path handle resolved to the wrong item type.");
            }

            VolumeSerialNumberHex = id.VolumeSerialNumber.ToString("x16");
            FileIdHex = id.FileIdLow.ToString("x16") + id.FileIdHigh.ToString("x16");
            CreationTimeTicks = basic.CreationTime;
            LastWriteTimeTicks = basic.LastWriteTime;
            ChangeTimeTicks = basic.ChangeTime;
            Length = standard.Directory ? 0 : standard.EndOfFile;
            Attributes = tag.FileAttributes;
            ReparseTag = tag.ReparseTag;
        }

        public void MarkDelete()
        {
            ThrowIfDisposed();
            SetDisposition(
                FileDispositionFlagDelete |
                FileDispositionFlagPosixSemantics |
                FileDispositionFlagIgnoreReadonlyAttribute);
        }

        public void ArmDeletePending()
        {
            ThrowIfDisposed();
            SetDisposition(
                FileDispositionFlagDelete |
                FileDispositionFlagIgnoreReadonlyAttribute);
        }

        public void ClearDeletePending()
        {
            ThrowIfDisposed();
            SetDisposition(0);
        }

        private void SetDisposition(uint flags)
        {
            FileDispositionInformationEx disposition =
                new FileDispositionInformationEx { Flags = flags };
            if (!SetFileInformationByHandle(
                    handle,
                    FileDispositionInfoEx,
                    ref disposition,
                    (uint)Marshal.SizeOf<FileDispositionInformationEx>()))
            {
                throw new Win32Exception(
                    Marshal.GetLastWin32Error(),
                    "Handle-bound retention deletion failed.");
            }
        }

        public void EnsureNoNamedDataStreamsOnHandle()
        {
            ThrowIfDisposed();
            int bufferSize = 4096;
            while (bufferSize <= 16 * 1024 * 1024)
            {
                IntPtr buffer = Marshal.AllocHGlobal(bufferSize);
                try
                {
                    if (GetFileInformationByHandleEx(
                            handle,
                            FileStreamInfo,
                            buffer,
                            (uint)bufferSize))
                    {
                        ValidateStreamInformation(buffer, bufferSize);
                        return;
                    }
                    int error = Marshal.GetLastWin32Error();
                    if (error == ErrorHandleEof)
                    {
                        return;
                    }
                    if (error != ErrorInsufficientBuffer && error != ErrorMoreData)
                    {
                        throw new Win32Exception(
                            error,
                            "Unable to enumerate retention stream metadata by handle.");
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
                bufferSize *= 2;
            }
            throw new InvalidOperationException(
                "Retention stream metadata exceeded the bounded handle buffer.");
        }

        private static void ValidateStreamInformation(IntPtr buffer, int bufferSize)
        {
            int offset = 0;
            while (true)
            {
                if (offset < 0 || offset > bufferSize - 24)
                {
                    throw new InvalidOperationException(
                        "Retention stream metadata has an invalid boundary.");
                }
                int nextOffset = Marshal.ReadInt32(buffer, offset);
                int nameLength = Marshal.ReadInt32(buffer, offset + 4);
                if (nameLength < 0 || (nameLength % 2) != 0 ||
                    nameLength > bufferSize - offset - 24)
                {
                    throw new InvalidOperationException(
                        "Retention stream metadata has an invalid name boundary.");
                }
                string name = Marshal.PtrToStringUni(
                    IntPtr.Add(buffer, offset + 24),
                    nameLength / 2);
                if (!String.Equals(
                        name,
                        "::$DATA",
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "A retention target contains a named alternate data stream.");
                }
                if (nextOffset == 0)
                {
                    return;
                }
                if (nextOffset < 24 || nextOffset > bufferSize - offset - 24)
                {
                    throw new InvalidOperationException(
                        "Retention stream metadata has an invalid next-entry boundary.");
                }
                offset += nextOffset;
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed || handle.IsClosed || handle.IsInvalid)
            {
                throw new ObjectDisposedException(nameof(NativePathHandle));
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            disposed = true;
            handle.Dispose();
        }
    }
}
'@
}

$localRetentionGitCommand = Get-Command -Name git -CommandType Application -ErrorAction Stop |
    Select-Object -First 1
$script:LocalRetentionGitPath = $localRetentionGitCommand.Source
$script:LocalRetentionGitGuard = $null
try {
    $script:LocalRetentionGitGuard =
        [RagChallenge.LocalRetention.NativePathHandle]::OpenWorktreeGuard(
            $script:LocalRetentionGitPath,
            $false)
    $script:LocalRetentionGitIdentityToken =
        $script:LocalRetentionGitGuard.IdentityToken
    $script:LocalRetentionGitSha256 = (Get-FileHash `
            -LiteralPath $script:LocalRetentionGitPath `
            -Algorithm SHA256).Hash.ToLowerInvariant()
    $script:LocalRetentionGitGuard.Refresh()
    if ($script:LocalRetentionGitGuard.IdentityToken -cne
        $script:LocalRetentionGitIdentityToken) {
        throw "The resolved Git executable changed identity during initialisation."
    }
}
catch {
    if ($null -ne $script:LocalRetentionGitGuard) {
        $script:LocalRetentionGitGuard.Dispose()
        $script:LocalRetentionGitGuard = $null
    }
    throw
}

function Get-LocalRetentionSha256Bytes {
    [CmdletBinding()]
    param([Parameter(Mandatory)][AllowEmptyCollection()][byte[]]$Bytes)

    return [System.Convert]::ToHexString(
        [System.Security.Cryptography.SHA256]::HashData($Bytes)).ToLowerInvariant()
}

function Get-LocalRetentionSha256 {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Value)

    return Get-LocalRetentionSha256Bytes -Bytes $script:LocalRetentionUtf8.GetBytes($Value)
}

function Get-LocalRetentionMarkerText {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Purpose,
        [Parameter(Mandatory)][string]$Owner,
        [Parameter(Mandatory)][string]$CanonicalRelativePath
    )

    foreach ($value in @($Purpose, $Owner, $CanonicalRelativePath)) {
        if ([string]::IsNullOrWhiteSpace($value) -or
            $value -notmatch '^[A-Za-z0-9._/-]+$') {
            throw "A retention ownership marker field is invalid."
        }
    }

    $marker = [ordered]@{
        schemaVersion = 1
        purpose = $Purpose
        owner = $Owner
        canonicalRelativePath = $CanonicalRelativePath
    }
    return ($marker | ConvertTo-Json -Compress) + "`n"
}

function New-LocalRetentionOwnedOutputRoot {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$OutputRoot,
        [Parameter(Mandatory)][string]$RepositoryRoot,
        [Parameter(Mandatory)][string]$Purpose,
        [Parameter(Mandatory)][string]$Owner,
        [Parameter(Mandatory)][string]$CanonicalRelativePath
    )

    $resolvedRepositoryRoot = [System.IO.Path]::GetFullPath($RepositoryRoot).TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar)
    $resolvedOutputRoot = [System.IO.Path]::GetFullPath($OutputRoot)
    $repositoryPrefix = $resolvedRepositoryRoot + [System.IO.Path]::DirectorySeparatorChar
    if (-not $resolvedOutputRoot.StartsWith(
            $repositoryPrefix,
            $script:LocalRetentionPathComparison)) {
        throw "The retention-owned output root escaped the repository."
    }

    Assert-LocalRetentionExistingComponentsAreSafe `
        -RepositoryRoot $resolvedRepositoryRoot `
        -Path $resolvedOutputRoot
    [System.IO.Directory]::CreateDirectory($resolvedOutputRoot) | Out-Null
    $markerPath = Join-Path $resolvedOutputRoot $script:LocalRetentionMarkerName
    $markerText = Get-LocalRetentionMarkerText `
        -Purpose $Purpose `
        -Owner $Owner `
        -CanonicalRelativePath $CanonicalRelativePath
    [System.IO.File]::WriteAllText($markerPath, $markerText, $script:LocalRetentionUtf8)
}

function Assert-LocalRetentionOwnershipMarker {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$OutputRoot,
        [Parameter(Mandatory)][string]$Purpose,
        [Parameter(Mandatory)][string]$Owner,
        [Parameter(Mandatory)][string]$CanonicalRelativePath
    )

    $markerPath = Join-Path $OutputRoot $script:LocalRetentionMarkerName
    $marker = Get-Item -LiteralPath $markerPath -Force -ErrorAction SilentlyContinue
    if ($null -eq $marker -or $marker.PSIsContainer -or
        ($marker.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0 -or
        $marker.Length -gt 4096) {
        throw "The retention ownership marker is missing or invalid."
    }

    $expected = Get-LocalRetentionMarkerText `
        -Purpose $Purpose `
        -Owner $Owner `
        -CanonicalRelativePath $CanonicalRelativePath
    $actual = [System.IO.File]::ReadAllText($markerPath, $script:LocalRetentionStrictUtf8)
    if ($actual -cne $expected) {
        throw "The retention ownership marker is missing or invalid."
    }
}

function Assert-LocalRetentionExistingComponentsAreSafe {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$RepositoryRoot,
        [Parameter(Mandatory)][string]$Path
    )

    $root = [System.IO.Path]::GetFullPath($RepositoryRoot).TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar)
    $resolved = [System.IO.Path]::GetFullPath($Path)
    $prefix = $root + [System.IO.Path]::DirectorySeparatorChar
    if (-not [string]::Equals($resolved, $root, $script:LocalRetentionPathComparison) -and
        -not $resolved.StartsWith($prefix, $script:LocalRetentionPathComparison)) {
        throw "The retention path escaped the repository root."
    }

    $rootItem = Get-Item -LiteralPath $root -Force -ErrorAction Stop
    if (($rootItem.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw "The repository root is a reparse point."
    }

    $relative = [System.IO.Path]::GetRelativePath($root, $resolved)
    if ($relative -eq ".") {
        return
    }

    $current = $root
    foreach ($segment in $relative.Split(
            [char[]]@(
                [System.IO.Path]::DirectorySeparatorChar,
                [System.IO.Path]::AltDirectorySeparatorChar),
            [System.StringSplitOptions]::RemoveEmptyEntries)) {
        $current = Join-Path $current $segment
        $item = Get-Item -LiteralPath $current -Force -ErrorAction SilentlyContinue
        if ($null -ne $item -and
            ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
            throw "The retention path contains an unsafe component."
        }
    }
}

function Assert-LocalRetentionTreeIsSafe {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Root)

    $rootItem = Get-Item -LiteralPath $Root -Force -ErrorAction Stop
    if (-not $rootItem.PSIsContainer -or
        ($rootItem.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw "The retention target root is not a safe directory."
    }

    $queue = [System.Collections.Generic.Queue[System.IO.DirectoryInfo]]::new()
    $queue.Enqueue([System.IO.DirectoryInfo]$rootItem)
    while ($queue.Count -gt 0) {
        $directory = $queue.Dequeue()
        foreach ($child in $directory.GetFileSystemInfos()) {
            if (($child.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
                throw "The retention target contains a reparse point."
            }
            if (($child.Attributes -band [System.IO.FileAttributes]::Directory) -ne 0) {
                $queue.Enqueue([System.IO.DirectoryInfo]$child)
            }
        }
    }
}

function Get-LocalRetentionCandidates {
    return @(
        [pscustomobject]@{ RelativePath = "src/RagChallenge.Application/bin"; Reason = "Reproducible .NET build output with no dependent gate"; Recoverability = "Regenerable from the tracked project and locked dependencies"; RetentionClass = "LegacyGeneratedRoot"; OwnershipKind = "OwnerApprovedExactLegacyRoot" },
        [pscustomobject]@{ RelativePath = "src/RagChallenge.Dashboard.Web/dist"; Reason = "Reproducible Dashboard distribution output with no dependent gate"; Recoverability = "Regenerable from tracked sources and the active lockfile"; RetentionClass = "LegacyGeneratedRoot"; OwnershipKind = "OwnerApprovedExactLegacyRoot" },
        [pscustomobject]@{ RelativePath = "src/RagChallenge.Domain/bin"; Reason = "Reproducible .NET build output with no dependent gate"; Recoverability = "Regenerable from the tracked project and locked dependencies"; RetentionClass = "LegacyGeneratedRoot"; OwnershipKind = "OwnerApprovedExactLegacyRoot" },
        [pscustomobject]@{ RelativePath = "src/RagChallenge.Infrastructure/bin"; Reason = "Reproducible .NET build output with no dependent gate"; Recoverability = "Regenerable from the tracked project and locked dependencies"; RetentionClass = "LegacyGeneratedRoot"; OwnershipKind = "OwnerApprovedExactLegacyRoot" },
        [pscustomobject]@{ RelativePath = "src/RagChallenge.Server.Api/bin"; Reason = "Reproducible .NET build output with no dependent gate"; Recoverability = "Regenerable from the tracked project and locked dependencies"; RetentionClass = "LegacyGeneratedRoot"; OwnershipKind = "OwnerApprovedExactLegacyRoot" },
        [pscustomobject]@{ RelativePath = "TestResults"; Reason = "Test and coverage evidence subject to a seven-day window and canonical gate release"; Recoverability = "Regenerable only as a new execution, never as the same evidence"; RetentionClass = "TestEvidence"; OwnershipKind = "GateReleaseManifestRequired" },
        [pscustomobject]@{ RelativePath = "tests/RagChallenge.Architecture.Tests/bin"; Reason = "Reproducible .NET test build output with no dependent gate"; Recoverability = "Regenerable from the tracked project and locked dependencies"; RetentionClass = "LegacyGeneratedRoot"; OwnershipKind = "OwnerApprovedExactLegacyRoot" },
        [pscustomobject]@{ RelativePath = "tests/RagChallenge.IntegrationTests/bin"; Reason = "Reproducible .NET test build output with no dependent gate"; Recoverability = "Regenerable from the tracked project and locked dependencies"; RetentionClass = "LegacyGeneratedRoot"; OwnershipKind = "OwnerApprovedExactLegacyRoot" },
        [pscustomobject]@{ RelativePath = "tests/RagChallenge.UnitTests/bin"; Reason = "Reproducible .NET test build output with no dependent gate"; Recoverability = "Regenerable from the tracked project and locked dependencies"; RetentionClass = "LegacyGeneratedRoot"; OwnershipKind = "OwnerApprovedExactLegacyRoot" },
        [pscustomobject]@{ RelativePath = "tools/ai-orchestrator/dist"; Reason = "Reproducible orchestrator distribution output with no dependent gate"; Recoverability = "Regenerable from tracked sources and the active lockfile"; RetentionClass = "LegacyGeneratedRoot"; OwnershipKind = "OwnerApprovedExactLegacyRoot" }
    )
}

function Get-LocalRetentionProtectedPaths {
    return @(
        [pscustomobject]@{ RelativePath = ".git"; Disposition = "PRESERVE_PROTECTED"; Reason = "Git identity and tracked history" },
        [pscustomobject]@{ RelativePath = ".env.local"; Disposition = "PRESERVE_SECRET_NO_READ"; Reason = "Local configuration and secret boundary; content is never read" },
        [pscustomobject]@{ RelativePath = "corpus"; Disposition = "PRESERVE_PROTECTED"; Reason = "Authorised source corpus" },
        [pscustomobject]@{ RelativePath = "reference-materials"; Disposition = "PRESERVE_PROTECTED"; Reason = "Local-only reference material" },
        [pscustomobject]@{ RelativePath = "artifacts-local"; Disposition = "PRESERVE_PROTECTED"; Reason = "Product data, recovery evidence, active and rollback identities, diagnostics and caches" },
        [pscustomobject]@{ RelativePath = "src/RagChallenge.Application/obj"; Disposition = "PRESERVE_ACTIVE_WORK"; Reason = "Active .NET restore/build state" },
        [pscustomobject]@{ RelativePath = "src/RagChallenge.Dashboard.Web/node_modules"; Disposition = "PRESERVE_ACTIVE_WORK"; Reason = "Active lockfile-bound dependency tree" },
        [pscustomobject]@{ RelativePath = "src/RagChallenge.Domain/obj"; Disposition = "PRESERVE_ACTIVE_WORK"; Reason = "Active .NET restore/build state" },
        [pscustomobject]@{ RelativePath = "src/RagChallenge.Infrastructure/obj"; Disposition = "PRESERVE_ACTIVE_WORK"; Reason = "Active .NET restore/build state" },
        [pscustomobject]@{ RelativePath = "src/RagChallenge.Server.Api/obj"; Disposition = "PRESERVE_ACTIVE_WORK"; Reason = "Active .NET restore/build state" },
        [pscustomobject]@{ RelativePath = "tests/RagChallenge.Architecture.Tests/obj"; Disposition = "PRESERVE_ACTIVE_WORK"; Reason = "Active .NET test restore/build state" },
        [pscustomobject]@{ RelativePath = "tests/RagChallenge.IntegrationTests/obj"; Disposition = "PRESERVE_ACTIVE_WORK"; Reason = "Active .NET test restore/build state" },
        [pscustomobject]@{ RelativePath = "tests/RagChallenge.UnitTests/obj"; Disposition = "PRESERVE_ACTIVE_WORK"; Reason = "Active .NET test restore/build state" },
        [pscustomobject]@{ RelativePath = "tools/ai-orchestrator/node_modules"; Disposition = "PRESERVE_ACTIVE_WORK"; Reason = "Active lockfile-bound dependency tree" }
    )
}

function Get-LocalRetentionCoverage {
    return @(
        [pscustomobject]@{ ClassId = "git-tracked-wip"; PathBoundary = "."; Disposition = "PRESERVE_PROTECTED"; Condition = "Never automatically deleted" },
        [pscustomobject]@{ ClassId = "secrets-and-configuration"; PathBoundary = ".env.local and sensitive names"; Disposition = "PRESERVE_SECRET_NO_READ"; Condition = "Never read or automatically deleted" },
        [pscustomobject]@{ ClassId = "corpus-and-reference-materials"; PathBoundary = "corpus; reference-materials"; Disposition = "PRESERVE_PROTECTED"; Condition = "Never automatically deleted" },
        [pscustomobject]@{ ClassId = "active-stores-source-intake-rights-and-human-freezes"; PathBoundary = "artifacts-local"; Disposition = "PRESERVE_PROTECTED"; Condition = "Product reachability policy remains authoritative" },
        [pscustomobject]@{ ClassId = "current-oci-candidate-and-validated-rollback"; PathBoundary = "artifacts-local"; Disposition = "PRESERVE_IDENTITY_BOUND"; Condition = "Both identities must remain preserved" },
        [pscustomobject]@{ ClassId = "arm64-and-other-active-caches"; PathBoundary = "obj; node_modules; artifacts-local"; Disposition = "PRESERVE_ACTIVE_CACHE"; Condition = "Preserved until lockfile and RID obsolescence are proven" },
        [pscustomobject]@{ ClassId = "test-and-coverage-evidence"; PathBoundary = "TestResults"; Disposition = "MANIFEST_REQUIRED"; Condition = "At least seven days and canonical gate release are both required" },
        [pscustomobject]@{ ClassId = "failure-diagnostics"; PathBoundary = "artifacts-local"; Disposition = "MANIFEST_REQUIRED"; Condition = "At least seven days and a closed incident are both required" },
        [pscustomobject]@{ ClassId = "empty-temporary-directories"; PathBoundary = "Exact marker-owned path only"; Disposition = "MANIFEST_REQUIRED"; Condition = "Literal path, verified empty tree and valid ownership marker" },
        [pscustomobject]@{ ClassId = "superseded-generations"; PathBoundary = "artifacts-local"; Disposition = "MANIFEST_REQUIRED"; Condition = "Current and rollback excluded; identity and result preserved canonically" },
        [pscustomobject]@{ ClassId = "extracted-content-with-validated-archive"; PathBoundary = "Exact manifest-bound path only"; Disposition = "MANIFEST_REQUIRED"; Condition = "Archive digest and sole-required-copy disposition must be proven" },
        [pscustomobject]@{ ClassId = "unknown-or-unclassified-data"; PathBoundary = "."; Disposition = "PRESERVE_UNCERTAIN"; Condition = "Any uncertainty blocks Apply" }
    )
}

function Assert-LocalRetentionGitExecutableIdentity {
    [CmdletBinding()]
    param()

    if ($null -eq $script:LocalRetentionGitGuard) {
        throw "The resolved Git executable is not protected by a native handle."
    }
    $script:LocalRetentionGitGuard.Refresh()
    if ($script:LocalRetentionGitGuard.IdentityToken -cne
        $script:LocalRetentionGitIdentityToken) {
        throw "The resolved Git executable changed filesystem identity."
    }
    $current = [RagChallenge.LocalRetention.NativePathHandle]::OpenIdentity(
        $script:LocalRetentionGitPath,
        $false)
    try {
        if ($current.IdentityToken -cne $script:LocalRetentionGitIdentityToken) {
            throw "The resolved Git executable path changed filesystem identity."
        }
    }
    finally {
        $current.Dispose()
    }
    $currentSha256 = (Get-FileHash `
            -LiteralPath $script:LocalRetentionGitPath `
            -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($currentSha256 -cne $script:LocalRetentionGitSha256) {
        throw "The resolved Git executable changed bytes."
    }
    return $currentSha256
}

function Invoke-LocalRetentionGitBytes {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$ResolvedRepositoryRoot,
        [Parameter(Mandatory)][string[]]$Arguments,
        [byte[]]$StandardInputBytes,
        [switch]$OmitLiteralPathspecs,
        [int[]]$AllowedExitCodes = @(0)
    )

    $null = Assert-LocalRetentionGitExecutableIdentity
    $gitDirectory = Join-Path $ResolvedRepositoryRoot ".git"
    $nullDevice = if ([System.OperatingSystem]::IsWindows()) { "NUL" } else { "/dev/null" }
    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $script:LocalRetentionGitPath
    $startInfo.WorkingDirectory = $ResolvedRepositoryRoot
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.RedirectStandardInput = $PSBoundParameters.ContainsKey(
        "StandardInputBytes")
    $startInfo.CreateNoWindow = $true
    foreach ($environmentName in @($startInfo.Environment.Keys)) {
        if ($environmentName.StartsWith(
                "GIT_",
                [System.StringComparison]::OrdinalIgnoreCase)) {
            [void]$startInfo.Environment.Remove($environmentName)
        }
    }
    $startInfo.Environment["GIT_CONFIG_NOSYSTEM"] = "1"
    $startInfo.Environment["GIT_CONFIG_GLOBAL"] = $nullDevice
    $startInfo.Environment["GIT_ATTR_NOSYSTEM"] = "1"
    $startInfo.Environment["GIT_OPTIONAL_LOCKS"] = "0"
    $startInfo.Environment["GIT_TERMINAL_PROMPT"] = "0"
    $startInfo.Environment["GCM_INTERACTIVE"] = "Never"
    $globalArguments = [System.Collections.Generic.List[string]]::new()
    foreach ($globalArgument in @(
            "--no-optional-locks",
            "--git-dir=$gitDirectory",
            "--work-tree=$ResolvedRepositoryRoot",
            "-c",
            "core.fsmonitor=false",
            "-c",
            "core.untrackedCache=false",
            "-c",
            "core.hooksPath=$nullDevice",
            "-c",
            "core.excludesFile=$nullDevice")) {
        $globalArguments.Add($globalArgument)
    }
    if (-not $OmitLiteralPathspecs) {
        $globalArguments.Insert(3, "--literal-pathspecs")
    }
    foreach ($globalArgument in $globalArguments) {
        $startInfo.ArgumentList.Add($globalArgument)
    }
    foreach ($argument in $Arguments) {
        $startInfo.ArgumentList.Add($argument)
    }

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    $memory = [System.IO.MemoryStream]::new()
    try {
        if (-not $process.Start()) {
            throw "Git could not be started while establishing the retention boundary."
        }
        $copyTask = $process.StandardOutput.BaseStream.CopyToAsync($memory)
        $errorTask = $process.StandardError.ReadToEndAsync()
        if ($PSBoundParameters.ContainsKey("StandardInputBytes")) {
            $process.StandardInput.BaseStream.Write(
                $StandardInputBytes,
                0,
                $StandardInputBytes.Length)
            $process.StandardInput.Close()
        }
        $process.WaitForExit()
        $null = $copyTask.GetAwaiter().GetResult()
        $null = $errorTask.GetAwaiter().GetResult()
        if ($process.ExitCode -notin $AllowedExitCodes) {
            throw "Git failed while establishing the retention boundary."
        }
        $bytes = $memory.ToArray()
        return ,$bytes
    }
    finally {
        $memory.Dispose()
        $process.Dispose()
    }
}

function ConvertFrom-LocalRetentionNullSeparatedUtf8 {
    [CmdletBinding()]
    param([Parameter(Mandatory)][AllowEmptyCollection()][byte[]]$Bytes)

    if ($Bytes.Length -eq 0) {
        return @()
    }
    $text = $script:LocalRetentionStrictUtf8.GetString($Bytes)
    return @($text.Split([char]0, [System.StringSplitOptions]::RemoveEmptyEntries))
}

function Get-LocalRetentionGitText {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$ResolvedRepositoryRoot,
        [Parameter(Mandatory)][string[]]$Arguments
    )

    $bytes = Invoke-LocalRetentionGitBytes -ResolvedRepositoryRoot $ResolvedRepositoryRoot -Arguments $Arguments
    return $script:LocalRetentionStrictUtf8.GetString($bytes).Trim()
}

function ConvertFrom-LocalRetentionGitStatus {
    [CmdletBinding()]
    param([Parameter(Mandatory)][AllowEmptyCollection()][byte[]]$Bytes)

    $tokens = @(ConvertFrom-LocalRetentionNullSeparatedUtf8 -Bytes $Bytes)
    $records = [System.Collections.Generic.List[object]]::new()
    for ($index = 0; $index -lt $tokens.Count; $index++) {
        $token = $tokens[$index]
        if ($token.Length -lt 4 -or $token[2] -ne ' ') {
            throw "Git returned an invalid porcelain status record."
        }
        $status = $token.Substring(0, 2)
        $path = $token.Substring(3)
        $originalPath = $null
        if ($status.Contains('R') -or $status.Contains('C')) {
            $index++
            if ($index -ge $tokens.Count) {
                throw "Git returned an incomplete rename status record."
            }
            $originalPath = $tokens[$index]
        }
        $records.Add([pscustomobject]@{ Status = $status; Path = $path; OriginalPath = $originalPath })
    }
    return @($records)
}

function Test-LocalRetentionSensitiveRelativePath {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$RelativePath)

    foreach ($segment in $RelativePath.Replace('\', '/').Split('/')) {
        if ($segment -match '^(?i:[.]env)(?:[.]|$)' -or
            $segment -match '^(?i:appsettings)(?:[.].+)?[.]json$' -or
            $segment -match '^(?i:[.](?:npmrc|yarnrc))(?:[.]|$)' -or
            $segment -match '^(?i:(?:nuget|web)[.]config)$' -or
            $segment -match '^(?i:(?:launchsettings|settings))[.]json$' -or
            $segment -match '^(?i:(?:secrets?|credentials?))(?:[.]|$)' -or
            $segment -match '(?i:[.](?:key|pem|pfx|p12))$') {
            return $true
        }
    }
    return $false
}

function Test-LocalRetentionSecretOrLocalConfigurationPath {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$RelativePath)

    foreach ($segment in $RelativePath.Replace('\', '/').Split('/')) {
        if ($segment -match '^(?i:[.]env)(?:[.]|$)' -or
            $segment -match '^(?i:(?:secrets?|credentials?))(?:[.]|$)' -or
            $segment -match '(?i:[.](?:key|pem|pfx|p12))$') {
            return $true
        }
    }
    return $false
}

function Test-LocalRetentionConfigurationPath {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$RelativePath)

    foreach ($segment in $RelativePath.Replace('\', '/').Split('/')) {
        if ($segment -match '^(?i:appsettings)(?:[.].+)?[.]json$' -or
            $segment -match '^(?i:[.](?:npmrc|yarnrc))(?:[.]|$)' -or
            $segment -match '^(?i:(?:nuget|web)[.]config)$' -or
            $segment -match '^(?i:(?:launchsettings|settings))[.]json$') {
            return $true
        }
    }
    return $false
}

function Get-LocalRetentionGitState {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$ResolvedRepositoryRoot)

    $topLevel = Get-LocalRetentionGitText -ResolvedRepositoryRoot $ResolvedRepositoryRoot -Arguments @("rev-parse", "--show-toplevel")
    $resolvedTopLevel = [System.IO.Path]::GetFullPath($topLevel).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
    if (-not [string]::Equals($resolvedTopLevel, $ResolvedRepositoryRoot, $script:LocalRetentionPathComparison)) {
        throw "The retention root is not the exact Git repository root."
    }

    $head = Get-LocalRetentionGitText -ResolvedRepositoryRoot $ResolvedRepositoryRoot -Arguments @("rev-parse", "HEAD")
    if ($head -notmatch '^[0-9a-f]{40}$') {
        throw "The retention baseline is not a valid Git commit."
    }

    $statusBytes = Invoke-LocalRetentionGitBytes -ResolvedRepositoryRoot $ResolvedRepositoryRoot -Arguments @("status", "--porcelain=v1", "-z", "--untracked-files=all")
    $statusRecords = @(ConvertFrom-LocalRetentionGitStatus -Bytes $statusBytes)
    $blockingReasons = [System.Collections.Generic.List[string]]::new()
    $trackedPaths = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($record in $statusRecords) {
        if ($record.Status.Contains('U') -or $record.Status -in "AA", "DD") {
            $blockingReasons.Add("Unmerged Git work is uncertain and blocks retention.")
        }
        if ($record.Status -ne "??") {
            foreach ($path in @($record.Path, $record.OriginalPath)) {
                if ([string]::IsNullOrWhiteSpace($path)) { continue }
                [void]$trackedPaths.Add($path)
            }
        }
    }

    $cachedRawSha256 = $null
    $unstagedRawSha256 = $null
    if ($blockingReasons.Count -eq 0) {
        $cachedRawBytes = Invoke-LocalRetentionGitBytes -ResolvedRepositoryRoot $ResolvedRepositoryRoot -Arguments @("diff", "--cached", "--raw", "--no-abbrev", "--no-renames", "--no-ext-diff", "-z", "HEAD", "--")
        $unstagedRawBytes = Invoke-LocalRetentionGitBytes -ResolvedRepositoryRoot $ResolvedRepositoryRoot -Arguments @("diff", "--raw", "--no-abbrev", "--no-renames", "--no-ext-diff", "-z", "--")
        $cachedRawSha256 = Get-LocalRetentionSha256Bytes -Bytes $cachedRawBytes
        $unstagedRawSha256 = Get-LocalRetentionSha256Bytes -Bytes $unstagedRawBytes
    }

    $untrackedBytes = Invoke-LocalRetentionGitBytes -ResolvedRepositoryRoot $ResolvedRepositoryRoot -Arguments @("ls-files", "--others", "--exclude-standard", "-z")
    $untrackedPaths = @(ConvertFrom-LocalRetentionNullSeparatedUtf8 -Bytes $untrackedBytes)
    $wipRecords = [System.Collections.Generic.List[object]]::new()
    $wipLockPaths = [System.Collections.Generic.HashSet[string]]::new(
        $(if ([System.OperatingSystem]::IsWindows()) { [System.StringComparer]::OrdinalIgnoreCase } else { [System.StringComparer]::Ordinal }))
    $allWipPaths = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($path in $trackedPaths) { [void]$allWipPaths.Add($path) }
    foreach ($path in $untrackedPaths) { [void]$allWipPaths.Add($path) }
    $repositoryPrefix = $ResolvedRepositoryRoot + [System.IO.Path]::DirectorySeparatorChar

    foreach ($path in ($allWipPaths | Sort-Object)) {
        $normalisedPath = $path.Replace('\', '/')
        try {
            $fullPath = [System.IO.Path]::GetFullPath((Join-Path $ResolvedRepositoryRoot $path))
            if (-not $fullPath.StartsWith($repositoryPrefix, $script:LocalRetentionPathComparison)) {
                throw "Git-visible WIP escaped the repository root."
            }
            Assert-LocalRetentionExistingComponentsAreSafe -RepositoryRoot $ResolvedRepositoryRoot -Path $fullPath
            $item = Get-Item -LiteralPath $fullPath -Force -ErrorAction SilentlyContinue
            if ($null -eq $item) {
                $wipRecords.Add([pscustomobject][ordered]@{
                        path = $normalisedPath
                        exists = $false
                        itemKind = "Absent"
                        contentRead = $false
                    })
                continue
            }
            if (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
                throw "Git-visible WIP is a reparse point."
            }
            if ($item.PSIsContainer) {
                throw "Git-visible WIP resolved to a directory instead of a file."
            }
            $classification = if (Test-LocalRetentionSensitiveRelativePath -RelativePath $normalisedPath) {
                "SensitiveOrConfiguration"
            }
            else {
                "OwnerWork"
            }
            $identityHandle =
                [RagChallenge.LocalRetention.NativePathHandle]::OpenIdentity(
                    $fullPath,
                    $false)
            try {
                $wipRecords.Add([pscustomobject][ordered]@{
                        path = $normalisedPath
                        exists = $true
                        itemKind = "File"
                        byteLength = [long]$identityHandle.Length
                        creationTimeTicks = $identityHandle.CreationTimeTicks
                        lastWriteTimeTicks = $identityHandle.LastWriteTimeTicks
                        changeTimeTicks = $identityHandle.ChangeTimeTicks
                        attributes = $identityHandle.Attributes
                        volumeSerialNumberHex =
                            $identityHandle.VolumeSerialNumberHex
                        fileIdHex = $identityHandle.FileIdHex
                        classification = $classification
                        contentRead = $false
                    })
            }
            finally {
                $identityHandle.Dispose()
            }
            [void]$wipLockPaths.Add($fullPath)
        }
        catch {
            $blockingReasons.Add($_.Exception.Message)
        }
    }

    $worktreeCore = [ordered]@{
        cachedRawSha256 = $cachedRawSha256
        unstagedRawSha256 = $unstagedRawSha256
        gitVisibleWip = @($wipRecords)
    }
    $worktreeJson = $worktreeCore | ConvertTo-Json -Depth 6 -Compress
    return [pscustomobject]@{
        Head = $head
        StatusEntryCount = $statusRecords.Count
        StatusSha256 = Get-LocalRetentionSha256Bytes -Bytes $statusBytes
        WorktreeIdentitySha256 = Get-LocalRetentionSha256 -Value $worktreeJson
        GitVisibleWip = @($wipRecords)
        WipLockPaths = @($wipLockPaths | Sort-Object)
        BlockingReasons = @($blockingReasons | Sort-Object -Unique)
    }
}

function Resolve-LocalRetentionPath {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$ResolvedRepositoryRoot, [Parameter(Mandatory)][string]$RelativePath)

    if ([string]::IsNullOrWhiteSpace($RelativePath) -or [System.IO.Path]::IsPathFullyQualified($RelativePath) -or
        [System.Management.Automation.WildcardPattern]::ContainsWildcardCharacters($RelativePath) -or
        $RelativePath.Contains('\', [System.StringComparison]::Ordinal) -or $RelativePath -notmatch '^[A-Za-z0-9._/-]+$') {
        throw "The retention path is not a literal repository-relative path."
    }
    $segments = $RelativePath.Split('/')
    if ($segments.Count -eq 0 -or @($segments | Where-Object { $_.Length -eq 0 -or $_ -in '.', '..' }).Count -gt 0) {
        throw "The retention path contains an unsafe segment."
    }
    $protectedRootSegments = @(".git", "corpus", "reference-materials", "artifacts-local")
    if ($segments[0] -in $protectedRootSegments -or @($segments | Where-Object {
                $_ -in "obj", "node_modules" -or $_ -eq ".env" -or $_ -eq ".env.local" -or
                $_.StartsWith(".env.", [System.StringComparison]::OrdinalIgnoreCase) }).Count -gt 0) {
        throw "The retention path is protected and must be preserved."
    }
    $resolved = [System.IO.Path]::GetFullPath((Join-Path $ResolvedRepositoryRoot $RelativePath))
    $repositoryPrefix = $ResolvedRepositoryRoot + [System.IO.Path]::DirectorySeparatorChar
    if (-not $resolved.StartsWith($repositoryPrefix, $script:LocalRetentionPathComparison)) {
        throw "The retention path escaped the repository root."
    }
    $roundTrip = [System.IO.Path]::GetRelativePath($ResolvedRepositoryRoot, $resolved).Replace('\', '/')
    if (-not [string]::Equals($roundTrip, $RelativePath, $script:LocalRetentionPathComparison)) {
        throw "The retention path failed its exact identity check."
    }
    Assert-LocalRetentionExistingComponentsAreSafe -RepositoryRoot $ResolvedRepositoryRoot -Path $resolved
    return $resolved
}

function Test-LocalRetentionGitIgnored {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$ResolvedRepositoryRoot, [Parameter(Mandatory)][string]$RelativePath)

    if ($RelativePath -notmatch '^[A-Za-z0-9._/-]+$' -or
        [System.Management.Automation.WildcardPattern]::ContainsWildcardCharacters(
            $RelativePath)) {
        throw "The Git ignore probe is not an exact literal path."
    }
    $paths = @($RelativePath)
    if (-not $RelativePath.EndsWith('/', [System.StringComparison]::Ordinal)) {
        $paths += $RelativePath + '/'
    }
    $inputText = ($paths -join [char]0) + [char]0
    # Git check-ignore rejects global literal pathspec magic; the closed syntax above provides the equivalent literal boundary for its NUL-delimited stdin.
    $match = Invoke-LocalRetentionGitBytes `
        -ResolvedRepositoryRoot $ResolvedRepositoryRoot `
        -Arguments @("check-ignore", "--stdin", "-z", "--no-index") `
        -StandardInputBytes ($script:LocalRetentionUtf8.GetBytes($inputText)) `
        -OmitLiteralPathspecs `
        -AllowedExitCodes @(0, 1)
    return $match.Length -gt 0
}

function Assert-LocalRetentionGitBoundary {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$ResolvedRepositoryRoot, [Parameter(Mandatory)][string]$RelativePath)

    if (-not (Test-LocalRetentionGitIgnored -ResolvedRepositoryRoot $ResolvedRepositoryRoot -RelativePath $RelativePath)) {
        throw "The retention target is not ignored generated output."
    }
    $parent = [System.IO.Path]::GetDirectoryName($RelativePath.Replace('/', '\'))
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        $parentRelative = $parent.Replace('\', '/')
        if (Test-LocalRetentionGitIgnored -ResolvedRepositoryRoot $ResolvedRepositoryRoot -RelativePath $parentRelative) {
            throw "The retention target has an ignored parent and must be preserved."
        }
    }
    $targetPath = [System.IO.Path]::GetFullPath((Join-Path $ResolvedRepositoryRoot $RelativePath))
    if ($null -eq (Get-Item -LiteralPath $targetPath -Force -ErrorAction SilentlyContinue)) { return }
    $trackedBytes = Invoke-LocalRetentionGitBytes -ResolvedRepositoryRoot $ResolvedRepositoryRoot -Arguments @("ls-files", "-z", "--", $RelativePath)
    if ($trackedBytes.Length -gt 0) { throw "The retention target contains tracked files and must be preserved." }
}

function Assert-LocalRetentionOwnership {
    [CmdletBinding()]
    param([Parameter(Mandatory)]$Candidate, [Parameter(Mandatory)][string]$TargetPath)

    if ($Candidate.OwnershipKind -ceq "OwnerApprovedExactLegacyRoot") { return }
    if ($Candidate.OwnershipKind -ceq "MarkerOwned") {
        Assert-LocalRetentionOwnershipMarker -OutputRoot $TargetPath -Purpose $Candidate.MarkerPurpose -Owner $Candidate.MarkerOwner -CanonicalRelativePath $Candidate.RelativePath
        return
    }
    if ($Candidate.RetentionClass -ceq "TestEvidence") { return }
    throw "The retention target has no recognised ownership proof."
}

function Assert-NoLocalRetentionSensitiveContent {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$TargetPath)

    Assert-LocalRetentionTreeIsSafe -Root $TargetPath
    $containsConfiguration = $false
    foreach ($item in Get-ChildItem -LiteralPath $TargetPath -Recurse -Force) {
        if ($item.PSIsContainer) {
            continue
        }
        if (Test-LocalRetentionSecretOrLocalConfigurationPath -RelativePath $item.Name) {
            throw "The retention target contains secret or local-configuration material and was preserved."
        }
        if (Test-LocalRetentionConfigurationPath -RelativePath $item.Name) {
            $containsConfiguration = $true
        }
    }
    if ($containsConfiguration) {
        throw "The retention target contains configuration copies and was preserved without reading their content."
    }
}

function Get-LocalRetentionGeneratedTreeContentClass {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$TargetPath)

    Assert-LocalRetentionTreeIsSafe -Root $TargetPath
    $containsConfiguration = $false
    foreach ($item in Get-ChildItem -LiteralPath $TargetPath -Recurse -File -Force) {
        if (Test-LocalRetentionSecretOrLocalConfigurationPath -RelativePath $item.Name) {
            throw "The retention target contains secret or local-configuration material and was preserved."
        }
        if (Test-LocalRetentionConfigurationPath -RelativePath $item.Name) {
            $containsConfiguration = $true
        }
    }
    return $(if ($containsConfiguration) {
            "ConfigurationPresent"
        }
        else {
            "GeneratedOnly"
        })
}

function Get-LocalRetentionTreeInventory {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$TargetPath)

    $item = Get-Item -LiteralPath $TargetPath -Force -ErrorAction SilentlyContinue
    if ($null -eq $item) {
        return [pscustomobject]@{
            Exists = $false
            FileCount = 0
            DirectoryCount = 0
            ByteLength = [long]0
            NewestWriteUtc = $null
        }
    }
    Assert-LocalRetentionTreeIsSafe -Root $TargetPath
    $fileCount = 0
    $directoryCount = 0
    $byteLength = [long]0
    $newestWriteUtc = $item.LastWriteTimeUtc
    foreach ($child in Get-ChildItem -LiteralPath $TargetPath -Recurse -Force) {
        if ($child.LastWriteTimeUtc -gt $newestWriteUtc) {
            $newestWriteUtc = $child.LastWriteTimeUtc
        }
        if ($child.PSIsContainer) {
            $directoryCount++
        }
        else {
            $fileCount++
            $byteLength += $child.Length
        }
    }
    return [pscustomobject]@{
        Exists = $true
        FileCount = $fileCount
        DirectoryCount = $directoryCount
        ByteLength = $byteLength
        NewestWriteUtc = $newestWriteUtc.ToString("o")
    }
}

function Get-LocalRetentionTreeMeasurement {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$TargetPath)

    $item = Get-Item -LiteralPath $TargetPath -Force -ErrorAction SilentlyContinue
    if ($null -eq $item) {
        return [pscustomobject]@{ Exists = $false; FileCount = 0; DirectoryCount = 0; ByteLength = [long]0; NewestWriteUtc = $null; StructuralTreeSha256 = $null }
    }
    if (-not [System.OperatingSystem]::IsWindows()) {
        throw "Handle-bound structural identity is unavailable on this platform."
    }
    if (-not $item.PSIsContainer) {
        throw "A retention target is not a directory."
    }
    Assert-LocalRetentionTreeIsSafe -Root $TargetPath
    $children = @(Get-ChildItem -LiteralPath $TargetPath -Recurse -Force |
        Sort-Object FullName)
    $items = @($item) + $children
    $records = [System.Text.StringBuilder]::new()
    $fileCount = 0
    $directoryCount = 0
    $byteLength = [long]0
    $newestWriteTicks = [long]0
    foreach ($child in $items) {
        $isRoot = [string]::Equals(
            $child.FullName,
            $item.FullName,
            $script:LocalRetentionPathComparison)
        $relative = if ($isRoot) {
            "."
        }
        else {
            [System.IO.Path]::GetRelativePath(
                $TargetPath,
                $child.FullName).Replace('\', '/')
        }
        [RagChallenge.LocalRetention.NativePathHandle]::EnsureNoNamedDataStreams(
            $child.FullName)
        $handle = [RagChallenge.LocalRetention.NativePathHandle]::OpenIdentity(
            $child.FullName,
            [bool]$child.PSIsContainer)
        try {
            if ($handle.LastWriteTimeTicks -gt $newestWriteTicks) {
                $newestWriteTicks = $handle.LastWriteTimeTicks
            }
            if ($child.PSIsContainer) {
                if (-not $isRoot) { $directoryCount++ }
                [void]$records.Append(
                    "D`0$relative`0$($handle.CreationTimeTicks)`0" +
                    "$($handle.LastWriteTimeTicks)`0" +
                    "$($handle.Attributes)`0$($handle.VolumeSerialNumberHex)`0" +
                    "$($handle.FileIdHex)`n")
            }
            else {
                $fileCount++
                $byteLength += $handle.Length
                [void]$records.Append(
                    "F`0$relative`0$($handle.Length)`0$($handle.CreationTimeTicks)`0" +
                    "$($handle.LastWriteTimeTicks)`0$($handle.ChangeTimeTicks)`0" +
                    "$($handle.Attributes)`0$($handle.VolumeSerialNumberHex)`0" +
                    "$($handle.FileIdHex)`n")
            }
        }
        finally {
            $handle.Dispose()
        }
    }
    $newestWriteUtc = [datetime]::FromFileTimeUtc($newestWriteTicks).ToString("o")
    return [pscustomobject]@{ Exists = $true; FileCount = $fileCount; DirectoryCount = $directoryCount; ByteLength = $byteLength; NewestWriteUtc = $newestWriteUtc; StructuralTreeSha256 = Get-LocalRetentionSha256 -Value $records.ToString() }
}

function Get-LocalRetentionProtectedBoundary {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$ResolvedRepositoryRoot)

    $entries = [System.Collections.Generic.List[object]]::new()
    foreach ($protected in Get-LocalRetentionProtectedPaths) {
        $path = [System.IO.Path]::GetFullPath((Join-Path $ResolvedRepositoryRoot $protected.RelativePath))
        Assert-LocalRetentionExistingComponentsAreSafe -RepositoryRoot $ResolvedRepositoryRoot -Path $path
        $item = Get-Item -LiteralPath $path -Force -ErrorAction SilentlyContinue
        $exists = $null -ne $item
        $kind = if (-not $exists) { "Absent" } elseif ($item.PSIsContainer) { "Directory" } else { "File" }
        $isReparse = $exists -and (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0)
        if ($isReparse) { throw "A protected retention path is a reparse point." }
        $entries.Add([pscustomobject][ordered]@{ Path = $path; RelativePath = $protected.RelativePath; Exists = $exists; ItemKind = $kind; Disposition = $protected.Disposition; Reason = $protected.Reason; ContentRead = $false })
    }
    return @($entries)
}

function Get-LocalRetentionTransactionBoundary {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$ResolvedRepositoryRoot)

    $root = Join-Path $ResolvedRepositoryRoot "artifacts-local\retention-transactions"
    $item = Get-Item -LiteralPath $root -Force -ErrorAction SilentlyContinue
    if ($null -eq $item) { return [pscustomobject]@{ Blocked = $false; Reason = $null } }
    if (-not $item.PSIsContainer -or ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
        return [pscustomobject]@{ Blocked = $true; Reason = "The retention transaction root is unsafe." }
    }
    try {
        Assert-LocalRetentionOwnershipMarker -OutputRoot $root -Purpose "local-retention-transactions" -Owner $script:LocalRetentionOwner -CanonicalRelativePath "artifacts-local/retention-transactions"
        $unexpectedFiles = @(Get-ChildItem -LiteralPath $root -Force -File | Where-Object Name -cne $script:LocalRetentionMarkerName)
        $activeDirectories = @(Get-ChildItem -LiteralPath $root -Force -Directory)
        if ($unexpectedFiles.Count -gt 0 -or $activeDirectories.Count -gt 0) {
            return [pscustomobject]@{ Blocked = $true; Reason = "An incomplete or unknown retention transaction requires recovery." }
        }
    }
    catch { return [pscustomobject]@{ Blocked = $true; Reason = $_.Exception.Message } }
    return [pscustomobject]@{ Blocked = $false; Reason = $null }
}

function Get-LocalArtefactRetentionPlan {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$RepositoryRoot, [datetime]$ReferenceUtc = [datetime]::UtcNow)

    $resolvedRepositoryRoot = [System.IO.Path]::GetFullPath($RepositoryRoot).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
    $gitDirectory = Get-Item -LiteralPath (Join-Path $resolvedRepositoryRoot ".git") -Force -ErrorAction SilentlyContinue
    if ($null -eq $gitDirectory -or -not $gitDirectory.PSIsContainer -or ($gitDirectory.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw "The retention root has no safe local Git directory."
    }

    $gitState = Get-LocalRetentionGitState -ResolvedRepositoryRoot $resolvedRepositoryRoot
    $executorSha256 = (Get-FileHash -LiteralPath $script:LocalRetentionExecutorPath -Algorithm SHA256).Hash.ToLowerInvariant()
    $gitExecutableSha256 = Assert-LocalRetentionGitExecutableIdentity
    $protectedBoundary = @(Get-LocalRetentionProtectedBoundary -ResolvedRepositoryRoot $resolvedRepositoryRoot)
    $transactionBoundary = Get-LocalRetentionTransactionBoundary -ResolvedRepositoryRoot $resolvedRepositoryRoot
    $entries = [System.Collections.Generic.List[object]]::new()
    foreach ($candidate in Get-LocalRetentionCandidates) {
        try {
            $targetPath = Resolve-LocalRetentionPath -ResolvedRepositoryRoot $resolvedRepositoryRoot -RelativePath $candidate.RelativePath
            Assert-LocalRetentionGitBoundary -ResolvedRepositoryRoot $resolvedRepositoryRoot -RelativePath $candidate.RelativePath
            $targetItem = Get-Item -LiteralPath $targetPath -Force -ErrorAction SilentlyContinue
            $contentClass = "GeneratedOnly"
            if ($null -ne $targetItem) {
                Assert-LocalRetentionOwnership -Candidate $candidate -TargetPath $targetPath
                $contentClass = Get-LocalRetentionGeneratedTreeContentClass `
                    -TargetPath $targetPath
            }
            $measurement = if ($contentClass -ceq "ConfigurationPresent") {
                $inventory = Get-LocalRetentionTreeInventory -TargetPath $targetPath
                [pscustomobject]@{
                    Exists = $inventory.Exists
                    FileCount = $inventory.FileCount
                    DirectoryCount = $inventory.DirectoryCount
                    ByteLength = $inventory.ByteLength
                    NewestWriteUtc = $inventory.NewestWriteUtc
                    StructuralTreeSha256 = $null
                }
            }
            else {
                Get-LocalRetentionTreeMeasurement -TargetPath $targetPath
            }

            $blockingReason = $null
            $disposition = if (-not $measurement.Exists) { "ABSENT" }
            elseif ($contentClass -ceq "ConfigurationPresent") {
                "PRESERVE_CONFIGURATION"
            }
            elseif ($candidate.RetentionClass -ceq "TestEvidence" -and $null -ne $measurement.NewestWriteUtc -and
                ($ReferenceUtc.ToUniversalTime() - [datetime]$measurement.NewestWriteUtc) -lt $script:LocalRetentionSevenDays) { "PRESERVE_RETENTION_WINDOW" }
            elseif ($candidate.RetentionClass -ceq "TestEvidence") { $blockingReason = "The seven-day window elapsed, but canonical gate and incident release evidence is absent."; "PRESERVE_UNCERTAIN" }
            else { "DELETE_CANDIDATE_REQUIRES_ATTESTATION" }

            $attestationAssertions = if ($disposition -ceq "DELETE_CANDIDATE_REQUIRES_ATTESTATION") {
                @("The exact tree is generated output and contains no manual or ignored WIP", "Restore from the tracked project and active lockfile is reproducible", "No canonical gate or open incident depends on these exact bytes", "Deletion is regenerable but the same ephemeral bytes are not recoverable")
            } else { @() }
            $entries.Add([pscustomobject][ordered]@{ Path = $targetPath; RelativePath = $candidate.RelativePath; ByteLength = $measurement.ByteLength; FileCount = $measurement.FileCount; DirectoryCount = $measurement.DirectoryCount; NewestWriteUtc = $measurement.NewestWriteUtc; StructuralTreeSha256 = $measurement.StructuralTreeSha256; ContentRead = $false; Reason = $candidate.Reason; Recoverability = $candidate.Recoverability; OwnershipKind = $candidate.OwnershipKind; Disposition = $disposition; AttestationAssertions = $attestationAssertions; BlockingReason = $blockingReason })
        }
        catch {
            $targetPath = [System.IO.Path]::GetFullPath((Join-Path $resolvedRepositoryRoot $candidate.RelativePath))
            $blockingReason = $_.Exception.Message
            $inventory = $null
            try {
                $inventory = Get-LocalRetentionTreeInventory -TargetPath $targetPath
            }
            catch {
                # Unsafe trees remain unmeasured and preserved.
            }
            $entries.Add([pscustomobject][ordered]@{ Path = $targetPath; RelativePath = $candidate.RelativePath; ByteLength = if ($null -eq $inventory) { $null } else { $inventory.ByteLength }; FileCount = if ($null -eq $inventory) { $null } else { $inventory.FileCount }; DirectoryCount = if ($null -eq $inventory) { $null } else { $inventory.DirectoryCount }; NewestWriteUtc = if ($null -eq $inventory) { $null } else { $inventory.NewestWriteUtc }; StructuralTreeSha256 = $null; ContentRead = $false; Reason = $candidate.Reason; Recoverability = $candidate.Recoverability; OwnershipKind = $candidate.OwnershipKind; Disposition = "PRESERVE_UNCERTAIN"; AttestationAssertions = @(); BlockingReason = $blockingReason })
        }
    }

    $deletionEntries = @($entries | Where-Object Disposition -ceq "DELETE_CANDIDATE_REQUIRES_ATTESTATION")
    $deletionCandidateBytes = if ($deletionEntries.Count -eq 0) { [long]0 } else { [long](($deletionEntries | Measure-Object ByteLength -Sum).Sum) }
    $attestationCore = [ordered]@{ baseline = $gitState.Head; targets = @($deletionEntries | ForEach-Object { [ordered]@{ relativePath = $_.RelativePath; byteLength = $_.ByteLength; structuralTreeSha256 = $_.StructuralTreeSha256; assertions = $_.AttestationAssertions } }) }
    $legacyAttestationSha256 = Get-LocalRetentionSha256 -Value ($attestationCore | ConvertTo-Json -Depth 8 -Compress)
    $boundaryReasons = [System.Collections.Generic.List[string]]::new()
    foreach ($reason in $gitState.BlockingReasons) { $boundaryReasons.Add($reason) }
    if ($transactionBoundary.Blocked) { $boundaryReasons.Add($transactionBoundary.Reason) }
    $uncertainCount = @($entries | Where-Object Disposition -ceq "PRESERVE_UNCERTAIN").Count
    $core = [ordered]@{ schemaVersion = 3; repositoryRoot = $resolvedRepositoryRoot; baseline = $gitState.Head; executorSha256 = $executorSha256; gitExecutableSha256 = $gitExecutableSha256; gitStatusEntryCount = $gitState.StatusEntryCount; gitStatusSha256 = $gitState.StatusSha256; worktreeIdentitySha256 = $gitState.WorktreeIdentitySha256; gitVisibleWip = $gitState.GitVisibleWip; legacyOwnershipAttestationSha256 = $legacyAttestationSha256; blocked = $uncertainCount -gt 0 -or $boundaryReasons.Count -gt 0; boundaryBlockingReasons = @($boundaryReasons | Sort-Object -Unique); deletionCandidateCount = $deletionEntries.Count; deletionCandidateBytes = $deletionCandidateBytes; protected = $protectedBoundary; coverage = @(Get-LocalRetentionCoverage); entries = @($entries) }
    $planJson = $core | ConvertTo-Json -Depth 10 -Compress
    return [pscustomobject][ordered]@{ schemaVersion = $core.schemaVersion; generatedAtUtc = $ReferenceUtc.ToUniversalTime().ToString("o"); repositoryRoot = $core.repositoryRoot; baseline = $core.baseline; executorSha256 = $core.executorSha256; gitExecutableSha256 = $core.gitExecutableSha256; gitStatusEntryCount = $core.gitStatusEntryCount; gitStatusSha256 = $core.gitStatusSha256; worktreeIdentitySha256 = $core.worktreeIdentitySha256; gitVisibleWip = $core.gitVisibleWip; legacyOwnershipAttestationSha256 = $core.legacyOwnershipAttestationSha256; blocked = $core.blocked; boundaryBlockingReasons = $core.boundaryBlockingReasons; deletionCandidateCount = $core.deletionCandidateCount; deletionCandidateBytes = $core.deletionCandidateBytes; planSha256 = Get-LocalRetentionSha256 -Value $planJson; protected = $core.protected; coverage = $core.coverage; entries = $core.entries }
}

function Get-LocalRetentionPathIdentity {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][bool]$Directory
    )

    $handle = [RagChallenge.LocalRetention.NativePathHandle]::OpenIdentity(
        $Path,
        $Directory)
    try {
        return [pscustomobject][ordered]@{
            identityToken = $handle.IdentityToken
            volumeSerialNumber = $handle.VolumeSerialNumberHex
            fileId = $handle.FileIdHex
            creationTimeTicks = $handle.CreationTimeTicks
            lastWriteTimeTicks = $handle.LastWriteTimeTicks
            changeTimeTicks = $handle.ChangeTimeTicks
            attributes = [uint32]$handle.Attributes
            length = [long]$handle.Length
        }
    }
    finally {
        $handle.Dispose()
    }
}

function Read-LocalRetentionRecoveryJournal {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$ResolvedRepositoryRoot,
        [Parameter(Mandatory)][string]$TransactionId
    )

    if ($TransactionId -notmatch '^[0-9a-f]{16}-[0-9a-f]{32}$') {
        throw "A recovery transaction ID is invalid."
    }
    $transactionsRoot = Join-Path $ResolvedRepositoryRoot (
        "artifacts-local\retention-transactions")
    Assert-LocalRetentionOwnershipMarker `
        -OutputRoot $transactionsRoot `
        -Purpose "local-retention-transactions" `
        -Owner $script:LocalRetentionOwner `
        -CanonicalRelativePath "artifacts-local/retention-transactions"
    Assert-LocalRetentionTreeIsSafe -Root $transactionsRoot
    $parentItems = @(Get-ChildItem -LiteralPath $transactionsRoot -Force)
    $unexpectedParentFiles = @($parentItems |
        Where-Object { -not $_.PSIsContainer -and
            $_.Name -cne $script:LocalRetentionMarkerName })
    $activeTransactions = @($parentItems | Where-Object PSIsContainer)
    if ($unexpectedParentFiles.Count -gt 0 -or
        $activeTransactions.Count -ne 1 -or
        $activeTransactions[0].Name -cne $TransactionId) {
        throw "Recovery requires exactly the named marker-owned transaction."
    }

    $transactionRoot = Join-Path $transactionsRoot $TransactionId
    $relativeRoot = "artifacts-local/retention-transactions/$TransactionId"
    $stagingRoot = Join-Path $transactionRoot "staged"
    $journalPath = Join-Path $transactionRoot "transaction.jsonl"
    Assert-LocalRetentionExistingComponentsAreSafe `
        -RepositoryRoot $ResolvedRepositoryRoot `
        -Path $journalPath
    Assert-LocalRetentionOwnershipMarker `
        -OutputRoot $transactionRoot `
        -Purpose "local-retention-transaction" `
        -Owner $script:LocalRetentionOwner `
        -CanonicalRelativePath $relativeRoot
    Assert-LocalRetentionOwnershipMarker `
        -OutputRoot $stagingRoot `
        -Purpose "local-retention-staging" `
        -Owner $script:LocalRetentionOwner `
        -CanonicalRelativePath "$relativeRoot/staged"
    Assert-LocalRetentionTreeIsSafe -Root $transactionRoot
    $rootItems = @(Get-ChildItem -LiteralPath $transactionRoot -Force)
    $expectedRootNames = @(
        $script:LocalRetentionMarkerName,
        "staged",
        "transaction.jsonl")
    if ($rootItems.Count -ne $expectedRootNames.Count -or
        @($rootItems | Where-Object Name -cnotin $expectedRootNames).Count -gt 0) {
        throw "The recovery transaction contains unexpected infrastructure."
    }
    $journalItem = Get-Item -LiteralPath $journalPath -Force -ErrorAction Stop
    if ($journalItem.PSIsContainer -or
        ($journalItem.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0 -or
        $journalItem.Length -le 0 -or $journalItem.Length -gt 10MB) {
        throw "The recovery journal is unsafe or oversized."
    }

    $journalGuard = [RagChallenge.LocalRetention.NativePathHandle]::OpenWorktreeGuard(
        $journalPath,
        $false)
    try {
        $journalIdentity = $journalGuard.IdentityToken
        $journalSha256 = (Get-FileHash `
                -LiteralPath $journalPath `
                -Algorithm SHA256).Hash.ToLowerInvariant()
        $events = [System.Collections.Generic.List[object]]::new()
        foreach ($line in [System.IO.File]::ReadLines(
                $journalPath,
                $script:LocalRetentionStrictUtf8)) {
            if ([string]::IsNullOrWhiteSpace($line) -or $line.Length -gt 65536) {
                throw "The recovery journal contains an empty or oversized event."
            }
            try {
                $events.Add(($line | ConvertFrom-Json -ErrorAction Stop))
            }
            catch {
                throw "The recovery journal contains invalid JSON."
            }
        }
        $journalSha256After = (Get-FileHash `
                -LiteralPath $journalPath `
                -Algorithm SHA256).Hash.ToLowerInvariant()
        $journalGuard.Refresh()
        if ($journalGuard.IdentityToken -cne $journalIdentity -or
            $journalSha256After -cne $journalSha256) {
            throw "The recovery journal changed while it was being read."
        }
        $journalNativeIdentity = [pscustomobject][ordered]@{
            identityToken = $journalGuard.IdentityToken
            volumeSerialNumber = $journalGuard.VolumeSerialNumberHex
            fileId = $journalGuard.FileIdHex
            creationTimeTicks = $journalGuard.CreationTimeTicks
            lastWriteTimeTicks = $journalGuard.LastWriteTimeTicks
            changeTimeTicks = $journalGuard.ChangeTimeTicks
            attributes = [uint32]$journalGuard.Attributes
            length = [long]$journalGuard.Length
        }
    }
    finally {
        $journalGuard.Dispose()
    }

    if ($events.Count -lt 6 -or $events[0].event -cne "PREPARED") {
        throw "The recovery journal has no valid PREPARED prefix."
    }
    $planSha256 = [string]$events[0].planSha256
    if ($planSha256 -notmatch '^[0-9a-f]{64}$' -or
        -not $TransactionId.StartsWith(
            $planSha256.Substring(0, 16) + "-",
            [System.StringComparison]::Ordinal)) {
        throw "The recovery transaction identity does not match its original plan."
    }
    foreach ($eventRecord in $events) {
        if ($eventRecord.schemaVersion -ne 1 -or
            [string]$eventRecord.planSha256 -cne $planSha256) {
            throw "The recovery journal contains a divergent event identity."
        }
    }
    $prepared = $events[0]
    $targets = @($prepared.data.targets)
    if ($targets.Count -eq 0) {
        throw "The recovery journal has no prepared targets."
    }
    $knownCandidates = @(Get-LocalRetentionCandidates)
    $seenTargets = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::Ordinal)
    foreach ($target in $targets) {
        $relativePath = [string]$target.relativePath
        if (-not $seenTargets.Add($relativePath) -or
            @($knownCandidates | Where-Object RelativePath -ceq $relativePath).Count -ne 1 -or
            [long]$target.byteLength -lt 0 -or
            [string]$target.structuralTreeSha256 -notmatch '^[0-9a-f]{64}$') {
            throw "The recovery journal contains an invalid prepared target."
        }
    }

    $cursor = 1
    foreach ($target in $targets) {
        if ($cursor -ge $events.Count -or
            $events[$cursor].event -cne "STAGED" -or
            [string]$events[$cursor].data.relativePath -cne
                [string]$target.relativePath -or
            [long]$events[$cursor].data.byteLength -ne [long]$target.byteLength -or
            [string]$events[$cursor].data.structuralTreeSha256 -cne
                [string]$target.structuralTreeSha256) {
            throw "The recovery journal has an invalid STAGED sequence."
        }
        $cursor++
    }

    $deletedRelativePaths = [System.Collections.Generic.List[string]]::new()
    $failedEvent = $null
    $deleteIndex = 0
    while ($cursor -lt $events.Count - 1) {
        if ($deleteIndex -ge $targets.Count -or
            $events[$cursor].event -cne "DELETE_STARTED" -or
            [string]$events[$cursor].data.relativePath -cne
                [string]$targets[$deleteIndex].relativePath) {
            throw "The recovery journal has an invalid deletion sequence."
        }
        $cursor++
        if ($cursor -ge $events.Count) {
            throw "The recovery journal ends during a deletion event."
        }
        if ($events[$cursor].event -ceq "DELETED" -and
            [string]$events[$cursor].data.relativePath -ceq
                [string]$targets[$deleteIndex].relativePath -and
            [long]$events[$cursor].data.byteLength -eq
                [long]$targets[$deleteIndex].byteLength) {
            $deletedRelativePaths.Add(
                [string]$targets[$deleteIndex].relativePath)
            $deleteIndex++
            $cursor++
            continue
        }
        if ($events[$cursor].event -ceq "PARTIAL_DELETE_FAILURE" -and
            [string]$events[$cursor].data.failedRelativePath -ceq
                [string]$targets[$deleteIndex].relativePath) {
            $failedEvent = $events[$cursor]
            $cursor++
            break
        }
        throw "The recovery journal has an invalid deletion result."
    }
    if ($null -eq $failedEvent -or $cursor -ne $events.Count - 1 -or
        $events[$cursor].event -cne "APPLY_FAILED" -or
        [int]$events[$cursor].data.deletedTargetCount -ne
            $deletedRelativePaths.Count) {
        throw "The recovery journal is not a supported partial-failure shape."
    }
    $reportedDeleted = @($failedEvent.data.deletedRelativePaths |
        ForEach-Object { [string]$_ })
    if ($reportedDeleted.Count -ne $deletedRelativePaths.Count) {
        throw "The recovery journal has a divergent deleted-target summary."
    }
    for ($index = 0; $index -lt $reportedDeleted.Count; $index++) {
        if ($reportedDeleted[$index] -cne $deletedRelativePaths[$index]) {
            throw "The recovery journal has a divergent deleted-target summary."
        }
    }

    return [pscustomobject][ordered]@{
        transactionId = $TransactionId
        transactionRoot = $transactionRoot
        stagingRoot = $stagingRoot
        journalPath = $journalPath
        journalSha256 = $journalSha256
        journalIdentity = $journalNativeIdentity
        originalPlanSha256 = $planSha256
        originalBaseline = [string]$prepared.data.baseline
        originalGitStatusSha256 = [string]$prepared.data.gitStatusSha256
        originalWorktreeIdentitySha256 =
            [string]$prepared.data.worktreeIdentitySha256
        targets = $targets
        deletedRelativePaths = @($deletedRelativePaths)
        failedEvent = $failedEvent
    }
}

function Get-LocalRetentionRecoveryPlan {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$RepositoryRoot,
        [Parameter(Mandatory)][string]$TransactionId,
        [datetime]$ReferenceUtc = [datetime]::UtcNow
    )

    $resolvedRepositoryRoot = [System.IO.Path]::GetFullPath(
        $RepositoryRoot).TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar)
    $gitState = Get-LocalRetentionGitState `
        -ResolvedRepositoryRoot $resolvedRepositoryRoot
    $journal = Read-LocalRetentionRecoveryJournal `
        -ResolvedRepositoryRoot $resolvedRepositoryRoot `
        -TransactionId $TransactionId
    $executorSha256 = (Get-FileHash `
            -LiteralPath $script:LocalRetentionExecutorPath `
            -Algorithm SHA256).Hash.ToLowerInvariant()
    $gitExecutableSha256 = Assert-LocalRetentionGitExecutableIdentity
    $protected = @(Get-LocalRetentionProtectedBoundary `
            -ResolvedRepositoryRoot $resolvedRepositoryRoot)
    $protectedBoundarySha256 = Get-LocalRetentionSha256 -Value (
        $protected | ConvertTo-Json -Depth 5 -Compress)
    $blockingReasons = [System.Collections.Generic.List[string]]::new()
    foreach ($reason in $gitState.BlockingReasons) {
        $blockingReasons.Add($reason)
    }
    if ($gitState.Head -cne $journal.originalBaseline) {
        $blockingReasons.Add(
            "The recovery baseline differs from the prepared transaction.")
    }

    $deletedSet = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::Ordinal)
    foreach ($relativePath in $journal.deletedRelativePaths) {
        [void]$deletedSet.Add($relativePath)
    }
    $failedRelativePath = [string]$journal.failedEvent.data.failedRelativePath
    $entries = [System.Collections.Generic.List[object]]::new()
    $expectedStagingNames = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::Ordinal)
    [void]$expectedStagingNames.Add($script:LocalRetentionMarkerName)
    $previouslyDeletedBytes = [long]0
    $recoveryTargetBytes = [long]0

    for ($index = 0; $index -lt $journal.targets.Count; $index++) {
        $target = $journal.targets[$index]
        $relativePath = [string]$target.relativePath
        $originalByteLength = [long]$target.byteLength
        $originalStructuralTreeSha256 =
            [string]$target.structuralTreeSha256
        $candidate = @(Get-LocalRetentionCandidates |
            Where-Object RelativePath -ceq $relativePath)[0]
        $originalPath = Resolve-LocalRetentionPath `
            -ResolvedRepositoryRoot $resolvedRepositoryRoot `
            -RelativePath $relativePath
        $stagingName = (("{0:D3}-" -f $index) +
            (Get-LocalRetentionSha256 -Value $relativePath).Substring(0, 16))
        $stagedPath = Join-Path $journal.stagingRoot $stagingName
        $originalExists = $null -ne (Get-Item `
                -LiteralPath $originalPath `
                -Force `
                -ErrorAction SilentlyContinue)
        if ($originalExists) {
            $blockingReasons.Add(
                "An original recovery path was recreated: $relativePath")
        }

        $currentMeasurement = $null
        $currentIdentity = $null
        $disposition = $null
        $blockingReason = $null
        $stagedExists = $null -ne (Get-Item `
                -LiteralPath $stagedPath `
                -Force `
                -ErrorAction SilentlyContinue)
        if ($deletedSet.Contains($relativePath)) {
            $previouslyDeletedBytes += $originalByteLength
            $disposition = "ALREADY_DELETED_REGENERABLE"
            if ($stagedExists) {
                $blockingReason =
                    "A target recorded as deleted still exists in staging."
            }
        }
        elseif ($relativePath -ceq $failedRelativePath) {
            [void]$expectedStagingNames.Add($stagingName)
            if (-not $stagedExists) {
                $disposition = "PRESERVE_UNCERTAIN"
                $blockingReason =
                    "The partially deleted recovery root is missing."
            }
            else {
                $currentMeasurement = Get-LocalRetentionTreeMeasurement `
                    -TargetPath $stagedPath
                $currentIdentity = Get-LocalRetentionPathIdentity `
                    -Path $stagedPath `
                    -Directory $true
                $reported = $journal.failedEvent.data.remaining
                $reportedMatches =
                    [string]$reported.state -ceq "MEASURED_PARTIAL" -and
                    [bool]$reported.exists -eq $currentMeasurement.Exists -and
                    [long]$reported.fileCount -eq $currentMeasurement.FileCount -and
                    [long]$reported.directoryCount -eq
                        $currentMeasurement.DirectoryCount -and
                    [long]$reported.byteLength -eq $currentMeasurement.ByteLength
                if (-not $reportedMatches) {
                    $disposition = "PRESERVE_UNCERTAIN"
                    $blockingReason =
                        "The partial recovery remainder differs from its journal."
                }
                elseif ($currentMeasurement.FileCount -ne 0 -or
                    $currentMeasurement.DirectoryCount -ne 0 -or
                    $currentMeasurement.ByteLength -ne 0) {
                    $disposition = "PRESERVE_UNCERTAIN"
                    $blockingReason =
                        "A partial recovery remainder contains unapproved data."
                }
                else {
                    $disposition =
                        "RECOVERY_DELETE_EMPTY_PARTIAL_ROOT_REQUIRES_APPROVAL"
                    $previouslyDeletedBytes += $originalByteLength
                }
            }
        }
        else {
            [void]$expectedStagingNames.Add($stagingName)
            if (-not $stagedExists) {
                $disposition = "PRESERVE_UNCERTAIN"
                $blockingReason = "An intact staged recovery target is missing."
            }
            else {
                $currentMeasurement = Get-LocalRetentionTreeMeasurement `
                    -TargetPath $stagedPath
                $currentIdentity = Get-LocalRetentionPathIdentity `
                    -Path $stagedPath `
                    -Directory $true
                if ($currentMeasurement.ByteLength -ne $originalByteLength -or
                    $currentMeasurement.StructuralTreeSha256 -cne
                        $originalStructuralTreeSha256) {
                    $disposition = "PRESERVE_UNCERTAIN"
                    $blockingReason =
                        "An intact staged recovery target differs from approval."
                }
                else {
                    $disposition =
                        "RECOVERY_DELETE_INTACT_TARGET_REQUIRES_APPROVAL"
                    $recoveryTargetBytes += $currentMeasurement.ByteLength
                }
            }
        }
        if ($null -ne $blockingReason) {
            $blockingReasons.Add("${relativePath}: $blockingReason")
        }
        $entries.Add([pscustomobject][ordered]@{
                index = $index
                relativePath = $relativePath
                originalPath = $originalPath
                stagedPath = $stagedPath
                originalByteLength = $originalByteLength
                originalStructuralTreeSha256 =
                    $originalStructuralTreeSha256
                currentByteLength = if ($null -eq $currentMeasurement) {
                    [long]0
                }
                else {
                    [long]$currentMeasurement.ByteLength
                }
                currentFileCount = if ($null -eq $currentMeasurement) {
                    0
                }
                else {
                    $currentMeasurement.FileCount
                }
                currentDirectoryCount = if ($null -eq $currentMeasurement) {
                    0
                }
                else {
                    $currentMeasurement.DirectoryCount
                }
                currentStructuralTreeSha256 = if (
                    $null -eq $currentMeasurement) {
                    $null
                }
                else {
                    $currentMeasurement.StructuralTreeSha256
                }
                currentIdentity = $currentIdentity
                disposition = $disposition
                reason = if ($relativePath -ceq $failedRelativePath) {
                    "The approved content was deleted, but its empty ReadOnly root remains."
                }
                else {
                    $candidate.Reason
                }
                recoverability = if ($deletedSet.Contains($relativePath) -or
                    $relativePath -ceq $failedRelativePath) {
                    "Original content is regenerable but not recoverable as the same ephemeral bytes."
                }
                else {
                    "Exact approved bytes remain preserved in the transaction until a separately approved recovery Apply."
                }
                contentRead = $false
                blockingReason = $blockingReason
            })
    }

    $actualStagingNames = @(Get-ChildItem `
            -LiteralPath $journal.stagingRoot `
            -Force |
        ForEach-Object Name)
    foreach ($name in $actualStagingNames) {
        if (-not $expectedStagingNames.Contains($name)) {
            $blockingReasons.Add(
                "The recovery staging root contains an unexpected item: $name")
        }
    }
    foreach ($name in $expectedStagingNames) {
        if ($name -cne $script:LocalRetentionMarkerName -and
            $name -cnotin $actualStagingNames) {
            $blockingReasons.Add(
                "The recovery staging root is missing an expected item: $name")
        }
    }

    $recoveryEntries = @($entries | Where-Object {
            $_.disposition -cin @(
                "RECOVERY_DELETE_EMPTY_PARTIAL_ROOT_REQUIRES_APPROVAL",
                "RECOVERY_DELETE_INTACT_TARGET_REQUIRES_APPROVAL")
        })
    $core = [ordered]@{
        schemaVersion = 1
        mode = "RECOVERY_DRY_RUN"
        repositoryRoot = $resolvedRepositoryRoot
        baseline = $gitState.Head
        transactionId = $TransactionId
        transactionRoot = $journal.transactionRoot
        stagingRoot = $journal.stagingRoot
        journalPath = $journal.journalPath
        journalSha256 = $journal.journalSha256
        journalIdentity = $journal.journalIdentity
        originalPlanSha256 = $journal.originalPlanSha256
        executorSha256 = $executorSha256
        gitExecutableSha256 = $gitExecutableSha256
        gitStatusEntryCount = $gitState.StatusEntryCount
        gitStatusSha256 = $gitState.StatusSha256
        worktreeIdentitySha256 = $gitState.WorktreeIdentitySha256
        protectedBoundarySha256 = $protectedBoundarySha256
        blocked = $blockingReasons.Count -gt 0
        boundaryBlockingReasons = @($blockingReasons | Sort-Object -Unique)
        previouslyDeletedBytes = $previouslyDeletedBytes
        recoveryTargetCount = $recoveryEntries.Count
        recoveryTargetBytes = $recoveryTargetBytes
        protected = $protected
        entries = @($entries)
    }
    $coreJson = $core | ConvertTo-Json -Depth 12 -Compress
    return [pscustomobject][ordered]@{
        schemaVersion = $core.schemaVersion
        mode = $core.mode
        generatedAtUtc = $ReferenceUtc.ToUniversalTime().ToString("o")
        repositoryRoot = $core.repositoryRoot
        baseline = $core.baseline
        transactionId = $core.transactionId
        transactionRoot = $core.transactionRoot
        stagingRoot = $core.stagingRoot
        journalPath = $core.journalPath
        journalSha256 = $core.journalSha256
        journalIdentity = $core.journalIdentity
        originalPlanSha256 = $core.originalPlanSha256
        executorSha256 = $core.executorSha256
        gitExecutableSha256 = $core.gitExecutableSha256
        gitStatusEntryCount = $core.gitStatusEntryCount
        gitStatusSha256 = $core.gitStatusSha256
        worktreeIdentitySha256 = $core.worktreeIdentitySha256
        protectedBoundarySha256 = $core.protectedBoundarySha256
        blocked = $core.blocked
        boundaryBlockingReasons = $core.boundaryBlockingReasons
        previouslyDeletedBytes = $core.previouslyDeletedBytes
        recoveryTargetCount = $core.recoveryTargetCount
        recoveryTargetBytes = $core.recoveryTargetBytes
        recoveryPlanSha256 = Get-LocalRetentionSha256 -Value $coreJson
        protected = $core.protected
        entries = $core.entries
    }
}

function Test-LocalRetentionPotentialWriterProcess {
    [CmdletBinding()]
    param([Parameter(Mandatory)]$Process, [Parameter(Mandatory)][string]$ResolvedRepositoryRoot, [Parameter(Mandatory)][object[]]$DeletionEntries)

    $name = [string]$Process.Name; $commandLine = [string]$Process.CommandLine; $executablePath = [string]$Process.ExecutablePath
    foreach ($path in @($ResolvedRepositoryRoot) + @($DeletionEntries | ForEach-Object Path)) {
        if ((-not [string]::IsNullOrWhiteSpace($commandLine) -and $commandLine.IndexOf($path, $script:LocalRetentionPathComparison) -ge 0) -or
            (-not [string]::IsNullOrWhiteSpace($executablePath) -and $executablePath.StartsWith($path, $script:LocalRetentionPathComparison))) { return $true }
    }
    if ($name -match '^(?i:RagChallenge|testhost|vstest|msbuild|dotnet)(?:[.]exe)?$') { return $true }
    if ($name -match '^(?i:node|npm|npx|vite|tsc)(?:[.]exe)?$') {
        if ([string]::IsNullOrWhiteSpace($commandLine) -or
            [string]::IsNullOrWhiteSpace($executablePath)) {
            return $true
        }
        if ($commandLine -match '(?i:RagChallenge|ai-orchestrator|npm\s+(?:run\s+)?(?:build|test)|vite|tsc)') {
            return $true
        }
    }
    return $false
}

function Assert-NoLocalRetentionTargetUse {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$ResolvedRepositoryRoot, [Parameter(Mandatory)][object[]]$DeletionEntries)

    if (-not [System.OperatingSystem]::IsWindows()) { throw "Process-use verification is unavailable on this platform; targets were preserved." }
    try { $processes = @(Get-CimInstance Win32_Process -ErrorAction Stop) } catch { throw "Unable to establish the RAG-Challenge process-use boundary." }
    $byId = @{}; foreach ($process in $processes) { $byId[[int]$process.ProcessId] = $process }
    $owned = @($processes | Where-Object {
            if ([int]$_.ProcessId -eq $PID) {
                return $false
            }
            $candidate = $_
            $seen = [System.Collections.Generic.HashSet[int]]::new()
            while ($null -ne $candidate -and $seen.Add([int]$candidate.ProcessId)) {
                if (Test-LocalRetentionPotentialWriterProcess `
                        -Process $candidate `
                        -ResolvedRepositoryRoot $ResolvedRepositoryRoot `
                        -DeletionEntries $DeletionEntries) {
                    return $true
                }
                $parentId = [int]$candidate.ParentProcessId
                $candidate = if ($byId.ContainsKey($parentId)) {
                    $byId[$parentId]
                }
                else {
                    $null
                }
            }
            return $false
        })
    if ($owned.Count -gt 0) { $identities = ($owned | ForEach-Object { "$($_.Name):$($_.ProcessId)" }) -join ", "; throw "A potential RAG-Challenge writer process blocks retention: $identities" }
    foreach ($entry in $DeletionEntries) {
        foreach ($file in Get-ChildItem -LiteralPath $entry.Path -Recurse -File -Force) {
            try {
                $guard = [RagChallenge.LocalRetention.NativePathHandle]::OpenWorktreeGuard(
                    $file.FullName,
                    $false)
                $guard.Dispose()
            }
            catch { throw "A retention target contains a file that is in use and was preserved." }
        }
    }
}

function Open-LocalRetentionWorktreeLocks {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string[]]$Paths)

    $handles = [System.Collections.Generic.List[object]]::new()
    try {
        foreach ($path in @(
                $Paths +
                $script:LocalRetentionExecutorPath +
                $script:LocalRetentionGitPath |
                Sort-Object -Unique)) {
            $item = Get-Item -LiteralPath $path -Force -ErrorAction SilentlyContinue
            if ($null -eq $item -or $item.PSIsContainer) { continue }
            if (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) { throw "A Git-visible worktree file became a reparse point." }
            $handles.Add(
                [RagChallenge.LocalRetention.NativePathHandle]::OpenWorktreeGuard(
                    $item.FullName,
                    $false))
        }
        return $handles
    }
    catch {
        foreach ($handle in $handles) { $handle.Dispose() }
        throw "The approved worktree could not be locked against concurrent writes."
    }
}

function New-LocalRetentionMutex {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$ResolvedRepositoryRoot)

    $rootHash = Get-LocalRetentionSha256 -Value $ResolvedRepositoryRoot.ToLowerInvariant()
    $mutex = [System.Threading.Mutex]::new($false, "Local\RAGChallenge.Retention.$rootHash")
    try { if (-not $mutex.WaitOne(0)) { throw "Another retention execution owns the repository mutex." } }
    catch [System.Threading.AbandonedMutexException] { $mutex.Dispose(); throw "An abandoned retention mutex requires recovery review." }
    return $mutex
}

function Initialise-LocalRetentionTransactionInfrastructure {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$ResolvedRepositoryRoot)

    $artifactsRoot = Join-Path $ResolvedRepositoryRoot "artifacts-local"
    Assert-LocalRetentionExistingComponentsAreSafe -RepositoryRoot $ResolvedRepositoryRoot -Path $artifactsRoot
    [System.IO.Directory]::CreateDirectory($artifactsRoot) | Out-Null
    foreach ($definition in @(
            [pscustomobject]@{ Path = (Join-Path $artifactsRoot "retention-transactions"); Purpose = "local-retention-transactions"; RelativePath = "artifacts-local/retention-transactions" },
            [pscustomobject]@{ Path = (Join-Path $artifactsRoot "retention-history"); Purpose = "local-retention-history"; RelativePath = "artifacts-local/retention-history" })) {
        $item = Get-Item -LiteralPath $definition.Path -Force -ErrorAction SilentlyContinue
        if ($null -eq $item) { New-LocalRetentionOwnedOutputRoot -OutputRoot $definition.Path -RepositoryRoot $ResolvedRepositoryRoot -Purpose $definition.Purpose -Owner $script:LocalRetentionOwner -CanonicalRelativePath $definition.RelativePath }
        else { Assert-LocalRetentionOwnershipMarker -OutputRoot $definition.Path -Purpose $definition.Purpose -Owner $script:LocalRetentionOwner -CanonicalRelativePath $definition.RelativePath }
    }
}

function Assert-LocalRetentionPlanNotConsumed {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$ResolvedRepositoryRoot,
        [Parameter(Mandatory)][string]$PlanSha256
    )

    $historyRoot = Join-Path $ResolvedRepositoryRoot "artifacts-local\retention-history"
    $historyItem = Get-Item -LiteralPath $historyRoot -Force -ErrorAction SilentlyContinue
    if ($null -eq $historyItem) {
        return
    }
    Assert-LocalRetentionOwnershipMarker `
        -OutputRoot $historyRoot `
        -Purpose "local-retention-history" `
        -Owner $script:LocalRetentionOwner `
        -CanonicalRelativePath "artifacts-local/retention-history"
    Assert-LocalRetentionTreeIsSafe -Root $historyRoot
    $historyItems = @(Get-ChildItem -LiteralPath $historyRoot -Force)
    if (@($historyItems | Where-Object PSIsContainer).Count -gt 0) {
        throw "The retention history contains an unexpected subdirectory."
    }
    foreach ($recordFile in $historyItems) {
        if ($recordFile.Name -ceq $script:LocalRetentionMarkerName) {
            continue
        }
        if ($recordFile.Extension -cne ".jsonl" -or $recordFile.Length -gt 10MB) {
            throw "The retention history contains an unknown or oversized record."
        }
        foreach ($line in [System.IO.File]::ReadLines(
                $recordFile.FullName,
                $script:LocalRetentionStrictUtf8)) {
            if ($line.Length -gt 65536) {
                throw "The retention history contains an oversized event."
            }
            try {
                $eventRecord = $line | ConvertFrom-Json -ErrorAction Stop
            }
            catch {
                throw "The retention history contains an invalid event."
            }
            if ($eventRecord.event -cin @("COMPLETED", "RECOVERY_COMPLETED") -and
                $eventRecord.planSha256 -ceq $PlanSha256) {
                throw "The approved retention plan was already consumed by a completed transaction."
            }
        }
    }
}

function Write-LocalRetentionTransactionEvent {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$JournalPath, [Parameter(Mandatory)][string]$Event, [Parameter(Mandatory)][string]$PlanSha256, [hashtable]$Data = @{})

    $record = [ordered]@{ schemaVersion = 1; recordedAtUtc = [datetime]::UtcNow.ToString("o"); event = $Event; planSha256 = $PlanSha256; data = $Data }
    $bytes = $script:LocalRetentionUtf8.GetBytes((($record | ConvertTo-Json -Depth 8 -Compress) + "`n"))
    $stream = [System.IO.FileStream]::new($JournalPath, [System.IO.FileMode]::Append, [System.IO.FileAccess]::Write, [System.IO.FileShare]::Read, 4096, [System.IO.FileOptions]::WriteThrough)
    try { $stream.Write($bytes, 0, $bytes.Length); $stream.Flush($true) } finally { $stream.Dispose() }
}

function Assert-LocalRetentionDirectoryGuardIdentity {
    [CmdletBinding()]
    param([Parameter(Mandatory)]$GuardRecord)

    $GuardRecord.Handle.Refresh()
    if ($GuardRecord.Handle.IdentityToken -cne $GuardRecord.IdentityToken) {
        throw "A retention directory guard changed identity."
    }
    $current = [RagChallenge.LocalRetention.NativePathHandle]::OpenIdentity(
        $GuardRecord.Path,
        $true)
    try {
        if ($current.IdentityToken -cne $GuardRecord.IdentityToken) {
            throw "A guarded retention directory path changed identity."
        }
    }
    finally {
        $current.Dispose()
    }
}

function Assert-LocalRetentionTransactionGuards {
    [CmdletBinding()]
    param([Parameter(Mandatory)]$Transaction)

    foreach ($guard in $Transaction.DirectoryGuards) {
        Assert-LocalRetentionDirectoryGuardIdentity -GuardRecord $guard
    }
    foreach ($definition in @(
            [pscustomobject]@{
                Handle = $Transaction.RootMarkerHandle
                IdentityToken = $Transaction.RootMarkerIdentityToken
            },
            [pscustomobject]@{
                Handle = $Transaction.StagingMarkerHandle
                IdentityToken = $Transaction.StagingMarkerIdentityToken
            })) {
        if ($null -eq $definition.Handle) { continue }
        $definition.Handle.Refresh()
        if ($definition.Handle.IdentityToken -cne $definition.IdentityToken) {
            throw "A retention ownership marker changed identity."
        }
    }
}

function New-LocalRetentionTransaction {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$ResolvedRepositoryRoot, [Parameter(Mandatory)]$Plan, [Parameter(Mandatory)][object[]]$DeletionEntries)

    $transactionId = $Plan.planSha256.Substring(0, 16) + "-" + [guid]::NewGuid().ToString("N")
    $transactionRoot = Join-Path $ResolvedRepositoryRoot ("artifacts-local\retention-transactions\" + $transactionId)
    $relativeRoot = "artifacts-local/retention-transactions/$transactionId"
    New-LocalRetentionOwnedOutputRoot -OutputRoot $transactionRoot -RepositoryRoot $ResolvedRepositoryRoot -Purpose "local-retention-transaction" -Owner $script:LocalRetentionOwner -CanonicalRelativePath $relativeRoot
    $stagingRoot = Join-Path $transactionRoot "staged"
    New-LocalRetentionOwnedOutputRoot `
        -OutputRoot $stagingRoot `
        -RepositoryRoot $ResolvedRepositoryRoot `
        -Purpose "local-retention-staging" `
        -Owner $script:LocalRetentionOwner `
        -CanonicalRelativePath "$relativeRoot/staged"
    $directoryGuards = [System.Collections.Generic.List[object]]::new()
    $rootMarkerHandle = $null
    $stagingMarkerHandle = $null
    try {
        foreach ($guardPath in @(
                (Join-Path $ResolvedRepositoryRoot "artifacts-local"),
                (Join-Path $ResolvedRepositoryRoot "artifacts-local\retention-transactions"),
                $transactionRoot,
                $stagingRoot)) {
            $handle = [RagChallenge.LocalRetention.NativePathHandle]::OpenDirectoryGuard(
                $guardPath)
            $directoryGuards.Add([pscustomobject]@{
                    Path = $guardPath
                    IdentityToken = $handle.IdentityToken
                    Handle = $handle
                })
        }
        $rootMarkerHandle = [RagChallenge.LocalRetention.NativePathHandle]::OpenDeletion(
            (Join-Path $transactionRoot $script:LocalRetentionMarkerName),
            $false)
        $stagingMarkerHandle = [RagChallenge.LocalRetention.NativePathHandle]::OpenDeletion(
            (Join-Path $stagingRoot $script:LocalRetentionMarkerName),
            $false)
        $journalPath = Join-Path $transactionRoot "transaction.jsonl"
        Write-LocalRetentionTransactionEvent -JournalPath $journalPath -Event "PREPARED" -PlanSha256 $Plan.planSha256 -Data @{ baseline = $Plan.baseline; gitStatusSha256 = $Plan.gitStatusSha256; worktreeIdentitySha256 = $Plan.worktreeIdentitySha256; targets = @($DeletionEntries | ForEach-Object { [ordered]@{ relativePath = $_.RelativePath; byteLength = $_.ByteLength; structuralTreeSha256 = $_.StructuralTreeSha256 } }) }
        $transaction = [pscustomobject]@{
            Id = $transactionId
            Root = $transactionRoot
            StagingRoot = $stagingRoot
            DirectoryGuards = $directoryGuards
            RootMarkerHandle = $rootMarkerHandle
            RootMarkerIdentityToken = $rootMarkerHandle.IdentityToken
            StagingMarkerHandle = $stagingMarkerHandle
            StagingMarkerIdentityToken = $stagingMarkerHandle.IdentityToken
            JournalPath = $journalPath
        }
        Assert-LocalRetentionTransactionGuards -Transaction $transaction
        return $transaction
    }
    catch {
        if ($null -ne $stagingMarkerHandle) { $stagingMarkerHandle.Dispose() }
        if ($null -ne $rootMarkerHandle) { $rootMarkerHandle.Dispose() }
        foreach ($guard in $directoryGuards) { $guard.Handle.Dispose() }
        throw
    }
}

function Open-LocalRetentionExistingTransaction {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$ResolvedRepositoryRoot,
        [Parameter(Mandatory)]$RecoveryPlan
    )

    $relativeRoot = "artifacts-local/retention-transactions/" +
        $RecoveryPlan.transactionId
    Assert-LocalRetentionOwnershipMarker `
        -OutputRoot $RecoveryPlan.transactionRoot `
        -Purpose "local-retention-transaction" `
        -Owner $script:LocalRetentionOwner `
        -CanonicalRelativePath $relativeRoot
    Assert-LocalRetentionOwnershipMarker `
        -OutputRoot $RecoveryPlan.stagingRoot `
        -Purpose "local-retention-staging" `
        -Owner $script:LocalRetentionOwner `
        -CanonicalRelativePath "$relativeRoot/staged"
    $directoryGuards = [System.Collections.Generic.List[object]]::new()
    $rootMarkerHandle = $null
    $stagingMarkerHandle = $null
    try {
        foreach ($guardPath in @(
                (Join-Path $ResolvedRepositoryRoot "artifacts-local"),
                (Join-Path $ResolvedRepositoryRoot (
                        "artifacts-local\retention-transactions")),
                $RecoveryPlan.transactionRoot,
                $RecoveryPlan.stagingRoot)) {
            $handle = [RagChallenge.LocalRetention.NativePathHandle]::OpenDirectoryGuard(
                $guardPath)
            $directoryGuards.Add([pscustomobject]@{
                    Path = $guardPath
                    IdentityToken = $handle.IdentityToken
                    Handle = $handle
                })
        }
        $rootMarkerHandle =
            [RagChallenge.LocalRetention.NativePathHandle]::OpenDeletion(
                (Join-Path $RecoveryPlan.transactionRoot (
                        $script:LocalRetentionMarkerName)),
                $false)
        $stagingMarkerHandle =
            [RagChallenge.LocalRetention.NativePathHandle]::OpenDeletion(
                (Join-Path $RecoveryPlan.stagingRoot (
                        $script:LocalRetentionMarkerName)),
                $false)
        $transaction = [pscustomobject]@{
            Id = $RecoveryPlan.transactionId
            Root = $RecoveryPlan.transactionRoot
            StagingRoot = $RecoveryPlan.stagingRoot
            DirectoryGuards = $directoryGuards
            RootMarkerHandle = $rootMarkerHandle
            RootMarkerIdentityToken = $rootMarkerHandle.IdentityToken
            StagingMarkerHandle = $stagingMarkerHandle
            StagingMarkerIdentityToken = $stagingMarkerHandle.IdentityToken
            JournalPath = $RecoveryPlan.journalPath
        }
        Assert-LocalRetentionTransactionGuards -Transaction $transaction
        return $transaction
    }
    catch {
        if ($null -ne $stagingMarkerHandle) {
            $stagingMarkerHandle.Dispose()
        }
        if ($null -ne $rootMarkerHandle) {
            $rootMarkerHandle.Dispose()
        }
        foreach ($guard in $directoryGuards) {
            $guard.Handle.Dispose()
        }
        throw
    }
}

function Close-LocalRetentionTransaction {
    [CmdletBinding()]
    param([Parameter(Mandatory)]$Transaction, [Parameter(Mandatory)][string]$ResolvedRepositoryRoot, [Parameter(Mandatory)][string]$PlanSha256, [Parameter(Mandatory)][string]$Outcome)

    Write-LocalRetentionTransactionEvent -JournalPath $Transaction.JournalPath -Event $Outcome -PlanSha256 $PlanSha256
    Assert-LocalRetentionTransactionGuards -Transaction $Transaction
    $remainingStagedItems = @(Get-ChildItem -LiteralPath $Transaction.StagingRoot -Force |
        Where-Object Name -cne $script:LocalRetentionMarkerName)
    if ($remainingStagedItems.Count -gt 0) { throw "The retention transaction cannot close while staged content remains." }
    $rootItems = @(Get-ChildItem -LiteralPath $Transaction.Root -Force)
    $expectedRootNames = @(
        $script:LocalRetentionMarkerName,
        "staged",
        "transaction.jsonl")
    if (@($rootItems | Where-Object Name -cnotin $expectedRootNames).Count -gt 0 -or
        $rootItems.Count -ne $expectedRootNames.Count) {
        throw "The retention transaction contains unexpected infrastructure."
    }
    $historyPath = Join-Path $ResolvedRepositoryRoot ("artifacts-local\retention-history\$($Transaction.Id).jsonl")
    [System.IO.File]::Move($Transaction.JournalPath, $historyPath)
    Assert-LocalRetentionTransactionGuards -Transaction $Transaction
    $Transaction.StagingMarkerHandle.MarkDelete()
    $Transaction.StagingMarkerHandle.Dispose()
    $Transaction.StagingMarkerHandle = $null
    $Transaction.RootMarkerHandle.MarkDelete()
    $Transaction.RootMarkerHandle.Dispose()
    $Transaction.RootMarkerHandle = $null
    $stagingGuard = @($Transaction.DirectoryGuards |
        Where-Object Path -ceq $Transaction.StagingRoot)[0]
    $rootGuard = @($Transaction.DirectoryGuards |
        Where-Object Path -ceq $Transaction.Root)[0]
    $stagingGuard.Handle.MarkDelete()
    $stagingGuard.Handle.Dispose()
    $rootGuard.Handle.MarkDelete()
    $rootGuard.Handle.Dispose()
    foreach ($guard in $Transaction.DirectoryGuards) {
        $guard.Handle.Dispose()
    }
    if ($null -ne (Get-Item -LiteralPath $Transaction.Root -Force -ErrorAction SilentlyContinue)) {
        throw "Completed retention infrastructure could not be removed by handle."
    }
    return $historyPath
}

function Remove-LocalRetentionStagedTarget {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$ExpectedStructuralTreeSha256,
        [Parameter(Mandatory)][long]$ExpectedByteLength,
        [scriptblock]$BeforeDispositionTestHook,
        [scriptblock]$AfterDeletePendingTestHook
    )

    Assert-LocalRetentionTreeIsSafe -Root $Path
    $rootItem = Get-Item -LiteralPath $Path -Force -ErrorAction Stop
    if (-not $rootItem.PSIsContainer) {
        throw "A staged retention target is not a directory."
    }
    $initialChildren = @(Get-ChildItem -LiteralPath $Path -Recurse -Force |
        Sort-Object FullName)
    $directoryItems = @($rootItem) + @($initialChildren |
        Where-Object PSIsContainer |
        Sort-Object FullName)
    $fileItems = @($initialChildren |
        Where-Object { -not $_.PSIsContainer } |
        Sort-Object FullName)
    $locked = [System.Collections.Generic.List[object]]::new()
    try {
        foreach ($directory in $directoryItems) {
            $handle = [RagChallenge.LocalRetention.NativePathHandle]::OpenDeletion(
                $directory.FullName,
                $true)
            $locked.Add([pscustomobject]@{
                    FullName = $directory.FullName
                    RelativePath = if ([string]::Equals(
                            $directory.FullName,
                            $rootItem.FullName,
                            $script:LocalRetentionPathComparison)) { "." } else {
                        [System.IO.Path]::GetRelativePath(
                            $Path,
                            $directory.FullName).Replace('\', '/')
                    }
                    IsDirectory = $true
                    IdentityToken = $handle.IdentityToken
                    Handle = $handle
                })
        }
        foreach ($file in $fileItems) {
            $handle = [RagChallenge.LocalRetention.NativePathHandle]::OpenDeletion(
                $file.FullName,
                $false)
            $locked.Add([pscustomobject]@{
                    FullName = $file.FullName
                    RelativePath = [System.IO.Path]::GetRelativePath(
                        $Path,
                        $file.FullName).Replace('\', '/')
                    IsDirectory = $false
                    IdentityToken = $handle.IdentityToken
                    Handle = $handle
                })
        }

        if ($null -ne $BeforeDispositionTestHook) {
            & $BeforeDispositionTestHook $Path
        }

        $currentItems = @($rootItem) + @(
            Get-ChildItem -LiteralPath $Path -Recurse -Force |
            Sort-Object FullName)
        if ($currentItems.Count -ne $locked.Count) {
            throw "The staged retention target path set changed after deletion locks were acquired."
        }
        $lockedByPath = [System.Collections.Generic.Dictionary[string, object]]::new(
            [System.StringComparer]::OrdinalIgnoreCase)
        foreach ($record in $locked) {
            $lockedByPath.Add($record.FullName, $record)
        }
        foreach ($currentItem in $currentItems) {
            if (-not $lockedByPath.ContainsKey($currentItem.FullName)) {
                throw "The staged retention target path set changed after deletion locks were acquired."
            }
            $record = $lockedByPath[$currentItem.FullName]
            $record.Handle.EnsureNoNamedDataStreamsOnHandle()
            $currentHandle = [RagChallenge.LocalRetention.NativePathHandle]::OpenIdentity(
                $currentItem.FullName,
                [bool]$currentItem.PSIsContainer)
            try {
                if ($currentHandle.IdentityToken -cne $record.IdentityToken) {
                    throw "A staged retention path changed filesystem identity."
                }
            }
            finally {
                $currentHandle.Dispose()
            }
        }

        $records = [System.Text.StringBuilder]::new()
        $byteLength = [long]0
        foreach ($record in @($locked | Sort-Object FullName)) {
            $record.Handle.Refresh()
            if ($record.Handle.IdentityToken -cne $record.IdentityToken) {
                throw "A staged retention handle changed filesystem identity."
            }
            if ($record.IsDirectory) {
                [void]$records.Append(
                    "D`0$($record.RelativePath)`0$($record.Handle.CreationTimeTicks)`0" +
                    "$($record.Handle.LastWriteTimeTicks)`0$($record.Handle.Attributes)`0" +
                    "$($record.Handle.VolumeSerialNumberHex)`0$($record.Handle.FileIdHex)`n")
            }
            else {
                $byteLength += $record.Handle.Length
                [void]$records.Append(
                    "F`0$($record.RelativePath)`0$($record.Handle.Length)`0" +
                    "$($record.Handle.CreationTimeTicks)`0$($record.Handle.LastWriteTimeTicks)`0" +
                    "$($record.Handle.ChangeTimeTicks)`0" +
                    "$($record.Handle.Attributes)`0$($record.Handle.VolumeSerialNumberHex)`0" +
                    "$($record.Handle.FileIdHex)`n")
            }
        }
        $structuralTreeSha256 = Get-LocalRetentionSha256 -Value $records.ToString()
        if ($structuralTreeSha256 -cne $ExpectedStructuralTreeSha256 -or
            $byteLength -ne $ExpectedByteLength) {
            throw "The staged retention target changed while acquiring deletion handles."
        }

        $fileRecords = @($locked |
            Where-Object { -not $_.IsDirectory } |
            Sort-Object FullName)
        $armedFileRecords = [System.Collections.Generic.List[object]]::new()
        try {
            foreach ($record in $fileRecords) {
                $record.Handle.ArmDeletePending()
                $armedFileRecords.Add($record)
            }
            if ($null -ne $AfterDeletePendingTestHook) {
                & $AfterDeletePendingTestHook $Path
            }
            foreach ($record in $armedFileRecords) {
                $record.Handle.Refresh()
                if ($record.Handle.IdentityToken -cne $record.IdentityToken) {
                    throw "A staged retention handle changed filesystem identity after deletion was armed."
                }
                $record.Handle.EnsureNoNamedDataStreamsOnHandle()
            }
        }
        catch {
            $deletionError = $_
            $rollbackFailures = [System.Collections.Generic.List[string]]::new()
            foreach ($record in $armedFileRecords) {
                try {
                    $record.Handle.ClearDeletePending()
                }
                catch {
                    $rollbackFailures.Add($record.RelativePath)
                }
            }
            if ($rollbackFailures.Count -gt 0) {
                throw (
                    "Reversible file deletion could not be cancelled for: " +
                    (($rollbackFailures | Sort-Object) -join ", ") +
                    ". Preserve the retention transaction for recovery. Original error: " +
                    $deletionError.Exception.Message)
            }
            throw $deletionError
        }
        foreach ($record in $armedFileRecords) {
            $record.Handle.Dispose()
        }
        foreach ($record in @($locked |
                Where-Object IsDirectory |
                Sort-Object @{ Expression = { $_.FullName.Length }; Descending = $true }, FullName)) {
            $directoryArmed = $false
            try {
                $record.Handle.EnsureNoNamedDataStreamsOnHandle()
                $record.Handle.ArmDeletePending()
                $directoryArmed = $true
                $record.Handle.EnsureNoNamedDataStreamsOnHandle()
                $record.Handle.Dispose()
                $directoryArmed = $false
            }
            catch {
                $deletionError = $_
                if ($directoryArmed) {
                    try {
                        $record.Handle.ClearDeletePending()
                        $directoryArmed = $false
                    }
                    catch {
                        throw (
                            "Reversible directory deletion could not be cancelled for " +
                            "$($record.RelativePath). Preserve the retention transaction " +
                            "for recovery. Original error: " +
                            $deletionError.Exception.Message)
                    }
                }
                throw $deletionError
            }
        }
    }
    finally {
        foreach ($record in $locked) {
            $record.Handle.Dispose()
        }
    }
    if ($null -ne (Get-Item -LiteralPath $Path -Force -ErrorAction SilentlyContinue)) {
        throw "The staged target was not removed by its approved handles."
    }
}

function Test-LocalRetentionRecoveryPlanIdentity {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]$Expected,
        [Parameter(Mandatory)]$Actual
    )

    return $Actual.recoveryPlanSha256 -ceq $Expected.recoveryPlanSha256 -and
        $Actual.journalSha256 -ceq $Expected.journalSha256 -and
        $Actual.baseline -ceq $Expected.baseline -and
        $Actual.gitStatusSha256 -ceq $Expected.gitStatusSha256 -and
        $Actual.worktreeIdentitySha256 -ceq
            $Expected.worktreeIdentitySha256 -and
        $Actual.executorSha256 -ceq $Expected.executorSha256 -and
        $Actual.gitExecutableSha256 -ceq $Expected.gitExecutableSha256 -and
        $Actual.protectedBoundarySha256 -ceq
            $Expected.protectedBoundarySha256
}

function Invoke-LocalRetentionRecovery {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$RepositoryRoot,
        [Parameter(Mandatory)][string]$TransactionId,
        [switch]$ApplyRecovery,
        [string]$ApprovedRecoveryPlanSha256,
        [string]$ApprovedRecoveryJournalSha256,
        [string]$ApprovedGitStatusSha256,
        [string]$ApprovedWorktreeIdentitySha256
    )

    $plan = Get-LocalRetentionRecoveryPlan `
        -RepositoryRoot $RepositoryRoot `
        -TransactionId $TransactionId
    if (-not $ApplyRecovery) {
        return $plan
    }
    $approvalValues = @(
        $ApprovedRecoveryPlanSha256,
        $ApprovedRecoveryJournalSha256,
        $ApprovedGitStatusSha256,
        $ApprovedWorktreeIdentitySha256)
    if (@($approvalValues | Where-Object {
                [string]::IsNullOrWhiteSpace($_)
            }).Count -gt 0) {
        throw "Recovery Apply requires every owner-approved recovery, journal, Git-status and WIP-identity SHA-256 value."
    }
    if ($plan.blocked) {
        throw "Recovery Apply was refused because the transaction is uncertain."
    }
    if ($ApprovedRecoveryPlanSha256 -cne $plan.recoveryPlanSha256 -or
        $ApprovedRecoveryJournalSha256 -cne $plan.journalSha256 -or
        $ApprovedGitStatusSha256 -cne $plan.gitStatusSha256 -or
        $ApprovedWorktreeIdentitySha256 -cne
            $plan.worktreeIdentitySha256) {
        throw "Recovery Apply approval does not match the current transaction boundary."
    }
    if (-not [System.OperatingSystem]::IsWindows()) {
        throw "Recovery Apply requires the Windows handle-bound boundary."
    }

    $recoveryEntries = @($plan.entries | Where-Object {
            $_.disposition -cin @(
                "RECOVERY_DELETE_EMPTY_PARTIAL_ROOT_REQUIRES_APPROVAL",
                "RECOVERY_DELETE_INTACT_TARGET_REQUIRES_APPROVAL")
        } | Sort-Object index)
    if ($recoveryEntries.Count -ne $plan.recoveryTargetCount) {
        throw "The recovery target set is internally inconsistent."
    }
    $processEntries = @($recoveryEntries | ForEach-Object {
            [pscustomobject]@{
                Path = $_.stagedPath
                RelativePath = $_.relativePath
            }
        })
    $gitStateForLocks = Get-LocalRetentionGitState `
        -ResolvedRepositoryRoot $plan.repositoryRoot
    $mutex = $null
    $wipLocks = $null
    $transaction = $null
    $recoveryStarted = $false
    $deleted = [System.Collections.Generic.List[object]]::new()
    $freeBefore = [long]0
    try {
        $mutex = New-LocalRetentionMutex `
            -ResolvedRepositoryRoot $plan.repositoryRoot
        $wipLocks = Open-LocalRetentionWorktreeLocks `
            -Paths $gitStateForLocks.WipLockPaths
        $lockedPlan = Get-LocalRetentionRecoveryPlan `
            -RepositoryRoot $plan.repositoryRoot `
            -TransactionId $TransactionId
        if (-not (Test-LocalRetentionRecoveryPlanIdentity `
                    -Expected $plan `
                    -Actual $lockedPlan)) {
            throw "The approved recovery boundary changed before deletion."
        }
        Assert-NoLocalRetentionTargetUse `
            -ResolvedRepositoryRoot $plan.repositoryRoot `
            -DeletionEntries $processEntries
        $immediatePlan = Get-LocalRetentionRecoveryPlan `
            -RepositoryRoot $plan.repositoryRoot `
            -TransactionId $TransactionId
        if (-not (Test-LocalRetentionRecoveryPlanIdentity `
                    -Expected $plan `
                    -Actual $immediatePlan)) {
            throw "The approved recovery boundary changed before deletion."
        }
        Assert-NoLocalRetentionTargetUse `
            -ResolvedRepositoryRoot $plan.repositoryRoot `
            -DeletionEntries $processEntries
        $transaction = Open-LocalRetentionExistingTransaction `
            -ResolvedRepositoryRoot $plan.repositoryRoot `
            -RecoveryPlan $plan
        Assert-LocalRetentionTransactionGuards -Transaction $transaction
        $currentJournalIdentity = Get-LocalRetentionPathIdentity `
            -Path $transaction.JournalPath `
            -Directory $false
        $currentJournalSha256 = (Get-FileHash `
                -LiteralPath $transaction.JournalPath `
                -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($currentJournalSha256 -cne $plan.journalSha256 -or
            $currentJournalIdentity.identityToken -cne
                $plan.journalIdentity.identityToken) {
            throw "The recovery journal changed before deletion."
        }
        $freeBefore = [System.IO.DriveInfo]::new(
            [System.IO.Path]::GetPathRoot(
                $plan.repositoryRoot)).AvailableFreeSpace
        Write-LocalRetentionTransactionEvent `
            -JournalPath $transaction.JournalPath `
            -Event "RECOVERY_STARTED" `
            -PlanSha256 $plan.originalPlanSha256 `
            -Data @{
                recoveryPlanSha256 = $plan.recoveryPlanSha256
                approvedJournalSha256 = $plan.journalSha256
            }
        $recoveryStarted = $true

        foreach ($entry in $recoveryEntries) {
            if ($null -ne (Get-Item `
                    -LiteralPath $entry.originalPath `
                    -Force `
                    -ErrorAction SilentlyContinue)) {
                throw "An original recovery path was recreated before deletion."
            }
            $measurement = Get-LocalRetentionTreeMeasurement `
                -TargetPath $entry.stagedPath
            $identity = Get-LocalRetentionPathIdentity `
                -Path $entry.stagedPath `
                -Directory $true
            if ($measurement.ByteLength -ne $entry.currentByteLength -or
                $measurement.StructuralTreeSha256 -cne
                    $entry.currentStructuralTreeSha256 -or
                $identity.identityToken -cne
                    $entry.currentIdentity.identityToken) {
                throw "A recovery target changed after owner approval."
            }
            Write-LocalRetentionTransactionEvent `
                -JournalPath $transaction.JournalPath `
                -Event "RECOVERY_DELETE_STARTED" `
                -PlanSha256 $plan.originalPlanSha256 `
                -Data @{
                    recoveryPlanSha256 = $plan.recoveryPlanSha256
                    relativePath = $entry.relativePath
                    stagedPath = $entry.stagedPath
                    byteLength = $entry.currentByteLength
                    structuralTreeSha256 =
                        $entry.currentStructuralTreeSha256
                }
            try {
                Remove-LocalRetentionStagedTarget `
                    -Path $entry.stagedPath `
                    -ExpectedStructuralTreeSha256 (
                        $entry.currentStructuralTreeSha256) `
                    -ExpectedByteLength $entry.currentByteLength
                $deleted.Add([pscustomobject][ordered]@{
                        relativePath = $entry.relativePath
                        stagedPath = $entry.stagedPath
                        logicalBytesRemoved = $entry.currentByteLength
                        recoverability =
                            "Original content is regenerable but not recoverable as the same ephemeral bytes."
                    })
                Write-LocalRetentionTransactionEvent `
                    -JournalPath $transaction.JournalPath `
                    -Event "RECOVERY_DELETED" `
                    -PlanSha256 $plan.originalPlanSha256 `
                    -Data @{
                        recoveryPlanSha256 = $plan.recoveryPlanSha256
                        relativePath = $entry.relativePath
                        byteLength = $entry.currentByteLength
                    }
            }
            catch {
                $remaining = @{ state = "UNKNOWN_PARTIAL" }
                try {
                    $inventory = Get-LocalRetentionTreeInventory `
                        -TargetPath $entry.stagedPath
                    $remaining = @{
                        state = "MEASURED_PARTIAL"
                        exists = $inventory.Exists
                        fileCount = $inventory.FileCount
                        directoryCount = $inventory.DirectoryCount
                        byteLength = $inventory.ByteLength
                    }
                }
                catch {
                    # An unsafe recovery remainder stays explicitly unknown.
                }
                Write-LocalRetentionTransactionEvent `
                    -JournalPath $transaction.JournalPath `
                    -Event "RECOVERY_PARTIAL_DELETE_FAILURE" `
                    -PlanSha256 $plan.originalPlanSha256 `
                    -Data @{
                        recoveryPlanSha256 = $plan.recoveryPlanSha256
                        failedRelativePath = $entry.relativePath
                        deletedRelativePaths = @($deleted |
                            ForEach-Object relativePath)
                        remaining = $remaining
                    }
                throw "Recovery stopped after a partial deletion; preserve the transaction journal."
            }
        }

        $gitAfter = Get-LocalRetentionGitState `
            -ResolvedRepositoryRoot $plan.repositoryRoot
        if ($gitAfter.Head -cne $plan.baseline -or
            $gitAfter.StatusSha256 -cne $plan.gitStatusSha256 -or
            $gitAfter.WorktreeIdentitySha256 -cne
                $plan.worktreeIdentitySha256) {
            Write-LocalRetentionTransactionEvent `
                -JournalPath $transaction.JournalPath `
                -Event "RECOVERY_POST_VALIDATION_FAILED" `
                -PlanSha256 $plan.originalPlanSha256 `
                -Data @{ boundary = "GitWip" }
            throw "Git-visible WIP changed during recovery."
        }
        $protectedAfter = @(Get-LocalRetentionProtectedBoundary `
                -ResolvedRepositoryRoot $plan.repositoryRoot)
        $protectedAfterSha256 = Get-LocalRetentionSha256 -Value (
            $protectedAfter | ConvertTo-Json -Depth 5 -Compress)
        if ($protectedAfterSha256 -cne $plan.protectedBoundarySha256) {
            Write-LocalRetentionTransactionEvent `
                -JournalPath $transaction.JournalPath `
                -Event "RECOVERY_POST_VALIDATION_FAILED" `
                -PlanSha256 $plan.originalPlanSha256 `
                -Data @{ boundary = "ProtectedPaths" }
            throw "A protected boundary changed during recovery."
        }
        $historyPath = Close-LocalRetentionTransaction `
            -Transaction $transaction `
            -ResolvedRepositoryRoot $plan.repositoryRoot `
            -PlanSha256 $plan.originalPlanSha256 `
            -Outcome "RECOVERY_COMPLETED"
        $transaction = $null
        $freeAfter = [System.IO.DriveInfo]::new(
            [System.IO.Path]::GetPathRoot(
                $plan.repositoryRoot)).AvailableFreeSpace
        $logicalBytesRemoved = if ($deleted.Count -eq 0) {
            [long]0
        }
        else {
            [long](($deleted | Measure-Object `
                        logicalBytesRemoved `
                        -Sum).Sum)
        }
        return [pscustomobject][ordered]@{
            schemaVersion = 1
            mode = "RECOVERY_APPLY"
            approvedRecoveryPlanSha256 = $plan.recoveryPlanSha256
            approvedJournalSha256 = $plan.journalSha256
            preservedGitStatusSha256 = $plan.gitStatusSha256
            preservedWorktreeIdentitySha256 =
                $plan.worktreeIdentitySha256
            deletedTargetCount = $deleted.Count
            logicalBytesRemoved = $logicalBytesRemoved
            previouslyDeletedBytes = $plan.previouslyDeletedBytes
            observedFreeSpaceIncrease = [long]($freeAfter - $freeBefore)
            protectedPathsPreserved = $true
            transactionRecordPath = $historyPath
            deleted = @($deleted)
        }
    }
    catch {
        if ($recoveryStarted -and $null -ne $transaction -and
            (Test-Path -LiteralPath $transaction.JournalPath)) {
            try {
                Write-LocalRetentionTransactionEvent `
                    -JournalPath $transaction.JournalPath `
                    -Event "RECOVERY_APPLY_FAILED" `
                    -PlanSha256 $plan.originalPlanSha256 `
                    -Data @{
                        recoveryPlanSha256 = $plan.recoveryPlanSha256
                        deletedTargetCount = $deleted.Count
                    }
            }
            catch {
                throw "Recovery failed and its journal could not be finalised; preserve the transaction."
            }
        }
        throw
    }
    finally {
        if ($null -ne $transaction) {
            if ($null -ne $transaction.StagingMarkerHandle) {
                $transaction.StagingMarkerHandle.Dispose()
                $transaction.StagingMarkerHandle = $null
            }
            if ($null -ne $transaction.RootMarkerHandle) {
                $transaction.RootMarkerHandle.Dispose()
                $transaction.RootMarkerHandle = $null
            }
            foreach ($guard in $transaction.DirectoryGuards) {
                $guard.Handle.Dispose()
            }
        }
        if ($null -ne $wipLocks) {
            foreach ($handle in $wipLocks) {
                $handle.Dispose()
            }
        }
        if ($null -ne $mutex) {
            try {
                $mutex.ReleaseMutex()
            }
            catch {
            }
            $mutex.Dispose()
        }
    }
}

function Test-LocalRetentionPlanIdentity {
    [CmdletBinding()]
    param([Parameter(Mandatory)]$Expected, [Parameter(Mandatory)]$Actual)
    return $Actual.planSha256 -ceq $Expected.planSha256 -and $Actual.baseline -ceq $Expected.baseline -and $Actual.gitStatusSha256 -ceq $Expected.gitStatusSha256 -and $Actual.worktreeIdentitySha256 -ceq $Expected.worktreeIdentitySha256 -and $Actual.executorSha256 -ceq $Expected.executorSha256 -and $Actual.gitExecutableSha256 -ceq $Expected.gitExecutableSha256 -and $Actual.legacyOwnershipAttestationSha256 -ceq $Expected.legacyOwnershipAttestationSha256
}

function Invoke-LocalArtefactRetention {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$RepositoryRoot, [switch]$Apply, [string]$ApprovedPlanSha256, [string]$ApprovedGitStatusSha256, [string]$ApprovedWorktreeIdentitySha256, [string]$ApprovedLegacyOwnershipAttestationSha256)

    $plan = Get-LocalArtefactRetentionPlan -RepositoryRoot $RepositoryRoot
    if (-not $Apply) { return $plan }
    $approvalValues = @(
        $ApprovedPlanSha256,
        $ApprovedGitStatusSha256,
        $ApprovedWorktreeIdentitySha256,
        $ApprovedLegacyOwnershipAttestationSha256)
    if (@($approvalValues | Where-Object {
                [string]::IsNullOrWhiteSpace($_)
            }).Count -gt 0) {
        throw "Apply requires every owner-approved plan, Git, worktree and ownership-attestation SHA-256 value."
    }
    if ($plan.blocked) { throw "Apply was refused because at least one retention boundary is uncertain." }
    if ($ApprovedPlanSha256 -cne $plan.planSha256) { throw "The approved retention plan SHA-256 does not match the current plan." }
    if ($ApprovedGitStatusSha256 -cne $plan.gitStatusSha256) { throw "The approved Git-status SHA-256 does not match the current worktree." }
    if ($ApprovedWorktreeIdentitySha256 -cne $plan.worktreeIdentitySha256) { throw "The approved worktree-identity SHA-256 does not match the current preserved WIP structure." }
    if ($ApprovedLegacyOwnershipAttestationSha256 -cne $plan.legacyOwnershipAttestationSha256) { throw "The approved legacy ownership attestation does not match the exact deletion trees." }
    if (-not [System.OperatingSystem]::IsWindows()) { throw "Apply is supported only where the Windows process-use boundary is available." }

    $deletionEntries = @($plan.entries | Where-Object Disposition -ceq "DELETE_CANDIDATE_REQUIRES_ATTESTATION")
    $gitStateForLocks = Get-LocalRetentionGitState -ResolvedRepositoryRoot $plan.repositoryRoot
    $mutex = $null; $wipLocks = $null; $transaction = $null
    $deleted = [System.Collections.Generic.List[object]]::new(); $freeBefore = [long]0
    try {
        $mutex = New-LocalRetentionMutex -ResolvedRepositoryRoot $plan.repositoryRoot
        $wipLocks = Open-LocalRetentionWorktreeLocks -Paths $gitStateForLocks.WipLockPaths
        $lockedPlan = Get-LocalArtefactRetentionPlan -RepositoryRoot $plan.repositoryRoot
        if (-not (Test-LocalRetentionPlanIdentity -Expected $plan -Actual $lockedPlan)) { throw "The approved retention boundary changed before staging; all targets were preserved." }
        Assert-NoLocalRetentionTargetUse -ResolvedRepositoryRoot $plan.repositoryRoot -DeletionEntries $deletionEntries
        $immediatePlan = Get-LocalArtefactRetentionPlan -RepositoryRoot $plan.repositoryRoot
        if (-not (Test-LocalRetentionPlanIdentity -Expected $plan -Actual $immediatePlan)) { throw "The approved retention boundary changed before staging; all targets were preserved." }
        Assert-NoLocalRetentionTargetUse `
            -ResolvedRepositoryRoot $plan.repositoryRoot `
            -DeletionEntries $deletionEntries

        Initialise-LocalRetentionTransactionInfrastructure -ResolvedRepositoryRoot $plan.repositoryRoot
        Assert-LocalRetentionPlanNotConsumed `
            -ResolvedRepositoryRoot $plan.repositoryRoot `
            -PlanSha256 $plan.planSha256
        $protectedBefore = @(Get-LocalRetentionProtectedBoundary -ResolvedRepositoryRoot $plan.repositoryRoot)
        $protectedBeforeSha256 = Get-LocalRetentionSha256 -Value ($protectedBefore | ConvertTo-Json -Depth 5 -Compress)
        $freeBefore = [System.IO.DriveInfo]::new([System.IO.Path]::GetPathRoot($plan.repositoryRoot)).AvailableFreeSpace
        $transaction = New-LocalRetentionTransaction -ResolvedRepositoryRoot $plan.repositoryRoot -Plan $plan -DeletionEntries $deletionEntries

        $staged = [System.Collections.Generic.List[object]]::new()
        try {
            for ($index = 0; $index -lt $deletionEntries.Count; $index++) {
                $entry = $deletionEntries[$index]
                $candidate = Get-LocalRetentionCandidates | Where-Object RelativePath -ceq $entry.RelativePath | Select-Object -First 1
                $source = Resolve-LocalRetentionPath -ResolvedRepositoryRoot $plan.repositoryRoot -RelativePath $entry.RelativePath
                Assert-LocalRetentionGitBoundary -ResolvedRepositoryRoot $plan.repositoryRoot -RelativePath $entry.RelativePath
                Assert-LocalRetentionOwnership -Candidate $candidate -TargetPath $source
                Assert-NoLocalRetentionSensitiveContent -TargetPath $source
                $measurement = Get-LocalRetentionTreeMeasurement -TargetPath $source
                if ($measurement.StructuralTreeSha256 -cne $entry.StructuralTreeSha256 -or $measurement.ByteLength -ne $entry.ByteLength) { throw "A retention target changed after approval." }
                $destination = Join-Path $transaction.StagingRoot (("{0:D3}-" -f $index) + (Get-LocalRetentionSha256 -Value $entry.RelativePath).Substring(0, 16))
                if ([System.IO.Path]::GetPathRoot($source) -cne [System.IO.Path]::GetPathRoot($destination)) { throw "The retention staging path is not on the source volume." }
                Assert-LocalRetentionTransactionGuards -Transaction $transaction
                Assert-LocalRetentionExistingComponentsAreSafe `
                    -RepositoryRoot $plan.repositoryRoot `
                    -Path $destination
                Assert-LocalRetentionTransactionGuards -Transaction $transaction
                [System.IO.Directory]::Move($source, $destination)
                Assert-LocalRetentionTransactionGuards -Transaction $transaction
                $moved = [pscustomobject]@{ Entry = $entry; Source = $source; Destination = $destination }; $staged.Add($moved)
                Assert-LocalRetentionExistingComponentsAreSafe -RepositoryRoot $plan.repositoryRoot -Path $destination
                $stagedMeasurement = Get-LocalRetentionTreeMeasurement -TargetPath $destination
                if ($stagedMeasurement.StructuralTreeSha256 -cne $entry.StructuralTreeSha256 -or $stagedMeasurement.ByteLength -ne $entry.ByteLength) { throw "A staged retention target differs from the approved tree." }
                Write-LocalRetentionTransactionEvent -JournalPath $transaction.JournalPath -Event "STAGED" -PlanSha256 $plan.planSha256 -Data @{ relativePath = $entry.RelativePath; byteLength = $entry.ByteLength; structuralTreeSha256 = $entry.StructuralTreeSha256 }
            }
        }
        catch {
            Write-LocalRetentionTransactionEvent -JournalPath $transaction.JournalPath -Event "STAGING_FAILED" -PlanSha256 $plan.planSha256 -Data @{ stagedCount = $staged.Count }
            $recoveryRequired = $false
            for ($index = $staged.Count - 1; $index -ge 0; $index--) {
                $moved = $staged[$index]
                try {
                    Assert-LocalRetentionExistingComponentsAreSafe `
                        -RepositoryRoot $plan.repositoryRoot `
                        -Path $moved.Destination
                    $measurement = Get-LocalRetentionTreeMeasurement -TargetPath $moved.Destination
                    if ($null -ne (Get-Item -LiteralPath $moved.Source -Force -ErrorAction SilentlyContinue) -or $measurement.StructuralTreeSha256 -cne $moved.Entry.StructuralTreeSha256 -or $measurement.ByteLength -ne $moved.Entry.ByteLength) { throw "Rollback identity could not be established." }
                    Assert-LocalRetentionTransactionGuards -Transaction $transaction
                    [System.IO.Directory]::Move($moved.Destination, $moved.Source)
                    Assert-LocalRetentionTransactionGuards -Transaction $transaction
                    Write-LocalRetentionTransactionEvent -JournalPath $transaction.JournalPath -Event "ROLLED_BACK" -PlanSha256 $plan.planSha256 -Data @{ relativePath = $moved.Entry.RelativePath }
                }
                catch { $recoveryRequired = $true; Write-LocalRetentionTransactionEvent -JournalPath $transaction.JournalPath -Event "RECOVERY_REQUIRED" -PlanSha256 $plan.planSha256 -Data @{ relativePath = $moved.Entry.RelativePath } }
            }
            if (-not $recoveryRequired) {
                $historyPath = Close-LocalRetentionTransaction -Transaction $transaction -ResolvedRepositoryRoot $plan.repositoryRoot -PlanSha256 $plan.planSha256 -Outcome "ABORTED_BEFORE_DELETE"
                $transaction = $null
                throw "Retention staging failed; every moved target was restored and no deletion was committed. Record: $historyPath"
            }
            throw "Retention staging failed and a quarantined target requires recovery. Record: $($transaction.JournalPath)"
        }

        foreach ($moved in $staged) {
            $measurement = Get-LocalRetentionTreeMeasurement -TargetPath $moved.Destination
            if ($measurement.StructuralTreeSha256 -cne $moved.Entry.StructuralTreeSha256 -or $measurement.ByteLength -ne $moved.Entry.ByteLength) {
                Write-LocalRetentionTransactionEvent -JournalPath $transaction.JournalPath -Event "RECOVERY_REQUIRED" -PlanSha256 $plan.planSha256 -Data @{ relativePath = $moved.Entry.RelativePath }
                throw "A staged target changed before deletion and remains in quarantine."
            }
        }

        foreach ($moved in $staged) {
            Write-LocalRetentionTransactionEvent -JournalPath $transaction.JournalPath -Event "DELETE_STARTED" -PlanSha256 $plan.planSha256 -Data @{ relativePath = $moved.Entry.RelativePath }
            try {
                Remove-LocalRetentionStagedTarget `
                    -Path $moved.Destination `
                    -ExpectedStructuralTreeSha256 $moved.Entry.StructuralTreeSha256 `
                    -ExpectedByteLength $moved.Entry.ByteLength
                if ($null -ne (Get-Item -LiteralPath $moved.Destination -Force -ErrorAction SilentlyContinue)) { throw "The staged target still exists." }
                $deleted.Add([pscustomobject][ordered]@{ Path = $moved.Entry.Path; RelativePath = $moved.Entry.RelativePath; ByteLength = $moved.Entry.ByteLength; Reason = $moved.Entry.Reason; Recoverability = $moved.Entry.Recoverability; RecoveryType = "Regenerable, not recoverable as the same ephemeral bytes" })
                Write-LocalRetentionTransactionEvent -JournalPath $transaction.JournalPath -Event "DELETED" -PlanSha256 $plan.planSha256 -Data @{ relativePath = $moved.Entry.RelativePath; byteLength = $moved.Entry.ByteLength }
            }
            catch {
                $remaining = @{ state = "UNKNOWN_PARTIAL" }
                try {
                    $remainingInventory = Get-LocalRetentionTreeInventory `
                        -TargetPath $moved.Destination
                    $remaining = @{
                        state = "MEASURED_PARTIAL"
                        exists = $remainingInventory.Exists
                        fileCount = $remainingInventory.FileCount
                        directoryCount = $remainingInventory.DirectoryCount
                        byteLength = $remainingInventory.ByteLength
                    }
                }
                catch {
                    # An unsafe or unmeasurable remainder stays explicitly unknown.
                }
                Write-LocalRetentionTransactionEvent -JournalPath $transaction.JournalPath -Event "PARTIAL_DELETE_FAILURE" -PlanSha256 $plan.planSha256 -Data @{ failedRelativePath = $moved.Entry.RelativePath; deletedRelativePaths = @($deleted | ForEach-Object RelativePath); remaining = $remaining }
                throw "Retention stopped after a partial material deletion; the durable transaction record requires review: $($transaction.JournalPath)"
            }
        }

        $gitAfter = Get-LocalRetentionGitState -ResolvedRepositoryRoot $plan.repositoryRoot
        if ($gitAfter.Head -cne $plan.baseline -or $gitAfter.StatusSha256 -cne $plan.gitStatusSha256 -or $gitAfter.WorktreeIdentitySha256 -cne $plan.worktreeIdentitySha256) {
            Write-LocalRetentionTransactionEvent -JournalPath $transaction.JournalPath -Event "POST_VALIDATION_FAILED" -PlanSha256 $plan.planSha256 -Data @{ boundary = "GitWip" }
            throw "Git-visible WIP changed during retention; the durable transaction record requires review."
        }
        $protectedAfter = @(Get-LocalRetentionProtectedBoundary -ResolvedRepositoryRoot $plan.repositoryRoot)
        $protectedAfterSha256 = Get-LocalRetentionSha256 -Value ($protectedAfter | ConvertTo-Json -Depth 5 -Compress)
        if ($protectedAfterSha256 -cne $protectedBeforeSha256) {
            Write-LocalRetentionTransactionEvent -JournalPath $transaction.JournalPath -Event "POST_VALIDATION_FAILED" -PlanSha256 $plan.planSha256 -Data @{ boundary = "ProtectedPaths" }
            throw "A protected structural boundary changed during retention."
        }

        $historyPath = Close-LocalRetentionTransaction -Transaction $transaction -ResolvedRepositoryRoot $plan.repositoryRoot -PlanSha256 $plan.planSha256 -Outcome "COMPLETED"
        $transaction = $null
        $freeAfter = [System.IO.DriveInfo]::new([System.IO.Path]::GetPathRoot($plan.repositoryRoot)).AvailableFreeSpace
        $logicalBytesRemoved = if ($deleted.Count -eq 0) { [long]0 } else { [long](($deleted | Measure-Object ByteLength -Sum).Sum) }
        return [pscustomobject][ordered]@{ schemaVersion = 3; mode = "APPLY"; approvedPlanSha256 = $plan.planSha256; preservedGitStatusSha256 = $plan.gitStatusSha256; preservedWorktreeIdentitySha256 = $plan.worktreeIdentitySha256; ownershipAttestationSha256 = $plan.legacyOwnershipAttestationSha256; deletedTargetCount = $deleted.Count; logicalBytesRemoved = $logicalBytesRemoved; observedFreeSpaceIncrease = [long]($freeAfter - $freeBefore); protectedPathsPreserved = $true; transactionRecordPath = $historyPath; deleted = @($deleted) }
    }
    catch {
        if ($null -ne $transaction -and (Test-Path -LiteralPath $transaction.JournalPath)) {
            try { Write-LocalRetentionTransactionEvent -JournalPath $transaction.JournalPath -Event "APPLY_FAILED" -PlanSha256 $plan.planSha256 -Data @{ deletedTargetCount = $deleted.Count } }
            catch { throw "Retention failed and its durable transaction record could not be finalised; preserve artifacts-local and inspect manually." }
        }
        throw
    }
    finally {
        if ($null -ne $transaction) {
            if ($null -ne $transaction.StagingMarkerHandle) {
                $transaction.StagingMarkerHandle.Dispose()
                $transaction.StagingMarkerHandle = $null
            }
            if ($null -ne $transaction.RootMarkerHandle) {
                $transaction.RootMarkerHandle.Dispose()
                $transaction.RootMarkerHandle = $null
            }
            foreach ($guard in $transaction.DirectoryGuards) {
                $guard.Handle.Dispose()
            }
        }
        if ($null -ne $wipLocks) { foreach ($stream in $wipLocks) { $stream.Dispose() } }
        if ($null -ne $mutex) { try { $mutex.ReleaseMutex() } catch { }; $mutex.Dispose() }
    }
}

if ($MyInvocation.InvocationName -ne '.') {
    try {
        if (-not [string]::IsNullOrWhiteSpace($RecoveryTransactionId)) {
            if ($Apply -or
                -not [string]::IsNullOrWhiteSpace($ApprovedPlanSha256) -or
                -not [string]::IsNullOrWhiteSpace(
                    $ApprovedLegacyOwnershipAttestationSha256)) {
                throw "Normal retention approval parameters cannot be combined with recovery mode."
            }
            $result = Invoke-LocalRetentionRecovery `
                -RepositoryRoot $RepositoryRoot `
                -TransactionId $RecoveryTransactionId `
                -ApplyRecovery:$ApplyRecovery `
                -ApprovedRecoveryPlanSha256 $ApprovedRecoveryPlanSha256 `
                -ApprovedRecoveryJournalSha256 $ApprovedRecoveryJournalSha256 `
                -ApprovedGitStatusSha256 $ApprovedGitStatusSha256 `
                -ApprovedWorktreeIdentitySha256 (
                    $ApprovedWorktreeIdentitySha256)
        }
        else {
            if ($ApplyRecovery -or
                -not [string]::IsNullOrWhiteSpace(
                    $ApprovedRecoveryPlanSha256) -or
                -not [string]::IsNullOrWhiteSpace(
                    $ApprovedRecoveryJournalSha256)) {
                throw "Recovery approval parameters require an exact recovery transaction ID."
            }
            $result = Invoke-LocalArtefactRetention `
                -RepositoryRoot $RepositoryRoot `
                -Apply:$Apply `
                -ApprovedPlanSha256 $ApprovedPlanSha256 `
                -ApprovedGitStatusSha256 $ApprovedGitStatusSha256 `
                -ApprovedWorktreeIdentitySha256 (
                    $ApprovedWorktreeIdentitySha256) `
                -ApprovedLegacyOwnershipAttestationSha256 (
                    $ApprovedLegacyOwnershipAttestationSha256)
        }
        $result | ConvertTo-Json -Depth 12
    }
    finally {
        if ($null -ne $script:LocalRetentionGitGuard) {
            $script:LocalRetentionGitGuard.Dispose()
            $script:LocalRetentionGitGuard = $null
        }
    }
}
