// Purpose: Models immutable notice-bearing derivative obligations and their deterministic binding to one complete ten-decision rights mapping; rendering and persistence remain outer-layer responsibilities.
using System.Collections.ObjectModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace RagChallenge.Domain.CorpusCatalog;

public enum DerivativeTrademarkTreatment
{
    Required,
    Prohibited,
    NotApplicable,
}

public static class DocumentRightsMappingRevisionV1
{
    private const string CanonicalDomain = "rag-challenge/document-rights-mapping/v1";

    public static RightsMappingRevision Create(DocumentRightsEligibilityRecordV1 rights)
    {
        ArgumentNullException.ThrowIfNull(rights);
        var canonical = new StringBuilder();
        Append(canonical, "canonicalDomain", CanonicalDomain);
        Append(canonical, "rightsSchemaVersion", rights.SchemaVersion.ToString(CultureInfo.InvariantCulture));
        Append(canonical, "documentId", rights.DocumentId.Value);
        Append(canonical, "documentVersion", rights.DocumentVersion.ToCanonicalString());

        foreach (var decision in rights.Decisions)
        {
            Append(canonical, "decision.right", decision.Right.ToString());
            Append(canonical, "decision.state", decision.State.ToString());
            Append(canonical, "decision.evidenceReference", decision.EvidenceReference.Value);
        }

        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString()));
        return new RightsMappingRevision(
            "rights-map-v1:" + Convert.ToHexString(digest).ToLowerInvariant());
    }

    private static void Append(StringBuilder target, string name, string value)
    {
        target.Append(name).Append(':')
            .Append(Encoding.UTF8.GetByteCount(value).ToString(CultureInfo.InvariantCulture))
            .Append(':').Append(value).Append('\n');
    }
}

public sealed class DerivativeObligationSetV1
{
    public const int CurrentSchemaVersion = 1;
    public const string PlacementModeValue = "VisibleInBinaryAndAccessibleContext";
    private const string CanonicalDomain = "rag-challenge/derivative-obligation-set/v1";

    private DerivativeObligationSetV1(
        DocumentId documentId,
        DocumentVersionNumber documentVersion,
        ContentObjectId sourceContentObjectId,
        RightsMappingRevision rightsMappingRevision,
        IEnumerable<DocumentRightsEvidenceReference> orderedEvidenceReferences,
        DocumentContentLanguage contentLanguage,
        string authoritativePublisherOrAuthor,
        string documentTitle,
        string documentVersionLabel,
        string sourceReference,
        string attributionText,
        string copyrightNotice,
        string permissionNotice,
        IEnumerable<string> orderedDisclaimers,
        DerivativeTrademarkTreatment trademarkTreatment,
        string trademarkOrNonEndorsementText,
        string changeMarkingText,
        DateTimeOffset assessedAt,
        string assessorId,
        DerivativeObligationSetSha256? expectedSha256)
    {
        ArgumentNullException.ThrowIfNull(documentId);
        ArgumentNullException.ThrowIfNull(documentVersion);
        ArgumentNullException.ThrowIfNull(sourceContentObjectId);
        ArgumentNullException.ThrowIfNull(rightsMappingRevision);
        ArgumentNullException.ThrowIfNull(orderedEvidenceReferences);
        ArgumentNullException.ThrowIfNull(contentLanguage);
        ArgumentNullException.ThrowIfNull(orderedDisclaimers);

        if (!Enum.IsDefined(trademarkTreatment) || assessedAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "A derivative obligation set requires a closed trademark treatment and UTC assessment instant.");
        }

        var evidence = orderedEvidenceReferences.ToArray();
        var disclaimers = orderedDisclaimers.ToArray();

        if (evidence.Length is <= 0 or > 32 || evidence.Any(item => item is null) ||
            evidence.Select(item => item.Value).Distinct(StringComparer.Ordinal).Count() != evidence.Length)
        {
            throw new ArgumentException(
                "Ordered obligation evidence must contain 1..32 unique stable references.",
                nameof(orderedEvidenceReferences));
        }

        if (disclaimers.Length > 16)
        {
            throw new ArgumentException(
                "A derivative obligation set may contain at most 16 ordered disclaimers.",
                nameof(orderedDisclaimers));
        }

        for (var index = 0; index < disclaimers.Length; index++)
        {
            disclaimers[index] = RequireExactText(
                disclaimers[index],
                8192,
                $"{nameof(orderedDisclaimers)}[{index}]");
        }

