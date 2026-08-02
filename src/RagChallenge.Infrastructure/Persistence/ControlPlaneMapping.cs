// Purpose: Converts between immutable Domain records and control.db rows without placing ORM annotations or storage representations in Domain.
using System.Globalization;

using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.Infrastructure.Persistence;

internal static class ControlPlaneMapping
{
    internal static GenerationManifestRow ToRow(
        GenerationCommitRequest request) =>
        new()
        {
            CorpusId = request.Manifest.CorpusId.Value,
            IndexGenerationId = request.Manifest.IndexGenerationId.Value,
            CandidateBuildId = request.CandidateBuildId.Value,
            ManifestSchemaVersion = request.Manifest.ManifestSchemaVersion,
            CorpusRevision = request.Manifest.CorpusRevision.Value,
            CatalogueRevision = request.Manifest.CatalogueRevision.Value,
            ActiveDocumentSetDigest = request.Manifest.ActiveDocumentSetDigest.Value,
            SourceBindingSetDigest = request.Manifest.SourceBindingSetDigest.Value,
            IndexCompatibilityKey = request.Manifest.IndexCompatibilityKey.Value,
            GenerationSpecDigest = request.Manifest.GenerationSpecDigest.Value,
            ChunkCount = request.Manifest.ChunkCount,
            VectorCount = request.Manifest.VectorCount,
            LogicalArtifactDigest = request.Manifest.LogicalArtifactDigest.Value,
            GenerationContentDigest = request.Manifest.GenerationContentDigest.Value,
            FinalisedAtUtc = FormatUtc(request.FinalisedAt),
            OperationId = request.OperationId.Value,
        };

    internal static GenerationManifestBindingRow ToGenerationBindingRow(
        CorpusId corpusId,
        IndexGenerationId generationId,
        DocumentBinding binding) =>
        new()
        {
            CorpusId = corpusId.Value,
            IndexGenerationId = generationId.Value,
            ProductId = binding.DatabaseProductId.Value,
            ProductRevision = binding.DatabaseProductRevision.Value,
            DocumentId = binding.DocumentId.Value,
            DocumentVersion = binding.DocumentVersion.Value,
            DocumentFormat = binding.DocumentFormat.ToString(),
            SourceAdapterId = binding.SourceAdapterId.Value,
            SourceTrustClass = binding.SourceTrustClass.ToString(),
            OfficialRegistrationId = binding.OfficialSourceRegistrationId?.Value,
            OfficialSnapshotId = binding.OfficialSnapshotId?.Value,
        };

    internal static ActivationRecordRow ToRow(
        ActivationCompareExchangeRequest request) =>
        new()
        {
            CorpusId = request.ProposedRecord.CorpusId.Value,
            RecordRevision = request.ProposedRecord.RecordRevision.Value,
            PreviousRecordRevision = request.ProposedRecord.PreviousRecordRevision?.Value,
            IndexGenerationId = request.ProposedRecord.IndexGenerationId.Value,
            CatalogueRevision = request.ProposedRecord.CatalogueRevision.Value,
            ActivationBindingSetDigest = request.ProposedRecord.ActivationBindingSetDigest.Value,
            MutationKind = request.MutationKind.ToString(),
            GenerationActivatedAtUtc = FormatUtc(request.ProposedRecord.GenerationActivatedAt),
            RecordUpdatedAtUtc = FormatUtc(request.ProposedRecord.RecordUpdatedAt),
            OperationId = request.OperationId.Value,
        };

    internal static ActivationBindingRow ToActivationBindingRow(
        CorpusId corpusId,
        ActivationRecordRevision recordRevision,
        DocumentBinding binding) =>
        new()
        {
            CorpusId = corpusId.Value,
            RecordRevision = recordRevision.Value,
            ProductId = binding.DatabaseProductId.Value,
            ProductRevision = binding.DatabaseProductRevision.Value,
            DocumentId = binding.DocumentId.Value,
            DocumentVersion = binding.DocumentVersion.Value,
            DocumentFormat = binding.DocumentFormat.ToString(),
            SourceAdapterId = binding.SourceAdapterId.Value,
            SourceTrustClass = binding.SourceTrustClass.ToString(),
            OfficialRegistrationId = binding.OfficialSourceRegistrationId?.Value,
            OfficialSnapshotId = binding.OfficialSnapshotId?.Value,
            SourceObservationId = binding.SourceObservationId?.Value,
        };

