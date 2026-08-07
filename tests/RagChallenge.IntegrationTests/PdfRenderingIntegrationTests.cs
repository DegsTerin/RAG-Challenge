// Purpose: Verifies the selected native renderer, worker boundary, PNG policy and existing-schema manifest persistence with synthetic PDF and image bytes only.
using System.Buffers.Binary;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

using RagChallenge.Application.Documents;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Infrastructure.Documents;

using SkiaSharp;

namespace RagChallenge.IntegrationTests;

[Collection(ParserRuntimeGateSerialisation.Name)]
public sealed class PdfRenderingIntegrationTests
{
    [Fact]
    public void RendererProducesRepeatableRgbPagesAt144DpiWithDeclaredRotation()
    {
        var policy = Policy(maximumPages: 2);
        var pdf = CreatePdf(
            new PageSpec(72, 36),
            new PageSpec(72, 36, Rotation: 90));
        var first = PdfToImagePdfPageRenderer.Render(
            pdf,
            policy,
            RuntimeInformation.RuntimeIdentifier);
        var repeat = PdfToImagePdfPageRenderer.Render(
            pdf,
            policy,
            RuntimeInformation.RuntimeIdentifier);
        var validator = new PngPageImageValidator();
        var firstPage = validator.Validate(first.Pages[0], policy);
        var rotatedPage = validator.Validate(first.Pages[1], policy);

        Assert.Equal((144, 72), (firstPage.WidthPixels, firstPage.HeightPixels));
        Assert.Equal((72, 144), (rotatedPage.WidthPixels, rotatedPage.HeightPixels));
        Assert.Equal(first.RendererDescriptor, repeat.RendererDescriptor);
        Assert.Equal(
            first.Pages.Select(page => page.PngBytes.ToArray()),
            repeat.Pages.Select(page => page.PngBytes.ToArray()),
            ByteArrayComparer.Instance);

        using var decoded = SKBitmap.Decode(first.Pages[0].PngBytes.ToArray());
        Assert.NotNull(decoded);
        Assert.Equal(SKColors.White, decoded.GetPixel(0, 0));
    }

    [Fact]
    public void RendererAccepts4096AndRejects4097BeforeRasterisation()
    {
        var policy = Policy(maximumPages: 1, maximumPixels: 100_000);
        var accepted = PdfToImagePdfPageRenderer.Render(
            CreatePdf(new PageSpec(2048, 10)),
            policy,
            RuntimeInformation.RuntimeIdentifier);
        var validation = new PngPageImageValidator().Validate(accepted.Pages[0], policy);

        Assert.Equal(4096, validation.WidthPixels);
        var exception = Assert.Throws<PdfRenderException>(() =>
            PdfToImagePdfPageRenderer.Render(
                CreatePdf(new PageSpec(2048.5, 10)),
                policy,
                RuntimeInformation.RuntimeIdentifier));
        Assert.Equal(PdfRenderFailureKind.LimitExceeded, exception.FailureKind);
    }

    [Fact]
    public void AnnotationAndFormAppearanceAreNotRendered()
    {
        var policy = Policy(maximumPages: 1);
        var blank = PdfToImagePdfPageRenderer.Render(
            CreatePdf(new PageSpec(72, 36)),
            policy,
            RuntimeInformation.RuntimeIdentifier);
        var widget = PdfToImagePdfPageRenderer.Render(
            CreatePdf(new PageSpec(72, 36, WithWidgetAppearance: true)),
            policy,
            RuntimeInformation.RuntimeIdentifier);

        Assert.Equal(blank.Pages[0].PngBytes.ToArray(), widget.Pages[0].PngBytes.ToArray());
    }

