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
        using var response = await httpClient.SendAsync(
            message,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);
        var bytes = await ReadBoundedJsonAsync(
            "embedding",
            response,
            2 * 1024 * 1024,
            cancellationToken)
            .ConfigureAwait(false);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new ProviderStageUnavailableException(
                "embedding",
                "The embedding provider returned a non-success status.");
        }

        try
        {
            using var document = JsonDocument.Parse(bytes);
            var root = document.RootElement;
            var observedModel = root.GetProperty("model").GetString() ??
                throw new JsonException("Embedding model is missing.");
            var data = root.GetProperty("data").EnumerateArray()
                .OrderBy(element => element.GetProperty("index").GetInt32())
                .ToArray();
            var vectors = data.Select(element =>
                (ReadOnlyMemory<float>)element.GetProperty("embedding")
                    .EnumerateArray().Select(value => value.GetSingle()).ToArray()).ToArray();
            return new EmbeddingBatchResult(
                new EmbeddingProviderDescriptor(
                    "openai",
                    observedModel,
                    observedModel,
                    request.ExpectedDescriptor.Dimensions),
                vectors);
        }
        catch (Exception exception) when (
            exception is JsonException or InvalidOperationException or FormatException)
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

    internal static async Task<byte[]> ReadBoundedJsonAsync(
        string stage,
        HttpResponseMessage response,
        int maximumBytes,
        CancellationToken cancellationToken)
    {
        if (response.Headers.Location is not null ||
            response.Content.Headers.ContentType?.MediaType is not "application/json" ||
            response.Content.Headers.ContentLength > maximumBytes)
        {
            throw new ProviderStageUnavailableException(
                stage,
                "The provider response violated HTTP policy.");
        }

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
}

public sealed class OpenAiHttpLanguageModel : ILanguageModel
{
    private static readonly Uri Route = new("/v1/responses", UriKind.Relative);
    private readonly HttpClient httpClient;
    private readonly Func<CancellationToken, ValueTask<string>> credentialSource;
    private readonly LanguageModelDescriptor expectedDescriptor;

    public OpenAiHttpLanguageModel(
        HttpClient httpClient,
        Func<CancellationToken, ValueTask<string>> credentialSource,
        LanguageModelDescriptor expectedDescriptor)
    {
        this.httpClient = OpenAiHttpEmbeddingProvider.ValidateClient(httpClient);
        this.credentialSource = credentialSource ??
            throw new ArgumentNullException(nameof(credentialSource));
        this.expectedDescriptor = expectedDescriptor ??
            throw new ArgumentNullException(nameof(expectedDescriptor));
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
                model = expectedDescriptor.ModelId,
                store = false,
                temperature = 0,
                max_output_tokens = request.MaximumOutputCharacters,
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
        using var response = await httpClient.SendAsync(
            message,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);
        var bytes = await OpenAiHttpEmbeddingProvider.ReadBoundedJsonAsync(
            "generation",
            response,
            2 * 1024 * 1024,
            cancellationToken).ConfigureAwait(false);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new ProviderStageUnavailableException(
                "generation",
                "The language-model provider returned a non-success status.");
        }

        try
        {
            using var document = JsonDocument.Parse(bytes);
            var root = document.RootElement;
            var observedModel = root.GetProperty("model").GetString() ??
                throw new JsonException("Response model is missing.");
            var outputText = root.GetProperty("output").EnumerateArray()
                .SelectMany(item => item.GetProperty("content").EnumerateArray())
                .Single(item => item.GetProperty("type").GetString() == "output_text")
                .GetProperty("text").GetString() ??
                throw new JsonException("Structured output text is missing.");
            using var structured = JsonDocument.Parse(outputText);
            var answerLanguage = structured.RootElement.GetProperty("answerLanguage")
                .GetString() switch
            {
                "pt-BR" => SupportedLanguage.PtBr,
                "en-GB" => SupportedLanguage.EnGb,
                _ => throw new JsonException("Answer language is unsupported."),
            };
            var answer = structured.RootElement.GetProperty("answer").GetString() ??
                throw new JsonException("Answer is missing.");
            var cited = structured.RootElement.GetProperty("citedChunkIds")
                .EnumerateArray().Select(item => item.GetString() ?? string.Empty).ToArray();
            return new GroundedGenerationResult(
                new LanguageModelDescriptor("openai", observedModel, observedModel),
                answerLanguage,
                answer,
                cited);
        }
        catch (Exception exception) when (
            exception is JsonException or InvalidOperationException)
        {
            throw new ProviderStageUnavailableException(
                "generation",
                "The language-model provider response was invalid.");
        }
    }

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
