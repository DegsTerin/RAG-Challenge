// Purpose: Maps the bounded anonymous v1 query endpoint, deadline, cancellation and canonical failures while keeping administration and authority fields absent.
using System.Globalization;
using System.Text;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Server.Api.OperationsGovernance;

namespace RagChallenge.Server.Api.Contracts.V1;

internal static class QueryEndpoints
{
    internal const string Route = "/api/v1/questions";
    internal const string RateLimitPolicy = "query-v1";
    internal const int MaximumRequestBytes = 8192;
    private static readonly TimeSpan Deadline = TimeSpan.FromSeconds(25);

    internal static RouteHandlerBuilder MapQueryV1(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(Route, HandleAsync)
            .Accepts<QueryRequestV1>("application/json")
            .Produces<QueryResponseV1>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .RequireRateLimiting(RateLimitPolicy)
            .WithMetadata(new RequestSizeLimitAttribute(MaximumRequestBytes));

    internal static async Task<IResult> HandleAsync(
        QueryRequestV1 request,
        HttpContext httpContext,
        IQuestionAnsweringService service,
        QueryConcurrencyGate concurrencyGate)
    {
        var correlationId = GetCorrelationId(httpContext);

        if (!TryMapRequest(request, correlationId, out var applicationRequest))
        {
            return Problem(QueryFailureKind.InvalidInput, correlationId);
        }

        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(
            httpContext.RequestAborted);
        deadline.CancelAfter(Deadline);

        if (!await concurrencyGate.TryEnterAsync(deadline.Token).ConfigureAwait(false))
        {
            return deadline.IsCancellationRequested
                ? Problem(QueryFailureKind.OperationCancelled, correlationId)
                : Problem(QueryFailureKind.RateLimited, correlationId, retryAfterSeconds: 1);
        }

        try
        {
            var result = await service.AskAsync(
                applicationRequest,
                DateTimeOffset.UtcNow,
                deadline.Token).ConfigureAwait(false);

            if (result.Completion is not null)
            {
                return Results.Ok(QueryContractMapper.ToV1(result.Completion));
            }

            return Problem(result.Failure!.Kind, correlationId);
        }
        catch (OperationCanceledException) when (deadline.IsCancellationRequested)
        {
            return Problem(QueryFailureKind.OperationCancelled, correlationId);
        }
        finally
        {
            concurrencyGate.Exit();
        }
    }

    internal static IResult Problem(
        QueryFailureKind failure,
        string correlationId,
        int? retryAfterSeconds = null)
    {
        correlationId = SanitiseCorrelationId(correlationId);
        var (status, code, title, detail) = failure switch
        {
            QueryFailureKind.InvalidInput => (
                StatusCodes.Status400BadRequest,
                "CH_QUERY_INVALID_INPUT",
                "Invalid query",
                "The query is invalid or outside the supported bounds."),
            QueryFailureKind.CorpusUnavailable => Unavailable(
                "CH_CORPUS_UNAVAILABLE",
                "Corpus unavailable"),
            QueryFailureKind.SourceUnavailable => Unavailable(
                "CH_SOURCE_UNAVAILABLE",
                "Source unavailable"),
            QueryFailureKind.SourceStale => Unavailable(
                "CH_SOURCE_STALE",
                "Source stale"),
            QueryFailureKind.SourcePolicyViolation => Unavailable(
                "CH_SOURCE_POLICY_VIOLATION",
                "Source policy violation"),
            QueryFailureKind.EmbeddingUnavailable => Unavailable(
                "CH_EMBEDDING_UNAVAILABLE",
                "Embedding unavailable"),
            QueryFailureKind.IndexUnavailable => Unavailable(
                "CH_INDEX_UNAVAILABLE",
                "Index unavailable"),
            QueryFailureKind.LanguageModelUnavailable => Unavailable(
                "CH_LANGUAGE_MODEL_UNAVAILABLE",
                "Language model unavailable"),
            QueryFailureKind.ConfigurationInvalid => Unavailable(
                "CH_CONFIGURATION_INVALID",
                "Configuration invalid"),
            QueryFailureKind.OperationCancelled => Unavailable(
                "CH_OPERATION_CANCELLED",
                "Operation cancelled"),
            QueryFailureKind.RateLimited => (
                StatusCodes.Status429TooManyRequests,
                "CH_QUERY_RATE_LIMITED",
                "Query rate limited",
                "The query budget is temporarily exhausted."),
            _ => (
                StatusCodes.Status500InternalServerError,
                "CH_UNEXPECTED_FAILURE",
                "Unexpected failure",
                "The request could not be completed."),
        };
        var extensions = new Dictionary<string, object?>
        {
            ["code"] = code,
            ["correlationId"] = correlationId,
        };

        if (retryAfterSeconds is not null)
        {
            extensions["retryAfterSeconds"] = retryAfterSeconds.Value;
        }

        return Results.Problem(
            statusCode: status,
            title: title,
            detail: detail,
            type: $"urn:rag-challenge:problem:{code.ToLowerInvariant()}",
            extensions: extensions);
    }

    private static bool TryMapRequest(
        QueryRequestV1 request,
        string correlationId,
        out QueryRequest applicationRequest)
    {
        applicationRequest = default!;

        try
        {
            var language = request.QuestionLanguage switch
            {
                "pt-BR" => SupportedLanguage.PtBr,
                "en-GB" => SupportedLanguage.EnGb,
                _ => (SupportedLanguage?)null,
            };

            var question = request.Question?.Trim().Normalize(NormalizationForm.FormC);

            if (language is null || string.IsNullOrWhiteSpace(question) ||
                Encoding.UTF8.GetByteCount(question) > 4096 ||
                question.Any(character => char.IsControl(character) &&
                    character is not '\r' and not '\n' and not '\t'))
            {
                return false;
            }

            applicationRequest = new QueryRequest(
                new CorpusId(request.CorpusId),
                language.Value,
                question,
                correlationId);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static string GetCorrelationId(HttpContext context) =>
        SanitiseCorrelationId(context.TraceIdentifier);

    private static string SanitiseCorrelationId(string? candidate)
    {
        return !string.IsNullOrWhiteSpace(candidate) && candidate.Length <= 128 &&
            candidate.All(character =>
                char.IsAsciiLetterOrDigit(character) || character is '-' or '_')
            ? candidate
            : Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
    }

    private static (int Status, string Code, string Title, string Detail) Unavailable(
        string code,
        string title) =>
        (StatusCodes.Status503ServiceUnavailable, code, title, "The capability is unavailable.");
}
