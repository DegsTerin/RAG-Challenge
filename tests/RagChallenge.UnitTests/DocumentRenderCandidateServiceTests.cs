// Purpose: Verifies deterministic render contracts and fail-closed candidate finalisation with synthetic bytes and in-memory ports only.
using System.Security.Cryptography;

using RagChallenge.Application.Documents;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.UnitTests;

public sealed class DocumentRenderCandidateServiceTests
{
    [Fact]
    public void PolicyAndDescriptorBindEveryLimitAndEffectiveRid()
    {
        var policy = Policy();
        var first = PdfPagePngV1RendererIdentity.CreateDescriptor(policy, "win-x64");
        var repeat = PdfPagePngV1RendererIdentity.CreateDescriptor(policy, "win-x64");
        var otherLimit = PdfPagePngV1RendererIdentity.CreateDescriptor(
            Policy(maximumPages: 3),
            "win-x64");
        var otherRid = PdfPagePngV1RendererIdentity.CreateDescriptor(policy, "linux-arm64");

        Assert.Equal(first, repeat);
        Assert.NotEqual(first, otherLimit);
        Assert.NotEqual(first, otherRid);
        Assert.Contains(PdfPagePngV1RendererIdentity.RendererId, first.Value);
        Assert.Contains(RenderProfileId.PdfPagePngV1, first.Value);
        Assert.InRange(first.Value.Length, 1, 128);
        Assert.Throws<ArgumentException>(() => new PdfRenderPolicy(
            1,
            1,
            1,
            2,
            1,
            1,
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(1)));
    }

    [Theory]
    [InlineData(DocumentRightDecisionState.Denied)]
    [InlineData(DocumentRightDecisionState.Unproven)]
    public async Task RightsGateBlocksBeforeSourceOrRenderer(DocumentRightDecisionState state)
    {
        var fixture = Fixture.Create(state);

        var exception = await Assert.ThrowsAsync<DocumentRenderCandidateException>(() =>
            fixture.Service.FinaliseAsync(fixture.Request));

        Assert.Equal(DocumentRenderCandidateFailureKind.RightsIneligible, exception.FailureKind);
        Assert.Equal(0, fixture.ContentStore.OpenCount);
        Assert.Equal(0, fixture.Renderer.RenderCount);
        Assert.Equal(0, fixture.ManifestStore.CommitCount);
    }

    [Fact]
    public async Task SourceMismatchBlocksBeforeRenderer()
    {
        var fixture = Fixture.Create();
        fixture.ContentStore.FailSourceOpen = true;

        var exception = await Assert.ThrowsAsync<DocumentRenderCandidateException>(() =>
            fixture.Service.FinaliseAsync(fixture.Request));

        Assert.Equal(
            DocumentRenderCandidateFailureKind.SourceVerificationFailed,
            exception.FailureKind);
        Assert.Equal(0, fixture.Renderer.RenderCount);
        Assert.Equal(0, fixture.ManifestStore.CommitCount);
    }

    [Fact]
    public async Task EveryPageIsValidatedBeforeAnyObjectIsPublished()
    {
        var fixture = Fixture.Create();
        fixture.Validator.FailPage = 2;

        var exception = await Assert.ThrowsAsync<DocumentRenderCandidateException>(() =>
            fixture.Service.FinaliseAsync(fixture.Request));

        Assert.Equal(DocumentRenderCandidateFailureKind.PageImageInvalid, exception.FailureKind);
        Assert.Equal(0, fixture.ContentStore.PutCount);
        Assert.Equal(0, fixture.ManifestStore.CommitCount);
    }

    [Fact]
    public async Task MissingOrOutOfOrderPageNeverPublishesAManifest()
    {
        var fixture = Fixture.Create();
        fixture.Renderer.Pages =
        [
            new RenderedPdfPageCandidate(2, 36, 24, new byte[] { 1, 2, 3 }),
            new RenderedPdfPageCandidate(1, 36, 24, new byte[] { 4, 5, 6 }),
        ];

        var exception = await Assert.ThrowsAsync<DocumentRenderCandidateException>(() =>
            fixture.Service.FinaliseAsync(fixture.Request));

        Assert.Equal(
            DocumentRenderCandidateFailureKind.IncompleteRendererOutput,
            exception.FailureKind);
        Assert.Equal(0, fixture.ContentStore.PutCount);
        Assert.Equal(0, fixture.ManifestStore.CommitCount);
    }

