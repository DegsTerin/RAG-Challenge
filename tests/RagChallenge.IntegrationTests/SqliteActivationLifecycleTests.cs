// Purpose: Verifies control-plane CAS, digest and observation gates, bounded retention, audited cleanup, rollback by new revision, and isolated recovery.
using System.Globalization;
using System.Text;

using Microsoft.Data.Sqlite;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;
using RagChallenge.Infrastructure.Persistence;

namespace RagChallenge.IntegrationTests;

public sealed class SqliteActivationLifecycleTests
{
    [Fact]
    public async Task ActivationAndQueryRejectDocumentLanguageOutsideRuntimeV1()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (_, binding) = await fixture.CommitLocalCatalogueAsync();
        var manifest = await fixture.CommitGenerationAsync(binding, "language-gate");
        var proposed = ActivationRecordFactory.CreateInitial(
            manifest,
            [binding],
            SqlitePersistenceFixture.At(2));
        await ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            "UPDATE document_versions SET content_language = 'en';");

        var rejected = await ActivateAsync(
            fixture,
            "activation-language-rejected",
            ActivationMutationKind.Initial,
            expectedRevision: 0,
            proposed,
            SqlitePersistenceFixture.At(2));

        Assert.Equal(StoreMutationOutcome.ValidationFailed, rejected.Outcome);
        await ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            "UPDATE document_versions SET content_language = 'en-GB';");
        var activated = await ActivateAsync(
            fixture,
            "activation-language-accepted",
            ActivationMutationKind.Initial,
            expectedRevision: 0,
            proposed,
            SqlitePersistenceFixture.At(2));
        Assert.Equal(StoreMutationOutcome.Applied, activated.Outcome);
        await ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            "UPDATE document_versions SET content_language = 'en';");

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            new SqliteQueryActivationReader(fixture.Options).ReadAsync(
                SqlitePersistenceFixture.CorpusId,
                SqlitePersistenceFixture.At(3)));
    }

    [Fact]
    public async Task CasRejectsAllThreeDigestMismatchesBeforeActivation()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (_, binding) = await fixture.CommitLocalCatalogueAsync();
        var manifest = await fixture.CommitGenerationAsync(binding, "digest-gate");
        var mismatchedBinding = new DocumentBinding(
            binding.DatabaseProductId,
            binding.DatabaseProductRevision,
            binding.DocumentId,
            binding.DocumentVersion,
            DocumentFormat.Csv,
            new SourceAdapterId("different-adapter"),
            SourceTrustClass.LocalAuthorised);
        var proposed = new CorpusActivationRecord(
            SqlitePersistenceFixture.CorpusId,
            new ActivationRecordRevision(1),
            previousRecordRevision: null,
            manifest.IndexGenerationId,
            manifest.CatalogueRevision,
            new ActivationBindingSetDigest(new string('0', 64)),
            [mismatchedBinding],
            SqlitePersistenceFixture.At(2),
            SqlitePersistenceFixture.At(2));

        var result = await fixture.ControlStore.CompareExchangeActivationAsync(
            new ActivationCompareExchangeRequest(
                new OperationId("activation-digest-rejected"),
                ActivationMutationKind.Initial,
                ExpectedCurrentRevision: 0,
                proposed,
                SqlitePersistenceFixture.CompatibilityKey,
                SqlitePersistenceFixture.At(2),
                SqliteControlPlaneStore.MinimumPreviousGenerationRetention));

        Assert.Equal(StoreMutationOutcome.ValidationFailed, result.Outcome);
        Assert.Contains(
            ActivationValidationFailure.ActiveDocumentSetDigestMismatch,
            result.ValidationFailures);
        Assert.Contains(
            ActivationValidationFailure.SourceBindingSetDigestMismatch,
            result.ValidationFailures);
        Assert.Contains(
            ActivationValidationFailure.ActivationBindingSetDigestMismatch,
            result.ValidationFailures);
        Assert.Null(await fixture.ControlStore.ReadActiveActivationAsync(
            SqlitePersistenceFixture.CorpusId));
    }

    [Fact]
    public async Task CasRejectsContentThatIsNoLongerReopenableByItsSha256Identity()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (catalogue, binding) = await fixture.CommitLocalCatalogueAsync();
        var manifest = await fixture.CommitGenerationAsync(binding, "content-readback");
        var contentObjectId = Assert.Single(catalogue.DocumentVersions).ContentObjectId;
        var objectPath = Path.Combine(
            fixture.Options.ContentStoreRoot,
            "objects",
            contentObjectId.Value[..2],
            $"{contentObjectId.Value}.bin");
        await File.WriteAllTextAsync(objectPath, "corrupted after generation finalisation");
        var initial = ActivationRecordFactory.CreateInitial(
            manifest,
            [binding],
            SqlitePersistenceFixture.At(2));

        var result = await ActivateAsync(
            fixture,
            "activation-content-rejected",
            ActivationMutationKind.Initial,
            expectedRevision: 0,
            initial,
            SqlitePersistenceFixture.At(2));

        Assert.Equal(StoreMutationOutcome.ValidationFailed, result.Outcome);
        Assert.Null(await fixture.ControlStore.ReadActiveActivationAsync(
            SqlitePersistenceFixture.CorpusId));
    }

    [Fact]
    public async Task CasRejectsVectorPayloadChangedAfterManifestCommit()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (_, binding) = await fixture.CommitLocalCatalogueAsync();
        var manifest = await fixture.CommitGenerationAsync(binding, "vector-readback");
        await ExecuteAsync(
            fixture.Options.VectorDatabasePath,
            "UPDATE vector_chunks SET vector = zeroblob(length(vector));");
        var initial = ActivationRecordFactory.CreateInitial(
            manifest,
            [binding],
            SqlitePersistenceFixture.At(2));

        var result = await ActivateAsync(
            fixture,
            "activation-vector-rejected",
            ActivationMutationKind.Initial,
            expectedRevision: 0,
            initial,
            SqlitePersistenceFixture.At(2));

        Assert.Equal(StoreMutationOutcome.ValidationFailed, result.Outcome);
        Assert.Null(await fixture.ControlStore.ReadActiveActivationAsync(
            SqlitePersistenceFixture.CorpusId));
    }

    [Fact]
    public async Task ConcurrentCasRetentionCleanupRollbackAndRecoveryRemainAuditable()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (_, binding) = await fixture.CommitLocalCatalogueAsync();
        var officialBytes = Encoding.UTF8.GetBytes("unbound official snapshot");
        await using var officialStream = new MemoryStream(officialBytes, writable: false);
        var officialContent = await fixture.ContentStore.PutAsync(
            officialStream,
            officialBytes.Length);
        var officialRegistration = new OfficialSourceRegistration(
            new OfficialSourceRegistrationId("cleanup-official-registration"),
            new SourceRegistrationRevision(1),
            binding.DatabaseProductId,
            binding.DocumentId,
            new SourceAdapterId("cleanup-official-adapter"),
            "https://maintainer.example/cleanup.pdf",
            CatalogueItemStatus.Active);
        var officialSnapshot = new OfficialSourceSnapshot(
            new OfficialSnapshotId("cleanup-official-snapshot"),
            officialRegistration.Id,
            officialContent.ContentObjectId,
            officialContent.ByteLength,
            "application/pdf",
            SqlitePersistenceFixture.At(1));
        var officialCommit = await fixture.ControlStore.CommitOfficialSourceAsync(
            new OfficialSourceCommitRequest(
                new OperationId("cleanup-official-commit"),
                SqlitePersistenceFixture.CorpusId,
                officialRegistration,
                officialSnapshot,
                SqlitePersistenceFixture.At(1)));
        Assert.Equal(StoreMutationOutcome.Applied, officialCommit.Outcome);
        var sharedBytes = Encoding.UTF8.GetBytes("content referenced by another corpus");
        await using var sharedStream = new MemoryStream(sharedBytes, writable: false);
        var sharedContent = await fixture.ContentStore.PutAsync(sharedStream, sharedBytes.Length);
        var sharedCorpusId = new CorpusId("shared-content-corpus");
        var sharedCategory = new DatabaseCategory(
            new DatabaseCategoryId("shared-content-category"),
            "Shared content category");
        var sharedProduct = new DatabaseProduct(
            new DatabaseProductId("shared-content-product"),
            new DatabaseProductRevision(1),
            "Shared content product",
            CatalogueItemStatus.Active,
            [sharedCategory.Id]);
        var sharedDocument = new DocumentVersion(
            new DocumentId("shared-content-document"),
            new DocumentVersionNumber(1),
            sharedProduct.Id,
            sharedProduct.Revision,
            DocumentFormat.Pdf,
            DocumentContentLanguage.EnGb,
            CatalogueItemStatus.Active,
            sharedContent.ContentObjectId,
            sharedContent.ByteLength,
            "application/pdf",
            new SourceAdapterId("shared-content-adapter"),
            SourceTrustClass.LocalAuthorised);
        var sharedCommit = await fixture.ControlStore.CommitCatalogueAsync(
            new CatalogueCommitRequest(
                new OperationId("shared-content-catalogue"),
                new CatalogueSnapshot(
                    sharedCorpusId,
                    new CatalogueRevision(1),
                    [sharedCategory],
                    [sharedProduct],
                    [sharedDocument]),
                ExpectedCurrentRevision: 0,
                SqlitePersistenceFixture.At(1)));
        Assert.Equal(StoreMutationOutcome.Applied, sharedCommit.Outcome);
        var generationA = await fixture.CommitGenerationAsync(binding, "a");
        var initial = ActivationRecordFactory.CreateInitial(
            generationA,
            [binding],
            SqlitePersistenceFixture.At(2));
        var initialResult = await ActivateAsync(
            fixture,
            "activation-a",
            ActivationMutationKind.Initial,
            expectedRevision: 0,
            initial,
            SqlitePersistenceFixture.At(2));
        Assert.Equal(StoreMutationOutcome.Applied, initialResult.Outcome);

        var generationB = await fixture.CommitGenerationAsync(binding, "b");
        var replacement = ActivationRecordFactory.CreateGenerationReplacement(
            initial,
            generationB,
            [binding],
            SqlitePersistenceFixture.At(3));
        var concurrentResults = await Task.WhenAll(
            ActivateAsync(
                fixture,
                "activation-b-first",
                ActivationMutationKind.Replacement,
                expectedRevision: 1,
                replacement,
                SqlitePersistenceFixture.At(3)),
            new SqliteControlPlaneStore(fixture.Options).CompareExchangeActivationAsync(
                new ActivationCompareExchangeRequest(
                    new OperationId("activation-b-second"),
                    ActivationMutationKind.Replacement,
                    ExpectedCurrentRevision: 1,
                    replacement,
                    SqlitePersistenceFixture.CompatibilityKey,
                    SqlitePersistenceFixture.At(3),
                    SqliteControlPlaneStore.MinimumPreviousGenerationRetention)));
        Assert.Single(
            concurrentResults,
            result => result.Outcome == StoreMutationOutcome.Applied);
        Assert.Single(
            concurrentResults,
            result => result.Outcome == StoreMutationOutcome.RevisionConflict);
        Assert.Equal(
            SqlitePersistenceFixture.At(17),
            await ReadRetentionUntilAsync(fixture, generationA.IndexGenerationId));

        var currentB = await fixture.ControlStore.ReadActiveActivationAsync(
            SqlitePersistenceFixture.CorpusId);
        Assert.NotNull(currentB);
        var rollback = ActivationRecordFactory.CreateRollback(
            currentB,
            generationA,
            [binding],
            SqlitePersistenceFixture.At(4));
        var rollbackResult = await ActivateAsync(
            fixture,
            "activation-rollback-a",
            ActivationMutationKind.Rollback,
            expectedRevision: 2,
            rollback,
            SqlitePersistenceFixture.At(4));
        Assert.Equal(StoreMutationOutcome.Applied, rollbackResult.Outcome);
        Assert.Equal(3, rollbackResult.CurrentRecord!.RecordRevision.Value);
        Assert.Equal(generationA.IndexGenerationId, rollbackResult.CurrentRecord.IndexGenerationId);

        var generationC = await fixture.CommitGenerationAsync(binding, "c");
        var replacementC = ActivationRecordFactory.CreateGenerationReplacement(
            rollbackResult.CurrentRecord,
            generationC,
            [binding],
            SqlitePersistenceFixture.At(5));
        var replacementCResult = await ActivateAsync(
            fixture,
            "activation-c",
            ActivationMutationKind.Replacement,
            expectedRevision: 3,
            replacementC,
            SqlitePersistenceFixture.At(5));
        Assert.Equal(StoreMutationOutcome.Applied, replacementCResult.Outcome);

        var orphanBytes = Encoding.UTF8.GetBytes("unreachable synthetic content");
        await using var orphanStream = new MemoryStream(orphanBytes, writable: false);
        var orphan = await fixture.ContentStore.PutAsync(orphanStream, orphanBytes.Length);
        await RegisterOrphanContentAsync(fixture, orphan, SqlitePersistenceFixture.At(5));
        var cleanup = new SqliteStorageMaintenance(fixture.Options);
        var cleanupResult = await cleanup.RunManualCleanupAsync(
            new OperationId("cleanup-expired-hold"),
            SqlitePersistenceFixture.CorpusId,
            SqlitePersistenceFixture.At(18));
        Assert.Equal(1, cleanupResult.RemovedVectorGenerations);
        Assert.Equal(1, cleanupResult.RemovedContentObjects);
        Assert.False(cleanupResult.AlreadyApplied);
        Assert.Equal(0, await ScalarAsync(
            fixture.Options.VectorDatabasePath,
            $"SELECT COUNT(*) FROM vector_builds WHERE index_generation_id = '{generationB.IndexGenerationId.Value}';"));
        await Assert.ThrowsAsync<FileNotFoundException>(async () =>
        {
            await using var _ = await fixture.ContentStore.OpenReadAsync(orphan.ContentObjectId);
        });
        await using (var preservedOfficial = await fixture.ContentStore.OpenReadAsync(
            officialContent.ContentObjectId))
        {
            Assert.Equal(officialContent.ByteLength, preservedOfficial.Length);
        }
        await using (var preservedShared = await fixture.ContentStore.OpenReadAsync(
            sharedContent.ContentObjectId))
        {
            Assert.Equal(sharedContent.ByteLength, preservedShared.Length);
        }
        Assert.Equal(0, await fixture.ScalarAsync(
            $"SELECT COUNT(*) FROM content_objects WHERE content_sha256 = '{orphan.ContentObjectId.Value}';"));
        Assert.Equal(2, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM audit_events WHERE operation_id = 'cleanup-expired-hold';"));

        var replay = await cleanup.RunManualCleanupAsync(
            new OperationId("cleanup-expired-hold"),
            SqlitePersistenceFixture.CorpusId,
            SqlitePersistenceFixture.At(18));
        Assert.True(replay.AlreadyApplied);

        var recovery = new SqliteRecoverySnapshotService(fixture.Options);
        var recoveryResult = await recovery.CreateAndVerifyAsync(
            new OperationId("recovery-verified"),
            SqlitePersistenceFixture.CorpusId,
            Path.Combine(fixture.RootPath, "recovery"),
            SqlitePersistenceFixture.At(18));
        var verified = await SqliteRecoverySnapshotService.VerifyIsolatedAsync(
            recoveryResult.SnapshotPath);
        Assert.True(verified.IsValid, string.Join(Environment.NewLine, verified.Failures));
        Assert.Equal(3, recoveryResult.ContentObjectCount);
        Assert.Equal(0, await fixture.ScalarAsync("SELECT COUNT(*) FROM recovery_leases;"));
        Assert.Equal(2, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM audit_events WHERE operation_id = 'recovery-verified';"));

        var copiedContent = Directory.EnumerateFiles(
            Path.Combine(recoveryResult.SnapshotPath, "content"),
            "*.bin",
            SearchOption.AllDirectories).ToArray();
        Assert.Equal(3, copiedContent.Length);
        await File.AppendAllTextAsync(copiedContent[0], "corruption");
        var corrupted = await SqliteRecoverySnapshotService.VerifyIsolatedAsync(
            recoveryResult.SnapshotPath);
        Assert.False(corrupted.IsValid);
        Assert.Contains(corrupted.Failures, failure =>
            failure.Contains("mismatch", StringComparison.Ordinal));
    }

    [Fact]
    public async Task OfficialActivationRequiresAStoredCurrentObservation()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var bytes = Encoding.UTF8.GetBytes("synthetic official snapshot");
        await using var content = new MemoryStream(bytes, writable: false);
        var contentResult = await fixture.ContentStore.PutAsync(content, bytes.Length);
        var registrationId = new OfficialSourceRegistrationId("official-registration");
        var snapshotId = new OfficialSnapshotId("official-snapshot");
        var productId = new DatabaseProductId("db-official");
        var productRevision = new DatabaseProductRevision(1);
        var documentId = new DocumentId("doc-official");
        var documentVersion = new DocumentVersionNumber(1);
        var adapterId = new SourceAdapterId("official-fixture");
        var category = new DatabaseCategory(
            new DatabaseCategoryId("category-official"),
            "Official fixture");
        var product = new DatabaseProduct(
            productId,
            productRevision,
            "Official Database",
            CatalogueItemStatus.Active,
            [category.Id]);
        var document = new DocumentVersion(
            documentId,
            documentVersion,
            productId,
            productRevision,
            DocumentFormat.Pdf,
            DocumentContentLanguage.EnGb,
            CatalogueItemStatus.Active,
            contentResult.ContentObjectId,
            contentResult.ByteLength,
            "application/pdf",
            adapterId,
            SourceTrustClass.OfficialExternal,
            registrationId,
            snapshotId);
        var catalogue = new CatalogueSnapshot(
            SqlitePersistenceFixture.CorpusId,
            new CatalogueRevision(1),
            [category],
            [product],
            [document]);
        Assert.Equal(StoreMutationOutcome.Applied, (
            await fixture.ControlStore.CommitCatalogueAsync(
                new CatalogueCommitRequest(
                    new OperationId("catalogue-official"),
                    catalogue,
                    ExpectedCurrentRevision: 0,
                    SqlitePersistenceFixture.At(1)))).Outcome);
        var registration = new OfficialSourceRegistration(
            registrationId,
            new SourceRegistrationRevision(1),
            productId,
            documentId,
            adapterId,
            "https://maintainer.example/docs.pdf",
            CatalogueItemStatus.Active);
        var snapshot = new OfficialSourceSnapshot(
            snapshotId,
            registrationId,
            contentResult.ContentObjectId,
            contentResult.ByteLength,
            "application/pdf",
            SqlitePersistenceFixture.At(1));
        Assert.Equal(StoreMutationOutcome.Applied, (
            await fixture.ControlStore.CommitOfficialSourceAsync(
                new OfficialSourceCommitRequest(
                    new OperationId("official-source"),
                    SqlitePersistenceFixture.CorpusId,
                    registration,
                    snapshot,
                    SqlitePersistenceFixture.At(1)))).Outcome);

        var missingBinding = CreateOfficialBinding(
            productId,
            productRevision,
            documentId,
            documentVersion,
            adapterId,
            registrationId,
            snapshotId,
            new OfficialObservationId("observation-missing"));
        var manifest = await fixture.CommitGenerationAsync(missingBinding, "official");
        var missingObservationRecord = ActivationRecordFactory.CreateInitial(
            manifest,
            [missingBinding],
            SqlitePersistenceFixture.At(2));
        var rejected = await ActivateAsync(
            fixture,
            "activation-observation-rejected",
            ActivationMutationKind.Initial,
            expectedRevision: 0,
            missingObservationRecord,
            SqlitePersistenceFixture.At(2));
        Assert.Equal(StoreMutationOutcome.ValidationFailed, rejected.Outcome);
        Assert.Contains(
            ActivationValidationFailure.ObservationMissing,
            rejected.ValidationFailures);

        var observation = new OfficialSourceObservation(
            new OfficialObservationId("observation-current"),
            registrationId,
            snapshotId,
            new ObservationJournalRevision(1),
            OfficialObservationState.Current,
            SqlitePersistenceFixture.At(1),
            TimeSpan.FromDays(7));
        Assert.Equal(StoreMutationOutcome.Applied, (
            await fixture.ControlStore.AppendObservationAsync(
                new ObservationCommitRequest(
                    new OperationId("observation-append"),
                    SqlitePersistenceFixture.CorpusId,
                    observation,
                    ExpectedJournalRevision: 0,
                    SqlitePersistenceFixture.At(1)))).Outcome);
        var observedBinding = missingBinding.WithObservation(observation.Id);
        var acceptedRecord = ActivationRecordFactory.CreateInitial(
            manifest,
            [observedBinding],
            SqlitePersistenceFixture.At(2));
        var accepted = await ActivateAsync(
            fixture,
            "activation-observation-accepted",
            ActivationMutationKind.Initial,
            expectedRevision: 0,
            acceptedRecord,
            SqlitePersistenceFixture.At(2));
        Assert.Equal(StoreMutationOutcome.Applied, accepted.Outcome);
    }

    private static Task<ActivationMutationResult> ActivateAsync(
        SqlitePersistenceFixture fixture,
        string operationId,
        ActivationMutationKind kind,
        long expectedRevision,
        CorpusActivationRecord record,
        DateTimeOffset evaluatedAt) =>
        fixture.ControlStore.CompareExchangeActivationAsync(
            new ActivationCompareExchangeRequest(
                new OperationId(operationId),
                kind,
                expectedRevision,
                record,
                SqlitePersistenceFixture.CompatibilityKey,
                evaluatedAt,
                SqliteControlPlaneStore.MinimumPreviousGenerationRetention));

    private static DocumentBinding CreateOfficialBinding(
        DatabaseProductId productId,
        DatabaseProductRevision productRevision,
        DocumentId documentId,
        DocumentVersionNumber documentVersion,
        SourceAdapterId adapterId,
        OfficialSourceRegistrationId registrationId,
        OfficialSnapshotId snapshotId,
        OfficialObservationId observationId) =>
        new(
            productId,
            productRevision,
            documentId,
            documentVersion,
            DocumentFormat.Pdf,
            adapterId,
            SourceTrustClass.OfficialExternal,
            registrationId,
            snapshotId,
            observationId);

    private static async Task<DateTimeOffset> ReadRetentionUntilAsync(
        SqlitePersistenceFixture fixture,
        IndexGenerationId generationId)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={fixture.Options.ControlDatabasePath};Mode=ReadOnly;Cache=Private");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT retain_until_utc
            FROM generation_retention
            WHERE corpus_id = $corpusId
              AND index_generation_id = $generationId;
            """;
        command.Parameters.AddWithValue("$corpusId", SqlitePersistenceFixture.CorpusId.Value);
        command.Parameters.AddWithValue("$generationId", generationId.Value);
        var value = Convert.ToString(
            await command.ExecuteScalarAsync(),
            CultureInfo.InvariantCulture);
        return DateTimeOffset.ParseExact(
            value!,
            "O",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None);
    }

    private static async Task RegisterOrphanContentAsync(
        SqlitePersistenceFixture fixture,
        ContentWriteResult content,
        DateTimeOffset registeredAt)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={fixture.Options.ControlDatabasePath};Mode=ReadWrite;Cache=Private");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO content_objects(content_sha256, byte_length, registered_at_utc)
            VALUES ($sha256, $byteLength, $registeredAtUtc);
            """;
        command.Parameters.AddWithValue("$sha256", content.ContentObjectId.Value);
        command.Parameters.AddWithValue("$byteLength", content.ByteLength);
        command.Parameters.AddWithValue("$registeredAtUtc", registeredAt.ToString("O", CultureInfo.InvariantCulture));
        _ = await command.ExecuteNonQueryAsync();
    }

    private static async Task<long> ScalarAsync(string path, string sql)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={path};Mode=ReadOnly;Cache=Private");
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
}
