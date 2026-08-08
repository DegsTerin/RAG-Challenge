// Purpose: Verifies AnswerEvidenceRecordV1 identity, canonical ordering, privacy minimisation, exact PDF bindings, coverage digest, and fixed P30D retention independently of persistence.
using System.Security.Cryptography;
using System.Text;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.UnitTests;

public sealed class AnswerEvidenceRecordPolicyTests
{
    [Fact]
    public void RecordIdentityUsesOpaqueLowercaseUuidNShape()
    {
        var id = AnswerEvidenceRecordId.FromGuid(
            Guid.Parse("00112233-4455-6677-8899-aabbccddeeff"));

        Assert.Equal("ans-evidence-00112233445566778899aabbccddeeff", id.Value);
        Assert.Throws<ArgumentException>(() => new AnswerEvidenceRecordId(
            "ans-evidence-00112233445566778899AABBCCDDEEFF"));
        Assert.Throws<ArgumentException>(() => new AnswerEvidenceRecordId(
            "00112233445566778899aabbccddeeff"));
    }

    [Fact]
    public void CanonicalRecordHasStableGoldenVector()
    {
        var record = CsvRecord();

        Assert.Equal(
            "819e0c9b3f925a09e8651d3cfe77dd897fdae193e6c810b93d585d4a388da004",
            record.RecordSha256.Value);
        Assert.Equal(record.RecordSha256, new AnswerEvidenceRecordSha256(
            Hash(record.SerialiseCanonicalUtf8())));
    }

    [Fact]
    public void CitationAndColumnInputOrderCanonicaliseWithoutChangingDigest()
    {
        var first = CsvRecord(
            [CsvCitation(1, "document-a", ["zeta", "alpha"]),
             CsvCitation(2, "document-b", ["beta"])]);
        var second = CsvRecord(
            [CsvCitation(2, "document-b", ["beta"]),
             CsvCitation(1, "document-a", ["alpha", "zeta"])]);

        Assert.Equal(first.RecordSha256, second.RecordSha256);
        Assert.Equal(["alpha", "zeta"], first.Citations[0].Columns);
        Assert.Equal([1, 2], second.Citations.Select(citation => citation.Ordinal));
    }

    [Fact]
    public void DuplicateCitationAndColumnIdentitiesFailClosed()
    {
        Assert.Throws<ArgumentException>(() => CsvRecord(
            [CsvCitation(1, "document-a", ["alpha"]),
             CsvCitation(1, "document-b", ["beta"])]));
        Assert.Throws<ArgumentException>(() => CsvCitation(
            1,
            "document-a",
            ["duplicate", "duplicate"]));
    }

    [Fact]
    public void CanonicalColumnMetadataCannotExceedItsPersistenceBound()
    {
        var columns = Enumerable.Range(0, 64)
            .Select(index => $"{index:D3}{new string('x', 125)}")
            .ToArray();

        Assert.Throws<ArgumentException>(() => CsvCitation(
            1,
            "document-a",
            columns));
    }

    [Fact]
    public void PdfCitationRequiresEveryExactPhysicalPageAndRejectsExtras()
    {
        var (citation, pages) = PdfBindings();
        var record = Record([citation], pages);

        Assert.Equal([1, 2], record.PageImages.Select(page => page.PageNumber));
        Assert.Throws<ArgumentException>(() => Record([citation], pages.Take(1)));
        Assert.Throws<ArgumentException>(() => Record(
            [citation],
            pages.Append(CreatePage(pageNumber: 3, citation.RenderManifestId!))));
    }

