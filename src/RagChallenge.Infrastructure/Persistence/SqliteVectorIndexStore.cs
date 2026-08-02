// Purpose: Implements the rebuildable exact-vector candidate store; it validates immutable builds but never decides or persists the active generation.
using System.Buffers.Binary;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.Infrastructure.Persistence;

public sealed class SqliteVectorIndexStore(SqliteStoreOptions options)
    : IVectorIndexStore
{
    private const int MaximumVectorDimensions = 8192;
    private const long MaximumCandidateChunks = 1_000_000;
    private const int MaximumBatchChunks = 1000;
    private const int MaximumSearchResults = 100;

    private readonly SqliteStoreOptions options =
        options ?? throw new ArgumentNullException(nameof(options));

    public async Task CreateCandidateAsync(
        CandidateBuildId candidateBuildId,
        CorpusId corpusId,
        IndexCompatibilityKey indexCompatibilityKey,
        int vectorDimensions,
        long expectedChunkCount,
        DateTimeOffset createdAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(candidateBuildId);
        ArgumentNullException.ThrowIfNull(corpusId);
        ArgumentNullException.ThrowIfNull(indexCompatibilityKey);
        EnsureUtc(createdAt, nameof(createdAt));

        if (vectorDimensions is <= 0 or > MaximumVectorDimensions)
        {
            throw new ArgumentOutOfRangeException(
                nameof(vectorDimensions),
                vectorDimensions,
                $"Vector dimensions must be 1..{MaximumVectorDimensions}.");
        }

        if (expectedChunkCount is <= 0 or > MaximumCandidateChunks)
        {
            throw new ArgumentOutOfRangeException(
                nameof(expectedChunkCount),
                expectedChunkCount,
                $"Candidate chunks must be 1..{MaximumCandidateChunks}.");
        }

        await using var context = options.CreateVectorContext();
        await using var transaction = await BeginImmediateAsync(
            context,
            cancellationToken).ConfigureAwait(false);

        if (await context.VectorBuilds.AnyAsync(
                row => row.CandidateBuildId == candidateBuildId.Value,
                cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException(
                "A vector candidate with this identity already exists.");
        }

        context.VectorBuilds.Add(new VectorBuildRow
        {
            CandidateBuildId = candidateBuildId.Value,
            CorpusId = corpusId.Value,
            Status = "Candidate",
            IndexGenerationId = null,
            IndexCompatibilityKey = indexCompatibilityKey.Value,
            VectorDimensions = vectorDimensions,
            ExpectedChunkCount = expectedChunkCount,
            CreatedAtUtc = FormatUtc(createdAt),
            ValidatedAtUtc = null,
        });

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task AddChunksAsync(
        CandidateBuildId candidateBuildId,
        IReadOnlyCollection<VectorChunkWrite> chunks,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(candidateBuildId);
        ArgumentNullException.ThrowIfNull(chunks);

        if (chunks.Count is <= 0 or > MaximumBatchChunks)
        {
            throw new ArgumentOutOfRangeException(
                nameof(chunks),
                chunks.Count,
                $"A vector write batch must contain 1..{MaximumBatchChunks} chunks.");
        }

        if (chunks.Select(chunk => chunk.ChunkOrdinal).Distinct().Count() != chunks.Count)
        {
            throw new ArgumentException(
                "A vector write batch cannot repeat a chunk ordinal.",
                nameof(chunks));
        }

        await using var context = options.CreateVectorContext();
        await using var transaction = await BeginImmediateAsync(
            context,
            cancellationToken).ConfigureAwait(false);
        var build = await context.VectorBuilds.SingleOrDefaultAsync(
            row => row.CandidateBuildId == candidateBuildId.Value,
            cancellationToken).ConfigureAwait(false) ??
            throw new KeyNotFoundException("The vector candidate does not exist.");

        if (!string.Equals(build.Status, "Candidate", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Only a Candidate vector build accepts chunks.");
        }

        var currentCount = await context.VectorChunks.LongCountAsync(
            row => row.CandidateBuildId == candidateBuildId.Value,
            cancellationToken).ConfigureAwait(false);

        if (currentCount + chunks.Count > build.ExpectedChunkCount)
        {
            throw new InvalidOperationException(
                "A vector write would exceed the candidate manifest count.");
        }

        foreach (var chunk in chunks)
        {
            ValidateChunk(chunk, build.VectorDimensions);
            context.VectorChunks.Add(new VectorChunkRow
            {
                CandidateBuildId = candidateBuildId.Value,
                ChunkOrdinal = chunk.ChunkOrdinal,
                DocumentId = chunk.DocumentId.Value,
                DocumentVersion = chunk.DocumentVersion.Value,
                ChunkDigest = chunk.ChunkDigest.Value,
                ChunkText = chunk.ChunkText,
                Vector = EncodeVector(chunk.Vector.Span),
            });
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<FinalisedIndexGenerationManifest> FinaliseCandidateAsync(
        CandidateBuildId candidateBuildId,
        IndexGenerationSpecification specification,
        DateTimeOffset validatedAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(candidateBuildId);
        ArgumentNullException.ThrowIfNull(specification);
        EnsureUtc(validatedAt, nameof(validatedAt));

        await using var context = options.CreateVectorContext();
        await using var transaction = await BeginImmediateAsync(
            context,
            cancellationToken).ConfigureAwait(false);
        var build = await context.VectorBuilds.SingleOrDefaultAsync(
            row => row.CandidateBuildId == candidateBuildId.Value,
            cancellationToken).ConfigureAwait(false) ??
            throw new KeyNotFoundException("The vector candidate does not exist.");

        if (!string.Equals(build.CorpusId, specification.CorpusId.Value, StringComparison.Ordinal) ||
            !string.Equals(
                build.IndexCompatibilityKey,
                specification.IndexCompatibilityKey.Value,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A generation specification must match the candidate corpus and compatibility key.",
                nameof(specification));
        }

        if (string.Equals(build.Status, "Failed", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "A Failed vector build cannot be finalised.");
        }

        var chunkRows = await context.VectorChunks.AsNoTracking()
            .Where(row => row.CandidateBuildId == candidateBuildId.Value)
            .OrderBy(row => row.ChunkOrdinal)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        if (chunkRows.LongLength != build.ExpectedChunkCount)
        {
            throw new InvalidOperationException(
                "A vector candidate cannot be finalised before its exact chunk count is durable.");
        }

        var artifacts = chunkRows
            .Select(row => ToLogicalArtifact(row, build.VectorDimensions))
            .ToArray();
        var manifest = IndexGenerationCanonicalizer.CreateFinalisedManifest(
            specification,
            artifacts);

        if (string.Equals(build.Status, "Validated", StringComparison.Ordinal))
        {
            if (!string.Equals(
                    build.IndexGenerationId,
                    manifest.IndexGenerationId.Value,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "A validated candidate cannot be finalised with different canonical content.");
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return manifest;
        }

        if (!string.Equals(build.Status, "Candidate", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Only a Candidate vector build can become Validated.");
        }

        build.Status = "Validated";
        build.IndexGenerationId = manifest.IndexGenerationId.Value;
        build.ValidatedAtUtc = FormatUtc(validatedAt);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var durableCount = await context.VectorChunks.LongCountAsync(
            row => row.CandidateBuildId == candidateBuildId.Value,
            cancellationToken).ConfigureAwait(false);

        if (durableCount != manifest.ChunkCount)
        {
            throw new InvalidOperationException(
                "Finalisation readback did not preserve the canonical chunk count.");
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return manifest;
    }

    public async Task MarkFailedAsync(
        CandidateBuildId candidateBuildId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(candidateBuildId);
        await using var context = options.CreateVectorContext();
        await using var transaction = await BeginImmediateAsync(
            context,
            cancellationToken).ConfigureAwait(false);
        var build = await context.VectorBuilds.SingleOrDefaultAsync(
            row => row.CandidateBuildId == candidateBuildId.Value,
            cancellationToken).ConfigureAwait(false) ??
            throw new KeyNotFoundException("The vector candidate does not exist.");

        if (!string.Equals(build.Status, "Candidate", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Only a Candidate vector build can become Failed.");
        }

        build.Status = "Failed";
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<VectorSearchHit>> SearchExactAsync(
        IndexGenerationId indexGenerationId,
        ReadOnlyMemory<float> queryVector,
        int maximumResults,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(indexGenerationId);

        if (maximumResults is <= 0 or > MaximumSearchResults)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumResults),
                maximumResults,
                $"Exact search must request 1..{MaximumSearchResults} results.");
        }

        await using var context = options.CreateVectorContext();
        var build = await context.VectorBuilds.AsNoTracking().SingleOrDefaultAsync(
            row => row.IndexGenerationId == indexGenerationId.Value &&
                row.Status == "Validated",
            cancellationToken).ConfigureAwait(false) ??
            throw new KeyNotFoundException(
                "The requested generation is not a validated vector build.");

        ValidateVector(queryVector.Span, build.VectorDimensions, nameof(queryVector));
        var queryNorm = CalculateNorm(queryVector.Span);

        if (queryNorm == 0)
        {
            throw new ArgumentException(
                "An exact-search vector cannot have zero magnitude.",
                nameof(queryVector));
        }

        var chunks = await context.VectorChunks.AsNoTracking()
            .Where(row => row.CandidateBuildId == build.CandidateBuildId)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var scored = new List<VectorSearchHit>(chunks.Length);

        foreach (var row in chunks)
        {
            var vector = DecodeVector(row.Vector, build.VectorDimensions);
            var vectorNorm = CalculateNorm(vector);
            var score = vectorNorm == 0
                ? 0
                : CalculateDotProduct(queryVector.Span, vector) /
                    (queryNorm * vectorNorm);
            scored.Add(new VectorSearchHit(
                new CandidateBuildId(row.CandidateBuildId),
                row.ChunkOrdinal,
                new DocumentId(row.DocumentId),
                new DocumentVersionNumber(row.DocumentVersion),
                new LogicalArtifactDigest(row.ChunkDigest),
                row.ChunkText,
                score));
        }

        return scored
            .OrderByDescending(hit => hit.Score)
            .ThenBy(hit => hit.ChunkOrdinal)
            .Take(maximumResults)
            .ToArray();
    }

    internal async Task<bool> MatchesFinalisedGenerationAsync(
        CandidateBuildId candidateBuildId,
        FinalisedIndexGenerationManifest manifest,
        CancellationToken cancellationToken)
    {
        await using var context = options.CreateVectorContext();
        var build = await context.VectorBuilds.AsNoTracking().SingleOrDefaultAsync(
            row => row.CandidateBuildId == candidateBuildId.Value &&
                row.Status == "Validated" &&
                row.IndexGenerationId == manifest.IndexGenerationId.Value &&
                row.ExpectedChunkCount == manifest.ChunkCount,
            cancellationToken).ConfigureAwait(false);

        if (build is null ||
            !string.Equals(
                build.CorpusId,
                manifest.CorpusId.Value,
                StringComparison.Ordinal) ||
            !string.Equals(
                build.IndexCompatibilityKey,
                manifest.IndexCompatibilityKey.Value,
                StringComparison.Ordinal))
        {
            return false;
        }

        var chunkRows = await context.VectorChunks.AsNoTracking()
            .Where(row => row.CandidateBuildId == candidateBuildId.Value)
            .OrderBy(row => row.ChunkOrdinal)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var artifacts = chunkRows
            .Select(row => ToLogicalArtifact(row, build.VectorDimensions))
            .ToArray();
        var specification = new IndexGenerationSpecification(
            manifest.ManifestSchemaVersion,
            manifest.CorpusId,
            manifest.CorpusRevision,
            manifest.CatalogueRevision,
            manifest.ActiveDocumentSetDigest,
            manifest.SourceBindingSetDigest,
            manifest.IndexCompatibilityKey);
        return IndexGenerationCanonicalizer.Matches(
            manifest,
            specification,
            artifacts);
    }

    internal async Task<bool> DeleteGenerationIfPresentAsync(
        IndexGenerationId generationId,
        CancellationToken cancellationToken)
    {
        await using var context = options.CreateVectorContext();
        await using var transaction = await BeginImmediateAsync(
            context,
            cancellationToken).ConfigureAwait(false);
        var build = await context.VectorBuilds.SingleOrDefaultAsync(
            row => row.IndexGenerationId == generationId.Value,
            cancellationToken).ConfigureAwait(false);

        if (build is null)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return false;
        }

        var chunks = context.VectorChunks.Where(
            row => row.CandidateBuildId == build.CandidateBuildId);
        context.VectorChunks.RemoveRange(chunks);
        context.VectorBuilds.Remove(build);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private static void ValidateChunk(VectorChunkWrite chunk, int dimensions)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        ArgumentNullException.ThrowIfNull(chunk.DocumentId);
        ArgumentNullException.ThrowIfNull(chunk.DocumentVersion);
        ArgumentNullException.ThrowIfNull(chunk.ChunkDigest);
        ArgumentException.ThrowIfNullOrWhiteSpace(chunk.ChunkText);

        if (chunk.ChunkOrdinal < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(chunk),
                chunk.ChunkOrdinal,
                "A chunk ordinal cannot be negative.");
        }

        if (chunk.ChunkText.Length > 1_048_576)
        {
            throw new ArgumentException(
                "A chunk text cannot exceed 1,048,576 characters.",
                nameof(chunk));
        }

        ValidateVector(chunk.Vector.Span, dimensions, nameof(chunk));
    }

    private static void ValidateVector(
        ReadOnlySpan<float> vector,
        int dimensions,
        string parameterName)
    {
        if (vector.Length != dimensions)
        {
            throw new ArgumentException(
                "A vector must match the candidate's fixed dimensions.",
                parameterName);
        }

        foreach (var value in vector)
        {
            if (!float.IsFinite(value))
            {
                throw new ArgumentException(
                    "A vector cannot contain NaN or infinity.",
                    parameterName);
            }
        }
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

    private static float[] DecodeVector(byte[] bytes, int dimensions)
    {
        if (bytes.Length != dimensions * sizeof(float))
        {
            throw new InvalidDataException(
                "A stored vector does not match its build dimensions.");
        }

        var vector = new float[dimensions];

        for (var index = 0; index < dimensions; index++)
        {
            vector[index] = BinaryPrimitives.ReadSingleLittleEndian(
                bytes.AsSpan(index * sizeof(float), sizeof(float)));
        }

        return vector;
    }

    private static LogicalIndexArtifact ToLogicalArtifact(
        VectorChunkRow row,
        int dimensions) =>
        new(
            row.ChunkOrdinal,
            new DocumentId(row.DocumentId),
            new DocumentVersionNumber(row.DocumentVersion),
            new LogicalArtifactDigest(row.ChunkDigest),
            row.ChunkText,
            DecodeVector(row.Vector, dimensions));

    private static double CalculateNorm(ReadOnlySpan<float> vector)
    {
        double sum = 0;

        foreach (var value in vector)
        {
            sum += value * value;
        }

        return Math.Sqrt(sum);
    }

    private static double CalculateDotProduct(
        ReadOnlySpan<float> left,
        ReadOnlySpan<float> right)
    {
        double sum = 0;

        for (var index = 0; index < left.Length; index++)
        {
            sum += left[index] * right[index];
        }

        return sum;
    }

    private static async Task<SqliteTransaction> BeginImmediateAsync(
        VectorStoreDbContext context,
        CancellationToken cancellationToken)
    {
        await context.Database
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        var connection = (SqliteConnection)context.Database.GetDbConnection();
        var transaction = connection.BeginTransaction(deferred: false);
        context.Database.UseTransaction(transaction);
        return transaction;
    }

    private static string FormatUtc(DateTimeOffset value) =>
        value.ToString("O", System.Globalization.CultureInfo.InvariantCulture);

    private static void EnsureUtc(DateTimeOffset value, string parameterName)
    {
        if (value.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Persistence instants must be expressed in UTC.",
                parameterName);
        }
    }
}
