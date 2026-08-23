// Purpose: Applies the exact authorised OpenAI embedding price schedule with conservative integer micro-USD accounting before credential lookup or provider egress.
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.ProviderBudget;

namespace RagChallenge.Infrastructure.Providers;

public interface IProviderBudgetMaximumChargeCalculator
{
    ProviderBudgetUnits CalculateMaximumCharge(
        ProviderBudgetEnvelopeV1 envelope,
        ProviderBudgetOperationClass operationClass,
        ReadOnlyMemory<byte> exactRequestBytes);
}

public sealed class OpenAiEmbeddingCostSchedule : IProviderBudgetMaximumChargeCalculator
{
    public const string ScheduleId = "PCS-OPENAI-TEXT-EMBEDDING-3-SMALL-20260823";
    public const string ScheduleSha256 =
        "8c67b5936b4ff6fd08612b14c64402de3189d3692233415181f688a5de52d704";
    public const string ProviderId = "openai";
    public const string ModelId = "text-embedding-3-small";
    public const string CurrencyCode = "USD";
    public const string AccountingUnitId = "USD-MICRO";
    public const long InputPriceMicroUsdPerMillionTokens = 20_000;
    public const long MicroUsdPerUsd = 1_000_000;

    public ProviderBudgetUnits CalculateMaximumCharge(
        ProviderBudgetEnvelopeV1 envelope,
        ProviderBudgetOperationClass operationClass,
        ReadOnlyMemory<byte> exactRequestBytes)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        if (operationClass != ProviderBudgetOperationClass.AdministrativeIndexEmbedding ||
            exactRequestBytes.IsEmpty ||
            !string.Equals(envelope.Scope.ProviderId.Value, ProviderId, StringComparison.Ordinal) ||
            !string.Equals(envelope.Scope.ModelId.Value, ModelId, StringComparison.Ordinal) ||
            !string.Equals(envelope.Scope.CurrencyCode.Value, CurrencyCode, StringComparison.Ordinal) ||
            !string.Equals(
                envelope.Scope.AccountingUnitId.Value,
                AccountingUnitId,
                StringComparison.Ordinal) ||
            !string.Equals(envelope.CostScheduleId.Value, ScheduleId, StringComparison.Ordinal) ||
            !string.Equals(
                envelope.CostScheduleSha256.Value,
                ScheduleSha256,
                StringComparison.Ordinal))
        {
            throw new ProviderBudgetAdmissionUnavailableException();
        }

        // OpenAI prices input tokens. Treating every request byte as one token is a
        // conservative upper bound for the byte-level tokenizer and also includes
        // JSON framing that is not itself billed as embedding input.
        return new ProviderBudgetUnits(CalculateMaximumMicroUsd(exactRequestBytes.Length));
    }

    public static long CalculateMaximumMicroUsd(int exactRequestByteLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(exactRequestByteLength);
        var numerator = checked(
            (long)exactRequestByteLength * InputPriceMicroUsdPerMillionTokens);
        return checked((numerator + MicroUsdPerUsd - 1) / MicroUsdPerUsd);
    }
}

public sealed record OpenAiEmbeddingPlanPolicy
{
    public OpenAiEmbeddingPlanPolicy(
        int exactRequestCount,
        int maximumInputsPerRequest,
        int exactLastRequestInputCount,
        int exactTotalInputCount,
        long maximumTotalMicroUsd)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(exactRequestCount);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumInputsPerRequest);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(exactLastRequestInputCount);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(exactTotalInputCount);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumTotalMicroUsd);

        if (exactLastRequestInputCount > maximumInputsPerRequest ||
            checked((exactRequestCount - 1) * maximumInputsPerRequest +
                exactLastRequestInputCount) != exactTotalInputCount)
        {
            throw new ArgumentException(
                "The embedding plan counts do not form one exact bounded schedule.");
        }

        ExactRequestCount = exactRequestCount;
        MaximumInputsPerRequest = maximumInputsPerRequest;
        ExactLastRequestInputCount = exactLastRequestInputCount;
        ExactTotalInputCount = exactTotalInputCount;
        MaximumTotalMicroUsd = maximumTotalMicroUsd;
    }

    public int ExactRequestCount { get; }

    public int MaximumInputsPerRequest { get; }

    public int ExactLastRequestInputCount { get; }

    public int ExactTotalInputCount { get; }

    public long MaximumTotalMicroUsd { get; }

    public void Validate(IReadOnlyCollection<EmbeddingBatchRequest> requests)
    {
        ArgumentNullException.ThrowIfNull(requests);
        var materialised = requests.ToArray();

        if (materialised.Length != ExactRequestCount ||
            materialised.Sum(request => request.Inputs.Count) != ExactTotalInputCount ||
            materialised.Take(materialised.Length - 1)
                .Any(request => request.Inputs.Count != MaximumInputsPerRequest) ||
            materialised[^1].Inputs.Count != ExactLastRequestInputCount ||
            materialised.Any(request =>
                !string.Equals(
                    request.ExpectedDescriptor.ProviderId,
                    OpenAiEmbeddingCostSchedule.ProviderId,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    request.ExpectedDescriptor.ModelId,
                    OpenAiEmbeddingCostSchedule.ModelId,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    request.ExpectedDescriptor.ModelRevision,
                    OpenAiEmbeddingCostSchedule.ModelId,
                    StringComparison.Ordinal) ||
                request.ExpectedDescriptor.Dimensions != 1536 ||
                request.Inputs.Count > MaximumInputsPerRequest))
        {
            throw new ProviderBudgetAdmissionUnavailableException();
        }
    }
}
