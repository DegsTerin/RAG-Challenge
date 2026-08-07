// Purpose: Adds open document-language storage and immutable render-manifest bindings while preserving legacy bilingual rows and leaving Vector unchanged.
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RagChallenge.Infrastructure.Persistence.Migrations.Control;

/// <inheritdoc />
public partial class AddDocumentLanguageAndRenderManifestModel : Migration
{
    private static readonly string[] DocumentContentIdentityColumns =
        ["corpus_id", "document_id", "document_version", "content_sha256"];

    private static readonly string[] SourceContentIdentityColumns =
        ["corpus_id", "document_id", "document_version", "source_content_sha256"];

    private static readonly string[] ManifestBindingColumns =
    [
        "render_manifest_id",
        "corpus_id",
        "document_id",
        "document_version",
        "source_content_sha256",
        "render_profile_id",
        "renderer_descriptor",
    ];

    private static readonly string[] PageReproducibilityColumns =
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
        migrationBuilder.DropCheckConstraint(
            name: "ck_document_versions_language",
            table: "document_versions");

        migrationBuilder.AddColumn<string>(
            name: "source_declared_language",
            table: "document_versions",
            type: "TEXT",
            maxLength: 128,
            nullable: true,
            collation: "BINARY");

        migrationBuilder.AddUniqueConstraint(
            name: "AK_document_versions_corpus_id_document_id_document_version_content_sha256",
            table: "document_versions",
            columns: DocumentContentIdentityColumns);

