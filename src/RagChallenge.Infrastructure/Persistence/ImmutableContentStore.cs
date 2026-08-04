// Purpose: Implements same-volume, SHA-256-addressed immutable content writes with bounded quarantine, atomic publication, reopen verification, and path containment.
using System.Buffers;
using System.Security.Cryptography;
using System.Text;

using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Infrastructure.Persistence;

public sealed class ImmutableContentStore : IImmutableContentStore
{
    private const long AbsoluteMaximumByteLength = 512L * 1024 * 1024;
    private const int BufferSize = 64 * 1024;

    private readonly string rootPath;
    private readonly string objectsPath;
    private readonly string quarantinePath;

    public ImmutableContentStore(SqliteStoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        rootPath = options.ContentStoreRoot;
        objectsPath = StoragePathSafety.CombineUnderRoot(rootPath, "objects");
        quarantinePath = StoragePathSafety.CombineUnderRoot(rootPath, "quarantine");

        CreateSafeDirectory(rootPath);
        CreateSafeDirectory(objectsPath);
        CreateSafeDirectory(quarantinePath);
    }

    internal string RootPath => rootPath;

    public async Task<ContentWriteResult> PutAsync(
        Stream content,
        long maximumByteLength,
        ContentObjectId? expectedContentObjectId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (!content.CanRead)
        {
            throw new ArgumentException("Content must be readable.", nameof(content));
        }

        if (maximumByteLength <= 0 || maximumByteLength > AbsoluteMaximumByteLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumByteLength),
                maximumByteLength,
                $"Content must be bounded to 1..{AbsoluteMaximumByteLength} bytes.");
        }

        var quarantineFile = StoragePathSafety.CombineUnderRoot(
            rootPath,
            "quarantine",
            $"{Guid.NewGuid():N}.tmp");
        long byteLength = 0;
        string digest;

        try
        {
            await using (var target = new FileStream(
                quarantineFile,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                BufferSize,
                FileOptions.Asynchronous |
                FileOptions.SequentialScan |
                FileOptions.WriteThrough))
            using (var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
            {
                var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);

                try
                {
                    while (true)
                    {
                        var read = await content
                            .ReadAsync(buffer.AsMemory(0, BufferSize), cancellationToken)
                            .ConfigureAwait(false);

                        if (read == 0)
                        {
                            break;
                        }

                        byteLength = checked(byteLength + read);

                        if (byteLength > maximumByteLength)
                        {
                            throw new InvalidDataException(
                                "Content exceeded its authorised byte limit.");
                        }

                        hash.AppendData(buffer, 0, read);
                        await target
                            .WriteAsync(buffer.AsMemory(0, read), cancellationToken)
                            .ConfigureAwait(false);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
                }

                if (byteLength == 0)
                {
                    throw new InvalidDataException("An immutable content object cannot be empty.");
                }

                await target.FlushAsync(cancellationToken).ConfigureAwait(false);
                target.Flush(flushToDisk: true);
                digest = Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
            }

            var contentObjectId = new ContentObjectId(digest);

            if (expectedContentObjectId is not null &&
                expectedContentObjectId != contentObjectId)
            {
                throw new InvalidDataException(
                    "Content did not match its expected SHA-256 identity.");
            }

            var destination = ResolveObjectPath(contentObjectId, createDirectory: true);

            if (File.Exists(destination))
            {
                await VerifyExistingAsync(
                    destination,
                    contentObjectId,
                    byteLength,
                    cancellationToken).ConfigureAwait(false);
                DeleteQuarantineFile(quarantineFile);
                return new ContentWriteResult(contentObjectId, byteLength, AlreadyExisted: true);
            }

            try
            {
                File.Move(quarantineFile, destination, overwrite: false);
            }
            catch (IOException) when (File.Exists(destination))
            {
                await VerifyExistingAsync(
                    destination,
                    contentObjectId,
                    byteLength,
                    cancellationToken).ConfigureAwait(false);
                DeleteQuarantineFile(quarantineFile);
                return new ContentWriteResult(contentObjectId, byteLength, AlreadyExisted: true);
            }

            await VerifyExistingAsync(
                destination,
                contentObjectId,
                byteLength,
                cancellationToken).ConfigureAwait(false);
            return new ContentWriteResult(contentObjectId, byteLength, AlreadyExisted: false);
        }
        catch
        {
            DeleteQuarantineFile(quarantineFile);
            throw;
        }
    }

    public async ValueTask<Stream> OpenReadAsync(
        ContentObjectId contentObjectId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contentObjectId);
        cancellationToken.ThrowIfCancellationRequested();
        var path = ResolveObjectPath(contentObjectId, createDirectory: false);
        StoragePathSafety.EnsureExistingPathIsNotReparsePoint(path, nameof(contentObjectId));

        var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            BufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        try
        {
            using var sha256 = SHA256.Create();
            var actual = Convert.ToHexString(
                await sha256.ComputeHashAsync(stream, cancellationToken)
                    .ConfigureAwait(false))
                .ToLowerInvariant();

            if (!string.Equals(
                    actual,
                    contentObjectId.Value,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "A content object no longer matches its SHA-256 identity.");
            }

            stream.Position = 0;
            return stream;
        }
        catch
        {
            await stream.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    internal bool DeleteIfPresent(ContentObjectId contentObjectId)
    {
        ArgumentNullException.ThrowIfNull(contentObjectId);
        var path = ResolveObjectPath(contentObjectId, createDirectory: false);

        if (!File.Exists(path))
        {
            return false;
        }

        StoragePathSafety.EnsureExistingPathIsNotReparsePoint(path, nameof(contentObjectId));
        File.Delete(path);
        return true;
    }

    internal async Task<ContentDeletionReservation> ReserveForDeletionAsync(
        OperationId operationId,
        ContentObjectId contentObjectId,
        long expectedByteLength,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operationId);
        ArgumentNullException.ThrowIfNull(contentObjectId);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(expectedByteLength);

        var sourcePath = ResolveObjectPath(contentObjectId, createDirectory: false);
        var reservationDirectory = ResolveCleanupReservationDirectory(
            operationId,
            createDirectory: true);
        var reservationPath = StoragePathSafety.CombineUnderRoot(
            reservationDirectory,
            $"{contentObjectId.Value}.delete");
        var sourceExists = File.Exists(sourcePath);
        var reservationExists = File.Exists(reservationPath);

        if (sourceExists && reservationExists)
        {
            throw new InvalidDataException(
                "A content deletion reservation conflicts with the published object.");
        }

        if (reservationExists)
        {
            await VerifyExistingAsync(
                reservationPath,
                contentObjectId,
                expectedByteLength,
                cancellationToken).ConfigureAwait(false);
            return new ContentDeletionReservation(
                sourcePath,
                reservationPath,
                WasPresent: true);
        }

        if (!sourceExists)
        {
            return new ContentDeletionReservation(
                sourcePath,
                reservationPath,
                WasPresent: false);
        }

        await VerifyExistingAsync(
            sourcePath,
            contentObjectId,
            expectedByteLength,
            cancellationToken).ConfigureAwait(false);
        File.Move(sourcePath, reservationPath, overwrite: false);
        return new ContentDeletionReservation(
            sourcePath,
            reservationPath,
            WasPresent: true);
    }

    internal static void RestoreDeletionReservation(ContentDeletionReservation reservation)
    {
        ArgumentNullException.ThrowIfNull(reservation);

        if (!reservation.WasPresent || !File.Exists(reservation.ReservationPath))
        {
            return;
        }

        StoragePathSafety.EnsureExistingPathIsNotReparsePoint(
            reservation.ReservationPath,
            nameof(reservation));

        if (File.Exists(reservation.SourcePath))
        {
            throw new InvalidDataException(
                "A published content object appeared while its deletion was being rolled back.");
        }

        File.Move(reservation.ReservationPath, reservation.SourcePath, overwrite: false);
    }

    internal int CountDeletionReservations(OperationId operationId)
    {
        ArgumentNullException.ThrowIfNull(operationId);
        var directory = ResolveCleanupReservationDirectory(operationId, createDirectory: false);

        if (!Directory.Exists(directory))
        {
            return 0;
        }

        StoragePathSafety.EnsureExistingPathIsNotReparsePoint(directory, nameof(operationId));
        return Directory.EnumerateFiles(directory, "*.delete", SearchOption.TopDirectoryOnly)
            .Count(path =>
            {
                StoragePathSafety.EnsureExistingPathIsNotReparsePoint(path, nameof(operationId));
                return true;
            });
    }

    internal void FinaliseDeletionReservations(OperationId operationId)
    {
        ArgumentNullException.ThrowIfNull(operationId);
        var directory = ResolveCleanupReservationDirectory(operationId, createDirectory: false);

        if (!Directory.Exists(directory))
        {
            return;
        }

        StoragePathSafety.EnsureExistingPathIsNotReparsePoint(directory, nameof(operationId));

        foreach (var path in Directory.EnumerateFiles(
            directory,
            "*.delete",
            SearchOption.TopDirectoryOnly))
        {
            StoragePathSafety.EnsureExistingPathIsNotReparsePoint(path, nameof(operationId));
            File.Delete(path);
        }

        if (!Directory.EnumerateFileSystemEntries(directory).Any())
        {
            Directory.Delete(directory);
        }
    }

    internal IEnumerable<string> EnumerateObjectFiles()
    {
        if (!Directory.Exists(objectsPath))
        {
            yield break;
        }

        var pending = new Stack<string>();
        pending.Push(objectsPath);

        while (pending.TryPop(out var directory))
        {
            StoragePathSafety.EnsureExistingPathIsNotReparsePoint(
                directory,
                nameof(objectsPath));

            foreach (var file in Directory.EnumerateFiles(directory, "*.bin"))
            {
                StoragePathSafety.EnsureExistingPathIsNotReparsePoint(
                    file,
                    nameof(objectsPath));
                yield return file;
            }

            foreach (var child in Directory.EnumerateDirectories(directory))
            {
                StoragePathSafety.EnsureExistingPathIsNotReparsePoint(
                    child,
                    nameof(objectsPath));
                pending.Push(child);
            }
        }
    }

    private string ResolveObjectPath(
        ContentObjectId contentObjectId,
        bool createDirectory)
    {
        var prefix = contentObjectId.Value[..2];
        var directory = StoragePathSafety.CombineUnderRoot(rootPath, "objects", prefix);

        if (createDirectory)
        {
            CreateSafeDirectory(directory);
        }

        return StoragePathSafety.CombineUnderRoot(
            rootPath,
            "objects",
            prefix,
            $"{contentObjectId.Value}.bin");
    }

    private string ResolveCleanupReservationDirectory(
        OperationId operationId,
        bool createDirectory)
    {
        var operationDigest = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(operationId.Value)))
            .ToLowerInvariant();
        var cleanupRoot = StoragePathSafety.CombineUnderRoot(
            rootPath,
            "quarantine",
            "cleanup");
        var directory = StoragePathSafety.CombineUnderRoot(cleanupRoot, operationDigest);

        if (createDirectory)
        {
            CreateSafeDirectory(cleanupRoot);
            CreateSafeDirectory(directory);
        }

        return directory;
    }

    private static async Task VerifyExistingAsync(
        string path,
        ContentObjectId expectedId,
        long expectedByteLength,
        CancellationToken cancellationToken)
    {
        StoragePathSafety.EnsureExistingPathIsNotReparsePoint(path, nameof(path));
        var file = new FileInfo(path);

        if (file.Length != expectedByteLength)
        {
            throw new InvalidDataException(
                "An existing content object has an unexpected byte length.");
        }

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            BufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var sha256 = SHA256.Create();
        var actual = Convert.ToHexString(
            await sha256.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false))
            .ToLowerInvariant();

        if (!string.Equals(actual, expectedId.Value, StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                "An existing content object does not match its SHA-256 path.");
        }
    }

    private static void CreateSafeDirectory(string path)
    {
        Directory.CreateDirectory(path);
        StoragePathSafety.EnsureExistingPathIsNotReparsePoint(path, nameof(path));
    }

    private static void DeleteQuarantineFile(string path)
    {
        if (File.Exists(path))
        {
            StoragePathSafety.EnsureExistingPathIsNotReparsePoint(path, nameof(path));
            File.Delete(path);
        }
    }
}

internal sealed record ContentDeletionReservation(
    string SourcePath,
    string ReservationPath,
    bool WasPresent);
