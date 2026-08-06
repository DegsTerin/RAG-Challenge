// Purpose: Defines provider-neutral persistence ports and explicit outcomes owned by Application; SQLite, filesystems, and migrations remain Infrastructure details.
using System.Collections.ObjectModel;

using RagChallenge.Application.Administration;
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.Application.Persistence;

public enum StoreMutationOutcome
{
    Applied,
    AlreadyApplied,
    RevisionConflict,
    ValidationFailed,
    NotFound,
    RetentionConflict,
}

public enum ActivationMutationKind
{
    Initial,
    Replacement,
    ObservationRebind,
    Rollback,
}

public sealed record StoreMutationResult(
    StoreMutationOutcome Outcome,
    long CurrentRevision);

public sealed class ActivationMutationResult
{
    public ActivationMutationResult(
        StoreMutationOutcome outcome,
        CorpusActivationRecord? currentRecord,
        IEnumerable<ActivationValidationFailure>? validationFailures = null)
    {
        Outcome = outcome;
        CurrentRecord = currentRecord;
        ValidationFailures = Array.AsReadOnly(
            validationFailures?.Distinct().ToArray() ??
                Array.Empty<ActivationValidationFailure>());
    }

    public StoreMutationOutcome Outcome { get; }

    public CorpusActivationRecord? CurrentRecord { get; }

    public ReadOnlyCollection<ActivationValidationFailure> ValidationFailures { get; }
}

public sealed record CatalogueCommitRequest(
    OperationId OperationId,
    CatalogueSnapshot Snapshot,
    long ExpectedCurrentRevision,
    DateTimeOffset CommittedAt,
    string? AuditDetailsDigest = null,
    AdministrationJournalCompletion? JournalCompletion = null);

public sealed record OfficialSourceCommitRequest(
    OperationId OperationId,
    CorpusId CorpusId,
    OfficialSourceRegistration Registration,
    OfficialSourceSnapshot Snapshot,
    DateTimeOffset CommittedAt,
    string? AuditDetailsDigest = null);

public sealed record OfficialSourceRegistrationCommitRequest(
    OperationId OperationId,
    CorpusId CorpusId,
    OfficialSourceRegistration Registration,
    DateTimeOffset CommittedAt,
    string? AuditDetailsDigest = null,
    AdministrationJournalCompletion? JournalCompletion = null);

public sealed record ObservationCommitRequest(
    OperationId OperationId,
    CorpusId CorpusId,
    OfficialSourceObservation Observation,
    long ExpectedJournalRevision,
    DateTimeOffset CommittedAt,
    string? AuditDetailsDigest = null);

public sealed record ObservationRebindCommitRequest(
    OperationId OperationId,
    CorpusId CorpusId,
    DocumentId DocumentId,
    DocumentVersionNumber DocumentVersion,
    OfficialSourceObservation Observation,
    long ExpectedJournalRevision,
    long ExpectedActivationRevision,
    DateTimeOffset CommittedAt,
    string? AuditDetailsDigest = null);

public sealed class ObservationRebindMutationResult
{
    public ObservationRebindMutationResult(
        StoreMutationOutcome outcome,
        long currentJournalRevision,
        CorpusActivationRecord? currentRecord,
        bool activationRecordRebound,
        IEnumerable<ActivationValidationFailure>? validationFailures = null)
    {
        Outcome = outcome;
        CurrentJournalRevision = currentJournalRevision;
        CurrentRecord = currentRecord;
        ActivationRecordRebound = activationRecordRebound;
        ValidationFailures = Array.AsReadOnly(
            validationFailures?.Distinct().ToArray() ??
                Array.Empty<ActivationValidationFailure>());
    }

    public StoreMutationOutcome Outcome { get; }

    public long CurrentJournalRevision { get; }

    public CorpusActivationRecord? CurrentRecord { get; }

    public bool ActivationRecordRebound { get; }

    public ReadOnlyCollection<ActivationValidationFailure> ValidationFailures { get; }
}

public sealed record GenerationCommitRequest(
    OperationId OperationId,
    CandidateBuildId CandidateBuildId,
    FinalisedIndexGenerationManifest Manifest,
    IReadOnlyCollection<DocumentBinding> Bindings,
    DateTimeOffset FinalisedAt,
    string? AuditDetailsDigest = null);

