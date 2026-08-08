// Purpose: Verifies the v1 endpoint surface, strict transport schema, completion mapping and sanitised canonical failures without opening a listener.
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Server.Api.Contracts.V1;
using RagChallenge.Server.Api.OperationsGovernance;

namespace RagChallenge.IntegrationTests;

public sealed class ApiV1ContractTests
{
    [Fact]
    public async Task QueryEndpointMapsCompletionWithoutAuthorityFields()
    {
        var service = new FakeQuestionService();
        await using var app = SetupHost.Build(
            [],
            services => services.AddSingleton<IQuestionAnsweringService>(service));
        var endpoint = FindEndpoint(app, QueryEndpoints.Route);
        Assert.Contains(
            endpoint.Metadata,
            metadata => metadata is EnableRateLimitingAttribute attribute &&
                attribute.PolicyName == QueryEndpoints.RateLimitPolicy);
        Assert.Contains(
            endpoint.Metadata,
            metadata => metadata is Microsoft.AspNetCore.Http.Metadata.IRequestSizeLimitMetadata limit &&
                limit.MaxRequestBodySize == QueryEndpoints.MaximumRequestBytes);

        var context = CreateContext(app.Services);
        var result = await QueryEndpoints.HandleAsync(
            new QueryRequestV1("main-corpus", "pt-BR", "Qual é a evidência?"),
            context,
            service,
            app.Services.GetRequiredService<QueryConcurrencyGate>());
        await result.ExecuteAsync(context);
        context.Response.Body.Position = 0;
        using var response = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal("InsufficientEvidence", response.RootElement.GetProperty("outcome").GetString());
        Assert.Equal("pt-BR", response.RootElement.GetProperty("answerLanguage").GetString());
        Assert.Equal("correlation-api-v1", response.RootElement.GetProperty("correlationId").GetString());
        Assert.Empty(response.RootElement.GetProperty("citations").EnumerateArray());
        Assert.Equal(1, service.CallCount);
    }

    [Fact]
    public async Task ConcurrencyGateRejectsTheTwentyFirstConcurrentQuery()
    {
        using var gate = new QueryConcurrencyGate();

        for (var index = 0; index < 20; index++)
        {
            Assert.True(await gate.TryEnterAsync(CancellationToken.None));
        }

        Assert.False(await gate.TryEnterAsync(CancellationToken.None));

        for (var index = 0; index < 20; index++)
        {
            gate.Exit();
        }
    }

    [Fact]
    public async Task UnsupportedLanguageReturnsSanitisedCanonicalProblem()
    {
        var service = new FakeQuestionService();
        await using var app = SetupHost.Build(
            [],
            services => services.AddSingleton<IQuestionAnsweringService>(service));
        var context = CreateContext(app.Services);
        var result = await QueryEndpoints.HandleAsync(
            new QueryRequestV1("main-corpus", "en-US", "Question"),
            context,
            service,
            app.Services.GetRequiredService<QueryConcurrencyGate>());
        await result.ExecuteAsync(context);
        context.Response.Body.Position = 0;
        using var response = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("CH_QUERY_INVALID_INPUT", response.RootElement.GetProperty("code").GetString());
        Assert.Equal(
            "correlation-api-v1",
            response.RootElement.GetProperty("correlationId").GetString());
        Assert.False(response.RootElement.TryGetProperty("stackTrace", out _));
        Assert.Equal(0, service.CallCount);
    }

