// Purpose: Resolves the official-source materialisation authority from control.db only, rejecting incomplete journal, snapshot, registration, or activation state before any transport can run.
using Microsoft.EntityFrameworkCore;

using RagChallenge.Application.Documents;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Infrastructure.Persistence;

public sealed class SqliteOfficialSourceAuthorityResolver(SqliteStoreOptions options)
    : IOfficialSourceAuthorityResolver
{
    private readonly SqliteStoreOptions options =
        options ?? throw new ArgumentNullException(nameof(options));

    public async Task<OfficialSourceAuthority?> ResolveAsync(
        CorpusId corpusId,
        OfficialSourceRegistrationId registrationId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(corpusId);
        ArgumentNullException.ThrowIfNull(registrationId);
        await using var context = options.CreateControlContext();
        var registration = await context.OfficialSourceRegistrations.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value &&
                row.RegistrationId == registrationId.Value)
            .OrderByDescending(row => row.RegistrationRevision)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (registration is null)
        {
            return null;
        }

        var journalHead = await context.ObservationJournalHeads.AsNoTracking()
            .SingleOrDefaultAsync(
                row => row.CorpusId == corpusId.Value,
                cancellationToken).ConfigureAwait(false);
        var maximumJournalRevision = await context.SourceObservations.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value)
            .Select(row => (long?)row.JournalRevision)
            .MaxAsync(cancellationToken).ConfigureAwait(false) ?? 0;
        var journalEntryCount = await context.SourceObservations.AsNoTracking()
            .LongCountAsync(
                row => row.CorpusId == corpusId.Value,
                cancellationToken).ConfigureAwait(false);

        if ((journalHead?.JournalRevision ?? 0) != maximumJournalRevision ||
            journalEntryCount != maximumJournalRevision)
        {
            throw new InvalidDataException(
                "The persisted official-source observation journal is incomplete.");
        }

        var latestObservation = await context.SourceObservations.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value &&
                row.RegistrationId == registrationId.Value)
            .OrderByDescending(row => row.JournalRevision)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        var snapshotsExist = await context.OfficialSourceSnapshots.AsNoTracking()
            .AnyAsync(
                row => row.CorpusId == corpusId.Value &&
                    row.RegistrationId == registrationId.Value,
                cancellationToken).ConfigureAwait(false);
        var orphanSnapshotExists = await context.OfficialSourceSnapshots.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value &&
                row.RegistrationId == registrationId.Value)
            .AnyAsync(
                snapshot => !context.SourceObservations.Any(observation =>
                    observation.CorpusId == corpusId.Value &&
                    observation.RegistrationId == registrationId.Value &&
                    observation.SnapshotId == snapshot.SnapshotId),
                cancellationToken).ConfigureAwait(false);
        OfficialSourceSnapshot? currentSnapshot = null;

        if (orphanSnapshotExists)
        {
            throw new InvalidDataException(
                "An official-source snapshot lacks its append-only observation authority.");
        }

        if (latestObservation is null)
        {
            if (snapshotsExist)
            {
                throw new InvalidDataException(
                    "An official-source snapshot lacks its append-only observation authority.");
            }
        }
        else
        {
            var snapshot = await context.OfficialSourceSnapshots.AsNoTracking()
                .SingleOrDefaultAsync(
                    row => row.CorpusId == corpusId.Value &&
                        row.SnapshotId == latestObservation.SnapshotId,
                    cancellationToken).ConfigureAwait(false);

            if (snapshot is null ||
                !string.Equals(
                    snapshot.RegistrationId,
                    registration.RegistrationId,
                    StringComparison.Ordinal) ||
                snapshot.RegistrationRevision != registration.RegistrationRevision)
            {
                throw new InvalidDataException(
                    "The current official-source observation drifted from its registration revision.");
            }

            currentSnapshot = new OfficialSourceSnapshot(
                new OfficialSnapshotId(snapshot.SnapshotId),
                registrationId,
                new ContentObjectId(snapshot.ContentSha256),
                snapshot.ByteLength,
                snapshot.MediaType,
                ControlPlaneMapping.ParseUtc(snapshot.RetrievedAtUtc));
        }

        var activationHead = await context.ActivationHeads.AsNoTracking()
            .SingleOrDefaultAsync(
                row => row.CorpusId == corpusId.Value,
                cancellationToken).ConfigureAwait(false);
        var activationRevision = activationHead?.RecordRevision ?? 0;
        var maximumActivationRevision = await context.ActivationRecords.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value)
            .Select(row => (long?)row.RecordRevision)
            .MaxAsync(cancellationToken).ConfigureAwait(false) ?? 0;

        if (activationRevision != maximumActivationRevision ||
            activationHead is not null &&
            !await context.ActivationRecords.AsNoTracking().AnyAsync(
                row => row.CorpusId == corpusId.Value &&
                    row.RecordRevision == activationRevision,
                cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidDataException(
                "The persisted activation head has no matching authority record.");
        }

        var domainRegistration = new OfficialSourceRegistration(
            registrationId,
            new SourceRegistrationRevision(registration.RegistrationRevision),
            new DatabaseProductId(registration.ProductId),
            new DocumentId(registration.DocumentId),
            new SourceAdapterId(registration.SourceAdapterId),
            registration.CanonicalHttpsUrl,
            Enum.Parse<CatalogueItemStatus>(registration.Status, ignoreCase: false));
        return new OfficialSourceAuthority(
            domainRegistration,
            currentSnapshot,
            maximumJournalRevision,
            activationRevision);
    }
}
