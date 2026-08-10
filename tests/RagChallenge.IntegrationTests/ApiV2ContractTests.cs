// Purpose: Verifies the frozen v2 query and same-origin visual-evidence HTTP projections, bounded revalidation and byte-for-byte v1 coexistence without opening a listener.
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;
using RagChallenge.Server.Api.OperationsGovernance;
using V2 = RagChallenge.Server.Api.Contracts.V2;

namespace RagChallenge.IntegrationTests;

public sealed class ApiV2ContractTests
{
    private static readonly string GenerationDigest = new('a', 64);
    private static readonly string ManifestDigest = new('b', 64);
    private static readonly byte[] ImageBytes = CreatePngHeader(width: 16, height: 24);
    private static readonly string ImageDigest = Convert.ToHexString(
        SHA256.HashData(ImageBytes)).ToLowerInvariant();

    [Fact]
    public async Task QueryEndpointProjectsExactLanguagesAndPageSelectors()
    {
        var service = new FakeQuestionService();
        await using var app = SetupHost.Build(
            [],
            services => services.AddSingleton<IQuestionAnsweringService>(service));
        var endpoint = FindEndpoint(app, V2.QueryEndpoints.Route);
        Assert.Contains(
            endpoint.Metadata,
            metadata => metadata is EnableRateLimitingAttribute attribute &&
                attribute.PolicyName ==
                    RagChallenge.Server.Api.Contracts.V1.QueryEndpoints.RateLimitPolicy);

        var context = CreateContext(app.Services, HttpMethods.Post);
        var result = await V2.QueryEndpoints.HandleAsync(
            new V2.QueryRequestV2("main-corpus", "en-GB", "What is the evidence?"),
            context,
            service,
            app.Services.GetRequiredService<QueryConcurrencyGate>());
        await result.ExecuteAsync(context);
        context.Response.Body.Position = 0;
        using var response = await JsonDocument.ParseAsync(context.Response.Body);
        var citation = response.RootElement.GetProperty("citations")[0];
        var image = citation.GetProperty("pageImages")[0];

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal(QueryContractVersion.V2, service.ContractVersion);
        Assert.Equal("en-GB", citation.GetProperty("contentLanguage").GetString());
        Assert.Equal("EN-gb", citation.GetProperty("sourceDeclaredLanguage").GetString());
        Assert.Equal($"rendermanifest-{ManifestDigest}",
            image.GetProperty("renderManifestId").GetString());
        Assert.Equal(ImageDigest, image.GetProperty("imageContentObjectId").GetString());
        Assert.Equal(JsonValueKind.Null, image.GetProperty("obligationSetId").ValueKind);
        Assert.Equal(JsonValueKind.Null,
            citation.GetProperty("derivativeObligationPresentation").ValueKind);
        Assert.False(response.RootElement.TryGetProperty("answerEvidenceRecordId", out _));
        Assert.False(citation.TryGetProperty("path", out _));
        Assert.False(citation.TryGetProperty("rights", out _));
    }

    [Fact]
    public async Task QueryEndpointProjectsTheCompleteNoticeBearingObligationSet()
    {
        var service = new FakeQuestionService { NoticeBearing = true };
        await using var app = SetupHost.Build(
            [],
            services => services.AddSingleton<IQuestionAnsweringService>(service));
        var context = CreateContext(app.Services, HttpMethods.Post);

        var result = await V2.QueryEndpoints.HandleAsync(
            new V2.QueryRequestV2("main-corpus", "en-GB", "What is the evidence?"),
            context,
            service,
            app.Services.GetRequiredService<QueryConcurrencyGate>());
        await result.ExecuteAsync(context);
        context.Response.Body.Position = 0;
        using var response = await JsonDocument.ParseAsync(context.Response.Body);
        var citation = response.RootElement.GetProperty("citations")[0];
        var image = citation.GetProperty("pageImages")[0];
        var obligations = citation.GetProperty("derivativeObligationPresentation");

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal(
            obligations.GetProperty("obligationSetId").GetString(),
            image.GetProperty("obligationSetId").GetString());
        Assert.Equal("en-GB", obligations.GetProperty("contentLanguage").GetString());
        Assert.Equal("Synthetic copyright notice.",
            obligations.GetProperty("copyrightNotice").GetString());
        Assert.Equal(2, obligations.GetProperty("orderedDisclaimers").GetArrayLength());
        Assert.Equal("NotApplicable",
            obligations.GetProperty("trademarkTreatment").GetString());
        Assert.Equal("Rendered synthetic derivative with unchanged source pixels.",
            obligations.GetProperty("changeMarkingText").GetString());
    }

