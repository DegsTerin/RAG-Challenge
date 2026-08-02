// Purpose: Verifies all pre-CAS digest projections, observation compatibility, coverage, immutable-field rebinding, and freshness-safe new-record rollback.
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.UnitTests;

public sealed class ActivationRecordPolicyTests
{
    [Fact]
    public void InitialActivationPassesAllThreeDigestsAndObservationRelation()
    {
        var bindings = Bindings();
        var manifest = TestModelFactory.Manifest(bindings);
        var proposed = ActivationRecordFactory.CreateInitial(
            manifest,
            bindings.Reverse(),
            TestModelFactory.Now);
        var observations = ObservationDictionary(TestModelFactory.Observation());

        var result = ActivationRecordValidator.ValidateForCompareAndSwap(
            currentRecord: null,
            manifest,
            proposed,
            manifest.IndexCompatibilityKey,
            observations,
            TestModelFactory.Now);

        Assert.True(result.IsValid);
        Assert.Empty(result.Failures);
        Assert.Equal("postgresql", proposed.DocumentBindings[0].DatabaseProductId.Value);
        Assert.Equal("redis", proposed.DocumentBindings[1].DatabaseProductId.Value);
    }

    [Fact]
    public void ThreeDigestMismatchesAreReportedIndependently()
    {
        var bindings = Bindings();
        var validManifest = TestModelFactory.Manifest(bindings);
        var wrongManifest = CopyManifest(
            validManifest,
            activeDocumentSetDigest: new ActiveDocumentSetDigest(new string('e', 64)),
            sourceBindingSetDigest: new SourceBindingSetDigest(new string('f', 64)));
        var validRecord = TestModelFactory.InitialRecord(validManifest, bindings);
        var wrongRecord = CopyRecord(
            validRecord,
            activationDigest: new ActivationBindingSetDigest(new string('9', 64)));

        var result = ActivationRecordValidator.ValidateForCompareAndSwap(
            currentRecord: null,
            wrongManifest,
            wrongRecord,
            wrongManifest.IndexCompatibilityKey,
            ObservationDictionary(TestModelFactory.Observation()),
            TestModelFactory.Now);

        Assert.False(result.IsValid);
        Assert.Contains(
            ActivationValidationFailure.ActiveDocumentSetDigestMismatch,
            result.Failures);
        Assert.Contains(
            ActivationValidationFailure.SourceBindingSetDigestMismatch,
            result.Failures);
        Assert.Contains(
            ActivationValidationFailure.ActivationBindingSetDigestMismatch,
            result.Failures);
    }

    [Fact]
    public void ManifestRecordIdentityMismatchesFailClosed()
    {
        var bindings = Bindings();
        var manifest = TestModelFactory.Manifest(bindings);
        var record = TestModelFactory.InitialRecord(manifest, bindings);
        var mismatchedManifest = TestModelFactory.Manifest(
            bindings,
            digestCharacter: '8',
            corpusId: "different-corpus",
            catalogueRevision: 2);

        var result = ActivationRecordValidator.ValidateForCompareAndSwap(
            currentRecord: null,
            mismatchedManifest,
            record,
            mismatchedManifest.IndexCompatibilityKey,
            ObservationDictionary(TestModelFactory.Observation()),
            TestModelFactory.Now);

        Assert.Contains(ActivationValidationFailure.CorpusMismatch, result.Failures);
        Assert.Contains(ActivationValidationFailure.GenerationMismatch, result.Failures);
        Assert.Contains(ActivationValidationFailure.CatalogueRevisionMismatch, result.Failures);
    }

    [Fact]
    public void CompareAndSwapLineageMustAdvanceFromExpectedRecord()
    {
        var binding = TestModelFactory.LocalBinding();
        var manifest = TestModelFactory.Manifest([binding]);
        var current = TestModelFactory.InitialRecord(manifest, [binding]);

        var result = ActivationRecordValidator.ValidateForCompareAndSwap(
            current,
            manifest,
            current,
            manifest.IndexCompatibilityKey,
            new Dictionary<OfficialObservationId, OfficialSourceObservation>(),
            TestModelFactory.Now);

        Assert.Contains(ActivationValidationFailure.RecordRevisionMismatch, result.Failures);
        Assert.Contains(
            ActivationValidationFailure.PreviousRecordRevisionMismatch,
            result.Failures);
    }

