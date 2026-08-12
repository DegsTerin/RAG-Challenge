// Purpose: Proves SQLite retrieval-v2 numerical semantics, pre-top-k filtering, total ordering and fail-closed behaviour using task-owned temporary data.
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Data.Sqlite;

using RagChallenge.Application.Documents;
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;
using RagChallenge.Infrastructure.Persistence;

namespace RagChallenge.IntegrationTests;

public sealed class SqliteVectorRetrievalDeterminismTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CompleteCompositeSortPrecedesTopKAcrossAdversarialWriteOrderAndReopen(
        bool alternateBatchPermutation)
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
            "retrieval-top-k",
            dimensions: 2);
        var compatibilityKey = CompatibilityKey(descriptor);
        var candidate = new CandidateBuildId("candidate-retrieval-adversarial-top-k");
        var writes = Enumerable.Range(0, 9).Select(ordinal =>
        {
            var binding = ordinal % 2 == 0 ? bindings[0] : bindings[1];
            var vector = ordinal switch
            {
                0 => new float[] { 0, 1 },
                1 => new float[] { 5, 12 },
                2 => new float[] { 3, 4 },
                3 or 4 => new float[] { 8, 6 },
                5 => new float[] { 12, 5 },
                6 => new float[] { 24, 7 },
                7 => new float[] { 40, 9 },
                8 => new float[] { 1, 0 },
                _ => throw new ArgumentOutOfRangeException(nameof(ordinal)),
            };
            return new VectorChunkWrite(
                ordinal,
                binding.DocumentId,
                binding.DocumentVersion,
                new LogicalArtifactDigest(Hash($"top-k-chunk-{ordinal}")),
                $"adversarial top-k passage {ordinal}",
                vector,
                DocumentContentLanguage.EnGb);
        }).ToArray();

        await fixture.VectorStore.CreateCandidateAsync(
            candidate,
            SqlitePersistenceFixture.CorpusId,
            compatibilityKey,
            vectorDimensions: 2,
            expectedChunkCount: writes.Length,
            SqlitePersistenceFixture.At(1));
        var batches = alternateBatchPermutation
            ? new[]
            {
                writes.Where(chunk => chunk.ChunkOrdinal % 2 == 0).Reverse().ToArray(),
                writes.Where(chunk => chunk.ChunkOrdinal % 2 != 0).Reverse().ToArray(),
            }
            : new[]
            {
                writes[4..].Reverse().ToArray(),
                writes[..4].Reverse().ToArray(),
            };
        await fixture.VectorStore.AddChunksAsync(candidate, batches[0]);
        await fixture.VectorStore.AddChunksAsync(candidate, batches[1]);
        var manifest = await fixture.VectorStore.FinaliseCandidateAsync(
            candidate,
            Specification(bindings, compatibilityKey),
            SqlitePersistenceFixture.At(2));
        var request = CreateRequest(manifest, bindings, descriptor, new float[] { 1, 0 });
        var configuration = RetrievalPolicyConfiguration.CreateRetrievalV2(
            descriptor,
            compatibilityKey);
        var executor = new RetrievalV2PolicyExecutor(
            fixture.VectorStore,
            configuration);
        var first = await executor.ExecuteAsync(request);
        var replayed = await executor.ExecuteAsync(request);
        var reopened = await new RetrievalV2PolicyExecutor(
            new SqliteVectorIndexStore(fixture.Options),
            configuration).ExecuteAsync(request);

        Assert.Equal(RetrievalPolicyOutcome.Succeeded, first.Outcome);
        Assert.Equal(RetrievalPolicyOutcome.Succeeded, replayed.Outcome);
        Assert.Equal(RetrievalPolicyOutcome.Succeeded, reopened.Outcome);
        var expectedOrdinals = new long[] { 8, 7, 6, 5, 3, 4, 2, 1 };
        var expectedScoreBits = new[]
        {
            Bits(1d),
            Bits(40d / 41d),
            Bits(24d / 25d),
            Bits(12d / 13d),
            Bits(0.8d),
            Bits(0.8d),
            Bits(0.6d),
            Bits(5d / 13d),
        };
        Assert.Equal(expectedOrdinals, first.RankedHits.Select(hit => hit.ChunkOrdinal));
        Assert.DoesNotContain(first.RankedHits, hit => hit.ChunkOrdinal == 0);
        Assert.Equal(
            expectedOrdinals.Select(ordinal => Hash($"top-k-chunk-{ordinal}")),
            first.RankedHits.Select(hit => hit.ChunkDigest.Value));
        Assert.Equal(expectedScoreBits, first.RankedHits.Select(hit => Bits(hit.Score)));
        Assert.Equal(
            first.RankedHits.Select(hit => (
                hit.ChunkOrdinal,
                hit.ChunkDigest.Value,
                ScoreBits: Bits(hit.Score))),
            replayed.RankedHits.Select(hit => (
                hit.ChunkOrdinal,
                hit.ChunkDigest.Value,
                ScoreBits: Bits(hit.Score))));
        Assert.Equal(
            first.RankedHits.Select(hit => (
                hit.ChunkOrdinal,
                hit.ChunkDigest.Value,
                ScoreBits: Bits(hit.Score))),
            reopened.RankedHits.Select(hit => (
                hit.ChunkOrdinal,
                hit.ChunkDigest.Value,
                ScoreBits: Bits(hit.Score))));
        Assert.Equal(
            expectedOrdinals[..6],
            first.SelectedEvidence.Select(item => item.Hit.ChunkOrdinal));
    }

    [Fact]
    public async Task BoundaryCanonicalisationAndInteriorScoresAreBitExactAcrossReopen()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var bindings = new[] { LocalCsvBinding("database-boundary", "document-boundary") };
        var descriptor = new EmbeddingProviderDescriptor(
            "synthetic",
            "deterministic-v1",
            "retrieval-boundary",
            dimensions: 3);
        var compatibilityKey = CompatibilityKey(descriptor);
        var candidate = new CandidateBuildId("candidate-retrieval-boundary");

        await fixture.VectorStore.CreateCandidateAsync(
            candidate,
            SqlitePersistenceFixture.CorpusId,
            compatibilityKey,
            vectorDimensions: 3,
            expectedChunkCount: 4,
            SqlitePersistenceFixture.At(1));
        await fixture.VectorStore.AddChunksAsync(candidate,
        [
            Chunk(0, bindings[0], "boundary-positive", new float[] { 1, 1, 1 }),
            Chunk(1, bindings[0], "boundary-negative", new float[] { -1, -1, -1 }),
            Chunk(2, bindings[0], "boundary-interior", new float[] { 1, 0, 1 }),
            Chunk(3, bindings[0], "boundary-zero", new float[] { 0, 0, 0 }),
        ]);
        var manifest = await fixture.VectorStore.FinaliseCandidateAsync(
            candidate,
            Specification(bindings, compatibilityKey),
            SqlitePersistenceFixture.At(2));
        var request = CreateRequest(
            manifest,
            bindings,
            descriptor,
            new float[] { 1, 1, 1 });
        var configuration = RetrievalPolicyConfiguration.CreateRetrievalV2(
            descriptor,
            compatibilityKey);
        var first = await new RetrievalV2PolicyExecutor(
            fixture.VectorStore,
            configuration).ExecuteAsync(request);
        var reopened = await new RetrievalV2PolicyExecutor(
            new SqliteVectorIndexStore(fixture.Options),
            configuration).ExecuteAsync(request);

        var rawPositiveBoundary = 3d / (Math.Sqrt(3d) * Math.Sqrt(3d));
        var rawNegativeBoundary = -3d / (Math.Sqrt(3d) * Math.Sqrt(3d));
        Assert.Equal(Bits(1.0000000000000002d), Bits(rawPositiveBoundary));
        Assert.Equal(Bits(-1.0000000000000002d), Bits(rawNegativeBoundary));
        Assert.Equal(RetrievalPolicyOutcome.Succeeded, first.Outcome);
        Assert.Equal([0L, 2L, 3L, 1L], first.RankedHits.Select(hit => hit.ChunkOrdinal));
        Assert.Equal(
            [
                Bits(1d),
                Bits(2d / (Math.Sqrt(3d) * Math.Sqrt(2d))),
                Bits(0d),
                Bits(-1d),
            ],
            first.RankedHits.Select(hit => Bits(hit.Score)));
        Assert.Equal(0L, Bits(first.RankedHits.Single(hit => hit.ChunkOrdinal == 3).Score));
        Assert.Equal(
            first.RankedHits.Select(hit => (hit.ChunkOrdinal, ScoreBits: Bits(hit.Score))),
            reopened.RankedHits.Select(hit => (hit.ChunkOrdinal, ScoreBits: Bits(hit.Score))));
    }

    [Fact]
    public void AdjacentBoundaryValuesAndSignedZeroFollowExactBitRules()
    {
        var negativeZero = BitConverter.Int64BitsToDouble(long.MinValue);
        var cases = new[]
        {
            (Raw: Math.BitDecrement(1d), Expected: Math.BitDecrement(1d)),
            (Raw: Math.BitIncrement(1d), Expected: 1d),
            (Raw: Math.BitIncrement(-1d), Expected: Math.BitIncrement(-1d)),
            (Raw: Math.BitDecrement(-1d), Expected: -1d),
            (Raw: double.MaxValue, Expected: 1d),
            (Raw: -double.MaxValue, Expected: -1d),
            (Raw: 0d, Expected: 0d),
            (Raw: negativeZero, Expected: negativeZero),
        };

        foreach (var item in cases)
        {
            Assert.True(SqliteVectorIndexStore.TryCanonicaliseCosineBoundary(
                item.Raw,
                out var score));
            Assert.Equal(Bits(item.Expected), Bits(score));
        }

        foreach (var value in new[]
                 {
                     double.NaN,
                     double.PositiveInfinity,
                     double.NegativeInfinity,
                 })
        {
            Assert.False(SqliteVectorIndexStore.TryCanonicaliseCosineBoundary(
                value,
                out _));
        }
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
        var compatibilityKey = CompatibilityKey(descriptor);
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
        var result = await new RetrievalV2PolicyExecutor(
            fixture.VectorStore,
            RetrievalPolicyConfiguration.CreateRetrievalV2(
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
        var compatibilityKey = CompatibilityKey(descriptor);
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
        var result = await new RetrievalV2PolicyExecutor(
            fixture.VectorStore,
            RetrievalPolicyConfiguration.CreateRetrievalV2(
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
        var compatibilityKey = CompatibilityKey(descriptor);
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

        var result = await new RetrievalV2PolicyExecutor(
            fixture.VectorStore,
            RetrievalPolicyConfiguration.CreateRetrievalV2(
                descriptor,
                compatibilityKey)).ExecuteAsync(
                    CreateRequest(manifest, bindings, descriptor, new float[] { 1, 0 }));

        Assert.Equal(RetrievalPolicyOutcome.InvalidIndexData, result.Outcome);
        Assert.Equal("RETRIEVAL_INVALID_INDEX_DATA", result.FailureIdentity);
        Assert.Empty(result.RankedHits);
        Assert.Empty(result.SelectedEvidence);
    }

    [Fact]
    public async Task CandidateEligibilityDatabaseAndDocumentFiltersAllPrecedeScoringAndTopK()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var target = LocalCsvBinding("database-target", "document-target");
        var eligibilityExcluded = Enumerable.Range(0, 3)
            .Select(index => LocalCsvBinding(
                "database-target",
                $"document-eligibility-excluded-{index}"))
            .ToArray();
        var databaseExcluded = Enumerable.Range(0, 3)
            .Select(index => LocalCsvBinding(
                $"database-excluded-{index}",
                $"document-database-excluded-{index}"))
            .ToArray();
        var documentExcluded = Enumerable.Range(0, 3)
            .Select(index => LocalCsvBinding(
                "database-target",
                $"document-filter-excluded-{index}"))
            .ToArray();
        DocumentBinding[] bindings =
        [
            target,
            .. eligibilityExcluded,
            .. databaseExcluded,
            .. documentExcluded,
        ];
        var descriptor = new EmbeddingProviderDescriptor(
            "synthetic",
            "deterministic-v1",
            "retrieval-filter-order",
            dimensions: 2);
        var compatibilityKey = CompatibilityKey(descriptor);
        var targetCandidate = new CandidateBuildId("candidate-filter-target");

        await fixture.VectorStore.CreateCandidateAsync(
            targetCandidate,
            SqlitePersistenceFixture.CorpusId,
            compatibilityKey,
            vectorDimensions: 2,
            expectedChunkCount: bindings.Length,
            SqlitePersistenceFixture.At(1));
        var competingChunks = bindings.Skip(1).Select((binding, index) =>
            Chunk(index + 1, binding, $"filter-competitor-{index}", new float[] { 1, 0 }))
            .ToArray();
        Assert.Equal(9, competingChunks.Length);
        await fixture.VectorStore.AddChunksAsync(targetCandidate,
        [
            Chunk(0, target, "filter-target", new float[] { 3, 4 }),
            .. competingChunks,
        ]);
        var targetManifest = await fixture.VectorStore.FinaliseCandidateAsync(
            targetCandidate,
            Specification(bindings, compatibilityKey),
            SqlitePersistenceFixture.At(2));

        var competingCandidate = new CandidateBuildId("candidate-filter-other-generation");
        await fixture.VectorStore.CreateCandidateAsync(
            competingCandidate,
            SqlitePersistenceFixture.CorpusId,
            compatibilityKey,
            vectorDimensions: 2,
            expectedChunkCount: 1,
            SqlitePersistenceFixture.At(3));
        await fixture.VectorStore.AddChunksAsync(competingCandidate,
        [
            Chunk(100, target, "filter-other-generation", new float[] { 1, 0 }),
        ]);
        var competingManifest = await fixture.VectorStore.FinaliseCandidateAsync(
            competingCandidate,
            Specification([target], compatibilityKey),
            SqlitePersistenceFixture.At(4));
        Assert.NotEqual(targetManifest.IndexGenerationId, competingManifest.IndexGenerationId);

        VectorSearchBindingSelector[] eligibleSelectors =
        [
            VectorSearchBindingSelector.FromBinding(target),
            .. databaseExcluded.Select(VectorSearchBindingSelector.FromBinding),
            .. documentExcluded.Select(VectorSearchBindingSelector.FromBinding),
        ];
        DatabaseProductId[] databaseFilters = [target.DatabaseProductId];
        DocumentId[] documentFilters =
        [
            target.DocumentId,
            .. eligibilityExcluded.Select(binding => binding.DocumentId),
            .. databaseExcluded.Select(binding => binding.DocumentId),
        ];
        var request = new VectorSearchRequest(
            SqlitePersistenceFixture.CorpusId,
            targetManifest.IndexGenerationId,
            compatibilityKey,
            new float[] { 1, 0 },
            maximumResults: 8,
            eligibleSelectors,
            databaseFilters,
            documentFilters);
        var first = await fixture.VectorStore.SearchExactAsync(request);
        var reopened = await new SqliteVectorIndexStore(fixture.Options).SearchExactAsync(request);

        Assert.Equal(VectorSearchOutcome.Succeeded, first.Outcome);
        var hit = Assert.Single(first.Hits);
        Assert.Equal(targetCandidate, hit.CandidateBuildId);
        Assert.Equal(0, hit.ChunkOrdinal);
        Assert.Equal(Hash("filter-target"), hit.ChunkDigest.Value);
        Assert.Equal(Bits(0.6d), Bits(hit.Score));
        Assert.Equal(
            first.Hits.Select(item => (
                item.CandidateBuildId.Value,
                item.ChunkOrdinal,
                item.ChunkDigest.Value,
                ScoreBits: Bits(item.Score))),
            reopened.Hits.Select(item => (
                item.CandidateBuildId.Value,
                item.ChunkOrdinal,
                item.ChunkDigest.Value,
                ScoreBits: Bits(item.Score))));

        var evidence = bindings.Select(CreateEvidence).ToArray();
        var activation = ActivationRecordFactory.CreateInitial(
            targetManifest,
            evidence,
            SqlitePersistenceFixture.At(5));
        var queryBindings = bindings.Select((binding, index) => new QueryEvidenceBinding(
            binding,
            evidence[index],
            renderManifest: null,
            eligibilityExcluded.Contains(binding)
                ? new DocumentContentLanguage("fr")
                : DocumentContentLanguage.EnGb,
            SourceFreshness.Local)).ToArray();
        var policyConfiguration = RetrievalPolicyConfiguration.CreateRetrievalV2(
            descriptor,
            compatibilityKey);
        var policyRequest = new RetrievalPolicyRequest(
            new QueryActivationSnapshot(activation, queryBindings, targetManifest),
            [queryBindings[0]],
            new float[] { 1, 0 },
            descriptor,
            SupportedQueryLanguage.EnGb,
            QueryContractVersion.V1,
            policyConfiguration,
            databaseFilters,
            documentFilters);
        var policyResult = await new RetrievalV2PolicyExecutor(
            new SqliteVectorIndexStore(fixture.Options),
            policyConfiguration).ExecuteAsync(policyRequest);

        Assert.Equal(RetrievalPolicyOutcome.Succeeded, policyResult.Outcome);
        Assert.Equal([0L], policyResult.RankedHits.Select(item => item.ChunkOrdinal));
        Assert.Equal(
            [Hash("filter-target")],
            policyResult.RankedHits.Select(item => item.ChunkDigest.Value));
        var selected = Assert.Single(policyResult.SelectedEvidence);
        Assert.Equal(0, selected.Hit.ChunkOrdinal);
        Assert.Equal(Hash("filter-target"), selected.Hit.ChunkDigest.Value);
    }

    [Fact]
    public async Task NegativeStoredOrdinalFailsExactRetrievalClosedWithoutHitsOrEvidence()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var bindings = new[] { LocalCsvBinding("database-negative", "document-negative") };
        var descriptor = new EmbeddingProviderDescriptor(
            "synthetic",
            "deterministic-v1",
            "retrieval-negative-ordinal",
            dimensions: 2);
        var compatibilityKey = CompatibilityKey(descriptor);
        var candidate = new CandidateBuildId("candidate-negative-ordinal");

        await fixture.VectorStore.CreateCandidateAsync(
            candidate,
            SqlitePersistenceFixture.CorpusId,
            compatibilityKey,
            vectorDimensions: 2,
            expectedChunkCount: 1,
            SqlitePersistenceFixture.At(1));
        await fixture.VectorStore.AddChunksAsync(candidate,
        [
            Chunk(0, bindings[0], "negative-ordinal", new float[] { 1, 0 }),
        ]);
        var manifest = await fixture.VectorStore.FinaliseCandidateAsync(
            candidate,
            Specification(bindings, compatibilityKey),
            SqlitePersistenceFixture.At(2));
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = fixture.Options.VectorDatabasePath,
            Mode = SqliteOpenMode.ReadWrite,
        }.ToString();
        await using (var connection = new SqliteConnection(connectionString))
        {
            await connection.OpenAsync();
            await using (var pragma = connection.CreateCommand())
            {
                pragma.CommandText = "PRAGMA ignore_check_constraints = ON;";
                await pragma.ExecuteNonQueryAsync();
            }

            await using var command = connection.CreateCommand();
            command.CommandText =
                "UPDATE vector_chunks SET chunk_ordinal = -1 WHERE candidate_build_id = $candidate;";
            command.Parameters.AddWithValue("$candidate", candidate.Value);
            Assert.Equal(1, await command.ExecuteNonQueryAsync());
        }

        var result = await new RetrievalV2PolicyExecutor(
            new SqliteVectorIndexStore(fixture.Options),
            RetrievalPolicyConfiguration.CreateRetrievalV2(
                descriptor,
                compatibilityKey)).ExecuteAsync(
                    CreateRequest(manifest, bindings, descriptor, new float[] { 1, 0 }));

        Assert.Equal(RetrievalPolicyOutcome.InvalidIndexData, result.Outcome);
        Assert.Equal("RETRIEVAL_INVALID_INDEX_DATA", result.FailureIdentity);
        Assert.Empty(result.RankedHits);
        Assert.Empty(result.SelectedEvidence);
    }

    [Fact]
    public async Task V2DescriptorAdvancesCompatibilityKeyAndRejectsLegacyGeneration()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        const string legacyDescriptor =
            "sqlite-exact-vector-store/1;schema=1;distance=cosine;algorithm=exact-scan;vector=float32";
        Assert.Equal(
            "cosine-f32mul-f64acc-boundary-canonical-v1",
            SqliteVectorIndexStore.CosineNumericalSemantics);
        Assert.Equal(
            "sqlite-exact-vector-store/2;schema=1;distance=cosine;algorithm=exact-scan;vector=float32;score=cosine-f32mul-f64acc-boundary-canonical-v1",
            SqliteVectorIndexStore.CompatibilityDescriptor);
        var bindings = new[] { LocalCsvBinding("database-legacy", "document-legacy") };
        var descriptor = new EmbeddingProviderDescriptor(
            "synthetic",
            "deterministic-v1",
            "retrieval-compatibility",
            dimensions: 2);
        var currentKey = CompatibilityKey(descriptor);
        var legacyKey = CompatibilityKey(descriptor, legacyDescriptor);
        Assert.NotEqual(currentKey, legacyKey);
        var candidate = new CandidateBuildId("candidate-legacy-semantics");

        await fixture.VectorStore.CreateCandidateAsync(
            candidate,
            SqlitePersistenceFixture.CorpusId,
            legacyKey,
            vectorDimensions: 2,
            expectedChunkCount: 1,
            SqlitePersistenceFixture.At(1));
        await fixture.VectorStore.AddChunksAsync(candidate,
        [
            Chunk(0, bindings[0], "legacy-semantics", new float[] { 1, 0 }),
        ]);
        var manifest = await fixture.VectorStore.FinaliseCandidateAsync(
            candidate,
            Specification(bindings, legacyKey),
            SqlitePersistenceFixture.At(2));
        var currentPolicy = RetrievalPolicyConfiguration.CreateRetrievalV2(
            descriptor,
            currentKey);
        var policyResult = await new RetrievalV2PolicyExecutor(
            fixture.VectorStore,
            currentPolicy).ExecuteAsync(CreateRequest(
                manifest,
                bindings,
                descriptor,
                new float[] { 1, 0 },
                currentPolicy));
        var storeResult = await fixture.VectorStore.SearchExactAsync(new VectorSearchRequest(
            SqlitePersistenceFixture.CorpusId,
            manifest.IndexGenerationId,
            currentKey,
            new float[] { 1, 0 },
            maximumResults: 8,
            [VectorSearchBindingSelector.FromBinding(bindings[0])]));

        Assert.Equal(RetrievalPolicyOutcome.GenerationUnavailable, policyResult.Outcome);
        Assert.Empty(policyResult.RankedHits);
        Assert.Empty(policyResult.SelectedEvidence);
        Assert.Equal(VectorSearchOutcome.GenerationUnavailable, storeResult.Outcome);
        Assert.Empty(storeResult.Hits);
    }

    private static RetrievalPolicyRequest CreateRequest(
        FinalisedIndexGenerationManifest manifest,
        IReadOnlyCollection<DocumentBinding> bindings,
        EmbeddingProviderDescriptor descriptor,
        ReadOnlyMemory<float> queryVector,
        RetrievalPolicyConfiguration? applicablePolicy = null)
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
        var configuration = applicablePolicy ?? RetrievalPolicyConfiguration.CreateRetrievalV2(
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

    private static VectorChunkWrite Chunk(
        long ordinal,
        DocumentBinding binding,
        string identity,
        ReadOnlyMemory<float> vector) =>
        new(
            ordinal,
            binding.DocumentId,
            binding.DocumentVersion,
            new LogicalArtifactDigest(Hash(identity)),
            $"synthetic passage {identity}",
            vector,
            DocumentContentLanguage.EnGb);

    private static IndexCompatibilityKey CompatibilityKey(
        EmbeddingProviderDescriptor descriptor,
        string vectorStoreDescriptor = SqliteVectorIndexStore.CompatibilityDescriptor) =>
        new IndexCompatibilityProfile(
            ["synthetic-parser/1"],
            new ChunkingPolicy(128, 16, 160),
            descriptor,
            vectorStoreDescriptor).Key;

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

    private static long Bits(double value) => BitConverter.DoubleToInt64Bits(value);
}