    [Fact]
    public async Task VisualEndpointRevalidatesBeforeBothContentAndNotModified()
    {
        var reader = new FakeVisualEvidenceReader();
        await using var app = SetupHost.Build(
            [],
            services => services.AddSingleton<IVisualEvidenceReader>(reader));
        var endpoint = FindEndpoint(app, V2.VisualEvidenceEndpoints.Route);
        Assert.Contains(
            endpoint.Metadata,
            metadata => metadata is EnableRateLimitingAttribute attribute &&
                attribute.PolicyName == V2.VisualEvidenceEndpoints.RateLimitPolicy);

        var first = CreateContext(app.Services, HttpMethods.Get);
        await V2.VisualEvidenceEndpoints.HandleAsync(
            $"idxgen-{GenerationDigest}",
            $"rendermanifest-{ManifestDigest}",
            "7",
            ImageDigest,
            first,
            reader,
            app.Services.GetRequiredService<VisualEvidenceConcurrencyGate>());
        var etag = first.Response.Headers.ETag.ToString();

        Assert.Equal(StatusCodes.Status200OK, first.Response.StatusCode);
        Assert.Equal(ImageBytes.Length, first.Response.ContentLength);
        Assert.Equal("image/png", first.Response.ContentType);
        Assert.Equal($"\"sha256-{ImageDigest}\"", etag);
        Assert.Equal("private, no-cache", first.Response.Headers.CacheControl);
        Assert.Equal("nosniff", first.Response.Headers.XContentTypeOptions);
        Assert.Equal("same-origin", first.Response.Headers["Cross-Origin-Resource-Policy"]);
        Assert.Equal(ImageBytes, ((MemoryStream)first.Response.Body).ToArray());

        var conditional = CreateContext(app.Services, HttpMethods.Get);
        conditional.Request.Headers.IfNoneMatch = etag;
        await V2.VisualEvidenceEndpoints.HandleAsync(
            $"idxgen-{GenerationDigest}",
            $"rendermanifest-{ManifestDigest}",
            "7",
            ImageDigest,
            conditional,
            reader,
            app.Services.GetRequiredService<VisualEvidenceConcurrencyGate>());

        Assert.Equal(StatusCodes.Status304NotModified, conditional.Response.StatusCode);
        Assert.Equal(0, conditional.Response.Body.Length);
        Assert.Equal(2, reader.CallCount);
    }

    [Fact]
    public async Task MalformedAndInactiveVisualSelectorsShareTheUniformNotAvailableFailure()
    {
        var reader = new FakeVisualEvidenceReader
        {
            Outcome = VisualEvidenceReadOutcome.NotAvailable,
        };
        await using var app = SetupHost.Build([]);

        foreach (var selector in new[]
        {
            (Generation: "bad", Manifest: $"rendermanifest-{ManifestDigest}", Page: "7",
                Image: ImageDigest),
            (Generation: $"idxgen-{GenerationDigest}",
                Manifest: $"rendermanifest-{ManifestDigest}", Page: "7", Image: ImageDigest),
        })
        {
            var context = CreateContext(app.Services, HttpMethods.Get);
            await V2.VisualEvidenceEndpoints.HandleAsync(
                selector.Generation,
                selector.Manifest,
                selector.Page,
                selector.Image,
                context,
                reader,
                app.Services.GetRequiredService<VisualEvidenceConcurrencyGate>());
            context.Response.Body.Position = 0;
            using var problem = await JsonDocument.ParseAsync(context.Response.Body);

            Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
            Assert.Equal("CH_VISUAL_EVIDENCE_NOT_AVAILABLE",
                problem.RootElement.GetProperty("code").GetString());
        }
    }

