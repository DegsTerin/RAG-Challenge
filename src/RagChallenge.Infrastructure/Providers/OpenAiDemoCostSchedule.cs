// Purpose: Prices the bounded OpenAI demo runtime conservatively across query embeddings and grounded responses before credential lookup or provider egress.
using System.Text.Json;

using RagChallenge.Application.ProviderBudget;

namespace RagChallenge.Infrastructure.Providers;

public sealed class OpenAiDemoCostSchedule : IProviderBudgetMaximumChargeCalculator
{
    public const string ScheduleId = "PCS-OPENAI-RAG-DEMO-20260823";
    public const string ScheduleIdentity =
        "openai-demo-cost-schedule-v1\n" +
        "query-model=text-embedding-3-small\n" +
        "query-input-usd-per-million=0.02\n" +
        "generation-model=gpt-5.4-mini-2026-03-17\n" +
        "generation-input-usd-per-million=0.75\n" +
        "generation-output-usd-per-million=4.50\n" +
        "retrieved-at-utc=2026-08-23T22:00:00Z\n" +
        "query-source=https://developers.openai.com/api/docs/models/text-embedding-3-small\n" +
        "generation-source=https://developers.openai.com/api/docs/models/gpt-5.4-mini\n";
    public const string ScheduleSha256 =
        "de166efc0bd2dfd3e989a0291ec0e8be872d42fd0ab732009ef76db7da37b81c";
    public const string ScopeModelId = "openai-rag-demo-v1";
    public const long InputPriceMicroUsdPerMillionTokens = 750_000;
    public const long OutputPriceMicroUsdPerMillionTokens = 4_500_000;

    public ProviderBudgetUnits CalculateMaximumCharge(
        ProviderBudgetEnvelopeV1 envelope,
        ProviderBudgetOperationClass operationClass,
        ReadOnlyMemory<byte> exactRequestBytes)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        if (exactRequestBytes.IsEmpty ||
            !string.Equals(
                envelope.Scope.ProviderId.Value,
                OpenAiEmbeddingCostSchedule.ProviderId,
                StringComparison.Ordinal) ||
            !string.Equals(
                envelope.Scope.ModelId.Value,
                ScopeModelId,
                StringComparison.Ordinal) ||
            !string.Equals(
                envelope.Scope.CurrencyCode.Value,
                OpenAiEmbeddingCostSchedule.CurrencyCode,
                StringComparison.Ordinal) ||
            !string.Equals(
                envelope.Scope.AccountingUnitId.Value,
                OpenAiEmbeddingCostSchedule.AccountingUnitId,
                StringComparison.Ordinal) ||
            !string.Equals(envelope.CostScheduleId.Value, ScheduleId, StringComparison.Ordinal) ||
            !string.Equals(
                envelope.CostScheduleSha256.Value,
                ScheduleSha256,
                StringComparison.Ordinal))
        {
            throw new ProviderBudgetAdmissionUnavailableException();
        }

        return operationClass switch
        {
            ProviderBudgetOperationClass.QueryEmbedding =>
                CalculateQueryEmbeddingMaximum(exactRequestBytes),
            ProviderBudgetOperationClass.GroundedGeneration =>
                CalculateGroundedGenerationMaximum(exactRequestBytes),
            _ => throw new ProviderBudgetAdmissionUnavailableException(),
        };
    }

    private static ProviderBudgetUnits CalculateQueryEmbeddingMaximum(
        ReadOnlyMemory<byte> exactRequestBytes)
    {
        try
        {
            using var document = JsonDocument.Parse(exactRequestBytes);
            if (!document.RootElement.TryGetProperty("model", out var model) ||
                model.ValueKind != JsonValueKind.String ||
                !string.Equals(
                    model.GetString(),
                    OpenAiEmbeddingCostSchedule.ModelId,
                    StringComparison.Ordinal))
            {
                throw new ProviderBudgetAdmissionUnavailableException();
            }

            return new ProviderBudgetUnits(
                OpenAiEmbeddingCostSchedule.CalculateMaximumMicroUsd(
                    exactRequestBytes.Length));
        }
        catch (JsonException)
        {
            throw new ProviderBudgetAdmissionUnavailableException();
        }
    }

    private static ProviderBudgetUnits CalculateGroundedGenerationMaximum(
        ReadOnlyMemory<byte> exactRequestBytes)
    {
        try
        {
            using var document = JsonDocument.Parse(exactRequestBytes);
            var root = document.RootElement;
            if (!root.TryGetProperty("model", out var model) ||
                model.ValueKind != JsonValueKind.String ||
                !string.Equals(
                    model.GetString(),
                    OpenAiLanguageModelOptions.MvpModelId,
                    StringComparison.Ordinal) ||
                !root.TryGetProperty("max_output_tokens", out var maximumOutputTokens) ||
                !maximumOutputTokens.TryGetInt32(out var outputTokens) ||
                outputTokens is < 256 or > 8192)
            {
                throw new ProviderBudgetAdmissionUnavailableException();
            }

            // Treating every UTF-8 request byte as one input token deliberately
            // overstates usage while the declared output ceiling is charged in full.
            var input = CalculateCeiling(
                exactRequestBytes.Length,
                InputPriceMicroUsdPerMillionTokens);
            var output = CalculateCeiling(outputTokens, OutputPriceMicroUsdPerMillionTokens);
            return new ProviderBudgetUnits(checked(input + output));
        }
        catch (JsonException)
        {
            throw new ProviderBudgetAdmissionUnavailableException();
        }
    }

    private static long CalculateCeiling(long units, long priceMicroUsdPerMillionUnits)
    {
        var numerator = checked(units * priceMicroUsdPerMillionUnits);
        return checked(
            (numerator + OpenAiEmbeddingCostSchedule.MicroUsdPerUsd - 1) /
            OpenAiEmbeddingCostSchedule.MicroUsdPerUsd);
    }
}
