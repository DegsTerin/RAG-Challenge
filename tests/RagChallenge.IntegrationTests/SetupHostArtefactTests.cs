// Purpose: Verifies setup-host configuration and health mappings without starting listeners or contacting external services.
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Server.Api.Contracts.V1;
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
        var administrationEnabled = document.RootElement
            .GetProperty("RagChallenge")
            .GetProperty("Administration")
            .GetProperty("Enabled")
            .GetBoolean();
        var product = document.RootElement
            .GetProperty("RagChallenge")
            .GetProperty("Product");
        var operationalGrants = product.GetProperty("OperationalGrants");
        var administrativeEmbedding = document.RootElement
            .GetProperty("RagChallenge")
            .GetProperty("Administration")
            .GetProperty("ProductMaterialisation")
            .GetProperty("Embedding");

        Assert.False(allowExternalServices);
        Assert.False(administrationEnabled);
        Assert.Equal(
            string.Empty,
            operationalGrants.GetProperty("QueryEmbeddingAuthorityReference").GetString());
        Assert.Equal(
            string.Empty,
            operationalGrants.GetProperty("GroundedGenerationAuthorityReference").GetString());
        Assert.Equal(
            string.Empty,
            administrativeEmbedding.GetProperty("TrustedOperationalGrantReference").GetString());
    }

    [Fact]
    public void IntegrationConfigurationIsNonSecretAndDisabledByDefault()
    {
        var path = Path.Combine(
            FindRepositoryRoot(),
            "src",
            "RagChallenge.Server.Api",
            "appsettings.Integration.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var integration = document.RootElement
            .GetProperty("RagChallenge")
            .GetProperty("Integration");

        Assert.False(integration.GetProperty("Enabled").GetBoolean());
        Assert.Equal(string.Empty, integration.GetProperty("StoreRoot").GetString());
        Assert.DoesNotContain(
            document.RootElement.ToString(),
            "secret",
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            document.RootElement.ToString(),
            "password",
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task VisualEvidenceCompositionIsFailClosedOutsideIntegration()
    {
        await using var defaultApp = SetupHost.Build([]);
        Assert.IsType<DisabledVisualEvidenceReader>(
            defaultApp.Services.GetRequiredService<IVisualEvidenceReader>());

        var taskRoot = Path.Combine(
            Path.GetTempPath(),
            "rag-challenge-state07-v2-composition",
            Guid.NewGuid().ToString("N"));

        try
        {
            await using var integrationApp = SetupHost.Build(
            [
                "--environment", IntegrationRuntimeOptions.EnvironmentName,
                $"--{IntegrationRuntimeOptions.EnabledKey}", "true",
                $"--{IntegrationRuntimeOptions.StoreRootKey}", taskRoot,
                "--RagChallenge:Setup:AllowExternalServices", "false",
            ]);
            var query = Assert.IsType<SyntheticIntegrationRuntime>(
                integrationApp.Services.GetRequiredService<IQuestionAnsweringService>());
            Assert.Same(
                query,
                integrationApp.Services.GetRequiredService<IVisualEvidenceReader>());
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

            if (Directory.Exists(taskRoot))
            {
                Directory.Delete(taskRoot, recursive: true);
            }
        }
    }

    [Theory]
    [InlineData(HealthEndpoints.LivenessRoute)]
    [InlineData(HealthEndpoints.ReadinessRoute)]
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
    public async Task ReadinessFailsClosedUntilQueryCapabilityIsConfigured()
    {
        await using var app = SetupHost.Build([]);
        var probe = app.Services.GetRequiredService<IQueryReadinessProbe>();
        var context = CreateContext(app.Services);
        var result = await HealthEndpoints.ReadyAsync(probe, CancellationToken.None);
        await result.ExecuteAsync(context);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        using var response = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal("Unready", response.RootElement.GetProperty("status").GetString());
        Assert.Equal(
            "unconfigured",
            response.RootElement.GetProperty("configurationRevision").GetString());
    }

    [Fact]
    public async Task ReadinessAcceptsAnExplicitQueryComposition()
    {
        await using var app = SetupHost.Build(
            [],
            services => services.AddSingleton<IQueryReadinessProbe, ReadyQueryProbe>());
        var probe = app.Services.GetRequiredService<IQueryReadinessProbe>();
        var context = CreateContext(app.Services);
        var result = await HealthEndpoints.ReadyAsync(probe, CancellationToken.None);
        await result.ExecuteAsync(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        using var response = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal("Degraded", response.RootElement.GetProperty("status").GetString());
        Assert.Equal(1, response.RootElement.GetProperty("eligibleDocumentCount").GetInt32());
    }

    [Fact]
    public void SetupHostFailsClosedWhenExternalServicesLackProductRuntime()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => SetupHost.Build(
                ["--RagChallenge:Setup:AllowExternalServices=true"]));

        Assert.Equal(
            "External services must be enabled exactly for the explicit product runtime.",
            exception.Message);
    }

    [Fact]
    public void SetupHostRejectsIntegrationRuntimeOutsideIntegrationEnvironment()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => SetupHost.Build(
        [
            "--environment", "Development",
            $"--{IntegrationRuntimeOptions.EnabledKey}", "true",
        ]));

        Assert.Equal(
            "The synthetic integration runtime requires the Integration environment.",
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

    [Fact]
    public void OciRehearsalBuilderRequiresTheRestoredOfflineArm64Boundary()
    {
        var serverRoot = Path.Combine(
            FindRepositoryRoot(),
            "src",
            "RagChallenge.Server.Api");
        var builder = File.ReadAllText(Path.Combine(
            serverRoot,
            "Build-OciRehearsalArtifact.ps1"));
        var verifier = File.ReadAllText(Path.Combine(
            serverRoot,
            "Test-OciRehearsalArtifact.ps1"));
        var scripts = builder + "\n" + verifier;

        Assert.Contains("net10.0/linux-arm64", builder, StringComparison.Ordinal);
        Assert.Contains("--runtime linux-arm64", builder, StringComparison.Ordinal);
        Assert.Contains("--self-contained true", builder, StringComparison.Ordinal);
        Assert.Contains("--no-restore", builder, StringComparison.Ordinal);
        Assert.Contains(
            "artifacts-local/s06-oci-rehearsal",
            builder,
            StringComparison.Ordinal);
        Assert.Contains("artifact-manifest.sha256", scripts, StringComparison.Ordinal);
        Assert.Contains("ZipFile]::OpenRead", verifier, StringComparison.Ordinal);
        Assert.Contains("LinuxArm64Executed = $false", scripts, StringComparison.Ordinal);
        Assert.DoesNotMatch(
            new Regex(
                @"(?im)^\s*(?:oci|docker|podman|curl)(?:[.]exe)?\b|Invoke-(?:WebRequest|RestMethod)|System[.]Net[.]Http"),
            scripts);
    }

    [Fact]
    public void OciRehearsalVerifierRequiresAarch64DashboardAndFailClosedConfiguration()
    {
        var verifier = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "src",
            "RagChallenge.Server.Api",
            "Test-OciRehearsalArtifact.ps1"));

        Assert.Contains("$machine -ne 183", verifier, StringComparison.Ordinal);
        Assert.Contains("libe_sqlite3.so", verifier, StringComparison.Ordinal);
        Assert.Contains("wwwroot/index.html", verifier, StringComparison.Ordinal);
        Assert.Contains("runtimes/win-", verifier, StringComparison.Ordinal);
        Assert.Contains("AllowExternalServices", verifier, StringComparison.Ordinal);
        Assert.Contains("ExternalServicesEnabledByDefault = $false", verifier, StringComparison.Ordinal);
        Assert.Contains("OciContacted = $false", verifier, StringComparison.Ordinal);
    }

    [Fact]
    public void IntegrationProviderSeamIsInternalAndAbsentFromConfiguration()
    {
        var runtimeType = typeof(SyntheticIntegrationRuntime);
        Assert.False(runtimeType.IsPublic);
        Assert.Empty(runtimeType.GetConstructors());
        Assert.Equal(
            2,
            runtimeType.GetConstructors(
                BindingFlags.Instance | BindingFlags.NonPublic).Length);

        var serverRoot = Path.Combine(
            FindRepositoryRoot(),
            "src",
            "RagChallenge.Server.Api");
        var committedConfiguration = string.Join(
            "\n",
            Directory.EnumerateFiles(serverRoot, "appsettings*.json")
                .OrderBy(path => path, StringComparer.Ordinal)
                .Select(File.ReadAllText));

        Assert.DoesNotContain("\"Fault", committedConfiguration, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"Failure", committedConfiguration, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ProductScriptsAreCommittedBoundedAndMigrationAware()
    {
        var engRoot = Path.Combine(FindRepositoryRoot(), "eng");
        var generator = File.ReadAllText(Path.Combine(
            engRoot,
            "New-Oracle19ProductPlans.ps1"));
        var launcher = File.ReadAllText(Path.Combine(
            engRoot,
            "Start-Oracle19Product.ps1"));
        var postgreSqlLauncher = File.ReadAllText(Path.Combine(
            engRoot,
            "Start-PostgreSql18Product.ps1"));

        Assert.Contains(
            "tests/RagChallenge.UnitTests/TestData/initial-catalogue-v1.json",
            generator.Replace('\\', '/'),
            StringComparison.Ordinal);
        Assert.Contains("'oracle-database'", generator, StringComparison.Ordinal);
        Assert.Contains("status = 'Candidate'", generator, StringComparison.Ordinal);
        Assert.DoesNotContain("status = 'Active'", generator, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "owner-oracle19-public-source-approval-2026-08-12",
            generator,
            StringComparison.Ordinal);
        Assert.DoesNotContain("render-oracle-document.json", generator, StringComparison.Ordinal);
        Assert.DoesNotContain("build-oracle-index.json", generator, StringComparison.Ordinal);
        Assert.Contains(
            "$ApprovedRightsEvidenceReference -ceq $supersededUnverifiedRightsEvidenceReference",
            launcher,
            StringComparison.Ordinal);
        Assert.DoesNotContain(".env.local", launcher, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Get-Content", launcher, StringComparison.Ordinal);
        Assert.Contains(
            "$env:RagChallenge__Product__ApplyMigrations = 'true'",
            launcher,
            StringComparison.Ordinal);
        Assert.Contains(
            "$env:RagChallenge__Product__CatalogueProfile = 'oracle-database-19c'",
            launcher,
            StringComparison.Ordinal);
        Assert.Contains(
            "$env:RagChallenge__Product__ApprovedRightsEvidenceReference =",
            launcher,
            StringComparison.Ordinal);
        Assert.Contains(
            "$env:RagChallenge__Product__QueryEmbeddingAuthorityReference",
            launcher,
            StringComparison.Ordinal);
        Assert.Contains(
            "$env:RagChallenge__Product__GroundedGenerationAuthorityReference",
            launcher,
            StringComparison.Ordinal);
        Assert.Contains("AUTH-QUERY-EMBEDDING-", launcher, StringComparison.Ordinal);
        Assert.Contains("AUTH-GROUNDED-GENERATION-", launcher, StringComparison.Ordinal);
        Assert.DoesNotContain("OperationalGrants", launcher, StringComparison.Ordinal);
        Assert.Contains(
            "SetEnvironmentVariable(",
            launcher,
            StringComparison.Ordinal);
        Assert.Contains("$credentialName = 'OPENAI_API' + '_KEY'", launcher, StringComparison.Ordinal);
        Assert.DoesNotContain("GetEnvironmentVariable", launcher, StringComparison.Ordinal);
        Assert.Contains(
            "artifacts-local/state-07/product-materialisation/postgresql-18-reference-a4/product-store",
            postgreSqlLauncher.Replace('\\', '/'),
            StringComparison.Ordinal);
        Assert.Contains(
            "$env:RagChallenge__Product__CatalogueProfile = 'postgresql-18.4'",
            postgreSqlLauncher,
            StringComparison.Ordinal);
        Assert.Contains(
            "$approvedRightsEvidenceReference = 'auth-s07-a-product-a0-003'",
            postgreSqlLauncher,
            StringComparison.Ordinal);
        Assert.DoesNotContain(".env.local", postgreSqlLauncher, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("QueryEmbeddingAuthorityReference", postgreSqlLauncher, StringComparison.Ordinal);
        Assert.Contains("GroundedGenerationAuthorityReference", postgreSqlLauncher, StringComparison.Ordinal);
        Assert.DoesNotContain("OperationalGrants", postgreSqlLauncher, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "corpus/oracle-database",
            postgreSqlLauncher.Replace('\\', '/'),
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "sk-",
            generator + launcher + postgreSqlLauncher,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PublishedHarnessDoesNotCancelInitialReadinessBeforeItsDeadline()
    {
        var harness = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "src",
            "RagChallenge.Server.Api",
            "Test-IntegrationArtifact.ps1"));

        Assert.Contains("$readinessTimeoutSeconds = 30", harness, StringComparison.Ordinal);
        Assert.Contains(
            "AddSeconds($readinessTimeoutSeconds)",
            harness,
            StringComparison.Ordinal);
        Assert.Contains(
            "-TimeoutSec $remainingReadinessSeconds",
            harness,
            StringComparison.Ordinal);
        Assert.DoesNotContain("-TimeoutSec 2", harness, StringComparison.Ordinal);
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

    private static DefaultHttpContext CreateContext(IServiceProvider services)
    {
        var context = new DefaultHttpContext
        {
            RequestServices = services,
        };
        context.Response.Body = new MemoryStream();
        return context;
    }

    private sealed class ReadyQueryProbe : IQueryReadinessProbe
    {
        public ValueTask<ReadinessV1> CheckAsync(
            DateTimeOffset observedAt,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(new ReadinessV1(
                "Degraded",
                ActiveDatabaseCount: 1,
                EligibleDocumentCount: 1,
                DegradedDocumentCount: 1,
                SourceStates: [new SanitisedSourceStateV1("source-1", "Stale")],
                ActiveGenerationId: "idxgen-synthetic",
                ConfigurationRevision: "configuration-1",
                Checks: [new SanitisedCapabilityCheckV1("query-runtime", "Available")],
                observedAt));
    }
}
