// Purpose: Verifies crash-surviving cleanup reservations are reconciled against global durable reachability before any physical deletion.
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Infrastructure.Persistence;

namespace RagChallenge.IntegrationTests;

public sealed class SqliteStorageMaintenanceReservationTests
{
    [Fact]
    public async Task RenderManifestSourceAndPageImageRemainGloballyReachable()
    {
        await using var fixture = await CreateInitialisedFixtureAsync();
        var image = await PutAndRegisterAsync(fixture, "synthetic immutable PNG bytes");
        const string manifestDigest =
            "9999999999999999999999999999999999999999999999999999999999999999";
        var manifestId = $"rendermanifest-{manifestDigest}";
        string sourceContent;

        await using (var context = fixture.Options.CreateControlContext())
        {
            sourceContent = await context.DocumentVersions
                .Where(row => row.CorpusId == SqlitePersistenceFixture.CorpusId.Value &&
                    row.DocumentId == "doc-fixture" &&
                    row.DocumentVersion == 1)
                .Select(row => row.ContentSha256)
                .SingleAsync();
            context.DocumentRenderManifests.Add(new DocumentRenderManifestRow
            {
                RenderManifestId = manifestId,
                ManifestSha256 = manifestDigest,
                SchemaVersion = 1,
                CorpusId = SqlitePersistenceFixture.CorpusId.Value,
                DocumentId = "doc-fixture",
                DocumentVersion = 1,
                SourceContentSha256 = sourceContent,
                SourcePageCount = 1,
                RenderProfileId = "pdf-page-png-v1",
                RendererDescriptor = "synthetic-renderer:v1",
                GeneratedAtUtc = SqlitePersistenceFixture.At(5).ToString(
                    "O",
                    CultureInfo.InvariantCulture),
            });
            await context.SaveChangesAsync();
        }

        await using (var mismatched = fixture.Options.CreateControlContext())
        {
            mismatched.DocumentPageImages.Add(PageRow(
                manifestId,
                image,
                sourceContentSha256: image.ContentObjectId.Value));
            await Assert.ThrowsAsync<DbUpdateException>(() => mismatched.SaveChangesAsync());
        }

        await using (var context = fixture.Options.CreateControlContext())
        {
            context.DocumentPageImages.Add(PageRow(manifestId, image, sourceContent));
            await context.SaveChangesAsync();
        }

        var result = await new SqliteStorageMaintenance(fixture.Options)
            .RunManualCleanupAsync(
                new OperationId("cleanup-render-manifest-reachability"),
                SqlitePersistenceFixture.CorpusId,
                SqlitePersistenceFixture.At(6));

        Assert.Equal(0, result.RemovedContentObjects);
        Assert.Equal(1, await CountContentRowsAsync(fixture, image.ContentObjectId));
        await using var reopened = await fixture.ContentStore.OpenReadAsync(
            image.ContentObjectId,
            CancellationToken.None);
        Assert.Equal(image.ByteLength, reopened.Length);
    }

    [Fact]
    public async Task AppliedReplayRestoresReservationReferencedByDocumentAfterCrash()
    {
        await using var fixture = await CreateInitialisedFixtureAsync();
        var operationId = new OperationId("cleanup-crash-document-reference");
        var reserved = await CreateAppliedReservationAsync(
            fixture,
            operationId,
            "reserved then referenced by a document");
        await AddDocumentReferenceAsync(
            fixture,
            reserved.ContentObjectId,
            reserved.ByteLength,
            SqlitePersistenceFixture.CorpusId,
            "doc-crash-reference");

        var result = await new SqliteStorageMaintenance(fixture.Options)
            .RunManualCleanupAsync(
                operationId,
                SqlitePersistenceFixture.CorpusId,
                SqlitePersistenceFixture.At(6));

        Assert.True(result.AlreadyApplied);
        Assert.Empty(fixture.ContentStore.EnumerateDeletionReservations(operationId));
        await AssertContentReopensAsync(fixture, reserved);
        Assert.Equal(1, await CountContentRowsAsync(fixture, reserved.ContentObjectId));
    }

