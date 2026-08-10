// Purpose: Implements the approved direct-HTTP OpenAI embedding and grounded-response adapters with exact routes, bounded JSON and no SDK, retry, redirect, proxy or provider-owned state.
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

using RagChallenge.Application.IndexingRetrieval;
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

public sealed class OpenAiHttpEmbeddingProvider : IEmbeddingProvider
{
    private static readonly Uri Route = new("/v1/embeddings", UriKind.Relative);
    private readonly HttpClient httpClient;
    private readonly Func<CancellationToken, ValueTask<string>> credentialSource;

    public OpenAiHttpEmbeddingProvider(
        HttpClient httpClient,
        Func<CancellationToken, ValueTask<string>> credentialSource)
    {
        this.httpClient = ValidateClient(httpClient);
        this.credentialSource = credentialSource ??
            throw new ArgumentNullException(nameof(credentialSource));
    }

    public async Task<EmbeddingBatchResult> EmbedAsync(
        EmbeddingBatchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var key = await ReadCredentialAsync("embedding", credentialSource, cancellationToken)
            .ConfigureAwait(false);
        using var message = new HttpRequestMessage(HttpMethod.Post, Route)
        {
            Content = JsonContent.Create(new
            {
                model = request.ExpectedDescriptor.ModelId,
                input = request.Inputs,
                dimensions = request.ExpectedDescriptor.Dimensions,
                encoding_format = "float",
            }),
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
        using var response = await SendAsync(
            "embedding",
            httpClient,
            message,
            cancellationToken).ConfigureAwait(false);
        var bytes = await ReadBoundedJsonAsync(
            "embedding",
            response,
            2 * 1024 * 1024,
            cancellationToken)
            .ConfigureAwait(false);

        try
        {
            using var document = JsonDocument.Parse(bytes);
            var root = document.RootElement;
            var observedModel = ReadRequiredString(root, "model");

            if (!string.Equals(
                    observedModel,
                    request.ExpectedDescriptor.ModelRevision,
                    StringComparison.Ordinal))
            {
                throw new JsonException("Embedding model revision is unexpected.");
            }

            if (!root.TryGetProperty("data", out var dataElement) ||
                dataElement.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException("Embedding data is missing.");
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
                    .Select(value => value.GetSingle())
                    .ToArray();

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

            return new EmbeddingBatchResult(
                request.ExpectedDescriptor,
                vectors);
        }
        catch (Exception exception) when (
            exception is JsonException or InvalidOperationException or FormatException or
                OverflowException)
        {
            throw new ProviderStageUnavailableException(
                "embedding",
                "The embedding provider response was invalid.");
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
                "The provider credential is unavailable.");
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
                "The provider request exceeded its time budget.");
        }
        catch (HttpRequestException)
        {
            throw new ProviderStageUnavailableException(
                stage,
                "The provider transport is unavailable.");
        }
    }

    internal static async Task<byte[]> ReadBoundedJsonAsync(
        string stage,
        HttpResponseMessage response,
        int maximumBytes,
        CancellationToken cancellationToken)
    {
        if (response.StatusCode != HttpStatusCode.OK ||
            response.Headers.Location is not null ||
            !string.Equals(
                response.Content.Headers.ContentType?.MediaType,
                "application/json",
                StringComparison.OrdinalIgnoreCase) ||
            response.Content.Headers.ContentLength > maximumBytes)
        {
            throw new ProviderStageUnavailableException(
                stage,
                "The provider response violated HTTP policy.");
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
                        "The provider response exceeded its byte limit.");
                }

