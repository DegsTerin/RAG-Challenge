// Purpose: Verifies the internal provider-budget Control schema, empty rollback guard, constraints, foreign keys, and append-only triggers without exercising a provider path.
using System.Globalization;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

using RagChallenge.Infrastructure.Persistence;

namespace RagChallenge.IntegrationTests;

public sealed class SqliteProviderBudgetSchemaMigrationTests
{
    private const string PreviousMigration =
        "20260814012801_AllowTextFirstPdfVisualEvidence";
    private const string ProviderBudgetMigration =
        "20260816202337_AddPersistentProviderBudgetAdmission";
    private const string Timestamp = "2026-01-02T12:00:00.0000000+00:00";
    private const string ZeroSha256 =
        "0000000000000000000000000000000000000000000000000000000000000000";

    [Fact]
    public async Task UpgradeCreatesEmptySchemaAndEmptyRollbackIsReversible()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();

        Assert.Equal(14, await ProviderBudgetTableCountAsync(fixture.Options.ControlDatabasePath));
        Assert.Equal(42, await ProviderBudgetTriggerCountAsync(fixture.Options.ControlDatabasePath));
        Assert.Equal(0, await ProviderBudgetRowCountAsync(fixture.Options.ControlDatabasePath));

        await MigrateAsync(fixture.Options, PreviousMigration);

        Assert.Equal(0, await ProviderBudgetTableCountAsync(fixture.Options.ControlDatabasePath));
        Assert.Equal(0, await ProviderBudgetTriggerCountAsync(fixture.Options.ControlDatabasePath));

        await MigrateAsync(fixture.Options, ProviderBudgetMigration);

