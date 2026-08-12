// Purpose: Orchestrates a trusted official-source registration through a bounded transport, immutable snapshot, parsing and append-only observation without accepting public URL authority.
using RagChallenge.Application.Administration;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.Application.Documents;

public enum OfficialFetchStatus
{
    Changed,
    NotModified,
    Withdrawn,
}

public sealed record OfficialFetchPolicy(
    long MaximumByteLength,
    string? IfNoneMatch,
    DateTimeOffset? IfModifiedSince);

public sealed class OfficialFetchResult
{
    public OfficialFetchResult(
        OfficialFetchStatus status,
        int statusCode,
        byte[]? content,
        string? mediaType,
        string? etag,
        DateTimeOffset? lastModified)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        if (statusCode is < 100 or > 599)
        {
            throw new ArgumentOutOfRangeException(nameof(statusCode));
        }

        if ((status == OfficialFetchStatus.Changed) != (content is not null))
        {
            throw new ArgumentException(
                "Only a changed official response can carry content.",
                nameof(content));
        }

        if (status == OfficialFetchStatus.Changed &&
            string.IsNullOrWhiteSpace(mediaType))
        {
            throw new ArgumentException(
                "Changed official content requires a media type.",
                nameof(mediaType));
        }

        if (lastModified is not null && lastModified.Value.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Official Last-Modified instants must be expressed in UTC.",
                nameof(lastModified));
        }

        Status = status;
        StatusCode = statusCode;
        Content = content;
        MediaType = mediaType;
        ETag = etag;
        LastModified = lastModified;
    }

    public OfficialFetchStatus Status { get; }

    public int StatusCode { get; }

    public byte[]? Content { get; }

    public string? MediaType { get; }

    public string? ETag { get; }

    public DateTimeOffset? LastModified { get; }
}

public interface IOfficialSourceTransport
{
    Task<OfficialFetchResult> FetchAsync(
        OfficialSourceRegistration registration,
        OfficialFetchPolicy policy,
        CancellationToken cancellationToken = default);
}

public interface IOfficialSourceAuthorityResolver
{
    Task<OfficialSourceAuthority?> ResolveAsync(
        CorpusId corpusId,
        OfficialSourceRegistrationId registrationId,
        CancellationToken cancellationToken = default);
}

public sealed record OfficialSourceAuthority
{
    public OfficialSourceAuthority(
        OfficialSourceRegistration registration,
        OfficialSourceSnapshot? currentSnapshot,
        long observationJournalRevision,
        long activationRevision)
    {
        Registration = registration ?? throw new ArgumentNullException(nameof(registration));

        if (currentSnapshot is not null &&
            currentSnapshot.RegistrationId != registration.Id)
        {
            throw new ArgumentException(
                "The resolved snapshot must belong to the resolved registration.",
                nameof(currentSnapshot));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(observationJournalRevision);
        ArgumentOutOfRangeException.ThrowIfNegative(activationRevision);
        CurrentSnapshot = currentSnapshot;
        ObservationJournalRevision = observationJournalRevision;
        ActivationRevision = activationRevision;
    }

    public OfficialSourceRegistration Registration { get; }

    public OfficialSourceSnapshot? CurrentSnapshot { get; }

    public long ObservationJournalRevision { get; }

    public long ActivationRevision { get; }
}

public enum OfficialSynchronisationOutcome
{
    UnchangedObservationCreated,
    SnapshotCreatedRebuildRequired,
    WithdrawnObservationCreated,
}

public sealed record OfficialSynchronisationRequest(
    CorpusId CorpusId,
    OfficialSourceRegistration Registration,
    DocumentFormat DocumentFormat,
    DocumentChunkingContext ChunkingContext,
    ParserPolicy ParserPolicy,
    ChunkingPolicy ChunkingPolicy,
    long MaximumByteLength,
    AdministrativeAuditContext SnapshotAuditContext,
    AdministrativeAuditContext ObservationAuditContext,
    long ExpectedJournalRevision,
    OfficialObservationId ObservationId,
    TimeSpan MaxAge,
    OfficialSourceSnapshot? CurrentSnapshot = null,
    string? CurrentEtag = null,
    DateTimeOffset? CurrentLastModified = null,
    long ExpectedActivationRevision = 0);

public sealed class OfficialSynchronisationResult
{
    public OfficialSynchronisationResult(
        OfficialSynchronisationOutcome outcome,
        OfficialSourceSnapshot snapshot,
        OfficialSourceObservation observation,
        IReadOnlyList<DocumentChunk> chunks,
        string? etag,
        DateTimeOffset? lastModified)
    {
        Outcome = outcome;
        Snapshot = snapshot;
        Observation = observation;
        Chunks = chunks;
        ETag = etag;
        LastModified = lastModified;
    }

