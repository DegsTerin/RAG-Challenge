// Purpose: Implements the typed notice-bearing render operation and projects a non-executed activation plan only from exact persisted render evidence.
using System.Text.Json;

using RagChallenge.Application.Administration;
using RagChallenge.Application.Documents;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;
using RagChallenge.Infrastructure.Persistence;

namespace RagChallenge.Server.Api.OperationsGovernance;

internal sealed class RenderDocumentAdministrativeCommand(
    DocumentRenderCandidateService renderService)
    : IAdministrativeMaterialisationCommand
{
    private readonly DocumentRenderCandidateService renderService = renderService ??
        throw new ArgumentNullException(nameof(renderService));

    public string CommandName => "render-document";

    public AdministrativeCommandIdentifiers DescribeIntent(
        CorpusId corpusId,
        JsonElement? input)
    {
        ArgumentNullException.ThrowIfNull(corpusId);
        var payload = ReadPayload(input);
        return new AdministrativeCommandIdentifiers(
            [$"content-object:{payload.SourceContentObjectId}"],
            [$"document-render:{payload.DocumentId}:{payload.DocumentVersion}"]);
    }

    public async Task<AdministrativeExecutionResult> ExecuteAsync(
        OneShotAdministrativeCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var payload = ReadPayload(command.Input);
            var rights = payload.Rights.ToDomain();
            var sourceContentObjectId = new ContentObjectId(payload.SourceContentObjectId);
            var obligationSet = payload.ObligationSet.ToDomain(rights, sourceContentObjectId);
            var result = await renderService.FinaliseAsync(
                new DocumentRenderCandidateRequest(
                    command.CorpusId,
                    new DocumentId(payload.DocumentId),
                    new DocumentVersionNumber(payload.DocumentVersion),
                    sourceContentObjectId,
                    payload.SourceByteLength,
                    rights,
                    payload.RenderPolicy.ToDomain(),
                    payload.GeneratedAt,
                    obligationSet),
                cancellationToken).ConfigureAwait(false);

            if (result.Manifest.RenderProfileId.Value != RenderProfileId.PdfPagePngNoticeV1 ||
                result.Manifest.ObligationSetId != obligationSet.ObligationSetId ||
                result.Manifest.ObligationSetSha256 != obligationSet.CanonicalSha256)
            {
                return Rejected();
            }

            var resultPayload = JsonSerializer.SerializeToElement(new
            {
                renderManifestId = result.Manifest.RenderManifestId.Value,
                manifestSha256 = result.Manifest.ManifestSha256.Value,
                renderProfileId = result.Manifest.RenderProfileId.Value,
                sourcePageCount = result.Manifest.SourcePageCount,
                obligationSetId = obligationSet.ObligationSetId.Value,
                obligationSetSha256 = obligationSet.CanonicalSha256.Value,
                rightsMappingRevision = obligationSet.RightsMappingRevision.Value,
            });
            return new AdministrativeExecutionResult(
                result.Outcome == StoreMutationOutcome.Applied
                    ? AdministrativeExecutionOutcome.Applied
                    : AdministrativeExecutionOutcome.AlreadyApplied,
                "CH_ADMIN_APPLIED",
                ResultPayload: resultPayload);
        }
        catch (DocumentRenderCandidateException)
        {
            return Rejected();
        }
        catch (Exception exception) when (
            exception is InvalidDataException or ArgumentException or OverflowException)
        {
            return Rejected();
        }
    }

    private static RenderDocumentPayload ReadPayload(JsonElement? input)
    {
        var payload = AdministrativeMaterialisationJson.Deserialize<RenderDocumentPayload>(input);
        _ = new DocumentId(payload.DocumentId);
        _ = new DocumentVersionNumber(payload.DocumentVersion);
        _ = new ContentObjectId(payload.SourceContentObjectId);

        if (payload.Rights is null || payload.RenderPolicy is null ||
            payload.ObligationSet is null || payload.GeneratedAt.Offset != TimeSpan.Zero ||
            payload.SourceByteLength <= 0 ||
            payload.Rights.DocumentId != payload.DocumentId ||
            payload.Rights.DocumentVersion != payload.DocumentVersion)
        {
            throw new InvalidDataException("The notice-bearing render plan is incomplete or drifted.");
        }

        return payload;
    }

    private static AdministrativeExecutionResult Rejected() =>
        new(AdministrativeExecutionOutcome.Rejected, "CH_ADMIN_VALIDATION_FAILED");
}