        Assert.Equal(14, await ProviderBudgetTableCountAsync(fixture.Options.ControlDatabasePath));
        Assert.Equal(42, await ProviderBudgetTriggerCountAsync(fixture.Options.ControlDatabasePath));
        Assert.Equal(0, await ProviderBudgetRowCountAsync(fixture.Options.ControlDatabasePath));
        Assert.Equal(0, await CountRowsAsync(
            fixture.Options.ControlDatabasePath,
            "PRAGMA foreign_key_check;"));
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM pragma_foreign_key_list('provider_budget_commitments') " +
            "WHERE \"table\" = 'provider_budget_reservations';"));
        Assert.Equal(2, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM pragma_foreign_key_list('provider_budget_rearms') " +
            "WHERE \"table\" = 'provider_budget_ledger_revisions' AND " +
            "((\"from\" = 'envelope_id' AND \"to\" = 'envelope_id') OR " +
            "(\"from\" = 'resulting_ledger_revision' AND \"to\" = 'ledger_revision'));"));

        await MigrateAsync(fixture.Options, PreviousMigration);

        Assert.Equal(0, await ProviderBudgetTableCountAsync(fixture.Options.ControlDatabasePath));
        Assert.Equal(0, await ProviderBudgetTriggerCountAsync(fixture.Options.ControlDatabasePath));
    }

    [Fact]
    public async Task ZeroBudgetGraphSealsAndRejectsConflictsOrMutation()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var epochSha256 = SqlitePersistenceFixture.Hash("provider-budget-epoch");
        var configurationSha256 = SqlitePersistenceFixture.Hash("provider-budget-configuration");
        var ledgerSha256 = SqlitePersistenceFixture.Hash("provider-budget-ledger");

        await ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            InitialGraphSql(epochSha256, configurationSha256, ledgerSha256));

        await Assert.ThrowsAsync<SqliteException>(() => ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            $"UPDATE provider_budget_configurations SET sealed_at_utc = '{Timestamp}' " +
            "WHERE envelope_id = 'budget-envelope-1' AND configuration_revision = 1;"));

        await ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            "INSERT INTO provider_budget_operation_allocations " +
            "(envelope_id, configuration_revision, operation_class, allocation_limit_units) " +
            "VALUES ('budget-envelope-1', 1, 'GroundedGeneration', 0);");
        await ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            $"UPDATE provider_budget_configurations SET sealed_at_utc = '{Timestamp}' " +
            "WHERE envelope_id = 'budget-envelope-1' AND configuration_revision = 1;");

        await Assert.ThrowsAsync<SqliteException>(() => ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            "UPDATE provider_budget_operation_allocations SET allocation_limit_units = 1 " +
            "WHERE envelope_id = 'budget-envelope-1' " +
            "AND configuration_revision = 1 AND operation_class = 'QueryEmbedding';"));

        await ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            InitialLedgerSql(ledgerSha256));
        await ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            "UPDATE provider_budget_ledger_revisions SET is_complete = 1 " +
            "WHERE envelope_id = 'budget-envelope-1' AND ledger_revision = 1;");
        await ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            InitialAuditSql(ledgerSha256));
        await ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            "UPDATE provider_budget_envelopes SET is_initialised = 1 " +
            "WHERE envelope_id = 'budget-envelope-1';");

        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT is_initialised FROM provider_budget_envelopes " +
            "WHERE envelope_id = 'budget-envelope-1';"));
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT is_complete FROM provider_budget_ledger_revisions " +
            "WHERE envelope_id = 'budget-envelope-1' AND ledger_revision = 1;"));
        Assert.Equal(0, await CountRowsAsync(
            fixture.Options.ControlDatabasePath,
            "PRAGMA foreign_key_check;"));

        await Assert.ThrowsAsync<SqliteException>(() => ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            "UPDATE provider_budget_store_epochs SET authority_reference = 'mutated' " +
            "WHERE store_epoch_id = 'budget-epoch-1';"));
        await Assert.ThrowsAsync<SqliteException>(() => ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            "UPDATE provider_budget_envelopes SET state = 'Tripped' " +
            "WHERE envelope_id = 'budget-envelope-1';"));
        await Assert.ThrowsAsync<SqliteException>(() => ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            DuplicateScopeEnvelopeSql(ledgerSha256)));
    }

    [Fact]
    public async Task RollbackRejectsAnyPersistentProviderBudgetRow()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var epochSha256 = SqlitePersistenceFixture.Hash("rollback-guard-epoch");
        await ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            StoreEpochSql(epochSha256));

        await Assert.ThrowsAsync<SqliteException>(() =>
            MigrateAsync(fixture.Options, PreviousMigration));

        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM provider_budget_store_epochs;"));
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            $"SELECT COUNT(*) FROM __EFMigrationsHistory WHERE MigrationId = '{ProviderBudgetMigration}';"));
    }

    private static string InitialGraphSql(
        string epochSha256,
        string configurationSha256,
        string ledgerSha256) =>
        $"""
        {StoreEpochSql(epochSha256)}

        INSERT INTO provider_budget_control_heads
            (control_id, current_store_epoch_id, epoch_revision, row_revision)
        VALUES
            ('provider-budget-control-v1', 'budget-epoch-1', 1, 1);

        INSERT INTO provider_budget_envelopes
            (envelope_id, schema_version, current_store_epoch_id, environment_id,
             provider_id, billing_scope_reference, model_id, currency_code,
             accounting_unit_id, current_configuration_revision,
             current_ledger_revision, current_rearm_revision, state,
             runtime_session_id, aggregate_limit_units,
             aggregate_committed_units, aggregate_reserved_units,
             aggregate_indeterminate_units, is_initialised, is_closed,
             created_at_utc, creation_authority_reference, closed_at_utc,
             closure_authority_reference, current_ledger_sha256)
        VALUES
            ('budget-envelope-1', 1, 'budget-epoch-1', 'synthetic-environment',
             'synthetic-provider', 'synthetic-billing-scope', 'synthetic-model',
             'GBP', 'synthetic-integer-unit', 1, 1, 0, 'Disarmed', NULL,
             0, 0, 0, 0, 0, 0, '{Timestamp}', 'synthetic-design-authority',
             NULL, NULL, '{ledgerSha256}');

        INSERT INTO provider_budget_configurations
            (envelope_id, configuration_revision, previous_configuration_revision,
             cost_schedule_id, cost_schedule_sha256, aggregate_limit_units,
             effective_at_utc, expires_at_utc, configuration_authority_reference,
             created_at_utc, sealed_at_utc, configuration_sha256)
        VALUES
            ('budget-envelope-1', 1, NULL, 'synthetic-zero-schedule',
             '{SqlitePersistenceFixture.Hash("synthetic zero schedule")}', 0,
             '{Timestamp}', '2027-01-02T12:00:00.0000000+00:00',
             'synthetic-design-authority', '{Timestamp}', NULL,
             '{configurationSha256}');

        INSERT INTO provider_budget_operation_allocations
            (envelope_id, configuration_revision, operation_class,
             allocation_limit_units)
        VALUES
            ('budget-envelope-1', 1, 'AdministrativeIndexEmbedding', 0),
            ('budget-envelope-1', 1, 'QueryEmbedding', 0);
        """;

    private static string InitialLedgerSql(string ledgerSha256) =>
        $"""
        INSERT INTO provider_budget_ledger_revisions
            (envelope_id, ledger_revision, store_epoch_id,
             previous_ledger_revision, configuration_revision, rearm_revision,
             state, runtime_session_id, aggregate_limit_units,
             aggregate_committed_units, aggregate_reserved_units,
             aggregate_indeterminate_units, transition_kind, provider_request_id,
             transition_authority_reference, occurred_at_utc,
             previous_ledger_sha256, ledger_sha256, is_complete)
        VALUES
            ('budget-envelope-1', 1, 'budget-epoch-1', NULL, 1, 0,
             'Disarmed', NULL, 0, 0, 0, 0, 'EnvelopeCreated', NULL,
             'synthetic-design-authority', '{Timestamp}', '{ZeroSha256}',
             '{ledgerSha256}', 0);

        INSERT INTO provider_budget_operation_balance_revisions
            (envelope_id, ledger_revision, operation_class,
             configuration_revision, allocation_limit_units, committed_units,
             reserved_units, indeterminate_units)
        VALUES
            ('budget-envelope-1', 1, 'AdministrativeIndexEmbedding', 1, 0, 0, 0, 0),
            ('budget-envelope-1', 1, 'QueryEmbedding', 1, 0, 0, 0, 0),
            ('budget-envelope-1', 1, 'GroundedGeneration', 1, 0, 0, 0, 0);
        """;

    private static string InitialAuditSql(string ledgerSha256) =>
        $"""
        INSERT INTO provider_budget_audit_events
            (audit_event_id, envelope_id, ledger_revision, provider_request_id,
             operation_class, event_type, authority_reference, actor_reference,
             request_sha256, maximum_charge_units, from_state, to_state,
             outcome_code, occurred_at_utc, details_sha256)
        VALUES
            ('budget-audit-1', 'budget-envelope-1', 1, NULL, NULL,
             'EnvelopeCreated', 'synthetic-design-authority', 'synthetic-actor',
             NULL, NULL, NULL, 'Disarmed', 'synthetic-created', '{Timestamp}',
             '{SqlitePersistenceFixture.Hash($"audit {ledgerSha256}")}');
        """;

    private static string DuplicateScopeEnvelopeSql(string ledgerSha256) =>
        $"""
        INSERT INTO provider_budget_envelopes
            (envelope_id, schema_version, current_store_epoch_id, environment_id,
             provider_id, billing_scope_reference, model_id, currency_code,
             accounting_unit_id, current_configuration_revision,
             current_ledger_revision, current_rearm_revision, state,
             runtime_session_id, aggregate_limit_units,
             aggregate_committed_units, aggregate_reserved_units,
             aggregate_indeterminate_units, is_initialised, is_closed,
             created_at_utc, creation_authority_reference, closed_at_utc,
             closure_authority_reference, current_ledger_sha256)
        VALUES
            ('budget-envelope-duplicate', 1, 'budget-epoch-1',
             'synthetic-environment', 'synthetic-provider',
             'synthetic-billing-scope', 'synthetic-model', 'GBP',
             'synthetic-integer-unit', 1, 1, 0, 'Disarmed', NULL,
             0, 0, 0, 0, 0, 0, '{Timestamp}', 'synthetic-design-authority',
             NULL, NULL, '{ledgerSha256}');
        """;

    private static string StoreEpochSql(string epochSha256) =>
        $"""
        INSERT INTO provider_budget_store_epochs
            (store_epoch_id, epoch_revision, previous_store_epoch_id, epoch_kind,
             restore_checkpoint_sha256, authority_reference, occurred_at_utc,
             previous_epoch_sha256, epoch_sha256)
        VALUES
            ('budget-epoch-1', 1, NULL, 'Initial', NULL,
             'synthetic-design-authority', '{Timestamp}', '{ZeroSha256}',
             '{epochSha256}');
        """;

    private static async Task MigrateAsync(SqliteStoreOptions options, string targetMigration)
    {
        await using var context = options.CreateControlContext();
        await context.Database.GetService<IMigrator>().MigrateAsync(targetMigration);
    }

    private static async Task ExecuteAsync(string path, string sql)
    {
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

    private static async Task<long> CountRowsAsync(string path, string sql)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={path};Mode=ReadOnly;Cache=Private;Foreign Keys=True");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await using var reader = await command.ExecuteReaderAsync();
        var count = 0L;
        while (await reader.ReadAsync())
        {
            count++;
        }

        return count;
    }

    private static Task<long> ProviderBudgetTableCountAsync(string path) =>
        ScalarAsync(
            path,
            "SELECT COUNT(*) FROM sqlite_master " +
            "WHERE type = 'table' AND name LIKE 'provider_budget_%';");

    private static Task<long> ProviderBudgetTriggerCountAsync(string path) =>
        ScalarAsync(
            path,
            "SELECT COUNT(*) FROM sqlite_master " +
            "WHERE type = 'trigger' AND name LIKE 'trg_provider_budget_%';");

    private static Task<long> ProviderBudgetRowCountAsync(string path) =>
        ScalarAsync(
            path,
            "SELECT " +
            "(SELECT COUNT(*) FROM provider_budget_store_epochs) + " +
            "(SELECT COUNT(*) FROM provider_budget_control_heads) + " +
            "(SELECT COUNT(*) FROM provider_budget_envelopes) + " +
            "(SELECT COUNT(*) FROM provider_budget_configurations) + " +
            "(SELECT COUNT(*) FROM provider_budget_operation_allocations) + " +
            "(SELECT COUNT(*) FROM provider_budget_ledger_revisions) + " +
            "(SELECT COUNT(*) FROM provider_budget_operation_balance_revisions) + " +
            "(SELECT COUNT(*) FROM provider_budget_reservations) + " +
            "(SELECT COUNT(*) FROM provider_budget_reservation_transitions) + " +
            "(SELECT COUNT(*) FROM provider_budget_commitments) + " +
            "(SELECT COUNT(*) FROM provider_budget_releases) + " +
            "(SELECT COUNT(*) FROM provider_budget_reconciliation_dispositions) + " +
            "(SELECT COUNT(*) FROM provider_budget_rearms) + " +
            "(SELECT COUNT(*) FROM provider_budget_audit_events);");
}
