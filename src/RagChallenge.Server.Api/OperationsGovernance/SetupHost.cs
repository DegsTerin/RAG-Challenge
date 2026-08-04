// Purpose: Builds the query host with fail-closed external access, bounded abuse controls and dependency-free health endpoints for local and test composition.
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Server.Api.Contracts.V1;

namespace RagChallenge.Server.Api.OperationsGovernance;

internal static class SetupHost
{
    internal static WebApplication Build(
        string[] args,
        Action<IServiceCollection>? configureServices = null)
    {
        var builder = WebApplication.CreateBuilder(args);

        if (builder.Configuration.GetValue<bool>("RagChallenge:Setup:AllowExternalServices"))
        {
            throw new InvalidOperationException(
                "External services must remain disabled during project setup.");
        }

        builder.Services.AddSingleton(
            new SetupCompositionBoundary(
                typeof(Application.ApplicationAssemblyMarker).Assembly,
                typeof(Infrastructure.InfrastructureAssemblyMarker).Assembly));
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<ApiExceptionHandler>();
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.UnmappedMemberHandling =
                JsonUnmappedMemberHandling.Disallow;
        });
        builder.Services.AddRateLimiter(options =>
        {
            options.AddPolicy(QueryEndpoints.RateLimitPolicy, context =>
                RateLimitPartition.GetTokenBucketLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 10,
                        TokensPerPeriod = 5,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        AutoReplenishment = true,
                    }));
            options.OnRejected = async (context, _) =>
            {
                await QueryEndpoints.Problem(
                    QueryFailureKind.RateLimited,
                    context.HttpContext.TraceIdentifier,
                    retryAfterSeconds: 10).ExecuteAsync(context.HttpContext);
            };
        });
        builder.Services.AddSingleton<QueryConcurrencyGate>();
        builder.Services.AddSingleton<IQuestionAnsweringService,
            DisabledQuestionAnsweringService>();
        builder.Services.AddSingleton<IQueryReadinessProbe,
            DisabledQueryReadinessProbe>();
        configureServices?.Invoke(builder.Services);

        var app = builder.Build();

        app.UseExceptionHandler();
        app.UseRateLimiter();
        app.MapHealthV1();
        app.MapQueryV1();

        return app;
    }
}
