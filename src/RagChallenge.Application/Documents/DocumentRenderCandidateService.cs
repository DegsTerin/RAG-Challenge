// Purpose: Finalises a complete deterministic PDF render candidate only after rights, source, page-image and durable readback verification; activation, serving and cleanup remain separate authorities.
using System.Collections.ObjectModel;

using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Application.Documents;

public enum DocumentRenderCandidateFailureKind
{
    RightsIneligible,
    SourceVerificationFailed,
    RendererFailed,
    IncompleteRendererOutput,
    PageImageInvalid,
    ContentPublicationFailed,
    ManifestPersistenceConflict,
    ManifestReadbackFailed,
    Cancelled,
}

public sealed class DocumentRenderCandidateException : Exception
{
    public DocumentRenderCandidateException(DocumentRenderCandidateFailureKind failureKind)
        : base($"Render-candidate finalisation failed with the sanitised outcome '{failureKind}'.")
    {
        if (!Enum.IsDefined(failureKind))
        {
            throw new ArgumentOutOfRangeException(nameof(failureKind));
        }

        FailureKind = failureKind;
    }

    public DocumentRenderCandidateFailureKind FailureKind { get; }
}

public sealed class DocumentRenderCandidateRequest
{
    public DocumentRenderCandidateRequest(
        CorpusId corpusId,
        DocumentId documentId,
        DocumentVersionNumber documentVersion,
        ContentObjectId sourceContentObjectId,
        long sourceByteLength,
        DocumentRightsEligibilityRecordV1 rights,
        PdfRenderPolicy policy,
        DateTimeOffset generatedAt,
        DerivativeObligationSetV1? obligationSet = null,
        IEnumerable<int>? pageNumbers = null)
    {
        ArgumentNullException.ThrowIfNull(corpusId);
        ArgumentNullException.ThrowIfNull(documentId);
        ArgumentNullException.ThrowIfNull(documentVersion);
        ArgumentNullException.ThrowIfNull(sourceContentObjectId);
        ArgumentNullException.ThrowIfNull(rights);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sourceByteLength);

        if (rights.DocumentId != documentId || rights.DocumentVersion != documentVersion)
        {
            throw new ArgumentException(
                "The rights record must bind the exact render-candidate document version.",
                nameof(rights));
        }

        if (obligationSet is not null &&
            (obligationSet.DocumentId != documentId ||
             obligationSet.DocumentVersion != documentVersion ||
             obligationSet.SourceContentObjectId != sourceContentObjectId ||
             !obligationSet.MatchesRights(rights)))
        {
            throw new ArgumentException(
                "The obligation set must bind the exact source and ten-decision rights mapping.",
                nameof(obligationSet));
        }

        if (generatedAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Render-candidate generation time must be expressed in UTC.",
                nameof(generatedAt));
        }

        var selectedPages = pageNumbers?.Order().ToArray();
        if (selectedPages is not null &&
            (obligationSet is null || selectedPages.Length == 0 ||
             selectedPages.Any(pageNumber => pageNumber <= 0) ||
             selectedPages.Distinct().Count() != selectedPages.Length))
        {
            throw new ArgumentException(
                "An on-demand render requires a non-empty unique positive page selection and its exact obligation set.",
                nameof(pageNumbers));
        }

        CorpusId = corpusId;
        DocumentId = documentId;
        DocumentVersion = documentVersion;
        SourceContentObjectId = sourceContentObjectId;
        SourceByteLength = sourceByteLength;
        Rights = rights;
        Policy = policy;
        GeneratedAt = generatedAt;
        ObligationSet = obligationSet;
        PageNumbers = selectedPages is null
            ? null
            : Array.AsReadOnly(selectedPages);
    }

    public CorpusId CorpusId { get; }

    public DocumentId DocumentId { get; }

    public DocumentVersionNumber DocumentVersion { get; }

    public ContentObjectId SourceContentObjectId { get; }

    public long SourceByteLength { get; }

    public DocumentRightsEligibilityRecordV1 Rights { get; }

    public PdfRenderPolicy Policy { get; }

    public DateTimeOffset GeneratedAt { get; }

    public DerivativeObligationSetV1? ObligationSet { get; }

    public IReadOnlyCollection<int>? PageNumbers { get; }
}

public sealed record DocumentRenderCandidateResult(
    StoreMutationOutcome Outcome,
    DocumentRenderManifest Manifest);

