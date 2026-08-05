// Purpose: Executes explicit, leased, replay-safe cleanup while reconciling durable references with crash-surviving content reservations.
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Infrastructure.Persistence;

public sealed class SqliteStorageMaintenance(SqliteStoreOptions options)
    : IStorageMaintenance
{
    private const int CleanupPlanSchemaVersion = 1;
    private const string CleanupOperationKind = "ManualCleanup";
    private const string CleanupLeaseName = "storage-maintenance";
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(10);
    private static readonly JsonSerializerOptions PlanJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

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
        var contentStore = new ImmutableContentStore(options);
        var prepared = await PrepareAsync(
            contentStore,
            operationId,
            corpusId,
            requestedAt,
            cancellationToken).ConfigureAwait(false);

        if (prepared.AlreadyApplied)
        {
            await ReconcileAndFinaliseAppliedReservationsAsync(
                contentStore,
                operationId,
                expectedContent: null,
                cancellationToken).ConfigureAwait(false);
            contentStore.DeleteCleanupPlanIfComplete(operationId);
            return new StorageCleanupResult(operationId, 0, 0, AlreadyApplied: true);
        }

        var plan = prepared.Plan ?? throw new InvalidOperationException(
            "An in-progress cleanup operation requires its persisted plan.");
        await ReconcileInProgressReservationsAsync(
            contentStore,
            operationId,
            plan,
            cancellationToken).ConfigureAwait(false);

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

        foreach (var contentObject in plan.ContentObjects)
        {
            await RemoveContentIfGloballyUnreferencedAsync(
                contentStore,
                operationId,
                contentObject,
                cancellationToken).ConfigureAwait(false);
        }

        var removedContent = await ReconcileBeforeCompletionAsync(
            contentStore,
            operationId,
            plan,
            cancellationToken).ConfigureAwait(false);
        await CompleteAsync(
            operationId,
            corpusId,
            requestedAt,
            removedGenerations,
            removedContent,
            cancellationToken).ConfigureAwait(false);
        await ReconcileAndFinaliseAppliedReservationsAsync(
            contentStore,
            operationId,
            plan.ContentObjects.ToDictionary(
                item => item.ContentObjectId.Value,
                item => item.ByteLength,
                StringComparer.Ordinal),
            cancellationToken).ConfigureAwait(false);
        contentStore.DeleteCleanupPlanIfComplete(operationId);
        return new StorageCleanupResult(
            operationId,
            removedGenerations,
            removedContent,
            AlreadyApplied: false);
    }

    private async Task<PreparedCleanup> PrepareAsync(
        ImmutableContentStore contentStore,
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
            ValidateOperationIdentity(operation, corpusId, requestedAt);

            if (string.Equals(operation.Status, "Applied", StringComparison.Ordinal))
            {
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                return new PreparedCleanup(null, AlreadyApplied: true);
            }

            if (!string.Equals(operation.Status, "InProgress", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "A completed non-applied cleanup operation cannot be replayed.");
            }
        }

        await EnsureLeaseAvailableAsync(
            context,
            operationId,
            corpusId,
            requestedAt,
            cancellationToken).ConfigureAwait(false);
        var planBytes = await contentStore.ReadCleanupPlanAsync(
            operationId,
            cancellationToken).ConfigureAwait(false);
        PersistedCleanupPlan plan;

        if (operation is not null)
        {
            if (planBytes is null)
            {
                throw new InvalidDataException(
                    "An in-progress cleanup operation has no persisted plan.");
            }

            plan = ParseAndValidatePlan(planBytes, operationId, corpusId, requestedAt);
            await ValidateAuditAsync(
                context,
                operationId,
                corpusId,
                "CleanupPlanned",
                requestedAt,
                plan.Digest,
                cancellationToken).ConfigureAwait(false);
        }
        else
        {
            plan = planBytes is null
                ? await BuildCurrentPlanAsync(
                    context,
                    operationId,
                    corpusId,
                    requestedAt,
                    cancellationToken).ConfigureAwait(false)
                : ParseAndValidatePlan(planBytes, operationId, corpusId, requestedAt);

            if (planBytes is null)
            {
                await contentStore.PublishCleanupPlanAsync(
                    operationId,
                    plan.CanonicalBytes,
                    cancellationToken).ConfigureAwait(false);
            }

            await ValidatePlanCanBeAdoptedAsync(
                context,
                plan,
                corpusId,
                requestedAt,
                cancellationToken).ConfigureAwait(false);
            operation = new AdminOperationRow
            {
                OperationId = operationId.Value,
                CorpusId = corpusId.Value,
                OperationKind = CleanupOperationKind,
                Status = "InProgress",
                ExpectedRevision = null,
                ResultRevision = null,
                RequestedAtUtc = ControlPlaneMapping.FormatUtc(requestedAt),
                CompletedAtUtc = null,
            };
            context.AdminOperations.Add(operation);
            await RemovePlannedExpiredHoldsAsync(
                context,
                plan,
                corpusId,
                requestedAt,
                cancellationToken).ConfigureAwait(false);
            await AddOrValidateAuditAsync(
                context,
                operationId,
                corpusId,
                "CleanupPlanned",
                requestedAt,
                plan.Digest,
                cancellationToken).ConfigureAwait(false);
        }

        await UpsertLeaseAsync(
            context,
            operationId,
            corpusId,
            requestedAt,
            cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new PreparedCleanup(plan, AlreadyApplied: false);
    }

    private static async Task<PersistedCleanupPlan> BuildCurrentPlanAsync(
        ControlPlaneDbContext context,
        OperationId operationId,
        CorpusId corpusId,
        DateTimeOffset requestedAt,
        CancellationToken cancellationToken)
    {
        var retentionRows = await context.GenerationRetentions.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var vectorGenerations = retentionRows
            .Where(row => row.ProtectionRole == "Hold" &&
                ControlPlaneMapping.ParseUtc(row.RetainUntilUtc) <= requestedAt)
            .Select(row => row.IndexGenerationId)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .Select(value => new CleanupGenerationPlanItem(value))
            .ToArray();
        var globallyReferencedContent = await FindGloballyReferencedContentAsync(
            context,
            cancellationToken).ConfigureAwait(false);
        var contentObjects = (await context.ContentObjects.AsNoTracking()
            .Select(row => new { row.ContentSha256, row.ByteLength })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false))
            .Where(row => !globallyReferencedContent.Contains(row.ContentSha256))
            .OrderBy(row => row.ContentSha256, StringComparer.Ordinal)
            .Select(row => new CleanupContentPlanItem(row.ContentSha256, row.ByteLength))
            .ToArray();
        var document = new CleanupPlanDocument(
            CleanupPlanSchemaVersion,
            operationId.Value,
            corpusId.Value,
            ControlPlaneMapping.FormatUtc(requestedAt),
            vectorGenerations,
            contentObjects);
        return CreatePersistedPlan(document);
    }

    private static PersistedCleanupPlan ParseAndValidatePlan(
        byte[] bytes,
        OperationId operationId,
        CorpusId corpusId,
        DateTimeOffset requestedAt)
    {
        CleanupPlanDocument document;

        try
        {
            document = JsonSerializer.Deserialize<CleanupPlanDocument>(bytes, PlanJsonOptions)
                ?? throw new InvalidDataException("A cleanup plan is empty.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("A cleanup plan is not valid canonical JSON.", exception);
        }

        ValidatePlanDocument(document, operationId, corpusId, requestedAt);
        var plan = CreatePersistedPlan(document);

        if (!CryptographicOperations.FixedTimeEquals(bytes, plan.CanonicalBytes))
        {
            throw new InvalidDataException("A cleanup plan is not in canonical form.");
        }

        return plan;
    }

    private static PersistedCleanupPlan CreatePersistedPlan(CleanupPlanDocument document)
    {
        var canonicalBytes = JsonSerializer.SerializeToUtf8Bytes(document, PlanJsonOptions);
        var vectorGenerations = document.VectorGenerations
            .Select(item => new IndexGenerationId(item.IndexGenerationId))
            .ToArray();
        var contentObjects = document.ContentObjects
            .Select(item => new CleanupContentCandidate(
                new ContentObjectId(item.ContentObjectId),
                item.ByteLength))
            .ToArray();
        return new PersistedCleanupPlan(
            vectorGenerations,
            contentObjects,
            canonicalBytes,
            Sha256(canonicalBytes));
    }

    private static void ValidatePlanDocument(
        CleanupPlanDocument document,
        OperationId operationId,
        CorpusId corpusId,
        DateTimeOffset requestedAt)
    {
        if (document.SchemaVersion != CleanupPlanSchemaVersion ||
            !string.Equals(document.OperationId, operationId.Value, StringComparison.Ordinal) ||
            !string.Equals(document.CorpusId, corpusId.Value, StringComparison.Ordinal) ||
            !string.Equals(
                document.RequestedAtUtc,
                ControlPlaneMapping.FormatUtc(requestedAt),
                StringComparison.Ordinal) ||
            document.VectorGenerations is null ||
            document.ContentObjects is null)
        {
            throw new InvalidDataException(
                "A cleanup plan does not match its operation identity and schema.");
        }

        var generationIds = document.VectorGenerations
            .Select(item => new IndexGenerationId(item.IndexGenerationId).Value)
            .ToArray();
        var contentIds = document.ContentObjects
            .Select(item =>
            {
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(item.ByteLength);
                return new ContentObjectId(item.ContentObjectId).Value;
            })
            .ToArray();
        EnsureStrictOrdinalOrder(generationIds, "generation");
        EnsureStrictOrdinalOrder(contentIds, "content object");
    }

    private static void EnsureStrictOrdinalOrder(string[] values, string label)
    {
        for (var index = 1; index < values.Length; index++)
        {
            if (string.CompareOrdinal(values[index - 1], values[index]) >= 0)
            {
                throw new InvalidDataException(
                    $"A cleanup plan contains a duplicated or unordered {label} identity.");
            }
        }
    }

    private static async Task ValidatePlanCanBeAdoptedAsync(
        ControlPlaneDbContext context,
        PersistedCleanupPlan plan,
        CorpusId corpusId,
        DateTimeOffset requestedAt,
        CancellationToken cancellationToken)
    {
        foreach (var generationId in plan.VectorGenerations)
        {
            var hold = await context.GenerationRetentions.AsNoTracking().SingleOrDefaultAsync(
                row => row.CorpusId == corpusId.Value &&
                    row.IndexGenerationId == generationId.Value &&
                    row.ProtectionRole == "Hold",
                cancellationToken).ConfigureAwait(false);

            if (hold is null ||
                ControlPlaneMapping.ParseUtc(hold.RetainUntilUtc) > requestedAt)
            {
                throw new InvalidDataException(
                    "A persisted cleanup plan no longer identifies its original expired hold.");
            }
        }

        foreach (var candidate in plan.ContentObjects)
        {
            var row = await context.ContentObjects.AsNoTracking().SingleOrDefaultAsync(
                item => item.ContentSha256 == candidate.ContentObjectId.Value,
                cancellationToken).ConfigureAwait(false);

            if (row is null || row.ByteLength != candidate.ByteLength)
            {
                throw new InvalidDataException(
                    "A persisted cleanup plan no longer identifies its original content object.");
            }
        }
    }

    private static async Task RemovePlannedExpiredHoldsAsync(
        ControlPlaneDbContext context,
        PersistedCleanupPlan plan,
        CorpusId corpusId,
        DateTimeOffset requestedAt,
        CancellationToken cancellationToken)
    {
        var plannedIds = plan.VectorGenerations
            .Select(item => item.Value)
            .ToHashSet(StringComparer.Ordinal);
        var rows = (await context.GenerationRetentions
            .Where(row => row.CorpusId == corpusId.Value &&
                plannedIds.Contains(row.IndexGenerationId) &&
                row.ProtectionRole == "Hold")
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false))
            .Where(row => ControlPlaneMapping.ParseUtc(row.RetainUntilUtc) <= requestedAt)
            .ToArray();

        if (rows.Length != plannedIds.Count)
        {
            throw new InvalidDataException(
                "A persisted cleanup plan cannot remove its exact expired holds.");
        }

        context.GenerationRetentions.RemoveRange(rows);
    }

    private async Task ReconcileInProgressReservationsAsync(
        ImmutableContentStore contentStore,
        OperationId operationId,
        PersistedCleanupPlan plan,
        CancellationToken cancellationToken)
    {
        var expected = plan.ContentObjects.ToDictionary(
            item => item.ContentObjectId.Value,
            item => item.ByteLength,
            StringComparer.Ordinal);

        foreach (var reservation in contentStore.EnumerateDeletionReservations(operationId))
        {
            if (!expected.TryGetValue(reservation.ContentObjectId.Value, out var byteLength))
            {
                throw new InvalidDataException(
                    "A cleanup reservation is not part of the persisted operation plan.");
            }

            await using var context = options.CreateControlContext();
            await using var transaction = await BeginImmediateAsync(
                context,
                cancellationToken).ConfigureAwait(false);
            var state = await ReadReferenceStateAsync(
                context,
                reservation.ContentObjectId,
                cancellationToken).ConfigureAwait(false);
            await ImmutableContentStore.VerifyDeletionReservationAsync(
                reservation,
                byteLength,
                cancellationToken).ConfigureAwait(false);

            if (state.HasDurableReference)
            {
                if (state.ContentObject is null || state.ContentObject.ByteLength != byteLength)
                {
                    throw new InvalidDataException(
                        "Durable content references exist without their exact content row.");
                }

                ImmutableContentStore.RestoreDeletionReservation(reservation);
            }
            else if (state.ContentObject is not null)
            {
                if (state.ContentObject.ByteLength != byteLength)
                {
                    throw new InvalidDataException(
                        "A reserved content row differs from its persisted cleanup plan.");
                }

                context.ContentObjects.Remove(state.ContentObject);
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task RemoveContentIfGloballyUnreferencedAsync(
        ImmutableContentStore contentStore,
        OperationId operationId,
        CleanupContentCandidate candidate,
        CancellationToken cancellationToken)
    {
        await using var context = options.CreateControlContext();
        await using var transaction = await BeginImmediateAsync(
            context,
            cancellationToken).ConfigureAwait(false);
        var state = await ReadReferenceStateAsync(
            context,
            candidate.ContentObjectId,
            cancellationToken).ConfigureAwait(false);

        if (state.ContentObject is null)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        if (state.ContentObject.ByteLength != candidate.ByteLength)
        {
            throw new InvalidDataException(
                "A cleanup candidate differs from its persisted plan.");
        }

        if (state.HasDurableReference)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        ContentDeletionReservation? reservation = null;

        try
        {
            reservation = await contentStore.ReserveForDeletionAsync(
                operationId,
                candidate.ContentObjectId,
                candidate.ByteLength,
                cancellationToken).ConfigureAwait(false);

            if (!reservation.WasPresent)
            {
                throw new InvalidDataException(
                    "A planned content object is physically absent before reservation.");
            }

            context.ContentObjects.Remove(state.ContentObject);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            if (reservation is not null)
            {
                ImmutableContentStore.RestoreDeletionReservation(reservation);
            }

            throw;
        }
    }

    private async Task<int> ReconcileBeforeCompletionAsync(
        ImmutableContentStore contentStore,
        OperationId operationId,
        PersistedCleanupPlan plan,
        CancellationToken cancellationToken)
    {
        await ReconcileInProgressReservationsAsync(
            contentStore,
            operationId,
            plan,
            cancellationToken).ConfigureAwait(false);
        var reservations = contentStore.EnumerateDeletionReservations(operationId);
        var reservationIds = reservations
            .Select(item => item.ContentObjectId.Value)
            .ToHashSet(StringComparer.Ordinal);
        var expected = plan.ContentObjects.ToDictionary(
            item => item.ContentObjectId.Value,
            item => item.ByteLength,
            StringComparer.Ordinal);

        foreach (var reservation in reservations)
        {
            if (!expected.TryGetValue(reservation.ContentObjectId.Value, out var byteLength))
            {
                throw new InvalidDataException(
                    "A cleanup reservation is not part of the persisted operation plan.");
            }

            await using var context = options.CreateControlContext();
            await using var transaction = await BeginImmediateAsync(
                context,
                cancellationToken).ConfigureAwait(false);
            var state = await ReadReferenceStateAsync(
                context,
                reservation.ContentObjectId,
                cancellationToken).ConfigureAwait(false);

            if (state.ContentObject is not null || state.HasDurableReference)
            {
                throw new InvalidDataException(
                    "A cleanup reservation regained durable reachability before completion.");
            }

            await ImmutableContentStore.VerifyDeletionReservationAsync(
                reservation,
                byteLength,
                cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        foreach (var candidate in plan.ContentObjects.Where(
            item => !reservationIds.Contains(item.ContentObjectId.Value)))
        {
            await using var context = options.CreateControlContext();
            await using var transaction = await BeginImmediateAsync(
                context,
                cancellationToken).ConfigureAwait(false);
            var state = await ReadReferenceStateAsync(
                context,
                candidate.ContentObjectId,
                cancellationToken).ConfigureAwait(false);

            if (state.ContentObject is null || !state.HasDurableReference ||
                state.ContentObject.ByteLength != candidate.ByteLength)
            {
                throw new InvalidDataException(
                    "A planned object is neither safely reserved nor durably reachable.");
            }

            await using var stream = await contentStore.OpenReadAsync(
                candidate.ContentObjectId,
                cancellationToken).ConfigureAwait(false);

            if (stream.Length != candidate.ByteLength)
            {
                throw new InvalidDataException(
                    "A restored content object differs from its cleanup plan.");
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        return reservations.Count;
    }

    private async Task ReconcileAndFinaliseAppliedReservationsAsync(
        ImmutableContentStore contentStore,
        OperationId operationId,
        Dictionary<string, long>? expectedContent,
        CancellationToken cancellationToken)
    {
        foreach (var reservation in contentStore.EnumerateDeletionReservations(operationId))
        {
            await using var context = options.CreateControlContext();
            await using var transaction = await BeginImmediateAsync(
                context,
                cancellationToken).ConfigureAwait(false);
            var operation = await context.AdminOperations.AsNoTracking().SingleAsync(
                row => row.OperationId == operationId.Value &&
                    row.OperationKind == CleanupOperationKind &&
                    row.Status == "Applied",
                cancellationToken).ConfigureAwait(false);
            _ = operation;
            var state = await ReadReferenceStateAsync(
                context,
                reservation.ContentObjectId,
                cancellationToken).ConfigureAwait(false);

            if (state.HasDurableReference && state.ContentObject is null)
            {
                throw new InvalidDataException(
                    "Durable content references exist without their content row.");
            }

            var byteLength = state.ContentObject?.ByteLength ??
                (expectedContent is not null &&
                    expectedContent.TryGetValue(reservation.ContentObjectId.Value, out var expected)
                    ? expected
                    : new FileInfo(reservation.ReservationPath).Length);
            await ImmutableContentStore.VerifyDeletionReservationAsync(
                reservation,
                byteLength,
                cancellationToken).ConfigureAwait(false);

            if (state.ContentObject is not null)
            {
                ImmutableContentStore.RestoreDeletionReservation(reservation);
            }
            else
            {
                ImmutableContentStore.FinaliseDeletionReservation(
                    new ContentDeletionFinalisation(reservation));
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
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
            row => row.OperationId == operationId.Value,
            cancellationToken).ConfigureAwait(false);
        ValidateOperationIdentity(operation, corpusId, completedAt);

        if (operation.Status == "Applied")
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        if (!string.Equals(operation.Status, "InProgress", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Only an in-progress cleanup operation can be completed.");
        }

        operation.Status = "Applied";
        operation.CompletedAtUtc = ControlPlaneMapping.FormatUtc(completedAt);
        var lease = await context.RecoveryLeases.SingleOrDefaultAsync(
            row => row.CorpusId == corpusId.Value &&
                row.LeaseName == CleanupLeaseName &&
                row.OperationId == operationId.Value,
            cancellationToken).ConfigureAwait(false);

        if (lease is not null)
        {
            context.RecoveryLeases.Remove(lease);
        }

        await AddOrValidateAuditAsync(
            context,
            operationId,
            corpusId,
            "CleanupApplied",
            completedAt,
            Sha256($"generations={removedGenerations};content={removedContent}"),
            cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void ValidateOperationIdentity(
        AdminOperationRow operation,
        CorpusId corpusId,
        DateTimeOffset requestedAt)
    {
        if (!string.Equals(operation.OperationKind, CleanupOperationKind, StringComparison.Ordinal) ||
            !string.Equals(operation.CorpusId, corpusId.Value, StringComparison.Ordinal) ||
            !string.Equals(
                operation.RequestedAtUtc,
                ControlPlaneMapping.FormatUtc(requestedAt),
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The cleanup operation identity was replayed with divergent input.");
        }
    }

    private static async Task<ContentReferenceState> ReadReferenceStateAsync(
        ControlPlaneDbContext context,
        ContentObjectId contentObjectId,
        CancellationToken cancellationToken)
    {
        var row = await context.ContentObjects.SingleOrDefaultAsync(
            item => item.ContentSha256 == contentObjectId.Value,
            cancellationToken).ConfigureAwait(false);
        var referencedByDocument = await context.DocumentVersions.AsNoTracking().AnyAsync(
            item => item.ContentSha256 == contentObjectId.Value,
            cancellationToken).ConfigureAwait(false);
        var referencedBySnapshot = await context.OfficialSourceSnapshots.AsNoTracking().AnyAsync(
            item => item.ContentSha256 == contentObjectId.Value,
            cancellationToken).ConfigureAwait(false);
        return new ContentReferenceState(row, referencedByDocument, referencedBySnapshot);
    }

    private static async Task<HashSet<string>> FindGloballyReferencedContentAsync(
        ControlPlaneDbContext context,
        CancellationToken cancellationToken)
    {
        var referenced = (await context.DocumentVersions.AsNoTracking()
            .Select(row => row.ContentSha256)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false))
            .ToHashSet(StringComparer.Ordinal);
        var officialSnapshots = await context.OfficialSourceSnapshots.AsNoTracking()
            .Select(row => row.ContentSha256)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        referenced.UnionWith(officialSnapshots);
        return referenced;
    }

    private static async Task EnsureLeaseAvailableAsync(
        ControlPlaneDbContext context,
        OperationId operationId,
        CorpusId corpusId,
        DateTimeOffset requestedAt,
        CancellationToken cancellationToken)
    {
        var otherLeases = await context.RecoveryLeases.AsNoTracking()
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
    }

    private static async Task UpsertLeaseAsync(
        ControlPlaneDbContext context,
        OperationId operationId,
        CorpusId corpusId,
        DateTimeOffset requestedAt,
        CancellationToken cancellationToken)
    {
        var lease = await context.RecoveryLeases.SingleOrDefaultAsync(
            row => row.CorpusId == corpusId.Value &&
                row.LeaseName == CleanupLeaseName,
            cancellationToken).ConfigureAwait(false);
        var expiresAt = requestedAt + LeaseDuration;

        if (lease is null)
        {
            context.RecoveryLeases.Add(new RecoveryLeaseRow
            {
                CorpusId = corpusId.Value,
                LeaseName = CleanupLeaseName,
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
    }

    private static async Task AddOrValidateAuditAsync(
        ControlPlaneDbContext context,
        OperationId operationId,
        CorpusId corpusId,
        string eventType,
        DateTimeOffset occurredAt,
        string detailsDigest,
        CancellationToken cancellationToken)
    {
        var eventId = $"audit-{Sha256($"{operationId.Value}\n{eventType}")}";
        var existing = await context.AuditEvents.SingleOrDefaultAsync(
            row => row.AuditEventId == eventId,
            cancellationToken).ConfigureAwait(false);

        if (existing is not null)
        {
            ValidateAuditRow(
                existing,
                operationId,
                corpusId,
                eventType,
                occurredAt,
                detailsDigest);
            return;
        }

        context.AuditEvents.Add(new AuditEventRow
        {
            AuditEventId = eventId,
            OperationId = operationId.Value,
            CorpusId = corpusId.Value,
            EventType = eventType,
            OccurredAtUtc = ControlPlaneMapping.FormatUtc(occurredAt),
            DetailsDigest = detailsDigest,
        });
    }

    private static async Task ValidateAuditAsync(
        ControlPlaneDbContext context,
        OperationId operationId,
        CorpusId corpusId,
        string eventType,
        DateTimeOffset occurredAt,
        string detailsDigest,
        CancellationToken cancellationToken)
    {
        var eventId = $"audit-{Sha256($"{operationId.Value}\n{eventType}")}";
        var existing = await context.AuditEvents.AsNoTracking().SingleOrDefaultAsync(
            row => row.AuditEventId == eventId,
            cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidDataException(
                "An in-progress cleanup operation has no exact planning audit event.");
        ValidateAuditRow(
            existing,
            operationId,
            corpusId,
            eventType,
            occurredAt,
            detailsDigest);
    }

    private static void ValidateAuditRow(
        AuditEventRow row,
        OperationId operationId,
        CorpusId corpusId,
        string eventType,
        DateTimeOffset occurredAt,
        string detailsDigest)
    {
        if (!string.Equals(row.OperationId, operationId.Value, StringComparison.Ordinal) ||
            !string.Equals(row.CorpusId, corpusId.Value, StringComparison.Ordinal) ||
            !string.Equals(row.EventType, eventType, StringComparison.Ordinal) ||
            !string.Equals(
                row.OccurredAtUtc,
                ControlPlaneMapping.FormatUtc(occurredAt),
                StringComparison.Ordinal) ||
            !string.Equals(row.DetailsDigest, detailsDigest, StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                "A cleanup audit identity contains divergent persisted evidence.");
        }
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
        Sha256(Encoding.UTF8.GetBytes(value));

    private static string Sha256(ReadOnlySpan<byte> value) =>
        Convert.ToHexString(SHA256.HashData(value)).ToLowerInvariant();

    private sealed record PreparedCleanup(
        PersistedCleanupPlan? Plan,
        bool AlreadyApplied);

    private sealed record PersistedCleanupPlan(
        IReadOnlyCollection<IndexGenerationId> VectorGenerations,
        IReadOnlyCollection<CleanupContentCandidate> ContentObjects,
        byte[] CanonicalBytes,
        string Digest);

    private sealed record CleanupPlanDocument(
        int SchemaVersion,
        string OperationId,
        string CorpusId,
        string RequestedAtUtc,
        CleanupGenerationPlanItem[] VectorGenerations,
        CleanupContentPlanItem[] ContentObjects);

    private sealed record CleanupGenerationPlanItem(string IndexGenerationId);

    private sealed record CleanupContentPlanItem(string ContentObjectId, long ByteLength);

    private sealed record CleanupContentCandidate(
        ContentObjectId ContentObjectId,
        long ByteLength);

    private sealed record ContentReferenceState(
        ContentObjectRow? ContentObject,
        bool ReferencedByDocument,
        bool ReferencedBySnapshot)
    {
        public bool HasDurableReference => ReferencedByDocument || ReferencedBySnapshot;
    }
}
