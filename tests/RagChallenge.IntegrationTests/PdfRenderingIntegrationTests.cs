// Purpose: Verifies the selected native renderer, worker boundary, legacy and notice-bearing PNG policies, immutable manifest persistence and reachability with synthetic bytes only.
using System.Buffers.Binary;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using RagChallenge.Application.Administration;
using RagChallenge.Application.Documents;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;
using RagChallenge.Infrastructure.Documents;
using RagChallenge.Infrastructure.Persistence;
using RagChallenge.Server.Api.OperationsGovernance;

using SkiaSharp;

namespace RagChallenge.IntegrationTests;

[Collection(ParserRuntimeGateSerialisation.Name)]
public sealed class PdfRenderingIntegrationTests
{
    [Fact]
    public void RendererProducesRepeatableRgbPagesAt144DpiWithDeclaredRotation()
    {
        var policy = Policy(maximumPages: 2);
        var pdf = CreatePdf(
            new PageSpec(72, 36),
            new PageSpec(72, 36, Rotation: 90));
        var first = PdfToImagePdfPageRenderer.Render(
            pdf,
            policy,
            RuntimeInformation.RuntimeIdentifier);
        var repeat = PdfToImagePdfPageRenderer.Render(
            pdf,
            policy,
            RuntimeInformation.RuntimeIdentifier);
        var validator = new PngPageImageValidator();
        var firstPage = validator.Validate(first.Pages[0], policy);
        var rotatedPage = validator.Validate(first.Pages[1], policy);

        Assert.Equal((144, 72), (firstPage.WidthPixels, firstPage.HeightPixels));
        Assert.Equal((72, 144), (rotatedPage.WidthPixels, rotatedPage.HeightPixels));
        Assert.Equal(first.RendererDescriptor, repeat.RendererDescriptor);
        Assert.Equal(
            first.Pages.Select(page => page.PngBytes.ToArray()),
            repeat.Pages.Select(page => page.PngBytes.ToArray()),
            ByteArrayComparer.Instance);

        using var decoded = SKBitmap.Decode(first.Pages[0].PngBytes.ToArray());
        Assert.NotNull(decoded);
        Assert.Equal(SKColors.White, decoded.GetPixel(0, 0));
    }

    [Fact]
    public void SelectiveRendererRasterisesOnlyTheRequestedPhysicalPage()
    {
        var policy = Policy(maximumPages: 3, maximumPixels: 25_000);
        var pdf = CreatePdf(
            new PageSpec(72, 72),
            new PageSpec(72, 72),
            new PageSpec(72, 72));

        var selected = PdfToImagePdfPageRenderer.RenderSelection(
            pdf,
            policy,
            RuntimeInformation.RuntimeIdentifier,
            [2]);

        Assert.Equal(3, selected.SourcePageCount);
        Assert.Equal(2, Assert.Single(selected.Pages).PageNumber);
        var fullFailure = Assert.Throws<PdfRenderException>(() =>
            PdfToImagePdfPageRenderer.Render(
                pdf,
                policy,
                RuntimeInformation.RuntimeIdentifier));
        Assert.Equal(PdfRenderFailureKind.LimitExceeded, fullFailure.FailureKind);
    }

    [Fact]
    public async Task StreamingRendererEmitsTheSameOrderedFramesWithoutACompleteBufferedResult()
    {
        var policy = Policy(maximumPages: 3, maximumPixels: 300_000);
        var pdf = CreatePdf(
            new PageSpec(72, 36),
            new PageSpec(36, 72),
            new PageSpec(72, 36, Rotation: 90));
        var buffered = PdfToImagePdfPageRenderer.Render(
            pdf,
            policy,
            RuntimeInformation.RuntimeIdentifier);
        RendererDescriptor? streamedDescriptor = null;
        var streamedPageCount = 0;
        var streamedRenderedPageCount = 0;
        var streamed = new List<(int PageNumber, string Sha256, long TotalBytes)>();

        await PdfToImagePdfPageRenderer.RenderToAsync(
            pdf,
            policy,
            RuntimeInformation.RuntimeIdentifier,
            (descriptor, pageCount, renderedPageCount, _) =>
            {
                streamedDescriptor = descriptor;
                streamedPageCount = pageCount;
                streamedRenderedPageCount = renderedPageCount;
                return Task.CompletedTask;
            },
            (page, totalBytes, _) =>
            {
                streamed.Add((
                    page.PageNumber,
                    Convert.ToHexString(SHA256.HashData(page.PngBytes.Span))
                        .ToLowerInvariant(),
                    totalBytes));
                return Task.CompletedTask;
            });

        Assert.Equal(buffered.RendererDescriptor, streamedDescriptor);
        Assert.Equal(buffered.SourcePageCount, streamedPageCount);
        Assert.Equal(buffered.Pages.Count, streamedRenderedPageCount);
        Assert.Equal(
            buffered.Pages.Select((page, index) => (
                page.PageNumber,
                Convert.ToHexString(SHA256.HashData(page.PngBytes.Span))
                    .ToLowerInvariant(),
                buffered.Pages.Take(index + 1).Sum(item => (long)item.PngBytes.Length))),
            streamed);
    }

    [Fact]
    public void RendererAccepts4096AndRejects4097BeforeRasterisation()
    {
        var policy = Policy(maximumPages: 1, maximumPixels: 100_000);
        var accepted = PdfToImagePdfPageRenderer.Render(
            CreatePdf(new PageSpec(2048, 10)),
            policy,
            RuntimeInformation.RuntimeIdentifier);
        var validation = new PngPageImageValidator().Validate(accepted.Pages[0], policy);

        Assert.Equal(4096, validation.WidthPixels);
        var exception = Assert.Throws<PdfRenderException>(() =>
            PdfToImagePdfPageRenderer.Render(
                CreatePdf(new PageSpec(2048.5, 10)),
                policy,
                RuntimeInformation.RuntimeIdentifier));
        Assert.Equal(PdfRenderFailureKind.LimitExceeded, exception.FailureKind);
    }

    [Fact]
    public void AnnotationAndFormAppearanceAreNotRendered()
    {
        var policy = Policy(maximumPages: 1);
        var blank = PdfToImagePdfPageRenderer.Render(
            CreatePdf(new PageSpec(72, 36)),
            policy,
            RuntimeInformation.RuntimeIdentifier);
        var widget = PdfToImagePdfPageRenderer.Render(
            CreatePdf(new PageSpec(72, 36, WithWidgetAppearance: true)),
            policy,
            RuntimeInformation.RuntimeIdentifier);

        Assert.Equal(blank.Pages[0].PngBytes.ToArray(), widget.Pages[0].PngBytes.ToArray());
    }

