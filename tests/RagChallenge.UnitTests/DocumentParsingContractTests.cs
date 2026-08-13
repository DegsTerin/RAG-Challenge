// Purpose: Proves sanitised parsing failures and ordered format-specific metadata without executing parser packages or persistence.
using RagChallenge.Application.Documents;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.UnitTests;

public sealed class DocumentParsingContractTests
{
    [Fact]
    public void ParsedArtifactsRequireOrderedOrdinalsAndIncreasingLocations()
    {
        Assert.Throws<ArgumentException>(() => PdfArtifact(
            new ParsedDocumentUnit(1, "page one", pageNumber: 1)));
        Assert.Throws<ArgumentException>(() => PdfArtifact(
            new ParsedDocumentUnit(0, "first", pageNumber: 1),
            new ParsedDocumentUnit(1, "duplicate", pageNumber: 1)));
        Assert.Throws<ArgumentException>(() => PdfArtifact(
            new ParsedDocumentUnit(0, "third", pageNumber: 3),
            new ParsedDocumentUnit(1, "second", pageNumber: 2)));
        Assert.Throws<ArgumentException>(() => CsvArtifact(
            new ParsedDocumentUnit(
                0,
                "second",
                recordNumber: 2,
                columns: Columns("second")),
            new ParsedDocumentUnit(
                1,
                "first",
                recordNumber: 1,
                columns: Columns("first"))));

        var withPhysicalPageGap = PdfArtifact(
            new ParsedDocumentUnit(0, "first", pageNumber: 1),
            new ParsedDocumentUnit(1, "third", pageNumber: 3));

        Assert.Equal([1, 3], withPhysicalPageGap.Units.Select(unit => unit.PageNumber));
    }

    [Fact]
    public void ParsedArtifactsRejectFormatIncompatibleMetadata()
    {
        Assert.Throws<ArgumentException>(() => PdfArtifact(
            new ParsedDocumentUnit(
                0,
                "record",
                recordNumber: 1,
                columns: new Dictionary<string, string> { ["id"] = "one" })));
        Assert.Throws<ArgumentException>(() => new ParsedDocumentArtifact(
            DocumentFormat.Csv,
            "synthetic-csv/1",
            [new ParsedDocumentUnit(0, "record", recordNumber: 1)]));
    }

    [Theory]
    [InlineData(
        DocumentParseFailureKind.UnsupportedFormat,
        "The document format is not supported.")]
    [InlineData(
        DocumentParseFailureKind.LimitExceeded,
        "The document exceeded an authorised parsing limit.")]
    [InlineData(
        DocumentParseFailureKind.MalformedContent,
        "The document content is malformed.")]
    [InlineData(
        DocumentParseFailureKind.NoExtractableText,
        "The document contains no extractable text.")]
    public void ParseFailuresExposeOnlyCanonicalMessages(
        DocumentParseFailureKind failureKind,
        string expectedMessage)
    {
        var exception = new DocumentParseException(failureKind);

        Assert.Equal(failureKind, exception.FailureKind);
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData(ContentInputFailureKind.Empty, "The content input is empty.")]
    [InlineData(
        ContentInputFailureKind.LimitExceeded,
        "The content input exceeded its authorised byte limit.")]
    [InlineData(
        ContentInputFailureKind.IdentityMismatch,
        "The content input did not match its expected identity.")]
    public void ContentInputFailuresRemainSanitisedAndTyped(
        ContentInputFailureKind failureKind,
        string expectedMessage)
    {
        var exception = new ContentInputException(failureKind);

        Assert.Equal(failureKind, exception.FailureKind);
        Assert.Equal(expectedMessage, exception.Message);
    }

    private static ParsedDocumentArtifact PdfArtifact(params ParsedDocumentUnit[] units) =>
        new(DocumentFormat.Pdf, "synthetic-pdf/1", units);

    private static ParsedDocumentArtifact CsvArtifact(params ParsedDocumentUnit[] units) =>
        new(DocumentFormat.Csv, "synthetic-csv/1", units);

    private static Dictionary<string, string> Columns(string value) =>
        new Dictionary<string, string> { ["id"] = value };
}
