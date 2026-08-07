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
        DateTimeOffset generatedAt)
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

        if (generatedAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Render-candidate generation time must be expressed in UTC.",
                nameof(generatedAt));
        }

        CorpusId = corpusId;
        DocumentId = documentId;
        DocumentVersion = documentVersion;
        SourceContentObjectId = sourceContentObjectId;
        SourceByteLength = sourceByteLength;
        Rights = rights;
        Policy = policy;
        GeneratedAt = generatedAt;
    }

    public CorpusId CorpusId { get; }

    public DocumentId DocumentId { get; }

    public DocumentVersionNumber DocumentVersion { get; }

    public ContentObjectId SourceContentObjectId { get; }

    public long SourceByteLength { get; }

    public DocumentRightsEligibilityRecordV1 Rights { get; }

    public PdfRenderPolicy Policy { get; }

    public DateTimeOffset GeneratedAt { get; }
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

    public DocumentRenderCandidateService(
        IDocumentContentStore contentStore,
        IPdfPageRenderer renderer,
        IPngPageImageValidator pngValidator,
        IDocumentRenderManifestStore manifestStore)
    {
        this.contentStore = contentStore ?? throw new ArgumentNullException(nameof(contentStore));
        this.renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        this.pngValidator = pngValidator ?? throw new ArgumentNullException(nameof(pngValidator));
        this.manifestStore = manifestStore ?? throw new ArgumentNullException(nameof(manifestStore));
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
            rendered = await renderer.RenderAsync(
                source,
                request.Policy,
                cancellationToken).ConfigureAwait(false);
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

        var expectedDescriptor = renderer.Describe(request.Policy);
        var validatedPages = ValidateCompleteRendererOutput(
            rendered,
            expectedDescriptor,
            request.Policy);
        var pageBindings = new List<DocumentPageImage>(validatedPages.Count);

        foreach (var item in validatedPages)
        {
            ContentObjectDescriptor descriptor;

            try
            {
                await using var png = new MemoryStream(
                    item.Candidate.PngBytes.ToArray(),
                    writable: false);
                descriptor = await contentStore.PutAndVerifyAsync(
                    new BoundedContentInput(
                        png,
                        request.Policy.MaximumPageOutputByteLength,
                        ContentMediaType.ImagePng,
                        item.Validation.Sha256),
                    cancellationToken).ConfigureAwait(false);
                await using var reopened = await contentStore.OpenVerifiedAsync(
                    descriptor.ContentObjectId,
                    new ExpectedHashAndLength(
                        item.Validation.Sha256,
                        item.Validation.ByteLength),
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

            if (descriptor.Sha256 != item.Validation.Sha256 ||
                descriptor.ByteLength != item.Validation.ByteLength ||
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
                new RenderProfileId(RenderProfileId.PdfPagePngV1),
                expectedDescriptor,
                descriptor.ContentObjectId,
                new ImageSha256(descriptor.Sha256.Value),
                descriptor.ByteLength,
                descriptor.MediaType.Value,
                item.Validation.WidthPixels,
                item.Validation.HeightPixels));
        }

        var manifest = DocumentRenderManifest.Create(
            request.DocumentId,
            request.DocumentVersion,
            request.SourceContentObjectId,
            rendered.SourcePageCount,
            new RenderProfileId(RenderProfileId.PdfPagePngV1),
            expectedDescriptor,
            pageBindings,
            request.GeneratedAt);
        var commit = await manifestStore.CommitAsync(
            new RenderManifestCommitRequest(request.CorpusId, manifest),
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

    private ReadOnlyCollection<ValidatedPage> ValidateCompleteRendererOutput(
        PdfRenderResult rendered,
        RendererDescriptor expectedDescriptor,
        PdfRenderPolicy policy)
    {
        if (rendered.RendererDescriptor != expectedDescriptor ||
            rendered.SourcePageCount > policy.MaximumPageCount ||
            rendered.Pages.Count != rendered.SourcePageCount)
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

            if (candidate.PageNumber != index + 1)
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
            page.HeightPixels);

    private sealed record ValidatedPage(
        RenderedPdfPageCandidate Candidate,
        PngPageImageValidation Validation);
}
