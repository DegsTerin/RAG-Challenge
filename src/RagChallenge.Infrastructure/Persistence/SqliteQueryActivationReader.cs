// Purpose: Resolves one complete query-time activation snapshot and its exact catalogue/freshness metadata without consulting external sources or selecting a later observation.
using Microsoft.EntityFrameworkCore;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.Infrastructure.Persistence;

public sealed class SqliteQueryActivationReader(SqliteStoreOptions options)
    : IQueryActivationReader
{
    private readonly SqliteStoreOptions options = options ??
        throw new ArgumentNullException(nameof(options));

    public async Task<QueryActivationSnapshot?> ReadAsync(
        CorpusId corpusId,
        DateTimeOffset observedAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(corpusId);
        ControlPlaneMapping.EnsureUtc(observedAt, nameof(observedAt));
        var controlStore = new SqliteControlPlaneStore(options);
        var activation = await controlStore.ReadActiveActivationAsync(
            corpusId,
            cancellationToken).ConfigureAwait(false);

        if (activation is null)
        {
            return null;
        }

        await using var context = options.CreateControlContext();
        var documentRows = await context.DocumentVersions.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var productRows = await context.DatabaseProductRevisions.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var registrationRows = await context.OfficialSourceRegistrations.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var snapshotRows = await context.OfficialSourceSnapshots.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var observationRows = await context.SourceObservations.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var resolved = new List<QueryEvidenceBinding>(activation.DocumentBindings.Count);

        foreach (var binding in activation.DocumentBindings)
        {
            var document = documentRows.SingleOrDefault(row =>
                row.DocumentId == binding.DocumentId.Value &&
                row.DocumentVersion == binding.DocumentVersion.Value) ??
                throw new InvalidDataException(
                    "An active binding has no exact document-version metadata.");
            var product = productRows.SingleOrDefault(row =>
                row.ProductId == binding.DatabaseProductId.Value &&
                row.ProductRevision == binding.DatabaseProductRevision.Value) ??
                throw new InvalidDataException(
                    "An active binding has no exact database revision metadata.");
            var language = document.ContentLanguage switch
            {
                "pt-BR" => SupportedLanguage.PtBr,
                "en-GB" => SupportedLanguage.EnGb,
                _ => throw new InvalidDataException(
                    "An active document has an unsupported content language."),
            };

            if (binding.SourceTrustClass == SourceTrustClass.LocalAuthorised)
            {
                resolved.Add(new QueryEvidenceBinding(
                    binding,
                    language,
                    SourceFreshness.Local,
                    product.DisplayName));
                continue;
            }

            var registrationId = binding.OfficialSourceRegistrationId!.Value;
            var snapshotId = binding.OfficialSnapshotId!.Value;
            var observationId = binding.SourceObservationId!.Value;
            var snapshot = snapshotRows.SingleOrDefault(row =>
                row.SnapshotId == snapshotId &&
                row.RegistrationId == registrationId) ??
                throw new InvalidDataException(
                    "An official binding has no exact immutable snapshot metadata.");
            var registration = registrationRows.SingleOrDefault(row =>
                row.RegistrationId == registrationId &&
                row.RegistrationRevision == snapshot.RegistrationRevision &&
                row.DocumentId == binding.DocumentId.Value &&
                row.ProductId == binding.DatabaseProductId.Value &&
                row.SourceAdapterId == binding.SourceAdapterId.Value) ??
                throw new InvalidDataException(
                    "An official binding has no exact immutable registration metadata.");
            var observationRow = observationRows.SingleOrDefault(row =>
                row.ObservationId == observationId &&
                row.RegistrationId == registrationId &&
                row.SnapshotId == snapshotId) ??
                throw new InvalidDataException(
                    "An official binding has no exact freshness observation.");
            var observation = ControlPlaneMapping.ToDomain(observationRow);
            resolved.Add(new QueryEvidenceBinding(
                binding,
                language,
                ResolveFreshness(observation, observedAt),
                product.DisplayName,
                registration.CanonicalHttpsUrl,
                observation.RevalidatedAt));
        }

        return new QueryActivationSnapshot(activation, resolved);
    }

    private static SourceFreshness ResolveFreshness(
        OfficialSourceObservation observation,
        DateTimeOffset observedAt) =>
        observation.State switch
        {
            OfficialObservationState.Current when observation.IsEligibleAt(observedAt) =>
                SourceFreshness.Current,
            OfficialObservationState.Current => SourceFreshness.Stale,
            OfficialObservationState.Stale => SourceFreshness.Stale,
            OfficialObservationState.Withdrawn => SourceFreshness.Withdrawn,
            OfficialObservationState.Deactivated => SourceFreshness.Deactivated,
            _ => SourceFreshness.Unavailable,
        };
}
