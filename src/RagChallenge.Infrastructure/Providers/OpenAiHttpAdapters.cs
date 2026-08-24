// Purpose: Implements direct-HTTP OpenAI embedding and grounded-response adapters behind durable operation-scoped budget admission, exact routes, bounded JSON and no SDK, retry, redirect, proxy or provider-owned state.
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.ProviderBudget;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Infrastructure.Providers;

public static class OpenAiHttpClientPolicy
{
    public static SocketsHttpHandler CreateDenyByDefaultHandler() =>
        new()
        {
            AllowAutoRedirect = false,
            UseProxy = false,
            AutomaticDecompression = DecompressionMethods.None,
            ConnectTimeout = TimeSpan.FromSeconds(10),
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        };
}

public enum OpenAiReasoningEffort
{
    None,
}

public enum OpenAiReasoningContext
{
    CurrentTurn,
}

public sealed class OpenAiLanguageModelOptions
{
    public const string MvpModelId = "gpt-5.4-mini-2026-03-17";

    public OpenAiLanguageModelOptions(
        LanguageModelDescriptor expectedDescriptor,
        OpenAiReasoningEffort reasoningEffort,
        OpenAiReasoningContext reasoningContext)
    {
        ExpectedDescriptor = expectedDescriptor ??
            throw new ArgumentNullException(nameof(expectedDescriptor));

        if (!string.Equals(expectedDescriptor.ProviderId, "openai", StringComparison.Ordinal) ||
            !string.Equals(expectedDescriptor.ModelId, MvpModelId, StringComparison.Ordinal) ||
            !string.Equals(expectedDescriptor.ModelRevision, MvpModelId, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The OpenAI language-model options require the accepted dated MVP snapshot.",
                nameof(expectedDescriptor));
        }

        ReasoningEffortApiValue = reasoningEffort switch
        {
            OpenAiReasoningEffort.None => "none",
            _ => throw new ArgumentOutOfRangeException(nameof(reasoningEffort)),
        };
        ReasoningContextApiValue = reasoningContext switch
        {
            OpenAiReasoningContext.CurrentTurn => "current_turn",
            _ => throw new ArgumentOutOfRangeException(nameof(reasoningContext)),
        };
        ReasoningEffort = reasoningEffort;
        ReasoningContext = reasoningContext;
    }

    public LanguageModelDescriptor ExpectedDescriptor { get; }

    public OpenAiReasoningEffort ReasoningEffort { get; }

    public OpenAiReasoningContext ReasoningContext { get; }

    internal string ReasoningEffortApiValue { get; }

    internal string ReasoningContextApiValue { get; }
}

public sealed class OpenAiHttpEmbeddingProvider : IEmbeddingProvider, IEmbeddingProviderPlanValidator
{
    private static readonly Uri Route = new("/v1/embeddings", UriKind.Relative);
    private readonly HttpClient httpClient;
    private readonly Func<CancellationToken, ValueTask<string>> credentialSource;
    private readonly ProviderBudgetAdmissionGate budgetAdmissionGate;
    private readonly ProviderBudgetOperationClass operationClass;
    private readonly OpenAiEmbeddingPlanPolicy? planPolicy;
    private readonly Func<CancellationToken, Task>? prepareBudget;

    public OpenAiHttpEmbeddingProvider(
        HttpClient httpClient,
        Func<CancellationToken, ValueTask<string>> credentialSource,
        ProviderBudgetAdmissionGate budgetAdmissionGate,
        ProviderBudgetOperationClass operationClass,
        OpenAiEmbeddingPlanPolicy? planPolicy = null,
        Func<CancellationToken, Task>? prepareBudget = null)
    {
        this.httpClient = ValidateClient(httpClient);
        this.credentialSource = credentialSource ??
            throw new ArgumentNullException(nameof(credentialSource));
        this.budgetAdmissionGate = budgetAdmissionGate ??
            throw new ArgumentNullException(nameof(budgetAdmissionGate));
        this.operationClass = operationClass is
            ProviderBudgetOperationClass.AdministrativeIndexEmbedding or
            ProviderBudgetOperationClass.QueryEmbedding
                ? operationClass
                : throw new ArgumentOutOfRangeException(nameof(operationClass));
        this.planPolicy = planPolicy;
        this.prepareBudget = prepareBudget;
    }

