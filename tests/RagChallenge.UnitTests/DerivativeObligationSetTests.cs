// Purpose: Verifies deterministic obligation identity, exact ten-right mapping, notice-bearing manifest binding, and legacy-profile isolation without persistence or runtime behaviour.
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.UnitTests;

public sealed class DerivativeObligationSetTests
{
    [Fact]
    public void CanonicalIdentityBindsEveryRightAndExactOrderedNoticeText()
    {
        var rights = Rights();
        var first = Obligations(rights);
        var repeat = Obligations(rights);
        var changed = DerivativeObligationSetV1.Create(
            rights,
            SourceId,
            rights.Decisions.Select(decision => decision.EvidenceReference),
            DocumentContentLanguage.EnGb,
            "Synthetic Publisher",
            "Synthetic Reference",
            "1.0",
            "synthetic-source-v1",
            "Synthetic attribution.",
            "Synthetic copyright notice.",
            "Changed permission notice.",
            ["First disclaimer.", "Second disclaimer."],
            DerivativeTrademarkTreatment.NotApplicable,
            "NotApplicable: no trademark applies to the synthetic fixture.",
            "Rendered synthetic derivative; source-page pixels are unchanged.",
            TestModelFactory.Now,
            "assessor-synthetic-v1");

        Assert.Equal(first.ObligationSetId, repeat.ObligationSetId);
        Assert.Equal(first.CanonicalSha256, repeat.CanonicalSha256);
        Assert.NotEqual(first.ObligationSetId, changed.ObligationSetId);
        Assert.True(first.MatchesRights(rights));
        Assert.Equal(
            $"obligationset-{first.CanonicalSha256.Value}",
            first.ObligationSetId.Value);

        var changedRights = Rights(DocumentRight.RuntimeDerivativeImageDisplay);
        Assert.False(first.MatchesRights(changedRights));
    }

    [Fact]
    public void NoticeManifestRequiresExactObligationIdentityAndRegionMeasurements()
    {
        var obligations = Obligations(Rights());
        var profile = new RenderProfileId(RenderProfileId.PdfPagePngNoticeV1);
        var renderer = new RendererDescriptor("notice-png-v1:synthetic");
        var digest = new string('b', 64);
        var page = new DocumentPageImage(
            DocumentId,
            DocumentVersion,
            SourceId,
            1,
            profile,
            renderer,
            new ContentObjectId(digest),
            new ImageSha256(digest),
            4096,
            DocumentPageImage.PngMediaType,
            100,
            180,
            sourceRegionWidthPixels: 100,
            sourceRegionHeightPixels: 120,
            noticeRegionHeightPixels: 60);
        var manifest = DocumentRenderManifest.CreateNoticeBearing(
            DocumentId,
            DocumentVersion,
            SourceId,
            1,
            renderer,
            obligations,
            [page],
            TestModelFactory.Now);

        Assert.Equal(DocumentRenderManifest.NoticeBearingSchemaVersion, manifest.SchemaVersion);
        Assert.Equal(obligations.ObligationSetId, manifest.ObligationSetId);
        Assert.Throws<ArgumentException>(() => new DocumentPageImage(
            DocumentId,
            DocumentVersion,
            SourceId,
            1,
            profile,
            renderer,
            new ContentObjectId(digest),
            new ImageSha256(digest),
            4096,
            DocumentPageImage.PngMediaType,
            100,
            180));
        Assert.Throws<ArgumentException>(() => DocumentRenderManifest.Create(
            DocumentId,
            DocumentVersion,
            SourceId,
            1,
            profile,
            renderer,
            [page],
            TestModelFactory.Now));
    }

    [Fact]
    public void NoticeManifestSelectionPersistsOneCitedPageWithoutClaimingCompleteness()
    {
        var obligations = Obligations(Rights());
        var profile = new RenderProfileId(RenderProfileId.PdfPagePngNoticeV1);
        var renderer = new RendererDescriptor("notice-png-v1:selective");
        var digest = new string('c', 64);
        var page = new DocumentPageImage(
            DocumentId,
            DocumentVersion,
            SourceId,
            2,
            profile,
            renderer,
            new ContentObjectId(digest),
            new ImageSha256(digest),
            4096,
            DocumentPageImage.PngMediaType,
            100,
            180,
            sourceRegionWidthPixels: 100,
            sourceRegionHeightPixels: 120,
            noticeRegionHeightPixels: 60);
        var manifest = DocumentRenderManifest.CreateNoticeBearingSelection(
            DocumentId,
            DocumentVersion,
            SourceId,
            sourcePageCount: 3,
            renderer,
            obligations,
            [page],
            TestModelFactory.Now);
        var reopened = DocumentRenderManifest.Rehydrate(
            manifest.DocumentId,
            manifest.DocumentVersion,
            manifest.SourceContentObjectId,
            manifest.SourcePageCount,
            manifest.RenderProfileId,
            manifest.RendererDescriptor,
            manifest.OrderedPageImages,
            manifest.ManifestSha256,
            manifest.GeneratedAt,
            manifest.ObligationSetId,
            manifest.ObligationSetSha256);

        Assert.False(manifest.IsComplete);
        Assert.Equal(2, Assert.Single(reopened.OrderedPageImages).PageNumber);
        Assert.Equal(manifest.RenderManifestId, reopened.RenderManifestId);
    }

    private static DocumentRightsEligibilityRecordV1 Rights(DocumentRight? denied = null) =>
        new(
            DocumentId,
            DocumentVersion,
            Enum.GetValues<DocumentRight>().Select(right => new DocumentRightDecision(
                right,
                right == denied
                    ? DocumentRightDecisionState.Denied
                    : DocumentRightDecisionState.Permitted,
                new DocumentRightsEvidenceReference($"rights-synthetic-{right}"))));

    private static DerivativeObligationSetV1 Obligations(
        DocumentRightsEligibilityRecordV1 rights) =>
        DerivativeObligationSetV1.Create(
            rights,
            SourceId,
            rights.Decisions.Select(decision => decision.EvidenceReference),
            DocumentContentLanguage.EnGb,
            "Synthetic Publisher",
            "Synthetic Reference",
            "1.0",
            "synthetic-source-v1",
            "Synthetic attribution.",
            "Synthetic copyright notice.",
            "Synthetic permission notice.",
            ["First disclaimer.", "Second disclaimer."],
            DerivativeTrademarkTreatment.NotApplicable,
            "NotApplicable: no trademark applies to the synthetic fixture.",
            "Rendered synthetic derivative; source-page pixels are unchanged.",
            TestModelFactory.Now,
            "assessor-synthetic-v1");

    private static readonly DocumentId DocumentId = new("document-synthetic-notice");
    private static readonly DocumentVersionNumber DocumentVersion = new(1);
    private static readonly ContentObjectId SourceId = new(new string('a', 64));
}