internal sealed class AdministrativeActivationPlanProjector(
    IDocumentRenderManifestStore manifestStore)
{
    private readonly IDocumentRenderManifestStore manifestStore = manifestStore ??
        throw new ArgumentNullException(nameof(manifestStore));

    internal async Task<ValidatedActivationProjection> ValidateAsync(
        CorpusId corpusId,
        IReadOnlyCollection<ActivationProjectionDocument> indexedDocuments,
        ActivationPlanProjectionPayload plan,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(corpusId);
        ArgumentNullException.ThrowIfNull(indexedDocuments);
        ArgumentNullException.ThrowIfNull(plan);

        if (plan.ExpectedCurrentRevision < 0 ||
            plan.PreviousGenerationRetentionDays <
                (int)SqliteControlPlaneStore.MinimumPreviousGenerationRetention.TotalDays ||
            plan.DocumentRenderManifests is null ||
            plan.DocumentRenderManifests.Length != indexedDocuments.Count)
        {
            throw new InvalidDataException("The activation-plan projection is incomplete.");
        }

        var requested = plan.DocumentRenderManifests.ToDictionary(
            item => (new DocumentId(item.DocumentId), new DocumentVersionNumber(item.DocumentVersion)));
        var evidence = new List<DocumentActivationEvidenceBinding>(indexedDocuments.Count);

        foreach (var indexed in indexedDocuments)
        {
            var binding = indexed.Binding;
            var rights = indexed.Rights;
            var source = indexed.SourceContentObjectId;

            if (!requested.Remove(
                    (binding.DocumentId, binding.DocumentVersion),
                    out var payload))
            {
                throw new InvalidDataException("The activation evidence set is incomplete.");
            }

            var renderManifestId = payload.RenderManifestId is null
                ? null
                : new RenderManifestId(payload.RenderManifestId);
            var item = new DocumentActivationEvidenceBinding(
                binding,
                source,
                rights,
                renderManifestId);

            if (binding.DocumentFormat == DocumentFormat.Pdf)
            {
                var manifest = await manifestStore.ReadAsync(
                    corpusId,
                    renderManifestId!,
                    cancellationToken).ConfigureAwait(false);

                if (manifest is null ||
                    manifest.RenderManifestId != renderManifestId ||
                    manifest.RenderProfileId.Value != RenderProfileId.PdfPagePngNoticeV1 ||
                    manifest.DocumentId != binding.DocumentId ||
                    manifest.DocumentVersion != binding.DocumentVersion ||
                    manifest.SourceContentObjectId != source ||
                    manifest.ObligationSetId is null ||
                    manifest.ObligationSetSha256 is null)
                {
                    throw new InvalidDataException("The exact notice-bearing render manifest was not persisted.");
                }

                var obligationSet = await manifestStore.ReadObligationSetAsync(
                    corpusId,
                    manifest.ObligationSetId,
                    cancellationToken).ConfigureAwait(false);

                if (obligationSet is null ||
                    obligationSet.ObligationSetId != manifest.ObligationSetId ||
                    obligationSet.CanonicalSha256 != manifest.ObligationSetSha256 ||
                    obligationSet.DocumentId != binding.DocumentId ||
                    obligationSet.DocumentVersion != binding.DocumentVersion ||
                    obligationSet.SourceContentObjectId != source ||
                    !obligationSet.MatchesRights(rights) ||
                    !DocumentRightsEligibilityPolicy.Evaluate(
                        rights,
                        DocumentRightsEligibilityGate.PdfVisualEvidence).IsEligible)
                {
                    throw new InvalidDataException("The persisted notice obligations diverged.");
                }
            }
            else if (!DocumentRightsEligibilityPolicy.Evaluate(
                rights,
                DocumentRightsEligibilityGate.TextualEvidence).IsEligible)
            {
                throw new InvalidDataException("The activation evidence is rights-ineligible.");
            }

            evidence.Add(item);
        }

        if (requested.Count != 0)
        {
            throw new InvalidDataException("The activation evidence set is incomplete.");
        }

        return new ValidatedActivationProjection(
            plan.ExpectedCurrentRevision,
            plan.PreviousGenerationRetentionDays,
            evidence);
    }

    internal static JsonElement Project(
        FinalisedIndexGenerationManifest manifest,
        ValidatedActivationProjection projection)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(projection);

        return JsonSerializer.SerializeToElement(new
        {
            activationPlan = new
            {
                expectedCurrentRevision = projection.ExpectedCurrentRevision,
                previousGenerationRetentionDays = projection.PreviousGenerationRetentionDays,
                manifest = new
                {
                    manifestSchemaVersion = manifest.ManifestSchemaVersion,
                    corpusRevision = manifest.CorpusRevision.Value,
                    catalogueRevision = manifest.CatalogueRevision.Value,
                    activeDocumentSetDigest = manifest.ActiveDocumentSetDigest.Value,
                    sourceBindingSetDigest = manifest.SourceBindingSetDigest.Value,
                    indexCompatibilityKey = manifest.IndexCompatibilityKey.Value,
                    generationSpecDigest = manifest.GenerationSpecDigest.Value,
                    chunkCount = manifest.ChunkCount,
                    vectorCount = manifest.VectorCount,
                    logicalArtifactDigest = manifest.LogicalArtifactDigest.Value,
                    generationContentDigest = manifest.GenerationContentDigest.Value,
                    indexGenerationId = manifest.IndexGenerationId.Value,
                },
                evidenceBindings = projection.EvidenceBindings.Select(ToPayload).ToArray(),
            },
        });
    }

    private static object ToPayload(DocumentActivationEvidenceBinding evidence) => new
    {
        binding = new
        {
            databaseProductId = evidence.DocumentBinding.DatabaseProductId.Value,
            databaseProductRevision = evidence.DocumentBinding.DatabaseProductRevision.Value,
            documentId = evidence.DocumentBinding.DocumentId.Value,
            documentVersion = evidence.DocumentBinding.DocumentVersion.Value,
            documentFormat = evidence.DocumentBinding.DocumentFormat.ToString(),
            sourceAdapterId = evidence.DocumentBinding.SourceAdapterId.Value,
            sourceTrustClass = evidence.DocumentBinding.SourceTrustClass.ToString(),
            officialSourceRegistrationId = evidence.DocumentBinding.OfficialSourceRegistrationId?.Value,
            officialSnapshotId = evidence.DocumentBinding.OfficialSnapshotId?.Value,
            sourceObservationId = evidence.DocumentBinding.SourceObservationId?.Value,
        },
        sourceContentObjectId = evidence.SourceContentObjectId.Value,
        rightsSchemaVersion = evidence.RightsSchemaVersion,
        rightsDecisions = evidence.Rights.Decisions.Select(decision => new
        {
            right = decision.Right.ToString(),
            state = decision.State.ToString(),
            evidenceReference = decision.EvidenceReference.Value,
        }).ToArray(),
        renderManifestId = evidence.RenderManifestId?.Value,
    };
}

