// Purpose: Implements same-volume, SHA-256-addressed immutable content writes with bounded quarantine, atomic publication, reopen verification, and path containment.
using System.Buffers;
using System.Security.Cryptography;
using System.Text;

using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Infrastructure.Persistence;

public sealed class ImmutableContentStore : IDocumentContentStore
{
    private const long AbsoluteMaximumByteLength = 512L * 1024 * 1024;
    private const int BufferSize = 64 * 1024;
    private const int CleanupPlanMaximumByteLength = 64 * 1024 * 1024;
    private const string CleanupPlanFileName = "cleanup-plan-v1.json";
    private const string CleanupPlanPartialFileName = "cleanup-plan-v1.json.partial";

    private static readonly ContentStoreImplementationDescriptor ImplementationDescriptor =
        new("filesystem-sha256-v1");

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

    public async Task<ContentObjectDescriptor> PutAndVerifyAsync(
        BoundedContentInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.MaximumByteLength > AbsoluteMaximumByteLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(input),
                input.MaximumByteLength,
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
                        var read = await input.Content
                            .ReadAsync(buffer.AsMemory(0, BufferSize), cancellationToken)
                            .ConfigureAwait(false);

                        if (read == 0)
                        {
                            break;
                        }

                        byteLength = checked(byteLength + read);

                        if (byteLength > input.MaximumByteLength)
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

            if (input.ExpectedContentObjectId is not null &&
                input.ExpectedContentObjectId != contentObjectId)
            {
                throw new InvalidDataException(
                    "Content did not match its expected SHA-256 identity.");
            }

            var destination = ResolveObjectPath(contentObjectId, createDirectory: true);

            if (File.Exists(destination))
            {
                var descriptor = await CreateVerifiedDescriptorAsync(
                    contentObjectId,
                    byteLength,
                    input.MediaType,
                    ContentObjectWriteOutcome.AlreadyExisted,
                    cancellationToken).ConfigureAwait(false);
                DeleteQuarantineFile(quarantineFile);
                return descriptor;
            }

            try
            {
                File.Move(quarantineFile, destination, overwrite: false);
            }
            catch (IOException) when (File.Exists(destination))
            {
                var descriptor = await CreateVerifiedDescriptorAsync(
                    contentObjectId,
                    byteLength,
                    input.MediaType,
                    ContentObjectWriteOutcome.AlreadyExisted,
                    cancellationToken).ConfigureAwait(false);
                DeleteQuarantineFile(quarantineFile);
                return descriptor;
            }

            return await CreateVerifiedDescriptorAsync(
                contentObjectId,
                byteLength,
                input.MediaType,
                ContentObjectWriteOutcome.Published,
                cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            DeleteQuarantineFile(quarantineFile);
            throw;
        }
    }

    public async ValueTask<VerifiedContentObject> OpenVerifiedAsync(
        ContentObjectId contentObjectId,
        ExpectedHashAndLength expected,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contentObjectId);
        ArgumentNullException.ThrowIfNull(expected);
        cancellationToken.ThrowIfCancellationRequested();

        if (contentObjectId != expected.Sha256)
        {
            throw new InvalidDataException(
                "The requested content identity does not match the expected SHA-256.");
        }

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
            if (stream.Length != expected.ByteLength)
            {
                throw new InvalidDataException(
                    "A content object no longer matches its expected byte length.");
            }

            using var sha256 = SHA256.Create();
            var actual = Convert.ToHexString(
                await sha256.ComputeHashAsync(stream, cancellationToken)
                    .ConfigureAwait(false))
                .ToLowerInvariant();

            if (!string.Equals(
                    actual,
                    expected.Sha256.Value,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "A content object no longer matches its SHA-256 identity.");
            }

