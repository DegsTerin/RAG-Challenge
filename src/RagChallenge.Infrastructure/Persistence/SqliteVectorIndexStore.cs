// Purpose: Implements the rebuildable exact-vector candidate store; it validates immutable builds but never decides or persists the active generation.
using System.Buffers.Binary;
using System.Linq.Expressions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.Infrastructure.Persistence;

public sealed class SqliteVectorIndexStore(SqliteStoreOptions options)
    : IVectorIndexStore
{
    public const string CosineNumericalSemantics =
        "cosine-f32mul-f64acc-boundary-canonical-v1";
    public const string CompatibilityDescriptor =
        "sqlite-exact-vector-store/2;schema=1;distance=cosine;algorithm=exact-scan;vector=float32;score=" +
        CosineNumericalSemantics;

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

        var existing = await context.VectorBuilds.SingleOrDefaultAsync(
            row => row.CandidateBuildId == candidateBuildId.Value,
            cancellationToken).ConfigureAwait(false);

        if (existing is not null)
        {
            if (!string.Equals(existing.CorpusId, corpusId.Value, StringComparison.Ordinal) ||
                !string.Equals(
                    existing.IndexCompatibilityKey,
                    indexCompatibilityKey.Value,
                    StringComparison.Ordinal) ||
                existing.VectorDimensions != vectorDimensions ||
                existing.ExpectedChunkCount != expectedChunkCount ||
                !string.Equals(
                    existing.CreatedAtUtc,
                    FormatUtc(createdAt),
                    StringComparison.Ordinal) ||
                string.Equals(existing.Status, "Failed", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "A vector candidate identity cannot be replayed with different immutable input.");
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return;
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

        if (string.Equals(build.Status, "Failed", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "A Failed vector build cannot accept chunks.");
        }

        var ordinals = chunks.Select(chunk => chunk.ChunkOrdinal).ToArray();
        var existingChunks = await context.VectorChunks
            .Where(row => row.CandidateBuildId == candidateBuildId.Value &&
                ordinals.Contains(row.ChunkOrdinal))
            .ToDictionaryAsync(row => row.ChunkOrdinal, cancellationToken)
            .ConfigureAwait(false);
        var newChunks = chunks
            .Where(chunk => !existingChunks.ContainsKey(chunk.ChunkOrdinal))
            .ToArray();

        foreach (var chunk in chunks)
        {
            ValidateChunk(chunk, build.VectorDimensions);

            if (existingChunks.TryGetValue(chunk.ChunkOrdinal, out var existingChunk) &&
                !MatchesChunk(existingChunk, chunk))
            {
                throw new InvalidOperationException(
                    "A vector chunk ordinal cannot be replayed with different content.");
            }
        }

        if (string.Equals(build.Status, "Validated", StringComparison.Ordinal))
        {
            if (newChunks.Length != 0)
            {
                throw new InvalidOperationException(
                    "A validated vector build cannot accept new chunks.");
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        var currentCount = await context.VectorChunks.LongCountAsync(
            row => row.CandidateBuildId == candidateBuildId.Value,
            cancellationToken).ConfigureAwait(false);

        if (currentCount + newChunks.Length > build.ExpectedChunkCount)
        {
            throw new InvalidOperationException(
                "A vector write would exceed the candidate manifest count.");
        }

        foreach (var chunk in newChunks)
        {
            context.VectorChunks.Add(new VectorChunkRow
            {
                CandidateBuildId = candidateBuildId.Value,
                ChunkOrdinal = chunk.ChunkOrdinal,
                DocumentId = chunk.DocumentId.Value,
                DocumentVersion = chunk.DocumentVersion.Value,
                ChunkDigest = chunk.ChunkDigest.Value,
                ChunkText = StoredVectorChunkCodec.Encode(chunk),
                Vector = EncodeVector(chunk.Vector.Span),
            });
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private static bool MatchesChunk(
        VectorChunkRow existing,
        VectorChunkWrite proposed)
    {
        var decoded = StoredVectorChunkCodec.Decode(existing.ChunkText);
        return string.Equals(existing.DocumentId, proposed.DocumentId.Value, StringComparison.Ordinal) &&
            existing.DocumentVersion == proposed.DocumentVersion.Value &&
            string.Equals(existing.ChunkDigest, proposed.ChunkDigest.Value, StringComparison.Ordinal) &&
            string.Equals(decoded.Text, proposed.ChunkText, StringComparison.Ordinal) &&
            decoded.ContentLanguage == proposed.ContentLanguage &&
            decoded.PageNumber == proposed.PageNumber &&
            decoded.RecordNumber == proposed.RecordNumber &&
            ColumnsEqual(decoded.Columns, proposed.Columns) &&
            existing.Vector.AsSpan().SequenceEqual(EncodeVector(proposed.Vector.Span));
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

        if (string.Equals(build.Status, "Validated", StringComparison.Ordinal))
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        if (!string.Equals(build.Status, "Candidate", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Only a Candidate vector build can become Failed.");
        }

        build.Status = "Failed";
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<VectorSearchResult> SearchExactAsync(
        VectorSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.MaximumResults > MaximumSearchResults)
        {
            return VectorSearchResult.Failed(VectorSearchOutcome.ContractViolation);
        }

        try
        {
            var eligibleSelectors = SelectEligibleSelectors(request);

            await using var context = options.CreateVectorContext();
            var build = await context.VectorBuilds.AsNoTracking().SingleOrDefaultAsync(
                row => row.IndexGenerationId == request.IndexGenerationId.Value &&
                    row.CorpusId == request.CorpusId.Value &&
                    row.Status == "Validated",
                cancellationToken).ConfigureAwait(false);

            if (build is null ||
                !string.Equals(
                    build.IndexCompatibilityKey,
                    request.ExpectedIndexCompatibilityKey.Value,
                    StringComparison.Ordinal))
            {
                return VectorSearchResult.Failed(
                    VectorSearchOutcome.GenerationUnavailable);
            }

            try
            {
                ValidateVector(
                    request.QueryVector.Span,
                    build.VectorDimensions,
                    nameof(request));
            }
            catch (ArgumentException)
            {
                return VectorSearchResult.Failed(VectorSearchOutcome.InvalidQueryVector);
            }

            if (!TryCalculateNorm(request.QueryVector.Span, out var queryNorm))
            {
                return VectorSearchResult.Failed(VectorSearchOutcome.InvalidQueryVector);
            }

            if (queryNorm == 0)
            {
                return VectorSearchResult.Failed(VectorSearchOutcome.InvalidQueryVector);
            }

            if (eligibleSelectors.Count == 0)
            {
                return VectorSearchResult.Successful([]);
            }

            var chunks = await context.VectorChunks.AsNoTracking()
                .Where(row => row.CandidateBuildId == build.CandidateBuildId)
                .Where(CreateEligibleChunkPredicate(eligibleSelectors.Keys))
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            var scored = new List<VectorSearchHit>();

            foreach (var row in chunks)
            {
                if (!eligibleSelectors.TryGetValue(
                        (row.DocumentId, row.DocumentVersion),
                        out var bindingSelector))
                {
                    throw new InvalidDataException(
                        "A retrieved vector chunk has no eligible binding selector.");
                }

                if (row.ChunkOrdinal < 0)
                {
                    throw new InvalidDataException(
                        "A retrieved vector chunk has a negative global ordinal.");
                }

                var decoded = StoredVectorChunkCodec.Decode(row.ChunkText);
                var vector = DecodeVector(row.Vector, build.VectorDimensions);
                ValidateStoredVector(vector);
                if (!TryCalculateNorm(vector, out var vectorNorm))
                {
                    throw new InvalidDataException(
                        "A retrieved vector chunk has a non-finite norm intermediate.");
                }

                double score;

                if (vectorNorm == 0)
                {
                    score = 0d;
                }
                else
                {
                    if (!TryCalculateDotProduct(
                            request.QueryVector.Span,
                            vector,
                            out var dotProduct))
                    {
                        throw new InvalidDataException(
                            "A retrieved vector chunk has a non-finite dot-product intermediate.");
                    }

                    var denominator = queryNorm * vectorNorm;
                    var rawScore = dotProduct / denominator;

                    if (!double.IsFinite(denominator) ||
                        !TryCanonicaliseCosineBoundary(rawScore, out score))
                    {
                        throw new InvalidDataException(
                            "A retrieved vector chunk produced a non-finite cosine intermediate.");
                    }
                }

                scored.Add(new VectorSearchHit(
                    new CandidateBuildId(row.CandidateBuildId),
                    request.CorpusId,
                    request.IndexGenerationId,
                    bindingSelector,
                    row.ChunkOrdinal,
                    new LogicalArtifactDigest(row.ChunkDigest),
                    decoded.Text,
                    score,
                    decoded.ContentLanguage,
                    decoded.PageNumber,
                    decoded.RecordNumber,
                    decoded.Columns));
            }

            return VectorSearchResult.Successful(scored
                .OrderByDescending(hit => hit.Score)
                .ThenBy(hit => hit.ChunkOrdinal)
                .Take(request.MaximumResults));
        }
        catch (OperationCanceledException)
        {
            return VectorSearchResult.Failed(VectorSearchOutcome.OperationCancelled);
        }
        catch (InvalidDataException)
        {
            return VectorSearchResult.Failed(VectorSearchOutcome.InvalidIndexData);
        }
        catch (ArgumentException)
        {
            return VectorSearchResult.Failed(VectorSearchOutcome.InvalidIndexData);
        }
        catch (Exception)
        {
            return VectorSearchResult.Failed(VectorSearchOutcome.UnexpectedFailure);
        }
    }

    private static Dictionary<(string DocumentId, long DocumentVersion),
        VectorSearchBindingSelector> SelectEligibleSelectors(VectorSearchRequest request)
    {
        var databaseFilters = request.DatabaseProductFilters
            .Select(identifier => identifier.Value)
            .ToHashSet(StringComparer.Ordinal);
        var documentFilters = request.DocumentFilters
            .Select(identifier => identifier.Value)
            .ToHashSet(StringComparer.Ordinal);
        var selectors = request.EligibleSelectors.Where(selector =>
            (databaseFilters.Count == 0 ||
                databaseFilters.Contains(selector.DatabaseProductId.Value)) &&
            (documentFilters.Count == 0 ||
                documentFilters.Contains(selector.DocumentId.Value)));
        return selectors.ToDictionary(
            selector => (
                selector.DocumentId.Value,
                selector.DocumentVersion.Value));
    }

    private static Expression<Func<VectorChunkRow, bool>> CreateEligibleChunkPredicate(
        IReadOnlyCollection<(string DocumentId, long DocumentVersion)> eligibleKeys)
    {
        var row = Expression.Parameter(typeof(VectorChunkRow), "row");
        Expression body = Expression.Constant(false);

        foreach (var key in eligibleKeys)
        {
            var documentMatch = Expression.Equal(
                Expression.Property(row, nameof(VectorChunkRow.DocumentId)),
                Expression.Constant(key.DocumentId));
            var versionMatch = Expression.Equal(
                Expression.Property(row, nameof(VectorChunkRow.DocumentVersion)),
                Expression.Constant(key.DocumentVersion));
            body = Expression.OrElse(
                body,
                Expression.AndAlso(documentMatch, versionMatch));
        }

        return Expression.Lambda<Func<VectorChunkRow, bool>>(body, row);
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

        if (StoredVectorChunkCodec.Encode(chunk).Length > 1_048_576)
        {
            throw new ArgumentException(
                "A stored chunk payload cannot exceed 1,048,576 characters.",
                nameof(chunk));
        }

        if (chunk.PageNumber is <= 0 || chunk.RecordNumber is <= 0 ||
            chunk.Columns?.Count > 64 ||
            chunk.Columns?.Any(column =>
                string.IsNullOrWhiteSpace(column.Key) ||
                column.Key.Length > 256 ||
                column.Value.Length > 4096) == true)
        {
            throw new ArgumentException(
                "Chunk citation metadata is outside the bounded representation.",
                nameof(chunk));
        }

        ValidateVector(chunk.Vector.Span, dimensions, nameof(chunk));
    }

    private static bool ColumnsEqual(
        IReadOnlyDictionary<string, string> existing,
        IReadOnlyDictionary<string, string>? proposed)
    {
        var proposedColumns = proposed ?? new Dictionary<string, string>();
        return existing.Count == proposedColumns.Count && existing.All(pair =>
            proposedColumns.TryGetValue(pair.Key, out var value) &&
            string.Equals(value, pair.Value, StringComparison.Ordinal));
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

    private static void ValidateStoredVector(ReadOnlySpan<float> vector)
    {
        foreach (var value in vector)
        {
            if (!float.IsFinite(value))
            {
                throw new InvalidDataException(
                    "A stored vector cannot contain NaN or infinity.");
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
        int dimensions)
    {
        var decoded = StoredVectorChunkCodec.Decode(row.ChunkText);
        return new LogicalIndexArtifact(
            row.ChunkOrdinal,
            new DocumentId(row.DocumentId),
            new DocumentVersionNumber(row.DocumentVersion),
            new LogicalArtifactDigest(row.ChunkDigest),
            decoded.Text,
            DecodeVector(row.Vector, dimensions));
    }

    // The float local fixes every product at binary32 before serial binary64 accumulation.
    private static bool TryCalculateNorm(
        ReadOnlySpan<float> vector,
        out double norm)
    {
        double sum = 0d;

        foreach (var value in vector)
        {
            float product = value * value;

            if (!float.IsFinite(product))
            {
                norm = default;
                return false;
            }

            sum += product;

            if (!double.IsFinite(sum))
            {
                norm = default;
                return false;
            }
        }

        norm = Math.Sqrt(sum);
        return double.IsFinite(norm);
    }

    private static bool TryCalculateDotProduct(
        ReadOnlySpan<float> left,
        ReadOnlySpan<float> right,
        out double dotProduct)
    {
        double sum = 0d;

        for (var index = 0; index < left.Length; index++)
        {
            float product = left[index] * right[index];

            if (!float.IsFinite(product))
            {
                dotProduct = default;
                return false;
            }

            sum += product;

            if (!double.IsFinite(sum))
            {
                dotProduct = default;
                return false;
            }
        }

        dotProduct = sum;
        return true;
    }

    internal static bool TryCanonicaliseCosineBoundary(
        double rawScore,
        out double score)
    {
        if (!double.IsFinite(rawScore))
        {
            score = default;
            return false;
        }

        score = rawScore > 1d
            ? 1d
            : rawScore < -1d
                ? -1d
                : rawScore;
        return true;
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
