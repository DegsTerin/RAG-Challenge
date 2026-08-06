// Purpose: Strengthens official snapshot references without changing stored binding columns or canonical identity.
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RagChallenge.Infrastructure.Persistence.Migrations.Control;

/// <inheritdoc />
public partial class StrengthenOfficialBindingReferences : Migration
{
    private static readonly string[] SnapshotIdentityColumns =
        ["corpus_id", "snapshot_id", "registration_id"];

    private static readonly string[] BindingSnapshotColumns =
        ["corpus_id", "official_snapshot_id", "official_registration_id"];

    private static readonly string[] PreviousObservationSnapshotColumns =
        ["corpus_id", "snapshot_id"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_source_observations_official_source_snapshots_corpus_id_snapshot_id",
            table: "source_observations");

        migrationBuilder.DropIndex(
            name: "IX_source_observations_corpus_id_snapshot_id",
            table: "source_observations");

        migrationBuilder.CreateIndex(
            name: "AK_official_source_snapshots_corpus_id_snapshot_id_registration_id",
            table: "official_source_snapshots",
            columns: SnapshotIdentityColumns,
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_source_observations_corpus_id_snapshot_id_registration_id",
            table: "source_observations",
            columns: SnapshotIdentityColumns);

        migrationBuilder.CreateIndex(
            name: "IX_generation_manifest_bindings_corpus_id_official_snapshot_id_official_registration_id",
            table: "generation_manifest_bindings",
            columns: BindingSnapshotColumns);

        migrationBuilder.CreateIndex(
            name: "IX_activation_bindings_corpus_id_official_snapshot_id_official_registration_id",
            table: "activation_bindings",
            columns: BindingSnapshotColumns);

        migrationBuilder.AddForeignKey(
            name: "FK_activation_bindings_official_source_snapshots_corpus_id_official_snapshot_id_official_registration_id",
            table: "activation_bindings",
            columns: BindingSnapshotColumns,
            principalTable: "official_source_snapshots",
            principalColumns: SnapshotIdentityColumns,
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_generation_manifest_bindings_official_source_snapshots_corpus_id_official_snapshot_id_official_registration_id",
            table: "generation_manifest_bindings",
            columns: BindingSnapshotColumns,
            principalTable: "official_source_snapshots",
            principalColumns: SnapshotIdentityColumns,
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_source_observations_official_source_snapshots_corpus_id_snapshot_id_registration_id",
            table: "source_observations",
            columns: SnapshotIdentityColumns,
            principalTable: "official_source_snapshots",
            principalColumns: SnapshotIdentityColumns,
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_activation_bindings_official_source_snapshots_corpus_id_official_snapshot_id_official_registration_id",
            table: "activation_bindings");

        migrationBuilder.DropForeignKey(
            name: "FK_generation_manifest_bindings_official_source_snapshots_corpus_id_official_snapshot_id_official_registration_id",
            table: "generation_manifest_bindings");

        migrationBuilder.DropForeignKey(
            name: "FK_source_observations_official_source_snapshots_corpus_id_snapshot_id_registration_id",
            table: "source_observations");

        migrationBuilder.DropIndex(
            name: "IX_source_observations_corpus_id_snapshot_id_registration_id",
            table: "source_observations");

        migrationBuilder.DropIndex(
            name: "AK_official_source_snapshots_corpus_id_snapshot_id_registration_id",
            table: "official_source_snapshots");

        migrationBuilder.DropIndex(
            name: "IX_generation_manifest_bindings_corpus_id_official_snapshot_id_official_registration_id",
            table: "generation_manifest_bindings");

        migrationBuilder.DropIndex(
            name: "IX_activation_bindings_corpus_id_official_snapshot_id_official_registration_id",
            table: "activation_bindings");

        migrationBuilder.CreateIndex(
            name: "IX_source_observations_corpus_id_snapshot_id",
            table: "source_observations",
            columns: PreviousObservationSnapshotColumns);

        migrationBuilder.AddForeignKey(
            name: "FK_source_observations_official_source_snapshots_corpus_id_snapshot_id",
            table: "source_observations",
            columns: PreviousObservationSnapshotColumns,
            principalTable: "official_source_snapshots",
            principalColumns: PreviousObservationSnapshotColumns,
            onDelete: ReferentialAction.Restrict);
    }
}
