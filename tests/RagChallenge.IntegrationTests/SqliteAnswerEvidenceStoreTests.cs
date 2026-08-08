// Purpose: Verifies atomic AnswerEvidenceRecordV1 persistence, replay/conflict, restart, concurrency, injected rollback boundaries, authority validation, and persistent privacy in disposable SQLite stores.
using System.Security.Cryptography;
using System.Text;

using RagChallenge.Application.Administration;
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;
using RagChallenge.Infrastructure.Persistence;

namespace RagChallenge.IntegrationTests;

public sealed class SqliteAnswerEvidenceStoreTests
{
    [Fact]
    public async Task CompleteRecordReplaysExactlySurvivesRestartAndDoesNotDedupeOtherIds()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var record = await CreateRecordAsync(fixture);
        var store = new SqliteAnswerEvidenceStore(fixture.Options);

        var applied = await store.PersistAsync(record);
        var replay = await new SqliteAnswerEvidenceStore(fixture.Options).PersistAsync(record);
        var restarted = await new SqliteAnswerEvidenceStore(fixture.Options)
            .ReadAsync(record.AnswerEvidenceRecordId);
        var secondIdentity = CloneRecord(
            record,
            new AnswerEvidenceRecordId(
                "ans-evidence-00000000000000000000000000000002"));
        var second = await store.PersistAsync(secondIdentity);

