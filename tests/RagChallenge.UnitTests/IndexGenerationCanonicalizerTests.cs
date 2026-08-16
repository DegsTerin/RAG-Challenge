// Purpose: Verifies deterministic, observation-independent generation specification, artefact, and manifest identity without involving persistence.
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.UnitTests;

public sealed class IndexGenerationCanonicalizerTests
{
    [Fact]
    public void ArtefactOrderDoesNotChangeFinalGenerationIdentity()
    {
        var specification = CreateSpecification();
        var first = CreateArtefact(0, "first", new float[] { 1, 0, 0 });
        var second = CreateArtefact(1, "second", new float[] { 0, 1, 0 });

        var ordered = IndexGenerationCanonicalizer.CreateFinalisedManifest(
            specification,
            [first, second]);
        var reversed = IndexGenerationCanonicalizer.CreateFinalisedManifest(
            specification,
            [second, first]);

        Assert.Equal(ordered.GenerationSpecDigest, reversed.GenerationSpecDigest);
        Assert.Equal(ordered.LogicalArtifactDigest, reversed.LogicalArtifactDigest);
        Assert.Equal(ordered.GenerationContentDigest, reversed.GenerationContentDigest);
        Assert.Equal(ordered.IndexGenerationId, reversed.IndexGenerationId);
    }

    [Fact]
    public void AnyLogicalPayloadChangeChangesFinalGenerationIdentity()
    {
        var specification = CreateSpecification();
        var baseline = IndexGenerationCanonicalizer.CreateFinalisedManifest(
            specification,
            [CreateArtefact(0, "baseline", new float[] { 1, 0, 0 })]);
        var changedText = IndexGenerationCanonicalizer.CreateFinalisedManifest(
            specification,
            [CreateArtefact(0, "changed", new float[] { 1, 0, 0 })]);
        var changedVector = IndexGenerationCanonicalizer.CreateFinalisedManifest(
            specification,
            [CreateArtefact(0, "baseline", new float[] { 0, 1, 0 })]);

        Assert.NotEqual(baseline.LogicalArtifactDigest, changedText.LogicalArtifactDigest);
        Assert.NotEqual(baseline.IndexGenerationId, changedText.IndexGenerationId);
        Assert.NotEqual(baseline.LogicalArtifactDigest, changedVector.LogicalArtifactDigest);
        Assert.NotEqual(baseline.IndexGenerationId, changedVector.IndexGenerationId);
    }

    [Fact]
    public void DuplicateOrdinalsAndNonFiniteVectorsFailClosed()
    {
        var first = CreateArtefact(0, "first", new float[] { 1, 0, 0 });
        var duplicate = CreateArtefact(0, "duplicate", new float[] { 0, 1, 0 });

        Assert.Throws<ArgumentException>(() =>
            IndexGenerationCanonicalizer.ComputeLogicalArtifactDigest(
                [first, duplicate]));
        Assert.Throws<ArgumentException>(() =>
            CreateArtefact(1, "invalid", new float[] { float.NaN, 0, 0 }));
    }

    private static IndexGenerationSpecification CreateSpecification() =>
        new(
            1,
            new CorpusId("fixture-corpus"),
            new CorpusRevision(1),
            new CatalogueRevision(1),
            new ActiveDocumentSetDigest(new string('a', 64)),
            new SourceBindingSetDigest(new string('b', 64)),
            new IndexCompatibilityKey(new string('c', 64)));

    private static LogicalIndexArtifact CreateArtefact(
        long ordinal,
        string text,
        float[] vector) =>
        new(
            ordinal,
            new DocumentId("doc-fixture"),
            new DocumentVersionNumber(1),
            new LogicalArtifactDigest(new string('d', 64)),
            text,
            vector);
}
