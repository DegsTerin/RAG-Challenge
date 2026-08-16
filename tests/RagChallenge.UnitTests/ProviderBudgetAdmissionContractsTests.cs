// Purpose: Verifies fail-closed provider-budget contracts, zero-budget admission replay/conflict and deterministic in-memory transitions without persistence, rearming, provider access or egress.
using System.Security.Cryptography;
using System.Text;

using RagChallenge.Application.ProviderBudget;

namespace RagChallenge.UnitTests;

public sealed class ProviderBudgetAdmissionContractsTests
{
    [Fact]
    public async Task DefaultLedgerIsDisarmedZeroAndRejectsWithoutCreatingAReservation()
    {
        var ledger = new FakeDeterministicProviderBudgetLedger();
        var request = CreateAdmissionRequest();

        var result = await ledger.AdmitAsync(request);

        Assert.Equal(ProviderBudgetAdmissionOutcome.Rejected, result.Outcome);
        Assert.Equal(ProviderBudgetState.Disarmed, result.State);
        Assert.Equal(
            ProviderBudgetAdmissionRejection.EnvelopeUnavailable,
            result.Rejection);
        Assert.Null(result.CurrentLedgerRevision);
        Assert.Null(result.Reservation);
        Assert.Null(await ledger.ReadEnvelopeAsync(EnvelopeId));
        Assert.Null(await ledger.ReadReservationAsync(RequestId));
    }

    [Fact]
    public async Task ExplicitArmedZeroEnvelopeAdmitsAndReadsBackExactReservation()
    {
        var ledger = CreateArmedLedger();
        var request = CreateAdmissionRequest();

        var result = await ledger.AdmitAsync(request);
        var readback = await ledger.ReadReservationAsync(RequestId);
        var envelope = await ledger.ReadEnvelopeAsync(EnvelopeId);

        Assert.Equal(ProviderBudgetAdmissionOutcome.Admitted, result.Outcome);
        Assert.Equal(ProviderBudgetState.Armed, result.State);
        Assert.Equal(2, result.CurrentLedgerRevision?.Value);
        Assert.Same(result.Reservation, readback);
        Assert.Equal(ProviderBudgetReservationStatus.Reserved, readback?.Status);
        Assert.Equal(0, readback?.MaximumCharge.Value);
        Assert.Equal(1, readback?.CurrentReservationRevision);
        Assert.Equal(0, envelope?.AggregateReserved.Value);
        Assert.All(envelope!.OperationBalances, balance =>
            Assert.Equal(0, balance.Reserved.Value));
    }

    [Fact]
    public async Task IdenticalProviderRequestIsReplayWithoutRevisionAdvance()
    {
        var ledger = CreateArmedLedger();
        var request = CreateAdmissionRequest();
        var admitted = await ledger.AdmitAsync(request);

        var replay = await ledger.AdmitAsync(request);

        Assert.Equal(ProviderBudgetAdmissionOutcome.Replay, replay.Outcome);
        Assert.Equal(admitted.CurrentLedgerRevision, replay.CurrentLedgerRevision);
        Assert.Equal(admitted.Reservation, replay.Reservation);
        Assert.Null(replay.Rejection);
    }

    [Fact]
    public async Task DivergentBindingForSameProviderRequestTripsWithoutReplacingReservation()
    {
        var ledger = CreateArmedLedger();
        var original = CreateAdmissionRequest();
        await ledger.AdmitAsync(original);
        var conflicting = CreateAdmissionRequest(requestSha256: Hash("different-request"));

        var conflict = await ledger.AdmitAsync(conflicting);
        var recorded = await ledger.ReadReservationAsync(RequestId);

        Assert.Equal(ProviderBudgetAdmissionOutcome.Conflict, conflict.Outcome);
        Assert.Equal(ProviderBudgetState.Tripped, conflict.State);
        Assert.Equal(3, conflict.CurrentLedgerRevision?.Value);
        Assert.Null(conflict.Reservation);
        Assert.Equal(original.RequestSha256, recorded?.RequestSha256);
    }

    [Fact]
    public async Task StaleLedgerRevisionRejectsWithoutMutation()
    {
        var ledger = CreateArmedLedger();
        var request = CreateAdmissionRequest(
            expectedLedgerRevision: new ProviderBudgetLedgerRevision(2));

        var result = await ledger.AdmitAsync(request);
        var envelope = await ledger.ReadEnvelopeAsync(EnvelopeId);

        Assert.Equal(ProviderBudgetAdmissionOutcome.Rejected, result.Outcome);
        Assert.Equal(
            ProviderBudgetAdmissionRejection.LedgerRevisionMismatch,
            result.Rejection);
        Assert.Equal(1, envelope?.LedgerRevision.Value);
        Assert.Null(await ledger.ReadReservationAsync(RequestId));
    }

