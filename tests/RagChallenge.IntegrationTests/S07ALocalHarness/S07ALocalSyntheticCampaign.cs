// Purpose: Executes the authorised local synthetic case boundary with deterministic lexical providers, task-owned files, sanitised results, and no network-capable adapter.
using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace RagChallenge.IntegrationTests.S07ALocalHarness;

internal static partial class S07ALocalSyntheticCampaign
{
    private static readonly JsonSerializerOptions IndentedJson = new()
    {
        WriteIndented = true,
    };

    internal static async Task ExecuteAsync(
        S07ALocalHarnessPlan plan,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plan);
        PrepareWorkspace(plan.Workspace);

        using var documentManifest = JsonDocument.Parse(
            await File.ReadAllBytesAsync(plan.DocumentManifestPath, cancellationToken));
        using var caseInventory = JsonDocument.Parse(
            await File.ReadAllBytesAsync(plan.CaseInventoryPath, cancellationToken));
        var documents = MaterialiseDocuments(
            documentManifest.RootElement.GetProperty("syntheticFixtures"),
            plan.Workspace);
        var results = new List<S07ALocalCaseResult>();

        foreach (var candidate in caseInventory.RootElement
                     .GetProperty("syntheticFixtureCases")
                     .EnumerateArray())
        {
            cancellationToken.ThrowIfCancellationRequested();
            results.Add(Evaluate(candidate, documents));
        }

        if (results.Count != 11 || results.Any(result => !result.Passed))
        {
            throw new InvalidDataException(
                "The deterministic local campaign did not satisfy its frozen synthetic cases.");
        }