public sealed record ActivationCompareExchangeRequest(
    OperationId OperationId,
    ActivationMutationKind MutationKind,
    long ExpectedCurrentRevision,
    CorpusActivationRecord ProposedRecord,
    IndexCompatibilityKey RequiredCompatibilityKey,
    DateTimeOffset EvaluatedAt,
    TimeSpan PreviousGenerationRetention,
    string? AuditDetailsDigest = null,
    AdministrationJournalCompletion? JournalCompletion = null);

public interface IControlPlaneStore
{
    Task<StoreMutationResult> CommitCatalogueAsync(
        CatalogueCommitRequest request,
        CancellationToken cancellationToken = default);

    Task<CatalogueSnapshot?> ReadCurrentCatalogueAsync(
        CorpusId corpusId,
        CancellationToken cancellationToken = default);

    Task<StoreMutationResult> RegisterOfficialSourceAsync(
        OfficialSourceRegistrationCommitRequest request,
        CancellationToken cancellationToken = default);

    Task<StoreMutationResult> CommitOfficialSourceAsync(
        OfficialSourceCommitRequest request,
        CancellationToken cancellationToken = default);

    Task<StoreMutationResult> AppendObservationAsync(
        ObservationCommitRequest request,
        CancellationToken cancellationToken = default);

    Task<ObservationRebindMutationResult> AppendObservationWithActivationRebindAsync(
        ObservationRebindCommitRequest request,
        CancellationToken cancellationToken = default);

    Task<StoreMutationResult> CommitGenerationAsync(
        GenerationCommitRequest request,
        CancellationToken cancellationToken = default);

    Task<ActivationMutationResult> CompareExchangeActivationAsync(
        ActivationCompareExchangeRequest request,
        CancellationToken cancellationToken = default);

    Task<CorpusActivationRecord?> ReadActiveActivationAsync(
        CorpusId corpusId,
        CancellationToken cancellationToken = default);
}

public sealed record ContentWriteResult(
    ContentObjectId ContentObjectId,
    long ByteLength,
    bool AlreadyExisted);

public interface IImmutableContentStore
{
    Task<ContentWriteResult> PutAsync(
        Stream content,
        long maximumByteLength,
        ContentObjectId? expectedContentObjectId = null,
        CancellationToken cancellationToken = default);

    ValueTask<Stream> OpenReadAsync(
        ContentObjectId contentObjectId,
        CancellationToken cancellationToken = default);
}

public sealed record VectorChunkWrite(
    long ChunkOrdinal,
    DocumentId DocumentId,
    DocumentVersionNumber DocumentVersion,
    LogicalArtifactDigest ChunkDigest,
    string ChunkText,
    ReadOnlyMemory<float> Vector,
    SupportedLanguage? ContentLanguage = null,
    int? PageNumber = null,
    long? RecordNumber = null,
    IReadOnlyDictionary<string, string>? Columns = null);

public sealed record VectorSearchBindingSelector(
    DatabaseProductId DatabaseProductId,
    DatabaseProductRevision DatabaseProductRevision,
    DocumentId DocumentId,
    DocumentVersionNumber DocumentVersion,
    DocumentFormat DocumentFormat,
    SourceAdapterId SourceAdapterId,
    SourceTrustClass SourceTrustClass,
    OfficialSourceRegistrationId? OfficialSourceRegistrationId,
    OfficialSnapshotId? OfficialSnapshotId)
{
    public static VectorSearchBindingSelector FromBinding(DocumentBinding binding)
    {
        ArgumentNullException.ThrowIfNull(binding);
        return new VectorSearchBindingSelector(
            binding.DatabaseProductId,
            binding.DatabaseProductRevision,
            binding.DocumentId,
            binding.DocumentVersion,
            binding.DocumentFormat,
            binding.SourceAdapterId,
            binding.SourceTrustClass,
            binding.OfficialSourceRegistrationId,
            binding.OfficialSnapshotId);
    }
}

public sealed record VectorSearchHit(
    CandidateBuildId CandidateBuildId,
    CorpusId CorpusId,
    IndexGenerationId IndexGenerationId,
    VectorSearchBindingSelector BindingSelector,
    long ChunkOrdinal,
    LogicalArtifactDigest ChunkDigest,
    string ChunkText,
    double Score,
    SupportedLanguage? ContentLanguage,
    int? PageNumber,
    long? RecordNumber,
    IReadOnlyDictionary<string, string> Columns)
{
    public DocumentId DocumentId => BindingSelector.DocumentId;

    public DocumentVersionNumber DocumentVersion => BindingSelector.DocumentVersion;
}

