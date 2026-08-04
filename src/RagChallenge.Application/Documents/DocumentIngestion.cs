// Purpose: Orchestrates immutable content persistence, verified reopen, parser selection and deterministic chunking without owning parser or storage implementations.
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;

using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Application.Documents;

public sealed record ChunkingPolicy
{
    public ChunkingPolicy(int maximumCharactersPerChunk)
    {
        if (maximumCharactersPerChunk is < 32 or > 32_768)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumCharactersPerChunk));
        }

        MaximumCharactersPerChunk = maximumCharactersPerChunk;
    }

    public int MaximumCharactersPerChunk { get; }
}

public sealed record DocumentChunkingContext(
    CorpusId CorpusId,
    DatabaseProductId DatabaseProductId,
    DatabaseProductRevision DatabaseProductRevision,
    DocumentId DocumentId,
    DocumentVersionNumber DocumentVersion,
    DocumentFormat DocumentFormat,
    SupportedLanguage ContentLanguage,
    SourceAdapterId SourceAdapterId,
    SourceTrustClass SourceTrustClass);

public sealed class DocumentChunk
{
    public DocumentChunk(
        long ordinal,
        LogicalArtifactDigest digest,
        string text,
        int? pageNumber,
        long? recordNumber,
        IReadOnlyDictionary<string, string> columns)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(ordinal);
        ArgumentNullException.ThrowIfNull(digest);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentNullException.ThrowIfNull(columns);
        Ordinal = ordinal;
        Digest = digest;
        Text = text;
        PageNumber = pageNumber;
        RecordNumber = recordNumber;
        Columns = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(columns, StringComparer.Ordinal));
    }

    public long Ordinal { get; }

    public LogicalArtifactDigest Digest { get; }

    public string Text { get; }

    public int? PageNumber { get; }

    public long? RecordNumber { get; }

    public ReadOnlyDictionary<string, string> Columns { get; }
}

public interface IChunkingStrategy
{
    IReadOnlyList<DocumentChunk> Chunk(
        ParsedDocumentArtifact artifact,
        DocumentChunkingContext context,
        ChunkingPolicy policy);
}

public sealed class DeterministicChunkingStrategy : IChunkingStrategy
{
    public IReadOnlyList<DocumentChunk> Chunk(
        ParsedDocumentArtifact artifact,
        DocumentChunkingContext context,
        ChunkingPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(artifact);
        ValidateContext(context);
        ArgumentNullException.ThrowIfNull(policy);

        if (artifact.Format != context.DocumentFormat)
        {
            throw new ArgumentException(
                "The parsed format must match the document chunking context.",
                nameof(context));
        }

        var chunks = new List<DocumentChunk>();

        foreach (var unit in artifact.Units.OrderBy(unit => unit.Ordinal))
        {
            var normalised = NormaliseWhitespace(unit.Text);

            foreach (var segment in Split(normalised, policy.MaximumCharactersPerChunk))
            {
                var ordinal = chunks.Count;
                var digest = new LogicalArtifactDigest(
                    HashChunk(context, unit, ordinal, segment));
                chunks.Add(new DocumentChunk(
                    ordinal,
                    digest,
                    segment,
                    unit.PageNumber,
                    unit.RecordNumber,
                    unit.Columns));
            }
        }

        if (chunks.Count == 0)
        {
            throw new DocumentParseException(DocumentParseFailureKind.NoExtractableText);
        }

        return chunks.AsReadOnly();
    }

    private static void ValidateContext(DocumentChunkingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.CorpusId);
        ArgumentNullException.ThrowIfNull(context.DatabaseProductId);
        ArgumentNullException.ThrowIfNull(context.DatabaseProductRevision);
        ArgumentNullException.ThrowIfNull(context.DocumentId);
        ArgumentNullException.ThrowIfNull(context.DocumentVersion);
        ArgumentNullException.ThrowIfNull(context.SourceAdapterId);