        migrationBuilder.CreateTable(
            name: "document_render_manifests",
            columns: table => new
            {
                render_manifest_id = table.Column<string>(type: "TEXT", maxLength: 79, nullable: false, collation: "BINARY"),
                manifest_sha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "BINARY"),
                schema_version = table.Column<int>(type: "INTEGER", nullable: false),
                corpus_id = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, collation: "BINARY"),
                document_id = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, collation: "BINARY"),
                document_version = table.Column<long>(type: "INTEGER", nullable: false),
                source_content_sha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "BINARY"),
                source_page_count = table.Column<int>(type: "INTEGER", nullable: false),
                render_profile_id = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, collation: "BINARY"),
                renderer_descriptor = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, collation: "BINARY"),
                generated_at_utc = table.Column<string>(type: "TEXT", maxLength: 33, nullable: false, collation: "BINARY")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_document_render_manifests", x => x.render_manifest_id);
                table.UniqueConstraint("AK_document_render_manifests_render_manifest_id_corpus_id_document_id_document_version_source_content_sha256_render_profile_id_renderer_descriptor", x => new { x.render_manifest_id, x.corpus_id, x.document_id, x.document_version, x.source_content_sha256, x.render_profile_id, x.renderer_descriptor });
                table.CheckConstraint("ck_render_manifests_generated_utc", "length(generated_at_utc) = 33 AND substr(generated_at_utc, -6) = '+00:00'");
                table.CheckConstraint("ck_render_manifests_identity", "length(render_manifest_id) = 79 AND substr(render_manifest_id, 1, 15) = 'rendermanifest-' AND substr(render_manifest_id, 16) = manifest_sha256");
                table.CheckConstraint("ck_render_manifests_pages", "source_page_count > 0");
                table.CheckConstraint("ck_render_manifests_profile", "render_profile_id = 'pdf-page-png-v1'");
                table.CheckConstraint("ck_render_manifests_renderer", "length(renderer_descriptor) BETWEEN 1 AND 128 AND renderer_descriptor GLOB '[A-Za-z0-9]*' AND renderer_descriptor NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_render_manifests_schema", "schema_version = 1");
                table.CheckConstraint("ck_render_manifests_sha", "length(manifest_sha256) = 64 AND manifest_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.ForeignKey(
                    name: "FK_document_render_manifests_content_objects_source_content_sha256",
                    column: x => x.source_content_sha256,
                    principalTable: "content_objects",
                    principalColumn: "content_sha256",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_document_render_manifests_document_versions_corpus_id_document_id_document_version_source_content_sha256",
                    columns: x => new { x.corpus_id, x.document_id, x.document_version, x.source_content_sha256 },
                    principalTable: "document_versions",
                    principalColumns: DocumentContentIdentityColumns,
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "document_page_images",
            columns: table => new
            {
                render_manifest_id = table.Column<string>(type: "TEXT", maxLength: 79, nullable: false, collation: "BINARY"),
                page_number = table.Column<int>(type: "INTEGER", nullable: false),
                corpus_id = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, collation: "BINARY"),
                document_id = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, collation: "BINARY"),
                document_version = table.Column<long>(type: "INTEGER", nullable: false),
                source_content_sha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "BINARY"),
                render_profile_id = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, collation: "BINARY"),
                renderer_descriptor = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, collation: "BINARY"),
                image_content_sha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "BINARY"),
                image_sha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "BINARY"),
                byte_length = table.Column<long>(type: "INTEGER", nullable: false),
                media_type = table.Column<string>(type: "TEXT", maxLength: 9, nullable: false, collation: "BINARY"),
                width_pixels = table.Column<int>(type: "INTEGER", nullable: false),
                height_pixels = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_document_page_images", x => new { x.render_manifest_id, x.page_number });
                table.CheckConstraint("ck_page_images_content_identity", "image_content_sha256 = image_sha256");
                table.CheckConstraint("ck_page_images_dimensions", "width_pixels BETWEEN 1 AND 4096 AND height_pixels BETWEEN 1 AND 4096");
                table.CheckConstraint("ck_page_images_length", "byte_length > 0");
                table.CheckConstraint("ck_page_images_media_type", "media_type = 'image/png'");
                table.CheckConstraint("ck_page_images_page", "page_number > 0");
                table.CheckConstraint("ck_page_images_profile", "render_profile_id = 'pdf-page-png-v1'");
                table.CheckConstraint("ck_page_images_renderer", "length(renderer_descriptor) BETWEEN 1 AND 128 AND renderer_descriptor GLOB '[A-Za-z0-9]*' AND renderer_descriptor NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_page_images_sha", "length(image_sha256) = 64 AND image_sha256 NOT GLOB '*[^0-9a-f]*'");
                table.ForeignKey(
                    name: "FK_document_page_images_content_objects_image_content_sha256",
                    column: x => x.image_content_sha256,
                    principalTable: "content_objects",
                    principalColumn: "content_sha256",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_document_page_images_document_render_manifests_render_manifest_id_corpus_id_document_id_document_version_source_content_sha256_render_profile_id_renderer_descriptor",
                    columns: x => new { x.render_manifest_id, x.corpus_id, x.document_id, x.document_version, x.source_content_sha256, x.render_profile_id, x.renderer_descriptor },
                    principalTable: "document_render_manifests",
                    principalColumns: ManifestBindingColumns,
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.AddCheckConstraint(
            name: "ck_document_versions_declared_language",
            table: "document_versions",
            sql: "source_declared_language IS NULL OR (length(source_declared_language) BETWEEN 1 AND 128 AND source_declared_language NOT GLOB '*[^A-Za-z0-9-]*' AND substr(source_declared_language, 1, 1) <> '-' AND substr(source_declared_language, -1) <> '-' AND instr(source_declared_language, '--') = 0)");

        migrationBuilder.AddCheckConstraint(
            name: "ck_document_versions_language",
            table: "document_versions",
            sql: "length(content_language) BETWEEN 1 AND 128 AND content_language NOT GLOB '*[^A-Za-z0-9-]*' AND substr(content_language, 1, 1) <> '-' AND substr(content_language, -1) <> '-' AND instr(content_language, '--') = 0");

        migrationBuilder.CreateIndex(
            name: "IX_document_page_images_corpus_id_document_id_document_version_source_content_sha256_page_number_render_profile_id_renderer_descriptor",
            table: "document_page_images",
            columns: PageReproducibilityColumns,
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_document_page_images_image_content_sha256",
            table: "document_page_images",
            column: "image_content_sha256");

        migrationBuilder.CreateIndex(
            name: "IX_document_page_images_render_manifest_id_corpus_id_document_id_document_version_source_content_sha256_render_profile_id_renderer_descriptor",
            table: "document_page_images",
            columns: ManifestBindingColumns);

        migrationBuilder.CreateIndex(
            name: "IX_document_render_manifests_corpus_id_document_id_document_version_source_content_sha256",
            table: "document_render_manifests",
            columns: SourceContentIdentityColumns);

        migrationBuilder.CreateIndex(
            name: "IX_document_render_manifests_manifest_sha256",
            table: "document_render_manifests",
            column: "manifest_sha256",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_document_render_manifests_source_content_sha256",
            table: "document_render_manifests",
            column: "source_content_sha256");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "document_page_images");

        migrationBuilder.DropTable(
            name: "document_render_manifests");

        migrationBuilder.DropUniqueConstraint(
            name: "AK_document_versions_corpus_id_document_id_document_version_content_sha256",
            table: "document_versions");

        migrationBuilder.DropCheckConstraint(
            name: "ck_document_versions_declared_language",
            table: "document_versions");

        migrationBuilder.DropCheckConstraint(
            name: "ck_document_versions_language",
            table: "document_versions");

        migrationBuilder.DropColumn(
            name: "source_declared_language",
            table: "document_versions");

        migrationBuilder.AddCheckConstraint(
            name: "ck_document_versions_language",
            table: "document_versions",
            sql: "content_language IN ('pt-BR', 'en-GB')");
    }
}
