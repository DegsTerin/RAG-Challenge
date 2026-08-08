// Purpose: Composes the local synthetic STATE-06 runtime over durable stores; it is available only in the explicit Integration environment and never enables external access.
using RagChallenge.Application.Administration;
using RagChallenge.Application.Documents;
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;
using RagChallenge.Infrastructure.Documents;
using RagChallenge.Infrastructure.Persistence;
using RagChallenge.Server.Api.Contracts.V1;

namespace RagChallenge.Server.Api.OperationsGovernance;

internal sealed record IntegrationRuntimeOptions(SqliteStoreOptions Stores)
{
    internal const string EnvironmentName = "Integration";
    internal const string EnabledKey = "RagChallenge:Integration:Enabled";
    internal const string StoreRootKey = "RagChallenge:Integration:StoreRoot";

    internal static IntegrationRuntimeOptions? Resolve(
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var enabled = configuration.GetValue<bool>(EnabledKey);

        if (!environment.IsEnvironment(EnvironmentName))
        {
            if (enabled)
            {
                throw new InvalidOperationException(
                    "The synthetic integration runtime requires the Integration environment.");
            }

            return null;
        }

        if (!enabled)
        {
            return null;
        }

        var configuredRoot = configuration[StoreRootKey];

        if (string.IsNullOrWhiteSpace(configuredRoot) ||
            !Path.IsPathFullyQualified(configuredRoot))
        {
            throw new InvalidOperationException(
                "The integration store root must be an explicit absolute path.");
        }

        var storeRoot = Path.GetFullPath(configuredRoot);
        Directory.CreateDirectory(storeRoot);
        return new IntegrationRuntimeOptions(new SqliteStoreOptions(
            Path.Combine(storeRoot, "control.db"),
            Path.Combine(storeRoot, "vectors.db"),
            Path.Combine(storeRoot, "content")));
    }
}