            stream.Position = 0;
            return new VerifiedContentObject(
                contentObjectId,
                expected.Sha256,
                expected.ByteLength,
                stream,
                ContentVerificationOutcome.Verified);
        }
        catch
        {
            await stream.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    private async Task<ContentObjectDescriptor> CreateVerifiedDescriptorAsync(
        ContentObjectId contentObjectId,
        long byteLength,
        ContentMediaType mediaType,
        ContentObjectWriteOutcome writeOutcome,
        CancellationToken cancellationToken)
    {
        await using var reopened = await OpenVerifiedAsync(
            contentObjectId,
            new ExpectedHashAndLength(contentObjectId, byteLength),
            cancellationToken).ConfigureAwait(false);
        return new ContentObjectDescriptor(
            contentObjectId,
            contentObjectId,
            byteLength,
            mediaType,
            ImplementationDescriptor,
            writeOutcome,
            new ContentObjectVerificationResult(
                ContentVerificationOutcome.Verified,
                reopened.ReopenVerification));
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
                contentObjectId,
                sourcePath,
                reservationPath,
                WasPresent: true);
        }

        if (!sourceExists)
        {
            return new ContentDeletionReservation(
                contentObjectId,
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
            contentObjectId,
            sourcePath,
            reservationPath,
            WasPresent: true);
    }

    internal static async Task VerifyDeletionReservationAsync(
        ContentDeletionReservation reservation,
        long expectedByteLength,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(reservation);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(expectedByteLength);

        if (!reservation.WasPresent || !File.Exists(reservation.ReservationPath))
        {
            throw new InvalidDataException(
                "A required content deletion reservation is absent.");
        }

        await VerifyExistingAsync(
            reservation.ReservationPath,
            reservation.ContentObjectId,
            expectedByteLength,
            cancellationToken).ConfigureAwait(false);
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

    internal IReadOnlyList<ContentDeletionReservation> EnumerateDeletionReservations(
        OperationId operationId)
    {
        ArgumentNullException.ThrowIfNull(operationId);
        var directory = ResolveCleanupReservationDirectory(operationId, createDirectory: false);

        if (!Directory.Exists(directory))
        {
            return [];
        }

        StoragePathSafety.EnsureExistingPathIsNotReparsePoint(directory, nameof(operationId));
        var reservations = new List<ContentDeletionReservation>();
        var identifiers = new HashSet<string>(StringComparer.Ordinal);

        foreach (var path in Directory.EnumerateFileSystemEntries(
            directory,
            "*",
            SearchOption.TopDirectoryOnly))
        {
            StoragePathSafety.EnsureExistingPathIsNotReparsePoint(path, nameof(operationId));

            if (Directory.Exists(path))
            {
                throw new InvalidDataException(
                    "A cleanup operation directory contains an unexpected directory.");
            }

            var name = Path.GetFileName(path);

            if (string.Equals(name, CleanupPlanFileName, StringComparison.Ordinal) ||
                string.Equals(name, CleanupPlanPartialFileName, StringComparison.Ordinal))
            {
                continue;
            }

            if (!name.EndsWith(".delete", StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "A cleanup operation directory contains an unexpected file.");
            }

            var identifier = name[..^".delete".Length];
            var contentObjectId = new ContentObjectId(identifier);

            if (!string.Equals(
                    name,
                    $"{contentObjectId.Value}.delete",
                    StringComparison.Ordinal) ||
                !identifiers.Add(contentObjectId.Value))
            {
                throw new InvalidDataException(
                    "A cleanup operation directory contains an invalid reservation identity.");
            }

            var sourcePath = ResolveObjectPath(contentObjectId, createDirectory: false);

            if (File.Exists(sourcePath))
            {
                throw new InvalidDataException(
                    "A published content object conflicts with its deletion reservation.");
            }

            reservations.Add(new ContentDeletionReservation(
                contentObjectId,
                sourcePath,
                path,
                WasPresent: true));
        }

        return reservations
            .OrderBy(item => item.ContentObjectId.Value, StringComparer.Ordinal)
            .ToArray();
    }

    internal static void FinaliseDeletionReservation(
        ContentDeletionFinalisation finalisation)
    {
        ArgumentNullException.ThrowIfNull(finalisation);
        var reservation = finalisation.Reservation;

        if (!File.Exists(reservation.ReservationPath))
        {
            throw new InvalidDataException(
                "A verified deletion reservation disappeared before finalisation.");
        }

        StoragePathSafety.EnsureExistingPathIsNotReparsePoint(
            reservation.ReservationPath,
            nameof(finalisation));

        if (File.Exists(reservation.SourcePath))
        {
            throw new InvalidDataException(
                "A deletion reservation cannot be finalised while published content exists.");
        }

        File.Delete(reservation.ReservationPath);
    }

    internal async Task PublishCleanupPlanAsync(
        OperationId operationId,
        ReadOnlyMemory<byte> canonicalPlan,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operationId);

        if (canonicalPlan.IsEmpty || canonicalPlan.Length > CleanupPlanMaximumByteLength)
        {
            throw new InvalidDataException("A cleanup plan has an invalid bounded length.");
        }

        var directory = ResolveCleanupReservationDirectory(operationId, createDirectory: true);
        var planPath = StoragePathSafety.CombineUnderRoot(directory, CleanupPlanFileName);
        var partialPath = StoragePathSafety.CombineUnderRoot(
            directory,
            CleanupPlanPartialFileName);

        if (File.Exists(planPath))
        {
            await EnsureCleanupPlanMatchesAsync(
                planPath,
                canonicalPlan,
                cancellationToken).ConfigureAwait(false);
            DeleteCleanupPlanPartial(partialPath);
            return;
        }

        DeleteCleanupPlanPartial(partialPath);

        try
        {
            await using (var stream = new FileStream(
                partialPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                BufferSize,
                FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await stream.WriteAsync(canonicalPlan, cancellationToken)
                    .ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                stream.Flush(flushToDisk: true);
            }

            File.Move(partialPath, planPath, overwrite: false);
            await EnsureCleanupPlanMatchesAsync(
                planPath,
                canonicalPlan,
                cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            DeleteCleanupPlanPartial(partialPath);
            throw;
        }
    }

    internal async Task<byte[]?> ReadCleanupPlanAsync(
        OperationId operationId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operationId);
        var directory = ResolveCleanupReservationDirectory(operationId, createDirectory: false);

        if (!Directory.Exists(directory))
        {
            return null;
        }

        StoragePathSafety.EnsureExistingPathIsNotReparsePoint(directory, nameof(operationId));
        var planPath = StoragePathSafety.CombineUnderRoot(directory, CleanupPlanFileName);

        if (!File.Exists(planPath))
        {
            return null;
        }

        StoragePathSafety.EnsureExistingPathIsNotReparsePoint(planPath, nameof(operationId));
        var length = new FileInfo(planPath).Length;

        if (length is <= 0 or > CleanupPlanMaximumByteLength)
        {
            throw new InvalidDataException("A cleanup plan has an invalid bounded length.");
        }

        return await File.ReadAllBytesAsync(planPath, cancellationToken).ConfigureAwait(false);
    }

    internal void DeleteCleanupPlanIfComplete(OperationId operationId)
    {
        ArgumentNullException.ThrowIfNull(operationId);
        var directory = ResolveCleanupReservationDirectory(operationId, createDirectory: false);

        if (!Directory.Exists(directory))
        {
            return;
        }

        if (EnumerateDeletionReservations(operationId).Count != 0)
        {
            throw new InvalidOperationException(
                "A cleanup plan cannot be removed while reservations remain.");
        }

        var partialPath = StoragePathSafety.CombineUnderRoot(
            directory,
            CleanupPlanPartialFileName);
        DeleteCleanupPlanPartial(partialPath);
        var planPath = StoragePathSafety.CombineUnderRoot(directory, CleanupPlanFileName);

        if (File.Exists(planPath))
        {
            StoragePathSafety.EnsureExistingPathIsNotReparsePoint(planPath, nameof(operationId));
            File.Delete(planPath);
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

    private static async Task EnsureCleanupPlanMatchesAsync(
        string path,
        ReadOnlyMemory<byte> expected,
        CancellationToken cancellationToken)
    {
        StoragePathSafety.EnsureExistingPathIsNotReparsePoint(path, nameof(path));
        var length = new FileInfo(path).Length;

        if (length is <= 0 or > CleanupPlanMaximumByteLength ||
            length != expected.Length)
        {
            throw new InvalidDataException(
                "A cleanup operation identity is associated with an invalid plan length.");
        }

        var actual = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);

        if (!CryptographicOperations.FixedTimeEquals(actual, expected.Span))
        {
            throw new InvalidDataException(
                "A cleanup operation identity is associated with a different plan.");
        }
    }

    private static void DeleteCleanupPlanPartial(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        StoragePathSafety.EnsureExistingPathIsNotReparsePoint(path, nameof(path));
        File.Delete(path);
    }
}

internal sealed record ContentDeletionReservation(
    ContentObjectId ContentObjectId,
    string SourcePath,
    string ReservationPath,
    bool WasPresent);

internal sealed record ContentDeletionFinalisation(
    ContentDeletionReservation Reservation);
