// Purpose: Verifies that the authorised integration synchronisation uses only a bounded fake HTTP listener on loopback while preserving official-source provenance and immutable content.
using System.Net;
using System.Net.Http.Headers;
using System.Text;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using RagChallenge.Application.Administration;
using RagChallenge.Application.Documents;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Infrastructure.Documents;

namespace RagChallenge.IntegrationTests;

public sealed class OfficialSourceLoopbackTests
{
    [Fact]
    public async Task OfficialSynchronisationUsesOnlyFakeLoopbackHttpServer()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var serverRoot = Path.Combine(
            Path.GetTempPath(),
            "rag-challenge-s06-a-official",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(serverRoot);
        var bytes = Encoding.UTF8.GetBytes(
            "feature,description\nloopback,\"Synthetic official fixture\"\n");
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = [],
            ContentRootPath = serverRoot,
            EnvironmentName = "Integration",
        });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        await using var server = builder.Build();
        server.MapGet("/synthetic.csv", async context =>
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "text/csv";
            context.Response.ContentLength = bytes.Length;
            context.Response.Headers.ETag = "\"s06-loopback-v1\"";
            context.Response.Headers.LastModified =
                SqlitePersistenceFixture.At(2).ToString("R", System.Globalization.CultureInfo.InvariantCulture);
            await context.Response.Body.WriteAsync(bytes, context.RequestAborted);
        });

        try
        {
            await server.StartAsync();
            var address = new Uri(Assert.Single(server.Services
                .GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()!
                .Addresses));
            Assert.True(IPAddress.IsLoopback(IPAddress.Parse(address.Host)));
            Assert.Equal(Uri.UriSchemeHttp, address.Scheme);

            using var handler = new SocketsHttpHandler
            {
                AllowAutoRedirect = false,
                UseProxy = false,
            };
            using var client = new HttpClient(handler)
            {
                BaseAddress = address,
                Timeout = TimeSpan.FromSeconds(5),
            };
            var transport = new LoopbackOfficialTransport(client);
            var ingestion = new DocumentIngestionService(
                fixture.ContentStore,
                [new PdfPigDocumentParser(), new CsvHelperDocumentParser()],
                new DeterministicChunkingStrategy());
            var productId = new DatabaseProductId("db-s06-loopback");
            var productRevision = new DatabaseProductRevision(1);
            var localDocumentId = new DocumentId("doc-s06-local-prerequisite");
            var documentId = new DocumentId("doc-s06-official-loopback");
            var documentVersion = new DocumentVersionNumber(1);
            var adapterId = new SourceAdapterId("official-s06-loopback");
            await using var localStream = new MemoryStream(bytes, writable: false);
            var localIngestion = await ingestion.IngestAsync(new DocumentIngestionRequest(
                localStream,
                MaximumByteLength: 131_072,
                new ParserPolicy(131_072, 32, 131_072, 32, 16_384),
                new ChunkingPolicy(128, 16, 160),
                new DocumentChunkingContext(
                    SqlitePersistenceFixture.CorpusId,
                    productId,
                    productRevision,
                    localDocumentId,
                    documentVersion,
                    DocumentFormat.Csv,
                    SupportedLanguage.EnGb,
                    new SourceAdapterId("local-s06-prerequisite"),
                    SourceTrustClass.LocalAuthorised)));
            var category = new DatabaseCategory(
                new DatabaseCategoryId("category-s06-loopback"),
                "Synthetic integration sources");
            var product = new DatabaseProduct(
                productId,
                productRevision,
                "Synthetic loopback database",
                CatalogueItemStatus.Active,
                [category.Id]);
            var localDocument = new DocumentVersion(
                localDocumentId,
                documentVersion,
                productId,
                productRevision,
                DocumentFormat.Csv,
                SupportedLanguage.EnGb,
                CatalogueItemStatus.Active,
                localIngestion.Content.ContentObjectId,
                localIngestion.Content.ByteLength,
                "text/csv",
                new SourceAdapterId("local-s06-prerequisite"),
                SourceTrustClass.LocalAuthorised);
            var officialCandidate = new DocumentVersion(
                documentId,
                documentVersion,
                productId,
                productRevision,
                DocumentFormat.Csv,
                SupportedLanguage.EnGb,
                CatalogueItemStatus.Candidate,
                localIngestion.Content.ContentObjectId,
                localIngestion.Content.ByteLength,
                "text/csv",
                new SourceAdapterId("local-s06-staging"),
                SourceTrustClass.LocalAuthorised);
            var catalogueCommit = await new CatalogueAdministrationService(fixture.ControlStore)
                .ApplyAsync(new CatalogueAdministrationRequest(
                    new CatalogueSnapshot(
                        SqlitePersistenceFixture.CorpusId,
                        new CatalogueRevision(1),
                        [category],
                        [product],
                        [localDocument, officialCandidate]),
                    ExpectedCurrentRevision: 0,
                    Audit("s06-loopback-catalogue", SqlitePersistenceFixture.At(1))));
            Assert.Equal(StoreMutationOutcome.Applied, catalogueCommit.Outcome);
            var synchroniser = new OfficialSourceSynchronisationService(
                transport,
                fixture.ControlStore,
                ingestion);
            var registration = new OfficialSourceRegistration(
                new OfficialSourceRegistrationId("registration-s06-loopback"),
                new SourceRegistrationRevision(1),
                productId,
                documentId,
                adapterId,
                "https://official.invalid/synthetic.csv",
                CatalogueItemStatus.Candidate);
            var result = await synchroniser.SynchroniseAsync(
                new OfficialSynchronisationRequest(
                    SqlitePersistenceFixture.CorpusId,
                    registration,
                    DocumentFormat.Csv,
                    new DocumentChunkingContext(
                        SqlitePersistenceFixture.CorpusId,
                        productId,
                        productRevision,
                        documentId,
                        documentVersion,
                        DocumentFormat.Csv,
                        SupportedLanguage.EnGb,
                        adapterId,
                        SourceTrustClass.OfficialExternal),
                    new ParserPolicy(131_072, 32, 131_072, 32, 16_384),
                    new ChunkingPolicy(128, 16, 160),
                    MaximumByteLength: 131_072,
                    Audit("s06-loopback-snapshot", SqlitePersistenceFixture.At(2)),
                    Audit("s06-loopback-observation", SqlitePersistenceFixture.At(2)),
                    ExpectedJournalRevision: 0,
                    new OfficialObservationId("observation-s06-loopback"),
                    TimeSpan.FromDays(7)));

            Assert.Equal(
                OfficialSynchronisationOutcome.SnapshotCreatedRebuildRequired,
                result.Outcome);
            Assert.Equal(registration.Id, result.Snapshot.RegistrationId);
            Assert.Equal("loopback", Assert.Single(result.Chunks).Columns["feature"]);
            Assert.Equal("\"s06-loopback-v1\"", result.ETag);
            Assert.Equal(Assert.Single(transport.RequestedUris), new Uri(address, "synthetic.csv"));
            await using var stored = await fixture.ContentStore.OpenReadAsync(
                result.Snapshot.ContentObjectId);
            Assert.Equal(bytes.Length, stored.Length);
        }
        finally
        {
            await server.StopAsync();

            if (Directory.Exists(serverRoot))
            {
                Directory.Delete(serverRoot, recursive: true);
            }
        }
    }

    private static AdministrativeAuditContext Audit(
        string operationId,
        DateTimeOffset at) =>
        new(
            new OperationId(operationId),
            "state-06-integration",
            "synchronise-official",
            "Synchronise the authorised fake loopback fixture.",
            at);

    private sealed class LoopbackOfficialTransport(HttpClient client) : IOfficialSourceTransport
    {
        internal List<Uri> RequestedUris { get; } = [];

        public async Task<OfficialFetchResult> FetchAsync(
            OfficialSourceRegistration registration,
            OfficialFetchPolicy policy,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(registration);
            var target = new Uri(client.BaseAddress!, "synthetic.csv");

            if (!target.IsLoopback || target.Scheme != Uri.UriSchemeHttp)
            {
                throw new InvalidOperationException("The fake official transport is loopback-only.");
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, target);

            if (policy.IfNoneMatch is not null)
            {
                request.Headers.IfNoneMatch.Add(EntityTagHeaderValue.Parse(policy.IfNoneMatch));
            }

            request.Headers.IfModifiedSince = policy.IfModifiedSince;
            RequestedUris.Add(target);
            using var response = await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            if (content.LongLength > policy.MaximumByteLength)
            {
                throw new InvalidDataException("The fake official response exceeded its byte bound.");
            }

            return new OfficialFetchResult(
                OfficialFetchStatus.Changed,
                (int)response.StatusCode,
                content,
                response.Content.Headers.ContentType?.MediaType,
                response.Headers.ETag?.ToString(),
                response.Content.Headers.LastModified);
        }
    }
}