    [Fact]
    public void PngValidatorRejectsCrcMetadataTransparencyAndUnknownChunks()
    {
        var policy = Policy(maximumPages: 1);
        var rendered = PdfToImagePdfPageRenderer.Render(
            CreatePdf(new PageSpec(72, 36)),
            policy,
            RuntimeInformation.RuntimeIdentifier);
        var valid = rendered.Pages[0];
        var validator = new PngPageImageValidator();

        Assert.Equal(8, ReadIhdr(valid.PngBytes.Span).BitDepth);
        Assert.Equal(2, ReadIhdr(valid.PngBytes.Span).ColourType);
        Assert.Throws<PdfRenderException>(() => validator.Validate(
            ReplaceIhdrColourType(valid, 6),
            policy));
        Assert.Throws<PdfRenderException>(() => validator.Validate(
            InsertChunk(valid, "tEXt", Encoding.ASCII.GetBytes("key\0value")),
            policy));
        Assert.Throws<PdfRenderException>(() => validator.Validate(
            InsertChunk(valid, "vpAg", new byte[] { 1 }),
            policy));

        var corrupt = valid.PngBytes.ToArray();
        corrupt[^1] ^= 1;
        Assert.Throws<PdfRenderException>(() => validator.Validate(
            new RenderedPdfPageCandidate(
                valid.PageNumber,
                valid.SourceWidthPoints,
                valid.SourceHeightPoints,
                corrupt),
            policy));
    }

    [Fact]
    public async Task ExistingServerExecutableRunsOneWorkerWithoutStartingHttp()
    {
        var policy = Policy(maximumPages: 1);
        var pdf = CreatePdf(new PageSpec(72, 36));
        var id = ContentId(pdf);
        await using var verified = new VerifiedContentObject(
            id,
            id,
            pdf.Length,
            new MemoryStream(pdf, writable: false),
            ContentVerificationOutcome.Verified);
        var renderer = CreateWorkerRenderer();

        var result = await renderer.RenderAsync(verified, policy);

        var page = Assert.Single(result.Pages);
        var validation = new PngPageImageValidator().Validate(page, policy);
        Assert.Equal((144, 72), (validation.WidthPixels, validation.HeightPixels));
        Assert.Equal(renderer.Describe(policy), result.RendererDescriptor);
    }

    [Fact]
    public async Task WorkerAndFinaliserPublishACompleteCandidateEndToEnd()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var pdf = CreatePdf(new PageSpec(72, 36), new PageSpec(36, 72));
        var document = await CommitPdfCatalogueAsync(fixture, pdf);
        var rights = new DocumentRightsEligibilityRecordV1(
            document.Id,
            document.Version,
            Enum.GetValues<DocumentRight>().Select(right => new DocumentRightDecision(
                right,
                DocumentRightDecisionState.Permitted,
                new DocumentRightsEvidenceReference($"rights-e2e-{right}"))));
        var service = new DocumentRenderCandidateService(
            fixture.ContentStore,
            CreateWorkerRenderer(),
            new PngPageImageValidator(),
            fixture.ControlStore);
        var request = new DocumentRenderCandidateRequest(
            SqlitePersistenceFixture.CorpusId,
            document.Id,
            document.Version,
            document.ContentObjectId,
            document.ByteLength,
            rights,
            Policy(),
            new DateTimeOffset(2026, 8, 7, 16, 0, 0, TimeSpan.Zero));

        var applied = await service.FinaliseAsync(request);
        var replay = await service.FinaliseAsync(request);

        Assert.Equal(StoreMutationOutcome.Applied, applied.Outcome);
        Assert.Equal(StoreMutationOutcome.AlreadyApplied, replay.Outcome);
        Assert.Equal(2, applied.Manifest.OrderedPageImages.Count);
        Assert.Equal(1, await fixture.ScalarAsync("SELECT COUNT(*) FROM document_render_manifests;"));
        Assert.Equal(2, await fixture.ScalarAsync("SELECT COUNT(*) FROM document_page_images;"));

