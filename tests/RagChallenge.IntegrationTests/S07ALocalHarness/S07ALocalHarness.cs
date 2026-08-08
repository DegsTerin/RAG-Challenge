// Purpose: Defines the opt-in S07-A local campaign boundary, validates its unscored dataset envelope, and keeps providers, stores, and network policy deterministic and test-owned.
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using RagChallenge.Application.IndexingRetrieval;

namespace RagChallenge.IntegrationTests.S07ALocalHarness;

internal static partial class S07ALocalHarnessDefinition
{
    internal const string DatasetId = "rag-eval-catalogue-v1";
    internal const string DatasetRevision = "rag-eval-catalogue-v1-candidate-001";
    internal const string DatasetRelativePath =
        "docs/evaluation/rag-eval-catalogue-v1";
    internal const string EnvironmentId = "ENV-S07-A-LOCAL-01";
    internal const string CampaignId = "s07-a-local-candidate-001";
    internal const string RunAuthorityId = "AUTH-S07-A-RUN-001";
    internal const string RunAuthorityEnvironmentVariable =
        "RAGCHALLENGE_S07_A_RUN_AUTHORITY";
    internal const string NetworkPolicyId = "deny-all-v1";
    internal const string StorePolicyId = "task-owned-artifacts-local-v1";
    internal const string CandidateStatus = "candidate-reviewed-unscored";
    internal const string CandidateFreezeStatus = "not-frozen-a2-not-executed";
    internal const string FrozenStatus = "frozen-a2-unscored";

    internal const string ExactValidationCommand =
        "pwsh -NoProfile -File tests/RagChallenge.IntegrationTests/S07ALocalHarness/Invoke-S07ALocalHarness.ps1 -Mode Validate";
    internal const string ExactFutureA3Command =
        "pwsh -NoProfile -File tests/RagChallenge.IntegrationTests/S07ALocalHarness/Invoke-S07ALocalHarness.ps1 -Mode Run -AuthorityId AUTH-S07-A-RUN-001";

    internal static readonly EmbeddingProviderDescriptor EmbeddingProvider = new(
        "deterministic-local",
        "token-hash-embedding-v1",
        "auth-s07-a-harness-001",
        dimensions: 64);

    internal static readonly LanguageModelDescriptor LanguageModel = new(
        "deterministic-local",
        "evidence-template-v1",
        "auth-s07-a-harness-001");

    internal static S07ALocalHarnessPlan LoadPreparationPlan(string repositoryRoot) =>
        LoadPlan(repositoryRoot, requireFrozenCampaign: false);

    internal static S07ALocalHarnessPlan LoadFrozenPlan(string repositoryRoot) =>
        LoadPlan(repositoryRoot, requireFrozenCampaign: true);

    internal static HttpClient CreateDenyAllHttpClient() =>
        new(new DenyAllNetworkMessageHandler(), disposeHandler: true);

