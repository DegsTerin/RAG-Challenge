// Purpose: Verifies focused local SQLite admission, terminal recovery and explicit rearming without provider credentials, network or non-zero schedules.
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using RagChallenge.Application.ProviderBudget;
using RagChallenge.Infrastructure.Persistence;

namespace RagChallenge.IntegrationTests;

public sealed class SqliteProviderBudgetLedgerTests
{
    private const string EnvelopeId = "budget-envelope-item-3";
    private const string StoreEpochId = "budget-epoch-item-3";
    private const string ConfigurationSha256 =
        "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string InitialLedgerSha256 =
        "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
    private const string ZeroSha256 =
        "0000000000000000000000000000000000000000000000000000000000000000";

    [Fact]
    public async Task ExplicitRearmAdmissionReplayDispatchAndCommitAreDurable()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var instant = DateTimeOffset.UtcNow;
        await SeedDisarmedGraphAsync(fixture.Options.ControlDatabasePath, instant);
        var ledger = new SqliteProviderBudgetLedger(fixture.Options);
        var disarmed = Assert.IsType<ProviderBudgetEnvelopeV1>(
            await ledger.ReadEnvelopeAsync(new ProviderBudgetEnvelopeId(EnvelopeId)));
        Assert.Equal(ProviderBudgetState.Disarmed, disarmed.State);

        var rearm = await ledger.RearmAsync(CreateRearmRequest(disarmed, instant));

        Assert.Equal(ProviderBudgetRearmOutcome.Applied, rearm.Outcome);
        var armed = Assert.IsType<ProviderBudgetEnvelopeV1>(rearm.Envelope);
        Assert.Equal(ProviderBudgetState.Armed, armed.State);
        Assert.Equal(2, armed.LedgerRevision.Value);
        Assert.Equal(1, armed.RearmRevision.Value);
        var admissionRequest = CreateAdmissionRequest(armed, "PBR-ITEM-3-001", instant.AddSeconds(1));

        var admitted = await ledger.AdmitAsync(admissionRequest);
        var replay = await ledger.AdmitAsync(admissionRequest);

        Assert.Equal(ProviderBudgetAdmissionOutcome.Admitted, admitted.Outcome);
        Assert.Equal(ProviderBudgetAdmissionOutcome.Replay, replay.Outcome);
        Assert.Equal(3, admitted.CurrentLedgerRevision!.Value);
        var reservation = Assert.IsType<ProviderBudgetReservation>(admitted.Reservation);
        Assert.Equal(ProviderBudgetReservationStatus.Reserved, reservation.Status);

        var durableLedger = new SqliteProviderBudgetLedger(fixture.Options);
        var durableReservation = Assert.IsType<ProviderBudgetReservation>(
            await durableLedger.ReadReservationAsync(reservation.ProviderRequestId));
        Assert.Equal(reservation.ProviderRequestId, durableReservation.ProviderRequestId);
        Assert.Equal(reservation.BindingSha256, durableReservation.BindingSha256);
        Assert.Equal(reservation.Status, durableReservation.Status);
        var dispatch = await durableLedger.MarkDispatchStartedAsync(
            new ProviderBudgetDispatchRequest(
                reservation.ProviderRequestId,
                admitted.CurrentLedgerRevision,
                reservation.CurrentReservationRevision,
                instant.AddSeconds(2)));
        var dispatched = Assert.IsType<ProviderBudgetReservation>(dispatch.Reservation);
        Assert.Equal(ProviderBudgetTransitionOutcome.Applied, dispatch.Outcome);
        Assert.Equal(ProviderBudgetReservationStatus.DispatchStarted, dispatched.Status);

        var commitment = await durableLedger.CommitAsync(
            new ProviderBudgetCommitRequest(
                reservation.ProviderRequestId,
                dispatch.CurrentLedgerRevision!,
                dispatched.CurrentReservationRevision,
                ProviderBudgetCommitmentKind.Observed,
                new ProviderBudgetUnits(0),
                Sha("observed zero"),
                new ProviderBudgetOutcomeCode("SYNTHETIC_OK"),
                TimeSpan.FromMilliseconds(1),
                instant.AddSeconds(3)));