        foreach (var page in applied.Manifest.OrderedPageImages)
        {
            await using var reopened = await fixture.ContentStore.OpenVerifiedAsync(
                page.ImageContentObjectId,
                new ExpectedHashAndLength(page.ImageContentObjectId, page.ByteLength));
            Assert.Equal(0, reopened.Content.Position);
        }
    }

    [Fact]
    public async Task NoticeBearingFinaliserPreservesSourcePixelsAndPersistsExactObligations()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var pdf = CreatePdf(new PageSpec(600, 300));
        var document = await CommitPdfCatalogueAsync(fixture, pdf);
        var rights = new DocumentRightsEligibilityRecordV1(
            document.Id,
            document.Version,
            Enum.GetValues<DocumentRight>().Select(right => new DocumentRightDecision(
                right,
                DocumentRightDecisionState.Permitted,
                new DocumentRightsEvidenceReference($"rights-notice-{right}"))));
        var obligationSet = DerivativeObligationSetV1.Create(
            rights,
            document.ContentObjectId,
            rights.Decisions.Select(decision => decision.EvidenceReference),
            DocumentContentLanguage.EnGb,
            "Synthetic Documentation Group",
            "Synthetic Database Reference",
            "1.0",
            "synthetic-source-reference-v1",
            "Synthetic Documentation Group attribution.",
            "Copyright 2026 Synthetic Documentation Group.",
            "Permission is granted for this project-owned synthetic fixture.",
            ["Synthetic fixture disclaimer one.", "Synthetic fixture disclaimer two."],
            DerivativeTrademarkTreatment.NotApplicable,
            "NotApplicable: this synthetic fixture contains no third-party trademark.",
            "Rendered derivative of the synthetic source; source pixels remain unchanged.",
            new DateTimeOffset(2026, 8, 10, 12, 0, 0, TimeSpan.Zero),
            "assessor-synthetic-v1");
        var policy = Policy(maximumPages: 1, maximumPixels: 5_000_000);
        var compositor = new NoticeBearingPageImageCompositor();
        var sourceRenderer = CreateWorkerRenderer();
        await using var source = await fixture.ContentStore.OpenVerifiedAsync(
            document.ContentObjectId,
            new ExpectedHashAndLength(document.ContentObjectId, document.ByteLength));
        var renderedSource = await sourceRenderer.RenderAsync(source, policy);
        var sourcePage = Assert.Single(renderedSource.Pages);
        var sourceValidation = new PngPageImageValidator().Validate(sourcePage, policy);
        var composed = compositor.Compose(
            sourcePage,
            sourceValidation,
            renderedSource.RendererDescriptor,
            obligationSet,
            policy);
        _ = compositor.Validate(sourcePage, sourceValidation, composed, policy);
        var service = new DocumentRenderCandidateService(
            fixture.ContentStore,
            sourceRenderer,
            new PngPageImageValidator(),
            fixture.ControlStore,
            compositor,
            compositor);
        var request = new DocumentRenderCandidateRequest(
            SqlitePersistenceFixture.CorpusId,
            document.Id,
            document.Version,
            document.ContentObjectId,
            document.ByteLength,
            rights,
            policy,
            new DateTimeOffset(2026, 8, 10, 12, 30, 0, TimeSpan.Zero),
            obligationSet);

        var applied = await service.FinaliseAsync(request);
        var replay = await service.FinaliseAsync(request);
        var persistedObligations = await fixture.ControlStore.ReadObligationSetAsync(
            SqlitePersistenceFixture.CorpusId,
            obligationSet.ObligationSetId);
        var page = Assert.Single(applied.Manifest.OrderedPageImages);

        Assert.Equal(StoreMutationOutcome.Applied, applied.Outcome);
        Assert.Equal(StoreMutationOutcome.AlreadyApplied, replay.Outcome);
        Assert.Equal(DocumentRenderManifest.NoticeBearingSchemaVersion, applied.Manifest.SchemaVersion);
        Assert.Equal(RenderProfileId.PdfPagePngNoticeV1, applied.Manifest.RenderProfileId.Value);
        Assert.Equal(obligationSet.ObligationSetId, applied.Manifest.ObligationSetId);
        Assert.Equal(obligationSet.CanonicalSha256, applied.Manifest.ObligationSetSha256);
        Assert.Equal(page.WidthPixels, page.SourceRegionWidthPixels);
        Assert.Equal(page.HeightPixels, page.SourceRegionHeightPixels + page.NoticeRegionHeightPixels);
        Assert.NotNull(persistedObligations);
        Assert.Equal(
            obligationSet.SerialiseCanonicalUtf8(),
            persistedObligations.SerialiseCanonicalUtf8());
        Assert.Equal(1, await fixture.ScalarAsync("SELECT COUNT(*) FROM derivative_obligation_sets;"));
        Assert.Equal(10, await fixture.ScalarAsync("SELECT COUNT(*) FROM derivative_obligation_evidence_references;"));
        Assert.Equal(2, await fixture.ScalarAsync("SELECT COUNT(*) FROM derivative_obligation_disclaimers;"));

        await using var composite = await fixture.ContentStore.OpenVerifiedAsync(
            page.ImageContentObjectId,
            new ExpectedHashAndLength(page.ImageContentObjectId, page.ByteLength));
        using var compositeBytes = new MemoryStream();
        await composite.Content.CopyToAsync(compositeBytes);
        var validation = compositor.Validate(
            sourcePage,
            sourceValidation,
            new NoticeBearingPageCandidate(
                page.PageNumber,
                compositeBytes.ToArray(),
                page.SourceRegionWidthPixels!.Value,
                page.SourceRegionHeightPixels!.Value,
                page.NoticeRegionHeightPixels!.Value,
                page.RendererDescriptor),
            policy);
        Assert.Equal(page.ImageSha256.Value, validation.Sha256.Value);

        var cleanup = await new SqliteStorageMaintenance(fixture.Options).RunManualCleanupAsync(
            new OperationId("cleanup-notice-bearing-reachability"),
            SqlitePersistenceFixture.CorpusId,
            new DateTimeOffset(2026, 9, 10, 12, 0, 0, TimeSpan.Zero));
        Assert.Equal(0, cleanup.RemovedContentObjects);
        await using var preserved = await fixture.ContentStore.OpenVerifiedAsync(
            page.ImageContentObjectId,
            new ExpectedHashAndLength(page.ImageContentObjectId, page.ByteLength));
        Assert.Equal(page.ByteLength, preserved.ByteLength);
    }

    [Fact]
    public async Task DistinctCitedPageSelectionsPersistAsIndependentSparseManifests()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var pdf = CreatePdf(new PageSpec(600, 300), new PageSpec(600, 300));
        var document = await CommitPdfCatalogueAsync(fixture, pdf);
        var rights = new DocumentRightsEligibilityRecordV1(
            document.Id,
            document.Version,
            Enum.GetValues<DocumentRight>().Select(right => new DocumentRightDecision(
                right,
                DocumentRightDecisionState.Permitted,
                new DocumentRightsEvidenceReference($"rights-selection-{right}"))));
        var obligationSet = DerivativeObligationSetV1.Create(
            rights,
            document.ContentObjectId,
            rights.Decisions.Select(decision => decision.EvidenceReference),
            DocumentContentLanguage.EnGb,
            "Synthetic Documentation Group",
            "Synthetic Database Reference",
            "1.0",
            "synthetic-source-selection-v1",
            "Synthetic attribution.",
            "Copyright 2026 Synthetic Documentation Group.",
            "Permission is granted for this project-owned synthetic fixture.",
            ["Synthetic fixture disclaimer."],
            DerivativeTrademarkTreatment.NotApplicable,
            "NotApplicable: this synthetic fixture contains no third-party trademark.",
            "Rendered derivative of the synthetic source; source pixels remain unchanged.",
            new DateTimeOffset(2026, 8, 10, 12, 0, 0, TimeSpan.Zero),
            "assessor-synthetic-v1");
        var policy = Policy(maximumPages: 2, maximumPixels: 5_000_000);
        var compositor = new NoticeBearingPageImageCompositor();
        var service = new DocumentRenderCandidateService(
            fixture.ContentStore,
            CreateWorkerRenderer(),
            new PngPageImageValidator(),
            fixture.ControlStore,
            compositor,
            compositor);

        var first = await service.FinaliseAsync(new DocumentRenderCandidateRequest(
            SqlitePersistenceFixture.CorpusId,
            document.Id,
            document.Version,
            document.ContentObjectId,
            document.ByteLength,
            rights,
            policy,
            new DateTimeOffset(2026, 8, 10, 13, 0, 0, TimeSpan.Zero),
            obligationSet,
            [1]));
        var second = await service.FinaliseAsync(new DocumentRenderCandidateRequest(
            SqlitePersistenceFixture.CorpusId,
            document.Id,
            document.Version,
            document.ContentObjectId,
            document.ByteLength,
            rights,
            policy,
            new DateTimeOffset(2026, 8, 10, 13, 1, 0, TimeSpan.Zero),
            obligationSet,
            [2]));

        Assert.Equal(StoreMutationOutcome.Applied, first.Outcome);
        Assert.Equal(StoreMutationOutcome.Applied, second.Outcome);
        Assert.False(first.Manifest.IsComplete);
        Assert.False(second.Manifest.IsComplete);
        Assert.Equal(1, Assert.Single(first.Manifest.OrderedPageImages).PageNumber);
        Assert.Equal(2, Assert.Single(second.Manifest.OrderedPageImages).PageNumber);
        Assert.Equal(2, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM document_render_manifests;"));
        Assert.Equal(2, await fixture.ScalarAsync(
            "SELECT COUNT(*) FROM document_page_images;"));
    }

    [Fact]
    public async Task AdministrativeRenderPersistsBeforeProjectingExactActivationPlan()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var pdf = CreatePdf(new PageSpec(600, 300));
        var document = await CommitPdfCatalogueAsync(fixture, pdf);
        var rights = new DocumentRightsEligibilityRecordV1(
            document.Id,
            document.Version,
            Enum.GetValues<DocumentRight>().Select(right => new DocumentRightDecision(
                right,
                DocumentRightDecisionState.Permitted,
                new DocumentRightsEvidenceReference($"rights-admin-{right}"))));
        var obligationSet = DerivativeObligationSetV1.Create(
            rights,
            document.ContentObjectId,
            rights.Decisions.Select(decision => decision.EvidenceReference),
            DocumentContentLanguage.EnGb,
            "Synthetic Documentation Group",
            "Synthetic Database Reference",
            "1.0",
            "synthetic-source-reference-v1",
            "Synthetic Documentation Group attribution.",
            "Copyright 2026 Synthetic Documentation Group.",
            "Permission is granted for this project-owned synthetic fixture.",
            ["Synthetic fixture disclaimer."],
            DerivativeTrademarkTreatment.NotApplicable,
            "NotApplicable: this synthetic fixture contains no third-party trademark.",
            "Rendered derivative of the synthetic source; source pixels remain unchanged.",
            new DateTimeOffset(2026, 8, 10, 12, 0, 0, TimeSpan.Zero),
            "assessor-synthetic-v1");
        var policy = Policy(maximumPages: 1, maximumPixels: 5_000_000);
        var compositor = new NoticeBearingPageImageCompositor();
        var service = new DocumentRenderCandidateService(
            fixture.ContentStore,
            CreateWorkerRenderer(),
            new PngPageImageValidator(),
            fixture.ControlStore,
            compositor,
            compositor);
        var input = JsonSerializer.SerializeToElement(new
        {
            documentId = document.Id.Value,
            documentVersion = document.Version.Value,
            sourceContentObjectId = document.ContentObjectId.Value,
            sourceByteLength = document.ByteLength,
            generatedAt = new DateTimeOffset(2026, 8, 10, 12, 30, 0, TimeSpan.Zero),
            rights = new
            {
                rightsSchemaVersion = rights.SchemaVersion,
                rightsDecisions = rights.Decisions.Select(decision => new
                {
                    right = decision.Right.ToString(),
                    state = decision.State.ToString(),
                    evidenceReference = decision.EvidenceReference.Value,
                }),
                documentId = document.Id.Value,
                documentVersion = document.Version.Value,
            },
            renderPolicy = new
            {
                maximumSourceByteLength = policy.MaximumSourceByteLength,
                maximumPageCount = policy.MaximumPageCount,
                maximumTotalPixels = policy.MaximumTotalPixels,
                maximumPageOutputByteLength = policy.MaximumPageOutputByteLength,
                maximumTotalOutputByteLength = policy.MaximumTotalOutputByteLength,
                maximumWorkerMemoryBytes = policy.MaximumWorkerMemoryBytes,
                maximumWorkerCpuMilliseconds = (long)policy.MaximumWorkerCpuTime.TotalMilliseconds,
                workerTimeoutMilliseconds = (long)policy.WorkerTimeout.TotalMilliseconds,
            },
            obligationSet = new
            {
                schemaVersion = obligationSet.SchemaVersion,
                expectedObligationSetId = obligationSet.ObligationSetId.Value,
                expectedCanonicalSha256 = obligationSet.CanonicalSha256.Value,
                expectedRightsMappingRevision = obligationSet.RightsMappingRevision.Value,
                orderedEvidenceReferences = obligationSet.OrderedEvidenceReferences.Select(item => item.Value),
                contentLanguage = obligationSet.ContentLanguage.ToCanonicalTag(),
                authoritativePublisherOrAuthor = obligationSet.AuthoritativePublisherOrAuthor,
                documentTitle = obligationSet.DocumentTitle,
                documentVersionLabel = obligationSet.DocumentVersionLabel,
                sourceReference = obligationSet.SourceReference,
                attributionText = obligationSet.AttributionText,
                copyrightNotice = obligationSet.CopyrightNotice,
                permissionNotice = obligationSet.PermissionNotice,
                orderedDisclaimers = obligationSet.OrderedDisclaimers,
                trademarkTreatment = obligationSet.TrademarkTreatment.ToString(),
                trademarkOrNonEndorsementText = obligationSet.TrademarkOrNonEndorsementText,
                changeMarkingText = obligationSet.ChangeMarkingText,
                assessedAt = obligationSet.AssessedAt,
                assessorId = obligationSet.AssessorId,
            },
        });
        var operationId = new OperationId("admin-notice-render");
        var command = new OneShotAdministrativeCommand(
            "render-document",
            SqlitePersistenceFixture.CorpusId,
            operationId,
            new AdministrativeAuditContext(
                operationId,
                "os-sha256:" + new string('a', 64),
                "render-document",
                "Render a bounded synthetic notice-bearing fixture.",
                new DateTimeOffset(2026, 8, 10, 12, 30, 0, TimeSpan.Zero)),
            input,
            InputSha256: null,
            JournalIntentDigest: "synthetic-intent");

        var rendered = await new RenderDocumentAdministrativeCommand(service)
            .ExecuteAsync(command);
        var renderManifestId = new RenderManifestId(
            rendered.ResultPayload!.Value.GetProperty("renderManifestId").GetString()!);
        var binding = new DocumentBinding(
            new DatabaseProductId("synthetic-product"),
            new DatabaseProductRevision(1),
            document.Id,
            document.Version,
            DocumentFormat.Pdf,
            new SourceAdapterId("synthetic-local-pdf"),
            SourceTrustClass.LocalAuthorised);
        var evidence = new DocumentRenderManifestProjectionPayload
        {
            DocumentId = document.Id.Value,
            DocumentVersion = document.Version.Value,
            RenderManifestId = renderManifestId.Value,
        };
        var projector = new AdministrativeActivationPlanProjector(fixture.ControlStore);
        var projection = await projector.ValidateAsync(
            SqlitePersistenceFixture.CorpusId,
            [new ActivationProjectionDocument(binding, document.ContentObjectId, rights)],
            new ActivationPlanProjectionPayload
            {
                ExpectedCurrentRevision = 0,
                PreviousGenerationRetentionDays = 14,
                DocumentRenderManifests = [evidence],
            },
            CancellationToken.None);
        var contentDigest = new string('b', 64);
        var manifest = new FinalisedIndexGenerationManifest(
            1,
            SqlitePersistenceFixture.CorpusId,
            new CorpusRevision(1),
            new CatalogueRevision(1),
            new ActiveDocumentSetDigest(new string('c', 64)),
            new SourceBindingSetDigest(new string('d', 64)),
            new IndexCompatibilityKey(new string('e', 64)),
            new GenerationSpecDigest(new string('f', 64)),
            1,
            1,
            new LogicalArtifactDigest(new string('1', 64)),
            new GenerationContentDigest(contentDigest),
            new IndexGenerationId("idxgen-" + contentDigest));
        var projected = AdministrativeActivationPlanProjector.Project(manifest, projection);
        var projectedPlan = projected.GetProperty("activationPlan");
        var describedIntent = new SqliteAdministrativeCommandExecutor(fixture.ControlStore)
            .DescribeIntent(
                "activate-generation",
                SqlitePersistenceFixture.CorpusId,
                projectedPlan);

        Assert.Equal(AdministrativeExecutionOutcome.Applied, rendered.Outcome);
        Assert.Contains(
            $"generation:{manifest.IndexGenerationId.Value}",
            describedIntent.TargetIdentifiers);
        Assert.Equal(
            renderManifestId.Value,
            projectedPlan
                .GetProperty("evidenceBindings")[0]
                .GetProperty("renderManifestId").GetString());

        var invalidEvidence = new DocumentRenderManifestProjectionPayload
        {
            DocumentId = document.Id.Value,
            DocumentVersion = document.Version.Value,
            RenderManifestId = "rendermanifest-" + new string('0', 64),
        };
        await Assert.ThrowsAsync<InvalidDataException>(() => projector.ValidateAsync(
            SqlitePersistenceFixture.CorpusId,
            [new ActivationProjectionDocument(binding, document.ContentObjectId, rights)],
            new ActivationPlanProjectionPayload
            {
                ExpectedCurrentRevision = 0,
                PreviousGenerationRetentionDays = 14,
                DocumentRenderManifests = [invalidEvidence],
            },
            CancellationToken.None));
    }

    [Fact]
    public void NoticeBearingCompositorFailsClosedWhenTheProjectOwnedGlyphSetCannotRepresentText()
    {
        var policy = Policy(maximumPages: 1, maximumPixels: 5_000_000);
        var rendered = PdfToImagePdfPageRenderer.Render(
            CreatePdf(new PageSpec(600, 300)),
            policy,
            RuntimeInformation.RuntimeIdentifier);
        var page = Assert.Single(rendered.Pages);
        var validation = new PngPageImageValidator().Validate(page, policy);
        var documentId = new DocumentId("document-unsupported-glyph");
        var documentVersion = new DocumentVersionNumber(1);
        var rights = new DocumentRightsEligibilityRecordV1(
            documentId,
            documentVersion,
            Enum.GetValues<DocumentRight>().Select(right => new DocumentRightDecision(
                right,
                DocumentRightDecisionState.Permitted,
                new DocumentRightsEvidenceReference($"rights-glyph-{right}"))));
        var obligationSet = DerivativeObligationSetV1.Create(
            rights,
            new ContentObjectId(new string('a', 64)),
            rights.Decisions.Select(decision => decision.EvidenceReference),
            DocumentContentLanguage.EnGb,
            "Synthétic Publisher".Normalize(NormalizationForm.FormC),
            "Synthetic Reference",
            "1.0",
            "synthetic-source-v1",
            "Synthetic attribution.",
            "Synthetic copyright notice.",
            "Synthetic permission notice.",
            [],
            DerivativeTrademarkTreatment.NotApplicable,
            "NotApplicable: no trademark applies.",
            "Rendered synthetic derivative.",
            new DateTimeOffset(2026, 8, 10, 12, 0, 0, TimeSpan.Zero),
            "assessor-synthetic-v1");
        var compositor = new NoticeBearingPageImageCompositor();

        var exception = Assert.Throws<PdfRenderException>(() => compositor.Compose(
            page,
            validation,
            rendered.RendererDescriptor,
            obligationSet,
            policy));

        Assert.Equal(PdfRenderFailureKind.InvalidPageImage, exception.FailureKind);
    }

    [Fact]
    public void NoticeBearingCompositorDeterministicallySupportsTheExactNoticeGlyphExtension()
    {
        var policy = Policy(maximumPages: 1, maximumPixels: 5_000_000);
        var rendered = PdfToImagePdfPageRenderer.Render(
            CreatePdf(new PageSpec(600, 300)),
            policy,
            RuntimeInformation.RuntimeIdentifier);
        var page = Assert.Single(rendered.Pages);
        var validation = new PngPageImageValidator().Validate(page, policy);
        var documentId = new DocumentId("document-exact-notice-glyphs");
        var documentVersion = new DocumentVersionNumber(1);
        var rights = new DocumentRightsEligibilityRecordV1(
            documentId,
            documentVersion,
            Enum.GetValues<DocumentRight>().Select(right => new DocumentRightDecision(
                right,
                DocumentRightDecisionState.Permitted,
                new DocumentRightsEvidenceReference($"rights-exact-glyph-{right}"))));
        var obligationSet = DerivativeObligationSetV1.Create(
            rights,
            new ContentObjectId(new string('b', 64)),
            rights.Decisions.Select(decision => decision.EvidenceReference),
            DocumentContentLanguage.EnGb,
            "Synthetic Publisher © 2026",
            "Synthetic Reference – Revision",
            "1.0",
            "synthetic-source-v1",
            "Synthetic “quoted” attribution.",
            "Copyright © 2026 Synthetic Publisher.",
            "Synthetic permission notice.",
            ["Synthetic disclaimer on an “AS-IS” basis."],
            DerivativeTrademarkTreatment.NotApplicable,
            "NotApplicable: no trademark applies.",
            "Rendered synthetic derivative – source pixels unchanged.",
            new DateTimeOffset(2026, 8, 12, 12, 0, 0, TimeSpan.Zero),
            "assessor-synthetic-v1");
        var firstCompositor = new NoticeBearingPageImageCompositor();
        var secondCompositor = new NoticeBearingPageImageCompositor();

        var first = firstCompositor.Compose(
            page,
            validation,
            rendered.RendererDescriptor,
            obligationSet,
            policy);
        var repeat = secondCompositor.Compose(
            page,
            validation,
            rendered.RendererDescriptor,
            obligationSet,
            policy);
        var verified = firstCompositor.Validate(page, validation, first, policy);

        Assert.Equal(firstCompositor.FontAssetSha256, secondCompositor.FontAssetSha256);
        Assert.Matches("^[0-9a-f]{64}$", firstCompositor.FontAssetSha256);
        Assert.Equal(first.RendererDescriptor, repeat.RendererDescriptor);
        Assert.Equal(first.PngBytes.ToArray(), repeat.PngBytes.ToArray());
        Assert.Equal(first.SourceRegionWidthPixels, verified.SourceRegionWidthPixels);
        Assert.Equal(first.SourceRegionHeightPixels, verified.SourceRegionHeightPixels);
        Assert.Equal(first.NoticeRegionHeightPixels, verified.NoticeRegionHeightPixels);
    }

    [Fact]
    public async Task WorkerBoundaryFailsClosedForCancellationCrashAndTruncatedFrames()
    {
        var policy = Policy(maximumPages: 1);
        var pdf = CreatePdf(new PageSpec(72, 36));
        var id = ContentId(pdf);
        await using var cancelledSource = new VerifiedContentObject(
            id,
            id,
            pdf.Length,
            new MemoryStream(pdf, writable: false),
            ContentVerificationOutcome.Verified);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var cancelled = await Assert.ThrowsAsync<PdfRenderException>(() =>
            CreateWorkerRenderer().RenderAsync(cancelledSource, policy, cancellation.Token));
        Assert.Equal(PdfRenderFailureKind.Cancelled, cancelled.FailureKind);

        await using var crashSource = new VerifiedContentObject(
            id,
            id,
            pdf.Length,
            new MemoryStream(pdf, writable: false),
            ContentVerificationOutcome.Verified);
        var crashing = new IsolatedPdfRendererProcess(new RendererWorkerLaunch(
            DotnetHostPath(),
            [Path.Combine(Path.GetTempPath(), "missing-render-worker.dll")],
            RuntimeInformation.RuntimeIdentifier));
        await Assert.ThrowsAsync<PdfRenderException>(() =>
            crashing.RenderAsync(crashSource, policy));

        await Assert.ThrowsAsync<EndOfStreamException>(() =>
            PdfRenderWorkerProtocol.ReadResponseAsync(
                new MemoryStream(new byte[] { 1, 2, 3 }),
                policy,
                CancellationToken.None));
        await Assert.ThrowsAsync<PdfRenderException>(() =>
            PdfRenderWorkerProtocol.ReadResponseAsync(
                new MemoryStream(new byte[12]),
                policy,
                CancellationToken.None));
        using var output = new MemoryStream();
        var exitCode = await PdfRenderWorker.RunAsync(
            new MemoryStream(new byte[] { 1, 2, 3 }),
            output);
        Assert.NotEqual(0, exitCode);
        Assert.Equal(12, output.Length);
    }

    [Fact]
    public async Task WorkerMemoryLimitFailsClosedBeforeRenderingCompletes()
    {
        var policy = new PdfRenderPolicy(
            maximumSourceByteLength: 1_000_000,
            maximumPageCount: 1,
            maximumTotalPixels: 100_000,
            maximumPageOutputByteLength: 1_000_000,
            maximumTotalOutputByteLength: 1_000_000,
            maximumWorkerMemoryBytes: 1_000_000,
            maximumWorkerCpuTime: TimeSpan.FromSeconds(5),
            workerTimeout: TimeSpan.FromSeconds(10));
        var pdf = CreatePdf(new PageSpec(72, 36));
        var id = ContentId(pdf);
        await using var source = new VerifiedContentObject(
            id,
            id,
            pdf.Length,
            new MemoryStream(pdf, writable: false),
            ContentVerificationOutcome.Verified);

        await Assert.ThrowsAsync<PdfRenderException>(() =>
            CreateWorkerRenderer().RenderAsync(source, policy));
    }

    [Fact]
    public async Task ManifestCommitIsAtomicReadableIdempotentAndSupportsIndependentSelections()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var catalogue = await fixture.CommitLocalCatalogueAsync("synthetic source bytes");
        var document = Assert.Single(catalogue.Snapshot.DocumentVersions);
        var renderer = PdfPagePngV1RendererIdentity.CreateDescriptor(Policy(), "win-x64");
        var first = Manifest(document, renderer, new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 });

        var applied = await fixture.ControlStore.CommitAsync(
            new RenderManifestCommitRequest(SqlitePersistenceFixture.CorpusId, first));
        var replay = await fixture.ControlStore.CommitAsync(
            new RenderManifestCommitRequest(SqlitePersistenceFixture.CorpusId, first));
        var readback = await fixture.ControlStore.ReadAsync(
            SqlitePersistenceFixture.CorpusId,
            first.RenderManifestId);
        var independentManifest = Manifest(
            document,
            renderer,
            new byte[] { 7, 8, 9 },
            new byte[] { 10, 11, 12 });
        var independent = await fixture.ControlStore.CommitAsync(
            new RenderManifestCommitRequest(
                SqlitePersistenceFixture.CorpusId,
                independentManifest));

        Assert.Equal(StoreMutationOutcome.Applied, applied.Outcome);
        Assert.Equal(StoreMutationOutcome.AlreadyApplied, replay.Outcome);
        Assert.Equal(StoreMutationOutcome.Applied, independent.Outcome);
        Assert.NotNull(readback);
        Assert.Equal(first.ManifestSha256, readback.ManifestSha256);
        Assert.Equal(2, await fixture.ScalarAsync("SELECT COUNT(*) FROM document_render_manifests;"));
        Assert.Equal(4, await fixture.ScalarAsync("SELECT COUNT(*) FROM document_page_images;"));
    }

    [Fact]
    public async Task ManifestFailureLeavesNoPartialManifestOrPageBindings()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var catalogue = await fixture.CommitLocalCatalogueAsync("synthetic source bytes");
        var document = Assert.Single(catalogue.Snapshot.DocumentVersions);
        var renderer = PdfPagePngV1RendererIdentity.CreateDescriptor(Policy(), "win-x64");
        var repeatedImage = ContentId(new byte[] { 1, 2, 3 });
        var pages = new[]
        {
            Page(document, renderer, repeatedImage, pageNumber: 1, byteLength: 3),
            Page(document, renderer, repeatedImage, pageNumber: 2, byteLength: 4),
        };
        var invalidPersistenceCandidate = DocumentRenderManifest.Create(
            document.Id,
            document.Version,
            document.ContentObjectId,
            sourcePageCount: 2,
            new RenderProfileId(RenderProfileId.PdfPagePngV1),
            renderer,
            pages,
            new DateTimeOffset(2026, 8, 7, 15, 30, 0, TimeSpan.Zero));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fixture.ControlStore.CommitAsync(new RenderManifestCommitRequest(
                SqlitePersistenceFixture.CorpusId,
                invalidPersistenceCandidate)));
        Assert.Equal(0, await fixture.ScalarAsync("SELECT COUNT(*) FROM document_render_manifests;"));
        Assert.Equal(0, await fixture.ScalarAsync("SELECT COUNT(*) FROM document_page_images;"));
    }

    private static DocumentRenderManifest Manifest(
        DocumentVersion document,
        RendererDescriptor renderer,
        params byte[][] images)
    {
        var pages = images.Select((bytes, index) =>
        {
            var imageId = ContentId(bytes);
            return new DocumentPageImage(
                document.Id,
                document.Version,
                document.ContentObjectId,
                index + 1,
                new RenderProfileId(RenderProfileId.PdfPagePngV1),
                renderer,
                imageId,
                new ImageSha256(imageId.Value),
                bytes.Length,
                DocumentPageImage.PngMediaType,
                72,
                48);
        });
        return DocumentRenderManifest.Create(
            document.Id,
            document.Version,
            document.ContentObjectId,
            images.Length,
            new RenderProfileId(RenderProfileId.PdfPagePngV1),
            renderer,
            pages,
            new DateTimeOffset(2026, 8, 7, 15, 0, 0, TimeSpan.Zero));
    }

    private static DocumentPageImage Page(
        DocumentVersion document,
        RendererDescriptor renderer,
        ContentObjectId imageId,
        int pageNumber,
        long byteLength) =>
        new(
            document.Id,
            document.Version,
            document.ContentObjectId,
            pageNumber,
            new RenderProfileId(RenderProfileId.PdfPagePngV1),
            renderer,
            imageId,
            new ImageSha256(imageId.Value),
            byteLength,
            DocumentPageImage.PngMediaType,
            72,
            48);

    private static async Task<DocumentVersion> CommitPdfCatalogueAsync(
        SqlitePersistenceFixture fixture,
        byte[] pdf)
    {
        await using var source = new MemoryStream(pdf, writable: false);
        var content = await fixture.ContentStore.PutAndVerifyAsync(new BoundedContentInput(
            source,
            pdf.Length,
            ContentMediaType.ApplicationPdf));
        var category = new DatabaseCategory(
            new DatabaseCategoryId("category-render"),
            "Synthetic render category");
        var product = new DatabaseProduct(
            new DatabaseProductId("db-render"),
            new DatabaseProductRevision(1),
            "Synthetic render database",
            CatalogueItemStatus.Active,
            [category.Id]);
        var document = new DocumentVersion(
            new DocumentId("document-render-e2e"),
            new DocumentVersionNumber(1),
            product.Id,
            product.Revision,
            DocumentFormat.Pdf,
            DocumentContentLanguage.EnGb,
            CatalogueItemStatus.Active,
            content.ContentObjectId,
            content.ByteLength,
            ContentMediaType.ApplicationPdf.Value,
            new SourceAdapterId("local-render-synthetic"),
            SourceTrustClass.LocalAuthorised);
        var snapshot = new CatalogueSnapshot(
            SqlitePersistenceFixture.CorpusId,
            new CatalogueRevision(1),
            [category],
            [product],
            [document]);
        var committed = await fixture.ControlStore.CommitCatalogueAsync(
            new CatalogueCommitRequest(
                new OperationId("catalogue-render-e2e"),
                snapshot,
                ExpectedCurrentRevision: 0,
                new DateTimeOffset(2026, 8, 7, 14, 0, 0, TimeSpan.Zero)));
        Assert.Equal(StoreMutationOutcome.Applied, committed.Outcome);
        return document;
    }

    private static PdfRenderPolicy Policy(
        int maximumPages = 2,
        long maximumPixels = 200_000) =>
        new(
            maximumSourceByteLength: 1_000_000,
            maximumPageCount: maximumPages,
            maximumTotalPixels: maximumPixels,
            maximumPageOutputByteLength: 1_000_000,
            maximumTotalOutputByteLength: 2_000_000,
            maximumWorkerMemoryBytes: 768L * 1024 * 1024,
            maximumWorkerCpuTime: TimeSpan.FromSeconds(20),
            workerTimeout: TimeSpan.FromSeconds(30));

    private static string DotnetHostPath()
    {
        var configured = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH");

        if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured))
        {
            return configured;
        }

        var runtimeDirectory = new DirectoryInfo(RuntimeEnvironment.GetRuntimeDirectory());
        var root = runtimeDirectory.Parent?.Parent?.Parent?.FullName ??
            throw new InvalidOperationException("The .NET host root could not be resolved.");
        var candidate = Path.Combine(root, OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet");
        return File.Exists(candidate)
            ? candidate
            : throw new InvalidOperationException("The .NET host could not be resolved.");
    }

    private static IsolatedPdfRendererProcess CreateWorkerRenderer() =>
        new(new RendererWorkerLaunch(
            DotnetHostPath(),
            [typeof(Program).Assembly.Location],
            RuntimeInformation.RuntimeIdentifier));

    private static RenderedPdfPageCandidate ReplaceIhdrColourType(
        RenderedPdfPageCandidate candidate,
        byte colourType)
    {
        var bytes = candidate.PngBytes.ToArray();
        bytes[25] = colourType;
        var crc = Crc32(bytes.AsSpan(12, 4), bytes.AsSpan(16, 13));
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(29, 4), crc);
        return new RenderedPdfPageCandidate(
            candidate.PageNumber,
            candidate.SourceWidthPoints,
            candidate.SourceHeightPoints,
            bytes);
    }

    private static RenderedPdfPageCandidate InsertChunk(
        RenderedPdfPageCandidate candidate,
        string type,
        byte[] data)
    {
        var original = candidate.PngBytes.ToArray();
        var iendOffset = original.Length - 12;
        var inserted = new byte[original.Length + 12 + data.Length];
        original.AsSpan(0, iendOffset).CopyTo(inserted);
        BinaryPrimitives.WriteUInt32BigEndian(inserted.AsSpan(iendOffset), (uint)data.Length);
        Encoding.ASCII.GetBytes(type).CopyTo(inserted, iendOffset + 4);
        data.CopyTo(inserted, iendOffset + 8);
        BinaryPrimitives.WriteUInt32BigEndian(
            inserted.AsSpan(iendOffset + 8 + data.Length),
            Crc32(Encoding.ASCII.GetBytes(type), data));
        original.AsSpan(iendOffset).CopyTo(inserted.AsSpan(iendOffset + 12 + data.Length));
        return new RenderedPdfPageCandidate(
            candidate.PageNumber,
            candidate.SourceWidthPoints,
            candidate.SourceHeightPoints,
            inserted);
    }

    private static (byte BitDepth, byte ColourType) ReadIhdr(ReadOnlySpan<byte> png) =>
        (png[24], png[25]);

    private static uint Crc32(ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
    {
        var crc = uint.MaxValue;

        foreach (var value in type)
        {
            crc = UpdateCrc(crc, value);
        }

        foreach (var value in data)
        {
            crc = UpdateCrc(crc, value);
        }

        return ~crc;
    }

    private static uint UpdateCrc(uint crc, byte value)
    {
        crc ^= value;

        for (var bit = 0; bit < 8; bit++)
        {
            crc = (crc & 1) == 0 ? crc >> 1 : (crc >> 1) ^ 0xedb88320u;
        }

        return crc;
    }

    private static ContentObjectId ContentId(ReadOnlySpan<byte> bytes) =>
        new(Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());

    private static byte[] CreatePdf(params PageSpec[] pages)
    {
        var objects = new SortedDictionary<int, string>();
        var pageIds = Enumerable.Range(0, pages.Length).Select(index => 3 + (index * 2)).ToArray();
        var nextObject = 3 + (pages.Length * 2);
        var widgetIds = new List<int>();
        var pageBodies = new Dictionary<int, string>();

        for (var index = 0; index < pages.Length; index++)
        {
            var spec = pages[index];
            var pageId = pageIds[index];
            var contentId = pageId + 1;
            var rotation = spec.Rotation == 0 ? string.Empty : $" /Rotate {spec.Rotation}";
            var annotation = string.Empty;

            if (spec.WithWidgetAppearance)
            {
                var widgetId = nextObject++;
                var appearanceId = nextObject++;
                widgetIds.Add(widgetId);
                annotation = $" /Annots [{widgetId} 0 R]";
                var drawing = string.Create(
                    CultureInfo.InvariantCulture,
                    $"1 0 0 rg 0 0 {spec.WidthPoints:0.###} {spec.HeightPoints:0.###} re f");
                objects[appearanceId] = string.Create(
                    CultureInfo.InvariantCulture,
                    $"<< /Type /XObject /Subtype /Form /BBox [0 0 {spec.WidthPoints:0.###} {spec.HeightPoints:0.###}] /Resources << >> /Length {Encoding.ASCII.GetByteCount(drawing)} >>\nstream\n{drawing}\nendstream");
                objects[widgetId] = string.Create(
                    CultureInfo.InvariantCulture,
                    $"<< /Type /Annot /Subtype /Widget /FT /Tx /T (synthetic) /Rect [0 0 {spec.WidthPoints:0.###} {spec.HeightPoints:0.###}] /AP << /N {appearanceId} 0 R >> /P {pageId} 0 R >>");
            }

            pageBodies[pageId] = string.Create(
                CultureInfo.InvariantCulture,
                $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {spec.WidthPoints:0.###} {spec.HeightPoints:0.###}]{rotation}{annotation} /Resources << >> /Contents {contentId} 0 R >>");
            const string content = "q Q";
            objects[contentId] = $"<< /Length {content.Length} >>\nstream\n{content}\nendstream";
        }

        objects[1] = widgetIds.Count == 0
            ? "<< /Type /Catalog /Pages 2 0 R >>"
            : $"<< /Type /Catalog /Pages 2 0 R /AcroForm << /Fields [{string.Join(' ', widgetIds.Select(id => $"{id} 0 R"))}] >> >>";
        objects[2] = $"<< /Type /Pages /Kids [{string.Join(' ', pageIds.Select(id => $"{id} 0 R"))}] /Count {pages.Length} >>";

        foreach (var page in pageBodies)
        {
            objects[page.Key] = page.Value;
        }

        using var stream = new MemoryStream();
        WriteAscii(stream, "%PDF-1.7\n");
        var maximumId = objects.Keys.Max();
        var offsets = new long[maximumId + 1];

        for (var id = 1; id <= maximumId; id++)
        {
            offsets[id] = stream.Position;
            WriteAscii(stream, $"{id} 0 obj\n{objects[id]}\nendobj\n");
        }

        var xref = stream.Position;
        WriteAscii(stream, $"xref\n0 {maximumId + 1}\n0000000000 65535 f \n");

        for (var id = 1; id <= maximumId; id++)
        {
            WriteAscii(stream, offsets[id].ToString("D10", CultureInfo.InvariantCulture) + " 00000 n \n");
        }

        WriteAscii(
            stream,
            $"trailer\n<< /Size {maximumId + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
        return stream.ToArray();
    }

    private static void WriteAscii(Stream stream, string value) =>
        stream.Write(Encoding.ASCII.GetBytes(value));

    private sealed record PageSpec(
        double WidthPoints,
        double HeightPoints,
        int Rotation = 0,
        bool WithWidgetAppearance = false);

    private sealed class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        internal static ByteArrayComparer Instance { get; } = new();

        public bool Equals(byte[]? left, byte[]? right) =>
            left is not null && right is not null && left.AsSpan().SequenceEqual(right);

        public int GetHashCode(byte[] value) => value.Length;
    }
}
