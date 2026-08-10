// Purpose: Models deterministic PDF page-image bindings and canonical render-manifest identity; byte rendering, signature verification, persistence, and serving remain outer-layer responsibilities.
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace RagChallenge.Domain.CorpusCatalog;

public sealed class DocumentPageImage
{
    public const string PngMediaType = "image/png";
    public const int MaximumDimensionPixels = 4096;

    public DocumentPageImage(
        DocumentId documentId,
        DocumentVersionNumber documentVersion,
        ContentObjectId sourceContentObjectId,
        int pageNumber,
        RenderProfileId renderProfileId,
        RendererDescriptor rendererDescriptor,
        ContentObjectId imageContentObjectId,
        ImageSha256 imageSha256,
        long byteLength,
        string mediaType,
        int widthPixels,
        int heightPixels,
        int? sourceRegionWidthPixels = null,
        int? sourceRegionHeightPixels = null,
        int? noticeRegionHeightPixels = null)
    {
        ArgumentNullException.ThrowIfNull(documentId);
        ArgumentNullException.ThrowIfNull(documentVersion);
        ArgumentNullException.ThrowIfNull(sourceContentObjectId);
        ArgumentNullException.ThrowIfNull(renderProfileId);
        ArgumentNullException.ThrowIfNull(rendererDescriptor);
        ArgumentNullException.ThrowIfNull(imageContentObjectId);
        ArgumentNullException.ThrowIfNull(imageSha256);

        if (pageNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber),
                pageNumber,
                "A physical PDF page number must be positive and one-based.");
        }

        if (byteLength <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(byteLength),
                byteLength,
                "A page-image content object must contain at least one byte.");
        }

        if (!string.Equals(mediaType, PngMediaType, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The accepted render profile produces exactly image/png.",
                nameof(mediaType));
        }

        if (widthPixels is <= 0 or > MaximumDimensionPixels)
        {
            throw new ArgumentOutOfRangeException(
                nameof(widthPixels),
                widthPixels,
                "A page-image width must be between 1 and 4,096 pixels.");
        }

        if (heightPixels is <= 0 or > MaximumDimensionPixels)
        {
            throw new ArgumentOutOfRangeException(
                nameof(heightPixels),
                heightPixels,
                "A page-image height must be between 1 and 4,096 pixels.");
        }

        if (!string.Equals(
                imageContentObjectId.Value,
                imageSha256.Value,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The image content identity must equal the SHA-256 of the exact PNG bytes.",
                nameof(imageSha256));
        }

        var isLegacy = string.Equals(
            renderProfileId.Value,
            RenderProfileId.PdfPagePngV1,
            StringComparison.Ordinal);
        var hasNoRegions = sourceRegionWidthPixels is null &&
            sourceRegionHeightPixels is null && noticeRegionHeightPixels is null;
        var hasNoticeRegions = sourceRegionWidthPixels == widthPixels &&
            sourceRegionHeightPixels is > 0 and <= MaximumDimensionPixels &&
            noticeRegionHeightPixels is > 0 and <= MaximumDimensionPixels &&
            sourceRegionHeightPixels + noticeRegionHeightPixels == heightPixels;

        if (isLegacy && !hasNoRegions || !isLegacy && !hasNoticeRegions)
        {
            throw new ArgumentException(
                "Page-image region measurements must match the selected render profile.");
        }

        DocumentId = documentId;
        DocumentVersion = documentVersion;
        SourceContentObjectId = sourceContentObjectId;
        PageNumber = pageNumber;
        RenderProfileId = renderProfileId;
        RendererDescriptor = rendererDescriptor;
        ImageContentObjectId = imageContentObjectId;
        ImageSha256 = imageSha256;
        ByteLength = byteLength;
        MediaType = mediaType;
        WidthPixels = widthPixels;
        HeightPixels = heightPixels;
        SourceRegionWidthPixels = sourceRegionWidthPixels;
        SourceRegionHeightPixels = sourceRegionHeightPixels;
        NoticeRegionHeightPixels = noticeRegionHeightPixels;
    }

    public DocumentId DocumentId { get; }

    public DocumentVersionNumber DocumentVersion { get; }

    public ContentObjectId SourceContentObjectId { get; }

    public int PageNumber { get; }

    public RenderProfileId RenderProfileId { get; }

    public RendererDescriptor RendererDescriptor { get; }

    public ContentObjectId ImageContentObjectId { get; }

    public ImageSha256 ImageSha256 { get; }

    public long ByteLength { get; }

    public string MediaType { get; }

    public int WidthPixels { get; }

    public int HeightPixels { get; }

    public int? SourceRegionWidthPixels { get; }

    public int? SourceRegionHeightPixels { get; }

    public int? NoticeRegionHeightPixels { get; }
}

