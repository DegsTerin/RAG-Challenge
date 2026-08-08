// Purpose: Resolves one complete query-time activation snapshot and its exact catalogue/freshness metadata without consulting external sources or selecting a later observation.
using Microsoft.EntityFrameworkCore;

using RagChallenge.Application.Documents;
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

        if (activation is null ||
            !activation.HasCompleteEvidenceBindings ||
            activation.EvidenceBindings.Any(evidence =>
                !DocumentRightsEligibilityPolicy.Evaluate(
                    evidence.Rights,
                    evidence.DocumentBinding.DocumentFormat == DocumentFormat.Pdf
                        ? DocumentRightsEligibilityGate.PdfVisualEvidence
                        : DocumentRightsEligibilityGate.TextualEvidence).IsEligible))
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
        var renderManifestRows = await context.DocumentRenderManifests.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var resolved = new List<QueryEvidenceBinding>(activation.DocumentBindings.Count);

        foreach (var binding in activation.DocumentBindings)
        {
            var evidence = activation.EvidenceBindings.Single(item =>
                item.DocumentBinding == binding);
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

            if (document.ProductId != binding.DatabaseProductId.Value ||
                document.ProductRevision != binding.DatabaseProductRevision.Value ||
                document.DocumentFormat != binding.DocumentFormat.ToString() ||
                document.ContentSha256 != evidence.SourceContentObjectId.Value ||
                document.SourceAdapterId != binding.SourceAdapterId.Value ||
                document.SourceTrustClass != binding.SourceTrustClass.ToString() ||
                document.OfficialRegistrationId != binding.OfficialSourceRegistrationId?.Value ||
                document.OfficialSnapshotId != binding.OfficialSnapshotId?.Value)
            {
                throw new InvalidDataException(
                    "An active evidence binding differs from its exact document source metadata.");
            }

            if (binding.DocumentFormat == DocumentFormat.Pdf)
            {
                var renderManifest = renderManifestRows.SingleOrDefault(row =>
                    row.RenderManifestId == evidence.RenderManifestId!.Value);

                if (renderManifest is null ||
                    renderManifest.DocumentId != binding.DocumentId.Value ||
                    renderManifest.DocumentVersion != binding.DocumentVersion.Value ||
                    renderManifest.SourceContentSha256 != evidence.SourceContentObjectId.Value)
                {
                    throw new InvalidDataException(
                        "An active PDF evidence binding has no exact render manifest.");
                }
            }

            var hydratedRenderManifest = evidence.RenderManifestId is null
                ? null
                : await controlStore.ReadAsync(
                    corpusId,
                    evidence.RenderManifestId,
                    cancellationToken).ConfigureAwait(false) ??
                    throw new InvalidDataException(
                        "An active PDF evidence binding has no readable final render manifest.");

            DocumentContentLanguage language;

            try
            {
                language = new DocumentContentLanguage(document.ContentLanguage);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidDataException(
                    "An active document has an invalid BCP 47 content language.",
                    exception);
            }

            if (!language.IsSupportedByV1)
            {
                throw new InvalidDataException(
                    "An active document has a content language unsupported by runtime v1.");
            }

            if (binding.SourceTrustClass == SourceTrustClass.LocalAuthorised)
            {
                resolved.Add(new QueryEvidenceBinding(
                    binding,
                    evidence,
                    hydratedRenderManifest,
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
                evidence,
                hydratedRenderManifest,
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
