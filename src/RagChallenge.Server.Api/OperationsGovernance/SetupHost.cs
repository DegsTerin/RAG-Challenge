// Purpose: Builds the setup-only host with fail-closed external access and dependency-free health endpoints for local and test composition.
namespace RagChallenge.Server.Api.OperationsGovernance;

internal static class SetupHost
{
    internal static WebApplication Build(string[] args)
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
        builder.Services.AddHealthChecks();

        var app = builder.Build();

        app.MapHealthChecks("/health/live");
        app.MapHealthChecks("/health/ready");

        return app;
    }
}
