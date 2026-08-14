// Purpose: Allows text-only PDF activation and answer evidence while permitting independent sparse cited-page manifests.
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RagChallenge.Infrastructure.Persistence.Migrations.Control;

/// <inheritdoc />
public partial class AllowTextFirstPdfVisualEvidence : Migration
{
    private const string PageLookupIndex =
        "IX_document_page_images_corpus_id_document_id_document_version_" +
        "source_content_sha256_page_number_render_profile_id_renderer_descriptor";

    private static readonly string[] PageLookupColumns =
    [
        "corpus_id",
        "document_id",
        "document_version",
        "source_content_sha256",
        "page_number",
        "render_profile_id",
        "renderer_descriptor",
    ];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(PageLookupIndex, "document_page_images");
        migrationBuilder.DropCheckConstraint(
            "ck_answer_evidence_citation_location",
            "answer_evidence_citations");
        migrationBuilder.DropCheckConstraint(
            "ck_activation_evidence_manifest",
            "activation_evidence_bindings");
        migrationBuilder.CreateIndex(
            PageLookupIndex,
            "document_page_images",
            PageLookupColumns);
        migrationBuilder.AddCheckConstraint(
            "ck_answer_evidence_citation_location",
            "answer_evidence_citations",
            "(document_format = 'Pdf' AND page_start > 0 AND page_end >= page_start " +
            "AND record_start IS NULL AND record_end IS NULL AND columns_json = '[]') OR " +
            "(document_format = 'Csv' AND page_start IS NULL AND page_end IS NULL " +
            "AND ((record_start IS NULL AND record_end IS NULL) OR " +
            "(record_start > 0 AND record_end >= record_start)) " +
            "AND render_manifest_id IS NULL)");
        migrationBuilder.AddCheckConstraint(
            "ck_activation_evidence_manifest",
            "activation_evidence_bindings",
            "document_format = 'Pdf' OR " +
            "(document_format = 'Csv' AND render_manifest_id IS NULL)");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(PageLookupIndex, "document_page_images");
        migrationBuilder.DropCheckConstraint(
            "ck_answer_evidence_citation_location",
            "answer_evidence_citations");
        migrationBuilder.DropCheckConstraint(
            "ck_activation_evidence_manifest",
            "activation_evidence_bindings");
        migrationBuilder.CreateIndex(
            PageLookupIndex,
            "document_page_images",
            PageLookupColumns,
            unique: true);
        migrationBuilder.AddCheckConstraint(
            "ck_answer_evidence_citation_location",
            "answer_evidence_citations",
            "(document_format = 'Pdf' AND page_start > 0 AND page_end >= page_start " +
            "AND record_start IS NULL AND record_end IS NULL AND columns_json = '[]' " +
            "AND render_manifest_id IS NOT NULL) OR " +
            "(document_format = 'Csv' AND page_start IS NULL AND page_end IS NULL " +
            "AND ((record_start IS NULL AND record_end IS NULL) OR " +
            "(record_start > 0 AND record_end >= record_start)) " +
            "AND render_manifest_id IS NULL)");
        migrationBuilder.AddCheckConstraint(
            "ck_activation_evidence_manifest",
            "activation_evidence_bindings",
            "(document_format = 'Pdf' AND render_manifest_id IS NOT NULL) OR " +
            "(document_format = 'Csv' AND render_manifest_id IS NULL)");
    }
}
