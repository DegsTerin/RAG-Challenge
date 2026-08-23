// Purpose: Creates one exact disarmed provider-budget envelope transactionally so a separately explicit initial rearm can bind the authorised runtime session before egress.
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using RagChallenge.Application.ProviderBudget;

namespace RagChallenge.Infrastructure.Persistence;

public sealed record ProviderBudgetEnvelopeInitialisationRequest
{
    public ProviderBudgetEnvelopeInitialisationRequest(
        ProviderBudgetEnvelopeId envelopeId,
        ProviderBudgetStoreEpochId storeEpochId,
        ProviderBudgetScope scope,
        ProviderBudgetCostScheduleId costScheduleId,
        ProviderBudgetSha256 costScheduleSha256,
        ProviderBudgetUnits aggregateLimit,
        IReadOnlyCollection<ProviderBudgetOperationBalance> operationBalances,
        DateTimeOffset effectiveAtUtc,
        DateTimeOffset expiresAtUtc,
        ProviderBudgetAuthorityReference authorityReference,
        ProviderBudgetAuthorityReference actorReference,
        DateTimeOffset occurredAtUtc)
    {
        EnvelopeId = envelopeId ?? throw new ArgumentNullException(nameof(envelopeId));
        StoreEpochId = storeEpochId ?? throw new ArgumentNullException(nameof(storeEpochId));
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        CostScheduleId = costScheduleId ?? throw new ArgumentNullException(nameof(costScheduleId));
        CostScheduleSha256 = costScheduleSha256 ??
            throw new ArgumentNullException(nameof(costScheduleSha256));
        AuthorityReference = authorityReference ??
            throw new ArgumentNullException(nameof(authorityReference));
        ActorReference = actorReference ?? throw new ArgumentNullException(nameof(actorReference));
        ArgumentNullException.ThrowIfNull(operationBalances);

        var balances = operationBalances.OrderBy(value => value.OperationClass).ToArray();
        if (effectiveAtUtc.Offset != TimeSpan.Zero || expiresAtUtc.Offset != TimeSpan.Zero ||
            occurredAtUtc.Offset != TimeSpan.Zero || effectiveAtUtc > occurredAtUtc ||
            occurredAtUtc >= expiresAtUtc ||
            balances.Any(balance =>
                balance.Committed.Value != 0 || balance.Reserved.Value != 0 ||
                balance.Indeterminate.Value != 0))
        {
            throw new ArgumentException(
                "A provider-budget envelope must start unused within one explicit UTC authority window.");
        }

        _ = new ProviderBudgetEnvelopeV1(
            envelopeId,
            storeEpochId,
            scope,
            new ProviderBudgetConfigurationRevision(1),
            new ProviderBudgetLedgerRevision(1),
            new ProviderBudgetRearmRevision(0),
            ProviderBudgetState.Disarmed,
            runtimeSessionId: null,
            costScheduleId,
            costScheduleSha256,
            aggregateLimit,
            new ProviderBudgetUnits(0),
            new ProviderBudgetUnits(0),
            new ProviderBudgetUnits(0),
            balances,
            effectiveAtUtc,
            expiresAtUtc,
            isClosed: false,
            new ProviderBudgetSha256(new string('0', 64)));

        AggregateLimit = aggregateLimit;
        OperationBalances = Array.AsReadOnly(balances);
        EffectiveAtUtc = effectiveAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        OccurredAtUtc = occurredAtUtc;
    }

    public ProviderBudgetEnvelopeId EnvelopeId { get; }
    public ProviderBudgetStoreEpochId StoreEpochId { get; }
    public ProviderBudgetScope Scope { get; }
    public ProviderBudgetCostScheduleId CostScheduleId { get; }
    public ProviderBudgetSha256 CostScheduleSha256 { get; }
    public ProviderBudgetUnits AggregateLimit { get; }
    public IReadOnlyCollection<ProviderBudgetOperationBalance> OperationBalances { get; }
    public DateTimeOffset EffectiveAtUtc { get; }
    public DateTimeOffset ExpiresAtUtc { get; }
    public ProviderBudgetAuthorityReference AuthorityReference { get; }
    public ProviderBudgetAuthorityReference ActorReference { get; }
    public DateTimeOffset OccurredAtUtc { get; }
}

public sealed record ProviderBudgetEnvelopeInitialisationResult(
    ProviderBudgetEnvelopeV1 Envelope,
    ProviderBudgetSha256 ConfigurationSha256);

public sealed class SqliteProviderBudgetEnvelopeInitialiser(SqliteStoreOptions options)
{
    private const string ControlId = "provider-budget-control-v1";
    private const string ZeroSha256 =
        "0000000000000000000000000000000000000000000000000000000000000000";

    private readonly SqliteStoreOptions options = options ??
        throw new ArgumentNullException(nameof(options));

