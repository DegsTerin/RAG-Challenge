// Purpose: Maps vectors.db as a rebuildable candidate-and-chunk store and deliberately contains no active-generation authority.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RagChallenge.Infrastructure.Persistence;

public sealed class VectorStoreDbContext(DbContextOptions<VectorStoreDbContext> options)
    : DbContext(options)
{
    internal DbSet<VectorBuildRow> VectorBuilds => Set<VectorBuildRow>();

    internal DbSet<VectorChunkRow> VectorChunks => Set<VectorChunkRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VectorBuildRow>(entity =>
        {
            entity.ToTable("vector_builds", table =>
            {
                table.HasCheckConstraint("ck_vector_builds_candidate_id", StableId("candidate_build_id"));
                table.HasCheckConstraint("ck_vector_builds_corpus_id", StableId("corpus_id"));
                table.HasCheckConstraint("ck_vector_builds_status", "status IN ('Candidate', 'Validated', 'Failed')");
                table.HasCheckConstraint(
                    "ck_vector_builds_generation_state",
                    "(status = 'Validated' AND index_generation_id IS NOT NULL AND validated_at_utc IS NOT NULL) OR " +
                    "(status <> 'Validated' AND index_generation_id IS NULL AND validated_at_utc IS NULL)");
                table.HasCheckConstraint(
                    "ck_vector_builds_generation_id",
                    "index_generation_id IS NULL OR " + GenerationId("index_generation_id"));
                table.HasCheckConstraint("ck_vector_builds_compatibility", Sha256("index_compatibility_key"));
                table.HasCheckConstraint("ck_vector_builds_dimensions", "vector_dimensions BETWEEN 1 AND 65536");
                table.HasCheckConstraint("ck_vector_builds_chunk_count", "expected_chunk_count > 0");
                table.HasCheckConstraint("ck_vector_builds_created_utc", UtcInstant("created_at_utc"));
                table.HasCheckConstraint("ck_vector_builds_validated_utc", "validated_at_utc IS NULL OR " + UtcInstant("validated_at_utc"));
            });
            entity.HasKey(row => row.CandidateBuildId);
            entity.HasIndex(row => row.IndexGenerationId).IsUnique();
        });

        modelBuilder.Entity<VectorChunkRow>(entity =>
        {
            entity.ToTable("vector_chunks", table =>
            {
                table.HasCheckConstraint("ck_vector_chunks_ordinal", "chunk_ordinal >= 0");
                table.HasCheckConstraint("ck_vector_chunks_document_version", "document_version > 0");
                table.HasCheckConstraint("ck_vector_chunks_digest", Sha256("chunk_digest"));
                table.HasCheckConstraint("ck_vector_chunks_text", "length(chunk_text) BETWEEN 1 AND 1048576");
                table.HasCheckConstraint("ck_vector_chunks_vector", "length(vector) > 0 AND length(vector) % 4 = 0");
            });
            entity.HasKey(row => new { row.CandidateBuildId, row.ChunkOrdinal });
            entity.HasIndex(row => new
            {
                row.CandidateBuildId,
                row.DocumentId,
                row.DocumentVersion,
                row.ChunkDigest,
            }).IsUnique();
            entity.HasOne<VectorBuildRow>().WithMany().HasForeignKey(row => row.CandidateBuildId).OnDelete(DeleteBehavior.Restrict);
        });

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));

                if (property.ClrType == typeof(string))
                {
                    property.SetCollation("BINARY");
                    property.SetMaxLength(property.GetMaxLength() ?? 1048576);
                }
            }
        }
    }

    private static string ToSnakeCase(string value) =>
        string.Concat(value.Select((character, index) =>
            char.IsUpper(character) && index > 0
                ? $"_{char.ToLowerInvariant(character)}"
                : char.ToLowerInvariant(character).ToString()));

    private static string StableId(string column) =>
        $"length({column}) BETWEEN 1 AND 128 AND " +
        $"{column} GLOB '[A-Za-z0-9]*' AND " +
        $"{column} NOT GLOB '*[^A-Za-z0-9._:-]*'";

    private static string Sha256(string column) =>
        $"length({column}) = 64 AND {column} NOT GLOB '*[^0-9a-f]*'";

    private static string GenerationId(string column) =>
        $"length({column}) = 71 AND substr({column}, 1, 7) = 'idxgen-' AND " +
        $"substr({column}, 8) NOT GLOB '*[^0-9a-f]*'";

    private static string UtcInstant(string column) =>
        $"length({column}) = 33 AND substr({column}, -6) = '+00:00'";
}

public sealed class VectorStoreDesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<VectorStoreDbContext>
{
    public VectorStoreDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<VectorStoreDbContext>()
            .Configure(DesignTimeStorePath.Resolve("vectors.db"))
            .Options;

        return new VectorStoreDbContext(options);
    }
}