    [Fact]
    public void ObservationMustExistAndMatchRegistrationAndSnapshot()
    {
        var binding = TestModelFactory.OfficialBinding();
        var manifest = TestModelFactory.Manifest([binding]);
        var record = TestModelFactory.InitialRecord(manifest, [binding]);
        var missing = ActivationRecordValidator.ValidateForCompareAndSwap(
            null,
            manifest,
            record,
            manifest.IndexCompatibilityKey,
            new Dictionary<OfficialObservationId, OfficialSourceObservation>(),
            TestModelFactory.Now);
        var wrongRegistration = TestModelFactory.Observation(
            registrationId: "different-registration");
        var wrongSnapshot = TestModelFactory.Observation(
            snapshotId: "different-snapshot");
        var registrationResult = ActivationRecordValidator.ValidateForCompareAndSwap(
            null,
            manifest,
            record,
            manifest.IndexCompatibilityKey,
            ObservationDictionary(wrongRegistration),
            TestModelFactory.Now);
        var snapshotResult = ActivationRecordValidator.ValidateForCompareAndSwap(
            null,
            manifest,
            record,
            manifest.IndexCompatibilityKey,
            ObservationDictionary(wrongSnapshot),
            TestModelFactory.Now);

        Assert.Contains(ActivationValidationFailure.ObservationMissing, missing.Failures);
        Assert.Contains(
            ActivationValidationFailure.ObservationRegistrationMismatch,
            registrationResult.Failures);
        Assert.Contains(
            ActivationValidationFailure.ObservationSnapshotMismatch,
            snapshotResult.Failures);
        Assert.All(
            [missing, registrationResult, snapshotResult],
            result => Assert.Contains(
                ActivationValidationFailure.ActiveDatabaseHasNoEligibleDocument,
                result.Failures));
    }

    [Fact]
    public void EveryActiveDatabaseRequiresAtLeastOneCurrentlyEligibleBinding()
    {
        var official = TestModelFactory.OfficialBinding(productId: "postgresql");
        var local = TestModelFactory.LocalBinding();
        var stale = TestModelFactory.Observation(
            state: OfficialObservationState.Stale);
        var officialOnlyManifest = TestModelFactory.Manifest([official]);
        var officialOnlyRecord = TestModelFactory.InitialRecord(
            officialOnlyManifest,
            [official]);
        var mixedManifest = TestModelFactory.Manifest([official, local]);
        var mixedRecord = TestModelFactory.InitialRecord(mixedManifest, [official, local]);

        var officialOnly = ActivationRecordValidator.ValidateForCompareAndSwap(
            null,
            officialOnlyManifest,
            officialOnlyRecord,
            officialOnlyManifest.IndexCompatibilityKey,
            ObservationDictionary(stale),
            TestModelFactory.Now);
        var mixed = ActivationRecordValidator.ValidateForCompareAndSwap(
            null,
            mixedManifest,
            mixedRecord,
            mixedManifest.IndexCompatibilityKey,
            ObservationDictionary(stale),
            TestModelFactory.Now);

        Assert.Contains(
            ActivationValidationFailure.ActiveDatabaseHasNoEligibleDocument,
            officialOnly.Failures);
        Assert.True(mixed.IsValid);
    }

    [Fact]
    public void DuplicateActiveDocumentProjectionFailsBeforeCompareAndSwap()
    {
        DocumentBinding[] bindings =
        [
            TestModelFactory.LocalBinding(sourceAdapterId: "local-a"),
            TestModelFactory.LocalBinding(sourceAdapterId: "local-b"),
        ];
        var sourceDigest = BindingDigestCanonicalizer
            .CanonicaliseSourceBindingSet(bindings)
            .Digest;
        var activationDigest = BindingDigestCanonicalizer
            .CanonicaliseActivationBindingSet(bindings)
            .Digest;
        var manifest = CreateManifest(
            sourceDigest,
            activeDocumentSetDigest: new ActiveDocumentSetDigest(new string('e', 64)));
        var record = new CorpusActivationRecord(
            manifest.CorpusId,
            new ActivationRecordRevision(1),
            null,
            manifest.IndexGenerationId,
            manifest.CatalogueRevision,
            activationDigest,
            bindings,
            TestModelFactory.Now,
            TestModelFactory.Now);

        var result = ActivationRecordValidator.ValidateForCompareAndSwap(
            null,
            manifest,
            record,
            manifest.IndexCompatibilityKey,
            new Dictionary<OfficialObservationId, OfficialSourceObservation>(),
            TestModelFactory.Now);

        Assert.Contains(
            ActivationValidationFailure.DuplicateActiveDocumentProjection,
            result.Failures);
    }

