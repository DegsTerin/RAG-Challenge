// Purpose: Adds the governed administration lease and catalogue-scoped lifecycle projections without replacing existing catalogue data.
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RagChallenge.Infrastructure.Persistence.Migrations.Control;

/// <inheritdoc />
public partial class AddAdministrationLease : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "status",
            table: "catalogue_revision_products",
            type: "TEXT",
            maxLength: 2048,
            nullable: false,
            defaultValue: "",
            collation: "BINARY");

        migrationBuilder.AddColumn<string>(
            name: "product_id",
            table: "catalogue_revision_documents",
            type: "TEXT",
            maxLength: 2048,
            nullable: false,
            defaultValue: "",
            collation: "BINARY");

        migrationBuilder.AddColumn<long>(
            name: "product_revision",
            table: "catalogue_revision_documents",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0L);

        migrationBuilder.Sql(
            """
                UPDATE catalogue_revision_products
                SET status = (
                    SELECT product.status
                    FROM database_product_revisions AS product
                    WHERE product.corpus_id = catalogue_revision_products.corpus_id
                      AND product.product_id = catalogue_revision_products.product_id
                      AND product.product_revision = catalogue_revision_products.product_revision
                );

                UPDATE catalogue_revision_documents
                SET product_id = (
                        SELECT document.product_id
                        FROM document_versions AS document
                        WHERE document.corpus_id = catalogue_revision_documents.corpus_id
                          AND document.document_id = catalogue_revision_documents.document_id
                          AND document.document_version = catalogue_revision_documents.document_version
                    ),
                    product_revision = (
                        SELECT document.product_revision
                        FROM document_versions AS document
                        WHERE document.corpus_id = catalogue_revision_documents.corpus_id
                          AND document.document_id = catalogue_revision_documents.document_id
                          AND document.document_version = catalogue_revision_documents.document_version
                    );
                """);

        migrationBuilder.CreateTable(
            name: "administration_leases",
            columns: table => new
            {
                corpus_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                operation_id = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                acquired_at_utc = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY"),
                expires_at_utc = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false, collation: "BINARY")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_administration_leases", x => x.corpus_id);
                table.CheckConstraint("ck_administration_leases_acquired_utc", "length(acquired_at_utc) = 33 AND substr(acquired_at_utc, -6) = '+00:00'");
                table.CheckConstraint("ck_administration_leases_corpus", "length(corpus_id) BETWEEN 1 AND 128 AND corpus_id GLOB '[A-Za-z0-9]*' AND corpus_id NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_administration_leases_expires_utc", "length(expires_at_utc) = 33 AND substr(expires_at_utc, -6) = '+00:00'");
                table.CheckConstraint("ck_administration_leases_operation", "length(operation_id) BETWEEN 1 AND 128 AND operation_id GLOB '[A-Za-z0-9]*' AND operation_id NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.CheckConstraint("ck_administration_leases_time_order", "expires_at_utc > acquired_at_utc");
            });

        migrationBuilder.CreateIndex(
            name: "IX_administration_leases_operation_id",
            table: "administration_leases",
            column: "operation_id",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "administration_leases");

        migrationBuilder.DropColumn(
            name: "status",
            table: "catalogue_revision_products");

        migrationBuilder.DropColumn(
            name: "product_id",
            table: "catalogue_revision_documents");

        migrationBuilder.DropColumn(
            name: "product_revision",
            table: "catalogue_revision_documents");
    }
}