    [Fact]
    public void PngValidatorRejectsCrcMetadataTransparencyAndUnknownChunks()
    {
        var policy = Policy(maximumPages: 1);
        var rendered = PdfToImagePdfPageRenderer.Render(
            CreatePdf(new PageSpec(72, 36)),
            policy,
            RuntimeInformation.RuntimeIdentifier);
        var valid = rendered.Pages[0];
        var validator = new PngPageImageValidator();

        Assert.Equal(8, ReadIhdr(valid.PngBytes.Span).BitDepth);
        Assert.Equal(2, ReadIhdr(valid.PngBytes.Span).ColourType);
        Assert.Throws<PdfRenderException>(() => validator.Validate(
            ReplaceIhdrColourType(valid, 6),
            policy));
        Assert.Throws<PdfRenderException>(() => validator.Validate(
            InsertChunk(valid, "tEXt", Encoding.ASCII.GetBytes("key\0value")),
            policy));
        Assert.Throws<PdfRenderException>(() => validator.Validate(
            InsertChunk(valid, "vpAg", new byte[] { 1 }),
            policy));

        var corrupt = valid.PngBytes.ToArray();
        corrupt[^1] ^= 1;
        Assert.Throws<PdfRenderException>(() => validator.Validate(
            new RenderedPdfPageCandidate(
                valid.PageNumber,
                valid.SourceWidthPoints,
                valid.SourceHeightPoints,
                corrupt),
            policy));
    }

    [Fact]
    public async Task ExistingServerExecutableRunsOneWorkerWithoutStartingHttp()
    {
        var policy = Policy(maximumPages: 1);
        var pdf = CreatePdf(new PageSpec(72, 36));
        var id = ContentId(pdf);
        await using var verified = new VerifiedContentObject(
            id,
            id,
            pdf.Length,
            new MemoryStream(pdf, writable: false),
            ContentVerificationOutcome.Verified);
        var renderer = CreateWorkerRenderer();

        var result = await renderer.RenderAsync(verified, policy);

        var page = Assert.Single(result.Pages);
        var validation = new PngPageImageValidator().Validate(page, policy);
        Assert.Equal((144, 72), (validation.WidthPixels, validation.HeightPixels));
        Assert.Equal(renderer.Describe(policy), result.RendererDescriptor);
    }

    [Fact]
    public async Task WorkerAndFinaliserPublishACompleteCandidateEndToEnd()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var pdf = CreatePdf(new PageSpec(72, 36), new PageSpec(36, 72));
        var document = await CommitPdfCatalogueAsync(fixture, pdf);
        var rights = new DocumentRightsEligibilityRecordV1(
            document.Id,
            document.Version,
            Enum.GetValues<DocumentRight>().Select(right => new DocumentRightDecision(
                right,
                DocumentRightDecisionState.Permitted,
                new DocumentRightsEvidenceReference($"rights-e2e-{right}"))));
        var service = new DocumentRenderCandidateService(
            fixture.ContentStore,
            CreateWorkerRenderer(),
            new PngPageImageValidator(),
            fixture.ControlStore);
        var request = new DocumentRenderCandidateRequest(
            SqlitePersistenceFixture.CorpusId,
            document.Id,
            document.Version,
            document.ContentObjectId,
            document.ByteLength,
            rights,
            Policy(),
            new DateTimeOffset(2026, 8, 7, 16, 0, 0, TimeSpan.Zero));

        var applied = await service.FinaliseAsync(request);
        var replay = await service.FinaliseAsync(request);

        Assert.Equal(StoreMutationOutcome.Applied, applied.Outcome);
        Assert.Equal(StoreMutationOutcome.AlreadyApplied, replay.Outcome);
        Assert.Equal(2, applied.Manifest.OrderedPageImages.Count);
        Assert.Equal(1, await fixture.ScalarAsync("SELECT COUNT(*) FROM document_render_manifests;"));
        Assert.Equal(2, await fixture.ScalarAsync("SELECT COUNT(*) FROM document_page_images;"));

