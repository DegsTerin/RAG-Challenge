// Purpose: Verifies that the local Render Free package remains private, free-only, fail-closed and separate from external publication.
using System.Text.RegularExpressions;

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
            "healthCheckPath: /api/v1/health/live",
            template,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "healthCheckPath: /api/v1/health/ready",
            template,
            StringComparison.Ordinal);
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
        Assert.Contains(
            "runtime_store=\"/tmp/rag-challenge-store\"",
            entrypoint,
            StringComparison.Ordinal);
        Assert.Contains(
            "runtime_marker=\".rag-challenge-runtime-store-v1\"",
            entrypoint,
            StringComparison.Ordinal);
        Assert.Contains("CH_DEPLOY_RUNTIME_STORE_UNSAFE", entrypoint, StringComparison.Ordinal);
        Assert.Contains("[ -L \"${runtime_store}\" ]", entrypoint, StringComparison.Ordinal);
        Assert.Contains("umask 077", entrypoint, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "runtime_store=\"${RAG_CHALLENGE_RUNTIME_STORE:-",
            entrypoint,
            StringComparison.Ordinal);
        Assert.True(
            entrypoint.IndexOf("runtime_marker_value", StringComparison.Ordinal) <
            entrypoint.IndexOf("rm -rf --", StringComparison.Ordinal),
            "Runtime ownership validation must precede recursive removal.");
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
        var outputPolicy = ReadRepositoryFile("eng", "render-free-output-policy.ps1");
        var ownedOutputPolicy = ReadRepositoryFile("eng", "owned-output-policy.ps1");
        var combinedPolicy = outputPolicy + "\n" + ownedOutputPolicy;

        Assert.Contains(
            "The owned output must use its exact canonical path.",
            combinedPolicy,
            StringComparison.Ordinal);
        Assert.Contains("Reset-RenderFreePackageOutput", builder, StringComparison.Ordinal);
        Assert.Contains(".rag-challenge-owned-output.json", combinedPolicy, StringComparison.Ordinal);
        Assert.Contains("FileMode]::CreateNew", combinedPolicy, StringComparison.Ordinal);
        Assert.Contains("contains a reparse point", combinedPolicy, StringComparison.Ordinal);
        Assert.True(
            Regex.Count(
                builder,
                "Assert-OwnedOutputTreeIsSafe -Root \\$expectedStoreRoot",
                RegexOptions.CultureInvariant) >= 2);
        Assert.Contains("Assert-OwnedOutputTreeIsSafe -Root $seedRoot", builder, StringComparison.Ordinal);
        Assert.Contains("--no-restore", builder, StringComparison.Ordinal);
        Assert.Contains("prepared-store.json", builder, StringComparison.Ordinal);
        Assert.Contains("expectedPreparedStoreSha256", builder, StringComparison.Ordinal);
        Assert.Contains("Get-RenderStoreStructuralTreeSha256", builder, StringComparison.Ordinal);
        Assert.Contains(
            "The prepared product-store attestation identity diverged.",
            builder,
            StringComparison.Ordinal);
        Assert.Contains(
            "idxgen-4b417b79a9d8cd2472cb657a5fe7509f297b39f4831215f62143080d896e4f0d",
            builder,
            StringComparison.Ordinal);
        Assert.Contains("publicDistributionAllowed = $false", builder, StringComparison.Ordinal);
        Assert.Contains("dockerInvoked = $false", builder, StringComparison.Ordinal);
        Assert.Contains("imagePublished = $false", builder, StringComparison.Ordinal);
        Assert.Contains("renderContacted = $false", builder, StringComparison.Ordinal);
        Assert.Contains("corpus = \"4.19.5\"", builder, StringComparison.Ordinal);
        Assert.Contains("providerQuerySubmitted = $false", builder, StringComparison.Ordinal);
        Assert.Contains("providerCredentialConfigured = $false", builder, StringComparison.Ordinal);
        Assert.Contains("trustedProviderGrantConfigured = $false", builder, StringComparison.Ordinal);
        Assert.Contains("egressObservationPerformed = $false", builder, StringComparison.Ordinal);
        Assert.Contains("$startInfo.Environment.Clear()", builder, StringComparison.Ordinal);
        Assert.DoesNotContain("$startInfo.Environment[$credentialName]", builder, StringComparison.Ordinal);
        Assert.Contains(
            "CH_ADMIN_STATUS_AVAILABLE",
            builder,
            StringComparison.Ordinal);
        Assert.Contains(
            "offlineAdministrativeStatusValidated = $true",
            builder,
            StringComparison.Ordinal);
        Assert.Contains("failClosedReadinessValidated = $true", builder, StringComparison.Ordinal);
        Assert.Contains("providerBudgetState = \"Disarmed\"", builder, StringComparison.Ordinal);
        Assert.Contains("loopbackLivenessValidated = $true", builder, StringComparison.Ordinal);
        Assert.Contains("-SkipHttpErrorCheck", builder, StringComparison.Ordinal);
        Assert.Contains("StatusCode -ne 503", builder, StringComparison.Ordinal);
        Assert.Contains("http://127.0.0.1:", builder, StringComparison.Ordinal);
        Assert.DoesNotContain("loopbackReadinessValidated", builder, StringComparison.Ordinal);
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
        Assert.Contains("source.corpus -cne \"4.19.5\"", verifier, StringComparison.Ordinal);
        Assert.Contains("providerQuerySubmitted -ne $false", verifier, StringComparison.Ordinal);
        Assert.Contains("providerCredentialConfigured -ne $false", verifier, StringComparison.Ordinal);
        Assert.Contains("trustedProviderGrantConfigured -ne $false", verifier, StringComparison.Ordinal);
        Assert.Contains("egressObservationPerformed -ne $false", verifier, StringComparison.Ordinal);
        Assert.Contains("prepared-store[.]json", verifier, StringComparison.Ordinal);
        Assert.Contains("expectedPreparedStoreSha256", verifier, StringComparison.Ordinal);
        Assert.Contains(
            "idxgen-4b417b79a9d8cd2472cb657a5fe7509f297b39f4831215f62143080d896e4f0d",
            verifier,
            StringComparison.Ordinal);
        Assert.Contains(
            "offlineAdministrativeStatusValidated -ne $true",
            verifier,
            StringComparison.Ordinal);
        Assert.Contains(
            "failClosedReadinessValidated -ne $true",
            verifier,
            StringComparison.Ordinal);
        Assert.Contains("providerBudgetState -cne \"Disarmed\"", verifier, StringComparison.Ordinal);
        Assert.Contains("loopbackLivenessValidated -ne $true", verifier, StringComparison.Ordinal);
        Assert.Contains("Assert-RenderFreePackageOwnedOutput", verifier, StringComparison.Ordinal);
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