    [Fact]
    public async Task AppliedReplayRestoresReservationWhileContentRowStillExists()
    {
        await using var fixture = await CreateInitialisedFixtureAsync();
        var operationId = new OperationId("cleanup-crash-content-row");
        var reserved = await CreateAppliedReservationAsync(
            fixture,
            operationId,
            "content row survived the interrupted deletion transaction");

        var result = await new SqliteStorageMaintenance(fixture.Options)
            .RunManualCleanupAsync(
                operationId,
                SqlitePersistenceFixture.CorpusId,
                SqlitePersistenceFixture.At(6));

        Assert.True(result.AlreadyApplied);
        Assert.Empty(fixture.ContentStore.EnumerateDeletionReservations(operationId));
        await AssertContentReopensAsync(fixture, reserved);
        Assert.Equal(1, await CountContentRowsAsync(fixture, reserved.ContentObjectId));
    }

    [Fact]
    public async Task InProgressReplayReconcilesPersistedPlanBeforeContinuingCleanup()
    {
        await using var fixture = await CreateInitialisedFixtureAsync();
        var operationId = new OperationId("cleanup-in-progress-crash-reference");
        var content = await PutAndRegisterAsync(
            fixture,
            "in-progress reservation regains a durable reference");
        var planBytes = JsonSerializer.SerializeToUtf8Bytes(new
        {
            schemaVersion = 1,
            operationId = operationId.Value,
            corpusId = SqlitePersistenceFixture.CorpusId.Value,
            requestedAtUtc = SqlitePersistenceFixture.At(6).ToString(
                "O",
                CultureInfo.InvariantCulture),
            vectorGenerations = Array.Empty<object>(),
            contentObjects = new[]
            {
                new
                {
                    contentObjectId = content.ContentObjectId.Value,
                    byteLength = content.ByteLength,
                },
            },
        });
        var planDigest = Hash(planBytes);
        await fixture.ContentStore.PublishCleanupPlanAsync(
            operationId,
            planBytes,
            CancellationToken.None);
        await AddInProgressOperationAndAuditAsync(
            fixture,
            operationId,
            planDigest);
        var reservation = await fixture.ContentStore.ReserveForDeletionAsync(
            operationId,
            content.ContentObjectId,
            content.ByteLength,
            CancellationToken.None);
        Assert.True(reservation.WasPresent);
        await AddDocumentReferenceAsync(
            fixture,
            content.ContentObjectId,
            content.ByteLength,
            SqlitePersistenceFixture.CorpusId,
            "doc-in-progress-reference");

        var result = await new SqliteStorageMaintenance(fixture.Options)
            .RunManualCleanupAsync(
                operationId,
                SqlitePersistenceFixture.CorpusId,
                SqlitePersistenceFixture.At(6));

        Assert.False(result.AlreadyApplied);
        Assert.Equal(0, result.RemovedContentObjects);
        Assert.Empty(fixture.ContentStore.EnumerateDeletionReservations(operationId));
        await using var reopened = await fixture.ContentStore.OpenReadAsync(
            content.ContentObjectId);
        Assert.Equal(content.ByteLength, reopened.Length);
        Assert.Equal(1, await fixture.ScalarAsync(
            $"SELECT COUNT(*) FROM admin_operations WHERE operation_id = '{operationId.Value}' AND status = 'Applied';"));
    }

    [Fact]
    public async Task AppliedReplayRestoresReservationReferencedByOfficialSnapshotAfterCrash()
    {
        await using var fixture = await CreateInitialisedFixtureAsync();
        var operationId = new OperationId("cleanup-crash-snapshot-reference");
        var reserved = await CreateAppliedReservationAsync(
            fixture,
            operationId,
            "reserved then referenced by an official snapshot");
        await AddOfficialSnapshotReferenceAsync(
            fixture,
            reserved.ContentObjectId,
            reserved.ByteLength);

        var result = await new SqliteStorageMaintenance(fixture.Options)
            .RunManualCleanupAsync(
                operationId,
                SqlitePersistenceFixture.CorpusId,
                SqlitePersistenceFixture.At(6));

        Assert.True(result.AlreadyApplied);
        Assert.Empty(fixture.ContentStore.EnumerateDeletionReservations(operationId));
        await AssertContentReopensAsync(fixture, reserved);
    }

