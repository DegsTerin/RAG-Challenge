// Purpose: Wires product provider operations and bounded readiness to the persistent zero-budget ledger while keeping the unconfigured runtime fail-closed and free of credential or egress authority.
using Microsoft.Data.Sqlite;

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
        return new ProviderBudgetAdmissionGate(
            new SqliteProviderBudgetLedger(stores),
            CreateContext(authority, operation),
            cancellationToken =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                trustedGrants.Demand(authority, operation);
                throw new ProviderBudgetAdmissionUnavailableException();
            });
    }

    internal static async Task<ProviderBudgetState> ReadQueryReadinessAsync(
        SqliteStoreOptions stores,
        ProductProviderOperationalAuthority queryEmbeddingAuthority,
        ProductProviderOperationalAuthority groundedGenerationAuthority,
        ProductProviderOperationalGrantSet trustedGrants,
        DateTimeOffset observedAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stores);
        ArgumentNullException.ThrowIfNull(queryEmbeddingAuthority);
        ArgumentNullException.ThrowIfNull(groundedGenerationAuthority);
        ArgumentNullException.ThrowIfNull(trustedGrants);

        try
        {
            trustedGrants.Demand(
                queryEmbeddingAuthority,
                ProductProviderOperation.QueryEmbedding);
            trustedGrants.Demand(
                groundedGenerationAuthority,
                ProductProviderOperation.GroundedGeneration);

            var queryContext = CreateContext(
                queryEmbeddingAuthority,
                ProductProviderOperation.QueryEmbedding);
            var generationContext = CreateContext(
                groundedGenerationAuthority,
                ProductProviderOperation.GroundedGeneration);
            var ledger = new SqliteProviderBudgetLedger(stores);
            var queryEnvelope = await ledger.ReadEnvelopeAsync(
                queryContext.EnvelopeId,
                cancellationToken).ConfigureAwait(false);
            var generationEnvelope = await ledger.ReadEnvelopeAsync(
                generationContext.EnvelopeId,
                cancellationToken).ConfigureAwait(false);

            return CombineReadinessStates(
                SanitiseEnvelopeState(queryEnvelope, queryContext, observedAt),
                SanitiseEnvelopeState(generationEnvelope, generationContext, observedAt));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (IsUnreadableBudgetState(exception))
        {
            return ProviderBudgetState.Disarmed;
        }
    }

    internal static ProviderBudgetAdmissionContext CreateContext(
        ProductProviderOperationalAuthority authority,
        ProductProviderOperation operation)
    {
        ArgumentNullException.ThrowIfNull(authority);
        authority.Revalidate(operation);
        var operationToken = ToOperationClass(operation).ToString();
        return new ProviderBudgetAdmissionContext(
            new ProviderBudgetEnvelopeId($"PBE-UNCONFIGURED-{operationToken}"),
            new ProviderRuntimeSessionId($"PBS-UNCONFIGURED-{operationToken}"),
            new ProviderBudgetAuthorityReference(authority.Reference));
    }

    internal static ProviderBudgetState SanitiseEnvelopeState(
        ProviderBudgetEnvelopeV1? envelope,
        ProviderBudgetAdmissionContext context,
        DateTimeOffset observedAt)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (envelope is null ||
            !string.Equals(
                envelope.EnvelopeId.Value,
                context.EnvelopeId.Value,
                StringComparison.Ordinal) ||
            envelope.RuntimeSessionId is null ||
            !string.Equals(
                envelope.RuntimeSessionId.Value,
                context.RuntimeSessionId.Value,
                StringComparison.Ordinal) ||
            envelope.IsClosed ||
            envelope.AggregateLimit.Value != 0 ||
            envelope.OperationBalances.Any(balance => balance.AllocationLimit.Value != 0) ||
            observedAt < envelope.EffectiveAtUtc)
        {
            return ProviderBudgetState.Disarmed;
        }

        if (envelope.State == ProviderBudgetState.ReconciliationRequired)
        {
            return ProviderBudgetState.ReconciliationRequired;
        }

        if (observedAt >= envelope.ExpiresAtUtc)
        {
            return ProviderBudgetState.Expired;
        }

        return envelope.State == ProviderBudgetState.Armed
            ? ProviderBudgetState.Disarmed
            : Enum.IsDefined(envelope.State)
                ? envelope.State
                : ProviderBudgetState.Disarmed;
    }

    internal static ProviderBudgetState CombineReadinessStates(
        params ProviderBudgetState[] states)
    {
        ArgumentNullException.ThrowIfNull(states);
        if (states.Length == 0 || states.Any(state => !Enum.IsDefined(state)))
        {
            return ProviderBudgetState.Disarmed;
        }

        return states.MaxBy(GetReadinessPriority);
    }

    private static int GetReadinessPriority(ProviderBudgetState state) => state switch
    {
        ProviderBudgetState.Armed => 0,
        ProviderBudgetState.Disarmed => 1,
        ProviderBudgetState.Expired => 2,
        ProviderBudgetState.Exhausted => 3,
        ProviderBudgetState.Tripped => 4,
        ProviderBudgetState.ReconciliationRequired => 5,
        _ => -1,
    };

    private static ProviderBudgetOperationClass ToOperationClass(
        ProductProviderOperation operation) => operation switch
        {
            ProductProviderOperation.AdministrativeIndexEmbedding =>
                ProviderBudgetOperationClass.AdministrativeIndexEmbedding,
            ProductProviderOperation.QueryEmbedding => ProviderBudgetOperationClass.QueryEmbedding,
            ProductProviderOperation.GroundedGeneration =>
                ProviderBudgetOperationClass.GroundedGeneration,
            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        };

    private static bool IsUnreadableBudgetState(Exception exception) => exception is
        ProductProviderOperationalAuthorityException or
        SqliteException or
        InvalidDataException or
        ArgumentException or
        InvalidOperationException or
        IOException or
        UnauthorizedAccessException;
}
