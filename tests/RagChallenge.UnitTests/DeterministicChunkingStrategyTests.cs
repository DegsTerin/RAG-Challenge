// Purpose: Proves the accepted paragraph-window-v1 scalar, overlap, boundary, normalisation and compatibility-key contract without parser or provider execution.
using RagChallenge.Application.Documents;
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.UnitTests;

public sealed class DeterministicChunkingStrategyTests
{
    private static readonly DocumentChunkingContext Context = new(
        new CorpusId("chunking-corpus"),
        new DatabaseProductId("chunking-product"),
        new DatabaseProductRevision(1),
        new DocumentId("chunking-document"),
        new DocumentVersionNumber(1),
        DocumentFormat.Pdf,
        DocumentContentLanguage.EnGb,
        new SourceAdapterId("synthetic-pdf"),
        SourceTrustClass.LocalAuthorised);

    [Fact]
    public void DefaultPolicyMatchesTheAcceptedParagraphWindowContract()
    {
        var policy = new ChunkingPolicy();

        Assert.Equal("paragraph-window-v1", ChunkingPolicy.StrategyId);
        Assert.Equal(3_200, policy.TargetScalarCount);
        Assert.Equal(480, policy.OverlapScalarCount);
        Assert.Equal(4_000, policy.HardMaximumScalarCount);
        Assert.Equal(
            "paragraph-window-v1;target-scalars=3200;overlap-scalars=480;hard-max-scalars=4000;boundaries=section,paragraph,sentence,word,scalar;separator=lf-paragraph-v1;normalisation=nfc-lf-horizontal-space-control-space-v1;unit=pdf-page-or-csv-record;digest-schema=rag-chunk-v3",
            policy.CompatibilityDescriptor);
        Assert.Equal("rag-chunk-v3", ChunkingPolicy.DigestSchema);
        Assert.Contains(
            $"digest-schema={ChunkingPolicy.DigestSchema}",
            policy.CompatibilityDescriptor,
            StringComparison.Ordinal);
    }

