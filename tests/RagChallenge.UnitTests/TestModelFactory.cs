// Purpose: Supplies deterministic Domain and Application test objects without external services, clocks, persistence, or production corpus data.
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.UnitTests;

internal static class TestModelFactory
{
    internal static readonly DateTimeOffset Now =
        new(2026, 8, 2, 12, 0, 0, TimeSpan.Zero);

    internal static DocumentBinding LocalBinding(
        string productId = "postgresql",
        string documentId = "postgresql-guide",
        string sourceAdapterId = "local-directory") =>
        new(
            new DatabaseProductId(productId),
            new DatabaseProductRevision(1),
            new DocumentId(documentId),
            new DocumentVersionNumber(1),
            DocumentFormat.Pdf,
            new SourceAdapterId(sourceAdapterId),
            SourceTrustClass.LocalAuthorised);

    internal static DocumentBinding OfficialBinding(
        string productId = "redis",
        string documentId = "redis-reference",
        string registrationId = "official-redis-v1",
        string snapshotId = "snapshot-redis-v1",
        string observationId = "observation-redis-v1") =>
        new(
            new DatabaseProductId(productId),
            new DatabaseProductRevision(1),
            new DocumentId(documentId),
            new DocumentVersionNumber(1),
            DocumentFormat.Csv,
            new SourceAdapterId("official-static-document"),
            SourceTrustClass.OfficialExternal,
            new OfficialSourceRegistrationId(registrationId),
            new OfficialSnapshotId(snapshotId),
            new OfficialObservationId(observationId));

    internal static OfficialSourceObservation Observation(
        string observationId = "observation-redis-v1",
        string registrationId = "official-redis-v1",
        string snapshotId = "snapshot-redis-v1",
        OfficialObservationState state = OfficialObservationState.Current,
        DateTimeOffset? revalidatedAt = null,
        TimeSpan? maxAge = null) =>
        new(
            new OfficialObservationId(observationId),
            new OfficialSourceRegistrationId(registrationId),
            new OfficialSnapshotId(snapshotId),
            new ObservationJournalRevision(1),
            state,
            revalidatedAt ?? Now.AddHours(-1),
            maxAge ?? TimeSpan.FromHours(24));

    internal static FinalisedIndexGenerationManifest Manifest(
        IEnumerable<DocumentBinding> bindings,
        char digestCharacter = 'a',
        string corpusId = "mvp-database-documentation",
        long catalogueRevision = 1)
    {
        var materialisedBindings = bindings.ToArray();
        var activeDigest = BindingDigestCanonicalizer
            .CanonicaliseActiveDocumentSet(materialisedBindings)
            .Digest;
        var sourceDigest = BindingDigestCanonicalizer
            .CanonicaliseSourceBindingSet(materialisedBindings)
            .Digest;
        var contentDigest = new string(digestCharacter, 64);

        return new FinalisedIndexGenerationManifest(
            manifestSchemaVersion: 1,
            new CorpusId(corpusId),
            new CorpusRevision(1),
            new CatalogueRevision(catalogueRevision),
            activeDigest,
            sourceDigest,
            new IndexCompatibilityKey(new string('b', 64)),
            new GenerationSpecDigest(new string('c', 64)),
            chunkCount: 2,
            vectorCount: 2,
            new LogicalArtifactDigest(new string('d', 64)),
            new GenerationContentDigest(contentDigest),
            new IndexGenerationId($"idxgen-{contentDigest}"));
    }

    internal static CorpusActivationRecord InitialRecord(
        FinalisedIndexGenerationManifest manifest,
        IEnumerable<DocumentBinding> bindings) =>
        ActivationRecordFactory.CreateInitial(
            manifest,
            bindings.Select(binding => Evidence(binding)),
            Now);

    internal static DocumentActivationEvidenceBinding Evidence(
        DocumentBinding binding,
        DocumentRightDecisionState defaultState = DocumentRightDecisionState.Permitted,
        DocumentRight? overriddenRight = null,
        DocumentRightDecisionState? overriddenState = null,
        char sourceDigestCharacter = 'e')
    {
        var rights = new DocumentRightsEligibilityRecordV1(
            binding.DocumentId,
            binding.DocumentVersion,
            Enum.GetValues<DocumentRight>().Select(right => new DocumentRightDecision(
                right,
                right == overriddenRight ? overriddenState!.Value : defaultState,
                new DocumentRightsEvidenceReference($"test-rights-{right}"))));
        return new DocumentActivationEvidenceBinding(
            binding,
            new ContentObjectId(new string(sourceDigestCharacter, 64)),
            rights,
            binding.DocumentFormat == DocumentFormat.Pdf
                ? new RenderManifestId($"rendermanifest-{new string('f', 64)}")
                : null);
    }

    internal static string FindTestData(string filename)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var candidate = Path.Combine(
                current.FullName,
                "tests",
                "RagChallenge.UnitTests",
                "TestData",
                filename);

            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new FileNotFoundException("The deterministic test fixture could not be found.", filename);
    }
}
