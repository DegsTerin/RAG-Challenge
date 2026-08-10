// Purpose: Verifies the notice-bearing Control schema, immutable obligation rows, conditional render bindings, and lossless legacy migration in task-owned SQLite stores.
using System.Globalization;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Infrastructure.Persistence;

namespace RagChallenge.IntegrationTests;

public sealed class SqliteNoticeBearingSchemaMigrationTests
{
    private const string PreviousMigration =
        "20260808033247_AddAnswerEvidenceRecords";
    private const string NoticeBearingMigration =
        "20260810034537_SealNoticeBearingObligationBindings";
    private const string SourceText = "synthetic notice-bearing source";

    [Fact]
    public async Task MigrationPreservesLegacyRenderAndActivationRowsWithoutBackfill()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (_, binding) = await fixture.CommitLocalCatalogueAsync(SourceText);
        var generation = await fixture.CommitGenerationAsync(binding, "notice-schema-legacy");
        var evidence = await fixture.CreateActivationEvidenceAsync(binding);
        _ = await new GenerationActivationService(fixture.ControlStore).ActivateAsync(
            new GenerationActivationRequest(
                generation,
                [evidence],
                ExpectedCurrentRevision: 0,
                SqliteControlPlaneStore.MinimumPreviousGenerationRetention,
                new RagChallenge.Application.Administration.AdministrativeAuditContext(
                    new OperationId("activate-notice-schema-legacy"),
                    "integration-test",
                    "activate-generation",
                    "synthetic notice schema migration fixture",
                    SqlitePersistenceFixture.At(3))));

        var renderManifestBefore = await ReadTextAsync(
            fixture.Options.ControlDatabasePath,
            LegacyRenderManifestProjection);
        var pageImageBefore = await ReadTextAsync(
            fixture.Options.ControlDatabasePath,
            LegacyPageImageProjection);
        var activationBefore = await ReadTextAsync(
            fixture.Options.ControlDatabasePath,
            LegacyActivationProjection);

        await MigrateAsync(fixture.Options, PreviousMigration);
        Assert.Equal(renderManifestBefore, await ReadTextAsync(
            fixture.Options.ControlDatabasePath,
            LegacyRenderManifestProjection));
        Assert.Equal(pageImageBefore, await ReadTextAsync(
            fixture.Options.ControlDatabasePath,
            LegacyPageImageProjection));
        Assert.Equal(activationBefore, await ReadTextAsync(
            fixture.Options.ControlDatabasePath,
            LegacyActivationProjection));

        await MigrateAsync(fixture.Options, NoticeBearingMigration);

