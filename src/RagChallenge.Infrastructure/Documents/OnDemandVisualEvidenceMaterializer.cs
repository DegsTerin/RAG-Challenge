// Purpose: Materialises at most five exact cited PDF pages after textual answering, preserving text-only success when the optional visual path is unavailable.
using RagChallenge.Application.Documents;
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Infrastructure.Documents;

public sealed class OnDemandVisualEvidenceMaterializer(
    CorpusId configuredCorpusId,
    IControlPlaneStore controlPlaneStore,
    IDocumentRenderManifestStore renderManifestStore,
    DocumentRenderCandidateService candidateService,
    PdfRenderPolicy policy) : IQueryVisualEvidenceMaterializer
{
    public const int MaximumPageCount = 5;

    private readonly CorpusId configuredCorpusId = configuredCorpusId ??
        throw new ArgumentNullException(nameof(configuredCorpusId));
    private readonly IControlPlaneStore controlPlaneStore = controlPlaneStore ??
        throw new ArgumentNullException(nameof(controlPlaneStore));
    private readonly IDocumentRenderManifestStore renderManifestStore = renderManifestStore ??
        throw new ArgumentNullException(nameof(renderManifestStore));
    private readonly DocumentRenderCandidateService candidateService = candidateService ??
        throw new ArgumentNullException(nameof(candidateService));
    private readonly PdfRenderPolicy policy = policy ?? throw new ArgumentNullException(nameof(policy));

    public async Task<IReadOnlyCollection<OnDemandVisualEvidenceMaterialisation>> MaterialiseAsync(
        QueryActivationSnapshot snapshot,
        IReadOnlyCollection<QueryCitation> citations,
        DateTimeOffset generatedAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(citations);
        if (snapshot.ActivationRecord.CorpusId != configuredCorpusId ||
            generatedAt.Offset != TimeSpan.Zero)
        {
            throw new InvalidDataException("On-demand visual evidence authority is invalid.");
        }

        var selections = citations
            .Where(citation => citation.DocumentFormat == DocumentFormat.Pdf)
            .SelectMany(citation => Enumerable.Range(
                citation.PageStart!.Value,
                citation.PageEnd!.Value - citation.PageStart.Value + 1)
                .Select(pageNumber => (citation.DocumentId, citation.DocumentVersion, pageNumber)))
            .Distinct()
            .OrderBy(item => item.DocumentId.Value, StringComparer.Ordinal)
            .ThenBy(item => item.DocumentVersion.Value)
            .ThenBy(item => item.pageNumber)
            .Take(MaximumPageCount)
            .GroupBy(item => (item.DocumentId, item.DocumentVersion))
            .ToArray();
        if (selections.Length == 0)
        {
            return [];
        }

        var catalogue = await controlPlaneStore.ReadCurrentCatalogueAsync(
            configuredCorpusId,
            cancellationToken).ConfigureAwait(false) ?? throw new InvalidDataException(
                "On-demand visual evidence has no current catalogue.");
        var results = new List<OnDemandVisualEvidenceMaterialisation>(selections.Length);

        foreach (var selection in selections)
        {
            var binding = snapshot.EvidenceBindings.Single(item =>
                item.Binding.DocumentId == selection.Key.DocumentId &&
                item.Binding.DocumentVersion == selection.Key.DocumentVersion);
            if (binding.EvidenceBinding.RenderManifestId is not null)
            {
                continue;
            }

            var rights = DocumentRightsEligibilityPolicy.Evaluate(
                binding.EvidenceBinding.Rights,
                DocumentRightsEligibilityGate.PdfVisualEvidence);
            if (!rights.IsEligible)
            {
                continue;
            }

            var document = catalogue.DocumentVersions.Single(item =>
                item.Id == selection.Key.DocumentId &&
                item.Version == selection.Key.DocumentVersion &&
                item.ContentObjectId == binding.EvidenceBinding.SourceContentObjectId &&
                item.Status == CatalogueItemStatus.Active);
            var obligationSet = await renderManifestStore.ReadObligationSetForSourceAsync(
                configuredCorpusId,
                document.Id,
                document.Version,
                document.ContentObjectId,
                cancellationToken).ConfigureAwait(false) ?? throw new InvalidDataException(
                    "On-demand visual evidence has no exact derivative obligation set.");
            var selectedPages = selection.Select(item => item.pageNumber).ToArray();
            var finalised = await candidateService.FinaliseAsync(
                new DocumentRenderCandidateRequest(
                    configuredCorpusId,
                    document.Id,
                    document.Version,
                    document.ContentObjectId,
                    document.ByteLength,
                    binding.EvidenceBinding.Rights,
                    policy,
                    generatedAt,
                    obligationSet,
                    selectedPages),
                cancellationToken).ConfigureAwait(false);
            if (finalised.Outcome is not StoreMutationOutcome.Applied and
                    not StoreMutationOutcome.AlreadyApplied ||
                finalised.Manifest.IsComplete ||
                !finalised.Manifest.OrderedPageImages.Select(page => page.PageNumber)
                    .SequenceEqual(selectedPages))
            {
                throw new InvalidDataException(
                    "On-demand visual evidence did not persist the exact cited page selection.");
            }

            results.Add(new OnDemandVisualEvidenceMaterialisation(
                finalised.Manifest,
                obligationSet));
        }

        return results.AsReadOnly();
    }
}
