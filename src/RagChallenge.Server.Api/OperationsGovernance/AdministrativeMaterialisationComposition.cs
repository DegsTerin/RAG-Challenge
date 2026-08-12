// Purpose: Composes the governed materialisation commands at the administrative production boundary from explicit ports, rejecting absent dependency pairs and never selecting a real provider implicitly.
using RagChallenge.Application.Documents;
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Infrastructure.Documents;
using RagChallenge.Infrastructure.Persistence;

namespace RagChallenge.Server.Api.OperationsGovernance;

internal sealed record AdministrativeMaterialisationPorts(
    IOfficialSourceAuthorityResolver? OfficialSourceAuthorityResolver = null,
    IOfficialSourceTransport? OfficialSourceTransport = null,
    IEmbeddingProvider? EmbeddingProvider = null,
    IndexCompatibilityProfile? IndexCompatibilityProfile = null);

internal static class AdministrativeMaterialisationComposition
{
    internal static SqliteAdministrativeCommandExecutor CreateExecutor(
        SqliteStoreOptions options,
        IControlPlaneStore controlPlaneStore,
        AdministrativeMaterialisationPorts? ports)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(controlPlaneStore);

        if (ports is null)
        {
            return new SqliteAdministrativeCommandExecutor(controlPlaneStore);
        }

        EnsureCompletePair(
            ports.OfficialSourceAuthorityResolver,
            ports.OfficialSourceTransport,
            "Official synchronisation requires both authority and transport ports.");
        EnsureCompletePair(
            ports.EmbeddingProvider,
            ports.IndexCompatibilityProfile,
            "Index construction requires both embedding and compatibility ports.");

        if (ports.OfficialSourceTransport is null && ports.EmbeddingProvider is null)
        {
            return new SqliteAdministrativeCommandExecutor(controlPlaneStore);
        }

        var contentStore = new ImmutableContentStore(options);
        var ingestionService = new DocumentIngestionService(
            contentStore,
            [new PdfPigDocumentParser(), new CsvHelperDocumentParser()],
            new DeterministicChunkingStrategy());

        IAdministrativeMaterialisationCommand? synchroniseOfficial = null;

        if (ports.OfficialSourceTransport is not null)
        {
            synchroniseOfficial = new OfficialSynchronisationAdministrativeCommand(
                controlPlaneStore,
                ports.OfficialSourceAuthorityResolver!,
                new OfficialSourceSynchronisationService(
                    ports.OfficialSourceTransport,
                    controlPlaneStore,
                    ingestionService));
        }

        IAdministrativeMaterialisationCommand? buildIndex = null;

        if (ports.EmbeddingProvider is not null)
        {
            EnsureCompatibilityMatchesSelectedRuntime(
                ports.IndexCompatibilityProfile!);
            buildIndex = new BuildIndexAdministrativeCommand(
                controlPlaneStore,
                contentStore,
                ingestionService,
                new CorpusIndexingService(
                    ports.EmbeddingProvider,
                    new SqliteVectorIndexStore(options),
                    controlPlaneStore),
                ports.IndexCompatibilityProfile!);
        }

        return new SqliteAdministrativeCommandExecutor(
            controlPlaneStore,
            synchroniseOfficial,
            buildIndex);
    }

    private static void EnsureCompatibilityMatchesSelectedRuntime(
        IndexCompatibilityProfile profile)
    {
        var selectedParsers = new[]
        {
            PdfPigDocumentParser.CompatibilityDescriptor,
            CsvHelperDocumentParser.CompatibilityDescriptor,
        }.Order(StringComparer.Ordinal);

        if (!profile.ParserDescriptors.SequenceEqual(selectedParsers) ||
            !string.Equals(
                profile.VectorStoreDescriptor,
                SqliteVectorIndexStore.CompatibilityDescriptor,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The index compatibility profile does not describe the selected parser and vector-store runtime.",
                nameof(profile));
        }
    }

    private static void EnsureCompletePair<TLeft, TRight>(
        TLeft? left,
        TRight? right,
        string message)
        where TLeft : class
        where TRight : class
    {
        if ((left is null) != (right is null))
        {
            throw new ArgumentException(message, left is null ? nameof(left) : nameof(right));
        }
    }
}
