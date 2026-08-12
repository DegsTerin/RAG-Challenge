// Purpose: Verifies the retrieval-v2 typed boundary, total ordering, fixed selection policy and fail-closed outcomes without providers, persistence or network access.
using System.Security.Cryptography;
using System.Text;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.UnitTests;

public sealed class RetrievalPolicyExecutorTests
{
    [Fact]
    public async Task ValidHitsPreserveCompositeOrderAndExposeBoundIdentities()
    {
        var context = CreateContext((manifest, bindings) => VectorSearchResult.Successful(
        [
            Hit(manifest, bindings[0], ordinal: 1, score: 0.9, "first"),
            Hit(manifest, bindings[1], ordinal: 4, score: 0.9, "second"),
            Hit(manifest, bindings[0], ordinal: 5, score: 0.25, "threshold"),
            Hit(manifest, bindings[1], ordinal: 6, score: 0.24, "below"),
        ]));

        var result = await context.Executor.ExecuteAsync(context.Request);

        Assert.Equal(RetrievalPolicyOutcome.Succeeded, result.Outcome);
        Assert.Equal([1L, 4L, 5L, 6L], result.RankedHits.Select(hit => hit.ChunkOrdinal));
        Assert.Equal([1L, 4L, 5L], result.SelectedEvidence.Select(
            item => item.Hit.ChunkOrdinal));
        Assert.Equal([0.9, 0.9, 0.25, 0.24], result.RankedHits.Select(hit => hit.Score));
        Assert.Null(result.FailureIdentity);
        Assert.NotNull(result.Identity);
        var manifest = Assert.IsType<FinalisedIndexGenerationManifest>(
            context.Request.ActivationSnapshot.FinalisedGenerationManifest);
        Assert.Equal(manifest.IndexGenerationId, result.Identity.IndexGenerationId);
        Assert.Equal(manifest.IndexCompatibilityKey, result.Identity.IndexCompatibilityKey);
        Assert.Equal(manifest.GenerationContentDigest, result.Identity.GenerationManifestDigest);
        Assert.Equal(
            RetrievalPolicyConfiguration.RetrievalV2,
            result.Identity.RetrievalPolicyVersion);
        Assert.Equal(
            RetrievalPolicyConfiguration.MinimumScoreV1,
            result.Identity.MinimumScorePolicyVersion);
        Assert.Equal(
            RetrievalPolicyConfiguration.QueryVectorRepresentation,
            result.Identity.QueryVectorRepresentation);
        Assert.Equal(
            "480376c6bf738a0227f2bbf2b3506b7cde209152c0ba9a9077e5527169eb292e",
            result.Identity.QueryVectorSha256);
        Assert.Equal(
            "8734c8a527b17c53f69922d40c4fdf8df7c01e53f105a1182898f6fe194a4d0c",
            result.Identity.PolicyManifestSha256);
    }

    [Fact]
    public async Task SelectionAppliesScalarBudgetSixPassageLimitAndInclusiveThreshold()
    {
        var context = CreateContext((manifest, bindings) => VectorSearchResult.Successful(
        [
            Hit(manifest, bindings[0], 0, 0.99, new string('x', 16001)),
            Hit(manifest, bindings[0], 1, 0.90, "one"),
            Hit(manifest, bindings[0], 2, 0.80, "two"),
            Hit(manifest, bindings[0], 3, 0.70, "three"),
            Hit(manifest, bindings[1], 4, 0.60, "four"),
            Hit(manifest, bindings[1], 5, 0.50, "five"),
            Hit(manifest, bindings[1], 6, 0.25, "six"),
        ]));

        var result = await context.Executor.ExecuteAsync(context.Request);

        Assert.Equal(RetrievalPolicyOutcome.Succeeded, result.Outcome);
        Assert.Equal(7, result.RankedHits.Count);
        Assert.Equal([1L, 2L, 3L, 4L, 5L, 6L], result.SelectedEvidence.Select(
            item => item.Hit.ChunkOrdinal));
        Assert.Equal(0.25, result.SelectedEvidence[^1].Hit.Score);
    }

    [Theory]
    [MemberData(nameof(InvalidScores))]
    public async Task InvalidScoreReturnsInvalidIndexData(double score)
    {
        var context = CreateContext((manifest, bindings) => VectorSearchResult.Successful(
        [
            Hit(manifest, bindings[0], ordinal: 0, score, "invalid-score"),
        ]));

        var result = await context.Executor.ExecuteAsync(context.Request);

        Assert.Equal(RetrievalPolicyOutcome.InvalidIndexData, result.Outcome);
        Assert.Equal("RETRIEVAL_INVALID_INDEX_DATA", result.FailureIdentity);
        Assert.Empty(result.RankedHits);
        Assert.Empty(result.SelectedEvidence);
    }

