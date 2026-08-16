// Purpose: Defines the SQLite control-plane row model that persists authoritative catalogue, manifest, activation, audit, retention, and recovery state outside Domain.
namespace RagChallenge.Infrastructure.Persistence;

internal sealed class CorpusRow
{
    public required string CorpusId { get; set; }

    public long CorpusRevision { get; set; }

    public required string CreatedAtUtc { get; set; }
}

internal sealed class DatabaseCategoryRow
{
    public required string CorpusId { get; set; }

    public required string CategoryId { get; set; }

    public required string DisplayName { get; set; }
}

internal sealed class DatabaseProductRevisionRow
{
    public required string CorpusId { get; set; }

    public required string ProductId { get; set; }

    public long ProductRevision { get; set; }

    public required string DisplayName { get; set; }

    public required string Status { get; set; }
}

internal sealed class DatabaseProductCategoryRow
{
    public required string CorpusId { get; set; }

    public required string ProductId { get; set; }

    public long ProductRevision { get; set; }

    public required string CategoryId { get; set; }
}

internal sealed class ContentObjectRow
{
    public required string ContentSha256 { get; set; }

    public long ByteLength { get; set; }

    public required string RegisteredAtUtc { get; set; }
}

internal sealed class DocumentVersionRow
{
    public required string CorpusId { get; set; }

    public required string DocumentId { get; set; }

    public long DocumentVersion { get; set; }

    public required string ProductId { get; set; }

    public long ProductRevision { get; set; }

    public required string DocumentFormat { get; set; }

    public required string ContentLanguage { get; set; }

    public string? SourceDeclaredLanguage { get; set; }

    public required string ContentSha256 { get; set; }

    public long ByteLength { get; set; }

    public required string MediaType { get; set; }

    public required string SourceAdapterId { get; set; }

    public required string SourceTrustClass { get; set; }

    public string? OfficialRegistrationId { get; set; }

    public string? OfficialSnapshotId { get; set; }
}

internal sealed class DerivativeObligationSetRow
{
    public required string ObligationSetId { get; set; }

    public int SchemaVersion { get; set; }

    public required string CanonicalSha256 { get; set; }

    public required string CorpusId { get; set; }

    public required string DocumentId { get; set; }

    public long DocumentVersion { get; set; }

    public required string SourceContentSha256 { get; set; }

    public required string RightsMappingRevision { get; set; }

    public required string ContentLanguage { get; set; }

    public required string AuthoritativePublisherOrAuthor { get; set; }

    public required string DocumentTitle { get; set; }

    public required string DocumentVersionLabel { get; set; }

    public required string SourceReference { get; set; }

    public required string AttributionText { get; set; }

    public required string CopyrightNotice { get; set; }

    public required string PermissionNotice { get; set; }

    public required string TrademarkTreatment { get; set; }

    public required string TrademarkOrNonEndorsementText { get; set; }

    public required string ChangeMarkingText { get; set; }

    public required string PlacementMode { get; set; }

    public required string AssessedAtUtc { get; set; }

    public required string AssessorId { get; set; }
}

internal sealed class DerivativeObligationEvidenceReferenceRow
{
    public required string ObligationSetId { get; set; }

    public int Ordinal { get; set; }

    public required string EvidenceReference { get; set; }
}

internal sealed class DerivativeObligationDisclaimerRow
{
    public required string ObligationSetId { get; set; }

    public int Ordinal { get; set; }

    public required string DisclaimerText { get; set; }
}

internal sealed class DocumentRenderManifestRow
{
    public required string RenderManifestId { get; set; }

    public required string ManifestSha256 { get; set; }

    public int SchemaVersion { get; set; }

    public required string CorpusId { get; set; }

    public required string DocumentId { get; set; }

    public long DocumentVersion { get; set; }

    public required string SourceContentSha256 { get; set; }

    public int SourcePageCount { get; set; }

    public required string RenderProfileId { get; set; }

    public required string RendererDescriptor { get; set; }

    public string? ObligationSetId { get; set; }

    public string? ObligationSetSha256 { get; set; }

    public required string GeneratedAtUtc { get; set; }
}

internal sealed class DocumentPageImageRow
{
    public required string RenderManifestId { get; set; }

    public int PageNumber { get; set; }

