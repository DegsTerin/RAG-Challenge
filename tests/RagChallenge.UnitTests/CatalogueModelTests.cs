// Purpose: Verifies catalogue identities, many-to-many category assignments, lifecycle states, document ownership, and immutable source metadata constraints.
using System.Text.Json;

using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.UnitTests;

public sealed class CatalogueModelTests
{
    [Fact]
    public void InitialCatalogueFixtureHasTheAcceptedCardinalitiesAndAssignments()
    {
        using var fixture = JsonDocument.Parse(
            File.ReadAllText(
                TestModelFactory.FindTestData("initial-catalogue-v1.json")));
        var root = fixture.RootElement;
        var categories = root.GetProperty("categories")
            .EnumerateArray()
            .Select(element => new DatabaseCategory(
                new DatabaseCategoryId(element.GetProperty("id").GetString()!),
                element.GetProperty("displayName").GetString()!))
            .ToArray();
        var products = root.GetProperty("products")
            .EnumerateArray()
            .Select(ReadProduct)
            .ToArray();
        var snapshot = new CatalogueSnapshot(
            new CorpusId(root.GetProperty("corpusId").GetString()!),
            new CatalogueRevision(1),
            categories,
            products,
            []);

        Assert.Equal(root.GetProperty("expectedProductCount").GetInt32(), products.Length);
        Assert.Equal(root.GetProperty("expectedCategoryCount").GetInt32(), categories.Length);
        Assert.Equal(
            root.GetProperty("expectedAssignmentCount").GetInt32(),
            products.Sum(product => product.CategoryIds.Count));
        Assert.Equal(51, products.Select(product => product.Id).Distinct().Count());
        Assert.All(products, product => Assert.Equal(CatalogueItemStatus.Candidate, product.Status));
        Assert.Equal(3, products.Count(product => product.CategoryIds.Count == 2));
        Assert.Equal(
            ["redis", "sap-hana", "singlestore"],
            products
                .Where(product => product.CategoryIds.Count == 2)
                .Select(product => product.Id.Value)
                .Order(StringComparer.Ordinal));
        Assert.Equal(51, snapshot.DatabaseProducts.Count);
        Assert.Equal(9, snapshot.DatabaseCategories.Count);
        Assert.Empty(snapshot.DocumentVersions);
    }

    [Fact]
    public void ActiveProductAndDocumentFormAValidSnapshot()
    {
        var category = Category();
        var product = Product(CatalogueItemStatus.Active);
        var document = Document(CatalogueItemStatus.Active);

        var snapshot = new CatalogueSnapshot(
            new CorpusId("mvp-database-documentation"),
            new CatalogueRevision(4),
            [category],
            [product],
            [document]);

        Assert.Equal(product, Assert.Single(snapshot.DatabaseProducts));
        Assert.Equal(document, Assert.Single(snapshot.DocumentVersions));
        Assert.Equal("application/pdf", document.MediaType);
        Assert.Equal(SupportedLanguage.EnGb, document.ContentLanguage);
    }

    [Fact]
    public void ActiveEvidenceAndOwnershipInvariantsFailClosed()
    {
        var category = Category();
        var activeProduct = Product(CatalogueItemStatus.Active);
        var candidateProduct = Product(CatalogueItemStatus.Candidate);

        Assert.Throws<ArgumentException>(
            () => new CatalogueSnapshot(
                new CorpusId("mvp-database-documentation"),
                new CatalogueRevision(1),
                [category],
                [activeProduct],
                []));
        Assert.Throws<ArgumentException>(
            () => new CatalogueSnapshot(
                new CorpusId("mvp-database-documentation"),
                new CatalogueRevision(1),
                [category],
                [candidateProduct],
                [Document(CatalogueItemStatus.Active)]));
        Assert.Throws<ArgumentException>(
            () => new CatalogueSnapshot(
                new CorpusId("mvp-database-documentation"),
                new CatalogueRevision(1),
                [category],
                [activeProduct],
                [Document(
                    CatalogueItemStatus.Active,
                    databaseProductRevision: 2)]));
    }

    [Fact]
    public void SnapshotRejectsDuplicateAndUnknownRelationships()
    {
        var category = Category();
        var product = Product(CatalogueItemStatus.Candidate);
        var document = Document(CatalogueItemStatus.Candidate);

        Assert.Throws<ArgumentException>(
            () => new CatalogueSnapshot(
                new CorpusId("mvp-database-documentation"),
                new CatalogueRevision(1),
                [category, category],
                [product],
                [document]));
        Assert.Throws<ArgumentException>(
            () => new CatalogueSnapshot(
                new CorpusId("mvp-database-documentation"),
                new CatalogueRevision(1),
                [new DatabaseCategory(new DatabaseCategoryId("different"), "Different")],
                [product],
                [document]));
        Assert.Throws<ArgumentException>(
            () => new CatalogueSnapshot(
                new CorpusId("mvp-database-documentation"),
                new CatalogueRevision(1),
                [category],
                [product, product],
                [document]));
        Assert.Throws<ArgumentException>(
            () => new CatalogueSnapshot(
                new CorpusId("mvp-database-documentation"),
                new CatalogueRevision(1),
                [category],
                [product],
                [document, document]));
    }

