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
        var requiredPrefix = operation switch
        {
            ProductProviderOperation.AdministrativeIndexEmbedding =>
                "AUTH-ADMINISTRATIVE-INDEX-EMBEDDING-",
            ProductProviderOperation.QueryEmbedding => "AUTH-QUERY-EMBEDDING-",
            ProductProviderOperation.GroundedGeneration => "AUTH-GROUNDED-GENERATION-",
            _ => string.Empty,
        };
        if (!Enum.IsDefined(operation) ||
            string.IsNullOrWhiteSpace(reference) ||
            reference.Length > 128 ||
            !reference.StartsWith(requiredPrefix, StringComparison.Ordinal) ||
            reference.Length < requiredPrefix.Length + 3 ||
            reference[requiredPrefix.Length] is not (>= 'A' and <= 'Z') and
                not (>= '0' and <= '9') ||
            reference.Skip(requiredPrefix.Length).Any(character =>
                character is not '-' and
                not (>= 'A' and <= 'Z') and
                not (>= '0' and <= '9')))
        {
            throw new ArgumentException(
                "A bounded operation-specific non-secret AUTH-* reference is required.",
                nameof(reference));
        }

        return new ProductProviderOperationalAuthority(operation, reference);
    }

    internal void Revalidate(ProductProviderOperation expectedOperation)
    {
        if (Operation != expectedOperation)
        {
            throw new ProductProviderOperationalAuthorityException();
        }
    }
}

internal sealed class ProductProviderOperationalGrantSet
{
    private readonly HashSet<(ProductProviderOperation Operation, string Reference)> grants;

    internal ProductProviderOperationalGrantSet(
        IEnumerable<ProductProviderOperationalAuthority> trustedGrants)
    {
        ArgumentNullException.ThrowIfNull(trustedGrants);
        grants = trustedGrants
            .Select(grant => (grant.Operation, grant.Reference))
            .ToHashSet();
    }

    internal static ProductProviderOperationalGrantSet DenyAll() => new([]);

    internal static ProductProviderOperationalGrantSet FromExplicitConfiguration(
        params (ProductProviderOperation Operation, string? Reference)[] configuredGrants) =>
        new(configuredGrants
            .Where(configured => !string.IsNullOrWhiteSpace(configured.Reference))
            .Select(configured => ProductProviderOperationalAuthority.Parse(
                configured.Operation,
                configured.Reference)));

    internal void Demand(
        ProductProviderOperationalAuthority authority,
        ProductProviderOperation expectedOperation)
    {
        ArgumentNullException.ThrowIfNull(authority);
        authority.Revalidate(expectedOperation);
        if (!grants.Contains((expectedOperation, authority.Reference)))
        {
            throw new ProductProviderOperationalAuthorityException();
        }
    }
}

internal sealed class ProductProviderOperationalAuthorityException : InvalidOperationException
{
    internal const string StopCode = "HUMAN_DECISION_REQUIRED";

    internal ProductProviderOperationalAuthorityException()
        : base(
            "HUMAN_DECISION_REQUIRED: the exact product provider operation has no trusted operational grant.")
    {
    }
}

internal sealed class ProductProviderCredentialSource
{
    private readonly ProductProviderOperationalAuthority authority;
    private readonly ProductProviderOperationalGrantSet trustedGrants;
    private readonly ProductProviderOperation expectedOperation;
    private readonly string credentialEnvironmentVariable;
    private readonly Func<string, string?> credentialReader;

    internal ProductProviderCredentialSource(
        ProductProviderOperationalAuthority authority,
        ProductProviderOperationalGrantSet trustedGrants,
        ProductProviderOperation expectedOperation,
        string credentialEnvironmentVariable,
        Func<string, string?> credentialReader)
    {
        this.authority = authority ?? throw new ArgumentNullException(nameof(authority));
        this.trustedGrants = trustedGrants ??
            throw new ArgumentNullException(nameof(trustedGrants));
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
        trustedGrants.Demand(authority, expectedOperation);
        return ValueTask.FromResult(
            credentialReader(credentialEnvironmentVariable) ?? string.Empty);
    }
}
