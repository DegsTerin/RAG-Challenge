// Purpose: Orchestrates provider-neutral embedding, immutable candidate finalisation and explicit activation; provider and persistence implementations remain outer-layer concerns.
using System.Collections.ObjectModel;
using System.Text;

using RagChallenge.Application.Administration;
using RagChallenge.Application.Documents;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.Application.IndexingRetrieval;

public sealed record EmbeddingProviderDescriptor
{
    public EmbeddingProviderDescriptor(
        string providerId,
        string modelId,
        string modelRevision,
        int dimensions)
    {
        ProviderId = RequireDescriptorValue(providerId, nameof(providerId));
        ModelId = RequireDescriptorValue(modelId, nameof(modelId));
        ModelRevision = RequireDescriptorValue(modelRevision, nameof(modelRevision));

        if (dimensions is <= 0 or > 8192)
        {
            throw new ArgumentOutOfRangeException(nameof(dimensions));
        }

        Dimensions = dimensions;
    }

    public string ProviderId { get; }

    public string ModelId { get; }

    public string ModelRevision { get; }

    public int Dimensions { get; }

    private static string RequireDescriptorValue(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);

        if (value.Length > 128 || value.Any(character =>
                !char.IsAsciiLetterOrDigit(character) &&
                character is not '.' and not '_' and not ':' and not '-'))
        {
            throw new ArgumentException(
                "An embedding descriptor value must be bounded safe ASCII.",
                parameterName);
        }

        return value;
    }
}

public sealed class EmbeddingBatchRequest
{
    public EmbeddingBatchRequest(
        EmbeddingProviderDescriptor expectedDescriptor,
        IReadOnlyCollection<string> inputs,
        int maximumUtf8Bytes)
    {
        ExpectedDescriptor = expectedDescriptor ??
            throw new ArgumentNullException(nameof(expectedDescriptor));
        ArgumentNullException.ThrowIfNull(inputs);

        if (inputs.Count is <= 0 or > 512)
        {
            throw new ArgumentOutOfRangeException(nameof(inputs));
        }

        if (maximumUtf8Bytes is <= 0 or > 4 * 1024 * 1024)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumUtf8Bytes));
        }

        var materialised = inputs.ToArray();

        if (materialised.Any(string.IsNullOrWhiteSpace) ||
            materialised.Sum(value => (long)Encoding.UTF8.GetByteCount(value)) > maximumUtf8Bytes)
        {
            throw new ArgumentException(
                "An embedding batch must contain bounded non-empty UTF-8 inputs.",
                nameof(inputs));
        }

        Inputs = Array.AsReadOnly(materialised);
        MaximumUtf8Bytes = maximumUtf8Bytes;
    }

    public EmbeddingProviderDescriptor ExpectedDescriptor { get; }

    public ReadOnlyCollection<string> Inputs { get; }

    public int MaximumUtf8Bytes { get; }
}

public sealed class EmbeddingBatchResult
{
    public EmbeddingBatchResult(
        EmbeddingProviderDescriptor observedDescriptor,
        IReadOnlyCollection<ReadOnlyMemory<float>> vectors)
    {
        ObservedDescriptor = observedDescriptor ??
            throw new ArgumentNullException(nameof(observedDescriptor));
        ArgumentNullException.ThrowIfNull(vectors);
        Vectors = Array.AsReadOnly(vectors.Select(vector =>
            (ReadOnlyMemory<float>)vector.ToArray()).ToArray());
    }

    public EmbeddingProviderDescriptor ObservedDescriptor { get; }

    public ReadOnlyCollection<ReadOnlyMemory<float>> Vectors { get; }
}

