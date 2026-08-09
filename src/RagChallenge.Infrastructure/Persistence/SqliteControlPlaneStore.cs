// Purpose: Implements control.db as the sole transactional authority for catalogue, observation, manifest, activation, audit, and retention state with expected-revision CAS.
using System.Security.Cryptography;
using System.Text;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using RagChallenge.Application.Administration;
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.Infrastructure.Persistence;

public sealed class SqliteControlPlaneStore(SqliteStoreOptions options)
    : IControlPlaneStore, IDocumentRenderManifestStore
{
    public static readonly TimeSpan MinimumPreviousGenerationRetention =
        TimeSpan.FromDays(14);

    private readonly SqliteStoreOptions options =
        options ?? throw new ArgumentNullException(nameof(options));

    public async Task<RenderManifestCommitResult> CommitAsync(
        RenderManifestCommitRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.CorpusId);
        ArgumentNullException.ThrowIfNull(request.Manifest);

        await using var context = options.CreateControlContext();
        await using var transaction = await BeginImmediateAsync(
            context,
            cancellationToken).ConfigureAwait(false);
        var manifest = request.Manifest;
        var existing = await context.DocumentRenderManifests.AsNoTracking()
            .FirstOrDefaultAsync(
                row => row.CorpusId == request.CorpusId.Value &&
                    row.DocumentId == manifest.DocumentId.Value &&
                    row.DocumentVersion == manifest.DocumentVersion.Value &&
                    row.SourceContentSha256 == manifest.SourceContentObjectId.Value &&
                    row.RenderProfileId == manifest.RenderProfileId.Value &&
                    row.RendererDescriptor == manifest.RendererDescriptor.Value,
                cancellationToken).ConfigureAwait(false);

        if (existing is not null)
        {
            var persisted = await ReadRenderManifestAsync(
                context,
                existing,
                cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return new RenderManifestCommitResult(
                persisted.RenderManifestId == manifest.RenderManifestId &&
                    persisted.ManifestSha256 == manifest.ManifestSha256
                    ? StoreMutationOutcome.AlreadyApplied
                    : StoreMutationOutcome.RevisionConflict,
                persisted);
        }

        var sourceExists = await context.DocumentVersions.AsNoTracking().AnyAsync(
            row => row.CorpusId == request.CorpusId.Value &&
                row.DocumentId == manifest.DocumentId.Value &&
                row.DocumentVersion == manifest.DocumentVersion.Value &&
                row.ContentSha256 == manifest.SourceContentObjectId.Value &&
                row.DocumentFormat == DocumentFormat.Pdf.ToString(),
            cancellationToken).ConfigureAwait(false);

        if (!sourceExists)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return new RenderManifestCommitResult(StoreMutationOutcome.NotFound, null);
        }

        foreach (var page in manifest.OrderedPageImages)
        {
            await AddOrValidateContentObjectAsync(
                context,
                page.ImageContentObjectId.Value,
                page.ByteLength,
                manifest.GeneratedAt,
                cancellationToken).ConfigureAwait(false);
        }

        context.DocumentRenderManifests.Add(new DocumentRenderManifestRow
        {
            RenderManifestId = manifest.RenderManifestId.Value,
            ManifestSha256 = manifest.ManifestSha256.Value,
            SchemaVersion = manifest.SchemaVersion,
            CorpusId = request.CorpusId.Value,
            DocumentId = manifest.DocumentId.Value,
            DocumentVersion = manifest.DocumentVersion.Value,
            SourceContentSha256 = manifest.SourceContentObjectId.Value,
            SourcePageCount = manifest.SourcePageCount,
            RenderProfileId = manifest.RenderProfileId.Value,
            RendererDescriptor = manifest.RendererDescriptor.Value,
            GeneratedAtUtc = ControlPlaneMapping.FormatUtc(manifest.GeneratedAt),
        });
        context.DocumentPageImages.AddRange(manifest.OrderedPageImages.Select(page =>
            new DocumentPageImageRow
            {
                RenderManifestId = manifest.RenderManifestId.Value,
                PageNumber = page.PageNumber,
                CorpusId = request.CorpusId.Value,
                DocumentId = page.DocumentId.Value,
                DocumentVersion = page.DocumentVersion.Value,
                SourceContentSha256 = page.SourceContentObjectId.Value,
                RenderProfileId = page.RenderProfileId.Value,
                RendererDescriptor = page.RendererDescriptor.Value,
                ImageContentSha256 = page.ImageContentObjectId.Value,
                ImageSha256 = page.ImageSha256.Value,
                ByteLength = page.ByteLength,
                MediaType = page.MediaType,
                WidthPixels = page.WidthPixels,
                HeightPixels = page.HeightPixels,
            }));
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new RenderManifestCommitResult(StoreMutationOutcome.Applied, manifest);
    }

    public async Task<DocumentRenderManifest?> ReadAsync(
        CorpusId corpusId,
        RenderManifestId renderManifestId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(corpusId);
        ArgumentNullException.ThrowIfNull(renderManifestId);
        await using var context = options.CreateControlContext();
        var row = await context.DocumentRenderManifests.AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.CorpusId == corpusId.Value &&
                    item.RenderManifestId == renderManifestId.Value,
                cancellationToken).ConfigureAwait(false);
        return row is null
            ? null
            : await ReadRenderManifestAsync(context, row, cancellationToken)
                .ConfigureAwait(false);
    }

    public async Task<StoreMutationResult> CommitCatalogueAsync(
        CatalogueCommitRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.OperationId);
        ArgumentNullException.ThrowIfNull(request.Snapshot);
        EnsureExpectedRevision(request.ExpectedCurrentRevision, request.Snapshot.Revision.Value);
        EnsureRecoverableCatalogueCategoryProjection(request.Snapshot);
        ControlPlaneMapping.EnsureUtc(request.CommittedAt, nameof(request));

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
            var replay = await ReadExactCatalogueReplayAsync(
                context,
                request,
                existingOperation,
                cancellationToken).ConfigureAwait(false);
            EnsureJournalCompletionMatchesMutation(
                request.JournalCompletion,
                request.OperationId,
                request.Snapshot.Revision.Value);
            await SqliteAdministrationCommandJournal.VerifyCompletionAsync(
                context,
                request.JournalCompletion,
                cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return replay;
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
            request.CommittedAt,
            request.AuditDetailsDigest);
        EnsureJournalCompletionMatchesMutation(
            request.JournalCompletion,
            request.OperationId,
            request.Snapshot.Revision.Value);
        await SqliteAdministrationCommandJournal.ApplyCompletionAsync(
            context,
            request.JournalCompletion,
            JournalCompletedAt(request.CommittedAt),
            cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new StoreMutationResult(
            StoreMutationOutcome.Applied,
            request.Snapshot.Revision.Value);
    }

    public async Task<CatalogueSnapshot?> ReadCurrentCatalogueAsync(
        CorpusId corpusId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(corpusId);
        await using var context = options.CreateControlContext();
        var head = await context.CatalogueHeads.AsNoTracking().SingleOrDefaultAsync(
            row => row.CorpusId == corpusId.Value,
            cancellationToken).ConfigureAwait(false);

        if (head is null)
        {
            return null;
        }

        return await ReadCatalogueSnapshotAsync(
            context,
            corpusId,
            head.CatalogueRevision,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<StoreMutationResult> RegisterOfficialSourceAsync(
        OfficialSourceRegistrationCommitRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.OperationId);
        ArgumentNullException.ThrowIfNull(request.CorpusId);
        ArgumentNullException.ThrowIfNull(request.Registration);
        ControlPlaneMapping.EnsureUtc(request.CommittedAt, nameof(request));
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
            var replay = await ReadExactOfficialRegistrationReplayAsync(
                context,
                request,
                existingOperation,
                cancellationToken).ConfigureAwait(false);
            EnsureJournalCompletionMatchesMutation(
                request.JournalCompletion,
                request.OperationId,
                request.Registration.Revision.Value);
            await SqliteAdministrationCommandJournal.VerifyCompletionAsync(
                context,
                request.JournalCompletion,
                cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return replay;
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

        var priorRevision = await context.OfficialSourceRegistrations
            .Where(row => row.CorpusId == request.CorpusId.Value &&
                row.RegistrationId == request.Registration.Id.Value)
            .Select(row => (long?)row.RegistrationRevision)
            .MaxAsync(cancellationToken).ConfigureAwait(false) ?? 0;
        var documentExists = await context.DocumentVersions.AnyAsync(
            row => row.CorpusId == request.CorpusId.Value &&
                row.DocumentId == request.Registration.DocumentId.Value &&
                row.ProductId == request.Registration.DatabaseProductId.Value,
            cancellationToken).ConfigureAwait(false);

        if (!documentExists)
        {
            return new StoreMutationResult(StoreMutationOutcome.NotFound, priorRevision);
        }

        if (request.Registration.Revision.Value != priorRevision + 1)
        {
            return new StoreMutationResult(
                StoreMutationOutcome.RevisionConflict,
                priorRevision);
        }

        AddOperation(
            context,
            request.OperationId,
            request.CorpusId,
            "OfficialSourceRegistration",
            priorRevision,
            request.CommittedAt);
        AddOfficialSourceRegistration(context, request.CorpusId, request.Registration);
        CompleteOperation(
            context,
            request.OperationId,
            request.CorpusId,
            "OfficialSourceRegistered",
            request.Registration.Revision.Value,
            request.CommittedAt,
            request.AuditDetailsDigest);
        EnsureJournalCompletionMatchesMutation(
            request.JournalCompletion,
            request.OperationId,
            request.Registration.Revision.Value);
        await SqliteAdministrationCommandJournal.ApplyCompletionAsync(
            context,
            request.JournalCompletion,
            JournalCompletedAt(request.CommittedAt),
            cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new StoreMutationResult(
            StoreMutationOutcome.Applied,
            request.Registration.Revision.Value);
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
        var existingOperation = await context.AdminOperations.AsNoTracking()
            .SingleOrDefaultAsync(
                row => row.OperationId == request.OperationId.Value,
                cancellationToken).ConfigureAwait(false);

        if (existingOperation is not null)
        {
            var replay = await ReadExactOfficialSourceReplayAsync(
                context,
                request,
                existingOperation,
                cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return replay;
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

        var latestRegistration = await context.OfficialSourceRegistrations.AsNoTracking()
            .Where(row => row.CorpusId == request.CorpusId.Value &&
                row.RegistrationId == request.Registration.Id.Value)
            .OrderByDescending(row => row.RegistrationRevision)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        var priorRevision = latestRegistration?.RegistrationRevision ?? 0;
        var registrationAlreadyCommitted =
            priorRevision == request.Registration.Revision.Value;

        if (registrationAlreadyCommitted &&
            !OfficialSourceRegistrationMatches(
                latestRegistration!,
                request.Registration))
        {
            throw new InvalidOperationException(
                "An immutable official-source registration changed after persistence.");
        }

        if (!registrationAlreadyCommitted &&
            request.Registration.Revision.Value != priorRevision + 1)
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
        if (!registrationAlreadyCommitted)
        {
            AddOfficialSourceRegistration(context, request.CorpusId, request.Registration);
        }
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
            request.CommittedAt,
            request.AuditDetailsDigest);
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
        var existingOperation = await context.AdminOperations.AsNoTracking()
            .SingleOrDefaultAsync(
                row => row.OperationId == request.OperationId.Value,
                cancellationToken).ConfigureAwait(false);

        if (existingOperation is not null)
        {
            var replay = await ReadExactObservationReplayAsync(
                context,
                request,
                existingOperation,
                checked((long)maxAgeSeconds),
                cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return replay;
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
            request.CommittedAt,
            request.AuditDetailsDigest);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new StoreMutationResult(
            StoreMutationOutcome.Applied,
            request.Observation.JournalRevision.Value);
    }

    public async Task<ObservationRebindMutationResult> AppendObservationWithActivationRebindAsync(
        ObservationRebindCommitRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.OperationId);
        ArgumentNullException.ThrowIfNull(request.CorpusId);
        ArgumentNullException.ThrowIfNull(request.DocumentId);
        ArgumentNullException.ThrowIfNull(request.DocumentVersion);
        ArgumentNullException.ThrowIfNull(request.Observation);
        ControlPlaneMapping.EnsureUtc(request.CommittedAt, nameof(request));
        EnsureExpectedRevision(
            request.ExpectedJournalRevision,
            request.Observation.JournalRevision.Value);

        if (request.ExpectedActivationRevision < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                request.ExpectedActivationRevision,
                "An expected activation revision cannot be negative.");
        }

        var maxAgeSeconds = GetWholeMaxAgeSeconds(request.Observation, nameof(request));

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
            var replay = await ReadExactObservationRebindReplayAsync(
                context,
                request,
                existingOperation,
                cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return replay;
        }

        if (await HasBlockingLeaseAsync(
                context,
                request.CorpusId,
                request.OperationId,
                request.CommittedAt,
                cancellationToken).ConfigureAwait(false))
        {
            var active = await ReadActiveActivationAsync(
                context,
                request.CorpusId,
                cancellationToken).ConfigureAwait(false);
            var journalRevision = await ReadObservationJournalRevisionAsync(
                context,
                request.CorpusId,
                cancellationToken).ConfigureAwait(false);
            return new ObservationRebindMutationResult(
                StoreMutationOutcome.RetentionConflict,
                journalRevision,
                active,
                activationRecordRebound: false);
        }

        var journalHead = await context.ObservationJournalHeads.SingleOrDefaultAsync(
            row => row.CorpusId == request.CorpusId.Value,
            cancellationToken).ConfigureAwait(false);
        var currentJournalRevision = journalHead?.JournalRevision ?? 0;

        if (currentJournalRevision != request.ExpectedJournalRevision)
        {
            var active = await ReadActiveActivationAsync(
                context,
                request.CorpusId,
                cancellationToken).ConfigureAwait(false);
            return new ObservationRebindMutationResult(
                StoreMutationOutcome.RevisionConflict,
                currentJournalRevision,
                active,
                activationRecordRebound: false);
        }

        var activationHead = await context.ActivationHeads.AsNoTracking()
            .SingleOrDefaultAsync(
                row => row.CorpusId == request.CorpusId.Value,
                cancellationToken).ConfigureAwait(false);
        var currentActivationRevision = activationHead?.RecordRevision ?? 0;

        if (currentActivationRevision != request.ExpectedActivationRevision)
        {
            var active = await ReadActiveActivationAsync(
                context,
                request.CorpusId,
                cancellationToken).ConfigureAwait(false);
            return new ObservationRebindMutationResult(
                StoreMutationOutcome.RevisionConflict,
                currentJournalRevision,
                active,
                activationRecordRebound: false);
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
            var active = await ReadActiveActivationAsync(
                context,
                request.CorpusId,
                cancellationToken).ConfigureAwait(false);
            return new ObservationRebindMutationResult(
                StoreMutationOutcome.NotFound,
                currentJournalRevision,
                active,
                activationRecordRebound: false);
        }

        if (await context.SourceObservations.AsNoTracking().AnyAsync(
                row => row.CorpusId == request.CorpusId.Value &&
                    row.ObservationId == request.Observation.Id.Value,
                cancellationToken).ConfigureAwait(false))
        {
            var active = await ReadActiveActivationAsync(
                context,
                request.CorpusId,
                cancellationToken).ConfigureAwait(false);
            return new ObservationRebindMutationResult(
                StoreMutationOutcome.ValidationFailed,
                currentJournalRevision,
                active,
                activationRecordRebound: false,
                new[] { ActivationValidationFailure.ObservationBindingMismatch });
        }

        var currentRecord = await ReadActiveActivationAsync(
            context,
            request.CorpusId,
            cancellationToken).ConfigureAwait(false);
        var targetBindings = currentRecord?.DocumentBindings
            .Where(binding =>
                binding.DocumentId == request.DocumentId &&
                binding.DocumentVersion == request.DocumentVersion)
            .ToArray() ?? [];

        if (targetBindings.Length > 1)
        {
            return new ObservationRebindMutationResult(
                StoreMutationOutcome.ValidationFailed,
                currentJournalRevision,
                currentRecord,
                activationRecordRebound: false,
                new[] { ActivationValidationFailure.DuplicateActiveDocumentProjection });
        }

        CorpusActivationRecord? proposedRecord = null;

        if (targetBindings.Length == 1)
        {
            var target = targetBindings[0];
            var targetFailures = new List<ActivationValidationFailure>();

            if (target.SourceTrustClass != SourceTrustClass.OfficialExternal)
            {
                targetFailures.Add(ActivationValidationFailure.ObservationBindingMismatch);
            }

            if (target.OfficialSourceRegistrationId != request.Observation.RegistrationId)
            {
                targetFailures.Add(ActivationValidationFailure.ObservationRegistrationMismatch);
            }

            if (target.OfficialSnapshotId != request.Observation.SnapshotId)
            {
                targetFailures.Add(ActivationValidationFailure.ObservationSnapshotMismatch);
            }

            if (targetFailures.Count > 0)
            {
                return new ObservationRebindMutationResult(
                    StoreMutationOutcome.ValidationFailed,
                    currentJournalRevision,
                    currentRecord,
                    activationRecordRebound: false,
                    targetFailures);
            }

            proposedRecord = ActivationRecordFactory.RebindObservation(
                currentRecord!,
                request.DocumentId,
                request.DocumentVersion,
                request.Observation,
                request.CommittedAt);
            var validation = await ValidateObservationRebindAsync(
                context,
                currentRecord!,
                proposedRecord,
                request.Observation,
                cancellationToken).ConfigureAwait(false);

            if (!validation.IsValid)
            {
                return new ObservationRebindMutationResult(
                    StoreMutationOutcome.ValidationFailed,
                    currentJournalRevision,
                    currentRecord,
                    activationRecordRebound: false,
                    validation.Failures);
            }
        }

        AddOperation(
            context,
            request.OperationId,
            request.CorpusId,
            "ObservationRebind",
            request.ExpectedActivationRevision,
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
            MaxAgeSeconds = maxAgeSeconds,
            OperationId = request.OperationId.Value,
        });

        if (journalHead is null)
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
            journalHead.JournalRevision = request.Observation.JournalRevision.Value;
            journalHead.RowRevision = checked(journalHead.RowRevision + 1);
        }

        var activationRecordRebound = proposedRecord is not null;
        var eventType = activationRecordRebound
            ? "ObservationRebound"
            : "ObservationAppended";
        var resultRevision = proposedRecord?.RecordRevision.Value ??
            request.Observation.JournalRevision.Value;

        if (proposedRecord is not null)
        {
            context.ActivationRecords.Add(new ActivationRecordRow
            {
                CorpusId = proposedRecord.CorpusId.Value,
                RecordRevision = proposedRecord.RecordRevision.Value,
                PreviousRecordRevision = proposedRecord.PreviousRecordRevision?.Value,
                IndexGenerationId = proposedRecord.IndexGenerationId.Value,
                CatalogueRevision = proposedRecord.CatalogueRevision.Value,
                ActivationBindingSetDigest = proposedRecord.ActivationBindingSetDigest.Value,
                GenerationActivatedAtUtc = ControlPlaneMapping.FormatUtc(
                    proposedRecord.GenerationActivatedAt),
                RecordUpdatedAtUtc = ControlPlaneMapping.FormatUtc(
                    proposedRecord.RecordUpdatedAt),
                MutationKind = ActivationMutationKind.ObservationRebind.ToString(),
                OperationId = request.OperationId.Value,
            });
            context.ActivationBindings.AddRange(
                proposedRecord.DocumentBindings.Select(binding =>
                    ControlPlaneMapping.ToActivationBindingRow(
                        proposedRecord.CorpusId,
                        proposedRecord.RecordRevision,
                        binding)));
            AddActivationEvidenceRows(context, proposedRecord);
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (proposedRecord is not null)
        {
            var affected = await context.Database.ExecuteSqlInterpolatedAsync(
                $"""
                UPDATE activation_heads
                SET record_revision = {proposedRecord.RecordRevision.Value},
                    row_revision = row_revision + 1
                WHERE corpus_id = {request.CorpusId.Value}
                  AND record_revision = {request.ExpectedActivationRevision};
                """,
                cancellationToken).ConfigureAwait(false);

            if (affected != 1)
            {
                throw new InvalidOperationException(
                    "The activation head changed during its immediate observation-rebind transaction.");
            }
        }

        CompleteOperation(
            context,
            request.OperationId,
            request.CorpusId,
            eventType,
            resultRevision,
            request.CommittedAt,
            request.AuditDetailsDigest);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new ObservationRebindMutationResult(
            StoreMutationOutcome.Applied,
            request.Observation.JournalRevision.Value,
            proposedRecord ?? currentRecord,
            activationRecordRebound);
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
            var replay = await ReadExactGenerationReplayAsync(
                context,
                request,
                existingOperation,
                cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return replay;
        }

        var vectorStore = new SqliteVectorIndexStore(options);

        if (!await vectorStore.MatchesFinalisedGenerationAsync(
                request.CandidateBuildId,
                request.Manifest,
                cancellationToken).ConfigureAwait(false))
        {
            return new StoreMutationResult(StoreMutationOutcome.ValidationFailed, 0);
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

        var expectedBindings = await ResolveExpectedGenerationBindingsAsync(
            context,
            request,
            cancellationToken).ConfigureAwait(false);

        if (expectedBindings is null ||
            BindingDigestCanonicalizer
                .CanonicaliseActiveDocumentSet(expectedBindings)
                .Digest != request.Manifest.ActiveDocumentSetDigest ||
            BindingDigestCanonicalizer
                .CanonicaliseSourceBindingSet(expectedBindings)
                .Digest != request.Manifest.SourceBindingSetDigest ||
            !await AreBindingContentsReopenableAsync(
                context,
                request.Manifest.CorpusId,
                expectedBindings,
                cancellationToken).ConfigureAwait(false))
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
            expectedBindings.Select(binding =>
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
            request.FinalisedAt,
            request.AuditDetailsDigest);
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
            var active = await ReadExactActivationReplayAsync(
                context,
                request,
                existingOperation,
                cancellationToken).ConfigureAwait(false);
            EnsureJournalCompletionMatchesMutation(
                request.JournalCompletion,
                request.OperationId,
                request.ProposedRecord.RecordRevision.Value);
            await SqliteAdministrationCommandJournal.VerifyCompletionAsync(
                context,
                request.JournalCompletion,
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
        var manifest = ControlPlaneMapping.ToDomain(manifestRow);

        if (!await vectorStore.MatchesFinalisedGenerationAsync(
                new CandidateBuildId(manifestRow.CandidateBuildId),
                manifest,
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
            manifest,
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

        if (!await IsActivationEvidenceValidAndReopenableAsync(
                context,
                request.ProposedRecord,
                cancellationToken).ConfigureAwait(false))
        {
            return new ActivationMutationResult(
                StoreMutationOutcome.ValidationFailed,
                currentRecord,
                [ActivationValidationFailure.ActivationEvidenceBindingMismatch]);
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
        AddActivationEvidenceRows(context, request.ProposedRecord);
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
            request.EvaluatedAt,
            request.AuditDetailsDigest);
        EnsureJournalCompletionMatchesMutation(
            request.JournalCompletion,
            request.OperationId,
            request.ProposedRecord.RecordRevision.Value);
        await SqliteAdministrationCommandJournal.ApplyCompletionAsync(
            context,
            request.JournalCompletion,
            JournalCompletedAt(request.EvaluatedAt),
            cancellationToken).ConfigureAwait(false);
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

    private async Task<(bool IsValid, IReadOnlyCollection<ActivationValidationFailure> Failures)>
        ValidateObservationRebindAsync(
            ControlPlaneDbContext context,
            CorpusActivationRecord currentRecord,
            CorpusActivationRecord proposedRecord,
            OfficialSourceObservation newObservation,
            CancellationToken cancellationToken)
    {
        var manifestRow = await context.GenerationManifests.AsNoTracking()
            .SingleOrDefaultAsync(
                row => row.CorpusId == proposedRecord.CorpusId.Value &&
                    row.IndexGenerationId == proposedRecord.IndexGenerationId.Value,
                cancellationToken).ConfigureAwait(false);

        if (manifestRow is null)
        {
            return (false, new[] { ActivationValidationFailure.GenerationMismatch });
        }

        var manifest = ControlPlaneMapping.ToDomain(manifestRow);
        var vectorStore = new SqliteVectorIndexStore(options);

        if (!await vectorStore.MatchesFinalisedGenerationAsync(
                new CandidateBuildId(manifestRow.CandidateBuildId),
                manifest,
                cancellationToken).ConfigureAwait(false) ||
            !await IsActivationEvidenceValidAndReopenableAsync(
                context,
                proposedRecord,
                cancellationToken).ConfigureAwait(false))
        {
            return (false, new[] { ActivationValidationFailure.GenerationMismatch });
        }

        var existingObservationIds = proposedRecord.DocumentBindings
            .Where(binding =>
                binding.SourceObservationId is not null &&
                binding.SourceObservationId != newObservation.Id)
            .Select(binding => binding.SourceObservationId!.Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var observationRows = await context.SourceObservations.AsNoTracking()
            .Where(row => row.CorpusId == proposedRecord.CorpusId.Value &&
                existingObservationIds.Contains(row.ObservationId))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var observations = observationRows.ToDictionary(
            row => new OfficialObservationId(row.ObservationId),
            ControlPlaneMapping.ToDomain);
        observations.Add(newObservation.Id, newObservation);
        var validation = ActivationRecordValidator.ValidateForCompareAndSwap(
            currentRecord,
            manifest,
            proposedRecord,
            manifest.IndexCompatibilityKey,
            observations,
            proposedRecord.RecordUpdatedAt);
        return (validation.IsValid, validation.Failures);
    }

    private static async Task<ObservationRebindMutationResult>
        ReadExactObservationRebindReplayAsync(
            ControlPlaneDbContext context,
            ObservationRebindCommitRequest request,
            AdminOperationRow existingOperation,
            CancellationToken cancellationToken)
    {
        EnsureOperationIdentity(existingOperation, "ObservationRebind");

        if (existingOperation.ExpectedRevision != request.ExpectedActivationRevision ||
            ControlPlaneMapping.ParseUtc(existingOperation.RequestedAtUtc) != request.CommittedAt ||
            existingOperation.ResultRevision is null)
        {
            throw new InvalidOperationException(
                "An observation-rebind operation identity was reused with different intent.");
        }

        var observationRow = await context.SourceObservations.AsNoTracking()
            .SingleOrDefaultAsync(
                row => row.CorpusId == request.CorpusId.Value &&
                    row.OperationId == request.OperationId.Value,
                cancellationToken).ConfigureAwait(false);

        if (observationRow is null ||
            request.Observation.JournalRevision.Value !=
                request.ExpectedJournalRevision + 1 ||
            !ObservationMatches(observationRow, request.Observation))
        {
            throw new InvalidOperationException(
                "An observation-rebind operation identity was reused with different observation data.");
        }

        var auditEvent = await context.AuditEvents.AsNoTracking().SingleOrDefaultAsync(
            row => row.CorpusId == request.CorpusId.Value &&
                row.OperationId == request.OperationId.Value,
            cancellationToken).ConfigureAwait(false);
        var rebound = string.Equals(
            auditEvent?.EventType,
            "ObservationRebound",
            StringComparison.Ordinal);

        if (auditEvent is null ||
            (!rebound && !string.Equals(
                auditEvent.EventType,
                "ObservationAppended",
                StringComparison.Ordinal)) ||
            ControlPlaneMapping.ParseUtc(auditEvent.OccurredAtUtc) != request.CommittedAt ||
            !string.Equals(
                auditEvent.DetailsDigest,
                BuildAuditDetailsDigest(
                    request.CorpusId,
                    request.OperationId,
                    auditEvent.EventType,
                    existingOperation.ResultRevision.Value,
                    request.CommittedAt,
                    request.AuditDetailsDigest),
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "An observation-rebind operation identity was reused with different audit data.");
        }

        if (rebound)
        {
            var historicalRecordMatches = await context.ActivationRecords.AsNoTracking()
                .AnyAsync(
                    row => row.CorpusId == request.CorpusId.Value &&
                        row.RecordRevision == existingOperation.ResultRevision.Value &&
                        row.OperationId == request.OperationId.Value &&
                        row.MutationKind == "ObservationRebind",
                    cancellationToken).ConfigureAwait(false);
            var historicalBindingMatches = await context.ActivationBindings.AsNoTracking()
                .AnyAsync(
                    row => row.CorpusId == request.CorpusId.Value &&
                        row.RecordRevision == existingOperation.ResultRevision.Value &&
                        row.DocumentId == request.DocumentId.Value &&
                        row.DocumentVersion == request.DocumentVersion.Value &&
                        row.SourceObservationId == request.Observation.Id.Value,
                    cancellationToken).ConfigureAwait(false);

            if (!historicalRecordMatches || !historicalBindingMatches)
            {
                throw new InvalidOperationException(
                    "The persisted observation-rebind evidence is incomplete.");
            }
        }
        else if (existingOperation.ResultRevision != request.Observation.JournalRevision.Value)
        {
            throw new InvalidOperationException(
                "The persisted observation-append revision is inconsistent.");
        }

        var active = await ReadActiveActivationAsync(
            context,
            request.CorpusId,
            cancellationToken).ConfigureAwait(false);
        var journalRevision = await ReadObservationJournalRevisionAsync(
            context,
            request.CorpusId,
            cancellationToken).ConfigureAwait(false);
        return new ObservationRebindMutationResult(
            StoreMutationOutcome.AlreadyApplied,
            journalRevision,
            active,
            rebound);
    }

    private static async Task<long> ReadObservationJournalRevisionAsync(
        ControlPlaneDbContext context,
        CorpusId corpusId,
        CancellationToken cancellationToken) =>
        await context.ObservationJournalHeads.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value)
            .Select(row => (long?)row.JournalRevision)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false) ?? 0;

    private static bool ObservationMatches(
        SourceObservationRow row,
        OfficialSourceObservation observation) =>
        string.Equals(row.ObservationId, observation.Id.Value, StringComparison.Ordinal) &&
        string.Equals(
            row.RegistrationId,
            observation.RegistrationId.Value,
            StringComparison.Ordinal) &&
        string.Equals(row.SnapshotId, observation.SnapshotId.Value, StringComparison.Ordinal) &&
        row.JournalRevision == observation.JournalRevision.Value &&
        string.Equals(row.State, observation.State.ToString(), StringComparison.Ordinal) &&
        ControlPlaneMapping.ParseUtc(row.RevalidatedAtUtc) == observation.RevalidatedAt &&
        row.MaxAgeSeconds == GetWholeMaxAgeSeconds(observation, nameof(observation));

    private static long GetWholeMaxAgeSeconds(
        OfficialSourceObservation observation,
        string parameterName)
    {
        var seconds = observation.MaxAge.TotalSeconds;

        if (seconds != Math.Truncate(seconds) || seconds > long.MaxValue)
        {
            throw new ArgumentException(
                "Observation maxAge must contain a whole number of seconds.",
                parameterName);
        }

        return checked((long)seconds);
    }

    private static void AddOfficialSourceRegistration(
        ControlPlaneDbContext context,
        CorpusId corpusId,
        OfficialSourceRegistration registration) =>
        context.OfficialSourceRegistrations.Add(new OfficialSourceRegistrationRow
        {
            CorpusId = corpusId.Value,
            RegistrationId = registration.Id.Value,
            RegistrationRevision = registration.Revision.Value,
            ProductId = registration.DatabaseProductId.Value,
            DocumentId = registration.DocumentId.Value,
            SourceAdapterId = registration.SourceAdapterId.Value,
            CanonicalHttpsUrl = registration.CanonicalHttpsUrl,
            Status = registration.Status.ToString(),
        });

    private static bool OfficialSourceRegistrationMatches(
        OfficialSourceRegistrationRow row,
        OfficialSourceRegistration registration) =>
        row.RegistrationRevision == registration.Revision.Value &&
        string.Equals(row.ProductId, registration.DatabaseProductId.Value, StringComparison.Ordinal) &&
        string.Equals(row.DocumentId, registration.DocumentId.Value, StringComparison.Ordinal) &&
        string.Equals(row.SourceAdapterId, registration.SourceAdapterId.Value, StringComparison.Ordinal) &&
        string.Equals(row.CanonicalHttpsUrl, registration.CanonicalHttpsUrl, StringComparison.Ordinal) &&
        string.Equals(row.Status, registration.Status.ToString(), StringComparison.Ordinal);

    private static DocumentContentLanguage ParseLanguage(string value)
    {
        try
        {
            return new DocumentContentLanguage(value);
        }
        catch (ArgumentException exception)
        {
            throw new InvalidDataException(
                "A persisted content language is not a valid BCP 47 value.",
                exception);
        }
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
            else
            {
                var persistedCategoryIds = await context.DatabaseProductCategories
                    .Where(row => row.CorpusId == corpusId &&
                        row.ProductId == product.Id.Value &&
                        row.ProductRevision == product.Revision.Value)
                    .Select(row => row.CategoryId)
                    .ToArrayAsync(cancellationToken).ConfigureAwait(false);
                var requestedCategoryIds = product.CategoryIds
                    .Select(categoryId => categoryId.Value)
                    .Order(StringComparer.Ordinal);

                if (!string.Equals(
                        existing.DisplayName,
                        product.DisplayName,
                        StringComparison.Ordinal) ||
                    !persistedCategoryIds.Order(StringComparer.Ordinal)
                        .SequenceEqual(requestedCategoryIds))
                {
                    throw new InvalidOperationException(
                        "An immutable database-product revision changed after persistence.");
                }
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
                    SourceDeclaredLanguage = document.SourceDeclaredLanguage?.ObservedTag,
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
                    Status = product.Status.ToString(),
                }));
        context.CatalogueRevisionDocuments.AddRange(
            request.Snapshot.DocumentVersions.Select(document =>
                new CatalogueRevisionDocumentRow
                {
                    CorpusId = corpusId,
                    CatalogueRevision = request.Snapshot.Revision.Value,
                    DocumentId = document.Id.Value,
                    DocumentVersion = document.Version.Value,
                    ProductId = document.DatabaseProductId.Value,
                    ProductRevision = document.DatabaseProductRevision.Value,
                    Status = document.Status.ToString(),
                }));
    }

    private static async Task<DocumentRenderManifest> ReadRenderManifestAsync(
        ControlPlaneDbContext context,
        DocumentRenderManifestRow row,
        CancellationToken cancellationToken)
    {
        if (row.SchemaVersion != DocumentRenderManifest.CurrentSchemaVersion)
        {
            throw new InvalidDataException(
                "A persisted render manifest uses an unsupported schema version.");
        }

        var pages = await context.DocumentPageImages.AsNoTracking()
            .Where(item => item.RenderManifestId == row.RenderManifestId)
            .OrderBy(item => item.PageNumber)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var documentId = new DocumentId(row.DocumentId);
        var documentVersion = new DocumentVersionNumber(row.DocumentVersion);
        var sourceContentObjectId = new ContentObjectId(row.SourceContentSha256);
        var profile = new RenderProfileId(row.RenderProfileId);
        var renderer = new RendererDescriptor(row.RendererDescriptor);
        var pageImages = pages.Select(page => new DocumentPageImage(
            documentId,
            documentVersion,
            sourceContentObjectId,
            page.PageNumber,
            profile,
            renderer,
            new ContentObjectId(page.ImageContentSha256),
            new ImageSha256(page.ImageSha256),
            page.ByteLength,
            page.MediaType,
            page.WidthPixels,
            page.HeightPixels));
        var manifest = DocumentRenderManifest.Rehydrate(
            documentId,
            documentVersion,
            sourceContentObjectId,
            row.SourcePageCount,
            profile,
            renderer,
            pageImages,
            new ManifestSha256(row.ManifestSha256),
            ControlPlaneMapping.ParseUtc(row.GeneratedAtUtc));

        if (manifest.RenderManifestId.Value != row.RenderManifestId)
        {
            throw new InvalidDataException(
                "A persisted render-manifest identifier does not match its canonical digest.");
        }

        return manifest;
    }

    private static async Task AddOrValidateContentObjectAsync(
        ControlPlaneDbContext context,
        string contentSha256,
        long byteLength,
        DateTimeOffset registeredAt,
        CancellationToken cancellationToken)
    {
        var existing = context.ContentObjects.Local.SingleOrDefault(
            row => row.ContentSha256 == contentSha256) ??
            await context.ContentObjects.SingleOrDefaultAsync(
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
        row.DocumentFormat == document.Format.ToString() &&
        row.ContentLanguage == document.ContentLanguage.ToCanonicalTag() &&
        row.SourceDeclaredLanguage == document.SourceDeclaredLanguage?.ObservedTag &&
        row.ContentSha256 == document.ContentObjectId.Value &&
        row.ByteLength == document.ByteLength &&
        row.MediaType == document.MediaType &&
        row.SourceAdapterId == document.SourceAdapterId.Value &&
        row.SourceTrustClass == document.SourceTrustClass.ToString() &&
        row.OfficialRegistrationId == document.OfficialSourceRegistrationId?.Value &&
        row.OfficialSnapshotId == document.OfficialSnapshotId?.Value;

    private async Task<bool> AreBindingContentsReopenableAsync(
        ControlPlaneDbContext context,
        CorpusId corpusId,
        IEnumerable<DocumentBinding> bindings,
        CancellationToken cancellationToken)
    {
        var requestedBindings = bindings
            .Select(binding => (binding.DocumentId.Value, binding.DocumentVersion.Value))
            .Distinct()
            .ToArray();
        var documentRows = await context.DocumentVersions.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value)
            .Select(row => new
            {
                row.DocumentId,
                row.DocumentVersion,
                row.ContentSha256,
                row.ByteLength,
                row.ContentLanguage,
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var contentByDocument = documentRows.ToDictionary(
            row => (row.DocumentId, row.DocumentVersion),
            row => (row.ContentSha256, row.ByteLength, row.ContentLanguage));
        var contentObjects = new HashSet<(string ContentSha256, long ByteLength)>();

        foreach (var binding in requestedBindings)
        {
            if (!contentByDocument.TryGetValue(binding, out var document))
            {
                return false;
            }

            _ = ParseLanguage(document.ContentLanguage);

            contentObjects.Add((document.ContentSha256, document.ByteLength));
        }

        var contentStore = new ImmutableContentStore(options);

        try
        {
            foreach (var contentObject in contentObjects)
            {
                var contentObjectId = new ContentObjectId(contentObject.ContentSha256);
                await using var content = await contentStore.OpenVerifiedAsync(
                    contentObjectId,
                    new ExpectedHashAndLength(
                        contentObjectId,
                        contentObject.ByteLength),
                    cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception exception) when (
            exception is IOException or InvalidDataException or UnauthorizedAccessException or
                ArgumentException)
        {
            return false;
        }

        return true;
    }

    private async Task<bool> IsActivationEvidenceValidAndReopenableAsync(
        ControlPlaneDbContext context,
        CorpusActivationRecord record,
        CancellationToken cancellationToken)
    {
        if (!record.HasCompleteEvidenceBindings)
        {
            return false;
        }

        var documentRows = await context.DocumentVersions.AsNoTracking()
            .Where(row => row.CorpusId == record.CorpusId.Value)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var generationRows = await context.GenerationManifestBindings.AsNoTracking()
            .Where(row => row.CorpusId == record.CorpusId.Value &&
                row.IndexGenerationId == record.IndexGenerationId.Value)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        if (generationRows.Length != record.EvidenceBindings.Count)
        {
            return false;
        }

        var contentStore = new ImmutableContentStore(options);

        try
        {
            foreach (var evidence in record.EvidenceBindings)
            {
                var binding = evidence.DocumentBinding;
                var document = documentRows.SingleOrDefault(row =>
                    row.DocumentId == binding.DocumentId.Value &&
                    row.DocumentVersion == binding.DocumentVersion.Value);
                var generation = generationRows.SingleOrDefault(row =>
                    row.DocumentId == binding.DocumentId.Value &&
                    row.DocumentVersion == binding.DocumentVersion.Value);

                if (document is null || generation is null ||
                    !ActivationEvidenceMatchesDocument(evidence, document) ||
                    !GenerationBindingMatches(generation, binding))
                {
                    return false;
                }

                _ = ParseLanguage(document.ContentLanguage);

                await using (var source = await contentStore.OpenVerifiedAsync(
                    evidence.SourceContentObjectId,
                    new ExpectedHashAndLength(
                        evidence.SourceContentObjectId,
                        document.ByteLength),
                    cancellationToken).ConfigureAwait(false))
                {
                }

                if (binding.DocumentFormat == DocumentFormat.Csv)
                {
                    continue;
                }

                var manifestRow = await context.DocumentRenderManifests.AsNoTracking()
                    .SingleOrDefaultAsync(
                        row => row.RenderManifestId == evidence.RenderManifestId!.Value,
                        cancellationToken).ConfigureAwait(false);

                if (manifestRow is null ||
                    manifestRow.CorpusId != record.CorpusId.Value ||
                    manifestRow.DocumentId != binding.DocumentId.Value ||
                    manifestRow.DocumentVersion != binding.DocumentVersion.Value ||
                    manifestRow.SourceContentSha256 != evidence.SourceContentObjectId.Value)
                {
                    return false;
                }

                var manifest = await ReadRenderManifestAsync(
                    context,
                    manifestRow,
                    cancellationToken).ConfigureAwait(false);

                foreach (var page in manifest.OrderedPageImages)
                {
                    await using var image = await contentStore.OpenVerifiedAsync(
                        page.ImageContentObjectId,
                        new ExpectedHashAndLength(
                            page.ImageContentObjectId,
                            page.ByteLength),
                        cancellationToken).ConfigureAwait(false);
                }
            }
        }
        catch (Exception exception) when (
            exception is IOException or InvalidDataException or UnauthorizedAccessException or
                ArgumentException or InvalidOperationException)
        {
            return false;
        }

        return true;
    }

    private static bool ActivationEvidenceMatchesDocument(
        DocumentActivationEvidenceBinding evidence,
        DocumentVersionRow document)
    {
        var binding = evidence.DocumentBinding;
        return document.ProductId == binding.DatabaseProductId.Value &&
            document.ProductRevision == binding.DatabaseProductRevision.Value &&
            document.DocumentFormat == binding.DocumentFormat.ToString() &&
            document.ContentSha256 == evidence.SourceContentObjectId.Value &&
            document.SourceAdapterId == binding.SourceAdapterId.Value &&
            document.SourceTrustClass == binding.SourceTrustClass.ToString() &&
            document.OfficialRegistrationId == binding.OfficialSourceRegistrationId?.Value &&
            document.OfficialSnapshotId == binding.OfficialSnapshotId?.Value;
    }

    private static bool GenerationBindingMatches(
        GenerationManifestBindingRow row,
        DocumentBinding binding) =>
        row.ProductId == binding.DatabaseProductId.Value &&
        row.ProductRevision == binding.DatabaseProductRevision.Value &&
        row.DocumentId == binding.DocumentId.Value &&
        row.DocumentVersion == binding.DocumentVersion.Value &&
        row.DocumentFormat == binding.DocumentFormat.ToString() &&
        row.SourceAdapterId == binding.SourceAdapterId.Value &&
        row.SourceTrustClass == binding.SourceTrustClass.ToString() &&
        row.OfficialRegistrationId == binding.OfficialSourceRegistrationId?.Value &&
        row.OfficialSnapshotId == binding.OfficialSnapshotId?.Value;

    private static void AddActivationEvidenceRows(
        ControlPlaneDbContext context,
        CorpusActivationRecord record)
    {
        context.ActivationEvidenceBindings.AddRange(record.EvidenceBindings.Select(evidence =>
            ControlPlaneMapping.ToActivationEvidenceBindingRow(
                record.CorpusId,
                record.RecordRevision,
                evidence)));
        context.ActivationRightsDecisions.AddRange(record.EvidenceBindings.SelectMany(evidence =>
            ControlPlaneMapping.ToActivationRightsDecisionRows(
                record.CorpusId,
                record.RecordRevision,
                evidence)));
    }

    private static async Task<IReadOnlyList<DocumentBinding>?>
        ResolveExpectedGenerationBindingsAsync(
            ControlPlaneDbContext context,
            GenerationCommitRequest request,
            CancellationToken cancellationToken)
    {
        var activeDocumentCount = await context.CatalogueRevisionDocuments.AsNoTracking()
            .CountAsync(
                row => row.CorpusId == request.Manifest.CorpusId.Value &&
                    row.CatalogueRevision == request.Manifest.CatalogueRevision.Value &&
                    row.Status == "Active",
                cancellationToken)
            .ConfigureAwait(false);
        var activeRows = await (
            from catalogueDocument in context.CatalogueRevisionDocuments.AsNoTracking()
            join document in context.DocumentVersions.AsNoTracking()
                on new
                {
                    catalogueDocument.CorpusId,
                    catalogueDocument.DocumentId,
                    catalogueDocument.DocumentVersion,
                }
                equals new
                {
                    document.CorpusId,
                    document.DocumentId,
                    document.DocumentVersion,
                }
            where catalogueDocument.CorpusId == request.Manifest.CorpusId.Value &&
                catalogueDocument.CatalogueRevision ==
                    request.Manifest.CatalogueRevision.Value &&
                catalogueDocument.Status == "Active"
            select new
            {
                CatalogueProductId = catalogueDocument.ProductId,
                CatalogueProductRevision = catalogueDocument.ProductRevision,
                DocumentProductId = document.ProductId,
                DocumentProductRevision = document.ProductRevision,
                document.DocumentId,
                document.DocumentVersion,
                document.DocumentFormat,
                document.ContentLanguage,
                document.SourceAdapterId,
                document.SourceTrustClass,
                document.OfficialRegistrationId,
                document.OfficialSnapshotId,
                document.ContentSha256,
                document.ByteLength,
                document.MediaType,
            })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var requestedByKey = new Dictionary<GenerationBindingKey, DocumentBinding>();

        foreach (var binding in request.Bindings)
        {
            if (!requestedByKey.TryAdd(ToReplayBindingKey(binding), binding))
            {
                return null;
            }
        }

        if (activeRows.Length != activeDocumentCount ||
            activeRows.Length != requestedByKey.Count)
        {
            return null;
        }

        var officialSnapshots = activeRows.Any(row =>
            string.Equals(
                row.SourceTrustClass,
                SourceTrustClass.OfficialExternal.ToString(),
                StringComparison.Ordinal))
            ? await context.OfficialSourceSnapshots.AsNoTracking()
                .Where(row => row.CorpusId == request.Manifest.CorpusId.Value)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false)
            : [];
        var officialRegistrations = officialSnapshots.Length == 0
            ? []
            : await context.OfficialSourceRegistrations.AsNoTracking()
                .Where(row => row.CorpusId == request.Manifest.CorpusId.Value)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
        var expectedBindings = new List<DocumentBinding>(activeRows.Length);

        foreach (var row in activeRows)
        {
            if (!string.Equals(
                    row.CatalogueProductId,
                    row.DocumentProductId,
                    StringComparison.Ordinal) ||
                    row.CatalogueProductRevision != row.DocumentProductRevision)
            {
                return null;
            }

            _ = ParseLanguage(row.ContentLanguage);

            var expectedKey = new GenerationBindingKey(
                row.DocumentProductId,
                row.DocumentProductRevision,
                row.DocumentId,
                row.DocumentVersion,
                row.DocumentFormat,
                row.SourceAdapterId,
                row.SourceTrustClass,
                row.OfficialRegistrationId,
                row.OfficialSnapshotId);

            if (!requestedByKey.Remove(expectedKey, out var requestedBinding))
            {
                return null;
            }

            if (string.Equals(
                    row.SourceTrustClass,
                    SourceTrustClass.OfficialExternal.ToString(),
                    StringComparison.Ordinal))
            {
                var snapshot = officialSnapshots.SingleOrDefault(snapshot =>
                    string.Equals(
                        snapshot.SnapshotId,
                        row.OfficialSnapshotId,
                        StringComparison.Ordinal) &&
                    string.Equals(
                        snapshot.RegistrationId,
                        row.OfficialRegistrationId,
                        StringComparison.Ordinal));
                var registration = snapshot is null
                    ? null
                    : officialRegistrations.SingleOrDefault(registration =>
                        string.Equals(
                            registration.RegistrationId,
                            snapshot.RegistrationId,
                            StringComparison.Ordinal) &&
                        registration.RegistrationRevision ==
                            snapshot.RegistrationRevision);

                if (snapshot is null || registration is null ||
                    !string.Equals(
                        snapshot.ContentSha256,
                        row.ContentSha256,
                        StringComparison.Ordinal) ||
                    snapshot.ByteLength != row.ByteLength ||
                    !string.Equals(
                        snapshot.MediaType,
                        row.MediaType,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        registration.ProductId,
                        row.DocumentProductId,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        registration.DocumentId,
                        row.DocumentId,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        registration.SourceAdapterId,
                        row.SourceAdapterId,
                        StringComparison.Ordinal))
                {
                    return null;
                }
            }

            expectedBindings.Add(requestedBinding);
        }

        return requestedByKey.Count == 0 ? expectedBindings : null;
    }

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
        var evidence = await context.ActivationEvidenceBindings.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value &&
                row.RecordRevision == head.RecordRevision)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var rights = await context.ActivationRightsDecisions.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value &&
                row.RecordRevision == head.RecordRevision)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        return ControlPlaneMapping.ToDomain(record, bindings, evidence, rights);
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

    private static async Task<StoreMutationResult> ReadExactOfficialSourceReplayAsync(
        ControlPlaneDbContext context,
        OfficialSourceCommitRequest request,
        AdminOperationRow operation,
        CancellationToken cancellationToken)
    {
        EnsureOperationIdentity(operation, "OfficialSourceCommit");
        var resultRevision = request.Registration.Revision.Value;

        if (!string.Equals(operation.CorpusId, request.CorpusId.Value, StringComparison.Ordinal) ||
            operation.ExpectedRevision is null ||
            (operation.ExpectedRevision != resultRevision - 1 &&
                operation.ExpectedRevision != resultRevision) ||
            operation.ResultRevision != resultRevision ||
            operation.CompletedAtUtc is null ||
            ControlPlaneMapping.ParseUtc(operation.RequestedAtUtc) != request.CommittedAt ||
            ControlPlaneMapping.ParseUtc(operation.CompletedAtUtc) != request.CommittedAt)
        {
            throw new InvalidOperationException(
                "An official-source operation identity was reused with different intent.");
        }

        var registration = await context.OfficialSourceRegistrations.AsNoTracking()
            .SingleOrDefaultAsync(
                row => row.CorpusId == request.CorpusId.Value &&
                    row.RegistrationId == request.Registration.Id.Value &&
                    row.RegistrationRevision == request.Registration.Revision.Value,
                cancellationToken).ConfigureAwait(false);
        var snapshot = await context.OfficialSourceSnapshots.AsNoTracking()
            .SingleOrDefaultAsync(
                row => row.CorpusId == request.CorpusId.Value &&
                    row.SnapshotId == request.Snapshot.Id.Value,
                cancellationToken).ConfigureAwait(false);
        var content = await context.ContentObjects.AsNoTracking()
            .SingleOrDefaultAsync(
                row => row.ContentSha256 == request.Snapshot.ContentObjectId.Value,
                cancellationToken).ConfigureAwait(false);
        var audit = await context.AuditEvents.AsNoTracking().SingleOrDefaultAsync(
            row => row.CorpusId == request.CorpusId.Value &&
                row.OperationId == request.OperationId.Value,
            cancellationToken).ConfigureAwait(false);
        var expectedAuditDigest = BuildAuditDetailsDigest(
            request.CorpusId,
            request.OperationId,
            "OfficialSourceCommitted",
            resultRevision,
            request.CommittedAt,
            request.AuditDetailsDigest);

        if (registration is null ||
            !OfficialSourceRegistrationMatches(registration, request.Registration) ||
            snapshot is null ||
            !string.Equals(snapshot.RegistrationId, request.Registration.Id.Value, StringComparison.Ordinal) ||
            snapshot.RegistrationRevision != request.Registration.Revision.Value ||
            !string.Equals(snapshot.ContentSha256, request.Snapshot.ContentObjectId.Value, StringComparison.Ordinal) ||
            snapshot.ByteLength != request.Snapshot.ByteLength ||
            !string.Equals(snapshot.MediaType, request.Snapshot.MediaType, StringComparison.Ordinal) ||
            ControlPlaneMapping.ParseUtc(snapshot.RetrievedAtUtc) != request.Snapshot.RetrievedAt ||
            content is null || content.ByteLength != request.Snapshot.ByteLength ||
            audit is null ||
            !string.Equals(audit.EventType, "OfficialSourceCommitted", StringComparison.Ordinal) ||
            ControlPlaneMapping.ParseUtc(audit.OccurredAtUtc) != request.CommittedAt ||
            !string.Equals(audit.DetailsDigest, expectedAuditDigest, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The persisted official-source replay evidence differs from the requested intent.");
        }

        return new StoreMutationResult(StoreMutationOutcome.AlreadyApplied, resultRevision);
    }

    private static async Task<StoreMutationResult> ReadExactObservationReplayAsync(
        ControlPlaneDbContext context,
        ObservationCommitRequest request,
        AdminOperationRow operation,
        long maxAgeSeconds,
        CancellationToken cancellationToken)
    {
        EnsureOperationIdentity(operation, "ObservationAppend");
        var resultRevision = request.Observation.JournalRevision.Value;

        if (!string.Equals(operation.CorpusId, request.CorpusId.Value, StringComparison.Ordinal) ||
            operation.ExpectedRevision != request.ExpectedJournalRevision ||
            operation.ResultRevision != resultRevision ||
            operation.CompletedAtUtc is null ||
            ControlPlaneMapping.ParseUtc(operation.RequestedAtUtc) != request.CommittedAt ||
            ControlPlaneMapping.ParseUtc(operation.CompletedAtUtc) != request.CommittedAt)
        {
            throw new InvalidOperationException(
                "An observation operation identity was reused with different intent.");
        }

        var observation = await context.SourceObservations.AsNoTracking()
            .SingleOrDefaultAsync(
                row => row.OperationId == request.OperationId.Value,
                cancellationToken).ConfigureAwait(false);
        var audit = await context.AuditEvents.AsNoTracking().SingleOrDefaultAsync(
            row => row.CorpusId == request.CorpusId.Value &&
                row.OperationId == request.OperationId.Value,
            cancellationToken).ConfigureAwait(false);
        var expectedAuditDigest = BuildAuditDetailsDigest(
            request.CorpusId,
            request.OperationId,
            "ObservationAppended",
            resultRevision,
            request.CommittedAt,
            request.AuditDetailsDigest);

        if (observation is null ||
            !string.Equals(observation.CorpusId, request.CorpusId.Value, StringComparison.Ordinal) ||
            !string.Equals(observation.ObservationId, request.Observation.Id.Value, StringComparison.Ordinal) ||
            !string.Equals(observation.RegistrationId, request.Observation.RegistrationId.Value, StringComparison.Ordinal) ||
            !string.Equals(observation.SnapshotId, request.Observation.SnapshotId.Value, StringComparison.Ordinal) ||
            observation.JournalRevision != resultRevision ||
            !string.Equals(observation.State, request.Observation.State.ToString(), StringComparison.Ordinal) ||
            ControlPlaneMapping.ParseUtc(observation.RevalidatedAtUtc) != request.Observation.RevalidatedAt ||
            observation.MaxAgeSeconds != maxAgeSeconds ||
            audit is null ||
            !string.Equals(audit.EventType, "ObservationAppended", StringComparison.Ordinal) ||
            ControlPlaneMapping.ParseUtc(audit.OccurredAtUtc) != request.CommittedAt ||
            !string.Equals(audit.DetailsDigest, expectedAuditDigest, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The persisted observation replay evidence differs from the requested intent.");
        }

        return new StoreMutationResult(StoreMutationOutcome.AlreadyApplied, resultRevision);
    }

    private static async Task<StoreMutationResult> ReadExactGenerationReplayAsync(
        ControlPlaneDbContext context,
        GenerationCommitRequest request,
        AdminOperationRow operation,
        CancellationToken cancellationToken)
    {
        EnsureOperationIdentity(operation, "GenerationCommit");
        var resultRevision = request.Manifest.CatalogueRevision.Value;

        if (!string.Equals(operation.CorpusId, request.Manifest.CorpusId.Value, StringComparison.Ordinal) ||
            operation.ExpectedRevision != resultRevision ||
            operation.ResultRevision != resultRevision ||
            operation.CompletedAtUtc is null ||
            ControlPlaneMapping.ParseUtc(operation.RequestedAtUtc) != request.FinalisedAt ||
            ControlPlaneMapping.ParseUtc(operation.CompletedAtUtc) != request.FinalisedAt)
        {
            throw new InvalidOperationException(
                "A generation operation identity was reused with different intent.");
        }

        var manifest = await context.GenerationManifests.AsNoTracking()
            .SingleOrDefaultAsync(
                row => row.OperationId == request.OperationId.Value,
                cancellationToken).ConfigureAwait(false);
        var bindings = await context.GenerationManifestBindings.AsNoTracking()
            .Where(row => row.CorpusId == request.Manifest.CorpusId.Value &&
                row.IndexGenerationId == request.Manifest.IndexGenerationId.Value)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var requestedBindings = request.Bindings.Select(ToReplayBindingKey).ToHashSet();
        var persistedBindings = bindings.Select(ToReplayBindingKey).ToHashSet();
        var audit = await context.AuditEvents.AsNoTracking().SingleOrDefaultAsync(
            row => row.CorpusId == request.Manifest.CorpusId.Value &&
                row.OperationId == request.OperationId.Value,
            cancellationToken).ConfigureAwait(false);
        var expectedAuditDigest = BuildAuditDetailsDigest(
            request.Manifest.CorpusId,
            request.OperationId,
            "GenerationCommitted",
            resultRevision,
            request.FinalisedAt,
            request.AuditDetailsDigest);

        if (manifest is null ||
            !GenerationManifestMatches(manifest, request) ||
            bindings.Length != request.Bindings.Count ||
            requestedBindings.Count != request.Bindings.Count ||
            !persistedBindings.SetEquals(requestedBindings) ||
            audit is null ||
            !string.Equals(audit.EventType, "GenerationCommitted", StringComparison.Ordinal) ||
            ControlPlaneMapping.ParseUtc(audit.OccurredAtUtc) != request.FinalisedAt ||
            !string.Equals(audit.DetailsDigest, expectedAuditDigest, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The persisted generation replay evidence differs from the requested intent.");
        }

        return new StoreMutationResult(StoreMutationOutcome.AlreadyApplied, resultRevision);
    }

    private static bool GenerationManifestMatches(
        GenerationManifestRow row,
        GenerationCommitRequest request) =>
        string.Equals(row.CorpusId, request.Manifest.CorpusId.Value, StringComparison.Ordinal) &&
        string.Equals(row.IndexGenerationId, request.Manifest.IndexGenerationId.Value, StringComparison.Ordinal) &&
        string.Equals(row.CandidateBuildId, request.CandidateBuildId.Value, StringComparison.Ordinal) &&
        row.ManifestSchemaVersion == request.Manifest.ManifestSchemaVersion &&
        row.CorpusRevision == request.Manifest.CorpusRevision.Value &&
        row.CatalogueRevision == request.Manifest.CatalogueRevision.Value &&
        string.Equals(row.ActiveDocumentSetDigest, request.Manifest.ActiveDocumentSetDigest.Value, StringComparison.Ordinal) &&
        string.Equals(row.SourceBindingSetDigest, request.Manifest.SourceBindingSetDigest.Value, StringComparison.Ordinal) &&
        string.Equals(row.IndexCompatibilityKey, request.Manifest.IndexCompatibilityKey.Value, StringComparison.Ordinal) &&
        string.Equals(row.GenerationSpecDigest, request.Manifest.GenerationSpecDigest.Value, StringComparison.Ordinal) &&
        row.ChunkCount == request.Manifest.ChunkCount &&
        row.VectorCount == request.Manifest.VectorCount &&
        string.Equals(row.LogicalArtifactDigest, request.Manifest.LogicalArtifactDigest.Value, StringComparison.Ordinal) &&
        string.Equals(row.GenerationContentDigest, request.Manifest.GenerationContentDigest.Value, StringComparison.Ordinal) &&
        ControlPlaneMapping.ParseUtc(row.FinalisedAtUtc) == request.FinalisedAt &&
        string.Equals(row.OperationId, request.OperationId.Value, StringComparison.Ordinal);

    private static GenerationBindingKey ToReplayBindingKey(DocumentBinding binding) =>
        new(
            binding.DatabaseProductId.Value,
            binding.DatabaseProductRevision.Value,
            binding.DocumentId.Value,
            binding.DocumentVersion.Value,
            binding.DocumentFormat.ToString(),
            binding.SourceAdapterId.Value,
            binding.SourceTrustClass.ToString(),
            binding.OfficialSourceRegistrationId?.Value,
            binding.OfficialSnapshotId?.Value);

    private static GenerationBindingKey ToReplayBindingKey(
        GenerationManifestBindingRow binding) =>
        new(
            binding.ProductId,
            binding.ProductRevision,
            binding.DocumentId,
            binding.DocumentVersion,
            binding.DocumentFormat,
            binding.SourceAdapterId,
            binding.SourceTrustClass,
            binding.OfficialRegistrationId,
            binding.OfficialSnapshotId);

    private static async Task<StoreMutationResult> ReadExactCatalogueReplayAsync(
        ControlPlaneDbContext context,
        CatalogueCommitRequest request,
        AdminOperationRow operation,
        CancellationToken cancellationToken)
    {
        EnsureOperationIdentity(operation, "CatalogueCommit");

        if (!string.Equals(
            operation.CorpusId,
                request.Snapshot.CorpusId.Value,
                StringComparison.Ordinal) ||
            operation.ExpectedRevision != request.ExpectedCurrentRevision ||
            operation.ResultRevision != request.Snapshot.Revision.Value)
        {
            throw new InvalidOperationException(
                "A catalogue operation identity was reused with different intent.");
        }

        var revisionExists = await context.CatalogueRevisions.AsNoTracking().AnyAsync(
            row => row.CorpusId == request.Snapshot.CorpusId.Value &&
                row.CatalogueRevision == request.Snapshot.Revision.Value &&
                row.OperationId == request.OperationId.Value,
            cancellationToken).ConfigureAwait(false);
        var persistedSnapshot = revisionExists
            ? await ReadCatalogueSnapshotAsync(
                context,
                request.Snapshot.CorpusId,
                request.Snapshot.Revision.Value,
                cancellationToken).ConfigureAwait(false)
            : null;
        var audit = await context.AuditEvents.AsNoTracking().SingleOrDefaultAsync(
            row => row.CorpusId == request.Snapshot.CorpusId.Value &&
                row.OperationId == request.OperationId.Value,
            cancellationToken).ConfigureAwait(false);
        var occurredAt = audit is null
            ? default
            : ControlPlaneMapping.ParseUtc(audit.OccurredAtUtc);
        var expectedAuditDigest = BuildAuditDetailsDigest(
            request.Snapshot.CorpusId,
            request.OperationId,
            "CatalogueCommitted",
            request.Snapshot.Revision.Value,
            occurredAt,
            request.AuditDetailsDigest);

        if (!revisionExists || persistedSnapshot is null ||
            !CatalogueSnapshotMatches(persistedSnapshot, request.Snapshot) ||
            audit is null ||
            !string.Equals(audit.EventType, "CatalogueCommitted", StringComparison.Ordinal) ||
            operation.CompletedAtUtc is null ||
            ControlPlaneMapping.ParseUtc(operation.RequestedAtUtc) != occurredAt ||
            ControlPlaneMapping.ParseUtc(operation.CompletedAtUtc) != occurredAt ||
            !string.Equals(audit.DetailsDigest, expectedAuditDigest, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The persisted catalogue replay evidence differs from the requested intent.");
        }

        return new StoreMutationResult(
            StoreMutationOutcome.AlreadyApplied,
            request.Snapshot.Revision.Value);
    }

    private static async Task<CatalogueSnapshot?> ReadCatalogueSnapshotAsync(
        ControlPlaneDbContext context,
        CorpusId corpusId,
        long catalogueRevision,
        CancellationToken cancellationToken)
    {
        var revisionExists = await context.CatalogueRevisions.AsNoTracking().AnyAsync(
            row => row.CorpusId == corpusId.Value &&
                row.CatalogueRevision == catalogueRevision,
            cancellationToken).ConfigureAwait(false);

        if (!revisionExists)
        {
            return null;
        }

        var productLinks = await context.CatalogueRevisionProducts.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value &&
                row.CatalogueRevision == catalogueRevision)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var activeProductKeys = productLinks
            .Select(row => (row.ProductId, row.ProductRevision))
            .ToHashSet();
        var productRows = await context.DatabaseProductRevisions.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var categoryLinks = await context.DatabaseProductCategories.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var categoryIds = categoryLinks
            .Where(row => activeProductKeys.Contains((row.ProductId, row.ProductRevision)))
            .Select(row => row.CategoryId)
            .ToHashSet(StringComparer.Ordinal);
        var categoryRows = await context.DatabaseCategories.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value &&
                categoryIds.Contains(row.CategoryId))
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var products = productLinks.Select(link =>
        {
            var row = productRows.Single(item =>
                item.ProductId == link.ProductId &&
                item.ProductRevision == link.ProductRevision);
            var categories = categoryLinks
                .Where(item => item.ProductId == link.ProductId &&
                    item.ProductRevision == link.ProductRevision)
                .Select(item => new DatabaseCategoryId(item.CategoryId));
            return new DatabaseProduct(
                new DatabaseProductId(row.ProductId),
                new DatabaseProductRevision(row.ProductRevision),
                row.DisplayName,
                Enum.Parse<CatalogueItemStatus>(link.Status, ignoreCase: false),
                categories);
        }).ToArray();
        var documentLinks = await context.CatalogueRevisionDocuments.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value &&
                row.CatalogueRevision == catalogueRevision)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var documentRows = await context.DocumentVersions.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var documents = documentLinks.Select(link =>
        {
            var row = documentRows.Single(item =>
                item.DocumentId == link.DocumentId &&
                item.DocumentVersion == link.DocumentVersion);
            return new DocumentVersion(
                new DocumentId(row.DocumentId),
                new DocumentVersionNumber(row.DocumentVersion),
                new DatabaseProductId(link.ProductId),
                new DatabaseProductRevision(link.ProductRevision),
                Enum.Parse<DocumentFormat>(row.DocumentFormat, ignoreCase: false),
                ParseLanguage(row.ContentLanguage),
                Enum.Parse<CatalogueItemStatus>(link.Status, ignoreCase: false),
                new ContentObjectId(row.ContentSha256),
                row.ByteLength,
                row.MediaType,
                new SourceAdapterId(row.SourceAdapterId),
                Enum.Parse<SourceTrustClass>(row.SourceTrustClass, ignoreCase: false),
                row.OfficialRegistrationId is null
                    ? null
                    : new OfficialSourceRegistrationId(row.OfficialRegistrationId),
                row.OfficialSnapshotId is null
                    ? null
                    : new OfficialSnapshotId(row.OfficialSnapshotId),
                row.SourceDeclaredLanguage is null
                    ? null
                    : new SourceDeclaredLanguage(row.SourceDeclaredLanguage));
        }).ToArray();
        return new CatalogueSnapshot(
            corpusId,
            new CatalogueRevision(catalogueRevision),
            categoryRows.Select(row => new DatabaseCategory(
                new DatabaseCategoryId(row.CategoryId),
                row.DisplayName)),
            products,
            documents);
    }

    private static bool CatalogueSnapshotMatches(
        CatalogueSnapshot left,
        CatalogueSnapshot right) =>
        left.CorpusId == right.CorpusId &&
        left.Revision == right.Revision &&
        left.DatabaseCategories
            .OrderBy(item => item.Id.Value, StringComparer.Ordinal)
            .Select(item => $"{item.Id.Value}\n{item.DisplayName}")
            .SequenceEqual(
                right.DatabaseCategories
                    .OrderBy(item => item.Id.Value, StringComparer.Ordinal)
                    .Select(item => $"{item.Id.Value}\n{item.DisplayName}")) &&
        left.DatabaseProducts
            .OrderBy(item => item.Id.Value, StringComparer.Ordinal)
            .Select(ProductProjection)
            .SequenceEqual(
                right.DatabaseProducts
                    .OrderBy(item => item.Id.Value, StringComparer.Ordinal)
                    .Select(ProductProjection)) &&
        left.DocumentVersions
            .OrderBy(item => item.Id.Value, StringComparer.Ordinal)
            .ThenBy(item => item.Version.Value)
            .Select(DocumentProjection)
            .SequenceEqual(
                right.DocumentVersions
                    .OrderBy(item => item.Id.Value, StringComparer.Ordinal)
                    .ThenBy(item => item.Version.Value)
                    .Select(DocumentProjection));

    private static string ProductProjection(DatabaseProduct product) =>
        string.Join(
            '\n',
            product.Id.Value,
            product.Revision.ToCanonicalString(),
            product.DisplayName,
            product.Status.ToString(),
            string.Join(
                ',',
                product.CategoryIds
                    .Select(item => item.Value)
                    .Order(StringComparer.Ordinal)));

    private static string DocumentProjection(DocumentVersion document) =>
        string.Join(
            '\n',
            document.Id.Value,
            document.Version.ToCanonicalString(),
            document.DatabaseProductId.Value,
            document.DatabaseProductRevision.ToCanonicalString(),
            document.Format.ToString(),
            document.ContentLanguage.ToCanonicalTag(),
            document.SourceDeclaredLanguage?.ObservedTag ?? string.Empty,
            document.Status.ToString(),
            document.ContentObjectId.Value,
            document.ByteLength.ToString(System.Globalization.CultureInfo.InvariantCulture),
            document.MediaType,
            document.SourceAdapterId.Value,
            document.SourceTrustClass.ToString(),
            document.OfficialSourceRegistrationId?.Value ?? "",
            document.OfficialSnapshotId?.Value ?? "");

    private static async Task<CorpusActivationRecord> ReadExactActivationReplayAsync(
        ControlPlaneDbContext context,
        ActivationCompareExchangeRequest request,
        AdminOperationRow operation,
        CancellationToken cancellationToken)
    {
        EnsureOperationIdentity(operation, "ActivationCAS");

        if (!string.Equals(
                operation.CorpusId,
                request.ProposedRecord.CorpusId.Value,
                StringComparison.Ordinal) ||
            operation.ExpectedRevision != request.ExpectedCurrentRevision ||
            operation.ResultRevision != request.ProposedRecord.RecordRevision.Value)
        {
            throw new InvalidOperationException(
                "An activation operation identity was reused with different intent.");
        }

        var record = await context.ActivationRecords.AsNoTracking()
            .SingleOrDefaultAsync(
                row => row.OperationId == request.OperationId.Value,
                cancellationToken).ConfigureAwait(false) ??
            throw new InvalidOperationException(
                "The persisted activation replay record is missing.");
        var bindings = await context.ActivationBindings.AsNoTracking()
            .Where(row => row.CorpusId == record.CorpusId &&
                row.RecordRevision == record.RecordRevision)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var evidence = await context.ActivationEvidenceBindings.AsNoTracking()
            .Where(row => row.CorpusId == record.CorpusId &&
                row.RecordRevision == record.RecordRevision)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var rights = await context.ActivationRightsDecisions.AsNoTracking()
            .Where(row => row.CorpusId == record.CorpusId &&
                row.RecordRevision == record.RecordRevision)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var persisted = ControlPlaneMapping.ToDomain(record, bindings, evidence, rights);
        var manifest = await context.GenerationManifests.AsNoTracking()
            .SingleOrDefaultAsync(
                row => row.CorpusId == record.CorpusId &&
                    row.IndexGenerationId == record.IndexGenerationId,
                cancellationToken).ConfigureAwait(false);
        var audit = await context.AuditEvents.AsNoTracking().SingleOrDefaultAsync(
            row => row.CorpusId == record.CorpusId &&
                row.OperationId == request.OperationId.Value,
            cancellationToken).ConfigureAwait(false);
        var occurredAt = audit is null
            ? default
            : ControlPlaneMapping.ParseUtc(audit.OccurredAtUtc);
        var expectedEventType = $"Activation{request.MutationKind}Applied";
        var expectedAuditDigest = BuildAuditDetailsDigest(
            request.ProposedRecord.CorpusId,
            request.OperationId,
            expectedEventType,
            request.ProposedRecord.RecordRevision.Value,
            occurredAt,
            request.AuditDetailsDigest);

        if (!ActivationRecordMatches(persisted, request.ProposedRecord) ||
            !string.Equals(record.MutationKind, request.MutationKind.ToString(), StringComparison.Ordinal) ||
            manifest is null ||
            !string.Equals(
                manifest.IndexCompatibilityKey,
                request.RequiredCompatibilityKey.Value,
                StringComparison.Ordinal) ||
            audit is null ||
            !string.Equals(audit.EventType, expectedEventType, StringComparison.Ordinal) ||
            operation.CompletedAtUtc is null ||
            ControlPlaneMapping.ParseUtc(operation.RequestedAtUtc) != occurredAt ||
            ControlPlaneMapping.ParseUtc(operation.CompletedAtUtc) != occurredAt ||
            !string.Equals(audit.DetailsDigest, expectedAuditDigest, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The persisted activation replay evidence differs from the requested intent.");
        }

        return persisted;
    }

    private static async Task<StoreMutationResult>
        ReadExactOfficialRegistrationReplayAsync(
            ControlPlaneDbContext context,
            OfficialSourceRegistrationCommitRequest request,
            AdminOperationRow operation,
            CancellationToken cancellationToken)
    {
        EnsureOperationIdentity(operation, "OfficialSourceRegistration");
        var expectedPriorRevision = request.Registration.Revision.Value - 1;

        if (!string.Equals(operation.CorpusId, request.CorpusId.Value, StringComparison.Ordinal) ||
            operation.ExpectedRevision != expectedPriorRevision ||
            operation.ResultRevision != request.Registration.Revision.Value)
        {
            throw new InvalidOperationException(
                "An official-source registration operation was reused with different intent.");
        }

        var registration = await context.OfficialSourceRegistrations.AsNoTracking()
            .SingleOrDefaultAsync(
                row => row.CorpusId == request.CorpusId.Value &&
                    row.RegistrationId == request.Registration.Id.Value &&
                    row.RegistrationRevision == request.Registration.Revision.Value,
                cancellationToken).ConfigureAwait(false);
        var audit = await context.AuditEvents.AsNoTracking().SingleOrDefaultAsync(
            row => row.CorpusId == request.CorpusId.Value &&
                row.OperationId == request.OperationId.Value,
            cancellationToken).ConfigureAwait(false);
        var occurredAt = audit is null
            ? default
            : ControlPlaneMapping.ParseUtc(audit.OccurredAtUtc);
        var expectedAuditDigest = BuildAuditDetailsDigest(
            request.CorpusId,
            request.OperationId,
            "OfficialSourceRegistered",
            request.Registration.Revision.Value,
            occurredAt,
            request.AuditDetailsDigest);

        if (registration is null ||
            !OfficialSourceRegistrationMatches(registration, request.Registration) ||
            audit is null ||
            !string.Equals(audit.EventType, "OfficialSourceRegistered", StringComparison.Ordinal) ||
            operation.CompletedAtUtc is null ||
            ControlPlaneMapping.ParseUtc(operation.RequestedAtUtc) != occurredAt ||
            ControlPlaneMapping.ParseUtc(operation.CompletedAtUtc) != occurredAt ||
            !string.Equals(audit.DetailsDigest, expectedAuditDigest, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The persisted official-source registration evidence differs from the request.");
        }

        return new StoreMutationResult(
            StoreMutationOutcome.AlreadyApplied,
            request.Registration.Revision.Value);
    }

    private static bool ActivationRecordMatches(
        CorpusActivationRecord left,
        CorpusActivationRecord right) =>
        left.CorpusId == right.CorpusId &&
        left.RecordRevision == right.RecordRevision &&
        left.PreviousRecordRevision == right.PreviousRecordRevision &&
        left.IndexGenerationId == right.IndexGenerationId &&
        left.CatalogueRevision == right.CatalogueRevision &&
        left.ActivationBindingSetDigest == right.ActivationBindingSetDigest &&
        left.GenerationActivatedAt == right.GenerationActivatedAt &&
        left.RecordUpdatedAt == right.RecordUpdatedAt &&
        left.DocumentBindings.SequenceEqual(right.DocumentBindings) &&
        ActivationEvidenceMatches(left.EvidenceBindings, right.EvidenceBindings);

    private static bool ActivationEvidenceMatches(
        System.Collections.ObjectModel.ReadOnlyCollection<DocumentActivationEvidenceBinding> left,
        System.Collections.ObjectModel.ReadOnlyCollection<DocumentActivationEvidenceBinding> right) =>
        left.Count == right.Count && left.Zip(right).All(pair =>
            pair.First.DocumentBinding == pair.Second.DocumentBinding &&
            pair.First.SourceContentObjectId == pair.Second.SourceContentObjectId &&
            pair.First.RightsSchemaVersion == pair.Second.RightsSchemaVersion &&
            pair.First.RenderManifestId == pair.Second.RenderManifestId &&
            pair.First.Rights.Decisions.SequenceEqual(pair.Second.Rights.Decisions));

    private static void EnsureJournalCompletionMatchesMutation(
        AdministrationJournalCompletion? completion,
        OperationId operationId,
        long resultRevision)
    {
        if (completion is null)
        {
            return;
        }

        if (completion.OperationId != operationId ||
            completion.Outcome != AdministrationJournalResultOutcome.Applied ||
            !string.Equals(completion.ResultCode, "CH_ADMIN_APPLIED", StringComparison.Ordinal) ||
            completion.ExitCategory != 0 ||
            completion.ResultRevision != resultRevision)
        {
            throw new InvalidOperationException(
                "The administrative journal completion does not match the committed mutation.");
        }
    }

    private static DateTimeOffset JournalCompletedAt(DateTimeOffset startedAt)
    {
        var completedAt = DateTimeOffset.UtcNow;
        return completedAt < startedAt ? startedAt : completedAt;
    }

    private static async Task<bool> HasBlockingLeaseAsync(
        ControlPlaneDbContext context,
        CorpusId corpusId,
        OperationId operationId,
        DateTimeOffset evaluatedAt,
        CancellationToken cancellationToken)
    {
        var recoveryLeases = await context.RecoveryLeases.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value &&
                row.OperationId != operationId.Value)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var administrationLeases = await context.AdministrationLeases.AsNoTracking()
            .Where(row => row.CorpusId == corpusId.Value &&
                row.OperationId != operationId.Value)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        return recoveryLeases.Any(row =>
                ControlPlaneMapping.ParseUtc(row.ExpiresAtUtc) > evaluatedAt) ||
            administrationLeases.Any(row =>
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

    private static void EnsureRecoverableCatalogueCategoryProjection(
        CatalogueSnapshot snapshot)
    {
        var categories = snapshot.DatabaseCategories
            .Select(category => category.Id)
            .ToHashSet();
        var referenced = snapshot.DatabaseProducts
            .SelectMany(product => product.CategoryIds)
            .ToHashSet();

        if (!categories.SetEquals(referenced))
        {
            throw new ArgumentException(
                "Every persisted catalogue category must be assigned in the same snapshot.",
                nameof(snapshot));
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
        DateTimeOffset occurredAt,
        string? supplementalDetailsDigest)
    {
        var operation = context.AdminOperations.Local.Single(
            row => row.OperationId == operationId.Value);
        operation.Status = "Applied";
        operation.ResultRevision = resultRevision;
        operation.CompletedAtUtc = ControlPlaneMapping.FormatUtc(occurredAt);
        var detailsDigest = BuildAuditDetailsDigest(
            corpusId,
            operationId,
            eventType,
            resultRevision,
            occurredAt,
            supplementalDetailsDigest);
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

    private static string BuildAuditDetailsDigest(
        CorpusId corpusId,
        OperationId operationId,
        string eventType,
        long resultRevision,
        DateTimeOffset occurredAt,
        string? supplementalDetailsDigest)
    {
        var auditMaterial = string.Join(
            '\n',
            corpusId.Value,
            operationId.Value,
            eventType,
            resultRevision.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ControlPlaneMapping.FormatUtc(occurredAt),
            ValidateSupplementalDigest(supplementalDetailsDigest));
        return Sha256(auditMaterial);
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

    private static string ValidateSupplementalDigest(string? value)
    {
        if (value is null)
        {
            return "none";
        }

        if (value.Length != 64 || value.Any(character =>
                character is not (>= '0' and <= '9') and
                    not (>= 'a' and <= 'f')))
        {
            throw new ArgumentException(
                "A supplemental audit digest must be lowercase SHA-256.",
                nameof(value));
        }

        return value;
    }

    private sealed record GenerationBindingKey(
        string ProductId,
        long ProductRevision,
        string DocumentId,
        long DocumentVersion,
        string DocumentFormat,
        string SourceAdapterId,
        string SourceTrustClass,
        string? OfficialRegistrationId,
        string? OfficialSnapshotId);
}
