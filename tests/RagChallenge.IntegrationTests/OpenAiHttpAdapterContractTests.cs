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
    public async Task ResponseAdapterDisablesStateAndToolsAndMapsStructuredCitations()
    {
        var structured = JsonSerializer.Serialize(new
        {
            answerLanguage = "en-GB",
            answer = "Grounded answer.",
            citedChunkIds = new List<string> { "chunk-allowed" },
        });
        var response = JsonSerializer.Serialize(new
        {
            model = "gpt-4.1-mini-2025-04-14",
            output = new[]
            {
                new
                {
                    type = "message",
                    content = new[] { new { type = "output_text", text = structured } },
                },
            },
        });
        var handler = new RecordingHandler(response);
        using var client = CreateClient(handler);
        var descriptor = new LanguageModelDescriptor(
            "openai",
            "gpt-4.1-mini-2025-04-14",
            "gpt-4.1-mini-2025-04-14");
        var adapter = new OpenAiHttpLanguageModel(client, Credential, descriptor);

        var result = await adapter.GenerateAsync(new GroundedGenerationRequest(
            "Trusted instruction.",
            "prompt-v1",
            "Question?",
            SupportedLanguage.EnGb,
            new[] { new GroundedEvidence("chunk-allowed", "Synthetic evidence.", SupportedLanguage.EnGb) },
            maximumOutputCharacters: 1024));

        Assert.Equal(descriptor, result.ObservedDescriptor);
        Assert.Equal(SupportedLanguage.EnGb, result.AnswerLanguage);
        Assert.Equal("Grounded answer.", result.Answer);
        Assert.Equal("chunk-allowed", Assert.Single(result.CitedChunkIds));
        Assert.Equal("https://api.openai.com/v1/responses", handler.Uri!.AbsoluteUri);
        using var body = JsonDocument.Parse(handler.RequestBody!);
        Assert.False(body.RootElement.GetProperty("store").GetBoolean());
        Assert.Equal(0, body.RootElement.GetProperty("temperature").GetInt32());
        Assert.False(body.RootElement.TryGetProperty("tools", out _));
        Assert.False(body.RootElement.TryGetProperty("background", out _));
        Assert.False(body.RootElement.TryGetProperty("previous_response_id", out _));
        Assert.Equal("json_schema", body.RootElement.GetProperty("text")
            .GetProperty("format").GetProperty("type").GetString());
        Assert.Equal(1, handler.CallCount);
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

    private static ValueTask<string> Credential(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult("<synthetic-credential>");
    }

    private sealed class RecordingHandler(string responseJson) : HttpMessageHandler
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
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json"),
            };
        }
    }
}