    [Fact]
    public void OneLogicalDocumentCannotHaveTwoActiveVersions()
    {
        var product = Product(CatalogueItemStatus.Active);
        var first = Document(CatalogueItemStatus.Active);
        var second = new DocumentVersion(
            first.Id,
            new DocumentVersionNumber(2),
            first.DatabaseProductId,
            first.DatabaseProductRevision,
            first.Format,
            first.ContentLanguage,
            CatalogueItemStatus.Active,
            new ContentObjectId(new string('b', 64)),
            byteLength: 200,
            first.MediaType,
            first.SourceAdapterId,
            first.SourceTrustClass);

        Assert.Throws<ArgumentException>(
            () => new CatalogueSnapshot(
                new CorpusId("mvp-database-documentation"),
                new CatalogueRevision(1),
                [Category()],
                [product],
                [first, second]));
    }

    [Fact]
    public void ProductAndDocumentConstructorsRejectInvalidCardinalities()
    {
        Assert.Throws<ArgumentException>(
            () => new DatabaseProduct(
                new DatabaseProductId("postgresql"),
                new DatabaseProductRevision(1),
                "PostgreSQL",
                CatalogueItemStatus.Candidate,
                []));
        Assert.Throws<ArgumentException>(
            () => new DatabaseProduct(
                new DatabaseProductId("postgresql"),
                new DatabaseProductRevision(1),
                "PostgreSQL",
                CatalogueItemStatus.Candidate,
                [new DatabaseCategoryId("relational-sql"), new DatabaseCategoryId("relational-sql")]));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Document(CatalogueItemStatus.Candidate, byteLength: 0));
        Assert.Throws<ArgumentException>(
            () => new DatabaseCategory(new DatabaseCategoryId("relational-sql"), " "));
    }

    [Fact]
    public void LocalAndOfficialDocumentIdentitiesRemainSeparated()
    {
        Assert.Throws<ArgumentException>(
            () => new DocumentVersion(
                new DocumentId("guide"),
                new DocumentVersionNumber(1),
                new DatabaseProductId("postgresql"),
                new DatabaseProductRevision(1),
                DocumentFormat.Pdf,
                SupportedLanguage.EnGb,
                CatalogueItemStatus.Candidate,
                new ContentObjectId(new string('a', 64)),
                100,
                "application/pdf",
                new SourceAdapterId("local-directory"),
                SourceTrustClass.LocalAuthorised,
                new OfficialSourceRegistrationId("unexpected")));
        Assert.Throws<ArgumentException>(
            () => new DocumentVersion(
                new DocumentId("reference"),
                new DocumentVersionNumber(1),
                new DatabaseProductId("redis"),
                new DatabaseProductRevision(1),
                DocumentFormat.Csv,
                SupportedLanguage.PtBr,
                CatalogueItemStatus.Candidate,
                new ContentObjectId(new string('a', 64)),
                100,
                "text/csv",
                new SourceAdapterId("official-static-document"),
                SourceTrustClass.OfficialExternal));
        Assert.Throws<ArgumentException>(
            () => new DocumentVersion(
                new DocumentId("reference"),
                new DocumentVersionNumber(1),
                new DatabaseProductId("redis"),
                new DatabaseProductRevision(1),
                DocumentFormat.Csv,
                SupportedLanguage.PtBr,
                CatalogueItemStatus.Active,
                new ContentObjectId(new string('a', 64)),
                100,
                "text/csv",
                new SourceAdapterId("official-static-document"),
                SourceTrustClass.OfficialExternal,
                new OfficialSourceRegistrationId("official-redis-v1")));
    }

    private static DatabaseCategory Category() =>
        new(new DatabaseCategoryId("relational-sql"), "Relational (SQL)");

    private static DatabaseProduct Product(CatalogueItemStatus status) =>
        new(
            new DatabaseProductId("postgresql"),
            new DatabaseProductRevision(1),
            "PostgreSQL",
            status,
            [new DatabaseCategoryId("relational-sql")]);

    private static DocumentVersion Document(
        CatalogueItemStatus status,
        long databaseProductRevision = 1,
        long byteLength = 100) =>
        new(
            new DocumentId("postgresql-guide"),
            new DocumentVersionNumber(1),
            new DatabaseProductId("postgresql"),
            new DatabaseProductRevision(databaseProductRevision),
            DocumentFormat.Pdf,
            SupportedLanguage.EnGb,
            status,
            new ContentObjectId(new string('a', 64)),
            byteLength,
            "application/pdf",
            new SourceAdapterId("local-directory"),
            SourceTrustClass.LocalAuthorised);

    private static DatabaseProduct ReadProduct(JsonElement element) =>
        new(
            new DatabaseProductId(element.GetProperty("id").GetString()!),
            new DatabaseProductRevision(1),
            element.GetProperty("displayName").GetString()!,
            Enum.Parse<CatalogueItemStatus>(element.GetProperty("status").GetString()!),
            element.GetProperty("categoryIds")
                .EnumerateArray()
                .Select(categoryId => new DatabaseCategoryId(categoryId.GetString()!)));
}
