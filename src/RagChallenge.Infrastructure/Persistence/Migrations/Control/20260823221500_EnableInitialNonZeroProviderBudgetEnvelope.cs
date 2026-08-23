// Purpose: Permits one sealed non-zero aggregate limit to become visible only when the complete initial provider-budget graph is published atomically.
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RagChallenge.Infrastructure.Persistence.Migrations.Control;

[DbContext(typeof(ControlPlaneDbContext))]
[Migration("20260823221500_EnableInitialNonZeroProviderBudgetEnvelope")]
public sealed class EnableInitialNonZeroProviderBudgetEnvelope : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_provider_budget_envelopes_validate_update;");
        migrationBuilder.Sql(CreateValidationTrigger(permitInitialAggregateLimit: true));
        migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_provider_budget_ledger_revisions_append_insert;");
        migrationBuilder.Sql(CreateLedgerAppendTrigger(permitInitialAggregateLimit: true));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_provider_budget_envelopes_validate_update;");
        migrationBuilder.Sql(CreateValidationTrigger(permitInitialAggregateLimit: false));
        migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_provider_budget_ledger_revisions_append_insert;");
        migrationBuilder.Sql(CreateLedgerAppendTrigger(permitInitialAggregateLimit: false));
    }

    private static string CreateLedgerAppendTrigger(bool permitInitialAggregateLimit)
    {
        var initialAggregateLimitRule = permitInitialAggregateLimit
            ? """
              NEW.aggregate_limit_units = (
                  SELECT configuration.aggregate_limit_units
                  FROM provider_budget_configurations AS configuration
                  WHERE configuration.envelope_id = NEW.envelope_id
                    AND configuration.configuration_revision = NEW.configuration_revision
                    AND configuration.sealed_at_utc IS NOT NULL) AND
              """
            : "NEW.aggregate_limit_units = 0 AND";

        return $$"""
            CREATE TRIGGER trg_provider_budget_ledger_revisions_append_insert
            BEFORE INSERT ON provider_budget_ledger_revisions
            BEGIN
                SELECT CASE WHEN NEW.is_complete <> 0
                THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_LEDGER_NOT_READY') END;

                SELECT CASE WHEN NOT EXISTS (
                    SELECT 1
                    FROM provider_budget_configurations AS configuration
                    WHERE configuration.envelope_id = NEW.envelope_id
                      AND configuration.configuration_revision = NEW.configuration_revision
                      AND configuration.sealed_at_utc IS NOT NULL)
                THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_CONFIGURATION_NOT_SEALED') END;

                SELECT CASE WHEN NOT EXISTS (
                    SELECT 1
                    FROM provider_budget_control_heads AS head
                    WHERE head.current_store_epoch_id = NEW.store_epoch_id)
                THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_RESTORE_CONFLICT') END;

                SELECT CASE WHEN
                    (NEW.ledger_revision = 1 AND NOT (
                        NEW.previous_ledger_sha256 = printf('%064d', 0) AND
                        NEW.transition_kind = 'EnvelopeCreated' AND
                        NEW.configuration_revision = 1 AND
                        NEW.rearm_revision = 0 AND
                        NEW.state = 'Disarmed' AND
                        NEW.runtime_session_id IS NULL AND
                        {{initialAggregateLimitRule}}
                        NEW.aggregate_committed_units = 0 AND
                        NEW.aggregate_reserved_units = 0 AND
                        NEW.aggregate_indeterminate_units = 0 AND
                        EXISTS (
                            SELECT 1
                            FROM provider_budget_envelopes AS envelope
                            WHERE envelope.envelope_id = NEW.envelope_id
                              AND envelope.is_initialised = 0) AND
                        NOT EXISTS (
                            SELECT 1
                            FROM provider_budget_ledger_revisions
                            WHERE envelope_id = NEW.envelope_id))) OR
                    (NEW.ledger_revision > 1 AND NOT (
                        EXISTS (
                            SELECT 1
                            FROM provider_budget_envelopes AS envelope
                            WHERE envelope.envelope_id = NEW.envelope_id
                              AND envelope.is_initialised = 1
                              AND NEW.ledger_revision = envelope.current_ledger_revision + 1) AND
                        EXISTS (
                            SELECT 1
                            FROM provider_budget_ledger_revisions AS previous
                            WHERE previous.envelope_id = NEW.envelope_id
                              AND previous.ledger_revision = NEW.ledger_revision - 1
                              AND previous.is_complete = 1
                              AND previous.ledger_sha256 = NEW.previous_ledger_sha256)))
                THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_LEDGER_CONFLICT') END;

                SELECT CASE WHEN
                    NEW.transition_kind IN (
                        'ReservationAdmitted', 'DispatchStarted', 'ObservedCommitted',
                        'IndeterminateCommitted', 'OverrunCommitted', 'PreSendReleased', 'Reconciled')
                    AND NEW.provider_request_id IS NULL
                THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_REQUEST_BINDING_MISSING') END;
            END;
            """;
    }

    private static string CreateValidationTrigger(bool permitInitialAggregateLimit)
    {
        var aggregateLimitRule = permitInitialAggregateLimit
            ? """
              (NEW.aggregate_limit_units IS OLD.aggregate_limit_units OR (
                  OLD.aggregate_limit_units = 0 AND
                  NEW.aggregate_limit_units = (
                      SELECT configuration.aggregate_limit_units
                      FROM provider_budget_configurations AS configuration
                      WHERE configuration.envelope_id = NEW.envelope_id
                        AND configuration.configuration_revision = NEW.current_configuration_revision
                        AND configuration.sealed_at_utc IS NOT NULL))) AND
              """
            : "NEW.aggregate_limit_units IS OLD.aggregate_limit_units AND";

        return $$"""
            CREATE TRIGGER trg_provider_budget_envelopes_validate_update
            BEFORE UPDATE ON provider_budget_envelopes
            BEGIN
                SELECT CASE WHEN OLD.is_initialised = 0 AND NOT (
                    NEW.is_initialised = 1 AND
                    NEW.envelope_id IS OLD.envelope_id AND
                    NEW.schema_version IS OLD.schema_version AND
                    NEW.current_store_epoch_id IS OLD.current_store_epoch_id AND
                    NEW.environment_id IS OLD.environment_id AND
                    NEW.provider_id IS OLD.provider_id AND
                    NEW.billing_scope_reference IS OLD.billing_scope_reference AND
                    NEW.model_id IS OLD.model_id AND
                    NEW.currency_code IS OLD.currency_code AND
                    NEW.accounting_unit_id IS OLD.accounting_unit_id AND
                    NEW.current_configuration_revision IS OLD.current_configuration_revision AND
                    NEW.current_ledger_revision IS OLD.current_ledger_revision AND
                    NEW.current_rearm_revision IS OLD.current_rearm_revision AND
                    NEW.state IS OLD.state AND
                    NEW.runtime_session_id IS OLD.runtime_session_id AND
                    {{aggregateLimitRule}}
                    NEW.aggregate_committed_units IS OLD.aggregate_committed_units AND
                    NEW.aggregate_reserved_units IS OLD.aggregate_reserved_units AND
                    NEW.aggregate_indeterminate_units IS OLD.aggregate_indeterminate_units AND
                    NEW.is_closed IS OLD.is_closed AND
                    NEW.created_at_utc IS OLD.created_at_utc AND
                    NEW.creation_authority_reference IS OLD.creation_authority_reference AND
                    NEW.closed_at_utc IS OLD.closed_at_utc AND
                    NEW.closure_authority_reference IS OLD.closure_authority_reference AND
                    NEW.current_ledger_sha256 IS OLD.current_ledger_sha256)
                THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_ENVELOPE_CONFLICT') END;

                SELECT CASE WHEN OLD.is_initialised = 1 AND NOT (
                    NEW.is_initialised = 1 AND
                    OLD.is_closed = 0 AND
                    NEW.envelope_id IS OLD.envelope_id AND
                    NEW.schema_version IS OLD.schema_version AND
                    NEW.environment_id IS OLD.environment_id AND
                    NEW.provider_id IS OLD.provider_id AND
                    NEW.billing_scope_reference IS OLD.billing_scope_reference AND
                    NEW.model_id IS OLD.model_id AND
                    NEW.currency_code IS OLD.currency_code AND
                    NEW.accounting_unit_id IS OLD.accounting_unit_id AND
                    NEW.created_at_utc IS OLD.created_at_utc AND
                    NEW.creation_authority_reference IS OLD.creation_authority_reference AND
                    NEW.current_ledger_revision = OLD.current_ledger_revision + 1)
                THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_ENVELOPE_CONFLICT') END;

                SELECT CASE WHEN NOT EXISTS (
                    SELECT 1
                    FROM provider_budget_configurations AS configuration
                    WHERE configuration.envelope_id = NEW.envelope_id
                      AND configuration.configuration_revision = NEW.current_configuration_revision
                      AND configuration.sealed_at_utc IS NOT NULL)
                THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_CONFIGURATION_NOT_SEALED') END;

                SELECT CASE WHEN NOT EXISTS (
                    SELECT 1
                    FROM provider_budget_ledger_revisions AS ledger
                    WHERE ledger.envelope_id = NEW.envelope_id
                      AND ledger.ledger_revision = NEW.current_ledger_revision
                      AND ledger.is_complete = 1
                      AND ledger.store_epoch_id = NEW.current_store_epoch_id
                      AND ledger.configuration_revision = NEW.current_configuration_revision
                      AND ledger.rearm_revision = NEW.current_rearm_revision
                      AND ledger.state = NEW.state
                      AND ledger.runtime_session_id IS NEW.runtime_session_id
                      AND ledger.aggregate_limit_units = NEW.aggregate_limit_units
                      AND ledger.aggregate_committed_units = NEW.aggregate_committed_units
                      AND ledger.aggregate_reserved_units = NEW.aggregate_reserved_units
                      AND ledger.aggregate_indeterminate_units = NEW.aggregate_indeterminate_units
                      AND ledger.ledger_sha256 = NEW.current_ledger_sha256)
                THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_LEDGER_CONFLICT') END;

                SELECT CASE WHEN NOT EXISTS (
                    SELECT 1
                    FROM provider_budget_control_heads AS head
                    WHERE head.current_store_epoch_id = NEW.current_store_epoch_id)
                THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_RESTORE_CONFLICT') END;

                SELECT CASE WHEN NOT EXISTS (
                    SELECT 1
                    FROM provider_budget_audit_events AS audit
                    JOIN provider_budget_ledger_revisions AS ledger
                      ON ledger.envelope_id = audit.envelope_id
                     AND ledger.ledger_revision = audit.ledger_revision
                    WHERE audit.envelope_id = NEW.envelope_id
                      AND audit.ledger_revision = NEW.current_ledger_revision
                      AND audit.event_type = CASE ledger.transition_kind
                          WHEN 'EnvelopeCreated' THEN 'EnvelopeCreated'
                          WHEN 'ConfigurationChanged' THEN 'ConfigurationRevised'
                          WHEN 'ReservationAdmitted' THEN 'ReservationAdmitted'
                          WHEN 'DispatchStarted' THEN 'DispatchStarted'
                          WHEN 'ObservedCommitted' THEN 'CommitmentRecorded'
                          WHEN 'IndeterminateCommitted' THEN 'IndeterminateCommitted'
                          WHEN 'OverrunCommitted' THEN 'OverrunDetected'
                          WHEN 'PreSendReleased' THEN 'ReservationReleased'
                          WHEN 'ConflictTripped' THEN 'EnvelopeTripped'
                          WHEN 'PolicyTripped' THEN 'EnvelopeTripped'
                          WHEN 'Exhausted' THEN 'EnvelopeExhausted'
                          WHEN 'Expired' THEN 'EnvelopeExpired'
                          WHEN 'Reconciled' THEN 'ReconciliationRecorded'
                          WHEN 'Rearmed' THEN 'EnvelopeRearmed'
                          WHEN 'EnvelopeClosed' THEN 'EnvelopeClosed'
                          WHEN 'RestoreDetected' THEN 'RestoreDetected'
                      END)
                THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_AUDIT_MISSING') END;
            END;
            """;
    }
}
