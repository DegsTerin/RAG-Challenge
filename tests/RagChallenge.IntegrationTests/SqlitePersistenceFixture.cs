// Purpose: Builds deterministic, synthetic persistence fixtures in isolated temporary stores; no product corpus, provider, or operational path is used.
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;
using RagChallenge.Infrastructure.Persistence;

namespace RagChallenge.IntegrationTests;

internal sealed class SqlitePersistenceFixture : IAsyncDisposable
{
    private SqlitePersistenceFixture(string rootPath)
    {
        RootPath = rootPath;
        Options = new SqliteStoreOptions(
            Path.Combine(rootPath, "stores", "control.db"),
            Path.Combine(rootPath, "stores", "vectors.db"),
            Path.Combine(rootPath, "content"));
        ControlStore = new SqliteControlPlaneStore(Options);
        VectorStore = new SqliteVectorIndexStore(Options);
        ContentStore = new ImmutableContentStore(Options);
    }

    internal static CorpusId CorpusId { get; } = new("fixture-corpus");

    internal static IndexCompatibilityKey CompatibilityKey { get; } =
        new(Hash("fixture-compatibility"));

    internal string RootPath { get; }

    internal SqliteStoreOptions Options { get; }

    internal SqliteControlPlaneStore ControlStore { get; }

    internal SqliteVectorIndexStore VectorStore { get; }

    internal ImmutableContentStore ContentStore { get; }

    internal static DateTimeOffset At(int day) =>
        new(2026, 1, day, 12, 0, 0, TimeSpan.Zero);

    internal static async Task<SqlitePersistenceFixture> CreateAsync()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "rag-challenge-s03b-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var fixture = new SqlitePersistenceFixture(root);
        await SqliteStoreProvisioner.ApplyMigrationsAsync(fixture.Options);
        return fixture;
    }

    internal async Task<(CatalogueSnapshot Snapshot, DocumentBinding Binding)> CommitLocalCatalogueAsync(
        string contentText = "deterministic local fixture")
    {
        var bytes = Encoding.UTF8.GetBytes(contentText);
        await using var content = new MemoryStream(bytes, writable: false);
        var contentResult = await ContentStore.PutAsync(content, bytes.Length);
        var productId = new DatabaseProductId("db-fixture");
        var productRevision = new DatabaseProductRevision(1);
        var documentId = new DocumentId("doc-fixture");
        var documentVersion = new DocumentVersionNumber(1);
        var category = new DatabaseCategory(
            new DatabaseCategoryId("category-relational"),
            "Relational databases");
        var product = new DatabaseProduct(
            productId,
            productRevision,
            "Fixture Database",
            CatalogueItemStatus.Active,
            [category.Id]);
        var document = new DocumentVersion(
            documentId,
            documentVersion,
            productId,
            productRevision,
            DocumentFormat.Pdf,
            SupportedLanguage.EnGb,
            CatalogueItemStatus.Active,
            contentResult.ContentObjectId,
            contentResult.ByteLength,
            "application/pdf",
            new SourceAdapterId("local-fixture"),
            SourceTrustClass.LocalAuthorised);
        var snapshot = new CatalogueSnapshot(
            CorpusId,
            new CatalogueRevision(1),
            [category],
            [product],
            [document]);
        var commit = await ControlStore.CommitCatalogueAsync(
            new CatalogueCommitRequest(
                new OperationId("catalogue-1"),
                snapshot,
                ExpectedCurrentRevision: 0,
                At(1)));
        Assert.Equal(StoreMutationOutcome.Applied, commit.Outcome);
        var binding = new DocumentBinding(
            productId,
            productRevision,
            documentId,
            documentVersion,
            DocumentFormat.Pdf,
            document.SourceAdapterId,
            SourceTrustClass.LocalAuthorised);
        return (snapshot, binding);
    }

    internal async Task<FinalisedIndexGenerationManifest> CommitGenerationAsync(
        DocumentBinding binding,
        string seed,
        long chunkCount = 1)
    {
        var bindings = new[] { binding };
        var candidateId = new CandidateBuildId($"candidate-{seed}");
        await VectorStore.CreateCandidateAsync(
            candidateId,
            CorpusId,
            CompatibilityKey,
            vectorDimensions: 3,
            expectedChunkCount: chunkCount,
            At(2));

        for (var ordinal = 0L; ordinal < chunkCount; ordinal++)
        {
            await VectorStore.AddChunksAsync(
                candidateId,
                [new VectorChunkWrite(
                    ordinal,
                    binding.DocumentId,
                    binding.DocumentVersion,
                    new LogicalArtifactDigest(Hash($"chunk:{seed}:{ordinal}")),
                    $"synthetic chunk {seed} {ordinal}",
                    new float[] { 1, ordinal + 1, seed.Length })]);
        }

        var specification = new IndexGenerationSpecification(
            manifestSchemaVersion: 1,
            CorpusId,
            new CorpusRevision(1),
            new CatalogueRevision(1),
            BindingDigestCanonicalizer.CanonicaliseActiveDocumentSet(bindings).Digest,
            BindingDigestCanonicalizer.CanonicaliseSourceBindingSet(bindings).Digest,
            CompatibilityKey);
        var manifest = await VectorStore.FinaliseCandidateAsync(
            candidateId,
            specification,
            At(2));
        var commit = await ControlStore.CommitGenerationAsync(
            new GenerationCommitRequest(
                new OperationId($"generation-{seed}"),
                candidateId,
                manifest,
                bindings,
                At(2)));
        Assert.Equal(StoreMutationOutcome.Applied, commit.Outcome);
        return manifest;
    }

    internal async Task<long> ScalarAsync(string sql)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={Options.ControlDatabasePath};Mode=ReadOnly;Cache=Private");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToInt64(
            await command.ExecuteScalarAsync(),
            CultureInfo.InvariantCulture);
    }

    internal static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    public ValueTask DisposeAsync()
    {
        SqliteConnection.ClearAllPools();

        if (Directory.Exists(RootPath))
        {
            Directory.Delete(RootPath, recursive: true);
        }

        return ValueTask.CompletedTask;
    }
}
