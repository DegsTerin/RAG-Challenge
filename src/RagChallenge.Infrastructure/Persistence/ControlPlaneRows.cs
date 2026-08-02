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

    public required string ContentSha256 { get; set; }

    public long ByteLength { get; set; }

    public required string MediaType { get; set; }

    public required string SourceAdapterId { get; set; }

    public required string SourceTrustClass { get; set; }

    public string? OfficialRegistrationId { get; set; }

    public string? OfficialSnapshotId { get; set; }
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
}

internal sealed class CatalogueRevisionDocumentRow
{
    public required string CorpusId { get; set; }

    public long CatalogueRevision { get; set; }

    public required string DocumentId { get; set; }

    public long DocumentVersion { get; set; }

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
