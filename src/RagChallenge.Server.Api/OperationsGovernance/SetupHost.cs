// Purpose: Builds the query host with fail-closed external access, bounded abuse controls and dependency-free health endpoints for local and test composition.
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Server.Api.Contracts.V1;
using V2Endpoints = RagChallenge.Server.Api.Contracts.V2;

namespace RagChallenge.Server.Api.OperationsGovernance;

internal static class SetupHost
{
    internal static WebApplication Build(
        string[] args,
        Action<IServiceCollection>? configureServices = null)
    {
        var builder = WebApplication.CreateBuilder(args);
        var productOptions = ProductQueryRuntimeOptions.Resolve(builder.Configuration);

        var allowExternalServices = builder.Configuration.GetValue<bool>(
            "RagChallenge:Setup:AllowExternalServices");
        if (allowExternalServices != (productOptions is not null))
        {
            throw new InvalidOperationException(
                "External services must be enabled exactly for the explicit product runtime.");
        }

        var integrationOptions = IntegrationRuntimeOptions.Resolve(
            builder.Configuration,
            builder.Environment);
        if (integrationOptions is not null && productOptions is not null)
        {
            throw new InvalidOperationException(
                "Product and synthetic integration runtimes cannot be enabled together.");
        }

        builder.Services.AddSingleton(
            new SetupCompositionBoundary(
                typeof(Application.ApplicationAssemblyMarker).Assembly,
                typeof(Infrastructure.InfrastructureAssemblyMarker).Assembly));
        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
                context.ProblemDetails.Extensions.Remove("traceId");
        });
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
            options.AddPolicy(V2Endpoints.VisualEvidenceEndpoints.RateLimitPolicy, context =>
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
                if (context.HttpContext.Request.Path.StartsWithSegments(
                    "/api/v2/evidence/page-images"))
                {
                    context.HttpContext.Response.Headers.RetryAfter = "10";
                    await V2Endpoints.VisualEvidenceEndpoints.Problem(
                        VisualEvidenceReadOutcome.Available,
                        context.HttpContext.TraceIdentifier,
                        rateLimited: true).ExecuteAsync(context.HttpContext);
                }
                else
                {
                    await QueryEndpoints.Problem(
                        QueryFailureKind.RateLimited,
                        context.HttpContext.TraceIdentifier,
                        retryAfterSeconds: 10).ExecuteAsync(context.HttpContext);
                }
            };
        });
        builder.Services.AddSingleton<QueryConcurrencyGate>();
        builder.Services.AddSingleton<VisualEvidenceConcurrencyGate>();
        if (integrationOptions is null && productOptions is null)
        {
            builder.Services.AddSingleton<IVisualEvidenceReader, DisabledVisualEvidenceReader>();
            builder.Services.AddSingleton<IQuestionAnsweringService,
                DisabledQuestionAnsweringService>();
            builder.Services.AddSingleton<IQueryReadinessProbe,
                DisabledQueryReadinessProbe>();
        }
        else if (integrationOptions is not null)
        {
            builder.Services.AddSingleton(integrationOptions);
            builder.Services.AddSingleton(services => new SyntheticIntegrationRuntime(
                services.GetRequiredService<IntegrationRuntimeOptions>(),
                new SanitisedAnswerEvidenceActivitySink(
                    services.GetRequiredService<ILogger<SanitisedAnswerEvidenceActivitySink>>())));
            builder.Services.AddSingleton<IQuestionAnsweringService>(services =>
                services.GetRequiredService<SyntheticIntegrationRuntime>());
            builder.Services.AddSingleton<IQueryReadinessProbe>(services =>
                services.GetRequiredService<SyntheticIntegrationRuntime>());
            builder.Services.AddSingleton<IVisualEvidenceReader>(services =>
                services.GetRequiredService<SyntheticIntegrationRuntime>());
        }
        else
        {
            builder.Services.AddSingleton(productOptions!);
            builder.Services.AddSingleton(services => new ProductQueryRuntime(
                services.GetRequiredService<ProductQueryRuntimeOptions>(),
                new SanitisedAnswerEvidenceActivitySink(
                    services.GetRequiredService<ILogger<SanitisedAnswerEvidenceActivitySink>>()),
                services.GetRequiredService<ILogger<ProductQueryRuntime>>()));
            builder.Services.AddSingleton<IQuestionAnsweringService>(services =>
                services.GetRequiredService<ProductQueryRuntime>());
            builder.Services.AddSingleton<IQueryReadinessProbe>(services =>
                services.GetRequiredService<ProductQueryRuntime>());
            builder.Services.AddSingleton<IVisualEvidenceReader>(services =>
                services.GetRequiredService<ProductQueryRuntime>());
        }
        configureServices?.Invoke(builder.Services);

        var app = builder.Build();

        app.UseExceptionHandler();
        if (integrationOptions is not null || productOptions is not null)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }

        app.UseRateLimiter();
        app.MapHealthV1();
        app.MapQueryV1();
        V2Endpoints.QueryEndpoints.MapQueryV2(app);
        V2Endpoints.VisualEvidenceEndpoints.MapVisualEvidenceV2(app);

        if (integrationOptions is not null || productOptions is not null)
        {
            app.MapFallbackToFile("index.html");
        }

        return app;
    }
}