    [Fact]
    public async Task DispatchAndObservedCommitAreDeterministicZeroValueTransitions()
    {
        var ledger = CreateArmedLedger();
        var admitted = await ledger.AdmitAsync(CreateAdmissionRequest());
        var dispatchRequest = new ProviderBudgetDispatchRequest(
            RequestId,
            admitted.CurrentLedgerRevision!,
            admitted.Reservation!.CurrentReservationRevision,
            At(2));
        var dispatched = await ledger.MarkDispatchStartedAsync(dispatchRequest);
        var commitRequest = new ProviderBudgetCommitRequest(
            RequestId,
            dispatched.CurrentLedgerRevision!,
            dispatched.Reservation!.CurrentReservationRevision,
            ProviderBudgetCommitmentKind.Observed,
            Zero,
            Hash("usage-evidence"),
            new ProviderBudgetOutcomeCode("provider-success"),
            TimeSpan.FromMilliseconds(7),
            At(3));

        var committed = await ledger.CommitAsync(commitRequest);
        var replay = await ledger.CommitAsync(commitRequest);

        Assert.Equal(ProviderBudgetTransitionOutcome.Applied, dispatched.Outcome);
        Assert.Equal(ProviderBudgetReservationStatus.DispatchStarted, dispatched.Reservation.Status);
        Assert.Equal(ProviderBudgetTransitionOutcome.Applied, committed.Outcome);
        Assert.Equal(ProviderBudgetState.Armed, committed.State);
        Assert.Equal(ProviderBudgetReservationStatus.Committed, committed.Reservation?.Status);
        Assert.Equal(4, committed.CurrentLedgerRevision?.Value);
        Assert.Equal(ProviderBudgetTransitionOutcome.Replay, replay.Outcome);
        Assert.Equal(committed.CurrentLedgerRevision, replay.CurrentLedgerRevision);
    }

    [Fact]
    public async Task IndeterminateCommitConservativelyRequiresReconciliation()
    {
        var ledger = CreateArmedLedger();
        var admitted = await ledger.AdmitAsync(CreateAdmissionRequest());
        var dispatched = await ledger.MarkDispatchStartedAsync(new ProviderBudgetDispatchRequest(
            RequestId,
            admitted.CurrentLedgerRevision!,
            admitted.Reservation!.CurrentReservationRevision,
            At(2)));

        var result = await ledger.CommitAsync(new ProviderBudgetCommitRequest(
            RequestId,
            dispatched.CurrentLedgerRevision!,
            dispatched.Reservation!.CurrentReservationRevision,
            ProviderBudgetCommitmentKind.IndeterminateMaximum,
            Zero,
            Hash("indeterminate-evidence"),
            new ProviderBudgetOutcomeCode("provider-outcome-uncertain"),
            providerDuration: null,
            At(3)));

        Assert.Equal(ProviderBudgetTransitionOutcome.Applied, result.Outcome);
        Assert.Equal(ProviderBudgetState.ReconciliationRequired, result.State);
        Assert.Equal(
            ProviderBudgetReservationStatus.IndeterminateCommitted,
            result.Reservation?.Status);
    }

    [Fact]
    public async Task ObservedChargeAboveMaximumCommitsMaximumAndTrips()
    {
        var ledger = CreateArmedLedger();
        var admitted = await ledger.AdmitAsync(CreateAdmissionRequest());
        var dispatched = await ledger.MarkDispatchStartedAsync(new ProviderBudgetDispatchRequest(
            RequestId,
            admitted.CurrentLedgerRevision!,
            admitted.Reservation!.CurrentReservationRevision,
            At(2)));

        var result = await ledger.CommitAsync(new ProviderBudgetCommitRequest(
            RequestId,
            dispatched.CurrentLedgerRevision!,
            dispatched.Reservation!.CurrentReservationRevision,
            ProviderBudgetCommitmentKind.Observed,
            new ProviderBudgetUnits(1),
            Hash("overrun-evidence"),
            new ProviderBudgetOutcomeCode("provider-overrun"),
            providerDuration: null,
            At(3)));
        var envelope = await ledger.ReadEnvelopeAsync(EnvelopeId);

        Assert.Equal(ProviderBudgetTransitionOutcome.Applied, result.Outcome);
        Assert.Equal(ProviderBudgetState.Tripped, result.State);
        Assert.Equal(ProviderBudgetReservationStatus.OverrunCommitted, result.Reservation?.Status);
        Assert.Equal(0, envelope?.AggregateCommitted.Value);
        Assert.Equal(0, envelope?.AggregateReserved.Value);
    }

    [Fact]
    public async Task ProvenPreSendFailureReleasesWithoutDispatch()
    {
        var ledger = CreateArmedLedger();
        var admitted = await ledger.AdmitAsync(CreateAdmissionRequest());
        var release = new ProviderBudgetReleaseRequest(
            RequestId,
            admitted.CurrentLedgerRevision!,
            admitted.Reservation!.CurrentReservationRevision,
            ProviderBudgetReleaseProofKind.BeforeCredentialLookup,
            Hash("pre-send-proof"),
            new ProviderBudgetAuthorityReference("authority-release"),
            At(2));

        var result = await ledger.ReleasePreSendAsync(release);
        var replay = await ledger.ReleasePreSendAsync(release);

        Assert.Equal(ProviderBudgetTransitionOutcome.Applied, result.Outcome);
        Assert.Equal(ProviderBudgetReservationStatus.ReleasedPreSend, result.Reservation?.Status);
        Assert.Null(result.Reservation?.DispatchStartedAtUtc);
        Assert.Equal(ProviderBudgetTransitionOutcome.Replay, replay.Outcome);
    }

