// Purpose: Verifies the physical SQLite authority split, immutable content publication, constraints, and exact derived-vector behaviour in temporary stores.
using System.Globalization;
using System.Text;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;
using RagChallenge.Infrastructure.Persistence;

namespace RagChallenge.IntegrationTests;

public sealed class SqlitePersistenceBoundaryTests
{
    [Fact]
    public async Task MigrationsCreateValidStoresWithAuthorityOnlyInControlDatabase()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();

        Assert.Equal("ok", await ReadPragmaAsync(
            fixture.Options.ControlDatabasePath,
            "PRAGMA integrity_check;"));
        Assert.Equal("ok", await ReadPragmaAsync(
            fixture.Options.VectorDatabasePath,
            "PRAGMA integrity_check;"));
        Assert.Equal(0, await CountRowsAsync(
            fixture.Options.ControlDatabasePath,
            "PRAGMA foreign_key_check;"));
        Assert.Equal(0, await CountRowsAsync(
            fixture.Options.VectorDatabasePath,
            "PRAGMA foreign_key_check;"));

        var prohibitedAuthorityTables = await ScalarAsync(
            fixture.Options.VectorDatabasePath,
            """
            SELECT COUNT(*)
            FROM sqlite_master
            WHERE type = 'table'
              AND name IN (
                'corpora',
                'catalogue_heads',
                'generation_manifests',
                'activation_heads',
                'activation_records',
                'generation_retention',
                'admin_operations',
                'audit_events');
            """);
        Assert.Equal(0, prohibitedAuthorityTables);
        Assert.Equal(2, await ScalarAsync(
            fixture.Options.VectorDatabasePath,
            """
            SELECT COUNT(*)
            FROM sqlite_master
            WHERE type = 'table'
              AND name IN ('vector_builds', 'vector_chunks');
            """));
        Assert.True(await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            """
            SELECT COUNT(*)
            FROM sqlite_master
            WHERE type = 'table'
              AND name IN (
                'catalogue_heads',
                'generation_manifests',
                'activation_heads',
                'generation_retention',
                'audit_events');
            """) >= 5);
    }

    [Fact]
    public async Task AdministrationMigrationsBackfillExistingCatalogueProjectionWithoutDataLoss()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "rag-challenge-migration-upgrade-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var options = new SqliteStoreOptions(
            Path.Combine(root, "control.db"),
            Path.Combine(root, "vectors.db"),
            Path.Combine(root, "content"));

        try
        {
            await using (var context = options.CreateControlContext())
            {
                var migrator = context.Database.GetService<IMigrator>();
                await migrator.MigrateAsync("20260802171743_InitialControlPlane");
            }

            await ExecuteAsync(
                options.ControlDatabasePath,
                """
                INSERT INTO corpora
                    (corpus_id, corpus_revision, created_at_utc)
                VALUES
                    ('upgrade-corpus', 1, '2026-08-04T12:00:00.0000000+00:00');

                INSERT INTO admin_operations
                    (operation_id, corpus_id, operation_kind, status,
                     expected_revision, result_revision, requested_at_utc,
                     completed_at_utc)
                VALUES
                    ('upgrade-catalogue', 'upgrade-corpus', 'CatalogueCommit',
                     'Applied', 0, 1, '2026-08-04T12:00:00.0000000+00:00',
                     '2026-08-04T12:00:00.0000000+00:00');

                INSERT INTO content_objects
                    (content_sha256, byte_length, registered_at_utc)
                VALUES
                    ('0000000000000000000000000000000000000000000000000000000000000000',
                     1, '2026-08-04T12:00:00.0000000+00:00');

                INSERT INTO database_categories
                    (corpus_id, category_id, display_name)
                VALUES
                    ('upgrade-corpus', 'category-upgrade', 'Upgrade category');

                INSERT INTO database_product_revisions
                    (corpus_id, product_id, product_revision, display_name, status)
                VALUES
                    ('upgrade-corpus', 'database-upgrade', 1, 'Upgrade database',
                     'Active');

                INSERT INTO database_product_categories
                    (corpus_id, product_id, product_revision, category_id)
                VALUES
                    ('upgrade-corpus', 'database-upgrade', 1, 'category-upgrade');

                INSERT INTO document_versions
                    (corpus_id, document_id, document_version, product_id,
                     product_revision, document_format, content_language,
                     content_sha256, byte_length, media_type, source_adapter_id,
                     source_trust_class, official_registration_id,
                     official_snapshot_id)
                VALUES
                    ('upgrade-corpus', 'document-upgrade', 1, 'database-upgrade',
                     1, 'Pdf', 'en-GB',
                     '0000000000000000000000000000000000000000000000000000000000000000',
                     1, 'application/pdf', 'local-upgrade', 'LocalAuthorised',
                     NULL, NULL);

                INSERT INTO catalogue_revisions
                    (corpus_id, catalogue_revision, created_at_utc, operation_id)
                VALUES
                    ('upgrade-corpus', 1,
                     '2026-08-04T12:00:00.0000000+00:00', 'upgrade-catalogue');

                INSERT INTO catalogue_revision_products
                    (corpus_id, catalogue_revision, product_id, product_revision)
                VALUES
                    ('upgrade-corpus', 1, 'database-upgrade', 1);

                INSERT INTO catalogue_revision_documents
                    (corpus_id, catalogue_revision, document_id,
                     document_version, status)
                VALUES
                    ('upgrade-corpus', 1, 'document-upgrade', 1, 'Active');

                INSERT INTO catalogue_heads
                    (corpus_id, catalogue_revision, row_revision)
                VALUES
                    ('upgrade-corpus', 1, 1);
                """);

            await using (var context = options.CreateControlContext())
            {
                var migrator = context.Database.GetService<IMigrator>();
                await migrator.MigrateAsync();
            }

            var store = new SqliteControlPlaneStore(options);
            var snapshot = Assert.IsType<CatalogueSnapshot>(
                await store.ReadCurrentCatalogueAsync(new CorpusId("upgrade-corpus")));
            var product = Assert.Single(snapshot.DatabaseProducts);
            var document = Assert.Single(snapshot.DocumentVersions);

            Assert.Equal(CatalogueItemStatus.Active, product.Status);
            Assert.Equal("database-upgrade", document.DatabaseProductId.Value);
            Assert.Equal(1, document.DatabaseProductRevision.Value);
            Assert.Equal(2, await ScalarAsync(
                options.ControlDatabasePath,
                """
                SELECT COUNT(*)
                FROM sqlite_master
                WHERE type = 'table'
                  AND name IN (
                    'administration_leases',
                    'administration_command_journal');
                """));
        }
        finally
        {
            SqliteConnection.ClearAllPools();

            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public async Task ImmutableContentUsesSha256IdentityAndRejectsMismatches()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var bytes = Encoding.UTF8.GetBytes("synthetic immutable content");
        await using var firstStream = new MemoryStream(bytes, writable: false);
        var first = await fixture.ContentStore.PutAsync(firstStream, bytes.Length);
        Assert.False(first.AlreadyExisted);
        Assert.Equal(SqlitePersistenceFixture.Hash("synthetic immutable content"), first.ContentObjectId.Value);

        await using var repeatedStream = new MemoryStream(bytes, writable: false);
        var repeated = await fixture.ContentStore.PutAsync(
            repeatedStream,
            bytes.Length,
            first.ContentObjectId);
        Assert.True(repeated.AlreadyExisted);

        await using (var reopened = await fixture.ContentStore.OpenReadAsync(first.ContentObjectId))
        using (var reader = new StreamReader(reopened, Encoding.UTF8))
        {
            Assert.Equal("synthetic immutable content", await reader.ReadToEndAsync());
        }

        await using var mismatchStream = new MemoryStream(bytes, writable: false);
        await Assert.ThrowsAsync<InvalidDataException>(
            () => fixture.ContentStore.PutAsync(
                mismatchStream,
                bytes.Length,
                new ContentObjectId(new string('0', 64))));
        Assert.Empty(Directory.EnumerateFiles(
            Path.Combine(fixture.Options.ContentStoreRoot, "quarantine")));

        var objectPath = Path.Combine(
            fixture.Options.ContentStoreRoot,
            "objects",
            first.ContentObjectId.Value[..2],
            $"{first.ContentObjectId.Value}.bin");
        await File.WriteAllTextAsync(objectPath, "corrupted content");
        await Assert.ThrowsAsync<InvalidDataException>(async () =>
        {
            await using var _ = await fixture.ContentStore.OpenReadAsync(first.ContentObjectId);
        });
    }

    [Fact]
    public async Task CataloguePersistenceRejectsAnUnrecoverableUnusedCategory()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var snapshot = new CatalogueSnapshot(
            SqlitePersistenceFixture.CorpusId,
            new CatalogueRevision(1),
            [
                new DatabaseCategory(
                    new DatabaseCategoryId("category-assigned"),
                    "Assigned category"),
                new DatabaseCategory(
                    new DatabaseCategoryId("category-unused"),
                    "Unused category"),
            ],
            [new DatabaseProduct(
                new DatabaseProductId("database"),
                new DatabaseProductRevision(1),
                "Database",
                CatalogueItemStatus.Candidate,
                [new DatabaseCategoryId("category-assigned")])],
            []);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            fixture.ControlStore.CommitCatalogueAsync(new CatalogueCommitRequest(
                new OperationId("reject-unused-category"),
                snapshot,
                0,
                SqlitePersistenceFixture.At(1))));
        Assert.Equal(
            0,
            await fixture.ScalarAsync("SELECT COUNT(*) FROM catalogue_revisions;"));
    }

    [Fact]
    public void StorePathsMustBeDistinctAndCannotUseFilesystemRootForContent()
    {
        var root = Path.GetPathRoot(Path.GetTempPath())!;
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            "rag-challenge-path-test",
            "store.db");

        Assert.Throws<ArgumentException>(() => new SqliteStoreOptions(
            databasePath,
            databasePath,
            Path.Combine(Path.GetTempPath(), "rag-challenge-content")));
        Assert.Throws<ArgumentException>(() => new SqliteStoreOptions(
            databasePath,
            Path.Combine(Path.GetDirectoryName(databasePath)!, "vectors.db"),
            root));
    }

    [Fact]
    public async Task ValidatedVectorsAreQueryableAndCandidatesRemainNonQueryable()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (_, binding) = await fixture.CommitLocalCatalogueAsync();
        var candidate = new CandidateBuildId("candidate-unvalidated");
        await fixture.VectorStore.CreateCandidateAsync(
            candidate,
            SqlitePersistenceFixture.CorpusId,
            SqlitePersistenceFixture.CompatibilityKey,
            vectorDimensions: 3,
            expectedChunkCount: 1,
            SqlitePersistenceFixture.At(2));
        await fixture.VectorStore.AddChunksAsync(
            candidate,
            [new VectorChunkWrite(
                0,
                binding.DocumentId,
                binding.DocumentVersion,
                new LogicalArtifactDigest(SqlitePersistenceFixture.Hash("candidate-chunk")),
                "candidate chunk",
                new float[] { 1, 0, 0 })]);
        var unvalidatedId = new IndexGenerationId(
            $"idxgen-{SqlitePersistenceFixture.Hash("unvalidated")}");
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            fixture.VectorStore.SearchExactAsync(
                new VectorSearchRequest(
                    SqlitePersistenceFixture.CorpusId,
                    unvalidatedId,
                    new float[] { 1, 0, 0 },
                    maximumResults: 1,
                    [VectorSearchBindingSelector.FromBinding(binding)])));

        var activeDigest = BindingDigestCanonicalizer
            .CanonicaliseActiveDocumentSet([binding])
            .Digest;
        var sourceDigest = BindingDigestCanonicalizer
            .CanonicaliseSourceBindingSet([binding])
            .Digest;
        var specification = new IndexGenerationSpecification(
            1,
            SqlitePersistenceFixture.CorpusId,
            new CorpusRevision(1),
            new CatalogueRevision(1),
            activeDigest,
            sourceDigest,
            SqlitePersistenceFixture.CompatibilityKey);
        var manifest = await fixture.VectorStore.FinaliseCandidateAsync(
            candidate,
            specification,
            SqlitePersistenceFixture.At(2));
        var idempotentReplay = await fixture.VectorStore.FinaliseCandidateAsync(
            candidate,
            specification,
            SqlitePersistenceFixture.At(3));
        var hits = await fixture.VectorStore.SearchExactAsync(
            new VectorSearchRequest(
                SqlitePersistenceFixture.CorpusId,
                manifest.IndexGenerationId,
                new float[] { 1, 0, 0 },
                maximumResults: 1,
                [VectorSearchBindingSelector.FromBinding(binding)]));
        var hit = Assert.Single(hits);
        Assert.Equal(manifest.IndexGenerationId, idempotentReplay.IndexGenerationId);
        Assert.Equal(0, hit.ChunkOrdinal);
        Assert.Equal(1, hit.Score, precision: 12);
    }

    [Fact]
    public async Task GenerationCommitRejectsVectorPayloadChangedAfterFinalisation()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (_, binding) = await fixture.CommitLocalCatalogueAsync();
        var candidate = new CandidateBuildId("candidate-corruption");
        await fixture.VectorStore.CreateCandidateAsync(
            candidate,
            SqlitePersistenceFixture.CorpusId,
            SqlitePersistenceFixture.CompatibilityKey,
            vectorDimensions: 3,
            expectedChunkCount: 1,
            SqlitePersistenceFixture.At(2));
        await fixture.VectorStore.AddChunksAsync(
            candidate,
            [new VectorChunkWrite(
                0,
                binding.DocumentId,
                binding.DocumentVersion,
                new LogicalArtifactDigest(SqlitePersistenceFixture.Hash("corruption-chunk")),
                "corruption sentinel",
                new float[] { 1, 2, 3 })]);
        var manifest = await fixture.VectorStore.FinaliseCandidateAsync(
            candidate,
            new IndexGenerationSpecification(
                1,
                SqlitePersistenceFixture.CorpusId,
                new CorpusRevision(1),
                new CatalogueRevision(1),
                BindingDigestCanonicalizer.CanonicaliseActiveDocumentSet([binding]).Digest,
                BindingDigestCanonicalizer.CanonicaliseSourceBindingSet([binding]).Digest,
                SqlitePersistenceFixture.CompatibilityKey),
            SqlitePersistenceFixture.At(2));
        await ExecuteAsync(
            fixture.Options.VectorDatabasePath,
            "UPDATE vector_chunks SET vector = zeroblob(length(vector));");

        var result = await fixture.ControlStore.CommitGenerationAsync(
            new GenerationCommitRequest(
                new OperationId("generation-corruption-rejected"),
                candidate,
                manifest,
                [binding],
                SqlitePersistenceFixture.At(2)));

        Assert.Equal(StoreMutationOutcome.ValidationFailed, result.Outcome);
        Assert.Equal(0, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM generation_manifests;"));
    }

    [Fact]
    public async Task DurableOperationReplaysRequireTheExactPersistedIntent()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (_, binding) = await fixture.CommitLocalCatalogueAsync();
        var officialBytes = Encoding.UTF8.GetBytes("exact replay official snapshot");
        await using var officialStream = new MemoryStream(officialBytes, writable: false);
        var officialContent = await fixture.ContentStore.PutAsync(
            officialStream,
            officialBytes.Length);
        var registration = new OfficialSourceRegistration(
            new OfficialSourceRegistrationId("exact-replay-registration"),
            new SourceRegistrationRevision(1),
            binding.DatabaseProductId,
            binding.DocumentId,
            new SourceAdapterId("exact-replay-adapter"),
            "https://maintainer.example/exact-replay.pdf",
            CatalogueItemStatus.Active);
        var snapshot = new OfficialSourceSnapshot(
            new OfficialSnapshotId("exact-replay-snapshot"),
            registration.Id,
            officialContent.ContentObjectId,
            officialContent.ByteLength,
            "application/pdf",
            SqlitePersistenceFixture.At(2));
        var officialRequest = new OfficialSourceCommitRequest(
            new OperationId("exact-replay-official"),
            SqlitePersistenceFixture.CorpusId,
            registration,
            snapshot,
            SqlitePersistenceFixture.At(2));

        Assert.Equal(
            StoreMutationOutcome.Applied,
            (await fixture.ControlStore.CommitOfficialSourceAsync(officialRequest)).Outcome);
        Assert.Equal(
            StoreMutationOutcome.AlreadyApplied,
            (await fixture.ControlStore.CommitOfficialSourceAsync(officialRequest)).Outcome);
        var divergentSnapshot = new OfficialSourceSnapshot(
            snapshot.Id,
            snapshot.RegistrationId,
            snapshot.ContentObjectId,
            snapshot.ByteLength,
            "application/octet-stream",
            snapshot.RetrievedAt);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fixture.ControlStore.CommitOfficialSourceAsync(
                officialRequest with { Snapshot = divergentSnapshot }));

        var observation = new OfficialSourceObservation(
            new OfficialObservationId("exact-replay-observation"),
            registration.Id,
            snapshot.Id,
            new ObservationJournalRevision(1),
            OfficialObservationState.Current,
            SqlitePersistenceFixture.At(2),
            TimeSpan.FromDays(7));
        var observationRequest = new ObservationCommitRequest(
            new OperationId("exact-replay-observation-append"),
            SqlitePersistenceFixture.CorpusId,
            observation,
            ExpectedJournalRevision: 0,
            SqlitePersistenceFixture.At(2));

        Assert.Equal(
            StoreMutationOutcome.Applied,
            (await fixture.ControlStore.AppendObservationAsync(observationRequest)).Outcome);
        Assert.Equal(
            StoreMutationOutcome.AlreadyApplied,
            (await fixture.ControlStore.AppendObservationAsync(observationRequest)).Outcome);
        var divergentObservation = new OfficialSourceObservation(
            observation.Id,
            observation.RegistrationId,
            observation.SnapshotId,
            observation.JournalRevision,
            OfficialObservationState.Stale,
            observation.RevalidatedAt,
            observation.MaxAge);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fixture.ControlStore.AppendObservationAsync(
                observationRequest with { Observation = divergentObservation }));

        var manifest = await fixture.CommitGenerationAsync(binding, "exact-replay");
        var generationRequest = new GenerationCommitRequest(
            new OperationId("generation-exact-replay"),
            new CandidateBuildId("candidate-exact-replay"),
            manifest,
            [binding],
            SqlitePersistenceFixture.At(2));

        Assert.Equal(
            StoreMutationOutcome.AlreadyApplied,
            (await fixture.ControlStore.CommitGenerationAsync(generationRequest)).Outcome);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fixture.ControlStore.CommitGenerationAsync(
                generationRequest with
                {
                    CandidateBuildId = new CandidateBuildId("candidate-divergent-replay"),
                }));
        Assert.Equal(1, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM official_source_snapshots;"));
        Assert.Equal(1, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM source_observations;"));
        Assert.Equal(1, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM generation_manifests;"));
    }

    private static async Task<string?> ReadPragmaAsync(string path, string sql)
    {
        await using var connection = OpenReadOnly(path);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToString(
            await command.ExecuteScalarAsync(),
            CultureInfo.InvariantCulture);
    }

    private static async Task<long> CountRowsAsync(string path, string sql)
    {
        await using var connection = OpenReadOnly(path);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await using var reader = await command.ExecuteReaderAsync();
        var count = 0L;

        while (await reader.ReadAsync())
        {
            count++;
        }

        return count;
    }

    private static async Task<long> ScalarAsync(string path, string sql)
    {
        await using var connection = OpenReadOnly(path);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToInt64(
            await command.ExecuteScalarAsync(),
            CultureInfo.InvariantCulture);
    }

    private static async Task ExecuteAsync(string path, string sql)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={path};Mode=ReadWrite;Cache=Private");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        _ = await command.ExecuteNonQueryAsync();
    }

    private static SqliteConnection OpenReadOnly(string path) =>
        new($"Data Source={path};Mode=ReadOnly;Cache=Private");
}
