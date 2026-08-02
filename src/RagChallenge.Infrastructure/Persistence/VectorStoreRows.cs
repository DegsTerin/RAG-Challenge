// Purpose: Defines the derived SQLite vector-build and chunk rows; active-generation authority remains exclusively in the control-plane schema.
namespace RagChallenge.Infrastructure.Persistence;

internal sealed class VectorBuildRow
{
    public required string CandidateBuildId { get; set; }

    public required string CorpusId { get; set; }

    public required string Status { get; set; }

    public string? IndexGenerationId { get; set; }

    public required string IndexCompatibilityKey { get; set; }

    public int VectorDimensions { get; set; }

    public long ExpectedChunkCount { get; set; }

    public required string CreatedAtUtc { get; set; }

    public string? ValidatedAtUtc { get; set; }
}

internal sealed class VectorChunkRow
{
    public required string CandidateBuildId { get; set; }

    public long ChunkOrdinal { get; set; }

    public required string DocumentId { get; set; }

    public long DocumentVersion { get; set; }

    public required string ChunkDigest { get; set; }

    public required string ChunkText { get; set; }

    public required byte[] Vector { get; set; }
}
