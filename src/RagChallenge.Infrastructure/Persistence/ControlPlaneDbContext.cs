// Purpose: Maps the authoritative control.db schema and its physical constraints while Domain and Application remain persistence-independent.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata;

namespace RagChallenge.Infrastructure.Persistence;

public sealed class ControlPlaneDbContext(DbContextOptions<ControlPlaneDbContext> options)
    : DbContext(options)
{
    internal DbSet<CorpusRow> Corpora => Set<CorpusRow>();

    internal DbSet<DatabaseCategoryRow> DatabaseCategories => Set<DatabaseCategoryRow>();

    internal DbSet<DatabaseProductRevisionRow> DatabaseProductRevisions =>
        Set<DatabaseProductRevisionRow>();

    internal DbSet<DatabaseProductCategoryRow> DatabaseProductCategories =>
        Set<DatabaseProductCategoryRow>();

    internal DbSet<ContentObjectRow> ContentObjects => Set<ContentObjectRow>();

    internal DbSet<DocumentVersionRow> DocumentVersions => Set<DocumentVersionRow>();

    internal DbSet<CatalogueRevisionRow> CatalogueRevisions => Set<CatalogueRevisionRow>();

    internal DbSet<CatalogueRevisionProductRow> CatalogueRevisionProducts =>
        Set<CatalogueRevisionProductRow>();

    internal DbSet<CatalogueRevisionDocumentRow> CatalogueRevisionDocuments =>
        Set<CatalogueRevisionDocumentRow>();

    internal DbSet<CatalogueHeadRow> CatalogueHeads => Set<CatalogueHeadRow>();

    internal DbSet<OfficialSourceRegistrationRow> OfficialSourceRegistrations =>
        Set<OfficialSourceRegistrationRow>();

    internal DbSet<OfficialSourceSnapshotRow> OfficialSourceSnapshots =>
        Set<OfficialSourceSnapshotRow>();

    internal DbSet<SourceObservationRow> SourceObservations => Set<SourceObservationRow>();

    internal DbSet<ObservationJournalHeadRow> ObservationJournalHeads =>
        Set<ObservationJournalHeadRow>();

    internal DbSet<GenerationManifestRow> GenerationManifests => Set<GenerationManifestRow>();

    internal DbSet<GenerationManifestBindingRow> GenerationManifestBindings =>
        Set<GenerationManifestBindingRow>();

    internal DbSet<ActivationRecordRow> ActivationRecords => Set<ActivationRecordRow>();

    internal DbSet<ActivationBindingRow> ActivationBindings => Set<ActivationBindingRow>();

    internal DbSet<ActivationHeadRow> ActivationHeads => Set<ActivationHeadRow>();

    internal DbSet<GenerationRetentionRow> GenerationRetentions =>
        Set<GenerationRetentionRow>();

    internal DbSet<AdminOperationRow> AdminOperations => Set<AdminOperationRow>();

    internal DbSet<AuditEventRow> AuditEvents => Set<AuditEventRow>();

    internal DbSet<RecoveryLeaseRow> RecoveryLeases => Set<RecoveryLeaseRow>();

    internal DbSet<AdministrationLeaseRow> AdministrationLeases =>
        Set<AdministrationLeaseRow>();

    internal DbSet<AdministrationCommandJournalRow> AdministrationCommandJournal =>
        Set<AdministrationCommandJournalRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureCorpora(modelBuilder);
        ConfigureCatalogue(modelBuilder);
        ConfigureOfficialSources(modelBuilder);
        ConfigureGenerations(modelBuilder);
        ConfigureActivations(modelBuilder);
        ConfigureOperations(modelBuilder);
        ApplyPhysicalConventions(modelBuilder);
    }

    private static void ConfigureCorpora(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CorpusRow>(entity =>
        {
            entity.ToTable("corpora", table =>
            {
                table.HasCheckConstraint("ck_corpora_id", StableId("corpus_id"));
                table.HasCheckConstraint("ck_corpora_revision", "corpus_revision > 0");
                table.HasCheckConstraint("ck_corpora_created_utc", UtcInstant("created_at_utc"));
            });
            entity.HasKey(row => row.CorpusId);
        });
    }

    private static void ConfigureCatalogue(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DatabaseCategoryRow>(entity =>
        {
            entity.ToTable("database_categories", table =>
            {
                table.HasCheckConstraint("ck_database_categories_id", StableId("category_id"));
                table.HasCheckConstraint("ck_database_categories_name", "length(display_name) BETWEEN 1 AND 256");
            });
            entity.HasKey(row => new { row.CorpusId, row.CategoryId });
            entity.HasOne<CorpusRow>().WithMany().HasForeignKey(row => row.CorpusId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DatabaseProductRevisionRow>(entity =>
        {
            entity.ToTable("database_product_revisions", table =>
            {
                table.HasCheckConstraint("ck_database_products_id", StableId("product_id"));
                table.HasCheckConstraint("ck_database_products_revision", "product_revision > 0");
                table.HasCheckConstraint("ck_database_products_name", "length(display_name) BETWEEN 1 AND 256");
                table.HasCheckConstraint("ck_database_products_status", CatalogueStatus("status"));
            });
            entity.HasKey(row => new { row.CorpusId, row.ProductId, row.ProductRevision });
            entity.HasOne<CorpusRow>().WithMany().HasForeignKey(row => row.CorpusId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DatabaseProductCategoryRow>(entity =>
        {
            entity.ToTable("database_product_categories");
            entity.HasKey(row => new
            {
                row.CorpusId,
                row.ProductId,
                row.ProductRevision,
                row.CategoryId,
            });
            entity.HasOne<DatabaseProductRevisionRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.ProductId,
                row.ProductRevision,
            }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<DatabaseCategoryRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.CategoryId,
            }).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ContentObjectRow>(entity =>
        {
            entity.ToTable("content_objects", table =>
            {
                table.HasCheckConstraint("ck_content_objects_sha", Sha256("content_sha256"));
                table.HasCheckConstraint("ck_content_objects_length", "byte_length > 0");
                table.HasCheckConstraint("ck_content_objects_registered_utc", UtcInstant("registered_at_utc"));
            });
            entity.HasKey(row => row.ContentSha256);
        });

        modelBuilder.Entity<DocumentVersionRow>(entity =>
        {
            entity.ToTable("document_versions", table =>
            {
                table.HasCheckConstraint("ck_document_versions_id", StableId("document_id"));
                table.HasCheckConstraint("ck_document_versions_version", "document_version > 0");
                table.HasCheckConstraint("ck_document_versions_format", "document_format IN ('Pdf', 'Csv')");
                table.HasCheckConstraint("ck_document_versions_language", "content_language IN ('pt-BR', 'en-GB')");
                table.HasCheckConstraint("ck_document_versions_length", "byte_length > 0");
                table.HasCheckConstraint("ck_document_versions_media_type", "length(media_type) BETWEEN 1 AND 128");
                table.HasCheckConstraint("ck_document_versions_trust", TrustClass("source_trust_class"));
                table.HasCheckConstraint(
                    "ck_document_versions_source_identity",
                    "(source_trust_class = 'LocalAuthorised' AND official_registration_id IS NULL AND official_snapshot_id IS NULL) OR " +
                    "(source_trust_class = 'OfficialExternal' AND official_registration_id IS NOT NULL AND official_snapshot_id IS NOT NULL)");
            });
            entity.HasKey(row => new { row.CorpusId, row.DocumentId, row.DocumentVersion });
            entity.HasOne<DatabaseProductRevisionRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.ProductId,
                row.ProductRevision,
            }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ContentObjectRow>().WithMany().HasForeignKey(row => row.ContentSha256).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CatalogueRevisionRow>(entity =>
        {
            entity.ToTable("catalogue_revisions", table =>
            {
                table.HasCheckConstraint("ck_catalogue_revisions_revision", "catalogue_revision > 0");
                table.HasCheckConstraint("ck_catalogue_revisions_created_utc", UtcInstant("created_at_utc"));
            });
            entity.HasKey(row => new { row.CorpusId, row.CatalogueRevision });
            entity.HasOne<CorpusRow>().WithMany().HasForeignKey(row => row.CorpusId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(row => row.OperationId).IsUnique();
            entity.HasOne<AdminOperationRow>().WithMany().HasForeignKey(row => row.OperationId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CatalogueRevisionProductRow>(entity =>
        {
            entity.ToTable("catalogue_revision_products");
            entity.HasKey(row => new
            {
                row.CorpusId,
                row.CatalogueRevision,
                row.ProductId,
            });
            entity.HasOne<CatalogueRevisionRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.CatalogueRevision,
            }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<DatabaseProductRevisionRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.ProductId,
                row.ProductRevision,
            }).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CatalogueRevisionDocumentRow>(entity =>
        {
            entity.ToTable("catalogue_revision_documents", table =>
                table.HasCheckConstraint(
                    "ck_catalogue_revision_documents_status",
                    CatalogueStatus("status")));
            entity.HasKey(row => new
            {
                row.CorpusId,
                row.CatalogueRevision,
                row.DocumentId,
                row.DocumentVersion,
            });
            entity.HasOne<CatalogueRevisionRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.CatalogueRevision,
            }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<DocumentVersionRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.DocumentId,
                row.DocumentVersion,
            }).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(row => new
            {
                row.CorpusId,
                row.CatalogueRevision,
                row.DocumentId,
            }).IsUnique()
                .HasFilter("status = 'Active'")
                .HasDatabaseName("ux_catalogue_revision_documents_one_active");
        });

        modelBuilder.Entity<CatalogueHeadRow>(entity =>
        {
            entity.ToTable("catalogue_heads", table =>
            {
                table.HasCheckConstraint("ck_catalogue_heads_revision", "catalogue_revision > 0");
                table.HasCheckConstraint("ck_catalogue_heads_row_revision", "row_revision > 0");
            });
            entity.HasKey(row => row.CorpusId);
            entity.HasOne<CatalogueRevisionRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.CatalogueRevision,
            }).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureOfficialSources(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OfficialSourceRegistrationRow>(entity =>
        {
            entity.ToTable("official_source_registrations", table =>
            {
                table.HasCheckConstraint("ck_source_registrations_id", StableId("registration_id"));
                table.HasCheckConstraint("ck_source_registrations_revision", "registration_revision > 0");
                table.HasCheckConstraint("ck_source_registrations_status", CatalogueStatus("status"));
                table.HasCheckConstraint("ck_source_registrations_https", "canonical_https_url GLOB 'https://*' AND instr(canonical_https_url, '#') = 0");
            });
            entity.HasKey(row => new
            {
                row.CorpusId,
                row.RegistrationId,
                row.RegistrationRevision,
            });
            entity.HasOne<CorpusRow>().WithMany().HasForeignKey(row => row.CorpusId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OfficialSourceSnapshotRow>(entity =>
        {
            entity.ToTable("official_source_snapshots", table =>
            {
                table.HasCheckConstraint("ck_source_snapshots_id", StableId("snapshot_id"));
                table.HasCheckConstraint("ck_source_snapshots_length", "byte_length > 0");
                table.HasCheckConstraint("ck_source_snapshots_media_type", "length(media_type) BETWEEN 1 AND 128");
                table.HasCheckConstraint("ck_source_snapshots_retrieved_utc", UtcInstant("retrieved_at_utc"));
            });
            entity.HasKey(row => new { row.CorpusId, row.SnapshotId });
            entity.HasAlternateKey(row => new
            {
                row.CorpusId,
                row.SnapshotId,
                row.RegistrationId,
            });
            entity.HasOne<OfficialSourceRegistrationRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.RegistrationId,
                row.RegistrationRevision,
            }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ContentObjectRow>().WithMany().HasForeignKey(row => row.ContentSha256).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SourceObservationRow>(entity =>
        {
            entity.ToTable("source_observations", table =>
            {
                table.HasCheckConstraint("ck_source_observations_id", StableId("observation_id"));
                table.HasCheckConstraint("ck_source_observations_revision", "journal_revision > 0");
                table.HasCheckConstraint("ck_source_observations_state", "state IN ('Current', 'Stale', 'Withdrawn', 'Deactivated')");
                table.HasCheckConstraint("ck_source_observations_revalidated_utc", UtcInstant("revalidated_at_utc"));
                table.HasCheckConstraint("ck_source_observations_max_age", "max_age_seconds > 0");
            });
            entity.HasKey(row => new { row.CorpusId, row.ObservationId });
            entity.HasOne<OfficialSourceSnapshotRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.SnapshotId,
                row.RegistrationId,
            }).HasPrincipalKey(snapshot => new
            {
                snapshot.CorpusId,
                snapshot.SnapshotId,
                snapshot.RegistrationId,
            }).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(row => new { row.CorpusId, row.JournalRevision }).IsUnique();
            entity.HasIndex(row => row.OperationId).IsUnique();
            entity.HasOne<AdminOperationRow>().WithMany().HasForeignKey(row => row.OperationId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ObservationJournalHeadRow>(entity =>
        {
            entity.ToTable("observation_journal_heads", table =>
            {
                table.HasCheckConstraint("ck_observation_heads_revision", "journal_revision > 0");
                table.HasCheckConstraint("ck_observation_heads_row_revision", "row_revision > 0");
            });
            entity.HasKey(row => row.CorpusId);
            entity.HasOne<CorpusRow>().WithMany().HasForeignKey(row => row.CorpusId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureGenerations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GenerationManifestRow>(entity =>
        {
            entity.ToTable("generation_manifests", table =>
            {
                table.HasCheckConstraint("ck_generation_manifests_id", GenerationId("index_generation_id"));
                table.HasCheckConstraint("ck_generation_manifests_candidate", StableId("candidate_build_id"));
                table.HasCheckConstraint("ck_generation_manifests_schema", "manifest_schema_version > 0");
                table.HasCheckConstraint("ck_generation_manifests_revisions", "corpus_revision > 0 AND catalogue_revision > 0");
                table.HasCheckConstraint("ck_generation_manifests_active_digest", Sha256("active_document_set_digest"));
                table.HasCheckConstraint("ck_generation_manifests_source_digest", Sha256("source_binding_set_digest"));
                table.HasCheckConstraint("ck_generation_manifests_compatibility", Sha256("index_compatibility_key"));
                table.HasCheckConstraint("ck_generation_manifests_spec", Sha256("generation_spec_digest"));
                table.HasCheckConstraint("ck_generation_manifests_logical", Sha256("logical_artifact_digest"));
                table.HasCheckConstraint("ck_generation_manifests_content", Sha256("generation_content_digest"));
                table.HasCheckConstraint("ck_generation_manifests_counts", "chunk_count > 0 AND vector_count = chunk_count");
                table.HasCheckConstraint("ck_generation_manifests_identity", "index_generation_id = 'idxgen-' || generation_content_digest");
                table.HasCheckConstraint("ck_generation_manifests_finalised_utc", UtcInstant("finalised_at_utc"));
            });
            entity.HasKey(row => new { row.CorpusId, row.IndexGenerationId });
            entity.HasIndex(row => row.IndexGenerationId).IsUnique();
            entity.HasIndex(row => new { row.CorpusId, row.CandidateBuildId }).IsUnique();
            entity.HasIndex(row => row.OperationId).IsUnique();
            entity.HasOne<AdminOperationRow>().WithMany().HasForeignKey(row => row.OperationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<CorpusRow>().WithMany().HasForeignKey(row => row.CorpusId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<CatalogueRevisionRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.CatalogueRevision,
            }).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GenerationManifestBindingRow>(entity =>
        {
            entity.ToTable("generation_manifest_bindings", table =>
            {
                table.HasCheckConstraint("ck_generation_bindings_format", "document_format IN ('Pdf', 'Csv')");
                table.HasCheckConstraint("ck_generation_bindings_trust", TrustClass("source_trust_class"));
                table.HasCheckConstraint(
                    "ck_generation_bindings_source_identity",
                    "(source_trust_class = 'LocalAuthorised' AND official_registration_id IS NULL AND official_snapshot_id IS NULL) OR " +
                    "(source_trust_class = 'OfficialExternal' AND official_registration_id IS NOT NULL AND official_snapshot_id IS NOT NULL)");
            });
            entity.HasKey(row => new
            {
                row.CorpusId,
                row.IndexGenerationId,
                row.DocumentId,
                row.DocumentVersion,
            });
            entity.HasOne<GenerationManifestRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.IndexGenerationId,
            }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<DocumentVersionRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.DocumentId,
                row.DocumentVersion,
            }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<OfficialSourceSnapshotRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.OfficialSnapshotId,
                row.OfficialRegistrationId,
            }).HasPrincipalKey(snapshot => new
            {
                snapshot.CorpusId,
                snapshot.SnapshotId,
                snapshot.RegistrationId,
            }).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureActivations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActivationRecordRow>(entity =>
        {
            entity.ToTable("activation_records", table =>
            {
                table.HasCheckConstraint("ck_activation_records_revision", "record_revision > 0");
                table.HasCheckConstraint(
                    "ck_activation_records_lineage",
                    "(record_revision = 1 AND previous_record_revision IS NULL) OR " +
                    "(record_revision > 1 AND previous_record_revision = record_revision - 1)");
                table.HasCheckConstraint("ck_activation_records_digest", Sha256("activation_binding_set_digest"));
                table.HasCheckConstraint("ck_activation_records_kind", "mutation_kind IN ('Initial', 'Replacement', 'ObservationRebind', 'Rollback')");
                table.HasCheckConstraint("ck_activation_records_activated_utc", UtcInstant("generation_activated_at_utc"));
                table.HasCheckConstraint("ck_activation_records_updated_utc", UtcInstant("record_updated_at_utc"));
                table.HasCheckConstraint("ck_activation_records_time_order", "record_updated_at_utc >= generation_activated_at_utc");
            });
            entity.HasKey(row => new { row.CorpusId, row.RecordRevision });
            entity.HasIndex(row => row.OperationId).IsUnique();
            entity.HasOne<AdminOperationRow>().WithMany().HasForeignKey(row => row.OperationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<GenerationManifestRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.IndexGenerationId,
            }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<CatalogueRevisionRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.CatalogueRevision,
            }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ActivationRecordRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.PreviousRecordRevision,
            }).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ActivationBindingRow>(entity =>
        {
            entity.ToTable("activation_bindings", table =>
            {
                table.HasCheckConstraint("ck_activation_bindings_format", "document_format IN ('Pdf', 'Csv')");
                table.HasCheckConstraint("ck_activation_bindings_trust", TrustClass("source_trust_class"));
                table.HasCheckConstraint(
                    "ck_activation_bindings_source_identity",
                    "(source_trust_class = 'LocalAuthorised' AND official_registration_id IS NULL AND official_snapshot_id IS NULL AND source_observation_id IS NULL) OR " +
                    "(source_trust_class = 'OfficialExternal' AND official_registration_id IS NOT NULL AND official_snapshot_id IS NOT NULL AND source_observation_id IS NOT NULL)");
            });
            entity.HasKey(row => new
            {
                row.CorpusId,
                row.RecordRevision,
                row.DocumentId,
                row.DocumentVersion,
            });
            entity.HasOne<ActivationRecordRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.RecordRevision,
            }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<DocumentVersionRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.DocumentId,
                row.DocumentVersion,
            }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<SourceObservationRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.SourceObservationId,
            }).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<OfficialSourceSnapshotRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.OfficialSnapshotId,
                row.OfficialRegistrationId,
            }).HasPrincipalKey(snapshot => new
            {
                snapshot.CorpusId,
                snapshot.SnapshotId,
                snapshot.RegistrationId,
            }).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ActivationHeadRow>(entity =>
        {
            entity.ToTable("activation_heads", table =>
            {
                table.HasCheckConstraint("ck_activation_heads_revision", "record_revision > 0");
                table.HasCheckConstraint("ck_activation_heads_row_revision", "row_revision > 0");
            });
            entity.HasKey(row => row.CorpusId);
            entity.HasOne<ActivationRecordRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.RecordRevision,
            }).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GenerationRetentionRow>(entity =>
        {
            entity.ToTable("generation_retention", table =>
            {
                table.HasCheckConstraint("ck_generation_retention_role", "protection_role IN ('Active', 'Previous', 'Hold')");
                table.HasCheckConstraint("ck_generation_retention_until_utc", UtcInstant("retain_until_utc"));
                table.HasCheckConstraint("ck_generation_retention_recorded_utc", UtcInstant("recorded_at_utc"));
                table.HasCheckConstraint("ck_generation_retention_time_order", "retain_until_utc >= recorded_at_utc");
            });
            entity.HasKey(row => new { row.CorpusId, row.IndexGenerationId });
            entity.HasIndex(row => row.CorpusId)
                .IsUnique()
                .HasFilter("protection_role = 'Active'")
                .HasDatabaseName("ux_generation_retention_active");
            entity.HasIndex(row => row.CorpusId)
                .IsUnique()
                .HasFilter("protection_role = 'Previous'")
                .HasDatabaseName("ux_generation_retention_previous");
            entity.HasOne<GenerationManifestRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.IndexGenerationId,
            }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<AdminOperationRow>().WithMany().HasForeignKey(row => row.OperationId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureOperations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminOperationRow>(entity =>
        {
            entity.ToTable("admin_operations", table =>
            {
                table.HasCheckConstraint("ck_admin_operations_id", StableId("operation_id"));
                table.HasCheckConstraint("ck_admin_operations_status", "status IN ('InProgress', 'Applied', 'Rejected', 'Failed')");
                table.HasCheckConstraint("ck_admin_operations_expected", "expected_revision IS NULL OR expected_revision >= 0");
                table.HasCheckConstraint("ck_admin_operations_result", "result_revision IS NULL OR result_revision > 0");
                table.HasCheckConstraint("ck_admin_operations_requested_utc", UtcInstant("requested_at_utc"));
                table.HasCheckConstraint("ck_admin_operations_completed_utc", "completed_at_utc IS NULL OR " + UtcInstant("completed_at_utc"));
            });
            entity.HasKey(row => row.OperationId);
            entity.HasOne<CorpusRow>().WithMany().HasForeignKey(row => row.CorpusId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditEventRow>(entity =>
        {
            entity.ToTable("audit_events", table =>
            {
                table.HasCheckConstraint("ck_audit_events_id", StableId("audit_event_id"));
                table.HasCheckConstraint("ck_audit_events_occurred_utc", UtcInstant("occurred_at_utc"));
                table.HasCheckConstraint("ck_audit_events_details", Sha256("details_digest"));
            });
            entity.HasKey(row => row.AuditEventId);
            entity.HasIndex(row => new { row.CorpusId, row.OccurredAtUtc });
            entity.HasOne<AdminOperationRow>().WithMany().HasForeignKey(row => row.OperationId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RecoveryLeaseRow>(entity =>
        {
            entity.ToTable("recovery_leases", table =>
            {
                table.HasCheckConstraint("ck_recovery_leases_name", "length(lease_name) BETWEEN 1 AND 64");
                table.HasCheckConstraint("ck_recovery_leases_acquired_utc", UtcInstant("acquired_at_utc"));
                table.HasCheckConstraint("ck_recovery_leases_expires_utc", UtcInstant("expires_at_utc"));
                table.HasCheckConstraint("ck_recovery_leases_time_order", "expires_at_utc > acquired_at_utc");
            });
            entity.HasKey(row => new { row.CorpusId, row.LeaseName });
            entity.HasOne<AdminOperationRow>().WithMany().HasForeignKey(row => row.OperationId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AdministrationLeaseRow>(entity =>
        {
            entity.ToTable("administration_leases", table =>
            {
                table.HasCheckConstraint("ck_administration_leases_corpus", StableId("corpus_id"));
                table.HasCheckConstraint("ck_administration_leases_operation", StableId("operation_id"));
                table.HasCheckConstraint("ck_administration_leases_acquired_utc", UtcInstant("acquired_at_utc"));
                table.HasCheckConstraint("ck_administration_leases_expires_utc", UtcInstant("expires_at_utc"));
                table.HasCheckConstraint("ck_administration_leases_time_order", "expires_at_utc > acquired_at_utc");
            });
            entity.HasKey(row => row.CorpusId);
            entity.HasIndex(row => row.OperationId).IsUnique();
        });

        modelBuilder.Entity<AdministrationCommandJournalRow>(entity =>
        {
            entity.ToTable("administration_command_journal", table =>
            {
                table.HasCheckConstraint("ck_administration_journal_operation", StableId("operation_id"));
                table.HasCheckConstraint("ck_administration_journal_corpus", StableId("corpus_id"));
                table.HasCheckConstraint("ck_administration_journal_command", "length(command) BETWEEN 1 AND 64 AND command NOT GLOB '*[^a-z0-9-]*'");
                table.HasCheckConstraint("ck_administration_journal_actor", "length(actor_identifier) BETWEEN 1 AND 128 AND actor_identifier NOT GLOB '*[^A-Za-z0-9._:-]*'");
                table.HasCheckConstraint("ck_administration_journal_reason", Sha256("reason_sha256"));
                table.HasCheckConstraint("ck_administration_journal_input", "input_sha256 IS NULL OR " + Sha256("input_sha256"));
                table.HasCheckConstraint("ck_administration_journal_intent", Sha256("intent_digest"));
                table.HasCheckConstraint("ck_administration_journal_sources", "length(source_ids_json) BETWEEN 2 AND 4096");
                table.HasCheckConstraint("ck_administration_journal_targets", "length(target_ids_json) BETWEEN 2 AND 4096");
                table.HasCheckConstraint("ck_administration_journal_started_utc", UtcInstant("started_at_utc"));
                table.HasCheckConstraint("ck_administration_journal_completed_utc", "completed_at_utc IS NULL OR " + UtcInstant("completed_at_utc"));
                table.HasCheckConstraint("ck_administration_journal_time_order", "completed_at_utc IS NULL OR completed_at_utc >= started_at_utc");
                table.HasCheckConstraint("ck_administration_journal_status", "status IN ('Started', 'Completed')");
                table.HasCheckConstraint("ck_administration_journal_outcome", "outcome IS NULL OR outcome IN ('Applied', 'Rejected', 'Unavailable', 'Failed')");
                table.HasCheckConstraint("ck_administration_journal_exit", "exit_category IS NULL OR exit_category IN (0, 2, 3, 4, 5, 10)");
                table.HasCheckConstraint("ck_administration_journal_result_revision", "result_revision IS NULL OR result_revision >= 0");
                table.HasCheckConstraint("ck_administration_journal_completion", "(status = 'Started' AND completed_at_utc IS NULL AND outcome IS NULL AND result_code IS NULL AND exit_category IS NULL AND result_revision IS NULL) OR (status = 'Completed' AND completed_at_utc IS NOT NULL AND outcome IS NOT NULL AND result_code IS NOT NULL AND exit_category IS NOT NULL)");
            });
            entity.HasKey(row => row.OperationId);
            entity.HasIndex(row => new { row.CorpusId, row.StartedAtUtc });
            entity.Property(row => row.SourceIdsJson).HasMaxLength(4096);
            entity.Property(row => row.TargetIdsJson).HasMaxLength(4096);
            entity.Property(row => row.ResultCode).HasMaxLength(128);
        });
    }

    private static void ApplyPhysicalConventions(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));

                if (property.ClrType == typeof(string))
                {
                    property.SetCollation("BINARY");
                    property.SetMaxLength(property.GetMaxLength() ?? 2048);
                }
            }
        }
    }

    private static string ToSnakeCase(string value) =>
        string.Concat(value.Select((character, index) =>
            char.IsUpper(character) && index > 0
                ? $"_{char.ToLowerInvariant(character)}"
                : char.ToLowerInvariant(character).ToString()));

    private static string StableId(string column) =>
        $"length({column}) BETWEEN 1 AND 128 AND " +
        $"{column} GLOB '[A-Za-z0-9]*' AND " +
        $"{column} NOT GLOB '*[^A-Za-z0-9._:-]*'";

    private static string Sha256(string column) =>
        $"length({column}) = 64 AND {column} NOT GLOB '*[^0-9a-f]*'";

    private static string GenerationId(string column) =>
        $"length({column}) = 71 AND substr({column}, 1, 7) = 'idxgen-' AND " +
        $"substr({column}, 8) NOT GLOB '*[^0-9a-f]*'";

    private static string UtcInstant(string column) =>
        $"length({column}) = 33 AND substr({column}, -6) = '+00:00'";

    private static string CatalogueStatus(string column) =>
        $"{column} IN ('Candidate', 'Active', 'Deactivated', 'Removed')";

    private static string TrustClass(string column) =>
        $"{column} IN ('LocalAuthorised', 'OfficialExternal')";
}

public sealed class ControlPlaneDesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<ControlPlaneDbContext>
{
    public ControlPlaneDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ControlPlaneDbContext>()
            .Configure(DesignTimeStorePath.Resolve("control.db"))
            .Options;

        return new ControlPlaneDbContext(options);
    }
}
