// Purpose: Creates the crash-safe internal Control schema for persistent provider-budget admission.
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RagChallenge.Infrastructure.Persistence.Migrations.Control;

/// <inheritdoc />
public partial class AddPersistentProviderBudgetAdmission : Migration
{
    private static readonly string[] EnvelopeConfigurationColumns = ["envelope_id", "configuration_revision"];
    private static readonly string[] EnvelopeLedgerColumns = ["envelope_id", "ledger_revision"];
    private static readonly string[] EnvelopeConfigurationOperationColumns =
        ["envelope_id", "configuration_revision", "operation_class"];
    private static readonly string[] EnvelopeOccurredColumns = ["envelope_id", "occurred_at_utc"];
    private static readonly string[] EnvelopeConfigurationDigestColumns =
        ["envelope_id", "configuration_sha256"];
    private static readonly string[] EnvelopeScopeColumns =
        ["environment_id", "provider_id", "billing_scope_reference", "model_id", "currency_code", "accounting_unit_id"];
    private static readonly string[] EnvelopeLedgerDigestColumns = ["envelope_id", "ledger_sha256"];
    private static readonly string[] StoreEpochEnvelopeLedgerColumns =
        ["store_epoch_id", "envelope_id", "ledger_revision"];
    private static readonly string[] EnvelopeResultingLedgerColumns =
        ["envelope_id", "resulting_ledger_revision"];
    private static readonly string[] EnvelopeStoreEpochSessionColumns =
        ["envelope_id", "store_epoch_id", "new_runtime_session_id"];
    private static readonly string[] EnvelopeAdmissionLedgerColumns =
        ["envelope_id", "admission_ledger_revision"];
    private static readonly string[] EnvelopeStatusColumns = ["envelope_id", "status"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "provider_budget_store_epochs",
            columns: table => new
            {
                store_epoch_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                epoch_revision = table.Column<long>(type: "INTEGER", nullable: false),
                previous_store_epoch_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                epoch_kind = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                restore_checkpoint_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                authority_reference = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                occurred_at_utc = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                previous_epoch_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                epoch_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_provider_budget_store_epochs", x => x.store_epoch_id);
                table.CheckConstraint("ck_provider_budget_store_epochs_authority", "length(authority_reference) BETWEEN 1 AND 128 AND authority_reference GLOB '[A-Za-z0-9]*' AND authority_reference NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_store_epochs_digest", "length(epoch_sha256) = 64 AND epoch_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_store_epochs_id", "length(store_epoch_id) BETWEEN 1 AND 128 AND store_epoch_id GLOB '[A-Za-z0-9]*' AND store_epoch_id NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_store_epochs_kind", "(epoch_kind = 'Initial' AND epoch_revision = 1 AND previous_store_epoch_id IS NULL AND restore_checkpoint_sha256 IS NULL) OR (epoch_kind = 'Restore' AND epoch_revision > 1 AND previous_store_epoch_id IS NOT NULL AND restore_checkpoint_sha256 IS NOT NULL)");
                table.CheckConstraint("ck_provider_budget_store_epochs_occurred_utc", "length(occurred_at_utc) = 33 AND substr(occurred_at_utc, -6) = '+00:00'");
                table.CheckConstraint("ck_provider_budget_store_epochs_previous_digest", "length(previous_epoch_sha256) = 64 AND previous_epoch_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_store_epochs_previous_id", "previous_store_epoch_id IS NULL OR length(previous_store_epoch_id) BETWEEN 1 AND 128 AND previous_store_epoch_id GLOB '[A-Za-z0-9]*' AND previous_store_epoch_id NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_store_epochs_restore_digest", "restore_checkpoint_sha256 IS NULL OR length(restore_checkpoint_sha256) = 64 AND restore_checkpoint_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_store_epochs_revision", "epoch_revision > 0");
                table.ForeignKey(
                    name: "FK_provider_budget_store_epochs_provider_budget_store_epochs_previous_store_epoch_id",
                    column: x => x.previous_store_epoch_id,
                    principalTable: "provider_budget_store_epochs",
                    principalColumn: "store_epoch_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "provider_budget_control_heads",
            columns: table => new
            {
                control_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                current_store_epoch_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                epoch_revision = table.Column<long>(type: "INTEGER", nullable: false),
                row_revision = table.Column<long>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_provider_budget_control_heads", x => x.control_id);
                table.CheckConstraint("ck_provider_budget_control_heads_id", "control_id = 'provider-budget-control-v1'");
                table.CheckConstraint("ck_provider_budget_control_heads_revisions", "epoch_revision > 0 AND row_revision > 0");
                table.ForeignKey(
                    name: "FK_provider_budget_control_heads_provider_budget_store_epochs_current_store_epoch_id",
                    column: x => x.current_store_epoch_id,
                    principalTable: "provider_budget_store_epochs",
                    principalColumn: "store_epoch_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "provider_budget_envelopes",
            columns: table => new
            {
                envelope_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                schema_version = table.Column<int>(type: "INTEGER", nullable: false),
                current_store_epoch_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                environment_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                provider_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                billing_scope_reference = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                model_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                currency_code = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                accounting_unit_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                current_configuration_revision = table.Column<long>(type: "INTEGER", nullable: false),
                current_ledger_revision = table.Column<long>(type: "INTEGER", nullable: false),
                current_rearm_revision = table.Column<long>(type: "INTEGER", nullable: false),
                state = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                runtime_session_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                aggregate_limit_units = table.Column<long>(type: "INTEGER", nullable: false),
                aggregate_committed_units = table.Column<long>(type: "INTEGER", nullable: false),
                aggregate_reserved_units = table.Column<long>(type: "INTEGER", nullable: false),
                aggregate_indeterminate_units = table.Column<long>(type: "INTEGER", nullable: false),
                is_initialised = table.Column<int>(type: "INTEGER", nullable: false),
                is_closed = table.Column<int>(type: "INTEGER", nullable: false),
                created_at_utc = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                creation_authority_reference = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                closed_at_utc = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                closure_authority_reference = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                current_ledger_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_provider_budget_envelopes", x => x.envelope_id);
                table.CheckConstraint("ck_provider_budget_envelopes_accounting_unit", "length(accounting_unit_id) BETWEEN 1 AND 128 AND accounting_unit_id GLOB '[A-Za-z0-9]*' AND accounting_unit_id NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_envelopes_amounts", "aggregate_limit_units >= 0 AND aggregate_committed_units >= 0 AND aggregate_reserved_units >= 0 AND aggregate_indeterminate_units >= 0 AND aggregate_committed_units <= aggregate_limit_units AND aggregate_reserved_units <= aggregate_limit_units - aggregate_committed_units AND aggregate_indeterminate_units <= aggregate_committed_units");
                table.CheckConstraint("ck_provider_budget_envelopes_billing_scope", "length(billing_scope_reference) BETWEEN 1 AND 128 AND billing_scope_reference GLOB '[A-Za-z0-9]*' AND billing_scope_reference NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_envelopes_closed", "(is_closed = 0 AND closed_at_utc IS NULL AND closure_authority_reference IS NULL) OR (is_closed = 1 AND state <> 'Armed' AND closed_at_utc IS NOT NULL AND closure_authority_reference IS NOT NULL AND aggregate_reserved_units = 0 AND aggregate_indeterminate_units = 0)");
                table.CheckConstraint("ck_provider_budget_envelopes_closed_utc", "closed_at_utc IS NULL OR length(closed_at_utc) = 33 AND substr(closed_at_utc, -6) = '+00:00'");
                table.CheckConstraint("ck_provider_budget_envelopes_closure_authority", "closure_authority_reference IS NULL OR length(closure_authority_reference) BETWEEN 1 AND 128 AND closure_authority_reference GLOB '[A-Za-z0-9]*' AND closure_authority_reference NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_envelopes_created_utc", "length(created_at_utc) = 33 AND substr(created_at_utc, -6) = '+00:00'");
                table.CheckConstraint("ck_provider_budget_envelopes_creation_authority", "length(creation_authority_reference) BETWEEN 1 AND 128 AND creation_authority_reference GLOB '[A-Za-z0-9]*' AND creation_authority_reference NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_envelopes_currency", "length(currency_code) = 3 AND currency_code NOT GLOB '*[^A-Z]*'");
                table.CheckConstraint("ck_provider_budget_envelopes_environment", "length(environment_id) BETWEEN 1 AND 128 AND environment_id GLOB '[A-Za-z0-9]*' AND environment_id NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_envelopes_id", "length(envelope_id) BETWEEN 1 AND 128 AND envelope_id GLOB '[A-Za-z0-9]*' AND envelope_id NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_envelopes_initialised", "is_initialised IN (0, 1) AND (is_initialised = 1 OR (state = 'Disarmed' AND runtime_session_id IS NULL AND aggregate_limit_units = 0 AND aggregate_committed_units = 0 AND aggregate_reserved_units = 0 AND aggregate_indeterminate_units = 0 AND is_closed = 0))");
                table.CheckConstraint("ck_provider_budget_envelopes_ledger_digest", "length(current_ledger_sha256) = 64 AND current_ledger_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_envelopes_model", "length(model_id) BETWEEN 1 AND 128 AND model_id GLOB '[A-Za-z0-9]*' AND model_id NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_envelopes_provider", "length(provider_id) BETWEEN 1 AND 128 AND provider_id GLOB '[A-Za-z0-9]*' AND provider_id NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_envelopes_revisions", "current_configuration_revision > 0 AND current_ledger_revision > 0 AND current_rearm_revision >= 0");
                table.CheckConstraint("ck_provider_budget_envelopes_schema", "schema_version = 1");
                table.CheckConstraint("ck_provider_budget_envelopes_session", "(state <> 'Armed') OR runtime_session_id IS NOT NULL");
                table.CheckConstraint("ck_provider_budget_envelopes_state", "state IN ('Disarmed', 'Armed', 'Tripped', 'Exhausted', 'ReconciliationRequired', 'Expired')");
                table.CheckConstraint("ck_provider_budget_envelopes_time_order", "closed_at_utc IS NULL OR closed_at_utc >= created_at_utc");
                table.ForeignKey(
                    name: "FK_provider_budget_envelopes_provider_budget_store_epochs_current_store_epoch_id",
                    column: x => x.current_store_epoch_id,
                    principalTable: "provider_budget_store_epochs",
                    principalColumn: "store_epoch_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "provider_budget_configurations",
            columns: table => new
            {
                envelope_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                configuration_revision = table.Column<long>(type: "INTEGER", nullable: false),
                previous_configuration_revision = table.Column<long>(type: "INTEGER", nullable: true),
                cost_schedule_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                cost_schedule_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                aggregate_limit_units = table.Column<long>(type: "INTEGER", nullable: false),
                effective_at_utc = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                expires_at_utc = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                configuration_authority_reference = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                created_at_utc = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                sealed_at_utc = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                configuration_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_provider_budget_configurations", x => new { x.envelope_id, x.configuration_revision });
                table.CheckConstraint("ck_provider_budget_configurations_authority", "length(configuration_authority_reference) BETWEEN 1 AND 128 AND configuration_authority_reference GLOB '[A-Za-z0-9]*' AND configuration_authority_reference NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_configurations_created_utc", "length(created_at_utc) = 33 AND substr(created_at_utc, -6) = '+00:00'");
                table.CheckConstraint("ck_provider_budget_configurations_digest", "length(configuration_sha256) = 64 AND configuration_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_configurations_effective_utc", "length(effective_at_utc) = 33 AND substr(effective_at_utc, -6) = '+00:00'");
                table.CheckConstraint("ck_provider_budget_configurations_expires_utc", "length(expires_at_utc) = 33 AND substr(expires_at_utc, -6) = '+00:00'");
                table.CheckConstraint("ck_provider_budget_configurations_limit", "aggregate_limit_units >= 0");
                table.CheckConstraint("ck_provider_budget_configurations_revision", "(configuration_revision = 1 AND previous_configuration_revision IS NULL) OR (configuration_revision > 1 AND previous_configuration_revision = configuration_revision - 1)");
                table.CheckConstraint("ck_provider_budget_configurations_schedule_digest", "length(cost_schedule_sha256) = 64 AND cost_schedule_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_configurations_schedule_id", "length(cost_schedule_id) BETWEEN 1 AND 128 AND cost_schedule_id GLOB '[A-Za-z0-9]*' AND cost_schedule_id NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_configurations_seal_order", "sealed_at_utc IS NULL OR sealed_at_utc >= created_at_utc");
                table.CheckConstraint("ck_provider_budget_configurations_sealed_utc", "sealed_at_utc IS NULL OR length(sealed_at_utc) = 33 AND substr(sealed_at_utc, -6) = '+00:00'");
                table.CheckConstraint("ck_provider_budget_configurations_time_order", "expires_at_utc > effective_at_utc");
                table.ForeignKey(
                    name: "FK_provider_budget_configurations_provider_budget_envelopes_envelope_id",
                    column: x => x.envelope_id,
                    principalTable: "provider_budget_envelopes",
                    principalColumn: "envelope_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "provider_budget_ledger_revisions",
            columns: table => new
            {
                envelope_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                ledger_revision = table.Column<long>(type: "INTEGER", nullable: false),
                store_epoch_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                previous_ledger_revision = table.Column<long>(type: "INTEGER", nullable: true),
                configuration_revision = table.Column<long>(type: "INTEGER", nullable: false),
                rearm_revision = table.Column<long>(type: "INTEGER", nullable: false),
                state = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                runtime_session_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                aggregate_limit_units = table.Column<long>(type: "INTEGER", nullable: false),
                aggregate_committed_units = table.Column<long>(type: "INTEGER", nullable: false),
                aggregate_reserved_units = table.Column<long>(type: "INTEGER", nullable: false),
                aggregate_indeterminate_units = table.Column<long>(type: "INTEGER", nullable: false),
                transition_kind = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                provider_request_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                transition_authority_reference = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                occurred_at_utc = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                previous_ledger_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                ledger_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                is_complete = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_provider_budget_ledger_revisions", x => new { x.envelope_id, x.ledger_revision });
                table.CheckConstraint("ck_provider_budget_ledger_revisions_amounts", "aggregate_limit_units >= 0 AND aggregate_committed_units >= 0 AND aggregate_reserved_units >= 0 AND aggregate_indeterminate_units >= 0 AND aggregate_committed_units <= aggregate_limit_units AND aggregate_reserved_units <= aggregate_limit_units - aggregate_committed_units AND aggregate_indeterminate_units <= aggregate_committed_units");
                table.CheckConstraint("ck_provider_budget_ledger_revisions_authority", "length(transition_authority_reference) BETWEEN 1 AND 128 AND transition_authority_reference GLOB '[A-Za-z0-9]*' AND transition_authority_reference NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_ledger_revisions_complete", "is_complete IN (0, 1)");
                table.CheckConstraint("ck_provider_budget_ledger_revisions_configuration", "configuration_revision > 0");
                table.CheckConstraint("ck_provider_budget_ledger_revisions_digest", "length(ledger_sha256) = 64 AND ledger_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_ledger_revisions_occurred_utc", "length(occurred_at_utc) = 33 AND substr(occurred_at_utc, -6) = '+00:00'");
                table.CheckConstraint("ck_provider_budget_ledger_revisions_previous_digest", "length(previous_ledger_sha256) = 64 AND previous_ledger_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_ledger_revisions_rearm", "rearm_revision >= 0");
                table.CheckConstraint("ck_provider_budget_ledger_revisions_request", "provider_request_id IS NULL OR length(provider_request_id) BETWEEN 1 AND 128 AND provider_request_id GLOB '[A-Za-z0-9]*' AND provider_request_id NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_ledger_revisions_revision", "(ledger_revision = 1 AND previous_ledger_revision IS NULL) OR (ledger_revision > 1 AND previous_ledger_revision = ledger_revision - 1)");
                table.CheckConstraint("ck_provider_budget_ledger_revisions_session", "(state <> 'Armed') OR runtime_session_id IS NOT NULL");
                table.CheckConstraint("ck_provider_budget_ledger_revisions_state", "state IN ('Disarmed', 'Armed', 'Tripped', 'Exhausted', 'ReconciliationRequired', 'Expired')");
                table.CheckConstraint("ck_provider_budget_ledger_revisions_transition", "transition_kind IN ('EnvelopeCreated', 'ConfigurationChanged', 'ReservationAdmitted', 'DispatchStarted', 'ObservedCommitted', 'IndeterminateCommitted', 'OverrunCommitted', 'PreSendReleased', 'ConflictTripped', 'PolicyTripped', 'Exhausted', 'Expired', 'Reconciled', 'Rearmed', 'EnvelopeClosed', 'RestoreDetected')");
                table.ForeignKey(
                    name: "FK_provider_budget_ledger_revisions_provider_budget_configurations_envelope_id_configuration_revision",
                    columns: x => new { x.envelope_id, x.configuration_revision },
                    principalTable: "provider_budget_configurations",
                    principalColumns: EnvelopeConfigurationColumns,
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_provider_budget_ledger_revisions_provider_budget_envelopes_envelope_id",
                    column: x => x.envelope_id,
                    principalTable: "provider_budget_envelopes",
                    principalColumn: "envelope_id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_provider_budget_ledger_revisions_provider_budget_store_epochs_store_epoch_id",
                    column: x => x.store_epoch_id,
                    principalTable: "provider_budget_store_epochs",
                    principalColumn: "store_epoch_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "provider_budget_operation_allocations",
            columns: table => new
            {
                envelope_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                configuration_revision = table.Column<long>(type: "INTEGER", nullable: false),
                operation_class = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                allocation_limit_units = table.Column<long>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_provider_budget_operation_allocations", x => new { x.envelope_id, x.configuration_revision, x.operation_class });
                table.CheckConstraint("ck_provider_budget_operation_allocations_limit", "allocation_limit_units >= 0");
                table.CheckConstraint("ck_provider_budget_operation_allocations_operation", "operation_class IN ('AdministrativeIndexEmbedding', 'QueryEmbedding', 'GroundedGeneration')");
                table.ForeignKey(
                    name: "FK_provider_budget_operation_allocations_provider_budget_configurations_envelope_id_configuration_revision",
                    columns: x => new { x.envelope_id, x.configuration_revision },
                    principalTable: "provider_budget_configurations",
                    principalColumns: EnvelopeConfigurationColumns,
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "provider_budget_audit_events",
            columns: table => new
            {
                audit_event_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                envelope_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                ledger_revision = table.Column<long>(type: "INTEGER", nullable: false),
                provider_request_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                operation_class = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                event_type = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                authority_reference = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                actor_reference = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                request_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                maximum_charge_units = table.Column<long>(type: "INTEGER", nullable: true),
                from_state = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                to_state = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                outcome_code = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                occurred_at_utc = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                details_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_provider_budget_audit_events", x => x.audit_event_id);
                table.CheckConstraint("ck_provider_budget_audit_events_actor", "actor_reference IS NULL OR length(actor_reference) BETWEEN 1 AND 128 AND actor_reference GLOB '[A-Za-z0-9]*' AND actor_reference NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_audit_events_authority", "authority_reference IS NULL OR length(authority_reference) BETWEEN 1 AND 128 AND authority_reference GLOB '[A-Za-z0-9]*' AND authority_reference NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_audit_events_details", "length(details_sha256) = 64 AND details_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_audit_events_event", "event_type IN ('EnvelopeCreated', 'ConfigurationRevised', 'ReservationAdmitted', 'ReservationConflict', 'DispatchStarted', 'PreSendFailureObserved', 'ReservationReleased', 'CommitmentRecorded', 'IndeterminateCommitted', 'OverrunDetected', 'EnvelopeTripped', 'EnvelopeExhausted', 'EnvelopeExpired', 'ReconciliationRecorded', 'EnvelopeRearmed', 'EnvelopeClosed', 'RestoreDetected')");
                table.CheckConstraint("ck_provider_budget_audit_events_from_state", "from_state IS NULL OR from_state IN ('Disarmed', 'Armed', 'Tripped', 'Exhausted', 'ReconciliationRequired', 'Expired')");
                table.CheckConstraint("ck_provider_budget_audit_events_id", "length(audit_event_id) BETWEEN 1 AND 128 AND audit_event_id GLOB '[A-Za-z0-9]*' AND audit_event_id NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_audit_events_maximum", "maximum_charge_units IS NULL OR maximum_charge_units >= 0");
                table.CheckConstraint("ck_provider_budget_audit_events_occurred_utc", "length(occurred_at_utc) = 33 AND substr(occurred_at_utc, -6) = '+00:00'");
                table.CheckConstraint("ck_provider_budget_audit_events_operation", "operation_class IS NULL OR operation_class IN ('AdministrativeIndexEmbedding', 'QueryEmbedding', 'GroundedGeneration')");
                table.CheckConstraint("ck_provider_budget_audit_events_outcome", "length(outcome_code) BETWEEN 1 AND 128 AND outcome_code GLOB '[A-Za-z0-9]*' AND outcome_code NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_audit_events_request", "request_sha256 IS NULL OR length(request_sha256) = 64 AND request_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_audit_events_request_id", "provider_request_id IS NULL OR length(provider_request_id) BETWEEN 1 AND 128 AND provider_request_id GLOB '[A-Za-z0-9]*' AND provider_request_id NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_audit_events_to_state", "to_state IS NULL OR to_state IN ('Disarmed', 'Armed', 'Tripped', 'Exhausted', 'ReconciliationRequired', 'Expired')");
                table.ForeignKey(
                    name: "FK_provider_budget_audit_events_provider_budget_envelopes_envelope_id",
                    column: x => x.envelope_id,
                    principalTable: "provider_budget_envelopes",
                    principalColumn: "envelope_id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_provider_budget_audit_events_provider_budget_ledger_revisions_envelope_id_ledger_revision",
                    columns: x => new { x.envelope_id, x.ledger_revision },
                    principalTable: "provider_budget_ledger_revisions",
                    principalColumns: EnvelopeLedgerColumns,
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "provider_budget_rearms",
            columns: table => new
            {
                envelope_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                rearm_revision = table.Column<long>(type: "INTEGER", nullable: false),
                store_epoch_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                expected_configuration_revision = table.Column<long>(type: "INTEGER", nullable: false),
                expected_ledger_revision = table.Column<long>(type: "INTEGER", nullable: false),
                expected_rearm_revision = table.Column<long>(type: "INTEGER", nullable: false),
                resulting_ledger_revision = table.Column<long>(type: "INTEGER", nullable: false),
                new_runtime_session_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                authority_reference = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                actor_reference = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                reason_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                acknowledged_committed_units = table.Column<long>(type: "INTEGER", nullable: false),
                acknowledged_reserved_units = table.Column<long>(type: "INTEGER", nullable: false),
                acknowledged_indeterminate_units = table.Column<long>(type: "INTEGER", nullable: false),
                operation_balances_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                configuration_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                occurred_at_utc = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                rearm_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_provider_budget_rearms", x => new { x.envelope_id, x.rearm_revision });
                table.CheckConstraint("ck_provider_budget_rearms_actor", "length(actor_reference) BETWEEN 1 AND 128 AND actor_reference GLOB '[A-Za-z0-9]*' AND actor_reference NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_rearms_amounts", "acknowledged_committed_units >= 0 AND acknowledged_reserved_units >= 0 AND acknowledged_indeterminate_units >= 0 AND acknowledged_indeterminate_units <= acknowledged_committed_units");
                table.CheckConstraint("ck_provider_budget_rearms_authority", "length(authority_reference) BETWEEN 1 AND 128 AND authority_reference GLOB '[A-Za-z0-9]*' AND authority_reference NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_rearms_balances", "length(operation_balances_sha256) = 64 AND operation_balances_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_rearms_configuration", "length(configuration_sha256) = 64 AND configuration_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_rearms_digest", "length(rearm_sha256) = 64 AND rearm_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_rearms_occurred_utc", "length(occurred_at_utc) = 33 AND substr(occurred_at_utc, -6) = '+00:00'");
                table.CheckConstraint("ck_provider_budget_rearms_reason", "length(reason_sha256) = 64 AND reason_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_rearms_revisions", "rearm_revision > 0 AND expected_configuration_revision > 0 AND expected_ledger_revision > 0 AND expected_rearm_revision = rearm_revision - 1 AND resulting_ledger_revision = expected_ledger_revision + 1");
                table.CheckConstraint("ck_provider_budget_rearms_session", "length(new_runtime_session_id) BETWEEN 1 AND 128 AND new_runtime_session_id GLOB '[A-Za-z0-9]*' AND new_runtime_session_id NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.ForeignKey(
                    name: "FK_provider_budget_rearms_provider_budget_envelopes_envelope_id",
                    column: x => x.envelope_id,
                    principalTable: "provider_budget_envelopes",
                    principalColumn: "envelope_id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_provider_budget_rearms_provider_budget_ledger_revisions_envelope_id_resulting_ledger_revision",
                    columns: x => new { x.envelope_id, x.resulting_ledger_revision },
                    principalTable: "provider_budget_ledger_revisions",
                    principalColumns: EnvelopeLedgerColumns,
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_provider_budget_rearms_provider_budget_store_epochs_store_epoch_id",
                    column: x => x.store_epoch_id,
                    principalTable: "provider_budget_store_epochs",
                    principalColumn: "store_epoch_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "provider_budget_reservations",
            columns: table => new
            {
                provider_request_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                envelope_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                store_epoch_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                configuration_revision = table.Column<long>(type: "INTEGER", nullable: false),
                operation_class = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                operation_authority_reference = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                request_plan_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                request_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                maximum_charge_basis_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                cost_schedule_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                binding_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                maximum_charge_units = table.Column<long>(type: "INTEGER", nullable: false),
                admitted_runtime_session_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                admission_ledger_revision = table.Column<long>(type: "INTEGER", nullable: false),
                current_reservation_revision = table.Column<long>(type: "INTEGER", nullable: false),
                is_initialised = table.Column<int>(type: "INTEGER", nullable: false),
                status = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                admitted_at_utc = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                dispatch_started_at_utc = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                terminal_at_utc = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                terminal_ledger_revision = table.Column<long>(type: "INTEGER", nullable: true),
                current_transition_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_provider_budget_reservations", x => x.provider_request_id);
                table.CheckConstraint("ck_provider_budget_reservations_admitted_utc", "length(admitted_at_utc) = 33 AND substr(admitted_at_utc, -6) = '+00:00'");
                table.CheckConstraint("ck_provider_budget_reservations_authority", "length(operation_authority_reference) BETWEEN 1 AND 128 AND operation_authority_reference GLOB '[A-Za-z0-9]*' AND operation_authority_reference NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_reservations_binding", "length(binding_sha256) = 64 AND binding_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_reservations_dispatch_utc", "dispatch_started_at_utc IS NULL OR length(dispatch_started_at_utc) = 33 AND substr(dispatch_started_at_utc, -6) = '+00:00'");
                table.CheckConstraint("ck_provider_budget_reservations_id", "length(provider_request_id) BETWEEN 1 AND 128 AND provider_request_id GLOB '[A-Za-z0-9]*' AND provider_request_id NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_reservations_initialised", "is_initialised IN (0, 1) AND (is_initialised = 1 OR (status = 'Reserved' AND dispatch_started_at_utc IS NULL AND terminal_at_utc IS NULL AND terminal_ledger_revision IS NULL))");
                table.CheckConstraint("ck_provider_budget_reservations_maximum", "maximum_charge_units >= 0");
                table.CheckConstraint("ck_provider_budget_reservations_maximum_basis", "length(maximum_charge_basis_sha256) = 64 AND maximum_charge_basis_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_reservations_operation", "operation_class IN ('AdministrativeIndexEmbedding', 'QueryEmbedding', 'GroundedGeneration')");
                table.CheckConstraint("ck_provider_budget_reservations_request", "length(request_sha256) = 64 AND request_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_reservations_request_plan", "length(request_plan_sha256) = 64 AND request_plan_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_reservations_revisions", "configuration_revision > 0 AND admission_ledger_revision > 0 AND current_reservation_revision > 0 AND (terminal_ledger_revision IS NULL OR terminal_ledger_revision > 0)");
                table.CheckConstraint("ck_provider_budget_reservations_schedule", "length(cost_schedule_sha256) = 64 AND cost_schedule_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_reservations_session", "length(admitted_runtime_session_id) BETWEEN 1 AND 128 AND admitted_runtime_session_id GLOB '[A-Za-z0-9]*' AND admitted_runtime_session_id NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_reservations_state_shape", "(status = 'Reserved' AND dispatch_started_at_utc IS NULL AND terminal_at_utc IS NULL AND terminal_ledger_revision IS NULL) OR (status = 'DispatchStarted' AND dispatch_started_at_utc IS NOT NULL AND terminal_at_utc IS NULL AND terminal_ledger_revision IS NULL) OR (status = 'ReleasedPreSend' AND dispatch_started_at_utc IS NULL AND terminal_at_utc IS NOT NULL AND terminal_ledger_revision IS NOT NULL) OR (status IN ('Committed', 'IndeterminateCommitted', 'OverrunCommitted') AND dispatch_started_at_utc IS NOT NULL AND terminal_at_utc IS NOT NULL AND terminal_ledger_revision IS NOT NULL)");
                table.CheckConstraint("ck_provider_budget_reservations_status", "status IN ('Reserved', 'DispatchStarted', 'Committed', 'ReleasedPreSend', 'IndeterminateCommitted', 'OverrunCommitted')");
                table.CheckConstraint("ck_provider_budget_reservations_terminal_utc", "terminal_at_utc IS NULL OR length(terminal_at_utc) = 33 AND substr(terminal_at_utc, -6) = '+00:00'");
                table.CheckConstraint("ck_provider_budget_reservations_time_order", "(dispatch_started_at_utc IS NULL OR dispatch_started_at_utc >= admitted_at_utc) AND (terminal_at_utc IS NULL OR terminal_at_utc >= admitted_at_utc) AND (terminal_at_utc IS NULL OR dispatch_started_at_utc IS NULL OR terminal_at_utc >= dispatch_started_at_utc)");
                table.CheckConstraint("ck_provider_budget_reservations_transition_digest", "length(current_transition_sha256) = 64 AND current_transition_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.ForeignKey(
                    name: "FK_provider_budget_reservations_provider_budget_configurations_envelope_id_configuration_revision",
                    columns: x => new { x.envelope_id, x.configuration_revision },
                    principalTable: "provider_budget_configurations",
                    principalColumns: EnvelopeConfigurationColumns,
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_provider_budget_reservations_provider_budget_envelopes_envelope_id",
                    column: x => x.envelope_id,
                    principalTable: "provider_budget_envelopes",
                    principalColumn: "envelope_id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_provider_budget_reservations_provider_budget_ledger_revisions_envelope_id_admission_ledger_revision",
                    columns: x => new { x.envelope_id, x.admission_ledger_revision },
                    principalTable: "provider_budget_ledger_revisions",
                    principalColumns: EnvelopeLedgerColumns,
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_provider_budget_reservations_provider_budget_store_epochs_store_epoch_id",
                    column: x => x.store_epoch_id,
                    principalTable: "provider_budget_store_epochs",
                    principalColumn: "store_epoch_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "provider_budget_operation_balance_revisions",
            columns: table => new
            {
                envelope_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                ledger_revision = table.Column<long>(type: "INTEGER", nullable: false),
                operation_class = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                configuration_revision = table.Column<long>(type: "INTEGER", nullable: false),
                allocation_limit_units = table.Column<long>(type: "INTEGER", nullable: false),
                committed_units = table.Column<long>(type: "INTEGER", nullable: false),
                reserved_units = table.Column<long>(type: "INTEGER", nullable: false),
                indeterminate_units = table.Column<long>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_provider_budget_operation_balance_revisions", x => new { x.envelope_id, x.ledger_revision, x.operation_class });
                table.CheckConstraint("ck_provider_budget_operation_balances_amounts", "allocation_limit_units >= 0 AND committed_units >= 0 AND reserved_units >= 0 AND indeterminate_units >= 0 AND committed_units <= allocation_limit_units AND reserved_units <= allocation_limit_units - committed_units AND indeterminate_units <= committed_units");
                table.CheckConstraint("ck_provider_budget_operation_balances_operation", "operation_class IN ('AdministrativeIndexEmbedding', 'QueryEmbedding', 'GroundedGeneration')");
                table.ForeignKey(
                    name: "FK_provider_budget_operation_balance_revisions_provider_budget_ledger_revisions_envelope_id_ledger_revision",
                    columns: x => new { x.envelope_id, x.ledger_revision },
                    principalTable: "provider_budget_ledger_revisions",
                    principalColumns: EnvelopeLedgerColumns,
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_provider_budget_operation_balance_revisions_provider_budget_operation_allocations_envelope_id_configuration_revision_operation_class",
                    columns: x => new { x.envelope_id, x.configuration_revision, x.operation_class },
                    principalTable: "provider_budget_operation_allocations",
                    principalColumns: EnvelopeConfigurationOperationColumns,
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "provider_budget_commitments",
            columns: table => new
            {
                provider_request_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                envelope_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                ledger_revision = table.Column<long>(type: "INTEGER", nullable: false),
                commitment_kind = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                committed_units = table.Column<long>(type: "INTEGER", nullable: false),
                usage_evidence_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                provider_outcome_code = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                provider_duration_milliseconds = table.Column<long>(type: "INTEGER", nullable: true),
                occurred_at_utc = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                commitment_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_provider_budget_commitments", x => x.provider_request_id);
                table.CheckConstraint("ck_provider_budget_commitments_amount", "committed_units >= 0");
                table.CheckConstraint("ck_provider_budget_commitments_digest", "length(commitment_sha256) = 64 AND commitment_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_commitments_duration", "provider_duration_milliseconds IS NULL OR provider_duration_milliseconds BETWEEN 0 AND 86400000");
                table.CheckConstraint("ck_provider_budget_commitments_kind", "commitment_kind IN ('Observed', 'IndeterminateMaximum', 'OverrunMaximum')");
                table.CheckConstraint("ck_provider_budget_commitments_occurred_utc", "length(occurred_at_utc) = 33 AND substr(occurred_at_utc, -6) = '+00:00'");
                table.CheckConstraint("ck_provider_budget_commitments_outcome", "length(provider_outcome_code) BETWEEN 1 AND 128 AND provider_outcome_code GLOB '[A-Za-z0-9]*' AND provider_outcome_code NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_commitments_usage", "length(usage_evidence_sha256) = 64 AND usage_evidence_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.ForeignKey(
                    name: "FK_provider_budget_commitments_provider_budget_ledger_revisions_envelope_id_ledger_revision",
                    columns: x => new { x.envelope_id, x.ledger_revision },
                    principalTable: "provider_budget_ledger_revisions",
                    principalColumns: EnvelopeLedgerColumns,
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_provider_budget_commitments_provider_budget_reservations_provider_request_id",
                    column: x => x.provider_request_id,
                    principalTable: "provider_budget_reservations",
                    principalColumn: "provider_request_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "provider_budget_reconciliation_dispositions",
            columns: table => new
            {
                disposition_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                provider_request_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                envelope_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                ledger_revision = table.Column<long>(type: "INTEGER", nullable: false),
                disposition_kind = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                confirmed_charge_units = table.Column<long>(type: "INTEGER", nullable: false),
                restored_units = table.Column<long>(type: "INTEGER", nullable: false),
                authority_reference = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                actor_reference = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                evidence_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                occurred_at_utc = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                disposition_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_provider_budget_reconciliation_dispositions", x => x.disposition_id);
                table.CheckConstraint("ck_provider_budget_reconciliation_dispositions_actor", "length(actor_reference) BETWEEN 1 AND 128 AND actor_reference GLOB '[A-Za-z0-9]*' AND actor_reference NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_reconciliation_dispositions_amounts", "confirmed_charge_units >= 0 AND restored_units >= 0 AND (disposition_kind <> 'ConfirmedNoCharge' OR confirmed_charge_units = 0) AND (disposition_kind <> 'ConfirmedMaximum' OR restored_units = 0)");
                table.CheckConstraint("ck_provider_budget_reconciliation_dispositions_authority", "length(authority_reference) BETWEEN 1 AND 128 AND authority_reference GLOB '[A-Za-z0-9]*' AND authority_reference NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_reconciliation_dispositions_digest", "length(disposition_sha256) = 64 AND disposition_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_reconciliation_dispositions_evidence", "length(evidence_sha256) = 64 AND evidence_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_reconciliation_dispositions_id", "length(disposition_id) BETWEEN 1 AND 128 AND disposition_id GLOB '[A-Za-z0-9]*' AND disposition_id NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_reconciliation_dispositions_kind", "disposition_kind IN ('ConfirmedNoCharge', 'ConfirmedCharge', 'ConfirmedMaximum')");
                table.CheckConstraint("ck_provider_budget_reconciliation_dispositions_occurred_utc", "length(occurred_at_utc) = 33 AND substr(occurred_at_utc, -6) = '+00:00'");
                table.ForeignKey(
                    name: "FK_provider_budget_reconciliation_dispositions_provider_budget_ledger_revisions_envelope_id_ledger_revision",
                    columns: x => new { x.envelope_id, x.ledger_revision },
                    principalTable: "provider_budget_ledger_revisions",
                    principalColumns: EnvelopeLedgerColumns,
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_provider_budget_reconciliation_dispositions_provider_budget_reservations_provider_request_id",
                    column: x => x.provider_request_id,
                    principalTable: "provider_budget_reservations",
                    principalColumn: "provider_request_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "provider_budget_releases",
            columns: table => new
            {
                provider_request_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                envelope_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                ledger_revision = table.Column<long>(type: "INTEGER", nullable: false),
                proof_kind = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                proof_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                authority_reference = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                occurred_at_utc = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                release_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_provider_budget_releases", x => x.provider_request_id);
                table.CheckConstraint("ck_provider_budget_releases_authority", "length(authority_reference) BETWEEN 1 AND 128 AND authority_reference GLOB '[A-Za-z0-9]*' AND authority_reference NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_releases_digest", "length(release_sha256) = 64 AND release_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_releases_occurred_utc", "length(occurred_at_utc) = 33 AND substr(occurred_at_utc, -6) = '+00:00'");
                table.CheckConstraint("ck_provider_budget_releases_proof", "length(proof_sha256) = 64 AND proof_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_releases_proof_kind", "proof_kind IN ('BeforeCredentialLookup', 'TransportConfirmedZeroRequestBytes')");
                table.ForeignKey(
                    name: "FK_provider_budget_releases_provider_budget_ledger_revisions_envelope_id_ledger_revision",
                    columns: x => new { x.envelope_id, x.ledger_revision },
                    principalTable: "provider_budget_ledger_revisions",
                    principalColumns: EnvelopeLedgerColumns,
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_provider_budget_releases_provider_budget_reservations_provider_request_id",
                    column: x => x.provider_request_id,
                    principalTable: "provider_budget_reservations",
                    principalColumn: "provider_request_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "provider_budget_reservation_transitions",
            columns: table => new
            {
                provider_request_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                reservation_revision = table.Column<long>(type: "INTEGER", nullable: false),
                envelope_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                ledger_revision = table.Column<long>(type: "INTEGER", nullable: false),
                from_status = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                to_status = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                transition_kind = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                proof_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                outcome_code = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                occurred_at_utc = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                previous_transition_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                transition_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_provider_budget_reservation_transitions", x => new { x.provider_request_id, x.reservation_revision });
                table.CheckConstraint("ck_provider_budget_reservation_transitions_digest", "length(transition_sha256) = 64 AND transition_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_reservation_transitions_from", "from_status IS NULL OR from_status IN ('Reserved', 'DispatchStarted', 'Committed', 'ReleasedPreSend', 'IndeterminateCommitted', 'OverrunCommitted')");
                table.CheckConstraint("ck_provider_budget_reservation_transitions_kind", "transition_kind IN ('Admission', 'DispatchStarted', 'ObservedCommitted', 'IndeterminateCommitted', 'OverrunCommitted', 'PreSendReleased')");
                table.CheckConstraint("ck_provider_budget_reservation_transitions_occurred_utc", "length(occurred_at_utc) = 33 AND substr(occurred_at_utc, -6) = '+00:00'");
                table.CheckConstraint("ck_provider_budget_reservation_transitions_outcome", "outcome_code IS NULL OR length(outcome_code) BETWEEN 1 AND 128 AND outcome_code GLOB '[A-Za-z0-9]*' AND outcome_code NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_provider_budget_reservation_transitions_previous_digest", "length(previous_transition_sha256) = 64 AND previous_transition_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_reservation_transitions_proof", "proof_sha256 IS NULL OR length(proof_sha256) = 64 AND proof_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_provider_budget_reservation_transitions_revision", "reservation_revision > 0");
                table.CheckConstraint("ck_provider_budget_reservation_transitions_shape", "(reservation_revision = 1 AND from_status IS NULL AND to_status = 'Reserved' AND transition_kind = 'Admission') OR (reservation_revision > 1 AND from_status IS NOT NULL)");
                table.CheckConstraint("ck_provider_budget_reservation_transitions_to", "to_status IN ('Reserved', 'DispatchStarted', 'Committed', 'ReleasedPreSend', 'IndeterminateCommitted', 'OverrunCommitted')");
                table.ForeignKey(
                    name: "FK_provider_budget_reservation_transitions_provider_budget_ledger_revisions_envelope_id_ledger_revision",
                    columns: x => new { x.envelope_id, x.ledger_revision },
                    principalTable: "provider_budget_ledger_revisions",
                    principalColumns: EnvelopeLedgerColumns,
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_provider_budget_reservation_transitions_provider_budget_reservations_provider_request_id",
                    column: x => x.provider_request_id,
                    principalTable: "provider_budget_reservations",
                    principalColumn: "provider_request_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_provider_budget_audit_events_envelope_id_ledger_revision",
            table: "provider_budget_audit_events",
            columns: EnvelopeLedgerColumns);

        migrationBuilder.CreateIndex(
            name: "IX_provider_budget_audit_events_envelope_id_occurred_at_utc",
            table: "provider_budget_audit_events",
            columns: EnvelopeOccurredColumns);

        migrationBuilder.CreateIndex(
            name: "IX_provider_budget_commitments_envelope_id_ledger_revision",
            table: "provider_budget_commitments",
            columns: EnvelopeLedgerColumns);

        migrationBuilder.CreateIndex(
            name: "IX_provider_budget_configurations_envelope_id_configuration_sha256",
            table: "provider_budget_configurations",
            columns: EnvelopeConfigurationDigestColumns,
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_provider_budget_control_heads_current_store_epoch_id",
            table: "provider_budget_control_heads",
            column: "current_store_epoch_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_provider_budget_envelopes_current_store_epoch_id",
            table: "provider_budget_envelopes",
            column: "current_store_epoch_id");

        migrationBuilder.CreateIndex(
            name: "IX_provider_budget_envelopes_environment_id_provider_id_billing_scope_reference_model_id_currency_code_accounting_unit_id",
            table: "provider_budget_envelopes",
            columns: EnvelopeScopeColumns,
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_provider_budget_ledger_revisions_envelope_id_configuration_revision",
            table: "provider_budget_ledger_revisions",
            columns: EnvelopeConfigurationColumns);

        migrationBuilder.CreateIndex(
            name: "IX_provider_budget_ledger_revisions_envelope_id_ledger_sha256",
            table: "provider_budget_ledger_revisions",
            columns: EnvelopeLedgerDigestColumns,
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_provider_budget_ledger_revisions_store_epoch_id_envelope_id_ledger_revision",
            table: "provider_budget_ledger_revisions",
            columns: StoreEpochEnvelopeLedgerColumns);

        migrationBuilder.CreateIndex(
            name: "IX_provider_budget_operation_balance_revisions_envelope_id_configuration_revision_operation_class",
            table: "provider_budget_operation_balance_revisions",
            columns: EnvelopeConfigurationOperationColumns);

        migrationBuilder.CreateIndex(
            name: "IX_provider_budget_rearms_envelope_id_resulting_ledger_revision",
            table: "provider_budget_rearms",
            columns: EnvelopeResultingLedgerColumns);

        migrationBuilder.CreateIndex(
            name: "IX_provider_budget_rearms_envelope_id_store_epoch_id_new_runtime_session_id",
            table: "provider_budget_rearms",
            columns: EnvelopeStoreEpochSessionColumns,
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_provider_budget_rearms_store_epoch_id",
            table: "provider_budget_rearms",
            column: "store_epoch_id");

        migrationBuilder.CreateIndex(
            name: "IX_provider_budget_reconciliation_dispositions_envelope_id_ledger_revision",
            table: "provider_budget_reconciliation_dispositions",
            columns: EnvelopeLedgerColumns);

        migrationBuilder.CreateIndex(
            name: "IX_provider_budget_reconciliation_dispositions_provider_request_id",
            table: "provider_budget_reconciliation_dispositions",
            column: "provider_request_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_provider_budget_releases_envelope_id_ledger_revision",
            table: "provider_budget_releases",
            columns: EnvelopeLedgerColumns);

        migrationBuilder.CreateIndex(
            name: "IX_provider_budget_reservation_transitions_envelope_id_ledger_revision",
            table: "provider_budget_reservation_transitions",
            columns: EnvelopeLedgerColumns);

        migrationBuilder.CreateIndex(
            name: "IX_provider_budget_reservations_envelope_id_admission_ledger_revision",
            table: "provider_budget_reservations",
            columns: EnvelopeAdmissionLedgerColumns);

        migrationBuilder.CreateIndex(
            name: "IX_provider_budget_reservations_envelope_id_configuration_revision",
            table: "provider_budget_reservations",
            columns: EnvelopeConfigurationColumns);

        migrationBuilder.CreateIndex(
            name: "IX_provider_budget_reservations_envelope_id_status",
            table: "provider_budget_reservations",
            columns: EnvelopeStatusColumns);

        migrationBuilder.CreateIndex(
            name: "IX_provider_budget_reservations_store_epoch_id",
            table: "provider_budget_reservations",
            column: "store_epoch_id");

        migrationBuilder.CreateIndex(
            name: "IX_provider_budget_store_epochs_epoch_revision",
            table: "provider_budget_store_epochs",
            column: "epoch_revision",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_provider_budget_store_epochs_epoch_sha256",
            table: "provider_budget_store_epochs",
            column: "epoch_sha256",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_provider_budget_store_epochs_previous_store_epoch_id",
            table: "provider_budget_store_epochs",
            column: "previous_store_epoch_id");

        CreateProviderBudgetTriggers(migrationBuilder);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        GuardEmptyProviderBudgetRollback(migrationBuilder);

        migrationBuilder.DropTable(
            name: "provider_budget_audit_events");

        migrationBuilder.DropTable(
            name: "provider_budget_commitments");

        migrationBuilder.DropTable(
            name: "provider_budget_control_heads");

        migrationBuilder.DropTable(
            name: "provider_budget_operation_balance_revisions");

        migrationBuilder.DropTable(
            name: "provider_budget_rearms");

        migrationBuilder.DropTable(
            name: "provider_budget_reconciliation_dispositions");

        migrationBuilder.DropTable(
            name: "provider_budget_releases");

        migrationBuilder.DropTable(
            name: "provider_budget_reservation_transitions");

        migrationBuilder.DropTable(
            name: "provider_budget_operation_allocations");

        migrationBuilder.DropTable(
            name: "provider_budget_reservations");

        migrationBuilder.DropTable(
            name: "provider_budget_ledger_revisions");

        migrationBuilder.DropTable(
            name: "provider_budget_configurations");

        migrationBuilder.DropTable(
            name: "provider_budget_envelopes");

        migrationBuilder.DropTable(
            name: "provider_budget_store_epochs");
    }

    private static void CreateProviderBudgetTriggers(MigrationBuilder migrationBuilder)
    {
        CreateStoreAndEnvelopeTriggers(migrationBuilder);
        CreateConfigurationAndLedgerTriggers(migrationBuilder);
        CreateReservationTriggers(migrationBuilder);
        CreateEvidenceAndAuditTriggers(migrationBuilder);
    }

    private static void CreateStoreAndEnvelopeTriggers(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
                CREATE TRIGGER trg_provider_budget_store_epochs_append_insert
                BEFORE INSERT ON provider_budget_store_epochs
                BEGIN
                    SELECT CASE WHEN
                        (NEW.epoch_revision = 1 AND
                            (EXISTS (SELECT 1 FROM provider_budget_store_epochs) OR
                             NEW.previous_epoch_sha256 <> printf('%064d', 0))) OR
                        (NEW.epoch_revision > 1 AND NOT EXISTS (
                            SELECT 1
                            FROM provider_budget_store_epochs AS previous
                            WHERE previous.store_epoch_id = NEW.previous_store_epoch_id
                              AND previous.epoch_revision = NEW.epoch_revision - 1
                              AND previous.epoch_sha256 = NEW.previous_epoch_sha256)) OR
                        EXISTS (
                            SELECT 1
                            FROM provider_budget_store_epochs AS existing
                            WHERE existing.epoch_revision >= NEW.epoch_revision)
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_EPOCH_CONFLICT') END;
                END;

                CREATE TRIGGER trg_provider_budget_store_epochs_immutable_update
                BEFORE UPDATE ON provider_budget_store_epochs
                BEGIN
                    SELECT RAISE(ABORT, 'CH_PROVIDER_BUDGET_APPEND_ONLY');
                END;

                CREATE TRIGGER trg_provider_budget_store_epochs_immutable_delete
                BEFORE DELETE ON provider_budget_store_epochs
                BEGIN
                    SELECT RAISE(ABORT, 'CH_PROVIDER_BUDGET_APPEND_ONLY');
                END;

                CREATE TRIGGER trg_provider_budget_control_heads_validate_insert
                BEFORE INSERT ON provider_budget_control_heads
                BEGIN
                    SELECT CASE WHEN NEW.row_revision <> 1 OR NOT EXISTS (
                        SELECT 1
                        FROM provider_budget_store_epochs AS epoch
                        WHERE epoch.store_epoch_id = NEW.current_store_epoch_id
                          AND epoch.epoch_revision = NEW.epoch_revision)
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_CONTROL_CONFLICT') END;
                END;

                CREATE TRIGGER trg_provider_budget_control_heads_validate_update
                BEFORE UPDATE ON provider_budget_control_heads
                BEGIN
                    SELECT CASE WHEN
                        NEW.control_id IS NOT OLD.control_id OR
                        NEW.row_revision <> OLD.row_revision + 1 OR
                        NEW.epoch_revision <> OLD.epoch_revision + 1 OR
                        NOT EXISTS (
                            SELECT 1
                            FROM provider_budget_store_epochs AS epoch
                            WHERE epoch.store_epoch_id = NEW.current_store_epoch_id
                              AND epoch.epoch_revision = NEW.epoch_revision
                              AND epoch.previous_store_epoch_id = OLD.current_store_epoch_id)
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_CONTROL_CONFLICT') END;
                END;

                CREATE TRIGGER trg_provider_budget_control_heads_immutable_delete
                BEFORE DELETE ON provider_budget_control_heads
                BEGIN
                    SELECT RAISE(ABORT, 'CH_PROVIDER_BUDGET_APPEND_ONLY');
                END;

                CREATE TRIGGER trg_provider_budget_envelopes_uninitialised_insert
                BEFORE INSERT ON provider_budget_envelopes
                BEGIN
                    SELECT CASE WHEN
                        NEW.is_initialised <> 0 OR
                        NEW.current_configuration_revision <> 1 OR
                        NEW.current_ledger_revision <> 1 OR
                        NEW.current_rearm_revision <> 0 OR
                        NOT EXISTS (
                            SELECT 1
                            FROM provider_budget_control_heads AS head
                            WHERE head.current_store_epoch_id = NEW.current_store_epoch_id)
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_ENVELOPE_NOT_READY') END;
                END;

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
                        NEW.aggregate_limit_units IS OLD.aggregate_limit_units AND
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

                CREATE TRIGGER trg_provider_budget_envelopes_immutable_delete
                BEFORE DELETE ON provider_budget_envelopes
                BEGIN
                    SELECT RAISE(ABORT, 'CH_PROVIDER_BUDGET_APPEND_ONLY');
                END;
                """);
    }

    private static void CreateConfigurationAndLedgerTriggers(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
                CREATE TRIGGER trg_provider_budget_configurations_append_insert
                BEFORE INSERT ON provider_budget_configurations
                BEGIN
                    SELECT CASE WHEN
                        (NEW.configuration_revision = 1 AND EXISTS (
                            SELECT 1 FROM provider_budget_configurations
                            WHERE envelope_id = NEW.envelope_id)) OR
                        (NEW.configuration_revision > 1 AND NOT EXISTS (
                            SELECT 1
                            FROM provider_budget_configurations AS previous
                            WHERE previous.envelope_id = NEW.envelope_id
                              AND previous.configuration_revision = NEW.configuration_revision - 1
                              AND previous.sealed_at_utc IS NOT NULL)) OR
                        EXISTS (
                            SELECT 1
                            FROM provider_budget_configurations AS existing
                            WHERE existing.envelope_id = NEW.envelope_id
                              AND existing.configuration_revision >= NEW.configuration_revision)
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_CONFIGURATION_CONFLICT') END;
                END;

                CREATE TRIGGER trg_provider_budget_configurations_seal_update
                BEFORE UPDATE ON provider_budget_configurations
                BEGIN
                    SELECT CASE WHEN NOT (
                        OLD.sealed_at_utc IS NULL AND
                        NEW.sealed_at_utc IS NOT NULL AND
                        NEW.envelope_id IS OLD.envelope_id AND
                        NEW.configuration_revision IS OLD.configuration_revision AND
                        NEW.previous_configuration_revision IS OLD.previous_configuration_revision AND
                        NEW.cost_schedule_id IS OLD.cost_schedule_id AND
                        NEW.cost_schedule_sha256 IS OLD.cost_schedule_sha256 AND
                        NEW.aggregate_limit_units IS OLD.aggregate_limit_units AND
                        NEW.effective_at_utc IS OLD.effective_at_utc AND
                        NEW.expires_at_utc IS OLD.expires_at_utc AND
                        NEW.configuration_authority_reference IS OLD.configuration_authority_reference AND
                        NEW.created_at_utc IS OLD.created_at_utc AND
                        NEW.configuration_sha256 IS OLD.configuration_sha256)
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_CONFIGURATION_IMMUTABLE') END;

                    SELECT CASE WHEN (
                        SELECT COUNT(*)
                        FROM provider_budget_operation_allocations AS allocation
                        WHERE allocation.envelope_id = NEW.envelope_id
                          AND allocation.configuration_revision = NEW.configuration_revision) <> 3
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_ALLOCATION_INCOMPLETE') END;

                    SELECT CASE WHEN NOT (
                        NEW.aggregate_limit_units >= COALESCE((
                            SELECT allocation_limit_units
                            FROM provider_budget_operation_allocations
                            WHERE envelope_id = NEW.envelope_id
                              AND configuration_revision = NEW.configuration_revision
                              AND operation_class = 'AdministrativeIndexEmbedding'), -1) AND
                        NEW.aggregate_limit_units - COALESCE((
                            SELECT allocation_limit_units
                            FROM provider_budget_operation_allocations
                            WHERE envelope_id = NEW.envelope_id
                              AND configuration_revision = NEW.configuration_revision
                              AND operation_class = 'AdministrativeIndexEmbedding'), -1) >= COALESCE((
                            SELECT allocation_limit_units
                            FROM provider_budget_operation_allocations
                            WHERE envelope_id = NEW.envelope_id
                              AND configuration_revision = NEW.configuration_revision
                              AND operation_class = 'QueryEmbedding'), -1) AND
                        NEW.aggregate_limit_units - COALESCE((
                            SELECT allocation_limit_units
                            FROM provider_budget_operation_allocations
                            WHERE envelope_id = NEW.envelope_id
                              AND configuration_revision = NEW.configuration_revision
                              AND operation_class = 'AdministrativeIndexEmbedding'), -1) - COALESCE((
                            SELECT allocation_limit_units
                            FROM provider_budget_operation_allocations
                            WHERE envelope_id = NEW.envelope_id
                              AND configuration_revision = NEW.configuration_revision
                              AND operation_class = 'QueryEmbedding'), -1) >= COALESCE((
                            SELECT allocation_limit_units
                            FROM provider_budget_operation_allocations
                            WHERE envelope_id = NEW.envelope_id
                              AND configuration_revision = NEW.configuration_revision
                              AND operation_class = 'GroundedGeneration'), -1))
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_ALLOCATION_LIMIT') END;
                END;

                CREATE TRIGGER trg_provider_budget_configurations_immutable_delete
                BEFORE DELETE ON provider_budget_configurations
                BEGIN
                    SELECT RAISE(ABORT, 'CH_PROVIDER_BUDGET_APPEND_ONLY');
                END;

                CREATE TRIGGER trg_provider_budget_operation_allocations_unsealed_insert
                BEFORE INSERT ON provider_budget_operation_allocations
                BEGIN
                    SELECT CASE WHEN NOT EXISTS (
                        SELECT 1
                        FROM provider_budget_configurations AS configuration
                        WHERE configuration.envelope_id = NEW.envelope_id
                          AND configuration.configuration_revision = NEW.configuration_revision
                          AND configuration.sealed_at_utc IS NULL)
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_CONFIGURATION_SEALED') END;
                END;

                CREATE TRIGGER trg_provider_budget_operation_allocations_sealed_update
                BEFORE UPDATE ON provider_budget_operation_allocations
                WHEN EXISTS (
                    SELECT 1
                    FROM provider_budget_configurations AS configuration
                    WHERE configuration.envelope_id = OLD.envelope_id
                      AND configuration.configuration_revision = OLD.configuration_revision
                      AND configuration.sealed_at_utc IS NOT NULL)
                BEGIN
                    SELECT RAISE(ABORT, 'CH_PROVIDER_BUDGET_CONFIGURATION_SEALED');
                END;

                CREATE TRIGGER trg_provider_budget_operation_allocations_sealed_delete
                BEFORE DELETE ON provider_budget_operation_allocations
                WHEN EXISTS (
                    SELECT 1
                    FROM provider_budget_configurations AS configuration
                    WHERE configuration.envelope_id = OLD.envelope_id
                      AND configuration.configuration_revision = OLD.configuration_revision
                      AND configuration.sealed_at_utc IS NOT NULL)
                BEGIN
                    SELECT RAISE(ABORT, 'CH_PROVIDER_BUDGET_CONFIGURATION_SEALED');
                END;

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
                            NEW.aggregate_limit_units = 0 AND
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

                CREATE TRIGGER trg_provider_budget_ledger_revisions_complete_update
                BEFORE UPDATE ON provider_budget_ledger_revisions
                BEGIN
                    SELECT CASE WHEN NOT (
                        OLD.is_complete = 0 AND NEW.is_complete = 1 AND
                        NEW.envelope_id IS OLD.envelope_id AND
                        NEW.ledger_revision IS OLD.ledger_revision AND
                        NEW.store_epoch_id IS OLD.store_epoch_id AND
                        NEW.previous_ledger_revision IS OLD.previous_ledger_revision AND
                        NEW.configuration_revision IS OLD.configuration_revision AND
                        NEW.rearm_revision IS OLD.rearm_revision AND
                        NEW.state IS OLD.state AND
                        NEW.runtime_session_id IS OLD.runtime_session_id AND
                        NEW.aggregate_limit_units IS OLD.aggregate_limit_units AND
                        NEW.aggregate_committed_units IS OLD.aggregate_committed_units AND
                        NEW.aggregate_reserved_units IS OLD.aggregate_reserved_units AND
                        NEW.aggregate_indeterminate_units IS OLD.aggregate_indeterminate_units AND
                        NEW.transition_kind IS OLD.transition_kind AND
                        NEW.provider_request_id IS OLD.provider_request_id AND
                        NEW.transition_authority_reference IS OLD.transition_authority_reference AND
                        NEW.occurred_at_utc IS OLD.occurred_at_utc AND
                        NEW.previous_ledger_sha256 IS OLD.previous_ledger_sha256 AND
                        NEW.ledger_sha256 IS OLD.ledger_sha256)
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_LEDGER_IMMUTABLE') END;

                    SELECT CASE WHEN (
                        SELECT COUNT(*)
                        FROM provider_budget_operation_balance_revisions AS balance
                        WHERE balance.envelope_id = NEW.envelope_id
                          AND balance.ledger_revision = NEW.ledger_revision) <> 3
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_BALANCE_INCOMPLETE') END;

                    SELECT CASE WHEN EXISTS (
                        SELECT 1
                        FROM provider_budget_operation_balance_revisions AS balance
                        LEFT JOIN provider_budget_operation_allocations AS allocation
                          ON allocation.envelope_id = balance.envelope_id
                         AND allocation.configuration_revision = balance.configuration_revision
                         AND allocation.operation_class = balance.operation_class
                        WHERE balance.envelope_id = NEW.envelope_id
                          AND balance.ledger_revision = NEW.ledger_revision
                          AND (balance.configuration_revision <> NEW.configuration_revision OR
                               allocation.allocation_limit_units IS NULL OR
                               allocation.allocation_limit_units <> balance.allocation_limit_units))
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_BALANCE_BINDING_CONFLICT') END;

                    SELECT CASE WHEN NOT EXISTS (
                        SELECT 1
                        FROM provider_budget_configurations AS configuration
                        WHERE configuration.envelope_id = NEW.envelope_id
                          AND configuration.configuration_revision = NEW.configuration_revision
                          AND configuration.sealed_at_utc IS NOT NULL
                          AND configuration.aggregate_limit_units = NEW.aggregate_limit_units)
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_CONFIGURATION_CONFLICT') END;

                    SELECT CASE WHEN NOT (
                        NEW.aggregate_limit_units >= COALESCE((
                            SELECT allocation_limit_units FROM provider_budget_operation_balance_revisions
                            WHERE envelope_id = NEW.envelope_id AND ledger_revision = NEW.ledger_revision
                              AND operation_class = 'AdministrativeIndexEmbedding'), -1) AND
                        NEW.aggregate_limit_units - COALESCE((
                            SELECT allocation_limit_units FROM provider_budget_operation_balance_revisions
                            WHERE envelope_id = NEW.envelope_id AND ledger_revision = NEW.ledger_revision
                              AND operation_class = 'AdministrativeIndexEmbedding'), -1) >= COALESCE((
                            SELECT allocation_limit_units FROM provider_budget_operation_balance_revisions
                            WHERE envelope_id = NEW.envelope_id AND ledger_revision = NEW.ledger_revision
                              AND operation_class = 'QueryEmbedding'), -1) AND
                        NEW.aggregate_limit_units - COALESCE((
                            SELECT allocation_limit_units FROM provider_budget_operation_balance_revisions
                            WHERE envelope_id = NEW.envelope_id AND ledger_revision = NEW.ledger_revision
                              AND operation_class = 'AdministrativeIndexEmbedding'), -1) - COALESCE((
                            SELECT allocation_limit_units FROM provider_budget_operation_balance_revisions
                            WHERE envelope_id = NEW.envelope_id AND ledger_revision = NEW.ledger_revision
                              AND operation_class = 'QueryEmbedding'), -1) >= COALESCE((
                            SELECT allocation_limit_units FROM provider_budget_operation_balance_revisions
                            WHERE envelope_id = NEW.envelope_id AND ledger_revision = NEW.ledger_revision
                              AND operation_class = 'GroundedGeneration'), -1))
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_BALANCE_LIMIT') END;

                    SELECT CASE WHEN NOT (
                        NEW.aggregate_committed_units >= COALESCE((
                            SELECT committed_units FROM provider_budget_operation_balance_revisions
                            WHERE envelope_id = NEW.envelope_id AND ledger_revision = NEW.ledger_revision
                              AND operation_class = 'AdministrativeIndexEmbedding'), -1) AND
                        NEW.aggregate_committed_units - COALESCE((
                            SELECT committed_units FROM provider_budget_operation_balance_revisions
                            WHERE envelope_id = NEW.envelope_id AND ledger_revision = NEW.ledger_revision
                              AND operation_class = 'AdministrativeIndexEmbedding'), -1) >= COALESCE((
                            SELECT committed_units FROM provider_budget_operation_balance_revisions
                            WHERE envelope_id = NEW.envelope_id AND ledger_revision = NEW.ledger_revision
                              AND operation_class = 'QueryEmbedding'), -1) AND
                        NEW.aggregate_committed_units - COALESCE((
                            SELECT committed_units FROM provider_budget_operation_balance_revisions
                            WHERE envelope_id = NEW.envelope_id AND ledger_revision = NEW.ledger_revision
                              AND operation_class = 'AdministrativeIndexEmbedding'), -1) - COALESCE((
                            SELECT committed_units FROM provider_budget_operation_balance_revisions
                            WHERE envelope_id = NEW.envelope_id AND ledger_revision = NEW.ledger_revision
                              AND operation_class = 'QueryEmbedding'), -1) = COALESCE((
                            SELECT committed_units FROM provider_budget_operation_balance_revisions
                            WHERE envelope_id = NEW.envelope_id AND ledger_revision = NEW.ledger_revision
                              AND operation_class = 'GroundedGeneration'), -1))
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_BALANCE_COMMITTED') END;

                    SELECT CASE WHEN NOT (
                        NEW.aggregate_reserved_units >= COALESCE((
                            SELECT reserved_units FROM provider_budget_operation_balance_revisions
                            WHERE envelope_id = NEW.envelope_id AND ledger_revision = NEW.ledger_revision
                              AND operation_class = 'AdministrativeIndexEmbedding'), -1) AND
                        NEW.aggregate_reserved_units - COALESCE((
                            SELECT reserved_units FROM provider_budget_operation_balance_revisions
                            WHERE envelope_id = NEW.envelope_id AND ledger_revision = NEW.ledger_revision
                              AND operation_class = 'AdministrativeIndexEmbedding'), -1) >= COALESCE((
                            SELECT reserved_units FROM provider_budget_operation_balance_revisions
                            WHERE envelope_id = NEW.envelope_id AND ledger_revision = NEW.ledger_revision
                              AND operation_class = 'QueryEmbedding'), -1) AND
                        NEW.aggregate_reserved_units - COALESCE((
                            SELECT reserved_units FROM provider_budget_operation_balance_revisions
                            WHERE envelope_id = NEW.envelope_id AND ledger_revision = NEW.ledger_revision
                              AND operation_class = 'AdministrativeIndexEmbedding'), -1) - COALESCE((
                            SELECT reserved_units FROM provider_budget_operation_balance_revisions
                            WHERE envelope_id = NEW.envelope_id AND ledger_revision = NEW.ledger_revision
                              AND operation_class = 'QueryEmbedding'), -1) = COALESCE((
                            SELECT reserved_units FROM provider_budget_operation_balance_revisions
                            WHERE envelope_id = NEW.envelope_id AND ledger_revision = NEW.ledger_revision
                              AND operation_class = 'GroundedGeneration'), -1))
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_BALANCE_RESERVED') END;

                    SELECT CASE WHEN NOT (
                        NEW.aggregate_indeterminate_units >= COALESCE((
                            SELECT indeterminate_units FROM provider_budget_operation_balance_revisions
                            WHERE envelope_id = NEW.envelope_id AND ledger_revision = NEW.ledger_revision
                              AND operation_class = 'AdministrativeIndexEmbedding'), -1) AND
                        NEW.aggregate_indeterminate_units - COALESCE((
                            SELECT indeterminate_units FROM provider_budget_operation_balance_revisions
                            WHERE envelope_id = NEW.envelope_id AND ledger_revision = NEW.ledger_revision
                              AND operation_class = 'AdministrativeIndexEmbedding'), -1) >= COALESCE((
                            SELECT indeterminate_units FROM provider_budget_operation_balance_revisions
                            WHERE envelope_id = NEW.envelope_id AND ledger_revision = NEW.ledger_revision
                              AND operation_class = 'QueryEmbedding'), -1) AND
                        NEW.aggregate_indeterminate_units - COALESCE((
                            SELECT indeterminate_units FROM provider_budget_operation_balance_revisions
                            WHERE envelope_id = NEW.envelope_id AND ledger_revision = NEW.ledger_revision
                              AND operation_class = 'AdministrativeIndexEmbedding'), -1) - COALESCE((
                            SELECT indeterminate_units FROM provider_budget_operation_balance_revisions
                            WHERE envelope_id = NEW.envelope_id AND ledger_revision = NEW.ledger_revision
                              AND operation_class = 'QueryEmbedding'), -1) = COALESCE((
                            SELECT indeterminate_units FROM provider_budget_operation_balance_revisions
                            WHERE envelope_id = NEW.envelope_id AND ledger_revision = NEW.ledger_revision
                              AND operation_class = 'GroundedGeneration'), -1))
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_BALANCE_INDETERMINATE') END;
                END;

                CREATE TRIGGER trg_provider_budget_ledger_revisions_immutable_delete
                BEFORE DELETE ON provider_budget_ledger_revisions
                BEGIN
                    SELECT RAISE(ABORT, 'CH_PROVIDER_BUDGET_APPEND_ONLY');
                END;

                CREATE TRIGGER trg_provider_budget_operation_balances_incomplete_insert
                BEFORE INSERT ON provider_budget_operation_balance_revisions
                BEGIN
                    SELECT CASE WHEN NOT EXISTS (
                        SELECT 1
                        FROM provider_budget_ledger_revisions AS ledger
                        WHERE ledger.envelope_id = NEW.envelope_id
                          AND ledger.ledger_revision = NEW.ledger_revision
                          AND ledger.is_complete = 0)
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_LEDGER_COMPLETE') END;
                END;

                CREATE TRIGGER trg_provider_budget_operation_balances_immutable_update
                BEFORE UPDATE ON provider_budget_operation_balance_revisions
                BEGIN
                    SELECT RAISE(ABORT, 'CH_PROVIDER_BUDGET_APPEND_ONLY');
                END;

                CREATE TRIGGER trg_provider_budget_operation_balances_immutable_delete
                BEFORE DELETE ON provider_budget_operation_balance_revisions
                BEGIN
                    SELECT RAISE(ABORT, 'CH_PROVIDER_BUDGET_APPEND_ONLY');
                END;
                """);
    }

    private static void CreateReservationTriggers(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
                CREATE TRIGGER trg_provider_budget_reservations_uninitialised_insert
                BEFORE INSERT ON provider_budget_reservations
                BEGIN
                    SELECT CASE WHEN
                        NEW.is_initialised <> 0 OR
                        NEW.current_reservation_revision <> 1 OR
                        NOT EXISTS (
                            SELECT 1
                            FROM provider_budget_ledger_revisions AS ledger
                            WHERE ledger.envelope_id = NEW.envelope_id
                              AND ledger.ledger_revision = NEW.admission_ledger_revision
                              AND ledger.store_epoch_id = NEW.store_epoch_id
                              AND ledger.configuration_revision = NEW.configuration_revision
                              AND ledger.provider_request_id = NEW.provider_request_id
                              AND ledger.transition_kind = 'ReservationAdmitted'
                              AND ledger.is_complete = 1)
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_RESERVATION_NOT_READY') END;
                END;

                CREATE TRIGGER trg_provider_budget_reservations_validate_update
                BEFORE UPDATE ON provider_budget_reservations
                BEGIN
                    SELECT CASE WHEN NOT (
                        NEW.provider_request_id IS OLD.provider_request_id AND
                        NEW.envelope_id IS OLD.envelope_id AND
                        NEW.store_epoch_id IS OLD.store_epoch_id AND
                        NEW.configuration_revision IS OLD.configuration_revision AND
                        NEW.operation_class IS OLD.operation_class AND
                        NEW.operation_authority_reference IS OLD.operation_authority_reference AND
                        NEW.request_plan_sha256 IS OLD.request_plan_sha256 AND
                        NEW.request_sha256 IS OLD.request_sha256 AND
                        NEW.maximum_charge_basis_sha256 IS OLD.maximum_charge_basis_sha256 AND
                        NEW.cost_schedule_sha256 IS OLD.cost_schedule_sha256 AND
                        NEW.binding_sha256 IS OLD.binding_sha256 AND
                        NEW.maximum_charge_units IS OLD.maximum_charge_units AND
                        NEW.admitted_runtime_session_id IS OLD.admitted_runtime_session_id AND
                        NEW.admission_ledger_revision IS OLD.admission_ledger_revision AND
                        NEW.admitted_at_utc IS OLD.admitted_at_utc)
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_RESERVATION_IMMUTABLE') END;

                    SELECT CASE WHEN OLD.is_initialised = 0 AND NOT (
                        NEW.is_initialised = 1 AND
                        NEW.current_reservation_revision = 1 AND
                        NEW.status IS OLD.status AND
                        NEW.dispatch_started_at_utc IS OLD.dispatch_started_at_utc AND
                        NEW.terminal_at_utc IS OLD.terminal_at_utc AND
                        NEW.terminal_ledger_revision IS OLD.terminal_ledger_revision)
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_RESERVATION_CONFLICT') END;

                    SELECT CASE WHEN OLD.is_initialised = 1 AND NOT (
                        NEW.is_initialised = 1 AND
                        NEW.current_reservation_revision = OLD.current_reservation_revision + 1)
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_RESERVATION_CONFLICT') END;

                    SELECT CASE WHEN NOT EXISTS (
                        SELECT 1
                        FROM provider_budget_reservation_transitions AS transition
                        WHERE transition.provider_request_id = NEW.provider_request_id
                          AND transition.reservation_revision = NEW.current_reservation_revision
                          AND transition.envelope_id = NEW.envelope_id
                          AND transition.to_status = NEW.status
                          AND transition.transition_sha256 = NEW.current_transition_sha256
                          AND (OLD.is_initialised = 0 OR transition.from_status = OLD.status)
                          AND (NEW.terminal_ledger_revision IS NULL OR
                               transition.ledger_revision = NEW.terminal_ledger_revision))
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_RESERVATION_TRANSITION_MISSING') END;
                END;

                CREATE TRIGGER trg_provider_budget_reservations_immutable_delete
                BEFORE DELETE ON provider_budget_reservations
                BEGIN
                    SELECT RAISE(ABORT, 'CH_PROVIDER_BUDGET_APPEND_ONLY');
                END;

                CREATE TRIGGER trg_provider_budget_reservation_transitions_append_insert
                BEFORE INSERT ON provider_budget_reservation_transitions
                BEGIN
                    SELECT CASE WHEN NOT EXISTS (
                        SELECT 1
                        FROM provider_budget_reservations AS reservation
                        WHERE reservation.provider_request_id = NEW.provider_request_id
                          AND reservation.envelope_id = NEW.envelope_id
                          AND ((reservation.is_initialised = 0 AND
                                NEW.reservation_revision = 1 AND
                                NEW.previous_transition_sha256 = printf('%064d', 0)) OR
                               (reservation.is_initialised = 1 AND
                                NEW.reservation_revision = reservation.current_reservation_revision + 1 AND
                                NEW.previous_transition_sha256 = reservation.current_transition_sha256)))
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_RESERVATION_CONFLICT') END;

                    SELECT CASE WHEN NOT (
                        (NEW.reservation_revision = 1 AND
                         NEW.from_status IS NULL AND
                         NEW.to_status = 'Reserved' AND
                         NEW.transition_kind = 'Admission') OR
                        (NEW.from_status = 'Reserved' AND
                         NEW.to_status = 'DispatchStarted' AND
                         NEW.transition_kind = 'DispatchStarted') OR
                        (NEW.from_status = 'Reserved' AND
                         NEW.to_status = 'ReleasedPreSend' AND
                         NEW.transition_kind = 'PreSendReleased') OR
                        (NEW.from_status = 'DispatchStarted' AND
                         NEW.to_status = 'Committed' AND
                         NEW.transition_kind = 'ObservedCommitted') OR
                        (NEW.from_status = 'DispatchStarted' AND
                         NEW.to_status = 'IndeterminateCommitted' AND
                         NEW.transition_kind = 'IndeterminateCommitted') OR
                        (NEW.from_status = 'DispatchStarted' AND
                         NEW.to_status = 'OverrunCommitted' AND
                         NEW.transition_kind = 'OverrunCommitted'))
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_RESERVATION_TRANSITION_INVALID') END;

                    SELECT CASE WHEN NOT EXISTS (
                        SELECT 1
                        FROM provider_budget_ledger_revisions AS ledger
                        WHERE ledger.envelope_id = NEW.envelope_id
                          AND ledger.ledger_revision = NEW.ledger_revision
                          AND ledger.provider_request_id = NEW.provider_request_id
                          AND ledger.is_complete = 1)
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_LEDGER_CONFLICT') END;
                END;

                CREATE TRIGGER trg_provider_budget_reservation_transitions_immutable_update
                BEFORE UPDATE ON provider_budget_reservation_transitions
                BEGIN
                    SELECT RAISE(ABORT, 'CH_PROVIDER_BUDGET_APPEND_ONLY');
                END;

                CREATE TRIGGER trg_provider_budget_reservation_transitions_immutable_delete
                BEFORE DELETE ON provider_budget_reservation_transitions
                BEGIN
                    SELECT RAISE(ABORT, 'CH_PROVIDER_BUDGET_APPEND_ONLY');
                END;
                """);
    }

    private static void CreateEvidenceAndAuditTriggers(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
                CREATE TRIGGER trg_provider_budget_commitments_validate_insert
                BEFORE INSERT ON provider_budget_commitments
                BEGIN
                    SELECT CASE WHEN EXISTS (
                        SELECT 1
                        FROM provider_budget_releases AS release
                        WHERE release.provider_request_id = NEW.provider_request_id)
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_TERMINAL_CONFLICT') END;

                    SELECT CASE WHEN NOT EXISTS (
                        SELECT 1
                        FROM provider_budget_reservations AS reservation
                        WHERE reservation.provider_request_id = NEW.provider_request_id
                          AND reservation.envelope_id = NEW.envelope_id
                          AND reservation.is_initialised = 1
                          AND reservation.terminal_ledger_revision = NEW.ledger_revision
                          AND ((NEW.commitment_kind = 'Observed' AND
                                reservation.status = 'Committed' AND
                                NEW.committed_units <= reservation.maximum_charge_units) OR
                               (NEW.commitment_kind = 'IndeterminateMaximum' AND
                                reservation.status = 'IndeterminateCommitted' AND
                                NEW.committed_units = reservation.maximum_charge_units) OR
                               (NEW.commitment_kind = 'OverrunMaximum' AND
                                reservation.status = 'OverrunCommitted' AND
                                NEW.committed_units = reservation.maximum_charge_units)))
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_COMMITMENT_CONFLICT') END;
                END;

                CREATE TRIGGER trg_provider_budget_commitments_immutable_update
                BEFORE UPDATE ON provider_budget_commitments
                BEGIN
                    SELECT RAISE(ABORT, 'CH_PROVIDER_BUDGET_APPEND_ONLY');
                END;

                CREATE TRIGGER trg_provider_budget_commitments_immutable_delete
                BEFORE DELETE ON provider_budget_commitments
                BEGIN
                    SELECT RAISE(ABORT, 'CH_PROVIDER_BUDGET_APPEND_ONLY');
                END;

                CREATE TRIGGER trg_provider_budget_releases_validate_insert
                BEFORE INSERT ON provider_budget_releases
                BEGIN
                    SELECT CASE WHEN EXISTS (
                        SELECT 1
                        FROM provider_budget_commitments AS commitment
                        WHERE commitment.provider_request_id = NEW.provider_request_id)
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_TERMINAL_CONFLICT') END;

                    SELECT CASE WHEN NOT EXISTS (
                        SELECT 1
                        FROM provider_budget_reservations AS reservation
                        WHERE reservation.provider_request_id = NEW.provider_request_id
                          AND reservation.envelope_id = NEW.envelope_id
                          AND reservation.is_initialised = 1
                          AND reservation.status = 'ReleasedPreSend'
                          AND reservation.dispatch_started_at_utc IS NULL
                          AND reservation.terminal_ledger_revision = NEW.ledger_revision)
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_RELEASE_CONFLICT') END;
                END;

                CREATE TRIGGER trg_provider_budget_releases_immutable_update
                BEFORE UPDATE ON provider_budget_releases
                BEGIN
                    SELECT RAISE(ABORT, 'CH_PROVIDER_BUDGET_APPEND_ONLY');
                END;

                CREATE TRIGGER trg_provider_budget_releases_immutable_delete
                BEFORE DELETE ON provider_budget_releases
                BEGIN
                    SELECT RAISE(ABORT, 'CH_PROVIDER_BUDGET_APPEND_ONLY');
                END;

                CREATE TRIGGER trg_provider_budget_reconciliation_validate_insert
                BEFORE INSERT ON provider_budget_reconciliation_dispositions
                BEGIN
                    SELECT CASE WHEN NOT EXISTS (
                        SELECT 1
                        FROM provider_budget_reservations AS reservation
                        JOIN provider_budget_commitments AS commitment
                          ON commitment.provider_request_id = reservation.provider_request_id
                        JOIN provider_budget_ledger_revisions AS ledger
                          ON ledger.envelope_id = NEW.envelope_id
                         AND ledger.ledger_revision = NEW.ledger_revision
                        WHERE reservation.provider_request_id = NEW.provider_request_id
                          AND reservation.envelope_id = NEW.envelope_id
                          AND reservation.status = 'IndeterminateCommitted'
                          AND commitment.commitment_kind = 'IndeterminateMaximum'
                          AND ledger.transition_kind = 'Reconciled'
                          AND ledger.provider_request_id = NEW.provider_request_id
                          AND ledger.is_complete = 1
                          AND NEW.confirmed_charge_units <= reservation.maximum_charge_units
                          AND NEW.restored_units = reservation.maximum_charge_units - NEW.confirmed_charge_units)
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_RECONCILIATION_CONFLICT') END;
                END;

                CREATE TRIGGER trg_provider_budget_reconciliation_immutable_update
                BEFORE UPDATE ON provider_budget_reconciliation_dispositions
                BEGIN
                    SELECT RAISE(ABORT, 'CH_PROVIDER_BUDGET_APPEND_ONLY');
                END;

                CREATE TRIGGER trg_provider_budget_reconciliation_immutable_delete
                BEFORE DELETE ON provider_budget_reconciliation_dispositions
                BEGIN
                    SELECT RAISE(ABORT, 'CH_PROVIDER_BUDGET_APPEND_ONLY');
                END;

                CREATE TRIGGER trg_provider_budget_rearms_validate_insert
                BEFORE INSERT ON provider_budget_rearms
                BEGIN
                    SELECT CASE WHEN NOT EXISTS (
                        SELECT 1
                        FROM provider_budget_envelopes AS envelope
                        JOIN provider_budget_configurations AS configuration
                          ON configuration.envelope_id = envelope.envelope_id
                         AND configuration.configuration_revision = NEW.expected_configuration_revision
                        JOIN provider_budget_ledger_revisions AS ledger
                          ON ledger.envelope_id = envelope.envelope_id
                         AND ledger.ledger_revision = NEW.resulting_ledger_revision
                        WHERE envelope.envelope_id = NEW.envelope_id
                          AND envelope.is_initialised = 1
                          AND envelope.is_closed = 0
                          AND envelope.state IN ('Disarmed', 'Tripped')
                          AND envelope.current_store_epoch_id = NEW.store_epoch_id
                          AND envelope.current_configuration_revision = NEW.expected_configuration_revision
                          AND envelope.current_ledger_revision = NEW.expected_ledger_revision
                          AND envelope.current_rearm_revision = NEW.expected_rearm_revision
                          AND NEW.rearm_revision = envelope.current_rearm_revision + 1
                          AND configuration.sealed_at_utc IS NOT NULL
                          AND configuration.configuration_sha256 = NEW.configuration_sha256
                          AND ledger.is_complete = 1
                          AND ledger.transition_kind = 'Rearmed'
                          AND ledger.store_epoch_id = NEW.store_epoch_id
                          AND ledger.configuration_revision = NEW.expected_configuration_revision
                          AND ledger.rearm_revision = NEW.rearm_revision
                          AND ledger.state = 'Armed'
                          AND ledger.runtime_session_id = NEW.new_runtime_session_id
                          AND ledger.aggregate_committed_units = NEW.acknowledged_committed_units
                          AND ledger.aggregate_reserved_units = NEW.acknowledged_reserved_units
                          AND ledger.aggregate_indeterminate_units = NEW.acknowledged_indeterminate_units)
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_REARM_CONFLICT') END;

                    SELECT CASE WHEN EXISTS (
                        SELECT 1
                        FROM provider_budget_rearms AS existing
                        WHERE existing.envelope_id = NEW.envelope_id
                          AND existing.rearm_revision >= NEW.rearm_revision)
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_REARM_CONFLICT') END;
                END;

                CREATE TRIGGER trg_provider_budget_rearms_immutable_update
                BEFORE UPDATE ON provider_budget_rearms
                BEGIN
                    SELECT RAISE(ABORT, 'CH_PROVIDER_BUDGET_APPEND_ONLY');
                END;

                CREATE TRIGGER trg_provider_budget_rearms_immutable_delete
                BEFORE DELETE ON provider_budget_rearms
                BEGIN
                    SELECT RAISE(ABORT, 'CH_PROVIDER_BUDGET_APPEND_ONLY');
                END;

                CREATE TRIGGER trg_provider_budget_audit_events_validate_insert
                BEFORE INSERT ON provider_budget_audit_events
                BEGIN
                    SELECT CASE WHEN NOT EXISTS (
                        SELECT 1
                        FROM provider_budget_ledger_revisions AS ledger
                        WHERE ledger.envelope_id = NEW.envelope_id
                          AND ledger.ledger_revision = NEW.ledger_revision
                          AND ledger.is_complete = 1)
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_LEDGER_NOT_READY') END;

                    SELECT CASE WHEN NEW.provider_request_id IS NOT NULL AND NOT EXISTS (
                        SELECT 1
                        FROM provider_budget_reservations AS reservation
                        WHERE reservation.provider_request_id = NEW.provider_request_id
                          AND reservation.envelope_id = NEW.envelope_id
                          AND (NEW.operation_class IS NULL OR
                               reservation.operation_class = NEW.operation_class)
                          AND (NEW.request_sha256 IS NULL OR
                               reservation.request_sha256 = NEW.request_sha256)
                          AND (NEW.maximum_charge_units IS NULL OR
                               reservation.maximum_charge_units = NEW.maximum_charge_units))
                    THEN RAISE(ABORT, 'CH_PROVIDER_BUDGET_AUDIT_BINDING_CONFLICT') END;
                END;

                CREATE TRIGGER trg_provider_budget_audit_events_immutable_update
                BEFORE UPDATE ON provider_budget_audit_events
                BEGIN
                    SELECT RAISE(ABORT, 'CH_PROVIDER_BUDGET_APPEND_ONLY');
                END;

                CREATE TRIGGER trg_provider_budget_audit_events_immutable_delete
                BEFORE DELETE ON provider_budget_audit_events
                BEGIN
                    SELECT RAISE(ABORT, 'CH_PROVIDER_BUDGET_APPEND_ONLY');
                END;
                """);
    }

    private static void GuardEmptyProviderBudgetRollback(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
                DROP TABLE IF EXISTS temp.__provider_budget_down_guard;
                CREATE TEMP TABLE __provider_budget_down_guard (
                    must_be_zero INTEGER NOT NULL CHECK (must_be_zero = 0)
                );
                INSERT INTO temp.__provider_budget_down_guard (must_be_zero)
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM provider_budget_store_epochs) OR
                    EXISTS (SELECT 1 FROM provider_budget_control_heads) OR
                    EXISTS (SELECT 1 FROM provider_budget_envelopes) OR
                    EXISTS (SELECT 1 FROM provider_budget_configurations) OR
                    EXISTS (SELECT 1 FROM provider_budget_operation_allocations) OR
                    EXISTS (SELECT 1 FROM provider_budget_ledger_revisions) OR
                    EXISTS (SELECT 1 FROM provider_budget_operation_balance_revisions) OR
                    EXISTS (SELECT 1 FROM provider_budget_reservations) OR
                    EXISTS (SELECT 1 FROM provider_budget_reservation_transitions) OR
                    EXISTS (SELECT 1 FROM provider_budget_commitments) OR
                    EXISTS (SELECT 1 FROM provider_budget_releases) OR
                    EXISTS (SELECT 1 FROM provider_budget_reconciliation_dispositions) OR
                    EXISTS (SELECT 1 FROM provider_budget_rearms) OR
                    EXISTS (SELECT 1 FROM provider_budget_audit_events)
                THEN 1 ELSE 0 END;
                DROP TABLE temp.__provider_budget_down_guard;
                """);
    }
}
