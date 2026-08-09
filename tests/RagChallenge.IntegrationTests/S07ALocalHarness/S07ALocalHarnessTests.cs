// Purpose: Verifies the S07-A local harness envelope without enumerating, executing, or scoring any candidate case.
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace RagChallenge.IntegrationTests.S07ALocalHarness;

public sealed class S07ALocalHarnessTests
{
    private static readonly string[] EvidenceJsonTestValues = ["alpha", "beta"];

    [Fact]
    public void PreparationEnvelopePinsInputsProvidersPoliciesAndCommands()
    {
        var plan = S07ALocalHarnessDefinition.LoadPreparationPlan(FindRepositoryRoot());

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
    public void WorkspaceIsTaskOwnedAcrossPreparationAndRetainedCampaignPhases()
    {
        var plan = S07ALocalHarnessDefinition.LoadPreparationPlan(FindRepositoryRoot());
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

        if (!plan.IsFrozen)
        {
            Assert.False(Directory.Exists(plan.Workspace.CampaignRoot));
            return;
        }

        if (!Directory.Exists(plan.Workspace.CampaignRoot))
        {
            return;
        }

        var expectedFiles = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["evidence/synthetic-campaign-result.json"] =
                "9efc2eef05388433af58e01242a1b1589556c43620eeec509f583fba0c2073bc",
            ["stores/content/261fbbae23b99e5a0761b92e876cbfe0e7649c009e73b8af12f5aa57a2771097.fixture"] =
                "261fbbae23b99e5a0761b92e876cbfe0e7649c009e73b8af12f5aa57a2771097",
            ["stores/content/5ef95f25d5ebbab2d838ccc8fea69992c78aa5ec4763b45bf0b9e590eb147b23.fixture"] =
                "5ef95f25d5ebbab2d838ccc8fea69992c78aa5ec4763b45bf0b9e590eb147b23",
            ["stores/content/d95cdea5a53a93e732d4b1af3dd9812d78a8c0430fcabb64504acac42b487750.fixture"] =
                "d95cdea5a53a93e732d4b1af3dd9812d78a8c0430fcabb64504acac42b487750",
            ["stores/control/campaign-boundary.json"] =
                "0eee7402dcfb3bb24be6c25bd4d8fea71d6faec672d0ed77fd1a7881f5ce0539",
            ["stores/vectors/fixture-aurora-operations-pt-br-csv.json"] =
                "b88d10d28ac9494f4df7e74a34b9d9a57e4fc0ba43a2f1282a0b469a769fbc1a",
            ["stores/vectors/fixture-beacon-operations-en-gb-pdf.json"] =
                "d4346d34450018cf34027314e2f3dad75801f1b1b9512379db0ca33b694791d5",
            ["stores/vectors/fixture-cinder-operations-en-pdf.json"] =
                "96c7b665fcd79b07504d705c1b0c7774bc912d73238da5b51b4f2111bd33639f",
        };
        var observedFiles = ReadRetainedFiles(plan.Workspace.CampaignRoot);

        Assert.Equal(expectedFiles.OrderBy(entry => entry.Key), observedFiles);
        Assert.All(
            observedFiles.Select(entry => entry.Key),
            relativePath => AssertGitIgnored(
                plan.RepositoryRoot,
                Path.Combine(
                    plan.Workspace.CampaignRoot,
                    relativePath.Replace('/', Path.DirectorySeparatorChar))));
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
    public void EvidenceJsonSerializationUsesDeterministicUtf8LfBytes()
    {
        var bytes = S07ALocalSyntheticCampaign.SerializeEvidenceJson(new
        {
            schemaVersion = 1,
            values = EvidenceJsonTestValues,
        });
        const string expected = "{\n" +
            "  \"schemaVersion\": 1,\n" +
            "  \"values\": [\n" +
            "    \"alpha\",\n" +
            "    \"beta\"\n" +
            "  ]\n" +
            "}\n";

        Assert.Equal(Encoding.UTF8.GetBytes(expected), bytes);
        Assert.DoesNotContain((byte)'\r', bytes);
    }

    [Fact]
    public void FrozenEntryPointMatchesThePreparationPhaseWithoutCaseEnumeration()
    {
        var repositoryRoot = FindRepositoryRoot();
        var preparation = S07ALocalHarnessDefinition.LoadPreparationPlan(repositoryRoot);

        if (preparation.IsFrozen)
        {
            Assert.True(S07ALocalHarnessDefinition.LoadFrozenPlan(repositoryRoot).IsFrozen);
            return;
        }

        var exception = Assert.Throws<InvalidDataException>(() =>
            S07ALocalHarnessDefinition.LoadFrozenPlan(repositoryRoot));
        Assert.Contains("freezeStatus", exception.Message, StringComparison.Ordinal);
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

    private static KeyValuePair<string, string>[] ReadRetainedFiles(
        string campaignRoot)
    {
        var pendingDirectories = new Stack<DirectoryInfo>();
        pendingDirectories.Push(new DirectoryInfo(campaignRoot));
        var files = new Dictionary<string, string>(StringComparer.Ordinal);

        while (pendingDirectories.TryPop(out var directory))
        {
            Assert.False(
                directory.Attributes.HasFlag(FileAttributes.ReparsePoint),
                $"Retained campaign path is a reparse point: {directory.Name}");

            foreach (var entry in directory.EnumerateFileSystemInfos())
            {
                Assert.False(
                    entry.Attributes.HasFlag(FileAttributes.ReparsePoint),
                    $"Retained campaign path is a reparse point: {entry.Name}");

                if (entry is DirectoryInfo childDirectory)
                {
                    pendingDirectories.Push(childDirectory);
                    continue;
                }

                var file = Assert.IsType<FileInfo>(entry);
                var relativePath = Path.GetRelativePath(campaignRoot, file.FullName)
                    .Replace(Path.DirectorySeparatorChar, '/');
                var digest = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file.FullName)))
                    .ToLowerInvariant();
                Assert.True(files.TryAdd(relativePath, digest));
            }
        }

        return files.OrderBy(entry => entry.Key).ToArray();
    }

    private static void AssertGitIgnored(string repositoryRoot, string path)
    {
        var relativePath = Path.GetRelativePath(repositoryRoot, path)
            .Replace(Path.DirectorySeparatorChar, '/');
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = repositoryRoot,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        startInfo.ArgumentList.Add("check-ignore");
        startInfo.ArgumentList.Add("--quiet");
        startInfo.ArgumentList.Add("--no-index");
        startInfo.ArgumentList.Add("--");
        startInfo.ArgumentList.Add(relativePath);

        using var process = Process.Start(startInfo);
        Assert.NotNull(process);
        process.WaitForExit();
        Assert.True(
            process.ExitCode == 0,
            $"Retained campaign evidence is not ignored by Git: {relativePath}");
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
