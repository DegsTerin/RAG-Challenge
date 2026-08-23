// Purpose: Implements the transactional Control-store provider-budget ledger and explicit local rearming boundary; provider credentials, egress, pricing and envelope creation remain outside this component.
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using RagChallenge.Application.ProviderBudget;

namespace RagChallenge.Infrastructure.Persistence;

public enum ProviderBudgetRearmOutcome
{
    Applied,
    Rejected,
    Conflict,
}

public sealed record ProviderBudgetRearmRequest
{
    public ProviderBudgetRearmRequest(
        ProviderBudgetEnvelopeId envelopeId,
        ProviderBudgetStoreEpochId expectedStoreEpochId,
        ProviderBudgetConfigurationRevision expectedConfigurationRevision,
        ProviderBudgetLedgerRevision expectedLedgerRevision,
        ProviderBudgetRearmRevision expectedRearmRevision,
        ProviderRuntimeSessionId newRuntimeSessionId,
        ProviderBudgetAuthorityReference authorityReference,
        ProviderBudgetAuthorityReference actorReference,
        ProviderBudgetSha256 reasonSha256,
        ProviderBudgetUnits acknowledgedCommitted,
        ProviderBudgetUnits acknowledgedReserved,
        ProviderBudgetUnits acknowledgedIndeterminate,
        ProviderBudgetSha256 operationBalancesSha256,
        ProviderBudgetSha256 configurationSha256,
        DateTimeOffset occurredAtUtc)
    {
        EnvelopeId = envelopeId ?? throw new ArgumentNullException(nameof(envelopeId));
        ExpectedStoreEpochId = expectedStoreEpochId ??
            throw new ArgumentNullException(nameof(expectedStoreEpochId));
        ExpectedConfigurationRevision = expectedConfigurationRevision ??
            throw new ArgumentNullException(nameof(expectedConfigurationRevision));
        ExpectedLedgerRevision = expectedLedgerRevision ??
            throw new ArgumentNullException(nameof(expectedLedgerRevision));
        NewRuntimeSessionId = newRuntimeSessionId ??
            throw new ArgumentNullException(nameof(newRuntimeSessionId));
        AuthorityReference = authorityReference ??
            throw new ArgumentNullException(nameof(authorityReference));
        ActorReference = actorReference ?? throw new ArgumentNullException(nameof(actorReference));
        ReasonSha256 = reasonSha256 ?? throw new ArgumentNullException(nameof(reasonSha256));
        OperationBalancesSha256 = operationBalancesSha256 ??
            throw new ArgumentNullException(nameof(operationBalancesSha256));
        ConfigurationSha256 = configurationSha256 ??
            throw new ArgumentNullException(nameof(configurationSha256));

        if (occurredAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Provider-budget rearming instants must use an explicit UTC offset.",
                nameof(occurredAtUtc));
        }

        ExpectedRearmRevision = expectedRearmRevision;
        AcknowledgedCommitted = acknowledgedCommitted;
        AcknowledgedReserved = acknowledgedReserved;
        AcknowledgedIndeterminate = acknowledgedIndeterminate;
        OccurredAtUtc = occurredAtUtc;
    }

    public ProviderBudgetEnvelopeId EnvelopeId { get; }
    public ProviderBudgetStoreEpochId ExpectedStoreEpochId { get; }
    public ProviderBudgetConfigurationRevision ExpectedConfigurationRevision { get; }
    public ProviderBudgetLedgerRevision ExpectedLedgerRevision { get; }
    public ProviderBudgetRearmRevision ExpectedRearmRevision { get; }
    public ProviderRuntimeSessionId NewRuntimeSessionId { get; }
    public ProviderBudgetAuthorityReference AuthorityReference { get; }
    public ProviderBudgetAuthorityReference ActorReference { get; }
    public ProviderBudgetSha256 ReasonSha256 { get; }
    public ProviderBudgetUnits AcknowledgedCommitted { get; }
    public ProviderBudgetUnits AcknowledgedReserved { get; }
    public ProviderBudgetUnits AcknowledgedIndeterminate { get; }
    public ProviderBudgetSha256 OperationBalancesSha256 { get; }
    public ProviderBudgetSha256 ConfigurationSha256 { get; }
    public DateTimeOffset OccurredAtUtc { get; }
}

public sealed record ProviderBudgetRearmResult(
    ProviderBudgetRearmOutcome Outcome,
    ProviderBudgetEnvelopeV1? Envelope);

