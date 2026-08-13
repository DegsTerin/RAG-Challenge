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
            var prepared = Prepare(source, policy, effectiveRuntimeIdentifier);
            var pages = new List<RenderedPdfPageCandidate>(prepared.PageSizes.Length);
            long totalOutputBytes = 0;

            for (var pageIndex = 0; pageIndex < prepared.PageSizes.Length; pageIndex++)
            {
                var page = RenderPage(prepared, pageIndex, policy, cancellationToken);
                totalOutputBytes = CountOutputBytes(page, totalOutputBytes, policy);
                pages.Add(page);
            }

            return new PdfRenderResult(
                prepared.RendererDescriptor,
                prepared.PageSizes.Length,
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
        catch (OutOfMemoryException)
        {
            throw new PdfRenderException(PdfRenderFailureKind.LimitExceeded);
        }
        catch
        {
            throw new PdfRenderException(PdfRenderFailureKind.MalformedContent);
        }
    }

    internal static async Task RenderToAsync(
        ReadOnlyMemory<byte> source,
        PdfRenderPolicy policy,
        string effectiveRuntimeIdentifier,
        Func<RendererDescriptor, int, CancellationToken, Task> startOutput,
        Func<RenderedPdfPageCandidate, long, CancellationToken, Task> writePage,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(startOutput);
        ArgumentNullException.ThrowIfNull(writePage);

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
            var prepared = Prepare(source, policy, effectiveRuntimeIdentifier);
            await startOutput(
                prepared.RendererDescriptor,
                prepared.PageSizes.Length,
                cancellationToken).ConfigureAwait(false);
            long totalOutputBytes = 0;

            for (var pageIndex = 0; pageIndex < prepared.PageSizes.Length; pageIndex++)
            {
                var page = RenderPage(prepared, pageIndex, policy, cancellationToken);
                totalOutputBytes = CountOutputBytes(page, totalOutputBytes, policy);
                await writePage(page, totalOutputBytes, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (PdfRenderException)
        {
            throw;
        }
        catch (OutOfMemoryException)
        {
            throw new PdfRenderException(PdfRenderFailureKind.LimitExceeded);
        }
        catch
        {
            throw new PdfRenderException(PdfRenderFailureKind.MalformedContent);
        }
    }

    private static PreparedPdf Prepare(
        ReadOnlyMemory<byte> source,
        PdfRenderPolicy policy,
        string effectiveRuntimeIdentifier)
    {
        var sourceBytes = source.ToArray();
#pragma warning disable CA1416 // The explicit Windows/Linux guard in each entry point matches the selected native assets.
        var discoveredPageSizes = Conversion.GetPageSizes(sourceBytes);
#pragma warning restore CA1416

        if (discoveredPageSizes.Count is <= 0 ||
            discoveredPageSizes.Count > policy.MaximumPageCount)
        {
            throw new PdfRenderException(PdfRenderFailureKind.LimitExceeded);
        }

        var pageSizes = discoveredPageSizes
            .Select(size => (size.Width, size.Height))
            .ToArray();
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

        return new PreparedPdf(
            sourceBytes,
            pageSizes,
            PdfPagePngV1RendererIdentity.CreateDescriptor(
                policy,
                effectiveRuntimeIdentifier));
    }

    private static RenderedPdfPageCandidate RenderPage(
        PreparedPdf prepared,
        int pageIndex,
        PdfRenderPolicy policy,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
#pragma warning disable CA1416 // The explicit Windows/Linux guard in each entry point matches the selected native assets.
        using var rendered = Conversion.ToImage(
            prepared.SourceBytes,
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
        return new RenderedPdfPageCandidate(
            pageIndex + 1,
            prepared.PageSizes[pageIndex].Width,
            prepared.PageSizes[pageIndex].Height,
            encoded.ToArray());
    }

    private static long CountOutputBytes(
        RenderedPdfPageCandidate page,
        long currentTotal,
        PdfRenderPolicy policy)
    {
        var total = checked(currentTotal + page.PngBytes.Length);

        if (page.PngBytes.Length > policy.MaximumPageOutputByteLength ||
            total > policy.MaximumTotalOutputByteLength)
        {
            throw new PdfRenderException(PdfRenderFailureKind.LimitExceeded);
        }

        return total;
    }

    private sealed record PreparedPdf(
        byte[] SourceBytes,
        (double Width, double Height)[] PageSizes,
        RendererDescriptor RendererDescriptor);
}