    [Fact]
    public void FixedRetentionAndCanonicalReadbackNeverRefreshInstants()
    {
        var record = CsvRecord();
        var rehydrated = AnswerEvidenceRecordV1.Rehydrate(
            record.RecordSha256,
            record.AnswerEvidenceRecordId,
            record.CorpusId,
            record.ActivationRecordRevision,
            record.CatalogueRevision,
            record.SourceBindingSetDigest,
            record.ActivationBindingSetDigest,
            record.IndexGenerationId,
            record.QuestionLanguage,
            record.AnswerSha256,
            record.AnswerUtf8ByteLength,
            record.EvidenceCoverageDigest,
            record.RetrievalPolicyVersion,
            record.PromptVersion,
            record.LanguageModelDescriptor,
            record.CorrelationId,
            record.CreatedAt,
            record.ExpiresAt,
            record.Citations,
            record.PageImages);

        Assert.Equal(TimeSpan.FromDays(30), rehydrated.ExpiresAt - rehydrated.CreatedAt);
        Assert.Equal(record.CreatedAt, rehydrated.CreatedAt);
        Assert.Throws<InvalidDataException>(() => AnswerEvidenceRecordV1.Rehydrate(
            record.RecordSha256,
            record.AnswerEvidenceRecordId,
            record.CorpusId,
            record.ActivationRecordRevision,
            record.CatalogueRevision,
            record.SourceBindingSetDigest,
            record.ActivationBindingSetDigest,
            record.IndexGenerationId,
            record.QuestionLanguage,
            record.AnswerSha256,
            record.AnswerUtf8ByteLength,
            record.EvidenceCoverageDigest,
            record.RetrievalPolicyVersion,
            record.PromptVersion,
            record.LanguageModelDescriptor,
            record.CorrelationId,
            record.CreatedAt,
            record.ExpiresAt.AddTicks(1),
            record.Citations,
            record.PageImages));
    }

    [Fact]
    public void CanonicalBytesContainNoQuestionAnswerOrSourceDisplayText()
    {
        var record = CsvRecord();
        var canonical = Encoding.UTF8.GetString(record.SerialiseCanonicalUtf8());

        Assert.DoesNotContain("What is secret?", canonical, StringComparison.Ordinal);
        Assert.DoesNotContain("Secret answer text", canonical, StringComparison.Ordinal);
        Assert.DoesNotContain("Source excerpt", canonical, StringComparison.Ordinal);
        Assert.DoesNotContain("https://", canonical, StringComparison.Ordinal);
        Assert.Contains(record.AnswerSha256.Value, canonical, StringComparison.Ordinal);
    }

    [Fact]
    public void CoverageDigestIsOrderIndependentAndStateSensitive()
    {
        var first = new EvidenceCoverage(
            2,
            3,
            1,
            1,
            new Dictionary<string, SourceFreshness>
            {
                ["source-b"] = SourceFreshness.Stale,
                ["source-a"] = SourceFreshness.Withdrawn,
            });
        var reordered = first with
        {
            DegradedSources = new Dictionary<string, SourceFreshness>
            {
                ["source-a"] = SourceFreshness.Withdrawn,
                ["source-b"] = SourceFreshness.Stale,
            },
        };
        var changed = first with
        {
            DegradedSources = new Dictionary<string, SourceFreshness>
            {
                ["source-a"] = SourceFreshness.Withdrawn,
                ["source-b"] = SourceFreshness.Deactivated,
            },
        };

        Assert.Equal(
            EvidenceCoverageCanonicalizer.Digest(first),
            EvidenceCoverageCanonicalizer.Digest(reordered));
        Assert.NotEqual(
            EvidenceCoverageCanonicalizer.Digest(first),
            EvidenceCoverageCanonicalizer.Digest(changed));
    }

    private static AnswerEvidenceRecordV1 CsvRecord(
        IEnumerable<AnswerEvidenceCitationBindingV1>? citations = null) =>
        Record(citations ?? [CsvCitation(1, "document-a", ["alpha", "beta"])], []);

