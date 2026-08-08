// Purpose: Owns Answered-only evidence-record composition, persistence ports, coverage canonicalisation, and sanitised operational signals without exposing the internal record through API v1.
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.Application.IndexingRetrieval;

public enum AnswerEvidencePersistenceOutcome
{
    Applied,
    AlreadyApplied,
    Conflict,
}

public sealed record AnswerEvidencePersistenceResult(
    AnswerEvidencePersistenceOutcome Outcome,
    AnswerEvidenceRecordV1? PersistedRecord);

public interface IAnswerEvidenceStore
{
    Task<AnswerEvidencePersistenceResult> PersistAsync(
        AnswerEvidenceRecordV1 record,
        CancellationToken cancellationToken = default);

    Task<AnswerEvidenceRecordV1?> ReadAsync(
        AnswerEvidenceRecordId recordId,
        CancellationToken cancellationToken = default);
}

public interface IAnswerEvidenceRecordIdSource
{
    AnswerEvidenceRecordId Create();
}

public sealed class SystemAnswerEvidenceRecordIdSource : IAnswerEvidenceRecordIdSource
{
    public AnswerEvidenceRecordId Create() => AnswerEvidenceRecordId.FromGuid(Guid.NewGuid());
}

public sealed record AnswerEvidenceActivity(
    AnswerEvidenceRecordId AnswerEvidenceRecordId,
    string CorrelationId,
    CorpusId CorpusId,
    IndexGenerationId IndexGenerationId,
    int CitationCount,
    int PageImageCount,
    long ElapsedMilliseconds,
    string RetentionOutcome,
    string? FailureCode);

public interface IAnswerEvidenceActivitySink
{
    void Record(AnswerEvidenceActivity activity);
}

public sealed class NullAnswerEvidenceActivitySink : IAnswerEvidenceActivitySink
{
    public static NullAnswerEvidenceActivitySink Instance { get; } = new();

    private NullAnswerEvidenceActivitySink()
    {
    }

    public void Record(AnswerEvidenceActivity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);
    }
}

public static class EvidenceCoverageCanonicalizer
{
    public const string CanonicalDomain = "rag-challenge/evidence-coverage/v1";

    public static EvidenceCoverageDigest Digest(EvidenceCoverage coverage)
    {
        ArgumentNullException.ThrowIfNull(coverage);

        if (coverage.ActiveDatabaseCount < 0 || coverage.ActiveDocumentCount < 0 ||
            coverage.EligibleDatabaseCount < 0 || coverage.EligibleDocumentCount < 0 ||
            coverage.EligibleDatabaseCount > coverage.ActiveDatabaseCount ||
            coverage.EligibleDocumentCount > coverage.ActiveDocumentCount)
        {
            throw new ArgumentException(
                "Evidence coverage counts must be non-negative subsets of the active counts.",
                nameof(coverage));
        }

        ArgumentNullException.ThrowIfNull(coverage.DegradedSources);
        var target = new StringBuilder();
        Append(target, "domain", CanonicalDomain);
        Append(target, "activeDatabaseCount", coverage.ActiveDatabaseCount);
        Append(target, "activeDocumentCount", coverage.ActiveDocumentCount);
        Append(target, "eligibleDatabaseCount", coverage.EligibleDatabaseCount);
        Append(target, "eligibleDocumentCount", coverage.EligibleDocumentCount);
        Append(target, "degradedSourceCount", coverage.DegradedSources.Count);

        foreach (var pair in coverage.DegradedSources.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            if (string.IsNullOrWhiteSpace(pair.Key) || pair.Key.Length > 128 ||
                !Enum.IsDefined(pair.Value))
            {
                throw new ArgumentException(
                    "Evidence coverage source identities and states must be bounded and valid.",
                    nameof(coverage));
            }

            Append(target, "degradedSource.id", pair.Key);
            Append(target, "degradedSource.state", pair.Value.ToString());
        }

        return new EvidenceCoverageDigest(Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(target.ToString()))).ToLowerInvariant());
    }

    private static void Append(StringBuilder target, string name, object value)
    {
        var text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        target.Append(name);
        target.Append(':');
        target.Append(Encoding.UTF8.GetByteCount(text).ToString(CultureInfo.InvariantCulture));
        target.Append(':');
        target.Append(text);
        target.Append('\n');
    }
}