    public required string CorpusId { get; set; }

    public required string DocumentId { get; set; }

    public long DocumentVersion { get; set; }

    public required string SourceContentSha256 { get; set; }

    public required string RenderProfileId { get; set; }

    public required string RendererDescriptor { get; set; }

    public required string ImageContentSha256 { get; set; }

    public required string ImageSha256 { get; set; }

    public long ByteLength { get; set; }

    public required string MediaType { get; set; }

    public int WidthPixels { get; set; }

    public int HeightPixels { get; set; }

    public int? SourceRegionWidthPixels { get; set; }

    public int? SourceRegionHeightPixels { get; set; }

    public int? NoticeRegionHeightPixels { get; set; }
}

internal sealed class CatalogueRevisionRow
{
    public required string CorpusId { get; set; }

    public long CatalogueRevision { get; set; }

    public required string CreatedAtUtc { get; set; }

    public required string OperationId { get; set; }
}

internal sealed class CatalogueRevisionProductRow
{
    public required string CorpusId { get; set; }

    public long CatalogueRevision { get; set; }

    public required string ProductId { get; set; }

    public long ProductRevision { get; set; }

    public required string Status { get; set; }
}

internal sealed class CatalogueRevisionDocumentRow
{
    public required string CorpusId { get; set; }

    public long CatalogueRevision { get; set; }

    public required string DocumentId { get; set; }

    public long DocumentVersion { get; set; }

    public required string ProductId { get; set; }

    public long ProductRevision { get; set; }

    public required string Status { get; set; }
}

internal sealed class CatalogueHeadRow
{
    public required string CorpusId { get; set; }

    public long CatalogueRevision { get; set; }

    public long RowRevision { get; set; }
}

internal sealed class OfficialSourceRegistrationRow
{
    public required string CorpusId { get; set; }

    public required string RegistrationId { get; set; }

    public long RegistrationRevision { get; set; }

    public required string ProductId { get; set; }

    public required string DocumentId { get; set; }

    public required string SourceAdapterId { get; set; }

    public required string CanonicalHttpsUrl { get; set; }

    public required string Status { get; set; }
}

internal sealed class OfficialSourceSnapshotRow
{
    public required string CorpusId { get; set; }

    public required string SnapshotId { get; set; }

    public required string RegistrationId { get; set; }

    public long RegistrationRevision { get; set; }

    public required string ContentSha256 { get; set; }

    public long ByteLength { get; set; }

    public required string MediaType { get; set; }

    public required string RetrievedAtUtc { get; set; }
}

internal sealed class SourceObservationRow
{
    public required string CorpusId { get; set; }

    public required string ObservationId { get; set; }

    public required string RegistrationId { get; set; }

    public required string SnapshotId { get; set; }

    public long JournalRevision { get; set; }

    public required string State { get; set; }

    public required string RevalidatedAtUtc { get; set; }

    public long MaxAgeSeconds { get; set; }

    public required string OperationId { get; set; }
}

internal sealed class ObservationJournalHeadRow
{
    public required string CorpusId { get; set; }

    public long JournalRevision { get; set; }

    public long RowRevision { get; set; }
}

internal sealed class GenerationManifestRow
{
    public required string CorpusId { get; set; }

    public required string IndexGenerationId { get; set; }

    public required string CandidateBuildId { get; set; }

    public int ManifestSchemaVersion { get; set; }

    public long CorpusRevision { get; set; }

    public long CatalogueRevision { get; set; }

    public required string ActiveDocumentSetDigest { get; set; }

    public required string SourceBindingSetDigest { get; set; }

    public required string IndexCompatibilityKey { get; set; }

    public required string GenerationSpecDigest { get; set; }

    public long ChunkCount { get; set; }

    public long VectorCount { get; set; }

    public required string LogicalArtifactDigest { get; set; }

    public required string GenerationContentDigest { get; set; }

    public required string FinalisedAtUtc { get; set; }

    public required string OperationId { get; set; }
}

internal sealed class GenerationManifestBindingRow
{
    public required string CorpusId { get; set; }

    public required string IndexGenerationId { get; set; }

    public required string ProductId { get; set; }

    public long ProductRevision { get; set; }

    public required string DocumentId { get; set; }

    public long DocumentVersion { get; set; }

