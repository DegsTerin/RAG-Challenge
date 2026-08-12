// Purpose: Composes the disabled-by-default product administration profile from exact typed configuration, persistent authority, ADR-0006 transport, and the accepted lazy OpenAI embedding descriptor.
using Microsoft.Extensions.Configuration;

using RagChallenge.Application.Documents;
using RagChallenge.Application.IndexingRetrieval;
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
        "d0890e6a252b37a84451bebc0814b897e0e146e3a7ec397a51f6475a25f45ddb";
    internal const string AcceptedEmbeddingProviderId = "openai";
    internal const string AcceptedEmbeddingModelId = "text-embedding-3-small";
    internal const string AcceptedEmbeddingModelRevision = "text-embedding-3-small";
    internal const int AcceptedEmbeddingDimensions = 1536;

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
        var embeddingProvider = new OpenAiHttpEmbeddingProvider(
            embeddingClient,
            cancellationToken =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return ValueTask.FromResult(
                    selectedDependencies.CredentialEnvironmentReader(
                        credentialReference.EnvironmentVariableName) ?? string.Empty);
            });
        return new AdministrativeMaterialisationPorts(
            authorityResolver,
            officialTransport,
            embeddingProvider,
            CompatibilityProfile);
    }
}

internal sealed class ProductAdministrativeMaterialisationOptions
{
    public bool Enabled { get; init; }

    public ProductOfficialSourceOptions OfficialSource { get; init; } = new();

    public ProductEmbeddingOptions Embedding { get; init; } = new();

    internal void Validate()
    {
        if (!Enabled || !OfficialSource.Enabled || !Embedding.Enabled ||
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