internal sealed class SyntheticIntegrationRuntime :
    IQuestionAnsweringService,
    IQueryReadinessProbe,
    IDisposable
{
    internal const string ConfigurationRevision = "s06-integration-v1";
    internal static readonly CorpusId CorpusId = new("database-systems-catalogue-mvp");

    private static readonly DateTimeOffset BaselineInstant =
        new(2026, 8, 5, 12, 0, 0, TimeSpan.Zero);
    private static readonly byte[] SyntheticCsv = System.Text.Encoding.UTF8.GetBytes(
        "capability,description\n" +
        "persistence,\"Raw content catalogue activation and index survive restart.\"\n");
    private static readonly EmbeddingProviderDescriptor EmbeddingDescriptor =
        new("synthetic", "deterministic-v1", "s06-a", dimensions: 3);
    private static readonly LanguageModelDescriptor LanguageModelDescriptor =
        new("synthetic", "grounded-v1", "s06-a");

    private readonly SqliteStoreOptions stores;
    private readonly IEmbeddingProvider queryEmbeddingProvider;
    private readonly ILanguageModel queryLanguageModel;
    private readonly SemaphoreSlim initialisationGate = new(1, 1);
    private QuestionAnsweringService? answeringService;

    internal SyntheticIntegrationRuntime(IntegrationRuntimeOptions options)
        : this(
            options,
            new DeterministicEmbeddingProvider(),
            new DeterministicLanguageModel())
    {
    }

    internal SyntheticIntegrationRuntime(
        IntegrationRuntimeOptions options,
        IEmbeddingProvider queryEmbeddingProvider,
        ILanguageModel queryLanguageModel)
    {
        ArgumentNullException.ThrowIfNull(options);
        stores = options.Stores;
        this.queryEmbeddingProvider = queryEmbeddingProvider ??
            throw new ArgumentNullException(nameof(queryEmbeddingProvider));
        this.queryLanguageModel = queryLanguageModel ??
            throw new ArgumentNullException(nameof(queryLanguageModel));
    }

    public async Task<QueryExecutionResult> AskAsync(
        QueryRequest request,
        DateTimeOffset observedAt,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureInitialisedAsync(cancellationToken).ConfigureAwait(false);
            return await answeringService!.AskAsync(
                request,
                observedAt,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return QueryExecutionResult.Failed(new QueryFailure(
                QueryFailureKind.OperationCancelled,
                request.CorrelationId));
        }
        catch (Exception exception) when (IsLocalRuntimeFailure(exception))
        {
            return QueryExecutionResult.Failed(new QueryFailure(
                QueryFailureKind.IndexUnavailable,
                request.CorrelationId));
        }
    }

    public async ValueTask<ReadinessV1> CheckAsync(
        DateTimeOffset observedAt,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureInitialisedAsync(cancellationToken).ConfigureAwait(false);
            var snapshot = await new SqliteQueryActivationReader(stores).ReadAsync(
                CorpusId,
                observedAt,
                cancellationToken).ConfigureAwait(false) ??
                throw new InvalidDataException(
                    "The integration activation record is unavailable.");
            var eligible = snapshot.EvidenceBindings.Count(binding => binding.IsEligible);
            var activeDatabases = snapshot.EvidenceBindings
                .Select(binding => binding.Binding.DatabaseProductId)
                .Distinct()
                .Count();
            return new ReadinessV1(
                "Ready",
                activeDatabases,
                eligible,
                DegradedDocumentCount: snapshot.EvidenceBindings.Count - eligible,
                SourceStates: Array.Empty<SanitisedSourceStateV1>(),
                snapshot.ActivationRecord.IndexGenerationId.Value,
                ConfigurationRevision,
                [
                    new SanitisedCapabilityCheckV1("control-store", "Ready"),
                    new SanitisedCapabilityCheckV1("content-store", "Ready"),
                    new SanitisedCapabilityCheckV1("vector-store", "Ready"),
                    new SanitisedCapabilityCheckV1("synthetic-providers", "Ready"),
                ],
                observedAt);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (IsLocalRuntimeFailure(exception))
        {
            return new ReadinessV1(
                "Unready",
                ActiveDatabaseCount: 0,
                EligibleDocumentCount: 0,
                DegradedDocumentCount: 0,
                SourceStates: Array.Empty<SanitisedSourceStateV1>(),
                ActiveGenerationId: null,
                ConfigurationRevision,
                [new SanitisedCapabilityCheckV1("integration-runtime", "Unavailable")],
                observedAt);
        }
    }

    internal async Task EnsureInitialisedAsync(
        CancellationToken cancellationToken = default)
    {
        if (answeringService is not null)
        {
            return;
        }

        await initialisationGate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (answeringService is not null)
            {
                return;
            }

            await SqliteStoreProvisioner.ApplyMigrationsAsync(stores, cancellationToken)
                .ConfigureAwait(false);
            var controlStore = new SqliteControlPlaneStore(stores);
            var vectorStore = new SqliteVectorIndexStore(stores);
            IDocumentContentStore contentStore = new ImmutableContentStore(stores);
            var activation = await controlStore.ReadActiveActivationAsync(
                CorpusId,
                cancellationToken).ConfigureAwait(false);

            if (activation is null)
            {
                await BootstrapAsync(
                    controlStore,
                    vectorStore,
                    contentStore,
                    cancellationToken).ConfigureAwait(false);
            }

            await VerifyPersistedStateAsync(
                controlStore,
                vectorStore,
                contentStore,
                cancellationToken).ConfigureAwait(false);
            answeringService = new QuestionAnsweringService(
                CorpusId,
                 EmbeddingDescriptor,
                 LanguageModelDescriptor,
                 new SqliteQueryActivationReader(stores),
                 queryEmbeddingProvider,
                 vectorStore,
                 queryLanguageModel,
                 minimumScore: 0.25);
        }
        finally
        {
            initialisationGate.Release();
        }
    }

    public void Dispose() => initialisationGate.Dispose();

    private static async Task BootstrapAsync(
        SqliteControlPlaneStore controlStore,
        SqliteVectorIndexStore vectorStore,
        IDocumentContentStore contentStore,
        CancellationToken cancellationToken)
    {
        var productId = new DatabaseProductId("db-s06-synthetic");
        var productRevision = new DatabaseProductRevision(1);
        var documentId = new DocumentId("doc-s06-synthetic-csv");
        var documentVersion = new DocumentVersionNumber(1);
        var adapterId = new SourceAdapterId("local-synthetic-csv");
        var context = new DocumentChunkingContext(
            CorpusId,
            productId,
            productRevision,
            documentId,
            documentVersion,
            DocumentFormat.Csv,
            DocumentContentLanguage.EnGb,
            adapterId,
            SourceTrustClass.LocalAuthorised);
        var ingestion = new DocumentIngestionService(
            contentStore,
            [new PdfPigDocumentParser(), new CsvHelperDocumentParser()],
            new DeterministicChunkingStrategy());
        await using var source = new MemoryStream(SyntheticCsv, writable: false);
        var ingested = await ingestion.IngestAsync(new DocumentIngestionRequest(
            source,
            MaximumByteLength: 131_072,
            ContentMediaType.TextCsv,
            new ParserPolicy(131_072, 32, 131_072, 32, 16_384),
            new ChunkingPolicy(128, 16, 160),
            context), cancellationToken).ConfigureAwait(false);
        var category = new DatabaseCategory(
            new DatabaseCategoryId("category-s06-synthetic"),
            "Synthetic integration fixture");
        var product = new DatabaseProduct(
            productId,
            productRevision,
            "Synthetic restart database",
            CatalogueItemStatus.Active,
            [category.Id]);
        var document = new DocumentVersion(
            documentId,
            documentVersion,
            productId,
            productRevision,
            DocumentFormat.Csv,
            DocumentContentLanguage.EnGb,
            CatalogueItemStatus.Active,
            ingested.Content.ContentObjectId,
            ingested.Content.ByteLength,
            ingested.Content.MediaType.Value,
            adapterId,
            SourceTrustClass.LocalAuthorised);
        var catalogue = new CatalogueSnapshot(
            CorpusId,
            new CatalogueRevision(1),
            [category],
            [product],
            [document]);
        EnsureApplied(await controlStore.CommitCatalogueAsync(
            new CatalogueCommitRequest(
                new OperationId("s06-catalogue-v1"),
                catalogue,
                ExpectedCurrentRevision: 0,
                BaselineInstant),
            cancellationToken).ConfigureAwait(false), "synthetic catalogue");

        var binding = new DocumentBinding(
            productId,
            productRevision,
            documentId,
            documentVersion,
            DocumentFormat.Csv,
            adapterId,
            SourceTrustClass.LocalAuthorised);
        var bindings = new[] { binding };
        var evidenceBindings = new[]
        {
            new DocumentActivationEvidenceBinding(
                binding,
                ingested.Content.ContentObjectId,
                CreatePermittedRights(documentId, documentVersion),
                renderManifestId: null),
        };
        var profile = CreateCompatibilityProfile();
        var specification = new IndexGenerationSpecification(
            manifestSchemaVersion: 1,
            CorpusId,
            new CorpusRevision(1),
            new CatalogueRevision(1),
            BindingDigestCanonicalizer.CanonicaliseActiveDocumentSet(bindings).Digest,
            BindingDigestCanonicalizer.CanonicaliseSourceBindingSet(bindings).Digest,
            profile.Key);
        var indexing = new CorpusIndexingService(
            new DeterministicEmbeddingProvider(),
            vectorStore,
            controlStore);
        var manifest = await indexing.BuildAsync(new CorpusIndexingRequest(
            new CandidateBuildId("candidate-s06-integration-v1"),
            specification,
            [new IndexDocumentInput(
                binding,
                DocumentContentLanguage.EnGb,
                ingested.Chunks,
                ingested.ParsedArtifact.ParserDescriptor,
                profile.ChunkingPolicy)],
            EmbeddingDescriptor,
            profile,
            Audit("s06-generation-v1", "build-index", BaselineInstant.AddMinutes(1)),
            BaselineInstant.AddMinutes(1)), cancellationToken).ConfigureAwait(false);
        var activation = await new GenerationActivationService(controlStore).ActivateAsync(
            new GenerationActivationRequest(
                manifest.Manifest,
                evidenceBindings,
                ExpectedCurrentRevision: 0,
                SqliteControlPlaneStore.MinimumPreviousGenerationRetention,
                Audit(
                    "s06-activation-v1",
                    "activate-generation",
                    BaselineInstant.AddMinutes(2))),
            cancellationToken).ConfigureAwait(false);

        if (activation.Outcome is not StoreMutationOutcome.Applied and
            not StoreMutationOutcome.AlreadyApplied)
        {
            throw new InvalidDataException(
                "The synthetic integration generation could not be activated.");
        }
    }

    private static DocumentRightsEligibilityRecordV1 CreatePermittedRights(
        DocumentId documentId,
        DocumentVersionNumber documentVersion) =>
        new(
            documentId,
            documentVersion,
            Enum.GetValues<DocumentRight>().Select(right => new DocumentRightDecision(
                right,
                DocumentRightDecisionState.Permitted,
                new DocumentRightsEvidenceReference($"synthetic-rights-{right}"))));

    private static async Task VerifyPersistedStateAsync(
        SqliteControlPlaneStore controlStore,
        SqliteVectorIndexStore vectorStore,
        IDocumentContentStore contentStore,
        CancellationToken cancellationToken)
    {
        var catalogue = await controlStore.ReadCurrentCatalogueAsync(
            CorpusId,
            cancellationToken).ConfigureAwait(false) ??
            throw new InvalidDataException("The persisted catalogue is unavailable.");
        var activation = await controlStore.ReadActiveActivationAsync(
            CorpusId,
            cancellationToken).ConfigureAwait(false) ??
            throw new InvalidDataException("The persisted activation is unavailable.");

        foreach (var binding in activation.DocumentBindings)
        {
            var document = catalogue.DocumentVersions.Single(item =>
                item.Id == binding.DocumentId && item.Version == binding.DocumentVersion);
            await using var content = await contentStore.OpenVerifiedAsync(
                document.ContentObjectId,
                new ExpectedHashAndLength(
                    document.ContentObjectId,
                    document.ByteLength),
                cancellationToken).ConfigureAwait(false);
        }

        var hits = await vectorStore.SearchExactAsync(new VectorSearchRequest(
            activation.CorpusId,
            activation.IndexGenerationId,
            new float[] { 1, 0, 0 },
            maximumResults: 1,
            activation.DocumentBindings
                .Select(VectorSearchBindingSelector.FromBinding)
                .ToArray()), cancellationToken).ConfigureAwait(false);

        if (hits.Count != 1)
        {
            throw new InvalidDataException(
                "The persisted integration index cannot serve its sentinel query.");
        }
    }

    private static AdministrativeAuditContext Audit(
        string operationId,
        string command,
        DateTimeOffset requestedAt) =>
        new(
            new OperationId(operationId),
            "state-06-integration",
            command,
            "Bootstrap the authorised synthetic integration fixture.",
            requestedAt);

    private static IndexCompatibilityProfile CreateCompatibilityProfile() =>
        new(
            [
                PdfPigDocumentParser.CompatibilityDescriptor,
                CsvHelperDocumentParser.CompatibilityDescriptor,
            ],
            new ChunkingPolicy(128, 16, 160),
            EmbeddingDescriptor,
            "sqlite-exact-vector-store/1;schema=1;distance=cosine;algorithm=exact-scan;vector=float32");

    private static void EnsureApplied(StoreMutationResult result, string operation)
    {
        if (result.Outcome is not StoreMutationOutcome.Applied and
            not StoreMutationOutcome.AlreadyApplied)
        {
            throw new InvalidDataException($"The {operation} could not be persisted.");
        }
    }

    private static bool IsLocalRuntimeFailure(Exception exception) =>
        exception is ArgumentException or InvalidOperationException or
            InvalidDataException or IOException or UnauthorizedAccessException or
            Microsoft.Data.Sqlite.SqliteException;

    private sealed class DeterministicEmbeddingProvider : IEmbeddingProvider
    {
        public Task<EmbeddingBatchResult> EmbedAsync(
            EmbeddingBatchRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var vectors = request.Inputs
                .Select(_ => (ReadOnlyMemory<float>)new float[] { 1, 0, 0 })
                .ToArray();
            return Task.FromResult(new EmbeddingBatchResult(
                EmbeddingDescriptor,
                vectors));
        }
    }

    private sealed class DeterministicLanguageModel : ILanguageModel
    {
        public Task<GroundedGenerationResult> GenerateAsync(
            GroundedGenerationRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var evidence = request.Evidence.First();
            var answer = request.QuestionLanguage == SupportedQueryLanguage.PtBr
                ? "Resposta sintética fundamentada na evidência persistida."
                : "Synthetic answer grounded in persisted evidence.";
            return Task.FromResult(new GroundedGenerationResult(
                LanguageModelDescriptor,
                request.QuestionLanguage,
                answer,
                [evidence.ChunkId]));
        }
    }
}
