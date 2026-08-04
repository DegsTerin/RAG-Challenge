// Purpose: Canonicalises parser, chunking, embedding and vector-store descriptors so a generation cannot silently reuse incompatible derived artefacts.
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;

using RagChallenge.Application.Documents;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Application.IndexingRetrieval;

public sealed class IndexCompatibilityProfile
{
    private const int MaximumDescriptorLength = 512;

    public IndexCompatibilityProfile(
        IEnumerable<string> parserDescriptors,
        ChunkingPolicy chunkingPolicy,
        EmbeddingProviderDescriptor embeddingDescriptor,
        string vectorStoreDescriptor)
    {
        ArgumentNullException.ThrowIfNull(parserDescriptors);
        ChunkingPolicy = chunkingPolicy ??
            throw new ArgumentNullException(nameof(chunkingPolicy));
        EmbeddingDescriptor = embeddingDescriptor ??
            throw new ArgumentNullException(nameof(embeddingDescriptor));
        VectorStoreDescriptor = ValidateDescriptor(
            vectorStoreDescriptor,
            nameof(vectorStoreDescriptor));

        var parsers = parserDescriptors
            .Select(value => ValidateDescriptor(value, nameof(parserDescriptors)))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        if (parsers.Length == 0)
        {
            throw new ArgumentException(
                "At least one parser compatibility descriptor is required.",
                nameof(parserDescriptors));
        }

        ParserDescriptors = Array.AsReadOnly(parsers);
        Key = CreateKey();
    }

    public ReadOnlyCollection<string> ParserDescriptors { get; }

    public ChunkingPolicy ChunkingPolicy { get; }

    public EmbeddingProviderDescriptor EmbeddingDescriptor { get; }

    public string VectorStoreDescriptor { get; }

    public IndexCompatibilityKey Key { get; }

    private IndexCompatibilityKey CreateKey()
    {
        var canonical = new StringBuilder("index-compatibility-v1\n");

        foreach (var parser in ParserDescriptors)
        {
            Append(canonical, "parser", parser);
        }

        Append(canonical, "chunker", ChunkingPolicy.CompatibilityDescriptor);
        Append(canonical, "embedding-provider", EmbeddingDescriptor.ProviderId);
        Append(canonical, "embedding-model", EmbeddingDescriptor.ModelId);
        Append(canonical, "embedding-revision", EmbeddingDescriptor.ModelRevision);
        Append(
            canonical,
            "embedding-dimensions",
            EmbeddingDescriptor.Dimensions.ToString(
                System.Globalization.CultureInfo.InvariantCulture));
        Append(canonical, "vector-store", VectorStoreDescriptor);
        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString()));
        return new IndexCompatibilityKey(Convert.ToHexString(digest).ToLowerInvariant());
    }

    private static void Append(StringBuilder builder, string name, string value)
    {
        builder.Append(name);
        builder.Append(':');
        builder.Append(value.Length.ToString(System.Globalization.CultureInfo.InvariantCulture));
        builder.Append(':');
        builder.Append(value);
        builder.Append('\n');
    }

    private static string ValidateDescriptor(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);

        if (value.Length > MaximumDescriptorLength ||
            value.Any(character => character is < ' ' or > '~'))
        {
            throw new ArgumentException(
                "A compatibility descriptor must be bounded printable ASCII.",
                parameterName);
        }

        return value;
    }
}
