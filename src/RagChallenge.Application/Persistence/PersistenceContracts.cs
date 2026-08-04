// Purpose: Defines provider-neutral persistence ports and explicit outcomes owned by Application; SQLite, filesystems, and migrations remain Infrastructure details.
using System.Collections.ObjectModel;

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
    string? AuditDetailsDigest = null);

public sealed record OfficialSourceCommitRequest(
    OperationId OperationId,
    CorpusId CorpusId,
    OfficialSourceRegistration Registration,
    OfficialSourceSnapshot Snapshot,
    DateTimeOffset CommittedAt,
    string? AuditDetailsDigest = null);

public sealed record ObservationCommitRequest(
    OperationId OperationId,
    CorpusId CorpusId,
    OfficialSourceObservation Observation,
    long ExpectedJournalRevision,
    DateTimeOffset CommittedAt,
    string? AuditDetailsDigest = null);

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
    string? AuditDetailsDigest = null);

public interface IControlPlaneStore
{
    Task<StoreMutationResult> CommitCatalogueAsync(
        CatalogueCommitRequest request,
        CancellationToken cancellationToken = default);

    Task<StoreMutationResult> CommitOfficialSourceAsync(
        OfficialSourceCommitRequest request,
        CancellationToken cancellationToken = default);

    Task<StoreMutationResult> AppendObservationAsync(
        ObservationCommitRequest request,
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
    ReadOnlyMemory<float> Vector);

public sealed record VectorSearchHit(
    CandidateBuildId CandidateBuildId,
    long ChunkOrdinal,
    DocumentId DocumentId,
    DocumentVersionNumber DocumentVersion,
    LogicalArtifactDigest ChunkDigest,
    string ChunkText,
    double Score);

public sealed class VectorSearchRequest
{
    public VectorSearchRequest(
        CorpusId corpusId,
        IndexGenerationId indexGenerationId,
        ReadOnlyMemory<float> queryVector,
        int maximumResults,
        IReadOnlyCollection<DocumentBinding> eligibleBindings,
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

        ArgumentNullException.ThrowIfNull(eligibleBindings);

        if (eligibleBindings.Count == 0)
        {
            throw new ArgumentException(
                "A vector search request requires generation-bound eligible documents.",
                nameof(eligibleBindings));
        }

        QueryVector = queryVector.ToArray();
        MaximumResults = maximumResults;
        EligibleBindings = Array.AsReadOnly(eligibleBindings.ToArray());
        DatabaseProductFilters = Array.AsReadOnly(
            databaseProductFilters?.Distinct().ToArray() ?? []);
        DocumentFilters = Array.AsReadOnly(
            documentFilters?.Distinct().ToArray() ?? []);
    }

    public CorpusId CorpusId { get; }

    public IndexGenerationId IndexGenerationId { get; }

    public ReadOnlyMemory<float> QueryVector { get; }

    public int MaximumResults { get; }

    public ReadOnlyCollection<DocumentBinding> EligibleBindings { get; }

    public ReadOnlyCollection<DatabaseProductId> DatabaseProductFilters { get; }

    public ReadOnlyCollection<DocumentId> DocumentFilters { get; }
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