        foreach (var page in applied.Manifest.OrderedPageImages)
        {
            await using var reopened = await fixture.ContentStore.OpenVerifiedAsync(
                page.ImageContentObjectId,
                new ExpectedHashAndLength(page.ImageContentObjectId, page.ByteLength));
            Assert.Equal(0, reopened.Content.Position);
        }
    }

    [Fact]
    public async Task WorkerBoundaryFailsClosedForCancellationCrashAndTruncatedFrames()
    {
        var policy = Policy(maximumPages: 1);
        var pdf = CreatePdf(new PageSpec(72, 36));
        var id = ContentId(pdf);
        await using var cancelledSource = new VerifiedContentObject(
            id,
            id,
            pdf.Length,
            new MemoryStream(pdf, writable: false),
            ContentVerificationOutcome.Verified);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var cancelled = await Assert.ThrowsAsync<PdfRenderException>(() =>
            CreateWorkerRenderer().RenderAsync(cancelledSource, policy, cancellation.Token));
        Assert.Equal(PdfRenderFailureKind.Cancelled, cancelled.FailureKind);

        await using var crashSource = new VerifiedContentObject(
            id,
            id,
            pdf.Length,
            new MemoryStream(pdf, writable: false),
            ContentVerificationOutcome.Verified);
        var crashing = new IsolatedPdfRendererProcess(new RendererWorkerLaunch(
            DotnetHostPath(),
            [Path.Combine(Path.GetTempPath(), "missing-render-worker.dll")],
            RuntimeInformation.RuntimeIdentifier));
        await Assert.ThrowsAsync<PdfRenderException>(() =>
            crashing.RenderAsync(crashSource, policy));

        await Assert.ThrowsAsync<EndOfStreamException>(() =>
            PdfRenderWorkerProtocol.ReadResponseAsync(
                new MemoryStream(new byte[] { 1, 2, 3 }),
                policy,
                CancellationToken.None));
        await Assert.ThrowsAsync<PdfRenderException>(() =>
            PdfRenderWorkerProtocol.ReadResponseAsync(
                new MemoryStream(new byte[12]),
                policy,
                CancellationToken.None));
        using var output = new MemoryStream();
        var exitCode = await PdfRenderWorker.RunAsync(
            new MemoryStream(new byte[] { 1, 2, 3 }),
            output);
        Assert.NotEqual(0, exitCode);
        Assert.Equal(12, output.Length);
    }

    [Fact]
    public async Task WorkerMemoryLimitFailsClosedBeforeRenderingCompletes()
    {
        var policy = new PdfRenderPolicy(
            maximumSourceByteLength: 1_000_000,
            maximumPageCount: 1,
            maximumTotalPixels: 100_000,
            maximumPageOutputByteLength: 1_000_000,
            maximumTotalOutputByteLength: 1_000_000,
            maximumWorkerMemoryBytes: 1_000_000,
            maximumWorkerCpuTime: TimeSpan.FromSeconds(5),
            workerTimeout: TimeSpan.FromSeconds(10));
        var pdf = CreatePdf(new PageSpec(72, 36));
        var id = ContentId(pdf);
        await using var source = new VerifiedContentObject(
            id,
            id,
            pdf.Length,
            new MemoryStream(pdf, writable: false),
            ContentVerificationOutcome.Verified);

        await Assert.ThrowsAsync<PdfRenderException>(() =>
            CreateWorkerRenderer().RenderAsync(source, policy));
    }

    [Fact]
    public async Task ManifestCommitIsAtomicReadableIdempotentAndConflictAware()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var catalogue = await fixture.CommitLocalCatalogueAsync("synthetic source bytes");
        var document = Assert.Single(catalogue.Snapshot.DocumentVersions);
        var renderer = PdfPagePngV1RendererIdentity.CreateDescriptor(Policy(), "win-x64");
        var first = Manifest(document, renderer, new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 });

        var applied = await fixture.ControlStore.CommitAsync(
            new RenderManifestCommitRequest(SqlitePersistenceFixture.CorpusId, first));
        var replay = await fixture.ControlStore.CommitAsync(
            new RenderManifestCommitRequest(SqlitePersistenceFixture.CorpusId, first));
        var readback = await fixture.ControlStore.ReadAsync(
            SqlitePersistenceFixture.CorpusId,
            first.RenderManifestId);
        var conflictManifest = Manifest(
            document,
            renderer,
            new byte[] { 7, 8, 9 },
            new byte[] { 10, 11, 12 });
        var conflict = await fixture.ControlStore.CommitAsync(
            new RenderManifestCommitRequest(
                SqlitePersistenceFixture.CorpusId,
                conflictManifest));

        Assert.Equal(StoreMutationOutcome.Applied, applied.Outcome);
        Assert.Equal(StoreMutationOutcome.AlreadyApplied, replay.Outcome);
        Assert.Equal(StoreMutationOutcome.RevisionConflict, conflict.Outcome);
        Assert.NotNull(readback);
        Assert.Equal(first.ManifestSha256, readback.ManifestSha256);
        Assert.Equal(1, await fixture.ScalarAsync("SELECT COUNT(*) FROM document_render_manifests;"));
        Assert.Equal(2, await fixture.ScalarAsync("SELECT COUNT(*) FROM document_page_images;"));
    }

    [Fact]
    public async Task ManifestFailureLeavesNoPartialManifestOrPageBindings()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var catalogue = await fixture.CommitLocalCatalogueAsync("synthetic source bytes");
        var document = Assert.Single(catalogue.Snapshot.DocumentVersions);
        var renderer = PdfPagePngV1RendererIdentity.CreateDescriptor(Policy(), "win-x64");
        var repeatedImage = ContentId(new byte[] { 1, 2, 3 });
        var pages = new[]
        {
            Page(document, renderer, repeatedImage, pageNumber: 1, byteLength: 3),
            Page(document, renderer, repeatedImage, pageNumber: 2, byteLength: 4),
        };
        var invalidPersistenceCandidate = DocumentRenderManifest.Create(
            document.Id,
            document.Version,
            document.ContentObjectId,
            sourcePageCount: 2,
            new RenderProfileId(RenderProfileId.PdfPagePngV1),
            renderer,
            pages,
            new DateTimeOffset(2026, 8, 7, 15, 30, 0, TimeSpan.Zero));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fixture.ControlStore.CommitAsync(new RenderManifestCommitRequest(
                SqlitePersistenceFixture.CorpusId,
                invalidPersistenceCandidate)));
        Assert.Equal(0, await fixture.ScalarAsync("SELECT COUNT(*) FROM document_render_manifests;"));
        Assert.Equal(0, await fixture.ScalarAsync("SELECT COUNT(*) FROM document_page_images;"));
    }

    private static DocumentRenderManifest Manifest(
        DocumentVersion document,
        RendererDescriptor renderer,
        params byte[][] images)
    {
        var pages = images.Select((bytes, index) =>
        {
            var imageId = ContentId(bytes);
            return new DocumentPageImage(
                document.Id,
                document.Version,
                document.ContentObjectId,
                index + 1,
                new RenderProfileId(RenderProfileId.PdfPagePngV1),
                renderer,
                imageId,
                new ImageSha256(imageId.Value),
                bytes.Length,
                DocumentPageImage.PngMediaType,
                72,
                48);
        });
        return DocumentRenderManifest.Create(
            document.Id,
            document.Version,
            document.ContentObjectId,
            images.Length,
            new RenderProfileId(RenderProfileId.PdfPagePngV1),
            renderer,
            pages,
            new DateTimeOffset(2026, 8, 7, 15, 0, 0, TimeSpan.Zero));
    }

    private static DocumentPageImage Page(
        DocumentVersion document,
        RendererDescriptor renderer,
        ContentObjectId imageId,
        int pageNumber,
        long byteLength) =>
        new(
            document.Id,
            document.Version,
            document.ContentObjectId,
            pageNumber,
            new RenderProfileId(RenderProfileId.PdfPagePngV1),
            renderer,
            imageId,
            new ImageSha256(imageId.Value),
            byteLength,
            DocumentPageImage.PngMediaType,
            72,
            48);

    private static async Task<DocumentVersion> CommitPdfCatalogueAsync(
        SqlitePersistenceFixture fixture,
        byte[] pdf)
    {
        await using var source = new MemoryStream(pdf, writable: false);
        var content = await fixture.ContentStore.PutAndVerifyAsync(new BoundedContentInput(
            source,
            pdf.Length,
            ContentMediaType.ApplicationPdf));
        var category = new DatabaseCategory(
            new DatabaseCategoryId("category-render"),
            "Synthetic render category");
        var product = new DatabaseProduct(
            new DatabaseProductId("db-render"),
            new DatabaseProductRevision(1),
            "Synthetic render database",
            CatalogueItemStatus.Active,
            [category.Id]);
        var document = new DocumentVersion(
            new DocumentId("document-render-e2e"),
            new DocumentVersionNumber(1),
            product.Id,
            product.Revision,
            DocumentFormat.Pdf,
            DocumentContentLanguage.EnGb,
            CatalogueItemStatus.Active,
            content.ContentObjectId,
            content.ByteLength,
            ContentMediaType.ApplicationPdf.Value,
            new SourceAdapterId("local-render-synthetic"),
            SourceTrustClass.LocalAuthorised);
        var snapshot = new CatalogueSnapshot(
            SqlitePersistenceFixture.CorpusId,
            new CatalogueRevision(1),
            [category],
            [product],
            [document]);
        var committed = await fixture.ControlStore.CommitCatalogueAsync(
            new CatalogueCommitRequest(
                new OperationId("catalogue-render-e2e"),
                snapshot,
                ExpectedCurrentRevision: 0,
                new DateTimeOffset(2026, 8, 7, 14, 0, 0, TimeSpan.Zero)));
        Assert.Equal(StoreMutationOutcome.Applied, committed.Outcome);
        return document;
    }

    private static PdfRenderPolicy Policy(
        int maximumPages = 2,
        long maximumPixels = 200_000) =>
        new(
            maximumSourceByteLength: 1_000_000,
            maximumPageCount: maximumPages,
            maximumTotalPixels: maximumPixels,
            maximumPageOutputByteLength: 1_000_000,
            maximumTotalOutputByteLength: 2_000_000,
            maximumWorkerMemoryBytes: 768L * 1024 * 1024,
            maximumWorkerCpuTime: TimeSpan.FromSeconds(20),
            workerTimeout: TimeSpan.FromSeconds(30));

    private static string DotnetHostPath()
    {
        var configured = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH");

        if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured))
        {
            return configured;
        }

        var runtimeDirectory = new DirectoryInfo(RuntimeEnvironment.GetRuntimeDirectory());
        var root = runtimeDirectory.Parent?.Parent?.Parent?.FullName ??
            throw new InvalidOperationException("The .NET host root could not be resolved.");
        var candidate = Path.Combine(root, OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet");
        return File.Exists(candidate)
            ? candidate
            : throw new InvalidOperationException("The .NET host could not be resolved.");
    }

    private static IsolatedPdfRendererProcess CreateWorkerRenderer() =>
        new(new RendererWorkerLaunch(
            DotnetHostPath(),
            [typeof(Program).Assembly.Location],
            RuntimeInformation.RuntimeIdentifier));

    private static RenderedPdfPageCandidate ReplaceIhdrColourType(
        RenderedPdfPageCandidate candidate,
        byte colourType)
    {
        var bytes = candidate.PngBytes.ToArray();
        bytes[25] = colourType;
        var crc = Crc32(bytes.AsSpan(12, 4), bytes.AsSpan(16, 13));
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(29, 4), crc);
        return new RenderedPdfPageCandidate(
            candidate.PageNumber,
            candidate.SourceWidthPoints,
            candidate.SourceHeightPoints,
            bytes);
    }

    private static RenderedPdfPageCandidate InsertChunk(
        RenderedPdfPageCandidate candidate,
        string type,
        byte[] data)
    {
        var original = candidate.PngBytes.ToArray();
        var iendOffset = original.Length - 12;
        var inserted = new byte[original.Length + 12 + data.Length];
        original.AsSpan(0, iendOffset).CopyTo(inserted);
        BinaryPrimitives.WriteUInt32BigEndian(inserted.AsSpan(iendOffset), (uint)data.Length);
        Encoding.ASCII.GetBytes(type).CopyTo(inserted, iendOffset + 4);
        data.CopyTo(inserted, iendOffset + 8);
        BinaryPrimitives.WriteUInt32BigEndian(
            inserted.AsSpan(iendOffset + 8 + data.Length),
            Crc32(Encoding.ASCII.GetBytes(type), data));
        original.AsSpan(iendOffset).CopyTo(inserted.AsSpan(iendOffset + 12 + data.Length));
        return new RenderedPdfPageCandidate(
            candidate.PageNumber,
            candidate.SourceWidthPoints,
            candidate.SourceHeightPoints,
            inserted);
    }

    private static (byte BitDepth, byte ColourType) ReadIhdr(ReadOnlySpan<byte> png) =>
        (png[24], png[25]);

    private static uint Crc32(ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
    {
        var crc = uint.MaxValue;

        foreach (var value in type)
        {
            crc = UpdateCrc(crc, value);
        }

        foreach (var value in data)
        {
            crc = UpdateCrc(crc, value);
        }

        return ~crc;
    }

    private static uint UpdateCrc(uint crc, byte value)
    {
        crc ^= value;

        for (var bit = 0; bit < 8; bit++)
        {
            crc = (crc & 1) == 0 ? crc >> 1 : (crc >> 1) ^ 0xedb88320u;
        }

        return crc;
    }

    private static ContentObjectId ContentId(ReadOnlySpan<byte> bytes) =>
        new(Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());

    private static byte[] CreatePdf(params PageSpec[] pages)
    {
        var objects = new SortedDictionary<int, string>();
        var pageIds = Enumerable.Range(0, pages.Length).Select(index => 3 + (index * 2)).ToArray();
        var nextObject = 3 + (pages.Length * 2);
        var widgetIds = new List<int>();
        var pageBodies = new Dictionary<int, string>();

        for (var index = 0; index < pages.Length; index++)
        {
            var spec = pages[index];
            var pageId = pageIds[index];
            var contentId = pageId + 1;
            var rotation = spec.Rotation == 0 ? string.Empty : $" /Rotate {spec.Rotation}";
            var annotation = string.Empty;

            if (spec.WithWidgetAppearance)
            {
                var widgetId = nextObject++;
                var appearanceId = nextObject++;
                widgetIds.Add(widgetId);
                annotation = $" /Annots [{widgetId} 0 R]";
                var drawing = string.Create(
                    CultureInfo.InvariantCulture,
                    $"1 0 0 rg 0 0 {spec.WidthPoints:0.###} {spec.HeightPoints:0.###} re f");
                objects[appearanceId] = string.Create(
                    CultureInfo.InvariantCulture,
                    $"<< /Type /XObject /Subtype /Form /BBox [0 0 {spec.WidthPoints:0.###} {spec.HeightPoints:0.###}] /Resources << >> /Length {Encoding.ASCII.GetByteCount(drawing)} >>\nstream\n{drawing}\nendstream");
                objects[widgetId] = string.Create(
                    CultureInfo.InvariantCulture,
                    $"<< /Type /Annot /Subtype /Widget /FT /Tx /T (synthetic) /Rect [0 0 {spec.WidthPoints:0.###} {spec.HeightPoints:0.###}] /AP << /N {appearanceId} 0 R >> /P {pageId} 0 R >>");
            }

            pageBodies[pageId] = string.Create(
                CultureInfo.InvariantCulture,
                $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {spec.WidthPoints:0.###} {spec.HeightPoints:0.###}]{rotation}{annotation} /Resources << >> /Contents {contentId} 0 R >>");
            const string content = "q Q";
            objects[contentId] = $"<< /Length {content.Length} >>\nstream\n{content}\nendstream";
        }

        objects[1] = widgetIds.Count == 0
            ? "<< /Type /Catalog /Pages 2 0 R >>"
            : $"<< /Type /Catalog /Pages 2 0 R /AcroForm << /Fields [{string.Join(' ', widgetIds.Select(id => $"{id} 0 R"))}] >> >>";
        objects[2] = $"<< /Type /Pages /Kids [{string.Join(' ', pageIds.Select(id => $"{id} 0 R"))}] /Count {pages.Length} >>";

        foreach (var page in pageBodies)
        {
            objects[page.Key] = page.Value;
        }

        using var stream = new MemoryStream();
        WriteAscii(stream, "%PDF-1.7\n");
        var maximumId = objects.Keys.Max();
        var offsets = new long[maximumId + 1];

        for (var id = 1; id <= maximumId; id++)
        {
            offsets[id] = stream.Position;
            WriteAscii(stream, $"{id} 0 obj\n{objects[id]}\nendobj\n");
        }

        var xref = stream.Position;
        WriteAscii(stream, $"xref\n0 {maximumId + 1}\n0000000000 65535 f \n");

        for (var id = 1; id <= maximumId; id++)
        {
            WriteAscii(stream, offsets[id].ToString("D10", CultureInfo.InvariantCulture) + " 00000 n \n");
        }

        WriteAscii(
            stream,
            $"trailer\n<< /Size {maximumId + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
        return stream.ToArray();
    }

    private static void WriteAscii(Stream stream, string value) =>
        stream.Write(Encoding.ASCII.GetBytes(value));

    private sealed record PageSpec(
        double WidthPoints,
        double HeightPoints,
        int Rotation = 0,
        bool WithWidgetAppearance = false);

    private sealed class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        internal static ByteArrayComparer Instance { get; } = new();

        public bool Equals(byte[]? left, byte[]? right) =>
            left is not null && right is not null && left.AsSpan().SequenceEqual(right);

        public int GetHashCode(byte[] value) => value.Length;
    }
}
