// Purpose: Proves deterministic fake embedding, inactive staging, validated generation commit, atomic activation, hard filtering and exact idempotent replay.
using RagChallenge.Application.Administration;
using RagChallenge.Application.Documents;
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;
using RagChallenge.Infrastructure.Documents;
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
        var compatibilityProfile = CreateCompatibilityProfile(descriptor);
        var specification = new IndexGenerationSpecification(
            1,
            SqlitePersistenceFixture.CorpusId,
            new CorpusRevision(1),
            new CatalogueRevision(1),
            BindingDigestCanonicalizer.CanonicaliseActiveDocumentSet(bindings).Digest,
            BindingDigestCanonicalizer.CanonicaliseSourceBindingSet(bindings).Digest,
            compatibilityProfile.Key);
        var request = new CorpusIndexingRequest(
            new CandidateBuildId("candidate-backend-indexing"),
            specification,
            [new IndexDocumentInput(
                binding,
                DocumentContentLanguage.EnGb,
                chunks,
                PdfPigDocumentParser.CompatibilityDescriptor,
                compatibilityProfile.ChunkingPolicy)],
            descriptor,
            compatibilityProfile,
            Audit("generation-backend-indexing", "index-generation", 2),
            SqlitePersistenceFixture.At(3));
        var service = new CorpusIndexingService(
            new DeterministicEmbeddingProvider(descriptor),
            fixture.VectorStore,
            fixture.ControlStore);
        var incompatibleRequest = request with
        {
            Documents =
            [
                new IndexDocumentInput(
                    binding,
                    DocumentContentLanguage.EnGb,
                    chunks,
                    "unapproved-parser/1",
                    compatibilityProfile.ChunkingPolicy),
            ],
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.BuildAsync(incompatibleRequest));

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
        var querySnapshot = await new SqliteQueryActivationReader(fixture.Options)
            .ReadAsync(SqlitePersistenceFixture.CorpusId, SqlitePersistenceFixture.At(4));
        Assert.NotNull(querySnapshot);
        var resolvedBinding = Assert.Single(querySnapshot.EvidenceBindings);
        Assert.Equal(DocumentContentLanguage.EnGb, resolvedBinding.ContentLanguage);
        Assert.Equal(SourceFreshness.Local, resolvedBinding.Freshness);
        var hits = await fixture.VectorStore.SearchExactAsync(
            new VectorSearchRequest(
                activated.CurrentRecord.CorpusId,
                activated.CurrentRecord.IndexGenerationId,
                new float[] { 1, 0, 0 },
                maximumResults: 2,
                activated.CurrentRecord.DocumentBindings
                    .Select(VectorSearchBindingSelector.FromBinding)
                    .ToArray(),
                [binding.DatabaseProductId]));
        Assert.Equal(2, hits.Count);
        Assert.All(hits, hit =>
        {
            Assert.Equal(activated.CurrentRecord.CorpusId, hit.CorpusId);
            Assert.Equal(activated.CurrentRecord.IndexGenerationId, hit.IndexGenerationId);
            Assert.Equal(
                VectorSearchBindingSelector.FromBinding(binding),
                hit.BindingSelector);
        });
        Assert.Equal("first indexed passage", hits[0].ChunkText);
        Assert.Equal(DocumentContentLanguage.EnGb, hits[0].ContentLanguage);
        Assert.Equal(1, hits[0].PageNumber);

        var deniedByDatabaseFilter = await fixture.VectorStore.SearchExactAsync(
            new VectorSearchRequest(
                activated.CurrentRecord.CorpusId,
                activated.CurrentRecord.IndexGenerationId,
                new float[] { 1, 0, 0 },
                maximumResults: 2,
                activated.CurrentRecord.DocumentBindings
                    .Select(VectorSearchBindingSelector.FromBinding)
                    .ToArray(),
                [new DatabaseProductId("db-not-authorised")]));
        Assert.Empty(deniedByDatabaseFilter);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            fixture.VectorStore.SearchExactAsync(
                new VectorSearchRequest(
                    new CorpusId("other-corpus"),
                    activated.CurrentRecord.IndexGenerationId,
                    new float[] { 1, 0, 0 },
                    maximumResults: 2,
                    activated.CurrentRecord.DocumentBindings
                        .Select(VectorSearchBindingSelector.FromBinding)
                        .ToArray())));

        var replayedBuild = await service.BuildAsync(request);
        var replayedActivation = await activationService.ActivateAsync(activationRequest);
        Assert.Equal(StoreMutationOutcome.AlreadyApplied, replayedBuild.CommitResult.Outcome);
        Assert.Equal(built.Manifest.IndexGenerationId, replayedBuild.Manifest.IndexGenerationId);
        Assert.Equal(StoreMutationOutcome.AlreadyApplied, replayedActivation.Outcome);
        Assert.Equal(1, replayedActivation.CurrentRecord!.RecordRevision.Value);

        var originalRecord = activated.CurrentRecord!;
        var originalAuditDigest = activationRequest.AuditContext.CreateDigest(
            built.Manifest.IndexGenerationId.Value,
            originalRecord.ActivationBindingSetDigest.Value,
            ActivationMutationKind.Initial.ToString());
        var laterExactReplay = await fixture.ControlStore.CompareExchangeActivationAsync(
            new ActivationCompareExchangeRequest(
                activationRequest.AuditContext.OperationId,
                ActivationMutationKind.Initial,
                ExpectedCurrentRevision: 0,
                originalRecord,
                built.Manifest.IndexCompatibilityKey,
                SqlitePersistenceFixture.At(10),
                SqliteControlPlaneStore.MinimumPreviousGenerationRetention,
                originalAuditDigest));
        Assert.Equal(StoreMutationOutcome.AlreadyApplied, laterExactReplay.Outcome);

        var divergentBinding = new DocumentBinding(
            binding.DatabaseProductId,
            binding.DatabaseProductRevision,
            binding.DocumentId,
            binding.DocumentVersion,
            binding.DocumentFormat,
            new SourceAdapterId("different-adapter"),
            binding.SourceTrustClass);
        var divergentDigest = BindingDigestCanonicalizer
            .CanonicaliseActivationBindingSet([divergentBinding])
            .Digest;
        var divergentRecord = new CorpusActivationRecord(
            originalRecord.CorpusId,
            originalRecord.RecordRevision,
            originalRecord.PreviousRecordRevision,
            originalRecord.IndexGenerationId,
            originalRecord.CatalogueRevision,
            divergentDigest,
            [divergentBinding],
            originalRecord.GenerationActivatedAt,
            originalRecord.RecordUpdatedAt);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fixture.ControlStore.CompareExchangeActivationAsync(
                new ActivationCompareExchangeRequest(
                    activationRequest.AuditContext.OperationId,
                    ActivationMutationKind.Initial,
                    ExpectedCurrentRevision: 0,
                    divergentRecord,
                    built.Manifest.IndexCompatibilityKey,
                    SqlitePersistenceFixture.At(11),
                    SqliteControlPlaneStore.MinimumPreviousGenerationRetention,
                    originalAuditDigest)));
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

    private static IndexCompatibilityProfile CreateCompatibilityProfile(
        EmbeddingProviderDescriptor embeddingDescriptor) =>
        new(
            [
                PdfPigDocumentParser.CompatibilityDescriptor,
                CsvHelperDocumentParser.CompatibilityDescriptor,
            ],
            new ChunkingPolicy(),
            embeddingDescriptor,
            "sqlite-exact-vector-store/1;schema=1;distance=cosine;algorithm=exact-scan;vector=float32");

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