        Assert.Equal(renderManifestBefore, await ReadTextAsync(
            fixture.Options.ControlDatabasePath,
            LegacyRenderManifestProjection));
        Assert.Equal(pageImageBefore, await ReadTextAsync(
            fixture.Options.ControlDatabasePath,
            LegacyPageImageProjection));
        Assert.Equal(activationBefore, await ReadTextAsync(
            fixture.Options.ControlDatabasePath,
            LegacyActivationProjection));
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM document_render_manifests " +
            "WHERE render_profile_id = 'pdf-page-png-v1' AND schema_version = 1 " +
            "AND obligation_set_id IS NULL AND obligation_set_sha256 IS NULL;"));
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM document_page_images " +
            "WHERE render_profile_id = 'pdf-page-png-v1' " +
            "AND source_region_width_pixels IS NULL " +
            "AND source_region_height_pixels IS NULL " +
            "AND notice_region_height_pixels IS NULL;"));
        Assert.Equal(0, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM derivative_obligation_sets;"));
        Assert.Equal(0, await CountRowsAsync(
            fixture.Options.ControlDatabasePath,
            "PRAGMA foreign_key_check;"));
        Assert.Equal(6, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM pragma_foreign_key_list('document_render_manifests') " +
            "WHERE \"table\" = 'derivative_obligation_sets';"));
    }

    [Fact]
    public async Task CompleteNoticeBearingRowsPersistExactOrderAndRejectMutation()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        _ = await fixture.CommitLocalCatalogueAsync(SourceText);
        var identities = await InsertCompleteNoticeBearingBindingAsync(fixture);

        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM derivative_obligation_sets;"));
        Assert.Equal("rights-evidence-a|rights-evidence-b", await ReadTextAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT group_concat(evidence_reference, '|') FROM " +
            "(SELECT evidence_reference FROM derivative_obligation_evidence_references " +
            "ORDER BY ordinal);"));
        Assert.Equal("Synthetic first disclaimer.|Synthetic second disclaimer.",
            await ReadTextAsync(
                fixture.Options.ControlDatabasePath,
                "SELECT group_concat(disclaimer_text, '|') FROM " +
                "(SELECT disclaimer_text FROM derivative_obligation_disclaimers " +
                "ORDER BY ordinal);"));
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM document_render_manifests " +
            $"WHERE obligation_set_id = '{identities.ObligationSetId}' " +
            $"AND obligation_set_sha256 = '{identities.ObligationSha256}' " +
            "AND render_profile_id = 'pdf-page-png-notice-v1' AND schema_version = 2;"));
        Assert.Equal(1, await ScalarAsync(
            fixture.Options.ControlDatabasePath,
            "SELECT COUNT(*) FROM document_page_images " +
            "WHERE source_region_width_pixels = width_pixels " +
            "AND source_region_height_pixels + notice_region_height_pixels = height_pixels;"));
        Assert.Equal(0, await CountRowsAsync(
            fixture.Options.ControlDatabasePath,
            "PRAGMA foreign_key_check;"));

        await Assert.ThrowsAsync<SqliteException>(() => ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            "UPDATE derivative_obligation_sets " +
            "SET attribution_text = 'mutated' " +
            $"WHERE obligation_set_id = '{identities.ObligationSetId}';"));
        await Assert.ThrowsAsync<SqliteException>(() => ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            "UPDATE derivative_obligation_evidence_references " +
            "SET evidence_reference = 'mutated-reference' WHERE ordinal = 1;"));
        await Assert.ThrowsAsync<SqliteException>(() => ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            "UPDATE derivative_obligation_disclaimers " +
            "SET disclaimer_text = 'mutated disclaimer' WHERE ordinal = 1;"));
        await Assert.ThrowsAsync<SqliteException>(() => ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            "INSERT INTO derivative_obligation_evidence_references " +
            "(obligation_set_id, ordinal, evidence_reference) VALUES " +
            $"('{identities.ObligationSetId}', 3, 'rights-evidence-c');"));
        await Assert.ThrowsAsync<SqliteException>(() => ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            "DELETE FROM derivative_obligation_evidence_references WHERE ordinal = 1;"));
        await Assert.ThrowsAsync<SqliteException>(() => ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            "INSERT INTO derivative_obligation_disclaimers " +
            "(obligation_set_id, ordinal, disclaimer_text) VALUES " +
            $"('{identities.ObligationSetId}', 3, 'Synthetic third disclaimer.');"));
        await Assert.ThrowsAsync<SqliteException>(() => ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            "DELETE FROM derivative_obligation_disclaimers WHERE ordinal = 1;"));
        await Assert.ThrowsAsync<SqliteException>(() => ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            "DELETE FROM derivative_obligation_sets " +
            $"WHERE obligation_set_id = '{identities.ObligationSetId}';"));
    }

    [Fact]
    public async Task ConditionalConstraintsRejectIncompleteOrMixedProfileBindings()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        _ = await fixture.CommitLocalCatalogueAsync(SourceText);
        var sourceSha256 = SqlitePersistenceFixture.Hash(SourceText);
        var obligationSha256 = SqlitePersistenceFixture.Hash("conditional obligation set");
        var obligationSetId = $"obligationset-{obligationSha256}";
        await InsertObligationSetAsync(
            fixture.Options.ControlDatabasePath,
            obligationSetId,
            obligationSha256,
            sourceSha256);

        await Assert.ThrowsAsync<SqliteException>(() => ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            NoticeManifestInsert(
                SqlitePersistenceFixture.Hash("manifest without evidence"),
                sourceSha256,
                obligationSetId,
                obligationSha256,
                profile: "pdf-page-png-notice-v1",
                schemaVersion: 2)));
        await ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            "INSERT INTO derivative_obligation_evidence_references " +
            "(obligation_set_id, ordinal, evidence_reference) VALUES " +
            $"('{obligationSetId}', 1, 'rights-evidence-conditional');");

        await Assert.ThrowsAsync<SqliteException>(() => ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            NoticeManifestInsert(
                SqlitePersistenceFixture.Hash("missing obligation manifest"),
                sourceSha256,
                obligationSetId: null,
                obligationSha256: null,
                profile: "pdf-page-png-notice-v1",
                schemaVersion: 2)));
        await Assert.ThrowsAsync<SqliteException>(() => ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            NoticeManifestInsert(
                SqlitePersistenceFixture.Hash("mixed legacy manifest"),
                sourceSha256,
                obligationSetId,
                obligationSha256,
                profile: "pdf-page-png-v1",
                schemaVersion: 1)));
        await Assert.ThrowsAsync<SqliteException>(() => ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            NoticeManifestInsert(
                SqlitePersistenceFixture.Hash("mismatched obligation manifest"),
                sourceSha256,
                obligationSetId,
                SqlitePersistenceFixture.Hash("different obligation set"),
                profile: "pdf-page-png-notice-v1",
                schemaVersion: 2)));
        var manifestSha256 = SqlitePersistenceFixture.Hash("valid conditional manifest");
        await ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            NoticeManifestInsert(
                manifestSha256,
                sourceSha256,
                obligationSetId,
                obligationSha256,
                profile: "pdf-page-png-notice-v1",
                schemaVersion: 2));
        var imageSha256 = SqlitePersistenceFixture.Hash("conditional image");
        await InsertContentObjectAsync(fixture.Options.ControlDatabasePath, imageSha256);

        await Assert.ThrowsAsync<SqliteException>(() => ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            PageImageInsert(
                manifestSha256,
                sourceSha256,
                imageSha256,
                "pdf-page-png-notice-v1",
                sourceWidth: "NULL",
                sourceHeight: "NULL",
                noticeHeight: "NULL")));
        await Assert.ThrowsAsync<SqliteException>(() => ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            PageImageInsert(
                manifestSha256,
                sourceSha256,
                imageSha256,
                "pdf-page-png-notice-v1",
                sourceWidth: "100",
                sourceHeight: "100",
                noticeHeight: "39")));
        await Assert.ThrowsAsync<SqliteException>(() => ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            PageImageInsert(
                manifestSha256,
                sourceSha256,
                imageSha256,
                "pdf-page-png-v1",
                sourceWidth: "100",
                sourceHeight: "100",
                noticeHeight: "40")));
    }

    private static async Task<(string ObligationSetId, string ObligationSha256)>
        InsertCompleteNoticeBearingBindingAsync(SqlitePersistenceFixture fixture)
    {
        var sourceSha256 = SqlitePersistenceFixture.Hash(SourceText);
        var obligationSha256 = SqlitePersistenceFixture.Hash("complete obligation set");
        var obligationSetId = $"obligationset-{obligationSha256}";
        var manifestSha256 = SqlitePersistenceFixture.Hash("complete notice manifest");
        var imageSha256 = SqlitePersistenceFixture.Hash("complete notice image");
        await InsertContentObjectAsync(fixture.Options.ControlDatabasePath, imageSha256);
        await InsertObligationSetAsync(
            fixture.Options.ControlDatabasePath,
            obligationSetId,
            obligationSha256,
            sourceSha256);
        await ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            $"""
            INSERT INTO derivative_obligation_evidence_references
                (obligation_set_id, ordinal, evidence_reference)
            VALUES
                ('{obligationSetId}', 1, 'rights-evidence-a'),
                ('{obligationSetId}', 2, 'rights-evidence-b');

            INSERT INTO derivative_obligation_disclaimers
                (obligation_set_id, ordinal, disclaimer_text)
            VALUES
                ('{obligationSetId}', 1, 'Synthetic first disclaimer.'),
                ('{obligationSetId}', 2, 'Synthetic second disclaimer.');
            """);
        await ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            NoticeManifestInsert(
                manifestSha256,
                sourceSha256,
                obligationSetId,
                obligationSha256,
                profile: "pdf-page-png-notice-v1",
                schemaVersion: 2));
        await ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            PageImageInsert(
                manifestSha256,
                sourceSha256,
                imageSha256,
                "pdf-page-png-notice-v1",
                sourceWidth: "100",
                sourceHeight: "100",
                noticeHeight: "40"));
        return (obligationSetId, obligationSha256);
    }

    private static Task InsertObligationSetAsync(
        string path,
        string obligationSetId,
        string obligationSha256,
        string sourceSha256) =>
        ExecuteAsync(
            path,
            $"""
            INSERT INTO derivative_obligation_sets
                (obligation_set_id, schema_version, canonical_sha256, corpus_id,
                 document_id, document_version, source_content_sha256,
                 rights_mapping_revision, content_language,
                 authoritative_publisher_or_author, document_title,
                 document_version_label, source_reference, attribution_text,
                 copyright_notice, permission_notice, trademark_treatment,
                 trademark_or_non_endorsement_text, change_marking_text,
                 placement_mode, assessed_at_utc, assessor_id)
            VALUES
                ('{obligationSetId}', 1, '{obligationSha256}',
                 '{SqlitePersistenceFixture.CorpusId.Value}', 'doc-fixture', 1,
                 '{sourceSha256}', 'rights-mapping-v1', 'en-GB',
                 'Synthetic Documentation Group', 'Synthetic Documentation',
                 '1.0', 'synthetic-source-v1', 'Synthetic attribution.',
                 'Synthetic copyright notice.', 'Synthetic permission notice.',
                 'NotApplicable', 'No endorsement is claimed.',
                 'Rendered derivative of version 1.0.',
                 'VisibleInBinaryAndAccessibleContext',
                 '2026-01-02T12:00:00.0000000+00:00', 'integration-test');
            """);

    private static string NoticeManifestInsert(
        string manifestSha256,
        string sourceSha256,
        string? obligationSetId,
        string? obligationSha256,
        string profile,
        int schemaVersion)
    {
        var obligationSetSql = obligationSetId is null ? "NULL" : $"'{obligationSetId}'";
        var obligationShaSql = obligationSha256 is null ? "NULL" : $"'{obligationSha256}'";
        return $"""
            INSERT INTO document_render_manifests
                (render_manifest_id, manifest_sha256, schema_version, corpus_id,
                 document_id, document_version, source_content_sha256,
                 source_page_count, render_profile_id, renderer_descriptor,
                 obligation_set_id, obligation_set_sha256, generated_at_utc)
            VALUES
                ('rendermanifest-{manifestSha256}', '{manifestSha256}', {schemaVersion},
                 '{SqlitePersistenceFixture.CorpusId.Value}', 'doc-fixture', 1,
                 '{sourceSha256}', 1, '{profile}', 'synthetic-renderer-v1',
                 {obligationSetSql}, {obligationShaSql},
                 '2026-01-02T12:00:00.0000000+00:00');
            """;
    }

    private static string PageImageInsert(
        string manifestSha256,
        string sourceSha256,
        string imageSha256,
        string profile,
        string sourceWidth,
        string sourceHeight,
        string noticeHeight) =>
        $"""
        INSERT INTO document_page_images
            (render_manifest_id, page_number, corpus_id, document_id,
             document_version, source_content_sha256, render_profile_id,
             renderer_descriptor, image_content_sha256, image_sha256,
             byte_length, media_type, width_pixels, height_pixels,
             source_region_width_pixels, source_region_height_pixels,
             notice_region_height_pixels)
        VALUES
            ('rendermanifest-{manifestSha256}', 1,
             '{SqlitePersistenceFixture.CorpusId.Value}', 'doc-fixture', 1,
             '{sourceSha256}', '{profile}', 'synthetic-renderer-v1',
             '{imageSha256}', '{imageSha256}', 128, 'image/png', 100, 140,
             {sourceWidth}, {sourceHeight}, {noticeHeight});
        """;

    private static Task InsertContentObjectAsync(string path, string contentSha256) =>
        ExecuteAsync(
            path,
            $"""
            INSERT INTO content_objects(content_sha256, byte_length, registered_at_utc)
            VALUES ('{contentSha256}', 128, '2026-01-02T12:00:00.0000000+00:00');
            """);

    private static async Task MigrateAsync(SqliteStoreOptions options, string targetMigration)
    {
        await using var context = options.CreateControlContext();
        await context.Database.GetService<IMigrator>().MigrateAsync(targetMigration);
    }

    private static async Task ExecuteAsync(string path, string sql)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={path};Mode=ReadWrite;Cache=Private;Foreign Keys=True");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        _ = await command.ExecuteNonQueryAsync();
    }

    private static async Task<string?> ReadTextAsync(string path, string sql)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={path};Mode=ReadOnly;Cache=Private;Foreign Keys=True");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToString(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }

    private static async Task<long> ScalarAsync(string path, string sql)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={path};Mode=ReadOnly;Cache=Private;Foreign Keys=True");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToInt64(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }

    private static async Task<long> CountRowsAsync(string path, string sql)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={path};Mode=ReadOnly;Cache=Private;Foreign Keys=True");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await using var reader = await command.ExecuteReaderAsync();
        var count = 0L;

        while (await reader.ReadAsync())
        {
            count++;
        }

        return count;
    }

    private const string LegacyRenderManifestProjection =
        """
        SELECT hex(CAST(
            render_manifest_id || char(0) || manifest_sha256 || char(0) ||
            schema_version || char(0) || corpus_id || char(0) || document_id ||
            char(0) || document_version || char(0) || source_content_sha256 ||
            char(0) || source_page_count || char(0) || render_profile_id ||
            char(0) || renderer_descriptor || char(0) || generated_at_utc AS BLOB))
        FROM document_render_manifests;
        """;

    private const string LegacyPageImageProjection =
        """
        SELECT hex(CAST(
            render_manifest_id || char(0) || page_number || char(0) || corpus_id ||
            char(0) || document_id || char(0) || document_version || char(0) ||
            source_content_sha256 || char(0) || render_profile_id || char(0) ||
            renderer_descriptor || char(0) || image_content_sha256 || char(0) ||
            image_sha256 || char(0) || byte_length || char(0) || media_type ||
            char(0) || width_pixels || char(0) || height_pixels AS BLOB))
        FROM document_page_images;
        """;

    private const string LegacyActivationProjection =
        """
        SELECT hex(CAST(
            corpus_id || char(0) || record_revision || char(0) ||
            ifnull(previous_record_revision, 'NULL') || char(0) ||
            index_generation_id || char(0) || catalogue_revision || char(0) ||
            activation_binding_set_digest || char(0) || mutation_kind || char(0) ||
            generation_activated_at_utc || char(0) || record_updated_at_utc ||
            char(0) || operation_id AS BLOB))
        FROM activation_records;
        """;
}
