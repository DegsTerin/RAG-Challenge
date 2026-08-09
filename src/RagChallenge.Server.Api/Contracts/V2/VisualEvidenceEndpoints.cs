// Purpose: Serves one fully revalidated same-origin PNG with frozen cache, integrity, concurrency and uniform-failure semantics.
using System.Globalization;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;
using RagChallenge.Server.Api.OperationsGovernance;

namespace RagChallenge.Server.Api.Contracts.V2;

internal static class VisualEvidenceEndpoints
{
    internal const string Route =
        "/api/v2/evidence/page-images/{indexGenerationId}/{renderManifestId}/{pageNumber:int}/{imageContentObjectId}";
    internal const string RateLimitPolicy = "visual-evidence-v2";
    private static readonly TimeSpan Deadline = TimeSpan.FromSeconds(30);

    internal static RouteHandlerBuilder MapVisualEvidenceV2(
        this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(Route, HandleAsync)
            .Produces(StatusCodes.Status200OK, contentType: "image/png")
            .Produces(StatusCodes.Status304NotModified)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .RequireRateLimiting(RateLimitPolicy);

    internal static async Task HandleAsync(
        string indexGenerationId,
        string renderManifestId,
        int pageNumber,
        string imageContentObjectId,
        HttpContext context,
        IVisualEvidenceReader reader,
        VisualEvidenceConcurrencyGate concurrencyGate)
    {
        var correlationId = SanitiseCorrelationId(context.TraceIdentifier);

        if (!TryCreateSelector(
                indexGenerationId,
                renderManifestId,
                pageNumber,
                imageContentObjectId,
                out var selector))
        {
            await Problem(
                VisualEvidenceReadOutcome.NotAvailable,
                correlationId).ExecuteAsync(context).ConfigureAwait(false);
            return;
        }

        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(
            context.RequestAborted);
        deadline.CancelAfter(Deadline);

        if (!await concurrencyGate.TryEnterAsync(deadline.Token).ConfigureAwait(false))
        {
            context.Response.Headers.RetryAfter = "10";
            await Problem(
                VisualEvidenceReadOutcome.Available,
                correlationId,
                rateLimited: true).ExecuteAsync(context).ConfigureAwait(false);
            return;
        }

        try
        {
            var result = await reader.ReadAsync(
                selector,
                DateTimeOffset.UtcNow,
                deadline.Token).ConfigureAwait(false);

            if (result.Outcome != VisualEvidenceReadOutcome.Available ||
                result.Evidence is null)
            {
                await Problem(result.Outcome, correlationId)
                    .ExecuteAsync(context).ConfigureAwait(false);
                return;
            }

            await using var evidence = result.Evidence;
            var etag = $"\"sha256-{evidence.Content.Sha256.Value}\"";
            SetAuthorisedHeaders(context.Response, etag);

            if (IsExactStrongMatch(context.Request.Headers.IfNoneMatch, etag))
            {
                context.Response.StatusCode = StatusCodes.Status304NotModified;
                return;
            }

            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = evidence.MediaType;
            context.Response.ContentLength = evidence.Content.ByteLength;
            await evidence.Content.Content.CopyToAsync(
                context.Response.Body,
                deadline.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            if (!context.Response.HasStarted)
            {
                await Problem(VisualEvidenceReadOutcome.Unavailable, correlationId)
                    .ExecuteAsync(context).ConfigureAwait(false);
            }
        }
        finally
        {
            concurrencyGate.Exit();
        }
    }

    internal static IResult Problem(
        VisualEvidenceReadOutcome outcome,
        string correlationId,
        bool rateLimited = false)
    {
        var (status, code, title, detail) = rateLimited
            ? (
                StatusCodes.Status429TooManyRequests,
                "CH_VISUAL_EVIDENCE_RATE_LIMITED",
                "Visual evidence rate limited",
                "The visual evidence budget is temporarily exhausted.")
            : outcome == VisualEvidenceReadOutcome.NotAvailable
                ? (
                    StatusCodes.Status404NotFound,
                    "CH_VISUAL_EVIDENCE_NOT_AVAILABLE",
                    "Visual evidence not available",
                    "The requested visual evidence is not available.")
                : (
                    StatusCodes.Status503ServiceUnavailable,
                    "CH_VISUAL_EVIDENCE_UNAVAILABLE",
                    "Visual evidence unavailable",
                    "The authorised visual evidence could not be verified safely.");
        var extensions = new Dictionary<string, object?>
        {
            ["code"] = code,
            ["correlationId"] = SanitiseCorrelationId(correlationId),
        };

        if (rateLimited)
        {
            extensions["retryAfterSeconds"] = 10;
        }

        return Results.Problem(
            statusCode: status,
            title: title,
            detail: detail,
            type: $"urn:rag-challenge:problem:{code.ToLowerInvariant()}",
            extensions: extensions);
    }

    private static bool TryCreateSelector(
        string indexGenerationId,
        string renderManifestId,
        int pageNumber,
        string imageContentObjectId,
        out VisualEvidenceSelector selector)
    {
        selector = default!;

        try
        {
            selector = new VisualEvidenceSelector(
                new IndexGenerationId(indexGenerationId),
                new RenderManifestId(renderManifestId),
                pageNumber,
                new ContentObjectId(imageContentObjectId));
            return pageNumber > 0;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static bool IsExactStrongMatch(
        Microsoft.Extensions.Primitives.StringValues values,
        string expected) =>
        values.Count == 1 && string.Equals(values[0], expected, StringComparison.Ordinal);

    private static void SetAuthorisedHeaders(HttpResponse response, string etag)
    {
        response.Headers.ETag = etag;
        response.Headers.CacheControl = "private, no-cache";
        response.Headers.XContentTypeOptions = "nosniff";
        response.Headers["Cross-Origin-Resource-Policy"] = "same-origin";
    }

    private static string SanitiseCorrelationId(string? candidate) =>
        !string.IsNullOrWhiteSpace(candidate) && candidate.Length <= 128 &&
        candidate.All(character =>
            char.IsAsciiLetterOrDigit(character) || character is '-' or '_')
            ? candidate
            : Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
}