    public async Task<ProviderBudgetEnvelopeInitialisationResult> InitialiseAsync(
        ProviderBudgetEnvelopeInitialisationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var configurationSha256 = ComputeConfigurationSha256(request);
        var epochSha256 = Digest(
            "provider-budget-store-epoch-v1",
            request.StoreEpochId.Value,
            request.AuthorityReference.Value,
            request.OccurredAtUtc);
        var ledgerSha256 = Digest(
            "provider-budget-ledger-v1",
            request.EnvelopeId.Value,
            1,
            request.StoreEpochId.Value,
            1,
            0,
            ProviderBudgetState.Disarmed,
            request.AggregateLimit.Value,
            request.OccurredAtUtc,
            configurationSha256.Value);
        var occurredAt = FormatUtc(request.OccurredAtUtc);

        await using var context = options.CreateControlContext();
        await context.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        var connection = (SqliteConnection)context.Database.GetDbConnection();
        await using var transaction = connection.BeginTransaction(deferred: false);
        context.Database.UseTransaction(transaction);

        if (await context.ProviderBudgetControlHeads.AsNoTracking().AnyAsync(cancellationToken)
                .ConfigureAwait(false) ||
            await context.ProviderBudgetEnvelopes.AsNoTracking().AnyAsync(cancellationToken)
                .ConfigureAwait(false) ||
            await context.ProviderBudgetStoreEpochs.AsNoTracking().AnyAsync(cancellationToken)
                .ConfigureAwait(false))
        {
            throw new InvalidOperationException(
                "Provider-budget initialisation requires an empty durable control boundary.");
        }

        context.ProviderBudgetStoreEpochs.Add(new ProviderBudgetStoreEpochRow
        {
            StoreEpochId = request.StoreEpochId.Value,
            EpochRevision = 1,
            PreviousStoreEpochId = null,
            EpochKind = "Initial",
            RestoreCheckpointSha256 = null,
            AuthorityReference = request.AuthorityReference.Value,
            OccurredAtUtc = occurredAt,
            PreviousEpochSha256 = ZeroSha256,
            EpochSha256 = epochSha256,
        });
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        context.ProviderBudgetControlHeads.Add(new ProviderBudgetControlHeadRow
        {
            ControlId = ControlId,
            CurrentStoreEpochId = request.StoreEpochId.Value,
            EpochRevision = 1,
            RowRevision = 1,
        });
        context.ProviderBudgetEnvelopes.Add(new ProviderBudgetEnvelopeRow
        {
            EnvelopeId = request.EnvelopeId.Value,
            SchemaVersion = ProviderBudgetEnvelopeV1.SchemaVersion,
            CurrentStoreEpochId = request.StoreEpochId.Value,
            EnvironmentId = request.Scope.EnvironmentId.Value,
            ProviderId = request.Scope.ProviderId.Value,
            BillingScopeReference = request.Scope.BillingScopeReference.Value,
            ModelId = request.Scope.ModelId.Value,
            CurrencyCode = request.Scope.CurrencyCode.Value,
            AccountingUnitId = request.Scope.AccountingUnitId.Value,
            CurrentConfigurationRevision = 1,
            CurrentLedgerRevision = 1,
            CurrentRearmRevision = 0,
            State = ProviderBudgetState.Disarmed.ToString(),
            RuntimeSessionId = null,
            AggregateLimitUnits = 0,
            AggregateCommittedUnits = 0,
            AggregateReservedUnits = 0,
            AggregateIndeterminateUnits = 0,
            IsInitialised = 0,
            IsClosed = 0,
            CreatedAtUtc = occurredAt,
            CreationAuthorityReference = request.AuthorityReference.Value,
            ClosedAtUtc = null,
            ClosureAuthorityReference = null,
            CurrentLedgerSha256 = ledgerSha256,
        });
        context.ProviderBudgetConfigurations.Add(new ProviderBudgetConfigurationRow
        {
            EnvelopeId = request.EnvelopeId.Value,
            ConfigurationRevision = 1,
            PreviousConfigurationRevision = null,
            CostScheduleId = request.CostScheduleId.Value,
            CostScheduleSha256 = request.CostScheduleSha256.Value,
            AggregateLimitUnits = request.AggregateLimit.Value,
            EffectiveAtUtc = FormatUtc(request.EffectiveAtUtc),
            ExpiresAtUtc = FormatUtc(request.ExpiresAtUtc),
            ConfigurationAuthorityReference = request.AuthorityReference.Value,
            CreatedAtUtc = occurredAt,
            SealedAtUtc = null,
            ConfigurationSha256 = configurationSha256.Value,
        });
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        foreach (var balance in request.OperationBalances)
        {
            context.ProviderBudgetOperationAllocations.Add(
                new ProviderBudgetOperationAllocationRow
                {
                    EnvelopeId = request.EnvelopeId.Value,
                    ConfigurationRevision = 1,
                    OperationClass = balance.OperationClass.ToString(),
                    AllocationLimitUnits = balance.AllocationLimit.Value,
                });
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var configuration = await context.ProviderBudgetConfigurations.SingleAsync(
            value => value.EnvelopeId == request.EnvelopeId.Value &&
                value.ConfigurationRevision == 1,
            cancellationToken).ConfigureAwait(false);
        configuration.SealedAtUtc = occurredAt;
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var ledger = new ProviderBudgetLedgerRevisionRow
        {
            EnvelopeId = request.EnvelopeId.Value,
            LedgerRevision = 1,
            StoreEpochId = request.StoreEpochId.Value,
            PreviousLedgerRevision = null,
            ConfigurationRevision = 1,
            RearmRevision = 0,
            State = ProviderBudgetState.Disarmed.ToString(),
            RuntimeSessionId = null,
            AggregateLimitUnits = request.AggregateLimit.Value,
            AggregateCommittedUnits = 0,
            AggregateReservedUnits = 0,
            AggregateIndeterminateUnits = 0,
            TransitionKind = "EnvelopeCreated",
            ProviderRequestId = null,
            TransitionAuthorityReference = request.AuthorityReference.Value,
            OccurredAtUtc = occurredAt,
            PreviousLedgerSha256 = ZeroSha256,
            LedgerSha256 = ledgerSha256,
            IsComplete = 0,
        };
        context.ProviderBudgetLedgerRevisions.Add(ledger);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        foreach (var balance in request.OperationBalances)
        {
            context.ProviderBudgetOperationBalanceRevisions.Add(
                new ProviderBudgetOperationBalanceRevisionRow
                {
                    EnvelopeId = request.EnvelopeId.Value,
                    LedgerRevision = 1,
                    OperationClass = balance.OperationClass.ToString(),
                    ConfigurationRevision = 1,
                    AllocationLimitUnits = balance.AllocationLimit.Value,
                    CommittedUnits = 0,
                    ReservedUnits = 0,
                    IndeterminateUnits = 0,
                });
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        ledger.IsComplete = 1;
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var auditDigest = Digest(
            "provider-budget-envelope-created-audit-v1",
            request.EnvelopeId.Value,
            request.StoreEpochId.Value,
            configurationSha256.Value,
            ledgerSha256);
        context.ProviderBudgetAuditEvents.Add(new ProviderBudgetAuditEventRow
        {
            AuditEventId = $"PBA-{auditDigest}",
            EnvelopeId = request.EnvelopeId.Value,
            LedgerRevision = 1,
            ProviderRequestId = null,
            OperationClass = null,
            EventType = "EnvelopeCreated",
            AuthorityReference = request.AuthorityReference.Value,
            ActorReference = request.ActorReference.Value,
            RequestSha256 = null,
            MaximumChargeUnits = null,
            FromState = null,
            ToState = ProviderBudgetState.Disarmed.ToString(),
            OutcomeCode = "Created",
            OccurredAtUtc = occurredAt,
            DetailsSha256 = auditDigest,
        });
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var envelopeRow = await context.ProviderBudgetEnvelopes.SingleAsync(
            value => value.EnvelopeId == request.EnvelopeId.Value,
            cancellationToken).ConfigureAwait(false);
        envelopeRow.AggregateLimitUnits = request.AggregateLimit.Value;
        envelopeRow.IsInitialised = 1;
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        var readback = await new SqliteProviderBudgetLedger(options).ReadEnvelopeAsync(
            request.EnvelopeId,
            cancellationToken).ConfigureAwait(false) ??
            throw new InvalidDataException(
                "The provider-budget envelope was not readable after initialisation.");
        return new ProviderBudgetEnvelopeInitialisationResult(readback, configurationSha256);
    }

    public static ProviderBudgetSha256 ComputeConfigurationSha256(
        ProviderBudgetEnvelopeInitialisationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new ProviderBudgetSha256(Digest(
            "provider-budget-configuration-v1",
            request.EnvelopeId.Value,
            request.StoreEpochId.Value,
            request.Scope.EnvironmentId.Value,
            request.Scope.ProviderId.Value,
            request.Scope.BillingScopeReference.Value,
            request.Scope.ModelId.Value,
            request.Scope.CurrencyCode.Value,
            request.Scope.AccountingUnitId.Value,
            request.CostScheduleId.Value,
            request.CostScheduleSha256.Value,
            request.AggregateLimit.Value,
            request.OperationBalances.Select(value =>
                $"{value.OperationClass}:{value.AllocationLimit.Value}").ToArray(),
            request.EffectiveAtUtc,
            request.ExpiresAtUtc,
            request.AuthorityReference.Value));
    }

    private static string FormatUtc(DateTimeOffset value) =>
        value.ToString("O", CultureInfo.InvariantCulture);

    private static string Digest(string domain, params object[] values)
    {
        var canonical = string.Join(
            '\n',
            new[] { domain }.Concat(values.Select(value => value switch
            {
                DateTimeOffset instant => FormatUtc(instant),
                string[] array => string.Join('|', array),
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString(),
            })));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)))
            .ToLowerInvariant();
    }
}
