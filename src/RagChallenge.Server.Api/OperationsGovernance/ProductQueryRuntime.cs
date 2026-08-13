// Purpose: Reopens an explicitly configured product store and composes real OpenAI query providers without bootstrapping synthetic data or mutating catalogue and activation state.
using System.Security.Cryptography;
using System.Text;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;
using RagChallenge.Infrastructure.Persistence;
using RagChallenge.Infrastructure.Providers;
using RagChallenge.Server.Api.Contracts.V1;

namespace RagChallenge.Server.Api.OperationsGovernance;

internal sealed record ProductQueryRuntimeOptions(
    SqliteStoreOptions Stores,
    string CredentialEnvironmentVariable,
    bool ApplyMigrations)
{
    internal const string EnabledKey = "RagChallenge:Product:Enabled";
    internal const string ApplyMigrationsKey = "RagChallenge:Product:ApplyMigrations";
    internal const string StoreRootKey = "RagChallenge:Product:StoreRoot";
    internal const string CredentialKey =
        "RagChallenge:Product:CredentialEnvironmentVariable";

    internal static ProductQueryRuntimeOptions? Resolve(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        if (!configuration.GetValue<bool>(EnabledKey))
        {
            return null;
        }

        var configuredRoot = configuration[StoreRootKey];
        if (string.IsNullOrWhiteSpace(configuredRoot) ||
            !Path.IsPathFullyQualified(configuredRoot))
        {
            throw new InvalidOperationException(
                "The product store root must be an explicit absolute path.");
        }

        var storeRoot = Path.GetFullPath(configuredRoot);
        if (!Directory.Exists(storeRoot))
        {
            throw new InvalidOperationException("The product store root is unavailable.");
        }

        var credential = OpaqueEnvironmentCredentialReference.Parse(
            configuration[CredentialKey] ?? string.Empty);
        if (string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable(credential.EnvironmentVariableName)))
        {
            throw new InvalidOperationException("The product provider credential is unavailable.");
        }

        return new ProductQueryRuntimeOptions(
            new SqliteStoreOptions(
                Path.Combine(storeRoot, "control.db"),
                Path.Combine(storeRoot, "vectors.db"),
                Path.Combine(storeRoot, "content")),
            credential.EnvironmentVariableName,
            configuration.GetValue<bool>(ApplyMigrationsKey));
    }
}