public interface IEmbeddingProvider
{
    Task<EmbeddingBatchResult> EmbedAsync(
        EmbeddingBatchRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class IndexDocumentInput
{
    public IndexDocumentInput(
        DocumentBinding binding,
        DocumentContentLanguage contentLanguage,
        IReadOnlyCollection<DocumentChunk> chunks,
        string parserDescriptor,
        ChunkingPolicy chunkingPolicy)
    {
        Binding = binding ?? throw new ArgumentNullException(nameof(binding));

        ArgumentNullException.ThrowIfNull(contentLanguage);

        if (!contentLanguage.IsSupportedByV1)
        {
            throw new ArgumentException(
                "Runtime v1 cannot index a document outside its closed query-language set.",
                nameof(contentLanguage));
        }

        ContentLanguage = contentLanguage;
        ArgumentNullException.ThrowIfNull(chunks);

        if (chunks.Count == 0)
        {
            throw new ArgumentException(
                "An indexed document requires at least one chunk.",
                nameof(chunks));
        }

        Chunks = Array.AsReadOnly(chunks.OrderBy(chunk => chunk.Ordinal).ToArray());
        ArgumentException.ThrowIfNullOrWhiteSpace(parserDescriptor);
        ParserDescriptor = parserDescriptor;
        ChunkingPolicy = chunkingPolicy ??
            throw new ArgumentNullException(nameof(chunkingPolicy));
    }

    public DocumentBinding Binding { get; }

    public DocumentContentLanguage ContentLanguage { get; }

    public ReadOnlyCollection<DocumentChunk> Chunks { get; }

    public string ParserDescriptor { get; }

    public ChunkingPolicy ChunkingPolicy { get; }
}

public sealed record CorpusIndexingRequest(
    CandidateBuildId CandidateBuildId,
    IndexGenerationSpecification Specification,
    IReadOnlyCollection<IndexDocumentInput> Documents,
    EmbeddingProviderDescriptor ExpectedEmbeddingDescriptor,
    IndexCompatibilityProfile CompatibilityProfile,
    AdministrativeAuditContext AuditContext,
    DateTimeOffset ValidatedAt,
    int MaximumEmbeddingBatchUtf8Bytes = 1_048_576);

public sealed record CorpusIndexingResult(
    FinalisedIndexGenerationManifest Manifest,
    StoreMutationResult CommitResult);

public sealed class CorpusIndexingService(
    IEmbeddingProvider embeddingProvider,
    IVectorIndexStore vectorStore,
    IControlPlaneStore controlPlaneStore)
{
    private const int MaximumEmbeddingBatchInputs = 256;
    private const int MaximumVectorWriteBatch = 500;

    private readonly IEmbeddingProvider embeddingProvider = embeddingProvider ??
        throw new ArgumentNullException(nameof(embeddingProvider));
    private readonly IVectorIndexStore vectorStore = vectorStore ??
        throw new ArgumentNullException(nameof(vectorStore));
    private readonly IControlPlaneStore controlPlaneStore = controlPlaneStore ??
        throw new ArgumentNullException(nameof(controlPlaneStore));

    public async Task<CorpusIndexingResult> BuildAsync(
        CorpusIndexingRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);
        var orderedDocuments = request.Documents
            .OrderBy(document => document.Binding.DatabaseProductId.Value, StringComparer.Ordinal)
            .ThenBy(document => document.Binding.DocumentId.Value, StringComparer.Ordinal)
            .ThenBy(document => document.Binding.DocumentVersion.Value)
            .ToArray();
        var bindings = orderedDocuments.Select(document => document.Binding).ToArray();
        var flattened = orderedDocuments
            .SelectMany(document => document.Chunks.Select(chunk =>
                (Document: document, document.Binding, Chunk: chunk)))
            .ToArray();

        await vectorStore.CreateCandidateAsync(
            request.CandidateBuildId,
            request.Specification.CorpusId,
            request.Specification.IndexCompatibilityKey,
            request.ExpectedEmbeddingDescriptor.Dimensions,
            flattened.LongLength,
            request.AuditContext.RequestedAt,
            cancellationToken).ConfigureAwait(false);

        var finalised = false;

        try
        {
            var vectors = await EmbedAsync(request, flattened, cancellationToken)
                .ConfigureAwait(false);
            var writes = flattened.Select((item, index) => new VectorChunkWrite(
                index,
                item.Binding.DocumentId,
                item.Binding.DocumentVersion,
                item.Chunk.Digest,
                item.Chunk.Text,
                vectors[index],
                item.Document.ContentLanguage,
                item.Chunk.PageNumber,
                item.Chunk.RecordNumber,
                item.Chunk.Columns)).ToArray();

            foreach (var batch in writes.Chunk(MaximumVectorWriteBatch))
            {
                await vectorStore.AddChunksAsync(
                    request.CandidateBuildId,
                    batch,
                    cancellationToken).ConfigureAwait(false);
            }

            var manifest = await vectorStore.FinaliseCandidateAsync(
                request.CandidateBuildId,
                request.Specification,
                request.ValidatedAt,
                cancellationToken).ConfigureAwait(false);
            finalised = true;
            var commit = await controlPlaneStore.CommitGenerationAsync(
                new GenerationCommitRequest(
                    request.AuditContext.OperationId,
                    request.CandidateBuildId,
                    manifest,
                    bindings,
                    request.ValidatedAt,
                    request.AuditContext.CreateDigest(
                        manifest.IndexGenerationId.Value,
                        manifest.GenerationContentDigest.Value,
                        request.ExpectedEmbeddingDescriptor.ProviderId,
                        request.ExpectedEmbeddingDescriptor.ModelId,
                        request.ExpectedEmbeddingDescriptor.ModelRevision,
                        request.ExpectedEmbeddingDescriptor.Dimensions.ToString(
                            System.Globalization.CultureInfo.InvariantCulture))),
                cancellationToken).ConfigureAwait(false);

            if (commit.Outcome is not StoreMutationOutcome.Applied and
                not StoreMutationOutcome.AlreadyApplied)
            {
                throw new InvalidOperationException(
                    $"Generation commit failed with {commit.Outcome}.");
            }

            return new CorpusIndexingResult(manifest, commit);
        }
        catch
        {
            if (!finalised)
            {
                await vectorStore.MarkFailedAsync(request.CandidateBuildId, cancellationToken)
                    .ConfigureAwait(false);
            }

            throw;
        }
    }

    private async Task<ReadOnlyMemory<float>[]> EmbedAsync(
        CorpusIndexingRequest request,
        (IndexDocumentInput Document, DocumentBinding Binding, DocumentChunk Chunk)[] flattened,
        CancellationToken cancellationToken)
    {
        var vectors = new List<ReadOnlyMemory<float>>(flattened.Length);

        foreach (var batch in flattened.Chunk(MaximumEmbeddingBatchInputs))
        {
            var embeddingRequest = new EmbeddingBatchRequest(
                request.ExpectedEmbeddingDescriptor,
                batch.Select(item => item.Chunk.Text).ToArray(),
                request.MaximumEmbeddingBatchUtf8Bytes);
            var result = await embeddingProvider.EmbedAsync(
                embeddingRequest,
                cancellationToken).ConfigureAwait(false);
            ValidateEmbeddingResult(embeddingRequest, result);
            vectors.AddRange(result.Vectors);
        }

        return vectors.ToArray();
    }

    private static void ValidateEmbeddingResult(
        EmbeddingBatchRequest request,
        EmbeddingBatchResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.ObservedDescriptor != request.ExpectedDescriptor ||
            result.Vectors.Count != request.Inputs.Count ||
            result.Vectors.Any(vector =>
                vector.Length != request.ExpectedDescriptor.Dimensions ||
                ContainsNonFinite(vector.Span)))
        {
            throw new InvalidDataException(
                "The embedding response descriptor, order, count, dimensions, or values diverged.");
        }
    }

