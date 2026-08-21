// Purpose: Verifies that SQLite path delimiters retain physical identity and recovery manifests fail before unbounded or unsafe processing.
using System.Text;
using System.Text.Json;

using Microsoft.Data.Sqlite;

using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Infrastructure.Persistence;

namespace RagChallenge.IntegrationTests;

public sealed class SqliteRecoveryBoundaryTests
{
    [Fact]
    public async Task ConnectionStringDelimitersRemainPartOfTheExactDatabasePaths()
    {
        var root = CreateTestRoot();
        var controlPath = Path.Combine(root, "control;Mode=Memory;Cache=Shared.db");
        var vectorPath = Path.Combine(root, "vectors;Mode=Memory;Cache=Shared.db");
        var options = new SqliteStoreOptions(
            controlPath,
            vectorPath,
            Path.Combine(root, "content"));

        try
        {
            await SqliteStoreProvisioner.ApplyMigrationsAsync(options);
            SqliteConnection.ClearAllPools();

            Assert.True(File.Exists(controlPath));
            Assert.True(File.Exists(vectorPath));
            var connectionString = SqliteConnectionStrings.Create(
                controlPath,
                SqliteOpenMode.ReadOnly,
                pooling: false);
            var parsed = new SqliteConnectionStringBuilder(connectionString);
            Assert.Equal(Path.GetFullPath(controlPath), parsed.DataSource);
            Assert.Equal(SqliteOpenMode.ReadOnly, parsed.Mode);
            Assert.Equal(SqliteCacheMode.Private, parsed.Cache);
            Assert.False(parsed.Pooling);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RecoverySnapshotUsesExactDelimitedStoreAndRecoveryPaths()
    {
        var root = CreateTestRoot();
        var storeRoot = Path.Combine(root, "stores;Mode=Memory;Cache=Shared");
        var recoveryRoot = Path.Combine(root, "recovery;Mode=Memory;Cache=Shared");
        var options = new SqliteStoreOptions(
            Path.Combine(storeRoot, "control;Mode=Memory;Cache=Shared.db"),
            Path.Combine(storeRoot, "vectors;Mode=Memory;Cache=Shared.db"),
            Path.Combine(root, "content;Mode=Memory;Cache=Shared"));

        try
        {
            await SqliteStoreProvisioner.ApplyMigrationsAsync(options);
            await using (var connection = new SqliteConnection(
                SqliteConnectionStrings.Create(
                    options.ControlDatabasePath,
                    SqliteOpenMode.ReadWrite,
                    pooling: false)))
            {
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = """
                    INSERT INTO corpora (corpus_id, corpus_revision, created_at_utc)
                    VALUES ('recovery-delimited-corpus', 1, '2026-08-21T00:00:00.0000000+00:00');
                    """;
                await command.ExecuteNonQueryAsync();
            }

            var service = new SqliteRecoverySnapshotService(options);
            var snapshot = await service.CreateAndVerifyAsync(
                new OperationId("recovery-delimited-paths"),
                new CorpusId("recovery-delimited-corpus"),
                recoveryRoot,
                new DateTimeOffset(2026, 8, 21, 0, 0, 0, TimeSpan.Zero));
            var verification = await SqliteRecoverySnapshotService.VerifyIsolatedAsync(
                snapshot.SnapshotPath);

            Assert.StartsWith(
                Path.GetFullPath(recoveryRoot) + Path.DirectorySeparatorChar,
                snapshot.SnapshotPath,
                StringComparison.Ordinal);
            Assert.True(File.Exists(Path.Combine(snapshot.SnapshotPath, "control.db")));
            Assert.True(File.Exists(Path.Combine(snapshot.SnapshotPath, "vectors.db")));
            Assert.True(verification.IsValid, string.Join(Environment.NewLine, verification.Failures));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task OversizedRecoveryManifestFailsBeforeJsonParsing()
    {
        var root = CreateTestRoot();

        try
        {
            await using (var stream = new FileStream(
                Path.Combine(root, "recovery-manifest.json"),
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None))
            {
                stream.SetLength(SqliteRecoverySnapshotService.MaximumManifestBytes + 1);
            }

            var result = await SqliteRecoverySnapshotService.VerifyIsolatedAsync(root);

            Assert.False(result.IsValid);
            Assert.Equal(["recovery manifest exceeds its bounded limits"], result.Failures);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ExcessivelyDeepRecoveryManifestReturnsASanitisedFailure()
    {
        var root = CreateTestRoot();

        try
        {
            var nested = "{}";
            for (var depth = 0; depth < 12; depth++)
            {
                nested = $"{{\"nested\":{nested}}}";
            }

            await File.WriteAllTextAsync(
                Path.Combine(root, "recovery-manifest.json"),
                nested,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            var result = await SqliteRecoverySnapshotService.VerifyIsolatedAsync(root);

            Assert.False(result.IsValid);
            Assert.Equal(["recovery manifest JSON is invalid"], result.Failures);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ExcessiveRecoveryManifestEntriesFailBeforeFileTraversal()
    {
        var root = CreateTestRoot();

        try
        {
            Assert.Equal(1_000_002, SqliteRecoverySnapshotService.MaximumManifestFileCount);
            await using (var stream = new FileStream(
                Path.Combine(root, "recovery-manifest.json"),
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                64 * 1024,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            await using (var writer = new StreamWriter(
                stream,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                64 * 1024,
                leaveOpen: false))
            {
                await writer.WriteAsync(
                    "{\"schemaVersion\":1,\"corpusId\":\"recovery-boundary-corpus\"," +
                    "\"operationId\":\"recovery-boundary-operation\"," +
                    "\"createdAtUtc\":\"2026-08-21T00:00:00.0000000+00:00\",\"files\":[");
                for (var index = 0;
                    index <= SqliteRecoverySnapshotService.MaximumManifestFileCount;
                    index++)
                {
                    if (index != 0)
                    {
                        writer.Write(',');
                    }

                    writer.Write("{}");
                }

                await writer.FlushAsync();
            }

            var result = await SqliteRecoverySnapshotService.VerifyIsolatedAsync(root);

            Assert.False(result.IsValid);
            Assert.Equal(["recovery manifest exceeds its bounded limits"], result.Failures);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task NullRecoveryManifestEntriesReturnASanitisedFailure()
    {
        var root = CreateTestRoot();

        try
        {
            await WriteManifestAsync(root, "[null,null]");
            var result = await SqliteRecoverySnapshotService.VerifyIsolatedAsync(root);

            Assert.False(result.IsValid);
            Assert.Equal(
                ["manifest file identities are incomplete or duplicated"],
                result.Failures);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task OversizedRecoveryManifestStringTokenFailsDuringPreflight()
    {
        var root = CreateTestRoot();

        try
        {
            var oversizedToken = new string(
                'a',
                SqliteRecoverySnapshotService.MaximumManifestTokenBytes + 1);
            await WriteManifestAsync(
                root,
                $"[{{\"relativePath\":\"{oversizedToken}\"}},null]");
            var result = await SqliteRecoverySnapshotService.VerifyIsolatedAsync(root);

            Assert.False(result.IsValid);
            Assert.Equal(["recovery manifest exceeds its bounded limits"], result.Failures);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData(
        "{\"schemaVersion\":1,\"schemaVersion\":1," +
        "\"corpusId\":\"recovery-boundary-corpus\"," +
        "\"operationId\":\"recovery-boundary-operation\"," +
        "\"createdAtUtc\":\"2026-08-21T00:00:00.0000000+00:00\"," +
        "\"files\":[]}")]
    [InlineData(
        "{\"schemaVersion\":1,\"corpusId\":\"recovery-boundary-corpus\"," +
        "\"operationId\":\"recovery-boundary-operation\"," +
        "\"createdAtUtc\":\"2026-08-21T00:00:00.0000000+00:00\"," +
        "\"files\":[{\"relativePath\":\"control.db\"," +
        "\"relativePath\":\"vectors.db\",\"byteLength\":0," +
        "\"sha256\":\"0000000000000000000000000000000000000000000000000000000000000000\"}]}")]
    public async Task DuplicateRecoveryManifestPropertiesReturnASanitisedFailure(string json)
    {
        var root = CreateTestRoot();

        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(root, "recovery-manifest.json"),
                json,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            var result = await SqliteRecoverySnapshotService.VerifyIsolatedAsync(root);

            Assert.False(result.IsValid);
            Assert.Equal(["recovery manifest JSON is invalid"], result.Failures);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RecoveryVerificationRejectsAFileReparsePointWhenSupported()
    {
        var root = CreateTestRoot();

        try
        {
            var snapshotPath = await CreateRecoverySnapshotAsync(root);
            var controlPath = Path.Combine(snapshotPath, "control.db");
            var outsidePath = Path.Combine(root, "outside-control.db");
            File.Move(controlPath, outsidePath);

            try
            {
                _ = File.CreateSymbolicLink(controlPath, outsidePath);
            }
            catch (Exception exception) when (
                exception is UnauthorizedAccessException or IOException or
                    PlatformNotSupportedException)
            {
                return;
            }

            var result = await SqliteRecoverySnapshotService.VerifyIsolatedAsync(snapshotPath);

            Assert.False(result.IsValid);
            Assert.Contains(
                result.Failures,
                failure => failure.StartsWith(
                    "unsafe manifest file entry at index ",
                    StringComparison.Ordinal));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RecoveryLengthMismatchStopsBeforeHashingTheChangedFile()
    {
        var root = CreateTestRoot();

        try
        {
            var snapshotPath = await CreateRecoverySnapshotAsync(root);
            var vectorPath = Path.Combine(snapshotPath, "vectors.db");
            await using var exclusive = new FileStream(
                vectorPath,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None);
            exclusive.SetLength(exclusive.Length + (64L * 1024 * 1024));

            var result = await SqliteRecoverySnapshotService.VerifyIsolatedAsync(snapshotPath);

            Assert.False(result.IsValid);
            Assert.Contains("length mismatch: vectors.db", result.Failures);
            Assert.DoesNotContain(
                result.Failures,
                failure => failure.StartsWith(
                    "unsafe manifest file entry at index ",
                    StringComparison.Ordinal));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            Directory.Delete(root, recursive: true);
        }
    }

    private static async Task<string> CreateRecoverySnapshotAsync(string root)
    {
        var storeRoot = Path.Combine(root, "store");
        var options = new SqliteStoreOptions(
            Path.Combine(storeRoot, "control.db"),
            Path.Combine(storeRoot, "vectors.db"),
            Path.Combine(storeRoot, "content"));
        await SqliteStoreProvisioner.ApplyMigrationsAsync(options);
        await using (var connection = new SqliteConnection(
            SqliteConnectionStrings.Create(
                options.ControlDatabasePath,
                SqliteOpenMode.ReadWrite,
                pooling: false)))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO corpora (corpus_id, corpus_revision, created_at_utc)
                VALUES ('recovery-boundary-corpus', 1, '2026-08-21T00:00:00.0000000+00:00');
                """;
            await command.ExecuteNonQueryAsync();
        }

        var service = new SqliteRecoverySnapshotService(options);
        var snapshot = await service.CreateAndVerifyAsync(
            new OperationId("recovery-boundary-snapshot"),
            new CorpusId("recovery-boundary-corpus"),
            Path.Combine(root, "recovery"),
            new DateTimeOffset(2026, 8, 21, 0, 0, 0, TimeSpan.Zero));
        return snapshot.SnapshotPath;
    }

    private static Task WriteManifestAsync(string root, string filesJson) =>
        File.WriteAllTextAsync(
            Path.Combine(root, "recovery-manifest.json"),
            "{\"schemaVersion\":1,\"corpusId\":\"recovery-boundary-corpus\"," +
                "\"operationId\":\"recovery-boundary-operation\"," +
                "\"createdAtUtc\":\"2026-08-21T00:00:00.0000000+00:00\"," +
                $"\"files\":{filesJson}}}",
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

    private static string CreateTestRoot()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "rag-challenge-sqlite-recovery-boundary-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }
}
