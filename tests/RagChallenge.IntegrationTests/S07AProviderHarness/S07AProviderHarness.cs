// Purpose: Validates and exercises the frozen provider-candidate preparation through an injected transport while keeping network, secrets and real evaluation outside this increment.
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Infrastructure.Providers;

namespace RagChallenge.IntegrationTests.S07AProviderHarness;

internal static class S07AProviderHarnessDefinition
{
    internal const string DatasetId = "rag-eval-catalogue-v1";
    internal const string DatasetRevision =
        "rag-eval-catalogue-v1-provider-gpt54m-candidate-001";
    internal const string PredecessorRevision = "rag-eval-catalogue-v1-candidate-001";
    internal const string CampaignId = "s07-a-provider-gpt54m-candidate-001";
    internal const string EnvironmentId = "ENV-S07-A-PROVIDER-01";
    internal const string AuthorityId = "AUTH-S07-A-PROVIDER-PREP-001";
    internal const string ModelId = OpenAiLanguageModelOptions.MvpModelId;
    internal const string PromptVersion = "grounded-answer-v1";
    internal const string FakeCredential = "<synthetic-credential>";

    private const string RelativeRevisionRoot =
        "docs/evaluation/rag-eval-catalogue-v1/revisions/" + DatasetRevision;
    private const string ZeroDigest =
        "0000000000000000000000000000000000000000000000000000000000000000";
    private static readonly string[] RevisionFiles =
    [
        "campaign-contract.json",
        "call-schedule.json",
        "case-inventory.json",
        "document-manifest.json",
    ];
    private static readonly string[] AttackClasses =
    [
        "instruction-override",
        "system-prompt-exfiltration",
        "citation-forgery",
        "provenance-confusion",
        "tool-or-network-request",
        "policy-redefinition",
    ];

    internal static S07AProviderHarnessPlan LoadFrozenPlan(string repositoryRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryRoot);
        var revisionRoot = Path.Combine(
            repositoryRoot,
            RelativeRevisionRoot.Replace('/', Path.DirectorySeparatorChar));
        var datasetPath = Path.Combine(revisionRoot, "dataset-manifest.json");

        using var dataset = ReadAndVerifyManifest(datasetPath);
        RequireIdentity(dataset.RootElement, "rag-evaluation-dataset-manifest");
        RequireString(dataset.RootElement, "predecessorRevision", PredecessorRevision);
        RequireString(dataset.RootElement, "campaignId", CampaignId);
        RequireString(dataset.RootElement, "environmentId", EnvironmentId);

        if (!dataset.RootElement.GetProperty("immutable").GetBoolean() ||
            dataset.RootElement.GetProperty("providerRunCount").GetInt32() != 0 ||
            dataset.RootElement.GetProperty("scoredResultObserved").GetBoolean())
        {
            throw new InvalidDataException(
                "The successor dataset is not frozen as an unexecuted immutable revision.");
        }

        VerifyPredecessor(dataset.RootElement, repositoryRoot);
        var manifests = RevisionFiles.ToDictionary(
            name => name,
            name => ReadAndVerifyManifest(Path.Combine(revisionRoot, name)),
            StringComparer.Ordinal);

