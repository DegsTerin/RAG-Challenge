// Purpose: Composes the disabled-by-default text-first product administration profile while keeping PDF rendering fail-closed until the accepted sandbox exists.
using Microsoft.Extensions.Configuration;

using RagChallenge.Application.Documents;
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Application.ProviderBudget;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Infrastructure.Documents;
using RagChallenge.Infrastructure.Persistence;
using RagChallenge.Infrastructure.Providers;

namespace RagChallenge.Server.Api.OperationsGovernance;

internal static class ProductAdministrativeMaterialisationProfile
{
    internal const string ProfileName = "product-administration-v1";
    internal const string ConfigurationSection =
        "RagChallenge:Administration:ProductMaterialisation";
    internal const string ExpectedCompatibilityKey =
        "d63b93fb7e7a91e3dd60c7bc61b3a802c1b0f0acdee7fdaf985dbccf8db78b81";
    internal const string AcceptedEmbeddingProviderId = "openai";
    internal const string AcceptedEmbeddingModelId = "text-embedding-3-small";
    internal const string AcceptedEmbeddingModelRevision = "text-embedding-3-small";
    internal const int AcceptedEmbeddingDimensions = 1536;
    internal const string AcceptedRenderProfileId = RenderProfileId.PdfPagePngNoticeV1;

    internal static readonly EmbeddingProviderDescriptor EmbeddingDescriptor = new(
        AcceptedEmbeddingProviderId,
        AcceptedEmbeddingModelId,
        AcceptedEmbeddingModelRevision,
        AcceptedEmbeddingDimensions);
    internal static readonly IndexCompatibilityProfile CompatibilityProfile = new(
        [
            PdfPigDocumentParser.CompatibilityDescriptor,
            CsvHelperDocumentParser.CompatibilityDescriptor,
        ],
        new ChunkingPolicy(),
        EmbeddingDescriptor,
        SqliteVectorIndexStore.CompatibilityDescriptor);

    static ProductAdministrativeMaterialisationProfile()
    {
        if (!string.Equals(
                CompatibilityProfile.Key.Value,
                ExpectedCompatibilityKey,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The frozen product index compatibility profile drifted.");
        }
    }

    internal static AdministrativeMaterialisationPorts Resolve(
        IConfiguration configuration,
        SqliteStoreOptions storeOptions,
        ProductAdministrativeMaterialisationDependencies? dependencies = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(storeOptions);
        var options = configuration.GetSection(ConfigurationSection)
            .Get<ProductAdministrativeMaterialisationOptions>() ??
            new ProductAdministrativeMaterialisationOptions();
        options.Validate();
        var credentialReference = OpaqueEnvironmentCredentialReference.Parse(
            options.Embedding.CredentialEnvironmentVariable);
        var operationalAuthority = ProductProviderOperationalAuthority.Parse(
            ProductProviderOperation.AdministrativeIndexEmbedding,
            options.Embedding.OperationalAuthorityReference);
        var operationalGrants =
            ProductProviderOperationalGrantSet.FromExplicitConfiguration(
                (ProductProviderOperation.AdministrativeIndexEmbedding,
                    options.Embedding.TrustedOperationalGrantReference));
        var selectedDependencies = dependencies ??
            ProductAdministrativeMaterialisationDependencies.CreateDefault();
        var authorityResolver = selectedDependencies.AuthorityResolverFactory(storeOptions) ??
            throw new ArgumentException(
                "The product authority resolver factory returned no port.",
                nameof(dependencies));
        var officialTransport = selectedDependencies.OfficialTransportFactory() ??
            throw new ArgumentException(
                "The product official transport factory returned no port.",
                nameof(dependencies));
        var embeddingClient = selectedDependencies.EmbeddingHttpClientFactory() ??
            throw new ArgumentException(
                "The product embedding client factory returned no client.",
                nameof(dependencies));
        var credentialSource = new ProductProviderCredentialSource(
            operationalAuthority,
            operationalGrants,
            ProductProviderOperation.AdministrativeIndexEmbedding,
            credentialReference.EnvironmentVariableName,
            selectedDependencies.CredentialEnvironmentReader);
        var budgetComposition = ProductProviderBudgetAdmission.CreateOperational(
            storeOptions,
            operationalAuthority,
            operationalGrants,
            ProductProviderOperation.AdministrativeIndexEmbedding,
            options.Embedding.Budget);
        var embeddingProvider = new OpenAiHttpEmbeddingProvider(
            embeddingClient,
            credentialSource.ReadAsync,
            budgetComposition.AdmissionGate,
            ProviderBudgetOperationClass.AdministrativeIndexEmbedding,
            new OpenAiEmbeddingPlanPolicy(
                exactRequestCount: 52,
                maximumInputsPerRequest: 64,
                exactLastRequestInputCount: 18,
                exactTotalInputCount: 3_282,
                maximumTotalMicroUsd: OpenAiEmbeddingCostSchedule.MicroUsdPerUsd),
            budgetComposition.PrepareAsync);
        return new AdministrativeMaterialisationPorts(
            LocalInputRoot: configuration["RagChallenge:Administration:InputRoot"],
            OfficialSourceAuthorityResolver: authorityResolver,
            OfficialSourceTransport: officialTransport,
            EmbeddingProvider: embeddingProvider,
            IndexCompatibilityProfile: CompatibilityProfile);
    }
}

internal sealed class ProductAdministrativeMaterialisationOptions
{
    public bool Enabled { get; init; }

