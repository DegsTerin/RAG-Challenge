// Purpose: Performs the complete fail-closed pre-CAS validation in Application; it neither persists records nor assumes an Infrastructure store.
using System.Collections.ObjectModel;

using RagChallenge.Application.Documents;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.Application.IndexingRetrieval;

public enum ActivationValidationFailure
{
    CorpusMismatch,
    RecordRevisionMismatch,
    PreviousRecordRevisionMismatch,
    GenerationMismatch,
    CatalogueRevisionMismatch,
    IndexCompatibilityMismatch,
    ActiveDocumentSetDigestMismatch,
    SourceBindingSetDigestMismatch,
    ActivationBindingSetDigestMismatch,
    ActivationEvidenceBindingMismatch,
    DocumentRightsNotPermitted,
    DuplicateActiveDocumentProjection,
    ObservationBindingMismatch,
    ObservationMissing,
    ObservationRegistrationMismatch,
    ObservationSnapshotMismatch,
    ActiveDatabaseHasNoEligibleDocument,
}

public sealed class ActivationValidationResult
{
    internal ActivationValidationResult(
        IEnumerable<ActivationValidationFailure> failures)
    {
        var distinctFailures = failures.Distinct().ToArray();
        Failures = Array.AsReadOnly(distinctFailures);
    }

    public bool IsValid => Failures.Count == 0;

    public ReadOnlyCollection<ActivationValidationFailure> Failures { get; }
}

public static class ActivationRecordValidator
{
    public static ActivationValidationResult ValidateForCompareAndSwap(
        CorpusActivationRecord? currentRecord,
        FinalisedIndexGenerationManifest manifest,
        CorpusActivationRecord proposedRecord,
        IndexCompatibilityKey requiredCompatibilityKey,
        IReadOnlyDictionary<OfficialObservationId, OfficialSourceObservation> observations,
        DateTimeOffset evaluatedAt)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(proposedRecord);
        ArgumentNullException.ThrowIfNull(requiredCompatibilityKey);
        ArgumentNullException.ThrowIfNull(observations);

