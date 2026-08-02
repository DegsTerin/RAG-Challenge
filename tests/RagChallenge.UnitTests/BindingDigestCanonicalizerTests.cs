// Purpose: Executes the versioned golden binding vectors and adversarial ordering, duplication, trust, and freshness cases entirely in memory.
using System.Text.Json;

using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.UnitTests;

public sealed class BindingDigestCanonicalizerTests
{
    [Fact]
    public void GoldenVectorsMatchCanonicalTextAndBothDigestDomains()
    {
        using var fixture = JsonDocument.Parse(
            File.ReadAllText(
                TestModelFactory.FindTestData("binding-digest-golden-v1.json")));
        var root = fixture.RootElement;
        var bindings = root.GetProperty("bindings")
            .EnumerateArray()
            .Select(ReadBinding)
            .ToArray();

        var source = BindingDigestCanonicalizer
            .CanonicaliseSourceBindingSet(bindings);
        var activation = BindingDigestCanonicalizer
            .CanonicaliseActivationBindingSet(bindings);
        var expectedSource = root.GetProperty("sourceBindingSet");
        var expectedActivation = root.GetProperty("activationBindingSet");

        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(
            BindingDigestCanonicalizer.SourceBindingSetDomain,
            expectedSource.GetProperty("domain").GetString());
        Assert.Equal(
            expectedSource.GetProperty("canonicalText").GetString(),
            source.CanonicalText);
        Assert.Equal(
            expectedSource.GetProperty("sha256").GetString(),
            source.Digest.Value);
        Assert.Equal(
            BindingDigestCanonicalizer.ActivationBindingSetDomain,
            expectedActivation.GetProperty("domain").GetString());
        Assert.Equal(
            expectedActivation.GetProperty("canonicalText").GetString(),
            activation.CanonicalText);
        Assert.Equal(
            expectedActivation.GetProperty("sha256").GetString(),
            activation.Digest.Value);
    }

    [Fact]
    public void InputOrderDoesNotChangeCanonicalDigests()
    {
        DocumentBinding[] bindings =
        [
            TestModelFactory.OfficialBinding(),
            TestModelFactory.LocalBinding(),
        ];

        var sourceForward = BindingDigestCanonicalizer
            .CanonicaliseSourceBindingSet(bindings);
        var sourceReverse = BindingDigestCanonicalizer
            .CanonicaliseSourceBindingSet(bindings.Reverse());
        var activationForward = BindingDigestCanonicalizer
            .CanonicaliseActivationBindingSet(bindings);
        var activationReverse = BindingDigestCanonicalizer
            .CanonicaliseActivationBindingSet(bindings.Reverse());

        Assert.Equal(sourceForward, sourceReverse);
        Assert.Equal(activationForward, activationReverse);
    }

    [Fact]
    public void ObservationOnlyChangePreservesSourceDigestAndChangesActivationDigest()
    {
        var original = TestModelFactory.OfficialBinding();
        var rebound = original.WithObservation(
            new OfficialObservationId("observation-redis-v2"));

        var originalSource = BindingDigestCanonicalizer
            .CanonicaliseSourceBindingSet([original]);
        var reboundSource = BindingDigestCanonicalizer
            .CanonicaliseSourceBindingSet([rebound]);
        var originalActivation = BindingDigestCanonicalizer
            .CanonicaliseActivationBindingSet([original]);
        var reboundActivation = BindingDigestCanonicalizer
            .CanonicaliseActivationBindingSet([rebound]);

        Assert.Equal(originalSource.Digest, reboundSource.Digest);
        Assert.NotEqual(originalActivation.Digest, reboundActivation.Digest);
    }

    [Fact]
    public void EmptyAndDuplicateGenerationBindingsAreRejected()
    {
        var binding = TestModelFactory.LocalBinding();

        Assert.Throws<ArgumentException>(
            () => BindingDigestCanonicalizer.CanonicaliseSourceBindingSet([]));
        Assert.Throws<ArgumentException>(
            () => BindingDigestCanonicalizer
                .CanonicaliseActivationBindingSet([binding, binding]));
    }