internal sealed record ActivationProjectionDocument(
    DocumentBinding Binding,
    ContentObjectId SourceContentObjectId,
    DocumentRightsEligibilityRecordV1 Rights);

internal sealed record ValidatedActivationProjection(
    long ExpectedCurrentRevision,
    int PreviousGenerationRetentionDays,
    IReadOnlyCollection<DocumentActivationEvidenceBinding> EvidenceBindings);

internal sealed class ActivationPlanProjectionPayload
{
    public long ExpectedCurrentRevision { get; init; }

    public int PreviousGenerationRetentionDays { get; init; }

    public required DocumentRenderManifestProjectionPayload[] DocumentRenderManifests { get; init; }
}

internal sealed class DocumentRenderManifestProjectionPayload
{
    public required string DocumentId { get; init; }

    public long DocumentVersion { get; init; }

    public string? RenderManifestId { get; init; }
}

internal sealed class RenderDocumentPayload
{
    public required string DocumentId { get; init; }

    public long DocumentVersion { get; init; }

    public required string SourceContentObjectId { get; init; }

    public long SourceByteLength { get; init; }

    public DateTimeOffset GeneratedAt { get; init; }

    public required DocumentRightsPayload Rights { get; init; }

    public required PdfRenderPolicyPayload RenderPolicy { get; init; }