internal sealed class ProductQueryRuntime :
    IQuestionAnsweringService,
    IQueryReadinessProbe,
    IVisualEvidenceReader,
    IDisposable
{
    internal static readonly CorpusId CorpusId = new("rag-challenge-product");
    internal const string ConfigurationRevision = "oracle-19c-product-v1";
    private const long ExpectedCatalogueRevision = 53;
    private const string ExpectedCatalogueFingerprint =
        "d6b38c65bbe991eebb2b1f6ae67979512caef1246f1bd1c46e7d2b6e4e281000";
    private const string ExpectedDocumentId = "oracle-database-19c-concepts";
    private const string ExpectedDocumentContentObjectId =
        "6a10b7840c42a1dd6ea9b69337532ed3f903d17af24f144c2a104b925f6533d2";
    private const long ExpectedDocumentByteLength = 9_322_921;

    private static readonly LanguageModelDescriptor LanguageModelDescriptor = new(
        "openai",
        OpenAiLanguageModelOptions.MvpModelId,
        OpenAiLanguageModelOptions.MvpModelId);

    private readonly ProductQueryRuntimeOptions options;
    private readonly IAnswerEvidenceActivitySink answerEvidenceActivitySink;
    private readonly HttpClient embeddingClient;
    private readonly HttpClient languageModelClient;
    private readonly SemaphoreSlim initialisationGate = new(1, 1);
    private QuestionAnsweringService? answeringService;
    private VerifiedPageImageEvidenceReader? visualEvidenceReader;

    internal ProductQueryRuntime(
        ProductQueryRuntimeOptions options,
        IAnswerEvidenceActivitySink? answerEvidenceActivitySink = null)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.answerEvidenceActivitySink = answerEvidenceActivitySink ??
            NullAnswerEvidenceActivitySink.Instance;
        embeddingClient = CreateOpenAiClient();
        languageModelClient = CreateOpenAiClient();
    }

    public async Task<QueryExecutionResult> AskAsync(
        QueryRequest request,
        DateTimeOffset observedAt,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureComposedAsync(cancellationToken).ConfigureAwait(false);
            await ValidateCurrentAuthorityAsync(cancellationToken).ConfigureAwait(false);
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
            await EnsureComposedAsync(cancellationToken).ConfigureAwait(false);
            var snapshot = await VerifyPersistedStateAsync(
                observedAt,
                cancellationToken).ConfigureAwait(false);
            var eligible = snapshot.EvidenceBindings.Count(binding => binding.IsEligible);
            return new ReadinessV1(
                "Ready",
                snapshot.EvidenceBindings
                    .Select(binding => binding.Binding.DatabaseProductId)
                    .Distinct()
                    .Count(),
                eligible,
                snapshot.EvidenceBindings.Count - eligible,
                Array.Empty<SanitisedSourceStateV1>(),
                snapshot.ActivationRecord.IndexGenerationId.Value,
                ConfigurationRevision,
                [
                    new SanitisedCapabilityCheckV1("control-store", "Ready"),
                    new SanitisedCapabilityCheckV1("content-store", "Ready"),
                    new SanitisedCapabilityCheckV1("vector-store", "Ready"),
                    new SanitisedCapabilityCheckV1("openai-providers", "Configured"),
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
                [new SanitisedCapabilityCheckV1("product-runtime", "Unavailable")],
                observedAt);
        }
    }

    public async Task<VisualEvidenceReadResult> ReadAsync(
        VisualEvidenceSelector selector,
        DateTimeOffset observedAt,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureComposedAsync(cancellationToken).ConfigureAwait(false);
            await ValidateCurrentAuthorityAsync(cancellationToken).ConfigureAwait(false);
            return await visualEvidenceReader!.ReadAsync(
                selector,
                observedAt,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return VisualEvidenceReadResult.Unavailable();
        }
        catch (Exception exception) when (IsLocalRuntimeFailure(exception))
        {
            return VisualEvidenceReadResult.Unavailable();
        }
    }

    private async Task EnsureComposedAsync(CancellationToken cancellationToken)
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

            var stores = options.Stores;
            if (options.ApplyMigrations)
            {
                await SqliteStoreProvisioner.ApplyMigrationsAsync(
                    stores,
                    cancellationToken).ConfigureAwait(false);
            }

            var controlStore = new SqliteControlPlaneStore(stores);
            var activationReader = new SqliteQueryActivationReader(stores);
            var vectorStore = new SqliteVectorIndexStore(stores);
            IDocumentContentStore contentStore = new ImmutableContentStore(stores);
            var credentialSource = CredentialSource;
            var embeddingProvider = new OpenAiHttpEmbeddingProvider(
                embeddingClient,
                credentialSource);
            var languageModel = new OpenAiHttpLanguageModel(
                languageModelClient,
                credentialSource,
                LanguageModelDescriptor);
            var retrievalPolicyConfiguration = RetrievalPolicyConfiguration.CreateRetrievalV2(
                ProductAdministrativeMaterialisationProfile.EmbeddingDescriptor,
                ProductAdministrativeMaterialisationProfile.CompatibilityProfile.Key);
            var retrievalPolicyExecutor = new RetrievalV2PolicyExecutor(
                vectorStore,
                retrievalPolicyConfiguration);
            visualEvidenceReader = new VerifiedPageImageEvidenceReader(
                CorpusId,
                activationReader,
                controlStore,
                contentStore);
            answeringService = new QuestionAnsweringService(
                CorpusId,
                ProductAdministrativeMaterialisationProfile.EmbeddingDescriptor,
                LanguageModelDescriptor,
                activationReader,
                embeddingProvider,
                retrievalPolicyExecutor,
                retrievalPolicyConfiguration,
                languageModel,
                new SqliteAnswerEvidenceStore(stores),
                new SystemAnswerEvidenceRecordIdSource(),
                answerEvidenceActivitySink);
        }
        finally
        {
            initialisationGate.Release();
        }
    }

    private async Task ValidateCurrentAuthorityAsync(CancellationToken cancellationToken)
    {
        EnsureCredentialAvailable();
        var controlStore = new SqliteControlPlaneStore(options.Stores);
        var catalogue = await controlStore.ReadCurrentCatalogueAsync(
            CorpusId,
            cancellationToken).ConfigureAwait(false) ??
            throw new InvalidDataException("The product catalogue is unavailable.");
        var activation = await controlStore.ReadActiveActivationAsync(
            CorpusId,
            cancellationToken).ConfigureAwait(false) ??
            throw new InvalidDataException("The product activation record is unavailable.");
        ValidateOracleOnlyAuthority(catalogue, activation);
    }

    private async Task<QueryActivationSnapshot> VerifyPersistedStateAsync(
        DateTimeOffset observedAt,
        CancellationToken cancellationToken)
    {
        EnsureCredentialAvailable();
        var stores = options.Stores;
        var controlStore = new SqliteControlPlaneStore(stores);
        var catalogue = await controlStore.ReadCurrentCatalogueAsync(
            CorpusId,
            cancellationToken).ConfigureAwait(false) ??
            throw new InvalidDataException("The product catalogue is unavailable.");
        var activation = await controlStore.ReadActiveActivationAsync(
            CorpusId,
            cancellationToken).ConfigureAwait(false) ??
            throw new InvalidDataException("The product activation record is unavailable.");
        ValidateOracleOnlyAuthority(catalogue, activation);

        var contentStore = new ImmutableContentStore(stores);
        foreach (var evidence in activation.EvidenceBindings)
        {
            var document = catalogue.DocumentVersions.Single(item =>
                item.Id == evidence.DocumentBinding.DocumentId &&
                item.Version == evidence.DocumentBinding.DocumentVersion);
            await using var content = await contentStore.OpenVerifiedAsync(
                evidence.SourceContentObjectId,
                new ExpectedHashAndLength(
                    document.ContentObjectId,
                    document.ByteLength),
                cancellationToken).ConfigureAwait(false);
        }

        var snapshot = await new SqliteQueryActivationReader(stores).ReadAsync(
            CorpusId,
            observedAt,
            cancellationToken).ConfigureAwait(false) ??
            throw new InvalidDataException("The product query snapshot is unavailable.");
        if (snapshot.ActivationRecord.RecordRevision != activation.RecordRevision ||
            snapshot.EvidenceBindings.Count == 0 ||
            snapshot.EvidenceBindings.Any(binding =>
                !binding.IsEligible ||
                binding.Binding.DatabaseProductId != OracleDatabaseId))
        {
            throw new InvalidDataException("The product query snapshot is not Oracle-only.");
        }

        var sentinel = new float[
            ProductAdministrativeMaterialisationProfile.AcceptedEmbeddingDimensions];
        sentinel[0] = 1;
        var search = await new SqliteVectorIndexStore(stores).SearchExactAsync(
            new VectorSearchRequest(
                CorpusId,
                activation.IndexGenerationId,
                ProductAdministrativeMaterialisationProfile.CompatibilityProfile.Key,
                sentinel,
                maximumResults: 1,
                activation.DocumentBindings
                    .Select(VectorSearchBindingSelector.FromBinding)
                    .ToArray()),
            cancellationToken).ConfigureAwait(false);
        if (search.Outcome != VectorSearchOutcome.Succeeded || search.Hits.Count == 0)
        {
            throw new InvalidDataException("The product vector index is unavailable.");
        }

        return snapshot;
    }

    internal static void ValidateOracleOnlyAuthority(
        CatalogueSnapshot catalogue,
        CorpusActivationRecord activation)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(activation);

        var oracle = catalogue.DatabaseProducts.SingleOrDefault(product =>
            product.Id == OracleDatabaseId);
        var activeDocuments = catalogue.DocumentVersions
            .Where(document => document.Status == CatalogueItemStatus.Active)
            .ToArray();
        if (catalogue.CorpusId != CorpusId || activation.CorpusId != CorpusId ||
            catalogue.Revision.Value != ExpectedCatalogueRevision ||
            catalogue.DatabaseCategories.Count != 9 ||
            catalogue.DatabaseProducts.Count != 51 ||
            catalogue.DatabaseProducts.Sum(product => product.CategoryIds.Count) != 54 ||
            !string.Equals(
                CalculateCatalogueFingerprint(catalogue),
                ExpectedCatalogueFingerprint,
                StringComparison.Ordinal) ||
            oracle is null ||
            !string.Equals(oracle.DisplayName, "Oracle Database", StringComparison.Ordinal) ||
            oracle.Status != CatalogueItemStatus.Active ||
            catalogue.DatabaseProducts.Count(product =>
                product.Status == CatalogueItemStatus.Active) != 1 ||
            catalogue.DatabaseProducts.Count(product =>
                product.Id != OracleDatabaseId &&
                product.Status == CatalogueItemStatus.Candidate) != 50 ||
            catalogue.DocumentVersions.Count != 1 ||
            activeDocuments.Length != 1 ||
            !MatchesExpectedDocument(activeDocuments[0]) ||
            activation.CatalogueRevision != catalogue.Revision ||
            !activation.HasCompleteEvidenceBindings ||
            activation.DocumentBindings.Count != activeDocuments.Length ||
            activation.DocumentBindings.Any(binding =>
                binding.DatabaseProductId != OracleDatabaseId ||
                !activeDocuments.Any(document =>
                    document.Id == binding.DocumentId &&
                    document.Version == binding.DocumentVersion &&
                    document.DatabaseProductRevision == binding.DatabaseProductRevision &&
                    document.Format == binding.DocumentFormat &&
                    document.SourceAdapterId == binding.SourceAdapterId &&
                    document.SourceTrustClass == binding.SourceTrustClass)) ||
            activation.EvidenceBindings.Any(evidence =>
                !activeDocuments.Any(document =>
                    document.Id == evidence.DocumentBinding.DocumentId &&
                    document.Version == evidence.DocumentBinding.DocumentVersion &&
                    document.ContentObjectId == evidence.SourceContentObjectId)))
        {
            throw new InvalidDataException(
                "The configured product store is not the Oracle-only catalogue profile.");
        }
    }

    private static bool MatchesExpectedDocument(DocumentVersion document) =>
        document.Id.Value == ExpectedDocumentId &&
        document.Version.Value == 1 &&
        document.DatabaseProductId == OracleDatabaseId &&
        document.DatabaseProductRevision.Value == 1 &&
        document.Format == DocumentFormat.Pdf &&
        document.ContentLanguage == new DocumentContentLanguage("en") &&
        document.SourceDeclaredLanguage == new SourceDeclaredLanguage("en") &&
        document.ContentObjectId.Value == ExpectedDocumentContentObjectId &&
        document.ByteLength == ExpectedDocumentByteLength &&
        string.Equals(document.MediaType, "application/pdf", StringComparison.Ordinal) &&
        document.SourceAdapterId == new SourceAdapterId("local-authorised-pdf-v1") &&
        document.SourceTrustClass == SourceTrustClass.LocalAuthorised &&
        document.OfficialSourceRegistrationId is null &&
        document.OfficialSnapshotId is null;

    private static string CalculateCatalogueFingerprint(CatalogueSnapshot catalogue)
    {
        var canonical = new StringBuilder();
        foreach (var category in catalogue.DatabaseCategories.OrderBy(
                     category => category.Id.Value,
                     StringComparer.Ordinal))
        {
            canonical.Append("category|")
                .Append(category.Id.Value)
                .Append('|')
                .Append(category.DisplayName)
                .Append('\n');
        }

        foreach (var product in catalogue.DatabaseProducts.OrderBy(
                     product => product.Id.Value,
                     StringComparer.Ordinal))
        {
            canonical.Append("product|")
                .Append(product.Id.Value)
                .Append('|')
                .Append(product.Revision.Value)
                .Append('|')
                .Append(product.DisplayName)
                .Append('|')
                .AppendJoin(
                    ',',
                    product.CategoryIds
                        .Select(category => category.Value)
                        .Order(StringComparer.Ordinal))
                .Append('\n');
        }

        return Convert.ToHexString(SHA256.HashData(
                Encoding.UTF8.GetBytes(canonical.ToString())))
            .ToLowerInvariant();
    }

    private void EnsureCredentialAvailable()
    {
        if (string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable(options.CredentialEnvironmentVariable)))
        {
            throw new InvalidOperationException("The product provider credential is unavailable.");
        }
    }

    private ValueTask<string> CredentialSource(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(
            Environment.GetEnvironmentVariable(options.CredentialEnvironmentVariable) ??
            string.Empty);
    }

    private static HttpClient CreateOpenAiClient() =>
        new(
            OpenAiHttpClientPolicy.CreateDenyByDefaultHandler(),
            disposeHandler: true)
        {
            BaseAddress = new Uri("https://api.openai.com/", UriKind.Absolute),
            Timeout = TimeSpan.FromSeconds(25),
        };

    private static bool IsLocalRuntimeFailure(Exception exception) =>
        exception is ArgumentException or InvalidDataException or InvalidOperationException or IOException or
            UnauthorizedAccessException or SqliteException or ProviderStageUnavailableException;

    private static readonly DatabaseProductId OracleDatabaseId = new("oracle-database");

    public void Dispose()
    {
        initialisationGate.Dispose();
        embeddingClient.Dispose();
        languageModelClient.Dispose();
    }
}