public sealed class DocumentRenderManifest
{
    public const int CurrentSchemaVersion = 1;
    public const int NoticeBearingSchemaVersion = 2;

    private DocumentRenderManifest(
        DocumentId documentId,
        DocumentVersionNumber documentVersion,
        ContentObjectId sourceContentObjectId,
        int sourcePageCount,
        RenderProfileId renderProfileId,
        RendererDescriptor rendererDescriptor,
        DocumentPageImage[] orderedPageImages,
        DerivativeObligationSetId? obligationSetId,
        DerivativeObligationSetSha256? obligationSetSha256,
        ManifestSha256 manifestSha256,
        DateTimeOffset generatedAt)
    {
        SchemaVersion = renderProfileId.Value == RenderProfileId.PdfPagePngV1
            ? CurrentSchemaVersion
            : NoticeBearingSchemaVersion;
        DocumentId = documentId;
        DocumentVersion = documentVersion;
        SourceContentObjectId = sourceContentObjectId;
        SourcePageCount = sourcePageCount;
        RenderProfileId = renderProfileId;
        RendererDescriptor = rendererDescriptor;
        OrderedPageImages = Array.AsReadOnly(orderedPageImages);
        ObligationSetId = obligationSetId;
        ObligationSetSha256 = obligationSetSha256;
        ManifestSha256 = manifestSha256;
        RenderManifestId = RenderManifestId.FromManifestSha256(manifestSha256);
        GeneratedAt = generatedAt;
    }

    public int SchemaVersion { get; }

    public DocumentId DocumentId { get; }

    public DocumentVersionNumber DocumentVersion { get; }

    public ContentObjectId SourceContentObjectId { get; }

    public int SourcePageCount { get; }

    public RenderProfileId RenderProfileId { get; }

    public RendererDescriptor RendererDescriptor { get; }

    public IReadOnlyList<DocumentPageImage> OrderedPageImages { get; }

    public DerivativeObligationSetId? ObligationSetId { get; }

    public DerivativeObligationSetSha256? ObligationSetSha256 { get; }

    public ManifestSha256 ManifestSha256 { get; }

    public RenderManifestId RenderManifestId { get; }

    public DateTimeOffset GeneratedAt { get; }

    public static DocumentRenderManifest Create(
        DocumentId documentId,
        DocumentVersionNumber documentVersion,
        ContentObjectId sourceContentObjectId,
        int sourcePageCount,
        RenderProfileId renderProfileId,
        RendererDescriptor rendererDescriptor,
        IEnumerable<DocumentPageImage> orderedPageImages,
        DateTimeOffset generatedAt)
    {
        var pages = Validate(
            documentId,
            documentVersion,
            sourceContentObjectId,
            sourcePageCount,
            renderProfileId,
            rendererDescriptor,
            orderedPageImages,
            null,
            null,
            generatedAt);
        var digest = CanonicalDigest(
            documentId,
            documentVersion,
            sourceContentObjectId,
            sourcePageCount,
            renderProfileId,
            rendererDescriptor,
            pages,
            null,
            null);
        return new DocumentRenderManifest(
            documentId,
            documentVersion,
            sourceContentObjectId,
            sourcePageCount,
            renderProfileId,
            rendererDescriptor,
            pages,
            null,
            null,
            digest,
            generatedAt);
    }

    public static DocumentRenderManifest CreateNoticeBearing(
        DocumentId documentId,
        DocumentVersionNumber documentVersion,
        ContentObjectId sourceContentObjectId,
        int sourcePageCount,
        RendererDescriptor rendererDescriptor,
        DerivativeObligationSetV1 obligationSet,
        IEnumerable<DocumentPageImage> orderedPageImages,
        DateTimeOffset generatedAt)
    {
        ArgumentNullException.ThrowIfNull(obligationSet);
        var profile = new RenderProfileId(RenderProfileId.PdfPagePngNoticeV1);
        var pages = Validate(
            documentId,
            documentVersion,
            sourceContentObjectId,
            sourcePageCount,
            profile,
            rendererDescriptor,
            orderedPageImages,
            obligationSet.ObligationSetId,
            obligationSet.CanonicalSha256,
            generatedAt);
        var digest = CanonicalDigest(
            documentId,
            documentVersion,
            sourceContentObjectId,
            sourcePageCount,
            profile,
            rendererDescriptor,
            pages,
            obligationSet.ObligationSetId,
            obligationSet.CanonicalSha256);
        return new DocumentRenderManifest(
            documentId,
            documentVersion,
            sourceContentObjectId,
            sourcePageCount,
            profile,
            rendererDescriptor,
            pages,
            obligationSet.ObligationSetId,
            obligationSet.CanonicalSha256,
            digest,
            generatedAt);
    }

