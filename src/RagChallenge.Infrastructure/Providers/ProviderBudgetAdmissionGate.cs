// Purpose: Enforces durable zero-budget admission and exact authority revalidation before provider credentials or synthetic transport can be reached; pricing and non-zero operational schedules remain out of scope.
using System.Security.Cryptography;
using System.Text;

using RagChallenge.Application.ProviderBudget;

namespace RagChallenge.Infrastructure.Providers;

public sealed class ProviderBudgetAdmissionContext
{
    public ProviderBudgetAdmissionContext(
        ProviderBudgetEnvelopeId envelopeId,
        ProviderRuntimeSessionId runtimeSessionId,
        ProviderBudgetAuthorityReference operationAuthorityReference,
        Func<ProviderRequestId>? providerRequestIdFactory = null)
    {
        EnvelopeId = envelopeId ?? throw new ArgumentNullException(nameof(envelopeId));
        RuntimeSessionId = runtimeSessionId ??
            throw new ArgumentNullException(nameof(runtimeSessionId));
        OperationAuthorityReference = operationAuthorityReference ??
            throw new ArgumentNullException(nameof(operationAuthorityReference));
        ProviderRequestIdFactory = providerRequestIdFactory ??
            (() => new ProviderRequestId($"PBR-{Guid.NewGuid():N}"));
    }

    public ProviderBudgetEnvelopeId EnvelopeId { get; }
    public ProviderRuntimeSessionId RuntimeSessionId { get; }
    public ProviderBudgetAuthorityReference OperationAuthorityReference { get; }
    internal Func<ProviderRequestId> ProviderRequestIdFactory { get; }
}

public sealed class ProviderBudgetAdmissionUnavailableException : InvalidOperationException
{
    public ProviderBudgetAdmissionUnavailableException()
        : base("Provider-budget admission is unavailable before credential lookup.")
    {
    }
}

public sealed class ProviderBudgetAdmissionGate
{
    private readonly IProviderBudgetLedger ledger;
    private readonly ProviderBudgetAdmissionContext context;
    private readonly Func<CancellationToken, ValueTask> revalidateAuthority;
    private readonly TimeProvider timeProvider;

    public ProviderBudgetAdmissionGate(
        IProviderBudgetLedger ledger,
        ProviderBudgetAdmissionContext context,
        Func<CancellationToken, ValueTask> revalidateAuthority,
        TimeProvider? timeProvider = null)
    {
        this.ledger = ledger ?? throw new ArgumentNullException(nameof(ledger));
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.revalidateAuthority = revalidateAuthority ??
            throw new ArgumentNullException(nameof(revalidateAuthority));
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<ProviderBudgetAdmissionLease> AdmitAsync(
        ProviderBudgetOperationClass operationClass,
        ReadOnlyMemory<byte> exactRequestBytes,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(operationClass))
        {
            throw new ArgumentOutOfRangeException(nameof(operationClass));
        }

        await revalidateAuthority(cancellationToken).ConfigureAwait(false);
        var envelope = await ledger.ReadEnvelopeAsync(context.EnvelopeId, cancellationToken)
            .ConfigureAwait(false);

        if (envelope is null || envelope.AggregateLimit.Value != 0 ||
            envelope.OperationBalances.Any(balance => balance.AllocationLimit.Value != 0))
        {
            throw new ProviderBudgetAdmissionUnavailableException();
        }

        var requestedAtUtc = timeProvider.GetUtcNow();
        var requestSha = Sha256(exactRequestBytes.Span);
        var planSha = Sha256("provider-budget-zero-request-plan-v1", requestSha);
        var maximumBasisSha = Sha256("provider-budget-zero-maximum-v1", operationClass, requestSha);
        var bindingSha = Sha256(
            "provider-budget-admission-binding-v1",
            envelope.EnvelopeId.Value,
            envelope.StoreEpochId.Value,
            envelope.ConfigurationRevision.Value,
            envelope.LedgerRevision.Value,
            context.RuntimeSessionId.Value,
            operationClass,
            context.OperationAuthorityReference.Value,
            requestSha,
            maximumBasisSha);
        var request = new ProviderBudgetAdmissionRequest(
            context.ProviderRequestIdFactory(),
            envelope.EnvelopeId,
            envelope.StoreEpochId,
            envelope.ConfigurationRevision,
            envelope.LedgerRevision,
            context.RuntimeSessionId,
            envelope.Scope,
            envelope.CostScheduleId,
            envelope.CostScheduleSha256,
            operationClass,
            context.OperationAuthorityReference,
            new ProviderBudgetSha256(planSha),
            new ProviderBudgetSha256(requestSha),
            new ProviderBudgetSha256(maximumBasisSha),
            new ProviderBudgetSha256(bindingSha),
            new ProviderBudgetUnits(0),
            requestedAtUtc);
        var admission = await ledger.AdmitAsync(request, cancellationToken).ConfigureAwait(false);

        if (admission.Outcome != ProviderBudgetAdmissionOutcome.Admitted ||
            admission.Reservation is null || admission.CurrentLedgerRevision is null ||
            admission.Reservation.Status != ProviderBudgetReservationStatus.Reserved)
        {
            throw new ProviderBudgetAdmissionUnavailableException();
        }

        var readback = await ledger.ReadReservationAsync(
            admission.Reservation.ProviderRequestId,
            cancellationToken).ConfigureAwait(false);

        if (readback is null || !HasExactReadback(admission.Reservation, readback))
        {
            throw new ProviderBudgetAdmissionUnavailableException();
        }

        var lease = new ProviderBudgetAdmissionLease(
            ledger,
            admission.Reservation,
            admission.CurrentLedgerRevision,
            context.OperationAuthorityReference,
            timeProvider);

        try
        {
            await revalidateAuthority(cancellationToken).ConfigureAwait(false);
            return lease;
        }
        catch
        {
            await lease.ReleaseBeforeCredentialLookupAsync(CancellationToken.None)
                .ConfigureAwait(false);
            throw;
        }
    }