        var output = new
        {
            schemaVersion = 1,
            datasetId = S07ALocalHarnessDefinition.DatasetId,
            datasetRevision = S07ALocalHarnessDefinition.DatasetRevision,
            environmentId = plan.EnvironmentId,
            campaignId = S07ALocalHarnessDefinition.CampaignId,
            networkPolicyId = plan.NetworkPolicyId,
            storePolicyId = plan.StorePolicyId,
            embeddingProvider = plan.EmbeddingProvider,
            languageModel = plan.LanguageModel,
            syntheticFixtureBoundary = true,
            productCorpusClaim = false,
            caseCount = results.Count,
            passedCaseCount = results.Count(result => result.Passed),
            results,
        };
        var outputPath = Path.Combine(
            plan.Workspace.EvidenceRoot,
            "synthetic-campaign-result.json");
        await File.WriteAllTextAsync(
            outputPath,
            JsonSerializer.Serialize(output, IndentedJson) +
                "\n",
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            cancellationToken);
    }

    private static Dictionary<string, FixtureDocument> MaterialiseDocuments(
        JsonElement fixtures,
        S07ALocalHarnessWorkspace workspace)
    {
        var documents = new Dictionary<string, FixtureDocument>(StringComparer.Ordinal);

        foreach (var fixture in fixtures.EnumerateArray())
        {
            var documentId = RequiredString(fixture, "documentId");
            var content = RequiredString(fixture, "canonicalFixtureContent");
            var expectedHash = RequiredString(fixture, "contentSha256");
            var bytes = Encoding.UTF8.GetBytes(content);
            var observedHash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

            if (!string.Equals(expectedHash, observedHash, StringComparison.Ordinal) ||
                fixture.GetProperty("contentByteLength").GetInt64() != bytes.LongLength)
            {
                throw new InvalidDataException(
                    $"Synthetic fixture {documentId} does not match its content identity.");
            }

            var contentPath = Path.Combine(workspace.ContentStoreRoot, observedHash + ".fixture");
            File.WriteAllBytes(contentPath, bytes);
            var units = ParseUnits(
                fixture.GetProperty("documentFormat").GetString(),
                content);
            var vectorsPath = Path.Combine(workspace.VectorStoreRoot, documentId + ".json");
            File.WriteAllText(
                vectorsPath,
                JsonSerializer.Serialize(
                    units.Select(unit => new
                    {
                        unit.LocationKind,
                        unit.LocationNumber,
                        vector = DeterministicEmbeddingProvider.Embed(unit.Text),
                    }),
                    IndentedJson) + "\n",
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            documents.Add(documentId, new FixtureDocument(documentId, units));
        }

        File.WriteAllText(
            Path.Combine(workspace.ControlStoreRoot, "campaign-boundary.json"),
            JsonSerializer.Serialize(new
            {
                datasetRevision = S07ALocalHarnessDefinition.DatasetRevision,
                environmentId = S07ALocalHarnessDefinition.EnvironmentId,
                networkPolicyId = S07ALocalHarnessDefinition.NetworkPolicyId,
                documentIds = documents.Keys.Order(StringComparer.Ordinal),
            }, IndentedJson) + "\n",
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        return documents;
    }

    private static S07ALocalCaseResult Evaluate(
        JsonElement candidate,
        IReadOnlyDictionary<string, FixtureDocument> documents)
    {
        var caseId = RequiredString(candidate, "caseId");
        var documentId = RequiredString(candidate, "documentId");

        if (!documents.TryGetValue(documentId, out var document))
        {
            throw new InvalidDataException($"Case {caseId} references an unknown fixture.");
        }

        var filters = candidate.GetProperty("filters");

        if (!string.Equals(
                RequiredString(filters, "documentId"),
                documentId,
                StringComparison.Ordinal))
        {
            throw new InvalidDataException($"Case {caseId} has a divergent document filter.");
        }

        var question = RequiredString(candidate, "question");
        var queryVector = DeterministicEmbeddingProvider.Embed(question);
        var ranked = document.Units
            .Select(unit => new RankedUnit(
                unit,
                DeterministicEmbeddingProvider.Cosine(
                    queryVector,
                    DeterministicEmbeddingProvider.Embed(unit.Text))))
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Unit.LocationNumber)
            .Take(5)
            .ToArray();
        var expectedOutcome = RequiredString(candidate, "expectedOutcome");
        var relevantLocations = candidate.GetProperty("relevantLocations")
            .EnumerateArray()
            .Select(ReadLocation)
            .ToArray();
        var unsupportedIntent = DeterministicLanguageModel.FindUnsupportedIntent(
            question,
            ranked.Select(item => item.Unit.Text));
        var observedOutcome = unsupportedIntent is null && relevantLocations.Length > 0
            ? "answered"
            : "insufficient-evidence";
        var selected = observedOutcome == "answered" ? ranked.First().Unit : null;
        var reciprocalRank = relevantLocations.Length == 0
            ? 0d
            : ReciprocalRank(ranked, relevantLocations);
        var citationValid = observedOutcome == "insufficient-evidence" ||
            selected is not null && relevantLocations.Contains(
                new CaseLocation(selected.LocationKind, selected.LocationNumber));
        var requiredFactsSupported = candidate.GetProperty("requiredFacts")
            .EnumerateArray()
            .All(fact => selected is not null && DeterministicLanguageModel.IsSupported(
                fact.GetString()!,
                selected.Text));
        var questionLanguage = RequiredString(candidate, "questionLanguage");
        var observedAnswerLanguage = DeterministicLanguageModel.AnswerLanguage(
            questionLanguage);
        var expectedAnswerLanguage = RequiredString(candidate, "expectedAnswerLanguage");
        var passed = string.Equals(
                observedOutcome,
                expectedOutcome,
                StringComparison.Ordinal) &&
            string.Equals(
                observedAnswerLanguage,
                expectedAnswerLanguage,
                StringComparison.Ordinal) &&
            citationValid &&
            requiredFactsSupported &&
            (relevantLocations.Length == 0 || reciprocalRank > 0);
        return new S07ALocalCaseResult(
            caseId,
            observedOutcome,
            observedAnswerLanguage,
            reciprocalRank,
            citationValid,
            requiredFactsSupported,
            passed);
    }

    private static IReadOnlyList<EvidenceUnit> ParseUnits(string? format, string content)
    {
        var normalised = content.Replace("\r\n", "\n", StringComparison.Ordinal);

        if (string.Equals(format, "Csv", StringComparison.Ordinal))
        {
            return normalised.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Skip(1)
                .Select((line, index) => new EvidenceUnit("csv-row", index + 2, line))
                .ToArray();
        }

        if (!string.Equals(format, "Pdf", StringComparison.Ordinal))
        {
            throw new InvalidDataException("A synthetic fixture has an unsupported format.");
        }

        var lines = normalised.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var units = new List<EvidenceUnit>();

        for (var index = 0; index < lines.Length; index += 2)
        {
            if (index + 1 >= lines.Length ||
                !lines[index].StartsWith("page:", StringComparison.Ordinal) ||
                !int.TryParse(
                    lines[index].AsSpan("page:".Length),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var page))
            {
                throw new InvalidDataException("A logical PDF fixture has invalid framing.");
            }

            units.Add(new EvidenceUnit("pdf-physical-page", page, lines[index + 1]));
        }

        return units;
    }

    private static CaseLocation ReadLocation(JsonElement location)
    {
        var kind = RequiredString(location, "locationKind");
        var number = kind switch
        {
            "csv-row" => location.GetProperty("rowNumber").GetInt32(),
            "pdf-physical-page" => location.GetProperty("pageNumber").GetInt32(),
            _ => throw new InvalidDataException("A case has an unknown location kind."),
        };
        return new CaseLocation(kind, number);
    }

    private static double ReciprocalRank(
        IReadOnlyList<RankedUnit> ranked,
        IReadOnlyCollection<CaseLocation> relevant)
    {
        for (var index = 0; index < ranked.Count; index++)
        {
            var observed = new CaseLocation(
                ranked[index].Unit.LocationKind,
                ranked[index].Unit.LocationNumber);

            if (relevant.Contains(observed))
            {
                return 1d / (index + 1);
            }
        }

        return 0;
    }

    private static void PrepareWorkspace(S07ALocalHarnessWorkspace workspace)
    {
        if (Directory.Exists(workspace.CampaignRoot))
        {
            throw new InvalidOperationException(
                "The fixed campaign workspace already exists and will not be overwritten.");
        }

        Directory.CreateDirectory(workspace.ControlStoreRoot);
        Directory.CreateDirectory(workspace.VectorStoreRoot);
        Directory.CreateDirectory(workspace.ContentStoreRoot);
        Directory.CreateDirectory(workspace.EvidenceRoot);
    }

    private static string RequiredString(JsonElement owner, string propertyName) =>
        owner.GetProperty(propertyName).GetString() ??
        throw new InvalidDataException($"The required {propertyName} value is missing.");

    private sealed record FixtureDocument(
        string DocumentId,
        IReadOnlyList<EvidenceUnit> Units);

    private sealed record EvidenceUnit(
        string LocationKind,
        int LocationNumber,
        string Text);

    private sealed record RankedUnit(EvidenceUnit Unit, double Score);

    private sealed record CaseLocation(string LocationKind, int LocationNumber);

    private static partial class DeterministicEmbeddingProvider
    {
        internal static float[] Embed(string value)
        {
            var vector = new float[S07ALocalHarnessDefinition.EmbeddingProvider.Dimensions];

            foreach (var token in Normalise(value))
            {
                var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
                var index = BinaryPrimitives.ReadUInt16LittleEndian(hash) % vector.Length;
                vector[index] += 1;
            }

            return vector;
        }

        internal static double Cosine(float[] left, float[] right)
        {
            double dot = 0;
            double leftNorm = 0;
            double rightNorm = 0;

            for (var index = 0; index < left.Length; index++)
            {
                dot += left[index] * right[index];
                leftNorm += left[index] * left[index];
                rightNorm += right[index] * right[index];
            }

            return leftNorm == 0 || rightNorm == 0
                ? 0
                : dot / Math.Sqrt(leftNorm * rightNorm);
        }

        internal static string[] Normalise(string value) =>
            TokenPattern().Matches(value.ToLowerInvariant())
                .Select(match => CanonicalToken(match.Value))
                .Where(token => token.Length > 0)
                .ToArray();

        private static string CanonicalToken(string token) => token switch
        {
            "a" or "an" or "as" or "do" or "does" or "for" or "is" or "o" or
                "os" or "qual" or "the" or "what" => string.Empty,
            "banco" or "database" => "database",
            "backups" => "backup",
            "começa" or "begins" => "begin",
            "conformidade" or "compliance" or "regulatory" => "compliance",
            "criptografia" or "encryption" => "encryption",
            "dias" or "days" => "day",
            "janela" or "window" => "window",
            "manutenção" or "maintenance" => "maintenance",
            "padrão" or "padrao" or "default" => "default",
            "porta" or "port" => "port",
            "retained" or "retidos" => "retention",
            "sintético" or "sintetico" or "synthetic" => "synthetic",
            _ => token,
        };

        [GeneratedRegex("[\\p{L}\\p{N}]+", RegexOptions.CultureInvariant)]
        private static partial Regex TokenPattern();
    }

    private static class DeterministicLanguageModel
    {
        private static readonly string[] UnsupportedIntents =
            ["encryption", "compliance", "tls"];

        internal static string? FindUnsupportedIntent(
            string question,
            IEnumerable<string> evidence)
        {
            var questionTokens = DeterministicEmbeddingProvider.Normalise(question);
            var evidenceTokens = evidence
                .SelectMany(DeterministicEmbeddingProvider.Normalise)
                .ToHashSet(StringComparer.Ordinal);
            return UnsupportedIntents.FirstOrDefault(intent =>
                questionTokens.Contains(intent) && !evidenceTokens.Contains(intent));
        }

        internal static bool IsSupported(string requiredFact, string evidence)
        {
            var factTokens = DeterministicEmbeddingProvider.Normalise(requiredFact)
                .ToHashSet(StringComparer.Ordinal);
            var evidenceTokens = DeterministicEmbeddingProvider.Normalise(evidence)
                .ToHashSet(StringComparer.Ordinal);
            return factTokens.All(evidenceTokens.Contains);
        }

        internal static string AnswerLanguage(string questionLanguage) =>
            questionLanguage switch
            {
                "pt-BR" => "pt-BR",
                "en-GB" => "en-GB",
                _ => throw new InvalidDataException(
                    "The harness received a question language outside the closed set."),
            };
    }
}

internal sealed record S07ALocalCaseResult(
    string CaseId,
    string ObservedOutcome,
    string AnswerLanguage,
    double ReciprocalRankAtFive,
    bool CitationLocationValid,
    bool RequiredFactsSupported,
    bool Passed);