    public OfficialSynchronisationOutcome Outcome { get; }

    public OfficialSourceSnapshot Snapshot { get; }

    public OfficialSourceObservation Observation { get; }

    public IReadOnlyList<DocumentChunk> Chunks { get; }

    public string? ETag { get; }

    public DateTimeOffset? LastModified { get; }
}

public sealed class OfficialSourceSynchronisationService
{
    private readonly IOfficialSourceTransport transport;
    private readonly IControlPlaneStore controlPlaneStore;
    private readonly DocumentIngestionService ingestionService;

    public OfficialSourceSynchronisationService(
        IOfficialSourceTransport transport,
        IControlPlaneStore controlPlaneStore,
        DocumentIngestionService ingestionService)
    {
        this.transport = transport ?? throw new ArgumentNullException(nameof(transport));
        this.controlPlaneStore = controlPlaneStore ??
            throw new ArgumentNullException(nameof(controlPlaneStore));
        this.ingestionService = ingestionService ??
            throw new ArgumentNullException(nameof(ingestionService));
    }

    public async Task<OfficialSynchronisationResult> SynchroniseAsync(
        OfficialSynchronisationRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);
        var fetch = await transport.FetchAsync(
            request.Registration,
            new OfficialFetchPolicy(
                request.MaximumByteLength,
                request.CurrentEtag,
                request.CurrentLastModified),
            cancellationToken).ConfigureAwait(false);

        ValidateResponse(fetch, request);

        if (fetch.Status is OfficialFetchStatus.NotModified or
            OfficialFetchStatus.Withdrawn)
        {
            if (request.CurrentSnapshot is null)
            {
                throw new InvalidOperationException(
                    "An unchanged or withdrawn observation requires a current snapshot.");
            }

            var terminalObservation = await AppendObservationWithRebindAsync(
                request,
                request.CurrentSnapshot,
                fetch,
                fetch.Status == OfficialFetchStatus.Withdrawn
                    ? OfficialObservationState.Withdrawn
                    : OfficialObservationState.Current,
                cancellationToken).ConfigureAwait(false);
            return new OfficialSynchronisationResult(
                fetch.Status == OfficialFetchStatus.Withdrawn
                    ? OfficialSynchronisationOutcome.WithdrawnObservationCreated
                    : OfficialSynchronisationOutcome.UnchangedObservationCreated,
                request.CurrentSnapshot,
                terminalObservation,
                [],
                fetch.ETag ?? request.CurrentEtag,
                fetch.LastModified ?? request.CurrentLastModified);
        }

        await using var content = new MemoryStream(fetch.Content!, writable: false);
        var mediaType = new ContentMediaType(fetch.MediaType!);
        var ingestion = await ingestionService.IngestAsync(
            new DocumentIngestionRequest(
                content,
                request.MaximumByteLength,
                mediaType,
                request.ParserPolicy,
                request.ChunkingPolicy,
                request.ChunkingContext),
            cancellationToken).ConfigureAwait(false);

        if (request.CurrentSnapshot?.ContentObjectId == ingestion.Content.ContentObjectId)
        {
            var unchangedObservation = await AppendObservationWithRebindAsync(
                request,
                request.CurrentSnapshot,
                fetch,
                OfficialObservationState.Current,
                cancellationToken).ConfigureAwait(false);
            return new OfficialSynchronisationResult(
                OfficialSynchronisationOutcome.UnchangedObservationCreated,
                request.CurrentSnapshot,
                unchangedObservation,
                ingestion.Chunks,
                fetch.ETag,
                fetch.LastModified);
        }

