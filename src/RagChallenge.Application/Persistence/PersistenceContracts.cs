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

public sealed record RenderManifestCommitRequest(
    CorpusId CorpusId,
    DocumentRenderManifest Manifest,
    DerivativeObligationSetV1? ObligationSet = null);

public sealed record RenderManifestCommitResult(
    StoreMutationOutcome Outcome,
    DocumentRenderManifest? CurrentManifest);

public interface IDocumentRenderManifestStore
{
    Task<RenderManifestCommitResult> CommitAsync(
        RenderManifestCommitRequest request,
        CancellationToken cancellationToken = default);

    Task<DocumentRenderManifest?> ReadAsync(
        CorpusId corpusId,
        RenderManifestId renderManifestId,
        CancellationToken cancellationToken = default);

    Task<DerivativeObligationSetV1?> ReadObligationSetAsync(
        CorpusId corpusId,
        DerivativeObligationSetId obligationSetId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<DerivativeObligationSetV1?>(null);
}

public sealed record ContentMediaType
{
    private const int MaximumLength = 127;

    public static ContentMediaType ApplicationPdf { get; } = new("application/pdf");

    public static ContentMediaType ApplicationCsv { get; } = new("application/csv");

    public static ContentMediaType ApplicationOctetStream { get; } =
        new("application/octet-stream");

    public static ContentMediaType ImagePng { get; } = new("image/png");

    public static ContentMediaType TextCsv { get; } = new("text/csv");

    public ContentMediaType(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Length is 0 or > MaximumLength ||
            value.Any(character => character > 0x7f))
        {
            throw new ArgumentException(
                "A media type must be bounded non-empty ASCII.",
                nameof(value));
        }

        var separator = value.IndexOf('/');

        if (separator <= 0 ||
            separator != value.LastIndexOf('/') ||
            separator == value.Length - 1 ||
            !value[..separator].All(IsTokenCharacter) ||
            !value[(separator + 1)..].All(IsTokenCharacter))
        {
            throw new ArgumentException(
                "A media type must contain one concrete type/subtype token without parameters.",
                nameof(value));
        }

        Value = value.ToLowerInvariant();
    }

    public string Value { get; }

    public override string ToString() => Value;

    private static bool IsTokenCharacter(char character) =>
        character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' ||
        character is '!' or '#' or '$' or '%' or '&' or '\'' or '+' or '-' or '.' or
            '^' or '_' or '`' or '|' or '~';
}

public sealed record ContentStoreImplementationDescriptor
{
    private const int MaximumLength = 128;

