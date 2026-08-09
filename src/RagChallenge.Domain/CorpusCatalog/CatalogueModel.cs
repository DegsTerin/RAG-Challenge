// Purpose: Models immutable catalogue revisions and enforces cross-record lifecycle invariants without assuming a database or ORM.
using System.Collections.ObjectModel;

namespace RagChallenge.Domain.CorpusCatalog;

public sealed class DatabaseProduct
{
    public DatabaseProduct(
        DatabaseProductId id,
        DatabaseProductRevision revision,
        string displayName,
        CatalogueItemStatus status,
        IEnumerable<DatabaseCategoryId> categoryIds)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(revision);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentNullException.ThrowIfNull(categoryIds);

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "A database product status must belong to the closed catalogue lifecycle.");
        }

        var materialisedCategoryIds = categoryIds.ToArray();

        if (materialisedCategoryIds.Length == 0)
        {
            throw new ArgumentException(
                "A database product must belong to at least one category.",
                nameof(categoryIds));
        }

        if (materialisedCategoryIds.Distinct().Count() != materialisedCategoryIds.Length)
        {
            throw new ArgumentException(
                "A database product cannot repeat a category assignment.",
                nameof(categoryIds));
        }

        Id = id;
        Revision = revision;
        DisplayName = displayName;
        Status = status;
        CategoryIds = Array.AsReadOnly(materialisedCategoryIds);
    }

    public DatabaseProductId Id { get; }

    public DatabaseProductRevision Revision { get; }

    public string DisplayName { get; }

    public CatalogueItemStatus Status { get; }

    public ReadOnlyCollection<DatabaseCategoryId> CategoryIds { get; }
}

public sealed class DatabaseCategory
{
    public DatabaseCategory(DatabaseCategoryId id, string displayName)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        Id = id;
        DisplayName = displayName;
    }

    public DatabaseCategoryId Id { get; }

    public string DisplayName { get; }
}

public sealed class DocumentVersion
{
    public DocumentVersion(
        DocumentId id,
        DocumentVersionNumber version,
        DatabaseProductId databaseProductId,
        DatabaseProductRevision databaseProductRevision,
        DocumentFormat format,
        DocumentContentLanguage contentLanguage,
        CatalogueItemStatus status,
        ContentObjectId contentObjectId,
        long byteLength,
        string mediaType,
        SourceAdapterId sourceAdapterId,
        SourceTrustClass sourceTrustClass,
        OfficialSourceRegistrationId? officialSourceRegistrationId = null,
        OfficialSnapshotId? officialSnapshotId = null,
        SourceDeclaredLanguage? sourceDeclaredLanguage = null)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(version);
        ArgumentNullException.ThrowIfNull(databaseProductId);
        ArgumentNullException.ThrowIfNull(databaseProductRevision);
        ArgumentNullException.ThrowIfNull(contentLanguage);
        ArgumentNullException.ThrowIfNull(contentObjectId);
        ArgumentNullException.ThrowIfNull(sourceAdapterId);
        ArgumentException.ThrowIfNullOrWhiteSpace(mediaType);

        if (!Enum.IsDefined(format))
        {
            throw new ArgumentOutOfRangeException(
                nameof(format),
                format,
                "A document format must belong to the closed MVP set.");
        }

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "A document status must belong to the closed catalogue lifecycle.");
        }

        if (!Enum.IsDefined(sourceTrustClass))
        {
            throw new ArgumentOutOfRangeException(
                nameof(sourceTrustClass),
                sourceTrustClass,
                "A source trust class must belong to the closed provenance set.");
        }

        if (byteLength <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(byteLength),
                byteLength,
                "A verified document version must contain at least one byte.");
        }

        ValidateSourceIdentity(
            status,
            sourceTrustClass,
            officialSourceRegistrationId,
            officialSnapshotId);

        Id = id;
        Version = version;
        DatabaseProductId = databaseProductId;
        DatabaseProductRevision = databaseProductRevision;
        Format = format;
        ContentLanguage = contentLanguage;
        Status = status;
        ContentObjectId = contentObjectId;
        ByteLength = byteLength;
        MediaType = mediaType;
        SourceAdapterId = sourceAdapterId;
        SourceTrustClass = sourceTrustClass;
        OfficialSourceRegistrationId = officialSourceRegistrationId;
        OfficialSnapshotId = officialSnapshotId;
        SourceDeclaredLanguage = sourceDeclaredLanguage;
    }

    public DocumentId Id { get; }

    public DocumentVersionNumber Version { get; }

    public DatabaseProductId DatabaseProductId { get; }

    public DatabaseProductRevision DatabaseProductRevision { get; }

    public DocumentFormat Format { get; }

    public DocumentContentLanguage ContentLanguage { get; }

    public SourceDeclaredLanguage? SourceDeclaredLanguage { get; }

    public CatalogueItemStatus Status { get; }

    public ContentObjectId ContentObjectId { get; }

    public long ByteLength { get; }

    public string MediaType { get; }

    public SourceAdapterId SourceAdapterId { get; }

    public SourceTrustClass SourceTrustClass { get; }

    public OfficialSourceRegistrationId? OfficialSourceRegistrationId { get; }

    public OfficialSnapshotId? OfficialSnapshotId { get; }

    private static void ValidateSourceIdentity(
        CatalogueItemStatus status,
        SourceTrustClass trustClass,
        OfficialSourceRegistrationId? registrationId,
        OfficialSnapshotId? snapshotId)
    {
        if (trustClass == SourceTrustClass.LocalAuthorised)
        {
            if (registrationId is not null || snapshotId is not null)
            {
                throw new ArgumentException(
                    "A local authorised document cannot carry official registration or snapshot identities.");
            }

            return;
        }

        if (registrationId is null)
        {
            throw new ArgumentException(
                "An official document requires an immutable source registration identity.",
                nameof(registrationId));
        }

        if (status == CatalogueItemStatus.Active && snapshotId is null)
        {
            throw new ArgumentException(
                "An active official document requires an immutable source snapshot identity.",
                nameof(snapshotId));
        }
    }
}

