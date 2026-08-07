// Purpose: Enforces the accepted project-reference matrix, core isolation, namespace placement, and prohibited dependencies.
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace RagChallenge.Architecture.Tests;

public sealed class ProjectBoundaryTests
{
    public static TheoryData<string, string[]> ApprovedReferences =>
        new()
        {
            { "src/RagChallenge.Domain/RagChallenge.Domain.csproj", [] },
            {
                "src/RagChallenge.Application/RagChallenge.Application.csproj",
                ["RagChallenge.Domain"]
            },
            {
                "src/RagChallenge.Infrastructure/RagChallenge.Infrastructure.csproj",
                ["RagChallenge.Application", "RagChallenge.Domain"]
            },
            {
                "src/RagChallenge.Server.Api/RagChallenge.Server.Api.csproj",
                ["RagChallenge.Application", "RagChallenge.Infrastructure"]
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
            "RagChallenge.Infrastructure",
            "RagChallenge.Server.Api",
            "Microsoft.AspNetCore",
            "Microsoft.EntityFrameworkCore",
            "OpenAI",
            "Oracle",
            "PDFtoImage",
            "SkiaSharp",
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

    [Theory]
    [InlineData("src/RagChallenge.Domain/RagChallenge.Domain.csproj")]
    [InlineData("src/RagChallenge.Application/RagChallenge.Application.csproj")]
    public void CoreProjectsDeclareNoPackageOrPersistenceDependency(
        string relativeProjectPath)
    {
        var repositoryRoot = RepositoryLayout.FindRoot();
        var projectPath = Path.Combine(
            repositoryRoot,
            relativeProjectPath.Replace('/', Path.DirectorySeparatorChar));
        var project = XDocument.Load(projectPath);
        var content = File.ReadAllText(projectPath);

        Assert.Empty(project.Descendants("PackageReference"));
        Assert.DoesNotContain(
            "EntityFramework",
            content,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQLite", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ProductionTypesRemainInsideTheirOwningRootNamespaces()
    {
        var assembliesAndNamespaces = new Dictionary<Assembly, string>
        {
            [typeof(Domain.DomainAssemblyMarker).Assembly] = "RagChallenge.Domain",
            [typeof(Application.ApplicationAssemblyMarker).Assembly] =
                "RagChallenge.Application",
            [typeof(Infrastructure.InfrastructureAssemblyMarker).Assembly] =
                "RagChallenge.Infrastructure",
            [typeof(global::Program).Assembly] = "RagChallenge.Server.Api",
        };

        foreach (var (assembly, rootNamespace) in assembliesAndNamespaces)
        {
            var misplacedTypes = assembly
                .GetTypes()
                .Where(type => type != typeof(global::Program))
                .Where(type =>
                    type.GetCustomAttribute<CompilerGeneratedAttribute>() is null)
                .Where(type =>
                    type.DeclaringType?.GetCustomAttribute<CompilerGeneratedAttribute>()
                        is null)
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
                "RagChallenge.Rag.Abstractions" or
                "RagChallenge.Persistence.Sqlite" or
                "RagChallenge.Tools.Admin");
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
                "RagChallenge.Dashboard.Web",
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
            if (File.Exists(Path.Combine(current.FullName, "RAG-Challenge.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException(
            "The RAG-Challenge repository root could not be located.");
    }

    internal static bool IsGeneratedPath(string path) =>
        path.Contains(
            $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
            StringComparison.OrdinalIgnoreCase) ||
        path.Contains(
            $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
            StringComparison.OrdinalIgnoreCase);
}
