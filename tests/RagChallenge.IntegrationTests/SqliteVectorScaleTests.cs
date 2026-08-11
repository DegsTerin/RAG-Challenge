// Purpose: Exercises the accepted 10,000-by-1,536 deterministic vector fixture against the derived SQLite store without claiming performance homologation.
using System.Globalization;

using Microsoft.Data.Sqlite;

using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.IntegrationTests;

public sealed class SqliteVectorScaleTests
{
    private const int ChunkCount = 10_000;
    private const int Dimensions = 1_536;
    private const int BatchSize = 1_000;

    [Fact]
    public async Task SyntheticTenThousandByOneThousandFiveHundredThirtySixFixtureRoundTrips()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var candidateId = new CandidateBuildId("candidate-scale-fixture");
        await fixture.VectorStore.CreateCandidateAsync(
            candidateId,
            SqlitePersistenceFixture.CorpusId,
            SqlitePersistenceFixture.CompatibilityKey,
            Dimensions,
            ChunkCount,
            SqlitePersistenceFixture.At(2));

        for (var batchStart = 0; batchStart < ChunkCount; batchStart += BatchSize)
        {
            var batch = new List<VectorChunkWrite>(BatchSize);

            for (var offset = 0; offset < BatchSize; offset++)
            {
                var ordinal = batchStart + offset;
                var vector = new float[Dimensions];
                vector[0] = 1;
                vector[1] = (ordinal + 1) / (float)ChunkCount;
                batch.Add(new VectorChunkWrite(
                    ordinal,
                    new DocumentId("doc-scale"),
                    new DocumentVersionNumber(1),
                    new LogicalArtifactDigest(SqlitePersistenceFixture.Hash(
                        $"scale-chunk:{ordinal.ToString(CultureInfo.InvariantCulture)}")),
                    $"synthetic scale chunk {ordinal.ToString(CultureInfo.InvariantCulture)}",
                    vector));
            }

            await fixture.VectorStore.AddChunksAsync(candidateId, batch);
        }

        var manifest = await fixture.VectorStore.FinaliseCandidateAsync(
            candidateId,
            new IndexGenerationSpecification(
                1,
                SqlitePersistenceFixture.CorpusId,
                new CorpusRevision(1),
                new CatalogueRevision(1),
                new ActiveDocumentSetDigest(SqlitePersistenceFixture.Hash("scale-documents")),
                new SourceBindingSetDigest(SqlitePersistenceFixture.Hash("scale-sources")),
                SqlitePersistenceFixture.CompatibilityKey),
            SqlitePersistenceFixture.At(2));
        var query = new float[Dimensions];
        query[0] = 1;
        var binding = new DocumentBinding(
            new DatabaseProductId("db-scale"),
            new DatabaseProductRevision(1),
            new DocumentId("doc-scale"),
            new DocumentVersionNumber(1),
            DocumentFormat.Pdf,
            new SourceAdapterId("local-scale"),
            SourceTrustClass.LocalAuthorised);
        var search = await fixture.VectorStore.SearchExactAsync(
            new VectorSearchRequest(
                SqlitePersistenceFixture.CorpusId,
                manifest.IndexGenerationId,
                manifest.IndexCompatibilityKey,
                query,
                maximumResults: 3,
                [VectorSearchBindingSelector.FromBinding(binding)]));

        Assert.Equal(VectorSearchOutcome.Succeeded, search.Outcome);
        var hits = search.Hits;
        Assert.Equal(3, hits.Count);
        Assert.Equal(0, hits[0].ChunkOrdinal);
        Assert.Equal(ChunkCount, await ScalarAsync(
            fixture.Options.VectorDatabasePath,
            "SELECT COUNT(*) FROM vector_chunks;"));
    }

    private static async Task<long> ScalarAsync(string path, string sql)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={path};Mode=ReadOnly;Cache=Private");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToInt64(
            await command.ExecuteScalarAsync(),
            CultureInfo.InvariantCulture);
    }
}
