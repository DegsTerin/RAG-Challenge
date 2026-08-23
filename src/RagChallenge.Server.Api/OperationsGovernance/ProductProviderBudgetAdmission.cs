// Purpose: Wires product provider operations and bounded readiness to the persistent ledger while keeping every unconfigured or unauthorised operation fail closed before credential or egress access.
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;

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

    internal static ProductProviderBudgetOperationalComposition CreateOperational(
        SqliteStoreOptions stores,
        ProductProviderOperationalAuthority authority,
        ProductProviderOperationalGrantSet trustedGrants,
        ProductProviderOperation operation,
        ProductProviderBudgetOptions options,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(stores);
        ArgumentNullException.ThrowIfNull(authority);
        ArgumentNullException.ThrowIfNull(trustedGrants);
        ArgumentNullException.ThrowIfNull(options);
        authority.Revalidate(operation);
        options.Validate(authority.Reference);

        var selectedTimeProvider = timeProvider ?? TimeProvider.System;
        var initialisationRequest = options.CreateInitialisationRequest();
        var context = new ProviderBudgetAdmissionContext(
            initialisationRequest.EnvelopeId,
            new ProviderRuntimeSessionId(options.RuntimeSessionId),
            new ProviderBudgetAuthorityReference(authority.Reference));
        var costSchedule = new OpenAiEmbeddingCostSchedule();
        var gate = new ProviderBudgetAdmissionGate(
            new SqliteProviderBudgetLedger(stores),
            context,
            cancellationToken =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                trustedGrants.Demand(authority, operation);
                return ValueTask.CompletedTask;
            },
            maximumChargeCalculator: costSchedule,
            timeProvider: selectedTimeProvider);
        var session = new ProductProviderBudgetOperationalSession(
            stores,
            authority,
            trustedGrants,
            operation,
            options,
            initialisationRequest,
            selectedTimeProvider);
        return new ProductProviderBudgetOperationalComposition(gate, session.PrepareAsync);
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

internal sealed record ProductProviderBudgetOperationalComposition(
    ProviderBudgetAdmissionGate AdmissionGate,
    Func<CancellationToken, Task> PrepareAsync);

internal sealed class ProductProviderBudgetOperationalSession
{
    private readonly Lock sync = new();
    private readonly SqliteStoreOptions stores;
    private readonly ProductProviderOperationalAuthority authority;
    private readonly ProductProviderOperationalGrantSet trustedGrants;
    private readonly ProductProviderOperation operation;
    private readonly ProductProviderBudgetOptions options;
    private readonly ProviderBudgetEnvelopeInitialisationRequest initialisationRequest;
    private readonly TimeProvider timeProvider;
    private Task? preparation;

    internal ProductProviderBudgetOperationalSession(
        SqliteStoreOptions stores,
        ProductProviderOperationalAuthority authority,
        ProductProviderOperationalGrantSet trustedGrants,
        ProductProviderOperation operation,
        ProductProviderBudgetOptions options,
        ProviderBudgetEnvelopeInitialisationRequest initialisationRequest,
        TimeProvider timeProvider)
    {
        this.stores = stores;
        this.authority = authority;
        this.trustedGrants = trustedGrants;
        this.operation = operation;
        this.options = options;
        this.initialisationRequest = initialisationRequest;
        this.timeProvider = timeProvider;
    }

    internal Task PrepareAsync(CancellationToken cancellationToken)
    {
        lock (sync)
        {
            return preparation ??= PrepareCoreAsync(cancellationToken);
        }
    }

    private async Task PrepareCoreAsync(CancellationToken cancellationToken)
    {
        trustedGrants.Demand(authority, operation);
        var ledger = new SqliteProviderBudgetLedger(stores);
        var envelope = await ledger.ReadEnvelopeAsync(
            initialisationRequest.EnvelopeId,
            cancellationToken).ConfigureAwait(false);

        if (envelope is null)
        {
            var initialised = await new SqliteProviderBudgetEnvelopeInitialiser(stores)
                .InitialiseAsync(initialisationRequest, cancellationToken)
                .ConfigureAwait(false);
            envelope = initialised.Envelope;
        }

        RequireExactEnvelope(envelope, requireArmed: false);

        if (envelope.State == ProviderBudgetState.Disarmed)
        {
            var occurredAtUtc = timeProvider.GetUtcNow();
            var configurationSha256 =
                SqliteProviderBudgetEnvelopeInitialiser.ComputeConfigurationSha256(
                    initialisationRequest);
            var rearm = await ledger.RearmAsync(
                new ProviderBudgetRearmRequest(
                    envelope.EnvelopeId,
                    envelope.StoreEpochId,
                    envelope.ConfigurationRevision,
                    envelope.LedgerRevision,
                    envelope.RearmRevision,
                    new ProviderRuntimeSessionId(options.RuntimeSessionId),
                    new ProviderBudgetAuthorityReference(options.RearmAuthorityReference),
                    new ProviderBudgetAuthorityReference(options.ActorReference),
                    Hash(
                        "provider-budget-product-initial-rearm-v1",
                        options.RearmAuthorityReference,
                        envelope.EnvelopeId.Value),
                    envelope.AggregateCommitted,
                    envelope.AggregateReserved,
                    envelope.AggregateIndeterminate,
                    SqliteProviderBudgetLedger.ComputeOperationBalancesSha256(envelope),
                    configurationSha256,
                    occurredAtUtc),
                cancellationToken).ConfigureAwait(false);

            if (rearm.Outcome != ProviderBudgetRearmOutcome.Applied ||
                rearm.Envelope is null)
            {
                throw new ProviderBudgetAdmissionUnavailableException();
            }

            envelope = rearm.Envelope;
        }

        RequireExactEnvelope(envelope, requireArmed: true);
    }

    private void RequireExactEnvelope(
        ProviderBudgetEnvelopeV1 envelope,
        bool requireArmed)
    {
        var expected = initialisationRequest;
        var exact = envelope.EnvelopeId == expected.EnvelopeId &&
            envelope.StoreEpochId == expected.StoreEpochId &&
            envelope.Scope == expected.Scope &&
            envelope.ConfigurationRevision.Value == 1 &&
            envelope.CostScheduleId == expected.CostScheduleId &&
            envelope.CostScheduleSha256 == expected.CostScheduleSha256 &&
            envelope.AggregateLimit == expected.AggregateLimit &&
            envelope.EffectiveAtUtc == expected.EffectiveAtUtc &&
            envelope.ExpiresAtUtc == expected.ExpiresAtUtc &&
            !envelope.IsClosed &&
            envelope.OperationBalances.SequenceEqual(expected.OperationBalances) &&
            envelope.AggregateCommitted.Value == 0 &&
            envelope.AggregateReserved.Value == 0 &&
            envelope.AggregateIndeterminate.Value == 0;

        if (requireArmed)
        {
            exact = exact && envelope.State == ProviderBudgetState.Armed &&
                envelope.RuntimeSessionId == new ProviderRuntimeSessionId(options.RuntimeSessionId) &&
                envelope.RearmRevision.Value == 1;
        }
        else
        {
            exact = exact && envelope.State is ProviderBudgetState.Disarmed or
                ProviderBudgetState.Armed;
        }

        if (!exact)
        {
            throw new ProviderBudgetAdmissionUnavailableException();
        }
    }

    private static ProviderBudgetSha256 Hash(string domain, params string[] values) =>
        new(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            string.Join('\n', new[] { domain }.Concat(values)))))
            .ToLowerInvariant());
}
