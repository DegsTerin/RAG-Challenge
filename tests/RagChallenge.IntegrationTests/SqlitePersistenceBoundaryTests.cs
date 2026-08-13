// Purpose: Verifies the physical SQLite authority split, immutable content publication, constraints, and exact derived-vector behaviour in temporary stores.
using System.Globalization;
using System.Text;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

using RagChallenge.Application.IndexingRetrieval;
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
    public async Task CataloguePersistsCanonicalDocumentLanguageAndExactSourceDeclaration()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        _ = await fixture.CommitLocalCatalogueAsync(
            "candidate English document",
            new DocumentContentLanguage("en"),
            new SourceDeclaredLanguage("EN"),
            CatalogueItemStatus.Candidate);

        var snapshot = Assert.IsType<CatalogueSnapshot>(
            await fixture.ControlStore.ReadCurrentCatalogueAsync(
                SqlitePersistenceFixture.CorpusId));
        var document = Assert.Single(snapshot.DocumentVersions);

        Assert.Equal("en", document.ContentLanguage.CanonicalTag);
        Assert.Equal("EN", document.SourceDeclaredLanguage!.ObservedTag);
        Assert.Equal(1, await fixture.ScalarAsync(
            """
            SELECT COUNT(*)
            FROM document_versions
            WHERE content_language = 'en'
              AND source_declared_language = 'EN';
            """));
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
    public async Task DocumentLanguageMigrationPreservesLegacyTagsAndSupportsRollbackReapply()
    {
        const string previousMigration =
            "20260806193919_StrengthenOfficialBindingReferences";
        const string migration =
            "20260807161323_AddDocumentLanguageAndRenderManifestModel";
        var root = Path.Combine(
            Path.GetTempPath(),
            "rag-challenge-document-language-migration-tests",
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
                await context.Database.GetService<IMigrator>().MigrateAsync(previousMigration);
            }

            await ExecuteAsync(
                options.ControlDatabasePath,
                """
                INSERT INTO corpora
                    (corpus_id, corpus_revision, created_at_utc)
                VALUES
                    ('language-upgrade', 1, '2026-08-07T12:00:00.0000000+00:00');

                INSERT INTO database_product_revisions
                    (corpus_id, product_id, product_revision, display_name, status)
                VALUES
                    ('language-upgrade', 'database-upgrade', 1,
                     'Language migration database', 'Candidate');

                INSERT INTO content_objects
                    (content_sha256, byte_length, registered_at_utc)
                VALUES
                    ('1111111111111111111111111111111111111111111111111111111111111111',
                     1, '2026-08-07T12:00:00.0000000+00:00'),
                    ('2222222222222222222222222222222222222222222222222222222222222222',
                     1, '2026-08-07T12:00:00.0000000+00:00');

                INSERT INTO document_versions
                    (corpus_id, document_id, document_version, product_id,
                     product_revision, document_format, content_language,
                     content_sha256, byte_length, media_type, source_adapter_id,
                     source_trust_class, official_registration_id,
                     official_snapshot_id)
                VALUES
                    ('language-upgrade', 'document-en-gb', 1, 'database-upgrade',
                     1, 'Pdf', 'en-GB',
                     '1111111111111111111111111111111111111111111111111111111111111111',
                     1, 'application/pdf', 'local-upgrade', 'LocalAuthorised',
                     NULL, NULL),
                    ('language-upgrade', 'document-pt-br', 1, 'database-upgrade',
                     1, 'Csv', 'pt-BR',
                     '2222222222222222222222222222222222222222222222222222222222222222',
                     1, 'text/csv', 'local-upgrade', 'LocalAuthorised',
                     NULL, NULL);
                """);

            await MigrateControlAsync(options, migration);
            await AssertLanguageMigrationStateAsync(options.ControlDatabasePath);
            await MigrateControlAsync(options, previousMigration);
            Assert.Equal(2, await ScalarAsync(
                options.ControlDatabasePath,
                "SELECT COUNT(*) FROM document_versions WHERE content_language IN ('pt-BR', 'en-GB');"));
            await MigrateControlAsync(options, migration);
            await AssertLanguageMigrationStateAsync(options.ControlDatabasePath);
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

    [Theory]
    [InlineData("pt-BR")]
    [InlineData("en-GB")]
    [InlineData("en")]
    public void StoredVectorMetadataLoadsDocumentLanguageWithoutRegionalInference(string tag)
    {
        var metadata = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(
                $"{{\"ContentLanguage\":\"{tag}\",\"PageNumber\":1," +
                "\"RecordNumber\":null,\"Columns\":{}}"));
        var decoded = StoredVectorChunkCodec.Decode($"RAG-CHUNK-V1:{metadata}\nEvidence");

        Assert.Equal(tag, decoded.ContentLanguage!.CanonicalTag);
        Assert.Equal("Evidence", decoded.Text);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task OfficialBindingMigrationPreservesValidLegacyRowsAndBlocksMismatches(
        bool mismatchedRegistration)
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        const string previousMigration = "20260804184939_AddAdministrationCommandJournal";
        const string migrationName = "20260806193919_StrengthenOfficialBindingReferences";
        _ = await fixture.CommitLocalCatalogueAsync();
        await using (var context = fixture.Options.CreateControlContext())
        {
            var migrator = context.Database.GetService<IMigrator>();
            await migrator.MigrateAsync(previousMigration);
        }

        var observationRegistration = mismatchedRegistration
            ? "legacy-registration-mismatch"
            : "legacy-registration";
        await ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            $"""
            INSERT INTO official_source_registrations
                (corpus_id, registration_id, registration_revision, product_id,
                 document_id, source_adapter_id, canonical_https_url, status)
            VALUES
                ('fixture-corpus', 'legacy-registration', 1, 'db-fixture',
                 'doc-fixture', 'local-fixture',
                 'https://official.invalid/legacy.pdf', 'Active');

            INSERT INTO official_source_snapshots
                (corpus_id, snapshot_id, registration_id, registration_revision,
                 content_sha256, byte_length, media_type, retrieved_at_utc)
            SELECT
                'fixture-corpus', 'legacy-snapshot', 'legacy-registration', 1,
                content_sha256, byte_length, 'application/pdf',
                '2026-01-02T12:00:00.0000000+00:00'
            FROM document_versions
            WHERE corpus_id = 'fixture-corpus'
              AND document_id = 'doc-fixture'
              AND document_version = 1;

            UPDATE document_versions
            SET source_trust_class = 'OfficialExternal',
                official_registration_id = 'legacy-registration',
                official_snapshot_id = 'legacy-snapshot'
            WHERE corpus_id = 'fixture-corpus'
              AND document_id = 'doc-fixture'
              AND document_version = 1;

            INSERT INTO source_observations
                (corpus_id, observation_id, registration_id, snapshot_id,
                 journal_revision, state, revalidated_at_utc, max_age_seconds,
                 operation_id)
            VALUES
                ('fixture-corpus', 'legacy-observation', '{observationRegistration}',
                 'legacy-snapshot', 1, 'Current',
                 '2026-01-02T12:00:00.0000000+00:00', 3600, 'catalogue-1');
            """);

        if (mismatchedRegistration)
        {
            await using var context = fixture.Options.CreateControlContext();
            var migrator = context.Database.GetService<IMigrator>();
            _ = await Assert.ThrowsAnyAsync<Exception>(() => migrator.MigrateAsync());
            Assert.Equal(0, await ScalarAsync(
                fixture.Options.ControlDatabasePath,
                $"SELECT COUNT(*) FROM __EFMigrationsHistory WHERE MigrationId = '{migrationName}';"));
            Assert.Equal(1, await ScalarAsync(
                fixture.Options.ControlDatabasePath,
                """
                SELECT COUNT(*)
                FROM source_observations
                WHERE registration_id = 'legacy-registration-mismatch';
                """));
            return;
        }

        await using (var context = fixture.Options.CreateControlContext())
        {
            var migrator = context.Database.GetService<IMigrator>();
            await migrator.MigrateAsync();
        }

        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            $"SELECT COUNT(*) FROM __EFMigrationsHistory WHERE MigrationId = '{migrationName}';"));
        Assert.Equal(0, await CountRowsAsync(
            fixture.Options.ControlDatabasePath,
            "PRAGMA foreign_key_check;"));
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            """
            SELECT COUNT(*)
            FROM source_observations
            WHERE registration_id = 'legacy-registration'
              AND snapshot_id = 'legacy-snapshot';
            """));
    }

    [Fact]
    public async Task ImmutableContentUsesSha256IdentityAndRejectsMismatches()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var bytes = Encoding.UTF8.GetBytes("synthetic immutable content");
        await using var firstStream = new MemoryStream(bytes, writable: false);
        var first = await fixture.ContentStore.PutAndVerifyAsync(
            new BoundedContentInput(
                firstStream,
                bytes.Length,
                new ContentMediaType("TEXT/PLAIN")));
        Assert.Equal(ContentObjectWriteOutcome.Published, first.WriteOutcome);
        Assert.Equal(SqlitePersistenceFixture.Hash("synthetic immutable content"), first.ContentObjectId.Value);
        Assert.Equal(first.ContentObjectId, first.Sha256);
        Assert.Equal(bytes.Length, first.ByteLength);
        Assert.Equal("text/plain", first.MediaType.Value);
        Assert.Equal("filesystem-sha256-v1", first.Implementation.Value);
        Assert.DoesNotContain(fixture.Options.ContentStoreRoot, first.Implementation.Value);
        Assert.Equal(
            ContentVerificationOutcome.Verified,
            first.Verification.WriteVerification);
        Assert.Equal(
            ContentVerificationOutcome.Verified,
            first.Verification.ReopenVerification);

        await using var repeatedStream = new MemoryStream(bytes, writable: false);
        var repeated = await fixture.ContentStore.PutAndVerifyAsync(
            new BoundedContentInput(
                repeatedStream,
                bytes.Length,
                new ContentMediaType("text/plain"),
                first.ContentObjectId));
        Assert.Equal(ContentObjectWriteOutcome.AlreadyExisted, repeated.WriteOutcome);

        await using (var reopened = await fixture.ContentStore.OpenVerifiedAsync(
            first.ContentObjectId,
            new ExpectedHashAndLength(first.Sha256, first.ByteLength)))
        using (var reader = new StreamReader(reopened.Content, Encoding.UTF8))
        {
            Assert.Equal(0, reopened.Content.Position);
            Assert.Equal(ContentVerificationOutcome.Verified, reopened.ReopenVerification);
            Assert.Equal("synthetic immutable content", await reader.ReadToEndAsync());
        }

        await using var mismatchStream = new MemoryStream(bytes, writable: false);
        var mismatch = await Assert.ThrowsAsync<InvalidDataException>(
            () => fixture.ContentStore.PutAndVerifyAsync(
                new BoundedContentInput(
                    mismatchStream,
                    bytes.Length,
                    new ContentMediaType("text/plain"),
                    new ContentObjectId(new string('0', 64)))));
        var mismatchFailure = Assert.IsType<ContentInputException>(mismatch.InnerException);
        Assert.Equal(ContentInputFailureKind.IdentityMismatch, mismatchFailure.FailureKind);
        Assert.Empty(Directory.EnumerateFiles(
            Path.Combine(fixture.Options.ContentStoreRoot, "quarantine")));

        await Assert.ThrowsAsync<InvalidDataException>(async () =>
        {
            await using var _ = await fixture.ContentStore.OpenVerifiedAsync(
                first.ContentObjectId,
                new ExpectedHashAndLength(first.Sha256, first.ByteLength + 1));
        });

        var objectPath = Path.Combine(
            fixture.Options.ContentStoreRoot,
            "objects",
            first.ContentObjectId.Value[..2],
            $"{first.ContentObjectId.Value}.bin");
        await File.WriteAllTextAsync(objectPath, "corrupted content");
        await Assert.ThrowsAsync<InvalidDataException>(async () =>
        {
            await using var _ = await fixture.ContentStore.OpenVerifiedAsync(
                first.ContentObjectId,
                new ExpectedHashAndLength(first.Sha256, first.ByteLength));
        });
    }

    [Fact]
    public async Task ImmutableContentEnforcesExplicitWriteLimits()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var bytes = "bounded"u8.ToArray();
        await using var exactStream = new MemoryStream(bytes, writable: false);
        var exact = await fixture.ContentStore.PutAndVerifyAsync(
            new BoundedContentInput(
                exactStream,
                bytes.Length,
                ContentMediaType.ApplicationOctetStream));
        Assert.Equal(bytes.Length, exact.ByteLength);

        await using var exceededStream = new MemoryStream(bytes, writable: false);
        await Assert.ThrowsAsync<InvalidDataException>(
            () => fixture.ContentStore.PutAndVerifyAsync(
                new BoundedContentInput(
                    exceededStream,
                    bytes.Length - 1,
                    ContentMediaType.ApplicationOctetStream)));

        await using var absoluteLimitStream = new MemoryStream(bytes, writable: false);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => fixture.ContentStore.PutAndVerifyAsync(
                new BoundedContentInput(
                    absoluteLimitStream,
                    512L * 1024 * 1024 + 1,
                    ContentMediaType.ApplicationOctetStream)));
        Assert.Empty(Directory.EnumerateFiles(
            Path.Combine(fixture.Options.ContentStoreRoot, "quarantine")));
    }

    [Fact]
    public async Task ImmutableContentDoesNotOverconsumeANonSeekableInput()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        await using var stream = new CountingNonSeekableStream(100);

        var failure = await Assert.ThrowsAsync<InvalidDataException>(
            () => fixture.ContentStore.PutAndVerifyAsync(
                new BoundedContentInput(
                    stream,
                    maximumByteLength: 7,
                    ContentMediaType.ApplicationOctetStream)));

        var inputFailure = Assert.IsType<ContentInputException>(failure.InnerException);
        Assert.Equal(ContentInputFailureKind.LimitExceeded, inputFailure.FailureKind);
        Assert.Equal(8, stream.BytesRead);
        Assert.InRange(stream.LargestReadRequest, 1, 8);
        Assert.Empty(Directory.EnumerateFiles(
            Path.Combine(fixture.Options.ContentStoreRoot, "quarantine")));
    }

    [Fact]
    public async Task ImmutableContentReportsEmptyInputWithoutPublishingIt()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        await using var stream = new MemoryStream();

        var failure = await Assert.ThrowsAsync<InvalidDataException>(
            () => fixture.ContentStore.PutAndVerifyAsync(
                new BoundedContentInput(
                    stream,
                    maximumByteLength: 7,
                    ContentMediaType.ApplicationOctetStream)));

        var inputFailure = Assert.IsType<ContentInputException>(failure.InnerException);
        Assert.Equal(ContentInputFailureKind.Empty, inputFailure.FailureKind);
        Assert.Empty(Directory.EnumerateFiles(
            Path.Combine(fixture.Options.ContentStoreRoot, "objects"),
            "*.bin",
            SearchOption.AllDirectories));
        Assert.Empty(Directory.EnumerateFiles(
            Path.Combine(fixture.Options.ContentStoreRoot, "quarantine")));
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
        var unvalidated = await fixture.VectorStore.SearchExactAsync(
            new VectorSearchRequest(
                SqlitePersistenceFixture.CorpusId,
                unvalidatedId,
                SqlitePersistenceFixture.CompatibilityKey,
                new float[] { 1, 0, 0 },
                maximumResults: 1,
                [VectorSearchBindingSelector.FromBinding(binding)]));
        Assert.Equal(VectorSearchOutcome.GenerationUnavailable, unvalidated.Outcome);

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
        var search = await fixture.VectorStore.SearchExactAsync(
            new VectorSearchRequest(
                SqlitePersistenceFixture.CorpusId,
                manifest.IndexGenerationId,
                manifest.IndexCompatibilityKey,
                new float[] { 1, 0, 0 },
                maximumResults: 1,
                [VectorSearchBindingSelector.FromBinding(binding)]));
        Assert.Equal(VectorSearchOutcome.Succeeded, search.Outcome);
        var hit = Assert.Single(search.Hits);
        Assert.Equal(manifest.IndexGenerationId, idempotentReplay.IndexGenerationId);
        Assert.Equal(0, hit.ChunkOrdinal);
        Assert.Equal(1, hit.Score, precision: 12);
        var compatibilityMismatch = await fixture.VectorStore.SearchExactAsync(
            new VectorSearchRequest(
                SqlitePersistenceFixture.CorpusId,
                manifest.IndexGenerationId,
                new IndexCompatibilityKey(new string('f', 64)),
                new float[] { 1, 0, 0 },
                maximumResults: 1,
                [VectorSearchBindingSelector.FromBinding(binding)]));
        Assert.Equal(
            VectorSearchOutcome.GenerationUnavailable,
            compatibilityMismatch.Outcome);
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
    public async Task GenerationCommitRejectsAnIncompleteActiveDocumentSet()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (initialSnapshot, firstBinding) = await fixture.CommitLocalCatalogueAsync();
        var secondBytes = Encoding.UTF8.GetBytes("second active document");
        await using var secondStream = new MemoryStream(secondBytes, writable: false);
        var secondContent = await fixture.ContentStore.PutAndVerifyAsync(
            new BoundedContentInput(
                secondStream,
                secondBytes.Length,
                ContentMediaType.ApplicationPdf));
        var firstDocument = Assert.Single(initialSnapshot.DocumentVersions);
        var secondDocument = new DocumentVersion(
            new DocumentId("doc-fixture-second"),
            new DocumentVersionNumber(1),
            firstDocument.DatabaseProductId,
            firstDocument.DatabaseProductRevision,
            DocumentFormat.Pdf,
            DocumentContentLanguage.EnGb,
            CatalogueItemStatus.Active,
            secondContent.ContentObjectId,
            secondContent.ByteLength,
            "application/pdf",
            new SourceAdapterId("local-fixture"),
            SourceTrustClass.LocalAuthorised);
        var secondSnapshot = new CatalogueSnapshot(
            initialSnapshot.CorpusId,
            new CatalogueRevision(2),
            initialSnapshot.DatabaseCategories,
            initialSnapshot.DatabaseProducts,
            [firstDocument, secondDocument]);
        var catalogueCommit = await fixture.ControlStore.CommitCatalogueAsync(
            new CatalogueCommitRequest(
                new OperationId("catalogue-two-active-documents"),
                secondSnapshot,
                ExpectedCurrentRevision: 1,
                SqlitePersistenceFixture.At(2)));
        Assert.Equal(StoreMutationOutcome.Applied, catalogueCommit.Outcome);
        var (candidate, manifest) = await FinaliseGenerationAsync(
            fixture,
            [firstBinding],
            new CatalogueRevision(2),
            "omitted-active-document");

        var result = await fixture.ControlStore.CommitGenerationAsync(
            new GenerationCommitRequest(
                new OperationId("generation-omitted-active-document"),
                candidate,
                manifest,
                [firstBinding],
                SqlitePersistenceFixture.At(3)));

        Assert.Equal(StoreMutationOutcome.ValidationFailed, result.Outcome);
        Assert.Equal(0, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM generation_manifests;"));
    }

    [Theory]
    [InlineData(GenerationBindingMismatch.DatabaseProduct)]
    [InlineData(GenerationBindingMismatch.DatabaseProductRevision)]
    [InlineData(GenerationBindingMismatch.Document)]
    [InlineData(GenerationBindingMismatch.DocumentVersion)]
    [InlineData(GenerationBindingMismatch.DocumentFormat)]
    [InlineData(GenerationBindingMismatch.SourceAdapter)]
    [InlineData(GenerationBindingMismatch.SourceTrustAndIdentity)]
    public async Task GenerationCommitRejectsDivergentBindingMetadata(
        GenerationBindingMismatch mismatch)
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (_, binding) = await fixture.CommitLocalCatalogueAsync();
        var divergent = CreateDivergentBinding(binding, mismatch);
        var seed = $"binding-{mismatch}".ToLowerInvariant();
        var (candidate, manifest) = await FinaliseGenerationAsync(
            fixture,
            [divergent],
            new CatalogueRevision(1),
            seed);

        var result = await fixture.ControlStore.CommitGenerationAsync(
            new GenerationCommitRequest(
                new OperationId($"generation-{seed}"),
                candidate,
                manifest,
                [divergent],
                SqlitePersistenceFixture.At(3)));

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
        var officialContent = await fixture.ContentStore.PutAndVerifyAsync(
            new BoundedContentInput(
                officialStream,
                officialBytes.Length,
                ContentMediaType.ApplicationPdf));
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

    [Fact]
    public async Task ActivationEvidenceMigrationPreservesHistoryWithoutBackfillAndFailsClosed()
    {
        const string previousMigration =
            "20260807161323_AddDocumentLanguageAndRenderManifestModel";
        const string migration =
            "20260808004846_AddDocumentRightsAndActivationEvidenceBindings";
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (_, binding) = await fixture.CommitLocalCatalogueAsync();
        var manifest = await fixture.CommitGenerationAsync(binding, "historical-activation");
        var activationDigest = BindingDigestCanonicalizer
            .CanonicaliseActivationBindingSet([binding])
            .Digest;

        await MigrateControlAsync(fixture.Options, previousMigration);
        await ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            $"""
            INSERT INTO admin_operations
                (operation_id, corpus_id, operation_kind, status,
                 expected_revision, result_revision, requested_at_utc,
                 completed_at_utc)
            VALUES
                ('historical-activation', '{SqlitePersistenceFixture.CorpusId.Value}',
                 'ActivationCAS', 'Applied', 0, 1,
                 '2026-01-02T12:00:00.0000000+00:00',
                 '2026-01-02T12:00:00.0000000+00:00');

            INSERT INTO activation_records
                (corpus_id, record_revision, previous_record_revision,
                 index_generation_id, catalogue_revision,
                 activation_binding_set_digest, mutation_kind,
                 generation_activated_at_utc, record_updated_at_utc, operation_id)
            VALUES
                ('{SqlitePersistenceFixture.CorpusId.Value}', 1, NULL,
                 '{manifest.IndexGenerationId.Value}', 1,
                 '{activationDigest.Value}', 'Initial',
                 '2026-01-02T12:00:00.0000000+00:00',
                 '2026-01-02T12:00:00.0000000+00:00',
                 'historical-activation');

            INSERT INTO activation_bindings
                (corpus_id, record_revision, product_id, product_revision,
                 document_id, document_version, document_format,
                 source_adapter_id, source_trust_class,
                 official_registration_id, official_snapshot_id,
                 source_observation_id)
            VALUES
                ('{SqlitePersistenceFixture.CorpusId.Value}', 1,
                 '{binding.DatabaseProductId.Value}', {binding.DatabaseProductRevision.Value},
                 '{binding.DocumentId.Value}', {binding.DocumentVersion.Value},
                 '{binding.DocumentFormat}', '{binding.SourceAdapterId.Value}',
                 '{binding.SourceTrustClass}', NULL, NULL, NULL);

            INSERT INTO activation_heads(corpus_id, record_revision, row_revision)
            VALUES ('{SqlitePersistenceFixture.CorpusId.Value}', 1, 1);
            """);
        var existingBytes = await ReadPragmaAsync(
            fixture.Options.ControlDatabasePath,
            ActivationRecordBytesSql);

        await MigrateControlAsync(fixture.Options, migration);
        var migratedBytes = await ReadPragmaAsync(
            fixture.Options.ControlDatabasePath,
            ActivationRecordBytesSql);
        var historical = Assert.IsType<CorpusActivationRecord>(
            await fixture.ControlStore.ReadActiveActivationAsync(
                SqlitePersistenceFixture.CorpusId));

        Assert.Equal(existingBytes, migratedBytes);
        Assert.False(historical.HasCompleteEvidenceBindings);
        Assert.Equal(0, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM activation_evidence_bindings;"));
        Assert.Equal(0, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM activation_rights_decisions;"));
        Assert.Null(await new SqliteQueryActivationReader(fixture.Options).ReadAsync(
            SqlitePersistenceFixture.CorpusId,
            SqlitePersistenceFixture.At(3)));
        Assert.Equal(0, await CountRowsAsync(
            fixture.Options.ControlDatabasePath,
            "PRAGMA foreign_key_check;"));

        await MigrateControlAsync(fixture.Options, previousMigration);
        Assert.Equal(1, await fixture.ScalarAsync("SELECT COUNT(*) FROM activation_records;"));
        await MigrateControlAsync(fixture.Options, migration);
        Assert.Equal(0, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM activation_evidence_bindings;"));
        Assert.Equal(existingBytes, await ReadPragmaAsync(
            fixture.Options.ControlDatabasePath,
            ActivationRecordBytesSql));
    }

    [Fact]
    public async Task AnswerEvidenceMigrationAddsEmptyControlTablesWithoutHistoricalInference()
    {
        const string previousMigration =
            "20260808004846_AddDocumentRightsAndActivationEvidenceBindings";
        const string migration = "20260808033247_AddAnswerEvidenceRecords";
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (_, binding) = await fixture.CommitLocalCatalogueAsync();
        var manifest = await fixture.CommitGenerationAsync(binding, "answer-migration");
        var evidence = await fixture.CreateActivationEvidenceAsync(binding);
        _ = await new GenerationActivationService(fixture.ControlStore).ActivateAsync(
            new GenerationActivationRequest(
                manifest,
                [evidence],
                ExpectedCurrentRevision: 0,
                SqliteControlPlaneStore.MinimumPreviousGenerationRetention,
                new RagChallenge.Application.Administration.AdministrativeAuditContext(
                    new OperationId("activate-answer-migration"),
                    "integration-test",
                    "activate-generation",
                    "synthetic migration fixture",
                    SqlitePersistenceFixture.At(3))));

        await MigrateControlAsync(fixture.Options, previousMigration);
        var activationBytes = await ReadPragmaAsync(
            fixture.Options.ControlDatabasePath,
            ActivationRecordBytesSql);
        var operationsBefore = await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM admin_operations;");

        await MigrateControlAsync(fixture.Options, migration);

        Assert.Equal(3, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' " +
            "AND name IN ('answer_evidence_records', 'answer_evidence_citations', " +
            "'answer_evidence_pages');"));
        Assert.Equal(0, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM answer_evidence_records;"));
        Assert.Equal(0, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM answer_evidence_citations;"));
        Assert.Equal(0, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM answer_evidence_pages;"));
        Assert.Equal(operationsBefore, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM admin_operations;"));
        Assert.Equal(activationBytes, await ReadPragmaAsync(
            fixture.Options.ControlDatabasePath,
            ActivationRecordBytesSql));
        Assert.Equal(0, await CountRowsAsync(
            fixture.Options.ControlDatabasePath,
            "PRAGMA foreign_key_check;"));

        await MigrateControlAsync(fixture.Options, previousMigration);
        Assert.Equal(0, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' " +
            "AND name LIKE 'answer_evidence_%';"));
        Assert.Equal(activationBytes, await ReadPragmaAsync(
            fixture.Options.ControlDatabasePath,
            ActivationRecordBytesSql));
        await MigrateControlAsync(fixture.Options, migration);
        Assert.Equal(0, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM answer_evidence_records;"));
    }

    private const string ActivationRecordBytesSql =
        """
        SELECT hex(CAST(
            corpus_id || char(0) || record_revision || char(0) ||
            ifnull(previous_record_revision, 'NULL') || char(0) ||
            index_generation_id || char(0) || catalogue_revision || char(0) ||
            activation_binding_set_digest || char(0) || mutation_kind || char(0) ||
            generation_activated_at_utc || char(0) || record_updated_at_utc ||
            char(0) || operation_id AS BLOB))
        FROM activation_records;
        """;

    private static async Task MigrateControlAsync(
        SqliteStoreOptions options,
        string targetMigration)
    {
        await using var context = options.CreateControlContext();
        await context.Database.GetService<IMigrator>().MigrateAsync(targetMigration);
    }

    private static async Task AssertLanguageMigrationStateAsync(string controlPath)
    {
        Assert.Equal(1, await ScalarAsync(
            controlPath,
            "SELECT COUNT(*) FROM document_versions WHERE document_id = 'document-pt-br' AND content_language = 'pt-BR';"));
        Assert.Equal(1, await ScalarAsync(
            controlPath,
            "SELECT COUNT(*) FROM document_versions WHERE document_id = 'document-en-gb' AND content_language = 'en-GB';"));
        Assert.Equal(2, await ScalarAsync(
            controlPath,
            "SELECT COUNT(*) FROM document_versions WHERE source_declared_language IS NULL;"));
        Assert.Equal(0, await ScalarAsync(
            controlPath,
            "SELECT COUNT(*) FROM document_render_manifests;"));
        Assert.Equal(0, await ScalarAsync(
            controlPath,
            "SELECT COUNT(*) FROM document_page_images;"));
        Assert.Equal(0, await CountRowsAsync(controlPath, "PRAGMA foreign_key_check;"));
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

    private static async Task<(CandidateBuildId Candidate, FinalisedIndexGenerationManifest Manifest)>
        FinaliseGenerationAsync(
            SqlitePersistenceFixture fixture,
            IReadOnlyCollection<DocumentBinding> bindings,
            CatalogueRevision catalogueRevision,
            string seed)
    {
        var candidate = new CandidateBuildId($"candidate-{seed}");
        await fixture.VectorStore.CreateCandidateAsync(
            candidate,
            SqlitePersistenceFixture.CorpusId,
            SqlitePersistenceFixture.CompatibilityKey,
            vectorDimensions: 3,
            expectedChunkCount: bindings.Count,
            SqlitePersistenceFixture.At(3));
        var ordinal = 0L;

        foreach (var binding in bindings)
        {
            await fixture.VectorStore.AddChunksAsync(
                candidate,
                [new VectorChunkWrite(
                    ordinal,
                    binding.DocumentId,
                    binding.DocumentVersion,
                    new LogicalArtifactDigest(
                        SqlitePersistenceFixture.Hash($"{seed}:{ordinal}")),
                    $"synthetic generation binding {ordinal}",
                    new float[] { 1, ordinal + 1, seed.Length })]);
            ordinal++;
        }

        var specification = new IndexGenerationSpecification(
            manifestSchemaVersion: 1,
            SqlitePersistenceFixture.CorpusId,
            new CorpusRevision(1),
            catalogueRevision,
            BindingDigestCanonicalizer.CanonicaliseActiveDocumentSet(bindings).Digest,
            BindingDigestCanonicalizer.CanonicaliseSourceBindingSet(bindings).Digest,
            SqlitePersistenceFixture.CompatibilityKey);
        var manifest = await fixture.VectorStore.FinaliseCandidateAsync(
            candidate,
            specification,
            SqlitePersistenceFixture.At(3));
        return (candidate, manifest);
    }

    private static DocumentBinding CreateDivergentBinding(
        DocumentBinding binding,
        GenerationBindingMismatch mismatch) =>
        mismatch switch
        {
            GenerationBindingMismatch.DatabaseProduct => new DocumentBinding(
                new DatabaseProductId("db-other"),
                binding.DatabaseProductRevision,
                binding.DocumentId,
                binding.DocumentVersion,
                binding.DocumentFormat,
                binding.SourceAdapterId,
                binding.SourceTrustClass),
            GenerationBindingMismatch.DatabaseProductRevision => new DocumentBinding(
                binding.DatabaseProductId,
                new DatabaseProductRevision(2),
                binding.DocumentId,
                binding.DocumentVersion,
                binding.DocumentFormat,
                binding.SourceAdapterId,
                binding.SourceTrustClass),
            GenerationBindingMismatch.Document => new DocumentBinding(
                binding.DatabaseProductId,
                binding.DatabaseProductRevision,
                new DocumentId("doc-other"),
                binding.DocumentVersion,
                binding.DocumentFormat,
                binding.SourceAdapterId,
                binding.SourceTrustClass),
            GenerationBindingMismatch.DocumentVersion => new DocumentBinding(
                binding.DatabaseProductId,
                binding.DatabaseProductRevision,
                binding.DocumentId,
                new DocumentVersionNumber(2),
                binding.DocumentFormat,
                binding.SourceAdapterId,
                binding.SourceTrustClass),
            GenerationBindingMismatch.DocumentFormat => new DocumentBinding(
                binding.DatabaseProductId,
                binding.DatabaseProductRevision,
                binding.DocumentId,
                binding.DocumentVersion,
                DocumentFormat.Csv,
                binding.SourceAdapterId,
                binding.SourceTrustClass),
            GenerationBindingMismatch.SourceAdapter => new DocumentBinding(
                binding.DatabaseProductId,
                binding.DatabaseProductRevision,
                binding.DocumentId,
                binding.DocumentVersion,
                binding.DocumentFormat,
                new SourceAdapterId("local-other"),
                binding.SourceTrustClass),
            GenerationBindingMismatch.SourceTrustAndIdentity => new DocumentBinding(
                binding.DatabaseProductId,
                binding.DatabaseProductRevision,
                binding.DocumentId,
                binding.DocumentVersion,
                binding.DocumentFormat,
                binding.SourceAdapterId,
                SourceTrustClass.OfficialExternal,
                new OfficialSourceRegistrationId("registration-other"),
                new OfficialSnapshotId("snapshot-other"),
                new OfficialObservationId("observation-other")),
            _ => throw new ArgumentOutOfRangeException(nameof(mismatch)),
        };

    public enum GenerationBindingMismatch
    {
        DatabaseProduct,
        DatabaseProductRevision,
        Document,
        DocumentVersion,
        DocumentFormat,
        SourceAdapter,
        SourceTrustAndIdentity,
    }

    private sealed class CountingNonSeekableStream(int length) : Stream
    {
        private int remaining = length;

        internal int BytesRead { get; private set; }

        internal int LargestReadRequest { get; private set; }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            ReadCore(buffer.AsSpan(offset, count));

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(ReadCore(buffer.Span));
        }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();

        private int ReadCore(Span<byte> buffer)
        {
            LargestReadRequest = Math.Max(LargestReadRequest, buffer.Length);
            var read = Math.Min(buffer.Length, remaining);
            buffer[..read].Fill((byte)'x');
            remaining -= read;
            BytesRead += read;
            return read;
        }
    }
}