    public async Task ValidatePlanAsync(
        IReadOnlyCollection<EmbeddingBatchRequest> requests,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requests);
        planPolicy?.Validate(requests);
        var exactRequests = requests.Select(SerialiseRequest).ToArray();

        if (planPolicy is not null)
        {
            long calculatedMaximum = 0;
            foreach (var exactRequest in exactRequests)
            {
                calculatedMaximum = checked(calculatedMaximum +
                    OpenAiEmbeddingCostSchedule.CalculateMaximumMicroUsd(
                        exactRequest.Length));
            }

            if (calculatedMaximum > planPolicy.MaximumTotalMicroUsd)
            {
                throw new ProviderBudgetAdmissionUnavailableException();
            }
        }

        if (prepareBudget is not null)
        {
            await prepareBudget(cancellationToken).ConfigureAwait(false);
        }

        var admittedMaximum = await budgetAdmissionGate.ValidatePlanAsync(
            operationClass,
            exactRequests.Select(bytes => (ReadOnlyMemory<byte>)bytes).ToArray(),
            cancellationToken).ConfigureAwait(false);

        if (planPolicy is not null && admittedMaximum.Value > planPolicy.MaximumTotalMicroUsd)
        {
            throw new ProviderBudgetAdmissionUnavailableException();
        }
    }

    public async Task<EmbeddingBatchResult> EmbedAsync(
        EmbeddingBatchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var exactRequestBytes = SerialiseRequest(request);
        var lease = await budgetAdmissionGate.AdmitAsync(
            operationClass,
            exactRequestBytes,
            cancellationToken).ConfigureAwait(false);
        string key;

        try
        {
            key = await ReadCredentialAsync("embedding", credentialSource, cancellationToken)
                .ConfigureAwait(false);
        }
        catch
        {
            await lease.ReleaseConfirmedZeroRequestBytesAsync(CancellationToken.None)
                .ConfigureAwait(false);
            throw;
        }

        using var message = new HttpRequestMessage(HttpMethod.Post, Route)
        {
            Content = new ByteArrayContent(exactRequestBytes),
        };
        message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await lease.MarkDispatchStartedAsync(cancellationToken).ConfigureAwait(false);
            using var response = await SendAsync(
                "embedding",
                httpClient,
                message,
                cancellationToken).ConfigureAwait(false);
            var bytes = await ReadBoundedJsonAsync(
                "embedding",
                response,
                2 * 1024 * 1024,
                cancellationToken).ConfigureAwait(false);
            var result = ParseEmbeddingResult(request, bytes);
            stopwatch.Stop();
            await lease.CommitObservedMaximumAsync(
                "EMBEDDING_OK",
                stopwatch.Elapsed,
                CancellationToken.None).ConfigureAwait(false);
            return result;
        }
        catch (Exception) when (!lease.IsTerminal)
        {
            if (lease.DispatchStarted)
            {
                await lease.CommitIndeterminateMaximumAsync(
                    "EMBEDDING_INDETERMINATE",
                    CancellationToken.None).ConfigureAwait(false);
            }
            else
            {
                await lease.ReleaseConfirmedZeroRequestBytesAsync(CancellationToken.None)
                    .ConfigureAwait(false);
            }

            throw;
        }
    }

    internal static byte[] SerialiseRequest(EmbeddingBatchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return JsonSerializer.SerializeToUtf8Bytes(new
        {
            model = request.ExpectedDescriptor.ModelId,
            input = request.Inputs,
            dimensions = request.ExpectedDescriptor.Dimensions,
            encoding_format = "float",
        });
    }

    private static EmbeddingBatchResult ParseEmbeddingResult(
        EmbeddingBatchRequest request,
        byte[] bytes)
    {
        try
        {
            using var document = JsonDocument.Parse(bytes);
            var root = document.RootElement;
            var observedModel = ReadRequiredString(root, "model");

            if (!string.Equals(observedModel, request.ExpectedDescriptor.ModelRevision,
                    StringComparison.Ordinal) ||
                !root.TryGetProperty("data", out var dataElement) ||
                dataElement.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException("Embedding response is unexpected.");
            }

            var data = dataElement.EnumerateArray().ToArray();
            if (data.Length != request.Inputs.Count)
            {
                throw new JsonException("Embedding result count is unexpected.");
            }

            var vectors = new ReadOnlyMemory<float>[data.Length];
            var seen = new bool[data.Length];
            foreach (var element in data)
            {
                if (element.ValueKind != JsonValueKind.Object ||
                    !element.TryGetProperty("index", out var indexElement) ||
                    !indexElement.TryGetInt32(out var index) ||
                    index < 0 || index >= data.Length || seen[index] ||
                    !element.TryGetProperty("embedding", out var embeddingElement) ||
                    embeddingElement.ValueKind != JsonValueKind.Array)
                {
                    throw new JsonException("Embedding indexes are invalid.");
                }

                var values = embeddingElement.EnumerateArray()
                    .Select(value => value.GetSingle()).ToArray();
                if (values.Length != request.ExpectedDescriptor.Dimensions ||
                    values.Any(value => !float.IsFinite(value)))
                {
                    throw new JsonException("Embedding dimensions are invalid.");
                }

                seen[index] = true;
                vectors[index] = values;
            }

            if (seen.Any(value => !value))
            {
                throw new JsonException("Embedding indexes are incomplete.");
            }

            return new EmbeddingBatchResult(request.ExpectedDescriptor, vectors);
        }
        catch (Exception exception) when (
            exception is JsonException or InvalidOperationException or FormatException or
                OverflowException)
        {
            throw new ProviderStageUnavailableException(
                "embedding",
                "The embedding provider response was invalid.",
                "invalid-response");
        }
    }

    internal static HttpClient ValidateClient(HttpClient client)
    {
        ArgumentNullException.ThrowIfNull(client);

        if (client.BaseAddress is null ||
            !string.Equals(
                client.BaseAddress.AbsoluteUri,
                "https://api.openai.com/",
                StringComparison.Ordinal) ||
            client.Timeout <= TimeSpan.Zero ||
            client.Timeout > TimeSpan.FromSeconds(25))
        {
            throw new ArgumentException(
                "The OpenAI client requires the exact approved HTTPS authority and a total timeout no greater than 25 seconds.",
                nameof(client));
        }

        return client;
    }

    internal static async ValueTask<string> ReadCredentialAsync(
        string stage,
        Func<CancellationToken, ValueTask<string>> source,
        CancellationToken cancellationToken)
    {
        var key = await source(cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(key) || key.Length > 4096 ||
            key.Any(char.IsControl))
        {
            throw new ProviderStageUnavailableException(
                stage,
                "The provider credential is unavailable.",
                "credential-unavailable");
        }

        return key;
    }

    internal static async Task<HttpResponseMessage> SendAsync(
        string stage,
        HttpClient client,
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ProviderStageUnavailableException(
                stage,
                "The provider request exceeded its time budget.",
                "request-timeout");
        }
        catch (HttpRequestException)
        {
            throw new ProviderStageUnavailableException(
                stage,
                "The provider transport is unavailable.",
                "request-transport");
        }
    }

    internal static async Task<byte[]> ReadBoundedJsonAsync(
        string stage,
        HttpResponseMessage response,
        int maximumBytes,
        CancellationToken cancellationToken)
    {
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new ProviderStageUnavailableException(
                stage,
                "The provider response violated HTTP policy.",
                $"http-status-{(int)response.StatusCode}");
        }

        if (response.Headers.Location is not null)
        {
            throw new ProviderStageUnavailableException(
                stage,
                "The provider response violated HTTP policy.",
                "redirect");
        }

        if (!string.Equals(
                response.Content.Headers.ContentType?.MediaType,
                "application/json",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ProviderStageUnavailableException(
                stage,
                "The provider response violated HTTP policy.",
                "content-type");
        }

        if (response.Content.Headers.ContentLength > maximumBytes)
        {
            throw new ProviderStageUnavailableException(
                stage,
                "The provider response violated HTTP policy.",
                "response-size");
        }

        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);
            using var output = new MemoryStream();
            var buffer = new byte[16 * 1024];

            while (true)
            {
                var read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);

                if (read == 0)
                {
                    return output.ToArray();
                }

                if (output.Length + read > maximumBytes)
                {
                    throw new ProviderStageUnavailableException(
                        stage,
                        "The provider response exceeded its byte limit.",
                        "response-size");
                }

                output.Write(buffer, 0, read);
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ProviderStageUnavailableException(
                stage,
                "The provider response exceeded its time budget.",
                "response-timeout");
        }
        catch (HttpRequestException)
        {
            throw new ProviderStageUnavailableException(
                stage,
                "The provider response transport is unavailable.",
                "response-transport");
        }
        catch (IOException)
        {
            throw new ProviderStageUnavailableException(
                stage,
                "The provider response could not be read.",
                "response-read");
        }
    }

    internal static string ReadRequiredString(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object ||
            !element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String)
        {
            throw new JsonException("A required provider field is missing.");
        }

        return property.GetString() ??
            throw new JsonException("A required provider field is null.");
    }
}

