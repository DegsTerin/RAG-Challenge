// Purpose: Models immutable official registrations and snapshots while keeping URL acquisition, validation, and durable content access in outer layers.
using System.Globalization;

namespace RagChallenge.Domain.CorpusCatalog;

public sealed class OfficialSourceRegistration
{
    public OfficialSourceRegistration(
        OfficialSourceRegistrationId id,
        SourceRegistrationRevision revision,
        DatabaseProductId databaseProductId,
        DocumentId documentId,
        SourceAdapterId sourceAdapterId,
        string canonicalHttpsUrl,
        CatalogueItemStatus status)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(revision);
        ArgumentNullException.ThrowIfNull(databaseProductId);
        ArgumentNullException.ThrowIfNull(documentId);
        ArgumentNullException.ThrowIfNull(sourceAdapterId);

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "An official source status must belong to the closed catalogue lifecycle.");
        }

        var canonicalUri = ValidateCanonicalHttpsUrl(canonicalHttpsUrl);

        Id = id;
        Revision = revision;
        DatabaseProductId = databaseProductId;
        DocumentId = documentId;
        SourceAdapterId = sourceAdapterId;
        CanonicalHttpsUrl = canonicalUri.AbsoluteUri;
        Status = status;
    }

    public OfficialSourceRegistrationId Id { get; }

    public SourceRegistrationRevision Revision { get; }

    public DatabaseProductId DatabaseProductId { get; }

    public DocumentId DocumentId { get; }

    public SourceAdapterId SourceAdapterId { get; }

    public string CanonicalHttpsUrl { get; }

    public CatalogueItemStatus Status { get; }

    private static Uri ValidateCanonicalHttpsUrl(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps ||
            !string.IsNullOrEmpty(uri.UserInfo) ||
            uri.IsDefaultPort is false ||
            !string.IsNullOrEmpty(uri.Fragment))
        {
            throw new ArgumentException(
                "An official source registration requires an absolute default-port HTTPS URL without user information or fragment.",
                nameof(value));
        }

        return uri;
    }
}

public sealed class OfficialSourceSnapshot
{
    public OfficialSourceSnapshot(
        OfficialSnapshotId id,
        OfficialSourceRegistrationId registrationId,
        ContentObjectId contentObjectId,
        long byteLength,
        string mediaType,
        DateTimeOffset retrievedAt)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(registrationId);
        ArgumentNullException.ThrowIfNull(contentObjectId);
        ArgumentException.ThrowIfNullOrWhiteSpace(mediaType);

        if (byteLength <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(byteLength),
                byteLength,
                "An immutable source snapshot must contain at least one byte.");
        }

        if (retrievedAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Snapshot instants must be expressed in UTC.",
                nameof(retrievedAt));
        }

        Id = id;
        RegistrationId = registrationId;
        ContentObjectId = contentObjectId;
        ByteLength = byteLength;
        MediaType = mediaType;
        RetrievedAt = retrievedAt;
    }

    public OfficialSnapshotId Id { get; }

    public OfficialSourceRegistrationId RegistrationId { get; }

    public ContentObjectId ContentObjectId { get; }

    public long ByteLength { get; }

    public string MediaType { get; }

    public DateTimeOffset RetrievedAt { get; }

    public string ToCanonicalRetrievalInstant() =>
        RetrievedAt.ToString("O", CultureInfo.InvariantCulture);
}
