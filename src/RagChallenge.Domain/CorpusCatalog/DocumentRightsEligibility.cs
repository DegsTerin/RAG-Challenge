// Purpose: Defines versioned, per-document rights decisions without embedding licence text, persistence, source data, or activation authority in Domain.
using System.Collections.ObjectModel;

namespace RagChallenge.Domain.CorpusCatalog;

public enum DocumentRight
{
    SourcePossessionOrDownload,
    ParsingAndTextualTransformation,
    Indexing,
    SourceByteRetention,
    QuotationAndCitation,
    PageRendering,
    DerivativeImageCreationAndRetention,
    RuntimeDerivativeImageDisplay,
    SourceAndDerivativeByteDistributionOrPublication,
    AttributionNoticeTrademarkAndChangeMarkingRequirements,
}

public enum DocumentRightDecisionState
{
    Permitted,
    Denied,
    Unproven,
}

public sealed record DocumentRightsEvidenceReference : StableIdentifier
{
    public DocumentRightsEvidenceReference(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record DocumentRightDecision
{
    public DocumentRightDecision(
        DocumentRight right,
        DocumentRightDecisionState state,
        DocumentRightsEvidenceReference evidenceReference)
    {
        if (!Enum.IsDefined(right))
        {
            throw new ArgumentOutOfRangeException(
                nameof(right),
                right,
                "A document right must belong to the closed rights contract.");
        }

        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "A rights decision must be Permitted, Denied, or Unproven.");
        }

        ArgumentNullException.ThrowIfNull(evidenceReference);

        Right = right;
        State = state;
        EvidenceReference = evidenceReference;
    }

    public DocumentRight Right { get; }

    public DocumentRightDecisionState State { get; }

    public DocumentRightsEvidenceReference EvidenceReference { get; }
}

public sealed class DocumentRightsEligibilityRecordV1
{
    public const int CurrentSchemaVersion = 1;

    private static readonly DocumentRight[] RequiredRights =
        Enum.GetValues<DocumentRight>();

    private readonly ReadOnlyDictionary<DocumentRight, DocumentRightDecision> decisionsByRight;

    public DocumentRightsEligibilityRecordV1(
        DocumentId documentId,
        DocumentVersionNumber documentVersion,
        IEnumerable<DocumentRightDecision> decisions)
    {
        ArgumentNullException.ThrowIfNull(documentId);
        ArgumentNullException.ThrowIfNull(documentVersion);
        ArgumentNullException.ThrowIfNull(decisions);

        var materialisedDecisions = decisions.ToArray();

        if (materialisedDecisions.Any(decision => decision is null))
        {
            throw new ArgumentException(
                "A document rights record cannot contain a null decision.",
                nameof(decisions));
        }

        var duplicateRight = materialisedDecisions
            .GroupBy(decision => decision.Right)
            .FirstOrDefault(group => group.Count() != 1);

        if (duplicateRight is not null)
        {
            throw new ArgumentException(
                $"A document rights record must decide '{duplicateRight.Key}' exactly once.",
                nameof(decisions));
        }

        var decisionsByRight = materialisedDecisions.ToDictionary(decision => decision.Right);
        var missingRights = RequiredRights
            .Where(right => !decisionsByRight.ContainsKey(right))
            .ToArray();

        if (missingRights.Length != 0)
        {
            throw new ArgumentException(
                "A document rights record must independently decide every right in schema version 1.",
                nameof(decisions));
        }

        DocumentId = documentId;
        DocumentVersion = documentVersion;
        SchemaVersion = CurrentSchemaVersion;
        Decisions = Array.AsReadOnly(
            RequiredRights.Select(right => decisionsByRight[right]).ToArray());
        this.decisionsByRight = new ReadOnlyDictionary<DocumentRight, DocumentRightDecision>(
            decisionsByRight);
    }

    public int SchemaVersion { get; }

    public DocumentId DocumentId { get; }

    public DocumentVersionNumber DocumentVersion { get; }

    public ReadOnlyCollection<DocumentRightDecision> Decisions { get; }

    public DocumentRightDecision DecisionFor(DocumentRight right)
    {
        if (!Enum.IsDefined(right))
        {
            throw new ArgumentOutOfRangeException(
                nameof(right),
                right,
                "A document right must belong to the closed rights contract.");
        }

        return decisionsByRight[right];
    }
}