    public required DerivativeObligationSetPayload ObligationSet { get; init; }
}

internal sealed class PdfRenderPolicyPayload
{
    public long MaximumSourceByteLength { get; init; }

    public int MaximumPageCount { get; init; }

    public long MaximumTotalPixels { get; init; }

    public long MaximumPageOutputByteLength { get; init; }

    public long MaximumTotalOutputByteLength { get; init; }

    public long MaximumWorkerMemoryBytes { get; init; }

    public long MaximumWorkerCpuMilliseconds { get; init; }

    public long WorkerTimeoutMilliseconds { get; init; }

    internal PdfRenderPolicy ToDomain() => new(
        MaximumSourceByteLength,
        MaximumPageCount,
        MaximumTotalPixels,
        MaximumPageOutputByteLength,
        MaximumTotalOutputByteLength,
        MaximumWorkerMemoryBytes,
        TimeSpan.FromMilliseconds(MaximumWorkerCpuMilliseconds),
        TimeSpan.FromMilliseconds(WorkerTimeoutMilliseconds));
}

internal sealed class DerivativeObligationSetPayload
{
    public int SchemaVersion { get; init; }

    public required string ExpectedObligationSetId { get; init; }

    public required string ExpectedCanonicalSha256 { get; init; }

    public required string ExpectedRightsMappingRevision { get; init; }

    public required string[] OrderedEvidenceReferences { get; init; }

    public required string ContentLanguage { get; init; }

    public required string AuthoritativePublisherOrAuthor { get; init; }

    public required string DocumentTitle { get; init; }

    public required string DocumentVersionLabel { get; init; }

    public required string SourceReference { get; init; }

    public required string AttributionText { get; init; }

    public required string CopyrightNotice { get; init; }

    public required string PermissionNotice { get; init; }

    public required string[] OrderedDisclaimers { get; init; }

    public required string TrademarkTreatment { get; init; }

    public required string TrademarkOrNonEndorsementText { get; init; }

    public required string ChangeMarkingText { get; init; }

    public DateTimeOffset AssessedAt { get; init; }

    public required string AssessorId { get; init; }

    internal DerivativeObligationSetV1 ToDomain(
        DocumentRightsEligibilityRecordV1 rights,
        ContentObjectId sourceContentObjectId)
    {
        if (SchemaVersion != DerivativeObligationSetV1.CurrentSchemaVersion)
        {
            throw new InvalidDataException("The derivative-obligation schema is not supported.");
        }

        var obligationSet = DerivativeObligationSetV1.Create(
            rights,
            sourceContentObjectId,
            OrderedEvidenceReferences.Select(value =>
                new DocumentRightsEvidenceReference(value)),
            AdministrativeMaterialisationJson.ParseCanonicalLanguage(ContentLanguage),
            AuthoritativePublisherOrAuthor,
            DocumentTitle,
            DocumentVersionLabel,
            SourceReference,
            AttributionText,
            CopyrightNotice,
            PermissionNotice,
            OrderedDisclaimers,
            AdministrativeMaterialisationJson.ParseEnum<DerivativeTrademarkTreatment>(
                TrademarkTreatment),
            TrademarkOrNonEndorsementText,
            ChangeMarkingText,
            AssessedAt,
            AssessorId);

        if (obligationSet.ObligationSetId !=
                new DerivativeObligationSetId(ExpectedObligationSetId) ||
            obligationSet.CanonicalSha256 !=
                new DerivativeObligationSetSha256(ExpectedCanonicalSha256) ||
            obligationSet.RightsMappingRevision !=
                new RightsMappingRevision(ExpectedRightsMappingRevision))
        {
            throw new InvalidDataException("The derivative-obligation identity drifted.");
        }

        return obligationSet;
    }
}
