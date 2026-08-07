// Purpose: Exercises typed identity syntax, closed lifecycle transitions, bilingual literals, and immutable official registration/snapshot constraints.
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.UnitTests;

public sealed class CatalogueIdentityAndSourceTests
{
    [Fact]
    public void TypedIdentitiesAndRevisionsPreserveCanonicalValues()
    {
        StableIdentifier[] identifiers =
        [
            new CorpusId("mvp-database-documentation"),
            new DatabaseProductId("PostgreSQL:16"),
            new DatabaseCategoryId("relational-sql"),
            new DocumentId("postgresql.guide"),
            new SourceAdapterId("local_directory"),
            new OfficialSourceRegistrationId("registration-1"),
            new OfficialSnapshotId("snapshot-1"),
            new OfficialObservationId("observation-1"),
            new CandidateBuildId("candidate-1"),
            new OperationId("operation-1"),
        ];
        PositiveRevision[] revisions =
        [
            new CorpusRevision(1),
            new CatalogueRevision(2),
            new ObservationJournalRevision(3),
            new ActivationRecordRevision(4),
            new DatabaseProductRevision(5),
            new DocumentVersionNumber(6),
            new SourceRegistrationRevision(7),
        ];

        Assert.Equal("mvp-database-documentation", identifiers[0].Value);
        Assert.Equal("PostgreSQL:16", identifiers[1].Value);
        Assert.Equal(["1", "2", "3", "4", "5", "6", "7"],
            revisions.Select(revision => revision.ToCanonicalString()));
        Assert.Equal(new DocumentId("postgresql.guide"), identifiers[3]);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" leading")]
    [InlineData("trailing ")]
    [InlineData("slash/not-allowed")]
    [InlineData("unicode-á")]
    public void StableIdentifierRejectsNonCanonicalValues(string value)
    {
        Assert.Throws<ArgumentException>(() => new DocumentId(value));
    }

    [Fact]
    public void CorpusAndDigestIdentitiesRequireTheirExactCanonicalSyntax()
    {
        Assert.Throws<ArgumentException>(() => new CorpusId("Uppercase"));
        Assert.Throws<ArgumentException>(() => new CorpusId(new string('a', 129)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CatalogueRevision(0));
        Assert.Throws<ArgumentException>(() => new ContentObjectId(new string('A', 64)));
        Assert.Throws<ArgumentException>(() => new SourceBindingSetDigest("abc"));
        Assert.Throws<ArgumentException>(
            () => new IndexGenerationId($"generation-{new string('a', 64)}"));

        var digest = new string('a', 64);
        Assert.Equal(digest, new ContentObjectId(digest).Value);
        Assert.Equal($"idxgen-{digest}", new IndexGenerationId($"idxgen-{digest}").Value);
    }

    [Theory]
    [InlineData(CatalogueItemStatus.Candidate, CatalogueItemStatus.Active)]
    [InlineData(CatalogueItemStatus.Candidate, CatalogueItemStatus.Removed)]
    [InlineData(CatalogueItemStatus.Active, CatalogueItemStatus.Deactivated)]
    [InlineData(CatalogueItemStatus.Deactivated, CatalogueItemStatus.Active)]
    [InlineData(CatalogueItemStatus.Deactivated, CatalogueItemStatus.Removed)]
    public void CatalogueLifecycleAllowsOnlyAcceptedTransitions(
        CatalogueItemStatus current,
        CatalogueItemStatus next)
    {
        Assert.True(CatalogueLifecycle.CanTransition(current, next));
        CatalogueLifecycle.EnsureTransition(current, next);
    }

    [Theory]
    [InlineData(CatalogueItemStatus.Candidate, CatalogueItemStatus.Deactivated)]
    [InlineData(CatalogueItemStatus.Active, CatalogueItemStatus.Removed)]
    [InlineData(CatalogueItemStatus.Removed, CatalogueItemStatus.Candidate)]
    [InlineData(CatalogueItemStatus.Active, CatalogueItemStatus.Active)]
    public void CatalogueLifecycleRejectsUnacceptedTransitions(
        CatalogueItemStatus current,
        CatalogueItemStatus next)
    {
        Assert.False(CatalogueLifecycle.CanTransition(current, next));
        Assert.Throws<InvalidOperationException>(
            () => CatalogueLifecycle.EnsureTransition(current, next));
    }

    [Fact]
    public void LanguageTagsAreClosedAndCanonical()
    {
        Assert.Equal("pt-BR", SupportedQueryLanguage.PtBr.ToCanonicalTag());
        Assert.Equal("en-GB", SupportedQueryLanguage.EnGb.ToCanonicalTag());
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ((SupportedQueryLanguage)99).ToCanonicalTag());
    }

    [Fact]
    public void OfficialRegistrationAcceptsOnlyCanonicalHttpsAuthority()
    {
        var registration = Registration("https://docs.example.com/reference.csv");

        Assert.Equal("https://docs.example.com/reference.csv", registration.CanonicalHttpsUrl);
        Assert.Equal(CatalogueItemStatus.Candidate, registration.Status);
        Assert.Throws<ArgumentException>(() => Registration("http://docs.example.com/reference.csv"));
        Assert.Throws<ArgumentException>(() => Registration("https://user@docs.example.com/reference.csv"));
        Assert.Throws<ArgumentException>(() => Registration("https://docs.example.com/reference.csv#fragment"));
        Assert.Throws<ArgumentException>(() => Registration("relative/reference.csv"));
    }

    [Fact]
    public void OfficialSnapshotRequiresReopenableBytesAndUtcTime()
    {
        var snapshot = new OfficialSourceSnapshot(
            new OfficialSnapshotId("snapshot-1"),
            new OfficialSourceRegistrationId("registration-1"),
            new ContentObjectId(new string('a', 64)),
            256,
            "text/csv",
            TestModelFactory.Now);

        Assert.Equal("2026-08-02T12:00:00.0000000+00:00", snapshot.ToCanonicalRetrievalInstant());
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new OfficialSourceSnapshot(
                snapshot.Id,
                snapshot.RegistrationId,
                snapshot.ContentObjectId,
                0,
                snapshot.MediaType,
                snapshot.RetrievedAt));
        Assert.Throws<ArgumentException>(
            () => new OfficialSourceSnapshot(
                snapshot.Id,
                snapshot.RegistrationId,
                snapshot.ContentObjectId,
                1,
                snapshot.MediaType,
                snapshot.RetrievedAt.ToOffset(TimeSpan.FromHours(-3))));
    }

    [Fact]
    public void ClosedEnumsRejectUndefinedValuesAtModelBoundaries()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new DatabaseProduct(
                new DatabaseProductId("postgresql"),
                new DatabaseProductRevision(1),
                "PostgreSQL",
                (CatalogueItemStatus)99,
                [new DatabaseCategoryId("relational-sql")]));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new DocumentBinding(
                new DatabaseProductId("postgresql"),
                new DatabaseProductRevision(1),
                new DocumentId("guide"),
                new DocumentVersionNumber(1),
                (DocumentFormat)99,
                new SourceAdapterId("local-directory"),
                SourceTrustClass.LocalAuthorised));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new DocumentBinding(
                new DatabaseProductId("postgresql"),
                new DatabaseProductRevision(1),
                new DocumentId("guide"),
                new DocumentVersionNumber(1),
                DocumentFormat.Pdf,
                new SourceAdapterId("local-directory"),
                (SourceTrustClass)99));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => TestModelFactory.Observation(state: (OfficialObservationState)99));
    }

    private static OfficialSourceRegistration Registration(string url) =>
        new(
            new OfficialSourceRegistrationId("registration-1"),
            new SourceRegistrationRevision(1),
            new DatabaseProductId("redis"),
            new DocumentId("redis-reference"),
            new SourceAdapterId("official-static-document"),
            url,
            CatalogueItemStatus.Candidate);
}
