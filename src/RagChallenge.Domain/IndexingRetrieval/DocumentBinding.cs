// Purpose: Defines immutable generation and activation bindings plus append-only freshness observations; network acquisition remains an Infrastructure concern.
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Domain.IndexingRetrieval;

public sealed record DocumentBinding
{
    public DocumentBinding(
        DatabaseProductId databaseProductId,
        DatabaseProductRevision databaseProductRevision,
        DocumentId documentId,
        DocumentVersionNumber documentVersion,
        DocumentFormat documentFormat,
        SourceAdapterId sourceAdapterId,
        SourceTrustClass sourceTrustClass,
        OfficialSourceRegistrationId? officialSourceRegistrationId = null,
        OfficialSnapshotId? officialSnapshotId = null,
        OfficialObservationId? sourceObservationId = null)
    {
        ArgumentNullException.ThrowIfNull(databaseProductId);
        ArgumentNullException.ThrowIfNull(databaseProductRevision);
        ArgumentNullException.ThrowIfNull(documentId);
        ArgumentNullException.ThrowIfNull(documentVersion);
        ArgumentNullException.ThrowIfNull(sourceAdapterId);

        if (!Enum.IsDefined(documentFormat))
        {
            throw new ArgumentOutOfRangeException(
                nameof(documentFormat),
                documentFormat,
                "A binding document format must belong to the closed MVP set.");
        }

        if (!Enum.IsDefined(sourceTrustClass))
        {
            throw new ArgumentOutOfRangeException(
                nameof(sourceTrustClass),
                sourceTrustClass,
                "A binding trust class must belong to the closed provenance set.");
        }

        if (sourceTrustClass == SourceTrustClass.LocalAuthorised &&
            (officialSourceRegistrationId is not null ||
             officialSnapshotId is not null ||
             sourceObservationId is not null))
        {
            throw new ArgumentException(
                "A local binding cannot carry official registration, snapshot, or observation identities.");
        }

        if (sourceTrustClass == SourceTrustClass.OfficialExternal &&
            (officialSourceRegistrationId is null ||
             officialSnapshotId is null ||
             sourceObservationId is null))
        {
            throw new ArgumentException(
                "An official activation binding requires registration, snapshot, and observation identities.");
        }

        DatabaseProductId = databaseProductId;
        DatabaseProductRevision = databaseProductRevision;
        DocumentId = documentId;
        DocumentVersion = documentVersion;
        DocumentFormat = documentFormat;
        SourceAdapterId = sourceAdapterId;
        SourceTrustClass = sourceTrustClass;
        OfficialSourceRegistrationId = officialSourceRegistrationId;
        OfficialSnapshotId = officialSnapshotId;
        SourceObservationId = sourceObservationId;
    }

    public DatabaseProductId DatabaseProductId { get; }

    public DatabaseProductRevision DatabaseProductRevision { get; }

    public DocumentId DocumentId { get; }

    public DocumentVersionNumber DocumentVersion { get; }

    public DocumentFormat DocumentFormat { get; }

    public SourceAdapterId SourceAdapterId { get; }

    public SourceTrustClass SourceTrustClass { get; }

    public OfficialSourceRegistrationId? OfficialSourceRegistrationId { get; }

    public OfficialSnapshotId? OfficialSnapshotId { get; }

    public OfficialObservationId? SourceObservationId { get; }

    public DocumentBinding WithObservation(OfficialObservationId observationId)
    {
        ArgumentNullException.ThrowIfNull(observationId);

        if (SourceTrustClass != SourceTrustClass.OfficialExternal)
        {
            throw new InvalidOperationException(
                "Only an official binding can be rebound to a freshness observation.");
        }

        return new DocumentBinding(
            DatabaseProductId,
            DatabaseProductRevision,
            DocumentId,
            DocumentVersion,
            DocumentFormat,
            SourceAdapterId,
            SourceTrustClass,
            OfficialSourceRegistrationId,
            OfficialSnapshotId,
            observationId);
    }
}

public sealed record OfficialSourceObservation
{
    public OfficialSourceObservation(
        OfficialObservationId id,
        OfficialSourceRegistrationId registrationId,
        OfficialSnapshotId snapshotId,
        ObservationJournalRevision journalRevision,
        OfficialObservationState state,
        DateTimeOffset revalidatedAt,
        TimeSpan maxAge)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(registrationId);
        ArgumentNullException.ThrowIfNull(snapshotId);
        ArgumentNullException.ThrowIfNull(journalRevision);

        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "An observation state must belong to the closed freshness set.");
        }

        if (revalidatedAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Observation instants must be expressed in UTC.",
                nameof(revalidatedAt));
        }

        if (maxAge <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxAge),
                maxAge,
                "Observation maxAge must be positive.");
        }

        Id = id;
        RegistrationId = registrationId;
        SnapshotId = snapshotId;
        JournalRevision = journalRevision;
        State = state;
        RevalidatedAt = revalidatedAt;
        MaxAge = maxAge;
    }

    public OfficialObservationId Id { get; }

    public OfficialSourceRegistrationId RegistrationId { get; }

    public OfficialSnapshotId SnapshotId { get; }

    public ObservationJournalRevision JournalRevision { get; }

    public OfficialObservationState State { get; }

    public DateTimeOffset RevalidatedAt { get; }

    public TimeSpan MaxAge { get; }

    public bool IsEligibleAt(DateTimeOffset observedAt)
    {
        if (observedAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Eligibility must be evaluated at a UTC instant.",
                nameof(observedAt));
        }

        return State == OfficialObservationState.Current &&
            observedAt >= RevalidatedAt &&
            observedAt - RevalidatedAt <= MaxAge;
    }
}