    private static void ValidateRequest(CorpusIndexingRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.CandidateBuildId);
        ArgumentNullException.ThrowIfNull(request.Specification);
        ArgumentNullException.ThrowIfNull(request.Documents);
        ArgumentNullException.ThrowIfNull(request.ExpectedEmbeddingDescriptor);
        ArgumentNullException.ThrowIfNull(request.CompatibilityProfile);
        ArgumentNullException.ThrowIfNull(request.AuditContext);

        if (request.ValidatedAt.Offset != TimeSpan.Zero ||
            request.ValidatedAt < request.AuditContext.RequestedAt ||
            request.Documents.Count == 0 ||
            request.ExpectedEmbeddingDescriptor !=
                request.CompatibilityProfile.EmbeddingDescriptor ||
            request.Specification.IndexCompatibilityKey !=
                request.CompatibilityProfile.Key)
        {
            throw new ArgumentException(
                "An indexing request requires ordered UTC instants, documents and an exact compatibility profile.",
                nameof(request));
        }

        var bindings = request.Documents.Select(document => document.Binding).ToArray();

        if (bindings.Select(binding => (binding.DocumentId, binding.DocumentVersion))
                .Distinct().Count() != bindings.Length ||
            request.Documents.Any(document =>
                !request.CompatibilityProfile.ParserDescriptors.Contains(
                    document.ParserDescriptor,
                    StringComparer.Ordinal) ||
                !string.Equals(
                    document.ChunkingPolicy.CompatibilityDescriptor,
                    request.CompatibilityProfile.ChunkingPolicy.CompatibilityDescriptor,
                    StringComparison.Ordinal)) ||
            BindingDigestCanonicalizer.CanonicaliseActiveDocumentSet(bindings).Digest !=
                request.Specification.ActiveDocumentSetDigest ||
            BindingDigestCanonicalizer.CanonicaliseSourceBindingSet(bindings).Digest !=
                request.Specification.SourceBindingSetDigest)
        {
            throw new ArgumentException(
                "Indexed bindings must uniquely match the generation specification.",
                nameof(request));
        }
    }

    private static bool ContainsNonFinite(ReadOnlySpan<float> vector)
    {
        foreach (var value in vector)
        {
            if (!float.IsFinite(value))
            {
                return true;
            }
        }

        return false;
    }
}