    [Fact]
    public void PolicyRejectsAnOverlapThatCannotGuaranteeForwardProgress()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChunkingPolicy(32, 16, 40));
    }

    [Fact]
    public void NonBmpTextUsesScalarLimitsAndAnExactScalarOverlap()
    {
        var text = string.Concat(Enumerable.Repeat("😀", 5_000));
        var chunks = Chunk(Artifact(Page(0, text, 1)), new ChunkingPolicy());

        Assert.Equal(2, chunks.Count);
        Assert.Equal(3_200, ScalarCount(chunks[0].Text));
        Assert.Equal(2_280, ScalarCount(chunks[1].Text));
        Assert.All(chunks, chunk => Assert.InRange(ScalarCount(chunk.Text), 1, 4_000));
        Assert.Equal(
            chunks[0].Text.EnumerateRunes().TakeLast(480),
            chunks[1].Text.EnumerateRunes().Take(480));
        Assert.DoesNotContain('\uFFFD', string.Concat(chunks.Select(chunk => chunk.Text)));
    }

    [Fact]
    public void SectionBoundaryTakesPrecedenceWithinTheAcceptedWindow()
    {
        var text = new string('a', 25) + ".\n" + new string('b', 8) +
            "\n\n" + new string('c', 60);
        var chunks = Chunk(
            Artifact(Page(0, text, 1)),
            new ChunkingPolicy(32, 8, 40));

        Assert.True(chunks.Count >= 2);
        Assert.Equal(37, ScalarCount(chunks[0].Text));
        Assert.EndsWith("\n\n", chunks[0].Text, StringComparison.Ordinal);
    }

    [Fact]
    public void ChunksNeverCrossParsedPageUnits()
    {
        var pageOne = "PAGE-ONE " + new string('a', 100);
        var pageTwo = "PAGE-TWO " + new string('b', 100);
        var chunks = Chunk(
            Artifact(Page(0, pageOne, 1), Page(1, pageTwo, 2)),
            new ChunkingPolicy(32, 8, 40));

        Assert.Contains(chunks, chunk => chunk.PageNumber == 1);
        Assert.Contains(chunks, chunk => chunk.PageNumber == 2);
        Assert.All(chunks.Where(chunk => chunk.PageNumber == 1), chunk =>
            Assert.DoesNotContain("PAGE-TWO", chunk.Text, StringComparison.Ordinal));
        Assert.All(chunks.Where(chunk => chunk.PageNumber == 2), chunk =>
            Assert.DoesNotContain("PAGE-ONE", chunk.Text, StringComparison.Ordinal));
    }

    [Fact]
    public void ChunksNeverCrossCsvRecordUnitsAndPreserveRecordMetadata()
    {
        var csvContext = Context with
        {
            DocumentFormat = DocumentFormat.Csv,
            SourceAdapterId = new SourceAdapterId("synthetic-csv"),
        };
        var artifact = new ParsedDocumentArtifact(
            DocumentFormat.Csv,
            "synthetic-csv/1",
            [
                new ParsedDocumentUnit(
                    0,
                    "RECORD-ONE " + new string('a', 100),
                    recordNumber: 1,
                    columns: new Dictionary<string, string> { ["id"] = "one" }),
                new ParsedDocumentUnit(
                    1,
                    "RECORD-TWO " + new string('b', 100),
                    recordNumber: 2,
                    columns: new Dictionary<string, string> { ["id"] = "two" }),
            ]);
        var chunks = new DeterministicChunkingStrategy().Chunk(
            artifact,
            csvContext,
            new ChunkingPolicy(32, 8, 40));

        Assert.Contains(chunks, chunk => chunk.RecordNumber == 1);
        Assert.Contains(chunks, chunk => chunk.RecordNumber == 2);
        Assert.All(chunks.Where(chunk => chunk.RecordNumber == 1), chunk =>
        {
            Assert.Equal("one", chunk.Columns["id"]);
            Assert.DoesNotContain("RECORD-TWO", chunk.Text, StringComparison.Ordinal);
        });
        Assert.All(chunks.Where(chunk => chunk.RecordNumber == 2), chunk =>
        {
            Assert.Equal("two", chunk.Columns["id"]);
            Assert.DoesNotContain("RECORD-ONE", chunk.Text, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void CsvColumnMetadataHasCanonicalChunkIdentity()
    {
        var csvContext = Context with
        {
            DocumentFormat = DocumentFormat.Csv,
            SourceAdapterId = new SourceAdapterId("synthetic-csv"),
        };
        var first = CsvArtifact(new Dictionary<string, string>
        {
            ["id"] = "one",
            ["name"] = "Alpha",
        });
        var reordered = CsvArtifact(new Dictionary<string, string>
        {
            ["name"] = "Alpha",
            ["id"] = "one",
        });
        var changed = CsvArtifact(new Dictionary<string, string>
        {
            ["id"] = "one",
            ["name"] = "Beta",
        });
        var strategy = new DeterministicChunkingStrategy();
        var policy = new ChunkingPolicy(64, 8, 80);

        var firstChunk = Assert.Single(strategy.Chunk(first, csvContext, policy));
        var reorderedChunk = Assert.Single(strategy.Chunk(reordered, csvContext, policy));
        var changedChunk = Assert.Single(strategy.Chunk(changed, csvContext, policy));

        Assert.Equal(firstChunk.Digest, reorderedChunk.Digest);
        Assert.NotEqual(firstChunk.Digest, changedChunk.Digest);
        Assert.Equal("Alpha", firstChunk.Columns["name"]);
    }

    [Fact]
    public void NormalisationAndDigestAreStableGoldenEvidence()
    {
        var artifact = Artifact(Page(0, "Cafe\u0301\r\n\r\nAlpha\tBeta\u0001Gamma", 1));
        var policy = new ChunkingPolicy(64, 8, 80);

        var first = Assert.Single(Chunk(artifact, policy));
        var replay = Assert.Single(Chunk(artifact, policy));

        Assert.Equal("Café\n\nAlpha Beta Gamma", first.Text);
        Assert.Equal(first.Digest, replay.Digest);
        Assert.Equal(
            "79b0674e60d48c96e7e03abcd452d94d43958405c783d47e49f46e2bac542f54",
            first.Digest.Value);
    }

    [Fact]
    public void EveryCompatibilityInputChangesTheIndexKey()
    {
        var embedding = new EmbeddingProviderDescriptor("fake", "model", "revision-1", 3);
        var baseline = Profile(["parser-a/1"], new ChunkingPolicy(), embedding, "vector/1");
        var variants = new[]
        {
            Profile(["parser-a/2"], new ChunkingPolicy(), embedding, "vector/1"),
            Profile(["parser-a/1"], new ChunkingPolicy(3_201, 480, 4_000), embedding, "vector/1"),
            Profile(
                ["parser-a/1"],
                new ChunkingPolicy(),
                new EmbeddingProviderDescriptor("fake", "model", "revision-2", 3),
                "vector/1"),
            Profile(["parser-a/1"], new ChunkingPolicy(), embedding, "vector/2"),
        };

        Assert.All(variants, variant => Assert.NotEqual(baseline.Key, variant.Key));
        Assert.Equal(variants.Length, variants.Select(variant => variant.Key).Distinct().Count());
    }

    private static IReadOnlyList<DocumentChunk> Chunk(
        ParsedDocumentArtifact artifact,
        ChunkingPolicy policy) =>
        new DeterministicChunkingStrategy().Chunk(artifact, Context, policy);

    private static ParsedDocumentArtifact Artifact(params ParsedDocumentUnit[] units) =>
        new(DocumentFormat.Pdf, "synthetic-pdf/1", units);

    private static ParsedDocumentArtifact CsvArtifact(
        IReadOnlyDictionary<string, string> columns) =>
        new(
            DocumentFormat.Csv,
            "synthetic-csv/1",
            [new ParsedDocumentUnit(0, "stable text", recordNumber: 1, columns: columns)]);

    private static ParsedDocumentUnit Page(int ordinal, string text, int pageNumber) =>
        new(ordinal, text, pageNumber: pageNumber);

    private static int ScalarCount(string value) => value.EnumerateRunes().Count();

    private static IndexCompatibilityProfile Profile(
        IEnumerable<string> parsers,
        ChunkingPolicy policy,
        EmbeddingProviderDescriptor embedding,
        string vector) =>
        new(parsers, policy, embedding, vector);
}
