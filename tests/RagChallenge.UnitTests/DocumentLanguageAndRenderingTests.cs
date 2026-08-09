// Purpose: Verifies fail-closed BCP 47 handling, v1 language eligibility, deterministic render-manifest identity, complete page bindings, and derivative reachability.
using RagChallenge.Application.Documents;
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.UnitTests;

public sealed class DocumentLanguageAndRenderingTests
{
    [Theory]
    [InlineData("en", "en")]
    [InlineData("EN-gb", "en-GB")]
    [InlineData("zh-hant-tw", "zh-Hant-TW")]
    [InlineData("de-DE-1901", "de-DE-1901")]
    [InlineData("en-u-ca-gregory", "en-u-ca-gregory")]
    [InlineData("x-PUBLISH-tag", "x-publish-tag")]
    public void DocumentLanguageCanonicalisesSyntaxWithoutInferringSubtags(
        string observed,
        string expected)
    {
        var language = new DocumentContentLanguage(observed);

        Assert.Equal(expected, language.CanonicalTag);
        Assert.Equal("en", new DocumentContentLanguage("en").CanonicalTag);
        Assert.NotEqual(DocumentContentLanguage.EnGb, new DocumentContentLanguage("en"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("e")]
    [InlineData("en--GB")]
    [InlineData("en_GB")]
    [InlineData("en-a")]
    [InlineData("en-GB-é")]
    public void DocumentLanguageRejectsMalformedOrNonAsciiValues(string value)
    {
        Assert.Throws<ArgumentException>(() => new DocumentContentLanguage(value));
    }

    [Fact]
    public void DocumentLanguageRejectsValuesBeyondTheBound()
    {
        Assert.Throws<ArgumentException>(
            () => new DocumentContentLanguage($"en-x-{new string('a', 124)}"));
    }

    [Fact]
    public void SourceDeclarationPreservesTheObservedTagAlongsideCanonicalComparison()
    {
        var declared = new SourceDeclaredLanguage("EN-gb");

        Assert.Equal("EN-gb", declared.ObservedTag);
        Assert.Equal("en-GB", declared.CanonicalTag);
        Assert.Equal("en", new SourceDeclaredLanguage("en").ObservedTag);
    }

    [Fact]
    public void BroaderDocumentLanguageRemainsExactAcrossCatalogueIndexAndQueryMetadata()
    {
        var language = new DocumentContentLanguage("en");
        var candidate = Document(language, CatalogueItemStatus.Candidate);
        var binding = new DocumentBinding(
            new DatabaseProductId("database-1"),
            new DatabaseProductRevision(1),
            new DocumentId("document-1"),
            new DocumentVersionNumber(1),
            DocumentFormat.Csv,
            new SourceAdapterId("local-csv"),
            SourceTrustClass.LocalAuthorised);

        Assert.Equal("en", candidate.ContentLanguage.CanonicalTag);
        Assert.Equal("en", Document(language, CatalogueItemStatus.Active)
            .ContentLanguage.ToCanonicalTag());
        Assert.Equal("en", new IndexDocumentInput(
            Binding(),
            language,
            [Chunk()],
            "parser-v1",
            new ChunkingPolicy()).ContentLanguage.ToCanonicalTag());
        var queryBinding = new QueryEvidenceBinding(
            binding,
            TestModelFactory.Evidence(binding),
            renderManifest: null,
            language,
            SourceFreshness.Local,
            sourceDeclaredLanguage: new SourceDeclaredLanguage("EN"));

        Assert.Equal("en", queryBinding.ContentLanguage.ToCanonicalTag());
        Assert.Equal("EN", queryBinding.SourceDeclaredLanguage!.ObservedTag);
    }

    [Fact]
    public void RenderManifestIdentityIsCanonicalAndExcludesOnlyGenerationTime()
    {
        var first = Manifest(TestModelFactory.Now);
        var second = Manifest(TestModelFactory.Now.AddMinutes(1));
        var changedMeasurement = CreateManifest(
            2,
            [Page(1, 'b', widthPixels: 1025), Page(2, 'c')],
            TestModelFactory.Now);

        Assert.Equal(first.ManifestSha256, second.ManifestSha256);
        Assert.NotEqual(first.ManifestSha256, changedMeasurement.ManifestSha256);
        Assert.Equal(
            $"rendermanifest-{first.ManifestSha256.Value}",
            first.RenderManifestId.Value);
        Assert.Equal(DocumentRenderManifest.CurrentSchemaVersion, first.SchemaVersion);
        Assert.Equal([1, 2], first.OrderedPageImages.Select(page => page.PageNumber));
    }

    [Fact]
    public void RenderManifestRejectsMissingDuplicateAndMismatchedPageBindings()
    {
        var first = Page(1, 'b');
        var second = Page(2, 'c');

        Assert.Throws<ArgumentException>(
            () => CreateManifest(2, [first]));
        Assert.Throws<ArgumentException>(
            () => CreateManifest(2, [first, Page(1, 'd')]));
        Assert.Throws<ArgumentException>(
            () => CreateManifest(2, [second, first]));
        Assert.Throws<ArgumentException>(
            () => CreateManifest(2, [first, Page(2, 'd', documentId: "other-document")]));
    }

    [Fact]
    public void PageImageRejectsInvalidPngIdentityDimensionsAndMeasurements()
    {
        Assert.Throws<ArgumentException>(
            () => Page(1, 'b', imageShaCharacter: 'c'));
        Assert.Throws<ArgumentException>(
            () => Page(1, 'b', mediaType: "image/jpeg"));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Page(1, 'b', widthPixels: 4097));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Page(1, 'b', heightPixels: 0));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Page(0, 'b'));
    }

    [Fact]
    public void RenderManifestSourcesAndImagesRemainReachable()
    {
        var manifest = Manifest(TestModelFactory.Now);
        var generation = new GenerationRetentionReference(
            new IndexGenerationId($"idxgen-{new string('e', 64)}"),
            [new ContentObjectId(new string('f', 64))]);
        var reachability = new RetentionReachability(
            generation,
            rollbackGeneration: null,
            [manifest]);

        Assert.False(reachability.CanPhysicallyDelete(manifest.SourceContentObjectId));
        Assert.All(
            manifest.OrderedPageImages,
            page => Assert.False(reachability.CanPhysicallyDelete(page.ImageContentObjectId)));
        Assert.True(reachability.CanPhysicallyDelete(new ContentObjectId(new string('9', 64))));
    }

    private static DocumentVersion Document(
        DocumentContentLanguage language,
        CatalogueItemStatus status) =>
        new(
            new DocumentId("document-1"),
            new DocumentVersionNumber(1),
            new DatabaseProductId("database-1"),
            new DatabaseProductRevision(1),
            DocumentFormat.Pdf,
            language,
            status,
            SourceContentId,
            100,
            "application/pdf",
            new SourceAdapterId("local-pdf"),
            SourceTrustClass.LocalAuthorised,
            sourceDeclaredLanguage: new SourceDeclaredLanguage("en"));

    private static DocumentBinding Binding() =>
        new(
            new DatabaseProductId("database-1"),
            new DatabaseProductRevision(1),
            new DocumentId("document-1"),
            new DocumentVersionNumber(1),
            DocumentFormat.Pdf,
            new SourceAdapterId("local-pdf"),
            SourceTrustClass.LocalAuthorised);

    private static DocumentChunk Chunk() =>
        new(
            0,
            new LogicalArtifactDigest(new string('a', 64)),
            "Evidence",
            pageNumber: 1,
            recordNumber: null,
            new Dictionary<string, string>());

    private static DocumentRenderManifest Manifest(DateTimeOffset generatedAt) =>
        CreateManifest(2, [Page(1, 'b'), Page(2, 'c')], generatedAt);

    private static DocumentRenderManifest CreateManifest(
        int sourcePageCount,
        IEnumerable<DocumentPageImage> pages,
        DateTimeOffset? generatedAt = null) =>
        DocumentRenderManifest.Create(
            new DocumentId("document-1"),
            new DocumentVersionNumber(1),
            SourceContentId,
            sourcePageCount,
            RenderProfile,
            Renderer,
            pages,
            generatedAt ?? TestModelFactory.Now);

    private static DocumentPageImage Page(
        int pageNumber,
        char imageCharacter,
        char? imageShaCharacter = null,
        string documentId = "document-1",
        string mediaType = DocumentPageImage.PngMediaType,
        int widthPixels = 1024,
        int heightPixels = 768)
    {
        var imageDigest = new string(imageCharacter, 64);
        return new DocumentPageImage(
            new DocumentId(documentId),
            new DocumentVersionNumber(1),
            SourceContentId,
            pageNumber,
            RenderProfile,
            Renderer,
            new ContentObjectId(imageDigest),
            new ImageSha256(new string(imageShaCharacter ?? imageCharacter, 64)),
            4096,
            mediaType,
            widthPixels,
            heightPixels);
    }

    private static readonly ContentObjectId SourceContentId =
        new(new string('a', 64));

    private static readonly RenderProfileId RenderProfile =
        new(RenderProfileId.PdfPagePngV1);

    private static readonly RendererDescriptor Renderer =
        new("renderer.synthetic:v1");
}