    [Fact]
    public async Task CompleteCandidatePublishesReopensCommitsAndReplaysIdempotently()
    {
        var fixture = Fixture.Create();

        var applied = await fixture.Service.FinaliseAsync(fixture.Request);
        var replay = await fixture.Service.FinaliseAsync(fixture.Request);

        Assert.Equal(StoreMutationOutcome.Applied, applied.Outcome);
        Assert.Equal(StoreMutationOutcome.AlreadyApplied, replay.Outcome);
        Assert.Equal(applied.Manifest.RenderManifestId, replay.Manifest.RenderManifestId);
        Assert.Equal(4, fixture.ContentStore.PutCount);
        Assert.Equal(6, fixture.ContentStore.OpenCount);
        Assert.Equal(2, fixture.ManifestStore.CommitCount);
        Assert.Equal(2, applied.Manifest.OrderedPageImages.Count);
        Assert.All(applied.Manifest.OrderedPageImages, page =>
        {
            Assert.Equal(DocumentPageImage.PngMediaType, page.MediaType);
            Assert.Equal(page.ImageContentObjectId.Value, page.ImageSha256.Value);
        });
    }

    private static PdfRenderPolicy Policy(int maximumPages = 2) =>
        new(
            maximumSourceByteLength: 1_024,
            maximumPageCount: maximumPages,
            maximumTotalPixels: 10_000,
            maximumPageOutputByteLength: 1_024,
            maximumTotalOutputByteLength: 2_048,
            maximumWorkerMemoryBytes: 64 * 1_024 * 1_024,
            maximumWorkerCpuTime: TimeSpan.FromSeconds(2),
            workerTimeout: TimeSpan.FromSeconds(3));

    private sealed class Fixture
    {
        private Fixture(
            FakeContentStore contentStore,
            FakeRenderer renderer,
            FakePngValidator validator,
            FakeManifestStore manifestStore,
            DocumentRenderCandidateRequest request)
        {
            ContentStore = contentStore;
            Renderer = renderer;
            Validator = validator;
            ManifestStore = manifestStore;
            Request = request;
            Service = new DocumentRenderCandidateService(
                contentStore,
                renderer,
                validator,
                manifestStore);
        }

        internal FakeContentStore ContentStore { get; }
        internal FakeRenderer Renderer { get; }
        internal FakePngValidator Validator { get; }
        internal FakeManifestStore ManifestStore { get; }
        internal DocumentRenderCandidateRequest Request { get; }
        internal DocumentRenderCandidateService Service { get; }

        internal static Fixture Create(
            DocumentRightDecisionState visualState = DocumentRightDecisionState.Permitted)
        {
            var source = new byte[] { 10, 20, 30, 40 };
            var sourceId = ContentId(source);
            var documentId = new DocumentId("document-render-synthetic");
            var documentVersion = new DocumentVersionNumber(1);
            var contentStore = new FakeContentStore(sourceId, source);
            var policy = Policy();
            var renderer = new FakeRenderer(policy);
            var validator = new FakePngValidator();
            var manifestStore = new FakeManifestStore();
            var decisions = Enum.GetValues<DocumentRight>()
                .Select(right => new DocumentRightDecision(
                    right,
                    right == DocumentRight.PageRendering ? visualState : DocumentRightDecisionState.Permitted,
                    new DocumentRightsEvidenceReference($"rights-render-{right}")));
            var request = new DocumentRenderCandidateRequest(
                new CorpusId("corpus-render-synthetic"),
                documentId,
                documentVersion,
                sourceId,
                source.Length,
                new DocumentRightsEligibilityRecordV1(
                    documentId,
                    documentVersion,
                    decisions),
                policy,
                new DateTimeOffset(2026, 8, 7, 12, 0, 0, TimeSpan.Zero));
            return new Fixture(contentStore, renderer, validator, manifestStore, request);
        }
    }

    private sealed class FakeRenderer(PdfRenderPolicy policy) : IPdfPageRenderer
    {
        internal IReadOnlyList<RenderedPdfPageCandidate> Pages { get; set; } =
        [
            new RenderedPdfPageCandidate(1, 36, 24, new byte[] { 1, 2, 3 }),
            new RenderedPdfPageCandidate(2, 36, 24, new byte[] { 4, 5, 6 }),
        ];

        internal int RenderCount { get; private set; }

