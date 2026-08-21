// Purpose: Creates and verifies isolated point-in-time recovery snapshots of both SQLite stores and immutable content under a control-plane maintenance lease.
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.Infrastructure.Persistence;

public sealed record RecoverySnapshotResult(
    string SnapshotPath,
    string ManifestSha256,
    int ContentObjectCount);

public sealed record RecoveryVerificationResult(
    bool IsValid,
    IReadOnlyList<string> Failures);

public sealed class SqliteRecoverySnapshotService(SqliteStoreOptions options)
{
    internal const long MaximumManifestBytes = 256L * 1024 * 1024;
    internal const int MaximumManifestFileCount = 1_000_002;
    internal const int MaximumManifestTokenBytes = 1024;
    private const int MaximumContentObjectCount = 1_000_000;
    private const int MaximumManifestIdentityLength = 128;
    private const int MaximumManifestRelativePathLength = 96;
    private const int MaximumVerificationFailures = 128;
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(10);
    private static readonly IReadOnlyList<string> MissingManifestFailures =
        Array.AsReadOnly(new[] { "recovery-manifest.json is missing" });
    private static readonly IReadOnlyList<string> UnsupportedManifestFailures =
        Array.AsReadOnly(new[] { "manifest schema version is unsupported" });
    private static readonly IReadOnlyList<string> BoundedManifestFailures =
        Array.AsReadOnly(new[] { "recovery manifest exceeds its bounded limits" });
    private static readonly IReadOnlyList<string> InvalidManifestFailures =
        Array.AsReadOnly(new[] { "recovery manifest JSON is invalid" });
    private static readonly IReadOnlyList<string> InvalidManifestMetadataFailures =
        Array.AsReadOnly(new[] { "recovery manifest metadata is invalid" });
    private static readonly IReadOnlyList<string> InvalidManifestFileIdentityFailures =
        Array.AsReadOnly(new[] { "manifest file identities are incomplete or duplicated" });
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        MaxDepth = 8,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    private readonly SqliteStoreOptions options =
        options ?? throw new ArgumentNullException(nameof(options));

    public async Task<RecoverySnapshotResult> CreateAndVerifyAsync(
        OperationId operationId,
        CorpusId corpusId,
        string recoveryRoot,
        DateTimeOffset requestedAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operationId);
        ArgumentNullException.ThrowIfNull(corpusId);
        ControlPlaneMapping.EnsureUtc(requestedAt, nameof(requestedAt));
        var resolvedRecoveryRoot = StoragePathSafety.ResolveRootPath(
            recoveryRoot,
            nameof(recoveryRoot));
        Directory.CreateDirectory(resolvedRecoveryRoot);
        StoragePathSafety.EnsureExistingPathIsNotReparsePoint(
            resolvedRecoveryRoot,
            nameof(recoveryRoot));
        var snapshotName = $"snapshot-{Sha256(operationId.Value)[..16]}";
        var snapshotPath = StoragePathSafety.CombineUnderRoot(
            resolvedRecoveryRoot,
            snapshotName);

        if (IsWithin(snapshotPath, options.ContentStoreRoot) ||
            Directory.Exists(snapshotPath) ||
            File.Exists(snapshotPath))
        {
            throw new ArgumentException(
                "A recovery snapshot must use a new root outside the content store.",
                nameof(recoveryRoot));
        }

        await AcquireLeaseAsync(
            operationId,
            corpusId,
            requestedAt,
            cancellationToken).ConfigureAwait(false);

