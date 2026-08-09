// Purpose: Defines the frozen transport-only v2 query and visual-evidence projection without exposing internal persistence identities, paths, rights or bytes.
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Server.Api.Contracts.V2;

public sealed record QueryRequestV2(
    string CorpusId,
    string QuestionLanguage,
    string Question);

public sealed record EvidenceCoverageV2(
    int ActiveDatabaseCount,
    int ActiveDocumentCount,
    int EligibleDatabaseCount,
    int EligibleDocumentCount,
    IReadOnlyDictionary<string, string> DegradedSources);

public sealed record PageImageEvidenceV1(
    int PageNumber,
    string RenderManifestId,
    string ImageContentObjectId,
    string MediaType,
    int WidthPixels,
    int HeightPixels,
    string ContentSha256);

public sealed record CitationV2(
    string CorpusId,
    string IndexGenerationId,
    string DatabaseProductId,
    long DatabaseProductRevision,
    string DocumentId,
    long DocumentVersion,
    string DocumentFormat,
    string ContentLanguage,
    string? SourceDeclaredLanguage,
    string ChunkId,
    string SourceAdapterId,
    string SourceTrustClass,
    string Excerpt,
    string? Title,
    int? PageStart,
    int? PageEnd,
    long? RecordStart,
    long? RecordEnd,
    IReadOnlyCollection<string> Columns,
    string? CanonicalUrl,
    string? SourceSnapshotId,
    DateTimeOffset? RevalidatedAt,
    string SourceFreshness,
    IReadOnlyCollection<PageImageEvidenceV1> PageImages);

public sealed record LanguageModelDescriptorV2(
    string ProviderId,
    string ModelId,
    string ModelRevision);

public sealed record QueryResponseV2(
    string Outcome,
    string AnswerLanguage,
    string? Answer,
    IReadOnlyCollection<CitationV2> Citations,
    EvidenceCoverageV2 EvidenceCoverage,
    string IndexGenerationId,
    string RetrievalPolicyVersion,
    string PromptVersion,
    LanguageModelDescriptorV2 LanguageModelDescriptor,
    string CorrelationId);

internal static class QueryContractMapper
{
    internal static QueryResponseV2 ToV2(QueryCompletion completion)
    {
        var citations = completion.Citations.Select(ToV2).ToArray();
        var pages = citations.SelectMany(citation => citation.PageImages.Select(page => (
            citation.DocumentId,
            citation.DocumentVersion,
            Page: page))).ToArray();

        if (pages.Length > 5 || pages
            .Select(item => (item.DocumentId, item.DocumentVersion, item.Page.PageNumber))
            .Distinct().Count() != pages.Length)
        {
            throw new InvalidDataException(
                "A v2 response contains an invalid visual-evidence projection.");
        }

        return new QueryResponseV2(
            completion.Outcome.ToString(),
            completion.AnswerLanguage.ToCanonicalTag(),
            completion.Answer,
            citations,
            new EvidenceCoverageV2(
                completion.EvidenceCoverage.ActiveDatabaseCount,
                completion.EvidenceCoverage.ActiveDocumentCount,
                completion.EvidenceCoverage.EligibleDatabaseCount,
                completion.EvidenceCoverage.EligibleDocumentCount,
                completion.EvidenceCoverage.DegradedSources.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.ToString(),
                    StringComparer.Ordinal)),
            completion.IndexGenerationId.Value,
            completion.RetrievalPolicyVersion,
            completion.PromptVersion,
            new LanguageModelDescriptorV2(
                completion.LanguageModelDescriptor.ProviderId,
                completion.LanguageModelDescriptor.ModelId,
                completion.LanguageModelDescriptor.ModelRevision),
            completion.CorrelationId);
    }

    private static CitationV2 ToV2(QueryCitation citation)
    {
        var pages = citation.PageImages.Select(page => new PageImageEvidenceV1(
            page.PageNumber,
            page.RenderManifestId.Value,
            page.ImageContentObjectId.Value,
            page.MediaType,
            page.WidthPixels,
            page.HeightPixels,
            page.ContentSha256.Value)).ToArray();

        if (citation.DocumentFormat == DocumentFormat.Csv && pages.Length != 0 ||
            citation.DocumentFormat == DocumentFormat.Pdf && pages.Any(page =>
                page.PageNumber < citation.PageStart || page.PageNumber > citation.PageEnd) ||
            pages.Any(page =>
                page.ImageContentObjectId != page.ContentSha256 ||
                !string.Equals(page.MediaType, DocumentPageImage.PngMediaType,
                    StringComparison.Ordinal)))
        {
            throw new InvalidDataException(
                "A citation contains visual evidence outside its exact source binding.");
        }

        return new CitationV2(
            citation.CorpusId.Value,
            citation.IndexGenerationId.Value,
            citation.DatabaseProductId.Value,
            citation.DatabaseProductRevision.Value,
            citation.DocumentId.Value,
            citation.DocumentVersion.Value,
            citation.DocumentFormat.ToString(),
            citation.ContentLanguage.ToCanonicalTag(),
            citation.SourceDeclaredLanguage?.ObservedTag,
            citation.ChunkId,
            citation.SourceAdapterId.Value,
            citation.SourceTrustClass.ToString(),
            citation.Excerpt,
            citation.Title,
            citation.PageStart,
            citation.PageEnd,
            citation.RecordStart,
            citation.RecordEnd,
            citation.Columns,
            citation.CanonicalUrl,
            citation.SourceSnapshotId?.Value,
            citation.RevalidatedAt,
            citation.SourceFreshness.ToString(),
            pages);
    }
}
