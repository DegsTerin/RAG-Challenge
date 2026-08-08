using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

// Purpose: Adds immutable per-revision activation evidence and rights snapshots without backfilling or changing existing activation rows.

namespace RagChallenge.Infrastructure.Persistence.Migrations.Control;

/// <inheritdoc />
public partial class AddDocumentRightsAndActivationEvidenceBindings : Migration
{
    private static readonly string[] ActivationDocumentKeyColumns =
        ["corpus_id", "record_revision", "document_id", "document_version"];

    private static readonly string[] DocumentSourcePrincipalColumns =
        ["corpus_id", "document_id", "document_version", "content_sha256"];

    private static readonly string[] DocumentSourceIndexColumns =
        ["corpus_id", "document_id", "document_version", "source_content_sha256"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "activation_evidence_bindings",
            columns: table => new
            {
                corpus_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                record_revision = table.Column<long>(type: "INTEGER", nullable: false),
                document_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                document_version = table.Column<long>(type: "INTEGER", nullable: false),
                document_format = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                source_content_sha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "BINARY"),
                rights_schema_version = table.Column<int>(type: "INTEGER", nullable: false),
                render_manifest_id = table.Column<string>(type: "TEXT", maxLength: 79, nullable: true, collation: "BINARY")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_activation_evidence_bindings", x => new { x.corpus_id, x.record_revision, x.document_id, x.document_version });
                table.CheckConstraint("ck_activation_evidence_format", "document_format IN ('Pdf', 'Csv')");
                table.CheckConstraint("ck_activation_evidence_manifest", "(document_format = 'Pdf' AND render_manifest_id IS NOT NULL) OR (document_format = 'Csv' AND render_manifest_id IS NULL)");
                table.CheckConstraint("ck_activation_evidence_rights_schema", "rights_schema_version = 1");
                table.CheckConstraint("ck_activation_evidence_source", "length(source_content_sha256) = 64 AND source_content_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.ForeignKey(
                    name: "FK_activation_evidence_bindings_activation_bindings_corpus_id_record_revision_document_id_document_version",
                    columns: x => new { x.corpus_id, x.record_revision, x.document_id, x.document_version },
                    principalTable: "activation_bindings",
                    principalColumns: ActivationDocumentKeyColumns,
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_activation_evidence_bindings_document_render_manifests_render_manifest_id",
                    column: x => x.render_manifest_id,
                    principalTable: "document_render_manifests",
                    principalColumn: "render_manifest_id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_activation_evidence_bindings_document_versions_corpus_id_document_id_document_version_source_content_sha256",
                    columns: x => new { x.corpus_id, x.document_id, x.document_version, x.source_content_sha256 },
                    principalTable: "document_versions",
                    principalColumns: DocumentSourcePrincipalColumns,
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "activation_rights_decisions",
            columns: table => new
            {
                corpus_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                record_revision = table.Column<long>(type: "INTEGER", nullable: false),
                document_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                document_version = table.Column<long>(type: "INTEGER", nullable: false),
                document_right = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                decision_state = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                evidence_reference = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_activation_rights_decisions", x => new { x.corpus_id, x.record_revision, x.document_id, x.document_version, x.document_right });
                table.CheckConstraint("ck_activation_rights_evidence", "length(evidence_reference) BETWEEN 1 AND 128 AND evidence_reference GLOB '[A-Za-z0-9]*' AND evidence_reference NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_activation_rights_right", "document_right IN ('SourcePossessionOrDownload', 'ParsingAndTextualTransformation', 'Indexing', 'SourceByteRetention', 'QuotationAndCitation', 'PageRendering', 'DerivativeImageCreationAndRetention', 'RuntimeDerivativeImageDisplay', 'SourceAndDerivativeByteDistributionOrPublication', 'AttributionNoticeTrademarkAndChangeMarkingRequirements')");
                table.CheckConstraint("ck_activation_rights_state", "decision_state IN ('Permitted', 'Denied', 'Unproven')");
                table.ForeignKey(
                    name: "FK_activation_rights_decisions_activation_evidence_bindings_corpus_id_record_revision_document_id_document_version",
                    columns: x => new { x.corpus_id, x.record_revision, x.document_id, x.document_version },
                    principalTable: "activation_evidence_bindings",
                    principalColumns: ActivationDocumentKeyColumns,
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_activation_evidence_bindings_corpus_id_document_id_document_version_source_content_sha256",
            table: "activation_evidence_bindings",
            columns: DocumentSourceIndexColumns);

        migrationBuilder.CreateIndex(
            name: "IX_activation_evidence_bindings_render_manifest_id",
            table: "activation_evidence_bindings",
            column: "render_manifest_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "activation_rights_decisions");

        migrationBuilder.DropTable(
            name: "activation_evidence_bindings");
    }
}