public sealed class OpenAiHttpLanguageModel : ILanguageModel
{
    private static readonly Uri Route = new("/v1/responses", UriKind.Relative);
    private readonly HttpClient httpClient;
    private readonly Func<CancellationToken, ValueTask<string>> credentialSource;
    private readonly OpenAiLanguageModelOptions options;
    private readonly ProviderBudgetAdmissionGate budgetAdmissionGate;
    private readonly Func<CancellationToken, Task>? prepareBudget;

    public OpenAiHttpLanguageModel(
        HttpClient httpClient,
        Func<CancellationToken, ValueTask<string>> credentialSource,
        LanguageModelDescriptor expectedDescriptor,
        ProviderBudgetAdmissionGate budgetAdmissionGate,
        Func<CancellationToken, Task>? prepareBudget = null)
        : this(
            httpClient,
            credentialSource,
            new OpenAiLanguageModelOptions(
                expectedDescriptor,
                OpenAiReasoningEffort.None,
                OpenAiReasoningContext.CurrentTurn),
            budgetAdmissionGate,
            prepareBudget)
    {
    }

    public OpenAiHttpLanguageModel(
        HttpClient httpClient,
        Func<CancellationToken, ValueTask<string>> credentialSource,
        OpenAiLanguageModelOptions options,
        ProviderBudgetAdmissionGate budgetAdmissionGate,
        Func<CancellationToken, Task>? prepareBudget = null)
    {
        this.httpClient = OpenAiHttpEmbeddingProvider.ValidateClient(httpClient);
        this.credentialSource = credentialSource ??
            throw new ArgumentNullException(nameof(credentialSource));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.budgetAdmissionGate = budgetAdmissionGate ??
            throw new ArgumentNullException(nameof(budgetAdmissionGate));
        this.prepareBudget = prepareBudget;
    }

