// Purpose: Defines the bounded PDF-rendering port, deterministic profile identity, page candidates and sanitised failures; native rendering and process containment remain Infrastructure responsibilities.
using System.Collections.ObjectModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Application.Documents;

public enum PdfRenderFailureKind
{
    LimitExceeded,
    MalformedContent,
    RendererUnavailable,
    RendererFailed,
    ProtocolViolation,
    InvalidPageImage,
    Cancelled,
    TimedOut,
}

public sealed class PdfRenderException : Exception
{
    public PdfRenderException(PdfRenderFailureKind failureKind)
        : base($"PDF rendering failed with the sanitised outcome '{failureKind}'.")
    {
        if (!Enum.IsDefined(failureKind))
        {
            throw new ArgumentOutOfRangeException(nameof(failureKind));
        }

        FailureKind = failureKind;
    }

    public PdfRenderFailureKind FailureKind { get; }
}

public sealed class PdfRenderPolicy
{
    public const int Dpi = 144;
    public const int MaximumDimensionPixels = 4096;

    public PdfRenderPolicy(
        long maximumSourceByteLength,
        int maximumPageCount,
        long maximumTotalPixels,
        long maximumPageOutputByteLength,
        long maximumTotalOutputByteLength,
        long maximumWorkerMemoryBytes,
        TimeSpan maximumWorkerCpuTime,
        TimeSpan workerTimeout)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumSourceByteLength);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumPageCount);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumTotalPixels);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumPageOutputByteLength);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumTotalOutputByteLength);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumWorkerMemoryBytes);

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
            maximumWorkerCpuTime,
            TimeSpan.Zero);

        if (workerTimeout <= TimeSpan.Zero || workerTimeout < maximumWorkerCpuTime)
        {
            throw new ArgumentOutOfRangeException(
                nameof(workerTimeout),
                "The elapsed worker timeout must be positive and no shorter than its CPU limit.");
        }

        if (maximumTotalOutputByteLength < maximumPageOutputByteLength)
        {
            throw new ArgumentException(
                "The total output-byte limit cannot be smaller than the per-page limit.",
                nameof(maximumTotalOutputByteLength));
        }

        MaximumSourceByteLength = maximumSourceByteLength;
        MaximumPageCount = maximumPageCount;
        MaximumTotalPixels = maximumTotalPixels;
        MaximumPageOutputByteLength = maximumPageOutputByteLength;
        MaximumTotalOutputByteLength = maximumTotalOutputByteLength;
        MaximumWorkerMemoryBytes = maximumWorkerMemoryBytes;
        MaximumWorkerCpuTime = maximumWorkerCpuTime;
        WorkerTimeout = workerTimeout;
    }

    public long MaximumSourceByteLength { get; }

    public int MaximumPageCount { get; }

    public long MaximumTotalPixels { get; }

    public long MaximumPageOutputByteLength { get; }

    public long MaximumTotalOutputByteLength { get; }

    public long MaximumWorkerMemoryBytes { get; }

    public TimeSpan MaximumWorkerCpuTime { get; }

    public TimeSpan WorkerTimeout { get; }
}

public static class PdfPagePngV1RendererIdentity
{
    public const string RendererId = "pdfium-pdftoimage-v1";
    public const string PdfToImageVersion = "5.3.0";
    public const string PdfiumVersion = "153.0.7988";
    public const string SkiaSharpVersion = "4.151.1";