public sealed class DocumentRenderCandidateService
{
    private readonly IDocumentContentStore contentStore;
    private readonly IPdfPageRenderer renderer;
    private readonly IPngPageImageValidator pngValidator;
    private readonly IDocumentRenderManifestStore manifestStore;
    private readonly INoticeBearingPageImageCompositor? noticeCompositor;
    private readonly INoticeBearingPageImageValidator? noticeValidator;

    public DocumentRenderCandidateService(
        IDocumentContentStore contentStore,
        IPdfPageRenderer renderer,
        IPngPageImageValidator pngValidator,
        IDocumentRenderManifestStore manifestStore,
        INoticeBearingPageImageCompositor? noticeCompositor = null,
        INoticeBearingPageImageValidator? noticeValidator = null)
    {
        this.contentStore = contentStore ?? throw new ArgumentNullException(nameof(contentStore));
        this.renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        this.pngValidator = pngValidator ?? throw new ArgumentNullException(nameof(pngValidator));
        this.manifestStore = manifestStore ?? throw new ArgumentNullException(nameof(manifestStore));
        this.noticeCompositor = noticeCompositor;
        this.noticeValidator = noticeValidator;
    }

    public async Task<DocumentRenderCandidateResult> FinaliseAsync(
        DocumentRenderCandidateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var rights = DocumentRightsEligibilityPolicy.Evaluate(
            request.Rights,
            DocumentRightsEligibilityGate.PdfVisualEvidence);

        if (!rights.IsEligible)
        {
            throw new DocumentRenderCandidateException(
                DocumentRenderCandidateFailureKind.RightsIneligible);
        }

        if (request.ObligationSet is not null &&
            (noticeCompositor is null || noticeValidator is null ||
             !request.ObligationSet.MatchesRights(request.Rights)))
        {
            throw new DocumentRenderCandidateException(
                DocumentRenderCandidateFailureKind.RightsIneligible);
        }

        if (request.SourceByteLength > request.Policy.MaximumSourceByteLength)
        {
            throw new DocumentRenderCandidateException(
                DocumentRenderCandidateFailureKind.SourceVerificationFailed);
        }

        PdfRenderResult rendered;

        try
        {
            await using var source = await contentStore.OpenVerifiedAsync(
                request.SourceContentObjectId,
                new ExpectedHashAndLength(
                    request.SourceContentObjectId,
                    request.SourceByteLength),
                cancellationToken).ConfigureAwait(false);
            rendered = request.PageNumbers is null
                ? await renderer.RenderAsync(
                    source,
                    request.Policy,
                    cancellationToken).ConfigureAwait(false)
                : renderer is ISelectivePdfPageRenderer selectiveRenderer
                    ? await selectiveRenderer.RenderSelectionAsync(
                        source,
                        request.Policy,
                        request.PageNumbers,
                        cancellationToken).ConfigureAwait(false)
                    : throw new PdfRenderException(PdfRenderFailureKind.RendererUnavailable);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw new DocumentRenderCandidateException(
                DocumentRenderCandidateFailureKind.Cancelled);
        }
        catch (PdfRenderException)
        {
            throw new DocumentRenderCandidateException(
                DocumentRenderCandidateFailureKind.RendererFailed);
        }
        catch (DocumentRenderCandidateException)
        {
            throw;
        }
        catch
        {
            throw new DocumentRenderCandidateException(
                DocumentRenderCandidateFailureKind.SourceVerificationFailed);
        }

        var sourceDescriptor = renderer.Describe(request.Policy);
        var validatedPages = ValidateRendererOutput(
            rendered,
            sourceDescriptor,
            request.Policy,
            request.PageNumbers);
        var pageBindings = new List<DocumentPageImage>(validatedPages.Count);
        var expectedDescriptor = request.ObligationSet is null
            ? sourceDescriptor
            : noticeCompositor!.Describe(
                request.Policy,
                sourceDescriptor,
                request.ObligationSet);
        long publishedPixels = 0;
        long publishedBytes = 0;

        foreach (var item in validatedPages)
        {
            ReadOnlyMemory<byte> pageBytes = item.Candidate.PngBytes;
            var pageSha256 = item.Validation.Sha256;
            var pageByteLength = item.Validation.ByteLength;
            var widthPixels = item.Validation.WidthPixels;
            var heightPixels = item.Validation.HeightPixels;
            int? sourceRegionWidthPixels = null;
            int? sourceRegionHeightPixels = null;
            int? noticeRegionHeightPixels = null;

            if (request.ObligationSet is not null)
            {
                try
                {
                    var composite = noticeCompositor!.Compose(
                        item.Candidate,
                        item.Validation,
                        sourceDescriptor,
                        request.ObligationSet,
                        request.Policy);
                    var validation = noticeValidator!.Validate(
                        item.Candidate,
                        item.Validation,
                        composite,
                        request.Policy);

                    if (composite.RendererDescriptor != expectedDescriptor ||
                        validation.PageNumber != item.Validation.PageNumber)
                    {
                        throw new PdfRenderException(PdfRenderFailureKind.ProtocolViolation);
                    }

                    pageBytes = composite.PngBytes;
                    pageSha256 = validation.Sha256;
                    pageByteLength = validation.ByteLength;
                    widthPixels = validation.WidthPixels;
                    heightPixels = validation.HeightPixels;
                    sourceRegionWidthPixels = validation.SourceRegionWidthPixels;
                    sourceRegionHeightPixels = validation.SourceRegionHeightPixels;
                    noticeRegionHeightPixels = validation.NoticeRegionHeightPixels;
                }
                catch (PdfRenderException)
                {
                    throw new DocumentRenderCandidateException(
                        DocumentRenderCandidateFailureKind.PageImageInvalid);
                }
            }

            publishedPixels = checked(publishedPixels + ((long)widthPixels * heightPixels));
            publishedBytes = checked(publishedBytes + pageByteLength);

            if (pageByteLength > request.Policy.MaximumPageOutputByteLength ||
                publishedPixels > request.Policy.MaximumTotalPixels ||
                publishedBytes > request.Policy.MaximumTotalOutputByteLength)
            {
                throw new DocumentRenderCandidateException(
                    DocumentRenderCandidateFailureKind.PageImageInvalid);
            }

            ContentObjectDescriptor descriptor;

            try
            {
                await using var png = new MemoryStream(
                    pageBytes.ToArray(),
                    writable: false);
                descriptor = await contentStore.PutAndVerifyAsync(
                    new BoundedContentInput(
                        png,
                        request.Policy.MaximumPageOutputByteLength,
                        ContentMediaType.ImagePng,
                        pageSha256),
                    cancellationToken).ConfigureAwait(false);
                await using var reopened = await contentStore.OpenVerifiedAsync(
                    descriptor.ContentObjectId,
                    new ExpectedHashAndLength(
                        pageSha256,
                        pageByteLength),
                    cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw new DocumentRenderCandidateException(
                    DocumentRenderCandidateFailureKind.Cancelled);
            }
            catch
            {
                throw new DocumentRenderCandidateException(
                    DocumentRenderCandidateFailureKind.ContentPublicationFailed);
            }

            if (descriptor.Sha256 != pageSha256 ||
                descriptor.ByteLength != pageByteLength ||
                descriptor.MediaType != ContentMediaType.ImagePng)
            {
                throw new DocumentRenderCandidateException(
                    DocumentRenderCandidateFailureKind.ContentPublicationFailed);
            }

            pageBindings.Add(new DocumentPageImage(
                request.DocumentId,
                request.DocumentVersion,
                request.SourceContentObjectId,
                item.Validation.PageNumber,
                new RenderProfileId(request.ObligationSet is null
                    ? RenderProfileId.PdfPagePngV1
                    : RenderProfileId.PdfPagePngNoticeV1),
                expectedDescriptor,
                descriptor.ContentObjectId,
                new ImageSha256(descriptor.Sha256.Value),
                descriptor.ByteLength,
                descriptor.MediaType.Value,
                widthPixels,
                heightPixels,
                sourceRegionWidthPixels,
                sourceRegionHeightPixels,
                noticeRegionHeightPixels));
        }

        var manifest = request.ObligationSet is null
            ? DocumentRenderManifest.Create(
                request.DocumentId,
                request.DocumentVersion,
                request.SourceContentObjectId,
                rendered.SourcePageCount,
                new RenderProfileId(RenderProfileId.PdfPagePngV1),
                expectedDescriptor,
                pageBindings,
                request.GeneratedAt)
            : request.PageNumbers is null
                ? DocumentRenderManifest.CreateNoticeBearing(
                    request.DocumentId,
                    request.DocumentVersion,
                    request.SourceContentObjectId,
                    rendered.SourcePageCount,
                    expectedDescriptor,
                    request.ObligationSet,
                    pageBindings,
                    request.GeneratedAt)
                : DocumentRenderManifest.CreateNoticeBearingSelection(
                    request.DocumentId,
                    request.DocumentVersion,
                    request.SourceContentObjectId,
                    rendered.SourcePageCount,
                    expectedDescriptor,
                    request.ObligationSet,
                    pageBindings,
                    request.GeneratedAt);
        var commit = await manifestStore.CommitAsync(
            new RenderManifestCommitRequest(request.CorpusId, manifest, request.ObligationSet),
            cancellationToken).ConfigureAwait(false);

        if (commit.Outcome is not (StoreMutationOutcome.Applied or StoreMutationOutcome.AlreadyApplied) ||
            commit.CurrentManifest is null ||
            !SameIdentity(commit.CurrentManifest, manifest))
        {
            throw new DocumentRenderCandidateException(
                DocumentRenderCandidateFailureKind.ManifestPersistenceConflict);
        }

        var readback = await manifestStore.ReadAsync(
            request.CorpusId,
            manifest.RenderManifestId,
            cancellationToken).ConfigureAwait(false);

        if (readback is null || !SameIdentity(readback, manifest))
        {
            throw new DocumentRenderCandidateException(
                DocumentRenderCandidateFailureKind.ManifestReadbackFailed);
        }

        return new DocumentRenderCandidateResult(commit.Outcome, readback);
    }

    private ReadOnlyCollection<ValidatedPage> ValidateRendererOutput(
        PdfRenderResult rendered,
        RendererDescriptor expectedDescriptor,
        PdfRenderPolicy policy,
        IReadOnlyCollection<int>? selectedPageNumbers)
    {
        var expectedPages = selectedPageNumbers?.Order().ToArray();
        if (rendered.RendererDescriptor != expectedDescriptor ||
            rendered.SourcePageCount > policy.MaximumPageCount ||
            rendered.Pages.Count != (expectedPages?.Length ?? rendered.SourcePageCount))
        {
            throw new DocumentRenderCandidateException(
                DocumentRenderCandidateFailureKind.IncompleteRendererOutput);
        }

        var validated = new List<ValidatedPage>(rendered.Pages.Count);
        long totalPixels = 0;
        long totalOutputBytes = 0;

        for (var index = 0; index < rendered.Pages.Count; index++)
        {
            var candidate = rendered.Pages[index];

            if (candidate.PageNumber != (expectedPages?[index] ?? index + 1) ||
                candidate.PageNumber > rendered.SourcePageCount)
            {
                throw new DocumentRenderCandidateException(
                    DocumentRenderCandidateFailureKind.IncompleteRendererOutput);
            }

            PngPageImageValidation validation;

            try
            {
                validation = pngValidator.Validate(candidate, policy);
                totalPixels = checked(
                    totalPixels + ((long)validation.WidthPixels * validation.HeightPixels));
                totalOutputBytes = checked(totalOutputBytes + validation.ByteLength);
            }
            catch (DocumentRenderCandidateException)
            {
                throw;
            }
            catch
            {
                throw new DocumentRenderCandidateException(
                    DocumentRenderCandidateFailureKind.PageImageInvalid);
            }

            if (validation.PageNumber != candidate.PageNumber ||
                validation.ByteLength > policy.MaximumPageOutputByteLength ||
                totalPixels > policy.MaximumTotalPixels ||
                totalOutputBytes > policy.MaximumTotalOutputByteLength)
            {
                throw new DocumentRenderCandidateException(
                    DocumentRenderCandidateFailureKind.PageImageInvalid);
            }

            validated.Add(new ValidatedPage(candidate, validation));
        }

        return Array.AsReadOnly(validated.ToArray());
    }

    private static bool SameIdentity(
        DocumentRenderManifest left,
        DocumentRenderManifest right) =>
        left.RenderManifestId == right.RenderManifestId &&
        left.ManifestSha256 == right.ManifestSha256 &&
        left.DocumentId == right.DocumentId &&
        left.DocumentVersion == right.DocumentVersion &&
        left.SourceContentObjectId == right.SourceContentObjectId &&
        left.SourcePageCount == right.SourcePageCount &&
        left.RenderProfileId == right.RenderProfileId &&
        left.RendererDescriptor == right.RendererDescriptor &&
        left.ObligationSetId == right.ObligationSetId &&
        left.ObligationSetSha256 == right.ObligationSetSha256 &&
        left.OrderedPageImages.Select(PageIdentity)
            .SequenceEqual(right.OrderedPageImages.Select(PageIdentity));

    private static string PageIdentity(DocumentPageImage page) =>
        string.Join(
            '\n',
            page.PageNumber,
            page.ImageContentObjectId.Value,
            page.ImageSha256.Value,
            page.ByteLength,
            page.WidthPixels,
            page.HeightPixels,
            page.SourceRegionWidthPixels,
            page.SourceRegionHeightPixels,
            page.NoticeRegionHeightPixels);

    private sealed record ValidatedPage(
        RenderedPdfPageCandidate Candidate,
        PngPageImageValidation Validation);
}
