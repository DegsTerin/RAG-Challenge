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
            DocumentContentLanguage.EnGb,
            new SourceAdapterId("local-csv"),
            SourceTrustClass.LocalAuthorised);
        var localBytes = SyntheticParserFixtureFactory.CsvValidQuotedUtf8;
        var localFirst = await IngestAsync(ingestion, localBytes, localContext);
        var localReplay = await IngestAsync(ingestion, localBytes, localContext);

        Assert.Equal(ContentObjectWriteOutcome.Published, localFirst.Content.WriteOutcome);
        Assert.Equal(ContentObjectWriteOutcome.AlreadyExisted, localReplay.Content.WriteOutcome);
        Assert.Equal(localFirst.Content.ContentObjectId, localReplay.Content.ContentObjectId);
        Assert.Equal(localFirst.Content.ContentObjectId, localFirst.Content.Sha256);
        Assert.Equal(localBytes.Length, localFirst.Content.ByteLength);
        Assert.Equal(ContentMediaType.TextCsv, localFirst.Content.MediaType);
        Assert.Equal("filesystem-sha256-v1", localFirst.Content.Implementation.Value);
        Assert.Equal(
            ContentVerificationOutcome.Verified,
            localFirst.Content.Verification.WriteVerification);
        Assert.Equal(
            ContentVerificationOutcome.Verified,
            localFirst.Content.Verification.ReopenVerification);
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
            DocumentContentLanguage.EnGb,
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
            DocumentContentLanguage.PtBr,
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
        var replayed = await administration.ApplyAsync(adminRequest with
        {
            AuditContext = Audit(
                "catalogue-ingestion-1",
                "add-document",
                "Register deterministic synthetic documents.",
                SqlitePersistenceFixture.At(9)),
        });

        Assert.Equal(StoreMutationOutcome.Applied, committed.Outcome);
        Assert.Equal(StoreMutationOutcome.AlreadyApplied, replayed.Outcome);
        Assert.Equal(1, await fixture.ScalarAsync("SELECT COUNT(*) FROM audit_events;"));
        var divergentCatalogue = new CatalogueSnapshot(
            SqlitePersistenceFixture.CorpusId,
            new CatalogueRevision(1),
            [category],
            [new DatabaseProduct(
                product.Id,
                product.Revision,
                "Divergent display name",
                product.Status,
                product.CategoryIds)],
            [localDocument, officialCandidate]);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            administration.ApplyAsync(adminRequest with
            {
                ProposedSnapshot = divergentCatalogue,
                AuditContext = Audit(
                    "catalogue-ingestion-1",
                    "add-document",
                    "Register deterministic synthetic documents.",
                    SqlitePersistenceFixture.At(10)),
            }));

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
            DocumentContentLanguage.PtBr,
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

    [Fact]
    public async Task IngestionRejectsAMediaTypeThatDoesNotMatchTheDocumentFormat()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var ingestion = CreateIngestionService(fixture);
        await using var content = new MemoryStream(
            SyntheticParserFixtureFactory.CsvValidQuotedUtf8,
            writable: false);
        var context = new DocumentChunkingContext(
            SqlitePersistenceFixture.CorpusId,
            new DatabaseProductId("db-media-mismatch"),
            new DatabaseProductRevision(1),
            new DocumentId("doc-media-mismatch"),
            new DocumentVersionNumber(1),
            DocumentFormat.Csv,
            DocumentContentLanguage.EnGb,
            new SourceAdapterId("local-media-mismatch"),
            SourceTrustClass.LocalAuthorised);

        var failure = await Assert.ThrowsAsync<DocumentParseException>(
            () => ingestion.IngestAsync(new DocumentIngestionRequest(
                content,
                MaximumByteLength: 131_072,
                ContentMediaType.ApplicationPdf,
                new ParserPolicy(131_072, 32, 131_072, 32, 16_384),
                new ChunkingPolicy(128, 16, 160),
                context)));

        Assert.Equal(DocumentParseFailureKind.UnsupportedFormat, failure.FailureKind);
        Assert.Empty(Directory.EnumerateFiles(
            Path.Combine(fixture.Options.ContentStoreRoot, "objects"),
            "*.bin",
            SearchOption.AllDirectories));
    }

    [Fact]
    public async Task EmptyIngestionFailsCanonicallyWithoutPublishingContent()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var ingestion = CreateIngestionService(fixture);
        var context = new DocumentChunkingContext(
            SqlitePersistenceFixture.CorpusId,
            new DatabaseProductId("db-empty-ingestion"),
            new DatabaseProductRevision(1),
            new DocumentId("doc-empty-ingestion"),
            new DocumentVersionNumber(1),
            DocumentFormat.Csv,
            DocumentContentLanguage.EnGb,
            new SourceAdapterId("local-empty-ingestion"),
            SourceTrustClass.LocalAuthorised);

        var failure = await Assert.ThrowsAsync<DocumentParseException>(
            () => IngestAsync(ingestion, [], context));

        Assert.Equal(DocumentParseFailureKind.NoExtractableText, failure.FailureKind);
        Assert.Empty(Directory.EnumerateFiles(
            Path.Combine(fixture.Options.ContentStoreRoot, "objects"),
            "*.bin",
            SearchOption.AllDirectories));
        Assert.Empty(Directory.EnumerateFiles(
            Path.Combine(fixture.Options.ContentStoreRoot, "quarantine"),
            "*",
            SearchOption.TopDirectoryOnly));
    }

    [Fact]
    public async Task DuplicateBytesReuseContentWithoutErasingDocumentIdentity()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var ingestion = CreateIngestionService(fixture);
        var firstContext = new DocumentChunkingContext(
            SqlitePersistenceFixture.CorpusId,
            new DatabaseProductId("db-shared-content"),
            new DatabaseProductRevision(1),
            new DocumentId("doc-shared-content-a"),
            new DocumentVersionNumber(1),
            DocumentFormat.Csv,
            DocumentContentLanguage.EnGb,
            new SourceAdapterId("local-shared-content"),
            SourceTrustClass.LocalAuthorised);
        var secondContext = firstContext with
        {
            DocumentId = new DocumentId("doc-shared-content-b"),
        };
        var bytes = SyntheticParserFixtureFactory.CsvValidQuotedUtf8;

        var first = await IngestAsync(ingestion, bytes, firstContext);
        var duplicate = await IngestAsync(ingestion, bytes, secondContext);
        var replay = await IngestAsync(ingestion, bytes, secondContext);

        Assert.Equal(ContentObjectWriteOutcome.Published, first.Content.WriteOutcome);
        Assert.Equal(ContentObjectWriteOutcome.AlreadyExisted, duplicate.Content.WriteOutcome);
        Assert.Equal(ContentObjectWriteOutcome.AlreadyExisted, replay.Content.WriteOutcome);
        Assert.Equal(first.Content.ContentObjectId, duplicate.Content.ContentObjectId);
        Assert.NotEqual(
            Assert.Single(first.Chunks).Digest,
            Assert.Single(duplicate.Chunks).Digest);
        Assert.Equal(
            duplicate.Chunks.Select(chunk => chunk.Digest),
            replay.Chunks.Select(chunk => chunk.Digest));
        Assert.Single(Directory.EnumerateFiles(
            Path.Combine(fixture.Options.ContentStoreRoot, "objects"),
            "*.bin",
            SearchOption.AllDirectories));
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
                ContentMediaType.TextCsv,
                new ParserPolicy(131_072, 32, 131_072, 32, 16_384),
                new ChunkingPolicy(128, 16, 160),
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
            new ChunkingPolicy(128, 16, 160),
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
