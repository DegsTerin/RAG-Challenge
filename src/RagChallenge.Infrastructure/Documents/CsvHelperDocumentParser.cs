// Purpose: Adapts the pinned CsvHelper package to the Application parsing port while enforcing strict UTF-8, RFC-style quote, shape and output limits.
using System.Globalization;
using System.Text;

using CsvHelper;
using CsvHelper.Configuration;

using RagChallenge.Application.Documents;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Infrastructure.Documents;

public sealed class CsvHelperDocumentParser : IDocumentParser
{
    public const string CompatibilityDescriptor =
        "csvhelper/33.1.0;encoding=utf-8-strict;delimiter=comma;header=required;quote=double;escape=double-double;newline=crlf-lf-cr;formula=literal;shape=strict-v1";

    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    public DocumentFormat Format => DocumentFormat.Csv;

    public async Task<ParsedDocumentArtifact> ParseAsync(
        Stream content,
        ParserPolicy policy,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(policy);
        var bytes = await BoundedDocumentReader
            .ReadAsync(content, policy.MaximumByteLength, cancellationToken)
            .ConfigureAwait(false);

        string text;

        try
        {
            text = StrictUtf8.GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            throw new DocumentParseException(DocumentParseFailureKind.MalformedContent);
        }

        if (text.Length > 0 && text[0] == '\uFEFF')
        {
            text = text[1..];
        }

        ValidateCharacters(text);
        var shape = ValidateShape(text, policy);

        try
        {
            using var textReader = new StringReader(text);
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = true,
            };
            using var csv = new CsvReader(textReader, configuration);

            if (!csv.Read())
            {
                throw new DocumentParseException(
                    DocumentParseFailureKind.MalformedContent);
            }

            csv.ReadHeader();
            var headers = Enumerable.Range(0, shape.FieldCount)
                .Select(index => csv.GetField(index) ?? string.Empty)
                .ToArray();

            if (headers.Any(string.IsNullOrWhiteSpace) ||
                headers.Distinct(StringComparer.Ordinal).Count() != headers.Length)
            {
                throw new DocumentParseException(
                    DocumentParseFailureKind.MalformedContent);
            }

            var units = new List<ParsedDocumentUnit>();
            var totalCharacters = 0;

            while (csv.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (units.Count >= policy.MaximumUnits)
                {
                    throw new DocumentParseException(
                        DocumentParseFailureKind.LimitExceeded);
                }

                var columns = new Dictionary<string, string>(StringComparer.Ordinal);

                for (var index = 0; index < headers.Length; index++)
                {
                    var value = csv.GetField(index) ?? string.Empty;

                    if (value.Length > policy.MaximumFieldCharacters)
                    {
                        throw new DocumentParseException(
                            DocumentParseFailureKind.LimitExceeded);
                    }

                    columns.Add(headers[index], value);
                }

                var rowText = string.Join(
                    " | ",
                    columns.Select(column => $"{column.Key}: {column.Value}"));
                totalCharacters = checked(totalCharacters + rowText.Length);

                if (totalCharacters > policy.MaximumTextCharacters)
                {
                    throw new DocumentParseException(
                        DocumentParseFailureKind.LimitExceeded);
                }

                units.Add(new ParsedDocumentUnit(
                    units.Count,
                    rowText,
                    recordNumber: units.Count + 1L,
                    columns: columns));
            }

            if (units.Count == 0)
            {
                throw new DocumentParseException(
                    DocumentParseFailureKind.NoExtractableText);
            }

            return new ParsedDocumentArtifact(
                DocumentFormat.Csv,
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

    private static void ValidateCharacters(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            throw new DocumentParseException(DocumentParseFailureKind.MalformedContent);
        }

        foreach (var character in text)
        {
            if (character == '\0' ||
                (char.IsControl(character) && character is not '\r' and not '\n' and not '\t'))
            {
                throw new DocumentParseException(DocumentParseFailureKind.MalformedContent);
            }
        }
    }

    private static CsvShape ValidateShape(string text, ParserPolicy policy)
    {
        var fieldCount = 1;
        int? expectedFieldCount = null;
        var recordCount = 0;
        var fieldCharacters = 0;
        var atFieldStart = true;
        var inQuotes = false;
        var quoteClosed = false;

        for (var index = 0; index < text.Length; index++)
        {
            var character = text[index];

            if (inQuotes)
            {
                if (character == '"')
                {
                    if (index + 1 < text.Length && text[index + 1] == '"')
                    {
                        index++;
                        fieldCharacters++;
                    }
                    else
                    {
                        inQuotes = false;
                        quoteClosed = true;
                    }
                }
                else
                {
                    fieldCharacters++;
                }

                EnsureFieldLimit(fieldCharacters, policy);
                continue;
            }

            if (quoteClosed && character is not ',' and not '\r' and not '\n')
            {
                throw new DocumentParseException(DocumentParseFailureKind.MalformedContent);
            }

            if (character == '"')
            {
                if (!atFieldStart)
                {
                    throw new DocumentParseException(
                        DocumentParseFailureKind.MalformedContent);
                }

                inQuotes = true;
                atFieldStart = false;
                continue;
            }

            if (character == ',')
            {
                fieldCount++;
                EnsureFieldCount(fieldCount, policy);
                fieldCharacters = 0;
                atFieldStart = true;
                quoteClosed = false;
                continue;
            }

            if (character is '\r' or '\n')
            {
                if (character == '\r' && index + 1 < text.Length && text[index + 1] == '\n')
                {
                    index++;
                }

                CompleteRecord(ref expectedFieldCount, fieldCount);
                recordCount++;
                fieldCount = 1;
                fieldCharacters = 0;
                atFieldStart = true;
                quoteClosed = false;
                continue;
            }

            fieldCharacters++;
            EnsureFieldLimit(fieldCharacters, policy);
            atFieldStart = false;
        }

        if (inQuotes)
        {
            throw new DocumentParseException(DocumentParseFailureKind.MalformedContent);
        }

        if (!atFieldStart || quoteClosed || fieldCount > 1)
        {
            CompleteRecord(ref expectedFieldCount, fieldCount);
            recordCount++;
        }

        if (expectedFieldCount is null || recordCount < 2)
        {
            throw new DocumentParseException(DocumentParseFailureKind.NoExtractableText);
        }

        if (recordCount - 1 > policy.MaximumUnits)
        {
            throw new DocumentParseException(DocumentParseFailureKind.LimitExceeded);
        }

        return new CsvShape(expectedFieldCount.Value, recordCount);
    }

    private static void CompleteRecord(ref int? expectedFieldCount, int fieldCount)
    {
        if (expectedFieldCount is null)
        {
            expectedFieldCount = fieldCount;
        }
        else if (expectedFieldCount.Value != fieldCount)
        {
            throw new DocumentParseException(DocumentParseFailureKind.MalformedContent);
        }
    }

    private static void EnsureFieldCount(int fieldCount, ParserPolicy policy)
    {
        if (fieldCount > policy.MaximumFieldsPerRecord)
        {
            throw new DocumentParseException(DocumentParseFailureKind.LimitExceeded);
        }
    }

    private static void EnsureFieldLimit(int fieldCharacters, ParserPolicy policy)
    {
        if (fieldCharacters > policy.MaximumFieldCharacters)
        {
            throw new DocumentParseException(DocumentParseFailureKind.LimitExceeded);
        }
    }

    private sealed record CsvShape(int FieldCount, int RecordCount);
}
