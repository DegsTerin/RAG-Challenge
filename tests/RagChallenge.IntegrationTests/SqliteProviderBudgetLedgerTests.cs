// Purpose: Verifies focused local SQLite persistence, explicit administrative rearming, durable zero admission and terminal accounting without provider credentials, network or non-zero schedules.
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Data.Sqlite;

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

    private static ProviderBudgetRearmRequest CreateRearmRequest(
        ProviderBudgetEnvelopeV1 envelope,
        DateTimeOffset instant) =>
        new(
            envelope.EnvelopeId,
            envelope.StoreEpochId,
            envelope.ConfigurationRevision,
            envelope.LedgerRevision,
            envelope.RearmRevision,
            new ProviderRuntimeSessionId("PRS-ITEM-3-001"),
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

    private static ProviderBudgetSha256 Sha(string value) =>
        new(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant());
}
