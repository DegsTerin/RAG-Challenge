// Purpose: Verifies S04-A administration, local ingestion, fake official synchronisation, immutable snapshots, deterministic chunks and idempotent persistence.
using System.Text;

using RagChallenge.Application.Administration;
using RagChallenge.Application.Documents;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Infrastructure.Documents;

namespace RagChallenge.IntegrationTests;

public sealed class BackendIngestionWorkflowTests
{
    [Fact]
    public async Task SyntheticLocalAndOfficialDocumentsRemainTraceableAndIdempotent()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var ingestion = CreateIngestionService(fixture);
        var productId = new DatabaseProductId("db-ingestion");
        var productRevision = new DatabaseProductRevision(1);
        var localDocumentId = new DocumentId("doc-local-csv");
        var officialDocumentId = new DocumentId("doc-official-csv");
        var registrationId = new OfficialSourceRegistrationId("source-official-csv");
        var localContext = new DocumentChunkingContext(
            SqlitePersistenceFixture.CorpusId,
            productId,
            productRevision,
            localDocumentId,
            new DocumentVersionNumber(1),
            DocumentFormat.Csv,
            SupportedLanguage.EnGb,
            new SourceAdapterId("local-csv"),
            SourceTrustClass.LocalAuthorised);
        var localBytes = SyntheticParserFixtureFactory.CsvValidQuotedUtf8;
        var localFirst = await IngestAsync(ingestion, localBytes, localContext);
        var localReplay = await IngestAsync(ingestion, localBytes, localContext);

        Assert.False(localFirst.Content.AlreadyExisted);
        Assert.True(localReplay.Content.AlreadyExisted);
        Assert.Equal(localFirst.Content.ContentObjectId, localReplay.Content.ContentObjectId);
        Assert.Equal(
            localFirst.Chunks.Select(chunk => chunk.Digest),
            localReplay.Chunks.Select(chunk => chunk.Digest));
        Assert.Equal(1, Assert.Single(localFirst.Chunks).RecordNumber);

        var category = new DatabaseCategory(
            new DatabaseCategoryId("category-synthetic"),
            "Synthetic databases");
        var product = new DatabaseProduct(
            productId,
            productRevision,
            "Synthetic Database",
            CatalogueItemStatus.Active,
            [category.Id]);
        var localDocument = new DocumentVersion(
            localDocumentId,
            new DocumentVersionNumber(1),
            productId,
            productRevision,
            DocumentFormat.Csv,
            SupportedLanguage.EnGb,
            CatalogueItemStatus.Active,
            localFirst.Content.ContentObjectId,
            localFirst.Content.ByteLength,
            "text/csv",
            localContext.SourceAdapterId,
            SourceTrustClass.LocalAuthorised);
        var officialAdapterId = new SourceAdapterId("official-fake-csv");
        var officialCandidate = new DocumentVersion(
            officialDocumentId,
            new DocumentVersionNumber(1),
            productId,
            productRevision,
            DocumentFormat.Csv,
            SupportedLanguage.PtBr,
            CatalogueItemStatus.Candidate,
            localFirst.Content.ContentObjectId,
            localFirst.Content.ByteLength,
            "text/csv",
            new SourceAdapterId("local-staged-csv"),
            SourceTrustClass.LocalAuthorised);
        var catalogue = new CatalogueSnapshot(
            SqlitePersistenceFixture.CorpusId,
            new CatalogueRevision(1),
            [category],
            [product],
            [localDocument, officialCandidate]);
        var administration = new CatalogueAdministrationService(fixture.ControlStore);
        var adminContext = Audit(
            "catalogue-ingestion-1",
            "add-document",
            "Register deterministic synthetic documents.",
            SqlitePersistenceFixture.At(1));
        var adminRequest = new CatalogueAdministrationRequest(
            catalogue,
            ExpectedCurrentRevision: 0,
            adminContext);

        var committed = await administration.ApplyAsync(adminRequest);
        var replayed = await administration.ApplyAsync(adminRequest);

        Assert.Equal(StoreMutationOutcome.Applied, committed.Outcome);
        Assert.Equal(StoreMutationOutcome.AlreadyApplied, replayed.Outcome);
        Assert.Equal(1, await fixture.ScalarAsync("SELECT COUNT(*) FROM audit_events;"));

        var registration = new OfficialSourceRegistration(
            registrationId,
            new SourceRegistrationRevision(1),
            productId,
            officialDocumentId,
            officialAdapterId,
            "https://official.invalid/synthetic.csv",
            CatalogueItemStatus.Candidate);
        var officialBytes = Encoding.UTF8.GetBytes(
            "feature,description\r\nÍndice,\"consulta, estável\"\r\n");
        var fakeServer = new FakeOfficialSourceTransport(
            new OfficialFetchResult(
                OfficialFetchStatus.Changed,
                statusCode: 200,
                officialBytes,
                "text/csv",
                "\"synthetic-v1\"",
                SqlitePersistenceFixture.At(2)),
            new OfficialFetchResult(
                OfficialFetchStatus.Changed,
                statusCode: 200,
                officialBytes,
                "text/csv",
                "\"synthetic-v1\"",
                SqlitePersistenceFixture.At(2)),
            new OfficialFetchResult(
                OfficialFetchStatus.Changed,
                statusCode: 200,
                SyntheticParserFixtureFactory.CsvMalformedQuote,
                "text/csv",
                "\"synthetic-broken\"",
                SqlitePersistenceFixture.At(4)));
        var synchroniser = new OfficialSourceSynchronisationService(
            fakeServer,
            fixture.ControlStore,
            ingestion);
        var officialContext = new DocumentChunkingContext(
            SqlitePersistenceFixture.CorpusId,
            productId,
            productRevision,
            officialDocumentId,
            new DocumentVersionNumber(1),
            DocumentFormat.Csv,
            SupportedLanguage.PtBr,
            officialAdapterId,
            SourceTrustClass.OfficialExternal);
        var firstSync = await synchroniser.SynchroniseAsync(
            CreateSyncRequest(
                registration,
                officialContext,
                expectedJournalRevision: 0,
                observationId: "observation-1",
                snapshotOperationId: "snapshot-operation-1",
                observationOperationId: "observation-operation-1",
                at: SqlitePersistenceFixture.At(2)));