    public ContentStoreImplementationDescriptor(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Length is 0 or > MaximumLength ||
            !char.IsAsciiLetterOrDigit(value[0]) ||
            value.Any(character =>
                !char.IsAsciiLetterOrDigit(character) &&
                character is not ('.' or '_' or ':' or '-')))
        {
            throw new ArgumentException(
                "A content-store descriptor must be a stable non-secret ASCII identifier.",
                nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public enum ContentObjectWriteOutcome
{
    Published,
    AlreadyExisted,
}

public enum ContentVerificationOutcome
{
    Verified,
}

public sealed class ContentObjectVerificationResult
{
    public ContentObjectVerificationResult(
        ContentVerificationOutcome writeVerification,
        ContentVerificationOutcome reopenVerification)
    {
        if (!Enum.IsDefined(writeVerification))
        {
            throw new ArgumentOutOfRangeException(nameof(writeVerification));
        }

        if (!Enum.IsDefined(reopenVerification))
        {
            throw new ArgumentOutOfRangeException(nameof(reopenVerification));
        }

        WriteVerification = writeVerification;
        ReopenVerification = reopenVerification;
    }

    public ContentVerificationOutcome WriteVerification { get; }

    public ContentVerificationOutcome ReopenVerification { get; }
}

public sealed class ContentObjectDescriptor
{
    public ContentObjectDescriptor(
        ContentObjectId contentObjectId,
        ContentObjectId sha256,
        long byteLength,
        ContentMediaType mediaType,
        ContentStoreImplementationDescriptor implementation,
        ContentObjectWriteOutcome writeOutcome,
        ContentObjectVerificationResult verification)
    {
        ArgumentNullException.ThrowIfNull(contentObjectId);
        ArgumentNullException.ThrowIfNull(sha256);
        ArgumentNullException.ThrowIfNull(mediaType);
        ArgumentNullException.ThrowIfNull(implementation);
        ArgumentNullException.ThrowIfNull(verification);

        if (contentObjectId != sha256)
        {
            throw new ArgumentException(
                "The content-object identity must equal the verified SHA-256.",
                nameof(sha256));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(byteLength);

        if (!Enum.IsDefined(writeOutcome))
        {
            throw new ArgumentOutOfRangeException(nameof(writeOutcome));
        }

        ContentObjectId = contentObjectId;
        Sha256 = sha256;
        ByteLength = byteLength;
        MediaType = mediaType;
        Implementation = implementation;
        WriteOutcome = writeOutcome;
        Verification = verification;
    }

    public ContentObjectId ContentObjectId { get; }

    public ContentObjectId Sha256 { get; }

    public long ByteLength { get; }

    public ContentMediaType MediaType { get; }

    public ContentStoreImplementationDescriptor Implementation { get; }

    public ContentObjectWriteOutcome WriteOutcome { get; }

    public ContentObjectVerificationResult Verification { get; }
}

public sealed class BoundedContentInput
{
    public BoundedContentInput(
        Stream content,
        long maximumByteLength,
        ContentMediaType mediaType,
        ContentObjectId? expectedContentObjectId = null)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(mediaType);

        if (!content.CanRead)
        {
            throw new ArgumentException("Content must be readable.", nameof(content));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumByteLength);
        Content = content;
        MaximumByteLength = maximumByteLength;
        MediaType = mediaType;
        ExpectedContentObjectId = expectedContentObjectId;
    }

    public Stream Content { get; }

    public long MaximumByteLength { get; }

    public ContentMediaType MediaType { get; }

    public ContentObjectId? ExpectedContentObjectId { get; }
}

public sealed class ExpectedHashAndLength
{
    public ExpectedHashAndLength(ContentObjectId sha256, long byteLength)
    {
        ArgumentNullException.ThrowIfNull(sha256);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(byteLength);
        Sha256 = sha256;
        ByteLength = byteLength;
    }

    public ContentObjectId Sha256 { get; }

    public long ByteLength { get; }
}

public sealed class VerifiedContentObject : IAsyncDisposable
{
    public VerifiedContentObject(
        ContentObjectId contentObjectId,
        ContentObjectId sha256,
        long byteLength,
        Stream content,
        ContentVerificationOutcome reopenVerification)
    {
        ArgumentNullException.ThrowIfNull(contentObjectId);
        ArgumentNullException.ThrowIfNull(sha256);
        ArgumentNullException.ThrowIfNull(content);

        if (contentObjectId != sha256)
        {
            throw new ArgumentException(
                "The reopened content identity must equal its verified SHA-256.",
                nameof(sha256));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(byteLength);

        if (!content.CanRead || !content.CanSeek || content.Position != 0)
        {
            throw new ArgumentException(
                "Verified content must be readable, seekable and positioned at zero.",
                nameof(content));
        }

        if (!Enum.IsDefined(reopenVerification))
        {
            throw new ArgumentOutOfRangeException(nameof(reopenVerification));
        }

        ContentObjectId = contentObjectId;
        Sha256 = sha256;
        ByteLength = byteLength;
        Content = content;
        ReopenVerification = reopenVerification;
    }

    public ContentObjectId ContentObjectId { get; }

    public ContentObjectId Sha256 { get; }

    public long ByteLength { get; }

    public Stream Content { get; }

    public ContentVerificationOutcome ReopenVerification { get; }

    public ValueTask DisposeAsync() => Content.DisposeAsync();
}

public interface IDocumentContentStore
{
    Task<ContentObjectDescriptor> PutAndVerifyAsync(
        BoundedContentInput input,
        CancellationToken cancellationToken = default);

    ValueTask<VerifiedContentObject> OpenVerifiedAsync(
        ContentObjectId contentObjectId,
        ExpectedHashAndLength expected,
        CancellationToken cancellationToken = default);
}

public sealed record VectorChunkWrite(
    long ChunkOrdinal,
    DocumentId DocumentId,
    DocumentVersionNumber DocumentVersion,
    LogicalArtifactDigest ChunkDigest,
    string ChunkText,
    ReadOnlyMemory<float> Vector,
    DocumentContentLanguage? ContentLanguage = null,
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
    DocumentContentLanguage? ContentLanguage,
    int? PageNumber,
    long? RecordNumber,
    IReadOnlyDictionary<string, string> Columns)
{
    public DocumentId DocumentId => BindingSelector.DocumentId;

    public DocumentVersionNumber DocumentVersion => BindingSelector.DocumentVersion;
}

public enum VectorSearchOutcome
{
    Succeeded,
    InvalidQueryVector,
    GenerationUnavailable,
    InvalidIndexData,
    ContractViolation,
    OperationCancelled,
    UnexpectedFailure,
}

public sealed class VectorSearchResult
{
    private VectorSearchResult(
        VectorSearchOutcome outcome,
        IEnumerable<VectorSearchHit>? hits,
        string? failureIdentity)
    {
        var materialised = hits?.ToArray() ?? [];

        if (!Enum.IsDefined(outcome) ||
            (outcome == VectorSearchOutcome.Succeeded) != (failureIdentity is null) ||
            outcome != VectorSearchOutcome.Succeeded && materialised.Length != 0 ||
            failureIdentity is not null && string.IsNullOrWhiteSpace(failureIdentity))
        {
            throw new ArgumentException(
                "A vector-search result must contain either successful hits or one sanitised failure identity.");
        }

        Outcome = outcome;
        Hits = Array.AsReadOnly(materialised);
        FailureIdentity = failureIdentity;
    }

    public VectorSearchOutcome Outcome { get; }

    public ReadOnlyCollection<VectorSearchHit> Hits { get; }

    public string? FailureIdentity { get; }

    public static VectorSearchResult Successful(IEnumerable<VectorSearchHit> hits)
    {
        ArgumentNullException.ThrowIfNull(hits);
        return new VectorSearchResult(VectorSearchOutcome.Succeeded, hits, failureIdentity: null);
    }

    public static VectorSearchResult Failed(VectorSearchOutcome outcome)
    {
        if (outcome == VectorSearchOutcome.Succeeded)
        {
            throw new ArgumentOutOfRangeException(nameof(outcome));
        }

        return new VectorSearchResult(
            outcome,
            hits: null,
            $"VECTOR_SEARCH_{ToUpperSnakeCase(outcome.ToString())}");
    }

    private static string ToUpperSnakeCase(string value)
    {
        var result = new System.Text.StringBuilder(value.Length + 8);

        foreach (var character in value)
        {
            if (char.IsUpper(character) && result.Length != 0)
            {
                result.Append('_');
            }

            result.Append(char.ToUpperInvariant(character));
        }

        return result.ToString();
    }
}

public sealed class VectorSearchRequest
{
    public VectorSearchRequest(
        CorpusId corpusId,
        IndexGenerationId indexGenerationId,
        IndexCompatibilityKey expectedIndexCompatibilityKey,
        ReadOnlyMemory<float> queryVector,
        int maximumResults,
        IReadOnlyCollection<VectorSearchBindingSelector> eligibleSelectors,
        IReadOnlyCollection<DatabaseProductId>? databaseProductFilters = null,
        IReadOnlyCollection<DocumentId>? documentFilters = null)
    {
        CorpusId = corpusId ?? throw new ArgumentNullException(nameof(corpusId));
        IndexGenerationId = indexGenerationId ??
            throw new ArgumentNullException(nameof(indexGenerationId));
        ExpectedIndexCompatibilityKey = expectedIndexCompatibilityKey ??
            throw new ArgumentNullException(nameof(expectedIndexCompatibilityKey));

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

    public IndexCompatibilityKey ExpectedIndexCompatibilityKey { get; }

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

    Task<VectorSearchResult> SearchExactAsync(
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
