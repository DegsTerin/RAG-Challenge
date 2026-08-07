// Purpose: Provides strongly typed catalogue, source, generation, revision, and digest identities without leaking storage-specific representations into Domain.
using System.Globalization;
using System.Text.RegularExpressions;

namespace RagChallenge.Domain.CorpusCatalog;

public abstract record StableIdentifier
{
    protected StableIdentifier(string value, string parameterName)
    {
        Value = IdentifierRules.RequireStableIdentifier(value, parameterName);
    }

    public string Value { get; }
}

public abstract record PositiveRevision
{
    protected PositiveRevision(long value, string parameterName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A canonical revision must be positive.");
        }

        Value = value;
    }

    public long Value { get; }

    public string ToCanonicalString() =>
        Value.ToString(CultureInfo.InvariantCulture);
}

public abstract record LowercaseSha256
{
    protected LowercaseSha256(string value, string parameterName)
    {
        Value = IdentifierRules.RequireLowercaseSha256(value, parameterName);
    }

    public string Value { get; }
}

public sealed record CorpusId : StableIdentifier
{
    public CorpusId(string value)
        : base(IdentifierRules.RequireLowercaseSlug(value, nameof(value)), nameof(value))
    {
    }
}

public sealed record DatabaseProductId : StableIdentifier
{
    public DatabaseProductId(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record DatabaseCategoryId : StableIdentifier
{
    public DatabaseCategoryId(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record DocumentId : StableIdentifier
{
    public DocumentId(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record SourceAdapterId : StableIdentifier
{
    public SourceAdapterId(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record OfficialSourceRegistrationId : StableIdentifier
{
    public OfficialSourceRegistrationId(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record OfficialSnapshotId : StableIdentifier
{
    public OfficialSnapshotId(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record OfficialObservationId : StableIdentifier
{
    public OfficialObservationId(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record CandidateBuildId : StableIdentifier
{
    public CandidateBuildId(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record OperationId : StableIdentifier
{
    public OperationId(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record CorpusRevision : PositiveRevision
{
    public CorpusRevision(long value)
        : base(value, nameof(value))
    {
    }
}

public sealed record CatalogueRevision : PositiveRevision
{
    public CatalogueRevision(long value)
        : base(value, nameof(value))
    {
    }
}

public sealed record ObservationJournalRevision : PositiveRevision
{
    public ObservationJournalRevision(long value)
        : base(value, nameof(value))
    {
    }
}

public sealed record ActivationRecordRevision : PositiveRevision
{
    public ActivationRecordRevision(long value)
        : base(value, nameof(value))
    {
    }
}

public sealed record DatabaseProductRevision : PositiveRevision
{
    public DatabaseProductRevision(long value)
        : base(value, nameof(value))
    {
    }
}

public sealed record DocumentVersionNumber : PositiveRevision
{
    public DocumentVersionNumber(long value)
        : base(value, nameof(value))
    {
    }
}

public sealed record SourceRegistrationRevision : PositiveRevision
{
    public SourceRegistrationRevision(long value)
        : base(value, nameof(value))
    {
    }
}

public sealed record ContentObjectId : LowercaseSha256
{
    public ContentObjectId(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record ImageSha256 : LowercaseSha256
{
    public ImageSha256(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record ManifestSha256 : LowercaseSha256
{
    public ManifestSha256(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record RenderProfileId : StableIdentifier
{
    public const string PdfPagePngV1 = "pdf-page-png-v1";

    public RenderProfileId(string value)
        : base(value, nameof(value))
    {
        if (!string.Equals(value, PdfPagePngV1, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The render profile must be the accepted 'pdf-page-png-v1' profile.",
                nameof(value));
        }
    }
}

public sealed record RendererDescriptor : StableIdentifier
{
    public RendererDescriptor(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record RenderManifestId
{
    private const string Prefix = "rendermanifest-";

    public RenderManifestId(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Length != Prefix.Length + 64 ||
            !value.StartsWith(Prefix, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A render manifest ID must use 'rendermanifest-' followed by a lowercase SHA-256 digest.",
                nameof(value));
        }

        _ = new ManifestSha256(value[Prefix.Length..]);
        Value = value;
    }

    public string Value { get; }

    public static RenderManifestId FromManifestSha256(ManifestSha256 manifestSha256)
    {
        ArgumentNullException.ThrowIfNull(manifestSha256);
        return new RenderManifestId(Prefix + manifestSha256.Value);
    }
}

public sealed record ActiveDocumentSetDigest : LowercaseSha256
{
    public ActiveDocumentSetDigest(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record SourceBindingSetDigest : LowercaseSha256
{
    public SourceBindingSetDigest(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record ActivationBindingSetDigest : LowercaseSha256
{
    public ActivationBindingSetDigest(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record IndexCompatibilityKey : LowercaseSha256
{
    public IndexCompatibilityKey(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record GenerationSpecDigest : LowercaseSha256
{
    public GenerationSpecDigest(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record LogicalArtifactDigest : LowercaseSha256
{
    public LogicalArtifactDigest(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record GenerationContentDigest : LowercaseSha256
{
    public GenerationContentDigest(string value)
        : base(value, nameof(value))
    {
    }
}

public sealed record IndexGenerationId
{
    private static readonly Regex Pattern = new(
        "^idxgen-[0-9a-f]{64}$",
        RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    public IndexGenerationId(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (!Pattern.IsMatch(value))
        {
            throw new ArgumentException(
                "An index generation ID must use 'idxgen-' followed by a lowercase SHA-256 digest.",
                nameof(value));
        }

        Value = value;
    }

    public string Value { get; }
}

internal static class IdentifierRules
{
    private const int MaximumIdentifierLength = 128;

    private static readonly Regex StableIdentifierPattern = new(
        "^[A-Za-z0-9][A-Za-z0-9._:-]*$",
        RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    private static readonly Regex LowercaseSlugPattern = new(
        "^[a-z0-9][a-z0-9._-]*$",
        RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    private static readonly Regex LowercaseSha256Pattern = new(
        "^[0-9a-f]{64}$",
        RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    internal static string RequireStableIdentifier(
        string value,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);

        if (value.Length is 0 or > MaximumIdentifierLength ||
            !StableIdentifierPattern.IsMatch(value))
        {
            throw new ArgumentException(
                "A stable identifier must be 1..128 ASCII letters, digits, '.', '_', ':', or '-', and begin with a letter or digit.",
                parameterName);
        }

        return value;
    }

    internal static string RequireLowercaseSlug(
        string value,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);

        if (value.Length is 0 or > MaximumIdentifierLength ||
            !LowercaseSlugPattern.IsMatch(value))
        {
            throw new ArgumentException(
                "A canonical slug must be 1..128 lowercase ASCII letters, digits, '.', '_', or '-', and begin with a letter or digit.",
                parameterName);
        }

        return value;
    }

    internal static string RequireLowercaseSha256(
        string value,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);

        if (!LowercaseSha256Pattern.IsMatch(value))
        {
            throw new ArgumentException(
                "A SHA-256 value must contain exactly 64 lowercase hexadecimal characters.",
                parameterName);
        }

        return value;
    }
}
