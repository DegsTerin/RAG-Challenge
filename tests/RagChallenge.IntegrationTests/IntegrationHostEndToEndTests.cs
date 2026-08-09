// Purpose: Verifies the synthetic v1/v2 composed flow over a real loopback listener and proves durable visual evidence across restart and confined cold backup/restore.
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Server.Api.OperationsGovernance;

namespace RagChallenge.IntegrationTests;

public sealed class IntegrationHostEndToEndTests
{
    private static readonly EmbeddingProviderDescriptor EmbeddingDescriptor =
        new("synthetic", "deterministic-v1", "s06-a", dimensions: 3);
    private static readonly LanguageModelDescriptor LanguageModelDescriptor =
        new("synthetic", "grounded-v1", "s06-a");

    [Fact]
    public async Task SameOriginV2VisualEvidenceSurvivesRestartAndColdRestore()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "rag-challenge-state07-v2-host",
            Guid.NewGuid().ToString("N"));
        var contentRoot = Path.Combine(root, "app");
        var storeRoot = Path.Combine(root, "store");
        var backupRoot = Path.Combine(root, "backup");
        var restoredRoot = Path.Combine(root, "restored");
        Directory.CreateDirectory(Path.Combine(contentRoot, "wwwroot"));
        await File.WriteAllTextAsync(
            Path.Combine(contentRoot, "wwwroot", "index.html"),
            "<!doctype html><html><body><div id=\"root\"></div></body></html>");

        try
        {
            var first = await StartHostAsync(contentRoot, storeRoot);
            var queryRuntime = Assert.IsType<SyntheticIntegrationRuntime>(
                first.Application.Services.GetRequiredService<IQuestionAnsweringService>());
            Assert.Same(
                queryRuntime,
                first.Application.Services.GetRequiredService<IVisualEvidenceReader>());
            string generation;
            VisualSelector selector;
            byte[] imageBytes;

            try
            {
                using var client = new HttpClient { BaseAddress = first.BaseAddress };
                var shell = await client.GetStringAsync("/");
                Assert.Contains("<div id=\"root\"></div>", shell, StringComparison.Ordinal);

                var response = await AskAsync(client, "en-GB");
                Assert.Equal("Answered", response.GetProperty("outcome").GetString());
                Assert.Equal("en-GB", response.GetProperty("answerLanguage").GetString());
                Assert.Single(response.GetProperty("citations").EnumerateArray());
                generation = response.GetProperty("indexGenerationId").GetString()!;

                var v2 = await AskV2Async(client, "en-GB");
                Assert.Equal("Answered", v2.GetProperty("outcome").GetString());
                selector = GetVisualSelector(v2);
                Assert.Equal(generation, selector.IndexGenerationId);
                var visual = await AssertVisualAvailableAsync(client, selector);
                imageBytes = visual.Bytes;
                await AssertVisualNotModifiedAsync(client, selector, visual.ETag);

                var portuguese = await AskAsync(client, "pt-BR");
                Assert.Equal("Answered", portuguese.GetProperty("outcome").GetString());
                Assert.Equal("pt-BR", portuguese.GetProperty("answerLanguage").GetString());
            }
            finally
            {
                await StopHostAsync(first.Application);
            }

            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            Assert.True(File.Exists(Path.Combine(storeRoot, "control.db")));
            Assert.True(File.Exists(Path.Combine(storeRoot, "vectors.db")));
            Assert.NotEmpty(Directory.EnumerateFiles(
                Path.Combine(storeRoot, "content"),
                "*",
                SearchOption.AllDirectories));
            var storeFingerprint = FingerprintStore(storeRoot);
            CopyColdStore(root, storeRoot, backupRoot);
            Assert.Equal(storeFingerprint, FingerprintStore(backupRoot));

            var second = await StartHostAsync(contentRoot, storeRoot);

            try
            {
                using var client = new HttpClient { BaseAddress = second.BaseAddress };
                var response = await AskAsync(client, "en-GB");
                Assert.Equal("Answered", response.GetProperty("outcome").GetString());
                Assert.Equal(
                    generation,
                    response.GetProperty("indexGenerationId").GetString());
                var v2 = await AskV2Async(client, "en-GB");
                Assert.Equal(selector, GetVisualSelector(v2));
                var visual = await AssertVisualAvailableAsync(client, selector);
                Assert.Equal(imageBytes, visual.Bytes);

                using var readiness = JsonDocument.Parse(
                    await client.GetStringAsync("/api/v1/health/ready"));
                Assert.Equal("Ready", readiness.RootElement.GetProperty("status").GetString());
                Assert.Equal(
                    generation,
                    readiness.RootElement.GetProperty("activeGenerationId").GetString());
            }
            finally
            {
                await StopHostAsync(second.Application);
            }

            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            CopyColdStore(root, backupRoot, restoredRoot);
            Assert.Equal(storeFingerprint, FingerprintStore(restoredRoot));
            var restored = await StartHostAsync(contentRoot, restoredRoot);

            try
            {
                using var client = new HttpClient { BaseAddress = restored.BaseAddress };
                var v1 = await AskAsync(client, "en-GB");
                Assert.Equal(generation, v1.GetProperty("indexGenerationId").GetString());
                var v2 = await AskV2Async(client, "en-GB");
                Assert.Equal(selector, GetVisualSelector(v2));
                var visual = await AssertVisualAvailableAsync(client, selector);
                Assert.Equal(imageBytes, visual.Bytes);
            }
            finally
            {
                await StopHostAsync(restored.Application);
            }
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
    public async Task VisualEvidenceTokenBucketRejectsTheEleventhImmediateRequest()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "rag-challenge-state07-v2-rate-limit",
            Guid.NewGuid().ToString("N"));
        var contentRoot = Path.Combine(root, "app");
        var storeRoot = Path.Combine(root, "store");
        Directory.CreateDirectory(Path.Combine(contentRoot, "wwwroot"));
        await File.WriteAllTextAsync(
            Path.Combine(contentRoot, "wwwroot", "index.html"),
            "<!doctype html><html><body><div id=\"root\"></div></body></html>");

        try
        {
            var running = await StartHostAsync(contentRoot, storeRoot);

            try
            {
                using var client = new HttpClient { BaseAddress = running.BaseAddress };
                var selector = GetVisualSelector(await AskV2Async(client, "en-GB"));

                for (var index = 0; index < 10; index++)
                {
                    using var accepted = await client.GetAsync(selector.Path);
                    Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);
                }

                using var rejected = await client.GetAsync(selector.Path);
                var body = await rejected.Content.ReadAsStringAsync();
                using var problem = JsonDocument.Parse(body);
                Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
                Assert.Equal(
                    "CH_VISUAL_EVIDENCE_RATE_LIMITED",
                    problem.RootElement.GetProperty("code").GetString());
                Assert.Contains(
                    "10",
                    rejected.Headers.GetValues("Retry-After"));
            }
            finally
            {
                await StopHostAsync(running.Application);
            }
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
    public async Task ComposedQueryRecoversAfterCancellationAndProviderFailure()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "rag-challenge-s06-correction-host",
            Guid.NewGuid().ToString("N"));
        var contentRoot = Path.Combine(root, "app");
        var storeRoot = Path.Combine(root, "store");
        Directory.CreateDirectory(Path.Combine(contentRoot, "wwwroot"));
        await File.WriteAllTextAsync(
            Path.Combine(contentRoot, "wwwroot", "index.html"),
            "<!doctype html><html><body><div id=\"root\"></div></body></html>");
        var languageModel = new ControlledLanguageModel(LanguageModelDescriptor);
        var embeddingProvider = new DeterministicEmbeddingProvider(EmbeddingDescriptor);

        try
        {
            var first = await StartHostAsync(
                contentRoot,
                storeRoot,
                embeddingProvider,
                languageModel);
            string generation;

            try
            {
                using var client = new HttpClient { BaseAddress = first.BaseAddress };
                var established = await AskAsync(client, "en-GB");
                Assert.Equal("Answered", established.GetProperty("outcome").GetString());
                generation = established.GetProperty("indexGenerationId").GetString()!;

                var cancellation = languageModel.ArmCancellation();
                using var cancellationSource = new CancellationTokenSource();
                using var request = CreateQuestionRequest("en-GB");
                var inFlight = client.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationSource.Token);
                await cancellation.Entered.WaitAsync(TimeSpan.FromSeconds(10));
                cancellationSource.Cancel();
                await cancellation.Observed.WaitAsync(TimeSpan.FromSeconds(10));

                try
                {
                    using var cancelledResponse = await inFlight.WaitAsync(
                        TimeSpan.FromSeconds(10));
                    using var cancelledBody = JsonDocument.Parse(
                        await cancelledResponse.Content.ReadAsStringAsync());
                    Assert.Equal(
                        "CH_OPERATION_CANCELLED",
                        cancelledBody.RootElement.GetProperty("code").GetString());
                }
                catch (OperationCanceledException)
                {
                    Assert.True(cancellationSource.IsCancellationRequested);
                }

                languageModel.ArmProviderFailure();
                using var failureRequest = CreateQuestionRequest("en-GB");
                using var failureResponse = await client.SendAsync(failureRequest);
                var failureText = await failureResponse.Content.ReadAsStringAsync();
                using var failureBody = JsonDocument.Parse(failureText);

                Assert.Equal(HttpStatusCode.ServiceUnavailable, failureResponse.StatusCode);
                Assert.Equal(
                    "CH_LANGUAGE_MODEL_UNAVAILABLE",
                    failureBody.RootElement.GetProperty("code").GetString());
                Assert.Equal(
                    "The capability is unavailable.",
                    failureBody.RootElement.GetProperty("detail").GetString());
                Assert.DoesNotContain(
                    ControlledLanguageModel.SensitiveFailureDetail,
                    failureText,
                    StringComparison.Ordinal);

                var recovered = await AskAsync(client, "en-GB");
                Assert.Equal("Answered", recovered.GetProperty("outcome").GetString());
                Assert.Equal(
                    generation,
                    recovered.GetProperty("indexGenerationId").GetString());
            }
            finally
            {
                await StopHostAsync(first.Application);
            }

            var second = await StartHostAsync(
                contentRoot,
                storeRoot,
                embeddingProvider,
                languageModel);

            try
            {
                using var client = new HttpClient { BaseAddress = second.BaseAddress };
                var restarted = await AskAsync(client, "en-GB");
                Assert.Equal("Answered", restarted.GetProperty("outcome").GetString());
                Assert.Equal(
                    generation,
                    restarted.GetProperty("indexGenerationId").GetString());
            }
            finally
            {
                await StopHostAsync(second.Application);
            }
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

    private static async Task<JsonElement> AskAsync(HttpClient client, string language)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/v1/questions",
            new
            {
                corpusId = "database-systems-catalogue-mvp",
                questionLanguage = language,
                question = language == "pt-BR"
                    ? "Qual evidência de persistência está disponível?"
                    : "What persistence evidence is available?",
            });
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Synthetic query failed with {(int)response.StatusCode}: {body}");
        }

        using var document = JsonDocument.Parse(body);
        return document.RootElement.Clone();
    }

    private static async Task<JsonElement> AskV2Async(HttpClient client, string language)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/v2/questions",
            new
            {
                corpusId = "database-systems-catalogue-mvp",
                questionLanguage = language,
                question = language == "pt-BR"
                    ? "Qual evidência visual de persistência está disponível?"
                    : "What visual persistence evidence is available?",
            });
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Synthetic v2 query failed with {(int)response.StatusCode}: {body}");
        }

        using var document = JsonDocument.Parse(body);
        return document.RootElement.Clone();
    }

    private static VisualSelector GetVisualSelector(JsonElement response)
    {
        var citation = response.GetProperty("citations")[0];
        Assert.Equal("Pdf", citation.GetProperty("documentFormat").GetString());
        var page = citation.GetProperty("pageImages")[0];
        Assert.Equal(
            page.GetProperty("imageContentObjectId").GetString(),
            page.GetProperty("contentSha256").GetString());
        return new VisualSelector(
            response.GetProperty("indexGenerationId").GetString()!,
            page.GetProperty("renderManifestId").GetString()!,
            page.GetProperty("pageNumber").GetInt32(),
            page.GetProperty("imageContentObjectId").GetString()!);
    }

    private static async Task<VisualResponse> AssertVisualAvailableAsync(
        HttpClient client,
        VisualSelector selector)
    {
        using var response = await client.GetAsync(selector.Path);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        var digest = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(bytes.LongLength, response.Content.Headers.ContentLength);
        Assert.Equal(selector.ImageContentObjectId, digest);
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.True(response.Headers.CacheControl?.NoCache);
        Assert.Contains("nosniff", response.Headers.GetValues("X-Content-Type-Options"));
        Assert.Contains("same-origin", response.Headers.GetValues(
            "Cross-Origin-Resource-Policy"));
        var etag = response.Headers.ETag?.ToString();
        Assert.Equal($"\"sha256-{digest}\"", etag);
        return new VisualResponse(bytes, etag!);
    }

    private static async Task AssertVisualNotModifiedAsync(
        HttpClient client,
        VisualSelector selector,
        string etag)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, selector.Path);
        request.Headers.TryAddWithoutValidation("If-None-Match", etag);
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync());
        Assert.Equal(etag, response.Headers.ETag?.ToString());
    }

    private static void CopyColdStore(
        string taskRoot,
        string sourceRoot,
        string destinationRoot)
    {
        var containmentRoot = Path.GetFullPath(taskRoot)
            .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var source = Path.GetFullPath(sourceRoot);
        var destination = Path.GetFullPath(destinationRoot);

        if (!source.StartsWith(containmentRoot, StringComparison.OrdinalIgnoreCase) ||
            !destination.StartsWith(containmentRoot, StringComparison.OrdinalIgnoreCase) ||
            !Directory.Exists(source) ||
            Directory.Exists(destination))
        {
            throw new InvalidOperationException(
                "A cold store copy must remain inside a fresh task-owned root.");
        }

        var directories = Directory.EnumerateDirectories(
            source,
            "*",
            SearchOption.AllDirectories).Prepend(source).ToArray();

        if (directories.Any(directory =>
            (File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0))
        {
            throw new InvalidOperationException(
                "A cold store copy cannot traverse a reparse point.");
        }

        Directory.CreateDirectory(destination);

        foreach (var directory in directories.Skip(1))
        {
            Directory.CreateDirectory(Path.Combine(
                destination,
                Path.GetRelativePath(source, directory)));
        }

        foreach (var file in Directory.EnumerateFiles(
            source,
            "*",
            SearchOption.AllDirectories))
        {
            if ((File.GetAttributes(file) & FileAttributes.ReparsePoint) != 0)
            {
                throw new InvalidOperationException(
                    "A cold store copy cannot traverse a reparse point.");
            }

            File.Copy(
                file,
                Path.Combine(destination, Path.GetRelativePath(source, file)),
                overwrite: false);
        }
    }

    private static string[] FingerprintStore(string storeRoot) =>
        Directory.EnumerateFiles(storeRoot, "*", SearchOption.AllDirectories)
            .Select(file =>
            {
                var bytes = File.ReadAllBytes(file);
                var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
                return $"{Path.GetRelativePath(storeRoot, file)}:{bytes.LongLength}:{hash}";
            })
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static HttpRequestMessage CreateQuestionRequest(string language) =>
        new(HttpMethod.Post, "/api/v1/questions")
        {
            Content = JsonContent.Create(new
            {
                corpusId = "database-systems-catalogue-mvp",
                questionLanguage = language,
                question = language == "pt-BR"
                    ? "Qual evidência de persistência está disponível?"
                    : "What persistence evidence is available?",
            }),
        };

    private static async Task<RunningHost> StartHostAsync(
        string contentRoot,
        string storeRoot,
        IEmbeddingProvider? embeddingProvider = null,
        ILanguageModel? languageModel = null)
    {
        if ((embeddingProvider is null) != (languageModel is null))
        {
            throw new ArgumentException(
                "The internal integration provider seam requires both adapters.");
        }

        var app = SetupHost.Build(
        [
            "--environment", IntegrationRuntimeOptions.EnvironmentName,
            "--contentRoot", contentRoot,
            "--urls", "http://127.0.0.1:0",
            $"--{IntegrationRuntimeOptions.EnabledKey}", "true",
            $"--{IntegrationRuntimeOptions.StoreRootKey}", storeRoot,
            "--RagChallenge:Setup:AllowExternalServices", "false",
        ],
        embeddingProvider is null
            ? null
            : services => services.AddSingleton(serviceProvider =>
                new SyntheticIntegrationRuntime(
                    serviceProvider.GetRequiredService<IntegrationRuntimeOptions>(),
                    embeddingProvider,
                    languageModel!)));
        await app.StartAsync();
        var addresses = app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()?.Addresses ??
            throw new InvalidOperationException("The listener address feature is unavailable.");
        var address = new Uri(Assert.Single(addresses));
        Assert.Equal("127.0.0.1", address.Host);
        return new RunningHost(app, address);
    }

    private static async Task StopHostAsync(WebApplication app)
    {
        await app.StopAsync();
        await app.DisposeAsync();
    }

    private sealed record RunningHost(WebApplication Application, Uri BaseAddress);

    private sealed record VisualSelector(
        string IndexGenerationId,
        string RenderManifestId,
        int PageNumber,
        string ImageContentObjectId)
    {
        internal string Path =>
            $"/api/v2/evidence/page-images/{IndexGenerationId}/{RenderManifestId}/" +
            $"{PageNumber}/{ImageContentObjectId}";
    }

    private sealed record VisualResponse(byte[] Bytes, string ETag);

    private sealed class DeterministicEmbeddingProvider(
        EmbeddingProviderDescriptor descriptor) : IEmbeddingProvider
    {
        public Task<EmbeddingBatchResult> EmbedAsync(
            EmbeddingBatchRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var vectors = request.Inputs
                .Select(_ => (ReadOnlyMemory<float>)new float[] { 1, 0, 0 })
                .ToArray();
            return Task.FromResult(new EmbeddingBatchResult(descriptor, vectors));
        }
    }

    private sealed class ControlledLanguageModel(
        LanguageModelDescriptor descriptor) : ILanguageModel
    {
        internal const string SensitiveFailureDetail =
            "injected provider detail must remain private";

        private TaskCompletionSource<object?> cancellationEntered = NewSignal();
        private TaskCompletionSource<object?> cancellationObserved = NewSignal();
        private int nextBehaviour;

        internal CancellationProbe ArmCancellation()
        {
            cancellationEntered = NewSignal();
            cancellationObserved = NewSignal();
            Arm(1);
            return new CancellationProbe(
                cancellationEntered.Task,
                cancellationObserved.Task);
        }

        internal void ArmProviderFailure() => Arm(2);

        public async Task<GroundedGenerationResult> GenerateAsync(
            GroundedGenerationRequest request,
            CancellationToken cancellationToken = default)
        {
            var behaviour = Interlocked.Exchange(ref nextBehaviour, 0);

            if (behaviour == 1)
            {
                cancellationEntered.TrySetResult(null);
                var cancellationWait = NewSignal();
                using var registration = cancellationToken.Register(() =>
                {
                    cancellationObserved.TrySetResult(null);
                    cancellationWait.TrySetCanceled(cancellationToken);
                });
                await cancellationWait.Task.ConfigureAwait(false);
            }

            if (behaviour == 2)
            {
                throw new ProviderStageUnavailableException(
                    "generation",
                    SensitiveFailureDetail);
            }

            cancellationToken.ThrowIfCancellationRequested();
            var evidence = Assert.Single(request.Evidence);
            var answer = request.QuestionLanguage == SupportedQueryLanguage.PtBr
                ? "Resposta sintética fundamentada na evidência persistida."
                : "Synthetic answer grounded in persisted evidence.";
            return new GroundedGenerationResult(
                descriptor,
                request.QuestionLanguage,
                answer,
                [evidence.ChunkId]);
        }

        private static TaskCompletionSource<object?> NewSignal() =>
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        private void Arm(int behaviour)
        {
            if (Interlocked.CompareExchange(ref nextBehaviour, behaviour, 0) != 0)
            {
                throw new InvalidOperationException(
                    "A deterministic provider behaviour is already armed.");
            }
        }
    }

    private sealed record CancellationProbe(Task Entered, Task Observed);
}
