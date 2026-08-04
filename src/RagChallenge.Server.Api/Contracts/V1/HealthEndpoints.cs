// Purpose: Exposes dependency-free liveness and sanitised readiness without calling providers or revealing internal endpoints, paths or failures.
using RagChallenge.Server.Api.OperationsGovernance;

namespace RagChallenge.Server.Api.Contracts.V1;

internal static class HealthEndpoints
{
    internal const string LivenessRoute = "/api/v1/health/live";
    internal const string ReadinessRoute = "/api/v1/health/ready";

    internal static void MapHealthV1(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(LivenessRoute, () => Results.Ok(new LivenessV1("Live")))
            .Produces<LivenessV1>(StatusCodes.Status200OK, "application/json");
        endpoints.MapGet(ReadinessRoute, ReadyAsync)
            .Produces<ReadinessV1>(StatusCodes.Status200OK, "application/json")
            .Produces<ReadinessV1>(StatusCodes.Status503ServiceUnavailable, "application/json");
    }

    internal static async Task<IResult> ReadyAsync(
        IQueryReadinessProbe probe,
        CancellationToken cancellationToken)
    {
        var snapshot = await probe.CheckAsync(
            DateTimeOffset.UtcNow,
            cancellationToken).ConfigureAwait(false);
        var status = snapshot.Status is "Ready" or "Degraded"
            ? StatusCodes.Status200OK
            : StatusCodes.Status503ServiceUnavailable;
        return Results.Json(snapshot, statusCode: status);
    }
}