        try
        {
            Directory.CreateDirectory(snapshotPath);
            StoragePathSafety.EnsureExistingPathIsNotReparsePoint(
                snapshotPath,
                nameof(recoveryRoot));
            var controlDestination = StoragePathSafety.CombineUnderRoot(
                snapshotPath,
                "control.db");
            var vectorDestination = StoragePathSafety.CombineUnderRoot(
                snapshotPath,
                "vectors.db");
            await BackupDatabaseAsync(
                options.ControlDatabasePath,
                controlDestination,
                cancellationToken).ConfigureAwait(false);
            await BackupDatabaseAsync(
                options.VectorDatabasePath,
                vectorDestination,
                cancellationToken).ConfigureAwait(false);
            var files = new List<RecoveryFile>
            {
                await DescribeFileAsync(snapshotPath, controlDestination, cancellationToken)
                    .ConfigureAwait(false),
                await DescribeFileAsync(snapshotPath, vectorDestination, cancellationToken)
                    .ConfigureAwait(false),
            };
            var contentStore = new ImmutableContentStore(options);
            var contentFiles = contentStore.EnumerateObjectFiles()
                .Take(MaximumContentObjectCount + 1)
                .ToArray();

            if (contentFiles.Length > MaximumContentObjectCount)
            {
                throw new InvalidOperationException(
                    $"A recovery snapshot cannot exceed {MaximumContentObjectCount} content objects.");
            }

            Array.Sort(contentFiles, StringComparer.Ordinal);

            foreach (var sourcePath in contentFiles)
            {
                var relativeToContent = Path.GetRelativePath(
                    contentStore.RootPath,
                    sourcePath);

                if (relativeToContent.StartsWith("..", StringComparison.Ordinal) ||
                    Path.IsPathRooted(relativeToContent))
                {
                    throw new InvalidDataException(
                        "A content object escaped the configured content root.");
                }

                var destination = StoragePathSafety.CombineUnderRoot(
                    snapshotPath,
                    "content",
                    relativeToContent);
                var destinationDirectory = Path.GetDirectoryName(destination) ??
                    throw new InvalidDataException(
                        "A recovery content path had no parent directory.");
                Directory.CreateDirectory(destinationDirectory);
                StoragePathSafety.EnsureExistingPathIsNotReparsePoint(
                    destinationDirectory,
                    nameof(recoveryRoot));
                File.Copy(sourcePath, destination, overwrite: false);
                files.Add(await DescribeFileAsync(
                    snapshotPath,
                    destination,
                    cancellationToken).ConfigureAwait(false));
            }

            var manifest = new RecoveryManifest(
                SchemaVersion: 1,
                CorpusId: corpusId.Value,
                OperationId: operationId.Value,
                CreatedAtUtc: ControlPlaneMapping.FormatUtc(requestedAt),
                Files: files.OrderBy(file => file.RelativePath, StringComparer.Ordinal).ToArray());
            var manifestPath = StoragePathSafety.CombineUnderRoot(
                snapshotPath,
                "recovery-manifest.json");
            var json = JsonSerializer.Serialize(manifest, JsonOptions) + "\n";
            if (Encoding.UTF8.GetByteCount(json) > MaximumManifestBytes)
            {
                throw new InvalidOperationException(
                    "The recovery manifest exceeds its bounded byte limit.");
            }

            await File.WriteAllTextAsync(
                manifestPath,
                json,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                cancellationToken).ConfigureAwait(false);
            var verification = await VerifyIsolatedAsync(
                snapshotPath,
                cancellationToken).ConfigureAwait(false);

            if (!verification.IsValid)
            {
                throw new InvalidDataException(
                    $"Recovery verification failed: {string.Join("; ", verification.Failures)}");
            }

            var manifestSha = await HashFileAsync(
                manifestPath,
                cancellationToken).ConfigureAwait(false);
            await CompleteLeaseAsync(
                operationId,
                corpusId,
                requestedAt,
                manifestSha,
                cancellationToken).ConfigureAwait(false);
            return new RecoverySnapshotResult(
                snapshotPath,
                manifestSha,
                contentFiles.Length);
        }
        catch
        {
            await FailLeaseAsync(
                operationId,
                corpusId,
                requestedAt,
                cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    public static async Task<RecoveryVerificationResult> VerifyIsolatedAsync(
        string snapshotPath,
        CancellationToken cancellationToken = default)
    {
        var root = StoragePathSafety.ResolveRootPath(
            snapshotPath,
            nameof(snapshotPath));
        StoragePathSafety.EnsureExistingPathIsNotReparsePoint(
            root,
            nameof(snapshotPath));
        var failures = new List<string>();
        var manifestPath = StoragePathSafety.CombineUnderRoot(
            root,
            "recovery-manifest.json");

        if (!File.Exists(manifestPath))
        {
            return new RecoveryVerificationResult(
                IsValid: false,
                MissingManifestFailures);
        }

        RecoveryManifest? manifest;

        try
        {
            StoragePathSafety.EnsureExistingPathIsNotReparsePoint(
                manifestPath,
                nameof(snapshotPath));
            var manifestBytes = await ReadBoundedManifestAsync(
                manifestPath,
                cancellationToken).ConfigureAwait(false);
            PreflightManifestJson(manifestBytes);
            manifest = JsonSerializer.Deserialize<RecoveryManifest>(
                manifestBytes,
                JsonOptions);
        }
        catch (RecoveryManifestLimitException)
        {
            return new RecoveryVerificationResult(
                IsValid: false,
                BoundedManifestFailures);
        }
        catch (Exception exception) when (
            exception is JsonException or NotSupportedException or ArgumentException or
                IOException or UnauthorizedAccessException)
        {
            return new RecoveryVerificationResult(
                IsValid: false,
                InvalidManifestFailures);
        }

        if (manifest is null || manifest.SchemaVersion != 1 || manifest.Files is null)
        {
            return new RecoveryVerificationResult(
                IsValid: false,
                UnsupportedManifestFailures);
        }

        if (manifest.Files.Count < 2 || manifest.Files.Count > MaximumManifestFileCount)
        {
            return new RecoveryVerificationResult(
                IsValid: false,
                BoundedManifestFailures);
        }

        try
        {
            if (string.IsNullOrWhiteSpace(manifest.CorpusId) ||
                manifest.CorpusId.Length > MaximumManifestIdentityLength ||
                string.IsNullOrWhiteSpace(manifest.OperationId) ||
                manifest.OperationId.Length > MaximumManifestIdentityLength ||
                string.IsNullOrWhiteSpace(manifest.CreatedAtUtc) ||
                manifest.CreatedAtUtc.Length > 64)
            {
                throw new InvalidDataException("Manifest identity metadata is invalid.");
            }

            _ = new CorpusId(manifest.CorpusId);
            _ = new OperationId(manifest.OperationId);
            _ = ControlPlaneMapping.ParseUtc(manifest.CreatedAtUtc);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidDataException)
        {
            return new RecoveryVerificationResult(
                IsValid: false,
                InvalidManifestMetadataFailures);
        }

        var relativePaths = new HashSet<string>(StringComparer.Ordinal);
        var controlCount = 0;
        var vectorCount = 0;
        var identitiesAreValid = true;
        foreach (var file in manifest.Files)
        {
            if (file is null ||
                string.IsNullOrWhiteSpace(file.RelativePath) ||
                file.RelativePath.Length > MaximumManifestRelativePathLength ||
                !relativePaths.Add(file.RelativePath))
            {
                identitiesAreValid = false;
                continue;
            }

            controlCount += file.RelativePath == "control.db" ? 1 : 0;
            vectorCount += file.RelativePath == "vectors.db" ? 1 : 0;
        }

        if (!identitiesAreValid || controlCount != 1 || vectorCount != 1)
        {
            return new RecoveryVerificationResult(
                IsValid: false,
                InvalidManifestFileIdentityFailures);
        }

        for (var index = 0; index < manifest.Files.Count; index++)
        {
            if (failures.Count >= MaximumVerificationFailures)
            {
                break;
            }

            var file = manifest.Files[index]!;
            try
            {
                if (!IsValidManifestEntry(file))
                {
                    failures.Add($"invalid manifest entry at index {index}");
                    continue;
                }

                var path = StoragePathSafety.CombineUnderRoot(
                    root,
                    file.RelativePath.Replace('/', Path.DirectorySeparatorChar));

                if (!File.Exists(path))
                {
                    failures.Add($"missing file: {file.RelativePath}");
                    continue;
                }

                StoragePathSafety.EnsureExistingPathIsNotReparsePoint(
                    path,
                    nameof(snapshotPath));
                var info = new FileInfo(path);

                if (info.Length != file.ByteLength)
                {
                    failures.Add($"length mismatch: {file.RelativePath}");
                    continue;
                }

                var actualHash = await HashFileAsync(path, cancellationToken)
                    .ConfigureAwait(false);

                if (!string.Equals(actualHash, file.Sha256, StringComparison.Ordinal))
                {
                    failures.Add($"SHA-256 mismatch: {file.RelativePath}");
                }
            }
            catch (Exception exception) when (
                exception is ArgumentException or IOException or UnauthorizedAccessException)
            {
                failures.Add($"unsafe manifest file entry at index {index}");
            }
        }

        var controlPath = StoragePathSafety.CombineUnderRoot(root, "control.db");
        var vectorPath = StoragePathSafety.CombineUnderRoot(root, "vectors.db");
        await VerifyDatabaseAsync(controlPath, "control.db", failures, cancellationToken)
            .ConfigureAwait(false);
        await VerifyDatabaseAsync(vectorPath, "vectors.db", failures, cancellationToken)
            .ConfigureAwait(false);
        try
        {
            await VerifyAuthorityLinkAsync(
                controlPath,
                vectorPath,
                failures,
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (
            exception is SqliteException or ArgumentException or InvalidDataException)
        {
            failures.Add("authority link could not be verified");
        }

        return new RecoveryVerificationResult(failures.Count == 0, failures.AsReadOnly());
    }

    private async Task AcquireLeaseAsync(
        OperationId operationId,
        CorpusId corpusId,
        DateTimeOffset requestedAt,
        CancellationToken cancellationToken)
    {
        await using var context = options.CreateControlContext();
        await using var transaction = await BeginImmediateAsync(
            context,
            cancellationToken).ConfigureAwait(false);
        var existingOperation = await context.AdminOperations.SingleOrDefaultAsync(
            row => row.OperationId == operationId.Value,
            cancellationToken).ConfigureAwait(false);

        if (existingOperation is not null)
        {
            throw new InvalidOperationException(
                "A recovery operation identity cannot be replayed into another snapshot path.");
        }

        var leases = await context.RecoveryLeases
            .Where(row => row.CorpusId == corpusId.Value)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        if (leases.Any(row =>
            ControlPlaneMapping.ParseUtc(row.ExpiresAtUtc) > requestedAt))
        {
            throw new InvalidOperationException(
                "Another storage-maintenance lease is active for this corpus.");
        }

        context.AdminOperations.Add(new AdminOperationRow
        {
            OperationId = operationId.Value,
            CorpusId = corpusId.Value,
            OperationKind = "RecoverySnapshot",
            Status = "InProgress",
            ExpectedRevision = null,
            ResultRevision = null,
            RequestedAtUtc = ControlPlaneMapping.FormatUtc(requestedAt),
            CompletedAtUtc = null,
        });
        context.RecoveryLeases.Add(new RecoveryLeaseRow
        {
            CorpusId = corpusId.Value,
            LeaseName = "storage-maintenance",
            OperationId = operationId.Value,
            AcquiredAtUtc = ControlPlaneMapping.FormatUtc(requestedAt),
            ExpiresAtUtc = ControlPlaneMapping.FormatUtc(requestedAt + LeaseDuration),
        });
        AddAudit(
            context,
            operationId,
            corpusId,
            "RecoverySnapshotPlanned",
            requestedAt,
            "snapshot-planned");
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task CompleteLeaseAsync(
        OperationId operationId,
        CorpusId corpusId,
        DateTimeOffset completedAt,
        string manifestSha256,
        CancellationToken cancellationToken)
    {
        await using var context = options.CreateControlContext();
        await using var transaction = await BeginImmediateAsync(
            context,
            cancellationToken).ConfigureAwait(false);
        var operation = await context.AdminOperations.SingleAsync(
            row => row.OperationId == operationId.Value &&
                row.OperationKind == "RecoverySnapshot",
            cancellationToken).ConfigureAwait(false);
        operation.Status = "Applied";
        operation.CompletedAtUtc = ControlPlaneMapping.FormatUtc(completedAt);
        var lease = await context.RecoveryLeases.SingleAsync(
            row => row.CorpusId == corpusId.Value &&
                row.LeaseName == "storage-maintenance" &&
                row.OperationId == operationId.Value,
            cancellationToken).ConfigureAwait(false);
        context.RecoveryLeases.Remove(lease);
        AddAudit(
            context,
            operationId,
            corpusId,
            "RecoverySnapshotVerified",
            completedAt,
            manifestSha256);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task FailLeaseAsync(
        OperationId operationId,
        CorpusId corpusId,
        DateTimeOffset failedAt,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var context = options.CreateControlContext();
            await using var transaction = await BeginImmediateAsync(
                context,
                cancellationToken).ConfigureAwait(false);
            var operation = await context.AdminOperations.SingleOrDefaultAsync(
                row => row.OperationId == operationId.Value &&
                    row.OperationKind == "RecoverySnapshot",
                cancellationToken).ConfigureAwait(false);

            if (operation is null || operation.Status != "InProgress")
            {
                return;
            }

            operation.Status = "Failed";
            operation.CompletedAtUtc = ControlPlaneMapping.FormatUtc(failedAt);
            var lease = await context.RecoveryLeases.SingleOrDefaultAsync(
                row => row.CorpusId == corpusId.Value &&
                    row.LeaseName == "storage-maintenance" &&
                    row.OperationId == operationId.Value,
                cancellationToken).ConfigureAwait(false);

            if (lease is not null)
            {
                context.RecoveryLeases.Remove(lease);
            }

            AddAudit(
                context,
                operationId,
                corpusId,
                "RecoverySnapshotFailed",
                failedAt,
                "snapshot-failed");
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (
            exception is SqliteException or IOException or InvalidOperationException)
        {
            // The original recovery failure remains primary; an expired lease permits later repair.
        }
    }

    private static async Task BackupDatabaseAsync(
        string sourcePath,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        await using var source = new SqliteConnection(
            SqliteConnectionStrings.Create(
                sourcePath,
                SqliteOpenMode.ReadOnly,
                pooling: false));
        await using var destination = new SqliteConnection(
            SqliteConnectionStrings.Create(
                destinationPath,
                SqliteOpenMode.ReadWriteCreate,
                pooling: false));
        await source.OpenAsync(cancellationToken).ConfigureAwait(false);
        await destination.OpenAsync(cancellationToken).ConfigureAwait(false);
        source.BackupDatabase(destination);
    }

    private static async Task<RecoveryFile> DescribeFileAsync(
        string root,
        string path,
        CancellationToken cancellationToken)
    {
        var relative = Path.GetRelativePath(root, path).Replace('\\', '/');
        return new RecoveryFile(
            relative,
            new FileInfo(path).Length,
            await HashFileAsync(path, cancellationToken).ConfigureAwait(false));
    }

    private static async Task<byte[]> ReadBoundedManifestAsync(
        string path,
        CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            64 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        var declaredLength = stream.Length;
        if (declaredLength <= 0 || declaredLength > MaximumManifestBytes)
        {
            throw new RecoveryManifestLimitException();
        }

        var bytes = GC.AllocateUninitializedArray<byte>((int)declaredLength);
        var totalRead = 0;
        while (totalRead < bytes.Length)
        {
            var read = await stream.ReadAsync(
                bytes.AsMemory(totalRead),
                cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                throw new RecoveryManifestLimitException();
            }

            totalRead += read;
        }

        var overflowProbe = new byte[1];
        if (await stream.ReadAsync(
                overflowProbe.AsMemory(),
                cancellationToken).ConfigureAwait(false) != 0)
        {
            throw new RecoveryManifestLimitException();
        }

        return bytes;
    }

    private static void PreflightManifestJson(ReadOnlySpan<byte> manifestBytes)
    {
        var reader = new Utf8JsonReader(
            manifestBytes,
            new JsonReaderOptions
            {
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow,
                MaxDepth = 8,
            });

        if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("The recovery manifest root must be an object.");
        }

        var filesPropertySeen = false;
        var awaitingFilesArray = false;
        var filesArrayDepth = -1;
        var filesArrayClosed = false;
        var fileCount = 0;
        var objectProperties = new Stack<HashSet<string>>();
        var reusablePropertySets = new Stack<HashSet<string>>();
        objectProperties.Push(new HashSet<string>(StringComparer.Ordinal));

        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.PropertyName or JsonTokenType.String &&
                GetManifestTokenByteLength(reader) > MaximumManifestTokenBytes)
            {
                throw new RecoveryManifestLimitException();
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString() ??
                    throw new JsonException("A recovery manifest property name is invalid.");
                if (objectProperties.Count == 0 ||
                    !objectProperties.Peek().Add(propertyName))
                {
                    throw new JsonException(
                        "A recovery manifest object contains a duplicated property.");
                }
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var properties = reusablePropertySets.TryPop(out var reusable)
                    ? reusable
                    : new HashSet<string>(StringComparer.Ordinal);
                objectProperties.Push(properties);
            }
            else if (reader.TokenType == JsonTokenType.EndObject)
            {
                if (!objectProperties.TryPop(out var properties))
                {
                    throw new JsonException("A recovery manifest object is unbalanced.");
                }

                properties.Clear();
                reusablePropertySets.Push(properties);
            }

            if (awaitingFilesArray)
            {
                if (reader.TokenType != JsonTokenType.StartArray)
                {
                    throw new JsonException("The recovery manifest files value must be an array.");
                }

                awaitingFilesArray = false;
                filesArrayDepth = reader.CurrentDepth;
                continue;
            }

            if (reader.TokenType == JsonTokenType.PropertyName &&
                reader.CurrentDepth == 1 &&
                reader.ValueTextEquals("files"u8))
            {
                if (filesPropertySeen)
                {
                    throw new JsonException("The recovery manifest files property is duplicated.");
                }

                filesPropertySeen = true;
                awaitingFilesArray = true;
                continue;
            }

            if (filesArrayDepth >= 0)
            {
                if (reader.TokenType == JsonTokenType.EndArray &&
                    reader.CurrentDepth == filesArrayDepth)
                {
                    filesArrayDepth = -1;
                    filesArrayClosed = true;
                    continue;
                }

                if (reader.CurrentDepth == filesArrayDepth + 1 &&
                    IsManifestArrayValueToken(reader.TokenType) &&
                    ++fileCount > MaximumManifestFileCount)
                {
                    throw new RecoveryManifestLimitException();
                }
            }
        }

        if (!filesPropertySeen || awaitingFilesArray || !filesArrayClosed)
        {
            throw new JsonException("The recovery manifest files array is incomplete.");
        }

        if (objectProperties.Count != 0)
        {
            throw new JsonException("The recovery manifest object is incomplete.");
        }
    }

    private static int GetManifestTokenByteLength(Utf8JsonReader reader) =>
        reader.HasValueSequence
            ? checked((int)reader.ValueSequence.Length)
            : reader.ValueSpan.Length;

    private static bool IsManifestArrayValueToken(JsonTokenType tokenType) =>
        tokenType is JsonTokenType.StartObject or
            JsonTokenType.StartArray or
            JsonTokenType.String or
            JsonTokenType.Number or
            JsonTokenType.True or
            JsonTokenType.False or
            JsonTokenType.Null;

    private static async Task<string> HashFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            64 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var sha256 = SHA256.Create();
        return Convert.ToHexString(
            await sha256.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false))
            .ToLowerInvariant();
    }

    private static async Task VerifyDatabaseAsync(
        string path,
        string label,
        List<string> failures,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            failures.Add($"{label} is missing");
            return;
        }

        try
        {
            StoragePathSafety.EnsureExistingPathIsNotReparsePoint(
                path,
                nameof(path));
            await using var connection = new SqliteConnection(
                SqliteConnectionStrings.Create(
                    path,
                    SqliteOpenMode.ReadOnly,
                    pooling: false));
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            await using var integrity = connection.CreateCommand();
            integrity.CommandText = "PRAGMA integrity_check;";
            var result = (string?)await integrity
                .ExecuteScalarAsync(cancellationToken)
                .ConfigureAwait(false);

            if (!string.Equals(result, "ok", StringComparison.Ordinal))
            {
                failures.Add($"{label} integrity_check did not return ok");
            }

            await using var foreignKeys = connection.CreateCommand();
            foreignKeys.CommandText = "PRAGMA foreign_key_check;";
            await using var reader = await foreignKeys
                .ExecuteReaderAsync(cancellationToken)
                .ConfigureAwait(false);

            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                failures.Add($"{label} foreign_key_check returned rows");
            }
        }
        catch (Exception exception) when (
            exception is SqliteException or ArgumentException or IOException or
                UnauthorizedAccessException)
        {
            failures.Add($"{label} could not be opened read-only");
        }
    }

    private static async Task VerifyAuthorityLinkAsync(
        string controlPath,
        string vectorPath,
        List<string> failures,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(controlPath) || !File.Exists(vectorPath))
        {
            return;
        }

        StoragePathSafety.EnsureExistingPathIsNotReparsePoint(
            controlPath,
            nameof(controlPath));
        StoragePathSafety.EnsureExistingPathIsNotReparsePoint(
            vectorPath,
            nameof(vectorPath));
        await using var control = new SqliteConnection(
            SqliteConnectionStrings.Create(
                controlPath,
                SqliteOpenMode.ReadOnly,
                pooling: false));
        await using var vectors = new SqliteConnection(
            SqliteConnectionStrings.Create(
                vectorPath,
                SqliteOpenMode.ReadOnly,
                pooling: false));
        await control.OpenAsync(cancellationToken).ConfigureAwait(false);
        await vectors.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var activeCommand = control.CreateCommand();
        activeCommand.CommandText = """
            SELECT gm.candidate_build_id,
                   gm.manifest_schema_version,
                   gm.corpus_id,
                   gm.corpus_revision,
                   gm.catalogue_revision,
                   gm.active_document_set_digest,
                   gm.source_binding_set_digest,
                   gm.index_compatibility_key,
                   gm.generation_spec_digest,
                   gm.chunk_count,
                   gm.vector_count,
                   gm.logical_artifact_digest,
                   gm.generation_content_digest,
                   gm.index_generation_id
            FROM activation_heads AS ah
            JOIN activation_records AS ar
              ON ar.corpus_id = ah.corpus_id
             AND ar.record_revision = ah.record_revision
            JOIN generation_manifests AS gm
              ON gm.corpus_id = ar.corpus_id
             AND gm.index_generation_id = ar.index_generation_id;
            """;
        var activeGenerations = new List<ActiveGenerationEvidence>();
        await using (var reader = await activeCommand
            .ExecuteReaderAsync(cancellationToken)
            .ConfigureAwait(false))
        {
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var manifest = new FinalisedIndexGenerationManifest(
                    reader.GetInt32(1),
                    new CorpusId(reader.GetString(2)),
                    new CorpusRevision(reader.GetInt64(3)),
                    new CatalogueRevision(reader.GetInt64(4)),
                    new ActiveDocumentSetDigest(reader.GetString(5)),
                    new SourceBindingSetDigest(reader.GetString(6)),
                    new IndexCompatibilityKey(reader.GetString(7)),
                    new GenerationSpecDigest(reader.GetString(8)),
                    reader.GetInt64(9),
                    reader.GetInt64(10),
                    new LogicalArtifactDigest(reader.GetString(11)),
                    new GenerationContentDigest(reader.GetString(12)),
                    new IndexGenerationId(reader.GetString(13)));
                activeGenerations.Add(new ActiveGenerationEvidence(
                    new CandidateBuildId(reader.GetString(0)),
                    manifest));
            }
        }

        foreach (var evidence in activeGenerations)
        {
            await using var buildCommand = vectors.CreateCommand();
            buildCommand.CommandText = """
                SELECT candidate_build_id,
                       corpus_id,
                       index_compatibility_key,
                       vector_dimensions,
                       expected_chunk_count
                FROM vector_builds
                WHERE status = 'Validated'
                  AND index_generation_id = $generationId;
                """;
            buildCommand.Parameters.AddWithValue(
                "$generationId",
                evidence.Manifest.IndexGenerationId.Value);
            await using var buildReader = await buildCommand
                .ExecuteReaderAsync(cancellationToken)
                .ConfigureAwait(false);

            if (!await buildReader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                failures.Add(
                    $"active generation {evidence.Manifest.IndexGenerationId.Value} has no validated vector build");
                continue;
            }

            var candidateBuildId = buildReader.GetString(0);
            var corpusId = buildReader.GetString(1);
            var compatibilityKey = buildReader.GetString(2);
            var dimensions = buildReader.GetInt32(3);
            var expectedChunkCount = buildReader.GetInt64(4);

            if (await buildReader.ReadAsync(cancellationToken).ConfigureAwait(false) ||
                candidateBuildId != evidence.CandidateBuildId.Value ||
                corpusId != evidence.Manifest.CorpusId.Value ||
                compatibilityKey != evidence.Manifest.IndexCompatibilityKey.Value ||
                expectedChunkCount != evidence.Manifest.ChunkCount)
            {
                failures.Add(
                    $"active generation {evidence.Manifest.IndexGenerationId.Value} has inconsistent vector-build identity");
                continue;
            }

            await buildReader.DisposeAsync().ConfigureAwait(false);
            await using var chunkCommand = vectors.CreateCommand();
            chunkCommand.CommandText = """
                SELECT chunk_ordinal,
                       document_id,
                       document_version,
                       chunk_digest,
                       chunk_text,
                       vector
                FROM vector_chunks
                WHERE candidate_build_id = $candidateBuildId
                ORDER BY chunk_ordinal;
                """;
            chunkCommand.Parameters.AddWithValue("$candidateBuildId", candidateBuildId);
            var artefacts = new List<LogicalIndexArtifact>();
            await using (var chunkReader = await chunkCommand
                .ExecuteReaderAsync(cancellationToken)
                .ConfigureAwait(false))
            {
                while (await chunkReader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    var decoded = StoredVectorChunkCodec.Decode(chunkReader.GetString(4));
                    artefacts.Add(new LogicalIndexArtifact(
                        chunkReader.GetInt64(0),
                        new DocumentId(chunkReader.GetString(1)),
                        new DocumentVersionNumber(chunkReader.GetInt64(2)),
                        new LogicalArtifactDigest(chunkReader.GetString(3)),
                        decoded.Text,
                        DecodeVector((byte[])chunkReader[5], dimensions)));
                }
            }

            var specification = new IndexGenerationSpecification(
                evidence.Manifest.ManifestSchemaVersion,
                evidence.Manifest.CorpusId,
                evidence.Manifest.CorpusRevision,
                evidence.Manifest.CatalogueRevision,
                evidence.Manifest.ActiveDocumentSetDigest,
                evidence.Manifest.SourceBindingSetDigest,
                evidence.Manifest.IndexCompatibilityKey);

            if (!IndexGenerationCanonicalizer.Matches(
                    evidence.Manifest,
                    specification,
                    artefacts))
            {
                failures.Add(
                    $"active generation {evidence.Manifest.IndexGenerationId.Value} failed canonical readback");
            }
        }

        await using var authorityCommand = vectors.CreateCommand();
        authorityCommand.CommandText = """
            SELECT COUNT(*)
            FROM sqlite_master
            WHERE type = 'table'
              AND name IN ('activation_heads', 'activation_records', 'generation_retention');
            """;
        var authorityTables = (long)(await authorityCommand
            .ExecuteScalarAsync(cancellationToken)
            .ConfigureAwait(false) ?? 0L);

        if (authorityTables != 0)
        {
            failures.Add("vectors.db contains prohibited active-authority tables");
        }
    }

    private static float[] DecodeVector(byte[] bytes, int dimensions)
    {
        if (bytes.Length != dimensions * sizeof(float))
        {
            throw new InvalidDataException(
                "A recovered vector does not match its declared dimensions.");
        }

        var vector = new float[dimensions];

        for (var index = 0; index < dimensions; index++)
        {
            vector[index] = BinaryPrimitives.ReadSingleLittleEndian(
                bytes.AsSpan(index * sizeof(float), sizeof(float)));
        }

        return vector;
    }

    private static bool IsValidManifestEntry(RecoveryFile file)
    {
        if (string.IsNullOrWhiteSpace(file.RelativePath) ||
            file.RelativePath.Length > MaximumManifestRelativePathLength ||
            string.IsNullOrWhiteSpace(file.Sha256) ||
            file.ByteLength <= 0 ||
            file.Sha256.Length != 64 ||
            file.Sha256.Any(character =>
                character is not (>= '0' and <= '9') and
                    not (>= 'a' and <= 'f')))
        {
            return false;
        }

        if (file.RelativePath is "control.db" or "vectors.db")
        {
            return true;
        }

        var segments = file.RelativePath.Split('/');

        if (segments.Length != 4 ||
            segments[0] != "content" ||
            segments[1] != "objects" ||
            segments[2].Length != 2 ||
            !segments[3].EndsWith(".bin", StringComparison.Ordinal))
        {
            return false;
        }

        var contentDigest = segments[3][..^4];
        return contentDigest.Length == 64 &&
            string.Equals(segments[2], contentDigest[..2], StringComparison.Ordinal) &&
            string.Equals(file.Sha256, contentDigest, StringComparison.Ordinal);
    }

    private static void AddAudit(
        ControlPlaneDbContext context,
        OperationId operationId,
        CorpusId corpusId,
        string eventType,
        DateTimeOffset occurredAt,
        string details)
    {
        context.AuditEvents.Add(new AuditEventRow
        {
            AuditEventId = $"audit-{Sha256($"{operationId.Value}\n{eventType}")}",
            OperationId = operationId.Value,
            CorpusId = corpusId.Value,
            EventType = eventType,
            OccurredAtUtc = ControlPlaneMapping.FormatUtc(occurredAt),
            DetailsDigest = Sha256(details),
        });
    }

    private static async Task<SqliteTransaction> BeginImmediateAsync(
        ControlPlaneDbContext context,
        CancellationToken cancellationToken)
    {
        await context.Database
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        var connection = (SqliteConnection)context.Database.GetDbConnection();
        var transaction = connection.BeginTransaction(deferred: false);
        context.Database.UseTransaction(transaction);
        return transaction;
    }

    private static bool IsWithin(string candidate, string root)
    {
        var fullCandidate = Path.GetFullPath(candidate);
        var fullRoot = Path.GetFullPath(root)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
            Path.DirectorySeparatorChar;
        return fullCandidate.StartsWith(fullRoot, StoragePathSafety.PathComparison);
    }

    private static string Sha256(string value) =>
        Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private sealed record RecoveryManifest(
        int SchemaVersion,
        string CorpusId,
        string OperationId,
        string CreatedAtUtc,
        IReadOnlyList<RecoveryFile?> Files);

    private sealed record RecoveryFile(
        string RelativePath,
        long ByteLength,
        string Sha256);

    private sealed record ActiveGenerationEvidence(
        CandidateBuildId CandidateBuildId,
        FinalisedIndexGenerationManifest Manifest);

    private sealed class RecoveryManifestLimitException : Exception
    {
    }
}