    public required string DocumentFormat { get; set; }

    public required string SourceAdapterId { get; set; }

    public required string SourceTrustClass { get; set; }

    public string? OfficialRegistrationId { get; set; }

    public string? OfficialSnapshotId { get; set; }
}

internal sealed class ActivationRecordRow
{
    public required string CorpusId { get; set; }

    public long RecordRevision { get; set; }

    public long? PreviousRecordRevision { get; set; }

    public required string IndexGenerationId { get; set; }

    public long CatalogueRevision { get; set; }

    public required string ActivationBindingSetDigest { get; set; }

    public required string MutationKind { get; set; }

    public required string GenerationActivatedAtUtc { get; set; }

    public required string RecordUpdatedAtUtc { get; set; }

    public required string OperationId { get; set; }
}

internal sealed class ActivationBindingRow
{
    public required string CorpusId { get; set; }

    public long RecordRevision { get; set; }

    public required string ProductId { get; set; }

    public long ProductRevision { get; set; }

    public required string DocumentId { get; set; }

    public long DocumentVersion { get; set; }

    public required string DocumentFormat { get; set; }

    public required string SourceAdapterId { get; set; }

    public required string SourceTrustClass { get; set; }

    public string? OfficialRegistrationId { get; set; }

    public string? OfficialSnapshotId { get; set; }

    public string? SourceObservationId { get; set; }
}

internal sealed class ActivationEvidenceBindingRow
{
    public required string CorpusId { get; set; }

    public long RecordRevision { get; set; }

    public required string DocumentId { get; set; }

    public long DocumentVersion { get; set; }

    public required string DocumentFormat { get; set; }

    public required string SourceContentSha256 { get; set; }

    public int RightsSchemaVersion { get; set; }

    public string? RenderManifestId { get; set; }
}

internal sealed class ActivationRightsDecisionRow
{
    public required string CorpusId { get; set; }

    public long RecordRevision { get; set; }

    public required string DocumentId { get; set; }

    public long DocumentVersion { get; set; }

    public required string DocumentRight { get; set; }

    public required string DecisionState { get; set; }

    public required string EvidenceReference { get; set; }
}

internal sealed class ActivationHeadRow
{
    public required string CorpusId { get; set; }

    public long RecordRevision { get; set; }

    public long RowRevision { get; set; }
}

internal sealed class GenerationRetentionRow
{
    public required string CorpusId { get; set; }

    public required string IndexGenerationId { get; set; }

    public required string ProtectionRole { get; set; }

    public required string RetainUntilUtc { get; set; }

    public required string RecordedAtUtc { get; set; }

    public required string OperationId { get; set; }
}

internal sealed class AnswerEvidenceRecordRow
{
    public required string AnswerEvidenceRecordId { get; set; }

    public int SchemaVersion { get; set; }

    public required string RecordSha256 { get; set; }

    public required string CorpusId { get; set; }

    public long ActivationRecordRevision { get; set; }

    public long CatalogueRevision { get; set; }

    public required string SourceBindingSetDigest { get; set; }

    public required string ActivationBindingSetDigest { get; set; }

    public required string IndexGenerationId { get; set; }

    public required string Outcome { get; set; }

    public required string QuestionLanguage { get; set; }

    public required string AnswerLanguage { get; set; }

    public required string AnswerSha256 { get; set; }

    public int AnswerUtf8ByteLength { get; set; }

    public required string EvidenceCoverageDigest { get; set; }

    public required string RetrievalPolicyVersion { get; set; }

    public required string PromptVersion { get; set; }

    public required string LanguageModelProviderId { get; set; }

    public required string LanguageModelId { get; set; }

    public required string LanguageModelRevision { get; set; }

    public required string CorrelationId { get; set; }

    public required string RetentionPolicyId { get; set; }

    public required string CreatedAtUtc { get; set; }

    public required string ExpiresAtUtc { get; set; }
}

internal sealed class AnswerEvidenceCitationRow
{
    public required string AnswerEvidenceRecordId { get; set; }

    public int Ordinal { get; set; }

    public required string ProductId { get; set; }

    public long ProductRevision { get; set; }

    public required string DocumentId { get; set; }

    public long DocumentVersion { get; set; }

    public required string DocumentFormat { get; set; }

