// Purpose: Wires product provider operations to the persistent zero-budget ledger while keeping the unconfigured runtime fail-closed and free of credential or egress authority.
using RagChallenge.Application.ProviderBudget;
using RagChallenge.Infrastructure.Persistence;
using RagChallenge.Infrastructure.Providers;

namespace RagChallenge.Server.Api.OperationsGovernance;

internal static class ProductProviderBudgetAdmission
{
    internal static ProviderBudgetAdmissionGate CreateFailClosed(
        SqliteStoreOptions stores,
        ProductProviderOperationalAuthority authority,
        ProductProviderOperationalGrantSet trustedGrants,
        ProductProviderOperation operation)
    {
        ArgumentNullException.ThrowIfNull(stores);
        ArgumentNullException.ThrowIfNull(authority);
        ArgumentNullException.ThrowIfNull(trustedGrants);
        var operationClass = operation switch
        {
            ProductProviderOperation.AdministrativeIndexEmbedding =>
                ProviderBudgetOperationClass.AdministrativeIndexEmbedding,
            ProductProviderOperation.QueryEmbedding => ProviderBudgetOperationClass.QueryEmbedding,
            ProductProviderOperation.GroundedGeneration =>
                ProviderBudgetOperationClass.GroundedGeneration,
            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        };
        var operationToken = operationClass.ToString();
        return new ProviderBudgetAdmissionGate(
            new SqliteProviderBudgetLedger(stores),
            new ProviderBudgetAdmissionContext(
                new ProviderBudgetEnvelopeId($"PBE-UNCONFIGURED-{operationToken}"),
                new ProviderRuntimeSessionId($"PBS-UNCONFIGURED-{operationToken}"),
                new ProviderBudgetAuthorityReference(authority.Reference)),
            cancellationToken =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                trustedGrants.Demand(authority, operation);
                return ValueTask.CompletedTask;
            });
    }
}