    internal static FinalisedIndexGenerationManifest ToDomain(
        GenerationManifestRow row) =>
        new(
            row.ManifestSchemaVersion,
            new CorpusId(row.CorpusId),
            new CorpusRevision(row.CorpusRevision),
            new CatalogueRevision(row.CatalogueRevision),
            new ActiveDocumentSetDigest(row.ActiveDocumentSetDigest),
            new SourceBindingSetDigest(row.SourceBindingSetDigest),
            new IndexCompatibilityKey(row.IndexCompatibilityKey),
            new GenerationSpecDigest(row.GenerationSpecDigest),
            row.ChunkCount,
            row.VectorCount,
            new LogicalArtifactDigest(row.LogicalArtifactDigest),
            new GenerationContentDigest(row.GenerationContentDigest),
            new IndexGenerationId(row.IndexGenerationId));

    internal static CorpusActivationRecord ToDomain(
        ActivationRecordRow row,
        IEnumerable<ActivationBindingRow> bindings) =>
        new(
            new CorpusId(row.CorpusId),
            new ActivationRecordRevision(row.RecordRevision),
            row.PreviousRecordRevision is null
                ? null
                : new ActivationRecordRevision(row.PreviousRecordRevision.Value),
            new IndexGenerationId(row.IndexGenerationId),
            new CatalogueRevision(row.CatalogueRevision),
            new ActivationBindingSetDigest(row.ActivationBindingSetDigest),
            bindings.Select(ToDomain),
            ParseUtc(row.GenerationActivatedAtUtc),
            ParseUtc(row.RecordUpdatedAtUtc));

    internal static DocumentBinding ToDomain(ActivationBindingRow row) =>
        new(
            new DatabaseProductId(row.ProductId),
            new DatabaseProductRevision(row.ProductRevision),
            new DocumentId(row.DocumentId),
            new DocumentVersionNumber(row.DocumentVersion),
            Enum.Parse<DocumentFormat>(row.DocumentFormat, ignoreCase: false),
            new SourceAdapterId(row.SourceAdapterId),
            Enum.Parse<SourceTrustClass>(row.SourceTrustClass, ignoreCase: false),
            row.OfficialRegistrationId is null
                ? null
                : new OfficialSourceRegistrationId(row.OfficialRegistrationId),
            row.OfficialSnapshotId is null
                ? null
                : new OfficialSnapshotId(row.OfficialSnapshotId),
            row.SourceObservationId is null
                ? null
                : new OfficialObservationId(row.SourceObservationId));

    internal static OfficialSourceObservation ToDomain(SourceObservationRow row) =>
        new(
            new OfficialObservationId(row.ObservationId),
            new OfficialSourceRegistrationId(row.RegistrationId),
            new OfficialSnapshotId(row.SnapshotId),
            new ObservationJournalRevision(row.JournalRevision),
            Enum.Parse<OfficialObservationState>(row.State, ignoreCase: false),
            ParseUtc(row.RevalidatedAtUtc),
            TimeSpan.FromSeconds(row.MaxAgeSeconds));

    internal static string FormatUtc(DateTimeOffset value) =>
        value.ToString("O", CultureInfo.InvariantCulture);

    internal static DateTimeOffset ParseUtc(string value) =>
        DateTimeOffset.ParseExact(
            value,
            "O",
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind);

    internal static void EnsureUtc(DateTimeOffset value, string parameterName)
    {
        if (value.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Persistence instants must be expressed in UTC.",
                parameterName);
        }
    }
}
