// Purpose: Persists and rehydrates one complete AnswerEvidenceRecordV1 atomically in Control, validating exact activation, source, manifest, and page authority while emitting only sanitised audit material.
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.Infrastructure.Persistence;

internal enum AnswerEvidenceStoreFaultPoint
{
    BeforeSave,
    AfterSave,
    BeforeReadback,
    AfterReadback,
    BeforeCommit,
}

internal interface IAnswerEvidenceStoreFaultInjector
{
    void ThrowIfRequested(AnswerEvidenceStoreFaultPoint point);
}

public sealed class SqliteAnswerEvidenceStore : IAnswerEvidenceStore
{
    private const string OperationKind = "AnswerEvidence";
    private const string AuditEventType = "AnswerEvidenceCreated";

    private readonly SqliteStoreOptions options;
    private readonly IAnswerEvidenceStoreFaultInjector? faultInjector;

    public SqliteAnswerEvidenceStore(SqliteStoreOptions options)
        : this(options, faultInjector: null)
    {
    }

    internal SqliteAnswerEvidenceStore(
        SqliteStoreOptions options,
        IAnswerEvidenceStoreFaultInjector? faultInjector)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.faultInjector = faultInjector;
    }

    public async Task<AnswerEvidencePersistenceResult> PersistAsync(
        AnswerEvidenceRecordV1 record,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        await using var context = options.CreateControlContext();
        await using var transaction = await BeginImmediateAsync(
            context,
            cancellationToken).ConfigureAwait(false);
        var existing = await ReadWithinAsync(
            context,
            record.AnswerEvidenceRecordId,
            cancellationToken).ConfigureAwait(false);

        if (existing is not null)
        {
            var outcome = existing.RecordSha256 == record.RecordSha256 &&
                existing.SerialiseCanonicalUtf8().AsSpan()
                    .SequenceEqual(record.SerialiseCanonicalUtf8())
                ? AnswerEvidencePersistenceOutcome.AlreadyApplied
                : AnswerEvidencePersistenceOutcome.Conflict;
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return new AnswerEvidencePersistenceResult(
                outcome,
                outcome == AnswerEvidencePersistenceOutcome.AlreadyApplied ? existing : null);
        }

        await ValidateAuthoritiesAsync(context, record, cancellationToken).ConfigureAwait(false);
        AddRows(context, record);
        faultInjector?.ThrowIfRequested(AnswerEvidenceStoreFaultPoint.BeforeSave);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        faultInjector?.ThrowIfRequested(AnswerEvidenceStoreFaultPoint.AfterSave);
        context.ChangeTracker.Clear();
        faultInjector?.ThrowIfRequested(AnswerEvidenceStoreFaultPoint.BeforeReadback);
        var readback = await ReadWithinAsync(
            context,
            record.AnswerEvidenceRecordId,
            cancellationToken).ConfigureAwait(false) ?? throw new InvalidDataException(
                "The complete answer-evidence record was not readable within its transaction.");
        faultInjector?.ThrowIfRequested(AnswerEvidenceStoreFaultPoint.AfterReadback);

        if (readback.RecordSha256 != record.RecordSha256 ||
            !readback.SerialiseCanonicalUtf8().AsSpan()
                .SequenceEqual(record.SerialiseCanonicalUtf8()))
        {
            throw new InvalidDataException(
                "The answer-evidence transactional readback differs from its canonical input.");
        }

        faultInjector?.ThrowIfRequested(AnswerEvidenceStoreFaultPoint.BeforeCommit);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new AnswerEvidencePersistenceResult(
            AnswerEvidencePersistenceOutcome.Applied,
            readback);
    }

    public async Task<AnswerEvidenceRecordV1?> ReadAsync(
        AnswerEvidenceRecordId recordId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(recordId);
        await using var context = options.CreateControlContext();
        return await ReadWithinAsync(context, recordId, cancellationToken).ConfigureAwait(false);
    }

    private static void AddRows(ControlPlaneDbContext context, AnswerEvidenceRecordV1 record)
    {
        context.AdminOperations.Add(new AdminOperationRow
        {
            OperationId = record.AnswerEvidenceRecordId.Value,
            CorpusId = record.CorpusId.Value,
            OperationKind = OperationKind,
            Status = "Applied",
            ExpectedRevision = null,
            ResultRevision = null,
            RequestedAtUtc = ControlPlaneMapping.FormatUtc(record.CreatedAt),
            CompletedAtUtc = ControlPlaneMapping.FormatUtc(record.CreatedAt),
        });
        context.AnswerEvidenceRecords.Add(new AnswerEvidenceRecordRow
        {
            AnswerEvidenceRecordId = record.AnswerEvidenceRecordId.Value,
            SchemaVersion = record.SchemaVersion,
            RecordSha256 = record.RecordSha256.Value,
            CorpusId = record.CorpusId.Value,
            ActivationRecordRevision = record.ActivationRecordRevision.Value,
            CatalogueRevision = record.CatalogueRevision.Value,
            SourceBindingSetDigest = record.SourceBindingSetDigest.Value,
            ActivationBindingSetDigest = record.ActivationBindingSetDigest.Value,
            IndexGenerationId = record.IndexGenerationId.Value,
            Outcome = record.Outcome,
            QuestionLanguage = record.QuestionLanguage.ToCanonicalTag(),
            AnswerLanguage = record.AnswerLanguage.ToCanonicalTag(),
            AnswerSha256 = record.AnswerSha256.Value,
            AnswerUtf8ByteLength = record.AnswerUtf8ByteLength,
            EvidenceCoverageDigest = record.EvidenceCoverageDigest.Value,
            RetrievalPolicyVersion = record.RetrievalPolicyVersion,
            PromptVersion = record.PromptVersion,
            LanguageModelProviderId = record.LanguageModelDescriptor.ProviderId,
            LanguageModelId = record.LanguageModelDescriptor.ModelId,
            LanguageModelRevision = record.LanguageModelDescriptor.ModelRevision,
            CorrelationId = record.CorrelationId,
            RetentionPolicyId = record.RetentionPolicyId,
            CreatedAtUtc = ControlPlaneMapping.FormatUtc(record.CreatedAt),
            ExpiresAtUtc = ControlPlaneMapping.FormatUtc(record.ExpiresAt),
        });

        context.AnswerEvidenceCitations.AddRange(record.Citations.Select(citation =>
            new AnswerEvidenceCitationRow
            {
                AnswerEvidenceRecordId = record.AnswerEvidenceRecordId.Value,
                Ordinal = citation.Ordinal,
                ProductId = citation.DatabaseProductId.Value,
                ProductRevision = citation.DatabaseProductRevision.Value,
                DocumentId = citation.DocumentId.Value,
                DocumentVersion = citation.DocumentVersion.Value,
                DocumentFormat = citation.DocumentFormat.ToString(),
                ContentLanguage = citation.ContentLanguage.CanonicalTag,
                ChunkId = citation.ChunkId,
                SourceAdapterId = citation.SourceAdapterId.Value,
                SourceTrustClass = citation.SourceTrustClass.ToString(),
                OfficialRegistrationId = citation.OfficialSourceRegistrationId?.Value,
                SourceSnapshotId = citation.SourceSnapshotId?.Value,
                SourceObservationId = citation.SourceObservationId?.Value,
                SourceContentSha256 = citation.SourceContentObjectId.Value,
                PageStart = citation.PageStart,
                PageEnd = citation.PageEnd,
                RecordStart = citation.RecordStart,
                RecordEnd = citation.RecordEnd,
                ColumnsJson = JsonSerializer.Serialize(citation.Columns),
                SectionLocator = citation.SectionLocator,
                RenderManifestId = citation.RenderManifestId?.Value,
            }));
        context.AnswerEvidencePages.AddRange(record.PageImages.Select(page =>
            new AnswerEvidencePageRow
            {
                AnswerEvidenceRecordId = record.AnswerEvidenceRecordId.Value,
                DocumentId = page.DocumentId.Value,
                DocumentVersion = page.DocumentVersion.Value,
                SourceContentSha256 = page.SourceContentObjectId.Value,
                PageNumber = page.PageNumber,
                RenderManifestId = page.RenderManifestId.Value,
                RenderProfileId = page.RenderProfileId.Value,
                RendererDescriptor = page.RendererDescriptor.Value,
                ImageContentSha256 = page.ImageContentObjectId.Value,
                ImageSha256 = page.ImageSha256.Value,
                ByteLength = page.ByteLength,
                MediaType = page.MediaType,
                WidthPixels = page.WidthPixels,
                HeightPixels = page.HeightPixels,
            }));

        context.AuditEvents.Add(new AuditEventRow
        {
            AuditEventId = $"audit-{Sha256(record.AnswerEvidenceRecordId.Value + "\n" + AuditEventType)}",
            OperationId = record.AnswerEvidenceRecordId.Value,
            CorpusId = record.CorpusId.Value,
            EventType = AuditEventType,
            OccurredAtUtc = ControlPlaneMapping.FormatUtc(record.CreatedAt),
            DetailsDigest = BuildAuditDetailsDigest(
                record.AnswerEvidenceRecordId.Value,
                record.CorrelationId,
                record.CorpusId.Value,
                record.IndexGenerationId.Value,
                record.Citations.Count,
                record.PageImages.Count,
                record.RetentionPolicyId,
                ControlPlaneMapping.FormatUtc(record.CreatedAt),
                ControlPlaneMapping.FormatUtc(record.ExpiresAt)),
        });
    }

    private static async Task ValidateAuthoritiesAsync(
        ControlPlaneDbContext context,
        AnswerEvidenceRecordV1 record,
        CancellationToken cancellationToken)
    {
        var activation = await context.ActivationRecords.AsNoTracking().SingleOrDefaultAsync(row =>
            row.CorpusId == record.CorpusId.Value &&
            row.RecordRevision == record.ActivationRecordRevision.Value,
            cancellationToken).ConfigureAwait(false) ?? throw new InvalidDataException(
                "The answer-evidence record names no exact activation revision.");
        var generation = await context.GenerationManifests.AsNoTracking().SingleOrDefaultAsync(row =>
            row.CorpusId == record.CorpusId.Value &&
            row.IndexGenerationId == record.IndexGenerationId.Value,
            cancellationToken).ConfigureAwait(false) ?? throw new InvalidDataException(
                "The answer-evidence record names no exact final generation.");

        if (activation.IndexGenerationId != record.IndexGenerationId.Value ||
            activation.CatalogueRevision != record.CatalogueRevision.Value ||
            activation.ActivationBindingSetDigest != record.ActivationBindingSetDigest.Value ||
            generation.CatalogueRevision != record.CatalogueRevision.Value ||
            generation.SourceBindingSetDigest != record.SourceBindingSetDigest.Value)
        {
            throw new InvalidDataException(
                "The answer-evidence header differs from activation or generation authority.");
        }

        foreach (var citation in record.Citations)
        {
            var binding = await context.ActivationBindings.AsNoTracking().SingleOrDefaultAsync(row =>
                row.CorpusId == record.CorpusId.Value &&
                row.RecordRevision == record.ActivationRecordRevision.Value &&
                row.DocumentId == citation.DocumentId.Value &&
                row.DocumentVersion == citation.DocumentVersion.Value,
                cancellationToken).ConfigureAwait(false) ?? throw new InvalidDataException(
                    "An answer citation names no exact activation binding.");
            var evidence = await context.ActivationEvidenceBindings.AsNoTracking()
                .SingleOrDefaultAsync(row =>
                    row.CorpusId == record.CorpusId.Value &&
                    row.RecordRevision == record.ActivationRecordRevision.Value &&
                    row.DocumentId == citation.DocumentId.Value &&
                    row.DocumentVersion == citation.DocumentVersion.Value,
                    cancellationToken).ConfigureAwait(false) ?? throw new InvalidDataException(
                        "An answer citation names no exact activation evidence binding.");
            var document = await context.DocumentVersions.AsNoTracking().SingleOrDefaultAsync(row =>
                row.CorpusId == record.CorpusId.Value &&
                row.DocumentId == citation.DocumentId.Value &&
                row.DocumentVersion == citation.DocumentVersion.Value,
                cancellationToken).ConfigureAwait(false) ?? throw new InvalidDataException(
                    "An answer citation names no exact document version.");

            if (binding.ProductId != citation.DatabaseProductId.Value ||
                binding.ProductRevision != citation.DatabaseProductRevision.Value ||
                binding.DocumentFormat != citation.DocumentFormat.ToString() ||
                binding.SourceAdapterId != citation.SourceAdapterId.Value ||
                binding.SourceTrustClass != citation.SourceTrustClass.ToString() ||
                binding.OfficialRegistrationId != citation.OfficialSourceRegistrationId?.Value ||
                binding.OfficialSnapshotId != citation.SourceSnapshotId?.Value ||
                binding.SourceObservationId != citation.SourceObservationId?.Value ||
                evidence.DocumentFormat != citation.DocumentFormat.ToString() ||
                evidence.SourceContentSha256 != citation.SourceContentObjectId.Value ||
                evidence.RenderManifestId != citation.RenderManifestId?.Value ||
                document.ContentLanguage != citation.ContentLanguage.CanonicalTag ||
                document.ContentSha256 != citation.SourceContentObjectId.Value)
            {
                throw new InvalidDataException(
                    "An answer citation differs from its exact source and activation authority.");
            }
        }

        foreach (var page in record.PageImages)
        {
            var manifest = await context.DocumentRenderManifests.AsNoTracking()
                .SingleOrDefaultAsync(row => row.RenderManifestId == page.RenderManifestId.Value,
                    cancellationToken).ConfigureAwait(false) ?? throw new InvalidDataException(
                        "An answer page names no final render manifest.");
            var persistedPage = await context.DocumentPageImages.AsNoTracking()
                .SingleOrDefaultAsync(row =>
                    row.RenderManifestId == page.RenderManifestId.Value &&
                    row.PageNumber == page.PageNumber,
                    cancellationToken).ConfigureAwait(false) ?? throw new InvalidDataException(
                        "An answer page names no exact physical page binding.");

            if (manifest.CorpusId != record.CorpusId.Value ||
                manifest.DocumentId != page.DocumentId.Value ||
                manifest.DocumentVersion != page.DocumentVersion.Value ||
                manifest.SourceContentSha256 != page.SourceContentObjectId.Value ||
                persistedPage.DocumentId != page.DocumentId.Value ||
                persistedPage.DocumentVersion != page.DocumentVersion.Value ||
                persistedPage.SourceContentSha256 != page.SourceContentObjectId.Value ||
                persistedPage.RenderProfileId != page.RenderProfileId.Value ||
                persistedPage.RendererDescriptor != page.RendererDescriptor.Value ||
                persistedPage.ImageContentSha256 != page.ImageContentObjectId.Value ||
                persistedPage.ImageSha256 != page.ImageSha256.Value ||
                persistedPage.ByteLength != page.ByteLength ||
                persistedPage.MediaType != page.MediaType ||
                persistedPage.WidthPixels != page.WidthPixels ||
                persistedPage.HeightPixels != page.HeightPixels)
            {
                throw new InvalidDataException(
                    "An answer page differs from the exact final render-manifest tuple.");
            }
        }
    }

    private static async Task<AnswerEvidenceRecordV1?> ReadWithinAsync(
        ControlPlaneDbContext context,
        AnswerEvidenceRecordId recordId,
        CancellationToken cancellationToken)
    {
        var row = await context.AnswerEvidenceRecords.AsNoTracking().SingleOrDefaultAsync(
            candidate => candidate.AnswerEvidenceRecordId == recordId.Value,
            cancellationToken).ConfigureAwait(false);

        if (row is null)
        {
            return null;
        }

        var citationRows = await context.AnswerEvidenceCitations.AsNoTracking()
            .Where(candidate => candidate.AnswerEvidenceRecordId == recordId.Value)
            .OrderBy(candidate => candidate.Ordinal)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var pageRows = await context.AnswerEvidencePages.AsNoTracking()
            .Where(candidate => candidate.AnswerEvidenceRecordId == recordId.Value)
            .OrderBy(candidate => candidate.DocumentId)
            .ThenBy(candidate => candidate.DocumentVersion)
            .ThenBy(candidate => candidate.PageNumber)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var operation = await context.AdminOperations.AsNoTracking().SingleOrDefaultAsync(
            candidate => candidate.OperationId == recordId.Value,
            cancellationToken).ConfigureAwait(false);
        var auditId = $"audit-{Sha256(recordId.Value + "\n" + AuditEventType)}";
        var audit = await context.AuditEvents.AsNoTracking().SingleOrDefaultAsync(
            candidate => candidate.AuditEventId == auditId,
            cancellationToken).ConfigureAwait(false);

        if (row.SchemaVersion != AnswerEvidenceRecordV1.CurrentSchemaVersion ||
            row.Outcome != "Answered" || row.QuestionLanguage != row.AnswerLanguage ||
            row.RetentionPolicyId != AnswerEvidenceRecordV1.RetentionPolicy ||
            operation is null || operation.OperationKind != OperationKind ||
            operation.Status != "Applied" || operation.CorpusId != row.CorpusId ||
            operation.ExpectedRevision is not null || operation.ResultRevision is not null ||
            operation.RequestedAtUtc != row.CreatedAtUtc ||
            operation.CompletedAtUtc != row.CreatedAtUtc ||
            audit is null || audit.OperationId != recordId.Value ||
            audit.CorpusId != row.CorpusId || audit.EventType != AuditEventType ||
            audit.OccurredAtUtc != row.CreatedAtUtc ||
            audit.DetailsDigest != BuildAuditDetailsDigest(
                row.AnswerEvidenceRecordId,
                row.CorrelationId,
                row.CorpusId,
                row.IndexGenerationId,
                citationRows.Length,
                pageRows.Length,
                row.RetentionPolicyId,
                row.CreatedAtUtc,
                row.ExpiresAtUtc))
        {
            throw new InvalidDataException(
                "A persisted answer-evidence header is outside schema version 1.");
        }

        var citations = citationRows.Select(citation =>
            new AnswerEvidenceCitationBindingV1(
                citation.Ordinal,
                new DatabaseProductId(citation.ProductId),
                new DatabaseProductRevision(citation.ProductRevision),
                new DocumentId(citation.DocumentId),
                new DocumentVersionNumber(citation.DocumentVersion),
                ParseDocumentFormat(citation.DocumentFormat),
                new DocumentContentLanguage(citation.ContentLanguage),
                citation.ChunkId,
                new SourceAdapterId(citation.SourceAdapterId),
                ParseTrustClass(citation.SourceTrustClass),
                citation.OfficialRegistrationId is null
                    ? null
                    : new OfficialSourceRegistrationId(citation.OfficialRegistrationId),
                citation.SourceSnapshotId is null
                    ? null
                    : new OfficialSnapshotId(citation.SourceSnapshotId),
                citation.SourceObservationId is null
                    ? null
                    : new OfficialObservationId(citation.SourceObservationId),
                new ContentObjectId(citation.SourceContentSha256),
                citation.PageStart,
                citation.PageEnd,
                citation.RecordStart,
                citation.RecordEnd,
                JsonSerializer.Deserialize<string[]>(citation.ColumnsJson) ??
                    throw new InvalidDataException("A persisted citation has invalid columns."),
                citation.SectionLocator,
                citation.RenderManifestId is null
                    ? null
                    : new RenderManifestId(citation.RenderManifestId))).ToArray();
        var pages = pageRows.Select(page => new AnswerEvidencePageBindingV1(
            new DocumentId(page.DocumentId),
            new DocumentVersionNumber(page.DocumentVersion),
            new ContentObjectId(page.SourceContentSha256),
            page.PageNumber,
            new RenderManifestId(page.RenderManifestId),
            new RenderProfileId(page.RenderProfileId),
            new RendererDescriptor(page.RendererDescriptor),
            new ContentObjectId(page.ImageContentSha256),
            new ImageSha256(page.ImageSha256),
            page.ByteLength,
            page.MediaType,
            page.WidthPixels,
            page.HeightPixels)).ToArray();

        return AnswerEvidenceRecordV1.Rehydrate(
            new AnswerEvidenceRecordSha256(row.RecordSha256),
            recordId,
            new CorpusId(row.CorpusId),
            new ActivationRecordRevision(row.ActivationRecordRevision),
            new CatalogueRevision(row.CatalogueRevision),
            new SourceBindingSetDigest(row.SourceBindingSetDigest),
            new ActivationBindingSetDigest(row.ActivationBindingSetDigest),
            new IndexGenerationId(row.IndexGenerationId),
            ParseLanguage(row.QuestionLanguage),
            new AnswerSha256(row.AnswerSha256),
            row.AnswerUtf8ByteLength,
            new EvidenceCoverageDigest(row.EvidenceCoverageDigest),
            row.RetrievalPolicyVersion,
            row.PromptVersion,
            new AnswerLanguageModelDescriptorV1(
                row.LanguageModelProviderId,
                row.LanguageModelId,
                row.LanguageModelRevision),
            row.CorrelationId,
            ControlPlaneMapping.ParseUtc(row.CreatedAtUtc),
            ControlPlaneMapping.ParseUtc(row.ExpiresAtUtc),
            citations,
            pages);
    }

    private static SupportedQueryLanguage ParseLanguage(string value) => value switch
    {
        "pt-BR" => SupportedQueryLanguage.PtBr,
        "en-GB" => SupportedQueryLanguage.EnGb,
        _ => throw new InvalidDataException("A persisted answer language is invalid."),
    };

    private static DocumentFormat ParseDocumentFormat(string value) => value switch
    {
        "Pdf" => DocumentFormat.Pdf,
        "Csv" => DocumentFormat.Csv,
        _ => throw new InvalidDataException("A persisted citation format is invalid."),
    };

    private static SourceTrustClass ParseTrustClass(string value) => value switch
    {
        "LocalAuthorised" => SourceTrustClass.LocalAuthorised,
        "OfficialExternal" => SourceTrustClass.OfficialExternal,
        _ => throw new InvalidDataException("A persisted citation trust class is invalid."),
    };

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

    private static string Sha256(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private static string BuildAuditDetailsDigest(
        string recordId,
        string correlationId,
        string corpusId,
        string generationId,
        int citationCount,
        int pageCount,
        string retentionPolicyId,
        string createdAtUtc,
        string expiresAtUtc) =>
        Sha256(string.Join(
            '\n',
            recordId,
            correlationId,
            corpusId,
            generationId,
            citationCount.ToString(CultureInfo.InvariantCulture),
            pageCount.ToString(CultureInfo.InvariantCulture),
            retentionPolicyId,
            createdAtUtc,
            expiresAtUtc));
}
