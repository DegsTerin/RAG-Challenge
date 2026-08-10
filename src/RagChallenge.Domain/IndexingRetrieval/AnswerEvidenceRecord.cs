// Purpose: Defines the immutable, privacy-minimised AnswerEvidenceRecordV1 identity, bindings, retention, and canonical digest while persistence and public transport remain outer-layer concerns.
using System.Collections.ObjectModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Domain.IndexingRetrieval;

public sealed record AnswerEvidenceRecordId
{
    private static readonly Regex Pattern = new(
        "^ans-evidence-[0-9a-f]{32}$",
        RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    public AnswerEvidenceRecordId(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (!Pattern.IsMatch(value))
        {
            throw new ArgumentException(
                "An answer-evidence record ID must use 'ans-evidence-' followed by 32 lowercase hexadecimal characters.",
                nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public static AnswerEvidenceRecordId FromGuid(Guid value) =>
        new($"ans-evidence-{value:N}");
}

public sealed record AnswerEvidenceRecordSha256 : LowercaseSha256
{
    public AnswerEvidenceRecordSha256(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record AnswerSha256 : LowercaseSha256
{
    public AnswerSha256(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record EvidenceCoverageDigest : LowercaseSha256
{
    public EvidenceCoverageDigest(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record AnswerLanguageModelDescriptorV1
{
    public AnswerLanguageModelDescriptorV1(
        string providerId,
        string modelId,
        string modelRevision)
    {
        ProviderId = RequireValue(providerId, nameof(providerId));
        ModelId = RequireValue(modelId, nameof(modelId));
        ModelRevision = RequireValue(modelRevision, nameof(modelRevision));
    }

    public string ProviderId { get; }

    public string ModelId { get; }

    public string ModelRevision { get; }

    private static string RequireValue(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);

        if (value.Length > 128 || value.Any(character =>
                !char.IsAsciiLetterOrDigit(character) &&
                character is not '.' and not '_' and not ':' and not '-'))
        {
            throw new ArgumentException(
                "A language-model descriptor must be bounded safe ASCII.",
                parameterName);
        }

        return value;
    }
}

public sealed class AnswerEvidenceCitationBindingV1
{
    public AnswerEvidenceCitationBindingV1(
        int ordinal,
        DatabaseProductId databaseProductId,
        DatabaseProductRevision databaseProductRevision,
        DocumentId documentId,
        DocumentVersionNumber documentVersion,
        DocumentFormat documentFormat,
        DocumentContentLanguage contentLanguage,
        string chunkId,
        SourceAdapterId sourceAdapterId,
        SourceTrustClass sourceTrustClass,
        OfficialSourceRegistrationId? officialSourceRegistrationId,
        OfficialSnapshotId? sourceSnapshotId,
        OfficialObservationId? sourceObservationId,
        ContentObjectId sourceContentObjectId,
        int? pageStart,
        int? pageEnd,
        long? recordStart,
        long? recordEnd,
        IEnumerable<string>? columns,
        string? sectionLocator,
        RenderManifestId? renderManifestId)
    {
        ArgumentNullException.ThrowIfNull(databaseProductId);
        ArgumentNullException.ThrowIfNull(databaseProductRevision);
        ArgumentNullException.ThrowIfNull(documentId);
        ArgumentNullException.ThrowIfNull(documentVersion);
        ArgumentNullException.ThrowIfNull(contentLanguage);
        ArgumentNullException.ThrowIfNull(sourceAdapterId);
        ArgumentNullException.ThrowIfNull(sourceContentObjectId);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ordinal);

        if (!Enum.IsDefined(documentFormat) || !Enum.IsDefined(sourceTrustClass))
        {
            throw new ArgumentException(
                "An answer-evidence citation must use the closed format and trust sets.");
        }

        chunkId = RequireSafeIdentifier(chunkId, nameof(chunkId));
        var canonicalColumns = CanonicaliseColumns(columns);

        if (sectionLocator is not null &&
            (sectionLocator.Length is 0 or > 512 ||
             sectionLocator.Any(char.IsControl)))
        {
            throw new ArgumentException(
                "A structural section locator must contain 1..512 non-control characters.",
                nameof(sectionLocator));
        }

        if (sourceTrustClass == SourceTrustClass.LocalAuthorised &&
            (officialSourceRegistrationId is not null || sourceSnapshotId is not null ||
             sourceObservationId is not null))
        {
            throw new ArgumentException(
                "A local citation cannot carry official source identities.",
                nameof(sourceTrustClass));
        }

        if (sourceTrustClass == SourceTrustClass.OfficialExternal &&
            (officialSourceRegistrationId is null || sourceSnapshotId is null ||
             sourceObservationId is null))
        {
            throw new ArgumentException(
                "An official citation requires registration, snapshot, and observation identities.",
                nameof(sourceTrustClass));
        }

        if (documentFormat == DocumentFormat.Pdf)
        {
            if (pageStart is null || pageEnd is null || pageStart <= 0 ||
                pageEnd < pageStart || renderManifestId is null ||
                recordStart is not null || recordEnd is not null || canonicalColumns.Length != 0)
            {
                throw new ArgumentException(
                    "A PDF citation requires a positive page range and render manifest, and prohibits CSV locations.");
            }
        }
        else if (pageStart is not null || pageEnd is not null || renderManifestId is not null ||
                 (recordStart is null) != (recordEnd is null) || recordStart <= 0 ||
                 recordEnd < recordStart)
        {
            throw new ArgumentException(
                "A CSV citation prohibits PDF locations and requires a complete positive record range.");
        }

        Ordinal = ordinal;
        DatabaseProductId = databaseProductId;
        DatabaseProductRevision = databaseProductRevision;
        DocumentId = documentId;
        DocumentVersion = documentVersion;
        DocumentFormat = documentFormat;
        ContentLanguage = contentLanguage;
        ChunkId = chunkId;
        SourceAdapterId = sourceAdapterId;
        SourceTrustClass = sourceTrustClass;
        OfficialSourceRegistrationId = officialSourceRegistrationId;
        SourceSnapshotId = sourceSnapshotId;
        SourceObservationId = sourceObservationId;
        SourceContentObjectId = sourceContentObjectId;
        PageStart = pageStart;
        PageEnd = pageEnd;
        RecordStart = recordStart;
        RecordEnd = recordEnd;
        Columns = Array.AsReadOnly(canonicalColumns);
        SectionLocator = sectionLocator;
        RenderManifestId = renderManifestId;
    }

    public int Ordinal { get; }

    public DatabaseProductId DatabaseProductId { get; }

    public DatabaseProductRevision DatabaseProductRevision { get; }

    public DocumentId DocumentId { get; }

    public DocumentVersionNumber DocumentVersion { get; }

    public DocumentFormat DocumentFormat { get; }

    public DocumentContentLanguage ContentLanguage { get; }

    public string ChunkId { get; }

    public SourceAdapterId SourceAdapterId { get; }

    public SourceTrustClass SourceTrustClass { get; }

    public OfficialSourceRegistrationId? OfficialSourceRegistrationId { get; }

    public OfficialSnapshotId? SourceSnapshotId { get; }

    public OfficialObservationId? SourceObservationId { get; }

    public ContentObjectId SourceContentObjectId { get; }

    public int? PageStart { get; }

    public int? PageEnd { get; }

    public long? RecordStart { get; }

    public long? RecordEnd { get; }

    public ReadOnlyCollection<string> Columns { get; }

    public string? SectionLocator { get; }

    public RenderManifestId? RenderManifestId { get; }

    private static string[] CanonicaliseColumns(IEnumerable<string>? columns)
    {
        var materialised = columns?.ToArray() ?? [];

        if (materialised.Length > 64 || materialised.Any(column =>
                string.IsNullOrWhiteSpace(column) || column.Length > 128 ||
                column.Any(character =>
                    !char.IsAsciiLetterOrDigit(character) &&
                    character is not '.' and not '_' and not ':' and not '-')) ||
            materialised.Distinct(StringComparer.Ordinal).Count() != materialised.Length)
        {
            throw new ArgumentException(
                "CSV column names must be unique bounded safe ASCII values.",
                nameof(columns));
        }

        var canonical = materialised.Order(StringComparer.Ordinal).ToArray();
        var canonicalJsonLength = 2 + canonical.Sum(column => column.Length + 2) +
            Math.Max(0, canonical.Length - 1);

        if (canonicalJsonLength > 8192)
        {
            throw new ArgumentException(
                "Canonical CSV column metadata must fit the bounded persistence field.",
                nameof(columns));
        }

        return canonical;
    }

    private static string RequireSafeIdentifier(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);

        if (value.Length > 128 || value.Any(character =>
                !char.IsAsciiLetterOrDigit(character) &&
                character is not '.' and not '_' and not ':' and not '-'))
        {
            throw new ArgumentException(
                "A citation chunk identity must be bounded safe ASCII.",
                parameterName);
        }

        return value;
    }
}

public sealed class AnswerEvidencePageBindingV1
{
    public AnswerEvidencePageBindingV1(
        DocumentId documentId,
        DocumentVersionNumber documentVersion,
        ContentObjectId sourceContentObjectId,
        int pageNumber,
        RenderManifestId renderManifestId,
        RenderProfileId renderProfileId,
        RendererDescriptor rendererDescriptor,
        ContentObjectId imageContentObjectId,
        ImageSha256 imageSha256,
        long byteLength,
        string mediaType,
        int widthPixels,
        int heightPixels)
    {
        ArgumentNullException.ThrowIfNull(documentId);
        ArgumentNullException.ThrowIfNull(documentVersion);
        ArgumentNullException.ThrowIfNull(sourceContentObjectId);
        ArgumentNullException.ThrowIfNull(renderManifestId);
        ArgumentNullException.ThrowIfNull(renderProfileId);
        ArgumentNullException.ThrowIfNull(rendererDescriptor);
        ArgumentNullException.ThrowIfNull(imageContentObjectId);
        ArgumentNullException.ThrowIfNull(imageSha256);

        if (pageNumber <= 0 || byteLength <= 0 ||
            widthPixels is <= 0 or > DocumentPageImage.MaximumDimensionPixels ||
            heightPixels is <= 0 or > DocumentPageImage.MaximumDimensionPixels ||
            imageContentObjectId.Value != imageSha256.Value ||
            !string.Equals(mediaType, DocumentPageImage.PngMediaType, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "An answer-evidence page must retain a bounded exact PNG binding.");
        }

        DocumentId = documentId;
        DocumentVersion = documentVersion;
        SourceContentObjectId = sourceContentObjectId;
        PageNumber = pageNumber;
        RenderManifestId = renderManifestId;
        RenderProfileId = renderProfileId;
        RendererDescriptor = rendererDescriptor;
        ImageContentObjectId = imageContentObjectId;
        ImageSha256 = imageSha256;
        ByteLength = byteLength;
        MediaType = mediaType;
        WidthPixels = widthPixels;
        HeightPixels = heightPixels;
    }

    public DocumentId DocumentId { get; }

    public DocumentVersionNumber DocumentVersion { get; }

    public ContentObjectId SourceContentObjectId { get; }

    public int PageNumber { get; }

    public RenderManifestId RenderManifestId { get; }

    public RenderProfileId RenderProfileId { get; }

    public RendererDescriptor RendererDescriptor { get; }

    public ContentObjectId ImageContentObjectId { get; }

    public ImageSha256 ImageSha256 { get; }

    public long ByteLength { get; }

    public string MediaType { get; }

    public int WidthPixels { get; }

    public int HeightPixels { get; }
}

public sealed class AnswerEvidenceRecordV1
{
    public const int CurrentSchemaVersion = 1;
    public const string CanonicalDomain = "rag-challenge/answer-evidence-record/v1";
    public const string RetentionPolicy = "answer-evidence-p30d-v1";
    public static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(30);

    private AnswerEvidenceRecordV1(
        AnswerEvidenceRecordId answerEvidenceRecordId,
        CorpusId corpusId,
        ActivationRecordRevision activationRecordRevision,
        CatalogueRevision catalogueRevision,
        SourceBindingSetDigest sourceBindingSetDigest,
        ActivationBindingSetDigest activationBindingSetDigest,
        IndexGenerationId indexGenerationId,
        SupportedQueryLanguage questionLanguage,
        AnswerSha256 answerSha256,
        int answerUtf8ByteLength,
        EvidenceCoverageDigest evidenceCoverageDigest,
        string retrievalPolicyVersion,
        string promptVersion,
        AnswerLanguageModelDescriptorV1 languageModelDescriptor,
        string correlationId,
        DateTimeOffset createdAt,
        IEnumerable<AnswerEvidenceCitationBindingV1> citations,
        IEnumerable<AnswerEvidencePageBindingV1> pageImages)
    {
        ArgumentNullException.ThrowIfNull(answerEvidenceRecordId);
        ArgumentNullException.ThrowIfNull(corpusId);
        ArgumentNullException.ThrowIfNull(activationRecordRevision);
        ArgumentNullException.ThrowIfNull(catalogueRevision);
        ArgumentNullException.ThrowIfNull(sourceBindingSetDigest);
        ArgumentNullException.ThrowIfNull(activationBindingSetDigest);
        ArgumentNullException.ThrowIfNull(indexGenerationId);
        ArgumentNullException.ThrowIfNull(answerSha256);
        ArgumentNullException.ThrowIfNull(evidenceCoverageDigest);
        ArgumentNullException.ThrowIfNull(languageModelDescriptor);
        ArgumentNullException.ThrowIfNull(citations);
        ArgumentNullException.ThrowIfNull(pageImages);

        if (!Enum.IsDefined(questionLanguage) || answerUtf8ByteLength <= 0 ||
            createdAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "An answer-evidence record requires a supported language, positive answer length, and UTC creation instant.");
        }

        retrievalPolicyVersion = RequireVersion(retrievalPolicyVersion, nameof(retrievalPolicyVersion));
        promptVersion = RequireVersion(promptVersion, nameof(promptVersion));
        correlationId = RequireVersion(correlationId, nameof(correlationId));
        var orderedCitations = citations.OrderBy(citation => citation.Ordinal).ToArray();
        var orderedPages = pageImages.OrderBy(page => page.DocumentId.Value, StringComparer.Ordinal)
            .ThenBy(page => page.DocumentVersion.Value)
            .ThenBy(page => page.PageNumber)
            .ToArray();
        ValidateBindings(orderedCitations, orderedPages);

        SchemaVersion = CurrentSchemaVersion;
        AnswerEvidenceRecordId = answerEvidenceRecordId;
        CorpusId = corpusId;
        ActivationRecordRevision = activationRecordRevision;
        CatalogueRevision = catalogueRevision;
        SourceBindingSetDigest = sourceBindingSetDigest;
        ActivationBindingSetDigest = activationBindingSetDigest;
        IndexGenerationId = indexGenerationId;
        Outcome = "Answered";
        QuestionLanguage = questionLanguage;
        AnswerLanguage = questionLanguage;
        AnswerSha256 = answerSha256;
        AnswerUtf8ByteLength = answerUtf8ByteLength;
        EvidenceCoverageDigest = evidenceCoverageDigest;
        RetrievalPolicyVersion = retrievalPolicyVersion;
        PromptVersion = promptVersion;
        LanguageModelDescriptor = languageModelDescriptor;
        CorrelationId = correlationId;
        RetentionPolicyId = RetentionPolicy;
        CreatedAt = createdAt;
        ExpiresAt = createdAt.Add(RetentionPeriod);
        Citations = Array.AsReadOnly(orderedCitations);
        PageImages = Array.AsReadOnly(orderedPages);
        CanonicalUtf8 = SerialiseCanonical();
        RecordSha256 = new AnswerEvidenceRecordSha256(
            Convert.ToHexString(SHA256.HashData(CanonicalUtf8)).ToLowerInvariant());
    }

    public int SchemaVersion { get; }

    public AnswerEvidenceRecordId AnswerEvidenceRecordId { get; }

    public AnswerEvidenceRecordSha256 RecordSha256 { get; }

    public CorpusId CorpusId { get; }

    public ActivationRecordRevision ActivationRecordRevision { get; }

    public CatalogueRevision CatalogueRevision { get; }

    public SourceBindingSetDigest SourceBindingSetDigest { get; }

    public ActivationBindingSetDigest ActivationBindingSetDigest { get; }

    public IndexGenerationId IndexGenerationId { get; }

    public string Outcome { get; }

    public SupportedQueryLanguage QuestionLanguage { get; }

    public SupportedQueryLanguage AnswerLanguage { get; }

    public AnswerSha256 AnswerSha256 { get; }

    public int AnswerUtf8ByteLength { get; }

    public EvidenceCoverageDigest EvidenceCoverageDigest { get; }

    public string RetrievalPolicyVersion { get; }

    public string PromptVersion { get; }

    public AnswerLanguageModelDescriptorV1 LanguageModelDescriptor { get; }

    public string CorrelationId { get; }

    public string RetentionPolicyId { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset ExpiresAt { get; }

    public ReadOnlyCollection<AnswerEvidenceCitationBindingV1> Citations { get; }

    public ReadOnlyCollection<AnswerEvidencePageBindingV1> PageImages { get; }

    private byte[] CanonicalUtf8 { get; }

    public static AnswerEvidenceRecordV1 Create(
        AnswerEvidenceRecordId answerEvidenceRecordId,
        CorpusId corpusId,
        ActivationRecordRevision activationRecordRevision,
        CatalogueRevision catalogueRevision,
        SourceBindingSetDigest sourceBindingSetDigest,
        ActivationBindingSetDigest activationBindingSetDigest,
        IndexGenerationId indexGenerationId,
        SupportedQueryLanguage questionLanguage,
        AnswerSha256 answerSha256,
        int answerUtf8ByteLength,
        EvidenceCoverageDigest evidenceCoverageDigest,
        string retrievalPolicyVersion,
        string promptVersion,
        AnswerLanguageModelDescriptorV1 languageModelDescriptor,
        string correlationId,
        DateTimeOffset createdAt,
        IEnumerable<AnswerEvidenceCitationBindingV1> citations,
        IEnumerable<AnswerEvidencePageBindingV1> pageImages) =>
        new(
            answerEvidenceRecordId,
            corpusId,
            activationRecordRevision,
            catalogueRevision,
            sourceBindingSetDigest,
            activationBindingSetDigest,
            indexGenerationId,
            questionLanguage,
            answerSha256,
            answerUtf8ByteLength,
            evidenceCoverageDigest,
            retrievalPolicyVersion,
            promptVersion,
            languageModelDescriptor,
            correlationId,
            createdAt,
            citations,
            pageImages);

    public static AnswerEvidenceRecordV1 Rehydrate(
        AnswerEvidenceRecordSha256 expectedRecordSha256,
        AnswerEvidenceRecordId answerEvidenceRecordId,
        CorpusId corpusId,
        ActivationRecordRevision activationRecordRevision,
        CatalogueRevision catalogueRevision,
        SourceBindingSetDigest sourceBindingSetDigest,
        ActivationBindingSetDigest activationBindingSetDigest,
        IndexGenerationId indexGenerationId,
        SupportedQueryLanguage questionLanguage,
        AnswerSha256 answerSha256,
        int answerUtf8ByteLength,
        EvidenceCoverageDigest evidenceCoverageDigest,
        string retrievalPolicyVersion,
        string promptVersion,
        AnswerLanguageModelDescriptorV1 languageModelDescriptor,
        string correlationId,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        IEnumerable<AnswerEvidenceCitationBindingV1> citations,
        IEnumerable<AnswerEvidencePageBindingV1> pageImages)
    {
        ArgumentNullException.ThrowIfNull(expectedRecordSha256);
        var record = Create(
            answerEvidenceRecordId,
            corpusId,
            activationRecordRevision,
            catalogueRevision,
            sourceBindingSetDigest,
            activationBindingSetDigest,
            indexGenerationId,
            questionLanguage,
            answerSha256,
            answerUtf8ByteLength,
            evidenceCoverageDigest,
            retrievalPolicyVersion,
            promptVersion,
            languageModelDescriptor,
            correlationId,
            createdAt,
            citations,
            pageImages);

        if (record.ExpiresAt != expiresAt || record.RecordSha256 != expectedRecordSha256)
        {
            throw new InvalidDataException(
                "A persisted answer-evidence record differs from its fixed retention or canonical digest.");
        }

        return record;
    }

    public byte[] SerialiseCanonicalUtf8() => [.. CanonicalUtf8];

    private static void ValidateBindings(
        AnswerEvidenceCitationBindingV1[] citations,
        AnswerEvidencePageBindingV1[] pages)
    {
        if (citations.Length == 0 || citations.Any(citation => citation is null) ||
            citations.Select(citation => citation.Ordinal).Distinct().Count() != citations.Length ||
            citations.Where((citation, index) => citation.Ordinal != index + 1).Any() ||
            pages.Any(page => page is null) ||
            pages.Select(page => (page.DocumentId, page.DocumentVersion, page.PageNumber))
                .Distinct().Count() != pages.Length)
        {
            throw new ArgumentException(
                "Answer-evidence citations must be non-empty, unique, consecutive, and page bindings unique.");
        }

        var requiredPages = new HashSet<(DocumentId, DocumentVersionNumber, int)>();

        foreach (var citation in citations.Where(citation =>
                     citation.DocumentFormat == DocumentFormat.Pdf))
        {
            for (var pageNumber = citation.PageStart!.Value;
                 pageNumber <= citation.PageEnd!.Value;
                 pageNumber++)
            {
                requiredPages.Add((citation.DocumentId, citation.DocumentVersion, pageNumber));
                var page = pages.SingleOrDefault(item =>
                    item.DocumentId == citation.DocumentId &&
                    item.DocumentVersion == citation.DocumentVersion &&
                    item.PageNumber == pageNumber);

                if (page is null || page.SourceContentObjectId != citation.SourceContentObjectId ||
                    page.RenderManifestId != citation.RenderManifestId)
                {
                    throw new ArgumentException(
                        "Every cited PDF page must bind the exact source and render manifest.",
                        nameof(pages));
                }
            }
        }

        if (pages.Any(page => !requiredPages.Contains(
                (page.DocumentId, page.DocumentVersion, page.PageNumber))))
        {
            throw new ArgumentException(
                "Answer-evidence page bindings cannot exceed the cited physical PDF pages.",
                nameof(pages));
        }
    }

    private byte[] SerialiseCanonical()
    {
        var target = new StringBuilder();
        Append(target, "domain", CanonicalDomain);
        Append(target, "schemaVersion", SchemaVersion.ToString(CultureInfo.InvariantCulture));
        Append(target, "answerEvidenceRecordId", AnswerEvidenceRecordId.Value);
        Append(target, "corpusId", CorpusId.Value);
        Append(target, "activationRecordRevision", ActivationRecordRevision.ToCanonicalString());
        Append(target, "catalogueRevision", CatalogueRevision.ToCanonicalString());
        Append(target, "sourceBindingSetDigest", SourceBindingSetDigest.Value);
        Append(target, "activationBindingSetDigest", ActivationBindingSetDigest.Value);
        Append(target, "indexGenerationId", IndexGenerationId.Value);
        Append(target, "outcome", Outcome);
        Append(target, "questionLanguage", QuestionLanguage.ToCanonicalTag());
        Append(target, "answerLanguage", AnswerLanguage.ToCanonicalTag());
        Append(target, "answerSha256", AnswerSha256.Value);
        Append(target, "answerUtf8ByteLength", AnswerUtf8ByteLength.ToString(CultureInfo.InvariantCulture));
        Append(target, "evidenceCoverageDigest", EvidenceCoverageDigest.Value);
        Append(target, "retrievalPolicyVersion", RetrievalPolicyVersion);
        Append(target, "promptVersion", PromptVersion);
        Append(target, "languageModel.providerId", LanguageModelDescriptor.ProviderId);
        Append(target, "languageModel.modelId", LanguageModelDescriptor.ModelId);
        Append(target, "languageModel.modelRevision", LanguageModelDescriptor.ModelRevision);
        Append(target, "correlationId", CorrelationId);
        Append(target, "retentionPolicyId", RetentionPolicyId);
        Append(target, "createdAt", CreatedAt.ToString("O", CultureInfo.InvariantCulture));
        Append(target, "expiresAt", ExpiresAt.ToString("O", CultureInfo.InvariantCulture));
        Append(target, "citationCount", Citations.Count.ToString(CultureInfo.InvariantCulture));

        foreach (var citation in Citations)
        {
            Append(target, "citation.ordinal", citation.Ordinal.ToString(CultureInfo.InvariantCulture));
            Append(target, "citation.databaseProductId", citation.DatabaseProductId.Value);
            Append(target, "citation.databaseProductRevision", citation.DatabaseProductRevision.ToCanonicalString());
            Append(target, "citation.documentId", citation.DocumentId.Value);
            Append(target, "citation.documentVersion", citation.DocumentVersion.ToCanonicalString());
            Append(target, "citation.documentFormat", citation.DocumentFormat.ToString());
            Append(target, "citation.contentLanguage", citation.ContentLanguage.CanonicalTag);
            Append(target, "citation.chunkId", citation.ChunkId);
            Append(target, "citation.sourceAdapterId", citation.SourceAdapterId.Value);
            Append(target, "citation.sourceTrustClass", citation.SourceTrustClass.ToString());
            Append(target, "citation.officialSourceRegistrationId", citation.OfficialSourceRegistrationId?.Value);
            Append(target, "citation.sourceSnapshotId", citation.SourceSnapshotId?.Value);
            Append(target, "citation.sourceObservationId", citation.SourceObservationId?.Value);
            Append(target, "citation.sourceContentObjectId", citation.SourceContentObjectId.Value);
            Append(target, "citation.pageStart", Canonical(citation.PageStart));
            Append(target, "citation.pageEnd", Canonical(citation.PageEnd));
            Append(target, "citation.recordStart", Canonical(citation.RecordStart));
            Append(target, "citation.recordEnd", Canonical(citation.RecordEnd));
            Append(target, "citation.columnCount", citation.Columns.Count.ToString(CultureInfo.InvariantCulture));

            foreach (var column in citation.Columns)
            {
                Append(target, "citation.column", column);
            }

            Append(target, "citation.sectionLocator", citation.SectionLocator);
            Append(target, "citation.renderManifestId", citation.RenderManifestId?.Value);
        }

        Append(target, "pageImageCount", PageImages.Count.ToString(CultureInfo.InvariantCulture));

        foreach (var page in PageImages)
        {
            Append(target, "page.documentId", page.DocumentId.Value);
            Append(target, "page.documentVersion", page.DocumentVersion.ToCanonicalString());
            Append(target, "page.sourceContentObjectId", page.SourceContentObjectId.Value);
            Append(target, "page.pageNumber", page.PageNumber.ToString(CultureInfo.InvariantCulture));
            Append(target, "page.renderManifestId", page.RenderManifestId.Value);
            Append(target, "page.renderProfileId", page.RenderProfileId.Value);
            Append(target, "page.rendererDescriptor", page.RendererDescriptor.Value);
            Append(target, "page.imageContentObjectId", page.ImageContentObjectId.Value);
            Append(target, "page.imageSha256", page.ImageSha256.Value);
            Append(target, "page.byteLength", page.ByteLength.ToString(CultureInfo.InvariantCulture));
            Append(target, "page.mediaType", page.MediaType);
            Append(target, "page.widthPixels", page.WidthPixels.ToString(CultureInfo.InvariantCulture));
            Append(target, "page.heightPixels", page.HeightPixels.ToString(CultureInfo.InvariantCulture));
        }

        return Encoding.UTF8.GetBytes(target.ToString());
    }

    private static void Append(StringBuilder target, string name, string? value)
    {
        target.Append(name);
        target.Append(':');

        if (value is null)
        {
            target.Append("-1:\n");
            return;
        }

        target.Append(Encoding.UTF8.GetByteCount(value).ToString(CultureInfo.InvariantCulture));
        target.Append(':');
        target.Append(value);
        target.Append('\n');
    }

    private static string? Canonical<T>(T? value)
        where T : struct, IFormattable =>
        value?.ToString(null, CultureInfo.InvariantCulture);

    private static string RequireVersion(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);

        if (value.Length > 128 || value.Any(character =>
                !char.IsAsciiLetterOrDigit(character) &&
                character is not '.' and not '_' and not ':' and not '-'))
        {
            throw new ArgumentException(
                "A record version or correlation identity must be bounded safe ASCII.",
                parameterName);
        }

        return value;
    }
}
