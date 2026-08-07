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
        DateTimeOffset recordUpdatedAt)
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

        CorpusId = corpusId;
        RecordRevision = recordRevision;
        PreviousRecordRevision = previousRecordRevision;
        IndexGenerationId = indexGenerationId;
        CatalogueRevision = catalogueRevision;
        ActivationBindingSetDigest = activationBindingSetDigest;
        DocumentBindings = Array.AsReadOnly(orderedBindings);
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
