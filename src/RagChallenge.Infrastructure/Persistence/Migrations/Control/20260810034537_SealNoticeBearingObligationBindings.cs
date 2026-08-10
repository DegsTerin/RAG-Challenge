using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

// Purpose: Seals ordered obligation evidence and disclaimers after a complete notice-bearing manifest binds their immutable set.

namespace RagChallenge.Infrastructure.Persistence.Migrations.Control;

/// <inheritdoc />
public partial class SealNoticeBearingObligationBindings : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            CREATE TRIGGER trg_render_manifests_notice_obligation_complete_insert
            BEFORE INSERT ON document_render_manifests
            WHEN NEW.render_profile_id = 'pdf-page-png-notice-v1'
              AND NOT EXISTS (
                  SELECT 1
                  FROM derivative_obligation_evidence_references
                  WHERE obligation_set_id = NEW.obligation_set_id)
            BEGIN
                SELECT RAISE(ABORT, 'Notice-bearing manifests require obligation evidence.');
            END;
            """);

        migrationBuilder.Sql(
            """
            CREATE TRIGGER trg_render_manifests_notice_obligation_complete_update
            BEFORE UPDATE ON document_render_manifests
            WHEN NEW.render_profile_id = 'pdf-page-png-notice-v1'
              AND NOT EXISTS (
                  SELECT 1
                  FROM derivative_obligation_evidence_references
                  WHERE obligation_set_id = NEW.obligation_set_id)
            BEGIN
                SELECT RAISE(ABORT, 'Notice-bearing manifests require obligation evidence.');
            END;
            """);

        migrationBuilder.Sql(
            """
            CREATE TRIGGER trg_derivative_obligation_evidence_sealed_insert
            BEFORE INSERT ON derivative_obligation_evidence_references
            WHEN EXISTS (
                SELECT 1
                FROM document_render_manifests
                WHERE obligation_set_id = NEW.obligation_set_id)
            BEGIN
                SELECT RAISE(ABORT, 'Bound obligation evidence is sealed.');
            END;
            """);

        migrationBuilder.Sql(
            """
            CREATE TRIGGER trg_derivative_obligation_evidence_sealed_delete
            BEFORE DELETE ON derivative_obligation_evidence_references
            WHEN EXISTS (
                SELECT 1
                FROM document_render_manifests
                WHERE obligation_set_id = OLD.obligation_set_id)
            BEGIN
                SELECT RAISE(ABORT, 'Bound obligation evidence is sealed.');
            END;
            """);

        migrationBuilder.Sql(
            """
            CREATE TRIGGER trg_derivative_obligation_disclaimers_sealed_insert
            BEFORE INSERT ON derivative_obligation_disclaimers
            WHEN EXISTS (
                SELECT 1
                FROM document_render_manifests
                WHERE obligation_set_id = NEW.obligation_set_id)
            BEGIN
                SELECT RAISE(ABORT, 'Bound obligation disclaimers are sealed.');
            END;
            """);

        migrationBuilder.Sql(
            """
            CREATE TRIGGER trg_derivative_obligation_disclaimers_sealed_delete
            BEFORE DELETE ON derivative_obligation_disclaimers
            WHEN EXISTS (
                SELECT 1
                FROM document_render_manifests
                WHERE obligation_set_id = OLD.obligation_set_id)
            BEGIN
                SELECT RAISE(ABORT, 'Bound obligation disclaimers are sealed.');
            END;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "DROP TRIGGER IF EXISTS trg_derivative_obligation_disclaimers_sealed_delete;");
        migrationBuilder.Sql(
            "DROP TRIGGER IF EXISTS trg_derivative_obligation_disclaimers_sealed_insert;");
        migrationBuilder.Sql(
            "DROP TRIGGER IF EXISTS trg_derivative_obligation_evidence_sealed_delete;");
        migrationBuilder.Sql(
            "DROP TRIGGER IF EXISTS trg_derivative_obligation_evidence_sealed_insert;");
        migrationBuilder.Sql(
            "DROP TRIGGER IF EXISTS trg_render_manifests_notice_obligation_complete_update;");
        migrationBuilder.Sql(
            "DROP TRIGGER IF EXISTS trg_render_manifests_notice_obligation_complete_insert;");
    }
}
