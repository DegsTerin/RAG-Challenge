// Purpose: Verifies exact direct-HTTP OpenAI routes, bounded request policy and response mapping through an in-process handler without network access.
using System.Net;
using System.Text;
using System.Text.Json;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Infrastructure.Providers;

namespace RagChallenge.IntegrationTests;

public sealed class OpenAiHttpAdapterContractTests
{
    private const string MvpLanguageModel = OpenAiLanguageModelOptions.MvpModelId;
    private static readonly string[] ExpectedLanguageModelRequestProperties =
        ["input", "max_output_tokens", "model", "reasoning", "store", "text"];

    [Fact]
    public async Task EmbeddingAdapterUsesOnlyTheExactRouteAndOrderedFloatContract()
    {
        var handler = new RecordingHandler(
            """
            {"model":"text-embedding-3-small","data":[{"index":0,"embedding":[1.0,0.0,0.0]}]}
            """);
        using var client = CreateClient(handler);
        var descriptor = new EmbeddingProviderDescriptor(
            "openai",
            "text-embedding-3-small",
            "text-embedding-3-small",
            dimensions: 3);
        var adapter = new OpenAiHttpEmbeddingProvider(client, Credential);

        var result = await adapter.EmbedAsync(new EmbeddingBatchRequest(
            descriptor,
            new List<string> { "synthetic question" },
            maximumUtf8Bytes: 4096));

        Assert.Equal(descriptor, result.ObservedDescriptor);
        Assert.Equal(new float[] { 1, 0, 0 }, Assert.Single(result.Vectors).ToArray());
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("https://api.openai.com/v1/embeddings", handler.Uri!.AbsoluteUri);
        using var body = JsonDocument.Parse(handler.RequestBody!);
        Assert.Equal("text-embedding-3-small", body.RootElement.GetProperty("model").GetString());
        Assert.Equal(3, body.RootElement.GetProperty("dimensions").GetInt32());
        Assert.Equal("float", body.RootElement.GetProperty("encoding_format").GetString());
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task ResponseAdapterUsesTheAcceptedMvpProfileAndMapsStructuredCitations()
    {
        var structured = JsonSerializer.Serialize(new
        {
            answerLanguage = "en-GB",
            answer = "Grounded answer.",
            citedChunkIds = new List<string> { "chunk-allowed" },
        });
        var response = JsonSerializer.Serialize(new
        {
            model = MvpLanguageModel,
            status = "completed",
            output = new[]
            {
                new
                {
                    type = "message",
                    role = "assistant",
                    status = "completed",
                    content = new[] { new { type = "output_text", text = structured } },
                },
            },
        });
        var handler = new RecordingHandler(response);
        using var client = CreateClient(handler);
        var options = CreateLanguageModelOptions();
        var adapter = new OpenAiHttpLanguageModel(client, Credential, options);

        var result = await adapter.GenerateAsync(new GroundedGenerationRequest(
            "Trusted instruction.",
            "prompt-v1",
            "Question?",
            SupportedQueryLanguage.EnGb,
            new[] { new GroundedEvidence("chunk-allowed", "Synthetic evidence.", DocumentContentLanguage.EnGb) },
            maximumOutputCharacters: 1024));

        Assert.Equal(options.ExpectedDescriptor, result.ObservedDescriptor);
        Assert.Equal(SupportedQueryLanguage.EnGb, result.AnswerLanguage);
        Assert.Equal("Grounded answer.", result.Answer);
        Assert.Equal("chunk-allowed", Assert.Single(result.CitedChunkIds));
        Assert.Equal("https://api.openai.com/v1/responses", handler.Uri!.AbsoluteUri);
        using var body = JsonDocument.Parse(handler.RequestBody!);
        Assert.Equal(MvpLanguageModel, body.RootElement.GetProperty("model").GetString());
        Assert.False(body.RootElement.GetProperty("store").GetBoolean());
        Assert.False(body.RootElement.TryGetProperty("temperature", out _));
        Assert.False(body.RootElement.TryGetProperty("tools", out _));
        Assert.False(body.RootElement.TryGetProperty("background", out _));
        Assert.False(body.RootElement.TryGetProperty("previous_response_id", out _));
        Assert.Equal("none", body.RootElement.GetProperty("reasoning")
            .GetProperty("effort").GetString());
        Assert.Equal("current_turn", body.RootElement.GetProperty("reasoning")
            .GetProperty("context").GetString());
        Assert.Equal(
            ExpectedLanguageModelRequestProperties,
            body.RootElement.EnumerateObject()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));
        Assert.Equal("json_schema", body.RootElement.GetProperty("text")
            .GetProperty("format").GetProperty("type").GetString());
        Assert.Equal(512, body.RootElement.GetProperty("max_output_tokens").GetInt32());
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("{\"model\":\"text-embedding-3-small\"}")]
    [InlineData("{\"model\":\"unexpected-model\",\"data\":[{\"index\":0,\"embedding\":[1,0,0]},{\"index\":1,\"embedding\":[0,1,0]}]}")]
    [InlineData("{\"model\":\"text-embedding-3-small\",\"data\":[{\"index\":0,\"embedding\":[1,0,0]},{\"index\":0,\"embedding\":[0,1,0]}]}")]
    [InlineData("{\"model\":\"text-embedding-3-small\",\"data\":[{\"index\":0,\"embedding\":[1,0,0]},{\"index\":2,\"embedding\":[0,1,0]}]}")]
    [InlineData("{\"model\":\"text-embedding-3-small\",\"data\":[{\"index\":0,\"embedding\":[1,0]},{\"index\":1,\"embedding\":[0,1,0]}]}")]
    public async Task EmbeddingAdapterRejectsMalformedOrMisalignedResponses(string responseJson)
    {
        using var client = CreateClient(new RecordingHandler(responseJson));
        var adapter = new OpenAiHttpEmbeddingProvider(client, Credential);
        var request = new EmbeddingBatchRequest(
            new EmbeddingProviderDescriptor(
                "openai",
                "text-embedding-3-small",
                "text-embedding-3-small",
                dimensions: 3),
            new List<string> { "first", "second" },
            maximumUtf8Bytes: 4096);

        var failure = await Assert.ThrowsAsync<ProviderStageUnavailableException>(
            () => adapter.EmbedAsync(request));

        Assert.Equal("embedding", failure.Stage);
        Assert.Equal("The embedding provider response was invalid.", failure.Message);
        Assert.DoesNotContain("unexpected-model", failure.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TransportAndHttpPolicyFailuresAreTypedAndSanitised()
    {
        using var transportClient = CreateClient(new FailingHandler(
            new HttpRequestException("sensitive transport detail")));
        var transportAdapter = new OpenAiHttpEmbeddingProvider(
            transportClient,
            Credential);
        var request = new EmbeddingBatchRequest(
            new EmbeddingProviderDescriptor(
                "openai",
                "text-embedding-3-small",
                "text-embedding-3-small",
                dimensions: 3),
            new List<string> { "synthetic" },
            maximumUtf8Bytes: 4096);

        var transportFailure = await Assert.ThrowsAsync<ProviderStageUnavailableException>(
            () => transportAdapter.EmbedAsync(request));
        Assert.Equal("embedding", transportFailure.Stage);
        Assert.DoesNotContain("sensitive", transportFailure.Message, StringComparison.Ordinal);

        using var statusClient = CreateClient(new RecordingHandler(
            "{\"private\":\"response\"}",
            HttpStatusCode.TooManyRequests));
        var statusAdapter = new OpenAiHttpEmbeddingProvider(statusClient, Credential);
        var statusFailure = await Assert.ThrowsAsync<ProviderStageUnavailableException>(
            () => statusAdapter.EmbedAsync(request));
        Assert.Equal("embedding", statusFailure.Stage);
        Assert.DoesNotContain("private", statusFailure.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ResponseAdapterRejectsUnexpectedModelAndMalformedStructuredOutput()
    {
        var response = JsonSerializer.Serialize(new
        {
            model = "gpt-5.4-mini",
            status = "completed",
            output = new[]
            {
                new
                {
                    type = "message",
                    role = "assistant",
                    status = "completed",
                    content = new[] { new { type = "output_text", text = "{}" } },
                },
            },
        });
        using var client = CreateClient(new RecordingHandler(response));
        var adapter = new OpenAiHttpLanguageModel(
            client,
            Credential,
            CreateLanguageModelOptions());
        var request = new GroundedGenerationRequest(
            "Trusted instruction.",
            "prompt-v1",
            "Question?",
            SupportedQueryLanguage.EnGb,
            new[]
            {
                new GroundedEvidence(
                    "chunk-allowed",
                    "Synthetic evidence.",
                    DocumentContentLanguage.EnGb),
            },
            maximumOutputCharacters: 1024);

        var failure = await Assert.ThrowsAsync<ProviderStageUnavailableException>(
            () => adapter.GenerateAsync(request));

        Assert.Equal("generation", failure.Stage);
        Assert.Equal("The language-model provider response was invalid.", failure.Message);
        Assert.DoesNotContain("gpt-5.4-mini", failure.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("""
        {"model":"gpt-5.4-mini-2026-03-17","status":"completed","output":[{"type":"reasoning"}]}
        """)]
    [InlineData("""
        {"model":"gpt-5.4-mini-2026-03-17","status":"completed","output":[{"type":"message","role":"assistant","status":"completed","content":[{"type":"refusal","refusal":"synthetic refusal"}]}]}
        """)]
    [InlineData("""
        {"model":"gpt-5.4-mini-2026-03-17","status":"incomplete","output":[]}
        """)]
    public async Task ResponseAdapterRejectsUnauthorisedOutputItems(string responseJson)
    {
        using var client = CreateClient(new RecordingHandler(responseJson));
        var adapter = new OpenAiHttpLanguageModel(
            client,
            Credential,
            CreateLanguageModelOptions());

        var failure = await Assert.ThrowsAsync<ProviderStageUnavailableException>(() =>
            adapter.GenerateAsync(CreateGenerationRequest()));

        Assert.Equal("generation", failure.Stage);
        Assert.Equal("The language-model provider response was invalid.", failure.Message);
        Assert.DoesNotContain("synthetic refusal", failure.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("other", "gpt-5.4-mini-2026-03-17", "gpt-5.4-mini-2026-03-17")]
    [InlineData("openai", "gpt-5.4-mini", "gpt-5.4-mini")]
    [InlineData("openai", "gpt-5.4-mini-2026-03-17", "gpt-5.4-mini")]
    public void LanguageModelOptionsRejectAnUnapprovedDescriptor(
        string providerId,
        string modelId,
        string modelRevision)
    {
        Assert.Throws<ArgumentException>(() => new OpenAiLanguageModelOptions(
            new LanguageModelDescriptor(providerId, modelId, modelRevision),
            OpenAiReasoningEffort.None,
            OpenAiReasoningContext.CurrentTurn));
    }

    [Fact]
    public void LanguageModelOptionsRejectUnsupportedReasoningConfiguration()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new OpenAiLanguageModelOptions(
            CreateLanguageModelDescriptor(),
            (OpenAiReasoningEffort)999,
            OpenAiReasoningContext.CurrentTurn));
        Assert.Throws<ArgumentOutOfRangeException>(() => new OpenAiLanguageModelOptions(
            CreateLanguageModelDescriptor(),
            OpenAiReasoningEffort.None,
            (OpenAiReasoningContext)999));
    }

    [Fact]
    public void ClientPolicyDisablesRedirectsProxyAndAutomaticDecompression()
    {
        using var handler = OpenAiHttpClientPolicy.CreateDenyByDefaultHandler();
        Assert.False(handler.AllowAutoRedirect);
        Assert.False(handler.UseProxy);
        Assert.Equal(DecompressionMethods.None, handler.AutomaticDecompression);
        Assert.Equal(TimeSpan.FromSeconds(10), handler.ConnectTimeout);
    }

    [Fact]
    public void AdapterRejectsAClientWhoseTotalTimeoutExceedsTheApprovedBudget()
    {
        using var client = CreateClient(new RecordingHandler("{}"));
        client.Timeout = TimeSpan.FromSeconds(26);

        Assert.Throws<ArgumentException>(() => new OpenAiHttpEmbeddingProvider(
            client,
            Credential));
    }

    private static HttpClient CreateClient(HttpMessageHandler handler) =>
        new(handler)
        {
            BaseAddress = new Uri("https://api.openai.com/", UriKind.Absolute),
            Timeout = TimeSpan.FromSeconds(25),
        };

    private static OpenAiLanguageModelOptions CreateLanguageModelOptions() =>
        new(
            CreateLanguageModelDescriptor(),
            OpenAiReasoningEffort.None,
            OpenAiReasoningContext.CurrentTurn);

    private static LanguageModelDescriptor CreateLanguageModelDescriptor() =>
        new("openai", MvpLanguageModel, MvpLanguageModel);

    private static GroundedGenerationRequest CreateGenerationRequest() =>
        new(
            "Trusted instruction.",
            "prompt-v1",
            "Question?",
            SupportedQueryLanguage.EnGb,
            new[]
            {
                new GroundedEvidence(
                    "chunk-allowed",
                    "Synthetic evidence.",
                    DocumentContentLanguage.EnGb),
            },
            maximumOutputCharacters: 1024);

    private static ValueTask<string> Credential(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult("<synthetic-credential>");
    }

    private sealed class RecordingHandler(
        string responseJson,
        HttpStatusCode statusCode = HttpStatusCode.OK) : HttpMessageHandler
    {
        public int CallCount { get; private set; }

        public HttpMethod? Method { get; private set; }

        public Uri? Uri { get; private set; }

        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Method = request.Method;
            Uri = request.RequestUri;
            RequestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json"),
            };
        }
    }

    private sealed class FailingHandler(Exception failure) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromException<HttpResponseMessage>(failure);
    }
}
