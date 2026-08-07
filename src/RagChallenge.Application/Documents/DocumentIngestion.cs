// Purpose: Orchestrates immutable content persistence, verified reopen, parser selection and deterministic chunking without owning parser or storage implementations.
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;

using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Application.Documents;

public sealed record ChunkingPolicy
{
    public const int DefaultTargetScalarCount = 3_200;
    public const int DefaultOverlapScalarCount = 480;
    public const int DefaultHardMaximumScalarCount = 4_000;
    public const string StrategyId = "paragraph-window-v1";
    public const string BoundaryPolicy = "section,paragraph,sentence,word,scalar";
    public const string SeparatorPolicy = "lf-paragraph-v1";
    public const string NormalisationVersion =
        "nfc-lf-horizontal-space-control-space-v1";

    public ChunkingPolicy(
        int targetScalarCount = DefaultTargetScalarCount,
        int overlapScalarCount = DefaultOverlapScalarCount,
        int hardMaximumScalarCount = DefaultHardMaximumScalarCount)
    {
        if (targetScalarCount is < 32 or > 32_768)
        {
            throw new ArgumentOutOfRangeException(nameof(targetScalarCount));
        }

        if (overlapScalarCount < 0 || overlapScalarCount >= targetScalarCount)
        {
            throw new ArgumentOutOfRangeException(nameof(overlapScalarCount));
        }

        if (hardMaximumScalarCount < targetScalarCount ||
            hardMaximumScalarCount > 32_768)
        {
            throw new ArgumentOutOfRangeException(nameof(hardMaximumScalarCount));
        }

        TargetScalarCount = targetScalarCount;
        OverlapScalarCount = overlapScalarCount;
        HardMaximumScalarCount = hardMaximumScalarCount;
        CompatibilityDescriptor = string.Join(
            ';',
            StrategyId,
            $"target-scalars={targetScalarCount.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
            $"overlap-scalars={overlapScalarCount.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
            $"hard-max-scalars={hardMaximumScalarCount.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
            $"boundaries={BoundaryPolicy}",
            $"separator={SeparatorPolicy}",
            $"normalisation={NormalisationVersion}",
            "unit=pdf-page-or-csv-record");
    }

    public int TargetScalarCount { get; }

    public int OverlapScalarCount { get; }

    public int HardMaximumScalarCount { get; }

    public string CompatibilityDescriptor { get; }
}

