// Purpose: Proves SQLite retrieval-v1 tie ordering and numerical fail-closed behaviour across write order and store reopen using only task-owned temporary data.
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Data.Sqlite;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;
using RagChallenge.Infrastructure.Persistence;

namespace RagChallenge.IntegrationTests;

public sealed class SqliteVectorRetrievalDeterminismTests
{
    [Fact]
    public async Task EqualScoresUseGlobalOrdinalAtTopKAcrossWriteOrderAndReopen()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var bindings = new[]
        {
            LocalCsvBinding("database-b", "document-b"),
            LocalCsvBinding("database-a", "document-a"),
        };
        var descriptor = new EmbeddingProviderDescriptor(
            "synthetic",
            "deterministic-v1",
            "retrieval-tie",
            dimensions: 2);
        var compatibilityKey = new IndexCompatibilityKey(Hash("tie-compatibility"));
        var candidate = new CandidateBuildId("candidate-retrieval-tie");
        var writes = Enumerable.Range(0, 9).Select(ordinal =>
        {
            var binding = ordinal % 2 == 0 ? bindings[0] : bindings[1];
            return new VectorChunkWrite(
                ordinal,
                binding.DocumentId,
                binding.DocumentVersion,
                new LogicalArtifactDigest(Hash($"tie-chunk-{ordinal}")),
                $"equal-score passage {ordinal}",
                new float[] { 1, 0 },
                DocumentContentLanguage.EnGb);
        }).ToArray();

        await fixture.VectorStore.CreateCandidateAsync(
            candidate,
            SqlitePersistenceFixture.CorpusId,
            compatibilityKey,
            vectorDimensions: 2,
            expectedChunkCount: writes.Length,
            SqlitePersistenceFixture.At(1));
        await fixture.VectorStore.AddChunksAsync(candidate, writes[4..].Reverse().ToArray());
        await fixture.VectorStore.AddChunksAsync(candidate, writes[..4].Reverse().ToArray());
        var manifest = await fixture.VectorStore.FinaliseCandidateAsync(
            candidate,
            Specification(bindings, compatibilityKey),
            SqlitePersistenceFixture.At(2));
        var request = CreateRequest(manifest, bindings, descriptor, new float[] { 1, 0 });
        var configuration = RetrievalPolicyConfiguration.CreateRetrievalV1(
            descriptor,
            compatibilityKey);
        var first = await new RetrievalV1PolicyExecutor(
            fixture.VectorStore,
            configuration).ExecuteAsync(request);
        var reopened = await new RetrievalV1PolicyExecutor(
            new SqliteVectorIndexStore(fixture.Options),
            configuration).ExecuteAsync(request);

