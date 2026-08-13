// Purpose: Reopens an explicitly configured product store and composes real OpenAI query providers without bootstrapping synthetic data or mutating catalogue and activation state.
using System.Security.Cryptography;
using System.Text;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

using RagChallenge.Application.Documents;
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;
using RagChallenge.Infrastructure.Persistence;
using RagChallenge.Infrastructure.Providers;
using RagChallenge.Server.Api.Contracts.V1;

namespace RagChallenge.Server.Api.OperationsGovernance;

internal enum ProductCatalogueProfile
{
    OracleDatabase19c,
    PostgreSql18,
}

internal sealed record ProductQueryRuntimeOptions(
    SqliteStoreOptions Stores,
    ProductCatalogueProfile CatalogueProfile,
    DocumentRightsEvidenceReference ApprovedRightsEvidenceReference,
    string CredentialEnvironmentVariable,
    bool ApplyMigrations)
{
    internal const string EnabledKey = "RagChallenge:Product:Enabled";
    internal const string ApplyMigrationsKey = "RagChallenge:Product:ApplyMigrations";
    internal const string StoreRootKey = "RagChallenge:Product:StoreRoot";
    internal const string CatalogueProfileKey = "RagChallenge:Product:CatalogueProfile";
    internal const string CredentialKey =
        "RagChallenge:Product:CredentialEnvironmentVariable";
    internal const string ApprovedRightsEvidenceKey =
        "RagChallenge:Product:ApprovedRightsEvidenceReference";
    internal const string SupersededUnverifiedRightsEvidenceReference =
        "owner-oracle19-public-source-approval-2026-08-12";

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

        var catalogueProfile = ParseCatalogueProfile(configuration[CatalogueProfileKey]);
        var approvedRightsEvidenceReference =
            ParseApprovedRightsEvidenceReference(configuration[ApprovedRightsEvidenceKey]);
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
            catalogueProfile,
            approvedRightsEvidenceReference,
            credential.EnvironmentVariableName,
            configuration.GetValue<bool>(ApplyMigrationsKey));
    }

    internal static ProductCatalogueProfile ParseCatalogueProfile(string? value) =>
        value switch
        {
            "oracle-database-19c" => ProductCatalogueProfile.OracleDatabase19c,
            "postgresql-18.4" => ProductCatalogueProfile.PostgreSql18,
            _ => throw new InvalidOperationException(
                "The product catalogue profile must be an exact supported identifier."),
        };

    internal static DocumentRightsEvidenceReference ParseApprovedRightsEvidenceReference(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                "An approved product rights evidence reference is required.");
        }

        var reference = new DocumentRightsEvidenceReference(value);
        if (string.Equals(
                reference.Value,
                SupersededUnverifiedRightsEvidenceReference,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The configured product rights evidence reference is not approved.");
        }

        return reference;
    }
}

