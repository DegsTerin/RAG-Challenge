// Purpose: Proves the frozen provider-candidate preparation and exact OpenAI request profile through deterministic in-process fake handlers without network or provider access.
using System.Net;
using System.Text;
using System.Text.Json;

namespace RagChallenge.IntegrationTests.S07AProviderHarness;

public sealed class S07AProviderHarnessTests
{
    private static readonly string[] ExpectedRequestProperties =
        ["input", "max_output_tokens", "model", "reasoning", "store", "text"];

    [Fact]
    public void FrozenSuccessorRevisionIsCompleteAndInternallyConsistent()
    {
        var plan = LoadPlan();

        Assert.Equal(60, plan.Cases.Count);
        Assert.Equal(40, plan.Cases.Count(candidate =>
            candidate.ExpectedOutcome == "answered"));
        Assert.Equal(20, plan.Cases.Count(candidate =>
            candidate.ExpectedOutcome == "insufficient-evidence"));
        Assert.Equal(50, plan.Cases.Count(candidate => candidate.ProviderCallExpected));
        Assert.Equal(12, plan.Cases.Count(candidate => candidate.PromptInjectionPresent));
    }

    [Fact]
    public async Task ProviderOptInHarnessUsesOnlyTheInjectedFakeHandler()
    {
        var plan = LoadPlan();
        var handler = new FrozenCampaignFakeHandler(plan);

        var results = await S07AProviderCandidateHarness.ExecuteAsync(
            plan,
            handler,
            FakeCredential);

        Assert.Equal(50, results.Count);
        Assert.Equal(50, handler.CallCount);
        Assert.All(results, result => Assert.False(string.IsNullOrWhiteSpace(result.CaseId)));
    }

    private static S07AProviderHarnessPlan LoadPlan() =>
        S07AProviderHarnessDefinition.LoadFrozenPlan(
            S07AProviderHarnessDefinition.FindRepositoryRoot());

    private static ValueTask<string> FakeCredential(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(S07AProviderHarnessDefinition.FakeCredential);
    }

    private sealed class FrozenCampaignFakeHandler : HttpMessageHandler
    {
        private readonly S07AProviderHarnessPlan plan;
        private readonly Dictionary<string, S07AProviderCase> casesByQuestion;

        internal FrozenCampaignFakeHandler(S07AProviderHarnessPlan plan)
        {
            this.plan = plan;
            casesByQuestion = plan.Cases.Where(candidate => candidate.ProviderCallExpected)
                .ToDictionary(candidate => candidate.Question, StringComparer.Ordinal);
        }

        internal int CallCount { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("https://api.openai.com/v1/responses", request.RequestUri!.AbsoluteUri);
            Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
            Assert.Equal(
                S07AProviderHarnessDefinition.FakeCredential,
                request.Headers.Authorization.Parameter);
            var requestJson = await request.Content!.ReadAsStringAsync(cancellationToken);
            using var body = JsonDocument.Parse(requestJson);
            var root = body.RootElement;
            Assert.Equal(
                ExpectedRequestProperties,
                root.EnumerateObject().Select(property => property.Name)
                    .Order(StringComparer.Ordinal));
            Assert.Equal(S07AProviderHarnessDefinition.ModelId,
                root.GetProperty("model").GetString());
            Assert.False(root.GetProperty("store").GetBoolean());
            Assert.Equal(8192, root.GetProperty("max_output_tokens").GetInt32());
            Assert.Equal("none", root.GetProperty("reasoning").GetProperty("effort").GetString());
            Assert.Equal(
                "current_turn",
                root.GetProperty("reasoning").GetProperty("context").GetString());
            var format = root.GetProperty("text").GetProperty("format");
            Assert.Equal("json_schema", format.GetProperty("type").GetString());
            Assert.Equal("grounded_answer", format.GetProperty("name").GetString());
            Assert.True(format.GetProperty("strict").GetBoolean());
            Assert.Equal(
                plan.SchemaSha256,
                S07AProviderHarnessDefinition.Sha256CanonicalJson(
                    format.GetProperty("schema")));
            var inputs = root.GetProperty("input").EnumerateArray().ToArray();
            Assert.Equal(2, inputs.Length);
            Assert.Equal("developer", inputs[0].GetProperty("role").GetString());
            var prompt = inputs[0].GetProperty("content")[0].GetProperty("text").GetString();
            Assert.Equal(plan.Prompt, prompt);
            Assert.Equal(
                plan.PromptSha256,
                S07AProviderHarnessDefinition.Sha256Utf8(prompt!));
            Assert.Equal("user", inputs[1].GetProperty("role").GetString());
            var userText = inputs[1].GetProperty("content")[0].GetProperty("text").GetString();
            using var userPayload = JsonDocument.Parse(userText!);
            var question = userPayload.RootElement.GetProperty("question").GetString()!;
            var candidate = casesByQuestion[question];
            Assert.Equal(
                candidate.QuestionLanguage,
                userPayload.RootElement.GetProperty("questionLanguage").GetString());
            var evidence = userPayload.RootElement.GetProperty("evidence")
                .EnumerateArray().ToArray();
            Assert.Equal(candidate.ProviderEvidence.Count, evidence.Length);

            for (var index = 0; index < evidence.Length; index++)
            {
                Assert.Equal(candidate.ProviderEvidence[index].ChunkId,
                    evidence[index].GetProperty("chunkId").GetString());
                Assert.Equal(candidate.ProviderEvidence[index].ContentLanguage,
                    evidence[index].GetProperty("contentLanguage").GetString());
                Assert.Equal(candidate.ProviderEvidence[index].Text,
                    evidence[index].GetProperty("text").GetString());
            }

            var answer = candidate.RequiredFacts.Count > 0
                ? candidate.RequiredFacts[0]
                : candidate.QuestionLanguage == "pt-BR"
                    ? "Evidência insuficiente."
                    : "Insufficient evidence.";
            var structured = JsonSerializer.Serialize(new
            {
                answerLanguage = candidate.ExpectedAnswerLanguage,
                answer,
                citedChunkIds = candidate.ExpectedCitedChunkIds,
            });
            var response = JsonSerializer.Serialize(new
            {
                model = S07AProviderHarnessDefinition.ModelId,
                status = "completed",
                output = new[]
                {
                    new
                    {
                        type = "message",
                        role = "assistant",
                        status = "completed",
                        content = new[] { new { type = "output_text", text = structured } },
                    },
                },
            });
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(response, Encoding.UTF8, "application/json"),
            };
        }
    }
}