    [Fact]
    public async Task Utf8QuestionLimitIsEnforcedBeforeTheService()
    {
        var service = new FakeQuestionService();
        await using var app = SetupHost.Build(
            [],
            services => services.AddSingleton<IQuestionAnsweringService>(service));
        var context = CreateContext(app.Services);
        var result = await QueryEndpoints.HandleAsync(
            new QueryRequestV1("main-corpus", "pt-BR", new string('é', 2049)),
            context,
            service,
            app.Services.GetRequiredService<QueryConcurrencyGate>());
        await result.ExecuteAsync(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal(0, service.CallCount);
    }

    [Theory]
    [InlineData(QueryFailureKind.CorpusUnavailable, 503, "CH_CORPUS_UNAVAILABLE")]
    [InlineData(QueryFailureKind.SourceUnavailable, 503, "CH_SOURCE_UNAVAILABLE")]
    [InlineData(QueryFailureKind.SourceStale, 503, "CH_SOURCE_STALE")]
    [InlineData(QueryFailureKind.SourcePolicyViolation, 503, "CH_SOURCE_POLICY_VIOLATION")]
    [InlineData(QueryFailureKind.EmbeddingUnavailable, 503, "CH_EMBEDDING_UNAVAILABLE")]
    [InlineData(QueryFailureKind.IndexUnavailable, 503, "CH_INDEX_UNAVAILABLE")]
    [InlineData(QueryFailureKind.LanguageModelUnavailable, 503, "CH_LANGUAGE_MODEL_UNAVAILABLE")]
    [InlineData(QueryFailureKind.ConfigurationInvalid, 503, "CH_CONFIGURATION_INVALID")]
    [InlineData(QueryFailureKind.OperationCancelled, 503, "CH_OPERATION_CANCELLED")]
    [InlineData(QueryFailureKind.RateLimited, 429, "CH_QUERY_RATE_LIMITED")]
    [InlineData(QueryFailureKind.UnexpectedFailure, 500, "CH_UNEXPECTED_FAILURE")]
    public async Task CanonicalFailureKindsMapToStableProblemDetails(
        QueryFailureKind kind,
        int expectedStatus,
        string expectedCode)
    {
        await using var app = SetupHost.Build([]);
        var context = CreateContext(app.Services);
        var result = QueryEndpoints.Problem(kind, "correlation-api-v1");
        await result.ExecuteAsync(context);
        context.Response.Body.Position = 0;
        using var response = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal(expectedStatus, context.Response.StatusCode);
        Assert.Equal(expectedCode, response.RootElement.GetProperty("code").GetString());
        Assert.StartsWith(
            "urn:rag-challenge:problem:",
            response.RootElement.GetProperty("type").GetString(),
            StringComparison.Ordinal);
        Assert.False(response.RootElement.TryGetProperty("stackTrace", out _));
    }

    [Fact]
    public async Task JsonAndOpenApiContractsRejectUnknownAuthorityAndRemainVersioned()
    {
        await using var app = SetupHost.Build([]);
        var jsonOptions = app.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        Assert.Equal(
            JsonUnmappedMemberHandling.Disallow,
            jsonOptions.SerializerOptions.UnmappedMemberHandling);
        var repositoryRoot = FindRepositoryRoot();
        var path = Path.Combine(repositoryRoot, "docs", "api", "openapi-v1.json");
        var openApiBytes = await File.ReadAllBytesAsync(path);
        Assert.Equal(
            "d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34",
            Convert.ToHexString(SHA256.HashData(openApiBytes)).ToLowerInvariant());
        using var document = JsonDocument.Parse(openApiBytes);
        var root = document.RootElement;
        Assert.Equal("3.1.0", root.GetProperty("openapi").GetString());
        Assert.Equal(
            "1.0.0",
            root.GetProperty("info").GetProperty("version").GetString());
        var requestSchema = root.GetProperty("components").GetProperty("schemas")
            .GetProperty("QueryRequestV1");
        Assert.False(requestSchema.GetProperty("additionalProperties").GetBoolean());
        var properties = requestSchema.GetProperty("properties")
            .EnumerateObject().Select(property => property.Name).ToArray();
        Assert.Equal(["corpusId", "questionLanguage", "question"], properties);
        Assert.DoesNotContain(properties, property =>
            property.Contains("url", StringComparison.OrdinalIgnoreCase) ||
            property.Contains("provider", StringComparison.OrdinalIgnoreCase) ||
            property.Contains("adapter", StringComparison.OrdinalIgnoreCase) ||
            property.Contains("theme", StringComparison.OrdinalIgnoreCase) ||
            property.Contains("interfaceLanguage", StringComparison.OrdinalIgnoreCase));
        Assert.True(root.GetProperty("paths").TryGetProperty(QueryEndpoints.Route, out _));
        Assert.True(root.GetProperty("paths").TryGetProperty("/api/v1/health/live", out _));
        Assert.True(root.GetProperty("paths").TryGetProperty("/api/v1/health/ready", out _));
    }

    private static RouteEndpoint FindEndpoint(WebApplication app, string route) =>
        ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Single(endpoint => endpoint.RoutePattern.RawText == route);

    private static DefaultHttpContext CreateContext(IServiceProvider services)
    {
        var context = new DefaultHttpContext
        {
            RequestServices = services,
            TraceIdentifier = "correlation-api-v1",
        };
        context.Request.Method = HttpMethods.Post;
        context.Request.ContentType = "application/json";
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

    private sealed class FakeQuestionService : IQuestionAnsweringService
    {
        public int CallCount { get; private set; }

        public Task<QueryExecutionResult> AskAsync(
            QueryRequest request,
            DateTimeOffset observedAt,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            var generationId = new IndexGenerationId(
                $"idxgen-{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("api"))).ToLowerInvariant()}");
            return Task.FromResult(QueryExecutionResult.Completed(new QueryCompletion(
                QueryOutcome.InsufficientEvidence,
                request.QuestionLanguage,
                Answer: null,
                Citations: [],
                new EvidenceCoverage(1, 1, 1, 1, new Dictionary<string, SourceFreshness>()),
                generationId,
                QuestionAnsweringService.RetrievalPolicyVersion,
                QuestionAnsweringService.PromptVersion,
                new LanguageModelDescriptor("fake", "model-v1", "fixture-1"),
                request.CorrelationId)));
        }
    }
}
