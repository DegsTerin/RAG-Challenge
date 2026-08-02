// Purpose: Verifies staging visibility, final manifest identity/count constraints, activation revision lineage, and bounded raw-content reachability.
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.UnitTests;

public sealed class IndexGenerationAndRetentionTests
{
    [Fact]
    public void CandidateIsNeverQueryableUntilValidatedWithFinalManifest()
    {
        var candidate = IndexBuildRecord.CreateCandidate(
            new CandidateBuildId("candidate-build-1"));
        var manifest = TestModelFactory.Manifest([TestModelFactory.LocalBinding()]);

        var validated = candidate.MarkValidated(manifest);
        var failed = candidate.MarkFailed();

        Assert.Equal(IndexBuildStatus.Candidate, candidate.Status);
        Assert.False(candidate.IsQueryable);
        Assert.Null(candidate.Manifest);
        Assert.Equal(IndexBuildStatus.Validated, validated.Status);
        Assert.True(validated.IsQueryable);
        Assert.Same(manifest, validated.Manifest);
        Assert.Equal(IndexBuildStatus.Failed, failed.Status);
        Assert.False(failed.IsQueryable);
        Assert.Throws<InvalidOperationException>(() => validated.MarkFailed());
        Assert.Throws<InvalidOperationException>(() => failed.MarkValidated(manifest));
    }

    [Fact]
    public void FinalManifestRequiresCountsAndGenerationIdentityToMatch()
    {
        var valid = TestModelFactory.Manifest([TestModelFactory.LocalBinding()]);

        Assert.Equal(1, valid.ManifestSchemaVersion);
        Assert.Equal(valid.ChunkCount, valid.VectorCount);
        Assert.Equal(
            $"idxgen-{valid.GenerationContentDigest.Value}",
            valid.IndexGenerationId.Value);
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateManifest(manifestSchemaVersion: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateManifest(chunkCount: 0));
        Assert.Throws<ArgumentException>(
            () => CreateManifest(chunkCount: 2, vectorCount: 1));
        Assert.Throws<ArgumentException>(
            () => CreateManifest(
                generationContentDigest: new string('a', 64),
                generationIdDigest: new string('b', 64)));
    }

    [Fact]
    public void ActivationRecordOrdersBindingsAndRequiresImmediateLineage()
    {
        DocumentBinding[] unordered =
        [
            TestModelFactory.OfficialBinding(),
            TestModelFactory.LocalBinding(),
        ];
        var manifest = TestModelFactory.Manifest(unordered);
        var digest = BindingDigestCanonicalizer
            .CanonicaliseActivationBindingSet(unordered)
            .Digest;
        var record = new CorpusActivationRecord(
            manifest.CorpusId,
            new ActivationRecordRevision(1),
            previousRecordRevision: null,
            manifest.IndexGenerationId,
            manifest.CatalogueRevision,
            digest,
            unordered,
            TestModelFactory.Now,
            TestModelFactory.Now);

        Assert.Equal("postgresql", record.DocumentBindings[0].DatabaseProductId.Value);
        Assert.Equal("redis", record.DocumentBindings[1].DatabaseProductId.Value);
        Assert.Throws<ArgumentException>(
            () => new CorpusActivationRecord(
                record.CorpusId,
                new ActivationRecordRevision(1),
                new ActivationRecordRevision(1),
                record.IndexGenerationId,
                record.CatalogueRevision,
                record.ActivationBindingSetDigest,
                record.DocumentBindings,
                record.GenerationActivatedAt,
                record.RecordUpdatedAt));
        Assert.Throws<ArgumentException>(
            () => new CorpusActivationRecord(
                record.CorpusId,
                new ActivationRecordRevision(3),
                new ActivationRecordRevision(1),
                record.IndexGenerationId,
                record.CatalogueRevision,
                record.ActivationBindingSetDigest,
                record.DocumentBindings,
                record.GenerationActivatedAt,
                record.RecordUpdatedAt));
    }

