// Purpose: Verifies exact direct-HTTP OpenAI routes, bounded request policy and response mapping through an in-process handler without network access.
using System.Net;
using System.Text;
using System.Text.Json;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.ProviderBudget;
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
        var adapter = CreateEmbeddingAdapter(client);

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
        var adapter = new OpenAiHttpLanguageModel(
            client,
            Credential,
            options,
            CreateBudgetGate(ProviderBudgetOperationClass.GroundedGeneration));

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

    [Fact]
    public async Task ResponseAdapterIgnoresReasoningMetadataAndMapsTheSingleMessage()
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
            output = new object[]
            {
                new { type = "reasoning", id = "reasoning-synthetic" },
                new
                {
                    type = "message",
                    role = "assistant",
                    status = "completed",
                    content = new[] { new { type = "output_text", text = structured } },
                },
            },
        });
        using var client = CreateClient(new RecordingHandler(response));
        var adapter = new OpenAiHttpLanguageModel(
            client,
            Credential,
            CreateLanguageModelOptions(),
            CreateBudgetGate(ProviderBudgetOperationClass.GroundedGeneration));

        var result = await adapter.GenerateAsync(CreateGenerationRequest());

        Assert.Equal("Grounded answer.", result.Answer);
        Assert.Equal("chunk-allowed", Assert.Single(result.CitedChunkIds));
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
        var adapter = CreateEmbeddingAdapter(client);
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
            Credential,
            CreateBudgetGate(ProviderBudgetOperationClass.QueryEmbedding),
            ProviderBudgetOperationClass.QueryEmbedding);
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
        var statusAdapter = CreateEmbeddingAdapter(statusClient);
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
            CreateLanguageModelOptions(),
            CreateBudgetGate(ProviderBudgetOperationClass.GroundedGeneration));
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
        {"model":"gpt-5.4-mini-2026-03-17","status":"completed","output":[{"type":"function_call"},{"type":"message","role":"assistant","status":"completed","content":[{"type":"output_text","text":"{}"}]}]}
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
            CreateLanguageModelOptions(),
            CreateBudgetGate(ProviderBudgetOperationClass.GroundedGeneration));

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
            Credential,
            CreateBudgetGate(ProviderBudgetOperationClass.QueryEmbedding),
            ProviderBudgetOperationClass.QueryEmbedding));
    }

    [Fact]
    public async Task MissingBudgetEnvelopeStopsBeforeCredentialLookupAndSyntheticHandler()
    {
        var credentialReads = 0;
        var handler = new RecordingHandler("{}");
        using var client = CreateClient(handler);
        var gate = new ProviderBudgetAdmissionGate(
            new FakeDeterministicProviderBudgetLedger(),
            new ProviderBudgetAdmissionContext(
                new ProviderBudgetEnvelopeId("PBE-MISSING"),
                new ProviderRuntimeSessionId("PRS-MISSING"),
                new ProviderBudgetAuthorityReference("AUTH-MISSING")),
            _ => ValueTask.CompletedTask);
        var adapter = new OpenAiHttpEmbeddingProvider(
            client,
            _ =>
            {
                credentialReads++;
                return ValueTask.FromResult("<synthetic-credential>");
            },
            gate,
            ProviderBudgetOperationClass.QueryEmbedding);

        await Assert.ThrowsAsync<ProviderBudgetAdmissionUnavailableException>(() =>
            adapter.EmbedAsync(new EmbeddingBatchRequest(
                new EmbeddingProviderDescriptor(
                    "openai",
                    "text-embedding-3-small",
                    "text-embedding-3-small",
                    dimensions: 3),
                ["synthetic"],
                maximumUtf8Bytes: 4096)));

        Assert.Equal(0, credentialReads);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task CredentialFailureReleasesReservationBeforeSyntheticHandler()
    {
        var envelope = CreateZeroEnvelope();
        var ledger = new FakeDeterministicProviderBudgetLedger(envelope);
        var providerRequestId = new ProviderRequestId("PBR-CREDENTIAL-FAILURE-001");
        var gate = new ProviderBudgetAdmissionGate(
            ledger,
            new ProviderBudgetAdmissionContext(
                envelope.EnvelopeId,
                envelope.RuntimeSessionId!,
                new ProviderBudgetAuthorityReference("AUTH-SYNTHETIC-QUERY"),
                () => providerRequestId),
            _ => ValueTask.CompletedTask);
        var handler = new RecordingHandler("{}");
        using var client = CreateClient(handler);
        var adapter = new OpenAiHttpEmbeddingProvider(
            client,
            _ => ValueTask.FromResult(string.Empty),
            gate,
            ProviderBudgetOperationClass.QueryEmbedding);

        await Assert.ThrowsAsync<ProviderStageUnavailableException>(() =>
            adapter.EmbedAsync(new EmbeddingBatchRequest(
                new EmbeddingProviderDescriptor(
                    "openai",
                    "text-embedding-3-small",
                    "text-embedding-3-small",
                    dimensions: 3),
                ["synthetic"],
                maximumUtf8Bytes: 4096)));

        var reservation = Assert.IsType<ProviderBudgetReservation>(
            await ledger.ReadReservationAsync(providerRequestId));
        Assert.Equal(ProviderBudgetReservationStatus.ReleasedPreSend, reservation.Status);
        Assert.Equal(0, handler.CallCount);
    }

    private static HttpClient CreateClient(HttpMessageHandler handler) =>
        new(handler)
        {
            BaseAddress = new Uri("https://api.openai.com/", UriKind.Absolute),
            Timeout = TimeSpan.FromSeconds(25),
        };

    private static OpenAiHttpEmbeddingProvider CreateEmbeddingAdapter(HttpClient client) =>
        new(
            client,
            Credential,
            CreateBudgetGate(ProviderBudgetOperationClass.QueryEmbedding),
            ProviderBudgetOperationClass.QueryEmbedding);

    private static ProviderBudgetAdmissionGate CreateBudgetGate(
        ProviderBudgetOperationClass operationClass)
    {
        var envelope = CreateZeroEnvelope();
        var authority = new ProviderBudgetAuthorityReference($"AUTH-SYNTHETIC-{operationClass}");
        return new ProviderBudgetAdmissionGate(
            new FakeDeterministicProviderBudgetLedger(envelope),
            new ProviderBudgetAdmissionContext(
                envelope.EnvelopeId,
                envelope.RuntimeSessionId!,
                authority),
            _ => ValueTask.CompletedTask);
    }

    private static ProviderBudgetEnvelopeV1 CreateZeroEnvelope()
    {
        var instant = DateTimeOffset.UtcNow;
        return new ProviderBudgetEnvelopeV1(
            new ProviderBudgetEnvelopeId($"PBE-SYNTHETIC-{Guid.NewGuid():N}"),
            new ProviderBudgetStoreEpochId("PSE-SYNTHETIC-001"),
            new ProviderBudgetScope(
                new ProviderBudgetEnvironmentId("ENV-SYNTHETIC"),
                new ProviderBudgetProviderId("openai"),
                new ProviderBudgetBillingScopeReference("BILLING-SYNTHETIC"),
                new ProviderBudgetModelId("MODEL-SYNTHETIC"),
                new ProviderBudgetCurrencyCode("USD"),
                new ProviderBudgetAccountingUnitId("UNIT-SYNTHETIC")),
            new ProviderBudgetConfigurationRevision(1),
            new ProviderBudgetLedgerRevision(1),
            new ProviderBudgetRearmRevision(1),
            ProviderBudgetState.Armed,
            new ProviderRuntimeSessionId("PRS-SYNTHETIC-001"),
            new ProviderBudgetCostScheduleId("PCS-SYNTHETIC-ZERO"),
            new ProviderBudgetSha256(new string('1', 64)),
            new ProviderBudgetUnits(0),
            new ProviderBudgetUnits(0),
            new ProviderBudgetUnits(0),
            new ProviderBudgetUnits(0),
            Enum.GetValues<ProviderBudgetOperationClass>().Select(value =>
                new ProviderBudgetOperationBalance(
                    value,
                    new ProviderBudgetUnits(0),
                    new ProviderBudgetUnits(0),
                    new ProviderBudgetUnits(0),
                    new ProviderBudgetUnits(0))),
            instant.AddMinutes(-1),
            instant.AddMinutes(10),
            isClosed: false,
            new ProviderBudgetSha256(new string('2', 64)));
    }

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
