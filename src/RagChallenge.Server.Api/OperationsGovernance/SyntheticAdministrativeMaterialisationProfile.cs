// Purpose: Supplies explicit deterministic fake ports for administrative materialisation tests; it is available only through an opt-in Integration profile and never performs external access.
using System.Text;

using Microsoft.Extensions.Configuration;

using RagChallenge.Application.Documents;
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Infrastructure.Documents;
using RagChallenge.Infrastructure.Persistence;

namespace RagChallenge.Server.Api.OperationsGovernance;

internal static class SyntheticAdministrativeMaterialisationProfile
{
    internal const string ProfileKey =
        "RagChallenge:Administration:MaterialisationProfile";
    internal const string ProfileName = "synthetic-integration-v1";
    internal const string EnvironmentName = "Integration";

    internal static readonly DateTimeOffset SourceLastModified =
        new(2026, 8, 4, 16, 0, 0, TimeSpan.Zero);
    internal static readonly byte[] SourceBytes = Encoding.UTF8.GetBytes(
        "feature,description\nofficial,synthetic source only\n");
    internal static readonly EmbeddingProviderDescriptor EmbeddingDescriptor = new(
        "synthetic-provider",
        "synthetic-model",
        "synthetic-revision-1",
        dimensions: 2);
    internal static readonly IndexCompatibilityProfile CompatibilityProfile = new(
        [
            PdfPigDocumentParser.CompatibilityDescriptor,
            CsvHelperDocumentParser.CompatibilityDescriptor,
        ],
        new ChunkingPolicy(64, 8, 96),
        EmbeddingDescriptor,
        SqliteVectorIndexStore.CompatibilityDescriptor);
    internal static readonly OfficialSourceRegistration Registration = new(
        new OfficialSourceRegistrationId("synthetic-official-registration"),
        new SourceRegistrationRevision(1),
        new DatabaseProductId("admin-database"),
        new DocumentId("admin-official-document"),
        new SourceAdapterId("synthetic-official-csv"),
        "https://official.invalid/synthetic.csv",
        CatalogueItemStatus.Candidate);

    private static readonly CorpusId CorpusId = new("admin-corpus");

    internal static AdministrativeMaterialisationPorts? Resolve(
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var profile = configuration[ProfileKey];

        if (string.IsNullOrWhiteSpace(profile))
        {
            return null;
        }

        if (!string.Equals(profile, ProfileName, StringComparison.Ordinal) ||
            !IsIntegrationEnvironment(configuration))
        {
            throw new ArgumentException(
                "Administrative materialisation requires an explicit supported profile in the Integration environment.",
                nameof(configuration));
        }

        return new AdministrativeMaterialisationPorts(
            new SyntheticOfficialSourceAuthorityResolver(),
            new SyntheticOfficialSourceTransport(),
            new DeterministicEmbeddingProvider(),
            CompatibilityProfile);
    }

    private static bool IsIntegrationEnvironment(IConfiguration configuration)
    {
        var dotnetEnvironment = configuration["DOTNET_ENVIRONMENT"];
        var aspNetCoreEnvironment = configuration["ASPNETCORE_ENVIRONMENT"];

        if (!string.IsNullOrWhiteSpace(dotnetEnvironment) &&
            !string.IsNullOrWhiteSpace(aspNetCoreEnvironment) &&
            !string.Equals(
                dotnetEnvironment,
                aspNetCoreEnvironment,
                StringComparison.Ordinal))
        {
            return false;
        }

        var environment = string.IsNullOrWhiteSpace(dotnetEnvironment)
            ? aspNetCoreEnvironment
            : dotnetEnvironment;
        return string.Equals(environment, EnvironmentName, StringComparison.Ordinal);
    }

    private sealed class SyntheticOfficialSourceAuthorityResolver
        : IOfficialSourceAuthorityResolver
    {
        public Task<OfficialSourceAuthority?> ResolveAsync(
            CorpusId corpusId,
            OfficialSourceRegistrationId registrationId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            OfficialSourceAuthority? authority =
                corpusId == CorpusId && registrationId == Registration.Id
                    ? new OfficialSourceAuthority(
                        Registration,
                        currentSnapshot: null,
                        observationJournalRevision: 0,
                        activationRevision: 0)
                    : null;
            return Task.FromResult(authority);
        }
    }

    private sealed class SyntheticOfficialSourceTransport : IOfficialSourceTransport
    {
        public Task<OfficialFetchResult> FetchAsync(
            OfficialSourceRegistration registration,
            OfficialFetchPolicy policy,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (registration != Registration || SourceBytes.Length > policy.MaximumByteLength)
            {
                throw new InvalidOperationException(
                    "The synthetic transport received an unauthorised request.");
            }

            return Task.FromResult(new OfficialFetchResult(
                OfficialFetchStatus.Changed,
                statusCode: 200,
                SourceBytes.ToArray(),
                ContentMediaType.TextCsv.Value,
                "\"synthetic-v1\"",
                SourceLastModified));
        }
    }

    private sealed class DeterministicEmbeddingProvider : IEmbeddingProvider
    {
        public Task<EmbeddingBatchResult> EmbedAsync(
            EmbeddingBatchRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var vectors = request.Inputs.Select((input, index) =>
            {
                var vector = new float[EmbeddingDescriptor.Dimensions];
                vector[0] = input.Length;
                vector[1] = index + 1;
                return (ReadOnlyMemory<float>)vector;
            }).ToArray();
            return Task.FromResult(new EmbeddingBatchResult(
                EmbeddingDescriptor,
                vectors));
        }
    }
}