    [Fact]
    public async Task MalformedPageNumberIsHandledBeforeTheDashboardFallback()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "rag-challenge-v2-routing",
            Guid.NewGuid().ToString("N"));
        var contentRoot = Path.Combine(root, "app");
        var storeRoot = Path.Combine(root, "store");
        Directory.CreateDirectory(Path.Combine(contentRoot, "wwwroot"));
        await File.WriteAllTextAsync(
            Path.Combine(contentRoot, "wwwroot", "index.html"),
            "<!doctype html><html><body>dashboard fallback</body></html>");
        var reader = new FakeVisualEvidenceReader();
        using var server = new InMemoryHttpServer();

        try
        {
            await using var app = SetupHost.Build(
            [
                "--environment", IntegrationRuntimeOptions.EnvironmentName,
                "--contentRoot", contentRoot,
                $"--{IntegrationRuntimeOptions.EnabledKey}", "true",
                $"--{IntegrationRuntimeOptions.StoreRootKey}", storeRoot,
                "--RagChallenge:Setup:AllowExternalServices", "false",
            ],
            services =>
            {
                services.AddSingleton<IServer>(server);
                services.AddSingleton<IVisualEvidenceReader>(reader);
            });
            await app.StartAsync();

            var response = await server.SendAsync(
                $"/api/v2/evidence/page-images/idxgen-{GenerationDigest}/" +
                $"rendermanifest-{ManifestDigest}/not-an-integer/{ImageDigest}");
            response.Body.Position = 0;
            using var problem = await JsonDocument.ParseAsync(response.Body);
            var selectedRoute = Assert.IsType<RouteEndpoint>(response.Endpoint);

            Assert.Equal(V2.VisualEvidenceEndpoints.Route, selectedRoute.RoutePattern.RawText);
            Assert.Equal(StatusCodes.Status404NotFound, response.StatusCode);
            Assert.Equal("CH_VISUAL_EVIDENCE_NOT_AVAILABLE",
                problem.RootElement.GetProperty("code").GetString());
            Assert.Equal(0, reader.CallCount);

            await app.StopAsync();
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public async Task VisualConcurrencyCeilingReturnsBoundedRateLimitWithoutReading()
    {
        var reader = new FakeVisualEvidenceReader();
        using var gate = new VisualEvidenceConcurrencyGate();

        for (var index = 0; index < 4; index++)
        {
            Assert.True(await gate.TryEnterAsync(CancellationToken.None));
        }

        await using var app = SetupHost.Build([]);
        var context = CreateContext(app.Services, HttpMethods.Get);
        await V2.VisualEvidenceEndpoints.HandleAsync(
            $"idxgen-{GenerationDigest}",
            $"rendermanifest-{ManifestDigest}",
            "7",
            ImageDigest,
            context,
            reader,
            gate);
        context.Response.Body.Position = 0;
        using var problem = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
        Assert.Equal("10", context.Response.Headers.RetryAfter);
        Assert.Equal("CH_VISUAL_EVIDENCE_RATE_LIMITED",
            problem.RootElement.GetProperty("code").GetString());
        Assert.Equal(0, reader.CallCount);

        for (var index = 0; index < 4; index++)
        {
            gate.Exit();
        }
    }

    [Fact]
    public async Task OpenApiVersionsRemainSeparateAndV1RemainsByteProtected()
    {
        var root = FindRepositoryRoot();
        var v1 = await File.ReadAllBytesAsync(Path.Combine(root, "docs", "api", "openapi-v1.json"));
        var v2 = await File.ReadAllBytesAsync(Path.Combine(root, "docs", "api", "openapi-v2.json"));

        Assert.Equal(
            "d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34",
            Convert.ToHexString(SHA256.HashData(v1)).ToLowerInvariant());
        Assert.Equal(
            "f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733",
            Convert.ToHexString(SHA256.HashData(v2)).ToLowerInvariant());
        using var v2Document = JsonDocument.Parse(v2);
        Assert.True(v2Document.RootElement.GetProperty("paths")
            .TryGetProperty(V2.QueryEndpoints.Route, out _));
        Assert.True(v2Document.RootElement.GetProperty("paths")
            .TryGetProperty(
                "/api/v2/evidence/page-images/{indexGenerationId}/{renderManifestId}/{pageNumber}/{imageContentObjectId}",
                out _));
        var schemas = v2Document.RootElement.GetProperty("components").GetProperty("schemas");
        Assert.False(schemas.GetProperty("DerivativeObligationPresentationV1")
            .GetProperty("additionalProperties").GetBoolean());
        Assert.Equal(
            "^obligationset-[a-f0-9]{64}$",
            schemas.GetProperty("ObligationSetId").GetProperty("pattern").GetString());
    }

    private static RouteEndpoint FindEndpoint(WebApplication app, string route) =>
        ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Single(endpoint => endpoint.RoutePattern.RawText == route);

    private static DefaultHttpContext CreateContext(IServiceProvider services, string method)
    {
        var context = new DefaultHttpContext
        {
            RequestServices = services,
            TraceIdentifier = "correlation-api-v2",
        };
        context.Request.Method = method;
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "RAG-Challenge.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("The repository root was not found.");
    }

    private static byte[] CreatePngHeader(int width, int height)
    {
        var bytes = new byte[24];
        byte[] signature = [137, 80, 78, 71, 13, 10, 26, 10];
        signature.CopyTo(bytes, 0);
        bytes[11] = 13;
        Encoding.ASCII.GetBytes("IHDR").CopyTo(bytes, 12);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(16, 4), width);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(20, 4), height);
        return bytes;
    }

    private sealed class FakeQuestionService : IQuestionAnsweringService
    {
        public QueryContractVersion ContractVersion { get; private set; }

        public bool NoticeBearing { get; init; }

        public Task<QueryExecutionResult> AskAsync(
            QueryRequest request,
            DateTimeOffset observedAt,
            CancellationToken cancellationToken = default)
        {
            ContractVersion = request.ContractVersion;
            var imageId = new ContentObjectId(ImageDigest);
            var documentId = new DocumentId("document-1");
            var documentVersion = new DocumentVersionNumber(1);
            var rights = new DocumentRightsEligibilityRecordV1(
                documentId,
                documentVersion,
                Enum.GetValues<DocumentRight>().Select(right => new DocumentRightDecision(
                    right,
                    DocumentRightDecisionState.Permitted,
                    new DocumentRightsEvidenceReference($"rights-contract-{right}"))));
            var obligationSet = NoticeBearing
                ? DerivativeObligationSetV1.Create(
                    rights,
                    new ContentObjectId(new string('c', 64)),
                    rights.Decisions.Select(decision => decision.EvidenceReference),
                    DocumentContentLanguage.EnGb,
                    "Synthetic Documentation Group",
                    "Synthetic Reference",
                    "1.0",
                    "synthetic-source-v1",
                    "Synthetic attribution.",
                    "Synthetic copyright notice.",
                    "Synthetic permission notice.",
                    ["Synthetic disclaimer one.", "Synthetic disclaimer two."],
                    DerivativeTrademarkTreatment.NotApplicable,
                    "NotApplicable: no trademark applies to the synthetic fixture.",
                    "Rendered synthetic derivative with unchanged source pixels.",
                    new DateTimeOffset(2026, 8, 10, 12, 0, 0, TimeSpan.Zero),
                    "assessor-synthetic-v1")
                : null;
            var citation = new QueryCitation(
                new CorpusId("main-corpus"),
                new IndexGenerationId($"idxgen-{GenerationDigest}"),
                new DatabaseProductId("database-1"),
                new DatabaseProductRevision(1),
                documentId,
                documentVersion,
                DocumentFormat.Pdf,
                DocumentContentLanguage.EnGb,
                "chunk-1",
                new SourceAdapterId("local-pdf"),
                SourceTrustClass.LocalAuthorised,
                "Bounded evidence.",
                "Document title",
                7,
                7,
                null,
                null,
                [],
                null,
                null,
                null,
                SourceFreshness.Local)
            {
                SourceDeclaredLanguage = new SourceDeclaredLanguage("EN-gb"),
                PageImages =
                [
                    new QueryPageImage(
                        7,
                        new RenderManifestId($"rendermanifest-{ManifestDigest}"),
                        imageId,
                        "image/png",
                        16,
                        24,
                        new ImageSha256(ImageDigest),
                        obligationSet?.ObligationSetId),
                ],
                DerivativeObligationSet = obligationSet,
            };
            return Task.FromResult(QueryExecutionResult.Completed(new QueryCompletion(
                QueryOutcome.Answered,
                request.QuestionLanguage,
                "Grounded answer.",
                [citation],
                new EvidenceCoverage(1, 1, 1, 1,
                    new Dictionary<string, SourceFreshness>()),
                new IndexGenerationId($"idxgen-{GenerationDigest}"),
                "retrieval-v1",
                "grounded-answer-v1",
                new LanguageModelDescriptor("fake", "model-v1", "fixture-1"),
                request.CorrelationId)));
        }
    }

    private sealed class FakeVisualEvidenceReader : IVisualEvidenceReader
    {
        public VisualEvidenceReadOutcome Outcome { get; init; } =
            VisualEvidenceReadOutcome.Available;

        public int CallCount { get; private set; }

        public Task<VisualEvidenceReadResult> ReadAsync(
            VisualEvidenceSelector selector,
            DateTimeOffset observedAt,
            CancellationToken cancellationToken = default)
        {
            CallCount++;

            if (Outcome != VisualEvidenceReadOutcome.Available)
            {
                return Task.FromResult(VisualEvidenceReadResult.NotAvailable());
            }

            var contentId = new ContentObjectId(ImageDigest);
            var content = new VerifiedContentObject(
                contentId,
                contentId,
                ImageBytes.Length,
                new MemoryStream(ImageBytes, writable: false),
                ContentVerificationOutcome.Verified);
            return Task.FromResult(VisualEvidenceReadResult.Available(
                new VisualEvidenceContent(content, "image/png", 16, 24)));
        }
    }

    private sealed record InMemoryHttpResponse(
        int StatusCode,
        Stream Body,
        Endpoint? Endpoint);

    private sealed class InMemoryHttpServer : IServer
    {
        private Func<IFeatureCollection, Task>? processRequest;

        public IFeatureCollection Features { get; } = new FeatureCollection();

        public Task StartAsync<TContext>(
            IHttpApplication<TContext> application,
            CancellationToken cancellationToken)
            where TContext : notnull
        {
            processRequest = async features =>
            {
                var context = application.CreateContext(features);
                Exception? exception = null;

                try
                {
                    await application.ProcessRequestAsync(context);
                }
                catch (Exception caught)
                {
                    exception = caught;
                    throw;
                }
                finally
                {
                    application.DisposeContext(context, exception);
                }
            };
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public async Task<InMemoryHttpResponse> SendAsync(string path)
        {
            var execute = processRequest ?? throw new InvalidOperationException(
                "The in-memory HTTP server has not started.");
            var body = new MemoryStream();
            var features = new FeatureCollection();
            features.Set<IHttpRequestFeature>(new HttpRequestFeature
            {
                Method = HttpMethods.Get,
                Path = path,
                RawTarget = path,
                Protocol = "HTTP/1.1",
                Scheme = "http",
                Headers = new HeaderDictionary(),
            });
            features.Set<IHttpResponseFeature>(new HttpResponseFeature
            {
                Headers = new HeaderDictionary(),
            });
            features.Set<IHttpResponseBodyFeature>(new StreamResponseBodyFeature(body));
            features.Set<IHttpConnectionFeature>(new HttpConnectionFeature
            {
                RemoteIpAddress = IPAddress.Loopback,
            });

            await execute(features);
            return new InMemoryHttpResponse(
                features.GetRequiredFeature<IHttpResponseFeature>().StatusCode,
                body,
                features.Get<IEndpointFeature>()?.Endpoint);
        }

        public void Dispose()
        {
        }
    }
}