        public RendererDescriptor Describe(PdfRenderPolicy requestedPolicy) =>
            PdfPagePngV1RendererIdentity.CreateDescriptor(requestedPolicy, "win-x64");

        public Task<PdfRenderResult> RenderAsync(
            VerifiedContentObject source,
            PdfRenderPolicy requestedPolicy,
            CancellationToken cancellationToken = default)
        {
            RenderCount++;
            return Task.FromResult(new PdfRenderResult(
                Describe(policy),
                Pages.Count,
                Pages));
        }
    }

    private sealed class FakePngValidator : IPngPageImageValidator
    {
        internal int? FailPage { get; set; }

        public PngPageImageValidation Validate(
            RenderedPdfPageCandidate candidate,
            PdfRenderPolicy policy)
        {
            if (candidate.PageNumber == FailPage)
            {
                throw new PdfRenderException(PdfRenderFailureKind.InvalidPageImage);
            }

            return new PngPageImageValidation(
                candidate.PageNumber,
                72,
                48,
                ContentId(candidate.PngBytes.Span),
                candidate.PngBytes.Length);
        }
    }

    private sealed class FakeContentStore : IDocumentContentStore
    {
        private readonly Dictionary<ContentObjectId, byte[]> objects = [];
        private readonly ContentObjectId sourceId;

        internal FakeContentStore(ContentObjectId sourceId, byte[] source)
        {
            this.sourceId = sourceId;
            objects.Add(sourceId, source);
        }

        internal bool FailSourceOpen { get; set; }
        internal int PutCount { get; private set; }
        internal int OpenCount { get; private set; }

        public Task<ContentObjectDescriptor> PutAndVerifyAsync(
            BoundedContentInput input,
            CancellationToken cancellationToken = default)
        {
            PutCount++;
            using var copy = new MemoryStream();
            input.Content.CopyTo(copy);
            var bytes = copy.ToArray();
            var id = ContentId(bytes);

            if (input.ExpectedContentObjectId is not null && input.ExpectedContentObjectId != id)
            {
                throw new InvalidDataException();
            }

            var outcome = objects.TryAdd(id, bytes)
                ? ContentObjectWriteOutcome.Published
                : ContentObjectWriteOutcome.AlreadyExisted;
            return Task.FromResult(new ContentObjectDescriptor(
                id,
                id,
                bytes.Length,
                input.MediaType,
                new ContentStoreImplementationDescriptor("fake-content-v1"),
                outcome,
                new ContentObjectVerificationResult(
                    ContentVerificationOutcome.Verified,
                    ContentVerificationOutcome.Verified)));
        }

        public ValueTask<VerifiedContentObject> OpenVerifiedAsync(
            ContentObjectId contentObjectId,
            ExpectedHashAndLength expected,
            CancellationToken cancellationToken = default)
        {
            OpenCount++;

            if ((contentObjectId == sourceId && FailSourceOpen) ||
                !objects.TryGetValue(contentObjectId, out var bytes) ||
                expected.Sha256 != contentObjectId ||
                expected.ByteLength != bytes.LongLength)
            {
                throw new InvalidDataException();
            }

            return ValueTask.FromResult(new VerifiedContentObject(
                contentObjectId,
                contentObjectId,
                bytes.LongLength,
                new MemoryStream(bytes, writable: false),
                ContentVerificationOutcome.Verified));
        }
    }

    private sealed class FakeManifestStore : IDocumentRenderManifestStore
    {
        private DocumentRenderManifest? manifest;

        internal int CommitCount { get; private set; }

        public Task<RenderManifestCommitResult> CommitAsync(
            RenderManifestCommitRequest request,
            CancellationToken cancellationToken = default)
        {
            CommitCount++;

            if (manifest is null)
            {
                manifest = request.Manifest;
                return Task.FromResult(new RenderManifestCommitResult(
                    StoreMutationOutcome.Applied,
                    manifest));
            }

            return Task.FromResult(new RenderManifestCommitResult(
                StoreMutationOutcome.AlreadyApplied,
                manifest));
        }

        public Task<DocumentRenderManifest?> ReadAsync(
            CorpusId corpusId,
            RenderManifestId renderManifestId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                manifest?.RenderManifestId == renderManifestId ? manifest : null);
    }

    private static ContentObjectId ContentId(ReadOnlySpan<byte> bytes) =>
        new(Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
}
