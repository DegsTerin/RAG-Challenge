// Purpose: Binds one non-secret operational authority reference to exactly one product provider operation before any credential lookup or egress.
namespace RagChallenge.Server.Api.OperationsGovernance;

internal enum ProductProviderOperation
{
    AdministrativeIndexEmbedding,
    QueryEmbedding,
    GroundedGeneration,
}

internal sealed record ProductProviderOperationalAuthority
{
    private ProductProviderOperationalAuthority(
        ProductProviderOperation operation,
        string reference)
    {
        Operation = operation;
        Reference = reference;
    }

    internal ProductProviderOperation Operation { get; }

    internal string Reference { get; }

    internal static ProductProviderOperationalAuthority Parse(
        ProductProviderOperation operation,
        string? reference)
    {
        if (!Enum.IsDefined(operation) ||
            string.IsNullOrWhiteSpace(reference) ||
            reference.Length < 8 ||
            reference.Length > 128 ||
            !reference.StartsWith("AUTH-", StringComparison.Ordinal) ||
            reference[5] is not (>= 'A' and <= 'Z') and not (>= '0' and <= '9') ||
            reference.Any(character =>
                character is not '-' and not (>= 'A' and <= 'Z') and not (>= '0' and <= '9')))
        {
            throw new ArgumentException(
                "A bounded non-secret AUTH-* operational authority reference is required.",
                nameof(reference));
        }

        return new ProductProviderOperationalAuthority(operation, reference);
    }

    internal void Revalidate(ProductProviderOperation expectedOperation)
    {
        if (Operation != expectedOperation)
        {
            throw new InvalidOperationException(
                "The product provider authority does not permit this operation.");
        }
    }
}

internal sealed class ProductProviderCredentialSource
{
    private readonly ProductProviderOperationalAuthority authority;
    private readonly ProductProviderOperation expectedOperation;
    private readonly string credentialEnvironmentVariable;
    private readonly Func<string, string?> credentialReader;

    internal ProductProviderCredentialSource(
        ProductProviderOperationalAuthority authority,
        ProductProviderOperation expectedOperation,
        string credentialEnvironmentVariable,
        Func<string, string?> credentialReader)
    {
        this.authority = authority ?? throw new ArgumentNullException(nameof(authority));
        this.expectedOperation = expectedOperation;
        this.credentialEnvironmentVariable =
            OpaqueEnvironmentCredentialReference.Parse(credentialEnvironmentVariable)
                .EnvironmentVariableName;
        this.credentialReader = credentialReader ??
            throw new ArgumentNullException(nameof(credentialReader));
    }

    internal ValueTask<string> ReadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        authority.Revalidate(expectedOperation);
        return ValueTask.FromResult(
            credentialReader(credentialEnvironmentVariable) ?? string.Empty);
    }
}