    public async Task<GroundedGenerationResult> GenerateAsync(
        GroundedGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var evidence = request.Evidence.Select(item => new
        {
            chunkId = item.ChunkId,
            contentLanguage = item.ContentLanguage.ToCanonicalTag(),
            text = item.Text,
        }).ToArray();
        var userPayload = JsonSerializer.Serialize(new
        {
            questionLanguage = request.QuestionLanguage.ToCanonicalTag(),
            question = request.Question,
            evidence,
        });
        var payload = new
        {
            model = options.ExpectedDescriptor.ModelId,
            store = false,
            max_output_tokens = ConvertCharacterBudgetToTokenBudget(
                request.MaximumOutputCharacters),
            reasoning = new
            {
                effort = options.ReasoningEffortApiValue,
                context = options.ReasoningContextApiValue,
            },
            input = new object[]
            {
                new
                {
                    role = "developer",
                    content = new[] { new { type = "input_text", text = request.TrustedInstructions } },
                },
                new
                {
                    role = "user",
                    content = new[] { new { type = "input_text", text = userPayload } },
                },
            },
            text = new
            {
                format = new
                {
                    type = "json_schema",
                    name = "grounded_answer",
                    strict = true,
                    schema = ResponseSchema,
                },
            },
        };
        var exactRequestBytes = JsonSerializer.SerializeToUtf8Bytes(payload);
        if (prepareBudget is not null)
        {
            await prepareBudget(cancellationToken).ConfigureAwait(false);
        }

        var lease = await budgetAdmissionGate.AdmitAsync(
            ProviderBudgetOperationClass.GroundedGeneration,
            exactRequestBytes,
            cancellationToken).ConfigureAwait(false);
        string key;

        try
        {
            key = await OpenAiHttpEmbeddingProvider.ReadCredentialAsync(
                "generation",
                credentialSource,
                cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await lease.ReleaseConfirmedZeroRequestBytesAsync(CancellationToken.None)
                .ConfigureAwait(false);
            throw;
        }

        using var message = new HttpRequestMessage(HttpMethod.Post, Route)
        {
            Content = new ByteArrayContent(exactRequestBytes),
        };
        message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await lease.MarkDispatchStartedAsync(cancellationToken).ConfigureAwait(false);
            using var response = await OpenAiHttpEmbeddingProvider.SendAsync(
                "generation",
                httpClient,
                message,
                cancellationToken).ConfigureAwait(false);
            var bytes = await OpenAiHttpEmbeddingProvider.ReadBoundedJsonAsync(
                "generation",
                response,
                2 * 1024 * 1024,
                cancellationToken).ConfigureAwait(false);
            var result = ParseGenerationResult(request, bytes);
            stopwatch.Stop();
            await lease.CommitObservedMaximumAsync(
                "GENERATION_OK",
                stopwatch.Elapsed,
                CancellationToken.None).ConfigureAwait(false);
            return result;
        }
        catch (Exception) when (!lease.IsTerminal)
        {
            if (lease.DispatchStarted)
            {
                await lease.CommitIndeterminateMaximumAsync(
                    "GENERATION_INDETERMINATE",
                    CancellationToken.None).ConfigureAwait(false);
            }
            else
            {
                await lease.ReleaseConfirmedZeroRequestBytesAsync(CancellationToken.None)
                    .ConfigureAwait(false);
            }

            throw;
        }
    }

