// Purpose: Canonicalises generation specifications, logical index artefacts, and complete manifests in Domain; persistence and provider details remain outside this module.
using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Domain.IndexingRetrieval;

public sealed class IndexGenerationSpecification
{
    public IndexGenerationSpecification(
        int manifestSchemaVersion,
        CorpusId corpusId,
        CorpusRevision corpusRevision,
        CatalogueRevision catalogueRevision,
        ActiveDocumentSetDigest activeDocumentSetDigest,
        SourceBindingSetDigest sourceBindingSetDigest,
        IndexCompatibilityKey indexCompatibilityKey)
    {
        if (manifestSchemaVersion <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(manifestSchemaVersion),
                manifestSchemaVersion,
                "A manifest schema version must be positive.");
        }

        ManifestSchemaVersion = manifestSchemaVersion;
        CorpusId = corpusId ?? throw new ArgumentNullException(nameof(corpusId));
        CorpusRevision = corpusRevision ?? throw new ArgumentNullException(nameof(corpusRevision));
        CatalogueRevision = catalogueRevision ??
            throw new ArgumentNullException(nameof(catalogueRevision));
        ActiveDocumentSetDigest = activeDocumentSetDigest ??
            throw new ArgumentNullException(nameof(activeDocumentSetDigest));
        SourceBindingSetDigest = sourceBindingSetDigest ??
            throw new ArgumentNullException(nameof(sourceBindingSetDigest));
        IndexCompatibilityKey = indexCompatibilityKey ??
            throw new ArgumentNullException(nameof(indexCompatibilityKey));
    }

    public int ManifestSchemaVersion { get; }

    public CorpusId CorpusId { get; }

    public CorpusRevision CorpusRevision { get; }

    public CatalogueRevision CatalogueRevision { get; }

    public ActiveDocumentSetDigest ActiveDocumentSetDigest { get; }

    public SourceBindingSetDigest SourceBindingSetDigest { get; }

    public IndexCompatibilityKey IndexCompatibilityKey { get; }
}

public sealed class LogicalIndexArtifact
{
    public LogicalIndexArtifact(
        long chunkOrdinal,
        DocumentId documentId,
        DocumentVersionNumber documentVersion,
        LogicalArtifactDigest chunkDigest,
        string chunkText,
        ReadOnlyMemory<float> vector)
    {
        if (chunkOrdinal < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(chunkOrdinal),
                chunkOrdinal,
                "A logical chunk ordinal cannot be negative.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(chunkText);

        if (vector.IsEmpty || ContainsNonFinite(vector.Span))
        {
            throw new ArgumentException(
                "A logical vector must be non-empty and contain only finite values.",
                nameof(vector));
        }

        ChunkOrdinal = chunkOrdinal;
        DocumentId = documentId ?? throw new ArgumentNullException(nameof(documentId));
        DocumentVersion = documentVersion ??
            throw new ArgumentNullException(nameof(documentVersion));
        ChunkDigest = chunkDigest ?? throw new ArgumentNullException(nameof(chunkDigest));
        ChunkText = chunkText;
        Vector = vector.ToArray();
    }

    public long ChunkOrdinal { get; }

    public DocumentId DocumentId { get; }

    public DocumentVersionNumber DocumentVersion { get; }

    public LogicalArtifactDigest ChunkDigest { get; }

    public string ChunkText { get; }

    public ReadOnlyMemory<float> Vector { get; }

    private static bool ContainsNonFinite(ReadOnlySpan<float> vector)
    {
        foreach (var value in vector)
        {
            if (!float.IsFinite(value))
            {
                return true;
            }
        }

        return false;
    }
}

public static class IndexGenerationCanonicalizer
{
    public const string GenerationSpecificationDomain =
        "rag-challenge/index-generation-specification/v1";

    public const string LogicalArtifactDomain =
        "rag-challenge/logical-index-artifacts/v1";

    public const string GenerationManifestDomain =
        "rag-challenge/index-generation-manifest/v1";

    public static GenerationSpecDigest ComputeGenerationSpecDigest(
        IndexGenerationSpecification specification)
    {
        ArgumentNullException.ThrowIfNull(specification);
        using var writer = new CanonicalHashWriter();
        writer.Append(GenerationSpecificationDomain);
        AppendSpecification(writer, specification);
        return new GenerationSpecDigest(writer.Finish());
    }

    public static LogicalArtifactDigest ComputeLogicalArtifactDigest(
        IEnumerable<LogicalIndexArtifact> artifacts)
    {
        ArgumentNullException.ThrowIfNull(artifacts);
        var ordered = artifacts.OrderBy(artifact => artifact.ChunkOrdinal).ToArray();

        if (ordered.Length == 0)
        {
            throw new ArgumentException(
                "A finalised generation requires at least one logical artefact.",
                nameof(artifacts));
        }

        if (ordered.Select(artifact => artifact.ChunkOrdinal).Distinct().Count() !=
            ordered.Length)
        {
            throw new ArgumentException(
                "Logical artefacts cannot repeat a chunk ordinal.",
                nameof(artifacts));
        }

        using var writer = new CanonicalHashWriter();
        writer.Append(LogicalArtifactDomain);
        writer.Append(ordered.Length.ToString(CultureInfo.InvariantCulture));

        foreach (var artifact in ordered)
        {
            writer.Append(artifact.ChunkOrdinal.ToString(CultureInfo.InvariantCulture));
            writer.Append(artifact.DocumentId.Value);
            writer.Append(artifact.DocumentVersion.ToCanonicalString());
            writer.Append(artifact.ChunkDigest.Value);
            writer.Append(artifact.ChunkText);
            writer.Append(EncodeVector(artifact.Vector.Span));
        }

        return new LogicalArtifactDigest(writer.Finish());
    }