    public required string ContentLanguage { get; set; }

    public required string ChunkId { get; set; }

    public required string SourceAdapterId { get; set; }

    public required string SourceTrustClass { get; set; }

    public string? OfficialRegistrationId { get; set; }

    public string? SourceSnapshotId { get; set; }

    public string? SourceObservationId { get; set; }

    public required string SourceContentSha256 { get; set; }

    public int? PageStart { get; set; }

    public int? PageEnd { get; set; }

    public long? RecordStart { get; set; }

    public long? RecordEnd { get; set; }

    public required string ColumnsJson { get; set; }

    public string? SectionLocator { get; set; }

    public string? RenderManifestId { get; set; }
}

internal sealed class AnswerEvidencePageRow
{
    public required string AnswerEvidenceRecordId { get; set; }

    public required string DocumentId { get; set; }

    public long DocumentVersion { get; set; }

    public required string SourceContentSha256 { get; set; }

    public int PageNumber { get; set; }

    public required string RenderManifestId { get; set; }

    public required string RenderProfileId { get; set; }

    public required string RendererDescriptor { get; set; }

    public required string ImageContentSha256 { get; set; }

    public required string ImageSha256 { get; set; }

    public long ByteLength { get; set; }

    public required string MediaType { get; set; }

    public int WidthPixels { get; set; }

    public int HeightPixels { get; set; }
}

internal sealed class ProviderBudgetStoreEpochRow
{
    public required string StoreEpochId { get; set; }

    public long EpochRevision { get; set; }

    public string? PreviousStoreEpochId { get; set; }

    public required string EpochKind { get; set; }

    public string? RestoreCheckpointSha256 { get; set; }

    public required string AuthorityReference { get; set; }

    public required string OccurredAtUtc { get; set; }

    public required string PreviousEpochSha256 { get; set; }

    public required string EpochSha256 { get; set; }
}

internal sealed class ProviderBudgetControlHeadRow
{
    public required string ControlId { get; set; }

    public required string CurrentStoreEpochId { get; set; }

    public long EpochRevision { get; set; }

    public long RowRevision { get; set; }
}

internal sealed class ProviderBudgetEnvelopeRow
{
    public required string EnvelopeId { get; set; }

    public int SchemaVersion { get; set; }

    public required string CurrentStoreEpochId { get; set; }

    public required string EnvironmentId { get; set; }

    public required string ProviderId { get; set; }

    public required string BillingScopeReference { get; set; }

    public required string ModelId { get; set; }

    public required string CurrencyCode { get; set; }

    public required string AccountingUnitId { get; set; }

    public long CurrentConfigurationRevision { get; set; }

    public long CurrentLedgerRevision { get; set; }

    public long CurrentRearmRevision { get; set; }

    public required string State { get; set; }

    public string? RuntimeSessionId { get; set; }

    public long AggregateLimitUnits { get; set; }

    public long AggregateCommittedUnits { get; set; }

    public long AggregateReservedUnits { get; set; }

    public long AggregateIndeterminateUnits { get; set; }

    public int IsInitialised { get; set; }

    public int IsClosed { get; set; }

    public required string CreatedAtUtc { get; set; }

    public required string CreationAuthorityReference { get; set; }

    public string? ClosedAtUtc { get; set; }

    public string? ClosureAuthorityReference { get; set; }

    public required string CurrentLedgerSha256 { get; set; }
}

internal sealed class ProviderBudgetConfigurationRow
{
    public required string EnvelopeId { get; set; }

    public long ConfigurationRevision { get; set; }

    public long? PreviousConfigurationRevision { get; set; }

    public required string CostScheduleId { get; set; }

    public required string CostScheduleSha256 { get; set; }

    public long AggregateLimitUnits { get; set; }

    public required string EffectiveAtUtc { get; set; }

    public required string ExpiresAtUtc { get; set; }

    public required string ConfigurationAuthorityReference { get; set; }

    public required string CreatedAtUtc { get; set; }

    public string? SealedAtUtc { get; set; }

    public required string ConfigurationSha256 { get; set; }
}

internal sealed class ProviderBudgetOperationAllocationRow
{
    public required string EnvelopeId { get; set; }

    public long ConfigurationRevision { get; set; }

    public required string OperationClass { get; set; }