    public ProductOfficialSourceOptions OfficialSource { get; init; } = new();

    public ProductEmbeddingOptions Embedding { get; init; } = new();

    public ProductRenderingOptions Rendering { get; init; } = new();

    internal void Validate()
    {
        if (!Enabled || !OfficialSource.Enabled || !Embedding.Enabled || Rendering.Enabled ||
            !string.Equals(
                Rendering.ProfileId,
                ProductAdministrativeMaterialisationProfile.AcceptedRenderProfileId,
                StringComparison.Ordinal) ||
            !string.Equals(
                Embedding.ProviderId,
                ProductAdministrativeMaterialisationProfile.AcceptedEmbeddingProviderId,
                StringComparison.Ordinal) ||
            !string.Equals(
                Embedding.ModelId,
                ProductAdministrativeMaterialisationProfile.AcceptedEmbeddingModelId,
                StringComparison.Ordinal) ||
            !string.Equals(
                Embedding.ModelRevision,
                ProductAdministrativeMaterialisationProfile.AcceptedEmbeddingModelRevision,
                StringComparison.Ordinal) ||
            Embedding.Dimensions !=
                ProductAdministrativeMaterialisationProfile.AcceptedEmbeddingDimensions)
        {
            throw new ArgumentException(
                "The product materialisation profile is disabled, incomplete, or drifted.");
        }
    }
}

internal sealed class ProductRenderingOptions
{
    public bool Enabled { get; init; }

    public string ProfileId { get; init; } = string.Empty;
}

internal sealed class ProductOfficialSourceOptions
{
    public bool Enabled { get; init; }
}

internal sealed class ProductEmbeddingOptions
{
    public bool Enabled { get; init; }

    public string ProviderId { get; init; } = string.Empty;

    public string ModelId { get; init; } = string.Empty;

    public string ModelRevision { get; init; } = string.Empty;

    public int Dimensions { get; init; }

    public string CredentialEnvironmentVariable { get; init; } = string.Empty;

    public string OperationalAuthorityReference { get; init; } = string.Empty;

    public string TrustedOperationalGrantReference { get; init; } = string.Empty;

    public ProductProviderBudgetOptions Budget { get; init; } = new();
}

internal sealed class ProductProviderBudgetOptions
{
    public bool Enabled { get; init; }

    public string EnvelopeId { get; init; } = string.Empty;

    public string StoreEpochId { get; init; } = string.Empty;

    public string RuntimeSessionId { get; init; } = string.Empty;

    public string EnvironmentId { get; init; } = string.Empty;

    public string BillingScopeReference { get; init; } = string.Empty;

    public string CostScheduleId { get; init; } = string.Empty;

    public string CostScheduleSha256 { get; init; } = string.Empty;

    public long AggregateLimitMicroUsd { get; init; }

    public long AdministrativeIndexEmbeddingLimitMicroUsd { get; init; }

    public long QueryEmbeddingLimitMicroUsd { get; init; }

    public long GroundedGenerationLimitMicroUsd { get; init; }

    public DateTimeOffset EffectiveAtUtc { get; init; }

    public DateTimeOffset ExpiresAtUtc { get; init; }

    public string CreationAuthorityReference { get; init; } = string.Empty;

    public string RearmAuthorityReference { get; init; } = string.Empty;

    public string ActorReference { get; init; } = string.Empty;

