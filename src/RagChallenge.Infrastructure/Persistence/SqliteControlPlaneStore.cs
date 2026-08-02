// Purpose: Implements control.db as the sole transactional authority for catalogue, observation, manifest, activation, audit, and retention state with expected-revision CAS.
using System.Security.Cryptography;
using System.Text;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.Infrastructure.Persistence;

public sealed class SqliteControlPlaneStore(SqliteStoreOptions options)
    : IControlPlaneStore
{
    public static readonly TimeSpan MinimumPreviousGenerationRetention =
        TimeSpan.FromDays(14);

    private readonly SqliteStoreOptions options =
        options ?? throw new ArgumentNullException(nameof(options));

    public async Task<StoreMutationResult> CommitCatalogueAsync(
        CatalogueCommitRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.OperationId);
        ArgumentNullException.ThrowIfNull(request.Snapshot);
        EnsureExpectedRevision(request.ExpectedCurrentRevision, request.Snapshot.Revision.Value);
        ControlPlaneMapping.EnsureUtc(request.CommittedAt, nameof(request));

        await using var context = options.CreateControlContext();
        await using var transaction = await BeginImmediateAsync(
            context,
            cancellationToken).ConfigureAwait(false);
        var idempotent = await GetIdempotentResultAsync(
            context,
            request.OperationId,
            "CatalogueCommit",
            cancellationToken).ConfigureAwait(false);

        if (idempotent is not null)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return idempotent;
        }

        var corpusId = request.Snapshot.CorpusId.Value;

        if (await HasBlockingLeaseAsync(
                context,
                request.Snapshot.CorpusId,
                request.OperationId,
                request.CommittedAt,
                cancellationToken).ConfigureAwait(false))
        {
            return new StoreMutationResult(StoreMutationOutcome.RetentionConflict, 0);
        }

        var head = await context.CatalogueHeads.SingleOrDefaultAsync(
            row => row.CorpusId == corpusId,
            cancellationToken).ConfigureAwait(false);
        var currentRevision = head?.CatalogueRevision ?? 0;

        if (currentRevision != request.ExpectedCurrentRevision)
        {
            return new StoreMutationResult(
                StoreMutationOutcome.RevisionConflict,
                currentRevision);
        }

        var corpus = await context.Corpora.SingleOrDefaultAsync(
            row => row.CorpusId == corpusId,
            cancellationToken).ConfigureAwait(false);

        if (corpus is null)
        {
            if (request.ExpectedCurrentRevision != 0)
            {
                return new StoreMutationResult(
                    StoreMutationOutcome.RevisionConflict,
                    currentRevision);
            }

            context.Corpora.Add(new CorpusRow
            {
                CorpusId = corpusId,
                CorpusRevision = 1,
                CreatedAtUtc = ControlPlaneMapping.FormatUtc(request.CommittedAt),
            });
        }

        AddOperation(
            context,
            request.OperationId,
            request.Snapshot.CorpusId,
            "CatalogueCommit",
            request.ExpectedCurrentRevision,
            request.CommittedAt);
        await AddCatalogueSnapshotAsync(context, request, cancellationToken)
            .ConfigureAwait(false);

        if (head is null)
        {
            context.CatalogueHeads.Add(new CatalogueHeadRow
            {
                CorpusId = corpusId,
                CatalogueRevision = request.Snapshot.Revision.Value,
                RowRevision = 1,
            });
        }
        else
        {
            head.CatalogueRevision = request.Snapshot.Revision.Value;
            head.RowRevision = checked(head.RowRevision + 1);
        }

        CompleteOperation(
            context,
            request.OperationId,
            request.Snapshot.CorpusId,
            "CatalogueCommitted",
            request.Snapshot.Revision.Value,
            request.CommittedAt);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new StoreMutationResult(
            StoreMutationOutcome.Applied,
            request.Snapshot.Revision.Value);
    }

    public async Task<StoreMutationResult> CommitOfficialSourceAsync(
        OfficialSourceCommitRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.OperationId);
        ArgumentNullException.ThrowIfNull(request.CorpusId);
        ArgumentNullException.ThrowIfNull(request.Registration);
        ArgumentNullException.ThrowIfNull(request.Snapshot);
        ControlPlaneMapping.EnsureUtc(request.CommittedAt, nameof(request));

        if (request.Snapshot.RegistrationId != request.Registration.Id)
        {
            throw new ArgumentException(
                "An official snapshot must name the committed registration.",
                nameof(request));
        }

        await using var context = options.CreateControlContext();
        await using var transaction = await BeginImmediateAsync(
            context,
            cancellationToken).ConfigureAwait(false);
        var idempotent = await GetIdempotentResultAsync(
            context,
            request.OperationId,
            "OfficialSourceCommit",
            cancellationToken).ConfigureAwait(false);

        if (idempotent is not null)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return idempotent;
        }

        if (await HasBlockingLeaseAsync(
                context,
                request.CorpusId,
                request.OperationId,
                request.CommittedAt,
                cancellationToken).ConfigureAwait(false))
        {
            return new StoreMutationResult(StoreMutationOutcome.RetentionConflict, 0);
        }

        if (!await context.Corpora.AnyAsync(
                row => row.CorpusId == request.CorpusId.Value,
                cancellationToken).ConfigureAwait(false))
        {
            return new StoreMutationResult(StoreMutationOutcome.NotFound, 0);
        }

        var priorRevision = await context.OfficialSourceRegistrations
            .Where(row => row.CorpusId == request.CorpusId.Value &&
                row.RegistrationId == request.Registration.Id.Value)
            .Select(row => (long?)row.RegistrationRevision)
            .MaxAsync(cancellationToken)
            .ConfigureAwait(false) ?? 0;

        if (request.Registration.Revision.Value != priorRevision + 1)
        {
            return new StoreMutationResult(
                StoreMutationOutcome.RevisionConflict,
                priorRevision);
        }

        var documentExists = await context.DocumentVersions.AnyAsync(
            row => row.CorpusId == request.CorpusId.Value &&
                row.DocumentId == request.Registration.DocumentId.Value &&
                row.ProductId == request.Registration.DatabaseProductId.Value,
            cancellationToken).ConfigureAwait(false);

        if (!documentExists)
        {
            return new StoreMutationResult(StoreMutationOutcome.NotFound, priorRevision);
        }

        AddOperation(
            context,
            request.OperationId,
            request.CorpusId,
            "OfficialSourceCommit",
            priorRevision,
            request.CommittedAt);
        context.OfficialSourceRegistrations.Add(new OfficialSourceRegistrationRow
        {
            CorpusId = request.CorpusId.Value,
            RegistrationId = request.Registration.Id.Value,
            RegistrationRevision = request.Registration.Revision.Value,
            ProductId = request.Registration.DatabaseProductId.Value,
            DocumentId = request.Registration.DocumentId.Value,
            SourceAdapterId = request.Registration.SourceAdapterId.Value,
            CanonicalHttpsUrl = request.Registration.CanonicalHttpsUrl,
            Status = request.Registration.Status.ToString(),
        });
        await AddOrValidateContentObjectAsync(
            context,
            request.Snapshot.ContentObjectId.Value,
            request.Snapshot.ByteLength,
            request.Snapshot.RetrievedAt,
            cancellationToken).ConfigureAwait(false);
        context.OfficialSourceSnapshots.Add(new OfficialSourceSnapshotRow
        {
            CorpusId = request.CorpusId.Value,
            SnapshotId = request.Snapshot.Id.Value,
            RegistrationId = request.Registration.Id.Value,
            RegistrationRevision = request.Registration.Revision.Value,
            ContentSha256 = request.Snapshot.ContentObjectId.Value,
            ByteLength = request.Snapshot.ByteLength,
            MediaType = request.Snapshot.MediaType,
            RetrievedAtUtc = ControlPlaneMapping.FormatUtc(request.Snapshot.RetrievedAt),
        });
        CompleteOperation(
            context,
            request.OperationId,
            request.CorpusId,
            "OfficialSourceCommitted",
            request.Registration.Revision.Value,
            request.CommittedAt);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new StoreMutationResult(
            StoreMutationOutcome.Applied,
            request.Registration.Revision.Value);
    }

    public async Task<StoreMutationResult> AppendObservationAsync(
        ObservationCommitRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.OperationId);
        ArgumentNullException.ThrowIfNull(request.CorpusId);
        ArgumentNullException.ThrowIfNull(request.Observation);
        ControlPlaneMapping.EnsureUtc(request.CommittedAt, nameof(request));
        EnsureExpectedRevision(
            request.ExpectedJournalRevision,
            request.Observation.JournalRevision.Value);
        var maxAgeSeconds = request.Observation.MaxAge.TotalSeconds;

        if (maxAgeSeconds != Math.Truncate(maxAgeSeconds) ||
            maxAgeSeconds > long.MaxValue)
        {
            throw new ArgumentException(
                "Observation maxAge must contain a whole number of seconds.",
                nameof(request));
        }

        await using var context = options.CreateControlContext();
        await using var transaction = await BeginImmediateAsync(
            context,
            cancellationToken).ConfigureAwait(false);
        var idempotent = await GetIdempotentResultAsync(
            context,
            request.OperationId,
            "ObservationAppend",
            cancellationToken).ConfigureAwait(false);

        if (idempotent is not null)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return idempotent;
        }

        if (await HasBlockingLeaseAsync(
                context,
                request.CorpusId,
                request.OperationId,
                request.CommittedAt,
                cancellationToken).ConfigureAwait(false))
        {
            return new StoreMutationResult(StoreMutationOutcome.RetentionConflict, 0);
        }

        var head = await context.ObservationJournalHeads.SingleOrDefaultAsync(
            row => row.CorpusId == request.CorpusId.Value,
            cancellationToken).ConfigureAwait(false);
        var currentRevision = head?.JournalRevision ?? 0;

        if (currentRevision != request.ExpectedJournalRevision)
        {
            return new StoreMutationResult(
                StoreMutationOutcome.RevisionConflict,
                currentRevision);
        }

        var snapshot = await context.OfficialSourceSnapshots.AsNoTracking()
            .SingleOrDefaultAsync(
                row => row.CorpusId == request.CorpusId.Value &&
                    row.SnapshotId == request.Observation.SnapshotId.Value,
                cancellationToken).ConfigureAwait(false);

        if (snapshot is null ||
            !string.Equals(
                snapshot.RegistrationId,
                request.Observation.RegistrationId.Value,
                StringComparison.Ordinal))
        {
            return new StoreMutationResult(StoreMutationOutcome.NotFound, currentRevision);
        }

        AddOperation(
            context,
            request.OperationId,
            request.CorpusId,
            "ObservationAppend",
            request.ExpectedJournalRevision,
            request.CommittedAt);
        context.SourceObservations.Add(new SourceObservationRow
        {
            CorpusId = request.CorpusId.Value,
            ObservationId = request.Observation.Id.Value,
            RegistrationId = request.Observation.RegistrationId.Value,
            SnapshotId = request.Observation.SnapshotId.Value,
            JournalRevision = request.Observation.JournalRevision.Value,
            State = request.Observation.State.ToString(),
            RevalidatedAtUtc = ControlPlaneMapping.FormatUtc(
                request.Observation.RevalidatedAt),
            MaxAgeSeconds = checked((long)maxAgeSeconds),
            OperationId = request.OperationId.Value,
        });

        if (head is null)
        {
            context.ObservationJournalHeads.Add(new ObservationJournalHeadRow
            {
                CorpusId = request.CorpusId.Value,
                JournalRevision = request.Observation.JournalRevision.Value,
                RowRevision = 1,
            });
        }
        else
        {
            head.JournalRevision = request.Observation.JournalRevision.Value;
            head.RowRevision = checked(head.RowRevision + 1);
        }

        CompleteOperation(
            context,
            request.OperationId,
            request.CorpusId,
            "ObservationAppended",
            request.Observation.JournalRevision.Value,
            request.CommittedAt);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new StoreMutationResult(
            StoreMutationOutcome.Applied,
            request.Observation.JournalRevision.Value);
    }

    public async Task<StoreMutationResult> CommitGenerationAsync(
        GenerationCommitRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.OperationId);
        ArgumentNullException.ThrowIfNull(request.CandidateBuildId);
        ArgumentNullException.ThrowIfNull(request.Manifest);
        ArgumentNullException.ThrowIfNull(request.Bindings);
        ControlPlaneMapping.EnsureUtc(request.FinalisedAt, nameof(request));

        if (request.Bindings.Count == 0)
        {
            throw new ArgumentException(
                "A generation manifest must bind at least one document version.",
                nameof(request));
        }

        var activeDigest = BindingDigestCanonicalizer
            .CanonicaliseActiveDocumentSet(request.Bindings)
            .Digest;
        var sourceDigest = BindingDigestCanonicalizer
            .CanonicaliseSourceBindingSet(request.Bindings)
            .Digest;

        if (activeDigest != request.Manifest.ActiveDocumentSetDigest ||
            sourceDigest != request.Manifest.SourceBindingSetDigest)
        {
            return new StoreMutationResult(StoreMutationOutcome.ValidationFailed, 0);
        }

        var vectorStore = new SqliteVectorIndexStore(options);

        if (!await vectorStore.IsValidatedGenerationAsync(
                request.CandidateBuildId,
                request.Manifest.IndexGenerationId,
                request.Manifest.ChunkCount,
                cancellationToken).ConfigureAwait(false))
        {
            return new StoreMutationResult(StoreMutationOutcome.ValidationFailed, 0);
        }

        await using var context = options.CreateControlContext();
        await using var transaction = await BeginImmediateAsync(
            context,
            cancellationToken).ConfigureAwait(false);
        var idempotent = await GetIdempotentResultAsync(
            context,
            request.OperationId,
            "GenerationCommit",
            cancellationToken).ConfigureAwait(false);

        if (idempotent is not null)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return idempotent;
        }

        if (await HasBlockingLeaseAsync(
                context,
                request.Manifest.CorpusId,
                request.OperationId,
                request.FinalisedAt,
                cancellationToken).ConfigureAwait(false))
        {
            return new StoreMutationResult(StoreMutationOutcome.RetentionConflict, 0);
        }

        var corpus = await context.Corpora.AsNoTracking().SingleOrDefaultAsync(
            row => row.CorpusId == request.Manifest.CorpusId.Value,
            cancellationToken).ConfigureAwait(false);

        if (corpus is null || corpus.CorpusRevision != request.Manifest.CorpusRevision.Value)
        {
            return new StoreMutationResult(StoreMutationOutcome.NotFound, 0);
        }

        if (!await context.CatalogueRevisions.AnyAsync(
                row => row.CorpusId == request.Manifest.CorpusId.Value &&
                    row.CatalogueRevision == request.Manifest.CatalogueRevision.Value,
                cancellationToken).ConfigureAwait(false))
        {
            return new StoreMutationResult(StoreMutationOutcome.NotFound, 0);
        }

        var activeDocuments = await context.CatalogueRevisionDocuments.AsNoTracking()
            .Where(row => row.CorpusId == request.Manifest.CorpusId.Value &&
                row.CatalogueRevision == request.Manifest.CatalogueRevision.Value &&
                row.Status == "Active")
            .Select(row => new { row.DocumentId, row.DocumentVersion })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var activeKeys = activeDocuments
            .Select(row => (row.DocumentId, row.DocumentVersion))
            .ToHashSet();

        if (request.Bindings.Any(binding =>
            !activeKeys.Contains((binding.DocumentId.Value, binding.DocumentVersion.Value))))
        {
            return new StoreMutationResult(StoreMutationOutcome.ValidationFailed, 0);
        }

        AddOperation(
            context,
            request.OperationId,
            request.Manifest.CorpusId,
            "GenerationCommit",
            request.Manifest.CatalogueRevision.Value,
            request.FinalisedAt);
        context.GenerationManifests.Add(ControlPlaneMapping.ToRow(request));
        context.GenerationManifestBindings.AddRange(
            request.Bindings.Select(binding =>
                ControlPlaneMapping.ToGenerationBindingRow(
                    request.Manifest.CorpusId,
                    request.Manifest.IndexGenerationId,
                    binding)));
        CompleteOperation(
            context,
            request.OperationId,
            request.Manifest.CorpusId,
            "GenerationCommitted",
            request.Manifest.CatalogueRevision.Value,
            request.FinalisedAt);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new StoreMutationResult(
            StoreMutationOutcome.Applied,
            request.Manifest.CatalogueRevision.Value);
    }

    public async Task<ActivationMutationResult> CompareExchangeActivationAsync(
        ActivationCompareExchangeRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.OperationId);
        ArgumentNullException.ThrowIfNull(request.ProposedRecord);
        ArgumentNullException.ThrowIfNull(request.RequiredCompatibilityKey);
        ControlPlaneMapping.EnsureUtc(request.EvaluatedAt, nameof(request));

        if (request.ExpectedCurrentRevision < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                request.ExpectedCurrentRevision,
                "An expected activation revision cannot be negative.");
        }

        if (request.PreviousGenerationRetention < MinimumPreviousGenerationRetention)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                request.PreviousGenerationRetention,
                "The previous generation must remain protected for at least 14 days.");
        }

        await using var context = options.CreateControlContext();
        await using var transaction = await BeginImmediateAsync(
            context,
            cancellationToken).ConfigureAwait(false);
        var existingOperation = await context.AdminOperations.AsNoTracking()
            .SingleOrDefaultAsync(
                row => row.OperationId == request.OperationId.Value,
                cancellationToken).ConfigureAwait(false);

        if (existingOperation is not null)
        {
            EnsureOperationIdentity(existingOperation, "ActivationCAS");
            var active = await ReadActiveActivationAsync(
                context,
                request.ProposedRecord.CorpusId,
                cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return new ActivationMutationResult(
                StoreMutationOutcome.AlreadyApplied,
                active);
        }

        if (await HasBlockingLeaseAsync(
                context,
                request.ProposedRecord.CorpusId,
                request.OperationId,
                request.EvaluatedAt,
                cancellationToken).ConfigureAwait(false))
        {
            var active = await ReadActiveActivationAsync(
                context,
                request.ProposedRecord.CorpusId,
                cancellationToken).ConfigureAwait(false);
            return new ActivationMutationResult(
                StoreMutationOutcome.RetentionConflict,
                active);
        }

        var head = await context.ActivationHeads.AsNoTracking().SingleOrDefaultAsync(
            row => row.CorpusId == request.ProposedRecord.CorpusId.Value,
            cancellationToken).ConfigureAwait(false);
        var currentRevision = head?.RecordRevision ?? 0;

        if (currentRevision != request.ExpectedCurrentRevision)
        {
            var current = await ReadActiveActivationAsync(
                context,
                request.ProposedRecord.CorpusId,
                cancellationToken).ConfigureAwait(false);
            return new ActivationMutationResult(
                StoreMutationOutcome.RevisionConflict,
                current);
        }

        var currentRecord = await ReadActiveActivationAsync(
            context,
            request.ProposedRecord.CorpusId,
            cancellationToken).ConfigureAwait(false);
        var manifestRow = await context.GenerationManifests.AsNoTracking()
            .SingleOrDefaultAsync(
                row => row.CorpusId == request.ProposedRecord.CorpusId.Value &&
                    row.IndexGenerationId == request.ProposedRecord.IndexGenerationId.Value,
                cancellationToken).ConfigureAwait(false);

        if (manifestRow is null)
        {
            return new ActivationMutationResult(StoreMutationOutcome.NotFound, currentRecord);
        }

        var vectorStore = new SqliteVectorIndexStore(options);

        if (!await vectorStore.IsValidatedGenerationAsync(
                new CandidateBuildId(manifestRow.CandidateBuildId),
                request.ProposedRecord.IndexGenerationId,
                manifestRow.ChunkCount,
                cancellationToken).ConfigureAwait(false))
        {
            return new ActivationMutationResult(
                StoreMutationOutcome.ValidationFailed,
                currentRecord);
        }

        var observationIds = request.ProposedRecord.DocumentBindings
            .Where(binding => binding.SourceObservationId is not null)
            .Select(binding => binding.SourceObservationId!.Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var observationRows = await context.SourceObservations.AsNoTracking()
            .Where(row => row.CorpusId == request.ProposedRecord.CorpusId.Value &&
                observationIds.Contains(row.ObservationId))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var observations = observationRows.ToDictionary(
            row => new OfficialObservationId(row.ObservationId),
            ControlPlaneMapping.ToDomain);
        var validation = ActivationRecordValidator.ValidateForCompareAndSwap(
            currentRecord,
            ControlPlaneMapping.ToDomain(manifestRow),
            request.ProposedRecord,
            request.RequiredCompatibilityKey,
            observations,
            request.EvaluatedAt);

        if (!validation.IsValid || !MutationKindMatches(request, currentRecord))
        {
            return new ActivationMutationResult(
                StoreMutationOutcome.ValidationFailed,
                currentRecord,
                validation.Failures);
        }

        var retentionRows = await context.GenerationRetentions
            .Where(row => row.CorpusId == request.ProposedRecord.CorpusId.Value)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var targetPreviouslyActivated = await context.ActivationRecords.AsNoTracking()
            .AnyAsync(
                row => row.CorpusId == request.ProposedRecord.CorpusId.Value &&
                    row.IndexGenerationId == request.ProposedRecord.IndexGenerationId.Value,
                cancellationToken).ConfigureAwait(false);

        if (!RetentionAllowsMutation(
                request,
                currentRecord,
                retentionRows,
                targetPreviouslyActivated))
        {
            return new ActivationMutationResult(
                StoreMutationOutcome.RetentionConflict,
                currentRecord);
        }

        AddOperation(
            context,
            request.OperationId,
            request.ProposedRecord.CorpusId,
            "ActivationCAS",
            request.ExpectedCurrentRevision,
            request.EvaluatedAt);
        context.ActivationRecords.Add(ControlPlaneMapping.ToRow(request));
        context.ActivationBindings.AddRange(
            request.ProposedRecord.DocumentBindings.Select(binding =>
                ControlPlaneMapping.ToActivationBindingRow(
                    request.ProposedRecord.CorpusId,
                    request.ProposedRecord.RecordRevision,
                    binding)));
        await ApplyRetentionAsync(
            context,
            request,
            currentRecord,
            retentionRows,
            cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (head is null)
        {
            context.ActivationHeads.Add(new ActivationHeadRow
            {
                CorpusId = request.ProposedRecord.CorpusId.Value,
                RecordRevision = request.ProposedRecord.RecordRevision.Value,
                RowRevision = 1,
            });
        }
        else
        {
            var affected = await context.Database.ExecuteSqlInterpolatedAsync(
                $"""
                UPDATE activation_heads
                SET record_revision = {request.ProposedRecord.RecordRevision.Value},
                    row_revision = row_revision + 1
                WHERE corpus_id = {request.ProposedRecord.CorpusId.Value}
                  AND record_revision = {request.ExpectedCurrentRevision};
                """,
                cancellationToken).ConfigureAwait(false);

            if (affected != 1)
            {
                throw new InvalidOperationException(
                    "The activation head changed during its immediate CAS transaction.");
            }
        }

        CompleteOperation(
            context,
            request.OperationId,
            request.ProposedRecord.CorpusId,
            $"Activation{request.MutationKind}Applied",
            request.ProposedRecord.RecordRevision.Value,
            request.EvaluatedAt);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new ActivationMutationResult(
            StoreMutationOutcome.Applied,
            request.ProposedRecord);
    }

    public async Task<CorpusActivationRecord?> ReadActiveActivationAsync(
        CorpusId corpusId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(corpusId);
        await using var context = options.CreateControlContext();
        return await ReadActiveActivationAsync(
            context,
            corpusId,
            cancellationToken).ConfigureAwait(false);
    }

    private static async Task AddCatalogueSnapshotAsync(
        ControlPlaneDbContext context,
        CatalogueCommitRequest request,
        CancellationToken cancellationToken)
    {
        var corpusId = request.Snapshot.CorpusId.Value;
        var existingCategories = await context.DatabaseCategories
            .Where(row => row.CorpusId == corpusId)
            .ToDictionaryAsync(row => row.CategoryId, StringComparer.Ordinal, cancellationToken)
            .ConfigureAwait(false);

        foreach (var category in request.Snapshot.DatabaseCategories)
        {
            if (existingCategories.TryGetValue(category.Id.Value, out var existing))
            {
                if (!string.Equals(
                        existing.DisplayName,
                        category.DisplayName,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "An immutable database category changed its display name.");
                }

                continue;
            }

            context.DatabaseCategories.Add(new DatabaseCategoryRow
            {
                CorpusId = corpusId,
                CategoryId = category.Id.Value,
                DisplayName = category.DisplayName,
            });
        }

        foreach (var product in request.Snapshot.DatabaseProducts)
        {
            var existing = await context.DatabaseProductRevisions
                .SingleOrDefaultAsync(
                    row => row.CorpusId == corpusId &&
                        row.ProductId == product.Id.Value &&
                        row.ProductRevision == product.Revision.Value,
                    cancellationToken).ConfigureAwait(false);

            if (existing is null)
            {
                context.DatabaseProductRevisions.Add(new DatabaseProductRevisionRow
                {
                    CorpusId = corpusId,
                    ProductId = product.Id.Value,
                    ProductRevision = product.Revision.Value,
                    DisplayName = product.DisplayName,
                    Status = product.Status.ToString(),
                });
                context.DatabaseProductCategories.AddRange(
                    product.CategoryIds.Select(categoryId =>
                        new DatabaseProductCategoryRow
                        {
                            CorpusId = corpusId,
                            ProductId = product.Id.Value,
                            ProductRevision = product.Revision.Value,
                            CategoryId = categoryId.Value,
                        }));
            }
            else if (!string.Equals(
                    existing.DisplayName,
                    product.DisplayName,
                    StringComparison.Ordinal) ||
                !string.Equals(existing.Status, product.Status.ToString(), StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "An immutable database-product revision changed after persistence.");
            }
        }

        foreach (var document in request.Snapshot.DocumentVersions)
        {
            await AddOrValidateContentObjectAsync(
                context,
                document.ContentObjectId.Value,
                document.ByteLength,
                request.CommittedAt,
                cancellationToken).ConfigureAwait(false);
            var existing = await context.DocumentVersions.SingleOrDefaultAsync(
                row => row.CorpusId == corpusId &&
                    row.DocumentId == document.Id.Value &&
                    row.DocumentVersion == document.Version.Value,
                cancellationToken).ConfigureAwait(false);

            if (existing is null)
            {
                context.DocumentVersions.Add(new DocumentVersionRow
                {
                    CorpusId = corpusId,
                    DocumentId = document.Id.Value,
                    DocumentVersion = document.Version.Value,
                    ProductId = document.DatabaseProductId.Value,
                    ProductRevision = document.DatabaseProductRevision.Value,
                    DocumentFormat = document.Format.ToString(),
                    ContentLanguage = document.ContentLanguage.ToCanonicalTag(),
                    ContentSha256 = document.ContentObjectId.Value,
                    ByteLength = document.ByteLength,
                    MediaType = document.MediaType,
                    SourceAdapterId = document.SourceAdapterId.Value,
                    SourceTrustClass = document.SourceTrustClass.ToString(),
                    OfficialRegistrationId = document.OfficialSourceRegistrationId?.Value,
                    OfficialSnapshotId = document.OfficialSnapshotId?.Value,
                });
            }
            else if (!DocumentMatches(existing, document))
            {
                throw new InvalidOperationException(
                    "An immutable document version changed after persistence.");
            }
        }

        context.CatalogueRevisions.Add(new CatalogueRevisionRow
        {
            CorpusId = corpusId,
            CatalogueRevision = request.Snapshot.Revision.Value,
            CreatedAtUtc = ControlPlaneMapping.FormatUtc(request.CommittedAt),
            OperationId = request.OperationId.Value,
        });
        context.CatalogueRevisionProducts.AddRange(
            request.Snapshot.DatabaseProducts.Select(product =>
                new CatalogueRevisionProductRow
                {
                    CorpusId = corpusId,
                    CatalogueRevision = request.Snapshot.Revision.Value,
                    ProductId = product.Id.Value,
                    ProductRevision = product.Revision.Value,
                }));
        context.CatalogueRevisionDocuments.AddRange(
            request.Snapshot.DocumentVersions.Select(document =>
                new CatalogueRevisionDocumentRow
                {
                    CorpusId = corpusId,
                    CatalogueRevision = request.Snapshot.Revision.Value,
                    DocumentId = document.Id.Value,
                    DocumentVersion = document.Version.Value,
                    Status = document.Status.ToString(),
                }));
    }

    private static async Task AddOrValidateContentObjectAsync(
        ControlPlaneDbContext context,
        string contentSha256,
        long byteLength,
        DateTimeOffset registeredAt,
        CancellationToken cancellationToken)
    {
        var existing = await context.ContentObjects.SingleOrDefaultAsync(
            row => row.ContentSha256 == contentSha256,
            cancellationToken).ConfigureAwait(false);

        if (existing is null)
        {
            context.ContentObjects.Add(new ContentObjectRow
            {
                ContentSha256 = contentSha256,
                ByteLength = byteLength,
                RegisteredAtUtc = ControlPlaneMapping.FormatUtc(registeredAt),
            });
        }
        else if (existing.ByteLength != byteLength)
        {
            throw new InvalidOperationException(
                "A content SHA-256 identity was reused with another byte length.");
        }
    }

    private static bool DocumentMatches(
        DocumentVersionRow row,
        DocumentVersion document) =>
        row.ProductId == document.DatabaseProductId.Value &&
        row.ProductRevision == document.DatabaseProductRevision.Value &&
        row.DocumentFormat == document.Format.ToString() &&
        row.ContentLanguage == document.ContentLanguage.ToCanonicalTag() &&
        row.ContentSha256 == document.ContentObjectId.Value &&
        row.ByteLength == document.ByteLength &&
        row.MediaType == document.MediaType &&
        row.SourceAdapterId == document.SourceAdapterId.Value &&
        row.SourceTrustClass == document.SourceTrustClass.ToString() &&
        row.OfficialRegistrationId == document.OfficialSourceRegistrationId?.Value &&
        row.OfficialSnapshotId == document.OfficialSnapshotId?.Value;

    private static async Task<CorpusActivationRecord?> ReadActiveActivationAsync(
        ControlPlaneDbContext context,
        CorpusId corpusId,
        CancellationToken cancellationToken)
    {
        var head = await context.ActivationHeads.AsNoTracking().SingleOrDefaultAsync(
            row => row.CorpusId == corpusId.Value,
            cancellationToken).ConfigureAwait(false);

        if (head is null)
        {
            return null;
        }

        var record = await context.ActivationRecords.AsNoTracking().SingleAsync(
            row => row.CorpusId == corpusId.Value &&
                row.RecordRevision == head.RecordRevision,
            cancellationToken).ConfigureAwait(false);
        var bindings = await context.ActivationBindings.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value &&
                row.RecordRevision == head.RecordRevision)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        return ControlPlaneMapping.ToDomain(record, bindings);
    }

    private static bool MutationKindMatches(
        ActivationCompareExchangeRequest request,
        CorpusActivationRecord? currentRecord)
    {
        if (currentRecord is null)
        {
            return request.MutationKind == ActivationMutationKind.Initial &&
                request.ExpectedCurrentRevision == 0;
        }

        return request.MutationKind switch
        {
            ActivationMutationKind.Initial => false,
            ActivationMutationKind.ObservationRebind =>
                request.ProposedRecord.IndexGenerationId == currentRecord.IndexGenerationId &&
                request.ProposedRecord.CatalogueRevision == currentRecord.CatalogueRevision &&
                request.ProposedRecord.GenerationActivatedAt == currentRecord.GenerationActivatedAt,
            ActivationMutationKind.Replacement or ActivationMutationKind.Rollback =>
                request.ProposedRecord.IndexGenerationId != currentRecord.IndexGenerationId,
            _ => false,
        };
    }

    private static bool RetentionAllowsMutation(
        ActivationCompareExchangeRequest request,
        CorpusActivationRecord? currentRecord,
        IReadOnlyCollection<GenerationRetentionRow> rows,
        bool targetPreviouslyActivated)
    {
        if (currentRecord is null ||
            request.ProposedRecord.IndexGenerationId == currentRecord.IndexGenerationId)
        {
            return request.MutationKind != ActivationMutationKind.Rollback;
        }

        var target = rows.SingleOrDefault(row =>
            row.IndexGenerationId == request.ProposedRecord.IndexGenerationId.Value);

        if (request.MutationKind == ActivationMutationKind.Rollback)
        {
            return target is not null &&
                string.Equals(target.ProtectionRole, "Previous", StringComparison.Ordinal) &&
                ControlPlaneMapping.ParseUtc(target.RetainUntilUtc) >= request.EvaluatedAt;
        }

        return target is null && !targetPreviouslyActivated;
    }

    private static async Task ApplyRetentionAsync(
        ControlPlaneDbContext context,
        ActivationCompareExchangeRequest request,
        CorpusActivationRecord? currentRecord,
        IReadOnlyCollection<GenerationRetentionRow> rows,
        CancellationToken cancellationToken)
    {
        var activeUntil = DateTimeOffset.MaxValue;
        var now = request.EvaluatedAt;

        if (currentRecord is null)
        {
            context.GenerationRetentions.Add(new GenerationRetentionRow
            {
                CorpusId = request.ProposedRecord.CorpusId.Value,
                IndexGenerationId = request.ProposedRecord.IndexGenerationId.Value,
                ProtectionRole = "Active",
                RetainUntilUtc = ControlPlaneMapping.FormatUtc(activeUntil),
                RecordedAtUtc = ControlPlaneMapping.FormatUtc(now),
                OperationId = request.OperationId.Value,
            });
            return;
        }

        if (request.ProposedRecord.IndexGenerationId == currentRecord.IndexGenerationId)
        {
            return;
        }

        var currentActive = rows.Single(row =>
            row.IndexGenerationId == currentRecord.IndexGenerationId.Value &&
            row.ProtectionRole == "Active");
        var currentPrevious = rows.SingleOrDefault(row =>
            row.ProtectionRole == "Previous");
        var target = rows.SingleOrDefault(row =>
            row.IndexGenerationId == request.ProposedRecord.IndexGenerationId.Value);

        currentActive.ProtectionRole = "Hold";

        if (currentPrevious is not null)
        {
            currentPrevious.ProtectionRole = "Hold";
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        currentActive.ProtectionRole = "Previous";
        var minimumUntil = now + request.PreviousGenerationRetention;
        currentActive.RetainUntilUtc = ControlPlaneMapping.FormatUtc(minimumUntil);
        currentActive.RecordedAtUtc = ControlPlaneMapping.FormatUtc(now);
        currentActive.OperationId = request.OperationId.Value;

        if (target is null)
        {
            context.GenerationRetentions.Add(new GenerationRetentionRow
            {
                CorpusId = request.ProposedRecord.CorpusId.Value,
                IndexGenerationId = request.ProposedRecord.IndexGenerationId.Value,
                ProtectionRole = "Active",
                RetainUntilUtc = ControlPlaneMapping.FormatUtc(activeUntil),
                RecordedAtUtc = ControlPlaneMapping.FormatUtc(now),
                OperationId = request.OperationId.Value,
            });
        }
        else
        {
            target.ProtectionRole = "Active";
            target.RetainUntilUtc = ControlPlaneMapping.FormatUtc(activeUntil);
            target.RecordedAtUtc = ControlPlaneMapping.FormatUtc(now);
            target.OperationId = request.OperationId.Value;
        }
    }

    private static async Task<StoreMutationResult?> GetIdempotentResultAsync(
        ControlPlaneDbContext context,
        OperationId operationId,
        string expectedKind,
        CancellationToken cancellationToken)
    {
        var existing = await context.AdminOperations.AsNoTracking()
            .SingleOrDefaultAsync(
                row => row.OperationId == operationId.Value,
                cancellationToken).ConfigureAwait(false);

        if (existing is null)
        {
            return null;
        }

        EnsureOperationIdentity(existing, expectedKind);
        return new StoreMutationResult(
            StoreMutationOutcome.AlreadyApplied,
            existing.ResultRevision ?? 0);
    }

    private static async Task<bool> HasBlockingLeaseAsync(
        ControlPlaneDbContext context,
        CorpusId corpusId,
        OperationId operationId,
        DateTimeOffset evaluatedAt,
        CancellationToken cancellationToken)
    {
        var leases = await context.RecoveryLeases.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value &&
                row.OperationId != operationId.Value)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        return leases.Any(row =>
            ControlPlaneMapping.ParseUtc(row.ExpiresAtUtc) > evaluatedAt);
    }

    private static void EnsureOperationIdentity(
        AdminOperationRow operation,
        string expectedKind)
    {
        if (!string.Equals(
                operation.OperationKind,
                expectedKind,
                StringComparison.Ordinal) ||
            !string.Equals(operation.Status, "Applied", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "An operation identity was reused for another or incomplete mutation.");
        }
    }

    private static void AddOperation(
        ControlPlaneDbContext context,
        OperationId operationId,
        CorpusId corpusId,
        string operationKind,
        long expectedRevision,
        DateTimeOffset requestedAt)
    {
        context.AdminOperations.Add(new AdminOperationRow
        {
            OperationId = operationId.Value,
            CorpusId = corpusId.Value,
            OperationKind = operationKind,
            Status = "InProgress",
            ExpectedRevision = expectedRevision,
            ResultRevision = null,
            RequestedAtUtc = ControlPlaneMapping.FormatUtc(requestedAt),
            CompletedAtUtc = null,
        });
    }

    private static void CompleteOperation(
        ControlPlaneDbContext context,
        OperationId operationId,
        CorpusId corpusId,
        string eventType,
        long resultRevision,
        DateTimeOffset occurredAt)
    {
        var operation = context.AdminOperations.Local.Single(
            row => row.OperationId == operationId.Value);
        operation.Status = "Applied";
        operation.ResultRevision = resultRevision;
        operation.CompletedAtUtc = ControlPlaneMapping.FormatUtc(occurredAt);
        var auditMaterial = string.Join(
            '\n',
            corpusId.Value,
            operationId.Value,
            eventType,
            resultRevision.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ControlPlaneMapping.FormatUtc(occurredAt));
        var detailsDigest = Sha256(auditMaterial);
        var eventId = $"audit-{Sha256($"{operationId.Value}\n{eventType}")}";
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

    private static void EnsureExpectedRevision(long expected, long proposed)
    {
        if (expected < 0 || proposed != expected + 1)
        {
            throw new ArgumentException(
                "A proposed revision must immediately follow its non-negative expected revision.");
        }
    }

    private static string Sha256(string value) =>
        Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();
}