    [Fact]
    public async Task AppliedReplayRestoresReservationSharedWithAnotherCorpus()
    {
        await using var fixture = await CreateInitialisedFixtureAsync();
        var operationId = new OperationId("cleanup-crash-shared-reference");
        var reserved = await CreateAppliedReservationAsync(
            fixture,
            operationId,
            "reserved then referenced by another corpus");
        var otherCorpus = new CorpusId("fixture-corpus-shared");
        await AddDocumentReferenceAsync(
            fixture,
            reserved.ContentObjectId,
            reserved.ByteLength,
            otherCorpus,
            "doc-shared-reference");

        var result = await new SqliteStorageMaintenance(fixture.Options)
            .RunManualCleanupAsync(
                operationId,
                SqlitePersistenceFixture.CorpusId,
                SqlitePersistenceFixture.At(6));

        Assert.True(result.AlreadyApplied);
        Assert.Empty(fixture.ContentStore.EnumerateDeletionReservations(operationId));
        await AssertContentReopensAsync(fixture, reserved);
    }

    [Fact]
    public async Task ReplayRejectsDivergentRequestedAtWithoutReplanning()
    {
        await using var fixture = await CreateInitialisedFixtureAsync();
        var orphan = await PutAndRegisterAsync(fixture, "replay exact orphan");
        var operationId = new OperationId("cleanup-replay-exact");
        var maintenance = new SqliteStorageMaintenance(fixture.Options);
        var first = await maintenance.RunManualCleanupAsync(
            operationId,
            SqlitePersistenceFixture.CorpusId,
            SqlitePersistenceFixture.At(6));
        Assert.False(first.AlreadyApplied);
        Assert.Equal(1, first.RemovedContentObjects);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            maintenance.RunManualCleanupAsync(
                operationId,
                SqlitePersistenceFixture.CorpusId,
                SqlitePersistenceFixture.At(7)));
        Assert.Equal(0, await CountContentRowsAsync(fixture, orphan.ContentObjectId));
    }

    [Fact]
    public async Task CanonicalAndReservationConflictFailsClosedWithoutDeletion()
    {
        await using var fixture = await CreateInitialisedFixtureAsync();
        var operationId = new OperationId("cleanup-canonical-reservation-conflict");
        const string contentText = "canonical and reserved copies";
        var reserved = await CreateAppliedReservationAsync(
            fixture,
            operationId,
            contentText);
        var bytes = Encoding.UTF8.GetBytes(contentText);
        await using var replacement = new MemoryStream(bytes, writable: false);
        var republished = await fixture.ContentStore.PutAsync(
            replacement,
            bytes.Length,
            reserved.ContentObjectId);
        Assert.Equal(reserved.ContentObjectId, republished.ContentObjectId);

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            new SqliteStorageMaintenance(fixture.Options).RunManualCleanupAsync(
                operationId,
                SqlitePersistenceFixture.CorpusId,
                SqlitePersistenceFixture.At(6)));

        Assert.True(File.Exists(reserved.Reservation.ReservationPath));
        Assert.True(File.Exists(reserved.Reservation.SourcePath));
        Assert.Equal(1, await CountContentRowsAsync(fixture, reserved.ContentObjectId));
    }

    [Fact]
    public async Task MissingOrCorruptPhysicalContentFailsClosedAndPreservesRow()
    {
        await using var fixture = await CreateInitialisedFixtureAsync();
        var missing = await PutAndRegisterAsync(fixture, "missing physical object");
        Assert.True(fixture.ContentStore.DeleteIfPresent(missing.ContentObjectId));
        var missingOperation = new OperationId("cleanup-missing-physical-object");

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            new SqliteStorageMaintenance(fixture.Options).RunManualCleanupAsync(
                missingOperation,
                SqlitePersistenceFixture.CorpusId,
                SqlitePersistenceFixture.At(6)));
        Assert.Equal(1, await CountContentRowsAsync(fixture, missing.ContentObjectId));
        Assert.Empty(fixture.ContentStore.EnumerateDeletionReservations(missingOperation));

        var corruptOperation = new OperationId("cleanup-corrupt-reservation");
        var corrupt = await CreateAppliedReservationAsync(
            fixture,
            corruptOperation,
            "reservation corrupted after crash");
        await File.AppendAllTextAsync(corrupt.Reservation.ReservationPath, "corrupt");

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            new SqliteStorageMaintenance(fixture.Options).RunManualCleanupAsync(
                corruptOperation,
                SqlitePersistenceFixture.CorpusId,
                SqlitePersistenceFixture.At(6)));
        Assert.True(File.Exists(corrupt.Reservation.ReservationPath));
        Assert.Equal(1, await CountContentRowsAsync(fixture, corrupt.ContentObjectId));
    }

    [Fact]
    public async Task UnexpectedReservationPathFailsClosedWithoutTouchingKnownReservation()
    {
        await using var fixture = await CreateInitialisedFixtureAsync();
        var operationId = new OperationId("cleanup-unexpected-reservation-path");
        var reserved = await CreateAppliedReservationAsync(
            fixture,
            operationId,
            "reservation beside unexpected path");
        var unexpectedPath = Path.Combine(
            Path.GetDirectoryName(reserved.Reservation.ReservationPath)!,
            "unexpected-entry.tmp");
        await File.WriteAllTextAsync(unexpectedPath, "synthetic invalid reservation entry");

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            new SqliteStorageMaintenance(fixture.Options).RunManualCleanupAsync(
                operationId,
                SqlitePersistenceFixture.CorpusId,
                SqlitePersistenceFixture.At(6)));

        Assert.True(File.Exists(unexpectedPath));
        Assert.True(File.Exists(reserved.Reservation.ReservationPath));
        Assert.Equal(1, await CountContentRowsAsync(fixture, reserved.ContentObjectId));
    }

    [Fact]
    public async Task ConcurrentReferenceCommitIsObservedBeforeAppliedFinalisation()
    {
        await using var fixture = await CreateInitialisedFixtureAsync();
        var operationId = new OperationId("cleanup-concurrent-reference");
        var reserved = await CreateAppliedReservationAsync(
            fixture,
            operationId,
            "reference committed across finalisation window");
        await using var connection = new SqliteConnection(
            $"Data Source={fixture.Options.ControlDatabasePath};Mode=ReadWrite;Cache=Private");
        await connection.OpenAsync();
        await using var transaction = connection.BeginTransaction(deferred: false);
        await InsertDocumentReferenceAsync(
            connection,
            transaction,
            reserved.ContentObjectId,
            reserved.ByteLength,
            SqlitePersistenceFixture.CorpusId,
            "doc-concurrent-reference");
        var cleanupTask = Task.Run(() =>
            new SqliteStorageMaintenance(fixture.Options).RunManualCleanupAsync(
                operationId,
                SqlitePersistenceFixture.CorpusId,
                SqlitePersistenceFixture.At(6)));
        await Task.Delay(TimeSpan.FromMilliseconds(250));
        Assert.False(cleanupTask.IsCompleted);

        await transaction.CommitAsync();
        var result = await cleanupTask;

        Assert.True(result.AlreadyApplied);
        Assert.Empty(fixture.ContentStore.EnumerateDeletionReservations(operationId));
        await AssertContentReopensAsync(fixture, reserved);
    }

    private static async Task<SqlitePersistenceFixture> CreateInitialisedFixtureAsync()
    {
        var fixture = await SqlitePersistenceFixture.CreateAsync();
        _ = await fixture.CommitLocalCatalogueAsync();
        return fixture;
    }

    private static DocumentPageImageRow PageRow(
        string manifestId,
        ContentWriteResult image,
        string sourceContentSha256) =>
        new()
        {
            RenderManifestId = manifestId,
            PageNumber = 1,
            CorpusId = SqlitePersistenceFixture.CorpusId.Value,
            DocumentId = "doc-fixture",
            DocumentVersion = 1,
            SourceContentSha256 = sourceContentSha256,
            RenderProfileId = "pdf-page-png-v1",
            RendererDescriptor = "synthetic-renderer:v1",
            ImageContentSha256 = image.ContentObjectId.Value,
            ImageSha256 = image.ContentObjectId.Value,
            ByteLength = image.ByteLength,
            MediaType = "image/png",
            WidthPixels = 1024,
            HeightPixels = 768,
        };

    private static async Task<ReservedContent> CreateAppliedReservationAsync(
        SqlitePersistenceFixture fixture,
        OperationId operationId,
        string contentText)
    {
        var content = await PutAndRegisterAsync(fixture, contentText);
        await using (var context = fixture.Options.CreateControlContext())
        {
            context.AdminOperations.Add(new AdminOperationRow
            {
                OperationId = operationId.Value,
                CorpusId = SqlitePersistenceFixture.CorpusId.Value,
                OperationKind = "ManualCleanup",
                Status = "Applied",
                ExpectedRevision = null,
                ResultRevision = null,
                RequestedAtUtc = SqlitePersistenceFixture.At(6).ToString(
                    "O",
                    CultureInfo.InvariantCulture),
                CompletedAtUtc = SqlitePersistenceFixture.At(6).ToString(
                    "O",
                    CultureInfo.InvariantCulture),
            });
            await context.SaveChangesAsync();
        }

        var reservation = await fixture.ContentStore.ReserveForDeletionAsync(
            operationId,
            content.ContentObjectId,
            content.ByteLength,
            CancellationToken.None);
        Assert.True(reservation.WasPresent);
        return new ReservedContent(
            content.ContentObjectId,
            content.ByteLength,
            reservation);
    }

    private static async Task AddInProgressOperationAndAuditAsync(
        SqlitePersistenceFixture fixture,
        OperationId operationId,
        string planDigest)
    {
        await using var context = fixture.Options.CreateControlContext();
        context.AdminOperations.Add(new AdminOperationRow
        {
            OperationId = operationId.Value,
            CorpusId = SqlitePersistenceFixture.CorpusId.Value,
            OperationKind = "ManualCleanup",
            Status = "InProgress",
            ExpectedRevision = null,
            ResultRevision = null,
            RequestedAtUtc = SqlitePersistenceFixture.At(6).ToString(
                "O",
                CultureInfo.InvariantCulture),
            CompletedAtUtc = null,
        });
        context.AuditEvents.Add(new AuditEventRow
        {
            AuditEventId = $"audit-{Hash(Encoding.UTF8.GetBytes(
                $"{operationId.Value}\nCleanupPlanned"))}",
            OperationId = operationId.Value,
            CorpusId = SqlitePersistenceFixture.CorpusId.Value,
            EventType = "CleanupPlanned",
            OccurredAtUtc = SqlitePersistenceFixture.At(6).ToString(
                "O",
                CultureInfo.InvariantCulture),
            DetailsDigest = planDigest,
        });
        await context.SaveChangesAsync();
    }

    private static async Task<ContentWriteResult> PutAndRegisterAsync(
        SqlitePersistenceFixture fixture,
        string contentText)
    {
        var bytes = Encoding.UTF8.GetBytes(contentText);
        await using var content = new MemoryStream(bytes, writable: false);
        var result = await fixture.ContentStore.PutAsync(content, bytes.Length);
        await using var context = fixture.Options.CreateControlContext();
        context.ContentObjects.Add(new ContentObjectRow
        {
            ContentSha256 = result.ContentObjectId.Value,
            ByteLength = result.ByteLength,
            RegisteredAtUtc = SqlitePersistenceFixture.At(5).ToString(
                "O",
                CultureInfo.InvariantCulture),
        });
        await context.SaveChangesAsync();
        return result;
    }

    private static async Task AddDocumentReferenceAsync(
        SqlitePersistenceFixture fixture,
        ContentObjectId contentObjectId,
        long byteLength,
        CorpusId corpusId,
        string documentId)
    {
        await using var context = fixture.Options.CreateControlContext();

        if (corpusId != SqlitePersistenceFixture.CorpusId)
        {
            context.Corpora.Add(new CorpusRow
            {
                CorpusId = corpusId.Value,
                CorpusRevision = 1,
                CreatedAtUtc = SqlitePersistenceFixture.At(5).ToString(
                    "O",
                    CultureInfo.InvariantCulture),
            });
            context.DatabaseProductRevisions.Add(new DatabaseProductRevisionRow
            {
                CorpusId = corpusId.Value,
                ProductId = "db-shared",
                ProductRevision = 1,
                DisplayName = "Shared synthetic database",
                Status = "Active",
            });
        }

        context.DocumentVersions.Add(CreateDocumentReference(
            contentObjectId,
            byteLength,
            corpusId,
            documentId));
        await context.SaveChangesAsync();
    }

    private static DocumentVersionRow CreateDocumentReference(
        ContentObjectId contentObjectId,
        long byteLength,
        CorpusId corpusId,
        string documentId) =>
        new()
        {
            CorpusId = corpusId.Value,
            DocumentId = documentId,
            DocumentVersion = 1,
            ProductId = corpusId == SqlitePersistenceFixture.CorpusId
                ? "db-fixture"
                : "db-shared",
            ProductRevision = 1,
            DocumentFormat = "Pdf",
            ContentLanguage = "en-GB",
            ContentSha256 = contentObjectId.Value,
            ByteLength = byteLength,
            MediaType = "application/pdf",
            SourceAdapterId = "local-fixture",
            SourceTrustClass = "LocalAuthorised",
            OfficialRegistrationId = null,
            OfficialSnapshotId = null,
        };

    private static async Task AddOfficialSnapshotReferenceAsync(
        SqlitePersistenceFixture fixture,
        ContentObjectId contentObjectId,
        long byteLength)
    {
        await using var context = fixture.Options.CreateControlContext();
        context.OfficialSourceRegistrations.Add(new OfficialSourceRegistrationRow
        {
            CorpusId = SqlitePersistenceFixture.CorpusId.Value,
            RegistrationId = "registration-crash-reference",
            RegistrationRevision = 1,
            ProductId = "db-fixture",
            DocumentId = "doc-fixture",
            SourceAdapterId = "official-fixture",
            CanonicalHttpsUrl = "https://maintainer.example/reference.pdf",
            Status = "Active",
        });
        context.OfficialSourceSnapshots.Add(new OfficialSourceSnapshotRow
        {
            CorpusId = SqlitePersistenceFixture.CorpusId.Value,
            SnapshotId = "snapshot-crash-reference",
            RegistrationId = "registration-crash-reference",
            RegistrationRevision = 1,
            ContentSha256 = contentObjectId.Value,
            ByteLength = byteLength,
            MediaType = "application/pdf",
            RetrievedAtUtc = SqlitePersistenceFixture.At(5).ToString(
                "O",
                CultureInfo.InvariantCulture),
        });
        await context.SaveChangesAsync();
    }

    private static async Task InsertDocumentReferenceAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        ContentObjectId contentObjectId,
        long byteLength,
        CorpusId corpusId,
        string documentId)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO document_versions(
                corpus_id,
                document_id,
                document_version,
                product_id,
                product_revision,
                document_format,
                content_language,
                content_sha256,
                byte_length,
                media_type,
                source_adapter_id,
                source_trust_class,
                official_registration_id,
                official_snapshot_id)
            VALUES (
                $corpusId,
                $documentId,
                1,
                'db-fixture',
                1,
                'Pdf',
                'en-GB',
                $contentSha256,
                $byteLength,
                'application/pdf',
                'local-fixture',
                'LocalAuthorised',
                NULL,
                NULL);
            """;
        command.Parameters.AddWithValue("$corpusId", corpusId.Value);
        command.Parameters.AddWithValue("$documentId", documentId);
        command.Parameters.AddWithValue("$contentSha256", contentObjectId.Value);
        command.Parameters.AddWithValue("$byteLength", byteLength);
        _ = await command.ExecuteNonQueryAsync();
    }

    private static async Task AssertContentReopensAsync(
        SqlitePersistenceFixture fixture,
        ReservedContent content)
    {
        await using var stream = await fixture.ContentStore.OpenReadAsync(
            content.ContentObjectId);
        Assert.Equal(content.ByteLength, stream.Length);
    }

    private static async Task<long> CountContentRowsAsync(
        SqlitePersistenceFixture fixture,
        ContentObjectId contentObjectId)
    {
        await using var context = fixture.Options.CreateControlContext();
        return await context.ContentObjects.AsNoTracking().LongCountAsync(
            row => row.ContentSha256 == contentObjectId.Value);
    }

    private static string Hash(ReadOnlySpan<byte> value) =>
        Convert.ToHexString(SHA256.HashData(value)).ToLowerInvariant();

    private sealed record ReservedContent(
        ContentObjectId ContentObjectId,
        long ByteLength,
        ContentDeletionReservation Reservation);
}
