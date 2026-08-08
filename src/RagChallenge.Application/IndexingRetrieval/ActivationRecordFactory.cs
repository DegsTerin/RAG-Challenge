// Purpose: Constructs initial, replacement, rollback, and observation-only activation revisions while preserving the accepted immutable-field boundaries.
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.Application.IndexingRetrieval;

public static class ActivationRecordFactory
{
    public static CorpusActivationRecord CreateInitial(
        FinalisedIndexGenerationManifest manifest,
        IEnumerable<DocumentActivationEvidenceBinding> evidenceBindings,
        DateTimeOffset activatedAt)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        var materialisedEvidence = Materialise(evidenceBindings);
        var materialisedBindings = materialisedEvidence
            .Select(binding => binding.DocumentBinding)
            .ToArray();
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
            activatedAt,
            materialisedEvidence);
    }

    public static CorpusActivationRecord CreateGenerationReplacement(
        CorpusActivationRecord currentRecord,
        FinalisedIndexGenerationManifest targetManifest,
        IEnumerable<DocumentActivationEvidenceBinding> targetEvidenceBindings,
        DateTimeOffset activatedAt) =>
        CreateReplacement(
            currentRecord,
            targetManifest,
            targetEvidenceBindings,
            activatedAt);

    public static CorpusActivationRecord CreateRollback(
        CorpusActivationRecord currentRecord,
        FinalisedIndexGenerationManifest retainedTargetManifest,
        IEnumerable<DocumentActivationEvidenceBinding> explicitlySelectedEvidenceBindings,
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
            explicitlySelectedEvidenceBindings,
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

        if (!currentRecord.HasCompleteEvidenceBindings)
        {
            throw new InvalidOperationException(
                "Observation rebinding cannot infer evidence for a historical activation revision.");
        }

        var reboundEvidence = currentRecord.EvidenceBindings
            .Select(evidence =>
                evidence.DocumentBinding == target
                    ? evidence.WithDocumentBinding(
                        evidence.DocumentBinding.WithObservation(observation.Id))
                    : evidence)
            .ToArray();
        var reboundBindings = reboundEvidence
            .Select(evidence => evidence.DocumentBinding)
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
            updatedAt,
            reboundEvidence);
    }

    private static CorpusActivationRecord CreateReplacement(
        CorpusActivationRecord currentRecord,
        FinalisedIndexGenerationManifest targetManifest,
        IEnumerable<DocumentActivationEvidenceBinding> targetEvidenceBindings,
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

        var materialisedEvidence = Materialise(targetEvidenceBindings);
        var materialisedBindings = materialisedEvidence
            .Select(binding => binding.DocumentBinding)
            .ToArray();
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
            activatedAt,
            materialisedEvidence);
    }

    private static DocumentActivationEvidenceBinding[] Materialise(
        IEnumerable<DocumentActivationEvidenceBinding> evidenceBindings)
    {
        ArgumentNullException.ThrowIfNull(evidenceBindings);
        var materialised = evidenceBindings.ToArray();

        if (materialised.Length == 0)
        {
            throw new ArgumentException(
                "An activation revision requires at least one explicit evidence binding.",
                nameof(evidenceBindings));
        }

        return materialised;
    }

    private static ActivationRecordRevision NextRevision(
        CorpusActivationRecord currentRecord) =>
        new(checked(currentRecord.RecordRevision.Value + 1));
}
