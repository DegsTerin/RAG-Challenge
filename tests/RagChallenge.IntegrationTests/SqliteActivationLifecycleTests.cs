// Purpose: Verifies control-plane CAS, digest and observation gates, bounded retention, audited cleanup, rollback by new revision, and isolated recovery.
using System.Globalization;
using System.Text;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;
using RagChallenge.Infrastructure.Persistence;

namespace RagChallenge.IntegrationTests;

public sealed class SqliteActivationLifecycleTests
{
    [Fact]
    public async Task TextOnlyPdfActivationPersistsWithoutARenderManifest()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (_, binding) = await fixture.CommitLocalCatalogueAsync();
        var generation = await fixture.CommitGenerationAsync(binding, "text-only-pdf");
        var rendered = await fixture.CreateActivationEvidenceAsync(binding);
        var textOnly = new DocumentActivationEvidenceBinding(
            binding,
            rendered.SourceContentObjectId,
            rendered.Rights,
            renderManifestId: null);

        var proposed = ActivationRecordFactory.CreateInitial(
            generation,
            [textOnly],
            SqlitePersistenceFixture.At(3));
        var activated = await ActivateAsync(
            fixture,
            "activate-text-only-pdf",
            ActivationMutationKind.Initial,
            expectedRevision: 0,
            proposed,
            SqlitePersistenceFixture.At(3));
        var snapshot = await new SqliteQueryActivationReader(fixture.Options).ReadAsync(
            SqlitePersistenceFixture.CorpusId,
            SqlitePersistenceFixture.At(4));