        try
        {
            VerifyDatasetFileIdentities(dataset.RootElement, revisionRoot, manifests);
            var contract = manifests["campaign-contract.json"].RootElement;
            var documentManifest = manifests["document-manifest.json"].RootElement;
            var caseInventory = manifests["case-inventory.json"].RootElement;
            var schedule = manifests["call-schedule.json"].RootElement;
            var contractSnapshot = VerifyContract(contract);
            var documents = VerifyDocuments(documentManifest);
            var cases = VerifyCases(caseInventory, documents);
            VerifySchedule(schedule, cases);

            return new S07AProviderHarnessPlan(
                repositoryRoot,
                revisionRoot,
                contractSnapshot.Prompt,
                contractSnapshot.PromptSha256,
                contractSnapshot.SchemaSha256,
                cases);
        }
        finally
        {
            foreach (var manifest in manifests.Values)
            {
                manifest.Dispose();
            }
        }
    }

    internal static string FindRepositoryRoot()
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

    internal static string Sha256Utf8(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    internal static string Sha256CanonicalJson(JsonElement value) =>
        Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(value)))
            .ToLowerInvariant();

    private static JsonDocument ReadAndVerifyManifest(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var document = JsonDocument.Parse(bytes);
        var digest = RequiredString(document.RootElement, "manifestSha256");

        if (digest.Length != 64 || digest.Any(character =>
                character is not (>= '0' and <= '9') and not (>= 'a' and <= 'f')))
        {
            document.Dispose();
            throw new InvalidDataException($"Manifest {path} has an invalid digest field.");
        }

        var text = new UTF8Encoding(false, true).GetString(bytes);
        var first = text.IndexOf(digest, StringComparison.Ordinal);
        var last = text.LastIndexOf(digest, StringComparison.Ordinal);

        if (first < 0 || first != last)
        {
            document.Dispose();
            throw new InvalidDataException($"Manifest {path} has an ambiguous digest field.");
        }

        var unsigned = string.Concat(text.AsSpan(0, first), ZeroDigest, text.AsSpan(first + 64));
        var observed = Sha256Utf8(unsigned);

        if (!string.Equals(digest, observed, StringComparison.Ordinal))
        {
            document.Dispose();
            throw new InvalidDataException($"Manifest {path} failed its frozen digest check.");
        }

        return document;
    }

    private static void VerifyPredecessor(JsonElement dataset, string repositoryRoot)
    {
        var predecessorRoot = Path.Combine(
            repositoryRoot,
            "docs",
            "evaluation",
            DatasetId);
        var expectedFiles = dataset.GetProperty("predecessorFiles");

        foreach (var property in expectedFiles.EnumerateObject())
        {
            var path = Path.Combine(predecessorRoot, property.Name);
            var bytes = File.ReadAllBytes(path);
            var observedHash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

            if (!string.Equals(
                    observedHash,
                    RequiredString(property.Value, "fileSha256"),
                    StringComparison.Ordinal) ||
                bytes.LongLength != property.Value.GetProperty("byteLength").GetInt64())
            {
                throw new InvalidDataException(
                    $"Frozen predecessor file {property.Name} has changed.");
            }
        }

        if (expectedFiles.EnumerateObject().Count() != 3)
        {
            throw new InvalidDataException("The predecessor identity set is incomplete.");
        }
    }

    private static void VerifyDatasetFileIdentities(
        JsonElement dataset,
        string revisionRoot,
        Dictionary<string, JsonDocument> manifests)
    {
        var entries = dataset.GetProperty("files").EnumerateArray().ToArray();

        if (entries.Length != RevisionFiles.Length ||
            !entries.Select(entry => RequiredString(entry, "path"))
                .ToHashSet(StringComparer.Ordinal)
                .SetEquals(RevisionFiles))
        {
            throw new InvalidDataException("The successor file identity set is incomplete.");
        }

        foreach (var entry in entries)
        {
            var name = RequiredString(entry, "path");
            var bytes = File.ReadAllBytes(Path.Combine(revisionRoot, name));
            var observed = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
            var embedded = RequiredString(manifests[name].RootElement, "manifestSha256");

            if (!string.Equals(observed, RequiredString(entry, "fileSha256"), StringComparison.Ordinal) ||
                !string.Equals(
                    embedded,
                    RequiredString(entry, "embeddedManifestSha256"),
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException($"Successor file {name} has changed.");
            }
        }
    }

    private static ContractSnapshot VerifyContract(JsonElement contract)
    {
        RequireIdentity(contract, "rag-evaluation-provider-campaign-contract");
        RequireString(contract, "campaignId", CampaignId);
        RequireString(contract, "environmentId", EnvironmentId);
        RequireString(contract, "status", "frozen-preparation-unexecuted");
        var provider = contract.GetProperty("providerConfiguration");
        RequireString(provider, "providerId", "openai");
        RequireString(provider, "api", "Responses API");
        RequireString(provider, "route", "/v1/responses");
        RequireString(provider, "modelId", ModelId);
        RequireString(provider, "modelRevision", ModelId);
        RequireString(provider.GetProperty("reasoning"), "effort", "none");
        RequireString(provider.GetProperty("reasoning"), "context", "current_turn");
        RequireString(provider, "tools", "omitted");
        RequireString(provider, "temperature", "omitted");
        RequireString(provider, "background", "omitted");
        RequireString(provider, "previousResponseId", "omitted");

        if (provider.GetProperty("store").GetBoolean() ||
            provider.GetProperty("retryCount").GetInt32() != 0 ||
            provider.GetProperty("concurrency").GetInt32() != 1)
        {
            throw new InvalidDataException("The provider configuration is not fail-closed.");
        }

        var prompt = contract.GetProperty("prompt");
        RequireString(prompt, "version", PromptVersion);
        var promptText = RequiredString(prompt, "text");
        var promptSha256 = RequiredString(prompt, "utf8Sha256");

        if (!string.Equals(Sha256Utf8(promptText), promptSha256, StringComparison.Ordinal))
        {
            throw new InvalidDataException("The frozen prompt digest is invalid.");
        }

        var responseSchema = contract.GetProperty("responseSchema");
        RequireString(responseSchema, "type", "json_schema");
        RequireString(responseSchema, "name", "grounded_answer");

        if (!responseSchema.GetProperty("strict").GetBoolean())
        {
            throw new InvalidDataException("The response schema is not strict.");
        }

        var schemaSha256 = RequiredString(responseSchema, "canonicalJsonSha256");

        if (!string.Equals(
                Sha256CanonicalJson(responseSchema.GetProperty("schema")),
                schemaSha256,
                StringComparison.Ordinal))
        {
            throw new InvalidDataException("The frozen response-schema digest is invalid.");
        }

        var limits = contract.GetProperty("limits");
        RequireInt(limits, "questionUtf8Bytes", 4096);
        RequireInt(limits, "evidenceChunks", 6);
        RequireInt(limits, "evidenceUnicodeScalars", 16000);
        RequireInt(limits, "retrievalTop", 8);
        RequireInt(limits, "answerCharacters", 32768);
        RequireInt(limits, "maximumOutputTokens", 8192);
        RequireInt(limits, "responseBytes", 2 * 1024 * 1024);
        RequireInt(limits, "connectTimeoutSeconds", 10);
        RequireInt(limits, "endToEndDeadlineSeconds", 25);
        RequireInt(limits, "latencyP95Seconds", 12);
        RequireInt(limits, "latencyP99Seconds", 20);

        var callPolicy = contract.GetProperty("callPolicy");
        RequireInt(callPolicy, "contractSmokeCalls", 4);
        RequireInt(callPolicy, "warmUpCalls", 5);
        RequireInt(callPolicy, "measuredProviderCalls", 100);
        RequireInt(callPolicy, "maximumProviderCalls", 109);
        RequireInt(callPolicy, "retryCount", 0);
        RequireInt(callPolicy, "concurrency", 1);

        if (callPolicy.GetProperty("repeatedCasesAffectQualityDenominators").GetBoolean())
        {
            throw new InvalidDataException("Repeated cases may not alter quality denominators.");
        }

        var budget = contract.GetProperty("budget");
        RequireString(budget, "currency", "USD");
        RequireInt(budget, "operationalLimit", 16);
        RequireInt(budget, "absoluteCeiling", 20);

        if (!budget.GetProperty("stopAtOrAboveAbsoluteCeiling").GetBoolean())
        {
            throw new InvalidDataException("The absolute spend ceiling is not fail-closed.");
        }

        var secretPolicy = contract.GetProperty("secretPolicy");
        RequireString(secretPolicy, "frozenReference", "<provider-secret-reference>");

        if (secretPolicy.GetProperty("valuePresent").GetBoolean() ||
            secretPolicy.GetProperty("valueAllowedInRepository").GetBoolean() ||
            secretPolicy.GetProperty("valueAllowedInLogs").GetBoolean() ||
            secretPolicy.GetProperty("ambientCredentialDiscovery").GetBoolean())
        {
            throw new InvalidDataException("The frozen secret-reference policy is unsafe.");
        }

        var boundary = contract.GetProperty("executionBoundary");
        RequireString(boundary, "preparation", "local-offline-deterministic-fake-handler-only");
        RequireString(boundary, "realProviderRun", "not-authorised");

        foreach (var property in boundary.EnumerateObject().Where(property =>
                     property.Value.ValueKind == JsonValueKind.False ||
                     property.Value.ValueKind == JsonValueKind.True))
        {
            if (property.Value.GetBoolean())
            {
                throw new InvalidDataException(
                    $"Execution-boundary flag {property.Name} must remain false.");
            }
        }

        return new ContractSnapshot(promptText, promptSha256, schemaSha256);
    }

    private static Dictionary<string, FrozenDocument> VerifyDocuments(
        JsonElement manifest)
    {
        RequireIdentity(manifest, "rag-evaluation-document-manifest");
        RequireInt(manifest, "scoredProductCorpusDocumentCount", 0);
        RequireInt(manifest, "realSourceCandidateCount", 0);
        RequireInt(manifest, "syntheticFixtureDocumentCount", 2);
        var fixtures = manifest.GetProperty("syntheticFixtures").EnumerateArray().ToArray();
        var result = new Dictionary<string, FrozenDocument>(StringComparer.Ordinal);

        foreach (var fixture in fixtures)
        {
            var id = RequiredString(fixture, "documentId");
            var content = RequiredString(fixture, "canonicalFixtureContent");
            var bytes = Encoding.UTF8.GetBytes(content);
            var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
            var format = RequiredString(fixture, "documentFormat");
            var language = RequiredString(fixture, "contentLanguage");

            if (!string.Equals(hash, RequiredString(fixture, "contentSha256"), StringComparison.Ordinal) ||
                bytes.LongLength != fixture.GetProperty("contentByteLength").GetInt64() ||
                format is not ("Csv" or "Pdf") ||
                language is not ("pt-BR" or "en-GB") ||
                fixture.GetProperty("productCorpus").GetBoolean())
            {
                throw new InvalidDataException($"Synthetic fixture {id} is invalid.");
            }

            result.Add(id, new FrozenDocument(id, format, language, hash, content));
        }

        if (result.Count != 2 ||
            !result.Values.Select(item => item.Format).ToHashSet(StringComparer.Ordinal)
                .SetEquals(["Csv", "Pdf"]) ||
            !result.Values.Select(item => item.ContentLanguage).ToHashSet(StringComparer.Ordinal)
                .SetEquals(["pt-BR", "en-GB"]))
        {
            throw new InvalidDataException("The synthetic document matrix is incomplete.");
        }

        return result;
    }

    private static S07AProviderCase[] VerifyCases(
        JsonElement inventory,
        IReadOnlyDictionary<string, FrozenDocument> documents)
    {
        RequireIdentity(inventory, "rag-evaluation-case-inventory");
        RequireInt(inventory, "scoredProductCorpusCaseCount", 0);
        RequireInt(inventory, "syntheticFixtureCaseCount", 60);
        var elements = inventory.GetProperty("syntheticFixtureCases").EnumerateArray().ToArray();
        var cases = elements.Select(ReadCase).ToArray();

        if (cases.Length != 60 ||
            cases.Select(candidate => candidate.CaseId).Distinct(StringComparer.Ordinal).Count() != 60 ||
            cases.Any(candidate => candidate.ProductCorpus))
        {
            throw new InvalidDataException("The synthetic case inventory identity is invalid.");
        }

        foreach (var candidate in cases)
        {
            if (!documents.TryGetValue(candidate.DocumentId, out var document) ||
                !string.Equals(candidate.DocumentFormat, document.Format, StringComparison.Ordinal) ||
                !string.Equals(candidate.ContentLanguage, document.ContentLanguage, StringComparison.Ordinal) ||
                !string.Equals(candidate.DocumentContentSha256, document.ContentSha256, StringComparison.Ordinal) ||
                candidate.QuestionLanguage is not ("pt-BR" or "en-GB") ||
                !string.Equals(
                    candidate.QuestionLanguage,
                    candidate.ExpectedAnswerLanguage,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException($"Case {candidate.CaseId} has invalid identity metadata.");
            }

            var expectedLocationKind = candidate.DocumentFormat == "Csv"
                ? "csv-row"
                : "pdf-physical-page";

            if (candidate.ExpectedOutcome == "answered")
            {
                if (!candidate.ProviderCallExpected ||
                    candidate.RelevantLocationKinds.Count != 1 ||
                    candidate.RelevantLocationKinds[0] != expectedLocationKind ||
                    candidate.ProviderEvidence.Count != 1 ||
                    candidate.ExpectedCitedChunkIds.Count != 1 ||
                    candidate.RequiredFacts.Count != 1)
                {
                    throw new InvalidDataException(
                        $"Answerable case {candidate.CaseId} is not fully frozen.");
                }
            }
            else if (candidate.ExpectedOutcome == "insufficient-evidence")
            {
                if (candidate.RelevantLocationKinds.Count != 0 ||
                    candidate.ExpectedCitedChunkIds.Count != 0 ||
                    candidate.RequiredFacts.Count != 0 ||
                    candidate.InsufficiencyPathway is not (
                        "no-retrieval-provider-call-zero" or
                        "evidence-present-but-insufficient"))
                {
                    throw new InvalidDataException(
                        $"Insufficient-evidence case {candidate.CaseId} is not fully frozen.");
                }

                var noRetrieval = candidate.InsufficiencyPathway ==
                    "no-retrieval-provider-call-zero";

                if (noRetrieval == candidate.ProviderCallExpected ||
                    noRetrieval != (candidate.ProviderEvidence.Count == 0))
                {
                    throw new InvalidDataException(
                        $"Insufficient-evidence case {candidate.CaseId} has an invalid call boundary.");
                }
            }
            else
            {
                throw new InvalidDataException($"Case {candidate.CaseId} has an unknown outcome.");
            }

            if (candidate.ProviderEvidence.Any(evidence =>
                    evidence.ContentLanguage != candidate.ContentLanguage ||
                    !document.Content.Contains(evidence.Text, StringComparison.Ordinal)) ||
                candidate.ProviderEvidence.Select(item => item.ChunkId)
                    .Distinct(StringComparer.Ordinal).Count() != candidate.ProviderEvidence.Count)
            {
                throw new InvalidDataException($"Case {candidate.CaseId} has invalid provider evidence.");
            }
        }

        var answerable = cases.Where(candidate => candidate.ExpectedOutcome == "answered").ToArray();
        var insufficient = cases.Where(candidate =>
            candidate.ExpectedOutcome == "insufficient-evidence").ToArray();

        if (answerable.Length != 40 || insufficient.Length != 20 ||
            cases.Count(candidate => candidate.ProviderCallExpected) != 50 ||
            insufficient.Count(candidate =>
                candidate.InsufficiencyPathway == "no-retrieval-provider-call-zero") != 10 ||
            insufficient.Count(candidate =>
                candidate.InsufficiencyPathway == "evidence-present-but-insufficient") != 10 ||
            insufficient.Count(candidate => candidate.QuestionLanguage == "pt-BR") != 10 ||
            insufficient.Count(candidate => candidate.QuestionLanguage == "en-GB") != 10)
        {
            throw new InvalidDataException("The case outcome or provider-call distribution is invalid.");
        }

        foreach (var questionLanguage in new[] { "pt-BR", "en-GB" })
        {
            foreach (var contentLanguage in new[] { "pt-BR", "en-GB" })
            {
                if (answerable.Count(candidate =>
                        candidate.QuestionLanguage == questionLanguage &&
                        candidate.ContentLanguage == contentLanguage) != 10)
                {
                    throw new InvalidDataException(
                        $"The answerable matrix cell {questionLanguage}->{contentLanguage} is incomplete.");
                }
            }

            var attacks = answerable.Where(candidate =>
                    candidate.QuestionLanguage == questionLanguage &&
                    candidate.PromptInjectionPresent)
                .Select(candidate => candidate.AttackClass!)
                .ToHashSet(StringComparer.Ordinal);

            if (!attacks.SetEquals(AttackClasses))
            {
                throw new InvalidDataException(
                    $"Prompt-injection coverage for {questionLanguage} is incomplete.");
            }
        }

        if (answerable.Count(candidate => candidate.PromptInjectionPresent) != 12)
        {
            throw new InvalidDataException("The prompt-injection case count is invalid.");
        }

        return cases;
    }

    private static void VerifySchedule(
        JsonElement schedule,
        IReadOnlyList<S07AProviderCase> cases)
    {
        RequireIdentity(schedule, "rag-evaluation-provider-call-schedule");
        RequireString(schedule, "campaignId", CampaignId);
        RequireInt(schedule, "maximumProviderCalls", 109);
        RequireInt(schedule, "retryCount", 0);
        RequireInt(schedule, "concurrency", 1);
        RequireInt(schedule, "providerCallingUniqueCaseCount", 50);
        var calls = schedule.GetProperty("calls").EnumerateArray().ToArray();
        var providerCases = cases.Where(candidate => candidate.ProviderCallExpected)
            .ToDictionary(candidate => candidate.CaseId, StringComparer.Ordinal);

        if (calls.Length != 109)
        {
            throw new InvalidDataException("The provider call schedule is not capped at 109 calls.");
        }

        for (var index = 0; index < calls.Length; index++)
        {
            var call = calls[index];
            var caseId = RequiredString(call, "caseId");

            if (call.GetProperty("callIndex").GetInt32() != index + 1 ||
                !providerCases.ContainsKey(caseId))
            {
                throw new InvalidDataException("The provider call schedule has an invalid entry.");
            }
        }

        if (calls.Count(call => RequiredString(call, "phase") == "contract-smoke") != 4 ||
            calls.Count(call => RequiredString(call, "phase") == "warm-up") != 5 ||
            calls.Count(call => RequiredString(call, "phase") == "measured") != 100)
        {
            throw new InvalidDataException("The provider call phase counts are invalid.");
        }

        var measured = calls.Where(call => RequiredString(call, "phase") == "measured")
            .ToArray();

        foreach (var providerCase in providerCases.Keys)
        {
            var repetitions = measured.Where(call =>
                    RequiredString(call, "caseId") == providerCase)
                .OrderBy(call => call.GetProperty("repetition").GetInt32())
                .ToArray();

            if (repetitions.Length != 2 ||
                repetitions[0].GetProperty("repetition").GetInt32() != 1 ||
                repetitions[1].GetProperty("repetition").GetInt32() != 2 ||
                !repetitions[0].GetProperty("qualityDenominatorContribution").GetBoolean() ||
                repetitions[1].GetProperty("qualityDenominatorContribution").GetBoolean())
            {
                throw new InvalidDataException(
                    $"Measured schedule repetitions for {providerCase} are invalid.");
            }
        }
    }

    private static S07AProviderCase ReadCase(JsonElement candidate)
    {
        RequireString(candidate, "datasetRevision", DatasetRevision);
        var injection = candidate.GetProperty("promptInjection");
        return new S07AProviderCase(
            RequiredString(candidate, "caseId"),
            RequiredString(candidate, "questionLanguage"),
            RequiredString(candidate, "expectedAnswerLanguage"),
            RequiredString(candidate, "contentLanguage"),
            RequiredString(candidate, "question"),
            RequiredString(candidate, "expectedOutcome"),
            candidate.GetProperty("providerCallExpected").GetBoolean(),
            OptionalString(candidate, "insufficiencyPathway"),
            candidate.GetProperty("productCorpus").GetBoolean(),
            RequiredString(candidate, "documentId"),
            RequiredString(candidate, "documentFormat"),
            RequiredString(candidate, "documentContentSha256"),
            candidate.GetProperty("relevantLocations").EnumerateArray()
                .Select(location => RequiredString(location, "locationKind")).ToArray(),
            candidate.GetProperty("requiredFacts").EnumerateArray()
                .Select(item => item.GetString()!).ToArray(),
            injection.GetProperty("present").GetBoolean(),
            OptionalString(injection, "attackClass"),
            candidate.GetProperty("providerEvidence").EnumerateArray()
                .Select(item => new S07AProviderEvidence(
                    RequiredString(item, "chunkId"),
                    RequiredString(item, "contentLanguage"),
                    RequiredString(item, "text")))
                .ToArray(),
            candidate.GetProperty("expectedCitedChunkIds").EnumerateArray()
                .Select(item => item.GetString()!).ToArray());
    }

    private static void RequireIdentity(JsonElement owner, string manifestType)
    {
        RequireInt(owner, "schemaVersion", 1);
        RequireString(owner, "manifestType", manifestType);
        RequireString(owner, "datasetId", DatasetId);
        RequireString(owner, "datasetRevision", DatasetRevision);
        RequireString(owner, "authorityId", AuthorityId);
    }

    private static void RequireString(
        JsonElement owner,
        string propertyName,
        string expected)
    {
        if (!string.Equals(
                RequiredString(owner, propertyName),
                expected,
                StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"The required {propertyName} value is not {expected}.");
        }
    }

    private static void RequireInt(JsonElement owner, string propertyName, int expected)
    {
        if (owner.GetProperty(propertyName).GetInt32() != expected)
        {
            throw new InvalidDataException(
                $"The required {propertyName} value is not {expected}.");
        }
    }

    private static string RequiredString(JsonElement owner, string propertyName) =>
        owner.GetProperty(propertyName).GetString() ??
        throw new InvalidDataException($"The required {propertyName} value is missing.");

    private static string? OptionalString(JsonElement owner, string propertyName)
    {
        var value = owner.GetProperty(propertyName);
        return value.ValueKind == JsonValueKind.Null ? null : value.GetString();
    }

    private sealed record ContractSnapshot(
        string Prompt,
        string PromptSha256,
        string SchemaSha256);

    private sealed record FrozenDocument(
        string DocumentId,
        string Format,
        string ContentLanguage,
        string ContentSha256,
        string Content);
}

internal static class S07AProviderCandidateHarness
{
    internal static async Task<IReadOnlyList<S07AProviderObservedResult>> ExecuteAsync(
        S07AProviderHarnessPlan plan,
        HttpMessageHandler injectedHandler,
        Func<CancellationToken, ValueTask<string>> credentialSource,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(injectedHandler);
        ArgumentNullException.ThrowIfNull(credentialSource);
        using var client = new HttpClient(injectedHandler, disposeHandler: false)
        {
            BaseAddress = new Uri("https://api.openai.com/", UriKind.Absolute),
            Timeout = TimeSpan.FromSeconds(25),
        };
        var descriptor = new LanguageModelDescriptor(
            "openai",
            S07AProviderHarnessDefinition.ModelId,
            S07AProviderHarnessDefinition.ModelId);
        var options = new OpenAiLanguageModelOptions(
            descriptor,
            OpenAiReasoningEffort.None,
            OpenAiReasoningContext.CurrentTurn);
        var adapter = new OpenAiHttpLanguageModel(client, credentialSource, options);
        var results = new List<S07AProviderObservedResult>();

        foreach (var candidate in plan.Cases.Where(item => item.ProviderCallExpected))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await adapter.GenerateAsync(
                new GroundedGenerationRequest(
                    plan.Prompt,
                    S07AProviderHarnessDefinition.PromptVersion,
                    candidate.Question,
                    ToQuestionLanguage(candidate.QuestionLanguage),
                    candidate.ProviderEvidence.Select(item => new GroundedEvidence(
                        item.ChunkId,
                        item.Text,
                        ToContentLanguage(item.ContentLanguage))).ToArray(),
                    maximumOutputCharacters: 32768),
                cancellationToken);
            var expectedCitations = candidate.ExpectedCitedChunkIds
                .Order(StringComparer.Ordinal).ToArray();
            var observedCitations = result.CitedChunkIds
                .Order(StringComparer.Ordinal).ToArray();

            if (result.ObservedDescriptor != descriptor ||
                result.AnswerLanguage != ToQuestionLanguage(candidate.ExpectedAnswerLanguage) ||
                string.IsNullOrWhiteSpace(result.Answer) ||
                !expectedCitations.SequenceEqual(observedCitations, StringComparer.Ordinal))
            {
                throw new InvalidDataException(
                    $"Fake-handler result for {candidate.CaseId} violated the frozen adapter contract.");
            }

            results.Add(new S07AProviderObservedResult(
                candidate.CaseId,
                candidate.ExpectedOutcome,
                result.AnswerLanguage.ToString(),
                result.CitedChunkIds.ToArray()));
        }

        return results;
    }

    private static SupportedQueryLanguage ToQuestionLanguage(string value) => value switch
    {
        "pt-BR" => SupportedQueryLanguage.PtBr,
        "en-GB" => SupportedQueryLanguage.EnGb,
        _ => throw new InvalidDataException("The frozen question language is unsupported."),
    };

    private static DocumentContentLanguage ToContentLanguage(string value) => value switch
    {
        "pt-BR" => DocumentContentLanguage.PtBr,
        "en-GB" => DocumentContentLanguage.EnGb,
        _ => throw new InvalidDataException("The frozen content language is unsupported."),
    };
}

internal sealed record S07AProviderHarnessPlan(
    string RepositoryRoot,
    string RevisionRoot,
    string Prompt,
    string PromptSha256,
    string SchemaSha256,
    IReadOnlyList<S07AProviderCase> Cases);

internal sealed record S07AProviderCase(
    string CaseId,
    string QuestionLanguage,
    string ExpectedAnswerLanguage,
    string ContentLanguage,
    string Question,
    string ExpectedOutcome,
    bool ProviderCallExpected,
    string? InsufficiencyPathway,
    bool ProductCorpus,
    string DocumentId,
    string DocumentFormat,
    string DocumentContentSha256,
    IReadOnlyList<string> RelevantLocationKinds,
    IReadOnlyList<string> RequiredFacts,
    bool PromptInjectionPresent,
    string? AttackClass,
    IReadOnlyList<S07AProviderEvidence> ProviderEvidence,
    IReadOnlyList<string> ExpectedCitedChunkIds);

internal sealed record S07AProviderEvidence(
    string ChunkId,
    string ContentLanguage,
    string Text);

internal sealed record S07AProviderObservedResult(
    string CaseId,
    string ExpectedOutcome,
    string ObservedAnswerLanguage,
    IReadOnlyList<string> ObservedCitedChunkIds);
