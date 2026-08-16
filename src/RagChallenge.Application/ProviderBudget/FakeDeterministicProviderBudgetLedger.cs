// Purpose: Provides a deterministic zero-budget in-memory test double for the Application ledger port; it is neither durable admission evidence nor an implementation of persistence, rearming or provider access.
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace RagChallenge.Application.ProviderBudget;

public sealed class FakeDeterministicProviderBudgetLedger : IProviderBudgetLedger
{
    private readonly Lock sync = new();
    private readonly Dictionary<ProviderRequestId, ProviderBudgetAdmissionRequest>
        admissionRequests = [];
    private readonly Dictionary<ProviderRequestId, ProviderBudgetReservation> reservations = [];
    private readonly Dictionary<ProviderRequestId, ProviderBudgetDispatchRequest> dispatches = [];
    private readonly Dictionary<ProviderRequestId, ProviderBudgetCommitRequest> commitments = [];
    private readonly Dictionary<ProviderRequestId, ProviderBudgetReleaseRequest> releases = [];
    private ProviderBudgetEnvelopeV1? envelope;

    public FakeDeterministicProviderBudgetLedger(ProviderBudgetEnvelopeV1? initialEnvelope = null)
    {
        if (initialEnvelope is not null && !IsZeroBudget(initialEnvelope))
        {
            throw new ArgumentException(
                "The deterministic fake accepts only zero limits and zero balances.",
                nameof(initialEnvelope));
        }

        envelope = initialEnvelope;
    }

    public Task<ProviderBudgetEnvelopeV1?> ReadEnvelopeAsync(
        ProviderBudgetEnvelopeId envelopeId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelopeId);
        cancellationToken.ThrowIfCancellationRequested();

