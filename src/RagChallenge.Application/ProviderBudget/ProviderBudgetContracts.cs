// Purpose: Defines the immutable fail-closed provider-budget policy contract owned by Application; persistence, rearming, pricing, credentials and provider egress remain outer authorised concerns.
using System.Collections.ObjectModel;

using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Application.ProviderBudget;

public enum ProviderBudgetState
{
    Disarmed,
    Armed,
    Tripped,
    Exhausted,
    ReconciliationRequired,
    Expired,
}

public enum ProviderBudgetOperationClass
{
    AdministrativeIndexEmbedding,
    QueryEmbedding,
    GroundedGeneration,
}

public enum ProviderBudgetReservationStatus
{
    Reserved,
    DispatchStarted,
    Committed,
    ReleasedPreSend,
    IndeterminateCommitted,
    OverrunCommitted,
}

public enum ProviderBudgetCommitmentKind
{
    Observed,
    IndeterminateMaximum,
    OverrunMaximum,
}

public enum ProviderBudgetReleaseProofKind
{
    BeforeCredentialLookup,
    TransportConfirmedZeroRequestBytes,
}

public enum ProviderBudgetAdmissionOutcome
{
    Admitted,
    Replay,
    Rejected,
    Conflict,
}

public enum ProviderBudgetAdmissionRejection
{
    EnvelopeUnavailable,
    Disarmed,
    Tripped,
    Exhausted,
    ReconciliationRequired,
    Expired,
    Closed,
    RuntimeSessionMismatch,
    StoreEpochMismatch,
    ConfigurationRevisionMismatch,
    LedgerRevisionMismatch,
    ScopeMismatch,
    CostScheduleMismatch,
    AggregateLimitExceeded,
    OperationLimitExceeded,
}

public enum ProviderBudgetTransitionOutcome
{
    Applied,
    Replay,
    Rejected,
    Conflict,
}

public enum ProviderBudgetTransitionRejection
{
    EnvelopeUnavailable,
    ReservationUnavailable,
    EnvelopeNotArmed,
    LedgerRevisionMismatch,
    ReservationRevisionMismatch,
    InvalidReservationState,
    InvalidTransitionTime,
}

