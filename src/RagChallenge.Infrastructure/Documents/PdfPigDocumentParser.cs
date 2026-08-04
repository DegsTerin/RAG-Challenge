// Purpose: Adapts the pinned PdfPig package to the Application parsing port while enforcing byte, structure, page and text bounds before returning inert text.
using System.Text;

using RagChallenge.Application.Documents;
using RagChallenge.Domain.CorpusCatalog;

using UglyToad.PdfPig;

namespace RagChallenge.Infrastructure.Documents;

public sealed class PdfPigDocumentParser : IDocumentParser
{
    public const string CompatibilityDescriptor =
        "pdfpig/0.1.15;format=pdf;envelope=pdf-header-eof-v1;text=page-text;pages=preserved";

    private static readonly byte[] PdfSignature = Encoding.ASCII.GetBytes("%PDF-");
    private static readonly byte[] EndOfFileMarker = Encoding.ASCII.GetBytes("%%EOF");

    public DocumentFormat Format => DocumentFormat.Pdf;

    public async Task<ParsedDocumentArtifact> ParseAsync(
        Stream content,
        ParserPolicy policy,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(policy);
        var bytes = await BoundedDocumentReader
            .ReadAsync(content, policy.MaximumByteLength, cancellationToken)
            .ConfigureAwait(false);

        ValidateEnvelope(bytes);

        try
        {
            using var stream = new MemoryStream(bytes, writable: false);
            using var document = PdfDocument.Open(stream);

            if (document.NumberOfPages is <= 0 ||
                document.NumberOfPages > policy.MaximumUnits)
            {
                throw new DocumentParseException(
                    DocumentParseFailureKind.LimitExceeded);
            }

            var units = new List<ParsedDocumentUnit>(document.NumberOfPages);
            var totalCharacters = 0;

            for (var pageNumber = 1; pageNumber <= document.NumberOfPages; pageNumber++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var text = NormaliseText(document.GetPage(pageNumber).Text);

                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                totalCharacters = checked(totalCharacters + text.Length);

                if (totalCharacters > policy.MaximumTextCharacters)
                {
                    throw new DocumentParseException(
                        DocumentParseFailureKind.LimitExceeded);
                }

                units.Add(new ParsedDocumentUnit(
                    units.Count,
                    text,
                    pageNumber: pageNumber));
            }

            if (units.Count == 0)
            {
                throw new DocumentParseException(
                    DocumentParseFailureKind.NoExtractableText);
            }

            return new ParsedDocumentArtifact(
                DocumentFormat.Pdf,
                CompatibilityDescriptor,
                units);
        }
        catch (DocumentParseException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            throw new DocumentParseException(DocumentParseFailureKind.MalformedContent);
        }
    }

    private static void ValidateEnvelope(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < PdfSignature.Length + EndOfFileMarker.Length ||
            !bytes.StartsWith(PdfSignature))
        {
            throw new DocumentParseException(DocumentParseFailureKind.MalformedContent);
        }

        var tailLength = Math.Min(bytes.Length, 1_024);
        var tail = bytes[^tailLength..];

        if (tail.LastIndexOf(EndOfFileMarker) < 0)
        {
            throw new DocumentParseException(DocumentParseFailureKind.MalformedContent);
        }
    }

    private static string NormaliseText(string text) =>
        text.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Trim();
}