    public static FinalisedIndexGenerationManifest CreateFinalisedManifest(
        IndexGenerationSpecification specification,
        IEnumerable<LogicalIndexArtifact> artifacts)
    {
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(artifacts);
        var materialised = artifacts.ToArray();
        var specificationDigest = ComputeGenerationSpecDigest(specification);
        var logicalDigest = ComputeLogicalArtifactDigest(materialised);
        var contentDigest = ComputeGenerationContentDigest(
            specification,
            specificationDigest,
            materialised.LongLength,
            materialised.LongLength,
            logicalDigest);
        return new FinalisedIndexGenerationManifest(
            specification.ManifestSchemaVersion,
            specification.CorpusId,
            specification.CorpusRevision,
            specification.CatalogueRevision,
            specification.ActiveDocumentSetDigest,
            specification.SourceBindingSetDigest,
            specification.IndexCompatibilityKey,
            specificationDigest,
            materialised.LongLength,
            materialised.LongLength,
            logicalDigest,
            contentDigest,
            new IndexGenerationId($"idxgen-{contentDigest.Value}"));
    }

    public static bool Matches(
        FinalisedIndexGenerationManifest manifest,
        IndexGenerationSpecification specification,
        IEnumerable<LogicalIndexArtifact> artifacts)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        var expected = CreateFinalisedManifest(specification, artifacts);
        return manifest.ManifestSchemaVersion == expected.ManifestSchemaVersion &&
            manifest.CorpusId == expected.CorpusId &&
            manifest.CorpusRevision == expected.CorpusRevision &&
            manifest.CatalogueRevision == expected.CatalogueRevision &&
            manifest.ActiveDocumentSetDigest == expected.ActiveDocumentSetDigest &&
            manifest.SourceBindingSetDigest == expected.SourceBindingSetDigest &&
            manifest.IndexCompatibilityKey == expected.IndexCompatibilityKey &&
            manifest.GenerationSpecDigest == expected.GenerationSpecDigest &&
            manifest.ChunkCount == expected.ChunkCount &&
            manifest.VectorCount == expected.VectorCount &&
            manifest.LogicalArtifactDigest == expected.LogicalArtifactDigest &&
            manifest.GenerationContentDigest == expected.GenerationContentDigest &&
            manifest.IndexGenerationId == expected.IndexGenerationId;
    }

    private static GenerationContentDigest ComputeGenerationContentDigest(
        IndexGenerationSpecification specification,
        GenerationSpecDigest specificationDigest,
        long chunkCount,
        long vectorCount,
        LogicalArtifactDigest logicalArtifactDigest)
    {
        using var writer = new CanonicalHashWriter();
        writer.Append(GenerationManifestDomain);
        AppendSpecification(writer, specification);
        writer.Append(specificationDigest.Value);
        writer.Append(chunkCount.ToString(CultureInfo.InvariantCulture));
        writer.Append(vectorCount.ToString(CultureInfo.InvariantCulture));
        writer.Append(logicalArtifactDigest.Value);
        return new GenerationContentDigest(writer.Finish());
    }

    private static void AppendSpecification(
        CanonicalHashWriter writer,
        IndexGenerationSpecification specification)
    {
        writer.Append(specification.ManifestSchemaVersion.ToString(CultureInfo.InvariantCulture));
        writer.Append(specification.CorpusId.Value);
        writer.Append(specification.CorpusRevision.ToCanonicalString());
        writer.Append(specification.CatalogueRevision.ToCanonicalString());
        writer.Append(specification.ActiveDocumentSetDigest.Value);
        writer.Append(specification.SourceBindingSetDigest.Value);
        writer.Append(specification.IndexCompatibilityKey.Value);
    }

    private static byte[] EncodeVector(ReadOnlySpan<float> vector)
    {
        var bytes = new byte[checked(vector.Length * sizeof(float))];

        for (var index = 0; index < vector.Length; index++)
        {
            BinaryPrimitives.WriteSingleLittleEndian(
                bytes.AsSpan(index * sizeof(float), sizeof(float)),
                vector[index]);
        }

        return bytes;
    }

    private sealed class CanonicalHashWriter : IDisposable
    {
        private readonly IncrementalHash hash =
            IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        internal void Append(string value) => Append(Encoding.UTF8.GetBytes(value));

        internal void Append(ReadOnlySpan<byte> value)
        {
            var prefix = Encoding.ASCII.GetBytes(
                $"{value.Length.ToString(CultureInfo.InvariantCulture)}:");
            hash.AppendData(prefix);
            hash.AppendData(value);
        }

        internal string Finish() =>
            Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();

        public void Dispose() => hash.Dispose();
    }

}
