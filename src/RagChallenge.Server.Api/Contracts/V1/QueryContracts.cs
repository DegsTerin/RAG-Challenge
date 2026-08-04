// Purpose: Defines the stable transport-only v1 query, completion, citation, coverage and Problem Details mappings without exposing Domain or provider types.
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Server.Api.Contracts.V1;

public sealed record QueryRequestV1(
    string CorpusId,
    string QuestionLanguage,
    string Question);

public sealed record LivenessV1(string Status);

public sealed record SanitisedSourceStateV1(
    string SourceId,
    string State);

public sealed record SanitisedCapabilityCheckV1(
    string Capability,
    string State);

public sealed record ReadinessV1(
    string Status,
    int ActiveDatabaseCount,
    int EligibleDocumentCount,
    int DegradedDocumentCount,
    IReadOnlyCollection<SanitisedSourceStateV1> SourceStates,
    string? ActiveGenerationId,
    string ConfigurationRevision,
    IReadOnlyCollection<SanitisedCapabilityCheckV1> Checks,
    DateTimeOffset ObservedAt);

public sealed record EvidenceCoverageV1(
    int ActiveDatabaseCount,
    int ActiveDocumentCount,
    int EligibleDatabaseCount,
    int EligibleDocumentCount,
    IReadOnlyDictionary<string, string> DegradedSources);

public sealed record CitationV1(
    string CorpusId,
    string IndexGenerationId,
    string DatabaseProductId,
    long DatabaseProductRevision,
    string DocumentId,
    long DocumentVersion,
    string DocumentFormat,
    string ContentLanguage,
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
    string SourceFreshness);

public sealed record LanguageModelDescriptorV1(
    string ProviderId,
    string ModelId,
    string ModelRevision);

public sealed record QueryResponseV1(
    string Outcome,
    string AnswerLanguage,
    string? Answer,
    IReadOnlyCollection<CitationV1> Citations,
    EvidenceCoverageV1 EvidenceCoverage,
    string IndexGenerationId,
    string RetrievalPolicyVersion,
    string PromptVersion,
    LanguageModelDescriptorV1 LanguageModelDescriptor,
    string CorrelationId);

internal static class QueryContractMapper
{
    internal static QueryResponseV1 ToV1(QueryCompletion completion) =>
        new(
            completion.Outcome.ToString(),
            completion.AnswerLanguage.ToCanonicalTag(),
            completion.Answer,
            completion.Citations.Select(ToV1).ToArray(),
            new EvidenceCoverageV1(
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
            new LanguageModelDescriptorV1(
                completion.LanguageModelDescriptor.ProviderId,
                completion.LanguageModelDescriptor.ModelId,
                completion.LanguageModelDescriptor.ModelRevision),
            completion.CorrelationId);

    private static CitationV1 ToV1(QueryCitation citation) =>
        new(
            citation.CorpusId.Value,
            citation.IndexGenerationId.Value,
            citation.DatabaseProductId.Value,
            citation.DatabaseProductRevision.Value,
            citation.DocumentId.Value,
            citation.DocumentVersion.Value,
            citation.DocumentFormat.ToString(),
            citation.ContentLanguage.ToCanonicalTag(),
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
            citation.SourceFreshness.ToString());
}
