// Purpose: Defines bounded, provider-neutral document parsing contracts owned by Application; concrete PDF and CSV libraries remain Infrastructure details.
using System.Collections.ObjectModel;

using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Application.Documents;

public enum DocumentParseFailureKind
{
    UnsupportedFormat,
    LimitExceeded,
    MalformedContent,
    NoExtractableText,
}

public sealed class DocumentParseException : Exception
{
    public DocumentParseException(DocumentParseFailureKind failureKind)
        : base(ToSanitisedMessage(failureKind))
    {
        FailureKind = failureKind;
    }

    public DocumentParseFailureKind FailureKind { get; }

    private static string ToSanitisedMessage(DocumentParseFailureKind failureKind) =>
        failureKind switch
        {
            DocumentParseFailureKind.UnsupportedFormat =>
                "The document format is not supported.",
            DocumentParseFailureKind.LimitExceeded =>
                "The document exceeded an authorised parsing limit.",
            DocumentParseFailureKind.MalformedContent =>
                "The document content is malformed.",
            DocumentParseFailureKind.NoExtractableText =>
                "The document contains no extractable text.",
            _ => "The document could not be parsed.",
        };
}

public sealed record ParserPolicy
{
    public ParserPolicy(
        long maximumByteLength,
        int maximumUnits,
        int maximumTextCharacters,
        int maximumFieldsPerRecord = 256,
        int maximumFieldCharacters = 16_384)
    {
        if (maximumByteLength is <= 0 or > 32 * 1024 * 1024)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumByteLength));
        }

        if (maximumUnits is <= 0 or > 100_000)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumUnits));
        }

        if (maximumTextCharacters is <= 0 or > 16_000_000)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumTextCharacters));
        }

        if (maximumFieldsPerRecord is <= 0 or > 1_024)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumFieldsPerRecord));
        }

        if (maximumFieldCharacters is <= 0 or > 1_048_576)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumFieldCharacters));
        }

        MaximumByteLength = maximumByteLength;
        MaximumUnits = maximumUnits;
        MaximumTextCharacters = maximumTextCharacters;
        MaximumFieldsPerRecord = maximumFieldsPerRecord;
        MaximumFieldCharacters = maximumFieldCharacters;
    }

    public long MaximumByteLength { get; }

    public int MaximumUnits { get; }

    public int MaximumTextCharacters { get; }

    public int MaximumFieldsPerRecord { get; }

    public int MaximumFieldCharacters { get; }
}

public sealed class ParsedDocumentUnit
{
    public ParsedDocumentUnit(
        int ordinal,
        string text,
        int? pageNumber = null,
        long? recordNumber = null,
        IReadOnlyDictionary<string, string>? columns = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(ordinal);

        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        if (pageNumber is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        }

        if (recordNumber is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(recordNumber));
        }

        if ((pageNumber is null) == (recordNumber is null))
        {
            throw new ArgumentException(
                "A parsed unit requires exactly one format-specific location.");
        }

        Ordinal = ordinal;
        Text = text;
        PageNumber = pageNumber;
        RecordNumber = recordNumber;
        Columns = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(
                columns ?? new Dictionary<string, string>(),
                StringComparer.Ordinal));
    }

    public int Ordinal { get; }

    public string Text { get; }

    public int? PageNumber { get; }

    public long? RecordNumber { get; }

    public ReadOnlyDictionary<string, string> Columns { get; }
}

public sealed class ParsedDocumentArtifact
{
    public ParsedDocumentArtifact(
        DocumentFormat format,
        string parserDescriptor,
        IEnumerable<ParsedDocumentUnit> units,
        IEnumerable<string>? warnings = null)
    {
        if (!Enum.IsDefined(format))
        {
            throw new ArgumentOutOfRangeException(nameof(format));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(parserDescriptor);
        ArgumentNullException.ThrowIfNull(units);

        var materialisedUnits = units.ToArray();

        if (materialisedUnits.Length == 0)
        {
            throw new ArgumentException(
                "A parsed artifact must contain at least one unit.",
                nameof(units));
        }

        if (materialisedUnits.Select(unit => unit.Ordinal).Distinct().Count() !=
            materialisedUnits.Length)
        {
            throw new ArgumentException(
                "Parsed unit ordinals must be unique.",
                nameof(units));
        }

        Format = format;
        ParserDescriptor = parserDescriptor;
        Units = Array.AsReadOnly(materialisedUnits);
        Warnings = Array.AsReadOnly(warnings?.Distinct(StringComparer.Ordinal).ToArray() ?? []);
    }

    public DocumentFormat Format { get; }

    public string ParserDescriptor { get; }

    public ReadOnlyCollection<ParsedDocumentUnit> Units { get; }

    public ReadOnlyCollection<string> Warnings { get; }
}

public interface IDocumentParser
{
    DocumentFormat Format { get; }

    Task<ParsedDocumentArtifact> ParseAsync(
        Stream content,
        ParserPolicy policy,
        CancellationToken cancellationToken = default);
}
