// Purpose: Proves deterministic fake embedding, inactive staging, validated generation commit, atomic activation, hard filtering and exact idempotent replay.
using RagChallenge.Application.Administration;
using RagChallenge.Application.Documents;
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;
using RagChallenge.Infrastructure.Persistence;

namespace RagChallenge.IntegrationTests;

public sealed class BackendIndexingWorkflowTests
{
    [Fact]
    public async Task CandidateBuildActivatesOnlyAfterValidationAndReplaysIdempotently()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (_, binding) = await fixture.CommitLocalCatalogueAsync();
        var descriptor = new EmbeddingProviderDescriptor(
            "fake",
            "deterministic-v1",
            "fixture-1",
            dimensions: 3);
        var chunks = new[]
        {
            new DocumentChunk(
                0,
                new LogicalArtifactDigest(SqlitePersistenceFixture.Hash("backend-index:first")),
                "first indexed passage",
                pageNumber: 1,
                recordNumber: null,
                new Dictionary<string, string>()),
            new DocumentChunk(
                1,
                new LogicalArtifactDigest(SqlitePersistenceFixture.Hash("backend-index:second")),
                "second indexed passage",
                pageNumber: 2,
                recordNumber: null,
                new Dictionary<string, string>()),
        };
        var bindings = new[] { binding };
        var specification = new IndexGenerationSpecification(
            1,
            SqlitePersistenceFixture.CorpusId,
            new CorpusRevision(1),
            new CatalogueRevision(1),
            BindingDigestCanonicalizer.CanonicaliseActiveDocumentSet(bindings).Digest,
            BindingDigestCanonicalizer.CanonicaliseSourceBindingSet(bindings).Digest,
            SqlitePersistenceFixture.CompatibilityKey);
        var request = new CorpusIndexingRequest(
            new CandidateBuildId("candidate-backend-indexing"),
            specification,
            [new IndexDocumentInput(binding, chunks)],
            descriptor,
            Audit("generation-backend-indexing", "index-generation", 2),
            SqlitePersistenceFixture.At(3));
        var service = new CorpusIndexingService(
            new DeterministicEmbeddingProvider(descriptor),
            fixture.VectorStore,
            fixture.ControlStore);

        var built = await service.BuildAsync(request);

        Assert.Equal(StoreMutationOutcome.Applied, built.CommitResult.Outcome);
        Assert.Equal(2, built.Manifest.ChunkCount);
        Assert.Null(await fixture.ControlStore.ReadActiveActivationAsync(
            SqlitePersistenceFixture.CorpusId));

        var activationService = new GenerationActivationService(fixture.ControlStore);
        var activationRequest = new GenerationActivationRequest(
            built.Manifest,
            bindings,
            ExpectedCurrentRevision: 0,
            SqliteControlPlaneStore.MinimumPreviousGenerationRetention,
            Audit("activation-backend-indexing", "activate-generation", 4));
        var activated = await activationService.ActivateAsync(activationRequest);

        Assert.Equal(StoreMutationOutcome.Applied, activated.Outcome);
        Assert.Equal(built.Manifest.IndexGenerationId, activated.CurrentRecord!.IndexGenerationId);
        var hits = await fixture.VectorStore.SearchExactAsync(
            new VectorSearchRequest(
                activated.CurrentRecord.CorpusId,
                activated.CurrentRecord.IndexGenerationId,
                new float[] { 1, 0, 0 },
                maximumResults: 2,
                activated.CurrentRecord.DocumentBindings,
                [binding.DatabaseProductId]));
        Assert.Equal(2, hits.Count);
        Assert.Equal("first indexed passage", hits[0].ChunkText);

        var deniedByDatabaseFilter = await fixture.VectorStore.SearchExactAsync(
            new VectorSearchRequest(
                activated.CurrentRecord.CorpusId,
                activated.CurrentRecord.IndexGenerationId,
                new float[] { 1, 0, 0 },
                maximumResults: 2,
                activated.CurrentRecord.DocumentBindings,
                [new DatabaseProductId("db-not-authorised")]));
        Assert.Empty(deniedByDatabaseFilter);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            fixture.VectorStore.SearchExactAsync(
                new VectorSearchRequest(
                    new CorpusId("other-corpus"),
                    activated.CurrentRecord.IndexGenerationId,
                    new float[] { 1, 0, 0 },
                    maximumResults: 2,
                    activated.CurrentRecord.DocumentBindings)));

        var replayedBuild = await service.BuildAsync(request);
        var replayedActivation = await activationService.ActivateAsync(activationRequest);
        Assert.Equal(StoreMutationOutcome.AlreadyApplied, replayedBuild.CommitResult.Outcome);
        Assert.Equal(built.Manifest.IndexGenerationId, replayedBuild.Manifest.IndexGenerationId);
        Assert.Equal(StoreMutationOutcome.AlreadyApplied, replayedActivation.Outcome);
        Assert.Equal(1, replayedActivation.CurrentRecord!.RecordRevision.Value);
    }

    private static AdministrativeAuditContext Audit(
        string operationId,
        string command,
        int hour) =>
        new(
            new OperationId(operationId),
            "integration-test",
            command,
            "synthetic indexing workflow verification",
            SqlitePersistenceFixture.At(hour));

    private sealed class DeterministicEmbeddingProvider(
        EmbeddingProviderDescriptor descriptor) : IEmbeddingProvider
    {
        public Task<EmbeddingBatchResult> EmbedAsync(
            EmbeddingBatchRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var vectors = request.Inputs.Select(input =>
                (ReadOnlyMemory<float>)(input.StartsWith("first", StringComparison.Ordinal)
                    ? new float[] { 1, 0, 0 }
                    : new float[] { 0, 1, 0 })).ToArray();
            return Task.FromResult(new EmbeddingBatchResult(descriptor, vectors));
        }
    }
}
