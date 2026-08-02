// Purpose: Models non-queryable candidate builds, immutable validated generation manifests, and deterministic final generation identity in Domain.
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Domain.IndexingRetrieval;

public enum IndexBuildStatus
{
    Candidate,
    Validated,
    Failed,
}

public sealed class FinalisedIndexGenerationManifest
{
    public FinalisedIndexGenerationManifest(
        int manifestSchemaVersion,
        CorpusId corpusId,
        CorpusRevision corpusRevision,
        CatalogueRevision catalogueRevision,
        ActiveDocumentSetDigest activeDocumentSetDigest,
        SourceBindingSetDigest sourceBindingSetDigest,
        IndexCompatibilityKey indexCompatibilityKey,
        GenerationSpecDigest generationSpecDigest,
        long chunkCount,
        long vectorCount,
        LogicalArtifactDigest logicalArtifactDigest,
        GenerationContentDigest generationContentDigest,
        IndexGenerationId indexGenerationId)
    {
        if (manifestSchemaVersion <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(manifestSchemaVersion),
                manifestSchemaVersion,
                "A manifest schema version must be positive.");
        }

        ArgumentNullException.ThrowIfNull(corpusId);
        ArgumentNullException.ThrowIfNull(corpusRevision);
        ArgumentNullException.ThrowIfNull(catalogueRevision);
        ArgumentNullException.ThrowIfNull(activeDocumentSetDigest);
        ArgumentNullException.ThrowIfNull(sourceBindingSetDigest);
        ArgumentNullException.ThrowIfNull(indexCompatibilityKey);
        ArgumentNullException.ThrowIfNull(generationSpecDigest);
        ArgumentNullException.ThrowIfNull(logicalArtifactDigest);
        ArgumentNullException.ThrowIfNull(generationContentDigest);
        ArgumentNullException.ThrowIfNull(indexGenerationId);

        if (chunkCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(chunkCount),
                chunkCount,
                "A finalised generation must contain at least one logical chunk.");
        }

        if (vectorCount != chunkCount)
        {
            throw new ArgumentException(
                "The MVP requires exactly one vector for every logical chunk.",
                nameof(vectorCount));
        }

        if (!string.Equals(
                indexGenerationId.Value,
                $"idxgen-{generationContentDigest.Value}",
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The final generation identity must derive from the complete manifest content digest.",
                nameof(indexGenerationId));
        }

        ManifestSchemaVersion = manifestSchemaVersion;
        CorpusId = corpusId;
        CorpusRevision = corpusRevision;
        CatalogueRevision = catalogueRevision;
        ActiveDocumentSetDigest = activeDocumentSetDigest;
        SourceBindingSetDigest = sourceBindingSetDigest;
        IndexCompatibilityKey = indexCompatibilityKey;
        GenerationSpecDigest = generationSpecDigest;
        ChunkCount = chunkCount;
        VectorCount = vectorCount;
        LogicalArtifactDigest = logicalArtifactDigest;
        GenerationContentDigest = generationContentDigest;
        IndexGenerationId = indexGenerationId;
    }

    public int ManifestSchemaVersion { get; }

    public CorpusId CorpusId { get; }

    public CorpusRevision CorpusRevision { get; }

    public CatalogueRevision CatalogueRevision { get; }

    public ActiveDocumentSetDigest ActiveDocumentSetDigest { get; }

    public SourceBindingSetDigest SourceBindingSetDigest { get; }

    public IndexCompatibilityKey IndexCompatibilityKey { get; }

    public GenerationSpecDigest GenerationSpecDigest { get; }

    public long ChunkCount { get; }

    public long VectorCount { get; }

    public LogicalArtifactDigest LogicalArtifactDigest { get; }

    public GenerationContentDigest GenerationContentDigest { get; }

    public IndexGenerationId IndexGenerationId { get; }
}

public sealed class IndexBuildRecord
{
    private IndexBuildRecord(
        CandidateBuildId candidateBuildId,
        IndexBuildStatus status,
        FinalisedIndexGenerationManifest? manifest)
    {
        ArgumentNullException.ThrowIfNull(candidateBuildId);

        if ((status == IndexBuildStatus.Validated) != (manifest is not null))
        {
            throw new ArgumentException(
                "Only a validated build can carry a finalised generation manifest.",
                nameof(manifest));
        }

        CandidateBuildId = candidateBuildId;
        Status = status;
        Manifest = manifest;
    }

    public CandidateBuildId CandidateBuildId { get; }

    public IndexBuildStatus Status { get; }

    public FinalisedIndexGenerationManifest? Manifest { get; }

    public bool IsQueryable =>
        Status == IndexBuildStatus.Validated && Manifest is not null;

    public static IndexBuildRecord CreateCandidate(CandidateBuildId candidateBuildId) =>
        new(candidateBuildId, IndexBuildStatus.Candidate, manifest: null);

    public IndexBuildRecord MarkValidated(FinalisedIndexGenerationManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        EnsureCandidate();
        return new IndexBuildRecord(CandidateBuildId, IndexBuildStatus.Validated, manifest);
    }

    public IndexBuildRecord MarkFailed()
    {
        EnsureCandidate();
        return new IndexBuildRecord(CandidateBuildId, IndexBuildStatus.Failed, manifest: null);
    }

    private void EnsureCandidate()
    {
        if (Status != IndexBuildStatus.Candidate)
        {
            throw new InvalidOperationException(
                "Only a candidate build can be finalised or marked as failed.");
        }
    }
}
