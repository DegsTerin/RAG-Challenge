using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

// Purpose: Adds the immutable notice-bearing obligation schema and widens render bindings without inferring or rewriting legacy data.

namespace RagChallenge.Infrastructure.Persistence.Migrations.Control;

/// <inheritdoc />
public partial class AddNoticeBearingObligationSchema : Migration
{
    private static readonly string[] DocumentVersionSourceColumns =
        ["corpus_id", "document_id", "document_version", "content_sha256"];

    private static readonly string[] ManifestObligationForeignKeyColumns =
    [
        "obligation_set_id",
        "obligation_set_sha256",
        "corpus_id",
        "document_id",
        "document_version",
        "source_content_sha256",
    ];

    private static readonly string[] ObligationPrincipalColumns =
    [
        "obligation_set_id",
        "canonical_sha256",
        "corpus_id",
        "document_id",
        "document_version",
        "source_content_sha256",
    ];

    private static readonly string[] EvidenceReferenceIndexColumns =
        ["obligation_set_id", "evidence_reference"];

    private static readonly string[] ObligationLookupColumns =
    [
        "corpus_id",
        "document_id",
        "document_version",
        "source_content_sha256",
        "rights_mapping_revision",
    ];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "ck_render_manifests_profile",
            table: "document_render_manifests");

        migrationBuilder.DropCheckConstraint(
            name: "ck_render_manifests_schema",
            table: "document_render_manifests");

        migrationBuilder.DropCheckConstraint(
            name: "ck_page_images_profile",
            table: "document_page_images");

        migrationBuilder.AddColumn<string>(
            name: "obligation_set_id",
            table: "document_render_manifests",
            type: "TEXT",
            maxLength: 78,
            nullable: true,
            collation: "BINARY");

        migrationBuilder.AddColumn<string>(
            name: "obligation_set_sha256",
            table: "document_render_manifests",
            type: "TEXT",
            maxLength: 64,
            nullable: true,
            collation: "BINARY");

        migrationBuilder.AddColumn<int>(
            name: "notice_region_height_pixels",
            table: "document_page_images",
            type: "INTEGER",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "source_region_height_pixels",
            table: "document_page_images",
            type: "INTEGER",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "source_region_width_pixels",
            table: "document_page_images",
            type: "INTEGER",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "derivative_obligation_sets",
            columns: table => new
            {
                obligation_set_id = table.Column<string>(type: "TEXT", maxLength: 78, nullable: false, collation: "BINARY"),
                schema_version = table.Column<int>(type: "INTEGER", nullable: false),
                canonical_sha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "BINARY"),
                corpus_id = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, collation: "BINARY"),
                document_id = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, collation: "BINARY"),
                document_version = table.Column<long>(type: "INTEGER", nullable: false),
                source_content_sha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "BINARY"),
                rights_mapping_revision = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, collation: "BINARY"),
                content_language = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, collation: "BINARY"),
                authoritative_publisher_or_author = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false, collation: "BINARY"),
                document_title = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false, collation: "BINARY"),
                document_version_label = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, collation: "BINARY"),
                source_reference = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                attribution_text = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: false, collation: "BINARY"),
                copyright_notice = table.Column<string>(type: "TEXT", maxLength: 8192, nullable: false, collation: "BINARY"),
                permission_notice = table.Column<string>(type: "TEXT", maxLength: 8192, nullable: false, collation: "BINARY"),
                trademark_treatment = table.Column<string>(type: "TEXT", maxLength: 13, nullable: false, collation: "BINARY"),
                trademark_or_non_endorsement_text = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: false, collation: "BINARY"),
                change_marking_text = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: false, collation: "BINARY"),
                placement_mode = table.Column<string>(type: "TEXT", maxLength: 35, nullable: false, collation: "BINARY"),
                assessed_at_utc = table.Column<string>(type: "TEXT", maxLength: 33, nullable: false, collation: "BINARY"),
                assessor_id = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, collation: "BINARY")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_derivative_obligation_sets", x => x.obligation_set_id);
                table.UniqueConstraint("AK_derivative_obligation_sets_obligation_set_id_canonical_sha256_corpus_id_document_id_document_version_source_content_sha256", x => new { x.obligation_set_id, x.canonical_sha256, x.corpus_id, x.document_id, x.document_version, x.source_content_sha256 });
                table.CheckConstraint("ck_derivative_obligation_sets_assessed_utc", "length(assessed_at_utc) = 33 AND substr(assessed_at_utc, -6) = '+00:00'");
                table.CheckConstraint("ck_derivative_obligation_sets_assessor", "length(assessor_id) BETWEEN 1 AND 128 AND assessor_id GLOB '[A-Za-z0-9]*' AND assessor_id NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_derivative_obligation_sets_attribution", "length(attribution_text) BETWEEN 1 AND 4096");
                table.CheckConstraint("ck_derivative_obligation_sets_canonical_sha", "length(canonical_sha256) = 64 AND canonical_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.CheckConstraint("ck_derivative_obligation_sets_change_marking", "length(change_marking_text) BETWEEN 1 AND 4096");
                table.CheckConstraint("ck_derivative_obligation_sets_copyright", "length(copyright_notice) BETWEEN 1 AND 8192");
                table.CheckConstraint("ck_derivative_obligation_sets_document_version", "document_version > 0");
                table.CheckConstraint("ck_derivative_obligation_sets_identity", "length(obligation_set_id) = 78 AND substr(obligation_set_id, 1, 14) = 'obligationset-' AND substr(obligation_set_id, 15) = canonical_sha256");
                table.CheckConstraint("ck_derivative_obligation_sets_language", "length(content_language) BETWEEN 1 AND 128 AND content_language NOT GLOB '*[^A-Za-z0-9-]*' AND substr(content_language, 1, 1) <> '-' AND substr(content_language, -1) <> '-' AND instr(content_language, '--') = 0");
                table.CheckConstraint("ck_derivative_obligation_sets_mapping_revision", "length(rights_mapping_revision) BETWEEN 1 AND 128 AND rights_mapping_revision GLOB '[A-Za-z0-9]*' AND rights_mapping_revision NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_derivative_obligation_sets_permission", "length(permission_notice) BETWEEN 1 AND 8192");
                table.CheckConstraint("ck_derivative_obligation_sets_placement", "placement_mode = 'VisibleInBinaryAndAccessibleContext'");
                table.CheckConstraint("ck_derivative_obligation_sets_publisher", "length(authoritative_publisher_or_author) BETWEEN 1 AND 512");
                table.CheckConstraint("ck_derivative_obligation_sets_schema", "schema_version = 1");
                table.CheckConstraint("ck_derivative_obligation_sets_source_reference", "length(source_reference) BETWEEN 1 AND 2048");
                table.CheckConstraint("ck_derivative_obligation_sets_title", "length(document_title) BETWEEN 1 AND 512");
                table.CheckConstraint("ck_derivative_obligation_sets_trademark_text", "length(trademark_or_non_endorsement_text) BETWEEN 1 AND 4096");
                table.CheckConstraint("ck_derivative_obligation_sets_trademark_treatment", "trademark_treatment IN ('Required', 'Prohibited', 'NotApplicable')");
                table.CheckConstraint("ck_derivative_obligation_sets_version_label", "length(document_version_label) BETWEEN 1 AND 128");
                table.ForeignKey(
                    name: "FK_derivative_obligation_sets_content_objects_source_content_sha256",
                    column: x => x.source_content_sha256,
                    principalTable: "content_objects",
                    principalColumn: "content_sha256",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_derivative_obligation_sets_document_versions_corpus_id_document_id_document_version_source_content_sha256",
                    columns: x => new { x.corpus_id, x.document_id, x.document_version, x.source_content_sha256 },
                    principalTable: "document_versions",
                    principalColumns: DocumentVersionSourceColumns,
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "derivative_obligation_disclaimers",
            columns: table => new
            {
                obligation_set_id = table.Column<string>(type: "TEXT", maxLength: 78, nullable: false, collation: "BINARY"),
                ordinal = table.Column<int>(type: "INTEGER", nullable: false),
                disclaimer_text = table.Column<string>(type: "TEXT", maxLength: 8192, nullable: false, collation: "BINARY")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_derivative_obligation_disclaimers", x => new { x.obligation_set_id, x.ordinal });
                table.CheckConstraint("ck_derivative_obligation_disclaimers_ordinal", "ordinal BETWEEN 1 AND 16");
                table.CheckConstraint("ck_derivative_obligation_disclaimers_text", "length(disclaimer_text) BETWEEN 1 AND 8192");
                table.ForeignKey(
                    name: "FK_derivative_obligation_disclaimers_derivative_obligation_sets_obligation_set_id",
                    column: x => x.obligation_set_id,
                    principalTable: "derivative_obligation_sets",
                    principalColumn: "obligation_set_id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "derivative_obligation_evidence_references",
            columns: table => new
            {
                obligation_set_id = table.Column<string>(type: "TEXT", maxLength: 78, nullable: false, collation: "BINARY"),
                ordinal = table.Column<int>(type: "INTEGER", nullable: false),
                evidence_reference = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, collation: "BINARY")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_derivative_obligation_evidence_references", x => new { x.obligation_set_id, x.ordinal });
                table.CheckConstraint("ck_derivative_obligation_evidence_ordinal", "ordinal > 0");
                table.CheckConstraint("ck_derivative_obligation_evidence_reference", "length(evidence_reference) BETWEEN 1 AND 128 AND evidence_reference GLOB '[A-Za-z0-9]*' AND evidence_reference NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.ForeignKey(
                    name: "FK_derivative_obligation_evidence_references_derivative_obligation_sets_obligation_set_id",
                    column: x => x.obligation_set_id,
                    principalTable: "derivative_obligation_sets",
                    principalColumn: "obligation_set_id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_document_render_manifests_obligation_set_id_obligation_set_sha256_corpus_id_document_id_document_version_source_content_sha256",
            table: "document_render_manifests",
            columns: ManifestObligationForeignKeyColumns);

        migrationBuilder.AddCheckConstraint(
            name: "ck_render_manifests_obligation",
            table: "document_render_manifests",
            sql: "(render_profile_id = 'pdf-page-png-v1' AND schema_version = 1 AND obligation_set_id IS NULL AND obligation_set_sha256 IS NULL) OR (render_profile_id = 'pdf-page-png-notice-v1' AND schema_version = 2 AND obligation_set_id IS NOT NULL AND obligation_set_sha256 IS NOT NULL)");

        migrationBuilder.AddCheckConstraint(
            name: "ck_render_manifests_obligation_identity",
            table: "document_render_manifests",
            sql: "(obligation_set_id IS NULL AND obligation_set_sha256 IS NULL) OR (length(obligation_set_id) = 78 AND substr(obligation_set_id, 1, 14) = 'obligationset-' AND substr(obligation_set_id, 15) = obligation_set_sha256 AND length(obligation_set_sha256) = 64 AND obligation_set_sha256 NOT GLOB '*[^0-9a-f]*')");

        migrationBuilder.AddCheckConstraint(
            name: "ck_render_manifests_profile",
            table: "document_render_manifests",
            sql: "render_profile_id IN ('pdf-page-png-v1', 'pdf-page-png-notice-v1')");

        migrationBuilder.AddCheckConstraint(
            name: "ck_render_manifests_schema",
            table: "document_render_manifests",
            sql: "schema_version IN (1, 2)");

        migrationBuilder.AddCheckConstraint(
            name: "ck_page_images_profile",
            table: "document_page_images",
            sql: "render_profile_id IN ('pdf-page-png-v1', 'pdf-page-png-notice-v1')");

        migrationBuilder.AddCheckConstraint(
            name: "ck_page_images_regions",
            table: "document_page_images",
                sql: "(render_profile_id = 'pdf-page-png-v1' AND source_region_width_pixels IS NULL AND source_region_height_pixels IS NULL AND notice_region_height_pixels IS NULL) OR (render_profile_id = 'pdf-page-png-notice-v1' AND source_region_width_pixels IS NOT NULL AND source_region_height_pixels IS NOT NULL AND notice_region_height_pixels IS NOT NULL AND source_region_width_pixels BETWEEN 1 AND 4096 AND source_region_height_pixels BETWEEN 1 AND 4096 AND notice_region_height_pixels BETWEEN 1 AND 4096 AND source_region_width_pixels = width_pixels AND source_region_height_pixels + notice_region_height_pixels = height_pixels)");

        migrationBuilder.CreateIndex(
            name: "IX_derivative_obligation_evidence_references_obligation_set_id_evidence_reference",
            table: "derivative_obligation_evidence_references",
            columns: EvidenceReferenceIndexColumns,
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_derivative_obligation_sets_corpus_id_document_id_document_version_source_content_sha256_rights_mapping_revision",
            table: "derivative_obligation_sets",
            columns: ObligationLookupColumns);

        migrationBuilder.CreateIndex(
            name: "IX_derivative_obligation_sets_source_content_sha256",
            table: "derivative_obligation_sets",
            column: "source_content_sha256");

        migrationBuilder.AddForeignKey(
            name: "FK_document_render_manifests_derivative_obligation_sets_obligation_set_id_obligation_set_sha256_corpus_id_document_id_document_version_source_content_sha256",
            table: "document_render_manifests",
            columns: ManifestObligationForeignKeyColumns,
            principalTable: "derivative_obligation_sets",
            principalColumns: ObligationPrincipalColumns,
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.Sql(
            """
                CREATE TRIGGER trg_derivative_obligation_sets_immutable_update
                BEFORE UPDATE ON derivative_obligation_sets
                BEGIN
                    SELECT RAISE(ABORT, 'Derivative obligation sets are immutable.');
                END;
                """);

        migrationBuilder.Sql(
            """
                CREATE TRIGGER trg_derivative_obligation_evidence_immutable_update
                BEFORE UPDATE ON derivative_obligation_evidence_references
                BEGIN
                    SELECT RAISE(ABORT, 'Derivative obligation evidence references are immutable.');
                END;
                """);

        migrationBuilder.Sql(
            """
                CREATE TRIGGER trg_derivative_obligation_disclaimers_immutable_update
                BEFORE UPDATE ON derivative_obligation_disclaimers
                BEGIN
                    SELECT RAISE(ABORT, 'Derivative obligation disclaimers are immutable.');
                END;
                """);

    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "DROP TRIGGER IF EXISTS trg_derivative_obligation_disclaimers_immutable_update;");
        migrationBuilder.Sql(
            "DROP TRIGGER IF EXISTS trg_derivative_obligation_evidence_immutable_update;");
        migrationBuilder.Sql(
            "DROP TRIGGER IF EXISTS trg_derivative_obligation_sets_immutable_update;");

        migrationBuilder.DropForeignKey(
            name: "FK_document_render_manifests_derivative_obligation_sets_obligation_set_id_obligation_set_sha256_corpus_id_document_id_document_version_source_content_sha256",
            table: "document_render_manifests");

        migrationBuilder.DropTable(
            name: "derivative_obligation_disclaimers");

        migrationBuilder.DropTable(
            name: "derivative_obligation_evidence_references");

        migrationBuilder.DropTable(
            name: "derivative_obligation_sets");

        migrationBuilder.DropIndex(
            name: "IX_document_render_manifests_obligation_set_id_obligation_set_sha256_corpus_id_document_id_document_version_source_content_sha256",
            table: "document_render_manifests");

        migrationBuilder.DropCheckConstraint(
            name: "ck_render_manifests_obligation",
            table: "document_render_manifests");

        migrationBuilder.DropCheckConstraint(
            name: "ck_render_manifests_obligation_identity",
            table: "document_render_manifests");

        migrationBuilder.DropCheckConstraint(
            name: "ck_render_manifests_profile",
            table: "document_render_manifests");

        migrationBuilder.DropCheckConstraint(
            name: "ck_render_manifests_schema",
            table: "document_render_manifests");

        migrationBuilder.DropCheckConstraint(
            name: "ck_page_images_profile",
            table: "document_page_images");

        migrationBuilder.DropCheckConstraint(
            name: "ck_page_images_regions",
            table: "document_page_images");

        migrationBuilder.DropColumn(
            name: "obligation_set_id",
            table: "document_render_manifests");

        migrationBuilder.DropColumn(
            name: "obligation_set_sha256",
            table: "document_render_manifests");

        migrationBuilder.DropColumn(
            name: "notice_region_height_pixels",
            table: "document_page_images");

        migrationBuilder.DropColumn(
            name: "source_region_height_pixels",
            table: "document_page_images");

        migrationBuilder.DropColumn(
            name: "source_region_width_pixels",
            table: "document_page_images");

        migrationBuilder.AddCheckConstraint(
            name: "ck_render_manifests_profile",
            table: "document_render_manifests",
            sql: "render_profile_id = 'pdf-page-png-v1'");

        migrationBuilder.AddCheckConstraint(
            name: "ck_render_manifests_schema",
            table: "document_render_manifests",
            sql: "schema_version = 1");

        migrationBuilder.AddCheckConstraint(
            name: "ck_page_images_profile",
            table: "document_page_images",
            sql: "render_profile_id = 'pdf-page-png-v1'");
    }
}