        var snapshot = new OfficialSourceSnapshot(
            new OfficialSnapshotId($"snapshot-{ingestion.Content.ContentObjectId.Value}"),
            request.Registration.Id,
            ingestion.Content.ContentObjectId,
            ingestion.Content.ByteLength,
            ingestion.Content.MediaType.Value,
            request.SnapshotAuditContext.RequestedAt);
        var snapshotAuditDigest = request.SnapshotAuditContext.CreateDigest(
            request.Registration.Id.Value,
            fetch.StatusCode.ToString(System.Globalization.CultureInfo.InvariantCulture),
            fetch.ETag ?? "",
            fetch.LastModified?.ToString(
                "O",
                System.Globalization.CultureInfo.InvariantCulture) ?? "",
            snapshot.ContentObjectId.Value);
        var snapshotCommit = await controlPlaneStore.CommitOfficialSourceAsync(
            new OfficialSourceCommitRequest(
                request.SnapshotAuditContext.OperationId,
                request.CorpusId,
                request.Registration,
                snapshot,
                request.SnapshotAuditContext.RequestedAt,
                snapshotAuditDigest),
            cancellationToken).ConfigureAwait(false);
        EnsureApplied(snapshotCommit, "official snapshot");
        var createdObservation = await AppendObservationAsync(
            request,
            snapshot,
            fetch,
            OfficialObservationState.Current,
            cancellationToken).ConfigureAwait(false);
        return new OfficialSynchronisationResult(
            OfficialSynchronisationOutcome.SnapshotCreatedRebuildRequired,
            snapshot,
            createdObservation,
            ingestion.Chunks,
            fetch.ETag,
            fetch.LastModified);
    }

    private async Task<OfficialSourceObservation> AppendObservationAsync(
        OfficialSynchronisationRequest request,
        OfficialSourceSnapshot snapshot,
        OfficialFetchResult fetch,
        OfficialObservationState state,
        CancellationToken cancellationToken)
    {
        var observation = CreateObservation(request, snapshot, state);
        var commit = await controlPlaneStore.AppendObservationAsync(
            new ObservationCommitRequest(
                request.ObservationAuditContext.OperationId,
                request.CorpusId,
                observation,
                request.ExpectedJournalRevision,
                request.ObservationAuditContext.RequestedAt,
                CreateObservationAuditDigest(request, snapshot, fetch)),
            cancellationToken).ConfigureAwait(false);
        EnsureApplied(commit, "official observation");
        return observation;
    }

    private async Task<OfficialSourceObservation> AppendObservationWithRebindAsync(
        OfficialSynchronisationRequest request,
        OfficialSourceSnapshot snapshot,
        OfficialFetchResult fetch,
        OfficialObservationState state,
        CancellationToken cancellationToken)
    {
        var observation = CreateObservation(request, snapshot, state);
        var commit = await controlPlaneStore.AppendObservationWithActivationRebindAsync(
            new ObservationRebindCommitRequest(
                request.ObservationAuditContext.OperationId,
                request.CorpusId,
                request.ChunkingContext.DocumentId,
                request.ChunkingContext.DocumentVersion,
                observation,
                request.ExpectedJournalRevision,
                request.ExpectedActivationRevision,
                request.ObservationAuditContext.RequestedAt,
                CreateObservationAuditDigest(request, snapshot, fetch)),
            cancellationToken).ConfigureAwait(false);

        if (commit.Outcome is not StoreMutationOutcome.Applied and
            not StoreMutationOutcome.AlreadyApplied)
        {
            throw new InvalidOperationException(
                "The official observation and any required activation rebinding were not committed.");
        }

        return observation;
    }

