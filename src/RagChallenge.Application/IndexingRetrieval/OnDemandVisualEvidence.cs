// Purpose: Defines the optional query-time materialisation boundary for cited PDF pages; textual answers remain authoritative when visual rendering is unavailable.
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Application.IndexingRetrieval;

public sealed class OnDemandVisualEvidenceMaterialisation
{
    public OnDemandVisualEvidenceMaterialisation(
        DocumentRenderManifest manifest,
        DerivativeObligationSetV1 obligationSet)
    {
        Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        ObligationSet = obligationSet ?? throw new ArgumentNullException(nameof(obligationSet));

        if (manifest.RenderProfileId.Value != RenderProfileId.PdfPagePngNoticeV1 ||
            manifest.ObligationSetId != obligationSet.ObligationSetId ||
            manifest.ObligationSetSha256 != obligationSet.CanonicalSha256 ||
            manifest.DocumentId != obligationSet.DocumentId ||
            manifest.DocumentVersion != obligationSet.DocumentVersion ||
            manifest.SourceContentObjectId != obligationSet.SourceContentObjectId ||
            manifest.OrderedPageImages.Count is <= 0 or > 5)
        {
            throw new ArgumentException(
                "On-demand visual evidence must bind one to five exact notice-bearing cited pages.",
                nameof(manifest));
        }
    }

    public DocumentRenderManifest Manifest { get; }

    public DerivativeObligationSetV1 ObligationSet { get; }
}

public interface IQueryVisualEvidenceMaterializer
{
    Task<IReadOnlyCollection<OnDemandVisualEvidenceMaterialisation>> MaterialiseAsync(
        QueryActivationSnapshot snapshot,
        IReadOnlyCollection<QueryCitation> citations,
        DateTimeOffset generatedAt,
        CancellationToken cancellationToken = default);
}
