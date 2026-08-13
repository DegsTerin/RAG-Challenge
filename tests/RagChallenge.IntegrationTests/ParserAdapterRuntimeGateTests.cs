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
            Assert.Equal(PdfPigDocumentParser.CompatibilityDescriptor, result.ParserDescriptor);
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

        await using (var stream = new MemoryStream(
            SyntheticParserFixtureFactory.CreatePdf("", "SECONDPAGE"),
            writable: false))
        {
            var result = await parser.ParseAsync(stream, PdfPolicy);
            var page = Assert.Single(result.Units);
            Assert.Equal(0, page.Ordinal);
            Assert.Equal(2, page.PageNumber);
            Assert.Contains("SECONDPAGE", page.Text, StringComparison.Ordinal);
        }

        await using (var stream = new MemoryStream(
            SyntheticParserFixtureFactory.CreatePdf(""),
            writable: false))
        {
            var failure = await Assert.ThrowsAsync<DocumentParseException>(
                () => parser.ParseAsync(stream, PdfPolicy));
            Assert.Equal(DocumentParseFailureKind.NoExtractableText, failure.FailureKind);
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
        Assert.Equal(
            "csvhelper/33.1.0;encoding=utf-8-strict;delimiter=comma;header=required;quote=double;escape=double-double;newline=crlf-lf-cr;formula=literal;shape=strict-v1;empty-record=skip-preserve-location-v1",
            CsvHelperDocumentParser.CompatibilityDescriptor);

        await using (var stream = new MemoryStream(
            SyntheticParserFixtureFactory.CsvValidQuotedUtf8,
            writable: false))
        {
            var result = await parser.ParseAsync(stream, CsvPolicy);
            Assert.Equal(CsvHelperDocumentParser.CompatibilityDescriptor, result.ParserDescriptor);
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
            "name,value\r\n,\r\nBanco,stable\r\n"u8.ToArray(),
            writable: false))
        {
            var result = await parser.ParseAsync(stream, CsvPolicy);
            var row = Assert.Single(result.Units);
            Assert.Equal(0, row.Ordinal);
            Assert.Equal(2, row.RecordNumber);
            Assert.Equal("stable", row.Columns["value"]);
        }

        await using (var stream = new MemoryStream(
            "name,description\r\nBanco,\"line one\r\nline two\"\r\n"u8.ToArray(),
            writable: false))
        {
            var result = await parser.ParseAsync(stream, CsvPolicy);
            Assert.Equal(
                "line one\r\nline two",
                Assert.Single(result.Units).Columns["description"]);
        }

        foreach (var emptyDocument in new[]
        {
            Array.Empty<byte>(),
            "\uFEFF"u8.ToArray(),
            "name,value\r\n"u8.ToArray(),
            "name,value\r\n,\r\n"u8.ToArray(),
            "name,value\r\n  ,\t\r\n"u8.ToArray(),
        })
        {
            await using var stream = new MemoryStream(emptyDocument, writable: false);
            var failure = await Assert.ThrowsAsync<DocumentParseException>(
                () => parser.ParseAsync(stream, CsvPolicy));
            Assert.Equal(DocumentParseFailureKind.NoExtractableText, failure.FailureKind);
        }

        await using (var stream = new MemoryStream(
            [.. "name,value\r\nBanco,"u8, 0xff, (byte)'\r', (byte)'\n'],
            writable: false))
        {
            var failure = await Assert.ThrowsAsync<DocumentParseException>(
                () => parser.ParseAsync(stream, CsvPolicy));
            Assert.Equal(DocumentParseFailureKind.MalformedContent, failure.FailureKind);
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

        await using (var stream = new CountingNonSeekableStream(100))
        {
            var policy = new ParserPolicy(32, 8, 32_768, 8, 4_096);
            var failure = await Assert.ThrowsAsync<DocumentParseException>(
                () => parser.ParseAsync(stream, policy));
            Assert.Equal(DocumentParseFailureKind.LimitExceeded, failure.FailureKind);
            Assert.Equal(33, stream.BytesRead);
            Assert.InRange(stream.LargestReadRequest, 1, 33);
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

    private sealed class CountingNonSeekableStream(int length) : Stream
    {
        private int remaining = length;

        public int BytesRead { get; private set; }

        public int LargestReadRequest { get; private set; }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            ReadCore(buffer.AsSpan(offset, count));

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(ReadCore(buffer.Span));
        }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();

        private int ReadCore(Span<byte> buffer)
        {
            LargestReadRequest = Math.Max(LargestReadRequest, buffer.Length);
            var read = Math.Min(buffer.Length, remaining);
            buffer[..read].Fill((byte)'x');
            remaining -= read;
            BytesRead += read;
            return read;
        }
    }
}
