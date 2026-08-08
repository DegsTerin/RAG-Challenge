// Purpose: Defines the complete activation authority and bounded content reachability invariants; compare-and-swap persistence remains an Infrastructure implementation.
using System.Collections.ObjectModel;

using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Domain.IndexingRetrieval;

public sealed class CorpusActivationRecord
{
    public CorpusActivationRecord(
        CorpusId corpusId,
        ActivationRecordRevision recordRevision,
        ActivationRecordRevision? previousRecordRevision,
        IndexGenerationId indexGenerationId,
        CatalogueRevision catalogueRevision,
        ActivationBindingSetDigest activationBindingSetDigest,
        IEnumerable<DocumentBinding> documentBindings,
        DateTimeOffset generationActivatedAt,
        DateTimeOffset recordUpdatedAt,
        IEnumerable<DocumentActivationEvidenceBinding>? evidenceBindings = null)
    {
        ArgumentNullException.ThrowIfNull(corpusId);
        ArgumentNullException.ThrowIfNull(recordRevision);
        ArgumentNullException.ThrowIfNull(indexGenerationId);
        ArgumentNullException.ThrowIfNull(catalogueRevision);
        ArgumentNullException.ThrowIfNull(activationBindingSetDigest);

        ValidateRevisionLineage(recordRevision, previousRecordRevision);
        ValidateUtcInstant(generationActivatedAt, nameof(generationActivatedAt));
        ValidateUtcInstant(recordUpdatedAt, nameof(recordUpdatedAt));

        if (recordUpdatedAt < generationActivatedAt)
        {
            throw new ArgumentException(
                "The activation-record update cannot predate generation activation.",
                nameof(recordUpdatedAt));
        }

        var orderedBindings = BindingDigestCanonicalizer
            .OrderAndValidate(documentBindings)
            .ToArray();
        var orderedEvidenceBindings = OrderAndValidateEvidenceBindings(
            orderedBindings,
            evidenceBindings);

        CorpusId = corpusId;
        RecordRevision = recordRevision;
        PreviousRecordRevision = previousRecordRevision;
        IndexGenerationId = indexGenerationId;
        CatalogueRevision = catalogueRevision;
        ActivationBindingSetDigest = activationBindingSetDigest;
        DocumentBindings = Array.AsReadOnly(orderedBindings);
        EvidenceBindings = Array.AsReadOnly(orderedEvidenceBindings);
        GenerationActivatedAt = generationActivatedAt;
        RecordUpdatedAt = recordUpdatedAt;
    }

    public CorpusId CorpusId { get; }

    public ActivationRecordRevision RecordRevision { get; }

    public ActivationRecordRevision? PreviousRecordRevision { get; }

    public IndexGenerationId IndexGenerationId { get; }

    public CatalogueRevision CatalogueRevision { get; }

    public ActivationBindingSetDigest ActivationBindingSetDigest { get; }

    public ReadOnlyCollection<DocumentBinding> DocumentBindings { get; }

    public ReadOnlyCollection<DocumentActivationEvidenceBinding> EvidenceBindings { get; }

    public bool HasCompleteEvidenceBindings =>
        EvidenceBindings.Count == DocumentBindings.Count && DocumentBindings.Count != 0;

    public DateTimeOffset GenerationActivatedAt { get; }

    public DateTimeOffset RecordUpdatedAt { get; }

    private static void ValidateRevisionLineage(
        ActivationRecordRevision current,
        ActivationRecordRevision? previous)
    {
        if (current.Value == 1 && previous is not null)
        {
            throw new ArgumentException(
                "The first activation-record revision cannot name a predecessor.",
                nameof(previous));
        }

        if (current.Value > 1 &&
            (previous is null || previous.Value != current.Value - 1))
        {
            throw new ArgumentException(
                "Every later activation-record revision must name its immediate predecessor.",
                nameof(previous));
        }
    }