    [Fact]
    public void FakeRejectsAnyNonZeroBudgetConfiguration()
    {
        var balances = ZeroBalances().ToArray();
        balances[0] = new ProviderBudgetOperationBalance(
            ProviderBudgetOperationClass.AdministrativeIndexEmbedding,
            new ProviderBudgetUnits(1),
            Zero,
            Zero,
            Zero);
        var envelope = CreateEnvelope(
            aggregateLimit: new ProviderBudgetUnits(1),
            balances);

        var exception = Assert.Throws<ArgumentException>(
            () => new FakeDeterministicProviderBudgetLedger(envelope));

        Assert.Equal("initialEnvelope", exception.ParamName);
    }

    [Fact]
    public void EnvelopeRequiresCompleteClosedOperationSetAndUppercaseCurrency()
    {
        Assert.Throws<ArgumentException>(() => CreateEnvelope(
            balances: ZeroBalances().Take(2)));
        Assert.Throws<ArgumentException>(() => new ProviderBudgetCurrencyCode("usd"));
    }

    [Fact]
    public void ExplicitResultShapeCannotClaimAdmissionWithoutReservationReadback()
    {
        Assert.Throws<ArgumentException>(() => new ProviderBudgetAdmissionResult(
            ProviderBudgetAdmissionOutcome.Admitted,
            ProviderBudgetState.Armed,
            new ProviderBudgetLedgerRevision(2),
            reservation: null,
            rejection: null));
    }

    private static FakeDeterministicProviderBudgetLedger CreateArmedLedger() =>
        new(CreateEnvelope());

    private static ProviderBudgetEnvelopeV1 CreateEnvelope(
        ProviderBudgetUnits? aggregateLimit = null,
        IEnumerable<ProviderBudgetOperationBalance>? balances = null) =>
        new(
            EnvelopeId,
            StoreEpochId,
            Scope,
            ConfigurationRevision,
            new ProviderBudgetLedgerRevision(1),
            new ProviderBudgetRearmRevision(0),
            ProviderBudgetState.Armed,
            RuntimeSessionId,
            CostScheduleId,
            CostScheduleSha256,
            aggregateLimit ?? Zero,
            Zero,
            Zero,
            Zero,
            balances ?? ZeroBalances(),
            At(0),
            At(12),
            isClosed: false,
            Hash("initial-ledger"));

    private static IEnumerable<ProviderBudgetOperationBalance> ZeroBalances() =>
        Enum.GetValues<ProviderBudgetOperationClass>().Select(operation =>
            new ProviderBudgetOperationBalance(operation, Zero, Zero, Zero, Zero));

    private static ProviderBudgetAdmissionRequest CreateAdmissionRequest(
        ProviderBudgetLedgerRevision? expectedLedgerRevision = null,
        ProviderBudgetSha256? requestSha256 = null) =>
        new(
            RequestId,
            EnvelopeId,
            StoreEpochId,
            ConfigurationRevision,
            expectedLedgerRevision ?? new ProviderBudgetLedgerRevision(1),
            RuntimeSessionId,
            Scope,
            CostScheduleId,
            CostScheduleSha256,
            ProviderBudgetOperationClass.QueryEmbedding,
            new ProviderBudgetAuthorityReference("authority-query-embedding"),
            Hash("request-plan"),
            requestSha256 ?? Hash("request"),
            Hash("maximum-charge-basis"),
            Hash("binding"),
            Zero,
            At(1));

    private static ProviderBudgetSha256 Hash(string value) =>
        new(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant());

    private static DateTimeOffset At(int hour) =>
        new(2026, 8, 16, hour, 0, 0, TimeSpan.Zero);

    private static readonly ProviderBudgetEnvelopeId EnvelopeId = new("envelope-1");
    private static readonly ProviderBudgetStoreEpochId StoreEpochId = new("epoch-1");
    private static readonly ProviderRequestId RequestId = new("provider-request-1");
    private static readonly ProviderRuntimeSessionId RuntimeSessionId = new("runtime-session-1");
    private static readonly ProviderBudgetConfigurationRevision ConfigurationRevision = new(1);
    private static readonly ProviderBudgetCostScheduleId CostScheduleId = new("schedule-zero-v1");
    private static readonly ProviderBudgetSha256 CostScheduleSha256 = Hash("schedule-zero-v1");
    private static readonly ProviderBudgetUnits Zero = new(0);
    private static readonly ProviderBudgetScope Scope = new(
        new ProviderBudgetEnvironmentId("local-test"),
        new ProviderBudgetProviderId("fake-provider"),
        new ProviderBudgetBillingScopeReference("billing-scope-none"),
        new ProviderBudgetModelId("fake-model"),
        new ProviderBudgetCurrencyCode("USD"),
        new ProviderBudgetAccountingUnitId("microunit"));
}