    public long AllocationLimitUnits { get; set; }
}

internal sealed class ProviderBudgetLedgerRevisionRow
{
    public required string EnvelopeId { get; set; }

    public long LedgerRevision { get; set; }

    public required string StoreEpochId { get; set; }

    public long? PreviousLedgerRevision { get; set; }

    public long ConfigurationRevision { get; set; }

    public long RearmRevision { get; set; }

    public required string State { get; set; }

    public string? RuntimeSessionId { get; set; }

    public long AggregateLimitUnits { get; set; }

    public long AggregateCommittedUnits { get; set; }

    public long AggregateReservedUnits { get; set; }

    public long AggregateIndeterminateUnits { get; set; }

    public required string TransitionKind { get; set; }

    public string? ProviderRequestId { get; set; }

    public required string TransitionAuthorityReference { get; set; }

    public required string OccurredAtUtc { get; set; }

    public required string PreviousLedgerSha256 { get; set; }

    public required string LedgerSha256 { get; set; }

    public int IsComplete { get; set; }
}

internal sealed class ProviderBudgetOperationBalanceRevisionRow
{
    public required string EnvelopeId { get; set; }

    public long LedgerRevision { get; set; }

    public required string OperationClass { get; set; }

    public long ConfigurationRevision { get; set; }

    public long AllocationLimitUnits { get; set; }

    public long CommittedUnits { get; set; }

    public long ReservedUnits { get; set; }

    public long IndeterminateUnits { get; set; }
}

internal sealed class ProviderBudgetReservationRow
{
    public required string ProviderRequestId { get; set; }

    public required string EnvelopeId { get; set; }

    public required string StoreEpochId { get; set; }

    public long ConfigurationRevision { get; set; }

    public required string OperationClass { get; set; }

    public required string OperationAuthorityReference { get; set; }

    public required string RequestPlanSha256 { get; set; }

    public required string RequestSha256 { get; set; }

    public required string MaximumChargeBasisSha256 { get; set; }

    public required string CostScheduleSha256 { get; set; }

    public required string BindingSha256 { get; set; }

    public long MaximumChargeUnits { get; set; }

    public required string AdmittedRuntimeSessionId { get; set; }

    public long AdmissionLedgerRevision { get; set; }

    public long CurrentReservationRevision { get; set; }

    public int IsInitialised { get; set; }

    public required string Status { get; set; }

    public required string AdmittedAtUtc { get; set; }

    public string? DispatchStartedAtUtc { get; set; }

    public string? TerminalAtUtc { get; set; }

    public long? TerminalLedgerRevision { get; set; }

    public required string CurrentTransitionSha256 { get; set; }
}

internal sealed class ProviderBudgetReservationTransitionRow
{
    public required string ProviderRequestId { get; set; }

    public long ReservationRevision { get; set; }

    public required string EnvelopeId { get; set; }

    public long LedgerRevision { get; set; }

    public string? FromStatus { get; set; }

    public required string ToStatus { get; set; }

    public required string TransitionKind { get; set; }

    public string? ProofSha256 { get; set; }

    public string? OutcomeCode { get; set; }

    public required string OccurredAtUtc { get; set; }

    public required string PreviousTransitionSha256 { get; set; }

    public required string TransitionSha256 { get; set; }
}

internal sealed class ProviderBudgetCommitmentRow
{
    public required string ProviderRequestId { get; set; }

    public required string EnvelopeId { get; set; }

    public long LedgerRevision { get; set; }

    public required string CommitmentKind { get; set; }

    public long CommittedUnits { get; set; }

    public required string UsageEvidenceSha256 { get; set; }

    public required string ProviderOutcomeCode { get; set; }

    public long? ProviderDurationMilliseconds { get; set; }

    public required string OccurredAtUtc { get; set; }

    public required string CommitmentSha256 { get; set; }
}

internal sealed class ProviderBudgetReleaseRow
{
    public required string ProviderRequestId { get; set; }

    public required string EnvelopeId { get; set; }

    public long LedgerRevision { get; set; }

    public required string ProofKind { get; set; }

    public required string ProofSha256 { get; set; }

    public required string AuthorityReference { get; set; }

    public required string OccurredAtUtc { get; set; }

    public required string ReleaseSha256 { get; set; }
}

