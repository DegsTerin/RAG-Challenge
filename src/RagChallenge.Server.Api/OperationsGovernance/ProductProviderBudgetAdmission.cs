// Purpose: Wires product provider operations and bounded readiness to the persistent ledger while keeping every unconfigured or unauthorised operation fail closed before credential or egress access.
using System.Security.Cryptography;
using System.Text;

using Microsoft.Data.Sqlite;

using RagChallenge.Application.ProviderBudget;
using RagChallenge.Infrastructure.Persistence;
using RagChallenge.Infrastructure.Providers;

namespace RagChallenge.Server.Api.OperationsGovernance;

internal static class ProductProviderBudgetAdmission
{
    internal const long DemoAggregateLimitMicroUsd = 500_000;
    internal const long DemoQueryEmbeddingLimitMicroUsd = 10_000;
    internal const long DemoGroundedGenerationLimitMicroUsd = 490_000;

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

    internal static ProductProviderBudgetOperationalComposition CreateRuntimeOperational(
        SqliteStoreOptions stores,
        ProductProviderOperationalAuthority operationAuthority,
        ProductProviderOperationalAuthority budgetAuthority,
        ProductProviderOperationalGrantSet trustedGrants,
        ProductProviderOperation operation,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(stores);
        ArgumentNullException.ThrowIfNull(operationAuthority);
        ArgumentNullException.ThrowIfNull(budgetAuthority);
        ArgumentNullException.ThrowIfNull(trustedGrants);
        operationAuthority.Revalidate(operation);
        budgetAuthority.Revalidate(ProductProviderOperation.QueryEmbedding);

        var selectedTimeProvider = timeProvider ?? TimeProvider.System;
        var configuration = CreateRuntimeConfiguration(budgetAuthority);
        var context = CreateRuntimeContext(configuration, operationAuthority, operation);
        var gate = new ProviderBudgetAdmissionGate(
            new SqliteProviderBudgetLedger(stores),
            context,
            cancellationToken =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                trustedGrants.Demand(operationAuthority, operation);
                return ValueTask.CompletedTask;
            },
            maximumChargeCalculator: new OpenAiDemoCostSchedule(),
            timeProvider: selectedTimeProvider);
        var session = new ProductProviderBudgetOperationalSession(
            stores,
            operationAuthority,
            trustedGrants,
            operation,
            configuration,
            selectedTimeProvider);
        return new ProductProviderBudgetOperationalComposition(gate, session.PrepareAsync);
    }

    internal static SqliteStoreOptions CreateRuntimeBudgetStores(SqliteStoreOptions stores)
    {
        ArgumentNullException.ThrowIfNull(stores);
        var storeRoot = Path.GetDirectoryName(stores.ControlDatabasePath) ??
            throw new InvalidOperationException("The product store root is unavailable.");
        var budgetRoot = Path.Combine(storeRoot, ".provider-budget-runtime-v1");
        return new SqliteStoreOptions(
            Path.Combine(budgetRoot, "control.db"),
            Path.Combine(budgetRoot, "vectors.db"),
            Path.Combine(budgetRoot, "content"));
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

            var configuration = CreateRuntimeConfiguration(queryEmbeddingAuthority);
            var queryContext = CreateRuntimeContext(
                configuration,
                queryEmbeddingAuthority,
                ProductProviderOperation.QueryEmbedding);
            var generationContext = CreateRuntimeContext(
                configuration,
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
                SanitiseRuntimeEnvelopeState(
                    queryEnvelope,
                    queryContext,
                    configuration.InitialisationRequest,
                    observedAt),
                SanitiseRuntimeEnvelopeState(
                    generationEnvelope,
                    generationContext,
                    configuration.InitialisationRequest,
                    observedAt));
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

    private static ProductProviderRuntimeBudgetConfiguration CreateRuntimeConfiguration(
        ProductProviderOperationalAuthority budgetAuthority)
    {
        budgetAuthority.Revalidate(ProductProviderOperation.QueryEmbedding);
        const string runtimeSessionId = "PBS-RENDER-FREE-DEMO-20260823";
        var effectiveAtUtc = DateTimeOffset.Parse(
            "2026-08-23T00:00:00Z",
            System.Globalization.CultureInfo.InvariantCulture);
        var expiresAtUtc = DateTimeOffset.Parse(
            "2027-08-31T00:00:00Z",
            System.Globalization.CultureInfo.InvariantCulture);
        var request = new ProviderBudgetEnvelopeInitialisationRequest(
            new ProviderBudgetEnvelopeId("PBE-RENDER-FREE-DEMO-20260823"),
            new ProviderBudgetStoreEpochId("PSE-RENDER-FREE-DEMO-20260823"),
            new ProviderBudgetScope(
                new ProviderBudgetEnvironmentId("ENV-RENDER-FREE-DEMO"),
                new ProviderBudgetProviderId(OpenAiEmbeddingCostSchedule.ProviderId),
                new ProviderBudgetBillingScopeReference("BILLING-OPENAI-CREDITS-DEMO"),
                new ProviderBudgetModelId(OpenAiDemoCostSchedule.ScopeModelId),
                new ProviderBudgetCurrencyCode(OpenAiEmbeddingCostSchedule.CurrencyCode),
                new ProviderBudgetAccountingUnitId(
                    OpenAiEmbeddingCostSchedule.AccountingUnitId)),
            new ProviderBudgetCostScheduleId(OpenAiDemoCostSchedule.ScheduleId),
            new ProviderBudgetSha256(OpenAiDemoCostSchedule.ScheduleSha256),
            new ProviderBudgetUnits(DemoAggregateLimitMicroUsd),
            [
                new ProviderBudgetOperationBalance(
                    ProviderBudgetOperationClass.AdministrativeIndexEmbedding,
                    new ProviderBudgetUnits(0),
                    new ProviderBudgetUnits(0),
                    new ProviderBudgetUnits(0),
                    new ProviderBudgetUnits(0)),
                new ProviderBudgetOperationBalance(
                    ProviderBudgetOperationClass.QueryEmbedding,
                    new ProviderBudgetUnits(DemoQueryEmbeddingLimitMicroUsd),
                    new ProviderBudgetUnits(0),
                    new ProviderBudgetUnits(0),
                    new ProviderBudgetUnits(0)),
                new ProviderBudgetOperationBalance(
                    ProviderBudgetOperationClass.GroundedGeneration,
                    new ProviderBudgetUnits(DemoGroundedGenerationLimitMicroUsd),
                    new ProviderBudgetUnits(0),
                    new ProviderBudgetUnits(0),
                    new ProviderBudgetUnits(0)),
            ],
            effectiveAtUtc,
            expiresAtUtc,
            new ProviderBudgetAuthorityReference(budgetAuthority.Reference),
            new ProviderBudgetAuthorityReference(budgetAuthority.Reference),
            effectiveAtUtc);
        return new ProductProviderRuntimeBudgetConfiguration(
            request,
            runtimeSessionId,
            budgetAuthority.Reference,
            budgetAuthority.Reference);
    }

    private static ProviderBudgetAdmissionContext CreateRuntimeContext(
        ProductProviderRuntimeBudgetConfiguration configuration,
        ProductProviderOperationalAuthority authority,
        ProductProviderOperation operation)
    {
        authority.Revalidate(operation);
        return new ProviderBudgetAdmissionContext(
            configuration.InitialisationRequest.EnvelopeId,
            new ProviderRuntimeSessionId(configuration.RuntimeSessionId),
            new ProviderBudgetAuthorityReference(authority.Reference));
    }

    private static ProviderBudgetState SanitiseRuntimeEnvelopeState(
        ProviderBudgetEnvelopeV1? envelope,
        ProviderBudgetAdmissionContext context,
        ProviderBudgetEnvelopeInitialisationRequest expected,
        DateTimeOffset observedAt)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(expected);
        if (envelope is null ||
            envelope.EnvelopeId != expected.EnvelopeId ||
            envelope.StoreEpochId != expected.StoreEpochId ||
            envelope.Scope != expected.Scope ||
            envelope.ConfigurationRevision.Value != 1 ||
            envelope.CostScheduleId != expected.CostScheduleId ||
            envelope.CostScheduleSha256 != expected.CostScheduleSha256 ||
            envelope.AggregateLimit != expected.AggregateLimit ||
            envelope.EffectiveAtUtc != expected.EffectiveAtUtc ||
            envelope.ExpiresAtUtc != expected.ExpiresAtUtc ||
            envelope.RuntimeSessionId != context.RuntimeSessionId ||
            envelope.IsClosed ||
            !HasExactAllocations(envelope, expected) ||
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

        return Enum.IsDefined(envelope.State)
            ? envelope.State
            : ProviderBudgetState.Disarmed;
    }

    internal static bool HasExactAllocations(
        ProviderBudgetEnvelopeV1 envelope,
        ProviderBudgetEnvelopeInitialisationRequest expected) =>
        envelope.OperationBalances
            .Select(balance => (balance.OperationClass, balance.AllocationLimit.Value))
            .SequenceEqual(expected.OperationBalances
                .Select(balance => (balance.OperationClass, balance.AllocationLimit.Value)));

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

internal sealed record ProductProviderRuntimeBudgetConfiguration(
    ProviderBudgetEnvelopeInitialisationRequest InitialisationRequest,
    string RuntimeSessionId,
    string RearmAuthorityReference,
    string ActorReference);

internal sealed class ProductProviderBudgetOperationalSession
{
    private readonly Lock sync = new();
    private readonly SqliteStoreOptions stores;
    private readonly ProductProviderOperationalAuthority authority;
    private readonly ProductProviderOperationalGrantSet trustedGrants;
    private readonly ProductProviderOperation operation;
    private readonly string runtimeSessionId;
    private readonly string rearmAuthorityReference;
    private readonly string actorReference;
    private readonly bool requireUnusedAtPreparation;
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
        runtimeSessionId = options.RuntimeSessionId;
        rearmAuthorityReference = options.RearmAuthorityReference;
        actorReference = options.ActorReference;
        requireUnusedAtPreparation = true;
        this.initialisationRequest = initialisationRequest;
        this.timeProvider = timeProvider;
    }

    internal ProductProviderBudgetOperationalSession(
        SqliteStoreOptions stores,
        ProductProviderOperationalAuthority authority,
        ProductProviderOperationalGrantSet trustedGrants,
        ProductProviderOperation operation,
        ProductProviderRuntimeBudgetConfiguration configuration,
        TimeProvider timeProvider)
    {
        this.stores = stores;
        this.authority = authority;
        this.trustedGrants = trustedGrants;
        this.operation = operation;
        runtimeSessionId = configuration.RuntimeSessionId;
        rearmAuthorityReference = configuration.RearmAuthorityReference;
        actorReference = configuration.ActorReference;
        requireUnusedAtPreparation = false;
        initialisationRequest = configuration.InitialisationRequest;
        this.timeProvider = timeProvider;
    }

    internal Task PrepareAsync(CancellationToken cancellationToken)
    {
        Task currentPreparation;
        lock (sync)
        {
            preparation ??= PrepareCoreAsync(CancellationToken.None);
            currentPreparation = preparation;
        }

        return currentPreparation.WaitAsync(cancellationToken);
    }

    private async Task PrepareCoreAsync(CancellationToken cancellationToken)
    {
        trustedGrants.Demand(authority, operation);
        Directory.CreateDirectory(
            Path.GetDirectoryName(stores.ControlDatabasePath) ??
                throw new ProviderBudgetAdmissionUnavailableException());
        await SqliteStoreProvisioner.ApplyMigrationsAsync(stores, cancellationToken)
            .ConfigureAwait(false);
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
                    new ProviderRuntimeSessionId(runtimeSessionId),
                    new ProviderBudgetAuthorityReference(rearmAuthorityReference),
                    new ProviderBudgetAuthorityReference(actorReference),
                    Hash(
                        "provider-budget-product-initial-rearm-v1",
                        rearmAuthorityReference,
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
            ProductProviderBudgetAdmission.HasExactAllocations(envelope, expected);

        if (requireUnusedAtPreparation)
        {
            exact = exact && envelope.OperationBalances.SequenceEqual(expected.OperationBalances) &&
                envelope.AggregateCommitted.Value == 0 &&
                envelope.AggregateReserved.Value == 0 &&
                envelope.AggregateIndeterminate.Value == 0;
        }

        if (requireArmed)
        {
            exact = exact && envelope.State == ProviderBudgetState.Armed &&
                envelope.RuntimeSessionId == new ProviderRuntimeSessionId(runtimeSessionId) &&
                envelope.RearmRevision.Value >= 1;
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