    private static S07ALocalHarnessPlan LoadPlan(
        string repositoryRoot,
        bool requireFrozenCampaign)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryRoot);
        var root = Path.GetFullPath(repositoryRoot);
        var datasetRoot = Path.GetFullPath(
            Path.Combine(root, DatasetRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        var datasetManifestPath = Path.Combine(datasetRoot, "dataset-manifest.json");
        var documentManifestPath = Path.Combine(datasetRoot, "document-manifest.json");
        var caseInventoryPath = Path.Combine(datasetRoot, "case-inventory.json");

        EnsureChildPath(root, datasetRoot, "The dataset must remain inside the repository.");
        EnsureExistingFile(datasetManifestPath);
        EnsureExistingFile(documentManifestPath);
        EnsureExistingFile(caseInventoryPath);

        using var datasetManifest = JsonDocument.Parse(
            File.ReadAllBytes(datasetManifestPath));
        var manifest = datasetManifest.RootElement;
        RequireString(manifest, "datasetId", DatasetId);
        RequireString(manifest, "datasetRevision", DatasetRevision);
        RequireString(manifest, "status", CandidateStatus);
        RequireBoolean(manifest, "scoredResultObserved", expected: false);
        RequireInt32(manifest, "scoredRunCount", expected: 0);

        var counts = manifest.GetProperty("counts");
        RequireInt32(counts, "syntheticFixtureCases", expected: 11);
        RequireInt32(counts, "scoredProductCorpusCases", expected: 0);

        var scope = manifest.GetProperty("scope");
        RequireBoolean(scope, "a1Completed", expected: true);
        RequireBoolean(scope, "evaluationExecuted", expected: false);
        RequireBoolean(scope, "testsExecuted", expected: false);
        RequireBoolean(scope, "networkAccessed", expected: false);

        ValidateManifestDigest(datasetManifestPath, manifest);
        ValidateReferencedFiles(
            datasetRoot,
            manifest.GetProperty("files"),
            documentManifestPath,
            caseInventoryPath);
        ValidateDocumentEnvelope(documentManifestPath);

        var freezeStatus = manifest.GetProperty("freezeStatus").GetString();
        var isFrozen = string.Equals(freezeStatus, FrozenStatus, StringComparison.Ordinal);

        if (requireFrozenCampaign && !isFrozen)
        {
            throw new InvalidDataException(
                "The freezeStatus value does not authorise the future A3 entry point.");
        }

        if (isFrozen)
        {
            RequireBoolean(scope, "a2OrLaterExecuted", expected: true);
            ValidateFrozenConfiguration(manifest);
        }
        else if (string.Equals(
                     freezeStatus,
                     CandidateFreezeStatus,
                     StringComparison.Ordinal))
        {
            RequireBoolean(scope, "a2OrLaterExecuted", expected: false);
            RequireNull(manifest, "campaignEnvironment");
            RequireNull(manifest, "providerConfiguration");
            RequireBoolean(
                manifest.GetProperty("thresholds"),
                "valuesFrozenForCampaign",
                expected: false);
        }
        else
        {
            throw new InvalidDataException(
                "The freezeStatus value is outside the closed harness lifecycle.");
        }

        var workspace = ResolveWorkspace(root);
        return new S07ALocalHarnessPlan(
            root,
            datasetRoot,
            datasetManifestPath,
            documentManifestPath,
            caseInventoryPath,
            workspace,
            EmbeddingProvider,
            LanguageModel,
            EnvironmentId,
            NetworkPolicyId,
            StorePolicyId,
            ExactValidationCommand,
            ExactFutureA3Command,
            isFrozen);
    }

    private static void ValidateFrozenConfiguration(JsonElement manifest)
    {
        var environment = manifest.GetProperty("campaignEnvironment");
        RequireString(environment, "environmentId", EnvironmentId);
        RequireString(environment, "campaignId", CampaignId);
        RequireString(environment, "networkPolicyId", NetworkPolicyId);
        RequireString(environment, "storePolicyId", StorePolicyId);
        RequireString(environment, "command", ExactFutureA3Command);

        var providers = manifest.GetProperty("providerConfiguration");
        var embedding = providers.GetProperty("embedding");
        RequireString(embedding, "providerId", EmbeddingProvider.ProviderId);
        RequireString(embedding, "modelId", EmbeddingProvider.ModelId);
        RequireString(embedding, "modelRevision", EmbeddingProvider.ModelRevision);
        RequireInt32(embedding, "dimensions", EmbeddingProvider.Dimensions);
        var languageModel = providers.GetProperty("languageModel");
        RequireString(languageModel, "providerId", LanguageModel.ProviderId);
        RequireString(languageModel, "modelId", LanguageModel.ModelId);
        RequireString(languageModel, "modelRevision", LanguageModel.ModelRevision);
        RequireBoolean(
            manifest.GetProperty("thresholds"),
            "valuesFrozenForCampaign",
            expected: true);
    }

    private static S07ALocalHarnessWorkspace ResolveWorkspace(string repositoryRoot)
    {
        var allowedRoot = Path.GetFullPath(Path.Combine(
            repositoryRoot,
            "artifacts-local",
            "state-07",
            "s07-a"));
        var campaignRoot = Path.GetFullPath(Path.Combine(allowedRoot, CampaignId));
        EnsureChildPath(
            allowedRoot,
            campaignRoot,
            "The campaign workspace must remain task-owned under artifacts-local/state-07/s07-a.");
        return new S07ALocalHarnessWorkspace(
            campaignRoot,
            Path.Combine(campaignRoot, "stores", "control"),
            Path.Combine(campaignRoot, "stores", "vectors"),
            Path.Combine(campaignRoot, "stores", "content"),
            Path.Combine(campaignRoot, "evidence"));
    }

    private static void ValidateReferencedFiles(
        string datasetRoot,
        JsonElement files,
        string documentManifestPath,
        string caseInventoryPath)
    {
        var expected = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["document-manifest.json"] = documentManifestPath,
            ["case-inventory.json"] = caseInventoryPath,
        };

        foreach (var entry in files.EnumerateArray())
        {
            var relativePath = entry.GetProperty("path").GetString() ??
                throw new InvalidDataException("A dataset file entry has no path.");

            if (!expected.Remove(relativePath, out var expectedPath))
            {
                throw new InvalidDataException(
                    $"The dataset references an unexpected file: {relativePath}.");
            }

            var resolvedPath = Path.GetFullPath(Path.Combine(datasetRoot, relativePath));
            EnsureChildPath(
                datasetRoot,
                resolvedPath,
                "A dataset file reference escapes its manifest directory.");

            if (!string.Equals(resolvedPath, expectedPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    $"The dataset file path is not canonical: {relativePath}.");
            }

            RequireString(
                entry,
                "fileSha256",
                ComputeSha256(File.ReadAllBytes(resolvedPath)));
            using var referenced = JsonDocument.Parse(File.ReadAllBytes(resolvedPath));
            RequireString(
                entry,
                "embeddedManifestSha256",
                referenced.RootElement.GetProperty("manifestSha256").GetString()!);
            ValidateManifestDigest(resolvedPath, referenced.RootElement);
        }

        if (expected.Count != 0)
        {
            throw new InvalidDataException(
                "The dataset manifest does not reference both required input files.");
        }
    }

    private static void ValidateDocumentEnvelope(string documentManifestPath)
    {
        using var documentManifest = JsonDocument.Parse(
            File.ReadAllBytes(documentManifestPath));
        var manifest = documentManifest.RootElement;
        RequireString(manifest, "datasetId", DatasetId);
        RequireString(manifest, "datasetRevision", DatasetRevision);
        RequireString(manifest, "status", CandidateStatus);
        RequireInt32(manifest, "scoredProductCorpusDocumentCount", expected: 0);
        RequireInt32(manifest, "syntheticFixtureDocumentCount", expected: 3);
    }

    private static void ValidateManifestDigest(string path, JsonElement manifest)
    {
        var expected = manifest.GetProperty("manifestSha256").GetString() ??
            throw new InvalidDataException("A manifest digest is missing.");
        var bytes = File.ReadAllBytes(path);
        var text = new UTF8Encoding(false, true).GetString(bytes);
        var matches = ManifestDigestPattern().Matches(text);

        if (matches.Count != 1)
        {
            throw new InvalidDataException(
                "A manifest must contain exactly one lower-case SHA-256 digest field.");
        }

        var zeroed = ManifestDigestPattern().Replace(
            text,
            match => match.Groups[1].Value + new string('0', 64) + match.Groups[2].Value,
            count: 1);
        var observed = ComputeSha256(Encoding.UTF8.GetBytes(zeroed));

        if (!string.Equals(observed, expected, StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"The embedded digest does not match the exact bytes of {Path.GetFileName(path)}.");
        }
    }

    private static void EnsureExistingFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("A required harness input is missing.", path);
        }
    }

    private static void EnsureChildPath(string parent, string candidate, string message)
    {
        var relative = Path.GetRelativePath(parent, candidate);

        if (Path.IsPathFullyQualified(relative) ||
            relative.Equals("..", StringComparison.Ordinal) ||
            relative.StartsWith(
                ".." + Path.DirectorySeparatorChar,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void RequireString(
        JsonElement owner,
        string propertyName,
        string expected)
    {
        var observed = owner.GetProperty(propertyName).GetString();

        if (!string.Equals(observed, expected, StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"The {propertyName} value does not match the fixed harness boundary.");
        }
    }

    private static void RequireBoolean(
        JsonElement owner,
        string propertyName,
        bool expected)
    {
        if (owner.GetProperty(propertyName).GetBoolean() != expected)
        {
            throw new InvalidDataException(
                $"The {propertyName} value does not match the fixed harness boundary.");
        }
    }

    private static void RequireInt32(
        JsonElement owner,
        string propertyName,
        int expected)
    {
        if (owner.GetProperty(propertyName).GetInt32() != expected)
        {
            throw new InvalidDataException(
                $"The {propertyName} value does not match the fixed harness boundary.");
        }
    }

    private static void RequireNull(JsonElement owner, string propertyName)
    {
        if (owner.GetProperty(propertyName).ValueKind != JsonValueKind.Null)
        {
            throw new InvalidDataException(
                $"The {propertyName} value must remain null before A2.");
        }
    }

    private static string ComputeSha256(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    [GeneratedRegex(
        "(\\\"manifestSha256\\\"\\s*:\\s*\\\")[0-9a-f]{64}(\\\")",
        RegexOptions.CultureInvariant)]
    private static partial Regex ManifestDigestPattern();

    private sealed class DenyAllNetworkMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromException<HttpResponseMessage>(new HttpRequestException(
                HttpRequestError.ConnectionError,
                $"Network access is denied by {NetworkPolicyId}.",
                inner: null,
                statusCode: null));
    }
}

internal sealed record S07ALocalHarnessPlan(
    string RepositoryRoot,
    string DatasetRoot,
    string DatasetManifestPath,
    string DocumentManifestPath,
    string CaseInventoryPath,
    S07ALocalHarnessWorkspace Workspace,
    EmbeddingProviderDescriptor EmbeddingProvider,
    LanguageModelDescriptor LanguageModel,
    string EnvironmentId,
    string NetworkPolicyId,
    string StorePolicyId,
    string ValidationCommand,
    string FutureA3Command,
    bool IsFrozen);

internal sealed record S07ALocalHarnessWorkspace(
    string CampaignRoot,
    string ControlStoreRoot,
    string VectorStoreRoot,
    string ContentStoreRoot,
    string EvidenceRoot);