    private GroundedGenerationResult ParseGenerationResult(
        GroundedGenerationRequest request,
        byte[] bytes)
    {
        try
        {
            using var document = JsonDocument.Parse(bytes);
            var root = document.RootElement;
            var observedModel = OpenAiHttpEmbeddingProvider.ReadRequiredString(root, "model");
            if (!string.Equals(observedModel, options.ExpectedDescriptor.ModelRevision,
                    StringComparison.Ordinal) ||
                OpenAiHttpEmbeddingProvider.ReadRequiredString(root, "status") != "completed" ||
                !root.TryGetProperty("output", out var outputElement) ||
                outputElement.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException("The response model or output is unexpected.");
            }

            var outputItems = outputElement.EnumerateArray().Select(item => new
            {
                Element = item,
                Type = OpenAiHttpEmbeddingProvider.ReadRequiredString(item, "type"),
            }).ToArray();
            var messageItems = outputItems.Where(item => item.Type == "message").ToArray();
            if (outputItems.Any(item => item.Type is not ("reasoning" or "message")) ||
                messageItems.Length != 1 ||
                OpenAiHttpEmbeddingProvider.ReadRequiredString(messageItems[0].Element, "role") != "assistant" ||
                OpenAiHttpEmbeddingProvider.ReadRequiredString(messageItems[0].Element, "status") != "completed" ||
                !messageItems[0].Element.TryGetProperty("content", out var contentElement) ||
                contentElement.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException("The structured response output is invalid.");
            }

            var contentItems = contentElement.EnumerateArray().ToArray();
            if (contentItems.Length != 1 ||
                OpenAiHttpEmbeddingProvider.ReadRequiredString(contentItems[0], "type") != "output_text")
            {
                throw new JsonException("The structured response content is invalid.");
            }

            var outputText = OpenAiHttpEmbeddingProvider.ReadRequiredString(contentItems[0], "text");
            using var structured = JsonDocument.Parse(outputText);
            var structuredRoot = structured.RootElement;
            var propertyNames = structuredRoot.ValueKind == JsonValueKind.Object
                ? structuredRoot.EnumerateObject().Select(property => property.Name)
                    .ToHashSet(StringComparer.Ordinal)
                : new HashSet<string>(StringComparer.Ordinal);
            if (!propertyNames.SetEquals(["answerLanguage", "answer", "citedChunkIds"]))
            {
                throw new JsonException("The structured answer schema is invalid.");
            }

            var answerLanguage = OpenAiHttpEmbeddingProvider.ReadRequiredString(
                structuredRoot,
                "answerLanguage") switch
            {
                "pt-BR" => SupportedQueryLanguage.PtBr,
                "en-GB" => SupportedQueryLanguage.EnGb,
                _ => throw new JsonException("Answer language is unsupported."),
            };
            var answer = OpenAiHttpEmbeddingProvider.ReadRequiredString(structuredRoot, "answer");
            if (answerLanguage != request.QuestionLanguage || string.IsNullOrWhiteSpace(answer) ||
                answer.Length > request.MaximumOutputCharacters ||
                !structuredRoot.TryGetProperty("citedChunkIds", out var citedElement) ||
                citedElement.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException("The structured answer values are invalid.");
            }

            var cited = citedElement.EnumerateArray()
                .Select(item => item.ValueKind == JsonValueKind.String ? item.GetString() : null)
                .ToArray();
            var allowedChunkIds = request.Evidence.Select(item => item.ChunkId)
                .ToHashSet(StringComparer.Ordinal);
            if (cited.Any(item => string.IsNullOrWhiteSpace(item) || item!.Length > 128 ||
                    item.Any(char.IsControl) || !allowedChunkIds.Contains(item)) ||
                cited.Distinct(StringComparer.Ordinal).Count() != cited.Length)
            {
                throw new JsonException("The structured citations are invalid.");
            }

            return new GroundedGenerationResult(
                options.ExpectedDescriptor,
                answerLanguage,
                answer,
                cited.Select(item => item!).ToArray());
        }
        catch (Exception exception) when (
            exception is JsonException or InvalidOperationException or FormatException)
        {
            throw new ProviderStageUnavailableException(
                "generation",
                "The language-model provider response was invalid.",
                "invalid-response");
        }
    }

    internal static int ConvertCharacterBudgetToTokenBudget(int maximumOutputCharacters) =>
        Math.Clamp(
            checked((maximumOutputCharacters + 1) / 2),
            256,
            8192);

    private static readonly object ResponseSchema = new
    {
        type = "object",
        additionalProperties = false,
        required = new[] { "answerLanguage", "answer", "citedChunkIds" },
        properties = new
        {
            answerLanguage = new { type = "string", @enum = new[] { "pt-BR", "en-GB" } },
            answer = new { type = "string" },
            citedChunkIds = new { type = "array", items = new { type = "string" } },
        },
    };
}
