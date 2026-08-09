// Purpose: Revalidates the active generation, rights-bound final manifest, exact page tuple and immutable PNG bytes before allowing same-origin visual serving.
using System.Buffers.Binary;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Infrastructure.Persistence;

public sealed class VerifiedPageImageEvidenceReader(
    CorpusId configuredCorpusId,
    IQueryActivationReader activationReader,
    IControlPlaneStore controlPlaneStore,
    IDocumentContentStore contentStore) : IVisualEvidenceReader
{
    public const long MaximumByteLength = 64L * 1024 * 1024;

    private static readonly byte[] PngSignature = [137, 80, 78, 71, 13, 10, 26, 10];
    private readonly CorpusId configuredCorpusId = configuredCorpusId ??
        throw new ArgumentNullException(nameof(configuredCorpusId));
    private readonly IQueryActivationReader activationReader = activationReader ??
        throw new ArgumentNullException(nameof(activationReader));
    private readonly IControlPlaneStore controlPlaneStore = controlPlaneStore ??
        throw new ArgumentNullException(nameof(controlPlaneStore));
    private readonly IDocumentContentStore contentStore = contentStore ??
        throw new ArgumentNullException(nameof(contentStore));

    public async Task<VisualEvidenceReadResult> ReadAsync(
        VisualEvidenceSelector selector,
        DateTimeOffset observedAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(selector);

        if (selector.PageNumber <= 0 || observedAt.Offset != TimeSpan.Zero)
        {
            return VisualEvidenceReadResult.NotAvailable();
        }

        try
        {
            var snapshot = await activationReader.ReadAsync(
                configuredCorpusId,
                observedAt,
                cancellationToken).ConfigureAwait(false);

            if (snapshot is null ||
                snapshot.ActivationRecord.IndexGenerationId != selector.IndexGenerationId)
            {
                return VisualEvidenceReadResult.NotAvailable();
            }

            var matchingBindings = snapshot.EvidenceBindings
                .Where(binding =>
                    binding.Binding.DocumentFormat == DocumentFormat.Pdf &&
                    binding.EvidenceBinding.RenderManifestId == selector.RenderManifestId &&
                    binding.RenderManifest?.RenderManifestId == selector.RenderManifestId)
                .ToArray();

            if (matchingBindings.Length != 1 ||
                !await IsCurrentlyActiveAsync(
                    matchingBindings[0],
                    cancellationToken).ConfigureAwait(false))
            {
                return VisualEvidenceReadResult.NotAvailable();
            }

            var matchingPages = matchingBindings[0].RenderManifest!.OrderedPageImages
                .Where(page =>
                    page.PageNumber == selector.PageNumber &&
                    page.ImageContentObjectId == selector.ImageContentObjectId)
                .ToArray();

            if (matchingPages.Length != 1)
            {
                return VisualEvidenceReadResult.NotAvailable();
            }

            var page = matchingPages[0];

            if (page.ByteLength is <= 0 or > MaximumByteLength ||
                page.ImageContentObjectId.Value != page.ImageSha256.Value ||
                !string.Equals(page.MediaType, DocumentPageImage.PngMediaType,
                    StringComparison.Ordinal))
            {
                return VisualEvidenceReadResult.Unavailable();
            }

            var content = await contentStore.OpenVerifiedAsync(
                page.ImageContentObjectId,
                new ExpectedHashAndLength(page.ImageContentObjectId, page.ByteLength),
                cancellationToken).ConfigureAwait(false);

            try
            {
                if (!await HasExpectedPngIdentityAsync(
                        content.Content,
                        page.WidthPixels,
                        page.HeightPixels,
                        cancellationToken).ConfigureAwait(false))
                {
                    await content.DisposeAsync().ConfigureAwait(false);
                    return VisualEvidenceReadResult.Unavailable();
                }

                return VisualEvidenceReadResult.Available(new VisualEvidenceContent(
                    content,
                    page.MediaType,
                    page.WidthPixels,
                    page.HeightPixels));
            }
            catch
            {
                await content.DisposeAsync().ConfigureAwait(false);
                throw;
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is ArgumentException or InvalidDataException or InvalidOperationException or
                IOException or Microsoft.Data.Sqlite.SqliteException or
                UnauthorizedAccessException)
        {
            return VisualEvidenceReadResult.Unavailable();
        }
    }

    private async Task<bool> IsCurrentlyActiveAsync(
        QueryEvidenceBinding binding,
        CancellationToken cancellationToken)
    {
        var catalogue = await controlPlaneStore.ReadCurrentCatalogueAsync(
            configuredCorpusId,
            cancellationToken).ConfigureAwait(false);

        if (catalogue is null)
        {
            throw new InvalidDataException(
                "The configured corpus has an active generation without a current catalogue.");
        }

        var document = catalogue.DocumentVersions.SingleOrDefault(document =>
            document.Id == binding.Binding.DocumentId &&
            document.Version == binding.Binding.DocumentVersion);
        var product = catalogue.DatabaseProducts.SingleOrDefault(product =>
            product.Id == binding.Binding.DatabaseProductId &&
            product.Revision == binding.Binding.DatabaseProductRevision);
        return document is not null && product is not null &&
            document.Status == CatalogueItemStatus.Active &&
            product.Status == CatalogueItemStatus.Active &&
            document.DatabaseProductId == product.Id &&
            document.DatabaseProductRevision == product.Revision &&
            document.ContentObjectId == binding.EvidenceBinding.SourceContentObjectId;
    }

    private static async Task<bool> HasExpectedPngIdentityAsync(
        Stream content,
        int expectedWidth,
        int expectedHeight,
        CancellationToken cancellationToken)
    {
        var header = new byte[24];
        var offset = 0;

        while (offset < header.Length)
        {
            var read = await content.ReadAsync(
                header.AsMemory(offset, header.Length - offset),
                cancellationToken).ConfigureAwait(false);

            if (read == 0)
            {
                content.Position = 0;
                return false;
            }

            offset += read;
        }

        content.Position = 0;
        return header.AsSpan(0, PngSignature.Length).SequenceEqual(PngSignature) &&
            BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(8, 4)) == 13 &&
            header.AsSpan(12, 4).SequenceEqual("IHDR"u8) &&
            BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(16, 4)) == expectedWidth &&
            BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(20, 4)) == expectedHeight;
    }
}