    private static OfficialSourceObservation CreateObservation(
        OfficialSynchronisationRequest request,
        OfficialSourceSnapshot snapshot,
        OfficialObservationState state) =>
        new(
            request.ObservationId,
            request.Registration.Id,
            snapshot.Id,
            new ObservationJournalRevision(request.ExpectedJournalRevision + 1),
            state,
            request.ObservationAuditContext.RequestedAt,
            request.MaxAge);

    private static string CreateObservationAuditDigest(
        OfficialSynchronisationRequest request,
        OfficialSourceSnapshot snapshot,
        OfficialFetchResult fetch) =>
        request.ObservationAuditContext.CreateDigest(
            request.Registration.Id.Value,
            snapshot.Id.Value,
            fetch.StatusCode.ToString(System.Globalization.CultureInfo.InvariantCulture),
            request.CurrentEtag ?? "",
            request.CurrentLastModified?.ToString(
                "O",
                System.Globalization.CultureInfo.InvariantCulture) ?? "",
            fetch.ETag ?? "",
            fetch.LastModified?.ToString(
                "O",
                System.Globalization.CultureInfo.InvariantCulture) ?? "");

    private static void ValidateRequest(OfficialSynchronisationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.CorpusId);
        ArgumentNullException.ThrowIfNull(request.Registration);
        ArgumentNullException.ThrowIfNull(request.ChunkingContext);
        ArgumentNullException.ThrowIfNull(request.ParserPolicy);
        ArgumentNullException.ThrowIfNull(request.ChunkingPolicy);
        ArgumentNullException.ThrowIfNull(request.SnapshotAuditContext);
        ArgumentNullException.ThrowIfNull(request.ObservationAuditContext);
        ArgumentNullException.ThrowIfNull(request.ObservationId);

        if (request.ExpectedJournalRevision < 0 ||
            request.ExpectedActivationRevision < 0 ||
            request.MaxAge <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(request));
        }

        if (request.MaximumByteLength != request.ParserPolicy.MaximumByteLength ||
            request.ChunkingContext.DocumentFormat != request.DocumentFormat ||
            request.ChunkingContext.SourceTrustClass != SourceTrustClass.OfficialExternal ||
            request.ChunkingContext.DatabaseProductId !=
                request.Registration.DatabaseProductId ||
            request.ChunkingContext.DocumentId != request.Registration.DocumentId ||
            request.ChunkingContext.SourceAdapterId != request.Registration.SourceAdapterId)
        {
            throw new ArgumentException(
                "Official synchronisation context does not match its trusted registration.",
                nameof(request));
        }

        if (request.CurrentSnapshot is not null &&
            request.CurrentSnapshot.RegistrationId != request.Registration.Id)
        {
            throw new ArgumentException(
                "The current snapshot does not belong to the trusted registration.",
                nameof(request));
        }
    }

    private static void ValidateResponse(
        OfficialFetchResult fetch,
        OfficialSynchronisationRequest request)
    {
        ArgumentNullException.ThrowIfNull(fetch);

        if (fetch.Content?.LongLength > request.MaximumByteLength)
        {
            throw new DocumentParseException(DocumentParseFailureKind.LimitExceeded);
        }

        if (fetch.Status == OfficialFetchStatus.Changed)
        {
            var mediaTypeAccepted = request.DocumentFormat switch
            {
                DocumentFormat.Pdf => string.Equals(
                    fetch.MediaType,
                    "application/pdf",
                    StringComparison.OrdinalIgnoreCase),
                DocumentFormat.Csv =>
                    string.Equals(fetch.MediaType, "text/csv", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(fetch.MediaType, "application/csv", StringComparison.OrdinalIgnoreCase),
                _ => false,
            };

            if (!mediaTypeAccepted)
            {
                throw new DocumentParseException(
                    DocumentParseFailureKind.UnsupportedFormat);
            }
        }
    }

    private static void EnsureApplied(StoreMutationResult result, string operation)
    {
        if (result.Outcome is not StoreMutationOutcome.Applied and
            not StoreMutationOutcome.AlreadyApplied)
        {
            throw new InvalidOperationException(
                $"The {operation} was not committed.");
        }
    }
}