    private static string Sha256(ReadOnlySpan<byte> bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static bool HasExactReadback(
        ProviderBudgetReservation expected,
        ProviderBudgetReservation actual) =>
        expected.ProviderRequestId == actual.ProviderRequestId &&
        expected.EnvelopeId == actual.EnvelopeId &&
        expected.StoreEpochId == actual.StoreEpochId &&
        expected.ConfigurationRevision == actual.ConfigurationRevision &&
        expected.OperationClass == actual.OperationClass &&
        expected.OperationAuthorityReference == actual.OperationAuthorityReference &&
        expected.RequestPlanSha256 == actual.RequestPlanSha256 &&
        expected.RequestSha256 == actual.RequestSha256 &&
        expected.MaximumChargeBasisSha256 == actual.MaximumChargeBasisSha256 &&
        expected.CostScheduleSha256 == actual.CostScheduleSha256 &&
        expected.BindingSha256 == actual.BindingSha256 &&
        expected.MaximumCharge == actual.MaximumCharge &&
        expected.AdmittedRuntimeSessionId == actual.AdmittedRuntimeSessionId &&
        expected.AdmissionLedgerRevision == actual.AdmissionLedgerRevision &&
        expected.CurrentReservationRevision == actual.CurrentReservationRevision &&
        expected.Status == actual.Status &&
        expected.AdmittedAtUtc == actual.AdmittedAtUtc &&
        expected.DispatchStartedAtUtc == actual.DispatchStartedAtUtc &&
        expected.TerminalAtUtc == actual.TerminalAtUtc &&
        expected.TerminalLedgerRevision == actual.TerminalLedgerRevision;

    private static string Sha256(string domain, params object[] values) =>
        Sha256(Encoding.UTF8.GetBytes(string.Join('\n', new[] { domain }.Concat(values))));
}

public sealed class ProviderBudgetAdmissionLease
{
    private readonly IProviderBudgetLedger ledger;
    private readonly ProviderBudgetAuthorityReference authorityReference;
    private readonly TimeProvider timeProvider;
    private ProviderBudgetReservation reservation;
    private ProviderBudgetLedgerRevision currentLedgerRevision;

    internal ProviderBudgetAdmissionLease(
        IProviderBudgetLedger ledger,
        ProviderBudgetReservation reservation,
        ProviderBudgetLedgerRevision currentLedgerRevision,
        ProviderBudgetAuthorityReference authorityReference,
        TimeProvider timeProvider)
    {
        this.ledger = ledger;
        this.reservation = reservation;
        this.currentLedgerRevision = currentLedgerRevision;
        this.authorityReference = authorityReference;
        this.timeProvider = timeProvider;
    }