        SchemaVersion = CurrentSchemaVersion;
        DocumentId = documentId;
        DocumentVersion = documentVersion;
        SourceContentObjectId = sourceContentObjectId;
        RightsMappingRevision = rightsMappingRevision;
        OrderedEvidenceReferences = Array.AsReadOnly(evidence);
        ContentLanguage = contentLanguage;
        AuthoritativePublisherOrAuthor = RequireExactText(authoritativePublisherOrAuthor, 512, nameof(authoritativePublisherOrAuthor));
        DocumentTitle = RequireExactText(documentTitle, 512, nameof(documentTitle));
        DocumentVersionLabel = RequireExactText(documentVersionLabel, 128, nameof(documentVersionLabel));
        SourceReference = RequireExactText(sourceReference, 2048, nameof(sourceReference));
        AttributionText = RequireExactText(attributionText, 4096, nameof(attributionText));
        CopyrightNotice = RequireExactText(copyrightNotice, 8192, nameof(copyrightNotice));
        PermissionNotice = RequireExactText(permissionNotice, 8192, nameof(permissionNotice));
        OrderedDisclaimers = Array.AsReadOnly(disclaimers);
        TrademarkTreatment = trademarkTreatment;
        TrademarkOrNonEndorsementText = RequireExactText(
            trademarkOrNonEndorsementText,
            4096,
            nameof(trademarkOrNonEndorsementText));
        ChangeMarkingText = RequireExactText(changeMarkingText, 4096, nameof(changeMarkingText));
        PlacementMode = PlacementModeValue;
        AssessedAt = assessedAt;
        AssessorId = new StableObligationAssessorId(assessorId).Value;
        CanonicalSha256 = CalculateCanonicalSha256();

        if (expectedSha256 is not null && expectedSha256 != CanonicalSha256)
        {
            throw new InvalidDataException(
                "A persisted derivative-obligation digest does not match its canonical content.");
        }