    [Fact]
    public void ActiveDocumentProjectionRejectsTwoSourcesForOneVersion()
    {
        var first = TestModelFactory.LocalBinding(sourceAdapterId: "local-a");
        var second = TestModelFactory.LocalBinding(sourceAdapterId: "local-b");

        Assert.Throws<ArgumentException>(
            () => BindingDigestCanonicalizer
                .CanonicaliseActiveDocumentSet([first, second]));
    }

    [Fact]
    public void BindingTrustClassControlsOfficialIdentityFields()
    {
        Assert.Throws<ArgumentException>(
            () => new DocumentBinding(
                new DatabaseProductId("postgresql"),
                new DatabaseProductRevision(1),
                new DocumentId("guide"),
                new DocumentVersionNumber(1),
                DocumentFormat.Pdf,
                new SourceAdapterId("local-directory"),
                SourceTrustClass.LocalAuthorised,
                new OfficialSourceRegistrationId("registration")));
        Assert.Throws<ArgumentException>(
            () => new DocumentBinding(
                new DatabaseProductId("redis"),
                new DatabaseProductRevision(1),
                new DocumentId("reference"),
                new DocumentVersionNumber(1),
                DocumentFormat.Csv,
                new SourceAdapterId("official-static-document"),
                SourceTrustClass.OfficialExternal));
        Assert.Throws<InvalidOperationException>(
            () => TestModelFactory.LocalBinding().WithObservation(
                new OfficialObservationId("observation")));
    }

    [Fact]
    public void ObservationEligibilityIsCurrentBoundedAndUtc()
    {
        var observation = TestModelFactory.Observation();

        Assert.True(observation.IsEligibleAt(TestModelFactory.Now));
        Assert.True(observation.IsEligibleAt(observation.RevalidatedAt));
        Assert.False(observation.IsEligibleAt(observation.RevalidatedAt.AddMinutes(-1)));
        Assert.False(observation.IsEligibleAt(
            observation.RevalidatedAt + observation.MaxAge + TimeSpan.FromTicks(1)));
        Assert.False(TestModelFactory.Observation(
            state: OfficialObservationState.Withdrawn).IsEligibleAt(TestModelFactory.Now));
        Assert.Throws<ArgumentException>(
            () => observation.IsEligibleAt(TestModelFactory.Now.ToOffset(TimeSpan.FromHours(-3))));
        Assert.Throws<ArgumentException>(
            () => TestModelFactory.Observation(
                revalidatedAt: TestModelFactory.Now.ToOffset(TimeSpan.FromHours(-3))));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => TestModelFactory.Observation(maxAge: TimeSpan.Zero));
    }

    private static DocumentBinding ReadBinding(JsonElement element)
    {
        var trustClass = Enum.Parse<SourceTrustClass>(
            element.GetProperty("sourceTrustClass").GetString()!,
            ignoreCase: false);

        return new DocumentBinding(
            new DatabaseProductId(element.GetProperty("databaseProductId").GetString()!),
            new DatabaseProductRevision(
                element.GetProperty("databaseProductRevision").GetInt64()),
            new DocumentId(element.GetProperty("documentId").GetString()!),
            new DocumentVersionNumber(element.GetProperty("documentVersion").GetInt64()),
            Enum.Parse<DocumentFormat>(
                element.GetProperty("documentFormat").GetString()!,
                ignoreCase: false),
            new SourceAdapterId(element.GetProperty("sourceAdapterId").GetString()!),
            trustClass,
            ReadOptionalRegistration(element),
            ReadOptionalSnapshot(element),
            ReadOptionalObservation(element));
    }

    private static OfficialSourceRegistrationId? ReadOptionalRegistration(
        JsonElement element) =>
        element.GetProperty("officialSourceRegistrationId").ValueKind == JsonValueKind.Null
            ? null
            : new OfficialSourceRegistrationId(
                element.GetProperty("officialSourceRegistrationId").GetString()!);

    private static OfficialSnapshotId? ReadOptionalSnapshot(JsonElement element) =>
        element.GetProperty("sourceSnapshotId").ValueKind == JsonValueKind.Null
            ? null
            : new OfficialSnapshotId(
                element.GetProperty("sourceSnapshotId").GetString()!);

    private static OfficialObservationId? ReadOptionalObservation(JsonElement element) =>
        element.GetProperty("sourceObservationId").ValueKind == JsonValueKind.Null
            ? null
            : new OfficialObservationId(
                element.GetProperty("sourceObservationId").GetString()!);
}