public sealed class VectorSearchRequest
{
    public VectorSearchRequest(
        CorpusId corpusId,
        IndexGenerationId indexGenerationId,
        ReadOnlyMemory<float> queryVector,
        int maximumResults,
        IReadOnlyCollection<VectorSearchBindingSelector> eligibleSelectors,
        IReadOnlyCollection<DatabaseProductId>? databaseProductFilters = null,
        IReadOnlyCollection<DocumentId>? documentFilters = null)
    {
        CorpusId = corpusId ?? throw new ArgumentNullException(nameof(corpusId));
        IndexGenerationId = indexGenerationId ??
            throw new ArgumentNullException(nameof(indexGenerationId));

        if (queryVector.IsEmpty)
        {
            throw new ArgumentException(
                "A vector search request requires a query vector.",
                nameof(queryVector));
        }

        if (maximumResults is <= 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumResults));
        }

        ArgumentNullException.ThrowIfNull(eligibleSelectors);

        if (eligibleSelectors.Count == 0)
        {
            throw new ArgumentException(
                "A vector search request requires generation-bound eligible documents.",
                nameof(eligibleSelectors));
        }

        var materialisedSelectors = eligibleSelectors.ToArray();

        if (materialisedSelectors.Any(selector => !IsValidSelector(selector)) ||
            materialisedSelectors
                .Select(selector => (selector.DocumentId, selector.DocumentVersion))
                .Distinct()
                .Count() != materialisedSelectors.Length)
        {
            throw new ArgumentException(
                "Eligible vector selectors must be valid and unique by document version.",
                nameof(eligibleSelectors));
        }

        QueryVector = queryVector.ToArray();
        MaximumResults = maximumResults;
        EligibleSelectors = Array.AsReadOnly(materialisedSelectors);
        DatabaseProductFilters = Array.AsReadOnly(
            databaseProductFilters?.Distinct().ToArray() ?? []);
        DocumentFilters = Array.AsReadOnly(
            documentFilters?.Distinct().ToArray() ?? []);
    }

    public CorpusId CorpusId { get; }

    public IndexGenerationId IndexGenerationId { get; }

    public ReadOnlyMemory<float> QueryVector { get; }

    public int MaximumResults { get; }

    public ReadOnlyCollection<VectorSearchBindingSelector> EligibleSelectors { get; }

    public ReadOnlyCollection<DatabaseProductId> DatabaseProductFilters { get; }

    public ReadOnlyCollection<DocumentId> DocumentFilters { get; }

    private static bool IsValidSelector(VectorSearchBindingSelector? selector) =>
        selector is not null &&
        selector.DatabaseProductId is not null &&
        selector.DatabaseProductRevision is not null &&
        selector.DocumentId is not null &&
        selector.DocumentVersion is not null &&
        Enum.IsDefined(selector.DocumentFormat) &&
        selector.SourceAdapterId is not null &&
        Enum.IsDefined(selector.SourceTrustClass) &&
        (selector.SourceTrustClass == SourceTrustClass.LocalAuthorised
            ? selector.OfficialSourceRegistrationId is null &&
                selector.OfficialSnapshotId is null
            : selector.OfficialSourceRegistrationId is not null &&
                selector.OfficialSnapshotId is not null);
}

public interface IVectorIndexStore
{
    Task CreateCandidateAsync(
        CandidateBuildId candidateBuildId,
        CorpusId corpusId,
        IndexCompatibilityKey indexCompatibilityKey,
        int vectorDimensions,
        long expectedChunkCount,
        DateTimeOffset createdAt,
        CancellationToken cancellationToken = default);

    Task AddChunksAsync(
        CandidateBuildId candidateBuildId,
        IReadOnlyCollection<VectorChunkWrite> chunks,
        CancellationToken cancellationToken = default);

    Task<FinalisedIndexGenerationManifest> FinaliseCandidateAsync(
        CandidateBuildId candidateBuildId,
        IndexGenerationSpecification specification,
        DateTimeOffset validatedAt,
        CancellationToken cancellationToken = default);

    Task MarkFailedAsync(
        CandidateBuildId candidateBuildId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VectorSearchHit>> SearchExactAsync(
        VectorSearchRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record StorageCleanupResult(
    OperationId OperationId,
    int RemovedVectorGenerations,
    int RemovedContentObjects,
    bool AlreadyApplied);

public interface IStorageMaintenance
{
    Task<StorageCleanupResult> RunManualCleanupAsync(
        OperationId operationId,
        CorpusId corpusId,
        DateTimeOffset requestedAt,
        CancellationToken cancellationToken = default);
}
