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

    internal DbSet<DerivativeObligationSetRow> DerivativeObligationSets =>
        Set<DerivativeObligationSetRow>();

    internal DbSet<DerivativeObligationEvidenceReferenceRow>
        DerivativeObligationEvidenceReferences =>
            Set<DerivativeObligationEvidenceReferenceRow>();

    internal DbSet<DerivativeObligationDisclaimerRow> DerivativeObligationDisclaimers =>
        Set<DerivativeObligationDisclaimerRow>();

    internal DbSet<DocumentRenderManifestRow> DocumentRenderManifests =>
        Set<DocumentRenderManifestRow>();

    internal DbSet<DocumentPageImageRow> DocumentPageImages => Set<DocumentPageImageRow>();

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

    internal DbSet<ActivationEvidenceBindingRow> ActivationEvidenceBindings =>
        Set<ActivationEvidenceBindingRow>();

    internal DbSet<ActivationRightsDecisionRow> ActivationRightsDecisions =>
        Set<ActivationRightsDecisionRow>();

    internal DbSet<ActivationHeadRow> ActivationHeads => Set<ActivationHeadRow>();

    internal DbSet<GenerationRetentionRow> GenerationRetentions =>
        Set<GenerationRetentionRow>();

    internal DbSet<AnswerEvidenceRecordRow> AnswerEvidenceRecords =>
        Set<AnswerEvidenceRecordRow>();

    internal DbSet<AnswerEvidenceCitationRow> AnswerEvidenceCitations =>
        Set<AnswerEvidenceCitationRow>();

    internal DbSet<AnswerEvidencePageRow> AnswerEvidencePages =>
        Set<AnswerEvidencePageRow>();

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
        ConfigureDerivativeObligations(modelBuilder);
        ConfigureOfficialSources(modelBuilder);
        ConfigureGenerations(modelBuilder);
        ConfigureActivations(modelBuilder);
        ConfigureAnswerEvidence(modelBuilder);
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
                table.HasCheckConstraint("ck_document_versions_language", Bcp47Shape("content_language"));
                table.HasCheckConstraint(
                    "ck_document_versions_declared_language",
                    $"source_declared_language IS NULL OR ({Bcp47Shape("source_declared_language")})");
                table.HasCheckConstraint("ck_document_versions_length", "byte_length > 0");
                table.HasCheckConstraint("ck_document_versions_media_type", "length(media_type) BETWEEN 1 AND 128");
                table.HasCheckConstraint("ck_document_versions_trust", TrustClass("source_trust_class"));
                table.HasCheckConstraint(
                    "ck_document_versions_source_identity",
                    "(source_trust_class = 'LocalAuthorised' AND official_registration_id IS NULL AND official_snapshot_id IS NULL) OR " +
                    "(source_trust_class = 'OfficialExternal' AND official_registration_id IS NOT NULL AND official_snapshot_id IS NOT NULL)");
            });
            entity.HasKey(row => new { row.CorpusId, row.DocumentId, row.DocumentVersion });
            entity.HasAlternateKey(row => new
            {
                row.CorpusId,
                row.DocumentId,
                row.DocumentVersion,
                row.ContentSha256,
            });
            entity.Property(row => row.ContentLanguage).HasMaxLength(128);
            entity.Property(row => row.SourceDeclaredLanguage).HasMaxLength(128);
            entity.HasOne<DatabaseProductRevisionRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.ProductId,
                row.ProductRevision,
            }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ContentObjectRow>().WithMany().HasForeignKey(row => row.ContentSha256).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DocumentRenderManifestRow>(entity =>
        {
            entity.ToTable("document_render_manifests", table =>
            {
                table.HasCheckConstraint(
                    "ck_render_manifests_identity",
                    "length(render_manifest_id) = 79 AND " +
                    "substr(render_manifest_id, 1, 15) = 'rendermanifest-' AND " +
                    "substr(render_manifest_id, 16) = manifest_sha256");
                table.HasCheckConstraint("ck_render_manifests_sha", Sha256("manifest_sha256"));
                table.HasCheckConstraint("ck_render_manifests_schema", "schema_version IN (1, 2)");
                table.HasCheckConstraint("ck_render_manifests_pages", "source_page_count > 0");
                table.HasCheckConstraint(
                    "ck_render_manifests_profile",
                    "render_profile_id IN ('pdf-page-png-v1', 'pdf-page-png-notice-v1')");
                table.HasCheckConstraint(
                    "ck_render_manifests_obligation",
                    "(render_profile_id = 'pdf-page-png-v1' AND schema_version = 1 " +
                    "AND obligation_set_id IS NULL AND obligation_set_sha256 IS NULL) OR " +
                    "(render_profile_id = 'pdf-page-png-notice-v1' AND schema_version = 2 " +
                    "AND obligation_set_id IS NOT NULL AND obligation_set_sha256 IS NOT NULL)");
                table.HasCheckConstraint(
                    "ck_render_manifests_obligation_identity",
                    "(obligation_set_id IS NULL AND obligation_set_sha256 IS NULL) OR " +
                    "(length(obligation_set_id) = 78 " +
                    "AND substr(obligation_set_id, 1, 14) = 'obligationset-' " +
                    "AND substr(obligation_set_id, 15) = obligation_set_sha256 " +
                    $"AND {Sha256("obligation_set_sha256")})");
                table.HasCheckConstraint(
                    "ck_render_manifests_renderer",
                    StableId("renderer_descriptor"));
                table.HasCheckConstraint(
                    "ck_render_manifests_generated_utc",
                    UtcInstant("generated_at_utc"));
                table.HasTrigger("trg_render_manifests_notice_obligation_complete_insert");
                table.HasTrigger("trg_render_manifests_notice_obligation_complete_update");
            });
            entity.HasKey(row => row.RenderManifestId);
            entity.Property(row => row.RenderManifestId).HasMaxLength(79);
            entity.Property(row => row.ManifestSha256).HasMaxLength(64);
            entity.Property(row => row.CorpusId).HasMaxLength(128);
            entity.Property(row => row.DocumentId).HasMaxLength(128);
            entity.Property(row => row.SourceContentSha256).HasMaxLength(64);
            entity.Property(row => row.RenderProfileId).HasMaxLength(128);
            entity.Property(row => row.RendererDescriptor).HasMaxLength(128);
            entity.Property(row => row.ObligationSetId).HasMaxLength(78);
            entity.Property(row => row.ObligationSetSha256).HasMaxLength(64);
            entity.Property(row => row.GeneratedAtUtc).HasMaxLength(33);
            entity.HasAlternateKey(row => new
            {
                row.RenderManifestId,
                row.CorpusId,
                row.DocumentId,
                row.DocumentVersion,
                row.SourceContentSha256,
                row.RenderProfileId,
                row.RendererDescriptor,
            });
            entity.HasIndex(row => row.ManifestSha256).IsUnique();
            entity.HasOne<ContentObjectRow>().WithMany()
                .HasForeignKey(row => row.SourceContentSha256)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<DocumentVersionRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.DocumentId,
                row.DocumentVersion,
                ContentSha256 = row.SourceContentSha256,
            }).HasPrincipalKey(row => new
            {
                row.CorpusId,
                row.DocumentId,
                row.DocumentVersion,
                row.ContentSha256,
            }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<DerivativeObligationSetRow>().WithMany().HasForeignKey(row => new
            {
                row.ObligationSetId,
                CanonicalSha256 = row.ObligationSetSha256,
                row.CorpusId,
                row.DocumentId,
                row.DocumentVersion,
                row.SourceContentSha256,
            }).HasPrincipalKey(row => new
            {
                row.ObligationSetId,
                row.CanonicalSha256,
                row.CorpusId,
                row.DocumentId,
                row.DocumentVersion,
                row.SourceContentSha256,
            }).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DocumentPageImageRow>(entity =>
        {
            entity.ToTable("document_page_images", table =>
            {
                table.HasCheckConstraint("ck_page_images_page", "page_number > 0");
                table.HasCheckConstraint(
                    "ck_page_images_profile",
                    "render_profile_id IN ('pdf-page-png-v1', 'pdf-page-png-notice-v1')");
                table.HasCheckConstraint(
                    "ck_page_images_renderer",
                    StableId("renderer_descriptor"));
                table.HasCheckConstraint("ck_page_images_sha", Sha256("image_sha256"));
                table.HasCheckConstraint(
                    "ck_page_images_content_identity",
                    "image_content_sha256 = image_sha256");
                table.HasCheckConstraint("ck_page_images_length", "byte_length > 0");
                table.HasCheckConstraint("ck_page_images_media_type", "media_type = 'image/png'");
                table.HasCheckConstraint(
                    "ck_page_images_dimensions",
                    "width_pixels BETWEEN 1 AND 4096 AND height_pixels BETWEEN 1 AND 4096");
                table.HasCheckConstraint(
                    "ck_page_images_regions",
                    "(render_profile_id = 'pdf-page-png-v1' " +
                    "AND source_region_width_pixels IS NULL " +
                    "AND source_region_height_pixels IS NULL " +
                    "AND notice_region_height_pixels IS NULL) OR " +
                    "(render_profile_id = 'pdf-page-png-notice-v1' " +
                    "AND source_region_width_pixels IS NOT NULL " +
                    "AND source_region_height_pixels IS NOT NULL " +
                    "AND notice_region_height_pixels IS NOT NULL " +
                    "AND source_region_width_pixels BETWEEN 1 AND 4096 " +
                    "AND source_region_height_pixels BETWEEN 1 AND 4096 " +
                    "AND notice_region_height_pixels BETWEEN 1 AND 4096 " +
                    "AND source_region_width_pixels = width_pixels " +
                    "AND source_region_height_pixels + notice_region_height_pixels = height_pixels)");
            });
            entity.HasKey(row => new { row.RenderManifestId, row.PageNumber });
            entity.Property(row => row.RenderManifestId).HasMaxLength(79);
            entity.Property(row => row.CorpusId).HasMaxLength(128);
            entity.Property(row => row.DocumentId).HasMaxLength(128);
            entity.Property(row => row.SourceContentSha256).HasMaxLength(64);
            entity.Property(row => row.RenderProfileId).HasMaxLength(128);
            entity.Property(row => row.RendererDescriptor).HasMaxLength(128);
            entity.Property(row => row.ImageContentSha256).HasMaxLength(64);
            entity.Property(row => row.ImageSha256).HasMaxLength(64);
            entity.Property(row => row.MediaType).HasMaxLength(9);
            entity.HasIndex(row => new
            {
                row.CorpusId,
                row.DocumentId,
                row.DocumentVersion,
                row.SourceContentSha256,
                row.PageNumber,
                row.RenderProfileId,
                row.RendererDescriptor,
            });
            entity.HasOne<DocumentRenderManifestRow>().WithMany().HasForeignKey(row => new
            {
                row.RenderManifestId,
                row.CorpusId,
                row.DocumentId,
                row.DocumentVersion,
                row.SourceContentSha256,
                row.RenderProfileId,
                row.RendererDescriptor,
            }).HasPrincipalKey(row => new
            {
                row.RenderManifestId,
                row.CorpusId,
                row.DocumentId,
                row.DocumentVersion,
                row.SourceContentSha256,
                row.RenderProfileId,
                row.RendererDescriptor,
            }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ContentObjectRow>().WithMany()
                .HasForeignKey(row => row.ImageContentSha256)
                .OnDelete(DeleteBehavior.Restrict);
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

    private static void ConfigureDerivativeObligations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DerivativeObligationSetRow>(entity =>
        {
            entity.ToTable("derivative_obligation_sets", table =>
            {
                table.HasCheckConstraint(
                    "ck_derivative_obligation_sets_identity",
                    "length(obligation_set_id) = 78 " +
                    "AND substr(obligation_set_id, 1, 14) = 'obligationset-' " +
                    "AND substr(obligation_set_id, 15) = canonical_sha256");
                table.HasCheckConstraint(
                    "ck_derivative_obligation_sets_canonical_sha",
                    Sha256("canonical_sha256"));
                table.HasCheckConstraint("ck_derivative_obligation_sets_schema", "schema_version = 1");
                table.HasCheckConstraint(
                    "ck_derivative_obligation_sets_document_version",
                    "document_version > 0");
                table.HasCheckConstraint(
                    "ck_derivative_obligation_sets_mapping_revision",
                    StableId("rights_mapping_revision"));
                table.HasCheckConstraint(
                    "ck_derivative_obligation_sets_language",
                    Bcp47Shape("content_language"));
                table.HasCheckConstraint(
                    "ck_derivative_obligation_sets_publisher",
                    "length(authoritative_publisher_or_author) BETWEEN 1 AND 512");
                table.HasCheckConstraint(
                    "ck_derivative_obligation_sets_title",
                    "length(document_title) BETWEEN 1 AND 512");
                table.HasCheckConstraint(
                    "ck_derivative_obligation_sets_version_label",
                    "length(document_version_label) BETWEEN 1 AND 128");
                table.HasCheckConstraint(
                    "ck_derivative_obligation_sets_source_reference",
                    "length(source_reference) BETWEEN 1 AND 2048");
                table.HasCheckConstraint(
                    "ck_derivative_obligation_sets_attribution",
                    "length(attribution_text) BETWEEN 1 AND 4096");
                table.HasCheckConstraint(
                    "ck_derivative_obligation_sets_copyright",
                    "length(copyright_notice) BETWEEN 1 AND 8192");
                table.HasCheckConstraint(
                    "ck_derivative_obligation_sets_permission",
                    "length(permission_notice) BETWEEN 1 AND 8192");
                table.HasCheckConstraint(
                    "ck_derivative_obligation_sets_trademark_treatment",
                    "trademark_treatment IN ('Required', 'Prohibited', 'NotApplicable')");
                table.HasCheckConstraint(
                    "ck_derivative_obligation_sets_trademark_text",
                    "length(trademark_or_non_endorsement_text) BETWEEN 1 AND 4096");
                table.HasCheckConstraint(
                    "ck_derivative_obligation_sets_change_marking",
                    "length(change_marking_text) BETWEEN 1 AND 4096");
                table.HasCheckConstraint(
                    "ck_derivative_obligation_sets_placement",
                    "placement_mode = 'VisibleInBinaryAndAccessibleContext'");
                table.HasCheckConstraint(
                    "ck_derivative_obligation_sets_assessed_utc",
                    UtcInstant("assessed_at_utc"));
                table.HasCheckConstraint(
                    "ck_derivative_obligation_sets_assessor",
                    StableId("assessor_id"));
                table.HasTrigger("trg_derivative_obligation_sets_immutable_update");
            });
            entity.HasKey(row => row.ObligationSetId);
            entity.HasAlternateKey(row => new
            {
                row.ObligationSetId,
                row.CanonicalSha256,
                row.CorpusId,
                row.DocumentId,
                row.DocumentVersion,
                row.SourceContentSha256,
            });
            entity.Property(row => row.ObligationSetId).HasMaxLength(78);
            entity.Property(row => row.CanonicalSha256).HasMaxLength(64);
            entity.Property(row => row.CorpusId).HasMaxLength(128);
            entity.Property(row => row.DocumentId).HasMaxLength(128);
            entity.Property(row => row.SourceContentSha256).HasMaxLength(64);
            entity.Property(row => row.RightsMappingRevision).HasMaxLength(128);
            entity.Property(row => row.ContentLanguage).HasMaxLength(128);
            entity.Property(row => row.AuthoritativePublisherOrAuthor).HasMaxLength(512);
            entity.Property(row => row.DocumentTitle).HasMaxLength(512);
            entity.Property(row => row.DocumentVersionLabel).HasMaxLength(128);
            entity.Property(row => row.SourceReference).HasMaxLength(2048);
            entity.Property(row => row.AttributionText).HasMaxLength(4096);
            entity.Property(row => row.CopyrightNotice).HasMaxLength(8192);
            entity.Property(row => row.PermissionNotice).HasMaxLength(8192);
            entity.Property(row => row.TrademarkTreatment).HasMaxLength(13);
            entity.Property(row => row.TrademarkOrNonEndorsementText).HasMaxLength(4096);
            entity.Property(row => row.ChangeMarkingText).HasMaxLength(4096);
            entity.Property(row => row.PlacementMode).HasMaxLength(35);
            entity.Property(row => row.AssessedAtUtc).HasMaxLength(33);
            entity.Property(row => row.AssessorId).HasMaxLength(128);
            entity.HasIndex(row => new
            {
                row.CorpusId,
                row.DocumentId,
                row.DocumentVersion,
                row.SourceContentSha256,
                row.RightsMappingRevision,
            });
            entity.HasOne<ContentObjectRow>().WithMany()
                .HasForeignKey(row => row.SourceContentSha256)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<DocumentVersionRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.DocumentId,
                row.DocumentVersion,
                ContentSha256 = row.SourceContentSha256,
            }).HasPrincipalKey(row => new
            {
                row.CorpusId,
                row.DocumentId,
                row.DocumentVersion,
                row.ContentSha256,
            }).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DerivativeObligationEvidenceReferenceRow>(entity =>
        {
            entity.ToTable("derivative_obligation_evidence_references", table =>
            {
                table.HasCheckConstraint(
                    "ck_derivative_obligation_evidence_ordinal",
                    "ordinal > 0");
                table.HasCheckConstraint(
                    "ck_derivative_obligation_evidence_reference",
                    StableId("evidence_reference"));
                table.HasTrigger("trg_derivative_obligation_evidence_immutable_update");
                table.HasTrigger("trg_derivative_obligation_evidence_sealed_insert");
                table.HasTrigger("trg_derivative_obligation_evidence_sealed_delete");
            });
            entity.HasKey(row => new { row.ObligationSetId, row.Ordinal });
            entity.Property(row => row.ObligationSetId).HasMaxLength(78);
            entity.Property(row => row.EvidenceReference).HasMaxLength(128);
            entity.HasIndex(row => new { row.ObligationSetId, row.EvidenceReference }).IsUnique();
            entity.HasOne<DerivativeObligationSetRow>().WithMany()
                .HasForeignKey(row => row.ObligationSetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DerivativeObligationDisclaimerRow>(entity =>
        {
            entity.ToTable("derivative_obligation_disclaimers", table =>
            {
                table.HasCheckConstraint(
                    "ck_derivative_obligation_disclaimers_ordinal",
                    "ordinal BETWEEN 1 AND 16");
                table.HasCheckConstraint(
                    "ck_derivative_obligation_disclaimers_text",
                    "length(disclaimer_text) BETWEEN 1 AND 8192");
                table.HasTrigger("trg_derivative_obligation_disclaimers_immutable_update");
                table.HasTrigger("trg_derivative_obligation_disclaimers_sealed_insert");
                table.HasTrigger("trg_derivative_obligation_disclaimers_sealed_delete");
            });
            entity.HasKey(row => new { row.ObligationSetId, row.Ordinal });
            entity.Property(row => row.ObligationSetId).HasMaxLength(78);
            entity.Property(row => row.DisclaimerText).HasMaxLength(8192);
            entity.HasOne<DerivativeObligationSetRow>().WithMany()
                .HasForeignKey(row => row.ObligationSetId)
                .OnDelete(DeleteBehavior.Cascade);
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

        modelBuilder.Entity<ActivationEvidenceBindingRow>(entity =>
        {
            entity.ToTable("activation_evidence_bindings", table =>
            {
                table.HasCheckConstraint(
                    "ck_activation_evidence_format",
                    "document_format IN ('Pdf', 'Csv')");
                table.HasCheckConstraint(
                    "ck_activation_evidence_source",
                    Sha256("source_content_sha256"));
                table.HasCheckConstraint(
                    "ck_activation_evidence_rights_schema",
                    "rights_schema_version = 1");
                table.HasCheckConstraint(
                    "ck_activation_evidence_manifest",
                    "document_format = 'Pdf' OR " +
                    "(document_format = 'Csv' AND render_manifest_id IS NULL)");
            });
            entity.HasKey(row => new
            {
                row.CorpusId,
                row.RecordRevision,
                row.DocumentId,
                row.DocumentVersion,
            });
            entity.Property(row => row.SourceContentSha256).HasMaxLength(64);
            entity.Property(row => row.RenderManifestId).HasMaxLength(79);
            entity.HasOne<ActivationBindingRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.RecordRevision,
                row.DocumentId,
                row.DocumentVersion,
            }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<DocumentVersionRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.DocumentId,
                row.DocumentVersion,
                ContentSha256 = row.SourceContentSha256,
            }).HasPrincipalKey(row => new
            {
                row.CorpusId,
                row.DocumentId,
                row.DocumentVersion,
                row.ContentSha256,
            }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<DocumentRenderManifestRow>().WithMany()
                .HasForeignKey(row => row.RenderManifestId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ActivationRightsDecisionRow>(entity =>
        {
            entity.ToTable("activation_rights_decisions", table =>
            {
                table.HasCheckConstraint(
                    "ck_activation_rights_right",
                    "document_right IN ('SourcePossessionOrDownload', " +
                    "'ParsingAndTextualTransformation', 'Indexing', 'SourceByteRetention', " +
                    "'QuotationAndCitation', 'PageRendering', " +
                    "'DerivativeImageCreationAndRetention', " +
                    "'RuntimeDerivativeImageDisplay', " +
                    "'SourceAndDerivativeByteDistributionOrPublication', " +
                    "'AttributionNoticeTrademarkAndChangeMarkingRequirements')");
                table.HasCheckConstraint(
                    "ck_activation_rights_state",
                    "decision_state IN ('Permitted', 'Denied', 'Unproven')");
                table.HasCheckConstraint(
                    "ck_activation_rights_evidence",
                    StableId("evidence_reference"));
            });
            entity.HasKey(row => new
            {
                row.CorpusId,
                row.RecordRevision,
                row.DocumentId,
                row.DocumentVersion,
                row.DocumentRight,
            });
            entity.HasOne<ActivationEvidenceBindingRow>().WithMany().HasForeignKey(row => new
            {
                row.CorpusId,
                row.RecordRevision,
                row.DocumentId,
                row.DocumentVersion,
            }).OnDelete(DeleteBehavior.Restrict);
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

    private static void ConfigureAnswerEvidence(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnswerEvidenceRecordRow>(entity =>
        {
            entity.ToTable("answer_evidence_records", table =>
            {
                table.HasCheckConstraint(
                    "ck_answer_evidence_id",
                    "length(answer_evidence_record_id) = 45 AND " +
                    "answer_evidence_record_id GLOB 'ans-evidence-[0-9a-f]*' AND " +
                    "substr(answer_evidence_record_id, 14) NOT GLOB '*[^0-9a-f]*'");
                table.HasCheckConstraint("ck_answer_evidence_schema", "schema_version = 1");
                table.HasCheckConstraint("ck_answer_evidence_record_digest", Sha256("record_sha256"));
                table.HasCheckConstraint("ck_answer_evidence_source_digest", Sha256("source_binding_set_digest"));
                table.HasCheckConstraint("ck_answer_evidence_activation_digest", Sha256("activation_binding_set_digest"));
                table.HasCheckConstraint("ck_answer_evidence_answer_digest", Sha256("answer_sha256"));
                table.HasCheckConstraint("ck_answer_evidence_coverage_digest", Sha256("evidence_coverage_digest"));
                table.HasCheckConstraint("ck_answer_evidence_activation_revision", "activation_record_revision > 0");
                table.HasCheckConstraint("ck_answer_evidence_catalogue_revision", "catalogue_revision > 0");
                table.HasCheckConstraint("ck_answer_evidence_outcome", "outcome = 'Answered'");
                table.HasCheckConstraint("ck_answer_evidence_question_language", "question_language IN ('pt-BR', 'en-GB')");
                table.HasCheckConstraint("ck_answer_evidence_answer_language", "answer_language = question_language");
                table.HasCheckConstraint("ck_answer_evidence_answer_length", "answer_utf8_byte_length > 0");
                table.HasCheckConstraint(
                    "ck_answer_evidence_retention",
                    "retention_policy_id = 'answer-evidence-p30d-v1'");
                table.HasCheckConstraint("ck_answer_evidence_created_utc", UtcInstant("created_at_utc"));
                table.HasCheckConstraint("ck_answer_evidence_expires_utc", UtcInstant("expires_at_utc"));
                table.HasCheckConstraint(
                    "ck_answer_evidence_p30d",
                    "julianday(expires_at_utc) = julianday(created_at_utc) + 30");
            });
            entity.HasKey(row => row.AnswerEvidenceRecordId);
            entity.HasIndex(row => row.RecordSha256);
            entity.HasIndex(row => new { row.CorpusId, row.ExpiresAtUtc });
            entity.Property(row => row.AnswerEvidenceRecordId).HasMaxLength(45);
            entity.Property(row => row.RecordSha256).HasMaxLength(64);
            entity.Property(row => row.SourceBindingSetDigest).HasMaxLength(64);
            entity.Property(row => row.ActivationBindingSetDigest).HasMaxLength(64);
            entity.Property(row => row.AnswerSha256).HasMaxLength(64);
            entity.Property(row => row.EvidenceCoverageDigest).HasMaxLength(64);
            entity.Property(row => row.IndexGenerationId).HasMaxLength(71);
            entity.Property(row => row.RetrievalPolicyVersion).HasMaxLength(128);
            entity.Property(row => row.PromptVersion).HasMaxLength(128);
            entity.Property(row => row.LanguageModelProviderId).HasMaxLength(128);
            entity.Property(row => row.LanguageModelId).HasMaxLength(128);
            entity.Property(row => row.LanguageModelRevision).HasMaxLength(128);
            entity.Property(row => row.CorrelationId).HasMaxLength(128);
            entity.HasOne<CorpusRow>().WithMany().HasForeignKey(row => row.CorpusId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<AdminOperationRow>().WithMany()
                .HasForeignKey(row => row.AnswerEvidenceRecordId)
                .HasPrincipalKey(row => row.OperationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AnswerEvidenceCitationRow>(entity =>
        {
            entity.ToTable("answer_evidence_citations", table =>
            {
                table.HasCheckConstraint("ck_answer_evidence_citation_ordinal", "ordinal > 0");
                table.HasCheckConstraint("ck_answer_evidence_citation_product_revision", "product_revision > 0");
                table.HasCheckConstraint("ck_answer_evidence_citation_document_version", "document_version > 0");
                table.HasCheckConstraint("ck_answer_evidence_citation_format", "document_format IN ('Pdf', 'Csv')");
                table.HasCheckConstraint(
                    "ck_answer_evidence_citation_language",
                    Bcp47Shape("content_language"));
                table.HasCheckConstraint("ck_answer_evidence_citation_trust", TrustClass("source_trust_class"));
                table.HasCheckConstraint("ck_answer_evidence_citation_source", Sha256("source_content_sha256"));
                table.HasCheckConstraint(
                    "ck_answer_evidence_citation_source_identity",
                    "(source_trust_class = 'LocalAuthorised' AND official_registration_id IS NULL " +
                    "AND source_snapshot_id IS NULL AND source_observation_id IS NULL) OR " +
                    "(source_trust_class = 'OfficialExternal' AND official_registration_id IS NOT NULL " +
                    "AND source_snapshot_id IS NOT NULL AND source_observation_id IS NOT NULL)");
                table.HasCheckConstraint(
                    "ck_answer_evidence_citation_location",
                    "(document_format = 'Pdf' AND page_start > 0 AND page_end >= page_start " +
                    "AND record_start IS NULL AND record_end IS NULL AND columns_json = '[]') OR " +
                    "(document_format = 'Csv' AND page_start IS NULL AND page_end IS NULL " +
                    "AND ((record_start IS NULL AND record_end IS NULL) OR " +
                    "(record_start > 0 AND record_end >= record_start)) AND render_manifest_id IS NULL)");
                table.HasCheckConstraint(
                    "ck_answer_evidence_citation_columns",
                    "json_valid(columns_json) AND json_type(columns_json) = 'array' " +
                    "AND length(columns_json) BETWEEN 2 AND 8192");
                table.HasCheckConstraint(
                    "ck_answer_evidence_citation_section",
                    "section_locator IS NULL OR length(section_locator) BETWEEN 1 AND 512");
            });
            entity.HasKey(row => new { row.AnswerEvidenceRecordId, row.Ordinal });
            entity.Property(row => row.AnswerEvidenceRecordId).HasMaxLength(45);
            entity.Property(row => row.SourceContentSha256).HasMaxLength(64);
            entity.Property(row => row.ColumnsJson).HasMaxLength(8192);
            entity.Property(row => row.SectionLocator).HasMaxLength(512);
            entity.Property(row => row.RenderManifestId).HasMaxLength(79);
            entity.HasOne<AnswerEvidenceRecordRow>().WithMany()
                .HasForeignKey(row => row.AnswerEvidenceRecordId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ContentObjectRow>().WithMany()
                .HasForeignKey(row => row.SourceContentSha256)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AnswerEvidencePageRow>(entity =>
        {
            entity.ToTable("answer_evidence_pages", table =>
            {
                table.HasCheckConstraint("ck_answer_evidence_page_document_version", "document_version > 0");
                table.HasCheckConstraint("ck_answer_evidence_page_number", "page_number > 0");
                table.HasCheckConstraint("ck_answer_evidence_page_source", Sha256("source_content_sha256"));
                table.HasCheckConstraint("ck_answer_evidence_page_image", Sha256("image_content_sha256"));
                table.HasCheckConstraint("ck_answer_evidence_page_image_digest", Sha256("image_sha256"));
                table.HasCheckConstraint("ck_answer_evidence_page_identity", "image_content_sha256 = image_sha256");
                table.HasCheckConstraint("ck_answer_evidence_page_length", "byte_length > 0");
                table.HasCheckConstraint("ck_answer_evidence_page_media", "media_type = 'image/png'");
                table.HasCheckConstraint("ck_answer_evidence_page_width", "width_pixels BETWEEN 1 AND 4096");
                table.HasCheckConstraint("ck_answer_evidence_page_height", "height_pixels BETWEEN 1 AND 4096");
            });
            entity.HasKey(row => new
            {
                row.AnswerEvidenceRecordId,
                row.DocumentId,
                row.DocumentVersion,
                row.PageNumber,
            });
            entity.Property(row => row.AnswerEvidenceRecordId).HasMaxLength(45);
            entity.Property(row => row.SourceContentSha256).HasMaxLength(64);
            entity.Property(row => row.ImageContentSha256).HasMaxLength(64);
            entity.Property(row => row.ImageSha256).HasMaxLength(64);
            entity.Property(row => row.RenderManifestId).HasMaxLength(79);
            entity.HasOne<AnswerEvidenceRecordRow>().WithMany()
                .HasForeignKey(row => row.AnswerEvidenceRecordId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ContentObjectRow>().WithMany()
                .HasForeignKey(row => row.SourceContentSha256)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ContentObjectRow>().WithMany()
                .HasForeignKey(row => row.ImageContentSha256)
                .OnDelete(DeleteBehavior.Restrict);
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

    private static string Bcp47Shape(string column) =>
        $"length({column}) BETWEEN 1 AND 128 AND " +
        $"{column} NOT GLOB '*[^A-Za-z0-9-]*' AND " +
        $"substr({column}, 1, 1) <> '-' AND substr({column}, -1) <> '-' AND " +
        $"instr({column}, '--') = 0";

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