        lock (sync)
        {
            return Task.FromResult(
                envelope?.EnvelopeId == envelopeId ? envelope : null);
        }
    }

    public Task<ProviderBudgetReservation?> ReadReservationAsync(
        ProviderRequestId providerRequestId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(providerRequestId);
        cancellationToken.ThrowIfCancellationRequested();

        lock (sync)
        {
            reservations.TryGetValue(providerRequestId, out var reservation);
            return Task.FromResult(reservation);
        }
    }

    public Task<ProviderBudgetAdmissionResult> AdmitAsync(
        ProviderBudgetAdmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        lock (sync)
        {
            if (admissionRequests.TryGetValue(request.ProviderRequestId, out var recordedRequest))
            {
                if (HasIdenticalReservationBinding(recordedRequest, request))
                {
                    return Task.FromResult(new ProviderBudgetAdmissionResult(
                        ProviderBudgetAdmissionOutcome.Replay,
                        envelope?.State ?? ProviderBudgetState.Disarmed,
                        envelope?.LedgerRevision,
                        reservations[request.ProviderRequestId],
                        rejection: null));
                }

                Trip("admission-conflict", request.ProviderRequestId);
                return Task.FromResult(new ProviderBudgetAdmissionResult(
                    ProviderBudgetAdmissionOutcome.Conflict,
                    ProviderBudgetState.Tripped,
                    envelope?.LedgerRevision,
                    reservation: null,
                    rejection: null));
            }

            var rejection = ValidateAdmission(request);

            if (rejection is not null)
            {
                return Task.FromResult(new ProviderBudgetAdmissionResult(
                    ProviderBudgetAdmissionOutcome.Rejected,
                    envelope?.State ?? ProviderBudgetState.Disarmed,
                    envelope?.LedgerRevision,
                    reservation: null,
                    rejection: rejection));
            }

            var current = envelope!;
            var nextRevision = NextLedgerRevision(current);
            var operationBalances = current.OperationBalances
                .Select(balance => balance.OperationClass == request.OperationClass
                    ? new ProviderBudgetOperationBalance(
                        balance.OperationClass,
                        balance.AllocationLimit,
                        balance.Committed,
                        new ProviderBudgetUnits(checked(
                            balance.Reserved.Value + request.MaximumCharge.Value)),
                        balance.Indeterminate)
                    : balance)
                .ToArray();
            var nextReserved = new ProviderBudgetUnits(checked(
                current.AggregateReserved.Value + request.MaximumCharge.Value));

            envelope = current.WithLedger(
                nextRevision,
                ProviderBudgetState.Armed,
                current.AggregateCommitted,
                nextReserved,
                current.AggregateIndeterminate,
                operationBalances,
                FakeDigest("admission", nextRevision, request.ProviderRequestId));

            var reservation = new ProviderBudgetReservation(
                request.ProviderRequestId,
                request.EnvelopeId,
                request.StoreEpochId,
                request.ExpectedConfigurationRevision,
                request.OperationClass,
                request.OperationAuthorityReference,
                request.RequestPlanSha256,
                request.RequestSha256,
                request.MaximumChargeBasisSha256,
                request.CostScheduleSha256,
                request.BindingSha256,
                request.MaximumCharge,
                request.RuntimeSessionId,
                nextRevision,
                currentReservationRevision: 1,
                ProviderBudgetReservationStatus.Reserved,
                request.RequestedAtUtc);

            admissionRequests.Add(request.ProviderRequestId, request);
            reservations.Add(request.ProviderRequestId, reservation);

            return Task.FromResult(new ProviderBudgetAdmissionResult(
                ProviderBudgetAdmissionOutcome.Admitted,
                envelope.State,
                envelope.LedgerRevision,
                reservation,
                rejection: null));
        }
    }

    public Task<ProviderBudgetTransitionResult> MarkDispatchStartedAsync(
        ProviderBudgetDispatchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateTransitionRequest(
            request.ProviderRequestId,
            request.ExpectedLedgerRevision,
            request.ExpectedReservationRevision,
            request.OccurredAtUtc);
        cancellationToken.ThrowIfCancellationRequested();

        lock (sync)
        {
            if (dispatches.TryGetValue(request.ProviderRequestId, out var recorded))
            {
                return Task.FromResult(recorded == request
                    ? Replay(reservations[request.ProviderRequestId])
                    : Conflict(request.ProviderRequestId));
            }

            var rejected = ValidateTransition(
                request.ProviderRequestId,
                request.ExpectedLedgerRevision,
                request.ExpectedReservationRevision,
                ProviderBudgetReservationStatus.Reserved,
                request.OccurredAtUtc);

            if (rejected is not null)
            {
                return Task.FromResult(rejected);
            }

            var currentReservation = reservations[request.ProviderRequestId];
            var nextRevision = NextLedgerRevision(envelope!);
            var reservation = currentReservation.WithTransition(
                checked(currentReservation.CurrentReservationRevision + 1),
                ProviderBudgetReservationStatus.DispatchStarted,
                request.OccurredAtUtc,
                terminalAtUtc: null,
                terminalLedgerRevision: null);

            envelope = envelope!.WithLedger(
                nextRevision,
                envelope.State,
                envelope.AggregateCommitted,
                envelope.AggregateReserved,
                envelope.AggregateIndeterminate,
                envelope.OperationBalances,
                FakeDigest("dispatch-started", nextRevision, request.ProviderRequestId));
            reservations[request.ProviderRequestId] = reservation;
            dispatches.Add(request.ProviderRequestId, request);
            return Task.FromResult(Applied(reservation));
        }
    }

    public Task<ProviderBudgetTransitionResult> CommitAsync(
        ProviderBudgetCommitRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateCommitRequest(request);
        cancellationToken.ThrowIfCancellationRequested();

        lock (sync)
        {
            if (commitments.TryGetValue(request.ProviderRequestId, out var recorded))
            {
                return Task.FromResult(recorded == request
                    ? Replay(reservations[request.ProviderRequestId])
                    : Conflict(request.ProviderRequestId));
            }

            var rejected = ValidateTransition(
                request.ProviderRequestId,
                request.ExpectedLedgerRevision,
                request.ExpectedReservationRevision,
                ProviderBudgetReservationStatus.DispatchStarted,
                request.OccurredAtUtc);

            if (rejected is not null)
            {
                return Task.FromResult(rejected);
            }

            var currentReservation = reservations[request.ProviderRequestId];

            if (request.CommitmentKind != ProviderBudgetCommitmentKind.Observed &&
                request.CommittedUnits != currentReservation.MaximumCharge)
            {
                throw new ArgumentException(
                    "An indeterminate or overrun commitment must conservatively commit the full admitted maximum.",
                    nameof(request));
            }

            var current = envelope!;
            var nextRevision = NextLedgerRevision(current);
            var effectiveKind =
                request.CommittedUnits.Value > currentReservation.MaximumCharge.Value
                    ? ProviderBudgetCommitmentKind.OverrunMaximum
                    : request.CommitmentKind;
            var committedUnits = effectiveKind == ProviderBudgetCommitmentKind.OverrunMaximum
                ? currentReservation.MaximumCharge
                : request.CommittedUnits;
            var indeterminateIncrement =
                effectiveKind == ProviderBudgetCommitmentKind.IndeterminateMaximum
                    ? committedUnits.Value
                    : 0;
            var nextState = effectiveKind switch
            {
                ProviderBudgetCommitmentKind.Observed => ProviderBudgetState.Armed,
                ProviderBudgetCommitmentKind.IndeterminateMaximum =>
                    ProviderBudgetState.ReconciliationRequired,
                ProviderBudgetCommitmentKind.OverrunMaximum => ProviderBudgetState.Tripped,
                _ => throw new ArgumentOutOfRangeException(nameof(request)),
            };
            var nextReservationStatus = effectiveKind switch
            {
                ProviderBudgetCommitmentKind.Observed => ProviderBudgetReservationStatus.Committed,
                ProviderBudgetCommitmentKind.IndeterminateMaximum =>
                    ProviderBudgetReservationStatus.IndeterminateCommitted,
                ProviderBudgetCommitmentKind.OverrunMaximum =>
                    ProviderBudgetReservationStatus.OverrunCommitted,
                _ => throw new ArgumentOutOfRangeException(nameof(request)),
            };
            var operationBalances = current.OperationBalances
                .Select(balance => balance.OperationClass == currentReservation.OperationClass
                    ? new ProviderBudgetOperationBalance(
                        balance.OperationClass,
                        balance.AllocationLimit,
                        new ProviderBudgetUnits(checked(
                            balance.Committed.Value + committedUnits.Value)),
                        new ProviderBudgetUnits(
                            balance.Reserved.Value - currentReservation.MaximumCharge.Value),
                        new ProviderBudgetUnits(checked(
                            balance.Indeterminate.Value + indeterminateIncrement)))
                    : balance)
                .ToArray();

            envelope = current.WithLedger(
                nextRevision,
                nextState,
                new ProviderBudgetUnits(checked(
                    current.AggregateCommitted.Value + committedUnits.Value)),
                new ProviderBudgetUnits(
                    current.AggregateReserved.Value - currentReservation.MaximumCharge.Value),
                new ProviderBudgetUnits(checked(
                    current.AggregateIndeterminate.Value + indeterminateIncrement)),
                operationBalances,
                FakeDigest("commit", nextRevision, request.ProviderRequestId));

            var reservation = currentReservation.WithTransition(
                checked(currentReservation.CurrentReservationRevision + 1),
                nextReservationStatus,
                currentReservation.DispatchStartedAtUtc,
                request.OccurredAtUtc,
                nextRevision);
            reservations[request.ProviderRequestId] = reservation;
            commitments.Add(request.ProviderRequestId, request);
            return Task.FromResult(Applied(reservation));
        }
    }

    public Task<ProviderBudgetTransitionResult> ReleasePreSendAsync(
        ProviderBudgetReleaseRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateReleaseRequest(request);
        cancellationToken.ThrowIfCancellationRequested();

        lock (sync)
        {
            if (releases.TryGetValue(request.ProviderRequestId, out var recorded))
            {
                return Task.FromResult(recorded == request
                    ? Replay(reservations[request.ProviderRequestId])
                    : Conflict(request.ProviderRequestId));
            }

            var rejected = ValidateTransition(
                request.ProviderRequestId,
                request.ExpectedLedgerRevision,
                request.ExpectedReservationRevision,
                ProviderBudgetReservationStatus.Reserved,
                request.OccurredAtUtc);

            if (rejected is not null)
            {
                return Task.FromResult(rejected);
            }

            var current = envelope!;
            var currentReservation = reservations[request.ProviderRequestId];
            var nextRevision = NextLedgerRevision(current);
            var operationBalances = current.OperationBalances
                .Select(balance => balance.OperationClass == currentReservation.OperationClass
                    ? new ProviderBudgetOperationBalance(
                        balance.OperationClass,
                        balance.AllocationLimit,
                        balance.Committed,
                        new ProviderBudgetUnits(
                            balance.Reserved.Value - currentReservation.MaximumCharge.Value),
                        balance.Indeterminate)
                    : balance)
                .ToArray();

            envelope = current.WithLedger(
                nextRevision,
                current.State,
                current.AggregateCommitted,
                new ProviderBudgetUnits(
                    current.AggregateReserved.Value - currentReservation.MaximumCharge.Value),
                current.AggregateIndeterminate,
                operationBalances,
                FakeDigest("pre-send-release", nextRevision, request.ProviderRequestId));

            var reservation = currentReservation.WithTransition(
                checked(currentReservation.CurrentReservationRevision + 1),
                ProviderBudgetReservationStatus.ReleasedPreSend,
                dispatchStartedAtUtc: null,
                terminalAtUtc: request.OccurredAtUtc,
                terminalLedgerRevision: nextRevision);
            reservations[request.ProviderRequestId] = reservation;
            releases.Add(request.ProviderRequestId, request);
            return Task.FromResult(Applied(reservation));
        }
    }

    private ProviderBudgetAdmissionRejection? ValidateAdmission(
        ProviderBudgetAdmissionRequest request)
    {
        if (envelope is null)
        {
            return ProviderBudgetAdmissionRejection.EnvelopeUnavailable;
        }

        if (envelope.IsClosed)
        {
            return ProviderBudgetAdmissionRejection.Closed;
        }

        if (request.RequestedAtUtc < envelope.EffectiveAtUtc)
        {
            return ProviderBudgetAdmissionRejection.Disarmed;
        }

        if (request.RequestedAtUtc >= envelope.ExpiresAtUtc)
        {
            AdvanceState(ProviderBudgetState.Expired, "authority-expired");
            return ProviderBudgetAdmissionRejection.Expired;
        }

        ProviderBudgetAdmissionRejection? stateRejection = envelope.State switch
        {
            ProviderBudgetState.Armed => null,
            ProviderBudgetState.Disarmed => ProviderBudgetAdmissionRejection.Disarmed,
            ProviderBudgetState.Tripped => ProviderBudgetAdmissionRejection.Tripped,
            ProviderBudgetState.Exhausted => ProviderBudgetAdmissionRejection.Exhausted,
            ProviderBudgetState.ReconciliationRequired =>
                ProviderBudgetAdmissionRejection.ReconciliationRequired,
            ProviderBudgetState.Expired => ProviderBudgetAdmissionRejection.Expired,
            _ => ProviderBudgetAdmissionRejection.Disarmed,
        };

        if (stateRejection is not null)
        {
            return stateRejection;
        }

        if (request.EnvelopeId != envelope.EnvelopeId || request.Scope != envelope.Scope)
        {
            return ProviderBudgetAdmissionRejection.ScopeMismatch;
        }

        if (request.StoreEpochId != envelope.StoreEpochId)
        {
            return ProviderBudgetAdmissionRejection.StoreEpochMismatch;
        }

        if (request.ExpectedConfigurationRevision != envelope.ConfigurationRevision)
        {
            return ProviderBudgetAdmissionRejection.ConfigurationRevisionMismatch;
        }

        if (request.ExpectedLedgerRevision != envelope.LedgerRevision)
        {
            return ProviderBudgetAdmissionRejection.LedgerRevisionMismatch;
        }

        if (request.RuntimeSessionId != envelope.RuntimeSessionId)
        {
            return ProviderBudgetAdmissionRejection.RuntimeSessionMismatch;
        }

        if (request.CostScheduleId != envelope.CostScheduleId ||
            request.CostScheduleSha256 != envelope.CostScheduleSha256)
        {
            return ProviderBudgetAdmissionRejection.CostScheduleMismatch;
        }

        if (request.MaximumCharge.Value >
            envelope.AggregateLimit.Value - envelope.AggregateCommitted.Value -
            envelope.AggregateReserved.Value)
        {
            AdvanceState(ProviderBudgetState.Exhausted, "aggregate-exhausted");
            return ProviderBudgetAdmissionRejection.AggregateLimitExceeded;
        }

        var operation = envelope.OperationBalances.Single(
            balance => balance.OperationClass == request.OperationClass);

        if (request.MaximumCharge.Value >
            operation.AllocationLimit.Value - operation.Committed.Value -
            operation.Reserved.Value)
        {
            AdvanceState(ProviderBudgetState.Exhausted, "operation-exhausted");
            return ProviderBudgetAdmissionRejection.OperationLimitExceeded;
        }

        return null;
    }

    private ProviderBudgetTransitionResult? ValidateTransition(
        ProviderRequestId providerRequestId,
        ProviderBudgetLedgerRevision expectedLedgerRevision,
        long expectedReservationRevision,
        ProviderBudgetReservationStatus requiredStatus,
        DateTimeOffset occurredAtUtc)
    {
        if (envelope is null)
        {
            return Rejected(ProviderBudgetTransitionRejection.EnvelopeUnavailable, null);
        }

        if (!reservations.TryGetValue(providerRequestId, out var reservation))
        {
            return Rejected(ProviderBudgetTransitionRejection.ReservationUnavailable, null);
        }

        if (envelope.State != ProviderBudgetState.Armed)
        {
            return Rejected(ProviderBudgetTransitionRejection.EnvelopeNotArmed, reservation);
        }

        if (expectedLedgerRevision != envelope.LedgerRevision)
        {
            return Rejected(
                ProviderBudgetTransitionRejection.LedgerRevisionMismatch,
                reservation);
        }

        if (expectedReservationRevision != reservation.CurrentReservationRevision)
        {
            return Rejected(
                ProviderBudgetTransitionRejection.ReservationRevisionMismatch,
                reservation);
        }

        if (reservation.Status != requiredStatus)
        {
            return Rejected(
                ProviderBudgetTransitionRejection.InvalidReservationState,
                reservation);
        }

        var earliest = reservation.DispatchStartedAtUtc ?? reservation.AdmittedAtUtc;

        if (occurredAtUtc < earliest)
        {
            return Rejected(
                ProviderBudgetTransitionRejection.InvalidTransitionTime,
                reservation);
        }

        return null;
    }

    private ProviderBudgetTransitionResult Applied(ProviderBudgetReservation reservation) =>
        new(
            ProviderBudgetTransitionOutcome.Applied,
            envelope!.State,
            envelope.LedgerRevision,
            reservation,
            rejection: null);

    private ProviderBudgetTransitionResult Replay(ProviderBudgetReservation reservation) =>
        new(
            ProviderBudgetTransitionOutcome.Replay,
            envelope?.State ?? ProviderBudgetState.Disarmed,
            envelope?.LedgerRevision,
            reservation,
            rejection: null);

    private ProviderBudgetTransitionResult Rejected(
        ProviderBudgetTransitionRejection rejection,
        ProviderBudgetReservation? reservation) =>
        new(
            ProviderBudgetTransitionOutcome.Rejected,
            envelope?.State ?? ProviderBudgetState.Disarmed,
            envelope?.LedgerRevision,
            reservation,
            rejection);

    private ProviderBudgetTransitionResult Conflict(ProviderRequestId providerRequestId)
    {
        reservations.TryGetValue(providerRequestId, out var reservation);
        Trip("transition-conflict", providerRequestId);
        return new ProviderBudgetTransitionResult(
            ProviderBudgetTransitionOutcome.Conflict,
            ProviderBudgetState.Tripped,
            envelope?.LedgerRevision,
            reservation,
            rejection: null);
    }

    private void Trip(string transition, ProviderRequestId providerRequestId)
    {
        if (envelope is null || envelope.State == ProviderBudgetState.Tripped)
        {
            return;
        }

        var nextRevision = NextLedgerRevision(envelope);
        envelope = envelope.WithLedger(
            nextRevision,
            ProviderBudgetState.Tripped,
            envelope.AggregateCommitted,
            envelope.AggregateReserved,
            envelope.AggregateIndeterminate,
            envelope.OperationBalances,
            FakeDigest(transition, nextRevision, providerRequestId));
    }

    private void AdvanceState(ProviderBudgetState state, string transition)
    {
        var current = envelope!;
        var nextRevision = NextLedgerRevision(current);
        envelope = current.WithLedger(
            nextRevision,
            state,
            current.AggregateCommitted,
            current.AggregateReserved,
            current.AggregateIndeterminate,
            current.OperationBalances,
            FakeDigest(transition, nextRevision, providerRequestId: null));
    }

    private static ProviderBudgetLedgerRevision NextLedgerRevision(
        ProviderBudgetEnvelopeV1 current) =>
        new(checked(current.LedgerRevision.Value + 1));

    private static ProviderBudgetSha256 FakeDigest(
        string transition,
        ProviderBudgetLedgerRevision revision,
        ProviderRequestId? providerRequestId)
    {
        var canonical = string.Join(
            '\n',
            "fake-provider-budget-ledger-v1",
            transition,
            revision.Value.ToString(CultureInfo.InvariantCulture),
            providerRequestId?.Value ?? "-");
        return new ProviderBudgetSha256(
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)))
                .ToLowerInvariant());
    }

    private static bool HasIdenticalReservationBinding(
        ProviderBudgetAdmissionRequest left,
        ProviderBudgetAdmissionRequest right) =>
        left.EnvelopeId == right.EnvelopeId &&
        left.StoreEpochId == right.StoreEpochId &&
        left.ExpectedConfigurationRevision == right.ExpectedConfigurationRevision &&
        left.RuntimeSessionId == right.RuntimeSessionId &&
        left.Scope == right.Scope &&
        left.CostScheduleId == right.CostScheduleId &&
        left.CostScheduleSha256 == right.CostScheduleSha256 &&
        left.OperationClass == right.OperationClass &&
        left.OperationAuthorityReference == right.OperationAuthorityReference &&
        left.RequestPlanSha256 == right.RequestPlanSha256 &&
        left.RequestSha256 == right.RequestSha256 &&
        left.MaximumChargeBasisSha256 == right.MaximumChargeBasisSha256 &&
        left.BindingSha256 == right.BindingSha256 &&
        left.MaximumCharge == right.MaximumCharge;

    private static bool IsZeroBudget(ProviderBudgetEnvelopeV1 value) =>
        value.AggregateLimit.Value == 0 &&
        value.AggregateCommitted.Value == 0 &&
        value.AggregateReserved.Value == 0 &&
        value.AggregateIndeterminate.Value == 0 &&
        value.OperationBalances.All(balance =>
            balance.AllocationLimit.Value == 0 &&
            balance.Committed.Value == 0 &&
            balance.Reserved.Value == 0 &&
            balance.Indeterminate.Value == 0);

    private static void ValidateTransitionRequest(
        ProviderRequestId providerRequestId,
        ProviderBudgetLedgerRevision expectedLedgerRevision,
        long expectedReservationRevision,
        DateTimeOffset occurredAtUtc)
    {
        ArgumentNullException.ThrowIfNull(providerRequestId);
        ArgumentNullException.ThrowIfNull(expectedLedgerRevision);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(expectedReservationRevision);

        if (occurredAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Provider-budget transition instants must use an explicit UTC offset.",
                nameof(occurredAtUtc));
        }
    }

    private static void ValidateCommitRequest(ProviderBudgetCommitRequest request)
    {
        ValidateTransitionRequest(
            request.ProviderRequestId,
            request.ExpectedLedgerRevision,
            request.ExpectedReservationRevision,
            request.OccurredAtUtc);
        ArgumentNullException.ThrowIfNull(request.UsageEvidenceSha256);
        ArgumentNullException.ThrowIfNull(request.ProviderOutcomeCode);

        if (!Enum.IsDefined(request.CommitmentKind))
        {
            throw new ArgumentOutOfRangeException(nameof(request));
        }

        if (request.ProviderDuration is { } duration &&
            (duration < TimeSpan.Zero || duration > TimeSpan.FromDays(1)))
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "Provider duration must be between zero and one day.");
        }
    }

    private static void ValidateReleaseRequest(ProviderBudgetReleaseRequest request)
    {
        ValidateTransitionRequest(
            request.ProviderRequestId,
            request.ExpectedLedgerRevision,
            request.ExpectedReservationRevision,
            request.OccurredAtUtc);
        ArgumentNullException.ThrowIfNull(request.ProofSha256);
        ArgumentNullException.ThrowIfNull(request.AuthorityReference);

        if (!Enum.IsDefined(request.ProofKind))
        {
            throw new ArgumentOutOfRangeException(nameof(request));
        }
    }
}
