using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

// Purpose: Aligns persisted answer-evidence citation languages with the bounded BCP 47 shape accepted by document versions.

namespace RagChallenge.Infrastructure.Persistence.Migrations.Control;

/// <inheritdoc />
public partial class AllowAnswerEvidenceCitationBcp47Language : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "ck_answer_evidence_citation_language",
            table: "answer_evidence_citations");

        migrationBuilder.AddCheckConstraint(
            name: "ck_answer_evidence_citation_language",
            table: "answer_evidence_citations",
            sql: "length(content_language) BETWEEN 1 AND 128 AND content_language NOT GLOB '*[^A-Za-z0-9-]*' AND substr(content_language, 1, 1) <> '-' AND substr(content_language, -1) <> '-' AND instr(content_language, '--') = 0");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "ck_answer_evidence_citation_language",
            table: "answer_evidence_citations");

        migrationBuilder.AddCheckConstraint(
            name: "ck_answer_evidence_citation_language",
            table: "answer_evidence_citations",
            sql: "content_language IN ('pt-BR', 'en-GB')");
    }
}