                output.Write(buffer, 0, read);
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ProviderStageUnavailableException(
                stage,
                "The provider response exceeded its time budget.");
        }
        catch (HttpRequestException)
        {
            throw new ProviderStageUnavailableException(
                stage,
                "The provider response transport is unavailable.");
        }
        catch (IOException)
        {
            throw new ProviderStageUnavailableException(
                stage,
                "The provider response could not be read.");
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

    public OpenAiHttpLanguageModel(
        HttpClient httpClient,
        Func<CancellationToken, ValueTask<string>> credentialSource,
        LanguageModelDescriptor expectedDescriptor)
        : this(
            httpClient,
            credentialSource,
            new OpenAiLanguageModelOptions(
                expectedDescriptor,
                OpenAiReasoningEffort.None,
                OpenAiReasoningContext.CurrentTurn))
    {
    }

    public OpenAiHttpLanguageModel(
        HttpClient httpClient,
        Func<CancellationToken, ValueTask<string>> credentialSource,
        OpenAiLanguageModelOptions options)
    {
        this.httpClient = OpenAiHttpEmbeddingProvider.ValidateClient(httpClient);
        this.credentialSource = credentialSource ??
            throw new ArgumentNullException(nameof(credentialSource));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<GroundedGenerationResult> GenerateAsync(
        GroundedGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var key = await OpenAiHttpEmbeddingProvider.ReadCredentialAsync(
            "generation",
            credentialSource,
            cancellationToken).ConfigureAwait(false);
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
        using var message = new HttpRequestMessage(HttpMethod.Post, Route)
        {
            Content = JsonContent.Create(new
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
            }),
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
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

        try
        {
            using var document = JsonDocument.Parse(bytes);
            var root = document.RootElement;
            var observedModel = OpenAiHttpEmbeddingProvider.ReadRequiredString(root, "model");

            if (!string.Equals(
                    observedModel,
                    options.ExpectedDescriptor.ModelRevision,
                    StringComparison.Ordinal) ||
                OpenAiHttpEmbeddingProvider.ReadRequiredString(root, "status") != "completed" ||
                !root.TryGetProperty("output", out var outputElement) ||
                outputElement.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException("The response model or output is unexpected.");
            }

            var outputItems = outputElement.EnumerateArray().ToArray();

            if (outputItems.Length != 1 ||
                OpenAiHttpEmbeddingProvider.ReadRequiredString(outputItems[0], "type") != "message" ||
                OpenAiHttpEmbeddingProvider.ReadRequiredString(outputItems[0], "role") != "assistant" ||
                OpenAiHttpEmbeddingProvider.ReadRequiredString(outputItems[0], "status") != "completed" ||
                !outputItems[0].TryGetProperty("content", out var contentElement) ||
                contentElement.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException("The structured response output is invalid.");
            }

            var contentItems = contentElement.EnumerateArray().ToArray();

            if (contentItems.Length != 1 ||
                OpenAiHttpEmbeddingProvider.ReadRequiredString(contentItems[0], "type") !=
                    "output_text")
            {
                throw new JsonException("The structured response content is invalid.");
            }

            var outputText = OpenAiHttpEmbeddingProvider.ReadRequiredString(
                contentItems[0],
                "text");
            using var structured = JsonDocument.Parse(outputText);
            var structuredRoot = structured.RootElement;
            var propertyNames = structuredRoot.ValueKind == JsonValueKind.Object
                ? structuredRoot.EnumerateObject()
                    .Select(property => property.Name)
                    .ToHashSet(StringComparer.Ordinal)
                : new HashSet<string>(StringComparer.Ordinal);

            if (!propertyNames.SetEquals(
                    ["answerLanguage", "answer", "citedChunkIds"]))
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
            var answer = OpenAiHttpEmbeddingProvider.ReadRequiredString(
                structuredRoot,
                "answer");

            if (answerLanguage != request.QuestionLanguage ||
                string.IsNullOrWhiteSpace(answer) ||
                answer.Length > request.MaximumOutputCharacters ||
                !structuredRoot.TryGetProperty("citedChunkIds", out var citedElement) ||
                citedElement.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException("The structured answer values are invalid.");
            }

            var cited = citedElement.EnumerateArray()
                .Select(item => item.ValueKind == JsonValueKind.String
                    ? item.GetString()
                    : null)
                .ToArray();
            var allowedChunkIds = request.Evidence
                .Select(item => item.ChunkId)
                .ToHashSet(StringComparer.Ordinal);

            if (cited.Any(item => string.IsNullOrWhiteSpace(item) ||
                    item!.Length > 128 || item.Any(char.IsControl) ||
                    !allowedChunkIds.Contains(item)) ||
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
                "The language-model provider response was invalid.");
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
