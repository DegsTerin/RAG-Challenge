// Purpose: Proves unchanged official-source observations and activation rebinding commit atomically, remain idempotent, and fail closed on conflicts or persistence faults.
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using RagChallenge.Application.Administration;
using RagChallenge.Application.Documents;
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;
using RagChallenge.Infrastructure.Documents;
using RagChallenge.Infrastructure.Persistence;

namespace RagChallenge.IntegrationTests;

public sealed class OfficialObservationRebindingTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task NotModifiedOrIdenticalHashRebindsTheActiveObservationAtomically(
        bool notModified)
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var source = await CreateActiveOfficialSourceAsync(fixture);
        var fetch = notModified
            ? new OfficialFetchResult(
                OfficialFetchStatus.NotModified,
                statusCode: 304,
                content: null,
                mediaType: null,
                source.ETag,
                source.LastModified)
            : new OfficialFetchResult(
                OfficialFetchStatus.Changed,
                statusCode: 200,
                source.Content,
                "text/csv",
                source.ETag,
                source.LastModified);
        var transport = new SequenceOfficialSourceTransport(fetch, fetch);
        var service = CreateSynchronisationService(fixture, transport);
        var request = CreateRequest(
            source,
            observationId: $"observation-revalidated-{notModified}",
            operationId: $"observation-rebind-{notModified}",
            expectedJournalRevision: 1,
            expectedActivationRevision: 1,
            at: SqlitePersistenceFixture.At(3));

        var result = await service.SynchroniseAsync(request);
        var replay = await service.SynchroniseAsync(request);

        Assert.Equal(
            OfficialSynchronisationOutcome.UnchangedObservationCreated,
            result.Outcome);
        Assert.Equal(result.Observation.Id, replay.Observation.Id);
        var active = await fixture.ControlStore.ReadActiveActivationAsync(
            SqlitePersistenceFixture.CorpusId);
        Assert.NotNull(active);
        Assert.Equal(2, active.RecordRevision.Value);
        Assert.Equal(source.InitialActivation.RecordRevision, active.PreviousRecordRevision);
        Assert.Equal(source.Manifest.IndexGenerationId, active.IndexGenerationId);
        Assert.Equal(source.Manifest.CatalogueRevision, active.CatalogueRevision);
        Assert.Equal(
            source.InitialActivation.GenerationActivatedAt,
            active.GenerationActivatedAt);
        Assert.Equal(SqlitePersistenceFixture.At(3), active.RecordUpdatedAt);
        Assert.NotEqual(
            source.InitialActivation.ActivationBindingSetDigest,
            active.ActivationBindingSetDigest);
        Assert.Equal(
            request.ObservationId,
            active.DocumentBindings.Single(binding =>
                binding.DocumentId == source.OfficialContext.DocumentId)
                .SourceObservationId);

        var query = await new SqliteQueryActivationReader(fixture.Options).ReadAsync(
            SqlitePersistenceFixture.CorpusId,
            SqlitePersistenceFixture.At(3));
        Assert.NotNull(query);
        var officialEvidence = query.EvidenceBindings.Single(binding =>
            binding.Binding.DocumentId == source.OfficialContext.DocumentId);
        Assert.Equal(request.ObservationId, officialEvidence.Binding.SourceObservationId);
        Assert.Equal(SourceFreshness.Current, officialEvidence.Freshness);
        Assert.Equal(2, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM source_observations;"));
        Assert.Equal(2, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM activation_records;"));
        Assert.Equal(1, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM generation_manifests;"));
        Assert.Equal(1, await fixture.ScalarAsync(
            $"SELECT COUNT(*) FROM audit_events WHERE operation_id = '{request.ObservationAuditContext.OperationId.Value}';"));
    }

    [Fact]
    public async Task WithdrawalRebindsWhileAnotherActiveDocumentKeepsTheDatabaseEligible()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var source = await CreateActiveOfficialSourceAsync(fixture);
        var transport = new SequenceOfficialSourceTransport(
            new OfficialFetchResult(
                OfficialFetchStatus.Withdrawn,
                statusCode: 410,
                content: null,
                mediaType: null,
                etag: null,
                lastModified: null));
        var service = CreateSynchronisationService(fixture, transport);
        var request = CreateRequest(
            source,
            observationId: "observation-withdrawn",
            operationId: "observation-rebind-withdrawn",
            expectedJournalRevision: 1,
            expectedActivationRevision: 1,
            at: SqlitePersistenceFixture.At(3));

        var result = await service.SynchroniseAsync(request);

        Assert.Equal(
            OfficialSynchronisationOutcome.WithdrawnObservationCreated,
            result.Outcome);
        var query = await new SqliteQueryActivationReader(fixture.Options).ReadAsync(
            SqlitePersistenceFixture.CorpusId,
            SqlitePersistenceFixture.At(3));
        Assert.NotNull(query);
        Assert.Equal(2, query.ActivationRecord.RecordRevision.Value);
        Assert.Equal(
            SourceFreshness.Withdrawn,
            query.EvidenceBindings.Single(binding =>
                binding.Binding.DocumentId == source.OfficialContext.DocumentId).Freshness);
        Assert.Contains(query.EvidenceBindings, binding =>
            binding.Binding.SourceTrustClass == SourceTrustClass.LocalAuthorised &&
            binding.IsEligible);
    }

    [Fact]
    public async Task SnapshotMismatchRollsBackTheObservationBeforeActivationChanges()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var source = await CreateActiveOfficialSourceAsync(fixture);
        var registration = new OfficialSourceRegistration(
            source.Registration.Id,
            new SourceRegistrationRevision(2),
            source.Registration.DatabaseProductId,
            source.Registration.DocumentId,
            source.Registration.SourceAdapterId,
            source.Registration.CanonicalHttpsUrl,
            CatalogueItemStatus.Active);
        var snapshot = new OfficialSourceSnapshot(
            new OfficialSnapshotId("snapshot-not-active"),
            registration.Id,
            source.Snapshot.ContentObjectId,
            source.Snapshot.ByteLength,
            source.Snapshot.MediaType,
            SqlitePersistenceFixture.At(3));
        var commit = await fixture.ControlStore.CommitOfficialSourceAsync(
            new OfficialSourceCommitRequest(
                new OperationId("source-second-snapshot"),
                SqlitePersistenceFixture.CorpusId,
                registration,
                snapshot,
                SqlitePersistenceFixture.At(3)));
        Assert.Equal(StoreMutationOutcome.Applied, commit.Outcome);
        var mismatched = source with { Registration = registration, Snapshot = snapshot };
        var service = CreateSynchronisationService(
            fixture,
            new SequenceOfficialSourceTransport(
                new OfficialFetchResult(
                    OfficialFetchStatus.NotModified,
                    statusCode: 304,
                    content: null,
                    mediaType: null,
                    source.ETag,
                    source.LastModified)));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SynchroniseAsync(CreateRequest(
                mismatched,
                observationId: "observation-mismatch",
                operationId: "observation-rebind-mismatch",
                expectedJournalRevision: 1,
                expectedActivationRevision: 1,
                at: SqlitePersistenceFixture.At(3))));

        Assert.Equal(1, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM source_observations;"));
        Assert.Equal(1, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM activation_records;"));
        Assert.Equal(1, await fixture.ScalarAsync(
            "SELECT journal_revision FROM observation_journal_heads;"));
        Assert.Equal(1, await fixture.ScalarAsync(
            "SELECT record_revision FROM activation_heads;"));
    }

    [Theory]
    [InlineData("activation_records")]
    [InlineData("audit_events")]
    public async Task PersistenceFaultRollsBackObservationJournalActivationAndAudit(
        string failingTable)
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var source = await CreateActiveOfficialSourceAsync(fixture);
        var operationId = $"observation-rebind-fault-{failingTable}";
        await CreateFailureTriggerAsync(fixture, failingTable, operationId);
        var service = CreateSynchronisationService(
            fixture,
            new SequenceOfficialSourceTransport(
                new OfficialFetchResult(
                    OfficialFetchStatus.NotModified,
                    statusCode: 304,
                    content: null,
                    mediaType: null,
                    source.ETag,
                    source.LastModified)));

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            service.SynchroniseAsync(CreateRequest(
                source,
                observationId: $"observation-fault-{failingTable}",
                operationId,
                expectedJournalRevision: 1,
                expectedActivationRevision: 1,
                at: SqlitePersistenceFixture.At(3))));

        Assert.Equal(1, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM source_observations;"));
        Assert.Equal(1, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM activation_records;"));
        Assert.Equal(1, await fixture.ScalarAsync(
            "SELECT journal_revision FROM observation_journal_heads;"));
        Assert.Equal(1, await fixture.ScalarAsync(
            "SELECT record_revision FROM activation_heads;"));
        Assert.Equal(0, await fixture.ScalarAsync(
            $"SELECT COUNT(*) FROM admin_operations WHERE operation_id = '{operationId}';"));
        Assert.Equal(0, await fixture.ScalarAsync(
            $"SELECT COUNT(*) FROM audit_events WHERE operation_id = '{operationId}';"));
    }

    [Fact]
    public async Task ActivationRevisionConflictDoesNotAppendTheObservation()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var source = await CreateActiveOfficialSourceAsync(fixture);
        var service = CreateSynchronisationService(
            fixture,
            new SequenceOfficialSourceTransport(
                new OfficialFetchResult(
                    OfficialFetchStatus.NotModified,
                    statusCode: 304,
                    content: null,
                    mediaType: null,
                    source.ETag,
                    source.LastModified)));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SynchroniseAsync(CreateRequest(
                source,
                observationId: "observation-conflict",
                operationId: "observation-rebind-conflict",
                expectedJournalRevision: 1,
                expectedActivationRevision: 0,
                at: SqlitePersistenceFixture.At(3))));

        Assert.Equal(1, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM source_observations;"));
        Assert.Equal(1, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM activation_records;"));
    }

    private static OfficialSourceSynchronisationService CreateSynchronisationService(
        SqlitePersistenceFixture fixture,
        IOfficialSourceTransport transport) =>
        new(
            transport,
            fixture.ControlStore,
            new DocumentIngestionService(
                fixture.ContentStore,
                [new PdfPigDocumentParser(), new CsvHelperDocumentParser()],
                new DeterministicChunkingStrategy()));

    private static OfficialSynchronisationRequest CreateRequest(
        ActiveOfficialSource source,
        string observationId,
        string operationId,
        long expectedJournalRevision,
        long expectedActivationRevision,
        DateTimeOffset at) =>
        new(
            SqlitePersistenceFixture.CorpusId,
            source.Registration,
            DocumentFormat.Csv,
            source.OfficialContext,
            new ParserPolicy(131_072, 32, 131_072, 32, 16_384),
            new ChunkingPolicy(128, 16, 160),
            MaximumByteLength: 131_072,
            Audit($"unused-snapshot-{operationId}", at),
            Audit(operationId, at),
            expectedJournalRevision,
            new OfficialObservationId(observationId),
            TimeSpan.FromDays(7),
            source.Snapshot,
            source.ETag,
            source.LastModified,
            expectedActivationRevision);

    private static async Task<ActiveOfficialSource> CreateActiveOfficialSourceAsync(
        SqlitePersistenceFixture fixture)
    {
        var officialBytes = SyntheticParserFixtureFactory.CsvValidQuotedUtf8;
        await using var officialStream = new MemoryStream(officialBytes, writable: false);
        var officialContent = await fixture.ContentStore.PutAsync(
            officialStream,
            officialBytes.Length);
        var localBytes = "local fallback"u8.ToArray();
        await using var localStream = new MemoryStream(localBytes, writable: false);
        var localContent = await fixture.ContentStore.PutAsync(localStream, localBytes.Length);
        var productId = new DatabaseProductId("db-observation-rebind");
        var productRevision = new DatabaseProductRevision(1);
        var officialDocumentId = new DocumentId("doc-official-rebind");
        var localDocumentId = new DocumentId("doc-local-fallback");
        var documentVersion = new DocumentVersionNumber(1);
        var adapterId = new SourceAdapterId("official-fake-csv");
        var registrationId = new OfficialSourceRegistrationId("source-rebind");
        var snapshotId = new OfficialSnapshotId("snapshot-rebind");
        var category = new DatabaseCategory(
            new DatabaseCategoryId("category-rebind"),
            "Synthetic database");
        var product = new DatabaseProduct(
            productId,
            productRevision,
            "Observation Rebind Database",
            CatalogueItemStatus.Active,
            [category.Id]);
        var officialDocument = new DocumentVersion(
            officialDocumentId,
            documentVersion,
            productId,
            productRevision,
            DocumentFormat.Csv,
            SupportedLanguage.EnGb,
            CatalogueItemStatus.Active,
            officialContent.ContentObjectId,
            officialContent.ByteLength,
            "text/csv",
            adapterId,
            SourceTrustClass.OfficialExternal,
            registrationId,
            snapshotId);
        var localDocument = new DocumentVersion(
            localDocumentId,
            documentVersion,
            productId,
            productRevision,
            DocumentFormat.Csv,
            SupportedLanguage.EnGb,
            CatalogueItemStatus.Active,
            localContent.ContentObjectId,
            localContent.ByteLength,
            "text/csv",
            new SourceAdapterId("local-fallback"),
            SourceTrustClass.LocalAuthorised);
        var catalogue = new CatalogueSnapshot(
            SqlitePersistenceFixture.CorpusId,
            new CatalogueRevision(1),
            [category],
            [product],
            [officialDocument, localDocument]);
        var catalogueCommit = await fixture.ControlStore.CommitCatalogueAsync(
            new CatalogueCommitRequest(
                new OperationId("catalogue-observation-rebind"),
                catalogue,
                ExpectedCurrentRevision: 0,
                SqlitePersistenceFixture.At(1)));
        Assert.Equal(StoreMutationOutcome.Applied, catalogueCommit.Outcome);

        var registration = new OfficialSourceRegistration(
            registrationId,
            new SourceRegistrationRevision(1),
            productId,
            officialDocumentId,
            adapterId,
            "https://official.invalid/rebind.csv",
            CatalogueItemStatus.Active);
        var snapshot = new OfficialSourceSnapshot(
            snapshotId,
            registrationId,
            officialContent.ContentObjectId,
            officialContent.ByteLength,
            "text/csv",
            SqlitePersistenceFixture.At(1));
        var sourceCommit = await fixture.ControlStore.CommitOfficialSourceAsync(
            new OfficialSourceCommitRequest(
                new OperationId("source-observation-rebind"),
                SqlitePersistenceFixture.CorpusId,
                registration,
                snapshot,
                SqlitePersistenceFixture.At(1)));
        Assert.Equal(StoreMutationOutcome.Applied, sourceCommit.Outcome);

        var initialObservation = new OfficialSourceObservation(
            new OfficialObservationId("observation-initial"),
            registrationId,
            snapshotId,
            new ObservationJournalRevision(1),
            OfficialObservationState.Current,
            SqlitePersistenceFixture.At(1),
            TimeSpan.FromDays(7));
        var observationCommit = await fixture.ControlStore.AppendObservationAsync(
            new ObservationCommitRequest(
                new OperationId("observation-initial"),
                SqlitePersistenceFixture.CorpusId,
                initialObservation,
                ExpectedJournalRevision: 0,
                SqlitePersistenceFixture.At(1)));
        Assert.Equal(StoreMutationOutcome.Applied, observationCommit.Outcome);

        var officialBinding = new DocumentBinding(
            productId,
            productRevision,
            officialDocumentId,
            documentVersion,
            DocumentFormat.Csv,
            adapterId,
            SourceTrustClass.OfficialExternal,
            registrationId,
            snapshotId,
            initialObservation.Id);
        var localBinding = new DocumentBinding(
            productId,
            productRevision,
            localDocumentId,
            documentVersion,
            DocumentFormat.Csv,
            localDocument.SourceAdapterId,
            SourceTrustClass.LocalAuthorised);
        var bindings = new[] { officialBinding, localBinding };
        var manifest = await CommitGenerationAsync(fixture, bindings);
        var initialActivation = ActivationRecordFactory.CreateInitial(
            manifest,
            bindings,
            SqlitePersistenceFixture.At(2));
        var activation = await fixture.ControlStore.CompareExchangeActivationAsync(
            new ActivationCompareExchangeRequest(
                new OperationId("activation-observation-rebind"),
                ActivationMutationKind.Initial,
                ExpectedCurrentRevision: 0,
                initialActivation,
                SqlitePersistenceFixture.CompatibilityKey,
                SqlitePersistenceFixture.At(2),
                SqliteControlPlaneStore.MinimumPreviousGenerationRetention));
        Assert.Equal(StoreMutationOutcome.Applied, activation.Outcome);

        return new ActiveOfficialSource(
            registration,
            snapshot,
            new DocumentChunkingContext(
                SqlitePersistenceFixture.CorpusId,
                productId,
                productRevision,
                officialDocumentId,
                documentVersion,
                DocumentFormat.Csv,
                SupportedLanguage.EnGb,
                adapterId,
                SourceTrustClass.OfficialExternal),
            manifest,
            initialActivation,
            officialBytes,
            "\"rebind-v1\"",
            SqlitePersistenceFixture.At(1));
    }

    private static async Task<FinalisedIndexGenerationManifest> CommitGenerationAsync(
        SqlitePersistenceFixture fixture,
        DocumentBinding[] bindings)
    {
        var candidateId = new CandidateBuildId("candidate-observation-rebind");
        await fixture.VectorStore.CreateCandidateAsync(
            candidateId,
            SqlitePersistenceFixture.CorpusId,
            SqlitePersistenceFixture.CompatibilityKey,
            vectorDimensions: 3,
            expectedChunkCount: bindings.Length,
            SqlitePersistenceFixture.At(2));
        var ordinal = 0L;

        foreach (var binding in bindings)
        {
            await fixture.VectorStore.AddChunksAsync(
                candidateId,
                [new VectorChunkWrite(
                    ordinal,
                    binding.DocumentId,
                    binding.DocumentVersion,
                    new LogicalArtifactDigest(
                        SqlitePersistenceFixture.Hash($"rebind:{ordinal}")),
                    $"synthetic rebind chunk {ordinal}",
                    new float[] { 1, ordinal + 1, 2 })]);
            ordinal++;
        }

        var specification = new IndexGenerationSpecification(
            manifestSchemaVersion: 1,
            SqlitePersistenceFixture.CorpusId,
            new CorpusRevision(1),
            new CatalogueRevision(1),
            BindingDigestCanonicalizer.CanonicaliseActiveDocumentSet(bindings).Digest,
            BindingDigestCanonicalizer.CanonicaliseSourceBindingSet(bindings).Digest,
            SqlitePersistenceFixture.CompatibilityKey);
        var manifest = await fixture.VectorStore.FinaliseCandidateAsync(
            candidateId,
            specification,
            SqlitePersistenceFixture.At(2));
        var commit = await fixture.ControlStore.CommitGenerationAsync(
            new GenerationCommitRequest(
                new OperationId("generation-observation-rebind"),
                candidateId,
                manifest,
                bindings,
                SqlitePersistenceFixture.At(2)));
        Assert.Equal(StoreMutationOutcome.Applied, commit.Outcome);
        return manifest;
    }

    private static AdministrativeAuditContext Audit(
        string operationId,
        DateTimeOffset at) =>
        new(
            new OperationId(operationId),
            "integration-test",
            "synchronise-official",
            "Verify transactional official observation rebinding.",
            at);

    private static async Task CreateFailureTriggerAsync(
        SqlitePersistenceFixture fixture,
        string table,
        string operationId)
    {
        var condition = table == "activation_records"
            ? "NEW.mutation_kind = 'ObservationRebind'"
            : $"NEW.operation_id = '{operationId}'";
        var triggerName = $"fail_{table}";
        await using var connection = new SqliteConnection(
            $"Data Source={fixture.Options.ControlDatabasePath};Mode=ReadWrite;Cache=Private");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            CREATE TRIGGER {triggerName}
            BEFORE INSERT ON {table}
            WHEN {condition}
            BEGIN
                SELECT RAISE(ABORT, 'synthetic observation-rebind fault');
            END;
            """;
        _ = await command.ExecuteNonQueryAsync();
    }

    private sealed record ActiveOfficialSource(
        OfficialSourceRegistration Registration,
        OfficialSourceSnapshot Snapshot,
        DocumentChunkingContext OfficialContext,
        FinalisedIndexGenerationManifest Manifest,
        CorpusActivationRecord InitialActivation,
        byte[] Content,
        string ETag,
        DateTimeOffset LastModified);

    private sealed class SequenceOfficialSourceTransport(
        params OfficialFetchResult[] responses) : IOfficialSourceTransport
    {
        private readonly Queue<OfficialFetchResult> responses = new(responses);

        public Task<OfficialFetchResult> FetchAsync(
            OfficialSourceRegistration registration,
            OfficialFetchPolicy policy,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (responses.Count == 0)
            {
                throw new InvalidOperationException("The fake source has no response.");
            }

            return Task.FromResult(responses.Dequeue());
        }
    }
}