        ObligationSetId = DerivativeObligationSetId.FromSha256(CanonicalSha256);
    }

    public int SchemaVersion { get; }

    public DerivativeObligationSetId ObligationSetId { get; }

    public DerivativeObligationSetSha256 CanonicalSha256 { get; }

    public DocumentId DocumentId { get; }

    public DocumentVersionNumber DocumentVersion { get; }

    public ContentObjectId SourceContentObjectId { get; }

    public RightsMappingRevision RightsMappingRevision { get; }

    public ReadOnlyCollection<DocumentRightsEvidenceReference> OrderedEvidenceReferences { get; }

    public DocumentContentLanguage ContentLanguage { get; }

    public string AuthoritativePublisherOrAuthor { get; }

    public string DocumentTitle { get; }

    public string DocumentVersionLabel { get; }

    public string SourceReference { get; }

    public string AttributionText { get; }

    public string CopyrightNotice { get; }

    public string PermissionNotice { get; }

    public ReadOnlyCollection<string> OrderedDisclaimers { get; }

    public DerivativeTrademarkTreatment TrademarkTreatment { get; }

    public string TrademarkOrNonEndorsementText { get; }

    public string ChangeMarkingText { get; }

    public string PlacementMode { get; }

    public DateTimeOffset AssessedAt { get; }

    public string AssessorId { get; }

    public static DerivativeObligationSetV1 Create(
        DocumentRightsEligibilityRecordV1 rights,
        ContentObjectId sourceContentObjectId,
        IEnumerable<DocumentRightsEvidenceReference> orderedEvidenceReferences,
        DocumentContentLanguage contentLanguage,
        string authoritativePublisherOrAuthor,
        string documentTitle,
        string documentVersionLabel,
        string sourceReference,
        string attributionText,
        string copyrightNotice,
        string permissionNotice,
        IEnumerable<string> orderedDisclaimers,
        DerivativeTrademarkTreatment trademarkTreatment,
        string trademarkOrNonEndorsementText,
        string changeMarkingText,
        DateTimeOffset assessedAt,
        string assessorId)
    {
        ArgumentNullException.ThrowIfNull(rights);
        var evidence = orderedEvidenceReferences?.ToArray() ??
            throw new ArgumentNullException(nameof(orderedEvidenceReferences));
        var missingEvidence = rights.Decisions
            .Select(decision => decision.EvidenceReference.Value)
            .Distinct(StringComparer.Ordinal)
            .Except(evidence.Select(item => item.Value), StringComparer.Ordinal)
            .Any();

        if (missingEvidence)
        {
            throw new ArgumentException(
                "The obligation set must retain every evidence reference used by the ten-decision rights mapping.",
                nameof(orderedEvidenceReferences));
        }

        return new DerivativeObligationSetV1(
            rights.DocumentId,
            rights.DocumentVersion,
            sourceContentObjectId,
            DocumentRightsMappingRevisionV1.Create(rights),
            evidence,
            contentLanguage,
            authoritativePublisherOrAuthor,
            documentTitle,
            documentVersionLabel,
            sourceReference,
            attributionText,
            copyrightNotice,
            permissionNotice,
            orderedDisclaimers,
            trademarkTreatment,
            trademarkOrNonEndorsementText,
            changeMarkingText,
            assessedAt,
            assessorId,
            expectedSha256: null);
    }

    public static DerivativeObligationSetV1 Rehydrate(
        DocumentId documentId,
        DocumentVersionNumber documentVersion,
        ContentObjectId sourceContentObjectId,
        RightsMappingRevision rightsMappingRevision,
        IEnumerable<DocumentRightsEvidenceReference> orderedEvidenceReferences,
        DocumentContentLanguage contentLanguage,
        string authoritativePublisherOrAuthor,
        string documentTitle,
        string documentVersionLabel,
        string sourceReference,
        string attributionText,
        string copyrightNotice,
        string permissionNotice,
        IEnumerable<string> orderedDisclaimers,
        DerivativeTrademarkTreatment trademarkTreatment,
        string trademarkOrNonEndorsementText,
        string changeMarkingText,
        DateTimeOffset assessedAt,
        string assessorId,
        DerivativeObligationSetSha256 expectedSha256) =>
        new(
            documentId,
            documentVersion,
            sourceContentObjectId,
            rightsMappingRevision,
            orderedEvidenceReferences,
            contentLanguage,
            authoritativePublisherOrAuthor,
            documentTitle,
            documentVersionLabel,
            sourceReference,
            attributionText,
            copyrightNotice,
            permissionNotice,
            orderedDisclaimers,
            trademarkTreatment,
            trademarkOrNonEndorsementText,
            changeMarkingText,
            assessedAt,
            assessorId,
            expectedSha256);

    public bool MatchesRights(DocumentRightsEligibilityRecordV1 rights)
    {
        ArgumentNullException.ThrowIfNull(rights);
        return rights.DocumentId == DocumentId &&
            rights.DocumentVersion == DocumentVersion &&
            DocumentRightsMappingRevisionV1.Create(rights) == RightsMappingRevision &&
            rights.Decisions.Select(decision => decision.EvidenceReference.Value)
                .Distinct(StringComparer.Ordinal)
                .All(reference => OrderedEvidenceReferences.Any(item => item.Value == reference));
    }

    public byte[] SerialiseCanonicalUtf8()
    {
        var canonical = new StringBuilder();
        Append(canonical, "canonicalDomain", CanonicalDomain);
        Append(canonical, "schemaVersion", CurrentSchemaVersion.ToString(CultureInfo.InvariantCulture));
        Append(canonical, "documentId", DocumentId.Value);
        Append(canonical, "documentVersion", DocumentVersion.ToCanonicalString());
        Append(canonical, "sourceContentObjectId", SourceContentObjectId.Value);
        Append(canonical, "rightsMappingRevision", RightsMappingRevision.Value);

        foreach (var reference in OrderedEvidenceReferences)
        {
            Append(canonical, "evidenceReference", reference.Value);
        }

        Append(canonical, "contentLanguage", ContentLanguage.ToCanonicalTag());
        Append(canonical, "authoritativePublisherOrAuthor", AuthoritativePublisherOrAuthor);
        Append(canonical, "documentTitle", DocumentTitle);
        Append(canonical, "documentVersionLabel", DocumentVersionLabel);
        Append(canonical, "sourceReference", SourceReference);
        Append(canonical, "attributionText", AttributionText);
        Append(canonical, "copyrightNotice", CopyrightNotice);
        Append(canonical, "permissionNotice", PermissionNotice);

        foreach (var disclaimer in OrderedDisclaimers)
        {
            Append(canonical, "disclaimer", disclaimer);
        }

        Append(canonical, "trademarkTreatment", TrademarkTreatment.ToString());
        Append(canonical, "trademarkOrNonEndorsementText", TrademarkOrNonEndorsementText);
        Append(canonical, "changeMarkingText", ChangeMarkingText);
        Append(canonical, "placementMode", PlacementMode);
        Append(canonical, "assessedAt", AssessedAt.ToString("O", CultureInfo.InvariantCulture));
        Append(canonical, "assessorId", AssessorId);
        return Encoding.UTF8.GetBytes(canonical.ToString());
    }

    private DerivativeObligationSetSha256 CalculateCanonicalSha256() =>
        new(Convert.ToHexString(SHA256.HashData(SerialiseCanonicalUtf8())).ToLowerInvariant());

    private static string RequireExactText(string value, int maximumLength, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);

        if (value.Length is 0 || value.Length > maximumLength ||
            !string.Equals(value, value.Normalize(NormalizationForm.FormC), StringComparison.Ordinal) ||
            !string.Equals(value, value.Trim(), StringComparison.Ordinal) ||
            value.Any(character => char.IsControl(character) && character != '\n'))
        {
            throw new ArgumentException(
                "Obligation text must be bounded, NFC, exact, trimmed, and free from unsupported control characters.",
                parameterName);
        }

        return value;
    }

    private static void Append(StringBuilder target, string name, string value)
    {
        target.Append(name).Append(':')
            .Append(Encoding.UTF8.GetByteCount(value).ToString(CultureInfo.InvariantCulture))
            .Append(':').Append(value).Append('\n');
    }

    private sealed record StableObligationAssessorId : StableIdentifier
    {
        internal StableObligationAssessorId(string value)
            : base(value, nameof(value))
        {
        }
    }
}
