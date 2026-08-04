// Purpose: Builds deterministic in-memory PDF and CSV fixtures for parser runtime gates without using product corpus content or filesystem authority.
using System.Globalization;
using System.Text;

namespace RagChallenge.IntegrationTests;

internal static class SyntheticParserFixtureFactory
{
    internal static byte[] CreatePdf(params string[] pageMarkers)
    {
        if (pageMarkers.Length == 0 ||
            pageMarkers.Any(marker => marker.Any(character => character is < 'A' or > 'Z')))
        {
            throw new ArgumentException(
                "PDF fixture markers must contain uppercase ASCII letters.",
                nameof(pageMarkers));
        }

        var objects = new List<string>();
        var pageObjectIds = new List<int>();
        var contentObjectIds = new List<int>();

        for (var index = 0; index < pageMarkers.Length; index++)
        {
            pageObjectIds.Add(3 + (index * 2));
            contentObjectIds.Add(4 + (index * 2));
        }

        var fontObjectId = 3 + (pageMarkers.Length * 2);
        objects.Add("<< /Type /Catalog /Pages 2 0 R >>");
        objects.Add(
            $"<< /Type /Pages /Kids [{string.Join(' ', pageObjectIds.Select(id => $"{id} 0 R"))}] /Count {pageMarkers.Length} >>");

        for (var index = 0; index < pageMarkers.Length; index++)
        {
            objects.Add(
                $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 {fontObjectId} 0 R >> >> /Contents {contentObjectIds[index]} 0 R >>");
            var content = $"BT /F1 12 Tf 72 720 Td ({pageMarkers[index]}) Tj ET";
            objects.Add(
                $"<< /Length {Encoding.ASCII.GetByteCount(content)} >>\nstream\n{content}\nendstream");
        }

        objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");

        using var stream = new MemoryStream();
        WriteAscii(stream, "%PDF-1.4\n");
        var offsets = new List<long> { 0 };

        for (var index = 0; index < objects.Count; index++)
        {
            offsets.Add(stream.Position);
            WriteAscii(
                stream,
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"{index + 1} 0 obj\n{objects[index]}\nendobj\n"));
        }

        var xrefOffset = stream.Position;
        WriteAscii(stream, $"xref\n0 {objects.Count + 1}\n");
        WriteAscii(stream, "0000000000 65535 f \n");

        foreach (var offset in offsets.Skip(1))
        {
            WriteAscii(
                stream,
                offset.ToString("D10", CultureInfo.InvariantCulture) + " 00000 n \n");
        }

        WriteAscii(
            stream,
            $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF\n");
        return stream.ToArray();
    }

    internal static byte[] CsvValidQuotedUtf8 =>
        Encoding.UTF8.GetBytes(
            "name,description\r\nBanco,\"vírgula, preservada e \"\"aspas\"\"\"\r\n");

    internal static byte[] CsvFormulaLiteral =>
        Encoding.UTF8.GetBytes("name,value\r\nFormula,=1+1\r\n");

    internal static byte[] CsvMalformedQuote =>
        Encoding.UTF8.GetBytes("name,value\r\nBroken,\"unterminated\r\n");

    private static void WriteAscii(Stream stream, string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        stream.Write(bytes);
    }
}