public interface IProviderBudgetRearmControl
{
    Task<ProviderBudgetRearmResult> RearmAsync(
        ProviderBudgetRearmRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class SqliteProviderBudgetLedger : IProviderBudgetLedger, IProviderBudgetRearmControl
{
    private const string ZeroSha256 =
        "0000000000000000000000000000000000000000000000000000000000000000";
    private readonly SqliteStoreOptions options;

    public SqliteProviderBudgetLedger(SqliteStoreOptions options)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<ProviderBudgetEnvelopeV1?> ReadEnvelopeAsync(
        ProviderBudgetEnvelopeId envelopeId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelopeId);
        await using var context = options.CreateControlContext();
        return await ReadEnvelopeWithinAsync(context, envelopeId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<ProviderBudgetReservation?> ReadReservationAsync(
        ProviderRequestId providerRequestId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(providerRequestId);
        await using var context = options.CreateControlContext();
        return await ReadReservationWithinAsync(context, providerRequestId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<ProviderBudgetAdmissionResult> AdmitAsync(
        ProviderBudgetAdmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await using var context = options.CreateControlContext();
        await using var transaction = await BeginImmediateAsync(context, cancellationToken)
            .ConfigureAwait(false);
        var existingRow = await context.ProviderBudgetReservations
            .SingleOrDefaultAsync(
                row => row.ProviderRequestId == request.ProviderRequestId.Value,
                cancellationToken)
            .ConfigureAwait(false);

        if (existingRow is not null)
        {
            var existingEnvelope = await RequireEnvelopeWithinAsync(
                context,
                new ProviderBudgetEnvelopeId(existingRow.EnvelopeId),
                cancellationToken).ConfigureAwait(false);

            if (!existingEnvelope.IsClosed &&
                request.RequestedAtUtc >= existingEnvelope.ExpiresAtUtc &&
                existingEnvelope.State is not (
                    ProviderBudgetState.Expired or
                    ProviderBudgetState.ReconciliationRequired))
            {
                existingEnvelope = await PersistExpiredAsync(
                    context,
                    existingEnvelope,
                    request,
                    cancellationToken).ConfigureAwait(false);
            }

            if (HasIdenticalBinding(existingRow, request))
            {
                var replay = ToDomain(existingRow);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                return new ProviderBudgetAdmissionResult(
                    ProviderBudgetAdmissionOutcome.Replay,
                    existingEnvelope.State,
                    existingEnvelope.LedgerRevision,
                    replay,
                    rejection: null);
            }

            if (existingEnvelope.State != ProviderBudgetState.Armed)
            {
                await AddPreservedStateConflictAuditAsync(
                    context,
                    existingEnvelope,
                    existingRow,
                    request.OperationAuthorityReference,
                    request.RequestedAtUtc,
                    AdmissionConflictSha256(request),
                    cancellationToken).ConfigureAwait(false);
                var stateRejection = existingEnvelope.IsClosed
                    ? ProviderBudgetAdmissionRejection.Closed
                    : existingEnvelope.State switch
                    {
                        ProviderBudgetState.Disarmed =>
                            ProviderBudgetAdmissionRejection.Disarmed,
                        ProviderBudgetState.Tripped =>
                            ProviderBudgetAdmissionRejection.Tripped,
                        ProviderBudgetState.Exhausted =>
                            ProviderBudgetAdmissionRejection.Exhausted,
                        ProviderBudgetState.ReconciliationRequired =>
                            ProviderBudgetAdmissionRejection.ReconciliationRequired,
                        ProviderBudgetState.Expired =>
                            ProviderBudgetAdmissionRejection.Expired,
                        _ => ProviderBudgetAdmissionRejection.Disarmed,
                    };
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                return new ProviderBudgetAdmissionResult(
                    ProviderBudgetAdmissionOutcome.Rejected,
                    existingEnvelope.State,
                    existingEnvelope.LedgerRevision,
                    reservation: null,
                    stateRejection);
            }

            existingEnvelope = await PersistStateOnlyAsync(
                context,
                existingEnvelope,
                ProviderBudgetState.Tripped,
                "ConflictTripped",
                request.ProviderRequestId,
                request.OperationAuthorityReference,
                request.RequestedAtUtc,
                cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return new ProviderBudgetAdmissionResult(
                ProviderBudgetAdmissionOutcome.Conflict,
                existingEnvelope.State,
                existingEnvelope.LedgerRevision,
                reservation: null,
                rejection: null);
        }

        var envelope = await ReadEnvelopeWithinAsync(context, request.EnvelopeId, cancellationToken)
            .ConfigureAwait(false);

        if (envelope is { IsClosed: false } &&
            request.RequestedAtUtc >= envelope.ExpiresAtUtc &&
            envelope.State is not (
                ProviderBudgetState.Expired or
                ProviderBudgetState.ReconciliationRequired))
        {
            envelope = await PersistExpiredAsync(context, envelope, request, cancellationToken)
                .ConfigureAwait(false);
        }

        var rejection = ValidateAdmission(envelope, request);

        if (rejection is not null)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return new ProviderBudgetAdmissionResult(
                ProviderBudgetAdmissionOutcome.Rejected,
                envelope?.State ?? ProviderBudgetState.Disarmed,
                envelope?.LedgerRevision,
                reservation: null,
                rejection);
        }

        var current = envelope!;
        var balances = ReplaceOperationBalance(
            current,
            request.OperationClass,
            balance => new ProviderBudgetOperationBalance(
                balance.OperationClass,
                balance.AllocationLimit,
                balance.Committed,
                new ProviderBudgetUnits(checked(
                    balance.Reserved.Value + request.MaximumCharge.Value)),
                balance.Indeterminate));
        var next = await PersistLedgerAsync(
            context,
            current,
            ProviderBudgetState.Armed,
            current.RuntimeSessionId,
            current.AggregateCommitted,
            new ProviderBudgetUnits(checked(
                current.AggregateReserved.Value + request.MaximumCharge.Value)),
            current.AggregateIndeterminate,
            balances,
            "ReservationAdmitted",
            request.ProviderRequestId,
            request.OperationAuthorityReference,
            request.RequestedAtUtc,
            cancellationToken).ConfigureAwait(false);
        var transitionSha = Digest(
            "provider-budget-reservation-transition-v1",
            request.ProviderRequestId.Value,
            "1",
            "Reserved",
            request.RequestedAtUtc);
        var reservationRow = new ProviderBudgetReservationRow
        {
            ProviderRequestId = request.ProviderRequestId.Value,
            EnvelopeId = request.EnvelopeId.Value,
            StoreEpochId = request.StoreEpochId.Value,
            ConfigurationRevision = request.ExpectedConfigurationRevision.Value,
            OperationClass = request.OperationClass.ToString(),
            OperationAuthorityReference = request.OperationAuthorityReference.Value,
            RequestPlanSha256 = request.RequestPlanSha256.Value,
            RequestSha256 = request.RequestSha256.Value,
            MaximumChargeBasisSha256 = request.MaximumChargeBasisSha256.Value,
            CostScheduleSha256 = request.CostScheduleSha256.Value,
            BindingSha256 = request.BindingSha256.Value,
            MaximumChargeUnits = request.MaximumCharge.Value,
            AdmittedRuntimeSessionId = request.RuntimeSessionId.Value,
            AdmissionLedgerRevision = next.LedgerRevision.Value,
            CurrentReservationRevision = 1,
            IsInitialised = 0,
            Status = ProviderBudgetReservationStatus.Reserved.ToString(),
            AdmittedAtUtc = FormatUtc(request.RequestedAtUtc),
            DispatchStartedAtUtc = null,
            TerminalAtUtc = null,
            TerminalLedgerRevision = null,
            CurrentTransitionSha256 = transitionSha,
        };
        context.ProviderBudgetReservations.Add(reservationRow);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        context.ProviderBudgetReservationTransitions.Add(new ProviderBudgetReservationTransitionRow
        {
            ProviderRequestId = request.ProviderRequestId.Value,
            ReservationRevision = 1,
            EnvelopeId = request.EnvelopeId.Value,
            LedgerRevision = next.LedgerRevision.Value,
            FromStatus = null,
            ToStatus = ProviderBudgetReservationStatus.Reserved.ToString(),
            TransitionKind = "Admission",
            ProofSha256 = null,
            OutcomeCode = null,
            OccurredAtUtc = FormatUtc(request.RequestedAtUtc),
            PreviousTransitionSha256 = ZeroSha256,
            TransitionSha256 = transitionSha,
        });
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        reservationRow.IsInitialised = 1;
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await AddAuditAsync(
            context,
            next,
            "ReservationAdmitted",
            request.ProviderRequestId,
            request.OperationClass,
            request.OperationAuthorityReference,
            request.RequestSha256,
            request.MaximumCharge,
            current.State,
            next.State,
            "Admitted",
            request.RequestedAtUtc,
            cancellationToken).ConfigureAwait(false);
        await AdvanceEnvelopeAsync(context, next, cancellationToken).ConfigureAwait(false);
        context.ChangeTracker.Clear();
        var readback = await RequireReservationWithinAsync(
            context,
            request.ProviderRequestId,
            cancellationToken).ConfigureAwait(false);
        var envelopeReadback = await RequireEnvelopeWithinAsync(
            context,
            request.EnvelopeId,
            cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new ProviderBudgetAdmissionResult(
            ProviderBudgetAdmissionOutcome.Admitted,
            envelopeReadback.State,
            envelopeReadback.LedgerRevision,
            readback,
            rejection: null);
    }

    public Task<ProviderBudgetTransitionResult> MarkDispatchStartedAsync(
        ProviderBudgetDispatchRequest request,
        CancellationToken cancellationToken = default) =>
        ApplyTransitionAsync(request, cancellationToken);

    public Task<ProviderBudgetTransitionResult> CommitAsync(
        ProviderBudgetCommitRequest request,
        CancellationToken cancellationToken = default) =>
        ApplyTransitionAsync(request, cancellationToken);

    public Task<ProviderBudgetTransitionResult> ReleasePreSendAsync(
        ProviderBudgetReleaseRequest request,
        CancellationToken cancellationToken = default) =>
        ApplyTransitionAsync(request, cancellationToken);

    public async Task<ProviderBudgetRearmResult> RearmAsync(
        ProviderBudgetRearmRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await using var context = options.CreateControlContext();
        await using var transaction = await BeginImmediateAsync(context, cancellationToken)
            .ConfigureAwait(false);
        var current = await ReadEnvelopeWithinAsync(context, request.EnvelopeId, cancellationToken)
            .ConfigureAwait(false);

        if (current is null || current.IsClosed)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return new ProviderBudgetRearmResult(ProviderBudgetRearmOutcome.Rejected, current);
        }

        var configuration = await context.ProviderBudgetConfigurations.AsNoTracking()
            .SingleAsync(row => row.EnvelopeId == request.EnvelopeId.Value &&
                row.ConfigurationRevision == current.ConfigurationRevision.Value,
                cancellationToken).ConfigureAwait(false);
        var expectedBalancesSha = new ProviderBudgetSha256(OperationBalancesDigest(current));
        var exactRearmBinding = current.StoreEpochId == request.ExpectedStoreEpochId &&
            current.ConfigurationRevision == request.ExpectedConfigurationRevision &&
            current.LedgerRevision == request.ExpectedLedgerRevision &&
            current.RearmRevision == request.ExpectedRearmRevision &&
            current.AggregateCommitted == request.AcknowledgedCommitted &&
            current.AggregateReserved == request.AcknowledgedReserved &&
            current.AggregateIndeterminate == request.AcknowledgedIndeterminate &&
            expectedBalancesSha == request.OperationBalancesSha256 &&
            string.Equals(
                configuration.ConfigurationSha256,
                request.ConfigurationSha256.Value,
                StringComparison.Ordinal) &&
            configuration.SealedAtUtc is not null &&
            current.RuntimeSessionId != request.NewRuntimeSessionId &&
            request.OccurredAtUtc >= current.EffectiveAtUtc;
        var exactZeroBudgetRecoveryBinding = exactRearmBinding && IsZeroBudget(current);
        var exactInitialNonZeroArmBinding = exactRearmBinding &&
            IsUnusedNonZeroBudget(current) &&
            current.State == ProviderBudgetState.Disarmed &&
            current.RearmRevision.Value == 0;

        if (exactZeroBudgetRecoveryBinding)
        {
            var revisionBeforeRecovery = current.LedgerRevision;
            current = await RecoverOrphanedDispatchesAsync(
                context,
                current,
                request,
                cancellationToken).ConfigureAwait(false);

            if (current.LedgerRevision != revisionBeforeRecovery)
            {
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                return new ProviderBudgetRearmResult(
                    ProviderBudgetRearmOutcome.Rejected,
                    current);
            }
        }

        var rearmableState = current.State is
            ProviderBudgetState.Disarmed or ProviderBudgetState.Tripped;
        if (!rearmableState)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return new ProviderBudgetRearmResult(ProviderBudgetRearmOutcome.Rejected, current);
        }

        var exactRearm = (exactZeroBudgetRecoveryBinding || exactInitialNonZeroArmBinding) &&
            request.OccurredAtUtc < current.ExpiresAtUtc;
        if (!exactRearm)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return new ProviderBudgetRearmResult(ProviderBudgetRearmOutcome.Conflict, current);
        }

        var hasPendingReservation = await context.ProviderBudgetReservations.AsNoTracking()
            .AnyAsync(
                row => row.EnvelopeId == current.EnvelopeId.Value &&
                    (row.Status == nameof(ProviderBudgetReservationStatus.Reserved) ||
                     row.Status == nameof(ProviderBudgetReservationStatus.DispatchStarted)),
                cancellationToken).ConfigureAwait(false);

        if (hasPendingReservation)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return new ProviderBudgetRearmResult(ProviderBudgetRearmOutcome.Rejected, current);
        }

        var nextRearm = new ProviderBudgetRearmRevision(
            checked(current.RearmRevision.Value + 1));
        var next = await PersistLedgerAsync(
            context,
            current,
            ProviderBudgetState.Armed,
            request.NewRuntimeSessionId,
            current.AggregateCommitted,
            current.AggregateReserved,
            current.AggregateIndeterminate,
            current.OperationBalances,
            "Rearmed",
            providerRequestId: null,
            request.AuthorityReference,
            request.OccurredAtUtc,
            cancellationToken,
            nextRearm).ConfigureAwait(false);
        var rearmSha = Digest(
            "provider-budget-rearm-v1",
            request.EnvelopeId.Value,
            nextRearm.Value,
            request.NewRuntimeSessionId.Value,
            request.ReasonSha256.Value,
            request.OccurredAtUtc);
        context.ProviderBudgetRearms.Add(new ProviderBudgetRearmRow
        {
            EnvelopeId = request.EnvelopeId.Value,
            RearmRevision = nextRearm.Value,
            StoreEpochId = request.ExpectedStoreEpochId.Value,
            ExpectedConfigurationRevision = request.ExpectedConfigurationRevision.Value,
            ExpectedLedgerRevision = request.ExpectedLedgerRevision.Value,
            ExpectedRearmRevision = request.ExpectedRearmRevision.Value,
            ResultingLedgerRevision = next.LedgerRevision.Value,
            NewRuntimeSessionId = request.NewRuntimeSessionId.Value,
            AuthorityReference = request.AuthorityReference.Value,
            ActorReference = request.ActorReference.Value,
            ReasonSha256 = request.ReasonSha256.Value,
            AcknowledgedCommittedUnits = request.AcknowledgedCommitted.Value,
            AcknowledgedReservedUnits = request.AcknowledgedReserved.Value,
            AcknowledgedIndeterminateUnits = request.AcknowledgedIndeterminate.Value,
            OperationBalancesSha256 = request.OperationBalancesSha256.Value,
            ConfigurationSha256 = request.ConfigurationSha256.Value,
            OccurredAtUtc = FormatUtc(request.OccurredAtUtc),
            RearmSha256 = rearmSha,
        });
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await AddAuditAsync(
            context,
            next,
            "EnvelopeRearmed",
            providerRequestId: null,
            operationClass: null,
            request.AuthorityReference,
            requestSha256: null,
            maximumCharge: null,
            current.State,
            next.State,
            "Rearmed",
            request.OccurredAtUtc,
            cancellationToken,
            request.ActorReference).ConfigureAwait(false);
        await AdvanceEnvelopeAsync(context, next, cancellationToken).ConfigureAwait(false);
        context.ChangeTracker.Clear();
        var readback = await RequireEnvelopeWithinAsync(
            context,
            request.EnvelopeId,
            cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new ProviderBudgetRearmResult(ProviderBudgetRearmOutcome.Applied, readback);
    }

    public static ProviderBudgetSha256 ComputeOperationBalancesSha256(
        ProviderBudgetEnvelopeV1 envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        return new ProviderBudgetSha256(OperationBalancesDigest(envelope));
    }

    private async Task<ProviderBudgetTransitionResult> ApplyTransitionAsync(
        object request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var requestId = request switch
        {
            ProviderBudgetDispatchRequest value => value.ProviderRequestId,
            ProviderBudgetCommitRequest value => value.ProviderRequestId,
            ProviderBudgetReleaseRequest value => value.ProviderRequestId,
            _ => throw new ArgumentOutOfRangeException(nameof(request)),
        };
        var expectedLedgerRevision = request switch
        {
            ProviderBudgetDispatchRequest value => value.ExpectedLedgerRevision,
            ProviderBudgetCommitRequest value => value.ExpectedLedgerRevision,
            ProviderBudgetReleaseRequest value => value.ExpectedLedgerRevision,
            _ => throw new ArgumentOutOfRangeException(nameof(request)),
        };
        var expectedReservationRevision = request switch
        {
            ProviderBudgetDispatchRequest value => value.ExpectedReservationRevision,
            ProviderBudgetCommitRequest value => value.ExpectedReservationRevision,
            ProviderBudgetReleaseRequest value => value.ExpectedReservationRevision,
            _ => throw new ArgumentOutOfRangeException(nameof(request)),
        };
        var occurredAtUtc = request switch
        {
            ProviderBudgetDispatchRequest value => value.OccurredAtUtc,
            ProviderBudgetCommitRequest value => value.OccurredAtUtc,
            ProviderBudgetReleaseRequest value => value.OccurredAtUtc,
            _ => throw new ArgumentOutOfRangeException(nameof(request)),
        };
        await using var context = options.CreateControlContext();
        await using var transaction = await BeginImmediateAsync(context, cancellationToken)
            .ConfigureAwait(false);
        var row = await context.ProviderBudgetReservations.SingleOrDefaultAsync(
            value => value.ProviderRequestId == requestId.Value,
            cancellationToken).ConfigureAwait(false);

        if (row is null)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return Rejected(
                ProviderBudgetTransitionRejection.ReservationUnavailable,
                envelope: null,
                reservation: null);
        }

        var current = await RequireEnvelopeWithinAsync(
            context,
            new ProviderBudgetEnvelopeId(row.EnvelopeId),
            cancellationToken).ConfigureAwait(false);
        var reservation = ToDomain(row);
        var replay = await ReadTransitionReplayAsync(context, request, cancellationToken)
            .ConfigureAwait(false);

        if (replay is not null)
        {
            if (replay.Value)
            {
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                return new ProviderBudgetTransitionResult(
                    ProviderBudgetTransitionOutcome.Replay,
                    current.State,
                    current.LedgerRevision,
                    reservation,
                    rejection: null);
            }

            if (current.State != ProviderBudgetState.Armed)
            {
                await AddPreservedStateConflictAuditAsync(
                    context,
                    current,
                    row,
                    reservation.OperationAuthorityReference,
                    occurredAtUtc,
                    TransitionConflictSha256(request),
                    cancellationToken).ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                return Rejected(
                    ProviderBudgetTransitionRejection.EnvelopeNotArmed,
                    current,
                    reservation);
            }

            current = await PersistStateOnlyAsync(
                context,
                current,
                ProviderBudgetState.Tripped,
                "ConflictTripped",
                requestId,
                reservation.OperationAuthorityReference,
                occurredAtUtc,
                cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return new ProviderBudgetTransitionResult(
                ProviderBudgetTransitionOutcome.Conflict,
                current.State,
                current.LedgerRevision,
                reservation,
                rejection: null);
        }

        var requiredStatus = request is ProviderBudgetDispatchRequest or ProviderBudgetReleaseRequest
            ? ProviderBudgetReservationStatus.Reserved
            : ProviderBudgetReservationStatus.DispatchStarted;
        var rejection = ValidateTransition(
            current,
            reservation,
            expectedLedgerRevision,
            expectedReservationRevision,
            requiredStatus,
            occurredAtUtc);

        if (rejection is not null)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return Rejected(rejection.Value, current, reservation);
        }

        var persisted = await PersistTransitionAsync(
            context,
            current,
            row,
            reservation,
            request,
            occurredAtUtc,
            cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new ProviderBudgetTransitionResult(
            ProviderBudgetTransitionOutcome.Applied,
            persisted.Envelope.State,
            persisted.Envelope.LedgerRevision,
            persisted.Reservation,
            rejection: null);
    }

    private static async Task<PersistedTransition> PersistTransitionAsync(
        ControlPlaneDbContext context,
        ProviderBudgetEnvelopeV1 current,
        ProviderBudgetReservationRow row,
        ProviderBudgetReservation reservation,
        object request,
        DateTimeOffset occurredAtUtc,
        CancellationToken cancellationToken,
        ProviderBudgetAuthorityReference? authorityOverride = null,
        ProviderBudgetAuthorityReference? actorReference = null)
    {
        var requestId = reservation.ProviderRequestId;
        var transition = CreateTransition(current, reservation, request, occurredAtUtc);

        if (authorityOverride is not null)
        {
            transition = transition with { AuthorityReference = authorityOverride };
        }

        var next = await PersistLedgerAsync(
            context,
            current,
            transition.State,
            current.RuntimeSessionId,
            transition.AggregateCommitted,
            transition.AggregateReserved,
            transition.AggregateIndeterminate,
            transition.Balances,
            transition.LedgerTransitionKind,
            requestId,
            transition.AuthorityReference,
            occurredAtUtc,
            cancellationToken).ConfigureAwait(false);
        var nextReservationRevision = checked(reservation.CurrentReservationRevision + 1);
        var transitionSha = Digest(
            "provider-budget-reservation-transition-v1",
            requestId.Value,
            nextReservationRevision,
            transition.ReservationStatus.ToString(),
            occurredAtUtc);
        context.ProviderBudgetReservationTransitions.Add(new ProviderBudgetReservationTransitionRow
        {
            ProviderRequestId = requestId.Value,
            ReservationRevision = nextReservationRevision,
            EnvelopeId = reservation.EnvelopeId.Value,
            LedgerRevision = next.LedgerRevision.Value,
            FromStatus = reservation.Status.ToString(),
            ToStatus = transition.ReservationStatus.ToString(),
            TransitionKind = transition.ReservationTransitionKind,
            ProofSha256 = transition.ProofSha256,
            OutcomeCode = transition.OutcomeCode,
            OccurredAtUtc = FormatUtc(occurredAtUtc),
            PreviousTransitionSha256 = row.CurrentTransitionSha256,
            TransitionSha256 = transitionSha,
        });
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        row.CurrentReservationRevision = nextReservationRevision;
        row.Status = transition.ReservationStatus.ToString();
        row.DispatchStartedAtUtc = transition.DispatchStartedAtUtc is null
            ? null
            : FormatUtc(transition.DispatchStartedAtUtc.Value);
        row.TerminalAtUtc = transition.TerminalAtUtc is null
            ? null
            : FormatUtc(transition.TerminalAtUtc.Value);
        row.TerminalLedgerRevision = transition.TerminalAtUtc is null
            ? null
            : next.LedgerRevision.Value;
        row.CurrentTransitionSha256 = transitionSha;
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await AddTerminalEvidenceAsync(
            context,
            reservation,
            next,
            request,
            transition,
            occurredAtUtc,
            cancellationToken).ConfigureAwait(false);
        await AddAuditAsync(
            context,
            next,
            transition.AuditEventType,
            requestId,
            reservation.OperationClass,
            transition.AuthorityReference,
            reservation.RequestSha256,
            reservation.MaximumCharge,
            current.State,
            next.State,
            transition.AuditOutcome,
            occurredAtUtc,
            cancellationToken,
            actorReference).ConfigureAwait(false);
        await AdvanceEnvelopeAsync(context, next, cancellationToken).ConfigureAwait(false);
        context.ChangeTracker.Clear();
        var readback = await RequireReservationWithinAsync(context, requestId, cancellationToken)
            .ConfigureAwait(false);
        var envelopeReadback = await RequireEnvelopeWithinAsync(
            context,
            reservation.EnvelopeId,
            cancellationToken).ConfigureAwait(false);
        return new PersistedTransition(envelopeReadback, readback);
    }

    private static async Task<ProviderBudgetEnvelopeV1> RecoverOrphanedDispatchesAsync(
        ControlPlaneDbContext context,
        ProviderBudgetEnvelopeV1 current,
        ProviderBudgetRearmRequest request,
        CancellationToken cancellationToken)
    {
        var orphanedRequestIds = await context.ProviderBudgetReservations.AsNoTracking()
            .Where(row => row.EnvelopeId == current.EnvelopeId.Value &&
                row.Status == nameof(ProviderBudgetReservationStatus.DispatchStarted))
            .OrderBy(row => row.ProviderRequestId)
            .Select(row => row.ProviderRequestId)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);

        foreach (var orphanedRequestId in orphanedRequestIds)
        {
            var row = await context.ProviderBudgetReservations.SingleAsync(
                value => value.ProviderRequestId == orphanedRequestId,
                cancellationToken).ConfigureAwait(false);
            var reservation = ToDomain(row);
            var recoveryAtUtc = reservation.DispatchStartedAtUtc is { } dispatchStartedAtUtc &&
                request.OccurredAtUtc < dispatchStartedAtUtc
                    ? dispatchStartedAtUtc
                    : request.OccurredAtUtc;
            var recovery = new ProviderBudgetCommitRequest(
                reservation.ProviderRequestId,
                current.LedgerRevision,
                reservation.CurrentReservationRevision,
                ProviderBudgetCommitmentKind.IndeterminateMaximum,
                reservation.MaximumCharge,
                new ProviderBudgetSha256(Digest(
                    "provider-budget-orphaned-dispatch-recovery-v1",
                    reservation.ProviderRequestId.Value,
                    row.CurrentTransitionSha256,
                    request.NewRuntimeSessionId.Value,
                    recoveryAtUtc)),
                new ProviderBudgetOutcomeCode("RESTART_ORPHAN_RECOVERY"),
                providerDuration: null,
                recoveryAtUtc);
            var persisted = await PersistTransitionAsync(
                context,
                current,
                row,
                reservation,
                recovery,
                recoveryAtUtc,
                cancellationToken,
                request.AuthorityReference,
                request.ActorReference).ConfigureAwait(false);
            current = persisted.Envelope;
        }

        return current;
    }

    private static TransitionShape CreateTransition(
        ProviderBudgetEnvelopeV1 current,
        ProviderBudgetReservation reservation,
        object request,
        DateTimeOffset occurredAtUtc)
    {
        if (request is ProviderBudgetDispatchRequest)
        {
            return new TransitionShape(
                current.State,
                current.AggregateCommitted,
                current.AggregateReserved,
                current.AggregateIndeterminate,
                current.OperationBalances,
                ProviderBudgetReservationStatus.DispatchStarted,
                "DispatchStarted",
                "DispatchStarted",
                "DispatchStarted",
                reservation.OperationAuthorityReference,
                occurredAtUtc,
                TerminalAtUtc: null,
                ProofSha256: null,
                OutcomeCode: null,
                "Applied");
        }

        if (request is ProviderBudgetReleaseRequest release)
        {
            var balances = ReplaceOperationBalance(
                current,
                reservation.OperationClass,
                balance => new ProviderBudgetOperationBalance(
                    balance.OperationClass,
                    balance.AllocationLimit,
                    balance.Committed,
                    new ProviderBudgetUnits(
                        balance.Reserved.Value - reservation.MaximumCharge.Value),
                    balance.Indeterminate));
            return new TransitionShape(
                current.State,
                current.AggregateCommitted,
                new ProviderBudgetUnits(
                    current.AggregateReserved.Value - reservation.MaximumCharge.Value),
                current.AggregateIndeterminate,
                balances,
                ProviderBudgetReservationStatus.ReleasedPreSend,
                "PreSendReleased",
                "PreSendReleased",
                "ReservationReleased",
                release.AuthorityReference,
                DispatchStartedAtUtc: null,
                occurredAtUtc,
                release.ProofSha256.Value,
                OutcomeCode: null,
                "Released");
        }

        var commit = (ProviderBudgetCommitRequest)request;
        var effectiveKind = commit.CommittedUnits.Value > reservation.MaximumCharge.Value
            ? ProviderBudgetCommitmentKind.OverrunMaximum
            : commit.CommitmentKind;

        if (effectiveKind != ProviderBudgetCommitmentKind.Observed &&
            commit.CommittedUnits != reservation.MaximumCharge)
        {
            throw new ArgumentException(
                "An indeterminate or overrun commitment must conservatively commit the admitted maximum.",
                nameof(request));
        }

        var committed = effectiveKind == ProviderBudgetCommitmentKind.OverrunMaximum
            ? reservation.MaximumCharge
            : commit.CommittedUnits;
        var indeterminate = effectiveKind == ProviderBudgetCommitmentKind.IndeterminateMaximum
            ? committed.Value
            : 0;
        var balancesAfterCommit = ReplaceOperationBalance(
            current,
            reservation.OperationClass,
            balance => new ProviderBudgetOperationBalance(
                balance.OperationClass,
                balance.AllocationLimit,
                new ProviderBudgetUnits(checked(balance.Committed.Value + committed.Value)),
                new ProviderBudgetUnits(
                    balance.Reserved.Value - reservation.MaximumCharge.Value),
                new ProviderBudgetUnits(checked(
                    balance.Indeterminate.Value + indeterminate))));
        return effectiveKind switch
        {
            ProviderBudgetCommitmentKind.Observed => new TransitionShape(
                ProviderBudgetState.Armed,
                new ProviderBudgetUnits(checked(current.AggregateCommitted.Value + committed.Value)),
                new ProviderBudgetUnits(current.AggregateReserved.Value - reservation.MaximumCharge.Value),
                current.AggregateIndeterminate,
                balancesAfterCommit,
                ProviderBudgetReservationStatus.Committed,
                "ObservedCommitted",
                "ObservedCommitted",
                "CommitmentRecorded",
                reservation.OperationAuthorityReference,
                reservation.DispatchStartedAtUtc,
                occurredAtUtc,
                commit.UsageEvidenceSha256.Value,
                commit.ProviderOutcomeCode.Value,
                "Committed"),
            ProviderBudgetCommitmentKind.IndeterminateMaximum => new TransitionShape(
                ProviderBudgetState.ReconciliationRequired,
                new ProviderBudgetUnits(checked(current.AggregateCommitted.Value + committed.Value)),
                new ProviderBudgetUnits(current.AggregateReserved.Value - reservation.MaximumCharge.Value),
                new ProviderBudgetUnits(checked(current.AggregateIndeterminate.Value + indeterminate)),
                balancesAfterCommit,
                ProviderBudgetReservationStatus.IndeterminateCommitted,
                "IndeterminateCommitted",
                "IndeterminateCommitted",
                "IndeterminateCommitted",
                reservation.OperationAuthorityReference,
                reservation.DispatchStartedAtUtc,
                occurredAtUtc,
                commit.UsageEvidenceSha256.Value,
                commit.ProviderOutcomeCode.Value,
                "Indeterminate"),
            ProviderBudgetCommitmentKind.OverrunMaximum => new TransitionShape(
                ProviderBudgetState.Tripped,
                new ProviderBudgetUnits(checked(current.AggregateCommitted.Value + committed.Value)),
                new ProviderBudgetUnits(current.AggregateReserved.Value - reservation.MaximumCharge.Value),
                current.AggregateIndeterminate,
                balancesAfterCommit,
                ProviderBudgetReservationStatus.OverrunCommitted,
                "OverrunCommitted",
                "OverrunCommitted",
                "OverrunDetected",
                reservation.OperationAuthorityReference,
                reservation.DispatchStartedAtUtc,
                occurredAtUtc,
                commit.UsageEvidenceSha256.Value,
                commit.ProviderOutcomeCode.Value,
                "Overrun"),
            _ => throw new ArgumentOutOfRangeException(nameof(request)),
        };
    }

    private static async Task AddTerminalEvidenceAsync(
        ControlPlaneDbContext context,
        ProviderBudgetReservation reservation,
        ProviderBudgetEnvelopeV1 next,
        object request,
        TransitionShape transition,
        DateTimeOffset occurredAtUtc,
        CancellationToken cancellationToken)
    {
        if (request is ProviderBudgetCommitRequest commit)
        {
            var kind = transition.ReservationStatus switch
            {
                ProviderBudgetReservationStatus.Committed => ProviderBudgetCommitmentKind.Observed,
                ProviderBudgetReservationStatus.IndeterminateCommitted =>
                    ProviderBudgetCommitmentKind.IndeterminateMaximum,
                ProviderBudgetReservationStatus.OverrunCommitted =>
                    ProviderBudgetCommitmentKind.OverrunMaximum,
                _ => throw new InvalidDataException("A commitment has an invalid terminal state."),
            };
            var committedUnits = kind == ProviderBudgetCommitmentKind.OverrunMaximum
                ? reservation.MaximumCharge.Value
                : commit.CommittedUnits.Value;
            context.ProviderBudgetCommitments.Add(new ProviderBudgetCommitmentRow
            {
                ProviderRequestId = reservation.ProviderRequestId.Value,
                EnvelopeId = reservation.EnvelopeId.Value,
                LedgerRevision = next.LedgerRevision.Value,
                CommitmentKind = kind.ToString(),
                CommittedUnits = committedUnits,
                UsageEvidenceSha256 = commit.UsageEvidenceSha256.Value,
                ProviderOutcomeCode = commit.ProviderOutcomeCode.Value,
                ProviderDurationMilliseconds = commit.ProviderDuration is null
                    ? null
                    : checked((long)commit.ProviderDuration.Value.TotalMilliseconds),
                OccurredAtUtc = FormatUtc(occurredAtUtc),
                CommitmentSha256 = Digest(
                    "provider-budget-commitment-v1",
                    reservation.ProviderRequestId.Value,
                    next.LedgerRevision.Value,
                    kind.ToString(),
                    committedUnits,
                    commit.UsageEvidenceSha256.Value),
            });
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        else if (request is ProviderBudgetReleaseRequest release)
        {
            context.ProviderBudgetReleases.Add(new ProviderBudgetReleaseRow
            {
                ProviderRequestId = reservation.ProviderRequestId.Value,
                EnvelopeId = reservation.EnvelopeId.Value,
                LedgerRevision = next.LedgerRevision.Value,
                ProofKind = release.ProofKind.ToString(),
                ProofSha256 = release.ProofSha256.Value,
                AuthorityReference = release.AuthorityReference.Value,
                OccurredAtUtc = FormatUtc(occurredAtUtc),
                ReleaseSha256 = Digest(
                    "provider-budget-release-v1",
                    reservation.ProviderRequestId.Value,
                    next.LedgerRevision.Value,
                    release.ProofKind.ToString(),
                    release.ProofSha256.Value),
            });
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task<bool?> ReadTransitionReplayAsync(
        ControlPlaneDbContext context,
        object request,
        CancellationToken cancellationToken)
    {
        if (request is ProviderBudgetDispatchRequest dispatch)
        {
            var row = await context.ProviderBudgetReservationTransitions.AsNoTracking()
                .SingleOrDefaultAsync(value =>
                    value.ProviderRequestId == dispatch.ProviderRequestId.Value &&
                    value.TransitionKind == "DispatchStarted",
                    cancellationToken).ConfigureAwait(false);
            return row is null
                ? null
                : row.LedgerRevision == dispatch.ExpectedLedgerRevision.Value + 1 &&
                  row.ReservationRevision == dispatch.ExpectedReservationRevision + 1 &&
                  row.OccurredAtUtc == FormatUtc(dispatch.OccurredAtUtc);
        }

        if (request is ProviderBudgetCommitRequest commit)
        {
            var row = await context.ProviderBudgetCommitments.AsNoTracking()
                .SingleOrDefaultAsync(value =>
                    value.ProviderRequestId == commit.ProviderRequestId.Value,
                    cancellationToken).ConfigureAwait(false);
            return row is null
                ? null
                : row.LedgerRevision == commit.ExpectedLedgerRevision.Value + 1 &&
                  row.CommitmentKind == commit.CommitmentKind.ToString() &&
                  row.CommittedUnits == commit.CommittedUnits.Value &&
                  row.UsageEvidenceSha256 == commit.UsageEvidenceSha256.Value &&
                  row.ProviderOutcomeCode == commit.ProviderOutcomeCode.Value &&
                  row.ProviderDurationMilliseconds == (commit.ProviderDuration is null
                      ? null
                      : checked((long)commit.ProviderDuration.Value.TotalMilliseconds)) &&
                  row.OccurredAtUtc == FormatUtc(commit.OccurredAtUtc);
        }

        var release = (ProviderBudgetReleaseRequest)request;
        var releaseRow = await context.ProviderBudgetReleases.AsNoTracking()
            .SingleOrDefaultAsync(value => value.ProviderRequestId == release.ProviderRequestId.Value,
                cancellationToken).ConfigureAwait(false);
        return releaseRow is null
            ? null
            : releaseRow.LedgerRevision == release.ExpectedLedgerRevision.Value + 1 &&
              releaseRow.ProofKind == release.ProofKind.ToString() &&
              releaseRow.ProofSha256 == release.ProofSha256.Value &&
              releaseRow.AuthorityReference == release.AuthorityReference.Value &&
              releaseRow.OccurredAtUtc == FormatUtc(release.OccurredAtUtc);
    }

    private static ProviderBudgetAdmissionRejection? ValidateAdmission(
        ProviderBudgetEnvelopeV1? envelope,
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
            return ProviderBudgetAdmissionRejection.Expired;
        }

        var stateRejection = envelope.State switch
        {
            ProviderBudgetState.Armed => (ProviderBudgetAdmissionRejection?)null,
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
            return ProviderBudgetAdmissionRejection.AggregateLimitExceeded;
        }

        var operation = envelope.OperationBalances.Single(
            value => value.OperationClass == request.OperationClass);
        return request.MaximumCharge.Value >
            operation.AllocationLimit.Value - operation.Committed.Value - operation.Reserved.Value
            ? ProviderBudgetAdmissionRejection.OperationLimitExceeded
            : null;
    }

    private static ProviderBudgetTransitionRejection? ValidateTransition(
        ProviderBudgetEnvelopeV1 envelope,
        ProviderBudgetReservation reservation,
        ProviderBudgetLedgerRevision expectedLedgerRevision,
        long expectedReservationRevision,
        ProviderBudgetReservationStatus requiredStatus,
        DateTimeOffset occurredAtUtc)
    {
        if (envelope.State != ProviderBudgetState.Armed)
        {
            return ProviderBudgetTransitionRejection.EnvelopeNotArmed;
        }

        if (expectedLedgerRevision != envelope.LedgerRevision)
        {
            return ProviderBudgetTransitionRejection.LedgerRevisionMismatch;
        }

        if (expectedReservationRevision != reservation.CurrentReservationRevision)
        {
            return ProviderBudgetTransitionRejection.ReservationRevisionMismatch;
        }

        if (reservation.Status != requiredStatus)
        {
            return ProviderBudgetTransitionRejection.InvalidReservationState;
        }

        var earliest = reservation.DispatchStartedAtUtc ?? reservation.AdmittedAtUtc;
        return occurredAtUtc < earliest
            ? ProviderBudgetTransitionRejection.InvalidTransitionTime
            : null;
    }

    private static ProviderBudgetTransitionResult Rejected(
        ProviderBudgetTransitionRejection rejection,
        ProviderBudgetEnvelopeV1? envelope,
        ProviderBudgetReservation? reservation) =>
        new(
            ProviderBudgetTransitionOutcome.Rejected,
            envelope?.State ?? ProviderBudgetState.Disarmed,
            envelope?.LedgerRevision,
            reservation,
            rejection);

    private static bool HasIdenticalBinding(
        ProviderBudgetReservationRow row,
        ProviderBudgetAdmissionRequest request) =>
        row.EnvelopeId == request.EnvelopeId.Value &&
        row.StoreEpochId == request.StoreEpochId.Value &&
        row.ConfigurationRevision == request.ExpectedConfigurationRevision.Value &&
        row.AdmittedRuntimeSessionId == request.RuntimeSessionId.Value &&
        row.OperationClass == request.OperationClass.ToString() &&
        row.OperationAuthorityReference == request.OperationAuthorityReference.Value &&
        row.RequestPlanSha256 == request.RequestPlanSha256.Value &&
        row.RequestSha256 == request.RequestSha256.Value &&
        row.MaximumChargeBasisSha256 == request.MaximumChargeBasisSha256.Value &&
        row.CostScheduleSha256 == request.CostScheduleSha256.Value &&
        row.BindingSha256 == request.BindingSha256.Value &&
        row.MaximumChargeUnits == request.MaximumCharge.Value;

    private static string AdmissionConflictSha256(ProviderBudgetAdmissionRequest request) =>
        Digest(
            "provider-budget-admission-conflict-v1",
            request.EnvelopeId.Value,
            request.StoreEpochId.Value,
            request.ExpectedConfigurationRevision.Value,
            request.RuntimeSessionId.Value,
            request.OperationClass.ToString(),
            request.OperationAuthorityReference.Value,
            request.RequestPlanSha256.Value,
            request.RequestSha256.Value,
            request.MaximumChargeBasisSha256.Value,
            request.CostScheduleSha256.Value,
            request.BindingSha256.Value,
            request.MaximumCharge.Value);

    private static string TransitionConflictSha256(object request) => request switch
    {
        ProviderBudgetDispatchRequest dispatch => Digest(
            "provider-budget-dispatch-conflict-v1",
            dispatch.ProviderRequestId.Value,
            dispatch.ExpectedLedgerRevision.Value,
            dispatch.ExpectedReservationRevision),
        ProviderBudgetCommitRequest commit => Digest(
            "provider-budget-commit-conflict-v1",
            commit.ProviderRequestId.Value,
            commit.ExpectedLedgerRevision.Value,
            commit.ExpectedReservationRevision,
            commit.CommitmentKind.ToString(),
            commit.CommittedUnits.Value,
            commit.UsageEvidenceSha256.Value,
            commit.ProviderOutcomeCode.Value,
            commit.ProviderDuration?.Ticks.ToString(CultureInfo.InvariantCulture) ?? "null"),
        ProviderBudgetReleaseRequest release => Digest(
            "provider-budget-release-conflict-v1",
            release.ProviderRequestId.Value,
            release.ExpectedLedgerRevision.Value,
            release.ExpectedReservationRevision,
            release.ProofKind.ToString(),
            release.ProofSha256.Value,
            release.AuthorityReference.Value),
        _ => throw new ArgumentOutOfRangeException(nameof(request)),
    };

    private static async Task<ProviderBudgetEnvelopeV1> PersistExpiredAsync(
        ControlPlaneDbContext context,
        ProviderBudgetEnvelopeV1 current,
        ProviderBudgetAdmissionRequest request,
        CancellationToken cancellationToken)
    {
        var next = await PersistLedgerAsync(
            context,
            current,
            ProviderBudgetState.Expired,
            current.RuntimeSessionId,
            current.AggregateCommitted,
            current.AggregateReserved,
            current.AggregateIndeterminate,
            current.OperationBalances,
            "Expired",
            providerRequestId: null,
            request.OperationAuthorityReference,
            request.RequestedAtUtc,
            cancellationToken).ConfigureAwait(false);
        await AddAuditAsync(
            context,
            next,
            "EnvelopeExpired",
            providerRequestId: null,
            operationClass: null,
            request.OperationAuthorityReference,
            requestSha256: null,
            maximumCharge: null,
            current.State,
            next.State,
            "Expired",
            request.RequestedAtUtc,
            cancellationToken).ConfigureAwait(false);
        await AdvanceEnvelopeAsync(context, next, cancellationToken).ConfigureAwait(false);
        context.ChangeTracker.Clear();
        return await RequireEnvelopeWithinAsync(
            context,
            current.EnvelopeId,
            cancellationToken).ConfigureAwait(false);
    }

    private static async Task AddPreservedStateConflictAuditAsync(
        ControlPlaneDbContext context,
        ProviderBudgetEnvelopeV1 current,
        ProviderBudgetReservationRow existingRow,
        ProviderBudgetAuthorityReference authorityReference,
        DateTimeOffset occurredAtUtc,
        string conflictSha256,
        CancellationToken cancellationToken)
    {
        var auditEventId = $"PBA-{Digest(
            "provider-budget-preserved-state-conflict-audit-id-v2",
            current.EnvelopeId.Value,
            existingRow.ProviderRequestId,
            conflictSha256)}";
        var auditExists = await context.ProviderBudgetAuditEvents.AsNoTracking()
            .AnyAsync(
                row => row.AuditEventId == auditEventId,
                cancellationToken).ConfigureAwait(false);

        if (auditExists)
        {
            return;
        }

        context.ProviderBudgetAuditEvents.Add(new ProviderBudgetAuditEventRow
        {
            AuditEventId = auditEventId,
            EnvelopeId = current.EnvelopeId.Value,
            LedgerRevision = current.LedgerRevision.Value,
            ProviderRequestId = existingRow.ProviderRequestId,
            OperationClass = null,
            EventType = "ReservationConflict",
            AuthorityReference = authorityReference.Value,
            ActorReference = null,
            RequestSha256 = null,
            MaximumChargeUnits = null,
            FromState = current.State.ToString(),
            ToState = current.State.ToString(),
            OutcomeCode = "Conflict",
            OccurredAtUtc = FormatUtc(occurredAtUtc),
            DetailsSha256 = Digest(
                "provider-budget-preserved-state-conflict-audit-v2",
                current.EnvelopeId.Value,
                existingRow.ProviderRequestId,
                conflictSha256),
        });
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<ProviderBudgetEnvelopeV1> PersistStateOnlyAsync(
        ControlPlaneDbContext context,
        ProviderBudgetEnvelopeV1 current,
        ProviderBudgetState state,
        string transitionKind,
        ProviderRequestId? providerRequestId,
        ProviderBudgetAuthorityReference authorityReference,
        DateTimeOffset occurredAtUtc,
        CancellationToken cancellationToken)
    {
        var next = await PersistLedgerAsync(
            context,
            current,
            state,
            current.RuntimeSessionId,
            current.AggregateCommitted,
            current.AggregateReserved,
            current.AggregateIndeterminate,
            current.OperationBalances,
            transitionKind,
            providerRequestId,
            authorityReference,
            occurredAtUtc,
            cancellationToken).ConfigureAwait(false);
        await AddAuditAsync(
            context,
            next,
            "EnvelopeTripped",
            providerRequestId,
            operationClass: null,
            authorityReference,
            requestSha256: null,
            maximumCharge: null,
            current.State,
            next.State,
            "Conflict",
            occurredAtUtc,
            cancellationToken).ConfigureAwait(false);
        await AdvanceEnvelopeAsync(context, next, cancellationToken).ConfigureAwait(false);
        context.ChangeTracker.Clear();
        return await RequireEnvelopeWithinAsync(
            context,
            current.EnvelopeId,
            cancellationToken).ConfigureAwait(false);
    }

    private static async Task<ProviderBudgetEnvelopeV1> PersistLedgerAsync(
        ControlPlaneDbContext context,
        ProviderBudgetEnvelopeV1 current,
        ProviderBudgetState state,
        ProviderRuntimeSessionId? runtimeSessionId,
        ProviderBudgetUnits aggregateCommitted,
        ProviderBudgetUnits aggregateReserved,
        ProviderBudgetUnits aggregateIndeterminate,
        IEnumerable<ProviderBudgetOperationBalance> balances,
        string transitionKind,
        ProviderRequestId? providerRequestId,
        ProviderBudgetAuthorityReference authorityReference,
        DateTimeOffset occurredAtUtc,
        CancellationToken cancellationToken,
        ProviderBudgetRearmRevision? rearmRevision = null)
    {
        var materialisedBalances = balances.OrderBy(value => value.OperationClass).ToArray();
        var nextRevision = new ProviderBudgetLedgerRevision(
            checked(current.LedgerRevision.Value + 1));
        var nextRearm = rearmRevision ?? current.RearmRevision;
        var ledgerSha = Digest(
            "provider-budget-ledger-v1",
            current.EnvelopeId.Value,
            nextRevision.Value,
            current.CurrentLedgerSha256.Value,
            state.ToString(),
            runtimeSessionId?.Value ?? "-",
            aggregateCommitted.Value,
            aggregateReserved.Value,
            aggregateIndeterminate.Value,
            transitionKind,
            providerRequestId?.Value ?? "-",
            occurredAtUtc);
        var ledgerRow = new ProviderBudgetLedgerRevisionRow
        {
            EnvelopeId = current.EnvelopeId.Value,
            LedgerRevision = nextRevision.Value,
            StoreEpochId = current.StoreEpochId.Value,
            PreviousLedgerRevision = current.LedgerRevision.Value,
            ConfigurationRevision = current.ConfigurationRevision.Value,
            RearmRevision = nextRearm.Value,
            State = state.ToString(),
            RuntimeSessionId = runtimeSessionId?.Value,
            AggregateLimitUnits = current.AggregateLimit.Value,
            AggregateCommittedUnits = aggregateCommitted.Value,
            AggregateReservedUnits = aggregateReserved.Value,
            AggregateIndeterminateUnits = aggregateIndeterminate.Value,
            TransitionKind = transitionKind,
            ProviderRequestId = providerRequestId?.Value,
            TransitionAuthorityReference = authorityReference.Value,
            OccurredAtUtc = FormatUtc(occurredAtUtc),
            PreviousLedgerSha256 = current.CurrentLedgerSha256.Value,
            LedgerSha256 = ledgerSha,
            IsComplete = 0,
        };
        context.ProviderBudgetLedgerRevisions.Add(ledgerRow);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        foreach (var balance in materialisedBalances)
        {
            context.ProviderBudgetOperationBalanceRevisions.Add(
                new ProviderBudgetOperationBalanceRevisionRow
                {
                    EnvelopeId = current.EnvelopeId.Value,
                    LedgerRevision = nextRevision.Value,
                    OperationClass = balance.OperationClass.ToString(),
                    ConfigurationRevision = current.ConfigurationRevision.Value,
                    AllocationLimitUnits = balance.AllocationLimit.Value,
                    CommittedUnits = balance.Committed.Value,
                    ReservedUnits = balance.Reserved.Value,
                    IndeterminateUnits = balance.Indeterminate.Value,
                });
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        ledgerRow.IsComplete = 1;
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new ProviderBudgetEnvelopeV1(
            current.EnvelopeId,
            current.StoreEpochId,
            current.Scope,
            current.ConfigurationRevision,
            nextRevision,
            nextRearm,
            state,
            runtimeSessionId,
            current.CostScheduleId,
            current.CostScheduleSha256,
            current.AggregateLimit,
            aggregateCommitted,
            aggregateReserved,
            aggregateIndeterminate,
            materialisedBalances,
            current.EffectiveAtUtc,
            current.ExpiresAtUtc,
            current.IsClosed,
            new ProviderBudgetSha256(ledgerSha));
    }

    private static async Task AddAuditAsync(
        ControlPlaneDbContext context,
        ProviderBudgetEnvelopeV1 next,
        string eventType,
        ProviderRequestId? providerRequestId,
        ProviderBudgetOperationClass? operationClass,
        ProviderBudgetAuthorityReference authorityReference,
        ProviderBudgetSha256? requestSha256,
        ProviderBudgetUnits? maximumCharge,
        ProviderBudgetState fromState,
        ProviderBudgetState toState,
        string outcomeCode,
        DateTimeOffset occurredAtUtc,
        CancellationToken cancellationToken,
        ProviderBudgetAuthorityReference? actorReference = null)
    {
        context.ProviderBudgetAuditEvents.Add(new ProviderBudgetAuditEventRow
        {
            AuditEventId = $"PBA-{Digest(
                "provider-budget-audit-id-v1",
                next.EnvelopeId.Value,
                next.LedgerRevision.Value)}",
            EnvelopeId = next.EnvelopeId.Value,
            LedgerRevision = next.LedgerRevision.Value,
            ProviderRequestId = providerRequestId?.Value,
            OperationClass = operationClass?.ToString(),
            EventType = eventType,
            AuthorityReference = authorityReference.Value,
            ActorReference = actorReference?.Value,
            RequestSha256 = requestSha256?.Value,
            MaximumChargeUnits = maximumCharge?.Value,
            FromState = fromState.ToString(),
            ToState = toState.ToString(),
            OutcomeCode = outcomeCode,
            OccurredAtUtc = FormatUtc(occurredAtUtc),
            DetailsSha256 = Digest(
                "provider-budget-audit-v1",
                next.EnvelopeId.Value,
                next.LedgerRevision.Value,
                eventType,
                outcomeCode,
                occurredAtUtc),
        });
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task AdvanceEnvelopeAsync(
        ControlPlaneDbContext context,
        ProviderBudgetEnvelopeV1 next,
        CancellationToken cancellationToken)
    {
        var row = await context.ProviderBudgetEnvelopes.SingleAsync(
            value => value.EnvelopeId == next.EnvelopeId.Value,
            cancellationToken).ConfigureAwait(false);
        row.CurrentStoreEpochId = next.StoreEpochId.Value;
        row.CurrentConfigurationRevision = next.ConfigurationRevision.Value;
        row.CurrentLedgerRevision = next.LedgerRevision.Value;
        row.CurrentRearmRevision = next.RearmRevision.Value;
        row.State = next.State.ToString();
        row.RuntimeSessionId = next.RuntimeSessionId?.Value;
        row.AggregateLimitUnits = next.AggregateLimit.Value;
        row.AggregateCommittedUnits = next.AggregateCommitted.Value;
        row.AggregateReservedUnits = next.AggregateReserved.Value;
        row.AggregateIndeterminateUnits = next.AggregateIndeterminate.Value;
        row.CurrentLedgerSha256 = next.CurrentLedgerSha256.Value;
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<ProviderBudgetEnvelopeV1?> ReadEnvelopeWithinAsync(
        ControlPlaneDbContext context,
        ProviderBudgetEnvelopeId envelopeId,
        CancellationToken cancellationToken)
    {
        var envelope = await context.ProviderBudgetEnvelopes.AsNoTracking()
            .SingleOrDefaultAsync(
                value => value.EnvelopeId == envelopeId.Value,
                cancellationToken).ConfigureAwait(false);

        if (envelope is null)
        {
            return null;
        }

        if (envelope.IsInitialised != 1)
        {
            throw new InvalidDataException("The provider-budget envelope is not initialised.");
        }

        var headMatches = await context.ProviderBudgetControlHeads.AsNoTracking().AnyAsync(
            value => value.CurrentStoreEpochId == envelope.CurrentStoreEpochId,
            cancellationToken).ConfigureAwait(false);
        var configuration = await context.ProviderBudgetConfigurations.AsNoTracking()
            .SingleOrDefaultAsync(value => value.EnvelopeId == envelope.EnvelopeId &&
                value.ConfigurationRevision == envelope.CurrentConfigurationRevision,
                cancellationToken).ConfigureAwait(false);
        var ledger = await context.ProviderBudgetLedgerRevisions.AsNoTracking()
            .SingleOrDefaultAsync(value => value.EnvelopeId == envelope.EnvelopeId &&
                value.LedgerRevision == envelope.CurrentLedgerRevision,
                cancellationToken).ConfigureAwait(false);
        var balances = await context.ProviderBudgetOperationBalanceRevisions.AsNoTracking()
            .Where(value => value.EnvelopeId == envelope.EnvelopeId &&
                value.LedgerRevision == envelope.CurrentLedgerRevision)
            .OrderBy(value => value.OperationClass)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);

        if (!headMatches || configuration is null || configuration.SealedAtUtc is null ||
            ledger is null || ledger.IsComplete != 1 || balances.Length != 3 ||
            ledger.StoreEpochId != envelope.CurrentStoreEpochId ||
            ledger.ConfigurationRevision != envelope.CurrentConfigurationRevision ||
            ledger.RearmRevision != envelope.CurrentRearmRevision ||
            ledger.State != envelope.State ||
            ledger.RuntimeSessionId != envelope.RuntimeSessionId ||
            ledger.AggregateLimitUnits != envelope.AggregateLimitUnits ||
            ledger.AggregateCommittedUnits != envelope.AggregateCommittedUnits ||
            ledger.AggregateReservedUnits != envelope.AggregateReservedUnits ||
            ledger.AggregateIndeterminateUnits != envelope.AggregateIndeterminateUnits ||
            ledger.LedgerSha256 != envelope.CurrentLedgerSha256 ||
            configuration.AggregateLimitUnits != envelope.AggregateLimitUnits ||
            balances.Any(value =>
                value.ConfigurationRevision != envelope.CurrentConfigurationRevision))
        {
            throw new InvalidDataException(
                "The persisted provider-budget graph is incomplete or internally inconsistent.");
        }

        return new ProviderBudgetEnvelopeV1(
            new ProviderBudgetEnvelopeId(envelope.EnvelopeId),
            new ProviderBudgetStoreEpochId(envelope.CurrentStoreEpochId),
            new ProviderBudgetScope(
                new ProviderBudgetEnvironmentId(envelope.EnvironmentId),
                new ProviderBudgetProviderId(envelope.ProviderId),
                new ProviderBudgetBillingScopeReference(envelope.BillingScopeReference),
                new ProviderBudgetModelId(envelope.ModelId),
                new ProviderBudgetCurrencyCode(envelope.CurrencyCode),
                new ProviderBudgetAccountingUnitId(envelope.AccountingUnitId)),
            new ProviderBudgetConfigurationRevision(envelope.CurrentConfigurationRevision),
            new ProviderBudgetLedgerRevision(envelope.CurrentLedgerRevision),
            new ProviderBudgetRearmRevision(envelope.CurrentRearmRevision),
            ParseEnum<ProviderBudgetState>(envelope.State),
            envelope.RuntimeSessionId is null
                ? null
                : new ProviderRuntimeSessionId(envelope.RuntimeSessionId),
            new ProviderBudgetCostScheduleId(configuration.CostScheduleId),
            new ProviderBudgetSha256(configuration.CostScheduleSha256),
            new ProviderBudgetUnits(envelope.AggregateLimitUnits),
            new ProviderBudgetUnits(envelope.AggregateCommittedUnits),
            new ProviderBudgetUnits(envelope.AggregateReservedUnits),
            new ProviderBudgetUnits(envelope.AggregateIndeterminateUnits),
            balances.Select(ToDomain),
            ParseUtc(configuration.EffectiveAtUtc),
            ParseUtc(configuration.ExpiresAtUtc),
            envelope.IsClosed == 1,
            new ProviderBudgetSha256(envelope.CurrentLedgerSha256));
    }

    private static async Task<ProviderBudgetEnvelopeV1> RequireEnvelopeWithinAsync(
        ControlPlaneDbContext context,
        ProviderBudgetEnvelopeId envelopeId,
        CancellationToken cancellationToken) =>
        await ReadEnvelopeWithinAsync(context, envelopeId, cancellationToken).ConfigureAwait(false) ??
        throw new InvalidDataException("The provider-budget envelope disappeared during its transaction.");

    private static async Task<ProviderBudgetReservation?> ReadReservationWithinAsync(
        ControlPlaneDbContext context,
        ProviderRequestId providerRequestId,
        CancellationToken cancellationToken)
    {
        var row = await context.ProviderBudgetReservations.AsNoTracking()
            .SingleOrDefaultAsync(
                value => value.ProviderRequestId == providerRequestId.Value,
                cancellationToken).ConfigureAwait(false);
        return row is null ? null : ToDomain(row);
    }

    private static async Task<ProviderBudgetReservation> RequireReservationWithinAsync(
        ControlPlaneDbContext context,
        ProviderRequestId providerRequestId,
        CancellationToken cancellationToken) =>
        await ReadReservationWithinAsync(context, providerRequestId, cancellationToken)
            .ConfigureAwait(false) ??
        throw new InvalidDataException("The provider-budget reservation disappeared during its transaction.");

    private static ProviderBudgetReservation ToDomain(ProviderBudgetReservationRow row)
    {
        if (row.IsInitialised != 1)
        {
            throw new InvalidDataException("The provider-budget reservation is not initialised.");
        }

        return new ProviderBudgetReservation(
            new ProviderRequestId(row.ProviderRequestId),
            new ProviderBudgetEnvelopeId(row.EnvelopeId),
            new ProviderBudgetStoreEpochId(row.StoreEpochId),
            new ProviderBudgetConfigurationRevision(row.ConfigurationRevision),
            ParseEnum<ProviderBudgetOperationClass>(row.OperationClass),
            new ProviderBudgetAuthorityReference(row.OperationAuthorityReference),
            new ProviderBudgetSha256(row.RequestPlanSha256),
            new ProviderBudgetSha256(row.RequestSha256),
            new ProviderBudgetSha256(row.MaximumChargeBasisSha256),
            new ProviderBudgetSha256(row.CostScheduleSha256),
            new ProviderBudgetSha256(row.BindingSha256),
            new ProviderBudgetUnits(row.MaximumChargeUnits),
            new ProviderRuntimeSessionId(row.AdmittedRuntimeSessionId),
            new ProviderBudgetLedgerRevision(row.AdmissionLedgerRevision),
            row.CurrentReservationRevision,
            ParseEnum<ProviderBudgetReservationStatus>(row.Status),
            ParseUtc(row.AdmittedAtUtc),
            row.DispatchStartedAtUtc is null ? null : ParseUtc(row.DispatchStartedAtUtc),
            row.TerminalAtUtc is null ? null : ParseUtc(row.TerminalAtUtc),
            row.TerminalLedgerRevision is null
                ? null
                : new ProviderBudgetLedgerRevision(row.TerminalLedgerRevision.Value));
    }

    private static ProviderBudgetOperationBalance ToDomain(
        ProviderBudgetOperationBalanceRevisionRow row) =>
        new(
            ParseEnum<ProviderBudgetOperationClass>(row.OperationClass),
            new ProviderBudgetUnits(row.AllocationLimitUnits),
            new ProviderBudgetUnits(row.CommittedUnits),
            new ProviderBudgetUnits(row.ReservedUnits),
            new ProviderBudgetUnits(row.IndeterminateUnits));

    private static ProviderBudgetOperationBalance[] ReplaceOperationBalance(
        ProviderBudgetEnvelopeV1 current,
        ProviderBudgetOperationClass operationClass,
        Func<ProviderBudgetOperationBalance, ProviderBudgetOperationBalance> replace) =>
        current.OperationBalances.Select(balance => balance.OperationClass == operationClass
            ? replace(balance)
            : balance).ToArray();

    private static bool IsZeroBudget(ProviderBudgetEnvelopeV1 envelope) =>
        envelope.AggregateLimit.Value == 0 &&
        envelope.AggregateCommitted.Value == 0 &&
        envelope.AggregateReserved.Value == 0 &&
        envelope.AggregateIndeterminate.Value == 0 &&
        envelope.OperationBalances.All(balance =>
            balance.AllocationLimit.Value == 0 &&
            balance.Committed.Value == 0 &&
            balance.Reserved.Value == 0 &&
            balance.Indeterminate.Value == 0);

    private static bool IsUnusedNonZeroBudget(ProviderBudgetEnvelopeV1 envelope) =>
        envelope.AggregateLimit.Value > 0 &&
        envelope.AggregateCommitted.Value == 0 &&
        envelope.AggregateReserved.Value == 0 &&
        envelope.AggregateIndeterminate.Value == 0 &&
        envelope.OperationBalances.Any(balance => balance.AllocationLimit.Value > 0) &&
        envelope.OperationBalances.All(balance =>
            balance.Committed.Value == 0 &&
            balance.Reserved.Value == 0 &&
            balance.Indeterminate.Value == 0);

    private static TEnum ParseEnum<TEnum>(string value)
        where TEnum : struct, Enum =>
        Enum.TryParse<TEnum>(value, ignoreCase: false, out var parsed) && Enum.IsDefined(parsed)
            ? parsed
            : throw new InvalidDataException("A persisted provider-budget enum value is invalid.");

    private static string OperationBalancesDigest(ProviderBudgetEnvelopeV1 envelope) =>
        Digest(
            "provider-budget-operation-balances-v1",
            envelope.OperationBalances.OrderBy(value => value.OperationClass).Select(value =>
                string.Join(
                    ':',
                    value.OperationClass,
                    value.AllocationLimit.Value.ToString(CultureInfo.InvariantCulture),
                    value.Committed.Value.ToString(CultureInfo.InvariantCulture),
                    value.Reserved.Value.ToString(CultureInfo.InvariantCulture),
                    value.Indeterminate.Value.ToString(CultureInfo.InvariantCulture))).ToArray());

    private static async Task<SqliteTransaction> BeginImmediateAsync(
        ControlPlaneDbContext context,
        CancellationToken cancellationToken)
    {
        await context.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        var connection = (SqliteConnection)context.Database.GetDbConnection();
        var transaction = connection.BeginTransaction(deferred: false);
        context.Database.UseTransaction(transaction);
        return transaction;
    }

    private static string FormatUtc(DateTimeOffset value) =>
        value.ToString("O", CultureInfo.InvariantCulture);

    private static DateTimeOffset ParseUtc(string value) =>
        DateTimeOffset.ParseExact(
            value,
            "O",
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind);

    private static string Digest(string domain, params object[] values)
    {
        var canonical = string.Join(
            '\n',
            new[] { domain }.Concat(values.Select(value => value switch
            {
                DateTimeOffset instant => FormatUtc(instant),
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString(),
            })));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)))
            .ToLowerInvariant();
    }

    private sealed record PersistedTransition(
        ProviderBudgetEnvelopeV1 Envelope,
        ProviderBudgetReservation Reservation);

    private sealed record TransitionShape(
        ProviderBudgetState State,
        ProviderBudgetUnits AggregateCommitted,
        ProviderBudgetUnits AggregateReserved,
        ProviderBudgetUnits AggregateIndeterminate,
        IReadOnlyCollection<ProviderBudgetOperationBalance> Balances,
        ProviderBudgetReservationStatus ReservationStatus,
        string LedgerTransitionKind,
        string ReservationTransitionKind,
        string AuditEventType,
        ProviderBudgetAuthorityReference AuthorityReference,
        DateTimeOffset? DispatchStartedAtUtc,
        DateTimeOffset? TerminalAtUtc,
        string? ProofSha256,
        string? OutcomeCode,
        string AuditOutcome);
}
