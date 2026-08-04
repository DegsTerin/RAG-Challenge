// Purpose: Persists the bounded one-shot administration intent and result journal; successful state mutations complete their journal row inside the same SQLite transaction.
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using RagChallenge.Application.Administration;

namespace RagChallenge.Infrastructure.Persistence;

public sealed class SqliteAdministrationCommandJournal(SqliteStoreOptions options)
    : IAdministrationCommandJournal
{
    private readonly SqliteStoreOptions options = options ??
        throw new ArgumentNullException(nameof(options));

    public async Task<AdministrationJournalBeginResult> BeginAsync(
        AdministrationJournalIntent intent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(intent);
        await using var context = options.CreateControlContext();
        await using var transaction = await BeginImmediateAsync(
            context,
            cancellationToken).ConfigureAwait(false);
        var existing = await context.AdministrationCommandJournal.SingleOrDefaultAsync(
            row => row.OperationId == intent.OperationId.Value,
            cancellationToken).ConfigureAwait(false);

        if (existing is not null)
        {
            EnsureExactIntent(existing, intent);
            var result = existing.Status switch
            {
                "Started" => new AdministrationJournalBeginResult(
                    AdministrationJournalBeginOutcome.Resumed,
                    existing.IntentDigest,
                    ControlPlaneMapping.ParseUtc(existing.StartedAtUtc)),
                "Completed" => new AdministrationJournalBeginResult(
                    AdministrationJournalBeginOutcome.CompletedReplay,
                    existing.IntentDigest,
                    ControlPlaneMapping.ParseUtc(existing.StartedAtUtc),
                    ToResult(existing)),
                _ => throw new InvalidDataException(
                    "The administrative journal contains an unknown status."),
            };
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return result;
        }

        context.AdministrationCommandJournal.Add(new AdministrationCommandJournalRow
        {
            OperationId = intent.OperationId.Value,
            CorpusId = intent.CorpusId.Value,
            Command = intent.Command,
            ActorIdentifier = intent.ActorIdentifier,
            ReasonSha256 = intent.ReasonSha256,
            InputSha256 = intent.InputSha256,
            SourceIdsJson = intent.SourceIdentifiersJson,
            TargetIdsJson = intent.TargetIdentifiersJson,
            StartedAtUtc = ControlPlaneMapping.FormatUtc(intent.StartedAt),
            CompletedAtUtc = null,
            Status = "Started",
            Outcome = null,
            ResultCode = null,
            ExitCategory = null,
            ResultRevision = null,
            IntentDigest = intent.IntentDigest,
        });
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new AdministrationJournalBeginResult(
            AdministrationJournalBeginOutcome.Started,
            intent.IntentDigest,
            intent.StartedAt);
    }

    public async Task CompleteAsync(
        AdministrationJournalCompletion completion,
        DateTimeOffset completedAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(completion);
        ControlPlaneMapping.EnsureUtc(completedAt, nameof(completedAt));
        await using var context = options.CreateControlContext();
        await using var transaction = await BeginImmediateAsync(
            context,
            cancellationToken).ConfigureAwait(false);
        await ApplyCompletionAsync(
            context,
            completion,
            completedAt,
            cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task VerifyCompletedAsync(
        AdministrationJournalCompletion completion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(completion);
        await using var context = options.CreateControlContext();
        await VerifyCompletionAsync(context, completion, cancellationToken)
            .ConfigureAwait(false);
    }

    internal static async Task ApplyCompletionAsync(
        ControlPlaneDbContext context,
        AdministrationJournalCompletion? completion,
        DateTimeOffset completedAt,
        CancellationToken cancellationToken)
    {
        if (completion is null)
        {
            return;
        }

        ControlPlaneMapping.EnsureUtc(completedAt, nameof(completedAt));
        var row = await context.AdministrationCommandJournal.SingleOrDefaultAsync(
            item => item.OperationId == completion.OperationId.Value,
            cancellationToken).ConfigureAwait(false) ??
            throw new InvalidOperationException(
                "A successful administrative mutation requires its started journal row.");

        if (row.Status == "Completed")
        {
            EnsureExactCompletion(row, completion);
            return;
        }

        if (row.Status != "Started" ||
            !string.Equals(row.IntentDigest, completion.IntentDigest, StringComparison.Ordinal))
        {
            throw new AdministrationJournalConflictException(
                "The administrative operation identity was reused with different intent.");
        }

        var startedAt = ControlPlaneMapping.ParseUtc(row.StartedAtUtc);

        if (completedAt < startedAt)
        {
            throw new InvalidOperationException(
                "The administrative completion cannot predate its start.");
        }

        row.CompletedAtUtc = ControlPlaneMapping.FormatUtc(completedAt);
        row.Status = "Completed";
        row.Outcome = completion.Outcome.ToString();
        row.ResultCode = completion.ResultCode;
        row.ExitCategory = completion.ExitCategory;
        row.ResultRevision = completion.ResultRevision;
    }

    internal static async Task VerifyCompletionAsync(
        ControlPlaneDbContext context,
        AdministrationJournalCompletion? completion,
        CancellationToken cancellationToken)
    {
        if (completion is null)
        {
            return;
        }

        var row = await context.AdministrationCommandJournal.AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.OperationId == completion.OperationId.Value,
                cancellationToken).ConfigureAwait(false) ??
            throw new InvalidOperationException(
                "The administrative journal completion is missing.");
        EnsureExactCompletion(row, completion);
    }

    private static void EnsureExactIntent(
        AdministrationCommandJournalRow row,
        AdministrationJournalIntent intent)
    {
        if (!string.Equals(row.CorpusId, intent.CorpusId.Value, StringComparison.Ordinal) ||
            !string.Equals(row.Command, intent.Command, StringComparison.Ordinal) ||
            !string.Equals(row.ActorIdentifier, intent.ActorIdentifier, StringComparison.Ordinal) ||
            !string.Equals(row.ReasonSha256, intent.ReasonSha256, StringComparison.Ordinal) ||
            !string.Equals(row.InputSha256, intent.InputSha256, StringComparison.Ordinal) ||
            !string.Equals(
                row.SourceIdsJson,
                intent.SourceIdentifiersJson,
                StringComparison.Ordinal) ||
            !string.Equals(
                row.TargetIdsJson,
                intent.TargetIdentifiersJson,
                StringComparison.Ordinal) ||
            !string.Equals(row.IntentDigest, intent.IntentDigest, StringComparison.Ordinal))
        {
            throw new AdministrationJournalConflictException(
                "The administrative operation identity was reused with different intent.");
        }
    }

    private static void EnsureExactCompletion(
        AdministrationCommandJournalRow row,
        AdministrationJournalCompletion completion)
    {
        if (row.Status != "Completed" ||
            row.CompletedAtUtc is null ||
            !string.Equals(row.IntentDigest, completion.IntentDigest, StringComparison.Ordinal) ||
            !string.Equals(row.Outcome, completion.Outcome.ToString(), StringComparison.Ordinal) ||
            !string.Equals(row.ResultCode, completion.ResultCode, StringComparison.Ordinal) ||
            row.ExitCategory != completion.ExitCategory ||
            row.ResultRevision != completion.ResultRevision)
        {
            throw new AdministrationJournalConflictException(
                "The administrative journal result differs from the requested completion.");
        }
    }

    private static AdministrationJournalResult ToResult(
        AdministrationCommandJournalRow row) =>
        new(
            Enum.Parse<AdministrationJournalResultOutcome>(row.Outcome!, ignoreCase: false),
            row.ResultCode!,
            row.ExitCategory!.Value,
            row.ResultRevision,
            ControlPlaneMapping.ParseUtc(row.CompletedAtUtc!));

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
