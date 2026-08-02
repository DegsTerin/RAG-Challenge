// Purpose: Constructs initial, replacement, rollback, and observation-only activation revisions while preserving the accepted immutable-field boundaries.
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.Application.IndexingRetrieval;

public static class ActivationRecordFactory
{
    public static CorpusActivationRecord CreateInitial(
        FinalisedIndexGenerationManifest manifest,
        IEnumerable<DocumentBinding> bindings,
        DateTimeOffset activatedAt)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        var materialisedBindings = Materialise(bindings);
        var digest = BindingDigestCanonicalizer
            .CanonicaliseActivationBindingSet(materialisedBindings)
            .Digest;

        return new CorpusActivationRecord(
            manifest.CorpusId,
            new ActivationRecordRevision(1),
            previousRecordRevision: null,
            manifest.IndexGenerationId,
            manifest.CatalogueRevision,
            digest,
            materialisedBindings,
            activatedAt,
            activatedAt);
    }

    public static CorpusActivationRecord CreateGenerationReplacement(
        CorpusActivationRecord currentRecord,
        FinalisedIndexGenerationManifest targetManifest,
        IEnumerable<DocumentBinding> targetBindings,
        DateTimeOffset activatedAt) =>
        CreateReplacement(
            currentRecord,
            targetManifest,
            targetBindings,
            activatedAt);

    public static CorpusActivationRecord CreateRollback(
        CorpusActivationRecord currentRecord,
        FinalisedIndexGenerationManifest retainedTargetManifest,
        IEnumerable<DocumentBinding> explicitlySelectedBindings,
        DateTimeOffset activatedAt)
    {
        ArgumentNullException.ThrowIfNull(currentRecord);
        ArgumentNullException.ThrowIfNull(retainedTargetManifest);

        if (currentRecord.IndexGenerationId == retainedTargetManifest.IndexGenerationId)
        {
            throw new ArgumentException(
                "A rollback target must differ from the current generation.",
                nameof(retainedTargetManifest));
        }

        return CreateReplacement(
            currentRecord,
            retainedTargetManifest,
            explicitlySelectedBindings,
            activatedAt);
    }

    public static CorpusActivationRecord RebindObservation(
        CorpusActivationRecord currentRecord,
        DocumentId documentId,
        DocumentVersionNumber documentVersion,
        OfficialSourceObservation observation,
        DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(currentRecord);
        ArgumentNullException.ThrowIfNull(documentId);
        ArgumentNullException.ThrowIfNull(documentVersion);
        ArgumentNullException.ThrowIfNull(observation);

        var matches = currentRecord.DocumentBindings
            .Where(binding =>
                binding.DocumentId == documentId &&
                binding.DocumentVersion == documentVersion)
            .ToArray();

        if (matches.Length != 1 ||
            matches[0].SourceTrustClass != SourceTrustClass.OfficialExternal)
        {
            throw new InvalidOperationException(
                "Observation rebinding requires one exact official document-version binding.");
        }

        var target = matches[0];

        if (target.OfficialSourceRegistrationId != observation.RegistrationId ||
            target.OfficialSnapshotId != observation.SnapshotId)
        {
            throw new ArgumentException(
                "A freshness observation must name the binding's immutable registration and snapshot.",
                nameof(observation));
        }

        var reboundBindings = currentRecord.DocumentBindings
            .Select(binding =>
                ReferenceEquals(binding, target)
                    ? binding.WithObservation(observation.Id)
                    : binding)
            .ToArray();
        var digest = BindingDigestCanonicalizer
            .CanonicaliseActivationBindingSet(reboundBindings)
            .Digest;

        return new CorpusActivationRecord(
            currentRecord.CorpusId,
            NextRevision(currentRecord),
            currentRecord.RecordRevision,
            currentRecord.IndexGenerationId,
            currentRecord.CatalogueRevision,
            digest,
            reboundBindings,
            currentRecord.GenerationActivatedAt,
            updatedAt);
    }

    private static CorpusActivationRecord CreateReplacement(
        CorpusActivationRecord currentRecord,
        FinalisedIndexGenerationManifest targetManifest,
        IEnumerable<DocumentBinding> targetBindings,
        DateTimeOffset activatedAt)
    {
        ArgumentNullException.ThrowIfNull(currentRecord);
        ArgumentNullException.ThrowIfNull(targetManifest);

        if (currentRecord.CorpusId != targetManifest.CorpusId)
        {
            throw new ArgumentException(
                "An activation replacement cannot cross corpus boundaries.",
                nameof(targetManifest));
        }

        var materialisedBindings = Materialise(targetBindings);
        var digest = BindingDigestCanonicalizer
            .CanonicaliseActivationBindingSet(materialisedBindings)
            .Digest;

        return new CorpusActivationRecord(
            currentRecord.CorpusId,
            NextRevision(currentRecord),
            currentRecord.RecordRevision,
            targetManifest.IndexGenerationId,
            targetManifest.CatalogueRevision,
            digest,
            materialisedBindings,
            activatedAt,
            activatedAt);
    }

    private static DocumentBinding[] Materialise(IEnumerable<DocumentBinding> bindings)
    {
        ArgumentNullException.ThrowIfNull(bindings);
        return bindings.ToArray();
    }

    private static ActivationRecordRevision NextRevision(
        CorpusActivationRecord currentRecord) =>
        new(checked(currentRecord.RecordRevision.Value + 1));
}
