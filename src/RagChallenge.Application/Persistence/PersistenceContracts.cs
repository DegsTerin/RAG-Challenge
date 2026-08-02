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
            validationFailures?.Distinct().ToArray() ?? []);
    }

    public StoreMutationOutcome Outcome { get; }

    public CorpusActivationRecord? CurrentRecord { get; }

    public ReadOnlyCollection<ActivationValidationFailure> ValidationFailures { get; }
}

public sealed record CatalogueCommitRequest(
    OperationId OperationId,
    CatalogueSnapshot Snapshot,
    long ExpectedCurrentRevision,
    DateTimeOffset CommittedAt);

public sealed record OfficialSourceCommitRequest(
    OperationId OperationId,
    CorpusId CorpusId,
    OfficialSourceRegistration Registration,
    OfficialSourceSnapshot Snapshot,
    DateTimeOffset CommittedAt);

public sealed record ObservationCommitRequest(
    OperationId OperationId,
    CorpusId CorpusId,
    OfficialSourceObservation Observation,
    long ExpectedJournalRevision,
    DateTimeOffset CommittedAt);

public sealed record GenerationCommitRequest(
    OperationId OperationId,
    CandidateBuildId CandidateBuildId,
    FinalisedIndexGenerationManifest Manifest,
    IReadOnlyCollection<DocumentBinding> Bindings,
    DateTimeOffset FinalisedAt);

public sealed record ActivationCompareExchangeRequest(
    OperationId OperationId,
    ActivationMutationKind MutationKind,
    long ExpectedCurrentRevision,
    CorpusActivationRecord ProposedRecord,
    IndexCompatibilityKey RequiredCompatibilityKey,
    DateTimeOffset EvaluatedAt,
    TimeSpan PreviousGenerationRetention);

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
        IndexGenerationId indexGenerationId,
        ReadOnlyMemory<float> queryVector,
        int maximumResults,
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