internal sealed class ProviderBudgetReconciliationDispositionRow
{
    public required string DispositionId { get; set; }

    public required string ProviderRequestId { get; set; }

    public required string EnvelopeId { get; set; }

    public long LedgerRevision { get; set; }

    public required string DispositionKind { get; set; }

    public long ConfirmedChargeUnits { get; set; }

    public long RestoredUnits { get; set; }

    public required string AuthorityReference { get; set; }

    public required string ActorReference { get; set; }

    public required string EvidenceSha256 { get; set; }

    public required string OccurredAtUtc { get; set; }

    public required string DispositionSha256 { get; set; }
}

internal sealed class ProviderBudgetRearmRow
{
    public required string EnvelopeId { get; set; }

    public long RearmRevision { get; set; }

    public required string StoreEpochId { get; set; }

    public long ExpectedConfigurationRevision { get; set; }

    public long ExpectedLedgerRevision { get; set; }

    public long ExpectedRearmRevision { get; set; }

    public long ResultingLedgerRevision { get; set; }

    public required string NewRuntimeSessionId { get; set; }

    public required string AuthorityReference { get; set; }

    public required string ActorReference { get; set; }

    public required string ReasonSha256 { get; set; }

    public long AcknowledgedCommittedUnits { get; set; }

    public long AcknowledgedReservedUnits { get; set; }

    public long AcknowledgedIndeterminateUnits { get; set; }

    public required string OperationBalancesSha256 { get; set; }

    public required string ConfigurationSha256 { get; set; }

    public required string OccurredAtUtc { get; set; }

    public required string RearmSha256 { get; set; }
}

internal sealed class ProviderBudgetAuditEventRow
{
    public required string AuditEventId { get; set; }

    public required string EnvelopeId { get; set; }

    public long LedgerRevision { get; set; }

    public string? ProviderRequestId { get; set; }

    public string? OperationClass { get; set; }

    public required string EventType { get; set; }

    public string? AuthorityReference { get; set; }

    public string? ActorReference { get; set; }

    public string? RequestSha256 { get; set; }

    public long? MaximumChargeUnits { get; set; }

    public string? FromState { get; set; }

    public string? ToState { get; set; }

    public required string OutcomeCode { get; set; }

    public required string OccurredAtUtc { get; set; }

    public required string DetailsSha256 { get; set; }
}

internal sealed class AdminOperationRow
{
    public required string OperationId { get; set; }

    public required string CorpusId { get; set; }

    public required string OperationKind { get; set; }

    public required string Status { get; set; }

    public long? ExpectedRevision { get; set; }

    public long? ResultRevision { get; set; }

    public required string RequestedAtUtc { get; set; }

    public string? CompletedAtUtc { get; set; }
}

internal sealed class AuditEventRow
{
    public required string AuditEventId { get; set; }

    public required string OperationId { get; set; }

    public required string CorpusId { get; set; }

    public required string EventType { get; set; }

    public required string OccurredAtUtc { get; set; }

    public required string DetailsDigest { get; set; }
}

internal sealed class RecoveryLeaseRow
{
    public required string CorpusId { get; set; }

    public required string LeaseName { get; set; }

    public required string OperationId { get; set; }

    public required string AcquiredAtUtc { get; set; }

    public required string ExpiresAtUtc { get; set; }
}

internal sealed class AdministrationLeaseRow
{
    public required string CorpusId { get; set; }

    public required string OperationId { get; set; }

    public required string AcquiredAtUtc { get; set; }

    public required string ExpiresAtUtc { get; set; }
}

internal sealed class AdministrationCommandJournalRow
{
    public required string OperationId { get; set; }

    public required string CorpusId { get; set; }

    public required string Command { get; set; }

    public required string ActorIdentifier { get; set; }

    public required string ReasonSha256 { get; set; }

    public string? InputSha256 { get; set; }

    public required string SourceIdsJson { get; set; }

    public required string TargetIdsJson { get; set; }

    public required string StartedAtUtc { get; set; }

    public string? CompletedAtUtc { get; set; }

    public required string Status { get; set; }

    public string? Outcome { get; set; }

    public string? ResultCode { get; set; }

    public int? ExitCategory { get; set; }

    public long? ResultRevision { get; set; }

    public required string IntentDigest { get; set; }
}
