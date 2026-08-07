// Purpose: Verifies the provider-neutral content-object value contracts without selecting storage, rendering, or cleanup behaviour.
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.UnitTests;

public sealed class ContentObjectContractTests
{
    [Fact]
    public void MediaTypesAreCanonicalAndRejectParametersOrNonAsciiValues()
    {
        Assert.Equal("text/plain", new ContentMediaType("TEXT/PLAIN").Value);

        foreach (var value in new[]
        {
            string.Empty,
            "application",
            "application/pdf;version=1",
            "*/pdf",
            "têxt/plain",
        })
        {
            Assert.Throws<ArgumentException>(() => new ContentMediaType(value));
        }
    }

    [Fact]
    public void DescriptorsRequireMatchingSha256IdentityAndANonSecretImplementationId()
    {
        var contentObjectId = new ContentObjectId(new string('a', 64));
        var mismatch = new ContentObjectId(new string('b', 64));
        var implementation = new ContentStoreImplementationDescriptor(
            "filesystem-sha256-v1");
        var verification = new ContentObjectVerificationResult(
            ContentVerificationOutcome.Verified,
            ContentVerificationOutcome.Verified);

        var descriptor = new ContentObjectDescriptor(
            contentObjectId,
            contentObjectId,
            12,
            ContentMediaType.ApplicationPdf,
            implementation,
            ContentObjectWriteOutcome.Published,
            verification);

        Assert.Equal(contentObjectId, descriptor.Sha256);
        Assert.Equal("filesystem-sha256-v1", descriptor.Implementation.Value);
        Assert.Throws<ArgumentException>(() => new ContentObjectDescriptor(
            contentObjectId,
            mismatch,
            12,
            ContentMediaType.ApplicationPdf,
            implementation,
            ContentObjectWriteOutcome.Published,
            verification));
        Assert.Throws<ArgumentException>(
            () => new ContentStoreImplementationDescriptor("C:\\content\\objects"));
    }

    [Fact]
    public void VerifiedContentRequiresASeekableStreamPositionedAtZero()
    {
        var contentObjectId = new ContentObjectId(new string('a', 64));
        using var positioned = new MemoryStream([1, 2, 3]);
        positioned.Position = 1;

        Assert.Throws<ArgumentException>(() => new VerifiedContentObject(
            contentObjectId,
            contentObjectId,
            3,
            positioned,
            ContentVerificationOutcome.Verified));
    }
}
