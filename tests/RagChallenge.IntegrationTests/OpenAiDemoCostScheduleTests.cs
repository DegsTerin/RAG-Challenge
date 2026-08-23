// Purpose: Verifies that the Render demo price schedule admits only the two exact models and reserves a conservative maximum before provider use.
using System.Security.Cryptography;
using System.Text;

using RagChallenge.Application.ProviderBudget;
using RagChallenge.Infrastructure.Providers;
using RagChallenge.Server.Api.OperationsGovernance;

namespace RagChallenge.IntegrationTests;

public sealed class OpenAiDemoCostScheduleTests
{
    [Fact]
    public void ScheduleDigestMatchesItsCanonicalOfficialPriceIdentity()
    {
        Assert.Equal(
            OpenAiDemoCostSchedule.ScheduleSha256,
            Convert.ToHexString(SHA256.HashData(
                Encoding.UTF8.GetBytes(OpenAiDemoCostSchedule.ScheduleIdentity)))
                .ToLowerInvariant());
    }

    [Fact]
    public void ExactQueryAndGenerationRequestsReceivePositiveConservativeCharges()
    {
        var schedule = new OpenAiDemoCostSchedule();
        var envelope = CreateEnvelope();
        var query = Encoding.UTF8.GetBytes(
            "{\"model\":\"text-embedding-3-small\",\"input\":[\"question\"]}");
        var generation = Encoding.UTF8.GetBytes(
            "{\"model\":\"gpt-5.4-mini-2026-03-17\",\"max_output_tokens\":512}");

        var queryCharge = schedule.CalculateMaximumCharge(
            envelope,
            ProviderBudgetOperationClass.QueryEmbedding,
            query);
        var generationCharge = schedule.CalculateMaximumCharge(
            envelope,
            ProviderBudgetOperationClass.GroundedGeneration,
            generation);

        Assert.InRange(
            queryCharge.Value,
            1,
            ProductProviderBudgetAdmission.DemoQueryEmbeddingLimitMicroUsd);
        Assert.InRange(
            generationCharge.Value,
            1,
            ProductProviderBudgetAdmission.DemoGroundedGenerationLimitMicroUsd);
    }

    [Theory]
    [InlineData("{\"model\":\"other\",\"input\":[\"question\"]}",
        ProviderBudgetOperationClass.QueryEmbedding)]
    [InlineData("{\"model\":\"gpt-5.4-mini-2026-03-17\",\"max_output_tokens\":9000}",
        ProviderBudgetOperationClass.GroundedGeneration)]
    [InlineData("not-json", ProviderBudgetOperationClass.GroundedGeneration)]
    public void DriftedRequestsFailClosed(
        string request,
        ProviderBudgetOperationClass operation)
    {
        Assert.Throws<ProviderBudgetAdmissionUnavailableException>(() =>
            new OpenAiDemoCostSchedule().CalculateMaximumCharge(
                CreateEnvelope(),
                operation,
                Encoding.UTF8.GetBytes(request)));
    }

    private static ProviderBudgetEnvelopeV1 CreateEnvelope()
    {
        var now = DateTimeOffset.UtcNow;
        return new ProviderBudgetEnvelopeV1(
            new ProviderBudgetEnvelopeId("PBE-DEMO-SCHEDULE-TEST"),
            new ProviderBudgetStoreEpochId("PSE-DEMO-SCHEDULE-TEST"),
            new ProviderBudgetScope(
                new ProviderBudgetEnvironmentId("ENV-DEMO-SCHEDULE-TEST"),
                new ProviderBudgetProviderId(OpenAiEmbeddingCostSchedule.ProviderId),
                new ProviderBudgetBillingScopeReference("BILLING-DEMO-SCHEDULE-TEST"),
                new ProviderBudgetModelId(OpenAiDemoCostSchedule.ScopeModelId),
                new ProviderBudgetCurrencyCode(OpenAiEmbeddingCostSchedule.CurrencyCode),
                new ProviderBudgetAccountingUnitId(
                    OpenAiEmbeddingCostSchedule.AccountingUnitId)),
            new ProviderBudgetConfigurationRevision(1),
            new ProviderBudgetLedgerRevision(2),
            new ProviderBudgetRearmRevision(1),
            ProviderBudgetState.Armed,
            new ProviderRuntimeSessionId("PBS-DEMO-SCHEDULE-TEST"),
            new ProviderBudgetCostScheduleId(OpenAiDemoCostSchedule.ScheduleId),
            new ProviderBudgetSha256(OpenAiDemoCostSchedule.ScheduleSha256),
            new ProviderBudgetUnits(
                ProductProviderBudgetAdmission.DemoAggregateLimitMicroUsd),
            new ProviderBudgetUnits(0),
            new ProviderBudgetUnits(0),
            new ProviderBudgetUnits(0),
            [
                new ProviderBudgetOperationBalance(
                    ProviderBudgetOperationClass.AdministrativeIndexEmbedding,
                    new ProviderBudgetUnits(0),
                    new ProviderBudgetUnits(0),
                    new ProviderBudgetUnits(0),
                    new ProviderBudgetUnits(0)),
                new ProviderBudgetOperationBalance(
                    ProviderBudgetOperationClass.QueryEmbedding,
                    new ProviderBudgetUnits(
                        ProductProviderBudgetAdmission.DemoQueryEmbeddingLimitMicroUsd),
                    new ProviderBudgetUnits(0),
                    new ProviderBudgetUnits(0),
                    new ProviderBudgetUnits(0)),
                new ProviderBudgetOperationBalance(
                    ProviderBudgetOperationClass.GroundedGeneration,
                    new ProviderBudgetUnits(
                        ProductProviderBudgetAdmission.DemoGroundedGenerationLimitMicroUsd),
                    new ProviderBudgetUnits(0),
                    new ProviderBudgetUnits(0),
                    new ProviderBudgetUnits(0)),
            ],
            now.AddMinutes(-1),
            now.AddMinutes(10),
            isClosed: false,
            new ProviderBudgetSha256(new string('1', 64)));
    }
}