        if (!Enum.IsDefined(context.DocumentFormat) ||
            !Enum.IsDefined(context.ContentLanguage) ||
            !Enum.IsDefined(context.SourceTrustClass))
        {
            throw new ArgumentException(
                "The chunking context contains an unsupported classification.",
                nameof(context));
        }
    }

    private static IEnumerable<string> Split(string text, int maximumLength)
    {
        var position = 0;

        while (position < text.Length)
        {
            var length = Math.Min(maximumLength, text.Length - position);

            if (position + length < text.Length)
            {
                var boundary = text.LastIndexOf(' ', position + length - 1, length);

                if (boundary >= position + (maximumLength / 2))
                {
                    length = boundary - position;
                }
            }

            var segment = text.Substring(position, length).Trim();

            if (segment.Length > 0)
            {
                yield return segment;
            }

            position += length;

            while (position < text.Length && text[position] == ' ')
            {
                position++;
            }
        }
    }

    private static string NormaliseWhitespace(string value)
    {
        var builder = new StringBuilder(value.Length);
        var pendingSpace = false;

        foreach (var character in value.Normalize(NormalizationForm.FormC))
        {
            if (char.IsWhiteSpace(character))
            {
                pendingSpace = builder.Length > 0;
                continue;
            }

            if (pendingSpace)
            {
                builder.Append(' ');
                pendingSpace = false;
            }

            builder.Append(character);
        }

        return builder.ToString();
    }

    private static string HashChunk(
        DocumentChunkingContext context,
        ParsedDocumentUnit unit,
        long ordinal,
        string text)
    {
        var material = string.Join(
            '\n',
            "rag-chunk-v1",
            context.CorpusId.Value,
            context.DatabaseProductId.Value,
            context.DatabaseProductRevision.ToCanonicalString(),
            context.DocumentId.Value,
            context.DocumentVersion.ToCanonicalString(),
            context.DocumentFormat.ToString(),
            context.ContentLanguage.ToCanonicalTag(),
            context.SourceAdapterId.Value,
            context.SourceTrustClass.ToString(),
            ordinal.ToString(System.Globalization.CultureInfo.InvariantCulture),
            unit.PageNumber?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "",
            unit.RecordNumber?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "",
            text);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(material)))
            .ToLowerInvariant();
    }
}

public sealed record DocumentIngestionRequest(
    Stream Content,
    long MaximumByteLength,
    ParserPolicy ParserPolicy,
    ChunkingPolicy ChunkingPolicy,
    DocumentChunkingContext ChunkingContext,
    ContentObjectId? ExpectedContentObjectId = null);

public sealed class DocumentIngestionResult
{
    public DocumentIngestionResult(
        ContentWriteResult content,
        ParsedDocumentArtifact parsedArtifact,
        IReadOnlyList<DocumentChunk> chunks)
    {
        Content = content;
        ParsedArtifact = parsedArtifact;
        Chunks = chunks;
    }

    public ContentWriteResult Content { get; }

    public ParsedDocumentArtifact ParsedArtifact { get; }

    public IReadOnlyList<DocumentChunk> Chunks { get; }
}

public sealed class DocumentIngestionService
{
    private readonly IImmutableContentStore contentStore;
    private readonly Dictionary<DocumentFormat, IDocumentParser> parsers;
    private readonly IChunkingStrategy chunkingStrategy;

    public DocumentIngestionService(
        IImmutableContentStore contentStore,
        IEnumerable<IDocumentParser> parsers,
        IChunkingStrategy chunkingStrategy)
    {
        this.contentStore =
            contentStore ?? throw new ArgumentNullException(nameof(contentStore));
        ArgumentNullException.ThrowIfNull(parsers);
        this.chunkingStrategy =
            chunkingStrategy ?? throw new ArgumentNullException(nameof(chunkingStrategy));

        var materialisedParsers = parsers.ToArray();

        if (materialisedParsers.Length == 0 ||
            materialisedParsers.Select(parser => parser.Format).Distinct().Count() !=
                materialisedParsers.Length)
        {
            throw new ArgumentException(
                "Each supported document format requires exactly one parser.",
                nameof(parsers));
        }

        this.parsers = materialisedParsers.ToDictionary(parser => parser.Format);
    }

    public async Task<DocumentIngestionResult> IngestAsync(
        DocumentIngestionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Content);
        ArgumentNullException.ThrowIfNull(request.ParserPolicy);
        ArgumentNullException.ThrowIfNull(request.ChunkingPolicy);
        ArgumentNullException.ThrowIfNull(request.ChunkingContext);

        if (request.MaximumByteLength != request.ParserPolicy.MaximumByteLength)
        {
            throw new ArgumentException(
                "Persistence and parsing must use the same byte limit.",
                nameof(request));
        }

        if (!parsers.TryGetValue(request.ChunkingContext.DocumentFormat, out var parser))
        {
            throw new DocumentParseException(
                DocumentParseFailureKind.UnsupportedFormat);
        }

        var content = await contentStore.PutAsync(
            request.Content,
            request.MaximumByteLength,
            request.ExpectedContentObjectId,
            cancellationToken).ConfigureAwait(false);
        await using var verified = await contentStore
            .OpenReadAsync(content.ContentObjectId, cancellationToken)
            .ConfigureAwait(false);
        var parsed = await parser
            .ParseAsync(verified, request.ParserPolicy, cancellationToken)
            .ConfigureAwait(false);
        var chunks = chunkingStrategy.Chunk(
            parsed,
            request.ChunkingContext,
            request.ChunkingPolicy);
        return new DocumentIngestionResult(content, parsed, chunks);
    }
}
