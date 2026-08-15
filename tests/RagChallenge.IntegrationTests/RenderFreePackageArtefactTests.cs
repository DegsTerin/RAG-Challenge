// Purpose: Verifies that the local Render Free package remains private, free-only, fail-closed and separate from external publication.
namespace RagChallenge.IntegrationTests;

public sealed class RenderFreePackageArtefactTests
{
    [Fact]
    public void RenderTemplateDeclaresOnlyOneFreeImageService()
    {
        var template = ReadRepositoryFile(
            "deploy",
            "render-free",
            "render.yaml.template");

        Assert.Contains("runtime: image", template, StringComparison.Ordinal);
        Assert.Contains("plan: free", template, StringComparison.Ordinal);
        Assert.Contains("numInstances: 1", template, StringComparison.Ordinal);
        Assert.Contains("autoDeployTrigger: off", template, StringComparison.Ordinal);
        Assert.Contains(
            "<private-image-reference-by-digest>",
            template,
            StringComparison.Ordinal);
        Assert.DoesNotContain("disk:", template, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("databases:", template, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("scaling:", template, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("plan: starter", template, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("plan: standard", template, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("plan: pro", template, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RenderTemplateKeepsTheProductConfigurationTypedAndTheSecretOpaque()
    {
        var template = ReadRepositoryFile(
            "deploy",
            "render-free",
            "render.yaml.template");

        Assert.Contains(
            "RagChallenge__Product__CatalogueProfile",
            template,
            StringComparison.Ordinal);
        Assert.Contains("postgresql-18.4", template, StringComparison.Ordinal);
        Assert.Contains(
            "RagChallenge__Product__ApprovedRightsEvidenceReference",
            template,
            StringComparison.Ordinal);
        Assert.Contains("auth-s07-a-product-a0-003", template, StringComparison.Ordinal);
        Assert.Contains(
            "RagChallenge__Product__ApplyMigrations",
            template,
            StringComparison.Ordinal);
        Assert.Contains("- key: OPENAI_API_KEY", template, StringComparison.Ordinal);
        Assert.Contains("sync: false", template, StringComparison.Ordinal);
        Assert.Contains("RagChallenge__Product__QueryEmbeddingAuthorityReference", template, StringComparison.Ordinal);
        Assert.Contains("RagChallenge__Product__GroundedGenerationAuthorityReference", template, StringComparison.Ordinal);
        Assert.Contains("<replace-with-AUTH-QUERY-EMBEDDING-reference>", template, StringComparison.Ordinal);
        Assert.Contains(
            "RagChallenge__Product__OperationalGrants__QueryEmbeddingAuthorityReference",
            template,
            StringComparison.Ordinal);
        Assert.Contains(
            "RagChallenge__Product__OperationalGrants__GroundedGenerationAuthorityReference",
            template,
            StringComparison.Ordinal);
        Assert.DoesNotContain("sk-", template, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ContainerUsesAPinnedRuntimeAndAnUnprivilegedVerifiedSeedBoundary()
    {
        var dockerfile = ReadRepositoryFile("deploy", "render-free", "Dockerfile");
        var entrypoint = ReadRepositoryFile("deploy", "render-free", "entrypoint.sh");

        Assert.Matches(
            "aspnet:10[.]0[.]11@sha256:[0-9a-f]{64}",
            dockerfile);
        Assert.Contains("USER app", dockerfile, StringComparison.Ordinal);
        Assert.Contains(
            "chmod -R a-w /opt/rag-challenge/seed",
            dockerfile,
            StringComparison.Ordinal);
        Assert.Contains(
            "sha256sum -c seed-manifest.sha256",
            entrypoint,
            StringComparison.Ordinal);
        Assert.DoesNotContain("OPENAI_API_KEY", entrypoint, StringComparison.Ordinal);
        Assert.DoesNotContain("CredentialEnvironmentVariable", entrypoint, StringComparison.Ordinal);
        Assert.Contains(
            "RagChallenge__Product__StoreRoot",
            entrypoint,
            StringComparison.Ordinal);
        Assert.DoesNotMatch(
            "(?im)^\\s*(?:curl|wget|Invoke-WebRequest|Invoke-RestMethod)\\b",
            dockerfile + entrypoint);
    }

    [Fact]
    public void BuilderConfinesPrivateOutputAndPerformsNoExternalAction()
    {
        var builder = ReadRepositoryFile("eng", "Build-RenderFreePackage.ps1");

        Assert.Contains(
            "The Render Free package output must remain under artifacts-local.",
            builder,
            StringComparison.Ordinal);
        Assert.Contains("--no-restore", builder, StringComparison.Ordinal);
        Assert.Contains("prepared-store.json", builder, StringComparison.Ordinal);
        Assert.Contains("publicDistributionAllowed = $false", builder, StringComparison.Ordinal);
        Assert.Contains("dockerInvoked = $false", builder, StringComparison.Ordinal);
        Assert.Contains("imagePublished = $false", builder, StringComparison.Ordinal);
        Assert.Contains("renderContacted = $false", builder, StringComparison.Ordinal);
        Assert.Contains("providerCalled = $false", builder, StringComparison.Ordinal);
        Assert.Contains("credentialRead = $false", builder, StringComparison.Ordinal);
        Assert.Contains("$startInfo.Environment.Clear()", builder, StringComparison.Ordinal);
        Assert.DoesNotContain("$startInfo.Environment[$credentialName]", builder, StringComparison.Ordinal);
        Assert.Contains(
            "AUTH-QUERY-EMBEDDING-RENDER-PACKAGE-READINESS",
            builder,
            StringComparison.Ordinal);
        Assert.Contains(
            "AUTH-GROUNDED-GENERATION-RENDER-PACKAGE-READINESS",
            builder,
            StringComparison.Ordinal);
        Assert.Contains("http://127.0.0.1:", builder, StringComparison.Ordinal);
        Assert.Contains("loopbackReadinessValidated = $true", builder, StringComparison.Ordinal);
        Assert.DoesNotMatch(
            "(?im)^\\s*(?:docker|curl|wget)\\b|https://",
            builder);
    }

    [Fact]
    public void VerifierRequiresTheFreeGuardAndCompleteContextIntegrity()
    {
        var verifier = ReadRepositoryFile("eng", "Test-RenderFreePackage.ps1");

        Assert.Contains("context-manifest.sha256", verifier, StringComparison.Ordinal);
        Assert.Contains("servicePlan -cne \"free\"", verifier, StringComparison.Ordinal);
        Assert.Contains("persistentDisk -ne $false", verifier, StringComparison.Ordinal);
        Assert.Contains("managedDatabase -ne $false", verifier, StringComparison.Ordinal);
        Assert.Contains("imagePublished -ne $false", verifier, StringComparison.Ordinal);
        Assert.Contains("renderContacted -ne $false", verifier, StringComparison.Ordinal);
        Assert.Contains("providerCalled -ne $false", verifier, StringComparison.Ordinal);
        Assert.Contains("credentialRead -ne $false", verifier, StringComparison.Ordinal);
        Assert.Contains("prepared-store[.]json", verifier, StringComparison.Ordinal);
    }

    private static string ReadRepositoryFile(params string[] components) =>
        File.ReadAllText(Path.Combine([FindRepositoryRoot(), .. components]));

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

        throw new InvalidOperationException("The repository root could not be located.");
    }
}