    [Fact]
    public async Task DuplicateGlobalOrdinalAcrossDocumentsReturnsInvalidIndexData()
    {
        var context = CreateContext((manifest, bindings) => VectorSearchResult.Successful(
        [
            Hit(manifest, bindings[0], ordinal: 2, score: 0.9, "first-document"),
            Hit(manifest, bindings[1], ordinal: 2, score: 0.8, "second-document"),
        ]));

        var result = await context.Executor.ExecuteAsync(context.Request);

        Assert.Equal(RetrievalPolicyOutcome.InvalidIndexData, result.Outcome);
        Assert.Empty(result.SelectedEvidence);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CompositeOrderViolationReturnsContractViolation(bool equalScores)
    {
        var context = CreateContext((manifest, bindings) => VectorSearchResult.Successful(
            equalScores
                ?
                [
                    Hit(manifest, bindings[0], ordinal: 3, score: 0.9, "larger-ordinal"),
                    Hit(manifest, bindings[1], ordinal: 1, score: 0.9, "smaller-ordinal"),
                ]
                :
                [
                    Hit(manifest, bindings[0], ordinal: 1, score: 0.8, "lower-score"),
                    Hit(manifest, bindings[1], ordinal: 2, score: 0.9, "higher-score"),
                ]));

        var result = await context.Executor.ExecuteAsync(context.Request);

        Assert.Equal(RetrievalPolicyOutcome.ContractViolation, result.Outcome);
        Assert.Equal("RETRIEVAL_CONTRACT_VIOLATION", result.FailureIdentity);
    }

    [Theory]
    [MemberData(nameof(InvalidQueryVectors))]
    public async Task InvalidQueryVectorFailsBeforeCallingStore(float[] vector)
    {
        var context = CreateContext();
        var request = CopyRequestWithVector(context.Request, vector);

        var result = await context.Executor.ExecuteAsync(request);

        Assert.Equal(RetrievalPolicyOutcome.InvalidQueryVector, result.Outcome);
        Assert.Equal(0, context.VectorStore.CallCount);
    }

    [Fact]
    public async Task CompatibilityMismatchReturnsGenerationUnavailableBeforeStore()
    {
        var context = CreateContext();
        var mismatched = context.Configuration with
        {
            ExpectedIndexCompatibilityKey = new IndexCompatibilityKey(new string('f', 64)),
        };
        var executor = new RetrievalV2PolicyExecutor(context.VectorStore, mismatched);
        var request = CopyRequestWithPolicy(context.Request, mismatched);

        var result = await executor.ExecuteAsync(request);

        Assert.Equal(RetrievalPolicyOutcome.GenerationUnavailable, result.Outcome);
        Assert.Equal(0, context.VectorStore.CallCount);
    }

    [Fact]
    public async Task InvalidConfigurationReturnsTypedFailureBeforeStore()
    {
        var context = CreateContext();
        var invalid = context.Configuration with { MaximumResults = 7 };
        var executor = new RetrievalV2PolicyExecutor(context.VectorStore, invalid);

        var result = await executor.ExecuteAsync(context.Request);

        Assert.Equal(RetrievalPolicyOutcome.InvalidConfiguration, result.Outcome);
        Assert.Equal("RETRIEVAL_INVALID_CONFIGURATION", result.FailureIdentity);
        Assert.Equal(0, context.VectorStore.CallCount);
    }

    [Theory]
    [InlineData("retrieval-version")]
    [InlineData("minimum-score-version")]
    [InlineData("maximum-results")]
    [InlineData("minimum-score")]
    [InlineData("maximum-evidence")]
    [InlineData("scalar-budget")]
    [InlineData("embedding-descriptor")]
    [InlineData("compatibility-key")]
    public async Task RequestPolicyMismatchReturnsInvalidConfigurationBeforeStore(
        string scenario)
    {
        var context = CreateContext();
        var requested = scenario switch
        {
            "retrieval-version" => context.Configuration with
            {
                RetrievalPolicyVersion = "retrieval-v3",
            },
            "minimum-score-version" => context.Configuration with
            {
                MinimumScorePolicyVersion = "minimum-score-v2",
            },
            "maximum-results" => context.Configuration with { MaximumResults = 7 },
            "minimum-score" => context.Configuration with { MinimumScore = 0.3 },
            "maximum-evidence" => context.Configuration with
            {
                MaximumSelectedEvidence = 5,
            },
            "scalar-budget" => context.Configuration with
            {
                MaximumSelectedEvidenceScalars = 15999,
            },
            "embedding-descriptor" => context.Configuration with
            {
                ExpectedEmbeddingDescriptor = new EmbeddingProviderDescriptor(
                    "fake",
                    "embedding-v1",
                    "other-revision",
                    dimensions: 3),
            },
            "compatibility-key" => context.Configuration with
            {
                ExpectedIndexCompatibilityKey = new IndexCompatibilityKey(new string('e', 64)),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };

        var result = await context.Executor.ExecuteAsync(
            CopyRequestWithPolicy(context.Request, requested));

        Assert.Equal(RetrievalPolicyOutcome.InvalidConfiguration, result.Outcome);
        Assert.Equal(0, context.VectorStore.CallCount);
    }

    [Fact]
    public async Task MoreThanTopKHitsReturnsContractViolation()
    {
        var context = CreateContext((manifest, bindings) => VectorSearchResult.Successful(
            Enumerable.Range(0, 9).Select(ordinal => Hit(
                manifest,
                bindings[ordinal % bindings.Length],
                ordinal,
                0.9 - (ordinal * 0.01),
                $"hit-{ordinal}"))));

        var result = await context.Executor.ExecuteAsync(context.Request);

        Assert.Equal(RetrievalPolicyOutcome.ContractViolation, result.Outcome);
    }

    [Fact]
    public async Task MixedCandidateIdentityReturnsInvalidIndexData()
    {
        var context = CreateContext((manifest, bindings) => VectorSearchResult.Successful(
        [
            Hit(manifest, bindings[0], 0, 0.9, "first"),
            Hit(
                manifest,
                bindings[1],
                1,
                0.8,
                "second",
                new CandidateBuildId("candidate-other")),
        ]));

        var result = await context.Executor.ExecuteAsync(context.Request);

        Assert.Equal(RetrievalPolicyOutcome.InvalidIndexData, result.Outcome);
    }

    [Fact]
    public async Task SelectedEvidenceUsesAuthoritativeSnapshotMetadata()
    {
        var context = CreateContext((manifest, bindings) => VectorSearchResult.Successful(
        [
            Hit(manifest, bindings[0], 0, 0.9, "selected"),
        ]));
        var bindings = context.Request.EligibleBindings.ToArray();
        var replacement = new QueryEvidenceBinding(
            bindings[0].Binding,
            bindings[0].EvidenceBinding,
            bindings[0].RenderManifest,
            bindings[0].ContentLanguage,
            bindings[0].Freshness,
            title: "caller-supplied replacement");
        bindings[0] = replacement;

        var result = await context.Executor.ExecuteAsync(
            CopyRequestWithBindings(context.Request, bindings));

        Assert.Equal(RetrievalPolicyOutcome.Succeeded, result.Outcome);
        var selected = Assert.Single(result.SelectedEvidence);
        Assert.NotSame(replacement, selected.Binding);
        Assert.Null(selected.Binding.Title);
    }

    [Theory]
    [InlineData("raw", RetrievalNoEvidenceReason.NoRawHits)]
    [InlineData("score", RetrievalNoEvidenceReason.BelowMinimumScore)]
    [InlineData("budget", RetrievalNoEvidenceReason.ScalarBudgetExcludedAll)]
    public async Task EmptySelectionReasonFollowsOrderedPolicyStages(
        string scenario,
        RetrievalNoEvidenceReason expectedReason)
    {
        var context = CreateContext((manifest, bindings) => scenario switch
        {
            "raw" => VectorSearchResult.Successful([]),
            "score" => VectorSearchResult.Successful(
            [
                Hit(manifest, bindings[0], ordinal: 0, score: 0.24, "below"),
            ]),
            "budget" => VectorSearchResult.Successful(
            [
                Hit(manifest, bindings[0], ordinal: 0, score: 0.9, new string('x', 16001)),
            ]),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        });

        var result = await context.Executor.ExecuteAsync(context.Request);

        Assert.Equal(RetrievalPolicyOutcome.NoSelectedEvidenceUnderPolicy, result.Outcome);
        Assert.Equal(expectedReason, result.NoEvidenceReason);
        Assert.Empty(result.SelectedEvidence);
        Assert.Null(result.FailureIdentity);
    }

    [Theory]
    [InlineData(VectorSearchOutcome.InvalidQueryVector, RetrievalPolicyOutcome.InvalidQueryVector)]
    [InlineData(
        VectorSearchOutcome.GenerationUnavailable,
        RetrievalPolicyOutcome.GenerationUnavailable)]
    [InlineData(VectorSearchOutcome.InvalidIndexData, RetrievalPolicyOutcome.InvalidIndexData)]
    [InlineData(VectorSearchOutcome.ContractViolation, RetrievalPolicyOutcome.ContractViolation)]
    [InlineData(VectorSearchOutcome.OperationCancelled, RetrievalPolicyOutcome.OperationCancelled)]
    [InlineData(VectorSearchOutcome.UnexpectedFailure, RetrievalPolicyOutcome.UnexpectedFailure)]
    public async Task TypedStoreFailureMapsWithoutLanguageModelBoundary(
        VectorSearchOutcome storeOutcome,
        RetrievalPolicyOutcome expectedOutcome)
    {
        var context = CreateContext((_, _) => VectorSearchResult.Failed(storeOutcome));

        var result = await context.Executor.ExecuteAsync(context.Request);

        Assert.Equal(expectedOutcome, result.Outcome);
        Assert.Empty(result.SelectedEvidence);
    }

    [Fact]
    public async Task CancellationReturnsTypedOutcomeWithoutCallingStore()
    {
        var context = CreateContext();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var result = await context.Executor.ExecuteAsync(
            context.Request,
            cancellation.Token);

        Assert.Equal(RetrievalPolicyOutcome.OperationCancelled, result.Outcome);
        Assert.Equal(0, context.VectorStore.CallCount);
    }

    public static IEnumerable<object[]> InvalidScores()
    {
        yield return [double.NaN];
        yield return [double.PositiveInfinity];
        yield return [double.NegativeInfinity];
        yield return [1.0000001];
        yield return [-1.0000001];
    }

    public static IEnumerable<object[]> InvalidQueryVectors()
    {
        yield return [Array.Empty<float>()];
        yield return [new float[] { 1, 0 }];
        yield return [new float[] { 0, 0, 0 }];
        yield return [new float[] { float.NaN, 0, 0 }];
        yield return [new float[] { float.PositiveInfinity, 0, 0 }];
        yield return [new float[] { float.MaxValue, float.MaxValue, float.MaxValue }];
    }

    private static TestContext CreateContext(
        Func<FinalisedIndexGenerationManifest, QueryEvidenceBinding[], VectorSearchResult>?
            createSearchResult = null)
    {
        var bindings = new[]
        {
            LocalCsvBinding("database-a", "document-a"),
            LocalCsvBinding("database-b", "document-b"),
        };
        var evidence = bindings.Select(binding => TestModelFactory.Evidence(binding)).ToArray();
        var manifest = CreateManifest(bindings);
        var activation = ActivationRecordFactory.CreateInitial(
            manifest,
            evidence,
            TestModelFactory.Now);
        var queryBindings = bindings.Select((binding, index) => new QueryEvidenceBinding(
            binding,
            evidence[index],
            renderManifest: null,
            DocumentContentLanguage.EnGb,
            SourceFreshness.Local)).ToArray();
        var snapshot = new QueryActivationSnapshot(activation, queryBindings, manifest);
        var descriptor = new EmbeddingProviderDescriptor(
            "fake",
            "embedding-v1",
            "retrieval-policy-tests",
            dimensions: 3);
        var configuration = RetrievalPolicyConfiguration.CreateRetrievalV2(
            descriptor,
            manifest.IndexCompatibilityKey);
        var searchResult = createSearchResult?.Invoke(manifest, queryBindings) ??
            VectorSearchResult.Successful([]);
        var vectorStore = new FakeVectorStore(searchResult);
        var request = new RetrievalPolicyRequest(
            snapshot,
            queryBindings,
            new float[] { 1, 0, 0 },
            descriptor,
            SupportedQueryLanguage.EnGb,
            QueryContractVersion.V1,
            configuration);
        return new TestContext(
            new RetrievalV2PolicyExecutor(vectorStore, configuration),
            request,
            configuration,
            vectorStore);
    }

    private static RetrievalPolicyRequest CopyRequestWithVector(
        RetrievalPolicyRequest source,
        ReadOnlyMemory<float> vector) =>
        new(
            source.ActivationSnapshot,
            source.EligibleBindings,
            vector,
            source.ObservedEmbeddingDescriptor,
            source.QuestionLanguage,
            source.EligibilityPolicyVersion,
            source.ApplicablePolicy,
            source.DatabaseProductFilters,
            source.DocumentFilters);

    private static RetrievalPolicyRequest CopyRequestWithPolicy(
        RetrievalPolicyRequest source,
        RetrievalPolicyConfiguration applicablePolicy) =>
        new(
            source.ActivationSnapshot,
            source.EligibleBindings,
            source.QueryVector,
            source.ObservedEmbeddingDescriptor,
            source.QuestionLanguage,
            source.EligibilityPolicyVersion,
            applicablePolicy,
            source.DatabaseProductFilters,
            source.DocumentFilters);

    private static RetrievalPolicyRequest CopyRequestWithBindings(
        RetrievalPolicyRequest source,
        IReadOnlyCollection<QueryEvidenceBinding> eligibleBindings) =>
        new(
            source.ActivationSnapshot,
            eligibleBindings,
            source.QueryVector,
            source.ObservedEmbeddingDescriptor,
            source.QuestionLanguage,
            source.EligibilityPolicyVersion,
            source.ApplicablePolicy,
            source.DatabaseProductFilters,
            source.DocumentFilters);

    private static DocumentBinding LocalCsvBinding(string productId, string documentId) =>
        new(
            new DatabaseProductId(productId),
            new DatabaseProductRevision(1),
            new DocumentId(documentId),
            new DocumentVersionNumber(1),
            DocumentFormat.Csv,
            new SourceAdapterId("local-csv"),
            SourceTrustClass.LocalAuthorised);

    private static FinalisedIndexGenerationManifest CreateManifest(
        IReadOnlyCollection<DocumentBinding> bindings)
    {
        var contentDigest = Hash("retrieval-policy-generation");
        return new FinalisedIndexGenerationManifest(
            manifestSchemaVersion: 1,
            new CorpusId("retrieval-policy-corpus"),
            new CorpusRevision(1),
            new CatalogueRevision(1),
            BindingDigestCanonicalizer.CanonicaliseActiveDocumentSet(bindings).Digest,
            BindingDigestCanonicalizer.CanonicaliseSourceBindingSet(bindings).Digest,
            new IndexCompatibilityKey(Hash("retrieval-policy-compatibility")),
            new GenerationSpecDigest(Hash("retrieval-policy-specification")),
            chunkCount: 8,
            vectorCount: 8,
            new LogicalArtifactDigest(Hash("retrieval-policy-artifacts")),
            new GenerationContentDigest(contentDigest),
            new IndexGenerationId($"idxgen-{contentDigest}"));
    }

    private static VectorSearchHit Hit(
        FinalisedIndexGenerationManifest manifest,
        QueryEvidenceBinding binding,
        long ordinal,
        double score,
        string text,
        CandidateBuildId? candidateBuildId = null) =>
        new(
            candidateBuildId ?? new CandidateBuildId("candidate-retrieval-policy"),
            manifest.CorpusId,
            manifest.IndexGenerationId,
            VectorSearchBindingSelector.FromBinding(binding.Binding),
            ordinal,
            new LogicalArtifactDigest(Hash($"chunk-{ordinal}-{binding.Binding.DocumentId.Value}")),
            text,
            score,
            binding.ContentLanguage,
            PageNumber: null,
            RecordNumber: null,
            new Dictionary<string, string>());

    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private sealed record TestContext(
        RetrievalV2PolicyExecutor Executor,
        RetrievalPolicyRequest Request,
        RetrievalPolicyConfiguration Configuration,
        FakeVectorStore VectorStore);

    private sealed class FakeVectorStore(VectorSearchResult result) : IVectorIndexStore
    {
        public int CallCount { get; private set; }

        public Task<VectorSearchResult> SearchExactAsync(
            VectorSearchRequest request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(result);
        }

        public Task CreateCandidateAsync(
            CandidateBuildId candidateBuildId,
            CorpusId corpusId,
            IndexCompatibilityKey indexCompatibilityKey,
            int vectorDimensions,
            long expectedChunkCount,
            DateTimeOffset createdAt,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task AddChunksAsync(
            CandidateBuildId candidateBuildId,
            IReadOnlyCollection<VectorChunkWrite> chunks,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<FinalisedIndexGenerationManifest> FinaliseCandidateAsync(
            CandidateBuildId candidateBuildId,
            IndexGenerationSpecification specification,
            DateTimeOffset validatedAt,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task MarkFailedAsync(
            CandidateBuildId candidateBuildId,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