    [Fact]
    public void ObservationRebindingChangesOnlyPermittedActivationFields()
    {
        var binding = TestModelFactory.OfficialBinding();
        var manifest = TestModelFactory.Manifest([binding]);
        var current = TestModelFactory.InitialRecord(manifest, [binding]);
        var nextObservation = TestModelFactory.Observation(
            observationId: "observation-redis-v2");
        var rebound = ActivationRecordFactory.RebindObservation(
            current,
            binding.DocumentId,
            binding.DocumentVersion,
            nextObservation,
            TestModelFactory.Now.AddMinutes(10));

        Assert.Equal(2, rebound.RecordRevision.Value);
        Assert.Equal(current.RecordRevision, rebound.PreviousRecordRevision);
        Assert.Equal(current.IndexGenerationId, rebound.IndexGenerationId);
        Assert.Equal(current.CatalogueRevision, rebound.CatalogueRevision);
        Assert.Equal(current.GenerationActivatedAt, rebound.GenerationActivatedAt);
        Assert.NotEqual(
            current.ActivationBindingSetDigest,
            rebound.ActivationBindingSetDigest);
        Assert.Equal(
            BindingDigestCanonicalizer
                .CanonicaliseSourceBindingSet(current.DocumentBindings)
                .Digest,
            BindingDigestCanonicalizer
                .CanonicaliseSourceBindingSet(rebound.DocumentBindings)
                .Digest);

        var validation = ActivationRecordValidator.ValidateForCompareAndSwap(
            current,
            manifest,
            rebound,
            manifest.IndexCompatibilityKey,
            ObservationDictionary(nextObservation),
            TestModelFactory.Now.AddMinutes(10));

        Assert.True(validation.IsValid);
    }

    [Fact]
    public void RebindingRejectsLocalOrMismatchedOfficialObservation()
    {
        var local = TestModelFactory.LocalBinding();
        var localManifest = TestModelFactory.Manifest([local]);
        var localRecord = TestModelFactory.InitialRecord(localManifest, [local]);
        var official = TestModelFactory.OfficialBinding();
        var officialManifest = TestModelFactory.Manifest([official]);
        var officialRecord = TestModelFactory.InitialRecord(officialManifest, [official]);

        Assert.Throws<InvalidOperationException>(
            () => ActivationRecordFactory.RebindObservation(
                localRecord,
                local.DocumentId,
                local.DocumentVersion,
                TestModelFactory.Observation(),
                TestModelFactory.Now.AddMinutes(1)));
        Assert.Throws<ArgumentException>(
            () => ActivationRecordFactory.RebindObservation(
                officialRecord,
                official.DocumentId,
                official.DocumentVersion,
                TestModelFactory.Observation(snapshotId: "different-snapshot"),
                TestModelFactory.Now.AddMinutes(1)));
    }

    [Fact]
    public void RollbackBuildsNewRevisionWithExplicitCurrentlyEligibleObservation()
    {
        var currentBinding = TestModelFactory.LocalBinding();
        var currentManifest = TestModelFactory.Manifest([currentBinding], digestCharacter: 'a');
        var currentRecord = TestModelFactory.InitialRecord(currentManifest, [currentBinding]);
        var targetBinding = TestModelFactory.OfficialBinding();
        var targetManifest = TestModelFactory.Manifest([targetBinding], digestCharacter: '7');
        var rollback = ActivationRecordFactory.CreateRollback(
            currentRecord,
            targetManifest,
            [targetBinding],
            TestModelFactory.Now.AddMinutes(15));
        var observation = TestModelFactory.Observation();

        var validation = ActivationRecordValidator.ValidateForCompareAndSwap(
            currentRecord,
            targetManifest,
            rollback,
            targetManifest.IndexCompatibilityKey,
            ObservationDictionary(observation),
            TestModelFactory.Now.AddMinutes(15));

        Assert.True(validation.IsValid);
        Assert.Equal(2, rollback.RecordRevision.Value);
        Assert.Equal(currentRecord.RecordRevision, rollback.PreviousRecordRevision);
        Assert.Equal(targetManifest.IndexGenerationId, rollback.IndexGenerationId);
        Assert.NotEqual(currentRecord.IndexGenerationId, rollback.IndexGenerationId);
        Assert.Equal(TestModelFactory.Now.AddMinutes(15), rollback.GenerationActivatedAt);

        var staleValidation = ActivationRecordValidator.ValidateForCompareAndSwap(
            currentRecord,
            targetManifest,
            rollback,
            targetManifest.IndexCompatibilityKey,
            ObservationDictionary(TestModelFactory.Observation(
                state: OfficialObservationState.Stale)),
            TestModelFactory.Now.AddMinutes(15));

        Assert.Contains(
            ActivationValidationFailure.ActiveDatabaseHasNoEligibleDocument,
            staleValidation.Failures);
    }