internal static class AnswerEvidenceRecordComposer
{
    internal static AnswerEvidenceRecordV1 Create(
        AnswerEvidenceRecordId recordId,
        QueryActivationSnapshot snapshot,
        QueryCompletion completion,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(recordId);
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(completion);

        if (completion.Outcome != QueryOutcome.Answered ||
            string.IsNullOrWhiteSpace(completion.Answer) ||
            completion.Citations.Count == 0)
        {
            throw new InvalidDataException(
                "Only a fully validated Answered completion can produce persistent evidence.");
        }

        var citationBindings = completion.Citations.Select((citation, index) =>
        {
            var resolved = snapshot.EvidenceBindings.SingleOrDefault(binding =>
                binding.Binding.DatabaseProductId == citation.DatabaseProductId &&
                binding.Binding.DatabaseProductRevision == citation.DatabaseProductRevision &&
                binding.Binding.DocumentId == citation.DocumentId &&
                binding.Binding.DocumentVersion == citation.DocumentVersion) ??
                throw new InvalidDataException(
                    "A response citation has no exact activation evidence binding.");
            var evidence = resolved.EvidenceBinding;

            return new AnswerEvidenceCitationBindingV1(
                index + 1,
                citation.DatabaseProductId,
                citation.DatabaseProductRevision,
                citation.DocumentId,
                citation.DocumentVersion,
                citation.DocumentFormat,
                citation.ContentLanguage,
                citation.ChunkId,
                citation.SourceAdapterId,
                citation.SourceTrustClass,
                resolved.Binding.OfficialSourceRegistrationId,
                resolved.Binding.OfficialSnapshotId,
                resolved.Binding.SourceObservationId,
                evidence.SourceContentObjectId,
                citation.PageStart,
                citation.PageEnd,
                citation.RecordStart,
                citation.RecordEnd,
                citation.Columns,
                sectionLocator: null,
                evidence.RenderManifestId);
        }).ToArray();
        var pageBindings = citationBindings
            .Where(citation => citation.DocumentFormat == DocumentFormat.Pdf)
            .SelectMany(citation => Enumerable.Range(
                citation.PageStart!.Value,
                citation.PageEnd!.Value - citation.PageStart.Value + 1)
                .Select(pageNumber => CreatePageBinding(snapshot, citation, pageNumber)))
            .GroupBy(page => (page.DocumentId, page.DocumentVersion, page.PageNumber))
            .Select(group => group.First())
            .ToArray();
        var answerBytes = Encoding.UTF8.GetBytes(completion.Answer);

        return AnswerEvidenceRecordV1.Create(
            recordId,
            snapshot.ActivationRecord.CorpusId,
            snapshot.ActivationRecord.RecordRevision,
            snapshot.ActivationRecord.CatalogueRevision,
            BindingDigestCanonicalizer.CanonicaliseSourceBindingSet(
                snapshot.ActivationRecord.DocumentBindings).Digest,
            snapshot.ActivationRecord.ActivationBindingSetDigest,
            snapshot.ActivationRecord.IndexGenerationId,
            completion.AnswerLanguage,
            new AnswerSha256(Convert.ToHexString(SHA256.HashData(answerBytes)).ToLowerInvariant()),
            answerBytes.Length,
            EvidenceCoverageCanonicalizer.Digest(completion.EvidenceCoverage),
            completion.RetrievalPolicyVersion,
            completion.PromptVersion,
            new AnswerLanguageModelDescriptorV1(
                completion.LanguageModelDescriptor.ProviderId,
                completion.LanguageModelDescriptor.ModelId,
                completion.LanguageModelDescriptor.ModelRevision),
            completion.CorrelationId,
            createdAt,
            citationBindings,
            pageBindings);
    }

    internal static AnswerEvidenceActivity CreateActivity(
        AnswerEvidenceRecordV1 record,
        long startedTimestamp,
        string retentionOutcome,
        string? failureCode = null) =>
        new(
            record.AnswerEvidenceRecordId,
            record.CorrelationId,
            record.CorpusId,
            record.IndexGenerationId,
            record.Citations.Count,
            record.PageImages.Count,
            (long)Stopwatch.GetElapsedTime(startedTimestamp).TotalMilliseconds,
            retentionOutcome,
            failureCode);

    private static AnswerEvidencePageBindingV1 CreatePageBinding(
        QueryActivationSnapshot snapshot,
        AnswerEvidenceCitationBindingV1 citation,
        int pageNumber)
    {
        var resolved = snapshot.EvidenceBindings.Single(binding =>
            binding.Binding.DocumentId == citation.DocumentId &&
            binding.Binding.DocumentVersion == citation.DocumentVersion);
        var manifest = resolved.RenderManifest ?? throw new InvalidDataException(
            "A cited PDF has no fully hydrated final render manifest.");

        if (manifest.RenderManifestId != citation.RenderManifestId ||
            manifest.SourceContentObjectId != citation.SourceContentObjectId)
        {
            throw new InvalidDataException(
                "A cited PDF differs from its activation-bound render manifest.");
        }

        var page = manifest.OrderedPageImages.SingleOrDefault(page =>
            page.PageNumber == pageNumber) ?? throw new InvalidDataException(
                "A cited physical page is absent from its final render manifest.");
        return new AnswerEvidencePageBindingV1(
            page.DocumentId,
            page.DocumentVersion,
            page.SourceContentObjectId,
            page.PageNumber,
            manifest.RenderManifestId,
            page.RenderProfileId,
            page.RendererDescriptor,
            page.ImageContentObjectId,
            page.ImageSha256,
            page.ByteLength,
            page.MediaType,
            page.WidthPixels,
            page.HeightPixels);
    }
}
