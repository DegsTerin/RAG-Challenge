// Purpose: Validates complete pdf-page-png-v1 byte structure, metadata policy, dimensions, opacity and identity before immutable publication; it never repairs renderer output.
using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

using RagChallenge.Application.Documents;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Infrastructure.Documents;

public sealed class PngPageImageValidator : IPngPageImageValidator
{
    private static readonly byte[] Signature = [137, 80, 78, 71, 13, 10, 26, 10];
    private static readonly HashSet<string> PermittedAncillaryChunks =
        new(StringComparer.Ordinal) { "cHRM", "gAMA", "pHYs", "sBIT", "sRGB" };

    public PngPageImageValidation Validate(
        RenderedPdfPageCandidate candidate,
        PdfRenderPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(policy);

        if (candidate.PngBytes.Length > policy.MaximumPageOutputByteLength)
        {
            throw new PdfRenderException(PdfRenderFailureKind.LimitExceeded);
        }

        var bytes = candidate.PngBytes.Span;

        if (bytes.Length < Signature.Length + 12 || !bytes.StartsWith(Signature))
        {
            throw InvalidImage();
        }

        var offset = Signature.Length;
        var sawHeader = false;
        var sawImageData = false;
        var imageDataEnded = false;
        var sawEnd = false;
        var width = 0;
        var height = 0;
        using var compressed = new MemoryStream();

        while (offset < bytes.Length)
        {
            if (bytes.Length - offset < 12)
            {
                throw InvalidImage();
            }

            var length = checked((int)BinaryPrimitives.ReadUInt32BigEndian(bytes[offset..]));
            offset += 4;

            if (length < 0 || bytes.Length - offset < length + 8)
            {
                throw InvalidImage();
            }

            var typeBytes = bytes.Slice(offset, 4);
            var type = Encoding.ASCII.GetString(typeBytes);
            offset += 4;
            var data = bytes.Slice(offset, length);
            offset += length;
            var expectedCrc = BinaryPrimitives.ReadUInt32BigEndian(bytes[offset..]);
            offset += 4;

            if (Crc32(typeBytes, data) != expectedCrc)
            {
                throw InvalidImage();
            }

            switch (type)
            {
                case "IHDR":
                    if (sawHeader || sawImageData || length != 13)
                    {
                        throw InvalidImage();
                    }

                    width = checked((int)BinaryPrimitives.ReadUInt32BigEndian(data));
                    height = checked((int)BinaryPrimitives.ReadUInt32BigEndian(data[4..]));

                    if (data[8] != 8 ||
                        data[9] != 2 ||
                        data[10] != 0 ||
                        data[11] != 0 ||
                        data[12] != 0)
                    {
                        throw InvalidImage();
                    }

                    sawHeader = true;
                    break;

                case "IDAT":
                    if (!sawHeader || imageDataEnded || sawEnd || length == 0)
                    {
                        throw InvalidImage();
                    }

                    sawImageData = true;
                    compressed.Write(data);
                    break;

                case "IEND":
                    if (!sawHeader || !sawImageData || sawEnd || length != 0)
                    {
                        throw InvalidImage();
                    }

                    sawEnd = true;
                    break;

                default:
                    if (!sawHeader || sawImageData || sawEnd ||
                        !PermittedAncillaryChunks.Contains(type) ||
                        !ValidTechnicalChunkLength(type, length))
                    {
                        throw InvalidImage();
                    }

                    break;
            }

            if (sawImageData && type != "IDAT" && type != "IEND")
            {
                imageDataEnded = true;
            }

            if (sawEnd && offset != bytes.Length)
            {
                throw InvalidImage();
            }
        }

        if (!sawHeader || !sawImageData || !sawEnd ||
            width is <= 0 or > PdfRenderPolicy.MaximumDimensionPixels ||
            height is <= 0 or > PdfRenderPolicy.MaximumDimensionPixels)
        {
            throw InvalidImage();
        }

        ValidateExpectedDimensions(candidate, width, height);
        ValidateScanlines(compressed, width, height);
        var sha256 = SHA256.HashData(bytes);
        return new PngPageImageValidation(
            candidate.PageNumber,
            width,
            height,
            new ContentObjectId(Convert.ToHexString(sha256).ToLowerInvariant()),
            bytes.Length);
    }

    private static void ValidateExpectedDimensions(
        RenderedPdfPageCandidate candidate,
        int width,
        int height)
    {
        var expectedWidth = candidate.SourceWidthPoints * PdfRenderPolicy.Dpi / 72d;
        var expectedHeight = candidate.SourceHeightPoints * PdfRenderPolicy.Dpi / 72d;

        if (expectedWidth > PdfRenderPolicy.MaximumDimensionPixels + 0.5d ||
            expectedHeight > PdfRenderPolicy.MaximumDimensionPixels + 0.5d ||
            Math.Abs(width - expectedWidth) > 1d ||
            Math.Abs(height - expectedHeight) > 1d)
        {
            throw InvalidImage();
        }

        var ratioError = Math.Abs(
            (width * candidate.SourceHeightPoints) -
            (height * candidate.SourceWidthPoints));
        var roundingTolerance = Math.Max(
            candidate.SourceWidthPoints,
            candidate.SourceHeightPoints);

        if (ratioError > roundingTolerance)
        {
            throw InvalidImage();
        }
    }

    private static void ValidateScanlines(MemoryStream compressed, int width, int height)
    {
        compressed.Position = 0;
        using var zlib = new ZLibStream(compressed, CompressionMode.Decompress, leaveOpen: true);
        var scanline = new byte[checked((width * 3) + 1)];

        for (var row = 0; row < height; row++)
        {
            zlib.ReadExactly(scanline);

            if (scanline[0] > 4)
            {
                throw InvalidImage();
            }
        }

        if (zlib.ReadByte() != -1)
        {
            throw InvalidImage();
        }
    }

    private static bool ValidTechnicalChunkLength(string type, int length) =>
        type switch
        {
            "cHRM" => length == 32,
            "gAMA" => length == 4,
            "pHYs" => length == 9,
            "sBIT" => length == 3,
            "sRGB" => length == 1,
            _ => false,
        };

    private static uint Crc32(ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
    {
        var crc = uint.MaxValue;

        foreach (var value in type)
        {
            crc = Update(crc, value);
        }

        foreach (var value in data)
        {
            crc = Update(crc, value);
        }

        return ~crc;
    }

    private static uint Update(uint crc, byte value)
    {
        crc ^= value;

        for (var bit = 0; bit < 8; bit++)
        {
            crc = (crc & 1) == 0
                ? crc >> 1
                : (crc >> 1) ^ 0xedb88320u;
        }

        return crc;
    }

    private static PdfRenderException InvalidImage() =>
        new(PdfRenderFailureKind.InvalidPageImage);
}
