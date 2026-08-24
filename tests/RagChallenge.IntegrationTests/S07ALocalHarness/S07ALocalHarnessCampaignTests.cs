// Purpose: Exposes the single future A3 entry point while failing closed before manifest freeze and explicit run authority; ordinary test runs never enumerate or score candidate cases.
namespace RagChallenge.IntegrationTests.S07ALocalHarness;

public sealed class S07ALocalHarnessCampaignTests
{
    [Fact]
    public async Task ExecuteFrozenCandidateAsync()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable(
                    S07ALocalHarnessDefinition.RunAuthorityEnvironmentVariable),
                S07ALocalHarnessDefinition.RunAuthorityId,
                StringComparison.Ordinal))
        {
            return;
        }

        var repositoryRoot = FindRepositoryRoot();
        var plan = S07ALocalHarnessDefinition.LoadFrozenPlan(repositoryRoot);
        await S07ALocalSyntheticCampaign.ExecuteAsync(plan);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "RAG-Challenge.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "The RAG-Challenge repository root could not be resolved.");
    }
}
