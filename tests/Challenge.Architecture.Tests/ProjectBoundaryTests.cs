// Purpose: Enforces the accepted project-reference matrix, core isolation, namespace placement, and prohibited dependencies.
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Challenge.Architecture.Tests;

public sealed class ProjectBoundaryTests
{
    public static TheoryData<string, string[]> ApprovedReferences =>
        new()
        {
            { "src/Challenge.Domain/Challenge.Domain.csproj", [] },
            {
                "src/Challenge.Application/Challenge.Application.csproj",
                ["Challenge.Domain"]
            },
            {
                "src/Challenge.Infrastructure/Challenge.Infrastructure.csproj",
                ["Challenge.Application", "Challenge.Domain"]
            },
            {
                "src/Challenge.Server.Api/Challenge.Server.Api.csproj",
                ["Challenge.Application", "Challenge.Infrastructure"]
            },
        };

    [Theory]
    [MemberData(nameof(ApprovedReferences))]
    public void ProductionProjectsFollowTheAcceptedReferenceMatrix(
        string relativeProjectPath,
        string[] expectedReferences)
    {
        var repositoryRoot = RepositoryLayout.FindRoot();
        var projectPath = Path.Combine(
            repositoryRoot,
            relativeProjectPath.Replace('/', Path.DirectorySeparatorChar));
        var project = XDocument.Load(projectPath);

        var actualReferences = project
            .Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => Path.GetFileNameWithoutExtension(value))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            expectedReferences.Order(StringComparer.Ordinal),
            actualReferences);
    }

    [Fact]
    public void CoreAssembliesDoNotReferenceOuterFrameworksOrAdapters()
    {
        Assembly[] coreAssemblies =
        [
            typeof(Domain.DomainAssemblyMarker).Assembly,
            typeof(Application.ApplicationAssemblyMarker).Assembly,
        ];
        string[] prohibitedPrefixes =
        [
            "Challenge.Infrastructure",
            "Challenge.Server.Api",
            "Microsoft.AspNetCore",
            "Microsoft.EntityFrameworkCore",
            "OpenAI",
            "Oracle",
        ];

        foreach (var assembly in coreAssemblies)
        {
            var references = assembly.GetReferencedAssemblies()
                .Select(reference => reference.Name ?? string.Empty)
                .ToArray();

            Assert.DoesNotContain(
                references,
                reference => prohibitedPrefixes.Any(
                    prefix => reference.StartsWith(
                        prefix,
                        StringComparison.OrdinalIgnoreCase)));
        }
    }

    [Fact]
    public void ProductionTypesRemainInsideTheirOwningRootNamespaces()
    {
        var assembliesAndNamespaces = new Dictionary<Assembly, string>
        {
            [typeof(Domain.DomainAssemblyMarker).Assembly] = "Challenge.Domain",
            [typeof(Application.ApplicationAssemblyMarker).Assembly] =
                "Challenge.Application",
            [typeof(Infrastructure.InfrastructureAssemblyMarker).Assembly] =
                "Challenge.Infrastructure",
            [typeof(global::Program).Assembly] = "Challenge.Server.Api",
        };

        foreach (var (assembly, rootNamespace) in assembliesAndNamespaces)
        {
            var misplacedTypes = assembly
                .GetTypes()
                .Where(type => type != typeof(global::Program))
                .Where(type =>
                    type.GetCustomAttribute<CompilerGeneratedAttribute>() is null)
                .Where(type => type.Namespace is null ||
                    !type.Namespace.StartsWith(
                        "Coverlet.Core.Instrumentation.Tracker",
                        StringComparison.Ordinal))
                .Where(type => type.Namespace is null ||
                    !type.Namespace.StartsWith(
                        rootNamespace,
                        StringComparison.Ordinal))
                .Select(type => type.FullName)
                .ToArray();

            Assert.Empty(misplacedTypes);
        }
    }

    [Fact]
    public void RepositoryContainsNoProhibitedProjectsOrDbNotifierReferences()
    {
        var repositoryRoot = RepositoryLayout.FindRoot();
        var projectFiles = Directory
            .EnumerateFiles(
                repositoryRoot,
                "*.csproj",
                SearchOption.AllDirectories)
            .Where(path => !RepositoryLayout.IsGeneratedPath(path))
            .ToArray();

        Assert.DoesNotContain(
            projectFiles,
            path => Path.GetFileNameWithoutExtension(path) is
                "Challenge.Rag.Abstractions" or
                "Challenge.Persistence.Sqlite" or
                "Challenge.Tools.Admin");
        Assert.DoesNotContain(
            projectFiles,
            path => File.ReadAllText(path).Contains(
                "DB-Notifier",
                StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void DashboardDoesNotDeclareProviderOrServerDependencies()
    {
        var repositoryRoot = RepositoryLayout.FindRoot();
        var packageJson = File.ReadAllText(
            Path.Combine(
                repositoryRoot,
                "src",
                "Challenge.Dashboard.Web",
                "package.json"));
        string[] prohibitedTerms =
        [
            "langchain",
            "openai",
            "oracle",
            "sqlite",
            "db-notifier",
        ];

        Assert.DoesNotContain(
            prohibitedTerms,
            term => packageJson.Contains(term, StringComparison.OrdinalIgnoreCase));
    }
}

internal static class RepositoryLayout
{
    internal static string FindRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Challenge.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException(
            "The Challenge repository root could not be located.");
    }

    internal static bool IsGeneratedPath(string path) =>
        path.Contains(
            $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
            StringComparison.OrdinalIgnoreCase) ||
        path.Contains(
            $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
            StringComparison.OrdinalIgnoreCase);
}