        Assert.Equal(RetrievalPolicyOutcome.Succeeded, first.Outcome);
        Assert.Equal(RetrievalPolicyOutcome.Succeeded, reopened.Outcome);
        Assert.Equal(
            Enumerable.Range(0, 8).Select(value => (long)value),
            first.RankedHits.Select(hit => hit.ChunkOrdinal));
        Assert.Equal(
            first.RankedHits.Select(hit => hit.ChunkOrdinal),
            reopened.RankedHits.Select(hit => hit.ChunkOrdinal));
        Assert.Equal(
            Enumerable.Range(0, 6).Select(value => (long)value),
            first.SelectedEvidence.Select(item => item.Hit.ChunkOrdinal));
        Assert.All(first.RankedHits, hit => Assert.Equal(1, hit.Score, precision: 12));
    }

    [Fact]
    public async Task FiniteStoredComponentsWithNonFiniteNormReturnInvalidIndexData()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var bindings = new[] { LocalCsvBinding("database-overflow", "document-overflow") };
        var descriptor = new EmbeddingProviderDescriptor(
            "synthetic",
            "deterministic-v1",
            "retrieval-overflow",
            dimensions: 2);
        var compatibilityKey = new IndexCompatibilityKey(Hash("overflow-compatibility"));
        var candidate = new CandidateBuildId("candidate-retrieval-overflow");

        await fixture.VectorStore.CreateCandidateAsync(
            candidate,
            SqlitePersistenceFixture.CorpusId,
            compatibilityKey,
            vectorDimensions: 2,
            expectedChunkCount: 1,
            SqlitePersistenceFixture.At(1));
        await fixture.VectorStore.AddChunksAsync(candidate,
        [
            new VectorChunkWrite(
                0,
                bindings[0].DocumentId,
                bindings[0].DocumentVersion,
                new LogicalArtifactDigest(Hash("overflow-chunk")),
                "finite components with overflowing norm",
                new float[] { float.MaxValue, float.MaxValue },
                DocumentContentLanguage.EnGb),
        ]);
        var manifest = await fixture.VectorStore.FinaliseCandidateAsync(
            candidate,
            Specification(bindings, compatibilityKey),
            SqlitePersistenceFixture.At(2));
        var request = CreateRequest(manifest, bindings, descriptor, new float[] { 1, 0 });
        var result = await new RetrievalV1PolicyExecutor(
            fixture.VectorStore,
            RetrievalPolicyConfiguration.CreateRetrievalV1(
                descriptor,
                compatibilityKey)).ExecuteAsync(request);

        Assert.Equal(RetrievalPolicyOutcome.InvalidIndexData, result.Outcome);
        Assert.Equal("RETRIEVAL_INVALID_INDEX_DATA", result.FailureIdentity);
        Assert.Empty(result.RankedHits);
        Assert.Empty(result.SelectedEvidence);
    }

    [Fact]
    public async Task StoredZeroVectorProducesRawScoreZeroAndBelowMinimumSelection()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var bindings = new[]
        {
            LocalCsvBinding("database-zero-b", "document-zero-b"),
            LocalCsvBinding("database-zero-a", "document-zero-a"),
        };
        var descriptor = new EmbeddingProviderDescriptor(
            "synthetic",
            "deterministic-v1",
            "retrieval-zero",
            dimensions: 2);
        var compatibilityKey = new IndexCompatibilityKey(Hash("zero-compatibility"));
        var candidate = new CandidateBuildId("candidate-retrieval-zero");

        await fixture.VectorStore.CreateCandidateAsync(
            candidate,
            SqlitePersistenceFixture.CorpusId,
            compatibilityKey,
            vectorDimensions: 2,
            expectedChunkCount: 2,
            SqlitePersistenceFixture.At(1));
        await fixture.VectorStore.AddChunksAsync(candidate,
        [
            new VectorChunkWrite(
                1,
                bindings[1].DocumentId,
                bindings[1].DocumentVersion,
                new LogicalArtifactDigest(Hash("zero-chunk-1")),
                "valid zero-vector passage one",
                new float[] { 0, 0 },
                DocumentContentLanguage.EnGb),
            new VectorChunkWrite(
                0,
                bindings[0].DocumentId,
                bindings[0].DocumentVersion,
                new LogicalArtifactDigest(Hash("zero-chunk-0")),
                "valid zero-vector passage zero",
                new float[] { 0, 0 },
                DocumentContentLanguage.EnGb),
        ]);
        var manifest = await fixture.VectorStore.FinaliseCandidateAsync(
            candidate,
            Specification(bindings, compatibilityKey),
            SqlitePersistenceFixture.At(2));
        var result = await new RetrievalV1PolicyExecutor(
            fixture.VectorStore,
            RetrievalPolicyConfiguration.CreateRetrievalV1(
                descriptor,
                compatibilityKey)).ExecuteAsync(
                    CreateRequest(manifest, bindings, descriptor, new float[] { 1, 0 }));

        Assert.Equal(RetrievalPolicyOutcome.NoSelectedEvidenceUnderPolicy, result.Outcome);
        Assert.Equal(RetrievalNoEvidenceReason.BelowMinimumScore, result.NoEvidenceReason);
        Assert.Equal([0L, 1L], result.RankedHits.Select(hit => hit.ChunkOrdinal));
        Assert.All(result.RankedHits, hit => Assert.Equal(0, hit.Score));
        Assert.Empty(result.SelectedEvidence);
    }

    [Fact]
    public async Task NonFiniteStoredComponentReturnsInvalidIndexData()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var bindings = new[] { LocalCsvBinding("database-nan", "document-nan") };
        var descriptor = new EmbeddingProviderDescriptor(
            "synthetic",
            "deterministic-v1",
            "retrieval-nan",
            dimensions: 2);
        var compatibilityKey = new IndexCompatibilityKey(Hash("nan-compatibility"));
        var candidate = new CandidateBuildId("candidate-retrieval-nan");

        await fixture.VectorStore.CreateCandidateAsync(
            candidate,
            SqlitePersistenceFixture.CorpusId,
            compatibilityKey,
            vectorDimensions: 2,
            expectedChunkCount: 1,
            SqlitePersistenceFixture.At(1));
        await fixture.VectorStore.AddChunksAsync(candidate,
        [
            new VectorChunkWrite(
                0,
                bindings[0].DocumentId,
                bindings[0].DocumentVersion,
                new LogicalArtifactDigest(Hash("nan-chunk")),
                "passage corrupted after finalisation",
                new float[] { 1, 0 },
                DocumentContentLanguage.EnGb),
        ]);
        var manifest = await fixture.VectorStore.FinaliseCandidateAsync(
            candidate,
            Specification(bindings, compatibilityKey),
            SqlitePersistenceFixture.At(2));
        var corrupted = new byte[2 * sizeof(float)];
        BinaryPrimitives.WriteSingleLittleEndian(corrupted, float.NaN);
        BinaryPrimitives.WriteSingleLittleEndian(corrupted.AsSpan(sizeof(float)), 0);
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = fixture.Options.VectorDatabasePath,
            Mode = SqliteOpenMode.ReadWrite,
        }.ToString();
        await using (var connection = new SqliteConnection(connectionString))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText =
                "UPDATE vector_chunks SET vector = $vector WHERE candidate_build_id = $candidate;";
            command.Parameters.AddWithValue("$vector", corrupted);
            command.Parameters.AddWithValue("$candidate", candidate.Value);
            Assert.Equal(1, await command.ExecuteNonQueryAsync());
        }

        var result = await new RetrievalV1PolicyExecutor(
            fixture.VectorStore,
            RetrievalPolicyConfiguration.CreateRetrievalV1(
                descriptor,
                compatibilityKey)).ExecuteAsync(
                    CreateRequest(manifest, bindings, descriptor, new float[] { 1, 0 }));

        Assert.Equal(RetrievalPolicyOutcome.InvalidIndexData, result.Outcome);
        Assert.Equal("RETRIEVAL_INVALID_INDEX_DATA", result.FailureIdentity);
        Assert.Empty(result.RankedHits);
        Assert.Empty(result.SelectedEvidence);
    }

    private static RetrievalPolicyRequest CreateRequest(
        FinalisedIndexGenerationManifest manifest,
        IReadOnlyCollection<DocumentBinding> bindings,
        EmbeddingProviderDescriptor descriptor,
        ReadOnlyMemory<float> queryVector)
    {
        var evidence = bindings.Select(CreateEvidence).ToArray();
        var activation = ActivationRecordFactory.CreateInitial(
            manifest,
            evidence,
            SqlitePersistenceFixture.At(3));
        var queryBindings = bindings.Select((binding, index) => new QueryEvidenceBinding(
            binding,
            evidence[index],
            renderManifest: null,
            DocumentContentLanguage.EnGb,
            SourceFreshness.Local)).ToArray();
        var snapshot = new QueryActivationSnapshot(activation, queryBindings, manifest);
        var configuration = RetrievalPolicyConfiguration.CreateRetrievalV1(
            descriptor,
            manifest.IndexCompatibilityKey);
        return new RetrievalPolicyRequest(
            snapshot,
            queryBindings,
            queryVector,
            descriptor,
            SupportedQueryLanguage.EnGb,
            QueryContractVersion.V1,
            configuration);
    }

    private static IndexGenerationSpecification Specification(
        IReadOnlyCollection<DocumentBinding> bindings,
        IndexCompatibilityKey compatibilityKey) =>
        new(
            manifestSchemaVersion: 1,
            SqlitePersistenceFixture.CorpusId,
            new CorpusRevision(1),
            new CatalogueRevision(1),
            BindingDigestCanonicalizer.CanonicaliseActiveDocumentSet(bindings).Digest,
            BindingDigestCanonicalizer.CanonicaliseSourceBindingSet(bindings).Digest,
            compatibilityKey);

    private static DocumentBinding LocalCsvBinding(string productId, string documentId) =>
        new(
            new DatabaseProductId(productId),
            new DatabaseProductRevision(1),
            new DocumentId(documentId),
            new DocumentVersionNumber(1),
            DocumentFormat.Csv,
            new SourceAdapterId("local-csv"),
            SourceTrustClass.LocalAuthorised);

    private static DocumentActivationEvidenceBinding CreateEvidence(DocumentBinding binding)
    {
        var rights = new DocumentRightsEligibilityRecordV1(
            binding.DocumentId,
            binding.DocumentVersion,
            Enum.GetValues<DocumentRight>().Select(right => new DocumentRightDecision(
                right,
                DocumentRightDecisionState.Permitted,
                new DocumentRightsEvidenceReference($"retrieval-test-{right}"))));
        return new DocumentActivationEvidenceBinding(
            binding,
            new ContentObjectId(Hash($"source-{binding.DocumentId.Value}")),
            rights,
            renderManifestId: null);
    }

    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();
}
