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
        int heightPixels)
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
}

public sealed class DocumentRenderManifest
{
    public const int CurrentSchemaVersion = 1;

    private DocumentRenderManifest(
        DocumentId documentId,
        DocumentVersionNumber documentVersion,
        ContentObjectId sourceContentObjectId,
        int sourcePageCount,
        RenderProfileId renderProfileId,
        RendererDescriptor rendererDescriptor,
        DocumentPageImage[] orderedPageImages,
        ManifestSha256 manifestSha256,
        DateTimeOffset generatedAt)
    {
        SchemaVersion = CurrentSchemaVersion;
        DocumentId = documentId;
        DocumentVersion = documentVersion;
        SourceContentObjectId = sourceContentObjectId;
        SourcePageCount = sourcePageCount;
        RenderProfileId = renderProfileId;
        RendererDescriptor = rendererDescriptor;
        OrderedPageImages = Array.AsReadOnly(orderedPageImages);
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
            generatedAt);
        var digest = CanonicalDigest(
            documentId,
            documentVersion,
            sourceContentObjectId,
            sourcePageCount,
            renderProfileId,
            rendererDescriptor,
            pages);
        return new DocumentRenderManifest(
            documentId,
            documentVersion,
            sourceContentObjectId,
            sourcePageCount,
            renderProfileId,
            rendererDescriptor,
            pages,
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
        DateTimeOffset generatedAt)
    {
        ArgumentNullException.ThrowIfNull(manifestSha256);
        var manifest = Create(
            documentId,
            documentVersion,
            sourceContentObjectId,
            sourcePageCount,
            renderProfileId,
            rendererDescriptor,
            orderedPageImages,
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
        IReadOnlyList<DocumentPageImage> pages)
    {
        var canonical = new StringBuilder();
        Append(canonical, "canonicalSchema", "rag-render-manifest-v1");
        Append(canonical, "schemaVersion", CurrentSchemaVersion.ToString(CultureInfo.InvariantCulture));
        Append(canonical, "documentId", documentId.Value);
        Append(canonical, "documentVersion", documentVersion.ToCanonicalString());
        Append(canonical, "sourceContentObjectId", sourceContentObjectId.Value);
        Append(canonical, "sourcePageCount", sourcePageCount.ToString(CultureInfo.InvariantCulture));
        Append(canonical, "renderProfileId", renderProfileId.Value);
        Append(canonical, "rendererDescriptor", rendererDescriptor.Value);

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