        Assert.Equal(AnswerEvidencePersistenceOutcome.Applied, applied.Outcome);
        Assert.Equal(AnswerEvidencePersistenceOutcome.AlreadyApplied, replay.Outcome);
        Assert.Equal(record.RecordSha256, restarted!.RecordSha256);
        Assert.Equal(record.CreatedAt, restarted.CreatedAt);
        Assert.Equal(record.ExpiresAt, restarted.ExpiresAt);
        Assert.Equal(AnswerEvidencePersistenceOutcome.Applied, second.Outcome);
        Assert.Equal(2, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM answer_evidence_records;"));
        Assert.Equal(2, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM answer_evidence_citations;"));
        Assert.Equal(2, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM answer_evidence_pages;"));
        Assert.Equal(2, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM audit_events WHERE event_type = 'AnswerEvidenceCreated';"));
    }

    [Fact]
    public async Task DivergentSameIdConflictsWithoutMutation()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var record = await CreateRecordAsync(fixture);
        var divergent = CloneRecord(
            record,
            record.AnswerEvidenceRecordId,
            correlationId: "correlation-divergent");
        var store = new SqliteAnswerEvidenceStore(fixture.Options);

        Assert.Equal(
            AnswerEvidencePersistenceOutcome.Applied,
            (await store.PersistAsync(record)).Outcome);
        var conflict = await store.PersistAsync(divergent);

        Assert.Equal(AnswerEvidencePersistenceOutcome.Conflict, conflict.Outcome);
        Assert.Null(conflict.PersistedRecord);
        Assert.Equal(1, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM answer_evidence_records;"));
        Assert.Equal(record.RecordSha256, (await store.ReadAsync(
            record.AnswerEvidenceRecordId))!.RecordSha256);
    }

    [Fact]
    public async Task ConcurrentSameIdHasOneAppliedAndOneExactReplay()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var record = await CreateRecordAsync(fixture);
        var firstStore = new SqliteAnswerEvidenceStore(fixture.Options);
        var secondStore = new SqliteAnswerEvidenceStore(fixture.Options);

        var results = await Task.WhenAll(
            Task.Run(() => firstStore.PersistAsync(record)),
            Task.Run(() => secondStore.PersistAsync(record)));

        Assert.Equal(
            [AnswerEvidencePersistenceOutcome.Applied,
             AnswerEvidencePersistenceOutcome.AlreadyApplied],
            results.Select(result => result.Outcome).Order().ToArray());
        Assert.Equal(1, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM answer_evidence_records;"));
    }

    [Theory]
    [InlineData((int)AnswerEvidenceStoreFaultPoint.BeforeSave)]
    [InlineData((int)AnswerEvidenceStoreFaultPoint.AfterSave)]
    [InlineData((int)AnswerEvidenceStoreFaultPoint.BeforeReadback)]
    [InlineData((int)AnswerEvidenceStoreFaultPoint.AfterReadback)]
    [InlineData((int)AnswerEvidenceStoreFaultPoint.BeforeCommit)]
    public async Task InjectedFailureRollsBackEveryTransactionAndReadbackBoundary(
        int faultValue)
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var record = await CreateRecordAsync(fixture);
        var store = new SqliteAnswerEvidenceStore(
            fixture.Options,
            new ThrowingFaultInjector((AnswerEvidenceStoreFaultPoint)faultValue));

        await Assert.ThrowsAsync<InjectedAnswerEvidenceFailure>(() =>
            store.PersistAsync(record));

        await AssertNoAnswerEvidenceMutationAsync(fixture);
    }

    [Theory]
    [InlineData(AuthorityMismatch.Citation)]
    [InlineData(AuthorityMismatch.Source)]
    [InlineData(AuthorityMismatch.Activation)]
    [InlineData(AuthorityMismatch.Manifest)]
    [InlineData(AuthorityMismatch.Page)]
    public async Task PersistedAuthorityMismatchFailsClosedBeforeAnyMutation(
        AuthorityMismatch mismatch)
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var validRecord = await CreateRecordAsync(fixture);
        var record = CreateAuthorityMismatch(validRecord, mismatch);
        var store = new SqliteAnswerEvidenceStore(fixture.Options);

        await Assert.ThrowsAsync<InvalidDataException>(() => store.PersistAsync(record));

        await AssertNoAnswerEvidenceMutationAsync(fixture);
    }

    [Fact]
    public async Task PersistentRowsAndAuditExcludeQuestionAnswerAndSourceDisplayText()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var record = await CreateRecordAsync(fixture);

        _ = await new SqliteAnswerEvidenceStore(fixture.Options).PersistAsync(record);
        await using var database = new FileStream(
            fixture.Options.ControlDatabasePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);
        using var copy = new MemoryStream();
        await database.CopyToAsync(copy);
        var databaseBytes = copy.ToArray();
        var searchable = Encoding.UTF8.GetString(databaseBytes);

        Assert.DoesNotContain("What is the private question?", searchable, StringComparison.Ordinal);
        Assert.DoesNotContain("Private answer body", searchable, StringComparison.Ordinal);
        Assert.DoesNotContain("Private source excerpt", searchable, StringComparison.Ordinal);
        Assert.DoesNotContain("https://private.invalid", searchable, StringComparison.Ordinal);
        Assert.Equal(1, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM audit_events WHERE event_type = 'AnswerEvidenceCreated' " +
            "AND length(details_digest) = 64;"));
    }

    private static async Task<AnswerEvidenceRecordV1> CreateRecordAsync(
        SqlitePersistenceFixture fixture,
        AnswerEvidenceRecordId? recordId = null,
        string correlationId = "correlation-answer-evidence",
        ActivationBindingSetDigest? activationDigest = null)
    {
        var (_, binding) = await fixture.CommitLocalCatalogueAsync(
            "Private source excerpt and https://private.invalid");
        var generation = await fixture.CommitGenerationAsync(binding, "answer-evidence");
        var evidence = await fixture.CreateActivationEvidenceAsync(binding);
        var activation = await new GenerationActivationService(fixture.ControlStore)
            .ActivateAsync(new GenerationActivationRequest(
                generation,
                [evidence],
                ExpectedCurrentRevision: 0,
                SqliteControlPlaneStore.MinimumPreviousGenerationRetention,
                Audit("activate-answer-evidence", SqlitePersistenceFixture.At(3))));
        var active = activation.CurrentRecord ?? throw new InvalidOperationException(
            "The synthetic activation was not persisted.");
        var manifest = await fixture.ControlStore.ReadAsync(
            SqlitePersistenceFixture.CorpusId,
            evidence.RenderManifestId!) ?? throw new InvalidOperationException(
                "The synthetic render manifest was not persisted.");
        var page = Assert.Single(manifest.OrderedPageImages);
        var citation = new AnswerEvidenceCitationBindingV1(
            1,
            binding.DatabaseProductId,
            binding.DatabaseProductRevision,
            binding.DocumentId,
            binding.DocumentVersion,
            binding.DocumentFormat,
            DocumentContentLanguage.EnGb,
            $"chunk-{Hash("answer-citation")}",
            binding.SourceAdapterId,
            binding.SourceTrustClass,
            binding.OfficialSourceRegistrationId,
            binding.OfficialSnapshotId,
            binding.SourceObservationId,
            evidence.SourceContentObjectId,
            pageStart: 1,
            pageEnd: 1,
            recordStart: null,
            recordEnd: null,
            columns: null,
            sectionLocator: null,
            evidence.RenderManifestId);
        var pageBinding = new AnswerEvidencePageBindingV1(
            page.DocumentId,
            page.DocumentVersion,
            page.SourceContentObjectId,
            page.PageNumber,
            manifest.RenderManifestId,
            page.RenderProfileId,
            page.RendererDescriptor,
            page.ImageContentObjectId,
            page.ImageSha256,
            page.ByteLength,
            page.MediaType,
            page.WidthPixels,
            page.HeightPixels);
        var answerBytes = Encoding.UTF8.GetBytes("Private answer body");
        return AnswerEvidenceRecordV1.Create(
            recordId ?? new AnswerEvidenceRecordId(
                "ans-evidence-00000000000000000000000000000001"),
            active.CorpusId,
            active.RecordRevision,
            active.CatalogueRevision,
            generation.SourceBindingSetDigest,
            activationDigest ?? active.ActivationBindingSetDigest,
            active.IndexGenerationId,
            SupportedQueryLanguage.EnGb,
            new AnswerSha256(Hash(answerBytes)),
            answerBytes.Length,
            new EvidenceCoverageDigest(Hash("answer-coverage")),
            QuestionAnsweringService.RetrievalPolicyVersion,
            QuestionAnsweringService.PromptVersion,
            new AnswerLanguageModelDescriptorV1(
                "synthetic",
                "grounded-v1",
                "fixture-1"),
            correlationId,
            SqlitePersistenceFixture.At(4),
            [citation],
            [pageBinding]);
    }

    private static AnswerEvidenceRecordV1 CreateAuthorityMismatch(
        AnswerEvidenceRecordV1 record,
        AuthorityMismatch mismatch)
    {
        var originalCitation = Assert.Single(record.Citations);
        var originalPage = Assert.Single(record.PageImages);
        var divergentSource = new ContentObjectId(Hash("divergent-source"));
        var divergentManifest = new RenderManifestId(
            $"rendermanifest-{Hash("divergent-manifest")}");
        var divergentImage = Hash("divergent-page-image");
        var sourceContentObjectId = mismatch == AuthorityMismatch.Source
            ? divergentSource
            : originalCitation.SourceContentObjectId;
        var renderManifestId = mismatch == AuthorityMismatch.Manifest
            ? divergentManifest
            : originalCitation.RenderManifestId;
        var citation = new AnswerEvidenceCitationBindingV1(
            originalCitation.Ordinal,
            mismatch == AuthorityMismatch.Citation
                ? new DatabaseProductId("divergent-product")
                : originalCitation.DatabaseProductId,
            originalCitation.DatabaseProductRevision,
            originalCitation.DocumentId,
            originalCitation.DocumentVersion,
            originalCitation.DocumentFormat,
            originalCitation.ContentLanguage,
            originalCitation.ChunkId,
            originalCitation.SourceAdapterId,
            originalCitation.SourceTrustClass,
            originalCitation.OfficialSourceRegistrationId,
            originalCitation.SourceSnapshotId,
            originalCitation.SourceObservationId,
            sourceContentObjectId,
            originalCitation.PageStart,
            originalCitation.PageEnd,
            originalCitation.RecordStart,
            originalCitation.RecordEnd,
            originalCitation.Columns,
            originalCitation.SectionLocator,
            renderManifestId);
        var page = new AnswerEvidencePageBindingV1(
            originalPage.DocumentId,
            originalPage.DocumentVersion,
            sourceContentObjectId,
            originalPage.PageNumber,
            renderManifestId!,
            originalPage.RenderProfileId,
            originalPage.RendererDescriptor,
            mismatch == AuthorityMismatch.Page
                ? new ContentObjectId(divergentImage)
                : originalPage.ImageContentObjectId,
            mismatch == AuthorityMismatch.Page
                ? new ImageSha256(divergentImage)
                : originalPage.ImageSha256,
            originalPage.ByteLength,
            originalPage.MediaType,
            originalPage.WidthPixels,
            originalPage.HeightPixels);

        return AnswerEvidenceRecordV1.Create(
            record.AnswerEvidenceRecordId,
            record.CorpusId,
            record.ActivationRecordRevision,
            record.CatalogueRevision,
            record.SourceBindingSetDigest,
            mismatch == AuthorityMismatch.Activation
                ? new ActivationBindingSetDigest(Hash("divergent-activation"))
                : record.ActivationBindingSetDigest,
            record.IndexGenerationId,
            record.QuestionLanguage,
            record.AnswerSha256,
            record.AnswerUtf8ByteLength,
            record.EvidenceCoverageDigest,
            record.RetrievalPolicyVersion,
            record.PromptVersion,
            record.LanguageModelDescriptor,
            record.CorrelationId,
            record.CreatedAt,
            [citation],
            [page]);
    }

    private static async Task AssertNoAnswerEvidenceMutationAsync(
        SqlitePersistenceFixture fixture)
    {
        Assert.Equal(0, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM answer_evidence_records;"));
        Assert.Equal(0, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM answer_evidence_citations;"));
        Assert.Equal(0, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM answer_evidence_pages;"));
        Assert.Equal(0, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM admin_operations WHERE operation_kind = 'AnswerEvidence';"));
        Assert.Equal(0, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM audit_events WHERE event_type = 'AnswerEvidenceCreated';"));
    }

    private static AdministrativeAuditContext Audit(
        string operationId,
        DateTimeOffset requestedAt) =>
        new(
            new OperationId(operationId),
            "integration-test",
            "activate-generation",
            "synthetic answer-evidence fixture",
            requestedAt);

    private static AnswerEvidenceRecordV1 CloneRecord(
        AnswerEvidenceRecordV1 record,
        AnswerEvidenceRecordId recordId,
        string? correlationId = null) =>
        AnswerEvidenceRecordV1.Create(
            recordId,
            record.CorpusId,
            record.ActivationRecordRevision,
            record.CatalogueRevision,
            record.SourceBindingSetDigest,
            record.ActivationBindingSetDigest,
            record.IndexGenerationId,
            record.QuestionLanguage,
            record.AnswerSha256,
            record.AnswerUtf8ByteLength,
            record.EvidenceCoverageDigest,
            record.RetrievalPolicyVersion,
            record.PromptVersion,
            record.LanguageModelDescriptor,
            correlationId ?? record.CorrelationId,
            record.CreatedAt,
            record.Citations,
            record.PageImages);

    private static string Hash(string value) => Hash(Encoding.UTF8.GetBytes(value));

    private static string Hash(ReadOnlySpan<byte> value) =>
        Convert.ToHexString(SHA256.HashData(value)).ToLowerInvariant();

    public enum AuthorityMismatch
    {
        Citation,
        Source,
        Activation,
        Manifest,
        Page,
    }

    private sealed class ThrowingFaultInjector(AnswerEvidenceStoreFaultPoint faultPoint)
        : IAnswerEvidenceStoreFaultInjector
    {
        public void ThrowIfRequested(AnswerEvidenceStoreFaultPoint point)
        {
            if (point == faultPoint)
            {
                throw new InjectedAnswerEvidenceFailure();
            }
        }
    }

    private sealed class InjectedAnswerEvidenceFailure : Exception;
}