    public static RendererDescriptor CreateDescriptor(
        PdfRenderPolicy policy,
        string effectiveRuntimeIdentifier)
    {
        ArgumentNullException.ThrowIfNull(policy);

        if (string.IsNullOrWhiteSpace(effectiveRuntimeIdentifier) ||
            effectiveRuntimeIdentifier.Length > 32 ||
            effectiveRuntimeIdentifier.Any(character =>
                !char.IsAsciiLetterOrDigit(character) && character is not '-'))
        {
            throw new ArgumentException(
                "An effective renderer RID must be a bounded non-secret RID token.",
                nameof(effectiveRuntimeIdentifier));
        }

        string[] canonicalSettingLines =
        [
            "descriptorSchema=pdfium-pdftoimage-v1-descriptor-v1",
            $"rendererId={RendererId}",
            $"PDFtoImage={PdfToImageVersion}",
            $"PDFium={PdfiumVersion}",
            $"SkiaSharp={SkiaSharpVersion}",
            $"rid={effectiveRuntimeIdentifier}",
            $"profile={RenderProfileId.PdfPagePngV1}",
            "dpi=144",
            "width=null",
            "height=null",
            "bounds=null",
            "rotation=Rotate0",
            "withAnnotations=false",
            "withFormFill=false",
            "antiAliasing=All",
            "backgroundColor=White",
            "useTiling=false",
            "dpiRelativeToBounds=false",
            "grayscale=false",
            "workerConcurrency=1",
            "coreDump=disabled",
            "environment=sanitised-v1",
            $"maximumSourceByteLength={Invariant(policy.MaximumSourceByteLength)}",
            $"maximumPageCount={Invariant(policy.MaximumPageCount)}",
            $"maximumDimensionPixels={Invariant(PdfRenderPolicy.MaximumDimensionPixels)}",
            $"maximumTotalPixels={Invariant(policy.MaximumTotalPixels)}",
            $"maximumPageOutputByteLength={Invariant(policy.MaximumPageOutputByteLength)}",
            $"maximumTotalOutputByteLength={Invariant(policy.MaximumTotalOutputByteLength)}",
            $"maximumWorkerMemoryBytes={Invariant(policy.MaximumWorkerMemoryBytes)}",
            $"maximumWorkerCpuTicks={Invariant(policy.MaximumWorkerCpuTime.Ticks)}",
            $"workerTimeoutTicks={Invariant(policy.WorkerTimeout.Ticks)}",
        ];
        var canonicalSettings = string.Join('\n', canonicalSettingLines);
        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(canonicalSettings));
        var base64Url = Convert.ToBase64String(digest)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        string[] descriptorSegments =
        [
            RendererId,
            $"p{PdfToImageVersion}",
            $"f{PdfiumVersion}",
            $"s{SkiaSharpVersion}",
            effectiveRuntimeIdentifier,
            RenderProfileId.PdfPagePngV1,
            base64Url,
        ];
        return new RendererDescriptor(string.Join(':', descriptorSegments));
    }

    private static string Invariant(long value) =>
        value.ToString(CultureInfo.InvariantCulture);
}

public sealed class RenderedPdfPageCandidate
{
    private readonly byte[] pngBytes;

    public RenderedPdfPageCandidate(
        int pageNumber,
        double sourceWidthPoints,
        double sourceHeightPoints,
        ReadOnlySpan<byte> pngBytes)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageNumber);

        if (!double.IsFinite(sourceWidthPoints) || sourceWidthPoints <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceWidthPoints));
        }

        if (!double.IsFinite(sourceHeightPoints) || sourceHeightPoints <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceHeightPoints));
        }

        if (pngBytes.IsEmpty)
        {
            throw new ArgumentException("A rendered page candidate must contain bytes.", nameof(pngBytes));
        }

        PageNumber = pageNumber;
        SourceWidthPoints = sourceWidthPoints;
        SourceHeightPoints = sourceHeightPoints;
        this.pngBytes = pngBytes.ToArray();
    }

    public int PageNumber { get; }

    public double SourceWidthPoints { get; }

    public double SourceHeightPoints { get; }

    public ReadOnlyMemory<byte> PngBytes => pngBytes;
}

public sealed class PdfRenderResult
{
    public PdfRenderResult(
        RendererDescriptor rendererDescriptor,
        int sourcePageCount,
        IEnumerable<RenderedPdfPageCandidate> pages)
    {
        ArgumentNullException.ThrowIfNull(rendererDescriptor);
        ArgumentNullException.ThrowIfNull(pages);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sourcePageCount);

        RendererDescriptor = rendererDescriptor;
        SourcePageCount = sourcePageCount;
        Pages = Array.AsReadOnly(pages.ToArray());
    }

    public RendererDescriptor RendererDescriptor { get; }

    public int SourcePageCount { get; }

    public ReadOnlyCollection<RenderedPdfPageCandidate> Pages { get; }
}

public interface IPdfPageRenderer
{
    RendererDescriptor Describe(PdfRenderPolicy policy);

    Task<PdfRenderResult> RenderAsync(
        VerifiedContentObject source,
        PdfRenderPolicy policy,
        CancellationToken cancellationToken = default);
}

public sealed record PngPageImageValidation(
    int PageNumber,
    int WidthPixels,
    int HeightPixels,
    ContentObjectId Sha256,
    long ByteLength);

public interface IPngPageImageValidator
{
    PngPageImageValidation Validate(
        RenderedPdfPageCandidate candidate,
        PdfRenderPolicy policy);
}