public sealed class CatalogueSnapshot
{
    public CatalogueSnapshot(
        CorpusId corpusId,
        CatalogueRevision revision,
        IEnumerable<DatabaseCategory> databaseCategories,
        IEnumerable<DatabaseProduct> databaseProducts,
        IEnumerable<DocumentVersion> documentVersions)
    {
        ArgumentNullException.ThrowIfNull(corpusId);
        ArgumentNullException.ThrowIfNull(revision);
        ArgumentNullException.ThrowIfNull(databaseCategories);
        ArgumentNullException.ThrowIfNull(databaseProducts);
        ArgumentNullException.ThrowIfNull(documentVersions);

        var categories = databaseCategories.ToArray();
        var products = databaseProducts.ToArray();
        var documents = documentVersions.ToArray();

        ValidateCategories(categories, products);
        ValidateUniqueProducts(products);
        ValidateUniqueDocumentVersions(documents);
        ValidateDocumentOwnership(products, documents);
        ValidateActiveEvidence(products, documents);

        CorpusId = corpusId;
        Revision = revision;
        DatabaseCategories = Array.AsReadOnly(categories);
        DatabaseProducts = Array.AsReadOnly(products);
        DocumentVersions = Array.AsReadOnly(documents);
    }

    public CorpusId CorpusId { get; }

    public CatalogueRevision Revision { get; }

    public ReadOnlyCollection<DatabaseCategory> DatabaseCategories { get; }

    public ReadOnlyCollection<DatabaseProduct> DatabaseProducts { get; }

    public ReadOnlyCollection<DocumentVersion> DocumentVersions { get; }

    private static void ValidateCategories(
        DatabaseCategory[] categories,
        DatabaseProduct[] products)
    {
        if (categories.Select(category => category.Id).Distinct().Count() != categories.Length)
        {
            throw new ArgumentException(
                "A catalogue revision cannot repeat a database category identity.",
                nameof(categories));
        }

        var categoryIds = categories
            .Select(category => category.Id)
            .ToHashSet();

        if (products
            .SelectMany(product => product.CategoryIds)
            .Any(categoryId => !categoryIds.Contains(categoryId)))
        {
            throw new ArgumentException(
                "Every product category assignment must name a category in the same catalogue snapshot.",
                nameof(products));
        }
    }

    private static void ValidateUniqueProducts(DatabaseProduct[] products)
    {
        if (products.Select(product => product.Id).Distinct().Count() != products.Length)
        {
            throw new ArgumentException(
                "A catalogue revision cannot contain two current revisions of the same database product.",
                nameof(products));
        }
    }

    private static void ValidateUniqueDocumentVersions(
        DocumentVersion[] documents)
    {
        var uniqueKeys = documents
            .Select(document => (document.Id, document.Version))
            .Distinct()
            .Count();

        if (uniqueKeys != documents.Length)
        {
            throw new ArgumentException(
                "A catalogue revision cannot repeat a document-version identity.",
                nameof(documents));
        }

        var multipleActiveVersions = documents
            .Where(document => document.Status == CatalogueItemStatus.Active)
            .GroupBy(document => document.Id)
            .Any(group => group.Count() > 1);

        if (multipleActiveVersions)
        {
            throw new ArgumentException(
                "A logical document can have at most one active version in a catalogue revision.",
                nameof(documents));
        }
    }

    private static void ValidateDocumentOwnership(
        DatabaseProduct[] products,
        DocumentVersion[] documents)
    {
        var productRevisions = products.ToDictionary(
            product => product.Id,
            product => product.Revision);

        foreach (var document in documents)
        {
            if (!productRevisions.TryGetValue(
                    document.DatabaseProductId,
                    out var productRevision) ||
                productRevision != document.DatabaseProductRevision)
            {
                throw new ArgumentException(
                    "Every document version must name a database product revision in the same catalogue snapshot.",
                    nameof(documents));
            }
        }
    }

    private static void ValidateActiveEvidence(
        DatabaseProduct[] products,
        DocumentVersion[] documents)
    {
        var activeDocumentsByProduct = documents
            .Where(document => document.Status == CatalogueItemStatus.Active)
            .GroupBy(document => document.DatabaseProductId)
            .ToDictionary(group => group.Key, group => group.Count());

        foreach (var product in products)
        {
            var activeDocumentCount = activeDocumentsByProduct.GetValueOrDefault(product.Id);

            if (product.Status == CatalogueItemStatus.Active && activeDocumentCount == 0)
            {
                throw new ArgumentException(
                    "Every active database product requires at least one active document version.",
                    nameof(documents));
            }

            if (product.Status != CatalogueItemStatus.Active && activeDocumentCount > 0)
            {
                throw new ArgumentException(
                    "An active document version cannot belong to a non-active database product.",
                    nameof(documents));
            }
        }
    }
}
