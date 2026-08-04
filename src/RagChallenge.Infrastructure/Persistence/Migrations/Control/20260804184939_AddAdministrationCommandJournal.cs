// Purpose: Adds the durable administration command journal as an isolated, additive control-plane structure.
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RagChallenge.Infrastructure.Persistence.Migrations.Control;

/// <inheritdoc />
public partial class AddAdministrationCommandJournal : Migration
{
    private static readonly string[] JournalIndexColumns =
        ["corpus_id", "started_at_utc"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "administration_command_journal",
            columns: table => new
            {
                operation_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                corpus_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                command = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                actor_identifier = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                reason_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                input_sha256 = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                source_ids_json = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: false, collation: "BINARY"),
                target_ids_json = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: false, collation: "BINARY"),
                started_at_utc = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                completed_at_utc = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                status = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                outcome = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                result_code = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, collation: "BINARY"),
                exit_category = table.Column<int>(type: "INTEGER", nullable: true),
                result_revision = table.Column<long>(type: "INTEGER", nullable: true),
                intent_digest = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_administration_command_journal", x => x.operation_id);
                table.CheckConstraint("ck_administration_journal_actor", "length(actor_identifier) BETWEEN 1 AND 128 AND actor_identifier NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_administration_journal_command", "length(command) BETWEEN 1 AND 64 AND command NOT GLOB '*[^a-z0-9-]*'");
                table.CheckConstraint("ck_administration_journal_completed_utc", "completed_at_utc IS NULL OR length(completed_at_utc) = 33 AND substr(completed_at_utc, -6) = '+00:00'");
                table.CheckConstraint("ck_administration_journal_completion", "(status = 'Started' AND completed_at_utc IS NULL AND outcome IS NULL AND result_code IS NULL AND exit_category IS NULL AND result_revision IS NULL) OR (status = 'Completed' AND completed_at_utc IS NOT NULL AND outcome IS NOT NULL AND result_code IS NOT NULL AND exit_category IS NOT NULL)");
                table.CheckConstraint("ck_administration_journal_corpus", "length(corpus_id) BETWEEN 1 AND 128 AND corpus_id GLOB '[A-Za-z0-9]*' AND corpus_id NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_administration_journal_exit", "exit_category IS NULL OR exit_category IN (0, 2, 3, 4, 5, 10)");
                table.CheckConstraint("ck_administration_journal_input", "input_sha256 IS NULL OR length(input_sha256) = 64 AND input_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_administration_journal_intent", "length(intent_digest) = 64 AND intent_digest NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_administration_journal_operation", "length(operation_id) BETWEEN 1 AND 128 AND operation_id GLOB '[A-Za-z0-9]*' AND operation_id NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_administration_journal_outcome", "outcome IS NULL OR outcome IN ('Applied', 'Rejected', 'Unavailable', 'Failed')");
                table.CheckConstraint("ck_administration_journal_reason", "length(reason_sha256) = 64 AND reason_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_administration_journal_result_revision", "result_revision IS NULL OR result_revision >= 0");
                table.CheckConstraint("ck_administration_journal_sources", "length(source_ids_json) BETWEEN 2 AND 4096");
                table.CheckConstraint("ck_administration_journal_started_utc", "length(started_at_utc) = 33 AND substr(started_at_utc, -6) = '+00:00'");
                table.CheckConstraint("ck_administration_journal_status", "status IN ('Started', 'Completed')");
                table.CheckConstraint("ck_administration_journal_targets", "length(target_ids_json) BETWEEN 2 AND 4096");
                table.CheckConstraint("ck_administration_journal_time_order", "completed_at_utc IS NULL OR completed_at_utc >= started_at_utc");
            });

        migrationBuilder.CreateIndex(
            name: "IX_administration_command_journal_corpus_id_started_at_utc",
            table: "administration_command_journal",
            columns: JournalIndexColumns);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "administration_command_journal");
    }
}