    public bool DispatchStarted =>
        reservation.Status == ProviderBudgetReservationStatus.DispatchStarted;

    public bool IsTerminal => reservation.Status is
        ProviderBudgetReservationStatus.Committed or
        ProviderBudgetReservationStatus.ReleasedPreSend or
        ProviderBudgetReservationStatus.IndeterminateCommitted or
        ProviderBudgetReservationStatus.OverrunCommitted;

    public async Task MarkDispatchStartedAsync(CancellationToken cancellationToken)
    {
        var result = await ledger.MarkDispatchStartedAsync(
            new ProviderBudgetDispatchRequest(
                reservation.ProviderRequestId,
                currentLedgerRevision,
                reservation.CurrentReservationRevision,
                timeProvider.GetUtcNow()),
            cancellationToken).ConfigureAwait(false);
        Apply(result);
    }

    public Task ReleaseBeforeCredentialLookupAsync(CancellationToken cancellationToken) =>
        ReleaseAsync(ProviderBudgetReleaseProofKind.BeforeCredentialLookup, cancellationToken);

    public Task ReleaseConfirmedZeroRequestBytesAsync(CancellationToken cancellationToken) =>
        ReleaseAsync(
            ProviderBudgetReleaseProofKind.TransportConfirmedZeroRequestBytes,
            cancellationToken);

    public async Task CommitObservedZeroAsync(
        string outcomeCode,
        TimeSpan? duration,
        CancellationToken cancellationToken)
    {
        var result = await ledger.CommitAsync(
            new ProviderBudgetCommitRequest(
                reservation.ProviderRequestId,
                currentLedgerRevision,
                reservation.CurrentReservationRevision,
                ProviderBudgetCommitmentKind.Observed,
                new ProviderBudgetUnits(0),
                new ProviderBudgetSha256(Sha256(
                    "provider-budget-observed-zero-v1",
                    reservation.ProviderRequestId.Value,
                    outcomeCode)),
                new ProviderBudgetOutcomeCode(outcomeCode),
                duration,
                timeProvider.GetUtcNow()),
            cancellationToken).ConfigureAwait(false);
        Apply(result);
    }

    public async Task CommitIndeterminateZeroAsync(
        string outcomeCode,
        CancellationToken cancellationToken)
    {
        var result = await ledger.CommitAsync(
            new ProviderBudgetCommitRequest(
                reservation.ProviderRequestId,
                currentLedgerRevision,
                reservation.CurrentReservationRevision,
                ProviderBudgetCommitmentKind.IndeterminateMaximum,
                new ProviderBudgetUnits(0),
                new ProviderBudgetSha256(Sha256(
                    "provider-budget-indeterminate-zero-v1",
                    reservation.ProviderRequestId.Value,
                    outcomeCode)),
                new ProviderBudgetOutcomeCode(outcomeCode),
                providerDuration: null,
                timeProvider.GetUtcNow()),
            cancellationToken).ConfigureAwait(false);
        Apply(result);
    }

    private async Task ReleaseAsync(
        ProviderBudgetReleaseProofKind proofKind,
        CancellationToken cancellationToken)
    {
        if (IsTerminal)
        {
            return;
        }

        var proofSha = new ProviderBudgetSha256(Sha256(
            "provider-budget-release-proof-v1",
            reservation.ProviderRequestId.Value,
            proofKind));
        var result = await ledger.ReleasePreSendAsync(
            new ProviderBudgetReleaseRequest(
                reservation.ProviderRequestId,
                currentLedgerRevision,
                reservation.CurrentReservationRevision,
                proofKind,
                proofSha,
                authorityReference,
                timeProvider.GetUtcNow()),
            cancellationToken).ConfigureAwait(false);
        Apply(result);
    }

    private void Apply(ProviderBudgetTransitionResult result)
    {
        if (result.Outcome != ProviderBudgetTransitionOutcome.Applied ||
            result.Reservation is null || result.CurrentLedgerRevision is null)
        {
            throw new ProviderBudgetAdmissionUnavailableException();
        }

        reservation = result.Reservation;
        currentLedgerRevision = result.CurrentLedgerRevision;
    }

    private static string Sha256(string domain, params object[] values) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            string.Join('\n', new[] { domain }.Concat(values)))))
            .ToLowerInvariant();
}