    private static AnswerEvidenceRecordV1 Record(
        IEnumerable<AnswerEvidenceCitationBindingV1> citations,
        IEnumerable<AnswerEvidencePageBindingV1> pages) =>
        AnswerEvidenceRecordV1.Create(
            new AnswerEvidenceRecordId("ans-evidence-00000000000000000000000000000001"),
            new CorpusId("main-corpus"),
            new ActivationRecordRevision(3),
            new CatalogueRevision(4),
            new SourceBindingSetDigest(Hash("source-bindings")),
            new ActivationBindingSetDigest(Hash("activation-bindings")),
            new IndexGenerationId($"idxgen-{Hash("generation")}"),
            SupportedQueryLanguage.EnGb,
            new AnswerSha256(Hash("Secret answer text")),
            Encoding.UTF8.GetByteCount("Secret answer text"),
            new EvidenceCoverageDigest(Hash("coverage")),
            "retrieval-v1",
            "grounded-answer-v1",
            new AnswerLanguageModelDescriptorV1("synthetic", "model-v1", "revision-1"),
            "correlation-golden",
            At,
            citations,
            pages);

    private static AnswerEvidenceCitationBindingV1 CsvCitation(
        int ordinal,
        string documentId,
        IEnumerable<string> columns) =>
        new(
            ordinal,
            new DatabaseProductId("database-a"),
            new DatabaseProductRevision(2),
            new DocumentId(documentId),
            new DocumentVersionNumber(3),
            DocumentFormat.Csv,
            DocumentContentLanguage.EnGb,
            $"chunk-{Hash(documentId)}",
            new SourceAdapterId("local-csv"),
            SourceTrustClass.LocalAuthorised,
            officialSourceRegistrationId: null,
            sourceSnapshotId: null,
            sourceObservationId: null,
            new ContentObjectId(Hash($"source-{documentId}")),
            pageStart: null,
            pageEnd: null,
            recordStart: 7,
            recordEnd: 7,
            columns,
            sectionLocator: "table:features",
            renderManifestId: null);

    private static (AnswerEvidenceCitationBindingV1 Citation,
        AnswerEvidencePageBindingV1[] Pages) PdfBindings()
    {
        var manifestId = new RenderManifestId($"rendermanifest-{Hash("manifest")}");
        var citation = new AnswerEvidenceCitationBindingV1(
            1,
            new DatabaseProductId("database-a"),
            new DatabaseProductRevision(2),
            new DocumentId("document-pdf"),
            new DocumentVersionNumber(3),
            DocumentFormat.Pdf,
            DocumentContentLanguage.EnGb,
            $"chunk-{Hash("pdf-chunk")}",
            new SourceAdapterId("local-pdf"),
            SourceTrustClass.LocalAuthorised,
            officialSourceRegistrationId: null,
            sourceSnapshotId: null,
            sourceObservationId: null,
            new ContentObjectId(Hash("pdf-source")),
            pageStart: 1,
            pageEnd: 2,
            recordStart: null,
            recordEnd: null,
            columns: null,
            sectionLocator: "section:1",
            manifestId);
        return (citation, [CreatePage(1, manifestId), CreatePage(2, manifestId)]);
    }

    private static AnswerEvidencePageBindingV1 CreatePage(
        int pageNumber,
        RenderManifestId manifestId)
    {
        var image = Hash($"page-{pageNumber}");
        return new AnswerEvidencePageBindingV1(
            new DocumentId("document-pdf"),
            new DocumentVersionNumber(3),
            new ContentObjectId(Hash("pdf-source")),
            pageNumber,
            manifestId,
            new RenderProfileId(RenderProfileId.PdfPagePngV1),
            new RendererDescriptor("renderer.synthetic:v1"),
            new ContentObjectId(image),
            new ImageSha256(image),
            byteLength: 4096 + pageNumber,
            DocumentPageImage.PngMediaType,
            widthPixels: 1024,
            heightPixels: 768);
    }

    private static readonly DateTimeOffset At =
        new(2026, 8, 8, 3, 0, 0, TimeSpan.Zero);

    private static string Hash(string value) => Hash(Encoding.UTF8.GetBytes(value));

    private static string Hash(ReadOnlySpan<byte> value) =>
        Convert.ToHexString(SHA256.HashData(value)).ToLowerInvariant();
}
