// Purpose: Verifies the versioned document-rights contract and its fail-closed textual and visual eligibility gates using synthetic records only.
using RagChallenge.Application.Documents;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.UnitTests;

public sealed class DocumentRightsEligibilityTests
{
    public static TheoryData<DocumentRight> AllRights =>
        new(Enum.GetValues<DocumentRight>());

    [Fact]
    public void RecordRequiresEveryIndependentRightExactlyOnce()
    {
        var decisions = Decisions(DocumentRightDecisionState.Permitted);
        var record = Record(decisions);

        Assert.Equal(
            DocumentRightsEligibilityRecordV1.CurrentSchemaVersion,
            record.SchemaVersion);
        Assert.Equal("document-synthetic", record.DocumentId.Value);
        Assert.Equal(1, record.DocumentVersion.Value);
        Assert.Equal(Enum.GetValues<DocumentRight>().Length, record.Decisions.Count);
        Assert.Equal(
            Enum.GetValues<DocumentRight>(),
            record.Decisions.Select(decision => decision.Right));

        Assert.Throws<ArgumentException>(() => Record(decisions.Skip(1)));
        Assert.Throws<ArgumentException>(() => Record([.. decisions, decisions[0]]));
    }

    [Fact]
    public void DecisionsRequireClosedStatesAndStableEvidenceReferences()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DocumentRightDecision(
            (DocumentRight)int.MaxValue,
            DocumentRightDecisionState.Permitted,
            Evidence(DocumentRight.SourcePossessionOrDownload)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new DocumentRightDecision(
            DocumentRight.SourcePossessionOrDownload,
            (DocumentRightDecisionState)int.MaxValue,
            Evidence(DocumentRight.SourcePossessionOrDownload)));
        Assert.Throws<ArgumentNullException>(() => new DocumentRightDecision(
            DocumentRight.SourcePossessionOrDownload,
            DocumentRightDecisionState.Permitted,
            null!));
        Assert.Throws<ArgumentException>(() => new DocumentRightsEvidenceReference(
            "C:\\rights\\licence.txt"));
    }

    [Theory]
    [MemberData(nameof(AllRights))]
    public void IndividualRightsFailClosedUnlessExplicitlyPermitted(DocumentRight right)
    {
        Assert.True(DocumentRightsEligibilityPolicy.IsPermitted(
            Record(Decisions(DocumentRightDecisionState.Permitted)),
            right));
        Assert.False(DocumentRightsEligibilityPolicy.IsPermitted(
            Record(Decisions(DocumentRightDecisionState.Permitted, right, DocumentRightDecisionState.Denied)),
            right));
        Assert.False(DocumentRightsEligibilityPolicy.IsPermitted(
            Record(Decisions(DocumentRightDecisionState.Permitted, right, DocumentRightDecisionState.Unproven)),
            right));
    }

    [Fact]
    public void TextualEvidenceRequiresEveryTextualDecisionAndNoticeCompliance()
    {
        var visualAndDistributionDenied = Enum.GetValues<DocumentRight>()
            .ToDictionary(
                right => right,
                right => right is DocumentRight.PageRendering or
                    DocumentRight.DerivativeImageCreationAndRetention or
                    DocumentRight.RuntimeDerivativeImageDisplay or
                    DocumentRight.SourceAndDerivativeByteDistributionOrPublication
                        ? DocumentRightDecisionState.Denied
                        : DocumentRightDecisionState.Permitted);
        var eligible = DocumentRightsEligibilityPolicy.Evaluate(
            Record(Decisions(visualAndDistributionDenied)),
            DocumentRightsEligibilityGate.TextualEvidence);

        Assert.True(eligible.IsEligible);
        Assert.Equal("document-synthetic", eligible.DocumentId.Value);
        Assert.Equal(1, eligible.DocumentVersion.Value);
        Assert.Equal(DocumentRightsEligibilityRecordV1.CurrentSchemaVersion, eligible.RightsSchemaVersion);
        Assert.Empty(eligible.BlockingDecisions);
        Assert.DoesNotContain(
            eligible.RequiredDecisions,
            decision => decision.Right ==
                DocumentRight.SourceAndDerivativeByteDistributionOrPublication);

        foreach (var right in eligible.RequiredDecisions.Select(decision => decision.Right))
        {
            var blocked = DocumentRightsEligibilityPolicy.Evaluate(
                Record(Decisions(
                    DocumentRightDecisionState.Permitted,
                    right,
                    DocumentRightDecisionState.Unproven)),
                DocumentRightsEligibilityGate.TextualEvidence);

            var decision = Assert.Single(blocked.BlockingDecisions);
            Assert.False(blocked.IsEligible);
            Assert.Equal(right, decision.Right);
            Assert.Equal(DocumentRightDecisionState.Unproven, decision.State);
        }
    }

    [Fact]
    public void PdfVisualEvidenceRequiresTextualRenderingDerivativeAndDisplayRights()
    {
        var distributionDenied = Decisions(
            DocumentRightDecisionState.Permitted,
            DocumentRight.SourceAndDerivativeByteDistributionOrPublication,
            DocumentRightDecisionState.Denied);
        var eligible = DocumentRightsEligibilityPolicy.Evaluate(
            Record(distributionDenied),
            DocumentRightsEligibilityGate.PdfVisualEvidence);

        Assert.True(eligible.IsEligible);
        Assert.DoesNotContain(
            eligible.RequiredDecisions,
            decision => decision.Right ==
                DocumentRight.SourceAndDerivativeByteDistributionOrPublication);
        Assert.False(DocumentRightsEligibilityPolicy.IsPermitted(
            Record(distributionDenied),
            DocumentRight.SourceAndDerivativeByteDistributionOrPublication));

        foreach (var state in new[]
        {
            DocumentRightDecisionState.Denied,
            DocumentRightDecisionState.Unproven,
        })
        {
            foreach (var right in eligible.RequiredDecisions.Select(decision => decision.Right))
            {
                var blocked = DocumentRightsEligibilityPolicy.Evaluate(
                    Record(Decisions(DocumentRightDecisionState.Permitted, right, state)),
                    DocumentRightsEligibilityGate.PdfVisualEvidence);

                var decision = Assert.Single(blocked.BlockingDecisions);
                Assert.False(blocked.IsEligible);
                Assert.Equal(right, decision.Right);
                Assert.Equal(state, decision.State);
            }
        }
    }

    [Theory]
    [InlineData(DocumentRightDecisionState.Permitted, true)]
    [InlineData(DocumentRightDecisionState.Denied, true)]
    [InlineData(DocumentRightDecisionState.Unproven, false)]
    public void PdfVisualEvidenceServingRequiresAProvenDistributionBoundary(
        DocumentRightDecisionState distributionState,
        bool expectedEligibility)
    {
        var result = DocumentRightsEligibilityPolicy.Evaluate(
            Record(Decisions(
                DocumentRightDecisionState.Permitted,
                DocumentRight.SourceAndDerivativeByteDistributionOrPublication,
                distributionState)),
            DocumentRightsEligibilityGate.PdfVisualEvidenceServing);

        Assert.Equal(expectedEligibility, result.IsEligible);
        Assert.Equal(
            Enum.GetValues<DocumentRight>().Length,
            result.RequiredDecisions.Select(decision => decision.Right).Distinct().Count());
        Assert.Contains(
            result.RequiredDecisions,
            decision => decision.Right ==
                DocumentRight.SourceAndDerivativeByteDistributionOrPublication);
        Assert.Equal(
            distributionState == DocumentRightDecisionState.Unproven,
            result.BlockingDecisions.Any(decision => decision.Right ==
                DocumentRight.SourceAndDerivativeByteDistributionOrPublication));
    }

    [Theory]
    [InlineData(DocumentRightDecisionState.Denied)]
    [InlineData(DocumentRightDecisionState.Unproven)]
    public void PdfVisualEvidenceServingRequiresPermittedRuntimeDisplay(
        DocumentRightDecisionState runtimeDisplayState)
    {
        var result = DocumentRightsEligibilityPolicy.Evaluate(
            Record(Decisions(
                DocumentRightDecisionState.Permitted,
                DocumentRight.RuntimeDerivativeImageDisplay,
                runtimeDisplayState)),
            DocumentRightsEligibilityGate.PdfVisualEvidenceServing);

        var blocking = Assert.Single(result.BlockingDecisions);
        Assert.False(result.IsEligible);
        Assert.Equal(DocumentRight.RuntimeDerivativeImageDisplay, blocking.Right);
        Assert.Equal(runtimeDisplayState, blocking.State);
    }

    private static DocumentRightsEligibilityRecordV1 Record(
        IEnumerable<DocumentRightDecision> decisions) =>
        new(
            new DocumentId("document-synthetic"),
            new DocumentVersionNumber(1),
            decisions);

    private static DocumentRightDecision[] Decisions(
        DocumentRightDecisionState defaultState,
        DocumentRight? overriddenRight = null,
        DocumentRightDecisionState? overriddenState = null) =>
        Decisions(
            Enum.GetValues<DocumentRight>().ToDictionary(
                right => right,
                right => right == overriddenRight
                    ? overriddenState!.Value
                    : defaultState));

    private static DocumentRightDecision[] Decisions(
        Dictionary<DocumentRight, DocumentRightDecisionState> states) =>
        Enum.GetValues<DocumentRight>()
            .Select(right => new DocumentRightDecision(
                right,
                states[right],
                Evidence(right)))
            .ToArray();

    private static DocumentRightsEvidenceReference Evidence(DocumentRight right) =>
        new($"rights-evidence-synthetic-{right}");
}
