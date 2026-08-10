// Purpose: Applies fixed fail-closed eligibility gates to versioned document-rights records while leaving persistence, activation, rendering, and distribution execution to separately authorised use cases.
using System.Collections.ObjectModel;

using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Application.Documents;

public enum DocumentRightsEligibilityGate
{
    TextualEvidence,
    PdfVisualEvidence,
    PdfVisualEvidenceServing,
}

public sealed class DocumentRightsEligibilityResult
{
    internal DocumentRightsEligibilityResult(
        DocumentRightsEligibilityGate gate,
        DocumentRightsEligibilityRecordV1 record,
        IEnumerable<DocumentRightDecision> requiredDecisions,
        Func<DocumentRightDecision, bool>? isBlocking = null)
    {
        if (!Enum.IsDefined(gate))
        {
            throw new ArgumentOutOfRangeException(
                nameof(gate),
                gate,
                "A rights eligibility gate must belong to the closed application policy.");
        }

        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(requiredDecisions);

        var materialisedDecisions = requiredDecisions.ToArray();
        Gate = gate;
        RightsSchemaVersion = record.SchemaVersion;
        DocumentId = record.DocumentId;
        DocumentVersion = record.DocumentVersion;
        RequiredDecisions = Array.AsReadOnly(materialisedDecisions);
        isBlocking ??= static decision =>
            decision.State != DocumentRightDecisionState.Permitted;
        BlockingDecisions = Array.AsReadOnly(
            materialisedDecisions
                .Where(isBlocking)
                .ToArray());
    }

    public DocumentRightsEligibilityGate Gate { get; }

    public int RightsSchemaVersion { get; }

    public DocumentId DocumentId { get; }

    public DocumentVersionNumber DocumentVersion { get; }

    public bool IsEligible => BlockingDecisions.Count == 0;

    public ReadOnlyCollection<DocumentRightDecision> RequiredDecisions { get; }

    public ReadOnlyCollection<DocumentRightDecision> BlockingDecisions { get; }
}

public static class DocumentRightsEligibilityPolicy
{
    private static readonly DocumentRight[] TextualEvidenceRights =
    [
        DocumentRight.SourcePossessionOrDownload,
        DocumentRight.ParsingAndTextualTransformation,
        DocumentRight.Indexing,
        DocumentRight.SourceByteRetention,
        DocumentRight.QuotationAndCitation,
        DocumentRight.AttributionNoticeTrademarkAndChangeMarkingRequirements,
    ];

    private static readonly DocumentRight[] PdfVisualEvidenceRights =
    [
        .. TextualEvidenceRights,
        DocumentRight.PageRendering,
        DocumentRight.DerivativeImageCreationAndRetention,
        DocumentRight.RuntimeDerivativeImageDisplay,
    ];

    private static readonly DocumentRight[] PdfVisualEvidenceServingRights =
    [
        .. PdfVisualEvidenceRights,
        DocumentRight.SourceAndDerivativeByteDistributionOrPublication,
    ];

    public static bool IsPermitted(
        DocumentRightsEligibilityRecordV1 record,
        DocumentRight right)
    {
        ArgumentNullException.ThrowIfNull(record);
        return record.DecisionFor(right).State == DocumentRightDecisionState.Permitted;
    }

    public static DocumentRightsEligibilityResult Evaluate(
        DocumentRightsEligibilityRecordV1 record,
        DocumentRightsEligibilityGate gate)
    {
        ArgumentNullException.ThrowIfNull(record);

        var requiredRights = gate switch
        {
            DocumentRightsEligibilityGate.TextualEvidence => TextualEvidenceRights,
            DocumentRightsEligibilityGate.PdfVisualEvidence => PdfVisualEvidenceRights,
            DocumentRightsEligibilityGate.PdfVisualEvidenceServing =>
                PdfVisualEvidenceServingRights,
            _ => throw new ArgumentOutOfRangeException(
                nameof(gate),
                gate,
                "A rights eligibility gate must belong to the closed application policy."),
        };

        return new DocumentRightsEligibilityResult(
            gate,
            record,
            requiredRights.Select(record.DecisionFor),
            gate == DocumentRightsEligibilityGate.PdfVisualEvidenceServing
                ? IsPdfVisualEvidenceServingBlocked
                : null);
    }

    private static bool IsPdfVisualEvidenceServingBlocked(DocumentRightDecision decision) =>
        decision.Right == DocumentRight.SourceAndDerivativeByteDistributionOrPublication
            ? decision.State == DocumentRightDecisionState.Unproven
            : decision.State != DocumentRightDecisionState.Permitted;
}