    private static void ValidateUtcInstant(DateTimeOffset value, string parameterName)
    {
        if (value.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Activation instants must be expressed in UTC.",
                parameterName);
        }
    }

    private static DocumentActivationEvidenceBinding[] OrderAndValidateEvidenceBindings(
        DocumentBinding[] orderedBindings,
        IEnumerable<DocumentActivationEvidenceBinding>? evidenceBindings)
    {
        var evidence = evidenceBindings?.ToArray() ?? [];

        if (evidence.Any(binding => binding is null))
        {
            throw new ArgumentException(
                "Activation evidence cannot contain a null binding.",
                nameof(evidenceBindings));
        }

        if (evidence.Length == 0)
        {
            return [];
        }

        var byDocument = new Dictionary<(DocumentId, DocumentVersionNumber),
            DocumentActivationEvidenceBinding>();

        foreach (var item in evidence)
        {
            if (!byDocument.TryAdd(
                    (item.DocumentBinding.DocumentId, item.DocumentBinding.DocumentVersion),
                    item))
            {
                throw new ArgumentException(
                    "Activation evidence must bind every document revision exactly once.",
                    nameof(evidenceBindings));
            }
        }

        if (byDocument.Count != orderedBindings.Length)
        {
            throw new ArgumentException(
                "Activation evidence must cover the complete document-binding set.",
                nameof(evidenceBindings));
        }

        var orderedEvidence = new List<DocumentActivationEvidenceBinding>(orderedBindings.Length);

        foreach (var binding in orderedBindings)
        {
            if (!byDocument.TryGetValue(
                    (binding.DocumentId, binding.DocumentVersion),
                    out var item) ||
                item.DocumentBinding != binding)
            {
                throw new ArgumentException(
                    "Activation evidence must contain the exact document binding.",
                    nameof(evidenceBindings));
            }

            orderedEvidence.Add(item);
        }

        return orderedEvidence.ToArray();
    }
}

public sealed class GenerationRetentionReference
{
    public GenerationRetentionReference(
        IndexGenerationId generationId,
        IEnumerable<ContentObjectId> contentObjectIds)
    {
        ArgumentNullException.ThrowIfNull(generationId);
        ArgumentNullException.ThrowIfNull(contentObjectIds);

        var contentIds = contentObjectIds.Distinct().ToArray();

        if (contentIds.Length == 0)
        {
            throw new ArgumentException(
                "A protected generation must retain at least one reopenable content object.",
                nameof(contentObjectIds));
        }

        GenerationId = generationId;
        ContentObjectIds = Array.AsReadOnly(contentIds);
    }

    public IndexGenerationId GenerationId { get; }

    public ReadOnlyCollection<ContentObjectId> ContentObjectIds { get; }
}

public sealed class RetentionReachability
{
    private readonly HashSet<ContentObjectId> reachableContentObjectIds;

    public RetentionReachability(
        GenerationRetentionReference activeGeneration,
        GenerationRetentionReference? rollbackGeneration,
        IEnumerable<DocumentRenderManifest>? renderManifests = null)
    {
        ArgumentNullException.ThrowIfNull(activeGeneration);

        if (rollbackGeneration is not null &&
            rollbackGeneration.GenerationId == activeGeneration.GenerationId)
        {
            throw new ArgumentException(
                "The bounded rollback target must differ from the active generation.",
                nameof(rollbackGeneration));
        }

        ActiveGeneration = activeGeneration;
        RollbackGeneration = rollbackGeneration;
        var manifests = renderManifests?.ToArray() ?? [];

        if (manifests.Any(manifest => manifest is null))
        {
            throw new ArgumentException(
                "Render-manifest reachability cannot contain a null manifest.",
                nameof(renderManifests));
        }

        reachableContentObjectIds = activeGeneration.ContentObjectIds
            .Concat(rollbackGeneration?.ContentObjectIds ?? [])
            .Concat(manifests.Select(manifest => manifest.SourceContentObjectId))
            .Concat(manifests.SelectMany(manifest =>
                manifest.OrderedPageImages.Select(page => page.ImageContentObjectId)))
            .ToHashSet();
    }

    public GenerationRetentionReference ActiveGeneration { get; }

    public GenerationRetentionReference? RollbackGeneration { get; }

    public bool IsGenerationProtected(IndexGenerationId generationId)
    {
        ArgumentNullException.ThrowIfNull(generationId);

        return ActiveGeneration.GenerationId == generationId ||
            RollbackGeneration?.GenerationId == generationId;
    }

    public bool CanPhysicallyDelete(ContentObjectId contentObjectId)
    {
        ArgumentNullException.ThrowIfNull(contentObjectId);
        return !reachableContentObjectIds.Contains(contentObjectId);
    }
}
