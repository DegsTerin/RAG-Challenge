// Purpose: Verifies setup-host configuration and health mappings without starting listeners or contacting external services.
using System.Text.Json;

using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

using RagChallenge.Server.Api.OperationsGovernance;

namespace RagChallenge.IntegrationTests;

public sealed class SetupHostArtefactTests
{
    [Fact]
    public void ExternalServicesAreDisabledInCommittedConfiguration()
    {
        var repositoryRoot = FindRepositoryRoot();
        var appSettingsPath = Path.Combine(
            repositoryRoot,
            "src",
            "RagChallenge.Server.Api",
            "appsettings.json");
        using var document = JsonDocument.Parse(File.ReadAllText(appSettingsPath));

        var allowExternalServices = document.RootElement
            .GetProperty("RagChallenge")
            .GetProperty("Setup")
            .GetProperty("AllowExternalServices")
            .GetBoolean();

        Assert.False(allowExternalServices);
    }

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task SetupHostMapsDependencyFreeHealthEndpoints(string route)
    {
        await using var app = SetupHost.Build([]);
        var mappedRoutes = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(endpoint => endpoint.RoutePattern.RawText)
            .ToArray();

        Assert.Contains(route, mappedRoutes);
    }

    [Fact]
    public void SetupHostFailsClosedWhenExternalServicesAreEnabled()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => SetupHost.Build(
                ["--RagChallenge:Setup:AllowExternalServices=true"]));

        Assert.Equal(
            "External services must remain disabled during project setup.",
            exception.Message);
    }

    [Fact]
    public async Task SetupHostComposesOnlyTheApprovedInwardAssemblies()
    {
        await using var app = SetupHost.Build([]);
        var boundary =
            app.Services.GetRequiredService<SetupCompositionBoundary>();

        Assert.Same(
            typeof(Application.ApplicationAssemblyMarker).Assembly,
            boundary.ApplicationAssembly);
        Assert.Same(
            typeof(Infrastructure.InfrastructureAssemblyMarker).Assembly,
            boundary.InfrastructureAssembly);
        Assert.Contains(
            typeof(Application.ApplicationAssemblyMarker).Assembly,
            Infrastructure.InfrastructureAssemblyMarker.ReferencedCoreAssemblies);
        Assert.Contains(
            typeof(Domain.DomainAssemblyMarker).Assembly,
            Infrastructure.InfrastructureAssemblyMarker.ReferencedCoreAssemblies);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "RAG-Challenge.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException(
            "The RAG-Challenge repository root could not be located.");
    }
}