    internal void Validate(string? operationalAuthorityReference = null)
    {
        if (!Enabled ||
            !string.Equals(
                CostScheduleId,
                OpenAiEmbeddingCostSchedule.ScheduleId,
                StringComparison.Ordinal) ||
            !string.Equals(
                CostScheduleSha256,
                OpenAiEmbeddingCostSchedule.ScheduleSha256,
                StringComparison.Ordinal) ||
            AggregateLimitMicroUsd != OpenAiEmbeddingCostSchedule.MicroUsdPerUsd ||
            AdministrativeIndexEmbeddingLimitMicroUsd != AggregateLimitMicroUsd ||
            QueryEmbeddingLimitMicroUsd != 0 || GroundedGenerationLimitMicroUsd != 0 ||
            EffectiveAtUtc.Offset != TimeSpan.Zero || ExpiresAtUtc.Offset != TimeSpan.Zero ||
            ExpiresAtUtc <= EffectiveAtUtc ||
            ExpiresAtUtc - EffectiveAtUtc > TimeSpan.FromHours(4) ||
            string.IsNullOrWhiteSpace(operationalAuthorityReference) ||
            !string.Equals(
                CreationAuthorityReference,
                operationalAuthorityReference,
                StringComparison.Ordinal) ||
            !string.Equals(
                RearmAuthorityReference,
                operationalAuthorityReference,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The product provider-budget envelope is absent, over-budget, or drifted.");
        }

        _ = CreateInitialisationRequest();
        _ = new ProviderRuntimeSessionId(RuntimeSessionId);
    }

    internal ProviderBudgetEnvelopeInitialisationRequest CreateInitialisationRequest() =>
        new(
            new ProviderBudgetEnvelopeId(EnvelopeId),
            new ProviderBudgetStoreEpochId(StoreEpochId),
            new ProviderBudgetScope(
                new ProviderBudgetEnvironmentId(EnvironmentId),
                new ProviderBudgetProviderId(OpenAiEmbeddingCostSchedule.ProviderId),
                new ProviderBudgetBillingScopeReference(BillingScopeReference),
                new ProviderBudgetModelId(OpenAiEmbeddingCostSchedule.ModelId),
                new ProviderBudgetCurrencyCode(OpenAiEmbeddingCostSchedule.CurrencyCode),
                new ProviderBudgetAccountingUnitId(OpenAiEmbeddingCostSchedule.AccountingUnitId)),
            new ProviderBudgetCostScheduleId(CostScheduleId),
            new ProviderBudgetSha256(CostScheduleSha256),
            new ProviderBudgetUnits(AggregateLimitMicroUsd),
            [
                new ProviderBudgetOperationBalance(
                    ProviderBudgetOperationClass.AdministrativeIndexEmbedding,
                    new ProviderBudgetUnits(AdministrativeIndexEmbeddingLimitMicroUsd),
                    new ProviderBudgetUnits(0),
                    new ProviderBudgetUnits(0),
                    new ProviderBudgetUnits(0)),
                new ProviderBudgetOperationBalance(
                    ProviderBudgetOperationClass.QueryEmbedding,
                    new ProviderBudgetUnits(QueryEmbeddingLimitMicroUsd),
                    new ProviderBudgetUnits(0),
                    new ProviderBudgetUnits(0),
                    new ProviderBudgetUnits(0)),
                new ProviderBudgetOperationBalance(
                    ProviderBudgetOperationClass.GroundedGeneration,
                    new ProviderBudgetUnits(GroundedGenerationLimitMicroUsd),
                    new ProviderBudgetUnits(0),
                    new ProviderBudgetUnits(0),
                    new ProviderBudgetUnits(0)),
            ],
            EffectiveAtUtc,
            ExpiresAtUtc,
            new ProviderBudgetAuthorityReference(CreationAuthorityReference),
            new ProviderBudgetAuthorityReference(ActorReference),
            EffectiveAtUtc);
}

internal sealed record ProductAdministrativeMaterialisationDependencies(
    Func<SqliteStoreOptions, IOfficialSourceAuthorityResolver> AuthorityResolverFactory,
    Func<IOfficialSourceTransport> OfficialTransportFactory,
    Func<HttpClient> EmbeddingHttpClientFactory,
    Func<string, string?> CredentialEnvironmentReader)
{
    internal static ProductAdministrativeMaterialisationDependencies CreateDefault() =>
        new(
            options => new SqliteOfficialSourceAuthorityResolver(options),
            () => new OfficialSourceHttpTransport(),
            () => new HttpClient(
                OpenAiHttpClientPolicy.CreateDenyByDefaultHandler(),
                disposeHandler: true)
            {
                BaseAddress = new Uri("https://api.openai.com/", UriKind.Absolute),
                Timeout = TimeSpan.FromSeconds(25),
            },
            Environment.GetEnvironmentVariable);
}

internal sealed record OpaqueEnvironmentCredentialReference
{
    private OpaqueEnvironmentCredentialReference(string environmentVariableName)
    {
        EnvironmentVariableName = environmentVariableName;
    }

    internal string EnvironmentVariableName { get; }

    internal static OpaqueEnvironmentCredentialReference Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (value.Length > 128 ||
            !(value[0] is '_' or >= 'A' and <= 'Z') ||
            value.Any(character =>
                character is not '_' and not (>= 'A' and <= 'Z') and not (>= '0' and <= '9')))
        {
            throw new ArgumentException(
                "The provider credential reference must be an opaque uppercase environment-variable name.",
                nameof(value));
        }

        return new OpaqueEnvironmentCredentialReference(value);
    }
}
