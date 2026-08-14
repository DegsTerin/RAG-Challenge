// Purpose: Binds one activation document revision to its exact source, immutable rights snapshot, and optional PDF render manifest without defining a new digest domain.
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Domain.IndexingRetrieval;

public sealed class DocumentActivationEvidenceBinding
{
    public DocumentActivationEvidenceBinding(
        DocumentBinding documentBinding,
        ContentObjectId sourceContentObjectId,
        DocumentRightsEligibilityRecordV1 rights,
        RenderManifestId? renderManifestId)
    {
        DocumentBinding = documentBinding ??
            throw new ArgumentNullException(nameof(documentBinding));
        SourceContentObjectId = sourceContentObjectId ??
            throw new ArgumentNullException(nameof(sourceContentObjectId));
        Rights = rights ?? throw new ArgumentNullException(nameof(rights));

        if (rights.SchemaVersion != DocumentRightsEligibilityRecordV1.CurrentSchemaVersion ||
            rights.DocumentId != documentBinding.DocumentId ||
            rights.DocumentVersion != documentBinding.DocumentVersion)
        {
            throw new ArgumentException(
                "An activation rights snapshot must use schema version 1 and name the exact document revision.",
                nameof(rights));
        }

        if (documentBinding.DocumentFormat == DocumentFormat.Csv && renderManifestId is not null)
        {
            throw new ArgumentException(
                "A CSV activation cannot name a PDF render manifest.",
                nameof(renderManifestId));
        }

        RenderManifestId = renderManifestId;
    }

    public DocumentBinding DocumentBinding { get; }

    public ContentObjectId SourceContentObjectId { get; }

    public int RightsSchemaVersion => Rights.SchemaVersion;

    public DocumentRightsEligibilityRecordV1 Rights { get; }

    public RenderManifestId? RenderManifestId { get; }

    public DocumentActivationEvidenceBinding WithDocumentBinding(DocumentBinding documentBinding)
    {
        ArgumentNullException.ThrowIfNull(documentBinding);

        if (documentBinding.DatabaseProductId != DocumentBinding.DatabaseProductId ||
            documentBinding.DatabaseProductRevision != DocumentBinding.DatabaseProductRevision ||
            documentBinding.DocumentId != DocumentBinding.DocumentId ||
            documentBinding.DocumentVersion != DocumentBinding.DocumentVersion ||
            documentBinding.DocumentFormat != DocumentBinding.DocumentFormat ||
            documentBinding.SourceAdapterId != DocumentBinding.SourceAdapterId ||
            documentBinding.SourceTrustClass != DocumentBinding.SourceTrustClass ||
            documentBinding.OfficialSourceRegistrationId !=
                DocumentBinding.OfficialSourceRegistrationId ||
            documentBinding.OfficialSnapshotId != DocumentBinding.OfficialSnapshotId)
        {
            throw new ArgumentException(
                "Only the freshness observation may change while preserving activation evidence.",
                nameof(documentBinding));
        }

        return new DocumentActivationEvidenceBinding(
            documentBinding,
            SourceContentObjectId,
            Rights,
            RenderManifestId);
    }
}