    public static DocumentRenderManifest Rehydrate(
        DocumentId documentId,
        DocumentVersionNumber documentVersion,
        ContentObjectId sourceContentObjectId,
        int sourcePageCount,
        RenderProfileId renderProfileId,
        RendererDescriptor rendererDescriptor,
        IEnumerable<DocumentPageImage> orderedPageImages,
        ManifestSha256 manifestSha256,
        DateTimeOffset generatedAt,
        DerivativeObligationSetId? obligationSetId = null,
        DerivativeObligationSetSha256? obligationSetSha256 = null)
    {
        ArgumentNullException.ThrowIfNull(manifestSha256);
        var pages = Validate(
            documentId,
            documentVersion,
            sourceContentObjectId,
            sourcePageCount,
            renderProfileId,
            rendererDescriptor,
            orderedPageImages,
            obligationSetId,
            obligationSetSha256,
            generatedAt);
        var digest = CanonicalDigest(
            documentId,
            documentVersion,
            sourceContentObjectId,
            sourcePageCount,
            renderProfileId,
            rendererDescriptor,
            pages,
            obligationSetId,
            obligationSetSha256);
        var manifest = new DocumentRenderManifest(
            documentId,
            documentVersion,
            sourceContentObjectId,
            sourcePageCount,
            renderProfileId,
            rendererDescriptor,
            pages,
            obligationSetId,
            obligationSetSha256,
            digest,
            generatedAt);

        if (manifest.ManifestSha256 != manifestSha256)
        {
            throw new InvalidDataException(
                "A persisted render-manifest digest does not match its canonical identity and measurements.");
        }

        return manifest;
    }

