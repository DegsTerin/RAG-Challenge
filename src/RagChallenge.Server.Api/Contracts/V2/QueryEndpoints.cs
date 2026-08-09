// Purpose: Maps the frozen anonymous v2 query surface with the existing abuse budget, strict request bounds, bounded response and v2-only language projection.
using System.Globalization;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Server.Api.OperationsGovernance;

namespace RagChallenge.Server.Api.Contracts.V2;

internal static class QueryEndpoints
{
    internal const string Route = "/api/v2/questions";
    internal const int MaximumRequestBytes = 8192;
    internal const int MaximumResponseBytes = 262_144;
    private static readonly JsonSerializerOptions ResponseJsonOptions =
        new(JsonSerializerDefaults.Web);
    private static readonly TimeSpan Deadline = TimeSpan.FromSeconds(25);

    internal static RouteHandlerBuilder MapQueryV2(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(Route, HandleAsync)
            .Accepts<QueryRequestV2>("application/json")
            .Produces<QueryResponseV2>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .RequireRateLimiting(Contracts.V1.QueryEndpoints.RateLimitPolicy)
            .WithMetadata(new RequestSizeLimitAttribute(MaximumRequestBytes));

    internal static async Task<IResult> HandleAsync(
        QueryRequestV2? request,
        HttpContext httpContext,
        IQuestionAnsweringService service,
        QueryConcurrencyGate concurrencyGate)
    {
        var correlationId = SanitiseCorrelationId(httpContext.TraceIdentifier);

        if (!TryMapRequest(request, correlationId, out var applicationRequest))
        {
            return Contracts.V1.QueryEndpoints.Problem(
                QueryFailureKind.InvalidInput,
                correlationId);
        }

        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(
            httpContext.RequestAborted);
        deadline.CancelAfter(Deadline);

        if (!await concurrencyGate.TryEnterAsync(deadline.Token).ConfigureAwait(false))
        {
            return deadline.IsCancellationRequested
                ? Contracts.V1.QueryEndpoints.Problem(
                    QueryFailureKind.OperationCancelled,
                    correlationId)
                : Contracts.V1.QueryEndpoints.Problem(
                    QueryFailureKind.RateLimited,
                    correlationId,
                    retryAfterSeconds: 1);
        }

        try
        {
            var result = await service.AskAsync(
                applicationRequest,
                DateTimeOffset.UtcNow,
                deadline.Token).ConfigureAwait(false);

            if (result.Completion is null)
            {
                return Contracts.V1.QueryEndpoints.Problem(
                    result.Failure!.Kind,
                    correlationId);
            }

            var bytes = JsonSerializer.SerializeToUtf8Bytes(
                QueryContractMapper.ToV2(result.Completion),
                ResponseJsonOptions);
            return bytes.Length <= MaximumResponseBytes
                ? Results.Bytes(bytes, "application/json; charset=utf-8")
                : Contracts.V1.QueryEndpoints.Problem(
                    QueryFailureKind.UnexpectedFailure,
                    correlationId);
        }
        catch (OperationCanceledException) when (deadline.IsCancellationRequested)
        {
            return Contracts.V1.QueryEndpoints.Problem(
                QueryFailureKind.OperationCancelled,
                correlationId);
        }
        finally
        {
            concurrencyGate.Exit();
        }
    }

    private static bool TryMapRequest(
        QueryRequestV2? request,
        string correlationId,
        out QueryRequest applicationRequest)
    {
        applicationRequest = default!;

        if (request is null)
        {
            return false;
        }

        try
        {
            var language = request.QuestionLanguage switch
            {
                "pt-BR" => SupportedQueryLanguage.PtBr,
                "en-GB" => SupportedQueryLanguage.EnGb,
                _ => (SupportedQueryLanguage?)null,
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
                correlationId,
                ContractVersion: QueryContractVersion.V2);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static string SanitiseCorrelationId(string? candidate) =>
        !string.IsNullOrWhiteSpace(candidate) && candidate.Length <= 128 &&
        candidate.All(character =>
            char.IsAsciiLetterOrDigit(character) || character is '-' or '_')
            ? candidate
            : Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
}
