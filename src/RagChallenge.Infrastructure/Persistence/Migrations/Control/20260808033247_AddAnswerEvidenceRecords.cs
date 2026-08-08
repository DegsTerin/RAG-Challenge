using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

// Purpose: Adds empty AnswerEvidenceRecordV1 header, citation, and page tables without backfill or historical inference.

namespace RagChallenge.Infrastructure.Persistence.Migrations.Control;

/// <inheritdoc />
public partial class AddAnswerEvidenceRecords : Migration
{
    private static readonly string[] CorpusExpiryColumns =
        ["corpus_id", "expires_at_utc"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "answer_evidence_records",
            columns: table => new
            {
                answer_evidence_record_id = table.Column<string>(type: "TEXT", maxLength: 45, nullable: false, collation: "BINARY"),
                schema_version = table.Column<int>(type: "INTEGER", nullable: false),
                record_sha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "BINARY"),
                corpus_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                activation_record_revision = table.Column<long>(type: "INTEGER", nullable: false),
                catalogue_revision = table.Column<long>(type: "INTEGER", nullable: false),
                source_binding_set_digest = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "BINARY"),
                activation_binding_set_digest = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "BINARY"),
                index_generation_id = table.Column<string>(type: "TEXT", maxLength: 71, nullable: false, collation: "BINARY"),
                outcome = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                question_language = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                answer_language = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                answer_sha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "BINARY"),
                answer_utf8_byte_length = table.Column<int>(type: "INTEGER", nullable: false),
                evidence_coverage_digest = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "BINARY"),
                retrieval_policy_version = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, collation: "BINARY"),
                prompt_version = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, collation: "BINARY"),
                language_model_provider_id = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, collation: "BINARY"),
                language_model_id = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, collation: "BINARY"),
                language_model_revision = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, collation: "BINARY"),
                correlation_id = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, collation: "BINARY"),
                retention_policy_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                created_at_utc = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                expires_at_utc = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_answer_evidence_records", x => x.answer_evidence_record_id);
                table.CheckConstraint("ck_answer_evidence_activation_digest", "length(activation_binding_set_digest) = 64 AND activation_binding_set_digest NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_answer_evidence_activation_revision", "activation_record_revision > 0");
                table.CheckConstraint("ck_answer_evidence_answer_digest", "length(answer_sha256) = 64 AND answer_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_answer_evidence_answer_language", "answer_language = question_language");
                table.CheckConstraint("ck_answer_evidence_answer_length", "answer_utf8_byte_length > 0");
                table.CheckConstraint("ck_answer_evidence_catalogue_revision", "catalogue_revision > 0");
                table.CheckConstraint("ck_answer_evidence_coverage_digest", "length(evidence_coverage_digest) = 64 AND evidence_coverage_digest NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_answer_evidence_created_utc", "length(created_at_utc) = 33 AND substr(created_at_utc, -6) = '+00:00'");
                table.CheckConstraint("ck_answer_evidence_expires_utc", "length(expires_at_utc) = 33 AND substr(expires_at_utc, -6) = '+00:00'");
                table.CheckConstraint("ck_answer_evidence_id", "length(answer_evidence_record_id) = 45 AND answer_evidence_record_id GLOB 'ans-evidence-[0-9a-f]*' AND substr(answer_evidence_record_id, 14) NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_answer_evidence_outcome", "outcome = 'Answered'");
                table.CheckConstraint("ck_answer_evidence_p30d", "julianday(expires_at_utc) = julianday(created_at_utc) + 30");
                table.CheckConstraint("ck_answer_evidence_question_language", "question_language IN ('pt-BR', 'en-GB')");
                table.CheckConstraint("ck_answer_evidence_record_digest", "length(record_sha256) = 64 AND record_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_answer_evidence_retention", "retention_policy_id = 'answer-evidence-p30d-v1'");
                table.CheckConstraint("ck_answer_evidence_schema", "schema_version = 1");
                table.CheckConstraint("ck_answer_evidence_source_digest", "length(source_binding_set_digest) = 64 AND source_binding_set_digest NOT GLOB '*[^0-9a-f]*'");
                table.ForeignKey(
                    name: "FK_answer_evidence_records_admin_operations_answer_evidence_record_id",
                    column: x => x.answer_evidence_record_id,
                    principalTable: "admin_operations",
                    principalColumn: "operation_id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_answer_evidence_records_corpora_corpus_id",
                    column: x => x.corpus_id,
                    principalTable: "corpora",
                    principalColumn: "corpus_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "answer_evidence_citations",
            columns: table => new
            {
                answer_evidence_record_id = table.Column<string>(type: "TEXT", maxLength: 45, nullable: false, collation: "BINARY"),
                ordinal = table.Column<int>(type: "INTEGER", nullable: false),
                product_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                product_revision = table.Column<long>(type: "INTEGER", nullable: false),
                document_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                document_version = table.Column<long>(type: "INTEGER", nullable: false),
                document_format = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                content_language = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                chunk_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                source_adapter_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                source_trust_class = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                official_registration_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                source_snapshot_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                source_observation_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true, collation: "BINARY"),
                source_content_sha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "BINARY"),
                page_start = table.Column<int>(type: "INTEGER", nullable: true),
                page_end = table.Column<int>(type: "INTEGER", nullable: true),
                record_start = table.Column<long>(type: "INTEGER", nullable: true),
                record_end = table.Column<long>(type: "INTEGER", nullable: true),
                columns_json = table.Column<string>(type: "TEXT", maxLength: 8192, nullable: false, collation: "BINARY"),
                section_locator = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true, collation: "BINARY"),
                render_manifest_id = table.Column<string>(type: "TEXT", maxLength: 79, nullable: true, collation: "BINARY")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_answer_evidence_citations", x => new { x.answer_evidence_record_id, x.ordinal });
                table.CheckConstraint("ck_answer_evidence_citation_columns", "json_valid(columns_json) AND json_type(columns_json) = 'array' AND length(columns_json) BETWEEN 2 AND 8192");
                table.CheckConstraint("ck_answer_evidence_citation_document_version", "document_version > 0");
                table.CheckConstraint("ck_answer_evidence_citation_format", "document_format IN ('Pdf', 'Csv')");
                table.CheckConstraint("ck_answer_evidence_citation_language", "content_language IN ('pt-BR', 'en-GB')");
                table.CheckConstraint("ck_answer_evidence_citation_location", "(document_format = 'Pdf' AND page_start > 0 AND page_end >= page_start AND record_start IS NULL AND record_end IS NULL AND columns_json = '[]' AND render_manifest_id IS NOT NULL) OR (document_format = 'Csv' AND page_start IS NULL AND page_end IS NULL AND ((record_start IS NULL AND record_end IS NULL) OR (record_start > 0 AND record_end >= record_start)) AND render_manifest_id IS NULL)");
                table.CheckConstraint("ck_answer_evidence_citation_ordinal", "ordinal > 0");
                table.CheckConstraint("ck_answer_evidence_citation_product_revision", "product_revision > 0");
                table.CheckConstraint("ck_answer_evidence_citation_section", "section_locator IS NULL OR length(section_locator) BETWEEN 1 AND 512");
                table.CheckConstraint("ck_answer_evidence_citation_source", "length(source_content_sha256) = 64 AND source_content_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_answer_evidence_citation_source_identity", "(source_trust_class = 'LocalAuthorised' AND official_registration_id IS NULL AND source_snapshot_id IS NULL AND source_observation_id IS NULL) OR (source_trust_class = 'OfficialExternal' AND official_registration_id IS NOT NULL AND source_snapshot_id IS NOT NULL AND source_observation_id IS NOT NULL)");
                table.CheckConstraint("ck_answer_evidence_citation_trust", "source_trust_class IN ('LocalAuthorised', 'OfficialExternal')");
                table.ForeignKey(
                    name: "FK_answer_evidence_citations_answer_evidence_records_answer_evidence_record_id",
                    column: x => x.answer_evidence_record_id,
                    principalTable: "answer_evidence_records",
                    principalColumn: "answer_evidence_record_id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_answer_evidence_citations_content_objects_source_content_sha256",
                    column: x => x.source_content_sha256,
                    principalTable: "content_objects",
                    principalColumn: "content_sha256",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "answer_evidence_pages",
            columns: table => new
            {
                answer_evidence_record_id = table.Column<string>(type: "TEXT", maxLength: 45, nullable: false, collation: "BINARY"),
                document_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                document_version = table.Column<long>(type: "INTEGER", nullable: false),
                page_number = table.Column<int>(type: "INTEGER", nullable: false),
                source_content_sha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "BINARY"),
                render_manifest_id = table.Column<string>(type: "TEXT", maxLength: 79, nullable: false, collation: "BINARY"),
                render_profile_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                renderer_descriptor = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                image_content_sha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "BINARY"),
                image_sha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "BINARY"),
                byte_length = table.Column<long>(type: "INTEGER", nullable: false),
                media_type = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                width_pixels = table.Column<int>(type: "INTEGER", nullable: false),
                height_pixels = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_answer_evidence_pages", x => new { x.answer_evidence_record_id, x.document_id, x.document_version, x.page_number });
                table.CheckConstraint("ck_answer_evidence_page_document_version", "document_version > 0");
                table.CheckConstraint("ck_answer_evidence_page_height", "height_pixels BETWEEN 1 AND 4096");
                table.CheckConstraint("ck_answer_evidence_page_identity", "image_content_sha256 = image_sha256");
                table.CheckConstraint("ck_answer_evidence_page_image", "length(image_content_sha256) = 64 AND image_content_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_answer_evidence_page_image_digest", "length(image_sha256) = 64 AND image_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_answer_evidence_page_length", "byte_length > 0");
                table.CheckConstraint("ck_answer_evidence_page_media", "media_type = 'image/png'");
                table.CheckConstraint("ck_answer_evidence_page_number", "page_number > 0");
                table.CheckConstraint("ck_answer_evidence_page_source", "length(source_content_sha256) = 64 AND source_content_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_answer_evidence_page_width", "width_pixels BETWEEN 1 AND 4096");
                table.ForeignKey(
                    name: "FK_answer_evidence_pages_answer_evidence_records_answer_evidence_record_id",
                    column: x => x.answer_evidence_record_id,
                    principalTable: "answer_evidence_records",
                    principalColumn: "answer_evidence_record_id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_answer_evidence_pages_content_objects_image_content_sha256",
                    column: x => x.image_content_sha256,
                    principalTable: "content_objects",
                    principalColumn: "content_sha256",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_answer_evidence_pages_content_objects_source_content_sha256",
                    column: x => x.source_content_sha256,
                    principalTable: "content_objects",
                    principalColumn: "content_sha256",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_answer_evidence_citations_source_content_sha256",
            table: "answer_evidence_citations",
            column: "source_content_sha256");

        migrationBuilder.CreateIndex(
            name: "IX_answer_evidence_pages_image_content_sha256",
            table: "answer_evidence_pages",
            column: "image_content_sha256");

        migrationBuilder.CreateIndex(
            name: "IX_answer_evidence_pages_source_content_sha256",
            table: "answer_evidence_pages",
            column: "source_content_sha256");

        migrationBuilder.CreateIndex(
            name: "IX_answer_evidence_records_corpus_id_expires_at_utc",
            table: "answer_evidence_records",
            columns: CorpusExpiryColumns);

        migrationBuilder.CreateIndex(
            name: "IX_answer_evidence_records_record_sha256",
            table: "answer_evidence_records",
            column: "record_sha256");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "answer_evidence_citations");

        migrationBuilder.DropTable(
            name: "answer_evidence_pages");

        migrationBuilder.DropTable(
            name: "answer_evidence_records");
    }
}
