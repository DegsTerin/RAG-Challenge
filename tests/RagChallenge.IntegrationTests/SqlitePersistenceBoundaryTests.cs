// Purpose: Verifies the physical SQLite authority split, immutable content publication, constraints, and exact derived-vector behaviour in temporary stores.
using System.Globalization;
using System.Text;

using Microsoft.Data.Sqlite;

using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;
using RagChallenge.Infrastructure.Persistence;

namespace RagChallenge.IntegrationTests;

public sealed class SqlitePersistenceBoundaryTests
{
    [Fact]
    public async Task MigrationsCreateValidStoresWithAuthorityOnlyInControlDatabase()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();

        Assert.Equal("ok", await ReadPragmaAsync(
            fixture.Options.ControlDatabasePath,
            "PRAGMA integrity_check;"));
        Assert.Equal("ok", await ReadPragmaAsync(
            fixture.Options.VectorDatabasePath,
            "PRAGMA integrity_check;"));
        Assert.Equal(0, await CountRowsAsync(
            fixture.Options.ControlDatabasePath,
            "PRAGMA foreign_key_check;"));
        Assert.Equal(0, await CountRowsAsync(
            fixture.Options.VectorDatabasePath,
            "PRAGMA foreign_key_check;"));

        var prohibitedAuthorityTables = await ScalarAsync(
            fixture.Options.VectorDatabasePath,
            """
            SELECT COUNT(*)
            FROM sqlite_master
            WHERE type = 'table'
              AND name IN (
                'corpora',
                'catalogue_heads',
                'generation_manifests',
                'activation_heads',
                'activation_records',
                'generation_retention',
                'admin_operations',
                'audit_events');
            """);
        Assert.Equal(0, prohibitedAuthorityTables);
        Assert.Equal(2, await ScalarAsync(
            fixture.Options.VectorDatabasePath,
            """
            SELECT COUNT(*)
            FROM sqlite_master
            WHERE type = 'table'
              AND name IN ('vector_builds', 'vector_chunks');
            """));
        Assert.True(await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            """
            SELECT COUNT(*)
            FROM sqlite_master
            WHERE type = 'table'
              AND name IN (
                'catalogue_heads',
                'generation_manifests',
                'activation_heads',
                'generation_retention',
                'audit_events');
            """) >= 5);
    }

    [Fact]
    public async Task ImmutableContentUsesSha256IdentityAndRejectsMismatches()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var bytes = Encoding.UTF8.GetBytes("synthetic immutable content");
        await using var firstStream = new MemoryStream(bytes, writable: false);
        var first = await fixture.ContentStore.PutAsync(firstStream, bytes.Length);
        Assert.False(first.AlreadyExisted);
        Assert.Equal(SqlitePersistenceFixture.Hash("synthetic immutable content"), first.ContentObjectId.Value);

        await using var repeatedStream = new MemoryStream(bytes, writable: false);
        var repeated = await fixture.ContentStore.PutAsync(
            repeatedStream,
            bytes.Length,
            first.ContentObjectId);
        Assert.True(repeated.AlreadyExisted);

        await using (var reopened = await fixture.ContentStore.OpenReadAsync(first.ContentObjectId))
        using (var reader = new StreamReader(reopened, Encoding.UTF8))
        {
            Assert.Equal("synthetic immutable content", await reader.ReadToEndAsync());
        }

        await using var mismatchStream = new MemoryStream(bytes, writable: false);
        await Assert.ThrowsAsync<InvalidDataException>(
            () => fixture.ContentStore.PutAsync(
                mismatchStream,
                bytes.Length,
                new ContentObjectId(new string('0', 64))));
        Assert.Empty(Directory.EnumerateFiles(
            Path.Combine(fixture.Options.ContentStoreRoot, "quarantine")));

        var objectPath = Path.Combine(
            fixture.Options.ContentStoreRoot,
            "objects",
            first.ContentObjectId.Value[..2],
            $"{first.ContentObjectId.Value}.bin");
        await File.WriteAllTextAsync(objectPath, "corrupted content");
        await Assert.ThrowsAsync<InvalidDataException>(async () =>
        {
            await using var _ = await fixture.ContentStore.OpenReadAsync(first.ContentObjectId);
        });
    }

    [Fact]
    public void StorePathsMustBeDistinctAndCannotUseFilesystemRootForContent()
    {
        var root = Path.GetPathRoot(Path.GetTempPath())!;
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            "rag-challenge-path-test",
            "store.db");

        Assert.Throws<ArgumentException>(() => new SqliteStoreOptions(
            databasePath,
            databasePath,
            Path.Combine(Path.GetTempPath(), "rag-challenge-content")));
        Assert.Throws<ArgumentException>(() => new SqliteStoreOptions(
            databasePath,
            Path.Combine(Path.GetDirectoryName(databasePath)!, "vectors.db"),
            root));
    }

    [Fact]
    public async Task ValidatedVectorsAreQueryableAndCandidatesRemainNonQueryable()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (_, binding) = await fixture.CommitLocalCatalogueAsync();
        var candidate = new CandidateBuildId("candidate-unvalidated");
        await fixture.VectorStore.CreateCandidateAsync(
            candidate,
            SqlitePersistenceFixture.CorpusId,
            SqlitePersistenceFixture.CompatibilityKey,
            vectorDimensions: 3,
            expectedChunkCount: 1,
            SqlitePersistenceFixture.At(2));
        await fixture.VectorStore.AddChunksAsync(
            candidate,
            [new VectorChunkWrite(
                0,
                binding.DocumentId,
                binding.DocumentVersion,
                new LogicalArtifactDigest(SqlitePersistenceFixture.Hash("candidate-chunk")),
                "candidate chunk",
                new float[] { 1, 0, 0 })]);
        var unvalidatedId = new IndexGenerationId(
            $"idxgen-{SqlitePersistenceFixture.Hash("unvalidated")}");
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            fixture.VectorStore.SearchExactAsync(
                new VectorSearchRequest(
                    SqlitePersistenceFixture.CorpusId,
                    unvalidatedId,
                    new float[] { 1, 0, 0 },
                    maximumResults: 1,
                    [binding])));

        var activeDigest = BindingDigestCanonicalizer
            .CanonicaliseActiveDocumentSet([binding])
            .Digest;
        var sourceDigest = BindingDigestCanonicalizer
            .CanonicaliseSourceBindingSet([binding])
            .Digest;
        var specification = new IndexGenerationSpecification(
            1,
            SqlitePersistenceFixture.CorpusId,
            new CorpusRevision(1),
            new CatalogueRevision(1),
            activeDigest,
            sourceDigest,
            SqlitePersistenceFixture.CompatibilityKey);
        var manifest = await fixture.VectorStore.FinaliseCandidateAsync(
            candidate,
            specification,
            SqlitePersistenceFixture.At(2));
        var idempotentReplay = await fixture.VectorStore.FinaliseCandidateAsync(
            candidate,
            specification,
            SqlitePersistenceFixture.At(3));
        var hits = await fixture.VectorStore.SearchExactAsync(
            new VectorSearchRequest(
                SqlitePersistenceFixture.CorpusId,
                manifest.IndexGenerationId,
                new float[] { 1, 0, 0 },
                maximumResults: 1,
                [binding]));
        var hit = Assert.Single(hits);
        Assert.Equal(manifest.IndexGenerationId, idempotentReplay.IndexGenerationId);
        Assert.Equal(0, hit.ChunkOrdinal);
        Assert.Equal(1, hit.Score, precision: 12);
    }

    [Fact]
    public async Task GenerationCommitRejectsVectorPayloadChangedAfterFinalisation()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (_, binding) = await fixture.CommitLocalCatalogueAsync();
        var candidate = new CandidateBuildId("candidate-corruption");
        await fixture.VectorStore.CreateCandidateAsync(
            candidate,
            SqlitePersistenceFixture.CorpusId,
            SqlitePersistenceFixture.CompatibilityKey,
            vectorDimensions: 3,
            expectedChunkCount: 1,
            SqlitePersistenceFixture.At(2));
        await fixture.VectorStore.AddChunksAsync(
            candidate,
            [new VectorChunkWrite(
                0,
                binding.DocumentId,
                binding.DocumentVersion,
                new LogicalArtifactDigest(SqlitePersistenceFixture.Hash("corruption-chunk")),
                "corruption sentinel",
                new float[] { 1, 2, 3 })]);
        var manifest = await fixture.VectorStore.FinaliseCandidateAsync(
            candidate,
            new IndexGenerationSpecification(
                1,
                SqlitePersistenceFixture.CorpusId,
                new CorpusRevision(1),
                new CatalogueRevision(1),
                BindingDigestCanonicalizer.CanonicaliseActiveDocumentSet([binding]).Digest,
                BindingDigestCanonicalizer.CanonicaliseSourceBindingSet([binding]).Digest,
                SqlitePersistenceFixture.CompatibilityKey),
            SqlitePersistenceFixture.At(2));
        await ExecuteAsync(
            fixture.Options.VectorDatabasePath,
            "UPDATE vector_chunks SET vector = zeroblob(length(vector));");

        var result = await fixture.ControlStore.CommitGenerationAsync(
            new GenerationCommitRequest(
                new OperationId("generation-corruption-rejected"),
                candidate,
                manifest,
                [binding],
                SqlitePersistenceFixture.At(2)));

        Assert.Equal(StoreMutationOutcome.ValidationFailed, result.Outcome);
        Assert.Equal(0, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM generation_manifests;"));
    }

    private static async Task<string?> ReadPragmaAsync(string path, string sql)
    {
        await using var connection = OpenReadOnly(path);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToString(
            await command.ExecuteScalarAsync(),
            CultureInfo.InvariantCulture);
    }

    private static async Task<long> CountRowsAsync(string path, string sql)
    {
        await using var connection = OpenReadOnly(path);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await using var reader = await command.ExecuteReaderAsync();
        var count = 0L;

        while (await reader.ReadAsync())
        {
            count++;
        }

        return count;
    }

    private static async Task<long> ScalarAsync(string path, string sql)
    {
        await using var connection = OpenReadOnly(path);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToInt64(
            await command.ExecuteScalarAsync(),
            CultureInfo.InvariantCulture);
    }

    private static async Task ExecuteAsync(string path, string sql)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={path};Mode=ReadWrite;Cache=Private");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        _ = await command.ExecuteNonQueryAsync();
    }

    private static SqliteConnection OpenReadOnly(string path) =>
        new($"Data Source={path};Mode=ReadOnly;Cache=Private");
}
