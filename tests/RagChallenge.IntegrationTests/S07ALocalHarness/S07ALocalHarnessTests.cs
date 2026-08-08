// Purpose: Verifies the S07-A local harness envelope without enumerating, executing, or scoring any candidate case.
using System.Net;
using System.Text.Json;

namespace RagChallenge.IntegrationTests.S07ALocalHarness;

public sealed class S07ALocalHarnessTests
{
    [Fact]
    public void CandidateEnvelopePinsInputsProvidersPoliciesAndCommands()
    {
        var plan = S07ALocalHarnessDefinition.LoadCandidatePlan(FindRepositoryRoot());

        Assert.Equal(
            S07ALocalHarnessDefinition.DatasetRevision,
            ReadString(plan.DatasetManifestPath, "datasetRevision"));
        Assert.Equal("deterministic-local", plan.EmbeddingProvider.ProviderId);
        Assert.Equal("token-hash-embedding-v1", plan.EmbeddingProvider.ModelId);
        Assert.Equal(64, plan.EmbeddingProvider.Dimensions);
        Assert.Equal("deterministic-local", plan.LanguageModel.ProviderId);
        Assert.Equal("evidence-template-v1", plan.LanguageModel.ModelId);
        Assert.Equal(S07ALocalHarnessDefinition.EnvironmentId, plan.EnvironmentId);
        Assert.Equal(S07ALocalHarnessDefinition.NetworkPolicyId, plan.NetworkPolicyId);
        Assert.Equal(S07ALocalHarnessDefinition.StorePolicyId, plan.StorePolicyId);
        Assert.Equal(
            S07ALocalHarnessDefinition.ExactValidationCommand,
            plan.ValidationCommand);
        Assert.Equal(
            S07ALocalHarnessDefinition.ExactFutureA3Command,
            plan.FutureA3Command);
    }

    [Fact]
    public void WorkspaceIsTaskOwnedAndDoesNotExistDuringPreparation()
    {
        var plan = S07ALocalHarnessDefinition.LoadCandidatePlan(FindRepositoryRoot());
        var allowedRoot = Path.GetFullPath(Path.Combine(
            plan.RepositoryRoot,
            "artifacts-local",
            "state-07",
            "s07-a"));
        var relative = Path.GetRelativePath(allowedRoot, plan.Workspace.CampaignRoot);

        Assert.False(Path.IsPathFullyQualified(relative));
        Assert.False(relative.StartsWith("..", StringComparison.Ordinal));
        Assert.EndsWith(
            Path.Combine("stores", "control"),
            plan.Workspace.ControlStoreRoot,
            StringComparison.Ordinal);
        Assert.EndsWith(
            Path.Combine("stores", "vectors"),
            plan.Workspace.VectorStoreRoot,
            StringComparison.Ordinal);
        Assert.EndsWith(
            Path.Combine("stores", "content"),
            plan.Workspace.ContentStoreRoot,
            StringComparison.Ordinal);
        Assert.False(Directory.Exists(plan.Workspace.CampaignRoot));
    }

    [Fact]
    public async Task NetworkPolicyRejectsWithoutUsingTransport()
    {
        using var client = S07ALocalHarnessDefinition.CreateDenyAllHttpClient();

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            client.GetAsync(new Uri("https://network-must-remain-denied.invalid/")));

        Assert.Equal(HttpRequestError.ConnectionError, exception.HttpRequestError);
        Assert.Contains(
            S07ALocalHarnessDefinition.NetworkPolicyId,
            exception.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void FrozenEntryPointRejectsTheCandidateBeforeCaseEnumeration()
    {
        var exception = Assert.Throws<InvalidDataException>(() =>
            S07ALocalHarnessDefinition.LoadFrozenPlan(FindRepositoryRoot()));

        Assert.Contains("a2OrLaterExecuted", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CampaignEntryPointIsNotAuthorisedByDefault()
    {
        Assert.NotEqual(
            S07ALocalHarnessDefinition.RunAuthorityId,
            Environment.GetEnvironmentVariable(
                S07ALocalHarnessDefinition.RunAuthorityEnvironmentVariable));
        Assert.Contains(
            "-Mode Run -AuthorityId AUTH-S07-A-RUN-001",
            S07ALocalHarnessDefinition.ExactFutureA3Command,
            StringComparison.Ordinal);
    }

    private static string ReadString(string path, string propertyName)
    {
        using var document = JsonDocument.Parse(File.ReadAllBytes(path));
        return document.RootElement.GetProperty(propertyName).GetString()!;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "RAG-Challenge.sln")) &&
                File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "The RAG-Challenge repository root could not be resolved.");
    }
}