    [Fact]
    public void ReplacementCannotCrossCorpusAndValidationRequiresUtc()
    {
        var binding = TestModelFactory.LocalBinding();
        var currentManifest = TestModelFactory.Manifest([binding]);
        var current = TestModelFactory.InitialRecord(currentManifest, [binding]);
        var otherCorpus = TestModelFactory.Manifest(
            [binding],
            digestCharacter: '8',
            corpusId: "other-corpus");

        Assert.Throws<ArgumentException>(
            () => ActivationRecordFactory.CreateGenerationReplacement(
                current,
                otherCorpus,
                [binding],
                TestModelFactory.Now));
        Assert.Throws<ArgumentException>(
            () => ActivationRecordFactory.CreateRollback(
                current,
                currentManifest,
                [binding],
                TestModelFactory.Now));

        var incompatible = ActivationRecordValidator.ValidateForCompareAndSwap(
            null,
            currentManifest,
            current,
            new IndexCompatibilityKey(new string('f', 64)),
            new Dictionary<OfficialObservationId, OfficialSourceObservation>(),
            TestModelFactory.Now);

        Assert.Contains(
            ActivationValidationFailure.IndexCompatibilityMismatch,
            incompatible.Failures);
        Assert.Throws<ArgumentException>(
            () => ActivationRecordValidator.ValidateForCompareAndSwap(
                null,
                currentManifest,
                current,
                currentManifest.IndexCompatibilityKey,
                new Dictionary<OfficialObservationId, OfficialSourceObservation>(),
                TestModelFactory.Now.ToOffset(TimeSpan.FromHours(-3))));
    }

    private static DocumentBinding[] Bindings() =>
        [TestModelFactory.LocalBinding(), TestModelFactory.OfficialBinding()];

    private static Dictionary<OfficialObservationId, OfficialSourceObservation>
        ObservationDictionary(OfficialSourceObservation observation) =>
        new() { [observation.Id] = observation };

    private static FinalisedIndexGenerationManifest CopyManifest(
        FinalisedIndexGenerationManifest source,
        ActiveDocumentSetDigest? activeDocumentSetDigest = null,
        SourceBindingSetDigest? sourceBindingSetDigest = null) =>
        new(
            source.ManifestSchemaVersion,
            source.CorpusId,
            source.CorpusRevision,
            source.CatalogueRevision,
            activeDocumentSetDigest ?? source.ActiveDocumentSetDigest,
            sourceBindingSetDigest ?? source.SourceBindingSetDigest,
            source.IndexCompatibilityKey,
            source.GenerationSpecDigest,
            source.ChunkCount,
            source.VectorCount,
            source.LogicalArtifactDigest,
            source.GenerationContentDigest,
            source.IndexGenerationId);

    private static CorpusActivationRecord CopyRecord(
        CorpusActivationRecord source,
        ActivationBindingSetDigest activationDigest) =>
        new(
            source.CorpusId,
            source.RecordRevision,
            source.PreviousRecordRevision,
            source.IndexGenerationId,
            source.CatalogueRevision,
            activationDigest,
            source.DocumentBindings,
            source.GenerationActivatedAt,
            source.RecordUpdatedAt);

    private static FinalisedIndexGenerationManifest CreateManifest(
        SourceBindingSetDigest sourceBindingSetDigest,
        ActiveDocumentSetDigest activeDocumentSetDigest)
    {
        var contentDigest = new string('a', 64);

        return new FinalisedIndexGenerationManifest(
            1,
            new CorpusId("mvp-database-documentation"),
            new CorpusRevision(1),
            new CatalogueRevision(1),
            activeDocumentSetDigest,
            sourceBindingSetDigest,
            new IndexCompatibilityKey(new string('b', 64)),
            new GenerationSpecDigest(new string('c', 64)),
            2,
            2,
            new LogicalArtifactDigest(new string('d', 64)),
            new GenerationContentDigest(contentDigest),
            new IndexGenerationId($"idxgen-{contentDigest}"));
    }
}
