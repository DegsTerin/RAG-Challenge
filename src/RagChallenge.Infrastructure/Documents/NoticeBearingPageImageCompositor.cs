// Purpose: Composes and verifies deterministic notice-bearing PNGs with an intact source-page region and a project-owned bitmap glyph asset; PDF rasterisation and persistence remain separate boundaries.
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

using RagChallenge.Application.Documents;
using RagChallenge.Domain.CorpusCatalog;

using SkiaSharp;

namespace RagChallenge.Infrastructure.Documents;

public sealed class NoticeBearingPageImageCompositor :
    INoticeBearingPageImageCompositor,
    INoticeBearingPageImageValidator
{
    private static readonly Dictionary<char, byte[]> Glyphs = CreateGlyphs();
    private static readonly string FontDigest = CalculateFontDigest();
    private readonly PngPageImageValidator structureValidator = new();

    public string FontAssetSha256 => FontDigest;

    public RendererDescriptor Describe(
        PdfRenderPolicy policy,
        RendererDescriptor sourceRendererDescriptor,
        DerivativeObligationSetV1 obligationSet) =>
        PdfPagePngNoticeV1RendererIdentity.CreateDescriptor(
            policy,
            sourceRendererDescriptor,
            obligationSet,
            FontDigest);

    public NoticeBearingPageCandidate Compose(
        RenderedPdfPageCandidate sourcePage,
        PngPageImageValidation sourceValidation,
        RendererDescriptor sourceRendererDescriptor,
        DerivativeObligationSetV1 obligationSet,
        PdfRenderPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(sourcePage);
        ArgumentNullException.ThrowIfNull(sourceValidation);
        ArgumentNullException.ThrowIfNull(sourceRendererDescriptor);
        ArgumentNullException.ThrowIfNull(obligationSet);
        ArgumentNullException.ThrowIfNull(policy);

        if (sourcePage.PageNumber != sourceValidation.PageNumber)
        {
            throw new PdfRenderException(PdfRenderFailureKind.ProtocolViolation);
        }

        var descriptor = Describe(policy, sourceRendererDescriptor, obligationSet);
        var lines = BuildLines(obligationSet, sourcePage.PageNumber, sourceValidation.WidthPixels);
        var noticeHeight = checked(
            PdfPagePngNoticeV1RendererIdentity.SeparatorHeightPixels +
            (2 * PdfPagePngNoticeV1RendererIdentity.PanelPaddingPixels) +
            (lines.Count * PdfPagePngNoticeV1RendererIdentity.LineAdvancePixels));
        var compositeHeight = checked(sourceValidation.HeightPixels + noticeHeight);

        if (compositeHeight > PdfRenderPolicy.MaximumDimensionPixels ||
            checked((long)sourceValidation.WidthPixels * compositeHeight) > policy.MaximumTotalPixels)
        {
            throw new PdfRenderException(PdfRenderFailureKind.LimitExceeded);
        }

        using var sourceBitmap = DecodeOpaqueRgb(sourcePage.PngBytes.Span);
        using var composite = new SKBitmap(new SKImageInfo(
            sourceBitmap.Width,
            compositeHeight,
            SKColorType.Rgb888x,
            SKAlphaType.Opaque));
        using (var canvas = new SKCanvas(composite))
        {
            canvas.Clear(SKColors.White);
        }

        CopySourceRows(sourceBitmap, composite);
        DrawNoticePanel(composite, sourceBitmap.Height, lines);
        using var image = SKImage.FromBitmap(composite);
        using var encoded = image.Encode(SKEncodedImageFormat.Png, quality: 100) ??
            throw new PdfRenderException(PdfRenderFailureKind.RendererFailed);
        var bytes = encoded.ToArray();

        if (bytes.LongLength > policy.MaximumPageOutputByteLength)
        {
            throw new PdfRenderException(PdfRenderFailureKind.LimitExceeded);
        }

        return new NoticeBearingPageCandidate(
            sourcePage.PageNumber,
            bytes,
            sourceBitmap.Width,
            sourceBitmap.Height,
            noticeHeight,
            descriptor);
    }

    public NoticeBearingPageValidation Validate(
        RenderedPdfPageCandidate sourcePage,
        PngPageImageValidation sourceValidation,
        NoticeBearingPageCandidate composite,
        PdfRenderPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(sourcePage);
        ArgumentNullException.ThrowIfNull(sourceValidation);
        ArgumentNullException.ThrowIfNull(composite);
        ArgumentNullException.ThrowIfNull(policy);

        if (sourcePage.PageNumber != sourceValidation.PageNumber ||
            composite.PageNumber != sourceValidation.PageNumber ||
            composite.SourceRegionWidthPixels != sourceValidation.WidthPixels ||
            composite.SourceRegionHeightPixels != sourceValidation.HeightPixels ||
            composite.NoticeRegionHeightPixels <= 0)
        {
            throw new PdfRenderException(PdfRenderFailureKind.InvalidPageImage);
        }

        var expectedHeight = checked(
            composite.SourceRegionHeightPixels + composite.NoticeRegionHeightPixels);
        var structural = structureValidator.Validate(
            new RenderedPdfPageCandidate(
                composite.PageNumber,
                composite.SourceRegionWidthPixels * 72d / PdfRenderPolicy.Dpi,
                expectedHeight * 72d / PdfRenderPolicy.Dpi,
                composite.PngBytes.Span),
            policy);

        if (structural.WidthPixels != composite.SourceRegionWidthPixels ||
            structural.HeightPixels != expectedHeight)
        {
            throw new PdfRenderException(PdfRenderFailureKind.InvalidPageImage);
        }

        using var sourceBitmap = DecodeOpaqueRgb(sourcePage.PngBytes.Span);
        using var compositeBitmap = DecodeOpaqueRgb(composite.PngBytes.Span);
        var sourcePixels = CopyCanonicalRgb(sourceBitmap, sourceBitmap.Height);
        var compositeSourcePixels = CopyCanonicalRgb(compositeBitmap, sourceBitmap.Height);

        if (!sourcePixels.AsSpan().SequenceEqual(compositeSourcePixels))
        {
            throw new PdfRenderException(PdfRenderFailureKind.InvalidPageImage);
        }

        var pixelDigest = SHA256.HashData(sourcePixels);
        return new NoticeBearingPageValidation(
            composite.PageNumber,
            structural.WidthPixels,
            structural.HeightPixels,
            composite.SourceRegionWidthPixels,
            composite.SourceRegionHeightPixels,
            composite.NoticeRegionHeightPixels,
            new ImageSha256(Convert.ToHexString(pixelDigest).ToLowerInvariant()),
            structural.Sha256,
            structural.ByteLength);
    }

    private static List<string> BuildLines(
        DerivativeObligationSetV1 set,
        int pageNumber,
        int widthPixels)
    {
        var availableWidth = widthPixels -
            (2 * PdfPagePngNoticeV1RendererIdentity.PanelPaddingPixels);
        var maximumCharacters = availableWidth /
            PdfPagePngNoticeV1RendererIdentity.GlyphAdvancePixels;

        if (maximumCharacters < 16)
        {
            throw new PdfRenderException(PdfRenderFailureKind.LimitExceeded);
        }

        var blocks = new List<string>
        {
            "RAG-CHALLENGE RENDERED DERIVATIVE",
            $"SOURCE DOCUMENT: {set.DocumentTitle} | VERSION: {set.DocumentVersionLabel} | PHYSICAL PAGE: {pageNumber.ToString(CultureInfo.InvariantCulture)}",
            $"PUBLISHER OR AUTHOR: {set.AuthoritativePublisherOrAuthor}\nSOURCE REFERENCE: {set.SourceReference}\nATTRIBUTION: {set.AttributionText}",
            $"COPYRIGHT NOTICE: {set.CopyrightNotice}",
            $"PERMISSION NOTICE: {set.PermissionNotice}",
        };

        blocks.AddRange(set.OrderedDisclaimers.Select((value, index) =>
            $"DISCLAIMER {(index + 1).ToString(CultureInfo.InvariantCulture)}: {value}"));
        blocks.Add(
            $"TRADEMARK TREATMENT: {set.TrademarkTreatment}\nTRADEMARK OR NON-ENDORSEMENT: {set.TrademarkOrNonEndorsementText}");
        blocks.Add($"CHANGE MARKING: {set.ChangeMarkingText}");
        var lines = new List<string>();

        for (var blockIndex = 0; blockIndex < blocks.Count; blockIndex++)
        {
            if (blockIndex > 0)
            {
                lines.Add(string.Empty);
            }

            foreach (var logicalLine in blocks[blockIndex].Split('\n'))
            {
                EnsureSupportedGlyphs(logicalLine);

                for (var offset = 0; offset < logicalLine.Length; offset += maximumCharacters)
                {
                    lines.Add(logicalLine.Substring(
                        offset,
                        Math.Min(maximumCharacters, logicalLine.Length - offset)));
                }

                if (logicalLine.Length == 0)
                {
                    lines.Add(string.Empty);
                }
            }
        }

        return lines;
    }

    private static void EnsureSupportedGlyphs(string value)
    {
        if (value.Any(character => !Glyphs.ContainsKey(character)))
        {
            throw new PdfRenderException(PdfRenderFailureKind.InvalidPageImage);
        }
    }

    private static void DrawNoticePanel(
        SKBitmap target,
        int sourceHeight,
        List<string> lines)
    {
        using var canvas = new SKCanvas(target);
        using var separator = new SKPaint { Color = new SKColor(48, 48, 48), IsAntialias = false };
        using var ink = new SKPaint { Color = SKColors.Black, IsAntialias = false };
        canvas.DrawRect(
            0,
            sourceHeight,
            target.Width,
            PdfPagePngNoticeV1RendererIdentity.SeparatorHeightPixels,
            separator);
        var y = sourceHeight + PdfPagePngNoticeV1RendererIdentity.SeparatorHeightPixels +
            PdfPagePngNoticeV1RendererIdentity.PanelPaddingPixels;

        foreach (var line in lines)
        {
            var x = PdfPagePngNoticeV1RendererIdentity.PanelPaddingPixels;

            foreach (var character in line)
            {
                DrawGlyph(canvas, ink, character, x, y);
                x += PdfPagePngNoticeV1RendererIdentity.GlyphAdvancePixels;
            }

            y += PdfPagePngNoticeV1RendererIdentity.LineAdvancePixels;
        }

        canvas.Flush();
    }

    private static void DrawGlyph(SKCanvas canvas, SKPaint ink, char character, int x, int y)
    {
        var glyph = Glyphs[character];
        var scale = PdfPagePngNoticeV1RendererIdentity.GlyphScale;

        for (var row = 0; row < glyph.Length; row++)
        {
            for (var column = 0; column < PdfPagePngNoticeV1RendererIdentity.GlyphWidthPixels; column++)
            {
                if ((glyph[row] & (1 << (4 - column))) != 0)
                {
                    canvas.DrawRect(x + (column * scale), y + (row * scale), scale, scale, ink);
                }
            }
        }
    }

    private static SKBitmap DecodeOpaqueRgb(ReadOnlySpan<byte> pngBytes)
    {
        using var decoded = SKBitmap.Decode(pngBytes.ToArray()) ??
            throw new PdfRenderException(PdfRenderFailureKind.InvalidPageImage);
        var info = new SKImageInfo(
            decoded.Width,
            decoded.Height,
            SKColorType.Rgb888x,
            SKAlphaType.Opaque);
        var bitmap = new SKBitmap(info);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.White);
            canvas.DrawBitmap(
                decoded,
                0,
                0,
                new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None));
            canvas.Flush();
        }

        return bitmap;
    }

    private static void CopySourceRows(SKBitmap source, SKBitmap destination)
    {
        if (source.Width != destination.Width || source.Height > destination.Height ||
            source.ColorType != SKColorType.Rgb888x || destination.ColorType != SKColorType.Rgb888x)
        {
            throw new PdfRenderException(PdfRenderFailureKind.InvalidPageImage);
        }

        var row = new byte[checked(source.Width * 4)];

        for (var y = 0; y < source.Height; y++)
        {
            Marshal.Copy(source.GetPixels() + (y * source.RowBytes), row, 0, row.Length);
            Marshal.Copy(row, 0, destination.GetPixels() + (y * destination.RowBytes), row.Length);
        }
    }

    private static byte[] CopyCanonicalRgb(SKBitmap bitmap, int rowCount)
    {
        if (rowCount > bitmap.Height || bitmap.ColorType != SKColorType.Rgb888x)
        {
            throw new PdfRenderException(PdfRenderFailureKind.InvalidPageImage);
        }

        var sourceRow = new byte[checked(bitmap.Width * 4)];
        var canonical = new byte[checked(bitmap.Width * rowCount * 3)];
        var targetOffset = 0;

        for (var y = 0; y < rowCount; y++)
        {
            Marshal.Copy(bitmap.GetPixels() + (y * bitmap.RowBytes), sourceRow, 0, sourceRow.Length);

            for (var x = 0; x < bitmap.Width; x++)
            {
                canonical[targetOffset++] = sourceRow[(x * 4)];
                canonical[targetOffset++] = sourceRow[(x * 4) + 1];
                canonical[targetOffset++] = sourceRow[(x * 4) + 2];
            }
        }

        return canonical;
    }

    private static string CalculateFontDigest()
    {
        var canonical = new StringBuilder(PdfPagePngNoticeV1RendererIdentity.FontAssetId);
        canonical.Append('\n');

        foreach (var pair in Glyphs.OrderBy(pair => pair.Key))
        {
            canonical.Append(((int)pair.Key).ToString("X2", CultureInfo.InvariantCulture))
                .Append(':')
                .Append(Convert.ToHexString(pair.Value).ToLowerInvariant())
                .Append('\n');
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.ASCII.GetBytes(canonical.ToString())))
            .ToLowerInvariant();
    }

    private static Dictionary<char, byte[]> CreateGlyphs()
    {
        var glyphs = new Dictionary<char, byte[]>();
        Add(glyphs, ' ', "00000", "00000", "00000", "00000", "00000", "00000", "00000");
        Add(glyphs, '!', "00100", "00100", "00100", "00100", "00100", "00000", "00100");
        Add(glyphs, '"', "01010", "01010", "01010", "00000", "00000", "00000", "00000");
        Add(glyphs, '#', "01010", "11111", "01010", "01010", "11111", "01010", "01010");
        Add(glyphs, '$', "00100", "01111", "10100", "01110", "00101", "11110", "00100");
        Add(glyphs, '%', "11001", "11010", "00100", "01000", "10110", "00110", "00000");
        Add(glyphs, '&', "01100", "10010", "10100", "01000", "10101", "10010", "01101");
        Add(glyphs, '\'', "00100", "00100", "01000", "00000", "00000", "00000", "00000");
        Add(glyphs, '(', "00010", "00100", "01000", "01000", "01000", "00100", "00010");
        Add(glyphs, ')', "01000", "00100", "00010", "00010", "00010", "00100", "01000");
        Add(glyphs, '*', "00000", "10101", "01110", "11111", "01110", "10101", "00000");
        Add(glyphs, '+', "00000", "00100", "00100", "11111", "00100", "00100", "00000");
        Add(glyphs, ',', "00000", "00000", "00000", "00000", "00100", "00100", "01000");
        Add(glyphs, '-', "00000", "00000", "00000", "11111", "00000", "00000", "00000");
        Add(glyphs, '.', "00000", "00000", "00000", "00000", "00000", "00110", "00110");
        Add(glyphs, '/', "00001", "00010", "00100", "01000", "10000", "00000", "00000");
        Add(glyphs, '0', "01110", "10001", "10011", "10101", "11001", "10001", "01110");
        Add(glyphs, '1', "00100", "01100", "00100", "00100", "00100", "00100", "01110");
        Add(glyphs, '2', "01110", "10001", "00001", "00010", "00100", "01000", "11111");
        Add(glyphs, '3', "11110", "00001", "00001", "01110", "00001", "00001", "11110");
        Add(glyphs, '4', "00010", "00110", "01010", "10010", "11111", "00010", "00010");
        Add(glyphs, '5', "11111", "10000", "10000", "11110", "00001", "00001", "11110");
        Add(glyphs, '6', "01110", "10000", "10000", "11110", "10001", "10001", "01110");
        Add(glyphs, '7', "11111", "00001", "00010", "00100", "01000", "01000", "01000");
        Add(glyphs, '8', "01110", "10001", "10001", "01110", "10001", "10001", "01110");
        Add(glyphs, '9', "01110", "10001", "10001", "01111", "00001", "00001", "01110");
        Add(glyphs, ':', "00000", "00110", "00110", "00000", "00110", "00110", "00000");
        Add(glyphs, ';', "00000", "00110", "00110", "00000", "00110", "00100", "01000");
        Add(glyphs, '<', "00010", "00100", "01000", "10000", "01000", "00100", "00010");
        Add(glyphs, '=', "00000", "00000", "11111", "00000", "11111", "00000", "00000");
        Add(glyphs, '>', "01000", "00100", "00010", "00001", "00010", "00100", "01000");
        Add(glyphs, '?', "01110", "10001", "00001", "00010", "00100", "00000", "00100");
        Add(glyphs, '@', "01110", "10001", "10111", "10101", "10111", "10000", "01110");

        var letters = new Dictionary<char, string[]>
        {
            ['A'] = ["01110", "10001", "10001", "11111", "10001", "10001", "10001"],
            ['B'] = ["11110", "10001", "10001", "11110", "10001", "10001", "11110"],
            ['C'] = ["01111", "10000", "10000", "10000", "10000", "10000", "01111"],
            ['D'] = ["11110", "10001", "10001", "10001", "10001", "10001", "11110"],
            ['E'] = ["11111", "10000", "10000", "11110", "10000", "10000", "11111"],
            ['F'] = ["11111", "10000", "10000", "11110", "10000", "10000", "10000"],
            ['G'] = ["01111", "10000", "10000", "10111", "10001", "10001", "01111"],
            ['H'] = ["10001", "10001", "10001", "11111", "10001", "10001", "10001"],
            ['I'] = ["01110", "00100", "00100", "00100", "00100", "00100", "01110"],
            ['J'] = ["00111", "00010", "00010", "00010", "10010", "10010", "01100"],
            ['K'] = ["10001", "10010", "10100", "11000", "10100", "10010", "10001"],
            ['L'] = ["10000", "10000", "10000", "10000", "10000", "10000", "11111"],
            ['M'] = ["10001", "11011", "10101", "10101", "10001", "10001", "10001"],
            ['N'] = ["10001", "11001", "10101", "10011", "10001", "10001", "10001"],
            ['O'] = ["01110", "10001", "10001", "10001", "10001", "10001", "01110"],
            ['P'] = ["11110", "10001", "10001", "11110", "10000", "10000", "10000"],
            ['Q'] = ["01110", "10001", "10001", "10001", "10101", "10010", "01101"],
            ['R'] = ["11110", "10001", "10001", "11110", "10100", "10010", "10001"],
            ['S'] = ["01111", "10000", "10000", "01110", "00001", "00001", "11110"],
            ['T'] = ["11111", "00100", "00100", "00100", "00100", "00100", "00100"],
            ['U'] = ["10001", "10001", "10001", "10001", "10001", "10001", "01110"],
            ['V'] = ["10001", "10001", "10001", "10001", "10001", "01010", "00100"],
            ['W'] = ["10001", "10001", "10001", "10101", "10101", "10101", "01010"],
            ['X'] = ["10001", "10001", "01010", "00100", "01010", "10001", "10001"],
            ['Y'] = ["10001", "10001", "01010", "00100", "00100", "00100", "00100"],
            ['Z'] = ["11111", "00001", "00010", "00100", "01000", "10000", "11111"],
        };

        foreach (var pair in letters)
        {
            Add(glyphs, pair.Key, pair.Value);
            Add(glyphs, char.ToLowerInvariant(pair.Key), pair.Value);
        }

        Add(glyphs, '[', "01110", "01000", "01000", "01000", "01000", "01000", "01110");
        Add(glyphs, '\\', "10000", "01000", "00100", "00010", "00001", "00000", "00000");
        Add(glyphs, ']', "01110", "00010", "00010", "00010", "00010", "00010", "01110");
        Add(glyphs, '^', "00100", "01010", "10001", "00000", "00000", "00000", "00000");
        Add(glyphs, '_', "00000", "00000", "00000", "00000", "00000", "00000", "11111");
        Add(glyphs, '`', "01000", "00100", "00010", "00000", "00000", "00000", "00000");
        Add(glyphs, '{', "00010", "00100", "00100", "01000", "00100", "00100", "00010");
        Add(glyphs, '|', "00100", "00100", "00100", "00100", "00100", "00100", "00100");
        Add(glyphs, '}', "01000", "00100", "00100", "00010", "00100", "00100", "01000");
        Add(glyphs, '~', "00000", "00000", "01001", "10110", "00000", "00000", "00000");
        Add(glyphs, '\u00a9', "01110", "10001", "10111", "10100", "10100", "10001", "01110");
        Add(glyphs, '\u2013', "00000", "00000", "00000", "11111", "11111", "00000", "00000");
        Add(glyphs, '\u201c', "01010", "01010", "10100", "00000", "00000", "00000", "00000");
        Add(glyphs, '\u201d', "10100", "01010", "01010", "00000", "00000", "00000", "00000");
        return glyphs;
    }

    private static void Add(Dictionary<char, byte[]> target, char character, params string[] rows)
    {
        if (rows.Length != PdfPagePngNoticeV1RendererIdentity.GlyphHeightPixels ||
            rows.Any(row => row.Length != PdfPagePngNoticeV1RendererIdentity.GlyphWidthPixels ||
                row.Any(value => value is not '0' and not '1')))
        {
            throw new InvalidOperationException("The project-owned bitmap glyph asset is invalid.");
        }

        target.Add(
            character,
            rows.Select(row => Convert.ToByte(row, 2)).ToArray());
    }
}