        if (evaluatedAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Pre-CAS eligibility must be evaluated at a UTC instant.",
                nameof(evaluatedAt));
        }

        var failures = new List<ActivationValidationFailure>();

        ValidateLineage(currentRecord, proposedRecord, failures);
        ValidateManifestIdentity(
            manifest,
            proposedRecord,
            requiredCompatibilityKey,
            failures);
        ValidateThreeDigestDomains(manifest, proposedRecord, failures);
        ValidateEvidenceBindings(proposedRecord, failures);
        ValidateObservationsAndCoverage(
            proposedRecord,
            observations,
            evaluatedAt,
            failures);

        return new ActivationValidationResult(failures);
    }

    private static void ValidateEvidenceBindings(
        CorpusActivationRecord proposedRecord,
        List<ActivationValidationFailure> failures)
    {
        if (!proposedRecord.HasCompleteEvidenceBindings)
        {
            failures.Add(ActivationValidationFailure.ActivationEvidenceBindingMismatch);
            return;
        }

        foreach (var evidence in proposedRecord.EvidenceBindings)
        {
            var gate = evidence.RenderManifestId is not null
                ? DocumentRightsEligibilityGate.PdfVisualEvidence
                : DocumentRightsEligibilityGate.TextualEvidence;

            if (!DocumentRightsEligibilityPolicy.Evaluate(evidence.Rights, gate).IsEligible)
            {
                failures.Add(ActivationValidationFailure.DocumentRightsNotPermitted);
            }
        }
    }

    private static void ValidateLineage(
        CorpusActivationRecord? currentRecord,
        CorpusActivationRecord proposedRecord,
        List<ActivationValidationFailure> failures)
    {
        if (currentRecord is null)
        {
            if (proposedRecord.RecordRevision.Value != 1)
            {
                failures.Add(ActivationValidationFailure.RecordRevisionMismatch);
            }

            if (proposedRecord.PreviousRecordRevision is not null)
            {
                failures.Add(ActivationValidationFailure.PreviousRecordRevisionMismatch);
            }

            return;
        }

        if (proposedRecord.CorpusId != currentRecord.CorpusId)
        {
            failures.Add(ActivationValidationFailure.CorpusMismatch);
        }

        if (proposedRecord.RecordRevision.Value != currentRecord.RecordRevision.Value + 1)
        {
            failures.Add(ActivationValidationFailure.RecordRevisionMismatch);
        }

        if (proposedRecord.PreviousRecordRevision != currentRecord.RecordRevision)
        {
            failures.Add(ActivationValidationFailure.PreviousRecordRevisionMismatch);
        }
    }

    private static void ValidateManifestIdentity(
        FinalisedIndexGenerationManifest manifest,
        CorpusActivationRecord proposedRecord,
        IndexCompatibilityKey requiredCompatibilityKey,
        List<ActivationValidationFailure> failures)
    {
        if (manifest.CorpusId != proposedRecord.CorpusId)
        {
            failures.Add(ActivationValidationFailure.CorpusMismatch);
        }

        if (manifest.IndexGenerationId != proposedRecord.IndexGenerationId)
        {
            failures.Add(ActivationValidationFailure.GenerationMismatch);
        }

        if (manifest.CatalogueRevision != proposedRecord.CatalogueRevision)
        {
            failures.Add(ActivationValidationFailure.CatalogueRevisionMismatch);
        }

        if (manifest.IndexCompatibilityKey != requiredCompatibilityKey)
        {
            failures.Add(ActivationValidationFailure.IndexCompatibilityMismatch);
        }
    }

    private static void ValidateThreeDigestDomains(
        FinalisedIndexGenerationManifest manifest,
        CorpusActivationRecord proposedRecord,
        List<ActivationValidationFailure> failures)
    {
        try
        {
            var activeDocuments = BindingDigestCanonicalizer
                .CanonicaliseActiveDocumentSet(proposedRecord.DocumentBindings);

            if (activeDocuments.Digest != manifest.ActiveDocumentSetDigest)
            {
                failures.Add(ActivationValidationFailure.ActiveDocumentSetDigestMismatch);
            }
        }
        catch (ArgumentException)
        {
            failures.Add(ActivationValidationFailure.DuplicateActiveDocumentProjection);
        }

        var sourceBindings = BindingDigestCanonicalizer
            .CanonicaliseSourceBindingSet(proposedRecord.DocumentBindings);

        if (sourceBindings.Digest != manifest.SourceBindingSetDigest)
        {
            failures.Add(ActivationValidationFailure.SourceBindingSetDigestMismatch);
        }

        var activationBindings = BindingDigestCanonicalizer
            .CanonicaliseActivationBindingSet(proposedRecord.DocumentBindings);

        if (activationBindings.Digest != proposedRecord.ActivationBindingSetDigest)
        {
            failures.Add(ActivationValidationFailure.ActivationBindingSetDigestMismatch);
        }
    }

    private static void ValidateObservationsAndCoverage(
        CorpusActivationRecord proposedRecord,
        IReadOnlyDictionary<OfficialObservationId, OfficialSourceObservation> observations,
        DateTimeOffset evaluatedAt,
        List<ActivationValidationFailure> failures)
    {
        var eligibleProducts = new HashSet<DatabaseProductId>();

        foreach (var binding in proposedRecord.DocumentBindings)
        {
            if (binding.SourceTrustClass == SourceTrustClass.LocalAuthorised)
            {
                eligibleProducts.Add(binding.DatabaseProductId);
                continue;
            }

            var observationId = binding.SourceObservationId!;

            if (!observations.TryGetValue(observationId, out var observation))
            {
                failures.Add(ActivationValidationFailure.ObservationMissing);
                continue;
            }

            var registrationMatches =
                observation.RegistrationId == binding.OfficialSourceRegistrationId;
            var snapshotMatches =
                observation.SnapshotId == binding.OfficialSnapshotId;

            if (!registrationMatches)
            {
                failures.Add(ActivationValidationFailure.ObservationRegistrationMismatch);
            }

            if (!snapshotMatches)
            {
                failures.Add(ActivationValidationFailure.ObservationSnapshotMismatch);
            }

            if (registrationMatches &&
                snapshotMatches &&
                observation.IsEligibleAt(evaluatedAt))
            {
                eligibleProducts.Add(binding.DatabaseProductId);
            }
        }

        foreach (var productId in proposedRecord.DocumentBindings
            .Select(binding => binding.DatabaseProductId)
            .Distinct())
        {
            if (!eligibleProducts.Contains(productId))
            {
                failures.Add(
                    ActivationValidationFailure.ActiveDatabaseHasNoEligibleDocument);
            }
        }
    }
}
