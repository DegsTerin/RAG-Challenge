// Purpose: Executes explicit, leased, and control-plane-audited cleanup of expired derived generations and physically unreachable immutable content.
using System.Security.Cryptography;
using System.Text;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Infrastructure.Persistence;

public sealed class SqliteStorageMaintenance(SqliteStoreOptions options)
    : IStorageMaintenance
{
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(10);

    private readonly SqliteStoreOptions options =
        options ?? throw new ArgumentNullException(nameof(options));

    public async Task<StorageCleanupResult> RunManualCleanupAsync(
        OperationId operationId,
        CorpusId corpusId,
        DateTimeOffset requestedAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operationId);
        ArgumentNullException.ThrowIfNull(corpusId);
        ControlPlaneMapping.EnsureUtc(requestedAt, nameof(requestedAt));
        var plan = await PlanAsync(
            operationId,
            corpusId,
            requestedAt,
            cancellationToken).ConfigureAwait(false);

        if (plan.AlreadyApplied)
        {
            return new StorageCleanupResult(operationId, 0, 0, AlreadyApplied: true);
        }

        var vectorStore = new SqliteVectorIndexStore(options);
        var removedGenerations = 0;

        foreach (var generationId in plan.VectorGenerations)
        {
            if (await vectorStore.DeleteGenerationIfPresentAsync(
                    generationId,
                    cancellationToken).ConfigureAwait(false))
            {
                removedGenerations++;
            }
        }

        var contentStore = new ImmutableContentStore(options);
        var removedContent = plan.ContentObjects.Count(contentStore.DeleteIfPresent);
        await CompleteAsync(
            operationId,
            corpusId,
            requestedAt,
            removedGenerations,
            removedContent,
            cancellationToken).ConfigureAwait(false);
        return new StorageCleanupResult(
            operationId,
            removedGenerations,
            removedContent,
            AlreadyApplied: false);
    }

    private async Task<CleanupPlan> PlanAsync(
        OperationId operationId,
        CorpusId corpusId,
        DateTimeOffset requestedAt,
        CancellationToken cancellationToken)
    {
        await using var context = options.CreateControlContext();
        await using var transaction = await BeginImmediateAsync(
            context,
            cancellationToken).ConfigureAwait(false);
        var operation = await context.AdminOperations.SingleOrDefaultAsync(
            row => row.OperationId == operationId.Value,
            cancellationToken).ConfigureAwait(false);

        if (operation is not null)
        {
            if (!string.Equals(operation.OperationKind, "ManualCleanup", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "The cleanup operation identity belongs to another mutation.");
            }

            if (string.Equals(operation.Status, "Applied", StringComparison.Ordinal))
            {
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                return new CleanupPlan([], [], AlreadyApplied: true);
            }

            if (!string.Equals(operation.Status, "InProgress", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "A completed non-applied cleanup operation cannot be replayed.");
            }
        }

        var otherLeases = await context.RecoveryLeases
            .Where(row => row.CorpusId == corpusId.Value &&
                row.OperationId != operationId.Value)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        if (otherLeases.Any(row =>
            ControlPlaneMapping.ParseUtc(row.ExpiresAtUtc) > requestedAt))
        {
            throw new InvalidOperationException(
                "Another storage-maintenance lease is active for this corpus.");
        }

        var retentionRows = await context.GenerationRetentions
            .Where(row => row.CorpusId == corpusId.Value)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var expiredHolds = retentionRows
            .Where(row => row.ProtectionRole == "Hold" &&
                ControlPlaneMapping.ParseUtc(row.RetainUntilUtc) <= requestedAt)
            .ToArray();
        var protectedGenerationIds = retentionRows
            .Except(expiredHolds)
            .Select(row => row.IndexGenerationId)
            .ToHashSet(StringComparer.Ordinal);
        var reachableContent = await FindReachableContentAsync(
            context,
            corpusId,
            protectedGenerationIds,
            cancellationToken).ConfigureAwait(false);
        var allContentIds = await context.ContentObjects.AsNoTracking()
            .Select(row => row.ContentSha256)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var physicalContentCandidates = allContentIds
            .Where(contentId => !reachableContent.Contains(contentId))
            .Select(contentId => new ContentObjectId(contentId))
            .ToArray();

        if (operation is null)
        {
            operation = new AdminOperationRow
            {
                OperationId = operationId.Value,
                CorpusId = corpusId.Value,
                OperationKind = "ManualCleanup",
                Status = "InProgress",
                ExpectedRevision = null,
                ResultRevision = null,
                RequestedAtUtc = ControlPlaneMapping.FormatUtc(requestedAt),
                CompletedAtUtc = null,
            };
            context.AdminOperations.Add(operation);
        }

        var lease = await context.RecoveryLeases.SingleOrDefaultAsync(
            row => row.CorpusId == corpusId.Value &&
                row.LeaseName == "storage-maintenance",
            cancellationToken).ConfigureAwait(false);
        var expiresAt = requestedAt + LeaseDuration;

        if (lease is null)
        {
            context.RecoveryLeases.Add(new RecoveryLeaseRow
            {
                CorpusId = corpusId.Value,
                LeaseName = "storage-maintenance",
                OperationId = operationId.Value,
                AcquiredAtUtc = ControlPlaneMapping.FormatUtc(requestedAt),
                ExpiresAtUtc = ControlPlaneMapping.FormatUtc(expiresAt),
            });
        }
        else
        {
            lease.OperationId = operationId.Value;
            lease.AcquiredAtUtc = ControlPlaneMapping.FormatUtc(requestedAt);
            lease.ExpiresAtUtc = ControlPlaneMapping.FormatUtc(expiresAt);
        }

        context.GenerationRetentions.RemoveRange(expiredHolds);
        await AddAuditIfMissingAsync(
            context,
            operationId,
            corpusId,
            "CleanupPlanned",
            requestedAt,
            $"generations={expiredHolds.Length};content={physicalContentCandidates.Length}",
            cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new CleanupPlan(
            expiredHolds.Select(row => new IndexGenerationId(row.IndexGenerationId)).ToArray(),
            physicalContentCandidates,
            AlreadyApplied: false);
    }

    private static async Task<HashSet<string>> FindReachableContentAsync(
        ControlPlaneDbContext context,
        CorpusId corpusId,
        HashSet<string> protectedGenerationIds,
        CancellationToken cancellationToken)
    {
        var reachableDocumentKeys = new HashSet<(string DocumentId, long Version)>();
        var catalogueHead = await context.CatalogueHeads.AsNoTracking()
            .SingleOrDefaultAsync(
                row => row.CorpusId == corpusId.Value,
                cancellationToken).ConfigureAwait(false);

        if (catalogueHead is not null)
        {
            var catalogueDocuments = await context.CatalogueRevisionDocuments.AsNoTracking()
                .Where(row => row.CorpusId == corpusId.Value &&
                    row.CatalogueRevision == catalogueHead.CatalogueRevision &&
                    row.Status != "Removed")
                .Select(row => new { row.DocumentId, row.DocumentVersion })
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var row in catalogueDocuments)
            {
                reachableDocumentKeys.Add((row.DocumentId, row.DocumentVersion));
            }
        }

        if (protectedGenerationIds.Count > 0)
        {
            var generationDocuments = await context.GenerationManifestBindings.AsNoTracking()
                .Where(row => row.CorpusId == corpusId.Value &&
                    protectedGenerationIds.Contains(row.IndexGenerationId))
                .Select(row => new { row.DocumentId, row.DocumentVersion })
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var row in generationDocuments)
            {
                reachableDocumentKeys.Add((row.DocumentId, row.DocumentVersion));
            }
        }

        var documentRows = await context.DocumentVersions.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value)
            .Select(row => new
            {
                row.DocumentId,
                row.DocumentVersion,
                row.ContentSha256,
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        return documentRows
            .Where(row => reachableDocumentKeys.Contains(
                (row.DocumentId, row.DocumentVersion)))
            .Select(row => row.ContentSha256)
            .ToHashSet(StringComparer.Ordinal);
    }

    private async Task CompleteAsync(
        OperationId operationId,
        CorpusId corpusId,
        DateTimeOffset completedAt,
        int removedGenerations,
        int removedContent,
        CancellationToken cancellationToken)
    {
        await using var context = options.CreateControlContext();
        await using var transaction = await BeginImmediateAsync(
            context,
            cancellationToken).ConfigureAwait(false);
        var operation = await context.AdminOperations.SingleAsync(
            row => row.OperationId == operationId.Value &&
                row.OperationKind == "ManualCleanup",
            cancellationToken).ConfigureAwait(false);

        if (operation.Status == "Applied")
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        operation.Status = "Applied";
        operation.CompletedAtUtc = ControlPlaneMapping.FormatUtc(completedAt);
        var lease = await context.RecoveryLeases.SingleOrDefaultAsync(
            row => row.CorpusId == corpusId.Value &&
                row.LeaseName == "storage-maintenance" &&
                row.OperationId == operationId.Value,
            cancellationToken).ConfigureAwait(false);

        if (lease is not null)
        {
            context.RecoveryLeases.Remove(lease);
        }

        await AddAuditIfMissingAsync(
            context,
            operationId,
            corpusId,
            "CleanupApplied",
            completedAt,
            $"generations={removedGenerations};content={removedContent}",
            cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task AddAuditIfMissingAsync(
        ControlPlaneDbContext context,
        OperationId operationId,
        CorpusId corpusId,
        string eventType,
        DateTimeOffset occurredAt,
        string details,
        CancellationToken cancellationToken)
    {
        var eventId = $"audit-{Sha256($"{operationId.Value}\n{eventType}")}";

        if (await context.AuditEvents.AnyAsync(
                row => row.AuditEventId == eventId,
                cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        context.AuditEvents.Add(new AuditEventRow
        {
            AuditEventId = eventId,
            OperationId = operationId.Value,
            CorpusId = corpusId.Value,
            EventType = eventType,
            OccurredAtUtc = ControlPlaneMapping.FormatUtc(occurredAt),
            DetailsDigest = Sha256(details),
        });
    }

    private static async Task<SqliteTransaction> BeginImmediateAsync(
        ControlPlaneDbContext context,
        CancellationToken cancellationToken)
    {
        await context.Database
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        var connection = (SqliteConnection)context.Database.GetDbConnection();
        var transaction = connection.BeginTransaction(deferred: false);
        context.Database.UseTransaction(transaction);
        return transaction;
    }

    private static string Sha256(string value) =>
        Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private sealed record CleanupPlan(
        IReadOnlyCollection<IndexGenerationId> VectorGenerations,
        IReadOnlyCollection<ContentObjectId> ContentObjects,
        bool AlreadyApplied);
}
