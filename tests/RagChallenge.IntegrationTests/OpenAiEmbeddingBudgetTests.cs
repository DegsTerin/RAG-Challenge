// Purpose: Verifies the authorised OpenAI embedding schedule and exact administrative plan fail closed before credential lookup or provider transport.
using System.Net;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.ProviderBudget;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Infrastructure.Providers;

namespace RagChallenge.IntegrationTests;

public sealed class OpenAiEmbeddingBudgetTests
{
    private static readonly EmbeddingProviderDescriptor Descriptor = new(
        OpenAiEmbeddingCostSchedule.ProviderId,
        OpenAiEmbeddingCostSchedule.ModelId,
        OpenAiEmbeddingCostSchedule.ModelId,
        dimensions: 1536);

    [Theory]
    [InlineData(1, 1)]
    [InlineData(50, 1)]
    [InlineData(51, 2)]
    [InlineData(1_000_000, 20_000)]
    public void CostScheduleRoundsUpConservatively(
        int requestByteLength,
        long expectedMicroUsd)
    {
        Assert.Equal(
            expectedMicroUsd,
            OpenAiEmbeddingCostSchedule.CalculateMaximumMicroUsd(requestByteLength));
    }

    [Fact]
    public void ExactAdministrativePlanAcceptsOnlyFiftyTwoBoundedRequests()
    {
        var policy = CreatePolicy();
        var requests = CreateExactPlan();

        policy.Validate(requests);

        Assert.Equal(52, requests.Length);
        Assert.All(requests.Take(51), request => Assert.Equal(64, request.Inputs.Count));
        Assert.Equal(18, requests[^1].Inputs.Count);
        Assert.Equal(3_282, requests.Sum(request => request.Inputs.Count));
    }

    [Fact]
    public void AdministrativePlanRejectsCountAndDescriptorDrift()
    {
        var policy = CreatePolicy();
        var missingRequest = CreateExactPlan().Take(51).ToArray();
        var driftedDescriptor = new EmbeddingProviderDescriptor(
            OpenAiEmbeddingCostSchedule.ProviderId,
            OpenAiEmbeddingCostSchedule.ModelId,
            OpenAiEmbeddingCostSchedule.ModelId,
            dimensions: 3072);
        var descriptorDrift = CreateExactPlan().ToArray();
        descriptorDrift[0] = new EmbeddingBatchRequest(
            driftedDescriptor,
            descriptorDrift[0].Inputs,
            maximumUtf8Bytes: 1_048_576);

        Assert.Throws<ProviderBudgetAdmissionUnavailableException>(() =>
            policy.Validate(missingRequest));
        Assert.Throws<ProviderBudgetAdmissionUnavailableException>(() =>
            policy.Validate(descriptorDrift));
    }

    [Fact]
    public async Task PlanAboveItsMaximumStopsBeforePreparationCredentialOrTransport()
    {
        var credentialReads = 0;
        var preparations = 0;
        var handler = new CountingHandler();
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.openai.com/", UriKind.Absolute),
            Timeout = TimeSpan.FromSeconds(25),
        };
        var adapter = new OpenAiHttpEmbeddingProvider(
            client,
            _ =>
            {
                credentialReads++;
                return ValueTask.FromResult("<synthetic-credential>");
            },
            new ProviderBudgetAdmissionGate(
                new FakeDeterministicProviderBudgetLedger(),
                new ProviderBudgetAdmissionContext(
                    new ProviderBudgetEnvelopeId("PBE-UNREACHED"),
                    new ProviderRuntimeSessionId("PRS-UNREACHED"),
                    new ProviderBudgetAuthorityReference("AUTH-UNREACHED")),
                _ => ValueTask.CompletedTask),
            ProviderBudgetOperationClass.AdministrativeIndexEmbedding,
            new OpenAiEmbeddingPlanPolicy(52, 64, 18, 3_282, maximumTotalMicroUsd: 1),
            _ =>
            {
                preparations++;
                return Task.CompletedTask;
            });

        await Assert.ThrowsAsync<ProviderBudgetAdmissionUnavailableException>(() =>
            adapter.ValidatePlanAsync(CreateExactPlan()));

        Assert.Equal(0, preparations);
        Assert.Equal(0, credentialReads);
        Assert.Equal(0, handler.CallCount);
    }

    private static OpenAiEmbeddingPlanPolicy CreatePolicy() =>
        new(52, 64, 18, 3_282, OpenAiEmbeddingCostSchedule.MicroUsdPerUsd);

    private static EmbeddingBatchRequest[] CreateExactPlan() =>
        Enumerable.Range(0, 52)
            .Select(requestIndex => new EmbeddingBatchRequest(
                Descriptor,
                Enumerable.Range(0, requestIndex == 51 ? 18 : 64)
                    .Select(inputIndex => $"synthetic-{requestIndex:D2}-{inputIndex:D2}")
                    .ToArray(),
                maximumUtf8Bytes: 1_048_576))
            .ToArray();

    private sealed class CountingHandler : HttpMessageHandler
    {
        internal int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