    [Fact]
    public void ActivationRecordRequiresUtcChronologyAndNonEmptyBindings()
    {
        var binding = TestModelFactory.LocalBinding();
        var manifest = TestModelFactory.Manifest([binding]);
        var digest = BindingDigestCanonicalizer
            .CanonicaliseActivationBindingSet([binding])
            .Digest;

        Assert.Throws<ArgumentException>(
            () => new CorpusActivationRecord(
                manifest.CorpusId,
                new ActivationRecordRevision(1),
                null,
                manifest.IndexGenerationId,
                manifest.CatalogueRevision,
                digest,
                [binding],
                TestModelFactory.Now.ToOffset(TimeSpan.FromHours(-3)),
                TestModelFactory.Now));
        Assert.Throws<ArgumentException>(
            () => new CorpusActivationRecord(
                manifest.CorpusId,
                new ActivationRecordRevision(1),
                null,
                manifest.IndexGenerationId,
                manifest.CatalogueRevision,
                digest,
                [binding],
                TestModelFactory.Now,
                TestModelFactory.Now.AddMinutes(-1)));
        Assert.Throws<ArgumentException>(
            () => new CorpusActivationRecord(
                manifest.CorpusId,
                new ActivationRecordRevision(1),
                null,
                manifest.IndexGenerationId,
                manifest.CatalogueRevision,
                digest,
                [],
                TestModelFactory.Now,
                TestModelFactory.Now));
    }

    [Fact]
    public void ActiveAndSingleRollbackGenerationProtectReachableContent()
    {
        var activeId = GenerationId('a');
        var rollbackId = GenerationId('b');
        var activeContent = new ContentObjectId(new string('c', 64));
        var sharedContent = new ContentObjectId(new string('d', 64));
        var rollbackContent = new ContentObjectId(new string('e', 64));
        var unreferenced = new ContentObjectId(new string('f', 64));
        var active = new GenerationRetentionReference(
            activeId,
            [activeContent, sharedContent, sharedContent]);
        var rollback = new GenerationRetentionReference(
            rollbackId,
            [sharedContent, rollbackContent]);
        var reachability = new RetentionReachability(active, rollback);

        Assert.Equal(2, active.ContentObjectIds.Count);
        Assert.True(reachability.IsGenerationProtected(activeId));
        Assert.True(reachability.IsGenerationProtected(rollbackId));
        Assert.False(reachability.IsGenerationProtected(GenerationId('9')));
        Assert.False(reachability.CanPhysicallyDelete(activeContent));
        Assert.False(reachability.CanPhysicallyDelete(rollbackContent));
        Assert.True(reachability.CanPhysicallyDelete(unreferenced));
        Assert.Throws<ArgumentException>(
            () => new GenerationRetentionReference(activeId, []));
        Assert.Throws<ArgumentException>(
            () => new RetentionReachability(active, active));
    }

    [Fact]
    public void RetentionCanProtectOnlyTheActiveGenerationBeforeRollbackExists()
    {
        var active = new GenerationRetentionReference(
            GenerationId('a'),
            [new ContentObjectId(new string('c', 64))]);
        var reachability = new RetentionReachability(active, rollbackGeneration: null);

        Assert.Null(reachability.RollbackGeneration);
        Assert.True(reachability.IsGenerationProtected(active.GenerationId));
    }

    private static FinalisedIndexGenerationManifest CreateManifest(
        int manifestSchemaVersion = 1,
        long chunkCount = 2,
        long vectorCount = 2,
        string? generationContentDigest = null,
        string? generationIdDigest = null)
    {
        var digest = generationContentDigest ?? new string('a', 64);

        return new FinalisedIndexGenerationManifest(
            manifestSchemaVersion,
            new CorpusId("mvp-database-documentation"),
            new CorpusRevision(1),
            new CatalogueRevision(1),
            new ActiveDocumentSetDigest(new string('b', 64)),
            new SourceBindingSetDigest(new string('c', 64)),
            new IndexCompatibilityKey(new string('d', 64)),
            new GenerationSpecDigest(new string('e', 64)),
            chunkCount,
            vectorCount,
            new LogicalArtifactDigest(new string('f', 64)),
            new GenerationContentDigest(digest),
            new IndexGenerationId($"idxgen-{generationIdDigest ?? digest}"));
    }

    private static IndexGenerationId GenerationId(char character) =>
        new($"idxgen-{new string(character, 64)}");
}