    private static DocumentPageImage[] Validate(
        DocumentId documentId,
        DocumentVersionNumber documentVersion,
        ContentObjectId sourceContentObjectId,
        int sourcePageCount,
        RenderProfileId renderProfileId,
        RendererDescriptor rendererDescriptor,
        IEnumerable<DocumentPageImage> orderedPageImages,
        DerivativeObligationSetId? obligationSetId,
        DerivativeObligationSetSha256? obligationSetSha256,
        DateTimeOffset generatedAt)
    {
        ArgumentNullException.ThrowIfNull(documentId);
        ArgumentNullException.ThrowIfNull(documentVersion);
        ArgumentNullException.ThrowIfNull(sourceContentObjectId);
        ArgumentNullException.ThrowIfNull(renderProfileId);
        ArgumentNullException.ThrowIfNull(rendererDescriptor);
        ArgumentNullException.ThrowIfNull(orderedPageImages);

        if (sourcePageCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sourcePageCount),
                sourcePageCount,
                "A final render manifest must cover at least one physical PDF page.");
        }

        if (generatedAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "A render-manifest generation instant must be expressed in UTC.",
                nameof(generatedAt));
        }

        var isLegacy = renderProfileId.Value == RenderProfileId.PdfPagePngV1;

        if (isLegacy && (obligationSetId is not null || obligationSetSha256 is not null) ||
            !isLegacy && (obligationSetId is null || obligationSetSha256 is null ||
                obligationSetId != DerivativeObligationSetId.FromSha256(obligationSetSha256)))
        {
            throw new ArgumentException(
                "A render manifest must carry obligation identity exactly when it uses the notice-bearing profile.");
        }

        var pages = orderedPageImages.ToArray();

        if (pages.Length != sourcePageCount)
        {
            throw new ArgumentException(
                "A final render manifest must contain exactly one image binding per source page.",
                nameof(orderedPageImages));
        }

        for (var index = 0; index < pages.Length; index++)
        {
            var page = pages[index] ?? throw new ArgumentException(
                "A render manifest cannot contain a null page-image binding.",
                nameof(orderedPageImages));

            if (page.PageNumber != index + 1)
            {
                throw new ArgumentException(
                    "Render-manifest pages must be unique, consecutive, one-based, and already ordered.",
                    nameof(orderedPageImages));
            }

            if (page.DocumentId != documentId ||
                page.DocumentVersion != documentVersion ||
                page.SourceContentObjectId != sourceContentObjectId ||
                page.RenderProfileId != renderProfileId ||
                page.RendererDescriptor != rendererDescriptor)
            {
                throw new ArgumentException(
                    "Every page image must bind the exact manifest document, source object, profile, and renderer descriptor.",
                    nameof(orderedPageImages));
            }
        }

        return pages;
    }

    private static ManifestSha256 CanonicalDigest(
        DocumentId documentId,
        DocumentVersionNumber documentVersion,
        ContentObjectId sourceContentObjectId,
        int sourcePageCount,
        RenderProfileId renderProfileId,
        RendererDescriptor rendererDescriptor,
        IReadOnlyList<DocumentPageImage> pages,
        DerivativeObligationSetId? obligationSetId,
        DerivativeObligationSetSha256? obligationSetSha256)
    {
        var isLegacy = renderProfileId.Value == RenderProfileId.PdfPagePngV1;
        var canonical = new StringBuilder();
        Append(canonical, "canonicalSchema", isLegacy
            ? "rag-render-manifest-v1"
            : "rag-render-manifest-v2");
        Append(canonical, "schemaVersion", (isLegacy
            ? CurrentSchemaVersion
            : NoticeBearingSchemaVersion).ToString(CultureInfo.InvariantCulture));
        Append(canonical, "documentId", documentId.Value);
        Append(canonical, "documentVersion", documentVersion.ToCanonicalString());
        Append(canonical, "sourceContentObjectId", sourceContentObjectId.Value);
        Append(canonical, "sourcePageCount", sourcePageCount.ToString(CultureInfo.InvariantCulture));
        Append(canonical, "renderProfileId", renderProfileId.Value);
        Append(canonical, "rendererDescriptor", rendererDescriptor.Value);

        if (!isLegacy)
        {
            Append(canonical, "obligationSetId", obligationSetId!.Value);
            Append(canonical, "obligationSetSha256", obligationSetSha256!.Value);
        }

        foreach (var page in pages)
        {
            Append(canonical, "page.documentId", page.DocumentId.Value);
            Append(canonical, "page.documentVersion", page.DocumentVersion.ToCanonicalString());
            Append(canonical, "page.sourceContentObjectId", page.SourceContentObjectId.Value);
            Append(canonical, "page.pageNumber", page.PageNumber.ToString(CultureInfo.InvariantCulture));
            Append(canonical, "page.renderProfileId", page.RenderProfileId.Value);
            Append(canonical, "page.rendererDescriptor", page.RendererDescriptor.Value);
            Append(canonical, "page.imageContentObjectId", page.ImageContentObjectId.Value);
            Append(canonical, "page.imageSha256", page.ImageSha256.Value);
            Append(canonical, "page.byteLength", page.ByteLength.ToString(CultureInfo.InvariantCulture));
            Append(canonical, "page.mediaType", page.MediaType);
            Append(canonical, "page.widthPixels", page.WidthPixels.ToString(CultureInfo.InvariantCulture));
            Append(canonical, "page.heightPixels", page.HeightPixels.ToString(CultureInfo.InvariantCulture));

            if (!isLegacy)
            {
                Append(canonical, "page.sourceRegionWidthPixels", page.SourceRegionWidthPixels!.Value.ToString(CultureInfo.InvariantCulture));
                Append(canonical, "page.sourceRegionHeightPixels", page.SourceRegionHeightPixels!.Value.ToString(CultureInfo.InvariantCulture));
                Append(canonical, "page.noticeRegionHeightPixels", page.NoticeRegionHeightPixels!.Value.ToString(CultureInfo.InvariantCulture));
            }
        }

        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString()));
        return new ManifestSha256(Convert.ToHexString(digest).ToLowerInvariant());
    }

    private static void Append(StringBuilder target, string name, string value)
    {
        target.Append(name);
        target.Append(':');
        target.Append(Encoding.UTF8.GetByteCount(value).ToString(CultureInfo.InvariantCulture));
        target.Append(':');
        target.Append(value);
        target.Append('\n');
    }
}