public sealed record DocumentChunkingContext(
    CorpusId CorpusId,
    DatabaseProductId DatabaseProductId,
    DatabaseProductRevision DatabaseProductRevision,
    DocumentId DocumentId,
    DocumentVersionNumber DocumentVersion,
    DocumentFormat DocumentFormat,
    DocumentContentLanguage ContentLanguage,
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
            var normalised = NormaliseText(unit.Text);
            var scalars = normalised.EnumerateRunes().ToArray();

            foreach (var segment in Split(scalars, policy))
            {
                var ordinal = chunks.Count;
                var digest = new LogicalArtifactDigest(
                    HashChunk(context, unit, ordinal, segment, policy));
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
        ArgumentNullException.ThrowIfNull(context.ContentLanguage);
        ArgumentNullException.ThrowIfNull(context.SourceAdapterId);

        if (!Enum.IsDefined(context.DocumentFormat) ||
            !Enum.IsDefined(context.SourceTrustClass))
        {
            throw new ArgumentException(
                "The chunking context contains an unsupported classification.",
                nameof(context));
        }
    }

    private static IEnumerable<string> Split(
        Rune[] scalars,
        ChunkingPolicy policy)
    {
        var start = 0;

        while (start < scalars.Length)
        {
            var remaining = scalars.Length - start;
            int end;

            if (remaining <= policy.HardMaximumScalarCount)
            {
                end = scalars.Length;
            }
            else
            {
                var target = checked(start + policy.TargetScalarCount);
                var lower = checked(
                    start + policy.TargetScalarCount - policy.OverlapScalarCount);
                var upper = checked(start + policy.HardMaximumScalarCount);
                end = SelectBoundary(scalars, lower, target, upper);
            }

            var segment = Materialise(scalars, start, end);

            if (!string.IsNullOrWhiteSpace(segment))
            {
                yield return segment;
            }

            if (end == scalars.Length)
            {
                yield break;
            }

            start = checked(end - policy.OverlapScalarCount);
        }
    }

    private static int SelectBoundary(
        Rune[] scalars,
        int lower,
        int target,
        int upper)
    {
        foreach (var kind in Enum.GetValues<ChunkBoundaryKind>())
        {
            var selected = -1;
            var selectedDistance = int.MaxValue;

            for (var position = lower; position <= upper; position++)
            {
                if (!IsBoundary(scalars, position, kind))
                {
                    continue;
                }

                var distance = Math.Abs(position - target);

                if (distance < selectedDistance ||
                    (distance == selectedDistance && position < selected))
                {
                    selected = position;
                    selectedDistance = distance;
                }
            }

            if (selected >= 0)
            {
                return selected;
            }
        }

        return target;
    }

    private static bool IsBoundary(
        Rune[] scalars,
        int position,
        ChunkBoundaryKind kind)
    {
        if (position <= 0 || position > scalars.Length)
        {
            return false;
        }

        var previous = scalars[position - 1].Value;

        return kind switch
        {
            ChunkBoundaryKind.Section =>
                position >= 2 && previous == '\n' && scalars[position - 2].Value == '\n',
            ChunkBoundaryKind.Paragraph => previous == '\n',
            ChunkBoundaryKind.Sentence =>
                previous is '.' or '?' or '!' &&
                (position == scalars.Length || IsSeparator(scalars[position])),
            ChunkBoundaryKind.Word => IsSeparator(scalars[position - 1]),
            ChunkBoundaryKind.Scalar => true,
            _ => false,
        };
    }

    private static bool IsSeparator(Rune value) =>
        value.Value == '\n' || Rune.IsWhiteSpace(value);

    private static string Materialise(Rune[] scalars, int start, int end)
    {
        var builder = new StringBuilder(end - start);

        for (var index = start; index < end; index++)
        {
            builder.Append(scalars[index].ToString());
        }

        return builder.ToString();
    }

    private static string NormaliseText(string value)
    {
        var canonical = value.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Normalize(NormalizationForm.FormC);
        var builder = new StringBuilder(canonical.Length);
        var pendingSpace = false;

        foreach (var scalar in canonical.EnumerateRunes())
        {
            if (scalar.Value is '\n' or 0x2028 or 0x2029)
            {
                TrimTrailingSpace(builder);
                pendingSpace = false;

                if (builder.Length > 0 &&
                    (builder[^1] != '\n' ||
                        (builder.Length < 2 || builder[^2] != '\n')))
                {
                    builder.Append('\n');
                }

                continue;
            }

            if (Rune.IsWhiteSpace(scalar) || IsControl(scalar))
            {
                pendingSpace = builder.Length > 0 && builder[^1] != '\n';
                continue;
            }

            if (pendingSpace)
            {
                builder.Append(' ');
                pendingSpace = false;
            }

            builder.Append(scalar.ToString());
        }

        return builder.ToString().Trim(' ', '\n');
    }

    private static bool IsControl(Rune scalar) =>
        scalar.Value < 0x20 || scalar.Value is >= 0x7f and <= 0x9f;

    private static void TrimTrailingSpace(StringBuilder builder)
    {
        if (builder.Length > 0 && builder[^1] == ' ')
        {
            builder.Length--;
        }
    }

    private static string HashChunk(
        DocumentChunkingContext context,
        ParsedDocumentUnit unit,
        long ordinal,
        string text,
        ChunkingPolicy policy)
    {
        var material = string.Join(
            '\n',
            "rag-chunk-v2",
            policy.CompatibilityDescriptor,
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

    private enum ChunkBoundaryKind
    {
        Section,
        Paragraph,
        Sentence,
        Word,
        Scalar,
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