public sealed record GenerationActivationRequest(
    FinalisedIndexGenerationManifest Manifest,
    IReadOnlyCollection<DocumentBinding> Bindings,
    long ExpectedCurrentRevision,
    TimeSpan PreviousGenerationRetention,
    AdministrativeAuditContext AuditContext,
    AdministrationJournalCompletion? JournalCompletion = null);

public sealed class GenerationActivationService(IControlPlaneStore controlPlaneStore)
{
    private readonly IControlPlaneStore controlPlaneStore = controlPlaneStore ??
        throw new ArgumentNullException(nameof(controlPlaneStore));

    public async Task<ActivationMutationResult> ActivateAsync(
        GenerationActivationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Manifest);
        ArgumentNullException.ThrowIfNull(request.Bindings);
        ArgumentNullException.ThrowIfNull(request.AuditContext);

        if (request.Bindings.Count == 0 || request.ExpectedCurrentRevision < 0)
        {
            throw new ArgumentException(
                "Generation activation requires bindings and a non-negative expected revision.",
                nameof(request));
        }

        var current = await controlPlaneStore.ReadActiveActivationAsync(
            request.Manifest.CorpusId,
            cancellationToken).ConfigureAwait(false);
        var mutationKind = request.ExpectedCurrentRevision == 0
            ? ActivationMutationKind.Initial
            : ActivationMutationKind.Replacement;
        CorpusActivationRecord proposed;

        if (mutationKind == ActivationMutationKind.Initial)
        {
            proposed = ActivationRecordFactory.CreateInitial(
                request.Manifest,
                request.Bindings,
                request.AuditContext.RequestedAt);
        }
        else
        {
            if (current is null)
            {
                return new ActivationMutationResult(
                    StoreMutationOutcome.RevisionConflict,
                    currentRecord: null);
            }

            proposed = ActivationRecordFactory.CreateGenerationReplacement(
                current,
                request.Manifest,
                request.Bindings,
                request.AuditContext.RequestedAt);
        }

        return await controlPlaneStore.CompareExchangeActivationAsync(
            new ActivationCompareExchangeRequest(
                request.AuditContext.OperationId,
                mutationKind,
                request.ExpectedCurrentRevision,
                proposed,
                request.Manifest.IndexCompatibilityKey,
                request.AuditContext.RequestedAt,
                request.PreviousGenerationRetention,
                request.AuditContext.CreateDigest(
                    request.Manifest.IndexGenerationId.Value,
                    proposed.ActivationBindingSetDigest.Value,
                    mutationKind.ToString()),
                request.JournalCompletion),
            cancellationToken).ConfigureAwait(false);
    }
}
