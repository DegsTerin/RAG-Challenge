// Purpose: Serialises one-shot administrative mutations per corpus in a dedicated SQLite lease without reusing recovery ownership or exposing transport concerns.
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RagChallenge.Application.Administration;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Infrastructure.Persistence;

public sealed class SqliteAdministrationLeaseManager(SqliteStoreOptions options)
    : IAdministrationLeaseManager
{
    private static readonly TimeSpan MaximumDuration = TimeSpan.FromMinutes(10);

    private readonly SqliteStoreOptions options = options ??
        throw new ArgumentNullException(nameof(options));

    public async Task<AdministrationLeaseOutcome> AcquireAsync(
        AdministrationLeaseRequest request,
        CancellationToken cancellationToken = default)
    {
        Validate(request);
        await using var context = options.CreateControlContext();
        await using var transaction = await BeginImmediateAsync(
            context,
            cancellationToken).ConfigureAwait(false);
        var existing = await context.AdministrationLeases.SingleOrDefaultAsync(
            row => row.CorpusId == request.CorpusId.Value,
            cancellationToken).ConfigureAwait(false);

        if (existing is not null &&
            ControlPlaneMapping.ParseUtc(existing.ExpiresAtUtc) <= request.AcquiredAt)
        {
            context.AdministrationLeases.Remove(existing);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            existing = null;
        }

        if (existing is not null)
        {
            var outcome = string.Equals(
                existing.OperationId,
                request.OperationId.Value,
                StringComparison.Ordinal)
                ? AdministrationLeaseOutcome.AlreadyOwned
                : AdministrationLeaseOutcome.Conflict;
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return outcome;
        }

        context.AdministrationLeases.Add(new AdministrationLeaseRow
        {
            CorpusId = request.CorpusId.Value,
            OperationId = request.OperationId.Value,
            AcquiredAtUtc = ControlPlaneMapping.FormatUtc(request.AcquiredAt),
            ExpiresAtUtc = ControlPlaneMapping.FormatUtc(
                request.AcquiredAt + request.Duration),
        });
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return AdministrationLeaseOutcome.Acquired;
    }

    public async Task ReleaseAsync(
        CorpusId corpusId,
        OperationId operationId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(corpusId);
        ArgumentNullException.ThrowIfNull(operationId);
        await using var context = options.CreateControlContext();
        await using var transaction = await BeginImmediateAsync(
            context,
            cancellationToken).ConfigureAwait(false);
        var existing = await context.AdministrationLeases.SingleOrDefaultAsync(
            row => row.CorpusId == corpusId.Value,
            cancellationToken).ConfigureAwait(false);

        if (existing is null)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        if (!string.Equals(
                existing.OperationId,
                operationId.Value,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "An administrative lease can only be released by its exact owner.");
        }

        context.AdministrationLeases.Remove(existing);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void Validate(AdministrationLeaseRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.CorpusId);
        ArgumentNullException.ThrowIfNull(request.OperationId);

        if (request.AcquiredAt.Offset != TimeSpan.Zero ||
            request.Duration <= TimeSpan.Zero ||
            request.Duration > MaximumDuration)
        {
            throw new ArgumentException(
                "An administrative lease requires a bounded UTC interval.",
                nameof(request));
        }
    }

    private static async Task<SqliteTransaction> BeginImmediateAsync(
        ControlPlaneDbContext context,
        CancellationToken cancellationToken)
    {
        await context.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        var connection = (SqliteConnection)context.Database.GetDbConnection();
        var transaction = connection.BeginTransaction(deferred: false);
        context.Database.UseTransaction(transaction);
        return transaction;
    }
}
