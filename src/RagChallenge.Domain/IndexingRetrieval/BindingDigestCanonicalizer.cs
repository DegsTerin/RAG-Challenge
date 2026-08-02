// Purpose: Implements the accepted versioned canonical byte domains for document, generation-source, and complete activation binding digests.
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Domain.IndexingRetrieval;

public sealed record ActiveDocumentSetCanonicalValue(
    string CanonicalText,
    ActiveDocumentSetDigest Digest);

public sealed record SourceBindingSetCanonicalValue(
    string CanonicalText,
    SourceBindingSetDigest Digest);

public sealed record ActivationBindingSetCanonicalValue(
    string CanonicalText,
    ActivationBindingSetDigest Digest);

public static class BindingDigestCanonicalizer
{
    public const string ActiveDocumentSetDomain =
        "rag-challenge/active-document-set/v1";

    public const string SourceBindingSetDomain =
        "rag-challenge/source-binding-set/v1";

    public const string ActivationBindingSetDomain =
        "rag-challenge/activation-binding-set/v1";

    public static ActiveDocumentSetCanonicalValue CanonicaliseActiveDocumentSet(
        IEnumerable<DocumentBinding> bindings)
    {
        var ordered = OrderAndValidate(bindings);
        EnsureUniqueActiveDocuments(ordered);
        var canonicalText = Serialise(
            ActiveDocumentSetDomain,
            ordered,
            includeSource: false,
            includeObservation: false);

        return new ActiveDocumentSetCanonicalValue(
            canonicalText,
            new ActiveDocumentSetDigest(Hash(canonicalText)));
    }

    public static SourceBindingSetCanonicalValue CanonicaliseSourceBindingSet(
        IEnumerable<DocumentBinding> bindings)
    {
        var ordered = OrderAndValidate(bindings);
        var canonicalText = Serialise(
            SourceBindingSetDomain,
            ordered,
            includeSource: true,
            includeObservation: false);

        return new SourceBindingSetCanonicalValue(
            canonicalText,
            new SourceBindingSetDigest(Hash(canonicalText)));
    }

    public static ActivationBindingSetCanonicalValue CanonicaliseActivationBindingSet(
        IEnumerable<DocumentBinding> bindings)
    {
        var ordered = OrderAndValidate(bindings);
        var canonicalText = Serialise(
            ActivationBindingSetDomain,
            ordered,
            includeSource: true,
            includeObservation: true);

        return new ActivationBindingSetCanonicalValue(
            canonicalText,
            new ActivationBindingSetDigest(Hash(canonicalText)));
    }

    internal static IReadOnlyList<DocumentBinding> OrderAndValidate(
        IEnumerable<DocumentBinding> bindings)
    {
        ArgumentNullException.ThrowIfNull(bindings);

        var ordered = bindings
            .Order(DocumentBindingGenerationComparer.Instance)
            .ToArray();

        if (ordered.Length == 0)
        {
            throw new ArgumentException(
                "A canonical binding set cannot be empty.",
                nameof(bindings));
        }

        for (var index = 1; index < ordered.Length; index++)
        {
            if (DocumentBindingGenerationComparer.Instance.Compare(
                    ordered[index - 1],
                    ordered[index]) == 0)
            {
                throw new ArgumentException(
                    "A canonical binding set cannot repeat a generation-bound projection.",
                    nameof(bindings));
            }
        }

        return Array.AsReadOnly(ordered);
    }

    private static void EnsureUniqueActiveDocuments(
        IReadOnlyList<DocumentBinding> bindings)
    {
        var duplicate = bindings
            .GroupBy(binding => new
            {
                binding.DatabaseProductId,
                binding.DatabaseProductRevision,
                binding.DocumentId,
                binding.DocumentVersion,
                binding.DocumentFormat,
            })
            .Any(group => group.Count() > 1);

        if (duplicate)
        {
            throw new ArgumentException(
                "An active document projection cannot have more than one source binding.",
                nameof(bindings));
        }
    }

    private static string Serialise(
        string domain,
        IReadOnlyList<DocumentBinding> bindings,
        bool includeSource,
        bool includeObservation)
    {
        var builder = new StringBuilder();
        AppendToken(builder, domain);
        AppendToken(
            builder,
            bindings.Count.ToString(CultureInfo.InvariantCulture));

        foreach (var binding in bindings)
        {
            AppendToken(builder, binding.DatabaseProductId.Value);
            AppendToken(builder, binding.DatabaseProductRevision.ToCanonicalString());
            AppendToken(builder, binding.DocumentId.Value);
            AppendToken(builder, binding.DocumentVersion.ToCanonicalString());
            AppendToken(builder, binding.DocumentFormat.ToString());

            if (!includeSource)
            {
                continue;
            }

            AppendToken(builder, binding.SourceAdapterId.Value);
            AppendToken(builder, binding.SourceTrustClass.ToString());
            AppendToken(builder, binding.OfficialSourceRegistrationId?.Value);
            AppendToken(builder, binding.OfficialSnapshotId?.Value);

            if (includeObservation)
            {
                AppendToken(builder, binding.SourceObservationId?.Value);
            }
        }

        return builder.ToString();
    }

    private static void AppendToken(StringBuilder builder, string? value)
    {
        if (value is null)
        {
            builder.Append("-1:");
            return;
        }

        builder.Append(Encoding.UTF8.GetByteCount(value));
        builder.Append(':');
        builder.Append(value);
    }

    private static string Hash(string canonicalText) =>
        Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(canonicalText)))
            .ToLowerInvariant();
}

internal sealed class DocumentBindingGenerationComparer
    : IComparer<DocumentBinding>
{
    internal static DocumentBindingGenerationComparer Instance { get; } = new();

    public int Compare(DocumentBinding? left, DocumentBinding? right)
    {
        if (ReferenceEquals(left, right))
        {
            return 0;
        }

        if (left is null)
        {
            return -1;
        }

        if (right is null)
        {
            return 1;
        }

        return CompareFields(
            left.DatabaseProductId.Value,
            right.DatabaseProductId.Value,
            left.DatabaseProductRevision.ToCanonicalString(),
            right.DatabaseProductRevision.ToCanonicalString(),
            left.DocumentId.Value,
            right.DocumentId.Value,
            left.DocumentVersion.ToCanonicalString(),
            right.DocumentVersion.ToCanonicalString(),
            left.DocumentFormat.ToString(),
            right.DocumentFormat.ToString(),
            left.SourceAdapterId.Value,
            right.SourceAdapterId.Value,
            left.SourceTrustClass.ToString(),
            right.SourceTrustClass.ToString(),
            left.OfficialSourceRegistrationId?.Value,
            right.OfficialSourceRegistrationId?.Value,
            left.OfficialSnapshotId?.Value,
            right.OfficialSnapshotId?.Value);
    }

    private static int CompareFields(params string?[] values)
    {
        for (var index = 0; index < values.Length; index += 2)
        {
            var comparison = CompareNullableOrdinal(values[index], values[index + 1]);

            if (comparison != 0)
            {
                return comparison;
            }
        }

        return 0;
    }

    private static int CompareNullableOrdinal(string? left, string? right)
    {
        if (left is null)
        {
            return right is null ? 0 : -1;
        }

        return right is null ? 1 : StringComparer.Ordinal.Compare(left, right);
    }
}
