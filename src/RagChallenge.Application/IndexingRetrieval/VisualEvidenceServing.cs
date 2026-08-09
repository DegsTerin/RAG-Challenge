// Purpose: Defines the fail-closed application port for resolving one currently authorised visual-evidence page without exposing storage paths or internal evidence records.
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.Application.IndexingRetrieval;

public sealed record VisualEvidenceSelector(
    IndexGenerationId IndexGenerationId,
    RenderManifestId RenderManifestId,
    int PageNumber,
    ContentObjectId ImageContentObjectId);

public enum VisualEvidenceReadOutcome
{
    Available,
    NotAvailable,
    Unavailable,
}

public sealed class VisualEvidenceContent : IAsyncDisposable
{
    public VisualEvidenceContent(
        VerifiedContentObject content,
        string mediaType,
        int widthPixels,
        int heightPixels)
    {
        Content = content ?? throw new ArgumentNullException(nameof(content));

        if (!string.Equals(mediaType, DocumentPageImage.PngMediaType, StringComparison.Ordinal) ||
            widthPixels is <= 0 or > DocumentPageImage.MaximumDimensionPixels ||
            heightPixels is <= 0 or > DocumentPageImage.MaximumDimensionPixels)
        {
            throw new ArgumentException("Visual evidence metadata is outside its frozen bounds.");
        }

        MediaType = mediaType;
        WidthPixels = widthPixels;
        HeightPixels = heightPixels;
    }

    public VerifiedContentObject Content { get; }

    public string MediaType { get; }

    public int WidthPixels { get; }

    public int HeightPixels { get; }

    public ValueTask DisposeAsync() => Content.DisposeAsync();
}

public sealed record VisualEvidenceReadResult(
    VisualEvidenceReadOutcome Outcome,
    VisualEvidenceContent? Evidence)
{
    public static VisualEvidenceReadResult Available(VisualEvidenceContent evidence) =>
        new(VisualEvidenceReadOutcome.Available, evidence);

    public static VisualEvidenceReadResult NotAvailable() =>
        new(VisualEvidenceReadOutcome.NotAvailable, Evidence: null);

    public static VisualEvidenceReadResult Unavailable() =>
        new(VisualEvidenceReadOutcome.Unavailable, Evidence: null);
}

public interface IVisualEvidenceReader
{
    Task<VisualEvidenceReadResult> ReadAsync(
        VisualEvidenceSelector selector,
        DateTimeOffset observedAt,
        CancellationToken cancellationToken = default);
}
