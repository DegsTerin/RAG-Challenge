// Purpose: Executes the selected PDFtoImage/PDFium/Skia rasterisation profile inside the one-document worker; process isolation and durable publication are owned by outer components.
using PDFtoImage;

using RagChallenge.Application.Documents;

using SkiaSharp;

namespace RagChallenge.Infrastructure.Documents;

public sealed class PdfToImagePdfPageRenderer
{
    private static readonly RenderOptions Options = new()
    {
        Dpi = PdfRenderPolicy.Dpi,
        Width = null,
        Height = null,
        WithAnnotations = false,
        WithFormFill = false,
        WithAspectRatio = false,
        Rotation = PdfRotation.Rotate0,
        AntiAliasing = PdfAntiAliasing.All,
        BackgroundColor = SKColors.White,
        Bounds = null,
        UseTiling = false,
        DpiRelativeToBounds = false,
        Grayscale = false,
    };

    public static PdfRenderResult Render(
        ReadOnlyMemory<byte> source,
        PdfRenderPolicy policy,
        string effectiveRuntimeIdentifier,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(policy);

        if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
        {
            throw new PdfRenderException(PdfRenderFailureKind.RendererUnavailable);
        }

        if (source.IsEmpty || source.Length > policy.MaximumSourceByteLength)
        {
            throw new PdfRenderException(PdfRenderFailureKind.LimitExceeded);
        }

        try
        {
            var sourceBytes = source.ToArray();
#pragma warning disable CA1416 // The explicit Windows/Linux guard above matches the selected native assets.
            var pageSizes = Conversion.GetPageSizes(sourceBytes);
#pragma warning restore CA1416

            if (pageSizes.Count is <= 0 || pageSizes.Count > policy.MaximumPageCount)
            {
                throw new PdfRenderException(PdfRenderFailureKind.LimitExceeded);
            }

            long totalPixels = 0;

            foreach (var size in pageSizes)
            {
                var width = Math.Ceiling(size.Width * PdfRenderPolicy.Dpi / 72d);
                var height = Math.Ceiling(size.Height * PdfRenderPolicy.Dpi / 72d);

                if (!double.IsFinite(width) || !double.IsFinite(height) ||
                    width is <= 0 or > PdfRenderPolicy.MaximumDimensionPixels ||
                    height is <= 0 or > PdfRenderPolicy.MaximumDimensionPixels)
                {
                    throw new PdfRenderException(PdfRenderFailureKind.LimitExceeded);
                }

                totalPixels = checked(totalPixels + ((long)width * (long)height));

                if (totalPixels > policy.MaximumTotalPixels)
                {
                    throw new PdfRenderException(PdfRenderFailureKind.LimitExceeded);
                }
            }

            var pages = new List<RenderedPdfPageCandidate>(pageSizes.Count);
            long totalOutputBytes = 0;

            for (var pageIndex = 0; pageIndex < pageSizes.Count; pageIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();
#pragma warning disable CA1416 // The explicit Windows/Linux guard above matches the selected native assets.
                using var rendered = Conversion.ToImage(
                    sourceBytes,
                    new Index(pageIndex),
                    password: null,
                    Options);
#pragma warning restore CA1416

                if (rendered.Width is <= 0 or > PdfRenderPolicy.MaximumDimensionPixels ||
                    rendered.Height is <= 0 or > PdfRenderPolicy.MaximumDimensionPixels)
                {
                    throw new PdfRenderException(PdfRenderFailureKind.LimitExceeded);
                }

                using var opaque = new SKBitmap(new SKImageInfo(
                    rendered.Width,
                    rendered.Height,
                    SKColorType.Rgb888x,
                    SKAlphaType.Opaque));
                using (var canvas = new SKCanvas(opaque))
                {
                    canvas.Clear(SKColors.White);
                    canvas.DrawBitmap(
                        rendered,
                        0,
                        0,
                        new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None));
                    canvas.Flush();
                }

                using var image = SKImage.FromBitmap(opaque);
                using var encoded = image.Encode(SKEncodedImageFormat.Png, quality: 100) ??
                    throw new PdfRenderException(PdfRenderFailureKind.RendererFailed);
                var pngBytes = encoded.ToArray();
                totalOutputBytes = checked(totalOutputBytes + pngBytes.LongLength);

                if (pngBytes.LongLength > policy.MaximumPageOutputByteLength ||
                    totalOutputBytes > policy.MaximumTotalOutputByteLength)
                {
                    throw new PdfRenderException(PdfRenderFailureKind.LimitExceeded);
                }

                pages.Add(new RenderedPdfPageCandidate(
                    pageIndex + 1,
                    pageSizes[pageIndex].Width,
                    pageSizes[pageIndex].Height,
                    pngBytes));
            }

            return new PdfRenderResult(
                PdfPagePngV1RendererIdentity.CreateDescriptor(
                    policy,
                    effectiveRuntimeIdentifier),
                pageSizes.Count,
                pages);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (PdfRenderException)
        {
            throw;
        }
        catch
        {
            throw new PdfRenderException(PdfRenderFailureKind.MalformedContent);
        }
    }
}