internal sealed class ProductQueryRuntime :
    IQuestionAnsweringService,
    IQueryReadinessProbe,
    IVisualEvidenceReader,
    IDisposable
{
    internal static readonly CorpusId CorpusId = new("rag-challenge-product");
    internal const string OracleConfigurationRevision = "oracle-19c-product-v1";
    internal const string PostgreSqlConfigurationRevision = "postgresql-18.4-product-v1";
    private const long ExpectedOracleCatalogueRevision = 53;
    private const string ExpectedOracleCatalogueFingerprint =
        "d6b38c65bbe991eebb2b1f6ae67979512caef1246f1bd1c46e7d2b6e4e281000";
    private const string ExpectedOracleDocumentId = "oracle-database-19c-concepts";
    private const string ExpectedOracleDocumentContentObjectId =
        "6a10b7840c42a1dd6ea9b69337532ed3f903d17af24f144c2a104b925f6533d2";
    private const long ExpectedOracleDocumentByteLength = 9_322_921;
    private const long ExpectedPostgreSqlCatalogueRevision = 3;
    private const string ExpectedPostgreSqlCatalogueFingerprint =
        "8b8b801908c6957339c73b29e25a80f915204019d705285373ab6f2bd4b577c1";
    private const string ExpectedPostgreSqlDocumentId = "postgresql-18-reference-a4";
    private const string ExpectedPostgreSqlDocumentContentObjectId =
        "cea7b845568095eb56dee1b51bfa145c6c6637bc4377c986019971577efefae4";
    private const long ExpectedPostgreSqlDocumentByteLength = 15_771_040;

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

    private string ConfigurationRevision => options.CatalogueProfile switch
    {
        ProductCatalogueProfile.OracleDatabase19c => OracleConfigurationRevision,
        ProductCatalogueProfile.PostgreSql18 => PostgreSqlConfigurationRevision,
        _ => throw new InvalidOperationException("The product catalogue profile is unsupported."),
    };

    private DatabaseProductId ActiveDatabaseProductId => options.CatalogueProfile switch
    {
        ProductCatalogueProfile.OracleDatabase19c => OracleDatabaseId,
        ProductCatalogueProfile.PostgreSql18 => PostgreSqlDatabaseId,
        _ => throw new InvalidOperationException("The product catalogue profile is unsupported."),
    };

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
        var controlStore = new SqliteControlPlaneStore(options.Stores);
        var catalogue = await controlStore.ReadCurrentCatalogueAsync(
            CorpusId,
            cancellationToken).ConfigureAwait(false) ??
            throw new InvalidDataException("The product catalogue is unavailable.");
        var activation = await controlStore.ReadActiveActivationAsync(
            CorpusId,
            cancellationToken).ConfigureAwait(false) ??
            throw new InvalidDataException("The product activation record is unavailable.");
        ValidateConfiguredAuthority(
            catalogue,
            activation,
            options.ApprovedRightsEvidenceReference,
            options.CatalogueProfile);
        EnsureCredentialAvailable();
    }

    private async Task<QueryActivationSnapshot> VerifyPersistedStateAsync(
        DateTimeOffset observedAt,
        CancellationToken cancellationToken)
    {
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
        ValidateConfiguredAuthority(
            catalogue,
            activation,
            options.ApprovedRightsEvidenceReference,
            options.CatalogueProfile);
        EnsureCredentialAvailable();

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
                binding.Binding.DatabaseProductId != ActiveDatabaseProductId))
        {
            throw new InvalidDataException(
                "The product query snapshot does not match the configured catalogue profile.");
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

    internal static void ValidateConfiguredAuthority(
        CatalogueSnapshot catalogue,
        CorpusActivationRecord activation,
        DocumentRightsEvidenceReference approvedRightsEvidenceReference,
        ProductCatalogueProfile catalogueProfile)
    {
        switch (catalogueProfile)
        {
            case ProductCatalogueProfile.OracleDatabase19c:
                ValidateOracleOnlyAuthority(
                    catalogue,
                    activation,
                    approvedRightsEvidenceReference);
                return;
            case ProductCatalogueProfile.PostgreSql18:
                ValidatePostgreSql18Authority(
                    catalogue,
                    activation,
                    approvedRightsEvidenceReference);
                return;
            default:
                throw new InvalidOperationException(
                    "The product catalogue profile is unsupported.");
        }
    }

    internal static void ValidateOracleOnlyAuthority(
        CatalogueSnapshot catalogue,
        CorpusActivationRecord activation,
        DocumentRightsEvidenceReference approvedRightsEvidenceReference)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(activation);
        ArgumentNullException.ThrowIfNull(approvedRightsEvidenceReference);

        var oracle = catalogue.DatabaseProducts.SingleOrDefault(product =>
            product.Id == OracleDatabaseId);
        var activeDocuments = catalogue.DocumentVersions
            .Where(document => document.Status == CatalogueItemStatus.Active)
            .ToArray();
        if (catalogue.CorpusId != CorpusId || activation.CorpusId != CorpusId ||
            catalogue.Revision.Value != ExpectedOracleCatalogueRevision ||
            catalogue.DatabaseCategories.Count != 9 ||
            catalogue.DatabaseProducts.Count != 51 ||
            catalogue.DatabaseProducts.Sum(product => product.CategoryIds.Count) != 54 ||
            !string.Equals(
                CalculateCatalogueFingerprint(catalogue),
                ExpectedOracleCatalogueFingerprint,
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
            !MatchesExpectedOracleDocument(activeDocuments[0]) ||
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
                    document.SourceTrustClass == binding.SourceTrustClass &&
                    document.OfficialSourceRegistrationId ==
                        binding.OfficialSourceRegistrationId &&
                    document.OfficialSnapshotId == binding.OfficialSnapshotId) ||
                binding.SourceObservationId is not null) ||
            HasInvalidActivationEvidence(
                activation,
                activeDocuments,
                approvedRightsEvidenceReference))
        {
            throw new InvalidDataException(
                "The configured product store is not the Oracle-only catalogue profile.");
        }
    }

    internal static void ValidatePostgreSql18Authority(
        CatalogueSnapshot catalogue,
        CorpusActivationRecord activation,
        DocumentRightsEvidenceReference approvedRightsEvidenceReference)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(activation);
        ArgumentNullException.ThrowIfNull(approvedRightsEvidenceReference);

        var postgresql = catalogue.DatabaseProducts.SingleOrDefault(product =>
            product.Id == PostgreSqlDatabaseId);
        var activeDocuments = catalogue.DocumentVersions
            .Where(document => document.Status == CatalogueItemStatus.Active)
            .ToArray();
        if (catalogue.CorpusId != CorpusId || activation.CorpusId != CorpusId ||
            catalogue.Revision.Value != ExpectedPostgreSqlCatalogueRevision ||
            catalogue.DatabaseCategories.Count != 1 ||
            catalogue.DatabaseProducts.Count != 1 ||
            catalogue.DatabaseProducts.Sum(product => product.CategoryIds.Count) != 1 ||
            !string.Equals(
                CalculateCatalogueFingerprint(catalogue),
                ExpectedPostgreSqlCatalogueFingerprint,
                StringComparison.Ordinal) ||
            postgresql is null ||
            !string.Equals(postgresql.DisplayName, "PostgreSQL 18", StringComparison.Ordinal) ||
            postgresql.Status != CatalogueItemStatus.Active ||
            catalogue.DocumentVersions.Count != 1 ||
            activeDocuments.Length != 1 ||
            !MatchesExpectedPostgreSqlDocument(activeDocuments[0]) ||
            activation.CatalogueRevision != catalogue.Revision ||
            !activation.HasCompleteEvidenceBindings ||
            activation.DocumentBindings.Count != activeDocuments.Length ||
            activation.DocumentBindings.Any(binding =>
                binding.DatabaseProductId != PostgreSqlDatabaseId ||
                !activeDocuments.Any(document =>
                    document.Id == binding.DocumentId &&
                    document.Version == binding.DocumentVersion &&
                    document.DatabaseProductRevision == binding.DatabaseProductRevision &&
                    document.Format == binding.DocumentFormat &&
                    document.SourceAdapterId == binding.SourceAdapterId &&
                    document.SourceTrustClass == binding.SourceTrustClass &&
                    document.OfficialSourceRegistrationId ==
                        binding.OfficialSourceRegistrationId &&
                    document.OfficialSnapshotId == binding.OfficialSnapshotId) ||
                binding.SourceObservationId is not null) ||
            HasInvalidActivationEvidence(
                activation,
                activeDocuments,
                approvedRightsEvidenceReference))
        {
            throw new InvalidDataException(
                "The configured product store is not the PostgreSQL 18.4 catalogue profile.");
        }
    }

    private static bool HasInvalidActivationEvidence(
        CorpusActivationRecord activation,
        IReadOnlyCollection<DocumentVersion> activeDocuments,
        DocumentRightsEvidenceReference approvedRightsEvidenceReference) =>
        activation.EvidenceBindings.Any(evidence =>
            !activeDocuments.Any(document =>
                document.Id == evidence.DocumentBinding.DocumentId &&
                document.Version == evidence.DocumentBinding.DocumentVersion &&
                document.ContentObjectId == evidence.SourceContentObjectId) ||
            !DocumentRightsEligibilityPolicy.Evaluate(
                evidence.Rights,
                DocumentRightsEligibilityGate.PdfVisualEvidenceServing).IsEligible ||
            evidence.Rights.Decisions.Any(decision =>
                decision.EvidenceReference != approvedRightsEvidenceReference));

    private static bool MatchesExpectedOracleDocument(DocumentVersion document) =>
        document.Id.Value == ExpectedOracleDocumentId &&
        document.Version.Value == 1 &&
        document.DatabaseProductId == OracleDatabaseId &&
        document.DatabaseProductRevision.Value == 1 &&
        document.Format == DocumentFormat.Pdf &&
        document.ContentLanguage == new DocumentContentLanguage("en") &&
        document.SourceDeclaredLanguage == new SourceDeclaredLanguage("en") &&
        document.ContentObjectId.Value == ExpectedOracleDocumentContentObjectId &&
        document.ByteLength == ExpectedOracleDocumentByteLength &&
        string.Equals(document.MediaType, "application/pdf", StringComparison.Ordinal) &&
        document.SourceAdapterId == new SourceAdapterId("local-authorised-pdf-v1") &&
        document.SourceTrustClass == SourceTrustClass.LocalAuthorised &&
        document.OfficialSourceRegistrationId is null &&
        document.OfficialSnapshotId is null;

    private static bool MatchesExpectedPostgreSqlDocument(DocumentVersion document) =>
        document.Id.Value == ExpectedPostgreSqlDocumentId &&
        document.Version.Value == 1 &&
        document.DatabaseProductId == PostgreSqlDatabaseId &&
        document.DatabaseProductRevision.Value == 1 &&
        document.Format == DocumentFormat.Pdf &&
        document.ContentLanguage == new DocumentContentLanguage("en") &&
        document.SourceDeclaredLanguage == new SourceDeclaredLanguage("en") &&
        document.ContentObjectId.Value == ExpectedPostgreSqlDocumentContentObjectId &&
        document.ByteLength == ExpectedPostgreSqlDocumentByteLength &&
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
    private static readonly DatabaseProductId PostgreSqlDatabaseId = new("postgresql-18");

    public void Dispose()
    {
        initialisationGate.Dispose();
        embeddingClient.Dispose();
        languageModelClient.Dispose();
    }
}
