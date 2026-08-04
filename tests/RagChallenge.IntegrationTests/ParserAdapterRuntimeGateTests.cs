// Purpose: Executes the first S04-A gate against synthetic bounded streams, proving valid extraction, malformed-input guards and literal CSV preservation.
using RagChallenge.Application.Documents;
using RagChallenge.Infrastructure.Documents;

namespace RagChallenge.IntegrationTests;

[Collection(ParserRuntimeGateSerialisation.Name)]
public sealed class ParserAdapterRuntimeGateTests
{
    private static readonly ParserPolicy PdfPolicy = new(
        maximumByteLength: 262_144,
        maximumUnits: 8,
        maximumTextCharacters: 32_768);

    private static readonly ParserPolicy CsvPolicy = new(
        maximumByteLength: 131_072,
        maximumUnits: 8,
        maximumTextCharacters: 32_768,
        maximumFieldsPerRecord: 8,
        maximumFieldCharacters: 4_096);

    [Fact]
    public async Task SelectedParsersPassTheSyntheticRuntimeGate()
    {
        var initialFiles = Directory.GetFiles(
            Environment.CurrentDirectory,
            "*",
            SearchOption.TopDirectoryOnly).ToHashSet(StringComparer.OrdinalIgnoreCase);

        await VerifyPdfCasesAsync();
        await VerifyCsvCasesAsync();

        var finalFiles = Directory.GetFiles(
            Environment.CurrentDirectory,
            "*",
            SearchOption.TopDirectoryOnly).ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.Equal(initialFiles, finalFiles);
    }

    private static async Task VerifyPdfCasesAsync()
    {
        var parser = new PdfPigDocumentParser();
        var allowedPackageAssemblies = new HashSet<string>(StringComparer.Ordinal)
        {
            "UglyToad.PdfPig",
            "UglyToad.PdfPig.Core",
            "UglyToad.PdfPig.DocumentLayoutAnalysis",
            "UglyToad.PdfPig.Fonts",
            "UglyToad.PdfPig.Package",
            "UglyToad.PdfPig.Tokenization",
            "UglyToad.PdfPig.Tokens",
        };

        var before = LoadedAssemblyNames();
        await using (var stream = new MemoryStream(
            SyntheticParserFixtureFactory.CreatePdf("ONEPAGE"),
            writable: false))
        {
            var result = await parser.ParseAsync(stream, PdfPolicy);
            Assert.Single(result.Units);
            Assert.Equal(1, result.Units[0].PageNumber);
            Assert.Contains("ONEPAGE", result.Units[0].Text, StringComparison.Ordinal);
        }

        await using (var stream = new MemoryStream(
            SyntheticParserFixtureFactory.CreatePdf("FIRSTPAGE", "SECONDPAGE"),
            writable: false))
        {
            var result = await parser.ParseAsync(stream, PdfPolicy);
            Assert.Equal(2, result.Units.Count);
            Assert.Contains("FIRSTPAGE", result.Units[0].Text, StringComparison.Ordinal);
            Assert.Contains("SECONDPAGE", result.Units[1].Text, StringComparison.Ordinal);
        }

        var truncated = SyntheticParserFixtureFactory.CreatePdf("TRUNCATED")[..^8];
        await using (var stream = new MemoryStream(truncated, writable: false))
        {
            var failure = await Assert.ThrowsAsync<DocumentParseException>(
                () => parser.ParseAsync(stream, PdfPolicy));
            Assert.Equal(DocumentParseFailureKind.MalformedContent, failure.FailureKind);
            Assert.DoesNotContain("xref", failure.Message, StringComparison.OrdinalIgnoreCase);
        }

        await using (var stream = new ReadProhibitedStream(262_145))
        {
            var failure = await Assert.ThrowsAsync<DocumentParseException>(
                () => parser.ParseAsync(stream, PdfPolicy));
            Assert.Equal(DocumentParseFailureKind.LimitExceeded, failure.FailureKind);
            Assert.Equal(0, stream.ReadAttempts);
        }

        AssertOnlyExpectedAssemblies(before, allowedPackageAssemblies);
    }

    private static async Task VerifyCsvCasesAsync()
    {
        var parser = new CsvHelperDocumentParser();
        var before = LoadedAssemblyNames();

        await using (var stream = new MemoryStream(
            SyntheticParserFixtureFactory.CsvValidQuotedUtf8,
            writable: false))
        {
            var result = await parser.ParseAsync(stream, CsvPolicy);
            var row = Assert.Single(result.Units);
            Assert.Equal("vírgula, preservada e \"aspas\"", row.Columns["description"]);
        }

        await using (var stream = new MemoryStream(
            SyntheticParserFixtureFactory.CsvFormulaLiteral,
            writable: false))
        {
            var result = await parser.ParseAsync(stream, CsvPolicy);
            Assert.Equal("=1+1", Assert.Single(result.Units).Columns["value"]);
        }

        await using (var stream = new MemoryStream(
            SyntheticParserFixtureFactory.CsvMalformedQuote,
            writable: false))
        {
            var failure = await Assert.ThrowsAsync<DocumentParseException>(
                () => parser.ParseAsync(stream, CsvPolicy));
            Assert.Equal(DocumentParseFailureKind.MalformedContent, failure.FailureKind);
            Assert.DoesNotContain("unterminated", failure.Message, StringComparison.OrdinalIgnoreCase);
        }

        await using (var stream = new ReadProhibitedStream(131_073))
        {
            var failure = await Assert.ThrowsAsync<DocumentParseException>(
                () => parser.ParseAsync(stream, CsvPolicy));
            Assert.Equal(DocumentParseFailureKind.LimitExceeded, failure.FailureKind);
            Assert.Equal(0, stream.ReadAttempts);
        }

        AssertOnlyExpectedAssemblies(
            before,
            new HashSet<string>(StringComparer.Ordinal) { "CsvHelper" });
    }

    private static HashSet<string> LoadedAssemblyNames() =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetName().Name)
            .Where(name => name is not null)
            .Select(name => name!)
            .ToHashSet(StringComparer.Ordinal);

    private static void AssertOnlyExpectedAssemblies(
        HashSet<string> before,
        HashSet<string> expectedPackageAssemblies)
    {
        var added = LoadedAssemblyNames().Except(before, StringComparer.Ordinal).ToArray();

        Assert.DoesNotContain(
            added,
            name => !expectedPackageAssemblies.Contains(name) &&
                !name.StartsWith("RagChallenge.", StringComparison.Ordinal) &&
                !name.StartsWith("System.", StringComparison.Ordinal) &&
                !name.StartsWith("Microsoft.", StringComparison.Ordinal));
    }

    private sealed class ReadProhibitedStream(long length) : Stream
    {
        public int ReadAttempts { get; private set; }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => length;

        public override long Position { get; set; }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ReadAttempts++;
            throw new InvalidOperationException("The parser must reject before reading.");
        }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();
    }
}