public sealed record ProviderBudgetEnvelopeId : StableIdentifier
{
    public ProviderBudgetEnvelopeId(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record ProviderBudgetStoreEpochId : StableIdentifier
{
    public ProviderBudgetStoreEpochId(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record ProviderRequestId : StableIdentifier
{
    public ProviderRequestId(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record ProviderRuntimeSessionId : StableIdentifier
{
    public ProviderRuntimeSessionId(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record ProviderBudgetAuthorityReference : StableIdentifier
{
    public ProviderBudgetAuthorityReference(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record ProviderBudgetEnvironmentId : StableIdentifier
{
    public ProviderBudgetEnvironmentId(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record ProviderBudgetProviderId : StableIdentifier
{
    public ProviderBudgetProviderId(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record ProviderBudgetBillingScopeReference : StableIdentifier
{
    public ProviderBudgetBillingScopeReference(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record ProviderBudgetModelId : StableIdentifier
{
    public ProviderBudgetModelId(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record ProviderBudgetAccountingUnitId : StableIdentifier
{
    public ProviderBudgetAccountingUnitId(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record ProviderBudgetCostScheduleId : StableIdentifier
{
    public ProviderBudgetCostScheduleId(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record ProviderBudgetOutcomeCode : StableIdentifier
{
    public ProviderBudgetOutcomeCode(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record ProviderBudgetSha256 : LowercaseSha256
{
    public ProviderBudgetSha256(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record ProviderBudgetCurrencyCode
{
    public ProviderBudgetCurrencyCode(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Length != 3 || value.Any(character => character is < 'A' or > 'Z'))
        {
            throw new ArgumentException(
                "A provider-budget currency code must contain exactly three uppercase ASCII letters.",
                nameof(value));
        }

        Value = value;
    }

    public string Value { get; }
}

public sealed record ProviderBudgetConfigurationRevision : PositiveRevision
{
    public ProviderBudgetConfigurationRevision(long value)
        : base(value, nameof(value))
    {
    }
}

public sealed record ProviderBudgetLedgerRevision : PositiveRevision
{
    public ProviderBudgetLedgerRevision(long value)
        : base(value, nameof(value))
    {
    }
}

public readonly record struct ProviderBudgetRearmRevision
{
    public ProviderBudgetRearmRevision(long value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        Value = value;
    }

    public long Value { get; }
}

public readonly record struct ProviderBudgetUnits
{
    public ProviderBudgetUnits(long value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        Value = value;
    }

    public long Value { get; }
}

public sealed record ProviderBudgetScope
{
    public ProviderBudgetScope(
        ProviderBudgetEnvironmentId environmentId,
        ProviderBudgetProviderId providerId,
        ProviderBudgetBillingScopeReference billingScopeReference,
        ProviderBudgetModelId modelId,
        ProviderBudgetCurrencyCode currencyCode,
        ProviderBudgetAccountingUnitId accountingUnitId)
    {
        EnvironmentId = environmentId ?? throw new ArgumentNullException(nameof(environmentId));
        ProviderId = providerId ?? throw new ArgumentNullException(nameof(providerId));
        BillingScopeReference = billingScopeReference ??
            throw new ArgumentNullException(nameof(billingScopeReference));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        CurrencyCode = currencyCode ?? throw new ArgumentNullException(nameof(currencyCode));
        AccountingUnitId = accountingUnitId ??
            throw new ArgumentNullException(nameof(accountingUnitId));
    }

    public ProviderBudgetEnvironmentId EnvironmentId { get; }

    public ProviderBudgetProviderId ProviderId { get; }

    public ProviderBudgetBillingScopeReference BillingScopeReference { get; }

    public ProviderBudgetModelId ModelId { get; }

    public ProviderBudgetCurrencyCode CurrencyCode { get; }

    public ProviderBudgetAccountingUnitId AccountingUnitId { get; }
}

public sealed record ProviderBudgetOperationBalance
{
    public ProviderBudgetOperationBalance(
        ProviderBudgetOperationClass operationClass,
        ProviderBudgetUnits allocationLimit,
        ProviderBudgetUnits committed,
        ProviderBudgetUnits reserved,
        ProviderBudgetUnits indeterminate)
    {
        if (!Enum.IsDefined(operationClass))
        {
            throw new ArgumentOutOfRangeException(nameof(operationClass));
        }

        if (committed.Value > allocationLimit.Value ||
            reserved.Value > allocationLimit.Value - committed.Value ||
            indeterminate.Value > committed.Value)
        {
            throw new ArgumentException(
                "Provider-budget operation amounts exceed their allocation or accounting bounds.");
        }

        OperationClass = operationClass;
        AllocationLimit = allocationLimit;
        Committed = committed;
        Reserved = reserved;
        Indeterminate = indeterminate;
    }

    public ProviderBudgetOperationClass OperationClass { get; }

    public ProviderBudgetUnits AllocationLimit { get; }

    public ProviderBudgetUnits Committed { get; }

    public ProviderBudgetUnits Reserved { get; }

    public ProviderBudgetUnits Indeterminate { get; }
}

public sealed record ProviderBudgetEnvelopeV1
{
    public const int SchemaVersion = 1;

    public ProviderBudgetEnvelopeV1(
        ProviderBudgetEnvelopeId envelopeId,
        ProviderBudgetStoreEpochId storeEpochId,
        ProviderBudgetScope scope,
        ProviderBudgetConfigurationRevision configurationRevision,
        ProviderBudgetLedgerRevision ledgerRevision,
        ProviderBudgetRearmRevision rearmRevision,
        ProviderBudgetState state,
        ProviderRuntimeSessionId? runtimeSessionId,
        ProviderBudgetCostScheduleId costScheduleId,
        ProviderBudgetSha256 costScheduleSha256,
        ProviderBudgetUnits aggregateLimit,
        ProviderBudgetUnits aggregateCommitted,
        ProviderBudgetUnits aggregateReserved,
        ProviderBudgetUnits aggregateIndeterminate,
        IEnumerable<ProviderBudgetOperationBalance> operationBalances,
        DateTimeOffset effectiveAtUtc,
        DateTimeOffset expiresAtUtc,
        bool isClosed,
        ProviderBudgetSha256 currentLedgerSha256)
    {
        EnvelopeId = envelopeId ?? throw new ArgumentNullException(nameof(envelopeId));
        StoreEpochId = storeEpochId ?? throw new ArgumentNullException(nameof(storeEpochId));
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        ConfigurationRevision = configurationRevision ??
            throw new ArgumentNullException(nameof(configurationRevision));
        LedgerRevision = ledgerRevision ?? throw new ArgumentNullException(nameof(ledgerRevision));
        CostScheduleId = costScheduleId ?? throw new ArgumentNullException(nameof(costScheduleId));
        CostScheduleSha256 = costScheduleSha256 ??
            throw new ArgumentNullException(nameof(costScheduleSha256));
        CurrentLedgerSha256 = currentLedgerSha256 ??
            throw new ArgumentNullException(nameof(currentLedgerSha256));
        ArgumentNullException.ThrowIfNull(operationBalances);

        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state));
        }

        if (state == ProviderBudgetState.Armed && runtimeSessionId is null)
        {
            throw new ArgumentException(
                "An armed provider-budget envelope requires an exact runtime-session identity.",
                nameof(runtimeSessionId));
        }

        RequireUtc(effectiveAtUtc, nameof(effectiveAtUtc));
        RequireUtc(expiresAtUtc, nameof(expiresAtUtc));

        if (expiresAtUtc <= effectiveAtUtc)
        {
            throw new ArgumentException(
                "Provider-budget expiry must be later than its effective instant.",
                nameof(expiresAtUtc));
        }

        if (aggregateCommitted.Value > aggregateLimit.Value ||
            aggregateReserved.Value > aggregateLimit.Value - aggregateCommitted.Value ||
            aggregateIndeterminate.Value > aggregateCommitted.Value)
        {
            throw new ArgumentException(
                "Provider-budget aggregate amounts exceed their accounting bounds.");
        }

        var balances = operationBalances.OrderBy(balance => balance.OperationClass).ToArray();
        var expectedOperations = Enum.GetValues<ProviderBudgetOperationClass>();

        if (balances.Length != expectedOperations.Length ||
            !balances.Select(balance => balance.OperationClass).SequenceEqual(expectedOperations))
        {
            throw new ArgumentException(
                "A provider-budget envelope requires exactly one balance for every closed operation class.",
                nameof(operationBalances));
        }

        RequireAggregateMatch(
            balances,
            aggregateLimit,
            aggregateCommitted,
            aggregateReserved,
            aggregateIndeterminate);

        if (isClosed &&
            (state == ProviderBudgetState.Armed ||
             aggregateReserved.Value != 0 ||
             aggregateIndeterminate.Value != 0))
        {
            throw new ArgumentException(
                "A closed provider-budget envelope cannot be armed or retain reserved or indeterminate units.",
                nameof(isClosed));
        }

        RearmRevision = rearmRevision;
        State = state;
        RuntimeSessionId = runtimeSessionId;
        AggregateLimit = aggregateLimit;
        AggregateCommitted = aggregateCommitted;
        AggregateReserved = aggregateReserved;
        AggregateIndeterminate = aggregateIndeterminate;
        OperationBalances = Array.AsReadOnly(balances);
        EffectiveAtUtc = effectiveAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        IsClosed = isClosed;
    }

    public ProviderBudgetEnvelopeId EnvelopeId { get; }

    public ProviderBudgetStoreEpochId StoreEpochId { get; }

    public ProviderBudgetScope Scope { get; }

    public ProviderBudgetConfigurationRevision ConfigurationRevision { get; }

    public ProviderBudgetLedgerRevision LedgerRevision { get; }

    public ProviderBudgetRearmRevision RearmRevision { get; }

    public ProviderBudgetState State { get; }

    public ProviderRuntimeSessionId? RuntimeSessionId { get; }

    public ProviderBudgetCostScheduleId CostScheduleId { get; }

    public ProviderBudgetSha256 CostScheduleSha256 { get; }

    public ProviderBudgetUnits AggregateLimit { get; }

    public ProviderBudgetUnits AggregateCommitted { get; }

    public ProviderBudgetUnits AggregateReserved { get; }

    public ProviderBudgetUnits AggregateIndeterminate { get; }

    public ReadOnlyCollection<ProviderBudgetOperationBalance> OperationBalances { get; }

    public DateTimeOffset EffectiveAtUtc { get; }

    public DateTimeOffset ExpiresAtUtc { get; }

    public bool IsClosed { get; }

    public ProviderBudgetSha256 CurrentLedgerSha256 { get; }

    internal ProviderBudgetEnvelopeV1 WithLedger(
        ProviderBudgetLedgerRevision ledgerRevision,
        ProviderBudgetState state,
        ProviderBudgetUnits aggregateCommitted,
        ProviderBudgetUnits aggregateReserved,
        ProviderBudgetUnits aggregateIndeterminate,
        IEnumerable<ProviderBudgetOperationBalance> operationBalances,
        ProviderBudgetSha256 currentLedgerSha256) =>
        new(
            EnvelopeId,
            StoreEpochId,
            Scope,
            ConfigurationRevision,
            ledgerRevision,
            RearmRevision,
            state,
            RuntimeSessionId,
            CostScheduleId,
            CostScheduleSha256,
            AggregateLimit,
            aggregateCommitted,
            aggregateReserved,
            aggregateIndeterminate,
            operationBalances,
            EffectiveAtUtc,
            ExpiresAtUtc,
            IsClosed,
            currentLedgerSha256);

    private static void RequireAggregateMatch(
        IReadOnlyCollection<ProviderBudgetOperationBalance> balances,
        ProviderBudgetUnits aggregateLimit,
        ProviderBudgetUnits aggregateCommitted,
        ProviderBudgetUnits aggregateReserved,
        ProviderBudgetUnits aggregateIndeterminate)
    {
        var remainingAllocation = aggregateLimit.Value;
        var remainingCommitted = aggregateCommitted.Value;
        var remainingReserved = aggregateReserved.Value;
        var remainingIndeterminate = aggregateIndeterminate.Value;

        foreach (var balance in balances)
        {
            if (balance.AllocationLimit.Value > remainingAllocation ||
                balance.Committed.Value > remainingCommitted ||
                balance.Reserved.Value > remainingReserved ||
                balance.Indeterminate.Value > remainingIndeterminate)
            {
                throw new ArgumentException(
                    "Provider-budget operation amounts do not fit their aggregate totals.",
                    nameof(balances));
            }

            remainingAllocation -= balance.AllocationLimit.Value;
            remainingCommitted -= balance.Committed.Value;
            remainingReserved -= balance.Reserved.Value;
            remainingIndeterminate -= balance.Indeterminate.Value;
        }

        if (remainingCommitted != 0 || remainingReserved != 0 || remainingIndeterminate != 0)
        {
            throw new ArgumentException(
                "Provider-budget aggregate usage must equal the sum of operation usage.",
                nameof(balances));
        }
    }

    private static void RequireUtc(DateTimeOffset value, string parameterName)
    {
        if (value.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Provider-budget instants must use an explicit UTC offset.",
                parameterName);
        }
    }
}

public sealed record ProviderBudgetAdmissionRequest
{
    public ProviderBudgetAdmissionRequest(
        ProviderRequestId providerRequestId,
        ProviderBudgetEnvelopeId envelopeId,
        ProviderBudgetStoreEpochId storeEpochId,
        ProviderBudgetConfigurationRevision expectedConfigurationRevision,
        ProviderBudgetLedgerRevision expectedLedgerRevision,
        ProviderRuntimeSessionId runtimeSessionId,
        ProviderBudgetScope scope,
        ProviderBudgetCostScheduleId costScheduleId,
        ProviderBudgetSha256 costScheduleSha256,
        ProviderBudgetOperationClass operationClass,
        ProviderBudgetAuthorityReference operationAuthorityReference,
        ProviderBudgetSha256 requestPlanSha256,
        ProviderBudgetSha256 requestSha256,
        ProviderBudgetSha256 maximumChargeBasisSha256,
        ProviderBudgetSha256 bindingSha256,
        ProviderBudgetUnits maximumCharge,
        DateTimeOffset requestedAtUtc)
    {
        ProviderRequestId = providerRequestId ??
            throw new ArgumentNullException(nameof(providerRequestId));
        EnvelopeId = envelopeId ?? throw new ArgumentNullException(nameof(envelopeId));
        StoreEpochId = storeEpochId ?? throw new ArgumentNullException(nameof(storeEpochId));
        ExpectedConfigurationRevision = expectedConfigurationRevision ??
            throw new ArgumentNullException(nameof(expectedConfigurationRevision));
        ExpectedLedgerRevision = expectedLedgerRevision ??
            throw new ArgumentNullException(nameof(expectedLedgerRevision));
        RuntimeSessionId = runtimeSessionId ??
            throw new ArgumentNullException(nameof(runtimeSessionId));
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        CostScheduleId = costScheduleId ?? throw new ArgumentNullException(nameof(costScheduleId));
        CostScheduleSha256 = costScheduleSha256 ??
            throw new ArgumentNullException(nameof(costScheduleSha256));
        OperationAuthorityReference = operationAuthorityReference ??
            throw new ArgumentNullException(nameof(operationAuthorityReference));
        RequestPlanSha256 = requestPlanSha256 ??
            throw new ArgumentNullException(nameof(requestPlanSha256));
        RequestSha256 = requestSha256 ?? throw new ArgumentNullException(nameof(requestSha256));
        MaximumChargeBasisSha256 = maximumChargeBasisSha256 ??
            throw new ArgumentNullException(nameof(maximumChargeBasisSha256));
        BindingSha256 = bindingSha256 ?? throw new ArgumentNullException(nameof(bindingSha256));

        if (!Enum.IsDefined(operationClass))
        {
            throw new ArgumentOutOfRangeException(nameof(operationClass));
        }

        if (requestedAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Provider-budget admission instants must use an explicit UTC offset.",
                nameof(requestedAtUtc));
        }

        OperationClass = operationClass;
        MaximumCharge = maximumCharge;
        RequestedAtUtc = requestedAtUtc;
    }

    public ProviderRequestId ProviderRequestId { get; }

    public ProviderBudgetEnvelopeId EnvelopeId { get; }

    public ProviderBudgetStoreEpochId StoreEpochId { get; }

    public ProviderBudgetConfigurationRevision ExpectedConfigurationRevision { get; }

    public ProviderBudgetLedgerRevision ExpectedLedgerRevision { get; }

    public ProviderRuntimeSessionId RuntimeSessionId { get; }

    public ProviderBudgetScope Scope { get; }

    public ProviderBudgetCostScheduleId CostScheduleId { get; }

    public ProviderBudgetSha256 CostScheduleSha256 { get; }

    public ProviderBudgetOperationClass OperationClass { get; }

    public ProviderBudgetAuthorityReference OperationAuthorityReference { get; }

    public ProviderBudgetSha256 RequestPlanSha256 { get; }

    public ProviderBudgetSha256 RequestSha256 { get; }

    public ProviderBudgetSha256 MaximumChargeBasisSha256 { get; }

    public ProviderBudgetSha256 BindingSha256 { get; }

    public ProviderBudgetUnits MaximumCharge { get; }

    public DateTimeOffset RequestedAtUtc { get; }
}

public sealed class ProviderBudgetReservation
{
    public ProviderBudgetReservation(
        ProviderRequestId providerRequestId,
        ProviderBudgetEnvelopeId envelopeId,
        ProviderBudgetStoreEpochId storeEpochId,
        ProviderBudgetConfigurationRevision configurationRevision,
        ProviderBudgetOperationClass operationClass,
        ProviderBudgetAuthorityReference operationAuthorityReference,
        ProviderBudgetSha256 requestPlanSha256,
        ProviderBudgetSha256 requestSha256,
        ProviderBudgetSha256 maximumChargeBasisSha256,
        ProviderBudgetSha256 costScheduleSha256,
        ProviderBudgetSha256 bindingSha256,
        ProviderBudgetUnits maximumCharge,
        ProviderRuntimeSessionId admittedRuntimeSessionId,
        ProviderBudgetLedgerRevision admissionLedgerRevision,
        long currentReservationRevision,
        ProviderBudgetReservationStatus status,
        DateTimeOffset admittedAtUtc,
        DateTimeOffset? dispatchStartedAtUtc = null,
        DateTimeOffset? terminalAtUtc = null,
        ProviderBudgetLedgerRevision? terminalLedgerRevision = null)
    {
        ProviderRequestId = providerRequestId ??
            throw new ArgumentNullException(nameof(providerRequestId));
        EnvelopeId = envelopeId ?? throw new ArgumentNullException(nameof(envelopeId));
        StoreEpochId = storeEpochId ?? throw new ArgumentNullException(nameof(storeEpochId));
        ConfigurationRevision = configurationRevision ??
            throw new ArgumentNullException(nameof(configurationRevision));
        OperationAuthorityReference = operationAuthorityReference ??
            throw new ArgumentNullException(nameof(operationAuthorityReference));
        RequestPlanSha256 = requestPlanSha256 ??
            throw new ArgumentNullException(nameof(requestPlanSha256));
        RequestSha256 = requestSha256 ?? throw new ArgumentNullException(nameof(requestSha256));
        MaximumChargeBasisSha256 = maximumChargeBasisSha256 ??
            throw new ArgumentNullException(nameof(maximumChargeBasisSha256));
        CostScheduleSha256 = costScheduleSha256 ??
            throw new ArgumentNullException(nameof(costScheduleSha256));
        BindingSha256 = bindingSha256 ?? throw new ArgumentNullException(nameof(bindingSha256));
        AdmittedRuntimeSessionId = admittedRuntimeSessionId ??
            throw new ArgumentNullException(nameof(admittedRuntimeSessionId));
        AdmissionLedgerRevision = admissionLedgerRevision ??
            throw new ArgumentNullException(nameof(admissionLedgerRevision));

        if (!Enum.IsDefined(operationClass))
        {
            throw new ArgumentOutOfRangeException(nameof(operationClass));
        }

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(currentReservationRevision);
        RequireUtc(admittedAtUtc, nameof(admittedAtUtc));
        RequireOptionalUtc(dispatchStartedAtUtc, nameof(dispatchStartedAtUtc));
        RequireOptionalUtc(terminalAtUtc, nameof(terminalAtUtc));

        if (dispatchStartedAtUtc < admittedAtUtc ||
            terminalAtUtc < admittedAtUtc ||
            (dispatchStartedAtUtc is not null && terminalAtUtc < dispatchStartedAtUtc))
        {
            throw new ArgumentException(
                "Provider-budget reservation transition instants are out of order.");
        }

        var validShape = status switch
        {
            ProviderBudgetReservationStatus.Reserved =>
                dispatchStartedAtUtc is null && terminalAtUtc is null &&
                terminalLedgerRevision is null,
            ProviderBudgetReservationStatus.DispatchStarted =>
                dispatchStartedAtUtc is not null && terminalAtUtc is null &&
                terminalLedgerRevision is null,
            ProviderBudgetReservationStatus.ReleasedPreSend =>
                dispatchStartedAtUtc is null && terminalAtUtc is not null &&
                terminalLedgerRevision is not null,
            ProviderBudgetReservationStatus.Committed or
            ProviderBudgetReservationStatus.IndeterminateCommitted or
            ProviderBudgetReservationStatus.OverrunCommitted =>
                dispatchStartedAtUtc is not null && terminalAtUtc is not null &&
                terminalLedgerRevision is not null,
            _ => false,
        };

        if (!validShape)
        {
            throw new ArgumentException(
                "Provider-budget reservation state and transition evidence are inconsistent.",
                nameof(status));
        }

        OperationClass = operationClass;
        MaximumCharge = maximumCharge;
        CurrentReservationRevision = currentReservationRevision;
        Status = status;
        AdmittedAtUtc = admittedAtUtc;
        DispatchStartedAtUtc = dispatchStartedAtUtc;
        TerminalAtUtc = terminalAtUtc;
        TerminalLedgerRevision = terminalLedgerRevision;
    }

    public ProviderRequestId ProviderRequestId { get; }

    public ProviderBudgetEnvelopeId EnvelopeId { get; }

    public ProviderBudgetStoreEpochId StoreEpochId { get; }

    public ProviderBudgetConfigurationRevision ConfigurationRevision { get; }

    public ProviderBudgetOperationClass OperationClass { get; }

    public ProviderBudgetAuthorityReference OperationAuthorityReference { get; }

    public ProviderBudgetSha256 RequestPlanSha256 { get; }

    public ProviderBudgetSha256 RequestSha256 { get; }

    public ProviderBudgetSha256 MaximumChargeBasisSha256 { get; }

    public ProviderBudgetSha256 CostScheduleSha256 { get; }

    public ProviderBudgetSha256 BindingSha256 { get; }

    public ProviderBudgetUnits MaximumCharge { get; }

    public ProviderRuntimeSessionId AdmittedRuntimeSessionId { get; }

    public ProviderBudgetLedgerRevision AdmissionLedgerRevision { get; }

    public long CurrentReservationRevision { get; }

    public ProviderBudgetReservationStatus Status { get; }

    public DateTimeOffset AdmittedAtUtc { get; }

    public DateTimeOffset? DispatchStartedAtUtc { get; }

    public DateTimeOffset? TerminalAtUtc { get; }

    public ProviderBudgetLedgerRevision? TerminalLedgerRevision { get; }

    internal ProviderBudgetReservation WithTransition(
        long reservationRevision,
        ProviderBudgetReservationStatus status,
        DateTimeOffset? dispatchStartedAtUtc,
        DateTimeOffset? terminalAtUtc,
        ProviderBudgetLedgerRevision? terminalLedgerRevision) =>
        new(
            ProviderRequestId,
            EnvelopeId,
            StoreEpochId,
            ConfigurationRevision,
            OperationClass,
            OperationAuthorityReference,
            RequestPlanSha256,
            RequestSha256,
            MaximumChargeBasisSha256,
            CostScheduleSha256,
            BindingSha256,
            MaximumCharge,
            AdmittedRuntimeSessionId,
            AdmissionLedgerRevision,
            reservationRevision,
            status,
            AdmittedAtUtc,
            dispatchStartedAtUtc,
            terminalAtUtc,
            terminalLedgerRevision);

    private static void RequireUtc(DateTimeOffset value, string parameterName)
    {
        if (value.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Provider-budget reservation instants must use an explicit UTC offset.",
                parameterName);
        }
    }

    private static void RequireOptionalUtc(DateTimeOffset? value, string parameterName)
    {
        if (value is not null)
        {
            RequireUtc(value.Value, parameterName);
        }
    }
}

public sealed record ProviderBudgetAdmissionResult
{
    public ProviderBudgetAdmissionResult(
        ProviderBudgetAdmissionOutcome outcome,
        ProviderBudgetState state,
        ProviderBudgetLedgerRevision? currentLedgerRevision,
        ProviderBudgetReservation? reservation,
        ProviderBudgetAdmissionRejection? rejection)
    {
        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(nameof(outcome));
        }

        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state));
        }

        if (rejection is not null && !Enum.IsDefined(rejection.Value))
        {
            throw new ArgumentOutOfRangeException(nameof(rejection));
        }

        var validShape = outcome switch
        {
            ProviderBudgetAdmissionOutcome.Admitted or
            ProviderBudgetAdmissionOutcome.Replay =>
                currentLedgerRevision is not null && reservation is not null &&
                rejection is null,
            ProviderBudgetAdmissionOutcome.Rejected =>
                reservation is null && rejection is not null &&
                (rejection == ProviderBudgetAdmissionRejection.EnvelopeUnavailable
                    ? state == ProviderBudgetState.Disarmed && currentLedgerRevision is null
                    : currentLedgerRevision is not null),
            ProviderBudgetAdmissionOutcome.Conflict =>
                state == ProviderBudgetState.Tripped && currentLedgerRevision is not null &&
                reservation is null && rejection is null,
            _ => false,
        };

        if (!validShape)
        {
            throw new ArgumentException(
                "Provider-budget admission result fields do not match the explicit outcome.",
                nameof(outcome));
        }

        Outcome = outcome;
        State = state;
        CurrentLedgerRevision = currentLedgerRevision;
        Reservation = reservation;
        Rejection = rejection;
    }

    public ProviderBudgetAdmissionOutcome Outcome { get; }

    public ProviderBudgetState State { get; }

    public ProviderBudgetLedgerRevision? CurrentLedgerRevision { get; }

    public ProviderBudgetReservation? Reservation { get; }

    public ProviderBudgetAdmissionRejection? Rejection { get; }
}

public sealed record ProviderBudgetDispatchRequest
{
    public ProviderBudgetDispatchRequest(
        ProviderRequestId providerRequestId,
        ProviderBudgetLedgerRevision expectedLedgerRevision,
        long expectedReservationRevision,
        DateTimeOffset occurredAtUtc)
    {
        ProviderRequestId = providerRequestId ??
            throw new ArgumentNullException(nameof(providerRequestId));
        ExpectedLedgerRevision = expectedLedgerRevision ??
            throw new ArgumentNullException(nameof(expectedLedgerRevision));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(expectedReservationRevision);
        RequireUtc(occurredAtUtc, nameof(occurredAtUtc));
        ExpectedReservationRevision = expectedReservationRevision;
        OccurredAtUtc = occurredAtUtc;
    }

    public ProviderRequestId ProviderRequestId { get; }

    public ProviderBudgetLedgerRevision ExpectedLedgerRevision { get; }

    public long ExpectedReservationRevision { get; }

    public DateTimeOffset OccurredAtUtc { get; }

    private static void RequireUtc(DateTimeOffset value, string parameterName)
    {
        if (value.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Provider-budget transition instants must use an explicit UTC offset.",
                parameterName);
        }
    }
}

public sealed record ProviderBudgetCommitRequest
{
    public ProviderBudgetCommitRequest(
        ProviderRequestId providerRequestId,
        ProviderBudgetLedgerRevision expectedLedgerRevision,
        long expectedReservationRevision,
        ProviderBudgetCommitmentKind commitmentKind,
        ProviderBudgetUnits committedUnits,
        ProviderBudgetSha256 usageEvidenceSha256,
        ProviderBudgetOutcomeCode providerOutcomeCode,
        TimeSpan? providerDuration,
        DateTimeOffset occurredAtUtc)
    {
        ProviderRequestId = providerRequestId ??
            throw new ArgumentNullException(nameof(providerRequestId));
        ExpectedLedgerRevision = expectedLedgerRevision ??
            throw new ArgumentNullException(nameof(expectedLedgerRevision));
        UsageEvidenceSha256 = usageEvidenceSha256 ??
            throw new ArgumentNullException(nameof(usageEvidenceSha256));
        ProviderOutcomeCode = providerOutcomeCode ??
            throw new ArgumentNullException(nameof(providerOutcomeCode));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(expectedReservationRevision);

        if (!Enum.IsDefined(commitmentKind))
        {
            throw new ArgumentOutOfRangeException(nameof(commitmentKind));
        }

        if (providerDuration is { } duration &&
            (duration < TimeSpan.Zero || duration > TimeSpan.FromDays(1)))
        {
            throw new ArgumentOutOfRangeException(
                nameof(providerDuration),
                "Provider duration must be between zero and one day.");
        }

        RequireUtc(occurredAtUtc, nameof(occurredAtUtc));
        ExpectedReservationRevision = expectedReservationRevision;
        CommitmentKind = commitmentKind;
        CommittedUnits = committedUnits;
        ProviderDuration = providerDuration;
        OccurredAtUtc = occurredAtUtc;
    }

    public ProviderRequestId ProviderRequestId { get; }

    public ProviderBudgetLedgerRevision ExpectedLedgerRevision { get; }

    public long ExpectedReservationRevision { get; }

    public ProviderBudgetCommitmentKind CommitmentKind { get; }

    public ProviderBudgetUnits CommittedUnits { get; }

    public ProviderBudgetSha256 UsageEvidenceSha256 { get; }

    public ProviderBudgetOutcomeCode ProviderOutcomeCode { get; }

    public TimeSpan? ProviderDuration { get; }

    public DateTimeOffset OccurredAtUtc { get; }

    private static void RequireUtc(DateTimeOffset value, string parameterName)
    {
        if (value.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Provider-budget transition instants must use an explicit UTC offset.",
                parameterName);
        }
    }
}

public sealed record ProviderBudgetReleaseRequest
{
    public ProviderBudgetReleaseRequest(
        ProviderRequestId providerRequestId,
        ProviderBudgetLedgerRevision expectedLedgerRevision,
        long expectedReservationRevision,
        ProviderBudgetReleaseProofKind proofKind,
        ProviderBudgetSha256 proofSha256,
        ProviderBudgetAuthorityReference authorityReference,
        DateTimeOffset occurredAtUtc)
    {
        ProviderRequestId = providerRequestId ??
            throw new ArgumentNullException(nameof(providerRequestId));
        ExpectedLedgerRevision = expectedLedgerRevision ??
            throw new ArgumentNullException(nameof(expectedLedgerRevision));
        ProofSha256 = proofSha256 ?? throw new ArgumentNullException(nameof(proofSha256));
        AuthorityReference = authorityReference ??
            throw new ArgumentNullException(nameof(authorityReference));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(expectedReservationRevision);

        if (!Enum.IsDefined(proofKind))
        {
            throw new ArgumentOutOfRangeException(nameof(proofKind));
        }

        if (occurredAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Provider-budget transition instants must use an explicit UTC offset.",
                nameof(occurredAtUtc));
        }

        ExpectedReservationRevision = expectedReservationRevision;
        ProofKind = proofKind;
        OccurredAtUtc = occurredAtUtc;
    }

    public ProviderRequestId ProviderRequestId { get; }

    public ProviderBudgetLedgerRevision ExpectedLedgerRevision { get; }

    public long ExpectedReservationRevision { get; }

    public ProviderBudgetReleaseProofKind ProofKind { get; }

    public ProviderBudgetSha256 ProofSha256 { get; }

    public ProviderBudgetAuthorityReference AuthorityReference { get; }

    public DateTimeOffset OccurredAtUtc { get; }
}

public sealed record ProviderBudgetTransitionResult
{
    public ProviderBudgetTransitionResult(
        ProviderBudgetTransitionOutcome outcome,
        ProviderBudgetState state,
        ProviderBudgetLedgerRevision? currentLedgerRevision,
        ProviderBudgetReservation? reservation,
        ProviderBudgetTransitionRejection? rejection)
    {
        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(nameof(outcome));
        }

        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state));
        }

        if (rejection is not null && !Enum.IsDefined(rejection.Value))
        {
            throw new ArgumentOutOfRangeException(nameof(rejection));
        }

        var validShape = outcome switch
        {
            ProviderBudgetTransitionOutcome.Applied or
            ProviderBudgetTransitionOutcome.Replay =>
                currentLedgerRevision is not null && reservation is not null &&
                rejection is null,
            ProviderBudgetTransitionOutcome.Rejected => rejection is not null,
            ProviderBudgetTransitionOutcome.Conflict =>
                state == ProviderBudgetState.Tripped && currentLedgerRevision is not null &&
                rejection is null,
            _ => false,
        };

        if (!validShape)
        {
            throw new ArgumentException(
                "Provider-budget transition result fields do not match the explicit outcome.",
                nameof(outcome));
        }

        Outcome = outcome;
        State = state;
        CurrentLedgerRevision = currentLedgerRevision;
        Reservation = reservation;
        Rejection = rejection;
    }

    public ProviderBudgetTransitionOutcome Outcome { get; }

    public ProviderBudgetState State { get; }

    public ProviderBudgetLedgerRevision? CurrentLedgerRevision { get; }

    public ProviderBudgetReservation? Reservation { get; }

    public ProviderBudgetTransitionRejection? Rejection { get; }
}

public interface IProviderBudgetLedger
{
    Task<ProviderBudgetEnvelopeV1?> ReadEnvelopeAsync(
        ProviderBudgetEnvelopeId envelopeId,
        CancellationToken cancellationToken = default);

    Task<ProviderBudgetReservation?> ReadReservationAsync(
        ProviderRequestId providerRequestId,
        CancellationToken cancellationToken = default);

    Task<ProviderBudgetAdmissionResult> AdmitAsync(
        ProviderBudgetAdmissionRequest request,
        CancellationToken cancellationToken = default);

    Task<ProviderBudgetTransitionResult> MarkDispatchStartedAsync(
        ProviderBudgetDispatchRequest request,
        CancellationToken cancellationToken = default);

    Task<ProviderBudgetTransitionResult> CommitAsync(
        ProviderBudgetCommitRequest request,
        CancellationToken cancellationToken = default);

    Task<ProviderBudgetTransitionResult> ReleasePreSendAsync(
        ProviderBudgetReleaseRequest request,
        CancellationToken cancellationToken = default);
}