        Assert.Equal(StoreMutationOutcome.Applied, activated.Outcome);
        var evidence = Assert.Single(snapshot!.EvidenceBindings);
        Assert.Null(evidence.EvidenceBinding.RenderManifestId);
        Assert.Null(evidence.RenderManifest);
        Assert.True(evidence.IsEligible);
    }

    [Fact]
    public async Task VisualReaderRevalidatesActiveSqliteAuthorityAndImmutableContent()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (_, binding) = await fixture.CommitLocalCatalogueAsync();
        var generation = await fixture.CommitGenerationAsync(binding, "visual-serving");
        var evidence = await fixture.CreateActivationEvidenceAsync(binding);
        var proposed = ActivationRecordFactory.CreateInitial(
            generation,
            [evidence],
            SqlitePersistenceFixture.At(2));
        var activation = await ActivateAsync(
            fixture,
            "activation-visual-serving",
            ActivationMutationKind.Initial,
            expectedRevision: 0,
            proposed,
            SqlitePersistenceFixture.At(2));
        Assert.Equal(StoreMutationOutcome.Applied, activation.Outcome);
        var manifest = Assert.IsType<DocumentRenderManifest>(
            await fixture.ControlStore.ReadAsync(
                SqlitePersistenceFixture.CorpusId,
                evidence.RenderManifestId!));
        var page = Assert.Single(manifest.OrderedPageImages);
        var reader = new VerifiedPageImageEvidenceReader(
            SqlitePersistenceFixture.CorpusId,
            new SqliteQueryActivationReader(fixture.Options),
            fixture.ControlStore,
            fixture.ContentStore);
        var selector = new VisualEvidenceSelector(
            generation.IndexGenerationId,
            manifest.RenderManifestId,
            page.PageNumber,
            page.ImageContentObjectId);

        var available = await reader.ReadAsync(
            selector,
            SqlitePersistenceFixture.At(3));
        byte[] servedBytes;
        await using (var served = Assert.IsType<VisualEvidenceContent>(available.Evidence))
        {
            Assert.Equal(VisualEvidenceReadOutcome.Available, available.Outcome);
            Assert.Equal(page.ByteLength, served.Content.ByteLength);
            Assert.Equal(page.ImageContentObjectId, served.Content.ContentObjectId);
            using var copy = new MemoryStream();
            await served.Content.Content.CopyToAsync(copy);
            servedBytes = copy.ToArray();
        }

        var staleGeneration = await reader.ReadAsync(
            selector with
            {
                IndexGenerationId = new IndexGenerationId(
                    $"idxgen-{SqlitePersistenceFixture.Hash("stale-generation")}"),
            },
            SqlitePersistenceFixture.At(3));
        Assert.Equal(VisualEvidenceReadOutcome.NotAvailable, staleGeneration.Outcome);

        Assert.True(fixture.ContentStore.DeleteIfPresent(page.ImageContentObjectId));
        var missingContent = await reader.ReadAsync(
            selector,
            SqlitePersistenceFixture.At(3));
        Assert.Equal(VisualEvidenceReadOutcome.Unavailable, missingContent.Outcome);

        await using (var restoredStream = new MemoryStream(servedBytes, writable: false))
        {
            var restored = await fixture.ContentStore.PutAndVerifyAsync(
                new BoundedContentInput(
                    restoredStream,
                    servedBytes.Length,
                    ContentMediaType.ImagePng,
                    page.ImageContentObjectId));
            Assert.Equal(page.ImageContentObjectId, restored.ContentObjectId);
        }

        var currentCatalogue = Assert.IsType<CatalogueSnapshot>(
            await fixture.ControlStore.ReadCurrentCatalogueAsync(
                SqlitePersistenceFixture.CorpusId));
        var currentProduct = Assert.Single(currentCatalogue.DatabaseProducts);
        var currentDocument = Assert.Single(currentCatalogue.DocumentVersions);
        var deactivatedCatalogue = new CatalogueSnapshot(
            currentCatalogue.CorpusId,
            new CatalogueRevision(2),
            currentCatalogue.DatabaseCategories,
            [new DatabaseProduct(
                currentProduct.Id,
                currentProduct.Revision,
                currentProduct.DisplayName,
                CatalogueItemStatus.Deactivated,
                currentProduct.CategoryIds)],
            [new DocumentVersion(
                currentDocument.Id,
                currentDocument.Version,
                currentDocument.DatabaseProductId,
                currentDocument.DatabaseProductRevision,
                currentDocument.Format,
                currentDocument.ContentLanguage,
                CatalogueItemStatus.Deactivated,
                currentDocument.ContentObjectId,
                currentDocument.ByteLength,
                currentDocument.MediaType,
                currentDocument.SourceAdapterId,
                currentDocument.SourceTrustClass,
                currentDocument.OfficialSourceRegistrationId,
                currentDocument.OfficialSnapshotId,
                currentDocument.SourceDeclaredLanguage)]);
        var deactivated = await fixture.ControlStore.CommitCatalogueAsync(
            new CatalogueCommitRequest(
                new OperationId("catalogue-visual-deactivated"),
                deactivatedCatalogue,
                ExpectedCurrentRevision: 1,
                SqlitePersistenceFixture.At(4)));
        Assert.Equal(StoreMutationOutcome.Applied, deactivated.Outcome);

        var noLongerActive = await reader.ReadAsync(
            selector,
            SqlitePersistenceFixture.At(5));
        Assert.Equal(VisualEvidenceReadOutcome.NotAvailable, noLongerActive.Outcome);
    }

    [Theory]
    [InlineData(DocumentRightDecisionState.Denied, VisualEvidenceReadOutcome.Available)]
    [InlineData(DocumentRightDecisionState.Unproven, VisualEvidenceReadOutcome.NotAvailable)]
    public async Task VisualReaderEnforcesTheProvenDistributionBoundaryBeforeServing(
        DocumentRightDecisionState distributionState,
        VisualEvidenceReadOutcome expectedOutcome)
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (_, binding) = await fixture.CommitLocalCatalogueAsync();
        var generation = await fixture.CommitGenerationAsync(binding, "visual-rights-boundary");
        var evidence = await fixture.CreateActivationEvidenceAsync(
            binding,
            overriddenRight: DocumentRight.SourceAndDerivativeByteDistributionOrPublication,
            overriddenState: distributionState);
        var proposed = ActivationRecordFactory.CreateInitial(
            generation,
            [evidence],
            SqlitePersistenceFixture.At(2));
        var activation = await ActivateAsync(
            fixture,
            $"activation-visual-rights-{distributionState.ToString().ToLowerInvariant()}",
            ActivationMutationKind.Initial,
            expectedRevision: 0,
            proposed,
            SqlitePersistenceFixture.At(2));
        Assert.Equal(StoreMutationOutcome.Applied, activation.Outcome);
        var manifest = Assert.IsType<DocumentRenderManifest>(
            await fixture.ControlStore.ReadAsync(
                SqlitePersistenceFixture.CorpusId,
                evidence.RenderManifestId!));
        var page = Assert.Single(manifest.OrderedPageImages);
        var reader = new VerifiedPageImageEvidenceReader(
            SqlitePersistenceFixture.CorpusId,
            new SqliteQueryActivationReader(fixture.Options),
            fixture.ControlStore,
            fixture.ContentStore);

        var result = await reader.ReadAsync(
            new VisualEvidenceSelector(
                generation.IndexGenerationId,
                manifest.RenderManifestId,
                page.PageNumber,
                page.ImageContentObjectId),
            SqlitePersistenceFixture.At(3));

        Assert.Equal(expectedOutcome, result.Outcome);

        if (expectedOutcome == VisualEvidenceReadOutcome.Available)
        {
            await Assert.IsType<VisualEvidenceContent>(result.Evidence).DisposeAsync();
        }
        else
        {
            Assert.Null(result.Evidence);
        }
    }

    [Theory]
    [InlineData(false, VisualEvidenceReadOutcome.Available)]
    [InlineData(true, VisualEvidenceReadOutcome.Unavailable)]
    public async Task NoticeBearingVisualReaderRevalidatesTheExactRightsMapping(
        bool mismatchActiveRights,
        VisualEvidenceReadOutcome expectedOutcome)
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (catalogue, binding) = await fixture.CommitLocalCatalogueAsync();
        var document = Assert.Single(catalogue.DocumentVersions);
        var generation = await fixture.CommitGenerationAsync(binding, "notice-visual-serving");
        var mappedRights = CreateRights(binding);
        var obligationSet = DerivativeObligationSetV1.Create(
            mappedRights,
            document.ContentObjectId,
            mappedRights.Decisions.Select(decision => decision.EvidenceReference),
            DocumentContentLanguage.EnGb,
            "Synthetic Documentation Group",
            "Synthetic Notice Reference",
            "1.0",
            "synthetic-source-v1",
            "Synthetic attribution.",
            "Synthetic copyright notice.",
            "Synthetic permission notice.",
            ["Synthetic disclaimer."],
            DerivativeTrademarkTreatment.NotApplicable,
            "NotApplicable: no trademark applies to this synthetic fixture.",
            "Rendered synthetic derivative with an unchanged source region.",
            SqlitePersistenceFixture.At(2),
            "assessor-synthetic-v1");
        var imageBytes = CreatePngHeader(width: 1, height: 2);
        await using var imageStream = new MemoryStream(imageBytes, writable: false);
        var image = await fixture.ContentStore.PutAndVerifyAsync(new BoundedContentInput(
            imageStream,
            imageBytes.Length,
            ContentMediaType.ImagePng));
        var profile = new RenderProfileId(RenderProfileId.PdfPagePngNoticeV1);
        var renderer = new RendererDescriptor("notice-png-v1:synthetic");
        var page = new DocumentPageImage(
            binding.DocumentId,
            binding.DocumentVersion,
            document.ContentObjectId,
            1,
            profile,
            renderer,
            image.ContentObjectId,
            new ImageSha256(image.Sha256.Value),
            image.ByteLength,
            DocumentPageImage.PngMediaType,
            1,
            2,
            sourceRegionWidthPixels: 1,
            sourceRegionHeightPixels: 1,
            noticeRegionHeightPixels: 1);
        var manifest = DocumentRenderManifest.CreateNoticeBearing(
            binding.DocumentId,
            binding.DocumentVersion,
            document.ContentObjectId,
            1,
            renderer,
            obligationSet,
            [page],
            SqlitePersistenceFixture.At(2));
        var committed = await fixture.ControlStore.CommitAsync(
            new RenderManifestCommitRequest(
                SqlitePersistenceFixture.CorpusId,
                manifest,
                obligationSet));
        Assert.Equal(StoreMutationOutcome.Applied, committed.Outcome);
        var activeRights = mismatchActiveRights
            ? CreateRights(
                binding,
                DocumentRight.SourceAndDerivativeByteDistributionOrPublication,
                DocumentRightDecisionState.Denied)
            : mappedRights;
        var evidence = new DocumentActivationEvidenceBinding(
            binding,
            document.ContentObjectId,
            activeRights,
            manifest.RenderManifestId);
        var proposed = ActivationRecordFactory.CreateInitial(
            generation,
            [evidence],
            SqlitePersistenceFixture.At(2));
        var activation = await ActivateAsync(
            fixture,
            $"activation-notice-{mismatchActiveRights.ToString().ToLowerInvariant()}",
            ActivationMutationKind.Initial,
            0,
            proposed,
            SqlitePersistenceFixture.At(2));
        Assert.Equal(StoreMutationOutcome.Applied, activation.Outcome);
        var reader = new VerifiedPageImageEvidenceReader(
            SqlitePersistenceFixture.CorpusId,
            new SqliteQueryActivationReader(fixture.Options),
            fixture.ControlStore,
            fixture.ContentStore);

        var result = await reader.ReadAsync(
            new VisualEvidenceSelector(
                generation.IndexGenerationId,
                manifest.RenderManifestId,
                1,
                page.ImageContentObjectId),
            SqlitePersistenceFixture.At(3));

        Assert.Equal(expectedOutcome, result.Outcome);

        if (result.Evidence is not null)
        {
            await result.Evidence.DisposeAsync();
        }
    }

    [Fact]
    public async Task ActivationAndQueryReaderPreserveStoredBcp47ForV2Projection()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (_, binding) = await fixture.CommitLocalCatalogueAsync(
            sourceDeclaredLanguage: new SourceDeclaredLanguage("EN-gb"));
        var manifest = await fixture.CommitGenerationAsync(binding, "language-gate");
        var proposed = ActivationRecordFactory.CreateInitial(
            manifest,
            [await fixture.CreateActivationEvidenceAsync(binding)],
            SqlitePersistenceFixture.At(2));
        await ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            "UPDATE document_versions SET content_language = 'en';");

        var activated = await ActivateAsync(
            fixture,
            "activation-language-v2",
            ActivationMutationKind.Initial,
            expectedRevision: 0,
            proposed,
            SqlitePersistenceFixture.At(2));

        Assert.Equal(StoreMutationOutcome.Applied, activated.Outcome);

        var snapshot = Assert.IsType<QueryActivationSnapshot>(
            await new SqliteQueryActivationReader(fixture.Options).ReadAsync(
                SqlitePersistenceFixture.CorpusId,
                SqlitePersistenceFixture.At(3)));

        var queryBinding = Assert.Single(snapshot.EvidenceBindings);
        Assert.Equal("en", queryBinding.ContentLanguage.ToCanonicalTag());
        Assert.Equal("EN-gb", queryBinding.SourceDeclaredLanguage!.ObservedTag);
    }

    [Fact]
    public async Task CasRejectsAllThreeDigestMismatchesBeforeActivation()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (_, binding) = await fixture.CommitLocalCatalogueAsync();
        var manifest = await fixture.CommitGenerationAsync(binding, "digest-gate");
        var mismatchedBinding = new DocumentBinding(
            binding.DatabaseProductId,
            binding.DatabaseProductRevision,
            binding.DocumentId,
            binding.DocumentVersion,
            DocumentFormat.Csv,
            new SourceAdapterId("different-adapter"),
            SourceTrustClass.LocalAuthorised);
        var proposed = new CorpusActivationRecord(
            SqlitePersistenceFixture.CorpusId,
            new ActivationRecordRevision(1),
            previousRecordRevision: null,
            manifest.IndexGenerationId,
            manifest.CatalogueRevision,
            new ActivationBindingSetDigest(new string('0', 64)),
            [mismatchedBinding],
            SqlitePersistenceFixture.At(2),
            SqlitePersistenceFixture.At(2));

        var result = await fixture.ControlStore.CompareExchangeActivationAsync(
            new ActivationCompareExchangeRequest(
                new OperationId("activation-digest-rejected"),
                ActivationMutationKind.Initial,
                ExpectedCurrentRevision: 0,
                proposed,
                SqlitePersistenceFixture.CompatibilityKey,
                SqlitePersistenceFixture.At(2),
                SqliteControlPlaneStore.MinimumPreviousGenerationRetention));

        Assert.Equal(StoreMutationOutcome.ValidationFailed, result.Outcome);
        Assert.Contains(
            ActivationValidationFailure.ActiveDocumentSetDigestMismatch,
            result.ValidationFailures);
        Assert.Contains(
            ActivationValidationFailure.SourceBindingSetDigestMismatch,
            result.ValidationFailures);
        Assert.Contains(
            ActivationValidationFailure.ActivationBindingSetDigestMismatch,
            result.ValidationFailures);
        Assert.Null(await fixture.ControlStore.ReadActiveActivationAsync(
            SqlitePersistenceFixture.CorpusId));
    }

    [Fact]
    public async Task CasRejectsContentThatIsNoLongerReopenableByItsSha256Identity()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (catalogue, binding) = await fixture.CommitLocalCatalogueAsync();
        var manifest = await fixture.CommitGenerationAsync(binding, "content-readback");
        var contentObjectId = Assert.Single(catalogue.DocumentVersions).ContentObjectId;
        var objectPath = Path.Combine(
            fixture.Options.ContentStoreRoot,
            "objects",
            contentObjectId.Value[..2],
            $"{contentObjectId.Value}.bin");
        await File.WriteAllTextAsync(objectPath, "corrupted after generation finalisation");
        var initial = ActivationRecordFactory.CreateInitial(
            manifest,
            [await fixture.CreateActivationEvidenceAsync(binding)],
            SqlitePersistenceFixture.At(2));

        var result = await ActivateAsync(
            fixture,
            "activation-content-rejected",
            ActivationMutationKind.Initial,
            expectedRevision: 0,
            initial,
            SqlitePersistenceFixture.At(2));

        Assert.Equal(StoreMutationOutcome.ValidationFailed, result.Outcome);
        Assert.Null(await fixture.ControlStore.ReadActiveActivationAsync(
            SqlitePersistenceFixture.CorpusId));
    }

    [Fact]
    public async Task CasRejectsVectorPayloadChangedAfterManifestCommit()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (_, binding) = await fixture.CommitLocalCatalogueAsync();
        var manifest = await fixture.CommitGenerationAsync(binding, "vector-readback");
        await ExecuteAsync(
            fixture.Options.VectorDatabasePath,
            "UPDATE vector_chunks SET vector = zeroblob(length(vector));");
        var initial = ActivationRecordFactory.CreateInitial(
            manifest,
            [await fixture.CreateActivationEvidenceAsync(binding)],
            SqlitePersistenceFixture.At(2));

        var result = await ActivateAsync(
            fixture,
            "activation-vector-rejected",
            ActivationMutationKind.Initial,
            expectedRevision: 0,
            initial,
            SqlitePersistenceFixture.At(2));

        Assert.Equal(StoreMutationOutcome.ValidationFailed, result.Outcome);
        Assert.Null(await fixture.ControlStore.ReadActiveActivationAsync(
            SqlitePersistenceFixture.CorpusId));
    }

    [Fact]
    public async Task ConcurrentCasRetentionCleanupRollbackAndRecoveryRemainAuditable()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (_, binding) = await fixture.CommitLocalCatalogueAsync();
        var officialBytes = Encoding.UTF8.GetBytes("unbound official snapshot");
        await using var officialStream = new MemoryStream(officialBytes, writable: false);
        var officialContent = await fixture.ContentStore.PutAndVerifyAsync(
            new BoundedContentInput(
                officialStream,
                officialBytes.Length,
                ContentMediaType.ApplicationPdf));
        var officialRegistration = new OfficialSourceRegistration(
            new OfficialSourceRegistrationId("cleanup-official-registration"),
            new SourceRegistrationRevision(1),
            binding.DatabaseProductId,
            binding.DocumentId,
            new SourceAdapterId("cleanup-official-adapter"),
            "https://maintainer.example/cleanup.pdf",
            CatalogueItemStatus.Active);
        var officialSnapshot = new OfficialSourceSnapshot(
            new OfficialSnapshotId("cleanup-official-snapshot"),
            officialRegistration.Id,
            officialContent.ContentObjectId,
            officialContent.ByteLength,
            "application/pdf",
            SqlitePersistenceFixture.At(1));
        var officialCommit = await fixture.ControlStore.CommitOfficialSourceAsync(
            new OfficialSourceCommitRequest(
                new OperationId("cleanup-official-commit"),
                SqlitePersistenceFixture.CorpusId,
                officialRegistration,
                officialSnapshot,
                SqlitePersistenceFixture.At(1)));
        Assert.Equal(StoreMutationOutcome.Applied, officialCommit.Outcome);
        var sharedBytes = Encoding.UTF8.GetBytes("content referenced by another corpus");
        await using var sharedStream = new MemoryStream(sharedBytes, writable: false);
        var sharedContent = await fixture.ContentStore.PutAndVerifyAsync(
            new BoundedContentInput(
                sharedStream,
                sharedBytes.Length,
                ContentMediaType.ApplicationPdf));
        var sharedCorpusId = new CorpusId("shared-content-corpus");
        var sharedCategory = new DatabaseCategory(
            new DatabaseCategoryId("shared-content-category"),
            "Shared content category");
        var sharedProduct = new DatabaseProduct(
            new DatabaseProductId("shared-content-product"),
            new DatabaseProductRevision(1),
            "Shared content product",
            CatalogueItemStatus.Active,
            [sharedCategory.Id]);
        var sharedDocument = new DocumentVersion(
            new DocumentId("shared-content-document"),
            new DocumentVersionNumber(1),
            sharedProduct.Id,
            sharedProduct.Revision,
            DocumentFormat.Pdf,
            DocumentContentLanguage.EnGb,
            CatalogueItemStatus.Active,
            sharedContent.ContentObjectId,
            sharedContent.ByteLength,
            "application/pdf",
            new SourceAdapterId("shared-content-adapter"),
            SourceTrustClass.LocalAuthorised);
        var sharedCommit = await fixture.ControlStore.CommitCatalogueAsync(
            new CatalogueCommitRequest(
                new OperationId("shared-content-catalogue"),
                new CatalogueSnapshot(
                    sharedCorpusId,
                    new CatalogueRevision(1),
                    [sharedCategory],
                    [sharedProduct],
                    [sharedDocument]),
                ExpectedCurrentRevision: 0,
                SqlitePersistenceFixture.At(1)));
        Assert.Equal(StoreMutationOutcome.Applied, sharedCommit.Outcome);
        var generationA = await fixture.CommitGenerationAsync(binding, "a");
        var initial = ActivationRecordFactory.CreateInitial(
            generationA,
            [await fixture.CreateActivationEvidenceAsync(binding)],
            SqlitePersistenceFixture.At(2));
        var initialResult = await ActivateAsync(
            fixture,
            "activation-a",
            ActivationMutationKind.Initial,
            expectedRevision: 0,
            initial,
            SqlitePersistenceFixture.At(2));
        Assert.Equal(StoreMutationOutcome.Applied, initialResult.Outcome);

        var generationB = await fixture.CommitGenerationAsync(binding, "b");
        var replacement = ActivationRecordFactory.CreateGenerationReplacement(
            initial,
            generationB,
            [await fixture.CreateActivationEvidenceAsync(binding)],
            SqlitePersistenceFixture.At(3));
        var concurrentResults = await Task.WhenAll(
            ActivateAsync(
                fixture,
                "activation-b-first",
                ActivationMutationKind.Replacement,
                expectedRevision: 1,
                replacement,
                SqlitePersistenceFixture.At(3)),
            new SqliteControlPlaneStore(fixture.Options).CompareExchangeActivationAsync(
                new ActivationCompareExchangeRequest(
                    new OperationId("activation-b-second"),
                    ActivationMutationKind.Replacement,
                    ExpectedCurrentRevision: 1,
                    replacement,
                    SqlitePersistenceFixture.CompatibilityKey,
                    SqlitePersistenceFixture.At(3),
                    SqliteControlPlaneStore.MinimumPreviousGenerationRetention)));
        Assert.Single(
            concurrentResults,
            result => result.Outcome == StoreMutationOutcome.Applied);
        Assert.Single(
            concurrentResults,
            result => result.Outcome == StoreMutationOutcome.RevisionConflict);
        Assert.Equal(
            SqlitePersistenceFixture.At(17),
            await ReadRetentionUntilAsync(fixture, generationA.IndexGenerationId));

        var currentB = await fixture.ControlStore.ReadActiveActivationAsync(
            SqlitePersistenceFixture.CorpusId);
        Assert.NotNull(currentB);
        var rollback = ActivationRecordFactory.CreateRollback(
            currentB,
            generationA,
            [await fixture.CreateActivationEvidenceAsync(binding)],
            SqlitePersistenceFixture.At(4));
        var rollbackResult = await ActivateAsync(
            fixture,
            "activation-rollback-a",
            ActivationMutationKind.Rollback,
            expectedRevision: 2,
            rollback,
            SqlitePersistenceFixture.At(4));
        Assert.Equal(StoreMutationOutcome.Applied, rollbackResult.Outcome);
        Assert.Equal(3, rollbackResult.CurrentRecord!.RecordRevision.Value);
        Assert.Equal(generationA.IndexGenerationId, rollbackResult.CurrentRecord.IndexGenerationId);

        var generationC = await fixture.CommitGenerationAsync(binding, "c");
        var replacementC = ActivationRecordFactory.CreateGenerationReplacement(
            rollbackResult.CurrentRecord,
            generationC,
            [await fixture.CreateActivationEvidenceAsync(binding)],
            SqlitePersistenceFixture.At(5));
        var replacementCResult = await ActivateAsync(
            fixture,
            "activation-c",
            ActivationMutationKind.Replacement,
            expectedRevision: 3,
            replacementC,
            SqlitePersistenceFixture.At(5));
        Assert.Equal(StoreMutationOutcome.Applied, replacementCResult.Outcome);

        var orphanBytes = Encoding.UTF8.GetBytes("unreachable synthetic content");
        await using var orphanStream = new MemoryStream(orphanBytes, writable: false);
        var orphan = await fixture.ContentStore.PutAndVerifyAsync(
            new BoundedContentInput(
                orphanStream,
                orphanBytes.Length,
                ContentMediaType.ApplicationOctetStream));
        await RegisterOrphanContentAsync(fixture, orphan, SqlitePersistenceFixture.At(5));
        var cleanup = new SqliteStorageMaintenance(fixture.Options);
        var cleanupResult = await cleanup.RunManualCleanupAsync(
            new OperationId("cleanup-expired-hold"),
            SqlitePersistenceFixture.CorpusId,
            SqlitePersistenceFixture.At(18));
        Assert.Equal(1, cleanupResult.RemovedVectorGenerations);
        Assert.Equal(1, cleanupResult.RemovedContentObjects);
        Assert.False(cleanupResult.AlreadyApplied);
        Assert.Equal(0, await ScalarAsync(
            fixture.Options.VectorDatabasePath,
            $"SELECT COUNT(*) FROM vector_builds WHERE index_generation_id = '{generationB.IndexGenerationId.Value}';"));
        await Assert.ThrowsAsync<FileNotFoundException>(async () =>
        {
            await using var _ = await fixture.ContentStore.OpenVerifiedAsync(
                orphan.ContentObjectId,
                new ExpectedHashAndLength(orphan.Sha256, orphan.ByteLength));
        });
        await using (var preservedOfficial = await fixture.ContentStore.OpenVerifiedAsync(
            officialContent.ContentObjectId,
            new ExpectedHashAndLength(officialContent.Sha256, officialContent.ByteLength)))
        {
            Assert.Equal(officialContent.ByteLength, preservedOfficial.Content.Length);
        }
        await using (var preservedShared = await fixture.ContentStore.OpenVerifiedAsync(
            sharedContent.ContentObjectId,
            new ExpectedHashAndLength(sharedContent.Sha256, sharedContent.ByteLength)))
        {
            Assert.Equal(sharedContent.ByteLength, preservedShared.Content.Length);
        }
        Assert.Equal(0, await fixture.ScalarAsync(
            $"SELECT COUNT(*) FROM content_objects WHERE content_sha256 = '{orphan.ContentObjectId.Value}';"));
        Assert.Equal(2, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM audit_events WHERE operation_id = 'cleanup-expired-hold';"));

        var replay = await cleanup.RunManualCleanupAsync(
            new OperationId("cleanup-expired-hold"),
            SqlitePersistenceFixture.CorpusId,
            SqlitePersistenceFixture.At(18));
        Assert.True(replay.AlreadyApplied);

        var recovery = new SqliteRecoverySnapshotService(fixture.Options);
        var recoveryResult = await recovery.CreateAndVerifyAsync(
            new OperationId("recovery-verified"),
            SqlitePersistenceFixture.CorpusId,
            Path.Combine(fixture.RootPath, "recovery"),
            SqlitePersistenceFixture.At(18));
        var verified = await SqliteRecoverySnapshotService.VerifyIsolatedAsync(
            recoveryResult.SnapshotPath);
        Assert.True(verified.IsValid, string.Join(Environment.NewLine, verified.Failures));
        Assert.Equal(4, recoveryResult.ContentObjectCount);
        Assert.Equal(0, await fixture.ScalarAsync("SELECT COUNT(*) FROM recovery_leases;"));
        Assert.Equal(2, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM audit_events WHERE operation_id = 'recovery-verified';"));

        var copiedContent = Directory.EnumerateFiles(
            Path.Combine(recoveryResult.SnapshotPath, "content"),
            "*.bin",
            SearchOption.AllDirectories).ToArray();
        Assert.Equal(4, copiedContent.Length);
        await File.AppendAllTextAsync(copiedContent[0], "corruption");
        var corrupted = await SqliteRecoverySnapshotService.VerifyIsolatedAsync(
            recoveryResult.SnapshotPath);
        Assert.False(corrupted.IsValid);
        Assert.Contains(corrupted.Failures, failure =>
            failure.Contains("mismatch", StringComparison.Ordinal));
    }

    [Fact]
    public async Task OfficialActivationRequiresAStoredCurrentObservation()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var bytes = Encoding.UTF8.GetBytes("synthetic official snapshot");
        await using var content = new MemoryStream(bytes, writable: false);
        var contentResult = await fixture.ContentStore.PutAndVerifyAsync(
            new BoundedContentInput(
                content,
                bytes.Length,
                ContentMediaType.ApplicationPdf));
        var registrationId = new OfficialSourceRegistrationId("official-registration");
        var snapshotId = new OfficialSnapshotId("official-snapshot");
        var productId = new DatabaseProductId("db-official");
        var productRevision = new DatabaseProductRevision(1);
        var documentId = new DocumentId("doc-official");
        var documentVersion = new DocumentVersionNumber(1);
        var adapterId = new SourceAdapterId("official-fixture");
        var category = new DatabaseCategory(
            new DatabaseCategoryId("category-official"),
            "Official fixture");
        var product = new DatabaseProduct(
            productId,
            productRevision,
            "Official Database",
            CatalogueItemStatus.Active,
            [category.Id]);
        var document = new DocumentVersion(
            documentId,
            documentVersion,
            productId,
            productRevision,
            DocumentFormat.Pdf,
            DocumentContentLanguage.EnGb,
            CatalogueItemStatus.Active,
            contentResult.ContentObjectId,
            contentResult.ByteLength,
            "application/pdf",
            adapterId,
            SourceTrustClass.OfficialExternal,
            registrationId,
            snapshotId);
        var bootstrapDocument = new DocumentVersion(
            documentId,
            new DocumentVersionNumber(2),
            productId,
            productRevision,
            DocumentFormat.Pdf,
            DocumentContentLanguage.EnGb,
            CatalogueItemStatus.Active,
            contentResult.ContentObjectId,
            contentResult.ByteLength,
            "application/pdf",
            new SourceAdapterId("local-official-bootstrap"),
            SourceTrustClass.LocalAuthorised);
        var bootstrapCatalogue = new CatalogueSnapshot(
            SqlitePersistenceFixture.CorpusId,
            new CatalogueRevision(1),
            [category],
            [product],
            [bootstrapDocument]);
        Assert.Equal(StoreMutationOutcome.Applied, (
            await fixture.ControlStore.CommitCatalogueAsync(
                new CatalogueCommitRequest(
                    new OperationId("catalogue-official-bootstrap"),
                    bootstrapCatalogue,
                    ExpectedCurrentRevision: 0,
                    SqlitePersistenceFixture.At(1)))).Outcome);
        var registration = new OfficialSourceRegistration(
            registrationId,
            new SourceRegistrationRevision(1),
            productId,
            documentId,
            adapterId,
            "https://maintainer.example/docs.pdf",
            CatalogueItemStatus.Active);
        var snapshot = new OfficialSourceSnapshot(
            snapshotId,
            registrationId,
            contentResult.ContentObjectId,
            contentResult.ByteLength,
            "application/pdf",
            SqlitePersistenceFixture.At(1));
        Assert.Equal(StoreMutationOutcome.Applied, (
            await fixture.ControlStore.CommitOfficialSourceAsync(
                new OfficialSourceCommitRequest(
                    new OperationId("official-source"),
                    SqlitePersistenceFixture.CorpusId,
                    registration,
                    snapshot,
                    SqlitePersistenceFixture.At(1)))).Outcome);

        var catalogue = new CatalogueSnapshot(
            SqlitePersistenceFixture.CorpusId,
            new CatalogueRevision(2),
            [category],
            [product],
            [document]);
        Assert.Equal(StoreMutationOutcome.Applied, (
            await fixture.ControlStore.CommitCatalogueAsync(
                new CatalogueCommitRequest(
                    new OperationId("catalogue-official"),
                    catalogue,
                    ExpectedCurrentRevision: 1,
                    SqlitePersistenceFixture.At(1)))).Outcome);

        var missingBinding = CreateOfficialBinding(
            productId,
            productRevision,
            documentId,
            documentVersion,
            adapterId,
            registrationId,
            snapshotId,
            new OfficialObservationId("observation-missing"));
        var manifest = await fixture.CommitGenerationAsync(
            missingBinding,
            "official",
            catalogueRevision: 2);
        var missingObservationRecord = ActivationRecordFactory.CreateInitial(
            manifest,
            [await fixture.CreateActivationEvidenceAsync(missingBinding)],
            SqlitePersistenceFixture.At(2));
        var rejected = await ActivateAsync(
            fixture,
            "activation-observation-rejected",
            ActivationMutationKind.Initial,
            expectedRevision: 0,
            missingObservationRecord,
            SqlitePersistenceFixture.At(2));
        Assert.Equal(StoreMutationOutcome.ValidationFailed, rejected.Outcome);
        Assert.Contains(
            ActivationValidationFailure.ObservationMissing,
            rejected.ValidationFailures);

        var observation = new OfficialSourceObservation(
            new OfficialObservationId("observation-current"),
            registrationId,
            snapshotId,
            new ObservationJournalRevision(1),
            OfficialObservationState.Current,
            SqlitePersistenceFixture.At(1),
            TimeSpan.FromDays(7));
        Assert.Equal(StoreMutationOutcome.Applied, (
            await fixture.ControlStore.AppendObservationAsync(
                new ObservationCommitRequest(
                    new OperationId("observation-append"),
                    SqlitePersistenceFixture.CorpusId,
                    observation,
                    ExpectedJournalRevision: 0,
                    SqlitePersistenceFixture.At(1)))).Outcome);
        var observedBinding = missingBinding.WithObservation(observation.Id);
        var acceptedRecord = ActivationRecordFactory.CreateInitial(
            manifest,
            [await fixture.CreateActivationEvidenceAsync(observedBinding)],
            SqlitePersistenceFixture.At(2));
        var accepted = await ActivateAsync(
            fixture,
            "activation-observation-accepted",
            ActivationMutationKind.Initial,
            expectedRevision: 0,
            acceptedRecord,
            SqlitePersistenceFixture.At(2));
        Assert.Equal(StoreMutationOutcome.Applied, accepted.Outcome);
    }

    [Fact]
    public async Task ActivationPersistsExactEvidenceAndReplayRejectsAnyRightsDivergence()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (_, binding) = await fixture.CommitLocalCatalogueAsync();
        var manifest = await fixture.CommitGenerationAsync(binding, "evidence-replay");
        var evidence = await fixture.CreateActivationEvidenceAsync(binding);
        var proposed = ActivationRecordFactory.CreateInitial(
            manifest,
            [evidence],
            SqlitePersistenceFixture.At(2));
        var request = new ActivationCompareExchangeRequest(
            new OperationId("activation-evidence-replay"),
            ActivationMutationKind.Initial,
            ExpectedCurrentRevision: 0,
            proposed,
            SqlitePersistenceFixture.CompatibilityKey,
            SqlitePersistenceFixture.At(2),
            SqliteControlPlaneStore.MinimumPreviousGenerationRetention);

        var applied = await fixture.ControlStore.CompareExchangeActivationAsync(request);
        var replayed = await fixture.ControlStore.CompareExchangeActivationAsync(request);
        var persisted = Assert.IsType<CorpusActivationRecord>(
            await fixture.ControlStore.ReadActiveActivationAsync(
                SqlitePersistenceFixture.CorpusId));

        Assert.Equal(StoreMutationOutcome.Applied, applied.Outcome);
        Assert.Equal(StoreMutationOutcome.AlreadyApplied, replayed.Outcome);
        Assert.True(persisted.HasCompleteEvidenceBindings);
        Assert.Equal(evidence.SourceContentObjectId, persisted.EvidenceBindings[0].SourceContentObjectId);
        Assert.Equal(evidence.RenderManifestId, persisted.EvidenceBindings[0].RenderManifestId);
        Assert.Equal(10, persisted.EvidenceBindings[0].Rights.Decisions.Count);
        Assert.Equal(1, await fixture.ScalarAsync("SELECT COUNT(*) FROM activation_evidence_bindings;"));
        Assert.Equal(10, await fixture.ScalarAsync("SELECT COUNT(*) FROM activation_rights_decisions;"));

        var divergentRights = new DocumentRightsEligibilityRecordV1(
            binding.DocumentId,
            binding.DocumentVersion,
            evidence.Rights.Decisions.Select((decision, index) => new DocumentRightDecision(
                decision.Right,
                decision.State,
                index == 0
                    ? new DocumentRightsEvidenceReference("different-evidence-reference")
                    : decision.EvidenceReference)));
        var divergent = ActivationRecordFactory.CreateInitial(
            manifest,
            [new DocumentActivationEvidenceBinding(
                binding,
                evidence.SourceContentObjectId,
                divergentRights,
                evidence.RenderManifestId)],
            SqlitePersistenceFixture.At(2));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fixture.ControlStore.CompareExchangeActivationAsync(
                request with { ProposedRecord = divergent }));
        Assert.Equal(1, await fixture.ScalarAsync("SELECT COUNT(*) FROM activation_records;"));
        Assert.Equal(1, await fixture.ScalarAsync("SELECT record_revision FROM activation_heads;"));
    }

    [Theory]
    [InlineData("missing-manifest")]
    [InlineData("missing-page")]
    [InlineData("corrupt-page")]
    public async Task PdfEvidenceReadbackFailureLeavesNoPartialActivationAuthority(
        string failure)
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (_, binding) = await fixture.CommitLocalCatalogueAsync();
        var manifest = await fixture.CommitGenerationAsync(binding, $"pdf-{failure}");
        var evidence = await fixture.CreateActivationEvidenceAsync(binding);
        var renderManifest = Assert.IsType<DocumentRenderManifest>(
            await fixture.ControlStore.ReadAsync(
                SqlitePersistenceFixture.CorpusId,
                evidence.RenderManifestId!));

        if (failure == "missing-manifest")
        {
            await ExecuteAsync(
                fixture.Options.ControlDatabasePath,
                "DELETE FROM document_page_images; DELETE FROM document_render_manifests;");
        }
        else if (failure == "missing-page")
        {
            await ExecuteAsync(
                fixture.Options.ControlDatabasePath,
                "DELETE FROM document_page_images;");
        }
        else
        {
            var imageId = Assert.Single(renderManifest.OrderedPageImages).ImageContentObjectId;
            var imagePath = Path.Combine(
                fixture.Options.ContentStoreRoot,
                "objects",
                imageId.Value[..2],
                $"{imageId.Value}.bin");
            await File.WriteAllTextAsync(imagePath, "corrupted page image");
        }

        var proposed = ActivationRecordFactory.CreateInitial(
            manifest,
            [evidence],
            SqlitePersistenceFixture.At(2));
        var result = await ActivateAsync(
            fixture,
            $"activation-{failure}",
            ActivationMutationKind.Initial,
            expectedRevision: 0,
            proposed,
            SqlitePersistenceFixture.At(2));

        Assert.Equal(StoreMutationOutcome.ValidationFailed, result.Outcome);
        Assert.Contains(
            ActivationValidationFailure.ActivationEvidenceBindingMismatch,
            result.ValidationFailures);
        Assert.Equal(0, await fixture.ScalarAsync("SELECT COUNT(*) FROM activation_records;"));
        Assert.Equal(0, await fixture.ScalarAsync("SELECT COUNT(*) FROM activation_bindings;"));
        Assert.Equal(0, await fixture.ScalarAsync("SELECT COUNT(*) FROM activation_evidence_bindings;"));
        Assert.Equal(0, await fixture.ScalarAsync("SELECT COUNT(*) FROM activation_rights_decisions;"));
        Assert.Equal(0, await fixture.ScalarAsync("SELECT COUNT(*) FROM activation_heads;"));
    }

    [Theory]
    [InlineData("activation_evidence_bindings", "INSERT")]
    [InlineData("activation_rights_decisions", "INSERT")]
    [InlineData("activation_heads", "UPDATE")]
    public async Task PersistenceFailurePreservesTheCompletePreviousActivationAuthority(
        string failingTable,
        string mutation)
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var (_, binding) = await fixture.CommitLocalCatalogueAsync();
        var evidence = await fixture.CreateActivationEvidenceAsync(binding);
        var generationA = await fixture.CommitGenerationAsync(binding, "failure-a");
        var initial = ActivationRecordFactory.CreateInitial(
            generationA,
            [evidence],
            SqlitePersistenceFixture.At(2));
        var initialResult = await ActivateAsync(
            fixture,
            "activation-failure-a",
            ActivationMutationKind.Initial,
            expectedRevision: 0,
            initial,
            SqlitePersistenceFixture.At(2));
        Assert.Equal(StoreMutationOutcome.Applied, initialResult.Outcome);

        var generationB = await fixture.CommitGenerationAsync(binding, "failure-b");
        var replacement = ActivationRecordFactory.CreateGenerationReplacement(
            initial,
            generationB,
            [await fixture.CreateActivationEvidenceAsync(binding)],
            SqlitePersistenceFixture.At(3));
        await ExecuteAsync(
            fixture.Options.ControlDatabasePath,
            $"""
            CREATE TRIGGER fail_activation_persistence
            BEFORE {mutation} ON {failingTable}
            BEGIN
                SELECT RAISE(ABORT, 'injected activation persistence failure');
            END;
            """);

        var exception = await Record.ExceptionAsync(() => ActivateAsync(
            fixture,
            $"activation-failure-{failingTable}",
            ActivationMutationKind.Replacement,
            expectedRevision: 1,
            replacement,
            SqlitePersistenceFixture.At(3)));
        Assert.True(exception is DbUpdateException or SqliteException);
        var active = Assert.IsType<CorpusActivationRecord>(
            await fixture.ControlStore.ReadActiveActivationAsync(
                SqlitePersistenceFixture.CorpusId));

        Assert.Equal(1, active.RecordRevision.Value);
        Assert.Equal(generationA.IndexGenerationId, active.IndexGenerationId);
        Assert.Equal(1, await fixture.ScalarAsync("SELECT COUNT(*) FROM activation_records;"));
        Assert.Equal(1, await fixture.ScalarAsync("SELECT COUNT(*) FROM activation_bindings;"));
        Assert.Equal(1, await fixture.ScalarAsync("SELECT COUNT(*) FROM activation_evidence_bindings;"));
        Assert.Equal(10, await fixture.ScalarAsync("SELECT COUNT(*) FROM activation_rights_decisions;"));
        Assert.Equal(1, await fixture.ScalarAsync("SELECT record_revision FROM activation_heads;"));
        Assert.Equal(1, await fixture.ScalarAsync("SELECT COUNT(*) FROM generation_retention;"));
        Assert.Equal(1, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM admin_operations WHERE operation_kind = 'ActivationCAS';"));
    }

    private static DocumentRightsEligibilityRecordV1 CreateRights(
        DocumentBinding binding,
        DocumentRight? overriddenRight = null,
        DocumentRightDecisionState overriddenState = DocumentRightDecisionState.Permitted) =>
        new(
            binding.DocumentId,
            binding.DocumentVersion,
            Enum.GetValues<DocumentRight>().Select(right => new DocumentRightDecision(
                right,
                right == overriddenRight
                    ? overriddenState
                    : DocumentRightDecisionState.Permitted,
                new DocumentRightsEvidenceReference($"fixture-rights-{right}"))));

    private static byte[] CreatePngHeader(int width, int height)
    {
        var bytes = new byte[24];
        byte[] signature = [137, 80, 78, 71, 13, 10, 26, 10];
        signature.CopyTo(bytes, 0);
        bytes[11] = 13;
        Encoding.ASCII.GetBytes("IHDR").CopyTo(bytes, 12);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(16, 4), width);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(20, 4), height);
        return bytes;
    }

    private static Task<ActivationMutationResult> ActivateAsync(
        SqlitePersistenceFixture fixture,
        string operationId,
        ActivationMutationKind kind,
        long expectedRevision,
        CorpusActivationRecord record,
        DateTimeOffset evaluatedAt) =>
        fixture.ControlStore.CompareExchangeActivationAsync(
            new ActivationCompareExchangeRequest(
                new OperationId(operationId),
                kind,
                expectedRevision,
                record,
                SqlitePersistenceFixture.CompatibilityKey,
                evaluatedAt,
                SqliteControlPlaneStore.MinimumPreviousGenerationRetention));

    private static DocumentBinding CreateOfficialBinding(
        DatabaseProductId productId,
        DatabaseProductRevision productRevision,
        DocumentId documentId,
        DocumentVersionNumber documentVersion,
        SourceAdapterId adapterId,
        OfficialSourceRegistrationId registrationId,
        OfficialSnapshotId snapshotId,
        OfficialObservationId observationId) =>
        new(
            productId,
            productRevision,
            documentId,
            documentVersion,
            DocumentFormat.Pdf,
            adapterId,
            SourceTrustClass.OfficialExternal,
            registrationId,
            snapshotId,
            observationId);

    private static async Task<DateTimeOffset> ReadRetentionUntilAsync(
        SqlitePersistenceFixture fixture,
        IndexGenerationId generationId)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={fixture.Options.ControlDatabasePath};Mode=ReadOnly;Cache=Private");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT retain_until_utc
            FROM generation_retention
            WHERE corpus_id = $corpusId
              AND index_generation_id = $generationId;
            """;
        command.Parameters.AddWithValue("$corpusId", SqlitePersistenceFixture.CorpusId.Value);
        command.Parameters.AddWithValue("$generationId", generationId.Value);
        var value = Convert.ToString(
            await command.ExecuteScalarAsync(),
            CultureInfo.InvariantCulture);
        return DateTimeOffset.ParseExact(
            value!,
            "O",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None);
    }

    private static async Task RegisterOrphanContentAsync(
        SqlitePersistenceFixture fixture,
        ContentObjectDescriptor content,
        DateTimeOffset registeredAt)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={fixture.Options.ControlDatabasePath};Mode=ReadWrite;Cache=Private");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO content_objects(content_sha256, byte_length, registered_at_utc)
            VALUES ($sha256, $byteLength, $registeredAtUtc);
            """;
        command.Parameters.AddWithValue("$sha256", content.ContentObjectId.Value);
        command.Parameters.AddWithValue("$byteLength", content.ByteLength);
        command.Parameters.AddWithValue("$registeredAtUtc", registeredAt.ToString("O", CultureInfo.InvariantCulture));
        _ = await command.ExecuteNonQueryAsync();
    }

    private static async Task<long> ScalarAsync(string path, string sql)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={path};Mode=ReadOnly;Cache=Private");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToInt64(
            await command.ExecuteScalarAsync(),
            CultureInfo.InvariantCulture);
    }

    private static async Task ExecuteAsync(string path, string sql)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={path};Mode=ReadWrite;Cache=Private");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        _ = await command.ExecuteNonQueryAsync();
    }
}