        Assert.Equal(
            OfficialSynchronisationOutcome.SnapshotCreatedRebuildRequired,
            firstSync.Outcome);
        Assert.NotEmpty(firstSync.Chunks);
        Assert.Equal("Índice", firstSync.Chunks[0].Columns["feature"]);

        var secondSync = await synchroniser.SynchroniseAsync(
            CreateSyncRequest(
                registration,
                officialContext,
                expectedJournalRevision: 1,
                observationId: "observation-2",
                snapshotOperationId: "snapshot-operation-2",
                observationOperationId: "observation-operation-2",
                at: SqlitePersistenceFixture.At(3),
                firstSync.Snapshot,
                firstSync.ETag,
                firstSync.LastModified));

        Assert.Equal(
            OfficialSynchronisationOutcome.UnchangedObservationCreated,
            secondSync.Outcome);
        Assert.Equal(firstSync.Snapshot.Id, secondSync.Snapshot.Id);
        Assert.Equal(1, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM official_source_snapshots;"));
        Assert.Equal(2, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM source_observations;"));

        var failure = await Assert.ThrowsAsync<DocumentParseException>(
            () => synchroniser.SynchroniseAsync(
                CreateSyncRequest(
                    registration,
                    officialContext,
                    expectedJournalRevision: 2,
                    observationId: "observation-3",
                    snapshotOperationId: "snapshot-operation-3",
                    observationOperationId: "observation-operation-3",
                    at: SqlitePersistenceFixture.At(4),
                    secondSync.Snapshot,
                    secondSync.ETag,
                    secondSync.LastModified)));
        Assert.Equal(DocumentParseFailureKind.MalformedContent, failure.FailureKind);
        Assert.Equal(1, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM official_source_snapshots;"));
        Assert.Equal(2, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM source_observations;"));
        Assert.Equal(3, fakeServer.Requests.Count);
        Assert.All(
            fakeServer.Requests,
            request => Assert.Equal(registration.Id, request.RegistrationId));
    }

    private static DocumentIngestionService CreateIngestionService(
        SqlitePersistenceFixture fixture) =>
        new(
            fixture.ContentStore,
            [new PdfPigDocumentParser(), new CsvHelperDocumentParser()],
            new DeterministicChunkingStrategy());

    private static async Task<DocumentIngestionResult> IngestAsync(
        DocumentIngestionService service,
        byte[] bytes,
        DocumentChunkingContext context)
    {
        await using var content = new MemoryStream(bytes, writable: false);
        return await service.IngestAsync(
            new DocumentIngestionRequest(
                content,
                MaximumByteLength: 131_072,
                new ParserPolicy(131_072, 32, 131_072, 32, 16_384),
                new ChunkingPolicy(128),
                context));
    }

    private static OfficialSynchronisationRequest CreateSyncRequest(
        OfficialSourceRegistration registration,
        DocumentChunkingContext context,
        long expectedJournalRevision,
        string observationId,
        string snapshotOperationId,
        string observationOperationId,
        DateTimeOffset at,
        OfficialSourceSnapshot? currentSnapshot = null,
        string? currentEtag = null,
        DateTimeOffset? currentLastModified = null) =>
        new(
            SqlitePersistenceFixture.CorpusId,
            registration,
            DocumentFormat.Csv,
            context,
            new ParserPolicy(131_072, 32, 131_072, 32, 16_384),
            new ChunkingPolicy(128),
            MaximumByteLength: 131_072,
            Audit(snapshotOperationId, "synchronise-official", "Create snapshot.", at),
            Audit(observationOperationId, "synchronise-official", "Append observation.", at),
            expectedJournalRevision,
            new OfficialObservationId(observationId),
            TimeSpan.FromDays(7),
            currentSnapshot,
            currentEtag,
            currentLastModified);

    private static AdministrativeAuditContext Audit(
        string operationId,
        string command,
        string reason,
        DateTimeOffset at) =>
        new(new OperationId(operationId), "test-operator", command, reason, at);

    private sealed class FakeOfficialSourceTransport(
        params OfficialFetchResult[] responses) : IOfficialSourceTransport
    {
        private readonly Queue<OfficialFetchResult> responses = new(responses);

        internal List<FakeRequest> Requests { get; } = [];

        public Task<OfficialFetchResult> FetchAsync(
            OfficialSourceRegistration registration,
            OfficialFetchPolicy policy,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (responses.Count == 0)
            {
                throw new InvalidOperationException("The fake server has no response.");
            }

            Requests.Add(new FakeRequest(
                registration.Id,
                registration.CanonicalHttpsUrl,
                policy.MaximumByteLength,
                policy.IfNoneMatch,
                policy.IfModifiedSince));
            return Task.FromResult(responses.Dequeue());
        }
    }

    private sealed record FakeRequest(
        OfficialSourceRegistrationId RegistrationId,
        string CanonicalUrl,
        long MaximumByteLength,
        string? IfNoneMatch,
        DateTimeOffset? IfModifiedSince);
}
