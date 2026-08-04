// Purpose: Encodes citation location metadata inside the existing derived chunk-text column while preserving the canonical logical text seen by Application and generation hashing.
using System.Text.Json;

using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Infrastructure.Persistence;

internal static class StoredVectorChunkCodec
{
    private const string Prefix = "RAG-CHUNK-V1:";

    public static string Encode(VectorChunkWrite chunk)
    {
        var metadata = new StoredChunkMetadata(
            chunk.ContentLanguage?.ToCanonicalTag(),
            chunk.PageNumber,
            chunk.RecordNumber,
            chunk.Columns is null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string>(chunk.Columns, StringComparer.Ordinal));
        var json = JsonSerializer.SerializeToUtf8Bytes(metadata);
        return string.Concat(
            Prefix,
            Convert.ToBase64String(json),
            "\n",
            chunk.ChunkText);
    }

    public static DecodedStoredChunk Decode(string storedValue)
    {
        ArgumentNullException.ThrowIfNull(storedValue);

        if (!storedValue.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return new DecodedStoredChunk(
                storedValue,
                ContentLanguage: null,
                PageNumber: null,
                RecordNumber: null,
                new Dictionary<string, string>());
        }

        var separator = storedValue.IndexOf('\n', Prefix.Length);

        if (separator < 0)
        {
            throw new InvalidDataException("Stored chunk metadata is incomplete.");
        }

        try
        {
            var metadataBytes = Convert.FromBase64String(
                storedValue[Prefix.Length..separator]);
            var metadata = JsonSerializer.Deserialize<StoredChunkMetadata>(metadataBytes) ??
                throw new InvalidDataException("Stored chunk metadata is missing.");
            var language = metadata.ContentLanguage switch
            {
                null => (SupportedLanguage?)null,
                "pt-BR" => SupportedLanguage.PtBr,
                "en-GB" => SupportedLanguage.EnGb,
                _ => throw new InvalidDataException(
                    "Stored chunk metadata contains an unsupported language."),
            };
            return new DecodedStoredChunk(
                storedValue[(separator + 1)..],
                language,
                metadata.PageNumber,
                metadata.RecordNumber,
                metadata.Columns ?? new Dictionary<string, string>());
        }
        catch (FormatException exception)
        {
            throw new InvalidDataException("Stored chunk metadata is not canonical Base64.", exception);
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("Stored chunk metadata is not valid JSON.", exception);
        }
    }

    internal sealed record DecodedStoredChunk(
        string Text,
        SupportedLanguage? ContentLanguage,
        int? PageNumber,
        long? RecordNumber,
        IReadOnlyDictionary<string, string> Columns);

    private sealed record StoredChunkMetadata(
        string? ContentLanguage,
        int? PageNumber,
        long? RecordNumber,
        IReadOnlyDictionary<string, string> Columns);
}