        Assert.Equal(ProviderBudgetTransitionOutcome.Applied, commitment.Outcome);
        Assert.Equal(
            ProviderBudgetReservationStatus.Committed,
            commitment.Reservation!.Status);
        Assert.Equal(5, commitment.CurrentLedgerRevision!.Value);
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_commitments;"));
        var afterCommit = (await durableLedger.ReadEnvelopeAsync(armed.EnvelopeId))!;
        Assert.Equal(0, afterCommit.AggregateCommitted.Value);
        var releasableAdmission = await durableLedger.AdmitAsync(
            CreateAdmissionRequest(afterCommit, "PBR-ITEM-3-RELEASE", instant.AddSeconds(4)));
        var releasable = Assert.IsType<ProviderBudgetReservation>(
            releasableAdmission.Reservation);
        var release = await durableLedger.ReleasePreSendAsync(
            new ProviderBudgetReleaseRequest(
                releasable.ProviderRequestId,
                releasableAdmission.CurrentLedgerRevision!,
                releasable.CurrentReservationRevision,
                ProviderBudgetReleaseProofKind.BeforeCredentialLookup,
                Sha("pre-credential release proof"),
                releasable.OperationAuthorityReference,
                instant.AddSeconds(5)));
        Assert.Equal(ProviderBudgetTransitionOutcome.Applied, release.Outcome);
        Assert.Equal(
            ProviderBudgetReservationStatus.ReleasedPreSend,
            release.Reservation!.Status);
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_releases;"));
    }

    [Fact]
    public async Task DivergentRequestIdentityTripsWithoutCreatingASecondReservation()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var instant = DateTimeOffset.UtcNow;
        await SeedDisarmedGraphAsync(fixture.Options.ControlDatabasePath, instant);
        var ledger = new SqliteProviderBudgetLedger(fixture.Options);
        var disarmed = (await ledger.ReadEnvelopeAsync(new ProviderBudgetEnvelopeId(EnvelopeId)))!;
        var armed = (await ledger.RearmAsync(CreateRearmRequest(disarmed, instant))).Envelope!;
        var original = CreateAdmissionRequest(armed, "PBR-ITEM-3-CONFLICT", instant.AddSeconds(1));
        _ = await ledger.AdmitAsync(original);
        var divergent = new ProviderBudgetAdmissionRequest(
            original.ProviderRequestId,
            original.EnvelopeId,
            original.StoreEpochId,
            original.ExpectedConfigurationRevision,
            original.ExpectedLedgerRevision,
            original.RuntimeSessionId,
            original.Scope,
            original.CostScheduleId,
            original.CostScheduleSha256,
            original.OperationClass,
            original.OperationAuthorityReference,
            original.RequestPlanSha256,
            Sha("different exact request"),
            original.MaximumChargeBasisSha256,
            original.BindingSha256,
            original.MaximumCharge,
            original.RequestedAtUtc);

        var conflict = await ledger.AdmitAsync(divergent);

        Assert.Equal(ProviderBudgetAdmissionOutcome.Conflict, conflict.Outcome);
        Assert.Equal(ProviderBudgetState.Tripped, conflict.State);
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_reservations;"));
    }

    [Fact]
    public async Task RearmRequiresExactConfigurationAndBalanceAcknowledgement()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var instant = DateTimeOffset.UtcNow;
        await SeedDisarmedGraphAsync(fixture.Options.ControlDatabasePath, instant);
        var ledger = new SqliteProviderBudgetLedger(fixture.Options);
        var disarmed = (await ledger.ReadEnvelopeAsync(new ProviderBudgetEnvelopeId(EnvelopeId)))!;
        var request = CreateRearmRequest(disarmed, instant);
        var conflicting = new ProviderBudgetRearmRequest(
            request.EnvelopeId,
            request.ExpectedStoreEpochId,
            request.ExpectedConfigurationRevision,
            request.ExpectedLedgerRevision,
            request.ExpectedRearmRevision,
            request.NewRuntimeSessionId,
            request.AuthorityReference,
            request.ActorReference,
            request.ReasonSha256,
            request.AcknowledgedCommitted,
            request.AcknowledgedReserved,
            request.AcknowledgedIndeterminate,
            Sha("wrong balances"),
            request.ConfigurationSha256,
            request.OccurredAtUtc);

        var result = await ledger.RearmAsync(conflicting);

        Assert.Equal(ProviderBudgetRearmOutcome.Conflict, result.Outcome);
        Assert.Equal(ProviderBudgetState.Disarmed, result.Envelope!.State);
        Assert.Equal(0, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_rearms;"));
    }

    [Fact]
    public async Task ExpiredAdmissionPersistsTerminalStateAcrossRestart()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var instant = DateTimeOffset.UtcNow;
        await SeedDisarmedGraphAsync(fixture.Options.ControlDatabasePath, instant);
        var ledger = new SqliteProviderBudgetLedger(fixture.Options);
        var disarmed = (await ledger.ReadEnvelopeAsync(new ProviderBudgetEnvelopeId(EnvelopeId)))!;
        var armed = (await ledger.RearmAsync(CreateRearmRequest(disarmed, instant))).Envelope!;
        var expiredRequest = CreateAdmissionRequest(
            armed,
            "PBR-RECOVERY-EXPIRED",
            instant.AddHours(2));

        var expired = await ledger.AdmitAsync(expiredRequest);

        Assert.Equal(ProviderBudgetAdmissionOutcome.Rejected, expired.Outcome);
        Assert.Equal(ProviderBudgetAdmissionRejection.Expired, expired.Rejection);
        Assert.Equal(ProviderBudgetState.Expired, expired.State);
        var expiredRevision = Assert.IsType<ProviderBudgetLedgerRevision>(
            expired.CurrentLedgerRevision);
        Assert.Equal(armed.LedgerRevision.Value + 1, expiredRevision.Value);
        var restartedLedger = new SqliteProviderBudgetLedger(fixture.Options);
        var durable = Assert.IsType<ProviderBudgetEnvelopeV1>(
            await restartedLedger.ReadEnvelopeAsync(armed.EnvelopeId));
        Assert.Equal(ProviderBudgetState.Expired, durable.State);
        Assert.Equal(expiredRevision, durable.LedgerRevision);
        Assert.Equal(0, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_reservations;"));
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_ledger_revisions " +
            "WHERE state = 'Expired' AND transition_kind = 'Expired' AND is_complete = 1;"));
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_audit_events " +
            "WHERE event_type = 'EnvelopeExpired' AND from_state = 'Armed' " +
            "AND to_state = 'Expired' AND outcome_code = 'Expired';"));

        var repeated = await restartedLedger.AdmitAsync(expiredRequest);

        Assert.Equal(ProviderBudgetAdmissionOutcome.Rejected, repeated.Outcome);
        Assert.Equal(ProviderBudgetAdmissionRejection.Expired, repeated.Rejection);
        Assert.Equal(expiredRevision, repeated.CurrentLedgerRevision);
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_ledger_revisions " +
            "WHERE state = 'Expired' AND transition_kind = 'Expired';"));
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_audit_events " +
            "WHERE event_type = 'EnvelopeExpired';"));
    }

    [Fact]
    public async Task ReservationReplayAfterExpiryPersistsExpiredAndBlocksNewAdmission()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var instant = DateTimeOffset.UtcNow;
        await SeedDisarmedGraphAsync(fixture.Options.ControlDatabasePath, instant);
        var ledger = new SqliteProviderBudgetLedger(fixture.Options);
        var disarmed = (await ledger.ReadEnvelopeAsync(new ProviderBudgetEnvelopeId(EnvelopeId)))!;
        var armed = (await ledger.RearmAsync(CreateRearmRequest(disarmed, instant))).Envelope!;
        var original = CreateAdmissionRequest(
            armed,
            "PBR-RECOVERY-EXPIRED-REPLAY",
            instant.AddSeconds(1));
        var admitted = await ledger.AdmitAsync(original);
        var replayAfterExpiry = new ProviderBudgetAdmissionRequest(
            original.ProviderRequestId,
            original.EnvelopeId,
            original.StoreEpochId,
            original.ExpectedConfigurationRevision,
            original.ExpectedLedgerRevision,
            original.RuntimeSessionId,
            original.Scope,
            original.CostScheduleId,
            original.CostScheduleSha256,
            original.OperationClass,
            original.OperationAuthorityReference,
            original.RequestPlanSha256,
            original.RequestSha256,
            original.MaximumChargeBasisSha256,
            original.BindingSha256,
            original.MaximumCharge,
            instant.AddHours(2));

        var replay = await ledger.AdmitAsync(replayAfterExpiry);

        Assert.Equal(ProviderBudgetAdmissionOutcome.Replay, replay.Outcome);
        Assert.Equal(ProviderBudgetState.Expired, replay.State);
        Assert.Equal(admitted.CurrentLedgerRevision!.Value + 1, replay.CurrentLedgerRevision!.Value);
        Assert.Equal(ProviderBudgetReservationStatus.Reserved, replay.Reservation!.Status);
        var restartedLedger = new SqliteProviderBudgetLedger(fixture.Options);
        var durable = Assert.IsType<ProviderBudgetEnvelopeV1>(
            await restartedLedger.ReadEnvelopeAsync(armed.EnvelopeId));
        Assert.Equal(ProviderBudgetState.Expired, durable.State);
        Assert.Equal(replay.CurrentLedgerRevision, durable.LedgerRevision);
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_ledger_revisions " +
            "WHERE state = 'Expired' AND transition_kind = 'Expired';"));
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_audit_events " +
            "WHERE event_type = 'EnvelopeExpired';"));

        var blocked = await restartedLedger.AdmitAsync(
            CreateAdmissionRequest(
                durable,
                "PBR-RECOVERY-EXPIRED-BLOCKED",
                instant.AddHours(2).AddSeconds(1)));

        Assert.Equal(ProviderBudgetAdmissionOutcome.Rejected, blocked.Outcome);
        Assert.Equal(ProviderBudgetAdmissionRejection.Expired, blocked.Rejection);
        Assert.Equal(ProviderBudgetState.Expired, blocked.State);
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_reservations;"));
    }

    [Fact]
    public async Task RestartRearmRecoversEveryOrphanedDispatchAsIndeterminateMaximum()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var instant = DateTimeOffset.UtcNow;
        await SeedDisarmedGraphAsync(fixture.Options.ControlDatabasePath, instant);
        var ledger = new SqliteProviderBudgetLedger(fixture.Options);
        var disarmed = (await ledger.ReadEnvelopeAsync(new ProviderBudgetEnvelopeId(EnvelopeId)))!;
        var current = (await ledger.RearmAsync(CreateRearmRequest(disarmed, instant))).Envelope!;
        var originalRequests = new List<ProviderBudgetAdmissionRequest>();
        var requestIds = new[] { "PBR-RECOVERY-ORPHAN-A", "PBR-RECOVERY-ORPHAN-B" };

        for (var index = 0; index < requestIds.Length; index++)
        {
            var admissionRequest = CreateAdmissionRequest(
                current,
                requestIds[index],
                instant.AddSeconds((index * 2) + 1));
            originalRequests.Add(admissionRequest);
            var admission = await ledger.AdmitAsync(admissionRequest);
            var reservation = Assert.IsType<ProviderBudgetReservation>(admission.Reservation);
            var dispatch = await ledger.MarkDispatchStartedAsync(
                new ProviderBudgetDispatchRequest(
                    reservation.ProviderRequestId,
                    admission.CurrentLedgerRevision!,
                    reservation.CurrentReservationRevision,
                    instant.AddSeconds((index * 2) + 2)));
            Assert.Equal(ProviderBudgetTransitionOutcome.Applied, dispatch.Outcome);
            Assert.Equal(
                ProviderBudgetReservationStatus.DispatchStarted,
                dispatch.Reservation!.Status);
            current = Assert.IsType<ProviderBudgetEnvelopeV1>(
                await ledger.ReadEnvelopeAsync(current.EnvelopeId));
        }

        var restartedLedger = new SqliteProviderBudgetLedger(fixture.Options);
        var recovery = await restartedLedger.RearmAsync(
            CreateRearmRequest(
                current,
                instant.AddSeconds(6),
                "PRS-RECOVERY-RESTART"));

        Assert.Equal(ProviderBudgetRearmOutcome.Rejected, recovery.Outcome);
        var reconciled = Assert.IsType<ProviderBudgetEnvelopeV1>(recovery.Envelope);
        Assert.Equal(ProviderBudgetState.ReconciliationRequired, reconciled.State);
        Assert.Equal(current.LedgerRevision.Value + requestIds.Length, reconciled.LedgerRevision.Value);
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_rearms;"));
        Assert.Equal(requestIds.Length, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_commitments AS commitment " +
            "JOIN provider_budget_reservations AS reservation " +
            "ON reservation.provider_request_id = commitment.provider_request_id " +
            "WHERE commitment.commitment_kind = 'IndeterminateMaximum' " +
            "AND commitment.committed_units = reservation.maximum_charge_units " +
            "AND commitment.provider_duration_milliseconds IS NULL;"));
        Assert.Equal(requestIds.Length, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_reservation_transitions " +
            "WHERE from_status = 'DispatchStarted' " +
            "AND to_status = 'IndeterminateCommitted' " +
            "AND transition_kind = 'IndeterminateCommitted';"));
        Assert.Equal(requestIds.Length, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_audit_events " +
            "WHERE event_type = 'IndeterminateCommitted' " +
            "AND authority_reference = 'AUTH-ITEM-3-REARM-001' " +
            "AND actor_reference = 'ACTOR-ITEM-3-001' " +
            "AND outcome_code = 'Indeterminate';"));

        foreach (var requestId in requestIds)
        {
            var durableReservation = Assert.IsType<ProviderBudgetReservation>(
                await restartedLedger.ReadReservationAsync(new ProviderRequestId(requestId)));
            Assert.Equal(
                ProviderBudgetReservationStatus.IndeterminateCommitted,
                durableReservation.Status);
            Assert.NotNull(durableReservation.TerminalAtUtc);
            Assert.NotNull(durableReservation.TerminalLedgerRevision);
        }

        var replay = await restartedLedger.AdmitAsync(originalRequests[0]);
        Assert.Equal(ProviderBudgetAdmissionOutcome.Replay, replay.Outcome);
        Assert.Equal(ProviderBudgetState.ReconciliationRequired, replay.State);
        Assert.Equal(
            ProviderBudgetReservationStatus.IndeterminateCommitted,
            replay.Reservation!.Status);
        var repeatedRearm = await restartedLedger.RearmAsync(
            CreateRearmRequest(
                reconciled,
                instant.AddSeconds(7),
                "PRS-RECOVERY-SECOND-RESTART"));
        Assert.Equal(ProviderBudgetRearmOutcome.Rejected, repeatedRearm.Outcome);
        Assert.Equal(reconciled.LedgerRevision, repeatedRearm.Envelope!.LedgerRevision);
        Assert.Equal(requestIds.Length, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_commitments;"));
        var furtherAdmission = await restartedLedger.AdmitAsync(
            CreateAdmissionRequest(
                reconciled,
                "PBR-RECOVERY-BLOCKED",
                instant.AddSeconds(8)));
        Assert.Equal(ProviderBudgetAdmissionOutcome.Rejected, furtherAdmission.Outcome);
        Assert.Equal(
            ProviderBudgetAdmissionRejection.ReconciliationRequired,
            furtherAdmission.Rejection);
        Assert.Equal(requestIds.Length, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_reservations;"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RestartAfterExpiryRecoversOrphanedDispatch(bool persistExpiryFirst)
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var instant = DateTimeOffset.UtcNow;
        await SeedDisarmedGraphAsync(fixture.Options.ControlDatabasePath, instant);
        var ledger = new SqliteProviderBudgetLedger(fixture.Options);
        var disarmed = (await ledger.ReadEnvelopeAsync(new ProviderBudgetEnvelopeId(EnvelopeId)))!;
        var armed = (await ledger.RearmAsync(CreateRearmRequest(disarmed, instant))).Envelope!;
        var admission = await ledger.AdmitAsync(
            CreateAdmissionRequest(
                armed,
                "PBR-RECOVERY-EXPIRED-ORPHAN",
                instant.AddSeconds(1)));
        var reservation = Assert.IsType<ProviderBudgetReservation>(admission.Reservation);
        var dispatch = await ledger.MarkDispatchStartedAsync(
            new ProviderBudgetDispatchRequest(
                reservation.ProviderRequestId,
                admission.CurrentLedgerRevision!,
                reservation.CurrentReservationRevision,
                instant.AddSeconds(2)));
        Assert.Equal(ProviderBudgetTransitionOutcome.Applied, dispatch.Outcome);
        var beforeRecovery = Assert.IsType<ProviderBudgetEnvelopeV1>(
            await ledger.ReadEnvelopeAsync(armed.EnvelopeId));

        if (persistExpiryFirst)
        {
            var expiry = await ledger.AdmitAsync(
                CreateAdmissionRequest(
                    beforeRecovery,
                    "PBR-RECOVERY-PERSIST-EXPIRY",
                    instant.AddHours(2)));
            Assert.Equal(ProviderBudgetAdmissionOutcome.Rejected, expiry.Outcome);
            Assert.Equal(ProviderBudgetState.Expired, expiry.State);
            beforeRecovery = Assert.IsType<ProviderBudgetEnvelopeV1>(
                await ledger.ReadEnvelopeAsync(armed.EnvelopeId));
        }

        var restartedLedger = new SqliteProviderBudgetLedger(fixture.Options);
        var recovery = await restartedLedger.RearmAsync(
            CreateRearmRequest(
                beforeRecovery,
                instant.AddHours(2).AddSeconds(1),
                "PRS-RECOVERY-AFTER-EXPIRY"));

        Assert.Equal(ProviderBudgetRearmOutcome.Rejected, recovery.Outcome);
        var reconciled = Assert.IsType<ProviderBudgetEnvelopeV1>(recovery.Envelope);
        Assert.Equal(ProviderBudgetState.ReconciliationRequired, reconciled.State);
        Assert.Equal(beforeRecovery.LedgerRevision.Value + 1, reconciled.LedgerRevision.Value);
        var durableReservation = Assert.IsType<ProviderBudgetReservation>(
            await restartedLedger.ReadReservationAsync(reservation.ProviderRequestId));
        Assert.Equal(
            ProviderBudgetReservationStatus.IndeterminateCommitted,
            durableReservation.Status);
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_commitments AS commitment " +
            "JOIN provider_budget_reservations AS reservation " +
            "ON reservation.provider_request_id = commitment.provider_request_id " +
            "WHERE commitment.commitment_kind = 'IndeterminateMaximum' " +
            "AND commitment.committed_units = reservation.maximum_charge_units;"));
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_rearms;"));
    }

    [Fact]
    public async Task DivergentReplayCannotDowngradeReconciliationRequiredOrEnableRearm()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var instant = DateTimeOffset.UtcNow;
        await SeedDisarmedGraphAsync(fixture.Options.ControlDatabasePath, instant);
        var ledger = new SqliteProviderBudgetLedger(fixture.Options);
        var disarmed = (await ledger.ReadEnvelopeAsync(new ProviderBudgetEnvelopeId(EnvelopeId)))!;
        var armed = (await ledger.RearmAsync(CreateRearmRequest(disarmed, instant))).Envelope!;
        var original = CreateAdmissionRequest(
            armed,
            "PBR-RECOVERY-RR-CONFLICT",
            instant.AddSeconds(1));
        var admission = await ledger.AdmitAsync(original);
        var reservation = Assert.IsType<ProviderBudgetReservation>(admission.Reservation);
        var dispatch = await ledger.MarkDispatchStartedAsync(
            new ProviderBudgetDispatchRequest(
                reservation.ProviderRequestId,
                admission.CurrentLedgerRevision!,
                reservation.CurrentReservationRevision,
                instant.AddSeconds(2)));
        Assert.Equal(ProviderBudgetTransitionOutcome.Applied, dispatch.Outcome);
        var dispatchedEnvelope = Assert.IsType<ProviderBudgetEnvelopeV1>(
            await ledger.ReadEnvelopeAsync(armed.EnvelopeId));
        var recovery = await ledger.RearmAsync(
            CreateRearmRequest(
                dispatchedEnvelope,
                instant.AddSeconds(3),
                "PRS-RECOVERY-RR-CONFLICT"));
        Assert.Equal(ProviderBudgetRearmOutcome.Rejected, recovery.Outcome);
        var reconciled = Assert.IsType<ProviderBudgetEnvelopeV1>(recovery.Envelope);
        Assert.Equal(ProviderBudgetState.ReconciliationRequired, reconciled.State);
        var recoveredReservation = Assert.IsType<ProviderBudgetReservation>(
            await ledger.ReadReservationAsync(reservation.ProviderRequestId));
        Assert.Equal(
            ProviderBudgetReservationStatus.IndeterminateCommitted,
            recoveredReservation.Status);
        var divergentAfterExpiry = new ProviderBudgetAdmissionRequest(
            original.ProviderRequestId,
            original.EnvelopeId,
            original.StoreEpochId,
            original.ExpectedConfigurationRevision,
            original.ExpectedLedgerRevision,
            original.RuntimeSessionId,
            original.Scope,
            original.CostScheduleId,
            original.CostScheduleSha256,
            original.OperationClass,
            original.OperationAuthorityReference,
            original.RequestPlanSha256,
            Sha("reconciliation conflict"),
            original.MaximumChargeBasisSha256,
            original.BindingSha256,
            original.MaximumCharge,
            instant.AddHours(2));

        var conflict = await ledger.AdmitAsync(divergentAfterExpiry);

        Assert.Equal(ProviderBudgetAdmissionOutcome.Rejected, conflict.Outcome);
        Assert.Equal(
            ProviderBudgetAdmissionRejection.ReconciliationRequired,
            conflict.Rejection);
        Assert.Equal(ProviderBudgetState.ReconciliationRequired, conflict.State);
        Assert.Equal(reconciled.LedgerRevision, conflict.CurrentLedgerRevision);
        var repeatedConflict = await ledger.AdmitAsync(
            WithRequestedAt(divergentAfterExpiry, instant.AddHours(3)));
        Assert.Equal(ProviderBudgetAdmissionOutcome.Rejected, repeatedConflict.Outcome);
        Assert.Equal(
            ProviderBudgetAdmissionRejection.ReconciliationRequired,
            repeatedConflict.Rejection);
        Assert.Equal(reconciled.LedgerRevision, repeatedConflict.CurrentLedgerRevision);
        var unchanged = Assert.IsType<ProviderBudgetEnvelopeV1>(
            await ledger.ReadEnvelopeAsync(armed.EnvelopeId));
        Assert.Equal(ProviderBudgetState.ReconciliationRequired, unchanged.State);
        Assert.Equal(reconciled.LedgerRevision, unchanged.LedgerRevision);
        Assert.Equal(reconciled.CurrentLedgerSha256, unchanged.CurrentLedgerSha256);
        var rearm = await ledger.RearmAsync(
            CreateRearmRequest(
                unchanged,
                instant.AddHours(2).AddSeconds(1),
                "PRS-RECOVERY-RR-SECOND-RESTART"));

        Assert.Equal(ProviderBudgetRearmOutcome.Rejected, rearm.Outcome);
        Assert.Equal(ProviderBudgetState.ReconciliationRequired, rearm.Envelope!.State);
        Assert.Equal(unchanged.LedgerRevision, rearm.Envelope.LedgerRevision);
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_rearms;"));
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_reservations;"));
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_commitments;"));
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_audit_events " +
            "WHERE event_type = 'ReservationConflict' " +
            "AND from_state = 'ReconciliationRequired' " +
            "AND to_state = 'ReconciliationRequired' " +
            "AND outcome_code = 'Conflict';"));
    }

    [Theory]
    [InlineData("Dispatch")]
    [InlineData("Commit")]
    [InlineData("Release")]
    public async Task DivergentTransitionReplayPreservesReconciliationRequiredAndAuditsOnce(
        string transitionKind)
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var instant = DateTimeOffset.UtcNow;
        await SeedDisarmedGraphAsync(fixture.Options.ControlDatabasePath, instant);
        var ledger = new SqliteProviderBudgetLedger(fixture.Options);
        var disarmed = (await ledger.ReadEnvelopeAsync(new ProviderBudgetEnvelopeId(EnvelopeId)))!;
        var armed = (await ledger.RearmAsync(CreateRearmRequest(disarmed, instant))).Envelope!;
        var admission = await ledger.AdmitAsync(
            CreateAdmissionRequest(
                armed,
                $"PBR-RECOVERY-{transitionKind.ToUpperInvariant()}-CONFLICT",
                instant.AddSeconds(1)));
        var reservation = Assert.IsType<ProviderBudgetReservation>(admission.Reservation);
        ProviderBudgetEnvelopeV1 reconciled;
        ProviderBudgetReservation expectedReservation;
        Func<DateTimeOffset, Task<ProviderBudgetTransitionResult>> divergentReplay;

        if (transitionKind == "Release")
        {
            var release = await ledger.ReleasePreSendAsync(
                new ProviderBudgetReleaseRequest(
                    reservation.ProviderRequestId,
                    admission.CurrentLedgerRevision!,
                    reservation.CurrentReservationRevision,
                    ProviderBudgetReleaseProofKind.BeforeCredentialLookup,
                    Sha("original release proof"),
                    reservation.OperationAuthorityReference,
                    instant.AddSeconds(2)));
            Assert.Equal(ProviderBudgetTransitionOutcome.Applied, release.Outcome);
            expectedReservation = Assert.IsType<ProviderBudgetReservation>(release.Reservation);
            var afterRelease = Assert.IsType<ProviderBudgetEnvelopeV1>(
                await ledger.ReadEnvelopeAsync(armed.EnvelopeId));
            var orphanAdmission = await ledger.AdmitAsync(
                CreateAdmissionRequest(
                    afterRelease,
                    "PBR-RECOVERY-RELEASE-ORPHAN",
                    instant.AddSeconds(3)));
            var orphan = Assert.IsType<ProviderBudgetReservation>(orphanAdmission.Reservation);
            var orphanDispatch = await ledger.MarkDispatchStartedAsync(
                new ProviderBudgetDispatchRequest(
                    orphan.ProviderRequestId,
                    orphanAdmission.CurrentLedgerRevision!,
                    orphan.CurrentReservationRevision,
                    instant.AddSeconds(4)));
            Assert.Equal(ProviderBudgetTransitionOutcome.Applied, orphanDispatch.Outcome);
            var recovery = await ledger.RearmAsync(
                CreateRearmRequest(
                    Assert.IsType<ProviderBudgetEnvelopeV1>(
                        await ledger.ReadEnvelopeAsync(armed.EnvelopeId)),
                    instant.AddSeconds(5),
                    "PRS-RECOVERY-RELEASE-RESTART"));
            reconciled = Assert.IsType<ProviderBudgetEnvelopeV1>(recovery.Envelope);
            divergentReplay = occurredAtUtc => ledger.ReleasePreSendAsync(
                new ProviderBudgetReleaseRequest(
                    reservation.ProviderRequestId,
                    admission.CurrentLedgerRevision!,
                    reservation.CurrentReservationRevision,
                    ProviderBudgetReleaseProofKind.BeforeCredentialLookup,
                    Sha("divergent release proof"),
                    reservation.OperationAuthorityReference,
                    occurredAtUtc));
        }
        else
        {
            var dispatch = await ledger.MarkDispatchStartedAsync(
                new ProviderBudgetDispatchRequest(
                    reservation.ProviderRequestId,
                    admission.CurrentLedgerRevision!,
                    reservation.CurrentReservationRevision,
                    instant.AddSeconds(2)));
            Assert.Equal(ProviderBudgetTransitionOutcome.Applied, dispatch.Outcome);
            var dispatched = Assert.IsType<ProviderBudgetReservation>(dispatch.Reservation);

            if (transitionKind == "Commit")
            {
                var commitment = await ledger.CommitAsync(
                    new ProviderBudgetCommitRequest(
                        reservation.ProviderRequestId,
                        dispatch.CurrentLedgerRevision!,
                        dispatched.CurrentReservationRevision,
                        ProviderBudgetCommitmentKind.IndeterminateMaximum,
                        reservation.MaximumCharge,
                        Sha("original indeterminate evidence"),
                        new ProviderBudgetOutcomeCode("ORIGINAL_INDETERMINATE"),
                        providerDuration: null,
                        instant.AddSeconds(3)));
                Assert.Equal(ProviderBudgetTransitionOutcome.Applied, commitment.Outcome);
                expectedReservation = Assert.IsType<ProviderBudgetReservation>(commitment.Reservation);
                reconciled = Assert.IsType<ProviderBudgetEnvelopeV1>(
                    await ledger.ReadEnvelopeAsync(armed.EnvelopeId));
                divergentReplay = occurredAtUtc => ledger.CommitAsync(
                    new ProviderBudgetCommitRequest(
                        reservation.ProviderRequestId,
                        dispatch.CurrentLedgerRevision!,
                        dispatched.CurrentReservationRevision,
                        ProviderBudgetCommitmentKind.IndeterminateMaximum,
                        reservation.MaximumCharge,
                        Sha("divergent indeterminate evidence"),
                        new ProviderBudgetOutcomeCode("ORIGINAL_INDETERMINATE"),
                        providerDuration: null,
                        occurredAtUtc));
            }
            else
            {
                var recovery = await ledger.RearmAsync(
                    CreateRearmRequest(
                        Assert.IsType<ProviderBudgetEnvelopeV1>(
                            await ledger.ReadEnvelopeAsync(armed.EnvelopeId)),
                        instant.AddSeconds(3),
                        "PRS-RECOVERY-DISPATCH-RESTART"));
                reconciled = Assert.IsType<ProviderBudgetEnvelopeV1>(recovery.Envelope);
                expectedReservation = Assert.IsType<ProviderBudgetReservation>(
                    await ledger.ReadReservationAsync(reservation.ProviderRequestId));
                divergentReplay = occurredAtUtc => ledger.MarkDispatchStartedAsync(
                    new ProviderBudgetDispatchRequest(
                        reservation.ProviderRequestId,
                        admission.CurrentLedgerRevision!,
                        reservation.CurrentReservationRevision,
                        occurredAtUtc));
            }
        }

        Assert.Equal(ProviderBudgetState.ReconciliationRequired, reconciled.State);
        var firstConflict = await divergentReplay(instant.AddSeconds(10));
        var repeatedConflict = await divergentReplay(instant.AddSeconds(11));

        Assert.Equal(ProviderBudgetTransitionOutcome.Rejected, firstConflict.Outcome);
        Assert.Equal(ProviderBudgetTransitionRejection.EnvelopeNotArmed, firstConflict.Rejection);
        Assert.Equal(ProviderBudgetState.ReconciliationRequired, firstConflict.State);
        Assert.Equal(reconciled.LedgerRevision, firstConflict.CurrentLedgerRevision);
        Assert.Equal(ProviderBudgetTransitionOutcome.Rejected, repeatedConflict.Outcome);
        Assert.Equal(ProviderBudgetTransitionRejection.EnvelopeNotArmed, repeatedConflict.Rejection);
        Assert.Equal(ProviderBudgetState.ReconciliationRequired, repeatedConflict.State);
        Assert.Equal(reconciled.LedgerRevision, repeatedConflict.CurrentLedgerRevision);
        var unchanged = Assert.IsType<ProviderBudgetEnvelopeV1>(
            await ledger.ReadEnvelopeAsync(armed.EnvelopeId));
        Assert.Equal(ProviderBudgetState.ReconciliationRequired, unchanged.State);
        Assert.Equal(reconciled.LedgerRevision, unchanged.LedgerRevision);
        Assert.Equal(reconciled.CurrentLedgerSha256, unchanged.CurrentLedgerSha256);
        var durableReservation = Assert.IsType<ProviderBudgetReservation>(
            await ledger.ReadReservationAsync(reservation.ProviderRequestId));
        Assert.Equal(expectedReservation.Status, durableReservation.Status);
        Assert.Equal(
            1,
            await ScalarAsync(
                fixture.Options.ControlDatabasePath,
                $"SELECT COUNT(*) FROM provider_budget_audit_events " +
                $"WHERE event_type = 'ReservationConflict' " +
                $"AND provider_request_id = '{reservation.ProviderRequestId.Value}' " +
                $"AND from_state = 'ReconciliationRequired' " +
                $"AND to_state = 'ReconciliationRequired' " +
                $"AND outcome_code = 'Conflict';"));
        Assert.Equal(0, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_audit_events " +
            "WHERE event_type = 'EnvelopeTripped' " +
            "AND from_state = 'ReconciliationRequired';"));
        var rejectedRearm = await ledger.RearmAsync(
            CreateRearmRequest(
                unchanged,
                instant.AddSeconds(12),
                $"PRS-RECOVERY-{transitionKind.ToUpperInvariant()}-SECOND-RESTART"));
        Assert.Equal(ProviderBudgetRearmOutcome.Rejected, rejectedRearm.Outcome);
        Assert.Equal(ProviderBudgetState.ReconciliationRequired, rejectedRearm.Envelope!.State);
        Assert.Equal(unchanged.LedgerRevision, rejectedRearm.Envelope.LedgerRevision);
    }

    [Fact]
    public async Task MultiOrphanRecoveryRollsBackAtomicallyWhenSecondCommitmentFails()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var instant = DateTimeOffset.UtcNow;
        await SeedDisarmedGraphAsync(fixture.Options.ControlDatabasePath, instant);
        var ledger = new SqliteProviderBudgetLedger(fixture.Options);
        var disarmed = (await ledger.ReadEnvelopeAsync(new ProviderBudgetEnvelopeId(EnvelopeId)))!;
        var current = (await ledger.RearmAsync(CreateRearmRequest(disarmed, instant))).Envelope!;
        var requestIds = new[] { "PBR-RECOVERY-FAULT-A", "PBR-RECOVERY-FAULT-B" };

        for (var index = 0; index < requestIds.Length; index++)
        {
            var admission = await ledger.AdmitAsync(
                CreateAdmissionRequest(
                    current,
                    requestIds[index],
                    instant.AddSeconds((index * 2) + 1)));
            var reservation = Assert.IsType<ProviderBudgetReservation>(admission.Reservation);
            var dispatch = await ledger.MarkDispatchStartedAsync(
                new ProviderBudgetDispatchRequest(
                    reservation.ProviderRequestId,
                    admission.CurrentLedgerRevision!,
                    reservation.CurrentReservationRevision,
                    instant.AddSeconds((index * 2) + 2)));
            Assert.Equal(ProviderBudgetTransitionOutcome.Applied, dispatch.Outcome);
            current = Assert.IsType<ProviderBudgetEnvelopeV1>(
                await ledger.ReadEnvelopeAsync(current.EnvelopeId));
        }

        await ExecuteWriteAsync(
            fixture.Options.ControlDatabasePath,
            """
            CREATE TRIGGER provider_budget_recovery_fault
            BEFORE INSERT ON provider_budget_commitments
            WHEN NEW.provider_request_id = 'PBR-RECOVERY-FAULT-B'
            BEGIN
                SELECT RAISE(ABORT, 'synthetic provider-budget recovery fault');
            END;
            """);
        var revisionBeforeRecovery = current.LedgerRevision;
        var shaBeforeRecovery = current.CurrentLedgerSha256;
        var restartedLedger = new SqliteProviderBudgetLedger(fixture.Options);

        _ = await Assert.ThrowsAsync<DbUpdateException>(() => restartedLedger.RearmAsync(
            CreateRearmRequest(
                current,
                instant.AddSeconds(6),
                "PRS-RECOVERY-FAULT-RESTART")));

        var afterFailureLedger = new SqliteProviderBudgetLedger(fixture.Options);
        var unchanged = Assert.IsType<ProviderBudgetEnvelopeV1>(
            await afterFailureLedger.ReadEnvelopeAsync(current.EnvelopeId));
        Assert.Equal(ProviderBudgetState.Armed, unchanged.State);
        Assert.Equal(revisionBeforeRecovery, unchanged.LedgerRevision);
        Assert.Equal(shaBeforeRecovery, unchanged.CurrentLedgerSha256);
        Assert.Equal(0, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_commitments;"));
        Assert.Equal(2, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_reservations " +
            "WHERE status = 'DispatchStarted';"));
        Assert.Equal(0, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_audit_events " +
            "WHERE event_type = 'IndeterminateCommitted';"));

        await ExecuteWriteAsync(
            fixture.Options.ControlDatabasePath,
            "DROP TRIGGER provider_budget_recovery_fault;");
        var retry = await afterFailureLedger.RearmAsync(
            CreateRearmRequest(
                unchanged,
                instant.AddSeconds(7),
                "PRS-RECOVERY-FAULT-RESTART"));

        Assert.Equal(ProviderBudgetRearmOutcome.Rejected, retry.Outcome);
        Assert.Equal(ProviderBudgetState.ReconciliationRequired, retry.Envelope!.State);
        Assert.Equal(2, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_commitments;"));
    }

    [Fact]
    public async Task SameSessionRearmDoesNotClassifyAnActiveDispatchAsOrphaned()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var instant = DateTimeOffset.UtcNow;
        await SeedDisarmedGraphAsync(fixture.Options.ControlDatabasePath, instant);
        var ledger = new SqliteProviderBudgetLedger(fixture.Options);
        var disarmed = (await ledger.ReadEnvelopeAsync(new ProviderBudgetEnvelopeId(EnvelopeId)))!;
        var armed = (await ledger.RearmAsync(CreateRearmRequest(disarmed, instant))).Envelope!;
        var admission = await ledger.AdmitAsync(
            CreateAdmissionRequest(armed, "PBR-RECOVERY-ACTIVE", instant.AddSeconds(1)));
        var reservation = Assert.IsType<ProviderBudgetReservation>(admission.Reservation);
        var dispatch = await ledger.MarkDispatchStartedAsync(
            new ProviderBudgetDispatchRequest(
                reservation.ProviderRequestId,
                admission.CurrentLedgerRevision!,
                reservation.CurrentReservationRevision,
                instant.AddSeconds(2)));
        var dispatchedEnvelope = Assert.IsType<ProviderBudgetEnvelopeV1>(
            await ledger.ReadEnvelopeAsync(armed.EnvelopeId));
        var restartedLedgerObject = new SqliteProviderBudgetLedger(fixture.Options);

        var result = await restartedLedgerObject.RearmAsync(
            CreateRearmRequest(
                dispatchedEnvelope,
                instant.AddSeconds(3),
                dispatchedEnvelope.RuntimeSessionId!.Value));

        Assert.Equal(ProviderBudgetRearmOutcome.Rejected, result.Outcome);
        Assert.Equal(ProviderBudgetState.Armed, result.Envelope!.State);
        Assert.Equal(dispatchedEnvelope.LedgerRevision, result.Envelope.LedgerRevision);
        Assert.Equal(
            ProviderBudgetReservationStatus.DispatchStarted,
            dispatch.Reservation!.Status);
        Assert.Equal(0, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_commitments;"));
    }

    [Theory]
    [InlineData("PRS-ITEM-3-001")]
    [InlineData("PRS-CLEAN-RESTART")]
    public async Task ArmedEnvelopeDoesNotRearmOnCleanRestart(string requestedSessionId)
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var instant = DateTimeOffset.UtcNow;
        await SeedDisarmedGraphAsync(fixture.Options.ControlDatabasePath, instant);
        var ledger = new SqliteProviderBudgetLedger(fixture.Options);
        var disarmed = (await ledger.ReadEnvelopeAsync(new ProviderBudgetEnvelopeId(EnvelopeId)))!;
        var armed = (await ledger.RearmAsync(CreateRearmRequest(disarmed, instant))).Envelope!;
        var restartedLedger = new SqliteProviderBudgetLedger(fixture.Options);

        var result = await restartedLedger.RearmAsync(
            CreateRearmRequest(armed, instant.AddSeconds(1), requestedSessionId));

        Assert.Equal(ProviderBudgetRearmOutcome.Rejected, result.Outcome);
        var unchanged = Assert.IsType<ProviderBudgetEnvelopeV1>(result.Envelope);
        Assert.Equal(ProviderBudgetState.Armed, unchanged.State);
        Assert.Equal(armed.RuntimeSessionId, unchanged.RuntimeSessionId);
        Assert.Equal(armed.LedgerRevision, unchanged.LedgerRevision);
        Assert.Equal(armed.CurrentLedgerSha256, unchanged.CurrentLedgerSha256);
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_rearms;"));
    }

    [Fact]
    public async Task TrippedEnvelopeWithReservedAttemptCannotBeRearmed()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var instant = DateTimeOffset.UtcNow;
        await SeedDisarmedGraphAsync(fixture.Options.ControlDatabasePath, instant);
        var ledger = new SqliteProviderBudgetLedger(fixture.Options);
        var disarmed = (await ledger.ReadEnvelopeAsync(new ProviderBudgetEnvelopeId(EnvelopeId)))!;
        var armed = (await ledger.RearmAsync(CreateRearmRequest(disarmed, instant))).Envelope!;
        var original = CreateAdmissionRequest(
            armed,
            "PBR-RECOVERY-RESERVED",
            instant.AddSeconds(1));
        _ = await ledger.AdmitAsync(original);
        var divergent = new ProviderBudgetAdmissionRequest(
            original.ProviderRequestId,
            original.EnvelopeId,
            original.StoreEpochId,
            original.ExpectedConfigurationRevision,
            original.ExpectedLedgerRevision,
            original.RuntimeSessionId,
            original.Scope,
            original.CostScheduleId,
            original.CostScheduleSha256,
            original.OperationClass,
            original.OperationAuthorityReference,
            original.RequestPlanSha256,
            Sha("reserved conflict"),
            original.MaximumChargeBasisSha256,
            original.BindingSha256,
            original.MaximumCharge,
            original.RequestedAtUtc);
        var conflict = await ledger.AdmitAsync(divergent);
        Assert.Equal(ProviderBudgetState.Tripped, conflict.State);
        var tripped = Assert.IsType<ProviderBudgetEnvelopeV1>(
            await ledger.ReadEnvelopeAsync(armed.EnvelopeId));

        var rearm = await ledger.RearmAsync(
            CreateRearmRequest(
                tripped,
                instant.AddSeconds(2),
                "PRS-RECOVERY-AFTER-TRIP"));

        Assert.Equal(ProviderBudgetRearmOutcome.Rejected, rearm.Outcome);
        Assert.Equal(ProviderBudgetState.Tripped, rearm.Envelope!.State);
        Assert.Equal(tripped.LedgerRevision, rearm.Envelope.LedgerRevision);
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_rearms;"));
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_reservations " +
            "WHERE status = 'Reserved';"));
    }

    private static ProviderBudgetRearmRequest CreateRearmRequest(
        ProviderBudgetEnvelopeV1 envelope,
        DateTimeOffset instant,
        string runtimeSessionId = "PRS-ITEM-3-001") =>
        new(
            envelope.EnvelopeId,
            envelope.StoreEpochId,
            envelope.ConfigurationRevision,
            envelope.LedgerRevision,
            envelope.RearmRevision,
            new ProviderRuntimeSessionId(runtimeSessionId),
            new ProviderBudgetAuthorityReference("AUTH-ITEM-3-REARM-001"),
            new ProviderBudgetAuthorityReference("ACTOR-ITEM-3-001"),
            Sha("explicit rearm reason"),
            envelope.AggregateCommitted,
            envelope.AggregateReserved,
            envelope.AggregateIndeterminate,
            SqliteProviderBudgetLedger.ComputeOperationBalancesSha256(envelope),
            new ProviderBudgetSha256(ConfigurationSha256),
            instant);

    private static ProviderBudgetAdmissionRequest CreateAdmissionRequest(
        ProviderBudgetEnvelopeV1 envelope,
        string requestId,
        DateTimeOffset instant) =>
        new(
            new ProviderRequestId(requestId),
            envelope.EnvelopeId,
            envelope.StoreEpochId,
            envelope.ConfigurationRevision,
            envelope.LedgerRevision,
            envelope.RuntimeSessionId!,
            envelope.Scope,
            envelope.CostScheduleId,
            envelope.CostScheduleSha256,
            ProviderBudgetOperationClass.QueryEmbedding,
            new ProviderBudgetAuthorityReference("AUTH-ITEM-3-QUERY-001"),
            Sha("request plan"),
            Sha("exact request"),
            Sha("zero maximum basis"),
            Sha("exact binding"),
            new ProviderBudgetUnits(0),
            instant);

    private static ProviderBudgetAdmissionRequest WithRequestedAt(
        ProviderBudgetAdmissionRequest request,
        DateTimeOffset requestedAtUtc) =>
        new(
            request.ProviderRequestId,
            request.EnvelopeId,
            request.StoreEpochId,
            request.ExpectedConfigurationRevision,
            request.ExpectedLedgerRevision,
            request.RuntimeSessionId,
            request.Scope,
            request.CostScheduleId,
            request.CostScheduleSha256,
            request.OperationClass,
            request.OperationAuthorityReference,
            request.RequestPlanSha256,
            request.RequestSha256,
            request.MaximumChargeBasisSha256,
            request.BindingSha256,
            request.MaximumCharge,
            requestedAtUtc);

    private static async Task SeedDisarmedGraphAsync(string path, DateTimeOffset instant)
    {
        var created = instant.AddMinutes(-5).ToString("O", CultureInfo.InvariantCulture);
        var effective = instant.AddMinutes(-1).ToString("O", CultureInfo.InvariantCulture);
        var expires = instant.AddHours(1).ToString("O", CultureInfo.InvariantCulture);
        var epochSha = Sha("item 3 epoch").Value;
        var scheduleSha = Sha("item 3 zero schedule").Value;
        var auditSha = Sha("item 3 initial audit").Value;
        var sql = $"""
            INSERT INTO provider_budget_store_epochs
                (store_epoch_id, epoch_revision, previous_store_epoch_id, epoch_kind,
                 restore_checkpoint_sha256, authority_reference, occurred_at_utc,
                 previous_epoch_sha256, epoch_sha256)
            VALUES
                ('{StoreEpochId}', 1, NULL, 'Initial', NULL,
                 'AUTH-ITEM-3-DESIGN-001', '{created}', '{ZeroSha256}', '{epochSha}');

            INSERT INTO provider_budget_control_heads
                (control_id, current_store_epoch_id, epoch_revision, row_revision)
            VALUES ('provider-budget-control-v1', '{StoreEpochId}', 1, 1);

            INSERT INTO provider_budget_envelopes
                (envelope_id, schema_version, current_store_epoch_id, environment_id,
                 provider_id, billing_scope_reference, model_id, currency_code,
                 accounting_unit_id, current_configuration_revision,
                 current_ledger_revision, current_rearm_revision, state,
                 runtime_session_id, aggregate_limit_units, aggregate_committed_units,
                 aggregate_reserved_units, aggregate_indeterminate_units,
                 is_initialised, is_closed, created_at_utc,
                 creation_authority_reference, closed_at_utc,
                 closure_authority_reference, current_ledger_sha256)
            VALUES
                ('{EnvelopeId}', 1, '{StoreEpochId}', 'ENV-ITEM-3', 'openai',
                 'BILLING-ITEM-3', 'MODEL-ITEM-3', 'USD', 'UNIT-ITEM-3',
                 1, 1, 0, 'Disarmed', NULL, 0, 0, 0, 0, 0, 0,
                 '{created}', 'AUTH-ITEM-3-DESIGN-001', NULL, NULL,
                 '{InitialLedgerSha256}');

            INSERT INTO provider_budget_configurations
                (envelope_id, configuration_revision, previous_configuration_revision,
                 cost_schedule_id, cost_schedule_sha256, aggregate_limit_units,
                 effective_at_utc, expires_at_utc, configuration_authority_reference,
                 created_at_utc, sealed_at_utc, configuration_sha256)
            VALUES
                ('{EnvelopeId}', 1, NULL, 'PCS-ITEM-3-ZERO', '{scheduleSha}', 0,
                 '{effective}', '{expires}', 'AUTH-ITEM-3-DESIGN-001',
                 '{created}', NULL, '{ConfigurationSha256}');

            INSERT INTO provider_budget_operation_allocations
                (envelope_id, configuration_revision, operation_class, allocation_limit_units)
            VALUES
                ('{EnvelopeId}', 1, 'AdministrativeIndexEmbedding', 0),
                ('{EnvelopeId}', 1, 'QueryEmbedding', 0),
                ('{EnvelopeId}', 1, 'GroundedGeneration', 0);

            UPDATE provider_budget_configurations SET sealed_at_utc = '{created}'
            WHERE envelope_id = '{EnvelopeId}' AND configuration_revision = 1;

            INSERT INTO provider_budget_ledger_revisions
                (envelope_id, ledger_revision, store_epoch_id, previous_ledger_revision,
                 configuration_revision, rearm_revision, state, runtime_session_id,
                 aggregate_limit_units, aggregate_committed_units,
                 aggregate_reserved_units, aggregate_indeterminate_units,
                 transition_kind, provider_request_id, transition_authority_reference,
                 occurred_at_utc, previous_ledger_sha256, ledger_sha256, is_complete)
            VALUES
                ('{EnvelopeId}', 1, '{StoreEpochId}', NULL, 1, 0, 'Disarmed', NULL,
                 0, 0, 0, 0, 'EnvelopeCreated', NULL, 'AUTH-ITEM-3-DESIGN-001',
                 '{created}', '{ZeroSha256}', '{InitialLedgerSha256}', 0);

            INSERT INTO provider_budget_operation_balance_revisions
                (envelope_id, ledger_revision, operation_class,
                 configuration_revision, allocation_limit_units, committed_units,
                 reserved_units, indeterminate_units)
            VALUES
                ('{EnvelopeId}', 1, 'AdministrativeIndexEmbedding', 1, 0, 0, 0, 0),
                ('{EnvelopeId}', 1, 'QueryEmbedding', 1, 0, 0, 0, 0),
                ('{EnvelopeId}', 1, 'GroundedGeneration', 1, 0, 0, 0, 0);

            UPDATE provider_budget_ledger_revisions SET is_complete = 1
            WHERE envelope_id = '{EnvelopeId}' AND ledger_revision = 1;

            INSERT INTO provider_budget_audit_events
                (audit_event_id, envelope_id, ledger_revision, provider_request_id,
                 operation_class, event_type, authority_reference, actor_reference,
                 request_sha256, maximum_charge_units, from_state, to_state,
                 outcome_code, occurred_at_utc, details_sha256)
            VALUES
                ('PBA-ITEM-3-INITIAL', '{EnvelopeId}', 1, NULL, NULL,
                 'EnvelopeCreated', 'AUTH-ITEM-3-DESIGN-001', 'ACTOR-ITEM-3-001',
                 NULL, NULL, NULL, 'Disarmed', 'Created', '{created}', '{auditSha}');

            UPDATE provider_budget_envelopes SET is_initialised = 1
            WHERE envelope_id = '{EnvelopeId}';
            """;
        await using var connection = new SqliteConnection(
            $"Data Source={path};Mode=ReadWrite;Cache=Private;Foreign Keys=True");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        _ = await command.ExecuteNonQueryAsync();
    }

    private static async Task<long> ScalarAsync(string path, string sql)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={path};Mode=ReadOnly;Cache=Private;Foreign Keys=True");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToInt64(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }

    private static async Task ExecuteWriteAsync(string path, string sql)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={path};Mode=ReadWrite;Cache=Private;Foreign Keys=True");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        _ = await command.ExecuteNonQueryAsync();
    }

    private static ProviderBudgetSha256 Sha(string value) =>
        new(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant());
}
